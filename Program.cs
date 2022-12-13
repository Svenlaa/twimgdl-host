using System.Net;
using TwitterSharp.Client;
using TwitterSharp.Request.AdvancedSearch;
using TwitterSharp.Request.Option;
using twimgdl_host;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

const string mediaFormat = "jpg";
WebClient webClient = new WebClient();
TwitterClient twitterClient = new TwitterClient(Secrets.TwitterBearer);

void DownloadFile(string mediaId) {
    var dateTime = DateTime.Now;
    webClient.DownloadFileAsync(new Uri($"https://pbs.twimg.com/media/{mediaId}.{mediaFormat}"),
    $"D:\\temp\\{dateTime:hhmmss}.{mediaFormat}");
}

async Task<MyTweet?> GetMedia(string tweetId)
{
    var tweet = await twitterClient.GetTweetAsync(tweetId, new TweetSearchOptions
    {
        TweetOptions = new [] { TweetOption.Attachments, TweetOption.Created_At },
        MediaOptions = new [] { MediaOption.Url }
    });
    if (tweet is null) return null;
    var media = tweet.Attachments.Media;
    var mediaFiles = new List<string>();
    
    foreach (var medium in media)
    {
        var currentUrl = medium.Url;
        if (currentUrl is null) continue;
        mediaFiles.Add(currentUrl.Split("/").Last());
    }

    return mediaFiles.Count != 0 ? new MyTweet(DateTime.Now, mediaFiles) : null;
}

app.MapGet("/downloadFile/{mediaId}", (string mediaId) =>
{
    DownloadFile(mediaId);
    return "Nice";
});

app.MapGet("/tweet/{tweetId}", async (string tweetId) =>
{
    var task = GetMedia(tweetId);
    var res = await task;
    if (res is null) return "No Images Found.";
    
    Console.WriteLine(res.CreatedAt);
    return "Success";
});


app.Run();

internal record MyTweet(DateTime CreatedAt, List<string> MediaFiles);
