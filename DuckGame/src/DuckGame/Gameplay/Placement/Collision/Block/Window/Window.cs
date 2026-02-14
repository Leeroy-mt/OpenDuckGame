using Microsoft.Xna.Framework;
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

    private List<Vector2> _hits = new List<Vector2>();

    private SinWaveManualUpdate _shake = 0.8f;

    private float _shakeVal;

    private Vector2 _shakeMult = Vector2.Zero;

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

    private Vector2 _enter;

    private bool _wrecked;

    private bool _hasGlass = true;

    public override Vector2 netPosition
    {
        get
        {
            return Position;
        }
        set
        {
            if (Position != value)
            {
                Position = value;
                if (_frame != null)
                {
                    _frame.Position = Position;
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

    public override void SetTranslation(Vector2 translation)
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
        Center = new Vector2(3f, 0f);
        if (floor)
        {
            collisionSize = new Vector2(high, 6f);
            collisionOffset = new Vector2(0f - high + 16f, -2f);
            _sprite.AngleDegrees = -90f;
        }
        else
        {
            collisionSize = new Vector2(6f, high);
            collisionOffset = new Vector2(-3f, 0f - high + 8f);
            _sprite.Angle = 0f;
        }
        _sprite.ScaleY = high;
        _borderSprite.ScaleY = high;
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
        Center = new Vector2(3f, 24f);
        collisionSize = new Vector2(6f, 32f);
        collisionOffset = new Vector2(-3f, -24f);
        base.Depth = -0.5f;
        _editorName = "Window";
        editorTooltip = "Classic window. Really opens up the room.";
        thickness = 0.3f;
        _sprite.color = new Color(1f, 1f, 1f, 0.2f);
        base.Alpha = 0.7f;
        base.breakForce = 3f;
        _canFlip = false;
        _translucent = true;
        UpdateHeight();
    }

    public override void Initialize()
    {
        if (!floor && !noframe)
        {
            _frame = new WindowFrame(base.X, base.Y, floor);
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
                Level.Add(new GlassParticle(base.X - 4f + Rando.Float(8f), base.Y - 16f + Rando.Float(32f), Vector2.Zero, tint.value)
                {
                    hSpeed = ((Rando.Float(1f) > 0.5f) ? 1f : (-1f)) * Rando.Float(3f),
                    vSpeed = 0f - Rando.Float(1f)
                });
            }
            if (this is FloorWindow)
            {
                for (int j = 0; j < 8; j++)
                {
                    Level.Add(new GlassDebris(rotate: false, base.left + (float)(j * 4), base.Y, 0f - Rando.Float(2f), 0f - Rando.Float(2f), 1));
                }
                foreach (PhysicsObject item in Level.CheckLineAll<PhysicsObject>(base.topLeft + new Vector2(-2f, -3f), base.topRight + new Vector2(2f, -3f)))
                {
                    item._sleeping = false;
                    item.vSpeed -= 2f;
                }
            }
            else
            {
                for (int k = 0; k < 8; k++)
                {
                    Level.Add(new GlassDebris(rotate: false, base.X, base.top + (float)(k * 4), 0f - Rando.Float(2f), 0f - Rando.Float(2f), 1, tint.value));
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

    public override bool Hit(Bullet bullet, Vector2 hitPos)
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
        if (_enter.X < base.X && _enter.X < base.left + 2f)
        {
            _enter.X = base.left;
        }
        else if (_enter.X > base.X && _enter.X > base.right - 2f)
        {
            _enter.X = base.right;
        }
        if (_enter.Y < base.Y && _enter.Y < base.top + 2f)
        {
            _enter.Y = base.top;
        }
        else if (_enter.Y > base.Y && _enter.Y > base.bottom - 2f)
        {
            _enter.Y = base.bottom;
        }
        if (hitPoints <= 0f)
        {
            return false;
        }
        hitPos -= bullet.travelDirNormalized;
        for (int i = 0; (float)i < 1f + damageMultiplier / 2f; i++)
        {
            Level.Add(new GlassParticle(hitPos.X, hitPos.Y, bullet.travelDirNormalized, tint.value));
        }
        SFX.Play("glassHit", 0.5f);
        if (base.isServerForObject && bullet.isLocal)
        {
            hitPoints -= damageMultiplier;
            damageMultiplier += 1f;
        }
        return base.Hit(bullet, hitPos);
    }

    public override void ExitHit(Bullet bullet, Vector2 exitPos)
    {
        if (_hasGlass)
        {
            _hits.Add(_enter);
            Vector2 exit = exitPos - bullet.travelDirNormalized;
            if (exit.X < base.X && exit.X < base.left + 2f)
            {
                exit.X = base.left;
            }
            else if (exit.X > base.X && exit.X > base.right - 2f)
            {
                exit.X = base.right;
            }
            if (exit.Y < base.Y && exit.Y < base.top + 2f)
            {
                exit.Y = base.top;
            }
            else if (exit.Y > base.Y && exit.Y > base.bottom - 2f)
            {
                exit.Y = base.bottom;
            }
            _hits.Add(exit);
            exitPos += bullet.travelDirNormalized;
            for (int i = 0; (float)i < 1f + damageMultiplier / 2f; i++)
            {
                Level.Add(new GlassParticle(exitPos.X, exitPos.Y, -bullet.travelDirNormalized, tint.value));
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
        _shakeMult = Lerp.Vector2(_shakeMult, Vector2.Zero, 0.1f);
        if (_localShakeTimes < shakeTimes)
        {
            Shake();
            _localShakeTimes = shakeTimes;
        }
        _shakeVal = Lerp.Float(_shakeVal, 0f, 0.05f);
    }

    public override void Draw()
    {
        Vector2 waver = Vector2.Zero;
        float shakeAmount = (float)_shake * _shakeVal * 0.8f;
        if (floor)
        {
            waver.Y = shakeAmount;
        }
        else
        {
            waver.X = shakeAmount;
        }
        Position += waver;
        float high = (float)windowHeight.value * 16f;
        _sprite.Depth = base.Depth;
        _borderSprite.Depth = base.Depth;
        _borderSprite.Angle = _sprite.Angle;
        _barSprite.Depth = _sprite.Depth + 4;
        _barSprite.ScaleY = _sprite.ScaleY;
        _barSprite.Alpha = _sprite.Alpha;
        _barSprite.Angle = Angle;
        if (_hasGlass)
        {
            Color c = windowColors[tint.value];
            c.A = 51;
            _sprite.color = c;
            base.Alpha = 0.7f;
            if (floor)
            {
                Graphics.Draw(_sprite, base.X - high + 16f, base.Y + 4f);
                Graphics.Draw(_borderSprite, base.X - high + 16f, base.Y + 4f);
            }
            else
            {
                Graphics.Draw(_sprite, base.X - 3f, base.Y - high + 8f);
                Graphics.Draw(_borderSprite, base.X - 3f, base.Y - high + 8f);
            }
            for (int i = 0; i < _hits.Count; i += 2)
            {
                if (i + 1 > _hits.Count)
                {
                    return;
                }
                Graphics.DrawLine(col: new Color((byte)((float)(int)c.R * 0.5f), (byte)((float)(int)c.G * 0.5f), (byte)((float)(int)c.B * 0.8f), (byte)178), p1: _hits[i] + waver, p2: _hits[i + 1] + waver);
            }
        }
        Position -= waver;
        if (floor)
        {
            if (bars.value)
            {
                Graphics.Draw(_barSprite, base.X - high + 16f, base.Y + 5f);
            }
        }
        else if (bars.value)
        {
            Graphics.Draw(_barSprite, base.X - 4f, base.Y - high + 8f);
        }
        base.Draw();
    }
}
