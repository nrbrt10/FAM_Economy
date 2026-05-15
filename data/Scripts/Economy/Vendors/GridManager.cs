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
                    this._gridBlocks.Add(grid.EntityId, BlockRegistry._blockTypeMap.Keys.ToDictionary(c => c, c => new List<IMyTerminalBlock>()));
                    this._gridToFaction.Add(grid.EntityId, faction.Tag);
                    
                    var terminalSystem = MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(grid);
                    
                    List<IMyTerminalBlock> allBlocks = new List<IMyTerminalBlock>();
                    terminalSystem.GetBlocksOfType<IMyTerminalBlock>(allBlocks, null);
                    
                    foreach (var block in allBlocks)
                    {
                        foreach (var kvp in BlockRegistry._blockTypeMap)
                        {
                            string expectedName;
                            if (_factionBlocks[faction.Tag].TryGetValue(kvp.Key, out expectedName) && BlockRegistry.IsExpectedBlockType(block, kvp.Key) && block.CustomName == expectedName)
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