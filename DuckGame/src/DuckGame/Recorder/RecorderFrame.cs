using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace DuckGame;

public struct RecorderFrame
{
    #region Public Fields

    public byte deaths;

    public byte actions;

    public byte bonus;

    public byte coolness;

    public int currentObject;

    public float totalVelocity;

    public Color backgroundColor;

    public RecorderFrameItem[] objects;

    public List<RecorderSoundItem> sounds;

    public Dictionary<long, RecorderFrameItem> sortedObjects;

    public Dictionary<int, RecorderFrameStateChange> _states;

    #endregion

    static int kMaxObjects = 1200;

    #region Public Methods

    public void Initialize()
    {
        currentObject = 0;
        objects = new RecorderFrameItem[kMaxObjects];
        _states = [];
        sortedObjects = [];
        sounds = [];
        backgroundColor = Color.White;
    }

    public void Reset()
    {
        currentObject = 0;
        totalVelocity = 0;
        actions = 0;
        bonus = 0;
        deaths = 0;
        coolness = 0;
        _states.Clear();
        sounds.Clear();
        sortedObjects.Clear();
    }

    public RecorderFrameStateChange GetStateWithIndex(int index)
    {
        return _states.FirstOrDefault(x => x.Value.stateIndex == index).Value;
    }

    public bool HasStateWithIndex(int index)
    {
        return _states.Any(x => x.Value.stateIndex == index);
    }

    public void StateChange(SpriteSortMode sortModeVal, BlendState blendStateVal, SamplerState samplerStateVal, DepthStencilState depthStencilStateVal, RasterizerState rasterizerStateVal, MTEffect effectVal, Matrix cameraVal, RectangleF sciss)
    {
        _states[currentObject] = new RecorderFrameStateChange
        {
            sortMode = sortModeVal,
            blendState = blendStateVal,
            samplerState = samplerStateVal,
            depthStencilState = depthStencilStateVal,
            rasterizerState = rasterizerStateVal,
            effectIndex = effectVal?.EffectIndex ?? (-1),
            camera = cameraVal,
            stateIndex = Graphics.currentStateIndex,
            scissor = sciss
        };
    }

    public void IncrementObject()
    {
        currentObject++;
        if (currentObject >= kMaxObjects)
            currentObject = kMaxObjects - 1;
    }

    public void Render()
    {
        bool begun = false;
        Graphics.Clear(backgroundColor * Graphics.fade);
        for (int i = 0; i < currentObject; i++)
        {
            if (_states.TryGetValue(i, out RecorderFrameStateChange state))
            {
                if (begun)
                    Graphics.screen.End();

                begun = true;
                MTEffect e = Content.GetMTEffectFromIndex(state.effectIndex);
                if (Layer.IsBasicLayerEffect(e))
                {
                    e.effect.Parameters["fade"].SetValue(new Vector3(Graphics.fade));
                    e.effect.Parameters["add"].SetValue(new Vector3(Graphics.fadeAddRenderValue));
                }
                Graphics.screen.Begin(state.sortMode, state.blendState, state.samplerState, state.depthStencilState, state.rasterizerState, Content.GetMTEffectFromIndex(state.effectIndex), state.camera);
                Graphics.SetScissorRectangle(state.scissor);
            }
            Graphics.DrawRecorderItem(ref objects[i]);
        }

        if (begun)
            Graphics.screen.End();
    }

    public void Update()
    {
        foreach (RecorderSoundItem item in sounds)
            SFX.Play(item.sound, item.volume, item.pitch, item.pan);
    }

    #endregion
}
