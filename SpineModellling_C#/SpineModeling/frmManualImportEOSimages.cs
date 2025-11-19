using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SpineAnalyzer
{
    public partial class frmManualImportEOSimages : Form
    {
        public string file1 = string.Empty;

        public string file2 = string.Empty;

        public frmManualImportEOSimages()
        {
            InitializeComponent();
        }

        private void frmManualImportEOSimages_Load(object sender, EventArgs e)
        {
            Application.UseWaitCursor = false;
        }



        private void run(out string fileSourcePath)
        {
            fileSourcePath = string.Empty;
            if (vistaOpenFileDialog1.ShowDialog(this) == DialogResult.OK)
            {

                string[] filepaths = vistaOpenFileDialog1.FileNames;
                int numberFiles = filepaths.Count<string>();

                if (numberFiles == 1)
                {
                    fileSourcePath = vistaOpenFileDialog1.FileName;
               

                }

               

            }
        }

        private void btnFile1_Click(object sender, EventArgs e)
        {
            string fileSourcePath;
            run(out fileSourcePath);
         
            if (!string.IsNullOrEmpty(fileSourcePath))
            {
                txtFileName1.Text = fileSourcePath;
                btnFile1.Enabled = false;
            }
            CheckCompletion();

        }

        private void btnFile2_Click(object sender, EventArgs e)
        {
            string fileSourcePath;
            run(out fileSourcePath);
            if(!string.IsNullOrEmpty(fileSourcePath))
            {
                txtFileName2.Text = fileSourcePath;
                btnFile2.Enabled = false;
            }
            CheckCompletion();



        }

        private void CheckCompletion()
        {
            if(!btnFile1.Enabled && !btnFile2.Enabled)
            {
                btnConfirm.Visible = true;
            }

        }

        private void btnConfirm_Click(object sender, EventArgs e)
        {
            file1 = txtFileName1.Text;
            file2 = txtFileName2.Text;

            this.Close();
        }
    }
}
