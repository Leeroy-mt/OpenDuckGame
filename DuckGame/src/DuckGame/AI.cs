using System;
using System.Collections.Generic;
using System.Linq;

namespace DuckGame;

public static class AI
{
	public static void InitializeLevelPaths()
	{
		foreach (PathNode item in Level.current.things[typeof(PathNode)])
		{
			item.UninitializeLinks();
			item.InitializeLinks();
		}
		foreach (PathNode item2 in Level.current.things[typeof(PathNode)])
		{
			item2.InitializePaths();
		}
	}

	private static Thing FilterBlocker(Thing b)
	{
		if (b is Door || b is Window)
		{
			return null;
		}
		return b;
	}

	public static T Nearest<T>(Vec2 position, Thing ignore = null)
	{
		PathNode start = NearestNode(position);
		if (start == null)
		{
			return default(T);
		}
		Type t = typeof(T);
		float shortestPath = 99999.9f;
		T closest = default(T);
		foreach (Thing thing in Level.current.things[t])
		{
			if (thing == ignore)
			{
				continue;
			}
			PathNode node = NearestNode(thing.position);
			if (node != null)
			{
				AIPath path = start.GetPath(node);
				if (path != null && path.length < shortestPath)
				{
					shortestPath = path.length;
					closest = (T)(object)thing;
				}
			}
		}
		return closest;
	}

	public static bool CanReach(PathNode from, Thing what)
	{
		return PathNode.CanTraverse(from.position, what.position, what);
	}

	public static Thing Nearest(Vec2 position, List<Thing> things)
	{
		PathNode start = NearestNode(position);
		if (start == null)
		{
			return null;
		}
		float shortestPath = 99999.9f;
		Thing closest = null;
		foreach (Thing thing in things)
		{
			PathNode node = NearestNode(thing.position, thing);
			if (node != null)
			{
				AIPath path = start.GetPath(node);
				if (path != null && path.nodes.Count > 0 && path.length < shortestPath && CanReach(path.nodes.Last(), thing))
				{
					shortestPath = path.length;
					closest = thing;
				}
			}
		}
		return closest;
	}

	public static PathNode NearestNode(Vec2 pos, Thing ignore = null)
	{
		List<Thing> list = Level.current.things[typeof(PathNode)].ToList();
		list.Sort((Thing a, Thing b) => (!((a.position - pos).lengthSq < (b.position - pos).lengthSq)) ? 1 : (-1));
		PathNode node = null;
		foreach (Thing thing in list)
		{
			if (PathNode.LineIsClear(pos, thing.position, ignore))
			{
				node = thing as PathNode;
				break;
			}
		}
		return node;
	}

	private static Thing GetHighest(List<IPlatform> things)
	{
		Thing highest = null;
		foreach (IPlatform plat in things)
		{
			if (!(plat is PhysicsObject))
			{
				Thing t = plat as Thing;
				if (highest == null || t.y < highest.y)
				{
					highest = t;
				}
			}
		}
		return highest;
	}

	private static Thing GetHighestNotGlass(List<IPlatform> things)
	{
		Thing highest = null;
		foreach (IPlatform plat in things)
		{
			if (!(plat is PhysicsObject) && !(plat is Window))
			{
				Thing t = plat as Thing;
				if (highest == null || t.y < highest.y)
				{
					highest = t;
				}
			}
		}
		return highest;
	}

	public static List<PathNode> GetPath(PathNode start, PathNode end)
	{
		List<PathNode> path = new List<PathNode>();
		List<PathNode> nodes = new List<PathNode>();
		foreach (PathNode t in Level.current.things[typeof(PathNode)])
		{
			t.Reset();
			nodes.Add(t);
		}
		List<PathNode> openList = new List<PathNode>();
		List<PathNode> closedList = new List<PathNode>();
		openList.Add(start);
		PathNode.CalculateNode(start, start, end);
		PathNode cheapest;
		while (true)
		{
			if (openList.Count == 0)
			{
				return path;
			}
			cheapest = null;
			foreach (PathNode node in openList)
			{
				if (cheapest == null)
				{
					cheapest = node;
				}
				else if (node.cost + node.heuristic < cheapest.cost + cheapest.heuristic)
				{
					cheapest = node;
				}
			}
			if (cheapest == null)
			{
				continue;
			}
			if (cheapest == end)
			{
				break;
			}
			closedList.Add(cheapest);
			foreach (PathNodeLink link in cheapest.links)
			{
				if (!(link.link is PathNode node2) || closedList.Contains(node2))
				{
					continue;
				}
				if (!openList.Contains(node2))
				{
					node2.parent = cheapest;
					PathNode.CalculateNode(node2, cheapest, end);
					openList.Add(node2);
					continue;
				}
				float dist = PathNode.CalculateCost(node2, cheapest);
				if (dist < node2.cost)
				{
					node2.cost = dist;
					node2.parent = cheapest;
				}
			}
			openList.Remove(cheapest);
		}
		PathNode cur = cheapest;
		path.Clear();
		while (cur != null)
		{
			path.Add(cur);
			cur = cur.parent;
		}
		foreach (PathNode item in nodes)
		{
			item.Reset();
		}
		path.Reverse();
		return path;
	}
}
