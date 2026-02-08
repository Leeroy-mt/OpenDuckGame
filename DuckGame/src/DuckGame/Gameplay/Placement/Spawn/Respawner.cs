using Microsoft.Xna.Framework;
using System;

namespace DuckGame;

[BaggedProperty("isInDemo", true)]
public class Respawner : Thing, IDrawToDifferentLayers
{
    private SpriteMap _sprite;

    private float _animate;

    private float _noiseOffset;

    public Respawner(float xpos, float ypos)
        : base(xpos, ypos)
    {
        _sprite = new SpriteMap("respawner", 18, 10);
        graphic = _sprite;
        Center = new Vector2(9f, 5f);
        collisionOffset = new Vector2(-8f, -4f);
        collisionSize = new Vector2(16f, 4f);
        base.hugWalls = WallHug.Floor;
        base.layer = Layer.Blocks;
        base.Depth = 0.8f;
        _animate = Rando.Float(100f);
        editorTooltip = "";
    }

    public override void Update()
    {
        base.Update();
    }

    public void OnDrawLayer(Layer pLayer)
    {
        if (pLayer != Layer.Game)
        {
            return;
        }
        _animate += 0.05f;
        _ = base.Y;
        int numLines = 6;
        for (int i = 0; i < numLines; i++)
        {
            Vector2 linePos = new Vector2(base.X - 6f, base.Y - (float)i * 4f - _animate % 1f * 4f);
            float dist = 1f - (base.Y - linePos.Y) / 24f;
            float thick = dist * 3f;
            linePos.Y += thick / 2f;
            Graphics.DrawLine(linePos, linePos + new Vector2(12f, 0f), Colors.DGBlue * (dist * 0.8f), thick, -0.75f);
        }
        Vector2 noiseSize = new Vector2(7f, 8f);
        Vector2 noisePos = Position + new Vector2(-7f, -24f);
        for (int j = 0; (float)j < noiseSize.X * noiseSize.Y; j++)
        {
            Vector2 pos = new Vector2((int)((float)j % noiseSize.X), (int)((float)j / noiseSize.Y));
            float fallSpeedMult = (Noise.Generate(pos.X * 32f, 0f) + 1f) / 2f * 1.5f + 0.1f;
            float noiseOffsetMult = (int)(_animate * fallSpeedMult / 1f);
            float noiseOffsetY = _animate * 0.1f - noiseOffsetMult;
            float noise = Noise.Generate(pos.X + 100f, (pos.Y + 100f - noiseOffsetY) * 0.5f);
            if (noise > 0.25f)
            {
                pos.Y -= _animate * fallSpeedMult % 1f;
                float edge = 1f - Math.Abs((noiseSize.X / 2f - pos.X) / noiseSize.X * 2f);
                float a = (noise - 0.25f) / 0.75f * edge * Math.Max(0f, Math.Min((pos.Y / noiseSize.Y - 0.1f) * 2f, 1f));
                pos *= 2f;
                pos.Y *= 2f;
                Graphics.DrawRect(pos + noisePos, pos + noisePos + new Vector2(1f, 1f), Color.White * a, -0.5f);
            }
        }
    }
}
