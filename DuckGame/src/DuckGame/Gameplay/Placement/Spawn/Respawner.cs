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
        center = new Vec2(9f, 5f);
        collisionOffset = new Vec2(-8f, -4f);
        collisionSize = new Vec2(16f, 4f);
        base.hugWalls = WallHug.Floor;
        base.layer = Layer.Blocks;
        base.depth = 0.8f;
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
        _ = base.y;
        int numLines = 6;
        for (int i = 0; i < numLines; i++)
        {
            Vec2 linePos = new Vec2(base.x - 6f, base.y - (float)i * 4f - _animate % 1f * 4f);
            float dist = 1f - (base.y - linePos.y) / 24f;
            float thick = dist * 3f;
            linePos.y += thick / 2f;
            Graphics.DrawLine(linePos, linePos + new Vec2(12f, 0f), Colors.DGBlue * (dist * 0.8f), thick, -0.75f);
        }
        Vec2 noiseSize = new Vec2(7f, 8f);
        Vec2 noisePos = position + new Vec2(-7f, -24f);
        for (int j = 0; (float)j < noiseSize.x * noiseSize.y; j++)
        {
            Vec2 pos = new Vec2((int)((float)j % noiseSize.x), (int)((float)j / noiseSize.y));
            float fallSpeedMult = (Noise.Generate(pos.x * 32f, 0f) + 1f) / 2f * 1.5f + 0.1f;
            float noiseOffsetMult = (int)(_animate * fallSpeedMult / 1f);
            float noiseOffsetY = _animate * 0.1f - noiseOffsetMult;
            float noise = Noise.Generate(pos.x + 100f, (pos.y + 100f - noiseOffsetY) * 0.5f);
            if (noise > 0.25f)
            {
                pos.y -= _animate * fallSpeedMult % 1f;
                float edge = 1f - Math.Abs((noiseSize.x / 2f - pos.x) / noiseSize.x * 2f);
                float a = (noise - 0.25f) / 0.75f * edge * Math.Max(0f, Math.Min((pos.y / noiseSize.y - 0.1f) * 2f, 1f));
                pos *= 2f;
                pos.y *= 2f;
                Graphics.DrawRect(pos + noisePos, pos + noisePos + new Vec2(1f, 1f), Color.White * a, -0.5f);
            }
        }
    }
}
