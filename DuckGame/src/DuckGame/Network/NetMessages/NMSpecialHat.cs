namespace DuckGame;

public class NMSpecialHat : NMDuckNetwork
{
    private Team _team;

    private byte[] _data;

    public Profile profile;

    public ushort customTeamIndex;

    public bool filtered;

    public byte[] GetData()
    {
        return _data;
    }

    public NMSpecialHat(Team pTeam, Profile pProfile, bool pFiltered)
    {
        _team = pTeam;
        profile = pProfile;
        customTeamIndex = (ushort)Teams.core.extraTeams.IndexOf(pTeam);
        filtered = pFiltered;
    }

    public NMSpecialHat(Team pTeam, Profile pProfile)
    {
        _team = pTeam;
        profile = pProfile;
        customTeamIndex = (ushort)Teams.core.extraTeams.IndexOf(pTeam);
    }

    public NMSpecialHat()
    {
    }

    protected override void OnSerialize()
    {
        if (_team != null)
        {
            base.serializedData.Write(val: true);
            base.serializedData.Write(customTeamIndex);
            base.serializedData.Write((byte)(filtered ? 1u : 0u));
            if (!filtered)
            {
                BitBuffer b = new BitBuffer(_team.customData);
                base.serializedData.Write(b);
            }
        }
        else
        {
            base.serializedData.Write(val: false);
        }
    }

    public override void OnDeserialize(BitBuffer data)
    {
        if (data.ReadBool())
        {
            customTeamIndex = data.ReadUShort();
            filtered = data.ReadByte() == 1;
            if (!filtered)
            {
                BitBuffer b = data.ReadBitBuffer();
                _data = b.GetBytes();
            }
        }
    }
}
