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
    public class OsimControlPointProperty
    {

        public PathPoint pathPoint;
        public bool isOrigin = false;
        public bool isInsertion = false;
        public bool isViaPoint = false;
        public int nbViaPoint = 0;
        private double[] _position;
        private double _controlPointActorRadius = 0.0017;
        private int _CpNumber = 0;

        public OsimForceProperty osimForceProperty;
        public OsimBodyProperty parentBodyProp;


        //VTK
        public vtkRenderWindow _vtkRenderwindow;
        public vtkRenderWindow _RenderWindowImage1;
        public vtkRenderWindow _RenderWindowImage2;
        public vtkTransform controlPointTransform = vtkTransform.New();
        private vtkActor _controlPointActor = vtkActor.New();
        private Vec3 _rOffset;

        [CategoryAttribute("Muscle controlpoint Properties"), DescriptionAttribute("Order number of the control point in the muscle bundle."), ReadOnlyAttribute(true)]
        public int CpNumber
        {
            get { return _CpNumber; }
            set { _CpNumber = value; }
        }

        [CategoryAttribute("Muscle controlpoint Properties"), DescriptionAttribute("Name of the ControlPoint."), ReadOnlyAttribute(false)]
        public string objectName
        {
            get { return pathPoint.getName();}
            set { pathPoint.setName(value); }
        }
        [CategoryAttribute("Muscle controlpoint Properties"), DescriptionAttribute("Name of the Sim Body."), ReadOnlyAttribute(true)]
        public string bodyName
        {
            get { return pathPoint.getBodyName(); }
        }

        [CategoryAttribute("Muscle controlpoint Properties"), DescriptionAttribute("X Offset of the control point relative to the Sim Body."), ReadOnlyAttribute(false)]
        public double X
        {
            get { return _rOffset.get(0); }
            set { _rOffset.set(0,value); }
        }

        [CategoryAttribute("Muscle controlpoint Properties"), DescriptionAttribute("Y Offset of the control point relative to the Sim Body."), ReadOnlyAttribute(false)]
        public double Y
        {
            get { return _rOffset.get(1); }
            set { _rOffset.set(1, value); }
        }

        [CategoryAttribute("Muscle controlpoint Properties"), DescriptionAttribute("Z Offset of the control point relative to the Sim Body."), ReadOnlyAttribute(false)]
        public double Z
        {
            get { return _rOffset.get(2); }
            set { _rOffset.set(2, value); }
        }


        [Browsable(false)]
        public Vec3 rOffset
        {
            get { return _rOffset; }
            set { _rOffset = value; }
        }

        [Browsable(false)]
        public double controlPointActorRadius
        {
            get { return _controlPointActorRadius; }
            set { _controlPointActorRadius = value; }
        }

        [Browsable(false)]
        public vtkActor controlPointActor
        {
            get { return _controlPointActor; }
            set { _controlPointActor = value; }
        }

        [Browsable(false)]
        public double[] position
        {
            get { return _controlPointActor.GetPosition(); }
            set { _controlPointActor.SetPosition(value[0], value[1], value[2]); }
        }

        //[Browsable(false)]
        //public vtkTransform controlPointTransform
        //{
        //    get { return _controlPointTransform; }
        //    set { _controlPointTransform = value; }
        //}

        public void MakeControlPointActor()
        {
            _rOffset = pathPoint.getLocation();
            vtkSphereSource sphere = new vtkSphereSource();
            sphere.SetRadius(_controlPointActorRadius);

            vtkPolyDataMapper sphereMapper = vtkPolyDataMapper.New();
            sphereMapper.SetInputConnection(sphere.GetOutputPort());
            pathPoint.getBody().getName();

            _controlPointActor.SetMapper(sphereMapper);
            _controlPointActor.PickableOff();
            _controlPointActor.GetProperty().SetColor(1, 0, 0);
            _controlPointActor.SetUserTransform(controlPointTransform);
        }

        public void ScaleControlPointActor(double value)
        {
            _controlPointActor.SetScale(value);

        }

        public vtkTransform getRelativeVTKTransform(vtkTransform childTransform, vtkTransform parentTransform)
        {
            vtkTransform relativeTransform = new vtkTransform();
            vtkTransform childTransformCopie = new vtkTransform();
            childTransformCopie.DeepCopy(childTransform);

            vtkMatrix4x4 inverseMatrix = new vtkMatrix4x4();
            parentTransform.GetInverse(inverseMatrix);
            childTransformCopie.PostMultiply();
            childTransformCopie.Concatenate(inverseMatrix);
            relativeTransform = childTransformCopie;

            return relativeTransform;
        }

        public void updateCpInModel(State si, vtkTransform t)
        {
            vtkTransform d =  getRelativeVTKTransform(controlPointTransform, parentBodyProp.transform);
            double[] pos = d.GetPosition();

            //Vec3 currentOffset = pathPoint.getLocation();
            //double[] pos = t.GetPosition();

            //pos[0] = pos[0] + currentOffset.get(0);
            //pos[1] = pos[1] + currentOffset.get(1);
            //pos[2] = pos[2] + currentOffset.get(2);

            //Vec3 AbsLocationCp = new Vec3();
            //Vec3 AbsLocientationInChild = new Vec3();

            Vec3 newLoc = new Vec3();
            newLoc.set(0, pos[0]);
            newLoc.set(1, pos[1]);
            newLoc.set(2, pos[2]);

            pathPoint.setLocation(si, newLoc);
            pathPoint.update(si);
            parentBodyProp._body.updateDisplayer(si);

        }
    }
}
