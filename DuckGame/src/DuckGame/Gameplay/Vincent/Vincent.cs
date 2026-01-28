using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DuckGame;

public class Vincent
{
    public static Vincent context = new Vincent();

    public static List<VincentProduct> products = new List<VincentProduct>();

    public static float alpha = 0f;

    public static bool lookingAtList = false;

    public static bool lookingAtChallenge = false;

    public static bool hover = false;

    private static FancyBitmapFont _font;

    private static BitmapFont _priceFont;

    private static BitmapFont _priceFontCrossout;

    private static BitmapFont _priceFontRightways;

    private static BitmapFont _descriptionFont;

    private static SpriteMap _dealer;

    private static Sprite _tail;

    private static Sprite _photo;

    private static SpriteMap _newSticker;

    private static SpriteMap _rareSticker;

    private static Sprite _soldSprite;

    private static List<string> _lines = new List<string>();

    private static DealerMood _mood;

    private static string _currentLine = "";

    private static List<TextLine> _lineProgress = new List<TextLine>();

    private static float _waitLetter = 1f;

    private static float _waitAfterLine = 1f;

    private static float _talkMove = 0f;

    private static float _showLerp = 0f;

    private static bool _allowMovement = false;

    private static bool _hasYoYo = true;

    public static DayType type;

    public static bool show = false;

    public static bool hasKid = false;

    public static bool openedCorners = false;

    private static float _afterShowWait = 0f;

    public static bool _willGiveYoYo = false;

    public static bool _willGiveVooDoo = false;

    public static bool _willGivePerimeterDefence = false;

    private static float _listLerp = 0f;

    private static float _challengeLerp = 0f;

    private static float _chancyLerp = 0f;

    public static Sprite _furniFrame;

    public static Sprite _furniFill;

    public static Sprite _furniHov;

    public static SpriteMap _furniTag;

    public static Sprite _cheapTape;

    public static Sprite _bigBanner;

    public static Sprite _fancyBanner;

    private static List<RenderTarget2D> _priceTargets = new List<RenderTarget2D>();

    private static int _soldSelectIndex = -1;

    private static bool killSkip = false;

    private static float _extraWait = 0f;

    public static int _giveTickets = 0;

    public static bool afterChallenge = false;

    public static float afterChallengeWait = 0f;

    private static List<ChallengeData> _chancyChallenges = new List<ChallengeData>();

    public static bool showingDay = false;

    private static string lastWord = "";

    private static int wait = 0;

    public static int _selectIndex = -1;

    public static int _selectDescIndex = -2;

    public static int frame
    {
        get
        {
            if (_mood == DealerMood.Concerned)
            {
                return _dealer.frame - 6;
            }
            if (_mood == DealerMood.Point)
            {
                return _dealer.frame - 3;
            }
            return _dealer.frame;
        }
        set
        {
            if (_mood == DealerMood.Concerned)
            {
                _dealer.frame = value + 6;
            }
            else if (_mood == DealerMood.Point)
            {
                _dealer.frame = value + 3;
            }
            else
            {
                _dealer.frame = value;
            }
        }
    }

    public static void Clear()
    {
        _lines.Clear();
        _lineProgress.Clear();
        _waitLetter = 0f;
        _waitAfterLine = 0f;
        _currentLine = "";
        _mood = DealerMood.Normal;
    }

    public static void Add(string line)
    {
        _lines.Add(line);
    }

    public static int SortSaleDayFurniture(Furniture t1, Furniture t2)
    {
        float val1 = Rando.Float(1f) - (float)Profiles.experienceProfile.GetNumFurnitures(t1.index) * 1f;
        float val2 = Rando.Float(1f) - (float)Profiles.experienceProfile.GetNumFurnitures(t2.index) * 1f;
        if (val1 > val2)
        {
            return 1;
        }
        if (val1 < val2)
        {
            return -1;
        }
        return 0;
    }

    public static void Open(DayType t)
    {
        _lineProgress.Clear();
        show = false;
        _afterShowWait = 0f;
        _showLerp = 0f;
        _allowMovement = false;
        _waitAfterLine = 1f;
        _waitLetter = 1f;
        _mood = DealerMood.Normal;
        _chancyLerp = 0f;
        hasKid = false;
        _allowMovement = false;
        _afterShowWait = 0f;
        show = false;
        if (Profiles.experienceProfile == null)
        {
            return;
        }
        openedCorners = false;
        Music.Play("vincent");
        if (!_hasYoYo)
        {
            _dealer = new SpriteMap("vincentNoYo", 113, 106);
        }
        else
        {
            _dealer = new SpriteMap("vincent", 113, 106);
        }
        _selectDescIndex = -2;
        _selectIndex = -1;
        hasKid = false;
        type = t;
        switch (t)
        {
            case DayType.SaleDay:
                if (Profiles.experienceProfile.timesMetVincentSale == 0)
                {
                    Add("|CALM|Hey!|2| |POINT|You're new to this so let me explain.");
                    Add("|CALM|At the end of every month I have a super sale.");
                    Add("|CONCERNED|Where I sell stuff I can't move, |POINT|at WAY LOW PRICES!");
                    Add("|CALM||SHOW|Check it out!");
                }
                else
                {
                    List<List<string>> obj = new List<List<string>>
                {
                    new List<string> { "|CONCERNED|Hey, guess what? |POINT||SHOW|It's SALE DAY!" },
                    new List<string> { "|CALM|Dang, I hope you're ready.. |SHOW|For |POINT|INSANE SAVINGS." },
                    new List<string> { "|CALM|+SAAALE |SHOW|DAAAAY+!" },
                    new List<string> { "|CALM|Oh wow, now here's some |SHOW|quality stuff." }
                };
                    foreach (string item in obj[Rando.Int(obj.Count - 1)])
                    {
                        Add(item);
                    }
                }
                hasKid = new List<int>
            {
                0, 1, 2, 3, 4, 6, 9, 12, 18, 28,
                51, 52, 53, 80, 140
            }.Contains(Profiles.experienceProfile.timesMetVincentSale);
                Profiles.experienceProfile.timesMetVincentSale++;
                break;
            case DayType.ImportDay:
                if (Profiles.experienceProfile.timesMetVincentImport == 0)
                {
                    Add("|CALM|OK. On the first day of every month I have a |POINT||GREEN|SPECIAL SALE|WHITE|!");
                    Add("|CALM|Today I sell only the |POINT||GREEN|FANCIEST IMPORTS|GREEN|.");
                    Add("|CONCERNED|This stuff aint cheap!|POINT||SHOW| See anything you like?");
                }
                else
                {
                    List<List<string>> intros3 = new List<List<string>>
                {
                    new List<string> { "|POINT|Hey hey hey!", "|POINT||SHOW|It's FANCY IMPORTS day!" },
                    new List<string> { "|CALM|Ahh, Fancy Imports day!", "|CALM|Hope you're ready for some |SHOW|EXOTIC FURNITURE." },
                    new List<string> { "|CALM|W-what's that?", "|POINT|OOH-HUHUU! |SHOW|Is that IMPORTED?" }
                };
                    if (Rando.Int(40) == 0)
                    {
                        intros3.Add(new List<string> { "|POINT|Fancy fancy imports day!", "|POINT|Get ready to overpay baby!|SHOW|" });
                    }
                    else if (Rando.Int(100) == 0)
                    {
                        intros3.Add(new List<string> { "|CONCERNED|Hmm, I actually had this stuff reserved for the Duck Duke of the Dark Spire..", "|POINT|But hey you're basically him so,|SHOW| have at 'er." });
                    }
                    foreach (string item2 in intros3[Rando.Int(intros3.Count - 1)])
                    {
                        Add(item2);
                    }
                }
                hasKid = new List<int> { 3, 6, 9 }.Contains(Profiles.experienceProfile.timesMetVincentImport);
                Profiles.experienceProfile.timesMetVincentImport++;
                break;
            case DayType.PawnDay:
                {
                    if (Profiles.experienceProfile.timesMetVincentSell == 0)
                    {
                        Add("|CALM|I keep an eye out on all the furniture that goes around here.");
                        Add("|CALM|Every second Wednesday I'll come see |POINT|if you have anything I like!");
                        Add("|CONCERNED|If something catches my eye I'll try to buy it from you..");
                        Add("|POINT|So!");
                    }
                    List<Furniture> possible = (from x in Profiles.experienceProfile.GetAvailableFurnis()
                                                where Profiles.experienceProfile.GetNumFurnitures(x.index) > Profiles.experienceProfile.GetNumFurnituresPlaced(x.index) && x.@group != Furniture.Momento && x.@group != Furniture.Default
                                                select x).ToList();
                    if (possible.Count() > 0)
                    {
                        possible = possible.OrderBy((Furniture x) => Rando.Float(1f) - Math.Min((float)Profiles.experienceProfile.GetNumFurnitures(x.index) / 10f, 1f) * 0.5f).ToList();
                        int sel = Rando.Int(3);
                        products.Clear();
                        VincentProduct p = new VincentProduct();
                        p.type = VPType.Furniture;
                        p.furnitureData = possible.First();
                        p.originalCost = p.furnitureData.price;
                        p.cost = (int)((float)p.furnitureData.price * 2.5f);
                        if (Rando.Float(1f) > 0.95f)
                        {
                            p.cost = (int)((float)p.furnitureData.price * 4f);
                        }
                        if (p.furnitureData.name == "ROUND TABLE" && Rando.Float(1f) > 0.5f)
                        {
                            Add("|POINT|Oh baby, is that one of them ROUND tables??");
                            Add("|CONCERNED| Can I buy it from you?|SHOW| I'm so tired of square tables...");
                        }
                        else
                        {
                            switch (sel)
                            {
                                case 0:
                                    Add("|POINT|I like the look of this,|SHOW| you wanna sell it?");
                                    break;
                                case 1:
                                    Add("|POINT|This is pretty cool,|SHOW| ya still in love with it? Cause I am!");
                                    break;
                                case 2:
                                    Add("|POINT|I haven't seen one of these in ages!|SHOW| Can I buy it from you?");
                                    break;
                                case 3:
                                    Add("|POINT|I mean, why do you even have one of these!?|SHOW| I can take it off your hands..");
                                    break;
                            }
                        }
                        products.Add(p);
                    }
                    else
                    {
                        Add("|CALM|Let's see what I could buy from you...");
                        Add("|CONCERNED|Looks like you don't have anything I want, sorry!|SHOW|");
                    }
                    hasKid = new List<int> { 4, 8, 10, 22, 45 }.Contains(Profiles.experienceProfile.timesMetVincentSell);
                    Profiles.experienceProfile.timesMetVincentSell++;
                    break;
                }
            case DayType.HintDay:
                {
                    products.Clear();
                    List<List<string>> intros2 = new List<List<string>>
            {
                new List<string> { "|CALM|Hey, word is that there's a hat you don't have yet...", "|POINT|and I caught word of how to unlock it-" },
                new List<string> { "|CALM|I just heard a rumour about how to unlock a new hat-" },
                new List<string> { "|CONCERNED|Probably shouldn't be telling you this, but-", "|POINT|I heard of a way to unlock a new hat!" },
                new List<string> { "|CONCERNED|PSST!", "|POINT|They say you can unlock a new hat if you just do this-" }
            };
                    if (Rando.Int(10) == 0)
                    {
                        intros2.Add(new List<string> { "|CALM|Hats my man, that's what life's all about!", "|POINT|And I happen to know how you can get a new one-" });
                    }
                    List<List<string>> outros = new List<List<string>>
            {
                new List<string> { "|CONCERNED|Huh, Doesn't sound too hard...|SHOW|" },
                new List<string> { "|CONCERNED|Now why would anybody wanna do that?|SHOW|" },
                new List<string> { "|CONCERNED|Sounds like a real pain...|SHOW|" },
                new List<string> { "|POINT|It's just that easy!|SHOW|" },
                new List<string> { "|CONCERNED|Even I could do that...|SHOW|" },
                new List<string> { "|CALM|That's just what I heard, anyway.|SHOW|" },
                new List<string> { "|CALM|+ |DGBLUE|So easy!|WHITE| +|SHOW|" },
                new List<string> { "|CONCERNED|Geez... Glad I don't have to do that.|SHOW|" }
            };
                    if (Rando.Int(10) == 0)
                    {
                        outros.Add(new List<string> { "|CALM|Whatever that means, right?|SHOW|" });
                        outros.Add(new List<string> { "|CALM|No problem, Eh?|SHOW|" });
                        if (Profiles.experienceProfile.timesMetVincent > 10)
                        {
                            outros.Add(new List<string> { "|POINT|See? Don't even need the wiki.|SHOW|" });
                        }
                    }
                    else if (Rando.Int(100) == 0)
                    {
                        outros.Add(new List<string> { "|CALM|Yep.. Good luck with that!|SHOW|" });
                        if (Profiles.experienceProfile.timesMetVincent > 15)
                        {
                            outros.Add(new List<string> { "|CALM|Hey, I'd do it for you if I had time...|SHOW|" });
                        }
                        outros.Add(new List<string> { "|CONCERNED|Hmm.. Do you really wanna do all that?|SHOW|" });
                    }
                    else if (Rando.Int(10000) == 0)
                    {
                        outros.Add(new List<string> { "|POINT|If you want my advice, just edit your save file..|SHOW|" });
                        outros.Add(new List<string> { "|POINT|Hmpf.. Nobody's got time for that!|SHOW|" });
                    }
                    if (Rando.Int(100) == 0)
                    {
                        intros2.Clear();
                        intros2.Add(new List<string> { "|CALM|Hey, my fortune cookie gave me a hint on how to unlock a new hat-" });
                        outros.Clear();
                        outros.Add(new List<string> { "|CONCERNED|That's all well and good but.. what's my fortune, then?|SHOW|" });
                    }
                    else if (Rando.Int(100) == 0 && Profiles.experienceProfile.timesMetVincent > 12)
                    {
                        intros2.Clear();
                        intros2.Add(new List<string> { "|POINT|Since we're buds I'll let you in on the secret to unlocking a new hat:" });
                        outros.Clear();
                        outros.Add(new List<string> { "|CONCERNED|That info's just between you and me, right?|SHOW|" });
                    }
                    else if (Rando.Int(30) == 0)
                    {
                        intros2.Clear();
                        intros2.Add(new List<string> { "|POINT|Hey.. Wanna know how to unlock a new hat?", "|CALM|Someone scratched this hint into the wall of the crapper-" });
                        outros.Clear();
                        outros.Add(new List<string> { "|CONCERNED|Huh? No way- I didn't scratch it in there, I use a permanent marker.|SHOW|" });
                    }
                    List<string> l = intros2[Rando.Int(intros2.Count - 1)];
                    List<Unlockable> unlocks = Unlockables.lockedItems;
                    Global.data.unlockListIndex += 1 + Rando.Int(10);
                    if (unlocks.Count > 0)
                    {
                        int idx = Global.data.unlockListIndex % unlocks.Count;
                        Unlockable u = unlocks[idx];
                        l.Add("'|GREEN|" + u.description + "|WHITE|'");
                    }
                    foreach (string s in outros[Rando.Int(outros.Count - 1)])
                    {
                        l.Add(s);
                    }
                    foreach (string item3 in l)
                    {
                        Add(item3);
                    }
                    hasKid = new List<int>
            {
                3, 5, 7, 10, 14, 19, 25, 35, 50, 80,
                120, 121, 122, 180, 300
            }.Contains(Profiles.experienceProfile.timesMetVincentHint);
                    Profiles.experienceProfile.timesMetVincentHint++;
                    break;
                }
            case DayType.Special:
                Add("|CALM|I've got something special for you today, |SHOW||POINT|Check it out!");
                break;
            default:
                hasKid = new List<int>
            {
                1, 3, 5, 9, 11, 16, 19, 31, 45, 50,
                51, 52, 60, 62, 64, 66, 68, 70, 90, 110,
                150, 200, 300, 500
            }.Contains(Profiles.experienceProfile.timesMetVincent);
                if (Profiles.experienceProfile.timesMetVincent >= 19 && Profiles.experienceProfile.GetNumFurnitures(RoomEditor.GetFurniture("YOYO").index) <= 0)
                {
                    _willGiveYoYo = true;
                    Add("|CONCERNED|You know what? |CALM|You're my best customer.");
                    Add("|POINT|Hands down. You are awesome");
                    Add("|CONCERNED|I want you to have this...|2||GIVE|");
                    Add("|POINT|Now then, |SHOW|Here's what I got!");
                    hasKid = false;
                }
                else if (Profiles.experienceProfile.numLittleMen > 0 && Profiles.experienceProfile.GetNumFurnitures(RoomEditor.GetFurniture("VOODOO VINCENT").index) <= 0)
                {
                    _willGiveVooDoo = true;
                    Add("|CONCERNED|Hey, I've got something special for you today..");
                    Add("|POINT|I'm required by Duck Game Law to give it to you.");
                    Add("|CONCERNED|Lets see here, I know I've got it around here somewhere.. |2||GIVE|");
                    Add("|POINT|If you put it in your room it'll give you the option to.. |CONCERNED|Er.. Well uh.. to make me go away.");
                    Add("|CALM|You'll still get new Furni and XP if you skip the level up screen, but you'll miss my handsome mug.");
                    Add("|POINT|Now then, |SHOW|Here's what I've got for today!");
                    hasKid = false;
                }
                else if (UILevelBox.currentLevel > 2 && Profiles.experienceProfile.punished >= 10 && Profiles.experienceProfile.GetNumFurnitures(RoomEditor.GetFurniture("PERIMETER DEFENCE").index) <= 0)
                {
                    _willGivePerimeterDefence = true;
                    Add("|CONCERNED|Hey.. I noticed other Ducks have been comin' into your room and givin' you a hard time.");
                    Add("|CONCERNED|That's not cool, you got a little man's safety to worry about!");
                    Add("|POINT|I'm a bit of a pacifist, but I bought this thing on sale and I want you to have it.");
                    Add("|CONCERNED|Let's see here...|3||GIVE|");
                    Add("|CALM|That thing's just to protect the home room, okay? Be careful with it!");
                    Add("|POINT|Now then, |SHOW|Here's what I've got for today!");
                    hasKid = false;
                }
                else if (Profiles.experienceProfile.timesMetVincent == 0)
                {
                    Add("|POINT|Hey, I'm Vincent and I sell toys.");
                    Add("|CALM|I'm around every |GREEN|Friday|WHITE| and have new stuff every week.");
                    Add("|CONCERNED|My stuff is a sure thing too, no gambling here.|2|");
                    Add("|POINT|SO! |SHOW|See anything you like?");
                }
                else if (Profiles.experienceProfile.timesMetVincent == 1)
                {
                    Add("|POINT|HEY!");
                    Add("|CALM|This is my son, |DGBLUE|Mini Vinny|WHITE|.");
                    Add("|CALM|He's helpin' me sell toys.");
                    Add("|POINT|His mom|3||CONCERNED| would|1| rather he |RED|didn't|WHITE|.|3|");
                    Add("|CONCERNED|But|2||CALM| this is |DGBLUE|our time|WHITE|. |SHOW|and ol' |DGBLUE|mini|WHITE| |GREEN|LOVES|WHITE| sellin' toys.");
                }
                else if (Profiles.experienceProfile.timesMetVincent == 7)
                {
                    Add("|CONCERNED|Here|SHOW| you go...");
                }
                else if (Profiles.experienceProfile.timesMetVincent == 8)
                {
                    Add("|POINT|Dang, It's good to see you.");
                    Add("|CONCERNED|You're my best customer, |SHOW|you know that?");
                }
                else if (Profiles.experienceProfile.timesMetVincent == 13)
                {
                    Add("|CALM|Friday is my favourite day 'cause we get to hang out.");
                    Add("|POINT|I'm not just sayin' that|SHOW| cause you buy stuff!");
                }
                else if (Profiles.experienceProfile.timesMetVincent == 22)
                {
                    Add("|CONCERNED|Today was a dang mess, but this makes it all worthwhile.");
                    Add("|POINT|+Sellin' |SHOW|toys is THE BEST+");
                }
                else
                {
                    List<List<string>> intros = new List<List<string>>
                {
                    new List<string> { "|CONCERNED|I hope you're ready,|2| cause today I've got...|0|", "|POINT|The |SHOW||GREEN|GREATEST PRODUCT LINEUP IN VINCENT HISTORY|WHITE|." },
                    new List<string> { "|CALM|Just got back from the dump.", "|POINT|Hope you like |GREEN|VALUE|WHITE|.|SHOW|" },
                    new List<string> { "|CONCERNED|Where do I even find this stuff?", "|CALM|You just can't find stuff like this|2| |SHOW||POINT|ANY. WHERE. ELSE." },
                    new List<string> { "|CALM|Look at all this stuff, |CONCERENED|I don't even want to sell it.", "|POINT||SHOW|I wanna keep this stuff, It's just too cool." },
                    new List<string> { "|CALM|Look at that. |GREEN|4 GOOD REASONS|WHITE||2| |SHOW|why today is a good day." },
                    new List<string> { "|POINT|I just finished putting prices on everything.", "|CONCERNED|WHOOPS! Looks like I accidentally|SHOW| made this stuff way too cheap..." }
                };
                    if (Profiles.experienceProfile.timesMetVincent > 2 && Rando.Int(100) == 0)
                    {
                        intros.Add(new List<string> { "|CALM|+GONNA BUY MYSELF |SHOW|A GREY GUITAR+" });
                    }
                    if (Profiles.experienceProfile.timesMetVincent > 2 && Rando.Int(100) == 0)
                    {
                        intros.Clear();
                        intros.Add(new List<string> { "|CALM|+BING AND BONG+                +|SHOW|THERE'S A TINY PLANET CALLING+" });
                    }
                    if (((Profiles.experienceProfile.timesMetVincent > 15 && Profiles.experienceProfile.timesMetVincent < 45) || (Profiles.experienceProfile.timesMetVincent > 100 && Profiles.experienceProfile.timesMetVincent < 125)) && Rando.Int(10000) == 0)
                    {
                        intros.Add(new List<string> { "|CALM|+I'M GOIN TO CALIFORNIA+ |SHOW|+GONNA LIVE THE LIFE+" });
                    }
                    if (((Profiles.experienceProfile.timesMetVincent > 30 && Profiles.experienceProfile.timesMetVincent < 45) || (Profiles.experienceProfile.timesMetVincent > 100 && Profiles.experienceProfile.timesMetVincent < 125)) && Rando.Int(10000) == 0)
                    {
                        intros.Add(new List<string> { "|CALM|+BACKSTREET'S BACK+ |SHOW|+ALRIGHT!!+" });
                    }
                    foreach (string item4 in intros[Rando.Int(intros.Count - 1)])
                    {
                        Add(item4);
                    }
                }
                Profiles.experienceProfile.timesMetVincent++;
                break;
        }
        if (Profiles.experienceProfile.timesMetVincent < 1)
        {
            hasKid = false;
        }
        if (t != DayType.PawnDay && t != DayType.HintDay)
        {
            GenerateProducts();
        }
    }

    public static void GenerateProducts()
    {
        products.Clear();
        if (type == DayType.Shop)
        {
            foreach (Furniture f in UIGachaBox.GetRandomFurniture(Rarity.VeryRare, 4, 0.4f, gacha: false, 1))
            {
                VincentProduct p = new VincentProduct();
                p.type = VPType.Furniture;
                p.furnitureData = f;
                if (Rando.Int(120) == 0)
                {
                    p.cost = (int)((float)p.furnitureData.price * 0.5f);
                    p.originalCost = p.furnitureData.price;
                }
                else
                {
                    p.cost = (p.originalCost = p.furnitureData.price);
                }
                products.Add(p);
            }
            return;
        }
        if (type == DayType.ImportDay)
        {
            IOrderedEnumerable<Furniture> orderedEnumerable = from x in UIGachaBox.GetRandomFurniture(Rarity.VeryVeryRare, 8, 0.4f, gacha: false, 0, avoidDupes: true)
                                                              orderby -x.rarity
                                                              select x;
            int num = 0;
            {
                foreach (Furniture f2 in orderedEnumerable)
                {
                    VincentProduct p2 = new VincentProduct();
                    p2.type = VPType.Furniture;
                    p2.furnitureData = f2;
                    if (Rando.Int(50) == 0)
                    {
                        p2.furnitureData = UIGachaBox.GetRandomFurniture(Rarity.SuperRare, 1, 0.3f)[0];
                        p2.cost = (int)((float)p2.furnitureData.price * 2f);
                        p2.originalCost = p2.furnitureData.price;
                    }
                    else
                    {
                        p2.cost = (int)((float)p2.furnitureData.price * 1.5f);
                        p2.originalCost = p2.furnitureData.price;
                    }
                    products.Add(p2);
                    num++;
                    if (num == 4)
                    {
                        break;
                    }
                }
                return;
            }
        }
        if (type == DayType.SaleDay)
        {
            foreach (Furniture f3 in UIGachaBox.GetRandomFurniture(Rarity.Common, 4, 2f, gacha: false, 0, avoidDupes: false, rareDupesChance: true))
            {
                VincentProduct p3 = new VincentProduct();
                p3.type = VPType.Furniture;
                p3.furnitureData = f3;
                p3.cost = (int)((float)p3.furnitureData.price * 0.5f);
                p3.originalCost = p3.furnitureData.price;
                products.Add(p3);
            }
            return;
        }
        if (type != DayType.Special)
        {
            return;
        }
        IEnumerable<Team> teams = new List<Team>
        {
            Teams.GetTeam("CYCLOPS"),
            Teams.GetTeam("BIG ROBO"),
            Teams.GetTeam("TINCAN"),
            Teams.GetTeam("WELDERS"),
            Teams.GetTeam("PONYCAP"),
            Teams.GetTeam("TRICORNE"),
            Teams.GetTeam("TWINTAIL"),
            Teams.GetTeam("HIGHFIVES"),
            Teams.GetTeam("MOTHERS")
        }.Where((Team x) => !Global.boughtHats.Contains(x.name));
        if (teams.Count() <= 0)
        {
            foreach (Furniture f4 in UIGachaBox.GetRandomFurniture(Rarity.VeryVeryRare, 1, 0.4f))
            {
                VincentProduct p4 = new VincentProduct();
                p4.type = VPType.Furniture;
                p4.furnitureData = f4;
                p4.cost = (int)((float)p4.furnitureData.price * 0.5f);
                p4.originalCost = p4.furnitureData.price;
                products.Add(p4);
            }
            return;
        }
        VincentProduct p5 = new VincentProduct();
        p5.type = VPType.Hat;
        p5.cost = (p5.originalCost = 150);
        p5.teamData = teams.ElementAt(Rando.Int(teams.Count() - 1));
        products.Add(p5);
    }

    public static void Initialize()
    {
        if (_tail == null)
        {
            _dealer = new SpriteMap("vincent", 113, 106);
            _tail = new Sprite("arcade/bubbleTail");
            _font = new FancyBitmapFont("smallFont");
            _priceFont = new BitmapFont("biosFontSideways", 8);
            _priceFontCrossout = new BitmapFont("biosFontSidewaysCrossout", 8);
            _priceFontRightways = new BitmapFont("priceFontRightways", 8);
            _descriptionFont = new BitmapFont("biosFontDescriptions", 8);
            _photo = new Sprite("arcade/challengePhoto");
            _furniFrame = new Sprite("furniFrame");
            _furniFrame.CenterOrigin();
            _furniFill = new Sprite("furniFill");
            _furniFill.CenterOrigin();
            _furniHov = new Sprite("furniHov");
            _furniHov.CenterOrigin();
            _soldSprite = new Sprite("soldStamp");
            _soldSprite.CenterOrigin();
            _newSticker = new SpriteMap("newSticker", 29, 28);
            _newSticker.CenterOrigin();
            _rareSticker = new SpriteMap("rareSticker", 29, 28);
            _rareSticker.CenterOrigin();
            _furniTag = new SpriteMap("furniTag", 14, 51);
            _cheapTape = new Sprite("cheapTape");
            _cheapTape.CenterOrigin();
            _bigBanner = new Sprite("bigBanner");
            _fancyBanner = new Sprite("fancyBanner");
            _priceTargets.Add(new RenderTarget2D(64, 16));
            _priceTargets.Add(new RenderTarget2D(64, 16));
            _priceTargets.Add(new RenderTarget2D(64, 16));
            _priceTargets.Add(new RenderTarget2D(64, 16));
        }
    }

    public static void ChangeSpeech()
    {
        Clear();
        _selectDescIndex = _selectIndex;
        if (products[_selectIndex].sold)
        {
            if (_soldSelectIndex == _selectIndex)
            {
                if (products.Where((VincentProduct x) => !x.sold).Count() == 0)
                {
                    Add("|CONCERNED|WOAH...");
                    Add("|POINT|YOU BOUGHT EVERYTHING!");
                }
                else
                {
                    List<string> intros = new List<string> { "|CONCERNED|AND THERE YOU GO...|0|", "|CONCERNED|FINALLY GOT RID OF IT!", "|CALM|SOLD!" };
                    if (Profiles.experienceProfile.timesMetVincent > 10)
                    {
                        intros.Add("|POINT|AND NOW ITS YOURS!");
                        intros.Add("|CALM|SOLD!");
                        if (Rando.Int(10) == 0)
                        {
                            intros.Add("|CONCERNED|GLAD SOMEONE LIKES THAT SORT OF THING..");
                        }
                    }
                    if (Profiles.experienceProfile.timesMetVincent > 30)
                    {
                        intros.Add("|POINT|TAKE GOOD CARE OF IT!");
                        intros.Add("|CONCERNED|HOPE YOU LIKE IT.");
                        intros.Add("|POINT|NOW THAT'S GOOD TASTE!");
                    }
                    Add(intros[Rando.Int(intros.Count - 1)]);
                }
                _soldSelectIndex = -1;
            }
            else
            {
                Add("|CONCERNED|GONNA MISS THAT ONE.");
                _soldSelectIndex = -1;
            }
        }
        else
        {
            string desc = products[_selectIndex].description;
            if (desc == "")
            {
                desc = ((products[_selectIndex].furnitureData != null) ? "What a fine piece of furniture." : "What a fine hat.");
            }
            _ = products[_selectIndex].type;
            Add(string.Concat(desc + "^|ORANGE|Part of the '" + products[_selectIndex].group + "' line.|WHITE| ", "|DGGREEN|$", Convert.ToString(products[_selectIndex].cost)));
        }
    }

    public static void Sold()
    {
        products[FurniShopScreen.attemptBuyIndex].sold = true;
        _soldSelectIndex = FurniShopScreen.attemptBuyIndex;
    }

    public static void Update()
    {
        if (_hasYoYo && Profiles.experienceProfile.timesMetVincent > 19 && !FurniShopScreen.giveYoYo && !_willGiveYoYo)
        {
            _dealer = new SpriteMap("vincentNoYo", 113, 106);
            _hasYoYo = false;
        }
        else if (!_hasYoYo && Profiles.experienceProfile.timesMetVincent < 20)
        {
            _dealer = new SpriteMap("vincent", 113, 106);
            _hasYoYo = true;
        }
        Initialize();
        if (UILevelBox.menuOpen)
        {
            return;
        }
        _showLerp = Lerp.FloatSmooth(_showLerp, show ? 1f : 0f, 0.09f, 1.05f);
        bool lerpList = lookingAtList && _challengeLerp < 0.3f;
        bool lerpChallenge = lookingAtChallenge && _listLerp < 0.3f;
        bool lerpChancy = FurniShopScreen.open && _listLerp < 0.3f;
        _listLerp = Lerp.FloatSmooth(_listLerp, lerpList ? 1f : 0f, 0.2f, 1.05f);
        _challengeLerp = Lerp.FloatSmooth(_challengeLerp, lerpChallenge ? 1f : 0f, 0.2f, 1.05f);
        _chancyLerp = Lerp.FloatSmooth(_chancyLerp, lerpChancy ? 1f : 0f, 0.2f, 1.05f);
        if (FurniShopScreen.open)
        {
            alpha = Lerp.Float(alpha, 1f, 0.05f);
        }
        else
        {
            alpha = Lerp.Float(alpha, 0f, 0.05f);
        }
        if (!FurniShopScreen.open || showingDay)
        {
            return;
        }
        if (!openedCorners)
        {
            openedCorners = true;
            HUD.ClearCorners();
            if (Profiles.experienceProfile.GetNumFurnituresPlaced(RoomEditor.GetFurniture("VOODOO VINCENT").index) > 0 && !_willGiveVooDoo && !_willGiveYoYo && !_willGivePerimeterDefence)
            {
                HUD.AddCornerControl(HUDCorner.TopLeft, "@START@SKIP");
                HUD.AddCornerMessage(HUDCorner.BottomLeft, "@CANCEL@DITCH");
            }
        }
        if (Input.Pressed("START") && Profiles.experienceProfile.GetNumFurnituresPlaced(RoomEditor.GetFurniture("VOODOO VINCENT").index) > 0 && !_willGiveVooDoo && !_willGiveYoYo && !_willGivePerimeterDefence)
        {
            Graphics.fadeAdd = 1f;
            SFX.skip = false;
            SFX.Play("dacBang");
            FurniShopScreen.close = true;
            return;
        }
        if (_allowMovement)
        {
            if ((Input.Pressed("ANY") || (type == DayType.PawnDay && products.Count > 0)) && _selectIndex == -1)
            {
                _selectIndex = 0;
                if (products.Count == 0)
                {
                    FurniShopScreen.close = true;
                }
                return;
            }
            if (Input.Pressed("MENULEFT"))
            {
                if (_selectIndex == -1)
                {
                    _selectIndex = 0;
                }
                else if (_selectIndex == 3)
                {
                    _selectIndex = 2;
                    SFX.Play("textLetter", 0.7f);
                }
                else if (_selectIndex == 1)
                {
                    _selectIndex = 0;
                    SFX.Play("textLetter", 0.7f);
                }
            }
            if (Input.Pressed("MENURIGHT"))
            {
                if (_selectIndex == -1)
                {
                    _selectIndex = 0;
                }
                else if (_selectIndex == 2)
                {
                    _selectIndex = 3;
                    SFX.Play("textLetter", 0.7f);
                }
                else if (_selectIndex == 0)
                {
                    _selectIndex = 1;
                    SFX.Play("textLetter", 0.7f);
                }
            }
            if (Input.Pressed("MENUUP"))
            {
                if (_selectIndex == -1)
                {
                    _selectIndex = 0;
                }
                else if (_selectIndex == 2)
                {
                    _selectIndex = 0;
                    SFX.Play("textLetter", 0.7f);
                }
                else if (_selectIndex == 3)
                {
                    _selectIndex = 1;
                    SFX.Play("textLetter", 0.7f);
                }
            }
            if (Input.Pressed("MENUDOWN"))
            {
                if (_selectIndex == -1)
                {
                    _selectIndex = 0;
                }
                else if (_selectIndex == 1)
                {
                    _selectIndex = 3;
                    SFX.Play("textLetter", 0.7f);
                }
                else if (_selectIndex == 0)
                {
                    _selectIndex = 2;
                    SFX.Play("textLetter", 0.7f);
                }
            }
            if (_selectIndex >= products.Count)
            {
                _selectIndex = products.Count - 1;
            }
            if (Input.Pressed("SELECT") && _selectIndex != -1 && !products[_selectIndex].sold && (type == DayType.PawnDay || products[_selectIndex].cost <= Profiles.experienceProfile.littleManBucks))
            {
                FurniShopScreen.attemptBuy = products[_selectIndex];
                _selectDescIndex = -1;
                FurniShopScreen.attemptBuyIndex = _selectIndex;
                HUD.CloseCorner(HUDCorner.BottomLeft);
                HUD.AddCornerMessage(HUDCorner.BottomMiddle, "|YELLOW|$" + Profiles.experienceProfile.littleManBucks);
                if (products[_selectIndex].furnitureData != null)
                {
                    HUD.AddCornerMessage(HUDCorner.BottomRight, "|DGBLUE|HAVE " + Profiles.experienceProfile.GetNumFurnitures(products[_selectIndex].furnitureData.index));
                }
                return;
            }
            if (_selectDescIndex != _selectIndex)
            {
                if (_selectIndex == -1)
                {
                    HUD.CloseAllCorners();
                    HUD.AddCornerMessage(HUDCorner.BottomRight, "@SELECT@CONTINUE");
                    _selectDescIndex = _selectIndex;
                }
                else
                {
                    if (type != DayType.PawnDay)
                    {
                        ChangeSpeech();
                    }
                    HUD.AddCornerMessage(HUDCorner.BottomMiddle, "|YELLOW|$" + Profiles.experienceProfile.littleManBucks);
                    int numOwned = ((products[_selectIndex].furnitureData != null) ? Profiles.experienceProfile.GetNumFurnitures(products[_selectIndex].furnitureData.index) : 0);
                    if (type == DayType.PawnDay)
                    {
                        if (products[_selectIndex].sold)
                        {
                            HUD.AddCornerMessage(HUDCorner.BottomRight, "SOLD");
                        }
                        else
                        {
                            string cornerMessage = "@SELECT@SELL";
                            if (numOwned > 1)
                            {
                                cornerMessage = cornerMessage + "|DGBLUE|(HAVE " + numOwned + ")";
                            }
                            HUD.AddCornerMessage(HUDCorner.BottomRight, cornerMessage);
                        }
                    }
                    else if (products[_selectIndex].sold)
                    {
                        HUD.AddCornerMessage(HUDCorner.BottomRight, "BOUGHT");
                    }
                    else
                    {
                        string cornerMessage2 = ((products[_selectIndex].cost > Profiles.experienceProfile.littleManBucks) ? "@SELECT@|RED|BUY" : "@SELECT@BUY");
                        if (numOwned > 1)
                        {
                            cornerMessage2 = cornerMessage2 + "|DGBLUE|(HAVE " + numOwned + ")";
                        }
                        HUD.AddCornerMessage(HUDCorner.BottomRight, cornerMessage2);
                    }
                    HUD.AddCornerMessage(HUDCorner.BottomLeft, "@CANCEL@EXIT");
                    _selectDescIndex = _selectIndex;
                }
            }
        }
        bool turbo = !_allowMovement && Input.Down("SELECT");
        if (_lines.Count > 0 && _currentLine == "")
        {
            bool num = _waitAfterLine <= 0f;
            _waitAfterLine -= 0.045f;
            if (turbo)
            {
                _waitAfterLine -= 0.045f;
            }
            if (killSkip)
            {
                _waitAfterLine -= 0.1f;
            }
            _talkMove += 0.75f;
            if (_talkMove > 1f)
            {
                frame = 0;
                _talkMove = 0f;
            }
            if (!num && _waitAfterLine <= 0f)
            {
                HUD.AddCornerMessage(HUDCorner.BottomRight, "@SELECT@CONTINUE");
            }
            if (_lineProgress.Count == 0 || Input.Pressed("SELECT"))
            {
                _lineProgress.Clear();
                _currentLine = _lines[0];
                _lines.RemoveAt(0);
                _waitAfterLine = 1.3f;
                if (show)
                {
                    _allowMovement = true;
                }
                killSkip = false;
            }
        }
        if (_currentLine != "")
        {
            _waitLetter -= 0.9f;
            if (turbo)
            {
                _waitLetter -= 1.8f;
            }
            if (_waitLetter < 0f)
            {
                _talkMove += 0.75f;
                if (_talkMove > 1f)
                {
                    if (_currentLine[0] != ' ' && frame == 1 && _extraWait <= 0f)
                    {
                        frame = 2;
                    }
                    else
                    {
                        frame = 1;
                    }
                    _talkMove = 0f;
                }
                _waitLetter = 1f;
                while (_currentLine[0] == '@')
                {
                    string val = _currentLine[0].ToString() ?? "";
                    _currentLine = _currentLine.Remove(0, 1);
                    while (_currentLine[0] != '@' && _currentLine.Length > 0)
                    {
                        val += _currentLine[0];
                        _currentLine = _currentLine.Remove(0, 1);
                    }
                    _currentLine = _currentLine.Remove(0, 1);
                    val += "@";
                    _lineProgress[0].Add(val);
                    _waitLetter = 3f;
                    if (_currentLine.Length == 0)
                    {
                        _currentLine = "";
                        return;
                    }
                }
                float addWaitLetter = 0f;
                while (_currentLine[0] == '|')
                {
                    _currentLine = _currentLine.Remove(0, 1);
                    string read = "";
                    while (_currentLine[0] != '|' && _currentLine.Length > 0)
                    {
                        read += _currentLine[0];
                        _currentLine = _currentLine.Remove(0, 1);
                    }
                    bool end = false;
                    if (_currentLine.Length <= 1)
                    {
                        _currentLine = "";
                        end = true;
                    }
                    else
                    {
                        _currentLine = _currentLine.Remove(0, 1);
                    }
                    Color c = Color.White;
                    bool foundColor = false;
                    switch (read)
                    {
                        case "RED":
                            foundColor = true;
                            c = Color.Red;
                            break;
                        case "WHITE":
                            foundColor = true;
                            c = Color.White;
                            break;
                        case "BLUE":
                            foundColor = true;
                            c = Color.Blue;
                            break;
                        case "ORANGE":
                            foundColor = true;
                            c = new Color(235, 137, 51);
                            break;
                        case "YELLOW":
                            foundColor = true;
                            c = new Color(247, 224, 90);
                            break;
                        case "GREEN":
                            foundColor = true;
                            c = Color.LimeGreen;
                            break;
                        case "CONCERNED":
                            _mood = DealerMood.Concerned;
                            addWaitLetter = 2f;
                            break;
                        case "POINT":
                            _mood = DealerMood.Point;
                            addWaitLetter = 2f;
                            break;
                        case "CALM":
                            _mood = DealerMood.Normal;
                            addWaitLetter = 2f;
                            break;
                        case "SHOW":
                            show = true;
                            break;
                        case "GIVE":
                            if (_willGiveYoYo)
                            {
                                _willGiveYoYo = false;
                                FurniShopScreen.giveYoYo = true;
                            }
                            else if (_willGiveVooDoo)
                            {
                                _willGiveVooDoo = false;
                                FurniShopScreen.giveVooDoo = true;
                            }
                            else if (_willGivePerimeterDefence)
                            {
                                _willGivePerimeterDefence = false;
                                FurniShopScreen.givePerimeterDefence = true;
                            }
                            break;
                        case "0":
                            killSkip = true;
                            break;
                        case "1":
                            addWaitLetter = 5f;
                            break;
                        case "2":
                            addWaitLetter = 10f;
                            break;
                        case "3":
                            addWaitLetter = 15f;
                            break;
                    }
                    if (foundColor)
                    {
                        if (_lineProgress.Count == 0)
                        {
                            _lineProgress.Insert(0, new TextLine
                            {
                                lineColor = c
                            });
                        }
                        else
                        {
                            _lineProgress[0].SwitchColor(c);
                        }
                    }
                    if (end)
                    {
                        return;
                    }
                }
                string nextWord = "";
                int index = 1;
                if (_currentLine[0] == ' ')
                {
                    while (index < _currentLine.Length && _currentLine[index] != ' ' && _currentLine[index] != '^')
                    {
                        if (_currentLine[index] == '|')
                        {
                            for (index++; index < _currentLine.Length && _currentLine[index] != '|'; index++)
                            {
                            }
                            index++;
                        }
                        else if (_currentLine[index] == '@')
                        {
                            for (index++; index < _currentLine.Length && _currentLine[index] != '@'; index++)
                            {
                            }
                            index++;
                        }
                        else
                        {
                            nextWord += _currentLine[index];
                            index++;
                        }
                    }
                }
                if (_lineProgress.Count == 0 || _currentLine[0] == '^' || (_currentLine[0] == ' ' && _lineProgress[0].Length() + nextWord.Length > 34))
                {
                    Color c2 = Color.White;
                    if (_lineProgress.Count > 0)
                    {
                        c2 = _lineProgress[0].lineColor;
                    }
                    _lineProgress.Insert(0, new TextLine
                    {
                        lineColor = c2
                    });
                    if (_currentLine[0] == ' ' || _currentLine[0] == '^')
                    {
                        _currentLine = _currentLine.Remove(0, 1);
                    }
                }
                else
                {
                    if (_currentLine[0] == '!' || _currentLine[0] == '?' || _currentLine[0] == '.')
                    {
                        _waitLetter = 5f;
                    }
                    else if (_currentLine[0] == ',')
                    {
                        _waitLetter = 3f;
                    }
                    _lineProgress[0].Add(_currentLine[0]);
                    char c3 = _currentLine[0].ToString().ToLowerInvariant()[0];
                    if (wait > 0)
                    {
                        wait--;
                    }
                    if ((c3 < 'a' || c3 > 'z') && (c3 < '0' || c3 > '9') && c3 != '\'' && lastWord != "")
                    {
                        CRC32.Generate(lastWord.Trim());
                        lastWord = "";
                    }
                    else
                    {
                        lastWord += c3;
                    }
                    if (wait > 0)
                    {
                        wait--;
                    }
                    else
                    {
                        wait = 2;
                        SFX.Play("tinyTick", 0.4f, 0.2f);
                    }
                    _currentLine = _currentLine.Remove(0, 1);
                }
                _waitLetter += addWaitLetter;
            }
        }
        else
        {
            if (show)
            {
                _afterShowWait += 0.12f;
                if (_afterShowWait >= 1f)
                {
                    _allowMovement = true;
                }
            }
            _talkMove += 0.75f;
            if (_talkMove > 1f)
            {
                frame = 0;
                _talkMove = 0f;
            }
        }
        string p = "";
        for (int i = 0; i < products.Count; i++)
        {
            p += 9;
            Camera cam = new Camera(0f, 0f, 64f, 16f);
            Graphics.SetRenderTarget(_priceTargets[i]);
            DepthStencilState state = new DepthStencilState
            {
                StencilEnable = true,
                StencilFunction = CompareFunction.Always,
                StencilPass = StencilOperation.Replace,
                ReferenceStencil = 1,
                DepthBufferEnable = false
            };
            Graphics.Clear(new Color(0, 0, 0, 0));
            Graphics.screen.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp, state, RasterizerState.CullNone, null, cam.getMatrix());
            string priceString = "$" + Math.Min(Math.Max(products[i].cost, 0), 9999);
            _furniTag.frame = priceString.Length - 1;
            _priceFontRightways.Draw(priceString, new Vec2((float)(5 - priceString.Length) / 5f * 20f, 0f), (products[i].cost > Profiles.experienceProfile.littleManBucks) ? Colors.DGRed : Color.Black, 0.97f);
            Graphics.screen.End();
            Graphics.SetRenderTarget(null);
        }
    }

    public static void Draw()
    {
        new Vec2(-200f + _listLerp * 270f, 20f);
        if (_challengeLerp < 0.01f && _chancyLerp < 0.01f)
        {
            return;
        }
        Vec2 dealerOffset = new Vec2(100f * (1f - _chancyLerp), 100f * (1f - _chancyLerp) - 4f);
        Vec2 descSize = new Vec2(280f, 30f);
        Vec2 descPos = new Vec2(20f, 132f) + dealerOffset;
        Graphics.DrawRect(descPos + new Vec2(-2f, 0f), descPos + descSize + new Vec2(2f, 0f), Color.Black, 0.96f);
        int index = 0;
        for (int i = _lineProgress.Count - 1; i >= 0; i--)
        {
            float wide = Graphics.GetStringWidth(_lineProgress[i].text);
            float ypos = descPos.Y + 2f + (float)(index * 9);
            float xpos = descPos.X + descSize.X / 2f - wide / 2f;
            for (int j = _lineProgress[i].segments.Count - 1; j >= 0; j--)
            {
                _descriptionFont.Draw(_lineProgress[i].segments[j].text, new Vec2(xpos, ypos), _lineProgress[i].segments[j].color, 0.97f);
                xpos += (float)(_lineProgress[i].segments[j].text.Length * 8);
            }
            index++;
        }
        _tail.flipV = true;
        Graphics.Draw(_tail, 222f + dealerOffset.X, 117f + dealerOffset.Y);
        if (hasKid)
        {
            _dealer.frame += 9;
        }
        _dealer.Depth = 0.96f;
        _dealer.Alpha = alpha;
        Graphics.Draw(_dealer, 200f + dealerOffset.X, 26f + dealerOffset.Y);
        if (type == DayType.SaleDay)
        {
            _bigBanner.Depth = 0.96f;
            Graphics.Draw(_bigBanner, 22f, -80f + _showLerp * 100f);
            Graphics.Draw(_bigBanner, 194f, -80f + _showLerp * 100f);
        }
        else if (type == DayType.ImportDay)
        {
            _fancyBanner.Depth = 0.96f;
            Graphics.Draw(_fancyBanner, 22f, -80f + _showLerp * 100f);
            Graphics.Draw(_fancyBanner, 194f, -80f + _showLerp * 100f);
        }
        _furniFrame.Alpha = alpha;
        _cheapTape.Alpha = alpha * 0.9f;
        _furniFill.Alpha = alpha;
        _furniHov.Alpha = alpha;
        _furniTag.Alpha = alpha;
        _newSticker.Alpha = alpha;
        _rareSticker.Alpha = alpha;
        _soldSprite.Alpha = alpha;
        Vec2 furniPos = new Vec2(84f, 46f);
        _cheapTape.Depth = 0.968f;
        _furniFrame.Depth = 0.96f;
        _furniFill.Depth = 0.965f;
        _furniHov.Depth = 0.965f;
        _furniTag.Depth = 0.972f;
        _newSticker.Depth = 0.972f;
        _rareSticker.Depth = 0.972f;
        _soldSprite.Depth = 0.975f;
        if (products.Count > 0)
        {
            int idx = 0;
            Vec2 framePos = new Vec2(furniPos.X - 200f + Math.Min(_showLerp * (float)(200 + 40 * idx), 200f), furniPos.Y);
            if (products.Count == 1)
            {
                framePos = new Vec2(furniPos.X - 200f + Math.Min(_showLerp * 275f, 240f), furniPos.Y + 30f);
            }
            Graphics.Draw(_furniFrame, framePos.X, framePos.Y);
            int pCost = products[0].cost;
            bool crossout = false;
            if (products[0].cost != products[0].originalCost)
            {
                crossout = true;
                pCost = products[0].originalCost;
                Graphics.Draw(_priceTargets[0], new Vec2(framePos.X - 13f, framePos.Y - 27f), null, Color.White, 0.3f, Vec2.Zero, Vec2.One, SpriteEffects.None, 0.9685f);
                Graphics.Draw(_cheapTape, framePos.X, framePos.Y);
            }
            _furniFill.color = products[idx].color;
            Graphics.Draw(_furniFill, framePos.X, framePos.Y);
            products[idx].Draw(framePos, alpha, 0.97f);
            if (idx == _selectIndex)
            {
                Graphics.Draw(_furniHov, framePos.X - 1f, framePos.Y);
            }
            if (products[idx].type == VPType.Furniture && products[idx].furnitureData.rarity >= Rarity.SuperRare)
            {
                _rareSticker.frame = ((idx == _selectIndex) ? 1 : 0);
                Graphics.Draw(_rareSticker, framePos.X - 23f, framePos.Y - 19f);
            }
            else if (products[idx].type == VPType.Hat || (!products[idx].sold && Profiles.experienceProfile.GetNumFurnitures(products[idx].furnitureData.index) <= 0))
            {
                _newSticker.frame = ((idx == _selectIndex) ? 1 : 0);
                Graphics.Draw(_newSticker, framePos.X - 23f, framePos.Y - 19f);
            }
            if (products[idx].sold)
            {
                Graphics.Draw(_soldSprite, framePos.X, framePos.Y);
            }
            else
            {
                string priceString = Math.Min(Math.Max(pCost, 0), 9999).ToString();
                _furniTag.frame = priceString.Length - 1;
                Graphics.Draw(_furniTag, framePos.X + 21f, framePos.Y - 25f);
                string vertString = "$\n";
                string text = priceString;
                for (int k = 0; k < text.Length; k++)
                {
                    vertString = vertString + text[k] + "\n";
                }
                ((!crossout) ? _priceFont : _priceFontCrossout).Draw(vertString, new Vec2(framePos.X + 24f, framePos.Y - 16f), (pCost > Profiles.experienceProfile.littleManBucks) ? Colors.DGRed : ((!crossout) ? Color.Black : Color.White), 0.974f);
            }
            if (products.Count > 1)
            {
                idx = 1;
                framePos = new Vec2(furniPos.X + 70f - 200f + Math.Min(_showLerp * (float)(200 + 40 * idx), 200f), furniPos.Y);
                Graphics.Draw(_furniFrame, framePos.X, framePos.Y);
                pCost = products[1].cost;
                crossout = false;
                if (products[1].cost != products[1].originalCost)
                {
                    crossout = true;
                    pCost = products[1].originalCost;
                    Graphics.Draw(_priceTargets[1], new Vec2(framePos.X - 13f, framePos.Y - 27f), null, Color.White, 0.3f, Vec2.Zero, Vec2.One, SpriteEffects.None, 0.9685f);
                    Graphics.Draw(_cheapTape, framePos.X, framePos.Y);
                }
                _furniFill.color = products[idx].color;
                Graphics.Draw(_furniFill, framePos.X, framePos.Y);
                products[idx].Draw(framePos, alpha, 0.97f);
                if (idx == _selectIndex)
                {
                    Graphics.Draw(_furniHov, framePos.X - 1f, framePos.Y);
                }
                if (products[idx].type == VPType.Furniture && products[idx].furnitureData.rarity >= Rarity.SuperRare)
                {
                    _rareSticker.frame = ((idx == _selectIndex) ? 1 : 0);
                    Graphics.Draw(_rareSticker, framePos.X - 23f, framePos.Y - 19f);
                }
                else if (Profiles.experienceProfile.GetNumFurnitures(products[idx].furnitureData.index) <= 0)
                {
                    _newSticker.frame = ((idx == _selectIndex) ? 1 : 0);
                    Graphics.Draw(_newSticker, framePos.X - 23f, framePos.Y - 19f);
                }
                if (products[idx].sold)
                {
                    Graphics.Draw(_soldSprite, framePos.X, framePos.Y);
                }
                else
                {
                    string priceString2 = Math.Min(Math.Max(pCost, 0), 9999).ToString();
                    _furniTag.frame = priceString2.Length - 1;
                    Graphics.Draw(_furniTag, framePos.X + 21f, framePos.Y - 25f);
                    string vertString2 = "$\n";
                    string text = priceString2;
                    for (int k = 0; k < text.Length; k++)
                    {
                        vertString2 = vertString2 + text[k] + "\n";
                    }
                    ((!crossout) ? _priceFont : _priceFontCrossout).Draw(vertString2, new Vec2(framePos.X + 24f, framePos.Y - 16f), (pCost > Profiles.experienceProfile.littleManBucks) ? Colors.DGRed : ((!crossout) ? Color.Black : Color.White), 0.974f);
                }
            }
            if (products.Count > 2)
            {
                idx = 2;
                framePos = new Vec2(furniPos.X - 200f + Math.Min(_showLerp * (float)(200 + 40 * idx), 200f), furniPos.Y + 54f);
                Graphics.Draw(_furniFrame, framePos.X, framePos.Y);
                pCost = products[2].cost;
                crossout = false;
                if (products[2].cost != products[2].originalCost)
                {
                    crossout = true;
                    pCost = products[2].originalCost;
                    Graphics.Draw(_priceTargets[2], new Vec2(framePos.X - 13f, framePos.Y - 27f), null, Color.White, 0.3f, Vec2.Zero, Vec2.One, SpriteEffects.None, 0.9685f);
                    Graphics.Draw(_cheapTape, framePos.X, framePos.Y);
                }
                _furniFill.color = products[idx].color;
                Graphics.Draw(_furniFill, framePos.X, framePos.Y);
                products[idx].Draw(framePos, alpha, 0.97f);
                if (idx == _selectIndex)
                {
                    Graphics.Draw(_furniHov, framePos.X - 1f, framePos.Y);
                }
                if (products[idx].type == VPType.Furniture && products[idx].furnitureData.rarity >= Rarity.SuperRare)
                {
                    _rareSticker.frame = ((idx == _selectIndex) ? 1 : 0);
                    Graphics.Draw(_rareSticker, framePos.X - 23f, framePos.Y - 19f);
                }
                else if (Profiles.experienceProfile.GetNumFurnitures(products[idx].furnitureData.index) <= 0)
                {
                    _newSticker.frame = ((idx == _selectIndex) ? 1 : 0);
                    Graphics.Draw(_newSticker, framePos.X - 23f, framePos.Y - 19f);
                }
                if (products[idx].sold)
                {
                    Graphics.Draw(_soldSprite, framePos.X, framePos.Y);
                }
                else
                {
                    string priceString3 = Math.Min(Math.Max(pCost, 0), 9999).ToString();
                    _furniTag.frame = priceString3.Length - 1;
                    Graphics.Draw(_furniTag, framePos.X + 21f, framePos.Y - 25f);
                    string vertString3 = "$\n";
                    string text = priceString3;
                    for (int k = 0; k < text.Length; k++)
                    {
                        vertString3 = vertString3 + text[k] + "\n";
                    }
                    ((!crossout) ? _priceFont : _priceFontCrossout).Draw(vertString3, new Vec2(framePos.X + 24f, framePos.Y - 16f), (pCost > Profiles.experienceProfile.littleManBucks) ? Colors.DGRed : ((!crossout) ? Color.Black : Color.White), 0.974f);
                }
            }
            if (products.Count > 3)
            {
                idx = 3;
                framePos = new Vec2(furniPos.X + 70f - 200f + Math.Min(_showLerp * (float)(200 + 40 * idx), 200f), furniPos.Y + 54f);
                Graphics.Draw(_furniFrame, framePos.X, framePos.Y);
                pCost = products[3].cost;
                crossout = false;
                if (products[3].cost != products[3].originalCost)
                {
                    crossout = true;
                    pCost = products[3].originalCost;
                    Graphics.Draw(_priceTargets[3], new Vec2(framePos.X - 13f, framePos.Y - 27f), null, Color.White, 0.3f, Vec2.Zero, Vec2.One, SpriteEffects.None, 0.9685f);
                    Graphics.Draw(_cheapTape, framePos.X, framePos.Y);
                }
                _furniFill.color = products[idx].color;
                Graphics.Draw(_furniFill, framePos.X, framePos.Y);
                products[idx].Draw(framePos, alpha, 0.97f);
                if (idx == _selectIndex)
                {
                    Graphics.Draw(_furniHov, framePos.X - 1f, framePos.Y);
                }
                if (products[idx].type == VPType.Furniture && products[idx].furnitureData.rarity >= Rarity.SuperRare)
                {
                    _rareSticker.frame = ((idx == _selectIndex) ? 1 : 0);
                    Graphics.Draw(_rareSticker, framePos.X - 23f, framePos.Y - 19f);
                }
                else if (Profiles.experienceProfile.GetNumFurnitures(products[idx].furnitureData.index) <= 0)
                {
                    _newSticker.frame = ((idx == _selectIndex) ? 1 : 0);
                    Graphics.Draw(_newSticker, framePos.X - 23f, framePos.Y - 19f);
                }
                if (products[idx].sold)
                {
                    Graphics.Draw(_soldSprite, framePos.X, framePos.Y);
                }
                else
                {
                    string priceString4 = Math.Min(Math.Max(pCost, 0), 9999).ToString();
                    _furniTag.frame = priceString4.Length - 1;
                    Graphics.Draw(_furniTag, framePos.X + 21f, framePos.Y - 25f);
                    string vertString4 = "$\n";
                    string text = priceString4;
                    for (int k = 0; k < text.Length; k++)
                    {
                        vertString4 = vertString4 + text[k] + "\n";
                    }
                    ((!crossout) ? _priceFont : _priceFontCrossout).Draw(vertString4, new Vec2(framePos.X + 24f, framePos.Y - 16f), (pCost > Profiles.experienceProfile.littleManBucks) ? Colors.DGRed : ((!crossout) ? Color.Black : Color.White), 0.974f);
                }
            }
        }
        if (show && products.Count > 0)
        {
            int sel = 0;
            if (_selectIndex >= 0)
            {
                sel = _selectIndex;
            }
            Vec2 namePos = new Vec2(20f, 6f);
            Vec2 nameSize = new Vec2(226f, 11f);
            Graphics.DrawRect(namePos, namePos + nameSize, Color.Black, 0.96f);
            string name = products[sel].name;
            Graphics.DrawString(name, namePos + new Vec2(nameSize.X / 2f - Graphics.GetStringWidth(name) / 2f, 2f), new Color(163, 206, 39) * alpha, 0.97f);
            _tail.Depth = 0.5f;
            _tail.Alpha = alpha;
            _tail.flipH = false;
            _tail.flipV = false;
            Graphics.Draw(_tail, 222f, 17f);
        }
        if (hasKid)
        {
            _dealer.frame -= 9;
        }
    }
}
