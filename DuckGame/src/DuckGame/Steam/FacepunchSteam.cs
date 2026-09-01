#if FACEPUNCH

using Steamworks;
using Steamworks.Data;
using Steamworks.Ugc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuckGame;

#region Classes

public static class FacepunchSteam
{
    public const uint AppId = 312530;

    public static SteamLobby Lobby { get; private set; }

    public static Friend Me { get; private set; }

    public static SteamId SteamId { get; private set; }

    public static bool Initialize()
    {
        try
        {
            SteamClient.Init(AppId, true);

            SteamId = SteamClient.SteamId;
            Me = new(SteamClient.SteamId);

            return SteamClient.IsValid;
        }
        catch
        {
            return false;
        }
    }

    public static void SetupEvents()
    {
        SteamMatchmaking.OnLobbyMemberJoined += (lobby, playerJoined) =>
            OnLobbyMemberStatus(lobby, playerJoined, MemberStateChange.Entered, null);
        SteamMatchmaking.OnLobbyMemberLeave += (lobby, playerLeft) =>
            OnLobbyMemberStatus(lobby, playerLeft, MemberStateChange.Left, null);
        SteamMatchmaking.OnLobbyMemberDisconnected += (lobby, playerLeft) =>
            OnLobbyMemberStatus(lobby, playerLeft, MemberStateChange.Disconnected, null);
        SteamMatchmaking.OnLobbyMemberKicked += (lobby, playerKicked, responsible) =>
            OnLobbyMemberStatus(lobby, playerKicked, MemberStateChange.Kicked, responsible);
        SteamMatchmaking.OnLobbyMemberBanned += (lobby, playerBanned, responsible) =>
            OnLobbyMemberStatus(lobby, playerBanned, MemberStateChange.Banned, responsible);

        SteamMatchmaking.OnChatMessage += (lobby, friend, message) =>
        {
            if (Lobby != null && Lobby.Id == lobby.Id)
            {
                var bytes = Encoding.UTF8.GetBytes(message);

                if (friend.Id == 0)
                    return;

                Lobby.OnChatMessage(friend, bytes);
            }
        };
    }

    static void OnLobbyMemberStatus(Lobby lobby, Friend user, MemberStateChange state, Friend? responsible)
    {
        if (Lobby == null || Lobby.Id != lobby.Id)
            return;

        Lobby.OnUserStatusChange(
            user,
            state,
            responsible
        );
    }

    public static void Update()
    {
        if (SteamClient.IsValid)
            SteamClient.RunCallbacks();
    }

    public static Query OrderType(Query query, WorkshopQueryFilterOrder order)
    {
        return order switch
        {
            WorkshopQueryFilterOrder.RankedByTotalUniqueSubscriptions => query.RankedByTotalUniqueSubscriptions(),
            WorkshopQueryFilterOrder.RankedByTextSearch => query.RankedByTextSearch(),
            WorkshopQueryFilterOrder.RankedByVotesUp => query.RankedByVotesUp(),
            WorkshopQueryFilterOrder.RankedByTotalVotesAsc => query.RankedByTotalVotesAsc(),
            WorkshopQueryFilterOrder.NotYetRated => query.NotYetRated(),
            WorkshopQueryFilterOrder.CreatedByFollowedUsersRankedByPublicationDate => query.CreatedByFollowedUsers()
                                                                                               .RankedByPublicationDate(),
            WorkshopQueryFilterOrder.RankedByNumTimesReported => query.RankedByNumTimesReported(),
            WorkshopQueryFilterOrder.CreatedByFriendsRankedByPublicationDate => query.CreatedByFriends()
                                                                                         .RankedByPublicationDate(),
            WorkshopQueryFilterOrder.FavoritedByFriendsRankedByPublicationDate => query.FavoritedByFriends()
                                                                                           .RankedByPublicationDate(),
            WorkshopQueryFilterOrder.RankedByTrend => query.RankedByTrend(),
            WorkshopQueryFilterOrder.AcceptedForGameRankedByAcceptanceDate => query.RankedByAcceptanceDate(),
            WorkshopQueryFilterOrder.RankedByPublicationDate => query.RankedByPublicationDate(),
            WorkshopQueryFilterOrder.RankedByVote => query.RankedByVote(),
            _ => query
        };
    }

    public static void OnSendQueryUGCRequest(ResultPage result)
    {
        for (int i = 0; i < result.ResultCount; i++)
        {
            var entry = result.Entries.ElementAt(i);
            var item = WorkshopItem.GetItem(entry.Id);
            if (item != null)
            {
                WorkshopItemData workshopData = new()
                {
                    PreviewPath = entry.PreviewImageUrl,
                    Description = entry.Description,
                    VotesUp = entry.VotesUp,
                    Name = entry.Title,
                    Tags = [.. entry.Tags]
                };
                item.Name = entry.Title;
                item.Data = workshopData;

                item.Dependencies = [];
                if (entry.Children != null)
                {

                    for (int j = 0; j < entry.Children.Length; j++)
                    {
                        var dependency = WorkshopItem.GetItem(entry.Children[j]);
                        if (dependency != null)
                            item.Dependencies.Add(dependency);
                    }
                }
                item.FinishedProcessing = true;
            }
        }
    }

    public static async void RequestItems(WorkshopItem[] items)
    {
        var query = Query.All
            .WithFileId(SelectArray(items, i => i.Id));

        var page = await query.GetPageAsync(1);

        for (int i = 2; page?.ResultCount > 0; i++)
        {
            OnSendQueryUGCRequest(page.Value);
            page = await query.GetPageAsync(i);
        }
    }

    public static async Task<WorkshopItem[]> GetAllWorkshopItems()
    {
        var query = Query.All
            .WhereUserSubscribed();

        if (await query.GetPageAsync(1) is ResultPage page)
        {
            var result = new WorkshopItem[page.TotalCount];
            for (int i = 2; page.ResultCount > 0; i++)
            {
                {
                    int itemIndex = 0;
                    foreach (var item in page.Entries)
                        result[itemIndex++] = WorkshopItem.GetItem(item.Id);
                }

                OnSendQueryUGCRequest(page);

                if (await query.GetPageAsync(i) is ResultPage p)
                    page = p;
                else break;
            }
            return result;
        }

        return null;
    }

    static TOut[] SelectArray<TIn, TOut>(TIn[] baseArray, Func<TIn, TOut> func)
    {
        var result = new TOut[baseArray.Length];
        for (int i = 0; i < baseArray.Length; i++)
            result[i] = func(baseArray[i]);
        return result;
    }

    public static async void JoinLobby(SteamId lobbyId)
    {
        if (Lobby?.Id != lobbyId && lobbyId != 0)
        {
            Lobby?.Leave();
            Lobby = new(lobbyId);
            Lobby.Join();
        }
    }

    public static async void GetStat(string stat)
    {
        SteamUserStats.GetStatInt(stat);
    }

    public static bool ShowOnscreenKeyboard(bool multiline, string description, string existingText, int maxChars)
    {
        var egamepadTextInputLineMode = multiline
            ? GamepadTextInputLineMode.MultipleLines
            : GamepadTextInputLineMode.SingleLine;

        return SteamUtils.ShowGamepadTextInput(GamepadTextInputMode.Normal, egamepadTextInputLineMode, description, maxChars, existingText);
    }

    public static Achievement? GetAchievement(string name)
    {
        if (SteamClient.IsValid && !string.IsNullOrEmpty(name))
            return new(name);
        return null;
    }

    public static void SetStat(string name, int value)
    {
        if (SteamClient.IsValid)
            SteamUserStats.SetStat(name, value);
    }

    public static void SetStat(string name, float value)
    {
        if (SteamClient.IsValid)
            SteamUserStats.SetStat(name, value);
    }

    public static void StoreStats()
    {
        if (SteamClient.IsValid)
            SteamUserStats.StoreStats();
    }

    public static async Task<SteamLobby> CreateLobby(int maxMembers)
    {
        Lobby = new(0);

        if (await SteamMatchmaking.CreateLobbyAsync(maxMembers) is Lobby result)
        {
            if (result.Id != 0)
                Lobby.OnProcessingComplete(result.Id, RoomEnter.Success);
            else
                Lobby.OnProcessingComplete(0, RoomEnter.Error);
        }

        return Lobby;
    }
}

public class SteamLobby
{
    public RoomEnter EnterResult;

    public Lobby Base;

    public LobbyVisibility Visibility
    {
        get;
        set
        {
            if (value switch
            {
                LobbyVisibility.Private => Base.SetPrivate(),
                LobbyVisibility.FriendsOnly => Base.SetFriendsOnly(),
                LobbyVisibility.Public => Base.SetPublic(),
                LobbyVisibility.Invisible => Base.SetInvisible(),
                _ => false
            }) field = value;
        }
    }

    public bool IsProcessing;

    public int MaxMembers
    {
        get => Base.MaxMembers;
        set => Base.MaxMembers = value;
    }

    public SteamId Id => Base.Id;

    public IEnumerable<Friend> Members =>
        Base.Members;

    public Friend Owner
    {
        get => Base.Owner;
        set => Base.Owner = value;
    }

    public event Action<Friend, MemberStateChange, Friend?> UserStatusChange;

    public event Action<Friend, byte[]> ChatMessage;

    public SteamLobby(Lobby lobby)
    {
        Base = lobby;
        IsProcessing = true;
    }

    public SteamLobby(SteamId id)
    {
        Base = new(id);
        IsProcessing = true;
    }

    public string GetData(string name) =>
        Base.GetData(name);

    public void SetData(string name, string value) =>
        Base.SetData(name, value);

    public void Leave() =>
        Base.Leave();

    public async void Join()
    {
        var result = await Base.Join();
        OnProcessingComplete(Id, result);
    }

    public void SetPrivate()
    {
        if (Base.SetPrivate())
            Visibility = LobbyVisibility.Private;
    }

    public void SetPublic()
    {
        if (Base.SetPublic())
            Visibility = LobbyVisibility.Public;
    }

    public void SetInvisible()
    {
        if (Base.SetInvisible())
            Visibility = LobbyVisibility.Invisible;
    }

    public void SetFriendsOnly()
    {
        if (Base.SetFriendsOnly())
            Visibility = LobbyVisibility.FriendsOnly;
    }

    public void SetJoinable(bool joinable) =>
        Base.SetJoinable(joinable);

    public void InviteFriend(SteamId friend) =>
        Base.InviteFriend(friend);

    public void SendChatBytes(byte[] data) =>
        Base.SendChatBytes(data);

    public void OnProcessingComplete(ulong idVal, RoomEnter result)
    {
        Base = new(idVal);
        EnterResult = result;
        IsProcessing = false;
    }

    public void OnUserStatusChange(Friend user, MemberStateChange status, Friend? responsible)
    {
        UserStatusChange?.Invoke(user, status, responsible);
    }

    public void OnChatMessage(Friend user, byte[] data)
    {
        ChatMessage?.Invoke(user, data);
    }
}

public class WorkshopItem : IProgress<float>
{
    static Dictionary<ulong, WorkshopItem> workshopItems = [];

    public static WorkshopItem GetItem(ulong id)
    {
        if (workshopItems.TryGetValue(id, out var result))
            return result;
        return workshopItems[id] = new(id);
    }

    public bool FinishedProcessing { get; set; }

    public bool NeedsLegal { get; private set; }

    public float Progress { get; private set; }

    public Result Result;

    public PublishedFileId Id;

    public Item? Item { get; private set; }

    public string Name { get; set; }

    public WorkshopItemData Data { get; set; }

    public List<WorkshopItem> Dependencies { get; set; }

    public bool IsSubscribed => Item?.IsSubscribed ?? false;

    public WorkshopItem(ulong id)
    {
        Id = id;
        FinishedProcessing = true;
        Result = Result.OK;
    }

    public async void Subscribe()
    {
        if (Item is Item item)
            await item.Subscribe();
    }

    public async void Unsubscribe()
    {
        if (Item is Item item)
            await item.Unsubscribe();
    }

    public async void Request()
    {
        Item = await SteamUGC.QueryFileAsync(Id);
        if (Item is Item item)
        {
            Data = new()
            {
                VotesUp = item.VotesUp,
                Name = item.Title,
                Description = item.Description,
                ContentFolder = item.Directory,
                PreviewPath = item.PreviewImageUrl,
                ChangeNotes = item.ChangelogUrl,
                Public = item.IsPublic,
                FriendsOnly = item.IsFriendsOnly,
                Private = item.IsPrivate,
                Tags = [.. item.Tags]
            };
        }
    }

    void ApplyResult(Result result, bool legalAgreement, PublishedFileId id)
    {
        Result = result;
        NeedsLegal = legalAgreement;
        Id = id;
        FinishedProcessing = true;
    }

    public void Publish()
    {
        _ = Steamworks.Ugc.Editor.NewCommunityFile
            .ForAppId(FacepunchSteam.AppId)
            .SubmitAsync(this, publishResult => 
                ApplyResult(publishResult.Result, publishResult.NeedsWorkshopAgreement, publishResult.FileId))
            .GetAwaiter()
            .GetResult();
    }

    public async void AddDependency(WorkshopItem dependency)
    {
        if (Item is Item item)
            await item.AddDependency(dependency.Id);
    }

    public async void RemoveDependency(WorkshopItem dependency)
    {
        if (Item is Item item)
            await item.RemoveDependency(dependency.Id);
    }

    public void ResetProcessing()
    {
        FinishedProcessing = false;
        NeedsLegal = false;
    }

    public async void ApplyWorkshopData(WorkshopItemData data)
    {
        Data = data;
        Steamworks.Ugc.Editor editor = new(Id);
        if (!string.IsNullOrEmpty(data.Name))
        {
            editor = editor.WithTitle(data.Name);
            if (data.Public) editor = editor.WithPublicVisibility();
            else if (data.FriendsOnly) editor = editor.WithFriendsOnlyVisibility();
            else if (data.Private) editor = editor.WithPrivateVisibility();
        }

        if (!string.IsNullOrEmpty(data.Description))
            editor = editor.WithDescription(data.Description);

        if (data.Tags?.Count > 0)
        {
            foreach (var tag in data.Tags)
                editor = editor.WithTag(tag);
        }

        editor = editor.WithPreviewFile(Data.PreviewPath);
        editor = editor.WithContent(data.ContentFolder);
        if (!string.IsNullOrEmpty(data.ChangeNotes))
            editor = editor.WithChangeLog(data.ChangeNotes);
        var publishResult = await editor.SubmitAsync(this);
        ApplyResult(publishResult.Result, publishResult.NeedsWorkshopAgreement, publishResult.FileId);
    }

    void IProgress<float>.Report(float value)
    {
        Progress = value;
    }
}

public class WorkshopItemData
{
    public bool Public;
    public bool FriendsOnly;
    public bool Private;

    public uint VotesUp;

    public string Name;
    public string Description;
    public string ContentFolder;
    public string PreviewPath;
    public string ChangeNotes;

    public List<string> Tags;
}

#endregion

#region Enums

public enum WorkshopQueryFilterOrder
{
    RankedByTotalUniqueSubscriptions = 12,
    RankedByTextSearch = 11,
    RankedByVotesUp = 10,
    RankedByTotalVotesAsc = 9,
    NotYetRated = 8,
    CreatedByFollowedUsersRankedByPublicationDate = 7,
    RankedByNumTimesReported = 6,
    CreatedByFriendsRankedByPublicationDate = 5,
    FavoritedByFriendsRankedByPublicationDate = 4,
    RankedByTrend = 3,
    AcceptedForGameRankedByAcceptanceDate = 2,
    RankedByPublicationDate = 1,
    RankedByVote = 0
}

public enum MemberStateChange
{
    Entered = 1,
    Left = 2,
    Disconnected = 4,
    Kicked = 8,
    Banned = 16
}

public enum LobbyVisibility
{
    Private = 0,
    FriendsOnly = 1,
    Public = 2,
    Invisible = 3
}

#endregion

#endif