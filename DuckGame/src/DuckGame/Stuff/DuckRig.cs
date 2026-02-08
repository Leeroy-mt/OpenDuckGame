using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;

namespace DuckGame;

public class DuckRig
{
    private static List<Vector2> _hatPoints = new List<Vector2>();

    private static List<Vector2> _chestPoints = new List<Vector2>();

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
                Vector2 hatPoint = new Vector2
                {
                    X = r.ReadInt32(),
                    Y = r.ReadInt32()
                };
                _hatPoints.Add(hatPoint);
                Vector2 chestPoint = new Vector2
                {
                    X = r.ReadInt32(),
                    Y = r.ReadInt32()
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

    public static Vector2 GetHatPoint(int frame)
    {
        return _hatPoints[frame];
    }

    public static Vector2 GetChestPoint(int frame)
    {
        return _chestPoints[frame];
    }
}
