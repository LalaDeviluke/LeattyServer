﻿using LeattyServer.Helpers;
using LeattyServer.ServerInfo.Player;
using LeattyServer.ServerInfo.Quest;
using reNX.NXProperties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace LeattyServer.Data.WZ
{
    public class WzQuestRequirement
    {
        public QuestRequirementType Type { get; set; }        

        protected WzQuestRequirement(QuestRequirementType type)
        {
            Type = type;            
        }

        public virtual bool Check(MapleCharacter chr, int npcId, MapleQuest quest) { return false; }
    }

    public class WzQuestStringRequirement : WzQuestRequirement
    {        
        public string Data { get; private set; }

        public WzQuestStringRequirement(QuestRequirementType type,string data)
            : base(type)
        {
            Data = data;
        }

        public override bool Check(MapleCharacter chr, int npcId, MapleQuest quest)
        {
            //TODO: run quest script
            return true;
        }
    }

    public class WzQuestIntegerRequirement : WzQuestRequirement
    {        
        public int Data { get; private set; }

        public WzQuestIntegerRequirement(QuestRequirementType type, int data)
            : base(type)
        {
            Data = data;
        }

        public override bool Check(MapleCharacter chr, int npcId, MapleQuest quest)
        {
            switch (Type)
            {
                case QuestRequirementType.lvmin:
                    return chr.Level >= Data;
                case QuestRequirementType.lvmax:
                    return chr.Level <= Data;
                case QuestRequirementType.npc:
                    return npcId == Data;                   
                case QuestRequirementType.interval:                    
                    return true; //todo: quest is repeatable
                case QuestRequirementType.pettamenessmin:
                    return true; //todo: chr.GetPet(0).Tameness >= Data;
                case QuestRequirementType.questComplete: //amount of quests completed
                    return chr.CompletedQuestCount >= Data;
                case QuestRequirementType.pop: //fame
                    return chr.Fame >= Data;
                case QuestRequirementType.charmMin:
                    return chr.Charisma >= Data;
                case QuestRequirementType.senseMin:
                    return chr.Sense >= Data;
                case QuestRequirementType.craftMin:
                    return chr.Craft >= Data;
                case QuestRequirementType.willMin:
                    return chr.Will >= Data;
                case QuestRequirementType.charismaMin:
                    return chr.Charisma >= Data;
                case QuestRequirementType.insightMin:
                    return chr.Insight >= Data;                    
                default:
                    ServerConsole.Warning("No check handler for " + Type.ToString());
                    break;                    
            }
            return false;
        }
    }

    public class WzQuestIntegerPairRequirement : WzQuestRequirement
    {        
        public Dictionary<int, int> Data { get; private set; }

        public WzQuestIntegerPairRequirement(QuestRequirementType type, Dictionary<int, int> data)
            : base(type)
        {
            Data = data;
        }

        public override bool Check(MapleCharacter chr, int npcId, MapleQuest quest)
        {
            switch (Type)
            {
                case QuestRequirementType.item:
                    foreach (var itemPair in Data)
                    {
                        if (!chr.Inventory.HasItem(itemPair.Key, itemPair.Value)) 
                            return false;
                    }
                    return true;
                case QuestRequirementType.mob:
                    foreach (var mobPair in Data)
                    {
                        if (!quest.MonsterKills.ContainsKey(mobPair.Key) || quest.MonsterKills[mobPair.Key] < mobPair.Value) 
                            return false;
                    }
                    return true;
                case QuestRequirementType.quest:
                    foreach (var questPair in Data)
                    {
                        if (!chr.HasCompletedQuest(questPair.Key)) 
                            return false;
                    }
                    return true;
                default:
                    ServerConsole.Warning("No check handler for " + Type.ToString());
                    break;                    
            }
            return false;
        }
    }

    public class WzQuestIntegerListRequirement : WzQuestRequirement
    {       
        public List<int> Data { get; private set; }

        public WzQuestIntegerListRequirement(QuestRequirementType type, List<int> data)
            : base(type)
        {
            Data = data;
        }

        public override bool Check(MapleCharacter chr, int npcId, MapleQuest quest)
        {
            switch (Type)
            {
                case QuestRequirementType.job:
                    return Data.Where(i => i == chr.Job).Any();                  
                case QuestRequirementType.fieldEnter:
                    return Data.Where(i => i == chr.MapId).Any();
                case QuestRequirementType.pet:
                    return true; //todo: 
                    /*
                     return Data.Where(x => x == chr.GetPet(0).ItemId).Any();
                     */
                default:
                    ServerConsole.Warning("No check handler for " + Type.ToString());
                    break;                    
            }
            return false;
        }
    }

    public class WzQuestDateRequirement : WzQuestRequirement
    {        
        public DateTime Date { get; private set; }

        public WzQuestDateRequirement(QuestRequirementType type, DateTime date)
            : base(type)
        {
            Date = date;
        }

        public override bool Check(MapleCharacter chr, int npcId, MapleQuest quest)
        {
            switch (Type)
            {
                case QuestRequirementType.end:
                    return DateTime.UtcNow <= Date;
                default:
                    ServerConsole.Warning("No check handler for " + Type.ToString());
                    break;                    
            }
            return false;
        }
    }
    
    public enum QuestRequirementType
    {
        undefined,
        job,
        item,
        quest,
        lvmin,
        lvmax,
        end,
        mob,
        npc,
        fieldEnter,
        interval,
        startscript,
        endscript,
        pet,
        pettamenessmin,       
        questComplete,
        pop,
        subJobFlags,
        dayByDay,
        normalAutoStart,
        partyQuest_S,
        charmMin,
        senseMin,
        craftMin,
        willMin,
        charismaMin,
        insightMin
    }
}
