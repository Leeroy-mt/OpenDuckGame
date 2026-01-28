namespace DuckGame;

[EditorGroup("Blocks|Snow")]
public class PineTreeSnowTileset : PineTree
{
    private SpriteMap _snowFall;

    private bool didChange;

    private float snowWait = 1f;

    public PineTreeSnowTileset(float x, float y)
        : base(x, y, "pineTilesetSnow")
    {
        _editorName = "Pine Snow";
        physicsMaterial = PhysicsMaterial.Wood;
        verticalWidth = 14f;
        verticalWidthThick = 15f;
        horizontalHeight = 8f;
        _tileset = "pineTileset";
        base.Depth = -0.55f;
        _snowFall = new SpriteMap("snowFall", 8, 24);
        _snowFall.AddAnimation("fall", 0.2f + Rando.Float(0.1f), false, 0, 1, 2, 3, 4);
        _snowFall.AddAnimation("idle", 0.4f, false, default(int));
        _snowFall.SetAnimation("idle");
        _snowFall.Center = new Vec2(4f, 0f);
        snowWait = Rando.Float(4f);
    }

    public override void KnockOffSnow(Vec2 dir, bool vertShake)
    {
        iterated = true;
        if (!knocked || vertShake)
        {
            bool num = knocked;
            knocked = true;
            PineTree left = null;
            PineTree right = null;
            left = Level.CheckPoint<PineTreeSnowTileset>(base.X - 8f, base.Y, this);
            right = Level.CheckPoint<PineTreeSnowTileset>(base.X + 8f, base.Y, this);
            if (left != null && !left.iterated && (!left.knocked || vertShake))
            {
                left.KnockOffSnow(dir, vertShake);
            }
            if (right != null && !right.iterated && (!right.knocked || vertShake))
            {
                right.KnockOffSnow(dir, vertShake);
            }
            if (!num)
            {
                for (int i = 0; i < 2; i++)
                {
                    Level.Add(new SnowFallParticle(base.X + Rando.Float(-4f, 4f), base.Y + Rando.Float(-4f, 4f), dir * Rando.Float(1f) + new Vec2(Rando.Float(-0.1f, -0.1f), Rando.Float(-0.1f, -0.1f) - Rando.Float(0.1f, 0.3f))));
                }
            }
        }
        if (_snowFall.currentAnimation == "idle")
        {
            _snowFall.SetAnimation("fall");
        }
        if (vertShake)
        {
            _vertPush = 0.5f;
        }
        knocked = true;
        iterated = false;
    }

    public override void Update()
    {
        if (!edge && !didChange)
        {
            snowWait -= 0.01f;
            if (snowWait <= 0f)
            {
                snowWait = Rando.Float(2f, 3f);
                if (Rando.Float(1f) > 0.92f)
                {
                    Level.Add(new SnowFallParticle(base.X + Rando.Float(-4f, 4f), base.Y + Rando.Float(-4f, 4f), new Vec2(0f, 0f)));
                }
            }
        }
        base.Update();
    }

    public override void Draw()
    {
        if (!edge && _snowFall.currentAnimation != "idle" && !_snowFall.finished)
        {
            _snowFall.Depth = -0.1f;
            _snowFall.Scale = new Vec2(1f, (float)_snowFall.frame / 5f * 0.4f + 0.2f);
            _snowFall.Alpha = 1f - (float)_snowFall.frame / 5f * 1f;
            Graphics.Draw(_snowFall, base.X, base.Y - 7f + (float)_snowFall.frame / 5f * 3f);
        }
        if (_snowFall.currentAnimation != "idle" && (edge || (_snowFall.frame == 1 && !didChange)))
        {
            didChange = true;
            _sprite = new SpriteMap("pineTileset", 8, 16);
            _sprite.frame = (graphic as SpriteMap).frame;
            graphic = _sprite;
        }
        base.Draw();
    }
}
