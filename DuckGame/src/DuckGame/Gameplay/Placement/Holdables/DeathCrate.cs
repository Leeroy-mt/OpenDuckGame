using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace DuckGame;

[EditorGroup("Stuff|Props")]
[BaggedProperty("isOnlineCapable", true)]
public class DeathCrate : Holdable, IPlatform
{
    #region Public Fields

    public bool activated;

    public byte _beeps;
    public byte settingIndex;

    public StateBinding _settingIndexBinding = new(nameof(settingIndex));
    public StateBinding _activatedBinding = new(nameof(activated));

    public DeathCrateSetting _storedSetting;

    #endregion

    #region Private Fields

    static List<DeathCrateSetting> _settings = new List<DeathCrateSetting>();

    bool _didActivation;

    SpriteMap _sprite;

    #endregion

    public DeathCrateSetting setting
    {
        get
        {
            if (settingIndex < _settings.Count)
                return _settings[settingIndex];
            return new DCSwordAdventure();
        }
    }

    public DeathCrate(float xpos, float ypos)
        : base(xpos, ypos)
    {
        _maxHealth = 15;
        _hitPoints = 15;
        _sprite = new SpriteMap("deathcrate", 16, 19);
        graphic = _sprite;
        Center = new Vector2(8, 11);
        collisionOffset = new Vector2(-8, -11);
        collisionSize = new Vector2(16, 18);
        Depth = -0.5f;
        _editorName = "Death Crate";
        editorTooltip = "Explodes in a violent surprise when triggered.";
        thickness = 2;
        weight = 5;
        _sprite.AddAnimation("idle", 1, true, default(int));
        _sprite.AddAnimation("activate", 0.35f, false, 1, 2, 3, 4, 4, 5, 4, 4, 5, 6, 6, 6, 6, 6, 6, 6, 6, 5, 7, 7, 7, 7, 7, 7, 7, 7, 5, 8, 8, 8, 8, 8, 8, 8, 8, 5, 9, 9, 5);
        _sprite.SetAnimation("idle");
        _holdOffset = new Vector2(2, 0);
        flammable = 0;
        collideSounds.Add("crateHit");

        for (int i = 0; i < 100; i++)
        {
            settingIndex = (byte)Rando.Int(_settings.Count - 1);
            if (_settings[settingIndex].likelyhood == 1 || Rando.Float(1) < _settings[settingIndex].likelyhood)
                break;
        }
    }

    #region Public Methods

    public static void InitializeDeathCrateSettings()
    {
        if (MonoMain.moddingEnabled)
        {
            foreach (Type type in ManagedContent.DeathCrateSettings.SortedTypes)
                _settings.Add(Activator.CreateInstance(type) as DeathCrateSetting);

            return;
        }

        foreach (Type t in Editor.GetSubclasses(typeof(DeathCrateSetting)))
            _settings.Add(Activator.CreateInstance(t) as DeathCrateSetting);
    }

    public override void OnSolidImpact(MaterialThing with, ImpactedFrom from)
    {
        if (with.isStateObject)
            with.Fondle(this);

        if (from == ImpactedFrom.Top || (Math.Abs(AngleDegrees) > 90 && Math.Abs(AngleDegrees) < 270 && from == ImpactedFrom.Bottom && with.totalImpactPower + totalImpactPower > 0.1f && _sprite.currentAnimation == "idle"))
        {
            activated = true;
            _sprite.SetAnimation("activate");
            SFX.Play("click");
            collisionOffset = new Vector2(-8, -8);
            collisionSize = new Vector2(16, 15);
        }

        base.OnSolidImpact(with, from);
    }

    public override void Terminate()
    {
        duck?.ThrowItem();
        base.Terminate();
    }

    public void ActivateSetting(bool isServer)
    {
        _didActivation = true;
        _storedSetting = setting;
        _storedSetting.Activate(this, isServer);

        if (isServer)
            Send.Message(new NMActivateDeathCrate(settingIndex, this));
    }

    public override void Update()
    {
        if (activated && _sprite.currentAnimation != "activate")
        {
            _sprite.SetAnimation("activate");
            collisionOffset = new Vector2(-8, -8);
            collisionSize = new Vector2(16, 15);
        }

        if (_sprite.imageIndex == 6 && _beeps == 0)
        {
            SFX.Play("singleBeep");
            _beeps++;
        }

        if (_sprite.imageIndex == 7 && _beeps == 1)
        {
            SFX.Play("singleBeep");
            _beeps++;
        }

        if (_sprite.imageIndex == 8 && _beeps == 2)
        {
            SFX.Play("singleBeep");
            _beeps++;
        }

        if (_sprite.imageIndex == 5 && _beeps == 3)
        {
            SFX.Play("doubleBeep", 1f, 0.2f);
            _beeps++;
        }

        if (isServerForObject)
        {
            if (_didActivation && _storedSetting != null)
                _storedSetting.Update(this);

            if (_sprite.currentAnimation == "activate" && _sprite.finished && !_didActivation)
                ActivateSetting(isServer: true);
        }

        base.Update();
    }

    public override void Draw()
    {
        var off = offDir;
        offDir = 1;
        base.Draw();
        offDir = off;
    }

    #endregion

    protected override bool OnDestroy(DestroyType type = null)
    {
        return false;
    }
}