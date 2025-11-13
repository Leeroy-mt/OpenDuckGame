namespace DuckGame;

public class DuckGameEditor : Editor
{
	public override void RunTestLevel(string name)
	{
		LevGenType genType = LevGenType.Any;
		if (Editor._currentLevelData.proceduralData.enableSingle && !Editor._currentLevelData.proceduralData.enableMulti)
		{
			genType = LevGenType.SinglePlayer;
		}
		else if (!Editor._currentLevelData.proceduralData.enableSingle && Editor._currentLevelData.proceduralData.enableMulti)
		{
			genType = LevGenType.Deathmatch;
		}
		if (base._levelThings.Exists((Thing x) => x is ChallengeMode))
		{
			foreach (Profile pro in Profiles.active)
			{
				if (pro.team != null)
				{
					pro.team.Leave(pro);
				}
			}
			Profiles.experienceProfile.team = Teams.Player1;
			Level.current = new ChallengeLevel(name);
		}
		else if (base._levelThings.Exists((Thing x) => x is ImportMachine))
		{
			Level.current = new ArcadeLevel(DuckFile.contentDirectory + "Levels/arcade_machine_preview.lev")
			{
				genType = LevGenType.CustomArcadeMachine,
				customMachine = name,
				editor = this
			};
		}
		else if (base._levelThings.Exists((Thing x) => x is ArcadeMode))
		{
			foreach (Profile pro2 in Profiles.active)
			{
				if (pro2.team != null)
				{
					pro2.team.Leave(pro2);
				}
			}
			Profiles.experienceProfile.team = Teams.Player1;
			Level.current = new ArcadeLevel(name)
			{
				editor = this
			};
		}
		else
		{
			foreach (Profile pro3 in Profiles.active)
			{
				if (pro3.team != null)
				{
					pro3.team.Leave(pro3);
				}
			}
			Profiles.experienceProfile.team = Teams.Player1;
			Profiles.DefaultPlayer2.team = Teams.Player2;
			Profiles.DefaultPlayer3.team = Teams.Player3;
			Profiles.DefaultPlayer4.team = Teams.Player4;
			Profiles.DefaultPlayer5.team = Teams.Player5;
			Profiles.DefaultPlayer6.team = Teams.Player6;
			Profiles.DefaultPlayer7.team = Teams.Player7;
			Profiles.DefaultPlayer8.team = Teams.Player8;
			Level.current = new DuckGameTestArea(this, name, _procSeed, _centerTile, genType);
		}
		Level.current.AddThing(new EditorTestLevel(this));
	}

	public override void Update()
	{
		base.Update();
	}
}
