using System;

namespace Microsoft.Xna.Framework.Input;

public class KeyEventArgs(Keys keyCode) : EventArgs
{
    public Keys KeyCode { get; } = keyCode;
}
