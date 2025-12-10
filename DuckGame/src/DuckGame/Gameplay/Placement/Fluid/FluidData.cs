namespace DuckGame;

public struct FluidData
{
    public float amount;

    public Vec4 color;

    public float flammable;

    public string sprite;

    public float heat;

    public float transparent;

    public float douseFire;

    public FluidData(float am, Vec4 col, float flam, string spr = "", float h = 0f, float trans = 0.7f)
    {
        amount = am;
        color = col;
        flammable = flam;
        sprite = spr;
        heat = h;
        transparent = trans;
        douseFire = 1f;
    }

    public void Mix(FluidData with)
    {
        float newAmount = with.amount + amount;
        if (with.amount > 0f)
        {
            float myRatio = amount / newAmount;
            float yourRatio = with.amount / newAmount;
            flammable = myRatio * flammable + yourRatio * with.flammable;
            color = color * myRatio + with.color * yourRatio;
            heat = heat * myRatio + with.heat * yourRatio;
            transparent = transparent * myRatio + with.transparent * yourRatio;
            douseFire = douseFire * myRatio + with.douseFire * yourRatio;
        }
        amount = newAmount;
    }

    public FluidData Take(float val)
    {
        if (val > amount)
        {
            val = amount;
        }
        amount -= val;
        FluidData newData = this;
        newData.amount = val;
        return newData;
    }
}
