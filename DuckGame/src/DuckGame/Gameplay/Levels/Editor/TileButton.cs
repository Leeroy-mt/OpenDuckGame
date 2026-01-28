using System;

namespace DuckGame;

public class TileButton : Thing
{
    private FieldBinding _binding;

    private FieldBinding _visibleBinding;

    private SpriteMap _sprite;

    private string _hoverText;

    private bool _hover;

    private InputProfile _focus;

    private TileButtonAlign _align;

    private Vec2 _alignOffset = Vec2.Zero;

    public override bool visible
    {
        get
        {
            if (_visibleBinding == null)
            {
                return base.visible;
            }
            return (bool)_visibleBinding.value;
        }
    }

    public string hoverText => _hoverText;

    public bool hover
    {
        get
        {
            return _hover;
        }
        set
        {
            _hover = value;
        }
    }

    public InputProfile focus
    {
        get
        {
            return _focus;
        }
        set
        {
            _focus = value;
        }
    }

    public TileButton(float xpos, float ypos, FieldBinding binding, FieldBinding visibleBinding, SpriteMap image, string hover, TileButtonAlign align = TileButtonAlign.None, float angleDeg = 0f)
        : base(xpos, ypos)
    {
        _sprite = image;
        _hoverText = hover;
        collisionSize = new Vec2(16f, 16f);
        collisionOffset = new Vec2(-8f, -8f);
        image.Center = new Vec2(image.w / 2, image.h / 2);
        _binding = binding;
        _visibleBinding = visibleBinding;
        _align = align;
        _alignOffset = new Vec2(xpos, ypos);
        base.AngleDegrees = angleDeg;
    }

    public override void Update()
    {
        if (!visible)
        {
            Position = new Vec2(-9999f, -9999f);
        }
        else
        {
            Position = (_binding.thing as Editor).GetAlignOffset(_align) + _alignOffset;
        }
        bool inc = false;
        bool dec = false;
        float changeMult = 1f;
        if (Editor.inputMode == EditorInput.Mouse && _hover)
        {
            if (Mouse.scroll > 0f)
            {
                dec = true;
            }
            else if (Mouse.scroll < 0f)
            {
                inc = true;
            }
            if (Mouse.middle == InputState.Down)
            {
                changeMult = 0.1f;
            }
        }
        if (_focus == null && (Editor.inputMode != EditorInput.Mouse || !_hover))
        {
            return;
        }
        if (_binding.value.GetType() == typeof(float))
        {
            if ((_focus != null && _focus.Pressed("MENULEFT")) || Keyboard.Pressed(Keys.Left) || dec)
            {
                float val = (float)_binding.value;
                val = Math.Max(val - _binding.inc * changeMult, _binding.min);
                _binding.value = val;
            }
            else if ((_focus != null && _focus.Pressed("MENURIGHT")) || Keyboard.Pressed(Keys.Right) || inc)
            {
                float val2 = (float)_binding.value;
                val2 = Math.Min(val2 + _binding.inc * changeMult, _binding.max);
                _binding.value = val2;
            }
        }
        else if (_binding.value.GetType() == typeof(int))
        {
            if ((_focus != null && _focus.Pressed("MENULEFT")) || Keyboard.Pressed(Keys.Left) || dec)
            {
                float val3 = (int)_binding.value;
                val3 = Math.Max(val3 - _binding.inc * changeMult, _binding.min);
                _binding.value = (int)val3;
            }
            else if ((_focus != null && _focus.Pressed("MENURIGHT")) || Keyboard.Pressed(Keys.Right) || inc)
            {
                float val4 = (int)_binding.value;
                val4 = Math.Min(val4 + _binding.inc * changeMult, _binding.max);
                _binding.value = (int)val4;
            }
        }
        else if (_binding.value.GetType() == typeof(Vec2))
        {
            if ((_focus != null && _focus.Pressed("MENULEFT")) || Keyboard.Pressed(Keys.Left) || dec)
            {
                Vec2 val5 = (Vec2)_binding.value;
                val5.X = Math.Max(val5.X - _binding.inc * changeMult, _binding.min);
                _binding.value = val5;
            }
            else if ((_focus != null && _focus.Pressed("MENURIGHT")) || Keyboard.Pressed(Keys.Right) || inc)
            {
                Vec2 val6 = (Vec2)_binding.value;
                val6.X = Math.Min(val6.X + _binding.inc * changeMult, _binding.max);
                _binding.value = val6;
            }
            else if ((_focus != null && _focus.Pressed("MENUUP")) || Keyboard.Pressed(Keys.Up) || dec)
            {
                Vec2 val7 = (Vec2)_binding.value;
                val7.Y = Math.Max(val7.Y - _binding.inc * changeMult, _binding.min);
                _binding.value = val7;
            }
            else if ((_focus != null && _focus.Pressed("MENUDOWN")) || Keyboard.Pressed(Keys.Down) || inc)
            {
                Vec2 val8 = (Vec2)_binding.value;
                val8.Y = Math.Min(val8.Y + _binding.inc * changeMult, _binding.max);
                _binding.value = val8;
            }
        }
        else if (_binding.value.GetType() == typeof(bool) && ((_focus != null && _focus.Pressed("SELECT")) || Mouse.left == InputState.Pressed || inc || dec))
        {
            bool val9 = (bool)_binding.value;
            val9 = !val9;
            _binding.value = val9;
        }
    }

    public override void Draw()
    {
        _sprite.frame = (_hover ? 1 : 0);
        _sprite.Angle = Angle;
        if (_binding.value.GetType() == typeof(bool))
        {
            bool val = (bool)_binding.value;
            _sprite.color = Color.White * (val ? 1f : 0.3f);
        }
        Graphics.Draw(_sprite, base.X, base.Y);
        if (_binding.value.GetType() == typeof(float))
        {
            Graphics.DrawString(((float)_binding.value).ToString("0.00"), new Vec2(base.X + 12f, base.Y - 4f), Color.White);
        }
        if (_binding.value.GetType() == typeof(int))
        {
            Graphics.DrawString(((int)_binding.value).ToString(), new Vec2(base.X + 12f, base.Y - 4f), Color.White);
        }
        _hover = false;
        base.Draw();
    }
}
