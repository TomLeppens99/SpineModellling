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
    public class OsimBodyProperty
    {
        #region Declarations

        //SYSTEM
        private string _objectName;
        private string _objectType;
        private string _jointName;
        private string _locationInParentStr;
        private string _orientationInParentStr;
        private string _locationInChildStr;
        private string _orientationInChildStr;
        private string _mass_center;
        private double _mass;
        private double _inertia_xx, _inertia_yy, _inertia_zz, _inertia_xy, _inertia_xz, _inertia_yz;
        private double _colorR = 1, _colorG = 1, _colorB = 1;
        private double _absoluteHeight = 0;
        private bool _isGround = false;
        private bool _isVisible;
        private Color _bodyColor = SystemColors.Control;

        private OsimJointProperty _osimJointProperty = new OsimJointProperty();
        public List<OsimGeometryProperty> _OsimGeometryPropertyList = new List<OsimGeometryProperty>();


        //OPENSIM
        private Vec3 _scaleFactors = new Vec3();
        public Vec3 _locationInParent = new Vec3();
        public Vec3 _locationInChild = new Vec3();
        public Vec3 _orientationInParent = new Vec3();
        public Vec3 _orientationInChild = new Vec3();
        public Body _body;
        public Body _parentBody;

        //VTK
        public vtkRenderWindow _vtkRenderwindow;
        public vtkRenderWindow _RenderWindowImage1;
        public vtkRenderWindow _RenderWindowImage2;
        public vtkRenderer renderer;
        public vtkTransform _transformChild = new vtkTransform();
        //private vtkTransform _transformParent = new vtkTransform();
        private vtkTransform _transform = new vtkTransform();
        private vtkAssembly _assembly = new vtkAssembly();
        private vtkAssembly _assemblyOpace1 = new vtkAssembly();
        private vtkAssembly _assemblyOpace2 = new vtkAssembly();

        public Transform absoluteChildTransform;
        public Transform absoluteParentTransform;

        //MENUSTRIP
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

        [CategoryAttribute("Body Properties"), DescriptionAttribute("Name of the body."), ReadOnlyAttribute(true)]
        public string objectName
        {
            get { return _objectName; }
            set { _objectName = value; }
        }

        [CategoryAttribute("Body Properties"), DescriptionAttribute("Type of the selected node"), ReadOnlyAttribute(true)]
        public string objectType
        {
            get { return _objectType; }
            set { _objectType = value; }
        }

        [CategoryAttribute("Body Properties"), DescriptionAttribute("Mass (kg)"), ReadOnlyAttribute(false)]
        public double mass
        {
            get { return _mass; }
            set { _mass = value;
                _body.setMass(_mass);
            }
        }

        [CategoryAttribute("Body Properties"), DescriptionAttribute("Center of Mass"), ReadOnlyAttribute(false)]
        public string mass_center
        {
            get { return _mass_center; }
            set { _mass_center = value; }
        }

        [CategoryAttribute("Body Properties"), DescriptionAttribute("Absolute height of the selected body"), ReadOnlyAttribute(false)]
        public double absoluteHeight
        {
            get { return _absoluteHeight; }
            set { _absoluteHeight = value; }
        }

        [CategoryAttribute("Body Properties"), DescriptionAttribute("Location In Parent"), ReadOnlyAttribute(false)]
        public string locationInParentStr
        {
            get { 
                _locationInParentStr = locationInParent.toString();
                return _locationInParentStr; }
            set { _locationInParentStr = value; }
        }

        [Browsable(false)]
        public Vec3 locationInParent
        {
            get
            {
                if (body.hasJoint())
                {
                    body.getJoint().getLocationInParent(_locationInParent);
                }
                return _locationInParent;
            }
            set { _locationInParent = value;
                body.getJoint().setLocationInParent(value);
            }
        }

        [Browsable(false)]
        public Vec3 locationInChild
        {
            get
            {
                if (body.hasJoint())
                {
                    body.getJoint().getLocation(_locationInChild);
                }
                return _locationInChild;
            }
            set { _locationInChild = value;
                body.getJoint().setLocationInChild(value);
            }
        }

        [Browsable(false)]
        public Vec3 orientationInChild
        {
            get
            {
                if (body.hasJoint())
                {
                    body.getJoint().getOrientation(_orientationInChild);
                }
                return _orientationInChild;
            }
            set { _orientationInChild = value;
                body.getJoint().setOrientation(value);
            }
        }

        [Browsable(false)]
        public Vec3 orientationInParent
        {
            get
            {
                if (body.hasJoint())
                {
                    body.getJoint().getOrientationInParent(_orientationInParent);
                }
                return _orientationInParent;
            }
            set { _orientationInParent = value;
                body.getJoint().setOrientationInParent(value);
            }
        }

        [CategoryAttribute("Body Properties"), DescriptionAttribute("Orientation In Parent"), ReadOnlyAttribute(false)]
        public string orientationInParentStr
        {
            get {
                _orientationInParentStr = orientationInParent.toString();
                return _orientationInParentStr; }
            set { _orientationInParentStr = value; }
        }

        [CategoryAttribute("Body Properties"), DescriptionAttribute("Location In Child"), ReadOnlyAttribute(false)]
        public string locationInChildStr
        {
            get
            { _locationInChildStr = locationInChild.toString();
                return _locationInChildStr; }
            set { _locationInChildStr = value; }
        }

        [CategoryAttribute("Body Properties"), DescriptionAttribute("Orientation In Child"), ReadOnlyAttribute(false)]
        public string orientationInChildStr
        {
            get {
                _orientationInChildStr = orientationInChild.toString();
                return _orientationInChildStr; }
            set { _orientationInChildStr = value; }
        }

        [CategoryAttribute("Body Properties"), DescriptionAttribute("Inertia_xx"), ReadOnlyAttribute(false)]
        public double inertia_xx
        {
            get { return _inertia_xx; }
            set { _inertia_xx = value; }
        }

        [CategoryAttribute("Body Properties"), DescriptionAttribute("Inertia_yy"), ReadOnlyAttribute(false)]
        public double inertia_yy
        {
            get { return _inertia_yy; }
            set { _inertia_yy = value; }
        }

        [CategoryAttribute("Body Properties"), DescriptionAttribute("Inertia_zz"), ReadOnlyAttribute(false)]
        public double inertia_zz
        {
            get { return _inertia_zz; }
            set { _inertia_zz = value; }
        }

        [CategoryAttribute("Body Properties"), DescriptionAttribute("Inertia_xy"), ReadOnlyAttribute(false)]
        public double inertia_xy
        {
            get { return _inertia_xy; }
            set { _inertia_xy = value; }
        }

        [CategoryAttribute("Body Properties"), DescriptionAttribute("Inertia_xz"), ReadOnlyAttribute(false)]
        public double inertia_xz
        {
            get { return _inertia_xz; }
            set { _inertia_xz = value; }
        }

        [CategoryAttribute("Body Properties"), DescriptionAttribute("Inertia_yz"), ReadOnlyAttribute(false)]
        public double inertia_yz
        {
            get { return _inertia_yz; }
            set { _inertia_yz = value; }
        }

        [CategoryAttribute("Body Properties"), DescriptionAttribute("Joint"), ReadOnlyAttribute(false)]
        public string jointName
        {
            get { return _jointName; }
            set { _jointName = value; }
        }

        [CategoryAttribute("Body Properties"), DescriptionAttribute("Choose a color.")]
        public Color bodyColor
        {
            get { return _bodyColor; }
            set
            {
                _bodyColor = value;
                _colorR = (double)_bodyColor.R / 255;
                _colorG = (double)_bodyColor.G / 255;
                _colorB = (double)_bodyColor.B / 255;
                UpdateBodyColor();
            }
        }

        [Browsable(false)]
        public ContextMenuStrip contextMenuStrip
        {
            get { return _contextMenuStrip; }
            set { _contextMenuStrip = value; }
        }

        //[Browsable(false)]
        [CategoryAttribute("Debug Body Properties"), DescriptionAttribute("")]
        public vtkAssembly assembly
        {
            get { return _assembly; }
            set { _assembly = value; }
        }

        //[Browsable(false)]
        [CategoryAttribute("Debug Body Properties"), DescriptionAttribute("")]
        public vtkAssembly assemblyOpace1
        {
            get { return _assemblyOpace1; }
            set { _assemblyOpace1 = value; }
        }

        //[Browsable(false)]
        [CategoryAttribute("Debug Body Properties"), DescriptionAttribute("")]
        public vtkAssembly assemblyOpace2
        {
            get { return _assemblyOpace2; }
            set { _assemblyOpace2 = value; }
        }

        //[Browsable(false)]
        //public vtkTransform transformChild
        //{
        //    get { return _transformChild; }
        //    set { _transformChild = value; }
        //}

        //[Browsable(false)]
        //public vtkTransform transformParent
        //{
        //    get { return _transformParent; }
        //    set { _transformParent = value; }
        //}

        [Browsable(false)]
        public vtkTransform transform
        {
            get {  //_transform = (vtkTransform)assembly.GetUserTransform();
                return (vtkTransform)assembly.GetUserTransform();
            }
            set {
                //_transform = value;
                assembly.SetUserTransform(value);
                    if(assemblyOpace1!=null)
                {
                    assemblyOpace1.SetUserTransform(value);
                    assemblyOpace2.SetUserTransform(value);
                }
      
            }
        }

        [Browsable(false)]
        public Body body
        {
            get { return _body; }
            set { _body = value; }
        }

        [Browsable(false)]
        public bool isGround
        {
            get { return _isGround; }
            set { _isGround = value; }
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
            get {if(renderer!=null)
                {
return renderer.GetRenderWindow();
                }
                else { return null; }
                 }
            set { _vtkRenderwindow = value; }
        }

        [Browsable(false)]
        public vtkRenderWindow RenderWindowImage1
        {
            get { return _RenderWindowImage1; }
            set { _RenderWindowImage1 = value; }
        }

        [Browsable(false)]
        public vtkRenderWindow RenderWindowImage2
        {
            get { return _RenderWindowImage2; }
            set { _RenderWindowImage2 = value; }
        }

        [Browsable(false)]
        public Vec3 scaleFactors
        {
            get { return _scaleFactors; }
            set { _scaleFactors = value;
                UpdateScales();
            }
        }

        [Browsable(false)]
        public OsimJointProperty osimJointProperty
        {
            get { return _osimJointProperty; }
            set { _osimJointProperty = value;
            }
        }
        #endregion

        #region PropertyMethods
        public void ReadBodyProperties(Body body)
        {
            _body = body;
            _mass = body.getMass();
            _objectType = body.GetType().ToString();
            _objectName = body.getName();
           
            _assembly.PickableOn();

            SetContextMenuStrip();
            _contextMenuStrip.ShowCheckMargin = true;
            smoothShadedLabel.Checked = true;

            if (_isGround == false)
            {
                //Vec3 rVec = new Vec3();
                //body.getMassCenter(rVec);
                //_mass_center = rVec.toString();   //Dit nog aanpassen!
                Mat33 rInertia = new Mat33();
                body.getInertia(rInertia);
                _inertia_xx = rInertia.get(0, 0);
                _inertia_yy = rInertia.get(1,1);
                _inertia_zz = rInertia.get(2,2);
                _inertia_xy = rInertia.get(0,1);
                _inertia_xz = rInertia.get(0,2);
                _inertia_yz = rInertia.get(1,2);
           }
            if (body.hasJoint())
            {
                //This does not work for object without joint (ground)
                _parentBody = _body.getJoint().getParentBody();
                _osimJointProperty.joint = body.getJoint();
                _osimJointProperty.osimBodyProp = this;
                _osimJointProperty.renderer = renderer;
              
                _osimJointProperty.ReadJoint();

                _locationInParent = _osimJointProperty.locationInParent;
                _orientationInParent = _osimJointProperty.orientationInParent;
                _locationInChild = _osimJointProperty.locationInChild;
                _orientationInChild = _osimJointProperty.orientationInChild;
                _jointName = body.getJoint().getName();

                //TODO: 
                // body.getDisplayer().getScaleFactors(_scaleFactors);
                body.getScaleFactors(_scaleFactors);
                
            }
        }

        public void HighlightBody()
        {
            foreach(OsimGeometryProperty geomProp in _OsimGeometryPropertyList)
            {
                geomProp._vtkActor.GetProperty().SetColor(1, 1, 0);
            }
        }

        public void UnhighlightBody()
        {
            foreach (OsimGeometryProperty geomProp in _OsimGeometryPropertyList)
            {
                geomProp._vtkActor.GetProperty().SetColor(geomProp.geomColorR, geomProp.geomColorG, geomProp.geomColorB);
            }
        }

        private void UpdateBodyColor()
        {
            foreach (OsimGeometryProperty geomProp in _OsimGeometryPropertyList)
            {
                geomProp._vtkActor.GetProperty().SetColor(geomProp.geomColorR, geomProp.geomColorG, geomProp.geomColorB);
            }
        }

        public void UpdateScales()
        {
            _assembly.SetScale(scaleFactors.get(0), scaleFactors.get(1), scaleFactors.get(2));
            _assemblyOpace1.SetScale(scaleFactors.get(0), scaleFactors.get(1), scaleFactors.get(2)); 
            _assemblyOpace2.SetScale(scaleFactors.get(0), scaleFactors.get(1), scaleFactors.get(2));
        }


        #region ContextMenuStrips
        private void SetContextMenuStrip()
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
            displayLabel.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { showLabel, showOnlyLabel, hideLabel,toolStripSeparator2, showHideTransparantLabel, toolStripSeparator, pointRepresentationLabel, smoothShadedLabel, wireframeLabel });
        }

        public void hideProgrammatically()
        {
            //This is without the render at the end
            _isVisible = false;
            hideLabel.Checked = true;
            showLabel.Enabled = !_isVisible;
            showOnlyLabel.Checked = false;
            showLabel.Checked = false;

            foreach (OsimGeometryProperty geomProp in _OsimGeometryPropertyList)
            {
                geomProp._vtkActor.GetProperty().SetOpacity(0);
                geomProp._vtkActor.PickableOff();
            }
        }

        private void hideLabel_Click(object sender, EventArgs e)
        {
            _isVisible = false;
            hideLabel.Checked = true;
            showLabel.Enabled = !_isVisible;
            showOnlyLabel.Checked = false;
            showLabel.Checked = false;

            foreach (OsimGeometryProperty geomProp in _OsimGeometryPropertyList)
            {
                geomProp._vtkActor.GetProperty().SetOpacity(0);
                geomProp._vtkActor.PickableOff();
            }

            if(vtkRenderwindow!=null)
            {
            vtkRenderwindow.Render();
            }


        }

        public void ShowProgrammatically()
        {
            showLabel.PerformClick();
        }

        private void showLabel_Click(object sender, EventArgs e)
        {
            _isVisible = true;
            hideLabel.Checked = false;
            showLabel.Enabled = !_isVisible;
            showOnlyLabel.Checked = false;
            foreach (OsimGeometryProperty geomProp in _OsimGeometryPropertyList)
            {
                geomProp._vtkActor.GetProperty().SetOpacity(1);
                geomProp._vtkActor.PickableOn();
            }
           //vtkRenderwindow.Render();
        }

        public void ShowOnlyProgrammatically()
        {
            showOnlyLabel.PerformClick();
            foreach (OsimGeometryProperty geomProp in _OsimGeometryPropertyList)
            {
                geomProp._vtkActor.GetProperty().SetOpacity(0);
                geomProp._vtkActor.PickableOff();
            }
            //vtkRenderwindow.Render();
        }

        private void showOnlyLabel_Click(object sender, EventArgs e)
        {
            showOnlyLabel.Checked = true;
            hideLabel.Checked = false;

            foreach (OsimGeometryProperty geomProp in _OsimGeometryPropertyList)
            {
                geomProp._vtkActor.GetProperty().SetOpacity(1);
                geomProp._vtkActor.PickableOn();
            }

            //vtkRenderwindow.Render();
        }

        private void showHideTransparantLabel_Click(object sender, EventArgs e)
        {

            ShowHideTranslparentProgrammatically();
        }


        public void ShowHideTranslparentProgrammatically()
        {
           double opacity;

            if (!showHideTransparantLabel.Checked)
            {
                opacity = 0.1;
            }
            else
            {
                opacity = 0;
            }

            foreach (OsimGeometryProperty geomProp in _OsimGeometryPropertyList)
            {
                geomProp._vtkActor1.GetProperty().SetOpacity(opacity);
            }

            showHideTransparantLabel.Checked = !showHideTransparantLabel.Checked; 
        }


        public void HideTranslucentProgrammatically()
        {
            foreach (OsimGeometryProperty geomProp in _OsimGeometryPropertyList)
            {
                geomProp._vtkActor1.GetProperty().SetOpacity(0);
            }
            showHideTransparantLabel.Checked = true;

        }
        public void ShowTranslucentProgrammatically()
        {
            foreach (OsimGeometryProperty geomProp in _OsimGeometryPropertyList)
            {
                geomProp._vtkActor1.GetProperty().SetOpacity(0.1);
            }

            showHideTransparantLabel.Checked = false;
        }
        private void showMassCenterLabel_Click(object sender, EventArgs e)
        { }

        private void showAxesLabel_Click(object sender, EventArgs e)
        { }

        public void PointRepresentProgrammatically()
        {
            pointRepresentationLabel.PerformClick();
        }

        private void pointRepresentationLabel_Click(object sender, EventArgs e)
        {
            foreach (OsimGeometryProperty geomProp in _OsimGeometryPropertyList)
            {
                geomProp._vtkActor.GetProperty().SetRepresentationToPoints();
                wireframeLabel.Checked = false;
                smoothShadedLabel.Checked = false;
                pointRepresentationLabel.Checked = true;
            }
            //vtkRenderwindow.Render();
        }

        public void SmoothShadedProgramatically()
        {
            smoothShadedLabel.PerformClick();

        }

        private void smoothShadedLabel_Click(object sender, EventArgs e)
        {
            foreach (OsimGeometryProperty geomProp in _OsimGeometryPropertyList)
            {
                geomProp._vtkActor.GetProperty().SetRepresentationToSurface();
                wireframeLabel.Checked = false;
                smoothShadedLabel.Checked = true;
                pointRepresentationLabel.Checked = false;
            }
           // vtkRenderwindow.Render();
        }

        public void WireFrameProgramatically()
        {
            wireframeLabel.PerformClick();

        }

        private void wireframeLabel_Click(object sender, EventArgs e)
        {
            foreach (OsimGeometryProperty geomProp in _OsimGeometryPropertyList)
            {
                geomProp._vtkActor.GetProperty().SetRepresentationToWireframe();
                wireframeLabel.Checked = true;
                smoothShadedLabel.Checked = false;
                pointRepresentationLabel.Checked = false;
            }
            vtkRenderwindow.Render();
        }
        
        
     
        #endregion
       
        #endregion
    }
}