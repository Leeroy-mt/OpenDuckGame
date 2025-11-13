using System;
using System.Collections.Generic;
using System.Linq;

namespace DuckGame;

[EditorGroup("Arcade|Targets", EditorItemType.ArcadeNew)]
[BaggedProperty("canSpawn", false)]
[BaggedProperty("isOnlineCapable", false)]
public class TargetDuckNew : TargetDuck
{
	protected bool _holdAction;

	public EditorProperty<int> Order = new EditorProperty<int>(0, null, -1f, 256f, 1f, "RANDOM");

	public EditorProperty<int> AutoFire = new EditorProperty<int>(-1, null, -1f, 240f, 2f, "INF");

	public EditorProperty<int> FireSpeed = new EditorProperty<int>(60, null, -1f, 240f, 2f);

	public EditorProperty<int> ReloadSpeed = new EditorProperty<int>(60, null, -1f, 240f, 2f);

	private float _reloadAdd;

	private float _targetLerpOut = 1f;

	public override bool action => _holdAction;

	public TargetDuckNew(float pX, float pY, TargetStance pStance)
		: base(pX, pY, pStance)
	{
		Order._tooltip = "All Targets/Goodies with smaller Order numbers must be destroyed/collected before this target appears.";
		AutoFire._tooltip = "How long the target waits (in frames) before auto firing it's weapon.";
		FireSpeed._tooltip = "How long the target waits (in frames) before firing it's weapon once it sees a target.";
		ReloadSpeed._tooltip = "How long the target waits (in frames) firing again once it's already fired.";
		_editorName = "Target Duck";
		_contextMenuFilter.Add("autofire");
		_contextMenuFilter.Add("time");
		_contextMenuFilter.Add("random");
		_contextMenuFilter.Add("maxrandom");
		_contextMenuFilter.Add("dropgun");
		_contextMenuFilter.Add("speediness");
		_contextMenuFilter.Add("Sequence");
		base.sequence._resetLikelyhood = false;
	}

	public override void OnSequenceActivate()
	{
		_popup = true;
		_waitFire = autofire.value;
		_reloadAdd = 0f;
		_holdAction = false;
	}

	public override void Initialize()
	{
		base.sequence.order = Order.value;
		base.Initialize();
		autofire = (float)AutoFire.value * Maths.IncFrameTimer();
		speediness = (float)FireSpeed.value * Maths.IncFrameTimer();
	}

	public override void UpdateFire()
	{
		if (!Level.current.simulatePhysics)
		{
			return;
		}
		Gun g = holdObject as Gun;
		float range = 300f;
		if (g.ammoType != null)
		{
			range = g.ammoType.range;
		}
		Vec2 at = holdObject.Offset(new Vec2(range * holdObject.angleMul, 0f));
		if (_waitFire <= 0f)
		{
			foreach (Duck d in Level.current.things[typeof(Duck)].Where((Thing thing) => !(thing is TargetDuck)))
			{
				if (!Collision.Line(holdObject.position + new Vec2(0f, -5f), at + new Vec2(0f, -5f), d.rectangle) && !Collision.Line(holdObject.position + new Vec2(0f, 5f), at + new Vec2(0f, 5f), d.rectangle))
				{
					continue;
				}
				IEnumerable<Block> enumerable = Level.CheckLineAll<Block>(holdObject.position, d.position);
				bool blocked = false;
				foreach (Block item in enumerable)
				{
					if (!(item is Window))
					{
						blocked = true;
						break;
					}
				}
				if (!blocked)
				{
					_waitFire = Math.Max((float)speediness + _reloadAdd, 0f);
				}
				break;
			}
		}
		_holdAction = false;
		if (_waitFire >= 0f)
		{
			_waitFire -= Maths.IncFrameTimer();
			if (_waitFire <= 0f)
			{
				g.PressAction();
				_holdAction = true;
				_reloadAdd = (float)ReloadSpeed.value * Maths.IncFrameTimer();
			}
		}
	}

	public override void Draw()
	{
		if (holdObject is Gun && (holdObject as Gun).ammoType != null && _waitFire < 1f && _waitFire > 0f)
		{
			float lerpval = _waitFire * _waitFire;
			Vec2 barrelPosition = (holdObject as Gun).barrelPosition;
			Vec2 topLine = barrelPosition + new Vec2(0f, (0f - lerpval) * 64f);
			Vec2 vec = barrelPosition + new Vec2(0f, lerpval * 64f);
			float wideInc = 1f - Math.Min(_waitFire, 0.08f) / 0.08f;
			Color c = Lerp.ColorSmooth(Color.White, Color.Red, wideInc);
			Graphics.DrawLine(topLine, topLine + new Vec2((holdObject as Gun).ammoType.range * (float)offDir, 0f), c * Math.Max(1f - _waitFire - 0.5f, 0f), 1f + wideInc, 0.99f);
			Graphics.DrawLine(vec, vec + new Vec2((holdObject as Gun).ammoType.range * (float)offDir, 0f), c * Math.Max(1f - _waitFire - 0.5f, 0f), 1f + wideInc, 0.99f);
		}
		base.Draw();
	}
}
