using Microsoft.Xna.Framework;
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
        Center = new Vector2(8f, 8f);
        collisionOffset = new Vector2(-5f, -5f);
        collisionSize = new Vector2(11f, 12f);
        _offset = new Vector2(-3f, 3f);
        _equippedDepth = -15;
        _jumpMod = true;
        thickness = 0.1f;
        _wearOffset = new Vector2(-2f, 0f);
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
            _offset = new Vector2(-3f, 3f);
            Angle = 0f;
            if (_equippedDuck.sliding && _equippedDuck._trapped == null)
            {
                if (_equippedDuck.offDir > 0)
                {
                    Angle = -(float)Math.PI / 2f;
                }
                else
                {
                    Angle = (float)Math.PI / 2f;
                }
                _offset.Y += 12f;
                smokeOff -= 6f;
            }
            if (_equippedDuck.crouch && !_equippedDuck.sliding)
            {
                _offset.Y += 4f;
            }
            collisionOffset = new Vector2(0f, -9999f);
            collisionSize = new Vector2(0f, 0f);
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
                    float realAngle = Angle;
                    Angle = propel.Angle;
                    Vector2 offset = Offset(new Vector2(0f, 8f));
                    Level.Add(new JetpackSmoke(offset.X, offset.Y));
                    Angle = realAngle;
                    if (propel.velocity.Length() < 7f)
                    {
                        RagdollPart part = propel as RagdollPart;
                        part.addWeight = 0.2f;
                        _equippedDuck.ragdoll.jetting = true;
                        float ang = 0 - (propel.Angle - float.Pi / 2f);
                        var dir = Vector2.Zero;
                        if (_equippedDuck.inputProfile.leftStick.Length() > 0.1f)
                            dir = new Vector2(_equippedDuck.inputProfile.leftStick.X, 0f - _equippedDuck.inputProfile.leftStick.Y);
                        else
                        {
                            dir = Vector2.Zero;
                            if (_equippedDuck.inputProfile.Down("LEFT"))
                                dir.X -= 1f;
                            if (_equippedDuck.inputProfile.Down("RIGHT"))
                                dir.X += 1f;
                            if (_equippedDuck.inputProfile.Down("UP"))
                                dir.Y -= 1f;
                            if (_equippedDuck.inputProfile.Down("DOWN"))
                                dir.Y += 1f;
                        }
                        if (dir.Length() < 0.1f)
                            dir = new Vector2(float.Cos(ang), float.Sin(ang));
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
                    for (int i = 0; i < 5; i++)
                    {
                        Level.Add(new JetpackSmoke(base.X, base.Y + 8f + smokeOff));
                    }
                    if (Angle > 0f)
                    {
                        if (propel.hSpeed < 6f)
                        {
                            propel.hSpeed += 0.9f;
                        }
                    }
                    else if (Angle < 0f)
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
            collisionOffset = new Vector2(-5f, -5f);
            collisionSize = new Vector2(11f, 12f);
            solid = true;
        }
    }
}
