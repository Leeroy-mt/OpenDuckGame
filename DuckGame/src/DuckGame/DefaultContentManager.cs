using System;
using System.Collections.Generic;
using System.Linq;

namespace DuckGame;

/// <summary>
/// The quick and easy default implementation. Pulls all exported types
/// that are subclassed by the requested Type.
/// </summary>
internal class DefaultContentManager : IManageContent
{
	public IEnumerable<Type> Compile<T>(Mod mod)
	{
		return from type in mod.configuration.assembly.GetTypes()
			where type.IsSubclassOf(typeof(T))
			select type;
	}
}
