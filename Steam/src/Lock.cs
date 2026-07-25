internal class Lock : IDisposable
{
    readonly object m_pObject;

    #region Constructor & Destructor

    public Lock(object pObject)
    {
        m_pObject = pObject;
        Monitor.Enter(m_pObject);
    }

    ~Lock()
    {
        Dispose(true);
    }

    #endregion

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
            Monitor.Exit(m_pObject);
    }
}