namespace DuckGame;

public class SN76489
{
    private SN76489Core _chip;

    public SN76489()
    {
        _chip = new SN76489Core();
    }

    public void Initialize(double clock)
    {
        _chip.clock((float)clock);
    }

    public void Update(int[] buffer, int length)
    {
        for (int i = 0; i < length; i++)
        {
            buffer[i * 2 + 1] = (buffer[i * 2] = (short)(_chip.render() * 8000f));
        }
    }

    public void Write(int value)
    {
        _chip.write(value);
    }
}
