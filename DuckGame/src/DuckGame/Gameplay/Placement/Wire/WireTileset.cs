using System.Collections.Generic;

namespace DuckGame;

[EditorGroup("Stuff|Wires")]
[BaggedProperty("isOnlineCapable", true)]
public class WireTileset : AutoTile
{
    public class WireSignal
    {
        public Vec2 position;

        public WireConnection travel;

        public WireConnection from;

        public WireTileset currentWire;

        public bool finished;

        public int signalType;

        public float life = 1f;

        public Vec2 prevPosition;
    }

    private List<WireConnection> _connections = new List<WireConnection>();

    private List<WireSignal> _signals = new List<WireSignal>();

    private List<WireSignal> _addSignals = new List<WireSignal>();

    private List<WireSignal> _removeSignals = new List<WireSignal>();

    private WireConnection _centerWire;

    private Sprite _signalSprite;

    public bool dullSignalLeft;

    public bool dullSignalRight;

    public bool dullSignalUp;

    public bool dullSignalDown;

    public override int frame
    {
        get
        {
            return base.frame;
        }
        set
        {
            base.frame = value;
            if (base.frame != value)
            {
                UpdateConnections();
            }
        }
    }

    public WireConnection centerWire => _centerWire;

    public WireTileset(float x, float y)
        : base(x, y, "wireTileset")
    {
        _editorName = "Wire";
        editorTooltip = "Connects a Button to other wire objects, allowing the Button to trigger the object's behavior.";
        physicsMaterial = PhysicsMaterial.Metal;
        verticalWidth = 8f;
        verticalWidthThick = 8f;
        horizontalHeight = 8f;
        base.layer = Layer.Foreground;
        base.depth = -0.8f;
        weight = 1f;
        _signalSprite = new Sprite("wireBulge");
        _signalSprite.CenterOrigin();
    }

    public void Emit(WireSignal signal = null, float overshoot = 0f, int type = 0)
    {
        if (_centerWire == null)
        {
            return;
        }
        if (signal == null)
        {
            if (_centerWire.up != null)
            {
                _signals.Add(new WireSignal
                {
                    position = _centerWire.position,
                    prevPosition = _centerWire.position,
                    travel = _centerWire.up,
                    from = _centerWire,
                    currentWire = this,
                    signalType = type
                });
            }
            if (_centerWire.down != null)
            {
                _signals.Add(new WireSignal
                {
                    position = _centerWire.position,
                    prevPosition = _centerWire.position,
                    travel = _centerWire.down,
                    from = _centerWire,
                    currentWire = this,
                    signalType = type
                });
            }
            if (_centerWire.left != null)
            {
                _signals.Add(new WireSignal
                {
                    position = _centerWire.position,
                    prevPosition = _centerWire.position,
                    travel = _centerWire.left,
                    from = _centerWire,
                    currentWire = this,
                    signalType = type
                });
            }
            if (_centerWire.right != null)
            {
                _signals.Add(new WireSignal
                {
                    position = _centerWire.position,
                    prevPosition = _centerWire.position,
                    travel = _centerWire.right,
                    from = _centerWire,
                    currentWire = this,
                    signalType = type
                });
            }
            return;
        }
        WireConnection c = signal.travel;
        _removeSignals.Add(signal);
        signal.finished = true;
        if (c == _centerWire)
        {
            Level.CheckCircle<IWirePeripheral>(_centerWire.position, 3f)?.Pulse(signal.signalType, this);
        }
        if (c.up != null && c.up != signal.from && !dullSignalUp)
        {
            WireSignal s = new WireSignal
            {
                position = c.position,
                travel = c.up,
                from = c,
                currentWire = this,
                life = signal.life,
                prevPosition = signal.prevPosition,
                signalType = signal.signalType
            };
            TravelSignal(s, overshoot, updatePrev: false);
            if (!s.finished)
            {
                _addSignals.Add(s);
            }
        }
        if (c.down != null && c.down != signal.from && !dullSignalDown)
        {
            WireSignal s2 = new WireSignal
            {
                position = c.position,
                travel = c.down,
                from = c,
                currentWire = this,
                life = signal.life,
                prevPosition = signal.prevPosition,
                signalType = signal.signalType
            };
            TravelSignal(s2, overshoot, updatePrev: false);
            if (!s2.finished)
            {
                _addSignals.Add(s2);
            }
        }
        if (c.left != null && c.left != signal.from && !dullSignalLeft)
        {
            WireSignal s3 = new WireSignal
            {
                position = c.position,
                travel = c.left,
                from = c,
                currentWire = this,
                life = signal.life,
                prevPosition = signal.prevPosition,
                signalType = signal.signalType
            };
            TravelSignal(s3, overshoot, updatePrev: false);
            if (!s3.finished)
            {
                _addSignals.Add(s3);
            }
        }
        if (c.right != null && c.right != signal.from && !dullSignalRight)
        {
            WireSignal s4 = new WireSignal
            {
                position = c.position,
                travel = c.right,
                from = c,
                currentWire = this,
                life = signal.life,
                prevPosition = signal.prevPosition,
                signalType = signal.signalType
            };
            TravelSignal(s4, overshoot, updatePrev: false);
            if (!s4.finished)
            {
                _addSignals.Add(s4);
            }
        }
        Vec2 travelPoint = signal.travel.position;
        if (c.wireLeft && !dullSignalLeft && leftTile is WireTileset leftWire && leftWire != signal.currentWire)
        {
            signal.travel = leftWire.GetConnection(travelPoint);
            leftWire.Emit(signal, overshoot, signal.signalType);
        }
        if (c.wireRight && !dullSignalRight && rightTile is WireTileset rightWire && rightWire != signal.currentWire)
        {
            signal.travel = rightWire.GetConnection(travelPoint);
            rightWire.Emit(signal, overshoot, signal.signalType);
        }
        if (c.wireUp && !dullSignalUp && upTile is WireTileset upWire && upWire != signal.currentWire)
        {
            signal.travel = upWire.GetConnection(travelPoint);
            upWire.Emit(signal, overshoot, signal.signalType);
        }
        if (c.wireDown && !dullSignalDown && downTile is WireTileset downWire && downWire != signal.currentWire)
        {
            signal.travel = downWire.GetConnection(travelPoint);
            downWire.Emit(signal, overshoot, signal.signalType);
        }
        dullSignalDown = false;
        dullSignalUp = false;
        dullSignalLeft = false;
        dullSignalRight = false;
    }

    public void TravelSignal(WireSignal signal, float travelSpeed, bool updatePrev = true)
    {
        float overShoot = -1f;
        if (updatePrev)
        {
            signal.prevPosition = signal.position;
        }
        if (signal.travel.position.x < signal.position.x)
        {
            signal.position.x -= travelSpeed;
            overShoot = signal.travel.position.x - signal.position.x;
        }
        else if (signal.travel.position.x > signal.position.x)
        {
            signal.position.x += travelSpeed;
            overShoot = signal.position.x - signal.travel.position.x;
        }
        else if (signal.travel.position.y > signal.position.y)
        {
            signal.position.y += travelSpeed;
            overShoot = signal.position.y - signal.travel.position.y;
        }
        else if (signal.travel.position.y < signal.position.y)
        {
            signal.position.y -= travelSpeed;
            overShoot = signal.travel.position.y - signal.position.y;
        }
        else
        {
            overShoot = 0f;
        }
        signal.life -= travelSpeed / 16f * 0.01f;
        if (overShoot >= 0f && signal.life > 0f)
        {
            Emit(signal, overShoot, signal.signalType);
        }
        if (signal.life <= 0f)
        {
            _removeSignals.Add(signal);
        }
    }

    public WireConnection GetConnection(Vec2 pos)
    {
        float shortest = 9999f;
        WireConnection bestConnection = _centerWire;
        foreach (WireConnection c in _connections)
        {
            float dist = (c.position - pos).lengthSq;
            if (dist < shortest)
            {
                shortest = dist;
                bestConnection = c;
            }
        }
        return bestConnection;
    }

    public override void Update()
    {
        if (_centerWire == null)
        {
            UpdateConnections();
        }
        float travelSpeed = 16f;
        foreach (WireSignal s in _signals)
        {
            TravelSignal(s, travelSpeed);
        }
        foreach (WireSignal s2 in _removeSignals)
        {
            _signals.Remove(s2);
        }
        foreach (WireSignal s3 in _addSignals)
        {
            _signals.Add(s3);
        }
        _removeSignals.Clear();
        _addSignals.Clear();
        base.Update();
    }

    public override void Draw()
    {
        foreach (WireSignal s in _signals)
        {
            _signalSprite.depth = -0.6f;
            _signalSprite.alpha = s.life;
            Sprite signalSprite = _signalSprite;
            float num = (_signalSprite.yscale = 1f);
            signalSprite.xscale = num;
            Graphics.Draw(_signalSprite, s.position.x, s.position.y);
            Vec2 curPos = s.prevPosition;
            Vec2 travelVec = s.position - s.prevPosition;
            float dist = travelVec.length;
            travelVec.Normalize();
            float al = 0.3f;
            for (int i = 0; i < 3; i++)
            {
                _signalSprite.depth -= 1;
                curPos += travelVec * (dist / 4f);
                _signalSprite.alpha = al * s.life;
                al += 0.2f;
                Graphics.Draw(_signalSprite, curPos.x, curPos.y);
            }
        }
        base.Draw();
    }

    private void UpdateConnections()
    {
        upTile = Level.CheckPoint<AutoTile>(base.x, base.y - 16f, this);
        downTile = Level.CheckPoint<AutoTile>(base.x, base.y + 16f, this);
        leftTile = Level.CheckPoint<AutoTile>(base.x - 16f, base.y, this);
        rightTile = Level.CheckPoint<AutoTile>(base.x + 16f, base.y, this);
        _connections.Clear();
        if (_sprite.frame == 32 || _sprite.frame == 41)
        {
            _centerWire = new WireConnection
            {
                position = position + new Vec2(0f, -4f)
            };
            WireConnection right = new WireConnection
            {
                position = position + new Vec2(8f, -4f),
                left = _centerWire,
                wireRight = true
            };
            _centerWire.right = right;
            _connections.Add(_centerWire);
            _connections.Add(right);
        }
        else if (_sprite.frame == 37 || _sprite.frame == 43)
        {
            _centerWire = new WireConnection
            {
                position = position + new Vec2(0f, -4f)
            };
            WireConnection left = new WireConnection
            {
                position = position + new Vec2(-8f, -4f),
                right = _centerWire,
                wireLeft = true
            };
            _centerWire.left = left;
            _connections.Add(_centerWire);
            _connections.Add(left);
        }
        else if (_sprite.frame == 33 || _sprite.frame == 35 || _sprite.frame == 36 || _sprite.frame == 59)
        {
            _centerWire = new WireConnection
            {
                position = position + new Vec2(0f, -4f)
            };
            WireConnection right2 = new WireConnection
            {
                position = position + new Vec2(8f, -4f),
                left = _centerWire,
                wireRight = true
            };
            WireConnection left2 = new WireConnection
            {
                position = position + new Vec2(-8f, -4f),
                right = _centerWire,
                wireLeft = true
            };
            _centerWire.right = right2;
            _centerWire.left = left2;
            _connections.Add(_centerWire);
            _connections.Add(right2);
            _connections.Add(left2);
        }
        else if (_sprite.frame == 34)
        {
            _centerWire = new WireConnection
            {
                position = position + new Vec2(0f, -4f)
            };
            WireConnection right3 = new WireConnection
            {
                position = position + new Vec2(8f, -4f),
                left = _centerWire,
                wireRight = true
            };
            WireConnection left3 = new WireConnection
            {
                position = position + new Vec2(-8f, -4f),
                right = _centerWire,
                wireLeft = true
            };
            WireConnection down = new WireConnection
            {
                position = position + new Vec2(0f, 8f),
                up = _centerWire,
                wireDown = true
            };
            _centerWire.right = right3;
            _centerWire.left = left3;
            _centerWire.down = down;
            _connections.Add(_centerWire);
            _connections.Add(right3);
            _connections.Add(left3);
            _connections.Add(down);
        }
        else if (_sprite.frame == 42)
        {
            _centerWire = new WireConnection
            {
                position = position + new Vec2(0f, -4f)
            };
            WireConnection right4 = new WireConnection
            {
                position = position + new Vec2(8f, -4f),
                left = _centerWire,
                wireRight = true
            };
            WireConnection left4 = new WireConnection
            {
                position = position + new Vec2(-8f, -4f),
                right = _centerWire,
                wireLeft = true
            };
            WireConnection down2 = new WireConnection
            {
                position = position + new Vec2(0f, 8f),
                up = _centerWire,
                wireDown = true
            };
            WireConnection up = new WireConnection
            {
                position = position + new Vec2(0f, -8f),
                down = _centerWire,
                wireUp = true
            };
            _centerWire.right = right4;
            _centerWire.left = left4;
            _centerWire.down = down2;
            _centerWire.up = up;
            _connections.Add(_centerWire);
            _connections.Add(right4);
            _connections.Add(left4);
            _connections.Add(down2);
            _connections.Add(up);
        }
        else if (_sprite.frame == 44)
        {
            _centerWire = new WireConnection
            {
                position = position + new Vec2(0f, 0f)
            };
            WireConnection up2 = new WireConnection
            {
                position = position + new Vec2(0f, -8f),
                down = _centerWire,
                wireUp = true
            };
            _centerWire.up = up2;
            _connections.Add(_centerWire);
            _connections.Add(up2);
        }
        else if (_sprite.frame == 45)
        {
            _centerWire = new WireConnection
            {
                position = position + new Vec2(0f, -4f)
            };
            WireConnection left5 = new WireConnection
            {
                position = position + new Vec2(-8f, -4f),
                right = _centerWire,
                wireLeft = true
            };
            WireConnection down3 = new WireConnection
            {
                position = position + new Vec2(0f, 8f),
                up = _centerWire,
                wireDown = true
            };
            WireConnection up3 = new WireConnection
            {
                position = position + new Vec2(0f, -8f),
                down = _centerWire,
                wireUp = true
            };
            _centerWire.left = left5;
            _centerWire.down = down3;
            _centerWire.up = up3;
            _connections.Add(_centerWire);
            _connections.Add(left5);
            _connections.Add(down3);
            _connections.Add(up3);
        }
        else if (_sprite.frame == 49)
        {
            _centerWire = new WireConnection
            {
                position = position + new Vec2(0f, 0f)
            };
            WireConnection down4 = new WireConnection
            {
                position = position + new Vec2(0f, 8f),
                up = _centerWire,
                wireDown = true
            };
            _centerWire.down = down4;
            _connections.Add(_centerWire);
            _connections.Add(down4);
        }
        else if (_sprite.frame == 50)
        {
            _centerWire = new WireConnection
            {
                position = position + new Vec2(0f, 0f)
            };
            WireConnection down5 = new WireConnection
            {
                position = position + new Vec2(0f, 8f),
                up = _centerWire,
                wireDown = true
            };
            WireConnection up4 = new WireConnection
            {
                position = position + new Vec2(0f, -8f),
                down = _centerWire,
                wireUp = true
            };
            _centerWire.down = down5;
            _centerWire.up = up4;
            _connections.Add(_centerWire);
            _connections.Add(down5);
            _connections.Add(up4);
        }
        else if (_sprite.frame == 51)
        {
            _centerWire = new WireConnection
            {
                position = position + new Vec2(0f, -4f)
            };
            WireConnection right5 = new WireConnection
            {
                position = position + new Vec2(8f, -4f),
                left = _centerWire,
                wireRight = true
            };
            WireConnection down6 = new WireConnection
            {
                position = position + new Vec2(0f, 8f),
                up = _centerWire,
                wireDown = true
            };
            _centerWire.right = right5;
            _centerWire.down = down6;
            _connections.Add(_centerWire);
            _connections.Add(right5);
            _connections.Add(down6);
        }
        else if (_sprite.frame == 52)
        {
            _centerWire = new WireConnection
            {
                position = position + new Vec2(0f, -4f)
            };
            WireConnection left6 = new WireConnection
            {
                position = position + new Vec2(-8f, -4f),
                right = _centerWire,
                wireLeft = true
            };
            WireConnection down7 = new WireConnection
            {
                position = position + new Vec2(0f, 8f),
                up = _centerWire,
                wireDown = true
            };
            _centerWire.left = left6;
            _centerWire.down = down7;
            _connections.Add(_centerWire);
            _connections.Add(left6);
            _connections.Add(down7);
        }
        else if (_sprite.frame == 53)
        {
            _centerWire = new WireConnection
            {
                position = position + new Vec2(0f, -4f)
            };
            WireConnection right6 = new WireConnection
            {
                position = position + new Vec2(8f, -4f),
                left = _centerWire,
                wireRight = true
            };
            WireConnection down8 = new WireConnection
            {
                position = position + new Vec2(0f, 8f),
                up = _centerWire,
                wireDown = true
            };
            WireConnection up5 = new WireConnection
            {
                position = position + new Vec2(0f, -8f),
                down = _centerWire,
                wireUp = true
            };
            _centerWire.right = right6;
            _centerWire.down = down8;
            _centerWire.up = up5;
            _connections.Add(_centerWire);
            _connections.Add(right6);
            _connections.Add(down8);
            _connections.Add(up5);
        }
        else if (_sprite.frame == 57)
        {
            _centerWire = new WireConnection
            {
                position = position + new Vec2(0f, -4f)
            };
            WireConnection right7 = new WireConnection
            {
                position = position + new Vec2(8f, -4f),
                left = _centerWire,
                wireRight = true
            };
            WireConnection left7 = new WireConnection
            {
                position = position + new Vec2(-8f, -4f),
                right = _centerWire,
                wireLeft = true
            };
            WireConnection up6 = new WireConnection
            {
                position = position + new Vec2(0f, -8f),
                down = _centerWire,
                wireUp = true
            };
            _centerWire.right = right7;
            _centerWire.left = left7;
            _centerWire.up = up6;
            _connections.Add(_centerWire);
            _connections.Add(right7);
            _connections.Add(left7);
            _connections.Add(up6);
        }
        else if (_sprite.frame == 58)
        {
            _centerWire = new WireConnection
            {
                position = position + new Vec2(0f, -4f)
            };
            WireConnection right8 = new WireConnection
            {
                position = position + new Vec2(8f, -4f),
                left = _centerWire,
                wireRight = true
            };
            WireConnection up7 = new WireConnection
            {
                position = position + new Vec2(0f, -8f),
                down = _centerWire,
                wireUp = true
            };
            _centerWire.right = right8;
            _centerWire.up = up7;
            _connections.Add(_centerWire);
            _connections.Add(right8);
            _connections.Add(up7);
        }
        else if (_sprite.frame == 60)
        {
            _centerWire = new WireConnection
            {
                position = position + new Vec2(0f, -4f)
            };
            WireConnection left8 = new WireConnection
            {
                position = position + new Vec2(-8f, -4f),
                right = _centerWire,
                wireLeft = true
            };
            WireConnection up8 = new WireConnection
            {
                position = position + new Vec2(0f, -8f),
                down = _centerWire,
                wireUp = true
            };
            _centerWire.left = left8;
            _centerWire.up = up8;
            _connections.Add(_centerWire);
            _connections.Add(left8);
            _connections.Add(up8);
        }
    }
}
