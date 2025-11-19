using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SpineAnalyzer.SkeletalModeling
{
    public partial class frmLogsAndMessages : Form
    {
        public string LogsAndMessages;


        public frmLogsAndMessages()
        {
            InitializeComponent();
        }

        private void frmLogsAndMessages_Load(object sender, EventArgs e)
        {
            richTextBox1.Text = LogsAndMessages;
            richTextBox1.SelectionStart = richTextBox1.Text.Length;
            // scroll it automatically
            richTextBox1.ScrollToCaret();
        }
    }
}
