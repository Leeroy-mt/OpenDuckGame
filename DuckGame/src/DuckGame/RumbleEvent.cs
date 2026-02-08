using Microsoft.Xna.Framework;

namespace DuckGame;

/// <summary>
/// Create a RumbleEvent and call RumbleManager.AddRumbleEvent with the event to add a rumble.
/// </summary>
public class RumbleEvent
{
    #region Public Fields

    public float intensityInitial;

    public float intensityCurrent;

    public float timeDuration;

    public float timeElapsed;

    public float timeFalloff;

    public RumbleType type;

    public Vector2? position;

    public Profile profile;

    #endregion

    #region Public Constructors

    /// <summary>
    /// Create a RumbleEvent using only enum definitions
    /// </summary>
    public RumbleEvent(RumbleIntensity intensityToSet, RumbleDuration durationToSet, RumbleFalloff falloffToSet, RumbleType rumbleTypeToSet = RumbleType.Gameplay)
    {
        SetRumbleParameters(intensityToSet, durationToSet, falloffToSet, rumbleTypeToSet);
    }

    /// <summary>
    /// Create a RumbleEvent with float-specified intensity, duration, and falloff
    /// </summary>
    public RumbleEvent(float intensityToSet, float durationToSet, float falloffToSet, RumbleType rumbleTypeToSet = RumbleType.Gameplay)
    {
        intensityInitial = intensityToSet;
        intensityCurrent = intensityInitial;
        timeDuration = durationToSet;
        timeFalloff = falloffToSet;
        type = rumbleTypeToSet;
    }

    #endregion

    #region Public Methods

    public void SetRumbleParameters(RumbleIntensity intensityToSet, RumbleDuration durationToSet, RumbleFalloff falloffToSet, RumbleType rumbleTypeToSet)
    {
        intensityInitial = intensityToSet switch
        {
            RumbleIntensity.Heavy => .8f,
            RumbleIntensity.Medium => .5f,
            RumbleIntensity.Light => .25f,
            RumbleIntensity.Kick => .15f,
            RumbleIntensity.None => 0,
            _ => .25f,
        };
        intensityCurrent = intensityInitial;
        timeDuration = durationToSet switch
        {
            RumbleDuration.Long => 1,
            RumbleDuration.Medium => .5f,
            RumbleDuration.Short => .15f,
            RumbleDuration.Pulse => .075f,
            _ => .1f,
        };
        timeFalloff = falloffToSet switch
        {
            RumbleFalloff.Long => .5f,
            RumbleFalloff.Medium => .25f,
            RumbleFalloff.Short => .1f,
            RumbleFalloff.None => 0,
            _ => .1f,
        };
        type = rumbleTypeToSet;
    }

    /// <summary>
    /// Updates the intensity of a RumbleEvent based on the time remaining in the falloff portion of the full duration.
    /// </summary> 
    public void FallOffLinear()
    {
        intensityCurrent = (1 - (timeElapsed - timeDuration) / timeFalloff) * intensityInitial;
    }

    /// <summary>
    /// Updates the elapsed time and updates the intensity for any falloff. Returns false if the rumble is completed and should be cleaned up by RumbleManager
    /// </summary>
    /// <returns></returns>
    public bool Update()
    {
        timeElapsed += .016f;
        if (timeElapsed < timeDuration + timeFalloff)
        {
            if (timeElapsed > timeDuration)
                FallOffLinear();
            return true;
        }
        return false;
    }

    #endregion
}
