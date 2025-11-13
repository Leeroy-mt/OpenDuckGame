using System.Collections.Generic;
using System.Linq;

namespace DuckGame;

[EditorGroup("Details|Terrain")]
public class WaterFlow : Thing
{
	protected HashSet<WaterFlow> _extraWater = new HashSet<WaterFlow>();

	public static int waterFrame;

	public static int waterFrameInc;

	public static bool updatedWaterFrame;

	private new bool _initialized;

	private bool _wallLeft;

	private bool _wallRight;

	public bool processed;

	private HashSet<PhysicsObject> _held = new HashSet<PhysicsObject>();

	public WaterFlow(float xpos, float ypos)
		: base(xpos, ypos)
	{
		SpriteMap flow = new SpriteMap("waterFlow", 16, 16);
		graphic = flow;
		center = new Vec2(8f, 14f);
		_collisionSize = new Vec2(16f, 4f);
		_collisionOffset = new Vec2(-8f, -2f);
		base.layer = Layer.Blocks;
		base.depth = 0.3f;
		base.alpha = 0.8f;
		base.hugWalls = WallHug.Floor;
	}

	public Rectangle ProcessGroupRect(Rectangle rect)
	{
		if (!processed)
		{
			if (base.left < rect.x)
			{
				rect.width += (int)(rect.x - base.left);
				rect.x = (int)base.left;
			}
			if (base.right > rect.x + rect.width)
			{
				rect.width += (int)(base.right - (rect.x + rect.width));
			}
			processed = true;
			if (!_wallLeft)
			{
				WaterFlow lef = Level.CheckPoint<WaterFlow>(new Vec2(base.x - 16f, base.y));
				if (lef != null && lef != this && lef.flipHorizontal == flipHorizontal)
				{
					rect = lef.ProcessGroupRect(rect);
					_extraWater.Add(lef);
					foreach (WaterFlow f in lef._extraWater)
					{
						_extraWater.Add(f);
					}
				}
			}
			if (!_wallRight)
			{
				WaterFlow rig = Level.CheckPoint<WaterFlow>(new Vec2(base.x + 16f, base.y));
				if (rig != null && rig != this && rig.flipHorizontal == flipHorizontal)
				{
					rect = rig.ProcessGroupRect(rect);
					_extraWater.Add(rig);
					foreach (WaterFlow f2 in rig._extraWater)
					{
						_extraWater.Add(f2);
					}
				}
			}
			return rect;
		}
		return rect;
	}

	public override void Update()
	{
		if (!_initialized)
		{
			_initialized = true;
			if (Level.CheckPoint<Block>(new Vec2(base.x - 16f, base.y)) != null)
			{
				_wallLeft = true;
			}
			else if (Level.CheckPoint<Block>(new Vec2(base.x + 16f, base.y)) != null)
			{
				_wallRight = true;
			}
			if (!processed)
			{
				Rectangle group = ProcessGroupRect(base.rectangle);
				if (_extraWater.Count > 0)
				{
					_extraWater.Remove(this);
					foreach (WaterFlow item in _extraWater)
					{
						Level.Remove(item);
						item._extraWater.Clear();
					}
					_collisionSize = new Vec2(group.width, group.height);
					_collisionOffset = new Vec2(group.x - base.x, _collisionOffset.y);
				}
			}
		}
		bool flp = flipHorizontal;
		flipHorizontal = false;
		IEnumerable<PhysicsObject> things = Level.CheckRectAll<PhysicsObject>(base.topLeft, base.bottomRight);
		foreach (PhysicsObject t in things)
		{
			if (flp && t.hSpeed > -2f)
			{
				t.hSpeed -= 0.3f;
			}
			else if (!flp && t.hSpeed < 2f)
			{
				t.hSpeed += 0.3f;
			}
			t.sleeping = false;
			t.frictionMult = 0.3f;
			_held.Add(t);
		}
		List<PhysicsObject> toRemove = new List<PhysicsObject>();
		foreach (PhysicsObject t2 in _held)
		{
			if (!things.Contains(t2))
			{
				toRemove.Add(t2);
				t2.frictionMult = 1f;
			}
		}
		foreach (PhysicsObject thing in toRemove)
		{
			_held.Remove(thing);
		}
		flipHorizontal = flp;
		base.Update();
	}

	public override void Draw()
	{
		(graphic as SpriteMap).frame = (int)((float)Graphics.frame / 3f % 4f);
		foreach (WaterFlow item in _extraWater)
		{
			(item.graphic as SpriteMap).frame = (int)((float)Graphics.frame / 3f % 4f);
		}
		graphic.flipH = offDir <= 0;
		base.Draw();
		if (!flipHorizontal)
		{
			if (_wallLeft)
			{
				Graphics.Draw(graphic, base.x - 4f, base.y, new Rectangle(graphic.w - 4, 0f, 4f, graphic.h));
			}
			if (_wallRight)
			{
				Graphics.Draw(graphic, base.x + 16f, base.y, new Rectangle(0f, 0f, 4f, graphic.h));
			}
		}
		else
		{
			if (_wallRight)
			{
				Graphics.Draw(graphic, base.x + 4f, base.y, new Rectangle(graphic.w - 4, 0f, 4f, graphic.h));
			}
			if (_wallLeft)
			{
				Graphics.Draw(graphic, base.x - 16f, base.y, new Rectangle(0f, 0f, 4f, graphic.h));
			}
		}
		foreach (WaterFlow water in _extraWater)
		{
			if (water != this && water._extraWater.Count == 0)
			{
				water.Draw();
			}
		}
	}
}
