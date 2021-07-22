# Documentation of the Phebi-Live WebSocket Endpoint
The Phebi-Live WebSocket Endpoint is to enable 3rd party Applications access to Phebi's realtime emotion analysis and speech-to-text transcription.

The Phebi-Live does not support speaker diarization, it is highly recommended to have a separate socket and audio stream per speaker to ensure capturing the correct emotions for respondents.

## 1. The socket endpoint

*The url to the Phebi-Live socket consists of
*The host, your own environment and analysis portal (provided to you by Phebi)
*The project name, where the live session should be saved to
*The session (appears as respondent-id in the analysis portal)
*The speaker (respondent, moderator etc.)


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
