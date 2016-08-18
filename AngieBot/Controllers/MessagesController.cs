using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;

namespace AngieBot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            if (activity.Type == ActivityTypes.Message)
            {
                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                // calculate something for us to return
                int length = (activity.Text ?? string.Empty).Length;
                bool understand = false;
                
                // Check if they said hey or hi or hello
                string[] greetingArray = { "hey", "hello", "sup", "hi", "howdy" };
                string[] horoscopeArray = { "aries", "taurus", "gemini", "cancer", "leo", "virgo", "libra", "scorpio", "sagittarius", "capricorn", "aquarius", "pisces" };
                foreach (string x in greetingArray)
                {
                    if (activity.Text.Contains(x))
                    {
                        Activity reply = activity.CreateReply($"Hello! I'm a simple bot made by t-anng that will tell you your horoscope for today. What is your sign?");
                        await connector.Conversations.ReplyToActivityAsync(reply);
                        understand = true;
                    }
                }
                foreach (string x in horoscopeArray)
                {
                    if (activity.Text.Contains(x))
                    {
                        using (var client = new HttpClient())
                        {
                            client.DefaultRequestHeaders.Add("Accept", "application/json;odata=verbose");
                            client.DefaultRequestHeaders.Add("ContentType", "application/json");

                            HttpResponseMessage res = await client.GetAsync("http://horoscope-api.herokuapp.com/horoscope/today/" + x);
                            if (res.IsSuccessStatusCode)
                            {
                                Console.WriteLine(res.StatusCode.ToString());
                                string responseString = await res.Content.ReadAsStringAsync();
                                Horoscope model = JsonConvert.DeserializeObject<Horoscope>(responseString);
                                                               
                                await connector.Conversations.ReplyToActivityAsync(activity.CreateReply($"Wow, a " + model.sunsign + "!"));
                                
                                await connector.Conversations.ReplyToActivityAsync(activity.CreateReply(model.horoscope));
                                
                                await connector.Conversations.ReplyToActivityAsync(activity.CreateReply($"Special thanks to this Horoscope API: http://tapasweni-pathak.github.io/Horoscope-API/"));
                                                            }
                            else
                            {
                                await connector.Conversations.ReplyToActivityAsync(activity.CreateReply($"Something went wrong with fetching your horoscope. :("));
                            }
                        }
                        understand = true;
                    }
                }
                
                if (!understand)
                {
                    Activity reply2 = activity.CreateReply($"Sorry, I can't understand what you're saying. What is your sign?");
                    await connector.Conversations.ReplyToActivityAsync(reply2);
                }
            }
            else
            {
                HandleSystemMessage(activity);
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }
    }
}