using System;

namespace DuckGame;

public class HUD
{
	private static HUDCore _core = new HUDCore();

	private const int CornerDisplayNextLineSeparation = 12;

	public static HUDCore core
	{
		get
		{
			return _core;
		}
		set
		{
			_core = value;
		}
	}

	public static bool hide
	{
		get
		{
			return _core._hide;
		}
		set
		{
			_core._hide = value;
		}
	}

	public static CornerDisplay FindDuplicateActiveCorner(HUDCorner corner, string text, bool allowStacking = false)
	{
		foreach (CornerDisplay d in _core._cornerDisplays)
		{
			if (d.corner == corner)
			{
				if (d.text == text && !d.closing)
				{
					return d;
				}
				break;
			}
		}
		if (!allowStacking)
		{
			CloseCorner(corner);
		}
		return null;
	}

	public static CornerDisplay AddCornerMessage(HUDCorner corner, string text)
	{
		return AddCornerMessage(corner, text, allowStacking: false);
	}

	public static CornerDisplay AddCornerMessage(HUDCorner corner, string text, bool allowStacking)
	{
		CornerDisplay d = null;
		d = FindDuplicateActiveCorner(corner, text, allowStacking);
		if (d == null)
		{
			d = new CornerDisplay();
			d.corner = corner;
			d.text = text;
			_core._cornerDisplays.Add(d);
		}
		if (!allowStacking)
		{
			foreach (CornerDisplay d2 in _core._cornerDisplays)
			{
				if (d2.corner == corner && d2 != d)
				{
					d2.closing = true;
				}
			}
		}
		return d;
	}

	public static CornerDisplay AddCornerControl(HUDCorner corner, string text, InputProfile pro)
	{
		return AddCornerControl(corner, text, pro, allowStacking: false);
	}

	public static CornerDisplay AddCornerControl(HUDCorner corner, string text, InputProfile pro = null, bool allowStacking = false)
	{
		CornerDisplay d = null;
		d = FindDuplicateActiveCorner(corner, text, allowStacking);
		if (d == null)
		{
			d = new CornerDisplay();
			d.corner = corner;
			d.text = text;
			d.isControl = true;
			d.profile = pro;
			_core._cornerDisplays.Add(d);
		}
		return d;
	}

	public static void AddInputChangeDisplay(string text)
	{
		_core._inputChangeDisplays.Clear();
		CornerDisplay d = new CornerDisplay();
		d.text = text;
		d.isControl = true;
		d.life = 3f;
		_core._inputChangeDisplays.Add(d);
	}

	public static void AddPlayerChangeDisplay(string text)
	{
		AddPlayerChangeDisplay(text, 4f);
	}

	public static void AddPlayerChangeDisplay(string text, float life)
	{
		_core._playerChangeDisplays.Clear();
		CornerDisplay d = new CornerDisplay();
		d.text = text;
		d.isControl = true;
		d.life = life;
		_core._playerChangeDisplays.Add(d);
	}

	public static void AddCornerTimer(HUDCorner corner, string text, Timer timer)
	{
		CornerDisplay d = new CornerDisplay();
		d.corner = corner;
		d.text = text;
		d.timer = timer;
		_core._cornerDisplays.Add(d);
	}

	public static void AddCornerCounter(HUDCorner corner, string text, FieldBinding counter, int max = 0, bool animateCount = false)
	{
		CornerDisplay d = new CornerDisplay();
		d.corner = corner;
		d.text = text;
		d.counter = counter;
		d.maxCount = max;
		d.animateCount = animateCount;
		d.curCount = (int)counter.value;
		d.realCount = (int)counter.value;
		_core._cornerDisplays.Add(d);
	}

	public static void ClearPlayerChangeDisplays()
	{
		_core._playerChangeDisplays.Clear();
	}

	public static void CloseAllCorners()
	{
		foreach (CornerDisplay cornerDisplay in _core._cornerDisplays)
		{
			cornerDisplay.closing = true;
		}
	}

	public static void CloseCorner(HUDCorner corner)
	{
		foreach (CornerDisplay d in _core._cornerDisplays)
		{
			if (d.corner == corner)
			{
				d.closing = true;
			}
		}
	}

	public static void CloseInputChangeDisplays()
	{
		foreach (CornerDisplay inputChangeDisplay in _core._inputChangeDisplays)
		{
			inputChangeDisplay.closing = true;
		}
	}

	public static void ClearCorners()
	{
		_core._cornerDisplays.Clear();
	}

	public static void Update()
	{
		for (int i = 0; i < _core._inputChangeDisplays.Count; i++)
		{
			CornerDisplay d = _core._inputChangeDisplays[i];
			if (d.closing)
			{
				d.slide = Lerp.FloatSmooth(d.slide, -0.3f, 0.15f);
				if (d.slide < -0.15f)
				{
					_core._inputChangeDisplays.RemoveAt(i);
					i--;
				}
			}
			else
			{
				d.life -= Maths.IncFrameTimer();
				d.slide = Lerp.FloatSmooth(d.slide, 1f, 0.15f, 1.2f);
				if (d.life <= 0f)
				{
					d.closing = true;
				}
			}
		}
		for (int j = 0; j < _core._playerChangeDisplays.Count; j++)
		{
			CornerDisplay d2 = _core._playerChangeDisplays[j];
			if (d2.closing)
			{
				d2.slide = Lerp.FloatSmooth(d2.slide, -0.3f, 0.15f);
				if (d2.slide < -0.15f)
				{
					_core._playerChangeDisplays.RemoveAt(j);
					j--;
				}
			}
			else
			{
				d2.life -= Maths.IncFrameTimer();
				d2.slide = Lerp.FloatSmooth(d2.slide, 1f, 0.15f, 1.2f);
				if (d2.life <= 0f)
				{
					d2.closing = true;
				}
			}
		}
		for (int k = 0; k < _core._cornerDisplays.Count; k++)
		{
			CornerDisplay d3 = _core._cornerDisplays[k];
			if (d3.closing)
			{
				d3.slide = Lerp.FloatSmooth(d3.slide, -0.3f, 0.15f);
				if (d3.slide < -0.15f)
				{
					_core._cornerDisplays.RemoveAt(k);
					k--;
				}
				continue;
			}
			if (d3.willDie)
			{
				d3.life -= Maths.IncFrameTimer();
				if (d3.life <= 0f)
				{
					d3.closing = true;
				}
			}
			if (_core._cornerDisplays.Exists((CornerDisplay v) => v.corner == d3.corner && v.closing))
			{
				continue;
			}
			if (d3.counter != null)
			{
				if (d3.addCount != 0)
				{
					d3.addCountWait -= 0.05f;
					if (d3.addCountWait <= 0f)
					{
						d3.addCountWait = 0.05f;
						if (d3.addCount > 0)
						{
							d3.addCount--;
							d3.curCount++;
						}
						else if (d3.addCount < 0)
						{
							d3.addCount++;
							d3.curCount--;
						}
						SFX.Play("tinyTick", 0.6f, 0.3f);
					}
				}
				int newVal = (int)d3.counter.value;
				if (newVal != d3.realCount)
				{
					if (d3.animateCount)
					{
						d3.addCountWait = 1f;
						d3.addCount = newVal - d3.realCount;
						d3.curCount = d3.realCount;
						d3.realCount = newVal;
					}
					else
					{
						d3.realCount = newVal;
						d3.curCount = newVal;
					}
				}
			}
			if (d3.timer != null && d3.timer.maxTime.TotalSeconds != 0.0 && (int)(d3.timer.maxTime - d3.timer.elapsed).TotalSeconds == d3.lowTimeTick)
			{
				d3.lowTimeTick--;
				SFX.Play("cameraBeep", 0.8f);
			}
			d3.slide = Lerp.FloatSmooth(d3.slide, 1f, 0.15f, 1.2f);
		}
	}

	public static void DrawForeground()
	{
		if (DevConsole.debugOrigin)
		{
			Graphics.DrawLine(new Vec2(0f, -32f), new Vec2(0f, 32f), Color.Orange);
			Graphics.DrawLine(new Vec2(-32f, 0f), new Vec2(32f, 0f), Color.Orange);
			Graphics.DrawRect(new Vec2(-2f, -2f), new Vec2(2f, 2f), Color.Red);
		}
		if (DevConsole.debugBounds && Level.current != null)
		{
			Graphics.DrawLine(Level.current.topLeft, new Vec2(Level.current.bottomRight.x, Level.current.topLeft.y), Color.Green);
			Graphics.DrawLine(Level.current.topLeft, new Vec2(Level.current.topLeft.x, Level.current.bottomRight.y), Color.Green);
			Graphics.DrawLine(Level.current.bottomRight, new Vec2(Level.current.topLeft.x, Level.current.bottomRight.y), Color.Green);
			Graphics.DrawLine(Level.current.bottomRight, new Vec2(Level.current.bottomRight.x, Level.current.topLeft.y), Color.Green);
		}
	}

	public static void Draw()
	{
		if (_core._hide)
		{
			return;
		}
		foreach (CornerDisplay d in _core._inputChangeDisplays)
		{
			Vec2 vec = new Vec2(Layer.HUD.camera.width / 2f, Layer.HUD.camera.height);
			string text = d.text;
			if (text == null)
			{
				text = "";
			}
			float stringWidth = Graphics.GetStringWidth(text);
			float wide = stringWidth;
			float stringHeight = Graphics.GetStringHeight(text);
			float high = stringHeight + 4f;
			float topOff = 0f;
			Vec2 topLeft = vec;
			Vec2 topLeftRect = vec;
			topLeft.x -= stringWidth / 2f;
			topLeftRect.x -= stringWidth / 2f;
			float offDist = Layer.HUD.camera.width / 32f + high;
			Vec2 tlOffset = Vec2.Zero;
			tlOffset = new Vec2(0f, 0f - offDist);
			Graphics.DrawRect(topLeftRect + tlOffset * d.slide, topLeftRect + new Vec2(wide, high - 1f) + tlOffset * d.slide, Color.Black, 0.95f);
			Graphics.DrawString(text, topLeft + new Vec2((wide - stringWidth) / 2f, (high - stringHeight) / 2f + topOff) + tlOffset * d.slide, Color.White, 0.97f, d.profile);
		}
		foreach (CornerDisplay d2 in _core._playerChangeDisplays)
		{
			Vec2 vec2 = new Vec2(Layer.HUD.camera.width / 2f, 0f);
			string text2 = d2.text;
			if (text2 == null)
			{
				text2 = "";
			}
			float stringWidth2 = Graphics.GetStringWidth(text2);
			float wide2 = stringWidth2;
			float stringHeight2 = Graphics.GetStringHeight(text2);
			float high2 = stringHeight2 + 4f;
			float topOff2 = 0f;
			Vec2 topLeft2 = vec2;
			Vec2 topLeftRect2 = vec2;
			topLeft2.x -= stringWidth2 / 2f;
			topLeftRect2.x -= stringWidth2 / 2f;
			float offDist2 = Layer.HUD.camera.width / 32f + high2;
			Vec2 tlOffset2 = Vec2.Zero;
			tlOffset2 = new Vec2(0f, offDist2);
			Graphics.DrawRect(topLeftRect2 + tlOffset2 * d2.slide, topLeftRect2 + new Vec2(wide2, high2 - 1f) + tlOffset2 * d2.slide, Color.Black, 0.95f);
			Graphics.DrawString(text2, topLeft2 + new Vec2((wide2 - stringWidth2) / 2f, (high2 - stringHeight2) / 2f + topOff2) + tlOffset2 * d2.slide, Color.White, 0.97f, d2.profile);
		}
		int numTopLeft = 0;
		int numTopRight = 0;
		int numBottomLeft = 0;
		int numBottomRight = 0;
		int numBottomMiddle = 0;
		int numTopMiddle = 0;
		foreach (CornerDisplay d3 in _core._cornerDisplays)
		{
			Vec2 pos = new Vec2(0f, 0f);
			switch (d3.corner)
			{
			case HUDCorner.TopLeft:
				pos = new Vec2(0f, numTopLeft * 12);
				numTopLeft++;
				break;
			case HUDCorner.TopRight:
				pos = new Vec2(Layer.HUD.camera.width, numTopRight * 12);
				numTopRight++;
				break;
			case HUDCorner.BottomLeft:
				pos = new Vec2(0f, Layer.HUD.camera.height - (float)(numBottomLeft * 12));
				numBottomLeft++;
				break;
			case HUDCorner.BottomRight:
				pos = new Vec2(Layer.HUD.camera.width, Layer.HUD.camera.height - (float)(numBottomRight * 12));
				numBottomRight++;
				break;
			case HUDCorner.BottomMiddle:
				pos = new Vec2(Layer.HUD.camera.width / 2f, Layer.HUD.camera.height - (float)(numBottomMiddle * 12));
				numBottomMiddle++;
				break;
			case HUDCorner.TopMiddle:
				pos = new Vec2(Layer.HUD.camera.width / 2f, numTopMiddle * 12);
				numTopMiddle++;
				break;
			}
			string text3 = d3.text;
			if (text3 == null)
			{
				text3 = "";
			}
			bool lowTime = false;
			if (d3.timer != null)
			{
				if (d3.timer.maxTime.TotalSeconds != 0.0)
				{
					TimeSpan t = d3.timer.maxTime - d3.timer.elapsed;
					text3 = text3 + d3.text + MonoMain.TimeString(t, 3, small: true);
					if (t.TotalSeconds < 10.0)
					{
						lowTime = true;
					}
				}
				else
				{
					text3 = text3 + d3.text + MonoMain.TimeString(d3.timer.elapsed, 3, small: true);
				}
			}
			else if (d3.counter != null && d3.counter.value is int)
			{
				int num = d3.curCount;
				if (d3.addCount == 0)
				{
					text3 = ((d3.maxCount == 0) ? (text3 + Convert.ToString(num)) : (text3 + Convert.ToString(num) + "/" + Convert.ToString(d3.maxCount)));
				}
				else
				{
					text3 += Convert.ToString(num);
					if (d3.addCount > 0)
					{
						text3 = text3 + " |GREEN|+" + Convert.ToString(d3.addCount);
					}
					else if (d3.addCount < 0)
					{
						text3 = text3 + " |RED|" + Convert.ToString(d3.addCount);
					}
				}
			}
			float stringWidth3 = Graphics.GetStringWidth(text3);
			float stringWidth4 = Graphics.GetStringWidth(text3, d3.isControl);
			float wide3 = stringWidth3 + 8f;
			float wideThin = stringWidth4 + 8f;
			float stringHeight3 = Graphics.GetStringHeight(text3);
			float high3 = stringHeight3 + 4f;
			Vec2 topLeft3 = pos;
			Vec2 topLeftRect3 = pos;
			if (d3.corner == HUDCorner.TopRight || d3.corner == HUDCorner.BottomRight)
			{
				topLeft3.x -= wide3 * d3.slide;
				topLeftRect3.x -= wideThin * d3.slide;
			}
			else if (d3.corner == HUDCorner.TopLeft || d3.corner == HUDCorner.BottomLeft)
			{
				topLeft3.x -= wide3 * (1f - d3.slide);
				topLeftRect3.x -= wideThin * (1f - d3.slide);
				topLeftRect3.x += wide3 - wideThin;
			}
			if (d3.corner == HUDCorner.BottomLeft || d3.corner == HUDCorner.BottomRight || d3.corner == HUDCorner.BottomMiddle)
			{
				topLeft3.y -= high3;
				topLeftRect3.y -= high3;
			}
			if (d3.corner == HUDCorner.BottomMiddle || d3.corner == HUDCorner.TopMiddle)
			{
				topLeft3.x -= wide3 / 2f;
				topLeftRect3.x -= wide3 / 2f;
				topLeftRect3.x += wide3 - wideThin;
			}
			if (d3.corner == HUDCorner.BottomMiddle)
			{
				topLeft3.y += 24f * (1f - d3.slide);
				topLeftRect3.y += 24f * (1f - d3.slide);
			}
			float offDist3 = Layer.HUD.camera.width / 32f;
			Vec2 tlOffset3 = Vec2.Zero;
			if (d3.corner == HUDCorner.TopLeft)
			{
				tlOffset3 = new Vec2(offDist3, offDist3);
			}
			else if (d3.corner == HUDCorner.TopRight)
			{
				tlOffset3 = new Vec2(0f - offDist3, offDist3);
			}
			else if (d3.corner == HUDCorner.BottomLeft)
			{
				tlOffset3 = new Vec2(offDist3, 0f - offDist3);
			}
			else if (d3.corner == HUDCorner.BottomRight)
			{
				tlOffset3 = new Vec2(0f - offDist3, 0f - offDist3);
			}
			else if (d3.corner == HUDCorner.BottomMiddle)
			{
				tlOffset3 = new Vec2(0f, 0f - offDist3);
			}
			else if (d3.corner == HUDCorner.TopMiddle)
			{
				tlOffset3 = new Vec2(0f, offDist3);
			}
			Graphics.DrawRect(topLeftRect3 + tlOffset3 * d3.slide, topLeftRect3 + new Vec2(wideThin, high3 - 1f) + tlOffset3 * d3.slide, Color.Black, 0.95f);
			Graphics.DrawRect(topLeftRect3 + new Vec2(wideThin, 1f) + tlOffset3 * d3.slide, topLeftRect3 + new Vec2(wideThin + 1f, high3 - 2f) + tlOffset3 * d3.slide, Color.Black, 0.95f);
			Graphics.DrawRect(topLeftRect3 + new Vec2(0f, 1f) + tlOffset3 * d3.slide, topLeftRect3 + new Vec2(-1f, high3 - 2f) + tlOffset3 * d3.slide, Color.Black, 0.95f);
			Graphics.DrawString(text3, topLeft3 + new Vec2((wide3 - stringWidth3) / 2f, (high3 - stringHeight3) / 2f) + tlOffset3 * d3.slide, lowTime ? Color.Red : Color.White, 0.98f, d3.profile);
		}
		if (!(Level.current is ChallengeLevel))
		{
			return;
		}
		foreach (TargetDuck item in Level.current.things[typeof(TargetDuck)])
		{
			item.DrawIcon();
		}
	}
}
