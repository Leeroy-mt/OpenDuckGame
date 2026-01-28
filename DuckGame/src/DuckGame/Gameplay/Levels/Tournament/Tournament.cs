using System;
using System.Collections.Generic;

namespace DuckGame;

public class Tournament : Level
{
    private List<TourneyGroup> _groups = new List<TourneyGroup>();

    private List<TourneyGroup> _loserGroups = new List<TourneyGroup>();

    private Random r;

    public override void Initialize()
    {
        Layer.HUD.camera.size *= 4f;
        int numPlayers = 19;
        int numPerMatch = 3;
        TourneyGroup curGroup = new TourneyGroup();
        int idx = 0;
        foreach (Team t in Teams.allRandomized)
        {
            curGroup.AddPlayer(t, ass: true);
            if (curGroup.players.Count == numPerMatch)
            {
                _groups.Add(curGroup);
                curGroup = new TourneyGroup();
                curGroup.groupIndex = _groups.Count;
            }
            idx++;
            if (idx == numPlayers)
            {
                break;
            }
        }
        if (curGroup.players.Count > 0)
        {
            _groups.Add(curGroup);
        }
        TourneyGroup curNext = null;
        List<TourneyGroup> curList = _groups;
        List<TourneyGroup> nextGroup = new List<TourneyGroup>();
        int deep = 1;
        while (curList.Count > 1)
        {
            int gIdx = 0;
            curNext = null;
            foreach (TourneyGroup g in curList)
            {
                if (curNext == null)
                {
                    gIdx = 0;
                    curNext = new TourneyGroup();
                    curNext.groupIndex = nextGroup.Count;
                    curNext.depth = deep;
                    nextGroup.Add(curNext);
                }
                g.next = curNext;
                g.next.AddPlayer(g.players[Rando.Int(g.players.Count - 1)]);
                gIdx++;
                if (gIdx == numPerMatch)
                {
                    curNext = null;
                }
            }
            deep++;
            curList = nextGroup;
            nextGroup = new List<TourneyGroup>();
        }
        base.Initialize();
    }

    public override void Update()
    {
        if (InputProfile.DefaultPlayer1.Pressed("CANCEL"))
        {
            _groups.Clear();
            Rando.generator = new Random(30502);
            int numPlayers = 2 + (int)(InputProfile.DefaultPlayer1.leftTrigger * 30f);
            int numPerMatch = 2 + (int)(InputProfile.DefaultPlayer1.rightTrigger * 2f);
            TourneyGroup curGroup = new TourneyGroup();
            int idx = 0;
            foreach (Team t in Teams.allRandomized)
            {
                curGroup.AddPlayer(t, ass: true);
                if (curGroup.players.Count == numPerMatch)
                {
                    _groups.Add(curGroup);
                    curGroup = new TourneyGroup();
                    curGroup.groupIndex = _groups.Count;
                }
                idx++;
                if (idx == numPlayers)
                {
                    break;
                }
            }
            if (curGroup.players.Count > 0)
            {
                _groups.Add(curGroup);
            }
            TourneyGroup curNext = null;
            List<TourneyGroup> curList = _groups;
            List<TourneyGroup> nextGroup = new List<TourneyGroup>();
            int deep = 1;
            while (curList.Count > 1)
            {
                int gIdx = 0;
                curNext = null;
                foreach (TourneyGroup g in curList)
                {
                    if (curNext == null)
                    {
                        gIdx = 0;
                        curNext = new TourneyGroup();
                        curNext.groupIndex = nextGroup.Count;
                        curNext.depth = deep;
                        nextGroup.Add(curNext);
                    }
                    g.next = curNext;
                    int winnerIndex = Rando.Int(g.players.Count - 1);
                    g.next.AddPlayer(g.players[winnerIndex], ass: true);
                    gIdx++;
                    if (gIdx == numPerMatch)
                    {
                        curNext = null;
                    }
                }
                deep++;
                curList = nextGroup;
                nextGroup = new List<TourneyGroup>();
            }
            _loserGroups.Clear();
            curNext = null;
            curList = _groups;
            List<TourneyGroup> fillLoserGroup = _loserGroups;
            nextGroup = new List<TourneyGroup>();
            List<Team> loserPull = new List<Team>();
            deep = 0;
            while (curList.Count > 1)
            {
                curNext = null;
                foreach (TourneyGroup g2 in curList)
                {
                    if (g2.next == null)
                    {
                        continue;
                    }
                    if (!nextGroup.Contains(g2))
                    {
                        nextGroup.Add(g2);
                    }
                    foreach (Team t2 in g2.players)
                    {
                        if (curNext == null)
                        {
                            curNext = new TourneyGroup();
                            curNext.groupIndex = fillLoserGroup.Count;
                            curNext.depth = deep;
                            fillLoserGroup.Add(curNext);
                        }
                        if (!g2.next.players.Contains(t2))
                        {
                            curNext.AddPlayer(t2, ass: true);
                            loserPull.Add(t2);
                        }
                        if (curNext.players.Count == numPerMatch)
                        {
                            curNext = null;
                        }
                    }
                }
                deep++;
                curList = nextGroup;
                nextGroup = new List<TourneyGroup>();
            }
        }
        base.Update();
    }

    public void DrawGroup(List<TourneyGroup> gr, Vec2 drawPos)
    {
        Vec2 curDrawOffset = new Vec2(0f, 0f);
        List<TourneyGroup> curDrawGroup = gr;
        List<TourneyGroup> nextDrawGroup = new List<TourneyGroup>();
        _ = Vec2.Zero;
        float lastSpacing = 8f;
        float lastOffsetVal = 0f;
        while (curDrawGroup.Count > 0)
        {
            int curNumPlayers = 0;
            foreach (TourneyGroup g in curDrawGroup)
            {
                if (g.next != null && !nextDrawGroup.Contains(g.next))
                {
                    nextDrawGroup.Add(g.next);
                }
                Graphics.DrawLine(drawPos + curDrawOffset + new Vec2(96f, 4f), drawPos + curDrawOffset + new Vec2(96f, (float)(g.players.Count - 1) * (lastSpacing + 8f) + 4f), Color.White);
                foreach (Team t in g.players)
                {
                    string name = (g.assigned[g.players.IndexOf(t)] ? t.name : "???");
                    if (g.depth > 0)
                    {
                        Graphics.DrawLine(drawPos + curDrawOffset + new Vec2(0f, 4f), drawPos + curDrawOffset + new Vec2((9 - (name.Length - 1)) * 8, 4f), Color.White);
                        Graphics.DrawLine(drawPos + curDrawOffset + new Vec2(0f, 4f), t.prevTreeDraw, Color.White);
                    }
                    t.prevTreeDraw = drawPos + curDrawOffset + new Vec2(96f, 4f);
                    Graphics.DrawLine(drawPos + curDrawOffset + new Vec2(90f, 4f), drawPos + curDrawOffset + new Vec2(96f, 4f), Color.White);
                    Graphics.DrawString(name, drawPos + curDrawOffset + new Vec2(88 - name.Length * 8, 0f), Color.White, 1f);
                    curDrawOffset.Y += lastSpacing + 8f;
                    curNumPlayers++;
                }
            }
            curDrawOffset.X += 96f;
            _ = (curNumPlayers * 16 + curDrawGroup.Count * 8) / 2;
            curDrawOffset.Y = lastOffsetVal + (float)(curDrawGroup[0].players.Count - 1) * (lastSpacing + 8f) / 2f;
            lastOffsetVal = curDrawOffset.Y;
            lastSpacing = lastSpacing * (float)curDrawGroup[0].players.Count + (float)(8 * (curDrawGroup[0].players.Count - 1));
            curDrawGroup = nextDrawGroup;
            nextDrawGroup = new List<TourneyGroup>();
            curNumPlayers = 0;
            if (curDrawGroup.Count <= 0)
            {
                break;
            }
        }
    }

    public override void PostDrawLayer(Layer layer)
    {
        if (layer == Layer.HUD)
        {
            DrawGroup(_groups, new Vec2(10f, 10f));
            DrawGroup(_loserGroups, new Vec2(550f, 10f));
        }
        base.PostDrawLayer(layer);
    }

    public override void Draw()
    {
        base.Draw();
    }
}
