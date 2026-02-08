using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace DuckGame;

public class RainbowTrail : Thing
{
    private float killTime = 0.0001f;

    private float killTimer;

    private float counter;

    private PhysicsObject _attach;

    private List<TrailPiece> capePeices = new List<TrailPiece>();

    private int maxLength = 10;

    private int minLength = 8;

    private GeometryItemTexture _geo;

    private float yDistance;

    private float _capeWave;

    private float _inverseWave;

    private float _inverseWave2;

    private float _capeWaveMult;

    private Vector2 _lastPos;

    private bool _initLastPos = true;

    private Tex2D _capeTexture;

    public RainbowTrail(float xpos, float ypos, PhysicsObject attach)
        : base(xpos, ypos)
    {
        graphic = new Sprite("cape");
        visible = attach.visible;
        killTimer = killTime;
        _attach = attach;
        base.Depth = -0.5f;
    }

    public override void Update()
    {
        base.Update();
        if (_initLastPos)
        {
            _lastPos = _attach.Position;
            _initLastPos = false;
        }
        Thing attach = _attach;
        float yOffset = 1f;
        if (attach is TeamHat && attach.owner != null)
        {
            attach = attach.owner;
        }
        if (attach is Duck)
        {
            if ((attach as Duck).ragdoll != null && (attach as Duck).ragdoll.part1 != null)
            {
                attach = (attach as Duck).ragdoll.part1;
            }
            else
            {
                if ((attach as Duck).crouch)
                {
                    yOffset += 5f;
                }
                if ((attach as Duck).sliding)
                {
                    yOffset += 2f;
                }
            }
        }
        float velLength = attach.velocity.Length();
        if (velLength > 3f)
        {
            velLength = 3f;
        }
        float inverseVel = 1f - velLength / 3f;
        _capeWave += velLength * 0.1f;
        _inverseWave += inverseVel * 0.09f;
        _inverseWave2 += inverseVel * 0.06f;
        float sin = (float)Math.Sin(_capeWave);
        float sin2 = (float)Math.Sin(_inverseWave);
        float sin3 = (float)Math.Sin(_inverseWave2);
        _capeWaveMult = velLength * 0.5f;
        float inverseMult = inverseVel * 0.5f;
        offDir = (sbyte)(-_attach.offDir);
        Vector2 p1 = attach.Position;
        Vector2 p2 = attach.Position;
        base.Depth = attach.Depth - 18;
        p1.Y += yOffset;
        p2.Y += yOffset;
        p1.Y += sin * _capeWaveMult * (attach.velocity.X * 0.5f);
        p1.X += sin * _capeWaveMult * (attach.velocity.Y * 0.2f);
        if (capePeices.Count > 0)
        {
            p2 = capePeices[capePeices.Count - 1].p1;
        }
        capePeices.Add(new TrailPiece(attach.X - (float)(offDir * -10), attach.Y + 6f, 0.5f, p1, p2));
        foreach (TrailPiece cp in capePeices)
        {
            if (cp.wide < 1f)
            {
                cp.wide += 0.05f;
            }
            cp.p1.X += sin2 * inverseMult * (cp.wide - 0.5f) * 0.9f;
            cp.p2.X += sin2 * inverseMult * (cp.wide - 0.5f) * 0.9f;
            cp.p1.Y += sin3 * inverseMult * (cp.wide - 0.5f) * 0.8f;
            cp.p2.Y += sin3 * inverseMult * (cp.wide - 0.5f) * 0.8f;
            cp.p1.Y += 1f;
            cp.p2.Y += 1f;
            cp.p1.X += 0.3f * (float)offDir;
            cp.p2.X += 0.3f * (float)offDir;
            cp.position.X += 0.5f * (float)offDir;
        }
        while (capePeices.Count > maxLength)
        {
            capePeices.RemoveAt(0);
        }
        _lastPos = attach.Position;
        visible = attach.visible;
        if (_capeTexture == null)
        {
            _capeTexture = Content.Load<Texture2D>("plainCape");
        }
    }

    public override void Draw()
    {
        float maxCapeLength = 22f;
        float curLength = 0f;
        float capeWide = 13f;
        Vector2 lastPart = Vector2.Zero;
        Vector2 lastEdgeOffset = Vector2.Zero;
        bool hasLastPart = false;
        bool bust = false;
        Vector2 texTL = new Vector2(0f, 0f);
        Vector2 texTR = new Vector2(1f, 0f);
        Vector2 texBL = new Vector2(0f, 1f);
        Vector2 texBR = new Vector2(1f, 1f);
        if (_capeTexture == null)
        {
            return;
        }
        float deep = Graphics.AdjustDepth(base.Depth);
        for (int i = capePeices.Count - 1; i >= 0; i--)
        {
            TrailPiece cp = capePeices[i];
            Vector2 edgeOffset = lastEdgeOffset;
            if (i > 0)
            {
                Vector2 v = cp.p1 - capePeices[i - 1].p1;
                v.Normalize();
                edgeOffset = v.Rotate(Maths.DegToRad(90f), Vector2.Zero);
            }
            Vector2 pos = cp.p1;
            if (hasLastPart)
            {
                Vector2 v2 = pos - lastPart;
                float partLength = v2.Length();
                curLength += partLength;
                v2.Normalize();
                if (curLength > maxCapeLength)
                {
                    pos = lastPart + v2 * (partLength - (curLength - maxCapeLength));
                    bust = true;
                }
                Graphics.screen.DrawQuad(pos - edgeOffset * (capeWide * cp.wide / 2f), pos + edgeOffset * (capeWide * cp.wide / 2f), lastPart - lastEdgeOffset * (capeWide * cp.wide / 2f), lastPart + lastEdgeOffset * (capeWide * cp.wide / 2f), texTL, texTR, texBL, texBR, deep, _capeTexture, Color.White);
                if (bust)
                {
                    break;
                }
            }
            hasLastPart = true;
            lastPart = pos;
            lastEdgeOffset = edgeOffset;
        }
    }
}
