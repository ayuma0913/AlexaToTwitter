# AlexaToTwitter
Amazon Echoを使用してTwitterへ挨拶文を投稿するカスタムスキルのLambda部分です。

以下のリクエストに反応にしてTwitterに投稿します。

例）「あれくさ、ついったーでおはようってつぶやいて」

# スキル設定
## スキル名 ＆ 呼び出し名
ともに「ついったー」としました。

## インテントスキーマ
TwitterIntentというインテントで、WordというスロットをGREETING_WORD型で定義

```json
{
  "intents": [
    {
      "slots": [
        {
          "name": "Word",
          "type": "GREETING_WORD"
        }
      ],
      "intent": "TwitterIntent"
    },
    {
      "intent": "AMAZON.HelpIntent"
    },
    {
      "intent": "AMAZON.StopIntent"
    }
  ]
}
```

## カスタムスロットタイプ
タイプ：GREETING_WORD

値：  
おはよう  
こんにちは  
おやすみ  
こんばんは  
やあ  
へい  
はーい  

## サンプル発話
以下の５通り

TwitterIntent {Word} をつぶやいて  
TwitterIntent {Word} ってつぶやいて  
TwitterIntent {Word} ってやって  
TwitterIntent {Word} て言って  
TwitterIntent {Word} して  

# AWS Lambda Empty Function Project

This starter project consists of:
* Function.cs - class file containing a class with a single function handler method
* aws-lambda-tools-defaults.json - default argument settings for use with Visual Studio and command line deployment tools for AWS
* project.json - .NET Core project file with build and tool declarations for the Amazon.Lambda.Tools Nuget package

You may also have a test project depending on the options selected.

The generated function handler is a simple method accepting a string argument that returns the uppercase equivalent of the input string. Replace the body of this method, and parameters, to suit your needs. 

## Here are some steps to follow from Visual Studio:

To deploy your function to AWS Lambda, right click the project in Solution Explorer and select *Publish to AWS Lambda*.

To view your deployed function open its Function View window by double-clicking the function name shown beneath the AWS Lambda node in the AWS Explorer tree.

To perform testing against your deployed function use the Test Invoke tab in the opened Function View window.

To configure event sources for your deployed function, for example to have your function invoked when an object is created in an Amazon S3 bucket, use the Event Sources tab in the opened Function View window.

To update the runtime configuration of your deployed function use the Configuration tab in the opened Function View window.

To view execution logs of invocations of your function use the Logs tab in the opened Function View window.

## Here are some steps to follow to get started from the command line:

Once you have edited your function you can use the following command lines to build, test and deploy your function to AWS Lambda from the command line (these examples assume the project name is *EmptyFunction*):

Restore dependencies
```
    cd "AlexaToTwitter"
    dotnet restore
```

Execute unit tests
```
    cd "AlexaToTwitter/test/AlexaToTwitter.Tests"
    dotnet test
```

Deploy function to AWS Lambda
```
    cd "AlexaToTwitter/src/AlexaToTwitter"
    dotnet lambda deploy-function
```
