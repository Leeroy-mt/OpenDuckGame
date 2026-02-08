using Microsoft.Xna.Framework;
using System;

namespace DuckGame;

public class Touch
{
    public static readonly Touch None = new Touch();

    public InputState state;

    public ulong touchFrame;

    public TSData data;

    public bool tap;

    public bool canBeDrag = true;

    public Vector2 originalPosition;

    public bool drag
    {
        get
        {
            if (!canBeDrag)
            {
                return false;
            }
            if (data != null)
            {
                return (data.touchXY - originalPosition).Length() > 25f;
            }
            return false;
        }
    }

    public Vector2 positionCamera
    {
        get
        {
            if (data == null)
            {
                return Vector2.Zero;
            }
            return Transform(Level.current.camera);
        }
    }

    public Vector2 positionHUD
    {
        get
        {
            if (data == null)
            {
                return Vector2.Zero;
            }
            return Transform(Layer.HUD.camera);
        }
    }

    public void SetData(TSData pData)
    {
        if (data == null && pData != null)
        {
            originalPosition = pData.touchXY;
        }
        data = pData;
    }

    public Vector2 Transform(Camera pCamera)
    {
        if (data != null)
        {
            return pCamera.transformScreenVector(data.touchXY);
        }
        return Vector2.Zero;
    }

    public Vector2 TransformGrid(Camera pCamera, float pCellSize)
    {
        Vector2 viewCoords = new Vector2(-1f, -1f);
        viewCoords = Transform(pCamera);
        if (viewCoords != new Vector2(-1f, -1f))
        {
            viewCoords.X = (float)Math.Round(viewCoords.X / pCellSize) * pCellSize;
            viewCoords.Y = (float)Math.Round(viewCoords.Y / pCellSize) * pCellSize;
        }
        return viewCoords;
    }

    /// <summary>
    /// Does a collision check between the current touch and a rectangle. pCamera tells 
    /// the function which camera space to transform the touch into
    /// </summary>
    /// <param name="pRect">Rectangle to collide with</param>
    /// <param name="pCamera">Camera coordinate space to transform touch into</param>
    /// <returns></returns>
    public bool Check(Rectangle pRect, Camera pCamera = null)
    {
        if (data == null)
        {
            return false;
        }
        if (pCamera == null)
        {
            pCamera = Level.current.camera;
        }
        return Collision.Point(Transform(pCamera), pRect);
    }

    /// <summary>
    /// Does a collision check between the current touch and a rectangle. pCamera tells 
    /// the function which camera space to transform the touch into
    /// </summary>
    /// <param name="pRect">Rectangle to collide with</param>
    /// <param name="pCellSize">Grid snap to apply to touch point</param>
    /// <param name="pCamera">Camera coordinate space to transform touch into</param>
    /// <returns></returns>
    public bool CheckGrid(Rectangle pRect, float pCellSize, Camera pCamera = null)
    {
        if (data == null)
        {
            return false;
        }
        if (pCamera == null)
        {
            pCamera = Level.current.camera;
        }
        return Collision.Point(TransformGrid(pCamera, pCellSize), pRect);
    }
}
