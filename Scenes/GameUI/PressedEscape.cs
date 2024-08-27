using Godot;

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
            ToggleEscapeScreen(); // Unpause the game
            gameManager.StartNewGame();
            SceneManager.Instance.ChangeScene("MainMenu");
        };
        quitAfterRound.Pressed += () =>
        {
            gameManager.quitNextRound = true;
            ToggleEscapeScreen();
        };
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("Escape"))
        {
            ToggleEscapeScreen();
            GetViewport().SetInputAsHandled(); 
        }
    }

    public void ToggleEscapeScreen()
    {
        Visible = !Visible;
        GetTree().Paused = Visible;
    }
}