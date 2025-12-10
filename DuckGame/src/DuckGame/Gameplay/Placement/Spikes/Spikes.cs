namespace DuckGame;

[EditorGroup("Stuff|Spikes")]
[BaggedProperty("isInDemo", true)]
[BaggedProperty("previewPriority", true)]
public class Spikes : MaterialThing, IDontMove
{
    private SpriteMap _sprite;

    public bool up = true;

    protected ImpactedFrom _killImpact;

    public Spikes(float xpos, float ypos)
        : base(xpos, ypos)
    {
        _sprite = new SpriteMap("spikes", 16, 19);
        _sprite.speed = 0.1f;
        graphic = _sprite;
        center = new Vec2(8f, 14f);
        collisionOffset = new Vec2(-6f, -3f);
        collisionSize = new Vec2(13f, 5f);
        base.depth = 0.28f;
        _editorName = "Spikes Up";
        editorTooltip = "Pointy and dangerous, unless you're wearing the right boots.";
        editorCycleType = typeof(SpikesRight);
        thickness = 3f;
        physicsMaterial = PhysicsMaterial.Metal;
        base.editorOffset = new Vec2(0f, 6f);
        base.hugWalls = WallHug.Floor;
        _editorImageCenter = true;
        _killImpact = ImpactedFrom.Top;
    }

    public override void OnSoftImpact(MaterialThing with, ImpactedFrom from)
    {
        if (with is TV || with is Hat || !with.isServerForObject)
        {
            return;
        }
        Duck d = with as Duck;
        if (_killImpact == ImpactedFrom.Top && d != null && d.holdObject is Sword && (d.holdObject as Sword)._slamStance)
        {
            return;
        }
        float killSpeed = 1f;
        if (from != _killImpact)
        {
            return;
        }
        if (from == ImpactedFrom.Left && with.hSpeed > killSpeed)
        {
            with.Destroy(new DTImpale(this));
        }
        if (from == ImpactedFrom.Right && with.hSpeed < 0f - killSpeed)
        {
            with.Destroy(new DTImpale(this));
        }
        if (from == ImpactedFrom.Top && with.vSpeed > killSpeed && (d == null || !d.HasEquipment(typeof(Boots))))
        {
            bool kill = true;
            if (with is PhysicsObject)
            {
                PhysicsObject p = with as PhysicsObject;
                Vec2 lastBr = with.bottomRight;
                Vec2 lastBl = with.bottomLeft;
                Vec2 bottomMiddle;
                Vec2 vec = (bottomMiddle = new Vec2(with.x, with.bottom));
                Vec2 posDif = p.lastPosition - p.position;
                lastBr += posDif;
                lastBl += posDif;
                Vec2 p2 = vec + posDif;
                kill = false;
                if (Collision.LineIntersect(p2, bottomMiddle, base.topLeft, base.topRight) || Collision.LineIntersect(lastBl, with.bottomLeft, base.topLeft, base.topRight) || Collision.LineIntersect(lastBr, with.bottomRight, base.topLeft, base.topRight))
                {
                    kill = true;
                }
            }
            if (kill)
            {
                with.Destroy(new DTImpale(this));
            }
        }
        if (from == ImpactedFrom.Bottom && with.vSpeed < 0f - killSpeed && (d == null || !d.HasEquipment(typeof(Helmet))))
        {
            with.Destroy(new DTImpale(this));
        }
    }

    public override void Update()
    {
        base.Update();
    }

    public override void Draw()
    {
        base.Draw();
    }
}
