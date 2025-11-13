using System;
using System.Collections.Generic;

namespace DuckGame;

[EditorGroup("Stuff|Doors", EditorItemType.Debug)]
public class WallDoor : Thing
{
	protected SpriteMap _sprite;

	private List<Duck> _transportingDucks = new List<Duck>();

	public WallDoor(float xpos, float ypos)
		: base(xpos, ypos)
	{
		_sprite = new SpriteMap("wallDoor", 21, 30);
		_sprite.AddAnimation("opening", 1f, false, 1, 2, 3, 4, 5, 6, 6, 6, 6, 6);
		_sprite.AddAnimation("closing", 1f, false, 5, 4, 3, 2, 1);
		_sprite.AddAnimation("open", 1f, false, 6);
		_sprite.AddAnimation("closed", 1f, false, default(int));
		_sprite.SetAnimation("closed");
		graphic = _sprite;
		center = new Vec2(10f, 22f);
		collisionSize = new Vec2(21f, 30f);
		collisionOffset = new Vec2(-10f, -20f);
		base.depth = -0.5f;
		_editorName = "Wall Door";
		_canFlip = false;
	}

	public void AddDuck(Duck d)
	{
		d.autoExitDoor = true;
		d.autoExitDoorFrames = 5;
		_transportingDucks.Add(d);
		_sprite.SetAnimation("open");
		if (d.spriteImageIndex < 4)
		{
			SFX.Play("doorOpen", Rando.Float(0.8f, 0.9f), Rando.Float(-0.1f, 0.1f));
		}
	}

	public override void Update()
	{
		foreach (Duck d in Level.CheckRectAll<Duck>(base.topLeft, base.bottomRight))
		{
			if (d.grounded && d.inputProfile.Pressed("UP") && !d.enteringWalldoor && !d.exitingWalldoor && !_transportingDucks.Contains(d))
			{
				_transportingDucks.Add(d);
				d.wallDoorAI = new DuckAI(d.inputProfile);
				d.autoExitDoorFrames = 0;
				d.enterDoorSpeed = d.hSpeed;
				if (d.spriteImageIndex < 4)
				{
					SFX.Play("doorOpen", Rando.Float(0.8f, 0.9f), Rando.Float(-0.1f, 0.1f));
				}
				_sprite.SetAnimation("opening");
			}
		}
		if (_sprite.currentAnimation == "opening" && _sprite.finished)
		{
			_sprite.SetAnimation("open");
		}
		if (_sprite.currentAnimation == "closing" && _sprite.finished)
		{
			_sprite.SetAnimation("closed");
			SFX.Play("doorClose", Rando.Float(0.5f, 0.6f), Rando.Float(-0.1f, 0.1f));
		}
		if (_transportingDucks.Count == 0 && _sprite.currentAnimation != "closing" && _sprite.currentAnimation != "closed")
		{
			_sprite.SetAnimation("closing");
		}
		for (int i = 0; i < _transportingDucks.Count; i++)
		{
			Duck d2 = _transportingDucks[i];
			if (d2.wallDoorAI == null && !d2.autoExitDoor && !d2.exitingWalldoor)
			{
				WallDoor transportDoor = null;
				if (d2.inputProfile.Pressed("LEFT") || (d2.inputProfile.Down("LEFT") && d2.autoExitDoorFrames > 5))
				{
					transportDoor = Level.CheckRay<WallDoor>(position, position + new Vec2(-10000f, 0f), this, out var _);
				}
				if (d2.inputProfile.Pressed("RIGHT") || (d2.inputProfile.Down("RIGHT") && d2.autoExitDoorFrames > 5))
				{
					transportDoor = Level.CheckRay<WallDoor>(position, position + new Vec2(10000f, 0f), this, out var _);
				}
				if (d2.inputProfile.Pressed("UP") || (d2.inputProfile.Down("UP") && d2.autoExitDoorFrames > 10))
				{
					transportDoor = Level.CheckRay<WallDoor>(position, position + new Vec2(0f, -10000f), this, out var _);
				}
				if (d2.inputProfile.Pressed("DOWN") || (d2.inputProfile.Down("DOWN") && d2.autoExitDoorFrames > 5))
				{
					transportDoor = Level.CheckRay<WallDoor>(position, position + new Vec2(0f, 10000f), this, out var _);
				}
				if (transportDoor != null)
				{
					d2.enteringWalldoor = true;
					d2.transportDoor = transportDoor;
					d2.autoExitDoorFrames = 0;
				}
			}
			if (Math.Abs(d2.x - base.x) < 3f && d2.wallDoorAI != null)
			{
				d2.hSpeed *= 0.5f;
				d2.moveLock = true;
				d2.wallDoorAI = null;
			}
			else if (d2.wallDoorAI != null)
			{
				if (d2.x > base.x + 2f)
				{
					d2.wallDoorAI.Press("LEFT");
				}
				if (d2.x < base.x - 2f)
				{
					d2.wallDoorAI.Press("RIGHT");
				}
			}
			if (d2.transportDoor != null)
			{
				d2.autoExitDoorFrames--;
			}
			else if (d2.autoExitDoor && d2.autoExitDoorFrames > 0)
			{
				d2.autoExitDoorFrames--;
			}
			else if (!d2.autoExitDoor)
			{
				d2.autoExitDoorFrames++;
			}
			if ((d2.inputProfile.Pressed("JUMP") && !d2.autoExitDoor) || (d2.autoExitDoor && d2.autoExitDoorFrames == 0))
			{
				d2.exitingWalldoor = true;
				d2.autoExitDoor = false;
			}
			if (d2.transportDoor != null && d2.autoExitDoorFrames <= 0)
			{
				d2.position = d2.transportDoor.position + new Vec2(0f, -6f);
				d2.transportDoor.AddDuck(d2);
				d2.transportDoor = null;
				_transportingDucks.RemoveAt(i);
				i--;
			}
			else if (d2.exitingWalldoor)
			{
				d2.moveLock = false;
				d2.enteringWalldoor = false;
				d2.exitingWalldoor = false;
				d2.wallDoorAI = null;
				_transportingDucks.RemoveAt(i);
				d2.autoExitDoor = false;
				d2.transportDoor = null;
				d2.hSpeed = d2.enterDoorSpeed;
				i--;
			}
		}
		base.Update();
	}

	public override void Draw()
	{
		Graphics.DrawRect(base.topLeft, base.bottomRight, new Color(18, 25, 33), -0.6f);
		base.Draw();
	}
}
