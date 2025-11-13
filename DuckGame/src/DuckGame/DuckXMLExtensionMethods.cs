using System.Collections.Generic;

namespace DuckGame;

public static class DuckXMLExtensionMethods
{
	public static IEnumerable<DXMLAttribute> Attributes<T>(this IEnumerable<T> source, string name) where T : DXMLNode
	{
		List<DXMLAttribute> attribs = new List<DXMLAttribute>();
		foreach (T n in source)
		{
			foreach (DXMLAttribute a in n.Attributes())
			{
				if (a.Name == name)
				{
					attribs.Add(a);
				}
			}
			IEnumerable<DXMLAttribute> innerNodes = n.Elements().Attributes(name);
			attribs.AddRange(innerNodes);
		}
		return attribs;
	}

	public static IEnumerable<DXMLNode> Elements<T>(this IEnumerable<T> source) where T : DXMLNode
	{
		List<DXMLNode> nodes = new List<DXMLNode>();
		foreach (T n in source)
		{
			nodes.Add(n);
			IEnumerable<DXMLNode> innerNodes = n.Elements().Elements();
			nodes.AddRange(innerNodes);
		}
		return nodes;
	}

	public static IEnumerable<DXMLNode> Elements<T>(this IEnumerable<T> source, string name) where T : DXMLNode
	{
		List<DXMLNode> nodes = new List<DXMLNode>();
		foreach (T n in source)
		{
			if (n.Name == name)
			{
				nodes.Add(n);
			}
			IEnumerable<DXMLNode> innerNodes = n.Elements().Elements(name);
			nodes.AddRange(innerNodes);
		}
		return nodes;
	}
}
