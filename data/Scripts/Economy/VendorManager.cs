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
using System.Diagnostics; // required for MyTransparentGeometry/MySimpleObjectDraw to be able to set blend type.

namespace FAM.Economy
{
    public struct VendorBlock
    {
        public string Category;
        public IMyTerminalBlock Block;

        public VendorBlock(string category, IMyTerminalBlock block)
        {
            Category = category;
            Block = block;
        }
    }
    public struct VendorGrid
    {
        public string Tag;
        public IMyCubeGrid Grid;
        List<VendorBlock> GridBlocks;

        public VendorGrid(string tag, IMyCubeGrid grid, List<VendorBlock> gridBlocks)
        {
            Tag = tag;
            Grid = grid;
            GridBlocks = gridBlocks;
        }
    }
        public class VendorState
    {
        public string Id;
        public string DisplayName;
        public string Tag;
        public string Category;
        public int RestockTick;
        public float RestockMultiplier;
        public bool BuyBack;
        public Dictionary<string, string> BlockData;
        public Dictionary<string, ProductDTO> ProductData;
        public Dictionary<string, List<ResourceEntry>> ProcurementData;

        public VendorState(string id, string displayName, string tag, int restockTick, float restockMultiplier, bool buyBack, Dictionary<string, string> blocks, Dictionary<string, ProductDTO> products, Dictionary<string, List<ResourceEntry>> procurement)
        {
            Id = id;
            DisplayName = displayName;
            Tag = tag;
            RestockTick = restockTick;
            RestockMultiplier = restockMultiplier;
            BuyBack = buyBack;
            BlockData = blocks;
            ProductData = products;
            ProcurementData = procurement;
        }
    }
    public class VendorManager
    {
        public Dictionary<string, VendorState> Vendors;
        
        public Dictionary<long, VendorGrid> VendorGrids = new Dictionary<long, VendorGrid>();
        

        public VendorManager(Dictionary<string, VendorState> vendors)
        {
            Vendors = vendors;
        }
        public void ManageVendors(int tick)
        {
            ManageGrids();
        }
        public void ManageGrids()
        {
            foreach (var kvp in this.VendorGrids)
            {
                
            }
        }
    }
}