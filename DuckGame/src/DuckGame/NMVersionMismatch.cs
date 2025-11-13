namespace DuckGame;

public class NMVersionMismatch : NMDuckNetwork
{
	public enum Type
	{
		Match = -1,
		Older,
		Newer,
		Error
	}

	public byte byteCode;

	public string serverVersion;

	public Type GetCode()
	{
		return (Type)byteCode;
	}

	public NMVersionMismatch()
	{
	}

	public NMVersionMismatch(Type code, string ver)
	{
		byteCode = (byte)code;
		serverVersion = ver;
	}
}
