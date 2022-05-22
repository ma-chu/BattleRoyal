using Photon.Bolt;
using UnityEngine;

// Чтобы была репликация и пр. нужна BoltEntity. 
// Пока же PhotonPlayerObject будет хранить лишь id соединения и имя
public class PhotonPlayerObject
{
//    public BoltEntity character;
    public BoltConnection connection;        // null, если игрок подключается к серверу с сервера же
    public string name;
    public bool IsServer
    {
        get { return connection == null; }
    }
    public bool IsClient
    {
        get { return connection != null; }
    }
    
/*    public void Spawn()                    // пока не буду использовать, понадобится, если придется контролировать сущности сервером (клиент изменяет их с помощью команд серверу)
    {
        if (!character && IsServer)
        {
            character = BoltNetwork.Instantiate(BoltPrefabs.HeroBoltEntity, Vector3.zero, Quaternion.identity);

            /*if (IsServer)
            {
                character.TakeControl();
            }
            else
            {
                character.AssignControl(connection);
            }*//*
        }

        // teleport entity to a random spawn position
        //character.transform.position = RandomPosition();
    }
*/
    public void SetName(string name)
    {
        this.name = name;
    }
}
