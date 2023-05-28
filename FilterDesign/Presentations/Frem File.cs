using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FilterDesign.Presentations
{
    partial class Frem_File : Form
    {
        string[] numbers;
        const int chunkSize = 200;
        double[] values = new double[chunkSize];
        double[] filtered = new double[chunkSize];
        bool start = false;
        int currIdx = 0;
        Filter filter;
        public Frem_File(Filter f)
        {
            InitializeComponent();
            filter = f;
            for(int i = 0; i < chunkSize; i++) 
                values[0] = 0;
            
            magRes.Plot.AddSignal(values);
            formsPlot1.Plot.AddSignal(filtered);
            magRes.Refresh();
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog file = new OpenFileDialog();
            file.Filter = "CSV Files |*.csv";
            if(file.ShowDialog() == DialogResult.OK) {
                textBox1.Text = file.FileName;
                String x = File.ReadAllText(file.FileName);
                numbers = x.Split('\n');
            }
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            timer1.Interval = trackBar1.Maximum - trackBar1.Value + trackBar1.Minimum;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (currIdx < numbers.Length) {
                for (int i = 0; i < chunkSize; i++) {
                    if (currIdx < numbers.Length) {
                        string xx = numbers[currIdx].Trim('\r');
                        if (xx.Length > 0) {
                            double num = Convert.ToDouble(xx);
                            values[i] = num;
                            filtered[i] = filter.ApplyFilter(num);
                            currIdx++;
                        }
                    }
                    else break;
                }
                setAxisLimits();
                magRes.Render();
                formsPlot1.Render();
            }
            else 
                currIdx = 0;
        }
        void setAxisLimits()
        {
            double max = values.Max(), min = values.Min();
            double mm = min - max, xx = max * 2;
            if (mm < xx) {
                magRes.Plot.SetAxisLimitsY(min - max, max * 2);
            }
            else {
                magRes.Plot.SetAxisLimitsY(mm, Math.Abs(xx + mm));
            }
            double max2 = filtered.Max(), min2 = filtered.Min();
            double mm2 = min2 - max2, xx2 = max2 * 2;
            if (mm2 < xx2) {
                formsPlot1.Plot.SetAxisLimitsY(mm2, xx2);
            }
            else {
                formsPlot1.Plot.SetAxisLimitsY(mm2, Math.Abs(xx2 + mm2));
            }
        }
        private void button2_Click(object sender, EventArgs e)
        {
            if(numbers != null) {
                if (!start) timer1.Start();
                else timer1.Stop();
                start = !start;
                button2.Text =  start ? "Pause":"Start";
            }
        }
    }
}
