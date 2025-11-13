using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;

namespace DuckGame;

public class Furniture
{
	public static FurnitureGroup Cheap = new FurnitureGroup
	{
		name = "cheap",
		color = new Color(222, 181, 144),
		priceMultiplier = 0.4f,
		additionalRarity = 5
	};

	public static FurnitureGroup Fancy = new FurnitureGroup
	{
		name = "fancy",
		color = new Color(245, 199, 91),
		font = new BitmapFont("furni/ornateFont", 8),
		priceMultiplier = 5f,
		additionalRarity = 300
	};

	public static FurnitureGroup Everyday = new FurnitureGroup
	{
		name = "everyday",
		color = new Color(141, 182, 201),
		additionalRarity = 5
	};

	public static FurnitureGroup Flowers = new FurnitureGroup
	{
		name = "flowers",
		color = new Color(237, 165, 206),
		font = new BitmapFont("furni/italFont", 8),
		additionalRarity = 80
	};

	public static FurnitureGroup Bathroom = new FurnitureGroup
	{
		name = "bathroom",
		color = new Color(211, 227, 163),
		additionalRarity = 100
	};

	public static FurnitureGroup Outdoor = new FurnitureGroup
	{
		name = "outdoor",
		color = new Color(192, 163, 227),
		additionalRarity = 20
	};

	public static FurnitureGroup Stone = new FurnitureGroup
	{
		name = "stone",
		color = new Color(128, 159, 178),
		priceMultiplier = 3f,
		additionalRarity = 160
	};

	public static FurnitureGroup Instruments = new FurnitureGroup
	{
		name = "music",
		color = new Color(279, 170, 150),
		priceMultiplier = 3.5f,
		additionalRarity = 120
	};

	public static FurnitureGroup Momento = new FurnitureGroup
	{
		name = "momento",
		color = new Color(241, 71, 85)
	};

	public static FurnitureGroup Bar = new FurnitureGroup
	{
		name = "bar",
		color = new Color(180, 211, 89),
		additionalRarity = 50
	};

	public static FurnitureGroup Ship = new FurnitureGroup
	{
		name = "ship",
		color = new Color(157, 157, 157),
		additionalRarity = 35
	};

	public static FurnitureGroup Freezer = new FurnitureGroup
	{
		name = "freezer",
		color = new Color(255, 255, 255),
		additionalRarity = 180
	};

	public static FurnitureGroup Office = new FurnitureGroup
	{
		name = "office",
		color = new Color(47, 72, 78),
		additionalRarity = 70
	};

	public static FurnitureGroup Characters = new FurnitureGroup
	{
		name = "characters",
		color = new Color(160, 21, 35),
		priceMultiplier = 2f,
		additionalRarity = 8
	};

	public static FurnitureGroup Default = new FurnitureGroup
	{
		name = "default",
		color = Colors.DGPurple
	};

	private List<VariatingSprite> _eggSprites = new List<VariatingSprite>();

	public float ballRot;

	public bool rareGen;

	private SpriteMap _photoSprite;

	public float yOffset;

	public bool isSurface;

	public bool stickToFloor;

	public bool stickToRoof;

	public SpriteMap sprite;

	public string name;

	public FurnitureType type;

	public SpriteMap icon;

	public SpriteMap background;

	public BitmapFont font;

	public FurnitureGroup group;

	public short index;

	public float topOffset;

	public int deep;

	private int _rarity;

	public int max;

	public string description;

	public bool canFlip;

	public bool neverFlip;

	public bool visible = true;

	public bool alwaysHave;

	public bool isFlag;

	public bool canGetInGacha = true;

	public int price
	{
		get
		{
			if (type == FurnitureType.Prop)
			{
				return (int)Math.Ceiling((float)(sprite.width * sprite.height) / 12f * group.priceMultiplier * (1f + (float)rarity / 100f));
			}
			if (type == FurnitureType.Theme)
			{
				return (int)Math.Ceiling(100f * group.priceMultiplier * (1f + (float)rarity / 100f));
			}
			if (type == FurnitureType.Font)
			{
				return (int)Math.Ceiling(60f * group.priceMultiplier * (1f + (float)rarity / 100f));
			}
			return 9999;
		}
	}

	public int rarity => _rarity + group.additionalRarity;

	public VariatingSprite GetSprite(ulong id, int variation, VSType t)
	{
		VariatingSprite spr = _eggSprites.FirstOrDefault((VariatingSprite x) => x.id == id && x.variation == variation && x.type == t);
		if (spr == null)
		{
			spr = new VariatingSprite();
			spr.variation = variation;
			spr.id = id;
			spr.type = t;
			if (t == VSType.Egg)
			{
				spr.sprite = Profile.GetEggSprite(variation, id);
			}
			else
			{
				spr.sprite = Profile.GetPaintingSprite(variation, id);
			}
			_eggSprites.Add(spr);
			return null;
		}
		return spr;
	}

	public void Draw(Vec2 pos, Depth depth, int variation = 0, Profile profile = null, bool affectScale = false, bool halfscale = false, float angle = 0f)
	{
		ulong seed = 0uL;
		if (profile == null)
		{
			if (Profiles.experienceProfile != null)
			{
				profile = Profiles.experienceProfile;
				seed = Profiles.experienceProfile.steamID;
			}
		}
		else
		{
			seed = profile.steamID;
		}
		SpriteMap spr = sprite;
		if (icon != null)
		{
			spr = icon;
		}
		if (spr != null && neverFlip)
		{
			spr.flipH = false;
		}
		if (isFlag && profile != null)
		{
			int flagIndex = (Network.isActive ? profile.flagIndex : Global.data.flag);
			if (flagIndex < 0)
			{
				flagIndex = profile.flagIndex;
			}
			if (flagIndex >= 0)
			{
				Sprite s = UIFlagSelection.GetFlag(flagIndex, smallVersion: true);
				if (s != null)
				{
					float scale = 0.39f * spr.scale.x;
					for (int i = 0; i < 30; i++)
					{
						float sinOffset = (float)Math.Sin((float)Graphics.frame / 10f + (float)i * 0.18f);
						Vec2 flagStart = pos + new Vec2((spr.flipH ? (-2f) : 2f) * spr.scale.x, -9f * spr.scale.y);
						Graphics.Draw(s.texture, flagStart + new Vec2((float)(i * 2) * scale * (spr.flipH ? (-1f) : 1f), sinOffset * 1.4f * ((float)i / 51f)), new Rectangle(i * 2, 0f, 3f, 41f), Color.White, 0f, Vec2.Zero, spr.flipH ? new Vec2(0f - scale, scale) : new Vec2(scale), SpriteEffects.None, depth - 2);
					}
				}
			}
		}
		if (name == "EGG")
		{
			VariatingSprite vSpr = GetSprite(seed, variation, VSType.Egg);
			if (vSpr != null && vSpr.sprite.texture != null && vSpr.sprite.texture != null)
			{
				vSpr.sprite.depth = depth + 6;
				vSpr.sprite.scale = sprite.scale;
				Graphics.Draw(vSpr.sprite, pos.x - 8f * vSpr.sprite.xscale, pos.y - 12f * vSpr.sprite.yscale);
				spr.frame = 0;
			}
		}
		else if (name == "PHOTO")
		{
			if (_photoSprite == null)
			{
				_photoSprite = new SpriteMap("littleMan", 16, 16);
			}
			_photoSprite.frame = UILevelBox.LittleManFrame(variation, 7, seed);
			_photoSprite.depth = depth + 6;
			_photoSprite.scale = sprite.scale;
			Graphics.Draw(_photoSprite, pos.x - 6f * _photoSprite.xscale, pos.y - 4f * _photoSprite.yscale, new Rectangle(2f, 0f, 12f, 10f));
			Graphics.DrawRect(pos + new Vec2(-6f * _photoSprite.xscale, -6f * _photoSprite.yscale), pos + new Vec2(6f * _photoSprite.xscale, 6f * _photoSprite.yscale), Colors.DGBlue, depth - 4);
			spr.frame = 0;
		}
		else if (name == "EASEL")
		{
			VariatingSprite vSpr2 = GetSprite(seed, variation, VSType.Portrait);
			if (vSpr2 != null && vSpr2.sprite.texture != null)
			{
				vSpr2.sprite.depth = depth + 6;
				vSpr2.sprite.scale = sprite.scale;
				Graphics.Draw(vSpr2.sprite, pos.x - 9f * vSpr2.sprite.xscale, pos.y - 8f * vSpr2.sprite.yscale);
				spr.frame = 0;
			}
			else
			{
				DevConsole.Log(DCSection.General, "null easel");
			}
		}
		else
		{
			spr.frame = variation;
		}
		if (font != null && sprite == null)
		{
			font.scale = new Vec2(1f, 1f);
			font.Draw("F", pos + new Vec2(-3.5f, -3f), Color.Black, depth + 8);
		}
		if (affectScale)
		{
			if (halfscale && (spr.width > 30 || spr.height > 30))
			{
				spr.scale = new Vec2(0.5f);
			}
			else
			{
				spr.scale = new Vec2(1f);
			}
		}
		spr.depth = depth;
		spr.angle = angle;
		Graphics.Draw(spr, pos.x, pos.y - yOffset);
		spr.scale = new Vec2(1f);
	}

    public Furniture(
          bool canflip,
          bool neverflip,
          string desc,
          int rarityval,
          string spr,
          int wide,
          int high,
          string nam,
          FurnitureGroup gr,
          SpriteMap ico = null,
          FurnitureType t = FurnitureType.Prop,
          BitmapFont f = null,
          string bak = null,
          bool stickToroof = false,
          bool stickTofloor = false,
          int deepval = 0,
          int maxval = -1,
          bool canGacha = true)
    {
        neverFlip = neverflip;
        canGetInGacha = canGacha;
        canFlip = canflip;
        stickToFloor = stickTofloor;
        stickToRoof = stickToroof;
        max = maxval;
        _rarity = rarityval;
        description = desc;
        if (spr != null) sprite = new SpriteMap("furni/" + gr.name + "/" + spr, wide, high);
        if (bak != null) background = new SpriteMap("furni/" + gr.name + "/" + bak, wide, high);
        name = nam;
        icon = ico;
        type = t;
        font = f;
        group = gr;
        deep = deepval;
        if (stickToroof) deep += 20;
        if (!stickToroof && !stickTofloor) deep--;
        if (sprite != null)
        {
            sprite.CenterOrigin();
            if (sprite.height / 2f - Math.Floor(sprite.height / 2f) == 0f) sprite.centery--;
            else sprite.centery = (float)Math.Floor(sprite.height / 2f);
        }
        if (icon != null)
            icon.CenterOrigin();
        if (background == null)
            return;
        background.CenterOrigin();
    }

    public Furniture(
          bool canflip,
          bool neverflip,
          string desc,
          int rarityval,
          string spr,
          int wide,
          int high,
          string nam,
          FurnitureGroup gr,
          SpriteMap ico,
          bool stickTofloor,
          bool stickToroof = false,
          bool surface = false,
          float topoff = 0f,
          int maxval = -1,
          bool canGacha = true)
    {
        neverFlip = neverflip;
        canGetInGacha = canGacha;
        canFlip = canflip;
        stickToFloor = stickTofloor;
        stickToRoof = stickToroof;
        isSurface = surface;
        topOffset = topoff;
        description = desc;
        max = maxval;
        _rarity = rarityval;
        if (spr != null) sprite = new SpriteMap("furni/" + gr.name + "/" + spr, wide, high);
        if (stickToroof) deep += 20;
        if (!stickToroof && !stickTofloor) deep--;
        name = nam;
        icon = ico;
        type = FurnitureType.Prop;
        group = gr;
        if (sprite != null)
        {
            sprite.CenterOrigin();
            if (sprite.height / 2f - Math.Floor(sprite.height / 2f) == 0f) --sprite.centery;
            else sprite.centery = (float)Math.Floor(sprite.height / 2f);
        }
        if (icon != null) icon.CenterOrigin();
        if (background == null) return;
        background.CenterOrigin();
    }
}
