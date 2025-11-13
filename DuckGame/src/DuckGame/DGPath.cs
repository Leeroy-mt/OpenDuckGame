using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DuckGame;

public struct DGPath
{
	public class DGPathException : Exception
	{
		public DGPathException(string pMessage)
			: base(pMessage)
		{
		}
	}

	public const char Slash = '/';

	private object _specialData;

	private string _path;

	private Dictionary<string, DGPath[]> _filesAndDirectories;

	private DGPath[] _directories;

	private Dictionary<string, DGPath[]> _files;

	private bool _file;

	private bool _rooted;

	private static StringBuilder kBuilder = new StringBuilder();

	public object specialData => _specialData;

	public string path => _path;

	public bool exists
	{
		get
		{
			if (_specialData != null)
			{
				return false;
			}
			if (_file)
			{
				return File.Exists(_path);
			}
			return Directory.Exists(_path);
		}
	}

	/// <summary>
	/// Gets the current directory represented by the path
	/// </summary>
	public DGPath directory
	{
		get
		{
			for (int i = _path.Length - 1; i >= 0; i--)
			{
				if (_path[i] == '/')
				{
					return CopyNewPath(_path.Substring(0, i + 1));
				}
			}
			return _path;
		}
	}

	/// <summary>
	/// Returns directoryName if this is a directory, otherwise returns fileName.
	/// </summary>
	public string name
	{
		get
		{
			if (_specialData != null)
			{
				return _path;
			}
			if (_file)
			{
				return fileName;
			}
			return directoryName;
		}
	}

	public string directoryName
	{
		get
		{
			if (_specialData != null)
			{
				return _path;
			}
			bool twoSlash = false;
			int secondPoint = _path.Length - 1;
			for (int i = _path.Length - 1; i >= 0; i--)
			{
				if (_path[i] == '/')
				{
					if (twoSlash)
					{
						return _path.Substring(i + 1, secondPoint - i - 1);
					}
					twoSlash = true;
					secondPoint = i;
				}
			}
			if (!twoSlash)
			{
				return "";
			}
			return _path.Substring(0, secondPoint);
		}
	}

	public string fileName
	{
		get
		{
			if (_specialData != null)
			{
				return _path;
			}
			if (!_file)
			{
				return "";
			}
			for (int i = _path.Length - 1; i >= 0; i--)
			{
				if (_path[i] == '/')
				{
					return _path.Substring(i + 1, _path.Length - i - 1);
				}
			}
			return _path;
		}
	}

	public bool isDirectory => !_file;

	public bool isFullPath => !_file;

	public DGPath[] GetFilesAndDirectories(params string[] pExtensions)
	{
		if (_filesAndDirectories == null)
		{
			_filesAndDirectories = new Dictionary<string, DGPath[]>();
		}
		string extensionList = "";
		foreach (string s in pExtensions)
		{
			extensionList += s;
		}
		DGPath[] elements = null;
		if (!_filesAndDirectories.TryGetValue(extensionList, out elements))
		{
			List<DGPath> list = new List<DGPath>();
			list.AddRange(GetDirectories());
			list.AddRange(GetFiles(pExtensions));
			elements = list.ToArray();
		}
		if (elements == null)
		{
			elements = new DGPath[0];
		}
		_filesAndDirectories[extensionList] = elements;
		return elements;
	}

	public DGPath[] GetDirectories()
	{
		if (_directories == null)
		{
			if (!isDirectory)
			{
				throw new DGPathException("DGPath.GetDirectories() does not work on file paths, only on directory paths.");
			}
			if (!exists)
			{
				_directories = new DGPath[0];
			}
			else
			{
				List<DGPath> dirList = new List<DGPath>();
				string[] directories = Directory.GetDirectories(_path);
				foreach (string s in directories)
				{
					dirList.Add(s);
				}
				_directories = dirList.ToArray();
			}
		}
		return _directories;
	}

	public DGPath[] GetFiles(params string[] pExtensions)
	{
		if (_files == null)
		{
			_files = new Dictionary<string, DGPath[]>();
		}
		string extensionList = "";
		string[] array = pExtensions;
		foreach (string s in array)
		{
			extensionList += s;
		}
		DGPath[] elements = null;
		if (!_files.TryGetValue(extensionList, out elements))
		{
			if (!isDirectory)
			{
				throw new DGPathException("DGPath.GetFiles() does not work on file paths, only on directory paths.");
			}
			if (!exists)
			{
				elements = new DGPath[0];
			}
			List<DGPath> fileList = new List<DGPath>();
			array = Directory.GetFiles(_path);
			foreach (string s2 in array)
			{
				if (pExtensions.Length != 0)
				{
					foreach (string ext in pExtensions)
					{
						if (s2.EndsWith(ext, StringComparison.InvariantCultureIgnoreCase))
						{
							fileList.Add(s2);
							break;
						}
					}
				}
				else
				{
					fileList.Add(s2);
				}
			}
			elements = fileList.ToArray();
			_files[extensionList] = elements;
		}
		return elements;
	}

	internal void CheckFileValidity(bool pMustBeFile = true, bool pWriting = false)
	{
		if (pMustBeFile && !_file)
		{
			throw new DGPathException("DGPath.ReadText(" + _path + ") failed: path is a directory, not a file.");
		}
		if (pWriting)
		{
			Directory.CreateDirectory(directory);
		}
		else if (!exists)
		{
			throw new DGPathException("DGPath.ReadText(" + _path + ") failed: file does not exist.");
		}
	}

	public DGPath(string pPath)
	{
		bool inSlash = false;
		_file = false;
		_rooted = false;
		_directories = null;
		_files = null;
		_filesAndDirectories = null;
		_specialData = null;
		int i = 0;
		kBuilder.Clear();
		if (pPath.Length > 1 && pPath[1] == ':')
		{
			_rooted = true;
			kBuilder.Append(char.ToUpper(pPath[0]));
			i++;
		}
		for (; i < pPath.Length; i++)
		{
			char c = pPath[i];
			switch (c)
			{
			case '/':
			case '\\':
				if (!inSlash)
				{
					kBuilder.Append('/');
					inSlash = true;
				}
				_file = false;
				continue;
			case '.':
				_file = true;
				break;
			}
			kBuilder.Append(c);
			inSlash = false;
		}
		_path = kBuilder.ToString();
		if (!_file && _path[_path.Length - 1] != '/')
		{
			_path += "/";
		}
	}

	public static DGPath Special(string pName, object pSpecialData)
	{
		return new DGPath
		{
			_path = pName,
			_specialData = pSpecialData
		};
	}

	public DGPath Up()
	{
		bool slash = false;
		for (int i = _path.Length - 1; i >= 0; i--)
		{
			if (_path[i] == '/')
			{
				if (slash)
				{
					return CopyNewPath(_path.Substring(0, i + 1));
				}
				slash = true;
			}
		}
		return this;
	}

	public DGPath Down(string pFolder)
	{
		if (pFolder == null)
		{
			return this;
		}
		return this + pFolder;
	}

	public DGPath Move(string pFile)
	{
		if (pFile == null)
		{
			return this;
		}
		return this + pFile;
	}

	public static implicit operator string(DGPath pPath)
	{
		return pPath._path;
	}

	public static implicit operator DGPath(string pPath)
	{
		return new DGPath(pPath);
	}

	public static bool operator ==(DGPath value1, DGPath value2)
	{
		return value1._path == value2._path;
	}

	public static bool operator !=(DGPath value1, DGPath value2)
	{
		return value1._path != value2._path;
	}

	public static DGPath operator +(DGPath value1, DGPath value2)
	{
		if (value2._rooted)
		{
			throw new DGPathException("(DGPath1 + DGPath2) failed- DGPath2 must be a subfolder and not a fully rooted path (C:/ type paths are not allowed)");
		}
		if (value1._file)
		{
			throw new DGPathException("(DGPath1 + DGPath2) failed- DGPath1 must NOT be a file!");
		}
		return new DGPath
		{
			_path = value1._path + value2._path,
			_rooted = value1._rooted,
			_file = value2._file
		};
	}

	public static bool operator <(DGPath value1, DGPath value2)
	{
		return value2._path.Contains(value1._path);
	}

	public static bool operator >(DGPath value1, DGPath value2)
	{
		return value1._path.Contains(value2._path);
	}

	/// <summary>
	/// Removes the root path from the current path
	/// </summary>
	/// <param name="pRoot">The current root to remove.</param>
	/// <returns>The unrooted path.</returns>
	public DGPath Unroot(DGPath pRoot)
	{
		if (_path.Contains(pRoot._path))
		{
			return CopyNewPath(_path.Replace(pRoot._path, ""));
		}
		return this;
	}

	public static DGPath operator -(DGPath value1, DGPath value2)
	{
		if (value1._path.Length < value2._path.Length)
		{
			return value2.Unroot(value1);
		}
		return value1.Unroot(value2);
	}

	internal DGPath CopyNewPath(string pPath)
	{
		return new DGPath
		{
			_path = pPath,
			_rooted = _rooted
		};
	}

	public override string ToString()
	{
		return _path;
	}

	public void Delete()
	{
		CheckFileValidity(pMustBeFile: false);
		if (_file)
		{
			File.Delete(_path);
		}
		else
		{
			Directory.Delete(_path);
		}
	}

	public string ReadText()
	{
		CheckFileValidity();
		return File.ReadAllText(_path);
	}

	public void WriteText(string pText)
	{
		CheckFileValidity(pMustBeFile: true, pWriting: true);
		File.WriteAllText(_path, pText);
	}

	public string[] ReadLines()
	{
		CheckFileValidity();
		return File.ReadAllLines(_path);
	}

	public void WriteLines(string[] pLines)
	{
		CheckFileValidity(pMustBeFile: true, pWriting: true);
		File.WriteAllLines(_path, pLines);
	}

	public byte[] ReadBytes()
	{
		CheckFileValidity();
		return File.ReadAllBytes(_path);
	}

	public void WriteBytes(byte[] pBytes)
	{
		CheckFileValidity(pMustBeFile: true, pWriting: true);
		File.WriteAllBytes(_path, pBytes);
	}

	public void CreatePath()
	{
		CheckFileValidity(pMustBeFile: true, pWriting: true);
	}
}
