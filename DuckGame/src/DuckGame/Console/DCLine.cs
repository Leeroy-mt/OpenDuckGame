using System;

namespace DuckGame;

public class DCLine
{
    public string line;

    public Color color;

    public int threadIndex;

    public DateTime timestamp;

    public DCSection section;

    public Verbosity verbosity;

    public int frames;

    public override string ToString()
    {
        return line + " | " + timestamp.ToLongTimeString();
    }

    public string ToSendString()
    {
        return timestamp.ToLongTimeString() + " " + SectionString() + " " + color.ToDGColorString() + line + "\n";
    }

    public string ToShortString()
    {
        return SectionString() + " " + line + "\n";
    }

    public static Color ColorForSection(DCSection s)
    {
        return s switch
        {
            DCSection.NetCore => Colors.DGBlue,
            DCSection.DuckNet => Colors.DGPink,
            DCSection.Steam => Colors.DGOrange,
            DCSection.GhostMan => Colors.DGPurple,
            DCSection.Mod => Colors.DGGreen,
            DCSection.Connection => Colors.DGYellow,
            DCSection.Ack => Colors.DGGreen,
            _ => Color.White,
        };
    }

    public static string StringForSection(DCSection s, bool colored, bool small, bool formatting = true)
    {
        if (formatting)
        {
            if (small)
            {
                if (colored)
                {
                    switch (s)
                    {
                        case DCSection.NetCore:
                            return "|DGBLUE|NC  ";
                        case DCSection.DuckNet:
                            return "|PINK|DN  ";
                        case DCSection.Steam:
                            return "|DGORANGE|ST  ";
                        case DCSection.GhostMan:
                            return "|DGPURPLE|GM  ";
                        case DCSection.Mod:
                            return "|DGGREEN|MD  ";
                        case DCSection.Connection:
                            return "|DGYELLOW|CN  ";
                        case DCSection.Ack:
                            return "|DGGREEN|AK  ";
                    }
                }
                else
                {
                    switch (s)
                    {
                        case DCSection.NetCore:
                            return "NC  ";
                        case DCSection.DuckNet:
                            return "DN  ";
                        case DCSection.Steam:
                            return "ST  ";
                        case DCSection.GhostMan:
                            return "GM  ";
                        case DCSection.Mod:
                            return "MD  ";
                        case DCSection.Connection:
                            return "CN  ";
                        case DCSection.Ack:
                            return "AK  ";
                    }
                }
            }
            if (colored)
            {
                switch (s)
                {
                    case DCSection.NetCore:
                        return "|DGBLUE|NETCORE  ";
                    case DCSection.DuckNet:
                        return "|PINK|DUCKNET  ";
                    case DCSection.Steam:
                        return "|DGORANGE|STEAM    ";
                    case DCSection.GhostMan:
                        return "|DGPURPLE|GHOSTMAN ";
                    case DCSection.Mod:
                        return "|DGGREEN|MOD      ";
                    case DCSection.Connection:
                        return "|DGYELLOW|CONNECT  ";
                    case DCSection.Ack:
                        return "|DGGREEN|ACK      ";
                }
            }
            else
            {
                switch (s)
                {
                    case DCSection.NetCore:
                        return "NETCORE  ";
                    case DCSection.DuckNet:
                        return "DUCKNET  ";
                    case DCSection.Steam:
                        return "STEAM    ";
                    case DCSection.GhostMan:
                        return "GHOSTMAN ";
                    case DCSection.Mod:
                        return "MOD      ";
                    case DCSection.Connection:
                        return "CONNECT  ";
                    case DCSection.Ack:
                        return "ACK      ";
                }
            }
        }
        else
        {
            if (small)
            {
                if (colored)
                {
                    switch (s)
                    {
                        case DCSection.NetCore:
                            return "|DGBLUE|NC";
                        case DCSection.DuckNet:
                            return "|PINK|DN";
                        case DCSection.Steam:
                            return "|DGORANGE|ST";
                        case DCSection.GhostMan:
                            return "|DGPURPLE|GM";
                        case DCSection.Mod:
                            return "|DGGREEN|MD";
                        case DCSection.Connection:
                            return "|DGYELLOW|CN";
                        case DCSection.Ack:
                            return "|DGGREEN|AK";
                    }
                }
                else
                {
                    switch (s)
                    {
                        case DCSection.NetCore:
                            return "NC";
                        case DCSection.DuckNet:
                            return "DN";
                        case DCSection.Steam:
                            return "ST";
                        case DCSection.GhostMan:
                            return "GM";
                        case DCSection.Mod:
                            return "MD";
                        case DCSection.Connection:
                            return "CN";
                        case DCSection.Ack:
                            return "AK";
                    }
                }
            }
            if (colored)
            {
                switch (s)
                {
                    case DCSection.NetCore:
                        return "|DGBLUE|NETCORE";
                    case DCSection.DuckNet:
                        return "|PINK|DUCKNET";
                    case DCSection.Steam:
                        return "|DGORANGE|STEAM";
                    case DCSection.GhostMan:
                        return "|DGPURPLE|GHOSTMAN";
                    case DCSection.Mod:
                        return "|DGGREEN|MOD";
                    case DCSection.Connection:
                        return "|DGYELLOW|CONNECT";
                    case DCSection.Ack:
                        return "|DGGREEN|ACK";
                }
            }
            else
            {
                switch (s)
                {
                    case DCSection.NetCore:
                        return "NETCORE";
                    case DCSection.DuckNet:
                        return "DUCKNET";
                    case DCSection.Steam:
                        return "STEAM";
                    case DCSection.GhostMan:
                        return "GHOSTMAN";
                    case DCSection.Mod:
                        return "MOD";
                    case DCSection.Connection:
                        return "CONNECT";
                    case DCSection.Ack:
                        return "ACK";
                }
            }
        }
        return "";
    }

    public string SectionString(bool colored = true, bool small = false)
    {
        return StringForSection(section, colored, small);
    }
}
