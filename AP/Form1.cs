using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NAudio.CoreAudioApi;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;

namespace Tutorial6
{
    public partial class Form1 : Form
    {

        List<Screen> screens = new List<Screen>();
        List<String> pictures = new List<String>();
        System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
        System.Windows.Forms.Timer screenshotTimer = new System.Windows.Forms.Timer();

        int screensTakenInRow = 0;
        public Form1()
        {
            InitializeComponent();  
            RefreshSources();

            timer.Tick += OnTimerTick;
            timer.Interval = 100;

            screenshotTimer.Tick += screenshotTimerTick;
            screenshotTimer.Interval = 1000;

            System.Windows.Forms.MessageBox.Show("Use at your own risk! (might crash, collapse, vanish etc)");
        }

        NAudio.Wave.WaveIn sourceStream = null;
        NAudio.Wave.DirectSoundOut waveOut = null;
        
        private void RefreshSources()
        {
            /* get available screens */
            screenList.Items.Clear();
            for (int i = 0; i < Screen.AllScreens.Length; i++)
            {
                screenList.Items.Add(Screen.AllScreens[i].DeviceName);
            }


            /* get available audio devices to listen */
            List<NAudio.Wave.WaveInCapabilities> sources = new List<NAudio.Wave.WaveInCapabilities>();

            for (int i = 0; i < NAudio.Wave.WaveIn.DeviceCount; i++)
            {
                sources.Add(NAudio.Wave.WaveIn.GetCapabilities(i));
            }

          
        }

        private void button1_Click(object sender, EventArgs e)
        {
            RefreshSources();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            timer.Start();

        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            MMDeviceEnumerator devEnum = new MMDeviceEnumerator();
            MMDevice defaultDevice =
              devEnum.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);

            float level = defaultDevice.AudioMeterInformation.MasterPeakValue;
            
            // show audio level 
            int newValue = (int)(level * 100);

            if (newValue > audioProgress.Maximum)
            {
                audioProgress.Value = audioProgress.Maximum;
            } else {
                audioProgress.Value = (int)(level * 100);
            }
            audioLevelText.Text = level.ToString();

            if (level > (float)audioThreshold.Value)
            {
                makeScreenshot();

                // and start screenShot timer
                screensTakenInRow = 0;
                screenshotTimer.Start();
            }

        }
        private void screenshotTimerTick(object sender, EventArgs e)
        {
            if (screensTakenInRow >= pictureCount.Value)
            {
                Console.WriteLine("Stopping");
                screenshotTimer.Stop();
            }
            screensTakenInRow++;
            makeScreenshot();
        }

        private void makeScreenshot()
        {
            Screen screen;
            if (screenList.SelectedItems.Count > 0)
            {
                screen = Screen.AllScreens[screenList.SelectedIndex];
            }
            else
            {
                // if no screen is selected, use screen 0
                screen = Screen.AllScreens[0];
            }
            

            Rectangle bounds = Screen.GetBounds(screen.Bounds);
            using (Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height))
            {
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.CopyFromScreen(screen.Bounds.X, screen.Bounds.Y, 0, 0, bounds.Size);
                }


                DateTime now = DateTime.Now;
                
                String nowFormatted = System.String.Format("{0:yyyy-MM-dd_hh-mm-ss}", now);

                // !TODO: Bubble screenshot saving so all screenshots would actually get saved
                bitmap.Save(System.String.Format("EvE_{0}.jpg", nowFormatted), ImageFormat.Jpeg);

                pictures.Add(nowFormatted.ToString());
                screenshotList.Items.Add(now);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (timer != null)
            {
                timer.Stop();
            }

            if (screenshotTimer != null)
            {
                screenshotTimer.Stop();
            }
            if (waveOut != null)
            {
                waveOut.Stop();
                waveOut.Dispose();
                waveOut = null;
            }
            if (sourceStream != null)
            {
                sourceStream.StopRecording();
                sourceStream.Dispose();
                sourceStream = null;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            button3_Click(sender, e);
            this.Close();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            makeScreenshot();
        }

        private void screenshotList_DoubleL(object sender, EventArgs e)
        {
            Console.WriteLine(pictures[screenshotList.SelectedIndex]);
            try
            {
                System.Diagnostics.Process.Start(System.String.Format("EvE_{0}.jpg", pictures[screenshotList.SelectedIndex]));
            }
            catch (Exception err)
            {
                Console.WriteLine(err);
            }
        }

        private void audioThreshold_ValueChanged(object sender, EventArgs e)
        {
            audioProgress.Maximum = (int)(audioThreshold.Value * 100);
        }
    }
}
