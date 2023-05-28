using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FilterDesign.Presentations
{
    partial class ChooseForm : Form
    {
        Filter filter;
        public ChooseForm(Filter f)
        {
            InitializeComponent();
            filter = f;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (TryFilter tf = new TryFilter(filter)) {
                tf.ShowDialog();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            using (Frem_File tf = new Frem_File(filter))
            {
                tf.ShowDialog();
            }
        }
    }
}
