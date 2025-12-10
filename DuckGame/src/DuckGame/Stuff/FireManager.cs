namespace DuckGame;

public class FireManager
{
    private static int _curFireID;

    private static int _curUpdateID;

    private static int _curSmokeID;

    public static int GetFireID()
    {
        _curFireID++;
        if (_curFireID > 20)
        {
            _curFireID = 0;
        }
        return _curFireID;
    }

    public static int GetSmokeID()
    {
        _curSmokeID++;
        if (_curSmokeID > 20)
        {
            _curSmokeID = 0;
        }
        return _curSmokeID;
    }

    public static void Update()
    {
        foreach (SmallFire f in Level.current.things[typeof(SmallFire)])
        {
            if (f.y < -2000f || f.fireID != _curUpdateID || !(f.alpha > 0.5f))
            {
                continue;
            }
            Thing dontBurn = null;
            if (f.stick != null && (f.stick is DartGun || f.stick is Chaindart))
            {
                dontBurn = f.stick.owner;
            }
            f.doFloat = false;
            foreach (MaterialThing t in Level.CheckCircleAll<MaterialThing>(f.position + new Vec2(0f, -4f), 6f))
            {
                if (t == dontBurn)
                {
                    continue;
                }
                t.DoHeatUp(0.05f, f.position);
                if (!t.isServerForObject)
                {
                    continue;
                }
                if (t is FluidPuddle)
                {
                    f.doFloat = true;
                    FluidPuddle p = t as FluidPuddle;
                    if (p.data.flammable <= 0.5f && p.data.heat < 0.5f && p.data.douseFire > 0.5f)
                    {
                        Level.Remove(f);
                        break;
                    }
                }
                Duck d = t as Duck;
                if (d != null && ((d.slideBuildup > 0f && d.sliding) || (d.holdObject is Sword && (d.holdObject as Sword)._slamStance)))
                {
                    SmallSmoke smallSmoke = SmallSmoke.New(f.x + Rando.Float(-1f, 1f), f.y + Rando.Float(-1f, 1f));
                    smallSmoke.vSpeed -= Rando.Float(0.1f, 0.2f);
                    Level.Add(smallSmoke);
                    Level.Remove(f);
                }
                else if (Rando.Float(1000f) < t.flammable * 1000f && (f.whoWait == null || d != f.whoWait))
                {
                    t.Burn(f.position + new Vec2(0f, 4f), f);
                }
            }
        }
        foreach (FluidPuddle f2 in Level.current.things[typeof(FluidPuddle)])
        {
            if (f2.data.flammable <= 0.5f)
            {
                f2.onFire = false;
            }
            else if (f2.onFire && f2.fireID == (float)_curUpdateID && f2.alpha > 0.5f)
            {
                foreach (MaterialThing t2 in Level.CheckRectAll<MaterialThing>(f2.topLeft + new Vec2(0f, -4f), f2.topRight + new Vec2(0f, 2f)))
                {
                    if (t2 != f2 && t2.isServerForObject)
                    {
                        if (!(t2 is Duck { slideBuildup: > 0f }) && Rando.Float(1000f) < t2.flammable * 1000f)
                        {
                            t2.Burn(f2.position + new Vec2(0f, 4f), f2);
                        }
                        t2.DoHeatUp(0.05f, f2.position);
                    }
                }
            }
            else
            {
                if (f2.onFire)
                {
                    continue;
                }
                Rectangle r = f2.rectangle;
                foreach (Spark s in Level.current.things[typeof(Spark)])
                {
                    if (s.x > r.x && s.x < r.x + r.width && s.y > r.y && s.y < r.y + r.height)
                    {
                        f2.Burn(f2.position, s);
                        break;
                    }
                }
            }
        }
        foreach (ExtinguisherSmoke f3 in Level.current.things[typeof(ExtinguisherSmoke)])
        {
            if (f3.smokeID != _curUpdateID)
            {
                continue;
            }
            foreach (SmallFire t3 in Level.CheckCircleAll<SmallFire>(f3.position + new Vec2(0f, -8f), 12f))
            {
                if (f3.scale.x > 1f)
                {
                    t3.SuckLife(10f);
                }
            }
            foreach (MaterialThing t4 in Level.CheckCircleAll<MaterialThing>(f3.position + new Vec2(0f, -8f), 4f))
            {
                if (f3.scale.x > 1f)
                {
                    t4.spreadExtinguisherSmoke = 1f;
                }
                if (t4.physicsMaterial == PhysicsMaterial.Metal)
                {
                    t4.DoFreeze(0.03f, f3.position);
                }
                if (t4.onFire && Rando.Float(1000f) > t4.flammable * 650f)
                {
                    t4.Extinquish();
                }
            }
        }
        _curUpdateID++;
        if (_curUpdateID > 20)
        {
            _curUpdateID = 0;
        }
    }
}
