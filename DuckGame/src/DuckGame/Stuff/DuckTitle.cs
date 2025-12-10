using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DuckGame;

public class DuckTitle
{
    private static List<DuckTitle> _titles = new List<DuckTitle>();

    private Dictionary<PropertyInfo, float> _requirementsFloat = new Dictionary<PropertyInfo, float>();

    private Dictionary<PropertyInfo, int> _requirementsInt = new Dictionary<PropertyInfo, int>();

    private Dictionary<PropertyInfo, string> _requirementsString = new Dictionary<PropertyInfo, string>();

    private string _name;

    private string _previousOwner;

    public string previousOwner
    {
        get
        {
            return _previousOwner;
        }
        set
        {
            _previousOwner = value;
        }
    }

    public static void Initialize()
    {
        string[] files = Content.GetFiles("Content/titles");
        for (int i = 0; i < files.Length; i++)
        {
            IEnumerable<DXMLNode> root = DuckXML.Load(DuckFile.OpenStream(files[i])).Elements("Title");
            if (root == null)
            {
                continue;
            }
            DXMLAttribute nameAttrib = root.Attributes("name").FirstOrDefault();
            if (nameAttrib == null)
            {
                continue;
            }
            DuckTitle newTitle = new DuckTitle();
            newTitle._name = nameAttrib.Value;
            bool broke = false;
            foreach (DXMLNode element in root.Elements())
            {
                if (element.Name == "StatRequirement")
                {
                    DXMLAttribute statNameAttrib = element.Attributes("name").FirstOrDefault();
                    DXMLAttribute statValueAttrib = element.Attributes("value").FirstOrDefault();
                    if (statNameAttrib == null || statValueAttrib == null)
                    {
                        broke = true;
                        break;
                    }
                    PropertyInfo info = typeof(ProfileStats).GetProperties().FirstOrDefault((PropertyInfo x) => x.Name == statNameAttrib.Value);
                    if (!(info != null))
                    {
                        broke = true;
                        break;
                    }
                    if (info.GetType() == typeof(float))
                    {
                        newTitle._requirementsFloat.Add(info, Change.ToSingle(statValueAttrib.Value));
                    }
                    else if (info.GetType() == typeof(int))
                    {
                        newTitle._requirementsFloat.Add(info, Convert.ToInt32(statValueAttrib.Value));
                    }
                    else
                    {
                        newTitle._requirementsString.Add(info, statValueAttrib.Value);
                    }
                }
            }
            if (!broke)
            {
                _titles.Add(newTitle);
            }
        }
    }

    public static DuckTitle GetTitle(string title)
    {
        return _titles.FirstOrDefault((DuckTitle x) => x._name == title);
    }

    public static Dictionary<DuckTitle, float> ScoreTowardsTitles(Profile p)
    {
        Dictionary<DuckTitle, float> retTitles = new Dictionary<DuckTitle, float>();
        foreach (DuckTitle t in _titles)
        {
            retTitles[t] = t.ScoreTowardsTitle(p);
        }
        return retTitles;
    }

    public float ScoreTowardsTitle(Profile p)
    {
        float totalFloatReq = 0f;
        float currentFloatReq = 0f;
        foreach (KeyValuePair<PropertyInfo, float> req in _requirementsFloat)
        {
            totalFloatReq += req.Value;
            currentFloatReq += (float)req.Key.GetValue(p.stats, null);
        }
        int totalIntReq = 0;
        int currentIntReq = 0;
        foreach (KeyValuePair<PropertyInfo, int> req2 in _requirementsInt)
        {
            totalIntReq += req2.Value;
            currentIntReq += (int)req2.Key.GetValue(p.stats, null);
        }
        int totalStringReq = 0;
        int currentStringReq = 0;
        foreach (KeyValuePair<PropertyInfo, string> req3 in _requirementsString)
        {
            totalStringReq++;
            if ((string)req3.Key.GetValue(p.stats, null) == req3.Value)
            {
                currentStringReq++;
            }
        }
        return (currentFloatReq + (float)currentIntReq + (float)currentStringReq) / (totalFloatReq + (float)totalIntReq + (float)totalStringReq);
    }

    public void UpdateTitles()
    {
        foreach (DuckTitle title in _titles)
        {
            _ = title;
        }
    }
}
