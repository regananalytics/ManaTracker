using MemCore;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ManaSpline
{
    public class MemManager
    {
        private string _configPath;
        private CancellationTokenSource _cts;

        private MemoryCore _memCore;
        private bool _isConnected = false;

        private readonly object _lock = new object();

        private Stopwatch _stopwatch;
        private long _syncIntervalMs = 25;

        private float _lastIGTSync = 0;

        public bool IsConnected => _isConnected;

        public MemManager()
        {
            _configPath = ManaSpline.GameConfig;
            _cts = new CancellationTokenSource();

            Task.Run(() => InitMemCore());
        }

        public Dictionary<string, object> GetGameState()
        {
            return _memCore.GetState();
        }

        public float GetGameTime()
        {
            if (_memCore != null)
            {
                var state = _memCore.GetState();
                if (state != null)
                    return (float)state["IGT"];
            }
            return 0;
        }

        public float GetIGT()
        {
            lock (_lock)
            {
                if (_stopwatch.ElapsedMilliseconds >= _syncIntervalMs)
                {
                    _lastIGTSync = GetGameTime();
                    _stopwatch.Restart();
                }
                return _lastIGTSync + (float)_stopwatch.ElapsedMilliseconds / 1000.0f;
            }
        }

        private async Task InitMemCore()
        {
            while (!_cts.Token.IsCancellationRequested && _memCore == null)
            {
                try
                {
                    _memCore = new MemoryCore(_configPath, false);
                    _isConnected = true;
                    ManaSpline.AppendLog("Connected to Game!");

                    _lastIGTSync = GetGameTime();
                    _stopwatch = Stopwatch.StartNew();
                    break;
                }
                catch (InvalidOperationException)
                {
                    // Process not available, keep waiting.
                    await Task.Delay(500);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Error occured while connecting to game: {ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error
                    );
                    return;
                }
            }
        }
    }
}