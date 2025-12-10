using System;
using System.Collections.Generic;

namespace DuckGame;

[EditorGroup("Spawns")]
[BaggedProperty("previewPriority", true)]
public class PurpleBlock : ItemBox, IDrawToDifferentLayers
{
    private Sprite _scanner;

    private Sprite _projector;

    private Sprite _none;

    private Sprite _projectorGlitch;

    private Thing _currentProjection;

    private SinWave _wave = 1f;

    private SinWave _projectionWave = 0.04f;

    private SinWave _projectionWave2 = 0.05f;

    private SinWave _projectionFlashWave = 0.8f;

    private bool _useWave;

    private bool _alternate;

    private float _double;

    private float _glitch;

    public static Material _grayscale = new Material("Shaders/greyscale");

    public List<Profile> _served = new List<Profile>();

    private List<Profile> _close = new List<Profile>();

    private float _closeWait;

    private int _closeIndex;

    private float _projectorAlpha;

    private bool _closeGlitch;

    private Holdable _hoverItem;

    private float hitWait;

    private static Dictionary<Profile, StoredItem> _storedItems => Level.core._storedItems;

    public static void Reset()
    {
        _storedItems.Clear();
    }

    public PurpleBlock(float xpos, float ypos)
        : base(xpos, ypos)
    {
        graphic = new Sprite("purpleBlock");
        graphic.center = new Vec2(8f, 8f);
        _scanner = new Sprite("purpleScanner");
        _scanner.center = new Vec2(4f, 1f);
        _scanner.alpha = 0.7f;
        _scanner.depth = 0.9f;
        _projector = new Sprite("purpleProjector");
        _projector.center = new Vec2(8f, 16f);
        _projector.alpha = 0.7f;
        _projector.depth = 0.9f;
        _none = new Sprite("none");
        _none.center = new Vec2(8f, 8f);
        _none.alpha = 0.7f;
        _projectorGlitch = new Sprite("projectorGlitch");
        _projectorGlitch.center = new Vec2(8f, 8f);
        _projectorGlitch.alpha = 0.7f;
        _projectorGlitch.depth = 0.91f;
        base.impactThreshold = 0.2f;
        _placementCost += 4;
        editorTooltip = "Makes a copy of a Duck's weapon when used. Spawns a new copy when used again.";
    }

    public override void Initialize()
    {
    }

    public static StoredItem GetStoredItem(Profile p)
    {
        StoredItem item = null;
        if (!_storedItems.TryGetValue(p, out item))
        {
            StoredItem storedItem = (_storedItems[p] = new StoredItem());
            item = storedItem;
        }
        return item;
    }

    public static void StoreItem(Profile p, Thing t)
    {
        if (!(t is RagdollPart) && !(t is TrappedDuck) && (!(t is Holdable) || (t as Holdable).canStore))
        {
            if (t is WeightBall)
            {
                t = (t as WeightBall).collar;
            }
            StoredItem item = GetStoredItem(p);
            t.GetType();
            try
            {
                item.serializedData = t.Serialize();
                item.thing = Thing.LoadThing(item.serializedData);
            }
            catch (Exception)
            {
                _storedItems.Clear();
            }
            SFX.Play("scanBeep");
            if (Network.isActive && p.connection == DuckNetwork.localConnection && p.duck != null)
            {
                Send.Message(new NMPurpleBoxStoreItem(p.duck, t as PhysicsObject));
            }
        }
    }

    private void BreakHoverBond()
    {
        _hoverItem.gravMultiplier = 1f;
        _hoverItem = null;
    }

    public override void Update()
    {
        if (hitWait > 0f)
        {
            hitWait -= 0.1f;
        }
        else
        {
            hitWait = 0f;
        }
        _alternate = !_alternate;
        _scanner.alpha = 0.4f + _wave.normalized * 0.6f;
        _projector.alpha = (0.4f + _wave.normalized * 0.6f) * _projectorAlpha;
        _double = Maths.CountDown(_double, 0.15f);
        _glitch = Maths.CountDown(_glitch, 0.1f);
        if (Rando.Float(1f) < 0.01f)
        {
            _glitch = 0.3f;
            _projectorGlitch.xscale = 0.8f + Rando.Float(0.7f);
            _projectorGlitch.yscale = 0.6f + Rando.Float(0.5f);
            _projectorGlitch.flipH = Rando.Float(1f) > 0.5f;
        }
        if (Rando.Float(1f) < 0.005f)
        {
            _glitch = 0.3f;
            _projectorGlitch.xscale = 0.8f + Rando.Float(0.7f);
            _projectorGlitch.yscale = 0.6f + Rando.Float(0.5f);
            _projectorGlitch.flipH = Rando.Float(1f) > 0.5f;
            _useWave = !_useWave;
        }
        if (Rando.Float(1f) < 0.008f)
        {
            _glitch = 0.3f;
            _projectorGlitch.xscale = 0.8f + Rando.Float(0.7f);
            _projectorGlitch.yscale = 0.6f + Rando.Float(0.5f);
            _projectorGlitch.flipH = Rando.Float(1f) > 0.5f;
            _useWave = !_useWave;
            _double = 0.6f + Rando.Float(0.6f);
        }
        _close.Clear();
        if (_hoverItem != null && _hoverItem.owner != null)
        {
            BreakHoverBond();
        }
        if (_hoverItem == null)
        {
            Holdable g = Level.Nearest<Holdable>(base.x, base.y);
            if (g != null && g.owner == null && g != null && g.canPickUp && g.bottom <= base.top && Math.Abs(g.hSpeed) + Math.Abs(g.vSpeed) < 2f)
            {
                float dist = 999f;
                if (g != null)
                {
                    dist = (position - g.position).length;
                }
                if (dist < 24f)
                {
                    _hoverItem = g;
                }
            }
        }
        else if (Math.Abs(_hoverItem.hSpeed) + Math.Abs(_hoverItem.vSpeed) > 2f || (_hoverItem.position - position).length > 25f)
        {
            BreakHoverBond();
        }
        else
        {
            _hoverItem.position = Lerp.Vec2Smooth(_hoverItem.position, position + new Vec2(0f, -12f - _hoverItem.collisionSize.y / 2f + (float)_projectionWave * 2f), 0.2f);
            _hoverItem.vSpeed = 0f;
            _hoverItem.gravMultiplier = 0f;
        }
        foreach (Duck d in _level.things[typeof(Duck)])
        {
            if (!d.dead && (d.position - position).length < 64f)
            {
                _close.Add(d.profile);
                _closeGlitch = false;
            }
        }
        _closeWait = Maths.CountDown(_closeWait, 0.05f);
        for (int i = 0; i < _close.Count; i++)
        {
            if (_close.Count == 1)
            {
                _closeIndex = 0;
            }
            else if (_close.Count > 1 && i == _closeIndex && _closeWait <= 0f)
            {
                _closeIndex = (_closeIndex + 1) % _close.Count;
                _closeWait = 1f;
                _glitch = 0.3f;
                _projectorGlitch.xscale = 0.8f + Rando.Float(0.7f);
                _projectorGlitch.yscale = 0.6f + Rando.Float(0.5f);
                _projectorGlitch.flipH = Rando.Float(1f) > 0.5f;
                _useWave = !_useWave;
                _double = 0.6f + Rando.Float(0.6f);
                break;
            }
        }
        if (_closeIndex >= _close.Count)
        {
            _closeIndex = 0;
        }
        if (_close.Count == 0)
        {
            if (!_closeGlitch)
            {
                _closeWait = 1f;
                _glitch = 0.3f;
                _projectorGlitch.xscale = 0.8f + Rando.Float(0.7f);
                _projectorGlitch.yscale = 0.6f + Rando.Float(0.5f);
                _projectorGlitch.flipH = Rando.Float(1f) > 0.5f;
                _useWave = !_useWave;
                _double = 0.6f + Rando.Float(0.6f);
                _closeGlitch = true;
            }
            _projectorAlpha = Maths.CountDown(_projectorAlpha, 0.1f);
            _currentProjection = null;
        }
        else
        {
            StoredItem item = GetStoredItem(_close[_closeIndex]);
            _currentProjection = item.thing;
            _projectorAlpha = Maths.CountUp(_projectorAlpha, 0.1f);
        }
        _projectorGlitch.alpha = _glitch * _projectorAlpha;
        base.Update();
    }

    public override void Draw()
    {
        base.Draw();
    }

    public void OnDrawLayer(Layer pLayer)
    {
        if (pLayer != Layer.Background)
        {
            return;
        }
        if (_alternate)
        {
            Graphics.Draw(_scanner, base.x, base.y + 9f);
        }
        if (!_alternate)
        {
            Graphics.Draw(_projector, base.x, base.y - 8f);
        }
        float wave = (_useWave ? _projectionWave : _projectionWave2);
        if (_double > 0f)
        {
            if (_currentProjection != null)
            {
                Duck.renderingIcon = true;
                Material obj = Graphics.material;
                Graphics.material = _grayscale;
                _currentProjection.depth = base.depth - 5;
                _currentProjection.x = base.x - _double * 2f;
                _currentProjection.y = base.y - 16f - wave;
                _currentProjection.Draw();
                _currentProjection.x = base.x + _double * 2f;
                _currentProjection.y = base.y - 16f - wave;
                _currentProjection.Draw();
                Graphics.material = obj;
                Duck.renderingIcon = false;
            }
            else
            {
                _none.alpha = (0.2f + _projectionFlashWave.normalized * 0.2f + _glitch * 1f) * _projectorAlpha;
                Graphics.Draw(_none, base.x - _double * 2f, base.y - 16f - wave);
                Graphics.Draw(_none, base.x + _double * 2f, base.y - 16f - wave);
            }
        }
        else if (_currentProjection != null)
        {
            Duck.renderingIcon = true;
            Material obj2 = Graphics.material;
            Graphics.material = _grayscale;
            _currentProjection.depth = base.depth - 5;
            _currentProjection.x = base.x;
            _currentProjection.y = base.y - 16f - wave;
            _currentProjection.Draw();
            Graphics.material = obj2;
            Duck.renderingIcon = false;
        }
        else
        {
            Graphics.Draw(_none, base.x, base.y - 16f - wave);
        }
        if (_currentProjection != null && _served.Contains(_close[_closeIndex]))
        {
            _none.alpha = (0.2f + _projectionFlashWave.normalized * 0.2f + _glitch * 1f) * _projectorAlpha;
            Graphics.Draw(_none, base.x, base.y - 16f - wave, base.depth + 5);
        }
        if (_glitch > 0f)
        {
            Graphics.Draw(_projectorGlitch, base.x, base.y - 16f);
        }
    }

    public override void OnSoftImpact(MaterialThing with, ImpactedFrom from)
    {
        if (from == ImpactedFrom.Bottom && hitWait == 0f && with.isServerForObject)
        {
            with.Fondle(this);
        }
        if (!base.isServerForObject || !with.isServerForObject || from != ImpactedFrom.Bottom || hitWait != 0f)
        {
            return;
        }
        hitWait = 1f;
        if (with is Holdable h && (h.lastThrownBy != null || (h is RagdollPart && !Network.isActive)))
        {
            Duck d = h.lastThrownBy as Duck;
            if (!(h is RagdollPart))
            {
                if (d != null)
                {
                    StoreItem(d.profile, with);
                }
                Bounce();
            }
        }
        else
        {
            if (!(with is Duck duck))
            {
                return;
            }
            RumbleManager.AddRumbleEvent(duck.profile, new RumbleEvent(RumbleIntensity.Light, RumbleDuration.Pulse, RumbleFalloff.None));
            StoredItem item = GetStoredItem(duck.profile);
            if (item.thing != null && !_served.Contains(duck.profile))
            {
                containContext = item.thing as PhysicsObject;
                item.thing = Thing.LoadThing(item.serializedData);
                _hit = false;
                Pop();
                _served.Add(duck.profile);
                if (Network.isActive && duck.isServerForObject)
                {
                    Send.Message(new NMPurpleBoxServed(duck, this));
                }
            }
            else
            {
                if (_served.Contains(duck.profile))
                {
                    SFX.PlaySynchronized("scanFail");
                }
                Bounce();
            }
            if (duck.holdObject != null)
            {
                Holdable hold = duck.holdObject;
                if (hold != null)
                {
                    StoreItem(duck.profile, hold);
                }
            }
        }
    }
}
