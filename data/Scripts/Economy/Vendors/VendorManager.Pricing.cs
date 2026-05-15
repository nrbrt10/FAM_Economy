using System.Collections.Generic;
using Sandbox.Definitions;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;
using VRage.Game;
using VRage.Utils;
using System.Linq;
using VRage;
using VRage.ObjectBuilders; // required for MyTransparentGeometry/MySimpleObjectDraw to be able to set blend type.

namespace FAM.Economy
{
    public partial class VendorManager
    {
        public Dictionary<string, Dictionary<string, Queue<Transaction>>> _Transactions;
        public void SetPricing()
        {
            foreach (var kvp in this.gridManager._gridBlocks)
            {
                long gridId = kvp.Key;
                var grid = kvp.Value;
                List<IMyTerminalBlock> stores;
                if (grid.TryGetValue("Store", out stores))
                {
                    
                }
            }
        }
    }
}