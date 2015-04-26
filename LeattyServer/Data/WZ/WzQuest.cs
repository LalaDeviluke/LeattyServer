﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeattyServer.Data.WZ
{
    public class WzQuest
    {
        public ushort Id { get; set; }
        public List<WzQuestRequirement> StartRequirements = new List<WzQuestRequirement>();
        public List<WzQuestRequirement> FinishRequirements = new List<WzQuestRequirement>();
        public List<WzQuestAction> StartActions = new List<WzQuestAction>();
        public List<WzQuestAction> FinishActions = new List<WzQuestAction>();

        public WzQuest(ushort id)
        {
            Id = id;
        }
    }   
}
