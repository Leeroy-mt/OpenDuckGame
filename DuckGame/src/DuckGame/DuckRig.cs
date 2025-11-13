using System;
using System.Collections.Generic;
using System.IO;

namespace DuckGame;

public class DuckRig
{
	private static List<Vec2> _hatPoints = new List<Vec2>();

	private static List<Vec2> _chestPoints = new List<Vec2>();

	public static void Initialize()
	{
		try
		{
			_hatPoints.Clear();
			_chestPoints.Clear();
			BinaryReader r = new BinaryReader(File.OpenRead(Content.path + "rig_duckRig.rig"));
			int num = r.ReadInt32();
			for (int i = 0; i < num; i++)
			{
				Vec2 hatPoint = new Vec2
				{
					x = r.ReadInt32(),
					y = r.ReadInt32()
				};
				_hatPoints.Add(hatPoint);
				Vec2 chestPoint = new Vec2
				{
					x = r.ReadInt32(),
					y = r.ReadInt32()
				};
				_chestPoints.Add(chestPoint);
			}
			r.Close();
			r.Dispose();
		}
		catch (Exception e)
		{
			Program.LogLine(MonoMain.GetExceptionString(e));
		}
	}

	public static Vec2 GetHatPoint(int frame)
	{
		return _hatPoints[frame];
	}

	public static Vec2 GetChestPoint(int frame)
	{
		return _chestPoints[frame];
	}
}
