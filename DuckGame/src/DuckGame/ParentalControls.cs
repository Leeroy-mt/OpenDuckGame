namespace DuckGame;

internal class ParentalControls
{
	private static bool hasCheckedParentalControls;

	private static bool cachedAreParentalControlsActive;

	public static void TestMethod()
	{
		string profaneString = "fu*c????k! this is a badf*(*(ucking phrase.";
		GetLatestProfanityOutputString();
		RunProfanityCheck(ref profaneString);
		GetLatestProfanityOutputString();
	}

	public static int RunProfanityCheck(ref string inOutTextToCheck)
	{
		return 0;
	}

	public static string GetLatestProfanityOutputString()
	{
		return "";
	}

	public static bool AreParentalControlsActive(bool bRestrictionsUI = false)
	{
		return false;
	}

	public static void TryOpenParentalControlsWarning()
	{
	}
}
