using Microsoft.Xna.Framework.Graphics;
using System;

namespace DuckGame;

public class RenderTarget2D : Tex2D
{
    public bool depth;

    public RenderTarget2D(int width, int height, bool pdepth, RenderTargetUsage usage)
        : base(new Microsoft.Xna.Framework.Graphics.RenderTarget2D(Graphics.device, MonoMain.hidef ? Math.Min(width, 4096) : Math.Min(width, 2048), MonoMain.hidef ? Math.Min(height, 4096) : Math.Min(height, 2048), mipMap: false, SurfaceFormat.Color, pdepth ? DepthFormat.Depth24Stencil8 : DepthFormat.None, 0, usage), "__renderTarget", 0)
    {
        depth = pdepth;
    }

    public RenderTarget2D(int width, int height, bool pdepth = false)
        : base(new Microsoft.Xna.Framework.Graphics.RenderTarget2D(Graphics.device, MonoMain.hidef ? Math.Min(width, 4096) : Math.Min(width, 2048), MonoMain.hidef ? Math.Min(height, 4096) : Math.Min(height, 2048), mipMap: false, SurfaceFormat.Color, pdepth ? DepthFormat.Depth24Stencil8 : DepthFormat.None, 0, RenderTargetUsage.DiscardContents), "__renderTarget", 0)
    {
        depth = pdepth;
    }

    public Tex2D ToTex2D()
    {
        Tex2D tex2D = new Tex2D(width, height);
        tex2D.SetData(GetData());
        return tex2D;
    }
}
