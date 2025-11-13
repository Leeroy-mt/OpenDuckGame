using System.Linq;

namespace DuckGame;

public class PhysicsSnapshotBuffer
{
	private PhysicsSnapshotObject[] _frames;

	private int _curFrame;

	private int _numFrames = 120;

	private int _storedFrames;

	public void StoreFrame(PhysicsSnapshotObject frame)
	{
		if (_frames == null)
		{
			_frames = new PhysicsSnapshotObject[_numFrames];
		}
		_frames[_curFrame] = frame;
		_curFrame = (_curFrame + 1) % _frames.Count();
		_storedFrames++;
		if (_storedFrames > _numFrames)
		{
			_storedFrames = _numFrames;
		}
	}

	public PhysicsSnapshotObject GetLatestFrame()
	{
		int frame = _curFrame - 1;
		if (frame < 0)
		{
			frame += _storedFrames;
		}
		return _frames[frame];
	}

	public int GetIndex(PhysicsSnapshotObject frame)
	{
		for (int i = 0; i < _frames.Count(); i++)
		{
			if (frame == _frames[i])
			{
				return i;
			}
		}
		return 0;
	}

	public PhysicsSnapshotObject GetFrame(double time)
	{
		if (_frames == null)
		{
			_frames = new PhysicsSnapshotObject[_numFrames];
		}
		int cur = _curFrame;
		int bestFrame = 0;
		double bestDif = 99999.8984375;
		do
		{
			if (cur > _storedFrames)
			{
				cur = 0;
			}
			if (_frames[cur] == null)
			{
				break;
			}
			double dif = time - _frames[cur].clientTime;
			if (dif < bestDif && dif > 0.0)
			{
				bestFrame = cur;
				bestDif = _frames[cur].clientTime;
			}
			else if (dif > bestDif)
			{
				return _frames[bestFrame];
			}
			cur = (cur + 1) % _storedFrames;
		}
		while (cur != _curFrame);
		return _frames[bestFrame];
	}

	public void FillGap(PhysicsSnapshotObject first, PhysicsSnapshotObject last)
	{
		PhysicsSnapshotObject[] newFrames = new PhysicsSnapshotObject[_numFrames];
		PhysicsSnapshotObject latest = GetLatestFrame();
		int index = 0;
		while (true)
		{
			newFrames[index] = last;
			if (last == first)
			{
				break;
			}
			last = GetNextFrame(last);
		}
		_frames = newFrames;
		_curFrame = GetIndex(latest);
		_storedFrames = index;
	}

	public PhysicsSnapshotObject GetFrame(PhysicsSnapshotObject reference)
	{
		if (_frames == null)
		{
			_frames = new PhysicsSnapshotObject[_numFrames];
		}
		int cur = _curFrame;
		int bestFrame = 0;
		double bestDif = 99999.8984375;
		do
		{
			if (cur > _storedFrames)
			{
				cur = 0;
			}
			if (_frames[cur] == null)
			{
				break;
			}
			double dif = reference.serverTime - _frames[cur].clientTime;
			if (dif < bestDif && dif > 0.0)
			{
				bestFrame = cur;
				bestDif = _frames[cur].clientTime;
			}
			else if (dif > bestDif && _frames[bestFrame].position == reference.position && _frames[bestFrame].velocity == reference.velocity)
			{
				return _frames[bestFrame];
			}
			cur = (cur + 1) % _storedFrames;
		}
		while (cur != _curFrame);
		for (int i = 0; i < 8; i++)
		{
			PhysicsSnapshotObject prev = GetPreviousFrame(_frames[bestFrame]);
			if (prev.position == reference.position && prev.velocity == reference.velocity)
			{
				return prev;
			}
		}
		for (int j = 0; j < 8; j++)
		{
			PhysicsSnapshotObject next = GetNextFrame(_frames[bestFrame]);
			if (next.position == reference.position && next.velocity == reference.velocity)
			{
				return next;
			}
		}
		return _frames[bestFrame];
	}

	public PhysicsSnapshotObject GetNextFrame(PhysicsSnapshotObject frame)
	{
		if (_frames == null)
		{
			_frames = new PhysicsSnapshotObject[_numFrames];
		}
		int cur = _curFrame;
		for (int i = 0; i < _numFrames; i++)
		{
			if (_frames[i] == frame)
			{
				cur = i;
				break;
			}
		}
		cur = (cur + 1) % _storedFrames;
		if (_frames[cur] != null)
		{
			return _frames[cur];
		}
		return frame;
	}

	public PhysicsSnapshotObject GetPreviousFrame(PhysicsSnapshotObject frame)
	{
		if (_frames == null)
		{
			_frames = new PhysicsSnapshotObject[_numFrames];
		}
		int cur = _curFrame;
		for (int i = 0; i < _numFrames; i++)
		{
			if (_frames[i] == frame)
			{
				cur = i;
				break;
			}
		}
		cur--;
		if (cur < 0)
		{
			cur += _storedFrames;
		}
		if (_frames[cur] != null)
		{
			return _frames[cur];
		}
		return frame;
	}
}
