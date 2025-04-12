using System;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;
using Salesforce.PubSubApi;
using SolTechnology.Avro;

namespace SalesforcePubSubExample.PubSubClient
{
  public class SalesforcePubSubClient
  {
    private readonly PubSub.PubSubClient _client;
    private readonly Metadata _metadata;

    public SalesforcePubSubClient(string address, Metadata metadata)
    {
      var channelSalesforce = GrpcChannel.ForAddress(address);
      _client = new PubSub.PubSubClient(channelSalesforce);
      _metadata = metadata;
    }

    public void SubscribeToChannel(string channelName, CancellationTokenSource cts)
    {
      // get schema and topic info
      var topicName = channelName;
      if (!topicName.StartsWith("/data/"))
      {
        topicName = $"/data/{channelName}";
      }

      var topicInfo = GetTopicByName(topicName);
      var schemaInfo = GetSchemaById(topicInfo.SchemaId);

      // subscribe to topic
      Task.Run(async () => await Subscribe(topicInfo.TopicName, schemaInfo.SchemaJson, cts), cts.Token);
    }

    private TopicInfo GetTopicByName(string topicName)
    {
      Console.WriteLine($"Getting topic {topicName}");

      TopicRequest topicRequest = new TopicRequest
      {
        TopicName = topicName
      };

      return _client.GetTopic(request: topicRequest, headers: _metadata);
    }

    private SchemaInfo GetSchemaById(string schemaId)
    {
      Console.WriteLine($"Getting schema for {schemaId}");

      SchemaRequest schemaRequest = new SchemaRequest
      {
        SchemaId = schemaId
      };

      return _client.GetSchema(request: schemaRequest, headers: _metadata);
    }

    private async Task Subscribe(string topicName, string jsonSchema, CancellationTokenSource cts)
    {
      try
      {
        Console.WriteLine($"Subscribing to topic {topicName}");

        using AsyncDuplexStreamingCall<FetchRequest, FetchResponse> stream = _client.Subscribe(headers: _metadata, cancellationToken: cts.Token);

        FetchRequest fetchRequest = new FetchRequest
        {
          TopicName = topicName,
          ReplayPreset = ReplayPreset.Latest,
          NumRequested = 10
        };

        await WriteToStream(stream.RequestStream, fetchRequest);

        await ReadFromStream(stream.ResponseStream, jsonSchema);

      }
      catch (RpcException e) when (e.StatusCode == StatusCode.Cancelled)
      {
        Console.WriteLine($"Operation Cancelled: {e.Message} Source {e.Source} {e.StackTrace}");
        throw;
      }
    }

    private async Task WriteToStream(IClientStreamWriter<FetchRequest> requestStream, FetchRequest fetchRequest)
    {
      await requestStream.WriteAsync(fetchRequest);
    }

    private async Task ReadFromStream(IAsyncStreamReader<FetchResponse> responseStream, string jsonSchema)
    {
      while (await responseStream.MoveNext())
      {
        Console.WriteLine($"RPC ID: {responseStream.Current.RpcId}");

        if (responseStream.Current.Events != null)
        {
          Console.WriteLine($"Number of events received: {responseStream.Current.Events.Count}");
          foreach (var item in responseStream.Current.Events)
          {

            byte[] bytePayload = item.Event.Payload.ToByteArray();
            string jsonPayload = AvroConvert.Avro2Json(bytePayload, jsonSchema);
            Console.WriteLine($"response: {jsonPayload}");
          }
        }
        else
        {
          Console.WriteLine($"Subscription is active");
        }
      }
    }
  }
}

