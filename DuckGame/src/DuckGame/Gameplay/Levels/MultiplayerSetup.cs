namespace DuckGame;

public class MultiplayerSetup : Level
{
    public int roundsPerSet = 8;

    public int setsPerGame = 3;

    public override void Initialize()
    {
        base.camera.x = 480f;
        UIMenu multiplayerMenu = new UIMenu("MULTIPLAYER", (float)Graphics.width / 2f, (float)Graphics.height / 2f, 160f);
        multiplayerMenu.Scale = new Vec2(4f);
        multiplayerMenu.Add(new UIMenuItemNumber("ROUNDS PER SET", null, new FieldBinding(this, "roundsPerSet", 0f, 50f)));
        multiplayerMenu.Add(new UIMenuItemNumber("SETS PER GAME", null, new FieldBinding(this, "setsPerGame", 0f, 50f)));
        multiplayerMenu.Add(new UIText(" ", Color.White));
        multiplayerMenu.Add(new UIMenuItem("START", new UIMenuActionChangeLevel(multiplayerMenu, new TeamSelect2())));
        multiplayerMenu.Add(new UIMenuItem("BACK", new UIMenuActionChangeLevel(multiplayerMenu, new TitleScreen())));
        Level.Add(multiplayerMenu);
        base.Initialize();
    }

    public override void Update()
    {
    }
}
