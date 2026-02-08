using Microsoft.Xna.Framework;
using System.Linq;

namespace DuckGame;

[BaggedProperty("canSpawn", false)]
public class ScoreRock : Holdable, IPlatform
{
    public StateBinding _planeBinding = new StateBinding(nameof(planeOfExistence));

    public StateBinding _depthBinding = new StateBinding(nameof(netDepth));

    public StateBinding _zBinding = new StateBinding(nameof(Z));

    public StateBinding _profileBinding = new StateBinding(nameof(netProfileIndex));

    private byte _netProfileIndex;

    private bool _customRock;

    private SpriteMap _sprite;

    private Sprite _dropShadow = new Sprite("dropShadow");

    private Vector2 _dropShadowPoint;

    private Vector2 _pos = Vector2.Zero;

    private Profile _profile;

    public float netDepth
    {
        get
        {
            return base.Depth.value;
        }
        set
        {
            base.Depth = value;
        }
    }

    public byte netProfileIndex
    {
        get
        {
            return _netProfileIndex;
        }
        set
        {
            _netProfileIndex = value;
            _profile = Profiles.all.ElementAt(_netProfileIndex);
            RefreshProfile(_profile);
        }
    }

    public ScoreRock(float xpos, float ypos, Profile profile)
        : base(xpos, ypos)
    {
        _sprite = new SpriteMap("scoreRock", 16, 16);
        graphic = _sprite;
        Center = new Vector2(8f, 8f);
        collisionOffset = new Vector2(-8f, -6f);
        collisionSize = new Vector2(16f, 13f);
        base.Depth = -0.5f;
        thickness = 4f;
        weight = 7f;
        RefreshProfile(profile);
        flammable = 0.3f;
        base.collideSounds.Add("rockHitGround2");
        _dropShadow.CenterOrigin();
        _profile = profile;
        base.impactThreshold = 1f;
    }

    private void RefreshProfile(Profile profile)
    {
        if (profile != null)
        {
            if (profile.team.hasHat)
            {
                _sprite.frame = 0;
            }
            else if (profile.persona == Persona.Duck1)
            {
                _sprite.frame = 1;
            }
            else if (profile.persona == Persona.Duck2)
            {
                _sprite.frame = 2;
            }
            else if (profile.persona == Persona.Duck3)
            {
                _sprite.frame = 3;
            }
            else if (profile.persona == Persona.Duck4)
            {
                _sprite.frame = 4;
            }
            else if (profile.persona == Persona.Duck5)
            {
                _sprite.frame = 5;
            }
            else if (profile.persona == Persona.Duck6)
            {
                _sprite.frame = 6;
            }
            else if (profile.persona == Persona.Duck7)
            {
                _sprite.frame = 7;
            }
            else if (profile.persona == Persona.Duck8)
            {
                _sprite.frame = 8;
            }
            if (profile.team.rockTexture != null)
            {
                _sprite = new SpriteMap(profile.team.rockTexture, 24, 24);
                Center = new Vector2(12f, 12f);
                graphic = _sprite;
                _customRock = true;
                collisionOffset = new Vector2(-8f, -1f);
                collisionSize = new Vector2(16f, 13f);
            }
        }
    }

    public override void Update()
    {
        foreach (Block block in Level.CheckLineAll<Block>(Position, Position + new Vector2(0f, 100f)))
        {
            if (block.solid)
            {
                _dropShadowPoint.X = base.X;
                _dropShadowPoint.Y = block.top;
            }
        }
        if (RockScoreboard.wallMode && base.X > 610f)
        {
            base.X = 610f;
            hSpeed = -1f;
            SFX.Play("rockHitGround2", 1f, -0.4f);
        }
        if (RockScoreboard.wallMode && base.X > 610f)
        {
            base.X = 610f;
            hSpeed = -1f;
            SFX.Play("rockHitGround2", 1f, -0.4f);
        }
        _pos = Position;
        base.Update();
    }

    public override void Draw()
    {
        base.Draw();
        if (!_customRock && _profile != null && _profile.team != null && _profile.team.hasHat)
        {
            SpriteMap hat = _profile.team.GetHat(_profile.persona);
            hat.Depth = base.Depth + 1;
            hat.Center = new Vector2(16f, 16f);
            Vector2 pos = Position - _profile.team.hatOffset;
            Graphics.Draw(hat, pos.X, pos.Y - 5f);
        }
    }
}
