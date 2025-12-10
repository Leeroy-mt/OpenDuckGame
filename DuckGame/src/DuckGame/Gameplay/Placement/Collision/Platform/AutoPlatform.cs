namespace DuckGame;

public abstract class AutoPlatform : MaterialThing, IAutoTile, IPlatform, IDontMove, IPathNodeBlocker
{
    protected SpriteMap _sprite;

    public Nubber _leftNub;

    public Nubber _rightNub;

    protected string _tileset;

    public float verticalWidth = 16f;

    public float verticalWidthThick = 16f;

    public float horizontalHeight = 16f;

    protected bool _hasNubs = true;

    protected bool _init50;

    protected bool _collideBottom;

    protected bool _neighborsInitialized;

    protected AutoPlatform _leftBlock;

    protected AutoPlatform _rightBlock;

    protected AutoPlatform _upBlock;

    protected AutoPlatform _downBlock;

    private bool _pathed;

    public bool treeLike;

    public bool cheap;

    public bool neverCheap;

    public bool needsRefresh;

    public bool _hasLeftNub = true;

    public bool _hasRightNub = true;

    public AutoPlatform leftBlock => _leftBlock;

    public AutoPlatform rightBlock => _rightBlock;

    public AutoPlatform upBlock => _upBlock;

    public AutoPlatform downBlock => _downBlock;

    public bool pathed
    {
        get
        {
            return _pathed;
        }
        set
        {
            _pathed = value;
        }
    }

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
            UpdateCollision();
            DoPositioning();
        }
    }

    public override void SetTranslation(Vec2 translation)
    {
        if (_leftNub != null)
        {
            _leftNub.SetTranslation(translation);
        }
        if (_rightNub != null)
        {
            _rightNub.SetTranslation(translation);
        }
        base.SetTranslation(translation);
    }

    public override void Draw()
    {
        flipHorizontal = false;
        if (cheap)
        {
            graphic.UltraCheapStaticDraw(flipHorizontal);
        }
        else
        {
            base.Draw();
        }
    }

    public AutoPlatform(float x, float y, string tileset)
        : base(x, y)
    {
        if (tileset != "")
        {
            _sprite = new SpriteMap(tileset, 16, 16);
        }
        _tileset = tileset;
        graphic = _sprite;
        collisionSize = new Vec2(16f, 16f);
        thickness = 0.2f;
        base.centerx = 8f;
        base.centery = 8f;
        collisionOffset = new Vec2(-8f, -8f);
        base.depth = 0.3f;
        _canBeGrouped = true;
        _isStatic = true;
        _placementCost = 1;
    }

    public virtual void InitializeNeighbors()
    {
        if (!_neighborsInitialized)
        {
            _leftBlock = Level.CheckPointPlacementLayer<AutoPlatform>(base.left - 2f, position.y, this, base.placementLayer);
            _rightBlock = Level.CheckPointPlacementLayer<AutoPlatform>(base.right + 2f, position.y, this, base.placementLayer);
            _upBlock = Level.CheckPointPlacementLayer<AutoPlatform>(position.x, base.top - 2f, this, base.placementLayer);
            _downBlock = Level.CheckPointPlacementLayer<AutoPlatform>(position.x, base.bottom + 2f, this, base.placementLayer);
            _neighborsInitialized = true;
        }
    }

    public override void Initialize()
    {
        DoPositioning();
    }

    public virtual void DoPositioning()
    {
        if (!(Level.current is Editor) && graphic != null)
        {
            cheap = !neverCheap && !RandomLevelNode.editorLoad;
            graphic.position = position;
            graphic.scale = base.scale;
            graphic.center = center;
            graphic.depth = base.depth;
            graphic.alpha = base.alpha;
            graphic.angle = angle;
            (graphic as SpriteMap).ClearCache();
            (graphic as SpriteMap).UpdateFrame();
            if (_leftNub != null)
            {
                _leftNub.cheap = cheap;
                _leftNub.DoPositioning();
            }
            if (_rightNub != null)
            {
                _rightNub.cheap = cheap;
                _rightNub.DoPositioning();
            }
        }
    }

    public override void EditorObjectsChanged()
    {
        PlaceBlock();
    }

    public virtual bool HasNoCollision()
    {
        return _sprite.frame == 50;
    }

    public virtual void UpdatePlatform()
    {
        if (needsRefresh)
        {
            PlaceBlock();
            if ((_sprite.frame == 50 || ((_sprite.frame == 44 || _sprite.frame == 26 || _sprite.frame == 27 || _sprite.frame == 28) && !_collideBottom) || _sprite.frame == 10 || _sprite.frame == 11 || _sprite.frame == 12 || _sprite.frame == 18 || _sprite.frame == 19 || _sprite.frame == 20 || _sprite.frame == 6 || _sprite.frame == 8 || _sprite.frame == 14 || _sprite.frame == 16 || _sprite.frame == 22) && !_init50 && (treeLike || (_sprite.frame != 8 && _sprite.frame != 14 && _sprite.frame != 16 && _sprite.frame != 22)))
            {
                solid = false;
                _init50 = true;
            }
            needsRefresh = false;
        }
        if (!_placed)
        {
            PlaceBlock();
        }
        else if ((_sprite.frame == 50 || ((_sprite.frame == 44 || _sprite.frame == 26 || _sprite.frame == 27 || _sprite.frame == 28) && !_collideBottom) || _sprite.frame == 10 || _sprite.frame == 11 || _sprite.frame == 12 || _sprite.frame == 18 || _sprite.frame == 19 || _sprite.frame == 20 || _sprite.frame == 6 || _sprite.frame == 8 || _sprite.frame == 14 || _sprite.frame == 16 || _sprite.frame == 22) && !_init50 && (treeLike || (_sprite.frame != 8 && _sprite.frame != 14 && _sprite.frame != 16 && _sprite.frame != 22)))
        {
            solid = false;
            _init50 = true;
        }
    }

    public override void Update()
    {
        UpdatePlatform();
        base.Update();
    }

    public override void Terminate()
    {
        TerminateNubs();
    }

    private void TerminateNubs()
    {
        if (_leftNub != null)
        {
            Level.Remove(_leftNub);
            _leftNub = null;
        }
        if (_rightNub != null)
        {
            Level.Remove(_rightNub);
            _rightNub = null;
        }
    }

    public void PlaceBlock()
    {
        _placed = true;
        FindFrame();
        UpdateCollision();
        DoPositioning();
        UpdateNubbers();
    }

    public virtual void UpdateCollision()
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
                _collisionSize.x = verticalWidthThick;
                _collisionOffset.x = -8f;
                break;
            case 1:
            case 2:
            case 7:
            case 18:
            case 26:
                _collisionSize.x = verticalWidthThick;
                _collisionOffset.x = -8f + (16f - verticalWidthThick);
                break;
            case 8:
            case 14:
            case 16:
            case 22:
                if (!treeLike)
                {
                    _collisionSize.x = verticalWidthThick;
                    _collisionOffset.x = -8f + (16f - verticalWidthThick);
                }
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
                _collisionSize.y = horizontalHeight;
                break;
            default:
                _collisionSize.y = 16f;
                break;
        }
    }

    public virtual void UpdateNubbers()
    {
        TerminateNubs();
        if (!_hasNubs || base.removeFromLevel)
        {
            return;
        }
        switch (_sprite.frame)
        {
            case 2:
                if (_hasLeftNub)
                {
                    _leftNub = new Nubber(base.x - 24f, base.y - 8f, left: true, _tileset);
                    Level.Add(_leftNub);
                }
                break;
            case 4:
                if (_hasRightNub)
                {
                    _rightNub = new Nubber(base.x + 8f, base.y - 8f, left: false, _tileset);
                    Level.Add(_rightNub);
                }
                break;
            case 32:
                if (_hasLeftNub)
                {
                    _leftNub = new Nubber(base.x - 24f, base.y - 8f, left: true, _tileset);
                    Level.Add(_leftNub);
                }
                break;
            case 37:
                if (_hasRightNub)
                {
                    _rightNub = new Nubber(base.x + 8f, base.y - 8f, left: false, _tileset);
                    Level.Add(_rightNub);
                }
                break;
            case 40:
                if (_hasRightNub)
                {
                    _rightNub = new Nubber(base.x + 8f, base.y - 8f, left: false, _tileset);
                    Level.Add(_rightNub);
                }
                if (_hasLeftNub)
                {
                    _leftNub = new Nubber(base.x - 24f, base.y - 8f, left: true, _tileset);
                    Level.Add(_leftNub);
                }
                break;
            case 41:
                if (_hasLeftNub)
                {
                    _leftNub = new Nubber(base.x - 24f, base.y - 8f, left: true, _tileset);
                    Level.Add(_leftNub);
                }
                break;
            case 43:
                if (_hasRightNub)
                {
                    _rightNub = new Nubber(base.x + 8f, base.y - 8f, left: false, _tileset);
                    Level.Add(_rightNub);
                }
                break;
            case 49:
                if (_hasRightNub)
                {
                    _rightNub = new Nubber(base.x + 8f, base.y - 8f, left: false, _tileset);
                    Level.Add(_rightNub);
                }
                if (_hasLeftNub)
                {
                    _leftNub = new Nubber(base.x - 24f, base.y - 8f, left: true, _tileset);
                    Level.Add(_leftNub);
                }
                break;
            case 51:
                if (_hasLeftNub)
                {
                    _leftNub = new Nubber(base.x - 24f, base.y - 8f, left: true, _tileset);
                    Level.Add(_leftNub);
                }
                break;
            case 52:
                if (_hasRightNub)
                {
                    _rightNub = new Nubber(base.x + 8f, base.y - 8f, left: false, _tileset);
                    Level.Add(_rightNub);
                }
                break;
        }
        if (_leftNub != null)
        {
            _leftNub.depth = base.depth;
            _leftNub.material = base.material;
        }
        if (_rightNub != null)
        {
            _rightNub.depth = base.depth;
            _rightNub.material = base.material;
        }
    }

    public virtual void FindFrame()
    {
        AutoPlatform left = null;
        AutoPlatform right = null;
        AutoPlatform up = null;
        AutoPlatform down = null;
        AutoPlatform topleft = null;
        AutoPlatform topright = null;
        AutoPlatform bottomleft = null;
        AutoPlatform bottomright = null;
        float off2 = 16f;
        if (!treeLike)
        {
            off2 = 10f;
        }
        up = Level.CheckPointPlacementLayer<AutoPlatform>(base.x, base.y - 17f, this, base.placementLayer);
        down = Level.CheckPointPlacementLayer<AutoPlatform>(base.x, base.y + off2, this, base.placementLayer);
        left = Level.CheckPointPlacementLayer<AutoPlatform>(base.x - 16f, base.y, this, base.placementLayer);
        right = Level.CheckPointPlacementLayer<AutoPlatform>(base.x + 16f, base.y, this, base.placementLayer);
        topleft = Level.CheckPointPlacementLayer<AutoPlatform>(base.x - 16f, base.y - 17f, this, base.placementLayer);
        topright = Level.CheckPointPlacementLayer<AutoPlatform>(base.x + 16f, base.y - 17f, this, base.placementLayer);
        bottomleft = Level.CheckPointPlacementLayer<AutoPlatform>(base.x - 16f, base.y + off2, this, base.placementLayer);
        bottomright = Level.CheckPointPlacementLayer<AutoPlatform>(base.x + 16f, base.y + off2, this, base.placementLayer);
        if (up != null && up._tileset != _tileset)
        {
            up = null;
        }
        if (down != null && down._tileset != _tileset)
        {
            down = null;
        }
        if (left != null && left._tileset != _tileset)
        {
            left = null;
        }
        if (right != null && right._tileset != _tileset)
        {
            right = null;
        }
        if (topleft != null && topleft._tileset != _tileset)
        {
            topleft = null;
        }
        if (topright != null && topright._tileset != _tileset)
        {
            topright = null;
        }
        if (bottomleft != null && bottomleft._tileset != _tileset)
        {
            bottomleft = null;
        }
        if (bottomright != null && bottomright._tileset != _tileset)
        {
            bottomright = null;
        }
        if (up != null)
        {
            if (right != null)
            {
                if (down != null)
                {
                    if (left != null)
                    {
                        if (topleft != null)
                        {
                            if (topright != null)
                            {
                                if (bottomleft != null)
                                {
                                    if (bottomright != null)
                                    {
                                        _sprite.frame = 11;
                                    }
                                    else
                                    {
                                        _sprite.frame = 21;
                                    }
                                }
                                else if (bottomright != null)
                                {
                                    _sprite.frame = 17;
                                }
                                else
                                {
                                    _sprite.frame = 23;
                                }
                            }
                            else if (bottomright != null)
                            {
                                if (bottomleft != null)
                                {
                                    _sprite.frame = 12;
                                }
                            }
                            else if (bottomleft != null)
                            {
                                _sprite.frame = 22;
                            }
                            else
                            {
                                _sprite.frame = 30;
                            }
                        }
                        else if (topright != null)
                        {
                            if (bottomright != null)
                            {
                                if (bottomleft != null)
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
                                _sprite.frame = 24;
                            }
                        }
                        else if (bottomright != null)
                        {
                            if (bottomleft != null)
                            {
                                _sprite.frame = 3;
                            }
                            else
                            {
                                _sprite.frame = 8;
                            }
                        }
                        else if (bottomleft == null)
                        {
                            _sprite.frame = 42;
                        }
                    }
                    else if (topright != null)
                    {
                        if (bottomright != null)
                        {
                            _sprite.frame = 18;
                            return;
                        }
                        if (topleft == null)
                        {
                        }
                        _sprite.frame = 7;
                    }
                    else if (topleft == null && bottomright != null)
                    {
                        _sprite.frame = 2;
                    }
                    else
                    {
                        _sprite.frame = 53;
                    }
                }
                else if (left != null)
                {
                    if (topleft != null)
                    {
                        if (topright != null)
                        {
                            _sprite.frame = 27;
                        }
                        else
                        {
                            _sprite.frame = 29;
                        }
                    }
                    else if (topright != null)
                    {
                        _sprite.frame = 25;
                    }
                    else
                    {
                        _sprite.frame = 57;
                    }
                }
                else if (topright != null)
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
                if (left != null)
                {
                    if (topleft != null)
                    {
                        if (bottomleft != null)
                        {
                            _sprite.frame = 20;
                            return;
                        }
                        if (bottomright == null)
                        {
                        }
                        _sprite.frame = 15;
                        return;
                    }
                    if (topright == null)
                    {
                        if (bottomright != null)
                        {
                            if (bottomleft != null)
                            {
                                _sprite.frame = 4;
                            }
                            else
                            {
                                _sprite.frame = 45;
                            }
                            return;
                        }
                        if (bottomleft != null)
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
            else if (left != null)
            {
                if (topleft != null)
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
        else if (right != null)
        {
            if (down != null)
            {
                if (left != null)
                {
                    if (bottomleft == null && bottomright == null)
                    {
                        _sprite.frame = 34;
                    }
                    else if (topleft != null)
                    {
                        if (topright != null)
                        {
                            _sprite.frame = 3;
                        }
                        else if (bottomright != null)
                        {
                            if (bottomleft != null)
                            {
                                _sprite.frame = 3;
                            }
                        }
                        else if (bottomleft != null)
                        {
                            _sprite.frame = 6;
                        }
                        else
                        {
                            _sprite.frame = 24;
                        }
                    }
                    else if (topright != null)
                    {
                        if (bottomright != null)
                        {
                            if (bottomleft != null)
                            {
                                _sprite.frame = 3;
                            }
                            else
                            {
                                _sprite.frame = 0;
                            }
                        }
                        else if (bottomleft == null)
                        {
                            _sprite.frame = 25;
                        }
                    }
                    else if (bottomright != null)
                    {
                        if (bottomleft != null)
                        {
                            _sprite.frame = 3;
                        }
                        else
                        {
                            _sprite.frame = 8;
                        }
                    }
                    else if (bottomleft != null)
                    {
                        _sprite.frame = 14;
                    }
                    else
                    {
                        _sprite.frame = 34;
                    }
                }
                else if (topleft == null && topright != null && bottomleft != null && bottomright != null)
                {
                    _sprite.frame = 1;
                }
                else if (bottomright != null)
                {
                    _sprite.frame = 2;
                }
                else
                {
                    _sprite.frame = 51;
                }
            }
            else if (left != null)
            {
                if ((bottomleft != null || topleft != null) && (topright != null || bottomright != null))
                {
                    _sprite.frame = 59;
                }
                else if (bottomright != null || topright != null)
                {
                    _sprite.frame = 33;
                }
                else if (bottomleft != null || topleft != null)
                {
                    _sprite.frame = 35;
                }
                else
                {
                    _sprite.frame = 36;
                }
            }
            else if (bottomright != null || topright != null)
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
            if (left != null)
            {
                if (topleft != null)
                {
                    if (topright == null)
                    {
                        if (bottomleft != null)
                        {
                            if (bottomright != null)
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
                else if (topright == null && bottomleft != null)
                {
                    _sprite.frame = 4;
                    return;
                }
                _sprite.frame = 52;
            }
            else
            {
                _sprite.frame = 49;
            }
        }
        else if (left != null)
        {
            if (bottomleft != null || topleft != null)
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
