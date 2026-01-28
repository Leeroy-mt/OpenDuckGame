using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace DuckGame;

public class UIFlagSelection : UIMenu
{
    #region Private Fields

    static Tex2D _flagTexture;

    static List<string> _flagFiles;

    static Dictionary<int, Sprite> _sprites = [];

    int _flagSelection;

    int numFlags = 283;

    int _numFlagsPerRow = 22;

    UIMenu _openOnClose;

    #endregion

    #region Public Constructors

    public UIFlagSelection(UIMenu openOnClose, string title, float xpos, float ypos)
        : base("FLAG SELECT", xpos, ypos, 214f, 142f, "@CANCEL@BACK @SELECT@SELECT")
    {
        UIBox box = new(vert: true, isVisible: false);
        box.Add(new UIText(" ", Color.White));
        box.Add(new UIText(" ", Color.White));
        box.Add(new UIText(" ", Color.White));
        box.Add(new UIText(" ", Color.White));
        box.Add(new UIText(" ", Color.White));
        box.Add(new UIText(" ", Color.White));
        box.Add(new UIText(" ", Color.White));
        box.Add(new UIText(" ", Color.White));
        box.Add(new UIText(" ", Color.White));
        box.Add(new UIText(" ", Color.White));
        box.Add(new UIText(" ", Color.White));
        box.Add(new UIText(" ", Color.White));
        box.Add(new UIText(" ", Color.White));
        Add(box);
        _flagSelection = Global.data.flag;
        _openOnClose = openOnClose;
    }

    #endregion

    #region Public Methods

    public static int GetNumFlags()
    {
        if (_flagFiles == null)
            GetFlag(0);
        return _flagFiles.Count;
    }

    public static Sprite GetFlag(int idx, bool smallVersion = false)
    {
        Sprite s = null;
        if (smallVersion && _sprites.TryGetValue(idx, out s))
            return s;
        try
        {
            _flagTexture ??= Content.Load<Tex2D>("flags/flags");
            if (_flagTexture != null)
            {
                Tex2D ret = new(61, 41);
                int num = idx % 16;
                int yIndex = idx / 16;
                Color[] colors = new Color[2501];
                Color[] allColors = new Color[_flagTexture.width * _flagTexture.height];
                (_flagTexture.nativeObject as Texture2D).GetData(allColors);
                int xCol = num * 61;
                int yCol = yIndex * 41;
                for (int yPos = 0; yPos < 41; yPos++)
                    for (int xPos = 0; xPos < 61; xPos++)
                        colors[xPos + yPos * 61] = allColors[xCol + xPos + (yCol + yPos) * _flagTexture.width];
                ret.SetData(colors);
                s = new Sprite(ret);
                _sprites[idx] = s;
            }
            else
                DevConsole.Log(DCSection.General, "Found no renderable flags...");
        }
        catch (Exception ex)
        {
            DevConsole.Log(DCSection.General, "GetFlag(" + idx + ") threw an exception:");
            DevConsole.Log(DCSection.General, ex.Message);
        }
        return s;
    }

    public override void Update()
    {
        if (open && !animating)
        {
            if (Input.Pressed("LEFT"))
            {
                _flagSelection--;
                if (_flagSelection <= 0)
                    _flagSelection = 0;
            }
            if (Input.Pressed("RIGHT"))
            {
                _flagSelection++;
                if (_flagSelection >= numFlags)
                    _flagSelection = numFlags - 1;
            }
            if (Input.Pressed("UP"))
            {
                _flagSelection -= _numFlagsPerRow;
                if (_flagSelection <= 0)
                    _flagSelection = 0;
            }
            if (Input.Pressed("DOWN"))
            {
                _flagSelection += _numFlagsPerRow;
                if (_flagSelection >= numFlags)
                    _flagSelection = numFlags - 1;
            }
            if (Input.Pressed("SELECT"))
            {
                Global.data.flag = _flagSelection;
                Close();
                _openOnClose.Open();
            }
            if (Input.Pressed("CANCEL"))
            {
                Close();
                _openOnClose.Open();
            }
        }
        base.Update();
    }

    public override void Draw()
    {
        _flagTexture ??= Content.Load<Tex2D>("flags/flags");
        int idx = 0;
        int flagIndx = 0;
        float xDraw = 0;
        float yDraw = 10;
        float xOffset = X - width / 2 + 8;
        float yOffset = Y - height / 2 + 8;
        for (int i = 0; i < numFlags; i++)
        {
            int xIndex = i % 16;
            int yIndex = i / 16;
            Graphics.Draw(_flagTexture, new Vec2(xOffset + xDraw, yOffset + yDraw), new Rectangle(xIndex * 61, yIndex * 41, 61, 41), (flagIndx == _flagSelection) ? Color.White : (Color.White * 0.7f), 0f, Vec2.Zero, new Vec2(0.14f, 0.14f), SpriteEffects.None, 0.9f);
            xDraw += 9;
            idx++;
            flagIndx++;
            if (idx >= _numFlagsPerRow)
            {
                idx = 0;
                yDraw += 8;
                xDraw = 0;
            }
        }
        base.Draw();
    }

    #endregion
}
