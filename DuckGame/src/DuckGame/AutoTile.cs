namespace DuckGame;

public abstract class AutoTile : MaterialThing, IAutoTile, IDontMove, IPathNodeBlocker
{
	protected SpriteMap _sprite;

	protected Nubber _leftNub;

	protected Nubber _rightNub;

	private string _tileset;

	public float verticalWidth = 16f;

	public float verticalWidthThick = 16f;

	public float horizontalHeight = 16f;

	protected bool _hasNubs = true;

	protected bool _init50;

	private bool _neighborsInitialized;

	private AutoPlatform _leftBlock;

	private AutoPlatform _rightBlock;

	private AutoPlatform _upBlock;

	private AutoPlatform _downBlock;

	public AutoTile leftTile;

	public AutoTile rightTile;

	public AutoTile upTile;

	public AutoTile downTile;

	private bool _pathed;

	public bool needsRefresh;

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
			UpdateCollision();
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

	public AutoTile(float x, float y, string tileset)
		: base(x, y)
	{
		_sprite = new SpriteMap(tileset, 16, 16);
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

	public void InitializeNeighbors()
	{
		if (!_neighborsInitialized)
		{
			_leftBlock = Level.CheckPoint<AutoPlatform>(base.left - 2f, position.y, this);
			_rightBlock = Level.CheckPoint<AutoPlatform>(base.right + 2f, position.y, this);
			_upBlock = Level.CheckPoint<AutoPlatform>(position.x, base.top - 2f, this);
			_downBlock = Level.CheckPoint<AutoPlatform>(position.x, base.bottom + 2f, this);
			_neighborsInitialized = true;
		}
	}

	public override void EditorObjectsChanged()
	{
		PlaceBlock();
	}

	public bool HasNoCollision()
	{
		return false;
	}

	public override void Update()
	{
		if (needsRefresh)
		{
			PlaceBlock();
			needsRefresh = false;
		}
		if (!_placed)
		{
			PlaceBlock();
		}
		base.Update();
	}

	public override void Terminate()
	{
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

	public void FindFrame()
	{
		AutoTile left = null;
		AutoTile right = null;
		AutoTile up = null;
		AutoTile down = null;
		AutoTile topleft = null;
		AutoTile topright = null;
		AutoTile bottomleft = null;
		AutoTile bottomright = null;
		up = Level.CheckPoint<AutoTile>(base.x, base.y - 16f, this);
		down = Level.CheckPoint<AutoTile>(base.x, base.y + 16f, this);
		left = Level.CheckPoint<AutoTile>(base.x - 16f, base.y, this);
		right = Level.CheckPoint<AutoTile>(base.x + 16f, base.y, this);
		topleft = Level.CheckPoint<AutoTile>(base.x - 16f, base.y - 16f, this);
		topright = Level.CheckPoint<AutoTile>(base.x + 16f, base.y - 16f, this);
		bottomleft = Level.CheckPoint<AutoTile>(base.x - 16f, base.y + 16f, this);
		bottomright = Level.CheckPoint<AutoTile>(base.x + 16f, base.y + 16f, this);
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

	public override ContextMenu GetContextMenu()
	{
		return null;
	}
}
