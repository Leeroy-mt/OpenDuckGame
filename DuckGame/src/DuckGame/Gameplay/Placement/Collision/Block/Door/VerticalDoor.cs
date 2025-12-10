namespace DuckGame;

[EditorGroup("Stuff|Doors")]
public class VerticalDoor : Block, IPlatform
{
    protected SpriteMap _sprite;

    protected SpriteMap _sensorSprite;

    protected SpriteMap _noSensorSprite;

    protected Sprite _bottom;

    protected Sprite _top;

    public float _open;

    protected float _desiredOpen;

    protected bool _opened;

    protected Vec2 _topLeft;

    protected Vec2 _topRight;

    protected Vec2 _bottomLeft;

    protected Vec2 _bottomRight;

    protected bool _cornerInit;

    public bool filterDefault;

    public bool slideLocked;

    public bool slideLockOpened;

    public bool stuck;

    private bool showedWarning;

    public VerticalDoor(float xpos, float ypos)
        : base(xpos, ypos)
    {
        _sensorSprite = (_sprite = new SpriteMap("verticalDoor", 16, 32));
        graphic = _sprite;
        center = new Vec2(8f, 24f);
        collisionSize = new Vec2(6f, 32f);
        collisionOffset = new Vec2(-3f, -24f);
        base.depth = -0.5f;
        _editorName = "Vertical Door";
        thickness = 3f;
        physicsMaterial = PhysicsMaterial.Metal;
        _bottom = new Sprite("verticalDoorBottom");
        _bottom.CenterOrigin();
        _top = new Sprite("verticalDoorTop");
        _top.CenterOrigin();
        editorTooltip = "One of them science fiction type doors.";
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
        if (!slideLocked)
        {
            _sprite = _sensorSprite;
            Duck hit = Level.CheckRect<Duck>(_topLeft - new Vec2(18f, 0f), _bottomRight + new Vec2(18f, 0f));
            if (hit != null)
            {
                if (!filterDefault || !Profiles.IsDefault(hit.profile))
                {
                    _desiredOpen = 1f;
                }
                else if (!showedWarning)
                {
                    HUD.AddPlayerChangeDisplay("@UNPLUG@|GRAY|NO ARCADE (SELECT A PROFILE)");
                    showedWarning = true;
                }
            }
            else if (Level.CheckRectFilter(new Vec2(base.x - 4f, base.y - 24f), new Vec2(base.x + 4f, base.y + 8f), (PhysicsObject d) => !(d is TeamHat)) == null)
            {
                _desiredOpen = 0f;
            }
        }
        else
        {
            if (_noSensorSprite == null)
            {
                _noSensorSprite = new SpriteMap("verticalDoorNoSensor", 16, 32);
            }
            _sprite = _noSensorSprite;
            _desiredOpen = (slideLockOpened ? 1f : 0f);
            if (Level.CheckRectFilter(new Vec2(base.x - 4f, base.y - 24f), new Vec2(base.x + 4f, base.y + 8f), (PhysicsObject d) => !(d is TeamHat)) != null && _opened)
            {
                _desiredOpen = 1f;
            }
        }
        if (_desiredOpen > 0.5f && !_opened)
        {
            _opened = true;
            SFX.Play("slideDoorOpen", 0.6f);
        }
        if (_desiredOpen < 0.5f && _opened)
        {
            _opened = false;
            SFX.Play("slideDoorClose", 0.6f);
        }
        graphic = _sprite;
        _open = Maths.LerpTowards(_open, _desiredOpen, 0.15f);
        _sprite.frame = (int)(_open * 32f);
        _collisionSize.y = (1f - _open) * 32f;
    }

    public override void Draw()
    {
        base.Draw();
        _top.depth = base.depth + 1;
        _bottom.depth = base.depth + 1;
        Graphics.Draw(_top, base.x, base.y - 27f);
        Graphics.Draw(_bottom, base.x, base.y + 5f);
    }
}
