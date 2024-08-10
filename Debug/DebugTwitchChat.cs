using Godot;
using System;

public partial class DebugTwitchChat : Control
{
    [Signal]
    public delegate void DebuggerDeleteSelfEventHandler();

    Button minimiseButton;
    Label previousText;
    LineEdit textBox;
    ScrollContainer scrollContainer;
    HBoxContainer hBoxContainer;

    string previousTextString = string.Empty;
    private StringRingBuffer commandBuffer;

    private bool isDragging = false;
    private Vector2 dragStart = Vector2.Zero;

    public override void _Ready()
    {
        base._Ready();

        minimiseButton = GetNode<Button>("MarginContainer/VBoxContainer/HBoxContainer/MinimiseButton");
        previousText = GetNode<Label>("MarginContainer/VBoxContainer/ScrollContainer/MarginContainer/VBoxContainer/PreviousMessagesLabel");
        textBox = GetNode<LineEdit>("MarginContainer/VBoxContainer/LineEdit");
        scrollContainer = GetNode<ScrollContainer>("MarginContainer/VBoxContainer/ScrollContainer");

        textBox.TextSubmitted += OnTextSubmitted;
        minimiseButton.Pressed += OnButtonPressed;

        commandBuffer = new StringRingBuffer(5);

        textBox.FocusMode = Control.FocusModeEnum.All;

        // Handle dragging
        hBoxContainer = GetNode<HBoxContainer>("MarginContainer/VBoxContainer/HBoxContainer");
        hBoxContainer.MouseEntered += () => Input.SetDefaultCursorShape(Input.CursorShape.Move);
        hBoxContainer.MouseExited += () => Input.SetDefaultCursorShape(Input.CursorShape.Arrow);
    }

    private void OnButtonPressed()
    {
        // Minimise previous texts and line edit so it's only shows HBoxContainer
        scrollContainer.Visible = !scrollContainer.Visible;
        textBox.Visible = !textBox.Visible;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        base._UnhandledInput(@event);
        if (@event is InputEventKey keyEvent && textBox.HasFocus())
        {
            HandleArrowMovement(keyEvent);
        }
    }

    private void OnTextSubmitted(string newText)
    {
        if (newText == "clear")
        {
            previousTextString = string.Empty;
            previousText.Text = previousTextString;
        }

        string[] messageInfo = new string[3];
        messageInfo[0] = "DEBUG";
        messageInfo[1] = "DEBUG";


        if (newText == "delete")
        {
            previousTextString += "delete\n";
            previousText.Text = previousTextString;
            EmitDeletePlayer();
        }

        if (newText == "join")
        {
            previousTextString += "join\n";
            previousText.Text = previousTextString;
            GameManager.Instance.HandleJoinRequest(messageInfo);
        }

        var (isValid, angle, power) = MessageParser.ParseMessage(newText);
        if (isValid)
        {
            commandBuffer.Add(newText);
            previousTextString += commandBuffer._buffer[commandBuffer._count - 1] + "\n";
            previousText.Text = previousTextString;
            messageInfo[2] = newText;
            DebuggerCommandsEmited(angle, power);

        }
        CallDeferred(nameof(ScrollContainerToBottom));

        textBox.Text = string.Empty;

        // Send this to the player in which has the init of debug
    }

    private void EmitDeletePlayer()
    {
        EmitSignal(SignalName.DebuggerDeleteSelf);
    }


    private void DebuggerCommandsEmited(float angle, float power)
    {
        Player DebugPlayer = (Player)GetTree().GetFirstNodeInGroup("DebugPlayer");
        DebugPlayer.DoJumpPhysics(angle, power);
    }


    private void ScrollContainerToBottom()
    {
        var timer = GetTree().CreateTimer(0.01); 
        timer.Timeout += () =>
        {
            var vScrollBar = scrollContainer.GetVScrollBar();
            if (vScrollBar != null)
            {
                scrollContainer.ScrollVertical = (int)vScrollBar.MaxValue;
            }
        };
    }

    private void HandleArrowMovement(InputEventKey keyEvent)
    {
        int currentPos = commandBuffer._count;

        if (keyEvent.Keycode == Key.Up)
        {
            textBox.Text = commandBuffer.GetIndexAbove();
        }

        if (keyEvent.Keycode == Key.Down)
        {
            textBox.Text = commandBuffer.GetIndexBelow();
        }
    }

    private void StartDragging()
    {
        isDragging = true;
        dragStart = GetGlobalMousePosition() - GlobalPosition;
    }

    private void StopDragging()
    {
        isDragging = false;
    }

    private void HandleDragging()
    {
        if (isDragging)
        {
            GlobalPosition = GetGlobalMousePosition() - dragStart;
        }
    }
    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseEvent)
        {
            if (mouseEvent.ButtonIndex == MouseButton.Left)
            {
                if (mouseEvent.Pressed)
                {
                    if (hBoxContainer.GetGlobalRect().HasPoint(GetGlobalMousePosition()))
                    {
                        StartDragging();
                    }
                }
                else
                {
                    StopDragging();
                }
            }
        }
        else if (@event is InputEventMouseMotion)
        {
            HandleDragging();
            ClampToScreen();
        }
    }
    private void ClampToScreen()
    {
        Vector2 screenSize = GetViewportRect().Size;
        Vector2 windowSize = GetGlobalRect().Size; // Use this instead of Size
        
        GlobalPosition = new Vector2(
            Mathf.Clamp(GlobalPosition.X, 0, ( screenSize.X - windowSize.X ) - 160 ),
            Mathf.Clamp(GlobalPosition.Y, 0, screenSize.Y - windowSize.Y - 64)
        );
    }

}
