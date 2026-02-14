using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace DuckGame;

public class MaterialGrid : Material
{
    private Thing _thing;

    private float transWave = 0.2f;

    private bool secondScan;

    private bool scanSwitch;

    public bool finished;

    public MaterialGrid(Thing t)
    {
        _effect = Content.Load<MTEffect>("Shaders/wireframeTexOuya");
        _thing = t;
    }

    public override void Update()
    {
        transWave -= 0.12f;
        if (Math.Sin(transWave) < -0.699999988079071 && !secondScan)
        {
            secondScan = true;
            transWave -= 2f;
        }
        else if (Math.Sin(transWave) > 0.9 && secondScan)
        {
            finished = true;
        }
        base.Update();
    }

    public override void Apply()
    {
        Matrix fullMatrix = Layer.Game.fullMatrix;
        Vector3 trans = Vector3.Transform(new Vector3(_thing.X - 28f, _thing.Y, 0f), fullMatrix);
        Vector3 trans2 = Vector3.Transform(new Vector3(_thing.X + 28f, _thing.Y, 0f), fullMatrix);
        SetValue("scan", trans.X + ((float)Math.Sin(transWave) + 1f) / 2f * (trans2.X - trans.X));
        SetValue("secondScan", secondScan ? 1f : 0f);
        foreach (EffectPass pass in _effect.effect.CurrentTechnique.Passes)
        {
            pass.Apply();
        }
    }
}
