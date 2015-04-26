﻿using System.Collections.Generic;

namespace LeattyServer.Scripting
{
    public abstract class ShopScript : NpcScript
    {
        public abstract List<ShopItem> ShopItems { get; }

        public override void Execute()
        {
            
        }

        public override bool IsShop => true;             
    }  

    public class ShopItem
    {
        public int Id { get; }
        public int Price { get; }
        public int ReqItemId { get; }
        public int ReqItemQuantity { get; }
        public int DefaultQuantity { get; }
        public int MaximumPurchase { get; }
        public short BulletCount { get; }
        public int Tab { get; }

        public ShopItem(int id, int price, int reqItemId, int reqItemQuantity, int defaultQuantity, int maximumPurchase, short bulletCount, int tab)
        {
            Id = id;
            Price = price;
            ReqItemId = reqItemId;
            ReqItemQuantity = reqItemQuantity;
            DefaultQuantity = defaultQuantity;
            MaximumPurchase = maximumPurchase;
            BulletCount = bulletCount;
            Tab = tab;
        }     
    }
}
