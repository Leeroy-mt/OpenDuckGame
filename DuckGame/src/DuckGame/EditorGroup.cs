using System;
using System.Collections.Generic;
using System.Linq;

namespace DuckGame;

public class EditorGroup
{
	public bool autopinGroup;

	public bool rootGroup;

	private List<Thing> _things = new List<Thing>();

	private List<Thing> _filteredThings = new List<Thing>();

	private List<Thing> _onlineFilteredThings = new List<Thing>();

	private List<Type> _types = new List<Type>();

	public List<EditorGroup> SubGroups = new List<EditorGroup>();

	public string Name = "";

	public List<Thing> Things
	{
		get
		{
			if (Main.isDemo)
			{
				return _filteredThings;
			}
			if (Editor._currentLevelData.metaData.onlineMode)
			{
				return _onlineFilteredThings;
			}
			return _things;
		}
	}

	public List<Thing> AllThings => _things;

	public List<Type> AllTypes => _types;

	public EditorGroup()
	{
	}

	public EditorGroup(Type filter = null, HashSet<Type> types = null)
	{
		rootGroup = filter == null;
		Initialize(filter, types);
	}

	public bool Contains(Type t)
	{
		if (_types.Contains(t))
		{
			return true;
		}
		foreach (EditorGroup subGroup in SubGroups)
		{
			if (subGroup.Contains(t))
			{
				return true;
			}
		}
		return false;
	}

	private void AddType(Type t, string group)
	{
		if (group == "survival")
		{
			return;
		}
		if (group == "")
		{
			Main.SpecialCode = "creating " + t.AssemblyQualifiedName;
			Thing newThing = null;
			newThing = Editor.GetOrCreateTypeInstance(t);
			Main.SpecialCode = "accessing " + t.AssemblyQualifiedName;
			IReadOnlyPropertyBag bag = ContentProperties.GetBag(t);
			if (bag.GetOrDefault("isInDemo", defaultValue: false))
			{
				_filteredThings.Add(newThing);
			}
			if (bag.GetOrDefault("isOnlineCapable", defaultValue: true))
			{
				_onlineFilteredThings.Add(newThing);
			}
			_things.Add(newThing);
			_types.Add(t);
			Editor.MapThing(newThing);
			MonoMain.lazyLoadyBits++;
			Main.SpecialCode = "finished " + t.AssemblyQualifiedName;
		}
		else
		{
			string[] groupName = group.Split('|');
			EditorGroup g = SubGroups.FirstOrDefault((EditorGroup x) => x.Name == groupName[0]);
			if (g == null)
			{
				g = new EditorGroup();
				g.Name = groupName[0];
				SubGroups.Add(g);
			}
			string newName = group;
			newName = ((groupName.Count() <= 1) ? newName.Remove(0, groupName[0].Length) : newName.Remove(0, groupName[0].Length + 1));
			g.AddType(t, newName);
		}
	}

	private void Sort()
	{
		SubGroups.Sort((EditorGroup x, EditorGroup y) => string.Compare(x.Name, y.Name));
		Things.Sort((Thing x, Thing y) => string.Compare(x.editorName, y.editorName));
		foreach (EditorGroup subGroup in SubGroups)
		{
			subGroup.Sort();
		}
		int maxGroups = 12;
		if (SubGroups.Count > maxGroups && rootGroup)
		{
			EditorGroup gr = new EditorGroup();
			gr.Name = "More...";
			gr.autopinGroup = true;
			int ct = SubGroups.Count - maxGroups;
			for (int i = 0; i < ct; i++)
			{
				gr.SubGroups.Add(SubGroups[maxGroups]);
				SubGroups.RemoveAt(maxGroups);
			}
			SubGroups.Add(gr);
		}
	}

	private void Initialize(Type filter = null, HashSet<Type> types = null)
	{
		List<Type> thingTypes = new List<Type>();
		if (types == null)
		{
			thingTypes.AddRange(Editor.ThingTypes);
		}
		else
		{
			thingTypes.AddRange(types);
		}
		for (int i = 0; i < thingTypes.Count; i++)
		{
			Type t = thingTypes[i];
			if (filter != null && t != filter && !Editor.AllBaseTypes[t].Contains(filter))
			{
				continue;
			}
			object[] attribute = t.GetCustomAttributes(typeof(EditorGroupAttribute), inherit: false);
			if (attribute.Length != 0)
			{
				EditorGroupAttribute attribValue = attribute[0] as EditorGroupAttribute;
				if (GroupIsAllowed(attribValue.editorType))
				{
					string newGroup = attribValue.editorGroup;
					AddType(t, newGroup);
				}
			}
		}
		Sort();
	}

	public bool GroupIsAllowed(EditorItemType pType)
	{
		if (!Options.Data.powerUser && (pType == EditorItemType.Arcade || pType == EditorItemType.Debug))
		{
			return false;
		}
		return true;
	}
}
