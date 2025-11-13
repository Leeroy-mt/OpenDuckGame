using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace DuckGame;

[BaggedProperty("canSpawn", false)]
public class TeamHat : Hat
{
	public class CustomParticle : PhysicsParticle, IFactory
	{
		private Team.CustomHatMetadata _metadata;

		private Vec2 _particleAlpha;

		private Vec2 _particleScale;

		private Vec2 _particleGravity;

		private Vec2 _particleFriction;

		private Vec2 _particleRotation;

		public List<Texture2D> animationFrames;

		public float animationSpeed;

		public bool animationLoop;

		private float _currentAnimationFrame;

		private float _lifespan;

		private Vec2 _movementDif;

		private Vec2 _prevOwnerPosition;

		private Vec2 _gravityVelocity;

		public CustomParticle(Vec2 pPosition, Thing pOwner, Team.CustomHatMetadata pMetadata)
			: base(pPosition.x, pPosition.y)
		{
			_metadata = pMetadata;
			_owner = pOwner;
			_gravMult = 0f;
			graphic = new Sprite(_metadata.team.customParticles[Rando.Int(_metadata.team.customParticles.Count - 1)]);
			graphic.CenterOrigin();
			offDir = pOwner.offDir;
			if (!_metadata.HatNoFlip.value)
			{
				graphic.flipH = offDir < 0;
			}
			center = new Vec2(graphic.width / 2, graphic.height / 2);
			_lifespan = _metadata.ParticleLifespan.value;
			_prevOwnerPosition = _owner.position;
			_particleAlpha = _metadata.ParticleAlpha.value;
			_particleScale = _metadata.ParticleScale.value;
			_particleGravity = new Vec2(_owner.OffsetLocal(_metadata.ParticleGravity.value).x, _metadata.ParticleGravity.value.y);
			_particleFriction = _metadata.ParticleFriction.value;
			_particleRotation = _metadata.ParticleRotation.value;
			base.depth = (_metadata.ParticleBackground.value ? (pOwner.depth - 8) : (pOwner.depth + 8));
			base.velocity = _owner.OffsetLocal(_metadata.ParticleVelocity.value);
			_life = 1f;
			if (!_metadata.ParticleAnchor.value)
			{
				sbyte ownerOffdir = _owner.offDir;
				if (_metadata.HatNoFlip.value)
				{
					_owner.offDir = 1;
				}
				position = _owner.Offset(position);
				_owner.offDir = ownerOffdir;
			}
			if (_metadata.ParticleAnimationRandomFrame.value)
			{
				_currentAnimationFrame = Rando.Int(_metadata.team.customParticles.Count - 1);
			}
			UpdateAppearance();
		}

		private void UpdateAppearance()
		{
			float num = (base.yscale = Lerp.FloatSmooth(_particleScale.x, _particleScale.y, 1f - _life));
			base.xscale = num;
			base.xscale = Maths.Clamp(base.xscale, 0f, 1f);
			base.yscale = Maths.Clamp(base.yscale, 0f, 1f);
			base.alpha = Lerp.FloatSmooth(_particleAlpha.x, _particleAlpha.y, 1f - _life);
			base.angleDegrees = Lerp.FloatSmooth(_particleRotation.x, _particleRotation.y, 1f - _life) * 10f;
		}

		public override void Update()
		{
			base.velocity += _particleGravity;
			base.velocity *= _particleFriction;
			hSpeed = Maths.Clamp(hSpeed, -4f, 4f);
			vSpeed = Maths.Clamp(vSpeed, -4f, 4f);
			float die = 60f / (60f * _lifespan) * Maths.IncFrameTimer();
			_life -= die;
			UpdateAppearance();
			if (_life <= 0f)
			{
				Level.Remove(this);
			}
			if (_metadata.ParticleAnchor.value)
			{
				Vec2 p = position;
				sbyte ownerOffdir = _owner.offDir;
				if (_metadata.HatNoFlip.value)
				{
					_owner.offDir = 1;
				}
				position = _owner.Offset(position);
				_owner.offDir = ownerOffdir;
				Vec2 preUpdate = position;
				base.Update();
				position = p + (position - preUpdate);
			}
			else
			{
				base.Update();
			}
			if (animationFrames != null)
			{
				graphic.texture = animationFrames[(int)_currentAnimationFrame % animationFrames.Count];
				_currentAnimationFrame += animationSpeed;
				if (!animationLoop && _currentAnimationFrame >= (float)animationFrames.Count)
				{
					_currentAnimationFrame = animationFrames.Count - 1;
				}
			}
		}

		public override void Draw()
		{
			if (_metadata.ParticleAnchor.value)
			{
				Vec2 p = position;
				sbyte ownerOffdir = _owner.offDir;
				if (_metadata.HatNoFlip.value)
				{
					_owner.offDir = 1;
				}
				position = _owner.Offset(position);
				_owner.offDir = ownerOffdir;
				float ang = angle;
				if (_metadata.ParticleAnchorOrientation.value)
				{
					base.angleDegrees += _owner.angleDegrees;
				}
				Vec2 preUpdate = position;
				base.Draw();
				position = p + (position - preUpdate);
				angle = ang;
			}
			else
			{
				base.Draw();
			}
		}
	}

	public bool hasBeenStolen;

	private float _timeOpen;

	private int _prevFrame;

	private Sprite _specialSprite;

	private SinWaveManualUpdate _wave = 0.1f;

	private float _fade;

	public StateBinding _netTeamIndexBinding = new StateBinding("netTeamIndex");

	private bool _shouldUpdateSprite;

	private Team _team;

	public DuckPersona quickPersona;

	private Team _lastLoadedTeam;

	private string _prevHatID;

	private Profile _profile;

	private Cape _cape;

	private MaterialKatanaman _katanaMaterial;

	private bool _isKatanaHat;

	private bool _specialInitialized;

	private int _networkCape = -1;

	private float glow;

	private bool _filter;

	private int _lastParticleFrame;

	private List<CustomParticle> _addedParticles;

	private float _quackWait;

	private float _quackHold;

	public ushort netTeamIndex
	{
		get
		{
			if (_team == null)
			{
				return 0;
			}
			return (ushort)Teams.IndexOf(_team);
		}
		set
		{
			team = Teams.ParseFromIndex(value);
		}
	}

	public Team team
	{
		get
		{
			return _team;
		}
		set
		{
			_ = _team;
			_team = value;
			_shouldUpdateSprite = true;
		}
	}

	public override void SetQuack(int pValue)
	{
		PositionOnOwner();
		frame = pValue;
		if (_equippedDuck != null && !destroyed)
		{
			if (_prevFrame == 0 && _sprite.frame == 1)
			{
				OpenHat();
			}
			else if (_prevFrame == 1 && _sprite.frame == 0)
			{
				CloseHat();
			}
		}
		_prevFrame = _sprite.frame;
	}

	public void UpdateSprite()
	{
		if (_profile == null && base.equippedDuck != null && base.equippedDuck.profile == Profiles.EnvironmentProfile)
		{
			_shouldUpdateSprite = true;
			return;
		}
		if (_team != null && ((_team != _lastLoadedTeam && (_team.facade == null || _team.facade != _lastLoadedTeam)) || _prevHatID != _team.hatID || _team.filter != _filter))
		{
			_filter = _team.filter;
			if (_profile == null && base.equippedDuck != null)
			{
				_profile = base.equippedDuck.profile;
			}
			base.sprite = _team.hat.CloneMap();
			base.pickupSprite = _team.hat.Clone();
			DuckPersona p = quickPersona;
			if (_profile != null)
			{
				p = _profile.persona;
			}
			if (p != null && _team.metadata != null && _team.metadata.UseDuckColor.value)
			{
				base.sprite = _team.GetHat(p).CloneMap();
				base.pickupSprite = _team.GetHat(p).Clone();
			}
			base.sprite.center = new Vec2(16f, 16f);
			base.hatOffset = _team.hatOffset;
			UpdateCape();
			_lastLoadedTeam = ((_team.facade != null) ? _team.facade : _team);
			_prevHatID = _team.hatID;
			graphic = base.sprite;
		}
		if (!_specialInitialized && _team != null)
		{
			_specialInitialized = true;
			_isKatanaHat = _sprite.texture.textureName == "hats/katanaman";
			if (_isKatanaHat)
			{
				_katanaMaterial = new MaterialKatanaman(this);
			}
		}
	}

	public TeamHat(float xpos, float ypos, Team t)
		: base(xpos, ypos)
	{
		team = t;
		base.depth = -0.5f;
	}

	public TeamHat(float xpos, float ypos, Team t, Profile p)
		: base(xpos, ypos)
	{
		_profile = p;
		team = t;
		base.depth = -0.5f;
	}

	public override BinaryClassChunk Serialize()
	{
		BinaryClassChunk binaryClassChunk = base.Serialize();
		binaryClassChunk.AddProperty("teamIndex", Teams.all.IndexOf(_team));
		return binaryClassChunk;
	}

	public override bool Deserialize(BinaryClassChunk node)
	{
		int teamIndex = node.GetProperty<int>("teamIndex");
		if (teamIndex >= 0 && teamIndex < Teams.all.Count - 1)
		{
			team = Teams.all[teamIndex];
		}
		return base.Deserialize(node);
	}

	public override void Initialize()
	{
		UpdateCape();
		base.Initialize();
	}

	public void UpdateCape()
	{
		if (_team == null)
		{
			return;
		}
		if (_sprite.texture.textureName == "hats/johnnys")
		{
			quacks = false;
		}
		if (_cape != null)
		{
			if (_cape.level != null)
			{
				Level.Remove(_cape);
			}
			_cape = null;
		}
		if (_sprite.texture.textureName == "hats/suit")
		{
			Tex2D capeTexture = null;
			int flagIndex = Global.data.flag;
			if (Network.isActive)
			{
				flagIndex = _networkCape;
			}
			if (flagIndex < 0)
			{
				return;
			}
			Sprite s = UIFlagSelection.GetFlag(flagIndex);
			if (s != null)
			{
				capeTexture = s.texture;
			}
			if (capeTexture != null)
			{
				_cape = new Cape(base.x, base.y, this);
				if (!capeTexture.textureName.Contains("full_"))
				{
					_cape.halfFlag = true;
				}
				_cape.SetCapeTexture(capeTexture);
			}
		}
		else if (_team.capeTexture != null)
		{
			_cape = new Cape(base.x, base.y, this);
			_cape.SetCapeTexture(_team.capeTexture);
			if (_team.metadata != null)
			{
				_cape.metadata = _team.metadata;
			}
		}
	}

	public override void Terminate()
	{
		if (_cape != null)
		{
			Level.Remove(_cape);
		}
		base.Terminate();
	}

	public override void Update()
	{
		if (_cape != null && _cape.level == null)
		{
			Level.Add(_cape);
		}
		if (Network.isActive)
		{
			if (_team != null && _team.filter != _filter)
			{
				UpdateCape();
				_shouldUpdateSprite = true;
			}
			if (_networkCape < 0 && base.duck != null && base.duck.profile != null)
			{
				if (base.duck.profile.localPlayer)
				{
					_networkCape = Global.data.flag;
				}
				else
				{
					_networkCape = base.duck.profile.flagIndex;
				}
				UpdateCape();
			}
			if (Network.InLobby() && _team != null && (base.sprite == null || (base.sprite != null && base.sprite.globalIndex != _team.hat.globalIndex)))
			{
				_shouldUpdateSprite = true;
			}
		}
		else if (Level.current is TeamSelect2 && _equippedDuck != null && team != null && team.customHatPath != null && Keyboard.Pressed(Keys.F5) && !Network.isActive)
		{
			int idx = Teams.core.extraTeams.IndexOf(team);
			Team.deserializeInto = team;
			Teams.core.extraTeams[idx] = Team.Deserialize(team.customHatPath);
			Team.deserializeInto = null;
			Duck obj = _equippedDuck;
			_equippedDuck.Unequip(this);
			Level.Remove(this);
			TeamHat t = new TeamHat(base.x, base.y, Teams.core.extraTeams[idx]);
			Level.Add(t);
			obj.Equip(t, makeSound: false);
		}
		if (_shouldUpdateSprite)
		{
			_shouldUpdateSprite = false;
			UpdateSprite();
		}
		if (_equippedDuck != null && !destroyed)
		{
			if (_sprite.frame == 1)
			{
				_timeOpen += 0.1f;
			}
			else
			{
				_timeOpen = 0f;
			}
		}
		if (_sprite.frame == 1 && _prevFrame == 0)
		{
			glow = 1.2f;
		}
		_prevFrame = _sprite.frame;
		if (destroyed)
		{
			base.alpha -= 0.05f;
		}
		if (base.alpha < 0f)
		{
			Level.Remove(this);
		}
		if (_quackWait > 0f)
		{
			_quackWait -= Maths.IncFrameTimer();
		}
		else if (_quackHold > 0f)
		{
			_quackHold -= Maths.IncFrameTimer();
		}
		base.Update();
	}

	public override void Quack(float volume, float pitch)
	{
		if (base.duck != null && _sprite.texture.textureName == "hats/hearts")
		{
			SFX.Play("heartfart", volume, Math.Min(pitch + 0.4f - Rando.Float(0.1f), 1f));
			Level.Add(new HeartPuff(base.x, base.y)
			{
				anchor = this
			});
			for (int i = 0; i < 2; i++)
			{
				SmallSmoke smallSmoke = SmallSmoke.New(base.x, base.y);
				smallSmoke.sprite.color = Color.Green * (0.4f + Rando.Float(0.3f));
				Level.Add(smallSmoke);
			}
		}
		else
		{
			SFX.Play("quack", volume, pitch);
		}
	}

	public override void OpenHat()
	{
		if (base.duck == null || base.duck.z != 0f)
		{
			return;
		}
		if (team != null && team.metadata != null)
		{
			if (team.metadata.QuackSuppressRequack.value && (_quackWait > 0f || _quackHold > 0f))
			{
				return;
			}
			_quackWait = team.metadata.QuackDelay.value;
			_quackHold = team.metadata.QuackHold.value;
			if (team.customParticles.Count <= 0)
			{
				return;
			}
			if (_addedParticles == null)
			{
				_addedParticles = new List<CustomParticle>();
			}
			int numSpawn = team.metadata.ParticleCount.value;
			Vec2 emitTL = new Vec2((0f - team.metadata.ParticleEmitShapeSize.value.x) / 2f, (0f - team.metadata.ParticleEmitShapeSize.value.y) / 2f);
			Vec2 emitBR = new Vec2(team.metadata.ParticleEmitShapeSize.value.x / 2f, team.metadata.ParticleEmitShapeSize.value.y / 2f);
			Vec2 emissionPositionMain = team.metadata.ParticleEmitterOffset.value;
			for (int i = 0; i < numSpawn; i++)
			{
				Vec2 emissionPosition = emissionPositionMain;
				if (team.metadata.ParticleEmitShape.value.x == 1f)
				{
					float ang = Maths.DegToRad((team.metadata.ParticleEmitShape.value.y == 2f) ? ((float)i * (360f / (float)numSpawn)) : Rando.Float(360f));
					Vec2 angVec = new Vec2((float)Math.Cos(ang) * (team.metadata.ParticleEmitShapeSize.value.x / 2f), (float)(0.0 - Math.Sin(ang)) * (team.metadata.ParticleEmitShapeSize.value.y / 2f));
					if (team.metadata.ParticleEmitShape.value.y == 1f)
					{
						emissionPosition += angVec * Rando.Float(1f);
					}
					else
					{
						emissionPosition += angVec;
					}
				}
				else if (team.metadata.ParticleEmitShape.value.x == 2f)
				{
					if (team.metadata.ParticleEmitShape.value.y == 0f)
					{
						float sideMul = ((Rando.Float(1f) >= 0.5f) ? 1f : (-1f));
						if (Rando.Float(1f) >= 0.5f)
						{
							emissionPosition += new Vec2(team.metadata.ParticleEmitShapeSize.value.x * sideMul, Rando.Float((0f - team.metadata.ParticleEmitShapeSize.value.y) / 2f, team.metadata.ParticleEmitShapeSize.value.y / 2f));
						}
						else
						{
							emissionPosition += new Vec2(Rando.Float((0f - team.metadata.ParticleEmitShapeSize.value.x) / 2f, team.metadata.ParticleEmitShapeSize.value.x / 2f), team.metadata.ParticleEmitShapeSize.value.y * sideMul);
						}
					}
					else if (team.metadata.ParticleEmitShape.value.y == 1f)
					{
						emissionPosition += new Vec2(Rando.Float((0f - team.metadata.ParticleEmitShapeSize.value.x) / 2f, team.metadata.ParticleEmitShapeSize.value.x / 2f), Rando.Float((0f - team.metadata.ParticleEmitShapeSize.value.y) / 2f, team.metadata.ParticleEmitShapeSize.value.y / 2f));
					}
					else if (team.metadata.ParticleEmitShape.value.y == 2f)
					{
						float ang2 = Maths.DegToRad((team.metadata.ParticleEmitShape.value.y == 2f) ? ((float)i * (360f / (float)numSpawn)) : Rando.Float(360f));
						Vec2 angVec2 = new Vec2((float)Math.Cos(ang2) * 100f, (float)(0.0 - Math.Sin(ang2)) * 100f);
						Vec2 col = Vec2.Zero;
						for (int iSide = 0; iSide < 4; iSide++)
						{
							col = Vec2.Zero;
							switch (iSide)
							{
							case 0:
								if (Collision.LineIntersect(Vec2.Zero, angVec2, emitTL, new Vec2(emitTL.x, emitBR.y)))
								{
									col = Collision.LineIntersectPoint(Vec2.Zero, angVec2, emitTL, new Vec2(emitTL.x, emitBR.y));
								}
								break;
							case 1:
								if (Collision.LineIntersect(Vec2.Zero, angVec2, emitTL, new Vec2(emitBR.x, emitTL.y)))
								{
									col = Collision.LineIntersectPoint(Vec2.Zero, angVec2, emitTL, new Vec2(emitBR.x, emitTL.y));
								}
								break;
							case 2:
								if (Collision.LineIntersect(Vec2.Zero, angVec2, new Vec2(emitTL.x, emitBR.y), emitBR))
								{
									col = Collision.LineIntersectPoint(Vec2.Zero, angVec2, new Vec2(emitTL.x, emitBR.y), emitBR);
								}
								break;
							case 3:
								if (Collision.LineIntersect(Vec2.Zero, angVec2, new Vec2(emitBR.x, emitTL.y), emitBR))
								{
									col = Collision.LineIntersectPoint(Vec2.Zero, angVec2, new Vec2(emitBR.x, emitTL.y), emitBR);
								}
								break;
							}
							if (col != Vec2.Zero)
							{
								angVec2 = col;
								break;
							}
						}
						emissionPosition += angVec2;
					}
				}
				CustomParticle c = new CustomParticle(emissionPosition, this, team.metadata);
				if (team.metadata.ParticleAnimated.value)
				{
					c.animationFrames = team.customParticles;
					c.animationSpeed = team.metadata.ParticleAnimationSpeed.value * 0.5f;
					c.animationLoop = team.metadata.ParticleAnimationLoop.value;
				}
				Level.Add(c);
				_addedParticles.Add(c);
			}
		}
		else if (_sprite.texture.textureName == "hats/burgers")
		{
			FluidData dat = Fluid.Ketchup;
			dat.amount = Rando.Float(0.0005f, 0.001f);
			int val = Rando.Int(4) + 1;
			for (int j = 0; j < val; j++)
			{
				Level.Add(new Fluid(base.x + (float)base.duck.offDir * (2f + Rando.Float(0f, 7f)), base.y + 3f + Rando.Float(0f, 3f), new Vec2((float)base.duck.offDir * Rando.Float(0.5f, 3f), Rando.Float(0f, -2f)), dat, null, 2.5f)
				{
					depth = base.depth + 1
				});
			}
		}
		else if (_sprite.texture.textureName == "hats/divers" || _sprite.texture.textureName == "hats/fridge")
		{
			FluidData dat2 = Fluid.Water;
			dat2.amount = Rando.Float(0.0001f, 0.0005f);
			int val2 = Rando.Int(3) + 1;
			for (int k = 0; k < val2; k++)
			{
				Level.Add(new Fluid(base.x + (float)base.duck.offDir * (2f + Rando.Float(0f, 4f)), base.y + 3f + Rando.Float(0f, 3f), new Vec2((float)base.duck.offDir * Rando.Float(0.5f, 3f), Rando.Float(0f, -2f)), dat2, null, 5f)
				{
					depth = base.depth + 1
				});
			}
		}
		else if (_sprite.texture.textureName == "hats/gross")
		{
			FluidData dat3 = Fluid.Water;
			dat3.amount = Rando.Float(0.0002f, 0.0007f);
			int val3 = Rando.Int(6) + 2;
			for (int l = 0; l < val3; l++)
			{
				Level.Add(new Fluid(base.x + (float)base.duck.offDir * (6f + Rando.Float(-2f, 4f)), base.y + Rando.Float(-2f, 4f), new Vec2((float)base.duck.offDir * Rando.Float(1.2f, 4f), Rando.Float(0f, -2.8f)), dat3, null, 5f)
				{
					depth = base.depth + 1
				});
			}
		}
		else if (_sprite.texture.textureName == "hats/tube")
		{
			for (int m = 0; m < 4; m++)
			{
				Level.Add(new TinyBubble(base.x + Rando.Float(-4f, 4f), base.y + Rando.Float(0f, 4f), Rando.Float(-1.5f, 1.5f), base.y - 12f, blue: true)
				{
					depth = base.depth + 1
				});
			}
		}
	}

	public override void CloseHat()
	{
		if (base.duck == null)
		{
			return;
		}
		if (team != null && team.metadata != null)
		{
			if (team.metadata.WetLips.value && _timeOpen > 1f)
			{
				SFX.Play("smallSplat", 0.9f, Rando.Float(-0.4f, 0.4f));
			}
			if (team.metadata.MechanicalLips.value && _timeOpen > 2f)
			{
				SFX.Play("smallDoorShut", 1f, Rando.Float(-0.1f, 0.1f));
			}
		}
		if (_sprite.texture.textureName == "hats/burgers")
		{
			if (_timeOpen > 1f)
			{
				FluidData dat = Fluid.Ketchup;
				dat.amount = Rando.Float(0.0005f, 0.001f);
				int val = Rando.Int(3) + 1;
				for (int i = 0; i < val; i++)
				{
					Level.Add(new Fluid(base.x + (float)base.duck.offDir * (3f + Rando.Float(0f, 6f)), base.y + 4f + Rando.Float(0f, 1f), new Vec2((float)base.duck.offDir * Rando.Float(-2f, 2f), Rando.Float(-1f, -2f)), dat, null, 2.5f)
					{
						depth = base.depth + 1
					});
				}
				SFX.Play("smallSplat", 0.9f, Rando.Float(-0.4f, 0.4f));
			}
		}
		else if ((_sprite.texture.textureName == "hats/divers" || _sprite.texture.textureName == "hats/fridge") && _timeOpen > 2f)
		{
			SFX.Play("smallDoorShut", 1f, Rando.Float(-0.1f, 0.1f));
		}
	}

	public override void Draw()
	{
		int hatFrame = _sprite.frame;
		sbyte prevOffDir = offDir;
		if (_team == null && base.duck != null)
		{
			_team = base.duck.team;
		}
		Vec2 poss = _hatOffset;
		if (_team != null)
		{
			if (_team.noCrouchOffset && base.duck != null && base.duck.crouch)
			{
				_hatOffset.y += 1f;
			}
			if (_team.metadata != null)
			{
				if (_team.metadata.HatNoFlip.value && offDir < 0)
				{
					_hatOffset.x -= 4f;
				}
				if (base.duck != null && base.duck.sliding)
				{
					_hatOffset.y += 1f;
				}
				if (_quackWait > 0f)
				{
					_sprite.frame = 0;
				}
				else if (_quackHold > 0f)
				{
					_sprite.frame = 1;
				}
			}
		}
		_wave.Update();
		if (_isKatanaHat && !(Level.current is RockScoreboard))
		{
			Graphics.material = _katanaMaterial;
			base.Draw();
			Graphics.material = null;
		}
		else if (_team != null && _team.metadata != null)
		{
			PositionOnOwner();
			if (graphic != null)
			{
				if (!_team.metadata.HatNoFlip.value)
				{
					graphic.flipH = offDir <= 0;
				}
				_graphic.position = position;
				_graphic.alpha = base.alpha;
				_graphic.angle = angle;
				_graphic.depth = base.depth;
				_graphic.scale = base.scale;
				_graphic.center = center;
				_graphic.Draw();
			}
		}
		else
		{
			base.Draw();
		}
		_hatOffset = poss;
		if (base.duck != null)
		{
			if (_sprite.texture.textureName == "hats/sensei")
			{
				if (_specialSprite == null)
				{
					_specialSprite = new Sprite("hats/senpaiStar");
					_specialSprite.CenterOrigin();
				}
				_fade = Lerp.Float(_fade, (frame == 1) ? 1f : 0f, 0.1f);
				if (_fade > 0.01f)
				{
					_specialSprite.alpha = base.alpha * 0.7f * (0.5f + _wave.normalized * 0.5f) * _fade;
					_specialSprite.scale = base.scale;
					_specialSprite.depth = base.depth - 10;
					_specialSprite.angle += 0.02f;
					float s = 0.8f + _wave.normalized * 0.2f;
					_specialSprite.scale = new Vec2(s, s);
					Vec2 pos = Offset(new Vec2(2f, 4f));
					Graphics.Draw(_specialSprite, pos.x, pos.y);
				}
			}
			else if (_sprite.frame == 1 && _sprite.texture.textureName == "hats/master")
			{
				if (_specialSprite == null)
				{
					_specialSprite = new Sprite("hats/master_glow");
					_specialSprite.CenterOrigin();
				}
				_specialSprite.alpha = Math.Min(glow, 1f);
				_specialSprite.scale = base.scale;
				_specialSprite.depth = base.depth + 10;
				_specialSprite.angle = angle;
				if (offDir < 0)
				{
					Vec2 pos2 = Offset(new Vec2(1f, 2f));
					Graphics.Draw(_specialSprite, pos2.x, pos2.y);
					pos2 = Offset(new Vec2(5f, 2f));
					Graphics.Draw(_specialSprite, pos2.x, pos2.y);
				}
				else
				{
					Vec2 pos3 = Offset(new Vec2(0f, 2f));
					Graphics.Draw(_specialSprite, pos3.x, pos3.y);
					pos3 = Offset(new Vec2(4f, 2f));
					Graphics.Draw(_specialSprite, pos3.x, pos3.y);
				}
				if (glow > 0f)
				{
					glow -= 0.02f;
				}
			}
		}
		if (_addedParticles != null)
		{
			foreach (CustomParticle addedParticle in _addedParticles)
			{
				addedParticle.DoDraw();
			}
			_addedParticles.Clear();
		}
		_sprite.frame = hatFrame;
		offDir = prevOffDir;
	}
}
