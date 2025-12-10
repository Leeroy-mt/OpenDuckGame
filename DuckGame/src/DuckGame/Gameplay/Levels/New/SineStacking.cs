using System.Collections.Generic;

namespace DuckGame;

internal class SineStacking : Level
{
    public record struct Sine(float Offset, float Frequency)
    {
        public float Sin(float r) =>
            float.SinPi(r * Frequency + Offset);
    }

    float Ticks;

    public List<Sine> Sines = [];

    static SineStacking()
    {
        DevConsole.AddCommand(new("sine",
            [
                new CMD.String("offset"),
                new CMD.String("frequency")
            ],
            cmd =>
            {
                var offset = float.Parse(cmd.Arg<string>("offset"));
                var frequency = float.Parse(cmd.Arg<string>("frequency"));

                var sine = current as SineStacking;
                sine.Sines.Add(new(offset, frequency));
            }));
    }

    public override void Update()
    {
        Ticks += 0.001f;

        if (Keyboard.Pressed(Keys.F1))
            Sines.Clear();
        if (Keyboard.Pressed(Keys.F2))
            Sines.Add(new(Rando.Float(1), Rando.Float(16)));
        if (Keyboard.Pressed(Keys.F3))
            Sines.Add(new(0, 8));
    }

    public override void Draw()
    {
        Graphics.DrawString($"{Sines.Count}", new(4), Color.White);

        Graphics.DrawLine(new(0, camera.height / 2), new(camera.width, camera.height / 2), Color.White, 1, -1);

        if (Sines.Count < 1)
            return;

        const bool ADDITIVE = true;
        const int RESOLUTION = 100;
        const float AMPLITUDE = -16;

        var area = 0f;

        Vec2 pp = default;
        for (int i = 0; i < RESOLUTION + 1; i++)
        {
            var t = i / (float)RESOLUTION;
            var x = t * camera.width;
            var y = ADDITIVE ? 0 : Sines[0].Sin(t + Ticks);

            Vec2 dotSize = new(.5f);

            for (int j = 0; j < Sines.Count; j++)
            {
                var localSin = Sines[j].Sin(t + Ticks);
                if (ADDITIVE)
                    y += localSin / Sines.Count;
                else if (j > 0)
                    y *= localSin;

                    Vec2 localPoint = new(x, localSin * AMPLITUDE + camera.height / 2);
                Graphics.DrawRect(localPoint - dotSize, localPoint + dotSize, Color.Lime / 10);
            }

            Vec2 p = new(x, y * AMPLITUDE + camera.height / 2);

            area += float.Abs(y);

            if (i > 0)
                Graphics.DrawLine(p, pp, Color.Red, .5f, 0);
            pp = p;
        }

        Graphics.DrawString($"{area}", new(32, 4), Color.White);
    }
}
