using Gma.System.MouseKeyHook;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsInput;
using WindowsInput.Native;

namespace ManaSpline
{
    public class KeyRecorderPlayer : RecorderPlayerBase<KeyPressRecord>
    {
        public const string KEY = "KEY";
        private readonly RecordingFileWriter _fileWriter;
        private readonly IKeyboardMouseEvents _globalHook = Hook.GlobalEvents();
        private InputSimulator _inputSimulator = new InputSimulator();

        private Dictionary<string, float> keyDownTimes = new Dictionary<string, float>();
        private float _lastTime = 0;


        public KeyRecorderPlayer(CancellationToken token)
        {
            _token = token;
            _fileWriter = ManaSpline.FileWriter;
            if (ManaSpline.Verbose)
                ManaSpline.AppendLog("[SETUP] Created KeyRecorderPlayer");
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            var igt = GetIGT();
            var key = e.KeyCode.ToString();
            try
            {
                if (IsRunning && !IsPaused)
                    keyDownTimes.TryAdd(key, igt);
            }
            catch (Exception ex)
            {
                ManaSpline.AppendLog($"Error on KeyDown: {ex.Message}");
            }
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            float downTime;
            var igt = GetIGT();
            var key = e.KeyCode.ToString();
            try
            {
                if (IsRunning && !IsPaused)
                    if (keyDownTimes.Remove(key, out downTime))
                        BuildRecord(key, downTime, igt);
                    else
                        ManaSpline.AppendLog($"KeyDown wasn't captured for {key}");
            }
            catch (Exception ex)
            {
                ManaSpline.AppendLog($"Error on KeyUp: {ex.Message}");
            }
        }

        private void BuildRecord(string key, float igtDownTime, float igtUpTime)
        {
            // Calculate times
            float duration = (float)(igtUpTime - igtDownTime) * 1000.0f; // ms

            if (igtUpTime > _lastTime)
            {
                var record = new KeyPressRecord
                {
                    IGT = igtDownTime,
                    Key = key,
                    Duration = duration
                };

                Task.Run(() => RecordAsync(record));
            }
            _lastTime = igtUpTime;
        }

        private async Task SimulateKeyPress(KeyPressRecord record)
        {
            // Simulate key press and release with InputSimulator
            VirtualKeyCode keyCode = (VirtualKeyCode)Enum.Parse(typeof(VirtualKeyCode), "VK_" + record.Key.ToUpper());

            int delayMS = Math.Max((int)((record.IGT - GetIGT()) * 1000.0f), 0);
            await Task.Delay((int)delayMS);
            _inputSimulator.Keyboard.KeyDown(keyCode);
            await Task.Delay((int)record.Duration);
            _inputSimulator.Keyboard.KeyUp(keyCode);

            if (ManaSpline.Verbose)
                ManaSpline.AppendLog($"Simulated key press: {record.Key} for duration: {record.Duration}");
        }

        public override async Task RecordAsync(KeyPressRecord record)
        {
            if (IsRunning && !IsPaused)
            {
                var json = SerializeRecord(record);
                await _fileWriter.EnqueueRecordAsync(json);
            }
        }
        
        // Playback a key press asynchronously
        public override async Task PlaybackAsync()
        {
            while (_playQueue.IsEmpty)
                await Task.Delay(10, _token);
            
            ManaSpline.AppendLog($"PlaybackAsync");

            while (!_token.IsCancellationRequested)
            {
                // Plan the next window
                if (_playQueue.TryDequeue(out var play))
                {
                    try
                    {
                        float startIGT = GetIGT();
                        float keyDownIGT = play.IGT;

                        if (startIGT > keyDownIGT)
                            continue;

                        // Delay until first move
                        while ((keyDownIGT - GetIGT()) * 1000.0f >= 250)
                        {
                            await Task.Delay(100, _token);
                        }
                        var delayMs = Math.Max((int)((keyDownIGT - GetIGT()) * 1000.0f), 0);
                        await Task.Delay(delayMs, _token);

                        // Execute first move
                        Task.Run(() => SimulateKeyPress(play));
                    }
                    catch (Exception ex)
                    {
                        ManaSpline.AppendLog($"Error in Executor: {ex.Message}");
                    }
                }
            }
        }

        public override string SerializeRecord(KeyPressRecord record)
        {
            return JsonConvert.SerializeObject(new
            {
                IGT = record.IGT,
                KEY = new
                {
                    IGT = record.IGT,
                    Key = record.Key,
                    Duration = record.Duration
                }
            });
        }

        public override KeyPressRecord DeserializeRecord(dynamic json)
        {
            return new KeyPressRecord
            {
                IGT = (float)json.IGT,
                Key = (string)json.Key,
                Duration = (float)json.Duration
            };
        }

        public override void Start()
        {
            _globalHook.KeyDown += OnKeyDown;
            _globalHook.KeyUp += OnKeyUp;

            base.Start();
        }

        public override void Play()
        {
            Task.Run(() => PlaybackAsync());
        }

        public override void Stop()
        {
            _globalHook.KeyDown -= OnKeyDown;
            _globalHook.KeyUp -= OnKeyUp;

            base.Stop();
        }
    }

    public struct KeyPressRecord
    {
        public float IGT { get; set; }
        public string Key {get; set; }
        public float Duration { get; set; }
    }
}