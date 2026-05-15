using System.Collections.Generic;
using Sandbox.Definitions;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;
using VRage.Game;
using VRage.Utils;
using VRage;
using VRage.ObjectBuilders;

namespace FAM.Economy
{
    public partial class VendorManager
    {
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

                    foreach (ResourceDTO resource in resources)
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