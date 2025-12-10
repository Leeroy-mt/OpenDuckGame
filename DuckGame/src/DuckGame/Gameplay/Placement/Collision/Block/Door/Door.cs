using System;
using System.Collections.Generic;

namespace DuckGame;

[EditorGroup("Stuff|Doors")]
public class Door : Block, IPlatform, IDontMove, ISequenceItem
{
    public StateBinding _hitPointsBinding = new StateBinding("_hitPoints");

    public StateBinding _openBinding = new StateBinding("_open");

    public StateBinding _openForceBinding = new StateBinding("_openForce");

    public StateBinding _jiggleBinding = new StateBinding("_jiggle");

    public StateBinding _jamBinding = new StateBinding("_jam");

    public StateBinding _damageMultiplierBinding = new StateBinding("damageMultiplier");

    public StateBinding _doorInstanceBinding = new StateBinding("_doorInstance");

    public StateBinding _doorStateBinding = new DoorFlagBinding();

    private DoorOffHinges _doorInstanceInternal;

    public float damageMultiplier = 1f;

    protected SpriteMap _sprite;

    public bool landed = true;

    public bool locked;

    public bool _lockDoor;

    public float _open;

    public float _openForce;

    private Vec2 _topLeft;

    private Vec2 _topRight;

    private Vec2 _bottomLeft;

    private Vec2 _bottomRight;

    private bool _cornerInit;

    public bool _jammed;

    public float _jiggle;

    public bool _didJiggle;

    public new bool _initialized;

    public float colWide = 6f;

    public float _jam = 1f;

    private Dictionary<Mine, float> _mines = new Dictionary<Mine, float>();

    private Sprite _lock;

    private bool _opened;

    private SpriteMap _key;

    public DoorFrame _frame;

    private bool _fucked;

    private List<PhysicsObject> _coll;

    public EditorProperty<bool> objective;

    protected bool secondaryFrame;

    private bool _lockedSprite;

    public bool networkUnlockMessage;

    private bool didUnlock;

    private bool prevLocked;

    private List<Mine> _removeMines = new List<Mine>();

    public DoorOffHinges _doorInstance
    {
        get
        {
            return _doorInstanceInternal;
        }
        set
        {
            _doorInstanceInternal = value;
        }
    }

    public override void SetTranslation(Vec2 translation)
    {
        if (_frame != null)
        {
            _frame.SetTranslation(translation);
        }
        base.SetTranslation(translation);
    }

    public override void EditorPropertyChanged(object property)
    {
        base.sequence.isValid = objective.value;
    }

    public Door(float xpos, float ypos)
        : base(xpos, ypos)
    {
        objective = new EditorProperty<bool>(val: false, this);
        _maxHealth = 50f;
        _hitPoints = 50f;
        _sprite = new SpriteMap("door", 32, 32);
        graphic = _sprite;
        center = new Vec2(16f, 25f);
        collisionSize = new Vec2(6f, 32f);
        collisionOffset = new Vec2(-3f, -25f);
        base.depth = -0.5f;
        _editorName = "Door";
        thickness = 2f;
        _lock = new Sprite("lock");
        _lock.CenterOrigin();
        _impactThreshold = 0f;
        _key = new SpriteMap("keyInDoor", 16, 16);
        _key.center = new Vec2(2f, 8f);
        _canFlip = false;
        physicsMaterial = PhysicsMaterial.Wood;
        base.sequence = new SequenceItem(this);
        base.sequence.type = SequenceItemType.Goody;
        _placementCost += 6;
        _coll = new List<PhysicsObject>();
        editorTooltip = "Your basic door type door. Blocks some projectiles. If locked, needs a key to open.";
    }

    public override void Initialize()
    {
        base.sequence.isValid = objective.value;
        _lockDoor = locked;
        if (_lockDoor)
        {
            _sprite = new SpriteMap("lockDoor", 32, 32);
            graphic = _sprite;
            _lockedSprite = true;
        }
        else
        {
            _frame = new DoorFrame(base.x, base.y - 1f, secondaryFrame);
            Level.Add(_frame);
        }
    }

    public override void Terminate()
    {
        if (_hitPoints > 5f && !Network.isActive)
        {
            Level.Remove(_frame);
            _frame = null;
        }
        base.Terminate();
    }

    protected override bool OnDestroy(DestroyType type = null)
    {
        if (_lockDoor || base._destroyed || !base.isServerForObject)
        {
            return false;
        }
        _hitPoints = 0f;
        Level.Remove(this);
        if (base.sequence != null && base.sequence.isValid)
        {
            base.sequence.Finished();
            if (ChallengeLevel.running)
            {
                ChallengeLevel.goodiesGot++;
            }
        }
        DoorOffHinges door = null;
        if (Network.isActive)
        {
            if (_doorInstance != null)
            {
                door = _doorInstance;
                door.visible = true;
                door.active = true;
                door.solid = true;
                Thing.Fondle(this, DuckNetwork.localConnection);
                Thing.Fondle(door, DuckNetwork.localConnection);
            }
        }
        else
        {
            door = new DoorOffHinges(base.x, base.y - 8f, secondaryFrame);
        }
        if (door != null)
        {
            if (type is DTShot { bullet: not null } shot)
            {
                door.hSpeed = shot.bullet.travelDirNormalized.x * 2f;
                door.vSpeed = shot.bullet.travelDirNormalized.y * 2f - 1f;
                door.offDir = (sbyte)((shot.bullet.travelDirNormalized.x > 0f) ? 1 : (-1));
            }
            else
            {
                door.hSpeed = (float)offDir * 2f;
                door.vSpeed = -2f;
                door.offDir = offDir;
            }
            if (!Network.isActive)
            {
                Level.Add(door);
                door.MakeEffects();
            }
        }
        return true;
    }

    public override bool Hit(Bullet bullet, Vec2 hitPos)
    {
        if (bullet.isLocal)
        {
            Thing.Fondle(this, DuckNetwork.localConnection);
        }
        if (_hitPoints <= 0f)
        {
            return base.Hit(bullet, hitPos);
        }
        hitPos -= bullet.travelDirNormalized;
        if (physicsMaterial == PhysicsMaterial.Wood)
        {
            for (int i = 0; (float)i < 1f + damageMultiplier / 2f; i++)
            {
                WoodDebris woodDebris = WoodDebris.New(hitPos.x, hitPos.y);
                woodDebris.hSpeed = (0f - bullet.travelDirNormalized.x) * 2f * (Rando.Float(1f) + 0.3f);
                woodDebris.vSpeed = (0f - bullet.travelDirNormalized.y) * 2f * (Rando.Float(1f) + 0.3f) - Rando.Float(2f);
                Level.Add(woodDebris);
            }
            SFX.Play("woodHit");
        }
        if (base.isServerForObject && bullet.isLocal)
        {
            _hitPoints -= damageMultiplier * 4f;
            damageMultiplier += 1f;
            if (_hitPoints <= 0f && !destroyed)
            {
                Destroy(new DTShot(bullet));
            }
        }
        return base.Hit(bullet, hitPos);
    }

    public override void ExitHit(Bullet bullet, Vec2 exitPos)
    {
        exitPos += bullet.travelDirNormalized;
        for (int i = 0; (float)i < 1f + damageMultiplier / 2f; i++)
        {
            WoodDebris woodDebris = WoodDebris.New(exitPos.x, exitPos.y);
            woodDebris.hSpeed = bullet.travelDirNormalized.x * 3f * (Rando.Float(1f) + 0.3f);
            woodDebris.vSpeed = bullet.travelDirNormalized.y * 3f * (Rando.Float(1f) + 0.3f) - (-1f + Rando.Float(2f));
            Level.Add(woodDebris);
        }
    }

    public override void OnSoftImpact(MaterialThing with, ImpactedFrom from)
    {
        if (with.isServerForObject && locked && with is Key)
        {
            UnlockDoor(with as Key);
        }
        base.OnSoftImpact(with, from);
    }

    public void UnlockDoor(Key with)
    {
        if (locked && with.isServerForObject)
        {
            if (Network.isActive)
            {
                Thing.ExtraFondle(this, with.connection);
                Send.Message(new NMUnlockDoor(this));
                networkUnlockMessage = true;
            }
            locked = false;
            if (with.owner is Duck d)
            {
                RumbleManager.AddRumbleEvent(d.profile, new RumbleEvent(RumbleIntensity.Kick, RumbleDuration.Pulse, RumbleFalloff.None));
                d.ThrowItem();
            }
            Level.Remove(with);
            if (!Network.isActive)
            {
                DoUnlock(with.position);
            }
        }
    }

    public void DoUnlock(Vec2 keyPos)
    {
        SFX.Play("deedleBeep");
        Level.Add(SmallSmoke.New(keyPos.x, keyPos.y));
        for (int i = 0; i < 3; i++)
        {
            Level.Add(SmallSmoke.New(base.x + Rando.Float(-3f, 3f), base.y + Rando.Float(-3f, 3f)));
        }
        didUnlock = true;
    }

    public override void Update()
    {
        if (_doorInstance == null && Network.isActive && base.isServerForObject)
        {
            _doorInstance = new DoorOffHinges(base.x, base.y - 8f, secondaryFrame);
            _doorInstance.active = false;
            _doorInstance.visible = false;
            _doorInstance.solid = false;
            Level.Add(_doorInstance);
        }
        if (!_lockDoor && locked)
        {
            _sprite = new SpriteMap("lockDoor", 32, 32);
            graphic = _sprite;
            _lockedSprite = true;
            _lockDoor = true;
        }
        if (networkUnlockMessage)
        {
            locked = false;
        }
        if (Network.isActive && !locked && prevLocked && !didUnlock)
        {
            DoUnlock(position);
        }
        prevLocked = locked;
        if (_lockDoor)
        {
            _hitPoints = 100f;
            physicsMaterial = PhysicsMaterial.Metal;
            thickness = 4f;
        }
        if (!_fucked && _hitPoints < _maxHealth / 2f)
        {
            _sprite = new SpriteMap(secondaryFrame ? "flimsyDoorDamaged" : "doorFucked", 32, 32);
            graphic = _sprite;
            _fucked = true;
        }
        if (!_cornerInit)
        {
            _topLeft = base.topLeft;
            _topRight = base.topRight;
            _bottomLeft = base.bottomLeft;
            _bottomRight = base.bottomRight;
            _cornerInit = true;
        }
        base.Update();
        if (damageMultiplier > 1f)
        {
            damageMultiplier -= 0.2f;
        }
        else
        {
            damageMultiplier = 1f;
        }
        _removeMines.Clear();
        foreach (KeyValuePair<Mine, float> val in _mines)
        {
            if ((val.Value < 0f && _open > val.Value) || (val.Value >= 0f && _open < val.Value))
            {
                val.Key.addWeight = 0f;
                _removeMines.Add(val.Key);
            }
            else
            {
                val.Key.addWeight = 3f;
            }
        }
        foreach (Mine m in _removeMines)
        {
            _mines.Remove(m);
        }
        bool keepOpen = false;
        PhysicsObject jamThing = null;
        if (_open < 0.9f && _open > -0.9f)
        {
            bool slamOpen = false;
            Thing hit = Level.CheckRectFilter(_topLeft - new Vec2(18f, 0f), _bottomRight + new Vec2(18f, 0f), (Duck d) => !(d is TargetDuck));
            if (hit == null)
            {
                hit = Level.CheckRectFilter(_topLeft - new Vec2(32f, 0f), _bottomRight + new Vec2(32f, 0f), (Duck d) => !(d is TargetDuck) && Math.Abs(d.hSpeed) > 4f);
                slamOpen = true;
            }
            if (hit != null)
            {
                (hit as Duck).Fondle(this);
                if (hit.x < base.x)
                {
                    _coll.Clear();
                    Level.CheckRectAll(_topRight, _bottomRight + new Vec2(10f, 0f), _coll);
                    bool canOpen = true;
                    _jam = 1f;
                    foreach (PhysicsObject thing in _coll)
                    {
                        if (thing is TeamHat || thing is Duck || thing.weight <= 3f || thing.owner != null || (thing is Holdable && (thing as Holdable).hoverSpawner != null))
                        {
                            continue;
                        }
                        if (thing is RagdollPart)
                        {
                            Fondle(thing);
                            thing.hSpeed = 2f;
                            continue;
                        }
                        float jamVal = Maths.Clamp((thing.left - _bottomRight.x) / 14f, 0f, 1f);
                        if (jamVal < 0.1f)
                        {
                            jamVal = 0.1f;
                        }
                        if (!(_jam > jamVal))
                        {
                            continue;
                        }
                        if (_open != 0f && thing is Gun)
                        {
                            if (thing is Mine { pin: false } m2 && !_mines.ContainsKey(m2))
                            {
                                _mines[m2] = _open;
                            }
                        }
                        else
                        {
                            _jam = jamVal;
                            jamThing = thing;
                        }
                    }
                    _coll.Clear();
                    if (locked)
                    {
                        _jam = 0.1f;
                        if (!_didJiggle)
                        {
                            _jiggle = 1f;
                            _didJiggle = true;
                        }
                    }
                    if (canOpen)
                    {
                        if (slamOpen)
                        {
                            _openForce += 0.25f;
                        }
                        else
                        {
                            _openForce += 0.08f;
                        }
                    }
                }
                else
                {
                    _coll.Clear();
                    Level.CheckRectAll(_topLeft - new Vec2(10f, 0f), _bottomLeft, _coll);
                    bool canOpen2 = true;
                    _jam = -1f;
                    foreach (PhysicsObject thing2 in _coll)
                    {
                        if (thing2 is TeamHat || thing2 is Duck || thing2.weight <= 3f || thing2.owner != null || (thing2 is Holdable && (thing2 as Holdable).hoverSpawner != null))
                        {
                            continue;
                        }
                        if (thing2 is RagdollPart)
                        {
                            Fondle(thing2);
                            thing2.hSpeed = -2f;
                            continue;
                        }
                        float jamVal2 = Maths.Clamp((thing2.right - base.left) / 14f, -1f, 0f);
                        if (jamVal2 > -0.1f)
                        {
                            jamVal2 = -0.1f;
                        }
                        if (!(_jam < jamVal2))
                        {
                            continue;
                        }
                        if (_open != 0f && thing2 is Gun)
                        {
                            if (thing2 is Mine { pin: false } m3 && !_mines.ContainsKey(m3))
                            {
                                _mines[m3] = _open;
                            }
                        }
                        else
                        {
                            _jam = jamVal2;
                            jamThing = thing2;
                        }
                    }
                    _coll.Clear();
                    if (locked)
                    {
                        _jam = -0.1f;
                        if (!_didJiggle)
                        {
                            _jiggle = 1f;
                            _didJiggle = true;
                        }
                    }
                    if (canOpen2)
                    {
                        if (slamOpen)
                        {
                            _openForce -= 0.25f;
                        }
                        else
                        {
                            _openForce -= 0.08f;
                        }
                    }
                }
            }
            else
            {
                _didJiggle = false;
            }
        }
        _coll.Clear();
        Level.CheckRectAll(_topLeft - new Vec2(18f, 0f), _bottomRight + new Vec2(18f, 0f), _coll);
        foreach (PhysicsObject hit2 in _coll)
        {
            if (hit2 is TeamHat || (!(hit2 is Duck) && _jammed) || (hit2 is Holdable && !(hit2 is Mine) && !(hit2 as Holdable).canPickUp) || !hit2.solid)
            {
                continue;
            }
            if (!(hit2 is Duck) && weight < 3f)
            {
                if (_open < -0f)
                {
                    Fondle(hit2);
                    hit2.hSpeed = 3f;
                }
                else if (_open > 0f)
                {
                    Fondle(hit2);
                    hit2.hSpeed = -3f;
                }
            }
            if (_open < -0f && hit2 != null && (hit2 is Duck || (hit2.right > _topLeft.x - 10f && hit2.left < _topRight.x)))
            {
                keepOpen = true;
            }
            if (_open > 0f && hit2 != null && (hit2 is Duck || (hit2.left < _topRight.x + 10f && hit2.right > _topLeft.x)))
            {
                keepOpen = true;
            }
        }
        _jiggle = Maths.CountDown(_jiggle, 0.08f);
        if (!keepOpen)
        {
            if (_openForce > 1f)
            {
                _openForce = 1f;
            }
            if (_openForce < -1f)
            {
                _openForce = -1f;
            }
            if (_openForce > 0.04f)
            {
                _openForce -= 0.04f;
            }
            else if (_openForce < -0.04f)
            {
                _openForce += 0.04f;
            }
            else if (_openForce > -0.06f && _openForce < 0.06f)
            {
                _openForce = 0f;
            }
        }
        _open += _openForce;
        if (Math.Abs(_open) > 0.5f && !_opened)
        {
            _opened = true;
            SFX.Play("doorOpen", Rando.Float(0.8f, 0.9f), Rando.Float(-0.1f, 0.1f));
        }
        else if (Math.Abs(_open) < 0.1f && _opened)
        {
            _opened = false;
            SFX.Play("doorClose", Rando.Float(0.5f, 0.6f), Rando.Float(-0.1f, 0.1f));
        }
        if (_open > 1f)
        {
            _open = 1f;
        }
        if (_open < -1f)
        {
            _open = -1f;
        }
        if (_jam > 0f && _open > _jam)
        {
            if (!_jammed)
            {
                if (Network.isActive)
                {
                    if (base.isServerForObject)
                    {
                        SFX.PlaySynchronized("doorJam");
                    }
                }
                else
                {
                    SFX.Play("doorJam");
                }
                _jammed = true;
                if (jamThing != null)
                {
                    jamThing.hSpeed += 0.6f;
                    Fondle(jamThing);
                }
            }
            _open = _jam;
            if (_openForce > 0.1f)
            {
                _openForce = 0.1f;
            }
        }
        if (_jam < 0f && _open < _jam)
        {
            if (!_jammed)
            {
                if (Network.isActive)
                {
                    if (base.isServerForObject)
                    {
                        SFX.PlaySynchronized("doorJam");
                    }
                }
                else
                {
                    SFX.Play("doorJam");
                }
                _jammed = true;
                if (jamThing != null)
                {
                    jamThing.hSpeed -= 0.6f;
                    Fondle(jamThing);
                }
            }
            _open = _jam;
            if (_openForce < -0.1f)
            {
                _openForce = -0.1f;
            }
        }
        if (_open > 0f)
        {
            _sprite.flipH = false;
            _sprite.frame = (int)(_open * 15f);
        }
        else
        {
            _sprite.flipH = true;
            _sprite.frame = (int)(Math.Abs(_open) * 15f);
        }
        if (_sprite.frame > 9)
        {
            collisionSize = new Vec2(0f, 0f);
            solid = false;
            collisionOffset = new Vec2(0f, -999999f);
            base.depth = -0.7f;
        }
        else
        {
            collisionSize = new Vec2(colWide, 32f);
            solid = true;
            collisionOffset = new Vec2((0f - colWide) / 2f, -24f);
            base.depth = -0.5f;
        }
        if (_hitPoints <= 0f && !base._destroyed)
        {
            Destroy(new DTImpact(this));
        }
        if (_openForce == 0f)
        {
            _open = Maths.LerpTowards(_open, 0f, 0.1f);
        }
        if (_open == 0f)
        {
            _jammed = false;
        }
        float f = _hitPoints / _maxHealth * 0.2f + 0.8f;
        _sprite.color = new Color(f, f, f);
    }

    public override void Draw()
    {
        base.Draw();
        if (Level.current is Editor)
        {
            if (locked && !_lockedSprite)
            {
                _sprite = new SpriteMap("lockDoor", 32, 32);
                graphic = _sprite;
                _lockedSprite = true;
            }
            else if (!locked && _lockedSprite)
            {
                _sprite = new SpriteMap("door", 32, 32);
                graphic = _sprite;
                _lockedSprite = false;
            }
        }
        if (_lockDoor && !locked)
        {
            _key.frame = _sprite.frame;
            if (_key.frame > 12)
            {
                _key.depth = base.depth - 1;
            }
            else
            {
                _key.depth = base.depth + 1;
            }
            _key.flipH = graphic.flipH;
            Graphics.Draw(_key, base.x + _open * 12f, base.y - 8f);
        }
    }

    public override BinaryClassChunk Serialize()
    {
        BinaryClassChunk binaryClassChunk = base.Serialize();
        binaryClassChunk.AddProperty("locked", locked);
        return binaryClassChunk;
    }

    public override bool Deserialize(BinaryClassChunk node)
    {
        base.Deserialize(node);
        locked = node.GetProperty<bool>("locked");
        return true;
    }

    public override DXMLNode LegacySerialize()
    {
        DXMLNode dXMLNode = base.LegacySerialize();
        dXMLNode.Add(new DXMLNode("locked", Change.ToString(locked)));
        return dXMLNode;
    }

    public override bool LegacyDeserialize(DXMLNode node)
    {
        base.LegacyDeserialize(node);
        DXMLNode n = node.Element("locked");
        if (n != null)
        {
            locked = Convert.ToBoolean(n.Value);
        }
        return true;
    }

    public override ContextMenu GetContextMenu()
    {
        ContextMenu contextMenu = base.GetContextMenu();
        contextMenu.AddItem(new ContextCheckBox("Locked", null, new FieldBinding(this, "locked")));
        return contextMenu;
    }
}
