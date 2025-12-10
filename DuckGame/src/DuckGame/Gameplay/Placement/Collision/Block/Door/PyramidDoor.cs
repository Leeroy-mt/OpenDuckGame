namespace DuckGame;

[EditorGroup("Stuff|Pyramid", EditorItemType.Pyramid)]
public class PyramidDoor : VerticalDoor, IPlatform
{
    public PyramidDoor(float xpos, float ypos)
        : base(xpos, ypos)
    {
        _sprite = new SpriteMap("pyramidDoor", 16, 32);
        graphic = _sprite;
        center = new Vec2(8f, 24f);
        collisionSize = new Vec2(10f, 32f);
        collisionOffset = new Vec2(-5f, -24f);
        base.depth = -0.5f;
        _editorName = "Pyramid Door";
        thickness = 3f;
        physicsMaterial = PhysicsMaterial.Metal;
        _bottom = new Sprite("pyramidDoorBottom");
        _bottom.CenterOrigin();
        _top = new Sprite("pyramidDoorTop");
        _top.CenterOrigin();
    }

    public override void Update()
    {
        if (!_cornerInit)
        {
            _topLeft = base.topLeft;
            _topRight = base.topRight;
            _bottomLeft = base.bottomLeft;
            _bottomRight = base.bottomRight;
            _cornerInit = true;
        }
        if (Level.CheckRect<Duck>(_topLeft - new Vec2(18f, 0f), _bottomRight + new Vec2(18f, 0f)) != null)
        {
            _desiredOpen = 1f;
        }
        else if (Level.CheckRectFilter(new Vec2(base.x - 4f, base.y - 24f), new Vec2(base.x + 4f, base.y + 8f), (PhysicsObject d) => !(d is TeamHat)) == null)
        {
            _desiredOpen = 0f;
        }
        if (_desiredOpen > 0.5f && !_opened)
        {
            _opened = true;
            SFX.Play("pyramidOpen", 0.6f);
        }
        if (_desiredOpen < 0.5f && _opened)
        {
            _opened = false;
            SFX.Play("pyramidClose", 0.6f);
        }
        _open = Maths.LerpTowards(_open, _desiredOpen, 0.15f);
        _sprite.frame = (int)(_open * 32f);
        _collisionSize.y = (1f - _open) * 32f;
    }
}
