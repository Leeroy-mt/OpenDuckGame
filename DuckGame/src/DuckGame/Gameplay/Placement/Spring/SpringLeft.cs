using Microsoft.Xna.Framework;

namespace DuckGame;

[EditorGroup("Stuff|Springs")]
[BaggedProperty("isInDemo", false)]
[BaggedProperty("previewPriority", false)]
public class SpringLeft : Spring
{
    public override bool flipHorizontal
    {
        get
        {
            return _flipHorizontal;
        }
        set
        {
            _flipHorizontal = value;
            offDir = (sbyte)((!_flipHorizontal) ? 1 : (-1));
            if (!_flipHorizontal)
            {
                Center = new Vector2(8f, 7f);
                collisionOffset = new Vector2(0f, -8f);
                collisionSize = new Vector2(8f, 16f);
                base.AngleDegrees = -90f;
                base.hugWalls = WallHug.Right;
            }
            else
            {
                Center = new Vector2(8f, 7f);
                collisionOffset = new Vector2(-8f, -8f);
                collisionSize = new Vector2(8f, 16f);
                base.AngleDegrees = 90f;
                base.hugWalls = WallHug.Left;
            }
        }
    }

    public SpringLeft(float xpos, float ypos)
        : base(xpos, ypos)
    {
        UpdateSprite();
        Center = new Vector2(8f, 7f);
        collisionOffset = new Vector2(0f, -8f);
        collisionSize = new Vector2(8f, 16f);
        base.Depth = -0.5f;
        _editorName = "Spring Left";
        editorTooltip = "Can't reach a high platform or want to get somewhere fast? That's why we built springs.";
        physicsMaterial = PhysicsMaterial.Metal;
        editorCycleType = typeof(SpringUpLeft);
        base.AngleDegrees = -90f;
        base.hugWalls = WallHug.Right;
    }

    public override void Touch(MaterialThing with)
    {
        if (with.isServerForObject && with.Sprung(this))
        {
            if (!_flipHorizontal)
            {
                if (with.hSpeed > -12f * _mult)
                {
                    with.hSpeed = -12f * _mult;
                }
            }
            else if (with.hSpeed < 12f * _mult)
            {
                with.hSpeed = 12f * _mult;
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

    public override void Draw()
    {
        base.Draw();
    }
}
