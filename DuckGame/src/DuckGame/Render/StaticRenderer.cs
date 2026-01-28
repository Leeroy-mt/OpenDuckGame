using System;

namespace DuckGame;

public class StaticRenderer
{
    private static MultiMap<Layer, StaticRenderSection> _targets = new MultiMap<Layer, StaticRenderSection>();

    private static Vec2 _position = new Vec2(-128f, -128f);

    private static int _size = 128;

    private static int _numSections = 8;

    public static void InitializeLayer(Layer layer)
    {
        if (_targets.ContainsKey(layer))
        {
            return;
        }
        for (int y = 0; y < _numSections; y++)
        {
            for (int x = 0; x < _numSections; x++)
            {
                StaticRenderSection section = new StaticRenderSection();
                section.target = new RenderTarget2D(_size, _size);
                section.position = new Vec2(_position.X + (float)(x * _size), _position.Y + (float)(y * _size));
                _targets.Add(layer, section);
            }
        }
    }

    public static void ProcessThing(Thing t)
    {
        Layer layer = Layer.Background;
        Vec2 vec = t.Position - t.Center - _position;
        int xpos1 = (int)Math.Floor(vec.X / (float)_size);
        int ypos1 = (int)Math.Floor(vec.Y / (float)_size);
        InitializeLayer(layer);
        Vec2 vec2 = t.Position - t.Center + new Vec2(t.graphic.width, t.graphic.height) - _position;
        int xpos2 = (int)Math.Floor(vec2.X / (float)_size);
        int ypos2 = (int)Math.Floor(vec2.Y / (float)_size);
        _targets[layer][ypos1 * _numSections + xpos1].things.Add(t);
        if (xpos1 != xpos2)
        {
            _targets[layer][ypos1 * _numSections + xpos2].things.Add(t);
        }
        if (ypos1 != ypos2)
        {
            _targets[layer][ypos2 * _numSections + xpos1].things.Add(t);
        }
        if (xpos1 != xpos2 && ypos1 != ypos2)
        {
            _targets[layer][ypos2 * _numSections + xpos2].things.Add(t);
        }
    }

    public static void Update()
    {
    }

    public static void RenderLayer(Layer layer)
    {
    }
}
