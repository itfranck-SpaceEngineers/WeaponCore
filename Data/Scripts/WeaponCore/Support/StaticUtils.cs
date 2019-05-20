﻿using VRage.Serialization;

namespace WeaponCore.Support
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Sandbox.Game;
    using Sandbox.Game.Entities;
    using Sandbox.ModAPI;
    using VRage.Game;
    using VRage.Game.Entity;
    using VRage.Game.ModAPI;
    using VRageMath;
    using Color = VRageMath.Color;
    using Quaternion = VRageMath.Quaternion;
    using Vector3 = VRageMath.Vector3;

    internal static class UtilsStatic
    {
        public static void PrepConfigFile()
        {
            const int Version = 70;

            var dsCfgExists = MyAPIGateway.Utilities.FileExistsInGlobalStorage("WeaponCore.cfg");
            if (dsCfgExists)
            {
                var unPackCfg = MyAPIGateway.Utilities.ReadFileInGlobalStorage("WeaponCore.cfg");
                var unPackedData = MyAPIGateway.Utilities.SerializeFromXML<WeaponEnforcement>(unPackCfg.ReadToEnd());
                /*
                var invalidValue = unPackedData.HpsEfficiency <= 0 || unPackedData.BaseScaler < 1 || unPackedData.MaintenanceCost <= 0;
                if (invalidValue)
                {
                    if (unPackedData.HpsEfficiency <= 0) unPackedData.HpsEfficiency = HpsEfficiency;
                    if (unPackedData.BaseScaler < 1) unPackedData.BaseScaler = BaseScaler;
                    if (unPackedData.MaintenanceCost <= 0) unPackedData.MaintenanceCost = MaintenanceCost;
                }
                if (unPackedData.Version == Version && !invalidValue) return;

                if (!invalidValue) Log.Line($"outdated config file regenerating, file version: {unPackedData.Version} - current version: {Version}");
                else Log.Line("Invalid config file, fixing");
                */

                /*
                Session.Enforced.BaseScaler = !unPackedData.BaseScaler.Equals(-1) ? unPackedData.BaseScaler : BaseScaler;
                Session.Enforced.HeatScaler = !unPackedData.HeatScaler.Equals(-1f) ? unPackedData.HeatScaler : HeatScaler;
                Session.Enforced.Unused = !unPackedData.Unused.Equals(-1f) ? unPackedData.Unused : Unused;
                Session.Enforced.StationRatio = !unPackedData.StationRatio.Equals(-1) ? unPackedData.StationRatio : StationRatio;
                Session.Enforced.LargeShipRatio = !unPackedData.LargeShipRatio.Equals(-1) ? unPackedData.LargeShipRatio : LargeShipRate;
                Session.Enforced.SmallShipRatio = !unPackedData.SmallShipRatio.Equals(-1) ? unPackedData.SmallShipRatio : SmallShipRatio;
                Session.Enforced.DisableVoxelSupport = !unPackedData.DisableVoxelSupport.Equals(-1) ? unPackedData.DisableVoxelSupport : DisableVoxel;
                Session.Enforced.DisableEntityBarrier = !unPackedData.DisableEntityBarrier.Equals(-1) ? unPackedData.DisableEntityBarrier : DisableEntityBarrier;
                Session.Enforced.Debug = !unPackedData.Debug.Equals(-1) ? unPackedData.Debug : Debug;
                Session.Enforced.SuperWeapons = !unPackedData.SuperWeapons.Equals(-1) ? unPackedData.SuperWeapons : SuperWeapons;
                Session.Enforced.CapScaler = !unPackedData.CapScaler.Equals(-1f) ? unPackedData.CapScaler : CapScaler;
                Session.Enforced.HpsEfficiency = !unPackedData.HpsEfficiency.Equals(-1f) ? unPackedData.HpsEfficiency : HpsEfficiency;
                Session.Enforced.MaintenanceCost = !unPackedData.MaintenanceCost.Equals(-1f) ? unPackedData.MaintenanceCost : MaintenanceCost;
                Session.Enforced.DisableBlockDamage = !unPackedData.DisableBlockDamage.Equals(-1) ? unPackedData.DisableBlockDamage : DisableBlockDamage;
                Session.Enforced.DisableLineOfSight = !unPackedData.DisableLineOfSight.Equals(-1) ? unPackedData.DisableLineOfSight : DisableLineOfSight;
                if (unPackedData.Version <= 69)
                {
                    Session.Enforced.CapScaler = 0.5f;
                    Session.Enforced.HpsEfficiency = 0.5f;
                    Session.Enforced.HeatScaler = 0.0065f;
                    Session.Enforced.BaseScaler = 10;
                }			
                */
               Session.Enforced.Version = Version;
                UpdateConfigFile(unPackCfg);
            }
            else
            {
                Session.Enforced.Debug = 1;
                Session.Enforced.SenderId = 0;
                Session.Enforced.Version = 1;
                WriteNewConfigFile();

                Log.Line($"wrote new config file - file exists: {MyAPIGateway.Utilities.FileExistsInGlobalStorage("WeaponCore.cfg")}");
            }
        }

        public static void ReadConfigFile()
        {
            var dsCfgExists = MyAPIGateway.Utilities.FileExistsInGlobalStorage("WeaponCore.cfg");

            if (Session.Enforced.Debug == 3) Log.Line($"Reading config, file exists? {dsCfgExists}");

            if (!dsCfgExists) return;

            var cfg = MyAPIGateway.Utilities.ReadFileInGlobalStorage("WeaponCore.cfg");
            var data = MyAPIGateway.Utilities.SerializeFromXML<WeaponEnforcement>(cfg.ReadToEnd());
            Session.Enforced = data;

            if (Session.Enforced.Debug == 3) Log.Line($"Writing settings to mod:\n{data}");
        }

        public static void FibonacciSeq(int magicNum)
        {
            var root5 = Math.Sqrt(5);
            var phi = (1 + root5) / 2;

            var n = 0;
            int Fn;
            do
            {
                Fn = (int)((Math.Pow(phi, n) - Math.Pow(-phi, -n)) / ((2 * phi) - 1));
                //Console.Write("{0} ", Fn);
                ++n;
            }
            while (Fn < magicNum);
        }

        public static void SphereCloud(int pointLimit, Vector3D[] physicsArray, MyEntity shieldEnt, bool transformAndScale, bool debug, Random rnd = null)
        {
            if (pointLimit > 10000) pointLimit = 10000;
            if (rnd == null) rnd = new Random(0);

            var sPosComp = shieldEnt.PositionComp;
            var unscaledPosWorldMatrix = MatrixD.Rescale(MatrixD.CreateTranslation(sPosComp.WorldAABB.Center), sPosComp.WorldVolume.Radius);
            var radius = sPosComp.WorldVolume.Radius;
            for (int i = 0; i < pointLimit; i++)
            {
                var value = rnd.Next(0, physicsArray.Length - 1);
                var phi = 2 * Math.PI * i / pointLimit;
                var x = (float)(radius * Math.Sin(phi) * Math.Cos(value));
                var z = (float)(radius * Math.Sin(phi) * Math.Sin(value));
                var y = (float)(radius * Math.Cos(phi));
                var v = new Vector3D(x, y, z);

                if (transformAndScale) v = Vector3D.Transform(Vector3D.Normalize(v), unscaledPosWorldMatrix);
                if (debug) DsDebugDraw.DrawX(v, sPosComp.LocalMatrix, 0.5);
                physicsArray[i] = v;
            }
        }

        public static void UnitSphereCloudQuick(int pointLimit, ref Vector3D[] physicsArray, MyEntity shieldEnt, bool translateAndScale, bool debug, Random rnd = null)
        {
            if (pointLimit > 10000) pointLimit = 10000;
            if (rnd == null) rnd = new Random(0);

            var sPosComp = shieldEnt.PositionComp;
            var radius = sPosComp.WorldVolume.Radius;
            var center = sPosComp.WorldAABB.Center;
            var v = Vector3D.Zero;

            for (int i = 0; i < pointLimit; i++)
            {
                while (true)
                {
                    v.X = (rnd.NextDouble() * 2) - 1;
                    v.Y = (rnd.NextDouble() * 2) - 1;
                    v.Z = (rnd.NextDouble() * 2) - 1;
                    var len2 = v.LengthSquared();
                    if (len2 < .0001) continue;
                    v *= radius / Math.Sqrt(len2);
                    break;
                }

                if (translateAndScale) physicsArray[i] = v += center;
                else physicsArray[i] = v;
                if (debug) DsDebugDraw.DrawX(v, sPosComp.LocalMatrix, 0.5);
            }
        }

        public static void UnitSphereRandomOnly(ref Vector3D[] physicsArray, Random rnd = null)
        {
            if (rnd == null) rnd = new Random(0);
            var v = Vector3D.Zero;

            for (int i = 0; i < physicsArray.Length; i++)
            {
                v.X = 0;
                v.Y = 0;
                v.Z = 0;
                while ((v.X * v.X) + (v.Y * v.Y) + (v.Z * v.Z) < 0.0001)
                {
                    v.X = (rnd.NextDouble() * 2) - 1;
                    v.Y = (rnd.NextDouble() * 2) - 1;
                    v.Z = (rnd.NextDouble() * 2) - 1;
                }
                v.Normalize();
                physicsArray[i] = v;
            }
        }

        public static void UnitSphereTranslateScale(int pointLimit, ref Vector3D[] physicsArray, ref Vector3D[] scaledCloudArray, MyEntity shieldEnt, bool debug)
        {
            var sPosComp = shieldEnt.PositionComp;
            var radius = sPosComp.WorldVolume.Radius;
            var center = sPosComp.WorldAABB.Center;

            for (int i = 0; i < pointLimit; i++)
            {
                var v = physicsArray[i];
                scaledCloudArray[i] = v = center + (radius * v);
                if (debug) DsDebugDraw.DrawX(v, sPosComp.LocalMatrix, 0.5);
            }
        }

        public static void UnitSphereTranslateScaleList(int pointLimit, ref Vector3D[] physicsArray, ref List<Vector3D> scaledCloudList, MyEntity shieldEnt, bool debug, MyEntity grid, bool rotate = true)
        {
            var sPosComp = shieldEnt.PositionComp;
            var radius = sPosComp.WorldVolume.Radius;
            var center = sPosComp.WorldAABB.Center;
            var gMatrix = grid.WorldMatrix;
            for (int i = 0; i < pointLimit; i++)
            {
                var v = physicsArray[i];
                if (rotate) Vector3D.Rotate(ref v, ref gMatrix, out v);
                v = center + (radius * v);
                scaledCloudList.Add(v);
                if (debug) DsDebugDraw.DrawX(v, sPosComp.LocalMatrix, 0.5);
            }
        }

        public static void DetermisticSphereCloud(List<Vector3D> physicsArray, int pointsInSextant)
        {
            physicsArray.Clear();
            int stepsPerCoord = (int)Math.Sqrt(pointsInSextant);
            double radPerStep = MathHelperD.PiOver2 / stepsPerCoord;

            for (double az = -MathHelperD.PiOver4; az < MathHelperD.PiOver4; az += radPerStep)
            {
                for (double el = -MathHelperD.PiOver4; el < MathHelperD.PiOver4; el += radPerStep)
                {
                    Vector3D vec;
                    Vector3D.CreateFromAzimuthAndElevation(az, el, out vec);
                    Vector3D vec2 = new Vector3D(vec.Z, vec.X, vec.Y);
                    Vector3D vec3 = new Vector3D(vec.Y, vec.Z, vec.X);
                    physicsArray.Add(vec); //first sextant
                    physicsArray.Add(vec2); //2nd sextant
                    physicsArray.Add(vec3); //3rd sextant
                    physicsArray.Add(-vec); //4th sextant
                    physicsArray.Add(-vec2); //5th sextant
                    physicsArray.Add(-vec3); //6th sextant
                }
            }
        }

        public static Vector3D? GetLineIntersectionExactAll(MyCubeGrid grid, ref LineD line, out double distance, out IMySlimBlock intersectedBlock)
        {
            intersectedBlock = (IMySlimBlock)null;
            distance = 3.40282346638529E+38;
            Vector3I? nullable = new Vector3I?();
            Vector3I zero = Vector3I.Zero;
            double distanceSquared = double.MaxValue;
            if (grid.GetLineIntersectionExactGrid(ref line, ref zero, ref distanceSquared))
            {
                distanceSquared = Math.Sqrt(distanceSquared);
                nullable = new Vector3I?(zero);
            }
            if (!nullable.HasValue)
                return new Vector3D?();
            distance = distanceSquared;
            intersectedBlock = grid.GetCubeBlock(nullable.Value);
            if (intersectedBlock == null)
                return new Vector3D?();
            return new Vector3D?((Vector3D)zero);
        }

        public static void UpdateTerminal(this MyCubeBlock block)
        {
            MyOwnershipShareModeEnum shareMode;
            long ownerId;
            if (block.IDModule != null)
            {
                ownerId = block.IDModule.Owner;
                shareMode = block.IDModule.ShareMode;
            }
            else
            {
                return;
            }
            block.ChangeOwner(ownerId, shareMode == MyOwnershipShareModeEnum.None ? MyOwnershipShareModeEnum.Faction : MyOwnershipShareModeEnum.None);
            block.ChangeOwner(ownerId, shareMode);
        }

        public static long IntPower(int x, short power)
        {
            if (power == 0) return 1;
            if (power == 1) return x;
            int n = 15;
            while ((power <<= 1) >= 0) n--;

            long tmp = x;
            while (--n > 0)
                tmp = tmp * tmp *
                      (((power <<= 1) < 0) ? x : 1);
            return tmp;
        }

        public static double InverseSqrDist(Vector3D source, Vector3D target, double range)
        {
            var rangeSq = range * range;
            var distSq = (target - source).LengthSquared();
            if (distSq > rangeSq)
                return 0.0;
            return 1.0 - (distSq / rangeSq);
        }

        public static double GetIntersectingSurfaceArea(MatrixD matrix, Vector3D hitPosLocal)
        {
            var surfaceArea = -1d; 

            var boxMax = matrix.Backward + matrix.Right + matrix.Up;
            var boxMin = -boxMax;
            var box = new BoundingBoxD(boxMin, boxMax);

            var maxWidth = box.Max.LengthSquared();
            var testLine = new LineD(Vector3D.Zero, Vector3D.Normalize(hitPosLocal) * maxWidth); 
            LineD testIntersection;
            box.Intersect(ref testLine, out testIntersection);

            var intersection = testIntersection.To;

            var epsilon = 1e-6; 
            var projFront = VectorProjection(intersection, matrix.Forward);
            if (Math.Abs(projFront.LengthSquared() - matrix.Forward.LengthSquared()) < epsilon)
            {
                var a = Vector3D.Distance(matrix.Left, matrix.Right);
                var b = Vector3D.Distance(matrix.Up, matrix.Down);
                surfaceArea = a * b;
            }

            var projLeft = VectorProjection(intersection, matrix.Left);
            if (Math.Abs(projLeft.LengthSquared() - matrix.Left.LengthSquared()) < epsilon) 
            {
                var a = Vector3D.Distance(matrix.Forward, matrix.Backward);
                var b = Vector3D.Distance(matrix.Up, matrix.Down);
                surfaceArea = a * b;
            }

            var projUp = VectorProjection(intersection, matrix.Up);
            if (Math.Abs(projUp.LengthSquared() - matrix.Up.LengthSquared()) < epsilon) 
            {
                var a = Vector3D.Distance(matrix.Forward, matrix.Backward);
                var b = Vector3D.Distance(matrix.Left, matrix.Right);
                surfaceArea = a * b;
            }
            return surfaceArea;
        }

        public static bool DistanceCheck(IMyCubeBlock block, int x, double range)
        {
            if (MyAPIGateway.Session.Player.Character == null) return false;

            var pPosition = MyAPIGateway.Session.Player.Character.PositionComp.WorldVolume.Center;
            var cPosition = block.CubeGrid.PositionComp.WorldVolume.Center;
            var dist = Vector3D.DistanceSquared(cPosition, pPosition) <= (x + range) * (x + range);
            return dist;
        }

        public static int BlockCount(IMyCubeBlock shield)
        {
            var subGrids = MyAPIGateway.GridGroups.GetGroup(shield.CubeGrid, GridLinkTypeEnum.Mechanical);
            var blockCnt = 0;
            foreach (var grid in subGrids)
            {
                blockCnt += ((MyCubeGrid)grid).BlocksCount;
            }
            return blockCnt;
        }

        public static void CreateExplosion(Vector3D position, float radius, float damage = 5000)
        {
            MyExplosionTypeEnum explosionTypeEnum = MyExplosionTypeEnum.WARHEAD_EXPLOSION_50;
            if (radius < 2.0)
                explosionTypeEnum = MyExplosionTypeEnum.WARHEAD_EXPLOSION_02;
            else if (radius < 15.0)
                explosionTypeEnum = MyExplosionTypeEnum.WARHEAD_EXPLOSION_15;
            else if (radius < 30.0)
                explosionTypeEnum = MyExplosionTypeEnum.WARHEAD_EXPLOSION_30;
            MyExplosionInfo explosionInfo = new MyExplosionInfo()
            {
                PlayerDamage = 0.0f,
                Damage = damage,
                ExplosionType = explosionTypeEnum,
                ExplosionSphere = new BoundingSphereD(position, radius),
                LifespanMiliseconds = 700,
                ParticleScale = 1f,
                Direction = Vector3.Down,
                VoxelExplosionCenter = position,
                ExplosionFlags = MyExplosionFlags.CREATE_DEBRIS | MyExplosionFlags.AFFECT_VOXELS | MyExplosionFlags.APPLY_FORCE_AND_DAMAGE | MyExplosionFlags.CREATE_DECALS | MyExplosionFlags.CREATE_PARTICLE_EFFECT | MyExplosionFlags.CREATE_SHRAPNELS | MyExplosionFlags.APPLY_DEFORMATION,
                VoxelCutoutScale = 1f,
                PlaySound = true,
                ApplyForceAndDamage = true,
                ObjectsRemoveDelayInMiliseconds = 40
            };
            MyExplosions.AddExplosion(ref explosionInfo);
        }

        public static void CreateFakeSmallExplosion(Vector3D position)
        {
            MyExplosionInfo explosionInfo = new MyExplosionInfo()
            {
                PlayerDamage = 0.0f,
                Damage = 0f,
                ExplosionType = MyExplosionTypeEnum.MISSILE_EXPLOSION,
                ExplosionSphere = new BoundingSphereD(position, 0d),
                LifespanMiliseconds = 0,
                ParticleScale = 1f,
                Direction = Vector3.Down,
                VoxelExplosionCenter = position,
                ExplosionFlags = MyExplosionFlags.CREATE_PARTICLE_EFFECT,
                VoxelCutoutScale = 0f,
                PlaySound = true,
                ApplyForceAndDamage = false,
                ObjectsRemoveDelayInMiliseconds = 0
            };
            MyExplosions.AddExplosion(ref explosionInfo);
        }

        private static void UpdateConfigFile(TextReader unPackCfg)
        {
            unPackCfg.Close();
            unPackCfg.Dispose();
            MyAPIGateway.Utilities.DeleteFileInGlobalStorage("WeaponCore.cfg");
            var newCfg = MyAPIGateway.Utilities.WriteFileInGlobalStorage("WeaponCore.cfg");
            var newData = MyAPIGateway.Utilities.SerializeToXML(Session.Enforced);
            newCfg.Write(newData);
            newCfg.Flush();
            newCfg.Close();
            Log.Line($"wrote modified config file - file exists: {MyAPIGateway.Utilities.FileExistsInGlobalStorage("WeaponCore.cfg")}");
        }

        private static void WriteNewConfigFile()
        {
            var cfg = MyAPIGateway.Utilities.WriteFileInGlobalStorage("WeaponCore.cfg");
            var data = MyAPIGateway.Utilities.SerializeToXML(Session.Enforced);
            cfg.Write(data);
            cfg.Flush();
            cfg.Close();
        }

        private static Vector3D VectorProjection(Vector3D a, Vector3D b)
        {
            if (Vector3D.IsZero(b))
                return Vector3D.Zero;

            return a.Dot(b) / b.LengthSquared() * b;
        }

        /*
        private static double PowerCalculation(IMyEntity breaching, IMyCubeGrid grid)
        {
            var bPhysics = breaching.Physics;
            var sPhysics = grid.Physics;

            const double wattsPerNewton = (3.36e6 / 288000);
            var velTarget = sPhysics.GetVelocityAtPoint(breaching.Physics.CenterOfMassWorld);
            var accelLinear = sPhysics.LinearAcceleration;
            var velTargetNext = velTarget + accelLinear * MyEngineConstants.PHYSICS_STEP_SIZE_IN_SECONDS;
            var velModifyNext = bPhysics.LinearVelocity;
            var linearImpulse = bPhysics.Mass * (velTargetNext - velModifyNext);
            var powerCorrectionInJoules = wattsPerNewton * linearImpulse.Length();

            return powerCorrectionInJoules * MyEngineConstants.PHYSICS_STEP_SIZE_IN_SECONDS;
        }
        */
    }
}
