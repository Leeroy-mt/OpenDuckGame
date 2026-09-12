using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;

namespace DuckGame;

public class DuckPersona
{
    #region Public Methods

    public SpriteMap chatBust;

    public MaterialPersona material;

    #endregion

    #region Private Methods

    int _index = -1;

    Vector3 _color;

    Vector3 _colorDark;

    Vector3 _colorLight;

    SpriteMap _skipSprite;

    SpriteMap _arrowSprite;

    SpriteMap _fingerPositionSprite;

    SpriteMap _featherSprite;

    SpriteMap _crowdSprite;

    SpriteMap _sprite;

    SpriteMap _armSprite;

    SpriteMap _quackSprite;

    SpriteMap _controlledSprite;

    SpriteMap _defaultHead;

    RenderTarget2D _iconMap;

    #endregion

    #region Public Properties

    public bool mallard => _colorDark != Vector3.Zero;

    public int index
    {
        get
        {
            if (_index < 0 && Persona.all.FirstOrDefault(x => x.color == color) is DuckPersona d)
            {
                _index++;

                using var enumerator = Persona.all.GetEnumerator();
                while (enumerator.MoveNext() && enumerator.Current != d)
                    _index++;

                if (_index > Persona.all.Count())
                    _index = 0;
            }
            return _index;
        }
        set
        {
            _index = value;
        }
    }

    public Vector3 color
    {
        get => _color;
        set => _color = value;
    }

    public Vector3 colorDark =>
        _colorDark == Vector3.Zero ? _color * 0.7f : _colorDark;

    public Vector3 colorLight => _colorLight;

    public Color colorUsable => new((byte)_color.X, (byte)_color.Y, (byte)_color.Z);

    public SpriteMap skipSprite
    {
        get => _skipSprite;
        set => _skipSprite = value;
    }

    public SpriteMap arrowSprite
    {
        get => _arrowSprite;
        set => _arrowSprite = value;
    }

    public SpriteMap fingerPositionSprite
    {
        get => _fingerPositionSprite;
        set => _fingerPositionSprite = value;
    }

    public SpriteMap featherSprite
    {
        get => _featherSprite;
        set => _featherSprite = value;
    }

    public SpriteMap crowdSprite
    {
        get => _crowdSprite;
        set => _crowdSprite = value;
    }

    public SpriteMap sprite
    {
        get => _sprite;
        set => _sprite = value;
    }

    public SpriteMap armSprite
    {
        get => _armSprite;
        set => _armSprite = value;
    }

    public SpriteMap quackSprite
    {
        get => _quackSprite;
        set => _quackSprite = value;
    }

    public SpriteMap controlledSprite
    {
        get => _controlledSprite;
        set => _controlledSprite = value;
    }

    public SpriteMap defaultHead
    {
        get => _defaultHead;
        set => _defaultHead = value;
    }

    public RenderTarget2D iconMap
    {
        get
        {
            if (_iconMap == null || _iconMap.IsDisposed || _iconMap.IsContentLost)
                _iconMap = RenderTarget2D.CreateSetUpTarget(96, 96, pdepth: false, RenderTargetUsage.PreserveContents);
            return _iconMap;
        }
    }

    #endregion

    public Texture2D Recolor(Texture2D pTex)
    {
        return Graphics.RecolorOld(pTex, _color);
    }

    public DuckPersona(Vector3 varCol)
        : this(varCol, Vector3.Zero, Vector3.Zero)
    {
    }

    public DuckPersona(Vector3 varCol, Vector3 varCol2, Vector3 varCol3)
    {
        _color = varCol;
        _colorDark = varCol2;
        _colorLight = varCol3;
        material = new MaterialPersona(this);

        try
        {
            if (varCol2 != Vector3.Zero)
            {
                Color c1 = new(varCol.X / 255, varCol.Y / 255, varCol.Z / 255);
                Color c2 = new(varCol2.X / 255, varCol2.Y / 255, varCol2.Z / 255);
                Color c3 = new(varCol3.X / 255, varCol3.Y / 255, varCol3.Z / 255);

                _skipSprite = new SpriteMap(Graphics.RecolorM(Content.Load<Texture2D>("skipSign_m"), c1, c2, c3), 52, 18);
                _skipSprite.Center = new Vector2(_skipSprite.width - 3, 15f);
                _arrowSprite = new SpriteMap(Graphics.RecolorM(Content.Load<Texture2D>("startArrow_m"), c1, c2, c3), 24, 16);
                _arrowSprite.CenterOrigin();
                _sprite = new SpriteMap(Graphics.RecolorM(Content.Load<Texture2D>("duck_m"), c1, c2, c3), 32, 32);
                _sprite.CenterOrigin();
                _crowdSprite = new SpriteMap(Graphics.RecolorM(Content.Load<Texture2D>("seatDuck_m"), c1, c2, c3), 19, 23);
                _crowdSprite.CenterOrigin();
                _sprite.ClearAnimations();
                _sprite.AddAnimation("idle", 1f, true, default(int));
                _sprite.AddAnimation("run", 1f, true, 1, 2, 3, 4, 5, 6);
                _sprite.AddAnimation("jump", 1f, true, 7, 8, 9);
                _sprite.AddAnimation("slide", 1f, true, 10);
                _sprite.AddAnimation("crouch", 1f, true, 11);
                _sprite.AddAnimation("groundSlide", 1f, true, 12);
                _sprite.AddAnimation("dead", 1f, true, 13);
                _sprite.AddAnimation("netted", 1f, true, 14);
                _sprite.AddAnimation("listening", 1f, true, 16);
                _sprite.SetAnimation("idle");
                _featherSprite = new(Graphics.RecolorM(Content.Load<Texture2D>("feather_m"), c1, c2, c3), 12, 4)
                {
                    speed = 0.3f
                };
                _featherSprite.AddAnimation("feather", 1f, true, 0, 1, 2, 3);
                _fingerPositionSprite = new SpriteMap(Graphics.RecolorM(Content.Load<Texture2D>("fingerPositions_m"), c1, c2, c3), 16, 12);
                _fingerPositionSprite.CenterOrigin();
                _quackSprite = new SpriteMap(Graphics.RecolorM(Content.Load<Texture2D>("quackduck_m"), c1, c2, c3), 32, 32);
                _quackSprite.CenterOrigin();
                _armSprite = new SpriteMap(Graphics.RecolorM(Content.Load<Texture2D>("duckArms_m"), c1, c2, c3), 16, 16);
                _armSprite.CenterOrigin();
                _controlledSprite = new SpriteMap(Graphics.RecolorM(Content.Load<Texture2D>("controlledDuck_m"), c1, c2, c3), 32, 32);
                _controlledSprite.CenterOrigin();
                _defaultHead = new SpriteMap(Graphics.RecolorM(Content.Load<Texture2D>("hats/default_m"), c1, c2, c3), 32, 32);
                _defaultHead.CenterOrigin();
                chatBust = new SpriteMap(Graphics.RecolorM(Content.Load<Texture2D>("chatBust_m"), c1, c2, c3), 14, 13);
                chatBust.CenterOrigin();
            }
            else
            {
                _skipSprite = new SpriteMap(Graphics.RecolorOld(Content.Load<Texture2D>("skipSign"), _color), 52, 18);
                _skipSprite.Center = new Vector2(_skipSprite.width - 3, 15f);
                _arrowSprite = new SpriteMap(Graphics.RecolorOld(Content.Load<Texture2D>("startArrow"), _color), 24, 16);
                _arrowSprite.CenterOrigin();
                _sprite = new SpriteMap(Graphics.RecolorOld(Content.Load<Texture2D>("duck"), _color), 32, 32);
                _sprite.CenterOrigin();
                _crowdSprite = new SpriteMap(Graphics.RecolorOld(Content.Load<Texture2D>("seatDuck"), _color), 19, 23);
                _crowdSprite.CenterOrigin();
                _sprite.ClearAnimations();
                _sprite.AddAnimation("idle", 1f, true, default(int));
                _sprite.AddAnimation("run", 1f, true, 1, 2, 3, 4, 5, 6);
                _sprite.AddAnimation("jump", 1f, true, 7, 8, 9);
                _sprite.AddAnimation("slide", 1f, true, 10);
                _sprite.AddAnimation("crouch", 1f, true, 11);
                _sprite.AddAnimation("groundSlide", 1f, true, 12);
                _sprite.AddAnimation("dead", 1f, true, 13);
                _sprite.AddAnimation("netted", 1f, true, 14);
                _sprite.AddAnimation("listening", 1f, true, 16);
                _sprite.SetAnimation("idle");
                _featherSprite = new SpriteMap(Graphics.RecolorOld(Content.Load<Texture2D>("feather"), _color), 12, 4)
                {
                    speed = 0.3f
                };
                _featherSprite.AddAnimation("feather", 1f, true, 0, 1, 2, 3);
                _fingerPositionSprite = new SpriteMap(Graphics.RecolorOld(Content.Load<Texture2D>("fingerPositions"), _color), 16, 12);
                _fingerPositionSprite.CenterOrigin();
                _quackSprite = new SpriteMap(Graphics.RecolorOld(Content.Load<Texture2D>("quackduck"), _color), 32, 32);
                _quackSprite.CenterOrigin();
                _armSprite = new SpriteMap(Graphics.RecolorOld(Content.Load<Texture2D>("duckArms"), _color), 16, 16);
                _armSprite.CenterOrigin();
                _controlledSprite = new SpriteMap(Graphics.RecolorOld(Content.Load<Texture2D>("controlledDuck"), _color), 32, 32);
                _controlledSprite.CenterOrigin();
                _defaultHead = new SpriteMap(Graphics.RecolorOld(Content.Load<Texture2D>("hats/default"), _color), 32, 32);
                _defaultHead.CenterOrigin();
                chatBust = new SpriteMap(Graphics.RecolorOld(Content.Load<Texture2D>("chatBust"), _color), 14, 13);
                chatBust.CenterOrigin();
            }
        }
        catch
        {
        }
    }
}