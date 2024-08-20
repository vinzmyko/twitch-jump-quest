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
    private int totalPlayers = 1;
    [Export(PropertyHint.Range, "1,100,1")]
    private int DEBUGPLAYERCOUNT = 1;
    
    private Camera2D camera;
    private Path2D path;
    private PathFollow2D pathFollow;
    private Area2D upwardTrigger;
    private Area2D killZone;

    private float currentCameraSpeed = 0;
    private List<Player> playersInTriggerArea = new List<Player>();
    private int currentPathPointIndex = 0;
    private enum CameraState { Idle, Moving }
    private CameraState currentState = CameraState.Idle;
    private GameManager gameManager;

    public override void _Ready()
    {
        base._Ready();
        camera = GetNode<Camera2D>("Camera2D");
        path = GetNode<Path2D>("Path2D");
        pathFollow = GetNode<PathFollow2D>("Path2D/PathFollow2D");
        upwardTrigger = GetNode<Area2D>("UpwardTriggerArea2D");
        killZone = GetNode<Area2D>("KillzoneTrigger");

        gameManager = GetNode<GameManager>("/root/GameManager");

        upwardTrigger.BodyEntered += OnPlayerEnterUpwardArea2D;
        upwardTrigger.BodyExited += OnPlayerExitUpwardArea2D;
        killZone.BodyEntered += OnPlayerEnterKillZone;
        var root = GetTree().Root;
        var levelNode = root.GetChild(root.GetChildCount() - 1);
        levelNode.GetNode<GameTimer>("CanvasLayer/GameTimer").waitTimeFinished += () => { upwardTrigger.GetNode<CollisionShape2D>("CollisionShape2D").Disabled = false; };
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        UpdateCameraMovement(delta);
    }

    private void OnPlayerEnterUpwardArea2D(Node2D body)
    {
        totalPlayers = Mathf.Max(1, gameManager.players.Count);
        if (body is Player player && !playersInTriggerArea.Contains(player))
        {
            if (player.IsOnFloor())
            {
                GD.Print("checking threshold");
                playersInTriggerArea.Add(player);
                // CheckPlayerThreshold();
            }
            else
            {
                // Optional: Start a coroutine to check if the player lands soon
                GD.Print("Start ground check routine");
                StartGroundedCheckCoroutine(player);
            }
        }
    }

    private void OnPlayerExitUpwardArea2D(Node2D body)
    {
        if (body is Player player)
        {
            playersInTriggerArea.Remove(player);
        }
    }

    private void OnPlayerEnterKillZone(Node2D body)
    {
        if (body is Player player)
        {
            EmitSignal(SignalName.PlayerHitKillZone, player);
        }
    }

    private void CheckPlayerThreshold()
    {
        if (currentState == CameraState.Idle)
        {
            currentState = CameraState.Moving;
            currentPathPointIndex++;
        }
    }

private void UpdateCameraMovement(double delta)
    {
        if (currentState == CameraState.Moving)
        {
            float targetSpeed = CalculateTargetSpeed();
            currentCameraSpeed = Mathf.MoveToward(currentCameraSpeed, targetSpeed * speedMultiplier, cameraAcceleration * (float)delta);

            Vector2 targetPointPosition = ToGlobal(path.Curve.GetPointPosition(currentPathPointIndex));
            GD.Print($"targetPos: {targetPointPosition}, PointIdx[{currentPathPointIndex}]: {path.Curve.GetPointPosition(currentPathPointIndex)}");
            MoveCameraWithArea2Ds(currentCameraSpeed, false);

            if (camera.GlobalPosition.Y <= targetPointPosition.Y)
            {
                upwardTrigger.GlobalPosition = camera.GlobalPosition;
                currentState = CameraState.Idle;
            }

            // GD.Print($"Players: {playersInTriggerArea.Count}/{totalPlayers}, Speed: {currentCameraSpeed:F2}");
        }
    }

    private float CalculateTargetSpeed()
    {
        int playersInTrigger = playersInTriggerArea.Count;
        // int playersInTrigger = DEBUGPLAYERCOUNT;
        float playerRatio = (float)playersInTrigger / totalPlayers;
        
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
                playersInTriggerArea.Add(player);
                CheckPlayerThreshold();
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
        }
        else
        {
            camera.GlobalPosition = new Vector2(camera.GlobalPosition.X, camera.GlobalPosition.Y - cameraSpeed);
            killZone.GlobalPosition = new Vector2(killZone.GlobalPosition.X, killZone.GlobalPosition.Y - cameraSpeed);
        }
    }
}