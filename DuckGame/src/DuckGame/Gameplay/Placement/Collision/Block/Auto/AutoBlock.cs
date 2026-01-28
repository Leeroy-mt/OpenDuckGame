using System;
using System.Collections.Generic;

namespace DuckGame;

public abstract class AutoBlock : Block, IAutoTile, IDontMove, IPathNodeBlocker
{
    public bool indestructable;

    protected bool _hasNubs = true;

    protected SpriteMap _sprite;

    public Nubber _bLeftNub;

    public Nubber _bRightNub;

    public string _tileset;

    public float verticalWidth = 16f;

    public float verticalWidthThick = 16f;

    public float horizontalHeight = 16f;

    private AutoBlock bLeft;

    private AutoBlock bRight;

    private AutoBlock up;

    private AutoBlock down;

    public ushort blockIndex;

    public static ushort _kBlockIndex;

    private Sprite _brokenSprite;

    public int northIndex;

    public int southIndex;

    public int eastIndex;

    public int westIndex;

    private List<Thing> _additionalBlocks = new List<Thing>();

    private Func<Thing, bool> checkFilter;

    private bool inObjectsChanged;

    protected bool brokeLeft;

    protected bool brokeRight;

    protected bool brokeUp;

    protected bool brokeDown;

    protected bool hasBroke;

    public bool _hasLeftNub = true;

    public bool _hasRightNub = true;

    public bool needsRefresh;

    private bool neededRefresh;

    public bool setLayer = true;

    public bool cheap;

    public bool isFlipped;

    private AutoBlock topbLeft;

    private AutoBlock topbRight;

    private AutoBlock bottombLeft;

    private AutoBlock bottombRight;

    public override int frame
    {
        get
        {
            return _sprite.frame;
        }
        set
        {
            _sprite.frame = value;
            UpdateNubbers();
        }
    }

    public override void SetTranslation(Vec2 translation)
    {
        if (_bLeftNub != null)
        {
            _bLeftNub.SetTranslation(translation);
        }
        if (_bRightNub != null)
        {
            _bRightNub.SetTranslation(translation);
        }
        base.SetTranslation(translation);
    }

    public override void InitializeNeighbors()
    {
        if (_neighborsInitialized)
        {
            return;
        }
        _neighborsInitialized = true;
        if (_leftBlock == null)
        {
            _leftBlock = Level.current.QuadTreePointFilter<AutoBlock>(new Vec2(base.left - 2f, Position.Y), checkFilter);
            if (_leftBlock != null)
            {
                _leftBlock.InitializeNeighbors();
            }
        }
        if (_rightBlock == null)
        {
            _rightBlock = Level.current.QuadTreePointFilter<AutoBlock>(new Vec2(base.right + 2f, Position.Y), checkFilter);
            if (_rightBlock != null)
            {
                _rightBlock.InitializeNeighbors();
            }
        }
        if (_upBlock == null)
        {
            _upBlock = Level.current.QuadTreePointFilter<AutoBlock>(new Vec2(Position.X, base.top - 2f), checkFilter);
            if (_upBlock != null)
            {
                _upBlock.InitializeNeighbors();
            }
        }
        if (_downBlock == null)
        {
            _downBlock = Level.current.QuadTreePointFilter<AutoBlock>(new Vec2(Position.X, base.bottom + 2f), checkFilter);
            if (_downBlock != null)
            {
                _downBlock.InitializeNeighbors();
            }
        }
    }

    public override BinaryClassChunk Serialize()
    {
        BinaryClassChunk element = base.Serialize();
        if (Editor.saving)
        {
            _processedByEditor = true;
            InitializeNeighbors();
            if (base.upBlock != null && !base.upBlock.processedByEditor)
            {
                element.AddProperty("north", base.upBlock.Serialize());
            }
            if (base.downBlock != null && !base.downBlock.processedByEditor)
            {
                element.AddProperty("north", base.downBlock.Serialize());
            }
            if (base.rightBlock != null && !base.rightBlock.processedByEditor)
            {
                element.AddProperty("east", base.rightBlock.Serialize());
            }
            if (base.leftBlock != null && !base.leftBlock.processedByEditor)
            {
                element.AddProperty("west", base.leftBlock.Serialize());
            }
        }
        element.AddProperty("frame", _sprite.frame);
        return element;
    }

    public override bool Deserialize(BinaryClassChunk node)
    {
        base.Deserialize(node);
        BinaryClassChunk n = null;
        if (Editor.saving)
        {
            _additionalBlocks.Clear();
            _neighborsInitialized = true;
            n = node.GetProperty<BinaryClassChunk>("north");
            if (n != null)
            {
                AutoBlock t = (AutoBlock)(_upBlock = Thing.LoadThing(n) as AutoBlock);
                t._downBlock = this;
                _additionalBlocks.Add(t);
            }
            n = node.GetProperty<BinaryClassChunk>("south");
            if (n != null)
            {
                AutoBlock t2 = (AutoBlock)(_downBlock = Thing.LoadThing(n) as AutoBlock);
                t2._upBlock = this;
                _additionalBlocks.Add(t2);
            }
            n = node.GetProperty<BinaryClassChunk>("east");
            if (n != null)
            {
                AutoBlock t3 = (AutoBlock)(_rightBlock = Thing.LoadThing(n) as AutoBlock);
                t3._leftBlock = this;
                _additionalBlocks.Add(t3);
            }
            n = node.GetProperty<BinaryClassChunk>("west");
            if (n != null)
            {
                AutoBlock t4 = (AutoBlock)(_leftBlock = Thing.LoadThing(n) as AutoBlock);
                t4._rightBlock = this;
                _additionalBlocks.Add(t4);
            }
        }
        _sprite.frame = node.GetProperty<int>("frame");
        return true;
    }

    public override DXMLNode LegacySerialize()
    {
        DXMLNode dXMLNode = base.LegacySerialize();
        if (Editor.saving)
        {
            _processedByEditor = true;
            InitializeNeighbors();
            if (base.upBlock != null && !base.upBlock.processedByEditor)
            {
                new DXMLNode("north").Add(base.upBlock.LegacySerialize());
            }
            if (base.downBlock != null && !base.downBlock.processedByEditor)
            {
                new DXMLNode("south").Add(base.downBlock.LegacySerialize());
            }
            if (base.rightBlock != null && !base.rightBlock.processedByEditor)
            {
                new DXMLNode("east").Add(base.rightBlock.LegacySerialize());
            }
            if (base.leftBlock != null && !base.leftBlock.processedByEditor)
            {
                new DXMLNode("west").Add(base.leftBlock.LegacySerialize());
            }
        }
        dXMLNode.Add(new DXMLNode("frame", _sprite.frame));
        return dXMLNode;
    }

    public override bool LegacyDeserialize(DXMLNode node)
    {
        base.LegacyDeserialize(node);
        DXMLNode n = null;
        if (Editor.saving)
        {
            _additionalBlocks.Clear();
            _neighborsInitialized = true;
            n = node.Element("north");
            if (n != null)
            {
                AutoBlock t = (AutoBlock)(_upBlock = Thing.LegacyLoadThing(n) as AutoBlock);
                t._downBlock = this;
                _additionalBlocks.Add(t);
            }
            n = node.Element("south");
            if (n != null)
            {
                AutoBlock t2 = (AutoBlock)(_downBlock = Thing.LegacyLoadThing(n) as AutoBlock);
                t2._upBlock = this;
                _additionalBlocks.Add(t2);
            }
            n = node.Element("east");
            if (n != null)
            {
                AutoBlock t3 = (AutoBlock)(_rightBlock = Thing.LegacyLoadThing(n) as AutoBlock);
                t3._leftBlock = this;
                _additionalBlocks.Add(t3);
            }
            n = node.Element("west");
            if (n != null)
            {
                AutoBlock t4 = (AutoBlock)(_leftBlock = Thing.LegacyLoadThing(n) as AutoBlock);
                t4._rightBlock = this;
                _additionalBlocks.Add(t4);
            }
        }
        n = node.Element("frame");
        if (n != null)
        {
            _sprite.frame = Convert.ToInt32(n.Value);
        }
        return true;
    }

    public override void Added(Level parent)
    {
        foreach (Thing additionalBlock in _additionalBlocks)
        {
            Level.Add(additionalBlock);
        }
        _additionalBlocks.Clear();
        base.Added(parent);
    }

    public AutoBlock(float x, float y, string tileset)
        : base(x, y)
    {
        checkFilter = (Thing blok) => blok != this && (blok as AutoBlock)._tileset == _tileset;
        if (tileset == null)
        {
            tileset = "";
        }
        if (tileset != "")
        {
            _sprite = new SpriteMap(tileset, 16, 16);
            _sprite.frame = 40;
        }
        _tileset = tileset;
        graphic = _sprite;
        collisionSize = new Vec2(16f, 16f);
        thickness = 10f;
        base.CenterX = 8f;
        base.CenterY = 8f;
        collisionOffset = new Vec2(-8f, -8f);
        base.Depth = 0.4f;
        flammable = 0.8f;
        _isStatic = true;
        _canBeGrouped = true;
        base.layer = Layer.Blocks;
        _impactThreshold = 100f;
        blockIndex = _kBlockIndex;
        _kBlockIndex++;
        _placementCost = 1;
    }

    public void RegisterHit(Vec2 hitPos, bool neighbors = false)
    {
    }

    public override bool Hit(Bullet bullet, Vec2 hitPos)
    {
        RegisterHit(hitPos, neighbors: true);
        return base.Hit(bullet, hitPos);
    }

    protected override bool OnBurn(Vec2 position, Thing litBy)
    {
        return false;
    }

    public override void EditorObjectsChanged()
    {
        inObjectsChanged = true;
        PlaceBlock();
        inObjectsChanged = false;
    }

    public void DestroyPuddle(FluidPuddle f)
    {
        if (f != null && !f.removeFromLevel)
        {
            int num = (int)(f.collisionSize.X / 8f);
            float div = f.data.amount / (float)num;
            for (int i = 0; i < num; i++)
            {
                FluidData newFluid = f.data;
                newFluid.amount = div;
                Level.Add(new Fluid(f.left + 8f + (float)(i * 8), f.top - 4f + (float)Math.Sin((float)i * 0.7f) * 2f, new Vec2(0f, 1f), newFluid)
                {
                    vSpeed = -2f
                });
            }
            Level.Remove(f);
        }
    }

    protected override bool OnDestroy(DestroyType type = null)
    {
        if (type is DTRocketExplosion)
        {
            if (up == null)
            {
                up = Level.CheckPoint<AutoBlock>(base.X, base.Y - 16f, this);
            }
            if (down == null)
            {
                down = Level.CheckPoint<AutoBlock>(base.X, base.Y + 16f, this);
            }
            if (bLeft == null)
            {
                bLeft = Level.CheckPoint<AutoBlock>(base.X - 16f, base.Y, this);
            }
            if (bRight == null)
            {
                bRight = Level.CheckPoint<AutoBlock>(base.X + 16f, base.Y, this);
            }
            if (up != null && up._tileset == _tileset)
            {
                up.brokeDown = true;
                up.hasBroke = true;
                up.downBlock = null;
                up.down = null;
            }
            if (down != null && down._tileset == _tileset)
            {
                down.brokeUp = true;
                down.hasBroke = true;
                down.upBlock = null;
                down.up = null;
            }
            if (bLeft != null && bLeft._tileset == _tileset)
            {
                bLeft.brokeRight = true;
                bLeft.hasBroke = true;
                bLeft.rightBlock = null;
                bLeft.bRight = null;
            }
            if (bRight != null && bRight._tileset == _tileset)
            {
                bRight.brokeLeft = true;
                bRight.hasBroke = true;
                bRight.leftBlock = null;
                bRight.bLeft = null;
            }
            if (base.structure != null)
            {
                foreach (Block block in base.structure.blocks)
                {
                    block.structure = null;
                }
                base.structure = null;
            }
            if (up == null)
            {
                FluidPuddle f = Level.CheckPoint<FluidPuddle>(new Vec2(base.X, base.Y - 9f));
                DestroyPuddle(f);
            }
            if (bLeft == null)
            {
                FluidPuddle f2 = Level.CheckPoint<FluidPuddle>(new Vec2(base.X - 9f, base.Y));
                DestroyPuddle(f2);
            }
            if (bRight == null)
            {
                FluidPuddle f3 = Level.CheckPoint<FluidPuddle>(new Vec2(base.X + 9f, base.Y));
                DestroyPuddle(f3);
            }
            return true;
        }
        return false;
    }

    public void UpdateNubbers()
    {
        TerminateNubs();
        if (!_hasNubs || base.removeFromLevel)
        {
            return;
        }
        switch (_sprite.frame)
        {
            case 1:
                if (_hasLeftNub)
                {
                    _bLeftNub = new Nubber(base.X - 24f, base.Y - 8f, left: true, _tileset);
                }
                break;
            case 2:
                if (_hasLeftNub)
                {
                    _bLeftNub = new Nubber(base.X - 24f, base.Y - 8f, left: true, _tileset);
                }
                break;
            case 4:
                if (_hasRightNub)
                {
                    _bRightNub = new Nubber(base.X + 8f, base.Y - 8f, left: false, _tileset);
                }
                break;
            case 5:
                if (_hasRightNub)
                {
                    _bRightNub = new Nubber(base.X + 8f, base.Y - 8f, left: false, _tileset);
                }
                break;
            case 32:
                if (_hasLeftNub)
                {
                    _bLeftNub = new Nubber(base.X - 24f, base.Y - 8f, left: true, _tileset);
                }
                break;
            case 37:
                if (_hasRightNub)
                {
                    _bRightNub = new Nubber(base.X + 8f, base.Y - 8f, left: false, _tileset);
                }
                break;
            case 40:
                if (_hasRightNub)
                {
                    _bRightNub = new Nubber(base.X + 8f, base.Y - 8f, left: false, _tileset);
                }
                if (_hasLeftNub)
                {
                    _bLeftNub = new Nubber(base.X - 24f, base.Y - 8f, left: true, _tileset);
                }
                break;
            case 41:
                if (_hasLeftNub)
                {
                    _bLeftNub = new Nubber(base.X - 24f, base.Y - 8f, left: true, _tileset);
                }
                break;
            case 43:
                if (_hasRightNub)
                {
                    _bRightNub = new Nubber(base.X + 8f, base.Y - 8f, left: false, _tileset);
                }
                break;
            case 49:
                if (_hasRightNub)
                {
                    _bRightNub = new Nubber(base.X + 8f, base.Y - 8f, left: false, _tileset);
                }
                if (_hasLeftNub)
                {
                    _bLeftNub = new Nubber(base.X - 24f, base.Y - 8f, left: true, _tileset);
                }
                break;
            case 51:
                if (_hasLeftNub)
                {
                    _bLeftNub = new Nubber(base.X - 24f, base.Y - 8f, left: true, _tileset);
                }
                break;
            case 52:
                if (_hasRightNub)
                {
                    _bRightNub = new Nubber(base.X + 8f, base.Y - 8f, left: false, _tileset);
                }
                break;
        }
        if (_bLeftNub != null)
        {
            Level.Add(_bLeftNub);
            _bLeftNub.Depth = base.Depth;
            _bLeftNub.layer = base.layer;
            _bLeftNub.material = base.material;
        }
        if (_bRightNub != null)
        {
            Level.Add(_bRightNub);
            _bRightNub.Depth = base.Depth;
            _bRightNub.layer = base.layer;
            _bRightNub.material = base.material;
        }
    }

    public override void Update()
    {
        if (skipWreck)
        {
            skipWreck = false;
        }
        else
        {
            if (shouldWreck)
            {
                Destroy(new DTRocketExplosion(null));
                Level.Remove(this);
            }
            if (needsRefresh)
            {
                PlaceBlock();
                needsRefresh = false;
                neededRefresh = true;
            }
        }
        if (setLayer)
        {
            base.layer = Layer.Blocks;
        }
        base.Update();
    }

    public BlockGroup GroupWithNeighbors(bool addToLevel = true)
    {
        if (_groupedWithNeighbors)
        {
            return null;
        }
        _groupedWithNeighbors = true;
        AutoBlock leftNeighbor = base.leftBlock as AutoBlock;
        AutoBlock rightNeighbor = base.rightBlock as AutoBlock;
        List<AutoBlock> neighbors = new List<AutoBlock>();
        neighbors.Add(this);
        while (leftNeighbor != null && !leftNeighbor._groupedWithNeighbors)
        {
            if (leftNeighbor.collisionSize.Y == collisionSize.Y && leftNeighbor.collisionOffset.Y == collisionOffset.Y)
            {
                neighbors.Add(leftNeighbor);
                leftNeighbor = leftNeighbor.leftBlock as AutoBlock;
            }
            else
            {
                leftNeighbor = null;
            }
        }
        while (rightNeighbor != null && !rightNeighbor._groupedWithNeighbors)
        {
            if (rightNeighbor.collisionSize.Y == collisionSize.Y && rightNeighbor.collisionOffset.Y == collisionOffset.Y)
            {
                neighbors.Add(rightNeighbor);
                rightNeighbor = rightNeighbor.rightBlock as AutoBlock;
            }
            else
            {
                rightNeighbor = null;
            }
        }
        List<AutoBlock> vertNeighbors = new List<AutoBlock>();
        vertNeighbors.Add(this);
        AutoBlock upNeighbor = base.upBlock as AutoBlock;
        AutoBlock downNeighbor = base.downBlock as AutoBlock;
        while (upNeighbor != null && !upNeighbor._groupedWithNeighbors)
        {
            if (upNeighbor.collisionSize.X == collisionSize.X && upNeighbor.collisionOffset.X == collisionOffset.X)
            {
                vertNeighbors.Add(upNeighbor);
                upNeighbor = upNeighbor.upBlock as AutoBlock;
            }
            else
            {
                upNeighbor = null;
            }
        }
        while (downNeighbor != null && !downNeighbor._groupedWithNeighbors)
        {
            if (downNeighbor.collisionSize.X == collisionSize.X && downNeighbor.collisionOffset.X == collisionOffset.X)
            {
                vertNeighbors.Add(downNeighbor);
                downNeighbor = downNeighbor.downBlock as AutoBlock;
            }
            else
            {
                downNeighbor = null;
            }
        }
        List<AutoBlock> blockList = neighbors;
        if (vertNeighbors.Count > blockList.Count)
        {
            blockList = vertNeighbors;
        }
        if (blockList.Count > 1)
        {
            BlockGroup group = new BlockGroup();
            foreach (AutoBlock b in blockList)
            {
                b._groupedWithNeighbors = true;
                group.Add(b);
                if (addToLevel)
                {
                    Level.Remove(b);
                }
            }
            group.CalculateSize();
            if (addToLevel)
            {
                Level.Add(group);
            }
            return group;
        }
        return null;
    }

    public override void Initialize()
    {
        if (_sprite != null)
        {
            UpdateCollision();
        }
        DoPositioning();
    }

    public virtual void DoPositioning()
    {
        if (!(Level.current is Editor) && graphic != null)
        {
            if (!RandomLevelNode.editorLoad)
            {
                cheap = true;
            }
            graphic.Position = Position;
            graphic.Scale = base.Scale;
            graphic.Center = Center;
            graphic.Depth = base.Depth;
            graphic.Alpha = base.Alpha;
            graphic.Angle = Angle;
            (graphic as SpriteMap).ClearCache();
            (graphic as SpriteMap).UpdateFrame();
        }
    }

    public override void Terminate()
    {
        if (!_groupedWithNeighbors || shouldWreck)
        {
            TerminateNubs();
        }
    }

    private void TerminateNubs()
    {
        if (_bLeftNub != null)
        {
            Level.Remove(_bLeftNub);
            _bLeftNub = null;
        }
        if (_bRightNub != null)
        {
            Level.Remove(_bRightNub);
            _bRightNub = null;
        }
    }

    public override void Draw()
    {
        if (DevConsole.showCollision)
        {
            if (base.leftBlock != null)
            {
                Graphics.DrawLine(Position, Position + new Vec2(-8f, 0f), Color.Red * 0.5f, 1f, 1f);
            }
            if (base.rightBlock != null)
            {
                Graphics.DrawLine(Position, Position + new Vec2(8f, 0f), Color.Red * 0.5f, 1f, 1f);
            }
            if (base.upBlock != null)
            {
                Graphics.DrawLine(Position, Position + new Vec2(0f, -8f), Color.Red * 0.5f, 1f, 1f);
            }
            if (base.downBlock != null)
            {
                Graphics.DrawLine(Position, Position + new Vec2(0f, 8f), Color.Red * 0.5f, 1f, 1f);
            }
        }
        if (hasBroke)
        {
            if (_brokenSprite == null)
            {
                _brokenSprite = new Sprite("brokeEdge");
                _brokenSprite.CenterOrigin();
                _brokenSprite.Depth = base.Depth;
            }
            if (brokeLeft)
            {
                _brokenSprite.AngleDegrees = 180f;
                Graphics.Draw(_brokenSprite, base.X - 16f, base.Y);
            }
            if (brokeRight)
            {
                _brokenSprite.AngleDegrees = 0f;
                Graphics.Draw(_brokenSprite, base.X + 16f, base.Y);
            }
            if (brokeUp)
            {
                _brokenSprite.AngleDegrees = 270f;
                Graphics.Draw(_brokenSprite, base.X, base.Y - 16f);
            }
            if (brokeDown)
            {
                _brokenSprite.AngleDegrees = 90f;
                Graphics.Draw(_brokenSprite, base.X, base.Y + 16f);
            }
        }
        if (cheap)
        {
            graphic.UltraCheapStaticDraw(flipHorizontal);
        }
        else
        {
            base.Draw();
        }
    }

    public void UpdateCollision()
    {
        switch (_sprite.frame)
        {
            case 40:
            case 44:
            case 49:
            case 50:
                collisionSize = new Vec2(verticalWidth, 16f);
                collisionOffset = new Vec2((0f - verticalWidth) / 2f, -8f);
                break;
            case 37:
            case 43:
            case 45:
            case 52:
            case 60:
                collisionSize = new Vec2(8f + verticalWidth / 2f, 16f);
                collisionOffset = new Vec2(-8f, -8f);
                break;
            case 32:
            case 41:
            case 51:
            case 53:
            case 58:
                collisionSize = new Vec2(8f + verticalWidth / 2f, 16f);
                collisionOffset = new Vec2((0f - verticalWidth) / 2f, -8f);
                break;
            default:
                collisionSize = new Vec2(16f, 16f);
                collisionOffset = new Vec2(-8f, -8f);
                break;
        }
        switch (_sprite.frame)
        {
            case 4:
            case 5:
            case 15:
            case 20:
            case 28:
                _collisionSize.X = verticalWidthThick;
                _collisionOffset.X = -8f;
                break;
            case 1:
            case 2:
            case 7:
            case 18:
            case 26:
                _collisionSize.X = verticalWidthThick;
                _collisionOffset.X = -8f + (16f - verticalWidthThick);
                break;
        }
        switch (_sprite.frame)
        {
            case 25:
            case 26:
            case 27:
            case 28:
            case 29:
            case 32:
            case 33:
            case 35:
            case 36:
            case 37:
            case 40:
            case 41:
            case 43:
            case 44:
            case 57:
            case 58:
            case 59:
            case 60:
                _collisionSize.Y = horizontalHeight;
                break;
            default:
                _collisionSize.Y = 16f;
                break;
        }
    }

    public void PlaceBlock()
    {
        if (_sprite != null)
        {
            _placed = true;
            FindFrame();
            UpdateNubbers();
            UpdateCollision();
            DoPositioning();
        }
    }

    public void FindFrame()
    {
        up = Level.current.QuadTreePointFilter<AutoBlock>(new Vec2(base.X, base.Y - 16f), checkFilter);
        down = Level.current.QuadTreePointFilter<AutoBlock>(new Vec2(base.X, base.Y + 16f), checkFilter);
        bLeft = Level.current.QuadTreePointFilter<AutoBlock>(new Vec2(base.X - 16f, base.Y), checkFilter);
        bRight = Level.current.QuadTreePointFilter<AutoBlock>(new Vec2(base.X + 16f, base.Y), checkFilter);
        topbLeft = Level.current.QuadTreePointFilter<AutoBlock>(new Vec2(base.X - 16f, base.Y - 16f), checkFilter);
        topbRight = Level.current.QuadTreePointFilter<AutoBlock>(new Vec2(base.X + 16f, base.Y - 16f), checkFilter);
        bottombLeft = Level.current.QuadTreePointFilter<AutoBlock>(new Vec2(base.X - 16f, base.Y + 16f), checkFilter);
        bottombRight = Level.current.QuadTreePointFilter<AutoBlock>(new Vec2(base.X + 16f, base.Y + 16f), checkFilter);
        if (up != null)
        {
            if (bRight != null)
            {
                if (down != null)
                {
                    if (bLeft != null)
                    {
                        if (topbLeft != null)
                        {
                            if (topbRight != null)
                            {
                                if (bottombLeft != null)
                                {
                                    if (bottombRight != null)
                                    {
                                        _sprite.frame = 11;
                                    }
                                    else
                                    {
                                        _sprite.frame = 21;
                                    }
                                }
                                else if (bottombRight != null)
                                {
                                    _sprite.frame = 17;
                                }
                                else
                                {
                                    _sprite.frame = 23;
                                }
                            }
                            else if (bottombRight != null)
                            {
                                if (bottombLeft != null)
                                {
                                    _sprite.frame = 12;
                                }
                            }
                            else if (bottombLeft != null)
                            {
                                _sprite.frame = 22;
                            }
                            else
                            {
                                _sprite.frame = 30;
                            }
                        }
                        else if (topbRight != null)
                        {
                            if (bottombRight != null)
                            {
                                if (bottombLeft != null)
                                {
                                    _sprite.frame = 10;
                                }
                                else
                                {
                                    _sprite.frame = 16;
                                }
                            }
                            else
                            {
                                _ = bottombLeft;
                                _sprite.frame = 24;
                            }
                        }
                        else if (bottombRight != null)
                        {
                            if (bottombLeft != null)
                            {
                                _sprite.frame = 3;
                            }
                            else
                            {
                                _sprite.frame = 8;
                            }
                        }
                        else if (bottombLeft == null)
                        {
                            _sprite.frame = 42;
                        }
                        return;
                    }
                    if (topbRight != null)
                    {
                        if (bottombRight != null)
                        {
                            _sprite.frame = 18;
                            return;
                        }
                        if (topbLeft == null)
                        {
                            _ = bottombLeft;
                        }
                        _sprite.frame = 7;
                        return;
                    }
                    if (topbLeft == null)
                    {
                        if (bottombRight != null)
                        {
                            _sprite.frame = 2;
                            return;
                        }
                        _ = bottombLeft;
                    }
                    _sprite.frame = 53;
                }
                else if (bLeft != null)
                {
                    if (topbLeft != null)
                    {
                        if (topbRight != null)
                        {
                            _sprite.frame = 27;
                        }
                        else
                        {
                            _sprite.frame = 29;
                        }
                    }
                    else if (topbRight != null)
                    {
                        _sprite.frame = 25;
                    }
                    else
                    {
                        _sprite.frame = 57;
                    }
                }
                else if (topbRight != null)
                {
                    _sprite.frame = 26;
                }
                else
                {
                    _sprite.frame = 58;
                }
            }
            else if (down != null)
            {
                if (bLeft != null)
                {
                    if (topbLeft != null)
                    {
                        if (bottombLeft != null)
                        {
                            _sprite.frame = 20;
                            return;
                        }
                        if (bottombRight == null)
                        {
                            _ = topbRight;
                        }
                        _sprite.frame = 15;
                        return;
                    }
                    if (topbRight == null)
                    {
                        if (bottombRight != null)
                        {
                            if (bottombLeft != null)
                            {
                                _sprite.frame = 4;
                            }
                            else
                            {
                                _sprite.frame = 45;
                            }
                            return;
                        }
                        if (bottombLeft != null)
                        {
                            _sprite.frame = 4;
                            return;
                        }
                    }
                    _sprite.frame = 45;
                }
                else
                {
                    _sprite.frame = 50;
                }
            }
            else if (bLeft != null)
            {
                if (topbLeft != null)
                {
                    _sprite.frame = 28;
                }
                else
                {
                    _sprite.frame = 60;
                }
            }
            else
            {
                _sprite.frame = 44;
            }
        }
        else if (bRight != null)
        {
            if (down != null)
            {
                if (bLeft != null)
                {
                    if (bottombLeft == null && bottombRight == null)
                    {
                        _sprite.frame = 34;
                    }
                    else if (topbLeft != null)
                    {
                        if (topbRight != null)
                        {
                            _sprite.frame = 3;
                        }
                        else if (bottombRight != null)
                        {
                            if (bottombLeft != null)
                            {
                                _sprite.frame = 3;
                            }
                        }
                        else if (bottombLeft != null)
                        {
                            _sprite.frame = 6;
                        }
                        else
                        {
                            _sprite.frame = 24;
                        }
                    }
                    else if (topbRight != null)
                    {
                        if (bottombRight != null)
                        {
                            if (bottombLeft != null)
                            {
                                _sprite.frame = 3;
                            }
                            else
                            {
                                _sprite.frame = 0;
                            }
                        }
                        else if (bottombLeft == null)
                        {
                            _sprite.frame = 25;
                        }
                    }
                    else if (bottombRight != null)
                    {
                        if (bottombLeft != null)
                        {
                            _sprite.frame = 3;
                        }
                        else
                        {
                            _sprite.frame = 8;
                        }
                    }
                    else if (bottombLeft != null)
                    {
                        _sprite.frame = 14;
                    }
                    else
                    {
                        _sprite.frame = 34;
                    }
                }
                else if (topbLeft == null && topbRight != null && bottombLeft != null && bottombRight != null)
                {
                    _sprite.frame = 1;
                }
                else if (bottombRight != null)
                {
                    _sprite.frame = 2;
                }
                else
                {
                    _sprite.frame = 51;
                }
            }
            else if (bLeft != null)
            {
                if ((bottombLeft != null || topbLeft != null) && (topbRight != null || bottombRight != null))
                {
                    _sprite.frame = 59;
                }
                else if (bottombRight != null || topbRight != null)
                {
                    _sprite.frame = 33;
                }
                else if (bottombLeft != null || topbLeft != null)
                {
                    _sprite.frame = 35;
                }
                else
                {
                    _sprite.frame = 36;
                }
            }
            else if (bottombRight != null || topbRight != null)
            {
                _sprite.frame = 41;
            }
            else
            {
                _sprite.frame = 32;
            }
        }
        else if (down != null)
        {
            if (bLeft != null)
            {
                if (topbLeft != null)
                {
                    if (topbRight == null)
                    {
                        if (bottombLeft != null)
                        {
                            if (bottombRight != null)
                            {
                                _sprite.frame = 5;
                            }
                            else
                            {
                                _sprite.frame = 4;
                            }
                        }
                        else
                        {
                            _sprite.frame = 52;
                        }
                        return;
                    }
                }
                else if (topbRight == null)
                {
                    if (bottombLeft != null)
                    {
                        _ = bottombRight;
                        _sprite.frame = 4;
                        return;
                    }
                    _ = bottombRight;
                }
                _sprite.frame = 52;
            }
            else
            {
                _sprite.frame = 49;
            }
        }
        else if (bLeft != null)
        {
            if (bottombLeft != null || topbLeft != null)
            {
                _sprite.frame = 43;
            }
            else
            {
                _sprite.frame = 37;
            }
        }
        else
        {
            _sprite.frame = 40;
        }
    }

    public override ContextMenu GetContextMenu()
    {
        return null;
    }
}
