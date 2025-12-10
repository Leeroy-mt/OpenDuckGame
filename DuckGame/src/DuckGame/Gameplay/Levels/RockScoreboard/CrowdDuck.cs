using System;
using System.Linq;

namespace DuckGame;

public class CrowdDuck : Thing
{
    private Mood _mood = Mood.Calm;

    private SpriteMap _sprite;

    private Sprite _letterSign;

    private Sprite _loveSign;

    private Sprite _sucksSign;

    private Sprite _suckSign;

    private bool _empty;

    private string _letter;

    private int _letterNumber;

    private float _letterSway = Rando.Float(2f);

    private BitmapFont _font;

    private Profile _lastLoyalty;

    private Profile _loyalty;

    public bool loyal;

    public bool newLoyal;

    private bool _busy;

    private bool _hate;

    private Profile _signProfile;

    private float _hatThrowTime = -1f;

    public int distVal;

    public int duckColor;

    private SpriteMap _originalSprite;

    public bool empty => _empty;

    public string letter
    {
        get
        {
            return _letter;
        }
        set
        {
            _letter = value;
        }
    }

    public Profile lastLoyalty => _lastLoyalty;

    public Profile loyalty
    {
        get
        {
            return _loyalty;
        }
        set
        {
            _loyalty = value;
        }
    }

    public bool busy
    {
        get
        {
            return _busy;
        }
        set
        {
            _busy = value;
        }
    }

    public void ClearActions()
    {
        _busy = false;
        _hate = false;
        _letterNumber = 0;
        _letter = null;
        _lastLoyalty = _loyalty;
    }

    public void SetLetter(string l, int num, bool hate = false, Profile p = null)
    {
        _letter = l;
        _letterNumber = num;
        _busy = _letter != null;
        _hate = hate;
        _signProfile = p;
    }

    public void TryChangingAllegiance(Profile to, float awesomeness)
    {
        if (awesomeness > 0.1f && Rando.Float(1f) < awesomeness)
        {
            if (loyalty != null)
            {
                if (loyalty != to)
                {
                    if (loyalty.stats.TryFanTransfer(to, awesomeness, loyal))
                    {
                        if (loyal)
                        {
                            newLoyal = false;
                        }
                        else
                        {
                            loyalty = to;
                        }
                    }
                }
                else if (awesomeness > 0.15f && Rando.Float(1.1f) < awesomeness)
                {
                    to.stats.MakeFanLoyal();
                    newLoyal = true;
                }
            }
            else
            {
                loyalty = to;
                to.stats.unloyalFans++;
            }
        }
        else if (awesomeness < -0.1f && Rando.Float(1f) < Math.Abs(awesomeness) && loyalty == to && loyalty.stats.FanConsidersLeaving(awesomeness, loyal))
        {
            if (loyal)
            {
                newLoyal = false;
            }
            else
            {
                loyalty = null;
            }
        }
    }

    public void ThrowHat(Profile p)
    {
        if (_lastLoyalty != _loyalty && ((_lastLoyalty == p && _loyalty == null) || _loyalty == p))
        {
            _hatThrowTime = Rando.Float(0.2f, 1f);
        }
        if (_loyalty == p)
        {
            loyal = newLoyal;
        }
    }

    public CrowdDuck(float xpos, float ypos, float zpos, int facing, int row, int dist, int empty = -1, Profile varLoyalty = null, Profile varLastLoyalty = null, bool varLoyal = false, int varColor = -1)
        : base(xpos, ypos)
    {
        distVal = dist;
        base.z = zpos;
        int fans = Crowd.totalFans;
        int emptyChance = 3;
        if (dist <= 20)
        {
            emptyChance += (int)((float)fans * 0.08f);
        }
        if (dist > 20)
        {
            emptyChance = 2;
            emptyChance += (int)((float)fans * 0.02f);
        }
        if (dist > 30)
        {
            emptyChance = 1;
            emptyChance += (int)((float)fans * 0.01f);
        }
        if (Crowd.totalFans < 1)
        {
            emptyChance = 0;
        }
        Crowd.fansUsed++;
        Rando.Int(1);
        duckColor = ((varColor > -1) ? varColor : Rando.Int(3));
        _originalSprite = Persona.all.ElementAt(duckColor).crowdSprite;
        SpriteMap duckSprite = _originalSprite.CloneMap();
        if (empty == 0 || (empty == -1 && Rando.Int(emptyChance) < 1))
        {
            duckSprite.AddAnimation("idle", Rando.Float(0.05f, 0.1f), true, 9);
            _empty = true;
        }
        else
        {
            if (empty == -1)
            {
                FanNum fan = Crowd.GetFan();
                Profile p = null;
                if (fan != null)
                {
                    p = fan.profile;
                    if (fan.loyalFans > 0)
                    {
                        newLoyal = (loyal = true);
                    }
                }
                _loyalty = (_lastLoyalty = p);
            }
            else
            {
                _loyalty = varLoyalty;
                _lastLoyalty = varLastLoyalty;
                loyal = (newLoyal = varLoyal);
            }
            switch (facing)
            {
                case 0:
                    duckSprite.AddAnimation("idle", Rando.Float(0.05f, 0.1f), true, default(int));
                    duckSprite.AddAnimation("cheer", Rando.Float(0.05f, 0.1f), true, 0, 1);
                    duckSprite.AddAnimation("scream", Rando.Float(0.05f, 0.1f), true, 0, 2);
                    break;
                case 1:
                    duckSprite.AddAnimation("idle", Rando.Float(0.05f, 0.1f), true, 3);
                    duckSprite.AddAnimation("cheer", Rando.Float(0.05f, 0.1f), true, 3, 4);
                    duckSprite.AddAnimation("scream", Rando.Float(0.05f, 0.1f), true, 3, 5);
                    break;
                default:
                    duckSprite.AddAnimation("idle", Rando.Float(0.05f, 0.1f), true, 6);
                    duckSprite.AddAnimation("cheer", Rando.Float(0.05f, 0.1f), true, 6, 7);
                    duckSprite.AddAnimation("scream", Rando.Float(0.05f, 0.1f), true, 6, 8);
                    break;
            }
        }
        duckSprite.SetAnimation("idle");
        _sprite = duckSprite;
        graphic = _sprite;
        collisionSize = new Vec2(_sprite.width, _sprite.height);
        collisionOffset = new Vec2(-(_sprite.w / 2), -(_sprite.h / 2));
        center = new Vec2(0f, duckSprite.h);
        collisionOffset = new Vec2(collisionOffset.x, -_sprite.h);
        base.depth = 0.3f - (float)row * 0.05f;
        base.layer = Layer.Background;
        _letterSign = new Sprite("letterSign");
        _letterSign.CenterOrigin();
        _letterSign.depth = base.depth + 2;
        _font = new BitmapFont("biosFont", 8);
        _loveSign = new Sprite("loveSign");
        _loveSign.CenterOrigin();
        _loveSign.depth = 0.32f - (float)row * 0.05f;
        _sucksSign = new Sprite("sucksSign");
        _sucksSign.CenterOrigin();
        _sucksSign.depth = 0.32f - (float)row * 0.05f;
        _suckSign = new Sprite("suckSign");
        _suckSign.CenterOrigin();
        _suckSign.depth = 0.32f - (float)row * 0.05f;
    }

    public override void Initialize()
    {
        base.Initialize();
    }

    public override void Update()
    {
        if (_empty)
        {
            return;
        }
        if (_mood != Crowd.mood)
        {
            _mood = Crowd.mood;
        }
        if (_mood == Mood.Calm || _mood == Mood.Silent || _mood == Mood.Dead || _mood == Mood.Excited)
        {
            _sprite.SetAnimation("cheer");
        }
        else if (_mood == Mood.Extatic)
        {
            _sprite.SetAnimation("scream");
        }
        if (_hatThrowTime > 0f)
        {
            _hatThrowTime -= 0.01f;
        }
        else if (_hatThrowTime > -0.5f)
        {
            _lastLoyalty = _loyalty;
            if (_lastLoyalty == null)
            {
                SFX.Play("cutOffQuack2", 0.9f, Rando.Float(-0.1f, 0.1f));
            }
            else
            {
                SFX.Play("cutOffQuack", 0.9f, Rando.Float(-0.1f, 0.1f));
            }
            Level.Add(SmallSmoke.New(base.x + 6f, base.y - 35f));
            _hatThrowTime = -1f;
        }
    }

    public override void Draw()
    {
        if (_sprite == null || _originalSprite == null)
        {
            return;
        }
        _sprite.texture = _originalSprite.texture;
        if (!_empty && _letter != null)
        {
            float offset = (float)Math.Sin(_letterSway + (float)_letterNumber * 0.1f) * 2f + 4f;
            _letterSway += 0.1f;
            if (_letter.Length == 1)
            {
                if ((_signProfile == null || _signProfile == loyalty) && _letter != " ")
                {
                    _letterSign.depth = base.depth + 5;
                    Graphics.Draw(_letterSign, base.x + 10f, base.y - 24f + offset);
                    _font.Draw(_letter, base.x + 6f, base.y - 28f + offset, Color.Gray, base.depth + 9);
                }
            }
            else if (_hate)
            {
                if (_letter[_letter.Length - 1] == 'S')
                {
                    Graphics.Draw(_suckSign, base.x + 28f, base.y - 27f + offset);
                    _font.Draw(_letter, base.x - _font.GetWidth(_letter) / 2f + 28f, base.y - 26f - 8f + offset, Color.Gray, _suckSign.depth + 3);
                }
                else
                {
                    Graphics.Draw(_sucksSign, base.x + 28f, base.y - 27f + offset);
                    _font.Draw(_letter, base.x - _font.GetWidth(_letter) / 2f + 28f, base.y - 26f - 8f + offset, Color.Gray, _sucksSign.depth + 3);
                }
            }
            else
            {
                Graphics.Draw(_loveSign, base.x + 28f, base.y - 27f + offset);
                _font.Draw(_letter, base.x - _font.GetWidth(_letter) / 2f + 29f, base.y - 26f + offset, Color.Gray, _loveSign.depth + 3);
            }
        }
        if (!_empty && _lastLoyalty != null && _lastLoyalty.persona != null && _lastLoyalty.team != null)
        {
            SpriteMap hat = _lastLoyalty.persona.defaultHead;
            Vec2 offset2 = Vec2.Zero;
            if (_lastLoyalty.team.hasHat)
            {
                offset2 = _lastLoyalty.team.hatOffset;
                hat = _lastLoyalty.team.GetHat(_lastLoyalty.persona);
            }
            if (hat == null)
            {
                return;
            }
            hat.depth = base.depth + 2;
            hat.angle = 0f;
            hat.alpha = 1f;
            hat.color = Color.White;
            bool open = false;
            open = ((_sprite.imageIndex == 1 || _sprite.imageIndex == 2 || _sprite.imageIndex == 4 || _sprite.imageIndex == 5 || _sprite.imageIndex == 7 || _sprite.imageIndex == 8) ? true : false);
            float xoff = 0f;
            if (_sprite.imageIndex > 2)
            {
                hat.flipH = true;
                xoff += 5f;
                if (open)
                {
                    xoff -= 1f;
                }
            }
            else
            {
                hat.flipH = false;
                if (open)
                {
                    xoff += 1f;
                }
            }
            if (hat.flipH)
            {
                offset2.x = 0f - offset2.x;
            }
            if (loyal)
            {
                hat.frame = (open ? 1 : 0);
            }
            hat.CenterOrigin();
            Graphics.Draw(hat, base.x - offset2.x + 8f + xoff, base.y - offset2.y - 22f - (float)(open ? 1 : 0));
            hat.frame = 0;
            hat.flipH = false;
        }
        base.Draw();
    }
}
