using Newtonsoft.Json;
using System;
using System.IO;
using System.Drawing;
using System.Windows.Forms;

namespace ManaSpline
{
    public partial class ManaForm
    {
        private const string ConfigFilePath = "config.json";

        // Main Component
        private void InitializeComponent()
        {
            // Initialize components
            int _width = 800;
            int _height = 880;
            int _lpad = 20;

            int _media_height = _height - 130;

            this.Text = "ManaSpline - Game Recording Configuration";
            this.Size = new Size(_width, _height);

            // Game Name
            Label lblGameConfig = new Label();
            lblGameConfig.Text = "Game Config Path:";
            lblGameConfig.Location = new Point(_lpad, 20);
            lblGameConfig.AutoSize = true;

            TextBox txtGameConfig = new TextBox();
            txtGameConfig.Name = "txtGameConfig";
            txtGameConfig.Location = new Point(180, 20);
            txtGameConfig.Size = new Size(470, 20);
            txtGameConfig.TextChanged += OnInputChange;

            Button btnGameConfigBrowse = new Button();
            btnGameConfigBrowse.Text = "...";
            btnGameConfigBrowse.Location = new Point(660, 20);
            btnGameConfigBrowse.AutoSize = true;
            btnGameConfigBrowse.Click += new EventHandler(BrowseGameConfigPath);

            // Output File Path
            Label lblOutputDir = new Label();
            lblOutputDir.Text = "Output Directory:";
            lblOutputDir.Location = new Point(_lpad, 60);
            lblOutputDir.AutoSize = true;

            TextBox txtOutputDir = new TextBox();
            txtOutputDir.Name = "txtOutputDir";
            txtOutputDir.Location = new Point(180, 60);
            txtOutputDir.Size = new Size(470, 20);
            txtOutputDir.TextChanged += OnInputChange;

            Button btnOutputDirBrowse = new Button();
            btnOutputDirBrowse.Text = "...";
            btnOutputDirBrowse.Location = new Point(660, 60);
            btnOutputDirBrowse.AutoSize = true;
            btnOutputDirBrowse.Click += new EventHandler(BrowseOutputDirPath);

            // Panel for Output File Name
            Label lblOutputFile = new Label();
            lblOutputFile.Text = "Output File Name:";
            lblOutputFile.Location = new Point(_lpad, 100);
            lblOutputFile.AutoSize = true;

            TextBox txtOutputFile = new TextBox();
            txtOutputFile.Name = "txtOutputFile";
            txtOutputFile.Location = new Point(180, 100);
            txtOutputFile.Size = new Size(470, 20);
            txtOutputFile.TextChanged += OnInputChange;

            // Input Playback File
            Label lblPlaybackFile = new Label();
            lblPlaybackFile.Text = "Playback File:";
            lblPlaybackFile.Location = new Point(_lpad, 140);
            lblPlaybackFile.AutoSize = true;

            TextBox txtPlaybackFile = new TextBox();
            txtPlaybackFile.Name = "txtPlaybackFile";
            txtPlaybackFile.Location = new Point(180, 140);
            txtPlaybackFile.Size = new Size(470, 20);
            txtPlaybackFile.TextChanged += OnInputChange;

            Button btnPlaybackFileBrowse = new Button();
            btnPlaybackFileBrowse.Text = "...";
            btnPlaybackFileBrowse.Location = new Point(660, 140);
            btnPlaybackFileBrowse.AutoSize = true;
            btnPlaybackFileBrowse.Click += new EventHandler(BrowsePlaybackFilePath);

            // Toggles for Recording
            Label lblRecording = new Label();
            lblRecording.Text = "Recording Options:";
            lblRecording.Location = new Point(60, 200);
            lblRecording.AutoSize = true;

            CheckBox chkKeyRecording = new CheckBox();
            chkKeyRecording.Name = "chkKeyRecording";
            chkKeyRecording.Text = "Enable Key Recording";
            chkKeyRecording.Location = new Point(80, 230);
            chkKeyRecording.AutoSize = true;
            chkKeyRecording.TextChanged += OnInputChange;

            CheckBox chkMouseRecording = new CheckBox();
            chkMouseRecording.Name = "chkMouseRecording";
            chkMouseRecording.Text = "Enable Mouse Recording";
            chkMouseRecording.Location = new Point(80, 260);
            chkMouseRecording.AutoSize = true;
            chkMouseRecording.TextChanged += OnInputChange;

            CheckBox chkStateRecording = new CheckBox();
            chkStateRecording.Name = "chkStateRecording";
            chkStateRecording.Text = "Enable State Recording";
            chkStateRecording.Location = new Point(80, 290);
            chkStateRecording.AutoSize = true;
            chkStateRecording.TextChanged += OnInputChange;

            // State Recording Interval
            Label lblInterval = new Label();
            lblInterval.Text = "State Interval (ms):";
            lblInterval.Location = new Point(80, 330);
            lblInterval.AutoSize = true;

            NumericUpDown numInterval = new NumericUpDown();
            numInterval.Name = "numInterval";
            numInterval.Location = new Point(240, 330);
            numInterval.Minimum = 100;
            numInterval.Maximum = 5000;
            numInterval.Value = 1000;
            numInterval.ValueChanged += OnInputChange;

            // Toggles for Playback (Key, Mouse, State)
            Label lblPlayback = new Label();
            lblPlayback.Text = "Playback Options:";
            lblPlayback.Location = new Point(440, 200);
            lblPlayback.AutoSize = true;

            CheckBox chkKeyPlayback = new CheckBox();
            chkKeyPlayback.Name = "chkKeyPlayback";
            chkKeyPlayback.Text = "Enable Key Playback";
            chkKeyPlayback.Location = new Point(460, 230);
            chkKeyPlayback.AutoSize = true;
            chkKeyPlayback.CheckedChanged += OnInputChange;

            CheckBox chkMousePlayback = new CheckBox();
            chkMousePlayback.Name = "chkMousePlayback";
            chkMousePlayback.Text = "Enable Mouse Playback";
            chkMousePlayback.Location = new Point(460, 260);
            chkMousePlayback.AutoSize = true;
            chkMousePlayback.CheckedChanged += OnInputChange;

            CheckBox chkStatePlayback = new CheckBox();
            chkStatePlayback.Name = "chkStatePlayback";
            chkStatePlayback.Text = "Enable State Playback";
            chkStatePlayback.Location = new Point(460, 290);
            chkStatePlayback.AutoSize = true;
            chkStatePlayback.CheckedChanged += OnInputChange;

            // Verbose Output
            CheckBox chkVerbose = new CheckBox();
            chkVerbose.Name = "chkVerbose";
            chkVerbose.Text = "Verbose";
            chkVerbose.Location = new Point(_lpad, _height - 100);
            chkVerbose.AutoSize = true;
            chkVerbose.CheckedChanged += OnInputChange;

            // Record Button
            var buttonFont = new Font("Arial", 24);
            
            Button btnRecord = new Button();
            btnRecord.Name = "btnRecord";
            btnRecord.Text = "\u23FA";
            btnRecord.Font = buttonFont;
            btnRecord.Size = new Size(64, 64);
            btnRecord.Location = new Point(_width - 340, _media_height);
            btnRecord.BackColor = Color.White;
            btnRecord.ForeColor = Color.Red;
            btnRecord.Click += BtnRecord_Click;

            // Play Recording Button
            Button btnPlay = new Button();
            btnPlay.Name = "btnPlay";
            btnPlay.Text = "\u23F5";
            btnPlay.Font = buttonFont;
            btnPlay.Size = new Size(64, 64);
            btnPlay.Location = new Point(_width - 260, _media_height);
            btnPlay.BackColor = Color.White;
            btnPlay.ForeColor = Color.Black;
            btnPlay.Click += BtnPlay_Click;

            // Pause Recording Button
            Button btnPause = new Button();
            btnPause.Name = "btnPause";
            btnPause.Text = "\u23F8";
            btnPause.Font = buttonFont;
            btnPause.Size = new Size(64, 64);
            btnPause.Location = new Point(_width - 180, _media_height);
            btnPause.BackColor = Color.White;
            btnPause.ForeColor = Color.Black;
            btnPause.Click += BtnPause_Click;

            // Stop Recording Button
            Button btnStop = new Button();
            btnStop.Name = "btnStop";
            btnStop.Text = "\u23F9";
            btnStop.Font = buttonFont;
            btnStop.Size = new Size(64, 64);
            btnStop.Location = new Point(_width - 100, _media_height);
            btnStop.BackColor = Color.White;
            btnStop.ForeColor = Color.Black;
            btnStop.Click += BtnStop_Click;

            // Textbox for output
            Label lblLogOutput = new Label();
            lblLogOutput.Text = "Log Output: ";
            lblLogOutput.Location = new Point(_lpad, 390); 
            lblLogOutput.AutoSize = true;
            ManaSpline.lblLogOutput = lblLogOutput;

            TextBox txtLogOutput = new TextBox();
            txtLogOutput.Name = "txtLogOutput";
            txtLogOutput.Location = new Point(_lpad, 420); 
            txtLogOutput.Size = new Size(_width - 60, 300);
            txtLogOutput.Multiline = true;
            txtLogOutput.ScrollBars = ScrollBars.Vertical;
            txtLogOutput.ReadOnly = true;
            txtLogOutput.BackColor = Color.White;

            // Add Controls to the Form
            this.Controls.Add(lblGameConfig);
            this.Controls.Add(txtGameConfig);
            this.Controls.Add(btnGameConfigBrowse);
            this.Controls.Add(lblOutputDir);
            this.Controls.Add(txtOutputDir);
            this.Controls.Add(btnOutputDirBrowse);
            this.Controls.Add(lblOutputFile);
            this.Controls.Add(txtOutputFile);
            this.Controls.Add(lblPlaybackFile);
            this.Controls.Add(txtPlaybackFile);
            this.Controls.Add(btnPlaybackFileBrowse);
            this.Controls.Add(lblRecording);
            this.Controls.Add(chkKeyRecording);
            this.Controls.Add(chkMouseRecording);
            this.Controls.Add(chkStateRecording);
            this.Controls.Add(lblInterval);
            this.Controls.Add(numInterval);
            this.Controls.Add(lblPlayback);
            this.Controls.Add(chkKeyPlayback);
            this.Controls.Add(chkMousePlayback);
            this.Controls.Add(chkStatePlayback);
            this.Controls.Add(lblLogOutput);
            this.Controls.Add(txtLogOutput);
            this.Controls.Add(chkVerbose);
            this.Controls.Add(btnRecord);
            this.Controls.Add(btnPlay);
            this.Controls.Add(btnPause);
            this.Controls.Add(btnStop);
        }

        // Callbacks
        private void BtnRecord_Click(object sender, EventArgs e)
        {
            InitializeClassAttributes();

            if (!IsRecording())
            {
                // Start Recording
                AppendLog("Recording started.");
                StartRecording();
                // Configure buttons
                btnRecordPressed();
                btnStopEnabled();
            }   
            else
            {
                // Stop Recording
                AppendLog("Recording stopped.");
                StopRecording();
                // Configure buttons
                btnRecordEnabled();
                if (!IsPlaying())
                    btnStopDisabled();
            }
        }

        private void BtnPlay_Click(object sender, EventArgs e)
        {
            InitializeClassAttributes();

            if (!IsPlaying())
            {
                // Start Playback
                AppendLog("Playback started.");
                StartPlayback();
                // Configure buttons
                btnPlayPressed();
                btnStopEnabled();
            }
            else
            {
                // Stop Playback
                AppendLog("Playback stopped.");
                StopPlayback();
                // Configure buttons
                btnPlayEnabled();
                if (!IsRecording())
                    btnStopDisabled();
            }
    }

    private void BtnPause_Click(object sender, EventArgs e)
    {
        if (!IsPaused())
        {
            AppendLog("Paused");

            if (IsRecording() || IsPlaying())
                _recordingPlaybackManager.Pause();

            // Configure buttons
            btnPausePressed();
        }
        else
        {
                AppendLog("Unpaused");

                if (IsRecording() || IsPlaying())
                    _recordingPlaybackManager.Resume();
                // Configure buttons
                btnPauseEnabled();
            }
        }

        private void BtnStop_Click(object sender, EventArgs e)
        {
            if (IsRecording())
            {
                // Stop recording
                StopRecording();
                AppendLog("Recording stopped.");
            }
            if (IsPlaying())
            {
                // Stop playback
                StopPlayback();
                AppendLog("Playback stopped.");
            }

            // Configure buttons
            btnRecordEnabled();
            btnPlayEnabled();
            btnPauseEnabled();
            btnStopDisabled();

            // Post-process
            if (!IsPlaying())
            {
                var postProcessor = new RecordPostProcessor(Path.Combine(ManaSpline.OutputDir, ManaSpline.OutputFile));
                postProcessor.Process();
            }
        }

        private void AppendLog(string message)
        {
            TextBox txtLogOutput = this.Controls["txtLogOutput"] as TextBox;
            txtLogOutput.AppendText($"{Environment.NewLine}{message}");
            txtLogOutput.SelectionStart = txtLogOutput.Text.Length;
            txtLogOutput.ScrollToCaret();
        }

        private void BrowseGameConfigPath(object sender, EventArgs e)
        {
            // Open file dialog to select the meory config file
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.InitialDirectory = Environment.CurrentDirectory;
            dlg.Filter = "YAML files (*.yaml)|*.yaml|All files (*.*)|*.*";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                TextBox txtGameConfig = this.Controls["txtGameConfig"] as TextBox;
                txtGameConfig.Text = dlg.FileName;
            }
        }

        private void BrowseOutputDirPath(object sender, EventArgs e)
        {
            // Open file dialog to select the output JSON file
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            dlg.Description = "Select Output Directory";
            dlg.InitialDirectory = Environment.CurrentDirectory;
            dlg.ShowNewFolderButton = true;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                TextBox txtOutputDir = this.Controls["txtOutputDir"] as TextBox;
                txtOutputDir.Text = dlg.SelectedPath;
            }
        }

        private void BrowsePlaybackFilePath(object sender, EventArgs e)
        {
            // Open file dialog to select the input (playback) JSON file
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.InitialDirectory = Environment.CurrentDirectory;
            dlg.Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                TextBox txtPlaybackFile = this.Controls["txtPlaybackFile"] as TextBox;
                txtPlaybackFile.Text = dlg.FileName;
            }
        }

        // Button State Functions
        // Start
        private void btnRecordEnabled()
        {
            var btnRecord = this.Controls["btnRecord"] as Button;
            btnRecord.Enabled = true;
            btnRecord.BackColor = Color.White;
            btnRecord.ForeColor = Color.Red;
        }

        private void btnRecordDisabled()
        {
            var btnRecord = this.Controls["btnRecord"] as Button;
            btnRecord.Enabled = false;
            btnRecord.BackColor = Color.White;
            btnRecord.ForeColor = Color.Red;
        }

        private void btnRecordPressed()
        {
            var btnRecord = this.Controls["btnRecord"] as Button;
            btnRecord.Enabled = true;
            btnRecord.BackColor = Color.Green;
            btnRecord.ForeColor = Color.White;
        }

        // Play
        private void btnPlayEnabled()
        {
            var btnPlay = this.Controls["btnPlay"] as Button;
            btnPlay.Enabled = true;
            btnPlay.BackColor = Color.White;
            btnPlay.ForeColor = Color.Green;
        }

        private void btnPlayDisabled()
        {
            var btnPlay = this.Controls["btnPlay"] as Button;
            btnPlay.Enabled = false;
            btnPlay.BackColor = Color.White;
            btnPlay.ForeColor = Color.Green;
        }

        private void btnPlayPressed()
        {
            var btnPlay = this.Controls["btnPlay"] as Button;
            btnPlay.Enabled = true;
            btnPlay.BackColor = Color.Green;
            btnPlay.ForeColor = Color.White;
        }

        // Pause
        private void btnPauseEnabled()
        {
            var btnPause = this.Controls["btnPause"] as Button;
            btnPause.Enabled = true;
            btnPause.BackColor = Color.White;
            btnPause.ForeColor = Color.Black;
        }

        private void btnPauseDisabled()
        {
            var btnPause = this.Controls["btnPause"] as Button;
            btnPause.Enabled = false;
            btnPause.BackColor = Color.White;
            btnPause.ForeColor = Color.Black;
        }

        private void btnPausePressed()
        {
            var btnPause = this.Controls["btnPause"] as Button;
            btnPause.Enabled = true;
            btnPause.BackColor = Color.Black;
            btnPause.ForeColor = Color.White;
        }

        // Stop
        private void btnStopEnabled()
        {
            var btnStop = this.Controls["btnStop"] as Button;
            btnStop.Enabled = true;
            btnStop.BackColor = Color.White;
            btnStop.ForeColor = Color.Black;
        }

        private void btnStopDisabled()
        {
            var btnStop = this.Controls["btnStop"] as Button;
            btnStop.Enabled = false;
            btnStop.BackColor = Color.White;
            btnStop.ForeColor = Color.Black;
        }

        private void btnStopPressed()
        {
            var btnStop = this.Controls["btnStop"] as Button;
            btnStop.Enabled = true;
            btnStop.BackColor = Color.Black;
            btnStop.ForeColor = Color.White;
        }

        // Config
        private void InitializeClassAttributes()
        {
            // Intialize values from controls when the form loads
            ManaSpline.GameConfig = (this.Controls["txtGameConfig"] as TextBox).Text;
            ManaSpline.OutputDir = (this.Controls["txtOutputDir"] as TextBox).Text;
            ManaSpline.OutputFile = (this.Controls["txtOutputFile"] as TextBox).Text;
            ManaSpline.PlaybackFile = (this.Controls["txtPlaybackFile"] as TextBox).Text;
            ManaSpline.EnableKeyRecording = (this.Controls["chkKeyRecording"] as CheckBox).Checked;
            ManaSpline.EnableMouseRecording = (this.Controls["chkMouseRecording"] as CheckBox).Checked;
            ManaSpline.EnableStateRecording = (this.Controls["chkStateRecording"] as CheckBox).Checked;
            ManaSpline.StateInterval = (int)(this.Controls["numInterval"] as NumericUpDown).Value;
            ManaSpline.EnableKeyPlayback = (this.Controls["chkKeyPlayback"] as CheckBox).Checked;
            ManaSpline.EnableMousePlayback = (this.Controls["chkMousePlayback"] as CheckBox).Checked;
            ManaSpline.EnableStatePlayback = (this.Controls["chkStatePlayback"] as CheckBox).Checked;
            ManaSpline.Verbose = (this.Controls["chkVerbose"] as CheckBox).Checked;
        }

        private void OnInputChange(object sender, EventArgs e)
        {
            // Update class-level attributes whenever an input changes
            InitializeClassAttributes();

            try
            {
                var config = new ConfigModel{
                    GameConfig = ManaSpline.GameConfig,
                    OutputDir = ManaSpline.OutputDir,
                    OutputFile = ManaSpline.OutputFile,
                    PlaybackFile = ManaSpline.PlaybackFile,
                    EnableKeyRecording = ManaSpline.EnableKeyRecording,
                    EnableMouseRecording = ManaSpline.EnableMouseRecording,
                    EnableStateRecording = ManaSpline.EnableStateRecording,
                    StateInterval = ManaSpline.StateInterval,
                    EnableKeyPlayback = ManaSpline.EnableKeyPlayback,
                    EnableMousePlayback = ManaSpline.EnableMousePlayback,
                    EnableStatePlayback = ManaSpline.EnableStatePlayback,
                    Verbose = ManaSpline.Verbose
                };

                // Write the config to a JSON file
                string json = JsonConvert.SerializeObject(config, Formatting.Indented);
                File.WriteAllText(ConfigFilePath, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving config: {ex.Message}");
            }
        }

        private void LoadLastConfig()
        {
            if (File.Exists(ConfigFilePath))
            {
                try
                {
                    // Read the config from the JSON file
                    string json = File.ReadAllText(ConfigFilePath);
                    ConfigModel config = JsonConvert.DeserializeObject<ConfigModel>(json);

                    // Safely populate form fields with config values (add null-checks)
                    var txtGameConfig = this.Controls["txtGameConfig"] as TextBox;
                    txtGameConfig.Text = config.GameConfig;
                    ManaSpline.GameConfig = config.GameConfig;

                    var txtOutputDir = this.Controls["txtOutputDir"] as TextBox;
                    txtOutputDir.Text = config.OutputDir;
                    ManaSpline.OutputDir = config.OutputDir;

                    var txtOutputFile = this.Controls["txtOutputFile"] as TextBox;
                    txtOutputFile.Text = config.OutputFile;
                    ManaSpline.OutputFile = config.OutputFile;

                    var txtPlaybackFile = this.Controls["txtPlaybackFile"] as TextBox;
                    txtPlaybackFile.Text = config.PlaybackFile;
                    ManaSpline.PlaybackFile = config.PlaybackFile;

                    var chkKeyRecording = this.Controls["chkKeyRecording"] as CheckBox;
                    chkKeyRecording.Checked = config.EnableKeyRecording;
                    ManaSpline.EnableKeyRecording = config.EnableKeyRecording;

                    var chkMouseRecording = this.Controls["chkMouseRecording"] as CheckBox;
                    chkMouseRecording.Checked = config.EnableMouseRecording;
                    ManaSpline.EnableMouseRecording = config.EnableMouseRecording;

                    var chkStateRecording = this.Controls["chkStateRecording"] as CheckBox;
                    chkStateRecording.Checked = config.EnableStateRecording;
                    ManaSpline.EnableStateRecording = config.EnableStateRecording;

                    var numInterval = this.Controls["numInterval"] as NumericUpDown;
                    numInterval.Value = config.StateInterval;
                    ManaSpline.StateInterval = config.StateInterval;

                    var chkKeyPlayback = this.Controls["chkKeyPlayback"] as CheckBox;
                    chkKeyPlayback.Checked = config.EnableKeyPlayback;
                    ManaSpline.EnableKeyPlayback = config.EnableKeyPlayback;

                    var chkMousePlayback = this.Controls["chkMousePlayback"] as CheckBox;
                    chkMousePlayback.Checked = config.EnableMousePlayback;
                    ManaSpline.EnableMousePlayback = config.EnableMousePlayback;

                    var chkStatePlayback = this.Controls["chkStatePlayback"] as CheckBox;
                    chkStatePlayback.Checked = config.EnableStatePlayback;
                    ManaSpline.EnableStatePlayback = config.EnableStatePlayback;

                    var chkVerbose = this.Controls["chkVerbose"] as CheckBox;
                    chkVerbose.Checked = config.Verbose;
                    ManaSpline.Verbose = config.Verbose;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading config: {ex.Message}");
                }
            }
        }
    }

    public class ConfigModel
    {
        public string GameConfig { get; set; }
        public string OutputDir { get; set; }
        public string OutputFile { get; set; }
        public string PlaybackFile { get; set; }
        public bool EnableKeyRecording { get; set; }
        public bool EnableMouseRecording { get; set; }
        public bool EnableStateRecording { get; set; }
        public int StateInterval { get; set; }
        public bool EnableKeyPlayback { get; set; }
        public bool EnableMousePlayback { get; set; }
        public bool EnableStatePlayback { get; set; }
        public bool Verbose { get; set; }
    }
}