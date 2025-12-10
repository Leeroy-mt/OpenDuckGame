using System.Linq;

namespace DuckGame;

public class NMSetPersona : NMEvent
{
    public Profile profile;

    public byte persona;

    public NMSetPersona()
    {
    }

    public NMSetPersona(Profile pProfile, DuckPersona pPersona)
    {
        profile = pProfile;
        persona = (byte)pPersona.index;
    }

    public override void Activate()
    {
        if (profile != null && persona >= 0 && persona < Persona.all.Count())
        {
            profile.persona = Persona.all.ElementAt(persona);
        }
    }
}
