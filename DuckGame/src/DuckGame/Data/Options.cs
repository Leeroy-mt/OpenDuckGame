using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DuckGame;

public static class Options
{
    private static UIMenu _optionsMenu;

    private static UIMenu _controllerWarning;

    private static OptionsData _data = new OptionsData();

    private static OptionsDataLocal _localData = new OptionsDataLocal();

    private static bool _removedOptionsMenu = false;

    private static bool _openedOptionsMenu = false;

    public static UIMenu openOnClose = null;

    private static UIMenu _graphicsMenu;

    private static UIMenu _audioMenu;

    private static UIMenu _accessibilityMenu;

    private static UIMenu _ttsMenu;

    private static UIMenu _blockMenu;

    private static UIMenu _controlsMenu;

    public static UIMenu _lastCreatedAudioMenu;

    public static UIMenu _lastCreatedGraphicsMenu;

    public static UIMenu _lastCreatedAccessibilityMenu;

    public static UIMenu _lastCreatedTTSMenu;

    public static UIMenu _lastCreatedBlockMenu;

    public static UIMenu _lastCreatedControlsMenu;

    public static int flagForSave = 0;

    private static bool _doingResolutionRestart = false;

    private static List<string> chatFonts = new List<string> { "Duck Font", "Arial", "Calibri", "Courier New", "Comic Sans MS", "Custom" };

    public static UIMenu tempTTSMenu;

    public static UIMenu tempBlockMenu;

    private static bool loadCalled = false;

    public static int legacyPreferredColor = -1;

    private static string _pendingTTS;

    private static bool _resolutionChanged = false;

    public static UIMenu optionsMenu => _optionsMenu;

    public static UIMenu controllerWarning => _controllerWarning;

    public static OptionsData Data
    {
        get
        {
            return _data;
        }
        set
        {
            _data = value;
        }
    }

    public static OptionsDataLocal LocalData
    {
        get
        {
            return _localData;
        }
        set
        {
            _localData = value;
        }
    }

    public static bool menuOpen => _optionsMenu.open;

    public static UIMenu graphicsMenu => _graphicsMenu;

    public static UIMenu audioMenu => _audioMenu;

    public static UIMenu accessibilityMenu => _accessibilityMenu;

    public static UIMenu ttsMenu => _ttsMenu;

    public static UIMenu blockMenu => _blockMenu;

    public static UIMenu controlsMenu => _controlsMenu;

    public static int selectedFont
    {
        get
        {
            string name = Data.chatFont;
            int idx = chatFonts.IndexOf(name);
            if (name == "")
            {
                idx = 0;
            }
            if (idx == -1)
            {
                return chatFonts.Count - 1;
            }
            return idx;
        }
        set
        {
            if (value < 0)
            {
                value = 0;
            }
            if (value >= chatFonts.Count - 1)
            {
                value = chatFonts.Count - 2;
            }
            if (value == 0)
            {
                Data.chatFont = "";
            }
            else
            {
                Data.chatFont = chatFonts[value];
            }
            DuckNetwork.UpdateFont();
        }
    }

    public static int fontSize
    {
        get
        {
            return Data.chatFontSize;
        }
        set
        {
            if (value < 12)
            {
                value = 12;
            }
            if (value >= 80)
            {
                value = 80;
            }
            Data.chatFontSize = value;
            DuckNetwork.UpdateFont();
        }
    }

    public static int languageFilter
    {
        get
        {
            if (!Data.languageFilter)
            {
                return 0;
            }
            return 1;
        }
        set
        {
            Data.languageFilter = value == 1;
        }
    }

    public static int mojiFilter
    {
        get
        {
            return Data.mojiFilter;
        }
        set
        {
            Data.mojiFilter = value;
        }
    }

    public static int hatFilter
    {
        get
        {
            return Data.hatFilter;
        }
        set
        {
            Data.hatFilter = value;
        }
    }

    public static string optionsFileName => DuckFile.optionsDirectory + "/options.dat";

    private static string optionsFileLocalName => DuckFile.optionsDirectory + "/localsettings.dat";

    public static string GetMuteSettings(Profile pProfile)
    {
        string s = "";
        if (Data.muteSettings.TryGetValue(pProfile.steamID, out s))
        {
            return s;
        }
        return "";
    }

    public static void SetMuteSetting(Profile pProfile, string pSetting, bool pValue)
    {
        string s = "";
        if (!Data.muteSettings.TryGetValue(pProfile.steamID, out s))
        {
            string text = (Data.muteSettings[pProfile.steamID] = "");
            s = text;
        }
        if (pValue && !s.Contains(pSetting))
        {
            s += pSetting;
        }
        else if (!pValue)
        {
            s = s.Replace(pSetting, "");
        }
        Data.muteSettings[pProfile.steamID] = s;
    }

    public static void AddMenus(UIComponent to)
    {
        to.Add(optionsMenu, doAnchor: false);
        to.Add(graphicsMenu, doAnchor: false);
        to.Add(audioMenu, doAnchor: false);
        if (accessibilityMenu != null)
        {
            to.Add(accessibilityMenu, doAnchor: false);
        }
        if (ttsMenu != null)
        {
            to.Add(ttsMenu, doAnchor: false);
        }
        if (blockMenu != null)
        {
            to.Add(blockMenu, doAnchor: false);
        }
        if (controlsMenu != null)
        {
            to.Add(controlsMenu, doAnchor: false);
            to.Add((controlsMenu as UIControlConfig)._confirmMenu, doAnchor: false);
            to.Add((controlsMenu as UIControlConfig)._warningMenu, doAnchor: false);
        }
        to.Add(controllerWarning, doAnchor: false);
    }

    public static void QuitShowingControllerWarning()
    {
        Data.showControllerWarning = false;
        Save();
    }

    public static UIMenu CreateOptionsMenu()
    {
        UIMenu men = new UIMenu("@WRENCH@OPTIONS@SCREWDRIVER@", Layer.HUD.camera.width / 2f, Layer.HUD.camera.height / 2f, 190f, -1f, "@CANCEL@BACK @SELECT@SELECT");
        men.Add(new UIMenuItemSlider("SFX Volume", null, new FieldBinding(Data, "sfxVolume"), 1f / 15f));
        men.Add(new UIMenuItemSlider("Music Volume", null, new FieldBinding(Data, "musicVolume"), 1f / 15f));
        men.Add(new UIMenuItemSlider("Rumble Intensity", null, new FieldBinding(Data, "rumbleIntensity"), 1f / 15f));
        men.Add(new UIText(" ", Color.White));
        men.Add(new UIMenuItemToggle("SHENANIGANS", null, new FieldBinding(Data, "shennanigans")));
        _lastCreatedGraphicsMenu = CreateGraphicsMenu(men);
        _lastCreatedAccessibilityMenu = CreateAccessibilityMenu(men);
        _lastCreatedTTSMenu = tempTTSMenu;
        _lastCreatedBlockMenu = tempBlockMenu;
        _lastCreatedAudioMenu = CreateAudioMenu(men);
        men.Add(new UIText(" ", Color.White));
        men.Add(new UIMenuItem("GRAPHICS", new UIMenuActionOpenMenu(men, _lastCreatedGraphicsMenu), UIAlign.Center, default(Color), backButton: true));
        men.Add(new UIMenuItem("AUDIO", new UIMenuActionOpenMenu(men, _lastCreatedAudioMenu), UIAlign.Center, default(Color), backButton: true));
        men.Add(new UIText(" ", Color.White));
        men.Add(new UIMenuItem("USABILITY", new UIMenuActionOpenMenu(men, _lastCreatedAccessibilityMenu), UIAlign.Center, default(Color), backButton: true));
        men.SetBackFunction(new UIMenuActionCloseMenuCallFunction(men, OptionsMenuClosed));
        men.Close();
        return men;
    }

    public static void Initialize()
    {
        _optionsMenu = CreateOptionsMenu();
        _controllerWarning = CreateControllerWarning();
        _graphicsMenu = _lastCreatedGraphicsMenu;
        _accessibilityMenu = _lastCreatedAccessibilityMenu;
        _audioMenu = _lastCreatedAudioMenu;
        _ttsMenu = _lastCreatedTTSMenu;
        _blockMenu = _lastCreatedBlockMenu;
    }

    public static UIMenu CreateControllerWarning()
    {
        UIMenu men = new UIMenu("Is that a PS4 Controller?", Layer.HUD.camera.width / 2f, Layer.HUD.camera.height / 2f, 220f, -1f, "@CANCEL@BACK @SELECT@SELECT");
        men.Add(new UIText("", Color.White, UIAlign.Center, -3f)
        {
            Scale = new Vector2(0.5f)
        });
        men.Add(new UIText("It seems you may have a |DGBLUE|PS4 Controller", Color.White, UIAlign.Center, -4f)
        {
            Scale = new Vector2(0.5f)
        });
        men.Add(new UIText("plugged in! If so, and if you are running", Color.White, UIAlign.Center, -4f)
        {
            Scale = new Vector2(0.5f)
        });
        men.Add(new UIText("|DGBLUE|DS4Windows|PREV| or a |DGBLUE|3rd party PS4 Controller Driver|PREV|,", Color.White, UIAlign.Center, -4f)
        {
            Scale = new Vector2(0.5f)
        });
        men.Add(new UIText("you may need to |DGRED|disable|PREV| it.", Color.White, UIAlign.Center, -4f)
        {
            Scale = new Vector2(0.5f)
        });
        men.Add(new UIText("", Color.White, UIAlign.Center, -3f)
        {
            Scale = new Vector2(0.5f)
        });
        men.Add(new UIText("If everything works okay, you can ignore", Color.White, UIAlign.Center, -4f)
        {
            Scale = new Vector2(0.5f)
        });
        men.Add(new UIText("this message. If you're controlling 2 ducks", Color.White, UIAlign.Center, -4f)
        {
            Scale = new Vector2(0.5f)
        });
        men.Add(new UIText("at once, then this message is for you!", Color.White, UIAlign.Center, -4f)
        {
            Scale = new Vector2(0.5f)
        });
        men.Add(new UIText("", Color.White, UIAlign.Center, -3f)
        {
            Scale = new Vector2(0.5f)
        });
        men.Add(new UIMenuItem("|DGORANGE|OK THEN", new UIMenuActionCloseMenu(men), UIAlign.Center, Color.White));
        men.Add(new UIMenuItem("|DGRED|DON'T SHOW THIS AGAIN", new UIMenuActionCloseMenuCallFunction(men, QuitShowingControllerWarning), UIAlign.Center, Color.White));
        men.SetBackFunction(new UIMenuActionCloseMenu(men));
        men.Close();
        return men;
    }

    public static float GetWindowScaleMultiplier()
    {
        if (Data.windowScale == 0)
        {
            return 1f;
        }
        if (Data.windowScale == 1)
        {
            return 1.5f;
        }
        return 2f;
    }

    public static void ScreenModeChanged(string pMode)
    {
        if (pMode == "Fullscreen")
        {
            Resolution.Set(Data.windowedFullscreen ? LocalData.windowedFullscreenResolution : LocalData.fullscreenResolution);
        }
        else
        {
            Resolution.Set(LocalData.windowedResolution);
        }
    }

    public static void WindowedFullscreenChanged()
    {
        if (Resolution.current.mode != ScreenMode.Windowed)
        {
            ScreenModeChanged("Fullscreen");
        }
    }

    public static void FullscreenChanged()
    {
        ScreenModeChanged(Data.fullscreen ? "Fullscreen" : "Windowed");
    }

    private static void ExclusiveAudioModeChanged()
    {
        SFX._audio.LoseDevice();
    }

    private static void AudioEngineChanged()
    {
        MonoMain.audioModeOverride = (AudioMode)Data.audioMode;
        Windows_Audio.ResetDevice();
    }

    private static void ApplyResolution()
    {
        if (!_doingResolutionRestart)
        {
            Resolution.Set(LocalData.currentResolution);
        }
        if (LocalData.currentResolution.mode == ScreenMode.Fullscreen)
        {
            LocalData.fullscreenResolution = LocalData.currentResolution;
        }
        else if (LocalData.currentResolution.mode == ScreenMode.Borderless)
        {
            LocalData.windowedFullscreenResolution = LocalData.currentResolution;
        }
        else
        {
            LocalData.windowedResolution = LocalData.currentResolution;
        }
    }

    public static UIMenu CreateGraphicsMenu(UIMenu pOptionsMenu)
    {
        UIMenu graphicsMenu = new UIMenu("@WRENCH@GRAPHICS@SCREWDRIVER@", Layer.HUD.camera.width / 2f, Layer.HUD.camera.height / 2f, 240f, -1f, "@CANCEL@BACK @SELECT@SELECT");
        graphicsMenu.Add(new UIMenuItemToggle("Fullscreen", new UIMenuActionCallFunction(FullscreenChanged), new FieldBinding(Data, "fullscreen")));
        graphicsMenu.Add(new UIMenuItemResolution("Resolution", new FieldBinding(LocalData, "currentResolution", 0f, 0f))
        {
            selectAction = ApplyResolution
        });
        graphicsMenu.Add(new UIText(" ", Color.White));
        graphicsMenu.Add(new UIMenuItemToggle("Windowed Fullscreen", new UIMenuActionCallFunction(WindowedFullscreenChanged), new FieldBinding(Data, "windowedFullscreen")));
        graphicsMenu.Add(new UIText(" ", Color.White));
        graphicsMenu.Add(new UIMenuItemToggle("Fire Glow", null, new FieldBinding(Data, "fireGlow")));
        graphicsMenu.Add(new UIMenuItemToggle("Lighting", null, new FieldBinding(Data, "lighting")));
        graphicsMenu.Add(new UIMenuItemToggle("Backfill Fix", null, new FieldBinding(Data, "fillBackground")));
        graphicsMenu.Add(new UIMenuItemToggle("Explosion Flashes", null, new FieldBinding(Data, "flashing")));
        graphicsMenu.Add(new UIText(" ", Color.White));
        graphicsMenu.Add(new UIMenuItemNumber("Console Width", null, new FieldBinding(Data, "consoleWidth", 25f, 100f), 10));
        graphicsMenu.Add(new UIMenuItemNumber("Console Height", null, new FieldBinding(Data, "consoleHeight", 10f, 100f), 10));
        graphicsMenu.Add(new UIMenuItemNumber("Console Scale", null, new FieldBinding(Data, "consoleScale", 0f, 4f), 1, default(Color), null, null, "", null, new List<string> { "Tiny", "Regular", "Large", "Gigantic", "WUMBO" }));
        graphicsMenu.Add(new UIText(" ", Color.White));
        graphicsMenu.Add(new UIMenuItem("BACK", new UIMenuActionOpenMenu(graphicsMenu, pOptionsMenu), UIAlign.Center, default(Color), backButton: true));
        return graphicsMenu;
    }

    public static UIMenu CreateAudioMenu(UIMenu pOptionsMenu)
    {
        UIMenu audioMenu = new UIMenu("@WRENCH@Audio@SCREWDRIVER@", Layer.HUD.camera.width / 2f, Layer.HUD.camera.height / 2f, 280f, -1f, "@CANCEL@BACK @SELECT@SELECT");
        audioMenu.Add(new UIText("Exclusive Mode can reduce", Colors.DGBlue));
        audioMenu.Add(new UIText("audio latency, but will", Colors.DGBlue));
        audioMenu.Add(new UIText("stop all other programs from", Colors.DGBlue));
        audioMenu.Add(new UIText("making sound while Duck Game", Colors.DGBlue));
        audioMenu.Add(new UIText("is running!", Colors.DGBlue));
        audioMenu.Add(new UIText(" ", Colors.DGBlue));
        audioMenu.Add(new UIMenuItemToggle("Exclusive Mode", new UIMenuActionCallFunction(ExclusiveAudioModeChanged), new FieldBinding(Data, "audioExclusiveMode")));
        audioMenu.Add(new UIMenuItemNumber("Audio Engine", new UIMenuActionCallFunction(AudioEngineChanged), new FieldBinding(Data, "audioMode", 1f, 3f), 1, default(Color), null, null, "", null, new List<string> { "None", "WaveOut", "Wasapi", "DirectSound" }));
        audioMenu.Add(new UIText(" ", Color.White));
        audioMenu.Add(new UIMenuItem("BACK", new UIMenuActionOpenMenu(audioMenu, pOptionsMenu), UIAlign.Center, default(Color), backButton: true));
        return audioMenu;
    }

    private static void CloseMoreMenu()
    {
        DuckNetwork.UpdateFont();
    }

    public static UIMenu CreateBlockMenu(UIMenu pAccessibilityMenu)
    {
        try
        {
            return new UIBlockManagement(pAccessibilityMenu);
        }
        catch (Exception)
        {
            return null;
        }
    }

    public static UIMenu CreateTTSMenu(UIMenu pAccessibilityMenu)
    {
        try
        {
            UIMenu accessibilityMenu = new("TTS SETTINGS", Layer.HUD.camera.width / 2f, Layer.HUD.camera.height / 2f, 280f, -1f, "@SELECT@SELECT");
            accessibilityMenu.Add(new UIMenuItemToggle("Text To Speech", null, new FieldBinding(Data, "textToSpeech")));
            List<string> voices = SFX.GetSayVoices();
            List<string> voiceNames = [];
            foreach (string name in voices)
            {
                //old name2 = name
                string name2 = name.Replace("Microsoft ", "");
                name2 = name2.Replace(" Desktop", "");
                if (name2.Length > 10)
                    name2 = $"{name2[..8]}..";
                voiceNames.Add(name);
            }
            accessibilityMenu.Add(new UIMenuItemNumber("TTS Voice", null, new FieldBinding(Data, "textToSpeechVoice", 0f, voices.Count), 1, default(Color), null, null, "", null, voiceNames));
            accessibilityMenu.Add(new UIMenuItemSlider("TTS Volume", null, new FieldBinding(Data, "textToSpeechVolume"), 1f / 15f));
            accessibilityMenu.Add(new UIMenuItemSlider("TTS Speed", null, new FieldBinding(Data, "textToSpeechRate"), 0.047393363f));
            accessibilityMenu.Add(new UIMenuItemToggle("TTS Read Names", null, new FieldBinding(Data, "textToSpeechReadNames")));
            accessibilityMenu.Add(new UIText(" ", Color.White));
            accessibilityMenu.Add(new UIMenuItem("BACK", new UIMenuActionOpenMenuCallFunction(accessibilityMenu, pAccessibilityMenu, CloseMoreMenu)));
            accessibilityMenu.SetBackFunction(new UIMenuActionOpenMenuCallFunction(accessibilityMenu, pAccessibilityMenu, CloseMoreMenu));
            return accessibilityMenu;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public static UIMenu CreateAccessibilityMenu(UIMenu pOptionsMenu)
    {
        try
        {
            UIMenu accessibilityMenu = new("USABILITY", Layer.HUD.camera.width / 2f, Layer.HUD.camera.height / 2f, 280f, -1f, "@SELECT@SELECT");
            accessibilityMenu.Add(new UIMenuItemToggle("IME Support", null, new FieldBinding(Data, "imeSupport")));
            accessibilityMenu.Add(new UIText(" ", Color.White));
            accessibilityMenu.Add(new UIText("Chat Settings", Color.White));
            accessibilityMenu.Add(new UIMenuItemNumber("Custom MOJIs", null, new FieldBinding(typeof(Options), "mojiFilter", 0f, 2f), 1, default, null, null, "", null, ["|DGGREENN|@languageFilterOn@DISABLED", "|DGYELLO|@languageFilterOn@FRIENDS ", "|DGREDDD| @languageFilterOff@ENABLED"]));
            accessibilityMenu.Add(new UIMenuItemNumber("Custom Hats", null, new FieldBinding(typeof(Options), "hatFilter", 0f, 2f), 1, default, null, null, "", null, ["|DGGREEN|   ENABLED", "|DGYELLO|   FRIENDS", "|DGRED|  DISABLED  "]));
            tempBlockMenu = CreateBlockMenu(accessibilityMenu);
            accessibilityMenu.Add(new UIMenuItem("Manage Block List", new UIMenuActionOpenMenu(accessibilityMenu, tempBlockMenu)));
            accessibilityMenu.Add(new UIText(" ", Color.White));
            accessibilityMenu.Add(new UIMenuItemNumber("Chat Font", null, new FieldBinding(typeof(Options), "selectedFont", 0f, 6f), 1, default, null, null, "", null, chatFonts));
            accessibilityMenu.Add(new UIMenuItemNumber("Chat Font Size", null, new FieldBinding(typeof(Options), "fontSize", 12f, 30f)));
            accessibilityMenu.Add(new UIMenuItemNumber("Chat Head Size", null, new FieldBinding(Data, "chatHeadScale"), 1, default, null, null, "", null, ["Regular", "Large"]));
            accessibilityMenu.Add(new UIMenuItemNumber("Chat Opacity", null, new FieldBinding(Data, "chatOpacity", 20f, 100f), 10));
            if (SFX.hasTTS)
            {
                tempTTSMenu = CreateTTSMenu(accessibilityMenu);
                accessibilityMenu.Add(new UIText(" ", Color.White));
                accessibilityMenu.Add(new UIMenuItem("Text To Speech", new UIMenuActionOpenMenu(accessibilityMenu, tempTTSMenu), UIAlign.Center, default, backButton: true));
            }
            else
            {
                accessibilityMenu.Add(new UIText(" ", Color.White));
                accessibilityMenu.Add(new UIText("|DGRED|Text To Speech Not Installed...", Color.White));
            }
            accessibilityMenu.Add(new UIText(" ", Color.White));
            accessibilityMenu.Add(new UIMenuItem("BACK", new UIMenuActionOpenMenuCallFunction(accessibilityMenu, pOptionsMenu, CloseMoreMenu)));
            accessibilityMenu.SetBackFunction(new UIMenuActionOpenMenuCallFunction(accessibilityMenu, pOptionsMenu, CloseMoreMenu));
            return accessibilityMenu;
        }
        catch (Exception ex)
        {
            DevConsole.LogComplexMessage("Error creating accessibility menu: " + ex.StackTrace.ToString(), Colors.DGRed);
            return null;
        }
    }

    public static void MergeDefaultPreferDefault()
    {
        MergeDefault(pPreferDefault: true);
    }

    public static void MergeDefaultPreferAccount()
    {
        MergeDefault(pPreferDefault: false);
    }

    public static void CancelResolutionChange()
    {
        LocalData.currentResolution = Resolution.lastApplied;
    }

    public static void RestartAndApplyResolution()
    {
        _doingResolutionRestart = true;
        ApplyResolution();
        Save();
        SaveLocalData();
        ModLoader.RestartGame();
    }

    public static void MergeDefault(bool pPreferDefault, bool pShowDialog = true)
    {
        if (Profiles.experienceProfile != null)
        {
            if (MonoMain.logFileOperations)
            {
                DevConsole.Log(DCSection.General, "Options.MergeDefault()");
            }
            Profile exp = Profiles.experienceProfile;
            Profile def = Profiles.all.ElementAt(0);
            Profiles.Save(exp, "__backup_");
            Profiles.Save(def, "__backup_");
            exp.numSandwiches += def.numSandwiches;
            exp.milkFill += def.milkFill;
            exp.littleManLevel += def.littleManLevel - 1;
            exp.numLittleMen += def.numLittleMen;
            exp.littleManBucks += def.littleManBucks;
            exp.timesMetVincent += def.timesMetVincent;
            exp.timesMetVincentSale += def.timesMetVincentSale;
            exp.timesMetVincentSell += def.timesMetVincentSell;
            exp.timesMetVincentImport += def.timesMetVincentImport;
            exp.xp += def.xp;
            foreach (KeyValuePair<int, int> p in def._furnitures)
            {
                if (!exp._furnitures.ContainsKey(p.Key))
                {
                    exp._furnitures[p.Key] = p.Value;
                }
                else
                {
                    exp._furnitures[p.Key] += p.Value;
                }
            }
            DataClass data = exp.stats;
            data += def.stats;
            exp.stats = (ProfileStats)data;
            //exp.stats += def.stats; //old
            foreach (string s in def.unlocks)
            {
                if (!exp.unlocks.Contains(s))
                {
                    exp.unlocks.Add(s);
                }
            }
            foreach (KeyValuePair<string, ChallengeData> pair in Challenges.challenges)
            {
                ChallengeSaveData defDat = def.GetSaveData(pair.Key);
                ChallengeSaveData expDat = exp.GetSaveData(pair.Key);
                if ((expDat.trophy == TrophyType.Baseline && defDat.trophy != TrophyType.Baseline) || (defDat.trophy != TrophyType.Baseline && pPreferDefault))
                {
                    expDat.Deserialize(defDat.Serialize());
                    expDat.profileID = exp.id;
                    exp.challengeData[expDat.challenge] = expDat;
                }
            }
            exp.ticketCount = Challenges.GetTicketCount(exp);
            Profiles.Save(exp);
        }
        if (pShowDialog)
        {
            UIMenu men = new UIMenu("@WRENCH@MERGE COMPLETE@SCREWDRIVER@", Layer.HUD.camera.width / 2f, Layer.HUD.camera.height / 2f, 260f, -1f, "@CANCEL@BACK @SELECT@SELECT");
            men.Add(new UIText("Successfully merged profiles!", Colors.DGBlue));
            men.Add(new UIMenuItem("FINALLY!!", new UIMenuActionCloseMenu(men)));
            men.SetBackFunction(new UIMenuActionCloseMenuCallFunction(men, OptionsMenuClosed));
            men.Close();
            Level.Add(men);
            MonoMain.pauseMenu = men;
            men.Open();
        }
        Data.defaultAccountMerged = true;
        Save();
    }

    public static UIMenu CreateProfileMergeMenu()
    {
        UIMenu men = new UIMenu("@WRENCH@MERGE PROFILES@SCREWDRIVER@", Layer.HUD.camera.width / 2f, Layer.HUD.camera.height / 2f, 270f, -1f, "@CANCEL@BACK @SELECT@SELECT");
        men.Add(new UIText("Looks like you have a 'DEFAULT'", Colors.DGBlue));
        men.Add(new UIText("profile. These are now obsolete.", Colors.DGBlue));
        men.Add(new UIText("", Colors.DGBlue));
        men.Add(new UIText("Would you like to merge all", Colors.DGBlue));
        men.Add(new UIText("data from the 'DEFAULT' profile", Colors.DGBlue));
        men.Add(new UIText("into this one?", Colors.DGBlue));
        men.Add(new UIText("", Colors.DGBlue));
        men.Add(new UIMenuItem("NO!", new UIMenuActionCloseMenu(men)));
        men.Add(new UIMenuItem("YES! (PREFER DEFAULT)", new UIMenuActionCloseMenuCallFunction(men, MergeDefaultPreferDefault)));
        men.Add(new UIMenuItem("YES! (PREFER THIS ACCOUNT)", new UIMenuActionCloseMenuCallFunction(men, MergeDefaultPreferAccount)));
        men.SetBackFunction(new UIMenuActionCloseMenuCallFunction(men, OptionsMenuClosed));
        men.Close();
        return men;
    }

    public static UIMenu CreateResolutionApplyMenu()
    {
        UIMenu men = new UIMenu("@WRENCH@NEW ASPECT RATIO@SCREWDRIVER@", Layer.HUD.camera.width / 2f, Layer.HUD.camera.height / 2f, 270f, -1f, "@CANCEL@BACK @SELECT@SELECT");
        men.Add(new UIText("To apply a resolution", Colors.DGBlue));
        men.Add(new UIText("with a different aspect ratio,", Colors.DGBlue));
        men.Add(new UIText("The game must be restarted.", Colors.DGBlue));
        men.Add(new UIText("", Colors.DGBlue));
        men.Add(new UIText("Would you like to restart", Colors.DGBlue));
        men.Add(new UIText("and apply changes?", Colors.DGBlue));
        men.Add(new UIText("", Colors.DGBlue));
        men.Add(new UIMenuItem("NO!", new UIMenuActionCloseMenuCallFunction(men, CancelResolutionChange)));
        men.Add(new UIMenuItem("YES! (Restart)", new UIMenuActionCloseMenuCallFunction(men, RestartAndApplyResolution)));
        men.SetBackFunction(new UIMenuActionCloseMenuCallFunction(men, OptionsMenuClosed));
        men.Close();
        return men;
    }

    public static void OpenOptionsMenu()
    {
        _removedOptionsMenu = false;
        _openedOptionsMenu = true;
        Level.Add(_optionsMenu);
        _optionsMenu.Open();
    }

    public static void OptionsMenuClosed()
    {
        Save();
        SaveLocalData();
        if (openOnClose != null)
        {
            openOnClose.Open();
        }
    }

    public static void Save()
    {
        if (!loadCalled)
        {
            if (MonoMain.logFileOperations)
            {
                DevConsole.Log(DCSection.General, "Options.Save() skipped (loadCalled = false)");
            }
            return;
        }
        if (MonoMain.logFileOperations)
        {
            DevConsole.Log(DCSection.General, "Options.Save()");
        }
        DuckXML duckXML = new DuckXML();
        DXMLNode data = new DXMLNode("Data");
        data.Add(_data.Serialize());
        duckXML.Add(data);
        string fileName = optionsFileName;
        DuckFile.SaveDuckXML(duckXML, fileName);
    }

    public static void SaveLocalData()
    {
        if (!loadCalled)
        {
            if (MonoMain.logFileOperations)
            {
                DevConsole.Log(DCSection.General, "Options.SaveLocalData() skipped (loadCalled = false)");
            }
            return;
        }
        if (MonoMain.logFileOperations)
        {
            DevConsole.Log(DCSection.General, "Options.SaveLocalData()");
        }
        DuckXML duckXML = new DuckXML();
        DXMLNode localDataNode = new DXMLNode("Data");
        localDataNode.Add(_localData.Serialize());
        duckXML.Add(localDataNode);
        DuckFile.SaveDuckXML(duckXML, optionsFileLocalName);
    }

    public static void Load()
    {
        if (MonoMain.logFileOperations)
        {
            DevConsole.Log(DCSection.General, "Options.Load()");
        }
        loadCalled = true;
        DuckXML doc = DuckFile.LoadDuckXML(optionsFileName);
        if (doc != null)
        {
            new Profile("");
            IEnumerable<DXMLNode> root = doc.Elements("Data");
            if (root == null)
            {
                root = doc.Elements("OptionsData");
            }
            if (root != null)
            {
                foreach (DXMLNode element in root.Elements())
                {
                    if (element.Name == "Options")
                    {
                        _data.Deserialize(element);
                        break;
                    }
                }
            }
        }
        DuckXML localDoc = DuckFile.LoadDuckXML(optionsFileLocalName);
        if (localDoc == null)
        {
            return;
        }
        new Profile("");
        IEnumerable<DXMLNode> root2 = localDoc.Elements("Data");
        if (root2 == null)
        {
            return;
        }
        foreach (DXMLNode element2 in root2.Elements())
        {
            if (element2.Name == "Options")
            {
                _localData.Deserialize(element2);
                break;
            }
        }
    }

    public static void PostLoad()
    {
        if (Data.musicVolume > 1f)
        {
            Data.musicVolume /= 100f;
        }
        if (Data.sfxVolume > 1f)
        {
            Data.sfxVolume /= 100f;
        }
        if (Data.windowScale < 0)
        {
            if (MonoMain.fourK)
            {
                Data.windowScale = 1;
            }
            else
            {
                Data.windowScale = 0;
            }
        }
        Data.consoleWidth = Math.Min(100, Math.Max(Data.consoleWidth, 25));
        Data.consoleHeight = Math.Min(100, Math.Max(Data.consoleHeight, 10));
        Data.consoleScale = Math.Min(5, Math.Max(Data.consoleScale, 1));
        if (Data.currentSaveVersion != -1)
        {
            if (Data.currentSaveVersion < 2)
            {
                Data.consoleScale = 1;
            }
            if (Data.currentSaveVersion < 3)
            {
                Data.windowedFullscreen = true;
            }
        }
        if (Data.currentSaveVersion < 4 || Data.currentSaveVersion == -1)
        {
            DGSave.showOnePointFiveMessages = true;
        }
        if (Data.currentSaveVersion < 5)
        {
            if (Data.keyboard1PlayerIndex > 0)
            {
                legacyPreferredColor = Data.keyboard1PlayerIndex;
                Data.keyboard1PlayerIndex = 0;
            }
            Data.windowedFullscreen = true;
        }
        if (Data.audioMode == 0 || Data.audioMode >= 4)
        {
            Data.audioMode = 2;
        }
        Data.UpdateCurrentVersion();
        if (LocalData.previousAdapterResolution == null || Resolution.adapterResolution != LocalData.previousAdapterResolution)
        {
            Resolution.RestoreDefaults();
            LocalData.previousAdapterResolution = Resolution.adapterResolution;
        }
        if (LocalData.windowedResolution.mode != ScreenMode.Windowed)
        {
            LocalData.windowedResolution = Resolution.FindNearest(ScreenMode.Windowed, LocalData.windowedResolution.x, LocalData.windowedResolution.y);
        }
        if (LocalData.fullscreenResolution.mode != ScreenMode.Fullscreen)
        {
            LocalData.fullscreenResolution = Resolution.FindNearest(ScreenMode.Fullscreen, LocalData.fullscreenResolution.x, LocalData.fullscreenResolution.y);
        }
        if (LocalData.windowedFullscreenResolution.mode != ScreenMode.Borderless)
        {
            LocalData.windowedFullscreenResolution = Resolution.FindNearest(ScreenMode.Borderless, LocalData.windowedFullscreenResolution.x, LocalData.windowedFullscreenResolution.y);
        }
        if (Data.fullscreen)
        {
            LocalData.currentResolution = (Data.windowedFullscreen ? LocalData.windowedFullscreenResolution : LocalData.fullscreenResolution);
        }
        else
        {
            LocalData.currentResolution = LocalData.windowedResolution;
        }
        if (MonoMain.oldAngles)
        {
            Data.oldAngleCode = true;
        }
        if (MonoMain.defaultControls)
        {
            Data.keyboard1PlayerIndex = 0;
            Data.keyboard2PlayerIndex = 1;
        }
    }

    public static void Update()
    {
        Music.masterVolume = Math.Min(1f, Math.Max(0f, Data.musicVolume));
        SFX.volume = Math.Min(1f, Math.Max(0f, Data.sfxVolume));
        if (_openedOptionsMenu && !_removedOptionsMenu && _optionsMenu != null && !_optionsMenu.open && !_optionsMenu.animating)
        {
            _openedOptionsMenu = false;
            _removedOptionsMenu = true;
            Level.Remove(_optionsMenu);
        }
        if (_resolutionChanged)
        {
            _resolutionChanged = false;
            ResolutionChanged();
        }
        if (flagForSave > 0)
        {
            flagForSave--;
            if (flagForSave == 0)
            {
                Save();
            }
        }
    }

    public static void ResolutionChanged()
    {
        if (!MonoMain.started)
        {
            _resolutionChanged = true;
            return;
        }
        FontGDIContext._fontDatas.Clear();
        DuckNetwork.UpdateFont();
        DevConsole.RefreshConsoleFont();
    }
}
