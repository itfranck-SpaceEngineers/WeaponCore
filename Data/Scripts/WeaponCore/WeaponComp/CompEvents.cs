﻿using System;
using System.Text;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using VRage.Game.Entity;

namespace WeaponCore.Support
{
    public partial class WeaponComponent
    {
        internal void RegisterEvents(bool register = true)
        {
            if (register)
            {
                //TODO change this
                Registered = true;
                TerminalBlock.AppendingCustomInfo += AppendingCustomInfo;

                MyCube.IsWorkingChanged += IsWorkingChanged;
                IsWorkingChanged(MyCube);

                BlockInventory.ContentsChanged += OnContentsChanged;

            }
            else
            {
                if (Registered)
                {
                    //TODO change this
                    Registered = false;
                    TerminalBlock.AppendingCustomInfo -= AppendingCustomInfo;

                    MyCube.IsWorkingChanged -= IsWorkingChanged;

                    if (BlockInventory == null) Log.Line($"BlockInventory is null");
                    else
                        BlockInventory.ContentsChanged -= OnContentsChanged;
                }
            }
        }

        private void OnContentsChanged(MyInventoryBase obj)
        {
            try
            {
                if (LastInventoryChangedTick < Session.Tick && !IgnoreInvChange && Registered)
                {
                    for (int i = 0; i < Platform.Weapons.Length; i++)
                    {
                        var w = Platform.Weapons[i];
                        if (!w.ActiveAmmoDef.AmmoDef.Const.EnergyAmmo)
                            Session.ComputeStorage(w);
                    }
                    
                    LastInventoryChangedTick = Session.Tick;
                }
            }
            catch (Exception ex) { Log.Line($"Exception in OnContentsChanged: {ex}");
            }
        }

        private void IsWorkingChanged(MyCubeBlock myCubeBlock)
        {
            try
            {
                var wasFunctional = IsFunctional;
                IsFunctional = myCubeBlock.IsFunctional;
                if (!wasFunctional && IsFunctional && IsWorkingChangedTick > 0)
                    Status = Start.ReInit;
                IsWorking = myCubeBlock.IsWorking;
                State.Value.Online = IsWorking && IsFunctional;
                if (MyCube.ResourceSink.CurrentInputByType(GId) < 0) Log.Line($"IsWorking:{IsWorking}(was:{wasFunctional}) - online:{State.Value.Online} - Func:{IsFunctional} - GridAvailPow:{Ai.GridAvailablePower} - SinkPow:{SinkPower} - SinkReq:{MyCube.ResourceSink.RequiredInputByType(GId)} - SinkCur:{MyCube.ResourceSink.CurrentInputByType(GId)}");

                if (!IsWorking && Registered)
                {
                    foreach (var w in Platform.Weapons)
                        w.StopShooting();
                }
                IsWorkingChangedTick = Session.Tick;
            }
            catch (Exception ex) { Log.ThreadedWrite($"Exception in IsWorkingChanged: {ex}"); }
        }

        internal string GetSystemStatus()
        {
            if (!State.Value.Online && !MyCube.IsFunctional) return "[Fault]";
            if (!State.Value.Online && !MyCube.IsWorking) return "[Offline]";
            return "[Online]";
        }

        private void AppendingCustomInfo(IMyTerminalBlock block, StringBuilder stringBuilder)
        {
            try
            {
                var status = GetSystemStatus();

                stringBuilder.Append(status +
                    "\n[Construct DPS]: " + Ai.EffectiveDps.ToString("0.0") +
                    "\n[ShotsPerSec  ]: " + ShotsPerSec.ToString("0.000") +
                    "\n" +
                    "\n[RealDps]: " + EffectiveDps.ToString("0.0") +
                    "\n[PeakDps]: " + PeakDps.ToString("0.0") +
                    "\n[BaseDps]: " + BaseDps.ToString("0.0") +
                    "\n[AreaDps]: " + AreaDps.ToString("0.0") +
                    "\n[Explode]: " + DetDps.ToString("0.0") +
                    "\n[Current]: " + CurrentDps.ToString("0.0") +" ("+ (CurrentDps/ PeakDps).ToString("P") + ")");

                if (HeatPerSecond > 0)
                    stringBuilder.Append("\n__________________________________" +
                    "\n[Heat Generated / s]: " + HeatPerSecond.ToString("0.0") + " W" +
                    "\n[Heat Dissipated / s]: " + HeatSinkRate.ToString("0.0") + " W" +
                    "\n[Current Heat]: " +CurrentHeat.ToString("0.0") + " j (" + (CurrentHeat / MaxHeat).ToString("P")+")");

                if (HeatPerSecond > 0 && HasEnergyWeapon)
                    stringBuilder.Append("\n__________________________________");

                if (HasEnergyWeapon)
                {
                    stringBuilder.Append("\n[Current Draw]: " + SinkPower.ToString("0.00") + " Mw");
                    if(HasChargeWeapon) stringBuilder.Append("\n[Current Charge]: " + State.Value.CurrentCharge.ToString("0.00") + " Mw");
                    stringBuilder.Append("\n[Required Power]: " + MaxRequiredPower.ToString("0.00") + " Mw");
                }

                stringBuilder.Append("\n\n** Use Weapon Wheel Menu\n** to control weapons using\n** MMB outside of this terminal");
                if (Debug)
                {
                    foreach (var weapon in Platform.Weapons)
                    {
                        stringBuilder.Append($"\n\nWeapon: {weapon.System.WeaponName} - Enabled: {weapon.Set.Enable && weapon.Comp.State.Value.Online && weapon.Comp.Set.Value.Overrides.Activate}");
                        stringBuilder.Append($"\nTargetState: {weapon.Target.CurrentState} - Manual: {weapon.Comp.UserControlled || weapon.Target.IsFakeTarget}");
                        stringBuilder.Append($"\nEvent: {weapon.LastEvent} - Ammo :{!weapon.OutOfAmmo}");
                        stringBuilder.Append($"\nOverHeat: {weapon.State.Sync.Overheated} - Shooting: {weapon.IsShooting}");
                        stringBuilder.Append($"\nisAligned: {weapon.Target.IsAligned} - Tracking: {weapon.Target.IsTracking}");
                        stringBuilder.Append($"\nCanShoot: {weapon.Timings.ShootDelayTick <= weapon.Comp.Session.Tick} - Charging: {weapon.State.Sync.Charging}");
                        stringBuilder.Append($"\nAiShooting: {weapon.AiShooting} - lastCheck: {weapon.Comp.Session.Tick - weapon.Target.CheckTick}");
                        stringBuilder.Append($"\n{(weapon.ActiveAmmoDef.AmmoDef.Const.EnergyAmmo ? "ChargeSize: " + weapon.ActiveAmmoDef.AmmoDef.Const.ChargSize.ToString() : "MagSize: " +  weapon.ActiveAmmoDef.AmmoDef.Const.MagazineSize.ToString())} - CurrentCharge: {State.Value.CurrentCharge}({weapon.State.Sync.CurrentCharge})");
                        stringBuilder.Append($"\nChargeTime: {weapon.Timings.ChargeUntilTick}({weapon.Comp.Ai.Session.Tick}) - Delay: {weapon.Timings.ChargeDelayTicks}");
                        stringBuilder.Append($"\nCharging: {weapon.State.Sync.Charging}({weapon.ActiveAmmoDef.AmmoDef.Const.MustCharge}) - Delay: {weapon.Timings.ChargeDelayTicks}");
                    }
                }
            }
            catch (Exception ex) { Log.Line($"Exception in Weapon AppendingCustomInfo: {ex}"); }
        }
    }
}
