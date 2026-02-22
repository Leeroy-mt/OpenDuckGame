using Microsoft.Xna.Framework;

namespace DuckGame;

public interface ILoadingScreen
{
    bool LoadingStarted { get; }

    void Start();

    void Update(GameTime gameTime);

    void Draw(GameTime gameTime);
}
