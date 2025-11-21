using System;
using System.Collections.Generic;

namespace DuckGame;

[EditorGroup("Guns|Misc")]
[BaggedProperty("isFatal", false)]
public class Trumpet : Gun
{
	public StateBinding _notePitchBinding = new StateBinding("notePitch");

	public float notePitch;

	private float prevNotePitch;

	private float hitPitch;

	private Sound noteSound;

	private List<InstrumentNote> _notes = new List<InstrumentNote>();

	private int currentPitch = -1;

	private bool leftPressed;

	private bool rightPressed;

	public Trumpet(float xval, float yval)
		: base(xval, yval)
	{
		ammo = 4;
		_ammoType = new ATLaser();
		_ammoType.range = 170f;
		_ammoType.accuracy = 0.8f;
		wideBarrel = true;
		barrelInsertOffset = new Vec2(-4f, -2f);
		_type = "gun";
		graphic = new Sprite("trumpet");
		center = new Vec2(12f, 5f);
		collisionOffset = new Vec2(-6f, -4f);
		collisionSize = new Vec2(12f, 8f);
		_barrelOffsetTL = new Vec2(24f, 4f);
		_fireSound = "smg";
		_fullAuto = true;
		_fireWait = 1f;
		_kickForce = 3f;
		_holdOffset = new Vec2(6f, 2f);
		hoverRaise = false;
		ignoreHands = true;
		_notePitchBinding.skipLerp = true;
		editorTooltip = "The poor man's trombone.";
		isFatal = false;
	}

	public override void Initialize()
	{
		base.Initialize();
	}

	public override void Update()
	{
		if (owner is Duck { inputProfile: not null } d)
		{
			hideLeftWing = (ignoreHands = !base.raised);
			if (base.isServerForObject)
			{
				if (d.inputProfile.Pressed("SHOOT"))
				{
					currentPitch = 2;
				}
				if (d.inputProfile.Pressed("STRAFE"))
				{
					currentPitch = 0;
				}
				if (d.inputProfile.Pressed("RAGDOLL"))
				{
					currentPitch = 1;
				}
				if (d.inputProfile.leftTrigger > 0.5f && !leftPressed)
				{
					currentPitch = 2;
					leftPressed = true;
				}
				if (d.inputProfile.rightTrigger > 0.5f && !rightPressed)
				{
					currentPitch = 3;
					rightPressed = true;
				}
				if (d.inputProfile.Released("STRAFE") && currentPitch == 0)
				{
					currentPitch = -1;
				}
				if (d.inputProfile.Released("SHOOT") && currentPitch == 2)
				{
					currentPitch = -1;
				}
				if (d.inputProfile.Released("RAGDOLL") && currentPitch == 1)
				{
					currentPitch = -1;
				}
				if (d.inputProfile.leftTrigger <= 0.5f)
				{
					if (currentPitch == 2 && leftPressed)
					{
						currentPitch = -1;
					}
					leftPressed = false;
				}
				if (d.inputProfile.rightTrigger <= 0.5f)
				{
					if (currentPitch == 3 && rightPressed)
					{
						currentPitch = -1;
					}
					rightPressed = false;
				}
				if (currentPitch >= 0 && !_raised)
				{
					notePitch = (float)currentPitch / 3f + 0.01f;
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
					if (noteSound != null)
					{
						noteSound.Stop();
						noteSound = null;
					}
					int note = (int)Math.Round(notePitch * 3f);
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
						Sound snd = SFX.Play("trumpet0" + Change.ToString(note + 1), 0.8f);
						noteSound = snd;
						Level.Add(new MusicNote(base.barrelPosition.x, base.barrelPosition.y, base.barrelVector));
					}
					else
					{
						noteSound.Pitch = Maths.Clamp((notePitch - hitPitch) * 0.01f, -1f, 1f);
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
				collisionOffset = new Vec2(4f, -4f);
				collisionSize = new Vec2(8f, 8f);
				_holdOffset = new Vec2(0f, 0f);
				handOffset = new Vec2(0f, 0f);
				OnReleaseAction();
			}
			else
			{
				collisionOffset = new Vec2(-6f, -4f);
				collisionSize = new Vec2(8f, 8f);
				_holdOffset = new Vec2(10f, -2f);
				handOffset = new Vec2(5f, -2f);
			}
		}
		else
		{
			leftPressed = false;
			rightPressed = false;
			currentPitch = -1;
			collisionOffset = new Vec2(-6f, -4f);
			collisionSize = new Vec2(8f, 8f);
			_holdOffset = new Vec2(6f, 2f);
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

	public override void Draw()
	{
		if (base.duck != null && !base.raised)
		{
			SpriteMap fingerPositionSprite = base.duck.profile.persona.fingerPositionSprite;
			fingerPositionSprite.frame = currentPitch + 1;
			fingerPositionSprite.depth = base.depth - 100;
			fingerPositionSprite.flipH = offDir <= 0;
			fingerPositionSprite.angle = 0f;
			Vec2 pos = Offset(new Vec2(-8f, -2f));
			Graphics.Draw(fingerPositionSprite, pos.x, pos.y);
		}
		base.Draw();
	}
}
