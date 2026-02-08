using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace DuckGame;

public class CollisionTree
{
    private Vector2 _position;

    private float _width;

    private CollisionTree[] _children;

    private SpriteFont _font;

    private List<Thing> _objects = new List<Thing>();

    private int _depth;

    public Vector2 position => _position;

    public CollisionTree(float xpos, float ypos, float wval, int divisions)
    {
        _depth = divisions;
        _position = new Vector2(xpos, ypos);
        _width = wval;
        _font = Content.Load<SpriteFont>("font_SuperNew");
        if (_depth > 0)
        {
            _children = new CollisionTree[4];
            _children[0] = new CollisionTree(_position.X, _position.Y, _width * 0.5f, _depth - 1);
            _children[1] = new CollisionTree(_position.X + _width * 0.5f, _position.Y, _width * 0.5f, _depth - 1);
            _children[2] = new CollisionTree(_position.X, _position.Y + _width * 0.5f, _width * 0.5f, _depth - 1);
            _children[3] = new CollisionTree(_position.X + _width * 0.5f, _position.Y + _width * 0.5f, _width * 0.5f, _depth - 1);
        }
    }

    public void Add(Thing t)
    {
    }

    public void Draw()
    {
    }
}
