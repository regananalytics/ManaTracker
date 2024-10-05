using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ManaSpline
{
    public class PlaybackFileReader
    {
        private readonly string _filePath;

        // Constants for record keys
        private const string IGT_KEY = "IGT";
        private const int PLAY_AHEAD_BUFFER_SECONDS = 10;
        private const int QUEUE_LOOKAHEAD_SECONDS = 10;
        private const int QUEUE_LOOKAHEAD_DELAY_MS = 100;

        // RecorderPlayers
        public static KeyRecorderPlayer keyRecorderPlayer => ManaSpline.keyRecorderPlayer;
        public static MouseRecorderPlayer mouseRecorderPlayer => ManaSpline.mouseRecorderPlayer;
        public static StateRecorderPlayer stateRecorderPlayer => ManaSpline.stateRecorderPlayer;

        private readonly ConcurrentQueue<(float igt, dynamic playJson)> _queue = new ConcurrentQueue<(float igt, dynamic playJson)>();
        private CancellationToken _token;

        public float GetIGT() => ManaSpline.GetIGT();

        public PlaybackFileReader(string filePath)
        {
            _filePath = filePath;
            ManaSpline.AppendLog($"Loading playback file: {_filePath}");
            ProcessFile();
        }

        public void Start(CancellationToken token)
        {
            _token = token;
            ManaSpline.AppendLog("Start Playback Tasks");
            Task.Run(() => ProcessPlayQueue());
        }

        public void ProcessFile()
        {
            ManaSpline.AppendLog($"Enqueing playback file: {_filePath}");

            using (var reader = File.OpenText(_filePath))
            {
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    var playJson = JsonConvert.DeserializeObject<dynamic>(line);
                    var nextIGT = (float)playJson.IGT;

                    _queue.Enqueue((nextIGT, playJson));
                }
            }
            ManaSpline.AppendLog("Reached end of Playback File.");
        }

        public async Task ProcessPlayQueue()
        {
            while (!_token.IsCancellationRequested)
            {
                int nQueued = 0;

                if (_queue.TryPeek(out var nextPlay) && nextPlay.igt <= GetIGT() + QUEUE_LOOKAHEAD_SECONDS)
                {
                    // Dequeue the next play if it fits within the queue lookahead window
                    if (_queue.TryDequeue(out var play))
                    {
                        var nextIGT = play.igt;
                        var json = play.playJson;

                        if (nextIGT < GetIGT())
                            continue;

                        try
                        {
                            // Read the record and dispatch to the appropriate deserializer and handler
                            if (json.ContainsKey(KeyRecorderPlayer.KEY))
                            {
                                var keyPressRecord = keyRecorderPlayer.DeserializeRecord(json[KeyRecorderPlayer.KEY]);
                                keyRecorderPlayer.QueuePlay(keyPressRecord);
                            }
                            
                            if (json.ContainsKey(MouseRecorderPlayer.KEY))
                            {
                                var mousePoint = mouseRecorderPlayer.DeserializeRecord(json[MouseRecorderPlayer.KEY]);
                                mouseRecorderPlayer.QueuePlay(mousePoint);
                            }

                            if (json.ContainsKey(StateRecorderPlayer.KEY))
                            {
                                var stateRecord = stateRecorderPlayer.DeserializeRecord(json[StateRecorderPlayer.KEY]);
                                mouseRecorderPlayer.QueuePlay(stateRecord);
                            }
                        }
                        catch (Exception ex)
                        {
                            ManaSpline.AppendLog($"Error processing line: {ex.Message}");
                            break;
                        }
                        nQueued++;
                    }
                }
                else
                {
                    // Wait for a short time before checking the queue again
                    await Task.Delay(QUEUE_LOOKAHEAD_DELAY_MS, _token);
                }
            }
        }
    }
}