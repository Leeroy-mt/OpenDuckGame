using System;
using System.Collections.Generic;

namespace DuckGame;

[EditorGroup("Spawns")]
[BaggedProperty("isInDemo", true)]
[BaggedProperty("canSpawn", false)]
public class ItemCrate : PhysicsObject, IPlatform, IContainAThing, IContainPossibleThings
{
	public StateBinding _containedObject1Binding = new StateBinding("_containedObject1");

	public StateBinding _containedObject2Binding = new StateBinding("_containedObject2");

	public StateBinding _containedObject3Binding = new StateBinding("_containedObject3");

	public StateBinding _containedObject4Binding = new StateBinding("_containedObject4");

	public StateBinding _destroyedBinding = new StateBinding("_destroyed");

	public StateBinding _hitPointsBinding = new StateBinding("_hitPoints");

	public StateBinding _damageMultiplierBinding = new StateBinding("damageMultiplier");

	public bool randomSpawn;

	public EditorProperty<bool> revealRandom = new EditorProperty<bool>(val: false);

	private List<TypeProbPair> _possible = new List<TypeProbPair>();

	private SpriteMap _sprite;

	public PhysicsObject _containedObject1;

	public PhysicsObject _containedObject2;

	public PhysicsObject _containedObject3;

	public PhysicsObject _containedObject4;

	public PhysicsObject[] _containedObjects;

	private Sprite _randomMark;

	private PhysicsObject _containedObject;

	private PhysicsObject _previewThing;

	private Sprite _containedSprite;

	private float damageMultiplier = 1f;

	public Type contains { get; set; }

	public List<TypeProbPair> possible => _possible;

	public PhysicsObject containedObject
	{
		get
		{
			return _containedObject;
		}
		set
		{
			_containedObject = value;
		}
	}

	public ItemCrate(float xpos, float ypos)
		: base(xpos, ypos)
	{
		_maxHealth = 15f;
		_hitPoints = 15f;
		_sprite = new SpriteMap("bigItemCrate", 32, 33);
		graphic = _sprite;
		center = new Vec2(16f, 24f);
		collisionOffset = new Vec2(-16f, -24f);
		collisionSize = new Vec2(32f, 32f);
		base.depth = -0.7f;
		thickness = 2f;
		weight = 10f;
		_randomMark = new Sprite("itemBoxRandom");
		_randomMark.CenterOrigin();
		flammable = 0.3f;
		base.collideSounds.Add("rockHitGround2");
		physicsMaterial = PhysicsMaterial.Metal;
		_containedObjects = new PhysicsObject[4] { _containedObject1, _containedObject2, _containedObject3, _containedObject4 };
		editorTooltip = "Chock full of good stuff- if you can get it open..";
	}

	public override void Initialize()
	{
		if (!(Level.current is Editor))
		{
			if (randomSpawn)
			{
				List<Type> things = ItemBox.GetPhysicsObjects(Editor.Placeables);
				contains = things[Rando.Int(things.Count - 1)];
			}
			else if (possible.Count > 0 && contains == null)
			{
				PreparePossibilities();
			}
		}
	}

	public void PreparePossibilities()
	{
		if (possible.Count > 0)
		{
			contains = MysteryGun.PickType(base.chanceGroup, possible);
		}
	}

	public virtual void UpdateContainedObject()
	{
		for (int i = 0; i < 4; i++)
		{
			if (Network.isActive && base.isServerForObject && _containedObjects[i] == null)
			{
				_containedObjects[i] = GetSpawnItem();
				if (_containedObjects[i] != null)
				{
					_containedObjects[i].visible = false;
					_containedObjects[i].solid = false;
					_containedObjects[i].active = false;
					_containedObjects[i].position = position;
					Level.Add(_containedObjects[i]);
				}
				_containedObject1 = _containedObjects[0];
				_containedObject2 = _containedObjects[1];
				_containedObject3 = _containedObjects[2];
				_containedObject4 = _containedObjects[3];
			}
		}
	}

	public virtual PhysicsObject GetSpawnItem()
	{
		if (contains == null)
		{
			return null;
		}
		IReadOnlyPropertyBag containsBag = ContentProperties.GetBag(contains);
		if (Network.isActive && !containsBag.GetOrDefault("isOnlineCapable", defaultValue: true))
		{
			return Activator.CreateInstance(typeof(Pistol), Editor.GetConstructorParameters(typeof(Pistol))) as PhysicsObject;
		}
		return Editor.CreateThing(contains) as PhysicsObject;
	}

	public override void EditorUpdate()
	{
		UpdatePreview();
		base.EditorUpdate();
	}

	private void UpdatePreview()
	{
		if (_previewThing == null || _previewThing.GetType() != contains)
		{
			_previewThing = GetSpawnItem();
			if (_previewThing != null)
			{
				_containedSprite = _previewThing.GetEditorImage(20, 16, transparentBack: true, null, null, pUseCollisionSize: true);
			}
			else
			{
				_containedSprite = null;
			}
		}
	}

	public override void Update()
	{
		UpdateContainedObject();
		_containedObjects[0] = _containedObject1;
		_containedObjects[1] = _containedObject2;
		_containedObjects[2] = _containedObject3;
		_containedObjects[3] = _containedObject4;
		if (base.isServerForObject)
		{
			if (damageMultiplier > 1f)
			{
				damageMultiplier -= 0.2f;
			}
			else
			{
				damageMultiplier = 1f;
			}
			if (_hitPoints <= 0f && !base._destroyed)
			{
				Destroy(new DTImpact(this));
			}
			if (_onFire)
			{
				_hitPoints = Math.Min(_hitPoints, (1f - burnt) * _maxHealth);
			}
		}
		UpdatePreview();
		if (contains == null)
		{
			buoyancy = 1f;
		}
		else
		{
			buoyancy = 0f;
		}
		base.Update();
	}

	public override void Draw()
	{
		if (randomSpawn && !revealRandom.value)
		{
			_sprite.frame = 4;
			Vec2 off = Offset(new Vec2(0f, -8f));
			_randomMark.angle = angle;
			_randomMark.flipH = offDir <= 0;
			Graphics.Draw(_randomMark, off.x, off.y, base.depth + 10);
		}
		else if (_containedSprite != null)
		{
			_sprite.frame = 4;
			_containedSprite.CenterOrigin();
			Vec2 off2 = Offset(new Vec2(0f, -8f));
			_containedSprite.angle = angle;
			_containedSprite.flipH = offDir <= 0;
			Graphics.Draw(_containedSprite, off2.x, off2.y, base.depth + 10);
		}
		else
		{
			_sprite.frame = 0;
		}
		_sprite.frame += (int)((1f - _hitPoints / _maxHealth) * 3.5f);
		base.Draw();
	}

	public override ContextMenu GetContextMenu()
	{
		FieldBinding binding = new FieldBinding(this, "contains");
		EditorGroupMenu obj = base.GetContextMenu() as EditorGroupMenu;
		obj.AddItem(new ContextCheckBox("Random", null, new FieldBinding(this, "randomSpawn")));
		EditorGroupMenu contain = new EditorGroupMenu(obj);
		contain.InitializeGroups(new EditorGroup(typeof(PhysicsObject)), binding);
		contain.text = "Contains";
		obj.AddItem(contain);
		EditorGroupMenu possib = new EditorGroupMenu(obj);
		possib.InitializeGroups(new EditorGroup(typeof(PhysicsObject)), new FieldBinding(this, "possible"));
		possib.text = "Possible";
		obj.AddItem(possib);
		return obj;
	}

	public override BinaryClassChunk Serialize()
	{
		BinaryClassChunk binaryClassChunk = base.Serialize();
		binaryClassChunk.AddProperty("contains", Editor.SerializeTypeName(contains));
		binaryClassChunk.AddProperty("randomSpawn", randomSpawn);
		binaryClassChunk.AddProperty("possible", MysteryGun.SerializeTypeProb(possible));
		return binaryClassChunk;
	}

	public override bool Deserialize(BinaryClassChunk node)
	{
		base.Deserialize(node);
		contains = Editor.DeSerializeTypeName(node.GetProperty<string>("contains"));
		randomSpawn = node.GetProperty<bool>("randomSpawn");
		_possible = MysteryGun.DeserializeTypeProb(node.GetProperty<string>("possible"));
		return true;
	}

	protected override bool OnDestroy(DestroyType type = null)
	{
		_hitPoints = 0f;
		for (int i = 0; i < 10; i++)
		{
			WoodDebris woodDebris = WoodDebris.New(base.x - 10f + Rando.Float(20f), base.y - 10f + Rando.Float(20f));
			woodDebris.hSpeed = Rando.Float(-4f, 4f);
			woodDebris.vSpeed = Rando.Float(-4f, 4f);
			Level.Add(woodDebris);
		}
		for (int j = 0; j < 3; j++)
		{
			MusketSmoke musketSmoke = new MusketSmoke(base.x + Rando.Float(-10f, 10f), base.y + Rando.Float(-10f, 10f));
			musketSmoke.hSpeed += Rando.Float(-0.3f, 0.3f);
			musketSmoke.vSpeed -= Rando.Float(0.1f, 0.2f);
			Level.Add(musketSmoke);
		}
		for (int k = 0; k < 4; k++)
		{
			PhysicsObject spawn = _containedObjects[k];
			if (!Network.isActive)
			{
				spawn = GetSpawnItem();
			}
			if (spawn == null)
			{
				continue;
			}
			if (_onFire)
			{
				spawn.heat = 0.8f;
			}
			spawn.position = position + new Vec2(-4f + (float)k * 2.6666667f, 0f);
			if (k == 0 || k == 3)
			{
				if (k == 0)
				{
					spawn.hSpeed = -2f;
				}
				else
				{
					spawn.hSpeed = 2f;
				}
				spawn.vSpeed = -2.5f;
			}
			else
			{
				if (k == 1)
				{
					spawn.hSpeed = -1.2f;
				}
				else
				{
					spawn.hSpeed = 1.2f;
				}
				spawn.vSpeed = -3.5f;
			}
			if (Network.isActive)
			{
				spawn.visible = true;
				spawn.solid = true;
				spawn.active = true;
			}
			else
			{
				Level.Add(spawn);
			}
		}
		SFX.Play("crateDestroy");
		Level.Remove(this);
		return true;
	}

	public override bool Hit(Bullet bullet, Vec2 hitPos)
	{
		if (_hitPoints <= 0f)
		{
			return base.Hit(bullet, hitPos);
		}
		if (bullet.isLocal && owner == null)
		{
			Thing.Fondle(this, DuckNetwork.localConnection);
		}
		for (int i = 0; (float)i < 1f + damageMultiplier / 2f; i++)
		{
			WoodDebris woodDebris = WoodDebris.New(hitPos.x, hitPos.y);
			woodDebris.hSpeed = (0f - bullet.travelDirNormalized.x) * 2f * (Rando.Float(1f) + 0.3f);
			woodDebris.vSpeed = (0f - bullet.travelDirNormalized.y) * 2f * (Rando.Float(1f) + 0.3f) - Rando.Float(2f);
			Level.Add(woodDebris);
		}
		SFX.Play("woodHit");
		if (base.isServerForObject && TeamSelect2.Enabled("EXPLODEYCRATES"))
		{
			Thing.Fondle(this, DuckNetwork.localConnection);
			Destroy(new DTShot(bullet));
			Level.Add(new GrenadeExplosion(base.x, base.y));
		}
		_hitPoints -= damageMultiplier;
		damageMultiplier += 2f;
		if (_hitPoints <= 0f)
		{
			if (bullet.isLocal)
			{
				Thing.SuperFondle(this, DuckNetwork.localConnection);
			}
			Destroy(new DTShot(bullet));
		}
		return base.Hit(bullet, hitPos);
	}
}
