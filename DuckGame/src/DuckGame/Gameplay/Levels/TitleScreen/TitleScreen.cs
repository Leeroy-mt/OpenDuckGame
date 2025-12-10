using System;
using System.Collections.Generic;
using System.Linq;

namespace DuckGame;

public class TitleScreen : Level
{
    public List<StarParticle> particles = new List<StarParticle>();

    private int wait;

    private float dim = 0.8f;

    private float moveSpeed = 1f;

    private float moveWait = 1f;

    private float flash;

    private bool _returnFromArcade;

    private Profile _arcadeProfile;

    private InputProfile _arcadeInputProfile;

    private TitleMenuSelection _selection = TitleMenuSelection.Play;

    private TitleMenuSelection _desiredSelection = TitleMenuSelection.Play;

    private BigTitle _title;

    private BitmapFont _font;

    private Sprite _background;

    private Sprite _optionsPlatform;

    private Sprite _rightPlatform;

    private Sprite _leftPlatform;

    private Sprite _beamPlatform;

    private Sprite _upperMonitor;

    private Sprite _optionsTV;

    private Sprite _libraryBookcase;

    private Sprite _editorBench;

    private Sprite _editorBenchPaint;

    private Sprite _bigUButton;

    private Sprite _airlock;

    private SpriteMap _controls;

    private Sprite _starField;

    public int roundsPerSet = 8;

    public int setsPerGame = 3;

    private SpaceBackgroundMenu _space;

    private bool _fadeIn;

    private bool _fadeInFull;

    private float _pressStartBlink;

    private string _selectionText = "";

    private string _selectionTextDesired = "MULTIPLAYER";

    private float _selectionFade = 1f;

    private int _controlsFrame;

    private int _controlsFrameDesired = 1;

    private float _controlsFade = 1f;

    private OptionsBeam _optionsBeam;

    private LibraryBeam _libraryBeam;

    private MultiBeam _multiBeam;

    private EditorBeam _editorBeam;

    private Duck _duck;

    private bool _enterMultiplayer;

    private UIComponent _optionsGroup;

    private MenuBoolean _quit = new MenuBoolean();

    private MenuBoolean _dontQuit = new MenuBoolean();

    private UIMenu _quitMenu;

    private UIMenu _optionsMenu;

    private UIMenu _controlConfigMenu;

    private UIMenu _graphicsMenu;

    private UIMenu _audioMenu;

    private UIMenu _flagMenu;

    private UIMenu _parentalControlsMenu;

    private UIMenu _duckGameUpdateMenu;

    private UIMenu _modsDisabledMenu;

    private UIMenu _steamWarningMessage;

    private UIComponent _pauseGroup;

    private UIMenu _mainPauseMenu;

    private MenuBoolean _enterCreditsMenuBool = new MenuBoolean();

    private UIMenu _betaMenu;

    private UIMenu _cloudConfigMenu;

    private UIMenu _cloudDeleteConfirmMenu;

    private UIMenu _accessibilityMenu;

    private UIMenu _ttsMenu;

    private UIMenu _blockMenu;

    private UIMenu _modConfigMenu;

    private UICloudManagement _cloudManagerMenu;

    private bool _enterEditor;

    private bool _enterCredits;

    private bool _enterArcade;

    private static bool _hasMenusOpen = false;

    public static bool modsChanged = false;

    private static bool firstStart = true;

    public List<List<string>> creditsRoll = new List<List<string>>();

    private bool showPendingDeletionDialogue;

    private bool showSizeNotificationDialogue;

    private bool _fadeBackground;

    private bool _enterLibrary;

    private bool _enterBuyScreen;

    private float extraFade = 1f;

    private bool _startedMusic;

    private float starWait;

    private float switchWait = 1f;

    private float creditsScroll;

    private bool shownPrompt;

    private bool startStars = true;

    private int cpick;

    private bool quittingCredits;

    private bool showedNewVersionStartup;

    private bool showedModsDisabled;

    private int time;

    private static bool _showedSteamFailMessage = false;

    public bool menuOpen
    {
        get
        {
            if (!Options.menuOpen)
            {
                return _enterMultiplayer;
            }
            return true;
        }
    }

    public static bool hasMenusOpen => _hasMenusOpen;

    public TitleScreen()
        : this(returnFromArcade: false, null)
    {
    }

    public TitleScreen(bool returnFromArcade, Profile arcadeProfile)
    {
        _centeredView = true;
        _returnFromArcade = returnFromArcade;
        _arcadeProfile = arcadeProfile;
        if (arcadeProfile != null)
        {
            _arcadeInputProfile = arcadeProfile.inputProfile;
        }
    }

    private void CloudDelete()
    {
        Cloud.DeleteAllCloudData(pNewDataOnly: false);
        DuckFile.DeleteAllSaveData();
    }

    private void AddCreditLine(params string[] s)
    {
        creditsRoll.Add(new List<string>(s));
    }

    public override void Initialize()
    {
        Vote.ClearVotes();
        Program.gameLoadedSuccessfully = true;
        Global.Save();
        HUD.ClearPlayerChangeDisplays();
        AddCreditLine("DUCK GAME");
        AddCreditLine("");
        AddCreditLine("");
        AddCreditLine("");
        AddCreditLine("");
        AddCreditLine("|CREDITSGRAY|@LWINGGRAY@COMMUNITY HERO@RWINGGRAY@");
        AddCreditLine("John \"BroDuck\" Pichardo");
        AddCreditLine("");
        AddCreditLine("");
        AddCreditLine("|CREDITSGRAY|@LWINGGRAY@LEAD TESTERS@RWINGGRAY@");
        AddCreditLine("Jacob Paul");
        AddCreditLine("Tyler Molz");
        AddCreditLine("Andrew Morrish");
        AddCreditLine("Dayton McKay");
        AddCreditLine("");
        AddCreditLine("");
        AddCreditLine("|CREDITSGRAY|@LWINGGRAY@MIGHTY HELPFUL FRIEND@RWINGGRAY@");
        AddCreditLine("|DGGREEN|YupDanielThatsMe");
        AddCreditLine("");
        AddCreditLine("");
        AddCreditLine("|CREDITSGRAY|@LWINGGRAY@HEROS OF MOD@RWINGGRAY@");
        AddCreditLine("Dord");
        AddCreditLine("YupDanielThatsMe");
        AddCreditLine("YoloCrayolo3");
        AddCreditLine("Zloty_Diament");
        AddCreditLine("eim64");
        AddCreditLine("Antikore");
        AddCreditLine("");
        AddCreditLine("");
        AddCreditLine("|CREDITSGRAY|@LWINGGRAY@TESTERS@RWINGGRAY@");
        AddCreditLine("John Pichardo", "Lotad");
        AddCreditLine("Tufukins", "Sleepy Jirachi");
        AddCreditLine("Paul Hartling", "thebluecosmonaut");
        AddCreditLine("Dan Gaechter", "James Nieves");
        AddCreditLine("Dr. Docter", "ShyNieke");
        AddCreditLine("Spectaqual", "RealGnomeTasty");
        AddCreditLine("Karim Aifi", "Zaahck");
        AddCreditLine("dino rex (guy)", "Peter Smith");
        AddCreditLine("Colin Jacobson", "mage legend");
        AddCreditLine("YvngXero", "Trevor Etzold");
        AddCreditLine("Fluury", "Phantom329");
        AddCreditLine("Kevin Duffy", "Michael Niemann");
        AddCreditLine("Zloty_Diament", "Ben");
        AddCreditLine("Bolus", "Unluck");
        AddCreditLine("Temppuuh", "Rasenshriken");
        AddCreditLine("Andresian", "Spencer Portwood");
        AddCreditLine("James \"Sunder\" Beliakoff");
        AddCreditLine("David Sabosky (SidDaSloth)");
        AddCreditLine("Jordan \"Renim\" Gauge");
        AddCreditLine("Tommaso \"Giampiero\" Bresciani");
        AddCreditLine("Nicodemo \"Nikkodemus\" Bresciani");
        AddCreditLine("Valentin Zeyfang (RedMser)");
        AddCreditLine("Luke Bromley (mrred55)");
        AddCreditLine("Christopher Alan Bell");
        AddCreditLine("Koteeevvv");
        AddCreditLine("Soh", "NiK0");
        AddCreditLine("cfif126", "kalamari");
        AddCreditLine("Mike Timofeev");
        AddCreditLine("JYAD (Just Your Average Duck)");
        AddCreditLine(" Argo The Rat");
        AddCreditLine("Adam Urbina");
        AddCreditLine("Leonardo \"Baffo\" Magnani");
        AddCreditLine("The Burger Always Wins");
        AddCreditLine("Max32212 B)", "RaV3_past");
        AddCreditLine("Collin", "|DGPURPLE|Drake");
        AddCreditLine("Tater");
        AddCreditLine("");
        AddCreditLine("Jaydex72");
        AddCreditLine("");
        AddCreditLine("");
        AddCreditLine("|CREDITSGRAY|@LWINGGRAY@EDITOR SEARCH IDEA@RWINGGRAY@");
        AddCreditLine("Zloty_Diament");
        AddCreditLine("");
        AddCreditLine("");
        AddCreditLine("|CREDITSGRAY|@LWINGGRAY@A DUCK GAME COSPLAYER@RWINGGRAY@");
        AddCreditLine("Colin Lamb");
        AddCreditLine("");
        AddCreditLine("");
        AddCreditLine("|CREDITSGRAY|@LWINGGRAY@SLEEPY AND FRIENDS@RWINGGRAY@");
        AddCreditLine("Lotad");
        AddCreditLine("Sleepy Jirachi");
        AddCreditLine("Silverlace");
        AddCreditLine("Slimy");
        AddCreditLine("");
        AddCreditLine("");
        AddCreditLine("|CREDITSGRAY|@LWINGGRAY@FEATHERS WILL FLY CREW@RWINGGRAY@");
        AddCreditLine("Dan \"lucidinertia\" Myszak");
        AddCreditLine("Yannick \"Becer\" Marcotte-Gourde");
        AddCreditLine("Aleksander \"Acrimonious Defect\" K.D.");
        AddCreditLine("Tater", "KlockworkCanary");
        AddCreditLine("Conre", "Xatmamune");
        AddCreditLine("White Ink", "CaptainCrack");
        AddCreditLine("laduck", "This Guy");
        AddCreditLine("Repiteo", "VirtualFishbowl");
        AddCreditLine("Slinky", "JaYlab212");
        AddCreditLine("", "");
        AddCreditLine("The Entire FWF Community!");
        AddCreditLine("");
        AddCreditLine("");
        AddCreditLine("|CREDITSGRAY|@LWINGGRAY@RUSS MONEY@RWINGGRAY@");
        AddCreditLine("AS HIMSELF");
        AddCreditLine("");
        AddCreditLine("");
        AddCreditLine("");
        AddCreditLine("");
        AddCreditLine("");
        AddCreditLine("");
        AddCreditLine("");
        AddCreditLine("DEVELOPMENT TEAM");
        AddCreditLine("");
        AddCreditLine("|CREDITSGRAY|@LWINGGRAY@ART, PROGRAMMING, MUSIC@RWINGGRAY@");
        AddCreditLine("Landon Podbielski");
        AddCreditLine("");
        AddCreditLine("|CREDITSGRAY|@LWINGGRAY@ROOM FURNITURE@RWINGGRAY@");
        AddCreditLine("|CREDITSGRAY|@LWINGGRAY@HOME UPDATE HAT ART@RWINGGRAY@");
        AddCreditLine("Dayton McKay");
        AddCreditLine("");
        AddCreditLine("|CREDITSGRAY|@LWINGGRAY@MOD SUPPORT PROGRAMMER@RWINGGRAY@");
        AddCreditLine("Paril");
        AddCreditLine("");
        AddCreditLine("|CREDITSGRAY|@LWINGGRAY@WAFFLES, CLAMS, HIGHFIVES HATS@RWINGGRAY@");
        AddCreditLine("Lindsey Layne King");
        AddCreditLine("");
        AddCreditLine("|CREDITSGRAY|@LWINGGRAY@EGGPAL, BRAD HATS@RWINGGRAY@");
        AddCreditLine("mushbuh");
        AddCreditLine("");
        AddCreditLine("|CREDITSGRAY|@LWINGGRAY@KERCHIEFS, POSTALS, WAHHS HATS@RWINGGRAY@");
        AddCreditLine("Case Marsteller");
        AddCreditLine("");
        AddCreditLine("|CREDITSGRAY|@LWINGGRAY@B52s, UUFOS HATS@RWINGGRAY@");
        AddCreditLine("William Baldwin");
        AddCreditLine("");
        AddCreditLine("|CREDITSGRAY|@LWINGGRAY@WOLVES HAT, MILKSHAKE@RWINGGRAY@");
        AddCreditLine("Dord");
        AddCreditLine("");
        AddCreditLine("");
        AddCreditLine("");
        AddCreditLine("NINTENDO SWITCH PORT");
        AddCreditLine("&");
        AddCreditLine("DEFINITIVE EDITION");
        AddCreditLine("Armature Studio");
        AddCreditLine("");
        AddCreditLine("|CREDITSGRAY|@LWINGGRAY@ENGINEERING@RWINGGRAY@");
        AddCreditLine("John Allensworth");
        AddCreditLine("Tom Ivey");
        AddCreditLine("Bryan Wagstaff");
        AddCreditLine("");
        AddCreditLine("|CREDITSGRAY|@LWINGGRAY@DIRECTOR OF PRODUCTION@RWINGGRAY@");
        AddCreditLine("Mark Nau");
        AddCreditLine("");
        AddCreditLine("|CREDITSGRAY|@LWINGGRAY@PRODUCTION@RWINGGRAY@");
        AddCreditLine("Tom Ivey");
        AddCreditLine("Mike Pirrone");
        AddCreditLine("");
        AddCreditLine("|CREDITSGRAY|@LWINGGRAY@QUALITY ASSURANCE@RWINGGRAY@");
        AddCreditLine("Gwen Dalmacio");
        AddCreditLine("Mike Pirrone");
        AddCreditLine("");
        AddCreditLine("|CREDITSGRAY|@LWINGGRAY@ARMATURE OPERATIONS@RWINGGRAY@");
        AddCreditLine("Nadine Rossignol");
        AddCreditLine("Nicole Casarona");
        AddCreditLine("Michael Thai");
        AddCreditLine("");
        AddCreditLine("|CREDITSGRAY|@LWINGGRAY@BUSINESS DEVELOPMENT@RWINGGRAY@");
        AddCreditLine("Jonathan Zamkoff");
        AddCreditLine("");
        AddCreditLine("|CREDITSGRAY|@LWINGGRAY@STUDIO EXECUTIVES@RWINGGRAY@");
        AddCreditLine("Greg John");
        AddCreditLine("Todd Keller");
        AddCreditLine("Mark Pacini");
        AddCreditLine("Jonathan Zamkoff");
        AddCreditLine("");
        AddCreditLine("|CREDITSGRAY|@LWINGGRAY@SPECIAL THANKS@RWINGGRAY@");
        AddCreditLine("Vitor Menezes");
        AddCreditLine("Wayne Sikes");
        AddCreditLine("");
        AddCreditLine("");
        AddCreditLine("");
        AddCreditLine("");
        AddCreditLine("ADULT SWIM GAMES");
        AddCreditLine("");
        AddCreditLine("Liz Pate");
        AddCreditLine("Jacob Paul");
        AddCreditLine("Brian Marquez");
        AddCreditLine("Daniel Fuller");
        AddCreditLine("Peter Gollan");
        AddCreditLine("Mariam Naziripour");
        AddCreditLine("Dan Nichols");
        AddCreditLine("Adam Baptiste");
        AddCreditLine("Case Marsteller");
        AddCreditLine("");
        AddCreditLine("Chris Johnston");
        AddCreditLine("Steve Gee");
        AddCreditLine("Charles Park");
        AddCreditLine("Kyle Young");
        AddCreditLine("Duke Nguyen");
        AddCreditLine("Andre Curtis");
        AddCreditLine("Briana Chichester");
        AddCreditLine("William Baldwin");
        AddCreditLine("Taylor Anderson-Barkley");
        AddCreditLine("Josh Terry");
        AddCreditLine("Maddie Beasley");
        AddCreditLine("Justin Morris");
        AddCreditLine("Joseph DuBois");
        AddCreditLine("Lindsey Wade");
        AddCreditLine("Adam Hatch");
        AddCreditLine("Kristy Sottilaro");
        AddCreditLine("");
        AddCreditLine("Jeff Olsen");
        AddCreditLine("Tucker Dean");
        AddCreditLine("Elizabeth Murphy");
        AddCreditLine("David Verble");
        AddCreditLine("Sean Baptiste");
        AddCreditLine("Jacqui Collins");
        AddCreditLine("Zo Douglas");
        AddCreditLine("Megan Fausti");
        AddCreditLine("Abigail Tyson");
        AddCreditLine("Ryan Murray");
        AddCreditLine("");
        AddCreditLine("");
        AddCreditLine("");
        AddCreditLine("Thank you OUYA for publishing");
        AddCreditLine("the original version of Duck Game.");
        AddCreditLine("Especially Bob Mills, who");
        AddCreditLine("made it all happen.");
        AddCreditLine("");
        AddCreditLine("We need to go camping again.");
        AddCreditLine("");
        AddCreditLine("");
        AddCreditLine("");
        AddCreditLine("Thank you ADULT SWIM GAMES");
        AddCreditLine("for publishing Duck Game, and");
        AddCreditLine("for doing so much promotion and");
        AddCreditLine("testing.");
        AddCreditLine("");
        AddCreditLine("");
        AddCreditLine("Thank you Paril for");
        AddCreditLine("writing the mod support for Duck Game.");
        AddCreditLine("Mods wouldn't have been possible");
        AddCreditLine("without you.");
        AddCreditLine("");
        AddCreditLine("");
        AddCreditLine("BroDuck you've been a huge help");
        AddCreditLine("keeping the community running,");
        AddCreditLine("I don't know what would have happened");
        AddCreditLine("without your help.");
        AddCreditLine("");
        AddCreditLine("");
        AddCreditLine("DORD your weapons mod is absolutely");
        AddCreditLine("amazing and beautiful.");
        AddCreditLine("");
        AddCreditLine("");
        AddCreditLine("");
        AddCreditLine("");
        AddCreditLine("Thank you everyone for playing");
        AddCreditLine("Duck Game, for all your support,");
        AddCreditLine("and for being so kind.");
        AddCreditLine("");
        AddCreditLine("");
        AddCreditLine("");
        AddCreditLine("");
        AddCreditLine("");
        AddCreditLine("");
        AddCreditLine("The End");
        for (int i = 0; i < 300; i++)
        {
            AddCreditLine("");
        }
        AddCreditLine("Cya later!");
        if (!DG.InitializeDRM())
        {
            Level.current = new BetaScreen();
            return;
        }
        _starField = new Sprite("background/starField");
        TeamSelect2.DefaultSettings();
        if (Network.isActive)
        {
            Network.EndNetworkingSession(new DuckNetErrorInfo(DuckNetError.ControlledDisconnect, "Returned to title screen."));
        }
        if ((Music.currentSong != "Title" && Music.currentSong != "TitleDemo") || Music.finished)
        {
            Music.Play("Title");
        }
        if (GameMode.playedGame)
        {
            GameMode.playedGame = false;
        }
        _optionsGroup = new UIComponent(Layer.HUD.camera.width / 2f, Layer.HUD.camera.height / 2f, 0f, 0f);
        _optionsMenu = new UIMenu("@WRENCH@OPTIONS@SCREWDRIVER@", Layer.HUD.camera.width / 2f, Layer.HUD.camera.height / 2f, 190f, -1f, "@CANCEL@BACK @SELECT@SELECT");
        _controlConfigMenu = new UIControlConfig(_optionsMenu, "@WRENCH@DEVICE DEFAULTS@SCREWDRIVER@", Layer.HUD.camera.width / 2f, Layer.HUD.camera.height / 2f, 194f, -1f, "@WASD@@SELECT@ADJUST @CANCEL@BACK");
        _flagMenu = new UIFlagSelection(_optionsMenu, "FLAG", Layer.HUD.camera.width / 2f, Layer.HUD.camera.height / 2f);
        _optionsMenu.Add(new UIMenuItemSlider("SFX Volume", null, new FieldBinding(Options.Data, "sfxVolume"), 1f / 15f));
        _optionsMenu.Add(new UIMenuItemSlider("Music Volume", null, new FieldBinding(Options.Data, "musicVolume"), 1f / 15f));
        _graphicsMenu = Options.CreateGraphicsMenu(_optionsMenu);
        _audioMenu = Options.CreateAudioMenu(_optionsMenu);
        _accessibilityMenu = Options.CreateAccessibilityMenu(_optionsMenu);
        _ttsMenu = Options.tempTTSMenu;
        _blockMenu = Options.tempBlockMenu;
        _optionsMenu.Add(new UIMenuItemSlider("Rumble Intensity", null, new FieldBinding(Options.Data, "rumbleIntensity"), 1f / 15f));
        _optionsMenu.Add(new UIText(" ", Color.White));
        _optionsMenu.Add(new UIMenuItemToggle("SHENANIGANS", null, new FieldBinding(Options.Data, "shennanigans")));
        _optionsMenu.Add(new UIText(" ", Color.White));
        _optionsMenu.Add(new UIMenuItem("EDIT CONTROLS", new UIMenuActionOpenMenuCallFunction(_optionsMenu, _controlConfigMenu, UIControlConfig.ResetWarning), UIAlign.Center, default(Color), backButton: true));
        _optionsMenu.Add(new UIMenuItem("GRAPHICS", new UIMenuActionOpenMenu(_optionsMenu, _graphicsMenu), UIAlign.Center, default(Color), backButton: true));
        _optionsMenu.Add(new UIMenuItem("AUDIO", new UIMenuActionOpenMenu(_optionsMenu, _audioMenu), UIAlign.Center, default(Color), backButton: true));
        _cloudConfigMenu = new UIMenu("@WRENCH@SAVE DATA@SCREWDRIVER@", Layer.HUD.camera.width / 2f, Layer.HUD.camera.height / 2f, 280f, -1f, "@CANCEL@BACK @SELECT@SELECT");
        _cloudDeleteConfirmMenu = new UIMenu("CLEAR SAVE DATA?", Layer.HUD.camera.width / 2f, Layer.HUD.camera.height / 2f, 280f, -1f, "@SELECT@SELECT");
        _cloudManagerMenu = new UICloudManagement(_cloudConfigMenu);
        _cloudConfigMenu.Add(new UIMenuItemToggle("Enable Steam Cloud", null, new FieldBinding(Options.Data, "cloud")));
        _cloudConfigMenu.Add(new UIMenuItem("Manage Save Data", new UIMenuActionOpenMenu(_cloudConfigMenu, _cloudManagerMenu)));
        _cloudConfigMenu.Add(new UIMenuItem("|DGRED|CLEAR ALL SAVE DATA", new UIMenuActionOpenMenu(_cloudConfigMenu, _cloudDeleteConfirmMenu), UIAlign.Center, default(Color), backButton: true));
        _cloudDeleteConfirmMenu.Add(new UIText("This will DELETE all data", Colors.DGRed));
        _cloudDeleteConfirmMenu.Add(new UIText("(Profiles, Options, Levels)", Colors.DGRed));
        _cloudDeleteConfirmMenu.Add(new UIText("from your Duck Game save!", Colors.DGRed));
        _cloudDeleteConfirmMenu.Add(new UIText("", Colors.DGRed));
        _cloudDeleteConfirmMenu.Add(new UIText("Do not do this, unless you're", Colors.DGRed));
        _cloudDeleteConfirmMenu.Add(new UIText("absolutely sure!", Colors.DGRed));
        _cloudDeleteConfirmMenu.Add(new UIText(" ", Colors.DGRed));
        _cloudDeleteConfirmMenu.Add(new UIMenuItem("|DGRED|DELETE AND RESTART.", new UIMenuActionOpenMenuCallFunction(_cloudDeleteConfirmMenu, _cloudConfigMenu, CloudDelete)));
        _cloudDeleteConfirmMenu.Add(new UIMenuItem("|DGGREEN|CANCEL!", new UIMenuActionOpenMenu(_cloudDeleteConfirmMenu, _cloudConfigMenu)));
        _cloudDeleteConfirmMenu._defaultSelection = 1;
        _cloudDeleteConfirmMenu.SetBackFunction(new UIMenuActionOpenMenu(_cloudDeleteConfirmMenu, _cloudConfigMenu));
        _cloudDeleteConfirmMenu.Close();
        _cloudConfigMenu.Add(new UIText(" ", Colors.DGBlue));
        _cloudConfigMenu.Add(new UIMenuItem("BACK", new UIMenuActionOpenMenu(_cloudConfigMenu, _optionsMenu), UIAlign.Center, default(Color), backButton: true));
        _optionsMenu.Add(new UIText(" ", Color.White));
        if (MonoMain.moddingEnabled)
        {
            _modConfigMenu = new UIModManagement(_optionsMenu, "@WRENCH@MANAGE MODS@SCREWDRIVER@", Layer.HUD.camera.width, Layer.HUD.camera.height, 550f, -1f, "@WASD@@SELECT@ADJUST @MENU1@TOGGLE @CANCEL@BACK");
            _optionsMenu.Add(new UIMenuItem("MANAGE MODS", new UIMenuActionOpenMenu(_optionsMenu, _modConfigMenu), UIAlign.Center, default(Color), backButton: true));
        }
        _optionsMenu.Add(new UIMenuItem("SELECT FLAG", new UIMenuActionOpenMenu(_optionsMenu, _flagMenu), UIAlign.Center, default(Color), backButton: true));
        _optionsMenu.Add(new UIText(" ", Color.White));
        if (_accessibilityMenu != null)
        {
            _optionsMenu.Add(new UIMenuItem("USABILITY", new UIMenuActionOpenMenu(_optionsMenu, _accessibilityMenu), UIAlign.Center, default(Color), backButton: true));
        }
        _optionsMenu.SetBackFunction(new UIMenuActionCloseMenuCallFunction(_optionsMenu, OptionsSaveAndClose));
        _optionsMenu.Close();
        _optionsGroup.Add(_optionsMenu, doAnchor: false);
        _controlConfigMenu.Close();
        _flagMenu.Close();
        if (MonoMain.moddingEnabled)
        {
            _modConfigMenu.Close();
        }
        _cloudConfigMenu.Close();
        _cloudManagerMenu.Close();
        _optionsGroup.Add(_controlConfigMenu, doAnchor: false);
        _optionsGroup.Add((_controlConfigMenu as UIControlConfig)._confirmMenu, doAnchor: false);
        _optionsGroup.Add((_controlConfigMenu as UIControlConfig)._warningMenu, doAnchor: false);
        _optionsGroup.Add(_flagMenu, doAnchor: false);
        _optionsGroup.Add(_graphicsMenu, doAnchor: false);
        _optionsGroup.Add(_audioMenu, doAnchor: false);
        if (_accessibilityMenu != null)
        {
            _optionsGroup.Add(_accessibilityMenu, doAnchor: false);
        }
        if (_ttsMenu != null)
        {
            _optionsGroup.Add(_ttsMenu, doAnchor: false);
        }
        if (_blockMenu != null)
        {
            _optionsGroup.Add(_blockMenu, doAnchor: false);
        }
        if (MonoMain.moddingEnabled)
        {
            _optionsGroup.Add(_modConfigMenu, doAnchor: false);
            _optionsGroup.Add((_modConfigMenu as UIModManagement)._modSettingsMenu, doAnchor: false);
            _optionsGroup.Add((_modConfigMenu as UIModManagement)._editModMenu, doAnchor: false);
            _optionsGroup.Add((_modConfigMenu as UIModManagement)._yesNoMenu, doAnchor: false);
        }
        _optionsGroup.Add(_cloudManagerMenu, doAnchor: false);
        _optionsGroup.Add(_cloudManagerMenu._deleteMenu, doAnchor: false);
        _optionsGroup.Add(_cloudConfigMenu, doAnchor: false);
        _optionsGroup.Add(_cloudDeleteConfirmMenu, doAnchor: false);
        _optionsGroup.Close();
        Level.Add(_optionsGroup);
        _betaMenu = new UIMenu("@WRENCH@WELCOME TO BETA!@SCREWDRIVER@", Layer.HUD.camera.width / 2f, Layer.HUD.camera.height / 2f, 240f, -1f, "@CANCEL@OK!");
        UIImage message = new UIImage(new Sprite("message"), UIAlign.Center, 0.25f, 51f);
        _betaMenu.Add(message);
        _betaMenu.Close();
        _betaMenu._backButton = new UIMenuItem("BACK", new UIMenuActionCloseMenu(_betaMenu), UIAlign.Center, default(Color), backButton: true);
        _betaMenu._isMenu = true;
        Level.Add(_betaMenu);
        _pauseGroup = new UIComponent(Layer.HUD.camera.width / 2f, Layer.HUD.camera.height / 2f, 0f, 0f);
        _pauseGroup.isPauseMenu = true;
        _mainPauseMenu = new UIMenu("@LWING@DUCK GAME@RWING@", Layer.HUD.camera.width / 2f, Layer.HUD.camera.height / 2f, 160f, -1f, "@CANCEL@CLOSE @SELECT@SELECT");
        _quitMenu = new UIMenu("REALLY QUIT?", Layer.HUD.camera.width / 2f, Layer.HUD.camera.height / 2f, 160f, -1f, "@CANCEL@BACK @SELECT@SELECT");
        _quitMenu.Add(new UIMenuItem("NO!", new UIMenuActionOpenMenu(_quitMenu, _mainPauseMenu)));
        _quitMenu.Add(new UIMenuItem("YES!", new UIMenuActionCloseMenuSetBoolean(_pauseGroup, _quit)));
        _quitMenu.Close();
        _pauseGroup.Add(_quitMenu, doAnchor: false);
        _parentalControlsMenu = new UIMenu("PARENTAL CONTROLS", Layer.HUD.camera.width / 2f, Layer.HUD.camera.height / 2f, -1f, -1f, "@CANCEL@CLOSE @SELECT@SELECT");
        _parentalControlsMenu.Add(new UIText("Certain online features have been", Color.White));
        _parentalControlsMenu.Add(new UIText("disabled by Parental Controls.", Color.White));
        _parentalControlsMenu.Add(new UIText("", Color.White));
        _parentalControlsMenu.Add(new UIMenuItem("OK", new UIMenuActionCloseMenu(_pauseGroup)));
        _parentalControlsMenu.Close();
        _pauseGroup.Add(_parentalControlsMenu, doAnchor: false);
        int padLength = 50;
        float heightAdd = 3f;
        _duckGameUpdateMenu = new UIMenu("DUCK GAME 1.5!", Layer.HUD.camera.width / 2f, Layer.HUD.camera.height / 2f, 220f, -1f, "@SELECT@OK!");
        _duckGameUpdateMenu.Add(new UIText("", Color.White, UIAlign.Center, -3f)
        {
            scale = new Vec2(0.5f)
        });
        _duckGameUpdateMenu.Add(new UIText("Duck Game has received a major update!", Color.White, UIAlign.Center, -4f)
        {
            scale = new Vec2(0.5f)
        });
        _duckGameUpdateMenu.Add(new UIText("Some of the biggest changes include:", Color.White, UIAlign.Center, -4f)
        {
            scale = new Vec2(0.5f)
        });
        _duckGameUpdateMenu.Add(new UIText("", Color.White, UIAlign.Center, -3f)
        {
            scale = new Vec2(0.5f)
        });
        _duckGameUpdateMenu.Add(new UIText("-Support for up to 8 players and 4 spectators".Padded(padLength), Color.White, UIAlign.Center, 0f - heightAdd)
        {
            scale = new Vec2(0.5f)
        });
        _duckGameUpdateMenu.Add(new UIText("-New hats, weapons, equipment and furniture".Padded(padLength), Color.White, UIAlign.Center, 0f - heightAdd)
        {
            scale = new Vec2(0.5f)
        });
        _duckGameUpdateMenu.Add(new UIText("-New city themed levels".Padded(padLength), Color.White, UIAlign.Center, 0f - heightAdd)
        {
            scale = new Vec2(0.5f)
        });
        _duckGameUpdateMenu.Add(new UIText("", Color.White, UIAlign.Center, -3f)
        {
            scale = new Vec2(0.5f)
        });
        _duckGameUpdateMenu.Add(new UIText("-Custom font support for chat".Padded(padLength), Color.White, UIAlign.Center, 0f - heightAdd)
        {
            scale = new Vec2(0.5f)
        });
        _duckGameUpdateMenu.Add(new UIText("-4K and custom resolution support".Padded(padLength), Color.White, UIAlign.Center, 0f - heightAdd)
        {
            scale = new Vec2(0.5f)
        });
        _duckGameUpdateMenu.Add(new UIText("-Host Migration, Invite Links, LAN play".Padded(padLength), Color.White, UIAlign.Center, 0f - heightAdd)
        {
            scale = new Vec2(0.5f)
        });
        _duckGameUpdateMenu.Add(new UIText("-Major online synchronization improvements".Padded(padLength), Color.White, UIAlign.Center, 0f - heightAdd)
        {
            scale = new Vec2(0.5f)
        });
        _duckGameUpdateMenu.Add(new UIText("-Major performance improvements".Padded(padLength), Color.White, UIAlign.Center, 0f - heightAdd)
        {
            scale = new Vec2(0.5f)
        });
        _duckGameUpdateMenu.Add(new UIText("-Hundreds and hundreds of bug fixes".Padded(padLength), Color.White, UIAlign.Center, 0f - heightAdd)
        {
            scale = new Vec2(0.5f)
        });
        _duckGameUpdateMenu.Add(new UIText("", Color.White, UIAlign.Center, -3f)
        {
            scale = new Vec2(0.5f)
        });
        _duckGameUpdateMenu.Add(new UIText("Thank you for all your support!", Color.White, UIAlign.Center, -4f)
        {
            scale = new Vec2(0.5f)
        });
        _duckGameUpdateMenu.Add(new UIText("", Color.White, UIAlign.Center, -3f)
        {
            scale = new Vec2(0.5f)
        });
        _duckGameUpdateMenu.SetAcceptFunction(new UIMenuActionCloseMenu(_pauseGroup));
        _duckGameUpdateMenu.SetBackFunction(new UIMenuActionCloseMenu(_pauseGroup));
        _duckGameUpdateMenu.Close();
        _pauseGroup.Add(_duckGameUpdateMenu, doAnchor: false);
        _steamWarningMessage = new UIMenu("Steam Not Connected!", Layer.HUD.camera.width / 2f, Layer.HUD.camera.height / 2f, 220f, -1f, "@SELECT@ I see...");
        _steamWarningMessage.Add(new UIText("", Color.White, UIAlign.Center, -3f)
        {
            scale = new Vec2(0.5f)
        });
        _steamWarningMessage.Add(new UIText("It seems that either you're not logged in", Color.White, UIAlign.Center, -4f)
        {
            scale = new Vec2(0.5f)
        });
        _steamWarningMessage.Add(new UIText("to Steam, or Steam failed to authenticate.", Color.White, UIAlign.Center, -4f)
        {
            scale = new Vec2(0.5f)
        });
        _steamWarningMessage.Add(new UIText("", Color.White, UIAlign.Center, -3f)
        {
            scale = new Vec2(0.5f)
        });
        _steamWarningMessage.Add(new UIText("You can still play- but realtime", Color.White, UIAlign.Center, 0f - heightAdd)
        {
            scale = new Vec2(0.5f)
        });
        _steamWarningMessage.Add(new UIText("features like Online Play and the Workshop", Color.White, UIAlign.Center, 0f - heightAdd)
        {
            scale = new Vec2(0.5f)
        });
        _steamWarningMessage.Add(new UIText("will be |DGRED|unavailable|PREV|.", Color.White, UIAlign.Center, 0f - heightAdd)
        {
            scale = new Vec2(0.5f)
        });
        _steamWarningMessage.Add(new UIText("", Color.White, UIAlign.Center, -3f)
        {
            scale = new Vec2(0.5f)
        });
        _steamWarningMessage.SetAcceptFunction(new UIMenuActionCloseMenu(_pauseGroup));
        _steamWarningMessage.SetBackFunction(new UIMenuActionCloseMenu(_pauseGroup));
        _steamWarningMessage.Close();
        _pauseGroup.Add(_steamWarningMessage, doAnchor: false);
        _modsDisabledMenu = new UIMenu("MODS CHANGED!", Layer.HUD.camera.width / 2f, Layer.HUD.camera.height / 2f, 240f, -1f, "@SELECT@I see...");
        _modsDisabledMenu.Add(new UIText("", Color.White, UIAlign.Center, -3f)
        {
            scale = new Vec2(0.5f)
        });
        _modsDisabledMenu.Add(new UIText("To ensure a smooth update, all enabled", Color.White, UIAlign.Center, -4f)
        {
            scale = new Vec2(0.5f)
        });
        _modsDisabledMenu.Add(new UIText("mods have been temporarily set to |DGRED|disabled|PREV|.", Color.White, UIAlign.Center, -4f)
        {
            scale = new Vec2(0.5f)
        });
        _modsDisabledMenu.Add(new UIText("", Color.White, UIAlign.Center, -3f)
        {
            scale = new Vec2(0.5f)
        });
        _modsDisabledMenu.Add(new UIText("Mod compatibility has been a high priority, and", Color.White, UIAlign.Center, 0f - heightAdd)
        {
            scale = new Vec2(0.5f)
        });
        _modsDisabledMenu.Add(new UIText("most mods should work no problem with the new version.", Color.White, UIAlign.Center, 0f - heightAdd)
        {
            scale = new Vec2(0.5f)
        });
        _modsDisabledMenu.Add(new UIText("They can be re-enabled through the |DGORANGE|MANAGE MODS|PREV| menu", Color.White, UIAlign.Center, 0f - heightAdd)
        {
            scale = new Vec2(0.5f)
        });
        _modsDisabledMenu.Add(new UIText("accessible via the top left options console.", Color.White, UIAlign.Center, 0f - heightAdd)
        {
            scale = new Vec2(0.5f)
        });
        _modsDisabledMenu.Add(new UIText("", Color.White, UIAlign.Center, -3f)
        {
            scale = new Vec2(0.5f)
        });
        _modsDisabledMenu.Add(new UIText("Some older mods may |DGRED|not|PREV| work...", Color.White, UIAlign.Center, 0f - heightAdd)
        {
            scale = new Vec2(0.5f)
        });
        _modsDisabledMenu.Add(new UIText("", Color.White, UIAlign.Center, -3f)
        {
            scale = new Vec2(0.5f)
        });
        _modsDisabledMenu.Add(new UIText("Please be mindful of any crashes caused by", Color.White, UIAlign.Center, 0f - heightAdd)
        {
            scale = new Vec2(0.5f)
        });
        _modsDisabledMenu.Add(new UIText("re-enabling specific mods, and use the '|DGBLUE|-nomods|PREV|'", Color.White, UIAlign.Center, 0f - heightAdd)
        {
            scale = new Vec2(0.5f)
        });
        _modsDisabledMenu.Add(new UIText("launch option if you run into trouble!", Color.White, UIAlign.Center, 0f - heightAdd)
        {
            scale = new Vec2(0.5f)
        });
        _modsDisabledMenu.Add(new UIText("", Color.White, UIAlign.Center, -3f)
        {
            scale = new Vec2(0.5f)
        });
        _modsDisabledMenu.SetAcceptFunction(new UIMenuActionCloseMenu(_pauseGroup));
        _modsDisabledMenu.SetBackFunction(new UIMenuActionCloseMenu(_pauseGroup));
        _modsDisabledMenu.Close();
        _pauseGroup.Add(_modsDisabledMenu, doAnchor: false);
        UIDivider pauseBox = new UIDivider(vert: true, 0.8f);
        pauseBox.rightSection.Add(new UIImage("pauseIcons", UIAlign.Right));
        _mainPauseMenu.Add(pauseBox);
        pauseBox.leftSection.Add(new UIMenuItem("RESUME", new UIMenuActionCloseMenu(_pauseGroup)));
        pauseBox.leftSection.Add(new UIMenuItem("OPTIONS", new UIMenuActionOpenMenu(_mainPauseMenu, Options.optionsMenu), UIAlign.Left));
        pauseBox.leftSection.Add(new UIMenuItem("CREDITS", new UIMenuActionCloseMenuSetBoolean(_pauseGroup, _enterCreditsMenuBool), UIAlign.Left));
        pauseBox.leftSection.Add(new UIText("", Color.White));
        pauseBox.leftSection.Add(new UIMenuItem("|DGRED|QUIT", new UIMenuActionOpenMenu(_mainPauseMenu, _quitMenu)));
        Options.openOnClose = _mainPauseMenu;
        Options.AddMenus(_pauseGroup);
        _mainPauseMenu.Close();
        _pauseGroup.Add(_mainPauseMenu, doAnchor: false);
        _pauseGroup.Close();
        Level.Add(_pauseGroup);
        _font = new BitmapFont("biosFont", 8);
        _background = new Sprite("title/background");
        _optionsPlatform = new Sprite("title/optionsPlatform");
        _optionsPlatform.depth = 0.9f;
        _rightPlatform = new Sprite("title/rightPlatform");
        _rightPlatform.depth = 0.9f;
        _beamPlatform = new Sprite("title/beamPlatform");
        _beamPlatform.depth = 0.9f;
        _upperMonitor = new Sprite("title/upperMonitor");
        _upperMonitor.depth = 0.85f;
        _airlock = new Sprite("title/airlock");
        _airlock.depth = -0.85f;
        _leftPlatform = new Sprite("title/leftPlatform");
        _leftPlatform.depth = 0.9f;
        _optionsTV = new Sprite("title/optionsTV");
        _optionsTV.depth = -0.9f;
        _libraryBookcase = new Sprite("title/libraryBookcase");
        _libraryBookcase.depth = -0.9f;
        _editorBench = new Sprite("title/editorBench");
        _editorBench.depth = -0.9f;
        _editorBenchPaint = new Sprite("title/editorBenchPaint");
        _editorBenchPaint.depth = 0.9f;
        _bigUButton = new Sprite("title/bigUButtonPC");
        _bigUButton.CenterOrigin();
        _bigUButton.depth = 0.95f;
        _controls = new SpriteMap("title/controlsPC", 100, 11);
        _controls.CenterOrigin();
        _controls.depth = 0.95f;
        _multiBeam = new MultiBeam(160f, -30f);
        Level.Add(_multiBeam);
        _optionsBeam = new OptionsBeam(28f, -110f);
        Level.Add(_optionsBeam);
        _libraryBeam = new LibraryBeam(292f, -110f);
        Level.Add(_libraryBeam);
        _editorBeam = new EditorBeam(28f, 100f);
        Level.Add(_editorBeam);
        for (int j = 0; j < 21; j++)
        {
            SpaceTileset t = new SpaceTileset(j * 16 - 6, 176f);
            t.frame = 3;
            t.layer = Layer.Game;
            t.setLayer = false;
            AddThing(t);
        }
        new SpriteMap("duck", 32, 32);
        _space = new SpaceBackgroundMenu(-999f, -999f, moving: true, 0.6f);
        _space.update = false;
        Level.Add(_space);
        _things.RefreshState();
        Layer.Game.fade = 0f;
        Layer.Foreground.fade = 0f;
        Level.Add(new Block(120f, 155f, 80f, 30f, PhysicsMaterial.Metal));
        Level.Add(new Block(134f, 148f, 52f, 30f, PhysicsMaterial.Metal));
        Level.Add(new Block(0f, 61f, 63f, 70f, PhysicsMaterial.Metal));
        Level.Add(new Block(257f, 61f, 63f, 60f, PhysicsMaterial.Metal));
        Level.Add(new Spring(90f, 160f, 0.32f));
        Level.Add(new Spring(229f, 160f, 0.32f));
        Level.Add(new VerticalDoor(270f, 160f)
        {
            filterDefault = true
        });
        foreach (Team item in Teams.all)
        {
            int prevScoreboardScore = (item.score = 0);
            item.prevScoreboardScore = prevScoreboardScore;
        }
        foreach (Profile pro in Profiles.all)
        {
            if (pro.team != null)
            {
                pro.team.Leave(pro);
            }
            pro.inputProfile = null;
        }
        InputProfile.ReassignDefaultInputProfiles(fullRemap: true);
        if (_arcadeProfile == null)
        {
            TeamSelect2.ControllerLayoutsChanged();
            Teams.Player1.Join(Profiles.DefaultPlayer1);
            Teams.Player2.Join(Profiles.DefaultPlayer2);
            Teams.Player3.Join(Profiles.DefaultPlayer3);
            Teams.Player4.Join(Profiles.DefaultPlayer4);
        }
        else
        {
            Teams.Player1.Join(_arcadeProfile);
            _arcadeProfile.inputProfile = _arcadeInputProfile;
        }
        Input.lastActiveProfile = InputProfile.DefaultPlayer1;
        if (!DuckNetwork.ShowUserXPGain() && Unlockables.HasPendingUnlocks())
        {
            MonoMain.pauseMenu = new UIUnlockBox(Unlockables.GetPendingUnlocks().ToList(), Layer.HUD.camera.width / 2f, Layer.HUD.camera.height / 2f, 190f);
        }
        base.Initialize();
    }

    private void Join()
    {
    }

    private void OptionsSaveAndClose()
    {
        Options.Save();
        Options.SaveLocalData();
        _optionsGroup.Close();
    }

    public override void OnSessionEnded(DuckNetErrorInfo error)
    {
        Teams.Player1.Join(Profiles.DefaultPlayer1);
        Teams.Player2.Join(Profiles.DefaultPlayer2);
        Teams.Player3.Join(Profiles.DefaultPlayer3);
        Teams.Player4.Join(Profiles.DefaultPlayer4);
        InputProfile.ReassignDefaultInputProfiles();
    }

    public override void Update()
    {
        if (_duck != null && DGSave.showOnePointFiveMessages)
        {
            if (!showedNewVersionStartup && !DuckFile.freshInstall && DGSave.upgradingFromVanilla)
            {
                MonoMain.pauseMenu = _duckGameUpdateMenu;
                _duckGameUpdateMenu.Open();
                showedNewVersionStartup = true;
                return;
            }
            if (!showedModsDisabled && DGSave.showModsDisabledMessage)
            {
                MonoMain.pauseMenu = _modsDisabledMenu;
                _modsDisabledMenu.Open();
                showedModsDisabled = true;
                return;
            }
            DGSave.showOnePointFiveMessages = false;
        }
        int iter = 1;
        if (startStars)
        {
            iter = 250;
            startStars = false;
        }
        if (_duck != null && _duck.profile != null && !_duck.profile.inputProfile.HasAnyConnectedDevice())
        {
            foreach (InputProfile i in InputProfile.defaultProfiles)
            {
                if (i.HasAnyConnectedDevice())
                {
                    InputProfile.SwapDefaultInputStrings(i.name, _duck.profile.inputProfile.name);
                    InputProfile.ReassignDefaultInputProfiles();
                    _duck.profile.inputProfile = i;
                    break;
                }
            }
        }
        if (!_enterCredits && !_enterMultiplayer && _duck != null && _duck.inputProfile.Pressed("START"))
        {
            _pauseGroup.Open();
            _mainPauseMenu.Open();
            SFX.Play("pause", 0.6f);
            MonoMain.pauseMenu = _pauseGroup;
        }
        for (int j = 0; j < iter; j++)
        {
            starWait -= Maths.IncFrameTimer();
            if (starWait < 0f)
            {
                starWait = 0.1f + Rando.Float(0.2f);
                Color c = Colors.DGRed;
                if (cpick == 1)
                {
                    c = Colors.DGBlue;
                }
                else if (cpick == 2)
                {
                    c = Colors.DGGreen;
                }
                if (Rando.Float(1f) > 0.995f)
                {
                    c = Colors.DGPink;
                }
                particles.Add(new StarParticle
                {
                    pos = new Vec2(0f, (int)(Rando.Float(0f, 150f) / 1f)),
                    speed = new Vec2(Rando.Float(0.5f, 1f), 0f),
                    color = c,
                    flicker = Rando.Float(100f, 230f)
                });
                cpick++;
                if (cpick > 2)
                {
                    cpick = 0;
                }
            }
            List<StarParticle> remove = new List<StarParticle>();
            foreach (StarParticle p in particles)
            {
                p.pos += p.speed * (0.5f + (1f - extraFade) * 0.5f);
                if ((p.pos.x > 300f && !_enterCredits) || p.pos.x > 680f)
                {
                    remove.Add(p);
                }
            }
            foreach (StarParticle p2 in remove)
            {
                particles.Remove(p2);
            }
        }
        if (menuOpen)
        {
            Layer.Game.fade = Lerp.Float(Layer.Game.fade, 0.2f, 0.02f);
            Layer.Foreground.fade = Lerp.Float(Layer.Foreground.fade, 0.2f, 0.02f);
            Layer.Background.fade = Lerp.Float(Layer.Foreground.fade, 0.2f, 0.02f);
        }
        else
        {
            Layer.Game.fade = Lerp.Float(Layer.Game.fade, _fadeInFull ? 1f : (_fadeIn ? 0.5f : 0f), _fadeInFull ? 0.01f : 0.006f);
            Layer.Foreground.fade = Lerp.Float(Layer.Foreground.fade, _fadeIn ? 1f : 0f, 0.01f);
            Layer.Background.fade = Lerp.Float(Layer.Background.fade, _fadeBackground ? 0f : 1f, 0.02f);
        }
        if (_enterArcade)
        {
            _duck.x += 1f;
            _duck.immobilized = true;
            _duck.enablePhysics = false;
            Graphics.fade = Lerp.Float(Graphics.fade, 0f, 0.05f);
            if (Graphics.fade < 0.01f)
            {
                Level.current.Clear();
                Level.current = new ArcadeLevel(Content.GetLevelID("arcade"));
            }
        }
        else
        {
            if (_enterCredits)
            {
                _duck.immobilized = true;
                _duck.updatePhysics = false;
                if (base.camera.x < 140f)
                {
                    flashDissipationSpeed = 0.08f;
                    Graphics.flashAdd = 2f;
                    base.camera.x += 330f;
                    foreach (StarParticle particle in particles)
                    {
                        particle.pos.x += 320f;
                    }
                }
                else
                {
                    switchWait -= 0.04f;
                    if (switchWait <= 0f)
                    {
                        if (!_startedMusic)
                        {
                            Music.volumeMult = Lerp.Float(Music.volumeMult, 0f, 0.006f);
                        }
                        if (Layer.Parallax.camera.y > -12f)
                        {
                            base.camera.y += 0.064f;
                            Layer.Parallax.camera.y -= 0.08f;
                        }
                        else
                        {
                            if (!_startedMusic)
                            {
                                Music.volumeMult = 1.2f;
                                Music.Play("tabledoodles", looping: false);
                                _startedMusic = true;
                            }
                            if (creditsScroll > 939f)
                            {
                                if (Layer.Parallax.camera.y > -22f)
                                {
                                    base.camera.y += 0.064f;
                                    Layer.Parallax.camera.y -= 0.08f;
                                }
                                extraFade -= 0.01f;
                                if (extraFade < 0f)
                                {
                                    extraFade = 0f;
                                }
                            }
                            if (creditsScroll > 2650f && !shownPrompt)
                            {
                                shownPrompt = true;
                                HUD.AddCornerControl(HUDCorner.BottomLeft, "@CANCEL@Exit");
                            }
                            creditsScroll += 0.25f;
                        }
                    }
                }
                if (_duck.inputProfile.Pressed("CANCEL"))
                {
                    _enterCredits = false;
                    quittingCredits = true;
                }
                return;
            }
            if (_duck != null)
            {
                _duck.updatePhysics = true;
                if (_duck.x > 324f)
                {
                    _enterArcade = true;
                }
            }
            if (quittingCredits)
            {
                flashDissipationSpeed = 0.08f;
                Graphics.flashAdd = 2f;
                base.camera.x = 0f;
                foreach (StarParticle particle2 in particles)
                {
                    particle2.pos.x -= 320f;
                }
                base.camera.y = 0f;
                Layer.Parallax.camera.y = 0f;
                creditsScroll = 0f;
                extraFade = 1f;
                _startedMusic = false;
                starWait = 0f;
                switchWait = 1f;
                creditsScroll = 0f;
                startStars = true;
                quittingCredits = false;
                shownPrompt = false;
                HUD.CloseAllCorners();
                _duck.immobilized = false;
                Music.Play("Title");
            }
        }
        Music.volumeMult = 1f;
        _hasMenusOpen = menuOpen;
        if (!_enterMultiplayer && !_enterEditor && !_enterLibrary && !_enterBuyScreen)
        {
            if (Graphics.fade < 1f)
            {
                Graphics.fade += 0.001f;
            }
            else
            {
                Graphics.fade = 1f;
            }
        }
        else
        {
            Graphics.fade -= 0.05f;
            if (Graphics.fade <= 0f)
            {
                Graphics.fade = 0f;
                Music.Stop();
                if (_enterMultiplayer)
                {
                    foreach (Team item in Teams.all)
                    {
                        item.ClearProfiles();
                    }
                    Level.current = new TeamSelect2();
                }
                else if (_enterEditor)
                {
                    Level.current = Main.editor;
                }
                else if (_enterLibrary)
                {
                    Level.current = new DoorRoom();
                }
                else if (_enterBuyScreen)
                {
                    Level.current = new BuyScreen(Main.currencyType, Main.price);
                }
            }
        }
        _pressStartBlink += 0.01f;
        if (_pressStartBlink > 1f)
        {
            _pressStartBlink -= 1f;
        }
        if (_duck != null)
        {
            if (_dontQuit.value)
            {
                _dontQuit.value = false;
                _duck.hSpeed = 10f;
            }
            if (_quit.value)
            {
                MonoMain.exit = true;
                return;
            }
            if (InputProfile.active.Pressed("START") && Main.foundPurchaseInfo && Main.isDemo)
            {
                _enterBuyScreen = true;
                _duck.immobilized = true;
            }
        }
        if (_enterCreditsMenuBool.value)
        {
            _enterCreditsMenuBool.value = false;
            _enterCredits = true;
            _duck.immobilized = true;
        }
        if (_multiBeam.entered)
        {
            _selectionTextDesired = "MULTIPLAYER";
            _desiredSelection = TitleMenuSelection.Play;
            if (!_enterMultiplayer && _duck.inputProfile.Pressed("SELECT") && MonoMain.pauseMenu == null)
            {
                SFX.Play("plasmaFire");
                _enterMultiplayer = true;
                _duck.immobilized = true;
            }
        }
        else if (_optionsBeam.entered)
        {
            _selectionTextDesired = "OPTIONS";
            _desiredSelection = TitleMenuSelection.Options;
            if (!Options.menuOpen && _duck.inputProfile.Pressed("SELECT"))
            {
                SFX.Play("plasmaFire");
                _optionsGroup.Open();
                _optionsMenu.Open();
                MonoMain.pauseMenu = _optionsGroup;
                _duck.immobilized = true;
            }
        }
        else if (_libraryBeam.entered)
        {
            _selectionTextDesired = "LIBRARY";
            _desiredSelection = TitleMenuSelection.Stats;
            if (_duck.inputProfile.Pressed("SELECT") && Profiles.allCustomProfiles.Count > 0 && MonoMain.pauseMenu == null)
            {
                SFX.Play("plasmaFire");
                _enterLibrary = true;
                _duck.immobilized = true;
            }
        }
        else if (_editorBeam.entered)
        {
            _selectionTextDesired = "LEVEL EDITOR";
            _desiredSelection = TitleMenuSelection.Editor;
            if (_duck.inputProfile.Pressed("SELECT") && MonoMain.pauseMenu == null)
            {
                SFX.Play("plasmaFire");
                _enterEditor = true;
                _duck.immobilized = true;
            }
        }
        else
        {
            _selectionTextDesired = " ";
            _desiredSelection = TitleMenuSelection.None;
        }
        if (_selectionTextDesired != " ")
        {
            _controlsFrameDesired = 1;
        }
        else
        {
            _controlsFrameDesired = 2;
        }
        if (_selectionText != _selectionTextDesired)
        {
            _selectionFade -= 0.1f;
            if (_selectionFade <= 0f)
            {
                _selectionFade = 0f;
                _selectionText = _selectionTextDesired;
                _selection = _desiredSelection;
            }
        }
        else
        {
            _selectionFade = Lerp.Float(_selectionFade, 1f, 0.1f);
        }
        if (_controlsFrame != _controlsFrameDesired)
        {
            _controlsFade -= 0.1f;
            if (_controlsFade <= 0f)
            {
                _controlsFade = 0f;
                _controlsFrame = _controlsFrameDesired;
            }
        }
        else
        {
            _controlsFade = Lerp.Float(_controlsFade, 1f, 0.1f);
        }
        if (_returnFromArcade)
        {
            if (!_fadeIn)
            {
                _fadeIn = true;
                _title = new BigTitle();
                _title.x = Layer.HUD.camera.width / 2f - (float)(_title.graphic.w / 2) + 3f;
                _title.y = Layer.HUD.camera.height / 2f;
                AddThing(_title);
                _title.Fade = true;
                _title.alpha = 0f;
                Layer.Game.fade = 1f;
                Layer.Foreground.fade = 1f;
                Layer.Background.fade = 1f;
                _arcadeProfile.inputProfile = _arcadeInputProfile;
                _duck = new Duck(310f, 160f, _arcadeProfile);
                _duck.offDir = -1;
                InputProfile.active = _duck.profile.inputProfile;
            }
            Graphics.fade = Lerp.Float(Graphics.fade, 1f, 0.05f);
            if (Graphics.fade > 0.99f)
            {
                Graphics.fade = 1f;
                _returnFromArcade = false;
            }
        }
        if (_fadeIn && !_fadeInFull)
        {
            if (!_returnFromArcade)
            {
                _duck = null;
            }
            if (firstStart && ParentalControls.AreParentalControlsActive())
            {
                MonoMain.pauseMenu = _parentalControlsMenu;
                _parentalControlsMenu.Open();
                firstStart = false;
            }
            foreach (Profile p3 in Profiles.defaultProfiles)
            {
                if (!p3.inputProfile.JoinGamePressed())
                {
                    continue;
                }
                Join();
                foreach (Profile item2 in Profiles.all)
                {
                    item2.team = null;
                }
                p3.ApplyDefaults();
                _duck = new Duck(160f, 60f, p3);
                if (SFX.NoSoundcard)
                {
                    HUD.AddInputChangeDisplay("@UNPLUG@|RED|No Soundcard Detected!!");
                }
            }
            if (_duck != null)
            {
                if (Main.foundPurchaseInfo && Main.isDemo)
                {
                    HUD.AddCornerControl(HUDCorner.TopRight, "@START@BUY GAME", _duck.inputProfile);
                }
                InputProfile.active = _duck.profile.inputProfile;
                _fadeInFull = true;
                _title.Fade = true;
                Level.Add(_duck);
            }
        }
        _space.parallax.y = -80f;
        moveWait -= 0.02f;
        if (moveWait < 0f)
        {
            if (_title == null)
            {
                _title = new BigTitle();
                _title.x = Layer.HUD.camera.width / 2f - (float)(_title.graphic.w / 2) + 3f;
                _title.y = Layer.HUD.camera.height / 2f;
                AddThing(_title);
            }
            moveSpeed = Maths.LerpTowards(moveSpeed, 0f, 0.0015f);
        }
        if (_title == null)
        {
            return;
        }
        wait++;
        _ = wait;
        _ = 30;
        if (wait == 60)
        {
            flash = 1f;
        }
        if (wait == 60)
        {
            _title.graphic.color = Color.White;
            _title.alpha = 1f;
            _fadeIn = true;
        }
        if (flash > 0f)
        {
            flash -= 0.016f;
            dim -= 0.08f;
            if (dim < 0f)
            {
                dim = 0f;
            }
        }
        else
        {
            flash = 0f;
        }
    }

    public override void PostDrawLayer(Layer layer)
    {
        if (layer == Layer.Foreground)
        {
            Graphics.Draw(_upperMonitor, 84f, 0f);
            if (_fadeInFull)
            {
                _font.alpha = _selectionFade;
                _font.inputProfile = _duck.inputProfile;
                if (_selection == TitleMenuSelection.None)
                {
                    string controlText = "@WASD@MOVE @JUMP@JUMP";
                    _font.Draw(controlText, Level.current.camera.PercentW(50f) - _font.GetWidth(controlText) / 2f, 16f, Color.White, 0.95f);
                }
                else if (_selection == TitleMenuSelection.Play)
                {
                    string text = "@SELECT@PLAY GAME";
                    _font.Draw(text, Level.current.camera.PercentW(50f) - _font.GetWidth(text) / 2f, 16f, Color.White, 0.95f);
                }
                else if (_selection == TitleMenuSelection.Stats)
                {
                    if (Profiles.allCustomProfiles.Count > 0)
                    {
                        string text2 = "@SELECT@LIBRARY";
                        _font.Draw(text2, Level.current.camera.PercentW(50f) - _font.GetWidth(text2) / 2f, 16f, Color.White, 0.95f);
                    }
                }
                else if (_selection == TitleMenuSelection.Options)
                {
                    string text3 = "@SELECT@OPTIONS";
                    _font.Draw(text3, Level.current.camera.PercentW(50f) - _font.GetWidth(text3) / 2f, 16f, Color.White, 0.95f);
                }
                else if (_selection == TitleMenuSelection.Editor)
                {
                    string text4 = "@SELECT@EDITOR";
                    _font.Draw(text4, Level.current.camera.PercentW(50f) - _font.GetWidth(text4) / 2f, 16f, Color.White, 0.95f);
                }
                Graphics.Draw(_editorBenchPaint, 45f, 168f);
            }
            else
            {
                string titleString = "PRESS START";
                if (_pressStartBlink >= 0.5f)
                {
                    _font.Draw(titleString, Level.current.camera.PercentW(50f) - _font.GetWidth(titleString) / 2f, 15f, Color.White, 0.95f);
                }
                else
                {
                    InputProfile prof = InputProfile.FirstProfileWithDevice;
                    if (prof != null && prof.lastActiveDevice != null && prof.lastActiveDevice is GenericController)
                    {
                        Graphics.Draw(_bigUButton, Level.current.camera.PercentW(50f) - 1f, 18f);
                    }
                    else
                    {
                        Graphics.DrawString("@START@", new Vec2(Level.current.camera.PercentW(50f) - 7f, 16f), Color.White, 0.9f, prof);
                    }
                }
            }
        }
        else if (layer == Layer.Game)
        {
            Graphics.Draw(_leftPlatform, 0f, 61f);
            Graphics.Draw(_airlock, 266f, 135f);
            Graphics.Draw(_rightPlatform, 255f, 61f);
            Graphics.Draw(_beamPlatform, 118f, 146f);
            Graphics.Draw(_optionsTV, 0f, 19f);
            Graphics.Draw(_libraryBookcase, 263f, 12f);
            Graphics.Draw(_editorBench, 1f, 130f);
            if (creditsScroll > 0.1f)
            {
                Graphics.caseSensitiveStringDrawing = true;
                float yDraw = 0f;
                foreach (List<string> line in creditsRoll)
                {
                    float lineStart = yDraw + (200f - creditsScroll);
                    if (lineStart >= -11f && lineStart < 200f)
                    {
                        if (line.Count == 1)
                        {
                            float wide = Graphics.GetStringWidth(line[0]);
                            Graphics.DrawStringColoredSymbols(line[0], new Vec2(490f - wide / 2f, yDraw + (200f - creditsScroll)), Color.White, 1f);
                        }
                        else
                        {
                            Graphics.GetStringWidth(line[0]);
                            Graphics.DrawStringColoredSymbols(line[0], new Vec2(347f, yDraw + (200f - creditsScroll)), Color.White, 1f);
                            Graphics.GetStringWidth(line[1]);
                            Graphics.DrawStringColoredSymbols(line[1], new Vec2(507f, yDraw + (200f - creditsScroll)), Color.White, 1f);
                        }
                    }
                    yDraw += 11f;
                }
                Graphics.caseSensitiveStringDrawing = false;
            }
        }
        else if (layer == Layer.Parallax)
        {
            float backFade = 0f;
            if (base.camera.y > 4f)
            {
                backFade += (base.camera.y - 4f) / 13f;
                _starField.alpha = backFade - extraFade * 0.7f;
                Graphics.Draw(_starField, 0f, -58f + layer.camera.y, -0.99f);
            }
        }
        else if (layer == Layer.Background)
        {
            foreach (StarParticle p in particles)
            {
                float flickerDist = Math.Max(1f - Math.Min(Math.Abs(p.pos.x - p.flicker) / 10f, 1f), 0f);
                float starFade = 0.2f;
                if (base.camera.y > 0f)
                {
                    starFade += base.camera.y / 52f;
                }
                Graphics.DrawRect(p.pos, p.pos + new Vec2(1f, 1f), Color.White * ((starFade + flickerDist * 0.6f) * (0.3f + (1f - extraFade) * 0.7f)), -0.3f);
                float trailFade = 0.1f;
                if (base.camera.y > 0f)
                {
                    trailFade += base.camera.y / 52f;
                }
                Vec2 trailPos = p.pos;
                int trailNum = 4;
                for (int i = 0; i < trailNum; i++)
                {
                    float move = p.speed.x * 8f;
                    Graphics.DrawLine(trailPos + new Vec2(0f - move, 0.5f), trailPos + new Vec2(0f, 0.5f), p.color * ((1f - (float)i / (float)trailNum) * trailFade) * (0.3f + (1f - extraFade) * 0.7f), 1f, -0.4f);
                    trailPos.x -= move;
                }
            }
            _background.depth = 0f;
            Graphics.Draw(sourceRectangle: new Rectangle(0f, 0f, 90f, _background.height), g: _background, x: 0f, y: 0f);
            Rectangle area = new Rectangle(63f, 107f, 194f, 61f);
            Graphics.Draw(_background, area.x, area.y, area);
            area = new Rectangle(230f, 61f, 28f, 61f);
            Graphics.Draw(_background, area.x, area.y, area);
            area = new Rectangle(230f, 0f, 90f, 61f);
            Graphics.Draw(_background, area.x, area.y, area);
            area = new Rectangle(230f, 124f, 90f, 56f);
            Graphics.Draw(_background, area.x, area.y, area);
            area = new Rectangle(90f, 0f, 140f, 50f);
            Graphics.Draw(_background, area.x, area.y, area);
        }
        base.PostDrawLayer(layer);
    }
}
