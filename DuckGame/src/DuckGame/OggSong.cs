using System.IO;

namespace DuckGame;

public class OggSong
{
	public static MemoryStream Load(string oggFile, bool localContent = true)
	{
		oggFile = oggFile.TrimStart('/');
		Stream st = null;
		st = ((!localContent) ? File.OpenRead(oggFile) : DuckFile.OpenStream(oggFile));
		MemoryStream mst = new MemoryStream();
		CopyStream(st, mst);
		st.Close();
		return mst;
	}

	public static void CopyStream(Stream input, Stream output)
	{
		byte[] buffer = new byte[16384];
		int read;
		while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
		{
			output.Write(buffer, 0, read);
		}
	}
}
