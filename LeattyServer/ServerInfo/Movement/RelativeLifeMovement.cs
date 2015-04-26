﻿using System.Drawing;
using LeattyServer.ServerInfo.Packets;

namespace LeattyServer.ServerInfo.Movement
{
    class RelativeLifeMovement : MapleMovementFragment
    {
        public short Unknown { get; set; }

        public RelativeLifeMovement(byte type, Point position, byte state, short duration, short unknown)
            : base(type, position, state, duration)
        {            
            Unknown = unknown;
        }

        public override void Serialize(PacketWriter pw)
        {
            pw.WriteByte(Type);
            pw.WritePoint(Position);
            pw.WriteByte(State);
            pw.WriteShort(Duration);
        }
    }
}
