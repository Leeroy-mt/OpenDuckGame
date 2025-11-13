namespace DuckGame;

public class Nubber : MaterialThing, IPlatform, IDontMove, IDontUpdate
{
	private SpriteMap _sprite;

	public string tileset;

	public bool cheap;

	public void UpdateCustomTileset()
	{
		int fr = 0;
		if (_sprite != null)
		{
			fr = _sprite.frame;
		}
		if (tileset == "CUSTOM01")
		{
			CustomTileData tileData = Custom.GetData(0, CustomType.Block);
			if (tileData != null && tileData.texture != null)
			{
				_sprite = new SpriteMap(tileData.texture, 16, 16);
			}
			else
			{
				_sprite = new SpriteMap("blueprintTileset", 16, 16);
			}
		}
		else if (tileset == "CUSTOM02")
		{
			CustomTileData tileData2 = Custom.GetData(1, CustomType.Block);
			if (tileData2 != null && tileData2.texture != null)
			{
				_sprite = new SpriteMap(tileData2.texture, 16, 16);
			}
			else
			{
				_sprite = new SpriteMap("blueprintTileset", 16, 16);
			}
		}
		else if (tileset == "CUSTOM03")
		{
			CustomTileData tileData3 = Custom.GetData(2, CustomType.Block);
			if (tileData3 != null && tileData3.texture != null)
			{
				_sprite = new SpriteMap(tileData3.texture, 16, 16);
			}
			else
			{
				_sprite = new SpriteMap("blueprintTileset", 16, 16);
			}
		}
		else if (tileset == "CUSTOMPLAT01")
		{
			CustomTileData tileData4 = Custom.GetData(0, CustomType.Platform);
			if (tileData4 != null && tileData4.texture != null)
			{
				_sprite = new SpriteMap(tileData4.texture, 16, 16);
			}
			else
			{
				_sprite = new SpriteMap("scaffolding", 16, 16);
			}
		}
		else if (tileset == "CUSTOMPLAT02")
		{
			CustomTileData tileData5 = Custom.GetData(1, CustomType.Platform);
			if (tileData5 != null && tileData5.texture != null)
			{
				_sprite = new SpriteMap(tileData5.texture, 16, 16);
			}
			else
			{
				_sprite = new SpriteMap("scaffolding", 16, 16);
			}
		}
		else if (tileset == "CUSTOMPLAT03")
		{
			CustomTileData tileData6 = Custom.GetData(2, CustomType.Platform);
			if (tileData6 != null && tileData6.texture != null)
			{
				_sprite = new SpriteMap(tileData6.texture, 16, 16);
			}
			else
			{
				_sprite = new SpriteMap("scaffolding", 16, 16);
			}
		}
		if (_sprite == null)
		{
			_sprite = new SpriteMap(tileset, 16, 16);
		}
		graphic = _sprite;
		_sprite.frame = fr;
	}

	public Nubber(float x, float y, bool left, string tset)
		: base(x, y)
	{
		tileset = tset;
		UpdateCustomTileset();
		graphic = _sprite;
		collisionSize = new Vec2(8f, 5f);
		_sprite.frame = (left ? 62 : 63);
		if (left)
		{
			collisionOffset = new Vec2(13f, 0f);
		}
		else
		{
			collisionOffset = new Vec2(-5f, 0f);
		}
		_editorCanModify = false;
		UpdateCustomTileset();
	}

	public override void Terminate()
	{
	}

	public virtual void DoPositioning()
	{
		if (!(Level.current is Editor) && graphic != null)
		{
			graphic.position = position;
			graphic.scale = base.scale;
			graphic.center = center;
			graphic.depth = base.depth;
			graphic.alpha = base.alpha;
			graphic.angle = angle;
			(graphic as SpriteMap).ClearCache();
			(graphic as SpriteMap).UpdateFrame();
		}
	}

	public override void Draw()
	{
		if (cheap)
		{
			graphic.UltraCheapStaticDraw(flipHorizontal);
		}
		else
		{
			base.Draw();
		}
	}
}
