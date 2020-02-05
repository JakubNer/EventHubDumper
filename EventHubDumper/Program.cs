using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.EventHubs.Processor;

namespace EventHubDumper
{
    class Program
    {
        public class Options
        {
            [Option("event-hub-connection-string", Required = true, HelpText = "Required EventHub connection string in the format \"Endpoint=sb://?.servicebus.windows.net/;SharedAccessKeyName=?;SharedAccessKey=?=;EntityPath=?.  Get this from Azure Portal.")]
            public string EventHubConnectionString { get; set; }

            [Option("event-hub-name", Required = true, HelpText = "Required EventHub name.  Get this from Azure Portal.")]
            public string EventHubName { get; set; }

            [Option("storage-connection-string", Required = true, HelpText = "Required EventHub connection string in the format \"DefaultEndpointsProtocol=https;AccountName=?;AccountKey=?.")]
            public string StorageConnectionString { get; set; }

            [Option("storage-container-name", Required = true, HelpText = "Required storage container name.  Get this from Azure Portal.")]
            public string StorageContainerName { get; set; }

            [Option("consumer-group", Required = false, HelpText = "Optional consumer group.  Get this from Azure Portal.")]
            public string ConsumerGroup { get; set; }
        }

        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                   .MapResult((o) =>
                   {
                       Run(o.EventHubConnectionString, 
                           o.EventHubName,
                           o.StorageConnectionString,
                           o.StorageContainerName,
                           o.ConsumerGroup).GetAwaiter().GetResult();
                       return 0;
                   }, (errs) => HandleParseError(errs));
        }

        //in case of errors or --help or --version
        static int HandleParseError(IEnumerable<Error> errs)
        {
            var result = -2;
            Console.WriteLine("errors {0}", errs.Count());
            if (errs.Any(x => x is HelpRequestedError || x is VersionRequestedError))
                result = -1;
            Console.WriteLine("Exit code {0}", result);
            return result;
        }

        static async Task<int> Run(string eventHubConnectionString, string eventHubName, string storageConnectionString, string storageContainerName, string consumerGroup)
        {
            Console.WriteLine("Running...");
            Console.WriteLine($"Connecting to {eventHubConnectionString} with {storageConnectionString}");
            Console.WriteLine($"Consumer group is:  {consumerGroup}");
            EventProcessorHost eventProcessorHost = new EventProcessorHost(
                eventHubName,
                consumerGroup == null ? PartitionReceiver.DefaultConsumerGroupName : consumerGroup, 
                eventHubConnectionString, 
                storageConnectionString,
                storageContainerName);

            // Registers the Event Processor Host and starts receiving messages
            await eventProcessorHost.RegisterEventProcessorAsync<SimpleEventProcessor>(
                new EventProcessorOptions 
                { 
                    InitialOffsetProvider = partitionId => EventPosition.FromEnqueuedTime(DateTime.UtcNow) 
                });

            Console.WriteLine("Receiving. Press ENTER to stop worker.");
            Console.ReadLine();

            // Disposes of the Event Processor Host
            await eventProcessorHost.UnregisterEventProcessorAsync();

            return 0;
        }

        public class SimpleEventProcessor : IEventProcessor
        {
            public Task CloseAsync(PartitionContext context, CloseReason reason)
            {
                Console.WriteLine($"Processor Shutting Down. Partition '{context.PartitionId}', Reason: '{reason}'.");
                return Task.CompletedTask;
            }

            public Task OpenAsync(PartitionContext context)
            {
                Console.WriteLine($"SimpleEventProcessor initialized. Partition: '{context.PartitionId}'");
                return Task.CompletedTask;
            }

            public Task ProcessErrorAsync(PartitionContext context, Exception error)
            {
                Console.WriteLine($"Error on Partition: {context.PartitionId}, Error: {error.Message}");
                return Task.CompletedTask;
            }

            public Task ProcessEventsAsync(PartitionContext context, IEnumerable<EventData> messages)
            {
                foreach (var eventData in messages)
                {
                    var data = Encoding.UTF8.GetString(eventData.Body.Array, eventData.Body.Offset, eventData.Body.Count);
                    Console.WriteLine($"Message received. Partition: '{context.PartitionId}', Data: '{data}'");
                }

                return context.CheckpointAsync();
            }
        }
    }
}
