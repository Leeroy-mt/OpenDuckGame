using Microsoft.Xna.Framework;

namespace DuckGame;

public class UploadDialogue : ContextMenu
{
    private new string _text = "";

    private BitmapFont _font;

    private bool _hoverOk;

    private WorkshopItem _item;

    private int _uploadIndex;

    public UploadDialogue()
        : base(null)
    {
    }

    public override void Initialize()
    {
        base.layer = Layer.HUD;
        base.Depth = 0.95f;
        float windowWidth = 300f;
        float windowHeight = 40f;
        Vector2 topLeft = new Vector2(base.layer.width / 2f - windowWidth / 2f, base.layer.height / 2f - windowHeight / 2f);
        new Vector2(base.layer.width / 2f + windowWidth / 2f, base.layer.height / 2f + windowHeight / 2f);
        Position = topLeft + new Vector2(4f, 20f);
        itemSize = new Vector2(490f, 16f);
        _root = true;
        _font = new BitmapFont("biosFont", 8);
    }

    public void Open(string text, WorkshopItem pItem)
    {
        base.opened = true;
        _text = text;
        SFX.Play("openClick", 0.4f);
        _item = pItem;
        _uploadIndex = 0;
    }

    public void Close()
    {
        base.opened = false;
    }

    public override void Selected(ContextMenu item)
    {
    }

    public override void Update()
    {
        if (base.opened)
        {
            if (_opening)
            {
                _opening = false;
                _selectedIndex = 1;
            }
            float windowWidth = 300f;
            float windowHeight = 80f;
            Vector2 vec = new Vector2(base.layer.width / 2f - windowWidth / 2f, base.layer.height / 2f - windowHeight / 2f);
            new Vector2(base.layer.width / 2f + windowWidth / 2f, base.layer.height / 2f + windowHeight / 2f);
            Vector2 okPos = vec + new Vector2(18f, 28f);
            Vector2 okSize = new Vector2(120f, 40f);
            _ = vec + new Vector2(160f, 28f);
            new Vector2(120f, 40f);
            if (Mouse.x > okPos.X && Mouse.x < okPos.X + okSize.X && Mouse.y > okPos.Y && Mouse.y < okPos.Y + okSize.Y)
            {
                _hoverOk = true;
            }
            else
            {
                _hoverOk = false;
            }
            if (!Editor.tookInput && Input.Pressed("MENULEFT"))
            {
                _selectedIndex--;
            }
            else if (!Editor.tookInput && Input.Pressed("MENURIGHT"))
            {
                _selectedIndex++;
            }
            if (_selectedIndex < 0)
            {
                _selectedIndex = 0;
            }
            if (_selectedIndex > 1)
            {
                _selectedIndex = 1;
            }
        }
    }

    public override void Draw()
    {
        if (base.opened)
        {
            base.Draw();
            float windowWidth = 300f;
            float windowHeight = 60f;
            Vector2 topLeft = new Vector2(base.layer.width / 2f - windowWidth / 2f, base.layer.height / 2f - windowHeight / 2f);
            Vector2 bottomRight = new Vector2(base.layer.width / 2f + windowWidth / 2f, base.layer.height / 2f + windowHeight / 2f);
            Graphics.DrawRect(topLeft, bottomRight, new Color(70, 70, 70), base.Depth, filled: false);
            Graphics.DrawRect(topLeft, bottomRight, new Color(30, 30, 30), base.Depth - 1);
            Graphics.DrawRect(topLeft + new Vector2(4f, 20f), bottomRight + new Vector2(-4f, -4f), new Color(10, 10, 10), base.Depth + 1);
            Graphics.DrawRect(topLeft + new Vector2(2f, 2f), new Vector2(bottomRight.X - 2f, topLeft.Y + 16f), new Color(70, 70, 70), base.Depth + 1);
            Graphics.DrawString(_text, topLeft + new Vector2(5f, 5f), Color.White, base.Depth + 2);
            _font.Scale = new Vector2(1f, 1f);
            Vector2 barPos = topLeft + new Vector2(14f, 38f);
            Vector2 barSize = new Vector2(270f, 16f);
            TransferProgress p = _item.GetUploadProgress();
            float progress = (float)p.bytesDownloaded / (float)p.bytesTotal;
            Graphics.DrawRect(barPos, barPos + barSize * new Vector2(progress, 1f), _hoverOk ? new Color(80, 80, 80) : new Color(30, 30, 30), base.Depth + 2);
            if (p.bytesTotal == 0L)
            {
                _font.Draw("Waiting...", barPos.X, barPos.Y - 12f, Color.White, base.Depth + 3);
                return;
            }
            _font.Draw("Uploading " + p.bytesDownloaded + "/" + p.bytesTotal + "B", barPos.X, barPos.Y - 12f, Color.White, base.Depth + 3);
        }
    }
}
