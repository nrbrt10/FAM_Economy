using System.Xml.Serialization;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using VRage.Game.ModAPI;
using Sandbox.ModAPI;
using System.IO;
using VRage.Utils;
using VRage.Game.Components;
using System.Security;
using VRageRender;
using System;
using System.Linq;
using VRage.Voxels.Mesh;

namespace FAM.Economy
{

    public partial class EconomySession : MySessionComponentBase
    {
        public MyModConfig Config;
        private MyModConfig LoadConfig()
        {
            var Filename = "config.xml";
            try
            {
                if (MyAPIGateway.Utilities.FileExistsInModLocation(Filename, ModContext.ModItem))
                {
                    using(var reader = MyAPIGateway.Utilities.ReadFileInModLocation(Filename, ModContext.ModItem))
                    {
                        string xmlContent = reader.ReadToEnd();
                        Config = MyAPIGateway.Utilities.SerializeFromXML<MyModConfig>(xmlContent);
                    }

                    MyLog.Default.WriteLineAndConsole("FAM Economy: Loaded FAM Economy config.");
                    MyLog.Default.WriteLineAndConsole($"FAM Economy: Loaded {Config.VendorConfig.Count} vendors.");
                    MyLog.Default.WriteLineAndConsole($"FAM Economy: Loaded {Config.ProductsConfig.Count} products.");
                    MyLog.Default.WriteLineAndConsole($"FAM Economy: Loaded {Config.BlockConfig.Count} blocks");
                }
                return Config;
            }
            catch
            {
                MyLog.Default.WriteLineAndConsole("FAM Economy: Failed to load config.");
                _failed = true;
                return Config;
            }
        }

        private void ValidateConfig(ref MyModConfig config)
        {
            if (ValidateVendors(ref config.VendorConfig) > 0)
            {
                _failed = true;
            }

        }

        private int ValidateVendors(ref List<VendorDTO> config)
        {
            int errors = 0;
            foreach (VendorDTO vendor in config)
            {
                if (vendor.RestockMultiplier <= 0){vendor.RestockMultiplier = 1;}
                if (vendor.RestockTick < 100){vendor.RestockTick = 100;}
                if (vendor.Products.Count == 0)
                {
                    MyLog.Default.WriteLineAndConsole($"FAM Economy: Vendor {vendor.Id} offers no products");
                }
                if (!(vendor.Tag.Length == 0))
                {
                    vendor.Tag = vendor.Tag?.Length > 3 ? vendor.Tag : vendor.Tag.Substring(0, 3);
                }
                else
                {
                    errors++;
                }
            }
            return errors;
        }

        private void ValidateVendorProcurement(ref List<ResourceDTO> config)
        {
            List<string> types = new List<string>();
            foreach (ResourceDTO entry in config)
            {
                if (entry.Quantity <= 0) continue;   
                entry.active = true;
            }
        }
    }
}