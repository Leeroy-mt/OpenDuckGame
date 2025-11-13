using System.Collections.Generic;

namespace DuckGame;

public class Dialogue
{
	private static MultiMap<string, string> _speechLines = new MultiMap<string, string>();

	public static void Initialize()
	{
		IEnumerable<DXMLNode> root = DuckXML.Load(DuckFile.OpenStream("Content/dialogue/sportscaster.tlk")).Elements("Dialogue");
		if (root == null)
		{
			return;
		}
		foreach (DXMLNode element in root.Elements())
		{
			foreach (DXMLNode line in element.Elements("Line"))
			{
				_speechLines.Add(element.Name, line.Value);
			}
		}
	}

	public static string GetLine(string type)
	{
		if (!_speechLines.ContainsKey(type))
		{
			return null;
		}
		return _speechLines[type][Rando.Int(_speechLines[type].Count - 1)];
	}

	public static string GetRemark(string type, string name = null, string name2 = null, string extra01 = null, string extra02 = null)
	{
		string line = GetLine(type);
		if (line != null)
		{
			if (name != null)
			{
				line = line.Replace("%NAME%", name);
			}
			if (name2 != null)
			{
				line = line.Replace("%NAME2%", name2);
			}
			if (extra01 != null)
			{
				line = line.Replace("%DATA%", extra01);
			}
			if (extra02 != null)
			{
				line = line.Replace("%DATA2%", extra02);
			}
		}
		return line;
	}

	public static string GetRemark(string type, ResultData data)
	{
		if (data.multi)
		{
			return GetTeamRemark(type, data.name);
		}
		return GetIndividualRemark(type, data.name);
	}

	public static string GetWinnerRemark(ResultData data)
	{
		if (data.multi)
		{
			return GetLine("WinnerTeamRemark").Replace("%NAME%", data.name);
		}
		return GetLine("WinnerIndividualRemark").Replace("%NAME%", data.name);
	}

	public static string GetRunnerUpRemark(string type, ResultData data)
	{
		if (data.multi)
		{
			if (type.Contains("Positive"))
			{
				return GetLine("PositiveRunnerUpTeamRemark").Replace("%NAME%", data.name);
			}
			if (type.Contains("Neutral"))
			{
				return GetLine("NeutralRunnerUpTeamRemark").Replace("%NAME%", data.name);
			}
			if (type.Contains("Negative"))
			{
				return GetLine("NegativeRunnerUpTeamRemark").Replace("%NAME%", data.name);
			}
			return "I don't know what to say!";
		}
		if (type.Contains("Positive"))
		{
			return GetLine("PositiveRunnerUpIndividualRemark").Replace("%NAME%", data.name);
		}
		if (type.Contains("Neutral"))
		{
			return GetLine("NeutralRunnerUpIndividualRemark").Replace("%NAME%", data.name);
		}
		if (type.Contains("Negative"))
		{
			return GetLine("NegativeRunnerUpIndividualRemark").Replace("%NAME%", data.name);
		}
		return "I don't know what to say!";
	}

	public static string GetTeamRemark(string type, string name)
	{
		if (type.Contains("Positive"))
		{
			return GetLine("PositiveTeamRemark").Replace("%NAME%", name);
		}
		if (type.Contains("Neutral"))
		{
			return GetLine("NeutralTeamRemark").Replace("%NAME%", name);
		}
		if (type.Contains("Negative"))
		{
			return GetLine("NegativeTeamRemark").Replace("%NAME%", name);
		}
		return "I don't know what to say!";
	}

	public static string GetIndividualRemark(string type, string name)
	{
		if (type.Contains("Positive"))
		{
			return GetLine("PositiveIndividualRemark").Replace("%NAME%", name);
		}
		if (type.Contains("Neutral"))
		{
			return GetLine("NeutralIndividualRemark").Replace("%NAME%", name);
		}
		if (type.Contains("Negative"))
		{
			return GetLine("NegativeIndividualRemark").Replace("%NAME%", name);
		}
		return "I don't know what to say!";
	}
}
