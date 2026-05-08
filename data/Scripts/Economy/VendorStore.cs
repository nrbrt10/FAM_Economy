using System.Collections.Generic;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.ModAPI.Ingame;

namespace FAM.Economy
{
    public class VendorStore
    {
        public IMyStoreBlock _storeBlock;
        public Dictionary<string, long> _offerIds;

        public VendorStore(IMyStoreBlock storeBlock)
        {
            _storeBlock = storeBlock;
            _offerIds = new Dictionary<string, long>();
        }

        public void UpsertOffer(string productId, MyDefinitionId itemDef, int quantity, int price)
        {
            long existingId;
            if (this._offerIds.TryGetValue(productId, out existingId))
            {
                this._storeBlock.CancelStoreItem(existingId);
                this._offerIds.Remove(productId);
            }

            var item = new MyStoreItemDataSimple(itemDef, quantity, price);
            long newId;
            _storeBlock.InsertOffer(item, out newId);
            _offerIds.Add(productId, newId);
        }

        public void GetSales(Dictionary<string, int> lastKnownStock)
        {
            
        }
    }
}