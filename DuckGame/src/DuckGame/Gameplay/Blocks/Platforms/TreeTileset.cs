namespace DuckGame;

[EditorGroup("Blocks|Jump Through")]
[BaggedProperty("isInDemo", true)]
public class TreeTileset : AutoPlatform
{
	public TreeTileset(float x, float y)
		: base(x, y, "treeTileset")
	{
		_editorName = "Tree";
		physicsMaterial = PhysicsMaterial.Default;
		verticalWidth = 6f;
		verticalWidthThick = 15f;
		horizontalHeight = 8f;
		_hasNubs = false;
		base.depth = -0.15f;
		placementLayerOverride = Layer.Blocks;
		treeLike = true;
	}
}
