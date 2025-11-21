namespace DuckGame;

public class ConsoleCommand
{
	private string _command;

	public ConsoleCommand(string command)
	{
		_command = command;
	}

	public string NextWord(bool toLower = true, bool peek = false)
	{
		int index = 0;
		if (_command.Length <= 0)
		{
			return "";
		}
		while (_command[index] == ' ')
		{
			index++;
			if (index >= _command.Length)
			{
				return "";
			}
		}
		int wordStart = index;
		while (_command[index] != ' ')
		{
			index++;
			if (index >= _command.Length)
			{
				break;
			}
		}
		string word = _command.Substring(wordStart, index - wordStart);
		if (!peek)
		{
			_command = _command.Substring(index, _command.Length - index);
		}
		if (!toLower)
		{
			return word;
		}
		return word.ToLower();
	}

	public string Remainder(bool toLower = true)
	{
		return (toLower ? _command.ToLower() : _command).Trim();
	}
}
