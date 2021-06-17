using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using CSCore;
using CSCore.Codecs;
using CSCore.DSP;
using CSCore.SoundOut;
using CSCore.SoundIn;
using CSCore.Streams;
using CSCore.Streams.Effects;
using CSCore.CoreAudioAPI;
using SMProjekt.Visualization;
using CSCore.Codecs.WAV;
using CSCore.DMO.Effects;
using System.IO;
using CSCore.DirectSound;

namespace SMProjekt
{
    public partial class Form1 : Form
    {
        private WasapiCapture _soundIn;
        private ISoundOut _soundOut;
        private IWaveSource _source;
        private PitchShifter _pitchShifter;
        private LineSpectrum _lineSpectrum;
        private readonly Bitmap _bitmap = new Bitmap(2000, 600);
        private WaveWriter writer;
        bool stop = true;
        bool stopRecord = true;
        private TimeSpan timer;
        private TimeSpan timeRecorded;
        private string pathtoFile;
        private string pathtoMergeFile1;
        private string pathtoMergeFile2;
        private string pathtoConvert;
        private bool endoffile = false;
        private ISampleSource source;
        private string dir = @"Zapisane";

        public Form1()
        {
            InitializeComponent();

            labelVolume.Text = "Volume: " + trackBarVolume.Value + " %";

            labelEchoUpdate();

            labelDistortionUpdate();

            labelChorusUpdate();
            
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
        }

        //Odtwórz
        private void openToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            var openFileDialog = new OpenFileDialog()
            {
                Filter = CodecFactory.SupportedFilesFilterEn,
                Title = "Select a file..."
            };
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                Stop();
                //open the selected file
                pathtoFile = openFileDialog.FileName;
                PlayFileAudio();
            }
            
        }


        //nagraj button
        private void button1_Click(object sender, EventArgs e)
        {
            var saveFileDialog = new SaveFileDialog()
            {
                Filter = CodecFactory.SupportedFilesFilterEn,
                Title = "Select a file..."
            };
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                Stop();

                _soundIn = new WasapiCapture();   
                _soundIn.Device = MMDeviceEnumerator.DefaultAudioEndpoint(DataFlow.Capture, Role.Console);
                _soundIn.Initialize();
                

                var soundInSource = new SoundInSource(_soundIn);
                source = soundInSource.ToSampleSource();
                SetupSampleSource(source);

                writer = new WaveWriter(saveFileDialog.FileName, _soundIn.WaveFormat);

                
                byte[] buffer = new byte[_source.WaveFormat.BytesPerSecond / 2];
                soundInSource.DataAvailable += (s, aEvent) =>
                {
                    int read;
                    while ((read = _source.Read(buffer, 0, buffer.Length)) > 0) ;
                    writer.Write(aEvent.Data, aEvent.Offset, aEvent.ByteCount);
                };

                //Nagraj
                _soundIn.Start();
                //Pokaż
                timer2.Start();
            }
        }

        //stop odtwórz button
        private void stopButton_Click(object sender, EventArgs e)
        {
            Stop();
            endoffile = true;
            trackBar1.Value = 0;
        }

        private void pauzePlayButton_Click(object sender, EventArgs e)
        {
            if (endoffile == true)
            {
                PlayFileAudio();
                endoffile = false;
            }
            else
            {
                if (_soundOut != null)
                {

                        if (stop)
                        {
                            timer1.Stop();
                            _soundOut.Stop();
                            stop = false;
                            return;
                        }
                        if (!stop)
                        {
                            timer1.Start();
                            _soundOut.Play();
                            stop = true;
                            return;
                        }
                    
                }
            }
        }

        private void trackBarVolume_Scroll(object sender, EventArgs e)
        {
            labelVolume.Text = "Volume: " + trackBarVolume.Value + " %";
            if (_soundOut != null)
            {
                _soundOut.Volume = trackBarVolume.Value / 100.0f;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="aSampleSource"></param>
        private void SetupSampleSource(ISampleSource aSampleSource)
        {
            const FftSize fftSize = FftSize.Fft4096;
            //create a spectrum provider which provides fft data based on some input
            var spectrumProvider = new BasicSpectrumProvider(aSampleSource.WaveFormat.Channels,
                aSampleSource.WaveFormat.SampleRate, fftSize);

            //linespectrum and voiceprint3dspectrum used for rendering some fft data
            //in oder to get some fft data, set the previously created spectrumprovider 
            _lineSpectrum = new LineSpectrum(fftSize)
            {
                SpectrumProvider = spectrumProvider,
                UseAverage = true,
                BarCount = 50,
                BarSpacing = 2,
                IsXLogScale = true,
                ScalingStrategy = ScalingStrategy.Sqrt
            };
           

            //the SingleBlockNotificationStream is used to intercept the played samples
            var notificationSource = new SingleBlockNotificationStream(aSampleSource);
            //pass the intercepted samples as input data to the spectrumprovider (which will calculate a fft based on them)
            notificationSource.SingleBlockRead += (s, a) => spectrumProvider.Add(a.Left, a.Right);

            _source = notificationSource.ToWaveSource(16);

        }

        //stop nagrywania button
        private void button1_Click_1(object sender, EventArgs e)
        {
            timer2.Stop();
            timeRecorded = TimeSpan.Zero;
            writer.Dispose();
            Stop();
        }

        private void pauzeRecordButton_Click(object sender, EventArgs e)
        {
            if (_soundIn != null)
            {
                if (stopRecord)
                {
                    pauzeRecordButton.Text = "Start nagrywania";
                    timer2.Stop();
                    _soundIn.Stop();
                    stopRecord = false;
                    return;
                }
                if (!stopRecord)
                {
                    pauzeRecordButton.Text = "Pauza nagrywania";
                    timer2.Start();
                    _soundIn.Start();
                    stopRecord = true;
                    return;
                }
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            Stop();
        }

        private void Stop()
        {
            timer1.Stop();
            if (_soundOut != null)
            {
                _soundOut.Stop();
                _soundOut.Dispose();
                _soundOut = null;
            }
            if (_soundIn != null)
            {

                _soundIn.Stop();
                _soundIn.Dispose();
                _soundIn = null;
            }
            if (_source != null)
            {
                _source.Dispose();
                _source = null;
            }
            timerLabel2.Text = "00:00:00";

        }

        //timer odtwarzania
        private void timer1_Tick_1(object sender, EventArgs e)
        {
            
            timer = _source.GetPosition();
            string timerString = timer.ToString();
            timerLabel2.Text = timerString;

            GenerateLineSpectrum(pictureBox2);
            trackBar1.Value = (int)timer.TotalMilliseconds;

            if(timer == _source.GetLength())
            {
                Stop();
                endoffile = true;
            }
        }

        //timer nagrywania
        private void timer2_Tick(object sender, EventArgs e)
        {
            GenerateLineSpectrum(pictureBox1);
            timeRecorded = timeRecorded.Add(TimeSpan.FromMilliseconds(10));
            timerLabel1.Text = timeRecorded.ToString();
        }

        private void GenerateLineSpectrum(PictureBox a)
        {
            Image image = a.Image;
            var newImage = _lineSpectrum.CreateSpectrumLine(a.Size, Color.Green, Color.Red, Color.White, true);
            if (newImage != null)
            {
                a.Image = newImage;
                if (image != null)
                    image.Dispose();
            }
        }


        private void PlayFileAudio()
        {
            source = CodecFactory.Instance.GetCodec(pathtoFile).ToSampleSource();
            SetupSampleSource(source);

            //play the audio
            _soundOut = new WasapiOut();
            _soundOut.Initialize(_source);
            _soundOut.Volume = trackBarVolume.Value / 100.0f;
            _soundOut.Play();
            TimeSpan xxx = _source.GetLength();
            trackBar1.Maximum = (int)xxx.TotalMilliseconds;
            timer1.Start();
        }

        /*
        private void pitchShiftToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form form = new Form()
            {
                Width = 250,
                Height = 70,
                Text = String.Empty
            };
            TrackBar trackBar = new TrackBar()
            {
                TickStyle = TickStyle.None,
                Minimum = -100,
                Maximum = 100,
                Value = (int)(_pitchShifter != null ? Math.Log10(_pitchShifter.PitchShiftFactor) / Math.Log10(2) * 120 : 0),
                Dock = DockStyle.Fill
            };
            trackBar.ValueChanged += (s, args) =>
            {
                if (_pitchShifter != null)
                {
                    _pitchShifter.PitchShiftFactor = (float)Math.Pow(2, trackBar.Value / 120.0);
                    form.Text = trackBar.Value.ToString();
                }
            };
            form.Controls.Add(trackBar);
            form.ShowDialog();
            form.Dispose();
        }
        */

        //przycisk przełączający na okno odtwarzania
        private void buttonZmiana_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedTab = tabPage2;
            pictureBox2.Image = null;
        }

        //przycisk przełączający na okno nagrywania
        private void button2_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedTab = tabPage1;
            pictureBox1.Image = null;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedTab = tabPage3;
            pictureBox1.Image = null;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedTab = tabPage4;
        }
        private void button5_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedTab = tabPage5;
        }
        private void button6_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedTab = tabPage6;
        }

        private void mergeButton1_Click(object sender, EventArgs e)
        {
            var openFileDialog = new OpenFileDialog()
            {
                Filter = CodecFactory.SupportedFilesFilterEn,
                Title = "Select a file..."
            };
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                Stop();
                //open the selected file
                pathtoMergeFile1 = openFileDialog.FileName;
            }
        }

        private void mergeButton2_Click(object sender, EventArgs e)
        {
            var openFileDialog = new OpenFileDialog()
            {
                Filter = CodecFactory.SupportedFilesFilterEn,
                Title = "Select a file..."
            };
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                Stop();
                //open the selected file
                pathtoMergeFile2 = openFileDialog.FileName;
            }
        }

        private void mergeButtonCommit_Click(object sender, EventArgs e)
        {
            try
            {
                using (var reader1 = new NAudio.Wave.AudioFileReader(pathtoMergeFile1))
                using (var reader2 = new NAudio.Wave.AudioFileReader(pathtoMergeFile2))
                {
                    if (exportFileName.Text != "")
                    {
                        var mixer = new NAudio.Wave.SampleProviders.MixingSampleProvider(new[] { reader1, reader2 });
                        NAudio.Wave.WaveFileWriter.CreateWaveFile16("Zapisane\\" + exportFileName.Text + ".wav", mixer);
                        MessageBox.Show("Udało się połączyć pliki");
                    }
                    else
                    {
                        MessageBox.Show("Nazwa pliku wyjściowego nie może być pusta", "Błąd nazwy pliku", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch
            {
                MessageBox.Show("Nie wybrano pliku, bądź plik jest uszkodzony", "Błąd pliku", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void filetoformatchange_Click(object sender, EventArgs e)
        {
            var openFileDialog = new OpenFileDialog()
            {
                Filter = CodecFactory.SupportedFilesFilterEn,
                Title = "Select a file..."
            };
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                Stop();
                pathtoConvert = openFileDialog.FileName;
            }
        }

        private void buttonConvert_Click(object sender, EventArgs e)
        {
            if (exportFileName.Text != "")
            {
                if (radioButton1.Checked)
                {
                    
                    if (pathtoConvert != null)
                    {
                        try
                        {
                            NAudio.MediaFoundation.MediaFoundationApi.Startup();
                            using (var reader = new NAudio.Wave.WaveFileReader(pathtoConvert))
                            {
                                NAudio.Wave.MediaFoundationEncoder.EncodeToMp3(reader, "Zapisane\\" + exportFileName.Text + ".mp3");
                                MessageBox.Show("Przekonwertowano do mp3");
                            }
                        }
                        catch
                        {
                            MessageBox.Show("Wybrano niepoprawny format pliku", "Błąd formatu", MessageBoxButtons.OK, MessageBoxIcon.Error);

                        }
                    }
                    else
                    {
                        MessageBox.Show("Nie wybrano pliku do konwersji");
                    }
                }
                if (radioButton2.Checked)
                {
                    
                    if (pathtoConvert != null)
                    {
                        try
                        {
                            NAudio.MediaFoundation.MediaFoundationApi.Startup();
                            using (var reader = new NAudio.Wave.Mp3FileReader(pathtoConvert))
                            {
                                NAudio.Wave.WaveFileWriter.CreateWaveFile("Zapisane\\" + exportFileName.Text + ".wav", reader);
                                MessageBox.Show("Przekonwertowano do wav");
                            }
                        }
                        catch
                        {
                            MessageBox.Show("Wybrano niepoprawny format pliku", "Błąd formatu", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Nie wybrano pliku do konwersji");
                    }
                }
            }
            else
            {
                MessageBox.Show("Nazwa pliku wyjściowego nie może być pusta", "Błąd nazwy pliku", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void exportFileName_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = !char.IsLetter(e.KeyChar) && !char.IsControl(e.KeyChar)
            && !char.IsSeparator(e.KeyChar) && !char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar);
        }


        private void trackBar1_MouseUp(object sender, MouseEventArgs e)
        {
            try
            {
                _soundOut.Stop();
                TimeSpan ts = new TimeSpan(trackBar1.Value * 10000);
                source.SetPosition(ts);
                SetupSampleSource(source);

                //play the audio
                _soundOut = new WasapiOut();
                _soundOut.Initialize(_source);
                _soundOut.Play();
                timer1.Start();
            }
            catch
            {
                trackBar1.Value = 0;
            }
        }

        private void trackBar1_MouseDown(object sender, MouseEventArgs e)
        {
            double dblValue;
            timer1.Stop();
            dblValue = ((double)e.X / (double)trackBar1.Width) * (trackBar1.Maximum - trackBar1.Minimum);
            trackBar1.Value = Convert.ToInt32(dblValue);
        }

        //Wczytywanie pliku do efektów

        private void buttonEchoWczytaj_Click(object sender, EventArgs e)
        {
            //ta funkcja jest wywoływana przez przycisk wczytaj w każdym groupBox z efektem
            var openFileDialog = new OpenFileDialog()
            {
                Filter = CodecFactory.SupportedFilesFilterEn,
                Title = "Select a file..."
            };
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                Stop();
                //open the selected file
                pathtoFile = openFileDialog.FileName;
                PlayFileAudio();
            }
        }

        //Pause play w efektach

        private void buttonEchoPlayPause_Click(object sender, EventArgs e)
        {
            //ta funkcja jest wywoływana przez przycisk Play Pause w każdym groupBox z efektem
            if (endoffile == true)
            {
                PlayFileAudio();
                endoffile = false;
            }
            else
            {
                if (_soundOut != null)
                {

                    if (stop)
                    {
                        timer1.Stop();
                        _soundOut.Stop();
                        stop = false;
                        return;
                    }
                    if (!stop)
                    {
                        timer1.Start();
                        _soundOut.Play();
                        stop = true;
                        return;
                    }

                }
            }
        }

        //Zastosowanie efektu echo

        private void buttonEchoApply_Click(object sender, EventArgs e)
        {
            timer1.Stop();
            _soundOut.Stop();
            stop = false;

            var echo = new DmoEchoEffect(_source);
            echo.Feedback = trackBarEchoFeedback.Value;         //0-100
            echo.LeftDelay = trackBarEchoLeftDelay.Value;       //1-2000ms
            echo.RightDelay = trackBarEchoRightDelay.Value;      //1-2000ms
            if(checkBoxEchoPanDelay.Checked == true)
            {
                echo.PanDelay = true;
            }
            else
            {
                echo.PanDelay = false;
            }
            echo.WetDryMix = trackBarEchoWetDryMix.Value;        //0-100
            _soundOut = new WasapiOut();
            _soundOut.Initialize(echo);

            timer1.Start();
            _soundOut.Play();
            stop = true;
        }

        private void labelEchoUpdate()
        {
            labelEchoFeedback.Text = "Feedback: " + trackBarEchoFeedback.Value.ToString();
            labelEchoLeftDelay.Text = "Left Delay: " + trackBarEchoLeftDelay.Value.ToString() + " ms";
            labelEchoRightDelay.Text = "Right Delay" + trackBarEchoRightDelay.Value.ToString() + " ms";
            labelEchoWetDry.Text = "WetDryMix: " + trackBarEchoWetDryMix.Value.ToString() + " %";
        }

        private void trackBarEchoFeedback_Scroll(object sender, EventArgs e)
        {
            labelEchoUpdate();
        }

        private void trackBarEchoLeftDelay_Scroll(object sender, EventArgs e)
        {
            labelEchoUpdate();
        }

        private void trackBarEchoRightDelay_Scroll(object sender, EventArgs e)
        {
            labelEchoUpdate();
        }

        private void trackBarEchoWetDryMix_Scroll(object sender, EventArgs e)
        {
            labelEchoUpdate();
        }

        //Zastosowanie efektu Distortion

        private void buttonDistortionApply_Click(object sender, EventArgs e)
        {
            timer1.Stop();
            _soundOut.Stop();
            stop = false;

            var distortionEffect = new DmoDistortionEffect(_source);
            distortionEffect.Edge = trackBarDistortionEdge.Value;         //0-100
            distortionEffect.Gain = trackBarDistortionGain.Value;        //-60 - 0dB
            distortionEffect.PostEQBandwidth = trackBarDistortionBandwidth.Value;        //100-8000Hz
            distortionEffect.PostEQCenterFrequency = trackBarDistortionCenter.Value;  //100-8000Hz
            distortionEffect.PreLowpassCutoff = trackBarDistortionLowpass.Value;       //100-8000Hz
            _soundOut = new WasapiOut();
            _soundOut.Initialize(distortionEffect);

            timer1.Start();
            _soundOut.Play();
            stop = true;
        }
        private void labelDistortionUpdate()
        {
            labelDistortionEdge.Text = "Edge: " + trackBarDistortionEdge.Value.ToString() + " %";
            labelDistortionGain.Text = "Gain: " + trackBarDistortionGain.Value.ToString() + " dB";
            labelDistortionBandwidth.Text = "Post EQ Bandwidth: " + trackBarDistortionBandwidth.Value.ToString() + " Hz";
            labelDistortionCenter.Text = "Post EQ Center Frequency: " +trackBarDistortionCenter.Value.ToString() + " Hz";
            labelDistortionLowpass.Text = "Pre Lowpass Cutoff: " + trackBarDistortionLowpass.Value.ToString() + " Hz";
        }

        private void trackBarDistortionEdge_Scroll(object sender, EventArgs e)
        {
            labelDistortionUpdate();
        }

        private void trackBarDistortionGain_Scroll(object sender, EventArgs e)
        {
            labelDistortionUpdate();
        }

        private void trackBarDistortionBandwidth_Scroll(object sender, EventArgs e)
        {
            labelDistortionUpdate();
        }

        private void trackBarDistortionCenter_Scroll(object sender, EventArgs e)
        {
            labelDistortionUpdate();
        }

        private void trackBarDistortionLowpass_Scroll(object sender, EventArgs e)
        {
            labelDistortionUpdate();
        }

        //zastosowanie efektu Chorus

        private void buttonChorusApply_Click(object sender, EventArgs e)
        {
            timer1.Stop();
            _soundOut.Stop();
            stop = false;

            var chorusEffect = new DmoChorusEffect(_source);
            chorusEffect.Delay = trackBarChorusDelay.Value;        //0-20ms
            chorusEffect.Depth = trackBarChorusDepth.Value;        //0-100 
            chorusEffect.Feedback = trackBarChorusFeedback.Value;     //-99 - 99
            chorusEffect.Frequency = (float)(trackBarChorusFrequency.Value / 10.00);    //0-10
            switch(comboBoxChorusPhase.Text)
            {
                case "-180":
                    chorusEffect.Phase = ChorusPhase.PhaseNegative180;
                    break;
                case "-90":
                    chorusEffect.Phase = ChorusPhase.PhaseNegative90;
                    break;
                case "0":
                    chorusEffect.Phase = ChorusPhase.PhaseZero;
                    break;
                case "90":
                    chorusEffect.Phase = ChorusPhase.Phase90;
                    break;
                case "180":
                    chorusEffect.Phase = ChorusPhase.Phase180;
                    break;
            }
            switch(comboBoxChorusWaveform.Text)
            {
                case "Sine":
                    chorusEffect.Waveform = ChorusWaveform.WaveformSin;
                    break;
                case "Triangle":
                    chorusEffect.Waveform = ChorusWaveform.WaveformTriangle;
                    break;

            }
            chorusEffect.WetDryMix = trackBarChorusWetDryMix.Value;        //0-100%

            _soundOut = new WasapiOut();
            _soundOut.Initialize(chorusEffect);

            timer1.Start();
            _soundOut.Play();
            stop = true;
        }
        private void labelChorusUpdate()
        {
            labelChorusDelay.Text = "Delay: " + trackBarChorusDelay.Value + " ms";
            labelChorusDepth.Text = "Depth: " + trackBarChorusDepth.Value;
            labelChorusFeedback.Text = "Feedback: " + trackBarChorusFeedback.Value;
            labelChorusFrequency.Text = "Frequency: " + (float)(trackBarChorusFrequency.Value / 10.00);
            comboBoxChorusPhase.SelectedIndex = 3;
            comboBoxChorusWaveform.SelectedIndex = 0;
            labelChorusWetDryMix.Text = "WetDryMix: " + trackBarChorusWetDryMix.Value + " %";
        }
        private void trackBarChorusDelay_Scroll(object sender, EventArgs e)
        {
            labelChorusUpdate();
        }
        
        private void trackBarChorusDepth_Scroll(object sender, EventArgs e)
        {
            labelChorusUpdate();
        }

        private void trackBarChorusFeedback_Scroll(object sender, EventArgs e)
        {
            labelChorusUpdate();
        }
        private void trackBarChorusFrequency_Scroll(object sender, EventArgs e)
        {
            labelChorusUpdate();
        }

        private void trackBarChorusWetDryMix_Scroll(object sender, EventArgs e)
        {
            labelChorusUpdate();
        }
    }
}
