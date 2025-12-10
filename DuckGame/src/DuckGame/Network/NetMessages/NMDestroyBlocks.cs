using System.Collections.Generic;
using System.Linq;

namespace DuckGame;

public class NMDestroyBlocks : NMEvent
{
    public HashSet<ushort> blocks = new HashSet<ushort>();

    private byte _levelIndex;

    public NMDestroyBlocks(HashSet<ushort> varBlocks)
    {
        blocks = varBlocks;
    }

    public NMDestroyBlocks()
    {
    }

    public override void Activate()
    {
        if (!(Level.current is GameLevel) || DuckNetwork.levelIndex != _levelIndex)
        {
            return;
        }
        foreach (BlockGroup g in Level.current.things[typeof(BlockGroup)])
        {
            bool had = false;
            foreach (ushort u in blocks)
            {
                Block b = g.blocks.FirstOrDefault((Block x) => x is AutoBlock && (x as AutoBlock).blockIndex == u);
                if (b != null)
                {
                    b.shouldWreck = true;
                    had = true;
                }
            }
            if (had)
            {
                g.Wreck();
            }
        }
        foreach (AutoBlock b2 in Level.current.things[typeof(AutoBlock)])
        {
            if (blocks.Contains(b2.blockIndex))
            {
                blocks.Remove(b2.blockIndex);
                b2.shouldWreck = true;
                b2.skipWreck = true;
            }
        }
    }

    protected override void OnSerialize()
    {
        base.OnSerialize();
        _serializedData.Write(DuckNetwork.levelIndex);
        _serializedData.Write((byte)blocks.Count);
        foreach (ushort u in blocks)
        {
            _serializedData.Write(u);
        }
    }

    public override void OnDeserialize(BitBuffer d)
    {
        base.OnDeserialize(d);
        _levelIndex = d.ReadByte();
        byte count = d.ReadByte();
        for (int i = 0; i < count; i++)
        {
            blocks.Add(d.ReadUShort());
        }
    }
}
