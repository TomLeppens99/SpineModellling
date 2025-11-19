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
using SpineAnalyzer.ModelVisualization;
using System.Diagnostics;

namespace SpineAnalyzer.ModelVisualization
{
    public class OsimJointProperty
    {
        #region Declarations
        //SYSTEM
        private string _objectName = "WorldFrameFixed";
        private string _objectType;
        private bool _hasJoint = false;
        public OsimBodyProperty osimBodyProp;
        public OsimBodyProperty osimParentBodyProp;

        //OPENSIM
        private Body _parentBody;
        private Body _childBody;
        private Joint _joint;
        private Vec3 _locationInParent = new Vec3(0,0,0);
        private Vec3 _locationInChild = new Vec3(0, 0, 0);
        private Vec3 _orientationInParent = new Vec3(0, 0, 0);
        private Vec3 _orientationInChild = new Vec3(0, 0, 0);
        public List<OsimJointCoordinateProperty> osimJointCoordinatePropertyList = new List<OsimJointCoordinateProperty>();
        public SimModelVisualization simModelVisualization;

        //VTK
        public vtkTransform _vtkTransform = new vtkTransform();
        private vtkAssembly _assembly = new vtkAssembly();
        public vtkRenderWindow _vtkRenderwindow;
        public vtkRenderer renderer;
        public vtkActor _jointActor = vtkActor.New();
        public vtkActor axesActor = new vtkActor();
        public vtkPolyDataMapper sphereMapper = vtkPolyDataMapper.New();
        public vtkSphereSource sphere = new vtkSphereSource();

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
        [CategoryAttribute("Joint Properties"), DescriptionAttribute("Name of the Joint."), ReadOnlyAttribute(true)]
        public string objectName
        {
            get { return _objectName; }
            set { _objectName = value; }
        }

        [CategoryAttribute("Joint Properties"), DescriptionAttribute("Type of the selected node"), ReadOnlyAttribute(true)]
        public string objectType
        {
            get { return _objectType; }
            set { _objectType = value; }
        }

        [CategoryAttribute("Joint Properties"), DescriptionAttribute("OpenSim Object: Parent Body"), ReadOnlyAttribute(true)]
        public Body parentBody
        {
            get { return _parentBody; }
            set { _parentBody = value; }
        }

        [CategoryAttribute("Joint Properties"), DescriptionAttribute("OpenSim Object: Parent Body"), ReadOnlyAttribute(true)]
        public string parentBodyStr
        {
            get { return _parentBody.getName(); }
        }

        [CategoryAttribute("Joint Properties"), DescriptionAttribute("OpenSim Object: Child Body"), ReadOnlyAttribute(true)]
        public Body childBody
        {
            get { return _childBody; }
            set { _childBody = value; }
        }

        [CategoryAttribute("Joint Properties"), DescriptionAttribute("OpenSim Object: Childe Body"), ReadOnlyAttribute(true)]
        public string childBodyStr
        {
            get { return _childBody.getName(); }
        }

        [CategoryAttribute("Joint Properties"), DescriptionAttribute("OpenSim Object: Joint"), ReadOnlyAttribute(true)]
        public Joint joint
        {
            get { return _joint; }
            set { _joint = value; }
        }

        [CategoryAttribute("Joint Properties"), DescriptionAttribute("Location of the joint in the parent body"), ReadOnlyAttribute(true)]
        public Vec3 locationInParent
        {
            get { return _locationInParent; }
            set { _locationInParent = value;
                joint.setLocationInParent(value);
            }
        }

        [CategoryAttribute("Joint Properties"), DescriptionAttribute("Orientation of the joint in the parent body"), ReadOnlyAttribute(true)]
        public Vec3 orientationInParent
        {
            get { return _orientationInParent; }
            set { _orientationInParent = value;
                joint.setOrientationInParent(value);
            }
        }

        [CategoryAttribute("Joint Properties"), DescriptionAttribute("Location of the joint in the child body"), ReadOnlyAttribute(true)]
        public Vec3 locationInChild
        {
            get { return _locationInChild; }
            set { _locationInChild = value;
                joint.setLocationInChild(value);
            }
        }

        [CategoryAttribute("Joint Properties"), DescriptionAttribute("Orientation of the joint in the child body"), ReadOnlyAttribute(true)]
        public Vec3 orientationInChild
        {
            get { return _orientationInChild; }
            set { _orientationInChild = value;
                joint.set_orientation(value);
            }
        }

        [CategoryAttribute("Joint Properties"), DescriptionAttribute("Location of the joint in the parent body"), ReadOnlyAttribute(true)]
        public string locationInParentStr
        {
            get { return _locationInParent.toString(); }
        
        }

        [CategoryAttribute("Joint Properties"), DescriptionAttribute("Orientation of the joint in the parent body"), ReadOnlyAttribute(true)]
        public string orientationInParentStr
        {
            get { return _orientationInParent.toString(); }

        }

        [CategoryAttribute("Joint Properties"), DescriptionAttribute("Location of the joint in the child body"), ReadOnlyAttribute(true)]
        public string locationInChildStr
        {
            get { return _locationInChild.toString(); }

        }

        [CategoryAttribute("Joint Properties"), DescriptionAttribute("Orientation of the joint in the child body"), ReadOnlyAttribute(true)]
        public string orientationInChildStr
        {
            get { return _orientationInChild.toString(); }

        }

       
        [Browsable(false)]
        public vtkRenderWindow vtkRenderwindow
        {
            get { return _vtkRenderwindow; }
            set { _vtkRenderwindow = value; }
        }

        [Browsable(false)]
        public vtkTransform vtkTransform
        {
            get { //return _vtkTransform;
                return (vtkTransform)jointActor.GetUserTransform();
            }
            set {
                jointActor.SetUserTransform(value);
                //_vtkTransform = value; 
            }
        }

        [Browsable(false)]
        public vtkActor jointActor
        {
            get { return _jointActor; }
            set { _jointActor = value; }
        }

        [Browsable(false)]
        public ContextMenuStrip contextMenuStrip
        {
            get { return _contextMenuStrip; }
            set { _contextMenuStrip = value; }
        }
        #endregion

        #region Methods
        public void ReadJoint()
        {
            _hasJoint = true;
            _childBody = _joint.getBody();
            
            _parentBody = _joint.getParentBody();
            _objectName = _joint.getName();
            _objectType = _joint.GetType().ToString();
            _joint.getLocationInParent(_locationInParent);
            _joint.getOrientationInParent(_orientationInParent);
            _joint.getLocation(_locationInChild);
            _joint.getOrientation(_orientationInChild);

            //Debug.Write("or in parent (in ReadJoint)");
            //Debug.Write(_orientationInParent.get(0).ToString() + "  " + _orientationInParent.get(1).ToString() + "  " + _orientationInParent.get(2).ToString());
            //Debug.Write("or in Child (in ReadJoint)");
            //Debug.Write(_orientationInChild.get(0).ToString() + "  " + _orientationInChild.get(1).ToString() + "  " + _orientationInChild.get(2).ToString());

            ReadCoordinates();
            //MakeVtkObject();

        }

        public void ReadCoordinates()
        {       
            CoordinateSet coordinateSet = _joint.get_CoordinateSet();
            int numCoordinates = coordinateSet.getSize();

            for (int i = 0; i < (numCoordinates); i++)
            {
                OsimJointCoordinateProperty osimJointCoordinateProperty = new OsimJointCoordinateProperty();
                osimJointCoordinateProperty.coordinate = coordinateSet.get(i);
                osimJointCoordinateProperty.coorNumber = i + 1;
                osimJointCoordinateProperty.ReadJointCoordinate();
                osimJointCoordinatePropertyList.Add(osimJointCoordinateProperty);             
            }
        }

        public void SetTransformation()
        {
            _vtkTransform = new vtkTransform();
            _vtkTransform.Translate(_locationInChild.get(0), _locationInChild.get(1), _locationInChild.get(2));

            _vtkTransform.PreMultiply();

            _vtkTransform.RotateX((_orientationInChild.get(0)) * (180 / Math.PI));
            _vtkTransform.RotateY((_orientationInChild.get(1)) * (180 / Math.PI));
            _vtkTransform.RotateZ((_orientationInChild.get(2)) * (180 / Math.PI));

            _vtkTransform.PreMultiply();

            _vtkTransform.SetInput(osimBodyProp.transform);

            axesActor.SetUserTransform(_vtkTransform);
            jointActor.SetUserTransform(_vtkTransform);

        }

        public void SetTransformationFromParent()
        {

            //_vtkTransform = new vtkTransform();

            //_vtkTransform.Identity();
            _vtkTransform = new vtkTransform();

            _vtkTransform.Translate(_locationInParent.get(0), _locationInParent.get(1), _locationInParent.get(2));

            _vtkTransform.PreMultiply();


            _vtkTransform.RotateZ((_orientationInParent.get(2)) * ((double)180 / Math.PI));
           
            _vtkTransform.RotateY((_orientationInParent.get(1)) * ((double)180 / Math.PI));
            _vtkTransform.RotateX((_orientationInParent.get(0)) * ((double)180 / Math.PI));

            _vtkTransform.PreMultiply();

            //_vtkTransform.Translate(_locationInParent.get(0), _locationInParent.get(1), _locationInParent.get(2));
            //_vtkTransform.PreMultiply();

            //_vtkTransform.RotateX(_orientationInParent.get(0));
            //_vtkTransform.RotateY(_orientationInParent.get(1));
            //_vtkTransform.RotateZ(_orientationInParent.get(2));

            //_vtkTransform.PreMultiply();

            //_vtkTransform.SetInput(osimBodyProp.assembly.GetUserTransform()); // transform);

            _vtkTransform.SetInput(osimParentBodyProp.transform);

            axesActor.SetUserTransform(_vtkTransform);
            jointActor.SetUserTransform(_vtkTransform);

        }

        public void SetTransformation2TEMP()
        {
            //_vtkTransform = new vtkTransform();

            //_vtkTransform.Identity();
            _vtkTransform = new vtkTransform();


            _vtkTransform.Translate(_locationInChild.get(0), _locationInChild.get(1), _locationInChild.get(2));


            _vtkTransform.PreMultiply();


           
            _vtkTransform.RotateX((orientationInChild.get(0)) * (180 / Math.PI));
        
            _vtkTransform.RotateY((orientationInChild.get(1)) * (180 / Math.PI));
            _vtkTransform.RotateZ((orientationInChild.get(2)) * (180 / Math.PI));


            _vtkTransform.PreMultiply();





            //_vtkTransform.Translate(_locationInParent.get(0), _locationInParent.get(1), _locationInParent.get(2));
            //_vtkTransform.PreMultiply();

            //_vtkTransform.RotateX(_orientationInParent.get(0));
            //_vtkTransform.RotateY(_orientationInParent.get(1));
            //_vtkTransform.RotateZ(_orientationInParent.get(2));

            //_vtkTransform.PreMultiply();

            //_vtkTransform.SetInput(osimBodyProp.assembly.GetUserTransform()); // transform);

            _vtkTransform.SetInput(osimBodyProp.transform);

            axesActor.SetUserTransform(_vtkTransform);
            jointActor.SetUserTransform(_vtkTransform);

        }
        public void SetTransformation3TEMP()
        {
            //_vtkTransform = new vtkTransform();

            //_vtkTransform.Identity();
            _vtkTransform = new vtkTransform();


            _vtkTransform.Translate(_locationInParent.get(0), _locationInParent.get(1), _locationInParent.get(2));


            _vtkTransform.PreMultiply();


            _vtkTransform.RotateZ((orientationInParent.get(2)) * (180 / Math.PI));
           

            _vtkTransform.RotateY((orientationInParent.get(1)) * (180 / Math.PI));
            _vtkTransform.RotateX((orientationInParent.get(0)) * (180 / Math.PI));


            _vtkTransform.PreMultiply();





            //_vtkTransform.Translate(_locationInParent.get(0), _locationInParent.get(1), _locationInParent.get(2));
            //_vtkTransform.PreMultiply();

            //_vtkTransform.RotateX(_orientationInParent.get(0));
            //_vtkTransform.RotateY(_orientationInParent.get(1));
            //_vtkTransform.RotateZ(_orientationInParent.get(2));

            //_vtkTransform.PreMultiply();

            //_vtkTransform.SetInput(osimBodyProp.assembly.GetUserTransform()); // transform);

          
            _vtkTransform.SetInput(osimParentBodyProp.transform);

            axesActor.SetUserTransform(_vtkTransform);
            jointActor.SetUserTransform(_vtkTransform);

        }

        public void MakeVtkObject()
        {
            SetTransformation();

            sphere.SetRadius(0.0040);

            sphereMapper.SetInputConnection(sphere.GetOutputPort());

            _jointActor.SetMapper(sphereMapper);
            _jointActor.SetUserTransform(_vtkTransform);
            _jointActor.GetProperty().SetDiffuseColor(1, 0.01, 0.2);
            _jointActor.GetProperty().SetRepresentationToWireframe();

        }

        public void MakeJointAxes()
        {



                vtkLineSource vtkLineSource1 = new vtkLineSource();

                vtkLineSource1.SetPoint1(0, 0, 0);
                vtkLineSource1.SetPoint2(0.06, 0, 0);

                vtkTubeFilter vtkTubeFilter1 = new vtkTubeFilter();
                vtkTubeFilter1.SetInput(vtkLineSource1.GetOutput());
                vtkTubeFilter1.UseDefaultNormalOff();

                vtkTubeFilter1.SetRadius(0.0020);
                vtkTubeFilter1.SetNumberOfSides(8);


                vtkLineSource vtkLineSource2 = new vtkLineSource();

                vtkLineSource2.SetPoint1(0, 0, 0);
                vtkLineSource2.SetPoint2(0, 0.06, 0);

                vtkTubeFilter vtkTubeFilter2 = new vtkTubeFilter();
                vtkTubeFilter2.SetInput(vtkLineSource2.GetOutput());
                vtkTubeFilter2.UseDefaultNormalOff();

                vtkTubeFilter2.SetRadius(0.0020);
                vtkTubeFilter2.SetNumberOfSides(8);



                vtkLineSource vtkLineSource3 = new vtkLineSource();

                vtkLineSource3.SetPoint1(0, 0, 0);
                vtkLineSource3.SetPoint2(0, 0, 0.06);


                vtkTubeFilter vtkTubeFilter3 = new vtkTubeFilter();
                vtkTubeFilter3.SetInput(vtkLineSource3.GetOutput());
                vtkTubeFilter3.UseDefaultNormalOff();

                vtkTubeFilter3.SetRadius(0.0020);
                vtkTubeFilter3.SetNumberOfSides(8);



                vtkAppendPolyData vtkAppendPolyData = vtkAppendPolyData.New();
                vtkAppendPolyData.AddInputConnection(vtkTubeFilter1.GetOutputPort());
                vtkAppendPolyData.AddInputConnection(vtkTubeFilter2.GetOutputPort());
                vtkAppendPolyData.AddInputConnection(vtkTubeFilter3.GetOutputPort());



                vtkPolyDataMapper vtkPolyDataMapper = vtkPolyDataMapper.New();
                //vtkPolyDataMapper.SetInput(vtkTubeFilter.GetOutput());
                vtkPolyDataMapper.SetInput(vtkAppendPolyData.GetOutput());

                //_muscleActor.SetMapper(vtkPolyDataMapper);
                //_muscleActor.GetProperty().SetDiffuseColor(1, 0.01, 0);


                //vtkAxes axes = vtkAxes.New();
                //axes.SetOrigin(0, 0, 0);
                //axes.SetScaleFactor(0.2);



                ////vtkVectorText textLabel = vtkVectorText.New();
                ////textLabel.SetText(fileName);
                ////vtkFollower follower = vtkFollower.New();


                //vtkAppendPolyData vtkAppendPolyData = vtkAppendPolyData.New();
                ////vtkAppendPolyData.AddInputConnection(vtkTransformFilterOrignal.GetOutputPort());
                //vtkAppendPolyData.AddInputConnection(axes.GetOutputPort());    //COMMENT THIS IF YOU WANT TO HIDE THE AXES

                //vtkPolyDataMapper vtkPolyDataMapper = vtkPolyDataMapper.New();
                //vtkPolyDataMapper.SetInputConnection(vtkAppendPolyData.GetOutputPort());
                ////vtkPolyDataMapper.SetProgressText(fileName);   //This is used as an Actor ID.


                axesActor.SetMapper(vtkPolyDataMapper);
                axesActor.GetProperty().SetDiffuseColor(1, 0.01, 0.2);
  

           

 



            axesActor.SetUserTransform(_vtkTransform);

        }

        public void HideJoint()
        {
            _jointActor.VisibilityOff();
        }

        public void ShowJoint()
        {
            _jointActor.VisibilityOn();
        }

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

        //    hideLabel.Click += new System.EventHandler(this.hideLabel_Click);
        //    showLabel.Click += new System.EventHandler(this.showLabel_Click);
        //    showLabel.Checked = true;
        //    showOnlyLabel.Click += new System.EventHandler(this.showOnlyLabel_Click);
        //    showHideTransparantLabel.Click += new System.EventHandler(this.showHideTransparantLabel_Click);
        //    showHideTransparantLabel.Checked = true;
        //    showMassCenterLabel.Click += new System.EventHandler(this.showMassCenterLabel_Click);
        //    showAxesLabel.Click += new System.EventHandler(this.showAxesLabel_Click);
        //    pointRepresentationLabel.Click += new System.EventHandler(this.pointRepresentationLabel_Click);
        //    smoothShadedLabel.Click += new System.EventHandler(this.smoothShadedLabel_Click);
        //    wireframeLabel.Click += new System.EventHandler(this.wireframeLabel_Click);
        //    showLabel.Enabled = !_isVisible;
        //    displayLabel.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { showLabel, showOnlyLabel, hideLabel, toolStripSeparator2, showHideTransparantLabel, toolStripSeparator, pointRepresentationLabel, smoothShadedLabel, wireframeLabel });
        }
        #endregion
    }
}
