using Godot;

public partial class ShowMVP : Control
{
    [Export]
    public Label mvpDisplayName;
    [Export]
    public Label mvpPlayerPoints;
    [Export]
    public Label mvpDistanceTravelled;
    [Export]
    public Label mvpNumOfFaceplants;
    [Export]
    public Label mvpLongestFaceplantDistance;
    [Export]
    public Label mvpComboStreak;
    [Export]
    private AnimatedSprite2D animatedSprite;
    private SettingsManager settingsManager;
    private LevelManager levelManager;

    public override void _Ready()
    {
        base._Ready();
        settingsManager = GetNode<SettingsManager>("/root/SettingsManager");
        levelManager = GetNode<LevelManager>("/root/LevelManager");
    }

    public void SetMVPControlNodeInfo(PlayerInfo mvp)
    {
        mvpDisplayName.Text = mvp.DisplayName;
        mvpPlayerPoints.Text = $"Points: {mvp.points}";
        mvpDistanceTravelled.Text = $"Distance Travelled: {mvp.HighestYPos:F1}m";

        string noOfFp = mvp.NumOfFaceplants == 0 ? "N/A" : mvp.NumOfFaceplants.ToString();
        mvpNumOfFaceplants.Text = $"Number of Faceplants: {noOfFp}";

        string disOfLongestFp = mvp.DistanceOfFurthestFaceplant == 0 ? "N/A" : $"{mvp.DistanceOfFurthestFaceplant:F1}m";
        mvpLongestFaceplantDistance.Text = $"Longest Faceplant Distance: {disOfLongestFp}";

        string comboStreak = mvp.ComboStreak == 0 ? "N/A" : $"{mvp.ComboStreak}";
        mvpComboStreak.Text = $"Highest Combo Streak: {comboStreak}";
    }

    public void SetShaderAnimatedSprite(PlayerInfo mvp)
    {
        ShaderMaterial shader = (ShaderMaterial)animatedSprite.Material;
        shader.SetShaderParameter("cape1_color_new", settingsManager.UNLTeams.GetTeam(mvp.team).TeamColours.CapeMain);
        shader.SetShaderParameter("cape2_color_new", settingsManager.UNLTeams.GetTeam(mvp.team).TeamColours.CapeTrim);
        shader.SetShaderParameter("armour_light_new", settingsManager.UNLTeams.GetTeam(mvp.team).TeamColours.ArmourLight);
        shader.SetShaderParameter("armour_med_new", settingsManager.UNLTeams.GetTeam(mvp.team).TeamColours.ArmourMedium);
        shader.SetShaderParameter("armour_dark_new", settingsManager.UNLTeams.GetTeam(mvp.team).TeamColours.ArmourDark);
        shader.SetShaderParameter("helmet_feathers_new", levelManager.uniqueColours[mvp.IdxOfUniqueFeatherColours]);
        // GD.Print($"levelManager = {levelManager != null}");
        // GD.Print($"levelManager.")
    }
}
