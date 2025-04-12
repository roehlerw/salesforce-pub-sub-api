using System;

namespace SalesforcePubSubExample.Config
{
  public class Settings
  {
    public string ChannelName { get; set; }
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
    public string InstanceUrl { get; set; }
    public string OrganizationId { get; set; }
    public string PubSubApiUrl { get; set; }
    public string RefreshToken { get; set; }

    public void Validate()
    {
      if (string.IsNullOrEmpty(ChannelName))
      {
        throw new Exception("the ChannelName property must be set");
      }

      if (string.IsNullOrEmpty(ClientId))
      {
        throw new Exception("the ClientId property must be set");
      }

      if (string.IsNullOrEmpty(ClientSecret))
      {
        throw new Exception("the ClientSecret property must be set");
      }

      if (string.IsNullOrEmpty(InstanceUrl))
      {
        throw new Exception("the InstanceUrl property must be set");
      }

      if (string.IsNullOrEmpty(OrganizationId))
      {
        throw new Exception("the OrganizationId property must be set");
      }

      if (string.IsNullOrEmpty(PubSubApiUrl))
      {
        throw new Exception("the PubSubApiUrl property must be set");
      }

      if (string.IsNullOrEmpty(RefreshToken))
      {
        throw new Exception("the RefreshToken property must be set");
      }
    }
  }
}