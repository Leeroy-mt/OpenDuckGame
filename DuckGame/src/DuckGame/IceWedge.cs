using System;

namespace DuckGame;

[EditorGroup("Details|Terrain")]
public class IceWedge : MaterialThing
{
	public IceWedge(float xpos, float ypos, int dir)
		: base(xpos, ypos)
	{
		_canFlipVert = true;
		graphic = new SpriteMap("iceWedge", 17, 17);
		base.hugWalls = WallHug.Left | WallHug.Right | WallHug.Floor;
		center = new Vec2(8f, 14f);
		collisionSize = new Vec2(14f, 8f);
		collisionOffset = new Vec2(-7f, -6f);
		base.depth = -0.9f;
	}

	public override void EditorUpdate()
	{
		base.EditorUpdate();
	}

	public override void OnSoftImpact(MaterialThing with, ImpactedFrom from)
	{
		if (flipVertical)
		{
			if (with.vSpeed < -1f && ((offDir > 0 && with.hSpeed < 1f) || (offDir < 0 && with.hSpeed >= -1f)))
			{
				with.hSpeed = (0f - with.vSpeed) * 1.5f * (float)offDir;
			}
			else if (((offDir < 0 && with.right > base.left + 4f) || (offDir > 0 && with.left < base.right - 4f)) && ((offDir > 0 && with.hSpeed < -1f) || (offDir < 0 && with.hSpeed > 1f)) && with.vSpeed < 0.5f)
			{
				with.vSpeed = Math.Abs(with.hSpeed * 1.6f);
			}
		}
		else if (with.vSpeed > 1f && ((offDir > 0 && with.hSpeed < 1f) || (offDir < 0 && with.hSpeed >= -1f)))
		{
			with.hSpeed = with.vSpeed * 1.5f * (float)offDir;
		}
		else if (((offDir < 0 && with.right > base.left + 4f) || (offDir > 0 && with.left < base.right - 4f)) && ((offDir > 0 && with.hSpeed < -1f) || (offDir < 0 && with.hSpeed > 1f)) && with.vSpeed > -0.5f)
		{
			with.vSpeed = 0f - Math.Abs(with.hSpeed * 1.6f);
		}
		base.OnSoftImpact(with, from);
	}

	public override void Draw()
	{
		base.hugWalls = WallHug.None;
		if (flipVertical)
		{
			base.hugWalls |= WallHug.Ceiling;
		}
		else
		{
			base.hugWalls |= WallHug.Floor;
		}
		if (flipHorizontal)
		{
			base.hugWalls |= WallHug.Right;
		}
		else
		{
			base.hugWalls |= WallHug.Left;
		}
		base.angleDegrees = 0f;
		if (flipVertical)
		{
			if (flipHorizontal)
			{
				base.angleDegrees = 180f;
				center = new Vec2(8f, 14f);
				collisionSize = new Vec2(14f, 8f);
				collisionOffset = new Vec2(-7f, -2f);
			}
			else
			{
				base.angleDegrees = 90f;
				center = new Vec2(3f, 9f);
				collisionSize = new Vec2(14f, 8f);
				collisionOffset = new Vec2(-7f, -2f);
			}
		}
		else if (flipHorizontal)
		{
			base.angleDegrees = 270f;
			center = new Vec2(3f, 9f);
			collisionSize = new Vec2(14f, 8f);
			collisionOffset = new Vec2(-7f, -6f);
		}
		else
		{
			base.angleDegrees = 0f;
			center = new Vec2(8f, 14f);
			collisionSize = new Vec2(14f, 8f);
			collisionOffset = new Vec2(-7f, -6f);
		}
		base.Draw();
	}
}
