using System.Collections.Generic;

namespace DuckGame;

public class LevelPlaylist
{
	public List<string> levels = new List<string>();

	public DXMLNode Serialize()
	{
		DXMLNode element = new DXMLNode("playlist");
		foreach (string s in levels)
		{
			DXMLNode member = new DXMLNode("element", s);
			element.Add(member);
		}
		return element;
	}

	public void Deserialize(DXMLNode node)
	{
		levels.Clear();
		foreach (DXMLNode e in node.Elements())
		{
			if (e.Name == "element" && DuckFile.FileExists(e.Value))
			{
				levels.Add(e.Value);
			}
		}
	}
}
