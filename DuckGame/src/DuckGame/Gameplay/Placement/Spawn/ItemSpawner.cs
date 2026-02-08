using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace DuckGame;

[EditorGroup("Spawns")]
[BaggedProperty("isInDemo", true)]
[BaggedProperty("previewPriority", true)]
public class ItemSpawner : Thing, IContainAThing, IContainPossibleThings
{
    protected bool _hasContainedItem = true;

    protected SpriteMap _sprite;

    public float _spawnWait;

    public float initialDelay;

    public float spawnTime = 10f;

    public bool spawnOnStart = true;

    public bool randomSpawn;

    public bool keepRandom;

    public int spawnNum = -1;

    private Holdable hoverItem;

    private SinWaveManualUpdate _hoverSin = 0.05f;

    public SpawnerBall _ball1;

    public SpawnerBall _ball2;

    protected int _numSpawned;

    public bool _seated;

    private bool _triedSeating;

    private bool _isClassicSpawner;

    private int _seatingTries;

    private Thing previewThing;

    private Sprite previewSprite;

    private float _bob;

    private List<TypeProbPair> _possible = new List<TypeProbPair>();

    public Type contains { get; set; }

    public Holdable _hoverItem
    {
        get
        {
            return hoverItem;
        }
        set
        {
            SetHoverItem(value);
        }
    }

    public List<TypeProbPair> possible => _possible;

    public override void SetTranslation(Vector2 translation)
    {
        if (_ball1 != null)
        {
            _ball1.SetTranslation(translation);
        }
        if (_ball2 != null)
        {
            _ball2.SetTranslation(translation);
        }
        base.SetTranslation(translation);
    }

    public void PreparePossibilities()
    {
        if (possible.Count > 0)
        {
            contains = MysteryGun.PickType(base.chanceGroup, possible);
        }
    }

    public ItemSpawner(float xpos, float ypos, Type c = null)
        : base(xpos, ypos)
    {
        _sprite = new SpriteMap("gunSpawner", 14, 10);
        graphic = _sprite;
        Center = new Vector2(7f, 0f);
        collisionSize = new Vector2(14f, 2f);
        collisionOffset = new Vector2(-7f, 0f);
        base.Depth = -0.35f;
        contains = c;
        base.hugWalls = WallHug.Floor;
        _placementCost += 4;
        editorTooltip = "Spawns a copy of the specified item after a specified duration.";
    }

    public override void Initialize()
    {
        if (GetType() == typeof(ItemSpawner))
        {
            _isClassicSpawner = true;
        }
        _ball1 = new SpawnerBall(base.X, base.Y - 1f, secondBall: false);
        _ball2 = new SpawnerBall(base.X, base.Y - 1f, secondBall: true);
        Level.Add(_ball1);
        Level.Add(_ball2);
        if (spawnOnStart)
        {
            _spawnWait = spawnTime;
        }
        if (!(Level.current is Editor))
        {
            if (randomSpawn && keepRandom)
            {
                List<Type> things = ItemBox.GetPhysicsObjects(Editor.Placeables);
                contains = things[Rando.Int(things.Count - 1)];
                randomSpawn = false;
            }
            else if (possible.Count > 0 && contains == null)
            {
                PreparePossibilities();
            }
        }
    }

    public void BreakHoverBond()
    {
        if (_hoverItem != null)
        {
            _hoverItem.gravMultiplier = 1f;
            _hoverItem.hoverSpawner = null;
            _hoverItem = null;
        }
    }

    public virtual void SpawnItem()
    {
        _spawnWait = 0f;
        if (Network.isActive && base.isServerForObject)
        {
            Send.Message(new NMItemSpawned(this));
        }
        IReadOnlyPropertyBag containsBag = ContentProperties.GetBag(contains);
        PhysicsObject newThing = ((!Network.isActive || containsBag.GetOrDefault("isOnlineCapable", defaultValue: true)) ? (Editor.CreateThing(contains) as PhysicsObject) : (Activator.CreateInstance(typeof(Pistol), Editor.GetConstructorParameters(typeof(Pistol))) as PhysicsObject));
        if (newThing != null)
        {
            newThing.X = base.X;
            newThing.Y = base.top + (newThing.Y - newThing.bottom) - 6f;
            newThing.vSpeed = -2f;
            newThing.spawnAnimation = true;
            newThing.isSpawned = true;
            newThing.offDir = offDir;
            Level.Add(newThing);
            if (_seated)
            {
                SetHoverItem(newThing as Holdable);
            }
        }
    }

    public virtual void SetHoverItem(Holdable hover)
    {
        if (_hoverItem != hover)
        {
            if (_hoverItem != null)
            {
                _hoverItem.hoverSpawner = null;
                _hoverItem.grounded = false;
            }
            hoverItem = hover;
            if (_hoverItem != null)
            {
                _hoverItem.hoverSpawner = this;
                _hoverItem.grounded = true;
            }
        }
    }

    public void TrySeating()
    {
        if (_seatingTries < 3 && _ball1 != null)
        {
            if (Level.CheckPoint<IPlatform>(Position + new Vector2(0f, 6f)) != null)
            {
                _seated = true;
                _seatingTries = 3;
            }
            else
            {
                _seated = false;
            }
            _seatingTries++;
        }
    }

    public override void EditorUpdate()
    {
        _hoverSin.Update();
        if (contains != null && !randomSpawn && Level.current is Editor && (previewThing == null || previewThing.GetType() != contains))
        {
            previewThing = Editor.GetThing(contains);
            if (previewThing != null)
            {
                previewSprite = previewThing.GeneratePreview(32, 32, transparentBack: true);
            }
        }
        collisionSize = new Vector2(14f, 8f);
        collisionOffset = new Vector2(-7f, -6f);
        base.EditorUpdate();
    }

    public override void Update()
    {
        _hoverSin.Update();
        TrySeating();
        if (_hoverItem == null)
        {
            if (_seated)
            {
                Holdable g = Level.current.NearestThingFilter<Holdable>(Position, (Thing d) => !(d is TeamHat) && (d as Holdable).canPickUp, 16f);
                if (g != null && g.owner == null && g != null && g.canPickUp && Math.Abs(g.hSpeed) + Math.Abs(g.vSpeed) < 2.5f && (!(g is Gun) || (g as Gun).ammo > 0))
                {
                    SetHoverItem(g);
                }
            }
            _ball1.desiredOrbitDistance = 3f;
            _ball2.desiredOrbitDistance = 3f;
            _ball1.desiredOrbitHeight = 1f;
            _ball2.desiredOrbitHeight = 1f;
            if (Level.current.simulatePhysics)
            {
                _spawnWait += 0.0166666f;
            }
        }
        else if (Math.Abs(_hoverItem.hSpeed) + Math.Abs(_hoverItem.vSpeed) > 2f || (_hoverItem.collisionCenter - Position).Length() > 18f || _hoverItem.destroyed || _hoverItem.removeFromLevel || _hoverItem.owner != null || !_hoverItem.visible)
        {
            BreakHoverBond();
        }
        else
        {
            float hoverHeight = 0f - (_hoverItem.bottom - _hoverItem.Y) - 2f + (float)_hoverSin * 2f;
            _hoverItem.Position = Lerp.Vec2Smooth(_hoverItem.Position, Position + new Vector2(0f, hoverHeight), 0.2f);
            _hoverItem.vSpeed = 0f;
            _hoverItem.gravMultiplier = 0f;
            _ball1.desiredOrbitDistance = _hoverItem.collisionSize.X / 2f;
            _ball2.desiredOrbitDistance = _hoverItem.collisionSize.X / 2f;
            _ball1.desiredOrbitHeight = 4f;
            _ball2.desiredOrbitHeight = 4f;
        }
        if (!Network.isServer || (_numSpawned >= spawnNum && spawnNum != -1) || _hoverItem != null || (!(contains != null) && !randomSpawn) || !(_spawnWait >= spawnTime))
        {
            return;
        }
        if (initialDelay > 0f)
        {
            initialDelay -= 0.0166666f;
            return;
        }
        if (randomSpawn)
        {
            List<Type> things = ItemBox.GetPhysicsObjects(Editor.Placeables);
            contains = things[Rando.Int(things.Count - 1)];
        }
        _numSpawned++;
        SpawnItem();
    }

    public override void Draw()
    {
        if (Level.current is Editor)
        {
            TrySeating();
        }
        if (_isClassicSpawner)
        {
            _sprite.frame = ((!_seated) ? 1 : 0) + (keepRandom ? 4 : (randomSpawn ? 2 : 0));
        }
        if (contains != null && !randomSpawn && Level.current is Editor && previewThing != null)
        {
            _bob += 0.05f;
            previewSprite.CenterOrigin();
            previewSprite.Alpha = 0.5f;
            previewSprite.flipH = offDir < 0;
            Graphics.Draw(previewSprite, base.X, base.Y - 8f + (float)Math.Sin(_bob) * 2f);
        }
        if (_isClassicSpawner && (_sprite.frame == 1 || _sprite.frame == 3))
        {
            base.Y -= 2f;
            base.Draw();
            base.Y += 2f;
        }
        else
        {
            base.Draw();
        }
    }

    public override void Terminate()
    {
        Level.Remove(_ball1);
        Level.Remove(_ball2);
    }

    public override BinaryClassChunk Serialize()
    {
        BinaryClassChunk element = base.Serialize();
        if (_hasContainedItem)
        {
            element.AddProperty("contains", Editor.SerializeTypeName(contains));
            element.AddProperty("randomSpawn", randomSpawn);
            element.AddProperty("keepRandom", keepRandom);
        }
        element.AddProperty("possible", MysteryGun.SerializeTypeProb(possible));
        element.AddProperty("spawnTime", spawnTime);
        element.AddProperty("initialDelay", initialDelay);
        element.AddProperty("spawnOnStart", spawnOnStart);
        element.AddProperty("spawnNum", spawnNum);
        return element;
    }

    public override bool Deserialize(BinaryClassChunk node)
    {
        base.Deserialize(node);
        if (_hasContainedItem)
        {
            contains = Editor.DeSerializeTypeName(node.GetProperty<string>("contains"));
            randomSpawn = node.GetProperty<bool>("randomSpawn");
            keepRandom = node.GetProperty<bool>("keepRandom");
        }
        _possible = MysteryGun.DeserializeTypeProb(node.GetProperty<string>("possible"));
        spawnTime = node.GetProperty<float>("spawnTime");
        initialDelay = node.GetProperty<float>("initialDelay");
        spawnOnStart = node.GetProperty<bool>("spawnOnStart");
        spawnNum = node.GetProperty<int>("spawnNum");
        return true;
    }

    public override DXMLNode LegacySerialize()
    {
        DXMLNode element = base.LegacySerialize();
        if (_hasContainedItem)
        {
            element.Add(new DXMLNode("contains", (contains != null) ? contains.AssemblyQualifiedName : ""));
        }
        element.Add(new DXMLNode("spawnTime", Change.ToString(spawnTime)));
        element.Add(new DXMLNode("initialDelay", Change.ToString(initialDelay)));
        element.Add(new DXMLNode("spawnOnStart", Change.ToString(spawnOnStart)));
        if (_hasContainedItem)
        {
            element.Add(new DXMLNode("randomSpawn", Change.ToString(randomSpawn)));
        }
        if (_hasContainedItem)
        {
            element.Add(new DXMLNode("keepRandom", Change.ToString(keepRandom)));
        }
        element.Add(new DXMLNode("spawnNum", Change.ToString(spawnNum)));
        return element;
    }

    public override bool LegacyDeserialize(DXMLNode node)
    {
        base.LegacyDeserialize(node);
        DXMLNode n = null;
        if (_hasContainedItem)
        {
            n = node.Element("contains");
            if (n != null)
            {
                contains = Editor.GetType(n.Value);
            }
        }
        n = node.Element("spawnTime");
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
        if (_hasContainedItem)
        {
            n = node.Element("randomSpawn");
            if (n != null)
            {
                randomSpawn = Convert.ToBoolean(n.Value);
            }
            n = node.Element("keepRandom");
            if (n != null)
            {
                keepRandom = Convert.ToBoolean(n.Value);
            }
        }
        n = node.Element("spawnNum");
        if (n != null)
        {
            spawnNum = Convert.ToInt32(n.Value);
        }
        return true;
    }

    public override ContextMenu GetContextMenu()
    {
        FieldBinding binding = new FieldBinding(this, "contains");
        EditorGroupMenu menu = base.GetContextMenu() as EditorGroupMenu;
        menu.AddItem(new ContextSlider("Delay", null, new FieldBinding(this, "spawnTime", 0.25f, 100f)));
        menu.AddItem(new ContextSlider("Initial Delay", null, new FieldBinding(this, "initialDelay", 0f, 100f)));
        menu.AddItem(new ContextCheckBox("Start Spawned", null, new FieldBinding(this, "spawnOnStart")));
        if (_hasContainedItem)
        {
            menu.AddItem(new ContextCheckBox("Random", null, new FieldBinding(this, "randomSpawn")));
            menu.AddItem(new ContextCheckBox("Keep Random", null, new FieldBinding(this, "keepRandom")));
        }
        menu.AddItem(new ContextSlider("Number", null, new FieldBinding(this, "spawnNum", -1f, 100f), 1f, "INF"));
        if (_hasContainedItem)
        {
            EditorGroupMenu contain = new EditorGroupMenu(menu);
            contain.InitializeGroups(new EditorGroup(typeof(PhysicsObject)), binding);
            contain.text = "Contains";
            menu.AddItem(contain);
        }
        EditorGroupMenu possib = new EditorGroupMenu(menu);
        possib.InitializeGroups(new EditorGroup(typeof(PhysicsObject)), new FieldBinding(this, "possible"));
        possib.text = "Possible";
        menu.AddItem(possib);
        return menu;
    }

    public override void DrawHoverInfo()
    {
        if (possible.Count > 0)
        {
            float yOff = 0f;
            {
                foreach (TypeProbPair p in possible)
                {
                    if (p.probability > 0f)
                    {
                        Color c = Color.White;
                        c = ((p.probability == 0f) ? Color.DarkGray : ((p.probability < 0.3f) ? Colors.DGRed : ((!(p.probability < 0.7f)) ? Color.Green : Color.Orange)));
                        string s = p.type.Name + ": " + p.probability.ToString("0.000");
                        Graphics.DrawString(s, Position + new Vector2((0f - Graphics.GetStringWidth(s, thinButtons: false, 0.5f)) / 2f, 0f - (16f + yOff)), c, 0.9f, null, 0.5f);
                        yOff += 4f;
                    }
                }
                return;
            }
        }
        string containString = "EMPTY";
        if (contains != null)
        {
            containString = contains.Name;
        }
        Graphics.DrawString(containString, Position + new Vector2((0f - Graphics.GetStringWidth(containString)) / 2f, -16f), Color.White, 0.9f);
    }

    public override string GetDetailsString()
    {
        string containString = "EMPTY";
        if (contains != null)
        {
            containString = contains.Name;
        }
        if (contains == null && spawnTime == 10f)
        {
            return base.GetDetailsString();
        }
        return base.GetDetailsString() + "Contains: " + containString + "\nTime: " + spawnTime.ToString("0.00", CultureInfo.InvariantCulture);
    }
}
