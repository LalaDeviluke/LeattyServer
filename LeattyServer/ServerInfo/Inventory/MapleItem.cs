﻿using System;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using LeattyServer.Helpers;
using LeattyServer.ServerInfo.Player;
using LeattyServer.Constants;
using LeattyServer.Data;
using LeattyServer.DB.Models;
using LeattyServer.ServerInfo.Packets;

namespace LeattyServer.ServerInfo.Inventory
{
    public class MapleItem
    {
        public long DbId { get; set; }
        public int ItemId { get; set; }
        public short Position { get; set; }
        public MapleItemFlags Flags { get; set; }
        public short Quantity { get; set; }
        public string Creator { get; set; }
        public string Source { get; set; }

        public MapleItem(int itemId, string source, short quantity = 1, string creator = "", MapleItemFlags flags = 0, short position = 0, long dbId = -1)
        {
            ItemId = itemId;
            Position = position;
            Quantity = quantity;
            Creator = creator;
            Flags = flags;
            Source = source;
            DbId = dbId;
        }

        public MapleItem(MapleItem item, string source)
        {
            ItemId = item.ItemId;
            Position = item.Position;
            Flags = item.Flags;
            Quantity = item.Quantity;
            Creator = item.Creator;
            DbId = -1;
            Source = source;
        }

        /// <param name="owner">if equal to null, deletes the item from the database</param>
        public virtual void SaveToDatabase(MapleCharacter owner)
        {
            using (LeattyContext dbContext = new LeattyContext())
            {
                if (owner == null)
                {
                    if (InventoryType == MapleInventoryType.Equip)
                    {
                        InventoryEquip equipEntry = dbContext.InventoryEquips.FirstOrDefault(x => x.Id == DbId);
                        if (equipEntry != null)
                            dbContext.InventoryEquips.Remove(equipEntry);
                    }
                    InventoryItem entry = dbContext.InventoryItems.FirstOrDefault(x => x.Id == DbId);
                    if (entry != null)
                        dbContext.InventoryItems.Remove(entry);
                    DbId = -1;
                }
                else
                {
                    InventoryItem dbActionItem = null;
                    if (DbId != -1)
                        dbActionItem = dbContext.InventoryItems.FirstOrDefault(x => x.Id == DbId);
                    if (dbActionItem == null)
                    {
                        dbActionItem = new InventoryItem();
                        dbContext.InventoryItems.Add(dbActionItem);
                        dbContext.SaveChanges();
                        DbId = dbActionItem.Id;
                    }
                    dbActionItem.CharacterId = owner.Id;
                    dbActionItem.ItemId = ItemId;
                    dbActionItem.Position = Position;
                    dbActionItem.Quantity = Quantity;
                    dbActionItem.Source = Source;
                    dbActionItem.Creator = Creator;
                    dbActionItem.Flags = (short)Flags;
                    dbContext.Entry(dbActionItem).State = System.Data.Entity.EntityState.Modified;
                }
                dbContext.SaveChanges();
            }
        }

        public bool CanStackWith(MapleItem otherItem)
        {
            return ItemId == otherItem.ItemId && Creator == otherItem.Creator;
        }

        #region Packets
        public static PacketWriter ShowItemGain(MapleItem item)
        {
            PacketWriter pw = new PacketWriter();
            pw.WriteHeader(SendHeader.ShowStatusInfo);
            pw.WriteShort(0);
            pw.WriteInt(item.ItemId);
            pw.WriteInt(item.Quantity);

            return pw;
        }

        public static void AddItemPosition(PacketWriter pw, MapleItem item)
        {
            short position = item.Position;
            if (position < 0)
            {
                position = Math.Abs(position);
                if (position > 100 && position < 1000)
                    position -= 100;
            }
            if (item.Type == 1) //equip
                pw.WriteShort(position);
            else
                pw.WriteByte((byte)position);
        }

        public bool CheckAndRemoveFlag(MapleItemFlags flag)
        {
            if (!Flags.HasFlag(flag)) return false;
            Flags &= ~flag;
            return true;
        }

        public static void AddItemInfo(PacketWriter pw, MapleItem item)
        {
            pw.WriteByte(item.Type); //TODO: pets = 3
            pw.WriteInt(item.ItemId);
            pw.WriteByte(0); //TODO:UniqueID,
            //pw.WriteLong(uniqueId) if uniqueId = 1;   
            pw.WriteLong(MapleFormatHelper.GetMapleTimeStamp(-1)); //TODO: item expiration
            pw.WriteInt(-1); //TODO: extended slots
            if (item.Type == 1) //Equip
            {
                MapleEquip equip = (MapleEquip)item;
                MapleEquip.AddStats(equip, pw);

                pw.WriteInt(4); //no clue
                pw.WriteByte(0xFF); //no clue
                
                pw.WriteMapleString(equip.Creator);
                pw.WriteByte((byte)equip.PotentialState);
                pw.WriteByte(equip.Enhancements);
                if (equip.PotentialState >= MaplePotentialState.Rare)
                {
                    pw.WriteUShort(equip.Potential1);
                    pw.WriteUShort(equip.Potential2);
                    pw.WriteUShort(equip.Potential3);
                }
                else
                {
                    pw.WriteZeroBytes(6); //Don't show the client the potentials if they're hidden
                }
                pw.WriteUShort(equip.BonusPotential1);
                pw.WriteUShort(equip.BonusPotential2);
                pw.WriteShort(0); //bonus pot 3
                pw.WriteShort(0); 
                pw.WriteShort(0); //socket state
                pw.WriteShort(equip.Socket1);
                pw.WriteShort(equip.Socket2);
                pw.WriteShort(equip.Socket3);
                
                //if (!HasUniqueId)
                pw.WriteLong(equip.DbId);

                pw.WriteLong(MapleFormatHelper.GetMapleTimeStamp(-2)); //don't know

                pw.WriteInt(-1);
                pw.WriteLong(0); //new v142
                pw.WriteLong(MapleFormatHelper.GetMapleTimeStamp(-2)); //new v142
                for (int i = 0; i < 5; i++)
                    pw.WriteInt(0);
                pw.WriteShort(0x1);
            }
            else
            {
                pw.WriteShort(item.Quantity);
                pw.WriteMapleString(item.Creator);
                pw.WriteShort((short)item.Flags);
                if (item.IsAmmo || item.IsFamiliarCard)
                    pw.WriteLong(-1);
            }
        }
        #endregion

        #region Constant & properties
        public virtual MapleInventoryType InventoryType => ItemConstants.GetInventoryType(ItemId);

        public bool Tradeable
        {
            get
            {
                MapleItemFlags noTradeFlags = Flags & (MapleItemFlags.Untradeable | MapleItemFlags.Lock);
                if (noTradeFlags > 0)
                    return false;
                var info = Type == 1 ? DataBuffer.GetEquipById(ItemId) : DataBuffer.GetItemById(ItemId);
                if (info == null || !info.Tradeable)
                    return false;
                return true;
            }
        }

        public virtual byte Type
        {
            get
            {
                if (InventoryType == MapleInventoryType.Equip)
                    return 1;
                else
                    return 2;
                //TODO: pet = 3
            }
        }

        public int ItemIdBase => ItemId / 10000;

        public bool IsAmmo => IsThrowingStar || IsBullet;
        public bool IsWeapon => ItemConstants.IsWeapon(ItemId);

        public bool IsBowArrow => ItemId >= 2060000 && ItemId < 2061000;
        public bool IsCrossbowArrow => ItemId >= 2061000 && ItemId < 2062000;
        public bool IsThrowingStar => ItemIdBase == 207;
        public bool IsSummonSack => ItemIdBase == 210;
        public bool IsBullet => ItemIdBase == 233;
        public bool IsMonsterCard => ItemIdBase == 238;

        public MapleItemType ItemType => ItemConstants.GetMapleItemType(ItemId);

        public bool IsFamiliarCard => ItemIdBase == 287;
        #endregion
    }

    [Flags]
    public enum MapleItemFlags : short
    {
        None = 0x0,
        Lock = 0x1,
        NoSlip = 0x2,
        ColdResistance = 0x4,
        Untradeable = 0x8,
        Karma = 0x10,
        Charm = 0x20,
        AndroidActivated = 0x40,
        Crafted = 0x80,
        CurseProtection = 0x100,
        LuckyDay = 0x200,
        KarmaAccUse = 0x400,
        KarmaAcc = 0x1000,
        UpgradeCountProtection = 0x2000, // Protects upgrade count
        ScrollProtection = 0x4000,

        KarmaUse = 0x2,
    }
}
