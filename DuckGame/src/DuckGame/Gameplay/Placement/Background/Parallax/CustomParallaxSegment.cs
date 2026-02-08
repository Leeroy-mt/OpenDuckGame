using Microsoft.Xna.Framework;

namespace DuckGame;

[EditorGroup("Background|Parallax|custom", EditorItemType.Custom)]
public class CustomParallaxSegment : Thing
{
    public EditorProperty<int> ystart = new EditorProperty<int>(0, null, 0f, 40f, 1f);

    public EditorProperty<int> yend = new EditorProperty<int>(0, null, 0f, 40f, 1f);

    public EditorProperty<float> speed = new EditorProperty<float>(0.5f, null, 0f, 2f);

    public EditorProperty<float> distance = new EditorProperty<float>(0f, null, 0f, 1f, 0.05f);

    public EditorProperty<bool> moving = new EditorProperty<bool>(val: false);

    private bool initializedParallax;

    public CustomParallaxSegment(float xpos, float ypos)
        : base(xpos, ypos)
    {
        graphic = new SpriteMap("backgroundIcons", 16, 16)
        {
            frame = 6
        };
        Center = new Vector2(8f, 8f);
        _collisionSize = new Vector2(16f, 16f);
        _collisionOffset = new Vector2(-8f, -8f);
        base.Depth = 0.9f;
        base.layer = Layer.Foreground;
        _visibleInGame = false;
        _editorName = "Parallax Segment";
        _canFlip = false;
        _canHaveChance = false;
    }

    public override void Update()
    {
        if (!initializedParallax)
        {
            CustomParallax u = Level.current.FirstOfType<CustomParallax>();
            if (u != null)
            {
                if (!u.didInit)
                {
                    u.DoInitialize();
                }
                if (u.parallax != null)
                {
                    for (int i = ystart; i <= (int)yend; i++)
                    {
                        u.parallax.AddZone(i, distance.value, speed.value, moving.value);
                    }
                }
            }
            initializedParallax = true;
        }
        base.Initialize();
    }
}
