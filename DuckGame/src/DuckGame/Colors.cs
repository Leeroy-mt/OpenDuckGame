using System;
using System.Collections.Generic;

namespace DuckGame;

public class Colors
{
	public static Color MenuOption = new Color(235, 137, 51);

	public static Color Bronze = new Color(216, 105, 65);

	public static Color Silver = new Color(191, 191, 191);

	public static Color Gold = new Color(247, 224, 90);

	public static Color Platinum = new Color(178, 220, 239);

	public static Color Developer = new Color(222, 32, 45);

	public static Color DGYellow = new Color(247, 224, 90);

	public static Color DGOrange = new Color(235, 136, 49);

	public static Color DGPink = new Color(235, 190, 242);

	public static Color DGEgg = new Color(245, 235, 211);

	public static Color DGBlue = new Color(49, 162, 242);

	public static Color DGGreen = new Color(163, 206, 39);

	public static Color DGRed = new Color(192, 32, 45);

	public static Color BlueGray = new Color(47, 73, 79);

	public static Color DGPurple = new Color(179, 54, 242);

	public static Color Transparent = new Color(0, 0, 0, 0);

	public static Color TransparentPink = new Color(255, 0, 255, 0);

	public static Color DGVanilla = new Color(255, 246, 214);

	public static Color Duck1 = new Color(255, 255, 255);

	public static Color Duck2 = new Color(125, 125, 125);

	public static Color Duck3 = new Color(247, 224, 90);

	public static Color Duck4 = new Color(205, 107, 29);

	public static Color SuperDarkBlueGray = new Color(8, 12, 16);

	private static Dictionary<string, Color> _colorMap = new Dictionary<string, Color>
	{
		{
			"AQUA",
			new Color(0, 255, 255, 255)
		},
		{
			"RED",
			Color.Red
		},
		{
			"WHITE",
			Color.White
		},
		{
			"BLACK",
			Color.Black
		},
		{
			"DARKNESS",
			new Color(10, 10, 10)
		},
		{
			"BLUE",
			Color.Blue
		},
		{ "DGBLUE", DGBlue },
		{ "DGRED", DGRed },
		{ "DGREDDD", DGRed },
		{ "DGGREEN", DGGreen },
		{ "DGGREENN", DGGreen },
		{ "DGYELLOW", DGYellow },
		{ "DGYELLO", DGYellow },
		{
			"DGORANGE",
			new Color(235, 136, 49)
		},
		{
			"ORANGE",
			new Color(235, 137, 51)
		},
		{ "MENUORANGE", MenuOption },
		{
			"YELLOW",
			new Color(247, 224, 90)
		},
		{
			"GREEN",
			Color.LimeGreen
		},
		{
			"LIME",
			Color.LimeGreen
		},
		{
			"TIMELIME",
			Color.Lime
		},
		{
			"GRAY",
			new Color(70, 70, 70)
		},
		{
			"LIGHTGRAY",
			new Color(96, 119, 124)
		},
		{
			"CREDITSGRAY",
			new Color(137, 159, 164)
		},
		{ "BLUEGRAY", BlueGray },
		{
			"PINK",
			new Color(246, 88, 191)
		},
		{
			"PURPLE",
			new Color(115, 48, 242)
		},
		{
			"DGPURPLE",
			new Color(115, 48, 242)
		},
		{ "CBRONZE", Bronze },
		{ "CSILVER", Silver },
		{ "CGOLD", Gold },
		{ "CPLATINUM", Platinum },
		{ "CDEV", Developer },
		{ "DUCKCOLOR1", Duck1 },
		{ "DUCKCOLOR2", Duck2 },
		{ "DUCKCOLOR3", Duck3 },
		{ "DUCKCOLOR4", Duck4 },
		{
			"RBOW_1",
			new Color(192, 35, 45)
		},
		{
			"RBOW_2",
			new Color(237, 94, 238)
		},
		{
			"RBOW_3",
			new Color(138, 38, 190)
		},
		{
			"RBOW_4",
			new Color(49, 162, 242)
		},
		{
			"RBOW_5",
			new Color(149, 188, 37)
		},
		{
			"RBOW_6",
			new Color(247, 224, 90)
		},
		{
			"RBOW_7",
			new Color(235, 137, 49)
		}
	};

	public static Color ParseColor(string color)
	{
		Color c = Color.Transparent;
		if (!_colorMap.TryGetValue(color, out c))
		{
			try
			{
				string[] parts = color.Split(',');
				if (parts.Length == 3)
				{
					byte r = Convert.ToByte(parts[0]);
					byte cg = Convert.ToByte(parts[1]);
					byte cb = Convert.ToByte(parts[2]);
					return new Color(r, cg, cb);
				}
			}
			catch (Exception)
			{
			}
		}
		return c;
	}
}
