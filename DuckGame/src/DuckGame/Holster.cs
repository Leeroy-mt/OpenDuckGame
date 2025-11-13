using System;
using System.Linq;

namespace DuckGame;

[EditorGroup("Equipment")]
[BaggedProperty("isInDemo", true)]
public class Holster : Equipment
{
	public StateBinding _containedObjectBinding = new StateBinding("netContainedObject");

	public StateBinding _netRaiseBinding = new StateBinding("netRaise");

	public StateBinding _netChainedBinding = new StateBinding("netChained");

	public bool netRaise;

	protected SpriteMap _sprite;

	protected SpriteMap _overPart;

	protected SpriteMap _underPart;

	public EditorProperty<bool> infinite = new EditorProperty<bool>(val: false);

	public EditorProperty<bool> chained = new EditorProperty<bool>(val: false);

	private Holdable _containedObject;

	private RenderTarget2D _preview;

	private Sprite _previewSprite;

	private Type _contains;

	private Sprite _chain;

	private Sprite _lock;

	private Vec2 _prevPos = Vec2.Zero;

	private float afterDrawAngle = -999999f;

	private float _chainSway;

	private float _chainSwayVel;

	protected float backOffset = -8f;

	public override NetworkConnection connection
	{
		get
		{
			return base.connection;
		}
		set
		{
			base.connection = value;
			if (containedObject != null)
			{
				containedObject.connection = value;
			}
		}
	}

	public bool netChained
	{
		get
		{
			return chained.value;
		}
		set
		{
			chained.value = value;
		}
	}

	public Holdable containedObject
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

	public Holdable netContainedObject
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

	public Type contains
	{
		get
		{
			return _contains;
		}
		set
		{
			_contains = value;
			if (!Level.skipInitialize)
			{
				if (_preview == null)
				{
					_preview = new RenderTarget2D(32, 32);
				}
				Thing t = GetContainedInstance();
				if (t != null)
				{
					_previewSprite = t.GetEditorImage(32, 32, transparentBack: true, null, _preview);
				}
			}
		}
	}

	public Thing GetContainedInstance(Vec2 pos = default(Vec2))
	{
		if (contains == null)
		{
			return null;
		}
		object[] p = Editor.GetConstructorParameters(contains);
		if (p.Count() > 1)
		{
			p[0] = pos.x;
			p[1] = pos.y;
		}
		PhysicsObject o = Editor.CreateThing(contains, p) as PhysicsObject;
		if (o is Gun)
		{
			(o as Gun).infinite = infinite;
		}
		return o;
	}

	public void SetContainedObject(Holdable h)
	{
		if (_containedObject != null)
		{
			_containedObject.visible = true;
			Fondle(_containedObject);
			_containedObject.owner = null;
			_containedObject = null;
		}
		if (h != null)
		{
			_containedObject = h;
			h.lastGrounded = DateTime.Now;
			h.visible = false;
		}
	}

	public virtual void EjectItem()
	{
		if (containedObject != null)
		{
			SFX.PlaySynchronized("pelletgunBad", 1f, Rando.Float(0.1f, 0.1f));
			containedObject.hSpeed = (float)(-owner.offDir) * 6f;
			containedObject.vSpeed = -1.5f;
			if (base.duck != null)
			{
				base.duck._lastHoldItem = containedObject;
				base.duck._timeSinceThrow = 0;
			}
			SetContainedObject(null);
		}
	}

	public virtual void EjectItem(Vec2 pSpeed)
	{
		if (containedObject != null)
		{
			SFX.PlaySynchronized("pelletgunBad", 1f, Rando.Float(0.1f, 0.1f));
			containedObject.hSpeed = pSpeed.x;
			containedObject.vSpeed = pSpeed.y;
			SetContainedObject(null);
		}
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

	public override DXMLNode LegacySerialize()
	{
		DXMLNode dXMLNode = base.LegacySerialize();
		dXMLNode.Add(new DXMLNode("contains", (contains != null) ? contains.AssemblyQualifiedName : ""));
		return dXMLNode;
	}

	public override bool LegacyDeserialize(DXMLNode node)
	{
		base.LegacyDeserialize(node);
		DXMLNode typeNode = node.Element("contains");
		if (typeNode != null)
		{
			Type t = Editor.GetType(typeNode.Value);
			contains = t;
		}
		return true;
	}

	public override ContextMenu GetContextMenu()
	{
		FieldBinding binding = new FieldBinding(this, "contains");
		EditorGroupMenu obj = base.GetContextMenu() as EditorGroupMenu;
		obj.InitializeGroups(new EditorGroup(typeof(PhysicsObject)), binding);
		return obj;
	}

	public override string GetDetailsString()
	{
		string containString = "EMPTY";
		if (contains != null)
		{
			containString = contains.Name;
		}
		if (contains == null)
		{
			return base.GetDetailsString();
		}
		return base.GetDetailsString() + "Contains: " + containString;
	}

	public override void DrawHoverInfo()
	{
		string containString = "EMPTY";
		if (contains != null)
		{
			containString = contains.Name;
		}
		Graphics.DrawString(containString, position + new Vec2((0f - Graphics.GetStringWidth(containString)) / 2f, -16f), Color.White, 0.9f);
	}

	public Holster(float xpos, float ypos)
		: base(xpos, ypos)
	{
		_chain = new Sprite("holsterChain");
		_chain.center = new Vec2(3f, 3f);
		_lock = new Sprite("holsterLock");
		_lock.center = new Vec2(3f, 2f);
		_sprite = new SpriteMap("holster", 12, 12);
		_overPart = new SpriteMap("holster_over", 10, 3);
		_overPart.center = new Vec2(6f, -1f);
		_underPart = new SpriteMap("holster_under", 8, 9);
		_underPart.center = new Vec2(10f, 8f);
		graphic = _sprite;
		collisionOffset = new Vec2(-5f, -5f);
		collisionSize = new Vec2(10f, 10f);
		center = new Vec2(6f, 6f);
		physicsMaterial = PhysicsMaterial.Wood;
		_equippedDepth = 4;
		_wearOffset = new Vec2(1f, 1f);
		editorTooltip = "Lets you carry around an additional item!";
	}

	protected override bool OnDestroy(DestroyType type = null)
	{
		if (owner != null || containedObject != null)
		{
			return false;
		}
		return base.OnDestroy(type);
	}

	public override void Initialize()
	{
		if (!(Level.current is Editor) && GetContainedInstance(position) is Holdable t)
		{
			Level.Add(t);
			SetContainedObject(t);
			if (Network.isActive && Thing.loadingLevel != null && _containedObject != null)
			{
				_containedObject.PrepareForHost();
			}
		}
		base.Initialize();
	}

	public override void Update()
	{
		if (_equippedDuck != null && base.duck == null)
		{
			return;
		}
		if (destroyed)
		{
			base.alpha -= 0.05f;
		}
		if (base.alpha < 0f)
		{
			Level.Remove(this);
		}
		if (base.isServerForObject)
		{
			netRaise = false;
			if (_equippedDuck != null && _equippedDuck.inputProfile != null && _equippedDuck.inputProfile.Down("UP"))
			{
				netRaise = true;
			}
			if (owner == null && base.equippedDuck == null)
			{
				base.angleDegrees = 0f;
			}
			if (containedObject != null)
			{
				PositionContainedObject();
				_containedObject.HolsterUpdate(this);
				weight = containedObject.weight;
				if (base.duck != null)
				{
					containedObject.owner = base.duck;
				}
				else
				{
					containedObject.owner = this;
				}
				if (base.duck != null && base.duck.ragdoll != null)
				{
					containedObject.solid = false;
					containedObject.grounded = false;
				}
				else
				{
					if (base.equippedDuck != null)
					{
						containedObject.solid = base.equippedDuck.velocity.length < 0.05f;
					}
					else
					{
						containedObject.solid = base.velocity.length < 0.05f;
					}
					containedObject.grounded = true;
				}
				if (!containedObject.isServerForObject && !(containedObject is IAmADuck))
				{
					Fondle(containedObject);
				}
				if (containedObject.removeFromLevel || containedObject.y < base.level.topLeft.y - 2000f || !containedObject.active || !containedObject.isServerForObject)
				{
					SetContainedObject(null);
				}
			}
			if (containedObject is Gun && Level.CheckRect<FunBeam>(containedObject.position + new Vec2(-4f, -4f), containedObject.position + new Vec2(4f, 4f)) != null)
			{
				(containedObject as Gun).triggerAction = true;
			}
			if (containedObject is RagdollPart && (containedObject as RagdollPart).doll != null && (containedObject as RagdollPart).doll.part1 != null && (containedObject as RagdollPart).doll.part2 != null && (containedObject as RagdollPart).doll.part3 != null)
			{
				if ((containedObject as RagdollPart).doll.part1.x < (containedObject as RagdollPart).doll.part3.x - 4f)
				{
					(containedObject as RagdollPart).doll.part1.x = (containedObject as RagdollPart).doll.part3.x - 4f;
				}
				if ((containedObject as RagdollPart).doll.part1.x > (containedObject as RagdollPart).doll.part3.x + 4f)
				{
					(containedObject as RagdollPart).doll.part1.x = (containedObject as RagdollPart).doll.part3.x + 4f;
				}
				Vec2 topTarget = (containedObject as RagdollPart).doll.part3.position + new Vec2(0f, -11f);
				Vec2 middleTarget = (containedObject as RagdollPart).doll.part3.position + new Vec2(0f, -5f);
				(containedObject as RagdollPart).doll.part1.x = Lerp.FloatSmooth((containedObject as RagdollPart).doll.part1.x, topTarget.x, 0.5f);
				(containedObject as RagdollPart).doll.part1.y = Lerp.FloatSmooth((containedObject as RagdollPart).doll.part1.y, topTarget.y, 0.5f);
				(containedObject as RagdollPart).doll.part2.x = Lerp.FloatSmooth((containedObject as RagdollPart).doll.part1.x, middleTarget.x, 0.5f);
				(containedObject as RagdollPart).doll.part2.y = Lerp.FloatSmooth((containedObject as RagdollPart).doll.part1.y, middleTarget.y, 0.5f);
				topTarget = (topTarget - (containedObject as RagdollPart).doll.part3.position).normalized;
				(containedObject as RagdollPart).doll.part1.vSpeed = topTarget.y;
				(containedObject as RagdollPart).doll.part2.vSpeed = topTarget.y;
				(containedObject as RagdollPart).doll.part1.hSpeed = topTarget.x;
				(containedObject as RagdollPart).doll.part2.hSpeed = topTarget.x;
				(containedObject as RagdollPart).doll.part1.vSpeed *= 0.8f;
				(containedObject as RagdollPart).doll.part1.hSpeed *= 0.8f;
				(containedObject as RagdollPart).doll.part2.vSpeed *= 0.8f;
				(containedObject as RagdollPart).doll.part2.hSpeed *= 0.8f;
			}
			if (containedObject != null && !(containedObject is Equipment))
			{
				containedObject.UpdateAction();
				if (containedObject is TapedGun)
				{
					(containedObject as TapedGun).UpdateSubActions(containedObject.triggerAction);
				}
			}
		}
		base.Update();
	}

	protected virtual void DrawParts()
	{
		if (_equippedDuck != null)
		{
			_ = owner.depth;
			_overPart.flipH = owner.offDir <= 0;
			_overPart.angle = angle;
			_overPart.alpha = base.alpha;
			_overPart.scale = base.scale;
			_overPart.depth = owner.depth + 5;
			Graphics.Draw(_overPart, base.x, base.y);
			_underPart.flipH = owner.offDir <= 0;
			_underPart.angle = angle;
			_underPart.alpha = base.alpha;
			_underPart.scale = base.scale;
			if (_equippedDuck.ragdoll != null && _equippedDuck.ragdoll.part2 != null)
			{
				_underPart.depth = _equippedDuck.ragdoll.part2.depth + -11;
			}
			else
			{
				_underPart.depth = owner.depth + -7;
			}
			Graphics.Draw(_underPart, base.x, base.y);
		}
	}

	private void PositionContainedObject()
	{
		if (_equippedDuck != null)
		{
			_containedObject.position = Offset(new Vec2(backOffset, -4f) + containedObject.holsterOffset);
			_containedObject.depth = owner.depth + -14;
			_containedObject.angleDegrees = ((owner.offDir > 0) ? containedObject.holsterAngle : (0f - containedObject.holsterAngle)) + base.angleDegrees;
			_containedObject.offDir = (sbyte)((owner.offDir > 0) ? 1 : (-1));
			if (containedObject is RagdollPart)
			{
				_containedObject.position = Offset(new Vec2(backOffset, 0f));
				_containedObject.angleDegrees += ((owner.offDir > 0) ? 90 : (-90));
				if (base.duck != null && base.duck.ragdoll == null)
				{
					afterDrawAngle = _containedObject.angleDegrees;
					_containedObject.angleDegrees -= base.duck.hSpeed * 3f;
				}
			}
			if (owner is Duck && (owner as Duck).ragdoll != null)
			{
				RagdollPart p = (owner as Duck).ragdoll.part2;
				if (p != null)
				{
					_containedObject.depth = p.depth + -14;
				}
			}
		}
		else
		{
			_containedObject.position = Offset(new Vec2(backOffset + 6f, -2f) + containedObject.holsterOffset);
			_containedObject.depth = base.depth + -14;
			_containedObject.angleDegrees = ((offDir > 0) ? containedObject.holsterAngle : (0f - containedObject.holsterAngle)) + base.angleDegrees;
			_containedObject.offDir = (sbyte)((offDir > 0) ? 1 : (-1));
		}
	}

	public override void Draw()
	{
		if (Level.current is Editor && _previewSprite != null)
		{
			_previewSprite.depth = base.depth + 1;
			_previewSprite.scale = new Vec2(0.5f, 0.5f);
			_previewSprite.center = new Vec2(16f, 16f);
			Graphics.Draw(_previewSprite, base.x, base.y);
		}
		if (_equippedDuck != null)
		{
			graphic = null;
		}
		else
		{
			graphic = _sprite;
		}
		base.Draw();
		if (_equippedDuck != null && base.duck == null)
		{
			return;
		}
		DrawParts();
		if (_containedObject != null)
		{
			_ = offDir;
			PositionContainedObject();
			if (chained.value)
			{
				float xOffChange = ((_equippedDuck != null) ? 0f : 8f);
				_chain.CenterOrigin();
				_chain.depth = _underPart.depth + 1;
				_chain.angleDegrees = base.angleDegrees - (float)(45 * offDir);
				Vec2 chainOff = Offset(new Vec2(-11f + xOffChange, -3f));
				Graphics.Draw(_chain, chainOff.x, chainOff.y);
				_lock.angleDegrees = _chainSway;
				float desiredDegrees = ((owner != null) ? owner.hSpeed : hSpeed) * 10f;
				_chainSwayVel -= (_lock.angleDegrees - desiredDegrees) * 0.1f;
				_chainSwayVel *= 0.95f;
				_chainSway += _chainSwayVel;
				_lock.depth = _underPart.depth + 2;
				Offset(new Vec2(-9f + xOffChange, -5f));
				Graphics.Draw(_lock, chainOff.x, chainOff.y);
			}
			if (!(containedObject is RagdollPart) || !Network.isActive)
			{
				_containedObject.Draw();
			}
			if (afterDrawAngle > -99999f)
			{
				_containedObject.angleDegrees = afterDrawAngle;
			}
		}
		else if (chained.value)
		{
			_chain.depth = base.depth + 1;
			Vec2 chainOff2 = Offset(new Vec2(-3f, -2f));
			if (base.equippedDuck != null)
			{
				chainOff2 = Offset(new Vec2(-9f, -2f));
			}
			_chain.center = new Vec2(3f, 3f);
			Graphics.Draw(_chain, chainOff2.x, chainOff2.y);
			Offset(new Vec2(0f, -8f));
			_chain.angleDegrees = 90f + _chainSway;
			float desiredDegrees2 = 90f + ((owner != null) ? owner.hSpeed : hSpeed) * 10f;
			_chainSwayVel -= (_chain.angleDegrees - desiredDegrees2) * 0.1f;
			_chainSwayVel *= 0.95f;
			_chainSway += _chainSwayVel;
		}
	}
}
