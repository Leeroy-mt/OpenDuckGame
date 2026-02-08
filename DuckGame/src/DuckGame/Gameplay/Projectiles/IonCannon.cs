using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace DuckGame;

public class IonCannon : Thing
{
    public float _blast = 1f;

    private Vector2 _target;

    public bool serverVersion;

    public IonCannon(Vector2 pos, Vector2 target)
        : base(pos.X, pos.Y)
    {
        _target = target;
    }

    public override void Initialize()
    {
        Vector2 upVec = Vector2.Normalize((Position - _target).Rotate(Maths.DegToRad(-90f), Vector2.Zero));
        Vector2 downVec = Vector2.Normalize((Position - _target).Rotate(Maths.DegToRad(90f), Vector2.Zero));
        Level.Add(new LaserLine(Position, _target - Position, upVec, 4f, Color.White, 1f, 0.03f));
        Level.Add(new LaserLine(Position, _target - Position, downVec, 4f, Color.White, 1f, 0.03f));
        Level.Add(new LaserLine(Position, _target - Position, upVec, 2.5f, Color.White, 2f, 0.03f));
        Level.Add(new LaserLine(Position, _target - Position, downVec, 2.5f, Color.White, 2f, 0.03f));
        if (!serverVersion)
        {
            return;
        }
        float wide = 64f;
        float numCheck = 12f;
        float inc = wide / (numCheck - 1f);
        Vector2 checkStart = Position + upVec * wide / 2f;
        HashSet<ushort> idx = new HashSet<ushort>();
        List<BlockGroup> wreckGroups = new List<BlockGroup>();
        for (int i = 0; (float)i < numCheck; i++)
        {
            Vector2 test = checkStart + downVec * inc * i;
            foreach (PhysicsObject item in Level.CheckLineAll<PhysicsObject>(test, test + (_target - Position)))
            {
                item.Destroy(new DTIncinerate(this));
                item._sleeping = false;
                item.vSpeed = -2f;
            }
            foreach (BlockGroup block in Level.CheckLineAll<BlockGroup>(test, test + (_target - Position)))
            {
                if (block == null)
                {
                    continue;
                }
                new List<Block>();
                foreach (Block bl in block.blocks)
                {
                    if (Collision.Line(test, test + (_target - Position), bl.rectangle))
                    {
                        bl.shouldWreck = true;
                        if (bl is AutoBlock)
                        {
                            idx.Add((bl as AutoBlock).blockIndex);
                        }
                    }
                }
                wreckGroups.Add(block);
            }
            foreach (Block block2 in Level.CheckLineAll<Block>(test, test + (_target - Position)))
            {
                if (block2 is AutoBlock)
                {
                    block2.skipWreck = true;
                    block2.shouldWreck = true;
                    idx.Add((block2 as AutoBlock).blockIndex);
                }
                else if (block2 is Door || block2 is VerticalDoor)
                {
                    Level.Remove(block2);
                    block2.Destroy(new DTRocketExplosion(null));
                }
            }
        }
        foreach (BlockGroup item2 in wreckGroups)
        {
            item2.Wreck();
        }
        if (Network.isActive && idx.Count > 0)
        {
            Send.Message(new NMDestroyBlocks(idx));
        }
        if (Recorder.currentRecording != null)
        {
            Recorder.currentRecording.LogBonus();
        }
    }

    public override void Update()
    {
        _blast = Maths.CountDown(_blast, 0.05f);
        if (_blast < 0f)
        {
            Level.Remove(this);
        }
    }

    public override void Draw()
    {
        Maths.NormalizeSection(_blast, 0f, 0.2f);
        Maths.NormalizeSection(_blast, 0.6f, 1f);
        _ = _blast;
        _ = 0f;
        Vector2 upVec = Vector2.Normalize((Position - _target).Rotate(Maths.DegToRad(-90f), Vector2.Zero));
        Vector2 downVec = Vector2.Normalize((Position - _target).Rotate(Maths.DegToRad(90f), Vector2.Zero));
        float wide = 64f;
        float numCheck = 7f;
        float inc = wide / (numCheck - 1f);
        Vector2 checkStart = Position + upVec * wide / 2f;
        for (int i = 0; (float)i < numCheck; i++)
        {
            Vector2 vec = checkStart + downVec * inc * i;
            Graphics.DrawLine(vec, vec + (_target - Position), Color.SkyBlue * (_blast * 0.9f), 2f, 0.9f);
        }
    }
}
