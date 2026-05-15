using System.Xml.Serialization;
using System.Collections.Generic;

namespace FAM.Economy
{
    public class ResourceDTO
    {
        public string Id;
        public string Type;
        public float Quantity;
        public bool active = false;
    }

    public class VendorDTO
    {
        public string Id;
        public string DisplayName;
        public string Tag;
        public string Category;
        [XmlArrayItem("Item")]
        public List<string> Products = new List<string>();
        [XmlArrayItem("Item")]
        public List<string> Blocks = new List<string>();
        public int RestockTick;
        public float RestockMultiplier;
        public List<ResourceDTO> Procurement = new List<ResourceDTO>();
        public bool BuyBack;
        public bool active = false;
    }

    public class ProductDTO
    {
        public string Id;
        public string Type;
        public int BasePrice;
        public int BufferSize;
        public bool active = false;

    }
    public class BlockDTO
    {
        public string Id;
        public string BlockName;
        public bool active = false;
    }
    public class MyModConfig
    {
        [XmlArrayItem("Item")]
        public List<VendorDTO> VendorConfig = new List<VendorDTO>();
        [XmlArrayItem("Item")]
        public List<ProductDTO> ProductsConfig = new List<ProductDTO>();
        [XmlArrayItem("Item")]
        public List<BlockDTO> BlockConfig = new List<BlockDTO>();
    }
}