using System;
using System.Reflection;

namespace DuckGame;

/// <summary>
/// The core "mod", for consistency sake.
/// </summary>
public sealed class CoreMod : Mod
{
	/// <summary>
	/// The core mod instance, for quick comparisons.
	/// </summary>
	public static CoreMod coreMod { get; internal set; }

	public override Priority priority => Priority.Inconsequential;

	internal CoreMod()
	{
		base.configuration = new ModConfiguration();
		base.configuration.assembly = Assembly.GetExecutingAssembly();
		base.configuration.contentManager = ContentManagers.GetContentManager(typeof(DefaultContentManager));
		base.configuration.name = "DuckGame";
		base.configuration.displayName = "Core";
		base.configuration.description = "The core mod for Duck Game content. This is no touchy. Bad. BAD duck. You will break your game, and I swear if you do I'm not picking that up. I just don't have the time these days to pick up after all of your goddamn mistakes. Seriously, just leave it alone. (it okay just do your best)";
		base.configuration.version = new Version(DG.version);
		base.configuration.author = "CORPTRON";
	}
}
