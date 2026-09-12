using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace DuckGame;

public static class Persona
{
    static List<DuckPersona> personas;

    public static DuckPersona Duck1 => personas[0];

    public static DuckPersona Duck2 => personas[1];

    public static DuckPersona Duck3 => personas[2];

    public static DuckPersona Duck4 => personas[3];

    public static DuckPersona Duck5 => personas[4];

    public static DuckPersona Duck6 => personas[5];

    public static DuckPersona Duck7 => personas[6];

    public static DuckPersona Duck8 => personas[7];

    public static IEnumerable<DuckPersona> all => personas;

    public static int Number(DuckPersona p)
    {
        return personas.IndexOf(p);
    }

    public static void Initialize()
    {
        personas = [
            new DuckPersona(new Vector3(255, 255, 255)) { index = 0 },
            new DuckPersona(new Vector3(125, 125, 125)) { index = 1 },
            new DuckPersona(new Vector3(247, 224, 90)) { index = 2 },
            new DuckPersona(new Vector3(205, 107, 29)) { index = 3 },
            new DuckPersona(new Vector3(0, 133, 74), new Vector3(0, 102, 57), new Vector3(0, 173, 97)) { index = 4 },
            new DuckPersona(new Vector3(255, 105, 117), new Vector3(207, 84, 94), new Vector3(255, 158, 166)) { index = 5 },
            new DuckPersona(new Vector3(49, 162, 242), new Vector3(13, 123, 181), new Vector3(148, 207, 245)) { index = 6 },
            new DuckPersona(new Vector3(175, 85, 221), new Vector3(141, 36, 194), new Vector3(213, 165, 238)) { index = 7 }
            ];
    }
}