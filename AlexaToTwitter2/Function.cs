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
    internal enum EConversationState
    {
        StartState,
        ConfirmState
    }

    public class Function
    {
        private readonly string APIKey;
        private readonly string APISecret;
        private readonly string AccessToken;
        private readonly string AccessTokenSecret;

        private Dictionary<EConversationState, Func<IntentRequest, Session, SkillResponse>> FunctionMap
            = new Dictionary<EConversationState, Func<IntentRequest, Session, SkillResponse>>();

        public Function()
        {
            APIKey = Environment.GetEnvironmentVariable("API_KEY");
            APISecret = Environment.GetEnvironmentVariable("API_KEY_SECRET");
            AccessToken = Environment.GetEnvironmentVariable("ACCESS_TOKEN");
            AccessTokenSecret = Environment.GetEnvironmentVariable("ACCESS_TOKEN_SECRET");

            // ステートに応じた関数をキャッシュしておく
            FunctionMap[EConversationState.StartState] = FunctionHandler_StartState;
            FunctionMap[EConversationState.ConfirmState] = FunctionHandler_ConfirmState;
        }

        public SkillResponse FunctionHandler(SkillRequest input, ILambdaContext context)
        {
            // リクエストタイプを取得
            var requestType = input.GetRequestType();

            // インテントリクエスト以外は無視
            if (requestType != typeof(IntentRequest)) return null;

            // ステートの読み取り
            EConversationState State = EConversationState.StartState;
            if (input.Session?.Attributes?.ContainsKey("STATE") == true)
            {
                Enum.TryParse(input.Session.Attributes["STATE"] as string, out State);
            }

            LambdaLogger.Log($">> STATE:{State.ToString()} <<\n");

            // ステートに応じたFunctionを呼び出し
            return FunctionMap[State](input.Request as IntentRequest, input.Session);
        }

        /// <summary>
        /// StartState時の処理
        /// </summary>
        /// <param name="intentRequest">リクエスト</param>
        /// <param name="Session">セッション</param>
        /// <returns>レスポンス</returns>
        private SkillResponse FunctionHandler_StartState(IntentRequest intentRequest, Session Session)
        {
            // TwitterIntent以外は無視
            if (intentRequest.Intent.Name.Equals("TwitterIntent") == false) return ResponseBuilder.Tell("予期しないリクエストです。中止します");

            // Wordスロットの値を取得
            var wordSlotValue = intentRequest.Intent.Slots["Word"].Value;

            // Axexaから応答
            Reprompt rep = new Reprompt();
            rep.OutputSpeech = new PlainTextOutputSpeech() { Text = "つぶやいていいですか?" };
            Session.Attributes = new Dictionary<string, object>();
            // つぶやく文言を記憶する
            Session.Attributes["Word"] = wordSlotValue;
            // ステートをConfirmStateに変更
            Session.Attributes["STATE"] = EConversationState.ConfirmState.ToString();

            return ResponseBuilder.Ask($"{wordSlotValue}とつぶやいていいですか?", rep, Session);
        }

        /// <summary>
        /// ConfirmState時の処理
        /// </summary>
        /// <param name="intentRequest">リクエスト</param>
        /// <param name="Session">セッション</param>
        /// <returns>レスポンス</returns>
        private SkillResponse FunctionHandler_ConfirmState(IntentRequest intentRequest, Session Session)
        {
            // NOが返ってきたらやめる
            if (intentRequest.Intent.Name.Equals("AMAZON.NoIntent"))
            {
                return ResponseBuilder.Tell("はい、やめます");
            }

            // YES以外は想定外なのでやめる
            if (intentRequest.Intent.Name.Equals("AMAZON.YesIntent") == false) return ResponseBuilder.Tell("予期しない返答です。中止します");
          
            // 記憶しておいた文言を取得
            var wordSlotValue = Session.Attributes["Word"] as string;

            // Twitter APIの必要情報を生成
            var tokens = CoreTweet.Tokens.Create($"{APIKey}", $"{APISecret}", $"{AccessToken}", $"{AccessTokenSecret}");

            // つぶやき実施
            tokens.Statuses.UpdateAsync(new { status = wordSlotValue }).Wait();

            // 結果を報告
            return ResponseBuilder.Tell($"{wordSlotValue}とつぶやきました");
          
        }
    }
}
