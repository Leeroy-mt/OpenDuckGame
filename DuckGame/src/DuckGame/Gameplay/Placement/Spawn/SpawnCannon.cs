using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace DuckGame;

[EditorGroup("Spawns")]
[BaggedProperty("isInDemo", false)]
[BaggedProperty("previewPriority", false)]
public class SpawnCannon : ItemSpawner, IWirePeripheral, ISequenceItem
{
    public EditorProperty<int> bing;

    public EditorProperty<bool> showClock;

    public EditorProperty<int> cannonColor;

    public float fireDirection;

    public float firePower = 5f;

    private float _startupDelay;

    private Sprite _arrowHead;

    private bool initializedWired;

    private bool wired;

    private bool wasPulse;

    private bool _running;

    private int beeps;

    private PhysicsObject _hoverThing;

    private Vector2 _scaleLerp = Vector2.One;

    public float direction => fireDirection + (flipHorizontal ? 180f : 0f);

    public override void EditorPropertyChanged(object property)
    {
        if (showClock.value)
        {
            _sprite = new SpriteMap("cannonTimer", 18, 18);
        }
        else
        {
            _sprite = new SpriteMap("cannon", 18, 18);
        }
        graphic = _sprite;
    }

    public SpawnCannon(float xpos, float ypos, Type c = null)
        : base(xpos, ypos)
    {
        bing = new EditorProperty<int>(0, this, 0f, 240f, 1f, "none");
        bing._tooltip = "If set, this cannon will BING this many frames before it activates.";
        showClock = new EditorProperty<bool>(val: false, this);
        cannonColor = new EditorProperty<int>(0, this, 0f, 3f, 1f);
        _arrowHead = new Sprite("arrowHead", new Vector2(3.5f, 8f));
        _sprite = new SpriteMap("cannon", 18, 18);
        graphic = _sprite;
        Center = new Vector2(7f, 9f);
        collisionSize = new Vector2(8f, 8f);
        collisionOffset = new Vector2(-6f, -6f);
        base.Depth = 0.8f;
        base.contains = c;
        base.hugWalls = WallHug.None;
        _placementCost += 4;
        editorTooltip = "Shoots the specified item in the specified direction after the specified delay.";
        base.sequence = new SequenceItem(this);
        base.sequence.type = SequenceItemType.Activator;
    }

    public override void OnSequenceActivate()
    {
        SFX.Play("basketball", 0.8f, Rando.Float(0.2f, 0.4f));
        base.Scale = new Vector2(2f, 2f);
        _running = true;
    }

    public override void Initialize()
    {
        if (spawnOnStart)
        {
            _spawnWait = spawnTime;
        }
        _startupDelay = initialDelay;
    }

    public void Spawn()
    {
        if (!Network.isServer && !wasPulse)
        {
            _spawnWait = 0f;
            _numSpawned++;
            return;
        }
        if (base.sequence != null)
        {
            if (base.sequence.waitTillOrder)
            {
                _running = false;
            }
            base.sequence.Finished();
        }
        base.ScaleX = 2f;
        base.ScaleY = 0.5f;
        if (!initializedWired)
        {
            WireTileset w = Level.current.NearestThing<WireTileset>(Position);
            if (w != null && (w.Position - Position).Length() < 1f)
            {
                wired = true;
            }
            initializedWired = true;
        }
        if (!wasPulse && wired)
        {
            return;
        }
        if (randomSpawn && keepRandom)
        {
            List<Type> things = ItemBox.GetPhysicsObjects(Editor.Placeables);
            base.contains = things[Rando.Int(things.Count - 1)];
        }
        else if (base.possible.Count > 0)
        {
            Type highest = MysteryGun.PickType(base.chanceGroup, base.possible);
            if (highest != null)
            {
                base.contains = highest;
            }
        }
        _spawnWait = 0f;
        _numSpawned++;
        if (!(base.contains == null) && Editor.CreateThing(base.contains) is PhysicsObject newThing)
        {
            Vector2 move = Maths.AngleToVec(Maths.DegToRad(direction)) * firePower;
            newThing.Position = Position + Vector2.Normalize(move) * 8f;
            newThing.hSpeed = move.X;
            newThing.vSpeed = move.Y;
            Level.Add(newThing);
            newThing.Ejected(this);
            Level.Add(SmallSmoke.New(newThing.X, newThing.Y));
            Level.Add(SmallSmoke.New(newThing.X, newThing.Y));
            SFX.Play("netGunFire", Rando.Float(0.9f, 1f), Rando.Float(-0.1f, 0.1f));
            if (newThing is Equipment)
            {
                (newThing as Equipment).autoEquipTime = 0.5f;
            }
            if (newThing is ChokeCollar)
            {
                (newThing as ChokeCollar).ball.hSpeed = newThing.hSpeed;
                (newThing as ChokeCollar).ball.vSpeed = newThing.vSpeed;
            }
            if (newThing is Sword)
            {
                (newThing as Sword)._wasLifted = true;
                (newThing as Sword)._framesExisting = 16;
            }
        }
    }

    public override void Update()
    {
        base.Scale = Lerp.Vec2Smooth(base.Scale, Vector2.One, 0.2f);
        if ((base.sequence == null || !base.sequence.waitTillOrder || _running) && (_numSpawned < spawnNum || spawnNum == -1))
        {
            if (Level.current.simulatePhysics)
            {
                _spawnWait += 0.0166666f;
            }
            if (bing.value > 0)
            {
                float remainingTime = Math.Max(spawnTime - _spawnWait, 0f) + initialDelay;
                float bingTime = (float)bing.value * Maths.IncFrameTimer();
                float gap = (bingTime - remainingTime) / bingTime;
                if (beeps == 0 && gap > 0f)
                {
                    SFX.Play("singleBeep");
                    beeps++;
                }
                if (beeps == 1 && gap > 1f / 3f)
                {
                    SFX.Play("singleBeep");
                    beeps++;
                }
                if (beeps == 2 && gap > 2f / 3f)
                {
                    SFX.Play("singleBeep");
                    beeps++;
                }
            }
            if (Level.current.simulatePhysics && _spawnWait >= spawnTime)
            {
                if (initialDelay > 0f)
                {
                    initialDelay -= 0.0166666f;
                }
                else
                {
                    if (bing.value > 0)
                    {
                        SFX.Play("bing");
                    }
                    beeps = 0;
                    Spawn();
                    _startupDelay = 0f;
                    initialDelay = 0f;
                }
            }
        }
        base.AngleDegrees = 0f - direction;
    }

    public void Pulse(int type, WireTileset wire)
    {
        wasPulse = true;
        Spawn();
        wasPulse = false;
        SFX.Play("click");
    }

    public override void Terminate()
    {
    }

    public override BinaryClassChunk Serialize()
    {
        BinaryClassChunk binaryClassChunk = base.Serialize();
        binaryClassChunk.AddProperty("fireDirection", fireDirection);
        binaryClassChunk.AddProperty("firePower", firePower);
        return binaryClassChunk;
    }

    public override bool Deserialize(BinaryClassChunk node)
    {
        base.Deserialize(node);
        fireDirection = node.GetProperty<float>("fireDirection");
        firePower = node.GetProperty<float>("firePower");
        return true;
    }

    public override DXMLNode LegacySerialize()
    {
        DXMLNode dXMLNode = base.LegacySerialize();
        dXMLNode.Add(new DXMLNode("fireDirection", Change.ToString(fireDirection)));
        dXMLNode.Add(new DXMLNode("firePower", Change.ToString(firePower)));
        return dXMLNode;
    }

    public override bool LegacyDeserialize(DXMLNode node)
    {
        base.LegacyDeserialize(node);
        DXMLNode n = node.Element("fireDirection");
        if (n != null)
        {
            fireDirection = Convert.ToSingle(n.Value);
        }
        n = node.Element("firePower");
        if (n != null)
        {
            firePower = Convert.ToSingle(n.Value);
        }
        return true;
    }

    public override ContextMenu GetContextMenu()
    {
        EditorGroupMenu obj = base.GetContextMenu() as EditorGroupMenu;
        obj.AddItem(new ContextSlider("Angle", null, new FieldBinding(this, "fireDirection", 0f, 360f), 1f));
        obj.AddItem(new ContextSlider("Power", null, new FieldBinding(this, "firePower", 1f, 20f)));
        return obj;
    }

    public override void DrawHoverInfo()
    {
        string containString = "EMPTY";
        if (base.contains != null)
        {
            containString = base.contains.Name;
        }
        Graphics.DrawString(containString, Position + new Vector2((0f - Graphics.GetStringWidth(containString)) / 2f, -16f), Color.White, 0.9f);
        if (!(base.contains != null))
        {
            return;
        }
        if (_hoverThing == null || _hoverThing.GetType() != base.contains)
        {
            _hoverThing = Editor.CreateThing(base.contains) as PhysicsObject;
        }
        if (_hoverThing != null)
        {
            Vector2 move = Maths.AngleToVec(Maths.DegToRad(direction)) * firePower;
            _hoverThing.Position = Position + Vector2.Normalize(move) * 8f;
            _hoverThing.hSpeed = move.X;
            _hoverThing.vSpeed = move.Y;
            SFX.enabled = false;
            Vector2 lastPos = _hoverThing.Position;
            for (int i = 0; i < 100; i++)
            {
                _hoverThing.UpdatePhysics();
                Graphics.DrawLine(lastPos, _hoverThing.Position, Color.Red, 2f, 1f);
                lastPos = _hoverThing.Position;
            }
            SFX.enabled = true;
        }
    }

    public override void Draw()
    {
        _sprite.frame = cannonColor.value;
        base.ScaleX = 1f;
        base.ScaleY = 1f;
        float normalizedTime = _spawnWait / (spawnTime + _startupDelay);
        if (showClock.value)
        {
            float fille = (flipHorizontal ? normalizedTime : (0f - normalizedTime)) * ((float)Math.PI * 2f);
            if (flipHorizontal)
            {
                fille += (float)Math.PI;
            }
            Vector2 p = Offset(new Vector2(0f, 0f));
            Vector2 pointer = Offset(Maths.AngleToVec(fille) * 3f);
            Graphics.DrawLine(p, pointer, Color.Black, 1f, base.Depth + 2);
            Vector2 head = Offset(Maths.AngleToVec(fille) * 2f);
            _arrowHead.Angle = (flipHorizontal ? fille : (0f - fille)) + Angle + (float)Math.PI * (flipHorizontal ? (-0.5f) : 0.5f);
            _arrowHead.Scale = new Vector2(0.5f, 0.5f);
            Graphics.Draw(_arrowHead, head.X, head.Y, base.Depth + 2);
        }
        normalizedTime = Maths.Clamp(normalizedTime, 0f, 1f);
        if (normalizedTime > 0.8f && !(Level.current is Editor))
        {
            base.ScaleX = 1f - (normalizedTime - 0.8f) * 2f;
            base.ScaleY = 1f + (normalizedTime - 0.8f) * 4f;
        }
        base.AngleDegrees = 0f - direction;
        base.Draw();
    }
}
