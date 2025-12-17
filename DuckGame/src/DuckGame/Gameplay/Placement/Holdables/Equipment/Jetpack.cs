using System;

namespace DuckGame;

[EditorGroup("Equipment")]
[BaggedProperty("previewPriority", true)]
public class Jetpack : Equipment
{
    public StateBinding _onBinding = new StateBinding(nameof(_on));

    public StateBinding _heatBinding = new StateBinding(nameof(_heat));

    protected SpriteMap _sprite;

    public bool _on;

    public float _heat;

    public Jetpack(float xpos, float ypos)
        : base(xpos, ypos)
    {
        _sprite = new SpriteMap("jetpack", 16, 16);
        graphic = _sprite;
        center = new Vec2(8f, 8f);
        collisionOffset = new Vec2(-5f, -5f);
        collisionSize = new Vec2(11f, 12f);
        _offset = new Vec2(-3f, 3f);
        _equippedDepth = -15;
        _jumpMod = true;
        thickness = 0.1f;
        _wearOffset = new Vec2(-2f, 0f);
        editorTooltip = "Allows you to fly like some kind of soaring bird.";
    }

    public override void OnPressAction()
    {
        _on = true;
    }

    public override void OnReleaseAction()
    {
        _on = false;
    }

    public override void Update()
    {
        base.Update();
        _sprite.frame = (int)(_heat * 7f);
        if (_equippedDuck != null)
        {
            float smokeOff = 0f;
            _offset = new Vec2(-3f, 3f);
            angle = 0f;
            if (_equippedDuck.sliding && _equippedDuck._trapped == null)
            {
                if (_equippedDuck.offDir > 0)
                {
                    angle = -(float)Math.PI / 2f;
                }
                else
                {
                    angle = (float)Math.PI / 2f;
                }
                _offset.y += 12f;
                smokeOff -= 6f;
            }
            if (_equippedDuck.crouch && !_equippedDuck.sliding)
            {
                _offset.y += 4f;
            }
            collisionOffset = new Vec2(0f, -9999f);
            collisionSize = new Vec2(0f, 0f);
            solid = false;
            PhysicsObject propel = _equippedDuck;
            if (_equippedDuck._trapped != null)
            {
                propel = _equippedDuck._trapped;
            }
            else if (_equippedDuck.ragdoll != null && _equippedDuck.ragdoll.part1 != null)
            {
                propel = _equippedDuck.ragdoll.part1;
            }
            _sprite.flipH = _equippedDuck._sprite.flipH;
            if (_on && _heat < 1f)
            {
                if (_equippedDuck._trapped == null && _equippedDuck.crouch)
                {
                    _equippedDuck.sliding = true;
                }
                if (base.isServerForObject)
                {
                    Global.data.jetFuelUsed.valueFloat += Maths.IncFrameTimer();
                }
                _heat += 0.011f;
                if (propel is RagdollPart)
                {
                    Global.data.timeJetpackedAsRagdoll++;
                    float realAngle = angle;
                    angle = propel.angle;
                    Vec2 offset = Offset(new Vec2(0f, 8f));
                    Level.Add(new JetpackSmoke(offset.x, offset.y));
                    angle = realAngle;
                    if (propel.velocity.length < 7f)
                    {
                        RagdollPart part = propel as RagdollPart;
                        part.addWeight = 0.2f;
                        _equippedDuck.ragdoll.jetting = true;
                        float ang = 0f - (propel.angle - (float)Math.PI / 2f);
                        Vec2 dir = Vec2.Zero;
                        if (_equippedDuck.inputProfile.leftStick.length > 0.1f)
                        {
                            dir = new Vec2(_equippedDuck.inputProfile.leftStick.x, 0f - _equippedDuck.inputProfile.leftStick.y);
                        }
                        else
                        {
                            dir = new Vec2(0f, 0f);
                            if (_equippedDuck.inputProfile.Down("LEFT"))
                            {
                                dir.x -= 1f;
                            }
                            if (_equippedDuck.inputProfile.Down("RIGHT"))
                            {
                                dir.x += 1f;
                            }
                            if (_equippedDuck.inputProfile.Down("UP"))
                            {
                                dir.y -= 1f;
                            }
                            if (_equippedDuck.inputProfile.Down("DOWN"))
                            {
                                dir.y += 1f;
                            }
                        }
                        if (dir.length < 0.1f)
                        {
                            dir = new Vec2((float)Math.Cos(ang), (float)(0.0 - Math.Sin(ang)));
                        }
                        propel.velocity += dir * 1.5f;
                        if (part.doll != null && part.doll.part1 != null && part.doll.part2 != null && part.doll.part3 != null)
                        {
                            part.doll.part1.extraGravMultiplier = 0.4f;
                            part.doll.part2.extraGravMultiplier = 0.4f;
                            part.doll.part3.extraGravMultiplier = 0.7f;
                        }
                    }
                }
                else
                {
                    Level.Add(new JetpackSmoke(base.x, base.y + 8f + smokeOff));
                    if (angle > 0f)
                    {
                        if (propel.hSpeed < 6f)
                        {
                            propel.hSpeed += 0.9f;
                        }
                    }
                    else if (angle < 0f)
                    {
                        if (propel.hSpeed > -6f)
                        {
                            propel.hSpeed -= 0.9f;
                        }
                    }
                    else if (propel.vSpeed > -4.5f)
                    {
                        propel.vSpeed -= 0.38f;
                    }
                }
            }
            if (_heat >= 1f)
            {
                _on = false;
            }
            if (propel.grounded)
            {
                if (_heat > 0f)
                {
                    _heat -= 0.25f;
                }
                else
                {
                    _heat = 0f;
                }
            }
        }
        else
        {
            _sprite.flipH = false;
            collisionOffset = new Vec2(-5f, -5f);
            collisionSize = new Vec2(11f, 12f);
            solid = true;
        }
    }
}
