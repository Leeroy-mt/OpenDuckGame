using System;
using System.Collections.Generic;
using System.Linq;

namespace DuckGame;

public class EditorBeam : MaterialThing
{
	private Sprite _selectBeam;

	private float _spawnWait;

	private SinWave _wave = 0.016f;

	private SinWave _wave2 = 0.02f;

	private List<BeamDuck> _ducks = new List<BeamDuck>();

	private List<Thing> _guns = new List<Thing>();

	private float _beamHeight = 180f;

	private float _flash;

	private bool _leaveLeft;

	public bool entered;

	public EditorBeam(float xpos, float ypos)
		: base(xpos, ypos)
	{
		_selectBeam = new Sprite("selectBeam");
		_selectBeam.alpha = 0.9f;
		_selectBeam.depth = -0.8f;
		_selectBeam.center = new Vec2(_selectBeam.w / 2, 0f);
		base.depth = 0.5f;
		_collisionOffset = new Vec2(0f - (float)(_selectBeam.w / 2) * 0.8f, 0f);
		_collisionSize = new Vec2((float)_selectBeam.w * 0.8f, 180f);
		center = new Vec2(_selectBeam.w / 2);
		base.layer = Layer.Background;
		thickness = 10f;
	}

	public override void Initialize()
	{
		base.Initialize();
	}

	public override void Update()
	{
		_selectBeam.color = new Color(0.5f, 0.2f + _wave2.normalized * 0.2f, 0.3f + _wave.normalized * 0.3f) * (1f + _flash);
		_flash = Maths.CountDown(_flash, 0.1f);
		_spawnWait -= 0.025f;
		if (_spawnWait < 0f)
		{
			Level.Add(new MultiBeamParticle(base.x, base.y + 190f, -0.8f - _wave.normalized, inverse: false, Color.Cyan * 0.8f));
			Level.Add(new MultiBeamParticle(base.x, base.y + 190f, -0.8f - _wave2.normalized, inverse: true, Color.LightBlue * 0.8f));
			_spawnWait = 1f;
		}
		foreach (Duck d in Level.CheckRectAll<Duck>(position - center, position - center + new Vec2(_collisionSize.x, _collisionSize.y)))
		{
			if (!_ducks.Any((BeamDuck beamDuck) => beamDuck.duck == d))
			{
				float entry = 0f;
				entry = ((!(d.y < 100f)) ? 130f : 40f);
				SFX.Play("stepInBeam");
				d.immobilized = true;
				d.crouch = false;
				d.sliding = false;
				if (d.holdObject != null)
				{
					_guns.Add(d.holdObject);
				}
				d.ThrowItem();
				d.solid = false;
				d.grounded = false;
				_ducks.Add(new BeamDuck
				{
					duck = d,
					entryHeight = entry,
					leaving = false,
					entryDir = ((!(d.x < base.x)) ? 1 : (-1)),
					sin = new SinWave(0.1f),
					sin2 = new SinWave(0.05f)
				});
				entered = true;
			}
		}
		foreach (Holdable t in Level.CheckRectAll<Holdable>(position - center, position - center + new Vec2(_collisionSize.x, _collisionSize.y)))
		{
			if (t.owner == null && !_guns.Contains(t))
			{
				_guns.Add(t);
			}
		}
		int numDucks = _ducks.Count;
		int duckNum = 0;
		float topOffset = 150f;
		float span = (_beamHeight - topOffset * 2f) / (float)((numDucks <= 1) ? 1 : (numDucks - 1));
		for (int i = 0; i < _ducks.Count; i++)
		{
			BeamDuck d2 = _ducks[i];
			if (d2.leaving)
			{
				d2.duck.solid = true;
				d2.duck.hSpeed = (_leaveLeft ? (-4f) : 4f);
				d2.duck.vSpeed = 0f;
				if (Math.Abs(d2.duck.position.x - base.x) > 24f)
				{
					d2.duck.immobilized = false;
					_ducks.RemoveAt(i);
					i--;
					continue;
				}
			}
			else
			{
				d2.duck.position.x = Lerp.FloatSmooth(d2.duck.position.x, position.x + (float)d2.sin2 * 1f, 0.2f);
				d2.duck.position.y = Lerp.FloatSmooth(d2.duck.position.y, topOffset + span * (float)i + (float)d2.sin * 2f, 0.08f);
				d2.duck.vSpeed = 0f;
				d2.duck.hSpeed = 0f;
			}
			if (!TitleScreen.hasMenusOpen && d2.duck.inputProfile.Pressed("RIGHT"))
			{
				d2.leaving = true;
				_leaveLeft = false;
				d2.duck.offDir = 1;
				entered = false;
			}
			duckNum++;
		}
		for (int i2 = 0; i2 < _guns.Count; i2++)
		{
			Thing g = _guns[i2];
			g.vSpeed = 0f;
			g.hSpeed = 0f;
			if (Math.Abs(position.x - g.position.x) < 6f)
			{
				g.position = Vec2.Lerp(g.position, new Vec2(position.x, g.position.y - 3f), 0.1f);
				g.alpha = Maths.LerpTowards(g.alpha, 0f, 0.1f);
				if (g.alpha <= 0f)
				{
					g.y = -200f;
					_guns.RemoveAt(i2);
					i2--;
				}
			}
			else
			{
				g.position = Vec2.Lerp(g.position, new Vec2(position.x, g.position.y), 0.2f);
			}
		}
		base.Update();
	}

	public override bool Hit(Bullet bullet, Vec2 hitPos)
	{
		for (int i = 0; i < 6; i++)
		{
			Level.Add(new GlassParticle(hitPos.x, hitPos.y, new Vec2(Rando.Float(-1f, 1f), Rando.Float(-1f, 1f))));
		}
		_flash = 1f;
		return true;
	}

	public override void Draw()
	{
		base.Draw();
		_selectBeam.depth = base.depth;
		for (int i = 0; i < 6; i++)
		{
			Graphics.Draw(_selectBeam, base.x, base.y + (float)(i * 32));
		}
	}
}
