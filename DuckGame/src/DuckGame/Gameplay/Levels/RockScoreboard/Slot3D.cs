using System.Collections.Generic;

namespace DuckGame;

public class Slot3D
{
    public RockThrow state;

    public Duck duck;

    public List<Duck> subDucks = new List<Duck>();

    public List<DuckAI> subAIs = new List<DuckAI>();

    public ScoreRock rock;

    public int slideWait;

    public DuckAI ai;

    public int slotIndex;

    public float startX;

    public bool follow;

    public bool showScore;

    public bool highestScore;

    public float scroll
    {
        get
        {
            if (slotIndex == 0)
            {
                return (0f - duck.position.x) * 0.665f + 100f;
            }
            if (slotIndex == 1)
            {
                return (0f - duck.position.x) * 0.665f + 100f;
            }
            _ = slotIndex;
            _ = 2;
            return (0f - duck.position.x) * 0.665f + 100f;
        }
    }
}
