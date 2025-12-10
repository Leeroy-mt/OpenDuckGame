using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DuckGame;

public class FollowCam : Camera
{
    public Viewport storedViewport;

    private HashSet<Thing> _follow = new HashSet<Thing>();

    private Dictionary<Thing, Vec2> _prevPositions = new Dictionary<Thing, Vec2>();

    private new float _viewSize;

    public float manualViewSize = -1f;

    public Vec2 _center;

    private float _lerpSpeed = 1f;

    private float _lerpBorder;

    private float border = 80f;

    public float minSize = 60f;

    private float _prevCenterMoveX;

    private float _prevCenterMoveY;

    private float _prevViewSize;

    private bool _startedFollowing;

    private Vec2 _averagePosition = Vec2.Zero;

    private bool _startCentered = true;

    private float _lerpMult = 1f;

    private float _speed = 1f;

    private bool immediate;

    private bool _allowWarps;

    private bool _checkedZoom;

    private float _zoomMult = 1f;

    private bool _skipResize;

    private bool _overFollow;

    public float hardLimitLeft = -999999f;

    public float hardLimitRight = 999999f;

    public float hardLimitTop = -999999f;

    public float hardLimitBottom = 999999f;

    private bool woteFrame;

    private CameraBounds _bounds;

    private List<Thing> _removeList = new List<Thing>();

    private int _framesCreated;

    public static bool boost;

    public float viewSize
    {
        get
        {
            return _viewSize;
        }
        set
        {
            _viewSize = value;
        }
    }

    public float lerpSpeed
    {
        get
        {
            return _lerpSpeed;
        }
        set
        {
            _lerpSpeed = value;
        }
    }

    public bool startCentered
    {
        get
        {
            return _startCentered;
        }
        set
        {
            _startCentered = value;
        }
    }

    public float lerpMult
    {
        get
        {
            return _lerpMult;
        }
        set
        {
            _lerpMult = value;
        }
    }

    public float speed
    {
        get
        {
            return _speed;
        }
        set
        {
            _speed = value;
        }
    }

    public float zoomMult
    {
        get
        {
            return _zoomMult;
        }
        set
        {
            _zoomMult = value;
        }
    }

    public void Add(Thing t)
    {
        if (t != null)
        {
            _follow.Add(t);
            _startedFollowing = true;
        }
    }

    public void Clear()
    {
        _follow.Clear();
        _prevPositions.Clear();
    }

    public void Remove(Thing t)
    {
        _follow.Remove(t);
        if (t != null && _prevPositions.ContainsKey(t))
        {
            _prevPositions.Remove(t);
        }
    }

    public bool Contains(Thing t)
    {
        return _follow.Contains(t);
    }

    public void Adjust()
    {
        Level.current.CalculateBounds();
        Update();
        immediate = true;
        Update();
        immediate = false;
    }

    public override void Update()
    {
        _framesCreated++;
        if (Network.isActive && (_framesCreated > 60 || _framesCreated > 120) && !_startedFollowing && _follow.Count == 0)
        {
            _startedFollowing = false;
        }
        if (Level.current is TeamSelect2)
        {
            _follow.RemoveWhere((Thing x) => x is Duck);
            foreach (Duck d in Level.current.things[typeof(Duck)])
            {
                Add(d);
            }
        }
        if (!_checkedZoom)
        {
            _checkedZoom = true;
            CameraZoom z = Level.First<CameraZoom>();
            if (z != null)
            {
                _zoomMult = z.zoomMult;
                _overFollow = z.overFollow;
                _allowWarps = z.allowWarps;
            }
            if (Level.current is RandomLevel)
            {
                _zoomMult = 1.25f;
            }
            CustomCamera c = Level.First<CustomCamera>();
            if (c != null)
            {
                base.width = c.wide.value;
                base.height = (float)c.wide.value * 0.5625f;
                base.center = c.position;
                _skipResize = true;
            }
            if (Level.First<CameraBounds>() != null)
            {
                Adjust();
            }
        }
        bool liveDucks = true;
        if (_startedFollowing)
        {
            liveDucks = false;
            foreach (Duck d2 in Level.current.things[typeof(Duck)])
            {
                if (!d2.dead)
                {
                    liveDucks = true;
                }
                if (Network.isActive && !d2.dead)
                {
                    Add(d2);
                }
            }
            if (Network.isActive)
            {
                foreach (RCCar car in Level.current.things[typeof(RCCar)])
                {
                    if (car.receivingSignal)
                    {
                        Add(car);
                    }
                    else
                    {
                        Remove(car);
                    }
                }
            }
            foreach (Thing item in _follow)
            {
                if (item is Duck { dead: false })
                {
                    liveDucks = true;
                }
            }
            if (Network.isActive)
            {
                for (int i = 0; i < _follow.Count; i++)
                {
                    if (!_follow.ElementAt(i).active)
                    {
                        _follow.Remove(_follow.ElementAt(i));
                        i--;
                    }
                }
            }
        }
        if (_skipResize)
        {
            return;
        }
        float lmult = lerpMult;
        if (lerpSpeed > 0.9f)
        {
            lerpMult = 1.8f;
        }
        border += (_lerpBorder - border) * (lerpSpeed * 16f * _lerpMult) * _speed;
        if (immediate)
        {
            border = _lerpBorder;
        }
        float leftmost = 99999f;
        float rightmost = -99999f;
        float topmost = 99999f;
        float bottommost = -99999f;
        Vec2 average = Vec2.Zero;
        _removeList.Clear();
        foreach (Thing t in _follow)
        {
            Vec2 followPos = t.cameraPosition;
            if (t.removeFromLevel)
            {
                _removeList.Add(t);
            }
            if (_prevPositions.ContainsKey(t))
            {
                Vec2 pos = _prevPositions[t];
                if (_overFollow || boost || t.overfollow > 0f)
                {
                    float overfollowVal = 0.3f;
                    if (t.overfollow > 0f)
                    {
                        overfollowVal = t.overfollow;
                    }
                    Vec2 move = (t.cameraPosition - pos) * 24f;
                    if (move.length > 100f)
                    {
                        move = move.normalized * 100f;
                    }
                    Vec2 over = t.cameraPosition + move;
                    followPos = Lerp.Vec2Smooth(followPos, over, overfollowVal);
                }
                if ((pos - followPos).length > 2500f && !_allowWarps)
                {
                    followPos.x = pos.x;
                    followPos.y = pos.y;
                }
                else
                {
                    _prevPositions[t] = t.cameraPosition;
                }
            }
            else
            {
                _prevPositions[t] = t.cameraPosition;
            }
            if (followPos.x < leftmost)
            {
                leftmost = followPos.x;
            }
            if (followPos.x > rightmost)
            {
                rightmost = followPos.x;
            }
            if (followPos.y < topmost)
            {
                topmost = followPos.y;
            }
            if (followPos.y > bottommost)
            {
                bottommost = followPos.y;
            }
            average += followPos;
        }
        foreach (Thing t2 in _removeList)
        {
            Remove(t2);
        }
        _removeList.Clear();
        float topOffset = Level.current.topLeft.y - 64f;
        float bottomOffset = Level.current.bottomRight.y;
        float leftOffset = Level.current.topLeft.x;
        float rightOffset = Level.current.bottomRight.x;
        if (bottommost > bottomOffset)
        {
            bottommost = bottomOffset;
            if (topmost > bottommost)
            {
                topmost = bottommost;
            }
        }
        if (topmost < topOffset)
        {
            topmost = topOffset;
            if (bottommost < topmost)
            {
                bottommost = topmost;
            }
        }
        if (leftmost < leftOffset)
        {
            leftmost = leftOffset;
            if (rightmost < leftmost)
            {
                rightmost = leftmost;
            }
        }
        if (rightmost > rightOffset)
        {
            rightmost = rightOffset;
            if (leftmost > rightmost)
            {
                leftmost = rightmost;
            }
        }
        topmost -= border;
        bottommost += border;
        leftmost -= border;
        rightmost += border;
        float centerMoveX = (leftmost + rightmost) / 2f;
        float centerMoveY = (topmost + bottommost) / 2f;
        float aspect = (float)Resolution.current.x / (float)Resolution.current.y;
        float twidth = Math.Abs(leftmost - rightmost);
        float theight = Math.Abs(topmost - bottommost);
        if (lerpSpeed > 0.9f)
        {
            twidth = Level.current.bottomRight.x - Level.current.topLeft.x;
            theight = Level.current.bottomRight.y - Level.current.topLeft.y;
        }
        float resize = 0f;
        resize = ((!(theight > twidth / aspect)) ? (twidth / aspect) : theight);
        if (!liveDucks && woteFrame)
        {
            resize = _prevViewSize;
        }
        else
        {
            _prevViewSize = resize;
        }
        _viewSize += (resize - _viewSize) * (lerpSpeed * _lerpMult * _speed);
        if (immediate)
        {
            _viewSize = resize;
        }
        float realViewSize = _viewSize;
        if (manualViewSize > 0f)
        {
            realViewSize = manualViewSize;
        }
        base.width = realViewSize * aspect;
        base.height = realViewSize;
        _lerpBorder = Maths.Clamp(Math.Min(base.width, 740f) / 740f * (90f * _zoomMult), minSize * _zoomMult, 90f * _zoomMult);
        if (!liveDucks && woteFrame)
        {
            centerMoveX = _prevCenterMoveX;
            centerMoveY = _prevCenterMoveY;
        }
        else
        {
            _prevCenterMoveX = centerMoveX;
            _prevCenterMoveY = centerMoveY;
        }
        if (!liveDucks)
        {
            woteFrame = true;
        }
        _center.x += (centerMoveX - _center.x) * (lerpSpeed * _lerpMult);
        _center.y += (centerMoveY - _center.y) * (lerpSpeed * _lerpMult);
        if (immediate)
        {
            _center.x = centerMoveX;
            _center.y = centerMoveY;
        }
        if (lerpSpeed > 0.9f && _startCentered)
        {
            _center.x = (Level.current.bottomRight.x + Level.current.topLeft.x) / 2f;
            _center.y = (Level.current.bottomRight.y + Level.current.topLeft.y) / 2f;
        }
        base.x = _center.x - base.width / 2f;
        base.y = _center.y - base.height / 2f;
        if (base.x < hardLimitLeft)
        {
            base.x = hardLimitLeft;
        }
        if (base.right > hardLimitRight)
        {
            base.x = hardLimitRight - base.width;
        }
        if (base.y < hardLimitTop)
        {
            base.y = hardLimitTop;
        }
        if (base.bottom > hardLimitBottom)
        {
            base.y = hardLimitBottom - base.height;
        }
        if (_lerpSpeed > 0.9f)
        {
            _lerpSpeed = 0.05f;
        }
        lerpMult = lmult;
        boost = false;
    }
}
