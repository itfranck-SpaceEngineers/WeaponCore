﻿using System.Collections.Generic;
using VRage.Game;
using VRage.Utils;
using VRageMath;
using WeaponCore.Support;
using BlendTypeEnum = VRageRender.MyBillboard.BlendTypeEnum;
namespace WeaponCore
{
    public partial class Session
    {
        private void DrawLists(List<Trajectile> drawList)
        {
            var sFound = false;
            var gFound = false;
            for (int i = 0; i < drawList.Count; i++)
            {
                var t = drawList[i];
                if (t.PrimeEntity != null)
                {
                    if (t.Draw != Trajectile.DrawState.Last && !t.PrimeEntity.InScene)
                    {
                        t.PrimeEntity.InScene = true;
                        t.PrimeEntity.Render.UpdateRenderObject(true, false);
                    }

                    t.PrimeEntity.PositionComp.SetWorldMatrix(t.PrimeMatrix, null, false, false, false);
                    if (t.Draw == Trajectile.DrawState.Last)
                    {
                        t.PrimeEntity.InScene = false;
                        t.PrimeEntity.Render.RemoveRenderObjects();
                    }
                    if (!t.System.Values.Graphics.Line.Tracer.Enable && t.TriggerEntity == null) continue;
                }

                if (t.Triggered && t.TriggerEntity != null)
                {
                    if ((t.Draw != Trajectile.DrawState.Last && !t.TriggerEntity.InScene))
                    {
                        t.TriggerEntity.InScene = true;
                        t.TriggerEntity.Render.UpdateRenderObject(true, false);
                    }

                    t.TriggerEntity.PositionComp.SetWorldMatrix(t.TriggerMatrix, null, false, false, false);
                    if (t.Draw == Trajectile.DrawState.Last)
                    {
                        t.TriggerEntity.InScene = false;
                        t.TriggerEntity.Render.RemoveRenderObjects();
                    }
                    if (!t.System.Values.Graphics.Line.Tracer.Enable) continue;
                }

                var thickness = t.LineWidth;

                var changeValue = 0.01f;
                if (t.System.IsBeamWeapon && t.BaseDamagePool > t.System.Values.Ammo.BaseDamage)
                {
                    thickness *= t.BaseDamagePool / t.System.Values.Ammo.BaseDamage;
                    changeValue = 0.02f;
                }

                var color = t.Color;
                if (t.System.IsBeamWeapon)
                {
                    if (_lCount < 60)
                    {
                        var adder = (_lCount + 1);
                        var adder2 = adder * changeValue;
                        var adder3 = adder2 + 1;
                        thickness = adder3 * thickness;
                        color *= adder3;
                    }
                    else
                    {
                        var shrinkFrom = ((60) * changeValue) + 1;

                        var adder = (_lCount - 59);
                        var adder2 = adder * changeValue;
                        var scaler = (shrinkFrom - adder2);
                        thickness = scaler * thickness;
                        color *= (shrinkFrom - adder2);
                    }
                }
                else
                {
                    if (t.ReSizing == Trajectile.ReSize.Shrink && t.HitEntity != null)
                    {
                        sFound = true;
                        var shrink = _shrinkPool.Get();
                        shrink.Init(t);
                        _shrinking.Add(shrink);
                    }
                    else if (t.System.Trail && t.ReSizing != Trajectile.ReSize.Grow)
                    {
                        gFound = true;
                        var afterGlow = new AfterGlow
                        {
                            System = t.System,
                            StepLength = (t.DistanceTraveled - t.PrevDistanceTraveled),
                            Direction = -t.Direction,
                            Back = t.Back,
                            FirstTick = Tick,
                        };
                        _afterGlow.Add(afterGlow);
                    }
                }

                var hitPos = t.Back + (t.Direction * t.Length);
                var distanceFromPointSqr = Vector3D.DistanceSquared(CameraPos, (MyUtils.GetClosestPointOnLine(ref t.Back, ref hitPos, ref CameraPos)));
                if (distanceFromPointSqr < 100) thickness *= 0.25f;
                else if (distanceFromPointSqr < 400) thickness *= 0.5f;
                else if (distanceFromPointSqr > 160000) thickness *= 8f;
                else if (distanceFromPointSqr > 40000) thickness *= 4f;
                else if (distanceFromPointSqr > 10000) thickness *= 2f;

                if (t.System.OffsetEffect)
                    LineOffsetEffect(t.System, t.Back, t.Direction, (float)t.DistanceTraveled, t.Length, thickness, color);
                else
                    MyTransparentGeometry.AddLineBillboard(t.System.TracerMaterial, color, t.Back, t.Direction, (float)t.Length, thickness);

                if (t.System.IsBeamWeapon && t.System.HitParticle && !(t.MuzzleId != 0 && (t.System.ConvergeBeams || t.System.OneHitParticle)))
                {
                    var c = t.Target.FiringCube;
                    if (c == null || c.MarkedForClose)
                        continue;

                    WeaponComponent weaponComp;
                    if (t.Ai.WeaponBase.TryGetValue(c, out weaponComp))
                    {
                        var weapon = weaponComp.Platform.Weapons[t.WeaponId];
                        var effect = weapon.HitEffects[t.MuzzleId];
                        if (t.HitEntity?.HitPos != null && t.OnScreen)
                        {
                            if (effect != null)
                            {
                                var elapsedTime = effect.GetElapsedTime();
                                if (elapsedTime <= 0 || elapsedTime >= 1)
                                {
                                    effect.Stop(true);
                                    effect = null;
                                }
                            }
                            var hit = t.HitEntity.HitPos.Value;
                            MatrixD matrix;
                            MatrixD.CreateTranslation(ref hit, out matrix);
                            if (effect == null)
                            {
                                MyParticlesManager.TryCreateParticleEffect(t.System.Values.Graphics.Particles.Hit.Name, ref matrix, ref hit, uint.MaxValue, out effect);
                                if (effect == null)
                                {
                                    weapon.HitEffects[t.MuzzleId] = null;
                                    continue;
                                }

                                effect.DistanceMax = t.System.Values.Graphics.Particles.Hit.Extras.MaxDistance;
                                effect.DurationMax = t.System.Values.Graphics.Particles.Hit.Extras.MaxDuration;
                                effect.UserColorMultiplier = t.System.Values.Graphics.Particles.Hit.Color;
                                const int scaler = 1;
                                effect.Loop = t.System.Values.Graphics.Particles.Hit.Extras.Loop;

                                effect.UserRadiusMultiplier = t.System.Values.Graphics.Particles.Hit.Extras.Scale * scaler;
                                effect.UserEmitterScale = 1 * scaler;
                            }
                            else if (effect.IsEmittingStopped)
                                effect.Play();

                            effect.WorldMatrix = matrix;
                            if (t.HitEntity.Projectile != null) effect.Velocity = t.HitEntity.Projectile.Velocity;
                            else if (t.HitEntity.Entity?.GetTopMostParent()?.Physics != null) effect.Velocity = t.HitEntity.Entity.GetTopMostParent().Physics.LinearVelocity;
                            weapon.HitEffects[t.MuzzleId] = effect;
                        }
                        else if (effect != null)
                        {
                            effect.Stop(false);
                            weapon.HitEffects[t.MuzzleId] = null;
                        }
                    }
                }
            }
            if (sFound) _shrinking.ApplyAdditions();
            if (gFound) _afterGlow.ApplyAdditions();
            drawList.Clear();
        }

        private void Shrink()
        {
            var sRemove = false;
            var gAdd = false;
            foreach (var s in _shrinking)
            {
                var shrunk = s.GetLine();
                if (shrunk.HasValue)
                {
                    MyTransparentGeometry.AddLineBillboard(s.System.TracerMaterial, s.System.Values.Graphics.Line.Tracer.Color, shrunk.Value.Back, shrunk.Value.Direction, (float)shrunk.Value.Length, s.System.Values.Graphics.Line.Tracer.Width);
                    if (s.System.Trail)
                    {
                        gAdd = true;
                        var afterGlow = new AfterGlow
                        {
                            System = s.System,
                            StepLength = shrunk.Value.StepLength,
                            Direction = shrunk.Value.Direction,
                            Back = (shrunk.Value.Back + (-shrunk.Value.Direction * (shrunk.Value.StepLength * 2))),
                            FirstTick = Tick,
                        };
                        _afterGlow.Add(afterGlow);
                    }
                }
                else
                {
                    _shrinking.Remove(s);
                    sRemove = true;
                }
            }
            if (gAdd) _afterGlow.ApplyAdditions();
            if (sRemove) _shrinking.ApplyRemovals();
        }

        private void AfterGlow()
        {
            var gRemove = false;
            for (int i = 0; i < _afterGlow.Count; i++)
            {
                var a = _afterGlow[i];
                var system = a.System;
                var fullSize = system.Values.Graphics.Line.Tracer.Width;
                var steps = system.Values.Graphics.Line.Trail.DecayTime;
                var thisStep = (Tick - a.FirstTick);
                var shrinkAmount = fullSize / steps;
                var reduction = (shrinkAmount * thisStep);
                var thickness = fullSize - reduction;
                var hitPos = a.Back + (-a.Direction * a.StepLength);
                if (thisStep == 0) DsDebugDraw.DrawSingleVec(a.Back, 0.5f, Color.Red);
                var distanceFromPointSqr = Vector3D.DistanceSquared(CameraPos, (MyUtils.GetClosestPointOnLine(ref a.Back, ref hitPos, ref CameraPos)));
                if (distanceFromPointSqr > 160000) thickness *= 8f;
                else if (distanceFromPointSqr > 40000) thickness *= 4f;
                else if (distanceFromPointSqr > 10000) thickness *= 2f;
                if (thisStep < steps)
                {
                    MyTransparentGeometry.AddLineBillboard(system.TrailMaterial, system.Values.Graphics.Line.Trail.Color, a.Back, a.Direction, (float) a.StepLength, thickness);
                }
                else
                {
                    //_afterGlow.Remove(a);
                    gRemove = true;
                }
            }
            if (gRemove) _afterGlow.ApplyRemovals();
        }

        internal void LineOffsetEffect(WeaponSystem system, Vector3D startPos, Vector3D direction, float distanceTraveled, double tracerLength, float beamRadius, Vector4 color)
        {
            MatrixD matrix;
            var up = MatrixD.Identity.Up;
            MatrixD.CreateWorld(ref startPos, ref direction, ref up, out matrix);
            var offsetMaterial = system.TracerMaterial;
            var tracerLengthSqr = tracerLength * tracerLength;
            var maxOffset = system.Values.Graphics.Line.OffsetEffect.MaxOffset;
            var minLength = system.Values.Graphics.Line.OffsetEffect.MinLength;
            var maxLength = system.Values.Graphics.Line.OffsetEffect.MaxLength;

            double currentForwardDistance = 0;

            while (currentForwardDistance < tracerLength)
            {
                currentForwardDistance += MyUtils.GetRandomDouble(minLength, maxLength);
                var lateralXDistance = MyUtils.GetRandomDouble(maxOffset * -1, maxOffset);
                var lateralYDistance = MyUtils.GetRandomDouble(maxOffset * -1, maxOffset);
                _offsetList.Add(new Vector3D(lateralXDistance, lateralYDistance, currentForwardDistance * -1));
            }

            for (int i = 0; i < _offsetList.Count; i++)
            {
                Vector3D fromBeam;
                Vector3D toBeam;

                if (i == 0)
                {
                    fromBeam = matrix.Translation;
                    toBeam = Vector3D.Transform(_offsetList[i], matrix);
                }
                else
                {
                    fromBeam = Vector3D.Transform(_offsetList[i - 1], matrix);
                    toBeam = Vector3D.Transform(_offsetList[i], matrix);
                }

                Vector3 dir = (toBeam - fromBeam);
                var length = dir.Length();
                var normDir = dir / length;
                MyTransparentGeometry.AddLineBillboard(offsetMaterial, color, fromBeam, normDir, length, beamRadius);

                if (Vector3D.DistanceSquared(matrix.Translation, toBeam) > tracerLengthSqr) break;
            }
            _offsetList.Clear();
        }
    }
}
