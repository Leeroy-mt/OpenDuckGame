using System.Collections.Generic;

namespace DuckGame;

[EditorGroup("Stuff|Props|Barrels")]
[BaggedProperty("noRandomSpawningOnline", true)]
public class YellowBarrel : Holdable, IPlatform, ISequenceItem
{
	public EditorProperty<bool> valid;

	protected float damageMultiplier = 1f;

	protected SpriteMap _sprite;

	protected float _fluidLevel = 1f;

	protected int _alternate;

	private List<FluidStream> _holes = new List<FluidStream>();

	protected FluidData _fluid;

	protected Sprite _melting;

	protected SpriteMap _toreUp;

	private bool _bottomHoles;

	private float _lossAccum;

	public YellowBarrel(float xpos, float ypos)
		: base(xpos, ypos)
	{
		valid = new EditorProperty<bool>(val: false, this);
		_maxHealth = 15f;
		_hitPoints = 15f;
		graphic = new Sprite("yellowBarrel");
		center = new Vec2(7f, 8f);
		_melting = new Sprite("yellowBarrelMelting");
		_toreUp = new SpriteMap("yellowBarrelToreUp", 14, 17);
		_toreUp.frame = 1;
		_toreUp.center = new Vec2(0f, -6f);
		base.sequence = new SequenceItem(this);
		base.sequence.type = SequenceItemType.Goody;
		collisionOffset = new Vec2(-7f, -8f);
		collisionSize = new Vec2(14f, 16f);
		base.depth = -0.1f;
		_editorName = "Barrel (Gasoline)";
		editorTooltip = "Do not smoke near this barrel. In fact, don't smoke at all. It's not cool, kids!";
		thickness = 4f;
		weight = 5f;
		physicsMaterial = PhysicsMaterial.Metal;
		base.collideSounds.Add("barrelThud");
		_holdOffset = new Vec2(1f, 0f);
		flammable = 0.3f;
		_fluid = Fluid.Gas;
		base.sequence.isValid = valid.value;
		_placementCost += 6;
	}

	public override void Initialize()
	{
		base.sequence.isValid = valid.value;
		base.Initialize();
	}

	public override bool Hit(Bullet bullet, Vec2 hitPos)
	{
		if (_hitPoints <= 0f)
		{
			return false;
		}
		hitPos += bullet.travelDirNormalized * 2f;
		if (1f - (hitPos.y - base.top) / (base.bottom - base.top) < _fluidLevel)
		{
			thickness = 2f;
			MakeHole(hitPos, bullet.travelDirNormalized);
			SFX.Play("bulletHitWater", 1f, Rando.Float(-0.2f, 0.2f));
			return base.Hit(bullet, hitPos);
		}
		thickness = 1f;
		return base.Hit(bullet, hitPos);
	}

	public void MakeHole(Vec2 pPos, Vec2 pImpaleDirection)
	{
		Vec2 offset = pPos - position;
		bool found = false;
		foreach (FluidStream hole in _holes)
		{
			if ((hole.offset - offset).length < 2f)
			{
				hole.offset = offset;
				hole.holeThickness += 0.5f;
				found = true;
				break;
			}
		}
		if (!found)
		{
			Vec2 holeVec = (-pImpaleDirection).Rotate(Rando.Float(-0.2f, 0.2f), Vec2.Zero);
			_holes.Add(new FluidStream(0f, 0f, holeVec, 1f, offset));
		}
	}

	public override void ExitHit(Bullet bullet, Vec2 exitPos)
	{
		exitPos -= bullet.travelDirNormalized * 2f;
		Vec2 offset = exitPos - position;
		bool found = false;
		foreach (FluidStream hole in _holes)
		{
			if ((hole.offset - offset).length < 2f)
			{
				hole.offset = offset;
				hole.holeThickness += 0.5f;
				found = true;
				break;
			}
		}
		if (!found)
		{
			Vec2 holeVec = bullet.travelDirNormalized;
			holeVec = holeVec.Rotate(Rando.Float(-0.2f, 0.2f), Vec2.Zero);
			_holes.Add(new FluidStream(0f, 0f, holeVec, 1f, offset));
		}
	}

	public override void Update()
	{
		base.Update();
		offDir = 1;
		if (_hitPoints <= 0f)
		{
			if (graphic != _toreUp)
			{
				float spray = _fluidLevel * 0.5f;
				float glob = _fluidLevel * 0.5f;
				FluidData dat = _fluid;
				dat.amount = spray / 20f;
				for (int i = 0; i < 20; i++)
				{
					Level.Add(new Fluid(base.x + Rando.Float(-4f, 4f), base.y + Rando.Float(-4f, 4f), new Vec2(Rando.Float(-4f, 4f), Rando.Float(-4f, 0f)), dat));
				}
				dat.amount = glob;
				Level.Add(new Fluid(base.x, base.y - 8f, new Vec2(0f, -1f), dat));
				Level.Add(SmallSmoke.New(base.x, base.y));
				SFX.Play("bulletHitWater");
				SFX.Play("crateDestroy");
			}
			graphic = _toreUp;
			_onFire = false;
			burnt = 0f;
			_fluidLevel = 0f;
			_weight = 0.1f;
			_collisionSize.y = 10f;
			_collisionOffset.y = -2f;
		}
		burnSpeed = 0.0015f;
		if (_onFire && burnt < 0.9f)
		{
			if (burnt > 0.3f)
			{
				graphic = _melting;
			}
			base.yscale = 0.5f + (1f - burnt) * 0.5f;
			base.centery = 8f - burnt * 7f;
			_collisionOffset.y = -8f + burnt * 7f;
			_collisionSize.y = 16f - burnt * 7f;
		}
		if (!_bottomHoles && burnt > 0.6f)
		{
			_bottomHoles = true;
			FluidStream hole = new FluidStream(0f, 0f, new Vec2(-1f, -1f), 1f, new Vec2(-7f, 8f));
			hole.holeThickness = 2f;
			_holes.Add(hole);
			hole = new FluidStream(0f, 0f, new Vec2(1f, -1f), 1f, new Vec2(7f, 8f));
			hole.holeThickness = 2f;
			_holes.Add(hole);
		}
		if (_owner != null)
		{
			hSpeed = owner.hSpeed;
			vSpeed = owner.vSpeed;
		}
		if (_fluidLevel > 0f && _alternate == 0)
		{
			foreach (FluidStream hole2 in _holes)
			{
				hole2.onFire = base.onFire;
				hole2.hSpeed = hSpeed;
				hole2.vSpeed = vSpeed;
				hole2.DoUpdate();
				hole2.position = Offset(hole2.offset);
				hole2.sprayAngle = OffsetLocal(hole2.startSprayAngle);
				float level = 1f - (hole2.offset.y - base.topLocal) / (base.bottomLocal - base.topLocal);
				if (!(hole2.x > base.left - 2f) || !(hole2.x < base.right + 2f) || !(level < _fluidLevel))
				{
					continue;
				}
				level = Maths.Clamp(_fluidLevel - level, 0.1f, 1f);
				FluidData f = _fluid;
				float loss = (f.amount = level * 0.008f * hole2.holeThickness);
				hole2.Feed(f);
				_fluidLevel -= loss;
				_lossAccum += loss;
				while (_lossAccum > 0.05f)
				{
					_lossAccum -= 0.05f;
					if (base.sequence != null && base.sequence.isValid && ChallengeLevel.running)
					{
						ChallengeLevel.goodiesGot++;
						SFX.Play("tinyTick");
					}
				}
			}
		}
		weight = _fluidLevel * 10f;
		_alternate++;
		if (_alternate > 4)
		{
			_alternate = 0;
		}
	}

	public override void Draw()
	{
		float level = 1f - _fluidLevel;
		float darken = 0.6f + (1f - burnt) * 0.4f;
		graphic.color = new Color((byte)(150f * darken), (byte)(150f * darken), (byte)(150f * darken));
		base.Draw();
		if (_hitPoints > 0f)
		{
			graphic.color = new Color((byte)(255f * darken), (byte)(255f * darken), (byte)(255f * darken));
			graphic.angle = angle;
			graphic.depth = base.depth + 1;
			graphic.scale = base.scale;
			float ypos = level * (float)graphic.height;
			graphic.center = center - new Vec2(0f, (int)ypos);
			Graphics.Draw(graphic, base.x, base.y, new Rectangle(0f, (int)ypos, graphic.w, (int)((float)graphic.h - ypos)));
		}
	}
}
