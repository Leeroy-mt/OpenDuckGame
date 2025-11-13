using System;
using System.Collections.Generic;

namespace DuckGame;

public abstract class MaterialThing : Thing
{
	public bool _ruined;

	protected NetSoundEffect _netCollideSound = new NetSoundEffect();

	public float buoyancy;

	public bool onlyFloatInLava;

	protected bool ignoreCollisions;

	public float thickness = -1f;

	public PhysicsMaterial physicsMaterial;

	private bool _destroyedReal;

	public bool cold;

	protected bool _translucent;

	protected float _maxHealth;

	public float _hitPoints;

	public bool destructive;

	public bool onlyCrush;

	protected bool _dontCrush;

	private HashSet<MaterialThing> _clip = new HashSet<MaterialThing>();

	private HashSet<MaterialThing> _impacting = new HashSet<MaterialThing>();

	private HashSet<MaterialThing> _solidImpacting = new HashSet<MaterialThing>();

	private byte _planeOfExistence = 4;

	public bool _didImpactSound;

	protected bool _grounded;

	protected float _bouncy;

	protected float _breakForce = 999f;

	protected float _impactThreshold = 0.5f;

	protected float _weight = 5f;

	public float spreadExtinguisherSmoke;

	private bool _islandDirty;

	private CollisionIsland _island;

	protected Thing _zapper;

	public bool willHeat;

	private Organizer<ImpactedFrom, string> _collideSounds = new Organizer<ImpactedFrom, string>();

	protected float _impactVolume = 0.5f;

	public float lastHSpeed;

	public float lastVSpeed;

	private Thing _lastBurnedBy;

	protected float _flameWait;

	public float burnSpeed = 0.002f;

	private int extWait;

	public float coolingFactor = 0.006f;

	protected bool _sendDestroyMessage;

	private bool _sentDestroyMessage;

	public float flammable;

	public float heat;

	public float burnt;

	protected bool _onFire;

	public ActionTimer _burnWaitTimer;

	public bool isBurnMessage;

	public bool superNonFlammable;

	public bool _destroyed
	{
		get
		{
			return _destroyedReal;
		}
		set
		{
			_destroyedReal = value;
		}
	}

	public bool translucent => _translucent;

	public bool dontCrush
	{
		get
		{
			return _dontCrush;
		}
		set
		{
			_dontCrush = value;
		}
	}

	public HashSet<MaterialThing> clip
	{
		get
		{
			return _clip;
		}
		set
		{
			_clip = value;
		}
	}

	public HashSet<MaterialThing> impacting
	{
		get
		{
			return _impacting;
		}
		set
		{
			_impacting = value;
		}
	}

	public HashSet<MaterialThing> solidImpacting
	{
		get
		{
			return _solidImpacting;
		}
		set
		{
			_solidImpacting = value;
		}
	}

	public byte planeOfExistence
	{
		get
		{
			return _planeOfExistence;
		}
		set
		{
			_planeOfExistence = value;
		}
	}

	public bool grounded
	{
		get
		{
			return _grounded;
		}
		set
		{
			_grounded = value;
		}
	}

	public float bouncy
	{
		get
		{
			return _bouncy;
		}
		set
		{
			_bouncy = value;
		}
	}

	public float breakForce
	{
		get
		{
			return _breakForce;
		}
		set
		{
			_breakForce = value;
		}
	}

	public virtual bool destroyed => _destroyed;

	public float impactThreshold
	{
		get
		{
			return _impactThreshold;
		}
		set
		{
			_impactThreshold = value;
		}
	}

	public virtual float weight
	{
		get
		{
			return _weight;
		}
		set
		{
			_weight = value;
		}
	}

	public float weightMultiplier
	{
		get
		{
			float w = weight;
			if (w < 5f)
			{
				w = 5f;
			}
			return 5f / w;
		}
	}

	public float weightMultiplierSmall
	{
		get
		{
			float w = weight * 0.75f;
			if (w < 5f)
			{
				w = 5f;
			}
			return 5f / w;
		}
	}

	public float weightMultiplierInv
	{
		get
		{
			float w = weight;
			if (w < 5f)
			{
				w = 5f;
			}
			return w / 5f;
		}
	}

	public float weightMultiplierInvTotal => weight / 5f;

	public bool islandDirty
	{
		get
		{
			return _islandDirty;
		}
		set
		{
			_islandDirty = value;
		}
	}

	public CollisionIsland island
	{
		get
		{
			return _island;
		}
		set
		{
			_island = value;
		}
	}

	public virtual float impactPowerH => Math.Abs(hSpeed) * weightMultiplierInvTotal;

	public virtual float impactPowerV => Math.Abs(vSpeed) * weightMultiplierInvTotal;

	public float impactDirectionH => hSpeed * weightMultiplierInvTotal;

	public float impactDirectionV => vSpeed * weightMultiplierInvTotal;

	public float totalImpactPower => impactPowerH + impactPowerV;

	public Vec2 impactDirection => new Vec2(impactDirectionH, impactDirectionV);

	public Organizer<ImpactedFrom, string> collideSounds
	{
		get
		{
			return _collideSounds;
		}
		set
		{
			_collideSounds = value;
		}
	}

	public float impactVolume
	{
		get
		{
			return _impactVolume;
		}
		set
		{
			_impactVolume = value;
		}
	}

	public Thing lastBurnedBy => _lastBurnedBy;

	public bool onFire
	{
		get
		{
			return _onFire;
		}
		set
		{
			_onFire = value;
		}
	}

	/// <summary>
	/// Called when the object hits a spring.
	/// </summary>
	/// <param name="pSpringer">The spring what which sprung</param>
	/// <returns>Return 'true' to continue running regular spring logic. 'False' to ignore it.</returns>
	public virtual bool Sprung(Thing pSpringer)
	{
		return true;
	}

	public static Vec2 ImpactVector(ImpactedFrom from)
	{
		return from switch
		{
			ImpactedFrom.Left => new Vec2(-1f, 0f), 
			ImpactedFrom.Right => new Vec2(1f, 0f), 
			ImpactedFrom.Top => new Vec2(0f, -1f), 
			_ => new Vec2(0f, 1f), 
		};
	}

	public virtual bool Hurt(float points)
	{
		if (_maxHealth == 0f)
		{
			return false;
		}
		_hitPoints -= points;
		return true;
	}

	public virtual void Zap(Thing zapper)
	{
		_zapper = zapper;
	}

	public void CheckIsland()
	{
		if (island != null && island.owner != this && base.level != null && base.level.simulatePhysics && (position - island.owner.position).lengthSq > island.radiusSquared)
		{
			island.RemoveThing(this);
			UpdateIsland();
		}
	}

	public void UpdateIsland()
	{
		CollisionIsland isle = Level.current.things.GetIsland(position);
		if (island != null && island != isle)
		{
			island.RemoveThing(this);
		}
		if (isle != null)
		{
			if (island != isle)
			{
				isle.AddThing(this);
			}
		}
		else
		{
			Level.current.things.AddIsland(this);
		}
		islandDirty = false;
	}

	public override void DoInitialize()
	{
		base.DoInitialize();
	}

	public override void DoTerminate()
	{
		if (island != null)
		{
			island.RemoveThing(this);
		}
		base.DoTerminate();
	}

	public MaterialThing(float x, float y)
		: base(x, y)
	{
	}

	public void Bounce(ImpactedFrom direction)
	{
		if (direction == ImpactedFrom.Left || direction == ImpactedFrom.Right)
		{
			BounceH();
		}
		else
		{
			BounceV();
		}
	}

	public void BounceH()
	{
		hSpeed = (0f - hSpeed) * bouncy;
	}

	public void BounceV()
	{
		vSpeed = (0f - vSpeed) * bouncy;
	}

	public override void Update()
	{
		_didImpactSound = false;
	}

	public virtual void UpdateOnFire()
	{
		if (burnt < 1f)
		{
			if (_flameWait > 1f)
			{
				AddFire();
				_flameWait = 0f;
			}
			_flameWait += 0.03f;
			burnt += burnSpeed;
		}
		else
		{
			Destroy(new DTIncinerate(_lastBurnedBy));
		}
	}

	public override void DoUpdate()
	{
		if (spreadExtinguisherSmoke > 0f)
		{
			spreadExtinguisherSmoke -= 0.15f;
			if (Math.Abs(hSpeed) + Math.Abs(vSpeed) > 2f)
			{
				extWait++;
				if (extWait >= 3)
				{
					JetpackSmoke s = new JetpackSmoke(base.x + Rando.Float(-1f, 1f), base.bottom + Rando.Float(-4f, 1f));
					s.depth = 0.9f;
					Level.current.AddThing(s);
					s.hSpeed += hSpeed * Rando.Float(0.2f, 0.3f);
					s.vSpeed = Rando.Float(-0.1f, 0f);
					s.vSpeed -= Math.Abs(vSpeed) * Rando.Float(0.05f, 0.1f);
					extWait = 0;
				}
			}
		}
		if (heat > 0f)
		{
			heat -= coolingFactor;
		}
		else if (heat < -0.01f)
		{
			heat += coolingFactor;
		}
		else
		{
			heat = 0f;
		}
		if (base.isServerForObject && _onFire)
		{
			UpdateOnFire();
		}
		base.DoUpdate();
	}

	public void NetworkDestroy()
	{
		OnDestroy(new DTImpact(this));
	}

	public virtual bool Destroy(DestroyType type = null)
	{
		if (!_destroyed)
		{
			_destroyed = OnDestroy(type);
			if (base.isServerForObject && (_destroyed || (_sendDestroyMessage && !_sentDestroyMessage)) && base.isStateObject)
			{
				Send.Message(new NMDestroyProp(this));
				_sentDestroyMessage = true;
			}
		}
		return _destroyed;
	}

	protected virtual bool OnDestroy(DestroyType type = null)
	{
		return false;
	}

	public virtual void Regenerate()
	{
		_destroyed = false;
	}

	public virtual bool DoHit(Bullet bullet, Vec2 hitPos)
	{
		return Hit(bullet, hitPos);
	}

	public virtual bool Hit(Bullet bullet, Vec2 hitPos)
	{
		if (physicsMaterial == PhysicsMaterial.Metal)
		{
			Level.Add(MetalRebound.New(hitPos.x, hitPos.y, (bullet.travelDirNormalized.x > 0f) ? 1 : (-1)));
			hitPos -= bullet.travelDirNormalized;
			for (int i = 0; i < 3; i++)
			{
				Level.Add(Spark.New(hitPos.x, hitPos.y, bullet.travelDirNormalized));
			}
		}
		else if (physicsMaterial == PhysicsMaterial.Wood)
		{
			hitPos -= bullet.travelDirNormalized;
			for (int j = 0; j < 3; j++)
			{
				WoodDebris woodDebris = WoodDebris.New(hitPos.x, hitPos.y);
				woodDebris.hSpeed = 0f - bullet.travelDirNormalized.x + Rando.Float(-1f, 1f);
				woodDebris.vSpeed = 0f - bullet.travelDirNormalized.y + Rando.Float(-1f, 1f);
				Level.Add(woodDebris);
			}
		}
		return thickness > bullet.ammo.penetration;
	}

	public virtual void DoExitHit(Bullet bullet, Vec2 exitPos)
	{
		ExitHit(bullet, exitPos);
	}

	public virtual void ExitHit(Bullet bullet, Vec2 exitPos)
	{
	}

	public virtual void Burn(Vec2 firePosition, Thing litBy)
	{
		if (Network.isActive && !base.isServerForObject && !isBurnMessage && !_onFire && this is Duck && (this as Duck).profile != null)
		{
			Send.Message(new NMLightDuck(this as Duck));
		}
		if ((base.isServerForObject || isBurnMessage) && !_onFire && (_burnWaitTimer == null || (bool)_burnWaitTimer))
		{
			_ = onFire;
			_onFire = OnBurn(firePosition, litBy);
			_lastBurnedBy = litBy;
		}
	}

	public virtual void UpdateFirePosition(SmallFire f)
	{
	}

	public virtual void AddFire()
	{
		Level.Add(SmallFire.New(Rando.Float((base.left - base.x) * 0.7f, (base.right - base.x) * 0.7f), Rando.Float((base.top - base.y) * 0.7f, (base.bottom - base.y) * 0.7f), 0f, 0f, shortLife: false, this));
	}

	protected virtual bool OnBurn(Vec2 firePosition, Thing litBy)
	{
		if (flammable < 0.001f)
		{
			return false;
		}
		if (!_onFire)
		{
			SFX.Play("ignite", 0.7f, -0.3f + Rando.Float(0.3f));
		}
		AddFire();
		AddFire();
		return true;
	}

	public virtual void DoHeatUp(float val, Vec2 location)
	{
		bool hadNegativeHeat = heat < 0f;
		if (!hadNegativeHeat || val > 0f)
		{
			heat += val;
			if (heat > 1.5f)
			{
				heat = 1.5f;
			}
			if (!hadNegativeHeat && heat < 0f)
			{
				heat = 0f;
			}
		}
		if (val > 0f)
		{
			HeatUp(location);
		}
	}

	public virtual void DoHeatUp(float val)
	{
		DoHeatUp(val, position);
	}

	public virtual void HeatUp(Vec2 location)
	{
	}

	public virtual void DoFreeze(float val, Vec2 location)
	{
		if (val < 0f)
		{
			val = 0f - val;
		}
		heat -= val;
		if (heat < -1.5f)
		{
			heat = -1.5f;
		}
		Freeze(location);
	}

	public virtual void DoFreeze(float val)
	{
		DoFreeze(val, position);
	}

	public virtual void Freeze(Vec2 location)
	{
	}

	public virtual void Extinquish()
	{
		foreach (SmallFire f in Level.CheckCircleAll<SmallFire>(position, 20f))
		{
			if (f.stick == this)
			{
				Level.Remove(f);
			}
		}
		_onFire = false;
		_burnWaitTimer = new ActionTimer(0.05f, 1f, reset: false);
	}

	protected float CalculateImpactPower(MaterialThing with, ImpactedFrom from)
	{
		float totalPower = 0f;
		if (from == ImpactedFrom.Left || from == ImpactedFrom.Right)
		{
			return impactPowerH + with.impactPowerH;
		}
		return impactPowerV + with.impactPowerV;
	}

	protected virtual float CalculatePersonalImpactPower(MaterialThing with, ImpactedFrom from)
	{
		float totalPower = 0f;
		totalPower = ((from != ImpactedFrom.Left && from != ImpactedFrom.Right) ? impactPowerV : impactPowerH);
		return Math.Abs(totalPower);
	}

	public virtual void Impact(MaterialThing with, ImpactedFrom from, bool solidImpact)
	{
		if (!with.ignoreCollisions && !ignoreCollisions && CalculateImpactPower(with, from) > _impactThreshold)
		{
			if (!with.onlyCrush || from == ImpactedFrom.Top)
			{
				OnSoftImpact(with, from);
			}
			OnImpact(with, from);
		}
	}

	public virtual void Touch(MaterialThing with)
	{
	}

	public virtual void SolidImpact(MaterialThing with, ImpactedFrom from)
	{
		if (!with.ignoreCollisions && !ignoreCollisions)
		{
			float num = CalculateImpactPower(with, from);
			if (num > _breakForce)
			{
				Destroy(new DTImpact(with));
			}
			if (CalculatePersonalImpactPower(with, from) > _impactThreshold)
			{
				_didImpactSound = PlayCollideSound(from);
			}
			if (num > _impactThreshold)
			{
				OnSolidImpact(with, from);
				OnImpact(with, from);
			}
		}
	}

	public virtual bool PlayCollideSound(ImpactedFrom from)
	{
		if (collideSounds.HasGroup(from) && !_didImpactSound)
		{
			if (Network.isActive)
			{
				if (base.isServerForObject)
				{
					_netCollideSound.Play();
				}
			}
			else
			{
				SFX.Play(collideSounds.GetRandom(from), _impactVolume);
			}
			return true;
		}
		return false;
	}

	public virtual void OnImpact(MaterialThing with, ImpactedFrom from)
	{
	}

	public virtual void OnSoftImpact(MaterialThing with, ImpactedFrom from)
	{
	}

	public virtual void OnSolidImpact(MaterialThing with, ImpactedFrom from)
	{
	}

	public virtual void DrawGlow()
	{
	}
}
