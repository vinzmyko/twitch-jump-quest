using System;
using System.Linq;
using Godot;

public partial class ManageTeams : Control
{
    [Export]
    private Button toMenu;
    private AnimatedSprite2D animatedSprite;
    private Button idlePreviewButton, jumpPreviewButton, facePlantPreviewButton;
    private Button teamNavigationLeft, teamNavigationRight;
    private Label numOfTeams;
    private TextureRect logoRect;
    private LineEdit teamNameLineEdit, teamAbbrevLineEdit, hexColourLineEdit;
    private ColorPickerButton capeMain, capeTrim, armourLight, armourMedium, armourDark;
    private Button addTeamButton, trashTeamButton, saveTeams;
    private Button jsonImportButton, jsonExportButton;
    private FileDialog importJsonDialog, exportJsonDialog, logoFileDialog;
    private SettingsManager settingsManager;

    public override void _Ready()
    {
        base._Ready();
        toMenu.ButtonDown += () => {SceneManager.Instance.ChangeScene("MainMenu");};

        animatedSprite = GetNode<AnimatedSprite2D>("MarginContainer/HBoxContainer/LeftVBox/Panel/AnimatedSprite2D2");

        idlePreviewButton = GetNode<Button>("MarginContainer/HBoxContainer/LeftVBox/AnimationNamesHBox/IdleButton");
        jumpPreviewButton = GetNode<Button>("MarginContainer/HBoxContainer/LeftVBox/AnimationNamesHBox/JumpButton");
        facePlantPreviewButton = GetNode<Button>("MarginContainer/HBoxContainer/LeftVBox/AnimationNamesHBox/FaceplantButton");

        teamNavigationLeft = GetNode<Button>("MarginContainer/HBoxContainer/RightVBox/TeamNavigationHBox/LeftArrowButton");
        numOfTeams = GetNode<Label>("MarginContainer/HBoxContainer/RightVBox/TeamNavigationHBox/Panel3/NumOfTeamsLabel");
        teamNavigationRight = GetNode<Button>("MarginContainer/HBoxContainer/RightVBox/TeamNavigationHBox/RightArrowButton");

        logoRect = GetNode<TextureRect>("MarginContainer/HBoxContainer/RightVBox/TeamInfoHBox/LogoPanel/TextureRect");
        teamNameLineEdit = GetNode<LineEdit>("MarginContainer/HBoxContainer/RightVBox/TeamInfoHBox/TeamNameVBox/TeamNameLineEdit");
        teamAbbrevLineEdit = GetNode<LineEdit>("MarginContainer/HBoxContainer/RightVBox/TeamInfoHBox/TeamNameVBox/TeamAbbrevLineEdit");
        hexColourLineEdit = GetNode<LineEdit>("MarginContainer/HBoxContainer/RightVBox/HexColourLineEdit");

        capeMain = GetNode<ColorPickerButton>("MarginContainer/HBoxContainer/RightVBox/ColourPicker/MarginContainer/ColourPickerVBox/CapeMainHBox/ColorPickerButton");
        capeTrim = GetNode<ColorPickerButton>("MarginContainer/HBoxContainer/RightVBox/ColourPicker/MarginContainer/ColourPickerVBox/CapeTrimHBox/ColorPickerButton");
        armourLight = GetNode<ColorPickerButton>("MarginContainer/HBoxContainer/RightVBox/ColourPicker/MarginContainer/ColourPickerVBox/ArmourLightHBox/ColorPickerButton");
        armourMedium = GetNode<ColorPickerButton>("MarginContainer/HBoxContainer/RightVBox/ColourPicker/MarginContainer/ColourPickerVBox/ArmourMediumHBox/ColorPickerButton");
        armourDark = GetNode<ColorPickerButton>("MarginContainer/HBoxContainer/RightVBox/ColourPicker/MarginContainer/ColourPickerVBox/ArmourDarkHBox2/ColorPickerButton");

        addTeamButton = GetNode<Button>("MarginContainer/HBoxContainer/RightVBox/AddRemoveTeamsHBox/MarginContainer/HBoxContainer/AddTeamButton");
        trashTeamButton = GetNode<Button>("MarginContainer/HBoxContainer/RightVBox/AddRemoveTeamsHBox/MarginContainer/HBoxContainer/TrashTeamButton");
        saveTeams = GetNode<Button>("MarginContainer/HBoxContainer/RightVBox/AddRemoveTeamsHBox/MarginContainer/HBoxContainer/SaveButton");

        jsonImportButton = GetNode<Button>("MarginContainer2/VBoxContainer/ImportJsonButton");
        jsonExportButton = GetNode<Button>("MarginContainer2/VBoxContainer/ExportJsonButton");
        importJsonDialog = GetNode<FileDialog>("MarginContainer2/ImportFileDialog");
        exportJsonDialog = GetNode<FileDialog>("MarginContainer2/ExportFileDialog");
        logoFileDialog = GetNode<FileDialog>("MarginContainer2/LogoFileDialog");

        settingsManager = GetNode<SettingsManager>("/root/SettingsManager");

        idlePreviewButton.Pressed += () => {animatedSprite.Play("Idle");};
        jumpPreviewButton.Pressed += () => {animatedSprite.SpriteFrames.SetAnimationLoop("Jump", true);animatedSprite.Play("Jump");};
        facePlantPreviewButton.Pressed += () => {animatedSprite.SpriteFrames.SetAnimationLoop("HeadOnFloor", true);animatedSprite.Play("HeadOnFloor");};

        teamNavigationLeft.Pressed += NavigateToPreviousTeam;
        teamNavigationRight.Pressed += NavigateToNextTeam;

        hexColourLineEdit.TextChanged += ValidateHexColour;

        capeMain.ColorChanged += (Color colour) => {ShaderMaterial material = (ShaderMaterial)animatedSprite.Material;material.SetShaderParameter("cape1_color_new", colour);};
        capeTrim.ColorChanged += (Color colour) => {ShaderMaterial material = (ShaderMaterial)animatedSprite.Material;material.SetShaderParameter("cape2_color_new", colour);};
        armourLight.ColorChanged += (Color colour) => {ShaderMaterial material = (ShaderMaterial)animatedSprite.Material;material.SetShaderParameter("armour_light_new", colour);};
        armourMedium.ColorChanged += (Color colour) => {ShaderMaterial material = (ShaderMaterial)animatedSprite.Material;material.SetShaderParameter("armour_med_new", colour);};
        armourDark.ColorChanged += (Color colour) => {ShaderMaterial material = (ShaderMaterial)animatedSprite.Material;material.SetShaderParameter("armour_dark_new", colour);};

        addTeamButton.Pressed += () => {AddTeamFromCurrentPage();};
        trashTeamButton.Pressed += DeleteCurrentTeam;
        saveTeams.Pressed += () => {SaveCurrentTeamChanges();settingsManager.SaveTeamsToJson();settingsManager.ShowFloatingMessage("Teams saved successfully!", true);};

        jsonImportButton.Pressed += () => 
        {
            importJsonDialog.Popup();
        };
        jsonExportButton.Pressed += () => 
        {
            exportJsonDialog.CurrentFile = "teams.json";
            exportJsonDialog.Popup();
        };
        logoFileDialog.FileSelected += (string path) => 
        {
            Image image = new Image();
            Error err = image.Load(path);
            if (err == Error.Ok)
            {
                var texture = ImageTexture.CreateFromImage(image);
                
                if (image.GetWidth() != image.GetHeight())
                {
                    settingsManager.ShowFloatingMessage("Aspect Ratio 1:1 required!", false);
                    return;
                }

                logoRect.Texture = texture;
            }
            else
            {
                settingsManager.ShowFloatingMessage("Failed to load file!", false);
            }
        };
        importJsonDialog.FileSelected += ImportTeams;
        exportJsonDialog.FileSelected += ExportTeams;

        SetProcessUnhandledInput(true);

        ToDefaults();
        UpdateTeamDisplay();
    }

    private void ValidateHexColour(string newText)
    {
        if (string.IsNullOrEmpty(newText) || (newText.StartsWith("#") && System.Text.RegularExpressions.Regex.IsMatch(newText, @"^#(?:[0-9a-fA-F]{3}){1,2}$")))
        {
            hexColourLineEdit.Modulate = Colors.White;
        }
        else
        {
            hexColourLineEdit.Modulate = Colors.Red;
        }
    }

    private void SaveCurrentTeamChanges()
    {
        var currentTeam = settingsManager.TeamPages.GetCurrentTeam();
        if (currentTeam != null)
        {
            currentTeam.TeamName = teamNameLineEdit.Text;
            currentTeam.TeamAbbreviation = teamAbbrevLineEdit.Text;
            currentTeam.HexColourCode = hexColourLineEdit.Text;
            currentTeam.TeamColours.CapeMain = capeMain.Color;
            currentTeam.TeamColours.CapeTrim = capeTrim.Color;
            currentTeam.TeamColours.ArmourLight = armourLight.Color;
            currentTeam.TeamColours.ArmourMedium = armourMedium.Color;
            currentTeam.TeamColours.ArmourDark = armourDark.Color;

            if (logoRect.Texture != null && logoRect.Texture is ImageTexture imageTexture)
            {
                string fileName = $"TeamImages/{currentTeam.TeamName.ToLower()}.png";
                string fullPath = $"user://{fileName}";
                Image image = imageTexture.GetImage();
                image.SavePng(fullPath);
                currentTeam.LogoPath = fileName;
            }
        }
    }

    private void ExportTeams(string path)
    {
        try
        {
            string json = settingsManager.ExportTeams();
            System.IO.File.WriteAllText(path, json);
            settingsManager.ShowFloatingMessage("Teams exported successfully!", true);
        }
        catch (Exception e)
        {
            GD.PrintErr($"Error exporting teams: {e.Message}");
            settingsManager.ShowFloatingMessage("Failed to export teams!", false);
        }
    }


    private void ImportTeams(string path)
    {
        try
        {
            string json = System.IO.File.ReadAllText(path);
            settingsManager.ImportTeams(json);
            settingsManager.TeamPages.InitializeFromList(settingsManager.UNLTeams.Teams);
            UpdateTeamDisplay();
            settingsManager.SaveTeamsToJson(); 
            settingsManager.ShowFloatingMessage("Teams imported and saved successfully!", true);
        }
        catch (Exception e)
        {
            GD.PrintErr($"Error importing teams: {e.Message}");
            settingsManager.ShowFloatingMessage("Failed to import teams!", false);
        }
    }


    private void DeleteCurrentTeam()
    {
        var currentTeam = settingsManager.TeamPages.GetCurrentTeam();
        if (currentTeam == null)
        {
            settingsManager.ShowFloatingMessage("No team to delete!", false);
            return;
        }

        int currentIndex = settingsManager.TeamPages.GetCurrentIndex();
        int totalTeams = settingsManager.TeamPages.GetTotalTeams();

        settingsManager.DeleteTeam(currentTeam);

        if (totalTeams > 2) 
        {
            if (currentIndex == totalTeams - 1) 
            {
                settingsManager.TeamPages.MovePrevious(); // Move to the last actual team
            }
            else if (currentIndex == totalTeams - 2) // We were on the last actual team
            {
                // stay on new team page
            }
            else
            {
                // show next team page
            }
        }
        else // only one team
        {
            // already on the default/new team apge
        }

        UpdateTeamDisplay();
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left)
        {
            if (MouseOverLogo(logoRect))
            {
                logoFileDialog.Popup();
            }
        }
    }

    private bool MouseOverLogo(Control logo)
    {

        Vector2 mousePos = GetGlobalMousePosition();

        if (mousePos.X < logo.GlobalPosition.X || mousePos.X > logo.GlobalPosition.X + logo.Size.X
            || mousePos.Y < logo.GlobalPosition.Y || mousePos.Y > logo.GlobalPosition.Y + logo.Size.Y)
            {
                return false;
            }
        return true;
    }

    private void AddTeamFromCurrentPage()
    {
        string teamName = teamNameLineEdit.Text;
        string teamAbbrev = teamAbbrevLineEdit.Text;
        string hexColourCode = hexColourLineEdit.Text;
        Texture2D logo = logoRect.Texture;
        ColorPickerButton[] avatarColours = new ColorPickerButton[5]
        {
            capeMain,
            capeTrim,
            armourLight,
            armourMedium,
            armourDark
        };

        string[] variablesThatAreNull = new string[4];
        string errorText = string.Empty;
        if (string.IsNullOrEmpty(teamName))
            errorText += "Team Name ";
        if (string.IsNullOrEmpty(teamAbbrev))
            errorText += "Team Abbreviation ";
        if (string.IsNullOrEmpty(hexColourCode))
            errorText += "Hex Colour Code ";
        if (logo == null)
            errorText += "Logo Image ";
        
        if (errorText != string.Empty)
        {
            settingsManager.ShowFloatingMessage($"{errorText}need to be filled!", false);
            return;
        }

        // Add the team to the list (also adds it to teampages)
        settingsManager.AddTeamToList(teamName, teamAbbrev, logo, avatarColours, hexColourCode);
        
        // Move to the new team's page
        settingsManager.TeamPages.MoveNext();
        
        settingsManager.ShowFloatingMessage($"{teamName} has been added!", true);
        UpdateTeamDisplay();
        ToDefaults();
    }

    private void NavigateToPreviousTeam()
    {
        if (settingsManager.TeamPages.MovePrevious())
        {
            UpdateTeamDisplay();
        }
    }

    private void NavigateToNextTeam()
    {
        if (settingsManager.TeamPages.MoveNext())
        {
            UpdateTeamDisplay();
        }
    }

    private void UpdateTeamDisplay()
    {
        var currentTeam = settingsManager.TeamPages.GetCurrentTeam();
        if (currentTeam != null)
        {
            teamNameLineEdit.Text = currentTeam.TeamName;
            teamAbbrevLineEdit.Text = currentTeam.TeamAbbreviation;
            hexColourLineEdit.Text = currentTeam.HexColourCode;

            // Load and set the logo texture
            string fullPath = $"user://{currentTeam.LogoPath.ToLower()}";
            if (FileAccess.FileExists(fullPath))
            {
                Image img = new Image();
                Error err = img.Load(fullPath);
                if (err == Error.Ok)
                {
                    logoRect.Texture = ImageTexture.CreateFromImage(img);
                }
                else
                {
                    GD.PrintErr($"Failed to load image: {err}");
                    logoRect.Texture = null;
                }
            }
            else
            {
                GD.PrintErr($"Logo file not found: {fullPath}");
                logoRect.Texture = null;
            }

            capeMain.Color = currentTeam.TeamColours.CapeMain;
            capeTrim.Color = currentTeam.TeamColours.CapeTrim;
            armourLight.Color = currentTeam.TeamColours.ArmourLight;
            armourMedium.Color = currentTeam.TeamColours.ArmourMedium;
            armourDark.Color = currentTeam.TeamColours.ArmourDark;

            SetSpriteShaderParameters("cape1_color_new", capeMain.Color);
            SetSpriteShaderParameters("cape2_color_new", capeTrim.Color);
            SetSpriteShaderParameters("armour_light_new", armourLight.Color);
            SetSpriteShaderParameters("armour_med_new", armourMedium.Color);
            SetSpriteShaderParameters("armour_dark_new", armourDark.Color);
        }
        else
        {
            ToDefaults();
        }

        int currentIndex = settingsManager.TeamPages.GetCurrentIndex();
        int totalTeams = settingsManager.TeamPages.GetTotalTeams();
        numOfTeams.Text = $"{currentIndex}/{totalTeams}";

        // Enable/disable navigation buttons
        teamNavigationLeft.Disabled = (currentIndex <= 1);
        teamNavigationRight.Disabled = (currentIndex >= totalTeams);
    }

    private void ToDefaults()
    {
        teamNameLineEdit.Text = string.Empty;
        teamAbbrevLineEdit.Text = string.Empty;
        hexColourLineEdit.Text = "#000000";
        logoRect.Texture = null;
        capeMain.Color = Color.FromHtml("#0a7030");
        capeTrim.Color = Color.FromHtml("#eba724");
        armourLight.Color = Color.FromHtml("#eadfd1");
        armourMedium.Color = Color.FromHtml("#b3aaa1");
        armourDark.Color = Color.FromHtml("#7c776f");

        SetSpriteShaderParameters("cape1_color_new", capeMain.Color);
        SetSpriteShaderParameters("cape2_color_new", capeTrim.Color);
        SetSpriteShaderParameters("armour_light_new", armourLight.Color);
        SetSpriteShaderParameters("armour_med_new", armourMedium.Color);
        SetSpriteShaderParameters("armour_dark_new", armourDark.Color);

    }

    private void SetSpriteShaderParameters(string parameter, Color colour)
    {
        ShaderMaterial material = (ShaderMaterial)animatedSprite.Material;material.SetShaderParameter(parameter, colour);
    }

}
