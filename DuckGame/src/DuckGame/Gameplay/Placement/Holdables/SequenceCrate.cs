namespace DuckGame;

[EditorGroup("Special", EditorItemType.Arcade)]
[BaggedProperty("isInDemo", true)]
public class SequenceCrate : Crate, ISequenceItem
{
    public SequenceCrate(float xpos, float ypos)
        : base(xpos, ypos)
    {
        base.sequence = new SequenceItem(this);
        base.sequence.type = SequenceItemType.Goody;
        _editorName = "Seq Crate";
    }

    protected override bool OnDestroy(DestroyType type = null)
    {
        if (base.sequence != null && base.sequence.isValid)
        {
            base.sequence.Finished();
            if (ChallengeLevel.running)
            {
                ChallengeLevel.goodiesGot++;
            }
        }
        return base.OnDestroy(type);
    }
}
