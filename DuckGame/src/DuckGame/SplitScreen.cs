using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace DuckGame;

public class SplitScreen
{
	private static Dictionary<Profile, FollowCam> _cameras = new Dictionary<Profile, FollowCam>();

	public static void Draw()
	{
		Camera c = Level.current.camera;
		foreach (Profile p in Profiles.active)
		{
			Viewport v = Graphics.viewport;
			if (p.duck == null)
			{
				continue;
			}
			FollowCam follow = null;
			if (!_cameras.TryGetValue(p, out follow))
			{
				FollowCam followCam = (_cameras[p] = new FollowCam());
				follow = followCam;
				if (_cameras.Count == 1)
				{
					follow.storedViewport = new Viewport(0, 0, Graphics.viewport.Width / 2 - 2, Graphics.viewport.Height / 2 - 2);
				}
				else if (_cameras.Count == 2)
				{
					follow.storedViewport = new Viewport(Graphics.viewport.Width / 2 + 2, 0, Graphics.viewport.Width / 2 - 2, Graphics.viewport.Height / 2 - 2);
				}
				else if (_cameras.Count == 3)
				{
					follow.storedViewport = new Viewport(0, Graphics.viewport.Height / 2 + 2, Graphics.viewport.Width / 2 - 2, Graphics.viewport.Height / 2 - 2);
				}
				else
				{
					follow.storedViewport = new Viewport(Graphics.viewport.Width / 2 + 2, Graphics.viewport.Height / 2 + 2, Graphics.viewport.Width / 2 - 2, Graphics.viewport.Height / 2 - 2);
				}
			}
			Graphics.viewport = follow.storedViewport;
			follow.minSize = 160f;
			follow.Clear();
			if (p.duck.ragdoll != null)
			{
				follow.Add(p.duck.ragdoll);
			}
			else if (p.duck._trapped != null)
			{
				follow.Add(p.duck._trapped);
			}
			else if (p.duck._cooked != null)
			{
				follow.Add(p.duck._cooked);
			}
			else
			{
				follow.Add(p.duck);
			}
			Level.current.camera = follow;
			follow.lerpSpeed = 0.11f;
			follow.DoUpdate();
			Layer.DrawLayers();
			Graphics.viewport = v;
		}
		Level.current.camera = c;
	}
}
