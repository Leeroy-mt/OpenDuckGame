using System;
using System.Collections.Generic;

namespace DuckGame;

[EditorGroup("Stuff|Ropes")]
[BaggedProperty("canSpawn", false)]
[BaggedProperty("isOnlineCapable", false)]
public class PhysicsRope : Thing
{
    protected bool chain;

    protected Sprite _vine;

    protected Sprite _vineEnd;

    protected Sprite _beam;

    private bool _root;

    private List<PhysicsRopeSection> _nodes = new List<PhysicsRopeSection>();

    private int _divisions = 10;

    private float _length = 0.5f;

    private float _lenDiv;

    private float _gravity = 0.25f;

    private bool _create = true;

    public Vine _lowestVine;

    private int _lowestVineSection;

    public EditorProperty<int> length = new EditorProperty<int>(4, null, 1f, 16f, 1f);

    private float soundWait;

    public bool root
    {
        get
        {
            return _root;
        }
        set
        {
            _root = value;
        }
    }

    public Vine highestVine
    {
        get
        {
            if (_lowestVine == null || _lowestVine.removeFromLevel)
            {
                return null;
            }
            Vine v = _lowestVine;
            while (v.prevVine != null)
            {
                v = v.prevVine;
            }
            return v;
        }
    }

    public PhysicsRope(float xpos, float ypos, PhysicsRope next = null)
        : base(xpos, ypos)
    {
        _vine = new SpriteMap("vine", 16, 16);
        graphic = _vine;
        Center = new Vec2(8f, 8f);
        _vineEnd = new Sprite("vineStretchEnd");
        _vineEnd.Center = new Vec2(8f, 0f);
        collisionOffset = new Vec2(-5f, -4f);
        collisionSize = new Vec2(11f, 7f);
        graphic = _vine;
        _beam = new Sprite("vineStretch");
        _editorName = "Vine";
        editorTooltip = "The original rope. Great for swinging through a jungle.";
        _placementCost += 50;
    }

    public override void Initialize()
    {
        if (Level.current is not Editor)
        {
            Y -= 8;
            _length = length.value / 6.5f;
            _divisions = (int)(length.value * 16F / 8);
            _lenDiv = _length / _divisions;
            for (int i = 0; i <= _divisions; i++)
            {
                _nodes.Add(new PhysicsRopeSection(X + _lenDiv * i, Y, this));
                Level.Add(_nodes[^1]);
            }
        }
    }

    public override void Terminate()
    {
        foreach (PhysicsRopeSection node in _nodes)
        {
            Level.Remove(node);
        }
        base.Terminate();
    }

    public virtual Vine GetSection(float x, float y, int div)
    {
        return new Vine(x, y, div);
    }

    public Vine LatchOn(PhysicsRopeSection section, Duck d)
    {
        UpdateVineProgress();
        for (int i = 0; i <= _divisions; i++)
        {
            if (section != _nodes[i])
            {
                continue;
            }
            if (i < _lowestVineSection)
            {
                int div = i;
                Vine newLowest = _lowestVine;
                Vec2 pos = new Vec2(base.X, base.Y + 8f);
                _lowestVine = GetSection(pos.X, pos.Y, div * 8);
                _lowestVine.length.value = div / 2;
                _lowestVine.owner = d;
                _lowestVine.sectionIndex = i;
                Level.Add(_lowestVine);
                if (newLowest != null)
                {
                    newLowest._rope.attach2 = _lowestVine.owner;
                    newLowest._rope.properLength = (newLowest._rope.attach2Point - newLowest._rope.attach1Point).Length();
                    _lowestVine.nextVine = newLowest;
                    newLowest.prevVine = _lowestVine;
                }
                _lowestVine.changeSpeed = false;
                if (d.vSpeed > 0f)
                {
                    d.vSpeed = 0f;
                }
                _lowestVine.UpdateRopeStuff();
                _lowestVine.UpdateRopeStuff();
                _lowestVine.changeSpeed = true;
                Vine lowestVine = _lowestVine;
                _lowestVine = newLowest;
                return lowestVine;
            }
            int div2 = i - _lowestVineSection;
            Vine prevLowest = _lowestVine;
            Vec2 pos2 = new Vec2(base.X, base.Y + 8f);
            if (_lowestVine != null)
            {
                pos2 = _lowestVine._rope.attach1Point;
            }
            _lowestVine = GetSection(pos2.X, pos2.Y, div2 * 8);
            _lowestVine.length.value = div2 / 2;
            _lowestVine.owner = d;
            _lowestVine.sectionIndex = i;
            Level.Add(_lowestVine);
            if (prevLowest != null)
            {
                _lowestVine._rope.attach2 = prevLowest.owner;
                prevLowest.nextVine = _lowestVine;
                _lowestVine.prevVine = prevLowest;
            }
            _lowestVine.changeSpeed = false;
            if (d.vSpeed > 0f)
            {
                d.vSpeed = 0f;
            }
            _lowestVine.UpdateRopeStuff();
            _lowestVine.UpdateRopeStuff();
            _lowestVine.changeSpeed = true;
            _lowestVineSection = i;
            return _lowestVine;
        }
        return null;
    }

    public void UpdateVineProgress()
    {
        if (_lowestVine != null && _lowestVine.owner != null)
        {
            _nodes[_lowestVineSection].Position = _lowestVine.owner.Position;
        }
        else if (_lowestVine != null && _lowestVine.prevVine != null)
        {
            _lowestVine = _lowestVine.prevVine;
            _lowestVineSection = _lowestVine.sectionIndex;
        }
        else
        {
            _lowestVineSection = 0;
            _lowestVine = null;
        }
    }

    public override void Update()
    {
        if (_create)
        {
            int idx = 0;
            foreach (PhysicsRopeSection node in _nodes)
            {
                node.Position = Position + new Vec2(0f, idx * 8);
                idx++;
            }
            _create = false;
            for (int i = 0; i < 10; i++)
            {
                Update();
            }
        }
        base.Update();
        if (_nodes.Count == 0)
        {
            return;
        }
        UpdateVineProgress();
        _nodes[0].Position = Position;
        foreach (PhysicsRopeSection node2 in _nodes)
        {
            node2.accel.Y = _gravity;
            node2.calcPos = node2.Position;
        }
        for (int j = 1; j <= _divisions; j++)
        {
            float tempX = _nodes[j].Position.X;
            _nodes[j].calcPos.X += 0.999f * _nodes[j].calcPos.X - 0.999f * _nodes[j].tempPos.X + _nodes[j].accel.X;
            float tempY = _nodes[j].Position.Y;
            _nodes[j].calcPos.Y += 0.999f * _nodes[j].calcPos.Y - 0.999f * _nodes[j].tempPos.Y + _nodes[j].accel.Y;
            _nodes[j].tempPos.X = tempX;
            _nodes[j].tempPos.Y = tempY;
        }
        for (int k = 1; k <= _divisions; k++)
        {
            for (int l = 1; l <= _divisions; l++)
            {
                float dx = (_nodes[l].calcPos.X - _nodes[l - 1].calcPos.X) / 100f;
                float dy = (_nodes[l].calcPos.Y - _nodes[l - 1].calcPos.Y) / 100f;
                float d = (float)Math.Sqrt(dx * dx + dy * dy);
                float diff = (d - _lenDiv) * 50f;
                _nodes[l].calcPos.X -= dx / d * diff;
                _nodes[l].calcPos.Y -= dy / d * diff;
                _nodes[l - 1].calcPos.X += dx / d * diff;
                _nodes[l - 1].calcPos.Y += dy / d * diff;
            }
        }
        Vine highestSection = highestVine;
        List<VineSection> sections = null;
        VineSection section = null;
        int sectionIndex = 0;
        if (highestSection != null)
        {
            sections = highestSection.points;
            section = sections[0];
        }
        bool jingle = false;
        bool bigJingle = false;
        int moveIndex = 0;
        for (int m = 0; m <= _divisions; m++)
        {
            if (section != null)
            {
                if (m >= section.lowestSection)
                {
                    sectionIndex++;
                    section = ((sectionIndex >= sections.Count) ? null : sections[sectionIndex]);
                    moveIndex = 0;
                }
                if (section != null && m < section.lowestSection)
                {
                    Vec2 vec = section.pos2 - section.pos1;
                    vec.Normalize();
                    _nodes[m].Position = section.pos1 + vec * moveIndex * 8f;
                    _nodes[m].calcPos = _nodes[m].Position;
                }
            }
            moveIndex++;
            _nodes[m].frictionMult = 0f;
            _nodes[m].gravMultiplier = 0f;
            _nodes[m].hSpeed = _nodes[m].calcPos.X - _nodes[m].Position.X;
            _nodes[m].vSpeed = _nodes[m].calcPos.Y - _nodes[m].Position.Y;
            float max = 5f;
            if (_nodes[m].hSpeed > 0f && _nodes[m].hSpeed > max)
            {
                _nodes[m].hSpeed = max;
            }
            if (_nodes[m].hSpeed < 0f && _nodes[m].hSpeed < 0f - max)
            {
                _nodes[m].hSpeed = 0f - max;
            }
            foreach (PhysicsObject obj in Level.CheckPointAll<PhysicsObject>(_nodes[m].Position))
            {
                if (obj.hSpeed > 0f && _nodes[m].hSpeed < obj.hSpeed)
                {
                    _nodes[m].hSpeed += obj.hSpeed;
                    if (Math.Abs(obj.hSpeed) > 2f)
                    {
                        if (Math.Abs(obj.hSpeed) > 4f)
                        {
                            bigJingle = true;
                        }
                        jingle = true;
                    }
                }
                if (!(obj.hSpeed < 0f) || !(_nodes[m].hSpeed > obj.hSpeed))
                {
                    continue;
                }
                _nodes[m].hSpeed += obj.hSpeed;
                if (Math.Abs(obj.hSpeed) > 2f)
                {
                    if (Math.Abs(obj.hSpeed) > 4f)
                    {
                        bigJingle = true;
                    }
                    jingle = true;
                }
            }
            _nodes[m].UpdatePhysics();
        }
        if (soundWait > 0f)
        {
            soundWait -= 0.01f;
        }
        if (!(chain && jingle) || !(soundWait <= 0f))
        {
            return;
        }
        soundWait = 0.1f;
        if (!jingle)
        {
            return;
        }
        int roll = Rando.Int(2);
        if (bigJingle)
        {
            switch (roll)
            {
                case 0:
                    SFX.Play("chainShake01", Rando.Float(0.6f, 0.8f), Rando.Float(-0.2f, 0.2f));
                    break;
                case 1:
                    SFX.Play("chainShake02", Rando.Float(0.6f, 0.8f), Rando.Float(-0.2f, 0.2f));
                    break;
                default:
                    SFX.Play("chainShake03", Rando.Float(0.6f, 0.8f), Rando.Float(-0.2f, 0.2f));
                    break;
            }
        }
        else
        {
            switch (roll)
            {
                case 0:
                    SFX.Play("chainShakeSmall", Rando.Float(0.3f, 0.5f), Rando.Float(-0.2f, 0.2f));
                    break;
                case 1:
                    SFX.Play("chainShakeSmall02", Rando.Float(0.3f, 0.5f), Rando.Float(-0.2f, 0.2f));
                    break;
                default:
                    SFX.Play("chainShakeSmall03", Rando.Float(0.3f, 0.5f), Rando.Float(-0.2f, 0.2f));
                    break;
            }
        }
    }

    public override void Draw()
    {
        if (Level.current is Editor)
        {
            graphic.Center = new Vec2(8f, 8f);
            graphic.Depth = base.Depth;
            for (int i = 0; i < (int)length; i++)
            {
                Graphics.Draw(graphic, base.X, base.Y + (float)(i * 16));
            }
            return;
        }
        UpdateVineProgress();
        Depth deep = -0.5f;
        Vec2 prevPos = Position + new Vec2(0f, -4f);
        if (_lowestVine != null && _lowestVine.owner != null)
        {
            prevPos = _lowestVine.owner.Position;
            if (highestVine != null && highestVine._rope.attach2 is Harpoon)
            {
                Graphics.DrawTexturedLine(_beam.texture, Position + new Vec2(0f, -4f), _nodes[0].Position + new Vec2(0f, 2f), Color.White, 1f, deep);
            }
        }
        int index = -1;
        foreach (PhysicsRopeSection s in _nodes)
        {
            deep += 1;
            index++;
            if (index >= _lowestVineSection)
            {
                Vec2 vec = (s.Position - prevPos).Normalized;
                if (index == _nodes.Count - 1)
                {
                    Graphics.DrawTexturedLine(_vineEnd.texture, prevPos, s.Position + vec, Color.White, 1f, deep);
                }
                else
                {
                    Graphics.DrawTexturedLine(_beam.texture, prevPos, s.Position + vec, Color.White, 1f, deep);
                }
                prevPos = s.Position;
            }
        }
    }
}
