using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using SalesforcePubSubExample.Auth;
using SalesforcePubSubExample.Config;
using SalesforcePubSubExample.PubSubClient;

namespace SalesforcePubSubExample;

class Program
{
  static async Task Main(string[] args)
  {
    // get configuration from appsettings.json
    var builder = new ConfigurationBuilder();
    builder.SetBasePath(Directory.GetCurrentDirectory())
          .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

    IConfiguration config = builder.Build();

    var settings = new Settings()
    {
      ChannelName = config["ChannelName"],
      ClientId = config["ClientId"],
      ClientSecret = config["ClientSecret"],
      InstanceUrl = config["InstanceUrl"],
      OrganizationId = config["OrganizationId"],
      RefreshToken = config["RefreshToken"],
      PubSubApiUrl = config["PubSubApiUrl"]
    };
    settings.Validate();

    // create authentication helper
    var authenticator = new Authenticator(settings, new System.Net.Http.HttpClient());

    // create pub sub client factory and client
    var pubSubClientFactory = new SalesforcePubSubClientFactory(authenticator, settings);
    var pubSubClient = await pubSubClientFactory.GetPubSubClient();

    // force the client to reconnect after 30 minutes
    var cts = new CancellationTokenSource();
    cts.CancelAfter(1800 * 1000);

    while (true)
    {
      try
      {
        pubSubClient.SubscribeToChannel("", cts);
      }
      catch (TaskCanceledException)
      {
        // recreate pub sub client every 30 minutes
        pubSubClient = await pubSubClientFactory.GetPubSubClient();
        cts = new CancellationTokenSource();
        cts.CancelAfter(1800 * 1000);
        pubSubClient.SubscribeToChannel("", cts);
      }
      catch (Exception e)
      {
        Console.WriteLine(e.Message);
        throw;
      }
      finally
      {
        cts.Cancel();
      }
    }
  }
}