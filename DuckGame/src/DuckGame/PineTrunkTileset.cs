namespace DuckGame;

[EditorGroup("Blocks|Snow")]
[BaggedProperty("isInDemo", false)]
public class PineTrunkTileset : AutoPlatform
{
	public PineTrunkTileset(float x, float y)
		: base(x, y, "pineTreeTileset")
	{
		_editorName = "Pine Trunk";
		physicsMaterial = PhysicsMaterial.Default;
		verticalWidth = 6f;
		verticalWidthThick = 15f;
		horizontalHeight = 8f;
		_hasNubs = false;
		base.depth = -0.6f;
		placementLayerOverride = Layer.Blocks;
	}

	public override void OnSolidImpact(MaterialThing with, ImpactedFrom from)
	{
		if (with.impactPowerV > 2.4f)
		{
			Level.CheckPoint<PineTree>(base.x, base.y)?.KnockOffSnow(with.velocity, vertShake: true);
			Level.CheckPoint<PineTree>(base.x, base.y - 16f)?.KnockOffSnow(with.velocity, vertShake: true);
		}
		base.OnSoftImpact(with, from);
	}
}
