using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace DuckGame;

public class NetDebugInterface
{
    private bool _visible;

    private NetworkInstance _instance;

    private List<NetDebugElement> _elements = new List<NetDebugElement>();

    private bool _tookInput;

    public bool visible => _visible;

    public NetDebugInterface(NetworkInstance pInstance)
    {
        NetDebugInterface netDebugInterface = this;
        _instance = pInstance;
        NetDebugDropdown connectionDropdown = new NetDebugDropdown(this, "Connection: ", delegate
        {
            List<NetDebugDropdown.Element> list = new List<NetDebugDropdown.Element>
            {
                new NetDebugDropdown.Element
                {
                    name = "ALL",
                    value = null
                }
            };
            foreach (NetworkConnection current in netDebugInterface._instance.network.core.allConnections)
            {
                string text = current.name;
                if (current.profile != null)
                {
                    text = "|" + current.profile.persona.colorUsable.r + "," + current.profile.persona.colorUsable.g + "," + current.profile.persona.colorUsable.b + "|" + text;
                    text = text + " |WHITE|(" + current.profile.networkIndex + ")";
                }
                list.Add(new NetDebugDropdown.Element
                {
                    name = text,
                    value = current
                });
            }
            return list;
        })
        {
            leading = 4f
        };
        _elements.Add(connectionDropdown);
        connectionDropdown.right = new NetDebugButton(this, "Lag Out", null, delegate
        {
            if (connectionDropdown.selected.value is NetworkConnection networkConnection)
            {
                networkConnection.debuggerContext.lagSpike = 10;
            }
        });
        connectionDropdown.right.right = new NetDebugButton(this, "Close", null, delegate
        {
            for (int i = 0; i < 5; i++)
            {
                Send.ImmediateUnreliableBroadcast(new NMClientClosedGame());
            }
            netDebugInterface._instance = NetworkDebugger.Reboot(netDebugInterface._instance);
        });
        connectionDropdown.right.right.right = new NetDebugButton(this, "Reboot", null, delegate
        {
            netDebugInterface._instance = NetworkDebugger.Reboot(netDebugInterface._instance);
        });
        _elements.Add(new NetDebugSlider(this, "Latency: ", delegate
        {
            NetDebugDropdown.Element selected = connectionDropdown.selected;
            NetworkConnection networkConnection = null;
            networkConnection = ((selected.value != null) ? (selected.value as NetworkConnection) : pInstance.duckNetworkCore.localConnection);
            return networkConnection.debuggerContext.latency;
        }, delegate (float f)
        {
            NetDebugDropdown.Element selected = connectionDropdown.selected;
            if (selected.value == null)
            {
                pInstance.duckNetworkCore.localConnection.debuggerContext.latency = f;
            }
            else
            {
                (selected.value as NetworkConnection).debuggerContext.latency = f;
            }
        }, delegate (float f)
        {
            string text = f * 1000f + "ms";
            NetDebugDropdown.Element selected = connectionDropdown.selected;
            if (selected.value == null)
            {
                text = text + " |GREEN|(" + (int)(pInstance.network.core.averagePing * 1000f) + "ms actual)";
                return text + " |YELLOW|(" + (int)(pInstance.network.core.averagePingPeak * 1000f) + "ms peak)";
            }
            NetworkConnection networkConnection = selected.value as NetworkConnection;
            text = text + " |GREEN|(" + (int)(networkConnection.manager.ping * 1000f) + "ms actual)";
            return text + " |YELLOW|(" + (int)(networkConnection.manager.pingPeak * 1000f) + "ms peak)";
        })
        {
            indent = 16f
        });
        _elements.Add(new NetDebugSlider(this, "Jitter: ", delegate
        {
            NetDebugDropdown.Element selected = connectionDropdown.selected;
            NetworkConnection networkConnection = null;
            networkConnection = ((selected.value != null) ? (selected.value as NetworkConnection) : pInstance.duckNetworkCore.localConnection);
            return networkConnection.debuggerContext.jitter;
        }, delegate (float f)
        {
            NetDebugDropdown.Element selected = connectionDropdown.selected;
            if (selected.value == null)
            {
                pInstance.duckNetworkCore.localConnection.debuggerContext.jitter = f;
            }
            else
            {
                (selected.value as NetworkConnection).debuggerContext.jitter = f;
            }
        }, (float f) => f * 1000f + "ms")
        {
            indent = 16f
        });
        _elements.Add(new NetDebugSlider(this, "Loss: ", delegate
        {
            NetDebugDropdown.Element selected = connectionDropdown.selected;
            NetworkConnection networkConnection = null;
            networkConnection = ((selected.value != null) ? (selected.value as NetworkConnection) : pInstance.duckNetworkCore.localConnection);
            return networkConnection.debuggerContext.loss;
        }, delegate (float f)
        {
            NetDebugDropdown.Element selected = connectionDropdown.selected;
            if (selected.value == null)
            {
                pInstance.duckNetworkCore.localConnection.debuggerContext.loss = f;
            }
            else
            {
                (selected.value as NetworkConnection).debuggerContext.loss = f;
            }
        }, delegate (float f)
        {
            string text = f * 100f + "%";
            NetDebugDropdown.Element selected = connectionDropdown.selected;
            if (selected.value == null)
            {
                text = text + " |RED|(" + pInstance.network.core.averagePacketLoss + " lost)";
                return text + " |RED|(" + pInstance.network.core.averagePacketLossPercent + "% avg)";
            }
            NetworkConnection networkConnection = selected.value as NetworkConnection;
            text = text + " |RED|(" + networkConnection.manager.losses + " lost)";
            int num = 0;
            if (networkConnection.manager.losses != 0)
            {
                num = (int)((float)networkConnection.manager.sent / (float)networkConnection.manager.losses * 100f);
            }
            return text + " |RED|(" + num + "% avg)";
        })
        {
            indent = 16f
        });
        _elements.Add(new NetDebugSlider(this, "Duplicate: ", delegate
        {
            NetDebugDropdown.Element selected = connectionDropdown.selected;
            NetworkConnection networkConnection = null;
            networkConnection = ((selected.value != null) ? (selected.value as NetworkConnection) : pInstance.duckNetworkCore.localConnection);
            return networkConnection.debuggerContext.duplicate;
        }, delegate (float f)
        {
            NetDebugDropdown.Element selected = connectionDropdown.selected;
            if (selected.value == null)
            {
                pInstance.duckNetworkCore.localConnection.debuggerContext.duplicate = f;
            }
            else
            {
                (selected.value as NetworkConnection).debuggerContext.duplicate = f;
            }
        }, (float f) => f * 100f + "%")
        {
            indent = 16f
        });
    }

    public void Update()
    {
    }

    public void Draw()
    {
        Rectangle rect = _instance.consoleSize;
        if (rect.Contains(Mouse.positionConsole) && Mouse.right == InputState.Pressed)
        {
            _visible = !_visible;
        }
        if (!_visible)
        {
            return;
        }
        rect.x += 8f;
        rect.width -= 18f;
        rect.y += 8f;
        rect.height = 120f;
        float depth = 0.8f;
        Vector2 offset = rect.tl + new Vector2(8f, 8f);
        foreach (NetDebugElement e in _elements)
        {
            e.depth = depth;
            _tookInput |= e.DoDraw(offset, !_tookInput);
            offset.Y += 10f + e.leading;
            depth -= 0.01f;
        }
        if (Mouse.left == InputState.Released)
        {
            _tookInput = false;
        }
        Graphics.DrawRect(rect, Color.Black * 0.5f, 0f);
    }
}
