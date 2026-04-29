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
        public Dictionary<string, VendorState> vendorStates;
        private void LoadConfig()
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

                Dictionary<string, ProductDTO> productData = BuildProductData(Config.ProductsConfig);
                Dictionary<string, string> blockData = BuildBlockData(Config.BlockConfig);
                vendorStates = BuildVendorState(Config.VendorConfig, blockData, productData);
            }
            catch
            {
                MyLog.Default.WriteLineAndConsole("FAM Economy: Failed to load config.");
                _failed = true;
            }
        }

        private void ValidateConfig(ref MyModConfig config)
        {
            ValidateVendors(ref config.VendorConfig);
        }

        private void ValidateVendors(ref List<VendorDTO> config)
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
        }

        private void ValidateVendorProcurement(ref List<ResourceEntry> config)
        {
            List<string> types = new List<string>();
            foreach (ResourceEntry entry in config)
            {
                if (entry.Quantity <= 0) continue;   
                entry.active = true;
            }
        }

        private Dictionary<string, ProductDTO> BuildProductData(List<ProductDTO> productsConfig)
        {
            return productsConfig
                .Where(p => p.active)
                .ToDictionary(p => p.Id, p => p);
        }
        private Dictionary<string, string> BuildBlockData(List<BlockDTO> blocksConfig)
        {
            return blocksConfig
                .Where(b => b.active)
                .ToDictionary(b => b.Id, b => b.BlockName);            
        }

        private Dictionary<string, List<ResourceEntry>> BuildProcurementData(List<ResourceEntry> procurementConfig)
        {
            return procurementConfig
                .Where(item => item.active)
                .GroupBy(item => item.Type)
                .ToDictionary(group => group.Key, group => group.ToList());
        }
        private Dictionary<string, VendorState> BuildVendorState(List<VendorDTO> vendorConfig, Dictionary<string, string> blockData, Dictionary<string, ProductDTO> productData)
        {
            Dictionary<string, VendorState> vendors = new Dictionary<string, VendorState>();
            
            foreach (VendorDTO vendor in vendorConfig)
            {
                if (!vendor.active) continue;

                Dictionary<string, string> vendorBlocks = new Dictionary<string, string>();
                Dictionary<string, ProductDTO> vendorProducts = new Dictionary<string, ProductDTO>();

                foreach (var block in vendor.Blocks)
                {
                    string blockString;
                    if (blockData.TryGetValue(block.ToString(), out blockString))
                    {
                        vendorBlocks.Add(block.ToString(), Regex.Replace(blockString, "%TAG%", vendor.Tag));
                    }
                    else
                    {
                        MyLog.Default.WriteLineAndConsole($"FAM Economy: vendor {vendor} block {block} not found in BlockConfig.");
                    }
                }

                foreach (var product in vendor.Products)
                {   
                    ProductDTO p;
                    if (productData.TryGetValue(product.ToString(), out p))
                    {
                        vendorProducts.Add(p.Id, p);
                    }
                    else
                    {
                        MyLog.Default.WriteLineAndConsole($"FAM Economy: vendor {vendor} product {product} not found in ProductConfig.");
                    }
                }

                var procurementData = BuildProcurementData(vendor.Procurement);
                vendors.Add(vendor.Tag, new VendorState(
                                                        vendor.Id,
                                                        vendor.DisplayName,
                                                        vendor.Tag,
                                                        vendor.RestockTick,
                                                        vendor.RestockMultiplier,
                                                        vendor.BuyBack,
                                                        blockData,
                                                        productData,
                                                        procurementData
                                                        ));
            }

            return vendors;
        }
    }
}