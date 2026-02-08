using Microsoft.Xna.Framework;

namespace DuckGame;

[EditorGroup("Background", EditorItemType.Pyramid)]
public class BackgroundPyramid : BackgroundTile
{
    private bool inited;

    public BackgroundPyramid(float xpos, float ypos)
        : base(xpos, ypos)
    {
        graphic = new SpriteMap("pyramidBackground", 16, 16, calculateTransparency: true);
        Center = new Vector2(8f, 8f);
        collisionSize = new Vector2(16f, 16f);
        collisionOffset = new Vector2(-8f, -8f);
        _editorName = "Pyramid";
    }

    public override void Initialize()
    {
        base.Initialize();
    }

    public override void Update()
    {
        if (!inited)
        {
            inited = true;
            SpriteMap s = graphic as SpriteMap;
            if (!flipHorizontal && s.frame % 8 == 0)
            {
                if (Level.CheckPoint<BackgroundPyramid>(Position + new Vector2(-16f, 0f)) != null)
                {
                    s.frame++;
                    s.UpdateFrame();
                }
            }
            else if (!flipHorizontal && s.frame % 8 == 7)
            {
                if (Level.CheckPoint<BackgroundPyramid>(Position + new Vector2(16f, 0f)) != null)
                {
                    s.frame--;
                    s.UpdateFrame();
                }
            }
            else if (flipHorizontal && s.frame % 8 == 0)
            {
                if (Level.CheckPoint<BackgroundPyramid>(Position + new Vector2(16f, 0f)) != null)
                {
                    s.frame++;
                    s.UpdateFrame();
                }
            }
            else if (flipHorizontal && s.frame % 8 == 7 && Level.CheckPoint<BackgroundPyramid>(Position + new Vector2(-16f, 0f)) != null)
            {
                s.frame--;
                s.UpdateFrame();
            }
        }
        base.Update();
    }
}
