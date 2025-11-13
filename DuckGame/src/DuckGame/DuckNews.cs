using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DuckGame;

public class DuckNews
{
	private static List<DuckNews> _stories = new List<DuckNews>();

	private NewsSection _section;

	private string _name;

	private List<ScriptStatement> _requirements = new List<ScriptStatement>();

	private CycleMode _cycle;

	private ScriptStatement _valueCalculation;

	private ScriptStatement _valueCalculation2;

	private List<DuckNews> _subStories = new List<DuckNews>();

	private List<string> _dialogue = new List<string>();

	public ScriptStatement valueCalculation => _valueCalculation;

	public ScriptStatement valueCalculation2 => _valueCalculation2;

	public static void Initialize()
	{
		string[] files = Content.GetFiles("Content/news");
		foreach (string title in files)
		{
			Stream s = DuckFile.OpenStream(title);
			DuckXML duckXML = DuckXML.Load(s);
			IEnumerable<DXMLNode> root = duckXML.Elements("NewsStory");
			if (duckXML.invalid || s == null || root.Count() == 0)
			{
				throw new Exception("Content Exception: Failed to load news story (" + Content.path + "news/" + title + ".news)! Is the file missing, or has it been edited?");
			}
			if (root == null)
			{
				continue;
			}
			if (DG.isHalloween)
			{
				IEnumerable<DXMLNode> hall = root.Elements("NewsStoryHalloween");
				if (hall != null && hall.Count() > 0)
				{
					root = hall;
				}
			}
			DuckNews story = Parse(root.ElementAt(0));
			if (story != null)
			{
				_stories.Add(story);
			}
		}
		_stories = _stories.OrderBy((DuckNews x) => (int)x._section).ToList();
	}

	public static List<DuckStory> CalculateStories()
	{
		foreach (Profile item in Profiles.active)
		{
			item.endOfRoundStats = null;
		}
		List<DuckStory> stories = new List<DuckStory>();
		foreach (DuckNews story in _stories)
		{
			List<DuckStory> duckStories = story.CalculateStory();
			stories.AddRange(duckStories);
		}
		return stories;
	}

	public string FillString(string text, List<Profile> p)
	{
		if (p != null)
		{
			if (p.Count > 0)
			{
				text = text.Replace("%NAME%", p[0].name);
			}
			if (p.Count > 1)
			{
				text = text.Replace("%NAME2%", p[1].name);
			}
			if (p.Count > 2)
			{
				text = text.Replace("%NAME3%", p[2].name);
			}
			if (p.Count > 3)
			{
				text = text.Replace("%NAME4%", p[3].name);
			}
		}
		text = text.Replace("%PRICE%", Main.GetPriceString());
		if (valueCalculation != null)
		{
			object val = valueCalculation.result;
			if (val is float || val is int)
			{
				float fVal = Change.ToSingle(val);
				text = text.Replace("%VALUE%", Change.ToString(fVal));
				int intVal = Convert.ToInt32(val);
				text = text.Replace("%INTVALUE%", Change.ToString(intVal));
			}
			else if (val is string)
			{
				text = text.Replace("%VALUE%", val as string);
			}
		}
		if (valueCalculation2 != null)
		{
			object val2 = valueCalculation2.result;
			if (val2 is float || val2 is int)
			{
				float fVal2 = Change.ToSingle(val2);
				text = text.Replace("%VALUE2%", Change.ToString(fVal2));
				int intVal2 = Convert.ToInt32(val2);
				text = text.Replace("%INTVALUE2%", Change.ToString(intVal2));
			}
			else if (val2 is string)
			{
				text = text.Replace("%VALUE2%", val2 as string);
			}
		}
		return text;
	}

	public List<DuckStory> CalculateStory()
	{
		List<DuckStory> stories = new List<DuckStory>();
		List<Profile> profiles = new List<Profile>();
		if (_cycle == CycleMode.Once)
		{
			profiles.Add(Profiles.DefaultPlayer1);
			stories.AddRange(CalculateStory(profiles));
		}
		else if (_cycle == CycleMode.PerProfile)
		{
			foreach (Profile p in Profiles.active)
			{
				profiles.Add(p);
				stories.AddRange(CalculateStory(profiles));
				profiles.Clear();
			}
		}
		else if (_cycle == CycleMode.PerPosition && _valueCalculation != null)
		{
			List<List<Profile>> positions = new List<List<Profile>>();
			_ = Profiles.active;
			foreach (Profile p2 in Profiles.active)
			{
				float curVal = -999999f;
				Script.activeProfile = p2;
				object res = valueCalculation.result;
				if ((res != null && res is float) || res is int || res is double)
				{
					curVal = Change.ToSingle(res);
				}
				p2.storeValue = curVal;
				bool inserted = false;
				for (int i = 0; i < positions.Count; i++)
				{
					if (positions[i][0].storeValue < curVal)
					{
						positions.Insert(i, new List<Profile>());
						positions[i].Add(p2);
						inserted = true;
						break;
					}
					if (positions[i][0].storeValue == curVal)
					{
						positions[i].Add(p2);
						inserted = true;
						break;
					}
				}
				if (!inserted)
				{
					positions.Add(new List<Profile>());
					positions.Last().Add(p2);
				}
			}
			positions.Reverse();
			Script.positions = positions;
			int index = positions.Count - 1;
			foreach (List<Profile> position in positions)
			{
				Script.currentPosition = index;
				profiles.AddRange(position);
				stories.AddRange(CalculateStory(profiles));
				profiles.Clear();
				index--;
			}
		}
		return stories;
	}

	public List<DuckStory> CalculateStory(List<Profile> p)
	{
		List<DuckStory> stories = new List<DuckStory>();
		Script.activeNewsStory = this;
		if (p == null || p.Count > 0)
		{
			Script.activeProfile = p[0];
		}
		foreach (ScriptStatement requirement in _requirements)
		{
			object result = requirement.result;
			if (result is bool && !(bool)result)
			{
				return stories;
			}
		}
		if (_dialogue.Count > 0)
		{
			DuckStory story = new DuckStory();
			story.section = _section;
			story.text = _dialogue[Rando.Int(_dialogue.Count - 1)];
			story.text = FillString(story.text, p);
			stories.Add(story);
		}
		foreach (DuckNews subStory in _subStories)
		{
			if (subStory._valueCalculation == null)
			{
				subStory._valueCalculation = _valueCalculation;
			}
			if (subStory._valueCalculation2 == null)
			{
				subStory._valueCalculation2 = _valueCalculation2;
			}
			if (subStory._section == NewsSection.None)
			{
				subStory._section = _section;
			}
			if (subStory._cycle == CycleMode.None)
			{
				stories.AddRange(subStory.CalculateStory(p));
			}
			else
			{
				stories.AddRange(subStory.CalculateStory());
			}
		}
		return stories;
	}

	public static DuckNews Parse(DXMLNode rootElement)
	{
		DuckNews story = new DuckNews();
		DXMLAttribute nameAttribute = rootElement.Attributes("name").FirstOrDefault();
		if (nameAttribute != null)
		{
			story._name = nameAttribute.Value;
		}
		foreach (DXMLNode element in rootElement.Elements())
		{
			if (element.Name == "Section")
			{
				DXMLAttribute sectionAttrib = element.Attributes("name").FirstOrDefault();
				if (sectionAttrib != null)
				{
					try
					{
						story._section = (NewsSection)Enum.Parse(typeof(NewsSection), sectionAttrib.Value);
					}
					catch
					{
						return null;
					}
				}
			}
			else if (element.Name == "Requirement")
			{
				Script.activeProfile = Profiles.DefaultPlayer1;
				story._requirements.Add(ScriptStatement.Parse(element.Value + " "));
			}
			else if (element.Name == "Dialogue")
			{
				DXMLAttribute sectionAttrib2 = element.Attributes("value").FirstOrDefault();
				if (sectionAttrib2 != null)
				{
					story._dialogue.Add(sectionAttrib2.Value);
				}
			}
			else if (element.Name == "VALUE")
			{
				Script.activeProfile = Profiles.DefaultPlayer1;
				story._valueCalculation = ScriptStatement.Parse(element.Value + " ");
			}
			else if (element.Name == "VALUE2")
			{
				Script.activeProfile = Profiles.DefaultPlayer1;
				story._valueCalculation2 = ScriptStatement.Parse(element.Value + " ");
			}
			else if (element.Name == "Cycle")
			{
				DXMLAttribute sectionAttrib3 = element.Attributes("value").FirstOrDefault();
				if (sectionAttrib3 != null)
				{
					story._cycle = (CycleMode)Enum.Parse(typeof(CycleMode), sectionAttrib3.Value);
				}
			}
			else if (element.Name == "SubStory")
			{
				DuckNews subStory = Parse(element);
				story._subStories.Add(subStory);
			}
		}
		return story;
	}
}
