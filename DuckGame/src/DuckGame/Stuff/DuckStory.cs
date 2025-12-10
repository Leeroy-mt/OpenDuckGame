namespace DuckGame;

public class DuckStory
{
    public delegate void OnStoryBeginDelegate(DuckStory story);

    public string text = "";

    public NewsSection section;

    public event OnStoryBeginDelegate OnStoryBegin;

    public void DoCallback()
    {
        if (this.OnStoryBegin != null)
        {
            this.OnStoryBegin(this);
        }
    }
}
