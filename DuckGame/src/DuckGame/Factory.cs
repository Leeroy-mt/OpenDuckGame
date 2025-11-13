namespace DuckGame;

internal static class Factory<T> where T : new()
{
	private static int kMaxObjects;

	private static T[] _objects;

	private static int _lastActiveObject;

	static Factory()
	{
		kMaxObjects = 1024;
		_objects = new T[kMaxObjects];
		_lastActiveObject = 0;
		for (int i = 0; i < kMaxObjects; i++)
		{
			_objects[i] = new T();
		}
	}

	public static T New()
	{
		T result = _objects[_lastActiveObject];
		_lastActiveObject = (_lastActiveObject + 1) % kMaxObjects;
		return result;
	}
}
