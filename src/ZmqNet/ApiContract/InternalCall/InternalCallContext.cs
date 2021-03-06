﻿using System;
using Newtonsoft.Json;

namespace Agebull.ZeroNet.ZeroApi
{
    /// <summary>
    /// 内部服务调用上下文（跨系统边界传递）
    /// </summary>
    [Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    public class CallContext
    {
        /// <summary>
        /// 请求服务身份
        /// </summary>
        [JsonProperty("sk", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public string ServiceKey { get; set; }

        /// <summary>
        /// 全局请求标识（源头为用户请求）
        /// </summary>
        [JsonProperty("ri", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public string RequestId { get; set; }

        /// <summary>
        /// 当前请求的Http Authorizaition
        /// </summary>
        [JsonProperty("ha", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public string Bear { get; set; }
        
        /// <summary>
        /// 参数类型
        /// </summary>
        [JsonProperty("at", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public ArgumentType ArgumentType { get; set; }

        /// <summary>
        /// 参数类型
        /// </summary>
        [JsonProperty("rt", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public RequestType RequestType { get; set; }

        /// <summary>
        /// 请求IP
        /// </summary>
        [JsonProperty("ip", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public string Ip { get; set; }

        /// <summary>
        /// 请求端口号
        /// </summary>
        [JsonProperty("pt", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public string Port { get; set; }
        
        /// <summary>
        /// HTTP的UserAgent
        /// </summary>
        [JsonProperty("ua", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public string UserAgent { get; set; }
        
    }

    /// <summary>
    /// 参数类型
    /// </summary>
    public enum ArgumentType
    {
        /// <summary>
        /// HTTP的Form格式
        /// </summary>
        HttpForm,
        /// <summary>
        /// Json格式
        /// </summary>
        Json,
        /// <summary>
        /// Tson格式（二进制）
        /// </summary>
        Tson,
        /// <summary>
        /// 自定义
        /// </summary>
        Custom
    }
    /// <summary>
    /// 参数类型
    /// </summary>
    public enum RequestType
    {
        /// <summary>
        /// 无
        /// </summary>
        None,
        /// <summary>
        /// HTTP请求
        /// </summary>
        Http,
        /// <summary>
        /// ZeroNet请求
        /// </summary>
        Zero
    }
}