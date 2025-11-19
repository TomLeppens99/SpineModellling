using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenSim;
using Kitware.VTK;
using Kitware.mummy;

namespace SpineAnalyzer.ModelVisualization
{
    public class OsimGroupElement
    {
        #region Declarations
        private bool _isVisible;
        private string _groupName;
        public vtkRenderWindow _vtkRenderwindow;
        private List<OsimBodyProperty> _osimBodyPropertyList = new List<OsimBodyProperty>();
        private List<OsimForceProperty> _osimForcePropertyList = new List<OsimForceProperty>();
        private ContextMenuStrip _contextMenuStrip = new ContextMenuStrip();
        private ToolStripMenuItem displayLabel = new ToolStripMenuItem();
        private ToolStripMenuItem hideLabel = new ToolStripMenuItem();
        private ToolStripMenuItem showLabel = new ToolStripMenuItem();
        private ToolStripMenuItem showOnlyLabel = new ToolStripMenuItem();
        private ToolStripSeparator toolStripSeparator2 = new ToolStripSeparator();
        private ToolStripMenuItem showHideTransparantLabel = new ToolStripMenuItem();
        private ToolStripMenuItem showAxesLabel = new ToolStripMenuItem();
        private ToolStripMenuItem showMassCenterLabel = new ToolStripMenuItem();
        private ToolStripSeparator toolStripSeparator = new ToolStripSeparator();
        private ToolStripMenuItem pointRepresentationLabel = new ToolStripMenuItem();
        private ToolStripMenuItem smoothShadedLabel = new ToolStripMenuItem();
        private ToolStripMenuItem wireframeLabel = new ToolStripMenuItem();
        #endregion

        #region Properties

        [CategoryAttribute("Group Properties"), DescriptionAttribute("Name of the Group."), ReadOnlyAttribute(true)]
        public string groupName
        {
            get { return _groupName; }
            set { _groupName = value; }
        }

        [Browsable(false)]
        public ContextMenuStrip contextMenuStrip
        {
            get { return _contextMenuStrip; }
            set { _contextMenuStrip = value; }
        }

        [Browsable(false)]
        public List<OsimBodyProperty> osimBodyPropertyList
        {
            get { return _osimBodyPropertyList; }
            set { _osimBodyPropertyList = value; }
        }

        [Browsable(false)]
        public List<OsimForceProperty> osimForcePropertyList
        {
            get { return _osimForcePropertyList; }
            set { _osimForcePropertyList = value; }
        }

        [Browsable(false)]
        public vtkRenderWindow vtkRenderwindow
        {
            get { return _vtkRenderwindow; }
            set { _vtkRenderwindow = value; }
        }
        #endregion

        #region Methods

        public void HighlightBody()
        {
            foreach (OsimBodyProperty bodyProp in _osimBodyPropertyList)
            {
                bodyProp.vtkRenderwindow = vtkRenderwindow;
                bodyProp.HighlightBody();
            }
        }

        public void UnhighlightBody()
        {
            foreach (OsimBodyProperty bodyProp in _osimBodyPropertyList)
            {
                bodyProp.vtkRenderwindow = vtkRenderwindow;
                bodyProp.UnhighlightBody();
            }
        }
        
        #region Menustrips
        public void SetContextMenuStrip()
        {
            displayLabel.Text = "Display";
            hideLabel.Text = "Hide";
            showLabel.Text = "Show";
            showOnlyLabel.Text = "Show Only";
            showHideTransparantLabel.Text = "Show 2D";
            showMassCenterLabel.Text = "Show Mass Center";
            showAxesLabel.Text = "Show Axes";
            pointRepresentationLabel.Text = "Point Representation";
            smoothShadedLabel.Text = "Smooth-Shaded";
            wireframeLabel.Text = "Wireframe";

            //Add the menu items to the menu.
            _contextMenuStrip.Items.AddRange(new ToolStripMenuItem[] { displayLabel, showAxesLabel, showMassCenterLabel });

            hideLabel.Click += new System.EventHandler(this.hideLabel_Click);
            showLabel.Click += new System.EventHandler(this.showLabel_Click);
            showLabel.Checked = true;
            showOnlyLabel.Click += new System.EventHandler(this.showOnlyLabel_Click);
            showHideTransparantLabel.Click += new System.EventHandler(this.showHideTransparantLabel_Click);
            showHideTransparantLabel.Checked = true;
            showMassCenterLabel.Click += new System.EventHandler(this.showMassCenterLabel_Click);
            showAxesLabel.Click += new System.EventHandler(this.showAxesLabel_Click);
            pointRepresentationLabel.Click += new System.EventHandler(this.pointRepresentationLabel_Click);
            smoothShadedLabel.Click += new System.EventHandler(this.smoothShadedLabel_Click);
            wireframeLabel.Click += new System.EventHandler(this.wireframeLabel_Click);
            showLabel.Enabled = !_isVisible;
            displayLabel.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { showLabel, showOnlyLabel, hideLabel, toolStripSeparator2, showHideTransparantLabel, toolStripSeparator, pointRepresentationLabel, smoothShadedLabel, wireframeLabel });
        }

        private void hideLabel_Click(object sender, EventArgs e)
        {
            foreach (OsimBodyProperty bodyProp in  _osimBodyPropertyList)
            {
                bodyProp.vtkRenderwindow = vtkRenderwindow;
                bodyProp.hideProgrammatically();
            }
            _isVisible = false;
            hideLabel.Checked = true;
            showLabel.Enabled = !_isVisible;
            showOnlyLabel.Checked = false;
            showLabel.Checked = false;

        }

        private void showLabel_Click(object sender, EventArgs e)
        {
            _isVisible = true;
            hideLabel.Checked = false;
            showLabel.Enabled = !_isVisible;
            showOnlyLabel.Checked = false;

            foreach (OsimBodyProperty bodyProp in _osimBodyPropertyList)
            {
                bodyProp.vtkRenderwindow = vtkRenderwindow;
                bodyProp.ShowProgrammatically();
            }
        }

        private void showOnlyLabel_Click(object sender, EventArgs e)
        {
            _isVisible = true;
            hideLabel.Checked = false;
            showLabel.Enabled = !_isVisible;
            showOnlyLabel.Checked = false;

            showOnlyLabel.Checked = true;
            hideLabel.Checked = false;

            foreach (OsimBodyProperty bodyProp in _osimBodyPropertyList)
            {
                bodyProp.vtkRenderwindow = vtkRenderwindow;
                bodyProp.ShowOnlyProgrammatically();
            }
        }

        private void showHideTransparantLabel_Click(object sender, EventArgs e)
        {
           // double opacity;

            //if (!showHideTransparantLabel.Checked)
            //{
            //    opacity = 0.1;
            //}
            //else
            //{
            //    opacity = 0;
            //}
            foreach (OsimBodyProperty bodyProp in _osimBodyPropertyList)
            {
                bodyProp.ShowHideTranslparentProgrammatically();
            }


            showHideTransparantLabel.Checked = !showHideTransparantLabel.Checked;
        }

        private void showMassCenterLabel_Click(object sender, EventArgs e)
        { }

        private void showAxesLabel_Click(object sender, EventArgs e)
        { }

        private void pointRepresentationLabel_Click(object sender, EventArgs e)
        {
            wireframeLabel.Checked = false;
            smoothShadedLabel.Checked = false;
            pointRepresentationLabel.Checked = true;
            foreach (OsimBodyProperty bodyProp in _osimBodyPropertyList)
            {
                bodyProp.vtkRenderwindow = vtkRenderwindow;
                bodyProp.PointRepresentProgrammatically();
            }

        }

        private void smoothShadedLabel_Click(object sender, EventArgs e)
        {
            wireframeLabel.Checked = false;
            smoothShadedLabel.Checked = true;
            pointRepresentationLabel.Checked = false;

            foreach (OsimBodyProperty bodyProp in _osimBodyPropertyList)
            {
                bodyProp.vtkRenderwindow = vtkRenderwindow;
                bodyProp.SmoothShadedProgramatically();
            }

        }

        private void wireframeLabel_Click(object sender, EventArgs e)
        {
            wireframeLabel.Checked = true;
            smoothShadedLabel.Checked = false;
            pointRepresentationLabel.Checked = false;

            foreach (OsimBodyProperty bodyProp in _osimBodyPropertyList)
            {
                bodyProp.vtkRenderwindow = vtkRenderwindow;
                bodyProp.WireFrameProgramatically();
            }
        }
        #endregion
        #endregion
    }
}
