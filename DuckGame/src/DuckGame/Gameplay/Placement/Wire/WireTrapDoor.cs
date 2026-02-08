using Microsoft.Xna.Framework;

namespace DuckGame;

[EditorGroup("Stuff|Wires")]
[BaggedProperty("isOnlineCapable", true)]
public class WireTrapDoor : Block, IWirePeripheral
{
    public StateBinding _openBinding = new StateBinding(nameof(_open));

    public bool _open;

    private Thing _shutter;

    private SpriteMap _sprite;

    public EditorProperty<int> length;

    public EditorProperty<int> color;

    public EditorProperty<bool> open;

    private bool _lastFlip;

    public EditorProperty<bool> fallthrough;

    private bool _lastFallthrough = true;

    public bool newTrapdoorVersion = true;

    public override bool flipHorizontal
    {
        get
        {
            return base.flipHorizontal;
        }
        set
        {
            base.flipHorizontal = value;
            if (flipHorizontal)
            {
                offDir = -1;
            }
            else
            {
                offDir = 1;
            }
            if (_initialized)
            {
                CreateShutter();
            }
        }
    }

    public override void EditorPropertyChanged(object property)
    {
        if (_initialized)
        {
            _open = open.value;
            UpdateShutter();
            newTrapdoorVersion = true;
        }
    }

    private void UpdateShutter()
    {
        if (_lastFallthrough != fallthrough.value)
        {
            Level.Remove(_shutter);
            _shutter = null;
        }
        bool makingShutter = false;
        if (_shutter == null)
        {
            makingShutter = true;
            CreateShutter();
        }
        _lastFallthrough = fallthrough.value;
        if (makingShutter || Level.current is Editor)
        {
            if (_open)
            {
                _shutter.AngleDegrees = 90f * (float)offDir;
            }
            else
            {
                _shutter.AngleDegrees = 0f;
            }
        }
        else if (_open)
        {
            _shutter.AngleDegrees = Lerp.Float(_shutter.AngleDegrees, 90f * (float)offDir, 10f);
        }
        else
        {
            _shutter.AngleDegrees = Lerp.Float(_shutter.AngleDegrees, 0f, 10f);
        }
        (_shutter as IShutter).UpdateSprite();
    }

    public WireTrapDoor(float xpos, float ypos)
        : base(xpos, ypos)
    {
        fallthrough = new EditorProperty<bool>(val: true, this);
        length = new EditorProperty<int>(2, this, 1f, 4f, 1f);
        color = new EditorProperty<int>(2, this, 0f, 3f, 1f);
        open = new EditorProperty<bool>(val: false, this);
        _sprite = new SpriteMap("wireBlock", 16, 16);
        graphic = _sprite;
        Center = new Vector2(8f, 8f);
        collisionOffset = new Vector2(-8f, -8f);
        collisionSize = new Vector2(16f, 16f);
        base.Depth = -0.5f;
        _editorName = "Wire Trapdoor";
        editorTooltip = "Opens and closes when a connected Button is pressed.";
        thickness = 4f;
        physicsMaterial = PhysicsMaterial.Metal;
        base.layer = Layer.Foreground;
        _canFlip = true;
    }

    public override BinaryClassChunk Serialize()
    {
        BinaryClassChunk binaryClassChunk = base.Serialize();
        binaryClassChunk.AddProperty("newTrapdoorVersion", newTrapdoorVersion);
        return binaryClassChunk;
    }

    public override bool Deserialize(BinaryClassChunk node)
    {
        base.Deserialize(node);
        newTrapdoorVersion = node.GetProperty<bool>("newTrapdoorVersion");
        if (!newTrapdoorVersion)
        {
            length.value = 2;
            if ((bool)fallthrough)
            {
                color.value = 2;
            }
            else
            {
                color.value = 3;
            }
            newTrapdoorVersion = true;
        }
        return true;
    }

    public void CreateShutter()
    {
        if (_shutter != null)
        {
            Level.Remove(_shutter);
        }
        if (fallthrough.value)
        {
            _shutter = new WireTrapDoorShutter(base.X + (float)(4 * offDir), base.Y - 5f, this);
        }
        else
        {
            _shutter = new WireTrapDoorShutterSolid(base.X + (float)(4 * offDir), base.Y - 5f, this);
        }
        _shutter.Depth = base.Depth + 5;
        _shutter.offDir = offDir;
        Level.Add(_shutter);
    }

    public override void Initialize()
    {
        _open = open.value;
        CreateShutter();
        (_shutter as IShutter).UpdateSprite();
        base.Initialize();
    }

    public override void Update()
    {
        UpdateShutter();
        base.Update();
    }

    public override void Terminate()
    {
        Level.Remove(_shutter);
        base.Terminate();
    }

    public void Pulse(int type, WireTileset wire)
    {
        Thing.Fondle(this, DuckNetwork.localConnection);
        _open = !_open;
        SFX.Play("click");
    }
}
