using Microsoft.Xna.Framework;
using System;

namespace DuckGame;

[EditorGroup("Spawns")]
[BaggedProperty("isInDemo", false)]
public class LaserSpawner : Thing
{
    protected float _spawnWait;

    public float initialDelay;

    public float spawnTime = 10f;

    public bool spawnOnStart = true;

    public int spawnNum = -1;

    protected int _numSpawned;

    public float fireDirection;

    public float firePower = 1f;

    public float direction => fireDirection + (flipHorizontal ? 180f : 0f);

    public LaserSpawner(float xpos, float ypos, Type c = null)
        : base(xpos, ypos)
    {
        graphic = new Sprite("laserSpawner");
        Center = new Vector2(8f, 8f);
        collisionSize = new Vector2(12f, 12f);
        collisionOffset = new Vector2(-6f, -6f);
        base.Depth = 0.99f;
        base.hugWalls = WallHug.None;
        _visibleInGame = false;
        editorTooltip = "Spawns Quad Laser bullets in the specified direction.";
    }

    public override void Initialize()
    {
        if (spawnOnStart)
        {
            _spawnWait = spawnTime;
        }
    }

    public override void Update()
    {
        if (Level.current.simulatePhysics)
        {
            _spawnWait += 0.0166666f;
        }
        if (Level.current.simulatePhysics && Network.isServer && (_numSpawned < spawnNum || spawnNum == -1) && _spawnWait >= spawnTime)
        {
            if (initialDelay > 0f)
            {
                initialDelay -= 0.0166666f;
            }
            else
            {
                Vector2 move = Maths.AngleToVec(Maths.DegToRad(direction)) * firePower;
                Vector2 spawn = Position - Vector2.Normalize(move) * 16f;
                Level.Add(new QuadLaserBullet(spawn.X, spawn.Y, move));
                _spawnWait = 0f;
                _numSpawned++;
            }
        }
        base.AngleDegrees = 0f - direction;
    }

    public override void Terminate()
    {
    }

    public override BinaryClassChunk Serialize()
    {
        BinaryClassChunk binaryClassChunk = base.Serialize();
        binaryClassChunk.AddProperty("spawnTime", spawnTime);
        binaryClassChunk.AddProperty("initialDelay", initialDelay);
        binaryClassChunk.AddProperty("spawnOnStart", spawnOnStart);
        binaryClassChunk.AddProperty("spawnNum", spawnNum);
        binaryClassChunk.AddProperty("fireDirection", fireDirection);
        binaryClassChunk.AddProperty("firePower", firePower);
        return binaryClassChunk;
    }

    public override bool Deserialize(BinaryClassChunk node)
    {
        base.Deserialize(node);
        spawnTime = node.GetProperty<float>("spawnTime");
        initialDelay = node.GetProperty<float>("initialDelay");
        spawnOnStart = node.GetProperty<bool>("spawnOnStart");
        spawnNum = node.GetProperty<int>("spawnNum");
        fireDirection = node.GetProperty<float>("fireDirection");
        firePower = node.GetProperty<float>("firePower");
        return true;
    }

    public override DXMLNode LegacySerialize()
    {
        DXMLNode dXMLNode = base.LegacySerialize();
        dXMLNode.Add(new DXMLNode("spawnTime", Change.ToString(spawnTime)));
        dXMLNode.Add(new DXMLNode("initialDelay", Change.ToString(initialDelay)));
        dXMLNode.Add(new DXMLNode("spawnOnStart", Change.ToString(spawnOnStart)));
        dXMLNode.Add(new DXMLNode("spawnNum", Change.ToString(spawnNum)));
        dXMLNode.Add(new DXMLNode("fireDirection", Change.ToString(fireDirection)));
        dXMLNode.Add(new DXMLNode("firePower", Change.ToString(firePower)));
        return dXMLNode;
    }

    public override bool LegacyDeserialize(DXMLNode node)
    {
        base.LegacyDeserialize(node);
        DXMLNode n = node.Element("spawnTime");
        if (n != null)
        {
            spawnTime = Change.ToSingle(n.Value);
        }
        n = node.Element("initialDelay");
        if (n != null)
        {
            initialDelay = Change.ToSingle(n.Value);
        }
        n = node.Element("spawnOnStart");
        if (n != null)
        {
            spawnOnStart = Convert.ToBoolean(n.Value);
        }
        n = node.Element("spawnNum");
        if (n != null)
        {
            spawnNum = Convert.ToInt32(n.Value);
        }
        n = node.Element("fireDirection");
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
        obj.AddItem(new ContextSlider("Delay", null, new FieldBinding(this, "spawnTime", 1f, 100f)));
        obj.AddItem(new ContextSlider("Initial Delay", null, new FieldBinding(this, "initialDelay", 0f, 100f)));
        obj.AddItem(new ContextCheckBox("Start Spawned", null, new FieldBinding(this, "spawnOnStart")));
        obj.AddItem(new ContextSlider("Number", null, new FieldBinding(this, "spawnNum", -1f, 100f), 1f, "INF"));
        obj.AddItem(new ContextSlider("Angle", null, new FieldBinding(this, "fireDirection", 0f, 360f), 1f));
        obj.AddItem(new ContextSlider("Power", null, new FieldBinding(this, "firePower", 1f, 20f)));
        return obj;
    }

    public override void DrawHoverInfo()
    {
        Vector2 move = Maths.AngleToVec(Maths.DegToRad(direction)) * (firePower * 5f);
        Graphics.DrawLine(Position, Position + move, Color.Red, 2f, 1f);
    }

    public override void Draw()
    {
        base.AngleDegrees = 0f - direction;
        base.Draw();
    }
}
