namespace DuckGame;

[EditorGroup("Stuff|Wires")]
[BaggedProperty("isOnlineCapable", true)]
public class WireFlipper : Block, IWirePeripheral
{
    private SpriteMap _sprite;

    public WireFlipper(float xpos, float ypos)
        : base(xpos, ypos)
    {
        _sprite = new SpriteMap("wireFlipper", 16, 16);
        graphic = _sprite;
        center = new Vec2(8f, 8f);
        collisionOffset = new Vec2(-8f, -8f);
        collisionSize = new Vec2(16f, 16f);
        base.depth = -0.5f;
        _editorName = "Wire Flipper";
        editorTooltip = "Alternates the direction a current will pass through when a connected Button is pressed.";
        thickness = 4f;
        physicsMaterial = PhysicsMaterial.Metal;
        base.layer = Layer.Foreground;
        _canFlip = true;
    }

    public override void Initialize()
    {
        base.Initialize();
    }

    public override void Draw()
    {
        bool fl = flipHorizontal;
        if (offDir < 0)
        {
            _sprite.frame = 1;
        }
        else
        {
            _sprite.frame = 0;
        }
        flipHorizontal = false;
        base.Draw();
        flipHorizontal = fl;
    }

    public override void Update()
    {
        base.Update();
    }

    public override void Terminate()
    {
        base.Terminate();
    }

    public void Pulse(int type, WireTileset wire)
    {
        SFX.Play("click");
        if (flipHorizontal)
        {
            wire.dullSignalLeft = true;
        }
        else
        {
            wire.dullSignalRight = true;
        }
        flipHorizontal = !flipHorizontal;
    }
}
