using System;

namespace Microsoft.Xna.Framework.Input;

public class KeyEventArgs : EventArgs
{
	private Keys keyCode;

	public Keys KeyCode => keyCode;

	public KeyEventArgs(Keys keyCode)
	{
		this.keyCode = keyCode;
	}
}
