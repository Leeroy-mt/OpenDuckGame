namespace DuckGame;

[EditorGroup("Equipment")]
[BaggedProperty("isInDemo", true)]
public class PowerHolster : Holster
{
    public StateBinding _triggerBinding = new StateBinding("trigger");

    public bool trigger;

    public PowerHolster(float pX, float pY)
        : base(pX, pY)
    {
        _sprite = new SpriteMap("powerHolster", 14, 12);
        _overPart = new SpriteMap("powerHolsterOver", 10, 3);
        _overPart.center = new Vec2(6f, -1f);
        _underPart = new SpriteMap("powerHolsterUnder", 11, 10);
        _underPart.center = new Vec2(10f, 8f);
        graphic = _sprite;
        collisionOffset = new Vec2(-5f, -5f);
        collisionSize = new Vec2(10f, 10f);
        center = new Vec2(6f, 6f);
        physicsMaterial = PhysicsMaterial.Wood;
        _equippedDepth = 4;
        _wearOffset = new Vec2(1f, 1f);
        editorTooltip = "Lets you carry around an additional item!";
        backOffset = -10f;
    }

    public override void Update()
    {
        if (base.isServerForObject)
        {
            if (_equippedDuck != null && _equippedDuck.inputProfile != null)
            {
                trigger = _equippedDuck.inputProfile.Down("QUACK");
            }
            if (base.containedObject != null && (!trigger || !base.containedObject.HolsterActivate(this)))
            {
                base.containedObject.triggerAction = trigger;
                if (base.containedObject is PowerHolster)
                {
                    (base.containedObject as PowerHolster).trigger = trigger;
                }
            }
            if (owner == null)
            {
                trigger = false;
            }
        }
        base.Update();
    }

    protected override void DrawParts()
    {
        if (_equippedDuck != null)
        {
            Thing ownerDepthObject = owner;
            if (_equippedDuck != null && _equippedDuck._trapped != null)
            {
                ownerDepthObject = _equippedDuck._trapped;
            }
            _overPart.flipH = owner.offDir <= 0;
            _overPart.angle = angle;
            _overPart.alpha = base.alpha;
            _overPart.scale = base.scale;
            _overPart.depth = ownerDepthObject.depth + 5;
            _overPart.frame = ((_equippedDuck.quack > 0) ? 1 : 0);
            Graphics.Draw(_overPart, base.x, base.y);
            _underPart.flipH = owner.offDir <= 0;
            _underPart.angle = angle;
            _underPart.alpha = base.alpha;
            _underPart.scale = base.scale;
            if (_equippedDuck.ragdoll != null && _equippedDuck.ragdoll.part2 != null)
            {
                _underPart.depth = _equippedDuck.ragdoll.part2.depth + -11;
            }
            else
            {
                _underPart.depth = ownerDepthObject.depth + -7;
            }
            _underPart.frame = (trigger ? 1 : 0);
            Vec2 pos2 = Offset(new Vec2(-2f, 0f));
            Graphics.Draw(_underPart, pos2.x, pos2.y);
        }
        else
        {
            _sprite.frame = (trigger ? 1 : 0);
        }
    }
}
