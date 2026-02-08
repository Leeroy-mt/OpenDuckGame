using Microsoft.Xna.Framework;
using System;

namespace DuckGame;

public class ChaingunBullet : Thing
{
    public Thing parentThing;

    public Thing childThing;

    public Vector2 chainOffset;

    public float sway;

    public float desiredSway;

    public float lastDesiredSway;

    public float wave;

    public float shake;

    public float waveSpeed;

    public float waveAdd = 0.07f;

    public ChaingunBullet(float xpos, float ypos)
        : base(xpos, ypos)
    {
        graphic = new Sprite("chainBullet");
        Center = new Vector2(4f, 3f);
        base.Depth = 0.8f;
    }

    public ChaingunBullet(float xpos, float ypos, bool dart)
        : base(xpos, ypos)
    {
        if (dart)
        {
            graphic = new SpriteMap("dart", 16, 16);
            Center = new Vector2(7f, 7f);
        }
        else
        {
            graphic = new Sprite("chainBullet");
            Center = new Vector2(4f, 3f);
        }
        base.Depth = 0.8f;
    }

    public override void Update()
    {
        wave += 0.1f + waveSpeed;
        if (childThing != null)
        {
            childThing.Update();
        }
    }

    public override void Draw()
    {
        if (parentThing != null)
        {
            Position = parentThing.Position + chainOffset + new Vector2(0f, 2f);
            graphic.flipH = parentThing.graphic.flipH;
            desiredSway = 0f;
            if (parentThing is Gun { owner: not null } g)
            {
                desiredSway = 0f - g.owner.hSpeed;
            }
            else
            {
                desiredSway = 0f - parentThing.hSpeed;
            }
            shake += Math.Abs(lastDesiredSway - desiredSway) * 0.3f;
            if (shake > 0f)
            {
                shake -= 0.01f;
            }
            else
            {
                shake = 0f;
            }
            if (shake > 1.5f)
            {
                shake = 1.5f;
                waveSpeed += 0.02f;
            }
            if (waveSpeed > 0.1f)
            {
                waveSpeed = 0.1f;
            }
            if (waveSpeed > 0)
            {
                waveSpeed -= 0.01f;
            }
            else
            {
                waveSpeed = 0;
            }
            lastDesiredSway = desiredSway;
            if (parentThing is ChaingunBullet b)
            {
                desiredSway += b.sway * 0.7f;
            }
            desiredSway += (float)Math.Sin(wave + waveAdd) * shake;
            sway = MathHelper.Lerp(sway, desiredSway, 1);
            X += sway;
        }
        base.Draw();
        if (childThing != null)
        {
            childThing.Depth = Depth - 1;
            childThing.Draw();
        }
    }
}
