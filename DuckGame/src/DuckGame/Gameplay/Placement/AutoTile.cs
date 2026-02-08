using Microsoft.Xna.Framework;

namespace DuckGame;

public abstract class AutoTile : MaterialThing, IAutoTile, IDontMove, IPathNodeBlocker
{
    protected SpriteMap _sprite;

    protected Nubber _leftNub;

    protected Nubber _rightNub;

    private string _tileset;

    public float verticalWidth = 16;

    public float verticalWidthThick = 16;

    public float horizontalHeight = 16;

    protected bool _hasNubs = true;

    protected bool _init50;

    public AutoTile leftTile;

    public AutoTile rightTile;

    public AutoTile upTile;

    public AutoTile downTile;

    public bool needsRefresh;

    public override int frame
    {
        get => _sprite.frame;
        set
        {
            _sprite.frame = value;
            UpdateCollision();
        }
    }

    public override void SetTranslation(Vector2 translation)
    {
        _leftNub?.SetTranslation(translation);
        _rightNub?.SetTranslation(translation);
        base.SetTranslation(translation);
    }

    public AutoTile(float x, float y, string tileset)
        : base(x, y)
    {
        _sprite = new(tileset, 16, 16);
        _tileset = tileset;
        graphic = _sprite;
        collisionSize = new(16, 16);
        thickness = 0.2f;
        CenterX = 8;
        CenterY = 8;
        collisionOffset = new(-8, -8);
        Depth = 0.3f;
        _canBeGrouped = true;
        _isStatic = true;
        _placementCost = 1;
    }

    public override void EditorObjectsChanged()
    {
        PlaceBlock();
    }

    public override void Update()
    {
        if (needsRefresh)
        {
            PlaceBlock();
            needsRefresh = false;
        }
        if (!_placed)
            PlaceBlock();
        base.Update();
    }

    public void PlaceBlock()
    {
        _placed = true;
        FindFrame();
        UpdateCollision();
    }

    public void UpdateCollision()
    {
        switch (_sprite.frame)
        {
            case 40:
            case 44:
            case 49:
            case 50:
                collisionSize = new(verticalWidth, 16);
                collisionOffset = new((-verticalWidth) / 2, -8);
                break;
            case 37:
            case 43:
            case 45:
            case 52:
            case 60:
                collisionSize = new(8 + verticalWidth / 2, 16);
                collisionOffset = new(-8, -8);
                break;
            case 32:
            case 41:
            case 51:
            case 53:
            case 58:
                collisionSize = new(8 + verticalWidth / 2, 16);
                collisionOffset = new((0 - verticalWidth) / 2, -8);
                break;
            default:
                collisionSize = new(16, 16);
                collisionOffset = new(-8, -8);
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
                _collisionOffset.X = -8 + (16 - verticalWidthThick);
                break;
        }
        _collisionSize.Y = _sprite.frame switch
        {
            25 or 26 or 27 or 28 or 29 or 32 or 33 or 35 or 36 or 37 or 40 or 41 or 43 or 44 or 57 or 58 or 59 or 60 => horizontalHeight,
            _ => 16
        };
    }

    public void FindFrame()
    {
        var up = Level.CheckPoint<AutoTile>(X, Y - 16, this);
        var down = Level.CheckPoint<AutoTile>(X, Y + 16, this);
        var left = Level.CheckPoint<AutoTile>(X - 16, Y, this);
        var right = Level.CheckPoint<AutoTile>(X + 16, Y, this);
        var topleft = Level.CheckPoint<AutoTile>(X - 16, Y - 16, this);
        var topright = Level.CheckPoint<AutoTile>(X + 16, Y - 16, this);
        var bottomleft = Level.CheckPoint<AutoTile>(X - 16, Y + 16, this);
        var bottomright = Level.CheckPoint<AutoTile>(X + 16, Y + 16, this);

        if (up != null && up._tileset != _tileset)
            up = null;
        if (down != null && down._tileset != _tileset)
            down = null;
        if (left != null && left._tileset != _tileset)
            left = null;
        if (right != null && right._tileset != _tileset)
            right = null;
        if (topleft != null && topleft._tileset != _tileset)
            topleft = null;
        if (topright != null && topright._tileset != _tileset)
            topright = null;
        if (bottomleft != null && bottomleft._tileset != _tileset)
            bottomleft = null;
        if (bottomright != null && bottomright._tileset != _tileset)
            bottomright = null;
        leftTile = left;
        rightTile = right;
        upTile = up;
        downTile = down;
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
                                        frame = 11;
                                    }
                                    else
                                    {
                                        frame = 21;
                                    }
                                }
                                else if (bottomright != null)
                                {
                                    frame = 17;
                                }
                                else
                                {
                                    frame = 23;
                                }
                            }
                            else if (bottomright != null)
                            {
                                if (bottomleft != null)
                                {
                                    frame = 12;
                                }
                            }
                            else if (bottomleft != null)
                            {
                                frame = 22;
                            }
                            else
                            {
                                frame = 30;
                            }
                        }
                        else if (topright != null)
                        {
                            if (bottomright != null)
                            {
                                if (bottomleft != null)
                                {
                                    frame = 10;
                                }
                                else
                                {
                                    frame = 16;
                                }
                            }
                            else
                            {
                                frame = 24;
                            }
                        }
                        else if (bottomright != null)
                        {
                            if (bottomleft != null)
                            {
                                frame = 3;
                            }
                            else
                            {
                                frame = 8;
                            }
                        }
                        else if (bottomleft == null)
                        {
                            frame = 42;
                        }
                    }
                    else if (topright != null)
                    {
                        if (bottomright != null)
                        {
                            frame = 18;
                            return;
                        }
                        if (topleft == null)
                        {
                        }
                        frame = 7;
                    }
                    else if (topleft == null && bottomright != null)
                    {
                        frame = 2;
                    }
                    else
                    {
                        frame = 53;
                    }
                }
                else if (left != null)
                {
                    if (topleft != null)
                    {
                        if (topright != null)
                        {
                            frame = 27;
                        }
                        else
                        {
                            frame = 29;
                        }
                    }
                    else if (topright != null)
                    {
                        frame = 25;
                    }
                    else
                    {
                        frame = 57;
                    }
                }
                else if (topright != null)
                {
                    frame = 26;
                }
                else
                {
                    frame = 58;
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
                            frame = 20;
                            return;
                        }
                        if (bottomright == null)
                        {
                        }
                        frame = 15;
                        return;
                    }
                    if (topright == null)
                    {
                        if (bottomright != null)
                        {
                            if (bottomleft != null)
                            {
                                frame = 4;
                            }
                            else
                            {
                                frame = 45;
                            }
                            return;
                        }
                        if (bottomleft != null)
                        {
                            frame = 4;
                            return;
                        }
                    }
                    frame = 45;
                }
                else
                {
                    frame = 50;
                }
            }
            else if (left != null)
            {
                if (topleft != null)
                {
                    frame = 28;
                }
                else
                {
                    frame = 60;
                }
            }
            else
            {
                frame = 44;
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
                        frame = 34;
                    }
                    else if (topleft != null)
                    {
                        if (topright != null)
                        {
                            frame = 3;
                        }
                        else if (bottomright != null)
                        {
                            if (bottomleft != null)
                            {
                                frame = 3;
                            }
                        }
                        else if (bottomleft != null)
                        {
                            frame = 6;
                        }
                        else
                        {
                            frame = 24;
                        }
                    }
                    else if (topright != null)
                    {
                        if (bottomright != null)
                        {
                            if (bottomleft != null)
                            {
                                frame = 3;
                            }
                            else
                            {
                                frame = 0;
                            }
                        }
                        else if (bottomleft == null)
                        {
                            frame = 25;
                        }
                    }
                    else if (bottomright != null)
                    {
                        if (bottomleft != null)
                        {
                            frame = 3;
                        }
                        else
                        {
                            frame = 8;
                        }
                    }
                    else if (bottomleft != null)
                    {
                        frame = 14;
                    }
                    else
                    {
                        frame = 34;
                    }
                }
                else if (topleft == null && topright != null && bottomleft != null && bottomright != null)
                {
                    frame = 1;
                }
                else if (bottomright != null)
                {
                    frame = 2;
                }
                else
                {
                    frame = 51;
                }
            }
            else if (left != null)
            {
                if ((bottomleft != null || topleft != null) && (topright != null || bottomright != null))
                {
                    frame = 59;
                }
                else if (bottomright != null || topright != null)
                {
                    frame = 33;
                }
                else if (bottomleft != null || topleft != null)
                {
                    frame = 35;
                }
                else
                {
                    frame = 36;
                }
            }
            else if (bottomright != null || topright != null)
            {
                frame = 41;
            }
            else
            {
                frame = 32;
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
                                frame = 5;
                            }
                            else
                            {
                                frame = 4;
                            }
                        }
                        else
                        {
                            frame = 52;
                        }
                        return;
                    }
                }
                else if (topright == null && bottomleft != null)
                {
                    frame = 4;
                    return;
                }
                frame = 52;
            }
            else
            {
                frame = 49;
            }
        }
        else if (left != null)
        {
            if (bottomleft != null || topleft != null)
            {
                frame = 43;
            }
            else
            {
                frame = 37;
            }
        }
        else
        {
            frame = 40;
        }
    }

    public override ContextMenu GetContextMenu() =>
        null;
}