using System.Linq;

namespace DuckGame;

public class ConnectionError : Level, IConnectionScreen
{
    private string _text;

    public static Lobby joinLobby;

    private UIMenu _downloadModsMenu;

    public ConnectionError(string text)
    {
        _text = text;
        _centeredView = true;
        if (_text == "CRASH")
        {
            _text = "The Host Crashed!";
        }
        else if (_text == "CLOSED")
        {
            _text = "Host Closed Duck Game!";
        }
    }

    public override void Initialize()
    {
        DuckNetwork.ClosePauseMenu();
        ConnectionStatusUI.Hide();
        if (joinLobby != null)
        {
            string loadedMods = joinLobby.GetLobbyData("mods");
            if (loadedMods != null && loadedMods != "" && loadedMods.Split('|').Contains("LOCAL"))
            {
                _text = "Host has non-workshop mods enabled!";
            }
            else if (_text == "INCOMPATIBLE MOD SETUP!" || _text == "Host has different Mods enabled!")
            {
                _downloadModsMenu = new UIMenu("MODS REQUIRED!", Layer.HUD.camera.width / 2f, Layer.HUD.camera.height / 2f, 290f, -1f, "@SELECT@SELECT");
                _downloadModsMenu.Add(new UIText("You're missing the mods required", Colors.DGBlue));
                _downloadModsMenu.Add(new UIText("to join this game. Would you", Colors.DGBlue));
                _downloadModsMenu.Add(new UIText("like to automatically subscribe to", Colors.DGBlue));
                _downloadModsMenu.Add(new UIText("all required mods, restart and", Colors.DGBlue));
                _downloadModsMenu.Add(new UIText("join the game?", Colors.DGBlue));
                _downloadModsMenu.Add(new UIText("", Colors.DGBlue));
                _downloadModsMenu.Add(new UIMenuItem("NO!", new UIMenuActionCloseMenu(_downloadModsMenu)));
                _downloadModsMenu.Add(new UIMenuItem("YES!", new UIMenuActionCloseMenuCallFunction(_downloadModsMenu, UIServerBrowser.SubscribeAndRestart)));
                _downloadModsMenu.Close();
                _downloadModsMenu.Open();
                MonoMain.pauseMenu = _downloadModsMenu;
            }
        }
        Level.core.gameFinished = true;
        _startCalled = true;
        HUD.AddCornerMessage(HUDCorner.BottomRight, "@START@CONTINUE");
        base.Initialize();
    }

    public override void Update()
    {
        if ((_downloadModsMenu == null || !_downloadModsMenu.open) && Input.Pressed("START"))
        {
            Level.current = new TitleScreen();
            joinLobby = null;
        }
        base.Update();
    }

    public override void Draw()
    {
        string[] array = _text.Split('{');
        float xoff = -(array.Count() - 1) * 8;
        string[] array2 = array;
        foreach (string text in array2)
        {
            xoff = Graphics.GetStringHeight(text);
            Graphics.DrawString(text, new Vec2(Layer.HUD.camera.width / 2f - Graphics.GetStringWidth(text) / 2f, Layer.HUD.camera.height / 2f - xoff / 2f), Color.White);
            xoff += 8f;
        }
    }
}
