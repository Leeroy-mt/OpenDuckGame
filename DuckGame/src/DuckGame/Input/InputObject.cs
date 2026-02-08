using Microsoft.Xna.Framework;

namespace DuckGame;

public class InputObject : Thing, ITakeInput
{
    public StateBinding _profileNumberBinding = new StateBinding(nameof(profileNumber));

    public StateBinding _votedBinding = new StateBinding(nameof(voted));

    public StateBinding _inputChangeIndexBinding = new StateBinding(nameof(_inputChangeIndex));

    public StateBinding _leftStickBinding = new StateBinding(doLerp: true, nameof(leftStick));

    public StateBinding _rightStickBinding = new StateBinding(doLerp: true, nameof(rightStick));

    public StateBinding _leftTriggerBinding = new StateBinding(nameof(leftTrigger));

    public byte _inputChangeIndex;

    private sbyte _profileNumber;

    private Vector2 _leftStick;

    private Vector2 _rightStick;

    private float _leftTrigger;

    private InputProfile _blankProfile = new InputProfile();

    public Profile duckProfile;

    public bool voted;

    private ushort prevState;

    public sbyte profileNumber
    {
        get
        {
            return _profileNumber;
        }
        set
        {
            _profileNumber = value;
            duckProfile = DuckNetwork.profiles[_profileNumber];
            if (duckProfile != null && duckProfile.connection == DuckNetwork.localConnection)
            {
                connection = DuckNetwork.localConnection;
            }
        }
    }

    public Vector2 leftStick
    {
        get
        {
            if (base.isServerForObject && inputProfile != null)
            {
                return inputProfile.leftStick;
            }
            return _leftStick;
        }
        set
        {
            _leftStick = value;
        }
    }

    public Vector2 rightStick
    {
        get
        {
            if (base.isServerForObject && inputProfile != null)
            {
                return inputProfile.rightStick;
            }
            return _rightStick;
        }
        set
        {
            _rightStick = value;
        }
    }

    public float leftTrigger
    {
        get
        {
            if (base.isServerForObject && inputProfile != null)
            {
                float trigger = inputProfile.leftTrigger;
                if (inputProfile.hasMotionAxis)
                {
                    trigger += inputProfile.motionAxis;
                }
                return trigger;
            }
            return _leftTrigger;
        }
        set
        {
            _leftTrigger = value;
        }
    }

    public InputProfile inputProfile
    {
        get
        {
            if (duckProfile != null)
            {
                return duckProfile.inputProfile;
            }
            return _blankProfile;
        }
    }

    public override void Update()
    {
        if (duckProfile != null && duckProfile.connection == DuckNetwork.localConnection)
        {
            Thing.Fondle(this, DuckNetwork.localConnection);
        }
        if (base.isServerForObject && inputProfile != null)
        {
            if (!Network.isServer)
            {
                inputProfile.UpdateTriggerStates();
            }
            if (prevState != inputProfile.state)
            {
                authority++;
                _inputChangeIndex++;
            }
            prevState = inputProfile.state;
        }
        RegisteredVote v = Vote.GetVote(DuckNetwork.profiles[_profileNumber]);
        if (v != null)
        {
            v.leftStick = leftStick;
            v.rightStick = rightStick;
        }
        if (Level.current is RockScoreboard)
        {
            foreach (Slot3D slot in (Level.current as RockScoreboard)._slots)
            {
                if (slot.duck != null && slot.duck.profile == duckProfile)
                {
                    if (inputProfile.virtualDevice != null)
                    {
                        inputProfile.virtualDevice.leftStick = leftStick;
                        inputProfile.virtualDevice.rightStick = rightStick;
                    }
                    slot.ai._manualQuack = inputProfile;
                    slot.duck.manualQuackPitch = true;
                    slot.duck.quackPitch = (byte)(leftTrigger * 255f);
                }
            }
        }
        base.Update();
    }
}
