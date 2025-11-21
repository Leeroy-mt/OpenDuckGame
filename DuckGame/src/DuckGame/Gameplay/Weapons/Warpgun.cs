using System;
using System.Collections.Generic;

namespace DuckGame;

[EditorGroup("Guns|Lasers")]
[BaggedProperty("isOnlineCapable", true)]
[BaggedProperty("isInDemo", true)]
public class Warpgun : Gun
{
	public class BlockGlow
	{
		public Block block;

		public float glow = 1f;

		public Vec2 pos;
	}

	public StateBinding _gravMultTimeBinding = new StateBinding("gravMultTime");

	public StateBinding _shotsSinceGroundedBinding = new StateBinding("shotsSinceGrounded");

	public StateBinding _shotsSinceDuckWasGroundedBinding = new StateBinding("shotsSinceDuckWasGrounded");

	private SpriteMap _sprite;

	private Sprite _warpLine;

	public int shotsSinceGrounded;

	protected new Sprite _sightHit;

	private Tex2D _laserTex;

	private int maxUngroundedShots = 2;

	public List<WarpLine> warpLines = new List<WarpLine>();

	private Vec2 warpPos;

	private bool onUpdate;

	public List<BlockGlow> blockGlows = new List<BlockGlow>();

	private int shotsSinceDuckWasGrounded;

	private int framesSinceShot;

	private float lerpShut;

	private Vec2 _warpPoint;

	private float gravMultTime;

	private bool warped;

	private Duck lastDuck;

	public new Vec2 laserOffset => Offset(new Vec2(16f, 9f) - center + new Vec2(-15f, 1f));

	public Warpgun(float xval, float yval)
		: base(xval, yval)
	{
		_fireWait = 0.3f;
		ammo = 9999;
		_ammoType = new ATWagnus();
		angleMul = -1f;
		_type = "gun";
		_sprite = new SpriteMap("warpgun", 19, 17);
		_sprite.speed = 0f;
		graphic = _sprite;
		center = new Vec2(11f, 8f);
		collisionOffset = new Vec2(-6f, -7f);
		collisionSize = new Vec2(12f, 14f);
		_barrelOffsetTL = new Vec2(14f, 4f);
		_fireSound = "warpgun";
		_kickForce = 0.3f;
		_fireRumble = RumbleIntensity.Kick;
		_holdOffset = new Vec2(-1f, -2f);
		_editorName = "WAGNUS";
		editorTooltip = "Science can be horrifying *and* FUN!";
		physicsMaterial = PhysicsMaterial.Metal;
		_warpLine = new Sprite("warpLine2");
		_sightHit = new Sprite("laserSightHit");
		_sightHit.CenterOrigin();
		_laserTex = Content.Load<Tex2D>("pointerLaser");
	}

	protected override void PlayFireSound()
	{
		PlaySFX(_fireSound, 1f, 0.6f + Rando.Float(0.2f));
	}

	public override void CheckIfHoldObstructed()
	{
		if (owner is Duck duckOwner)
		{
			if (duckOwner is TargetDuck)
			{
				Block hit = Level.CheckLine<Block>(position + new Vec2((offDir > 0) ? (-16) : 16, 0f), duckOwner.position + new Vec2((offDir > 0) ? (-16) : 16, 8f));
				duckOwner.holdObstructed = hit != null;
			}
			else
			{
				Block hit2 = Level.CheckLine<Block>(duckOwner.position + new Vec2((offDir > 0) ? (-10) : 10, 0f), duckOwner.position + new Vec2((offDir > 0) ? (-10) : 10, 10f));
				duckOwner.holdObstructed = hit2 != null;
			}
		}
	}

	public override void UpdateAction()
	{
		if (!base.isServerForObject || onUpdate)
		{
			return;
		}
		onUpdate = true;
		if (base.duck != null && tape == null)
		{
			offDir = base.duck.offDir;
			CheckIfHoldObstructed();
			if (!base.duck._hovering && base.duck.holdObject != null && (!base.duck.HasEquipment(typeof(Holster)) || !base.duck.inputProfile.Down("UP")))
			{
				base.duck.UpdateHoldLerp(updateLerp: true, instant: true);
				base.duck.UpdateHoldPosition();
			}
		}
		base.UpdateAction();
		onUpdate = false;
	}

	public override void Update()
	{
		ammo = 9999;
		if (base.isServerForObject && !_triggerHeld)
		{
			gravMultTime = 0f;
		}
		IPlatform near = Level.Nearest<IPlatform>(base.x, base.y);
		bool close = false;
		if (near != null && ((near as Thing).position - position).length < 32f)
		{
			close = true;
		}
		if (!close)
		{
			near = Level.CheckCircle<IPlatform>(position, 18f);
			if (near != null)
			{
				close = true;
			}
		}
		if (near != null && close && shotsSinceGrounded > 0 && framesSinceShot > 2)
		{
			if (shotsSinceGrounded > 1)
			{
				SFX.PlaySynchronized("laserChargeTeeny", 0.8f, 0.3f);
			}
			shotsSinceGrounded = 0;
			for (int i = 0; i < 8; i++)
			{
				float dir = (float)i * 45f - 5f + Rando.Float(20f);
				Vec2 vec = new Vec2((float)Math.Cos(Maths.DegToRad(dir)), (float)Math.Sin(Maths.DegToRad(dir)));
				if (Level.CheckLine<IPlatform>(position, position + vec * 32f, out var hitPos) != null)
				{
					blockGlows.Add(new BlockGlow
					{
						pos = hitPos
					});
					for (int j = 0; j < 4; j++)
					{
						Level.Add(WagnusChargeParticle.New(hitPos.x + Rando.Float(-4f, 4f), hitPos.y + Rando.Float(-4f, 4f), this));
					}
				}
			}
		}
		base.ammoType.range = 128f;
		if (tape != null && tape.gun1 is Warpgun && tape.gun2 is Warpgun)
		{
			if (this == tape.gun2)
			{
				heat = (tape.gun1 as Warpgun).heat;
			}
			base.ammoType.range *= 2f;
		}
		if (base.isServerForObject && heat > 1f)
		{
			explode = true;
			PressAction();
		}
		if (base.duck != null)
		{
			if (base.isServerForObject)
			{
				if (base.duck.grounded)
				{
					shotsSinceDuckWasGrounded = 0;
					if (heat > 0f)
					{
						heat -= 0.05f;
					}
				}
				if (!infinite)
				{
					if (shotsSinceDuckWasGrounded >= 16)
					{
						heat = 1f;
					}
					else if (!infinite)
					{
						float minheat = Math.Min((float)shotsSinceDuckWasGrounded / 38f, 1f);
						if (heat < minheat)
						{
							heat = minheat;
						}
					}
				}
			}
			if (base.isServerForObject)
			{
				if (gravMultTime > 0f && !base.duck.inPipe)
				{
					heat += 0.005f;
					if (warped)
					{
						base.duck.blendColor = Lerp.Color(Color.White, Color.Purple, gravMultTime);
						base.duck.position = warpPos;
						base.duck.vSpeed = -0.3f;
						base.duck.hSpeed = -0.3f;
					}
				}
				else
				{
					if (warped)
					{
						base.duck.gravMultiplier = 1f;
						base.duck.blendColor = Color.White;
					}
					gravMultTime = 0f;
				}
			}
			lastDuck = base.duck;
		}
		else if (lastDuck != null)
		{
			lastDuck.blendColor = Color.White;
			lastDuck.gravMultiplier = 1f;
			gravMultTime = 0f;
			warped = false;
			lastDuck = null;
		}
		if (shotsSinceGrounded == 0 || (bool)infinite)
		{
			_sprite.frame = 0;
		}
		else if (shotsSinceGrounded == 1)
		{
			_sprite.frame = 1;
		}
		else
		{
			lerpShut += 0.2f;
			if (lerpShut < 0.4f)
			{
				_sprite.frame = 2;
			}
			else if (lerpShut < 0.8f)
			{
				_sprite.frame = 3;
			}
			else
			{
				_sprite.frame = 4;
			}
		}
		framesSinceShot++;
		base.Update();
	}

	public override void OnPressAction()
	{
		if (!base.isServerForObject)
		{
			return;
		}
		Vec2 pos = position;
		bool slide = false;
		if (base.duck != null)
		{
			if (base.duck.holdObject is TapedGun)
			{
				(base.duck.holdObject as TapedGun).UpdatePositioning();
			}
			pos = base.duck.position;
			if (base.duck.sliding)
			{
				pos.y += 4f;
				slide = true;
			}
			else if (base.duck.crouch)
			{
				pos.y += 4f;
				slide = true;
			}
		}
		if (shotsSinceGrounded >= maxUngroundedShots && !infinite)
		{
			return;
		}
		lerpShut = 0f;
		float ang = 0f - angle;
		if (offDir < 0)
		{
			ang += (float)Math.PI;
		}
		Vec2 final = Vec2.Zero;
		float finalLen = 999999f;
		float middleLen = 999999f;
		Vec2 centralDestNorm = Vec2.Zero;
		Thing bottomCollide = null;
		float size = 7f;
		if (slide)
		{
			size = 5f;
		}
		float sameThresh = 8f;
		if (base.angleDegrees != 0f && Math.Abs(base.angleDegrees) != 90f && Math.Abs(base.angleDegrees) != 180f)
		{
			sameThresh = 24f;
		}
		int normSub = 6;
		if ((float)Math.Abs((int)base.angleDegrees) < 70f && (float)Math.Abs((int)base.angleDegrees) > 65f)
		{
			normSub = 12;
		}
		if ((float)Math.Abs((int)base.angleDegrees) > -70f)
		{
			_ = (float)Math.Abs((int)base.angleDegrees);
			_ = -65f;
		}
		for (int i = 0; i < 3; i++)
		{
			Vec2 destAim = new Vec2((float)Math.Cos(ang) * 134f, (float)(0.0 - Math.Sin(ang)) * 134f);
			if (Math.Abs(destAim.x) < 16f)
			{
				size = 2f;
				normSub = 8;
			}
			float off = 0f - size + (float)i * size;
			Vec2 testPos = pos + new Vec2((float)Math.Cos((double)ang + Math.PI / 2.0) * off, (float)(0.0 - Math.Sin((double)ang + Math.PI / 2.0)) * off);
			Vec2 dest = testPos - destAim;
			Vec2 destNorm = -(testPos - dest).normalized;
			Vec2 hit = Vec2.Zero;
			Thing b = Level.CheckRay<Desk>(testPos + destNorm * 8f, dest + new Vec2(0.2f, 0.2f), out hit);
			if (b != null && (b as Desk).flipped == 0)
			{
				b = null;
			}
			if (b == null)
			{
				b = Level.CheckRay<Block>(testPos, dest + new Vec2(0.2f, 0.2f), out hit);
			}
			if (b != null)
			{
				Vec2 dif = testPos - (hit + destNorm * -normSub);
				Vec2 set = (final = hit + destNorm * -normSub);
				if (i == 1)
				{
					middleLen = dif.length;
				}
				if (dif.length < finalLen)
				{
					finalLen = dif.length;
					dest = set;
				}
			}
			else if ((testPos - dest).length < finalLen)
			{
				finalLen = (testPos - dest).length - 7f;
				final = testPos - (dest + destNorm * -7f);
				if (i == 1)
				{
					middleLen = finalLen;
				}
			}
			if (i == 1)
			{
				centralDestNorm = destNorm;
			}
			if (i == 2)
			{
				bottomCollide = b;
			}
		}
		if (middleLen < 99999f && sameThresh > 9f && Math.Abs(middleLen - finalLen) < sameThresh)
		{
			finalLen = middleLen;
		}
		warpLines.Add(new WarpLine
		{
			start = pos,
			end = pos + centralDestNorm * finalLen,
			lerp = 0.6f,
			wide = ((base.duck != null && base.duck.sliding) ? 14 : 24)
		});
		if (base.isServerForObject)
		{
			_ammoType.range = finalLen - 8f;
			_barrelOffsetTL = new Vec2(8f, 3f);
			Fire();
			if (Network.isActive)
			{
				Send.Message(new NMFireGun(this, firedBullets, bulletFireIndex, rel: true, (byte)((base.duck != null) ? base.duck.netProfileIndex : 4)), NetMessagePriority.Urgent);
				firedBullets.Clear();
			}
			_wait = 0f;
			_barrelOffsetTL = new Vec2(8f, 15f);
			Fire();
			if (Network.isActive)
			{
				Send.Message(new NMFireGun(this, firedBullets, bulletFireIndex, rel: true, (byte)((base.duck != null) ? base.duck.netProfileIndex : 4)), NetMessagePriority.Urgent);
				firedBullets.Clear();
			}
			_wait = 0f;
			_barrelOffsetTL = new Vec2(8f, 9f);
			Fire();
			if (Network.isActive)
			{
				Send.Message(new NMFireGun(this, firedBullets, bulletFireIndex, rel: true, (byte)((base.duck != null) ? base.duck.netProfileIndex : 4)), NetMessagePriority.Urgent);
				firedBullets.Clear();
			}
			_wait = 0f;
			_barrelOffsetTL = new Vec2(8f, 4f);
			if (base.duck != null)
			{
				if (final.y < base.duck.y - 16f && Math.Abs(final.x - base.duck.x) < 16f)
				{
					finalLen -= 2f;
				}
				base.duck.position = base.duck.position + centralDestNorm * finalLen;
				base.duck.sleeping = false;
				base.duck._disarmDisable = 10;
				base.duck.gravMultiplier = 0f;
				base.duck.OnTeleport();
				base.duck.blendColor = Color.Purple;
				warped = true;
				gravMultTime = 1f;
				Block b2 = Level.CheckLine<Block>(new Vec2(base.duck.position.x, base.duck.bottom - 5f), new Vec2(base.duck.position.x, base.duck.bottom - 2f));
				if (b2 != null)
				{
					base.duck.bottom = b2.top;
				}
				IPlatform plat = Level.CheckLine<IPlatform>(new Vec2(base.duck.position.x, base.duck.bottom - 2f), new Vec2(base.duck.position.x, base.duck.bottom + 1f), bottomCollide);
				if (plat != null && (plat as Thing).solid && (bottomCollide == null || bottomCollide.top < (plat as Thing).top - 0.5f))
				{
					base.duck.bottom = (plat as Thing).top;
				}
				warpPos = base.duck.position;
			}
			else if (owner == null)
			{
				position = pos + centralDestNorm * finalLen;
				base.sleeping = false;
			}
			if (owner != null)
			{
				Thing thing = owner;
				float num = (owner.vSpeed = -0.01f);
				thing.hSpeed = num;
				if (owner is MaterialThing)
				{
					foreach (MaterialThing m in Level.CheckRectAll<MaterialThing>(owner.topLeft, owner.bottomRight))
					{
						m.OnSoftImpact(owner as MaterialThing, ImpactedFrom.Top);
						if (owner != null)
						{
							m.Touch(owner as MaterialThing);
						}
					}
				}
			}
		}
		framesSinceShot = 0;
		shotsSinceGrounded++;
		shotsSinceDuckWasGrounded++;
		if (heat > 0.8f)
		{
			explode = true;
			PressAction();
		}
		if (base.level != null && base.y < base.level.topLeft.y - 256f)
		{
			shotsSinceDuckWasGrounded = 16;
			heat = 1f;
		}
		if (shotsSinceDuckWasGrounded == 15 && !infinite)
		{
			SFX.PlaySynchronized("wagnusAlert", 0.8f);
		}
		if (shotsSinceGrounded == maxUngroundedShots && !infinite)
		{
			SFX.PlaySynchronized("laserUnchargeShortLoud", 1f, 0.7f);
		}
	}

	public override void Draw()
	{
		base.Draw();
	}

	public override void DrawGlow()
	{
		foreach (BlockGlow b in blockGlows)
		{
			Graphics.DrawTexturedLine(_warpLine.texture, b.pos, b.pos + new Vec2(0f, -4f), Color.Purple * b.glow, 0.25f, 0.9f);
			Graphics.DrawTexturedLine(_warpLine.texture, b.pos, b.pos + new Vec2(0f, 4f), Color.Purple * b.glow, 0.25f, 0.9f);
			b.glow -= 0.05f;
		}
		blockGlows.RemoveAll((BlockGlow x) => x.glow < 0.01f);
		Color c = Color.Purple;
		foreach (WarpLine l in warpLines)
		{
			Vec2 vec = l.start - l.end;
			Vec2 vec2 = l.end - l.start;
			Graphics.DrawTexturedLine(_warpLine.texture, l.end + vec * (1f - l.lerp), l.end, c * 0.8f, l.wide / 32f, 0.9f);
			Graphics.DrawTexturedLine(_warpLine.texture, l.start + vec2 * l.lerp, l.start, c * 0.8f, l.wide / 32f, 0.9f);
			l.lerp += 0.1f;
		}
		warpLines.RemoveAll((WarpLine v) => v.lerp >= 1f);
		if (base.duck != null && visible)
		{
			if (gravMultTime > 0f)
			{
				Graphics.DrawTexturedLine(_warpLine.texture, new Vec2(base.duck.x, base.duck.y), new Vec2(base.duck.x, base.duck.top - 8f), c * (gravMultTime + 0.2f), 0.7f, 0.9f);
				Graphics.DrawTexturedLine(_warpLine.texture, new Vec2(base.duck.x, base.duck.y), new Vec2(base.duck.x, base.duck.bottom + 8f), c * (gravMultTime + 0.2f), 0.7f, 0.9f);
			}
			if (shotsSinceGrounded < maxUngroundedShots || (bool)infinite)
			{
				float ang = 0f - angle;
				if (offDir < 0)
				{
					ang += (float)Math.PI;
				}
				Vec2 pos = laserOffset;
				Vec2 dest = pos - new Vec2((float)Math.Cos(ang) * 122f, (float)(0.0 - Math.Sin(ang)) * 122f);
				Vec2 destNorm = -(pos - dest).normalized;
				Vec2 hit = Vec2.Zero;
				if (Level.CheckRay<Block>(pos, dest + new Vec2(0.2f, 0.2f), out hit) != null)
				{
					_warpPoint = hit + destNorm * -9f;
					dest = hit;
				}
				else
				{
					_warpPoint = dest + new Vec2(-5f, 0f);
				}
				Graphics.DrawTexturedLine(_laserTex, pos, dest, Color.Red, 0.5f, base.depth - 1);
				if (_sightHit != null)
				{
					_sightHit.color = Color.Red;
					Graphics.Draw(_sightHit, dest.x, dest.y);
				}
			}
		}
		base.DrawGlow();
	}
}
