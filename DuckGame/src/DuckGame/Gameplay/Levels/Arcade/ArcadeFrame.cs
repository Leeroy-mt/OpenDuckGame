using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace DuckGame;

[EditorGroup("Details|Arcade", EditorItemType.Arcade)]
[BaggedProperty("isOnlineCapable", false)]
public class ArcadeFrame : Thing
{
    private SpriteMap _frame;

    public EditorProperty<int> style = new EditorProperty<int>(0, null, 0f, 5f, 1f);

    public EditorProperty<float> respect = new EditorProperty<float>(0f, null, 0f, 1f, 0.05f);

    public EditorProperty<string> requirement = new EditorProperty<string>("");

    private Sprite _image;

    private ChallengeSaveData _saveData;

    public Sprite _screen;

    public string _identifier;

    public ChallengeSaveData saveData
    {
        get
        {
            return _saveData;
        }
        set
        {
            if (_saveData != null && _saveData != value)
            {
                _saveData.frameID = "";
                _saveData.frameImage = "";
            }
            _saveData = value;
            if (_saveData != null)
            {
                Texture2D image = Editor.StringToTexture(_saveData.frameImage);
                if (image != null)
                {
                    _image = new Sprite(image);
                }
                _saveData.frameID = _identifier;
            }
        }
    }

    public ArcadeFrame(float xpos, float ypos)
        : base(xpos, ypos)
    {
        _frame = new SpriteMap("arcadeFrame01", 48, 48);
        _frame.imageIndex = 0;
        graphic = _frame;
        Center = new Vector2(graphic.width / 2, graphic.height / 2);
        _collisionSize = new Vector2(16f, 16f);
        _collisionOffset = new Vector2(-8f, -8f);
        _screen = new Sprite("shot01");
        base.Depth = -0.9f;
    }

    public Vector2 GetRenderTargetSize()
    {
        if ((int)style == 0)
        {
            return new Vector2(38f, 28f);
        }
        if ((int)style == 1)
        {
            return new Vector2(28f, 38f);
        }
        if ((int)style == 2)
        {
            return new Vector2(28f, 20f);
        }
        if ((int)style == 3)
        {
            return new Vector2(20f, 28f);
        }
        if ((int)style == 4)
        {
            return new Vector2(18f, 12f);
        }
        if ((int)style == 5)
        {
            return new Vector2(12f, 16f);
        }
        return new Vector2(32f, 32f);
    }

    public float GetRenderTargetZoom()
    {
        return 1f;
    }

    public override void Initialize()
    {
        if (_identifier == null)
        {
            _identifier = Guid.NewGuid().ToString();
        }
        base.Initialize();
    }

    public override BinaryClassChunk Serialize()
    {
        BinaryClassChunk el = base.Serialize();
        if (Editor.copying)
        {
            el.AddProperty("FrameID", Guid.NewGuid().ToString());
        }
        else
        {
            el.AddProperty("FrameID", _identifier);
        }
        return el;
    }

    public override bool Deserialize(BinaryClassChunk node)
    {
        _identifier = node.GetProperty<string>("FrameID");
        return base.Deserialize(node);
    }

    public override DXMLNode LegacySerialize()
    {
        DXMLNode el = base.LegacySerialize();
        if (Editor.copying)
        {
            el.Add(new DXMLNode("FrameID", Guid.NewGuid().ToString()));
        }
        else
        {
            el.Add(new DXMLNode("FrameID", _identifier));
        }
        return el;
    }

    public override bool LegacyDeserialize(DXMLNode node)
    {
        DXMLNode el = node.Element("FrameID");
        if (el != null)
        {
            _identifier = el.Value;
        }
        return base.LegacyDeserialize(node);
    }

    public override void Update()
    {
        visible = saveData != null && _image != null;
        _ = visible;
        base.Update();
    }

    public override void Draw()
    {
        _frame.frame = style.value;
        if (_image != null)
        {
            Vector2 frameSize = GetRenderTargetSize();
            _image.Depth = base.Depth + 10;
            _image.Scale = new Vector2(1f / 6f);
            Graphics.doSnap = false;
            Graphics.Draw(_image, base.X - frameSize.X / 2f, base.Y - frameSize.Y / 2f);
            Graphics.doSnap = true;
        }
        base.Draw();
    }
}
