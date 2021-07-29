using System.Collections;
using System.Collections.Generic;
using Bolt;
using Bolt.Utils;
using UnityEngine;

public class PlayerServerToken : IProtocolToken
{
    public BoltEntity enemyEntity;
    
    public void Write(UdpKit.UdpPacket packet) {
        packet.WriteBoltEntity(enemyEntity);
    }

    public void Read(UdpKit.UdpPacket packet) {
        enemyEntity = packet.ReadBoltEntity();
    }
}
