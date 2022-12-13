using System.Net;
using System.Runtime.InteropServices;
using TwitterSharp.Client;
using TwitterSharp.Request.AdvancedSearch;
using TwitterSharp.Request.Option;
using twimgdl_host;
using TwitterSharp.Response.RMedia;

var app = WebApplication.CreateBuilder(args).Build();

WebClient webClient = new WebClient();
TwitterClient twitterClient = new TwitterClient(Secrets.TwitterBearer);


void SaveFile(string fileName, DateTime createdAt)
{
    var filePath = $"{Directory.GetCurrentDirectory()}\\{fileName}";
    webClient.DownloadFile(new Uri($"https://pbs.twimg.com/media/{fileName}"), filePath);
    File.SetLastWriteTime(filePath, createdAt);
}

async Task<MyTweet?> GetMedia(string tweetId, [Optional] int? position)
{
    var tweet = await twitterClient.GetTweetAsync(tweetId, new TweetSearchOptions
    {
        TweetOptions = new [] { TweetOption.Attachments, TweetOption.Created_At },
        MediaOptions = new [] { MediaOption.Url }
    });
    if (tweet is null) return null;
    var media = tweet.Attachments.Media;
    var mediaFiles = new List<string>();

    void AddFileToArray(Media medi)
    {
        var currentUrl = medi.Url;
        if (currentUrl is null) return;
        mediaFiles.Add(currentUrl.Split("/").Last());
    }

    if (position.HasValue)
    {
        AddFileToArray(media[position.Value]);
    }
    else
    {
        foreach (var medium in media)
        {
            AddFileToArray(medium);
        }
    }
    
    return mediaFiles.Count != 0 ? new MyTweet(CreatedAt: tweet.CreatedAt ?? DateTime.UnixEpoch, mediaFiles) : null;
}

app.MapGet("/tweet/{tweetId}", async (string tweetId) =>
{
    var res = await GetMedia(tweetId);
    if (res is null) return "No Images Found.";

    var amountDownloaded = 0;
    foreach (string mediaFile in res.MediaFiles)
    {
        SaveFile(mediaFile, createdAt: res.CreatedAt);
        amountDownloaded += 1;
    }
    
    return $"{amountDownloaded} files downloaded.";
});

app.MapGet("/tweet/{tweetId}/{position}", async (string tweetId, int position) =>
{
    var res = await GetMedia(tweetId);
    if (res is null || position > res.MediaFiles.Count) return "Image Not found";
    
    SaveFile(res.MediaFiles[position], createdAt: res.CreatedAt);
    
    return "Image downloaded.";
});

app.Run();

internal record MyTweet(DateTime CreatedAt, List<string> MediaFiles);
