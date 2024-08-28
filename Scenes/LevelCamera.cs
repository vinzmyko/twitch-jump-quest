using System.Collections.Generic;
using Godot;

public partial class LevelCamera : Node2D
{
    [Signal]
    public delegate void PlayerHitKillZoneEventHandler(Player player);
    [Export(PropertyHint.Range, "1.0,10.0,0.1")]
    private float cameraAcceleration = 10.0f;
    [Export(PropertyHint.Range, "0.1,2.0,0.1")]
    private float minSpeed = 0.6f;
    
    [Export(PropertyHint.Range, "2.0,10.0,0.1")]
    private float maxSpeed = 5.0f;
    
    [Export(PropertyHint.Range, "0.1,1.5,0.05")]
    private float speedMultiplier = 0.35f;
    [Export(PropertyHint.Range, "1.0,10.0,0.1")]
    private float speedCurve = 5f;

    [Export(PropertyHint.Range, "1,100,1")]
    private int DEBUGPLAYERCOUNT = 1;
    
    private Camera2D camera;
    private Path2D path;
    private PathFollow2D pathFollow;
    private Area2D upwardTrigger;
    private Area2D killZone;
    private StaticBody2D preventFromPassing;

    private float currentCameraSpeed = 0;
    private List<Player> playersInTriggerArea = new List<Player>();
    private int currentPathPointIndex = 0;
    private enum CameraState { Idle, Moving }
    private CameraState currentState = CameraState.Idle;
    private GameManager gameManager;
    private EasyModeComponent easyModeComponent;
    private GameTimer gameTimer;

    public override void _Ready()
    {
        base._Ready();
        camera = GetNode<Camera2D>("Camera2D");
        path = GetNode<Path2D>("Path2D");
        pathFollow = GetNode<PathFollow2D>("Path2D/PathFollow2D");
        upwardTrigger = GetNode<Area2D>("UpwardTriggerArea2D");
        killZone = GetNode<Area2D>("KillzoneTrigger");
        preventFromPassing = GetNode<StaticBody2D>("StaticBody2D");

        gameManager = GetNode<GameManager>("/root/GameManager");
        gameManager.PlayerDied += OnPlayerDied;

        upwardTrigger.BodyEntered += OnPlayerEnterUpwardArea2D;
        upwardTrigger.BodyExited += OnPlayerExitUpwardArea2D;
        killZone.BodyEntered += OnPlayerEnterKillZone;
        var root = GetTree().Root;
        var levelNode = root.GetChild(root.GetChildCount() - 1);
        gameTimer = levelNode.GetNode<GameTimer>("CanvasLayer/GameTimer");
        gameTimer.waitTimeFinished += () => 
        { 
            upwardTrigger.GetNode<CollisionShape2D>("CollisionShape2D").Disabled = false;
            if (gameManager.easyMode)
            {
                gameTimer.EasyModeActivated();
                gameManager.UpdateTotalPlayers();
                UpdateEasyModeRequiredPlayersTextLabel();
                GD.Print($"total players: {gameManager.UpdateTotalPlayers}");
            }
        };

        if (gameManager.easyMode)
        {
            easyModeComponent = GetNode<Node>("Components").GetNode<EasyModeComponent>("EasyModeComponent");

            if (easyModeComponent == null)
                GD.Print("EasyModeComponent not found, while easyMode = true.");
            
            easyModeComponent.ToggleEasyMode(true);
        }

    }

    private async void OnPlayerDied(string displayName, string userID, string teamAbbrev)
    {
        await ToSignal(GetTree().CreateTimer(0.25f), "timeout");
        UpdateEasyModeRequiredPlayersTextLabel();
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        UpdateCameraMovement(delta);
    }

    private void OnPlayerEnterUpwardArea2D(Node2D body)
    {
        if (body is Player player && !playersInTriggerArea.Contains(player))
        {
            // if (player.IsOnFloor())
            // {
            //     playersInTriggerArea.Add(player);
            // }
            // else
            {
                GD.Print("Started coroutine");
                StartGroundedCheckCoroutine(player);
            }
        }
    }

    private void UpdateEasyModeRequiredPlayersTextLabel()
    {
        if (gameManager.easyMode && easyModeComponent != null)
        {
            int threshold = easyModeComponent.CalculatePlayerThreshold(gameManager.UpdateTotalPlayers(), currentPathPointIndex);
            gameTimer.ChangeRequiredPlayersLabelText($"{playersInTriggerArea.Count} / {threshold}");
            // GD.Print($"Updated label: {playersInTriggerArea.Count} / {threshold} (Total players: {totalPlayers})");
        }
    }

    public void CheckPlayerInTriggerArea()
    {
        if (upwardTrigger.OverlapsBody(this))
        {
            OnPlayerEnterUpwardArea2D(this);
        }
    }

    private void OnPlayerExitUpwardArea2D(Node2D body)
    {
        if (body is Player player)
        {
            playersInTriggerArea.Remove(player);
            UpdateEasyModeRequiredPlayersTextLabel();
        }
    }

    private void OnPlayerEnterKillZone(Node2D body)
    {
        if (body is Player player)
        {
            GD.Print($"Player {player.Name} entered kill zone");
            player.Die();
            EmitSignal(SignalName.PlayerHitKillZone, player);
            GD.Print($"Player {player.Name} killed");
        }
    }

    private void EmitPlayerHitKillzone(Player player)
    {
        // add a timer to see if player has been in the zone for a certain amount of time
        var timer = GetTree().CreateTimer(0.5); 
        timer.Timeout += () =>
        {
            EmitSignal(SignalName.PlayerHitKillZone, player);
        };
    }

   private void CheckPlayerThreshold()
    {
        if (currentState == CameraState.Idle)
        {
            if(easyModeComponent == null)
            {
                StartCameraMovement();
                GD.Print("Moving camera when easycompoent is null");
                return;
            }

            // If the amount of playersInTriggerArea is greater then the threshold then start cam movement

            if (easyModeComponent.ShouldTriggerCameraMovement(playersInTriggerArea.Count, currentPathPointIndex,
            gameManager.UpdateTotalPlayers()))
            {
                StartCameraMovement();
            }
        }
    }

    private void StartCameraMovement()
    {
        currentState = CameraState.Moving;
        currentPathPointIndex++;
    }

    private void UpdateCameraMovement(double delta)
    {
        if (currentState == CameraState.Moving)
        {
            float targetSpeed = CalculateTargetSpeed();
            currentCameraSpeed = Mathf.MoveToward(currentCameraSpeed, targetSpeed * speedMultiplier, cameraAcceleration * (float)delta);

            Vector2 targetPointPosition = ToGlobal(path.Curve.GetPointPosition(currentPathPointIndex));
            MoveCameraWithArea2Ds(currentCameraSpeed, false);

            if (camera.GlobalPosition.Y <= targetPointPosition.Y)
            {
                upwardTrigger.GlobalPosition = camera.GlobalPosition;
                currentState = CameraState.Idle;
                
                // Clear the players in trigger area list and update the label
                playersInTriggerArea.Clear();
                UpdateEasyModeRequiredPlayersTextLabel();
                
                // Check for players in the new trigger area position
                GetTree().CallGroup("players", nameof(CheckPlayerInTriggerArea));
            }
        }
    }

    private float CalculateTargetSpeed()
    {
        int playersInTrigger = playersInTriggerArea.Count;
        int totalPlayers = gameManager.UpdateTotalPlayers();
        
        float playerRatio = (float)playersInTrigger / totalPlayers;
        
        float minPlayerRatio = 0.3f;
        float maxPlayerRatio = 0.95f;
        playerRatio = Mathf.Clamp(playerRatio, minPlayerRatio, maxPlayerRatio);
        
        float speedFactor = 1 - Mathf.Exp(-speedCurve * (1 - playerRatio));
        
        return Mathf.Lerp(maxSpeed, minSpeed, speedFactor);
    }

    private async void StartGroundedCheckCoroutine(Player player)
    {
        const float maxWaitTime = 1.0f; // Maximum time to wait for grounding
        float elapsedTime = 0f;

        while (elapsedTime < maxWaitTime)
        {
            if (player.IsOnFloor() && upwardTrigger.OverlapsBody(player))
            {
                GD.Print("Player in floor inside coroutine");
                playersInTriggerArea.Add(player);
                CheckPlayerThreshold();
                UpdateEasyModeRequiredPlayersTextLabel();
                return;
            }

            await ToSignal(GetTree(), "physics_frame");
            elapsedTime += (float)GetPhysicsProcessDeltaTime();
        }
    }

    private void MoveCameraWithArea2Ds(float cameraSpeed, bool includeUpwardTrigger = true)
    {
        if (includeUpwardTrigger)
        {
            camera.GlobalPosition = new Vector2(camera.GlobalPosition.X, camera.GlobalPosition.Y - cameraSpeed);
            upwardTrigger.GlobalPosition = new Vector2(upwardTrigger.GlobalPosition.X, upwardTrigger.GlobalPosition.Y - cameraSpeed);
            killZone.GlobalPosition = new Vector2(killZone.GlobalPosition.X, killZone.GlobalPosition.Y - cameraSpeed);
            preventFromPassing.GlobalPosition = new Vector2(preventFromPassing.GlobalPosition.X, preventFromPassing.GlobalPosition.Y - cameraSpeed);
        }
        else
        {
            camera.GlobalPosition = new Vector2(camera.GlobalPosition.X, camera.GlobalPosition.Y - cameraSpeed);
            killZone.GlobalPosition = new Vector2(killZone.GlobalPosition.X, killZone.GlobalPosition.Y - cameraSpeed);
            preventFromPassing.GlobalPosition = new Vector2(preventFromPassing.GlobalPosition.X, preventFromPassing.GlobalPosition.Y - cameraSpeed);
        }
    }
}