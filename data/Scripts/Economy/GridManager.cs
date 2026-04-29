using System;
using System.Collections.Generic;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;
using System.Linq;
using SpaceEngineers.Game.ModAPI;


namespace FAM.Economy
{

    public class GridManager
    {
        private static readonly Dictionary<string, Type> _blockTypeMap = new Dictionary<string, Type>
        {
            { "Storage",  typeof(IMyCargoContainer) },
            { "Store",    typeof(IMyStoreBlock) },
            { "Reactor",  typeof(IMyReactor) },
            { "Assembler",typeof(IMyAssembler) },
            { "H2/O2",    typeof(IMyGasGenerator) },
            { "Safezone", typeof(IMySafeZoneBlock) },
            { "Refinery", typeof(IMyRefinery) },
        };

        private static bool IsExpectedBlockType(IMyTerminalBlock block, string category)
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
        public Dictionary<long, IMyCubeGrid> _trackedGrids;
        public Dictionary<long, Dictionary<string, List<IMyTerminalBlock>>> _gridBlocks;
        public Dictionary<string, List<long>> _factionToGrids;
        public Dictionary<long, string> _gridToFaction;
        public Dictionary<string, Dictionary<string, string>> _factionBlocks;
        public GridManager(Dictionary<string, Dictionary<string, string>> factionBlocks)
        {
            _factionBlocks = factionBlocks;
            _factionToGrids = factionBlocks.Keys.ToDictionary(f => f, f => new List<long>());
            _trackedGrids = new Dictionary<long, IMyCubeGrid>();
            _gridBlocks = new Dictionary<long, Dictionary<string, List<IMyTerminalBlock>>>();
            _gridToFaction = new Dictionary<long, string>();
        }

        public void CheckAndAddGrid(IMyCubeGrid grid)
        {
            if (grid.BigOwners != null && grid.BigOwners.Count > 0)
            {
                long primaryOwnerId = grid.BigOwners[0];
                IMyFaction faction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(primaryOwnerId);

                if (faction != null && _factionBlocks.ContainsKey(faction.Tag))
                {
                    this._trackedGrids.Add(grid.EntityId, grid);
                    this._factionToGrids[faction.Tag].Add(grid.EntityId);
                    this._gridBlocks.Add(grid.EntityId, _blockTypeMap.Keys.ToDictionary(c => c, c => new List<IMyTerminalBlock>()));
                    this._gridToFaction.Add(grid.EntityId, faction.Tag);
                    
                    var terminalSystem = MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(grid);
                    
                    List<IMyTerminalBlock> allBlocks = new List<IMyTerminalBlock>();
                    terminalSystem.GetBlocksOfType<IMyTerminalBlock>(allBlocks, null);
                    
                    foreach (var block in allBlocks)
                    {
                        if (!block.CustomName.Contains($"[FAM Econ]")) continue;
                        foreach (var kvp in _blockTypeMap)
                        {
                            string expectedName;
                            if (_factionBlocks[faction.Tag].TryGetValue(kvp.Key, out expectedName) && IsExpectedBlockType(block, kvp.Key) && block.CustomName == expectedName)
                            {
                                CheckAndAddBlock(grid.EntityId, kvp.Key, block);
                                break;
                            }
                        }

                    }
                    allBlocks.Clear();
                }
            }
        }

        public void RemoveGrid(long entityId)
        {
            string factionTag;
            if (_gridToFaction.TryGetValue(entityId, out factionTag))
            {
                this._factionToGrids[factionTag].Remove(entityId);
            }
            this._gridBlocks.Remove(entityId);
            this._trackedGrids.Remove(entityId);
            this._gridToFaction.Remove(entityId);
        }
        public void CheckAndAddBlock(long gridId, string category, IMyTerminalBlock block)
        {
            if (this._gridBlocks.ContainsKey(gridId) && this._gridBlocks[gridId].ContainsKey(category))
            {
                this._gridBlocks[gridId][category].Add(block);
            }
        }
    }
}