namespace DuckGame;

public class StartLight : Thing
{
	private SpriteMap _sprite;

	public StartLight()
	{
		_sprite = new SpriteMap("trafficLight", 42, 23);
		center = new Vec2(_sprite.w / 2, _sprite.h / 2);
		graphic = _sprite;
		base.layer = Layer.HUD;
		base.x = Layer.HUD.camera.width / 2f;
		base.y = 20f;
	}

	public override void Update()
	{
	}
}
