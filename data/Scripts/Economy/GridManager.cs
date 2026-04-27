using System;
using System.Collections.Generic;
using Sandbox.Definitions;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.Game;
using VRage.Game.Components;
using VRage.Utils;
using BlendTypeEnum = VRageRender.MyBillboard.BlendTypeEnum;
using System.Linq;
using System.Security.Policy;
using System.Net;
using VRage.Library.Net;
using System.Text.RegularExpressions;
using System.Diagnostics;
using VRage.Game.VisualScripting.Utils;
using System.Runtime.InteropServices;
using Sandbox.Game.GameSystems; // required for MyTransparentGeometry/MySimpleObjectDraw to be able to set blend type.

namespace FAM.Economy
{
    public struct TrackedBlock
    {
        public long GridId;
        public string FactionTag;
        public string BlockCategory;
        public IMyTerminalBlock Block;
    }

    public class GridManager
    {
        public Dictionary<long, IMyCubeGrid> TrackedGrids;
        public List<TrackedBlock> ActiveBlocks;
        public Dictionary<long, int> _blockIdToIndex;
        public Dictionary<long, List<int>> _gridToIndices;
        public Dictionary<string, List<int>> _categoryToIndices;
        public Dictionary<string, List<int>> _factionToIndices;
        public Dictionary<string, Dictionary<string, string>> _trackedBlockNames;

        public GridManager()
        {
            
        }


        public void CheckAndAddGrid(IMyCubeGrid grid, Dictionary<string, Dictionary<string, string>> factionBlocks)
        {
            if (grid.BigOwners != null && grid.BigOwners.Count > 0)
            {
                long primaryOwnerId = grid.BigOwners[0];
                IMyFaction faction = MyAPIGateway.Session.Factions.TryGetFactionById(primaryOwnerId);

                if (faction != null && factionBlocks.Keys.ToHashSet().Contains(faction.Tag))
                {
                    this.TrackedGrids.Add(grid.EntityId, grid);
                    
                    var terminalSystem = MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(grid);
                    List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
                    
                    foreach (var blockData in factionBlocks[faction.Tag])
                    {
                        terminalSystem.GetBlocksOfType<IMyTerminalBlock>(blocks, null);
                    }
                    
                }
            }
        }

        public void RemoveGrid(long entityId)
        {
            List<int> blockIndices;
            if (this._gridToIndices.TryGetValue(entityId, out blockIndices))
            {
                if (blockIndices.Count != 0)
                {
                    foreach (int index in blockIndices)
                    {
                        TrackedBlock block = ActiveBlocks[index];
                        if (block.GridId != entityId) continue;
                        this.RemoveBlock(block, index);
                    }
                }
                IMyCubeGrid grid = TrackedGrids[entityId];
                this.TrackedGrids.Remove(entityId);
            }
            
        }

        public void RemoveBlock(TrackedBlock block, int index)
        {
            _categoryToIndices[block.BlockCategory].Remove(index);
            _factionToIndices[block.FactionTag].Remove(index);
            _gridToIndices[block.GridId].Remove(index);
            _blockIdToIndex.Remove(block.Block.EntityId);
            ActiveBlocks.RemoveAt(index);
        }

        public void AddBlock(IMyTerminalBlock block)
        {
            IMyFaction faction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(block.OwnerId);
            if (faction != null)
            {
                
            }
        }

    

        public void FlushHooks()
        {
            
        }
    }
}