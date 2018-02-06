using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Agebull.Common.Logging;
using Agebull.ZeroNet.ZeroApi;
using Aliyun.Acs.Dysmsapi.Model.V20170525;
using Aliyun.Net.SDK.Core;
using Aliyun.Net.SDK.Core.Exceptions;
using Aliyun.Net.SDK.Core.Profile;
using Newtonsoft.Json;

namespace ZeroNet.Http.Route
{
    /// <summary>
    ///     ·������ʱ������ص�����
    /// </summary>
    public class RouteRuntime
    {
        #region Ĭ�Ϸ�������

        /// <summary>
        ///     �ܾ����ʵ�Json�ַ���
        /// </summary>
        internal static readonly string DenyAccess = JsonConvert.SerializeObject(ApiResult.Error(ErrorCode.DenyAccess, "*�ܾ�����*"));
        
        /// <summary>
        ///     �������޷���ֵ���ַ���
        /// </summary>
        internal static readonly string RemoteEmptyError =
            JsonConvert.SerializeObject(ApiResult.Error(ErrorCode.UnknowError, "*�������޷���ֵ*"));

        /// <summary>
        ///     �����������쳣
        /// </summary>
        internal static readonly string NetworkError =
            JsonConvert.SerializeObject(ApiResult.Error(ErrorCode.NetworkError, "*�����������쳣*"));

        /// <summary>
        ///     �����������쳣
        /// </summary>
        internal static readonly string Inner2Error =
            JsonConvert.SerializeObject(ApiResult.Error(ErrorCode.InnerError, "**ϵͳ�ڲ�����**"));

        /// <summary>
        ///     �����������쳣
        /// </summary>
        internal static readonly string InnerError =
            JsonConvert.SerializeObject(ApiResult.Error(ErrorCode.InnerError, "*ϵͳ�ڲ�����*"));


        #endregion

        #region ��������

        /// <summary>
        ///     ��������
        /// </summary>
        internal static Dictionary<string, CacheData> Cache =
            new Dictionary<string, CacheData>(StringComparer.OrdinalIgnoreCase);


        /// <summary>
        ///     ˢ��
        /// </summary>
        internal static void Flush()
        {
            lock (Cache)
            {
                Cache.Clear();
            }
            lock (WaringsTime)
                WaringsTime.Clear();
        }

        /// <summary>
        /// ��黺��
        /// </summary>
        /// <returns>ȡ�����棬����ֱ�ӷ���</returns>
        internal static bool LoadCache(Uri uri, string bearer, out CacheSetting setting, out string key, ref string resultMessage)
        {
            if (!AppConfig.Config.CacheMap.TryGetValue(uri.LocalPath, out setting))
            {
                key = null;
                return false;
            }

            if (setting.Feature.HasFlag(CacheFeature.Bear) && bearer.Substring(0, setting.Bear.Length) != setting.Bear)
            {
                setting = null;
                key = null;
                return false;
            }

            CacheData cacheData;

            lock (setting)
            {
                key = setting.OnlyName ? uri.LocalPath : uri.PathAndQuery;
                if (!Cache.TryGetValue(key, out cacheData))
                    return false;
                if (cacheData.UpdateTime <= DateTime.Now)
                {
                    Cache.Remove(key);
                    return false;
                }
            }

            resultMessage = cacheData.Content;
            return true;
        }

        /// <summary>
        /// ���淵��ֵ
        /// </summary>
        /// <param name="data"></param>
        internal static void CacheResult(RouteData data)
        {
            if (data.CacheSetting == null || !data.IsSucceed)
                return;
            CacheData cacheData;
            if (data.CacheSetting.Feature.HasFlag(CacheFeature.NetError) && data.Status == RouteStatus.RemoteError)
            {
                cacheData = new CacheData
                {
                    Content = data.ResultMessage,
                    UpdateTime = DateTime.Now.AddSeconds(30)
                };
            }
            else
            {
                cacheData = new CacheData
                {
                    Content = data.ResultMessage,
                    UpdateTime = DateTime.Now.AddSeconds(data.CacheSetting.FlushSecond)
                };
            }

            lock (data.CacheSetting)
            {
                if (!Cache.ContainsKey(data.CacheKey))
                    Cache.Add(data.CacheKey, cacheData);
                else
                    Cache[data.CacheKey] = cacheData;
            }
        }

        #endregion

        #region ��ά����

        /// <summary>
        /// ��鷵��ֵ�Ƿ����
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        internal static bool CheckResult(RouteData data)
        {
            if (data.Status != RouteStatus.None || data.HostName == null)// "".Equals(data.HostName,StringComparison.OrdinalIgnoreCase))
                return false;
            try
            {
                var result = JsonConvert.DeserializeObject<ApiResult>(data.ResultMessage);
                if (result == null)
                {
                    RuntimeWaring(data.HostName, data.ApiName, "����ֵ�Ƿ�(������)");
                    return false;
                }
                if (result.Status != null && !result.Result)
                {
                    switch (result.Status.ErrorCode)
                    {
                        case ErrorCode.ReTry:
                        case ErrorCode.DenyAccess:
                        case ErrorCode.Ignore:
                        case ErrorCode.ArgumentError:
                        case ErrorCode.Auth_RefreshToken_Unknow:
                        case ErrorCode.Auth_ServiceKey_Unknow:
                        case ErrorCode.Auth_AccessToken_Unknow:
                        case ErrorCode.Auth_User_Unknow:
                        case ErrorCode.Auth_Device_Unknow:
                        case ErrorCode.Auth_AccessToken_TimeOut:
                            return false;
                        default:
                            RuntimeWaring(data.HostName, data.ApiName, result.Status?.Message ?? "������������Ϣ");
                            return false;
                    }
                }
            }
            catch
            {
                RuntimeWaring(data.HostName, data.ApiName, "����ֵ�Ƿ�(����Json��ʽ)");
                return false;
            }
            return true;
        }

        /// <summary>
        /// ����ʱ����ڵ�
        /// </summary>
        public class RuntimeWaringItem
        {
            /*��ֹʱ��*/
            public DateTime StartTime, LastTime;
            /// <summary>
            /// ���һ�η�����ʱ��
            /// </summary>
            public DateTime MessageTime;
            /// <summary>
            /// �������������ʹ��������ͺ�������
            /// </summary>
            public int WaringCount, SendCount, LastCount;
            /// <summary>
            /// ���������API
            /// </summary>
            public Dictionary<string, List<string>> Apis = new Dictionary<string, List<string>>();
        }

        /// <summary>
        /// ��������ʱ����
        /// </summary>
        internal static readonly Dictionary<string, RuntimeWaringItem> WaringsTime = new Dictionary<string, RuntimeWaringItem>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// ����ʱ����
        /// </summary>
        /// <param name="host"></param>
        /// <param name="api"></param>
        /// <param name="message"></param>
        public static void RuntimeWaring(string host, string api, string message)
        {
            if (AppConfig.Config.SmsConfig == null || AppConfig.Config.SmsConfig.CycleHours <= 0)
                return;
            RuntimeWaringItem item;
            lock (WaringsTime)
            {
                if (!WaringsTime.TryGetValue(host, out item))
                {
                    WaringsTime.Add(host, item = new RuntimeWaringItem
                    {
                        StartTime = DateTime.Now,
                        LastTime = DateTime.Now,
                        WaringCount = 1,
                        LastCount = 1
                    });
                }
                else
                {
                    item.LastTime = DateTime.Now;
                    if (item.MessageTime != DateTime.MinValue && (DateTime.Now - item.MessageTime).TotalHours > AppConfig.Config.SmsConfig.CycleHours)
                    {
                        item.SendCount = 0;
                        item.WaringCount = 1;
                        item.LastCount = 1;
                    }
                    else
                    {
                        item.WaringCount += 1;
                        item.LastCount += 1;
                    }
                }

                if (!item.Apis.ContainsKey(api))
                    item.Apis.Add(api, new List<string> { message });
                else if (!item.Apis[api].Contains(message))
                    item.Apis[api].Add(message);
                //�ѵ���෢��������ֵ
                if (item.SendCount > AppConfig.Config.SmsConfig.CycleSendCount)
                    return;
                //����Ƶ������
                if (item.SendCount > 0)
                {
                    if (item.LastCount <= 10)
                    {
                        if ((DateTime.Now - item.MessageTime).TotalMinutes < 30.0)
                            return;
                    }
                    else if (item.LastCount <= 50)
                    {
                        if ((DateTime.Now - item.MessageTime).TotalMinutes < 20.0)
                            return;
                    }
                    else if (item.LastCount <= 200)
                    {
                        if ((DateTime.Now - item.MessageTime).TotalMinutes < 10.0)
                            return;
                    }
                    else if ((DateTime.Now - item.MessageTime).TotalMinutes < 5.0)
                    {
                        return;
                    }
                }
            }
            if (api.Length >= 20)
                api = api.Substring(api.Length - 19, 19);
            if (message.Length >= 20)
                message = message.Substring(20);
            //���Ͷ���
            Console.WriteLine($"������{host}��{api}����${message}����{item.LastCount}�Σ�����������");
            foreach (var phone in AppConfig.Config.SmsConfig.Phones)
            {
                if (!SendSmsByAli(host, phone, api, message, item.WaringCount))
                    continue;
                item.MessageTime = DateTime.Now;
                item.SendCount++;
                item.LastCount = 0;
            }
        }

        /// <summary>
        /// ������ŷ���
        /// </summary>
        /// <param name="server"></param>
        /// <param name="PhoneNumber"></param>
        /// <param name="api"></param>
        /// <param name="message"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        static bool SendSmsByAli(string server, string PhoneNumber, string api, string message, int count)
        {
            IClientProfile profile = DefaultProfile.GetProfile(AppConfig.Config.SmsConfig.AliRegionId, AppConfig.Config.SmsConfig.AliAccessKeyId, AppConfig.Config.SmsConfig.AliAccessKeySecret);
            DefaultProfile.AddEndpoint(AppConfig.Config.SmsConfig.AliEndPointName, AppConfig.Config.SmsConfig.AliRegionId, AppConfig.Config.SmsConfig.AliProduct, AppConfig.Config.SmsConfig.AliDomain);
            IAcsClient acsClient = new DefaultAcsClient(profile);
            var request = new SendSmsRequest
            {
                PhoneNumbers = PhoneNumber,
                SignName = AppConfig.Config.SmsConfig.AliSignName,
                TemplateCode = AppConfig.Config.SmsConfig.AliTemplateCode,
                //������${server}��${url}����${message}����${count}�Σ�����������
                TemplateParam = JsonConvert.SerializeObject(new
                {
                    server,
                    api,
                    message,
                    count
                }),
                OutId = server
            };
            SendSmsResponse sendSmsResponse;
            try
            {
                //����ʧ���������ClientException�쳣
                sendSmsResponse = acsClient.GetAcsResponse(request);
            }
            catch (ServerException e)
            {
                LogRecorder.Exception(e);
                return false;
            }
            catch (ClientException e)
            {
                LogRecorder.Exception(e);
                return false;
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e);
                return false;
            }
            return sendSmsResponse.Message == "OK";
        }


        #endregion
    }

}