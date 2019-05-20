﻿using System.Collections;
using System.Collections.Generic;
using Sandbox.Game.Entities;
using VRage.Utils;
using VRageMath;
using WeaponCore.Support;
using static WeaponCore.Support.WeaponDefinition.AmmoType;
using static WeaponCore.Support.WeaponDefinition.ShieldType;
using static WeaponCore.Support.WeaponDefinition.EffectType;
using static WeaponCore.Support.WeaponDefinition.GuidanceType;
namespace WeaponCore
{
    class ConfigMe
    {
        class TurretDefinition2 : IEnumerable
        {
            public TurretDefinition2(string name)
            {
            }

            public IEnumerator GetEnumerator()
            {
                return null;
            }

            public void Add(string name, string something, string somethingElse)
            {

            }
        }

        class BarrelDefinition2 : IEnumerable
        {
            public BarrelDefinition2(string name)
            {
            }

            public IEnumerator GetEnumerator()
            {
                return null;
            }

            public void Add(string name)
            {

            }
        }

        public void Test()
        {
            var bg1 = new BarrelDefinition2("BarrelGroup1");
            bg1.Add("test1");
            bg1.Add("test2");

            var turret = new TurretDefinition2("TurretType1");
            turret.Add("TurretSubPart1", "LargeGatling", "BarrelGroup1");
            turret.Add("TurretSubPart1", "LargeGatling", "BarrelGroup1");
        }


        internal Dictionary<string, TurretDefinition> TurretDefinitions = new Dictionary<string, TurretDefinition>()
        {
            ["TurretType1"] = new TurretDefinition() { TurretMap = new Dictionary<string, TurretParts>()
                {
                    ["TurretSubPart1"] = new TurretParts("LargeGatling", "BarrelGroup1"),
                    ["TurretSubPart2"] = new TurretParts("LargeGatling", "BarrelGroup2")
                },
            },
            ["TurretType2"] = new TurretDefinition() { TurretMap = new Dictionary<string, TurretParts>()
                {
                    ["TurretSubPart1"] = new TurretParts("LargeGatling", "BarrelGroup1"),
                },
            },
            ["PDCTurretLB"] = new TurretDefinition() { TurretMap = new Dictionary<string, TurretParts>()
                {
                    ["Boomsticks"] = new TurretParts("LargeGatling", "BarrelGroup2"),
                    ["MissileTurretBarrels"] = new TurretParts("LargeGatling", "BarrelGroup3")
                },
            }
        };

        internal Dictionary<string, BarrelGroup> BarrelDefinitions = new Dictionary<string, BarrelGroup>()
        {
            ["BarrelGroup1"] = new BarrelGroup() { Barrels = new List<string>()
                {
                    "muzzle_barrel_001",
                }
            },
            ["BarrelGroup2"] = new BarrelGroup() { Barrels = new List<string>()
                {
                    "muzzle_barrel_001",
                    "muzzle_barrel_002",
                    "muzzle_barrel_003",
                    "muzzle_barrel_004",
                    "muzzle_barrel_005",
                    "muzzle_barrel_006",
                }
            },
            ["BarrelGroup3"] = new BarrelGroup() { Barrels = new List<string>()
                {
                    "muzzle_missile_001",
                    "muzzle_missile_002",
                    "muzzle_missile_003",
                    "muzzle_missile_004",
                    "muzzle_missile_005",
                    "muzzle_missile_006",
                }
            },
        };

        internal Dictionary<string, WeaponDefinition> WeaponDefinitions = new Dictionary<string, WeaponDefinition>() {
            //Weapon2SubTyeId is the second SubtypeId in your block.sbc
            ["LargeGatling"] = new WeaponDefinition() {
                // Turret properties
                TurretMode = true,
                TrackTarget = true,
                RotateBarrelAxis = 3, // 0 = off, 1 = xAxis, 2 = yAxis, 3 = zAxis
                RateOfFire = 3600,
                BarrelsPerShot = 1,
                SkipBarrels = 0,
                ShotsPerBarrel = 1,
                HeatPerRoF = 1,
                MaxHeat = 180,
                HeatSinkRate = 2,
                MuzzleFlashLifeSpan = 0,
                RotateSpeed = 1f,
                FiringSound = new MySoundPair("cueName"),

                // Ammo Mag properties
                ReloadTime = 10,
                Ammo = Bolt,
                ReleaseTimeAfterFire = 10f,
                ReloadSound = new MySoundPair("cueName"),

                //Ammo Properties
                Guidance = Smart,
                DefaultDamage = 150f, 
                InitalSpeed = 10f,
                AccelPerSec = 10f,
                DesiredSpeed = 60f,
                MaxTrajectory = 1000f,
                ShotLength = 120f,
                ShotWidth = 0.1f,
                DeviateShotAngle = 2f,
                BackkickForce = 2.5f,
                SpeedVariance = 5f,
                RangeMultiplier = 2.1f,
                AreaEffectYield = 0f,
                AreaEffectRadius = 0f,
                RealisticDamage = false,
                // If set to realistic DefaultDamage is disabled the 
                // and following values are used, damage equation is: 
                // ((Mass / 2) * (Velocity * Velocity) / 1000) * KeenScaler
                KeenScaler = 0.0125f,
                Mass = 150f,  // in grams
                ThermalDamage = 0, // MegaWatts
                Health = 0f,

                // Ammo Visual Audio properties
                Trail = true,
                UseRandomizedRange = false,
                PhysicalMaterial = MyStringId.GetOrCompute("ProjectileTrailLine"),
                TrailColor = new Vector4(255, 10, 0, 110f),
                ParticleColor = new Vector4(255, 0, 0, 175),
                Effect = Lance,
                ModelName = MyStringId.GetOrCompute("Custom"),
                AmmoSound = new MySoundPair("cueName"),

                //Shield Behavior
                ShieldHitDraw = true,
                ShieldDmgMultiplier = 1.1f,
                ShieldDamage = Kinetic,
            },
        };

        internal void Init()
        {
            foreach (var def in TurretDefinitions)
                Session.Instance.WeaponStructure.Add(MyStringHash.GetOrCompute(def.Key), new WeaponStructure(def, WeaponDefinitions, BarrelDefinitions));
        }
    }
}
