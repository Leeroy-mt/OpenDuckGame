using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;

namespace DuckGame;

[EditorGroup("Guns|Misc")]
[BaggedProperty("isFatal", false)]
public class MagnetGun : Gun
{
	public StateBinding _powerBinding = new StateBinding("_power");

	public StateBinding _grabbedBinding = new StateBinding("grabbed");

	public StateBinding _magnetActiveBinding = new StateBinding("_magnetActive");

	public StateBinding _keepRaisedBinding = new StateBinding("_keepRaised");

	public StateBinding _attachIndexBinding = new StateBinding("attachIndex");

	public NetIndex4 attachIndex = new NetIndex4(0);

	public NetIndex4 localAttachIndex = new NetIndex4(0);

	private Sprite _magnet;

	private SinWave _wave = 0.8f;

	private float _waveMult;

	public Thing _grabbed;

	private Block _stuck;

	private Vec2 _stickPos = Vec2.Zero;

	private Vec2 _stickNormal = Vec2.Zero;

	private Sound _beamSound;

	public bool _magnetActive;

	private List<MagnaLine> _lines = new List<MagnaLine>();

	private Vec2 _rayHit;

	private bool _hasRay;

	private bool prevMagActive;

	public float _power = 1f;

	public Thing _prevGrabDuck;

	public Thing grabbed
	{
		get
		{
			return _grabbed;
		}
		set
		{
			if (_grabbed != null && _grabbed != value)
			{
				ReleaseGrab(_grabbed);
			}
			_grabbed = value;
		}
	}

	public MagnetGun(float xval, float yval)
		: base(xval, yval)
	{
		ammo = 99;
		_ammoType = new ATLaser();
		_ammoType.range = 150f;
		_ammoType.accuracy = 0.8f;
		_ammoType.penetration = -1f;
		_type = "gun";
		graphic = new Sprite("magnetGun");
		center = new Vec2(16f, 16f);
		collisionOffset = new Vec2(-8f, -4f);
		collisionSize = new Vec2(14f, 9f);
		_barrelOffsetTL = new Vec2(24f, 14f);
		_fireSound = "smg";
		_fullAuto = true;
		_fireWait = 1f;
		_kickForce = 3f;
		_magnet = new Sprite("magnet");
		_magnet.CenterOrigin();
		_bio = "Nope.";
		_editorName = "Magnet Gun";
		editorTooltip = "Attracts metal objects. This seems like a bad idea.";
		_holdOffset = new Vec2(3f, 1f);
		_lowerOnFire = false;
	}

	public override void Initialize()
	{
		_beamSound = SFX.Get("magnetBeam", 0f, 0f, 0f, looped: true);
		int numLines = 10;
		for (int i = 0; i < numLines; i++)
		{
			_lines.Add(new MagnaLine(0f, 0f, this, _ammoType.range, (float)i / (float)numLines));
		}
		base.Initialize();
	}

	public override void CheckIfHoldObstructed()
	{
		if (_stuck == null)
		{
			base.CheckIfHoldObstructed();
		}
	}

	public override void Update()
	{
		_waveMult = Lerp.Float(_waveMult, 0f, 0.1f);
		if (base.isServerForObject)
		{
			_magnetActive = action && _power > 0.01f;
		}
		if (_magnetActive)
		{
			_waveMult = 1f;
		}
		if (base.isServerForObject && _magnetActive && !prevMagActive)
		{
			_power -= 0.01f;
		}
		prevMagActive = _magnetActive;
		if (_beamSound.Volume > 0.01f && _beamSound.State != SoundState.Playing)
		{
			_beamSound.Play();
		}
		else if (_beamSound.Volume < 0.01f && _beamSound.State == SoundState.Playing)
		{
			_beamSound.Stop();
		}
		_beamSound.Volume = Maths.LerpTowards(_beamSound.Volume, _magnetActive ? 0.1f : 0f, 0.1f);
		if (_power > 1f)
		{
			_power = 1f;
		}
		if (_power < 0f)
		{
			_power = 0f;
		}
		if (_power < 0.5f)
		{
			_beamSound.Pitch = -0.5f + _power;
		}
		else
		{
			_beamSound.Pitch = 0f;
		}
		if (base.isServerForObject && ((base.duck == null && base.grounded) || (base.duck != null && base.duck.grounded) || infinite.value))
		{
			_power = 1f;
		}
		Vec2 pos = Offset(base.barrelOffset);
		bool grabLoop = _grabbed is MagnetGun && (_grabbed as MagnetGun)._grabbed == this;
		if (_magnetActive && base.held && !grabLoop)
		{
			if (base.isServerForObject && base.duck != null && !base.duck.grounded)
			{
				_power -= 0.0015f;
			}
			foreach (MagnaLine line in _lines)
			{
				line.Update();
				line.show = true;
				float length = _ammoType.range;
				if (_hasRay)
				{
					length = (base.barrelPosition - _rayHit).length;
				}
				line.dist = length;
			}
			if (_grabbed == null && _stuck == null)
			{
				Holdable nearest = null;
				float nearestDist = 0f;
				Vec2 downVec = base.barrelVector.Rotate(Maths.DegToRad(90f), Vec2.Zero).normalized;
				for (int i = 0; i < 3; i++)
				{
					Vec2 checkPos = pos;
					switch (i)
					{
					case 0:
						checkPos += downVec * 8f;
						break;
					case 2:
						checkPos -= downVec * 8f;
						break;
					}
					foreach (Holdable thing in Level.CheckLineAll<Holdable>(checkPos, checkPos + base.barrelVector * _ammoType.range))
					{
						if (thing != this && thing != owner && thing.owner != owner && thing.physicsMaterial == PhysicsMaterial.Metal && (thing.duck == null || !(thing.duck.holdObject is MagnetGun)) && (thing.duck == null || !(thing.duck.holdObject is TapedGun) || thing == thing.duck.holdObject) && !(thing.owner is MagnetGun))
						{
							Holdable realThing = thing;
							if (thing.tape != null)
							{
								realThing = thing.tape;
							}
							float dist = (realThing.position - pos).length;
							if (nearest == null || dist < nearestDist)
							{
								nearestDist = dist;
								nearest = realThing;
							}
						}
					}
				}
				_hasRay = false;
				if (nearest != null && Level.CheckLine<Block>(pos, nearest.position) == null)
				{
					float power = (1f - Math.Min(nearestDist, _ammoType.range) / _ammoType.range) * 0.8f;
					Duck duckOwner = nearest.owner as Duck;
					if (duckOwner != null && !(duckOwner.holdObject is MagnetGun) && power > 0.3f)
					{
						if (!(nearest is Equipment) || nearest.equippedDuck == null)
						{
							duckOwner.ThrowItem(throwWithForce: false);
							duckOwner = null;
						}
						else if (nearest is TinfoilHat)
						{
							duckOwner.Unequip(nearest as Equipment);
							duckOwner = null;
						}
					}
					Vec2 vec = (pos - nearest.position).normalized;
					if (duckOwner != null && nearest is Equipment)
					{
						if (duckOwner.ragdoll != null)
						{
							duckOwner.ragdoll.makeActive = true;
							return;
						}
						if (!(nearest.owner.realObject is Duck) && Network.isActive)
						{
							return;
						}
						nearest.owner.realObject.hSpeed += vec.x * power;
						nearest.owner.realObject.vSpeed += vec.y * power * 4f;
						if ((nearest.owner.realObject as PhysicsObject).grounded && nearest.owner.realObject.vSpeed > 0f)
						{
							nearest.owner.realObject.vSpeed = 0f;
						}
					}
					else
					{
						Fondle(nearest);
						nearest.hSpeed += vec.x * power;
						nearest.vSpeed += vec.y * power * 4f;
						if (nearest.grounded && nearest.vSpeed > 0f)
						{
							nearest.vSpeed = 0f;
						}
					}
					_hasRay = true;
					_rayHit = nearest.position;
					if (base.isServerForObject && nearestDist < 20f)
					{
						if (nearest is Equipment && nearest.duck != null)
						{
							_grabbed = nearest.owner.realObject;
							RumbleManager.AddRumbleEvent(base.duck.profile, new RumbleEvent(RumbleIntensity.Kick, RumbleDuration.Pulse, RumbleFalloff.Short));
							nearest.duck.immobilized = true;
							nearest.duck.gripped = true;
							nearest.duck.ThrowItem();
							if (_grabbed != null && nearest.owner != null && !(_grabbed is Duck))
							{
								_grabbed.owner = this;
								_grabbed.offDir = offDir;
								Thing.SuperFondle(_grabbed, DuckNetwork.localConnection);
							}
						}
						else
						{
							_grabbed = nearest;
							RumbleManager.AddRumbleEvent(base.duck.profile, new RumbleEvent(RumbleIntensity.Kick, RumbleDuration.Pulse, RumbleFalloff.Short));
							nearest.owner = this;
							nearest.owner.offDir = offDir;
							if (nearest is Grenade)
							{
								(nearest as Grenade).OnPressAction();
							}
						}
						attachIndex += 1;
					}
				}
				else if (base.isServerForObject && _stuck == null && (Math.Abs(angle) < 0.05f || Math.Abs(angle) > 1.5f))
				{
					Vec2 checkPos2 = owner.position;
					if (base.duck.sliding)
					{
						checkPos2.y += 4f;
					}
					Vec2 rayHit;
					Block b = Level.CheckRay<Block>(checkPos2, checkPos2 + base.barrelVector * _ammoType.range, out rayHit);
					_hasRay = true;
					_rayHit = rayHit;
					if (b != null && b.physicsMaterial == PhysicsMaterial.Metal)
					{
						float power2 = (1f - Math.Min((b.position - checkPos2).length, _ammoType.range) / _ammoType.range) * 0.8f;
						Vec2 pull = rayHit - base.duck.position;
						float length2 = pull.length;
						pull.Normalize();
						owner.hSpeed += pull.x * power2;
						owner.vSpeed += pull.y * power2;
						if (length2 < 20f)
						{
							_stuck = b;
							RumbleManager.AddRumbleEvent(base.duck.profile, new RumbleEvent(RumbleIntensity.Kick, RumbleDuration.Pulse, RumbleFalloff.Short));
							_stickPos = rayHit;
							_stickNormal = -base.barrelVector;
							attachIndex += 1;
						}
					}
				}
			}
		}
		else
		{
			if (base.isServerForObject)
			{
				if (_grabbed != null)
				{
					ReleaseGrab(_grabbed);
					_grabbed = null;
					_collisionSize = new Vec2(14f, _collisionSize.y);
				}
				if (_stuck != null)
				{
					_stuck = null;
					if (owner != null && !_raised)
					{
						base.duck._groundValid = 6;
					}
				}
			}
			foreach (MagnaLine line2 in _lines)
			{
				line2.show = false;
			}
		}
		if (owner is MagnetGun && (owner as MagnetGun)._grabbed != this)
		{
			Fondle(this);
			ReleaseGrab(this);
		}
		if (Network.isActive)
		{
			if (_grabbed != null)
			{
				if (_grabbed is TrappedDuck && _grabbed.connection != connection)
				{
					_grabbed = (_grabbed as TrappedDuck)._duckOwner;
					if (_grabbed != null)
					{
						Duck obj = _grabbed as Duck;
						obj.immobilized = true;
						obj.gripped = true;
						obj.ThrowItem();
						obj._trapped = null;
					}
				}
				if (_grabbed is Duck d)
				{
					d.isGrabbedByMagnet = true;
					if (base.isServerForObject)
					{
						Fondle(d);
						Fondle(d.holdObject);
						foreach (Equipment e in d._equipment)
						{
							Fondle(e);
						}
						Fondle(d._ragdollInstance);
						Fondle(d._trappedInstance);
						Fondle(d._cookedInstance);
					}
				}
			}
			if (_grabbed == null && _prevGrabDuck != null && _prevGrabDuck is Duck)
			{
				(_prevGrabDuck as Duck).isGrabbedByMagnet = false;
			}
			_prevGrabDuck = _grabbed;
		}
		if (_grabbed != null && owner != null)
		{
			if (base.isServerForObject)
			{
				Fondle(_grabbed);
			}
			_grabbed.hSpeed = owner.hSpeed;
			_grabbed.vSpeed = owner.vSpeed;
			_grabbed.angle = angle;
			_grabbed.offDir = offDir;
			_grabbed.enablePhysics = false;
			if (_grabbed is TapedGun)
			{
				(_grabbed as TapedGun).UpdatePositioning();
			}
			_collisionSize = new Vec2(16f + _grabbed.width, _collisionSize.y);
			if (_grabbed is Duck d2)
			{
				d2.grounded = true;
				d2.sliding = false;
				d2.crouch = false;
			}
		}
		if (localAttachIndex < attachIndex)
		{
			for (int j = 0; j < 2; j++)
			{
				Level.Add(SmallSmoke.New(pos.x + Rando.Float(-1f, 1f), pos.y + Rando.Float(-1f, 1f)));
			}
			SFX.Play("grappleHook");
			for (int k = 0; k < 6; k++)
			{
				Level.Add(Spark.New(pos.x - base.barrelVector.x * 2f + Rando.Float(-1f, 1f), pos.y - base.barrelVector.y * 2f + Rando.Float(-1f, 1f), base.barrelVector + new Vec2(Rando.Float(-1f, 1f), Rando.Float(-1f, 1f))));
			}
			localAttachIndex = attachIndex;
		}
		if (base.isServerForObject)
		{
			if (_magnetActive && _raised && base.duck != null && !base.duck.grounded && _grabbed == null)
			{
				_keepRaised = true;
			}
			else
			{
				_keepRaised = false;
			}
			if (_stuck != null && base.duck != null)
			{
				if (_stickPos.y < owner.position.y - 8f)
				{
					owner.position = _stickPos + _stickNormal * 12f;
					_raised = true;
					_keepRaised = true;
				}
				else
				{
					owner.position = _stickPos + _stickNormal * 16f;
					_raised = false;
					_keepRaised = false;
				}
				Thing thing2 = owner;
				float num = (owner.vSpeed = 0f);
				thing2.hSpeed = num;
				base.duck.moveLock = true;
			}
			else if (_stuck == null && base.duck != null)
			{
				base.duck.moveLock = false;
			}
			if (owner == null && base.prevOwner != null)
			{
				if (base.prevOwner is Duck d3)
				{
					d3.moveLock = false;
				}
				_prevOwner = null;
			}
		}
		grabLoop = _grabbed is MagnetGun && (_grabbed as MagnetGun)._grabbed == this;
		if (_grabbed != null && !grabLoop)
		{
			if (_grabbed is Duck)
			{
				Vec2 poss = Offset(base.barrelOffset + new Vec2(0f, -6f));
				_grabbed.position = poss + base.barrelVector * _grabbed.halfWidth;
				(_grabbed as Duck).UpdateSkeleton();
				(_grabbed as Duck).gripped = true;
			}
			else
			{
				Vec2 poss2 = Offset(base.barrelOffset);
				_grabbed.position = poss2 + base.barrelVector * _grabbed.halfWidth;
			}
		}
		base.Update();
	}

	private void ReleaseGrab(Thing pThing)
	{
		pThing.angle = 0f;
		if (pThing is Holdable h)
		{
			h.owner = null;
			h.ReturnToWorld();
			ReturnItemToWorld(h);
		}
		if (pThing is Duck d)
		{
			d.immobilized = false;
			d.gripped = false;
			d.crippleTimer = 1f;
		}
		pThing.enablePhysics = true;
		pThing.hSpeed = base.barrelVector.x * 5f;
		pThing.vSpeed = base.barrelVector.y * 5f;
		if (pThing is EnergyScimitar)
		{
			(pThing as EnergyScimitar).StartFlying((offDir < 0) ? (0f - base.angleDegrees - 180f) : (0f - base.angleDegrees), pThrown: true);
		}
	}

	public override void Draw()
	{
		base.Draw();
		Draw(_magnet, new Vec2(5f, -2f + (float)_wave * _waveMult));
		foreach (MagnaLine line in _lines)
		{
			line.Draw();
		}
	}

	public override void OnPressAction()
	{
		_waveMult = 1f;
		if (base.raised)
		{
			_keepRaised = true;
		}
	}

	public override void OnHoldAction()
	{
	}

	public override void OnReleaseAction()
	{
		_keepRaised = false;
	}

	public override void Fire()
	{
	}
}
