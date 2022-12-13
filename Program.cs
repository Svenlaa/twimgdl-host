using System.Net;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

const string mediaId = "";
const string mediaFormat = "jpg";
WebClient webClient = new WebClient();

app.MapGet("/do", () =>
{
    var dateTime = DateTime.Now;
    webClient.DownloadFileAsync(new Uri($"https://pbs.twimg.com/media/{mediaId}?format={mediaFormat}"), $"D:\\temp\\{dateTime:hhmmss}.png");
    
    return "最高";
});

app.Run();