namespace DuckGame;

public abstract class Goody : MaterialThing, ISequenceItem
{
    public string collectSound = "goody";

    public bool hidden;

    public Goody(float xpos, float ypos, Sprite sprite)
        : base(xpos, ypos)
    {
        graphic = sprite;
        Center = new Vec2(sprite.w / 2, sprite.h / 2);
        _collisionSize = new Vec2(10f, 10f);
        collisionOffset = new Vec2(-5f, -5f);
        base.sequence = new SequenceItem(this);
        base.sequence.type = SequenceItemType.Goody;
        enablePhysics = false;
        _impactThreshold = 1E-06f;
    }

    public override void Initialize()
    {
        if (!(Level.current is Editor) && base.sequence.waitTillOrder && base.sequence.order != 0)
        {
            visible = false;
            hidden = true;
        }
        base.Initialize();
    }

    public override void OnSequenceActivate()
    {
        if (base.sequence.waitTillOrder)
        {
            if (_visibleInGame)
            {
                visible = true;
            }
            hidden = false;
        }
        base.OnSequenceActivate();
    }

    public override void OnSoftImpact(MaterialThing with, ImpactedFrom from)
    {
        if (hidden || (!(with is Duck) && !(with is RagdollPart) && !(with is TrappedDuck)) || with.destroyed)
        {
            return;
        }
        visible = false;
        hidden = true;
        if (collectSound != null && collectSound != "")
        {
            SFX.Play(collectSound, 0.8f);
        }
        if (_visibleInGame)
        {
            Profile duckProfile = null;
            if (with is Duck)
            {
                duckProfile = (with as Duck).profile;
            }
            else if (with is RagdollPart)
            {
                if ((with as RagdollPart).doll != null && (with as RagdollPart).doll._duck != null)
                {
                    duckProfile = (with as RagdollPart).doll._duck.profile;
                }
            }
            else if (with is TrappedDuck)
            {
                duckProfile = (with as TrappedDuck)._duckOwner.profile;
            }
            if (duckProfile != null)
            {
                RumbleManager.AddRumbleEvent(duckProfile, new RumbleEvent(RumbleIntensity.Kick, RumbleDuration.Pulse, RumbleFalloff.Short));
            }
        }
        if (!(Level.current is Editor))
        {
            _sequence.Finished();
            if (ChallengeLevel.running && base.sequence.isValid)
            {
                ChallengeLevel.goodiesGot++;
            }
        }
    }
}
