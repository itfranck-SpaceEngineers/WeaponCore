﻿using Sandbox.ModAPI;
using VRage.Collections;
using VRage.Game.VisualScripting;
using VRageMath;
using WeaponCore.Platform;

namespace WeaponCore.Support
{
    public partial class WeaponComponent 
    {
        internal void TerminalRefresh(bool update = true)
        {
            if (IsAIOnlyTurret)
                AIOnlyTurret.RefreshCustomInfo();
            else
                ControllableTurret.RefreshCustomInfo();

            if (update && InControlPanel)
            {
                 MyCube.UpdateTerminal();
            }
        }

        private void SaveAndSendAll()
        {
            _firstSync = true;
            if (!_isServer) return;
            Set.SaveSettings();
            Set.NetworkUpdate();
            State.SaveState();
            State.NetworkUpdate();
        }

        internal void UpdateCompPower()
        {
            var shooting = false;
            for (int i = 0; i < Platform.Weapons.Length; i++)
            {
                if (Platform.Weapons[i].IsShooting && Platform.Weapons[i].System.EnergyAmmo) shooting = true;
            }
            if (shooting)
            {
                if (!Ai.AvailablePowerIncrease)
                {
                    if (Ai.ResetPower)
                    {
                        //Log.Line($"grid available: {Ai.GridAvailablePower + Ai.CurrentWeaponsDraw}");
                        Ai.WeaponCleanPower = Ai.GridMaxPower - (Ai.GridCurrentPower - Ai.CurrentWeaponsDraw);
                        Ai.ResetPower = false;
                    }

                    SinkPower = CompPowerPerc * Ai.WeaponCleanPower;

                    DelayTicks += (uint)(5 * MaxRequiredPower / SinkPower) - DelayTicks;
                    ShootTick = DelayTicks + Ai.Session.Tick;
                    Ai.RecalcDone = true;
                }
                else
                {
                    SinkPower = CurrentSinkPowerRequested;
                    Ai.ResetPower = true;
                }

                Sink.Update();
                TerminalRefresh();
            }            
        }

        public void StopRotSound(bool force)
        {
            if (RotationEmitter != null)
            {
                if (!RotationEmitter.IsPlaying)
                    return;
                RotationEmitter.StopSound(force);
            }
        }

        public void StopAllSounds()
        {
            RotationEmitter?.StopSound(true, true);
            if (Platform == null)
            {
                Log.Line($"[StopAllSounds] MyCubeId:{MyCube.EntityId} - Grid:{MyCube.CubeGrid.DebugName} - WeaponName:{MyCube.BlockDefinition.Id.SubtypeId.String} - !Marked:{!MyCube.MarkedForClose} - inScene:{MyCube.InScene} - gridMatch:{MyCube.CubeGrid == Ai.MyGrid}");
                return;
            }
            foreach (var w in Platform.Weapons)
            {
                w.StopReloadSound();
                w.StopRotateSound();
                w.StopShooting(true);
            }
        }

        public void StopAllGraphics()
        {
            foreach (var w in Platform.Weapons)
            {
                foreach (var barrels in w.BarrelAvUpdater)
                {
                    var id = barrels.Key.MuzzleId;
                    if (w.System.BarrelEffect1)
                    {
                        if (w.BarrelEffects1?[id] != null)
                        {
                            w.BarrelEffects1[id].Stop(true);
                            w.BarrelEffects1[id] = null;
                        }
                    }
                    if (w.System.BarrelEffect2)
                    {
                        if (w.BarrelEffects2?[id] != null)
                        {
                            w.BarrelEffects2[id].Stop(true);
                            w.BarrelEffects2[id] = null;
                        }
                    }
                    if (w.HitEffects?[id] != null)
                    {
                        w.HitEffects[id].Stop(true);
                        w.HitEffects[id] = null;
                    }
                }
            }
        }

        public void StopAllAv()
        {
            StopAllSounds();
            StopAllGraphics();
        }
    }
}
