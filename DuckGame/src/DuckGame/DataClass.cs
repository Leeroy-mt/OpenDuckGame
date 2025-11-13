using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace DuckGame;

public class DataClass
{
	protected string _nodeName = "DataNode";

	private static DXMLNode SerializeDict(string name, IDictionary dict)
	{
		if (dict.Keys.Count > 0)
		{
			string bigString = "";
			foreach (object val in dict.Keys)
			{
				bigString = bigString + Convert.ToString(val) + "|" + Convert.ToString(dict[val]) + "@";
			}
			DXMLNode dXMLNode = new DXMLNode(name);
			dXMLNode.Add(new DXMLNode("valueString", bigString));
			return dXMLNode;
		}
		return null;
	}

	private static DXMLNode SerializeCollection(string name, IList coll)
	{
		if (coll.Count > 0)
		{
			string bigString = "";
			foreach (object o in coll)
			{
				bigString = bigString + Convert.ToString(o) + "|";
			}
			bigString = bigString.Substring(0, bigString.Length - 1);
			DXMLNode dXMLNode = new DXMLNode(name);
			dXMLNode.Add(new DXMLNode("valueString", bigString));
			return dXMLNode;
		}
		return null;
	}

	public static DXMLNode SerializeClass(object o, string nodeName)
	{
		DXMLNode element = new DXMLNode(nodeName);
		PropertyInfo[] properties = o.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		FieldInfo[] fields = o.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		PropertyInfo[] array = properties;
		foreach (PropertyInfo property in array)
		{
			if (property.PropertyType == typeof(RasterFont))
			{
				RasterFont f = property.GetValue(o, null) as RasterFont;
				if (f == null)
				{
					f = RasterFont.None;
				}
				element.Add(new DXMLNode(property.Name, f.Serialize()));
			}
			else if (property.PropertyType == typeof(StatBinding))
			{
				StatBinding b = property.GetValue(o, null) as StatBinding;
				element.Add(new DXMLNode(property.Name, b.value));
			}
			else if (property.PropertyType == typeof(Resolution))
			{
				Resolution b2 = property.GetValue(o, null) as Resolution;
				string name = property.Name;
				string[] obj = new string[5]
				{
					b2.x.ToString(),
					"x",
					b2.y.ToString(),
					"x",
					null
				};
				int mode = (int)b2.mode;
				obj[4] = mode.ToString();
				element.Add(new DXMLNode(name, string.Concat(obj)));
			}
			else if (property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(Dictionary<, >))
			{
				IDictionary first = property.GetValue(o, null) as IDictionary;
				DXMLNode newElement = SerializeDict(property.Name, first);
				if (newElement != null)
				{
					element.Add(newElement);
				}
			}
			else if (property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(IList<>))
			{
				IList first2 = property.GetValue(o, null) as IList;
				DXMLNode newElement2 = SerializeCollection(property.Name, first2);
				if (newElement2 != null)
				{
					element.Add(newElement2);
				}
			}
			else if (property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(List<>))
			{
				IList first3 = property.GetValue(o, null) as IList;
				DXMLNode newElement3 = SerializeCollection(property.Name, first3);
				if (newElement3 != null)
				{
					element.Add(newElement3);
				}
			}
			else if (property.PropertyType.IsPrimitive || property.PropertyType.Equals(typeof(string)))
			{
				element.Add(new DXMLNode(property.Name, property.GetValue(o, null)));
			}
		}
		FieldInfo[] array2 = fields;
		foreach (FieldInfo field in array2)
		{
			if (field.Name.Contains("k__BackingField") || field.Name.StartsWith("__"))
			{
				continue;
			}
			if (field.FieldType == typeof(RasterFont))
			{
				RasterFont f2 = field.GetValue(o) as RasterFont;
				if (f2 == null)
				{
					f2 = RasterFont.None;
				}
				element.Add(new DXMLNode(field.Name, f2.Serialize()));
			}
			else if (field.FieldType == typeof(StatBinding))
			{
				StatBinding b3 = field.GetValue(o) as StatBinding;
				element.Add(new DXMLNode(field.Name, b3.value));
			}
			else if (field.FieldType == typeof(Resolution))
			{
				Resolution b4 = field.GetValue(o) as Resolution;
				string name2 = field.Name;
				string[] obj2 = new string[5]
				{
					b4.x.ToString(),
					"x",
					b4.y.ToString(),
					"x",
					null
				};
				int mode = (int)b4.mode;
				obj2[4] = mode.ToString();
				element.Add(new DXMLNode(name2, string.Concat(obj2)));
			}
			else if (field.FieldType.IsGenericType && field.FieldType.GetGenericTypeDefinition() == typeof(Dictionary<, >))
			{
				IDictionary first4 = field.GetValue(o) as IDictionary;
				DXMLNode newElement4 = SerializeDict(field.Name, first4);
				if (newElement4 != null)
				{
					element.Add(newElement4);
				}
			}
			else if (field.FieldType.IsGenericType && field.FieldType.GetGenericTypeDefinition() == typeof(IList<>))
			{
				IList first5 = field.GetValue(o) as IList;
				DXMLNode newElement5 = SerializeCollection(field.Name, first5);
				if (newElement5 != null)
				{
					element.Add(newElement5);
				}
			}
			else if (field.FieldType.IsGenericType && field.FieldType.GetGenericTypeDefinition() == typeof(List<>))
			{
				IList first6 = field.GetValue(o) as IList;
				DXMLNode newElement6 = SerializeCollection(field.Name, first6);
				if (newElement6 != null)
				{
					element.Add(newElement6);
				}
			}
			else if (field.FieldType.IsPrimitive || field.FieldType.Equals(typeof(string)))
			{
				element.Add(new DXMLNode(field.Name, field.GetValue(o)));
			}
		}
		return element;
	}

	public static object ReadValue(string value, Type t)
	{
		if (t == typeof(string))
		{
			return value;
		}
		if (t == typeof(float))
		{
			return Convert.ToSingle(value, CultureInfo.InvariantCulture);
		}
		if (t == typeof(double))
		{
			return Convert.ToDouble(value, CultureInfo.InvariantCulture);
		}
		if (t == typeof(byte))
		{
			return Convert.ToByte(value, CultureInfo.InvariantCulture);
		}
		if (t == typeof(short))
		{
			return Convert.ToInt16(value, CultureInfo.InvariantCulture);
		}
		if (t == typeof(int))
		{
			return Convert.ToInt32(value, CultureInfo.InvariantCulture);
		}
		if (t == typeof(long))
		{
			return Convert.ToInt64(value, CultureInfo.InvariantCulture);
		}
		if (t == typeof(ulong))
		{
			return Convert.ToUInt64(value, CultureInfo.InvariantCulture);
		}
		return null;
	}

	private static void DeserializeDict(IDictionary dict, DXMLNode element, Type keyType, Type valType)
	{
		dict.Clear();
		DXMLNode strinval = element.Element("valueString");
		if (strinval != null)
		{
			string[] array = strinval.Value.Split('@');
			for (int i = 0; i < array.Length; i++)
			{
				string[] split = array[i].Split('|');
				if (split.Length == 2)
				{
					try
					{
						dict[ReadValue(split[0].Trim(), keyType)] = ReadValue(split[1].Trim(), valType);
					}
					catch (Exception)
					{
					}
				}
			}
			return;
		}
		foreach (DXMLNode dictElement in element.Elements())
		{
			if (dictElement.Elements().Count() == 2)
			{
				object key = ReadValue(dictElement.Elements().ElementAt(0).Value, keyType);
				object val = ReadValue(dictElement.Elements().ElementAt(1).Value, valType);
				if (key != null && val != null)
				{
					dict[key] = val;
				}
			}
		}
	}

	private static void DeserializeCollection(IList dict, DXMLNode element, Type keyType)
	{
		dict.Clear();
		DXMLNode strinval = element.Element("valueString");
		if (strinval == null)
		{
			return;
		}
		string[] array = strinval.Value.Split('|');
		foreach (string part in array)
		{
			try
			{
				dict.Add(ReadValue(part.Trim(), keyType));
			}
			catch (Exception)
			{
			}
		}
	}

	public static void DeserializeClass(object output, DXMLNode node)
	{
		if (output == null)
		{
			return;
		}
		Type type = output.GetType();
		foreach (DXMLNode element in node.Elements())
		{
			try
			{
				PropertyInfo property = type.GetProperty(element.Name);
				if (property != null)
				{
					if (property.PropertyType == typeof(RasterFont))
					{
						property.SetValue(output, RasterFont.Deserialize(element.Value));
					}
					else if (property.PropertyType == typeof(StatBinding))
					{
						if ((!Steam.IsInitialized() || Steam.GetStat(property.Name) < -99999f) && property.GetValue(output, null) is StatBinding b)
						{
							if (b.isFloat)
							{
								b.valueFloat = Convert.ToSingle(element.Value);
							}
							else
							{
								b.valueInt = Convert.ToInt32(element.Value);
							}
						}
					}
					else if (property.PropertyType == typeof(Resolution))
					{
						property.SetValue(output, Resolution.Load(element.Value, property.Name));
					}
					else if (property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(Dictionary<, >))
					{
						Type[] genericArguments = property.PropertyType.GetGenericArguments();
						Type keyType = genericArguments[0];
						Type valType = genericArguments[1];
						DeserializeDict(property.GetValue(output, null) as IDictionary, element, keyType, valType);
					}
					else if (property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(IList<>))
					{
						Type keyType2 = property.PropertyType.GetGenericArguments()[0];
						DeserializeCollection(property.GetValue(output, null) as IList, element, keyType2);
					}
					else if (property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(List<>))
					{
						Type keyType3 = property.PropertyType.GetGenericArguments()[0];
						DeserializeCollection(property.GetValue(output, null) as IList, element, keyType3);
					}
					else if (property.PropertyType.IsPrimitive || property.PropertyType.Equals(typeof(string)))
					{
						property.SetValue(output, Convert.ChangeType(element.Value, property.PropertyType, CultureInfo.InvariantCulture), null);
					}
					continue;
				}
				FieldInfo field = type.GetField(element.Name);
				if (!(field != null))
				{
					continue;
				}
				if (field.FieldType == typeof(RasterFont))
				{
					field.SetValue(output, RasterFont.Deserialize(element.Value));
				}
				else if (field.FieldType == typeof(StatBinding))
				{
					if ((!Steam.IsInitialized() || Steam.GetStat(field.Name) < -99999f) && field.GetValue(output) is StatBinding b2)
					{
						if (b2.isFloat)
						{
							b2.valueFloat = Convert.ToSingle(element.Value);
						}
						else
						{
							b2.valueInt = Convert.ToInt32(element.Value);
						}
					}
				}
				else if (field.FieldType == typeof(Resolution))
				{
					field.SetValue(output, Resolution.Load(element.Value, property.Name));
				}
				else if (field.FieldType.IsGenericType && field.FieldType.GetGenericTypeDefinition() == typeof(Dictionary<, >))
				{
					Type[] genericArguments2 = field.FieldType.GetGenericArguments();
					Type keyType4 = genericArguments2[0];
					Type valType2 = genericArguments2[1];
					DeserializeDict(field.GetValue(output) as IDictionary, element, keyType4, valType2);
				}
				else if (field.FieldType.IsGenericType && field.FieldType.GetGenericTypeDefinition() == typeof(IList<>))
				{
					Type keyType5 = field.FieldType.GetGenericArguments()[0];
					DeserializeCollection(field.GetValue(output) as IList, element, keyType5);
				}
				else if (field.FieldType.IsGenericType && field.FieldType.GetGenericTypeDefinition() == typeof(List<>))
				{
					Type keyType6 = field.FieldType.GetGenericArguments()[0];
					DeserializeCollection(field.GetValue(output) as IList, element, keyType6);
				}
				else if (field.FieldType.IsPrimitive || field.FieldType.Equals(typeof(string)))
				{
					field.SetValue(output, Convert.ChangeType(element.Value, field.FieldType, CultureInfo.InvariantCulture));
				}
			}
			catch
			{
				Program.LogLine("Error parsing data value in " + type.ToString() + " (" + element.Name + ")");
			}
		}
	}

	public virtual DXMLNode Serialize()
	{
		return SerializeClass(this, _nodeName);
	}

	public virtual bool Deserialize(DXMLNode node)
	{
		DeserializeClass(this, node);
		return true;
	}

	public static DataClass operator -(DataClass value1, DataClass value2)
	{
		DataClass newStats = Activator.CreateInstance(value1.GetType(), null) as DataClass;
		PropertyInfo[] properties = value1.GetType().GetProperties();
		foreach (PropertyInfo property in properties)
		{
			if (property.PropertyType == typeof(int))
			{
				int me = (int)property.GetValue(value1, null);
				int you = (int)property.GetValue(value2, null);
				property.SetValue(newStats, me - you, null);
			}
			else if (property.PropertyType == typeof(float))
			{
				float me2 = (float)property.GetValue(value1, null);
				float you2 = (float)property.GetValue(value2, null);
				property.SetValue(newStats, me2 - you2, null);
			}
			else if (property.PropertyType == typeof(DateTime))
			{
				_ = (DateTime)property.GetValue(value1, null);
				DateTime you3 = (DateTime)property.GetValue(value2, null);
				property.SetValue(newStats, you3, null);
			}
			else
			{
				property.SetValue(newStats, property.GetValue(value2, null), null);
			}
		}
		return newStats;
	}

	public static DataClass operator +(DataClass value1, DataClass value2)
	{
		DataClass newStats = Activator.CreateInstance(value1.GetType(), null) as DataClass;
		PropertyInfo[] properties = value1.GetType().GetProperties();
		foreach (PropertyInfo property in properties)
		{
			if (property.PropertyType == typeof(int))
			{
				int me = (int)property.GetValue(value1, null);
				int you = (int)property.GetValue(value2, null);
				property.SetValue(newStats, me + you, null);
			}
			else if (property.PropertyType == typeof(float))
			{
				float me2 = (float)property.GetValue(value1, null);
				float you2 = (float)property.GetValue(value2, null);
				property.SetValue(newStats, me2 + you2, null);
			}
			else if (property.PropertyType == typeof(DateTime))
			{
				DateTime me3 = (DateTime)property.GetValue(value1, null);
				DateTime you3 = (DateTime)property.GetValue(value2, null);
				if (me3 > you3)
				{
					property.SetValue(newStats, me3, null);
				}
				else
				{
					property.SetValue(newStats, you3, null);
				}
			}
			else
			{
				property.SetValue(newStats, property.GetValue(value2, null), null);
			}
		}
		return newStats;
	}
}
