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
    public bool waitTimeFinishedEmitted = false, gameTimeFinishedEmitted  = false;
    [Export]
    private Label requiredPlayersLabel;

    public override void _Ready()
    {
        base._Ready();
        gameTimeLabel = GetNode<Label>("GameTimeLabel");
        joinCTALabel = GetNode<Label>("JoinTeamCallToActionLabel");

        gameManager = GetNode<GameManager>("/root/GameManager");

        // waitTime = gameManager.waitTime;
        waitTime = 15f;
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
                EasyModeActivated();
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

    public void EasyModeActivated()
    {
        if (gameManager.easyMode)
        {
            requiredPlayersLabel.Visible = true;
        }
    }

    public void ChangeRequiredPlayersLabelText(string text)
    {
        Label requiredPlayersLabel = GetNode<Label>("RequiredPlayers");
        requiredPlayersLabel.Text = text;
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