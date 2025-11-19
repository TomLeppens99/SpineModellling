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
    public class OsimForceProperty
    {
        #region Declarations

        //SYSTEM
        private string _objectName;
        
        public string groupname = "Undefined";

        private double _colorR = 1, _colorG = 1, _colorB = 1;
        public int forceSetIndex;
        private int _totalNumberCP = 0;
        private bool _isVisible;
        private bool _isMuscle;
        private Color _muscleColor = Color.Red; //SystemColors.Control;

        private List<vtkPolyDataMapper> _vtkMapperList = new List<vtkPolyDataMapper>();
        private List<OsimControlPointProperty> _controlPointsList = new List<OsimControlPointProperty>();
        
        private List<OsimMuscleActuatorLineProperty> _muscleLineList = new List<OsimMuscleActuatorLineProperty>();
        public SimModelVisualization SimModelVisualization;

        //OPENSIM
        private Force _force;
        public Muscle _muscle;
        public Model osimModel;
        public GeometryPath geometryPath;

        //VTK
        public vtkRenderWindow _vtkRenderwindow;
        public vtkRenderer ren1;
        public vtkRenderWindow _RenderWindowImage1;
        public vtkRenderWindow _RenderWindowImage2;
        public vtkRenderer vtkRenderer;
        private vtkAssembly _assembly = new vtkAssembly();
        private vtkTransform _transform = new vtkTransform();


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

        private string _objectType = " ";
        private bool _isDisabled;
        private double _min_control;
        private double _max_control;
        private double _max_isometric_force;
        private double _optimal_fibre_length;
        private double _tendon_slack_angle;
        private double _pennation_angle_at_optimal;
        private double _max_contraction_velocity;
        private double _tendon_slack_length;
        private bool _ignore_tendon_compliance;
        private bool _ignore_activation_dynamics;

        #endregion

        #region Properties

        [CategoryAttribute("Force Properties"), DescriptionAttribute("Name of the body."), ReadOnlyAttribute(false)]
        public string objectName
        {
            get { return _objectName; }
            set { _objectName = value;
                UpdateForceProperties(); 
            }
        }

        [CategoryAttribute("Force Properties"), DescriptionAttribute("Type of the selected node"), ReadOnlyAttribute(false)]
        public string objectType
        {
            get { return _objectType; }
            set { _objectType = value; }
        }

        [CategoryAttribute("Force Properties"), DescriptionAttribute("Number of containing control points"), ReadOnlyAttribute(true)]
        public int totalNumberCP
        {
            get { return _totalNumberCP; }
            set { _totalNumberCP = value; }
        }
        
        [CategoryAttribute("Force Properties"), DescriptionAttribute("Choose a color.")]
        public Color muscleColor
        {
            get { return _muscleColor; }
            set
            {
                _muscleColor = value;
                _colorR = (double)_muscleColor.R / 255;
                _colorG = (double)_muscleColor.G / 255;
                _colorB = (double)_muscleColor.B / 255;
                UpdateBodyColor();
            }
        }

        [CategoryAttribute("Force Properties"), DescriptionAttribute("Flag indicating wheter the force is disabled or not. Disabled means that the force is not active in subsequent dynamic realizations."), ReadOnlyAttribute(false)]
        public bool is_Disabled
        {
            get { return _isDisabled; }
            set { _isDisabled = value; UpdateForceProperties(); }
        }

        [CategoryAttribute("Force Properties"), DescriptionAttribute("Minimum allowed value for control signal. Used primarily when solving for control values."), ReadOnlyAttribute(false)]
        public double min_control
        {
            get { return _min_control; }
            set { _min_control = value; UpdateForceProperties(); }
        }

        [CategoryAttribute("Force Properties"), DescriptionAttribute("Maximum allowed value for control signal. Used primarily when solving for control values."), ReadOnlyAttribute(false)]
        public double max_control
        {
            get { return _max_control; }
            set { _max_control = value; UpdateForceProperties(); }
        }

        [CategoryAttribute("Force Properties"), DescriptionAttribute("Maximum isometric force that the fibers can generate."), ReadOnlyAttribute(false)]
        public double max_isometric_force
        {
            get { return _max_isometric_force; }
            set { _max_isometric_force = value; UpdateForceProperties(); }
        }

        [CategoryAttribute("Force Properties"), DescriptionAttribute("Optimal length of the muscle fibers."), ReadOnlyAttribute(false)]
        public double optimal_fibre_length
        {
            get { return _optimal_fibre_length; }
            set { _optimal_fibre_length = value; UpdateForceProperties(); }
        }

        [CategoryAttribute("Force Properties"), DescriptionAttribute("Resting length of the tendon."), ReadOnlyAttribute(false)]
        public double tendon_slack_angle
        {
            get { return _tendon_slack_angle; }
            set { _tendon_slack_angle = value; UpdateForceProperties(); }
        }

        [CategoryAttribute("Force Properties"), DescriptionAttribute("Angle between the tendon and the fibers at optimal fiber length, expressed in radians."), ReadOnlyAttribute(false)]
        public double pennation_angle_at_optimal
        {
            get { return _pennation_angle_at_optimal; }
            set { _pennation_angle_at_optimal = value; UpdateForceProperties(); }
        }

        [CategoryAttribute("Force Properties"), DescriptionAttribute("Maximum contraction velocity of the fibers, in optimal fiberlengths/second."), ReadOnlyAttribute(false)]
        public double max_contraction_velocity
        {
            get { return _max_contraction_velocity; }
            set { _max_contraction_velocity = value; UpdateForceProperties(); }
        }

        [CategoryAttribute("Force Properties"), DescriptionAttribute("Resting length of the tendon."), ReadOnlyAttribute(false)]
        public double tendon_slack_length
        {
            get { return _tendon_slack_length; }
            set { _tendon_slack_length = value; UpdateForceProperties(); }
        }

        [CategoryAttribute("Force Properties"), DescriptionAttribute("Compute muscle dynamics ignoring tendon compliance. Tendon is assumed to be rigid."), ReadOnlyAttribute(false)]
        public bool ignore_tendon_compliance
        {
            get { return _ignore_tendon_compliance; }
            set { _ignore_tendon_compliance = value; UpdateForceProperties(); }
        }

        [CategoryAttribute("Force Properties"), DescriptionAttribute("Compute muscle dynamics ignoring activation dynamics. Activation is equivalent to excitation."), ReadOnlyAttribute(false)]
        public bool ignore_activation_dynamics
        {
            get { return _ignore_activation_dynamics; }
            set { _ignore_activation_dynamics = value; UpdateForceProperties(); }
        }

        [Browsable(false)]
        public ContextMenuStrip contextMenuStrip
        {
            get { return _contextMenuStrip; }
            set { _contextMenuStrip = value; }
        }

        [Browsable(false)]
        public vtkAssembly assembly
        {
            get { return _assembly; }
            set { _assembly = value; }
        }

        [Browsable(false)]
        public vtkTransform transform
        {
            get { return _transform; }
            set { _transform = value; }
        }

        [Browsable(false)]
        public Force force
        {
            get { return _force; }
            set { _force = value; }
        }

        [CategoryAttribute("Force Properties"), DescriptionAttribute("Is this force a representation of a muscle fibre?"), ReadOnlyAttribute(true)]
        public bool isMuscle
        {
            get { return _isMuscle; }
            set { _isMuscle = value; }
        }

        [Browsable(false)]
        public List<vtkPolyDataMapper> vtkMapperList
        {
            get { return _vtkMapperList; }
            set { _vtkMapperList = value; }
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
            get { return vtkRenderer.GetRenderWindow(); }
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
        public List<OsimControlPointProperty> controlPointsList
        {
            get { return _controlPointsList; }
            set { _controlPointsList = value; }
        }

        [Browsable(false)]
        public List<OsimMuscleActuatorLineProperty> muscleLineList
        {
            get { return _muscleLineList; }
            set { _muscleLineList = value; }
        }
        #endregion

        #region PropertyMethods
        public void ReadForceProperties(Force force)
        {
            _force = this.force;
            _objectName = force.getName();
            _controlPointsList.Clear();
            _muscleLineList.Clear();
            _vtkMapperList.Clear();

            if (osimModel.getMuscles().contains(_objectName))
            {
                _muscle = osimModel.getMuscles().get(_objectName);
                _objectType = _muscle. GetType().ToString();
                _isDisabled = _muscle.get_isDisabled();
                _min_control = _muscle.get_min_control();
                _max_control = _muscle.get_max_control();
                _max_isometric_force = _muscle.getMaxIsometricForce();
                _optimal_fibre_length = _muscle.get_optimal_fiber_length();
                _tendon_slack_length = _muscle.get_tendon_slack_length();
                _pennation_angle_at_optimal = _muscle.get_pennation_angle_at_optimal();
                _max_contraction_velocity = _muscle.getMaxContractionVelocity();
                _ignore_tendon_compliance = _muscle.get_ignore_tendon_compliance();
                _ignore_activation_dynamics = _muscle.get_ignore_activation_dynamics();
                _isMuscle = true;
            }
            else { _isMuscle = false;
                return; }


            if (_muscle == null)
            { return;  }

           

            //MakeMuscleControlPointActors();
            ////MakeMuscleLineActors();

            //_assembly.PickableOn();

            SetContextMenuStrip();
            _contextMenuStrip.ShowCheckMargin = true;
            smoothShadedLabel.Checked = true;


            //if (_isGround == false)
            //{
            //    //Vec3 rVec = new Vec3();
            //    //body.getMassCenter(rVec);
            //    //_mass_center = rVec.toString();   //Dit nog aanpassen!
            //    Mat33 rInertia = new Mat33();
            //    body.getInertia(rInertia);
            //    _inertia_xx = rInertia.get(0, 0);
            //    _inertia_yy = rInertia.get(1, 1);
            //    _inertia_zz = rInertia.get(2, 2);
            //    _inertia_xy = rInertia.get(0, 1);
            //    _inertia_xz = rInertia.get(0, 2);
            //    _inertia_yz = rInertia.get(1, 2);
            //}
            //if (body.hasJoint())
            //{
            //    //This does not work for object without joint (ground)
            //    _jointName = body.getJoint().getName();
            //    Vec3 locationInParent = new Vec3();
            //    Vec3 location = new Vec3();
            //    Vec3 orientation = new Vec3();
            //    Vec3 orientationInParent = new Vec3();


            //    //TODO: 
            //    // body.getDisplayer().getScaleFactors(_scaleFactors);
            //    body.getScaleFactors(_scaleFactors);

            //    Transform transform = body.getDisplayer().getTransform();
            //    Vec3 transl = transform.T();
            //    Rotation rot = transform.R();
            //    Vec3 rotvect = rot.convertRotationToBodyFixedXYZ();
            //    vtkTransform vtktransf = vtkTransform.New();
            //    vtktransf.Translate(transl.get(0), transl.get(1), transl.get(2));
            //    vtktransf.RotateX(rotvect.get(0));
            //    vtktransf.RotateY(rotvect.get(1));
            //    vtktransf.RotateZ(rotvect.get(2));
            //}
        }

        public void UpdateForceProperties()
        {
            _muscle.setName(_objectName);
            // _objectType = _muscle.GetType().ToString();
            _muscle.set_isDisabled(_isDisabled);
            _muscle.set_min_control(_min_control);
            _muscle.set_max_control(_max_control);
            _muscle.setMaxIsometricForce(_max_isometric_force);
            _muscle.set_optimal_fiber_length(_optimal_fibre_length);
            _muscle.set_tendon_slack_length(_tendon_slack_length);
            _muscle.set_pennation_angle_at_optimal(_pennation_angle_at_optimal);
            _muscle.setMaxContractionVelocity(_max_contraction_velocity);
            _muscle.set_ignore_tendon_compliance(_ignore_tendon_compliance);
            _muscle.set_ignore_activation_dynamics(_ignore_activation_dynamics);
            //_isMuscle = true;

            _muscle.updateDisplayer(SimModelVisualization.si);
            osimModel.updateDisplayer(SimModelVisualization.si);
         
        }

        public void CreateTheObjects()
        {

            MakeMuscleControlPointActors();
            //MakeMuscleLineActors();

            _assembly.PickableOn();

        }

        private void MakeMuscleControlPointActors()
        {
            geometryPath = _muscle.get_GeometryPath();
            PathPointSet pathPointSet = geometryPath.getPathPointSet();
            
            int nbPathpoints = pathPointSet.getSize();
            _totalNumberCP = nbPathpoints;
           
            for (int j = 0; j < nbPathpoints; j++)
            {
                MakeControlPoint(pathPointSet.get(j), j);
            }
        }

        public void MakeMuscleLineActors()
        {

            GeometryPath geometryPath = _muscle.get_GeometryPath();
            PathPointSet pathPointSet = geometryPath.getPathPointSet();

            int nbPathpoints = pathPointSet.getSize();

            for (int j = 1; j < nbPathpoints; j++)
            {
                OsimMuscleActuatorLineProperty muscleLineprop = new OsimMuscleActuatorLineProperty();
                muscleLineprop.cP1 = getSpecifiedCPProperty(pathPointSet.get(j - 1));
                muscleLineprop.cP2 = getSpecifiedCPProperty(pathPointSet.get(j));

                muscleLineprop.MakeMuscleLineActor();
                _muscleLineList.Add(muscleLineprop);
                ren1.AddActor(muscleLineprop.muscleActor);
            }
        }

        public void RemoveActorsFromRen()
        {
            foreach (OsimControlPointProperty cpProp in _controlPointsList)
            {
                ren1.RemoveActor(cpProp.controlPointActor);
            }

            foreach (OsimMuscleActuatorLineProperty muscleLineprop in _muscleLineList)
            {
                ren1.RemoveActor(muscleLineprop.muscleActor);
            }
        }

        public OsimControlPointProperty getSpecifiedCPProperty(PathPoint pathPoint1)
        {
            string name = pathPoint1.getName();
            int index = _controlPointsList.FindIndex(x => x.objectName == name);
            return _controlPointsList[index];
        }

        private void MakeControlPoint(PathPoint pathPoint, int index)
        {
            OsimControlPointProperty osimControlPointProperty = new OsimControlPointProperty();
            osimControlPointProperty.pathPoint = pathPoint;

            osimControlPointProperty.osimForceProperty = this;
            osimControlPointProperty.CpNumber = index + 1; 
            osimControlPointProperty.MakeControlPointActor();
            _controlPointsList.Add(osimControlPointProperty);
           // _vtkControlPointsList.Add(osimControlPointProperty.controlPointActor);

        }

        public void HighlightBody()
        {
            foreach (OsimControlPointProperty cPprop  in _controlPointsList)
            {
                cPprop.controlPointActor.GetProperty().SetColor(1, 1, 0);
            }
        }

        public OsimControlPointProperty getSpecifiedControlPointPropertyFromName(string name)
        {
            int index = _controlPointsList.FindIndex(x => x.objectName == name);
            return _controlPointsList[index];
        }

        public void UnhighlightBody()
        {

            foreach (OsimControlPointProperty cPprop in _controlPointsList)
            {
                cPprop.controlPointActor.GetProperty().SetColor(1, 0, 0);
            }

        }

        public void ChangePickable(Boolean value)
        {
            if (value == false)
            {
                foreach (OsimControlPointProperty cpProp in _controlPointsList)
                {
                    cpProp.controlPointActor.PickableOff();
                }
            }
            else
            {
                foreach (OsimControlPointProperty cpProp in _controlPointsList)
                {
                    cpProp.controlPointActor.PickableOn();
                }
            }
        }

        private void UpdateBodyColor()
        {
            foreach (OsimControlPointProperty cPprop in _controlPointsList)
            {
                cPprop.controlPointActor.GetProperty().SetColor(_colorR, _colorG, _colorB);
            }
            foreach (OsimMuscleActuatorLineProperty mlprop in _muscleLineList)
            {
                mlprop.muscleActor.GetProperty().SetColor(_colorR, _colorG, _colorB);
            }

        }

        #region ContextMenuStrips

        private void SetContextMenuStrip()
        {
            displayLabel.Text = "Display";
            hideLabel.Text = "Hide";
            showLabel.Text = "Show";
            showOnlyLabel.Text = "Show Only";
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
            showMassCenterLabel.Click += new System.EventHandler(this.showMassCenterLabel_Click);
            showAxesLabel.Click += new System.EventHandler(this.showAxesLabel_Click);
            pointRepresentationLabel.Click += new System.EventHandler(this.pointRepresentationLabel_Click);
            smoothShadedLabel.Click += new System.EventHandler(this.smoothShadedLabel_Click);
            wireframeLabel.Click += new System.EventHandler(this.wireframeLabel_Click);
            showLabel.Enabled = !_isVisible;
            displayLabel.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { showLabel, showOnlyLabel, hideLabel, toolStripSeparator2, toolStripSeparator, pointRepresentationLabel, smoothShadedLabel, wireframeLabel });
        }

        public void hideProgrammatically()
        {
            hideLabel.PerformClick();
        }

        private void hideLabel_Click(object sender, EventArgs e)
        {
            _isVisible = false;
            hideLabel.Checked = true;
            showLabel.Enabled = !_isVisible;
            showOnlyLabel.Checked = false;
            showLabel.Checked = false;
            foreach (OsimControlPointProperty cPprop in _controlPointsList)
            {
                cPprop.controlPointActor.GetProperty().SetOpacity(0);
                cPprop.controlPointActor.PickableOff();
            }
            vtkRenderwindow.Render();
        }

        public void ShowProgrammatically()
        {
            _isVisible = true;
            hideLabel.Checked = false;
            showLabel.Enabled = !_isVisible;
            showOnlyLabel.Checked = false;
            foreach (OsimControlPointProperty cPprop in _controlPointsList)
            {
                cPprop.controlPointActor.VisibilityOn();
                cPprop.controlPointActor.GetProperty().SetOpacity(1);
                cPprop.controlPointActor.PickableOn();
            }
            foreach (OsimMuscleActuatorLineProperty muscleLineProp in _muscleLineList)
            {
                muscleLineProp.muscleActor.VisibilityOn();
                muscleLineProp.muscleActor.GetProperty().SetOpacity(1);
                muscleLineProp.muscleActor.PickableOn();
            }
        }

        private void showLabel_Click(object sender, EventArgs e)
        {
            _isVisible = true;
            hideLabel.Checked = false;
            showLabel.Enabled = !_isVisible;
            showOnlyLabel.Checked = false;
            foreach (OsimControlPointProperty cPprop in _controlPointsList)
            {
                cPprop.controlPointActor.VisibilityOn();
                cPprop.controlPointActor.GetProperty().SetOpacity(1);
                cPprop.controlPointActor.PickableOn();
            }
            foreach (OsimMuscleActuatorLineProperty muscleLineProp in _muscleLineList)
            {
                muscleLineProp.muscleActor.VisibilityOn();
                muscleLineProp.muscleActor.GetProperty().SetOpacity(1);
                muscleLineProp.muscleActor.PickableOn();
            }
            vtkRenderwindow.Render();
        }

        public void ShowOnlyProgrammatically()
        {
            showOnlyLabel.PerformClick();
            foreach (OsimControlPointProperty cPprop in _controlPointsList)
            {
                cPprop.controlPointActor.GetProperty().SetOpacity(0);
                cPprop.controlPointActor.PickableOff();
            }
            vtkRenderwindow.Render();
        }

        private void showOnlyLabel_Click(object sender, EventArgs e)
        {
            showOnlyLabel.Checked = true;
            hideLabel.Checked = false;

            foreach (OsimControlPointProperty cPprop in _controlPointsList)
            {
                cPprop.controlPointActor.GetProperty().SetOpacity(1);
                cPprop.controlPointActor.PickableOn();
            }

            vtkRenderwindow.Render();
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
            foreach (OsimControlPointProperty cPprop in _controlPointsList)
            {
                cPprop.controlPointActor.GetProperty().SetRepresentationToPoints();
                wireframeLabel.Checked = false;
                smoothShadedLabel.Checked = false;
                pointRepresentationLabel.Checked = true;
            }
            vtkRenderwindow.Render();
        }

        public void SmoothShadedProgramatically()
        {
            smoothShadedLabel.PerformClick();

        }

        private void smoothShadedLabel_Click(object sender, EventArgs e)
        {
            foreach (OsimControlPointProperty cPprop in _controlPointsList)
            {
                cPprop.controlPointActor.GetProperty().SetRepresentationToSurface();
                wireframeLabel.Checked = false;
                smoothShadedLabel.Checked = true;
                pointRepresentationLabel.Checked = false;
            }
            vtkRenderwindow.Render();
        }

        public void WireFrameProgramatically()
        {
            wireframeLabel.PerformClick();

        }

        private void wireframeLabel_Click(object sender, EventArgs e)
        {
            foreach (OsimControlPointProperty cPprop in _controlPointsList)
            {
                cPprop.controlPointActor.GetProperty().SetRepresentationToWireframe();
                wireframeLabel.Checked = true;
                smoothShadedLabel.Checked = false;
                pointRepresentationLabel.Checked = false;
            }
            vtkRenderwindow.Render();
        }

        public void UpdateMuscleLinePropertyTransform()
        {
            foreach (OsimMuscleActuatorLineProperty muscleLineprop in _muscleLineList)
            {
                muscleLineprop._colorR = _colorR;
                muscleLineprop._colorG = _colorG;
                muscleLineprop._colorB = _colorB;
                muscleLineprop.MakeMuscleLineActor();
            }

        }

        public void UpdateMuscleLineActorTransform()
        {
            foreach (OsimMuscleActuatorLineProperty muscleLineprop in _muscleLineList)
            {
                muscleLineprop.UpdateMuscleLineActor();
            }

        }

        #endregion

        public void ChangeOrder(OsimControlPointProperty cpProp)
        {
            int index = cpProp.CpNumber-1;
            PathPointSet set = geometryPath.getPathPointSet();
            PathPoint pp1 = set.get(index);
            
             PathPoint pp2 = set.get(index + 1);
            geometryPath.replacePathPoint(SimModelVisualization.si, pp2, pp1);
            
            //geometryPath.moveDownPathWrap(SimModelVisualization.si, 0);
            //geometryPath.updateDisplayer(SimModelVisualization.si);
            osimModel.updateDisplayer(SimModelVisualization.si);
        }

        #endregion


    }
}
