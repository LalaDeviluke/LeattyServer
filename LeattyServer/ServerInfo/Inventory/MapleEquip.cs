﻿using System;
using System.Collections.Generic;
using System.Linq;
using LeattyServer.Data.WZ;
using LeattyServer.Helpers;
using LeattyServer.ServerInfo.Player;
using System.Data.Entity;
using LeattyServer.Data;
using LeattyServer.DB.Models;
using LeattyServer.ServerInfo.Packets;

namespace LeattyServer.ServerInfo.Inventory
{
    public class MapleEquip : MapleItem
    {
        public byte RemainingUpgradeCount { get; set; }
        public byte UpgradeCount { get; set; }
        public short Str { get; set; }
        public short Int { get; set; }
        public short Dex { get; set; }
        public short Luk { get; set; }
        public short IncMhp { get; set; }
        public short IncMmp { get; set; }
        public short Pad { get; set; }
        public short Mad { get; set; }
        public short Pdd { get; set; }
        public short Mdd { get; set; }
        public short Acc { get; set; }
        public short Eva { get; set; }
        public short Speed { get; set; }
        public short Jump { get; set; }
        public short Diligence { get; set; }
        public MaplePotentialState PotentialState { get; set; }
        public int Durability { get; set; }
        public byte HammersApplied { get; set; }
        public byte Enhancements { get; set; }
        public ushort Potential1 { get; set; }
        public ushort Potential2 { get; set; }
        public ushort Potential3 { get; set; }
        public ushort BonusPotential1 { get; set; }
        public ushort BonusPotential2 { get; set; }
        public short Socket1 { get; set; }
        public short Socket2 { get; set; }
        public short Socket3 { get; set; }
        public byte CustomLevel { get; set; }
        public short CustomExp { get; set; }

        public override byte Type => 1;
        public override MapleInventoryType InventoryType => MapleInventoryType.Equip;

        public MapleEquip(int itemId, string source, string creator = "", MapleItemFlags flags = 0, short position = 0, long dbId = -1)
            : base(itemId, source, 1, creator, flags, position, dbId)
        {
            RemainingUpgradeCount = 0;
            UpgradeCount = 0;
            Str = 0;
            Int = 0;
            Dex = 0;
            Luk = 0;
            IncMhp = 0;
            IncMmp = 0;
            Pad = 0;
            Mad = 0;
            Pdd = 0;
            Mdd = 0;
            Acc = 0;
            Eva = 0;
            Speed = 0;
            Jump = 0;
            Diligence = 0;
            PotentialState = 0;
            Durability = -1;
            HammersApplied = 0;
            Enhancements = 0;
            Potential1 = 0;
            Potential2 = 0;
            Potential3 = 0;
            BonusPotential1 = 0;
            BonusPotential2 = 0;
            Socket1 = -1;
            Socket2 = -1;
            Socket3 = -1;
            CustomLevel = 0;
            CustomExp = 0;
        }

        public override void SaveToDatabase(MapleCharacter owner)
        {
            base.SaveToDatabase(owner);
            if (owner == null) return; //delete is handled in base function
            InventoryEquip dbActionEquip;
            using (LeattyContext dbContext = new LeattyContext())
            {
                dbActionEquip = dbContext.InventoryEquips.FirstOrDefault(x => x.InventoryItemId == DbId);
                if (dbActionEquip == null)
                {
                    dbActionEquip = new InventoryEquip { InventoryItemId = DbId };
                    dbContext.InventoryEquips.Add(dbActionEquip);
                    dbContext.SaveChanges();
                }
            }

            dbActionEquip.RemainingUpgradeCount = RemainingUpgradeCount;
            dbActionEquip.UpgradeCount = UpgradeCount;
            dbActionEquip.Str = Str;
            dbActionEquip.Dex = Dex;
            dbActionEquip.Int = Int;
            dbActionEquip.IncMhp = IncMhp;
            dbActionEquip.IncMmp = IncMmp;
            dbActionEquip.Pad = Pad;
            dbActionEquip.Mad = Mad;
            dbActionEquip.Pdd = Pdd;
            dbActionEquip.Mdd = Mdd;
            dbActionEquip.Acc = Acc;
            dbActionEquip.Eva = Eva;
            dbActionEquip.Speed = Speed;
            dbActionEquip.Jump = Jump;
            dbActionEquip.Durability = Durability;
            dbActionEquip.HammerApplied = HammersApplied;
            dbActionEquip.Enhancements = Enhancements;
            dbActionEquip.Diligence = Diligence;
            dbActionEquip.Potential1 = (short)Potential1;
            dbActionEquip.Potential2 = (short)Potential2;
            dbActionEquip.Potential3 = (short)Potential3;
            dbActionEquip.BonusPotential1 = (short)BonusPotential1;
            dbActionEquip.BonusPotential2 = (short)BonusPotential2;        
            dbActionEquip.Socket1 = Socket1;
            dbActionEquip.Socket2 = Socket2;
            dbActionEquip.Socket3 = Socket3;
            dbActionEquip.CustomLevel = CustomLevel;
            dbActionEquip.CustomExp = CustomExp;
            dbActionEquip.PotentialState = (byte)PotentialState;
            using (LeattyContext dbContext = new LeattyContext())
            {
                dbContext.Entry(dbActionEquip).State = EntityState.Modified;
                dbContext.SaveChanges();
            }
        }

        public void SetDefaultStats(WzEquip info, bool randomizeStats = false, bool addRandomPotential = false)
        {
            UpgradeCount = 0;
            Enhancements = 0;
            Diligence = 0;
            CustomLevel = 0;
            CustomExp = 0;
            HammersApplied = 0;
            if (info != null)
            {
                RemainingUpgradeCount = info.TotalUpgradeCount;
                if (randomizeStats)
                {
                    Str = GetRandomizedStat(info.IncStr);
                    Int = GetRandomizedStat(info.IncInt);
                    Dex = GetRandomizedStat(info.IncDex);
                    Luk = GetRandomizedStat(info.IncLuk);
                    IncMhp = GetRandomizedStat(info.IncMhp);
                    IncMmp = GetRandomizedStat(info.IncMmp);
                    Pad = GetRandomizedStat(info.IncPad);
                    Mad = GetRandomizedStat(info.IncMad);
                    Pdd = GetRandomizedStat(info.IncPdd);
                    Mdd = GetRandomizedStat(info.IncMdd);
                    Acc = GetRandomizedStat(info.IncAcc);
                    Eva = GetRandomizedStat(info.IncEva);
                    Speed = GetRandomizedStat(info.IncSpeed);
                    Jump = GetRandomizedStat(info.IncJump);
                }
                else
                {
                    Str = info.IncStr;
                    Int = info.IncInt;
                    Dex = info.IncDex;
                    Luk = info.IncLuk;
                    IncMhp = info.IncMhp;
                    IncMmp = info.IncMmp;
                    Pad = info.IncPad;
                    Mad = info.IncMad;
                    Pdd = info.IncPdd;
                    Mdd = info.IncMdd;
                    Acc = info.IncAcc;
                    Eva = info.IncEva;
                    Speed = info.IncSpeed;
                    Jump = info.IncJump;
                }
                if (addRandomPotential) //todo: check if this equip can have potential
                {
                    MapleEquipEnhancer.AddRandomPotential(this, equipInfo: info);
                }
            }
            else
                ServerConsole.Error("Equip id {0} not in Data Buffer!", ItemId);
        }

        private short GetRandomizedStat(short defaultStat, double deviationPercent = 10)
        {
            if (defaultStat == 0)
                return 0;
            //Randomizes a stat with given ceiled deviation percent, default is 10
            //E.g. with defaultstat 50 and deviationpercent 10, stat can be 45-55.
            int dev = (int)Math.Ceiling(defaultStat * (deviationPercent / 100));
            return (short)Math.Max(0, (defaultStat - dev) + Functions.Random(0, dev * 2));
        }

        #region Packets
        public static void AddStats(MapleEquip equip, PacketWriter pw)
        {
            Dictionary<MapleEquipStat, short> statValuePairs = new Dictionary<MapleEquipStat, short>();

            statValuePairs.Add(MapleEquipStat.RemainingUpgrades, equip.RemainingUpgradeCount);

            if (equip.UpgradeCount > 0) statValuePairs.Add(MapleEquipStat.Upgrades, equip.UpgradeCount);
            if (equip.Str > 0) statValuePairs.Add(MapleEquipStat.Str, equip.Str);
            if (equip.Dex > 0) statValuePairs.Add(MapleEquipStat.Dex, equip.Dex);
            if (equip.Int > 0) statValuePairs.Add(MapleEquipStat.Int, equip.Int);
            if (equip.Luk > 0) statValuePairs.Add(MapleEquipStat.Luk, equip.Luk);
            if (equip.IncMhp > 0) statValuePairs.Add(MapleEquipStat.IncMhp, equip.IncMhp);
            if (equip.IncMmp > 0) statValuePairs.Add(MapleEquipStat.IncMmp, equip.IncMmp);
            if (equip.Pad > 0) statValuePairs.Add(MapleEquipStat.Pad, equip.Pad);
            if (equip.Mad > 0) statValuePairs.Add(MapleEquipStat.Mad, equip.Mad);
            if (equip.Pdd > 0) statValuePairs.Add(MapleEquipStat.Pdd, equip.Pdd);
            if (equip.Mdd > 0) statValuePairs.Add(MapleEquipStat.Mdd, equip.Mdd);
            if (equip.Acc > 0) statValuePairs.Add(MapleEquipStat.Acc, equip.Acc);
            if (equip.Eva > 0) statValuePairs.Add(MapleEquipStat.Eva, equip.Eva);
            if (equip.Diligence > 0) statValuePairs.Add(MapleEquipStat.Diligence, equip.Diligence);
            if (equip.Speed > 0) statValuePairs.Add(MapleEquipStat.Speed, equip.Speed);
            if (equip.Jump > 0) statValuePairs.Add(MapleEquipStat.Jump, equip.Jump);
            if (equip.Flags > 0) statValuePairs.Add(MapleEquipStat.Flag, (short)equip.Flags);

            if (equip.HammersApplied > 0) statValuePairs.Add(MapleEquipStat.Hammer, equip.HammersApplied);

            int mask = 0;
            foreach (KeyValuePair<MapleEquipStat, short> kvp in statValuePairs)
            {
                mask += (int)kvp.Key;
            }
            pw.WriteInt(mask);
            foreach (KeyValuePair<MapleEquipStat, short> kvp in statValuePairs)
            {
                switch (kvp.Key)
                {
                    case MapleEquipStat.Hammer:
                    pw.WriteInt(kvp.Value);
                    break;
                    case MapleEquipStat.RemainingUpgrades:
                    case MapleEquipStat.Upgrades:
                    pw.WriteByte((byte)kvp.Value);
                    break;
                    default:
                    pw.WriteShort(kvp.Value);
                    break;
                }
            }
        }
        #endregion

        [Flags]
        public enum MapleEquipStat : int
        {
            None = 0x0,
            RemainingUpgrades = 0x1, //Byte
            Upgrades =          0x2, //Byte
            Str =               0x4, //Short
            Dex =               0x8, //Short
            Int =               0x10, //Short
            Luk =               0x20, //Short
            IncMhp =            0x40, //Short
            IncMmp =            0x80, //Short
            Pad =               0x100, //Short
            Mad =               0x200, //Short
            Pdd =               0x400, //Short
            Mdd =               0x800, //Short
            Acc =               0x1000, //Short
            Eva =               0x2000, //Short
            Diligence =         0x4000, //Short
            Speed =             0x8000, //Short
            Jump =              0x10000, //Short
            Flag =              0x20000, //short
                              //0x40000, //unk
                              //0x80000, //unk
                              //0x100000, //unk
                              //0x200000, //unk
            Hammer =            0x400000, //int
                              //0x800000, //unk
                              //0x1000000, //unk
            FeverTime =         0x02000000 //short 0x4 as value
        }   
    }

    public enum MaplePotentialState : byte
    {        
        None = 0,
        HiddenRare = 1,
        HiddenEpic = 2,
        HiddenUnique = 3,
        HiddenLegendary = 4,
        Rare = 17,
        Epic = 18,
        Unique = 19,
        Legendary = 20
    }
}


