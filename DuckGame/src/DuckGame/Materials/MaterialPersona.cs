namespace DuckGame;

public class MaterialPersona : Material
{
    private DuckPersona persona;

    public MaterialPersona(DuckPersona pPersona)
    {
        persona = pPersona;
        _effect = Content.Load<MTEffect>("Shaders/recolor_duo");
    }

    public override void Update()
    {
    }

    public override void Apply()
    {
        _effect.effect.Parameters["replace1"].SetValue(persona.color / 255f);
        _effect.effect.Parameters["replace2"].SetValue(persona.colorDark / 255f);
        base.Apply();
    }
}
