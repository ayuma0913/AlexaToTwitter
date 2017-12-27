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
            // ���N�G�X�g�^�C�v���擾
            var requestType = input.GetRequestType();

            // �C���e���g���N�G�X�g�ȊO�͖���
            if (requestType != typeof(IntentRequest)) return null;

            var intentRequest = input.Request as IntentRequest;

            // TwitterIntetn�ȊO�͖���
            if (!intentRequest.Intent.Name.Equals("TwitterIntent")) return null;

            // Word�X���b�g�̒l���擾
            var wordSlotValue = intentRequest.Intent.Slots["Word"].Value;

            // Twitter API�̕K�v���𐶐�
            var tokens = CoreTweet.Tokens.Create($"{APIKey}", $"{APISecret}", $"{AccessToken}", $"{AccessTokenSecret}");

            // �Ԃ₫���{
            tokens.Statuses.UpdateAsync(new { status = wordSlotValue }).Wait();

            // Axexa���牞��
            return ResponseBuilder.Tell($"{wordSlotValue}�ƂԂ₫�܂���");
        }
    }
}
