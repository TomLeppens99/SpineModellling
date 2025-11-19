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
    public partial class frmSkeletalModelingPreferences : Form
    {

        public List<string> GeoemtryDirs = new List<string>();

        public AppData AppData;

        public frmSkeletalModelingPreferences()
        {
            InitializeComponent();
        }

        private void lblexplanation_Click(object sender, EventArgs e)
        {

        }

        private void frmSkeletalModelingPreferences_Load(object sender, EventArgs e)
        {
            GeoemtryDirs.Clear();
            if (System.IO.File.Exists(System.AppContext.BaseDirectory + @"\GeometryPreferences_"+AppData.globalUser._UserID+".txt"))
            {
                ReadLinesFromTXT();
            }
                else
            {
                GeoemtryDirs.Add("SWS");
            }


            richTXTgeometryDirs.Text = string.Empty;
 //richTXTgeometryDirs.Clear();
            foreach (string dir in GeoemtryDirs)
            {
                richTXTgeometryDirs.AppendText(dir + Environment.NewLine);
            }
          
           
        }


        public void Justread()
        {
            GeoemtryDirs.Clear();
            if (System.IO.File.Exists(System.AppContext.BaseDirectory + @"\GeometryPreferences_" + AppData.globalUser._UserID + ".txt"))
            {
                ReadLinesFromTXT();
            }
            else
            {
                GeoemtryDirs.Add("SWS");
            }

        }

        private void ReadLinesFromTXT()
        {
            string line;
            System.IO.StreamReader file =    new System.IO.StreamReader(System.AppContext.BaseDirectory + @"\GeometryPreferences_" + AppData.globalUser._UserID + ".txt");
            while ((line = file.ReadLine()) != null)
            {
                GeoemtryDirs.Add(line);
                
            }

            file.Close();
           

        }
       
        private void frmSkeletalModelingPreferences_FormClosed(object sender, FormClosedEventArgs e)
        {
            GeoemtryDirs.Clear();

    //        string[] lines = richTXTgeometryDirs.Text.Trim().Split(
    //new[] { Environment.NewLine },
    //StringSplitOptions.None

    string[] lines = richTXTgeometryDirs.Text.Trim().Split(    new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            bool uncorrect = false;

            List<string> linesChecked = new List<string>();
         
            foreach (string dir2 in lines)
            {
            
                if (dir2.Trim().ToLower() == "SWS".ToLower())
                {
                    linesChecked.Add(dir2.Trim());
                   
                }
                else
                {
                    if (System.IO.Directory.Exists(dir2.Trim()))
                    {
                        linesChecked.Add(dir2.Trim());
                     
                    }
                    else
                    {
                        uncorrect = true;
                    }
                }
  
            }
            SaveLinesToTXT(linesChecked);

            foreach (string dir in linesChecked)
            {
                if (dir != null)
                {
                    GeoemtryDirs.Add(dir.Trim());
                }
            }

            if (uncorrect)
            {
                MessageBox.Show("One or more of the directories you entered could not be found. Correct directories were saved.", "Directory not found", MessageBoxButtons.OK);
            }
        }


        private void SaveLinesToTXT(List<string> lines)
        {

            System.IO.File.WriteAllLines(System.AppContext.BaseDirectory + @"\GeometryPreferences_" + AppData.globalUser._UserID + ".txt", lines);


        }
    }
}
