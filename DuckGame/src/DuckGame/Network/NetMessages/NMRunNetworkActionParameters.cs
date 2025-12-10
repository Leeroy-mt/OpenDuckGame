using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DuckGame;

public class NMRunNetworkActionParameters : NMEvent
{
    public PhysicsObject target;

    private MethodInfo _method;

    private object[] _parameters;

    public NMRunNetworkActionParameters(PhysicsObject pTarget, MethodInfo pMethod, object[] pParameters)
    {
        target = pTarget;
        _method = pMethod;
        _parameters = pParameters;
        if (pMethod.GetParameters().Count() != pParameters.Count())
        {
            throw new Exception("NMRunNetworkActionParameters.pParameters.Count() != MethodInfo.GetParameters().Count(). Are you including the correct parameters in your SyncNetworkAction call?");
        }
    }

    public NMRunNetworkActionParameters()
    {
    }

    protected override void OnSerialize()
    {
        BitBuffer buf = new BitBuffer();
        buf.Write(target);
        buf.Write(Editor.NetworkActionIndex(target.GetType(), _method));
        for (int i = 0; i < _parameters.Count(); i++)
        {
            buf.Write(_parameters[i]);
        }
        _serializedData.Write(buf);
    }

    public override void OnDeserialize(BitBuffer d)
    {
        BitBuffer buf = d.ReadBitBuffer();
        target = buf.Read<PhysicsObject>();
        if (target == null)
        {
            return;
        }
        _method = Editor.MethodFromNetworkActionIndex(target.GetType(), buf.ReadByte());
        if (_method != null)
        {
            List<object> parms = new List<object>();
            ParameterInfo[] parameters = _method.GetParameters();
            foreach (ParameterInfo i2 in parameters)
            {
                parms.Add(buf.Read(i2.ParameterType));
            }
            _parameters = parms.ToArray();
        }
    }

    public override void Activate()
    {
        if (target != null && _method != null)
        {
            _method.Invoke(target, _parameters);
        }
    }
}
