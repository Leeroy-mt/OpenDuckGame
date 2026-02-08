using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace DuckGame;

public class BlockGroup : AutoBlock
{
    private List<Block> _blocks = new List<Block>();

    private bool _wreck;

    public List<Block> blocks => _blocks;

    public override void SetTranslation(Vector2 translation)
    {
        foreach (Block block in _blocks)
        {
            block.SetTranslation(translation);
        }
        base.SetTranslation(translation);
    }

    public void Add(Block b)
    {
        _blocks.Add(b);
        b.group = this;
        _impactThreshold = Math.Min(_impactThreshold, b.impactThreshold);
        willHeat = willHeat || b.willHeat;
        if (b is AutoBlock)
        {
            _tileset = (b as AutoBlock)._tileset;
        }
    }

    public void Remove(Block b)
    {
        _blocks.Remove(b);
        _wreck = true;
    }

    public BlockGroup()
        : base(0f, 0f, "")
    {
        _isStatic = true;
    }

    public void CalculateSize()
    {
        Vector2 tl = new Vector2(99999f, 99999f);
        Vector2 br = new Vector2(-99999f, -99999f);
        foreach (Block b in _blocks)
        {
            if (b.left < tl.X)
            {
                tl.X = b.left;
            }
            if (b.right > br.X)
            {
                br.X = b.right;
            }
            if (b.top < tl.Y)
            {
                tl.Y = b.top;
            }
            if (b.bottom > br.Y)
            {
                br.Y = b.bottom;
            }
            physicsMaterial = b.physicsMaterial;
            thickness = b.thickness;
        }
        Position = (tl + br) / 2f;
        collisionOffset = tl - Position;
        collisionSize = br - tl;
    }

    public void Wreck()
    {
        foreach (Block block in _blocks)
        {
            Level.Add(block);
        }
        Level.Remove(this);
    }

    public override void Update()
    {
        if (_wreck)
        {
            foreach (Block block in _blocks)
            {
                Level.Add(block);
            }
            Level.Remove(this);
            _wreck = false;
        }
        if (needsRefresh)
        {
            foreach (Block b in _blocks)
            {
                if (b is AutoBlock)
                {
                    (b as AutoBlock).PlaceBlock();
                }
            }
            needsRefresh = false;
        }
        base.Update();
    }

    public override List<BlockCorner> GetGroupCorners()
    {
        if (_blocks.Count > 0)
        {
            return _blocks[0].GetGroupCorners();
        }
        return base.GetGroupCorners();
    }

    public override void Draw()
    {
        foreach (Block block in _blocks)
        {
            block.Draw();
        }
        if (DevConsole.showCollision)
        {
            Graphics.DrawRect(base.topLeft + new Vector2(-0.5f, 0.5f), base.bottomRight + new Vector2(0.5f, -0.5f), Color.Green * 0.5f, 1f);
        }
    }

    public override void OnSolidImpact(MaterialThing with, ImpactedFrom from)
    {
        foreach (Block b in _blocks)
        {
            if (Collision.Rect(b.topLeft, b.bottomRight, with))
            {
                b.OnSolidImpact(with, from);
            }
        }
    }

    public override void HeatUp(Vector2 location)
    {
        if (willHeat)
        {
            foreach (Block b in _blocks)
            {
                if (Collision.Circle(location, 3f, b))
                {
                    b.HeatUp(location);
                }
            }
        }
        base.HeatUp(location);
    }

    public override void Terminate()
    {
        foreach (Block block in _blocks)
        {
            block.groupedWithNeighbors = false;
            block.Terminate();
        }
    }
}
