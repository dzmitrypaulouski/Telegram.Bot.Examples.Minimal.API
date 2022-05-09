using Telegram.Bot;
using Telegram.Bot.Examples.Minimal.API;
using Telegram.Bot.Examples.Minimal.API.Services;

var builder = WebApplication.CreateBuilder(args);

var botConfig = builder.Configuration.GetSection("BotConfiguration").Get<BotConfiguration>();
var botToken = botConfig.BotToken ?? string.Empty;

// There are several strategies for completing asynchronous tasks during startup.
// Some of them could be found in this article https://andrewlock.net/running-async-tasks-on-app-startup-in-asp-net-core-part-1/
// We are going to use IHostedService to add and later remove Webhook
builder.Services.AddHostedService<ConfigureWebhook>();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register named HttpClient to get benefits of IHttpClientFactory
// and consume it with ITelegramBotClient typed client.
// More read:
//  https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-5.0#typed-clients
//  https://docs.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests
builder.Services.AddHttpClient("TelegramWebhook")
    .AddTypedClient<ITelegramBotClient>(httpClient => new TelegramBotClient(botToken, httpClient));

// Dummy business-logic service
builder.Services.AddScoped<HandleUpdateService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseHttpLogging();

// Configure custom endpoint per Telegram API recommendations:
// https://core.telegram.org/bots/api#setwebhook
// If you'd like to make sure that the Webhook request comes from Telegram, we recommend
// using a secret path in the URL, e.g. https://www.example.com/<token>.
// Since nobody else knows your bot's token, you can be pretty sure it's us.
app.MapPost($"/bot/{botConfig.EscapedBotToken}", async (
    ITelegramBotClient botClient,
    HttpRequest request,
    HandleUpdateService handleUpdateService,
    NewtonsoftJsonUpdate update) =>
{
    if (update.Message == null)
    {
        throw new ArgumentException(nameof(update.Message));
    }

    await handleUpdateService.EchoAsync(update);

    return Results.Ok();
})
.WithName("TelegramWebhook");

app.Run();