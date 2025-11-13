using System;
using System.Collections.Generic;
using System.Linq;

namespace DuckGame;

public static class RandomChallengeDownloader
{
	private static WorkshopQueryAll _currentQuery = null;

	public static bool ready = false;

	public static int numToHaveReady = 10;

	public static int numSinceLowRating = 0;

	private static List<LevelData> _readyChallenges = new List<LevelData>();

	private static int _toFetchIndex = -1;

	private static int _numFetch = 0;

	private static WorkshopItem _downloading;

	private static WorkshopQueryFilterOrder _orderMode = WorkshopQueryFilterOrder.RankedByVote;

	public static LevelData GetNextChallenge()
	{
		if (_readyChallenges.Count == 0)
		{
			return null;
		}
		LevelData result = _readyChallenges.First();
		_readyChallenges.RemoveAt(0);
		return result;
	}

	public static LevelData PeekNextChallenge()
	{
		if (_readyChallenges.Count == 0)
		{
			return null;
		}
		return _readyChallenges.First();
	}

	private static void Fetched(object sender, WorkshopQueryResult result)
	{
		if (_toFetchIndex == -1)
		{
			_toFetchIndex = Rando.Int((int)(sender as WorkshopQueryAll)._numResultsFetched);
		}
		if (_toFetchIndex == _numFetch && Steam.DownloadWorkshopItem(result.details.publishedFile))
		{
			_downloading = result.details.publishedFile;
		}
		_currentQuery = null;
		_numFetch++;
	}

	private static void FinishedTotalQuery(object sender)
	{
		WorkshopQueryAll fin = sender as WorkshopQueryAll;
		if (fin._numResultsTotal != 0)
		{
			int page = Rando.Int((int)(fin._numResultsTotal / 50)) + 1;
			if (numSinceLowRating > 3)
			{
				numSinceLowRating = 0;
			}
			else
			{
				page %= 10;
			}
			if (numSinceLowRating == 2)
			{
				_orderMode = WorkshopQueryFilterOrder.RankedByTrend;
			}
			else
			{
				_orderMode = WorkshopQueryFilterOrder.RankedByVote;
			}
			if (page == 0)
			{
				page = 1;
			}
			numSinceLowRating++;
			WorkshopQueryAll workshopQueryAll = Steam.CreateQueryAll(_orderMode, WorkshopType.Items);
			workshopQueryAll.requiredTags.Add("Arcade Machine");
			workshopQueryAll.ResultFetched += Fetched;
			workshopQueryAll._page = (uint)page;
			workshopQueryAll.justOnePage = true;
			workshopQueryAll.Request();
		}
	}

	private static void SearchDirLevels(string dir, LevelLocation location)
	{
		string[] array = ((location == LevelLocation.Content) ? Content.GetFiles(dir) : DuckFile.GetFiles(dir, "*.*"));
		for (int i = 0; i < array.Length; i++)
		{
			ProcessChallenge(array[i], location);
		}
		array = ((location == LevelLocation.Content) ? Content.GetDirectories(dir) : DuckFile.GetDirectories(dir));
		for (int i = 0; i < array.Length; i++)
		{
			SearchDirLevels(array[i], location);
		}
	}

	private static void ProcessChallenge(string path, LevelLocation location)
	{
		Main.SpecialCode = (("Loading Challenge " + path != null) ? path : "null");
		try
		{
			if (!path.EndsWith(".lev"))
			{
				return;
			}
			path = path.Replace('\\', '/');
			LevelData dat = null;
			dat = DuckFile.LoadLevel(path);
			dat.SetPath(path);
			path = path.Substring(0, path.Length - 4);
			path.Substring(path.IndexOf("/levels/") + 8);
			bool canLoad = true;
			if (dat.modData.workshopIDs.Count != 0)
			{
				foreach (ulong id in dat.modData.workshopIDs)
				{
					bool found = false;
					foreach (Mod m in ModLoader.accessibleMods)
					{
						if (m.configuration != null && m.configuration.workshopID == id)
						{
							found = true;
							break;
						}
					}
					if (!found)
					{
						canLoad = false;
						break;
					}
				}
			}
			if (canLoad && !dat.modData.hasLocalMods)
			{
				_readyChallenges.Add(dat);
				DevConsole.Log(DCSection.Steam, "Downloaded random challenge " + _readyChallenges.Count + "/" + numToHaveReady);
			}
			else
			{
				DevConsole.Log(DCSection.Steam, "Downloaded challenge had incompatible mods, and was ignored!");
			}
		}
		catch (Exception)
		{
		}
	}

	//public static void Update()
	//{
	//	if (!Steam.IsInitialized() || !Network.isServer)
	//	{
	//		return;
	//	}
	//	if (_downloading != null)
	//	{
	//		if (_downloading.finishedProcessing)
	//		{
	//			if (_downloading.downloadResult == SteamResult.OK)
	//			{
	//				SearchDirLevels(_downloading.path, LevelLocation.Workshop);
	//			}
	//			_downloading = null;
	//		}
	//	}
	//	else if (_currentQuery == null && _readyChallenges.Count < numToHaveReady)
	//	{
	//		_toFetchIndex = -1;
	//		_numFetch = 0;
	//		_currentQuery = Steam.CreateQueryAll(_orderMode, WorkshopType.Items);
	//		_currentQuery.requiredTags.Add("Arcade Machine");
	//		_currentQuery.QueryFinished += FinishedTotalQuery;
	//		_currentQuery.fetchedData = WorkshopQueryData.TotalOnly;
	//		_currentQuery.Request();
	//		DevConsole.Log(DCSection.Steam, "Querying for random Challenges.");
	//	}
	//}
}
