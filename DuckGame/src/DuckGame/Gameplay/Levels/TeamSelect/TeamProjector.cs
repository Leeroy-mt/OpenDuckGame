using System.Collections.Generic;

namespace DuckGame;

public class TeamProjector : Thing
{
    private Sprite _selectPlatform;

    private Sprite _selectProjector;

    private SinWave _projectorSin = 0.5f;

    private Profile _profile;

    private List<Profile> _profiles = new List<Profile>();

    private bool _swap;

    private float _swapFade = 1f;

    public TeamProjector(float xpos, float ypos, Profile profile)
        : base(xpos, ypos)
    {
        _selectPlatform = new Sprite("selectPlatform");
        _selectPlatform.CenterOrigin();
        _selectProjector = new Sprite("selectProjector");
        _selectProjector.CenterOrigin();
        _profile = profile;
        _profiles.Add(profile);
    }

    public void SetProfile(Profile newProfile)
    {
        _profile = newProfile;
    }

    public override void Update()
    {
        List<Profile> profiles = new List<Profile>();
        profiles.Add(_profile);
        Team t = _profile.team;
        if (t != null)
        {
            profiles = t.activeProfiles;
        }
        bool same = profiles.Count == _profiles.Count;
        foreach (Profile p in profiles)
        {
            if (!_profiles.Contains(p))
            {
                same = false;
                break;
            }
        }
        if (!same)
        {
            _swap = true;
        }
        if (_swap)
        {
            _swapFade -= 0.1f;
        }
        else
        {
            _swapFade += 0.1f;
            if (_swapFade > 1f)
            {
                _swapFade = 1f;
            }
        }
        if (_swapFade <= 0f && _swap)
        {
            _swap = false;
            _swapFade = 0f;
            _profiles.Clear();
            _profiles.AddRange(profiles);
        }
    }

    public override void Draw()
    {
        float deep = -0.53f;
        _selectProjector.depth = deep;
        _selectProjector.alpha = 0.3f + _projectorSin.normalized * 0.2f;
        _selectPlatform.depth = deep;
        int num = _profiles.Count;
        int index = 0;
        foreach (Profile p in _profiles)
        {
            Color darken = new Color(0.35f, 0.5f, 0.6f);
            p.persona.sprite.alpha = Maths.Clamp(_swapFade, 0f, 1f);
            p.persona.sprite.color = darken * (0.7f + _projectorSin.normalized * 0.1f);
            p.persona.sprite.color = new Color(p.persona.sprite.color.r, p.persona.sprite.color.g, p.persona.sprite.color.b);
            p.persona.sprite.flipH = false;
            p.persona.armSprite.alpha = Maths.Clamp(_swapFade, 0f, 1f);
            p.persona.armSprite.color = darken * (0.7f + _projectorSin.normalized * 0.1f);
            p.persona.armSprite.color = new Color(p.persona.armSprite.color.r, p.persona.armSprite.color.g, p.persona.armSprite.color.b);
            p.persona.armSprite.flipH = false;
            p.persona.sprite.scale = new Vec2(1f, 1f);
            p.persona.armSprite.scale = new Vec2(1f, 1f);
            float sizePerDuck = 12f;
            float xpos = base.x - (float)(num - 1) * sizePerDuck / 2f + (float)index * sizePerDuck;
            p.persona.sprite.depth = deep + 0.01f + (float)index * 0.001f;
            p.persona.armSprite.depth = deep + 0.02f + (float)index * 0.001f;
            Graphics.Draw(p.persona.sprite, xpos + 1f, base.y - 17f);
            Graphics.Draw(p.persona.armSprite, xpos + 1f - 3f, base.y - 17f + 6f);
            Team t = p.team;
            if (t != null)
            {
                Vec2 offset = DuckRig.GetHatPoint(p.persona.sprite.imageIndex);
                SpriteMap hat = p.team.GetHat(p.persona);
                hat.depth = p.persona.sprite.depth + 1;
                hat.alpha = p.persona.sprite.alpha;
                hat.color = p.persona.sprite.color;
                hat.center = new Vec2(16f, 16f) + t.hatOffset;
                hat.flipH = false;
                Graphics.Draw(hat, xpos + offset.x + 1f, base.y - 17f + offset.y);
                hat.color = Color.White;
            }
            _profile.persona.sprite.color = Color.White;
            _profile.persona.armSprite.color = Color.White;
            index++;
        }
        Graphics.Draw(_selectPlatform, base.x, base.y);
        Graphics.Draw(_selectProjector, base.x, base.y - 6f);
    }
}
