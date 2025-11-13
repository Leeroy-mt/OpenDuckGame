using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DuckGame;

public class News
{
	private static List<NewsStory> _availableStories = new List<NewsStory>();

	public static void Initialize()
	{
		Type newsType = typeof(NewsStory);
		foreach (Type t in from c in Assembly.GetAssembly(typeof(NewsStory)).GetTypes()
			where newsType.IsAssignableFrom(c)
			select c)
		{
			_availableStories.Add(Activator.CreateInstance(t) as NewsStory);
		}
	}

	public static List<NewsStory> GetStories()
	{
		Stats.CalculateStats();
		List<Team> teams = Teams.active;
		List<NewsStory> stories = new List<NewsStory>();
		foreach (NewsStory story in _availableStories)
		{
			story.DoCalculate(teams);
			stories.Add(story);
		}
		FilterBest(stories, NewsSection.MatchComments, 1);
		FilterBest(stories, NewsSection.PlayerComments, 2);
		stories.Sort((NewsStory a, NewsStory b) => (a.section != b.section) ? ((a.section >= b.section) ? 1 : (-1)) : 0);
		return stories;
	}

	public static void FilterBest(List<NewsStory> stories, NewsSection section, int numToPick)
	{
		List<NewsStory> list = stories.Where((NewsStory x) => x.section == section).ToList();
		list.OrderBy((NewsStory x) => x.weight * x.importance);
		int index = 0;
		foreach (NewsStory story in list)
		{
			if (index >= numToPick)
			{
				stories.Remove(story);
			}
			index++;
		}
	}
}
