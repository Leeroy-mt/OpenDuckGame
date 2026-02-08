using Microsoft.Xna.Framework;

namespace DuckGame;

public abstract class TutorialSign : Thing
{
    public TutorialSign(float xpos, float ypos, string image, string name)
        : base(xpos, ypos)
    {
        if (image != null)
        {
            graphic = new Sprite(image);
            Center = new Vector2(graphic.w / 2, graphic.h / 2);
            _collisionSize = new Vector2(16f, 16f);
            _collisionOffset = new Vector2(-8f, -8f);
            base.Depth = -0.5f;
            _editorName = name;
            base.layer = Layer.Background;
        }
    }
}
