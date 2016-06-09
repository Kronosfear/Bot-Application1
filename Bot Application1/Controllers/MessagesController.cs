using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Utilities;
using Newtonsoft.Json;
using MyAnimeListSharp;
using MyAnimeListSharp.Facade;
using System.Xml;
using System.Text;
using MyAnimeListSharp.Facade.Async;
using System.Text.RegularExpressions;
using static System.Collections.Specialized.BitVector32;
using System.Runtime.Remoting.Contexts;
using System.Collections.Generic;

namespace SauceBot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        string aniname;
        string synopsis;
        string score;
        string url;
        string id;
        string re;
        string imgurl;

        public static string StripHTML(string input)
        {
            string x = Regex.Replace(input, @"\[[^]]*\]+", String.Empty);
            x = Regex.Replace(x, "[^a-zA-Z0-9 .,-]", String.Empty);
            x = Regex.Replace(x, "mdash", " ");
            x = Regex.Replace(x, "039", "\'");
            x = Regex.Replace(x, "quot", "\"");
            return x;
        }
        
        public async Task<Message> Post([FromBody]Message message)
        {
            if (message.Type == "Message" && message.Text.Contains("!sauce"))
            {
                // calculate something for us to return
                int length = (message.Text ?? string.Empty).Length;
                MyAnimeListSharp.Auth.ICredentialContext credential = new MyAnimeListSharp.Auth.CredentialContext
                {
                    UserName = "WizardOfAce",
                    Password = "8zagmwmy"
                };
                try
                {
                    message.Text = message.Text.Replace("!sauce ", string.Empty);
                    var asyncAnimeSearcher = new AnimeSearchMethodsAsync(credential);
                    var response = await asyncAnimeSearcher.SearchAsync(message.Text);
                    aniname = "";
                    score = "";
                    synopsis = "";
                    re = "";
                    imgurl = "";
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(response);
                    XmlNodeList parentNode = xmlDoc.GetElementsByTagName("entry");
                    foreach (XmlNode childrenNode in parentNode)
                    {
                        aniname = childrenNode.SelectSingleNode("title").InnerText;
                        id = childrenNode.SelectSingleNode("id").InnerText;
                        url = "http://myanimelist.net/anime/" + id + "/" + message.Text.Replace(' ', '_');
                        score = childrenNode.SelectSingleNode("score").InnerText;
                        synopsis = childrenNode.SelectSingleNode("synopsis").InnerText;
                        synopsis = synopsis.Replace("<br />", " ");
                        synopsis = synopsis.Replace("\"", "\\\"");
                        imgurl = childrenNode.SelectSingleNode("image").InnerText;


                        if (aniname != "")
                            break;

                    }
                    if (aniname != "")
                    {
                        StringBuilder res = new StringBuilder();
                        res.Append("**" + aniname + "**");
                        res.Append("\n");
                        res.Append("\nMAL URL: ");
                        res.Append(url);
                        res.Append("\n");
                        res.Append("\nScore: ");
                        res.Append(score);
                        res.Append("\n");
                        res.Append("\nSynopsis: ");
                        res.Append(StripHTML(synopsis));
                        re = res.ToString();
                        var returnMessage = message.CreateReplyMessage(re);
                        returnMessage.Attachments = new List<Attachment>
                        {
                            new Attachment
                            {
                                ContentType = "image/jpg",
                                ContentUrl = imgurl
                            }
                        };
                        return returnMessage;
                    }
                    else
                        return message.CreateReplyMessage($"Anime not found");
                }
                catch(XmlException e)
                {
                    return message.CreateReplyMessage("Anime not found");
                }
            }
            else
            {
                return HandleSystemMessage(message);
            }
        }

        private Message HandleSystemMessage(Message message)
        {
            if (message.Type == "Ping")
            {
                Message reply = message.CreateReplyMessage();
                reply.Type = "Ping";
                return reply;
            }
            else if (message.Type == "DeleteUserData")
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == "BotAddedToConversation")
            {
            }
            else if (message.Type == "BotRemovedFromConversation")
            {
            }
            else if (message.Type == "UserAddedToConversation")
            {
            }
            else if (message.Type == "UserRemovedFromConversation")
            {
            }
            else if (message.Type == "EndOfConversation")
            {
            }

            return null;
        }
    }
}