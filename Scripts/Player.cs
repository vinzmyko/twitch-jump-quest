using System.Threading.Tasks;
using Godot;
using UNLTeamJumpQuest.TwitchIntegration;

public partial class Player : CharacterBody2D
{
	AnimatedSprite2D animatedSprite;
    Label displayLabel;

    [Export]
    public float BaseJumpVelocity = 500.0f;

    [Export]
    public float Gravity = 980.0f; // You can adjust this value
    [Export]
    public float distanceForHeadOnFloor = 150;
    public bool headOnFloor = false;

    private float currentYPos = 0;
    private float previousYPos = 0;

    private Vector2 _jumpVelocity = Vector2.Zero;

    public string userID;
    public string displayName;

    private DebugTwitchChat debugger;

    int points;

    public void Initialize(string _displayName, string _userID)
    {
        displayName = _displayName;
        userID = _userID;
        points = 0;
    }
    public override async void _Ready()
    {
        base._Ready();

        debugger = GetNodeOrNull<DebugTwitchChat>("/root/Main/CanvasLayer/DebugTwitchChat");

        // Then it's debugging mode
        // Now send data from twitchchat.cs to player.cs
        if (debugger != null)
        {
            debugger.DebuggerDeleteSelf += OnDeleteSelf;
        }

		animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        displayLabel = GetNode<Label>("DisplayNameLabel");
        displayLabel.Text = displayName;

        TwitchBot.Instance.MessageReceived += OnMessageReceived;
        animatedSprite.AnimationFinished += OnHeadOnFloorAnimationFinished;

        if (userID == "DEBUG")
        {
            AddToGroup("DebugPlayer");
        }
        else
            AddToGroup("Player");
        
        GD.Print(IsInGroup("DebugPlayer"));

        // Some reason it won't change in Player scene so I do it through code.
        SetCollisionLayerValue(1, false);
        await showDisplayName(3.5);
    }

    private void OnHeadOnFloorAnimationFinished()
    {
        if (animatedSprite.Animation == "HeadOnFloor")
        {
            currentYPos = 0;
            previousYPos = 0;
            headOnFloor = false;
        }
    }

    private void OnDeleteSelf()
    {
        if (userID == "DEBUG")
        {
            GameManager.Instance.DeletePlayer("DEBUG");
        }
        
    }

    private void OnDebuggerCommandsGiven(float angle, float power)
    {
        GD.Print(Name);
        GD.Print($"{displayName} has {angle} degrees and {power} power, jumpvelocity {BaseJumpVelocity}");
        if (userID != "DEBUG")
            return;
        DoJumpPhysics(angle, power);
    }

    public void DoJumpPhysics(float angle, float power)
    {
        // Only jumps when not head on floor
        if (headOnFloor)
            return;

        _jumpVelocity = MyMaths.SetJumpVector(angle, power, BaseJumpVelocity);
        
        if (_jumpVelocity.X < 0)
            animatedSprite.FlipH = true;
        else
            animatedSprite.FlipH = false;
    }

    private void OnMessageReceived(string[] messageInfo)
    {
        if (!IsOnFloor()) return; // Only process jump commands when on the floor

        // If not sent by the Player's user ignore it/return.
        if (messageInfo[1] != userID)
            return;

        var (isValid, angle, power) = MessageParser.ParseMessage(messageInfo[2]);
        if (isValid)
        {
            DoJumpPhysics(angle, power);
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        Vector2 velocity = Velocity;

        if (!IsOnFloor())
        {
            velocity.Y += Gravity * (float)delta;
            animatedSprite.Play("Jump");
        }
        if (IsOnFloor())
        {
            if (previousYPos != 0)
            {
                if (currentYPos > previousYPos)
                {
                    float heightDifference = currentYPos - previousYPos;
                    if (heightDifference > distanceForHeadOnFloor)
                    {
                        headOnFloor = true;
                    }
                }
            }
            currentYPos = GlobalPosition.Y;

            if (headOnFloor)
                animatedSprite.Play("HeadOnFloor");
            else
                animatedSprite.Play("Idle");

            // Apply jump velocity if there's a pending jump
            if (_jumpVelocity != Vector2.Zero)
            {
                previousYPos = currentYPos;
                // _ = stop warning error :)
                _ = showDisplayName(2.0);
                velocity = _jumpVelocity;
                _jumpVelocity = Vector2.Zero; // Reset jump velocity after applying
            }
            else
            {
                velocity.X = 0; // Stop horizontal movement when on floor and not jumping
            }
        }

        // Move the character
        Velocity = velocity;
        MoveAndSlide();
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        TwitchBot.Instance.MessageReceived -= OnMessageReceived;
    }

    public string GetUserID()
    {
        return userID;
    }

    private async Task showDisplayName(double time)
    {
        displayLabel.Visible = true;
        await ToSignal(GetTree().CreateTimer(time), "timeout");
        displayLabel.Visible = false;
    }

    public void SetPlayerColours(Color cape1Color, Color cape2Color, Color helmetFeathersColor, 
        Color armourDarkColor, Color armourMedColor, Color armourLightColor)
    {
        ShaderMaterial material = (ShaderMaterial)animatedSprite.Material;
        material.SetShaderParameter("cape1_color_new", cape1Color); // 0a7030
        material.SetShaderParameter("cape2_color_new", cape2Color); // eba724
        material.SetShaderParameter("helmet_feathers_new", helmetFeathersColor); // d2202c
        material.SetShaderParameter("armour_dark_new", armourDarkColor); // 7c776f
        material.SetShaderParameter("armour_med_new", armourMedColor); // b3aaa1
        material.SetShaderParameter("armour_light_new", armourLightColor); // eadfd1
    }
}
