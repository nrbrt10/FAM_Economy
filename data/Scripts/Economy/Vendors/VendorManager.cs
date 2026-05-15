using System.Collections.Generic;

namespace FAM.Economy
{
    public partial class VendorManager
    {
        public Dictionary<string, VendorState> Vendors;
        public GridManager gridManager;
        private int _tick;
        
        public VendorManager(List<VendorDTO> vendorConfig, List<BlockDTO> blocksConfig, List<ProductDTO> productsConfig)
        {
            _tick = 0; 
            Vendors = Initialize(vendorConfig, blocksConfig, productsConfig);
        }
        
        public void ManageVendors()
        {
            SupplyVendors(_tick);
        }
    }
}