﻿using NetworkSocket.Core;
using NetworkSocket.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebsocketChatServer
{
    /// <summary>
    /// 在线聊天Api服务单元
    /// </summary>
    public sealed class ChatApiService : JsonWebSocketApiService
    {
        /// <summary>
        /// 获取其它成员
        /// </summary>
        public IEnumerable<JsonWebSocketSession> OtherSessions
        {
            get
            {
                return this.CurrentContext
                    .AllSessions
                    .Where(item => item != this.CurrentContext.Session);
            }
        }

        /// <summary>
        /// 获取成员的名称
        /// </summary>
        /// <returns></returns>
        [Api]
        public string[] GetAllMembers()
        {
            var members = this.CurrentContext
                .AllSessions
                .Select(item => (string)item.TagBag.Name)
                .Where(item => item != null)
                .ToArray();

            return members;
        }

        /// <summary>
        /// 设置昵称
        /// </summary>
        /// <param name="name">名称</param>
        /// <returns></returns>
        [Api]
        public object SetNickName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return new { state = false, message = "昵称不能为空 .." };
            }

            if (this.OtherSessions.Any(item => item.TagBag.Name == name))
            {
                return new { state = false, message = "此昵称已经被占用 .." };
            }

            // 推送新成员上线提醒
            this.CurrentContext.Session.TagBag.Name = name;
            var members = this.GetAllMembers();

            foreach (var session in this.OtherSessions)
            {
                session.TryInvokeApi("OnMemberChange", 1, name, members);
            }
            return new { state = true, name };
        }

        /// <summary>
        /// 成员发表群聊天内容
        /// </summary>
        /// <param name="message">内容</param>
        /// <returns></returns>        
        [Api]
        [NickNameFilter] //设置了昵称才可以发言
        public bool ChatMessage(string message)
        {
            if (string.IsNullOrEmpty(message) == true)
            {
                return false;
            }

            var name = (string)this.CurrentContext.Session.TagBag.Name; // 发言人
            foreach (var session in this.OtherSessions)
            {
                session.TryInvokeApi("OnChatMessage", name, message, DateTime.Now.ToString("HH:mm:ss"));
            }
            return true;
        }
    }
}
