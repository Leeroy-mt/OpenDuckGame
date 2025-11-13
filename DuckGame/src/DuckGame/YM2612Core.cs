using System;
using System.Linq;

namespace DuckGame;

public class YM2612Core
{
	public class FM_SLOT
	{
		public LongPointerArray32 DT = new LongPointerArray32();

		public byte KSR;

		public uint ar;

		public uint d1r;

		public uint d2r;

		public uint rr;

		public byte ksr;

		public uint mul;

		public uint phase;

		public long Incr;

		public byte state;

		public uint tl;

		public long volume;

		public uint sl;

		public uint vol_out;

		public byte eg_sh_ar;

		public byte eg_sel_ar;

		public byte eg_sh_d1r;

		public byte eg_sel_d1r;

		public byte eg_sh_d2r;

		public byte eg_sel_d2r;

		public byte eg_sh_rr;

		public byte eg_sel_rr;

		public byte ssg;

		public byte ssgn;

		public byte key;

		public uint AMmask;
	}

	public class FM_CH
	{
		public FM_SLOT[] SLOT = new FM_SLOT[4];

		public byte ALGO;

		public byte FB;

		public long[] op1_out = new long[2];

		public LongPointer connect1;

		public LongPointer connect3;

		public LongPointer connect2;

		public LongPointer connect4;

		public LongPointer mem_connect;

		public long mem_value;

		public long pms;

		public byte ams;

		public uint fc;

		public byte kcode;

		public uint block_fnum;

		public FM_CH()
		{
			SLOT[0] = new FM_SLOT();
			SLOT[1] = new FM_SLOT();
			SLOT[2] = new FM_SLOT();
			SLOT[3] = new FM_SLOT();
		}
	}

	public class FM_ST
	{
		public double clock;

		public uint rate;

		public ushort address;

		public byte status;

		public uint mode;

		public byte fn_h;

		public long TimerBase;

		public long TA;

		public long TAL;

		public long TAC;

		public long TB;

		public long TBL;

		public long TBC;

		public LongPointerArray32[] dt_tab = new LongPointerArray32[8];

		public FM_ST()
		{
			for (int i = 0; i < 8; i++)
			{
				dt_tab[i] = new LongPointerArray32();
			}
		}
	}

	public class FM_3SLOT
	{
		public uint[] fc = new uint[3];

		public byte fn_h;

		public byte[] kcode = new byte[3];

		public uint[] block_fnum = new uint[3];

		public byte key_csm;
	}

	public class FM_OPN
	{
		public FM_ST ST = new FM_ST();

		public FM_3SLOT SL3 = new FM_3SLOT();

		public uint[] pan = new uint[12];

		public uint eg_cnt;

		public uint eg_timer;

		public uint eg_timer_add;

		public uint eg_timer_overflow;

		public uint[] fn_table = new uint[4096];

		public uint fn_max;

		public byte lfo_cnt;

		public uint lfo_timer;

		public uint lfo_timer_add;

		public uint lfo_timer_overflow;

		public uint LFO_AM;

		public uint LFO_PM;
	}

	public class _YM2612_data
	{
		public FM_CH[] CH = new FM_CH[6];

		public byte dacen;

		public long dacout;

		public FM_OPN OPN = new FM_OPN();

		public _YM2612_data()
		{
			for (int i = 0; i < 6; i++)
			{
				CH[i] = new FM_CH();
			}
		}
	}

	private const int FREQ_SH = 16;

	private const int EG_SH = 16;

	private const int LFO_SH = 24;

	private const int TIMER_SH = 16;

	private const int FREQ_MASK = 65535;

	private const int ENV_BITS = 10;

	private const int ENV_LEN = 1024;

	private const float ENV_STEP_GX = 0.125f;

	private const int MAX_ATT_INDEX = 1023;

	private const int MIN_ATT_INDEX = 0;

	private const int EG_ATT = 4;

	private const int EG_DEC = 3;

	private const int EG_SUS = 2;

	private const int EG_REL = 1;

	private const int EG_OFF = 0;

	private const int SIN_BITS = 10;

	private const int SIN_LEN = 1024;

	private const int SIN_MASK_GX = 1023;

	private const int TL_RES_LEN = 256;

	private static uint[] sl_table = new uint[16]
	{
		SC(0),
		SC(1),
		SC(2),
		SC(3),
		SC(4),
		SC(5),
		SC(6),
		SC(7),
		SC(8),
		SC(9),
		SC(10),
		SC(11),
		SC(12),
		SC(13),
		SC(14),
		SC(31)
	};

	private const int RATE_STEPS = 8;

	private static byte[] eg_inc = new byte[152]
	{
		0, 1, 0, 1, 0, 1, 0, 1, 0, 1,
		0, 1, 1, 1, 0, 1, 0, 1, 1, 1,
		0, 1, 1, 1, 0, 1, 1, 1, 1, 1,
		1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
		1, 1, 1, 2, 1, 1, 1, 2, 1, 2,
		1, 2, 1, 2, 1, 2, 1, 2, 2, 2,
		1, 2, 2, 2, 2, 2, 2, 2, 2, 2,
		2, 2, 2, 2, 2, 4, 2, 2, 2, 4,
		2, 4, 2, 4, 2, 4, 2, 4, 2, 4,
		4, 4, 2, 4, 4, 4, 4, 4, 4, 4,
		4, 4, 4, 4, 4, 4, 4, 8, 4, 4,
		4, 8, 4, 8, 4, 8, 4, 8, 4, 8,
		4, 8, 8, 8, 4, 8, 8, 8, 8, 8,
		8, 8, 8, 8, 8, 8, 16, 16, 16, 16,
		16, 16, 16, 16, 0, 0, 0, 0, 0, 0,
		0, 0
	};

	private static byte[] eg_rate_select = new byte[128]
	{
		eg_rate_selectO(18),
		eg_rate_selectO(18),
		eg_rate_selectO(18),
		eg_rate_selectO(18),
		eg_rate_selectO(18),
		eg_rate_selectO(18),
		eg_rate_selectO(18),
		eg_rate_selectO(18),
		eg_rate_selectO(18),
		eg_rate_selectO(18),
		eg_rate_selectO(18),
		eg_rate_selectO(18),
		eg_rate_selectO(18),
		eg_rate_selectO(18),
		eg_rate_selectO(18),
		eg_rate_selectO(18),
		eg_rate_selectO(18),
		eg_rate_selectO(18),
		eg_rate_selectO(18),
		eg_rate_selectO(18),
		eg_rate_selectO(18),
		eg_rate_selectO(18),
		eg_rate_selectO(18),
		eg_rate_selectO(18),
		eg_rate_selectO(18),
		eg_rate_selectO(18),
		eg_rate_selectO(18),
		eg_rate_selectO(18),
		eg_rate_selectO(18),
		eg_rate_selectO(18),
		eg_rate_selectO(18),
		eg_rate_selectO(18),
		eg_rate_selectO(18),
		eg_rate_selectO(18),
		eg_rate_selectO(0),
		eg_rate_selectO(0),
		eg_rate_selectO(0),
		eg_rate_selectO(0),
		eg_rate_selectO(2),
		eg_rate_selectO(2),
		eg_rate_selectO(0),
		eg_rate_selectO(1),
		eg_rate_selectO(2),
		eg_rate_selectO(3),
		eg_rate_selectO(0),
		eg_rate_selectO(1),
		eg_rate_selectO(2),
		eg_rate_selectO(3),
		eg_rate_selectO(0),
		eg_rate_selectO(1),
		eg_rate_selectO(2),
		eg_rate_selectO(3),
		eg_rate_selectO(0),
		eg_rate_selectO(1),
		eg_rate_selectO(2),
		eg_rate_selectO(3),
		eg_rate_selectO(0),
		eg_rate_selectO(1),
		eg_rate_selectO(2),
		eg_rate_selectO(3),
		eg_rate_selectO(0),
		eg_rate_selectO(1),
		eg_rate_selectO(2),
		eg_rate_selectO(3),
		eg_rate_selectO(0),
		eg_rate_selectO(1),
		eg_rate_selectO(2),
		eg_rate_selectO(3),
		eg_rate_selectO(0),
		eg_rate_selectO(1),
		eg_rate_selectO(2),
		eg_rate_selectO(3),
		eg_rate_selectO(0),
		eg_rate_selectO(1),
		eg_rate_selectO(2),
		eg_rate_selectO(3),
		eg_rate_selectO(0),
		eg_rate_selectO(1),
		eg_rate_selectO(2),
		eg_rate_selectO(3),
		eg_rate_selectO(4),
		eg_rate_selectO(5),
		eg_rate_selectO(6),
		eg_rate_selectO(7),
		eg_rate_selectO(8),
		eg_rate_selectO(9),
		eg_rate_selectO(10),
		eg_rate_selectO(11),
		eg_rate_selectO(12),
		eg_rate_selectO(13),
		eg_rate_selectO(14),
		eg_rate_selectO(15),
		eg_rate_selectO(16),
		eg_rate_selectO(16),
		eg_rate_selectO(16),
		eg_rate_selectO(16),
		eg_rate_selectO(16),
		eg_rate_selectO(16),
		eg_rate_selectO(16),
		eg_rate_selectO(16),
		eg_rate_selectO(16),
		eg_rate_selectO(16),
		eg_rate_selectO(16),
		eg_rate_selectO(16),
		eg_rate_selectO(16),
		eg_rate_selectO(16),
		eg_rate_selectO(16),
		eg_rate_selectO(16),
		eg_rate_selectO(16),
		eg_rate_selectO(16),
		eg_rate_selectO(16),
		eg_rate_selectO(16),
		eg_rate_selectO(16),
		eg_rate_selectO(16),
		eg_rate_selectO(16),
		eg_rate_selectO(16),
		eg_rate_selectO(16),
		eg_rate_selectO(16),
		eg_rate_selectO(16),
		eg_rate_selectO(16),
		eg_rate_selectO(16),
		eg_rate_selectO(16),
		eg_rate_selectO(16),
		eg_rate_selectO(16),
		eg_rate_selectO(16),
		eg_rate_selectO(16),
		eg_rate_selectO(16),
		eg_rate_selectO(16)
	};

	private static byte[] eg_rate_shift = new byte[128]
	{
		eg_rate_shiftO(11),
		eg_rate_shiftO(11),
		eg_rate_shiftO(11),
		eg_rate_shiftO(11),
		eg_rate_shiftO(11),
		eg_rate_shiftO(11),
		eg_rate_shiftO(11),
		eg_rate_shiftO(11),
		eg_rate_shiftO(11),
		eg_rate_shiftO(11),
		eg_rate_shiftO(11),
		eg_rate_shiftO(11),
		eg_rate_shiftO(11),
		eg_rate_shiftO(11),
		eg_rate_shiftO(11),
		eg_rate_shiftO(11),
		eg_rate_shiftO(11),
		eg_rate_shiftO(11),
		eg_rate_shiftO(11),
		eg_rate_shiftO(11),
		eg_rate_shiftO(11),
		eg_rate_shiftO(11),
		eg_rate_shiftO(11),
		eg_rate_shiftO(11),
		eg_rate_shiftO(11),
		eg_rate_shiftO(11),
		eg_rate_shiftO(11),
		eg_rate_shiftO(11),
		eg_rate_shiftO(11),
		eg_rate_shiftO(11),
		eg_rate_shiftO(11),
		eg_rate_shiftO(11),
		eg_rate_shiftO(11),
		eg_rate_shiftO(11),
		eg_rate_shiftO(11),
		eg_rate_shiftO(11),
		eg_rate_shiftO(10),
		eg_rate_shiftO(10),
		eg_rate_shiftO(10),
		eg_rate_shiftO(10),
		eg_rate_shiftO(9),
		eg_rate_shiftO(9),
		eg_rate_shiftO(9),
		eg_rate_shiftO(9),
		eg_rate_shiftO(8),
		eg_rate_shiftO(8),
		eg_rate_shiftO(8),
		eg_rate_shiftO(8),
		eg_rate_shiftO(7),
		eg_rate_shiftO(7),
		eg_rate_shiftO(7),
		eg_rate_shiftO(7),
		eg_rate_shiftO(6),
		eg_rate_shiftO(6),
		eg_rate_shiftO(6),
		eg_rate_shiftO(6),
		eg_rate_shiftO(5),
		eg_rate_shiftO(5),
		eg_rate_shiftO(5),
		eg_rate_shiftO(5),
		eg_rate_shiftO(4),
		eg_rate_shiftO(4),
		eg_rate_shiftO(4),
		eg_rate_shiftO(4),
		eg_rate_shiftO(3),
		eg_rate_shiftO(3),
		eg_rate_shiftO(3),
		eg_rate_shiftO(3),
		eg_rate_shiftO(2),
		eg_rate_shiftO(2),
		eg_rate_shiftO(2),
		eg_rate_shiftO(2),
		eg_rate_shiftO(1),
		eg_rate_shiftO(1),
		eg_rate_shiftO(1),
		eg_rate_shiftO(1),
		eg_rate_shiftO(0),
		eg_rate_shiftO(0),
		eg_rate_shiftO(0),
		eg_rate_shiftO(0),
		eg_rate_shiftO(0),
		eg_rate_shiftO(0),
		eg_rate_shiftO(0),
		eg_rate_shiftO(0),
		eg_rate_shiftO(0),
		eg_rate_shiftO(0),
		eg_rate_shiftO(0),
		eg_rate_shiftO(0),
		eg_rate_shiftO(0),
		eg_rate_shiftO(0),
		eg_rate_shiftO(0),
		eg_rate_shiftO(0),
		eg_rate_shiftO(0),
		eg_rate_shiftO(0),
		eg_rate_shiftO(0),
		eg_rate_shiftO(0),
		eg_rate_shiftO(0),
		eg_rate_shiftO(0),
		eg_rate_shiftO(0),
		eg_rate_shiftO(0),
		eg_rate_shiftO(0),
		eg_rate_shiftO(0),
		eg_rate_shiftO(0),
		eg_rate_shiftO(0),
		eg_rate_shiftO(0),
		eg_rate_shiftO(0),
		eg_rate_shiftO(0),
		eg_rate_shiftO(0),
		eg_rate_shiftO(0),
		eg_rate_shiftO(0),
		eg_rate_shiftO(0),
		eg_rate_shiftO(0),
		eg_rate_shiftO(0),
		eg_rate_shiftO(0),
		eg_rate_shiftO(0),
		eg_rate_shiftO(0),
		eg_rate_shiftO(0),
		eg_rate_shiftO(0),
		eg_rate_shiftO(0),
		eg_rate_shiftO(0),
		eg_rate_shiftO(0),
		eg_rate_shiftO(0),
		eg_rate_shiftO(0),
		eg_rate_shiftO(0),
		eg_rate_shiftO(0),
		eg_rate_shiftO(0),
		eg_rate_shiftO(0),
		eg_rate_shiftO(0)
	};

	private static byte[] dt_tab = new byte[128]
	{
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 1, 1, 1, 1,
		1, 1, 1, 1, 2, 2, 2, 2, 2, 3,
		3, 3, 4, 4, 4, 5, 5, 6, 6, 7,
		8, 8, 8, 8, 1, 1, 1, 1, 2, 2,
		2, 2, 2, 3, 3, 3, 4, 4, 4, 5,
		5, 6, 6, 7, 8, 8, 9, 10, 11, 12,
		13, 14, 16, 16, 16, 16, 2, 2, 2, 2,
		2, 3, 3, 3, 4, 4, 4, 5, 5, 6,
		6, 7, 8, 8, 9, 10, 11, 12, 13, 14,
		16, 17, 19, 20, 22, 22, 22, 22
	};

	private static byte[] opn_fktable = new byte[16]
	{
		0, 0, 0, 0, 0, 0, 0, 1, 2, 3,
		3, 3, 3, 3, 3, 3
	};

	private static uint[] lfo_samples_per_step = new uint[8] { 108u, 77u, 71u, 67u, 62u, 44u, 8u, 5u };

	private static byte[] lfo_ams_depth_shift = new byte[4] { 8, 3, 1, 0 };

	private static byte[,] lfo_pm_output = new byte[56, 8]
	{
		{ 0, 0, 0, 0, 0, 0, 0, 0 },
		{ 0, 0, 0, 0, 0, 0, 0, 0 },
		{ 0, 0, 0, 0, 0, 0, 0, 0 },
		{ 0, 0, 0, 0, 0, 0, 0, 0 },
		{ 0, 0, 0, 0, 0, 0, 0, 0 },
		{ 0, 0, 0, 0, 0, 0, 0, 0 },
		{ 0, 0, 0, 0, 0, 0, 0, 0 },
		{ 0, 0, 0, 0, 1, 1, 1, 1 },
		{ 0, 0, 0, 0, 0, 0, 0, 0 },
		{ 0, 0, 0, 0, 0, 0, 0, 0 },
		{ 0, 0, 0, 0, 0, 0, 0, 0 },
		{ 0, 0, 0, 0, 0, 0, 0, 0 },
		{ 0, 0, 0, 0, 0, 0, 0, 0 },
		{ 0, 0, 0, 0, 0, 0, 0, 0 },
		{ 0, 0, 0, 0, 1, 1, 1, 1 },
		{ 0, 0, 1, 1, 2, 2, 2, 3 },
		{ 0, 0, 0, 0, 0, 0, 0, 0 },
		{ 0, 0, 0, 0, 0, 0, 0, 0 },
		{ 0, 0, 0, 0, 0, 0, 0, 0 },
		{ 0, 0, 0, 0, 0, 0, 0, 0 },
		{ 0, 0, 0, 0, 0, 0, 0, 1 },
		{ 0, 0, 0, 0, 1, 1, 1, 1 },
		{ 0, 0, 1, 1, 2, 2, 2, 3 },
		{ 0, 0, 2, 3, 4, 4, 5, 6 },
		{ 0, 0, 0, 0, 0, 0, 0, 0 },
		{ 0, 0, 0, 0, 0, 0, 0, 0 },
		{ 0, 0, 0, 0, 0, 0, 1, 1 },
		{ 0, 0, 0, 0, 1, 1, 1, 1 },
		{ 0, 0, 0, 1, 1, 1, 1, 2 },
		{ 0, 0, 1, 1, 2, 2, 2, 3 },
		{ 0, 0, 2, 3, 4, 4, 5, 6 },
		{ 0, 0, 4, 6, 8, 8, 10, 12 },
		{ 0, 0, 0, 0, 0, 0, 0, 0 },
		{ 0, 0, 0, 0, 1, 1, 1, 1 },
		{ 0, 0, 0, 1, 1, 1, 2, 2 },
		{ 0, 0, 1, 1, 2, 2, 3, 3 },
		{ 0, 0, 1, 2, 2, 2, 3, 4 },
		{ 0, 0, 2, 3, 4, 4, 5, 6 },
		{ 0, 0, 4, 6, 8, 8, 10, 12 },
		{ 0, 0, 8, 12, 16, 16, 20, 24 },
		{ 0, 0, 0, 0, 0, 0, 0, 0 },
		{ 0, 0, 0, 0, 2, 2, 2, 2 },
		{ 0, 0, 0, 2, 2, 2, 4, 4 },
		{ 0, 0, 2, 2, 4, 4, 6, 6 },
		{ 0, 0, 2, 4, 4, 4, 6, 8 },
		{ 0, 0, 4, 6, 8, 8, 10, 12 },
		{ 0, 0, 8, 12, 16, 16, 20, 24 },
		{ 0, 0, 16, 24, 32, 32, 40, 48 },
		{ 0, 0, 0, 0, 0, 0, 0, 0 },
		{ 0, 0, 0, 0, 4, 4, 4, 4 },
		{ 0, 0, 0, 4, 4, 4, 8, 8 },
		{ 0, 0, 4, 4, 8, 8, 12, 12 },
		{ 0, 0, 4, 8, 8, 8, 12, 16 },
		{ 0, 0, 8, 12, 16, 16, 20, 24 },
		{ 0, 0, 16, 24, 32, 32, 40, 48 },
		{ 0, 0, 32, 48, 64, 64, 80, 96 }
	};

	private const int STATE_SIZE = 295168;

	private const string STATE_VERSION = "GENPLUS-GX 1.6.1";

	private const int ENV_QUIET = 832;

	private const int SLOT1 = 0;

	private const int SLOT2 = 2;

	private const int SLOT3 = 1;

	private const int SLOT4 = 3;

	private const int TL_TAB_LEN = 6656;

	private int[] tl_tab = new int[6656];

	private uint[] sin_tab = new uint[1024];

	private long[] lfo_pm_table = new long[32768];

	private t_config config = new t_config();

	private _YM2612_data ym2612 = new _YM2612_data();

	private LongPointer m2 = new LongPointer();

	private LongPointer c1 = new LongPointer();

	private LongPointer c2 = new LongPointer();

	private LongPointer mem = new LongPointer();

	private LongPointer[] out_fm = new LongPointer[8];

	private static uint SC(int db)
	{
		return (uint)((float)db * 32f);
	}

	private static byte eg_rate_selectO(byte a)
	{
		return (byte)(a * 8);
	}

	private static byte eg_rate_shiftO(byte a)
	{
		return a;
	}

	public YM2612Core()
	{
		for (int i = 0; i < 8; i++)
		{
			out_fm[i] = new LongPointer();
		}
	}

	private void FM_KEYON(FM_CH CH, int s)
	{
		FM_SLOT SLOT = CH.SLOT[s];
		if (SLOT.key == 0 && ym2612.OPN.SL3.key_csm == 0)
		{
			SLOT.phase = 0u;
			SLOT.ssgn = 0;
			if (SLOT.ar + SLOT.ksr < 94)
			{
				SLOT.state = (byte)((SLOT.volume > 0) ? 4u : ((SLOT.sl == 0) ? 2u : 3u));
			}
			else
			{
				SLOT.volume = 0L;
				SLOT.state = (byte)((SLOT.sl == 0) ? 2u : 3u);
			}
			if ((SLOT.ssg & 8) != 0 && (SLOT.ssgn ^ (SLOT.ssg & 4)) != 0)
			{
				SLOT.vol_out = (uint)((int)(512 - SLOT.volume) & 0x3FF) + SLOT.tl;
			}
			else
			{
				SLOT.vol_out = (uint)(int)SLOT.volume + SLOT.tl;
			}
		}
		SLOT.key = 1;
	}

	private void FM_KEYOFF(FM_CH CH, int s)
	{
		FM_SLOT SLOT = CH.SLOT[s];
		if (SLOT.key > 0 && ym2612.OPN.SL3.key_csm == 0 && SLOT.state > 1)
		{
			SLOT.state = 1;
			if ((SLOT.ssg & 8) > 0)
			{
				if ((SLOT.ssgn ^ (SLOT.ssg & 4)) != 0)
				{
					SLOT.volume = 512 - SLOT.volume;
				}
				if (SLOT.volume >= 512)
				{
					SLOT.volume = 1023L;
					SLOT.state = 0;
				}
				SLOT.vol_out = (uint)(int)SLOT.volume + SLOT.tl;
			}
		}
		SLOT.key = 0;
	}

	private void FM_KEYON_CSM(FM_CH CH, int s)
	{
		FM_SLOT SLOT = CH.SLOT[s];
		if (SLOT.key == 0 && ym2612.OPN.SL3.key_csm == 0)
		{
			SLOT.phase = 0u;
			SLOT.ssgn = 0;
			if (SLOT.ar + SLOT.ksr < 94)
			{
				SLOT.state = (byte)((SLOT.volume > 0) ? 4u : ((SLOT.sl == 0) ? 2u : 3u));
			}
			else
			{
				SLOT.volume = 0L;
				SLOT.state = (byte)((SLOT.sl == 0) ? 2u : 3u);
			}
			if ((SLOT.ssg & 8) != 0 && (SLOT.ssgn ^ (SLOT.ssg & 4)) != 0)
			{
				SLOT.vol_out = (uint)((int)(512 - SLOT.volume) & 0x3FF) + SLOT.tl;
			}
			else
			{
				SLOT.vol_out = (uint)(int)SLOT.volume + SLOT.tl;
			}
		}
	}

	private void FM_KEYOFF_CSM(FM_CH CH, int s)
	{
		FM_SLOT SLOT = CH.SLOT[s];
		if (SLOT.key != 0 || SLOT.state <= 1)
		{
			return;
		}
		SLOT.state = 1;
		if ((SLOT.ssg & 8) > 0)
		{
			if ((SLOT.ssgn ^ (SLOT.ssg & 4)) > 0)
			{
				SLOT.volume = 512 - SLOT.volume;
			}
			if (SLOT.volume >= 512)
			{
				SLOT.volume = 1023L;
				SLOT.state = 0;
			}
			SLOT.vol_out = (uint)(int)SLOT.volume + SLOT.tl;
		}
	}

	private void CSMKeyControll(FM_CH CH)
	{
		FM_KEYON_CSM(CH, 0);
		FM_KEYON_CSM(CH, 2);
		FM_KEYON_CSM(CH, 1);
		FM_KEYON_CSM(CH, 3);
		ym2612.OPN.SL3.key_csm = 1;
	}

	private void INTERNAL_TIMER_A()
	{
		if ((ym2612.OPN.ST.mode & 1) != 0 && (ym2612.OPN.ST.TAC -= ym2612.OPN.ST.TimerBase) <= 0)
		{
			if ((ym2612.OPN.ST.mode & 4) != 0)
			{
				ym2612.OPN.ST.status |= 1;
			}
			if (ym2612.OPN.ST.TAL != 0L)
			{
				ym2612.OPN.ST.TAC += ym2612.OPN.ST.TAL;
			}
			else
			{
				ym2612.OPN.ST.TAC = ym2612.OPN.ST.TAL;
			}
			if ((ym2612.OPN.ST.mode & 0xC0) == 128)
			{
				CSMKeyControll(ym2612.CH[2]);
			}
		}
	}

	private void INTERNAL_TIMER_B(int step)
	{
		if ((ym2612.OPN.ST.mode & 2) != 0 && (ym2612.OPN.ST.TBC -= ym2612.OPN.ST.TimerBase * step) <= 0)
		{
			if ((ym2612.OPN.ST.mode & 8) != 0)
			{
				ym2612.OPN.ST.status |= 2;
			}
			if (ym2612.OPN.ST.TBL != 0L)
			{
				ym2612.OPN.ST.TBC += ym2612.OPN.ST.TBL;
			}
			else
			{
				ym2612.OPN.ST.TBC = ym2612.OPN.ST.TBL;
			}
		}
	}

	private void set_timers(int v)
	{
		if (((ym2612.OPN.ST.mode ^ v) & 0xC0) != 0L)
		{
			ym2612.CH[2].SLOT[0].Incr = -1L;
			if ((v & 0xC0) != 128 && ym2612.OPN.SL3.key_csm != 0)
			{
				FM_KEYOFF_CSM(ym2612.CH[2], 0);
				FM_KEYOFF_CSM(ym2612.CH[2], 2);
				FM_KEYOFF_CSM(ym2612.CH[2], 1);
				FM_KEYOFF_CSM(ym2612.CH[2], 3);
				ym2612.OPN.SL3.key_csm = 0;
			}
		}
		if ((v & 1) != 0 && (ym2612.OPN.ST.mode & 1) == 0)
		{
			ym2612.OPN.ST.TAC = ym2612.OPN.ST.TAL;
		}
		if ((v & 2) != 0 && (ym2612.OPN.ST.mode & 2) == 0)
		{
			ym2612.OPN.ST.TBC = ym2612.OPN.ST.TBL;
		}
		ym2612.OPN.ST.status &= (byte)(~v >> 4);
		ym2612.OPN.ST.mode = (uint)v;
	}

	private void setup_connection(FM_CH CH, int ch)
	{
		LongPointer carrier = out_fm[ch];
		switch (CH.ALGO)
		{
		case 0:
			CH.connect1 = c1;
			CH.connect2 = mem;
			CH.connect3 = c2;
			CH.mem_connect = m2;
			break;
		case 1:
			CH.connect1 = mem;
			CH.connect2 = mem;
			CH.connect3 = c2;
			CH.mem_connect = m2;
			break;
		case 2:
			CH.connect1 = c2;
			CH.connect2 = mem;
			CH.connect3 = c2;
			CH.mem_connect = m2;
			break;
		case 3:
			CH.connect1 = c1;
			CH.connect2 = mem;
			CH.connect3 = c2;
			CH.mem_connect = c2;
			break;
		case 4:
			CH.connect1 = c1;
			CH.connect2 = carrier;
			CH.connect3 = c2;
			CH.mem_connect = mem;
			break;
		case 5:
			CH.connect1 = null;
			CH.connect2 = carrier;
			CH.connect3 = carrier;
			CH.mem_connect = m2;
			break;
		case 6:
			CH.connect1 = c1;
			CH.connect2 = carrier;
			CH.connect3 = carrier;
			CH.mem_connect = mem;
			break;
		case 7:
			CH.connect1 = carrier;
			CH.connect2 = carrier;
			CH.connect3 = carrier;
			CH.mem_connect = mem;
			break;
		}
		CH.connect4 = carrier;
	}

	private void set_det_mul(FM_CH CH, FM_SLOT SLOT, int v)
	{
		SLOT.mul = (((v & 0xF) == 0) ? 1u : ((uint)((v & 0xF) * 2)));
		SLOT.DT = ym2612.OPN.ST.dt_tab[(v >> 4) & 7];
		CH.SLOT[0].Incr = -1L;
	}

	private void set_tl(FM_SLOT SLOT, int v)
	{
		SLOT.tl = (uint)((v & 0x7F) << 3);
		if ((SLOT.ssg & 8) != 0 && (SLOT.ssgn ^ (SLOT.ssg & 4)) != 0 && SLOT.state > 1)
		{
			SLOT.vol_out = (uint)((int)(512 - SLOT.volume) & 0x3FF) + SLOT.tl;
		}
		else
		{
			SLOT.vol_out = (uint)(int)SLOT.volume + SLOT.tl;
		}
	}

	private void set_ar_ksr(FM_CH CH, FM_SLOT SLOT, int v)
	{
		byte old_KSR = SLOT.KSR;
		SLOT.ar = (((v & 0x1F) != 0) ? ((uint)(32 + ((v & 0x1F) << 1))) : 0u);
		SLOT.KSR = (byte)(3 - (v >> 6));
		if (SLOT.KSR != old_KSR)
		{
			CH.SLOT[0].Incr = -1L;
		}
		if (SLOT.ar + SLOT.ksr < 94)
		{
			SLOT.eg_sh_ar = eg_rate_shift[SLOT.ar + SLOT.ksr];
			SLOT.eg_sel_ar = eg_rate_select[SLOT.ar + SLOT.ksr];
		}
		else
		{
			SLOT.eg_sh_ar = 0;
			SLOT.eg_sel_ar = 144;
		}
	}

	private void set_dr(FM_SLOT SLOT, int v)
	{
		SLOT.d1r = (((v & 0x1F) != 0) ? ((uint)(32 + ((v & 0x1F) << 1))) : 0u);
		SLOT.eg_sh_d1r = eg_rate_shift[SLOT.d1r + SLOT.ksr];
		SLOT.eg_sel_d1r = eg_rate_select[SLOT.d1r + SLOT.ksr];
	}

	private void set_sr(FM_SLOT SLOT, int v)
	{
		SLOT.d2r = (((v & 0x1F) != 0) ? ((uint)(32 + ((v & 0x1F) << 1))) : 0u);
		SLOT.eg_sh_d2r = eg_rate_shift[SLOT.d2r + SLOT.ksr];
		SLOT.eg_sel_d2r = eg_rate_select[SLOT.d2r + SLOT.ksr];
	}

	private void set_sl_rr(FM_SLOT SLOT, int v)
	{
		SLOT.sl = sl_table[v >> 4];
		if (SLOT.state == 3 && SLOT.volume >= (int)SLOT.sl)
		{
			SLOT.state = 2;
		}
		SLOT.rr = (uint)(34 + ((v & 0xF) << 2));
		SLOT.eg_sh_rr = eg_rate_shift[SLOT.rr + SLOT.ksr];
		SLOT.eg_sel_rr = eg_rate_select[SLOT.rr + SLOT.ksr];
	}

	private void advance_lfo()
	{
		if (ym2612.OPN.lfo_timer_overflow == 0)
		{
			return;
		}
		ym2612.OPN.lfo_timer += ym2612.OPN.lfo_timer_add;
		while (ym2612.OPN.lfo_timer >= ym2612.OPN.lfo_timer_overflow)
		{
			ym2612.OPN.lfo_timer -= ym2612.OPN.lfo_timer_overflow;
			ym2612.OPN.lfo_cnt = (byte)((ym2612.OPN.lfo_cnt + 1) & 0x7F);
			if (ym2612.OPN.lfo_cnt < 64)
			{
				ym2612.OPN.LFO_AM = (uint)(ym2612.OPN.lfo_cnt * 2);
			}
			else
			{
				ym2612.OPN.LFO_AM = (uint)(126 - (ym2612.OPN.lfo_cnt & 0x3F) * 2);
			}
			ym2612.OPN.LFO_PM = (uint)(ym2612.OPN.lfo_cnt >> 2);
		}
	}

	private void advance_eg_channels()
	{
		uint eg_cnt = ym2612.OPN.eg_cnt;
		uint i = 0u;
		int curSlot = 0;
		do
		{
			curSlot = 0;
			FM_SLOT SLOT = ym2612.CH[i].SLOT[curSlot];
			uint j = 4u;
			do
			{
				switch (SLOT.state)
				{
				case 4:
					if ((eg_cnt & ((1 << (int)SLOT.eg_sh_ar) - 1)) == 0L)
					{
						SLOT.volume += ~SLOT.volume * eg_inc[SLOT.eg_sel_ar + ((eg_cnt >> (int)SLOT.eg_sh_ar) & 7)] >> 4;
						if (SLOT.volume <= 0)
						{
							SLOT.volume = 0L;
							SLOT.state = (byte)((SLOT.sl == 0) ? 2u : 3u);
						}
						if ((SLOT.ssg & 8) != 0 && (SLOT.ssgn ^ (SLOT.ssg & 4)) != 0)
						{
							SLOT.vol_out = (uint)((int)(512 - SLOT.volume) & 0x3FF) + SLOT.tl;
						}
						else
						{
							SLOT.vol_out = (uint)(int)SLOT.volume + SLOT.tl;
						}
					}
					break;
				case 3:
					if ((eg_cnt & ((1 << (int)SLOT.eg_sh_d1r) - 1)) != 0L)
					{
						break;
					}
					if ((SLOT.ssg & 8) != 0)
					{
						if (SLOT.volume < 512)
						{
							SLOT.volume += 4 * eg_inc[SLOT.eg_sel_d1r + ((eg_cnt >> (int)SLOT.eg_sh_d1r) & 7)];
							if ((SLOT.ssgn ^ (SLOT.ssg & 4)) != 0)
							{
								SLOT.vol_out = (uint)((int)(512 - SLOT.volume) & 0x3FF) + SLOT.tl;
							}
							else
							{
								SLOT.vol_out = (uint)(int)SLOT.volume + SLOT.tl;
							}
						}
					}
					else
					{
						SLOT.volume += eg_inc[SLOT.eg_sel_d1r + ((eg_cnt >> (int)SLOT.eg_sh_d1r) & 7)];
						SLOT.vol_out = (uint)(int)SLOT.volume + SLOT.tl;
					}
					if (SLOT.volume >= (int)SLOT.sl)
					{
						SLOT.state = 2;
					}
					break;
				case 2:
					if ((eg_cnt & ((1 << (int)SLOT.eg_sh_d2r) - 1)) != 0L)
					{
						break;
					}
					if ((SLOT.ssg & 8) != 0)
					{
						if (SLOT.volume < 512)
						{
							SLOT.volume += 4 * eg_inc[SLOT.eg_sel_d2r + ((eg_cnt >> (int)SLOT.eg_sh_d2r) & 7)];
							if ((SLOT.ssgn ^ (SLOT.ssg & 4)) != 0)
							{
								SLOT.vol_out = (uint)((int)(512 - SLOT.volume) & 0x3FF) + SLOT.tl;
							}
							else
							{
								SLOT.vol_out = (uint)(int)SLOT.volume + SLOT.tl;
							}
						}
					}
					else
					{
						SLOT.volume += eg_inc[SLOT.eg_sel_d2r + ((eg_cnt >> (int)SLOT.eg_sh_d2r) & 7)];
						if (SLOT.volume >= 1023)
						{
							SLOT.volume = 1023L;
						}
						SLOT.vol_out = (uint)(int)SLOT.volume + SLOT.tl;
					}
					break;
				case 1:
					if ((eg_cnt & ((1 << (int)SLOT.eg_sh_rr) - 1)) != 0L)
					{
						break;
					}
					if ((SLOT.ssg & 8) != 0)
					{
						if (SLOT.volume < 512)
						{
							SLOT.volume += 4 * eg_inc[SLOT.eg_sel_rr + ((eg_cnt >> (int)SLOT.eg_sh_rr) & 7)];
						}
						if (SLOT.volume >= 512)
						{
							SLOT.volume = 1023L;
							SLOT.state = 0;
						}
					}
					else
					{
						SLOT.volume += eg_inc[SLOT.eg_sel_rr + ((eg_cnt >> (int)SLOT.eg_sh_rr) & 7)];
						if (SLOT.volume >= 1023)
						{
							SLOT.volume = 1023L;
							SLOT.state = 0;
						}
					}
					SLOT.vol_out = (uint)(int)SLOT.volume + SLOT.tl;
					break;
				}
				curSlot++;
				if (curSlot < ym2612.CH[i].SLOT.Count())
				{
					SLOT = ym2612.CH[i].SLOT[curSlot];
				}
				j--;
			}
			while (j != 0);
			i++;
		}
		while (i < 6);
	}

	private void update_ssg_eg_channel(FM_SLOT[] SLOTS)
	{
		uint i = 4u;
		int curSlot = 0;
		FM_SLOT SLOT = SLOTS[curSlot];
		do
		{
			if ((SLOT.ssg & 8) != 0 && SLOT.volume >= 512 && SLOT.state > 1)
			{
				if ((SLOT.ssg & 1) != 0)
				{
					if ((SLOT.ssg & 2) != 0)
					{
						SLOT.ssgn = 4;
					}
					if (SLOT.state != 4 && (SLOT.ssgn ^ (SLOT.ssg & 4)) == 0)
					{
						SLOT.volume = 1023L;
					}
				}
				else
				{
					if ((SLOT.ssg & 2) != 0)
					{
						SLOT.ssgn ^= 4;
					}
					else
					{
						SLOT.phase = 0u;
					}
					if (SLOT.state != 4)
					{
						if (SLOT.ar + SLOT.ksr < 94)
						{
							SLOT.state = (byte)((SLOT.volume > 0) ? 4u : ((SLOT.sl == 0) ? 2u : 3u));
						}
						else
						{
							SLOT.volume = 0L;
							SLOT.state = (byte)((SLOT.sl == 0) ? 2u : 3u);
						}
					}
				}
				if ((SLOT.ssgn ^ (SLOT.ssg & 4)) != 0)
				{
					SLOT.vol_out = (uint)((int)(512 - SLOT.volume) & 0x3FF) + SLOT.tl;
				}
				else
				{
					SLOT.vol_out = (uint)(int)SLOT.volume + SLOT.tl;
				}
			}
			curSlot++;
			if (curSlot < SLOTS.Count())
			{
				SLOT = SLOTS[curSlot];
			}
			i--;
		}
		while (i != 0);
	}

	private void update_phase_lfo_slot(FM_SLOT SLOT, long pms, uint block_fnum)
	{
		long lfo_fn_table_index_offset = lfo_pm_table[((block_fnum & 0x7F0) >> 4 << 8) + pms + ym2612.OPN.LFO_PM];
		if (lfo_fn_table_index_offset != 0L)
		{
			block_fnum = (uint)(block_fnum * 2 + lfo_fn_table_index_offset);
			byte blk = (byte)((block_fnum & 0x7000) >> 12);
			block_fnum &= 0xFFF;
			int kc = (blk << 2) | opn_fktable[block_fnum >> 8];
			int fc = (int)((ym2612.OPN.fn_table[block_fnum] >> 7 - blk) + SLOT.DT.value[kc]);
			if (fc < 0)
			{
				fc += (int)ym2612.OPN.fn_max;
			}
			SLOT.phase += (uint)(int)(fc * SLOT.mul >> 1);
		}
		else
		{
			SLOT.phase += (uint)(int)SLOT.Incr;
		}
	}

	private void update_phase_lfo_channel(FM_CH CH)
	{
		uint block_fnum = CH.block_fnum;
		long lfo_fn_table_index_offset = lfo_pm_table[((block_fnum & 0x7F0) >> 4 << 8) + CH.pms + ym2612.OPN.LFO_PM];
		if (lfo_fn_table_index_offset != 0L)
		{
			block_fnum = (uint)(block_fnum * 2 + lfo_fn_table_index_offset);
			byte blk = (byte)((block_fnum & 0x7000) >> 12);
			block_fnum &= 0xFFF;
			int kc = (blk << 2) | opn_fktable[block_fnum >> 8];
			uint num = ym2612.OPN.fn_table[block_fnum] >> 7 - blk;
			int finc = (int)((int)num + CH.SLOT[0].DT.value[kc]);
			if (finc < 0)
			{
				finc += (int)ym2612.OPN.fn_max;
			}
			CH.SLOT[0].phase += (uint)(int)(finc * CH.SLOT[0].mul >> 1);
			finc = (int)((int)num + CH.SLOT[2].DT.value[kc]);
			if (finc < 0)
			{
				finc += (int)ym2612.OPN.fn_max;
			}
			CH.SLOT[2].phase += (uint)(int)(finc * CH.SLOT[2].mul >> 1);
			finc = (int)((int)num + CH.SLOT[1].DT.value[kc]);
			if (finc < 0)
			{
				finc += (int)ym2612.OPN.fn_max;
			}
			CH.SLOT[1].phase += (uint)(int)(finc * CH.SLOT[1].mul >> 1);
			finc = (int)((int)num + CH.SLOT[3].DT.value[kc]);
			if (finc < 0)
			{
				finc += (int)ym2612.OPN.fn_max;
			}
			CH.SLOT[3].phase += (uint)(int)(finc * CH.SLOT[3].mul >> 1);
		}
		else
		{
			CH.SLOT[0].phase += (uint)(int)CH.SLOT[0].Incr;
			CH.SLOT[2].phase += (uint)(int)CH.SLOT[2].Incr;
			CH.SLOT[1].phase += (uint)(int)CH.SLOT[1].Incr;
			CH.SLOT[3].phase += (uint)(int)CH.SLOT[3].Incr;
		}
	}

	private void refresh_fc_eg_slot(FM_SLOT SLOT, int fc, int kc)
	{
		fc += (int)SLOT.DT.value[kc];
		if (fc < 0)
		{
			fc += (int)ym2612.OPN.fn_max;
		}
		SLOT.Incr = fc * SLOT.mul >> 1;
		kc >>= (int)SLOT.KSR;
		if (SLOT.ksr != kc)
		{
			SLOT.ksr = (byte)kc;
			if (SLOT.ar + kc < 94)
			{
				SLOT.eg_sh_ar = eg_rate_shift[SLOT.ar + kc];
				SLOT.eg_sel_ar = eg_rate_select[SLOT.ar + kc];
			}
			else
			{
				SLOT.eg_sh_ar = 0;
				SLOT.eg_sel_ar = 144;
			}
			SLOT.eg_sh_d1r = eg_rate_shift[SLOT.d1r + kc];
			SLOT.eg_sel_d1r = eg_rate_select[SLOT.d1r + kc];
			SLOT.eg_sh_d2r = eg_rate_shift[SLOT.d2r + kc];
			SLOT.eg_sel_d2r = eg_rate_select[SLOT.d2r + kc];
			SLOT.eg_sh_rr = eg_rate_shift[SLOT.rr + kc];
			SLOT.eg_sel_rr = eg_rate_select[SLOT.rr + kc];
		}
	}

	private void refresh_fc_eg_chan(FM_CH CH)
	{
		if (CH.SLOT[0].Incr == -1)
		{
			int fc = (int)CH.fc;
			int kc = CH.kcode;
			refresh_fc_eg_slot(CH.SLOT[0], fc, kc);
			refresh_fc_eg_slot(CH.SLOT[2], fc, kc);
			refresh_fc_eg_slot(CH.SLOT[1], fc, kc);
			refresh_fc_eg_slot(CH.SLOT[3], fc, kc);
		}
	}

	private uint volume_calc(FM_SLOT slot, uint AM)
	{
		return slot.vol_out + (AM & slot.AMmask);
	}

	private int op_calc(uint phase, uint env, int pm)
	{
		uint p = (env << 3) + sin_tab[((int)((phase & -65536) + (pm << 15)) >> 16) & 0x3FF];
		if (p >= 6656)
		{
			return 0;
		}
		return tl_tab[p];
	}

	private int op_calc1(uint phase, uint env, int pm)
	{
		uint p = (env << 3) + sin_tab[((int)((phase & -65536) + pm) >> 16) & 0x3FF];
		if (p >= 6656)
		{
			return 0;
		}
		return tl_tab[p];
	}

	private void chan_calc(FM_CH CH)
	{
		uint AM = ym2612.OPN.LFO_AM >> (int)CH.ams;
		uint eg_out = volume_calc(CH.SLOT[0], AM);
		m2.value = (c1.value = (c2.value = (mem.value = 0L)));
		CH.mem_connect.value = CH.mem_value;
		long outVal = CH.op1_out[0] + CH.op1_out[1];
		CH.op1_out[0] = CH.op1_out[1];
		if (CH.connect1 == null)
		{
			mem.value = (c1.value = (c2.value = CH.op1_out[0]));
		}
		else
		{
			CH.connect1.value += CH.op1_out[0];
		}
		CH.op1_out[1] = 0L;
		if (eg_out < 832)
		{
			if (CH.FB == 0)
			{
				outVal = 0L;
			}
			CH.op1_out[1] = op_calc1(CH.SLOT[0].phase, eg_out, (int)(outVal << (int)CH.FB));
		}
		eg_out = volume_calc(CH.SLOT[1], AM);
		if (eg_out < 832)
		{
			CH.connect3.value += op_calc(CH.SLOT[1].phase, eg_out, (int)m2.value);
		}
		eg_out = volume_calc(CH.SLOT[2], AM);
		if (eg_out < 832)
		{
			CH.connect2.value += op_calc(CH.SLOT[2].phase, eg_out, (int)c1.value);
		}
		eg_out = volume_calc(CH.SLOT[3], AM);
		if (eg_out < 832)
		{
			CH.connect4.value += op_calc(CH.SLOT[3].phase, eg_out, (int)c2.value);
		}
		CH.mem_value = mem.value;
		if (CH.pms != 0L)
		{
			if ((ym2612.OPN.ST.mode & 0xC0) != 0 && CH == ym2612.CH[2])
			{
				update_phase_lfo_slot(CH.SLOT[0], CH.pms, ym2612.OPN.SL3.block_fnum[1]);
				update_phase_lfo_slot(CH.SLOT[2], CH.pms, ym2612.OPN.SL3.block_fnum[2]);
				update_phase_lfo_slot(CH.SLOT[1], CH.pms, ym2612.OPN.SL3.block_fnum[0]);
				update_phase_lfo_slot(CH.SLOT[3], CH.pms, CH.block_fnum);
			}
			else
			{
				update_phase_lfo_channel(CH);
			}
		}
		else
		{
			CH.SLOT[0].phase += (uint)(int)CH.SLOT[0].Incr;
			CH.SLOT[2].phase += (uint)(int)CH.SLOT[2].Incr;
			CH.SLOT[1].phase += (uint)(int)CH.SLOT[1].Incr;
			CH.SLOT[3].phase += (uint)(int)CH.SLOT[3].Incr;
		}
	}

	private void OPNWriteMode(int r, int v)
	{
		switch (r)
		{
		case 34:
			if ((v & 8) != 0)
			{
				if (ym2612.OPN.lfo_timer_overflow == 0)
				{
					ym2612.OPN.lfo_cnt = 0;
					ym2612.OPN.lfo_timer = 0u;
					ym2612.OPN.LFO_AM = 0u;
					ym2612.OPN.LFO_PM = 0u;
				}
				ym2612.OPN.lfo_timer_overflow = lfo_samples_per_step[v & 7] << 24;
			}
			else
			{
				ym2612.OPN.lfo_timer_overflow = 0u;
			}
			break;
		case 36:
			ym2612.OPN.ST.TA = (ym2612.OPN.ST.TA & 3) | ((long)v << 2);
			ym2612.OPN.ST.TAL = 1024 - ym2612.OPN.ST.TA << 16;
			break;
		case 37:
			ym2612.OPN.ST.TA = (ym2612.OPN.ST.TA & 0x3FC) | (long)((ulong)v & 3uL);
			ym2612.OPN.ST.TAL = 1024 - ym2612.OPN.ST.TA << 16;
			break;
		case 38:
			ym2612.OPN.ST.TB = v;
			ym2612.OPN.ST.TBL = 256 - ym2612.OPN.ST.TB << 20;
			break;
		case 39:
			set_timers(v);
			break;
		case 40:
		{
			byte c = (byte)(v & 3);
			if (c != 3)
			{
				if ((v & 4) != 0)
				{
					c += 3;
				}
				FM_CH CH = ym2612.CH[c];
				if ((v & 0x10) != 0)
				{
					FM_KEYON(CH, 0);
				}
				else
				{
					FM_KEYOFF(CH, 0);
				}
				if ((v & 0x20) != 0)
				{
					FM_KEYON(CH, 2);
				}
				else
				{
					FM_KEYOFF(CH, 2);
				}
				if ((v & 0x40) != 0)
				{
					FM_KEYON(CH, 1);
				}
				else
				{
					FM_KEYOFF(CH, 1);
				}
				if ((v & 0x80) != 0)
				{
					FM_KEYON(CH, 3);
				}
				else
				{
					FM_KEYOFF(CH, 3);
				}
			}
			break;
		}
		case 33:
		case 35:
			break;
		}
	}

	private byte OPN_CHAN(int N)
	{
		return (byte)(N & 3);
	}

	private int OPN_SLOT(int N)
	{
		return (N >> 2) & 3;
	}

	private void OPNWriteReg(int r, int v)
	{
		byte c = OPN_CHAN(r);
		if (c == 3)
		{
			return;
		}
		if (r >= 256)
		{
			c += 3;
		}
		FM_CH CH = ym2612.CH[c];
		FM_SLOT SLOT = CH.SLOT[OPN_SLOT(r)];
		switch (r & 0xF0)
		{
		case 48:
			set_det_mul(CH, SLOT, v);
			break;
		case 64:
			set_tl(SLOT, v);
			break;
		case 80:
			set_ar_ksr(CH, SLOT, v);
			break;
		case 96:
			set_dr(SLOT, v);
			SLOT.AMmask = (((v & 0x80) != 0) ? uint.MaxValue : 0u);
			break;
		case 112:
			set_sr(SLOT, v);
			break;
		case 128:
			set_sl_rr(SLOT, v);
			break;
		case 144:
			SLOT.ssg = (byte)(v & 0xF);
			if (SLOT.state > 1)
			{
				if ((SLOT.ssg & 8) != 0 && (SLOT.ssgn ^ (SLOT.ssg & 4)) != 0)
				{
					SLOT.vol_out = (uint)((int)(512 - SLOT.volume) & 0x3FF) + SLOT.tl;
				}
				else
				{
					SLOT.vol_out = (uint)(int)SLOT.volume + SLOT.tl;
				}
			}
			break;
		case 160:
			switch (OPN_SLOT(r))
			{
			case 0:
			{
				uint fn2 = (uint)((uint)((ym2612.OPN.ST.fn_h & 7) << 8) + v);
				byte blk2 = (byte)(ym2612.OPN.ST.fn_h >> 3);
				CH.kcode = (byte)((blk2 << 2) | opn_fktable[fn2 >> 7]);
				CH.fc = ym2612.OPN.fn_table[fn2 * 2] >> 7 - blk2;
				CH.block_fnum = (uint)(blk2 << 11) | fn2;
				CH.SLOT[0].Incr = -1L;
				break;
			}
			case 1:
				ym2612.OPN.ST.fn_h = (byte)(v & 0x3F);
				break;
			case 2:
				if (r < 256)
				{
					uint fn = (uint)((uint)((ym2612.OPN.SL3.fn_h & 7) << 8) + v);
					byte blk = (byte)(ym2612.OPN.SL3.fn_h >> 3);
					ym2612.OPN.SL3.kcode[c] = (byte)((blk << 2) | opn_fktable[fn >> 7]);
					ym2612.OPN.SL3.fc[c] = ym2612.OPN.fn_table[fn * 2] >> 7 - blk;
					ym2612.OPN.SL3.block_fnum[c] = (uint)(blk << 11) | fn;
					ym2612.CH[2].SLOT[0].Incr = -1L;
				}
				break;
			case 3:
				if (r < 256)
				{
					ym2612.OPN.SL3.fn_h = (byte)(v & 0x3F);
				}
				break;
			}
			break;
		case 176:
			switch (OPN_SLOT(r))
			{
			case 0:
			{
				int feedback = (v >> 3) & 7;
				CH.ALGO = (byte)(v & 7);
				CH.FB = (byte)((feedback != 0) ? ((uint)(feedback + 6)) : 0u);
				setup_connection(CH, c);
				break;
			}
			case 1:
				CH.pms = (v & 7) * 32;
				CH.ams = lfo_ams_depth_shift[(v >> 4) & 3];
				ym2612.OPN.pan[c * 2] = (((v & 0x80) != 0) ? uint.MaxValue : 0u);
				ym2612.OPN.pan[c * 2 + 1] = (((v & 0x40) != 0) ? uint.MaxValue : 0u);
				break;
			}
			break;
		}
	}

	private void init_timetables(double freqbase)
	{
		for (int d = 0; d <= 3; d++)
		{
			for (int i = 0; i <= 31; i++)
			{
				double rate = (double)(int)dt_tab[d * 32 + i] * freqbase * 64.0;
				ym2612.OPN.ST.dt_tab[d].value[i] = (int)rate;
				ym2612.OPN.ST.dt_tab[d + 4].value[i] = -ym2612.OPN.ST.dt_tab[d].value[i];
			}
		}
		for (int i = 0; i < 4096; i++)
		{
			ym2612.OPN.fn_table[i] = (uint)((double)i * 32.0 * freqbase * 64.0);
		}
		ym2612.OPN.fn_max = (uint)(131072.0 * freqbase * 64.0);
	}

	private void OPNSetPres(int pres)
	{
		double freqbase = ym2612.OPN.ST.clock / (double)ym2612.OPN.ST.rate / (double)pres;
		if (config.hq_fm != 0)
		{
			freqbase = 1.0;
		}
		ym2612.OPN.eg_timer_add = (uint)(65536.0 * freqbase);
		ym2612.OPN.eg_timer_overflow = 196608u;
		ym2612.OPN.lfo_timer_add = (uint)(16777216.0 * freqbase);
		ym2612.OPN.ST.TimerBase = (int)(65536.0 * freqbase);
		init_timetables(freqbase);
	}

	private void reset_channels(FM_CH[] CH, int num)
	{
		for (int c = 0; c < num; c++)
		{
			CH[c].mem_value = 0L;
			CH[c].op1_out[0] = 0L;
			CH[c].op1_out[1] = 0L;
			for (int s = 0; s < 4; s++)
			{
				CH[c].SLOT[s].Incr = -1L;
				CH[c].SLOT[s].key = 0;
				CH[c].SLOT[s].phase = 0u;
				CH[c].SLOT[s].ssgn = 0;
				CH[c].SLOT[s].state = 0;
				CH[c].SLOT[s].volume = 1023L;
				CH[c].SLOT[s].vol_out = 1023u;
			}
		}
	}

	private void init_tables()
	{
		uint mask = (uint)(~((1 << 14 - config.dac_bits) - 1));
		for (int x = 0; x < 256; x++)
		{
			double m = 65536.0 / Math.Pow(2.0, (double)(x + 1) * (1.0 / 32.0) / 8.0);
			m = Math.Floor(m);
			int n = (int)m;
			n >>= 4;
			n = (((n & 1) == 0) ? (n >> 1) : ((n >> 1) + 1));
			n <<= 2;
			tl_tab[x * 2] = (int)(n & mask);
			tl_tab[x * 2 + 1] = (int)(-tl_tab[x * 2] & mask);
			for (int i = 1; i < 13; i++)
			{
				tl_tab[x * 2 + i * 2 * 256] = (int)((tl_tab[x * 2] >> i) & mask);
				tl_tab[x * 2 + 1 + i * 2 * 256] = (int)(-tl_tab[x * 2 + i * 2 * 256] & mask);
			}
		}
		for (int i = 0; i < 1024; i++)
		{
			double m = Math.Sin((double)(i * 2 + 1) * Math.PI / 1024.0);
			double o = ((!(m > 0.0)) ? (8.0 * Math.Log(-1.0 / m) / Math.Log(2.0)) : (8.0 * Math.Log(1.0 / m) / Math.Log(2.0)));
			o /= 1.0 / 32.0;
			int n = (int)(2.0 * o);
			n = (((n & 1) == 0) ? (n >> 1) : ((n >> 1) + 1));
			sin_tab[i] = (uint)(n * 2) + ((!(m >= 0.0)) ? 1u : 0u);
		}
		for (int i = 0; i < 8; i++)
		{
			for (byte fnum = 0; fnum < 128; fnum++)
			{
				uint offset_depth = (uint)i;
				for (byte step = 0; step < 8; step++)
				{
					byte value = 0;
					for (uint bit_tmp = 0u; bit_tmp < 7; bit_tmp++)
					{
						if ((fnum & (1 << (int)bit_tmp)) != 0)
						{
							uint offset_fnum_bit = bit_tmp * 8;
							value += lfo_pm_output[offset_fnum_bit + offset_depth, step];
						}
					}
					lfo_pm_table[fnum * 32 * 8 + i * 32 + step] = value;
					lfo_pm_table[fnum * 32 * 8 + i * 32 + (step ^ 7) + 8] = value;
					lfo_pm_table[fnum * 32 * 8 + i * 32 + step + 16] = -value;
					lfo_pm_table[fnum * 32 * 8 + i * 32 + (step ^ 7) + 24] = -value;
				}
			}
		}
	}

	public void YM2612Init(double clock, int rate)
	{
		config.psg_preamp = 150;
		config.fm_preamp = 100;
		config.hq_fm = 0;
		config.psgBoostNoise = 0;
		config.filter = 0;
		config.lp_range = 50;
		config.low_freq = 880;
		config.high_freq = 5000;
		config.lg = 1;
		config.mg = 1;
		config.hg = 1;
		config.rolloff = 0.99f;
		config.dac_bits = 14;
		config.ym2413 = 2;
		config.system = 0;
		config.region_detect = 0;
		config.vdp_mode = 0;
		config.master_clock = 0;
		config.force_dtack = 0;
		config.addr_error = 1;
		config.bios = 0;
		config.lock_on = 0;
		config.hot_swap = 0;
		config.xshift = 0;
		config.yshift = 0;
		config.xscale = 0;
		config.yscale = 0;
		config.aspect = 1;
		config.overscan = 3;
		config.ntsc = 0;
		config.vsync = 1;
		config.render = 0;
		config.bilinear = 0;
		config.tv_mode = 1;
		config.gun_cursor[0] = 1;
		config.gun_cursor[1] = 1;
		config.invert_mouse = 0;
		config.autoload = 0;
		config.autocheat = 0;
		config.s_auto = 0;
		config.s_default = 1;
		config.s_device = 0;
		config.l_device = 0;
		config.bg_overlay = 0;
		config.screen_w = 658;
		config.bgm_volume = 100f;
		config.sfx_volume = 100f;
		config.hot_swap &= 1;
		init_tables();
		ym2612.OPN.ST.clock = clock;
		ym2612.OPN.ST.rate = (uint)rate;
		OPNSetPres(144);
	}

	public void YM2612ResetChip()
	{
		ym2612.OPN.eg_timer = 0u;
		ym2612.OPN.eg_cnt = 0u;
		ym2612.OPN.lfo_timer_overflow = 0u;
		ym2612.OPN.lfo_timer = 0u;
		ym2612.OPN.lfo_cnt = 0;
		ym2612.OPN.LFO_AM = 0u;
		ym2612.OPN.LFO_PM = 0u;
		ym2612.OPN.ST.TAC = 0L;
		ym2612.OPN.ST.TBC = 0L;
		ym2612.OPN.SL3.key_csm = 0;
		ym2612.dacen = 0;
		ym2612.dacout = 0L;
		set_timers(48);
		ym2612.OPN.ST.TB = 0L;
		ym2612.OPN.ST.TBL = 268435456L;
		ym2612.OPN.ST.TA = 0L;
		ym2612.OPN.ST.TAL = 67108864L;
		reset_channels(ym2612.CH, 6);
		for (int i = 182; i >= 180; i--)
		{
			OPNWriteReg(i, 192);
			OPNWriteReg(i | 0x100, 192);
		}
		for (int i = 178; i >= 48; i--)
		{
			OPNWriteReg(i, 0);
			OPNWriteReg(i | 0x100, 0);
		}
	}

	public void YM2612Write(uint a, uint v)
	{
		v &= 0xFF;
		switch (a)
		{
		case 0u:
			ym2612.OPN.ST.address = (ushort)v;
			return;
		case 2u:
			ym2612.OPN.ST.address = (ushort)(v | 0x100);
			return;
		}
		int addr = ym2612.OPN.ST.address;
		if ((addr & 0x1F0) == 32)
		{
			switch (addr)
			{
			case 42:
				ym2612.dacout = (int)(v - 128 << 6);
				break;
			case 43:
				ym2612.dacen = (byte)(v & 0x80);
				break;
			default:
				OPNWriteMode(addr, (int)v);
				break;
			}
		}
		else
		{
			OPNWriteReg(addr, (int)v);
		}
	}

	public uint YM2612Read()
	{
		return (uint)(ym2612.OPN.ST.status & 0xFF);
	}

	public void YM2612Update(int[] buffer, int length)
	{
		refresh_fc_eg_chan(ym2612.CH[0]);
		refresh_fc_eg_chan(ym2612.CH[1]);
		if ((ym2612.OPN.ST.mode & 0xC0) == 0)
		{
			refresh_fc_eg_chan(ym2612.CH[2]);
		}
		else if (ym2612.CH[2].SLOT[0].Incr == -1)
		{
			refresh_fc_eg_slot(ym2612.CH[2].SLOT[0], (int)ym2612.OPN.SL3.fc[1], ym2612.OPN.SL3.kcode[1]);
			refresh_fc_eg_slot(ym2612.CH[2].SLOT[2], (int)ym2612.OPN.SL3.fc[2], ym2612.OPN.SL3.kcode[2]);
			refresh_fc_eg_slot(ym2612.CH[2].SLOT[1], (int)ym2612.OPN.SL3.fc[0], ym2612.OPN.SL3.kcode[0]);
			refresh_fc_eg_slot(ym2612.CH[2].SLOT[3], (int)ym2612.CH[2].fc, ym2612.CH[2].kcode);
		}
		refresh_fc_eg_chan(ym2612.CH[3]);
		refresh_fc_eg_chan(ym2612.CH[4]);
		refresh_fc_eg_chan(ym2612.CH[5]);
		int bufferPos = 0;
		for (int i = 0; i < length; i++)
		{
			out_fm[0].value = 0L;
			out_fm[1].value = 0L;
			out_fm[2].value = 0L;
			out_fm[3].value = 0L;
			out_fm[4].value = 0L;
			out_fm[5].value = 0L;
			update_ssg_eg_channel(ym2612.CH[0].SLOT);
			update_ssg_eg_channel(ym2612.CH[1].SLOT);
			update_ssg_eg_channel(ym2612.CH[2].SLOT);
			update_ssg_eg_channel(ym2612.CH[3].SLOT);
			update_ssg_eg_channel(ym2612.CH[4].SLOT);
			update_ssg_eg_channel(ym2612.CH[5].SLOT);
			chan_calc(ym2612.CH[0]);
			chan_calc(ym2612.CH[1]);
			chan_calc(ym2612.CH[2]);
			chan_calc(ym2612.CH[3]);
			chan_calc(ym2612.CH[4]);
			if (ym2612.dacen == 0)
			{
				chan_calc(ym2612.CH[5]);
			}
			else
			{
				out_fm[5].value = ym2612.dacout;
			}
			advance_lfo();
			ym2612.OPN.eg_timer += ym2612.OPN.eg_timer_add;
			while (ym2612.OPN.eg_timer >= ym2612.OPN.eg_timer_overflow)
			{
				ym2612.OPN.eg_timer -= ym2612.OPN.eg_timer_overflow;
				ym2612.OPN.eg_cnt++;
				advance_eg_channels();
			}
			if (out_fm[0].value > 8192)
			{
				out_fm[0].value = 8192L;
			}
			else if (out_fm[0].value < -8192)
			{
				out_fm[0].value = -8192L;
			}
			if (out_fm[1].value > 8192)
			{
				out_fm[1].value = 8192L;
			}
			else if (out_fm[1].value < -8192)
			{
				out_fm[1].value = -8192L;
			}
			if (out_fm[2].value > 8192)
			{
				out_fm[2].value = 8192L;
			}
			else if (out_fm[2].value < -8192)
			{
				out_fm[2].value = -8192L;
			}
			if (out_fm[3].value > 8192)
			{
				out_fm[3].value = 8192L;
			}
			else if (out_fm[3].value < -8192)
			{
				out_fm[3].value = -8192L;
			}
			if (out_fm[4].value > 8192)
			{
				out_fm[4].value = 8192L;
			}
			else if (out_fm[4].value < -8192)
			{
				out_fm[4].value = -8192L;
			}
			if (out_fm[5].value > 8192)
			{
				out_fm[5].value = 8192L;
			}
			else if (out_fm[5].value < -8192)
			{
				out_fm[5].value = -8192L;
			}
			int lt = (int)(out_fm[0].value & ym2612.OPN.pan[0]);
			int rt = (int)(out_fm[0].value & ym2612.OPN.pan[1]);
			lt += (int)(out_fm[1].value & ym2612.OPN.pan[2]);
			rt += (int)(out_fm[1].value & ym2612.OPN.pan[3]);
			lt += (int)(out_fm[2].value & ym2612.OPN.pan[4]);
			rt += (int)(out_fm[2].value & ym2612.OPN.pan[5]);
			lt += (int)(out_fm[3].value & ym2612.OPN.pan[6]);
			rt += (int)(out_fm[3].value & ym2612.OPN.pan[7]);
			lt += (int)(out_fm[4].value & ym2612.OPN.pan[8]);
			rt += (int)(out_fm[4].value & ym2612.OPN.pan[9]);
			lt += (int)(out_fm[5].value & ym2612.OPN.pan[10]);
			rt += (int)(out_fm[5].value & ym2612.OPN.pan[11]);
			buffer[bufferPos] = lt;
			bufferPos++;
			buffer[bufferPos] = rt;
			bufferPos++;
			ym2612.OPN.SL3.key_csm <<= 1;
			INTERNAL_TIMER_A();
			if ((ym2612.OPN.SL3.key_csm & 2) != 0)
			{
				FM_KEYOFF_CSM(ym2612.CH[2], 0);
				FM_KEYOFF_CSM(ym2612.CH[2], 2);
				FM_KEYOFF_CSM(ym2612.CH[2], 1);
				FM_KEYOFF_CSM(ym2612.CH[2], 3);
				ym2612.OPN.SL3.key_csm = 0;
			}
		}
		INTERNAL_TIMER_B(length);
	}
}
