using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using XnaToFna;

namespace DuckGame;

internal class GraphicsDebug : IUpdateable, IDrawable
{
    #region Properties&Events
    public bool Enabled => true;

    public bool Visible { get; set; }

    public int UpdateOrder => 0;

    public int DrawOrder => UpdateOrder;

    public event EventHandler<EventArgs> EnabledChanged;

    public event EventHandler<EventArgs> VisibleChanged;

    public event EventHandler<EventArgs> UpdateOrderChanged;

    public event EventHandler<EventArgs> DrawOrderChanged;
    #endregion

    public static void Initialize()
    {
        GraphicsDebug debug = new();
        var type = typeof(Game);
        (type.GetField("updateableComponents", (BindingFlags)36).GetValue(MonoMain.instance) as List<IUpdateable>).Add(debug);
        (type.GetField("drawableComponents", (BindingFlags)36).GetValue(MonoMain.instance) as List<IDrawable>).Add(debug);
    }

    public void Update(GameTime gameTime)
    {
        if (Keyboard.Pressed(Keys.Tab))
        {
            Visible = !Visible;
        }
    }

    SpriteBatch Batch = new(Graphics.device);

    MTSpriteBatch MTBatch = Graphics.screen;

    SpriteMap Sprite = new("basketBall", 8, 8);

    VertexBatch VBatch= new(Graphics.device);

    public void Draw(GameTime gameTime)
    {
        var t = (float)gameTime.TotalGameTime.TotalSeconds * 10;
        var ratio = Graphics.height / (float)Graphics.width;

        Graphics.Clear(Color.Black);
        var iii = 0;
        var scale = .5f;
        var size = (int)(float.Max(Sprite.w, Sprite.h) * scale);
        for (int i = 0; i <= Graphics.height - size; i += size)
        {
        //MTBatch.Begin(SpriteSortMode.BackToFront, BlendState.NonPremultiplied, SamplerState.PointClamp, null, RasterizerState.CullNone);
            var ni = i / (float)Graphics.height;
            var str = "";
            for (int j = 0; j <= Graphics.width - size; j += size)
            {
                var nj = j / (float)Graphics.width;
                //Sprite.frame = iii % Sprite.frames;
                //Graphics.Draw(Sprite, j, i);
                Graphics.Draw(Sprite.texture, j, i, scale, scale, -1);
                //Graphics.DrawString("B", new(j, i), Color.White);
                var ii = (i + 1) * 10 + (j);
                str += (char)(iii);
                iii++;
            }
            //Graphics.DrawString(str, new(0, i), Color.White, 1, null, scale);
        //MTBatch.End();
        }

        //Circle(Mouse.mousePos, Color.White / 2, 128, 10000);
        //VBatch.Flush();
    }

    float Circle(float x, float y, float angle, float orbit, float radius, Vec2 offset)
    {
        return 1 - Vec2.Distance(new(x, y), Maths.AngleToVec(angle*float.Pi*2) * orbit + offset) * radius;
    }

    Color Add(Color a, Color b) =>
        new(a.r + b.r, a.g + b.g, a.b + b.b, a.a + b.a);

    Color Subtract(Color a, Color b) =>
        new(a.r - b.r, a.g - b.g, a.b - b.b, a.a - b.a);

    Color Mix(Color a, Color b, float t) =>
        Add(a, Subtract(b, a)) * t;

    void Circle(Vec2 position, Color color, float radius, int iterations)
    {
        Vec2 ps = default;
        for (int i = 0; i < iterations + 1; i++)
        {
            var t = i / (float)iterations * float.Pi * 2;
            var segment = new Vec2(float.Cos(t), float.Sin(t)) * radius + position;

            if (i != 0)
            {
                VBatch.SetVertex(new (new(position, 0), Color.Red));
                VBatch.SetVertex(new(new(segment, 0), color));
                VBatch.SetVertex(new(new(ps, 0), color));
            }

            ps = segment;
        }
    }
}

class VertexBatch(GraphicsDevice device)
{
    public int Capacity
    {
        get => Buffer.Length;
        set
        {
            if (Buffer.Length != value)
                Buffer = new VertexPositionColor[value];
        }
    }

    int Position;

    VertexPositionColor[] Buffer = new VertexPositionColor[512];

    Effect Effect = new BasicEffect(device)
    {
        VertexColorEnabled = true,
        Projection = Matrix.CreateOrthographicOffCenter(0, Graphics.width, Graphics.height, 0, 0, 1)
    };

    public void SetVertex(VertexPositionColor vertex)
    {
        if (Position / 3 >= Capacity / 3)
            Flush();

        Buffer[Position++] = vertex;
    }

    public void Flush()
    {
        foreach (var pass in Effect.CurrentTechnique.Passes)
            pass.Apply();

        device.DrawUserPrimitives(PrimitiveType.TriangleList, Buffer, 0, Position / 3);
        Position = 0;
    }
}
