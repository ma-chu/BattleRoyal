using System.Collections;
using System.Collections.Generic;
using Bolt;
using Bolt.Utils;
using UnityEngine;

public class PlayerClientToken : IProtocolToken
{
    public string username;
    /*public int inventory1;
    public int inventory2;
    public int inventory3;*/
    
    public void Write(UdpKit.UdpPacket packet) {
        packet.WriteString(username);
        /*packet.WriteInt(inventory1);
        packet.WriteInt(inventory2);
        packet.WriteInt(inventory3);*/
    }

    public void Read(UdpKit.UdpPacket packet) {
        username = packet.ReadString();
        /*inventory1 = packet.ReadInt();
        inventory2 = packet.ReadInt();
        inventory3 = packet.ReadInt();*/
    }
}
