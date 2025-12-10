using System;
using System.Collections.Generic;
using System.Linq;

namespace DuckGame;

public class ConnectionStatusUI
{
    private static ConnectionStatusUICore _core = new ConnectionStatusUICore();

    private static BitmapFont _smallBios;

    private static BitmapFont _smallBios_m;

    private static Sprite _bar;

    private static int spectatorNum = 0;

    public static ConnectionStatusUICore core
    {
        get
        {
            return _core;
        }
        set
        {
            _core = value;
        }
    }

    public static void Initialize()
    {
        _smallBios = new BitmapFont("smallBiosFont", 7, 6);
        _smallBios_m = new BitmapFont("smallBiosFont", 7, 6);
        _bar = new Sprite("statusBar");
    }

    public static void Show()
    {
        spectatorNum = 0;
        _core.bars.Clear();
        foreach (Profile p in Profiles.active)
        {
            _core.bars.Add(new ConnectionStatusBar
            {
                profile = p
            });
            if (p.slotType == SlotType.Spectator || p.pendingSpectatorMode == SlotType.Spectator)
            {
                spectatorNum++;
            }
        }
        _core.bars = new List<ConnectionStatusBar>(_core.bars.OrderBy((ConnectionStatusBar x) => x.profile.slotType == SlotType.Spectator || x.profile.pendingSpectatorMode == SlotType.Spectator));
        _core.open = true;
    }

    public static void Hide()
    {
        _core.open = false;
    }

    public static void Update()
    {
        if (_core.tempShow > 0)
        {
            if (!core.open)
            {
                Show();
            }
            _core.tempShow--;
            if (_core.tempShow <= 0)
            {
                _core.tempShow = 0;
                Hide();
            }
        }
        if (_core.open)
        {
            ConnectionStatusBar prev = null;
            {
                foreach (ConnectionStatusBar bar in _core.bars)
                {
                    if (prev == null || prev.position > 0.3f)
                    {
                        bar.position = Lerp.FloatSmooth(bar.position, 1f, 0.16f, 1.1f);
                    }
                    prev = bar;
                }
                return;
            }
        }
        ConnectionStatusBar prev2 = null;
        foreach (ConnectionStatusBar bar2 in _core.bars)
        {
            if (prev2 == null || prev2.position < 0.7f)
            {
                bar2.position = Lerp.FloatSmooth(bar2.position, 0f, 0.08f, 1.1f);
            }
            prev2 = bar2;
        }
    }

    public static void Draw()
    {
        int numElements = _core.bars.Count;
        if (spectatorNum > 0)
        {
            numElements++;
        }
        float heightPerElement = 14f;
        Vec2 drawPos = new Vec2(30f, Layer.HUD.height / 2f - (float)numElements * heightPerElement / 2f);
        bool didSpectatorIncrement = false;
        int i = 0;
        foreach (ConnectionStatusBar bar in _core.bars)
        {
            if (bar.profile.slotType == SlotType.Spectator && !didSpectatorIncrement)
            {
                didSpectatorIncrement = true;
                i++;
            }
            if (bar.profile.connection == null || bar.profile.connection.status == ConnectionStatus.Disconnected)
            {
                continue;
            }
            if (bar.position > 0.01f)
            {
                Vec2 meDraw = new Vec2(drawPos.x, drawPos.y + (float)(i * 14));
                meDraw.x -= Layer.HUD.width * (1f - bar.position);
                _bar.depth = 0.84f;
                Graphics.Draw(_bar, meDraw.x, meDraw.y);
                _smallBios.depth = 0.9f;
                string type = "CUSTOM";
                int transferProgress = 0;
                int transferSize = 0;
                bool local = false;
                if (bar.profile.connection == DuckNetwork.localConnection)
                {
                    transferProgress = DuckNetwork.core.levelTransferProgress;
                    transferSize = DuckNetwork.core.levelTransferSize;
                    if (DuckNetwork.core.logTransferSize > 0)
                    {
                        transferProgress = DuckNetwork.core.logTransferProgress * 500;
                        transferSize = DuckNetwork.core.logTransferSize * 500;
                        type = "LOG";
                    }
                    local = true;
                }
                else
                {
                    transferProgress = bar.profile.connection.dataTransferProgress;
                    transferSize = bar.profile.connection.dataTransferSize;
                    if (bar.profile.connection.logTransferSize > 0)
                    {
                        transferProgress = bar.profile.connection.logTransferProgress * 500;
                        transferSize = bar.profile.connection.logTransferSize * 500;
                        type = "LOG";
                    }
                }
                if (transferProgress != transferSize)
                {
                    _smallBios.scale = new Vec2(0.5f, 0.5f);
                    if (type == "LOG")
                    {
                        if (local)
                        {
                            _smallBios.Draw("@ONLINENEUTRAL@|DGYELLOW|SENDING LOG   " + transferProgress + "\\" + transferSize + "B", new Vec2(meDraw.x + 3f, meDraw.y + 3f), Color.White, 0.9f);
                        }
                        else
                        {
                            _smallBios.Draw("@ONLINENEUTRAL@|DGYELLOW|DOWNLOADING LOG " + type + " " + transferProgress + "\\" + transferSize + "B", new Vec2(meDraw.x + 3f, meDraw.y + 3f), Color.White, 0.9f);
                        }
                    }
                    else if (local)
                    {
                        _smallBios.Draw("@ONLINENEUTRAL@|DGYELLOW|DOWNLOADING   " + transferProgress + "\\" + transferSize + "B", new Vec2(meDraw.x + 3f, meDraw.y + 3f), Color.White, 0.9f);
                    }
                    else
                    {
                        _smallBios.Draw("@ONLINENEUTRAL@|DGYELLOW|SENDING " + type + " " + transferProgress + "\\" + transferSize + "B", new Vec2(meDraw.x + 3f, meDraw.y + 3f), Color.White, 0.9f);
                    }
                    float progress = (float)transferProgress / (float)transferSize;
                    int barHeight = 3;
                    int barLeftOffset = 11;
                    int barYOffset = 7;
                    int barWidth = 90;
                    Graphics.DrawRect(meDraw + new Vec2(barLeftOffset, barYOffset), meDraw + new Vec2(barLeftOffset + barWidth, barYOffset + barHeight), Color.White, 0.9f, filled: false, 0.5f);
                    Graphics.DrawRect(meDraw + new Vec2(barLeftOffset, barYOffset), meDraw + new Vec2((float)barLeftOffset + (float)barWidth * progress, barYOffset + barHeight), Colors.DGGreen, 0.87f);
                    Graphics.DrawRect(meDraw + new Vec2(barLeftOffset, barYOffset), meDraw + new Vec2(barLeftOffset + barWidth, barYOffset + barHeight), Colors.DGRed, 0.84f);
                }
                else if (bar.profile.connection.levelIndex != DuckNetwork.levelIndex)
                {
                    _smallBios.Draw("@ONLINENEUTRAL@|DGYELLOW|SENDING...", new Vec2(meDraw.x + 3f, meDraw.y + 3f), Color.White, 0.9f);
                }
                else
                {
                    _smallBios.Draw("@ONLINEGOOD@|DGGREEN|READY!", new Vec2(meDraw.x + 3f, meDraw.y + 3f), Color.White, 0.9f);
                }
                _smallBios.scale = new Vec2(1f, 1f);
                string name = bar.profile.nameUI;
                if (name.Length > 14)
                {
                    name = name.Substring(0, 14) + "..";
                }
                string colorString = "|" + bar.profile.persona.colorUsable.r + "," + bar.profile.persona.colorUsable.g + "," + bar.profile.persona.colorUsable.b + "|";
                if (bar.profile.connection != null && bar.profile.connection.isHost)
                {
                    name = "@HOSTCROWN@" + name;
                }
                if (bar.profile.slotType == SlotType.Spectator || bar.profile.pendingSpectatorMode == SlotType.Spectator)
                {
                    name = "@SPECTATOR@" + name;
                    colorString = "|DGPURPLE|";
                }
                name = colorString + name;
                _smallBios.Draw(name, new Vec2(meDraw.x + (float)_bar.width - 3f - _smallBios.GetWidth(name) - 60f, meDraw.y + 3f), Color.White, 0.9f);
                int pingval = (int)Math.Round(bar.profile.connection.manager.ping * 1000f);
                if (bar.profile.connection == DuckNetwork.localConnection)
                {
                    pingval = 0;
                }
                string ping = pingval.ToString();
                ping += "|WHITE|MS";
                ping.Count();
                ping = ((pingval < 150) ? ("|DGGREEN|" + ping + "@SIGNALGOOD@") : ((pingval < 250) ? ("|DGYELLOW|" + ping + "@SIGNALNORMAL@") : ((bar.profile.connection.status != ConnectionStatus.Connected) ? ("|DGRED|" + ping + "@SIGNALDEAD@") : ("|DGRED|" + ping + "@SIGNALBAD@"))));
                _smallBios.Draw(ping, new Vec2(meDraw.x + (float)_bar.width - 3f - _smallBios.GetWidth(ping), meDraw.y + 3f), Color.White, 0.9f);
            }
            i++;
        }
    }
}
