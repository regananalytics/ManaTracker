using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ManaSpline
{
    public class StateRecorderPlayer : RecorderPlayerBase<Dictionary<string, object>>
    {
        public const string KEY = "STATE";
        private readonly RecordingFileWriter _fileWriter;
        private readonly MemManager _memManager;
        private int _interval;

        public StateRecorderPlayer(CancellationToken token)
        {
            _token = token;
            _memManager = ManaSpline.memManager;
            _fileWriter = ManaSpline.FileWriter;
            _interval = ManaSpline.StateInterval;

            if (ManaSpline.Verbose)
                ManaSpline.AppendLog("[SETUP] StateRecordPlayer created.");
        }

        private async Task StartRecording()
        {
            float lastRecordedTime = 0;

            while (!_token.IsCancellationRequested)
            {
                if (IsRunning && !IsPaused)
                {
                    var state = _memManager.IsConnected ? _memManager.GetGameState() : null;
                    if (state != null && state.ContainsKey("IGT"))
                    {
                        float currentTime = (float)state["IGT"];

                        if (currentTime > lastRecordedTime)
                        {
                            await RecordAsync(state);
                            lastRecordedTime = currentTime;
                        }

                        await Task.Delay(_interval, _token);
                    }
                }
            }
        }

        public override async Task RecordAsync(Dictionary<string, object> state)
        {
            if (IsRunning && !IsPaused)
            {
                var json = SerializeRecord(state);
                await _fileWriter.EnqueueRecordAsync(json);
            }
        }

        public override async Task PlaybackAsync()
        {
            // Logic for playback
            await Task.CompletedTask;
        }

        public override string SerializeRecord(Dictionary<string, object> record)
        {
            return JsonConvert.SerializeObject(new
            {
                IGT = record.ContainsKey("IGT") ? record["IGT"] : 0,
                STATE = record
            });
        }

        public override Dictionary<string, object> DeserializeRecord(dynamic json)
        {
            return JsonConvert.DeserializeObject<Dictionary<string, object>>(stateData.ToString());
        }

        public override void Start()
        {
            base.Start();
            Task.Run(() => StartRecording());
        }

        public override void Play()
        {
            Task.Run(() => PlaybackAsync());
        }
    }
}