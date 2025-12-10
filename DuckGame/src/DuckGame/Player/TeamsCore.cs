using System.Collections.Generic;

namespace DuckGame;

public class TeamsCore
{
    public bool facadesChanged;

    public bool appliedFacades;

    public Dictionary<Profile, Team> _facadeMap = new Dictionary<Profile, Team>();

    public List<Team> teams;

    public Team nullTeam = new Team("???", "hats/cluehat");

    public List<Team> extraTeams = new List<Team>();

    public SpriteMap hats;

    public List<Team> SpectatorTeams = new List<Team>();

    private List<Team> _folders = new List<Team>();

    public Team Player1 => teams[0];

    public Team Player2 => teams[1];

    public Team Player3 => teams[2];

    public Team Player4 => teams[3];

    public Team Player5 => teams[4];

    public Team Player6 => teams[5];

    public Team Player7 => teams[6];

    public Team Player8 => teams[7];

    public int numTeams => teams.Count;

    public List<Team> all
    {
        get
        {
            List<Team> newTeams = new List<Team>(teams);
            if (!Network.isActive)
            {
                newTeams.AddRange(extraTeams);
            }
            else
            {
                foreach (Profile p in DuckNetwork.profiles)
                {
                    if (p == DuckNetwork.localProfile)
                    {
                        newTeams.AddRange(extraTeams);
                    }
                    else
                    {
                        newTeams.AddRange(p.customTeams);
                    }
                }
            }
            return newTeams;
        }
    }

    public List<Team> folders => _folders;

    public List<Team> allStock => new List<Team>(teams);

    public void Initialize()
    {
        hats = new SpriteMap("hatCollection", 32, 32);
        hats.center = new Vec2(16f, 16f);
        teams = new List<Team>
        {
            new Team("Player 1", "hats/noHat", demo: true)
            {
                defaultTeam = true
            },
            new Team("Player 2", "hats/noHat", demo: true)
            {
                defaultTeam = true
            },
            new Team("Player 3", "hats/noHat", demo: true)
            {
                defaultTeam = true
            },
            new Team("Player 4", "hats/noHat", demo: true)
            {
                defaultTeam = true
            },
            new Team("Player 5", "hats/noHat", demo: true)
            {
                defaultTeam = true
            },
            new Team("Player 6", "hats/noHat", demo: true)
            {
                defaultTeam = true
            },
            new Team("Player 7", "hats/noHat", demo: true)
            {
                defaultTeam = true
            },
            new Team("Player 8", "hats/noHat", demo: true)
            {
                defaultTeam = true
            },
            new Team("Sombreros", "hats/sombrero", demo: true),
            new Team("Dappers", "hats/dapper", demo: true),
            new Team("Dicks", "hats/dicks", demo: true),
            new Team(varHair: true, "Frank", "hats/frank", demo: false, lockd: true),
            new Team("DUCKS", "hats/reallife", demo: false, lockd: true),
            new Team("Frogs?", "hats/frogs", demo: true),
            new Team("Drunks", "hats/drunks"),
            new Team("Joey", "hats/joey", demo: false, lockd: true),
            new Team("BALLZ", "hats/ballhead"),
            new Team("Agents", "hats/agents"),
            new Team("Sailors", "hats/sailors"),
            new Team("astropal", "hats/astrobud", demo: false, lockd: true),
            new Team("Cowboys", "hats/cowboys", demo: false, lockd: true),
            new Team(varHair: true, "Pulpy", "hats/pulpy", demo: false, lockd: true),
            new Team("SKULLY", "hats/skelly", demo: false, lockd: true),
            new Team("Hearts", "hats/hearts"),
            new Team("LOCKED", "hats/locked"),
            new Team("Jazzducks", "hats/jazzducks", demo: false, lockd: false, new Vec2(-2f, -7f)),
            new Team("Divers", "hats/divers"),
            new Team("Uglies", "hats/uglies"),
            new Team("Dinos", "hats/dinos"),
            new Team("Caps", "hats/caps"),
            new Team("Burgers", "hats/burgers"),
            new Team("Turing", "hats/turing", demo: true),
            new Team("Retro", "hats/retros"),
            new Team("Senpai", "hats/sensei"),
            new Team("BAWB", "hats/bawb", demo: false, lockd: true, new Vec2(-1f, -10f))
            {
                noCrouchOffset = true
            },
            new Team("SWACK", "hats/guac", demo: true, lockd: true),
            new Team("eggpal", "hats/eggy", demo: false, lockd: true),
            new Team("Valet", "hats/valet"),
            new Team("Pilots", "hats/pilots"),
            new Team("Cyborgs", "hats/cyborgs"),
            new Team("Tubes", "hats/tube", demo: false, lockd: false, new Vec2(-1f, 0f)),
            new Team("Gents", "hats/gents"),
            new Team("Potheads", "hats/pots"),
            new Team("Skis", "hats/ski"),
            new Team("Fridges", "hats/fridge"),
            new Team("Witchtime", "hats/witchtime"),
            new Team("Wizards", "hats/wizbiz"),
            new Team("FUNNYMAN", "hats/FunnyMan"),
            new Team("Pumpkins", "hats/Dumplin"),
            new Team("CAPTAIN", "hats/devhat", demo: false, lockd: true, default(Vec2), "", Content.Load<Tex2D>("hats/devCape")),
            new Team("BRICK", "hats/brick", demo: false, lockd: true),
            new Team(varHair: true, "Pompadour", "hats/pompadour"),
            new Team(varHair: true, "Super", "hats/super"),
            new Team("Chancy", "hats/chancy", demo: false, lockd: true),
            new Team("Log", "hats/log"),
            new Team("Meeee", "hats/toomany", demo: false, lockd: true),
            new Team("BRODUCK", "hats/broduck", demo: false, lockd: true),
            new Team("brad", "hats/handy", demo: false, lockd: true),
            new Team("eyebob", "hats/gross"),
            new Team("masters", "hats/master"),
            new Team("clams", "hats/clams"),
            new Team("waffles", "hats/waffles", demo: false, lockd: false, default(Vec2), "", Content.Load<Tex2D>("hats/waffleCape")),
            new Team("HIGHFIVES", "hats/highfives", demo: false, lockd: true, default(Vec2), "Right on!!"),
            new Team("toeboys", "hats/toeboys", demo: false, lockd: false, new Vec2(-1f, -2f)),
            new Team("bigearls", "hats/bigearls", demo: false, lockd: false, new Vec2(0f, 1f)),
            new Team("zeros", "hats/katanaman", demo: false, lockd: false, default(Vec2)),
            new Team(varHair: true, "CYCLOPS", "hats/cyclops", demo: false, lockd: true, default(Vec2), "These wounds they will not heal."),
            new Team("MOTHERS", "hats/motherduck", demo: false, lockd: true, default(Vec2), "Not a goose."),
            new Team("BIG ROBO", "hats/newrobo", demo: false, lockd: true, default(Vec2)),
            new Team("TINCAN", "hats/oldrobo", demo: false, lockd: true, default(Vec2)),
            new Team("WELDERS", "hats/WELDER", demo: false, lockd: true, default(Vec2), "Safety has never looked so cool."),
            new Team("PONYCAP", "hats/ponycap", demo: false, lockd: true, default(Vec2)),
            new Team("TRICORNE", "hats/tricorne", demo: false, lockd: true, default(Vec2), "We fight for freedom!"),
            new Team(varHair: true, "TWINTAIL", "hats/twintail", demo: false, lockd: true, default(Vec2), "Two tails are better than one."),
            new Team("MAJESTY", "hats/royalty", demo: false, lockd: true, default(Vec2), "", Content.Load<Tex2D>("hats/royalCape")),
            new Team("MOONWALK", "hats/moonwalker", demo: false, lockd: true, default(Vec2), "", Content.Load<Tex2D>("hats/moonCape")),
            new Team("kerchiefs", "hats/kerchief", demo: false, lockd: false, new Vec2(0f, -1f)),
            new Team("postals", "hats/mailbox"),
            new Team("wahhs", "hats/wahhs"),
            new Team("uufos", "hats/ufos"),
            new Team(varHair: true, "B52s", "hats/b52s"),
            new Team("diplomats", "hats/suit"),
            new Team("johnnygrey", "hats/johnnys"),
            new Team("wolfy", "hats/werewolves")
        };
    }
}
