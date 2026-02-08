using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DuckGame;

public class DuckPersona
{
    private int _index = -1;

    private Vec3 _color;

    private Vec3 _colorDark;

    private Vec3 _colorLight;

    private SpriteMap _skipSprite;

    private SpriteMap _arrowSprite;

    private SpriteMap _fingerPositionSprite;

    private SpriteMap _featherSprite;

    private SpriteMap _crowdSprite;

    private SpriteMap _sprite;

    private SpriteMap _armSprite;

    private SpriteMap _quackSprite;

    private SpriteMap _controlledSprite;

    private SpriteMap _defaultHead;

    public SpriteMap chatBust;

    private RenderTarget2D _iconMap;

    public MaterialPersona material;

    public bool mallard => _colorDark != Vec3.Zero;

    public int index
    {
        get
        {
            if (_index < 0)
            {
                DuckPersona d = Persona.all.FirstOrDefault((DuckPersona x) => x.color == color);
                if (d != null)
                {
                    _index++;
                    using (IEnumerator<DuckPersona> enumerator = Persona.all.GetEnumerator())
                    {
                        while (enumerator.MoveNext() && enumerator.Current != d)
                        {
                            _index++;
                        }
                    }
                    if (_index > Persona.all.Count())
                    {
                        _index = 0;
                    }
                }
            }
            return _index;
        }
        set
        {
            _index = value;
        }
    }

    public Vec3 color
    {
        get
        {
            return _color;
        }
        set
        {
            _color = value;
        }
    }

    public Vec3 colorDark
    {
        get
        {
            if (_colorDark == Vec3.Zero)
            {
                return _color * 0.7f;
            }
            return _colorDark;
        }
    }

    public Vec3 colorLight => _colorLight;

    public Color colorUsable => new Color((byte)_color.x, (byte)_color.y, (byte)_color.z);

    public SpriteMap skipSprite
    {
        get
        {
            return _skipSprite;
        }
        set
        {
            _skipSprite = value;
        }
    }

    public SpriteMap arrowSprite
    {
        get
        {
            return _arrowSprite;
        }
        set
        {
            _arrowSprite = value;
        }
    }

    public SpriteMap fingerPositionSprite
    {
        get
        {
            return _fingerPositionSprite;
        }
        set
        {
            _fingerPositionSprite = value;
        }
    }

    public SpriteMap featherSprite
    {
        get
        {
            return _featherSprite;
        }
        set
        {
            _featherSprite = value;
        }
    }

    public SpriteMap crowdSprite
    {
        get
        {
            return _crowdSprite;
        }
        set
        {
            _crowdSprite = value;
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

    public SpriteMap armSprite
    {
        get
        {
            return _armSprite;
        }
        set
        {
            _armSprite = value;
        }
    }

    public SpriteMap quackSprite
    {
        get
        {
            return _quackSprite;
        }
        set
        {
            _quackSprite = value;
        }
    }

    public SpriteMap controlledSprite
    {
        get
        {
            return _controlledSprite;
        }
        set
        {
            _controlledSprite = value;
        }
    }

    public SpriteMap defaultHead
    {
        get
        {
            return _defaultHead;
        }
        set
        {
            _defaultHead = value;
        }
    }

    public RenderTarget2D iconMap
    {
        get
        {
            if (_iconMap == null || _iconMap.IsDisposed || (_iconMap.nativeObject as Microsoft.Xna.Framework.Graphics.RenderTarget2D).IsContentLost)
            {
                _iconMap = new RenderTarget2D(96, 96, pdepth: false, RenderTargetUsage.PreserveContents);
            }
            return _iconMap;
        }
    }

    public Tex2D Recolor(Tex2D pTex)
    {
        return Graphics.RecolorOld(pTex, _color);
    }

    public DuckPersona(Vec3 varCol)
        : this(varCol, Vec3.Zero, Vec3.Zero)
    {
    }

    public DuckPersona(Vec3 varCol, Vec3 varCol2, Vec3 varCol3)
    {
        _color = varCol;
        _colorDark = varCol2;
        _colorLight = varCol3;
        material = new MaterialPersona(this);
        try
        {
            if (varCol2 != Vec3.Zero)
            {
                Color c1 = new Color(varCol.x / 255f, varCol.y / 255f, varCol.z / 255f);
                Color c2 = new Color(varCol2.x / 255f, varCol2.y / 255f, varCol2.z / 255f);
                Color c3 = new Color(varCol3.x / 255f, varCol3.y / 255f, varCol3.z / 255f);
                _skipSprite = new SpriteMap(Graphics.RecolorM(Content.Load<Tex2D>("skipSign_m"), c1, c2, c3), 52, 18);
                _skipSprite.Center = new Vector2(_skipSprite.width - 3, 15f);
                _arrowSprite = new SpriteMap(Graphics.RecolorM(Content.Load<Tex2D>("startArrow_m"), c1, c2, c3), 24, 16);
                _arrowSprite.CenterOrigin();
                _sprite = new SpriteMap(Graphics.RecolorM(Content.Load<Tex2D>("duck_m"), c1, c2, c3), 32, 32);
                _sprite.CenterOrigin();
                _crowdSprite = new SpriteMap(Graphics.RecolorM(Content.Load<Tex2D>("seatDuck_m"), c1, c2, c3), 19, 23);
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
                _featherSprite = new SpriteMap(Graphics.RecolorM(Content.Load<Tex2D>("feather_m"), c1, c2, c3), 12, 4);
                _featherSprite.speed = 0.3f;
                _featherSprite.AddAnimation("feather", 1f, true, 0, 1, 2, 3);
                _fingerPositionSprite = new SpriteMap(Graphics.RecolorM(Content.Load<Tex2D>("fingerPositions_m"), c1, c2, c3), 16, 12);
                _fingerPositionSprite.CenterOrigin();
                _quackSprite = new SpriteMap(Graphics.RecolorM(Content.Load<Tex2D>("quackduck_m"), c1, c2, c3), 32, 32);
                _quackSprite.CenterOrigin();
                _armSprite = new SpriteMap(Graphics.RecolorM(Content.Load<Tex2D>("duckArms_m"), c1, c2, c3), 16, 16);
                _armSprite.CenterOrigin();
                _controlledSprite = new SpriteMap(Graphics.RecolorM(Content.Load<Tex2D>("controlledDuck_m"), c1, c2, c3), 32, 32);
                _controlledSprite.CenterOrigin();
                _defaultHead = new SpriteMap(Graphics.RecolorM(Content.Load<Tex2D>("hats/default_m"), c1, c2, c3), 32, 32);
                _defaultHead.CenterOrigin();
                chatBust = new SpriteMap(Graphics.RecolorM(Content.Load<Tex2D>("chatBust_m"), c1, c2, c3), 14, 13);
                chatBust.CenterOrigin();
            }
            else
            {
                _skipSprite = new SpriteMap(Graphics.RecolorOld(Content.Load<Tex2D>("skipSign"), _color), 52, 18);
                _skipSprite.Center = new Vector2(_skipSprite.width - 3, 15f);
                _arrowSprite = new SpriteMap(Graphics.RecolorOld(Content.Load<Tex2D>("startArrow"), _color), 24, 16);
                _arrowSprite.CenterOrigin();
                _sprite = new SpriteMap(Graphics.RecolorOld(Content.Load<Tex2D>("duck"), _color), 32, 32);
                _sprite.CenterOrigin();
                _crowdSprite = new SpriteMap(Graphics.RecolorOld(Content.Load<Tex2D>("seatDuck"), _color), 19, 23);
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
                _featherSprite = new SpriteMap(Graphics.RecolorOld(Content.Load<Tex2D>("feather"), _color), 12, 4);
                _featherSprite.speed = 0.3f;
                _featherSprite.AddAnimation("feather", 1f, true, 0, 1, 2, 3);
                _fingerPositionSprite = new SpriteMap(Graphics.RecolorOld(Content.Load<Tex2D>("fingerPositions"), _color), 16, 12);
                _fingerPositionSprite.CenterOrigin();
                _quackSprite = new SpriteMap(Graphics.RecolorOld(Content.Load<Tex2D>("quackduck"), _color), 32, 32);
                _quackSprite.CenterOrigin();
                _armSprite = new SpriteMap(Graphics.RecolorOld(Content.Load<Tex2D>("duckArms"), _color), 16, 16);
                _armSprite.CenterOrigin();
                _controlledSprite = new SpriteMap(Graphics.RecolorOld(Content.Load<Tex2D>("controlledDuck"), _color), 32, 32);
                _controlledSprite.CenterOrigin();
                _defaultHead = new SpriteMap(Graphics.RecolorOld(Content.Load<Tex2D>("hats/default"), _color), 32, 32);
                _defaultHead.CenterOrigin();
                chatBust = new SpriteMap(Graphics.RecolorOld(Content.Load<Tex2D>("chatBust"), _color), 14, 13);
                chatBust.CenterOrigin();
            }
        }
        catch (Exception)
        {
        }
    }

    public void Recreate()
    {
    }
}
