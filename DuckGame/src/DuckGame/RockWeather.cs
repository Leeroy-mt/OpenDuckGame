using System;
using System.Collections.Generic;

namespace DuckGame;

public class RockWeather : Thing
{
	private static Weather _weather = Weather.Sunny;

	private Color skyColor = new Color(0.54509807f, 0.8f, 0.972549f);

	private Vec3 winterColor = new Vec3(-0.1f, -0.1f, 0.2f);

	private Vec3 summerColor = new Vec3(0f, 0f, 0f);

	private List<WeatherParticle> _particles = new List<WeatherParticle>();

	private float _particleWait;

	private RockScoreboard _board;

	private Color _skyColor;

	private Vec3 _enviroColor;

	private static float _timeOfDay = 0.25f;

	private static float _weatherTime = 1f;

	private List<RockWeatherState> timeOfDayColorMultMap = new List<RockWeatherState>
	{
		new RockWeatherState
		{
			add = new Vec3(0f, 0f, 0.06f),
			multiply = new Vec3(0.98f, 0.98f, 1f),
			sky = new Vec3(0.15f, 0.15f, 0.25f),
			sunPos = new Vec2(0f, 0.6f),
			lightOpacity = 1f,
			sunGlow = 0f,
			sunOpacity = 0f,
			rainbowLight = 0f
		},
		new RockWeatherState
		{
			add = new Vec3(0.1f, 0.1f, 0.06f),
			multiply = new Vec3(0.9f, 0.9f, 1f),
			sky = new Vec3(0.1f, 0.1f, 0.2f),
			sunPos = new Vec2(0f, 0.6f),
			lightOpacity = 1f,
			sunGlow = 0f,
			sunOpacity = 1f,
			rainbowLight = 0f
		},
		new RockWeatherState
		{
			add = new Vec3(0.1f, 0.1f, 0.06f),
			multiply = new Vec3(0.9f, 0.9f, 1f),
			sky = new Vec3(0.1f, 0.1f, 0.2f),
			sunPos = new Vec2(0f, 0.6f),
			lightOpacity = 1f,
			sunGlow = 0f,
			sunOpacity = 1f,
			rainbowLight = 0f
		},
		new RockWeatherState
		{
			add = new Vec3(0.1f, 0.1f, 0.06f),
			multiply = new Vec3(0.9f, 0.9f, 1f),
			sky = new Vec3(0.1f, 0.1f, 0.2f),
			sunPos = new Vec2(0f, 0.6f),
			lightOpacity = 1f,
			sunGlow = 0f,
			sunOpacity = 1f,
			rainbowLight = 0f
		},
		new RockWeatherState
		{
			add = new Vec3(0.1f, 0.1f, 0.06f),
			multiply = new Vec3(0.9f, 0.9f, 1f),
			sky = new Vec3(0.1f, 0.1f, 0.2f),
			sunPos = new Vec2(0f, 0.6f),
			lightOpacity = 1f,
			sunGlow = 0f,
			sunOpacity = 1f,
			rainbowLight = 0f
		},
		new RockWeatherState
		{
			add = new Vec3(0.08f, 0.05f, 0f),
			multiply = new Vec3(1f, 0.8f, 0.7f),
			sky = new Vec3(50f / 51f, 38f / 51f, 28f / 51f),
			sunPos = new Vec2(0f, 1f),
			lightOpacity = 0f,
			sunGlow = 0f,
			sunOpacity = 1f,
			rainbowLight = 0.25f,
			rainbowLight2 = 0.35f
		},
		new RockWeatherState
		{
			add = new Vec3(0f, 0f, 0f),
			multiply = new Vec3(1f, 1f, 1f),
			sky = new Vec3(0.54509807f, 0.8f, 0.972549f),
			sunPos = new Vec2(0.3f, 1.8f),
			lightOpacity = 0f,
			sunGlow = 0f,
			sunOpacity = 1f,
			rainbowLight = 0.25f,
			rainbowLight2 = 0.35f
		},
		new RockWeatherState
		{
			add = new Vec3(0f, 0f, 0f),
			multiply = new Vec3(1f, 1f, 1f),
			sky = new Vec3(0.54509807f, 0.8f, 0.972549f),
			sunPos = new Vec2(0.5f, 0.9f),
			lightOpacity = 0f,
			sunGlow = 0f,
			sunOpacity = 1f,
			rainbowLight = 0.2f,
			rainbowLight2 = 0.35f
		},
		new RockWeatherState
		{
			add = new Vec3(0f, 0f, 0f),
			multiply = new Vec3(1f, 1f, 1f),
			sky = new Vec3(0.54509807f, 0.8f, 0.972549f),
			sunPos = new Vec2(0.5f, 0.9f),
			lightOpacity = 0f,
			sunGlow = 0f,
			sunOpacity = 1f,
			rainbowLight = 0.2f,
			rainbowLight2 = 0.3f
		},
		new RockWeatherState
		{
			add = new Vec3(0f, 0f, 0f),
			multiply = new Vec3(1f, 1f, 1f),
			sky = new Vec3(0.54509807f, 0.8f, 0.972549f),
			sunPos = new Vec2(0.5f, 0.9f),
			lightOpacity = 0f,
			sunGlow = 0f,
			sunOpacity = 1f,
			rainbowLight = 0.25f,
			rainbowLight2 = 0.35f
		},
		new RockWeatherState
		{
			add = new Vec3(0.08f, 0.05f, 0f),
			multiply = new Vec3(1f, 0.8f, 0.7f),
			sky = new Vec3(40f / 51f, 0.47058824f, 38f / 51f),
			sunPos = new Vec2(0.6f, 0f),
			lightOpacity = 0f,
			sunGlow = 0f,
			sunOpacity = 1f,
			rainbowLight = 0.35f,
			rainbowLight2 = 0.35f
		},
		new RockWeatherState
		{
			add = new Vec3(0.08f, 0.05f, 0f),
			multiply = new Vec3(1f, 0.8f, 0.7f),
			sky = new Vec3(50f / 51f, 2f / 3f, 22f / 51f),
			sunPos = new Vec2(0.7f, -0.5f),
			lightOpacity = 0f,
			sunGlow = 0.3f,
			sunOpacity = 1f,
			rainbowLight = 0.15f,
			rainbowLight2 = 0.15f
		},
		new RockWeatherState
		{
			add = new Vec3(0f, 0f, 0f),
			multiply = new Vec3(1f, 1f, 1f),
			sky = new Vec3(0.1f, 0.1f, 0.2f),
			sunPos = new Vec2(0.8f, -1f),
			lightOpacity = 1f,
			sunGlow = 0f,
			sunOpacity = 1f,
			rainbowLight = 0f
		},
		new RockWeatherState
		{
			add = new Vec3(0f, 0f, 0.06f),
			multiply = new Vec3(0.98f, 0.98f, 1f),
			sky = new Vec3(0.15f, 0.15f, 0.25f),
			sunPos = new Vec2(0.9f, -1.2f),
			lightOpacity = 1f,
			sunGlow = -0.2f,
			sunOpacity = 0f,
			rainbowLight = 0f
		}
	};

	private List<RockWeatherState> timeOfDayColorMultMapWinter = new List<RockWeatherState>
	{
		new RockWeatherState
		{
			add = new Vec3(0f, 0f, 0.06f) + new Vec3(-0.1f, -0.1f, 0.2f),
			multiply = new Vec3(0.98f, 0.98f, 1f),
			sky = new Vec3(0.15f, 0.15f, 0.25f),
			sunPos = new Vec2(0f, 0.6f),
			lightOpacity = 1f,
			sunGlow = 0f,
			sunOpacity = 0f
		},
		new RockWeatherState
		{
			add = new Vec3(0.1f, 0.1f, 0.06f) + new Vec3(-0.1f, -0.1f, 0.2f),
			multiply = new Vec3(0.9f, 0.9f, 1f),
			sky = new Vec3(0.1f, 0.1f, 0.2f),
			sunPos = new Vec2(0f, 0.6f),
			lightOpacity = 1f,
			sunGlow = 0f,
			sunOpacity = 1f
		},
		new RockWeatherState
		{
			add = new Vec3(0.1f, 0.1f, 0.06f) + new Vec3(-0.1f, -0.1f, 0.2f),
			multiply = new Vec3(0.9f, 0.9f, 1f),
			sky = new Vec3(0.1f, 0.1f, 0.2f),
			sunPos = new Vec2(0f, 0.6f),
			lightOpacity = 1f,
			sunGlow = 0f,
			sunOpacity = 1f
		},
		new RockWeatherState
		{
			add = new Vec3(0.1f, 0.1f, 0.06f) + new Vec3(-0.1f, -0.1f, 0.2f),
			multiply = new Vec3(0.9f, 0.9f, 1f),
			sky = new Vec3(0.1f, 0.1f, 0.2f),
			sunPos = new Vec2(0f, 0.6f),
			lightOpacity = 1f,
			sunGlow = 0f,
			sunOpacity = 1f
		},
		new RockWeatherState
		{
			add = new Vec3(0.1f, 0.1f, 0.06f) + new Vec3(-0.1f, -0.1f, 0.2f),
			multiply = new Vec3(0.9f, 0.9f, 1f),
			sky = new Vec3(0.1f, 0.1f, 0.2f),
			sunPos = new Vec2(0f, 0.6f),
			lightOpacity = 1f,
			sunGlow = 0f,
			sunOpacity = 1f
		},
		new RockWeatherState
		{
			add = new Vec3(0.08f, 0.05f, 0f) + new Vec3(-0.1f, -0.1f, 0.2f),
			multiply = new Vec3(1f, 0.8f, 0.7f),
			sky = new Vec3(50f / 51f, 38f / 51f, 28f / 51f),
			sunPos = new Vec2(0f, 1f),
			lightOpacity = 0f,
			sunGlow = 0f,
			sunOpacity = 1f,
			rainbowLight = 0.25f,
			rainbowLight2 = 0.35f
		},
		new RockWeatherState
		{
			add = new Vec3(0f, 0f, 0f) + new Vec3(-0.1f, -0.1f, 0.2f),
			multiply = new Vec3(1f, 1f, 1f),
			sky = new Vec3(0.54509807f, 0.8f, 0.972549f),
			sunPos = new Vec2(0.3f, 1.8f),
			lightOpacity = 0f,
			sunGlow = 0f,
			sunOpacity = 1f,
			rainbowLight = 0.25f,
			rainbowLight2 = 0.35f
		},
		new RockWeatherState
		{
			add = new Vec3(0f, 0f, 0f) + new Vec3(-0.1f, -0.1f, 0.2f),
			multiply = new Vec3(1f, 1f, 1f),
			sky = new Vec3(0.54509807f, 0.8f, 0.972549f),
			sunPos = new Vec2(0.5f, 0.9f),
			lightOpacity = 0f,
			sunGlow = 0f,
			sunOpacity = 1f,
			rainbowLight = 0.2f,
			rainbowLight2 = 0.35f
		},
		new RockWeatherState
		{
			add = new Vec3(0f, 0f, 0f) + new Vec3(-0.1f, -0.1f, 0.2f),
			multiply = new Vec3(1f, 1f, 1f),
			sky = new Vec3(0.54509807f, 0.8f, 0.972549f),
			sunPos = new Vec2(0.5f, 0.9f),
			lightOpacity = 0f,
			sunGlow = 0f,
			sunOpacity = 1f,
			rainbowLight = 0.2f,
			rainbowLight2 = 0.3f
		},
		new RockWeatherState
		{
			add = new Vec3(0f, 0f, 0f) + new Vec3(-0.1f, -0.1f, 0.2f),
			multiply = new Vec3(1f, 1f, 1f),
			sky = new Vec3(0.54509807f, 0.8f, 0.972549f),
			sunPos = new Vec2(0.5f, 0.9f),
			lightOpacity = 0f,
			sunGlow = 0f,
			sunOpacity = 1f,
			rainbowLight = 0.25f,
			rainbowLight2 = 0.35f
		},
		new RockWeatherState
		{
			add = new Vec3(0.08f, 0.05f, 0f) + new Vec3(-0.1f, -0.1f, 0.2f),
			multiply = new Vec3(1f, 0.8f, 0.7f),
			sky = new Vec3(40f / 51f, 0.47058824f, 38f / 51f),
			sunPos = new Vec2(0.6f, 0f),
			lightOpacity = 0f,
			sunGlow = 0f,
			sunOpacity = 1f,
			rainbowLight = 0.35f,
			rainbowLight2 = 0.35f
		},
		new RockWeatherState
		{
			add = new Vec3(0.08f, 0.05f, 0f) + new Vec3(-0.1f, -0.1f, 0.2f),
			multiply = new Vec3(1f, 0.8f, 0.7f),
			sky = new Vec3(50f / 51f, 2f / 3f, 22f / 51f),
			sunPos = new Vec2(0.7f, -0.5f),
			lightOpacity = 0f,
			sunGlow = 0.3f,
			sunOpacity = 1f,
			rainbowLight = 0.15f,
			rainbowLight2 = 0.15f
		},
		new RockWeatherState
		{
			add = new Vec3(0f, 0f, 0f) + new Vec3(-0.1f, -0.1f, 0.2f),
			multiply = new Vec3(1f, 1f, 1f),
			sky = new Vec3(0.1f, 0.1f, 0.2f),
			sunPos = new Vec2(0.8f, -1f),
			lightOpacity = 1f,
			sunGlow = 0f,
			sunOpacity = 1f
		},
		new RockWeatherState
		{
			add = new Vec3(0f, 0f, 0.06f) + new Vec3(-0.1f, -0.1f, 0.2f),
			multiply = new Vec3(0.98f, 0.98f, 1f),
			sky = new Vec3(0.15f, 0.15f, 0.25f),
			sunPos = new Vec2(0.9f, -1.2f),
			lightOpacity = 1f,
			sunGlow = -0.2f,
			sunOpacity = 0f
		}
	};

	private List<RockWeatherState> timeOfDayColorMultMapRaining = new List<RockWeatherState>
	{
		new RockWeatherState
		{
			add = new Vec3(0f, 0f, 0.06f) + new Vec3(-0.1f, -0.1f, 0.2f),
			multiply = new Vec3(0.91f, 0.99f, 0.94f),
			sky = new Vec3(0.15f, 0.15f, 0.25f),
			sunPos = new Vec2(0f, 0.6f),
			lightOpacity = 1f,
			sunGlow = 0f,
			sunOpacity = 0f
		},
		new RockWeatherState
		{
			add = new Vec3(0.1f, 0.1f, 0.06f) + new Vec3(-0.1f, -0.1f, 0.2f),
			multiply = new Vec3(0.91f, 0.99f, 0.94f),
			sky = new Vec3(0.1f, 0.1f, 0.2f),
			sunPos = new Vec2(0f, 0.6f),
			lightOpacity = 1f,
			sunGlow = 0f,
			sunOpacity = 1f
		},
		new RockWeatherState
		{
			add = new Vec3(0.1f, 0.1f, 0.06f) + new Vec3(-0.1f, -0.1f, 0.2f),
			multiply = new Vec3(0.91f, 0.99f, 0.94f),
			sky = new Vec3(0.1f, 0.1f, 0.2f),
			sunPos = new Vec2(0f, 0.6f),
			lightOpacity = 1f,
			sunGlow = 0f,
			sunOpacity = 1f
		},
		new RockWeatherState
		{
			add = new Vec3(0.1f, 0.1f, 0.06f) + new Vec3(-0.1f, -0.1f, 0.2f),
			multiply = new Vec3(0.91f, 0.99f, 0.94f),
			sky = new Vec3(0.1f, 0.1f, 0.2f),
			sunPos = new Vec2(0f, 0.6f),
			lightOpacity = 1f,
			sunGlow = 0f,
			sunOpacity = 1f
		},
		new RockWeatherState
		{
			add = new Vec3(0.1f, 0.1f, 0.06f) + new Vec3(-0.1f, -0.1f, 0.2f),
			multiply = new Vec3(0.9f, 0.9f, 1f),
			sky = new Vec3(0.1f, 0.1f, 0.2f),
			sunPos = new Vec2(0f, 0.6f),
			lightOpacity = 1f,
			sunGlow = 0f,
			sunOpacity = 1f
		},
		new RockWeatherState
		{
			add = new Vec3(0.08f, 0.05f, 0f) + new Vec3(-0.15f, -0.1f, 0.1f),
			multiply = new Vec3(1f, 0.85f, 0.7f),
			sky = new Vec3(44f / 51f, 2f / 3f, 28f / 51f),
			sunPos = new Vec2(0f, 1f),
			lightOpacity = 0f,
			sunGlow = 0f,
			sunOpacity = 0.5f,
			rainbowLight = 0.25f,
			rainbowLight2 = 0.35f
		},
		new RockWeatherState
		{
			add = new Vec3(0f, 0f, 0f) + new Vec3(-0.15f, -0.1f, 0.1f),
			multiply = new Vec3(0.89f, 1.05f, 1f),
			sky = new Vec3(31f / 85f, 48f / 85f, 31f / 51f),
			sunPos = new Vec2(0.3f, 1.8f),
			lightOpacity = 0f,
			sunGlow = 0f,
			sunOpacity = 0.4f,
			rainbowLight = 0.25f,
			rainbowLight2 = 0.35f
		},
		new RockWeatherState
		{
			add = new Vec3(0f, 0f, 0f) + new Vec3(-0.19f, -0.09f, 0.1f),
			multiply = new Vec3(0.89f, 1.05f, 1f),
			sky = new Vec3(31f / 85f, 48f / 85f, 31f / 51f),
			sunPos = new Vec2(0.5f, 0.9f),
			lightOpacity = 0f,
			sunGlow = 0f,
			sunOpacity = 0.4f,
			rainbowLight = 0.2f,
			rainbowLight2 = 0.35f
		},
		new RockWeatherState
		{
			add = new Vec3(0f, 0f, 0f) + new Vec3(-0.2f, -0.1f, 0.07f),
			multiply = new Vec3(0.89f, 1.05f, 1f),
			sky = new Vec3(31f / 85f, 48f / 85f, 31f / 51f),
			sunPos = new Vec2(0.5f, 0.9f),
			lightOpacity = 0f,
			sunGlow = 0f,
			sunOpacity = 0.4f,
			rainbowLight = 0.2f,
			rainbowLight2 = 0.3f
		},
		new RockWeatherState
		{
			add = new Vec3(0f, 0f, 0f) + new Vec3(-0.22f, -0.12f, 0f),
			multiply = new Vec3(0.86f, 1.05f, 1f),
			sky = new Vec3(0.3254902f, 38f / 85f, 0.5137255f),
			sunPos = new Vec2(0.5f, 0.9f),
			lightOpacity = 0f,
			sunGlow = 0f,
			sunOpacity = 0.4f,
			rainbowLight = 0.25f,
			rainbowLight2 = 0.35f
		},
		new RockWeatherState
		{
			add = new Vec3(0.08f, 0.05f, 0f) + new Vec3(-0.1f, -0.1f, 0.2f),
			multiply = new Vec3(1f, 0.8f, 0.8f),
			sky = new Vec3(0.5882353f, 20f / 51f, 32f / 51f),
			sunPos = new Vec2(0.6f, 0f),
			lightOpacity = 0f,
			sunGlow = 0f,
			sunOpacity = 0.4f,
			rainbowLight = 0.35f,
			rainbowLight2 = 0.35f
		},
		new RockWeatherState
		{
			add = new Vec3(0.08f, 0.05f, 0f) + new Vec3(-0.1f, -0.1f, 0.2f),
			multiply = new Vec3(1f, 0.8f, 0.7f),
			sky = new Vec3(0.7058824f, 28f / 51f, 0.7058824f),
			sunPos = new Vec2(0.7f, -0.5f),
			lightOpacity = 0f,
			sunGlow = 0.3f,
			sunOpacity = 0.4f,
			rainbowLight = 0.15f,
			rainbowLight2 = 0.15f
		},
		new RockWeatherState
		{
			add = new Vec3(0f, 0f, 0f) + new Vec3(-0.1f, -0.1f, 0.2f),
			multiply = new Vec3(1f, 1f, 1f),
			sky = new Vec3(0.1f, 0.15f, 0.2f),
			sunPos = new Vec2(0.8f, -1f),
			lightOpacity = 1f,
			sunGlow = 0f,
			sunOpacity = 0.3f
		},
		new RockWeatherState
		{
			add = new Vec3(0f, 0f, 0.06f) + new Vec3(-0.1f, -0.1f, 0.2f),
			multiply = new Vec3(0.91f, 0.99f, 0.94f),
			sky = new Vec3(0.15f, 0.15f, 0.25f),
			sunPos = new Vec2(0.9f, -1.2f),
			lightOpacity = 1f,
			sunGlow = -0.2f,
			sunOpacity = 0f
		}
	};

	public static Weather _prevWeather;

	public static float _prevWeatherLerp = 0f;

	public static float lightOpacity;

	public static float sunGlow;

	public static float sunOpacity = 1f;

	public static Vec2 sunPos;

	private RockWeatherState _lastAppliedState;

	private static float snowChance = 0f;

	private static float rainChance = 0f;

	private static float sunshowers = 0f;

	private float wait;

	public float rainbowLight;

	public float rainbowLight2;

	public static float rainbowFade = 0f;

	public static float rainbowTime = 0f;

	public static float _timeRaining = 0f;

	public static bool alwaysRainbow = false;

	public static bool neverRainbow = false;

	public static Weather weather => _weather;

	private RockWeatherState GetWeatherState(float time, bool lerp = true)
	{
		RockWeatherState curMap = null;
		RockWeatherState nextMap = null;
		float zoneSize = 0f;
		int curZone = 0;
		if (_weather == Weather.Sunny)
		{
			zoneSize = 1f / (float)timeOfDayColorMultMap.Count;
			curZone = (int)(_timeOfDay * (float)timeOfDayColorMultMap.Count);
			if (curZone >= timeOfDayColorMultMap.Count)
			{
				curZone = timeOfDayColorMultMap.Count - 1;
			}
			curMap = timeOfDayColorMultMap[curZone];
			nextMap = ((curZone + 1 <= timeOfDayColorMultMap.Count - 1) ? timeOfDayColorMultMap[curZone + 1] : timeOfDayColorMultMap[0]);
		}
		else if (_weather == Weather.Snowing)
		{
			zoneSize = 1f / (float)timeOfDayColorMultMapWinter.Count;
			curZone = (int)(_timeOfDay * (float)timeOfDayColorMultMapWinter.Count);
			if (curZone >= timeOfDayColorMultMapWinter.Count)
			{
				curZone = timeOfDayColorMultMapWinter.Count - 1;
			}
			curMap = timeOfDayColorMultMapWinter[curZone];
			nextMap = ((curZone + 1 <= timeOfDayColorMultMapWinter.Count - 1) ? timeOfDayColorMultMapWinter[curZone + 1] : timeOfDayColorMultMapWinter[0]);
		}
		else if (_weather == Weather.Raining)
		{
			zoneSize = 1f / (float)timeOfDayColorMultMapRaining.Count;
			curZone = (int)(_timeOfDay * (float)timeOfDayColorMultMapRaining.Count);
			if (curZone >= timeOfDayColorMultMapRaining.Count)
			{
				curZone = timeOfDayColorMultMapRaining.Count - 1;
			}
			curMap = timeOfDayColorMultMapRaining[curZone];
			nextMap = ((curZone + 1 <= timeOfDayColorMultMapRaining.Count - 1) ? timeOfDayColorMultMapRaining[curZone + 1] : timeOfDayColorMultMapRaining[0]);
		}
		float travel = Maths.NormalizeSection(_timeOfDay, zoneSize * (float)curZone, zoneSize * (float)(curZone + 1));
		RockWeatherState newState = new RockWeatherState();
		if (_lastAppliedState == null)
		{
			_lastAppliedState = curMap.Copy();
		}
		if (lerp)
		{
			float lerpSpeed = 0.001f;
			newState.add = Lerp.Vec3(_lastAppliedState.add, curMap.add + (nextMap.add - curMap.add) * travel, lerpSpeed);
			newState.multiply = Lerp.Vec3(_lastAppliedState.multiply, curMap.multiply + (nextMap.multiply - curMap.multiply) * travel, lerpSpeed);
			newState.sky = Lerp.Vec3(_lastAppliedState.sky, curMap.sky + (nextMap.sky - curMap.sky) * travel, lerpSpeed);
			newState.lightOpacity = Lerp.Float(_lastAppliedState.lightOpacity, curMap.lightOpacity + (nextMap.lightOpacity - curMap.lightOpacity) * travel, lerpSpeed);
			newState.sunPos = Lerp.Vec2(_lastAppliedState.sunPos, curMap.sunPos + (nextMap.sunPos - curMap.sunPos) * travel, lerpSpeed);
			newState.sunGlow = Lerp.Float(_lastAppliedState.sunGlow, curMap.sunGlow + (nextMap.sunGlow - curMap.sunGlow) * travel, lerpSpeed);
			newState.sunOpacity = Lerp.Float(_lastAppliedState.sunOpacity, curMap.sunOpacity + (nextMap.sunOpacity - curMap.sunOpacity) * travel, lerpSpeed);
			newState.rainbowLight = Lerp.Float(_lastAppliedState.rainbowLight, curMap.rainbowLight + (nextMap.rainbowLight - curMap.rainbowLight) * travel, lerpSpeed);
			newState.rainbowLight2 = Lerp.Float(_lastAppliedState.rainbowLight2, curMap.rainbowLight2 + (nextMap.rainbowLight2 - curMap.rainbowLight2) * travel, lerpSpeed);
		}
		else
		{
			newState.add = curMap.add + (nextMap.add - curMap.add) * travel;
			newState.multiply = curMap.multiply + (nextMap.multiply - curMap.multiply) * travel;
			newState.sky = curMap.sky + (nextMap.sky - curMap.sky) * travel;
			newState.lightOpacity = curMap.lightOpacity + (nextMap.lightOpacity - curMap.lightOpacity) * travel;
			newState.sunPos = curMap.sunPos + (nextMap.sunPos - curMap.sunPos) * travel;
			newState.sunGlow = curMap.sunGlow + (nextMap.sunGlow - curMap.sunGlow) * travel;
			newState.sunOpacity = curMap.sunOpacity + (nextMap.sunOpacity - curMap.sunOpacity) * travel;
			newState.rainbowLight = curMap.rainbowLight + (nextMap.rainbowLight - curMap.rainbowLight) * travel;
			newState.rainbowLight2 = curMap.rainbowLight2 + (nextMap.rainbowLight2 - curMap.rainbowLight2) * travel;
		}
		_lastAppliedState = newState;
		return newState;
	}

	private void ApplyWeatherState(RockWeatherState state)
	{
		Layer.Game.colorMul = state.multiply * Layer.Game.fade;
		Layer.Background.colorMul = state.multiply * Layer.Background.fade;
		_board.fieldMulColor = state.multiply * Layer.Game.fade;
		Layer.Game.colorAdd = state.add * Layer.Game.fade;
		Layer.Background.colorAdd = state.add * Layer.Background.fade;
		_board.fieldAddColor = state.add * Layer.Game.fade;
		Level.current.backgroundColor = new Color(state.sky.x, state.sky.y, state.sky.z) * Layer.Game.fade;
		lightOpacity = state.lightOpacity;
		sunPos = state.sunPos;
		sunGlow = state.sunGlow;
		sunOpacity = state.sunOpacity;
		_lastAppliedState = state;
	}

	public RockWeather(RockScoreboard board)
	{
		base.layer = Layer.Foreground;
		_board = board;
		if (_weather == Weather.Snowing)
		{
			_skyColor = skyColor;
			_enviroColor = winterColor;
		}
		else
		{
			_skyColor = skyColor;
			_enviroColor = summerColor;
		}
		RainParticle.splash = new SpriteMap("rainSplash", 8, 8);
	}

	public void Start()
	{
		RockWeatherState state = GetWeatherState(_timeOfDay, lerp: false);
		ApplyWeatherState(state);
	}

	public BitBuffer NetSerialize()
	{
		BitBuffer bitBuffer = new BitBuffer();
		bitBuffer.Write(_timeOfDay);
		bitBuffer.Write((byte)_weather);
		return bitBuffer;
	}

	public void NetDeserialize(BitBuffer data)
	{
		_timeOfDay = data.ReadFloat();
		_weather = (Weather)data.ReadByte();
	}

	public static void TickWeather()
	{
		_timeOfDay += 6.1728397E-06f;
		_weatherTime += 6.1728397E-06f;
		if (_weather == Weather.Raining)
		{
			_timeRaining += Maths.IncFrameTimer();
		}
		if (_timeOfDay > 1f)
		{
			_timeOfDay = 0f;
		}
	}

	public static void Reset()
	{
		_timeOfDay = Rando.Float(0.35f, 0.42f);
		_weatherTime = 1f;
		_weather = Weather.Sunny;
		alwaysRainbow = false;
		neverRainbow = false;
		DateTime now = MonoMain.GetLocalTime();
		if (now.Month < 3)
		{
			snowChance = 0.1f;
			if (now.Month < 2)
			{
				snowChance = 0.05f;
			}
			rainChance = 0.006f;
			if (now.Month < 2)
			{
				rainChance = 0.003f;
			}
		}
		else if (now.Month > 6)
		{
			snowChance = 0.0001f;
			if (now.Month > 7)
			{
				snowChance = 0.001f;
				if (now.Month > 8)
				{
					snowChance = 0.01f;
					if (now.Month > 9)
					{
						snowChance = 0.03f;
						if (now.Month > 10)
						{
							snowChance = 0.08f;
							if (now.Month == 12)
							{
								snowChance = 0.25f;
							}
						}
					}
				}
			}
		}
		if (now.Month > 3)
		{
			rainChance = 0.08f;
			if (now.Month > 5)
			{
				rainChance = 0.02f;
			}
			if (now.Month > 7)
			{
				rainChance = 0.005f;
			}
			if (now.Month > 8)
			{
				rainChance = 0.001f;
			}
			if (now.Month > 10)
			{
				rainChance = 0f;
			}
		}
		if (now.Month == 12 && Rando.Float(1f) > 0.85f)
		{
			_weather = Weather.Snowing;
		}
		if (now.Month == 4 && Rando.Float(1f) > 0.92f)
		{
			_weather = Weather.Raining;
			rainChance = 0.2f;
		}
		if (now.Month == 12 && now.Day == 25)
		{
			_weather = Weather.Snowing;
		}
		if (now.Month == 4 && now.Day == 20)
		{
			_weather = Weather.Raining;
			neverRainbow = true;
		}
		if (now.Month == 7 && Rando.Int(10000) == 1)
		{
			_weather = Weather.Snowing;
			snowChance = 0.1f;
		}
		if (now.Month == 3 && now.Day == 9)
		{
			_weather = Weather.Sunny;
			alwaysRainbow = true;
			rainChance = 0f;
			snowChance = 0f;
		}
		if (now.Month == 10 && now.Day == 24)
		{
			_weather = Weather.Sunny;
			alwaysRainbow = true;
			rainChance = 0f;
			snowChance = 0f;
		}
		if (now.Year == 2030)
		{
			_weather = Weather.Snowing;
			snowChance = 1f;
			rainChance = 0f;
		}
		if (now.Year == 2031 && now.Month <= 3)
		{
			_weather = Weather.Raining;
			snowChance = 0f;
			rainChance = 1f;
		}
		if (now.Year == 2031 && now.Month == 4)
		{
			alwaysRainbow = true;
			sunshowers = 999999f;
		}
		if (now.Year == 2031 && now.Month > 4)
		{
			snowChance = 0f;
			if (_weather == Weather.Snowing)
			{
				_weather = Weather.Raining;
			}
		}
	}

	public void SetWeather(Weather w)
	{
		_weather = w;
		_weatherTime = 0f;
	}

	public override void Update()
	{
		if (alwaysRainbow)
		{
			rainbowFade = 1f;
			rainbowTime = 1f;
		}
		rainbowFade = Lerp.Float(rainbowFade, (rainbowTime > 0f) ? 1f : 0f, 0.001f);
		rainbowTime -= Maths.IncFrameTimer();
		if (_weather != Weather.Sunny)
		{
			rainbowTime -= Maths.IncFrameTimer() * 8f;
		}
		if (rainbowTime < 0f)
		{
			rainbowTime = 0f;
		}
		if (neverRainbow)
		{
			rainbowFade = 0f;
		}
		RockWeatherState state = GetWeatherState(_timeOfDay);
		rainbowLight = state.rainbowLight * rainbowFade;
		rainbowLight2 = state.rainbowLight2 * rainbowFade;
		ApplyWeatherState(state);
		_prevWeatherLerp = Lerp.Float(_prevWeatherLerp, 0f, 0.05f);
		if (Network.isServer)
		{
			wait += 0.003f;
			if (wait > 1f)
			{
				wait = 0f;
				if (_weatherTime > 0.1f)
				{
					if (snowChance > 0f && _weather != Weather.Snowing && Rando.Float(1f) > 1f - snowChance)
					{
						_prevWeatherLerp = 1f;
						sunshowers = 0f;
						_prevWeather = _weather;
						_weather = Weather.Snowing;
						if (Network.isActive)
						{
							Send.Message(new NMChangeWeather((byte)_weather));
						}
						_weatherTime = 0f;
					}
					if (rainChance > 0f && _weather != Weather.Raining && Rando.Float(1f) > 1f - rainChance)
					{
						_prevWeatherLerp = 1f;
						sunshowers = 0f;
						_prevWeather = _weather;
						_weather = Weather.Raining;
						if (Network.isActive)
						{
							Send.Message(new NMChangeWeather((byte)_weather));
						}
						_weatherTime = 0f;
					}
					if (_weather != Weather.Sunny && Rando.Float(1f) > 0.98f)
					{
						_prevWeatherLerp = 1f;
						if (_weather == Weather.Raining)
						{
							if ((_timeRaining > 900f && Rando.Float(1f) > 0.45f) || Rando.Float(1f) > 0.95f)
							{
								rainbowTime = Rando.Float(30f, 240f);
							}
							if (Rando.Float(1f) > 0.4f)
							{
								sunshowers = Rando.Float(0.1f, 60f);
							}
						}
						_timeRaining = 0f;
						_prevWeather = _weather;
						_weather = Weather.Sunny;
						if (Network.isActive)
						{
							Send.Message(new NMChangeWeather((byte)_weather));
						}
						_weatherTime = 0f;
					}
				}
			}
		}
		sunshowers -= Maths.IncFrameTimer();
		if (sunshowers <= 0f)
		{
			sunshowers = 0f;
		}
		if (_weather == Weather.Snowing)
		{
			while (_particleWait <= 0f)
			{
				_particleWait += 1f;
				SnowParticle particle = new SnowParticle(new Vec2(Rando.Float(-100f, 400f), Rando.Float(-500f, -550f)));
				particle.z = Rando.Float(0f, 200f);
				_particles.Add(particle);
			}
			_particleWait -= 0.5f;
		}
		else if (_weather == Weather.Raining || sunshowers > 0f)
		{
			while (_particleWait <= 0f)
			{
				_particleWait += 1f;
				RainParticle particle2 = new RainParticle(new Vec2(Rando.Float(-100f, 900f), Rando.Float(-500f, -550f)));
				particle2.z = Rando.Float(0f, 200f);
				_particles.Add(particle2);
			}
			_particleWait -= 1f;
		}
		List<WeatherParticle> remove = new List<WeatherParticle>();
		foreach (WeatherParticle p in _particles)
		{
			p.Update();
			if (p.position.y > 0f)
			{
				p.die = true;
			}
			if (p is RainParticle && p.z < 70f && p.position.y > -62f)
			{
				p.die = true;
				p.position.y = -58f;
			}
			else if (p is RainParticle && p.z < 40f && p.position.y > -98f)
			{
				p.die = true;
				p.position.y = -98f;
			}
			else if (p is RainParticle && p.z < 25f && p.position.x > 175f && p.position.x < 430f && p.position.y > -362f && p.position.y < -352f)
			{
				p.die = true;
				p.position.y = -362f;
			}
			if (p.alpha < 0.01f)
			{
				remove.Add(p);
			}
		}
		foreach (WeatherParticle p2 in remove)
		{
			_particles.Remove(p2);
		}
	}

	public override void Draw()
	{
		if (RockScoreboard.drawingLighting)
		{
			return;
		}
		foreach (WeatherParticle particle in _particles)
		{
			particle.Draw();
		}
	}
}
