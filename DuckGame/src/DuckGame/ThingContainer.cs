using System;
using System.Collections.Generic;
using System.Linq;

namespace DuckGame;

public class ThingContainer : Thing
{
	protected List<Thing> _things;

	protected new Type _type;

	public bool bozocheck;

	public bool quickSerialize;

	public List<Thing> things => _things;

	public override void SetTranslation(Vec2 translation)
	{
		foreach (Thing thing in _things)
		{
			thing.SetTranslation(translation);
		}
		base.SetTranslation(translation);
	}

	public ThingContainer(List<Thing> things, Type t)
	{
		_things = things;
		_type = t;
	}

	public ThingContainer()
	{
	}

	public override BinaryClassChunk Serialize()
	{
		BinaryClassChunk element = new BinaryClassChunk();
		element.AddProperty("type", ModLoader.SmallTypeName(GetType()));
		element.AddProperty("blockType", ModLoader.SmallTypeName(_type));
		BitBuffer buf = new BitBuffer(allowPacking: false);
		new BitBuffer(allowPacking: false);
		buf.Write(_things.Count);
		if (typeof(AutoBlock).IsAssignableFrom(_type))
		{
			foreach (Thing thing3 in _things)
			{
				AutoBlock obj = thing3 as AutoBlock;
				obj.groupedWithNeighbors = false;
				obj.neighborsInitialized = false;
			}
			BitBuffer groupBuf = new BitBuffer(allowPacking: false);
			groupBuf.Write((ushort)0);
			ushort numGroups = 0;
			foreach (Thing thing in _things)
			{
				AutoBlock b = thing as AutoBlock;
				b.InitializeNeighbors();
				buf.Write(thing.x);
				buf.Write(thing.y);
				buf.Write((byte)thing.frame);
				buf.Write((short)((b.upBlock != null) ? _things.IndexOf(b.upBlock) : (-1)));
				buf.Write((short)((b.downBlock != null) ? _things.IndexOf(b.downBlock) : (-1)));
				buf.Write((short)((b.rightBlock != null) ? _things.IndexOf(b.rightBlock) : (-1)));
				buf.Write((short)((b.leftBlock != null) ? _things.IndexOf(b.leftBlock) : (-1)));
				if (Editor.miniMode)
				{
					continue;
				}
				BlockGroup group = b.GroupWithNeighbors(addToLevel: false);
				if (group == null)
				{
					continue;
				}
				groupBuf.Write(group.x);
				groupBuf.Write(group.y);
				groupBuf.Write(group.collisionOffset.x);
				groupBuf.Write(group.collisionOffset.y);
				groupBuf.Write(group.collisionSize.x);
				groupBuf.Write(group.collisionSize.y);
				groupBuf.Write(group.blocks.Count());
				foreach (Block block in group.blocks)
				{
					groupBuf.Write((short)_things.IndexOf(block));
				}
				numGroups++;
			}
			groupBuf.position = 0;
			groupBuf.Write(numGroups);
			foreach (Thing thing4 in _things)
			{
				AutoBlock obj2 = thing4 as AutoBlock;
				obj2.groupedWithNeighbors = false;
				obj2.neighborsInitialized = false;
			}
			if (groupBuf.lengthInBytes > 2)
			{
				element.AddProperty("groupData", groupBuf);
			}
		}
		else
		{
			foreach (Thing thing2 in _things)
			{
				if ((byte)thing2.frame == byte.MaxValue)
				{
					buf.Write(-999999f);
				}
				buf.Write(thing2.x);
				buf.Write(thing2.y);
				if (thing2.flipHorizontal)
				{
					buf.Write(byte.MaxValue);
				}
				if ((byte)thing2.frame != byte.MaxValue)
				{
					buf.Write((byte)thing2.frame);
				}
				else
				{
					buf.Write((byte)0);
				}
			}
		}
		element.AddProperty("data", buf);
		element.AddProperty("ct20", true);
		return element;
	}

	private bool DoDeserialize(BinaryClassChunk node)
	{
		Type blockType = Editor.GetType(node.GetProperty<string>("blockType"));
		if (blockType == null)
		{
			return false;
		}
		bool autoBlocks = typeof(AutoBlock).IsAssignableFrom(blockType);
		_things = new List<Thing>();
		BitBuffer data = node.GetProperty<BitBuffer>("data");
		if (!typeof(AutoBlock).IsAssignableFrom(blockType))
		{
			autoBlocks = false;
		}
		bool newFormat = node.GetProperty<bool>("ct20");
		List<AutoBlock> blocks = new List<AutoBlock>();
		int numBlocks = data.ReadInt();
		for (int i = 0; i < numBlocks; i++)
		{
			bool twoFiftyFive = false;
			float xpos = data.ReadFloat();
			if (!autoBlocks && newFormat && xpos < -99999f)
			{
				xpos = data.ReadFloat();
				twoFiftyFive = true;
			}
			float ypos = data.ReadFloat();
			int f = 0;
			bool readFlip = false;
			if (newFormat && !autoBlocks)
			{
				f = data.ReadByte();
				if (f == 255)
				{
					readFlip = true;
					f = data.ReadByte();
				}
				if (twoFiftyFive)
				{
					f = 255;
				}
			}
			else
			{
				f = data.ReadByte();
				if (f == 255 && (Thing.loadingLevel == null || Thing.loadingLevel.GetVersion() == 2))
				{
					readFlip = true;
					f = data.ReadByte();
				}
			}
			bool flip = Level.flipH;
			if (Level.loadingOppositeSymmetry)
			{
				flip = !flip;
			}
			if (flip)
			{
				xpos = 192f - xpos - 16f;
			}
			Thing newThing = Editor.CreateThing(blockType);
			if (flip && newThing is AutoBlock)
			{
				(newThing as AutoBlock).needsRefresh = true;
				(newThing as AutoBlock).isFlipped = true;
			}
			if (newThing is BackgroundTile)
			{
				if (flip)
				{
					(newThing as BackgroundTile).isFlipped = true;
				}
				(newThing as BackgroundTile).oppositeSymmetry = !Level.loadingOppositeSymmetry;
			}
			if (flip && newThing is AutoPlatform)
			{
				(newThing as AutoPlatform).needsRefresh = true;
			}
			if (readFlip)
			{
				newThing.flipHorizontal = true;
			}
			newThing.x = xpos;
			newThing.y = ypos;
			newThing.placed = true;
			if (newThing.isStatic)
			{
				_isStatic = true;
			}
			if (autoBlocks)
			{
				short north = data.ReadShort();
				short south = data.ReadShort();
				short east = data.ReadShort();
				short west = data.ReadShort();
				AutoBlock b = newThing as AutoBlock;
				b.northIndex = north;
				b.southIndex = south;
				if (flip)
				{
					b.westIndex = east;
					b.eastIndex = west;
				}
				else
				{
					b.eastIndex = east;
					b.westIndex = west;
				}
				blocks.Add(b);
			}
			bool add = true;
			if (Level.symmetry)
			{
				if (Level.leftSymmetry && xpos > 80f)
				{
					add = false;
				}
				if (!Level.leftSymmetry && xpos < 96f)
				{
					add = false;
				}
			}
			if (add)
			{
				newThing.frame = f;
				_things.Add(newThing);
			}
		}
		if (autoBlocks && !(Level.current is Editor))
		{
			foreach (AutoBlock b2 in blocks)
			{
				if (b2.northIndex != -1)
				{
					b2.upBlock = blocks[b2.northIndex];
				}
				if (b2.southIndex != -1)
				{
					b2.downBlock = blocks[b2.southIndex];
				}
				if (b2.eastIndex != -1)
				{
					b2.rightBlock = blocks[b2.eastIndex];
				}
				if (b2.westIndex != -1)
				{
					b2.leftBlock = blocks[b2.westIndex];
				}
			}
			BitBuffer groupData = node.GetProperty<BitBuffer>("groupData");
			if (groupData != null)
			{
				ushort num = groupData.ReadUShort();
				int i2;
				for (i2 = 0; i2 < num; i2++)
				{
					BlockGroup group = new BlockGroup();
					group.position = new Vec2(groupData.ReadFloat(), groupData.ReadFloat());
					bool flip2 = Level.flipH;
					if (Level.loadingOppositeSymmetry)
					{
						flip2 = !flip2;
					}
					if (flip2)
					{
						group.position.x = 192f - group.position.x - 16f;
					}
					group.collisionOffset = new Vec2(groupData.ReadFloat(), groupData.ReadFloat());
					group.collisionSize = new Vec2(groupData.ReadFloat(), groupData.ReadFloat());
					float cpos = 88f;
					if (Level.symmetry)
					{
						if (Level.leftSymmetry)
						{
							if (group.left < cpos && group.right > cpos)
							{
								float dif = group.right - cpos;
								float newWide = group.collisionSize.x - dif;
								group.position.x -= dif;
								group.position.x += newWide / 2f;
								group.collisionSize = new Vec2(newWide, group.collisionSize.y);
								group.collisionOffset = new Vec2(0f - newWide / 2f, group.collisionOffset.y);
								group.right = cpos;
							}
						}
						else
						{
							cpos = 88f;
							if (group.right > cpos && group.left < cpos)
							{
								float dif2 = cpos - group.left;
								float newWide2 = group.collisionSize.x - dif2;
								group.position.x += dif2;
								group.position.x -= newWide2 / 2f;
								group.collisionSize = new Vec2(newWide2, group.collisionSize.y);
								group.collisionOffset = new Vec2(0f - newWide2 / 2f, group.collisionOffset.y);
								group.left = cpos;
							}
						}
					}
					int count = groupData.ReadInt();
					for (int j = 0; j < count; j++)
					{
						int index = groupData.ReadShort();
						if (index < 0)
						{
							continue;
						}
						AutoBlock b3 = blocks[index];
						bool add2 = true;
						if (Level.symmetry)
						{
							if (Level.leftSymmetry && b3.x > 80f)
							{
								add2 = false;
							}
							if (!Level.leftSymmetry && b3.x < 96f)
							{
								add2 = false;
							}
						}
						if (add2)
						{
							b3.groupedWithNeighbors = true;
							group.Add(b3);
							group.physicsMaterial = b3.physicsMaterial;
							group.thickness = b3.thickness;
						}
						_things.Remove(b3);
					}
					i2 += count;
					if (flip2)
					{
						group.needsRefresh = true;
					}
					if (Level.symmetry)
					{
						if (Level.leftSymmetry && group.left < cpos)
						{
							_things.Add(group);
						}
						else if (!Level.leftSymmetry && group.right > cpos)
						{
							_things.Add(group);
						}
					}
					else
					{
						_things.Add(group);
					}
				}
			}
		}
		return true;
	}

	public override bool Deserialize(BinaryClassChunk node)
	{
		if (Level.symmetry)
		{
			Level.leftSymmetry = true;
			Level.loadingOppositeSymmetry = false;
			DoDeserialize(node);
			List<Thing> t = new List<Thing>(_things);
			Level.loadingOppositeSymmetry = true;
			Level.leftSymmetry = false;
			DoDeserialize(node);
			_things.AddRange(t);
			return true;
		}
		return DoDeserialize(node);
	}

	public override DXMLNode LegacySerialize()
	{
		DXMLNode element = new DXMLNode("Object");
		element.Add(new DXMLNode("type", GetType().AssemblyQualifiedName));
		element.Add(new DXMLNode("blockType", _type.AssemblyQualifiedName));
		string blocks = "n,";
		string groupInfo = "";
		if (typeof(AutoBlock).IsAssignableFrom(_type))
		{
			foreach (Thing thing3 in _things)
			{
				AutoBlock obj = thing3 as AutoBlock;
				obj.groupedWithNeighbors = false;
				obj.neighborsInitialized = false;
			}
			foreach (Thing thing in _things)
			{
				AutoBlock b = thing as AutoBlock;
				b.InitializeNeighbors();
				blocks = blocks + Change.ToString(thing.x) + ",";
				blocks = blocks + Change.ToString(thing.y) + ",";
				blocks = blocks + thing.frame + ",";
				blocks = ((b.upBlock == null) ? (blocks + "-1,") : (blocks + Change.ToString(_things.IndexOf(b.upBlock)) + ","));
				blocks = ((b.downBlock == null) ? (blocks + "-1,") : (blocks + Change.ToString(_things.IndexOf(b.downBlock)) + ","));
				blocks = ((b.rightBlock == null) ? (blocks + "-1,") : (blocks + Change.ToString(_things.IndexOf(b.rightBlock)) + ","));
				blocks = ((b.leftBlock == null) ? (blocks + "-1,") : (blocks + Change.ToString(_things.IndexOf(b.leftBlock)) + ","));
				BlockGroup group = b.GroupWithNeighbors(addToLevel: false);
				if (group == null)
				{
					continue;
				}
				groupInfo = groupInfo + Change.ToString(group.x) + ",";
				groupInfo = groupInfo + Change.ToString(group.y) + ",";
				groupInfo = groupInfo + Change.ToString(group.collisionOffset.x) + ",";
				groupInfo = groupInfo + Change.ToString(group.collisionOffset.y) + ",";
				groupInfo = groupInfo + Change.ToString(group.collisionSize.x) + ",";
				groupInfo = groupInfo + Change.ToString(group.collisionSize.y) + ",";
				groupInfo = groupInfo + Change.ToString(group.blocks.Count()) + ",";
				foreach (Block block in group.blocks)
				{
					groupInfo = groupInfo + Change.ToString(_things.IndexOf(block)) + ",";
				}
			}
			foreach (Thing thing4 in _things)
			{
				AutoBlock obj2 = thing4 as AutoBlock;
				obj2.groupedWithNeighbors = false;
				obj2.neighborsInitialized = false;
			}
			if (groupInfo.Length > 0)
			{
				groupInfo = groupInfo.Substring(0, groupInfo.Length - 1);
				element.Add(new DXMLNode("groupData", groupInfo));
			}
		}
		else
		{
			foreach (Thing thing2 in _things)
			{
				blocks = blocks + Change.ToString(thing2.x) + ",";
				blocks = blocks + Change.ToString(thing2.y) + ",";
				blocks = blocks + thing2.frame + ",";
			}
		}
		blocks = blocks.Substring(0, blocks.Length - 1);
		element.Add(new DXMLNode("data", blocks));
		return element;
	}

	private bool LegacyDoDeserialize(DXMLNode node)
	{
		Type blockType = Editor.GetType(node.Element("blockType").Value);
		bool autoBlocks = typeof(AutoBlock).IsAssignableFrom(blockType);
		_things = new List<Thing>();
		string[] elements = node.Element("data").Value.Split(',');
		bool num = elements[0] == "n";
		if (!num)
		{
			autoBlocks = false;
		}
		List<AutoBlock> blocks = new List<AutoBlock>();
		for (int i = (num ? 1 : 0); i < elements.Count(); i += 3)
		{
			float xpos = Change.ToSingle(elements[i]);
			float ypos = Change.ToSingle(elements[i + 1]);
			int f = Convert.ToInt32(elements[i + 2]);
			bool flip = Level.flipH;
			if (Level.loadingOppositeSymmetry)
			{
				flip = !flip;
			}
			if (flip)
			{
				xpos = 192f - xpos - 16f;
			}
			Thing newThing = Editor.CreateThing(blockType);
			if (flip && newThing is AutoBlock)
			{
				(newThing as AutoBlock).needsRefresh = true;
				(newThing as AutoBlock).isFlipped = true;
			}
			if (flip && newThing is AutoPlatform)
			{
				(newThing as AutoPlatform).needsRefresh = true;
			}
			newThing.x = xpos;
			newThing.y = ypos;
			newThing.placed = true;
			if (newThing.isStatic)
			{
				_isStatic = true;
			}
			if (autoBlocks)
			{
				AutoBlock b = newThing as AutoBlock;
				b.northIndex = Convert.ToInt32(elements[i + 3]);
				b.southIndex = Convert.ToInt32(elements[i + 4]);
				if (flip)
				{
					b.westIndex = Convert.ToInt32(elements[i + 5]);
					b.eastIndex = Convert.ToInt32(elements[i + 6]);
				}
				else
				{
					b.eastIndex = Convert.ToInt32(elements[i + 5]);
					b.westIndex = Convert.ToInt32(elements[i + 6]);
				}
				blocks.Add(b);
				i += 4;
			}
			bool add = true;
			if (Level.symmetry)
			{
				if (Level.leftSymmetry && xpos > 80f)
				{
					add = false;
				}
				if (!Level.leftSymmetry && xpos < 96f)
				{
					add = false;
				}
			}
			if (add)
			{
				newThing.frame = f;
				_things.Add(newThing);
			}
		}
		if (autoBlocks && !(Level.current is Editor))
		{
			foreach (AutoBlock b2 in blocks)
			{
				if (b2.northIndex != -1)
				{
					b2.upBlock = blocks[b2.northIndex];
				}
				if (b2.southIndex != -1)
				{
					b2.downBlock = blocks[b2.southIndex];
				}
				if (b2.eastIndex != -1)
				{
					b2.rightBlock = blocks[b2.eastIndex];
				}
				if (b2.westIndex != -1)
				{
					b2.leftBlock = blocks[b2.westIndex];
				}
				b2.neighborsInitialized = true;
			}
			DXMLNode gData = node.Element("groupData");
			if (gData != null)
			{
				string[] groupElements = gData.Value.Split(',');
				int i2;
				for (i2 = 0; i2 < groupElements.Count(); i2 += 7)
				{
					BlockGroup group = new BlockGroup();
					group.position = new Vec2(Change.ToSingle(groupElements[i2]), Change.ToSingle(groupElements[i2 + 1]));
					bool flip2 = Level.flipH;
					if (Level.loadingOppositeSymmetry)
					{
						flip2 = !flip2;
					}
					if (flip2)
					{
						group.position.x = 192f - group.position.x - 16f;
					}
					group.collisionOffset = new Vec2(Change.ToSingle(groupElements[i2 + 2]), Change.ToSingle(groupElements[i2 + 3]));
					group.collisionSize = new Vec2(Change.ToSingle(groupElements[i2 + 4]), Change.ToSingle(groupElements[i2 + 5]));
					float cpos = 88f;
					if (Level.symmetry)
					{
						if (Level.leftSymmetry)
						{
							if (group.left < cpos && group.right > cpos)
							{
								float dif = group.right - cpos;
								float newWide = group.collisionSize.x - dif;
								group.position.x -= dif;
								group.position.x += newWide / 2f;
								group.collisionSize = new Vec2(newWide, group.collisionSize.y);
								group.collisionOffset = new Vec2(0f - newWide / 2f, group.collisionOffset.y);
								group.right = cpos;
							}
						}
						else
						{
							cpos = 88f;
							if (group.right > cpos && group.left < cpos)
							{
								float dif2 = cpos - group.left;
								float newWide2 = group.collisionSize.x - dif2;
								group.position.x += dif2;
								group.position.x -= newWide2 / 2f;
								group.collisionSize = new Vec2(newWide2, group.collisionSize.y);
								group.collisionOffset = new Vec2(0f - newWide2 / 2f, group.collisionOffset.y);
								group.left = cpos;
							}
						}
					}
					int count = Convert.ToInt32(groupElements[i2 + 6]);
					for (int j = 0; j < count; j++)
					{
						int index = Convert.ToInt32(groupElements[i2 + 7 + j]);
						AutoBlock b3 = blocks[index];
						bool add2 = true;
						if (Level.symmetry)
						{
							if (Level.leftSymmetry && b3.x > 80f)
							{
								add2 = false;
							}
							if (!Level.leftSymmetry && b3.x < 96f)
							{
								add2 = false;
							}
						}
						if (add2)
						{
							b3.groupedWithNeighbors = true;
							group.Add(b3);
							group.physicsMaterial = b3.physicsMaterial;
							group.thickness = b3.thickness;
						}
						_things.Remove(b3);
					}
					i2 += count;
					if (flip2)
					{
						group.needsRefresh = true;
					}
					if (Level.symmetry)
					{
						if (Level.leftSymmetry && group.left < cpos)
						{
							_things.Add(group);
						}
						else if (!Level.leftSymmetry && group.right > cpos)
						{
							_things.Add(group);
						}
					}
					else
					{
						_things.Add(group);
					}
				}
			}
		}
		return true;
	}

	public override bool LegacyDeserialize(DXMLNode node)
	{
		if (Level.symmetry)
		{
			Level.leftSymmetry = true;
			Level.loadingOppositeSymmetry = false;
			LegacyDoDeserialize(node);
			List<Thing> t = new List<Thing>(_things);
			Level.loadingOppositeSymmetry = true;
			Level.leftSymmetry = false;
			LegacyDoDeserialize(node);
			_things.AddRange(t);
			return true;
		}
		return LegacyDoDeserialize(node);
	}
}
