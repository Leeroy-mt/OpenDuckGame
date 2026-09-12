using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
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

    public void Initialize(IProgress<float> progress = null)
    {
        hats = new SpriteMap("hatCollection", 32, 32)
        {
            Center = new Vector2(16, 16)
        };
        const int TeamsTotalCount = 84;
        teams = [];

        teams.Add(new Team("Player 1", "hats/noHat", demo: true) { defaultTeam = true });
        progress?.Report(teams.Count / (float)TeamsTotalCount);

        teams.Add(new Team("Player 2", "hats/noHat", demo: true) { defaultTeam = true });
        progress?.Report(teams.Count / (float)TeamsTotalCount);

        teams.Add(new Team("Player 3", "hats/noHat", demo: true) { defaultTeam = true });
        progress?.Report(teams.Count / (float)TeamsTotalCount);

        teams.Add(new Team("Player 4", "hats/noHat", demo: true) { defaultTeam = true });
        progress?.Report(teams.Count / (float)TeamsTotalCount);

        teams.Add(new Team("Player 5", "hats/noHat", demo: true) { defaultTeam = true });
        progress?.Report(teams.Count / (float)TeamsTotalCount);

        teams.Add(new Team("Player 6", "hats/noHat", demo: true) { defaultTeam = true });
        progress?.Report(teams.Count / (float)TeamsTotalCount);

        teams.Add(new Team("Player 7", "hats/noHat", demo: true) { defaultTeam = true });
        progress?.Report(teams.Count / (float)TeamsTotalCount);

        teams.Add(new Team("Player 8", "hats/noHat", demo: true) { defaultTeam = true });
        progress?.Report(teams.Count / (float)TeamsTotalCount);

        teams.Add(new Team("Sombreros", "hats/sombrero", demo: true));
        progress?.Report(teams.Count / (float)TeamsTotalCount);

        teams.Add(new Team("Dappers", "hats/dapper", demo: true));
        progress?.Report(teams.Count / (float)TeamsTotalCount);

        teams.Add(new Team("Dicks", "hats/dicks", demo: true));
        progress?.Report(teams.Count / (float)TeamsTotalCount);

        teams.Add(new Team(varHair: true, "Frank", "hats/frank", demo: false, lockd: true));
        progress?.Report(teams.Count / (float)TeamsTotalCount);

        teams.Add(new Team("DUCKS", "hats/reallife", demo: false, lockd: true));
        progress?.Report(teams.Count / (float)TeamsTotalCount);

        teams.Add(new Team("Frogs?", "hats/frogs", demo: true));
        progress?.Report(teams.Count / (float)TeamsTotalCount);

        teams.Add(new Team("Drunks", "hats/drunks"));
        progress?.Report(teams.Count / (float)TeamsTotalCount);

        teams.Add(new Team("Joey", "hats/joey", demo: false, lockd: true));
        progress?.Report(teams.Count / (float)TeamsTotalCount);

        teams.Add(new Team("BALLZ", "hats/ballhead"));
        progress?.Report(teams.Count / (float)TeamsTotalCount);

        teams.Add(new Team("Agents", "hats/agents"));
        progress?.Report(teams.Count / (float)TeamsTotalCount);

        teams.Add(new Team("Sailors", "hats/sailors"));
        progress?.Report(teams.Count / (float)TeamsTotalCount);

        teams.Add(new Team("astropal", "hats/astrobud", demo: false, lockd: true));
        progress?.Report(teams.Count / (float)TeamsTotalCount);

        teams.Add(new Team("Cowboys", "hats/cowboys", demo: false, lockd: true));
        progress?.Report(teams.Count / (float)TeamsTotalCount);
        
        teams.Add(new Team(varHair: true, "Pulpy", "hats/pulpy", demo: false, lockd: true));
        progress?.Report(teams.Count / (float)TeamsTotalCount);
        
        teams.Add(new Team("SKULLY", "hats/skelly", demo: false, lockd: true));
        progress?.Report(teams.Count / (float)TeamsTotalCount);
        
        teams.Add(new Team("Hearts", "hats/hearts"));
        progress?.Report(teams.Count / (float)TeamsTotalCount);
        
        teams.Add(new Team("LOCKED", "hats/locked"));
        progress?.Report(teams.Count / (float)TeamsTotalCount);
        
        teams.Add(new Team("Jazzducks", "hats/jazzducks", demo: false, lockd: false, new Vector2(-2f, -7f)));
        progress?.Report(teams.Count / (float)TeamsTotalCount);
        
        teams.Add(new Team("Divers", "hats/divers"));
        progress?.Report(teams.Count / (float)TeamsTotalCount);
        
        teams.Add(new Team("Uglies", "hats/uglies"));
        progress?.Report(teams.Count / (float)TeamsTotalCount);
        
        teams.Add(new Team("Dinos", "hats/dinos"));
        progress?.Report(teams.Count / (float)TeamsTotalCount);
        
        teams.Add(new Team("Caps", "hats/caps"));
        progress?.Report(teams.Count / (float)TeamsTotalCount);
        
        teams.Add(new Team("Burgers", "hats/burgers"));
        progress?.Report(teams.Count / (float)TeamsTotalCount);
        
        teams.Add(new Team("Turing", "hats/turing", demo: true));
        progress?.Report(teams.Count / (float)TeamsTotalCount);
        
        teams.Add(new Team("Retro", "hats/retros"));
        progress?.Report(teams.Count / (float)TeamsTotalCount);
        
        teams.Add(new Team("Senpai", "hats/sensei"));
        progress?.Report(teams.Count / (float)TeamsTotalCount);
        
        teams.Add(new Team("BAWB", "hats/bawb", demo: false, lockd: true, new Vector2(-1, -10)) { noCrouchOffset = true });
        progress?.Report(teams.Count / (float)TeamsTotalCount);
        
        teams.Add(new Team("SWACK", "hats/guac", demo: true, lockd: true));
        progress?.Report(teams.Count / (float)TeamsTotalCount);
        
        teams.Add(new Team("eggpal", "hats/eggy", demo: false, lockd: true));
        progress?.Report(teams.Count / (float)TeamsTotalCount);
        
        teams.Add(new Team("Valet", "hats/valet"));
        progress?.Report(teams.Count / (float)TeamsTotalCount);
        
        teams.Add(new Team("Pilots", "hats/pilots"));
        progress?.Report(teams.Count / (float)TeamsTotalCount);
        
        teams.Add(new Team("Cyborgs", "hats/cyborgs"));
        progress?.Report(teams.Count / (float)TeamsTotalCount);
        
        teams.Add(new Team("Tubes", "hats/tube", demo: false, lockd: false, new Vector2(-1f, 0f)));
        progress?.Report(teams.Count / (float)TeamsTotalCount);
        
        teams.Add(new Team("Gents", "hats/gents"));
        progress?.Report(teams.Count / (float)TeamsTotalCount);
        
        teams.Add(new Team("Potheads", "hats/pots"));
        progress?.Report(teams.Count / (float)TeamsTotalCount);
        
        teams.Add(new Team("Skis", "hats/ski"));
        progress?.Report(teams.Count / (float)TeamsTotalCount);
        
        teams.Add(new Team("Fridges", "hats/fridge"));
        progress?.Report(teams.Count / (float)TeamsTotalCount);
        
        teams.Add(new Team("Witchtime", "hats/witchtime"));
        progress?.Report(teams.Count / (float)TeamsTotalCount);
        
        teams.Add(new Team("Wizards", "hats/wizbiz"));
        progress?.Report(teams.Count / (float)TeamsTotalCount);
        
        teams.Add(new Team("FUNNYMAN", "hats/FunnyMan"));
        progress?.Report(teams.Count / (float)TeamsTotalCount);
        
        teams.Add(new Team("Pumpkins", "hats/Dumplin"));
        progress?.Report(teams.Count / (float)TeamsTotalCount);
        
        teams.Add(new Team("CAPTAIN", "hats/devhat", demo: false, lockd: true, default, "", Content.Load<Texture2D>("hats/devCape")));
        progress?.Report(teams.Count / (float)TeamsTotalCount);
        
        teams.Add(new Team("BRICK", "hats/brick", demo: false, lockd: true));
        progress?.Report(teams.Count / (float)TeamsTotalCount);
        
        teams.Add(new Team(varHair: true, "Pompadour", "hats/pompadour"));
        progress?.Report(teams.Count / (float)TeamsTotalCount);
        
        teams.Add(new Team(varHair: true, "Super", "hats/super"));
        progress?.Report(teams.Count / (float)TeamsTotalCount);
        
        teams.Add(new Team("Chancy", "hats/chancy", demo: false, lockd: true));
        progress?.Report(teams.Count / (float)TeamsTotalCount);
        
        teams.Add(new Team("Log", "hats/log"));
        progress?.Report(teams.Count / (float)TeamsTotalCount);
        
        teams.Add(new Team("Meeee", "hats/toomany", demo: false, lockd: true));
        progress?.Report(teams.Count / (float)TeamsTotalCount);
        
        teams.Add(new Team("BRODUCK", "hats/broduck", demo: false, lockd: true));
        progress?.Report(teams.Count / (float)TeamsTotalCount);
        
        teams.Add(new Team("brad", "hats/handy", demo: false, lockd: true));
        progress?.Report(teams.Count / (float)TeamsTotalCount);
        
        teams.Add(new Team("eyebob", "hats/gross"));
        progress?.Report(teams.Count / (float)TeamsTotalCount);
        
        teams.Add(new Team("masters", "hats/master"));
        progress?.Report(teams.Count / (float)TeamsTotalCount);
        
        teams.Add(new Team("clams", "hats/clams"));
        progress?.Report(teams.Count / (float)TeamsTotalCount);
        
        teams.Add(new Team("waffles", "hats/waffles", demo: false, lockd: false, default, "", Content.Load<Texture2D>("hats/waffleCape")));
        progress?.Report(teams.Count / (float)TeamsTotalCount);
        
        teams.Add(new Team("HIGHFIVES", "hats/highfives", demo: false, lockd: true, default, "Right on!!"));
        progress?.Report(teams.Count / (float)TeamsTotalCount);
        
        teams.Add(new Team("toeboys", "hats/toeboys", demo: false, lockd: false, new Vector2(-1, -2)));
        progress?.Report(teams.Count / (float)TeamsTotalCount);
        
        teams.Add(new Team("bigearls", "hats/bigearls", demo: false, lockd: false, new Vector2(0, 1)));
        progress?.Report(teams.Count / (float)TeamsTotalCount);
        
        teams.Add(new Team("zeros", "hats/katanaman", demo: false, lockd: false, default(Vector2)));
        progress?.Report(teams.Count / (float)TeamsTotalCount);
        
        teams.Add(new Team(varHair: true, "CYCLOPS", "hats/cyclops", demo: false, lockd: true, default(Vector2), "These wounds they will not heal."));
        progress?.Report(teams.Count / (float)TeamsTotalCount);
        
        teams.Add(new Team("MOTHERS", "hats/motherduck", demo: false, lockd: true, default(Vector2), "Not a goose."));
        progress?.Report(teams.Count / (float)TeamsTotalCount);
        
        teams.Add(new Team("BIG ROBO", "hats/newrobo", demo: false, lockd: true, default(Vector2)));
        progress?.Report(teams.Count / (float)TeamsTotalCount);
        
        teams.Add(new Team("TINCAN", "hats/oldrobo", demo: false, lockd: true, default(Vector2)));
        progress?.Report(teams.Count / (float)TeamsTotalCount);
        
        teams.Add(new Team("WELDERS", "hats/WELDER", demo: false, lockd: true, default(Vector2), "Safety has never looked so cool."));
        progress?.Report(teams.Count / (float)TeamsTotalCount);
        
        teams.Add(new Team("PONYCAP", "hats/ponycap", demo: false, lockd: true, default(Vector2)));
        progress?.Report(teams.Count / (float)TeamsTotalCount);
        
        teams.Add(new Team("TRICORNE", "hats/tricorne", demo: false, lockd: true, default(Vector2), "We fight for freedom!"));
        progress?.Report(teams.Count / (float)TeamsTotalCount);
        
        teams.Add(new Team(varHair: true, "TWINTAIL", "hats/twintail", demo: false, lockd: true, default(Vector2), "Two tails are better than one."));
        progress?.Report(teams.Count / (float)TeamsTotalCount);
        
        teams.Add(new Team("MAJESTY", "hats/royalty", demo: false, lockd: true, default(Vector2), "", Content.Load<Texture2D>("hats/royalCape")));
        progress?.Report(teams.Count / (float)TeamsTotalCount);
        
        teams.Add(new Team("MOONWALK", "hats/moonwalker", demo: false, lockd: true, default(Vector2), "", Content.Load<Texture2D>("hats/moonCape")));
        progress?.Report(teams.Count / (float)TeamsTotalCount);
        
        teams.Add(new Team("kerchiefs", "hats/kerchief", demo: false, lockd: false, new Vector2(0f, -1f)));
        progress?.Report(teams.Count / (float)TeamsTotalCount);
        
        teams.Add(new Team("postals", "hats/mailbox"));
        progress?.Report(teams.Count / (float)TeamsTotalCount);
        
        teams.Add(new Team("wahhs", "hats/wahhs"));
        progress?.Report(teams.Count / (float)TeamsTotalCount);
        
        teams.Add(new Team("uufos", "hats/ufos"));
        progress?.Report(teams.Count / (float)TeamsTotalCount);
        
        teams.Add(new Team(varHair: true, "B52s", "hats/b52s"));
        progress?.Report(teams.Count / (float)TeamsTotalCount);
        
        teams.Add(new Team("diplomats", "hats/suit"));
        progress?.Report(teams.Count / (float)TeamsTotalCount);
        
        teams.Add(new Team("johnnygrey", "hats/johnnys"));
        progress?.Report(teams.Count / (float)TeamsTotalCount);

        teams.Add(new Team("wolfy", "hats/werewolves"));
        progress?.Report(teams.Count / (float)TeamsTotalCount);
    }
}
