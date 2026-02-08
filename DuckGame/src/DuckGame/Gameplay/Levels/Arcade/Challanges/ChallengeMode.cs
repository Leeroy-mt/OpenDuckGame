using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DuckGame;

[EditorGroup("Special", EditorItemType.Arcade)]
[BaggedProperty("isOnlineCapable", false)]
public class ChallengeMode : Thing
{
    public EditorProperty<bool> random = new EditorProperty<bool>(val: false);

    public EditorProperty<string> music = new EditorProperty<string>("");

    public EditorProperty<string> Next = new EditorProperty<string>("", null, 0f, 1f, 0f, null, isTime: false, isLevel: true);

    private List<ChallengeTrophy> _eligibleTrophies = new List<ChallengeTrophy>();

    private List<ChallengeTrophy> _wonTrophies = new List<ChallengeTrophy>();

    private int _startGoodies;

    public static bool showReticles = true;

    public List<GoalType> goalTypes;

    protected int baselineTargetCount = -1;

    protected int baselineGoodyCount = -1;

    private bool hasTargetLimit;

    private bool hasGoodyLimit;

    private bool reverseTimeLimit;

    private bool _ended;

    public Duck duck;

    private int restartWait;

    private ContextMenu _hatMenu;

    private int _hatIndex;

    protected ChallengeData _challenge = new ChallengeData();

    public List<ChallengeTrophy> wonTrophies => _wonTrophies;

    public int hatIndex
    {
        get
        {
            return _hatIndex;
        }
        set
        {
            _hatIndex = value;
            UpdateMenuHat();
        }
    }

    public ChallengeData challenge => _challenge;

    public ChallengeMode()
    {
        graphic = new Sprite("challengeIcon");
        Center = new Vector2(8f, 8f);
        _collisionSize = new Vector2(16f, 16f);
        _collisionOffset = new Vector2(-8f, -8f);
        base.Depth = 0.9f;
        base.layer = Layer.Foreground;
        _editorName = "Challenge";
        _canFlip = false;
        _canHaveChance = false;
        random._tooltip = "If enabled, this challenge will activate a random target sequence whenever all targets are down.";
        music._tooltip = "The name of a music file (without extension) from the Duck Game Content/Audio/Music/InGame folder.";
        Next._tooltip = "If specified, the next challenge will play immediately after this one (for challenge rush modes).";
    }

    public override void Initialize()
    {
        showReticles = true;
        if (Level.current is Editor)
        {
            return;
        }
        if (Level.current.camera is FollowCam cam)
        {
            cam.minSize = 90f;
        }
        _eligibleTrophies.AddRange(_challenge.trophies);
        if (ChallengeLevel.timer != null)
        {
            if (_challenge.trophies[0].timeRequirement != 0)
            {
                ChallengeLevel.timer.maxTime = new TimeSpan(0, 0, _challenge.trophies[0].timeRequirement);
            }
            else
            {
                ChallengeLevel.timer.maxTime = default(TimeSpan);
            }
        }
        _startGoodies = Level.current.things[typeof(Goody)].Count();
    }

    public virtual void PrepareCounts()
    {
        baselineTargetCount = _challenge.trophies[0].targets;
        baselineGoodyCount = _challenge.trophies[0].goodies;
        goalTypes = new List<GoalType>();
        foreach (GoalType gt in Level.current.things[typeof(GoalType)])
        {
            goalTypes.Add(gt);
        }
        for (int i = 0; i < _eligibleTrophies.Count; i++)
        {
            if (_eligibleTrophies[i].targets > 0)
            {
                hasTargetLimit = true;
            }
            else if (_eligibleTrophies[i].goodies > 0)
            {
                hasGoodyLimit = true;
            }
            if (i > 0 && _eligibleTrophies[i - 1].timeRequirement < _eligibleTrophies[i].timeRequirement && _eligibleTrophies[i - 1].timeRequirement != 0)
            {
                reverseTimeLimit = true;
            }
        }
    }

    public override void Update()
    {
        if (_ended || ChallengeLevel.timer == null)
        {
            return;
        }
        if (duck != null && duck.dead)
        {
            restartWait++;
            if (restartWait >= 3)
            {
                if (_wonTrophies.Count > 0)
                {
                    _eligibleTrophies.Clear();
                }
                else
                {
                    _ended = true;
                    if (Level.current is ChallengeLevel cur)
                    {
                        cur.RestartChallenge();
                    }
                }
            }
        }
        if (duck == null)
        {
            return;
        }
        bool enableDev = true;
        for (int i = 0; i < _eligibleTrophies.Count; i++)
        {
            bool stillEligible = true;
            bool won = false;
            ChallengeTrophy t = _eligibleTrophies[i];
            if (t.type == TrophyType.Developer && ((t.goodies == -1 && t.targets == -1 && t.timeRequirement == 0) || !enableDev))
            {
                stillEligible = false;
            }
            else
            {
                bool timebreak = reverseTimeLimit;
                if (t.timeRequirement != 0 && (int)ChallengeLevel.timer.elapsed.TotalSeconds >= t.timeRequirement && (t.type == TrophyType.Baseline || Math.Abs(ChallengeLevel.timer.elapsed.TotalSeconds - (double)t.timeRequirement) > 0.009999999776482582) && (t.timeRequirementMilliseconds == 0 || (int)Math.Round(ChallengeLevel.timer.elapsed.TotalSeconds % 1.0 * 100.0) > t.timeRequirementMilliseconds))
                {
                    stillEligible = false;
                    timebreak = !reverseTimeLimit;
                }
                if (!timebreak)
                {
                    won = true;
                    if (baselineTargetCount == -1)
                    {
                        if (t.targets == -1 && !SequenceItem.IsFinished(SequenceItemType.Target))
                        {
                            won = false;
                        }
                        else if (t.targets != -1 && ChallengeLevel.targetsShot < t.targets)
                        {
                            won = false;
                        }
                    }
                    else if (ChallengeLevel.targetsShot < baselineTargetCount)
                    {
                        won = false;
                    }
                    if (baselineGoodyCount == -1 || (baselineGoodyCount == 0 && _challenge.countGoodies))
                    {
                        if ((t.goodies == -1 || (t.goodies == 0 && _challenge.countGoodies)) && !SequenceItem.IsFinished(SequenceItemType.Goody))
                        {
                            won = false;
                        }
                        else if (ChallengeLevel.goodiesGot < t.goodies)
                        {
                            won = false;
                        }
                    }
                    else if (ChallengeLevel.goodiesGot < baselineGoodyCount)
                    {
                        won = false;
                    }
                    if (t.targets == -1 && !hasTargetLimit)
                    {
                        foreach (GoalType goalType in goalTypes)
                        {
                            if (goalType.numObjectsRemaining != 0)
                            {
                                won = false;
                            }
                        }
                    }
                }
                using (List<GoalType>.Enumerator enumerator = goalTypes.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        switch (enumerator.Current.Check())
                        {
                            case GoalType.Result.None:
                                won = false;
                                break;
                            case GoalType.Result.Lose:
                                stillEligible = false;
                                break;
                        }
                    }
                }
                if (won)
                {
                    stillEligible = false;
                    if (t.type != TrophyType.Baseline)
                    {
                        _wonTrophies.Add(t);
                        if (DuckNetwork.core.speedrunMaxTrophy > 0 && t.type == (TrophyType)DuckNetwork.core.speedrunMaxTrophy)
                        {
                            _eligibleTrophies.Clear();
                            break;
                        }
                    }
                }
            }
            if (!stillEligible)
            {
                _eligibleTrophies.RemoveAt(i);
                if (t.type == TrophyType.Baseline && !won)
                {
                    _eligibleTrophies.Clear();
                }
                i--;
            }
        }
        if (Level.current != base.level || _eligibleTrophies.Count != 0)
        {
            return;
        }
        foreach (ChallengeTrophy t2 in _wonTrophies.ToList())
        {
            if (t2.targets != -1 && ChallengeLevel.targetsShot < t2.targets)
            {
                _wonTrophies.Remove(t2);
            }
            if (t2.goodies != -1 && ChallengeLevel.goodiesGot < t2.goodies)
            {
                _wonTrophies.Remove(t2);
            }
        }
        if (_wonTrophies.Count > 1)
        {
            ChallengeTrophy best = _wonTrophies[0];
            foreach (ChallengeTrophy t3 in _wonTrophies)
            {
                if (t3.type > best.type)
                {
                    best = t3;
                }
            }
            _wonTrophies.Clear();
            _wonTrophies.Add(best);
        }
        ChallengeLevel.timer.Stop();
        if (Level.current is ChallengeLevel cur2)
        {
            cur2.ChallengeEnded(this);
        }
        _ended = true;
    }

    public override void Draw()
    {
        if (Level.current is Editor)
        {
            base.Draw();
        }
    }

    private void UpdateMenuHat()
    {
        if (_hatMenu != null)
        {
            if (Teams.all[_hatIndex].hasHat)
            {
                _hatMenu.image = Teams.all[_hatIndex].hat.CloneMap();
                _hatMenu.image.Center = new Vector2(12f, 12f) + Teams.all[_hatIndex].hatOffset;
            }
            else
            {
                _hatMenu.image = null;
            }
        }
    }

    public override ContextMenu GetContextMenu()
    {
        new FieldBinding(this, "hatIndex");
        EditorGroupMenu menu = base.GetContextMenu() as EditorGroupMenu;
        ContextTextbox nameBox = new ContextTextbox("Name", null, new FieldBinding(_challenge, "name"), "The name of this Challenge.");
        menu.AddItem(nameBox);
        ContextTextbox descBox = new ContextTextbox("Desc", null, new FieldBinding(_challenge, "description"), "The Story.");
        menu.AddItem(descBox);
        ContextTextbox goalBox = new ContextTextbox("Goal Desc", null, new FieldBinding(_challenge, "goal"), "A description of how to win the challenge.");
        menu.AddItem(goalBox);
        if (!(this is ChallengeModeNew))
        {
            ContextTextbox requirementBox = new ContextTextbox("Requires", null, new FieldBinding(_challenge, "requirement"), "Number of arcade trophies required to unlock. B15 = 15 Bronze. B2P2 = 2 Bronze, 2 Platinum.");
            menu.AddItem(requirementBox);
            ContextTextbox prefixBox = new ContextTextbox("noun(plural)", null, new FieldBinding(_challenge, "prefix"), "Name of goal object pluralized, for example \"Ducks\", \"Stars\", etc.");
            menu.AddItem(prefixBox);
            menu.AddItem(new ContextFile("Prev", null, new FieldBinding(_challenge, "prevchal"), ContextFileType.Level, "If set, Chancy will offer this challenge as a special challenge after PREV is completed, if REQUIRES is met."));
            menu.AddItem(new ContextCheckBox("Goodies", null, new FieldBinding(_challenge, "countGoodies"), null, "If set, the goal for this challenge is to collect goodies (stars, finish flags, etc)."));
            menu.AddItem(new ContextCheckBox("Targets", null, new FieldBinding(_challenge, "countTargets"), null, "If set, the goal for this challenge is to knock down targets."));
        }
        int index = 0;
        bool noTime = false;
        bool noGoodyNumber = false;
        bool noTargetNumber = false;
        foreach (ChallengeTrophy t in _challenge.trophies)
        {
            SpriteMap image = null;
            image = new SpriteMap("challengeTrophyIcons", 16, 16);
            image.frame = index;
            index++;
            EditorGroupMenu goalMenu = new EditorGroupMenu(menu, root: false, image);
            if (t.type == TrophyType.Bronze)
            {
                goalMenu.tooltip = "This one should be pretty easy.";
            }
            if (t.type == TrophyType.Silver)
            {
                goalMenu.tooltip = "You're getting there!";
            }
            if (t.type == TrophyType.Gold)
            {
                goalMenu.tooltip = "Someone should win this when they've become pretty good at your challenge.";
            }
            if (t.type == TrophyType.Platinum)
            {
                goalMenu.tooltip = "A hidden trophy that should be pretty tricky to get.";
            }
            if (t.type == TrophyType.Developer)
            {
                goalMenu.tooltip = "Your very best score goes here, it should be harder to get than Platinum!";
            }
            goalMenu.text = t.type.ToString();
            if (t.type == TrophyType.Baseline)
            {
                goalMenu.text = "Goals";
                goalMenu.AddItem(new ContextSlider("Goodies", null, new FieldBinding(t, "goodies", -1f, 300f), 1f, "ALL", time: false, null, "You must always collect exactly this many goodies."));
                goalMenu.AddItem(new ContextSlider("Targets", null, new FieldBinding(t, "targets", -1f, 300f), 1f, "ALL", time: false, null, "You must always knock down exactly this many targets."));
                goalMenu.AddItem(new ContextSlider("Time Limit", null, new FieldBinding(t, "timeRequirement", 0f, 600f), 1f, "NONE", time: true, null, "You have at most this much time to complete the challenge."));
                if (t.goodies >= 0)
                {
                    noGoodyNumber = true;
                }
                if (t.targets >= 0)
                {
                    noTargetNumber = true;
                }
            }
            else
            {
                if (!noGoodyNumber)
                {
                    goalMenu.AddItem(new ContextSlider("Goodies", null, new FieldBinding(t, "goodies", -1f, 300f), 1f, "ALL", time: false, null, "Collect this many items to get this trophy"));
                }
                if (!noTargetNumber)
                {
                    goalMenu.AddItem(new ContextSlider("Targets", null, new FieldBinding(t, "targets", -1f, 300f), 1f, "ALL", time: false, null, "Knock down this many targets to get this trophy"));
                }
                if (!noTime)
                {
                    goalMenu.AddItem(new ContextSlider("Time", null, new FieldBinding(t, "timeRequirement", 0f, 600f), 1f, "GOAL TIME", time: true, null, "Complete challenge in this time or less to get this trophy."));
                    goalMenu.AddItem(new ContextSlider("Milis", null, new FieldBinding(t, "timeRequirementMilliseconds", 0f, 99f), 1f, "NONE", time: false, null, "Fine control over challenge time requirement."));
                }
            }
            menu.AddItem(goalMenu);
        }
        return menu;
    }

    public override BinaryClassChunk Serialize()
    {
        BinaryClassChunk binaryClassChunk = base.Serialize();
        binaryClassChunk.AddProperty("hatIndex", hatIndex);
        binaryClassChunk.AddProperty("challengeData", _challenge.Serialize());
        return binaryClassChunk;
    }

    public override bool Deserialize(BinaryClassChunk node)
    {
        base.Deserialize(node);
        hatIndex = node.GetProperty<int>("hatIndex");
        BinaryClassChunk challengeData = node.GetProperty<BinaryClassChunk>("challengeData");
        if (challengeData != null)
        {
            _challenge = new ChallengeData();
            _challenge.Deserialize(challengeData);
        }
        if (Next.value == null)
        {
            Next.value = "";
        }
        return true;
    }

    public override DXMLNode LegacySerialize()
    {
        DXMLNode dXMLNode = base.LegacySerialize();
        dXMLNode.Add(new DXMLNode("hatIndex", hatIndex));
        dXMLNode.Add(_challenge.LegacySerialize());
        return dXMLNode;
    }

    public override bool LegacyDeserialize(DXMLNode node)
    {
        base.LegacyDeserialize(node);
        DXMLNode getNode = node.Element("hatIndex");
        if (getNode != null)
        {
            hatIndex = Convert.ToInt32(getNode.Value);
        }
        getNode = node.Element("challengeData");
        if (getNode != null)
        {
            _challenge = new ChallengeData();
            _challenge.LegacyDeserialize(getNode);
        }
        return true;
    }
}
