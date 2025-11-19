using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SpineModeling;
using SpineAnalyzer;
using SpineAnalyzer.SkeletalModeling;

namespace SpineModeling
{
    public partial class btnmuscular : Form
    {
        public btnmuscular()
        {
            InitializeComponent();
        }

        private void btnImageAnalysis_Click(object sender, EventArgs e)
        {

        }

        private void btnMS_Click(object sender, EventArgs e)
        {

        }

        private void btnCompareModels_Click(object sender, EventArgs e)
        {

        }

        private void btnPerturbator_Click(object sender, EventArgs e)
        {

        }

        private void btnSkeletal_Click(object sender, EventArgs e)
        {
            frmImageAnalysis_new frmImageAnalysis = new frmImageAnalysis_new();
            //frmImageAnalysis frmImageAnalysis = new frmImageAnalysis();
            //frmImageAnalysis.Subject = Subject;
            //frmImageAnalysis.AppData = AppData;
            //frmImageAnalysis.SQLDB = SQLDB;
            frmImageAnalysis.ShowDialog();

        }
    }
}
