namespace DuckGame;

[EditorGroup("Stuff|Springs")]
[BaggedProperty("previewPriority", false)]
public class SpringUpRight : Spring
{
    public SpringUpRight(float xpos, float ypos)
        : base(xpos, ypos)
    {
        UpdateSprite();
        Center = new Vec2(8f, 7f);
        collisionOffset = new Vec2(-8f, 0f);
        collisionSize = new Vec2(16f, 8f);
        base.Depth = -0.5f;
        _editorName = "Spring UpRight";
        editorTooltip = "Can't reach a high platform or want to get somewhere fast? That's why we built springs.";
        physicsMaterial = PhysicsMaterial.Metal;
        editorCycleType = typeof(SpringRight);
        base.AngleDegrees = 45f;
    }

    protected override void UpdateSprite()
    {
        if (purple)
        {
            _sprite = new SpriteMap("springAnglePurple", 16, 20);
            _sprite.ClearAnimations();
            _sprite.AddAnimation("idle", 1f, false, default(int));
            _sprite.AddAnimation("spring", 4f, false, 1, 2, 1, 0);
            _sprite.SetAnimation("idle");
            _sprite.speed = 0.1f;
            graphic = _sprite;
        }
        else
        {
            _sprite = new SpriteMap("springAngle", 16, 20);
            _sprite.ClearAnimations();
            _sprite.AddAnimation("idle", 1f, false, default(int));
            _sprite.AddAnimation("spring", 4f, false, 1, 2, 1, 0);
            _sprite.SetAnimation("idle");
            _sprite.speed = 0.1f;
            graphic = _sprite;
        }
    }

    public override void Touch(MaterialThing with)
    {
        if (with.isServerForObject && with.Sprung(this))
        {
            if (with.vSpeed > -22f * _mult)
            {
                with.vSpeed = -22f * _mult;
            }
            if (flipHorizontal)
            {
                if (purple)
                {
                    if (with.hSpeed > -7f)
                    {
                        with.hSpeed = -7f;
                    }
                }
                else if (with.hSpeed > -10f)
                {
                    with.hSpeed = -10f;
                }
            }
            else if (purple)
            {
                if (with.hSpeed < 7f)
                {
                    with.hSpeed = 7f;
                }
            }
            else if (with.hSpeed < 10f)
            {
                with.hSpeed = 10f;
            }
            if (with is Gun)
            {
                (with as Gun).PressAction();
            }
            if (with is Duck)
            {
                (with as Duck).jumping = false;
                DoRumble(with as Duck);
            }
            with.lastHSpeed = with._hSpeed;
            with.lastVSpeed = with._vSpeed;
        }
        SpringUp();
    }

    public override void UpdateAngle()
    {
        if (flipHorizontal)
        {
            base.AngleDegrees = -45f;
        }
        else
        {
            base.AngleDegrees = 45f;
        }
    }

    public override void Draw()
    {
        base.Draw();
    }
}
