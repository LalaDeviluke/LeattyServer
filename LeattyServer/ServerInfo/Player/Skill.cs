﻿using System.Collections.Generic;
using LeattyServer.Constants;
using LeattyServer.Data;
using LeattyServer.Data.WZ;
using LeattyServer.Helpers;
using LeattyServer.ServerInfo.Packets;

namespace LeattyServer.ServerInfo.Player
{
    public class Skill
    {       
        public int SkillId { get; set; }
        public short SkillExp { get; set; }
        public byte Level { get; set; }
        public byte MasterLevel { get; set; }
        public long Expiration { get; set; }        

        public Skill(int skillId, byte masterLevel = 0, byte level = 0)
        {
            SkillId = skillId;           
            MasterLevel = masterLevel;
            Expiration = -1;
            Level = level;
            SkillExp = 0;
        }

        #region Helpers        
        public bool HasMastery
        {
            get
            {                  
                WzCharacterSkill skillInfo = DataBuffer.GetCharacterSkillById(SkillId);
                if (skillInfo == null)
                    return false;
                else
                    return skillInfo.HasMastery;
            }
        }
        #endregion

        #region Packets
        public static PacketWriter UpdateSkills(List<Skill> skills)
        {
            PacketWriter pw = new PacketWriter();
            pw.WriteHeader(SendHeader.UpdateSkills);

            pw.WriteBool(true); //enable actions
            pw.WriteByte(0);
            pw.WriteByte(0);

            short count = (short)skills.Count;
            pw.WriteShort(count);
            for (int i = 0; i < count; i++)
            {
                pw.WriteInt(skills[i].SkillId);
                pw.WriteInt(skills[i].Level);
                pw.WriteInt(skills[i].MasterLevel);
                pw.WriteLong(MapleFormatHelper.GetMapleTimeStamp(skills[i].Expiration));
            }
            pw.WriteByte(0x2F);
            return pw;
        }

        public static PacketWriter UpdateSingleSkill(Skill skill)
        {
            return UpdateSkills(new List<Skill>() { skill });
        }

        public static PacketWriter ShowCooldown(int skillId, uint time)
        {
            PacketWriter pw = new PacketWriter();
            pw.WriteHeader(SendHeader.GiveCooldown);

            pw.WriteInt(skillId);
            pw.WriteUInt(time); //time in seconds, 0 = cooldown is removed
            return pw;
        }

        public static PacketWriter ShowOwnSkillEffect(int skillId, byte skillLevel)
        {
            PacketWriter pw = new PacketWriter();
            pw.WriteHeader(SendHeader.ShowSkillEffect);

            pw.WriteByte(2);
            pw.WriteInt(skillId);
            pw.WriteByte(skillLevel); 
            return pw;
        }

        //TODO: update this with final pact
        public static PacketWriter ShowBuffEffect(int skillId, byte characterLevel, byte? skillLevel, bool show) //remove if show = false
        {
            PacketWriter pw = new PacketWriter();
            pw.WriteHeader(SendHeader.ShowSkillEffect);

            pw.WriteByte(1);
            pw.WriteInt(skillId);
            pw.WriteByte(characterLevel);
            if (skillLevel.HasValue) //some use this and some don't, idk why :S
                pw.WriteByte(skillLevel.Value);
            pw.WriteBool(show);
            return pw;
        }
        #endregion

        public bool IsStealable 
        { 
            get 
            {
                int jobId = SkillId / 10000;
                return Level > 0 && jobId < 600 && SkillId % 10000 >= 1000 && !JobConstants.IsBeginnerJob(jobId) && !JobConstants.IsDualBlade(jobId) && !JobConstants.IsCannonneer(jobId) && !JobConstants.IsJett(jobId);
            }   
        }
    }
}
