namespace DuckGame;

public class FeatherVolume : MaterialThing
{
    private Duck _duckOwner;

    public Duck duckOwner => _duckOwner;

    public FeatherVolume(Duck duckOwner)
        : base(0f, 0f)
    {
        thickness = 0.1f;
        _duckOwner = duckOwner;
        _editorCanModify = false;
        ignoreCollisions = true;
        visible = false;
    }

    public override bool Hit(Bullet bullet, Vec2 hitPos)
    {
        Gun gunOwner = bullet.owner as Gun;
        if (bullet.owner != null && (bullet.owner == _duckOwner || (gunOwner != null && gunOwner.owner == _duckOwner)))
        {
            return false;
        }
        Feather feather = Feather.New(0f, 0f, _duckOwner.persona);
        feather.hSpeed = (0f - bullet.travelDirNormalized.x) * (1f + Rando.Float(1f));
        feather.vSpeed = 0f - Rando.Float(2f);
        feather.position = hitPos;
        Level.Add(feather);
        Vec2 move = hitPos + bullet.travelDirNormalized * 3f;
        if (bullet.isLocal && _duckOwner.sliding && _duckOwner.ragdoll == null && move.x > base.left + 2f && move.x < base.right - 2f && move.y > base.top + 2f && move.y < base.bottom - 2f)
        {
            foreach (Equipment q in Level.CheckPointAll<Equipment>(move))
            {
                if (q is Helmet || q is ChestPlate)
                {
                    return false;
                }
            }
            _duckOwner.Kill(new DTShot(bullet));
        }
        return false;
    }

    public override void ExitHit(Bullet bullet, Vec2 exitPos)
    {
        Gun gunOwner = bullet.owner as Gun;
        if (bullet.owner == null || (bullet.owner != _duckOwner && (gunOwner == null || gunOwner.owner != _duckOwner)))
        {
            Feather feather = Feather.New(0f, 0f, _duckOwner.persona);
            feather.hSpeed = (0f - bullet.travelDirNormalized.x) * (1f + Rando.Float(1f));
            feather.vSpeed = 0f - Rando.Float(2f);
            feather.position = exitPos;
            Level.Add(feather);
        }
    }
}
