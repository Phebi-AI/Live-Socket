# Phebi-Live WebSocket Endpoint (C#)
The Phebi-Live WebSocket Endpoint is to enable 3rd party Applications access to Phebi's realtime emotion analysis and speech-to-text transcription.

Phebi-Live does not support speaker diarization, it is highly recommended to have a separate socket and audio stream per speaker to ensure capturing the correct emotions for respondents.

## 1. The socket endpoint

The url to the Phebi-Live socket consists of

* The host, your own environment and analysis portal (provided to you by Phebi)
* The project name, where the live session should be saved to
* The session (appears as respondent-id in the analysis portal)
* The speaker (respondent, moderator etc.)


```
string subscriptionKey = "6d210acbaa9e49e696c8c747a7e0ed26";
string project = "Test_Project";
string session = "meeting1";
string speaker = "speaker1";

// Build the url to the live websocket.
string url = string.Format(
    "wss://dev.phebi.ai/sockets/live/{0}/{1}/{2}",
    project,
    session,
    speaker
);
```

## 2. Establish a socket connection

To establish a connection to the Phebi-Live WebSocket you have to provide the subscription key 'Subscription' in the request headers.

```
// Create a new websocket client.
ClientWebSocket socket = new ClientWebSocket();

// Add the phebi subscription key to the request headers.
socket.Options.SetRequestHeader("Subscription", subscriptionKey);

// Connect to the websocket.
await socket.ConnectAsync(new Uri(url), CancellationToken.None);
```

## 3. Receiving data from the socket

```
private async Task OnResponse(ClientWebSocket socket)
{
    // Create a new 10kb buffer.
    byte[] buffer = new byte[10240];

    WebSocketReceiveResult result;
    string responseString;
    PhebiResponse response;

    // Recieve while the socket is open.
    while (socket.State == WebSocketState.Open)
    {
        // Recieve a response from the phebi websocket.
        result = await socket.ReceiveAsync(buffer, CancellationToken.None);

        // Decode the utf-8 response.
        responseString = System.Text.Encoding.UTF8.GetString(buffer, 0, result.Count);

        Console.WriteLine(responseString);

        // Deserialize the response string.
        response = JsonConvert.DeserializeObject<PhebiResponse>(responseString);
    }
}
```

A response would look like this:
```
{ 
    "Transcription": [
        {
            "Position": 0.1,
            "Duration": 1.13,
            "Text": "This",
            "Speaker": 0
        }
    ],
    "InterimTranscription":"this is a number 15,476,521", 
    "Emotions":[
        {
            "Confidence": 0.6080625666666667,
            "Scores": [0.78,0.0,0.0,0.22,0.0],
            "Speaker": 0,
            "Position": 0
        }
    ]
}
```

## 4. Sending audio data to the socket

The Phebi-Live WebSocket requires wave RIFF audio data with 16k sample rate, 16 bits per sample and 1 audio channel.

```
// Read the sample wave file.
byte[] buffer = File.ReadAllBytes("test.wav");

int i = 0;
while (i < buffer.Length)
{
    // Send 10kb of the buffer to the websocket.
    await socket.SendAsync(
        buffer[i..Math.Min(i + 10240, buffer.Length)],
        WebSocketMessageType.Binary,
        false,
        CancellationToken.None
    );

    i += 10240;

    // Wait 10ms.
    Task.Delay(10).Wait();
}
```

## 5. Ending the transmission

When the Live session has ended, we need to tell Phebi-Live that' we're at the end of the file and we need the last final transcription.
If we close the session now, without sending the EOF message, the final transcription will not be sent to the client.

The EOF message is a single byte with value 1.

```
// Send EOF message.
await socket.SendAsync(
    new byte[] { 1 },
    WebSocketMessageType.Binary,
    false,
    CancellationToken.None
);
```
