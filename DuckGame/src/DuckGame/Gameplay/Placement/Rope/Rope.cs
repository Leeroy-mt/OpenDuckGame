using System;

namespace DuckGame;

public class Rope : Thing
{
    private Thing _attach1;

    private Thing _attach2;

    public Vec2 offsetDir = Vec2.Zero;

    public float linkDirectionOnSplit;

    public float offsetDegrees;

    private float _properLength = -1f;

    private BlockCorner _corner;

    private Vec2 _pos1;

    private Vec2 _pos2;

    private bool _terminated;

    public Thing _thing;

    private Sprite _vine;

    private bool _isVine;

    public Thing _belongsTo;

    private Vec2 dirLine;

    private Vec2 tpos = Vec2.Zero;

    private bool pulled;

    public Vec2 linkVector;

    public Vec2 cornerVector;

    public Vec2 breakVector;

    public float startLength;

    private Rope pull;

    public bool serverForObject;

    public Thing attach1
    {
        get
        {
            return _attach1;
        }
        set
        {
            _attach1 = value;
        }
    }

    public Thing attach2
    {
        get
        {
            return _attach2;
        }
        set
        {
            _attach2 = value;
        }
    }

    public override NetworkConnection connection
    {
        get
        {
            if (_belongsTo != null)
            {
                return _belongsTo.connection;
            }
            return base.connection;
        }
    }

    public override NetIndex8 authority
    {
        get
        {
            if (_belongsTo != null)
            {
                return _belongsTo.authority;
            }
            return base.authority;
        }
    }

    public float linkDirection
    {
        get
        {
            if (attach2 is Rope r)
            {
                Vec2 dir = (attach2Point - attach1Point).Rotate(Maths.DegToRad(r.offsetDegrees), Vec2.Zero);
                return Maths.PointDirection(new Vec2(0f, 0f), dir);
            }
            return 0f;
        }
    }

    public float linkDirectionNormalized => Maths.PointDirection(new Vec2(0f, 0f), attach2Point - attach1Point);

    public float properLength
    {
        get
        {
            return _properLength;
        }
        set
        {
            _properLength = value;
        }
    }

    public Vec2 attach1Point
    {
        get
        {
            if (_attach1 is Rope { _corner: not null } r)
            {
                Vec2 move = r._corner.corner - r._corner.block.position;
                move.Normalize();
                return _attach1.position + move * 4f;
            }
            return _attach1.position;
        }
    }

    public Vec2 attach2Point
    {
        get
        {
            if (_attach2 is Rope { _corner: not null } r)
            {
                Vec2 move = r._corner.corner - r._corner.block.position;
                move.Normalize();
                return _attach2.position + move * 4f;
            }
            return _attach2.position;
        }
    }

    public float length
    {
        get
        {
            if (_attach1 != null && _attach2 != null)
            {
                return (_attach1.position - _attach2.position).length;
            }
            return 0f;
        }
    }

    public Rope(float xpos, float ypos, Thing attach1Val, Thing attach2Val, Thing thing = null, bool vine = false, Sprite tex = null, Thing belongsTo = null)
        : base(xpos, ypos)
    {
        _belongsTo = belongsTo;
        if (attach1Val == null)
        {
            attach1Val = this;
        }
        if (attach2Val == null)
        {
            attach2Val = this;
        }
        _attach1 = attach1Val;
        _attach2 = attach2Val;
        _pos1 = attach1Val.position;
        _pos2 = attach2Val.position;
        _thing = thing;
        if (vine)
        {
            _vine = new Sprite("vine");
            _vine.center = new Vec2(8f, 0f);
        }
        if (tex != null)
        {
            _vine = tex;
        }
        _isVine = vine;
        base.depth = -0.5f;
    }

    public void RemoveRope()
    {
        _terminated = true;
        visible = false;
        Level.Remove(this);
        if (_attach1 is Rope { _terminated: false } a1)
        {
            a1.RemoveRope();
        }
        if (_attach2 is Rope { _terminated: false } a2)
        {
            a2.RemoveRope();
        }
    }

    public void TerminateLaterRopes()
    {
        if (_attach2 is Rope { _terminated: false } a2)
        {
            a2.TerminateLaterRopesRecurse();
        }
        _attach2 = null;
    }

    public void TerminateLaterRopesRecurse()
    {
        _terminated = true;
        visible = false;
        Level.Remove(this);
        if (_attach2 is Rope { _terminated: false } a2)
        {
            a2.TerminateLaterRopesRecurse();
        }
    }

    public override void Terminate()
    {
    }

    public void CheckLinks()
    {
        if (!(_attach2.GetType() == typeof(Rope)) || !(cornerVector != Vec2.Zero))
        {
            return;
        }
        Rope at2 = _attach2 as Rope;
        bool regroup = false;
        if ((attach1Point - attach2Point).length < 4f)
        {
            regroup = true;
        }
        else
        {
            float angleDir = 0f;
            angleDir = ((!(cornerVector.x > 0f)) ? (at2.linkDirectionNormalized + 90f) : (at2.linkDirectionNormalized - 90f));
            breakVector = Maths.AngleToVec(Maths.DegToRad(angleDir));
            if (Math.Acos(Vec2.Dot(breakVector, cornerVector)) > Math.PI / 2.0)
            {
                angleDir += 180f;
                breakVector = Maths.AngleToVec(Maths.DegToRad(angleDir));
            }
            dirLine = (attach1.position - attach2.position).normalized;
            if (Math.Acos(Vec2.Dot(breakVector, dirLine)) < 1.5207963260498385)
            {
                regroup = true;
            }
        }
        if (regroup)
        {
            _attach2 = at2.attach2;
            _properLength += at2.properLength;
            Level.Remove(at2);
            cornerVector = at2.cornerVector;
            at2 = _attach2 as Rope;
        }
    }

    public void AddLength(float length)
    {
        if (_attach2 is Rope next)
        {
            next.AddLength(length);
        }
        else if (_attach2 is Harpoon harpoon)
        {
            Vec2 dir = position - harpoon.position;
            dir.Normalize();
            harpoon.position -= dir * length;
        }
    }

    public void Pull(float length)
    {
        pulled = true;
        Rope next = pull;
        if (next != null)
        {
            next.Pull(length);
        }
        else
        {
            properLength += length;
        }
    }

    public override void Update()
    {
        if (Network.isActive && _belongsTo != null && _belongsTo is Grapple)
        {
            _isVine = false;
            _vine = (_belongsTo as Grapple)._ropeSprite;
        }
        if (_terminated || !base.isServerForObject || !serverForObject)
        {
            return;
        }
        bool changed = false;
        if (_attach1.position != _pos1)
        {
            changed = true;
            _pos1 = _attach1.position;
        }
        if (_attach2.position != _pos2)
        {
            changed = true;
            _pos2 = _attach2.position;
        }
        if (changed || pulled)
        {
            pulled = false;
            Vec2 start = attach1Point;
            Vec2 end = attach2Point;
            Vec2 add = end - start;
            add.Normalize();
            int tries = 0;
            for (; Level.CheckPoint<Block>(end) != null; end -= add)
            {
                tries++;
                if (tries > 30)
                {
                    break;
                }
            }
            tries = 0;
            add = end - start;
            float len = add.length;
            add.Normalize();
            if (_belongsTo is IPullBack)
            {
                if (pull == null && _attach2 is Harpoon)
                {
                    properLength = startLength;
                }
                else if (attach2 is Harpoon)
                {
                    Pull(properLength - len);
                    properLength = len;
                }
            }
            while (Level.CheckPoint<Block>(start) != null)
            {
                tries++;
                if (tries > 30)
                {
                    len = 0f;
                    break;
                }
                start += add;
                len -= 1f;
            }
            if (len > 8f)
            {
                Vec2 point;
                AutoBlock b = Level.CheckLine<AutoBlock>(start, start + add * len, out point);
                if (b != null)
                {
                    BlockCorner near = b.GetNearestCorner(point);
                    if (near != null)
                    {
                        BlockCorner c = near.Copy();
                        Vec2 move = c.corner - c.block.position;
                        move.Normalize();
                        c.corner += move * 1f;
                        if ((c.corner - attach2.position).length > 4f)
                        {
                            linkVector = (attach2Point - attach1Point).normalized;
                            Rope newRope = new Rope(c.corner.x, c.corner.y, null, _attach2, null, _isVine, _vine);
                            newRope.cornerVector = cornerVector;
                            cornerVector = new Vec2((move.x > 0f) ? 1f : (-1f), (move.y > 0f) ? 1f : (-1f)).normalized;
                            newRope._corner = c;
                            newRope._belongsTo = _belongsTo;
                            _attach2 = newRope;
                            Level.Add(newRope);
                            properLength -= newRope.length;
                            newRope.properLength = newRope.length;
                            newRope.pull = this;
                            newRope.offsetDegrees = newRope.linkDirectionNormalized;
                            newRope.offsetDir = newRope.attach2Point - newRope.attach1Point;
                            newRope.linkDirectionOnSplit = linkDirection;
                        }
                    }
                }
            }
        }
        CheckLinks();
    }

    public void SetServer(bool server)
    {
        serverForObject = server;
        if (_attach2 is Rope { _terminated: false } a2)
        {
            a2.SetServer(server);
        }
    }

    public override void Draw()
    {
        if (DevConsole.showCollision && cornerVector != Vec2.Zero)
        {
            Graphics.DrawLine(_attach2.position, _attach2.position + cornerVector * 32f, Color.Red);
            Graphics.DrawLine(_attach2.position, _attach2.position + breakVector * 16f, Color.Blue);
            Graphics.DrawLine(_attach2.position, _attach2.position + dirLine * 8f, Color.Orange);
        }
        float amount = length / properLength;
        if (!serverForObject)
        {
            amount = 1f;
        }
        if (_vine != null)
        {
            Vec2 travel = attach2Point - attach1Point;
            Vec2 travelNorm = travel.normalized;
            Vec2 travelOffset = travelNorm;
            travelOffset = travelOffset.Rotate(Maths.DegToRad(90f), Vec2.Zero);
            float num = travel.length;
            float stepSize = 16f;
            Vec2 drawStart = attach1Point + travelNorm * stepSize;
            Vec2 drawPrev = attach1Point;
            Depth d = base.depth;
            int num2 = (int)Math.Ceiling(num / stepSize);
            for (int i = 0; i < num2; i++)
            {
                float sinVal = (float)Math.PI * 2f / (float)num2 * (float)i;
                float sinMult = (1f - amount) * 16f;
                Vec2 toPos = drawStart + travelOffset * (float)(Math.Sin(sinVal) * (double)sinMult);
                if (i == num2 - 1)
                {
                    toPos = attach2Point;
                }
                _vine.angleDegrees = 0f - (Maths.PointDirection(drawPrev, toPos) + 90f);
                _vine.depth = d;
                d += 1;
                float lent = (toPos - drawPrev).length;
                if (i == num2 - 1)
                {
                    _vine.yscale = 1f;
                    Graphics.Draw(_vine, drawPrev.x, drawPrev.y, new Rectangle(0f, 0f, 16f, (int)(lent % stepSize)));
                }
                else
                {
                    _vine.yscale = lent / 16f + 0.1f;
                    Graphics.Draw(_vine, drawPrev.x, drawPrev.y);
                }
                drawPrev = toPos;
                drawStart += travelNorm * stepSize;
                sinVal += (float)Math.PI * 2f / (float)num2;
            }
        }
        else if (amount < 0.95f && amount > 0f)
        {
            Vec2 travel2 = attach2Point - attach1Point;
            Vec2 travelOffset2 = travel2.normalized.Rotate(Maths.DegToRad(90f), Vec2.Zero);
            float sinVal2 = (float)Math.PI / 4f;
            Vec2 drawStart2 = attach1Point + travel2 / 8f;
            Vec2 drawPrev2 = attach1Point;
            for (int j = 0; j < 8; j++)
            {
                float sinMult2 = (1f - amount) * 8f;
                Graphics.DrawLine(drawPrev2, drawStart2 + travelOffset2 * (float)Math.Sin(sinVal2) * sinMult2, Color.White * 0.8f, 1f, base.depth - 1);
                drawPrev2 = drawStart2 + travelOffset2 * (float)Math.Sin(sinVal2) * sinMult2;
                drawStart2 += travel2 / 8f;
                sinVal2 += (float)Math.PI / 4f;
            }
        }
        else
        {
            Graphics.DrawLine(attach1Point, attach2Point, Color.White * 0.8f, 1f, base.depth - 1);
        }
    }
}
