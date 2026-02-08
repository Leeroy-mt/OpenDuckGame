using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace DuckGame;

[BaggedProperty("canSpawn", false)]
public class TeamHat : Hat
{
    public class CustomParticle : PhysicsParticle
    {
        private Team.CustomHatMetadata _metadata;

        private Vector2 _particleAlpha;

        private Vector2 _particleScale;

        private Vector2 _particleGravity;

        private Vector2 _particleFriction;

        private Vector2 _particleRotation;

        public List<Texture2D> animationFrames;

        public float animationSpeed;

        public bool animationLoop;

        private float _currentAnimationFrame;

        private float _lifespan;

        private Vector2 _movementDif;

        private Vector2 _prevOwnerPosition;

        private Vector2 _gravityVelocity;

        public CustomParticle(Vector2 pPosition, Thing pOwner, Team.CustomHatMetadata pMetadata)
            : base(pPosition.X, pPosition.Y)
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
            Center = new Vector2(graphic.width / 2, graphic.height / 2);
            _lifespan = _metadata.ParticleLifespan.value;
            _prevOwnerPosition = _owner.Position;
            _particleAlpha = _metadata.ParticleAlpha.value;
            _particleScale = _metadata.ParticleScale.value;
            _particleGravity = new Vector2(_owner.OffsetLocal(_metadata.ParticleGravity.value).X, _metadata.ParticleGravity.value.Y);
            _particleFriction = _metadata.ParticleFriction.value;
            _particleRotation = _metadata.ParticleRotation.value;
            base.Depth = (_metadata.ParticleBackground.value ? (pOwner.Depth - 8) : (pOwner.Depth + 8));
            base.velocity = _owner.OffsetLocal(_metadata.ParticleVelocity.value);
            _life = 1f;
            if (!_metadata.ParticleAnchor.value)
            {
                sbyte ownerOffdir = _owner.offDir;
                if (_metadata.HatNoFlip.value)
                {
                    _owner.offDir = 1;
                }
                Position = _owner.Offset(Position);
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
            float num = (base.ScaleY = Lerp.FloatSmooth(_particleScale.X, _particleScale.Y, 1f - _life));
            base.ScaleX = num;
            base.ScaleX = Maths.Clamp(base.ScaleX, 0f, 1f);
            base.ScaleY = Maths.Clamp(base.ScaleY, 0f, 1f);
            base.Alpha = Lerp.FloatSmooth(_particleAlpha.X, _particleAlpha.Y, 1f - _life);
            base.AngleDegrees = Lerp.FloatSmooth(_particleRotation.X, _particleRotation.Y, 1f - _life) * 10f;
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
                Vector2 p = Position;
                sbyte ownerOffdir = _owner.offDir;
                if (_metadata.HatNoFlip.value)
                {
                    _owner.offDir = 1;
                }
                Position = _owner.Offset(Position);
                _owner.offDir = ownerOffdir;
                Vector2 preUpdate = Position;
                base.Update();
                Position = p + (Position - preUpdate);
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
                Vector2 p = Position;
                sbyte ownerOffdir = _owner.offDir;
                if (_metadata.HatNoFlip.value)
                {
                    _owner.offDir = 1;
                }
                Position = _owner.Offset(Position);
                _owner.offDir = ownerOffdir;
                float ang = Angle;
                if (_metadata.ParticleAnchorOrientation.value)
                {
                    base.AngleDegrees += _owner.AngleDegrees;
                }
                Vector2 preUpdate = Position;
                base.Draw();
                Position = p + (Position - preUpdate);
                Angle = ang;
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

    public StateBinding _netTeamIndexBinding = new StateBinding(nameof(netTeamIndex));

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
            base.sprite.Center = new Vector2(16f, 16f);
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
        base.Depth = -0.5f;
    }

    public TeamHat(float xpos, float ypos, Team t, Profile p)
        : base(xpos, ypos)
    {
        _profile = p;
        team = t;
        base.Depth = -0.5f;
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
                _cape = new Cape(base.X, base.Y, this);
                if (!capeTexture.textureName.Contains("full_"))
                {
                    _cape.halfFlag = true;
                }
                _cape.SetCapeTexture(capeTexture);
            }
        }
        else if (_team.capeTexture != null)
        {
            _cape = new Cape(base.X, base.Y, this);
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
            TeamHat t = new TeamHat(base.X, base.Y, Teams.core.extraTeams[idx]);
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
            base.Alpha -= 0.05f;
        }
        if (base.Alpha < 0f)
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
            Level.Add(new HeartPuff(base.X, base.Y)
            {
                anchor = this
            });
            for (int i = 0; i < 2; i++)
            {
                SmallSmoke smallSmoke = SmallSmoke.New(base.X, base.Y);
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
        if (base.duck == null || base.duck.Z != 0f)
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
            Vector2 emitTL = new Vector2((0f - team.metadata.ParticleEmitShapeSize.value.X) / 2f, (0f - team.metadata.ParticleEmitShapeSize.value.Y) / 2f);
            Vector2 emitBR = new Vector2(team.metadata.ParticleEmitShapeSize.value.X / 2f, team.metadata.ParticleEmitShapeSize.value.Y / 2f);
            Vector2 emissionPositionMain = team.metadata.ParticleEmitterOffset.value;
            for (int i = 0; i < numSpawn; i++)
            {
                Vector2 emissionPosition = emissionPositionMain;
                if (team.metadata.ParticleEmitShape.value.X == 1f)
                {
                    float ang = Maths.DegToRad((team.metadata.ParticleEmitShape.value.Y == 2f) ? ((float)i * (360f / (float)numSpawn)) : Rando.Float(360f));
                    Vector2 angVec = new Vector2((float)Math.Cos(ang) * (team.metadata.ParticleEmitShapeSize.value.X / 2f), (float)(0.0 - Math.Sin(ang)) * (team.metadata.ParticleEmitShapeSize.value.Y / 2f));
                    if (team.metadata.ParticleEmitShape.value.Y == 1f)
                    {
                        emissionPosition += angVec * Rando.Float(1f);
                    }
                    else
                    {
                        emissionPosition += angVec;
                    }
                }
                else if (team.metadata.ParticleEmitShape.value.X == 2f)
                {
                    if (team.metadata.ParticleEmitShape.value.Y == 0f)
                    {
                        float sideMul = ((Rando.Float(1f) >= 0.5f) ? 1f : (-1f));
                        if (Rando.Float(1f) >= 0.5f)
                        {
                            emissionPosition += new Vector2(team.metadata.ParticleEmitShapeSize.value.X * sideMul, Rando.Float((0f - team.metadata.ParticleEmitShapeSize.value.Y) / 2f, team.metadata.ParticleEmitShapeSize.value.Y / 2f));
                        }
                        else
                        {
                            emissionPosition += new Vector2(Rando.Float((0f - team.metadata.ParticleEmitShapeSize.value.X) / 2f, team.metadata.ParticleEmitShapeSize.value.X / 2f), team.metadata.ParticleEmitShapeSize.value.Y * sideMul);
                        }
                    }
                    else if (team.metadata.ParticleEmitShape.value.Y == 1f)
                    {
                        emissionPosition += new Vector2(Rando.Float((0f - team.metadata.ParticleEmitShapeSize.value.X) / 2f, team.metadata.ParticleEmitShapeSize.value.X / 2f), Rando.Float((0f - team.metadata.ParticleEmitShapeSize.value.Y) / 2f, team.metadata.ParticleEmitShapeSize.value.Y / 2f));
                    }
                    else if (team.metadata.ParticleEmitShape.value.Y == 2f)
                    {
                        float ang2 = Maths.DegToRad((team.metadata.ParticleEmitShape.value.Y == 2f) ? ((float)i * (360f / (float)numSpawn)) : Rando.Float(360f));
                        Vector2 angVec2 = new Vector2((float)Math.Cos(ang2) * 100f, (float)(0.0 - Math.Sin(ang2)) * 100f);
                        Vector2 col = Vector2.Zero;
                        for (int iSide = 0; iSide < 4; iSide++)
                        {
                            col = Vector2.Zero;
                            switch (iSide)
                            {
                                case 0:
                                    if (Collision.LineIntersect(Vector2.Zero, angVec2, emitTL, new Vector2(emitTL.X, emitBR.Y)))
                                    {
                                        col = Collision.LineIntersectPoint(Vector2.Zero, angVec2, emitTL, new Vector2(emitTL.X, emitBR.Y));
                                    }
                                    break;
                                case 1:
                                    if (Collision.LineIntersect(Vector2.Zero, angVec2, emitTL, new Vector2(emitBR.X, emitTL.Y)))
                                    {
                                        col = Collision.LineIntersectPoint(Vector2.Zero, angVec2, emitTL, new Vector2(emitBR.X, emitTL.Y));
                                    }
                                    break;
                                case 2:
                                    if (Collision.LineIntersect(Vector2.Zero, angVec2, new Vector2(emitTL.X, emitBR.Y), emitBR))
                                    {
                                        col = Collision.LineIntersectPoint(Vector2.Zero, angVec2, new Vector2(emitTL.X, emitBR.Y), emitBR);
                                    }
                                    break;
                                case 3:
                                    if (Collision.LineIntersect(Vector2.Zero, angVec2, new Vector2(emitBR.X, emitTL.Y), emitBR))
                                    {
                                        col = Collision.LineIntersectPoint(Vector2.Zero, angVec2, new Vector2(emitBR.X, emitTL.Y), emitBR);
                                    }
                                    break;
                            }
                            if (col != Vector2.Zero)
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
                Level.Add(new Fluid(base.X + (float)base.duck.offDir * (2f + Rando.Float(0f, 7f)), base.Y + 3f + Rando.Float(0f, 3f), new Vector2((float)base.duck.offDir * Rando.Float(0.5f, 3f), Rando.Float(0f, -2f)), dat, null, 2.5f)
                {
                    Depth = base.Depth + 1
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
                Level.Add(new Fluid(base.X + (float)base.duck.offDir * (2f + Rando.Float(0f, 4f)), base.Y + 3f + Rando.Float(0f, 3f), new Vector2((float)base.duck.offDir * Rando.Float(0.5f, 3f), Rando.Float(0f, -2f)), dat2, null, 5f)
                {
                    Depth = base.Depth + 1
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
                Level.Add(new Fluid(base.X + (float)base.duck.offDir * (6f + Rando.Float(-2f, 4f)), base.Y + Rando.Float(-2f, 4f), new Vector2((float)base.duck.offDir * Rando.Float(1.2f, 4f), Rando.Float(0f, -2.8f)), dat3, null, 5f)
                {
                    Depth = base.Depth + 1
                });
            }
        }
        else if (_sprite.texture.textureName == "hats/tube")
        {
            for (int m = 0; m < 4; m++)
            {
                Level.Add(new TinyBubble(base.X + Rando.Float(-4f, 4f), base.Y + Rando.Float(0f, 4f), Rando.Float(-1.5f, 1.5f), base.Y - 12f, blue: true)
                {
                    Depth = base.Depth + 1
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
                    Level.Add(new Fluid(base.X + (float)base.duck.offDir * (3f + Rando.Float(0f, 6f)), base.Y + 4f + Rando.Float(0f, 1f), new Vector2((float)base.duck.offDir * Rando.Float(-2f, 2f), Rando.Float(-1f, -2f)), dat, null, 2.5f)
                    {
                        Depth = base.Depth + 1
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
        Vector2 poss = _hatOffset;
        if (_team != null)
        {
            if (_team.noCrouchOffset && base.duck != null && base.duck.crouch)
            {
                _hatOffset.Y += 1f;
            }
            if (_team.metadata != null)
            {
                if (_team.metadata.HatNoFlip.value && offDir < 0)
                {
                    _hatOffset.X -= 4f;
                }
                if (base.duck != null && base.duck.sliding)
                {
                    _hatOffset.Y += 1f;
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
                _graphic.Position = Position;
                _graphic.Alpha = base.Alpha;
                _graphic.Angle = Angle;
                _graphic.Depth = base.Depth;
                _graphic.Scale = base.Scale;
                _graphic.Center = Center;
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
                    _specialSprite.Alpha = base.Alpha * 0.7f * (0.5f + _wave.normalized * 0.5f) * _fade;
                    _specialSprite.Scale = base.Scale;
                    _specialSprite.Depth = base.Depth - 10;
                    _specialSprite.Angle += 0.02f;
                    float s = 0.8f + _wave.normalized * 0.2f;
                    _specialSprite.Scale = new Vector2(s, s);
                    Vector2 pos = Offset(new Vector2(2f, 4f));
                    Graphics.Draw(_specialSprite, pos.X, pos.Y);
                }
            }
            else if (_sprite.frame == 1 && _sprite.texture.textureName == "hats/master")
            {
                if (_specialSprite == null)
                {
                    _specialSprite = new Sprite("hats/master_glow");
                    _specialSprite.CenterOrigin();
                }
                _specialSprite.Alpha = Math.Min(glow, 1f);
                _specialSprite.Scale = base.Scale;
                _specialSprite.Depth = base.Depth + 10;
                _specialSprite.Angle = Angle;
                if (offDir < 0)
                {
                    Vector2 pos2 = Offset(new Vector2(1f, 2f));
                    Graphics.Draw(_specialSprite, pos2.X, pos2.Y);
                    pos2 = Offset(new Vector2(5f, 2f));
                    Graphics.Draw(_specialSprite, pos2.X, pos2.Y);
                }
                else
                {
                    Vector2 pos3 = Offset(new Vector2(0f, 2f));
                    Graphics.Draw(_specialSprite, pos3.X, pos3.Y);
                    pos3 = Offset(new Vector2(4f, 2f));
                    Graphics.Draw(_specialSprite, pos3.X, pos3.Y);
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
