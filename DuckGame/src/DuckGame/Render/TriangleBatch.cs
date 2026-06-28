using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections;
using System.Collections.Generic;

namespace DuckGame;

/// <summary>
/// Alternative to MTSpriteBatcher and MTSpriteBatch
/// </summary>
public class TriangleBatch
{
    /// <summary>
    /// Represents info about polygon texture and start index in the vertex array
    /// </summary>
    public struct TriangleInfo
    {
        public float Depth;

        public VertexPositionColorTexture V0, V1, V2;

        public Texture2D Texture;

        public Material Effect;
    }

    class TextureComparer : IComparer<TriangleInfo>
    {
        public int Compare(TriangleInfo x, TriangleInfo y)
        {
            return x.Texture != y.Texture ? 1 : 0;
        }
    }

    class DepthComparer : IComparer<TriangleInfo>
    {
        public int Compare(TriangleInfo x, TriangleInfo y)
        {
            return x.Depth.CompareTo(y.Depth);
        }
    }

    class ReverseDepthComparer : IComparer<TriangleInfo>
    {
        public int Compare(TriangleInfo x, TriangleInfo y)
        {
            return y.Depth.CompareTo(x.Depth);
        }
    }

    public GraphicsDevice GraphicsDevice { get; }

    public Matrix Projection => projectionMatrix;

    public Matrix View => viewMatrix;

    public Matrix FullMatrix { get; private set; }

    static float edgeBias = 1E-05f;

    Matrix projectionMatrix;

    Matrix viewMatrix;

    int trianglePosition;

    VertexPositionColorTexture[] vertices;

    TriangleInfo[] triangles;

#if USE_BASICEFFECT
    BasicEffect basicEffect;
#else
    MTEffect spriteEffect, simpleEffect;
#endif

    Effect effect;

    SpriteSortMode spriteSortMode;

    BlendState blendState;

    SamplerState samplerState;

    DepthStencilState depthStencilState;

    RasterizerState rasterizerState;

    IComparer<TriangleInfo> CompareTexture = new TextureComparer();

    IComparer<TriangleInfo> CompareDepth = new DepthComparer();

    IComparer<TriangleInfo> CompareReverseDepth = new ReverseDepthComparer();

    public TriangleBatch(GraphicsDevice device)
    {
        GraphicsDevice = device;

        triangles = new TriangleInfo[512];
        vertices = new VertexPositionColorTexture[triangles.Length * 3];

#if USE_BASICEFFECT
        basicEffect = new BasicEffect(device)
        {
            VertexColorEnabled = true
        };
#else
        spriteEffect = Content.Load<MTEffect>("Shaders/SpriteEffect");
        simpleEffect = Content.Load<MTEffect>("Shaders/SpriteEffectSimple");
#endif
    }

    public void DrawTriangle(
        Vector2 p0,
        Vector2 p1,
        Vector2 p2,
        Vector2 t0,
        Vector2 t1,
        Vector2 t2,
        float depth,
        Texture2D texture,
        Color color
        )
    {
        AppendTriangle(new()
        {
            V0 = new(new(p0, 0), color, t0),
            V1 = new(new(p1, 0), color, t1),
            V2 = new(new(p2, 0), color, t2),
            Depth = depth,
            Texture = texture
        });
    }

    public void DrawQuad(
            Vector2 p0,
            Vector2 p1,
            Vector2 p2,
            Vector2 p3,
            Vector2 t0,
            Vector2 t1,
            Vector2 t2,
            Vector2 t3,
            float depth,
            Texture2D texture,
            Color color
            )
    {
        TriangleInfo triangle0 = new()
        {
            V0 = new(new(p0, 0), color, t0),
            V1 = new(new(p1, 0), color, t1),
            V2 = new(new(p2, 0), color, t2),
            Depth = depth,
            Texture = texture
        };
        TriangleInfo triangle1 = new()
        {
            V0 = new(new(p2, 0), color, t2),
            V1 = new(new(p1, 0), color, t1),
            V2 = new(new(p3, 0), color, t3),
            Depth = depth,
            Texture = texture
        };

        AppendTriangle(triangle0);
        AppendTriangle(triangle1);
    }

    public void DrawQuad(
        Vector2 v0,
        Vector2 v1,
        Vector2 v2,
        Vector2 v3,
        float depth,
        Color color
        )
    {
        DrawQuad(v0, v1, v2, v3, default, default, default, default, depth, null, color);
    }

    public void DrawTexture(
            Tex2D texture,
            Vector2 position,
            RectangleF? sourceRectangle,
            Color color,
            float rotation,
            Vector2 origin,
            Vector2 scale,
            SpriteEffects effect,
            float depth,
            Material fx
            )
    {
        float w = texture.width * scale.X;
        float h = texture.height * scale.Y;
        if (sourceRectangle.HasValue)
        {
            w = sourceRectangle.Value.Width * scale.X;
            h = sourceRectangle.Value.Height * scale.Y;
        }
        DrawTexture(texture, new(position.X, position.Y, w, h), sourceRectangle, color, rotation, origin * scale, effect, depth, fx);
    }

    public void DrawTexture(
            Texture2D texture,
            Vector4 destination,
            RectangleF? source,
            Color color,
            float rotation,
            Vector2 origin,
            SpriteEffects effect,
            float depth,
            Material fx
            )
    {
        float z = depth;

        RectangleF rectangle;
        if (source.HasValue)
            rectangle = source.Value;
        else
            rectangle = new(0, 0, texture.Width, texture.Height);

        Vector2 tl = new(rectangle.X / texture.Width + edgeBias, rectangle.Y / texture.Height + edgeBias),
                br = new((rectangle.X + rectangle.Width) / texture.Width - edgeBias, (rectangle.Y + rectangle.Height) / texture.Height - edgeBias);

        if ((effect & SpriteEffects.FlipVertically) != SpriteEffects.None)
            (br.Y, tl.Y) = (tl.Y, br.Y);
        if ((effect & SpriteEffects.FlipHorizontally) != SpriteEffects.None)
            (br.X, tl.X) = (tl.X, br.X);

        float cos = float.Cos(rotation),
              sin = float.Sin(rotation);

        origin = -origin;

        TriangleInfo triangle0 = new()
        {
            Depth = depth,
            Effect = fx,
            Texture = texture,
            V0 = new(
                new(destination.X + origin.X * cos - origin.Y * sin, destination.Y + origin.X * sin + origin.Y * cos, z),
                color,
                tl),
            V1 = new(
                new(destination.X + (origin.X + destination.Z) * cos - origin.Y * sin, destination.Y + (origin.X + destination.Z) * sin + origin.Y * cos, z),
                color,
                new(br.X, tl.Y)),
            V2 = new(
                new(destination.X + origin.X * cos - (origin.Y + destination.W) * sin, destination.Y + origin.X * sin + (origin.Y + destination.W) * cos, z),
                color,
                new(tl.X, br.Y))
        };
        TriangleInfo triangle1 = new()
        {
            Depth = depth,
            Effect = fx,
            Texture = texture,
            V0 = new(
                new(destination.X + origin.X * cos - (origin.Y + destination.W) * sin, destination.Y + origin.X * sin + (origin.Y + destination.W) * cos, z),
                color,
                new(tl.X, br.Y)),
            V1 = new(
                new(destination.X + (origin.X + destination.Z) * cos - origin.Y * sin, destination.Y + (origin.X + destination.Z) * sin + origin.Y * cos, z),
                color,
                new(br.X, tl.Y)),
            V2 = new(
                new(destination.X + (origin.X + destination.Z) * cos - (origin.Y + destination.W) * sin, destination.Y + (origin.X + destination.Z) * sin + (origin.Y + destination.W) * cos, z),
                color,
                br)
        };

        AppendTriangle(triangle0);
        AppendTriangle(triangle1);
    }

    public TriangleInfo StealLastTriangle()
    {
        if (trianglePosition == 0)
            throw new InvalidOperationException("Triangle list was empty.");

        return triangles[--trianglePosition];
    }

    public void AppendTriangle(TriangleInfo triangle)
    {
        if (triangles.Length <= trianglePosition)
        {
            var newArray = new TriangleInfo[triangles.Length * 2];
            Array.Copy(triangles, newArray, triangles.Length);
            triangles = newArray;
        }
        triangles[trianglePosition++] = triangle;
    }

    public void SubmitExternalBatch(ExternalTriangleBatch batch)
    {
        ExpandBufferIfNeeded(batch.Count);
        Array.Copy(batch.TriangleBuffer, 0, triangles, trianglePosition, batch.Count);
        trianglePosition += batch.Count;
    }

    public void Begin(
        SpriteSortMode spriteSortMode,
        BlendState blendState,
        SamplerState samplerState,
        DepthStencilState depthStencilState,
        RasterizerState rasterizerState,
        MTEffect effect,
        Matrix viewMatrix
        )
    {
        trianglePosition = 0;

        this.spriteSortMode = spriteSortMode;
        this.blendState = blendState;
        this.samplerState = samplerState;
        this.depthStencilState = depthStencilState;
        this.rasterizerState = rasterizerState;
        this.effect = effect;
        this.viewMatrix = viewMatrix;
    }

    /// <summary>
    /// Applies all settings
    /// </summary>
    void PrepareDevice()
    {
        var graphicsDevice = GraphicsDevice;

        graphicsDevice.BlendState = blendState;
        graphicsDevice.DepthStencilState = depthStencilState;
        graphicsDevice.SamplerStates[0] = samplerState;
        graphicsDevice.RasterizerState = rasterizerState;

        var vp = graphicsDevice.Viewport;

        Matrix.CreateOrthographicOffCenter(0, vp.Width, vp.Height, 0, 1, -1, out projectionMatrix);

        FullMatrix = Matrix.Multiply(viewMatrix, projectionMatrix);
    }

    void PrepareEffect(bool enableTexture)
    {
        if (effect != null)
        {
            if (!enableTexture && effect.Techniques.Count > 1 && effect.Techniques[1].Name == "BasicSimple")
                effect.CurrentTechnique = effect.Techniques[1];
            else
                effect.CurrentTechnique = effect.Techniques[0];

            effect.Parameters["MatrixTransform"]?.SetValue(FullMatrix);
            effect.CurrentTechnique.Passes[0].Apply();

            return;
        }

#if USE_BASICEFFECT
        basicEffect.Projection = projectionMatrix;
        basicEffect.View = viewMatrix;
        basicEffect.TextureEnabled = enableTexture;
#else
        var basicEffect = (enableTexture ? spriteEffect : simpleEffect).effect;
        basicEffect.Parameters["MatrixTransform"].SetValue(FullMatrix);
        basicEffect.CurrentTechnique.Passes[0].Apply();
#endif
    }

    void ExpandBufferIfNeeded(int count)
    {
        // TODO: loop-based expansion
        if (trianglePosition + count >= triangles.Length)
        {
            Array.Resize(ref triangles, triangles.Length * 2);
        }
    }

    void PrepareBuffers(SpriteSortMode sortMode)
    {
        switch (sortMode)
        {
            case SpriteSortMode.Texture:
                Array.Sort(triangles, 0, trianglePosition, CompareTexture);
                break;
            case SpriteSortMode.BackToFront:
                Array.Sort(triangles, 0, trianglePosition, CompareReverseDepth);
                break;
            case SpriteSortMode.FrontToBack:
                Array.Sort(triangles, 0, trianglePosition, CompareDepth);
                break;
        }

        if (vertices.Length < trianglePosition * 3)
            vertices = new VertexPositionColorTexture[trianglePosition * 6];

        for (int i = 0; i < trianglePosition; i++)
        {
            var triangle = triangles[i];
            var t = (i + 1) * 3;
            vertices[t - 3] = triangle.V0;
            vertices[t - 2] = triangle.V1;
            vertices[t - 1] = triangle.V2;
        }
    }

    public void End()
    {
        if (trianglePosition == 0)
            return;

        PrepareDevice();
        PrepareBuffers(spriteSortMode);

        var drawCalls = 0;

        int offset = 0, length = 3;
        TriangleInfo triangle = triangles[0];
        for (int i = 1; i < trianglePosition; i++)
        {
            var newTriangle = triangles[i];
            if (newTriangle.Texture == triangle.Texture && newTriangle.Effect == triangle.Effect)
            {
                length += 3;
            }
            else
            {
                Flush(offset, length, triangle.Texture, triangle.Effect, ref drawCalls);

                offset += length;
                length = 3;
                triangle = newTriangle;
            }
        }
        Flush(offset, length, triangle.Texture, triangle.Effect, ref drawCalls);
    }

    /// <summary>
    /// Flushes primitive info to device
    /// <paramref name="offset"/> offset in the vertex array to start flushing from
    /// <paramref name="length"/> number of vertices to flush
    /// </summary>
    void Flush(int offset, int length, Texture2D texture, Material material, ref int drawCalls)
    {
        if (material != null)
        {
            material.SetValue("MatrixTransform", FullMatrix);
            material.Apply();
        }
        else
        {
            PrepareEffect(texture != null);
        }

        GraphicsDevice.Textures[0] = texture;
        GraphicsDevice.DrawUserPrimitives(
            PrimitiveType.TriangleList,
            vertices,
            offset,
            length / 3
            );

        drawCalls++; /* Increasing number of draw calls for debug purposes */
    }
}

public class ExternalTriangleBatch
{
    public TriangleBatch.TriangleInfo[] TriangleBuffer { get; }

    public int Count { get; private set; }

    public int Capacity => TriangleBuffer.Length;

    public ExternalTriangleBatch(int capacity)
    {
        TriangleBuffer = new TriangleBatch.TriangleInfo[capacity];
    }

    public void SetTriangle(
        Vector2 p0,
        Vector2 p1,
        Vector2 p2,
        Color c0,
        Color c1,
        Color c2,
        Vector2 t0,
        Vector2 t1,
        Vector2 t2,
        Texture2D texture,
        float depth
    )
    {
        TriangleBatch.TriangleInfo triangleInfo = new();

        triangleInfo.V0.Position = new(p0, depth);
        triangleInfo.V0.Color = c0;
        triangleInfo.V0.TextureCoordinate = t0;

        triangleInfo.V1.Position = new(p1, depth);
        triangleInfo.V1.Color = c1;
        triangleInfo.V1.TextureCoordinate = t1;

        triangleInfo.V2.Position = new(p2, depth);
        triangleInfo.V2.Color = c2;
        triangleInfo.V2.TextureCoordinate = t2;

        triangleInfo.Texture = texture;
        triangleInfo.Depth = depth;

        TriangleBuffer[Count++] = triangleInfo;
    }

    public void SetTriangle(
        Vector2 p0,
        Vector2 p1,
        Vector2 p2,
        Color c0,
        Color c1,
        Color c2,
        float depth
    )
    {
        SetTriangle(p0, p1, p2, c0, c1, c2, default, default, default, null, depth);
    }

    public void SetTriangle(
        Vector2 p0,
        Vector2 p1,
        Vector2 p2,
        Color color,
        float depth
    )
    {
        SetTriangle(p0, p1, p2, color, color, color, depth);
    }

    public void Clear()
    {
        Count = 0;
    }
}