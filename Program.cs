using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace LiveSocketExample
{
    class Program
    {
        static void Main(string[] args)
        {
            Task task = LiveTest();
            task.Wait();
        }

        static async Task LiveTest()
        {
            string subscriptionKey = "b97076b5-faf4-4c66-8123-1deb12db9817";
            string project = "Test_Project";
            string session = "meeting1";
            string speaker = "speaker1";

            // Build the url to the live websocket.
            string url = string.Format(
                "ws://dev.phebi.ai/sockets/live/{0}/{1}/{2}",
                project,
                session,
                speaker
            );

            // Create a new websocket client.
            ClientWebSocket socket = new ClientWebSocket();

            // Add the phebi subscription key to the request headers.
            socket.Options.SetRequestHeader("Subscription", subscriptionKey);

            // Connect to the websocket.
            await socket.ConnectAsync(new Uri(url), CancellationToken.None);

            Task task = OnResponse(socket);

            // Read the sample wave file.
            byte[] buffer = File.ReadAllBytes("test.wav");

            int i = 0;
            Stopwatch stopwatch = new Stopwatch();
            while (i < buffer.Length)
            {
                stopwatch.Reset();
                stopwatch.Start();
                // Send 8kb (250ms) of the buffer to the websocket.
                await socket.SendAsync(
                    buffer[i..Math.Min(i + 8000, buffer.Length)],
                    WebSocketMessageType.Binary,
                    false,
                    CancellationToken.None
                );
                stopwatch.Stop();

                i += 8000;

                // Wait 250ms.
                Task.Delay(Math.Max(0, 250 - (int)stopwatch.ElapsedMilliseconds)).Wait();
            }

            // Send EOF message.
            await socket.SendAsync(
                new byte[] { 1 },
                WebSocketMessageType.Binary,
                false,
                CancellationToken.None
            );

            task.Wait();

            socket.Dispose();
        }

        static async Task OnResponse(ClientWebSocket socket)
        {
            // Create a new 10kb buffer.
            byte[] buffer = new byte[10240];

            WebSocketReceiveResult result;
            string responseString;
            PhebiResponse response;

            // Recieve while the socket is open.
            while (socket.State == WebSocketState.Open)
            {
                try
                {
                    // Recieve a response from the phebi websocket.
                    result = await socket.ReceiveAsync(buffer, CancellationToken.None);
                }
                catch { continue; }

                // Decode the utf-8 response.
                responseString = System.Text.Encoding.UTF8.GetString(buffer, 0, result.Count);

                Console.WriteLine(responseString);

                // Deserialize the response string.
                response = JsonConvert.DeserializeObject<PhebiResponse>(responseString);
            }
        }
    }

    public class PhebiResponse
    {
        /// <summary>
        /// Gets or sets the current interim transcription.
        /// </summary>
        public string InterimTranscription { get; set; }

        /// <summary>
        /// Gets or sets the current emotions captured.
        /// </summary>
        public Emotion[] Emotions { get; set; }

        /// <summary>
        /// Gets or sets the final transcription captured.
        /// </summary>
        public Transcription[] Transcription { get; set; }

        /// <summary>
        /// Gets or sets any error encountered.
        /// </summary>
        public string Error { get; set; }
    }

    public class Emotion
    {
        /// <summary>
        /// Gets or sets the confidence level for the emotion detection.
        /// To be multipled with each score.
        /// </summary>
        public double Confidence { get; set; }

        /// <summary>
        /// Gets or sets the emotion scores in the order of strong, calm, anxious, happy, sad.
        /// To be multipled with the confidence level.
        /// </summary>
        public double[] Scores { get; set; }
    }

    public class Transcription
    {
        /// <summary>
        /// Gets or sets the position of the transcribed text in seconds.
        /// </summary>
        public double Position { get; set; }

        /// <summary>
        /// Gets or sets the duration of the transcribed text in seconds.
        /// </summary>
        public double Duration { get; set; }

        /// <summary>
        /// Gets or sets the transcribed text.
        /// </summary>
        public string Text { get; set; }
    }
}
