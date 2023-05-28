using FilterDesign.Controllers;
using FilterDesign.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ScottPlot.Drawing.Colormaps;
using ScottPlot.Plottable;
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
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace FilterDesign.Presentations {
    partial class AllPassLibrary : Form {
        const int chunkSize = 32;
        public  DesignPageController controller;
        List<Filter> filters = new List<Filter>();
        MagnitudeResponse magResponse;
        UnitCircle uC;
        
        bool ctrl = false;
        public AllPassLibrary() {
            InitializeComponent();
            magResponse = new MagnitudeResponse(magRes);
            uC = new UnitCircle(unitCircle,true);
            controller = new DesignPageController(uC,magResponse,null,true);
            unitCircle.LeftClicked += clickEvent;
            loadFilters(); 
        }
        
        private void loadFilters() {
            string[] filePaths = Directory.GetFiles(@"library/");
            ImageList imList = new ImageList();
            listView1.LargeImageList = imList;
            ScottPlot.Plot plt = new ScottPlot.Plot();
            imList.ImageSize = new Size(256,256);
            for(int i = 0; i < filePaths.Length; i++) {
                String fil = File.ReadAllText(filePaths[i]);
                Filter f = JsonConvert.DeserializeObject<Filter>(fil);
                plt.Clear();
                double[] signal = controller.calculateMagResValues(f.zeros, f.poles);
                plt.AddSignal(signal);
                using (MemoryStream st = new MemoryStream(plt.GetImageBytes())) {
                    imList.Images.Add(i.ToString(),new Bitmap(st));
                    var lst = listView1.Items.Add($"Filter ${i+1}");
                    lst.ImageKey = i.ToString();
                }
                filters.Add(f);
            }
        }

        private void clickEvent(object sender, EventArgs e) {
            (double xPixel, double yPixel) = unitCircle.GetMousePixel();
            DraggableMarkerPlot clicked = (DraggableMarkerPlot)unitCircle.Plot.GetDraggable(xPixel, yPixel);
            if (!chkDelete.Checked && !ctrl) {
                (double x, double y) = unitCircle.GetMouseCoordinates();
                if (clicked == null) {
                    double r = x * x + y * y;
                    DraggableMarkerPlot p1 = controller.addPoint(x, y, pole.Checked, false);
                    DraggableMarkerPlot p2 = controller.addPoint(x/r, y/r, !pole.Checked, false);
                    uC.conjects.Add(p1,p2);
                    uC.conjects.Add(p2,p1);
                   // uC.drawPoint(x, y, pole.Checked, conjectChk.Checked);
                    //uC.drawPoint(x/r, y/r, !pole.Checked, conjectChk.Checked);
                }
            }
            else if (clicked != null && (chkDelete.Checked || ctrl)) {
                controller.deletePoint(clicked);
            }

        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e) {
            if(listView1.SelectedIndices.Count != 0) {
                controller.setCurrentFilter(filters[listView1.SelectedIndices[0]]);
            }
        }

        private void button1_Click(object sender, EventArgs e) {
            this.DialogResult = DialogResult.OK;
            
            Close();
        }

        private void AllPassLibrary_KeyDown(object sender, KeyEventArgs e) {
            ctrl = e.KeyValue == 17;
            if(chkDelete.Checked != ctrl)
                chkDelete.Checked = ctrl;
        }

        private void AllPassLibrary_KeyUp(object sender, KeyEventArgs e) {
            ctrl = false;
        }

        private void button3_Click(object sender, EventArgs e) {

        }
    }
}
