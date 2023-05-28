using FilterDesign.Controllers;
using FilterDesign.Models;
using FilterDesign.Presentations;
using ScottPlot;
using ScottPlot.Drawing.Colormaps;
using ScottPlot.Plottable;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Input;

namespace FilterDesign {
    public partial class DesignPage : Form {
        bool ctrl = false;
        DesignPageController controller;
        UnitCircle circle;
        MagnitudeResponse magnitudeResponse;
         
        public DesignPage() {
            InitializeComponent();
            circle = new UnitCircle(unitCircle);
            magnitudeResponse = new MagnitudeResponse(magRes);
            controller = new DesignPageController(circle, magnitudeResponse,formsPlot1);
            unitCircle.LeftClicked += clickEvent;


            formsPlot1.Plot.Title("Phase Response",color: Color.White);
            formsPlot1.Plot.XAxis.Color(Color.White);
            formsPlot1.Plot.YAxis.Color(Color.White);
            formsPlot1.Refresh();
        }
        private void clickEvent(object sender, EventArgs e) {
            (double xPixel, double yPixel) = unitCircle.GetMousePixel();
            DraggableMarkerPlot clicked = (DraggableMarkerPlot)unitCircle.Plot.GetDraggable(xPixel, yPixel);
            if (!chkDelete.Checked && !ctrl) {
                (double x, double y) = unitCircle.GetMouseCoordinates();
                if (clicked == null) {
                    controller.addPoint(x, y, pole.Checked, conjectChk.Checked);
                    //circle.drawPoint(x, y, pole.Checked, conjectChk.Checked);
                }
            }
            else if(clicked != null && (chkDelete.Checked || ctrl)) {
                controller.deletePoint(clicked);
            }
            
        }
        
        private void button1_Click(object sender, EventArgs e) {
            controller.addPoint(Convert.ToDouble(textBox1.Text),Convert.ToDouble(textBox2.Text),pole.Checked,conjectChk.Checked);
            updateFilter();
        }

        private void button3_Click(object sender, EventArgs e) {
            controller.clearAll();
            updateFilter();
        }

        private void button2_Click(object sender, EventArgs e) {
            controller.removeAllZeros();
            updateFilter();
        }

        private void button4_Click(object sender, EventArgs e) {
            controller.removeAllPoles();
            updateFilter();
        }

        private void DesignPage_KeyDown(object sender, KeyEventArgs e) {
            ctrl = (e.KeyValue == 17);
            chkDelete.Checked = ctrl;
        }

        private void DesignPage_KeyUp(object sender, KeyEventArgs e) {
            ctrl = false;
            if (chkDelete.Checked != ctrl)
                chkDelete.Checked = ctrl;
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e) {
            Application.Exit();
        }

        private void exportFilterToolStripMenuItem_Click(object sender, EventArgs e) {
            controller.exportFilter();
        }

        private void importFilterToolStripMenuItem_Click(object sender, EventArgs e) {
            controller.loadFilter();
        }

        private void addToAllPassFiltersLibraryToolStripMenuItem_Click(object sender, EventArgs e) {
            controller.exportMagValues();
        }

        private void button5_Click(object sender, EventArgs e) {
            AllPassLibrary l = new AllPassLibrary();
            DialogResult dr = l.ShowDialog();
            if(dr == DialogResult.OK && l.controller.filter != null) {
                controller.addAllPassFilter(l.controller.filter);
                updateFilter();
            }

        }

        private void button6_Click(object sender, EventArgs e) {
            controller.filter.initFilter();
            ChooseForm cf = new ChooseForm(controller.filter);
            cf.ShowDialog();
        }
        void updateFilter() {
            controller.filter.initFilter();
            filterEqn.Text = controller.filter.filterEquation();
            listBox1.Items.Clear();
            for(int i=0;i<controller.filter.allPassFilters.Count;i++) {
                listBox1.Items.Add($"All-Pass {i}");
            }
        }
        private void unitCircle_MouseUp(object sender, MouseEventArgs e) {
            updateFilter();
        }
        
        private void button7_Click(object sender, EventArgs e) {
            if(listBox1.Items.Count > 0) {
                if (MessageBox.Show("Are you sure you want to delete the filter?", "Warning", MessageBoxButtons.OKCancel) == DialogResult.OK) {
                    controller.filter.allPassFilters.RemoveAt(listBox1.SelectedIndex);
                    updateFilter();
                    controller.update();
                }
            }
        }

        private void DesignPage_Resize(object sender, EventArgs e) {
            formsPlot1.Refresh();
        }
    }
}
