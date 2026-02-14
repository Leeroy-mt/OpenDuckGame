using Microsoft.Xna.Framework;

namespace DuckGame;

public class NCError
{
    public string text;

    public NCErrorType type;

    public Color color => type switch
    {
        NCErrorType.Success => Color.Lime,
        NCErrorType.Message => Color.White,
        NCErrorType.Warning => Color.Yellow,
        NCErrorType.Debug => Color.LightPink,
        _ => Color.Red,
    };

    public NCError(string s, NCErrorType tp)
    {
        text = s;
        type = tp;
    }
}
