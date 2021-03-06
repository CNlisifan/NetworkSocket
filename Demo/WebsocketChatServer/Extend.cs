﻿using NetworkSocket.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebsocketChatServer
{
    /// <summary>
    /// 扩展
    /// </summary>
    public static class Extend
    {
        /// <summary>
        /// 调用远程端实现的服务方法        
        /// </summary>       
        /// <param name="api">api</param>
        /// <param name="parameters">参数列表</param>   
        /// <returns></returns>
        public static bool TryInvokeApi(this JsonWebSocketSession session, string api, params object[] parameters)
        {
            try
            {
                session.InvokeApi(api, parameters);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
