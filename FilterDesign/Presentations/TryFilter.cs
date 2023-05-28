using FilterDesign.Controllers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ScottPlot;
using NAudio.Wave;
using NAudio.Gui;

namespace FilterDesign.Presentations {
    partial class TryFilter : Form {
        RunFilterController controller;
        WaveIn waveIn;

        int sampleRate = 10000;
        const int chunkSize = 500;
        double[] audioData = new double[chunkSize];
        double[] original = new double[chunkSize];
        void InitializeAudioCapture() {
            waveIn = new WaveIn();
            waveIn.WaveFormat = new WaveFormat(sampleRate, 1);
            for (int i = 0; i < chunkSize; i++) audioData[i] = 0;
            magRes.Plot.AddSignal(audioData);
            formsPlot1.Plot.AddSignal(original); 
            waveIn.DataAvailable += WaveIn_DataAvailable;
        }

        void WaveIn_DataAvailable(object sender, WaveInEventArgs e) {
            for (int i = 0; i < chunkSize; i ++) {
                short sample = BitConverter.ToInt16(e.Buffer, i*2);
                audioData[i] = controller.filter.ApplyFilter(sample);
                original[i] = sample;
            }
            setAxisLimits();
            magRes.Render();
            formsPlot1.Render();
        }

        private void setAxisLimits() {
            double max = audioData.Max(), min = audioData.Min();
            double max2 = original.Max(), min2 = original.Min();
            if (min - max < max * 2)
                magRes.Plot.SetAxisLimitsY(min - max, max * 2);
            else
                magRes.Plot.SetAxisLimitsY(min - max, Math.Abs(min - max) * 2+1);

            if (min2 - max2 < max2 * 2)
                formsPlot1.Plot.SetAxisLimitsY(min2 - max2, max2 * 2);
            else
                formsPlot1.Plot.SetAxisLimitsY(min2 - max2, Math.Abs(min2 - max2) * 2 + 1);
        }

        public TryFilter(Filter f) {
            InitializeComponent();
            InitializeAudioCapture();
            controller = new RunFilterController(f,magRes); 
            formsPlot1.Plot.Title("Original Signal", color: Color.White);
        }

        private void button1_Click(object sender, EventArgs e) {
            waveIn.StartRecording();
        }

        private void TryFilter_FormClosing(object sender, FormClosingEventArgs e) {
            waveIn.StopRecording();
            waveIn.Dispose();
        }

        private void button2_Click(object sender, EventArgs e) {
            
        }

        private void button2_Click_1(object sender, EventArgs e) {
            waveIn.StopRecording();
        }
    }
}
