using System;

namespace DuckGame;

[EditorGroup("Stuff|Props")]
[BaggedProperty("canSpawn", false)]
public class Trophy : Holdable
{
	public StateBinding _actionBinding = new StateBinding("ownerAction");

	private bool ownerAction;

	private SpriteMap _sprite;

	private int _framesSinceThrown;

	private float _throwSpin;

	public Trophy(float xpos, float ypos)
		: base(xpos, ypos)
	{
		_sprite = new SpriteMap("trophy", 17, 20);
		graphic = _sprite;
		center = new Vec2(8f, 10f);
		collisionOffset = new Vec2(-7f, -10f);
		collisionSize = new Vec2(15f, 19f);
		base.depth = -0.5f;
		thickness = 4f;
		weight = 4f;
		flammable = 0.3f;
		base.collideSounds.Add("rockHitGround2");
		physicsMaterial = PhysicsMaterial.Metal;
		_holdOffset = new Vec2(-2f, 0f);
		editorTooltip = "It doesn't count if you don't EARN it.";
	}

	public override void Update()
	{
		if (base.isServerForObject)
		{
			ownerAction = action;
		}
		if (base.duck != null && ownerAction)
		{
			_holdOffset = Lerp.Vec2(_holdOffset, new Vec2(-13f, -4f), 2f);
			angle = Lerp.Float(angle, -1f, 0.1f);
			handFlip = true;
			handOffset = Lerp.Vec2(handOffset, new Vec2(-3f, -4f), 1f);
			_canRaise = false;
		}
		else
		{
			float lerpSpeed = 1f;
			if (base.duck != null)
			{
				angle = Lerp.Float(angle, 0f, 0.1f * lerpSpeed);
			}
			else
			{
				lerpSpeed = 20f;
			}
			_holdOffset = Lerp.Vec2(_holdOffset, new Vec2(-2f, 0f), lerpSpeed * 2f);
			handFlip = false;
			handOffset = Lerp.Vec2(handOffset, new Vec2(0f, 0f), 1f * lerpSpeed);
			_canRaise = true;
		}
		if (owner == null && base.level.simulatePhysics)
		{
			if (_framesSinceThrown == 0)
			{
				_throwSpin = base.angleDegrees;
			}
			_framesSinceThrown++;
			if (_framesSinceThrown > 15)
			{
				_framesSinceThrown = 15;
			}
			base.angleDegrees = _throwSpin;
			bool spinning = false;
			bool againstWall = false;
			if ((Math.Abs(hSpeed) + Math.Abs(vSpeed) > 2f || !base.grounded) && gravMultiplier > 0f && !againstWall && !_grounded)
			{
				if (offDir > 0)
				{
					_throwSpin += (Math.Abs(hSpeed * 2f) + Math.Abs(vSpeed)) * 1f + 5f;
				}
				else
				{
					_throwSpin -= (Math.Abs(hSpeed * 2f) + Math.Abs(vSpeed)) * 1f + 5f;
				}
				spinning = true;
			}
			if (!spinning || againstWall)
			{
				_throwSpin %= 360f;
				if (_throwSpin < 0f)
				{
					_throwSpin += 360f;
				}
				if (againstWall)
				{
					if (Math.Abs(_throwSpin - 90f) < Math.Abs(_throwSpin + 90f))
					{
						_throwSpin = Lerp.Float(_throwSpin, 90f, 16f);
					}
					else
					{
						_throwSpin = Lerp.Float(-90f, 0f, 16f);
					}
				}
				else if (_throwSpin > 90f && _throwSpin < 270f)
				{
					_throwSpin = Lerp.Float(_throwSpin, 180f, 14f);
				}
				else
				{
					if (_throwSpin > 180f)
					{
						_throwSpin -= 360f;
					}
					else if (_throwSpin < -180f)
					{
						_throwSpin += 360f;
					}
					_throwSpin = Lerp.Float(_throwSpin, 0f, 14f);
				}
			}
		}
		else
		{
			_throwSpin = 0f;
		}
		base.Update();
	}
}
