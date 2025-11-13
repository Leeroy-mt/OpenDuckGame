using System.Collections.Generic;

namespace DuckGame;

/// <summary>
/// The RumbleManager keeps a list of RumbleEvents added through AddRumbleEvent calls, and each frame evaluates each event, 
/// looking for profiles it should effect. 
///
/// The intensity of a rumble is affected by duration or distance, depending on the type.
///
/// The modified intensity of every rumble affecting a Profile is added to that Profile's active controller, and then each controller
/// is Rumbled at the intensity specified in this accumulated value.
///
/// RumbleEvents can be added as :
/// - positional (find all Profiles with ducks nearby to the rumble's position and rumble them, modifying the intensity by distance falloff
/// - profile (rumble the specified profile. If no profile is set, rumble all active profiles)
/// </summary>
public static class RumbleManager
{
	private const float RUMBLE_DISTANCE_MAX = 512f;

	private const float RUMBLE_DISTANCE_MIN = 32f;

	private static List<RumbleEvent> ListRumbleEvents = new List<RumbleEvent>();

	/// <summary>
	/// Evaluates whether we're in a "game" setting, in regards to rumble
	/// Either in an actual match level, or in team select 2
	/// </summary>        
	private static bool isInGameForRumble()
	{
		if (!Level.core.gameInProgress)
		{
			return Level.current is TeamSelect2;
		}
		return true;
	}

	/// <summary>
	///  Add a Rumble Event after all properties have been set.
	/// </summary>        
	public static void AddRumbleEvent(RumbleEvent rumbleEvent)
	{
		if (!(rumbleEvent.intensityInitial <= 0f))
		{
			ListRumbleEvents.Add(rumbleEvent);
		}
	}

	/// <summary>
	/// Add a Rumble that will affect all Profiles whose duck is near the rumble's position (with distance attenuation)
	/// Positional rumbles are always type Gameplay.
	/// </summary>
	public static void AddRumbleEvent(Vec2 positionToSet, RumbleEvent rumbleEvent)
	{
		rumbleEvent.position = positionToSet;
		rumbleEvent.type = RumbleType.Gameplay;
		AddRumbleEvent(rumbleEvent);
	}

	/// <summary>
	/// Add a Rumble for a specific profile.
	/// </summary>        
	public static void AddRumbleEvent(Profile profileToRumble, RumbleEvent rumbleEvent)
	{
		if (profileToRumble != null && profileToRumble.localPlayer)
		{
			rumbleEvent.profile = profileToRumble;
			AddRumbleEvent(rumbleEvent);
		}
	}

	/// <summary>
	/// Add a rumble for all profiles 
	/// </summary>      
	public static void AddRumbleEventForAll(RumbleEvent rumbleEvent)
	{
		AddRumbleEvent(rumbleEvent);
	}

	public static void ClearRumbles(RumbleType? rumbleType)
	{
		if (rumbleType.HasValue)
		{
			ListRumbleEvents.RemoveAll((RumbleEvent rumble) => rumble.type == rumbleType);
		}
		else
		{
			ListRumbleEvents.Clear();
		}
	}

	/// <summary>
	/// Each frame we total up the intensities from various rumbles and add them to a device for a total intensity to rumble the controller by.
	/// </summary>        
	private static void AddIntensityToDevice(InputDevice controllerToSet, float intensityToAdd)
	{
		if (controllerToSet != null)
		{
			controllerToSet.rumbleIntensity += intensityToAdd;
		}
	}

	public static void Update()
	{
		if (!Graphics.inFocus)
		{
			ClearRumbles(null);
			return;
		}
		List<Profile> activeProfiles = Profiles.active;
		foreach (Profile profileToClear in activeProfiles)
		{
			if (profileToClear != null && profileToClear.inputProfile != null && profileToClear.inputProfile.lastActiveDevice != null)
			{
				profileToClear.inputProfile.lastActiveDevice.rumbleIntensity = 0f;
			}
		}
		for (int i = ListRumbleEvents.Count - 1; i >= 0; i--)
		{
			RumbleEvent rumbleEvent = ListRumbleEvents[i];
			if (rumbleEvent.type != RumbleType.Gameplay || !MonoMain.shouldPauseGameplay)
			{
				if (rumbleEvent.position.HasValue)
				{
					foreach (Profile profileToCheck in activeProfiles)
					{
						if (profileToCheck == null || !profileToCheck.localPlayer || profileToCheck.duck == null || (rumbleEvent.profile != null && rumbleEvent.profile != profileToCheck))
						{
							continue;
						}
						float distance = Vec2.Distance(rumbleEvent.position.Value, profileToCheck.duck.cameraPosition);
						float intensityMultiplier = 1f;
						if (distance > 32f)
						{
							if (distance > 512f)
							{
								continue;
							}
							intensityMultiplier = 1f - ((distance - 32f > 0f) ? (distance - 32f) : 0f) / 512f;
						}
						AddIntensityToDevice(profileToCheck.inputProfile.lastActiveDevice, rumbleEvent.intensityCurrent * (intensityMultiplier * intensityMultiplier));
					}
				}
				else if (rumbleEvent.profile == null)
				{
					foreach (Profile profile in activeProfiles)
					{
						if (profile != null && profile.inputProfile != null && profile.localPlayer && profile.inputProfile.lastActiveDevice != null)
						{
							AddIntensityToDevice(profile.inputProfile.lastActiveDevice, rumbleEvent.intensityCurrent);
						}
					}
				}
				else if (rumbleEvent.profile != null && rumbleEvent.profile.inputProfile != null && rumbleEvent.profile.inputProfile.lastActiveDevice != null)
				{
					AddIntensityToDevice(rumbleEvent.profile.inputProfile.lastActiveDevice, rumbleEvent.intensityCurrent);
				}
				if (!rumbleEvent.Update())
				{
					ListRumbleEvents.RemoveAt(i);
				}
			}
		}
		if (!(Options.Data.rumbleIntensity > 0f))
		{
			return;
		}
		foreach (Profile profileToCheck2 in activeProfiles)
		{
			if (profileToCheck2 != null && profileToCheck2.inputProfile != null)
			{
				InputDevice deviceToRumble = profileToCheck2.inputProfile.lastActiveDevice;
				deviceToRumble?.Rumble(deviceToRumble.rumbleIntensity * deviceToRumble.RumbleIntensityModifier(), deviceToRumble.rumbleIntensity * deviceToRumble.RumbleIntensityModifier());
			}
		}
	}
}
