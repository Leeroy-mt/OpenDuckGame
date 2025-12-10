using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;

namespace DuckGame;

public class Recording
{
    private static int kNumFrames = 300;

    protected RecorderFrame[] _frames = new RecorderFrame[kNumFrames];

    protected int _frame;

    private int _startFrame;

    private int _endFrame;

    private bool _rolledOver;

    private float _highlightScore;

    private static FrameAnalytics _analytics = new FrameAnalytics();

    public int frame
    {
        get
        {
            return _frame;
        }
        set
        {
            _frame = value % kNumFrames;
        }
    }

    public int startFrame => _startFrame;

    public int endFrame => _endFrame;

    public bool finished => _frame == _endFrame;

    public float highlightScore
    {
        get
        {
            return _highlightScore;
        }
        set
        {
            _highlightScore = value;
        }
    }

    public Recording()
    {
        Initialize();
    }

    public void Initialize()
    {
        for (int i = 0; i < _frames.Count(); i++)
        {
            _frames[i].Initialize();
        }
    }

    public void Reset()
    {
        _frame = 0;
        _startFrame = 0;
        _rolledOver = false;
        _highlightScore = 0f;
        _endFrame = 0;
    }

    public float GetFrameVelocity()
    {
        return _frames[_frame].totalVelocity * 0.06f;
    }

    public float GetFrameCoolness()
    {
        return (int)_frames[_frame].coolness;
    }

    public int GetFrame(int f)
    {
        if (f < 0)
        {
            f += kNumFrames - 1;
        }
        else if (f >= kNumFrames)
        {
            f -= kNumFrames;
        }
        return f;
    }

    public float GetFrameAction()
    {
        return (int)_frames[_frame].actions;
    }

    public float GetFrameBonus()
    {
        return (int)_frames[_frame].bonus;
    }

    public float GetFrameTotal()
    {
        FrameAnalytics data = GetAnalytics(_analytics);
        return 0f + data.deaths + data.coolness + data.bonus + data.actions + data.totalVelocity;
    }

    public void Rewind()
    {
        _frame = _startFrame;
    }

    public virtual void RenderFrame()
    {
        _frames[_frame].Render();
    }

    public virtual void RenderFrame(float timeLag)
    {
        int framesBack = (int)(timeLag / Maths.IncFrameTimer());
        _frames[GetFrame(_frame - framesBack)].Render();
    }

    public void UpdateFrame()
    {
        _frames[_frame].Update();
    }

    public virtual void IncrementFrame(float speed = 1f)
    {
        _frame = (_frame + 1) % kNumFrames;
    }

    public virtual void NextFrame()
    {
        _frame++;
        if (_frame >= kNumFrames)
        {
            _rolledOver = true;
            _frame = 0;
        }
        _frames[_frame].Reset();
        _frames[_frame].actions += (byte)Math.Max(_frames[GetFrame(_frame - 1)].actions - 1, 0);
        _frames[_frame].bonus += (byte)Math.Max(_frames[GetFrame(_frame - 1)].bonus - 1, 0);
        _frames[_frame].coolness += (byte)Math.Max(_frames[GetFrame(_frame - 1)].coolness - 1, 0);
        _endFrame = _frame;
        if (_rolledOver)
        {
            _startFrame = (_frame + 1) % kNumFrames;
        }
    }

    public bool StepForward()
    {
        _frame = (_frame + 1) % kNumFrames;
        return _frame == _startFrame;
    }

    public void LogVelocity(float velocity)
    {
        _frames[_frame].totalVelocity += velocity * Highlights.highlightRatingMultiplier;
    }

    public void LogCoolness(int val)
    {
        _frames[_frame].coolness = Math.Max((byte)(_frames[_frame].coolness + (byte)((float)val * Highlights.highlightRatingMultiplier)), _frames[_frame].coolness);
    }

    public void LogDeath()
    {
        _frames[_frame].deaths = Math.Max((byte)(_frames[_frame].deaths + (byte)(1f * Highlights.highlightRatingMultiplier)), _frames[_frame].deaths);
    }

    public void LogAction(int num = 1)
    {
        _frames[_frame].actions = Math.Max((byte)(_frames[_frame].actions + (byte)((float)num * Highlights.highlightRatingMultiplier)), _frames[_frame].actions);
    }

    public void LogBonus()
    {
        _frames[_frame].bonus = Math.Max((byte)(_frames[_frame].bonus + (byte)(1f * Highlights.highlightRatingMultiplier)), _frames[_frame].bonus);
    }

    public void LogBackgroundColor(Color c)
    {
        _frames[_frame].backgroundColor = c;
    }

    public void StateChange(SpriteSortMode sortModeVal, BlendState blendStateVal, SamplerState samplerStateVal, DepthStencilState depthStencilStateVal, RasterizerState rasterizerStateVal, MTEffect effectVal, Matrix cameraVal, Rectangle scissor)
    {
        _frames[_frame].StateChange(sortModeVal, blendStateVal, samplerStateVal, depthStencilStateVal, rasterizerStateVal, effectVal, cameraVal, scissor);
    }

    public void LogDraw(short textureVal, Vec2 topLeftVal, Vec2 bottomRightVal, float rotationVal, Color colorVal, short texXVal, short texYVal, short texWVal, short texHVal, float depthVal)
    {
        _frames[_frame].objects[_frames[_frame].currentObject].SetData(textureVal, topLeftVal, bottomRightVal, rotationVal, colorVal, texXVal, texYVal, texWVal, texHVal, depthVal);
        _frames[_frame].IncrementObject();
    }

    public void LogSound(string soundVal, float volumeVal, float pitchVal, float panVal)
    {
        _frames[_frame].sounds.Add(new RecorderSoundItem
        {
            sound = soundVal,
            volume = volumeVal,
            pitch = pitchVal,
            pan = panVal
        });
    }

    public FrameAnalytics GetAnalytics(FrameAnalytics f, int fr = -1)
    {
        fr = ((fr != -1) ? GetFrame(fr) : _frame);
        int walkFrames = kNumFrames;
        int curFrame = fr;
        float time = 0f;
        bool found = false;
        for (int i = 0; i < walkFrames; i++)
        {
            if (_frames[curFrame].deaths > 0)
            {
                found = true;
                break;
            }
            time += 0.016f;
            curFrame++;
            if (curFrame >= kNumFrames)
            {
                curFrame = 0;
            }
            if (curFrame == _startFrame)
            {
                break;
            }
        }
        if (!found)
        {
            time = 99f;
        }
        f.timeBeforeKill = time;
        float timeMultiplier = (1f - Maths.Clamp(f.timeBeforeKill, 0f, 3f) / 3f) * 1f + 1f;
        f.actions = (float)(int)_frames[fr].actions * (timeMultiplier * 0.03f);
        f.deaths = (float)(int)_frames[fr].deaths * timeMultiplier;
        f.bonus = (float)(int)_frames[fr].bonus * (timeMultiplier * 0.08f);
        f.coolness = (float)(int)_frames[fr].coolness * (timeMultiplier * 0.1f);
        f.totalVelocity = _frames[fr].totalVelocity * 0.002f * timeMultiplier;
        return f;
    }
}
