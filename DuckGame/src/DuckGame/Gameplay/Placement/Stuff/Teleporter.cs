using System;
using System.Collections.Generic;
using System.Linq;

namespace DuckGame;

[EditorGroup("Stuff")]
public class Teleporter : MaterialThing
{
    public List<WarpLine> warpLines = new List<WarpLine>();

    private Sprite _bottom;

    private Sprite _top;

    private SinWaveManualUpdate _pulse = 0.1f;

    private SinWaveManualUpdate _float = 0.2f;

    private Sprite _arrow;

    public Teleporter _link;

    public EditorProperty<bool> noduck = new EditorProperty<bool>(val: false);

    public EditorProperty<int> teleHeight;

    public EditorProperty<bool> horizontal;

    private Sprite _warpLine;

    private bool _initLinks;

    private List<ITeleport> _teleporting = new List<ITeleport>();

    private List<ITeleport> _teleported = new List<ITeleport>();

    public Vec2 _dir;

    public int direction;

    public bool newVersion = true;

    public Teleporter link => _link;

    public Teleporter(float xpos, float ypos)
        : base(xpos, ypos)
    {
        teleHeight = new EditorProperty<int>(2, this, 1f, 16f, 1f);
        teleHeight.name = "height";
        horizontal = new EditorProperty<bool>(val: false, this);
        Center = new Vec2(8f, 24f);
        collisionSize = new Vec2(6f, 32f);
        collisionOffset = new Vec2(-3f, -24f);
        base.Depth = -0.5f;
        _editorName = "Teleporter";
        editorTooltip = "Place 2 teleporters pointing toward each other and Ducks can transport between them.";
        _editorIcon = new Sprite("teleporterIcon");
        _bottom = new Sprite("teleporterBottom");
        _bottom.CenterOrigin();
        _top = new Sprite("teleporterTop");
        _top.CenterOrigin();
        _arrow = new Sprite("upArrow");
        _arrow.CenterOrigin();
        thickness = 99f;
        _placementCost += 2;
    }

    public override void EditorPropertyChanged(object property)
    {
        UpdateHeight();
    }

    private void UpdateHeight()
    {
        if ((bool)horizontal)
        {
            Center = new Vec2(24f, 8f);
            collisionSize = new Vec2((int)teleHeight * 16, 6f);
            collisionOffset = new Vec2(-8f, -3f);
        }
        else
        {
            Center = new Vec2(8f, 24f);
            collisionSize = new Vec2(6f, (int)teleHeight * 16);
            collisionOffset = new Vec2(-3f, -((int)teleHeight * 16 - 8));
        }
    }

    public override void Initialize()
    {
        if (noduck.value)
        {
            _bottom = new Sprite("littleTeleBottom");
            _bottom.CenterOrigin();
            _top = new Sprite("littleTeleTop");
            _top.CenterOrigin();
        }
        _warpLine = new Sprite("warpLine2");
        UpdateHeight();
        base.Initialize();
    }

    public override void TabRotate()
    {
        if (Keyboard.control)
        {
            direction += 4;
            horizontal.value = !horizontal.value;
        }
        else
        {
            direction++;
        }
        if (direction > 3)
        {
            direction = 0;
        }
    }

    public void InitLinks()
    {
        _initLinks = true;
        Vec2 dir = new Vec2(0f, -1f);
        if (direction == 1)
        {
            dir = new Vec2(0f, 1f);
        }
        else if (direction == 2)
        {
            dir = new Vec2(-1f, 0f);
        }
        else if (direction == 3)
        {
            dir = new Vec2(1f, 0f);
        }
        Vec2 outer;
        if (horizontal.value)
        {
            Vec2 centerCheck = base.rectangle.Center;
            Teleporter tele = Level.CheckRay<Teleporter>(centerCheck + dir * 20f, centerCheck + dir * 5000f, this, out outer);
            if (tele != null)
            {
                _link = tele;
            }
        }
        else
        {
            Vec2 centerCheck2 = Position + new Vec2(0f, 0f - ((float)(int)teleHeight * 16f / 2f - 8f));
            Teleporter tele2 = Level.CheckRay<Teleporter>(centerCheck2 + dir * 20f, centerCheck2 + dir * 5000f, this, out outer);
            if (tele2 != null)
            {
                _link = tele2;
            }
        }
        _dir = dir;
    }

    public override void Update()
    {
        _pulse.Update();
        _float.Update();
        if (!_initLinks)
        {
            InitLinks();
        }
        if (_link == null)
        {
            return;
        }
        IEnumerable<ITeleport> stuff = Level.CheckRectAll<ITeleport>(base.topLeft, base.bottomRight);
        for (int i = 0; i < _teleported.Count; i++)
        {
            ITeleport t = _teleported[i];
            if (!stuff.Contains(t))
            {
                _teleported.RemoveAt(i);
                i--;
            }
        }
        foreach (ITeleport t2 in stuff)
        {
            if (!noduck.value || (!(t2 is Duck) && !(t2 is Ragdoll) && !(t2 is RagdollPart) && !(t2 is TrappedDuck)))
            {
                ITeleport thinger = t2;
                if ((thinger as Thing).owner == null && (thinger as Thing).isServerForObject && !_teleported.Contains(thinger) && !_teleporting.Contains(thinger))
                {
                    _teleporting.Add(thinger);
                }
            }
        }
        int i2;
        for (i2 = 0; i2 < _teleporting.Count; i2++)
        {
            Thing t3 = _teleporting[i2] as Thing;
            _teleporting.RemoveAt(i2);
            for (int j = 0; j < 2; j++)
            {
                Level.Add(SmallSmoke.New(t3.Position.X + Rando.Float(-8, 8), t3.Position.Y + Rando.Float(-8, 8)));
            }
            Vec2 origPos = t3.Position;
            float offsetAdd = 0;
            if (t3 is RagdollPart)
                offsetAdd = 8;
            _link._teleported.Add(t3 as ITeleport);
            if ((int)teleHeight != 2 || (int)_link.teleHeight != 2)
            {
                if (_dir.Y == 0)
                {
                    t3.X = _link.X - (base.X - t3.X);
                    if (!horizontal.value)
                    {
                        if (t3 is PhysicsObject)
                        {
                            if (t3.hSpeed > 0f)
                                t3.X = _link.X + 8;
                            else
                                t3.X = _link.X - 8;
                        }
                    }
                    else if (t3 is PhysicsObject)
                    {
                        if (t3.vSpeed > 0f)
                            t3.Y = _link.Y + (t3.height / 2 + 6 + offsetAdd);
                        else
                            t3.Y = _link.Y - (t3.height / 2 + 6 + offsetAdd);
                    }
                }
                else
                {
                    t3.Y = _link.Y - (base.Y - t3.Y);
                    if (!horizontal.value)
                    {
                        if (t3 is PhysicsObject)
                        {
                            if (t3.hSpeed > 0f)
                            {
                                t3.X = _link.Position.X + 8f;
                            }      
                            else   
                            {      
                                t3.X = _link.Position.X - 8f;
                            }
                        }
                    }
                    else if (t3 is PhysicsObject)
                    {
                        if (t3.vSpeed > 0f)
                        {
                            t3.Y = _link.Position.Y + (t3.height / 2f + 6f + offsetAdd);
                        }      
                        else   
                        {      
                            t3.Y = _link.Position.Y - (t3.height / 2f + 6f + offsetAdd);
                        }
                    }
                }
                if (!horizontal.value)
                {
                    if (t3.bottom > _link.bottom)
                    {
                        t3.bottom = _link.bottom;
                    }
                    if (t3.top < _link.top)
                    {
                        t3.top = _link.top;
                    }
                }
                else
                {
                    if (t3.right > _link.right)
                    {
                        t3.right = _link.right;
                    }
                    if (t3.left < _link.left)
                    {
                        t3.left = _link.left;
                    }
                }
            }
            else
            {
                t3.Position = _link.rectangle.Center - (base.rectangle.Center - t3.Position);
                if (!horizontal.value)
                {
                    if (t3 is RagdollPart)
                    {
                        t3.Y = _link.Position.Y;
                    }
                    if (t3 is PhysicsObject)
                    {
                        if (t3.hSpeed > 0f)
                        {
                            t3.X = _link.Position.X + 8f;
                        }
                        else
                        {
                            t3.X = _link.Position.X - 8f;
                        }
                    }
                }
                else
                {
                    if (t3 is RagdollPart)
                    {
                        t3.X = _link.Position.X;
                    }
                    if (t3 is PhysicsObject)
                    {
                        if (t3.vSpeed > 0f)
                        {
                            t3.Y = _link.Position.Y + 8f;
                        }
                        else
                        {
                            t3.Y = _link.Position.Y - 8f;
                        }
                    }
                }
            }
            for (int k = 0; k < 2; k++)
            {
                Level.Add(SmallSmoke.New(t3.Position.X + Rando.Float(-8f, 8f), t3.Position.Y + Rando.Float(-8f, 8f)));
            }
            i2--;
            Vec2 startPos = origPos;
            Vec2 endPos = t3.Position;
            if (t3 is Duck && (t3 as Duck).sliding)
            {
                startPos.Y += 9f;
                endPos.Y += 9f;
            }
            if (_dir.Y != 0f && !horizontal.value)
            {
                startPos.X = Position.X;
                endPos.X = _link.Position.X;
            }
            float wid = Math.Max((_dir.X != 0f) ? t3.height : t3.width, 8f);
            warpLines.Add(new WarpLine
            {
                start = startPos,
                end = endPos,
                lerp = 0.6f,
                wide = wid
            });
            t3.OnTeleport();
        }
    }

    public override void DrawGlow()
    {
        Color c = Color.Purple;
        if ((bool)noduck)
        {
            c = Color.Yellow;
        }
        foreach (WarpLine l in warpLines)
        {
            Vec2 vec = l.start - l.end;
            Vec2 vec2 = l.end - l.start;
            Graphics.DrawTexturedLine(_warpLine.texture, l.end, l.end + vec * (1f - l.lerp), c * 0.8f, l.wide / 32f, 0.9f);
            Graphics.DrawTexturedLine(_warpLine.texture, l.start + vec2 * l.lerp, l.start, c * 0.8f, l.wide / 32f, 0.9f);
            l.lerp += 0.1f;
        }
        warpLines.RemoveAll((WarpLine v) => v.lerp >= 1f);
    }

    public override void Draw()
    {
        base.Draw();
        if ((bool)horizontal)
        {
            Color c = Color.Purple;
            if ((bool)noduck)
            {
                c = Color.Yellow;
                Graphics.DrawRect(new Vec2(base.X + (float)((int)teleHeight * 16 - 9), base.Y - 2f), new Vec2(base.X - 5f, base.Y + 2f), c * (_pulse.normalized * 0.3f + 0.2f), base.Depth);
            }
            else
            {
                Graphics.DrawRect(new Vec2(base.X + (float)((int)teleHeight * 16 - 9), base.Y - 4f), new Vec2(base.X - 5f, base.Y + 4f), c * (_pulse.normalized * 0.3f + 0.2f), base.Depth);
            }
            _top.AngleDegrees = 90f;
            _bottom.AngleDegrees = 90f;
            _top.Depth = base.Depth + 1;
            _bottom.Depth = base.Depth + 1;
            Graphics.Draw(_top, base.X + (float)((int)teleHeight * 16 - 9), base.Y);
            Graphics.Draw(_bottom, base.X - 5f, base.Y);
            _arrow.Depth = base.Depth + 2;
            _arrow.Alpha = 0.5f;
            if (direction == 0)
            {
                _arrow.AngleDegrees = 0f;
            }
            else if (direction == 1)
            {
                _arrow.AngleDegrees = 180f;
            }
            else if (direction == 2)
            {
                _arrow.AngleDegrees = -90f;
            }
            else if (direction == 3)
            {
                _arrow.AngleDegrees = 90f;
            }
            Graphics.Draw(_arrow, base.X - 8f + (float)((int)teleHeight * 16 / 2) + (float)_float * 2f, base.Y);
        }
        else
        {
            Color c2 = Color.Purple;
            if ((bool)noduck)
            {
                c2 = Color.Yellow;
                Graphics.DrawRect(new Vec2(base.X - 2f, base.Y - (float)((int)teleHeight * 16 - 9)), new Vec2(base.X + 2f, base.Y + 5f), c2 * (_pulse.normalized * 0.3f + 0.2f), base.Depth);
            }
            else
            {
                Graphics.DrawRect(new Vec2(base.X - 4f, base.Y - (float)((int)teleHeight * 16 - 9)), new Vec2(base.X + 4f, base.Y + 5f), c2 * (_pulse.normalized * 0.3f + 0.2f), base.Depth);
            }
            _top.Angle = 0f;
            _bottom.Angle = 0f;
            _top.Depth = base.Depth + 1;
            _bottom.Depth = base.Depth + 1;
            Graphics.Draw(_top, base.X, base.Y - (float)((int)teleHeight * 16 - 9));
            Graphics.Draw(_bottom, base.X, base.Y + 5f);
            _arrow.Depth = base.Depth + 2;
            _arrow.Alpha = 0.5f;
            if (direction == 0)
            {
                _arrow.AngleDegrees = 0f;
            }
            else if (direction == 1)
            {
                _arrow.AngleDegrees = 180f;
            }
            else if (direction == 2)
            {
                _arrow.AngleDegrees = -90f;
            }
            else if (direction == 3)
            {
                _arrow.AngleDegrees = 90f;
            }
            Graphics.Draw(_arrow, base.X, base.Y + 8f - (float)((int)teleHeight * 16 / 2) + (float)_float * 2f);
        }
    }

    public override BinaryClassChunk Serialize()
    {
        BinaryClassChunk binaryClassChunk = base.Serialize();
        binaryClassChunk.AddProperty("direction", direction);
        binaryClassChunk.AddProperty("newVersion", true);
        return binaryClassChunk;
    }

    public override bool Deserialize(BinaryClassChunk node)
    {
        base.Deserialize(node);
        newVersion = node.GetProperty<bool>("newVersion");
        if (!newVersion)
        {
            teleHeight.value = 2;
        }
        newVersion = true;
        direction = node.GetProperty<int>("direction");
        return true;
    }

    public override DXMLNode LegacySerialize()
    {
        DXMLNode dXMLNode = base.LegacySerialize();
        dXMLNode.Add(new DXMLNode("direction", Change.ToString(direction)));
        return dXMLNode;
    }

    public override bool LegacyDeserialize(DXMLNode node)
    {
        base.LegacyDeserialize(node);
        teleHeight.value = 2;
        DXMLNode n = node.Element("direction");
        if (n != null)
        {
            direction = Convert.ToInt32(n.Value);
        }
        return true;
    }

    public override ContextMenu GetContextMenu()
    {
        EditorGroupMenu obj = base.GetContextMenu() as EditorGroupMenu;
        obj.AddItem(new ContextRadio("Up", direction == 0, 0, null, new FieldBinding(this, "direction")));
        obj.AddItem(new ContextRadio("Down", direction == 1, 1, null, new FieldBinding(this, "direction")));
        obj.AddItem(new ContextRadio("Left", direction == 2, 2, null, new FieldBinding(this, "direction")));
        obj.AddItem(new ContextRadio("Right", direction == 3, 3, null, new FieldBinding(this, "direction")));
        return obj;
    }

    public override void EditorUpdate()
    {
        _pulse.Update();
        _float.Update();
        base.EditorUpdate();
    }
}
