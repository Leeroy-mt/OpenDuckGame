using System;
using System.Collections.Generic;
using System.Linq;

namespace DuckGame;

public class Bullet : Thing
{
	private new NetworkConnection _connection;

	protected Teleporter _teleporter;

	public AmmoType ammo;

	public bool randomDir;

	public Vec2 start;

	public byte bulletIndex;

	public bool hitArmor;

	private Vec2 _realEnd;

	public Vec2 travelStart;

	public Vec2 travelEnd;

	public Vec2 travel;

	public Vec2 willCol;

	public bool create = true;

	public bool col;

	public bool traced;

	public Thing lastReboundSource;

	public bool rebound;

	protected bool _tracer;

	private bool _tracePhase;

	private bool _gravityAffected;

	public float gravityMultiplier = 1f;

	private float _travelTime;

	protected float _bulletDistance;

	protected float _bulletLength = 100f;

	protected float _bulletSpeed = 28f;

	public Vec2 _actualStart;

	private bool _didPenetrate;

	public Color color = Color.White;

	protected Thing _firedFrom;

	protected Profile _contributeToAccuracy;

	public static bool isRebound = false;

	public float range;

	private PhysicalBullet _physicalBullet;

	public bool trickshot;

	protected int timesRebounded;

	protected int reboundBulletsCreated;

	private Bullet _reboundedBullet;

	public bool reboundCalled;

	public Vec2 travelDirNormalized;

	public bool reboundOnce;

	private int _totalSteps;

	public List<MaterialThing> _currentlyImpacting = new List<MaterialThing>();

	private static List<MaterialThing> bulletImpactList = new List<MaterialThing>();

	private int hitsLogged;

	public Vec2 currentTravel;

	public bool renderedGhost;

	public float _totalLength;

	public Vec2 drawStart;

	protected Vec2 drawEnd;

	protected List<Vec2> prev = new List<Vec2>();

	protected List<float> vel = new List<float>();

	protected float _totalArc;

	protected bool doneTravelling;

	protected float startpoint;

	protected float drawdist;

	protected bool _initializedDraw;

	private byte networkKillWait = 60;

	public new NetworkConnection connection
	{
		get
		{
			return _connection;
		}
		set
		{
			_connection = value;
		}
	}

	public Vec2 end
	{
		get
		{
			return _realEnd;
		}
		set
		{
			_realEnd = value;
		}
	}

	public bool gravityAffected
	{
		get
		{
			return _gravityAffected;
		}
		set
		{
			_gravityAffected = value;
		}
	}

	public float travelTime => _travelTime;

	public float bulletDistance => _bulletDistance;

	public float bulletSpeed => _bulletSpeed;

	public bool didPenetrate => _didPenetrate;

	public Thing firedFrom
	{
		get
		{
			return _firedFrom;
		}
		set
		{
			_firedFrom = value;
		}
	}

	public Profile contributeToAccuracy
	{
		get
		{
			return _contributeToAccuracy;
		}
		set
		{
			_contributeToAccuracy = value;
		}
	}

	public Bullet(float xval, float yval, AmmoType type, float ang = -1f, Thing owner = null, bool rbound = false, float distance = -1f, bool tracer = false, bool network = true)
	{
		_gravityAffected = type.affectedByGravity;
		gravityMultiplier = type.gravityMultiplier;
		_bulletLength = type.bulletLength;
		base.depth = -0.1f;
		if (owner is Duck && (Math.Abs((owner as Duck).holdAngle) > 0.1f || ((owner as Duck).holdObject is Gun && Math.Abs(((owner as Duck).holdObject as Gun).angleDegrees) > 20f && !_gravityAffected)))
		{
			trickshot = true;
		}
		if (!tracer)
		{
			_tracePhase = true;
			if (owner != null && owner is Duck d)
			{
				_contributeToAccuracy = d.profile;
				if (Highlights.highlightRatingMultiplier != 0f)
				{
					d.profile.stats.bulletsFired++;
				}
			}
		}
		base.x = xval;
		base.y = yval;
		ammo = type;
		rebound = rbound;
		_owner = owner;
		angle = ang;
		_tracer = tracer;
		range = type.range - Rando.Float(type.rangeVariation);
		if (distance > 0f)
		{
			range = distance;
		}
		_bulletSpeed = type.bulletSpeed + Rando.Float(type.speedVariation);
		if (!traced)
		{
			if (randomDir)
			{
				angle = Rando.Float(360f);
			}
			float nxt = Rando.Float(30f);
			angle += (-15f + nxt) * (1f - ammo.accuracy);
			travel.x = (float)Math.Cos(Maths.DegToRad(angle)) * range;
			travel.y = (float)(0.0 - Math.Sin(Maths.DegToRad(angle))) * range;
			start = new Vec2(base.x, base.y);
			_actualStart = start;
			end = start + travel;
			travelDirNormalized = end - start;
			travelDirNormalized.Normalize();
			if (_gravityAffected)
			{
				hSpeed = travelDirNormalized.x * _bulletSpeed;
				vSpeed = travelDirNormalized.y * _bulletSpeed;
				_physicalBullet = new PhysicalBullet();
				_physicalBullet.bullet = this;
				_physicalBullet.weight = ammo.weight;
			}
			if (_tracer)
			{
				TravelBullet();
			}
			else
			{
				travelStart = start;
				travelEnd = end;
				_totalLength = (end - start).length;
				_tracePhase = false;
			}
			traced = true;
		}
		if (!PewPewLaser.inFire)
		{
			_ = travelDirNormalized.x;
			_ = 0f;
		}
	}

	public Bullet ReverseTravel()
	{
		reboundBulletsCreated++;
		Vec2 incidence = travelDirNormalized;
		Vec2 normal = new Vec2(-Math.Sign(travelDirNormalized.x), 0f);
		Vec2 reflection = incidence - normal * 2f * Vec2.Dot(incidence, normal);
		float newAngle = Maths.PointDirection(Vec2.Zero, reflection);
		float travelLen = (_actualStart - start).length;
		if (travelLen > 2f)
		{
			float newLength = _totalLength - travelLen;
			Rebound(start, newAngle, newLength);
			end = start;
			travelEnd = end;
			doneTravelling = true;
			position = start;
			drawStart = start;
			travelDirNormalized = Vec2.Zero;
			OnHit(destroyed: true);
		}
		return _reboundedBullet;
	}

	public virtual void DoRebound(Vec2 pos, float dir, float rng)
	{
		Rebound(pos, dir, rng);
	}

	protected virtual void Rebound(Vec2 pos, float dir, float rng)
	{
		reboundBulletsCreated++;
		Bullet bullet = ammo.GetBullet(pos.x, pos.y, null, 0f - dir, firedFrom, rng, _tracer);
		bullet._teleporter = _teleporter;
		bullet.timesRebounded = timesRebounded + 1;
		bullet.lastReboundSource = lastReboundSource;
		bullet.isLocal = isLocal;
		_reboundedBullet = bullet;
		reboundCalled = true;
		Level.Add(bullet);
	}

	public virtual void OnCollide(Vec2 pos, Thing t, bool willBeStopped)
	{
	}

	protected virtual bool RaycastBullet(Vec2 p1, Vec2 p2, Vec2 dir, float length, List<MaterialThing> collideList)
	{
		int steps = (int)Math.Ceiling(length);
		currentTravel = p1;
		_ = Vec2.Zero;
		bool stopOnHit = false;
		reboundCalled = false;
		do
		{
			bulletImpactList.Clear();
			steps--;
			_totalSteps--;
			Level.current.CollisionBullet(currentTravel, bulletImpactList);
			if (!_tracer)
			{
				for (int i = 0; i < _currentlyImpacting.Count; i++)
				{
					MaterialThing t = _currentlyImpacting[i];
					if (!bulletImpactList.Contains(t))
					{
						if (ammo.deadly)
						{
							t.DoExitHit(this, currentTravel);
						}
						_currentlyImpacting.RemoveAt(i);
						i--;
					}
				}
			}
			bool checkingDucks = false;
			Duck duckOwner = _owner as Duck;
			for (int j = 0; j < 2; j++)
			{
				checkingDucks = j == 1;
				for (int k = 0; k < bulletImpactList.Count; k++)
				{
					MaterialThing thing = bulletImpactList[k];
					if (checkingDucks != thing is IAmADuck || ((thing == _owner || (_owner is Duck && (_owner as Duck).ExtendsTo(thing))) && !ammo.immediatelyDeadly) || (duckOwner != null && thing == duckOwner.holdObject) || thing == _teleporter || (thing is Teleporter && (_tracer || !ammo.canTeleport)) || (ammo.ownerSafety > 0 && _travelTime / Maths.IncFrameTimer() < (float)ammo.ownerSafety && firedFrom != null && thing == firedFrom.owner))
					{
						continue;
					}
					bool stopHit = false;
					if (DevConsole.shieldMode && thing is Duck && (thing as Duck)._shieldCharge > 0.6f)
					{
						stopHit = true;
						stopOnHit = true;
					}
					if (thing is Duck && !_tracer && _contributeToAccuracy != null)
					{
						if (Highlights.highlightRatingMultiplier != 0f)
						{
							_contributeToAccuracy.stats.bulletsThatHit++;
						}
						_contributeToAccuracy = null;
					}
					if (!stopHit && thing.thickness >= 0f && !_currentlyImpacting.Contains(thing))
					{
						if (!_tracer && !_tracePhase)
						{
							if (ammo.deadly)
							{
								stopOnHit = thing.DoHit(this, currentTravel);
								if (thing is Duck && (thing as Duck).dead && !(ammo is ATShrapnel) && trickshot)
								{
									Global.data.angleShots++;
									if (_owner != null && _owner is Duck && (_owner as Duck).profile != null)
									{
										(_owner as Duck).profile.stats.trickShots++;
									}
								}
							}
							else if (_physicalBullet != null)
							{
								ImpactedFrom from = ImpactedFrom.Top;
								from = ((!(currentTravel.y >= thing.top + 1f) || !(currentTravel.y <= thing.bottom - 1f)) ? ((travelDirNormalized.y > 0f) ? ImpactedFrom.Top : ImpactedFrom.Bottom) : ((!(travelDirNormalized.x > 0f)) ? ImpactedFrom.Right : ImpactedFrom.Left));
								_physicalBullet.position = currentTravel;
								_physicalBullet.velocity = base.velocity;
								if (thing is Block || (thing is IPlatform && travelDirNormalized.y > 0f))
								{
									thing.SolidImpact(_physicalBullet, from);
								}
								else if (thing.thickness > ammo.penetration)
								{
									thing.Impact(_physicalBullet, from, solidImpact: false);
								}
								base.velocity = _physicalBullet.velocity;
								stopOnHit = thing.thickness > ammo.penetration;
							}
							else
							{
								stopOnHit = thing.thickness > ammo.penetration;
							}
							if (Recorder.currentRecording != null && hitsLogged < 1)
							{
								Recorder.currentRecording.LogAction();
								hitsLogged++;
							}
						}
						else
						{
							stopOnHit = thing.thickness > ammo.penetration;
						}
						OnCollide(currentTravel, thing, stopOnHit);
						_currentlyImpacting.Add(thing);
						if (thing.thickness > 1.5f && ammo.penetration >= thing.thickness)
						{
							_didPenetrate = true;
							position = currentTravel;
							if (isLocal)
							{
								OnHit(destroyed: false);
							}
						}
					}
					bool didRebound = reboundCalled;
					if (!stopOnHit)
					{
						continue;
					}
					stopOnHit = true;
					if (thing is Teleporter)
					{
						_teleporter = thing as Teleporter;
						if (_teleporter.link != null)
						{
							float newLength = _totalLength - (_actualStart - currentTravel).length;
							if (newLength > 0f)
							{
								float newAngle = Maths.PointDirection(_actualStart, currentTravel);
								if ((int)_teleporter.teleHeight == 2 && (int)_teleporter._link.teleHeight == 2)
								{
									Vec2 offset = _teleporter.position - currentTravel;
									_teleporter = _teleporter.link;
									Rebound(_teleporter.position - offset, newAngle, newLength);
								}
								else
								{
									Vec2 offset2 = currentTravel;
									if (_teleporter._dir.y == 0f)
									{
										offset2.x = _teleporter._link.x - (_teleporter.x - currentTravel.x) + travelDirNormalized.x;
									}
									else if (_teleporter._dir.x == 0f)
									{
										offset2.y = _teleporter._link.y - (_teleporter.y - currentTravel.y) + travelDirNormalized.y;
									}
									if ((bool)_teleporter._link.horizontal)
									{
										if (offset2.x < _teleporter._link.left + 2f)
										{
											offset2.x = _teleporter._link.left + 2f;
										}
										if (offset2.x > _teleporter._link.right - 2f)
										{
											offset2.x = _teleporter._link.right - 2f;
										}
									}
									else
									{
										if (offset2.y < _teleporter._link.top + 2f)
										{
											offset2.y = _teleporter._link.top + 2f;
										}
										if (offset2.y > _teleporter._link.bottom - 2f)
										{
											offset2.y = _teleporter._link.bottom - 2f;
										}
									}
									_teleporter = _teleporter.link;
									Rebound(offset2, newAngle, newLength);
								}
							}
							didRebound = true;
						}
					}
					else if (!didRebound && ((rebound && (!ammo.softRebound || thing.physicsMaterial != PhysicsMaterial.Wood) && thing is Block) || reboundOnce))
					{
						float travelLen = (_actualStart - currentTravel).length;
						if (travelLen > 2f)
						{
							float newLength2 = _totalLength - travelLen;
							if (newLength2 > 0f)
							{
								Vec2 normal = Vec2.Zero;
								Vec2 startPos = currentTravel;
								Vec2 oneBack = currentTravel - travelDirNormalized;
								float dif = 0f;
								float minDif = 999.9f;
								if (currentTravel.y >= thing.top && oneBack.y < thing.top)
								{
									dif = Math.Abs(currentTravel.y - oneBack.y);
									if (dif < minDif)
									{
										normal = new Vec2(0f, -1f);
										startPos = new Vec2(currentTravel.x, thing.top - 1f);
										minDif = dif;
									}
								}
								if (currentTravel.y <= thing.bottom && oneBack.y > thing.bottom)
								{
									dif = Math.Abs(currentTravel.y - oneBack.y);
									if (dif < minDif)
									{
										normal = new Vec2(0f, 1f);
										startPos = new Vec2(currentTravel.x, thing.bottom + 1f);
										minDif = dif;
									}
								}
								if (currentTravel.x >= thing.left && oneBack.x < thing.left)
								{
									dif = Math.Abs(currentTravel.x - oneBack.x);
									if (dif < minDif)
									{
										normal = new Vec2(1f, 0f);
										startPos = new Vec2(thing.left - 1f, currentTravel.y);
										minDif = dif;
									}
								}
								if (currentTravel.x <= thing.right && oneBack.x > thing.right)
								{
									dif = Math.Abs(currentTravel.x - oneBack.x);
									if (dif < minDif)
									{
										normal = new Vec2(-1f, 0f);
										startPos = new Vec2(thing.right + 1f, currentTravel.y);
										minDif = dif;
									}
								}
								if (normal == Vec2.Zero)
								{
									normal = new Vec2(0f, -1f);
									startPos = new Vec2(currentTravel.x, thing.top - 1f);
									minDif = dif;
								}
								Vec2 incidence = travelDirNormalized;
								Vec2 reflection = incidence - normal * 2f * Vec2.Dot(incidence, normal);
								lastReboundSource = thing;
								if (reboundOnce)
								{
									startPos += reflection.normalized * 3f;
								}
								float newAngle2 = Maths.PointDirection(Vec2.Zero, reflection);
								Rebound(startPos, newAngle2, newLength2);
							}
							didRebound = true;
						}
						else
						{
							stopOnHit = false;
						}
						reboundOnce = false;
					}
					end = currentTravel;
					travelEnd = end;
					doneTravelling = true;
					position = currentTravel;
					OnHit(!didRebound);
					if (hitArmor)
					{
						j = 1;
					}
					break;
				}
			}
			_ = currentTravel;
			currentTravel += travelDirNormalized;
		}
		while (!(steps <= 0 || stopOnHit));
		return stopOnHit;
	}

	protected virtual void OnHit(bool destroyed)
	{
		ammo.OnHit(destroyed, this);
	}

	protected virtual void CheckTravelPath(Vec2 pStart, Vec2 pEnd)
	{
	}

	private void TravelBullet()
	{
		travelDirNormalized = end - start;
		if (travelDirNormalized.x == float.NaN || travelDirNormalized.y == float.NaN)
		{
			travelDirNormalized = Vec2.One;
			return;
		}
		float length = travelDirNormalized.length;
		if (length <= 0.001f)
		{
			return;
		}
		travelDirNormalized.Normalize();
		_totalSteps = (int)Math.Ceiling(length);
		List<MaterialThing> collideList = new List<MaterialThing>();
		Stack<TravelInfo> tests = new Stack<TravelInfo>();
		CheckTravelPath(start, end);
		tests.Push(new TravelInfo(start, end, length));
		float lengthDiv2 = 0f;
		int loops = 0;
		while (tests.Count > 0 && loops < 128)
		{
			loops++;
			TravelInfo current = tests.Pop();
			if (Level.current.CollisionLine<MaterialThing>(current.p1, current.p2) == null)
			{
				continue;
			}
			if (current.length < 8f)
			{
				if (RaycastBullet(current.p1, current.p2, travelDirNormalized, current.length, collideList))
				{
					break;
				}
			}
			else
			{
				lengthDiv2 = current.length * 0.5f;
				Vec2 halfPoint = current.p1 + travelDirNormalized * lengthDiv2;
				tests.Push(new TravelInfo(halfPoint, current.p2, lengthDiv2));
				tests.Push(new TravelInfo(current.p1, halfPoint, lengthDiv2));
			}
		}
	}

	public override void Update()
	{
		if (_tracer)
		{
			Level.Remove(this);
		}
		if (!_initializedDraw)
		{
			prev.Add(start);
			vel.Add(0f);
			_initializedDraw = true;
		}
		_travelTime += Maths.IncFrameTimer();
		_bulletDistance += _bulletSpeed;
		startpoint = Maths.Clamp(_bulletDistance - _bulletLength, 0f, 99999f);
		float dist = _bulletDistance;
		if (_gravityAffected)
		{
			end = start + base.velocity;
			vSpeed += PhysicsObject.gravity * gravityMultiplier;
			hSpeed *= ammo.airFrictionMultiplier;
			if (vSpeed > 8f)
			{
				vSpeed = 8f;
			}
			if (!doneTravelling)
			{
				prev.Add(end);
				float newVel = (end - start).length;
				_totalArc += newVel;
				vel.Add(newVel);
			}
		}
		else
		{
			end = start + travelDirNormalized * _bulletSpeed;
		}
		if (!doneTravelling)
		{
			TravelBullet();
			_totalLength = (travelStart - travelEnd).length;
			if (_bulletDistance >= _totalLength)
			{
				doneTravelling = true;
				travelEnd = end;
				_totalLength = (travelStart - end).length;
			}
			if (_gravityAffected && doneTravelling)
			{
				prev[prev.Count - 1] = travelEnd;
				float newVel2 = (travelEnd - start).length;
				_totalArc += newVel2;
				vel[vel.Count - 1] = newVel2;
			}
		}
		else
		{
			base.alpha -= 0.1f;
			if (base.alpha <= 0f)
			{
				Level.Remove(this);
			}
		}
		start = end;
		if (dist > _totalLength)
		{
			dist = _totalLength;
		}
		if (startpoint > dist)
		{
			startpoint = dist;
		}
		drawStart = travelStart + travelDirNormalized * startpoint;
		drawEnd = travelStart + travelDirNormalized * dist;
		drawdist = dist;
	}

	public Vec2 GetPointOnArc(float distanceBack)
	{
		float travelled = 0f;
		Vec2 curTrav = prev.Last();
		for (int i = prev.Count - 1; i > 0; i--)
		{
			if (i == 0)
			{
				return prev[i];
			}
			float prevTrav = travelled;
			travelled += vel[i];
			if (travelled >= distanceBack)
			{
				if (i == 1)
				{
					return prev[i - 1];
				}
				float dist = (distanceBack - prevTrav) / vel[i];
				return prev[i] + (prev[i - 1] - prev[i]) * dist;
			}
			curTrav = prev[i];
		}
		return curTrav;
	}

	public override void Draw()
	{
		if (_tracer || !(_bulletDistance > 0.1f))
		{
			return;
		}
		if (gravityAffected)
		{
			if (prev.Count < 1)
			{
				return;
			}
			int num = (int)Math.Ceiling((drawdist - startpoint) / 8f);
			Vec2 prevus = prev.Last();
			for (int i = 0; i < num; i++)
			{
				Vec2 cur = GetPointOnArc(i * 8);
				Graphics.DrawLine(cur, prevus, color * (1f - (float)i / (float)num) * base.alpha, ammo.bulletThickness, 0.9f);
				if (!(cur == prev.First()))
				{
					prevus = cur;
					if (i == 0 && ammo.sprite != null && !doneTravelling)
					{
						ammo.sprite.depth = 1f;
						ammo.sprite.angleDegrees = 0f - Maths.PointDirection(Vec2.Zero, travelDirNormalized);
						Graphics.Draw(ammo.sprite, prevus.x, prevus.y);
					}
					continue;
				}
				break;
			}
			return;
		}
		if (ammo.sprite != null && !doneTravelling)
		{
			ammo.sprite.depth = base.depth + 10;
			ammo.sprite.angleDegrees = 0f - Maths.PointDirection(Vec2.Zero, travelDirNormalized);
			Graphics.Draw(ammo.sprite, drawEnd.x, drawEnd.y);
		}
		float length = (drawStart - drawEnd).length;
		float dist = 0f;
		float incs = 1f / (length / 8f);
		float alph = 1f;
		float drawLength = 8f;
		while (true)
		{
			bool doBreak = false;
			if (dist + drawLength > length)
			{
				drawLength = length - Maths.Clamp(dist, 0f, 99f);
				doBreak = true;
			}
			alph -= incs;
			Graphics.currentDrawIndex--;
			Graphics.DrawLine(drawStart + travelDirNormalized * length - travelDirNormalized * dist, drawStart + travelDirNormalized * length - travelDirNormalized * (dist + drawLength), color * alph, ammo.bulletThickness, base.depth);
			if (!doBreak)
			{
				dist += 8f;
				continue;
			}
			break;
		}
	}
}
