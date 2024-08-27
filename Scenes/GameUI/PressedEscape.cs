using Godot;
using System;

public partial class PressedEscape : Control
{
    [Export]
    private Button mainMenu;
    [Export]
    private Button quitAfterRound;

    public override void _Ready()
    {
        base._Ready();

        GameManager gameManager = GetNode<GameManager>("/root/GameManager");
        
        mainMenu.Pressed += () =>
        {
            gameManager.StartNewGame();
            SceneManager.Instance.ChangeScene("MainMenu");
        };
        quitAfterRound.Pressed += () =>
        {
            gameManager.quitNextRound = true;
            ToggleEscapeScreen();
        };
    }

    public void ToggleEscapeScreen()
    {
        Visible = !Visible;
        GetTree().Paused = Visible;
    }
}
