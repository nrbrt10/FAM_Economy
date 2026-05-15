using VRage.Utils;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

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
        public Dictionary<string, List<ResourceDTO>> ProcurementData;

        public VendorState(string id, string displayName, string tag, int restockTick, float restockMultiplier, bool buyBack, Dictionary<string, string> blocks, Dictionary<string, ProductDTO> products, Dictionary<string, List<ResourceDTO>> procurement)
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
    public partial class VendorManager
    {

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
        private Dictionary<string, List<ResourceDTO>> BuildProcurementData(List<ResourceDTO> procurementConfig)
        {
            return procurementConfig
                .Where(item => item.active)
                .GroupBy(item => item.Type)
                .ToDictionary(
                    group => group.Key,
                    group => group.ToList()
                );
        }
        public Dictionary<string, VendorState> Initialize(List<VendorDTO> vendorConfig, List<BlockDTO> blocksConfig, List<ProductDTO> productsConfig)
        {
            Dictionary<string, VendorState> vendors = new Dictionary<string, VendorState>();
            Dictionary<string, ProductDTO> productData = BuildProductData(productsConfig);
            Dictionary<string, string> blockData = BuildBlockData(blocksConfig);
            
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