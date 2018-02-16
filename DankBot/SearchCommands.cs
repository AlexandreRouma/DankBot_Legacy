using Discord;
using Discord.WebSocket;
using Google.Apis.YouTube.v3.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tweetinvi;
using Tweetinvi.Models;
using UrbanDictionnet;

namespace DankBot
{
    class SearchCommands
    {
        public static async Task Google(SocketMessage message, string[] arg, string msg)
        {
            if (arg.Count() > 1)
            {
                try
                {
                    Google.Apis.Customsearch.v1.Data.Result r = GoogleHelper.Search(msg.Substring(2), !message.Channel.IsNsfw);
                    await message.Channel.SendMessageAsync("", false, GenGoogleEmbed(r.Title, r.Link, r.Snippet));
                }
                catch
                {
                    await message.Channel.SendMessageAsync($":no_entry: `No results found`");
                }
                
            }
            else
            {
                await message.Channel.SendMessageAsync($":white_check_mark: `Please enter a search term`");
            }
        }

        public static async Task Image(SocketMessage message, string[] arg, string msg)
        {
            if (arg.Count() > 1)
            {
                try
                {
                    Google.Apis.Customsearch.v1.Data.Result r = GoogleHelper.SearchImage(msg.Substring(2), !message.Channel.IsNsfw);
                    await message.Channel.SendMessageAsync(r.Link);
                }
                catch
                {
                    await message.Channel.SendMessageAsync($":no_entry: `No results found`");
                }
            }
            else
            {
                await message.Channel.SendMessageAsync($":no_entry: `Please enter a search term`");
            }
        }

        public static async Task Youtube(SocketMessage message, string[] arg, string msg)
        {
            try
            {
                await message.Channel.SendMessageAsync(YouTubeHelper.Search(msg.Substring(3)).Url);
            }
            catch
            {
                await message.Channel.SendMessageAsync($":no_entry: `That video doesn't exist nigga`:joy:");
            }
        }

        public static async Task Urban(SocketMessage message, string[] arg, string msg)
        {
            if (arg.Count() > 1)
            {
                UrbanClient uc = new UrbanClient();
                try
                {
                    string w = msg.Substring(6);
                    DefinitionData word;
                    if (w == "random")
                    {
                        word = await uc.GetRandomWordAsync();
                    }
                    else
                    {
                        word = (await uc.GetWordAsync(w)).FirstOrDefault();
                    }
                    await message.Channel.SendMessageAsync("", false, genUrban(word));
                }
                catch
                {
                    await message.Channel.SendMessageAsync($":no_entry: `No results` :thinking:");
                }
            }
            else
            {
                await message.Channel.SendMessageAsync($":no_entry: `Tell me which word to search...`");
            }
        }

        public static async Task Comment(SocketMessage message, string[] arg, string msg)
        {
            if (arg.Count() > 1)
            {
                try
                {
                    YouTubeVideo video = YouTubeHelper.Search(msg.Substring(8));
                    CommentThreadListResponse comments = YouTubeHelper.GetComments(video.Url.Substring(32));
                    if (comments == null)
                    {
                        await message.Channel.SendMessageAsync($":no_entry: `Could not list comments for this video`");
                    }
                    else
                    {
                        await message.Channel.SendMessageAsync("", false, genYtComment(comments.Items.ElementAt(new Random().Next(comments.Items.Count)).Snippet.TopLevelComment.Snippet));
                    }
                }
                catch
                {
                    await message.Channel.SendMessageAsync($":no_entry: `That video doesn't exist nigga`:joy:");
                }
            }
            else
            {
                await message.Channel.SendMessageAsync($":no_entry: `Please include a video link`");
            }
        }

        public static async Task LastTweet(SocketMessage message, string[] arg, string msg)
        {
            if (arg.Count() > 1)
            {
                try
                {
                    Auth.SetUserCredentials(ConfigUtils.Configuration.TwitterConsumerKey,
                                            ConfigUtils.Configuration.TwitterConsumerSecret,
                                            ConfigUtils.Configuration.TwitterAccessToken,
                                            ConfigUtils.Configuration.TwitterAccessTokenSecret);
                    await message.Channel.SendMessageAsync("", false, genTwitter(User.GetUserFromScreenName(msg.Substring(10)).GetUserTimeline(10).FirstOrDefault()));
                }
                catch
                {
                    await message.Channel.SendMessageAsync($":no_entry: `Could not gather tweeter data for this user`");
                }
            }
            else
            {
                await message.Channel.SendMessageAsync($":no_entry: `Please enter the username of the twitter user`");
            }
        }

        public static async Task RandomTweet(SocketMessage message, string[] arg, string msg)
        {
            if (arg.Count() > 1)
            {
                try
                {
                    Auth.SetUserCredentials(ConfigUtils.Configuration.TwitterConsumerKey,
                                            ConfigUtils.Configuration.TwitterConsumerSecret,
                                            ConfigUtils.Configuration.TwitterAccessToken,
                                            ConfigUtils.Configuration.TwitterAccessTokenSecret);
                    var tweets = User.GetUserFromScreenName(msg.Substring(12)).GetUserTimeline(500);
                    await message.Channel.SendMessageAsync("", false, genTwitter(tweets.ElementAt(new Random().Next(tweets.Count()))));
                }
                catch
                {
                    await message.Channel.SendMessageAsync($":no_entry: `Could not gather tweeter data for this user`");
                }
            }
            else
            {
                await message.Channel.SendMessageAsync($":no_entry: `Please enter the username of the twitter user`");
            }
        }

        static Embed GenGoogleEmbed(string title, string url, string desc)
        {
            EmbedBuilder em = new EmbedBuilder();
            em.Color = Discord.Color.Blue;
            em.Title = title;
            em.Description = desc;
            em.Url = url;
            return em.Build();
        }

        static Embed genUrban(DefinitionData definition)
        {
            EmbedBuilder em = new EmbedBuilder();
            em.Color = Discord.Color.Blue;
            em.Title = definition.Word;
            string def = definition.Definition;
            if (def.Length > 1024)
            {
                def = $"{def.Substring(0, 1021)}...";
            }
            em.AddField("Definition:", def);
            string exp = definition.Example;
            if (exp.Length > 1024)
            {
                exp = $"{exp.Substring(0, 1021)}...";
            }
            em.AddField("Example:", exp);
            EmbedFooterBuilder emfb = new EmbedFooterBuilder();
            emfb.Text = $"By {definition.Author}";
            em.Footer = emfb;
            em.Url = definition.Permalink;
            return em.Build();
        }

        static Embed genYtComment(CommentSnippet comment)
        {
            EmbedBuilder em = new Discord.EmbedBuilder();
            em.Color = Discord.Color.Blue;
            EmbedAuthorBuilder eab = new EmbedAuthorBuilder();
            eab.Url = comment.AuthorChannelUrl;
            eab.Name = comment.AuthorDisplayName;
            eab.IconUrl = comment.AuthorProfileImageUrl;
            em.Author = eab;
            EmbedFooterBuilder emfb = new EmbedFooterBuilder();
            emfb.Text = $"{comment.LikeCount} likes";
            em.Footer = emfb;
            em.Description = comment.TextDisplay;
            return em.Build();
        }

        static Embed genTwitter(ITweet tweet)
        {
            EmbedBuilder em = new Discord.EmbedBuilder();
            em.Color = Discord.Color.Blue;
            EmbedAuthorBuilder eab = new EmbedAuthorBuilder();
            eab.Url = $"https://twitter.com/{tweet.CreatedBy.ScreenName}";
            eab.Name = $"{tweet.CreatedBy.Name} (@{tweet.CreatedBy.ScreenName})"; ;
            eab.IconUrl = tweet.CreatedBy.ProfileImageUrlFullSize;
            em.Author = eab;
            EmbedFooterBuilder emfb = new EmbedFooterBuilder();
            emfb.Text = $"{tweet.RetweetCount} Retweets, {tweet.FavoriteCount} Likes";
            em.Footer = emfb;
            em.Description = tweet.FullText;
            em.Timestamp = new DateTimeOffset(tweet.CreatedAt);
            return em.Build();
        }
    }
}
