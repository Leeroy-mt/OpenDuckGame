using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using XnaRenderTarget2D = Microsoft.Xna.Framework.Graphics.RenderTarget2D;

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

public static class RenderTarget2DExtensions
{
    readonly static HashSet<XnaRenderTarget2D> depthRenderTargets = [];

    extension(XnaRenderTarget2D target)
    {
        public static XnaRenderTarget2D CreateSetUpTarget(int width, int height, bool pdepth, RenderTargetUsage usage)
        {
            return new XnaRenderTarget2D(
                Graphics.device,
                MonoMain.hidef ? Math.Min(width, 4096) : Math.Min(width, 2048),
                MonoMain.hidef ? Math.Min(height, 4096) : Math.Min(height, 2048),
                mipMap: false,
                SurfaceFormat.Color,
                pdepth ? DepthFormat.Depth24Stencil8 : DepthFormat.None,
                0,
                usage
            );
        }

        public static XnaRenderTarget2D CreateSetUpTarget(int width, int height, bool pdepth = false)
        {
            return CreateSetUpTarget(width, height, pdepth, RenderTargetUsage.DiscardContents);
        }

        public Texture2D GetTexture2D()
        {
            {
                int width = target.Width, hegiht = target.Height;
                using Stream stream = new MemoryStream();
                target.SaveAsPng(stream, width, hegiht);
                return Texture2D.FromStream(target.GraphicsDevice, stream);
            }

            {
                int width = target.Width, hegiht = target.Height;
                Texture2D texture2D = new(target.GraphicsDevice, width, hegiht);
                var data = new Color[width * hegiht];

                target.GetData(data);
                texture2D.SetData(data);
                return texture2D;
            }
        }

        public bool DepthEnabled()
        {
            return depthRenderTargets.Contains(target);
        }

        public bool EnableDepth()
        {
            return depthRenderTargets.Add(target);
        }

        public bool DisableDepth()
        {
            return depthRenderTargets.Remove(target);
        }
    }
}