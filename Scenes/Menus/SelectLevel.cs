using Godot;

public partial class SelectLevel : Control
{
    [Export]
    private Button toMenu;
    [Export]
    private Button fiveMinutes;
    [Export]
    private Button tenMinutes;
    [Export]
    private LineEdit customTime;
    [Export]
    private CheckButton EasyModeToggleButton;
    [Export]
    private CheckButton autoPlayToggleButton;
    private GameManager gameManager;

    public override void _Ready()
    {
        base._Ready();
        gameManager = GetNode<GameManager>("/root/GameManager");

        toMenu.ButtonDown += () => {SceneManager.Instance.ChangeScene("MainMenu");};
        fiveMinutes.ButtonDown += () => {gameManager.gameTime = 300;SceneManager.Instance.ChangeScene("LevelOne");};
        tenMinutes.ButtonDown += () => {gameManager.gameTime = 600;SceneManager.Instance.ChangeScene("LevelOne");};
        customTime.TextSubmitted += (string newText) => 
        {
            if (int.TryParse(newText, out int customTime) && customTime > 0)
            {
                gameManager.gameTime = customTime;
                SceneManager.Instance.ChangeScene("LevelOne");
            }
        };
        EasyModeToggleButton.Toggled += (bool toggledOn) => {gameManager.easyMode = toggledOn;};
        autoPlayToggleButton.Toggled += (bool toggledOn) => {gameManager.autoPlay = toggledOn;};

        EasyModeToggleButton.ButtonPressed = gameManager.easyMode;
        autoPlayToggleButton.ButtonPressed = gameManager.autoPlay;
    }
}
