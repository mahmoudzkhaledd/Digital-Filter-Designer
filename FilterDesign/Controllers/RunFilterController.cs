using ScottPlot;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace FilterDesign.Controllers {
    internal class RunFilterController {
        public  Filter filter;
        FormsPlot plot;
        int curr = 0;
        const int chunkSize = 500;
        double[] sinWave;

        double[] values = new double[chunkSize];
        public RunFilterController(Filter f,FormsPlot plt) {
            filter = f;
            this.plot = plt;
            sinWave = GenerateSineWave(1000,10);
            setupPlot();
            filter.initFilter();
        }
        void setupPlot() {
            //plot.Configuration.Pan = false;
            //plot.Configuration.Zoom = false;
            plot.Plot.Title("Filtered Signal", color: Color.White);
            plot.Plot.XAxis.Color(Color.White);
            plot.Plot.YAxis.Color(Color.White);
            plot.Plot.AddSignal(values);
            plot.Refresh();
        }

        double[] GenerateSineWave(double frequency, double duration) {
            int sampleRate = 44100; // Sample rate in Hz
            int numSamples = (int)(duration * sampleRate);

            double[] samples = new double[numSamples];

            double amplitude = 0.5;
            double angularFrequency = 2 * Math.PI * frequency;

            for (int i = 0; i < numSamples; i++) {
                double time = (double)i / sampleRate;
                samples[i] = amplitude * Math.Sin(angularFrequency * time);
            }

            return samples;
        }

        public void timerTick() {
            if (curr >= sinWave.Length) return;
            for(int i=0;i < chunkSize; i++) {
                values[i] = filter.ApplyFilter(sinWave[curr]);
                curr++;
            }
            setAxisLimits();
            plot.Render();
        }
        private void setAxisLimits() {
            double max = values.Max(), min = values.Min();
            if (min - max < max * 2)
                plot.Plot.SetAxisLimitsY(min - max, max * 2);
            else
                plot.Plot.SetAxisLimitsY(min - max, max * 2 + 1);
        }
    }
}
