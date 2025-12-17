using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace DuckGame;

[EditorGroup("Guns|Misc")]
[BaggedProperty("isFatal", false)]
public class Keytar : Gun
{
    private Sound noteSound;

    public string[] presets = new string[5] { "metalkeys", "touch", "deepbass", "strings2", "synthdrums" };

    public EditorProperty<int> color;

    private SpriteMap _sprite;

    private SpriteMap _keybed;

    private SpriteMap _settingStrip;

    public float handPitch;

    public float notePitch;

    public float prevNotePitch;

    private float _benderOffset;

    private float _bender;

    public sbyte preset;

    public int prevNote;

    public float playPitch;

    public byte colorVariation = byte.MaxValue;

    private byte _prevColorVariation = byte.MaxValue;

    public StateBinding _ruinedBinding = new StateBinding(nameof(_ruined));

    public StateBinding _benderBinding = new StateBinding(nameof(bender));

    public StateBinding _notePitchBinding = new StateBinding(nameof(notePitch));

    public StateBinding _handPitchBinding = new StateBinding(nameof(handPitch));

    public StateBinding _presetBinding = new StateBinding(nameof(preset));

    public StateBinding _brokenKeyBinding = new StateBinding(nameof(brokenKey));

    public StateBinding _colorVariationBinding = new StateBinding(nameof(colorVariation));

    private bool _prevRuined;

    private sbyte _prevPreset;

    private byte brokenKey;

    private List<Sound> _prevSounds = new List<Sound>();

    public bool duckMoving;

    public float bender
    {
        get
        {
            return Maths.Clamp(_bender + _benderOffset, 0f, 1f);
        }
        set
        {
            _bender = value;
        }
    }

    public int currentNote
    {
        get
        {
            int note = (int)Math.Round(handPitch * 13f);
            if (note < 0)
            {
                note = 0;
            }
            if (note > 12)
            {
                note = 12;
            }
            return note;
        }
    }

    public override void EditorPropertyChanged(object property)
    {
        RefreshColor();
    }

    public override Sprite GeneratePreview(int wide = 16, int high = 16, bool transparentBack = false, Effect effect = null, RenderTarget2D target = null)
    {
        color.value = 0;
        RefreshColor();
        return base.GeneratePreview(wide, high, transparentBack, effect, target);
    }

    public Keytar(float xval, float yval)
        : base(xval, yval)
    {
        ammo = 4;
        _ammoType = new ATLaser();
        _ammoType.range = 170f;
        _ammoType.accuracy = 0.8f;
        _type = "gun";
        _sprite = new SpriteMap("keytar", 23, 8);
        graphic = _sprite;
        center = new Vec2(12f, 3f);
        collisionOffset = new Vec2(-8f, -1f);
        collisionSize = new Vec2(16f, 7f);
        _barrelOffsetTL = new Vec2(12f, 3f);
        _fireSound = "smg";
        _fullAuto = true;
        _fireWait = 1f;
        _kickForce = 3f;
        _holdOffset = new Vec2(-8f, 2f);
        _keybed = new SpriteMap("keytarKeybed", 13, 4);
        _settingStrip = new SpriteMap("keytarSettingStrip", 9, 1);
        thickness = 1f;
        color = new EditorProperty<int>(-1, this, -1f, 3f, 1f);
        base.collideSounds.Add("rockHitGround");
        _canRaise = false;
        ignoreHands = true;
        _editorName = "Keytar";
        editorTooltip = "Eats batteries and steals hearts.";
        isFatal = false;
    }

    private void RefreshColor()
    {
        if (color.value < 0)
        {
            colorVariation = (byte)Rando.Int(3);
            if (Rando.Int(100) == 0 && !(Level.current is Editor))
            {
                colorVariation = 4;
            }
        }
        else
        {
            colorVariation = (byte)color.value;
        }
    }

    public override void Initialize()
    {
        RefreshColor();
        base.Initialize();
    }

    public override void CheckIfHoldObstructed()
    {
        if (owner is Duck duckOwner)
        {
            duckOwner.holdObstructed = false;
        }
    }

    public override void OnSolidImpact(MaterialThing with, ImpactedFrom from)
    {
        if (base.isServerForObject && (Math.Abs(hSpeed) > 4f || Math.Abs(vSpeed) > 4f) && !_ruined && owner == null)
        {
            _ruined = true;
        }
        base.OnSolidImpact(with, from);
    }

    public override bool Hit(Bullet bullet, Vec2 hitPos)
    {
        if (base.isServerForObject)
        {
            _ruined = true;
        }
        else if (bullet.isLocal && owner == null)
        {
            Thing.Fondle(this, DuckNetwork.localConnection);
            _ruined = true;
        }
        return base.Hit(bullet, hitPos);
    }

    public override bool Destroy(DestroyType type = null)
    {
        if (base.isServerForObject)
        {
            _ruined = true;
        }
        return false;
    }

    public override void Update()
    {
        if (colorVariation != _prevColorVariation)
        {
            _keybed = new SpriteMap((colorVariation == 4) ? "keytarKeybedBlue" : "keytarKeybed", 13, 4);
        }
        _prevColorVariation = colorVariation;
        if (!_prevRuined && _ruined)
        {
            SFX.Play("smallElectronicBreak", 0.8f, Rando.Float(-0.1f, 0.1f));
            for (int i = 0; i < 8; i++)
            {
                Level.Add(Spark.New(base.x + Rando.Float(-8f, 8f), base.y + Rando.Float(-4f, 4f), new Vec2(Rando.Float(-1f, 1f), Rando.Float(-1f, 1f))));
            }
            if (base.isServerForObject && Rando.Int(5) == 0)
            {
                brokenKey = (byte)(1 + Rando.Int(12));
            }
        }
        _prevRuined = _ruined;
        if (owner is Duck d)
        {
            if (base.isServerForObject && d.inputProfile != null)
            {
                if (_ruined && Rando.Int(20) == 0)
                {
                    _benderOffset += Rando.Float(-0.05f, 0.05f);
                }
                if (d.inputProfile.Pressed("STRAFE"))
                {
                    preset++;
                    if (preset >= presets.Length)
                    {
                        preset = 0;
                    }
                }
                handPitch = d.inputProfile.leftTrigger;
                bender = d.inputProfile.rightTrigger;
                if (d.inputProfile.hasMotionAxis)
                {
                    handPitch += d.inputProfile.motionAxis;
                }
                int keyboardNote = Keyboard.CurrentNote(d.inputProfile, this);
                if (keyboardNote >= 0)
                {
                    notePitch = (float)keyboardNote / 13f + 0.01f;
                    handPitch = notePitch;
                }
                else if (d.inputProfile.Down("SHOOT"))
                {
                    notePitch = handPitch + 0.01f;
                }
                else
                {
                    notePitch = 0f;
                }
            }
            else
            {
                _benderOffset = 0f;
            }
            if (noteSound != null && _ruined && Rando.Int(30) == 0)
            {
                noteSound.Volume *= 0.75f;
            }
            duckMoving = d._sprite.currentAnimation == "run";
            hideRightWing = true;
            ignoreHands = true;
            hideLeftWing = !d._hovering;
            if (noteSound != null && d._hovering)
            {
                noteSound.Stop();
                noteSound = null;
            }
            int note = currentNote;
            if (preset == presets.Length - 1)
            {
                note = (int)Math.Round((float)currentNote / 2f);
            }
            if (notePitch == 0f || ((note != prevNote || (noteSound != null && noteSound.Pitch + 1f != bender / 12f)) && !d._hovering))
            {
                if (notePitch != 0f)
                {
                    if (noteSound == null || note != prevNote)
                    {
                        bool broken = brokenKey != 0 && note == brokenKey - 1;
                        float vol = 1f;
                        if (_ruined)
                        {
                            vol -= 0.15f + Rando.Float(-0.15f);
                            if (Rando.Int(5) == 0)
                            {
                                vol -= 0.13f;
                            }
                            if (Rando.Int(7) == 0)
                            {
                                vol -= 0.25f;
                            }
                            if (broken)
                            {
                                vol = ((Rando.Int(3) == 0) ? 0.2f : 0f);
                            }
                            vol = Maths.Clamp(vol, 0f, 1f);
                        }
                        if (noteSound != null)
                        {
                            _prevSounds.Add(noteSound);
                        }
                        Sound snd = SFX.Play(presets[preset] + "-" + ((note < 10) ? "0" : "") + Change.ToString(note), vol, -1f);
                        noteSound = snd;
                        playPitch = notePitch;
                        prevNote = note;
                        if (_ruined)
                        {
                            _benderOffset = Rando.Float(0.05f, 0.1f);
                            if (Rando.Int(10) == 0)
                            {
                                _benderOffset = Rando.Float(0.15f, 0.2f);
                            }
                            if (broken)
                            {
                                _benderOffset += 0.1f + Rando.Float(0.2f);
                            }
                        }
                        if (!_ruined)
                        {
                            Level.Add(new MusicNote(base.barrelPosition.x, base.barrelPosition.y, base.barrelVector));
                        }
                    }
                    else
                    {
                        noteSound.Pitch = -1f + bender / 12f;
                    }
                }
                else
                {
                    if (noteSound != null)
                    {
                        _prevSounds.Add(noteSound);
                        noteSound = null;
                    }
                    prevNote = -1;
                }
            }
            handOffset = new Vec2(5f + (1f - handPitch) * 2f, -2f + (1f - handPitch) * 4f);
            handAngle = ((duckMoving ? 0f : (1f - handPitch)) * 0.2f + 0.2f + ((notePitch > 0f) ? 0.05f : 0f)) * (float)offDir;
            _holdOffset = new Vec2(-4f + handPitch * 1f, (0f - handPitch) * 1f + 3f - (float)(duckMoving ? 3 : 0) + (float)(base.duck._hovering ? 2 : 0));
            collisionOffset = new Vec2(-1f, -7f);
            collisionSize = new Vec2(2f, 16f);
        }
        else
        {
            collisionOffset = new Vec2(-8f, -2f);
            collisionSize = new Vec2(16f, 6f);
            if (noteSound != null)
            {
                _prevSounds.Add(noteSound);
                noteSound = null;
            }
        }
        for (int j = 0; j < _prevSounds.Count; j++)
        {
            if (_prevSounds[j].Volume < 0.01f)
            {
                _prevSounds[j].Stop();
                _prevSounds.RemoveAt(j);
                j--;
            }
            else
            {
                _prevSounds[j].Volume = Lerp.Float(_prevSounds[j].Volume, 0f, 0.15f);
            }
        }
        if (preset != _prevPreset)
        {
            SFX.Play("click");
        }
        _prevPreset = preset;
        prevNotePitch = notePitch;
        base.Update();
    }

    public override void OnPressAction()
    {
    }

    public override void OnReleaseAction()
    {
    }

    public override void Fire()
    {
    }

    public override void Draw()
    {
        _sprite.frame = (_ruined ? 1 : 0) + colorVariation * 2;
        if (base.duck != null && !base.raised)
        {
            SpriteMap spr = base.duck.profile.persona.fingerPositionSprite;
            if (!base.duck._hovering)
            {
                float xOffset = 0f;
                if (noteSound == null)
                {
                    spr.frame = 5;
                    xOffset = (int)(2f + (-4f + (float)currentNote / 12f * 8f));
                }
                else
                {
                    spr.frame = 6 + currentNote;
                    xOffset = 2f;
                }
                spr.depth = base.depth + 4;
                spr.flipH = offDir <= 0;
                spr.angle = angle;
                Vec2 pos = Offset(new Vec2(xOffset, -3f));
                Graphics.Draw(spr, pos.x, pos.y);
            }
            spr.frame = 19;
            Vec2 pos2 = Offset(new Vec2(-8f, (0f - bender) * 1f));
            Graphics.Draw(spr, pos2.x, pos2.y);
        }
        _keybed.depth = base.depth + 2;
        _keybed.flipH = offDir <= 0;
        _keybed.angle = angle;
        _keybed.frame = ((notePitch != 0f) ? (currentNote + 1) : 0);
        Vec2 bedOff = Offset(new Vec2(-5f, -2f));
        Graphics.Draw(_keybed, bedOff.x, bedOff.y);
        _settingStrip.depth = base.depth + 2;
        _settingStrip.flipH = offDir <= 0;
        _settingStrip.angle = angle;
        _settingStrip.frame = preset;
        Vec2 stripOff = Offset(new Vec2(-1f, 3f));
        Graphics.Draw(_settingStrip, stripOff.x, stripOff.y);
        base.Draw();
    }
}
