using System.Collections.Generic;
using System.Linq;

namespace DuckGame;

[FixedNetworkID(30232)]
public class NMInputSettings : NMDuckNetworkEvent
{
    private InputDevice _device;

    private InputProfile _inputProfile;

    private Profile _profile;

    private DeviceInputMapping _mapping;

    private byte _index;

    private bool _valid = true;

    public NMInputSettings()
    {
    }

    public NMInputSettings(Profile pro, InputDevice d = null)
    {
        _device = ((d != null) ? d : pro.inputProfile.lastActiveDevice);
        if (_device != null && _device.productName == null && _device.productGUID == null)
        {
            _device = null;
        }
        _index = pro.networkIndex;
        _profile = pro;
        _inputProfile = pro.inputProfile;
    }

    protected override void OnSerialize()
    {
        if (_serializedData == null || _device == null || _inputProfile == null)
        {
            _serializedData.Write(val: false);
            return;
        }
        _serializedData.Write(val: true);
        _serializedData.Write(_index);
        _serializedData.Write(_device.inputDeviceType);
        MultiMap<string, int> vals = null;
        if (_device != null)
        {
            vals = _inputProfile.GetMappings(_device.GetType());
        }
        if (vals != null)
        {
            base.serializedData.Write(val: true);
            byte count = 0;
            foreach (KeyValuePair<string, List<int>> pair in vals)
            {
                if (pair.Value.Count > 0 && Triggers.toIndex.ContainsKey(pair.Key))
                {
                    count++;
                }
            }
            base.serializedData.Write(count);
            foreach (KeyValuePair<string, List<int>> pair2 in vals)
            {
                if (pair2.Value.Count > 0 && Triggers.toIndex.ContainsKey(pair2.Key))
                {
                    base.serializedData.Write(Triggers.toIndex[pair2.Key]);
                    base.serializedData.Write(pair2.Value[0]);
                }
            }
            DeviceInputMapping map = null;
            map = ((_device.overrideMap == null) ? Input.GetDefaultMapping(_device.productName, _device.productGUID, presets: false, makeClone: true, _profile) : _device.overrideMap);
            if (map.graphicMap.Count > 0)
            {
                base.serializedData.Write(val: true);
                base.serializedData.Write((byte)map.graphicMap.Count);
                foreach (KeyValuePair<int, string> pair3 in map.graphicMap)
                {
                    Sprite spr = Input.buttonStyles.FirstOrDefault((Sprite x) => x.texture != null && x.texture.textureName == pair3.Value);
                    byte idx = 0;
                    if (spr != null)
                    {
                        idx = (byte)Input.buttonStyles.IndexOf(spr);
                    }
                    base.serializedData.Write(pair3.Key);
                    base.serializedData.Write(idx);
                }
            }
            else
            {
                base.serializedData.Write(val: false);
            }
        }
        else
        {
            base.serializedData.Write(val: false);
        }
        base.OnSerialize();
    }

    public override void OnDeserialize(BitBuffer msg)
    {
        if (!msg.ReadBool())
        {
            _valid = false;
            return;
        }
        _index = msg.ReadByte();
        byte inputType = msg.ReadByte();
        DeviceInputMapping map = new DeviceInputMapping();
        map.inputOverrideType = inputType;
        switch (inputType)
        {
            case 0:
                map.deviceOverride = new Keyboard("", 0);
                break;
            case 1:
                map.deviceOverride = new XInputPad(0);
                break;
            default:
                map.deviceOverride = new DInputPad(0);
                break;
        }
        map.deviceOverride.overrideMap = map;
        if (msg.ReadBool())
        {
            byte count = msg.ReadByte();
            for (int i = 0; i < count; i++)
            {
                byte idx = msg.ReadByte();
                int val = msg.ReadInt();
                map.MapInput(Triggers.fromIndex[idx], val);
            }
            if (msg.ReadBool())
            {
                count = msg.ReadByte();
                for (int j = 0; j < count; j++)
                {
                    int idx2 = msg.ReadInt();
                    int val2 = msg.ReadByte();
                    map.graphicMap[idx2] = Input.buttonStyles[val2].texture.textureName;
                }
            }
        }
        _mapping = map;
        base.OnDeserialize(msg);
    }

    public override void Activate()
    {
        if (_index < 0 || _index > 3 || !_valid)
        {
            return;
        }
        Profile p = DuckNetwork.profiles[_index];
        p.inputMappingOverrides.RemoveAll((DeviceInputMapping x) => x.inputOverrideType == _mapping.inputOverrideType);
        p.inputMappingOverrides.Add(_mapping);
        foreach (KeyValuePair<string, int> pair in _mapping.map)
        {
            p.inputProfile.Map(_mapping.deviceOverride, pair.Key, pair.Value, clearExisting: true);
        }
        p.inputProfile.lastActiveOverride = _mapping.deviceOverride;
        base.Activate();
    }
}
