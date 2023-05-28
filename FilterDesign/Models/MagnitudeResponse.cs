using ScottPlot;
using ScottPlot.Plottable;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FilterDesign.Models {
    internal class MagnitudeResponse {
        public List<Tuple<double, double>> circlePoints = new List<Tuple<double, double>>();
        const int chunkSize = 32;
        public double[] values = new double[chunkSize];
        public double[] phase = new double[chunkSize];
       // public string[] xAxis = new string[chunkSize];
        public string valuesStr { get => string.Join(",", values);set {
                String[] val = value.Split('=');
                String[] x = val[1].Split(',');
                for (int i = 0; i < chunkSize; i++) {
                    values[i] = Convert.ToDouble(x[i]);
                }
            } }
        FormsPlot plot;
        public MagnitudeResponse(FormsPlot plt) {
            this.plot = plt;
            initCirclePoints();
            setupPlot();
          
        } 

        void initCirclePoints() {
            double angle = 0.0;
            int c = 0;
            while (angle <= Math.PI) {
                values[c] = 1;
                double x = Math.Cos(angle);
                double y = Math.Sin(angle);
                circlePoints.Add(new Tuple<double, double>(x, y));
                //xAxis[c] = angle.ToString();
                angle += 0.1;
                c++;
            }
        }
        
        void setupPlot() {
            plot.Plot.AddSignal(values);
            plot.Configuration.Pan = false;
            plot.Configuration.Zoom = false;
            plot.Plot.SetAxisLimitsX(0, chunkSize);
            
            plot.Plot.Title("Magnitude Response", color: Color.White);
            plot.Plot.XAxis.Color(Color.White);
            plot.Plot.YAxis.Color(Color.White);
            plot.Refresh();
        }

        public void clear() {
            plot.Plot.Clear();
            for (int i = 0; i < chunkSize; i++) values[i] = 0;
            plot.Plot.AddSignal(values);
            plot.Refresh();
        }

        public void refresh() {
            setAxisLimits();
            plot.Refresh();
        }

        public double[] updateFrame(Filter ff) {
            int c = 0;
            foreach (Tuple<double, double> point in circlePoints) {
                double dis = 1;
                double ph = 0;
                for (int i = 0; i < ff.zeros.Count; i++) {
                    DraggableMarkerPlot curr = ff.zeros[i];
                    double x = curr.X, y = curr.Y;
                    dis *= Math.Sqrt(Math.Pow(point.Item1 - x, 2) + Math.Pow(point.Item2 - y, 2));
                    ph += Math.Atan2(point.Item1 - x, point.Item2 - y);    
                }
                for (int i = 0; i < ff.poles.Count; i++) {
                    DraggableMarkerPlot curr = ff.poles[i];
                    double x = curr.X, y = curr.Y;
                    double val = Math.Sqrt(Math.Pow(point.Item1 - x, 2) + Math.Pow(point.Item2 - y, 2));
                    if (val == 0) val = 0.01;
                    dis /= val;
                    ph -= Math.Atan2(point.Item1 - x, point.Item2 - y);
                }
                
                foreach (var fil in ff.allPassFilters) {
                    for (int i = 0; i < fil.zeros.Count; i++)
                    {
                        DraggableMarkerPlot curr = fil.zeros[i];
                        double x = curr.X, y = curr.Y;
                        dis *= Math.Sqrt(Math.Pow(point.Item1 - x, 2) + Math.Pow(point.Item2 - y, 2));
                        ph += Math.Atan2(point.Item1 - x, point.Item2 - y);
                    }
                    for (int i = 0; i < fil.poles.Count; i++)
                    {
                        DraggableMarkerPlot curr = fil.poles[i];
                        double x = curr.X, y = curr.Y;
                        double val = Math.Sqrt(Math.Pow(point.Item1 - x, 2) + Math.Pow(point.Item2 - y, 2));
                        if (val == 0) val = 0.01;
                        dis /= val;
                        ph -= Math.Atan2(point.Item1 - x, point.Item2 - y);
                    }
                }
                phase[c] = ph*20;
                values[c] = dis;
                c++;
            }
            setAxisLimits();
            plot.Render();
            return values;
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
