namespace DuckGame;

public enum DuckNetError
{
	NewSessionRequested = 0,
	ControlledDisconnect = 1,
	HostDisconnected = 2,
	ClientDisconnected = 3,
	ConnectionTimeout = 4,
	VersionMismatch = 5,
	FullServer = 6,
	InvalidLevel = 7,
	InvalidUser = 8,
	InvalidCustomHat = 9,
	GameInProgress = 10,
	Kicked = 11,
	HostIgnoredMessage = 12,
	InvalidConnectionInformation = 13,
	ModsIncompatible = 14,
	ConnectionTrouble = 15,
	ConnectionLost = 16,
	EveryoneDisconnected = 17,
	YourVersionTooNew = 18,
	YourVersionTooOld = 19,
	ParentalControls = 20,
	GameNotFoundOrClosed = 21,
	InvalidPassword = 22,
	ClientCrashed = 23,
	Banned = 24,
	HostIsABlockedUser = 25,
	UnknownError = 100,
	None = 101
}
