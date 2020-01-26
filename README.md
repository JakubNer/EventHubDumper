# EventHubDumper

Very basic EventHub consumer based on sample code from: https://docs.microsoft.com/en-us/azure/event-hubs/event-hubs-dotnet-standard-getstarted-send#receive-events.

## Usage

On any platform with [.Net Core](https://dotnet.microsoft.com/download/dotnet-core/current/runtime) installed: `dotnet distro/EventHubDumper.dll --event-hub-connection-string ${EHCSTRING} --event-hub-name ${EHNAME} --storage-connection-string ${SCSTRING} --storage-container-name ${SCNAME}`.

On Windows: `distro\EventHubDumper.exe --event-hub-connection-string ${EHCSTRING} --event-hub-name ${EHNAME} --storage-connection-string ${SCSTRING} --storage-container-name ${SCNAME}`

All parameters mandatory:

```
  --event-hub-connection-string    Required. Required EventHub connection string in the format
                                   "Endpoint=sb://?.servicebus.windows.net/;SharedAccessKeyName=?;SharedAccessKey=?=;EntityPath=?.  Get

                                   this from Azure Portal.

  --event-hub-name                 Required. Required EventHub name.  Get this from Azure Portal.

  --storage-connection-string      Required. Required EventHub connection string in the format
                                   "DefaultEndpointsProtocol=https;AccountName=?;AccountKey=?.

  --storage-container-name         Required. Required storage container name.  Get this from Azure Portal.

```
