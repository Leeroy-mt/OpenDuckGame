namespace DuckGame;

public class MaterialPersona : AutoEffect
{
    private DuckPersona persona;

    public MaterialPersona(DuckPersona pPersona) : base(Content.Load<MTEffect>("Shaders/recolor_duo"))
    {
        persona = pPersona;
    }

    public override void Update()
    {
    }

    public override void Apply()
    {
        Parameters["replace1"].SetValue(persona.color / 255f);
        Parameters["replace2"].SetValue(persona.colorDark / 255f);
        base.Apply();
    }
}
