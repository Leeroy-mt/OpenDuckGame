namespace DuckGame;

/// <summary>
/// Represents a transformable component.
/// </summary>
public abstract class Transform
{
	public Vec2 position;

	protected float _z;

	protected Depth _depth = new Depth(0f);

	protected Vec2 _center;

	public float _angle;

	private Vec2 _scale = new Vec2(1f, 1f);

	private float _alpha = 1f;

	private float _alphaSub;

	public float x
	{
		get
		{
			return position.x;
		}
		set
		{
			position.x = value;
		}
	}

	public float y
	{
		get
		{
			return position.y;
		}
		set
		{
			position.y = value;
		}
	}

	public float z
	{
		get
		{
			return _z;
		}
		set
		{
			_z = value;
		}
	}

	public Depth depth
	{
		get
		{
			return _depth;
		}
		set
		{
			_depth = value;
		}
	}

	public virtual Vec2 center
	{
		get
		{
			return _center;
		}
		set
		{
			_center = value;
		}
	}

	public float centerx
	{
		get
		{
			return _center.x;
		}
		set
		{
			_center.x = value;
		}
	}

	public float centery
	{
		get
		{
			return _center.y;
		}
		set
		{
			_center.y = value;
		}
	}

	public virtual float angle
	{
		get
		{
			return _angle;
		}
		set
		{
			_angle = value;
		}
	}

	public float angleDegrees
	{
		get
		{
			return Maths.RadToDeg(angle);
		}
		set
		{
			angle = Maths.DegToRad(value);
		}
	}

	public Vec2 scale
	{
		get
		{
			return _scale;
		}
		set
		{
			_scale = value;
		}
	}

	public float xscale
	{
		get
		{
			return _scale.x;
		}
		set
		{
			_scale.x = value;
		}
	}

	public float yscale
	{
		get
		{
			return _scale.y;
		}
		set
		{
			_scale.y = value;
		}
	}

	public float alpha
	{
		get
		{
			return _alpha;
		}
		set
		{
			_alpha = value;
		}
	}

	public float alphaSub
	{
		get
		{
			return _alphaSub;
		}
		set
		{
			_alphaSub = value;
		}
	}
}
