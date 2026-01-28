using System;

namespace DuckGame;

[EditorGroup("Arcade|Cameras", EditorItemType.ArcadeNew)]
public class CameraFixed : CustomCamera
{
    public EditorProperty<int> Size = new EditorProperty<int>(320, null, 60f, 1920f, 1f);

    public EditorProperty<float> MoveX = new EditorProperty<float>(0f, null, -10f, 10f, 0.25f);

    public EditorProperty<float> MoveY = new EditorProperty<float>(0f, null, -10f, 10f, 0.25f);

    public EditorProperty<float> MoveDelay = new EditorProperty<float>(1f, null, 0f, 120f, 0.25f);

    private bool moving;

    private int inc;

    private CameraMover curMover;

    public CameraFixed()
    {
        _contextMenuFilter.Add("wide");
        _editorName = "Camera Fixed";
        editorTooltip = "A fixed Camera that stays in one place.";
        Size._tooltip = "The size of the camera view (In pixels).";
        graphic = new Sprite("cameraIcon");
        collisionSize = new Vec2(8f, 8f);
        collisionOffset = new Vec2(-4f, -4f);
        _visibleInGame = false;
    }

    public override void Initialize()
    {
        wide.value = Size.value;
        base.Initialize();
    }

    public override void Update()
    {
        if (!(Level.current is GameLevel) || GameMode.started)
        {
            if (MoveDelay.value > 0f)
            {
                MoveDelay.value -= Maths.IncFrameTimer();
            }
            else
            {
                Level.current.camera.x += MoveX.value;
                Level.current.camera.y += MoveY.value;
                Position = Level.current.camera.center;
                if (MoveX.value != 0f || MoveY.value != 0f)
                {
                    CameraMover m = Level.CheckLine<CameraMover>(Position, Position + new Vec2(MoveX.value, MoveY.value));
                    if (m != null && m != curMover && ((m.Position - Position).Length() < 0.5f || (MoveX.value != 0f && Math.Sign(m.Position.X - Position.X) != Math.Sign(MoveX.value)) || (MoveY.value != 0f && Math.Sign(m.Position.Y - Position.Y) != Math.Sign(MoveY.value))))
                    {
                        Position = m.Position;
                        MoveX.value = m.SpeedX.value;
                        MoveY.value = m.SpeedY.value;
                        MoveDelay.value = m.MoveDelay.value;
                        curMover = m;
                    }
                }
            }
        }
        base.Update();
    }

    public override void Draw()
    {
        wide.value = Size.value;
        base.Draw();
    }
}
