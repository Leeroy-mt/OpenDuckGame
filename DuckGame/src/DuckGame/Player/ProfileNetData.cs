using System;
using System.Collections.Generic;

namespace DuckGame;

public class ProfileNetData
{
    public class NetDataPair
    {
        public NetIndex16 lastReceivedIndex = 1;

        public NetworkConnection activeControllingConnection;

        public Dictionary<NetworkConnection, NetIndex16> lastSyncIndex = new Dictionary<NetworkConnection, NetIndex16>();

        public bool filtered;

        public string id;

        public object data;

        public Dictionary<NetworkConnection, bool> dirtyConnections = new Dictionary<NetworkConnection, bool>();

        public NetIndex16 GetLastSyncIndex(NetworkConnection pConnection)
        {
            if (!lastSyncIndex.ContainsKey(pConnection))
            {
                return 0;
            }
            return lastSyncIndex[pConnection];
        }

        public void SetConnectionDirty(NetworkConnection pConnection, bool pValue)
        {
            dirtyConnections[pConnection] = pValue;
        }

        public bool IsDirty(NetworkConnection pConnection)
        {
            if (filtered && pConnection.profile != null && pConnection.profile.muteChat)
            {
                return false;
            }
            bool connectionDirty = true;
            if (dirtyConnections.ContainsKey(pConnection))
            {
                connectionDirty = dirtyConnections[pConnection];
            }
            return connectionDirty;
        }

        public void MakeDirty()
        {
            foreach (NetworkConnection c in Network.connections)
            {
                dirtyConnections[c] = true;
            }
        }

        public void Clean(NetworkConnection pConnection)
        {
            dirtyConnections[pConnection] = true;
            lastSyncIndex[pConnection] = 0;
        }
    }

    public Dictionary<NetworkConnection, NetIndex16> syncIndex = new Dictionary<NetworkConnection, NetIndex16>();

    private Dictionary<int, NetDataPair> _elements = new Dictionary<int, NetDataPair>();

    private bool _settingFiltered;

    public NetIndex16 GetAndIncrementSyncIndex(NetworkConnection pConnection)
    {
        if (!syncIndex.ContainsKey(pConnection))
        {
            syncIndex[pConnection] = 0;
        }
        syncIndex[pConnection]++;
        return syncIndex[pConnection];
    }

    public NetIndex16 GetSyncIndex(NetworkConnection pConnection)
    {
        if (!syncIndex.ContainsKey(pConnection))
        {
            return 0;
        }
        return syncIndex[pConnection];
    }

    /// <summary>
    /// This is just for iterating, Don't go modifying it.
    /// </summary>
    /// <returns></returns>
    public Dictionary<int, NetDataPair> GetElementList()
    {
        return _elements;
    }

    public bool IsDirty(NetworkConnection pConnection)
    {
        foreach (KeyValuePair<int, NetDataPair> element in _elements)
        {
            if (element.Value.IsDirty(pConnection))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Returns a property value based on a string. These values are synchronized over the network!
    /// </summary>
    /// <typeparam name="T">The type of the value you're getting.</typeparam>
    /// <param name="pKey">The name of the property you're getting.</param>
    /// <returns></returns>
    public T Get<T>(string pKey)
    {
        return Get(pKey, default(T));
    }

    /// <summary>
    /// Returns a property value based on a string. These values are synchronized over the network!
    /// </summary>
    /// <typeparam name="T">The type of the value you're getting.</typeparam>
    /// <param name="pKey">The name of the property you're getting.</param>
    /// <param name="pDefault">The value to return if no key is found.</param>
    /// <returns></returns>
    public T Get<T>(string pKey, T pDefault)
    {
        int hash = pKey.GetHashCode();
        NetDataPair o = null;
        if (_elements.TryGetValue(hash, out o) && o.data is T)
        {
            return (T)o.data;
        }
        return pDefault;
    }

    /// <summary>
    /// Set a property to a value. This property will be synchronized over the network
    /// and accessible from this profile on other computers!
    /// </summary>
    /// <typeparam name="T">The type of the value you're setting</typeparam>
    /// <param name="pKey">A unique name for the property. This runs through string.GetHashCode, so try to make it pretty unique.</param>
    /// <param name="pValue">The value!</param>
    public void Set<T>(string pKey, T pValue)
    {
        int hash = pKey.GetHashCode();
        NetDataPair o = null;
        if (!_elements.TryGetValue(hash, out o))
        {
            o = (_elements[hash] = new NetDataPair());
            o.MakeDirty();
            if (_settingFiltered)
            {
                o.filtered = true;
            }
        }
        if (o.id != null && o.id != pKey)
        {
            throw new Exception("Profile.netData.Set<" + typeof(T).Name + ">(" + pKey + ") error: GetHashCode for (" + pKey + ") is identical to GetHashCode for (" + o.id + "), a value already set in Profile.netData! Please use a more unique key name.");
        }
        if (!object.Equals(o.data, pValue) || o.activeControllingConnection != DuckNetwork.localConnection)
        {
            o.data = pValue;
            o.id = pKey;
            o.MakeDirty();
        }
    }

    public void SetFiltered<T>(string pKey, T pValue)
    {
        _settingFiltered = true;
        Set(pKey, pValue);
        _settingFiltered = false;
    }

    public void MakeDirty(int pHash, NetworkConnection pConnection, NetIndex16 pSyncIndex)
    {
        if (pHash == int.MaxValue)
        {
            foreach (KeyValuePair<int, NetDataPair> element in _elements)
            {
                element.Value.Clean(pConnection);
            }
            return;
        }
        NetDataPair o = null;
        if (_elements.TryGetValue(pHash, out o) && (int)o.GetLastSyncIndex(pConnection) <= (int)pSyncIndex)
        {
            o.SetConnectionDirty(pConnection, pValue: true);
        }
    }

    public void Clean(NetworkConnection pConnection)
    {
        foreach (KeyValuePair<int, NetDataPair> element in _elements)
        {
            element.Value.SetConnectionDirty(pConnection, pValue: false);
        }
    }

    internal void Set(int pHash, object pValue, NetIndex16 pSyncIndex, NetworkConnection pConnection)
    {
        NetDataPair o = null;
        if (!_elements.TryGetValue(pHash, out o))
        {
            o = (_elements[pHash] = new NetDataPair());
        }
        if (!(o.lastReceivedIndex > pSyncIndex) || o.activeControllingConnection != pConnection)
        {
            o.lastReceivedIndex = pSyncIndex;
            o.activeControllingConnection = pConnection;
            o.data = pValue;
        }
    }

    public BitBuffer Serialize(NetworkConnection pConnection, HashSet<int> pOutputHashlist)
    {
        BitBuffer b = new BitBuffer();
        NetIndex16 sync = GetAndIncrementSyncIndex(pConnection);
        b.Write((object)sync);
        foreach (KeyValuePair<int, NetDataPair> pair in _elements)
        {
            if (pair.Value.IsDirty(pConnection))
            {
                b.Write(pair.Key);
                b.WriteObject(pair.Value.data);
                pair.Value.SetConnectionDirty(pConnection, pValue: false);
                pair.Value.lastSyncIndex[pConnection] = sync;
                pOutputHashlist.Add(pair.Key);
                pair.Value.activeControllingConnection = DuckNetwork.localConnection;
            }
        }
        return b;
    }

    public void Deserialize(BitBuffer pBuffer, NetworkConnection pConnection, bool pMakingDirty)
    {
        NetIndex16 syncIndex = pBuffer.ReadNetIndex16();
        while (pBuffer.positionInBits != pBuffer.lengthInBits)
        {
            int hash = pBuffer.ReadInt();
            Type dataType = null;
            object data = pBuffer.ReadObject(out dataType);
            if (!pMakingDirty)
            {
                Set(hash, data, syncIndex, pConnection);
            }
            else
            {
                MakeDirty(hash, pConnection, 0);
            }
        }
    }
}
