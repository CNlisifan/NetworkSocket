﻿using NetworkSocket.Core;
using NetworkSocket.Exceptions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NetworkSocket.WebSocket
{
    /// <summary>
    /// 任务设置行为接口
    /// </summary>
    internal interface ITaskSetAction
    {
        /// <summary>
        /// 获取创建时间
        /// </summary>
        int CreateTime { get; }

        /// <summary>
        /// 设置行为
        /// </summary>
        /// <param name="setType">行为类型</param>
        /// <param name="value">数据值</param>
        /// <param name="serializer">序列化工具</param>
        void SetAction(SetTypes setType, object value, IDynamicJsonSerializer serializer);
    }

    /// <summary>
    /// 任务设置行为信息
    /// </summary>
    [DebuggerDisplay("InitTime = {InitTime}")]
    internal class TaskSetAction<T> : ITaskSetAction
    {
        /// <summary>
        /// 任务源
        /// </summary>
        private TaskCompletionSource<T> taskSource;

        /// <summary>
        /// 获取创建时间
        /// </summary>
        public int CreateTime { get; private set; }

        /// <summary>
        /// 任务设置行为
        /// </summary>               
        /// <param name="taskSource">任务源</param>
        public TaskSetAction(TaskCompletionSource<T> taskSource)
        {
            this.taskSource = taskSource;
            this.CreateTime = Environment.TickCount;
        }

        /// <summary>
        /// 设置行为
        /// </summary>
        /// <param name="setType">行为类型</param>
        /// <param name="value">数据值</param>
        /// <param name="serializer">序列化工具</param>
        public void SetAction(SetTypes setType, object value, IDynamicJsonSerializer serializer)
        {
            switch (setType)
            {
                case SetTypes.SetReturnReult:
                    this.SetResult(value, serializer);
                    break;

                case SetTypes.SetReturnException:
                    var remoteException = new RemoteException((string)value);
                    this.taskSource.TrySetException(remoteException);
                    break;

                case SetTypes.SetTimeoutException:
                    var timeoutException = new TimeoutException();
                    this.taskSource.TrySetException(timeoutException);
                    break;

                case SetTypes.SetShutdownException:
                    var shutdownException = new SocketException((int)SocketError.Shutdown);
                    this.taskSource.TrySetException(shutdownException);
                    break;
            }
        }

        /// <summary>
        /// 设置结果
        /// </summary>
        /// <param name="value">数据</param>
        /// <param name="serializer">序列化工具</param>
        private void SetResult(object value, IDynamicJsonSerializer serializer)
        {
            try
            {
                var result = (T)serializer.Convert(value, typeof(T));
                this.taskSource.TrySetResult(result);
            }
            catch (SerializerException ex)
            {
                this.taskSource.TrySetException(ex);
            }
            catch (Exception ex)
            {
                this.taskSource.TrySetException(new SerializerException(ex));
            }
        }
    }
}
