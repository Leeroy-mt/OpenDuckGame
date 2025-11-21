namespace DuckGame;

[FixedNetworkID(8)]
public class NMDisconnect : NMNetworkCoreMessage
{
	public byte error;

	public DuckNetErrorInfo GetError()
	{
		byte e = error;
		DuckNetError duckNetError = (DuckNetError)error;
		return new DuckNetErrorInfo((DuckNetError)e, duckNetError.ToString());
	}

	public NMDisconnect(DuckNetError pError)
	{
		error = (byte)pError;
	}

	public NMDisconnect(byte pError)
	{
		error = pError;
	}

	public NMDisconnect()
	{
	}
}
