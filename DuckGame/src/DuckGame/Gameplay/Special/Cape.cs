using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace DuckGame;

public class Cape : Thing
{
    private float killTime = 0.0001f;

    private float killTimer;

    private float counter;

    private PhysicsObject _attach;

    private List<CapePeice> capePeices = new List<CapePeice>();

    private int maxLength = 10;

    private int minLength = 8;

    private GeometryItemTexture _geo;

    public bool _trail;

    private float yDistance;

    private float _capeWave;

    private float _inverseWave;

    private float _inverseWave2;

    private float _capeWaveMult;

    private Vector2 _lastPos;

    private bool _initLastPos = true;

    public Team.CustomHatMetadata metadata = new Team.CustomHatMetadata(null);

    public Tex2D _capeTexture;

    public bool halfFlag;

    public Cape(float xpos, float ypos, PhysicsObject attach, bool trail = false)
        : base(xpos, ypos)
    {
        graphic = new Sprite("cape");
        visible = attach.visible;
        killTimer = killTime;
        _attach = attach;
        base.Depth = -0.5f;
        _trail = trail;
        _editorCanModify = false;
        if (_trail)
        {
            metadata.CapeTaperStart.value = 0.8f;
            metadata.CapeTaperEnd.value = 0f;
        }
    }

    public override void Update()
    {
        base.Update();
        Vector2 attachOffset = Vector2.Zero;
        if (_attach != null)
        {
            attachOffset = _attach.OffsetLocal(metadata.CapeOffset.value);
            if (_attach.removeFromLevel)
            {
                Level.Remove(this);
                return;
            }
        }
        _trail = metadata.CapeIsTrail.value;
        if (_initLastPos)
        {
            _lastPos = _attach.Position + attachOffset;
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
        if (_trail)
        {
            sin = 0f;
            sin2 = 0f;
            sin3 = 0f;
        }
        _capeWaveMult = velLength * 0.5f;
        float inverseMult = inverseVel * 0.5f;
        offDir = (sbyte)(-_attach.offDir);
        Vector2 p1 = attach.Position + attachOffset;
        Vector2 p2 = attach.Position + attachOffset;
        base.Depth = (metadata.CapeForeground.value ? (attach.Depth + 50) : (attach.Depth - 50));
        p1.Y += yOffset;
        p2.Y += yOffset;
        if (!_trail)
        {
            p1.Y += sin * _capeWaveMult * metadata.CapeWiggleModifier.value.Y * (attach.velocity.X * 0.5f);
            p1.X += sin * _capeWaveMult * metadata.CapeWiggleModifier.value.X * (attach.velocity.Y * 0.2f);
        }
        if (capePeices.Count > 0)
        {
            p2 = capePeices[capePeices.Count - 1].p1;
        }
        if (_trail)
        {
            capePeices.Add(new CapePeice(attach.X + attachOffset.X, attach.Y + attachOffset.Y, metadata.CapeTaperStart.value, p1, p2));
        }
        else
        {
            capePeices.Add(new CapePeice(attach.X - (float)(offDir * -10) + attachOffset.X, attach.Y + 6f + attachOffset.Y, metadata.CapeTaperStart.value, p1, p2));
        }
        int idx = 0;
        foreach (CapePeice cp in capePeices)
        {
            cp.wide = Lerp.FloatSmooth(metadata.CapeTaperEnd.value, metadata.CapeTaperStart.value, (float)idx / (float)(capePeices.Count - 1));
            if (!_trail)
            {
                cp.p1.X += sin2 * inverseMult * metadata.CapeWiggleModifier.value.X * (cp.wide - 0.5f) * 0.9f;
                cp.p2.X += sin2 * inverseMult * metadata.CapeWiggleModifier.value.X * (cp.wide - 0.5f) * 0.9f;
                cp.p1.Y += sin3 * inverseMult * metadata.CapeWiggleModifier.value.Y * (cp.wide - 0.5f) * 0.8f;
                cp.p2.Y += sin3 * inverseMult * metadata.CapeWiggleModifier.value.Y * (cp.wide - 0.5f) * 0.8f;
                cp.p1.Y += metadata.CapeSwayModifier.value.Y;
                cp.p2.Y += metadata.CapeSwayModifier.value.Y;
                cp.p1.X += metadata.CapeSwayModifier.value.X * (float)offDir;
                cp.p2.X += metadata.CapeSwayModifier.value.X * (float)offDir;
                cp.position.X += 0.5f * (float)offDir;
            }
            idx++;
        }
        if (_trail)
        {
            maxLength = 16;
        }
        while (capePeices.Count > maxLength + 1 && capePeices.Count > 0)
        {
            capePeices.RemoveAt(0);
        }
        _lastPos = attach.Position + attachOffset;
        visible = attach.visible;
        if (attach is Holdable && attach.owner != null)
        {
            visible = attach.owner.visible;
            if (attach.owner.owner != null && attach.owner.owner is Duck)
            {
                visible = attach.owner.owner.visible;
            }
        }
        if (_capeTexture == null)
        {
            SetCapeTexture(Content.Load<Texture2D>("plainCape"));
        }
    }

    public void SetCapeTexture(Texture2D tex)
    {
        _capeTexture = tex;
        maxLength = _capeTexture.height / 2 - 6;
        if (halfFlag)
        {
            maxLength = (int)((float)_capeTexture.width * 0.28f) - 6;
        }
    }

    public override void Draw()
    {
        if (_attach != null)
        {
            base.Depth = (metadata.CapeForeground.value ? (_attach.Depth + 50) : (_attach.Depth - 50));
            bool hide = !_attach.visible;
            if (_attach.owner != null)
            {
                hide &= !_attach.owner.visible;
                if (_attach.owner.owner != null)
                {
                    hide &= !_attach.owner.owner.visible;
                }
            }
            if (hide)
            {
                return;
            }
        }
        float capeWide = 13f;
        Vector2 lastPart = Vector2.Zero;
        Vector2 lastEdgeOffset = Vector2.Zero;
        bool hasLastPart = false;
        bool bust = false;
        if (_capeTexture == null)
        {
            return;
        }
        float deep = Graphics.AdjustDepth(base.Depth);
        float uvPart = 1f / (float)(capePeices.Count - 1);
        float uvInc = 0f;
        for (int i = capePeices.Count - 1; i >= 0; i--)
        {
            Vector2 texTL = new Vector2(0f, Math.Min(uvInc + uvPart, 1f));
            Vector2 texTR = new Vector2(1f, Math.Min(uvInc + uvPart, 1f));
            Vector2 texBL = new Vector2(0f, Math.Min(uvInc, 1f));
            Vector2 texBR = new Vector2(1f, Math.Min(uvInc, 1f));
            if (halfFlag)
            {
                texTL = new Vector2(Math.Min(uvInc + uvPart, 1f), 0f);
                texTR = new Vector2(Math.Min(uvInc + uvPart, 1f), 1f);
                texBL = new Vector2(Math.Min(uvInc, 1f), 0f);
                texBR = new Vector2(Math.Min(uvInc, 1f), 1f);
            }
            if (offDir > 0)
            {
                Vector2 vec = texTL;
                Vector2 bbl = texBL;
                texTL = texTR;
                texTR = vec;
                texBL = texBR;
                texBR = bbl;
            }
            CapePeice cp = capePeices[i];
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
                float length = v2.Length();
                v2.Normalize();
                if (length > 2f)
                {
                    pos = lastPart + v2 * 2f;
                }
                float drawAlpha = Lerp.Float(metadata.CapeAlphaStart.value, metadata.CapeAlphaEnd.value, (float)i / (float)(capePeices.Count - 1));
                Graphics.screen.DrawQuad(pos - edgeOffset * (capeWide * cp.wide / 2f), pos + edgeOffset * (capeWide * cp.wide / 2f), lastPart - lastEdgeOffset * (capeWide * cp.wide / 2f), lastPart + lastEdgeOffset * (capeWide * cp.wide / 2f), texTL, texTR, texBL, texBR, deep, _capeTexture, Color.White * drawAlpha);
                if (bust)
                {
                    break;
                }
            }
            if (hasLastPart)
            {
                uvInc += uvPart;
            }
            hasLastPart = true;
            lastPart = pos;
            lastEdgeOffset = edgeOffset;
        }
    }
}
