using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ManaSpline
{
    public static class ManaSpline
    {
        // Configuration and state attributes
        public static string GameConfig { get; set; }
        public static string OutputDir { get; set; }
        public static string OutputFile { get; set; }
        public static string PlaybackFile { get; set; }

        public static bool EnableKeyRecording { get; set; }
        public static bool EnableMouseRecording { get; set; }
        public static bool EnableStateRecording { get; set; }
        public static int StateInterval { get; set; }

        public static bool EnableKeyPlayback { get; set; }
        public static bool EnableMousePlayback { get; set; }
        public static bool EnableStatePlayback { get; set; }

        public static bool Verbose { get; set; }

        // RecorderPlayers
        public static KeyRecorderPlayer keyRecorderPlayer { get; set; }
        public static MouseRecorderPlayer mouseRecorderPlayer { get; set; }
        public static StateRecorderPlayer stateRecorderPlayer { get; set; }

        // Time
        public static Label lblLogOutput {get; set; }

        // Object attributes
        public static MemManager memManager { get; set; }
        public static RecordingFileWriter FileWriter { get; set; }
        public static PlaybackFileReader FileReader { get; set; }

        public static bool IsWriting => FileWriter.IsWriting;

        // Delegates
        public delegate void LogHandler(string message);
        public static LogHandler AppendLog { get; set; }

        public delegate void KeyRecorderInitDelegate();
        public static KeyRecorderInitDelegate KeyRecorderInit { get; set; }

        public static float GetIGT()
        {
            try
            {
                return memManager.GetIGT();
            }
            catch (Exception)
            {
                return 0f;
            }
        }
    }

    public partial class ManaForm : Form
    {
        private RecordingPlaybackManager _recordingPlaybackManager;

        private int _igtInterval = 100;

        private bool IsRecording() => (_recordingPlaybackManager != null) ? _recordingPlaybackManager.IsRecording() : false;
        private bool IsPlaying() => (_recordingPlaybackManager != null) ? _recordingPlaybackManager.IsPlaying() : false;
        private bool IsPaused() => (_recordingPlaybackManager != null) ? _recordingPlaybackManager.IsPaused() : false;


        public ManaForm()
        {
            InitializeComponent();

            // Init Configuration
            LoadLastConfig();
            InitializeClassAttributes();

            ManaSpline.AppendLog = AppendLog;

            // Init Button States
            btnRecordEnabled();
            btnPlayEnabled();
            btnPauseEnabled();
            btnStopDisabled();
        }

        private void InitRecordingPlaybackManager()
        {
            if (string.IsNullOrWhiteSpace(ManaSpline.OutputDir) || string.IsNullOrWhiteSpace(ManaSpline.OutputFile))
            {
                MessageBox.Show(
                    "Please provide valid paths for configuration and output.", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error
                );
                return;
            }

            if (ManaSpline.EnableKeyRecording || ManaSpline.EnableMouseRecording || ManaSpline.EnableStateRecording)
            {
                ManaSpline.AppendLog("ManaForm - Init File Writer");
                ManaSpline.FileWriter = new RecordingFileWriter(Path.Combine(ManaSpline.OutputDir, ManaSpline.OutputFile));
            }
                

            if (ManaSpline.EnableKeyPlayback || ManaSpline.EnableMousePlayback || ManaSpline.EnableStatePlayback)
            {
                ManaSpline.AppendLog("ManaForm - Init File Reader");
                ManaSpline.FileReader = new PlaybackFileReader(ManaSpline.PlaybackFile);
            }
                

            if (_recordingPlaybackManager == null)
            {
                ManaSpline.AppendLog("ManaForm - Init RecordingPlayback Manager");
                _recordingPlaybackManager = new RecordingPlaybackManager();
            }

            Task.Run(() => UpdateIgtText());
                
        }

        private async Task UpdateIgtText()
        {
            while (true)
            {
                float igt = ManaSpline.GetIGT();
                ManaSpline.lblLogOutput.Text = $"Log Output: [ {igt:F2} ]";
                await Task.Delay(_igtInterval);
            }
        }

        private void StartRecording()
        {
            ManaSpline.AppendLog("ManaForm - StartRecording");
            InitRecordingPlaybackManager();
            _recordingPlaybackManager.StartRecording();
        }

        private void StopRecording()
        {
            ManaSpline.AppendLog("ManaForm - StopRecording");
            _recordingPlaybackManager.Stop();
        } 

        private void StartPlayback()
        {
            ManaSpline.AppendLog("ManaForm - StartPlayback");
            InitRecordingPlaybackManager();
            _recordingPlaybackManager.StartPlayback();
        }

        private void StopPlayback()
        {
            ManaSpline.AppendLog("ManaForm - StopPlayback");
            if (_recordingPlaybackManager != null)
            {
                _recordingPlaybackManager.Stop();
                _recordingPlaybackManager = null;
            }
        }
    }    
}