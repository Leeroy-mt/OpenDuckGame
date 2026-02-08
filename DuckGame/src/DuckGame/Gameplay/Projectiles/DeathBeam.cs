using Microsoft.Xna.Framework;

namespace DuckGame;

public class DeathBeam : Thing
{
    public float _blast = 1f;

    private Vector2 _target;

    private Thing _blastOwner;

    public DeathBeam(Vector2 pos, Vector2 target, Thing blastOwner)
        : base(pos.X, pos.Y)
    {
        _target = target;
        _blastOwner = blastOwner;
    }

    public DeathBeam(Vector2 pos, Vector2 target)
        : base(pos.X, pos.Y)
    {
        _target = target;
    }

    public override void Initialize()
    {
        Vector2 upVec = Vector2.Normalize(_target.Rotate(Maths.DegToRad(-90f), Vector2.Zero));
        Vector2 downVec = Vector2.Normalize(_target.Rotate(Maths.DegToRad(90f), Vector2.Zero));
        Level.Add(new LaserLine(Position, _target, upVec, 4f, Color.White, 1f));
        Level.Add(new LaserLine(Position, _target, downVec, 4f, Color.White, 1f));
        Level.Add(new LaserLine(Position, _target, upVec, 2.5f, Color.White, 2f));
        Level.Add(new LaserLine(Position, _target, downVec, 2.5f, Color.White, 2f));
        if (isLocal)
        {
            int ducks = 0;
            Vector2 checkStart = Position + upVec * 16f;
            for (int i = 0; i < 5; i++)
            {
                Vector2 vec = checkStart + downVec * 8f * i;
                foreach (MaterialThing thing in Level.CheckLineAll<MaterialThing>(vec, vec + _target))
                {
                    if (_blastOwner == thing || Duck.GetAssociatedDuck(thing) == _blastOwner)
                    {
                        continue;
                    }
                    if (thing is IAmADuck && !thing.destroyed && !(thing is TargetDuck))
                    {
                        if (thing is RagdollPart && (thing as RagdollPart).doll != null && (thing as RagdollPart).doll.captureDuck != null && !(thing as RagdollPart).doll.captureDuck.dead)
                        {
                            ducks++;
                        }
                        if (thing is TrappedDuck && (thing as TrappedDuck).captureDuck != null && !(thing as TrappedDuck).captureDuck.dead)
                        {
                            ducks++;
                        }
                        if (thing is Duck && !(thing as Duck).dead)
                        {
                            ducks++;
                        }
                    }
                    Thing.SuperFondle(thing, DuckNetwork.localConnection);
                    thing.Destroy(new DTIncinerate(this));
                }
            }
            if (ducks > 2)
            {
                Global.GiveAchievement("laser");
            }
            Global.data.giantLaserKills += ducks;
        }
        if (Recorder.currentRecording != null)
        {
            Recorder.currentRecording.LogBonus();
        }
    }

    public override void Update()
    {
        _blast = Maths.CountDown(_blast, 0.1f);
        if (_blast < 0f)
        {
            Level.Remove(this);
        }
    }

    public override void Draw()
    {
        Maths.NormalizeSection(_blast, 0f, 0.2f);
        Maths.NormalizeSection(_blast, 0.6f, 1f);
        _ = _blast;
        _ = 0f;
        Vector2 upVec = Vector2.Normalize(_target.Rotate(Maths.DegToRad(-90f), Vector2.Zero));
        Vector2 downVec = Vector2.Normalize(_target.Rotate(Maths.DegToRad(90f), Vector2.Zero));
        Vector2 checkStart = Position + upVec * 16f;
        for (int i = 0; i < 5; i++)
        {
            Vector2 vec = checkStart + downVec * 8f * i;
            Graphics.DrawLine(vec, vec + _target, Color.Red * (_blast * 0.5f), 2f, 0.9f);
        }
    }
}
