using Microsoft.Xna.Framework;

namespace DuckGame;

[EditorGroup("Equipment")]
[BaggedProperty("isInDemo", true)]
public class PowerHolster : Holster
{
    public StateBinding _triggerBinding = new StateBinding(nameof(trigger));

    public bool trigger;

    public PowerHolster(float pX, float pY)
        : base(pX, pY)
    {
        _sprite = new SpriteMap("powerHolster", 14, 12);
        _overPart = new SpriteMap("powerHolsterOver", 10, 3);
        _overPart.Center = new Vector2(6f, -1f);
        _underPart = new SpriteMap("powerHolsterUnder", 11, 10);
        _underPart.Center = new Vector2(10f, 8f);
        graphic = _sprite;
        collisionOffset = new Vector2(-5f, -5f);
        collisionSize = new Vector2(10f, 10f);
        Center = new Vector2(6f, 6f);
        physicsMaterial = PhysicsMaterial.Wood;
        _equippedDepth = 4;
        _wearOffset = new Vector2(1f, 1f);
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
            _overPart.Angle = Angle;
            _overPart.Alpha = base.Alpha;
            _overPart.Scale = base.Scale;
            _overPart.Depth = ownerDepthObject.Depth + 5;
            _overPart.frame = ((_equippedDuck.quack > 0) ? 1 : 0);
            Graphics.Draw(_overPart, base.X, base.Y);
            _underPart.flipH = owner.offDir <= 0;
            _underPart.Angle = Angle;
            _underPart.Alpha = base.Alpha;
            _underPart.Scale = base.Scale;
            if (_equippedDuck.ragdoll != null && _equippedDuck.ragdoll.part2 != null)
            {
                _underPart.Depth = _equippedDuck.ragdoll.part2.Depth + -11;
            }
            else
            {
                _underPart.Depth = ownerDepthObject.Depth + -7;
            }
            _underPart.frame = (trigger ? 1 : 0);
            Vector2 pos2 = Offset(new Vector2(-2f, 0f));
            Graphics.Draw(_underPart, pos2.X, pos2.Y);
        }
        else
        {
            _sprite.frame = (trigger ? 1 : 0);
        }
    }
}
