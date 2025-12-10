using System;
using System.Collections.Generic;

namespace DuckGame;

public class NewsStory
{
    protected float _modifier;

    protected float _importance;

    protected float _value;

    protected float _awfulValue;

    protected float _impressiveValue;

    private string _storyName;

    public string name;

    public string name2;

    public string extra01;

    public string extra02;

    public string remark;

    protected NewsSection _section = NewsSection.MatchComments;

    private List<NewsStory> _subStories = new List<NewsStory>();

    public string remarkModifierString
    {
        get
        {
            if (_impressiveValue - _awfulValue == 0f)
            {
                return "";
            }
            if (badRange > 0.2f)
            {
                return "Bad";
            }
            if (goodRange > 0.2f)
            {
                return "Good";
            }
            return "";
        }
    }

    public float importance => _importance;

    public float weight
    {
        get
        {
            if (_impressiveValue - _awfulValue == 0f)
            {
                return 1f;
            }
            return goodRange + badRange;
        }
    }

    public float goodRange
    {
        get
        {
            float range = Math.Abs(_impressiveValue - _awfulValue);
            if (_impressiveValue < _awfulValue)
            {
                return Maths.Clamp((_impressiveValue + range / 2f - (_impressiveValue + _value)) * 2f / range, 0f, 99f);
            }
            return Maths.Clamp((_value - range / 2f) / (_impressiveValue - range / 2f), 0f, 99f);
        }
    }

    public float badRange
    {
        get
        {
            float range = Math.Abs(_impressiveValue - _awfulValue);
            if (_impressiveValue < _awfulValue)
            {
                return Maths.Clamp((_value - range / 2f) / (_awfulValue - range / 2f), 0f, 99f);
            }
            return Maths.Clamp((_awfulValue + range / 2f - (_awfulValue + _value)) * 2f / range, 0f, 99f);
        }
    }

    public NewsSection section => _section;

    public NewsStory FromName(string storyName, string name1Val = null, string name2Val = null, string extra01Val = null, string extra02Val = null)
    {
        NewsStory obj = new NewsStory
        {
            name = name1Val,
            name2 = name2Val,
            extra01 = extra01Val,
            extra02 = extra02Val,
            _storyName = storyName
        };
        obj._importance = obj.importance;
        obj._section = _section;
        return obj;
    }

    public void AddSubStory(NewsStory story)
    {
        if (story.DoCalculateRemark() != null)
        {
            _subStories.Add(story);
        }
    }

    public void DoCalculate(List<Team> teams)
    {
        _value = 0f;
        _storyName = null;
        name = null;
        name2 = null;
        extra01 = null;
        extra02 = null;
        remark = null;
        Calculate(teams);
    }

    protected virtual void Calculate(List<Team> teams)
    {
    }

    public string DoCalculateRemark()
    {
        remark = CalculateRemark();
        return remark;
    }

    protected virtual string CalculateRemark()
    {
        return Dialogue.GetRemark((_storyName != null) ? _storyName : (GetType().Name + remarkModifierString), name, name2, extra01, extra02);
    }
}
