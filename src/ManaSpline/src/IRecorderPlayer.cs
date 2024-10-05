using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace ManaSpline
{

    public abstract class RecorderPlayerBase<TRecord>
    {
        private bool _isRunning = false;
        private bool _isPaused = false;

        public CancellationToken _token;

        public readonly ConcurrentQueue<TRecord> _playQueue = new ConcurrentQueue<TRecord>();

        public bool IsRunning => _isRunning;
        public bool IsPaused => _isPaused;
        private bool _verbose => ManaSpline.Verbose;

        public virtual void Start()
        {
            _isRunning = true;
            _isPaused = false;
        }

        public virtual void Play()
        {
            _isRunning = true;
            _isPaused = false;
        }

        public virtual void Stop()
        {
            _isRunning = false;
            _isPaused = false;
        }

        public virtual void Pause()
        {
            if (_isRunning)
                _isPaused = true;
        }

        public virtual void Resume()
        {
            if (_isRunning && _isPaused)
                _isPaused = false;
        }

        // Abstract methods that subclasses must implement
        public abstract Task RecordAsync(TRecord record);
        public abstract Task PlaybackAsync();
        public abstract string SerializeRecord(TRecord record);
        public abstract TRecord DeserializeRecord(dynamic json);

        public void QueuePlay(TRecord play) => _playQueue.Enqueue(play);

        // Time Helpers

        public float GetIGT() => ManaSpline.GetIGT();
    }
}