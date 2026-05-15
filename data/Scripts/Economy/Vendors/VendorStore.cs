using System.Collections.Generic;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;
using VRage.Game;
using VRage.Game.ObjectBuilders.Definitions;
using System.Runtime.InteropServices;
using System;

namespace FAM.Economy
{
    public struct Transaction
    {
        public int Tick;
        public string ProductId;
        public int SoldQty;
        public Transaction(int tick, string productId, int soldQty)
        {
            Tick = tick;
            ProductId = productId;
            SoldQty = soldQty;
        }
    }
    public struct StoreOffer
    {
        public IMyStoreItem Item;
        public Action<int,int,long,long,long> TransactionHandler;
        public Action CancelHandler;
        public StoreOffer(IMyStoreItem item, Action<int,int,long,long,long> txnHandler,  Action cancelHandler)
        {
            Item = item;
            TransactionHandler = txnHandler;
            CancelHandler = cancelHandler;
        }
    }
    public class VendorStore
    {
        public Func<int> _getTick;
        public Sandbox.ModAPI.IMyStoreBlock _storeBlock;
        public Dictionary<string, StoreOffer> _offers;
        public Dictionary<string, Queue<Transaction>> _productTransactions;

        public VendorStore(Func<int> getTick, Sandbox.ModAPI.IMyStoreBlock storeBlock)
        {
            _getTick = getTick;
            _storeBlock = storeBlock;
            _offers = new Dictionary<string, StoreOffer>();
            _productTransactions = new Dictionary<string, Queue<Transaction>>();
        }

        public void UpsertOffer(string productId, MyDefinitionId itemDef, int quantity, int price)
        {
            StoreOffer existingStoreOffer;
            if (this._offers.TryGetValue(productId, out existingStoreOffer))
            {
                existingStoreOffer.Item.OnTransaction -= existingStoreOffer.TransactionHandler;
                this._storeBlock.RemoveStoreItem(existingStoreOffer.Item);
                this._offers.Remove(productId);
            }

            IMyStoreItem item = _storeBlock.CreateStoreItem(itemDef, quantity, price, StoreItemTypes.Offer);

            Action<int,int,long,long,long> transactionHandler = (amountSold, amountRemaining, itemPrice, owner, buyer) => OnTransaction(productId, amountSold);
            Action cancelHandler = () => OnCancel(productId);
            item.OnTransaction += transactionHandler;
            item.OnCancel += cancelHandler;
            _storeBlock.InsertStoreItem(item);
            _offers.Add(productId, new StoreOffer(item, transactionHandler, cancelHandler));
            
            if (!_productTransactions.ContainsKey(productId))
            {
                _productTransactions.Add(productId, new Queue<Transaction>());
            }
        }

        public void OnTransaction(string productId, int amountSold)
        {
            _productTransactions[productId].Enqueue(new Transaction(_getTick(), productId, amountSold));
        }

        public void OnCancel(string productId)
        {
            StoreOffer offer;
            if (_offers.TryGetValue(productId, out offer))
            {
                offer.Item.OnTransaction -= offer.TransactionHandler;
                offer.Item.OnCancel -= offer.CancelHandler;
                _offers.Remove(productId);
            }
        }

        public void FlushOffers()
        {
            foreach (var storeOffer in _offers.Values)
            {
                storeOffer.Item.OnTransaction -= storeOffer.TransactionHandler;
                storeOffer.Item.OnCancel -= storeOffer.CancelHandler;
                _storeBlock.RemoveStoreItem(storeOffer.Item);
            }
            _offers.Clear();
        }
    }
}