﻿using System;
using Sandbox.ModAPI;
using VRageMath;
using WeaponCore.Support;

namespace WeaponCore
{
    public partial class Session
    {
        #region Network sync
        internal void PacketizeToClientsInRange(IMyFunctionalBlock block, PacketBase packet)
        {
            try
            {
                var bytes = MyAPIGateway.Utilities.SerializeToBinary(packet);
                var localSteamId = MyAPIGateway.Multiplayer.MyId;
                foreach (var p in Players.Values)
                {
                    var id = p.SteamUserId;
                    if (id != localSteamId && id != packet.SenderId && Vector3D.DistanceSquared(p.GetPosition(), block.PositionComp.WorldAABB.Center) <= SyncBufferedDistSqr)
                        MyAPIGateway.Multiplayer.SendMessageTo(PACKET_ID, bytes, p.SteamUserId);
                }
            }
            catch (Exception ex) { Log.Line($"Exception in PacketizeToClientsInRange: {ex}"); }
        }

        private void ReceivedPacket(byte[] rawData)
        {
            try
            {
                var packet = MyAPIGateway.Utilities.SerializeFromBinary<PacketBase>(rawData);
                if (packet.Received(IsServer) && packet.Entity != null)
                {
                    var localSteamId = MyAPIGateway.Multiplayer.MyId;
                    foreach (var p in Players.Values)
                    {
                        var id = p.SteamUserId;
                        if (id != localSteamId && id != packet.SenderId && Vector3D.DistanceSquared(p.GetPosition(), packet.Entity.PositionComp.WorldAABB.Center) <= SyncBufferedDistSqr)
                            MyAPIGateway.Multiplayer.SendMessageTo(PACKET_ID, rawData, p.SteamUserId);
                    }
                }
            }
            catch (Exception ex) { Log.Line($"Exception in ReceivedPacket: {ex}"); }
        }
        #endregion
    }
}