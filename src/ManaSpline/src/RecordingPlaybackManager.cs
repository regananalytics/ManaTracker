using System;
using System.Threading;
using System.Threading.Tasks;

namespace ManaSpline
{
    public class RecordingPlaybackManager
    {
        private MemManager _memManager;
        public StateRecorderPlayer _stateRecorderPlayer;
        public KeyRecorderPlayer _keyRecorderPlayer;
        public MouseRecorderPlayer _mouseRecorderPlayer;

        private CancellationTokenSource _cts;

        private bool _keyRecordingStarted = false;
        private bool _mouseRecordingStarted = false;
        private bool _stateRecordingStarted = false;

        private bool _keyPlaybackStarted = false;
        private bool _mousePlaybackStarted = false;
        private bool _statePlaybackStarted = false;

        private bool _isRecording = false;
        private bool _isPlaying = false;
        private bool _isPaused = false;

        public bool IsRecording() => _isRecording;
        public bool IsPlaying() => _isPlaying;
        public bool IsPaused() => _isPaused;


        public RecordingPlaybackManager()
        {
            ManaSpline.AppendLog("RPM - Initializing");
            _cts = new CancellationTokenSource();

            _memManager = new MemManager();
            ManaSpline.memManager = _memManager;

            _keyRecorderPlayer = new KeyRecorderPlayer(_cts.Token);
            _mouseRecorderPlayer = new MouseRecorderPlayer(_cts.Token);
            _stateRecorderPlayer = new StateRecorderPlayer(_cts.Token);

            ManaSpline.keyRecorderPlayer = _keyRecorderPlayer;
            ManaSpline.mouseRecorderPlayer = _mouseRecorderPlayer;
            ManaSpline.stateRecorderPlayer = _stateRecorderPlayer;
        }

        public void StartRecording()
        {
            _isRecording = true;
            _isPlaying = false;
            _isPaused = false;

            ManaSpline.AppendLog("RPM - Start Recording");

            // Start both state and key recorders
            if (ManaSpline.EnableStateRecording)
            {
                ManaSpline.AppendLog("RPM - Started State Recording");
                _stateRecorderPlayer.Start();
                _stateRecordingStarted = true;
            }
            if (ManaSpline.EnableKeyRecording)
            {
                ManaSpline.AppendLog("RPM - Started State Recording");
                _keyRecorderPlayer.Start();
                _keyRecordingStarted = true;
            }
            if (ManaSpline.EnableMouseRecording)
            {
                ManaSpline.AppendLog("RPM - Started State Recording");
                _mouseRecorderPlayer.Start();
                _mouseRecordingStarted = true;
            }
        }

        public void StartPlayback()
        {
            _isPlaying = true;
            _isRecording = false;
            _isPaused = false;

            ManaSpline.AppendLog("RPM - Start Playback");
            ManaSpline.FileReader.Start(_cts.Token);

            // Start state playback if enabled
            if (ManaSpline.EnableStatePlayback)
            {
                ManaSpline.AppendLog("RPM - Started State Playback");
                _stateRecorderPlayer.Play();
                _statePlaybackStarted = true;
            }

            // Start key playback if enabled
            if (ManaSpline.EnableKeyPlayback)
            {
                ManaSpline.AppendLog("RPM - Started Key Playback");
                _keyRecorderPlayer.Play();
                _keyPlaybackStarted = true;
            }

            // Start mouse playback if enabled
            if (ManaSpline.EnableMousePlayback)
            {
                ManaSpline.AppendLog("RPM - Started Mouse Playback");
                _mouseRecorderPlayer.Play();
                _mousePlaybackStarted = true;
            }
        }

        public void Stop()
        {
            _cts.Cancel();
            ManaSpline.FileWriter.Stop();

            _isRecording = false;
            _isPlaying = false;
            _isPaused = false;

            ManaSpline.AppendLog("RPM - Stop");

            // Stop both the state and key recorders
            if (_stateRecordingStarted)
                _stateRecorderPlayer.Stop();

            if (_keyRecordingStarted)
                _keyRecorderPlayer.Stop();

            if (_mouseRecordingStarted)
                _mouseRecorderPlayer.Stop();

        }

        public void Pause()
        {
            _isPaused = true;

            ManaSpline.AppendLog("RPM - Pause");

            if (_stateRecordingStarted || _statePlaybackStarted)
                _stateRecorderPlayer.Pause();

            if (_keyRecordingStarted || _keyPlaybackStarted)
                _keyRecorderPlayer.Pause();

            if (_mouseRecordingStarted || _mousePlaybackStarted)
                _mouseRecorderPlayer.Pause();
        }

        public void Resume()
        {
            _isPaused = false;

            ManaSpline.AppendLog("RPM - Resume");

            if (_stateRecordingStarted || _statePlaybackStarted)
                _stateRecorderPlayer.Resume();

            if (_keyRecordingStarted || _keyPlaybackStarted)
                _keyRecorderPlayer.Resume();
                
            if (_mouseRecordingStarted || _mousePlaybackStarted)
                _mouseRecorderPlayer.Resume();
        }
    }
}