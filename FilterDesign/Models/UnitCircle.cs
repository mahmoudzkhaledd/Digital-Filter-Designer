using ScottPlot;
using ScottPlot.Plottable;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilterDesign.Models {
    internal class UnitCircle {
        public FormsPlot plot;
        public Dictionary<DraggableMarkerPlot, DraggableMarkerPlot> conjects ;
        bool allPass = false;
        public UnitCircle(FormsPlot plt,bool all = false) {
            conjects = new Dictionary<DraggableMarkerPlot, DraggableMarkerPlot>();
            plot = plt;
            allPass = all;
            setpuPlot();
        }
        void setpuPlot() {
            plot.ResetText();
            plot.Plot.AddEllipse(0, 0, 1, 1);
            plot.Plot.Title("Unit Circle",color:Color.White);
            plot.Plot.XAxis.Color(Color.White);
            plot.Plot.YAxis.Color(Color.White);
            plot.Refresh();
        }
        public (DraggableMarkerPlot, DraggableMarkerPlot) drawPoint(double x, double y,bool isPloe,bool isConject) {
            (double xMouse, double yMouse) = plot.GetMousePixel();
            IDraggable pointMouse = plot.Plot.GetDraggable(xMouse, yMouse);
            if(pointMouse != null && !allPass) {
                return (null,null);
            }
            if (isPloe) {
                if (x == 1) x = 0.9999;
                if (y == 1) y = 0.9999;
                DraggableMarkerPlot point = plot.Plot.AddMarkerDraggable(x, y, MarkerShape.asterisk, 20,color:Color.Orange);
                
                point.Label = "Pole";
                if (isConject) {
                    DraggableMarkerPlot point2 = plot.Plot.AddMarkerDraggable(x, -y, MarkerShape.asterisk, 20, color: Color.Orange);
                    
                    point.Label = "Pole";
                    conjects.Add(point, point2);
                    conjects.Add(point2, point);
                    plot.Refresh();
                    return (point, point2);
                }
                else {
                    plot.Refresh();
                    return (point,null);
                }
            }
            else {
                DraggableMarkerPlot point = plot.Plot.AddMarkerDraggable(x, y, MarkerShape.filledCircle, 20, color: Color.Orange);
                point.Label = "Zero";
                if (isConject) {
                    DraggableMarkerPlot point2 = plot.Plot.AddMarkerDraggable(x, -y, MarkerShape.filledCircle, 20, color: Color.Orange);
                    point.Label = "Zero";
                    conjects.Add(point, point2);
                    conjects.Add(point2, point);
                    plot.Refresh();
                    return (point, point2);
                } else {
                    plot.Refresh();
                    return (point,null);
                }
            }
        }
        public void clear() {
            plot.Plot.Clear();
            conjects.Clear();
            plot.Plot.AddEllipse(0, 0, 1, 1);
            plot.Refresh();
        }
        public void removeAllPoles() {
            for (int i = 0; i < conjects.Count; i++) {
                if (conjects.ElementAt(i).Key.Label == "Pole") {
                    conjects.Remove(conjects.ElementAt(i).Key);
                }
            }
        }
        public void removeAllZeros() {
            for(int i = 0; i < conjects.Count; i++) {
                if (conjects.ElementAt(i).Key.Label == "Zero") {
                    conjects.Remove(conjects.ElementAt(i).Key);
                }
            }
        }
        public void dragPointEvent() {
            (double x, double y) = plot.GetMousePixel();
            DraggableMarkerPlot d = (DraggableMarkerPlot)plot.Plot.GetDraggable(x, y);
            if (d != null && conjects.ContainsKey(d)) {
                if (!allPass) {
                    conjects[d].DragTo(d.X, -d.Y, true);
                }
                else {
                    double r = d.X * d.X + d.Y * d.Y;
                    conjects[d].DragTo(d.X/r, d.Y/r, true);
                }
            }
        }

        public void removePoint(DraggableMarkerPlot point) {
            plot.Plot.Remove(point);
            if (conjects.ContainsKey(point)) {
                conjects.Remove(conjects[point]);
                plot.Plot.Remove(conjects[point]);
                conjects.Remove(point);
            }
            plot.Refresh();
        }
    }
}
