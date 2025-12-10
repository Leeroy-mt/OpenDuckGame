using System;
using System.Collections.Generic;

namespace DuckGame;

[EditorGroup("Guns|Misc")]
[BaggedProperty("isFatal", false)]
[BaggedProperty("previewPriority", true)]
public class Saxaphone : Gun
{
    public StateBinding _notePitchBinding = new StateBinding("notePitch");

    public StateBinding _handPitchBinding = new StateBinding("handPitch");

    public float notePitch;

    public float handPitch;

    private float prevNotePitch;

    private float hitPitch;

    private Sound noteSound;

    private List<InstrumentNote> _notes = new List<InstrumentNote>();

    public Saxaphone(float xval, float yval)
        : base(xval, yval)
    {
        ammo = 4;
        _ammoType = new ATLaser();
        _ammoType.range = 170f;
        _ammoType.accuracy = 0.8f;
        _type = "gun";
        graphic = new Sprite("saxaphone");
        center = new Vec2(20f, 18f);
        collisionOffset = new Vec2(-4f, -7f);
        collisionSize = new Vec2(8f, 16f);
        _barrelOffsetTL = new Vec2(24f, 16f);
        _fireSound = "smg";
        _fullAuto = true;
        _fireWait = 1f;
        _kickForce = 3f;
        _holdOffset = new Vec2(6f, 2f);
        holsterAngle = 90f;
        _notePitchBinding.skipLerp = true;
        _editorName = "Saxophone";
        editorTooltip = "Crave the dulcet tones of smooth jazz? This is the item for you.";
        isFatal = false;
    }

    public override void Initialize()
    {
        base.Initialize();
    }

    public override void Update()
    {
        if (owner is Duck d)
        {
            if (base.isServerForObject && d.inputProfile != null)
            {
                handPitch = d.inputProfile.leftTrigger;
                if (d.inputProfile.hasMotionAxis)
                {
                    handPitch += d.inputProfile.motionAxis;
                }
                int keyboardNote = Keyboard.CurrentNote(d.inputProfile, this);
                if (keyboardNote >= 0)
                {
                    notePitch = (float)keyboardNote / 12f + 0.01f;
                    handPitch = notePitch;
                    if (notePitch != prevNotePitch)
                    {
                        prevNotePitch = 0f;
                        if (noteSound != null)
                        {
                            noteSound.Stop();
                            noteSound = null;
                        }
                    }
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
            if (notePitch != prevNotePitch)
            {
                if (notePitch != 0f)
                {
                    int note = (int)Math.Round(notePitch * 12f);
                    if (note < 0)
                    {
                        note = 0;
                    }
                    if (note > 12)
                    {
                        note = 12;
                    }
                    if (noteSound == null)
                    {
                        hitPitch = notePitch;
                        Sound snd = SFX.Play("sax" + Change.ToString(note));
                        noteSound = snd;
                        Level.Add(new MusicNote(base.barrelPosition.x, base.barrelPosition.y, base.barrelVector));
                    }
                    else
                    {
                        noteSound.Pitch = Maths.Clamp((notePitch - hitPitch) * 0.1f, -1f, 1f);
                    }
                }
                else if (noteSound != null)
                {
                    noteSound.Stop();
                    noteSound = null;
                }
            }
            if (_raised)
            {
                handAngle = 0f;
                handOffset = new Vec2(0f, 0f);
                _holdOffset = new Vec2(0f, 2f);
                collisionOffset = new Vec2(-4f, -7f);
                collisionSize = new Vec2(8f, 16f);
                OnReleaseAction();
            }
            else
            {
                handOffset = new Vec2(5f + (1f - handPitch) * 2f, -2f + (1f - handPitch) * 4f);
                handAngle = (1f - handPitch) * 0.4f * (float)offDir;
                _holdOffset = new Vec2(4f + handPitch * 2f, handPitch * 2f);
                collisionOffset = new Vec2(-1f, -7f);
                collisionSize = new Vec2(2f, 16f);
            }
        }
        else
        {
            collisionOffset = new Vec2(-4f, -7f);
            collisionSize = new Vec2(8f, 16f);
        }
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
}
