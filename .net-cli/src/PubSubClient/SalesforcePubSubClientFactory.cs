using System;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using SalesforcePubSubExample.Auth;
using SalesforcePubSubExample.Config;

namespace SalesforcePubSubExample.PubSubClient
{
  public class SalesforcePubSubClientFactory
  {
    private Authenticator _authenticator;
    private Settings _settings;

    public SalesforcePubSubClientFactory(Authenticator authenticator, Settings settings)
    {
      _authenticator = authenticator;
      _settings = settings;
    }

    public async Task<SalesforcePubSubClient> GetPubSubClient()
    {
      SalesforcePubSubClient client;

      // get access token
      var accessToken = await _authenticator.GetToken();

      // create pub sub client
      try
      {
        var metadata = new Metadata()
        {
            {"accesstoken", accessToken},
            { "instanceurl", _settings.InstanceUrl},
            { "tenantid", _settings.OrganizationId}
        };

        client = new SalesforcePubSubClient(_settings.PubSubApiUrl, metadata);
      }
      catch (Exception e)
      {
        throw new Exception("Error creating pub sub client: " + e.Message);
      }

      if (client == null)
      {
        throw new Exception("Error creating pub sub client");
      }

      return client;
    }
  }
}

