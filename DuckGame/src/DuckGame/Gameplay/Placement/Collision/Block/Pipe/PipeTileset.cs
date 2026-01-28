using System;
using System.Collections.Generic;
using System.Linq;

namespace DuckGame;

public class PipeTileset : Block, IDontMove, IDrawToDifferentLayers
{
    public enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }

    private class PipeBundle
    {
        public Thing thing;

        public Vec2 cameraPosition;
    }

    private class PipeParticle
    {
        public Vec2 position;

        public Vec2 velocity;

        public float alpha;
    }

    public EditorProperty<bool> trapdoor = new EditorProperty<bool>(val: false);

    public EditorProperty<bool> background = new EditorProperty<bool>(val: false);

    private Dictionary<Direction, PipeTileset> connections = new Dictionary<Direction, PipeTileset>();

    public SpriteMap _sprite;

    public float pipeDepth;

    private bool searchUp;

    private bool searchDown;

    private bool searchLeft;

    private bool searchRight;

    private PipeTileset _pipeUp;

    private PipeTileset _pipeDown;

    private PipeTileset _pipeLeft;

    private PipeTileset _pipeRight;

    private bool _validPipe;

    private PipeTileset _oppositeEnd;

    public float travelLength;

    private HashSet<ITeleport> _objectsInPipes = new HashSet<ITeleport>();

    private List<PipeBundle> _transporting = new List<PipeBundle>();

    private HashSet<PhysicsObject> _pipingOut = new HashSet<PhysicsObject>();

    private List<ITeleport> _removeFromPipe = new List<ITeleport>();

    private List<MaterialThing> _colliding;

    private int framesSincePipeout;

    private int _transportingIndex;

    private bool _initializedConnections;

    private bool _testedValidity;

    private int _initializedBackground;

    private bool entered;

    private int _failBullets;

    private static PipeTileset _lastAdd;

    public bool hasKinks;

    public bool _foregroundDraw;

    private bool _drawBlockOverlay;

    private List<PipeParticle> _particles = new List<PipeParticle>();

    private float partRot;

    private int partWait = -100;

    public float _flapLerp;

    public float _flap;

    public Vec2 endOffset => Position + endNormal * 9f;

    public Vec2 endNormal
    {
        get
        {
            if (Up() != null)
            {
                return new Vec2(0f, 1f);
            }
            if (Down() != null)
            {
                return new Vec2(0f, -1f);
            }
            if (Left() != null)
            {
                return new Vec2(1f, 0f);
            }
            if (Right() != null)
            {
                return new Vec2(-1f, 0f);
            }
            return Position;
        }
    }

    public PipeTileset oppositeEnd => _oppositeEnd;

    public bool isEntryPipe
    {
        get
        {
            if (_validPipe && connections.Count == 1)
            {
                return !trapdoor;
            }
            return false;
        }
    }

    public bool IsBackground()
    {
        if (connections.Count > 1)
        {
            return background.value;
        }
        return false;
    }

    public PipeTileset(float x, float y, string pSprite)
        : base(x, y)
    {
        _editorName = "Pipe";
        editorTooltip = "Travel through pipes!";
        base.layer = Layer.Game;
        base.Depth = 0.9f;
        thickness = 3f;
        _sprite = new SpriteMap(pSprite, 18, 18);
        graphic = _sprite;
        physicsMaterial = PhysicsMaterial.Metal;
        Center = new Vec2(9f, 9f);
        _sprite.CenterOrigin();
        _collisionSize = new Vec2(16f, 16f);
        _collisionOffset = new Vec2(-8f, -8f);
        _sprite.frame = 0;
        placementLayerOverride = Layer.Foreground;
    }

    public bool MovingIntoPipe(Vec2 pPosition, Vec2 pVelocity, float pThresh = 2f)
    {
        bool valid = false;
        if (endNormal.X != 0f && pVelocity.X != 0f)
        {
            if (Math.Sign(pVelocity.X) != Math.Sign(endNormal.X))
            {
                valid = true;
            }
        }
        else if (endNormal.Y != 0f && pVelocity.Y != 0f && Math.Sign(pVelocity.Y) != Math.Sign(endNormal.Y))
        {
            valid = true;
        }
        if (valid)
        {
            if (Left() != null && pPosition.X < base.right + pThresh && pPosition.Y <= base.bottom + pThresh && pPosition.Y >= base.top - pThresh)
            {
                return true;
            }
            if (Right() != null && pPosition.X < base.left - pThresh && pPosition.Y <= base.bottom + pThresh && pPosition.Y >= base.top - pThresh)
            {
                return true;
            }
            if (Up() != null && pPosition.Y < base.bottom + pThresh && pPosition.X <= base.right + pThresh && pPosition.X >= base.left - pThresh)
            {
                return true;
            }
            if (Down() != null && pPosition.Y > base.top - pThresh && pPosition.X <= base.right + pThresh && pPosition.X >= base.left - pThresh)
            {
                return true;
            }
        }
        return false;
    }

    public override BinaryClassChunk Serialize()
    {
        BinaryClassChunk element = base.Serialize();
        if (connections.Count > 0)
        {
            element.AddProperty("up", Up() != null);
            element.AddProperty("down", Down() != null);
            element.AddProperty("left", Left() != null);
            element.AddProperty("right", Right() != null);
        }
        else
        {
            element.AddProperty("up", searchUp);
            element.AddProperty("down", searchDown);
            element.AddProperty("left", searchLeft);
            element.AddProperty("right", searchRight);
        }
        element.AddProperty("pipeFrame", _sprite.frame);
        return element;
    }

    public override bool Deserialize(BinaryClassChunk node)
    {
        base.Deserialize(node);
        searchUp = node.GetProperty<bool>("up");
        searchDown = node.GetProperty<bool>("down");
        searchLeft = node.GetProperty<bool>("left");
        searchRight = node.GetProperty<bool>("right");
        _sprite.frame = node.GetProperty<int>("pipeFrame");
        return true;
    }

    private void PipeOut(PhysicsObject d)
    {
        FlapPipe();
        if (d is Duck)
        {
            (d as Duck).immobilized = false;
            (d as Duck).pipeOut = 6;
            (d as Duck).CancelFlapping();
        }
        bool dontAdd = false;
        d.hSpeed = 0f;
        d.clip.Clear();
        if (Down() != null)
        {
            d.Position = Position - new Vec2(0f, 10f);
            if (d is Duck)
            {
                (d as Duck).jumping = true;
                (d as Duck).slamWait = 4;
            }
            d.vSpeed = -6f;
            if (d is RagdollPart)
            {
                d.hSpeed += Rando.Float(-1f, 1f);
            }
            for (int i = 0; i < 6; i++)
            {
                SmallSmoke smallSmoke = SmallSmoke.New(base.X + Rando.Float(-4f, 4f), base.Y + Rando.Float(-4f, 4f));
                smallSmoke.velocity = new Vec2(Rando.Float(-0.5f, 0.5f), Rando.Float(0f, -0.5f));
                Level.Add(smallSmoke);
            }
            if (Network.isActive && framesSincePipeout > 2)
            {
                Send.Message(new NMPipeOut(new Vec2(base.X, base.Y), 0));
                framesSincePipeout = 0;
            }
            if (d is Duck && Level.CheckLine<Block>(Position - new Vec2(0f, 16f), Position - new Vec2(0f, 32f)) != null)
            {
                Duck obj = d as Duck;
                obj.Position = Position - new Vec2(0f, 16f);
                obj.GoRagdoll();
                dontAdd = true;
            }
        }
        else if (Left() != null || Right() != null)
        {
            d.Position = Position + new Vec2((Left() != null) ? 12 : (-12), -2f);
            d.vSpeed = -0f;
            if (Left() != null)
            {
                d.hSpeed = 6f;
            }
            else
            {
                d.hSpeed = -6f;
            }
            for (int j = 0; j < 6; j++)
            {
                SmallSmoke s = SmallSmoke.New(base.X + (float)((Left() != null) ? 12 : (-12)) + Rando.Float(-4f, 4f), base.Y + Rando.Float(-4f, 4f));
                if (Left() != null)
                {
                    s.velocity = new Vec2(Rando.Float(0.2f, 0.7f), Rando.Float(-0.5f, 0.5f));
                }
                else
                {
                    s.velocity = new Vec2(Rando.Float(-0.7f, -0.2f), Rando.Float(-0.5f, 0.5f));
                }
                Level.Add(s);
            }
            if (Network.isActive && framesSincePipeout > 2)
            {
                Send.Message(new NMPipeOut(new Vec2(base.X + (float)((Left() != null) ? 12 : (-12)), base.Y), (byte)((Left() != null) ? 1 : 3)));
                framesSincePipeout = 0;
            }
            if (d is Duck)
            {
                Duck obj2 = d as Duck;
                obj2.sliding = true;
                obj2.crouch = true;
                obj2.crouchLock = true;
                obj2.SetCollisionMode("slide");
                obj2.Y -= 6;
                obj2.ReturnItemToWorld(obj2);
            }
            d.clip.Add(this);
            dontAdd = true;
        }
        else
        {
            d.Position = Position + new Vec2(0f, 4f);
            d.vSpeed = 5f;
            for (int k = 0; k < 6; k++)
            {
                SmallSmoke smallSmoke2 = SmallSmoke.New(base.X + Rando.Float(-4f, 4f), base.Y + 12f + Rando.Float(-4f, 4f));
                smallSmoke2.velocity = new Vec2(Rando.Float(-0.5f, 0.5f), Rando.Float(0.2f, 0.7f));
                Level.Add(smallSmoke2);
            }
            if (Network.isActive && framesSincePipeout > 2)
            {
                Send.Message(new NMPipeOut(new Vec2(base.X, base.Y + 12f), 2));
                framesSincePipeout = 0;
            }
            if (d is Duck && (Level.CheckLine<Block>(Position + new Vec2(0f, 16f), Position + new Vec2(0f, 32f)) != null || Level.CheckLine<IPlatform>(Position + new Vec2(0f, 16f), Position + new Vec2(0f, 32f)) != null))
            {
                Duck obj3 = d as Duck;
                obj3.Position = Position + new Vec2(0f, 16f);
                obj3.GoRagdoll();
                dontAdd = true;
            }
        }
        d.Ejected(this);
        if (!dontAdd)
        {
            Clip(d);
            d.skipClip = true;
            _pipingOut.Add(d);
        }
        else
        {
            d.skipClip = false;
        }
    }

    private void BreakPipeLink(PhysicsObject d)
    {
        _removeFromPipe.Add(d);
        if (d is Duck)
        {
            (d as Duck).immobilized = false;
        }
        d.skipClip = false;
    }

    private void Clip(PhysicsObject d)
    {
        if (d == null)
        {
            return;
        }
        if (_colliding != null)
        {
            foreach (MaterialThing t in _colliding)
            {
                d.clip.Add(t);
            }
        }
        d.clip.Add(this);
    }

    private void UpdatePipeEnd()
    {
        if (_colliding != null)
        {
            return;
        }
        _colliding = new List<MaterialThing>();
        IEnumerable<IPlatform> blocks = null;
        blocks = ((_pipeUp == null && _pipeDown == null) ? Level.CheckLineAll<IPlatform>(base.topLeft + new Vec2(-16f, 0f), base.bottomRight + new Vec2(16f, 0f)).ToList() : Level.CheckRectAll<IPlatform>(base.topLeft + new Vec2(0f, -16f), base.bottomRight + new Vec2(0f, 16f)).ToList());
        foreach (IPlatform b in blocks)
        {
            if (b is MaterialThing && !(b is PhysicsObject))
            {
                _colliding.Add(b as MaterialThing);
            }
        }
    }

    private void UpdatePipeEndLate()
    {
        foreach (PhysicsObject p in _pipingOut)
        {
            Thing.Fondle(p, DuckNetwork.localConnection);
            Clip(p);
            p.skipClip = true;
            p.grounded = false;
            p._sleeping = false;
            if (!Collision.Rect(base.rectangle, p.rectangle))
            {
                BreakPipeLink(p);
            }
        }
        foreach (ITeleport d in _removeFromPipe)
        {
            _objectsInPipes.Remove(d);
            if (d is PhysicsObject)
            {
                _pipingOut.Remove(d as PhysicsObject);
            }
        }
        _removeFromPipe.Clear();
    }

    private void StartTransporting(Thing pThing)
    {
        if (pThing is RagdollPart)
        {
            Ragdoll r = (pThing as RagdollPart).doll;
            if (r != null)
            {
                _transporting.Add(new PipeBundle
                {
                    thing = r.part1,
                    cameraPosition = r.part1.Position
                });
                _transporting.Add(new PipeBundle
                {
                    thing = r.part2,
                    cameraPosition = r.part1.Position
                });
                _transporting.Add(new PipeBundle
                {
                    thing = r.part3,
                    cameraPosition = r.part1.Position
                });
                _removeFromPipe.Add(r.part1);
                _removeFromPipe.Add(r.part2);
                _removeFromPipe.Add(r.part3);
            }
        }
        else
        {
            _transporting.Add(new PipeBundle
            {
                thing = pThing,
                cameraPosition = pThing.Position
            });
            _removeFromPipe.Add(pThing as ITeleport);
        }
    }

    private void UpdateEntryPipe()
    {
        IEnumerable<PhysicsObject> objects = null;
        if (Down() != null)
        {
            objects = Level.CheckRectAll<PhysicsObject>(base.topLeft + new Vec2(1f, -32f), base.bottomRight + new Vec2(-1f, 4f));
        }
        else if (Up() != null)
        {
            objects = Level.CheckRectAll<PhysicsObject>(base.topLeft + new Vec2(1f, -4f), base.bottomRight + new Vec2(-1f, 32f));
        }
        else if (Left() != null)
        {
            objects = Level.CheckRectAll<PhysicsObject>(base.topLeft + new Vec2(-4f, 3f), base.bottomRight + new Vec2(32f, -3f));
        }
        else if (Right() != null)
        {
            objects = Level.CheckRectAll<PhysicsObject>(base.topLeft + new Vec2(-32f, 3f), base.bottomRight + new Vec2(4f, -3f));
        }
        foreach (PhysicsObject d in objects)
        {
            if (d.owner != null || d.inPipe)
            {
                continue;
            }
            bool check1 = false;
            if (Down() != null)
            {
                check1 = d.bottom + d.vSpeed > base.top - 6f && d.width <= 16f && (!(d is Duck) || !(d as Duck).sliding);
            }
            else if (Up() != null)
            {
                check1 = d.top + d.vSpeed < base.bottom + 6f && d.width <= 16f && (!(d is Duck) || !(d as Duck).sliding);
            }
            else if (Left() != null)
            {
                check1 = d.left + d.hSpeed < base.right + 2f && d.height <= 16f;
            }
            else if (Right() != null)
            {
                check1 = d.right + d.hSpeed > base.left - 2f && d.height <= 16f;
            }
            if (!check1 || d == null || !d.isServerForObject)
            {
                continue;
            }
            bool check2 = false;
            if (Down() != null)
            {
                check2 = d.vSpeed > -0.1f && d.bottom < base.top + 4f && Math.Abs(d.hSpeed) < 10f;
            }
            else if (Up() != null)
            {
                check2 = d.vSpeed < -2f && d.top > base.bottom - 4f && Math.Abs(d.hSpeed) < 10f;
            }
            else if (Left() != null)
            {
                check2 = d.hSpeed < -0.2f && d.left > base.right - 4f && Math.Abs(d.vSpeed) < 4f;
            }
            else if (Right() != null)
            {
                check2 = d.hSpeed > 0.2f && d.right < base.left + 4f && Math.Abs(d.vSpeed) < 4f;
            }
            if (!check2 || _pipingOut.Contains(d))
            {
                continue;
            }
            if (d is RagdollPart)
            {
                Ragdoll r = (d as RagdollPart).doll;
                if (r != null && r.part1 != null && r.part2 != null && r.part3 != null && r.part1.owner == null && r.part2.owner == null && r.part3.owner == null && !_pipingOut.Contains(r.part1) && !_pipingOut.Contains(r.part2) && !_pipingOut.Contains(r.part3))
                {
                    _objectsInPipes.Add(r.part1);
                    _objectsInPipes.Add(r.part2);
                    _objectsInPipes.Add(r.part3);
                    r.part1.inPipe = true;
                    r.part2.inPipe = true;
                    r.part3.inPipe = true;
                }
            }
            else
            {
                d.inPipe = true;
                d.OnTeleport();
                _objectsInPipes.Add(d);
            }
        }
        foreach (ITeleport iT in Level.CheckCircleAll<ITeleport>(endOffset, 16f))
        {
            if (iT is QuadLaserBullet)
            {
                QuadLaserBullet qb = iT as QuadLaserBullet;
                if (!qb.inPipe && oppositeEnd != null && !oppositeEnd.trapdoor && MovingIntoPipe(qb.Position, qb.travel, 4f))
                {
                    _objectsInPipes.Add(iT);
                    qb.inPipe = true;
                }
            }
            else if (iT is PhysicsParticle)
            {
                PhysicsParticle pp = iT as PhysicsParticle;
                if (!pp.inPipe && oppositeEnd != null && !oppositeEnd.trapdoor && MovingIntoPipe(pp.Position, pp.velocity, 4f))
                {
                    pp.inPipe = true;
                    _objectsInPipes.Add(iT);
                }
            }
        }
        foreach (ITeleport iT2 in _objectsInPipes)
        {
            if (_removeFromPipe.Contains(iT2))
            {
                continue;
            }
            if (iT2 is QuadLaserBullet)
            {
                QuadLaserBullet qb2 = iT2 as QuadLaserBullet;
                qb2.Position = Lerp.Vec2Smooth(qb2.Position, Position, 0.2f);
                if ((Position - qb2.Position).Length() < 6f)
                {
                    qb2.Position = oppositeEnd.Position + oppositeEnd.endNormal * 4f;
                    qb2.travel = oppositeEnd.endNormal * qb2.travel.Length();
                    _removeFromPipe.Add(iT2);
                    qb2.inPipe = false;
                }
            }
            else if (iT2 is PhysicsParticle)
            {
                PhysicsParticle pp2 = iT2 as PhysicsParticle;
                pp2._grounded = true;
                pp2.Position = Lerp.Vec2Smooth(pp2.Position, Position, 0.2f);
                pp2.hSpeed *= 0.9f;
                pp2.vSpeed *= 0.9f;
                if ((Position - pp2.Position).Length() < 6f)
                {
                    pp2.Position = oppositeEnd.endOffset + new Vec2(Rando.Float(-5f, 5f) * Math.Abs(oppositeEnd.endNormal.Y), Rando.Float(-5f, 5f) * Math.Abs(oppositeEnd.endNormal.X));
                    pp2.velocity = oppositeEnd.endNormal * Rando.Float(1f, 2f);
                    pp2.hSpeed += Rando.Float(-1f, 1f) * Math.Abs(oppositeEnd.endNormal.Y);
                    pp2.vSpeed += Rando.Float(-1f, 1f) * Math.Abs(oppositeEnd.endNormal.X);
                    pp2._grounded = false;
                    _removeFromPipe.Add(iT2);
                    pp2.inPipe = false;
                }
            }
            else
            {
                if (!(iT2 is PhysicsObject))
                {
                    continue;
                }
                PhysicsObject d2 = iT2 as PhysicsObject;
                bool vertical = Up() != null || Down() != null;
                if (d2 is Duck)
                {
                    (d2 as Duck).immobilized = true;
                    if (!vertical)
                    {
                        (d2 as Duck).crouch = true;
                        (d2 as Duck).sliding = true;
                    }
                }
                Thing.Fondle(d2, DuckNetwork.localConnection);
                Clip(d2);
                d2.skipClip = true;
                d2.grounded = false;
                d2._sleeping = false;
                if (vertical)
                {
                    d2.X = Lerp.FloatSmooth(d2.Position.X, base.X, 0.4f);
                    d2.hSpeed *= 0.8f;
                    if (Down() != null)
                    {
                        d2.vSpeed += 0.4f;
                    }
                    else
                    {
                        d2.vSpeed -= 0.4f;
                    }
                }
                else
                {
                    if (d2 is Duck)
                    {
                        d2.Y = Lerp.FloatSmooth(d2.Position.Y, base.Y - 10f, 0.6f);
                    }
                    else
                    {
                        d2.Y = Lerp.FloatSmooth(d2.Position.Y, base.Y - (d2.collisionCenter.Y - base.Y), 0.5f);
                    }
                    d2.vSpeed *= 0.8f;
                    if (Left() != null)
                    {
                        d2.hSpeed -= 0.4f;
                    }
                    else if (Right() != null)
                    {
                        d2.hSpeed += 0.4f;
                    }
                }
                if (vertical)
                {
                    foreach (IPlatform p in Level.CheckRectAll<IPlatform>(base.topLeft + new Vec2(2f, -24f), base.bottomRight + new Vec2(-2f, 24f)))
                    {
                        if (p is MaterialThing)
                        {
                            d2.clip.Add(p as MaterialThing);
                        }
                    }
                }
                else
                {
                    foreach (IPlatform p2 in Level.CheckRectAll<IPlatform>(base.topLeft + new Vec2(-24f, 2f), base.bottomRight + new Vec2(24f, -2f)))
                    {
                        if (p2 is MaterialThing)
                        {
                            d2.clip.Add(p2 as MaterialThing);
                        }
                    }
                }
                if ((d2.Position - Position).Length() > 32f || d2.owner != null)
                {
                    d2.inPipe = false;
                    BreakPipeLink(d2);
                }
                else if ((vertical && Math.Abs(d2.Position.X - base.X) < 4f) || (!vertical && ((d2 is Duck && Math.Abs(d2.Position.Y - (base.Y - 10f)) < 4f) || (!(d2 is Duck) && Math.Abs(d2.Position.Y - base.Y) < 4f))))
                {
                    bool check3 = false;
                    if (Down() != null)
                    {
                        check3 = d2.Position.Y > base.top + 6f;
                    }
                    else if (Up() != null)
                    {
                        check3 = d2.Position.Y < base.bottom - 6f;
                    }
                    else if (Left() != null)
                    {
                        check3 = d2.Position.X < base.right - 6f;
                    }
                    else if (Right() != null)
                    {
                        check3 = d2.Position.X > base.left + 6f;
                    }
                    if (check3)
                    {
                        StartTransporting(d2);
                    }
                }
            }
        }
        for (_transportingIndex = 0; _transportingIndex < _transporting.Count; _transportingIndex++)
        {
            PipeBundle b = _transporting[_transportingIndex];
            Thing t = b.thing;
            if (t is PhysicsObject)
            {
                PhysicsObject o = t as PhysicsObject;
                o.updatePhysics = false;
                o.overfollow = 0.5f;
                o.Position = new Vec2(-5000f, -1000f);
                o.cameraPositionOverride = b.cameraPosition;
                b.cameraPosition = Lerp.Vec2(b.cameraPosition, _oppositeEnd.Position, travelLength / 10f);
                if ((b.cameraPosition - _oppositeEnd.Position).Length() < 4f)
                {
                    FinishTransporting(o, b);
                    SFX.Play("pipeOut", 1f, Rando.Float(-0.1f, 0.1f));
                }
            }
        }
        base.Update();
    }

    private void FinishTransporting(PhysicsObject o, PipeBundle bundle, bool pDoingRagthing = false)
    {
        o.updatePhysics = true;
        o.overfollow = 0f;
        if (bundle != null)
        {
            o.Position = bundle.cameraPosition;
        }
        o.cameraPositionOverride = Vec2.Zero;
        o.inPipe = false;
        if (bundle == null)
        {
            return;
        }
        int idx = _transporting.IndexOf(bundle);
        if (idx < 0)
        {
            return;
        }
        if (!pDoingRagthing && o is RagdollPart)
        {
            Ragdoll r = (o as RagdollPart).doll;
            if (r != null)
            {
                FinishTransporting(r.part1, _transporting.FirstOrDefault((PipeBundle x) => x.thing == r.part1), pDoingRagthing: true);
                FinishTransporting(r.part2, _transporting.FirstOrDefault((PipeBundle x) => x.thing == r.part2), pDoingRagthing: true);
                FinishTransporting(r.part3, _transporting.FirstOrDefault((PipeBundle x) => x.thing == r.part3), pDoingRagthing: true);
                r.part1.Position = new Vec2(r.part2.X + Rando.Float(-4f, 4f), r.part2.Y + Rando.Float(-4f, 4f));
                r.part3.Position = new Vec2(r.part2.X + Rando.Float(-4f, 4f), r.part2.Y + Rando.Float(-4f, 4f));
                return;
            }
        }
        else
        {
            if (o is RagdollPart)
            {
                (o as RagdollPart)._lastReasonablePosition = o.Position;
            }
            _oppositeEnd.PipeOut(o);
            _transporting.RemoveAt(idx);
        }
        if (idx <= _transportingIndex)
        {
            _transportingIndex--;
        }
    }

    public override void EditorAdded()
    {
        Dictionary<Direction, PipeTileset> neighbors = GetNeighbors();
        if (!AttemptObviousConnection(neighbors) && _lastAdd != null)
        {
            if (Up(neighbors) == _lastAdd && Up(neighbors).ReadyForConnection())
            {
                MakeConnection(Up(neighbors));
            }
            else if (Down(neighbors) == _lastAdd && Down(neighbors).ReadyForConnection())
            {
                MakeConnection(Down(neighbors));
            }
            else if (Left(neighbors) == _lastAdd && Left(neighbors).ReadyForConnection())
            {
                MakeConnection(Left(neighbors));
            }
            else if (Right(neighbors) == _lastAdd && Right(neighbors).ReadyForConnection())
            {
                MakeConnection(Right(neighbors));
            }
        }
        searchUp = (searchDown = (searchLeft = (searchRight = false)));
        if (Up() != null)
        {
            searchUp = true;
            Up().searchDown = true;
        }
        if (Down() != null)
        {
            searchDown = true;
            Down().searchUp = true;
        }
        if (Left() != null)
        {
            searchLeft = true;
            Left().searchRight = true;
        }
        if (Right() != null)
        {
            searchRight = true;
            Right().searchLeft = true;
        }
        TestValidity();
        _lastAdd = this;
    }

    public override void EditorFlip(bool pVertical)
    {
        if (pVertical)
        {
            bool swap = searchUp;
            searchUp = searchDown;
            searchDown = swap;
        }
        else
        {
            bool swap2 = searchLeft;
            searchLeft = searchRight;
            searchRight = swap2;
        }
    }

    public override void EditorRemoved()
    {
        if (Up() != null)
        {
            PipeTileset pipeTileset = Up();
            BreakConnection(Up());
            pipeTileset.TestValidity();
        }
        if (Down() != null)
        {
            PipeTileset pipeTileset2 = Down();
            BreakConnection(Down());
            pipeTileset2.TestValidity();
        }
        if (Left() != null)
        {
            PipeTileset pipeTileset3 = Left();
            BreakConnection(Left());
            pipeTileset3.TestValidity();
        }
        if (Right() != null)
        {
            PipeTileset pipeTileset4 = Right();
            BreakConnection(Right());
            pipeTileset4.TestValidity();
        }
        TestValidity();
        _lastAdd = null;
    }

    public override void EditorObjectsChanged()
    {
        ReConnect();
        TestValidity();
        UpdateConnectionFrame();
        base.EditorObjectsChanged();
    }

    public override void EditorRender()
    {
        if (Level.current is Editor && (Level.current as Editor).placementType is PipeTileset)
        {
            base.Alpha = 0.6f;
            Draw();
            base.Alpha = 1f;
            base.EditorRender();
        }
    }

    public override void OnEditorLoaded()
    {
        ReConnect();
        TestValidity();
    }

    public override void Update()
    {
        if (!_initializedConnections)
        {
            ReConnect();
            _initializedConnections = true;
        }
        else if (!_testedValidity)
        {
            TestValidity();
        }
        if (_failBullets > 0)
        {
            _failBullets--;
        }
        if (connections.Count == 1)
        {
            UpdatePipeEnd();
        }
        framesSincePipeout++;
        if (isEntryPipe)
        {
            UpdateEntryPipe();
        }
        if (connections.Count == 1)
        {
            UpdatePipeEndLate();
        }
        _initializedBackground++;
        if (_initializedBackground == 2 && !(Level.current is Editor))
        {
            if (IsBackground())
            {
                _collisionOffset = new Vec2(Vec2.MinValue);
            }
            if (_validPipe && connections.Count == 1 && (Left() != null || Right() != null) && Level.CheckPoint<Block>(Position, this) != null)
            {
                solid = false;
            }
        }
        base.Update();
    }

    public override void Draw()
    {
        Color c = graphic.color;
        if (IsBackground())
        {
            base.Depth = pipeDepth - 1.8f;
            graphic.color = c * 0.5f;
            graphic.color = new Color(graphic.color.r, graphic.color.g, graphic.color.b, byte.MaxValue);
        }
        else
        {
            base.Depth = pipeDepth;
            if (Left() != null && Left().IsBackground())
            {
                int f = _sprite.frame;
                _sprite.frame = 22;
                _sprite.flipH = true;
                Graphics.Draw(_sprite, base.X - 16f, base.Y, base.Depth + 5);
                _sprite.flipH = false;
                _sprite.frame = f;
            }
            if (Right() != null && Right().IsBackground())
            {
                int f2 = _sprite.frame;
                _sprite.frame = 22;
                Graphics.Draw(_sprite, base.X + 16f, base.Y, base.Depth + 5);
                _sprite.frame = f2;
            }
            if (Up() != null && Up().IsBackground())
            {
                int f3 = _sprite.frame;
                _sprite.frame = 22;
                _sprite.AngleDegrees = -90f;
                Graphics.Draw(_sprite, base.X, base.Y - 16f, base.Depth + 5);
                _sprite.AngleDegrees = 0f;
                _sprite.frame = f3;
            }
            if (Down() != null && Down().IsBackground())
            {
                int f4 = _sprite.frame;
                _sprite.frame = 22;
                _sprite.AngleDegrees = 90f;
                _sprite.flipV = true;
                Graphics.Draw(_sprite, base.X, base.Y + 16f, base.Depth + 5);
                _sprite.AngleDegrees = 0f;
                _sprite.flipV = false;
                _sprite.frame = f4;
            }
        }
        if (isEntryPipe && !(Level.current is Editor))
        {
            DrawParticles();
        }
        base.Draw();
        graphic.color = c;
    }

    public override void OnSolidImpact(MaterialThing with, ImpactedFrom from)
    {
        if (with is PhysicalBullet)
        {
            Hit((with as PhysicalBullet).bullet, (with as PhysicalBullet).bullet.currentTravel);
            if (entered)
            {
                return;
            }
        }
        base.OnSolidImpact(with, from);
    }

    public override bool Hit(Bullet bullet, Vec2 hitPos)
    {
        entered = false;
        if (connections.Count == 1 && (!trapdoor || bullet.ammo.penetration >= thickness) && ((((hitPos.X - 8f < base.left && Right() != null && bullet.travelDirNormalized.X > 0.3f) || (hitPos.X + 8f > base.right && Left() != null && bullet.travelDirNormalized.X < -0.3f)) && hitPos.Y > base.top && hitPos.Y < base.bottom) || (((hitPos.Y - 8f < base.top && Down() != null && bullet.travelDirNormalized.Y > 0.3f) || (hitPos.Y + 8f > base.bottom && Up() != null && bullet.travelDirNormalized.Y < -0.3f)) && hitPos.X > base.left && hitPos.X < base.right)) && oppositeEnd != null)
        {
            float newLength = bullet._totalLength - (bullet._actualStart - hitPos).Length();
            float randomDir = 0f;
            if (newLength > 0f)
            {
                bool fail = false;
                bool missile = bullet.ammo is ATMissile;
                if (bullet.ammo != null && !bullet.ammo.flawlessPipeTravel && !missile && hasKinks)
                {
                    fail = true;
                    _failBullets++;
                    if (_failBullets > 5)
                    {
                        fail = false;
                    }
                }
                if (missile)
                {
                    newLength = bullet.ammo.range;
                }
                if ((bool)oppositeEnd.trapdoor && !(bullet.ammo.penetration >= oppositeEnd.thickness))
                {
                    fail = true;
                }
                if (!fail)
                {
                    if (oppositeEnd.Left() != null)
                    {
                        bullet.DoRebound(oppositeEnd.endOffset, 0f + randomDir, newLength);
                    }
                    if (oppositeEnd.Right() != null)
                    {
                        bullet.DoRebound(oppositeEnd.endOffset, 180f + randomDir, newLength);
                    }
                    if (oppositeEnd.Up() != null)
                    {
                        bullet.DoRebound(oppositeEnd.endOffset, 270f + randomDir, newLength);
                    }
                    if (oppositeEnd.Down() != null)
                    {
                        bullet.DoRebound(oppositeEnd.endOffset, 90f + randomDir, newLength);
                    }
                }
                entered = true;
                return true;
            }
        }
        return base.Hit(bullet, hitPos);
    }

    private void ReConnect()
    {
        if (searchUp || searchDown || searchLeft || searchRight)
        {
            connections.Clear();
            _pipeLeft = null;
            _pipeRight = null;
            _pipeUp = null;
            _pipeDown = null;
            Dictionary<Direction, PipeTileset> neighbors = GetNeighbors();
            if (searchUp && Up(neighbors) != null)
            {
                MakeConnection(Up(neighbors));
            }
            if (searchDown && Down(neighbors) != null)
            {
                MakeConnection(Down(neighbors));
            }
            if ((searchLeft || (flipHorizontal && searchRight)) && Left(neighbors) != null)
            {
                MakeConnection(Left(neighbors));
            }
            if ((searchRight || (flipHorizontal && searchLeft)) && Right(neighbors) != null)
            {
                MakeConnection(Right(neighbors));
            }
        }
    }

    public PipeTileset Up(Dictionary<Direction, PipeTileset> pNeighbors = null)
    {
        if (pNeighbors == null)
        {
            return _pipeUp;
        }
        PipeTileset ret = null;
        pNeighbors.TryGetValue(Direction.Up, out ret);
        return ret;
    }

    public PipeTileset Down(Dictionary<Direction, PipeTileset> pNeighbors = null)
    {
        if (pNeighbors == null)
        {
            return _pipeDown;
        }
        PipeTileset ret = null;
        pNeighbors.TryGetValue(Direction.Down, out ret);
        return ret;
    }

    public PipeTileset Left(Dictionary<Direction, PipeTileset> pNeighbors = null)
    {
        if (pNeighbors == null)
        {
            return _pipeLeft;
        }
        PipeTileset ret = null;
        pNeighbors.TryGetValue(Direction.Left, out ret);
        return ret;
    }

    public PipeTileset Right(Dictionary<Direction, PipeTileset> pNeighbors = null)
    {
        if (pNeighbors == null)
        {
            return _pipeRight;
        }
        PipeTileset ret = null;
        pNeighbors.TryGetValue(Direction.Right, out ret);
        return ret;
    }

    protected virtual Dictionary<Direction, PipeTileset> GetNeighbors()
    {
        Dictionary<Direction, PipeTileset> neighbors = new Dictionary<Direction, PipeTileset>();
        PipeTileset up = (from x in Level.CheckPointAll<PipeTileset>(base.X, base.Y - 16f)
                          where x.@group == @group
                          select x).FirstOrDefault();
        if (up != null)
        {
            neighbors[Direction.Up] = up;
        }
        PipeTileset down = (from x in Level.CheckPointAll<PipeTileset>(base.X, base.Y + 16f)
                            where x.@group == @group
                            select x).FirstOrDefault();
        if (down != null)
        {
            neighbors[Direction.Down] = down;
        }
        PipeTileset left = (from x in Level.CheckPointAll<PipeTileset>(base.X - 16f, base.Y)
                            where x.@group == @group
                            select x).FirstOrDefault();
        if (left != null)
        {
            neighbors[Direction.Left] = left;
        }
        PipeTileset right = (from x in Level.CheckPointAll<PipeTileset>(base.X + 16f, base.Y)
                             where x.@group == @group
                             select x).FirstOrDefault();
        if (right != null)
        {
            neighbors[Direction.Right] = right;
        }
        return neighbors;
    }

    public bool ReadyForConnection()
    {
        return connections.Count <= 1;
    }

    private PipeTileset Travel(PipeTileset pTowards)
    {
        PipeTileset last = this;
        int loops = 0;
        while (pTowards != this)
        {
            loops++;
            if (loops > 255)
            {
                break;
            }
            if ((pTowards._pipeLeft != null && (pTowards._pipeUp != null || pTowards._pipeDown != null)) || (pTowards._pipeRight != null && (pTowards._pipeUp != null || pTowards._pipeDown != null)))
            {
                hasKinks = true;
            }
            if (pTowards.connections.Count > 2)
            {
                pTowards.ReConnect();
            }
            if (pTowards._pipeUp != null && pTowards._pipeUp != last)
            {
                last = pTowards;
                pTowards = pTowards._pipeUp;
                continue;
            }
            if (pTowards._pipeDown != null && pTowards._pipeDown != last)
            {
                last = pTowards;
                pTowards = pTowards._pipeDown;
                continue;
            }
            if (pTowards._pipeLeft != null && pTowards._pipeLeft != last)
            {
                last = pTowards;
                pTowards = pTowards._pipeLeft;
                continue;
            }
            if (pTowards._pipeRight == null || pTowards._pipeRight == last)
            {
                break;
            }
            last = pTowards;
            pTowards = pTowards._pipeRight;
        }
        return pTowards;
    }

    private void MakeConnection(PipeTileset pWith)
    {
        if (pWith.Y == base.Y)
        {
            if (pWith.X > base.X)
            {
                pWith.connections[Direction.Left] = this;
                pWith._pipeLeft = this;
                connections[Direction.Right] = pWith;
                _pipeRight = pWith;
            }
            if (pWith.X < base.X)
            {
                pWith.connections[Direction.Right] = this;
                pWith._pipeRight = this;
                connections[Direction.Left] = pWith;
                _pipeLeft = pWith;
            }
        }
        else if (pWith.X == base.X)
        {
            if (pWith.Y > base.Y)
            {
                pWith.connections[Direction.Up] = this;
                pWith._pipeUp = this;
                connections[Direction.Down] = pWith;
                _pipeDown = pWith;
            }
            if (pWith.Y < base.Y)
            {
                pWith.connections[Direction.Down] = this;
                pWith._pipeDown = this;
                connections[Direction.Up] = pWith;
                _pipeUp = pWith;
            }
        }
        UpdateConnectionFrame();
        pWith.UpdateConnectionFrame();
    }

    private void BreakConnection(PipeTileset pWith)
    {
        if (pWith.Y == base.Y)
        {
            if (pWith.X > base.X)
            {
                pWith.connections.Remove(Direction.Left);
                pWith.searchLeft = false;
                connections.Remove(Direction.Right);
                _pipeRight = null;
            }
            if (pWith.X < base.X)
            {
                pWith.connections.Remove(Direction.Right);
                pWith._pipeRight = null;
                pWith.searchRight = false;
                connections.Remove(Direction.Left);
                _pipeLeft = null;
            }
        }
        else if (pWith.X == base.X)
        {
            if (pWith.Y > base.Y)
            {
                pWith.connections.Remove(Direction.Up);
                pWith._pipeUp = null;
                pWith.searchUp = false;
                connections.Remove(Direction.Down);
                _pipeDown = null;
            }
            if (pWith.Y < base.Y)
            {
                pWith.connections.Remove(Direction.Down);
                pWith._pipeDown = null;
                pWith.searchDown = false;
                connections.Remove(Direction.Up);
                _pipeUp = null;
            }
        }
        UpdateConnectionFrame();
        pWith.UpdateConnectionFrame();
    }

    private void UpdateConnectionFrame()
    {
        OnUpdateConnectionFrame();
        if (connections.Count == 1)
        {
            _drawBlockOverlay = Level.CheckPoint<Block>(Position, this) != null;
            _foregroundDraw = true;
        }
        else
        {
            _foregroundDraw = false;
        }
        if (frame == 0)
        {
            collisionSize = new Vec2(14f, 14f);
            collisionOffset = new Vec2(-7f, -7f);
        }
        else if (frame == 1)
        {
            collisionSize = new Vec2(15f, 15f);
            collisionOffset = new Vec2(-7f, -7f);
        }
        else if (frame == 2)
        {
            collisionSize = new Vec2(15f, 15f);
            collisionOffset = new Vec2(-8f, -7f);
        }
        else if (frame == 3)
        {
            collisionSize = new Vec2(16f, 14f);
            collisionOffset = new Vec2(-8f, -7f);
        }
        else if (frame == 5)
        {
            collisionSize = new Vec2(15f, 15f);
            collisionOffset = new Vec2(-7f, -8f);
        }
        else if (frame == 6)
        {
            collisionSize = new Vec2(15f, 15f);
            collisionOffset = new Vec2(-8f, -8f);
        }
        else if (frame == 7)
        {
            collisionSize = new Vec2(14f, 16f);
            collisionOffset = new Vec2(-7f, -8f);
        }
        else if (frame == 8)
        {
            collisionSize = new Vec2(14f, 15f);
            collisionOffset = new Vec2(-7f, -7f);
        }
        else if (frame == 9)
        {
            collisionSize = new Vec2(14f, 15f);
            collisionOffset = new Vec2(-7f, -8f);
        }
        else if (frame == 10)
        {
            collisionSize = new Vec2(15f, 14f);
            collisionOffset = new Vec2(-7f, -7f);
        }
        else if (frame == 11)
        {
            collisionSize = new Vec2(15f, 14f);
            collisionOffset = new Vec2(-8f, -7f);
        }
        if (IsBackground())
        {
            solid = false;
            thickness = 0f;
            physicsMaterial = PhysicsMaterial.Default;
        }
        else
        {
            solid = true;
            thickness = 3f;
            physicsMaterial = PhysicsMaterial.Metal;
        }
    }

    private void OnUpdateConnectionFrame()
    {
        if (connections.Count == 0)
        {
            _sprite.frame = 0;
        }
        else if (connections.Count == 1)
        {
            if (Up() != null)
            {
                _sprite.frame = 9;
            }
            else if (Down() != null)
            {
                _sprite.frame = 8;
            }
            else if (Left() != null)
            {
                _sprite.frame = 11;
            }
            else if (Right() != null)
            {
                _sprite.frame = 10;
            }
        }
        else if (Up() != null && Right() != null)
        {
            _sprite.frame = 5;
        }
        else if (Up() != null && Left() != null)
        {
            _sprite.frame = 6;
        }
        else if (Up() != null && Down() != null)
        {
            _sprite.frame = 7;
        }
        else if (Down() != null && Right() != null)
        {
            _sprite.frame = 1;
        }
        else if (Down() != null && Left() != null)
        {
            _sprite.frame = 2;
        }
        else if (Left() != null && Right() != null)
        {
            _sprite.frame = 3;
        }
    }

    private bool AttemptObviousConnection(Dictionary<Direction, PipeTileset> pNeighbors)
    {
        bool connected = false;
        if (Up(pNeighbors) == null && Down(pNeighbors) == null)
        {
            if (Left(pNeighbors) != null && Left(pNeighbors).ReadyForConnection())
            {
                MakeConnection(Left(pNeighbors));
                connected = true;
            }
            if (Right(pNeighbors) != null && Right(pNeighbors).ReadyForConnection())
            {
                MakeConnection(Right(pNeighbors));
                connected = true;
            }
        }
        if (Left(pNeighbors) == null && Right(pNeighbors) == null)
        {
            if (Up(pNeighbors) != null && Up(pNeighbors).ReadyForConnection())
            {
                MakeConnection(Up(pNeighbors));
                connected = true;
            }
            if (Down(pNeighbors) != null && Down(pNeighbors).ReadyForConnection())
            {
                MakeConnection(Down(pNeighbors));
                connected = true;
            }
        }
        if (!connected)
        {
            List<PipeTileset> canConnect = new List<PipeTileset>();
            foreach (KeyValuePair<Direction, PipeTileset> pair in pNeighbors)
            {
                if (pair.Value.ReadyForConnection())
                {
                    canConnect.Add(pair.Value);
                }
            }
            if (canConnect.Count <= 0 || canConnect.Count > 2)
            {
                return false;
            }
            connected = true;
            if (_lastAdd != null && (canConnect[0] == _lastAdd || (canConnect.Count > 1 && canConnect[1] == _lastAdd)))
            {
                MakeConnection(_lastAdd);
            }
            else
            {
                MakeConnection(canConnect[0]);
                if (canConnect.Count == 2)
                {
                    MakeConnection(canConnect[1]);
                }
            }
        }
        return connected;
    }

    private void TestValidity()
    {
        int numUp = 0;
        int numDown = 0;
        PipeTileset first = null;
        PipeTileset second = null;
        PipeTileset firstTest = null;
        PipeTileset secondTest = null;
        if (connections.Count == 1)
        {
            if (_pipeUp != null)
            {
                numDown++;
                first = _pipeUp;
                secondTest = this;
            }
            else if (_pipeDown != null)
            {
                numUp++;
                first = _pipeDown;
                secondTest = this;
            }
            else if (_pipeLeft != null)
            {
                first = _pipeLeft;
                secondTest = this;
            }
            else if (_pipeRight != null)
            {
                first = _pipeRight;
                secondTest = this;
            }
        }
        else
        {
            if (_pipeUp != null)
            {
                if (first == null)
                {
                    first = _pipeUp;
                }
                else
                {
                    second = _pipeUp;
                }
            }
            if (_pipeDown != null)
            {
                if (first == null)
                {
                    first = _pipeDown;
                }
                else
                {
                    second = _pipeDown;
                }
            }
            if (_pipeLeft != null)
            {
                if (first == null)
                {
                    first = _pipeLeft;
                }
                else
                {
                    second = _pipeLeft;
                }
            }
            if (_pipeRight != null)
            {
                if (first == null)
                {
                    first = _pipeRight;
                }
                else
                {
                    second = _pipeRight;
                }
            }
        }
        if (first != null)
        {
            firstTest = Travel(first);
            if (firstTest != this && firstTest.connections.Count == 1)
            {
                if (firstTest._pipeUp == null && firstTest._pipeDown != null)
                {
                    numUp++;
                }
                if (firstTest._pipeDown == null && firstTest._pipeUp != null)
                {
                    numDown++;
                }
            }
        }
        if (second != null)
        {
            secondTest = Travel(second);
            if (secondTest != this && secondTest.connections.Count == 1)
            {
                if (secondTest._pipeUp == null && secondTest._pipeDown != null)
                {
                    numUp++;
                }
                if (secondTest._pipeDown == null && secondTest._pipeUp != null)
                {
                    numDown++;
                }
            }
        }
        _validPipe = numUp switch
        {
            1 => numDown == 1,
            2 => true,
            _ => false,
        };
        _validPipe = connections.Count > 0;
        if (firstTest != null)
        {
            firstTest._validPipe = _validPipe;
        }
        if (secondTest != null)
        {
            secondTest._validPipe = _validPipe;
        }
        if (_validPipe)
        {
            firstTest._oppositeEnd = secondTest;
            secondTest._oppositeEnd = firstTest;
            firstTest.travelLength = (secondTest.travelLength = (firstTest.Position - secondTest.Position).Length());
        }
        _testedValidity = true;
    }

    private void DrawParticles()
    {
        if (partWait == -100)
        {
            partRot = Rando.Float(10f);
        }
        Vec2 rotatedEndNormal = endNormal.Rotate(Maths.DegToRad((Up() != null || Right() != null) ? 90f : (-90f)), Vec2.Zero);
        partWait--;
        if (partWait <= 0)
        {
            PipeParticle p = null;
            foreach (PipeParticle part in _particles)
            {
                if (part.alpha >= 1f)
                {
                    p = part;
                    break;
                }
            }
            if (p == null)
            {
                p = new PipeParticle();
                _particles.Add(p);
            }
            p.position = Position + endNormal * 20f + Maths.AngleToVec(partRot) * (10f + Rando.Float(24f)) * rotatedEndNormal;
            p.alpha = 0f;
            p.velocity = Vec2.Zero;
            partWait = 5;
            partRot += 1.72152f;
        }
        for (int i = 0; i < _particles.Count; i++)
        {
            if (_particles[i].alpha < 1f)
            {
                Vec2 dif = Position - _particles[i].position;
                _particles[i].velocity -= endNormal * 0.03f;
                _particles[i].position -= dif * rotatedEndNormal * 0.07f;
                Graphics.DrawLine(_particles[i].position, _particles[i].position + _particles[i].velocity * 3f, Color.White * _particles[i].alpha, 0.75f, base.Depth - 10);
                _particles[i].position += _particles[i].velocity;
                _particles[i].alpha += 0.016f;
                if ((_particles[i].position * endNormal - Position * endNormal).Length() < 2f)
                {
                    _particles[i].alpha = 1f;
                }
            }
        }
    }

    public void OnDrawLayer(Layer pLayer)
    {
        if (!_foregroundDraw || pLayer != Layer.Blocks)
        {
            return;
        }
        int prev = _sprite.frame;
        _flapLerp = Lerp.FloatSmooth(_flapLerp, _flap, 0.25f);
        _flap = Lerp.Float(_flap, 0f, 0.15f);
        if (_drawBlockOverlay)
        {
            _sprite.frame += 8;
            Graphics.Draw(_sprite, Position.X, Position.Y, 0.5f);
        }
        if (trapdoor.value)
        {
            Vec2 c = _sprite.Center;
            _sprite.frame = 20;
            float capOffset = (_drawBlockOverlay ? 10 : 9);
            if (Left() != null)
            {
                _sprite.Center = new Vec2(2f, 0f);
                _sprite.AngleDegrees = 0f - _flapLerp * 90f;
                Graphics.Draw(_sprite, Position.X + capOffset, Position.Y - 9f, 0.5f);
                _sprite.Center = new Vec2(9f, 9f);
                _sprite.AngleDegrees = 0f;
                _sprite.frame = 21;
                Graphics.Draw(_sprite, Position.X + (capOffset - 1f), Position.Y - 9f, 0.4f);
            }
            else if (Right() != null)
            {
                _sprite.Center = new Vec2(2f, 18f);
                _sprite.AngleDegrees = 180f + _flapLerp * 90f;
                _sprite.flipV = true;
                Graphics.Draw(_sprite, Position.X - capOffset, Position.Y - 9f, 0.5f);
                _sprite.Center = new Vec2(9f, 9f);
                _sprite.AngleDegrees = 0f;
                _sprite.frame = 21;
                _sprite.flipH = true;
                Graphics.Draw(_sprite, Position.X - (capOffset - 1f), Position.Y - 9f, 0.4f);
            }
            else if (Up() != null)
            {
                _sprite.Center = new Vec2(2f, 0f);
                _sprite.AngleDegrees = 90f - _flapLerp * 90f;
                Graphics.Draw(_sprite, Position.X + 9f, Position.Y + capOffset, 0.5f);
                _sprite.Center = new Vec2(9f, 9f);
                _sprite.AngleDegrees = 90f;
                _sprite.frame = 21;
                Graphics.Draw(_sprite, Position.X + 9f, Position.Y + (capOffset - 1f), 0.4f);
            }
            else if (Down() != null)
            {
                _sprite.Center = new Vec2(2f, 18f);
                _sprite.AngleDegrees = 270f + _flapLerp * 90f;
                _sprite.flipV = true;
                Graphics.Draw(_sprite, Position.X + 9f, Position.Y - capOffset, 0.5f);
                _sprite.Center = new Vec2(9f, 9f);
                _sprite.AngleDegrees = 90f;
                _sprite.frame = 21;
                _sprite.flipH = true;
                Graphics.Draw(_sprite, Position.X + 9f, Position.Y - (capOffset - 1f), 0.4f);
            }
            _sprite.flipV = false;
            _sprite.flipH = false;
            _sprite.Center = c;
        }
        _sprite.frame = prev;
    }

    public void FlapPipe()
    {
        _flap = 1.9f;
    }
}
