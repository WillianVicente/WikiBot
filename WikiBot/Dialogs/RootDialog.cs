using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using WikiBot.Model;

namespace WikiBot.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        string responseString = string.Empty;

        static string knowledgebaseId = "5f29991c-056d-40cd-8337-040c71fb4bbe";
        static string qnamakerSubscriptionKey = "cf1fa4ef514c4ef59280691b46536d5d";
        static Uri qnamakerUriBase = new Uri("https://westus.api.cognitive.microsoft.com/qnamaker/v1.0");
        static UriBuilder builder = new UriBuilder($"{qnamakerUriBase}/knowledgebases/{knowledgebaseId}/generateAnswer");

        public Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);

            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;
            var postBody = $"{{\"question\": \"{activity.Text}\"}}";

            //Send the POST request
            using (WebClient client = new WebClient())
            {
                //Set the encoding to UTF8
                client.Encoding = System.Text.Encoding.UTF8;

                //Add the subscription key header
                client.Headers.Add("Ocp-Apim-Subscription-Key", qnamakerSubscriptionKey);
                client.Headers.Add("Content-Type", "application/json");
                responseString = client.UploadString(builder.Uri, postBody);
            }

            //De-serialize the response
            QnAMakerResult response;
            try
            {
                response = JsonConvert.DeserializeObject<QnAMakerResult>(responseString);
            }
            catch
            {
                throw new Exception("Unable to deserialize QnA Maker response string.");
            }

            await context.PostAsync($"Resposta: {response.Answer}, Score:{response.Score}");
            context.Wait(MessageReceivedAsync);
        }

        //private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        //{
        //    var activity = await result as Activity;
        //
        //    // calculate something for us to return
        //    int length = (activity.Text ?? string.Empty).Length;
        //
        //    // return our reply to the user
        //    
        //
        //    context.Wait(MessageReceivedAsync);
        //}
    }
}