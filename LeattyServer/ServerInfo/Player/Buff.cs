﻿using System;
using System.Collections.Generic;
using System.Timers;
using LeattyServer.Constants;
using LeattyServer.Data.WZ;
using LeattyServer.Helpers;
using LeattyServer.ServerInfo.Packets;

namespace LeattyServer.ServerInfo.Player
{
    public class Buff
    {
        public int SkillId {get; private set; }        
        public uint Duration { get; private set; }
        public DateTime StartTime { get; private set; }
        public SkillEffect Effect { get; private set; }
        public Timer CancelSchedule { get; private set; }
        public int Stacks { get; set; }
        
       
        /// <summary>
        ///
        /// </summary>
        /// <param name="skillId"></param>
        /// <param name="effect"></param>
        /// <param name="duration">Milliseconds</param>
        /// <param name="startTime"></param>
        /// <param name="affectedCharacter"></param>
        public Buff(int skillId, SkillEffect effect, uint duration, MapleCharacter affectedCharacter, DateTime? nStartTime = null)
        {
            DateTime startTime = nStartTime ?? DateTime.UtcNow;
            SkillId = skillId;
            Effect = effect;
            Duration = duration;
            StartTime = startTime;
            if (duration < SkillEffect.MAX_BUFF_TIME_MS)
            {
                CancelSchedule = Scheduler.ScheduleRemoveBuff(affectedCharacter, skillId, duration);                
            }
            Stacks = 1;
        }

        
        public void CancelRemoveBuffSchedule()
        {            
            Scheduler.DisposeTimer(CancelSchedule);
        }

        public static PacketWriter CancelBuff(Buff buff) 
        {
            PacketWriter pw = new PacketWriter();
            pw.WriteHeader(SendHeader.RemoveBuff);

            WriteBuffMask(pw, buff.Effect.BuffInfo.Keys);

            foreach (var kvp in buff.Effect.BuffInfo)
            {
                if (kvp.Key.IsStackingBuff)
                    pw.WriteInt(0);
            }

            pw.WriteInt(0);
            return pw;
        }

        public static void WriteBuffMask(PacketWriter pw, ICollection<BuffStat> buffStats)
        {
            int[] mask = new int[GameConstants.BUFFSTAT_MASKS];
            foreach (BuffStat buffStat in buffStats)
            {
                int maskIndex = buffStat.BitIndex / 32;
                int relativeBitPos = buffStat.BitIndex % 32;
                int bit = 1 << 31 - relativeBitPos;
                mask[maskIndex] |= bit;
            }
            for (int i = 0; i < mask.Length; i++)
            {
                pw.WriteInt(mask[i]);
            }
        }
    

        public static void WriteSingleBuffMask(PacketWriter pw, BuffStat buffStat)
        {
            WriteBuffMask(pw, new List<BuffStat>() { buffStat });      
        }      

        public static PacketWriter GiveBuff(Buff buff)
        {
            PacketWriter pw = new PacketWriter(SendHeader.GiveBuff);

            WriteBuffMask(pw, buff.Effect.BuffInfo.Keys);
            bool stacked = false;
            foreach (var b in buff.Effect.BuffInfo)
            {
                if (b.Key.IsStackingBuff)
                {
                    if (!stacked)
                    {
                        stacked = true;
                        pw.WriteInt(0);
                        pw.WriteByte(0); //? 
                        pw.WriteInt(0);
                        if (buff.SkillId == DarkKnight.SACRIFICE || buff.SkillId == Bishop.ADVANCED_BLESSING)
                            pw.WriteInt(0);
                    }
                    pw.WriteInt(1); //amount of the same buffstat
                    pw.WriteInt(buff.SkillId);
                    if (b.Key.UsesStacksAsValue)
                        pw.WriteInt(buff.Stacks);
                    else
                        pw.WriteInt(b.Value);
                    pw.WriteInt(0); //tickcount?
                    pw.WriteInt(0);
                }
                else
                {
                    if (b.Key.UsesStacksAsValue)
                        pw.WriteShort((short)buff.Stacks);
                    /*else if (b.Key == MapleBuffStat.SHADOW_PARTNER)
                        pw.WriteInt(b.Value);*/
                    else
                        pw.WriteShort((short)b.Value);
                    pw.WriteInt(buff.SkillId);
                }
                pw.WriteUInt(buff.Duration);
            }
            
            if (!stacked)
            {
                pw.WriteZeroBytes(9);
            }

            pw.WriteInt(0);
            pw.WriteInt(0);
            pw.WriteInt(0);

            return pw;
        }

        #region Special Buffs
        public static PacketWriter GiveLuminousStateBuff(Buff buff, int lightGauge, int darkGauge, int lightLevel, int darkLevel)
        {
            //[01 00] [19 CA 31 01] [00 C2 EB 0B] [00 00 00 00 00] [19 CA 31 01] [B7 FC FA 24] [00 00 00 00 00 00 00 00] [B7 01 00 00] [C3 01 00 00] [00 00 00 00] [01 00 00 00] B7 3A 0F 19 00 00 00 00 00 00 00 00 01 00 00 00 00
            PacketWriter pw = new PacketWriter();
            pw.WriteHeader(SendHeader.GiveBuff);

            WriteBuffMask(pw, buff.Effect.BuffInfo.Keys);
            pw.WriteShort(1);
            pw.WriteInt(buff.SkillId);
            pw.WriteUInt(buff.Duration);
            pw.WriteZeroBytes(5);
            pw.WriteInt(buff.SkillId);
            pw.WriteInt(0);
            pw.WriteLong(0);
            pw.WriteInt(darkGauge);
            pw.WriteInt(lightGauge);
            pw.WriteInt(darkLevel);
            pw.WriteInt(lightLevel);
            pw.WriteZeroBytes(17);
            
            return pw;
        }
        public static PacketWriter GiveEvilEyeBuff(Buff buff)
        {
            //[01 00] [19 CA 31 01] [00 C2 EB 0B] [00 00 00 00 00] [19 CA 31 01] [B7 FC FA 24] [00 00 00 00 00 00 00 00] [B7 01 00 00] [C3 01 00 00] [00 00 00 00] [01 00 00 00] B7 3A 0F 19 00 00 00 00 00 00 00 00 01 00 00 00 00
            PacketWriter pw = new PacketWriter();
            pw.WriteHeader(SendHeader.GiveBuff);

            //WriteSingleBuffMask(pw, MapleBuffStat.EVIL_EYE);
            pw.WriteShort(1);
            pw.WriteInt(Spearman.EVIL_EYE);
            pw.WriteUInt(buff.Duration);
            pw.WriteInt(0);
            if (buff.Stacks == Berserker.EVIL_EYE_OF_DOMINATION)
            {
                pw.WriteByte(0x13);
                pw.WriteInt(Berserker.EVIL_EYE_OF_DOMINATION);
                pw.WriteInt(0);
                pw.WriteLong(0);
                pw.WriteInt(1);
                pw.WriteByte(0);
            }
            else
            {
                pw.WriteByte(0x13);
                pw.WriteInt(Spearman.EVIL_EYE);
                pw.WriteInt(0);
                pw.WriteLong(0);
                pw.WriteInt(0);
                pw.WriteByte(0);
                
            }
            return pw;
        }

        public static PacketWriter GiveCrossSurgeBuff(Buff buff, MapleCharacter chr, SkillEffect effect)
        {
            BuffedCharacterStats stats = chr.Stats;
            int damageIncPercent = effect.Info[CharacterSkillStat.x];
            int absorbPercent = effect.Info[CharacterSkillStat.y];            
            PacketWriter pw = new PacketWriter();
            pw.WriteHeader(SendHeader.GiveBuff);

            WriteBuffMask(pw, buff.Effect.BuffInfo.Keys);
            double hpPercent = (chr.Hp / (double)stats.MaxHp) * 100;
            short dmgInc = (short)(hpPercent * (damageIncPercent / 100.0));
            pw.WriteShort(dmgInc);
            pw.WriteInt(buff.SkillId);
            pw.WriteUInt(buff.Duration);

            pw.WriteInt(0);
            pw.WriteByte(0);
            int absorb = (int)((stats.MaxHp - chr.Hp) * (absorbPercent / 100.0));
            absorb = Math.Min(absorb, 4000);
            pw.WriteInt(absorb);
            pw.WriteInt(0);
            pw.WriteInt(540); //?
            pw.WriteInt(1);
            pw.WriteByte(0);           

            return pw;
        }

        public static PacketWriter GiveFinalPactBuff(Buff buff)
        {            
            PacketWriter pw = new PacketWriter();
            pw.WriteHeader(SendHeader.GiveBuff);

            WriteBuffMask(pw, buff.Effect.BuffInfo.Keys);

            pw.WriteShort(1);
            pw.WriteInt(DarkKnight.FINAL_PACT2);
            pw.WriteUInt(buff.Duration);

            pw.WriteShort(1);
            pw.WriteInt(DarkKnight.FINAL_PACT2);
            pw.WriteUInt(buff.Duration);

            pw.WriteInt(0);
            pw.WriteByte(0);
            pw.WriteInt(buff.Stacks);

            pw.WriteInt(0);
            pw.WriteInt(1);
            pw.WriteInt(DarkKnight.FINAL_PACT2);
            pw.WriteInt(buff.Effect.Info[CharacterSkillStat.indieDamR]);
            pw.WriteInt(0);
            pw.WriteInt(0);
            pw.WriteUInt(buff.Duration);

            pw.WriteInt(0);
            pw.WriteInt(1);
            pw.WriteByte(0);

            return pw;
        }

        public static PacketWriter UpdateFinalPactKillCount(int remainingKillCount, uint remainingDurationMS)
        {
            PacketWriter pw = new PacketWriter();
            pw.WriteHeader(SendHeader.GiveBuff);            
            //WriteSingleBuffMask(pw, MapleBuffStat.FINAL_PACT3);

            pw.WriteShort(1);
            pw.WriteInt(DarkKnight.FINAL_PACT2);
            pw.WriteUInt(remainingDurationMS);

            pw.WriteInt(0);
            pw.WriteByte(0);
            pw.WriteInt(remainingKillCount);
           
            pw.WriteInt(0);            
            pw.WriteInt(0);
            pw.WriteInt(0);
            pw.WriteByte(0);

            return pw;
        }
        #endregion

        // Used for testing purposes
        public static PacketWriter GiveBuffTestPacket(BuffStat b, int value, int skillId = 1002, int duration = (5 * 60 * 1000))
        {
            PacketWriter pw = new PacketWriter(SendHeader.GiveBuff);

            WriteSingleBuffMask(pw, b);
            bool stacked = false;
            if (b.IsStackingBuff)
            {
                if (!stacked)
                {
                    stacked = true;
                    pw.WriteInt(0);
                    pw.WriteByte(0); //? 
                    pw.WriteInt(0);
                }
                pw.WriteInt(1); //amount of the same buffstat
                pw.WriteInt(skillId);
                pw.WriteInt(value);
                pw.WriteInt(0); //tickcount?
                pw.WriteInt(0);
            }
            else
            {
                pw.WriteShort((short)value);
                pw.WriteInt(skillId);
            }
            pw.WriteInt(duration);
            if (!stacked)
            {
                pw.WriteZeroBytes(9);
            }

            pw.WriteInt(0);
            pw.WriteInt(0);
            pw.WriteInt(0);

            return pw;
        }

        public static PacketWriter RemoveBuffTestPacket(BuffStat buffstat)
        {
            PacketWriter pw = new PacketWriter(SendHeader.RemoveBuff);

            WriteSingleBuffMask(pw, buffstat);
            pw.WriteInt(0); //if stacked it shows remaining stacks I think;
            pw.WriteInt(0);
            return pw;
        }
    }    
}