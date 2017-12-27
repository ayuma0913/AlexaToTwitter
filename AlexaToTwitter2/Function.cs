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

            // �X�e�[�g�ɉ������֐����L���b�V�����Ă���
            FunctionMap[EConversationState.StartState] = FunctionHandler_StartState;
            FunctionMap[EConversationState.ConfirmState] = FunctionHandler_ConfirmState;
        }

        public SkillResponse FunctionHandler(SkillRequest input, ILambdaContext context)
        {
            // ���N�G�X�g�^�C�v���擾
            var requestType = input.GetRequestType();

            // �C���e���g���N�G�X�g�ȊO�͖���
            if (requestType != typeof(IntentRequest)) return null;

            // �X�e�[�g�̓ǂݎ��
            EConversationState State = EConversationState.StartState;
            if (input.Session?.Attributes?.ContainsKey("STATE") == true)
            {
                Enum.TryParse(input.Session.Attributes["STATE"] as string, out State);
            }

            LambdaLogger.Log($">> STATE:{State.ToString()} <<\n");

            // �X�e�[�g�ɉ�����Function���Ăяo��
            return FunctionMap[State](input.Request as IntentRequest, input.Session);
        }

        /// <summary>
        /// StartState���̏���
        /// </summary>
        /// <param name="intentRequest">���N�G�X�g</param>
        /// <param name="Session">�Z�b�V����</param>
        /// <returns>���X�|���X</returns>
        private SkillResponse FunctionHandler_StartState(IntentRequest intentRequest, Session Session)
        {
            // TwitterIntent�ȊO�͖���
            if (intentRequest.Intent.Name.Equals("TwitterIntent") == false) return ResponseBuilder.Tell("�\�����Ȃ����N�G�X�g�ł��B���~���܂�");

            // Word�X���b�g�̒l���擾
            var wordSlotValue = intentRequest.Intent.Slots["Word"].Value;

            // Axexa���牞��
            Reprompt rep = new Reprompt();
            rep.OutputSpeech = new PlainTextOutputSpeech() { Text = "�Ԃ₢�Ă����ł���?" };
            Session.Attributes = new Dictionary<string, object>();
            // �Ԃ₭�������L������
            Session.Attributes["Word"] = wordSlotValue;
            // �X�e�[�g��ConfirmState�ɕύX
            Session.Attributes["STATE"] = EConversationState.ConfirmState.ToString();

            return ResponseBuilder.Ask($"{wordSlotValue}�ƂԂ₢�Ă����ł���?", rep, Session);
        }

        /// <summary>
        /// ConfirmState���̏���
        /// </summary>
        /// <param name="intentRequest">���N�G�X�g</param>
        /// <param name="Session">�Z�b�V����</param>
        /// <returns>���X�|���X</returns>
        private SkillResponse FunctionHandler_ConfirmState(IntentRequest intentRequest, Session Session)
        {
            // NO���Ԃ��Ă������߂�
            if (intentRequest.Intent.Name.Equals("AMAZON.NoIntent"))
            {
                return ResponseBuilder.Tell("�͂��A��߂܂�");
            }

            // YES�ȊO�͑z��O�Ȃ̂ł�߂�
            if (intentRequest.Intent.Name.Equals("AMAZON.YesIntent") == false) return ResponseBuilder.Tell("�\�����Ȃ��ԓ��ł��B���~���܂�");
          
            // �L�����Ă������������擾
            var wordSlotValue = Session.Attributes["Word"] as string;

            // Twitter API�̕K�v���𐶐�
            var tokens = CoreTweet.Tokens.Create($"{APIKey}", $"{APISecret}", $"{AccessToken}", $"{AccessTokenSecret}");

            // �Ԃ₫���{
            tokens.Statuses.UpdateAsync(new { status = wordSlotValue }).Wait();

            // ���ʂ��
            return ResponseBuilder.Tell($"{wordSlotValue}�ƂԂ₫�܂���");
          
        }
    }
}
