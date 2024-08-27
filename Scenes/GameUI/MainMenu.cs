using Godot;

public partial class EscapeButton : Button
{
    [Export]
    private Button mainMenu;
    [Export]
    private Button quitAfterRound;

    public override void _Ready()
    {
        base._Ready();
        GameManager gameManager = GetNode<GameManager>("/root/GameManager");
        mainMenu.Pressed += () => {SceneManager.Instance.ChangeScene("MainMenu");};
        quitAfterRound.Pressed += () => {gameManager.quitNextRound = true;};
    }
}
