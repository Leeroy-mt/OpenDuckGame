using System.Collections.Generic;

namespace DuckGame
{
    public class DInputPad : AnalogGamePad
    {
        public bool prevIsConnected;
        private Dictionary<int, string> _triggerNames = new Dictionary<int, string>()
    {
      {
        4096,
        "2"
      },
      {
        8192,
        "3"
      },
      {
        16384,
        "1"
      },
      {
        32768,
        "4"
      },
      {
        16,
        Triggers.Start
      },
      {
        32,
        "BACK"
      },
      {
        4,
        Triggers.Left
      },
      {
        8,
        Triggers.Right
      },
      {
        1,
        Triggers.Up
      },
      {
        2,
        Triggers.Down
      },
      {
        2097152,
        "L{"
      },
      {
        1073741824,
        "L/"
      },
      {
        268435456,
        "L}"
      },
      {
        536870912,
        "L~"
      },
      {
        134217728,
        "R{"
      },
      {
        67108864,
        "R/"
      },
      {
        16777216,
        "R}"
      },
      {
        33554432,
        "R~"
      },
      {
        256,
        "LB"
      },
      {
        512,
        "RB"
      },
      {
        8388608,
        "LT"
      },
      {
        4194304,
        "RT"
      },
      {
        64,
        "LS"
      },
      {
        128,
        "RS"
      }
    };
        private Dictionary<int, string> _triggerNamesPS = new Dictionary<int, string>()
    {
      {
        4096,
        "CROSS"
      },
      {
        8192,
        "CIRCLE"
      },
      {
        16384,
        "SQUARE"
      },
      {
        32768,
        "TRIANGLE"
      },
      {
        16,
        Triggers.Start
      },
      {
        32,
        "BACK"
      },
      {
        4,
        Triggers.Left
      },
      {
        8,
        Triggers.Right
      },
      {
        1,
        Triggers.Up
      },
      {
        2,
        Triggers.Down
      },
      {
        2097152,
        "L{"
      },
      {
        1073741824,
        "L/"
      },
      {
        268435456,
        "L}"
      },
      {
        536870912,
        "L~"
      },
      {
        134217728,
        "R{"
      },
      {
        67108864,
        "R/"
      },
      {
        16777216,
        "R}"
      },
      {
        33554432,
        "R~"
      },
      {
        256,
        "L1"
      },
      {
        512,
        "R1"
      },
      {
        8388608,
        "L2"
      },
      {
        4194304,
        "R2"
      },
      {
        64,
        "L3"
      },
      {
        128,
        "R3"
      }
    };
        private Dictionary<int, Sprite> _triggerImages = new Dictionary<int, Sprite>()
    {
      {
        4096,
         new ButtonImage('\u0002')
      },
      {
        8192,
         new ButtonImage('\u0003')
      },
      {
        16384,
         new ButtonImage('\u0001')
      },
      {
        32768,
         new ButtonImage('\u0004')
      },
      {
        16,
         new ButtonImage('\n')
      },
      {
        32,
         new ButtonImage('\t')
      },
      {
        4,
        new Sprite("buttons/xbox/dPadLeft")
      },
      {
        8,
        new Sprite("buttons/xbox/dPadRight")
      },
      {
        1,
        new Sprite("buttons/xbox/dPadUp")
      },
      {
        2,
        new Sprite("buttons/xbox/dPadDown")
      },
      {
        256,
         new ButtonImage('\u0005')
      },
      {
        512,
         new ButtonImage('\u0006')
      },
      {
        8388608,
         new ButtonImage('\a')
      },
      {
        4194304,
         new ButtonImage('\b')
      },
      {
        64,
         new ButtonImage('\f')
      },
      {
        128,
         new ButtonImage('\r')
      },
      {
        9999,
        new Sprite("buttons/xbox/dPad")
      },
      {
        9998,
        new Sprite("buttons/xbox/dPad")
      }
    };
        private Dictionary<int, Sprite> _triggerImagesPS = new Dictionary<int, Sprite>()
    {
      {
        4096,
        new Sprite("buttons/ps4/x")
      },
      {
        8192,
        new Sprite("buttons/ps4/circle")
      },
      {
        16384,
        new Sprite("buttons/ps4/square")
      },
      {
        32768,
        new Sprite("buttons/ps4/triangle")
      },
      {
        16,
        new Sprite("buttons/ps4/startButton")
      },
      {
        32,
        new Sprite("buttons/ps4/startButton")
      },
      {
        4,
        new Sprite("buttons/ps4/dPadLeft")
      },
      {
        8,
        new Sprite("buttons/ps4/dPadRight")
      },
      {
        1,
        new Sprite("buttons/ps4/dPadUp")
      },
      {
        2,
        new Sprite("buttons/ps4/dPadDown")
      },
      {
        256,
        new Sprite("buttons/ps4/leftBumper")
      },
      {
        512,
        new Sprite("buttons/ps4/rightBumper")
      },
      {
        8388608,
        new Sprite("buttons/ps4/leftTrigger")
      },
      {
        4194304,
        new Sprite("buttons/ps4/rightTrigger")
      },
      {
        64,
        new Sprite("buttons/ps4/leftStick")
      },
      {
        128,
        new Sprite("buttons/ps4/rightStick")
      },
      {
        9999,
        new Sprite("buttons/ps4/dPad")
      },
      {
        9998,
        new Sprite("buttons/ps4/dPad")
      }
    };
        private XInputPad _internalXInput;

        public override bool allowStartRemap => true;

        public override int numSticks => 2;

        public override int numTriggers => 2;

        public bool isMaybePlaystation => _productName == "Wireless Controller";

        public override string productName
        {
            get
            {
                if (isConnected)
                {
                    //string productName = DInput.GetProductName(index); old
                    string productName = ""; //new
                    if (productName != null)
                        _productName = productName.Trim();
                    else
                        _productName = null;
                }
                return _productName;
            }
            set => _productName = value;
        }

        public override string productGUID
        {
            get
            {
                if (isConnected)
                {
                    //string productGuid = DInput.GetProductGUID(index); old
                    string productGuid = ""; //new
                    if (productGuid != null)
                        _productGUID = productGuid;
                    else
                        _productGUID = null;
                }
                return _productGUID;
            }
            set => _productGUID = value;
        }

        public override bool isConnected => /*DInput.IsConnected(index) old*/ false; //new

        public bool isXInput => /*DInput.IsXInput(index) old*/ false; //new

        public DInputPad(int idx)
          : base(idx)
        {
            _name = "dinput" + idx.ToString();
            //_productName = DInput.GetProductName(index); old
            if (_productName != null)
                _productName = _productName.Trim();
            //_productGUID = DInput.GetProductGUID(index); old
        }

        public override Dictionary<int, string> GetTriggerNames()
        {
            if (isXInput)
            {
                if (_internalXInput == null)
                    _internalXInput = new XInputPad(0);
                return _internalXInput._triggerNames;
            }
            return isMaybePlaystation ? _triggerNamesPS : _triggerNames;
        }

        public override Sprite GetMapImage(int map)
        {
            Sprite mapImage;
            if (isXInput)
            {
                if (_internalXInput == null)
                    _internalXInput = new XInputPad(0);
                _internalXInput._triggerImages.TryGetValue(map, out mapImage);
            }
            else if (isMaybePlaystation)
                _triggerImagesPS.TryGetValue(map, out mapImage);
            else
                _triggerImages.TryGetValue(map, out mapImage);
            return mapImage;
        }

        //private PadState ConvertDInputState(DInputState state)
        //{
        //    PadState padState = new PadState();
        //    if (state == null)
        //        return padState;
        //    if (isXInput)
        //    {
        //        if (state.buttons[0])
        //            padState.buttons |= PadButton.A;
        //        if (state.buttons[3])
        //            padState.buttons |= PadButton.Y;
        //        if (state.buttons[1])
        //            padState.buttons |= PadButton.B;
        //        if (state.buttons[2])
        //            padState.buttons |= PadButton.X;
        //        if (state.buttons[4])
        //            padState.buttons |= PadButton.LeftShoulder;
        //        if (state.buttons[5])
        //            padState.buttons |= PadButton.RightShoulder;
        //        if (state.buttons[6])
        //        {
        //            padState.buttons |= PadButton.Back;
        //            padState.triggers.left = 1f;
        //        }
        //        if (state.buttons[7])
        //            padState.buttons |= PadButton.Start;
        //        if (state.buttons[8])
        //            padState.buttons |= PadButton.LeftStick;
        //        if (state.buttons[9])
        //            padState.buttons |= PadButton.RightStick;
        //        if (state.buttons[11])
        //            padState.buttons |= PadButton.LeftStick;
        //        if (state.buttons[12])
        //            padState.buttons |= PadButton.RightStick;
        //        if (state.left)
        //            padState.buttons |= PadButton.DPadLeft;
        //        if (state.right)
        //            padState.buttons |= PadButton.DPadRight;
        //        if (state.up)
        //            padState.buttons |= PadButton.DPadUp;
        //        if (state.down)
        //            padState.buttons |= PadButton.DPadDown;
        //        padState.sticks.left = new Vector2(state.leftX, state.leftY * -1f);
        //        padState.sticks.right = new Vector2(state.rightX, -state.rightY);
        //        if (padState.sticks.left.Length() < 0.1f)
        //            padState.sticks.left = Vector2.Zero;
        //        if (padState.sticks.right.Length() < 0.1f)
        //            padState.sticks.right = Vector2.Zero;
        //        if (state.leftZ > 0f)
        //            padState.triggers.left = state.leftZ;
        //    }
        //    else
        //    {
        //        if (state.buttons[0])
        //            padState.buttons |= PadButton.X;
        //        if (state.buttons[3])
        //            padState.buttons |= PadButton.Y;
        //        if (state.buttons[1])
        //            padState.buttons |= PadButton.A;
        //        if (state.buttons[2])
        //            padState.buttons |= PadButton.B;
        //        if (state.buttons[4])
        //            padState.buttons |= PadButton.LeftShoulder;
        //        if (state.buttons[5])
        //            padState.buttons |= PadButton.RightShoulder;
        //        if (state.buttons[6])
        //        {
        //            padState.buttons |= PadButton.LeftTrigger;
        //            padState.triggers.left = 1f;
        //        }
        //        if (state.buttons[7])
        //        {
        //            padState.buttons |= PadButton.RightTrigger;
        //            padState.triggers.right = 1f;
        //        }
        //        if (state.buttons[8])
        //            padState.buttons |= PadButton.Back;
        //        if (state.buttons[9])
        //            padState.buttons |= PadButton.Start;
        //        if (state.buttons[11])
        //            padState.buttons |= PadButton.LeftStick;
        //        if (state.buttons[12])
        //            padState.buttons |= PadButton.RightStick;
        //        if (state.left)
        //            padState.buttons |= PadButton.DPadLeft;
        //        if (state.right)
        //            padState.buttons |= PadButton.DPadRight;
        //        if (state.up)
        //            padState.buttons |= PadButton.DPadUp;
        //        if (state.down)
        //            padState.buttons |= PadButton.DPadDown;
        //        padState.sticks.left = new Vector2(state.leftX, state.leftY * -1f);
        //        padState.sticks.right = new Vector2(state.leftZ, -state.rightZ);
        //        if (padState.sticks.left.Length() < 0.1f)
        //            padState.sticks.left = Vector2.Zero;
        //        if (padState.sticks.right.Length() < 0.1f)
        //            padState.sticks.right = Vector2.Zero;
        //    }
        //    return padState;
        //} old

        protected override PadState GetState(int index) => /*ConvertDInputState(DInput.GetState(index))*/ default;
    }
}


//using System.Collections.Generic;

//namespace DuckGame;

//public class DInputPad : AnalogGamePad
//{
//	public bool prevIsConnected;

//	private Dictionary<int, string> _triggerNames = new Dictionary<int, string>
//	{
//		{ 4096, "2" },
//		{ 8192, "3" },
//		{ 16384, "1" },
//		{ 32768, "4" },
//		{ 16, "START" },
//		{ 32, "BACK" },
//		{ 4, "LEFT" },
//		{ 8, "RIGHT" },
//		{ 1, "UP" },
//		{ 2, "DOWN" },
//		{ 2097152, "L{" },
//		{ 1073741824, "L/" },
//		{ 268435456, "L}" },
//		{ 536870912, "L~" },
//		{ 134217728, "R{" },
//		{ 67108864, "R/" },
//		{ 16777216, "R}" },
//		{ 33554432, "R~" },
//		{ 256, "LB" },
//		{ 512, "RB" },
//		{ 8388608, "LT" },
//		{ 4194304, "RT" },
//		{ 64, "LS" },
//		{ 128, "RS" }
//	};

//	private Dictionary<int, string> _triggerNamesPS = new Dictionary<int, string>
//	{
//		{ 4096, "CROSS" },
//		{ 8192, "CIRCLE" },
//		{ 16384, "SQUARE" },
//		{ 32768, "TRIANGLE" },
//		{ 16, "START" },
//		{ 32, "BACK" },
//		{ 4, "LEFT" },
//		{ 8, "RIGHT" },
//		{ 1, "UP" },
//		{ 2, "DOWN" },
//		{ 2097152, "L{" },
//		{ 1073741824, "L/" },
//		{ 268435456, "L}" },
//		{ 536870912, "L~" },
//		{ 134217728, "R{" },
//		{ 67108864, "R/" },
//		{ 16777216, "R}" },
//		{ 33554432, "R~" },
//		{ 256, "L1" },
//		{ 512, "R1" },
//		{ 8388608, "L2" },
//		{ 4194304, "R2" },
//		{ 64, "L3" },
//		{ 128, "R3" }
//	};

//	private Dictionary<int, Sprite> _triggerImages = new Dictionary<int, Sprite>
//	{
//		{
//			4096,
//			new ButtonImage('\u0002')
//		},
//		{
//			8192,
//			new ButtonImage('\u0003')
//		},
//		{
//			16384,
//			new ButtonImage('\u0001')
//		},
//		{
//			32768,
//			new ButtonImage('\u0004')
//		},
//		{
//			16,
//			new ButtonImage('\n')
//		},
//		{
//			32,
//			new ButtonImage('\t')
//		},
//		{
//			4,
//			new Sprite("buttons/xbox/dPadLeft")
//		},
//		{
//			8,
//			new Sprite("buttons/xbox/dPadRight")
//		},
//		{
//			1,
//			new Sprite("buttons/xbox/dPadUp")
//		},
//		{
//			2,
//			new Sprite("buttons/xbox/dPadDown")
//		},
//		{
//			256,
//			new ButtonImage('\u0005')
//		},
//		{
//			512,
//			new ButtonImage('\u0006')
//		},
//		{
//			8388608,
//			new ButtonImage('\a')
//		},
//		{
//			4194304,
//			new ButtonImage('\b')
//		},
//		{
//			64,
//			new ButtonImage('\f')
//		},
//		{
//			128,
//			new ButtonImage('\r')
//		},
//		{
//			9999,
//			new Sprite("buttons/xbox/dPad")
//		},
//		{
//			9998,
//			new Sprite("buttons/xbox/dPad")
//		}
//	};

//	private Dictionary<int, Sprite> _triggerImagesPS = new Dictionary<int, Sprite>
//	{
//		{
//			4096,
//			new Sprite("buttons/ps4/x")
//		},
//		{
//			8192,
//			new Sprite("buttons/ps4/circle")
//		},
//		{
//			16384,
//			new Sprite("buttons/ps4/square")
//		},
//		{
//			32768,
//			new Sprite("buttons/ps4/triangle")
//		},
//		{
//			16,
//			new Sprite("buttons/ps4/startButton")
//		},
//		{
//			32,
//			new Sprite("buttons/ps4/startButton")
//		},
//		{
//			4,
//			new Sprite("buttons/ps4/dPadLeft")
//		},
//		{
//			8,
//			new Sprite("buttons/ps4/dPadRight")
//		},
//		{
//			1,
//			new Sprite("buttons/ps4/dPadUp")
//		},
//		{
//			2,
//			new Sprite("buttons/ps4/dPadDown")
//		},
//		{
//			256,
//			new Sprite("buttons/ps4/leftBumper")
//		},
//		{
//			512,
//			new Sprite("buttons/ps4/rightBumper")
//		},
//		{
//			8388608,
//			new Sprite("buttons/ps4/leftTrigger")
//		},
//		{
//			4194304,
//			new Sprite("buttons/ps4/rightTrigger")
//		},
//		{
//			64,
//			new Sprite("buttons/ps4/leftStick")
//		},
//		{
//			128,
//			new Sprite("buttons/ps4/rightStick")
//		},
//		{
//			9999,
//			new Sprite("buttons/ps4/dPad")
//		},
//		{
//			9998,
//			new Sprite("buttons/ps4/dPad")
//		}
//	};

//	private XInputPad _internalXInput;

//	public override bool allowStartRemap => true;

//	public override int numSticks => 2;

//	public override int numTriggers => 2;

//	public bool isMaybePlaystation => _productName == "Wireless Controller";

//	public override string productName
//	{
//		get
//		{
//			if (isConnected)
//			{
//				string name = DInput.GetProductName(base.index);
//				if (name != null)
//				{
//					_productName = name.Trim();
//				}
//				else
//				{
//					_productName = null;
//				}
//			}
//			return _productName;
//		}
//		set
//		{
//			_productName = value;
//		}
//	}

//	public override string productGUID
//	{
//		get
//		{
//			if (isConnected)
//			{
//				string guid = DInput.GetProductGUID(base.index);
//				if (guid != null)
//				{
//					_productGUID = guid;
//				}
//				else
//				{
//					_productGUID = null;
//				}
//			}
//			return _productGUID;
//		}
//		set
//		{
//			_productGUID = value;
//		}
//	}

//	public override bool isConnected => DInput.IsConnected(base.index);

//	public bool isXInput => DInput.IsXInput(base.index);

//	public DInputPad(int idx)
//		: base(idx)
//	{
//		_name = "dinput" + idx;
//		_productName = DInput.GetProductName(base.index);
//		if (_productName != null)
//		{
//			_productName = _productName.Trim();
//		}
//		_productGUID = DInput.GetProductGUID(base.index);
//	}

//	public override Dictionary<int, string> GetTriggerNames()
//	{
//		if (isXInput)
//		{
//			if (_internalXInput == null)
//			{
//				_internalXInput = new XInputPad(0);
//			}
//			return _internalXInput._triggerNames;
//		}
//		if (isMaybePlaystation)
//		{
//			return _triggerNamesPS;
//		}
//		return _triggerNames;
//	}

//	public override Sprite GetMapImage(int map)
//	{
//		Sprite spr = null;
//		if (isXInput)
//		{
//			if (_internalXInput == null)
//			{
//				_internalXInput = new XInputPad(0);
//			}
//			_internalXInput._triggerImages.TryGetValue(map, out spr);
//		}
//		else if (isMaybePlaystation)
//		{
//			_triggerImagesPS.TryGetValue(map, out spr);
//		}
//		else
//		{
//			_triggerImages.TryGetValue(map, out spr);
//		}
//		return spr;
//	}

//	private PadState ConvertDInputState(DInputState state)
//	{
//		PadState newState = default(PadState);
//		if (state == null)
//		{
//			return newState;
//		}
//		if (isXInput)
//		{
//			if (state.buttons[0])
//			{
//				newState.buttons |= PadButton.A;
//			}
//			if (state.buttons[3])
//			{
//				newState.buttons |= PadButton.Y;
//			}
//			if (state.buttons[1])
//			{
//				newState.buttons |= PadButton.B;
//			}
//			if (state.buttons[2])
//			{
//				newState.buttons |= PadButton.X;
//			}
//			if (state.buttons[4])
//			{
//				newState.buttons |= PadButton.LeftShoulder;
//			}
//			if (state.buttons[5])
//			{
//				newState.buttons |= PadButton.RightShoulder;
//			}
//			if (state.buttons[6])
//			{
//				newState.buttons |= PadButton.Back;
//				newState.triggers.left = 1f;
//			}
//			if (state.buttons[7])
//			{
//				newState.buttons |= PadButton.Start;
//			}
//			if (state.buttons[8])
//			{
//				newState.buttons |= PadButton.LeftStick;
//			}
//			if (state.buttons[9])
//			{
//				newState.buttons |= PadButton.RightStick;
//			}
//			if (state.buttons[11])
//			{
//				newState.buttons |= PadButton.LeftStick;
//			}
//			if (state.buttons[12])
//			{
//				newState.buttons |= PadButton.RightStick;
//			}
//			if (state.left)
//			{
//				newState.buttons |= PadButton.DPadLeft;
//			}
//			if (state.right)
//			{
//				newState.buttons |= PadButton.DPadRight;
//			}
//			if (state.up)
//			{
//				newState.buttons |= PadButton.DPadUp;
//			}
//			if (state.down)
//			{
//				newState.buttons |= PadButton.DPadDown;
//			}
//			newState.sticks.left = new Vector2(state.leftX, state.leftY * -1f);
//			newState.sticks.right = new Vector2(state.rightX, 0f - state.rightY);
//			if (newState.sticks.left.Length() < 0.1f)
//			{
//				newState.sticks.left = Vector2.Zero;
//			}
//			if (newState.sticks.right.Length() < 0.1f)
//			{
//				newState.sticks.right = Vector2.Zero;
//			}
//			if (state.leftZ > 0f)
//			{
//				newState.triggers.left = state.leftZ;
//			}
//		}
//		else
//		{
//			if (state.buttons[0])
//			{
//				newState.buttons |= PadButton.X;
//			}
//			if (state.buttons[3])
//			{
//				newState.buttons |= PadButton.Y;
//			}
//			if (state.buttons[1])
//			{
//				newState.buttons |= PadButton.A;
//			}
//			if (state.buttons[2])
//			{
//				newState.buttons |= PadButton.B;
//			}
//			if (state.buttons[4])
//			{
//				newState.buttons |= PadButton.LeftShoulder;
//			}
//			if (state.buttons[5])
//			{
//				newState.buttons |= PadButton.RightShoulder;
//			}
//			if (state.buttons[6])
//			{
//				newState.buttons |= PadButton.LeftTrigger;
//				newState.triggers.left = 1f;
//			}
//			if (state.buttons[7])
//			{
//				newState.buttons |= PadButton.RightTrigger;
//				newState.triggers.right = 1f;
//			}
//			if (state.buttons[8])
//			{
//				newState.buttons |= PadButton.Back;
//			}
//			if (state.buttons[9])
//			{
//				newState.buttons |= PadButton.Start;
//			}
//			if (state.buttons[11])
//			{
//				newState.buttons |= PadButton.LeftStick;
//			}
//			if (state.buttons[12])
//			{
//				newState.buttons |= PadButton.RightStick;
//			}
//			if (state.left)
//			{
//				newState.buttons |= PadButton.DPadLeft;
//			}
//			if (state.right)
//			{
//				newState.buttons |= PadButton.DPadRight;
//			}
//			if (state.up)
//			{
//				newState.buttons |= PadButton.DPadUp;
//			}
//			if (state.down)
//			{
//				newState.buttons |= PadButton.DPadDown;
//			}
//			newState.sticks.left = new Vector2(state.leftX, state.leftY * -1f);
//			newState.sticks.right = new Vector2(state.leftZ, 0f - state.rightZ);
//			if (newState.sticks.left.Length() < 0.1f)
//			{
//				newState.sticks.left = Vector2.Zero;
//			}
//			if (newState.sticks.right.Length() < 0.1f)
//			{
//				newState.sticks.right = Vector2.Zero;
//			}
//		}
//		return newState;
//	}

//	protected override PadState GetState(int index)
//	{
//		return ConvertDInputState(DInput.GetState(index));
//	}
//}
