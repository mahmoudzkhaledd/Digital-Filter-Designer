using FilterDesign.Models;
using NAudio.Gui;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ScottPlot;
using ScottPlot.Drawing.Colormaps;
using ScottPlot.Plottable;
using ScottPlot.Styles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FilterDesign.Controllers {
    internal class DesignPageController {
        public Filter filter;
        UnitCircle unitCircle;
        MagnitudeResponse magRes;
        FormsPlot phase;
        double[] phaseRes = new double[32];
        bool allPass = false;
        public DesignPageController(UnitCircle circle, MagnitudeResponse magRes, FormsPlot phase,bool allPass = false) {
            this.unitCircle = circle;
            this.allPass = allPass;
            this.magRes = magRes;
            filter = new Filter();
            unitCircle.plot.PlottableDragged += dragPointEvent;
            this.phase = phase;
            if(phase != null)
                phase.Plot.AddSignal(magRes.phase);
        }
        public void clearAll() {
            filter.clearAll();
            unitCircle.clear();
            magRes.clear();
            update();
        }
        void plotFilterZerosPoles(Filter f) {
            foreach (var fi in f.zeros)
            {
                //unitCircle.drawPoint(fi.X, fi.Y, false, false);
                unitCircle.plot.Plot.Add(fi);
            }
            foreach (var fi in f.poles)
            {
                unitCircle.plot.Plot.Add(fi);
                //unitCircle.drawPoint(fi.X, fi.Y, true, false);
            }
        }
        public void update() {
            updatePhase();
            unitCircle.plot.Plot.Clear();
            unitCircle. plot.Plot.AddEllipse(0, 0, 1, 1); 
            plotFilterZerosPoles(filter);
            foreach(Filter f in filter.allPassFilters) {
                plotFilterZerosPoles(f);
            }
            updateFremeMagRes();
            unitCircle.plot.Refresh();
        }
        public void removeAllPoles() {
            foreach (var pole in filter.poles) {
                unitCircle.plot.Plot.Remove(pole);
            }
            
            filter.removeAllPoles();
            unitCircle.removeAllPoles();

            update();
        }
        public void removeAllZeros() {
            foreach (var zero in filter.zeros) {
                unitCircle.plot.Plot.Remove(zero);
            }
            filter.removeAllZeros();
            unitCircle.removeAllZeros();
            update();
        }
        public DraggableMarkerPlot addPoint(double x, double y, bool isPole, bool isConject) {
            
            (DraggableMarkerPlot point1, DraggableMarkerPlot point2) = unitCircle.drawPoint(x, y, isPole, isConject);
            if (point1 == null) return null;
            if (isPole) {
                filter.poles.Add(point1);
                if (point2 != null) filter.poles.Add(point2);
            }
            else {
                filter.zeros.Add(point1);
                if (point2 != null) filter.zeros.Add(point2);
            }
            updateFremeMagRes();
            updatePhase();
            unitCircle.plot.Refresh();
            return point1;
        }

        void updatePhase() {
            if (phase != null) {
                setAxisLimits();
                phase.Render();
            }
        }
        void setAxisLimits() {
            double max = magRes.phase.Max(), min = magRes.phase.Min();
            if (min - max < max * 2)
                phase.Plot.SetAxisLimitsY(min - max, max * 2);
            else
                phase.Plot.SetAxisLimitsY(min - max, max * 2 + 1);
        }
        public void dragPointEvent(object sender, EventArgs e) {
            unitCircle.dragPointEvent();
            updateFremeMagRes();
            updatePhase();
        }

        internal void setCurrentFilter(Filter filter)
        {
            this.filter = filter;
            unitCircle.clear();
            foreach(var point in filter.zeros) {
                unitCircle.drawPoint(point.X,point.Y, false, false);
            }
            foreach (var point in filter.poles) {
                unitCircle.drawPoint(point.X, point.Y, true, false);
            }
            update();
        }

        public double[] calculateMagResValues(List<DraggableMarkerPlot> zeros, List<DraggableMarkerPlot> poles) {
            List<double> values = new List<double>();
            int c = 0;
            foreach (Tuple<double, double> point in magRes.circlePoints) {
                double dis = 1;
                for (int i = 0; i < zeros.Count; i++) {
                    DraggableMarkerPlot curr = zeros[i];
                    double x = curr.X, y = curr.Y;
                    dis *= Math.Sqrt(Math.Pow(point.Item1 - x, 2) + Math.Pow(point.Item2 - y, 2));
                }
                for (int i = 0; i < poles.Count; i++) {
                    DraggableMarkerPlot curr = poles[i];
                    double x = curr.X, y = curr.Y;
                    double val = Math.Sqrt(Math.Pow(point.Item1 - x, 2) + Math.Pow(point.Item2 - y, 2));
                    if (val == 0) val = 0.01;
                    dis /= val;
                }
                values.Add(dis);
                c++;
            }
            return values.ToArray();
        }

        public void updateFremeMagRes() {
            magRes.updateFrame(filter);
        }

        public void deletePoint(DraggableMarkerPlot point) {
            if (point.Label == "Zero") {
                filter.removeZero(point);
                if (unitCircle.conjects.ContainsKey(point))
                    filter.zeros.Remove(unitCircle.conjects[point]);
            }
            else {
                filter.removePole(point);

                if (unitCircle.conjects.ContainsKey(point))
                    filter.poles.Remove(unitCircle.conjects[point]);
            }
            unitCircle.removePoint(point);
            update();
        }
        public void exportFilter() {
            String strFilter = JsonConvert.SerializeObject(filter);
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "Json Files | *.json";
            DialogResult res = dialog.ShowDialog();
            if (res == DialogResult.OK) {
                File.WriteAllText(dialog.FileName, strFilter);
            }
        }
        public void refreshFromFilter(Filter tmpFilter) {
            int zC = tmpFilter.zeros.Count, pC = tmpFilter.poles.Count;
            for (int i = 0; i < zC; i++) {
                DraggableMarkerPlot dp = tmpFilter.zeros[i];
                addPoint(dp.X, dp.Y, false, false);
            }
            for (int i = 0; i < pC; i++) {
                DraggableMarkerPlot dp = tmpFilter.poles[i];
                addPoint(dp.X, dp.Y, true, false);
            }
        }
        internal void loadFilter() {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Json Files | *.json";
            DialogResult res = dialog.ShowDialog();
            if (res == DialogResult.OK) {
                String filterStr = File.ReadAllText(dialog.FileName);
                Filter tmpFilter = JsonConvert.DeserializeObject<Filter>(filterStr);
                refreshFromFilter(tmpFilter);
            }
        }

        public void exportMagValues() {
            String fil = JsonConvert.SerializeObject(filter);
            File.WriteAllText($"library/file_{DateTime.Now.Second.ToString()}.json", fil);
            MessageBox.Show("FIlter Exported To Library");
        }

        

        public void addAllPassFilter(Filter f) {
            filter.allPassFilters.Add(f);
            //refreshFromFilter(f);
            update();
        }

    }
}
