using System;
using System.Collections.Generic;
using VRage.Game;
using Sandbox.Common.ObjectBuilders.Definitions;
using Sandbox.ModAPI;
using SpaceEngineers.Game.ModAPI;


namespace FAM.Economy
{
    public static class ObjectBuilderRegistry
    {
        public static readonly Dictionary<string, Type> _objectCategoryMap = new Dictionary<string, Type>
        {
            {"Ingot", typeof(MyObjectBuilder_Ingot)},
            {"Ore", typeof(MyObjectBuilder_Ore)},
            {"Component", typeof(MyObjectBuilder_Component)},
            {"Consumable", typeof(MyObjectBuilder_ConsumableItem)},
            {"Tools/Ammo", typeof(MyObjectBuilder_PhysicalGunObject)},
            {"OxygenContainerObject", typeof(MyObjectBuilder_OxygenContainerObject)},
            {"GasContainerObject", typeof(MyObjectBuilder_GasContainerObject)},
        };
    }

    public static class BlockRegistry
    {
        public static readonly Dictionary<string, Type> _blockTypeMap = new Dictionary<string, Type>
        {
            { "Storage",  typeof(IMyCargoContainer) },
            { "Store",    typeof(IMyStoreBlock) },
            { "Reactor",  typeof(IMyReactor) },
            { "Assembler",typeof(IMyAssembler) },
            { "H2/O2",    typeof(IMyGasGenerator) },
            { "Safezone", typeof(IMySafeZoneBlock) },
            { "Refinery", typeof(IMyRefinery) },
        };

        public static bool IsExpectedBlockType(IMyTerminalBlock block, string category)
        {
            switch (category)
            {
                case "Storage":  return block is IMyCargoContainer;
                case "Store":    return block is IMyStoreBlock;
                case "Reactor":  return block is IMyReactor;
                case "Assembler":return block is IMyAssembler;
                case "H2/O2":    return block is IMyGasGenerator;
                case "Safezone": return block is IMySafeZoneBlock;
                case "Refinery": return block is IMyRefinery;
                default:         return false;
            }
        }
    }
}