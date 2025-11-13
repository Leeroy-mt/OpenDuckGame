using System;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;

namespace DuckGame;

public class ChallengeCard : Thing
{
	private ChallengeData _challenge;

	private SpriteMap _thumb;

	private SpriteMap _preview;

	private SpriteMap _medalNoRibbon;

	private SpriteMap _medalRibbon;

	private BitmapFont _font;

	public bool hover;

	private bool _unlocked;

	public bool testing;

	public bool expand;

	public bool contract;

	private float _size = 42f;

	public float _alphaMul = 1f;

	public float _dataAlpha;

	private ChallengeSaveData _save;

	private ChallengeSaveData _realSave;

	private FancyBitmapFont _fancyFont;

	public ChallengeData challenge => _challenge;

	public bool unlocked
	{
		get
		{
			return _unlocked;
		}
		set
		{
			_unlocked = value;
		}
	}

	public ChallengeCard(float xpos, float ypos, ChallengeData c)
		: base(xpos, ypos)
	{
		_challenge = c;
		_thumb = new SpriteMap("arcade/challengeThumbnails", 38, 38);
		_thumb.frame = 1;
		_font = new BitmapFont("biosFont", 8);
		_medalNoRibbon = new SpriteMap("arcade/medalNoRibbon", 18, 18);
		_realSave = Profiles.active[0].GetSaveData(_challenge.levelID);
		_save = _realSave.Clone();
		_medalRibbon = new SpriteMap("arcade/medalRibbon", 18, 27);
		_medalRibbon.center = new Vec2(6f, 3f);
		_fancyFont = new FancyBitmapFont("smallFont");
	}

	public override void Update()
	{
		if (_preview == null && _challenge.preview != null)
		{
			MemoryStream stream = new MemoryStream(Convert.FromBase64String(_challenge.preview));
			Texture2D tex = Texture2D.FromStream(Graphics.device, stream);
			_preview = new SpriteMap(tex, tex.Width, tex.Height);
			_preview.scale = new Vec2(0.25f);
		}
		_size = Lerp.Float(_size, contract ? 1 : (expand ? 130 : 42), 8f);
		_alphaMul = Lerp.Float(_alphaMul, contract ? 0f : 1f, 0.1f);
		_dataAlpha = Lerp.Float(_dataAlpha, (_size > 126f && expand) ? 1f : 0f, (!expand) ? 1f : 0.2f);
	}

	public string MakeQuestionMarks(string val)
	{
		for (int i = 0; i < val.Length; i++)
		{
			if (val[i] != ' ')
			{
				val = val.Remove(i, 1);
				val = val.Insert(i, "?");
			}
		}
		return val;
	}

	public bool HasNewTrophy()
	{
		return _realSave.trophy != _save.trophy;
	}

	public bool HasNewBest()
	{
		if (_realSave.bestTime == _save.bestTime && _realSave.targets == _save.targets)
		{
			return _realSave.goodies != _save.goodies;
		}
		return true;
	}

	public int GiveTrophy()
	{
		int give = 0;
		if (_save.trophy != _realSave.trophy)
		{
			for (int i = (int)(_save.trophy + 1); i <= (int)_realSave.trophy; i++)
			{
				switch (i)
				{
				case 1:
					give += Challenges.valueBronze;
					break;
				case 2:
					give += Challenges.valueSilver;
					break;
				case 3:
					give += Challenges.valueGold;
					break;
				case 4:
					give += Challenges.valuePlatinum;
					break;
				}
			}
			_save.trophy = _realSave.trophy;
		}
		if (testing)
		{
			return 0;
		}
		return give;
	}

	public void GiveTime()
	{
		if (_save.bestTime != _realSave.bestTime)
		{
			_save.bestTime = _realSave.bestTime;
		}
		if (_save.goodies != _realSave.goodies)
		{
			_save.goodies = _realSave.goodies;
		}
		if (_save.targets != _realSave.targets)
		{
			_save.targets = _realSave.targets;
		}
	}

	public void UnlockAnimation()
	{
		SFX.Play("landTV", 1f, -0.3f);
		SmallSmoke smallSmoke = SmallSmoke.New(base.x + 2f, base.y + 2f);
		smallSmoke.layer = Layer.HUD;
		Level.Add(smallSmoke);
	}

	public override void Draw()
	{
		float mul = base.alpha * (hover ? 1f : 0.6f) * _alphaMul;
		_font.alpha = mul;
		Graphics.DrawRect(position, position + new Vec2(258f, _size), Color.White * mul, 0.8f + mul * 0.04f, filled: false);
		if (_save.trophy != TrophyType.Baseline)
		{
			_medalRibbon.depth = 0.81f + mul * 0.04f;
			_medalRibbon.color = new Color(mul, mul, mul);
			_medalRibbon.alpha = ArcadeHUD.alphaVal;
			if (_save.trophy == TrophyType.Bronze)
			{
				_medalRibbon.frame = 0;
			}
			else if (_save.trophy == TrophyType.Silver)
			{
				_medalRibbon.frame = 1;
			}
			else if (_save.trophy == TrophyType.Gold)
			{
				_medalRibbon.frame = 2;
			}
			else if (_save.trophy == TrophyType.Platinum)
			{
				_medalRibbon.frame = 3;
			}
			else if (_save.trophy == TrophyType.Developer)
			{
				_medalRibbon.frame = 4;
			}
			Graphics.Draw(_medalRibbon, position.x, position.y);
		}
		else if (!_unlocked)
		{
			_medalRibbon.depth = 0.81f + mul * 0.04f;
			_medalRibbon.color = new Color(mul, mul, mul);
			_medalRibbon.frame = 5;
			Graphics.Draw(_medalRibbon, position.x, position.y);
		}
		_thumb.alpha = mul;
		_thumb.depth = 0.8f + mul * 0.04f;
		if (!_unlocked)
		{
			_thumb.frame = 0;
		}
		else
		{
			_thumb.frame = 1;
		}
		if (_unlocked && _preview != null)
		{
			_preview.alpha = mul;
			_preview.depth = 0.8f + mul * 0.04f;
			Graphics.Draw(_preview, base.x + 2f, base.y + 2f);
		}
		else
		{
			Graphics.Draw(_thumb, base.x + 2f, base.y + 2f);
		}
		_font.maxWidth = 200;
		string drawName = _challenge.GetNameForDisplay();
		if (!_unlocked)
		{
			drawName = MakeQuestionMarks(drawName);
		}
		_font.Draw(drawName, base.x + 41f, base.y + 2f, Color.White * mul, 1f);
		Color c = new Color(247, 224, 89);
		string drawDesc = _challenge.description;
		if (!_unlocked)
		{
			drawDesc = MakeQuestionMarks(drawDesc);
		}
		_fancyFont.maxWidth = 200;
		_fancyFont.alpha = mul;
		FancyBitmapFont fancyFont = _fancyFont;
		float num = (_fancyFont.yscale = 0.75f);
		fancyFont.xscale = num;
		_fancyFont.Draw(drawDesc, base.x + 41f, base.y + 12f, c, 1f);
		if (!(_dataAlpha > 0.01f))
		{
			return;
		}
		float dataAlpha = _dataAlpha * mul;
		Graphics.DrawLine(position + new Vec2(0f, 42f), position + new Vec2(258f, 42f), Color.White * dataAlpha, 1f, 0.8f + mul * 0.04f);
		Graphics.DrawLine(position + new Vec2(0f, 64f), position + new Vec2(258f, 64f), Color.White * dataAlpha, 1f, 0.8f + mul * 0.04f);
		_font.alpha = dataAlpha;
		Color bestColor = new Color(245, 165, 36);
		bestColor = Colors.DGRed;
		if (_save.trophy == TrophyType.Bronze)
		{
			bestColor = Colors.Bronze;
		}
		else if (_save.trophy == TrophyType.Silver)
		{
			bestColor = Colors.Silver;
		}
		else if (_save.trophy == TrophyType.Gold)
		{
			bestColor = Colors.Gold;
		}
		else if (_save.trophy == TrophyType.Platinum)
		{
			bestColor = Colors.Platinum;
		}
		else if (_save.trophy == TrophyType.Developer)
		{
			bestColor = Colors.Developer;
		}
		_fancyFont.Draw("|DGBLUE|" + _challenge.goal, base.x + 6f, base.y + 45f, Color.White, 1f);
		_font.Draw(Chancy.GetChallengeBestString(_save, _challenge), base.x + 6f, base.y + 45f + 9f, bestColor, 1f);
		bool prevTimeReq = false;
		_medalNoRibbon.depth = 0.8f + mul * 0.04f;
		_medalNoRibbon.alpha = dataAlpha;
		_medalNoRibbon.frame = 2;
		float medalX = base.x + 6f;
		float medalY = base.y + 68f;
		Graphics.Draw(_medalNoRibbon, medalX, medalY);
		Color medalColor = new Color(245, 165, 36);
		_font.Draw("GOLD", medalX + 22f, medalY, medalColor, 1f);
		ChallengeTrophy dat = _challenge.trophies.FirstOrDefault((ChallengeTrophy val) => val.type == TrophyType.Gold);
		string reqString = "";
		bool drewTimes = false;
		if (dat.timeRequirement > 0)
		{
			TimeSpan ts = TimeSpan.FromSeconds(dat.timeRequirement);
			reqString = reqString + MonoMain.TimeString(ts, 3, small: true) + " ";
			prevTimeReq = true;
			drewTimes = true;
		}
		if (dat.targets > 0)
		{
			if (reqString != "")
			{
				reqString += ", ";
			}
			reqString = reqString + "|LIME|" + dat.targets + " TARGETS";
		}
		if (dat.goodies > 0)
		{
			if (reqString != "")
			{
				reqString += ", ";
			}
			string prefix = "GOODIES";
			if (_challenge.prefix != "")
			{
				prefix = _challenge.prefix;
			}
			reqString = reqString + "|ORANGE|" + dat.goodies + " " + prefix;
		}
		_font.Draw(reqString, medalX + 22f, medalY + 9f, Color.White, 1f);
		medalY = base.y + 68f + 20f;
		_medalNoRibbon.alpha = dataAlpha;
		_medalNoRibbon.frame = 1;
		Graphics.Draw(_medalNoRibbon, medalX, medalY);
		medalColor = new Color(173, 173, 173);
		_font.Draw("SILVER", medalX + 22f, medalY, medalColor, 1f);
		dat = _challenge.trophies.FirstOrDefault((ChallengeTrophy val) => val.type == TrophyType.Silver);
		reqString = "";
		if (drewTimes && dat.timeRequirement == 0 && _challenge.trophies[0].timeRequirement != 0)
		{
			dat.timeRequirement = _challenge.trophies[0].timeRequirement;
		}
		if (dat.timeRequirement > 0)
		{
			TimeSpan ts2 = TimeSpan.FromSeconds(dat.timeRequirement);
			reqString = reqString + MonoMain.TimeString(ts2, 3, small: true) + " ";
			prevTimeReq = true;
			drewTimes = true;
		}
		else if (prevTimeReq && _challenge.trophies[0].timeRequirement == 0)
		{
			reqString = "ANY TIME ";
		}
		if (dat.targets > 0)
		{
			if (reqString != "")
			{
				reqString += ", ";
			}
			reqString = reqString + "|LIME|" + dat.targets + " TARGETS";
		}
		if (dat.goodies > 0)
		{
			if (reqString != "")
			{
				reqString += ", ";
			}
			string prefix2 = "GOODIES";
			if (_challenge.prefix != "")
			{
				prefix2 = _challenge.prefix;
			}
			reqString = reqString + "|ORANGE|" + dat.goodies + " " + prefix2;
		}
		_font.Draw(reqString, medalX + 22f, medalY + 9f, Color.White, 1f);
		medalY = base.y + 68f + 40f;
		_medalNoRibbon.alpha = dataAlpha;
		_medalNoRibbon.frame = 0;
		Graphics.Draw(_medalNoRibbon, medalX, medalY);
		medalColor = new Color(181, 86, 3);
		_font.Draw("BRONZE", medalX + 22f, medalY, medalColor, 1f);
		dat = _challenge.trophies.FirstOrDefault((ChallengeTrophy val) => val.type == TrophyType.Bronze);
		reqString = "";
		if (drewTimes && dat.timeRequirement == 0 && _challenge.trophies[0].timeRequirement != 0)
		{
			dat.timeRequirement = _challenge.trophies[0].timeRequirement;
		}
		if (dat.timeRequirement > 0)
		{
			TimeSpan ts3 = TimeSpan.FromSeconds(dat.timeRequirement);
			reqString = reqString + MonoMain.TimeString(ts3, 3, small: true) + " ";
		}
		else if (prevTimeReq && _challenge.trophies[0].timeRequirement == 0)
		{
			reqString = "ANY TIME ";
		}
		if (dat.targets > 0)
		{
			if (reqString != "")
			{
				reqString += ", ";
			}
			reqString = reqString + "|LIME|" + dat.targets + " TARGETS";
		}
		if (dat.goodies > 0)
		{
			if (reqString != "")
			{
				reqString += ", ";
			}
			string prefix3 = "GOODIES";
			if (_challenge.prefix != "")
			{
				prefix3 = _challenge.prefix;
			}
			reqString = reqString + "|ORANGE|" + dat.goodies + " " + prefix3;
		}
		_font.Draw(reqString, medalX + 22f, medalY + 9f, Color.White, 1f);
	}
}
