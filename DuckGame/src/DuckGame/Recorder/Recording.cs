using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace DuckGame;

public class Recording
{
    #region Protected Fields

    protected int _frame;

    protected RecorderFrame[] _frames = new RecorderFrame[kNumFrames];

    #endregion

    #region Private Fields

    bool _rolledOver;

    int _startFrame;

    int _endFrame;

    static int kNumFrames = 300;

    static FrameAnalytics _analytics = new();

    #endregion

    #region Public Properties

    public bool finished => _frame == _endFrame;

    public int frame
    {
        get => _frame;
        set => _frame = value % kNumFrames;
    }

    public int startFrame => _startFrame;

    public int endFrame => _endFrame;

    public float highlightScore { get; set; }

    #endregion

    public Recording()
    {
        Initialize();
    }

    #region Public Methods

    public void Initialize()
    {
        for (int i = 0; i < _frames.Length; i++)
            _frames[i].Initialize();
    }

    public void Reset()
    {
        _frame = 0;
        _startFrame = 0;
        _rolledOver = false;
        highlightScore = 0;
        _endFrame = 0;
    }

    public float GetFrameVelocity()
    {
        return _frames[_frame].totalVelocity * 0.06f;
    }

    public float GetFrameCoolness()
    {
        return _frames[_frame].coolness;
    }

    public int GetFrame(int f)
    {
        if (f < 0)
            f += kNumFrames - 1;
        else if (f >= kNumFrames)
            f -= kNumFrames;
        return f;
    }

    public float GetFrameAction()
    {
        return _frames[_frame].actions;
    }

    public float GetFrameBonus()
    {
        return _frames[_frame].bonus;
    }

    public float GetFrameTotal()
    {
        FrameAnalytics data = GetAnalytics(_analytics);
        return data.deaths + data.coolness + data.bonus + data.actions + data.totalVelocity;
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
            _startFrame = (_frame + 1) % kNumFrames;
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
        _frames[_frame].coolness = Math.Max((byte)(_frames[_frame].coolness + (byte)(val * Highlights.highlightRatingMultiplier)), _frames[_frame].coolness);
    }

    public void LogDeath()
    {
        _frames[_frame].deaths = Math.Max((byte)(_frames[_frame].deaths + (byte)Highlights.highlightRatingMultiplier), _frames[_frame].deaths);
    }

    public void LogAction(int num = 1)
    {
        _frames[_frame].actions = Math.Max((byte)(_frames[_frame].actions + (byte)(num * Highlights.highlightRatingMultiplier)), _frames[_frame].actions);
    }

    public void LogBonus()
    {
        _frames[_frame].bonus = Math.Max((byte)(_frames[_frame].bonus + (byte)Highlights.highlightRatingMultiplier), _frames[_frame].bonus);
    }

    public void LogBackgroundColor(Color c)
    {
        _frames[_frame].backgroundColor = c;
    }

    public void StateChange(SpriteSortMode sortModeVal, BlendState blendStateVal, SamplerState samplerStateVal, DepthStencilState depthStencilStateVal, RasterizerState rasterizerStateVal, MTEffect effectVal, Matrix cameraVal, RectangleF scissor)
    {
        _frames[_frame].StateChange(sortModeVal, blendStateVal, samplerStateVal, depthStencilStateVal, rasterizerStateVal, effectVal, cameraVal, scissor);
    }

    public void LogDraw(short textureVal, Vector2 topLeftVal, Vector2 bottomRightVal, float rotationVal, Color colorVal, short texXVal, short texYVal, short texWVal, short texHVal, float depthVal)
    {
        // textureVal: 516
        // topLeftVal 152 136
        // bottomRightVal 168 152
        // rotationVal 0
        // colorVal 255 255 255 255
        // texXVal 48
        // texYVal 64
        // texWVal 16
        // texHVal 16
        // depthVal 0.305250049
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
        fr = (fr != -1) ? GetFrame(fr) : _frame;
        int walkFrames = kNumFrames;
        int curFrame = fr;
        float time = 0;
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
                curFrame = 0;

            if (curFrame == _startFrame)
                break;
        }

        if (!found)
            time = 99;

        f.timeBeforeKill = time;
        float timeMultiplier = 1 - float.Clamp(f.timeBeforeKill, 0, 3) / 3 + 1;
        f.actions = _frames[fr].actions * (timeMultiplier * 0.03f);
        f.deaths = _frames[fr].deaths * timeMultiplier;
        f.bonus = _frames[fr].bonus * (timeMultiplier * 0.08f);
        f.coolness = _frames[fr].coolness * (timeMultiplier * 0.1f);
        f.totalVelocity = _frames[fr].totalVelocity * 0.002f * timeMultiplier;
        return f;
    }

    #endregion
}