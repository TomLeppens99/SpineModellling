using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Kitware.VTK;
using Kitware.mummy;
using OpenSim;


namespace SpineAnalyzer.ModelVisualization
{
    public class OsimMakerProperty
    {
        #region Declarations

        //SYSTEM
        private string _objectName;
        private string _objectType;
        private string _referenceBody;
        private bool _isFixed;
        private bool _isVisible = true;
        private double _colorR = 0.82, _colorG = 0.37, _colorB = 0.93;
        private ContextMenuStrip _contextMenuStrip = new ContextMenuStrip();
        private ToolStripMenuItem displayLabel = new ToolStripMenuItem();
        private ToolStripMenuItem hideLabel = new ToolStripMenuItem();
        private ToolStripMenuItem showLabel = new ToolStripMenuItem();
        private ToolStripMenuItem showOnlyLabel = new ToolStripMenuItem();
        private ToolStripMenuItem deleteLabel = new ToolStripMenuItem();
        private Color _markerColor = SystemColors.Control;
        private Position _absPosition = new Position(0,0,0);
        public OsimBodyProperty parentbodyprop; 

        //OPENSIM
        public Body referenceBodyObject;
        private Vec3 _rOffset = new Vec3();
        private Marker _marker = new Marker();

        //VTK
        private vtkActor _markerActor = new vtkActor();
        private vtkRenderWindow _vtkRenderwindow;
        public vtkAssembly _opaceAssembly = new vtkAssembly();
        public vtkTransform markerTransform = new vtkTransform();

        #endregion

        #region Properties

        [CategoryAttribute("Marker Properties"), DescriptionAttribute("Name of the marker as defined in the protocol."), ReadOnlyAttribute(true)]
        public string objectName
        {
           get { return _objectName; }
           set { _objectName = value;
                _marker.setName(_objectName);
            }
        }

        [CategoryAttribute("Marker Properties"), DescriptionAttribute("Type of the selected node."), ReadOnlyAttribute(true)]
        public string objectType
        {
            get { return _objectType; }
            set { _objectType = value; }
        }

        [CategoryAttribute("Marker Properties") , DefaultValueAttribute(false), DescriptionAttribute("If the marker is fixed to the reference body.")]
        public bool isFixed
        {
            get { return _isFixed; }
            set { _isFixed = value;
                _marker.setFixed(_isFixed);
            }
        }

        [CategoryAttribute("Marker Properties"), DescriptionAttribute("The body to which the marker is linked.")]
        public string referenceBody
        {
            get { return _referenceBody; }
            set { _referenceBody = value; }
        }

        [CategoryAttribute("Marker Properties"), DescriptionAttribute("Choose a color.")]
        public Color markerColor
        {
            get { return _markerColor; }
            set
            {
                _markerColor = value;
                _colorR = (double)_markerColor.R / 255;
                _colorG = (double)_markerColor.G / 255;
                _colorB = (double)_markerColor.B / 255;
                UpdateMarkerColor();
            }
        }

        [CategoryAttribute("Marker Properties"), DescriptionAttribute("Visibility of the marker.")]
        public bool isVisible
        {
            get { return _isVisible; }
            set { _isVisible = value;
                ModifyVisible();
            }
        }

        [CategoryAttribute("Marker Properties"), DescriptionAttribute("Absolute position of the marker (to the ground).")]
        public string absPositionText
        {
            get { return _absPosition.ToString(); }
            
        }

        [CategoryAttribute("Marker Properties"), DescriptionAttribute("Absolute position of the marker (to the ground).")]
        public Position absPosition
        {
            get { return _absPosition; }
            set { _absPosition = value; }
        }

        [CategoryAttribute("Marker Properties"), DescriptionAttribute("Absolute position of the marker (to the ground).")]
        public string absPositionX
        {
            get { return _absPosition.X.ToString(); }
          
        }
        [CategoryAttribute("Marker Properties"), DescriptionAttribute("Absolute position of the marker (to the ground).")]
        public string absPositionY
        {
            get { return _absPosition.Y.ToString(); }

        }
        [CategoryAttribute("Marker Properties"), DescriptionAttribute("Absolute position of the marker (to the ground).")]
        public string absPositionZ
        {
            get { return _absPosition.Z.ToString(); }

        }
        [CategoryAttribute("Marker Properties"), DescriptionAttribute("Relative position of the marker (to the referenced body).")]
        public string rOffsetText
        {
            get { return _rOffset.toString(); }
            //set { _rOffset = value; }
        }

        [Browsable(false)]
        public Vec3 rOffset
        {
            get {
                _marker.getOffset(_rOffset);
                return _rOffset; }
            set { _rOffset = value; }
        }

        [Browsable(false)]
        public Marker marker
        {
            get { return _marker; }
            set { _marker = value; }
        }

        [Browsable(false)]
        public vtkActor markerActor
        {
            get { return _markerActor; }
            set { _markerActor = value; }
        }

        [Browsable(false)]
        public ContextMenuStrip contextMenuStrip
        {
            get { return _contextMenuStrip; }
            set { _contextMenuStrip = value; }
        }

        [Browsable(false)]
        public double colorR
        {
            get { return _colorR; }
            set { _colorR = value; }
        }

        [Browsable(false)]
        public double colorG
        {
            get { return _colorG; }
            set { _colorG = value; }
        }

        [Browsable(false)]
        public double colorB
        {
            get { return _colorB; }
            set { _colorB = value; }
        }

        [Browsable(false)]
        public vtkRenderWindow vtkRenderwindow
        {
            get { return _vtkRenderwindow; }
            set { _vtkRenderwindow = value; }
        }

        #endregion

        #region PropertyMethods
        public void ReadMarkerProperties(Marker marker)
        {
            _contextMenuStrip.ShowCheckMargin = true;
            _marker = marker;
            _objectName = marker.getName();
            _isFixed = marker.getFixed();

            // _absPosition = marker.changeBodyPreserveLocation()     //Dit misschien later nog nodig.
            _referenceBody = _marker.getBody().getName();
            _marker.getOffset(_rOffset);
            _objectType =  _marker.GetType().ToString();
            SetContextMenuStrip();
        }

     

        private void ModifyVisible()
        {
            if(_isVisible == true)
            {
                showLabel.PerformClick();
            }
            if(_isVisible == false)
            {
                hideLabel.PerformClick();
            }
        }

        public void HighlightMarker()
        {
                _markerActor.GetProperty().SetColor(0, 0.8, 0.5);
        }

        public void UnhighlightMarker()
        {
                _markerActor.GetProperty().SetColor(_colorR, _colorG, _colorB);
        }

        private void UpdateMarkerColor()
        {
            _markerActor.GetProperty().SetColor(_colorR, _colorG, _colorB);

        }

        #region Menustrips
        public void SetContextMenuStrip()
        {
            displayLabel.Text = "Display";
            hideLabel.Text = "Hide";
            showLabel.Text = "Show";
            showOnlyLabel.Text = "Show Only";
            deleteLabel.Text = "Delete";

            //Add the menu items to the menu.
            _contextMenuStrip.Items.AddRange(new ToolStripMenuItem[] { displayLabel, deleteLabel });

            hideLabel.Click += new System.EventHandler(this.hideLabel_Click);
            showLabel.Click += new System.EventHandler(this.showLabel_Click);
            showOnlyLabel.Click += new System.EventHandler(this.showOnlyLabel_Click);
            deleteLabel.Click += new System.EventHandler(this.deleteLabel_Click);
            showLabel.Enabled = !_isVisible;
            displayLabel.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { showLabel, showOnlyLabel, hideLabel });
        }

        private void hideLabel_Click(object sender, EventArgs e)
        {
            _isVisible = false;
            hideLabel.Checked = true;
            showLabel.Enabled = !_isVisible;
            showOnlyLabel.Checked = false;
            hideLabel.Checked = true;

            _markerActor.GetProperty().SetOpacity(0);
            _markerActor.PickableOff();

            //vtkRenderwindow.Render();

        }

        private void showLabel_Click(object sender, EventArgs e)
        {
            _isVisible = true;
            hideLabel.Checked = false;
            showLabel.Enabled = !_isVisible;
            showOnlyLabel.Checked = false;

            _markerActor.GetProperty().SetOpacity(1);
            _markerActor.PickableOn();

            //vtkRenderwindow.Render();
        }

        private void showOnlyLabel_Click(object sender, EventArgs e)
        {
            showOnlyLabel.Checked = true;
        }

        private void deleteLabel_Click(object sender, EventArgs e)
        {
            
        
        }
        #endregion

        #endregion
    }
}
