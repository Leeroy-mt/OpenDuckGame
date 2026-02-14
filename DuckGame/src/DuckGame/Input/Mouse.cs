using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace DuckGame;

public class Mouse : InputDevice
{
    private static Vector2 _mousePos;

    private static Vector2 _mouseScreenPos;

    private static MouseState _mouseState;

    private static MouseState _mouseStatePrev;

    private static float _prevScrollValue;

    private static bool _outOfFocus;

    public const int kMouseLBMapping = 999990;

    public const int kMouseMBMapping = 999991;

    public const int kMouseRBMapping = 999992;

    public const int kMouseScrollUpMapping = 999993;

    public const int kMouseScrollDownMapping = 999994;

    public static InputState left
    {
        get
        {
            if (_mouseState.LeftButton == ButtonState.Pressed && _mouseStatePrev.LeftButton == ButtonState.Released)
            {
                return InputState.Pressed;
            }
            if (_mouseState.LeftButton == ButtonState.Pressed && _mouseStatePrev.LeftButton == ButtonState.Pressed)
            {
                return InputState.Down;
            }
            if (_mouseState.LeftButton == ButtonState.Released && _mouseStatePrev.LeftButton == ButtonState.Pressed)
            {
                return InputState.Released;
            }
            return InputState.None;
        }
    }

    public static InputState middle
    {
        get
        {
            if (_mouseState.MiddleButton == ButtonState.Pressed && _mouseStatePrev.MiddleButton == ButtonState.Released)
            {
                return InputState.Pressed;
            }
            if (_mouseState.MiddleButton == ButtonState.Pressed && _mouseStatePrev.MiddleButton == ButtonState.Pressed)
            {
                return InputState.Down;
            }
            if (_mouseState.MiddleButton == ButtonState.Released && _mouseStatePrev.MiddleButton == ButtonState.Pressed)
            {
                return InputState.Released;
            }
            return InputState.None;
        }
    }

    public static InputState right
    {
        get
        {
            if (_mouseState.RightButton == ButtonState.Pressed && _mouseStatePrev.RightButton == ButtonState.Released)
            {
                return InputState.Pressed;
            }
            if (_mouseState.RightButton == ButtonState.Pressed && _mouseStatePrev.RightButton == ButtonState.Pressed)
            {
                return InputState.Down;
            }
            if (_mouseState.RightButton == ButtonState.Released && _mouseStatePrev.RightButton == ButtonState.Pressed)
            {
                return InputState.Released;
            }
            return InputState.None;
        }
    }

    public static bool available => true;

    public static float scroll => _mouseStatePrev.ScrollWheelValue - _mouseState.ScrollWheelValue;

    public static bool prevScrollDown => _prevScrollValue > 0f;

    public static bool prevScrollUp => _prevScrollValue < 0f;

    public static float x => _mouseScreenPos.X;

    public static float y => _mouseScreenPos.Y;

    public static float xScreen => positionScreen.X;

    public static float yScreen => positionScreen.Y;

    public static float xConsole => positionConsole.X;

    public static float yConsole => positionConsole.Y;

    public static Vector2 position
    {
        get
        {
            return new Vector2(x, y);
        }
        set
        {
            _mouseScreenPos = value;
            value = new Vector2(value.X / Layer.HUD.camera.width * Resolution.size.X, value.Y / Layer.HUD.camera.height * Resolution.size.Y);
            Microsoft.Xna.Framework.Input.Mouse.SetPosition((int)value.X, (int)value.Y);
        }
    }

    public static Vector2 mousePos => _mousePos;

    public static Vector2 positionScreen => Level.current.camera.transformScreenVector(_mousePos);

    public static Vector2 positionConsole => Layer.Console.camera.transformScreenVector(_mousePos);

    public override void Update()
    {
        if (!Graphics.inFocus)
        {
            _outOfFocus = true;
            return;
        }
        _prevScrollValue = scroll;
        _mouseStatePrev = _mouseState;
        _mouseState = Microsoft.Xna.Framework.Input.Mouse.GetState();
        Vector3 mousePos = new Vector3(_mouseState.X, _mouseState.Y, 0f);
        if (Graphics._screenViewport.HasValue)
        {
            _mouseScreenPos = new Vector2(mousePos.X / Resolution.size.X * Layer.HUD.camera.width, mousePos.Y / Resolution.size.Y * Layer.HUD.camera.height);
        }
        _mouseScreenPos.X = (int)_mouseScreenPos.X;
        _mouseScreenPos.Y = (int)_mouseScreenPos.Y;
        _mousePos = new Vector2(_mouseState.X, _mouseState.Y);
        if (_outOfFocus)
        {
            if (_mouseState.LeftButton == ButtonState.Released && _mouseState.MiddleButton == ButtonState.Released && _mouseState.RightButton == ButtonState.Released)
            {
                _outOfFocus = false;
            }
            else
            {
                _mouseState = (_mouseStatePrev = new MouseState(_mouseState.X, _mouseState.Y, _mouseState.ScrollWheelValue, ButtonState.Released, ButtonState.Released, ButtonState.Released, ButtonState.Released, ButtonState.Released));
            }
        }
    }
}
