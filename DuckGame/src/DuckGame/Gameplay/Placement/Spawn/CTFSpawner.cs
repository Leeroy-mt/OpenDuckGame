namespace DuckGame;

[EditorGroup("Special", EditorItemType.Arcade)]
public class CTFSpawner : ItemSpawner
{
    public EditorProperty<bool> team = new EditorProperty<bool>(val: false);

    private CTFPresent _present;

    public CTFSpawner(float xpos, float ypos)
        : base(xpos, ypos)
    {
        _sprite = new SpriteMap("ctf/spawner", 16, 4);
        graphic = _sprite;
        Center = new Vec2(8f, 2f);
        collisionOffset = new Vec2(-8f, -2f);
        collisionSize = new Vec2(16f, 4f);
        randomSpawn = true;
    }

    public override void SetHoverItem(Holdable hover)
    {
        if (hover == _present && base._hoverItem != hover)
        {
            base.SetHoverItem(hover);
        }
    }

    public override void Update()
    {
        spawnTime = 4f;
        if (_present != null && _present.removeFromLevel)
        {
            _present = null;
        }
        CTFPresent otherPres = null;
        CTFPresent yourPres = null;
        foreach (CTFPresent pres in Level.CheckCircleAll<CTFPresent>(Position, 16f))
        {
            if (pres != _present)
            {
                yourPres = pres;
            }
            else
            {
                otherPres = pres;
            }
        }
        if (yourPres != null && otherPres != null)
        {
            if (yourPres.duck != null)
            {
                yourPres.duck.ThrowItem();
            }
            Level.Remove(yourPres);
            Level.Add(SmallSmoke.New(yourPres.X, yourPres.Y));
            CTF.CaptureFlag(team);
            SFX.Play("equip");
        }
        base.Update();
    }

    public override void Draw()
    {
        _sprite.frame = ((!team) ? 1 : 0);
        base.Draw();
    }

    public override void SpawnItem()
    {
        if (_present == null)
        {
            _spawnWait = 0f;
            _present = new CTFPresent(base.X, base.Y, team);
            _present.X = base.X;
            _present.Y = base.top + (_present.Y - _present.bottom) - 6f;
            _present.vSpeed = -2f;
            Level.Add(_present);
            if (_seated)
            {
                SetHoverItem(_present);
            }
        }
    }
}
