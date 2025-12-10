namespace DuckGame;

[FixedNetworkID(29422)]
public class NMInputDeviceSwitch : NMDuckNetworkEvent
{
    public byte index;

    public byte inputType;

    public NMInputDeviceSwitch()
    {
    }

    public NMInputDeviceSwitch(byte idx, byte inpType)
    {
        index = idx;
        inputType = inpType;
    }

    public override void Activate()
    {
        if (index < 0 || index > 3)
        {
            return;
        }
        Profile p = DuckNetwork.profiles[index];
        if (p != null && p.inputProfile != null)
        {
            foreach (DeviceInputMapping m in p.inputMappingOverrides)
            {
                if (m.inputOverrideType == inputType && m.deviceOverride != null)
                {
                    p.inputProfile.lastActiveOverride = m.deviceOverride;
                    break;
                }
            }
        }
        base.Activate();
    }
}
