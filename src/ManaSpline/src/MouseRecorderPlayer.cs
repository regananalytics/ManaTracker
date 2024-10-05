using Gma.System.MouseKeyHook;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsInput;
using WindowsInput.Native;

namespace ManaSpline
{
    public class MouseRecorderPlayer : RecorderPlayerBase<MousePoint>
    {
        public const string KEY = "MOUSE";
        private readonly RecordingFileWriter _fileWriter;
        private readonly IKeyboardMouseEvents _globalHook;
        private InputSimulator _inputSimulator = new InputSimulator();

        private ConcurrentQueue<List<(MousePoint move, float igt)>> _plans = new ConcurrentQueue<List<(MousePoint move, float igt)>>();

        private const float _planDurationMs = 1000;
        private readonly object _lock = new object();


        private float _min_interval = 15;
        private float _lastIgt = 0;

        public MouseRecorderPlayer(CancellationToken token)
        {
            _token = token;
            _fileWriter = ManaSpline.FileWriter;
            _globalHook = Hook.GlobalEvents();

            if (ManaSpline.Verbose)
                ManaSpline.AppendLog("[SETUP] StateRecordPlayer created.");
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            var igt = GetIGT();
            if ((igt - _lastIgt) * 1000.0f > _min_interval)
            {
                _lastIgt = igt;
                Task.Run(() => RecordAsync(new MousePoint{
                    IGT = igt,
                    MouseX = e.X,
                    MouseY = e.Y
                }));
            }
                
        }

        private void SimulateMouseMove(MousePoint play)
        {
            var igt = GetIGT();
            ManaSpline.AppendLog($"{igt} ({igt-play.IGT}): [{play.IGT}] {play.MouseX}, {play.MouseY}");
            _inputSimulator.Mouse.MoveMouseTo(play.MouseX, play.MouseY);
        }

        public async override Task RecordAsync(MousePoint record)
        {
            if (IsRunning && !IsPaused)
            {
                var json = SerializeRecord(record);
                await _fileWriter.EnqueueRecordAsync(json);
            }
        }

        public async override Task PlaybackAsync()
        {
            // ManaSpline.AppendLog($"Mouse Playback started. Waiting for plays...");

            while (_playQueue.IsEmpty)
                await Task.Delay(10, _token);

            float startIGT = GetIGT();

            while (!_token.IsCancellationRequested)
            {
                // Plan the next window
                List<(MousePoint move, float igt)> plan = new List<(MousePoint move, float igt)>();

                try
                {
                    // Get IGT Range
                    float endIGT = startIGT + _planDurationMs / 1000.0f;

                    while (_playQueue.TryPeek(out var play) && play.IGT < endIGT)
                    {
                        // Get queued item
                        if (_playQueue.TryDequeue(out var nextMove))
                        {
                            nextMove.MouseX = (int)((float)nextMove.MouseX * 1.2);
                            nextMove.MouseY = (int)((float)nextMove.MouseY * 1.2);
                            plan.Add((nextMove, nextMove.IGT));
                        }
                    }

                    if (plan.Count > 0)
                    {
                        _plans.Enqueue(plan);
                        ManaSpline.AppendLog($"Planned {plan.Count} moves between {startIGT} - {endIGT}");
                    }

                    // Set new start IGT
                    startIGT = endIGT;

                    // while (_plans.Count > 2)
                    await Task.Delay((int)_planDurationMs / 2, _token);

                }
                catch (TaskCanceledException)
                {
                    break;  // Handle cancellation
                }
                catch (Exception ex)
                {
                    ManaSpline.AppendLog($"Error in PlanRunner: {ex.Message}");
                    break;
                }
            }
        }


        private async Task ExecutePlan(int id)
        {
            // Wait for plans
            while (_plans.IsEmpty)
                await Task.Delay(100, _token);

            await Task.Delay((int)_planDurationMs / 2 * id, _token);

            while (!_token.IsCancellationRequested)
            {
                if (_plans.TryDequeue(out var plan))
                {
                    try
                    {
                        ManaSpline.AppendLog($"Executing plan with {plan.Count} moves.");

                        float startIGT = GetIGT();
                        int firstIdx = 0;

                        // Find first move in the future
                        for (int i=0; i<plan.Count; i++)
                        {
                            if (plan[i].move.IGT > startIGT)
                            {
                                firstIdx = i;
                                break;
                            }                            
                        }

                        MousePoint firstMove = plan[firstIdx].move;
                        float firstIGT = firstMove.IGT;
                        
                        // Delay until first move
                        ManaSpline.AppendLog($"[{id}] Starting move {firstIdx} at {firstIGT}");

                        while ((firstIGT - GetIGT()) * 1000.0f >= 250)
                        {
                            await Task.Delay(100, _token);
                        }
                        var delayMs = Math.Max((int)((firstIGT - GetIGT()) * 1000.0f), 0);
                        await Task.Delay(delayMs, _token);

                        // Execute first move
                        SimulateMouseMove(firstMove);

                        // Loop through subsequent moves
                        for (int i=firstIdx+1; i<plan.Count; i++)
                        {
                            delayMs = Math.Max((int)((plan[i].igt - GetIGT()) * 1000.0f), 0);
                            await Task.Delay(delayMs, _token);
                            SimulateMouseMove(plan[i].move);
                        }
                        float drift = (GetIGT() - plan[plan.Count-1].move.IGT) * 1000.0f;
                        ManaSpline.AppendLog($"[{id}] Total drift = {drift} ms, {drift/plan.Count} ms/move");
                    }
                    catch (Exception ex)
                    {
                        ManaSpline.AppendLog($"Error in Executor: {ex.Message}");
                    }
                }
            }
        }

        public override string SerializeRecord(MousePoint record)
        {
            return JsonConvert.SerializeObject(new
            {
                IGT = record.IGT,
                MOUSE = new
                {   
                    IGT = record.IGT,
                    MouseX = record.MouseX,
                    MouseY = record.MouseY
                }
            });
        }

        public override MousePoint DeserializeRecord(dynamic json)
        {
            return new MousePoint
            {
                IGT = (float)json.IGT,
                MouseX = (int)json.MouseX,
                MouseY = (int)json.MouseY
            };
        }

        public override void Start()
        {
            _globalHook.MouseMove += OnMouseMove;
            base.Start();
        }

        public override void Play()
        {
            ManaSpline.AppendLog("Mouse - Play");
            Task.Run(() => PlaybackAsync());
            Task.Run(() => ExecutePlan(0));
            Task.Run(() => ExecutePlan(1));
        }

        public override void Stop()
        {
            _globalHook.MouseMove -= OnMouseMove;
            base.Stop();
        }
    }

    public struct MousePoint
    {
        public float IGT { get; set; }
        public int MouseX {get; set; }
        public int MouseY { get; set; }
    }
}