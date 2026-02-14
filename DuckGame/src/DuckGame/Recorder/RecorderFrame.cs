using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace DuckGame;

public struct RecorderFrame
{
    private static int kMaxObjects = 1200;

    public RecorderFrameItem[] objects;

    public Dictionary<long, RecorderFrameItem> sortedObjects;

    public int currentObject;

    public Dictionary<int, RecorderFrameStateChange> _states;

    public List<RecorderSoundItem> sounds;

    public Color backgroundColor;

    public float totalVelocity;

    public byte deaths;

    public byte actions;

    public byte bonus;

    public byte coolness;

    public void Initialize()
    {
        currentObject = 0;
        objects = new RecorderFrameItem[kMaxObjects];
        _states = new Dictionary<int, RecorderFrameStateChange>();
        sortedObjects = new Dictionary<long, RecorderFrameItem>();
        sounds = new List<RecorderSoundItem>();
        backgroundColor = Color.White;
    }

    public void Reset()
    {
        currentObject = 0;
        totalVelocity = 0f;
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
        return _states.FirstOrDefault((KeyValuePair<int, RecorderFrameStateChange> x) => x.Value.stateIndex == index).Value;
    }

    public bool HasStateWithIndex(int index)
    {
        return _states.Where((KeyValuePair<int, RecorderFrameStateChange> x) => x.Value.stateIndex == index).Count() > 0;
    }

    public void StateChange(SpriteSortMode sortModeVal, BlendState blendStateVal, SamplerState samplerStateVal, DepthStencilState depthStencilStateVal, RasterizerState rasterizerStateVal, MTEffect effectVal, Matrix cameraVal, Rectangle sciss)
    {
        _states[currentObject] = new RecorderFrameStateChange
        {
            sortMode = sortModeVal,
            blendState = blendStateVal,
            samplerState = samplerStateVal,
            depthStencilState = depthStencilStateVal,
            rasterizerState = rasterizerStateVal,
            effectIndex = (effectVal?.effectIndex ?? (-1)),
            camera = cameraVal,
            stateIndex = Graphics.currentStateIndex,
            scissor = sciss
        };
    }

    public void IncrementObject()
    {
        currentObject++;
        if (currentObject >= kMaxObjects)
        {
            currentObject = kMaxObjects - 1;
        }
    }

    public void Render()
    {
        bool begun = false;
        Graphics.Clear(backgroundColor * Graphics.fade);
        for (int i = 0; i < currentObject; i++)
        {
            if (_states.ContainsKey(i))
            {
                if (begun)
                {
                    Graphics.screen.End();
                }
                RecorderFrameStateChange state = _states[i];
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
        {
            Graphics.screen.End();
        }
    }

    public void Update()
    {
        foreach (RecorderSoundItem item in sounds)
        {
            SFX.Play(item.sound, item.volume, item.pitch, item.pan);
        }
    }
}
