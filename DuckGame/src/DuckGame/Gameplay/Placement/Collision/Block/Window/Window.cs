using System;
using System.Collections.Generic;

namespace DuckGame;

[EditorGroup("Stuff")]
public class Window : Block, IPlatform, ISequenceItem, IDontMove
{
    public StateBinding _positionBinding = new StateBinding(nameof(netPosition));

    public StateBinding _hitPointsBinding = new StateBinding(nameof(hitPoints));

    public StateBinding _destroyedBinding = new StateBinding(nameof(_destroyed));

    public StateBinding _damageMultiplierBinding = new StateBinding(nameof(damageMultiplier));

    public StateBinding _shakeTimesBinding = new StateBinding(nameof(shakeTimes));

    public NetIndex4 shakeTimes = 0;

    private NetIndex4 _localShakeTimes = 0;

    public float maxHealth = 5f;

    public float hitPoints = 5f;

    public float damageMultiplier = 1f;

    protected Sprite _sprite;

    protected Sprite _borderSprite;

    protected Sprite _barSprite;

    public bool landed = true;

    private List<Vec2> _hits = new List<Vec2>();

    private SinWaveManualUpdate _shake = 0.8f;

    private float _shakeVal;

    private Vec2 _shakeMult = Vec2.Zero;

    public bool floor;

    public bool doShake;

    protected WindowFrame _frame;

    public EditorProperty<int> windowHeight;

    public EditorProperty<int> tint = new EditorProperty<int>(0, null, 0f, windowColors.Count - 1, 1f);

    public EditorProperty<bool> valid;

    public EditorProperty<bool> bars = new EditorProperty<bool>(val: false);

    public static List<Color> windowColors = new List<Color>
    {
        new Color(102, 186, 245),
        Color.Red,
        Color.Orange,
        Color.Yellow,
        Color.Pink,
        Color.Purple,
        Color.Green,
        Color.Lime,
        Color.Maroon,
        Color.Magenta,
        Color.Cyan,
        Color.DarkGoldenrod
    };

    public bool noframe;

    public bool lobbyRemoving;

    private Vec2 _enter;

    private bool _wrecked;

    private bool _hasGlass = true;

    public override Vec2 netPosition
    {
        get
        {
            return position;
        }
        set
        {
            if (position != value)
            {
                position = value;
                if (_frame != null)
                {
                    _frame.position = position;
                }
                Level.current.things.quadTree.Remove(this);
                Level.current.things.quadTree.Add(this);
            }
        }
    }

    public override void EditorPropertyChanged(object property)
    {
        UpdateHeight();
        base.sequence.isValid = valid.value;
    }

    public override void SetTranslation(Vec2 translation)
    {
        if (_frame != null)
        {
            _frame.SetTranslation(translation);
        }
        base.SetTranslation(translation);
    }

    public virtual void UpdateHeight()
    {
        float high = (float)windowHeight.value * 16f;
        center = new Vec2(3f, 0f);
        if (floor)
        {
            collisionSize = new Vec2(high, 6f);
            collisionOffset = new Vec2(0f - high + 16f, -2f);
            _sprite.angleDegrees = -90f;
        }
        else
        {
            collisionSize = new Vec2(6f, high);
            collisionOffset = new Vec2(-3f, 0f - high + 8f);
            _sprite.angle = 0f;
        }
        _sprite.yscale = high;
        _borderSprite.yscale = high;
        if (_frame != null)
        {
            _frame.high = high;
        }
        base.sequence.isValid = valid.value;
    }

    public Window(float xpos, float ypos)
        : base(xpos, ypos)
    {
        windowHeight = new EditorProperty<int>(2, this, 1f, 16f, 1f);
        valid = new EditorProperty<bool>(val: false, this);
        _sprite = new Sprite("window32", 6f, 1f);
        _barSprite = new Sprite("windowBars", 8f, 1f);
        _borderSprite = new Sprite("window32border");
        _editorIcon = new Sprite("windowIconVertical");
        base.sequence = new SequenceItem(this);
        base.sequence.type = SequenceItemType.Goody;
        physicsMaterial = PhysicsMaterial.Glass;
        center = new Vec2(3f, 24f);
        collisionSize = new Vec2(6f, 32f);
        collisionOffset = new Vec2(-3f, -24f);
        base.depth = -0.5f;
        _editorName = "Window";
        editorTooltip = "Classic window. Really opens up the room.";
        thickness = 0.3f;
        _sprite.color = new Color(1f, 1f, 1f, 0.2f);
        base.alpha = 0.7f;
        base.breakForce = 3f;
        _canFlip = false;
        _translucent = true;
        UpdateHeight();
    }

    public override void Initialize()
    {
        if (!floor && !noframe)
        {
            _frame = new WindowFrame(base.x, base.y, floor);
            Level.Add(_frame);
        }
        UpdateHeight();
    }

    public override void Terminate()
    {
        if (!(Level.current is Editor) && !_wrecked && !lobbyRemoving)
        {
            _wrecked = true;
            for (int i = 0; i < 8; i++)
            {
                Level.Add(new GlassParticle(base.x - 4f + Rando.Float(8f), base.y - 16f + Rando.Float(32f), Vec2.Zero, tint.value)
                {
                    hSpeed = ((Rando.Float(1f) > 0.5f) ? 1f : (-1f)) * Rando.Float(3f),
                    vSpeed = 0f - Rando.Float(1f)
                });
            }
            if (this is FloorWindow)
            {
                for (int j = 0; j < 8; j++)
                {
                    Level.Add(new GlassDebris(rotate: false, base.left + (float)(j * 4), base.y, 0f - Rando.Float(2f), 0f - Rando.Float(2f), 1));
                }
                foreach (PhysicsObject item in Level.CheckLineAll<PhysicsObject>(base.topLeft + new Vec2(-2f, -3f), base.topRight + new Vec2(2f, -3f)))
                {
                    item._sleeping = false;
                    item.vSpeed -= 2f;
                }
            }
            else
            {
                for (int k = 0; k < 8; k++)
                {
                    Level.Add(new GlassDebris(rotate: false, base.x, base.top + (float)(k * 4), 0f - Rando.Float(2f), 0f - Rando.Float(2f), 1, tint.value));
                }
            }
            SFX.Play("glassBreak");
        }
        if (!floor && !_wrecked)
        {
            Level.Remove(_frame);
            _frame = null;
        }
        base.Terminate();
    }

    public override bool Hit(Bullet bullet, Vec2 hitPos)
    {
        if (bullet.isLocal)
        {
            Thing.Fondle(this, DuckNetwork.localConnection);
        }
        if (!_hasGlass)
        {
            return base.Hit(bullet, hitPos);
        }
        _enter = hitPos + bullet.travelDirNormalized;
        if (_enter.x < base.x && _enter.x < base.left + 2f)
        {
            _enter.x = base.left;
        }
        else if (_enter.x > base.x && _enter.x > base.right - 2f)
        {
            _enter.x = base.right;
        }
        if (_enter.y < base.y && _enter.y < base.top + 2f)
        {
            _enter.y = base.top;
        }
        else if (_enter.y > base.y && _enter.y > base.bottom - 2f)
        {
            _enter.y = base.bottom;
        }
        if (hitPoints <= 0f)
        {
            return false;
        }
        hitPos -= bullet.travelDirNormalized;
        for (int i = 0; (float)i < 1f + damageMultiplier / 2f; i++)
        {
            Level.Add(new GlassParticle(hitPos.x, hitPos.y, bullet.travelDirNormalized, tint.value));
        }
        SFX.Play("glassHit", 0.5f);
        if (base.isServerForObject && bullet.isLocal)
        {
            hitPoints -= damageMultiplier;
            damageMultiplier += 1f;
        }
        return base.Hit(bullet, hitPos);
    }

    public override void ExitHit(Bullet bullet, Vec2 exitPos)
    {
        if (_hasGlass)
        {
            _hits.Add(_enter);
            Vec2 exit = exitPos - bullet.travelDirNormalized;
            if (exit.x < base.x && exit.x < base.left + 2f)
            {
                exit.x = base.left;
            }
            else if (exit.x > base.x && exit.x > base.right - 2f)
            {
                exit.x = base.right;
            }
            if (exit.y < base.y && exit.y < base.top + 2f)
            {
                exit.y = base.top;
            }
            else if (exit.y > base.y && exit.y > base.bottom - 2f)
            {
                exit.y = base.bottom;
            }
            _hits.Add(exit);
            exitPos += bullet.travelDirNormalized;
            for (int i = 0; (float)i < 1f + damageMultiplier / 2f; i++)
            {
                Level.Add(new GlassParticle(exitPos.x, exitPos.y, -bullet.travelDirNormalized, tint.value));
            }
        }
    }

    public void Shake()
    {
        if (_hasGlass)
        {
            SFX.Play("glassBump", 0.7f);
        }
        _shakeVal = (float)Math.PI;
    }

    public override void OnSolidImpact(MaterialThing with, ImpactedFrom from)
    {
        with.Fondle(this);
        if (floor && with.top > base.top && CalculateImpactPower(with, from) > 2.8f && with.isServerForObject)
        {
            if (with is Duck duckImpact)
            {
                RumbleManager.AddRumbleEvent(duckImpact.profile, new RumbleEvent(RumbleIntensity.Light, RumbleDuration.Pulse, RumbleFalloff.Short));
            }
            Destroy(new DTImpact(with));
            return;
        }
        float totalSpeed = Math.Abs(with.hSpeed) + Math.Abs(with.vSpeed);
        if (!destroyed && totalSpeed > 1.5f)
        {
            ++shakeTimes;
            if (base.isServerForObject && Level.current is TeamSelect2 && with is PhysicsObject && (with as PhysicsObject).gravMultiplier < 0.1f)
            {
                Destroy(new DTImpact(with));
            }
        }
        if (destroyed && with is Duck duckImpact2)
        {
            RumbleManager.AddRumbleEvent(duckImpact2.profile, new RumbleEvent(RumbleIntensity.Light, RumbleDuration.Pulse, RumbleFalloff.Short));
        }
    }

    protected override bool OnDestroy(DestroyType type = null)
    {
        if (!_hasGlass)
        {
            return false;
        }
        if (bars.value)
        {
            _hasGlass = false;
        }
        else
        {
            Level.Remove(this);
        }
        if (base.sequence != null && base.sequence.isValid)
        {
            base.sequence.Finished();
            if (ChallengeLevel.running)
            {
                ChallengeLevel.goodiesGot++;
            }
        }
        return !bars.value;
    }

    public override void Update()
    {
        _shake.Update();
        base.breakForce = 6f * (hitPoints / maxHealth);
        if (hitPoints <= 0f)
        {
            Destroy(new DTImpact(null));
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
        _shakeMult = Lerp.Vec2(_shakeMult, Vec2.Zero, 0.1f);
        if (_localShakeTimes < shakeTimes)
        {
            Shake();
            _localShakeTimes = shakeTimes;
        }
        _shakeVal = Lerp.Float(_shakeVal, 0f, 0.05f);
    }

    public override void Draw()
    {
        Vec2 waver = Vec2.Zero;
        float shakeAmount = (float)_shake * _shakeVal * 0.8f;
        if (floor)
        {
            waver.y = shakeAmount;
        }
        else
        {
            waver.x = shakeAmount;
        }
        position += waver;
        float high = (float)windowHeight.value * 16f;
        _sprite.depth = base.depth;
        _borderSprite.depth = base.depth;
        _borderSprite.angle = _sprite.angle;
        _barSprite.depth = _sprite.depth + 4;
        _barSprite.yscale = _sprite.yscale;
        _barSprite.alpha = _sprite.alpha;
        _barSprite.angle = angle;
        if (_hasGlass)
        {
            Color c = windowColors[tint.value];
            c.a = 51;
            _sprite.color = c;
            base.alpha = 0.7f;
            if (floor)
            {
                Graphics.Draw(_sprite, base.x - high + 16f, base.y + 4f);
                Graphics.Draw(_borderSprite, base.x - high + 16f, base.y + 4f);
            }
            else
            {
                Graphics.Draw(_sprite, base.x - 3f, base.y - high + 8f);
                Graphics.Draw(_borderSprite, base.x - 3f, base.y - high + 8f);
            }
            for (int i = 0; i < _hits.Count; i += 2)
            {
                if (i + 1 > _hits.Count)
                {
                    return;
                }
                Graphics.DrawLine(col: new Color((byte)((float)(int)c.r * 0.5f), (byte)((float)(int)c.g * 0.5f), (byte)((float)(int)c.b * 0.8f), (byte)178), p1: _hits[i] + waver, p2: _hits[i + 1] + waver);
            }
        }
        position -= waver;
        if (floor)
        {
            if (bars.value)
            {
                Graphics.Draw(_barSprite, base.x - high + 16f, base.y + 5f);
            }
        }
        else if (bars.value)
        {
            Graphics.Draw(_barSprite, base.x - 4f, base.y - high + 8f);
        }
        base.Draw();
    }
}
