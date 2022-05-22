using System.Collections.Generic;
using Photon.Bolt;

public static class PhotonPlayerObjectRegisty
{
    private static List<PhotonPlayerObject> players = new List<PhotonPlayerObject>();

    // create a player for a connection
    // note: connection can be null
    private static PhotonPlayerObject CreatePlayer(BoltConnection connection)
    {
        PhotonPlayerObject player;

        // create a new player object, assign the connection property
        // of the object to the connection was passed in
        player = new PhotonPlayerObject();
        player.connection = connection;

        // if we have a connection, assign this player
        // as the user data for the connection so that we
        // always have an easy way to get the player object
        // for a connection
        if (player.connection != null)
        {
            player.connection.UserData = player;                    // Еще раз: в PlayerObject есть поле BoltConnection,
        }                                                           // теперь мы и в св-ва BoltConnection помещаем ссылку на PlayerObject

        // add to list of all players
        players.Add(player);

        return player;
    }

    // this simply returns the 'players' list cast to
    // an IEnumerable<T> so that we hide the ability
    // to modify the player list from the outside.
    public static IEnumerable<PhotonPlayerObject> AllPlayers
    {
        get { return players; }
    }

    // finds the server player by checking the
    // .IsServer property for every player object.
    public static PhotonPlayerObject ServerPhotonPlayer
    {
        get { return players.Find(player => player.IsServer); }
    }

    // utility function which creates a server player
    public static PhotonPlayerObject CreateServerPlayer()
    {
        return CreatePlayer(null);
    }

    // utility that creates a client player object.
    public static PhotonPlayerObject CreateClientPlayer(BoltConnection connection)
    {
        return CreatePlayer(connection);
    }

    // utility function which lets us pass in a
    // BoltConnection object (even a null) and have
    // it return the proper player object for it.
    public static PhotonPlayerObject GetPlayer(BoltConnection connection)
    {
        if (connection == null)
        {
            return ServerPhotonPlayer;
        }

        return (PhotonPlayerObject) connection.UserData;
    }
    
    public static PhotonPlayerObject GetPlayer(string playerName)
    {
        return players.Find(pl=> pl.name.Equals(playerName));
    }
}
