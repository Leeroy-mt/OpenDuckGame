using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace DuckGame;

public class NCBasic : NCNetworkImplementation
{
	private bool _initializedSettings = true;

	public const long kLanMessageHeader = 2449832521355936907L;

	protected UdpClient _socket;

	private byte[] _receiveBuffer = new byte[4096];

	private string _serverIdentifier = "";

	private List<NCBasicConnection> _basicConnections = new List<NCBasicConnection>();

	public int bytesThisFrame;

	public int headerBytes;

	public int ghostBytes;

	public int ackBytes;

	private const long kDuckGameLANHeader = 5892070176735L;

	private int _port;

	protected IPEndPoint localEndPoint;

	public static string _localName = "";

	private float _broadcastWait;

	public List<UIServerBrowser.LobbyData> _foundLobbies = new List<UIServerBrowser.LobbyData>();

	public List<UIServerBrowser.LobbyData> _threadLobbies;

	private object lobbyLock = new object();

	private bool _lobbyThreadRunning;

	public static int lobbySearchPort = 1337;

	public NCBasic(Network c, int networkIndex)
		: base(c, networkIndex)
	{
	}

	public override NCError OnSendPacket(byte[] data, int length, object connection)
	{
		byte[] newData = new byte[length + 8];
		BitBuffer b = new BitBuffer(newData, copyData: false);
		b.Write(2449832521355936907L);
		if (data != null)
		{
			b.Write(data, 0, length);
		}
		_socket.Send(newData, length + 8, connection as IPEndPoint);
		bytesThisFrame += length + 8;
		return null;
	}

	protected override object GetConnectionObject(string identifier)
	{
		return MakeConnection(CreateIPEndPoint(identifier)).connection;
	}

	private void BroadcastServerHeader()
	{
		if (_socket != null)
		{
			BitBuffer send = new BitBuffer();
			send.Write(5892070176735L);
			send.Write(DG.versionMajor);
			send.Write(DG.versionHigh);
			send.Write(DG.versionLow);
			send.Write((byte)DuckNetwork.profiles.Where((Profile x) => x.connection != null).Count());
			send.Write(TeamSelect2.GetOnlineSetting("name").value);
			send.Write(ModLoader.modHash);
			send.Write((byte)TeamSelect2.GetSettingInt("requiredwins"));
			send.Write((byte)TeamSelect2.GetSettingInt("restsevery"));
			send.Write(TeamSelect2.GetSettingBool("wallmode"));
			send.Write(Editor.customLevelCount);
			send.Write(!(Level.current is TeamSelect2));
			send.Write(TeamSelect2.UpdateModifierStatus());
			send.Write(DuckNetwork.numSlots);
			send.Write((string)TeamSelect2.GetOnlineSetting("password").value != "");
			send.Write((bool)TeamSelect2.GetOnlineSetting("dedicated").value);
			send.Write(val: true);
			send.Write(Network.gameDataHash);
			_socket.Send(send.buffer, send.lengthInBytes, "255.255.255.255", _port);
		}
	}

	public override NCError OnHostServer(string identifier, int port, NetworkLobbyType lobbyType, int maxConnections)
	{
		if (_socket == null)
		{
			_basicConnections.Clear();
			_serverIdentifier = identifier;
			_socket = new UdpClient();
			_socket.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, optionValue: true);
			_socket.Client.Bind(new IPEndPoint(IPAddress.Any, port));
			_socket.AllowNatTraversal(allowed: true);
			if (NetworkDebugger.enabled)
			{
				localEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1330 + NetworkDebugger.currentIndex);
			}
			else
			{
				localEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), _port);
			}
			_port = port;
			_socket.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, optionValue: true);
			StartServerThread();
			return new NCError("server started on port " + port + ".", NCErrorType.Success);
		}
		return new NCError("server is already started...", NCErrorType.Error);
	}

	public static IPEndPoint CreateIPEndPoint(string endPoint)
	{
		string[] ep = endPoint.Split(':');
		if (ep.Length < 2)
		{
			throw new FormatException("Invalid endpoint format");
		}
		IPAddress ip;
		if (ep.Length > 2)
		{
			if (!IPAddress.TryParse(string.Join(":", ep, 0, ep.Length - 1), out ip))
			{
				throw new FormatException("Invalid ip-adress");
			}
		}
		else if (!IPAddress.TryParse(ep[0], out ip))
		{
			throw new FormatException("Invalid ip-adress");
		}
		if (!int.TryParse(ep[ep.Length - 1], NumberStyles.None, NumberFormatInfo.CurrentInfo, out var port))
		{
			throw new FormatException("Invalid port");
		}
		return new IPEndPoint(ip, port);
	}

	public override NCError OnJoinServer(string identifier, int port, string ip)
	{
		switch (ip)
		{
		case null:
		case "":
		case "localhost":
			return new NCError("Invalid LAN IP String, format must be IP:PORT", NCErrorType.CriticalError);
		default:
			if (_socket == null)
			{
				IPEndPoint endPoint = null;
				endPoint = ((!(ip == "netdebug")) ? CreateIPEndPoint(ip) : new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1330 + NetworkDebugger.CurrentServerIndex()));
				_serverIdentifier = identifier;
				_basicConnections.Clear();
				_socket = new UdpClient();
				_socket.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, optionValue: true);
				_port = port;
				int localPort = 1336;
				if (NetworkDebugger.enabled)
				{
					localPort = 1330 + NetworkDebugger.currentIndex;
					localEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), localPort);
				}
				else
				{
					localEndPoint = new IPEndPoint(IPAddress.Any, localPort);
				}
				_socket.Client.Bind(localEndPoint);
				_socket.AllowNatTraversal(allowed: true);
				MakeConnection(endPoint, isHost: true);
				StartClientThread();
				return null;
			}
			return new NCError("client is already started...", NCErrorType.Error);
		}
	}

	public NCBasicConnection MakeConnection(IPEndPoint endPoint, bool isHost = false)
	{
		lock (_basicConnections)
		{
			NCBasicConnection connection = _basicConnections.FirstOrDefault((NCBasicConnection x) => x.connection.ToString() == endPoint.ToString());
			if (connection != null)
			{
				return connection;
			}
			connection = new NCBasicConnection
			{
				connection = endPoint,
				status = NCBasicStatus.TryingToConnect
			};
			connection.isHost = isHost;
			_basicConnections.Add(connection);
			_pendingMessages.Enqueue(new NCError("client connecting to " + endPoint.ToString() + ".", NCErrorType.Message));
			return connection;
		}
	}

	public override string GetConnectionIdentifier(object connection)
	{
		return (connection as IPEndPoint).ToString();
	}

	public override string GetConnectionName(object connection)
	{
		return (connection as IPEndPoint).ToString().ToString();
	}

	protected override string OnGetLocalName()
	{
		return _localName;
	}

	protected override NCError OnSpinServerThread()
	{
		if (_socket == null)
		{
			if (NetworkDebugger.enabled)
			{
				return null;
			}
			return new NCError("NCBasic connection was lost.", NCErrorType.CriticalError);
		}
		for (int i = 0; i < _basicConnections.Count; i++)
		{
			NCBasicConnection c = _basicConnections[i];
			if (c.status == NCBasicStatus.Disconnected || (!(c.heartbeat.elapsed.TotalSeconds > 1.0) && c.status != NCBasicStatus.TryingToConnect))
			{
				continue;
			}
			if (c.status == NCBasicStatus.Disconnecting)
			{
				if (c.heartbeat.elapsed.TotalSeconds > 3.0)
				{
					c.status = NCBasicStatus.Disconnected;
					c.packets = 0;
				}
				continue;
			}
			c.heartbeat.Restart();
			new BitBuffer();
			OnSendPacket(null, 0, c.connection);
			if (c.status == NCBasicStatus.TryingToConnect)
			{
				c.status = NCBasicStatus.Connecting;
			}
		}
		Queue<NCBasicPacket> packets = new Queue<NCBasicPacket>();
		ReceivePackets(packets);
		foreach (NCBasicPacket item in packets)
		{
			IPEndPoint remote = item.sender;
			byte[] bytes = item.data;
			string address = remote.ToString();
			lock (_basicConnections)
			{
				if (bytes.Length < 8 || new BitBuffer(bytes).ReadLong() != 2449832521355936907L)
				{
					continue;
				}
				NCBasicConnection connection = _basicConnections.FirstOrDefault((NCBasicConnection x) => x.connection.ToString() == address);
				if (connection == null)
				{
					connection = new NCBasicConnection
					{
						connection = remote,
						status = NCBasicStatus.Connecting
					};
					_basicConnections.Add(connection);
					_pendingMessages.Enqueue(new NCError("connection attempt from " + connection.connection.ToString(), NCErrorType.Message));
				}
				if (connection != null)
				{
					connection.packets++;
					if (connection.status == NCBasicStatus.Connected && bytes.Length > 8)
					{
						byte[] newBytes = new byte[bytes.Length - 8];
						Array.Copy(bytes, 8, newBytes, 0, bytes.Length - 8);
						OnPacket(newBytes, connection.connection);
					}
				}
			}
		}
		return null;
	}

	protected virtual void ReceivePackets(Queue<NCBasicPacket> packets)
	{
		try
		{
			IPEndPoint remote = null;
			byte[] bytes = null;
			while (_socket.Available > 0)
			{
				bytes = _socket.Receive(ref remote);
				if (bytes != null)
				{
					packets.Enqueue(new NCBasicPacket
					{
						data = bytes,
						sender = remote
					});
				}
			}
		}
		catch
		{
		}
	}

	protected override NCError OnSpinClientThread()
	{
		return OnSpinServerThread();
	}

	public override void Update()
	{
		if (Network.isActive)
		{
			if (Network.isServer)
			{
				_broadcastWait += Maths.IncFrameTimer();
				if (_broadcastWait > 0.75f)
				{
					_broadcastWait = 0f;
					BroadcastServerHeader();
				}
			}
			for (int i = 0; i < _basicConnections.Count; i++)
			{
				NCBasicConnection connection = _basicConnections[i];
				if (connection.status != NCBasicStatus.Connected && connection.packets > 0 && connection.status != NCBasicStatus.Disconnecting)
				{
					_pendingMessages.Enqueue(new NCError("connection to " + connection.connection.ToString() + " succeeded!", NCErrorType.Success));
					connection.status = NCBasicStatus.Connected;
					AttemptConnection(connection.connection, connection.isHost);
					connection.isHost = false;
				}
			}
		}
		base.Update();
	}

	protected override void KillConnection()
	{
		_basicConnections.Clear();
		if (_socket != null)
		{
			_socket.Close();
		}
		_socket = null;
		base.KillConnection();
	}

	protected override void Disconnect(NetworkConnection c)
	{
		if (c != null)
		{
			NCBasicConnection connection = _basicConnections.FirstOrDefault((NCBasicConnection x) => x.connection.ToString() == c.identifier);
			if (connection != null)
			{
				connection.status = NCBasicStatus.Disconnecting;
			}
		}
		base.Disconnect(c);
	}

	public override void Terminate()
	{
		base.Terminate();
	}

	public override void AddLobbyStringFilter(string key, string value, LobbyFilterComparison op)
	{
		Steam.AddLobbyStringFilter(key, value, (SteamLobbyComparison)op);
	}

	public override bool IsLobbySearchComplete()
	{
		if (_lobbyThreadRunning)
		{
			return false;
		}
		if (_threadLobbies != null)
		{
			lock (lobbyLock)
			{
				_foundLobbies = _threadLobbies;
				_threadLobbies = null;
			}
		}
		return true;
	}

	private void SearchForLobbyThread()
	{
		_lobbyThreadRunning = true;
		lock (lobbyLock)
		{
			_threadLobbies = new List<UIServerBrowser.LobbyData>();
			using UdpClient udpClient = new UdpClient();
			udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, optionValue: true);
			udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, lobbySearchPort));
			udpClient.Client.ReceiveTimeout = 100;
			IPEndPoint from = new IPEndPoint(0L, 0);
			for (int i = 0; i < 15; i++)
			{
				try
				{
					byte[] recvBuffer = udpClient.Receive(ref from);
					if (recvBuffer == null)
					{
						continue;
					}
					BitBuffer b = new BitBuffer(recvBuffer);
					long num = b.ReadLong();
					string address = from.ToString();
					if (num == 5892070176735L && _threadLobbies.FirstOrDefault((UIServerBrowser.LobbyData x) => x.lanAddress == address) == null)
					{
						UIServerBrowser.LobbyData dat = new UIServerBrowser.LobbyData();
						dat.lanAddress = address;
						int major = b.ReadInt();
						int high = b.ReadInt();
						int low = b.ReadInt();
						dat.version = DG.MakeVersionString(major, high, low);
						dat._userCount = b.ReadByte();
						dat.name = b.ReadString();
						dat.modHash = b.ReadString();
						dat.requiredWins = b.ReadByte().ToString();
						dat.restsEvery = b.ReadByte().ToString();
						dat.wallMode = (b.ReadBool() ? "true" : "false");
						dat.customLevels = b.ReadInt().ToString();
						dat.started = (b.ReadBool() ? "true" : "false");
						dat.hasModifiers = (b.ReadBool() ? "true" : "false");
						dat.numSlots = (dat.maxPlayers = b.ReadInt());
						dat.hasPassword = b.ReadBool();
						dat.dedicated = b.ReadBool();
						if (b.positionInBits != b.lengthInBits && b.ReadBool())
						{
							dat.datahash = b.ReadLong();
						}
						_threadLobbies.Add(dat);
						i--;
					}
				}
				catch (Exception)
				{
				}
			}
			udpClient.Close();
		}
		_lobbyThreadRunning = false;
	}

	public override void SearchForLobby()
	{
		if (!_lobbyThreadRunning)
		{
			_foundLobbies.Clear();
			new Thread((ThreadStart)delegate
			{
				SearchForLobbyThread();
			}).Start();
		}
	}

	public override int NumLobbiesFound()
	{
		return _foundLobbies.Count;
	}

	public override bool TryRequestDailyKills(out long kills)
	{
		kills = 0L;
		if (!Steam.waitingForGlobalStats)
		{
			kills = (long)Steam.GetDailyGlobalStat("kills");
			return true;
		}
		return false;
	}

	public override void ApplyTS2LobbyFilters()
	{
		foreach (MatchSetting s in TeamSelect2.matchSettings)
		{
			if (s.value is int)
			{
				if (s.filtered)
				{
					Steam.AddLobbyNumericalFilter(s.id, (int)s.value, (SteamLobbyComparison)s.filterMode);
				}
				else if (!s.filtered)
				{
					Steam.AddLobbyNearFilter(s.id, (int)s.defaultValue);
				}
			}
			if (s.value is bool)
			{
				if (s.filtered)
				{
					Steam.AddLobbyNumericalFilter(s.id, ((bool)s.value) ? 1 : 0, (SteamLobbyComparison)s.filterMode);
				}
				else if (!s.filtered)
				{
					Steam.AddLobbyNearFilter(s.id, ((bool)s.defaultValue) ? 1 : 0);
				}
			}
		}
		foreach (MatchSetting s2 in TeamSelect2.onlineSettings)
		{
			if (s2.value is int)
			{
				if (s2.filtered)
				{
					Steam.AddLobbyNumericalFilter(s2.id, (int)s2.value, (SteamLobbyComparison)s2.filterMode);
				}
				else if (!s2.filtered)
				{
					Steam.AddLobbyNearFilter(s2.id, (int)s2.defaultValue);
				}
			}
			if (!(s2.value is bool))
			{
				continue;
			}
			if (s2.id == "modifiers")
			{
				if (s2.filtered)
				{
					Steam.AddLobbyStringFilter(s2.id, ((bool)s2.value) ? "true" : "false", SteamLobbyComparison.Equal);
				}
			}
			else if (s2.id == "customlevelsenabled")
			{
				if (s2.filtered)
				{
					if ((bool)s2.value)
					{
						Steam.AddLobbyNumericalFilter(s2.id, 0, SteamLobbyComparison.GreaterThan);
					}
					else
					{
						Steam.AddLobbyNumericalFilter(s2.id, 0, SteamLobbyComparison.Equal);
					}
				}
			}
			else if (s2.filtered)
			{
				Steam.AddLobbyNumericalFilter(s2.id, ((bool)s2.value) ? 1 : 0, (SteamLobbyComparison)s2.filterMode);
			}
			else if (!s2.filtered)
			{
				Steam.AddLobbyNearFilter(s2.id, ((bool)s2.defaultValue) ? 1 : 0);
			}
		}
	}

	public override void AddLobbyNumericalFilter(string key, int value, LobbyFilterComparison op)
	{
		Steam.AddLobbyNumericalFilter(key, value, (SteamLobbyComparison)op);
	}

	public override void RequestGlobalStats()
	{
		Steam.RequestGlobalStats();
	}

	public override Lobby GetSearchLobbyAtIndex(int i)
	{
		return Steam.GetSearchLobbyAtIndex(i);
	}
}
