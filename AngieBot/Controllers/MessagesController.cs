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

                string uText = activity.Text.ToLower();
                foreach (string x in greetingArray)
                {
                    if (uText.Contains(x))
                    {
                        Activity reply = activity.CreateReply($"Hello! I'm a simple bot made by t-anng that will tell you your horoscope for today. What is your sign?");
                        await connector.Conversations.ReplyToActivityAsync(reply);

                        await connector.Conversations.ReplyToActivityAsync(activity.CreateReply($"(If you don't know your sign, say 'help')"));
                        understand = true;
                    }
                }
                foreach (string x in horoscopeArray)
                {
                    if (uText.Contains(x))
                    {

                        await connector.Conversations.ReplyToActivityAsync(activity.CreateReply($"Wow, a " + x + "!"));
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
                if (uText.Contains("help"))
                {
                    Activity helpReply = activity.CreateReply($"Here's the list of all the zodiac signs.\n\n"
                        + "Aries: March 21 - April 19\n\n"
                        + "Taurus: April 20 - May 20\n\n"
                        + "Gemini: May 21 - June 20\n\n"
                        + "Cancer: June 21 - July 22\n\n"
                        + "Leo: July 23 - August 22\n\n"
                        + "Virgo: August 23 - Septemeber 22\n\n"
                        + "Libra: September 23 - October 22\n\n"
                        + "Scorpio: October 23 - November 21\n\n"
                        + "Sagittarius: November 22 - December 21\n\n"
                        + "Capricorn: December 22 - January 19\n\n"
                        + "Aquarius: January 20 - February 18\n\n"
                        + "Pisces: February 19 - March 20");

                    await connector.Conversations.ReplyToActivityAsync(helpReply);
                    understand = true;
                }
                
                if (!understand)
                {
                    await connector.Conversations.ReplyToActivityAsync(activity.CreateReply($"Sorry, I couldn't understand what you said."));
                    await connector.Conversations.ReplyToActivityAsync(activity.CreateReply($"I'm a simple bot made by t-anng that will tell you your horoscope for today. What is your sign?"));

                    await connector.Conversations.ReplyToActivityAsync(activity.CreateReply($"(If you don't know your sign, say 'help'.)"));
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