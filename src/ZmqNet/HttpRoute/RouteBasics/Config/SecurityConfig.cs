﻿using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace ZeroNet.Http.Route
{
    /// <summary>
    /// 安全相关的配置
    /// </summary>
    public class SecurityConfig
    {
        /// <summary>
        /// 启用验签
        /// </summary>
        [JsonProperty("fireSign")]
        public bool FireSign { get; set; }

        /// <summary>
        /// 黑洞地址
        /// </summary>
        [JsonProperty("blockAddress")]
        public string BlockHost { get; set; }

        /// <summary>
        /// 是否检查Auth头
        /// </summary>
        [JsonProperty("fireBearer")]
        public bool CheckBearerCache { get; set; }

        /// <summary>
        /// 黑名单令牌
        /// </summary>
        [JsonProperty("denyTokens")]
        public Dictionary<string, string> denyTokens { get; set; }

        /// <summary>
        /// 黑名单令牌
        /// </summary>
        [JsonIgnore]
        public Dictionary<string, string> DenyTokens { get; set; }

        /// <summary>
        /// 禁止的Http头信息
        /// </summary>
        [JsonProperty("denyHttpHeaders")] public List<DenyItem> denyHttpHeaders;
        /// <summary>
        /// 禁止的Http头信息
        /// </summary>
        [JsonProperty]
        public List<DenyItem> DenyHttpHeaders => denyHttpHeaders ?? (denyHttpHeaders = new List<DenyItem>());

        /// <summary>
        /// 需要检查的Api
        /// </summary>
        [JsonProperty("checkApis")] public Dictionary<string, ApiItem> checkApis;

        /// <summary>
        /// 需要检查的Api
        /// </summary>
        [JsonIgnore]
        public Dictionary<string, ApiItem> CheckApis { get; set; }
        
    }


    /// <summary>
    /// 阻止类型
    /// </summary>
    public enum DenyType
    {
        /// <summary>
        /// 不阻止
        /// </summary>
        None,
        /// <summary>
        /// 有此内容
        /// </summary>
        Hase,
        /// <summary>
        /// 没有此内容
        /// </summary>
        NonHase,
        /// <summary>
        /// 达到数组数量
        /// </summary>
        Count,
        /// <summary>
        /// 内容等于
        /// </summary>
        Equals,
        /// <summary>
        /// 内容包含
        /// </summary>
        Like,
        /// <summary>
        /// 正则匹配
        /// </summary>
        Regex
    }

    /// <summary>
    /// Api节点
    /// </summary>
    [JsonObject(MemberSerialization.OptIn), DataContract, Serializable]
    public class ApiItem
    {
        /// <summary>
        /// 头
        /// </summary>
        [JsonProperty]
        public string Name { get; set; }

        /// <summary>
        /// 是否允许不携带OAuth2.0的令牌
        /// </summary>
        [JsonProperty]
        public bool NoBearer { get; set; }

        /// <summary>
        /// 操作系统
        /// </summary>
        [JsonProperty]
        public string Os { get; set; }

        /// <summary>
        /// 浏览器
        /// </summary>
        [JsonProperty]
        public string Browser { get; set; }
    }

    /// <summary>
    /// 阻止节点
    /// </summary>
    [JsonObject(MemberSerialization.OptIn), DataContract, Serializable]
    public class DenyItem
    {
        /// <summary>
        /// 头
        /// </summary>
        [JsonProperty]
        public string Head { get; set; }
        /// <summary>
        /// 内容
        /// </summary>
        [JsonProperty]
        public string Value { get; set; }
        /// <summary>
        /// 类型
        /// </summary>
        [JsonProperty]
        public DenyType DenyType { get; set; }
    }

}