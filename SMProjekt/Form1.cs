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
using System.IO;

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
        private bool endoffile = false;

        public Form1()
        {
            InitializeComponent();
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
            }
            PlayFileAudio();
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
                ISampleSource source = soundInSource.ToSampleSource();
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
            ISampleSource source = CodecFactory.Instance.GetCodec(pathtoFile).ToSampleSource();
            SetupSampleSource(source);

            //play the audio
            _soundOut = new WasapiOut();
            _soundOut.Initialize(_source);
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

            using (var reader1 = new NAudio.Wave.AudioFileReader(pathtoMergeFile1))
            using (var reader2 = new NAudio.Wave.AudioFileReader(pathtoMergeFile2))
            {
                var mixer = new NAudio.Wave.SampleProviders.MixingSampleProvider(new[] { reader1, reader2 });
                NAudio.Wave.WaveFileWriter.CreateWaveFile16("C:\\Users\\wojte\\source\\repos\\SMProjekt\\result.wav", mixer);
            }

        }
    }
}
