using Microsoft.Xna.Framework;

namespace DuckGame;

public class FurniShopScreen : Thing
{
    public static bool close;

    private Sprite _tail;

    public bool quitOut;

    private PlasmaLayer _plasma;

    private Layer _treeLayer;

    private static bool _prevOpen;

    public static bool open;

    public static VincentProduct attemptBuy;

    public static bool giveYoYo;

    public static bool giveVooDoo;

    public static bool givePerimeterDefence;

    public static int attemptBuyIndex;

    private UIComponent _pauseGroup;

    private UIMenu _confirmMenu;

    private MenuBoolean _confirm = new MenuBoolean();

    private UnlockData _tryBuy;

    public override bool visible
    {
        get
        {
            if (!(base.Alpha < 0.01f))
            {
                return base.visible;
            }
            return false;
        }
        set
        {
            base.visible = value;
        }
    }

    public FurniShopScreen()
    {
        _tail = new Sprite("arcade/bubbleTail");
        base.layer = Layer.HUD;
    }

    public override void Initialize()
    {
        _plasma = new PlasmaLayer("PLASMA", -85);
        Layer.Add(_plasma);
        Camera treeCam = new Camera();
        _treeLayer = new Layer("TREE", -95, treeCam);
        Layer.Add(_treeLayer);
    }

    public void OpenBuyConfirmation(UnlockData unlock)
    {
        if (_pauseGroup != null)
        {
            Level.Remove(_pauseGroup);
            _pauseGroup = null;
        }
        _confirm.value = false;
        _pauseGroup = new UIComponent(Layer.HUD.camera.width / 2f, Layer.HUD.camera.height / 2f, 0f, 0f);
        _confirmMenu = new UIMenu("UNLOCK FEATURE", Layer.HUD.camera.width / 2f, Layer.HUD.camera.height / 2f, 230f, -1f, "@CANCEL@CANCEL  @SELECT@BUY");
        _confirmMenu.Add(new UIText(unlock.name, Color.Green));
        _confirmMenu.Add(new UIText(" ", Color.White));
        float width = 190f;
        string text = unlock.longDescription;
        string curText = "";
        string nextWord = "";
        while (true)
        {
            if (text.Length > 0 && text[0] != ' ')
            {
                nextWord += text[0];
            }
            else
            {
                if ((float)((curText.Length + nextWord.Length) * 8) > width)
                {
                    _confirmMenu.Add(new UIText(curText, Color.White, UIAlign.Left));
                    curText = "";
                }
                if (curText.Length > 0)
                {
                    curText += " ";
                }
                curText += nextWord;
                nextWord = "";
            }
            if (text.Length == 0)
            {
                break;
            }
            text = text.Remove(0, 1);
        }
        if (nextWord.Length > 0)
        {
            if (curText.Length > 0)
            {
                curText += " ";
            }
            curText += nextWord;
        }
        if (curText.Length > 0)
        {
            _confirmMenu.Add(new UIText(curText, Color.White, UIAlign.Left));
            curText = "";
        }
        _confirmMenu.Add(new UIText(" ", Color.White));
        _confirmMenu.Add(new UIMenuItem("CANCEL", new UIMenuActionCloseMenu(_pauseGroup), UIAlign.Center, Colors.MenuOption, backButton: true));
        _confirmMenu.Add(new UIMenuItem("BUY UNLOCK |WHITE|(|LIME|" + unlock.cost + "|WHITE| TICKETS)", new UIMenuActionCloseMenuSetBoolean(_pauseGroup, _confirm)));
        _confirmMenu.Close();
        _pauseGroup.Add(_confirmMenu, doAnchor: false);
        _pauseGroup.Close();
        Level.Add(_pauseGroup);
        for (int i = 0; i < 10; i++)
        {
            _pauseGroup.Update();
            _confirmMenu.Update();
        }
        _pauseGroup.Open();
        _confirmMenu.Open();
        MonoMain.pauseMenu = _pauseGroup;
        SFX.Play("pause", 0.6f);
        _tryBuy = unlock;
    }

    public void ChangeSpeech()
    {
        Chancy.Clear();
    }

    public void MakeActive()
    {
        HUD.AddCornerCounter(HUDCorner.TopRight, "@TICKET@ ", new FieldBinding(Profiles.active[0], "ticketCount"), 0, animateCount: true);
        HUD.AddCornerControl(HUDCorner.BottomLeft, "@CANCEL@BACK");
    }

    public override void Update()
    {
    }

    public override void Draw()
    {
        if (!(base.Alpha < 0.01f))
        {
            Graphics.DrawRect(new Vector2(26f, 22f), new Vector2(Layer.HUD.width - 105f, Layer.HUD.height - 51f), new Color(20, 20, 20) * base.Alpha * 0.7f, -0.9f);
            Vector2 namePos = new Vector2(20f, 8f);
            Vector2 nameSize = new Vector2(226f, 11f);
            Graphics.DrawRect(namePos, namePos + nameSize, Color.Black, 0.96f);
            string name = "what a name";
            Graphics.DrawString(name, namePos + new Vector2((nameSize.X - 27f) / 2f - Graphics.GetStringWidth(name) / 2f, 2f), new Color(163, 206, 39) * base.Alpha, 0.97f);
            _tail.Depth = 0.5f;
            _tail.Alpha = base.Alpha;
            _tail.flipH = false;
            _tail.flipV = false;
            Graphics.Draw(_tail, 222f, 18f);
            Chancy.alpha = base.Alpha;
            Chancy.Draw();
        }
    }
}
