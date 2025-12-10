using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DuckGame;

public class BinaryClassChunk
{
    private BinaryClassChunk _extraHeaderInfo;

    public bool ignore;

    private long _magicNumber;

    private ushort _version;

    private uint _size;

    private uint _offset;

    private uint _checksum;

    private BitBuffer _data;

    private BitBuffer _serializedData;

    private Dictionary<string, BinaryClassChunk> _headerDictionary = new Dictionary<string, BinaryClassChunk>();

    private MultiMap<string, object> _extraProperties;

    private DeserializeResult _result;

    private Exception _exception;

    public static bool fullDeserializeMode;

    public BinaryClassChunk GetExtraHeaderInfo()
    {
        return _extraHeaderInfo;
    }

    public void SetExtraHeaderInfo(BinaryClassChunk pValue)
    {
        _extraHeaderInfo = pValue;
    }

    public T Header<T>()
    {
        return (T)(object)_extraHeaderInfo;
    }

    public ushort GetVersion()
    {
        return _version;
    }

    public BitBuffer GetData()
    {
        if (_data == null && _serializedData == null)
        {
            _serializedData = Serialize();
        }
        if (_serializedData == null)
        {
            return _data;
        }
        return _serializedData;
    }

    public Exception GetException()
    {
        return _exception;
    }

    public DeserializeResult GetResult()
    {
        return _result;
    }

    public uint GetChecksum()
    {
        if (_data == null && _serializedData == null)
        {
            _serializedData = Serialize();
        }
        if (_checksum == 0)
        {
            _checksum = Editor.Checksum(GetData().buffer, (int)_offset, (int)_size);
        }
        return _checksum;
    }

    public void AddProperty(string id, object value)
    {
        if (_extraProperties == null)
        {
            _extraProperties = new MultiMap<string, object>();
        }
        _extraProperties.Add(id, value);
    }

    public T GetProperty<T>(string id)
    {
        object t = GetProperty(id);
        if (t == null)
        {
            return default(T);
        }
        return (T)t;
    }

    public List<T> GetProperties<T>(string id)
    {
        List<object> obj = GetProperty(id, multiple: true) as List<object>;
        List<T> list = new List<T>();
        foreach (object o in obj)
        {
            list.Add((T)o);
        }
        return list;
    }

    public bool HasProperty(string id)
    {
        if (_extraProperties != null)
        {
            return _extraProperties.ContainsKey(id);
        }
        return false;
    }

    public object GetProperty(string id, bool multiple = false)
    {
        if (_extraProperties == null)
        {
            return null;
        }
        List<object> o = null;
        _extraProperties.TryGetValue(id, out o);
        if (o != null)
        {
            foreach (object obj in o)
            {
                if (obj is BinaryClassChunk)
                {
                    BinaryClassChunk chunk = obj as BinaryClassChunk;
                    if (chunk._result == DeserializeResult.HeaderDeserialized)
                    {
                        chunk.Deserialize();
                    }
                }
                if (obj is BitBuffer)
                {
                    (obj as BitBuffer).SeekToStart();
                }
                if (!multiple)
                {
                    return obj;
                }
            }
        }
        return o;
    }

    public T GetPrimitive<T>(string id)
    {
        if (_extraProperties == null)
        {
            return default(T);
        }
        List<object> o = null;
        _extraProperties.TryGetValue(id, out o);
        if (o != null)
        {
            return (T)o.First();
        }
        return default(T);
    }

    public void SetData(BitBuffer data)
    {
        SetData(data, pHeaderOnly: false);
    }

    public bool SetData(BitBuffer data, bool pHeaderOnly)
    {
        DeserializeHeader(GetType(), data, this);
        if (!pHeaderOnly && _result == DeserializeResult.HeaderDeserialized)
        {
            Deserialize();
        }
        return _result == DeserializeResult.HeaderDeserialized;
    }

    public static T FromData<T>(BitBuffer data) where T : BinaryClassChunk
    {
        return FromData<T>(data, pHeaderOnly: false);
    }

    public static T FromData<T>(BitBuffer data, bool pHeaderOnly) where T : BinaryClassChunk
    {
        BinaryClassChunk obj = Activator.CreateInstance(typeof(T), null) as BinaryClassChunk;
        obj.SetData(data, pHeaderOnly);
        return (T)obj;
    }

    private Array DeserializeArray(Type type, Type arrayType, BitBuffer data)
    {
        int length = _data.ReadInt();
        Array arrayData = Array.CreateInstance(arrayType, length);
        for (int iArrayElement = 0; iArrayElement < length; iArrayElement++)
        {
            bool num = _data.ReadBool();
            object o = null;
            if (num)
            {
                if (typeof(BinaryClassChunk).IsAssignableFrom(arrayType))
                {
                    BinaryClassChunk chunk = DeserializeHeader(arrayType, _data, null, root: false, skipData: true);
                    chunk?.Deserialize();
                    o = chunk;
                }
                else
                {
                    o = _data.Read(arrayType);
                }
            }
            arrayData.SetValue(o, iArrayElement);
        }
        return arrayData;
    }

    public bool Deserialize()
    {
        if (_data == null)
        {
            _result = DeserializeResult.NoData;
            return false;
        }
        if (_result == DeserializeResult.Success)
        {
            return true;
        }
        try
        {
            _data.position = (int)_offset;
            ushort numMembers = _data.ReadUShort();
            Type this_type = GetType();
            for (int i = 0; i < numMembers; i++)
            {
                string memberName = _data.ReadString();
                Type classMemberType = null;
                ClassMember classMember = null;
                byte typeByte = 0;
                if (memberName.StartsWith("@"))
                {
                    if (_extraProperties == null)
                    {
                        _extraProperties = new MultiMap<string, object>();
                    }
                    typeByte = _data.ReadByte();
                    if (typeByte != byte.MaxValue)
                    {
                        if ((typeByte & 1) != 0)
                        {
                            typeByte >>= 1;
                            BinaryClassMember.typeMap.TryGetKey(typeByte, out classMemberType);
                        }
                        else
                        {
                            classMemberType = Editor.GetType(_data.ReadString());
                        }
                        memberName = memberName.Substring(1, memberName.Length - 1);
                    }
                }
                else
                {
                    classMember = Editor.GetMember(this_type, memberName);
                    if (classMember != null)
                    {
                        classMemberType = classMember.type;
                    }
                }
                if (typeByte == byte.MaxValue)
                {
                    continue;
                }
                if (classMemberType != null)
                {
                    _ = classMemberType.FullName;
                }
                if (classMember != null)
                {
                    _ = classMember.name;
                    if (classMember.field != null)
                    {
                        classMember.field.GetType();
                    }
                    else if (classMember.property != null)
                    {
                        classMember.property.GetType();
                    }
                }
                uint memberSize = _data.ReadUInt();
                if (memberSize == 0)
                {
                    continue;
                }
                if (classMemberType != null)
                {
                    int readPos = _data.position;
                    if (typeof(BinaryClassChunk).IsAssignableFrom(classMemberType))
                    {
                        BinaryClassChunk newChunk = DeserializeHeader(classMemberType, _data, null, root: false);
                        if (fullDeserializeMode && newChunk._result == DeserializeResult.HeaderDeserialized)
                        {
                            newChunk.Deserialize();
                        }
                        if (classMember == null)
                        {
                            _extraProperties.Add(memberName, newChunk);
                        }
                        else
                        {
                            _headerDictionary[memberName] = newChunk;
                        }
                        _data.position = readPos + (int)memberSize;
                    }
                    else if (classMemberType.IsArray)
                    {
                        Array arrayData = DeserializeArray(classMemberType, classMemberType.GetElementType(), _data);
                        if (classMember == null)
                        {
                            _extraProperties.Add(memberName, arrayData);
                        }
                        else
                        {
                            classMember.SetValue(this, arrayData);
                        }
                    }
                    else if (classMemberType.IsGenericType && classMemberType.GetGenericTypeDefinition() == typeof(List<>))
                    {
                        Array array = DeserializeArray(typeof(object[]), classMemberType.GetGenericArguments()[0], _data);
                        IList list = Activator.CreateInstance(classMemberType) as IList;
                        foreach (object o in array)
                        {
                            list.Add(o);
                        }
                        if (classMember == null)
                        {
                            _extraProperties.Add(memberName, list);
                        }
                        else
                        {
                            classMember.SetValue(this, list);
                        }
                    }
                    else if (classMemberType.IsGenericType && classMemberType.GetGenericTypeDefinition() == typeof(HashSet<>))
                    {
                        Array arrayData2 = DeserializeArray(typeof(object[]), classMemberType.GetGenericArguments()[0], _data);
                        IList tempList = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(classMemberType.GetGenericArguments()[0]));
                        foreach (object o2 in arrayData2)
                        {
                            tempList.Add(o2);
                        }
                        object list2 = Activator.CreateInstance(classMemberType, tempList);
                        if (classMember == null)
                        {
                            _extraProperties.Add(memberName, list2);
                        }
                        else
                        {
                            classMember.SetValue(this, list2);
                        }
                    }
                    else
                    {
                        object val = null;
                        val = ((!classMemberType.IsEnum) ? _data.Read(classMemberType, allowPacking: false) : ((object)_data.ReadInt()));
                        if (classMember == null)
                        {
                            _extraProperties.Add(memberName, val);
                        }
                        else
                        {
                            classMember.SetValue(this, val);
                        }
                    }
                }
                else
                {
                    _data.position += (int)memberSize;
                }
            }
        }
        catch (Exception exception)
        {
            _exception = exception;
            _result = DeserializeResult.ExceptionThrown;
            return false;
        }
        _result = DeserializeResult.Success;
        return true;
    }

    public static BinaryClassChunk DeserializeHeader(Type t, BitBuffer data, BinaryClassChunk target = null, bool root = true, bool skipData = false)
    {
        if (target == null)
        {
            target = Activator.CreateInstance(t, null) as BinaryClassChunk;
        }
        try
        {
            long magicNumber = 0L;
            if (root)
            {
                magicNumber = data.ReadLong();
                if (magicNumber != MagicNumber(t))
                {
                    target._result = DeserializeResult.InvalidMagicNumber;
                    return target;
                }
                target._checksum = data.ReadUInt();
            }
            ushort version = data.ReadUShort();
            ushort currentVersion = ChunkVersion(t);
            bool versionCheckPass = true;
            if (version != currentVersion)
            {
                if (version > currentVersion)
                {
                    target._result = DeserializeResult.FileVersionTooNew;
                    versionCheckPass = false;
                }
                else if (currentVersion != 2)
                {
                    target._result = DeserializeResult.FileVersionTooOld;
                    versionCheckPass = false;
                }
                if (!versionCheckPass)
                {
                    return target;
                }
            }
            target._magicNumber = magicNumber;
            target._version = version;
            if (version > 1 && target is LevelData && data.ReadBool())
            {
                Type classMemberType = Editor.GetType(data.ReadString());
                target.SetExtraHeaderInfo(DeserializeHeader(classMemberType, data, null, root: false));
                if (target.GetExtraHeaderInfo() != null && target.GetExtraHeaderInfo()._result == DeserializeResult.HeaderDeserialized)
                {
                    target.GetExtraHeaderInfo().Deserialize();
                }
            }
            target._size = data.ReadUInt();
            target._offset = (uint)data.position;
            target._data = data;
            target._result = DeserializeResult.HeaderDeserialized;
            if (skipData)
            {
                data.position = (int)(target._offset + target._size);
            }
            return target;
        }
        catch (Exception exception)
        {
            target._exception = exception;
            target._result = DeserializeResult.ExceptionThrown;
            return target;
        }
    }

    public T GetChunk<T>(string name)
    {
        return GetChunk<T>(name, pPartialDeserialize: false);
    }

    public T GetChunk<T>(string name, bool pPartialDeserialize)
    {
        return GetChunk<T>(name, pPartialDeserialize, pForceCreation: false);
    }

    public T GetChunk<T>(string name, bool pPartialDeserialize, bool pForceCreation)
    {
        BinaryClassChunk chunk = null;
        if (_result == DeserializeResult.HeaderDeserialized)
        {
            Deserialize();
        }
        if (!_headerDictionary.TryGetValue(name, out chunk))
        {
            chunk = Activator.CreateInstance(typeof(T), null) as BinaryClassChunk;
            chunk._result = DeserializeResult.Success;
            _headerDictionary[name] = chunk;
        }
        if (chunk != null)
        {
            if (chunk._result == DeserializeResult.HeaderDeserialized)
            {
                chunk.Deserialize();
            }
            return (T)(object)chunk;
        }
        return default(T);
    }

    private void SerializeArray(Array array, Type arrayType, BitBuffer data)
    {
        data.Write(array.Length);
        for (int i = 0; i < array.Length; i++)
        {
            object val = array.GetValue(i);
            data.Write(val != null);
            if (val != null)
            {
                if (typeof(BinaryClassChunk).IsAssignableFrom(arrayType))
                {
                    (val as BinaryClassChunk).Serialize(data, root: false);
                }
                else
                {
                    data.Write(val);
                }
            }
        }
    }

    public virtual BitBuffer Serialize(BitBuffer data = null, bool root = true)
    {
        if (data == null)
        {
            data = new BitBuffer(allowPacking: false);
        }
        _serializedData = data;
        if (data.allowPacking)
        {
            throw new Exception("This class does not support serialization with a packed bit buffer. Construct the buffer with allowPacking set to false.");
        }
        Type t = GetType();
        List<ClassMember> members = Editor.GetMembers(t);
        List<BinaryClassMember> members2 = new List<BinaryClassMember>();
        foreach (ClassMember memberInfo in members)
        {
            if (!memberInfo.isPrivate && !(memberInfo.name == "metaData") && (memberInfo.type.IsEnum || memberInfo.type.IsPrimitive || memberInfo.type.Equals(typeof(string)) || typeof(BinaryClassChunk).IsAssignableFrom(memberInfo.type) || memberInfo.type.IsArray || (memberInfo.type.IsGenericType && (memberInfo.type.GetGenericTypeDefinition() == typeof(List<>) || memberInfo.type.GetGenericTypeDefinition() == typeof(HashSet<>)))))
            {
                object val = memberInfo.GetValue(this);
                if (memberInfo.type.IsEnum)
                {
                    val = (int)val;
                }
                if (val != null)
                {
                    BinaryClassMember member = new BinaryClassMember
                    {
                        name = memberInfo.name,
                        data = val
                    };
                    members2.Add(member);
                }
            }
        }
        if (_extraProperties != null)
        {
            foreach (KeyValuePair<string, List<object>> pair in _extraProperties)
            {
                if (pair.Value == null)
                {
                    continue;
                }
                foreach (object item2 in pair.Value)
                {
                    object val2 = item2;
                    if (val2 != null && val2.GetType().IsEnum)
                    {
                        val2 = (int)val2;
                    }
                    BinaryClassMember member2 = new BinaryClassMember
                    {
                        name = "@" + pair.Key,
                        data = val2,
                        extra = true
                    };
                    members2.Add(member2);
                }
            }
        }
        if (root)
        {
            long magic = MagicNumber(t);
            data.Write(magic);
            data.Write(0u);
        }
        data.Write(ChunkVersion(t));
        if (ChunkVersion(t) == 2)
        {
            if (GetExtraHeaderInfo() != null)
            {
                data.Write(val: true);
                data.Write(ModLoader.SmallTypeName(GetExtraHeaderInfo().GetType()));
                GetExtraHeaderInfo().Serialize(data, root: false);
            }
            else
            {
                data.Write(val: false);
            }
        }
        int sizeOffset = data.position;
        data.Write(0u);
        data.Write((ushort)members2.Count);
        foreach (BinaryClassMember member3 in members2)
        {
            data.Write(member3.name);
            byte typeByte = 0;
            if (member3.extra)
            {
                if (member3.data == null)
                {
                    typeByte = byte.MaxValue;
                    data.Write(typeByte);
                }
                else
                {
                    Type memberType = member3.data.GetType();
                    if (BinaryClassMember.typeMap.TryGetValue(memberType, out typeByte))
                    {
                        typeByte = (byte)((typeByte << 1) | 1);
                    }
                    data.Write(typeByte);
                    if (typeByte == 0)
                    {
                        data.Write(ModLoader.SmallTypeName(memberType));
                    }
                }
            }
            if (typeByte == byte.MaxValue)
            {
                continue;
            }
            int memberSizeOffset = data.position;
            data.Write(0u);
            if (member3.data is BinaryClassChunk)
            {
                if ((member3.data as BinaryClassChunk).ignore)
                {
                    continue;
                }
                (member3.data as BinaryClassChunk).Serialize(data, root: false);
            }
            else if (member3.data is Array)
            {
                SerializeArray(member3.data as Array, member3.data.GetType().GetElementType(), data);
            }
            else if (member3.data.GetType().IsGenericType && member3.data.GetType().GetGenericTypeDefinition() == typeof(List<>))
            {
                IList obj = member3.data as IList;
                Array a = new object[obj.Count];
                obj.CopyTo(a, 0);
                SerializeArray(a, member3.data.GetType().GetGenericArguments()[0], data);
            }
            else if (member3.data.GetType().IsGenericType && member3.data.GetType().GetGenericTypeDefinition() == typeof(HashSet<>))
            {
                IEnumerable obj2 = member3.data as IEnumerable;
                List<object> temp = new List<object>();
                foreach (object item in obj2)
                {
                    temp.Add(item);
                }
                object[] a2 = new object[temp.Count];
                temp.CopyTo(a2, 0);
                SerializeArray(a2, member3.data.GetType().GetGenericArguments()[0], data);
            }
            else
            {
                data.Write(member3.data);
            }
            int memberSizeEnd = data.position;
            data.position = memberSizeOffset;
            data.Write((uint)(memberSizeEnd - memberSizeOffset - 4));
            data.position = memberSizeEnd;
        }
        int endOffset = data.position;
        data.position = sizeOffset;
        data.Write((uint)(endOffset - sizeOffset - 4));
        if (root)
        {
            _checksum = Editor.Checksum(data.buffer);
            data.position = 8;
            data.Write(_checksum);
        }
        data.position = endOffset;
        return data;
    }

    public static long MagicNumber<T>()
    {
        return MagicNumber(typeof(T));
    }

    public static long MagicNumber(Type t)
    {
        object[] attributes = t.GetCustomAttributes(typeof(MagicNumberAttribute), inherit: true);
        if (attributes.Length != 0)
        {
            return (attributes[0] as MagicNumberAttribute).magicNumber;
        }
        return 0L;
    }

    public static ushort ChunkVersion<T>()
    {
        return ChunkVersion(typeof(T));
    }

    public static ushort ChunkVersion(Type t)
    {
        object[] attributes = t.GetCustomAttributes(typeof(ChunkVersionAttribute), inherit: true);
        if (attributes.Length != 0)
        {
            return (attributes[0] as ChunkVersionAttribute).version;
        }
        return 0;
    }
}
