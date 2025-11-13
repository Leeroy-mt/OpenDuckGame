using System;
using Microsoft.Xna.Framework.Graphics;

namespace DuckGame;

[EditorGroup("Stuff|Props")]
[BaggedProperty("isInDemo", false)]
public class IceBlock : Holdable, IPlatform
{
	public StateBinding _hitPointsBinding = new StateBinding("_hitPoints");

	public StateBinding _carvedBinding = new StateBinding("carved");

	public float carved;

	public bool didCarve;

	private SpriteMap _sprite;

	private Type _previewType;

	private Sprite _previewSprite;

	private float breakPoints = 15f;

	private Thing _containedThing;

	private float damageMultiplier;

	private MaterialFrozen _frozen;

	public Type contains { get; set; }

	public override NetworkConnection connection
	{
		get
		{
			return base.connection;
		}
		set
		{
			base.connection = value;
			if (base.isServerForObject)
			{
				Fondle(_containedThing);
			}
		}
	}

	public override ContextMenu GetContextMenu()
	{
		FieldBinding binding = new FieldBinding(this, "contains");
		EditorGroupMenu obj = base.GetContextMenu() as EditorGroupMenu;
		EditorGroupMenu contain = new EditorGroupMenu(obj);
		contain.InitializeGroups(new EditorGroup(typeof(Holdable)), binding);
		contain.text = "Contains";
		obj.AddItem(contain);
		return obj;
	}

	public override BinaryClassChunk Serialize()
	{
		BinaryClassChunk binaryClassChunk = base.Serialize();
		binaryClassChunk.AddProperty("contains", Editor.SerializeTypeName(contains));
		return binaryClassChunk;
	}

	public override bool Deserialize(BinaryClassChunk node)
	{
		base.Deserialize(node);
		contains = Editor.DeSerializeTypeName(node.GetProperty<string>("contains"));
		return true;
	}

	public override void EditorUpdate()
	{
		if (contains != null && (_previewSprite == null || _previewType != contains))
		{
			Thing previewThing = Editor.GetThing(contains);
			if (previewThing != null)
			{
				_previewSprite = previewThing.GeneratePreview(48, 48, transparentBack: true);
				_previewSprite.CenterOrigin();
			}
			_previewType = contains;
		}
		base.EditorUpdate();
	}

	public override void EditorRender()
	{
		if (contains != null && _previewSprite != null)
		{
			if (_frozen == null)
			{
				_frozen = new MaterialFrozen(this)
				{
					intensity = 1f
				};
			}
			Material obj = Graphics.material;
			Graphics.material = _frozen;
			_previewSprite.alpha = 0.5f;
			Graphics.Draw(_previewSprite, base.x, base.y, base.depth + 10);
			Graphics.material = obj;
		}
		base.EditorRender();
	}

	public IceBlock(float xpos, float ypos)
		: base(xpos, ypos)
	{
		_sprite = new SpriteMap("iceBlock", 16, 16);
		graphic = _sprite;
		center = new Vec2(8f, 8f);
		collisionOffset = new Vec2(-8f, -8f);
		collisionSize = new Vec2(16f, 16f);
		base.depth = -0.5f;
		_editorName = "Ice Block";
		editorTooltip = "Slippery, slidery, fun. Also great for keeping your (gigantic) drinks cold.";
		thickness = 2f;
		weight = 5f;
		buoyancy = 1f;
		_hitPoints = 1f;
		base.impactThreshold = -1f;
		physicsMaterial = PhysicsMaterial.Glass;
		_holdOffset = new Vec2(2f, 0f);
		flammable = 0f;
		base.collideSounds.Add("glassHit");
		superNonFlammable = true;
	}

	public override void Initialize()
	{
		base.Initialize();
		if (!Network.isActive)
		{
			UpdateContainedThing();
		}
	}

	private void UpdateContainedThing()
	{
		if (contains != null && !(Level.current is Editor))
		{
			_containedThing = Editor.CreateThing(contains);
			_containedThing.active = false;
			_containedThing.visible = false;
			Level.Add(_containedThing);
		}
	}

	public override void PrepareForHost()
	{
		UpdateContainedThing();
		if (_containedThing != null)
		{
			_containedThing.PrepareForHost();
		}
		base.PrepareForHost();
	}

	protected override float CalculatePersonalImpactPower(MaterialThing with, ImpactedFrom from)
	{
		return base.CalculatePersonalImpactPower(with, from) - 1.5f;
	}

	public override void OnSolidImpact(MaterialThing with, ImpactedFrom from)
	{
		if (with is PhysicsObject)
		{
			(with as PhysicsObject).specialFrictionMod = 0.16f;
			(with as PhysicsObject).modFric = true;
		}
		base.OnSolidImpact(with, from);
	}

	public override bool Hit(Bullet bullet, Vec2 hitPos)
	{
		if (bullet.isLocal && owner == null)
		{
			Thing.Fondle(this, DuckNetwork.localConnection);
		}
		for (int i = 0; i < 4; i++)
		{
			GlassParticle glassParticle = new GlassParticle(hitPos.x, hitPos.y, bullet.travelDirNormalized);
			Level.Add(glassParticle);
			glassParticle.hSpeed = (0f - bullet.travelDirNormalized.x) * 2f * (Rando.Float(1f) + 0.3f);
			glassParticle.vSpeed = (0f - bullet.travelDirNormalized.y) * 2f * (Rando.Float(1f) + 0.3f) - Rando.Float(2f);
			Level.Add(glassParticle);
		}
		SFX.Play("glassHit", 0.6f);
		if (bullet.isLocal && TeamSelect2.Enabled("EXPLODEYCRATES"))
		{
			Thing.Fondle(this, DuckNetwork.localConnection);
			if (base.duck != null)
			{
				base.duck.ThrowItem();
			}
			Destroy(new DTShot(bullet));
			Level.Add(new GrenadeExplosion(base.x, base.y));
		}
		if (base.isServerForObject && bullet.isLocal)
		{
			breakPoints -= damageMultiplier;
			damageMultiplier += 2f;
			if (breakPoints <= 0f)
			{
				Destroy(new DTShot(bullet));
			}
			vSpeed -= 1f;
			hSpeed += bullet.travelDirNormalized.x;
			vSpeed += bullet.travelDirNormalized.y;
		}
		return base.Hit(bullet, hitPos);
	}

	public override bool Hurt(float points)
	{
		if (carved >= 0f)
		{
			carved += points * 0.05f;
		}
		return true;
	}

	protected override bool OnDestroy(DestroyType type = null)
	{
		_hitPoints = 0f;
		Level.Remove(this);
		SFX.Play("glassHit");
		Vec2 flyDir = Vec2.Zero;
		if (type is DTShot)
		{
			flyDir = (type as DTShot).bullet.travelDirNormalized;
		}
		for (int i = 0; i < 8; i++)
		{
			GlassParticle glassParticle = new GlassParticle(base.x + Rando.Float(-4f, 4f), base.y + Rando.Float(-4f, 4f), flyDir);
			Level.Add(glassParticle);
			glassParticle.hSpeed = flyDir.x * 2f * (Rando.Float(1f) + 0.3f);
			glassParticle.vSpeed = flyDir.y * 2f * (Rando.Float(1f) + 0.3f) - Rando.Float(2f);
			Level.Add(glassParticle);
		}
		for (int j = 0; j < 5; j++)
		{
			SmallSmoke smallSmoke = SmallSmoke.New(base.x + Rando.Float(-6f, 6f), base.y + Rando.Float(-6f, 6f));
			smallSmoke.hSpeed += Rando.Float(-0.3f, 0.3f);
			smallSmoke.vSpeed -= Rando.Float(0.1f, 0.2f);
			Level.Add(smallSmoke);
		}
		ReleaseContainedObject();
		return true;
	}

	public override void ExitHit(Bullet bullet, Vec2 exitPos)
	{
		for (int i = 0; i < 4; i++)
		{
			GlassParticle glassParticle = new GlassParticle(exitPos.x, exitPos.y, bullet.travelDirNormalized);
			Level.Add(glassParticle);
			glassParticle.hSpeed = bullet.travelDirNormalized.x * 2f * (Rando.Float(1f) + 0.3f);
			glassParticle.vSpeed = bullet.travelDirNormalized.y * 2f * (Rando.Float(1f) + 0.3f) - Rando.Float(2f);
			Level.Add(glassParticle);
		}
	}

	private void ReleaseContainedObject()
	{
		if (base.isServerForObject && _containedThing != null)
		{
			Fondle(_containedThing);
			_containedThing.alpha = 1f;
			_containedThing.active = true;
			_containedThing.material = null;
			_containedThing.visible = true;
			_containedThing.velocity = base.velocity + new Vec2(0f, -2f);
			if (base.duck != null)
			{
				base.duck.GiveHoldable(_containedThing as Holdable);
			}
		}
	}

	public override void HeatUp(Vec2 location)
	{
		_hitPoints -= 0.01f;
		if (_hitPoints < 0.05f)
		{
			Level.Remove(this);
			base._destroyed = true;
			ReleaseContainedObject();
			for (int i = 0; i < 16; i++)
			{
				FluidData dat = Fluid.Water;
				dat.amount = 0.001f;
				Level.Add(new Fluid(base.x + (float)Rando.Int(-6, 6), base.y + (float)Rando.Int(-6, 6), Vec2.Zero, dat)
				{
					hSpeed = ((float)i / 16f - 0.5f) * Rando.Float(0.3f, 0.4f),
					vSpeed = Rando.Float(-1.5f, 0.5f)
				});
			}
		}
		FluidData dat2 = Fluid.Water;
		dat2.amount = 0.001f;
		Level.Add(new Fluid(base.x + (float)Rando.Int(-6, 6), base.y + (float)Rando.Int(-6, 6), Vec2.Zero, dat2)
		{
			hSpeed = Rando.Float(-0.1f, 0.1f),
			vSpeed = Rando.Float(-0.3f, 0.3f)
		});
		base.HeatUp(location);
	}

	public override void Draw()
	{
		if (_containedThing != null)
		{
			Depth d = base.depth;
			base.depth = d - 8;
			base.Draw();
			if (_frozen == null)
			{
				_frozen = new MaterialFrozen(_containedThing);
				_frozen.intensity = 1f;
			}
			Material obj = Graphics.material;
			Graphics.material = _frozen;
			_containedThing.position = position;
			_containedThing.alpha = 1f;
			_containedThing.depth = d - 4;
			_containedThing.angle = angle;
			_containedThing.offDir = offDir;
			_containedThing.Draw();
			Graphics.material = obj;
			base.depth = d;
			base.alpha = 0.5f;
			base.Draw();
			base.alpha = 1f;
			base.depth = d;
		}
		else if (didCarve)
		{
			float ypos = base.y;
			graphic.flipH = offDir <= 0;
			_graphic.position = position;
			_graphic.alpha = base.alpha;
			_graphic.angle = angle;
			_graphic.depth = base.depth;
			_graphic.scale = base.scale;
			_graphic.center = center;
			int yOffset = (int)((1f - _hitPoints) * 12f);
			Graphics.Draw(_graphic.texture, position + new Vec2(0f, yOffset), new Rectangle(0f, 0f, 16f, 24 - yOffset), Color.White, angle, _graphic.center, base.scale, graphic.flipH ? SpriteEffects.FlipHorizontally : SpriteEffects.None, base.depth);
			base.y = ypos;
		}
		else
		{
			base.Draw();
		}
	}

	public override void UpdateMaterial()
	{
	}

	public override void Update()
	{
		base.Update();
		heat = -1f;
		if (_containedThing != null && _containedThing is Holdable)
		{
			if ((_containedThing as MaterialThing).weight > weight)
			{
				weight = (_containedThing as MaterialThing).weight;
			}
			(_containedThing as MaterialThing).heat = -1f;
			(_containedThing as Holdable).UpdateMaterial();
		}
		if (carved >= 1f && !didCarve)
		{
			if (_containedThing != null)
			{
				Destroy(new DTImpale(this));
			}
			else
			{
				_sprite = new SpriteMap("iceSculpture", 16, 24);
				graphic = _sprite;
				center = new Vec2(8f, 15f);
				for (int i = 0; i < 12; i++)
				{
					SmallSmoke smallSmoke = SmallSmoke.New(base.x + Rando.Float(-9f, 9f), base.y + Rando.Float(-9f, 9f));
					smallSmoke._sprite.color = Color.White;
					Level.Add(smallSmoke);
				}
				SFX.Play("crateDestroy", 1f, Rando.Float(0.1f, 0.3f));
				didCarve = true;
			}
		}
		if (damageMultiplier > 1f)
		{
			damageMultiplier -= 0.2f;
		}
		else
		{
			damageMultiplier = 1f;
			breakPoints = 15f;
		}
		if (!didCarve)
		{
			_sprite.frame = (int)Math.Floor((1f - _hitPoints / 1f) * 5f);
			if (_sprite.frame == 0)
			{
				collisionOffset = new Vec2(-8f, -8f);
				collisionSize = new Vec2(16f, 16f);
			}
			else if (_sprite.frame == 1)
			{
				collisionOffset = new Vec2(-8f, -7f);
				collisionSize = new Vec2(16f, 15f);
			}
			else if (_sprite.frame == 2)
			{
				collisionOffset = new Vec2(-7f, -4f);
				collisionSize = new Vec2(14f, 11f);
			}
			else if (_sprite.frame == 3)
			{
				collisionOffset = new Vec2(-6f, -2f);
				collisionSize = new Vec2(12f, 7f);
			}
			else if (_sprite.frame == 4)
			{
				collisionOffset = new Vec2(-6f, -1f);
				collisionSize = new Vec2(12f, 5f);
			}
		}
		else
		{
			int yOffset = (int)((1f - _hitPoints) * 12f);
			collisionOffset = new Vec2(-8f, -8 + yOffset);
			collisionSize = new Vec2(16f, 16 - yOffset);
		}
	}
}
