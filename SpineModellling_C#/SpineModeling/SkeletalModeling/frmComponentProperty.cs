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
    public partial class frmComponentProperty : Form
    {
        public object selectedobject;
        public frmComponentProperty()
        {
            InitializeComponent();
        }

        private void frmComponentProperty_Load(object sender, EventArgs e)
        {
            if(selectedobject!=null)
            {
                propertyGrid1.SelectedObject = selectedobject;
            }
        }
    }
}
