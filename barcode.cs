using AForge.Video.DirectShow;
using System;
using System.Drawing;
using System.Windows.Forms;
using ZXing;

namespace LibraryManagementSystem
{
    public partial class barcode : Form
    {
        private FilterInfoCollection FilterInfoc;
        private VideoCaptureDevice captureDevice;
        private bool isCapturing;

        public barcode()
        {
            InitializeComponent();
            backgroundWorker1.DoWork += BackgroundWorker1_DoWork;
            backgroundWorker1.RunWorkerCompleted += BackgroundWorker1_RunWorkerCompleted;
        }

        private void barcode_Load(object sender, EventArgs e)
        {
            FilterInfoc = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            foreach (FilterInfo filterInfo in FilterInfoc)
                comboBox1.Items.Add(filterInfo.Name);
            comboBox1.SelectedIndex = 0;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!isCapturing)
            {
                captureDevice = new VideoCaptureDevice(FilterInfoc[comboBox1.SelectedIndex].MonikerString);
                captureDevice.NewFrame += CaptureDevice_NewFrame;
                captureDevice.Start();
                isCapturing = true;
                button1.Text = "Stop";
            }
            else
            {
                StopCapture();
                button1.Text = "Start";
            }
        }

        private void CaptureDevice_NewFrame(object sender, AForge.Video.NewFrameEventArgs eventArgs)
        {
            if (isCapturing)
            {
                Bitmap bitmap = (Bitmap)eventArgs.Frame.Clone();
                backgroundWorker1.RunWorkerAsync(bitmap);
            }
        }

        private void BackgroundWorker1_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            Bitmap bitmap = (Bitmap)e.Argument;
            BarcodeReader barcodeReader = new BarcodeReader();
            var result = barcodeReader.Decode(bitmap);
            e.Result = result;
        }

        private void BackgroundWorker1_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                textBox1.Text = e.Result.ToString();
            }
        }

        private void StopCapture()
        {
            if (captureDevice != null)
            {
                if (captureDevice.IsRunning)
                {
                    captureDevice.SignalToStop();
                    captureDevice.WaitForStop();
                }
                captureDevice = null;
                isCapturing = false;
            }
        }

        private void barcode_FormClosing(object sender, FormClosingEventArgs e)
        {
            StopCapture();
        }
    }
}
