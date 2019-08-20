﻿using System.Collections.Generic;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;
using VRage.Utils;
using VRageMath;
using WeaponCore.Projectiles;
using WeaponCore.Support;

namespace WeaponCore
{
    internal partial class Wheel
    {
        private readonly Vector2D _wheelPosition = new Vector2D(0, 0);

        private string _currentMenu = "Main";
        private int _previousWheel;
        private int _currentWheel;
        internal bool WheelActive;
        internal bool MouseButtonPressed;
        internal bool MouseButtonLeft;
        internal bool MouseButtonMiddle;
        internal bool MouseButtonRight;
        internal bool ResetMenu;
        internal readonly List<GridAi.TargetInfo> Grids = new List<GridAi.TargetInfo>();
        internal readonly List<GridAi.TargetInfo> Characters = new List<GridAi.TargetInfo>();
        internal readonly List<Projectile> Projectiles = new List<Projectile>();
        internal readonly Dictionary<string, Menu> Menus = new Dictionary<string, Menu>();
        internal GridAi Ai;
        internal IMyHudNotification HudNotify;

        internal readonly Item[] GridItems =
        {
            new Item { Texture = MyStringId.GetOrCompute("DS_Empty_Wheel"), Message = "[Grids]", ParentName = "Main", SubName = "SubSystems"},
        };

        internal readonly Item[] CharacterItems =
        {
            new Item { Texture = MyStringId.GetOrCompute("DS_Empty_Wheel"), Message = "[Characters]", ParentName = "Main", SubName = "WeaponGroups"},
        };

        internal readonly Item[] OrdinanceItems =
        {
            new Item { Texture = MyStringId.GetOrCompute("DS_Empty_Wheel"), Message = "[Ordinance]", ParentName = "Main", SubName = "WeaponGroups"},
        };

        internal readonly Item[] SubSystemItems =
        {
           new Item { Texture = MyStringId.GetOrCompute("DS_TargetWheel_Comms"), Message = "[Production]", ParentName = "Grids"},
           new Item { Texture = MyStringId.GetOrCompute("DS_TargetWheel_JumpDrive"), Message = "[Navigation]", ParentName = "Grids"},
           new Item { Texture = MyStringId.GetOrCompute("DS_TargetWheel_Engines"), Message = "[Engines]", ParentName = "Grids"},
           new Item { Texture = MyStringId.GetOrCompute("DS_TargetWheel_Weapons"), Message = "[Weapons]", ParentName = "Grids"},
           new Item { Texture = MyStringId.GetOrCompute("DS_TargetWheel_Power"), Message = "[Power]", ParentName = "Grids"},
           new Item { Texture = MyStringId.GetOrCompute("DS_TargetWheel_Ordinance"), Message = "[Ordinance]", ParentName = "Grids"},
        };

        internal readonly Item[] MainItems =
        {
            //new Item { Texture = MyStringId.GetOrCompute("DS_MainWheel_NoSelect"), Message = "Main" },
            new Item { Texture = MyStringId.GetOrCompute("DS_MainWheel_Grids"), Message = "[Grids]", SubName = "Grids"},
            new Item { Texture = MyStringId.GetOrCompute("DS_MainWheel_Players"), Message = "[Characters]", SubName = "Characters"},
            new Item { Texture = MyStringId.GetOrCompute("DS_MainWheel_WeaponGroups"), Message = "[Weapon Groups]", SubName = "WeaponGroups"},
            new Item { Texture = MyStringId.GetOrCompute("DS_MainWheel_Ordinance"), Message = "[Ordinance]", SubName = "Ordinance"},
        };

        internal readonly Item[] WeaponGroupItems =
        {
            new Item { Texture = MyStringId.GetOrCompute("DS_TargetWheel_Comms"), Message = "[Production]", ParentName = "Main"},
            new Item { Texture = MyStringId.GetOrCompute("DS_TargetWheel_JumpDrive"), Message = "[Navigation]", ParentName = "Main"},
            new Item { Texture = MyStringId.GetOrCompute("DS_TargetWheel_Engines"), Message = "[Engines]", ParentName = "Main"},
            new Item { Texture = MyStringId.GetOrCompute("DS_TargetWheel_Weapons"), Message = "[Weapons]", ParentName = "Main"},
            new Item { Texture = MyStringId.GetOrCompute("DS_TargetWheel_Power"), Message = "[Power]", ParentName = "Main"},
            new Item { Texture = MyStringId.GetOrCompute("DS_TargetWheel_Ordinance"), Message = "[Ordinance]", ParentName = "Main"},
        };

        internal enum State
        {
            Close,
            Open,
            NoChange,
        }

        internal State ChangeState
        {
            get
            {
                var cockPit = Session.Instance.Session.ControlledObject as MyCockpit;
                var isGridAi = cockPit != null && Session.Instance.GridTargetingAIs.TryGetValue(cockPit.CubeGrid, out Ai);
                if (MyAPIGateway.Input.WasMiddleMouseReleased() && !WheelActive && isGridAi) return State.Open;
                if (MyAPIGateway.Input.WasMiddleMouseReleased() && WheelActive) return State.Close;
                return State.NoChange;
            }
        }

        internal Wheel()
        {
            var main = new Menu(this, "Main", MainItems, MainItems.Length);
            var subSystems = new Menu(this, "SubSystems", SubSystemItems, SubSystemItems.Length);
            var grids = new Menu(this, "Grids", GridItems, GridItems.Length);
            var characters = new Menu(this, "Characters", CharacterItems, CharacterItems.Length);
            var ordinance = new Menu(this, "Ordinance", OrdinanceItems, OrdinanceItems.Length);
            var weaponGroups = new Menu(this, "WeaponGroups", WeaponGroupItems, WeaponGroupItems.Length);

            Menus.Add(main.Name, main);
            Menus.Add(subSystems.Name, subSystems);
            Menus.Add(grids.Name, grids);
            Menus.Add(characters.Name, characters);
            Menus.Add(ordinance.Name, ordinance);
            Menus.Add(weaponGroups.Name, weaponGroups);
        }
    }
}
