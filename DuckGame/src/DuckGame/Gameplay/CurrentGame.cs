namespace DuckGame;

public class CurrentGame
{
    private int _kills;

    private int _cash;

    public int kills
    {
        get
        {
            return _kills;
        }
        set
        {
            _kills = value;
        }
    }

    public int cash
    {
        get
        {
            return _cash;
        }
        set
        {
            _cash = value;
        }
    }
}
