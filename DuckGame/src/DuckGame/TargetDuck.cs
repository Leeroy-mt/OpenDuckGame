using System;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;

namespace DuckGame;

[EditorGroup("Stuff|Props", EditorItemType.Arcade)]
[BaggedProperty("canSpawn", false)]
[BaggedProperty("isOnlineCapable", false)]
public class TargetDuck : Duck, ISequenceItem
{
	protected new SpriteMap _sprite;

	protected Sprite _base;

	protected Sprite _woodWing;

	protected bool _popup;

	protected float _upSpeed;

	protected bool _up;

	protected TargetStance _stance;

	public Type contains;

	public bool chestPlate;

	public bool helmet;

	private float _timeCount;

	public EditorProperty<float> time = new EditorProperty<float>(0f, null, 0f, 30f, 0.1f, "INF");

	public EditorProperty<float> autofire = new EditorProperty<float>(0f, null, 0f, 100f, 0.1f, "INF");

	public EditorProperty<bool> random = new EditorProperty<bool>(val: false);

	public EditorProperty<int> maxrandom = new EditorProperty<int>(1, null, 1f, 32f, 1f);

	public EditorProperty<bool> dropgun = new EditorProperty<bool>(val: true);

	public EditorProperty<float> speediness = new EditorProperty<float>(1f, null, 0f, 2f, 0.01f);

	private float _autoFireWait;

	private bool editorUpdate;

	protected float _waitFire = 1f;

	private Sprite _reticule;

	private int _stanceSetting;

	public override bool action => false;

	public int stanceSetting
	{
		get
		{
			return _stanceSetting;
		}
		set
		{
			_stanceSetting = value;
			_stance = (TargetStance)_stanceSetting;
			UpdateCollision();
		}
	}

	public TargetDuck(float xpos, float ypos, TargetStance stance)
		: base(xpos, ypos, null)
	{
		_sprite = new SpriteMap("woodDuck", 32, 32);
		_base = new Sprite("popupPad");
		_woodWing = new Sprite("woodWing");
		graphic = _sprite;
		center = new Vec2(16f, 22f);
		_stance = stance;
		UpdateCollision();
		physicsMaterial = PhysicsMaterial.Wood;
		thickness = 0.5f;
		_hitPoints = (_maxHealth = 0.1f);
		base.editorOffset = new Vec2(0f, -4f);
		base.hugWalls = WallHug.Floor;
		_canHaveChance = false;
		base.sequence = new SequenceItem(this);
		base.sequence.type = SequenceItemType.Target;
		speediness.value = 1f;
	}

	public override void OnDrawLayer(Layer pLayer)
	{
	}

	public override void Initialize()
	{
		_profile = Profiles.EnvironmentProfile;
		InitProfile();
		_sprite = new SpriteMap("woodDuck", 32, 32);
		_base = new Sprite("popupPad");
		_woodWing = new Sprite("woodWing");
		graphic = _sprite;
		if (!(Level.current is Editor))
		{
			if (_stance != TargetStance.Fly)
			{
				base.scale = new Vec2(1f, 0f);
			}
			else
			{
				base.scale = new Vec2(0f, 1f);
			}
			ChallengeLevel.allTargetsShot = false;
			_autoFireWait = autofire.value;
		}
		if ((float)speediness == 0f)
		{
			speediness.value = 1f;
		}
		_waitFire = speediness;
		UpdateCollision();
	}

	public void SpawnHoldObject()
	{
		_autoFireWait = autofire;
		if (contains != null && Editor.CreateThing(contains) is Holdable newThing)
		{
			Level.Add(newThing);
			GiveThing(newThing);
		}
	}

	public override void ReturnItemToWorld(Thing t)
	{
		Vec2 vec = position + new Vec2(offDir * 3, 0f);
		Block rightWall = Level.CheckLine<Block>(vec, vec + new Vec2(16f, 0f));
		if (rightWall != null && rightWall.solid && t.right > rightWall.left)
		{
			t.right = rightWall.left;
		}
		Block leftWall = Level.CheckLine<Block>(vec, vec - new Vec2(16f, 0f));
		if (leftWall != null && leftWall.solid && t.left < leftWall.right)
		{
			t.left = leftWall.right;
		}
		Block topWall = Level.CheckLine<Block>(vec, vec + new Vec2(0f, -16f));
		if (topWall != null && topWall.solid && t.top < topWall.bottom)
		{
			t.top = topWall.bottom;
		}
		Block bottomWall = Level.CheckLine<Block>(vec, vec + new Vec2(0f, 16f));
		if (bottomWall != null && bottomWall.solid && t.bottom > bottomWall.top)
		{
			t.bottom = bottomWall.top;
		}
	}

	public void UpdateCollision()
	{
		if (Level.current is Editor || Level.current == null || (_up && _popup))
		{
			crouch = false;
			sliding = false;
			if (_stance == TargetStance.Stand)
			{
				_sprite.frame = 0;
				_collisionOffset = new Vec2(-6f, -24f);
				collisionSize = new Vec2(12f, 24f);
				base.hugWalls = WallHug.Floor;
			}
			else if (_stance == TargetStance.StandArmed)
			{
				_sprite.frame = 1;
				_collisionOffset = new Vec2(-6f, -23f);
				collisionSize = new Vec2(12f, 23f);
				base.hugWalls = WallHug.Floor;
			}
			else if (_stance == TargetStance.Crouch)
			{
				_sprite.frame = 2;
				_collisionOffset = new Vec2(-6f, -18f);
				collisionSize = new Vec2(12f, 18f);
				crouch = true;
				base.hugWalls = WallHug.Floor;
			}
			else if (_stance == TargetStance.Slide)
			{
				_sprite.frame = 3;
				_collisionOffset = new Vec2(-6f, -10f);
				collisionSize = new Vec2(12f, 10f);
				sliding = true;
				base.hugWalls = WallHug.Floor;
			}
			else if (_stance == TargetStance.Fly)
			{
				_sprite.frame = 4;
				_collisionOffset = new Vec2(-8f, -24f);
				collisionSize = new Vec2(16f, 24f);
				base.hugWalls = WallHug.Left | WallHug.Right;
			}
		}
		else
		{
			base.hugWalls = WallHug.Floor;
			if (_stance == TargetStance.Stand)
			{
				_sprite.frame = 0;
			}
			else if (_stance == TargetStance.StandArmed)
			{
				_sprite.frame = 1;
			}
			else if (_stance == TargetStance.Crouch)
			{
				_sprite.frame = 2;
			}
			else if (_stance == TargetStance.Slide)
			{
				_sprite.frame = 3;
			}
			else if (_stance == TargetStance.Fly)
			{
				_sprite.frame = 4;
				base.hugWalls = WallHug.Left | WallHug.Right;
			}
			_collisionOffset = new Vec2(-6000f, 0f);
			collisionSize = new Vec2(2f, 2f);
		}
		_collisionOffset.y += 10f;
		_collisionSize.y -= 1f;
		_featherVolume.collisionSize = new Vec2(collisionSize.x + 2f, collisionSize.y + 2f);
		_featherVolume.collisionOffset = new Vec2(collisionOffset.x - 1f, collisionOffset.y - 1f);
	}

	public override void OnSequenceActivate()
	{
		_popup = true;
	}

	public void PopDown()
	{
		_popup = false;
		if (holdObject != null)
		{
			Level.Remove(holdObject);
			holdObject = null;
		}
		foreach (Equipment e in _equipment)
		{
			if (e != null)
			{
				Level.Remove(e);
			}
		}
		_equipment.Clear();
		_sequence.Finished();
	}

	public override bool Kill(DestroyType type = null)
	{
		if (_up && _popup)
		{
			if (ChallengeLevel.running)
			{
				ChallengeLevel.targetsShot++;
			}
			if (holdObject is Gun && !dropgun)
			{
				(holdObject as Gun).ammo = 0;
			}
			ThrowItem(throwWithForce: false);
			foreach (Equipment e in _equipment)
			{
				if (e != null)
				{
					e.owner = null;
					e.hSpeed = -1f + Rando.Float(2f);
					e.vSpeed = 0f - Rando.Float(1.5f);
					ReturnItemToWorld(e);
					e.UnEquip();
				}
			}
			SFX.Play("ting", Rando.Float(0.7f, 0.8f), Rando.Float(-0.2f, 0.2f));
			if (type is DTShot)
			{
				SFX.Play("targetRebound", Rando.Float(0.7f, 0.8f), Rando.Float(-0.2f, 0.2f));
			}
			Vec2 flyDir = Vec2.Zero;
			if (type is DTShot)
			{
				flyDir = (type as DTShot).bullet.travelDirNormalized;
			}
			for (int i = 0; i < 4; i++)
			{
				WoodDebris woodDebris = WoodDebris.New(base.x - 8f + Rando.Float(16f), base.y - 20f + Rando.Float(16f));
				woodDebris.hSpeed = ((Rando.Float(1f) > 0.5f) ? 1f : (-1f)) * Rando.Float(3f) + (float)Math.Sign(flyDir.x) * 0.5f;
				woodDebris.vSpeed = 0f - Rando.Float(1f);
				Level.Add(woodDebris);
			}
			for (int j = 0; j < 2; j++)
			{
				Level.Add(Feather.New(base.x, base.y - 16f, base.persona));
			}
			PopDown();
		}
		return false;
	}

	public override bool Hit(Bullet bullet, Vec2 hitPos)
	{
		if (_up && _popup)
		{
			return base.Hit(bullet, hitPos);
		}
		return false;
	}

	public override void ExitHit(Bullet bullet, Vec2 hitPos)
	{
		if (_up && !_popup)
		{
			for (int i = 0; i < 2; i++)
			{
				WoodDebris woodDebris = WoodDebris.New(hitPos.x, hitPos.y);
				woodDebris.hSpeed = (0f - bullet.travelDirNormalized.x) * 2f * (Rando.Float(1f) + 0.3f);
				woodDebris.vSpeed = (0f - bullet.travelDirNormalized.y) * 2f * (Rando.Float(1f) + 0.3f) - Rando.Float(2f);
				Level.Add(woodDebris);
			}
		}
	}

	public override bool Hurt(float points)
	{
		if (!_popup)
		{
			return false;
		}
		if (_maxHealth == 0f)
		{
			return false;
		}
		_hitPoints -= points;
		return true;
	}

	public void GiveThing(Holdable h)
	{
		holdObject = h;
		holdObject.owner = this;
		holdObject.solid = false;
	}

	public override void UpdateSkeleton()
	{
		int off = 6;
		int f = 0;
		if (sliding)
		{
			f = 12;
		}
		else if (crouch)
		{
			f = 11;
		}
		_skeleton.head.position = Offset(DuckRig.GetHatPoint(f) + new Vec2(0f, -off));
		_skeleton.upperTorso.position = Offset(DuckRig.GetChestPoint(f) + new Vec2(0f, -off));
		_skeleton.lowerTorso.position = position + new Vec2(0f, 10 - off);
		if (sliding)
		{
			_skeleton.head.orientation = Maths.DegToRad(90f);
			_skeleton.upperTorso.orientation = Maths.DegToRad(90f);
		}
		else
		{
			_skeleton.head.orientation = 0f;
			_skeleton.upperTorso.orientation = 0f;
		}
	}

	public override void UpdateHoldPosition(bool updateLerp = true)
	{
		if (holdObject == null)
		{
			return;
		}
		holdOffY = 0f;
		armOffY = 0f;
		if ((!_up || !_popup) && !editorUpdate)
		{
			return;
		}
		holdObject.UpdateAction();
		holdObject.position = base.armPosition + holdObject.holdOffset + new Vec2(holdOffX, holdOffY) + new Vec2(2 * offDir, 0f);
		holdObject.offDir = offDir;
		if (_sprite.currentAnimation == "slide")
		{
			holdOffY -= 1f;
			holdOffX += 1f;
		}
		else if (crouch)
		{
			if (holdObject != null)
			{
				armOffY += 4f;
			}
		}
		else if (sliding && holdObject != null)
		{
			armOffY += 6f;
		}
		_ = _stance;
		_ = 4;
		holdObject.position = HoldOffset(holdObject.holdOffset) + new Vec2(offDir * 3, 0f);
		holdObject.angle = holdObject.handAngle + holdAngleOff;
	}

	public override void DuckUpdate()
	{
	}

	public virtual void UpdateFire()
	{
		Gun g = holdObject as Gun;
		float range = 300f;
		if (g.ammoType != null)
		{
			range = g.ammoType.range;
		}
		Vec2 at = holdObject.Offset(new Vec2(range * holdObject.angleMul, 0f));
		if (_autoFireWait <= 0f)
		{
			foreach (Duck d in Level.current.things[typeof(Duck)].Where((Thing thing) => !(thing is TargetDuck)))
			{
				if ((Collision.Line(holdObject.position + new Vec2(0f, -5f), at + new Vec2(0f, -5f), d.rectangle) || Collision.Line(holdObject.position + new Vec2(0f, 5f), at + new Vec2(0f, 5f), d.rectangle)) && Level.CheckLine<Block>(holdObject.position, d.position) == null)
				{
					_waitFire -= 0.03f;
					break;
				}
			}
		}
		bool fire = false;
		if (_autoFireWait > 0f)
		{
			_autoFireWait -= Maths.IncFrameTimer();
			if (_autoFireWait <= 0f)
			{
				fire = true;
			}
		}
		if (_waitFire <= 0f || fire)
		{
			g.PressAction();
			_waitFire = speediness;
		}
		if (_waitFire < (float)speediness)
		{
			_waitFire += 0.01f;
		}
	}

	public override void Update()
	{
		base.impacting.Clear();
		if (_up && _popup && holdObject is Gun)
		{
			UpdateFire();
		}
		UpdateCollision();
		UpdateSkeleton();
		if (_hitPoints <= 0f)
		{
			Destroy(new DTCrush(null));
		}
		if (!_up)
		{
			_timeCount = 0f;
			if (_popup)
			{
				_upSpeed += 0.1f;
			}
			if (_stance != TargetStance.Fly)
			{
				base.yscale += _upSpeed;
				if (base.yscale >= 1f)
				{
					base.yscale = 1f;
					_upSpeed = 0f;
					_up = true;
					SFX.Play("grappleHook", 0.7f, Rando.Float(-0.2f, 0.2f));
					Level.Add(SmallSmoke.New(base.x - 4f, base.y));
					Level.Add(SmallSmoke.New(base.x + 4f, base.y));
					SpawnHoldObject();
					if (helmet)
					{
						Helmet h = new Helmet(base.x, base.y);
						Level.Add(h);
						Equip(h);
					}
					if (chestPlate)
					{
						ChestPlate p = new ChestPlate(base.x, base.y);
						Level.Add(p);
						Equip(p);
					}
				}
				return;
			}
			base.xscale += _upSpeed;
			if (base.xscale >= 1f)
			{
				base.xscale = 1f;
				_upSpeed = 0f;
				_up = true;
				SFX.Play("grappleHook", 0.7f, Rando.Float(-0.2f, 0.2f));
				Level.Add(SmallSmoke.New(base.x - 4f, base.y));
				Level.Add(SmallSmoke.New(base.x + 4f, base.y));
				if (helmet)
				{
					Helmet h2 = new Helmet(base.x, base.y);
					Level.Add(h2);
					Equip(h2);
				}
				if (chestPlate)
				{
					ChestPlate p2 = new ChestPlate(base.x, base.y);
					Level.Add(p2);
					Equip(p2);
				}
			}
			return;
		}
		_timeCount += Maths.IncFrameTimer();
		if (_popup && time.value != 0f && _timeCount >= time.value)
		{
			SFX.Play("grappleHook", 0.2f, Rando.Float(-0.2f, 0.2f));
			PopDown();
			return;
		}
		if (!_popup)
		{
			_upSpeed += 0.1f;
		}
		if (_stance != TargetStance.Fly)
		{
			base.yscale -= _upSpeed;
			if (base.yscale < 0f)
			{
				base.yscale = 0f;
				_upSpeed = 0f;
				_up = false;
				SFX.Play("grappleHook", 0.2f, Rando.Float(-0.2f, 0.2f));
				Level.Add(SmallSmoke.New(base.x - 4f, base.y));
				Level.Add(SmallSmoke.New(base.x + 4f, base.y));
				_hitPoints = (_maxHealth = 0.1f);
			}
		}
		else
		{
			base.xscale -= _upSpeed;
			if (base.xscale < 0f)
			{
				base.xscale = 0f;
				_upSpeed = 0f;
				_up = false;
				SFX.Play("grappleHook", 0.2f, Rando.Float(-0.2f, 0.2f));
				Level.Add(SmallSmoke.New(base.x - 4f, base.y));
				Level.Add(SmallSmoke.New(base.x + 4f, base.y));
				_hitPoints = (_maxHealth = 0.1f);
			}
		}
	}

	public new void DrawIcon()
	{
		if (!ShouldDrawIcon() || !_up || !ChallengeMode.showReticles)
		{
			return;
		}
		if (_reticule == null)
		{
			_reticule = new Sprite("challenge/reticule");
		}
		Vec2 pos = position;
		if (base.ragdoll != null)
		{
			pos = base.ragdoll.part1.position;
		}
		else if (base._trapped != null)
		{
			pos = base._trapped.position;
		}
		if (!((pos - Level.current.camera.position).length > Level.current.camera.width * 2f))
		{
			float boarder = 14f;
			if (pos.x < Level.current.camera.left + boarder)
			{
				pos.x = Level.current.camera.left + boarder;
			}
			if (pos.x > Level.current.camera.right - boarder)
			{
				pos.x = Level.current.camera.right - boarder;
			}
			if (pos.y < Level.current.camera.top + boarder)
			{
				pos.y = Level.current.camera.top + boarder;
			}
			if (pos.y > Level.current.camera.bottom - boarder)
			{
				pos.y = Level.current.camera.bottom - boarder;
			}
			pos = Level.current.camera.transform(pos);
			pos = Layer.HUD.camera.transformInverse(pos);
			Graphics.DrawRect(pos + new Vec2(-5f, -5f), pos + new Vec2(5f, 5f), Color.Black, 0.8f);
			Graphics.DrawRect(pos + new Vec2(-5f, -5f), pos + new Vec2(5f, 5f), Color.White, 0.81f, filled: false);
			Graphics.Draw(_reticule.texture, pos, null, Color.White, 0f, new Vec2(_reticule.width / 2, _reticule.height / 2), new Vec2(0.5f, 0.5f), SpriteEffects.None, 0.9f + base.depth.span);
		}
	}

	public override void Draw()
	{
		if (base._trapped != null)
		{
			base.y = -10000f;
		}
		if (graphic != null)
		{
			graphic.flipH = offDir <= 0;
			graphic.scale = base.scale;
			if (Level.current is Editor)
			{
				graphic.center = center;
				graphic.position = position;
			}
			else if (_stance != TargetStance.Fly)
			{
				graphic.center = center + new Vec2(0f, 10f);
				graphic.position = position + new Vec2(0f, 10f);
			}
			else
			{
				graphic.center = center + new Vec2(-12f, 10f);
				graphic.position = position + new Vec2(-12 * offDir, 10f);
			}
			graphic.depth = base.depth;
			graphic.alpha = base.alpha;
			graphic.angle = angle;
			graphic.Draw();
			if (_popup && _up)
			{
				DrawHat();
			}
		}
	}

	public override BinaryClassChunk Serialize()
	{
		BinaryClassChunk binaryClassChunk = base.Serialize();
		binaryClassChunk.AddProperty("stanceSetting", stanceSetting);
		binaryClassChunk.AddProperty("contains", Editor.SerializeTypeName(contains));
		binaryClassChunk.AddProperty("chestPlate", chestPlate);
		binaryClassChunk.AddProperty("helmet", helmet);
		return binaryClassChunk;
	}

	public override bool Deserialize(BinaryClassChunk node)
	{
		base.Deserialize(node);
		stanceSetting = node.GetProperty<int>("stanceSetting");
		contains = Editor.DeSerializeTypeName(node.GetProperty<string>("contains"));
		chestPlate = node.GetProperty<bool>("chestPlate");
		helmet = node.GetProperty<bool>("helmet");
		return true;
	}

	public override DXMLNode LegacySerialize()
	{
		DXMLNode dXMLNode = base.LegacySerialize();
		dXMLNode.Add(new DXMLNode("stanceSetting", Change.ToString(stanceSetting)));
		dXMLNode.Add(new DXMLNode("contains", (contains != null) ? contains.AssemblyQualifiedName : ""));
		dXMLNode.Add(new DXMLNode("chestPlate", Change.ToString(chestPlate)));
		dXMLNode.Add(new DXMLNode("helmet", Change.ToString(helmet)));
		return dXMLNode;
	}

	public override bool LegacyDeserialize(DXMLNode node)
	{
		base.LegacyDeserialize(node);
		DXMLNode n = node.Element("stanceSetting");
		if (n != null)
		{
			stanceSetting = Convert.ToInt32(n.Value);
		}
		DXMLNode typeNode = node.Element("contains");
		if (typeNode != null)
		{
			Type t = Editor.GetType(typeNode.Value);
			contains = t;
		}
		n = node.Element("chestPlate");
		if (n != null)
		{
			chestPlate = Convert.ToBoolean(n.Value);
		}
		n = node.Element("helmet");
		if (n != null)
		{
			helmet = Convert.ToBoolean(n.Value);
		}
		return true;
	}

	public override string GetDetailsString()
	{
		string containString = "NONE";
		if (contains != null)
		{
			containString = contains.Name;
		}
		return base.GetDetailsString() + "Order: " + base.sequence.order + "\nHolding: " + containString;
	}

	public override void Netted(Net n)
	{
		base.Netted(n);
		base.y -= 10000f;
		base._trapped.infinite = true;
	}

	public override void EditorUpdate()
	{
		if (chestPlate && GetEquipment(typeof(ChestPlate)) == null)
		{
			Equip(new ChestPlate(0f, 0f), makeSound: false);
		}
		else if (!chestPlate)
		{
			Equipment cp = GetEquipment(typeof(ChestPlate));
			if (cp != null)
			{
				Unequip(cp);
			}
		}
		if (helmet && GetEquipment(typeof(Helmet)) == null)
		{
			Equip(new Helmet(0f, 0f), makeSound: false);
		}
		else if (!helmet)
		{
			Equipment cp2 = GetEquipment(typeof(Helmet));
			if (cp2 != null)
			{
				Unequip(cp2);
			}
		}
		if (contains != null)
		{
			if (holdObject == null || holdObject.GetType() != contains)
			{
				Holdable t = Editor.CreateThing(contains) as Holdable;
				GiveHoldable(t);
			}
		}
		else
		{
			holdObject = null;
		}
		foreach (Equipment item in _equipment)
		{
			item.DoUpdate();
		}
		if (holdObject != null)
		{
			editorUpdate = true;
			UpdateHoldPosition();
			holdObject.DoUpdate();
			editorUpdate = false;
		}
		base.EditorUpdate();
	}

	public override void EditorRender()
	{
		foreach (Equipment item in _equipment)
		{
			item.DoDraw();
		}
		if (holdObject != null)
		{
			holdObject.depth = 0.9f;
			holdObject.DoDraw();
		}
		base.EditorRender();
	}

	public override ContextMenu GetContextMenu()
	{
		EditorGroupMenu obj = base.GetContextMenu() as EditorGroupMenu;
		obj.AddItem(new ContextRadio("Stand", stanceSetting == 0, 0, null, new FieldBinding(this, "stanceSetting")));
		obj.AddItem(new ContextRadio("Crouch", stanceSetting == 2, 2, null, new FieldBinding(this, "stanceSetting")));
		obj.AddItem(new ContextRadio("Slide", stanceSetting == 3, 3, null, new FieldBinding(this, "stanceSetting")));
		obj.AddItem(new ContextRadio("Fly", stanceSetting == 4, 4, null, new FieldBinding(this, "stanceSetting")));
		obj.AddItem(new ContextCheckBox("Chest Plate", null, new FieldBinding(this, "chestPlate")));
		obj.AddItem(new ContextCheckBox("Helmet", null, new FieldBinding(this, "helmet")));
		FieldBinding binding = new FieldBinding(this, "contains");
		EditorGroupMenu contain = new EditorGroupMenu(obj);
		contain.InitializeTypelist(typeof(PhysicsObject), binding);
		contain.text = "Holding";
		obj.AddItem(contain);
		return obj;
	}
}
