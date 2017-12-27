using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Amazon.Lambda.Core;
using Alexa.NET;
using Alexa.NET.Request;
using Alexa.NET.Response;
using Alexa.NET.Request.Type;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace AlexaToTwitter2
{
    public class Function
    {
        private readonly string APIKey;
        private readonly string APISecret;
        private readonly string AccessToken;
        private readonly string AccessTokenSecret;

        public Function()
        {
            APIKey = Environment.GetEnvironmentVariable("API_KEY");
            APISecret = Environment.GetEnvironmentVariable("API_KEY_SECRET");
            AccessToken = Environment.GetEnvironmentVariable("ACCESS_TOKEN");
            AccessTokenSecret = Environment.GetEnvironmentVariable("ACCESS_TOKEN_SECRET");
        }

        public SkillResponse FunctionHandler(SkillRequest input, ILambdaContext context)
        {
            // リクエストタイプを取得
            var requestType = input.GetRequestType();

            // インテントリクエスト以外は無視
            if (requestType != typeof(IntentRequest)) return null;

            var intentRequest = input.Request as IntentRequest;

            // TwitterIntetn以外は無視
            if (!intentRequest.Intent.Name.Equals("TwitterIntent")) return null;

            // Wordスロットの値を取得
            var wordSlotValue = intentRequest.Intent.Slots["Word"].Value;

            // Twitter APIの必要情報を生成
            var tokens = CoreTweet.Tokens.Create($"{APIKey}", $"{APISecret}", $"{AccessToken}", $"{AccessTokenSecret}");

            // つぶやき実施
            tokens.Statuses.UpdateAsync(new { status = wordSlotValue }).Wait();

            // Axexaから応答
            return ResponseBuilder.Tell($"{wordSlotValue}とつぶやきました");
        }
    }
}
