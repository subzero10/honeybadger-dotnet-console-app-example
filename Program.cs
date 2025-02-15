using Honeybadger;
using Honeybadger.DotNetCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);
builder.AddHoneybadger();

builder.Logging.AddHoneybadger();
using var host = builder.Build();

var source = new CancellationTokenSource();
var token = source.Token;

// switch  between "manual" and "logger" to test the different ways to notify Honeybadger
// "automatic" is not supported yet in console apps
const string flag = "logger";
switch (flag)
{
    case "manual":
        await NotifyHoneybadgerManually(host.Services);
        await source.CancelAsync();
        break;
    // case "automatic":
        // todo: Unhandled exceptions are only caught in .Net Core web apps (using a middleware).
        //       In a console app, we need to catch and report the exceptions manually.
        // NotifyHoneybadgerAutomatically();
        // break;
    case "logger":
        NotifyHoneybadgerWithLogger(host.Services);
        await source.CancelAsync();
        break;
    default:
        throw new ArgumentOutOfRangeException();
}


await host.RunAsync(token);

return;

static async Task NotifyHoneybadgerManually(IServiceProvider serviceProvider)
{
    var honeybadger = serviceProvider.GetRequiredService<IHoneybadgerClient>();
    await honeybadger.NotifyAsync(new Exception("Something went wrong"));
}

static void NotifyHoneybadgerAutomatically()
{
    throw new Exception("Something went wrong");
}

static void NotifyHoneybadgerWithLogger(IServiceProvider serviceProvider)
{
    var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
    logger.LogError("Something went wrong. This was reported using a logger instance.");
}

