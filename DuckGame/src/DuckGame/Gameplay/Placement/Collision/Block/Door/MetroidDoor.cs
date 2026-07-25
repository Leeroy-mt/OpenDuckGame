using System.Collections.Generic;
using System.Linq;
using XnaRenderTarget2D = Microsoft.Xna.Framework.Graphics.RenderTarget2D;

namespace DuckGame;

[EditorGroup("Stuff|Doors", EditorItemType.Debug)]
public class MetroidDoor : VerticalDoor
{
    public Profile _arcadeProfile;

#if NO_TEX2D
    XnaRenderTarget2D _screenCapture;
#else
    private RenderTarget2D _screenCapture;
#endif

    private bool _transitioning;

    public MetroidDoor(float xpos, float ypos)
        : base(xpos, ypos)
    {
        _editorName = "Arcadeexit Door";
    }

    public override void Update()
    {
        if (!_transitioning)
        {
            IEnumerable<Thing> ducks = base.level.things[typeof(Duck)];
            if (ducks.Count() > 0)
            {
                Duck d = ducks.First() as Duck;
                if (d.X < base.X - 5f)
                {
                    d.X -= 10f;
                    MonoMain.transitionDirection = TransitionDirection.Left;
                    MonoMain.transitionLevel = new TitleScreen(returnFromArcade: true, _arcadeProfile);
                }
            }
        }
        base.Update();
    }

    public override void Draw()
    {
        base.Draw();
    }
}
