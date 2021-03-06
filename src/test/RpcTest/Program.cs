﻿using System;
using Agebull.ZeroNet.ZeroApi;

namespace RpcTest
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.ReadKey();
            var tester = new RegistTester();
            tester.RegistAction<VerificationApiController>();
            Console.ReadKey();
        }
    }

    /// <summary>
    /// 身份验证服务API
    /// </summary>
    public class VerificationApiController
    {
        /// <summary>
        ///     验证图片验证码:验证图片验证码:
        /// </summary>
        /// <param name="arg">验证码检查参数</param>
        /// <returns>操作结果</returns>
        [Route("v2/vc/img/valiadate")]
        [ApiAccessOptionFilter(ApiAccessOption.Internal | ApiAccessOption.Public | ApiAccessOption.Anymouse)]
        public static IApiResult CheckImgVerificationCode(IApiArgument arg)
        {
            return ApiResult.Error(-1,arg?.ToString());
        }
    }
}
