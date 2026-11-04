namespace DuckGame;

/// <summary>
/// When implemented in a derived class, defines <see cref="DeathCrate"/>'s activation event.
/// </summary>
public abstract class DeathCrateSetting
{
    public float likelyhood = 1;

    public virtual void Update(DeathCrate pCrate) { }

    public abstract void Activate(DeathCrate c, bool server = true);
}