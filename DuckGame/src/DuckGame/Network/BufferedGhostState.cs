using System;
using System.Collections.Generic;

namespace DuckGame;

public class BufferedGhostState
{
    public BufferedGhostState previousState;

    public BufferedGhostState nextState;

    public List<ushort> inputStates = new List<ushort>();

    public List<BufferedGhostProperty> properties = new List<BufferedGhostProperty>(30);

    public long mask;

    public NetIndex16 tick = 0;

    public NetIndex8 authority = 0;

    public int _framesApplied;

    public Thing owner
    {
        get
        {
            if (properties.Count <= 0)
            {
                return null;
            }
            return properties[0].binding.owner as Thing;
        }
    }

    public override string ToString()
    {
        return "Tick: " + tick.ToString() + " FA: " + _framesApplied + " Mask: " + mask;
    }

    public BufferedGhostState()
    {
        for (int i = 0; i < NetworkConnection.packetsEvery; i++)
        {
            inputStates.Add(0);
        }
    }

    ~BufferedGhostState()
    {
    }

    public void ReInitialize()
    {
        properties.Clear();
        _framesApplied = 0;
    }

    public void Reset(bool clearProperties = true)
    {
        if (clearProperties)
        {
            properties.Clear();
        }
        _framesApplied = 0;
    }

    public void Apply(float lerp, BufferedGhostState updateNetworkState, bool pApplyPosition = true)
    {
        foreach (BufferedGhostProperty prop in properties)
        {
            try
            {
                if (prop.isNetworkStateValue)
                {
                    prop.Apply(1f);
                }
                else if (!(updateNetworkState.properties[prop.index].tick > prop.tick))
                {
                    if (!pApplyPosition)
                    {
                        prop.Apply(0f);
                    }
                    else
                    {
                        prop.Apply((_framesApplied >= NetworkConnection.packetsEvery) ? 1f : NetworkConnection.ghostLerpDivisor);
                    }
                    updateNetworkState.properties[prop.index].UpdateFrom(prop);
                }
            }
            catch (Exception e)
            {
                System_ApplyException(e, prop, updateNetworkState);
            }
        }
        _framesApplied++;
    }

    private void System_ApplyException(Exception e, BufferedGhostProperty prop, BufferedGhostState updateNetworkState = null)
    {
        string specialCode = "";
        if (GhostObject.applyContext != null)
        {
            specialCode = ((prop == null) ? (specialCode + GhostObject.applyContext.thing.GetType().Name) : ((updateNetworkState != null) ? (specialCode + GhostObject.applyContext.thing.GetType().Name + "." + prop.binding.name + "=" + ((prop.value != null) ? prop.value.ToString() : "null") + "(" + prop.index + "/" + updateNetworkState.properties.Count + ")") : (specialCode + GhostObject.applyContext.thing.GetType().Name + "." + prop.binding.name + "=" + ((prop.value != null) ? prop.value.ToString() : "null"))));
        }
        DevConsole.LogComplexMessage("Error applying BufferedGhostProperty (" + specialCode + "): " + e.Message, Color.Red);
    }

    public void ApplyImmediately(long pMask, BufferedGhostState updateNetworkState)
    {
        foreach (BufferedGhostProperty prop in properties)
        {
            if (prop.isNetworkStateValue || updateNetworkState.properties[prop.index].tick > prop.tick)
            {
                continue;
            }
            long itemMask = 1L << prop.index;
            if ((pMask & itemMask) != 0L)
            {
                try
                {
                    prop.Apply(1f);
                    updateNetworkState.properties[prop.index].UpdateFrom(prop.binding);
                }
                catch (Exception e)
                {
                    System_ApplyException(e, prop, updateNetworkState);
                }
            }
        }
        _framesApplied = NetworkConnection.packetsEvery;
    }

    public void ApplyImmediately(BufferedGhostState updateNetworkState)
    {
        foreach (BufferedGhostProperty prop in properties)
        {
            if (!prop.isNetworkStateValue && !(updateNetworkState.properties[prop.index].tick > prop.tick))
            {
                try
                {
                    prop.Apply(1f);
                    updateNetworkState.properties[prop.index].UpdateFrom(prop);
                }
                catch (Exception e)
                {
                    System_ApplyException(e, prop, updateNetworkState);
                }
            }
        }
        _framesApplied = NetworkConnection.packetsEvery;
    }

    public void ApplyImmediately()
    {
        foreach (BufferedGhostProperty prop in properties)
        {
            try
            {
                prop.Apply(1f);
            }
            catch (Exception e)
            {
                System_ApplyException(e, prop);
            }
        }
        _framesApplied = NetworkConnection.packetsEvery;
    }

    private Vec2 Slerp(Vec2 from, Vec2 to, float step)
    {
        if (step == 0f)
        {
            return from;
        }
        if (from == to || step == 1f)
        {
            return to;
        }
        double theta = Math.Acos(Vec2.Dot(from, to));
        if (theta == 0.0)
        {
            return to;
        }
        double sinTheta = Math.Sin(theta);
        return (float)(Math.Sin((double)(1f - step) * theta) / sinTheta) * from + (float)(Math.Sin((double)step * theta) / sinTheta) * to;
    }
}
