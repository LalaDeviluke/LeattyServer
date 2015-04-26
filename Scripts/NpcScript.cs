﻿using System;

namespace LeattyServer.Scripting
{
    public abstract class NpcScript : CharacterScript
    {
        /* NPC text reference sheet
        #b = Blue text.
        #c[itemid]# Shows how many [itemid] the player has in their inventory.
        #d = Purple text.
        #e = Bold text.
        #f[imagelocation]# - Shows an image inside the .wz files.
        #g = Green text.
        #h # - Shows the name of the player.
        #i[itemid]# - Shows a picture of the item.
        #k = Black text.
        #l - Selection close.
        #m[mapid]# - Shows the name of the map.
        #n = Normal text (removes bold).
        #o[mobid]# - Shows the name of the mob.
        #p[npcid]# - Shows the name of the NPC.
        #q[skillid]# - Shows the name of the skill.
        #r = Red text.
        #s[skillid]# - Shows the image of the skill.
        #t[itemid]# - Shows the name of the item.
        #v[itemid]# - Shows a picture of the item.
        #w - White text.
        #x - Returns "0%" (need more information on this).
        #z[itemid]# - Shows the name of the item.
        #B[%]# - Shows a 'progress' bar.
        #F[imagelocation]# - Shows an image inside the .wz files.
        #L[number]# Selection open.
        \r\n - Moves down a line.
        \r = Return Carriage
        \n = New Line
        \t = Tab (4 spaces)
        \b = Backwards
         */
        public int ObjectId;
        public int State = 0;
        public int Selection;
        public string InText;

        #region Messages
        
        public Action<string> SendOk;
        public Action<string> SendNext;
        public Action<string> SendPrev;
        public Action<string> SendNextPrev;
        public Action<string> SendSimple;
        public Action<string> SendYesNo;
        public Action<string, int, int, string> SendAskText;
        public Action EndChat;

        /*
        public virtual void SendOk(string text)
        {
            OnSendOk(text);
        }

        public virtual void SendNext(string text)
        {
            OnSendNext(text);
        }

        public virtual void SendPrev(string text)
        {
            OnSendPrev(text);
        }

        public virtual void SendNextPrev(string text)
        {
            OnSendNextPrev(text);
        }
        public virtual void SendPrevNext(string text)
        {
            OnSendNextPrev(text);
        }

        public virtual void SendSimple(string text)
        {
            OnSendSimple(text);
        }

        public virtual void SendYesNo(string text)
        {
            OnSendYesNo(text);
        }
        public virtual void SendAskText(string text, int min, int max, string textboxText = "")
        {
            OnSendAskText(text, min, max, textboxText);
        }

        public virtual void EndChat()
        {
            OnEndChat();
        }*/

        public virtual void OpenNpc(int npcId)
        {
            EndChat();
            Character.OpenNpc(npcId);
        }

        public virtual bool IsShop => false;     
        #endregion
    }
}