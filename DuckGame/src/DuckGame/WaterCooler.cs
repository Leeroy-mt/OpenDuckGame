using System;
using System.Collections.Generic;

namespace DuckGame;

[EditorGroup("Details")]
public class WaterCooler : MaterialThing, IPlatform
{
	private SpriteMap _sprite;

	private SpriteMap _jugLine;

	private Sprite _bottom;

	private SinWave _colorFlux = 0.1f;

	protected float _fluidLevel = 1f;

	protected int _alternate;

	private List<FluidStream> _holes = new List<FluidStream>();

	protected FluidData _fluid;

	public float _shakeMult;

	private float _shakeInc;

	public WaterCooler(float xpos, float ypos)
		: base(xpos, ypos)
	{
		_sprite = new SpriteMap("waterCoolerJug", 16, 16);
		graphic = _sprite;
		center = new Vec2(8f, 8f);
		collisionOffset = new Vec2(-5f, -5f);
		collisionSize = new Vec2(10f, 10f);
		base.depth = -0.5f;
		_editorName = "Water Cooler";
		editorTooltip = "Looking for all the latest hot gossip? This is the place to hang.";
		thickness = 2f;
		weight = 5f;
		_jugLine = new SpriteMap("waterCoolerJugLine", 16, 16);
		_jugLine.CenterOrigin();
		flammable = 0.3f;
		_bottom = new Sprite("waterCoolerBottom");
		_bottom.CenterOrigin();
		base.editorOffset = new Vec2(0f, -8f);
		_fluid = Fluid.Water;
	}

	protected override bool OnDestroy(DestroyType type = null)
	{
		return true;
	}

	public override bool Hit(Bullet bullet, Vec2 hitPos)
	{
		hitPos += bullet.travelDirNormalized * 2f;
		if (1f - (hitPos.y - base.top) / (base.bottom - base.top) < _fluidLevel)
		{
			thickness = 2f;
			Vec2 offset = hitPos - position;
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
				Vec2 holeVec = (-bullet.travelDirNormalized).Rotate(Rando.Float(-0.2f, 0.2f), Vec2.Zero);
				FluidStream newHole = new FluidStream(0f, 0f, holeVec, 1f, offset);
				_holes.Add(newHole);
				newHole.streamSpeedMultiplier = 2f;
			}
			_shakeMult = 1f;
			SFX.Play("bulletHitWater", 1f, Rando.Float(-0.2f, 0.2f));
			return base.Hit(bullet, hitPos);
		}
		thickness = 1f;
		return base.Hit(bullet, hitPos);
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
		_shakeInc += 0.8f;
		_shakeMult = Lerp.Float(_shakeMult, 0f, 0.05f);
		if (_alternate == 0)
		{
			foreach (FluidStream hole in _holes)
			{
				hole.onFire = base.onFire;
				hole.hSpeed = hSpeed;
				hole.vSpeed = vSpeed;
				hole.DoUpdate();
				hole.position = Offset(hole.offset);
				hole.sprayAngle = OffsetLocal(hole.startSprayAngle);
				float level = 1f - (hole.offset.y - base.topLocal) / (base.bottomLocal - base.topLocal);
				if (hole.x > base.left - 2f && hole.x < base.right + 2f && level < _fluidLevel)
				{
					level = Maths.Clamp(_fluidLevel - level, 0.1f, 1f);
					float loss = level * 0.0012f * hole.holeThickness;
					FluidData f = _fluid;
					f.amount = loss;
					hole.Feed(f);
					_fluidLevel -= loss;
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
		_sprite.frame = (int)((1f - _fluidLevel) * 10f);
		Vec2 pos = position;
		float shakeOffset = (float)Math.Sin(_shakeInc) * _shakeMult * 1f;
		position.x += shakeOffset;
		base.Draw();
		position = pos;
		_bottom.depth = base.depth + 1;
		Graphics.Draw(_bottom, base.x, base.y + 9f);
		_jugLine.depth = base.depth + 1;
		_jugLine.imageIndex = _sprite.imageIndex;
		_jugLine.alpha = _fluidLevel * 10f % 1f;
		Graphics.Draw(_jugLine, base.x + shakeOffset, base.y);
	}
}
