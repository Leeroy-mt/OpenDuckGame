using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace DuckGame;

public class NMAssignWin : NMEvent
{
    public List<Profile> profiles = new List<Profile>();

    public Profile theRealWinnerHere;

    protected string _sound = "scoreDing";

    public NMAssignWin(List<Profile> pProfiles, Profile pTheRealWinnerHere)
    {
        profiles = pProfiles;
        theRealWinnerHere = pTheRealWinnerHere;
    }

    public NMAssignWin()
    {
    }

    protected override void OnSerialize()
    {
        base.OnSerialize();
        _serializedData.Write((byte)profiles.Count);
        foreach (Profile pro in profiles)
        {
            _serializedData.WriteProfile(pro);
        }
    }

    public override void OnDeserialize(BitBuffer d)
    {
        base.OnDeserialize(d);
        byte num = d.ReadByte();
        for (int i = 0; i < num; i++)
        {
            profiles.Add(d.ReadProfile());
        }
    }

    public override void Activate()
    {
        SFX.Play(_sound, 0.8f);
        foreach (Profile p in profiles)
        {
            GameMode.lastWinners.Add(p);
            Profile winpro = ((theRealWinnerHere != null) ? theRealWinnerHere : p);
            if (p.duck != null)
            {
                PlusOne plusOne = new PlusOne(0f, 0f, winpro, temp: false, testMode: true);
                plusOne._duck = p.duck;
                plusOne.anchor = p.duck;
                plusOne.anchor.offset = new Vector2(0f, -16f);
                plusOne.Depth = 0.95f;
                Level.Add(plusOne);
            }
        }
        if (!(this is NMPlusOne))
        {
            GameMode.numMatchesPlayed++;
        }
    }
}
