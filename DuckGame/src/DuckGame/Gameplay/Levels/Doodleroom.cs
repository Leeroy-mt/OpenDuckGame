using Microsoft.Xna.Framework.Graphics;

namespace DuckGame;

internal class Doodleroom : Level
{
    private Texture2D _image;

    private MonoMain.WebCharData _data;

    public override void Initialize()
    {
        _data = MonoMain.RequestRandomCharacter();
        base.camera.width *= 2f;
        base.camera.height *= 2f;
        base.backgroundColor = Color.White;
        base.Initialize();
    }

    public override void Update()
    {
        if (Keyboard.Pressed(Keys.R))
        {
            _data = MonoMain.RequestRandomCharacter();
        }
        base.Update();
    }

    public override void PostDrawLayer(Layer layer)
    {
        if (layer == Layer.HUD && _data != null)
        {
            Graphics.Draw(_data.image, layer.camera.width / 2f - 32f, layer.camera.height / 2f - 32f, 0.5f, 0.5f, 0.5f);
            float nameWidth = Graphics.GetStringWidth(_data.name);
            Graphics.DrawString(_data.name, new Vec2(layer.camera.width / 2f - nameWidth / 2f, layer.camera.height / 2f - 43f), Color.Black, 1f);
            string text = "\"" + _data.quote + "\"";
            float quoteWidth = Graphics.GetStringWidth(text);
            Graphics.DrawString(text, new Vec2(layer.camera.width / 2f - quoteWidth / 2f, layer.camera.height / 2f + 38f), Color.Black, 1f);
        }
        base.PostDrawLayer(layer);
    }
}
