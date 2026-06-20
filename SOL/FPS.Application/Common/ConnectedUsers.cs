using System.Collections.Concurrent;

namespace Template.Application.Common;

public static class ConnectedUsers
{
    public static ConcurrentDictionary<string, ConcurrentDictionary<string, HubType>> UserConnections
        = new();

    // Add connection
    public static void AddConnection(string userId, string connectionId, HubType hubType)
    {
        var connections = UserConnections.GetOrAdd(userId, _ => new ConcurrentDictionary<string, HubType>());
        connections[connectionId] = hubType;
    }

    // Remove connection
    public static void RemoveConnection(string userId, string connectionId)
    {
        if (!UserConnections.TryGetValue(userId, out var connections))
            return;

        connections.TryRemove(connectionId, out _);

        if (connections.IsEmpty)
            UserConnections.TryRemove(userId, out _);
    }

    // Check if user has any connection (to any hub type)
    public static bool IsUserOnline(string userId)
    {
        return UserConnections.TryGetValue(userId, out var connections) && !connections.IsEmpty;
    }

    // Check if user is online for a specific hub type only
    public static bool IsUserOnlineForHub(string userId, HubType hubType)
    {
        return UserConnections.TryGetValue(userId, out var connections)
               && connections.Values.Any(x => x == hubType);
    }

    // Check if user connected to specific hub (alias for backward compatibility)
    public static bool IsUserConnectedToHub(string userId, HubType hubType)
    {
        return IsUserOnlineForHub(userId, hubType);
    }

    // Get all connections for user
    public static IEnumerable<string> GetUserConnections(string userId)
    {
        if (UserConnections.TryGetValue(userId, out var connections))
            return connections.Keys;

        return Enumerable.Empty<string>();
    }

    
    public static IEnumerable<string> GetUserConnectionsByHub(string userId, HubType hubType)
    {
        if (!UserConnections.TryGetValue(userId, out var connections))
            return Enumerable.Empty<string>();

        return connections
            .Where(x => x.Value == hubType)
            .Select(x => x.Key);
    }

    // Get count of connections for specific hub type
    public static int GetUserConnectionsCountByHub(string userId, HubType hubType)
    {
        if (!UserConnections.TryGetValue(userId, out var connections))
            return 0;

        return connections.Values.Count(x => x == hubType);
    }

    // Get all hub types the user is connected to
    public static IEnumerable<HubType> GetUserConnectedHubTypes(string userId)
    {
        if (!UserConnections.TryGetValue(userId, out var connections))
            return Enumerable.Empty<HubType>();

        return connections.Values.Distinct();
    }
}