using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GoogleAnalytics
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void SendEvent_Click(object sender, EventArgs e)
        {
            GoogleAnalytics.Add(new GAEvent("WORD", "OPEN"));
            GoogleAnalytics.Dispatch();
        }
    }
}
