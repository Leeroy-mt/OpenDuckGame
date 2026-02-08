using Microsoft.Xna.Framework;

namespace DuckGame;

public abstract class Hat : Equipment
{
    protected Vector2 _hatOffset = Vector2.Zero;

    public bool strappedOn;

    public bool quacks = true;

    protected SpriteMap _sprite;

    protected Sprite _pickupSprite;

    protected bool _hasUnequippedCenter;

    public Vector2 hatOffset
    {
        get
        {
            return _hatOffset;
        }
        set
        {
            _hatOffset = value;
        }
    }

    public SpriteMap sprite
    {
        get
        {
            return _sprite;
        }
        set
        {
            _sprite = value;
        }
    }

    public Sprite pickupSprite
    {
        get
        {
            return _pickupSprite;
        }
        set
        {
            _pickupSprite = value;
        }
    }

    public virtual void SetQuack(int pValue)
    {
        frame = pValue;
    }

    public Hat(float xpos, float ypos)
        : base(xpos, ypos)
    {
        Center = new Vector2(16f, 16f);
        collisionOffset = new Vector2(-6f, -6f);
        collisionSize = new Vector2(12f, 12f);
        _autoOffset = false;
        thickness = 0.1f;
        _sprite = new SpriteMap("hats/burgers", 32, 32);
        _pickupSprite = new SpriteMap("hats/burgers", 32, 32);
        _equippedDepth = 6;
    }

    public virtual void Quack(float volume, float pitch)
    {
        SFX.Play("quack", volume, pitch);
    }

    public virtual void OpenHat()
    {
    }

    public virtual void CloseHat()
    {
    }

    public override void Update()
    {
        if (_equippedDuck != null && !destroyed)
        {
            Center = new Vector2(_sprite.w / 2, _sprite.h / 2);
            graphic = _sprite;
            solid = false;
            visible = false;
        }
        else
        {
            _sprite.frame = 0;
            if (!_hasUnequippedCenter)
            {
                Center = new Vector2(_pickupSprite.w / 2, _pickupSprite.h / 2);
            }
            graphic = _pickupSprite;
            solid = true;
            _sprite.flipH = false;
            visible = true;
        }
        if (destroyed)
        {
            base.Alpha -= 0.05f;
        }
        if (base.Alpha < 0f)
        {
            Level.Remove(this);
        }
        base.Update();
        if (_equippedDuck != null && _equippedDuck._trapped != null)
        {
            base.Depth = _equippedDuck._trapped.Depth + 2;
        }
        else if (owner == null && this is TeamHat)
        {
            base.Depth = -0.2f;
        }
    }
}
