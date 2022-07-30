using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Vector;
using emiLab.Lib.ImageLib;
using Accord.MachineLearning;

namespace Vector_KMeans_test
{
    public partial class Form1 : Form
    {
        Robot robot;
        Bitmap inputBitmap;
        double[] arrayImage;
        double[][] data;
        int contatore, maxSample, NClusters;
        KMeans kmean;
        double[][] centroids;

        public Form1()
        {
            InitializeComponent();
            maxSample = 100;
            robot = new Robot();
            inputBitmap = new Bitmap(1280, 640);
            arrayImage = new double[2048];
            data = new double[maxSample][];
            contatore = 0;
            NClusters = 16;
            centroids = new double[NClusters][];
        }

        private async void connectToVectorToolStripMenuItem_Click(object sender, EventArgs e)
        { 
            await robot.ConnectAsync("A1N3");
            robot.SuppressPersonalityAsync().ThrowFeedException();
            await robot.WaitTillPersonalitySuppressedAsync();
            await robot.Audio.SayTextAsync("all done");
            robot.Camera.CameraFeedAsync().ThrowFeedException();

            robot.Camera.OnImageReceived += (os, oe) =>
            {
                inputBitmap = (Bitmap)oe.Image;
                ImageProc.Resize(ref inputBitmap, 64, 32);
                ImageProc.Gray(ref inputBitmap);
                arrayImage = new double[2048];
                for (int j = 0; j < inputBitmap.Height; j++)
                    for (int i = 0; i < inputBitmap.Width; i++)
                    {
                        var pixel = inputBitmap.GetPixel(i, j);
                        arrayImage[j * inputBitmap.Width + i] = (pixel.R + pixel.G + pixel.B) / 3;
                    }

                if (contatore < maxSample)
                {
                    data[contatore] = arrayImage;
                    contatore++;
                    Console.WriteLine(contatore);
                }

                pictureBox1.Invalidate();
            };
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawImage(inputBitmap, 0, 0, Width, Height);
        }

        private void saveResultsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog pippo = new SaveFileDialog();
            pippo.ShowDialog();

            int size = (int)(Math.Sqrt(NClusters));
            Bitmap results = new Bitmap(size * inputBitmap.Width, size * inputBitmap.Height);

            int i, j, jj, ii;
            for (int k = 0; k < centroids.GetLength(0); k++)
            {
                j = k / size;
                i = k % size;

                for (int kk = 0; kk < centroids[k].GetLength(0); kk++)
                {
                    jj = kk / 64;
                    ii = kk % 64;
                    Color c = Color.FromArgb((int)centroids[k][kk],(int)centroids[k][kk], (int)centroids[k][kk]);
                    results.SetPixel(ii + i * 64, jj + j * 32, c);
                }
            }
            results.Save(pippo.FileName);
        }

        private void kMeansToolStripMenuItem_Click(object sender, EventArgs e)
        {
            kmean = new KMeans(NClusters);
            KMeansClusterCollection clustersKmeans = kmean.Learn(data);
            int[] labels = clustersKmeans.Decide(data);
            centroids = kmean.Centroids;
        }

        private void disconnectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (robot.IsConnected)
            {
                robot.DisconnectAsync();
                Application.Exit();
            }
        }
    }
}
