﻿using ProtoBuf;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using System.Collections.Generic;
using System.ComponentModel;
using VRage;
using VRageMath;
using WeaponCore.Support;
using static WeaponCore.Platform.Weapon;
using static WeaponCore.Support.Target;

namespace WeaponCore
{

    public enum PacketType
    {
        Invalid,
        GridSyncRequestUpdate,
        CompStateUpdate,
        CompSettingsUpdate,
        WeaponSyncUpdate,
        WeaponPacket,
        FakeTargetUpdate,
        ClientMouseEvent,
        ActiveControlUpdate,
        PlayerIdUpdate,
        ActiveControlFullUpdate,
        FocusUpdate,
        MagUpdate,
        ReticleUpdate,
        OverRidesUpdate,
        PlayerControlUpdate,
        TargetExpireUpdate,
        WeaponUpdateRequest,
        ClientEntityClosed,
        RequestMouseStates,
        FullMouseUpdate,
        CompToolbarShootState,
        WeaponToolbarShootState,
        RangeUpdate,
        GridAiUiMidUpdate,
        CycleAmmo,
        ReassignTargetUpdate,
        NextActiveUpdate,
        ReleaseActiveUpdate,
        GridOverRidesSync,
        RescanGroupRequest,
        GridFocusListSync,
        FixedWeaponHitEvent,
    }

    #region packets
    [ProtoContract]
    [ProtoInclude(4, typeof(GridWeaponPacket))]
    [ProtoInclude(5, typeof(InputPacket))]
    [ProtoInclude(6, typeof(BoolUpdatePacket))]
    [ProtoInclude(7, typeof(FakeTargetPacket))]
    [ProtoInclude(8, typeof(CurrentGridPlayersPacket))]
    [ProtoInclude(9, typeof(FocusPacket))]
    [ProtoInclude(10, typeof(WeaponIdPacket))]
    [ProtoInclude(11, typeof(RequestTargetsPacket))]
    [ProtoInclude(12, typeof(MouseInputSyncPacket))]
    [ProtoInclude(13, typeof(GridOverRidesSyncPacket))]
    [ProtoInclude(14, typeof(GridFocusListPacket))]
    [ProtoInclude(15, typeof(FixedWeaponHitPacket))]
    [ProtoInclude(16, typeof(MIdPacket))]
    public class Packet
    {
        [ProtoMember(1)] internal long EntityId;
        [ProtoMember(2)] internal ulong SenderId;
        [ProtoMember(3)] internal PacketType PType;

        public virtual void CleanUp()
        {
            EntityId = 0;
            SenderId = 0;
            PType = PacketType.Invalid;
        }

        //can override in other packet
        protected bool Equals(Packet other)
        {
            return (EntityId.Equals(other.EntityId) && SenderId.Equals(other.SenderId) && PType.Equals(other.PType));
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Packet)obj);
        }

        public override int GetHashCode()
        {
            return (EntityId.GetHashCode() + PType.GetHashCode() + SenderId.GetHashCode());
        }
    }

    [ProtoContract]
    public class GridWeaponPacket : Packet
    {
        [ProtoMember(1)] internal List<WeaponData> Data;
        public GridWeaponPacket() { }

        public override void CleanUp()
        {
            base.CleanUp();
            Data = null;
        }
    }

    [ProtoContract]
    public class InputPacket : Packet
    {
        [ProtoMember(1)] internal InputStateData Data;
        public InputPacket() { }

        public override void CleanUp()
        {
            base.CleanUp();
            Data = null;
        }
    }

    [ProtoContract]
    public class BoolUpdatePacket : Packet
    {
        [ProtoMember(1)] internal bool Data;
        public BoolUpdatePacket() { }

        public override void CleanUp()
        {
            base.CleanUp();
            Data = false;
        }
    }

    [ProtoContract]
    public class FakeTargetPacket : Packet
    {
        [ProtoMember(1)] internal Vector3 Data;
        public FakeTargetPacket() { }

        public override void CleanUp()
        {
            base.CleanUp();
            Data = new Vector3();
        }
    }

    [ProtoContract]
    public class CurrentGridPlayersPacket : Packet
    {
        [ProtoMember(1)] internal ControllingPlayersSync Data;
        public CurrentGridPlayersPacket() { }

        public override void CleanUp()
        {
            base.CleanUp();
            Data = new ControllingPlayersSync();
        }
    }

    [ProtoContract]
    public class FocusPacket : Packet
    {
        [ProtoMember(1)] internal long TargetId;
        [ProtoMember(2), DefaultValue(-1)] internal int FocusId;
        [ProtoMember(3)] internal bool AddSecondary;
        public FocusPacket() { }

        public override void CleanUp()
        {
            base.CleanUp();
            TargetId = 0;
            FocusId = -1;
            AddSecondary = false;
        }
    }

    [ProtoContract]
    public class WeaponIdPacket : Packet
    {
        [ProtoMember(1), DefaultValue(-1)] internal int WeaponId = -1;

        public WeaponIdPacket() { }

        public override void CleanUp()
        {
            base.CleanUp();
            WeaponId = -1;
        }
    }

    [ProtoContract]
    public class RequestTargetsPacket : Packet
    {
        [ProtoMember(1)] internal List<long> Comps;

        public RequestTargetsPacket() { }

        public override void CleanUp()
        {
            base.CleanUp();
            Comps.Clear();
        }
    }

    [ProtoContract]
    public class MouseInputSyncPacket : Packet
    {
        [ProtoMember(1)] internal PlayerMouseData[] Data = new PlayerMouseData[0];
        public MouseInputSyncPacket() { }

        public override void CleanUp()
        {
            base.CleanUp();
            Data = new PlayerMouseData[0];
        }
    }

    [ProtoContract]
    public class GridOverRidesSyncPacket : Packet
    {
        [ProtoMember(1)] internal OverRidesData[] Data = new OverRidesData[0];
        public GridOverRidesSyncPacket() { }

        public override void CleanUp()
        {
            base.CleanUp();
            Data = new OverRidesData[0];
        }
    }

    [ProtoContract]
    public class GridFocusListPacket : Packet
    {
        [ProtoMember(1)] internal long[] EntityIds;
        public GridFocusListPacket() { }

        public override void CleanUp()
        {
            base.CleanUp();
            EntityIds = null;
        }
    }

    [ProtoContract]
    public class FixedWeaponHitPacket : Packet
    {
        [ProtoMember(1), DefaultValue(-1)] internal long HitEnt;
        [ProtoMember(2)] internal Vector3 HitDirection;
        [ProtoMember(3)] internal Vector3 HitOffset;
        [ProtoMember(4)] internal Vector3 Up;
        [ProtoMember(5)] internal int MuzzleId;
        [ProtoMember(6), DefaultValue(-1)] internal int WeaponId;

        public FixedWeaponHitPacket() { }

        public override void CleanUp()
        {
            base.CleanUp();
            HitEnt = -1;
            HitDirection = Vector3.Zero;
            HitOffset = Vector3.Zero;
            Up = Vector3.Zero;
            MuzzleId = -1;
            WeaponId = -1;
        }
    }
    #endregion

    #region MId Based Packets
    [ProtoContract]
    [ProtoInclude(21, typeof(RangePacket))]
    [ProtoInclude(22, typeof(CycleAmmoPacket))]
    [ProtoInclude(23, typeof(ShootStatePacket))]
    [ProtoInclude(24, typeof(OverRidesPacket))]
    [ProtoInclude(25, typeof(ControllingPlayerPacket))]
    [ProtoInclude(26, typeof(StatePacket))]
    [ProtoInclude(27, typeof(SettingPacket))]
    public class MIdPacket : Packet
    {
        [ProtoMember(1)] internal uint MId;

        public MIdPacket() { }

        public override void CleanUp()
        {
            base.CleanUp();
            MId = 0;
        }
    }

    [ProtoContract]
    public class RangePacket : MIdPacket
    {
        [ProtoMember(1)] internal float Data;
        public RangePacket() { }

        public override void CleanUp()
        {
            base.CleanUp();
            Data = 0f;
        }
    }

    [ProtoContract]
    public class CycleAmmoPacket : MIdPacket
    {
        [ProtoMember(1), DefaultValue(-1)] internal int AmmoId = -1;
        [ProtoMember(2), DefaultValue(-1)] internal int WeaponId = -1;
        public CycleAmmoPacket() { }

        public override void CleanUp()
        {
            base.CleanUp();
            AmmoId = -1;
            WeaponId = -1;
        }
    }

    [ProtoContract]
    public class ShootStatePacket : MIdPacket
    {
        [ProtoMember(1)] internal ManualShootActionState Data = ManualShootActionState.ShootOff;
        [ProtoMember(2), DefaultValue(-1)] internal int WeaponId = -1;
        public ShootStatePacket() { }

        public override void CleanUp()
        {
            base.CleanUp();
            Data = ManualShootActionState.ShootOff;
            WeaponId = -1;
        }
    }

    [ProtoContract]
    public class OverRidesPacket : MIdPacket
    {
        [ProtoMember(1)] internal GroupOverrides Data;
        [ProtoMember(2), DefaultValue("")] internal string GroupName = "";

        public OverRidesPacket() { }

        public override void CleanUp()
        {
            base.CleanUp();
            Data = null;
            GroupName = "";
        }
    }

    [ProtoContract]
    public class ControllingPlayerPacket : MIdPacket
    {
        [ProtoMember(1)] internal PlayerControl Data;

        public ControllingPlayerPacket() { }

        public override void CleanUp()
        {
            base.CleanUp();
            Data = null;
        }
    }

    [ProtoContract]
    public class StatePacket : MIdPacket
    {
        [ProtoMember(1)] internal CompStateValues Data;

        public StatePacket() { }

        public override void CleanUp()
        {
            base.CleanUp();
            Data = null;
        }
    }

    [ProtoContract]
    public class SettingPacket : MIdPacket
    {
        [ProtoMember(1)] internal CompSettingsValues Data;
        public SettingPacket() { }

        public override void CleanUp()
        {
            base.CleanUp();
            Data = null;
        }
    }

    #endregion

    #region packet Data

    [ProtoContract]
    public class WeaponData
    {
        [ProtoMember(1)] internal TransferTarget TargetData;
        [ProtoMember(2)] internal long CompEntityId;
        [ProtoMember(3)] internal WeaponSyncValues SyncData;
        [ProtoMember(4)] internal WeaponTimings Timmings;
        [ProtoMember(5)] internal WeaponRandomGenerator WeaponRng;

        public WeaponData() { }
    }

    [ProtoContract]
    internal class InputStateData
    {
        [ProtoMember(1)] internal bool MouseButtonLeft;
        [ProtoMember(2)] internal bool MouseButtonMiddle;
        [ProtoMember(3)] internal bool MouseButtonRight;
        [ProtoMember(4)] internal bool InMenu;
    }

    [ProtoContract]
    internal class PlayerMouseData
    {
        [ProtoMember(1)] internal long PlayerId;
        [ProtoMember(2)] internal InputStateData MouseStateData;
    }

    [ProtoContract]
    internal class GroupSettingsData
    {
        [ProtoMember(1)] internal string SettingName;
        [ProtoMember(2)] internal bool Value;
    }

    [ProtoContract]
    public class TransferTarget
    {
        [ProtoMember(1)] public long EntityId;
        [ProtoMember(2)] public Vector3 TargetPos;
        [ProtoMember(3)] public float HitShortDist;
        [ProtoMember(4)] public float OrigDistance;
        [ProtoMember(5)] public long TopEntityId;
        [ProtoMember(6)] public TargetInfo State = TargetInfo.Expired;
        [ProtoMember(7)] public int WeaponId;

        public enum TargetInfo
        {
            IsEntity,
            IsProjectile,
            IsFakeTarget,
            Expired
        }

        internal void SyncTarget(Target target, bool allowChange = true)
        {
            var entity = MyEntities.GetEntityByIdOrDefault(EntityId);
            target.Entity = entity;
            target.TargetPos = TargetPos;
            target.HitShortDist = HitShortDist;
            target.OrigDistance = OrigDistance;
            target.TopEntityId = TopEntityId;

            target.IsProjectile = false;
            target.IsFakeTarget = false;

            if (State == TargetInfo.IsProjectile)
                target.IsProjectile = true;

            else if (State == TargetInfo.IsFakeTarget)
                target.IsFakeTarget = true;

            var state = State != TargetInfo.Expired ? States.Acquired : States.Expired;

            
            target.StateChange(State != TargetInfo.Expired, state);

            if (!allowChange)
                target.TargetChanged = false;
        }

        public TransferTarget()
        {
        }
    }

    [ProtoContract]
    public struct OverRidesData
    {
        [ProtoMember(1)] public string GroupName;
        [ProtoMember(2)] public GroupOverrides Overrides;
    }
    #endregion
}
