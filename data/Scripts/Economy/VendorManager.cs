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
using Sandbox.Common.ObjectBuilders.Definitions;
using VRage;
using VRage.ObjectBuilders; // required for MyTransparentGeometry/MySimpleObjectDraw to be able to set blend type.

namespace FAM.Economy
{
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
    public  class VendorManager
    {
        public Dictionary<string, VendorState> Vendors;
        public GridManager gridManager;
        
        public VendorManager(Dictionary<string, VendorState> vendors)
        {
            Vendors = vendors;
            gridManager = new GridManager(Vendors.ToDictionary(v => v.Key, v => v.Value.BlockData));
        }
        public void ManageVendors(int tick)
        {
            SupplyVendors(tick);
        }

        public void SupplyVendors(int tick)
        {
            foreach (var kvp in this.gridManager._trackedGrids)
            {
                long entityId = kvp.Key;
                IMyCubeGrid grid = kvp.Value;

                if (grid.Closed) continue;

                string factionTag = this.gridManager._gridToFaction[entityId];
                if (tick % this.Vendors[factionTag].RestockTick != 0) continue;
                List<IMyTerminalBlock> cargoContainers = this.gridManager._gridBlocks[entityId]["Storage"];
                if (cargoContainers.Count == 0) continue;

                foreach (var typeResourcePair in this.Vendors[factionTag].ProcurementData)
                {
                    string type = typeResourcePair.Key;
                    var resources = typeResourcePair.Value;

                    foreach (ResourceEntry resource in resources)
                    {
                        var typeBuilder = ObjectBuilderRegistry._objectCategoryMap[type];
                        MyDefinitionId defId = new MyDefinitionId(typeBuilder, resource.Id);
                        float volumePerUnit = 0f;
                        MyPhysicalItemDefinition itemDef;
                        if (MyDefinitionManager.Static.TryGetPhysicalItemDefinition(defId, out itemDef))
                        {
                            volumePerUnit = itemDef.Volume;
                        }

                        this.DepositItems(cargoContainers, defId, (MyFixedPoint)resource.Quantity, (MyFixedPoint)volumePerUnit);
                    }
                }
            }
        }

        public void DepositItems(List<IMyTerminalBlock> cargoContainers, MyDefinitionId defId, MyFixedPoint amount, MyFixedPoint volumePerUnit)
        {
            MyObjectBuilder_PhysicalObject builder = MyObjectBuilderSerializer.CreateNewObject(defId) as MyObjectBuilder_PhysicalObject;
            if (builder == null)
            {
                MyLog.Default.WriteLine($"FAM Economy: Failed to create builder for {defId}.");
                return;
            }

            MyFixedPoint remaining = amount;

            foreach (var container in cargoContainers)
            {
                IMyInventory inventory = container.GetInventory();
                MyFixedPoint availableVolume = inventory.MaxVolume - inventory.CurrentVolume;
                MyFixedPoint unitsThatFit = (MyFixedPoint)((float)availableVolume / (float)volumePerUnit);
                MyFixedPoint toDeposit = MyFixedPoint.Min(remaining, unitsThatFit);
                if (toDeposit > 0)
                {
                    inventory.AddItems(toDeposit, builder);
                    remaining -= toDeposit;
                }
            }
            
        }
    }
}