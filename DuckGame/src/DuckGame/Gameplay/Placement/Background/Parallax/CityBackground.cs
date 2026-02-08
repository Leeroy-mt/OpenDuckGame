using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace DuckGame;

[EditorGroup("Background|Parallax")]
[BaggedProperty("previewPriority", true)]
public class CityBackground : BackgroundUpdater
{
    private class Plane : SpriteMap
    {
        private FancyBitmapFont _font;

        private RenderTarget2D bannerTarget;

        private MaterialWiggle _wiggle;

        private float originalY;

        private string _text = "";

        private bool _flyLeft;

        public new bool finished;

        public float textWidth => (bannerTarget == null) ? 1 : bannerTarget.width;

        public Plane(Vector2 pos, string text, bool flyLeft)
            : base("plane", 18, 13)
        {
            _font = new FancyBitmapFont("smallFont");
            _flyLeft = flyLeft;
            _text = text;
            originalY = pos.Y;
            Position = pos;
            AddAnimation("idle", 0.8f, true, 0, 1);
            SetAnimation("idle");
            float wide = _font.GetWidth(text);
            bannerTarget = new RenderTarget2D((int)(wide + 4f) + 8, 15);
            _wiggle = new MaterialWiggle(this);
            Camera cam = new Camera(0f, 0f, bannerTarget.width, bannerTarget.height)
            {
                position = Vector2.Zero
            };
            Graphics.SetRenderTarget(bannerTarget);
            DepthStencilState state = new DepthStencilState
            {
                StencilEnable = true,
                StencilFunction = CompareFunction.Always,
                StencilPass = StencilOperation.Replace,
                ReferenceStencil = 1,
                DepthBufferEnable = false
            };
            Graphics.Clear(Color.Transparent);
            Graphics.screen.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp, state, RasterizerState.CullNone, null, cam.getMatrix());
            Graphics.DrawRect(new Vector2(0f, 2f), new Vector2(bannerTarget.width - 8, bannerTarget.height - 2), Color.Black);
            _font.Draw(text, new Vector2(1f, 3f), new Color(47, 0, 66), 1f);
            Graphics.screen.End();
            Graphics.SetRenderTarget(null);
        }

        public void UpdateFlying()
        {
            X += (_flyLeft ? (-0.25f) : 0.25f);
            if (bannerTarget != null && ((_flyLeft && base.X < (float)(-(400 + bannerTarget.width))) || (!_flyLeft && base.X > (float)(400 + bannerTarget.width))))
            {
                finished = true;
            }
            _ = (0f - (Level.current.bottomRight.Y + Level.current.topLeft.Y)) / 2f;
        }

        public override void Draw()
        {
            if (bannerTarget != null)
            {
                base.Scale = new Vector2(0.5f, 0.5f);
                if (_flyLeft)
                {
                    base.flipH = true;
                    base.Draw();
                    Graphics.material = _wiggle;
                    Graphics.Draw(bannerTarget, base.X + 4f, base.Y, 0.5f, 0.5f, 1f);
                    Graphics.material = null;
                }
                else
                {
                    base.Draw();
                    Graphics.material = _wiggle;
                    Graphics.Draw(bannerTarget, base.X - (float)(bannerTarget.width / 2 + 4), base.Y, 0.5f, 0.5f, 1f);
                    Graphics.material = null;
                }
            }
        }
    }

    private Vector2 backgroundPlanePos;

    private List<Plane> _planes = new List<Plane>();

    private float timeSinceSkySay;

    public CityBackground(float xpos, float ypos)
        : base(xpos, ypos)
    {
        graphic = new SpriteMap("backgroundIcons", 16, 16)
        {
            frame = 5
        };
        Center = new Vector2(8f, 8f);
        _collisionSize = new Vector2(16f, 16f);
        _collisionOffset = new Vector2(-8f, -8f);
        base.Depth = 0.9f;
        base.layer = Layer.Foreground;
        _visibleInGame = false;
        _editorName = "City BG";
    }

    public override void Initialize()
    {
        if (!(Level.current is Editor))
        {
            backgroundColor = new Color(24, 0, 31);
            Level.current.backgroundColor = backgroundColor;
            _parallax = new ParallaxBackground("background/city", 0f, 0f, 3);
            float speed = 0.4f;
            _parallax.AddZone(0, 0f, 0f - speed, moving: true);
            _parallax.AddZone(1, 0f, 0f - speed, moving: true);
            _parallax.AddZone(2, 0f, 0f - speed, moving: true);
            _parallax.AddZone(3, 0.2f, 0f - speed, moving: true);
            _parallax.AddZone(4, 0.2f, 0f - speed, moving: true);
            _parallax.AddZone(5, 0.4f, 0f - speed, moving: true);
            _parallax.AddZone(6, 0.6f, 0f - speed, moving: true);
            float cityDistance = 0.8f;
            _parallax.AddZone(14, cityDistance, speed);
            _parallax.AddZone(15, cityDistance, speed);
            _parallax.AddZone(16, cityDistance, speed);
            _parallax.AddZone(17, cityDistance, speed);
            _parallax.AddZone(18, cityDistance, speed);
            _parallax.AddZone(19, cityDistance, speed);
            _parallax.AddZone(20, cityDistance, speed);
            _parallax.AddZone(21, cityDistance, speed);
            _parallax.AddZone(22, cityDistance, speed);
            _parallax.AddZone(23, cityDistance, speed);
            _parallax.AddZone(24, cityDistance, speed);
            _parallax.AddZone(25, cityDistance, speed);
            _parallax.AddZone(26, cityDistance, speed);
            _parallax.AddZone(27, cityDistance, speed);
            _parallax.AddZone(28, cityDistance, speed);
            _parallax.AddZone(29, cityDistance, speed);
            base.layer = Layer.Parallax;
            Level.Add(_parallax);
            if (Rando.Int(200) == 0)
            {
                RandomSkySay();
            }
            _visibleInGame = true;
            visible = true;
        }
    }

    public void SkySay(string text, Vector2 spawn = default(Vector2), bool pFlyLeft = false)
    {
        List<string> parts = new List<string>();
        string part = "";
        for (int i = 0; i < text.Length; i++)
        {
            if (text[i] == ' ' && part.Length > 150)
            {
                parts.Add(part);
                part = "";
            }
            else
            {
                part += text[i];
            }
        }
        if (!string.IsNullOrWhiteSpace(part))
        {
            parts.Add(part);
        }
        if (parts.Count <= 0)
        {
            return;
        }
        bool flyLeft = false;
        if (Rando.Float(1f) > 0.5f || parts.Count > 1 || parts[0].Length > 80)
        {
            flyLeft = true;
        }
        Vector2 offset = new Vector2(flyLeft ? 350 : (-50), 60f + Rando.Float(80f));
        if (spawn != Vector2.Zero)
        {
            offset = spawn;
            flyLeft = pFlyLeft;
        }
        else if (Network.isActive && Network.isServer)
        {
            Send.Message(new NMSkySay(text, offset, flyLeft));
        }
        foreach (string s in parts)
        {
            Plane p = new Plane(offset, s, flyLeft);
            if (flyLeft)
            {
                offset.X += p.textWidth * 0.5f + 80f;
            }
            else
            {
                offset.X -= p.textWidth * 0.5f + 80f;
            }
            _planes.Add(p);
        }
    }

    public void RandomSkySay()
    {
        string text = "DUCK GAME";
        if (DateTime.Now.Day == 25 && DateTime.Now.Month == 12)
        {
            text = "HAPPY 25TH!";
        }
        else if (DateTime.Now.Day == 1 && DateTime.Now.Month == 1)
        {
            text = "HAPPY NEW YEARS";
        }
        else if (Teams.active.Count > 0 && Rando.Int(10) == 1 && !(Level.current is ChallengeLevel))
        {
            int pick = Rando.Int(Profiles.active.Count - 1);
            text = "GO " + Profiles.active[pick].team.name.ToUpperInvariant();
        }
        else if (Teams.active.Count > 0 && Rando.Int(500) == 1 && !(Level.current is ChallengeLevel))
        {
            int pick2 = Rando.Int(Profiles.active.Count - 1);
            text = Profiles.active[pick2].team.name.ToUpperInvariant() + " ARE GONNA WIN!";
        }
        else if (Rando.Int(100) == 1)
        {
            text = "SHOP AT VINCENT'S";
        }
        else if (Rando.Int(150) == 1)
        {
            text = "HEY KIDS!";
        }
        else if (Rando.Int(150) == 1)
        {
            text = "MOM, HELLO";
        }
        else if (Rando.Int(15000) == 1)
        {
            text = "I FOUND YOUR WALLET JERRY";
        }
        else if (Rando.Int(50) == 1)
        {
            text = "CORPTRON GAMES";
        }
        else if (Rando.Int(1500) == 1)
        {
            text = "ARMATURE STUDIOS";
        }
        else if (Rando.Int(5000) == 1)
        {
            text = "ADULT SWIM GAMES";
        }
        else if (Rando.Int(7000) == 1)
        {
            text = "chancy owns the sky lol";
        }
        else if (Rando.Int(5000) == 1)
        {
            text = "WHERE IS JOHN MALLARD";
        }
        else if (Rando.Int(1000) == 1)
        {
            text = "I SEE YOU";
        }
        else if (Rando.Int(200) == 1)
        {
            text = "LET'S DANCE";
        }
        else if (Global.data.timesSpawned > 300 && Rando.Int(200) == 1)
        {
            text = ":)";
        }
        else if (Global.data.timesSpawned > 500 && Rando.Int(10000) == 1)
        {
            text = "MAY ANGLES LEAD YOU IN";
        }
        else if (Rando.Int(200) == 1 && DateTime.Now.DayOfWeek == DayOfWeek.Tuesday)
        {
            text = "HAPPY TUESDAY!";
        }
        else if (Global.data.timesSpawned > 100 && Rando.Int(50000) == 1)
        {
            text = "HEY DON'T WRITE YOURSELF OFF YET IT'S ONLY IN YOUR HEAD TO FEEL LEFT OUT OR LOOKED DOWN ON JUST TRY YOUR BEST TRY EVERYTHING YOU CAN AND DON'T YOU WORRY WHAT THEY TELL THEMSELVES WHEN YOUR AWAY IT JUST TAKES SOME TIME LITTLE GIRL IN THE MIDDLE OF THE RIDE EVERYTHING EVERYTHING'L BE JUST FINE EVERYTHING EVERYTHING'L BE ALRIGHT ALRIGHT RAY YOU KNOW THEY'RE ALL THE SAME YOU KNOW YOU'RE DOING BETTER ON YOUR OWN SO DON'T BUY IN LIVE RIGHT NOW YEAH JUST BE YOURSELF IT DOESN'T MATTER IF IT'S GOOD ENOUGH FOR SOMEONE ELSE IT JUST TAKES SOME TIME LITTLE GIRL IN THE MIDDLE OF THE RIDE EVERYTHING EVERYTHING'L BE JUST FINE EVERYTHING EVERYTHING'L BE ALRIGHT ALRIGHT HEY DON'T WRITE YOURSELF OFF YET IT'S ONLY IN YOUR HEAD TO FEEL LEFT OUT OR LOOKED DOWN ON JUST DO YOUR BEST DO EVERYTHING YOU CAN AND DON'T YOU WORRY WHAT THEIR BITTER HEARTS ARE GONNA SAY IT JUST TAKES SOME TIME LITTLE GIRL IN THE MIDDLE OF THE RIDE EVERYTHING EVERYTHING'L BE JUST FINE EVERYTHING EVERYTHING'L BE ALRIGHT ALRIGHT IT JUST TAKES SOME TIME LITTLE GIRL IN THE MIDDLE OF THE RIDE EVERYTHING EVERYTHING'L BE JUST FINE EVERYTHING EVERYTHING'L BE ALRIGHT ALRIGHT";
        }
        else if (Global.data.timesSpawned > 1000 && Rando.Int(1000000) == 1)
        {
            text = "FUNNY STORY ONE TIME I WAS JUST CHILLING AT ZELLERS WHEN I NOTICED THE OFF BRAND CHIPS WHERE ON SALE 2 FOR ONE SO I BOUGHT SOME. WHEN I GOT HOME I OPENED THE SALT AND VINEGAR ONES AND THERE WHERE ALL DRESSED CHIPS INSIDE SO I WENT BACK TO ZELLERS AND THE CEO WAS THERE AND I TOLD HIM DIRECTLY. HE CRIED PROFUSELY BEFORE SAYING THAT NEVER SHOULD HAVE HAPPENED, THEN DECLARING THAT HIS STORE WAS A MISTAKE AND THEN HE CLOSED ALL OF THE ZELLERS. I WISH I NEVER BROUGHT MY CHIPS BACK TO ZELLERS. I MISS ZELLERS.";
        }
        else if (Global.data.timesSpawned > 10000 && Rando.Int(1000) == 1)
        {
            text = "WOW YOU PLAY DUCK GAME A LOT!";
        }
        else if (Global.data.timesSpawned < 100 && Rando.Int(100) == 1)
        {
            text = "WELCOME TO DUCK GAME!";
        }
        if (DateTime.Now.Day == 1 && Rando.Int(1500) == 1)
        {
            text = "HAPPY BIRTHDAY!";
        }
        if (Rando.Int(200) == 1 && Network.isActive && DuckNetwork.core.chatMessages.Count > 0)
        {
            text = DuckNetwork.core.chatMessages[Rando.Int(DuckNetwork.core.chatMessages.Count - 1)].text;
        }
        SkySay(text);
    }

    public override void Update()
    {
        foreach (Plane plane in _planes)
        {
            plane.UpdateFlying();
        }
        _planes.RemoveAll((Plane x) => x.finished);
        if (!Network.isActive || Network.isServer)
        {
            timeSinceSkySay += Maths.IncFrameTimer();
            if (timeSinceSkySay > 30f && _planes.Count == 0)
            {
                if (Rando.Float(1f) > 0.5f)
                {
                    RandomSkySay();
                }
                timeSinceSkySay = Rando.Float(15f);
            }
        }
        base.Update();
    }

    public override void Draw()
    {
        if (Level.current is Editor)
        {
            base.Draw();
        }
        foreach (Plane plane in _planes)
        {
            plane.Depth = 1f;
            plane.Draw();
        }
    }

    public override void Terminate()
    {
        Level.Remove(_parallax);
    }
}
