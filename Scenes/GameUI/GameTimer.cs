using System;
using Godot;

public partial class GameTimer : Node
{
    [Signal]
    public delegate void waitTimeFinishedEventHandler();
    [Signal]
    public delegate void gameTimeFinishedEventHandler();
    public Label gameTimeLabel;
    private Label joinCTALabel;
    private GameManager gameManager;
    private float waitTime, gameTime;
    private bool waitTimeFinishedEmitted = false, gameTimeFinishedEmitted  = false;

    public override void _Ready()
    {
        base._Ready();
        gameTimeLabel = GetNode<Label>("GameTimeLabel");
        joinCTALabel = GetNode<Label>("JoinTeamCallToActionLabel");

        gameManager = GetNode<GameManager>("/root/GameManager");

        // waitTime = gameManager.waitTime;
        waitTime = 40.0f;
        gameTime = gameManager.gameTime;
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        if (gameManager.CurrentGameState == GameManager.GameState.WaitingForPlayers)
        {
            waitTime -= (float)delta;
            SetTimeLabel(FormatTime(waitTime));

            if (waitTime <= 0 && !waitTimeFinishedEmitted)
            {
                EmitSignal(SignalName.waitTimeFinished);
                waitTimeFinishedEmitted = true;
                SetCTALabelInvisible();
            }
        }
        else if (gameManager.CurrentGameState == GameManager.GameState.Playing)
        {
            gameTime -= (float)delta;
            SetTimeLabel(FormatTime(gameTime));

            if (gameTime <= 0 && !gameTimeFinishedEmitted)
            {
                EmitSignal(SignalName.gameTimeFinished);
                gameTimeFinishedEmitted = true;
            }
        }
    }

    public void SetTimeLabel(string text)
    {
        gameTimeLabel.Text = text;
    }

    public void SetCTALabelInvisible()
    {
        joinCTALabel.Visible = false;
    }

    public void ActivateTimer(ref float time, double delta)
    {
        float remainingTime = time -= (float)delta;
        SetTimeLabel(FormatTime(remainingTime));
    }

    private string FormatTime(float remainingTime)
    {
        TimeSpan time = TimeSpan.FromSeconds(Math.Max(0, remainingTime));
        return $"{(int)time.TotalMinutes:D2}:{(int)time.Seconds:D2}";
    }
}