using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using System.IO.Compression;

namespace DuckGame;

public class FileRecording : Recording
{
    private BinaryWriter _writer;

    private BinaryReader _reader;

    private string _fileName = "";

    private bool _setFile;

    private int _lastTextureWrittenIndex;

    private int _lastEffectWrittenIndex;

    private RasterizerState _defaultRasterizerState;

    private bool _loadedNextFrame;

    private int _curFrame;

    private float _framePos;

    public string fileName
    {
        get
        {
            return _fileName;
        }
        set
        {
            _fileName = value;
            _setFile = true;
        }
    }

    public FileRecording()
    {
        Initialize();
        _defaultRasterizerState = new RasterizerState();
        _defaultRasterizerState.CullMode = CullMode.None;
    }

    public void StopWriting()
    {
        if (_writer != null)
        {
            _writer.Close();
        }
        if (_reader != null)
        {
            _reader.Close();
        }
        UpdateAtlasFile();
    }

    public void StartWriting(string name)
    {
        if (_reader != null)
        {
            _reader.Close();
            _reader = null;
        }
        _loadedNextFrame = false;
        if (!_setFile)
        {
            if (name != "" && name != null)
            {
                _fileName = name;
            }
            else
            {
                string d = DateTime.Now.ToShortDateString() + "-" + DateTime.Now.ToShortTimeString();
                d = d.Replace("/", "_");
                d = d.Replace(":", "-");
                d = d.Replace(" ", "");
                _fileName = "funstream-" + d;
            }
            _writer = new BinaryWriter(new GZipStream(File.Open(_fileName + ".vid", FileMode.Create), CompressionMode.Compress));
        }
        else if (_writer != null)
        {
            _writer.Close();
            _writer = null;
        }
    }

    public void LoadAtlasFile(string file = "")
    {
        if (_writer != null)
        {
            UpdateAtlasFile();
        }
        if (file == "")
        {
            file = _fileName;
        }
        _fileName = file;
        BinaryReader atlas = new BinaryReader(File.Open(file + ".dat", FileMode.Open));
        while (atlas.BaseStream.Position != atlas.BaseStream.Length)
        {
            byte num = atlas.ReadByte();
            short index = atlas.ReadInt16();
            if (num == 0)
            {
                if (atlas.ReadByte() == 0)
                {
                    int width = atlas.ReadInt32();
                    int height = atlas.ReadInt32();
                    byte[] texData = new byte[width * height * 4];
                    atlas.Read(texData, 0, width * height * 4);
                    RenderTarget2D target = new RenderTarget2D(width, height);
                    target.SetData(texData);
                    Content.SetTextureAtIndex(index, target);
                }
                else
                {
                    string name = atlas.ReadString();
                    Content.SetTextureAtIndex(index, Content.Load<Tex2D>(name));
                }
            }
            else
            {
                string name2 = atlas.ReadString();
                Content.SetEffectAtIndex(index, (name2 == "") ? ((MTEffect)new BasicEffect(Graphics.device)) : Content.Load<MTEffect>(name2));
            }
        }
    }

    public void UpdateAtlasFile()
    {
        if (_writer == null)
        {
            return;
        }
        BinaryWriter atlas = new BinaryWriter(File.Open(_fileName + ".dat", FileMode.OpenOrCreate));
        atlas.Seek(0, SeekOrigin.End);
        for (int i = _lastTextureWrittenIndex; i < Content.textureList.Count; i++)
        {
            atlas.Write((byte)0);
            Tex2D tex = Content.textureList[i];
            atlas.Write(tex.textureIndex);
            if (tex.textureName == "" || tex.textureName == "__renderTarget" || tex.textureName == "__internal")
            {
                atlas.Write((byte)0);
                atlas.Write(tex.width);
                atlas.Write(tex.height);
                byte[] data = new byte[tex.width * tex.height * 4];
                if (!tex.IsDisposed && !((Texture2D)tex.nativeObject).IsDisposed)
                {
                    tex.GetData(data);
                }
                atlas.Write(data);
            }
            else
            {
                atlas.Write((byte)1);
                atlas.Write(tex.textureName);
            }
            _lastTextureWrittenIndex++;
        }
        for (int j = _lastEffectWrittenIndex; j < Content.effectList.Count; j++)
        {
            atlas.Write((byte)1);
            MTEffect effect = Content.effectList[j];
            atlas.Write(effect.effectIndex);
            atlas.Write(effect.effectName);
            _lastEffectWrittenIndex++;
        }
        atlas.Close();
    }

    public override void IncrementFrame(float speed = 1f)
    {
        if (_writer != null)
        {
            _writer.Close();
            _writer = null;
        }
        if (_reader == null)
        {
            _reader = new BinaryReader(new GZipStream(File.Open(_fileName + ".vid", FileMode.Open), CompressionMode.Decompress));
        }
        int framesToLoad = 2;
        int startFrame = 0;
        if (_loadedNextFrame)
        {
            _framePos += speed;
            if (!(_framePos >= 1f))
            {
                return;
            }
            _framePos -= 1f;
            framesToLoad = 1;
            startFrame = ((_curFrame != 0) ? 1 : 0);
            _curFrame = ((_curFrame == 0) ? 1 : 0);
            _frames[_curFrame].Update();
        }
        _loadedNextFrame = true;
        RecorderSoundItem item = default(RecorderSoundItem);
        for (int f = startFrame; f < startFrame + framesToLoad; f++)
        {
            _frame = f;
            _frames[_frame].Reset();
            Color backgroundColor = new Color(_reader.ReadByte(), _reader.ReadByte(), _reader.ReadByte(), _reader.ReadByte());
            int numSounds = _reader.ReadInt32();
            _frames[_frame].sounds.Clear();
            for (int i = 0; i < numSounds; i++)
            {
                item.sound = _reader.ReadString();
                item.pan = _reader.ReadSingle();
                item.pitch = _reader.ReadSingle();
                item.volume = _reader.ReadSingle();
                _frames[_frame].sounds.Add(item);
            }
            int num = _reader.ReadInt32();
            _frames[_frame].currentObject = num;
            _frames[_frame].backgroundColor = backgroundColor;
            for (int j = 0; j < num; j++)
            {
                if (_reader.ReadByte() == 0)
                {
                    RecorderFrameStateChange state = new RecorderFrameStateChange
                    {
                        rasterizerState = _defaultRasterizerState,
                        samplerState = SamplerState.PointClamp,
                        blendState = BlendState.AlphaBlend,
                        sortMode = (SpriteSortMode)_reader.ReadInt32(),
                        depthStencilState = ((_reader.ReadByte() == 0) ? DepthStencilState.Default : DepthStencilState.DepthRead),
                        effectIndex = _reader.ReadInt16(),
                        stateIndex = _reader.ReadInt32(),
                        camera = new Matrix
                        {
                            M11 = _reader.ReadSingle(),
                            M12 = _reader.ReadSingle(),
                            M13 = _reader.ReadSingle(),
                            M14 = _reader.ReadSingle(),
                            M21 = _reader.ReadSingle(),
                            M22 = _reader.ReadSingle(),
                            M23 = _reader.ReadSingle(),
                            M24 = _reader.ReadSingle(),
                            M31 = _reader.ReadSingle(),
                            M32 = _reader.ReadSingle(),
                            M33 = _reader.ReadSingle(),
                            M34 = _reader.ReadSingle(),
                            M41 = _reader.ReadSingle(),
                            M42 = _reader.ReadSingle(),
                            M43 = _reader.ReadSingle(),
                            M44 = _reader.ReadSingle()
                        },
                        scissor = new Rectangle(_reader.ReadInt32(), _reader.ReadInt32(), _reader.ReadInt32(), _reader.ReadInt32())
                    };
                    _frames[_frame]._states[j] = state;
                }
                _frames[_frame].objects[j].texture = _reader.ReadInt16();
                _frames[_frame].objects[j].topLeft.X = _reader.ReadSingle();
                _frames[_frame].objects[j].topLeft.Y = _reader.ReadSingle();
                _frames[_frame].objects[j].bottomRight.X = _reader.ReadSingle();
                _frames[_frame].objects[j].bottomRight.Y = _reader.ReadSingle();
                _frames[_frame].objects[j].rotation = _reader.ReadSingle();
                _frames[_frame].objects[j].color = new Color(_reader.ReadByte(), _reader.ReadByte(), _reader.ReadByte(), _reader.ReadByte());
                _frames[_frame].objects[j].texX = _reader.ReadInt16();
                _frames[_frame].objects[j].texY = _reader.ReadInt16();
                _frames[_frame].objects[j].texW = _reader.ReadInt16();
                _frames[_frame].objects[j].texH = _reader.ReadInt16();
                _frames[_frame].objects[j].depth = _reader.ReadSingle();
            }
        }
        _frame = _curFrame;
    }

    public override void NextFrame()
    {
        if (_writer == null)
        {
            return;
        }
        _writer.Write(_frames[_frame].backgroundColor.r);
        _writer.Write(_frames[_frame].backgroundColor.g);
        _writer.Write(_frames[_frame].backgroundColor.b);
        _writer.Write(_frames[_frame].backgroundColor.a);
        _writer.Write(_frames[_frame].sounds.Count);
        for (int i = 0; i < _frames[_frame].sounds.Count; i++)
        {
            _writer.Write(_frames[_frame].sounds[i].sound);
            _writer.Write(_frames[_frame].sounds[i].pan);
            _writer.Write(_frames[_frame].sounds[i].pitch);
            _writer.Write(_frames[_frame].sounds[i].volume);
        }
        _writer.Write(_frames[_frame].currentObject);
        for (int j = 0; j < _frames[_frame].currentObject; j++)
        {
            if (_frames[_frame]._states.ContainsKey(j))
            {
                _writer.Write((byte)0);
                RecorderFrameStateChange state = _frames[_frame]._states[j];
                _writer.Write((int)state.sortMode);
                _writer.Write((byte)((state.depthStencilState != DepthStencilState.Default) ? 1u : 0u));
                _writer.Write(state.effectIndex);
                _writer.Write(state.stateIndex);
                _writer.Write(state.camera.M11);
                _writer.Write(state.camera.M12);
                _writer.Write(state.camera.M13);
                _writer.Write(state.camera.M14);
                _writer.Write(state.camera.M21);
                _writer.Write(state.camera.M22);
                _writer.Write(state.camera.M23);
                _writer.Write(state.camera.M24);
                _writer.Write(state.camera.M31);
                _writer.Write(state.camera.M32);
                _writer.Write(state.camera.M33);
                _writer.Write(state.camera.M34);
                _writer.Write(state.camera.M41);
                _writer.Write(state.camera.M42);
                _writer.Write(state.camera.M43);
                _writer.Write(state.camera.M44);
                _writer.Write(state.scissor.x);
                _writer.Write(state.scissor.y);
                _writer.Write(state.scissor.width);
                _writer.Write(state.scissor.height);
            }
            else
            {
                _writer.Write((byte)1);
            }
            _writer.Write(_frames[_frame].objects[j].texture);
            _writer.Write(_frames[_frame].objects[j].topLeft.X);
            _writer.Write(_frames[_frame].objects[j].topLeft.Y);
            _writer.Write(_frames[_frame].objects[j].bottomRight.X);
            _writer.Write(_frames[_frame].objects[j].bottomRight.Y);
            _writer.Write(_frames[_frame].objects[j].rotation);
            _writer.Write(_frames[_frame].objects[j].color.r);
            _writer.Write(_frames[_frame].objects[j].color.g);
            _writer.Write(_frames[_frame].objects[j].color.b);
            _writer.Write(_frames[_frame].objects[j].color.a);
            _writer.Write(_frames[_frame].objects[j].texX);
            _writer.Write(_frames[_frame].objects[j].texY);
            _writer.Write(_frames[_frame].objects[j].texW);
            _writer.Write(_frames[_frame].objects[j].texH);
            _writer.Write(_frames[_frame].objects[j].depth);
        }
        base.NextFrame();
    }
}
