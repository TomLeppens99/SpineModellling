using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenSim;
using System.IO;
using Kitware.VTK;
using System.Runtime.InteropServices;
using System.Diagnostics;
using itk;

namespace SpineAnalyzer.ModelVisualization
{
    public class SimModelVisualization
    {
        #region Declarations

        //SYSTEM
        private string _modelFile = string.Empty;
        private string _geometryDir = string.Empty;
        public bool printedTo2Drenderer = false;
        public bool printMuscles;
        //private List<string> _vtkIDList = new List<string>();
        private List<OsimMakerProperty> _markerPropertyList = new List<OsimMakerProperty>();
        private List<OsimJointProperty> _jointPropertyList = new List<OsimJointProperty>();
        private List<OsimBodyProperty> _bodyPropertyList = new List<OsimBodyProperty>();
        private List<OsimForceProperty> _forcePropertyList = new List<OsimForceProperty>();
        private List<OsimGroupElement> _osimGroupElementList = new List<OsimGroupElement>();
        private List<OsimGroupElement> _osimGroupElementListForces = new List<OsimGroupElement>();
        public List<OsimJointCoordinateProperty> _coordinatePropertyList = new List<OsimJointCoordinateProperty>();
        public AppData appData;

        //OPENSIM
        private Model _osimModel;
        private State _si;
        public Subject Subject;
        public BodySet BodySet = new BodySet();

        //VTK
        public vtkRenderWindow RenderWindowImage1;
        public vtkRenderWindow RenderWindowImage2;
        public vtkRenderer renderer;

        public List<string> GeoemtryDirs = new List<string>();

        public EosImage EosImage1;
        public EosImage EosImage2;

        #endregion

        #region Properties
        public string modelFile
        {
            get { return _modelFile; }
            set { _modelFile = value; }
        }

        public string geometryDir
        {
            get { return _geometryDir; }
            set { _geometryDir = value; }
        }

        public Model osimModel
        {
            get { return _osimModel; }
            set { _osimModel = value; }
        }

        public State si
        {
            get { return _si; }
            set { _si = value; }
        }

      

        public List<OsimMakerProperty> markerPropertyList
        {
            get { return _markerPropertyList; }
            set { _markerPropertyList = value; }
        }

        public List<OsimBodyProperty> bodyPropertyList
        {
            get { return _bodyPropertyList; }
            set { _bodyPropertyList = value; }
        }

        public List<OsimGroupElement> osimGroupElementList
        {
            get { return _osimGroupElementList; }
            set { _osimGroupElementList = value; }
        }

        public List<OsimForceProperty> forcePropertyList
        {
            get { return _forcePropertyList; }
            set { _forcePropertyList = value; }
        }

        public List<OsimGroupElement> osimGroupElementListForces
        {
            get { return _osimGroupElementListForces; }
            set { _osimGroupElementListForces = value; }
        }
        public List<OsimJointProperty> jointPropertyList
        {
            get { return _jointPropertyList; }
            set { _jointPropertyList = value; }
        }
        #endregion

        #region Methods
        public void InitializeModelInRen(vtkRenderer renderer)
        {
            // Initialize the OpenSim system
            _si = _osimModel.initSystem();
            
            AddGroundReferenceAxes(renderer);

            InitializedBodiesInRendererNEW(renderer);
            InitializeMarkersinRenderer(renderer);

            if (printMuscles)
            {
                InitializeMusclesInRenderer(renderer);
            }
        }


        public vtkAxesActor GroundReferenceAxis;

        public void AddGroundReferenceAxes(vtkRenderer renderer)
        {
            //Set a ground reference system
            GroundReferenceAxis= vtkAxesActor.New();
            GroundReferenceAxis.AxisLabelsOn();
            GroundReferenceAxis.SetTotalLength(0.20, 0.20, 0.20);
            renderer.AddActor(GroundReferenceAxis);

        }

        public void ShowHideGroundReferenceAxes(vtkRenderer renderer, bool show)
        {
            if (show)
            {
                GroundReferenceAxis.SetVisibility(1);
            }
            else
            {
                GroundReferenceAxis.SetVisibility(0);
            }
           
        }

        public void UnhighlightEverything()
        {
            foreach(OsimBodyProperty bodyProp in _bodyPropertyList)
            {
                bodyProp.UnhighlightBody();
            }
            foreach (OsimMakerProperty markerProp in _markerPropertyList)
            {
                markerProp.UnhighlightMarker();
            }

        }

        public void ChangeVisibility()
        {


        }

        public void ChangeBodiesPickable(Boolean value)
        {
            if(value == false)
            {
                foreach (OsimBodyProperty bodyProp in _bodyPropertyList)
                {
                    bodyProp.assembly.PickableOff();
                }
            } 
            else
            {
                foreach (OsimBodyProperty bodyProp in _bodyPropertyList)
                {
                    bodyProp.assembly.PickableOn();
                }
            }
        }


        public void ChangeMarkersPickable(Boolean value)
        {
            if (value == false)
            {
                foreach (OsimMakerProperty makerProp in _markerPropertyList)
                {
                    makerProp.markerActor.PickableOff();
                }
            }
            else
            {
                foreach (OsimMakerProperty makerProp in _markerPropertyList)
                {
                    makerProp.markerActor.PickableOn();
                }
            }
        }

        public void ChangeForcesPickable(Boolean value)
        {
                foreach (OsimForceProperty forceProp in _forcePropertyList)
                {
                    forceProp.ChangePickable(value);
                }
        }
        public void ScaleBody(OsimBodyProperty bodyProp, object x, object y, object z)
        {

            ScaleSet scaleSet = new ScaleSet();
            Scale scale = new Scale();
            _si = _osimModel.initSystem();

            Vec3 currentVec3 = new Vec3();
            osimModel.getBodySet().get(bodyProp.objectName).getScaleFactors(currentVec3);
            scale.setSegmentName(bodyProp.objectName);
            Vec3 vec3_3 = new Vec3();

            if (x == null && y == null)
            {
                vec3_3 = new Vec3(1, 1, (double)z / currentVec3.get(2));
            }
            if (y == null && z == null)
            {
                vec3_3 = new Vec3((double)x / currentVec3.get(0), 1, 1);
            }
            if (x == null && z == null)
            {
                vec3_3 = new Vec3(1, (double)y / currentVec3.get(1), 1);
            }

            if (x != null && y != null && z != null)
            {
                vec3_3 = new Vec3((double)x / currentVec3.get(0), (double)y / currentVec3.get(1), (double)z / currentVec3.get(2));
            }

            scale.setScaleFactors(vec3_3);

            scaleSet.set(0, scale);
            osimModel.scale(si, scaleSet);
            osimModel.updBodySet();

            osimModel.getBodySet().get(bodyProp.objectName).getScaleFactors(currentVec3);
            bodyProp.scaleFactors = currentVec3;

            osimModel.updateDisplayer(si);

            //The lines below are needed because the program is trying to delete an object that was allready deleted internally (dll).
            System.GC.SuppressFinalize(scale);
            // System.GC.SuppressFinalize(scaleSet);   //This line may not be needed.
        }

        #region OpenSim related
        public void UpdateOsimJointDef(Body body, double incrRotX, double incrRotY, double incrRotZ, double incrTransX, double incrTransY, double incrTransZ)
        {
            _si = _osimModel.initSystem();

            Vec3 rOrientationInParent = new Vec3();
            Vec3 rLocationInParent = new Vec3();
            Vec3 rOrientationInChild = new Vec3();
            Vec3 rLocationInChild = new Vec3();

            OpenSim.Joint tempJoint = body.getJoint();
            tempJoint.getOrientationInParent(rOrientationInParent);
            tempJoint.getLocationInParent(rLocationInParent);
            tempJoint.getOrientation(rOrientationInChild);
            tempJoint.getLocation(rLocationInChild);

            
            if (tempJoint.getParentName().ToLower().Trim() == "ground")
            {

                rOrientationInChild.set(0, rOrientationInChild.get(0) - DegreeToRadian(incrRotX));
                rOrientationInChild.set(1, rOrientationInChild.get(1) - DegreeToRadian(incrRotY));
                rOrientationInChild.set(2, rOrientationInChild.get(2) - DegreeToRadian(incrRotZ));

                rLocationInChild.set(0, rLocationInChild.get(0) - incrTransX);
                rLocationInChild.set(1, rLocationInChild.get(1) - incrTransY);
                rLocationInChild.set(2, rLocationInChild.get(2) - incrTransZ);



                tempJoint.setOrientation(rOrientationInChild);
                tempJoint.setLocation(rLocationInChild); //What is the difference with setLocationInChild ????  ?

            }
            else
            {
                //Example: A translation of 2 mm in a direction defined by the parent reference frame must be devided as follows:
                // +1 mm in location in parent
                //-1 mm in location in child
                //To keep the actual joint in the center.
                rOrientationInParent.set(0, rOrientationInParent.get(0) + DegreeToRadian(incrRotX) / 2);
                rOrientationInParent.set(1, rOrientationInParent.get(1) + DegreeToRadian(incrRotY) / 2);
                rOrientationInParent.set(2, rOrientationInParent.get(2) + DegreeToRadian(incrRotZ) / 2);

                rLocationInParent.set(0, rLocationInParent.get(0) + incrTransX / 2);
                rLocationInParent.set(1, rLocationInParent.get(1) + incrTransY / 2);
                rLocationInParent.set(2, rLocationInParent.get(2) + incrTransZ / 2);

                rOrientationInChild.set(0, rOrientationInChild.get(0) - DegreeToRadian(incrRotX) / 2);
                rOrientationInChild.set(1, rOrientationInChild.get(1) - DegreeToRadian(incrRotY) / 2);
                rOrientationInChild.set(2, rOrientationInChild.get(2) - DegreeToRadian(incrRotZ) / 2);

                rLocationInChild.set(0, rLocationInChild.get(0) - incrTransX / 2);
                rLocationInChild.set(1, rLocationInChild.get(1) - incrTransY / 2);
                rLocationInChild.set(2, rLocationInChild.get(2) - incrTransZ / 2);

                tempJoint.setOrientationInParent(rOrientationInParent);
                tempJoint.setLocationInParent(rLocationInParent);
                tempJoint.setOrientation(rOrientationInChild);
                tempJoint.setLocation(rLocationInChild); //What is the difference with setLocationInChild ???? 
                



            }
            _osimModel.updBodySet();
           tempJoint.updDisplayer();
           _osimModel.updateDisplayer(_si);
            getSpecifiedJointProperty(body.getJoint()).joint = tempJoint;
        }

        public void UpdateOsimJointDefNEW(OsimBodyProperty bodyProp, double incrRotX, double incrRotY, double incrRotZ, double incrTransX, double incrTransY, double incrTransZ)
        {
            //_si = _osimModel.initSystem();

            Vec3 rOrientationInParent = new Vec3();
            Vec3 rLocationInParent = new Vec3();
            Vec3 rOrientationInChild = new Vec3();
            Vec3 rLocationInChild = new Vec3();

        
            bodyProp.osimJointProperty.joint.getOrientationInParent(rOrientationInParent);
            bodyProp.osimJointProperty.joint.getLocationInParent(rLocationInParent);
            bodyProp.osimJointProperty.joint.getOrientation(rOrientationInChild);
            bodyProp.osimJointProperty.joint.getLocation(rLocationInChild);


            if (bodyProp._parentBody.getName().ToLower().Trim() == "ground") 
            {

                rOrientationInChild.set(0, rOrientationInChild.get(0) - DegreeToRadian(incrRotX));
                rOrientationInChild.set(1, rOrientationInChild.get(1) - DegreeToRadian(incrRotY));
                rOrientationInChild.set(2, rOrientationInChild.get(2) - DegreeToRadian(incrRotZ));

                rLocationInChild.set(0, rLocationInChild.get(0) - incrTransX);
                rLocationInChild.set(1, rLocationInChild.get(1) - incrTransY);
                rLocationInChild.set(2, rLocationInChild.get(2) - incrTransZ);

                bodyProp.osimJointProperty.orientationInChild = rOrientationInChild; // joint.setOrientation(rOrientationInChild);
                bodyProp.osimJointProperty.locationInChild = rLocationInChild; // joint.setLocation(); //What is the difference with setLocationInChild ????  ?

            }
            else
            {
                //Example: A translation of 2 mm in a direction defined by the parent reference frame must be devided as follows:
                // +1 mm in location in parent
                //-1 mm in location in child
                //To keep the actual joint in the center.
                rOrientationInParent.set(0, rOrientationInParent.get(0) + DegreeToRadian(incrRotX) / 2);
                rOrientationInParent.set(1, rOrientationInParent.get(1) + DegreeToRadian(incrRotY) / 2);
                rOrientationInParent.set(2, rOrientationInParent.get(2) + DegreeToRadian(incrRotZ) / 2);

                rLocationInParent.set(0, rLocationInParent.get(0) + incrTransX / 2);
                rLocationInParent.set(1, rLocationInParent.get(1) + incrTransY / 2);
                rLocationInParent.set(2, rLocationInParent.get(2) + incrTransZ / 2);

                rOrientationInChild.set(0, rOrientationInChild.get(0) - DegreeToRadian(incrRotX) / 2);
                rOrientationInChild.set(1, rOrientationInChild.get(1) - DegreeToRadian(incrRotY) / 2);
                rOrientationInChild.set(2, rOrientationInChild.get(2) - DegreeToRadian(incrRotZ) / 2);

                rLocationInChild.set(0, rLocationInChild.get(0) - incrTransX / 2);
                rLocationInChild.set(1, rLocationInChild.get(1) - incrTransY / 2);
                rLocationInChild.set(2, rLocationInChild.get(2) - incrTransZ / 2);

                bodyProp.osimJointProperty.orientationInParent = rOrientationInParent;  //setOrientationInParent(rOrientationInParent);
                bodyProp.osimJointProperty.locationInParent = rLocationInParent; // joint.setLocationInParent();
                bodyProp.osimJointProperty.orientationInChild= rOrientationInChild;//  joint.setOrientation();
                bodyProp.osimJointProperty.locationInChild = rLocationInChild; // joint.setLocation(); //What is the difference with setLocationInChild ???? 

            }
            //_osimModel.updBodySet();
            //tempJoint.updDisplayer();
            //_osimModel.updateDisplayer(_si);
            //getSpecifiedJointProperty(body.getJoint()).joint = tempJoint;
        }


        public void BodyRotateX(Body body, double value)
        {
            OsimBodyProperty bodyProp = getSpecifiedBodyProperty(body);
            //vtkTransform tempTrans = vtkTransform.New();
            //tempTrans = bodyProp.transform;
            //tempTrans.RotateX(value);
            //bodyProp.transform = tempTrans;
            if (bodyProp.isGround)
            { return; }
            UpdateOsimJointDef(body, value, 0, 0, 0, 0, 0);
            UpdateBodyJointTransform(body);

            UpdateRenderer();
            ConeBeamCorrectBody(bodyProp);
            UpdateOpaceAssembly(bodyProp);


        }
        public void BodyRotateX(OsimBodyProperty bodyProp, double value)
        {
            if (bodyProp.isGround)
            { return; }
            UpdateOsimJointDefNEW(bodyProp, value, 0, 0, 0, 0, 0);
            UpdateBodyJointTransform_NEW(bodyProp);

            UpdateRenderer();
            ConeBeamCorrectBody(bodyProp);
            UpdateOpaceAssembly(bodyProp);

        }
        public void BodyRotateY(Body body, double value)
        {
            OsimBodyProperty bodyProp = getSpecifiedBodyProperty(body);
            //vtkTransform tempTrans = vtkTransform.New();
            //tempTrans = bodyProp.transform;
            //tempTrans.RotateY(value);
            //bodyProp.transform = tempTrans;
            if (bodyProp.isGround)
            { return; }
            UpdateOsimJointDef(body, 0, value, 0, 0, 0, 0);
            UpdateBodyJointTransform(body);

            UpdateRenderer();
            ConeBeamCorrectBody(bodyProp);
            UpdateOpaceAssembly(bodyProp);

        }
        public void BodyRotateY(OsimBodyProperty bodyProp, double value)
        {
            if (bodyProp.isGround)
            { return; }
            UpdateOsimJointDefNEW(bodyProp, 0, value, 0, 0, 0, 0);

            UpdateBodyJointTransform_NEW(bodyProp);

            UpdateRenderer();
            ConeBeamCorrectBody(bodyProp);
            UpdateOpaceAssembly(bodyProp);
        }
        public void BodyRotateZ(Body body, double value)
        {
            OsimBodyProperty bodyProp = getSpecifiedBodyProperty(body);
            //vtkTransform tempTrans = vtkTransform.New();
            //tempTrans = bodyProp.transform;
            //tempTrans.RotateZ(value);
            //bodyProp.transform = tempTrans;
            if (bodyProp.isGround)
            { return; }
            UpdateOsimJointDef(body, 0, 0, value, 0, 0, 0);
            UpdateBodyJointTransform(body);

            UpdateRenderer();
            ConeBeamCorrectBody(bodyProp);
            UpdateOpaceAssembly(bodyProp);

        }
        public void BodyRotateZ(OsimBodyProperty bodyProp, double value)
        {
            if (bodyProp.isGround)
            { return; }
            UpdateOsimJointDefNEW(bodyProp, 0, 0, value, 0, 0, 0);
            UpdateBodyJointTransform_NEW(bodyProp);

            UpdateRenderer();
            ConeBeamCorrectBody(bodyProp);
            UpdateOpaceAssembly(bodyProp);


        }
        public void BodyTranslate(Body body, double valueX, double valueY, double valueZ)
        {
            OsimBodyProperty bodyProp = getSpecifiedBodyProperty(body);
            //vtkTransform tempTrans = vtkTransform.New();
            //tempTrans = bodyProp.transform;    //THIS IS THE PROBLEM
            //tempTrans.Translate(valueX, valueY, valueZ);
            //bodyProp.transform = tempTrans;
            if(bodyProp.isGround)
            { return; }

            UpdateOsimJointDef(body, 0, 0, 0, valueX, valueY, valueZ);
            UpdateBodyJointTransform(body);

        }
        public void BodyTranslate(OsimBodyProperty bodyProp, double valueX, double valueY, double valueZ)
        {
            if (bodyProp.isGround)
            { return; }

            UpdateOsimJointDefNEW(bodyProp, 0, 0, 0, valueX, valueY, valueZ);
            UpdateBodyJointTransform_NEW(bodyProp);

            UpdateRenderer();
            ConeBeamCorrectBody(bodyProp);
            UpdateOpaceAssembly(bodyProp);

        }
        public void JointTranslate(Joint joint, double valueX, double valueY, double valueZ)
        {
            OsimJointProperty jointProp = getSpecifiedJointProperty(joint);

            //if (jointProp.parentBody.isGround)
            //{ return; }

            UpdateJointPlacement(joint, 0, 0, 0, valueX, valueY, valueZ);
            jointProp.ReadJoint();
            //jointProp.MakeVtkObject();
            jointProp.SetTransformation();

            //UpdateBodyJointTransform(jointProp.parentBody);
            UpdateBodyJointTransform(jointProp.childBody);
        }

        public double[] CalculateEulerAnglesFromMatrix(vtkTransform transf)
        {
           // double[] orientation = transf.GetOrientation();

            vtkMatrix4x4 matrix = transf.GetMatrix();
            matrix.GetElement(0, 0);
            matrix.GetElement(0, 1);
            matrix.GetElement(0, 2);
            matrix.GetElement(0, 3);
            matrix.GetElement(1, 0);
            matrix.GetElement(1, 1);
            matrix.GetElement(1, 2);
            matrix.GetElement(1, 3);
            matrix.GetElement(2, 0);
            matrix.GetElement(2, 1);
            matrix.GetElement(2, 2);
            matrix.GetElement(2, 3);
            matrix.GetElement(3, 0);
            matrix.GetElement(3, 1);
            matrix.GetElement(3, 2);
            matrix.GetElement(3, 3);



            double sy = Math.Sqrt(matrix.GetElement(2, 2) * matrix.GetElement(2, 2) + matrix.GetElement(1, 2) * matrix.GetElement(1, 2)); //0,128838346235426

            double[] eul = { Math.Atan2(matrix.GetElement(0, 1), matrix.GetElement(0, 0)), Math.Atan2(-matrix.GetElement(0, 2), sy), Math.Atan2(matrix.GetElement(1, 2), matrix.GetElement(2, 2)) }; //1,62410062213009	-1,44159885332337	-0,859606099789089

            double[] eulInverted = { -Math.Atan2(matrix.GetElement(1, 2), matrix.GetElement(2, 2)), -Math.Atan2(-matrix.GetElement(0, 2), sy), -Math.Atan2(matrix.GetElement(0, 1), matrix.GetElement(0, 0)) }; //0,859606099789089	1,44159885332337	-1,62410062213009

            return eulInverted;

        }

        public void SetJointPlacement(Joint joint, vtkTransform childTrans, vtkTransform jointTrans, vtkTransform parentTrans)
        {

            ///Test debug 
            ///

            
            _si = _osimModel.initSystem();
            Vec3 rOrientationInParent = new Vec3();
            Vec3 rLocationInParent = new Vec3();
            Vec3 rOrientationInChild = new Vec3();
            Vec3 rLocationInChild = new Vec3();


            ///For the Parent ///
            /////////////////////

            //First determine the Delta translations
            vtkTransform relativeJointParentTransform = getRelativeVTKTransform(jointTrans, parentTrans);
           
            double[] transParent = relativeJointParentTransform.GetPosition();
            rLocationInParent.set(0, transParent[0]);
            rLocationInParent.set(1, transParent[1]);
            rLocationInParent.set(2, transParent[2]);


            //Now substract these translations form the corresponding transform, before determining the relative (delta) orientations.
            vtkTransform jointTrans2parent = new vtkTransform();
            jointTrans2parent.DeepCopy(jointTrans);
            //jointTrans2parent.PostMultiply();
            jointTrans2parent.Update();


            jointTrans2parent.Translate(-transParent[0], -transParent[1], -transParent[2]);
            jointTrans2parent.Update();

            relativeJointParentTransform = getRelativeVTKTransform(jointTrans2parent, parentTrans);


            double[] OrParent = relativeJointParentTransform.GetOrientation();
            relativeJointParentTransform.PostMultiply();
            relativeJointParentTransform.Update();

            rOrientationInParent.set(0, 0);
            rOrientationInParent.set(1, 0);
            rOrientationInParent.set(2, 0);

            double[] orTemp = CalculateEulerAnglesFromMatrix(relativeJointParentTransform);
            rOrientationInParent.set(0, orTemp[0]);
            rOrientationInParent.set(1, orTemp[1]);
            rOrientationInParent.set(2, orTemp[2]);




            ///For the Child ///
            ////////////////////


            //First determine the Delta orientations
            vtkTransform relativeJointChildTransform = getRelativeVTKTransform(jointTrans, childTrans);
            relativeJointChildTransform.PostMultiply();
            relativeJointChildTransform.Update();


            double[] OrChild = relativeJointChildTransform.GetOrientation();

            rOrientationInChild.set(0, 0);
            rOrientationInChild.set(1, 0);
            rOrientationInChild.set(2, 0);


            double[] orTempChild = CalculateEulerAnglesFromMatrix(relativeJointChildTransform);
            rOrientationInChild.set(0, orTempChild[0]);
            rOrientationInChild.set(1, orTempChild[1]);
            rOrientationInChild.set(2, orTempChild[2]);

            //rOrientationInChild.set(0, DegreeToRadian(OrChild[0]));
            //rOrientationInChild.set(1, DegreeToRadian(OrChild[1]));
            //rOrientationInChild.set(2, DegreeToRadian(OrChild[2]));

            //Now substract these Orientations form the corresponding transform, before determining the relative (delta) translations.  
            vtkTransform jointTrans2child = new vtkTransform();
            jointTrans2child.DeepCopy(jointTrans);
            jointTrans2child.PostMultiply();
            jointTrans2child.Update();

            //jointTrans2child.RotateX(-OrChild[0]);
            //jointTrans2child.RotateY(-OrChild[1]);
            //jointTrans2child.RotateZ(-OrChild[2]);
            jointTrans2child.Update();

            double[] or1 = jointTrans2child.GetOrientation();
            double[] or2 = childTrans.GetOrientation();



            relativeJointChildTransform = getRelativeVTKTransform(jointTrans2child, childTrans);
            

            double[] transChild = relativeJointChildTransform.GetPosition();

           
            rLocationInChild.set(2, transChild[2]);
            rLocationInChild.set(1, transChild[1]);
            rLocationInChild.set(0, transChild[0]);



            //Debug.Write("or in parent (in SetJointPlacement)");
            //Debug.Write(rOrientationInParent.get(0).ToString() + "  " + rOrientationInParent.get(1).ToString() + "  " + rOrientationInParent.get(2).ToString());
            //Debug.Write("or in Child (in SetJointPlacement)");
            //Debug.Write(rOrientationInChild.get(0).ToString() + "  " + rOrientationInChild.get(1).ToString() + "  " + rOrientationInChild.get(2).ToString());


            joint.setOrientationInParent(rOrientationInParent);
            joint.setLocationInParent(rLocationInParent);
            joint.setOrientation(rOrientationInChild);
            joint.setLocation(rLocationInChild);


            joint.updDisplayer();
            joint.updBody();
            _osimModel =  joint.updModel();

            _osimModel.updBodySet();
            
            _osimModel.updateDisplayer(_si);
            getSpecifiedJointProperty(joint).joint = joint;
            _si = _osimModel.initSystem();

        }

        public void UpdateJointPlacement(Joint joint, double incrRotX, double incrRotY, double incrRotZ, double incrTransX, double incrTransY, double incrTransZ)
        {

            _si = _osimModel.initSystem();

            Vec3 rOrientationInParent = new Vec3();
            Vec3 rLocationInParent = new Vec3();
            Vec3 rOrientationInChild = new Vec3();
            Vec3 rLocationInChild = new Vec3();

            joint.getOrientationInParent(rOrientationInParent);
            joint.getLocationInParent(rLocationInParent);
            joint.getOrientation(rOrientationInChild);
            joint.getLocation(rLocationInChild);

            //Example: A translation of 2 mm in a direction defined by the parent reference frame must be devided as follows:
            // +1 mm in location in parent
            //-1 mm in location in child
            //To keep the actual joint in the center.
            rOrientationInParent.set(0, rOrientationInParent.get(0) + DegreeToRadian(incrRotX) );
            rOrientationInParent.set(1, rOrientationInParent.get(1) + DegreeToRadian(incrRotY) );
            rOrientationInParent.set(2, rOrientationInParent.get(2) + DegreeToRadian(incrRotZ) );

            rLocationInParent.set(0, rLocationInParent.get(0) + incrTransX);
            rLocationInParent.set(1, rLocationInParent.get(1) + incrTransY);
            rLocationInParent.set(2, rLocationInParent.get(2) + incrTransZ);

            rOrientationInChild.set(0, rOrientationInChild.get(0) + DegreeToRadian(incrRotX) );
            rOrientationInChild.set(1, rOrientationInChild.get(1) + DegreeToRadian(incrRotY) );
            rOrientationInChild.set(2, rOrientationInChild.get(2) + DegreeToRadian(incrRotZ) );

            rLocationInChild.set(0, rLocationInChild.get(0) + incrTransX);
            rLocationInChild.set(1, rLocationInChild.get(1) + incrTransY);
            rLocationInChild.set(2, rLocationInChild.get(2) + incrTransZ);

            joint.setOrientationInParent(rOrientationInParent);
            joint.setLocationInParent(rLocationInParent);
            joint.setOrientation(rOrientationInChild);
            joint.setLocation(rLocationInChild); //What is the difference with setLocationInChild ???? 



            //OsimJointProperty jointProp = getSpecifiedBodyProperty(body).osimJointProperty;
            //jointProp.vtkTransform.Translate(-incrTransX / 2, -incrTransY / 2, -incrTransZ / 2);
            //jointProp.vtkTransform.RotateX(-DegreeToRadian(incrRotX) / 2);
            //jointProp.vtkTransform.RotateY(-DegreeToRadian(incrRotY) / 2);
            //jointProp.vtkTransform.RotateZ(-DegreeToRadian(incrRotZ) / 2);

            //jointProp.vtkTransform.RotateX(-(incrRotX) / 2);
            //jointProp.vtkTransform.RotateY(-(incrRotY) / 2);
            //jointProp.vtkTransform.RotateZ(-(incrRotZ) / 2);


            //((vtkTransform)jointProp.jointActor.GetUserTransform()).Translate(-incrTransX / 2, -incrTransY / 2, -incrTransZ / 2);
            //((vtkTransform)jointProp.jointActor.GetUserTransform()).RotateX(-DegreeToRadian(incrRotX) / 2);
            //((vtkTransform)jointProp.jointActor.GetUserTransform()).RotateY(-DegreeToRadian(incrRotY) / 2);
            //((vtkTransform)jointProp.jointActor.GetUserTransform()).RotateZ(-DegreeToRadian(incrRotZ) / 2);



            _osimModel.updBodySet();
            joint.updDisplayer();
            _osimModel.updateDisplayer(_si);
            getSpecifiedJointProperty(joint).joint = joint;

        }

        public void ReadModel()
        {
                _osimModel = new Model(_modelFile);
              
                ClearPropertyLists();
                FillPropertyLists();
        }

        //public string[] GetBodyNames()
        //{
        //    BodySet = _osimModel.getBodySet();
        //    int numberBodies = BodySet.getSize();   
        //    string[] names = new string[numberBodies-1];
        //    for (int i = 1; i < numberBodies; i++)
        //    {
        //        Body body = BodySet.get(i);
        //        string name = body.getName();
        //        names[i-1]= name;
        //    }
        //    _bodyNames = names;
        //    return names;
        //}

        public OsimBodyProperty getSpecifiedBodyProperty(Body body1)
        {
            string name = body1.getName();
            int index = _bodyPropertyList.FindIndex(x => x.objectName == name);
            return _bodyPropertyList[index];
        }

        public OsimBodyProperty getSpecifiedBodyPropertyFromName(string name)
        {
            int index = _bodyPropertyList.FindIndex(x => x.objectName == name);
            return _bodyPropertyList[index];
        }

        public OsimJointProperty getSpecifiedJointProperty(Joint joint1)
        {
            string name = joint1.getName();
            int index = _jointPropertyList.FindIndex(x => x.objectName == name);
            return _jointPropertyList[index];
        }

        public OsimJointProperty getSpecifiedJointPropertyFromName(string name)
        {
            int index = _jointPropertyList.FindIndex(x => x.objectName == name);
            return _jointPropertyList[index];
        }



        public OsimForceProperty getSpecifiedForceProperty(Muscle muscle1)
        {
            string name = muscle1.getName();
            int index = _forcePropertyList.FindIndex(x => x.objectName == name);
            return _forcePropertyList[index];
        }

        public OsimForceProperty getSpecifiedForcePropertyFromName(string name)
        {
            int index = _forcePropertyList.FindIndex(x => x.objectName == name);
            return _forcePropertyList[index];
        }


        public OsimGroupElement getSpecifiedForceGroupPropertyFromName(string name)
        {
            int index = _osimGroupElementListForces.FindIndex(x => x.groupName == name);
            return _osimGroupElementListForces[index];
        }

        public OsimMakerProperty getSpecifiedMarkerProperty(Marker marker1)
        {
            string name = marker1.getName();
            int index = _markerPropertyList.FindIndex(x => x.objectName == name);
            return _markerPropertyList[index];

        }

        public OsimMakerProperty getSpecifiedMarkerPropertyFromName(string name)
        {
            int index = _markerPropertyList.FindIndex(x => x.objectName == name);
            return _markerPropertyList[index];
        }

        public OsimJointCoordinateProperty getSpecifiedCoorPropertyFromName(string name)
        {

            int index = _coordinatePropertyList.FindIndex(x => x.objectName == name);
            return _coordinatePropertyList[index];
        }

        public OsimJointCoordinateProperty getSpecifiedCoorPropertyFromName(OsimJointProperty joint, string name)
        {

            int index = joint.osimJointCoordinatePropertyList.FindIndex(x => x.objectName == name);
            return joint.osimJointCoordinatePropertyList[index];
        }

        public void GetAbsoluteLocation()
        {

            
            //CoordinateSet modelCoordinateSet = osimModel.updCoordinateSet();
            //Coordinate coor = new Coordinate();
            //coor = modelCoordinateSet.get(1);
            Body body = BodySet.get("femur_r");
            Joint joint;
            joint = body.getJoint();
     

            Vec3 childvec3 = new Vec3();
            //joint.getLocation(childvec3);
            //joint.getLocationInParent(childvec3);


            // transform.
            TransformAxis transformaxis = new TransformAxis();
            //transformaxis
            transformaxis.connectToJoint(joint);
           
            transformaxis.getAxis(childvec3);
            double nr0 = childvec3.get(0);
            double nr1 = childvec3.get(1);
            double nr2 = childvec3.get(2);
            Transform transform = new Transform();



            _osimModel.setUseVisualizer(true);
            _osimModel.initSystem();
            
            

            //InverseKinematicsTool iktool = new InverseKinematicsTool();
            //osimModel.visu
            //iktool.setModel(osimModel);


            //ForwardTool tool = new ForwardTool();

            //tool.setModel(osimModel);
            //tool.run();

            //osimModel.getBodySet().get("femur_r");



            //osimModel.buildSystem();
            //osimModel.getMultibodySystem();
            //Manager manager = new Manager(osimModel);
            //osimModel.updVisualizer();
            //manager.initialize(s, 10);

            

            //
            //manager.setWriteToStorage(true);
            //manager.integrate(s);

            //manager.initialize(s, 200);
        }

        public void InternalOsimVisualization()
        {
            _osimModel.setUseVisualizer(true);
            _osimModel.initSystem();
        }

        public void PrintModel(string loc)
        {
            try
            {
                _osimModel.printToXML(loc);

            }
            catch
            {
                MessageBox.Show("Something went wrong during export. Try a different folder.", "error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        
        }
        
        public void UpdateTransforms()
        {

            for (int i = 0; i < _bodyPropertyList.Count; i++)
            {
                OsimBodyProperty bodyProp = _bodyPropertyList[i];
                Body bodyTemp = bodyProp.body;
                
                Transform transform = new Transform();
                transform = _osimModel.getSimbodyEngine().getTransform(_si, bodyTemp);
                bodyProp.transform = ConvertTransformFromSim2VTK(transform);
                bodyProp.assembly.SetUserTransform(ConvertTransformFromSim2VTK(transform));
            }
            

        }

        public void ChangeJointCoordinate(OsimJointProperty jointProp, double radian,  vtkRenderer ren1, OsimJointCoordinateProperty jointCoorProp)
        {
            
            jointCoorProp.coordinate.setLocked(si, false);  // (SimModelVisualization.si, radian);
            jointCoorProp.coordinate.setDefaultValue(radian);
            jointCoorProp.coordinate.setValue(si, radian);
            //richTextBox1.AppendText(jointProp.osimJointCoordinatePropertyList[2].coordinate.getName());
            //richTextBox1.Text += Environment.NewLine;

            _osimModel.updBodySet();
            UpdateRenderer(ren1);
            ren1.GetRenderWindow().Render();
        }

        public void DeleteSelectedMarker(Marker marker1)
        {
            int size = osimModel.getMarkerSet().getSize();
            MarkerSet markerset = osimModel.getMarkerSet();
            ArrayStr markersToRetain = new ArrayStr();
            for (int i = 0; i < size; i++)
            {
                //Marker markerEmpty = new Marker();
                //markerset.set(i, markerEmpty);
                if (markerset.get(i).getName() != marker1.getName())
                {
                    markersToRetain.append(markerset.get(i).getName());
                }

            }
            osimModel.deleteUnusedMarkers(markersToRetain);
            //osimModel.updateMarkerSet(markerset);
            osimModel.updDisplayer();
            //osimModel.updModel();
        }
        #endregion

        #region VTK methods

        public void AddReferenceCubeToRenderer(vtkRenderer ren1, vtkRenderWindowInteractor iren)
        {
            //FIXED ITEM TO THE SCREEN
            vtkAnnotatedCubeActor cube = vtkAnnotatedCubeActor.New();
            cube.SetXPlusFaceText("R");
            cube.SetXMinusFaceText("L");
            cube.SetYPlusFaceText("P");
            cube.SetYMinusFaceText("A");
            cube.SetZPlusFaceText("I");
            cube.SetZMinusFaceText("S");
            cube.SetXFaceTextRotation(180);
            cube.SetYFaceTextRotation(180);
            cube.SetZFaceTextRotation(-90);
            cube.SetFaceTextScale(0.65);
            cube.GetCubeProperty().SetColor(0.5, 0.8, 0.6);

            vtkAxesActor axes = vtkAxesActor.New();
            axes.SetShaftTypeToCylinder();
            //axes.SetXAxisLabelText("X");
            //axes.SetYAxisLabelText("Y");
            //axes.SetZAxisLabelText("Z");
            axes.SetTotalLength(1.5, 1.5, 1.5);

            vtkTextProperty tprop = vtkTextProperty.New();
            tprop.ItalicOn();
            tprop.ShadowOn();
            tprop.SetFontFamilyToArial();

            axes.GetXAxisCaptionActor2D().SetCaptionTextProperty(tprop);
            axes.GetYAxisCaptionActor2D().SetCaptionTextProperty(tprop);
            axes.GetZAxisCaptionActor2D().SetCaptionTextProperty(tprop);

            vtkPropAssembly assembly = vtkPropAssembly.New();
            assembly.AddPart(axes);
            assembly.AddPart(cube);

            vtkOrientationMarkerWidget marker = vtkOrientationMarkerWidget.New();
            marker.SetOutlineColor(0.93, 0.57, 0.13);
            marker.SetOrientationMarker(assembly);
            marker.SetViewport(0, 0, 0.2, 0.2);
            marker.SetInteractor(iren);
            marker.SetEnabled(1);
            marker.InteractiveOff();
        }

        public void InitializedBodiesInRenderer(vtkRenderer renderer)
        {
            foreach (OsimBodyProperty bodyProp in _bodyPropertyList)
            {

               // bodyProp.renderer = renderer;
                //bodyProp.vtkRenderwindow = renderer.GetRenderWindow();


                OpenSim.Body body = bodyProp.body;

                GeometrySet GeometrySet = new GeometrySet();
                GeometrySet = body.getDisplayer().getGeometrySet();
                Vec3 scaleFactors = new Vec3();
                body.getScaleFactors(scaleFactors);

                #region Making an actor for each geometry and adding it to an Assembly
                int nrGeometries = GeometrySet.getSize();
                for (int j = 0; j < nrGeometries; j++)
                {
                    
                    OsimGeometryProperty geomProp = new OsimGeometryProperty();
                    geomProp.Model = osimModel;
                    geomProp.ReadGeometry(GeometrySet.get(j));
                    bodyProp.colorR = geomProp.geomColorR;
                    bodyProp.colorG = geomProp.geomColorG;
                    bodyProp.colorB = geomProp.geomColorB;
                    //Check if the geometry exists in the Geometry directory (set in settings)
                    if (File.Exists(Path.Combine(geometryDir, geomProp.geometryFile)))
                    {
                        geomProp.geometryDirAndFile = Path.Combine(geometryDir, geomProp.geometryFile);
                    }
                    else
                    {
                        //Check if the filename (without extension) is a number  
                        //TO: the identification by number should be more advanced/robust.
                        int n;
                        bool isNumeric = int.TryParse(Path.GetFileNameWithoutExtension(geomProp.geometryFile), out n);

                        if (isNumeric)
                        {
                            //Try to find the Geometry in the database. 
                            int geomNumber = n;
                            DataBase SQLDB = new DataBase(appData.SQLServer, appData.SQLDatabase, appData.SQLAuthSQL, appData.SQLUser, appData.SQLPassword);
                            Acquisitions.GeometryFiles GeometryFiles = new SpineAnalyzer.Acquisitions.GeometryFiles(SQLDB, Subject, geomNumber);

                            if (File.Exists(Path.GetFullPath(appData.AcquisitionDir) + GeometryFiles.Directory))
                            {
                                geomProp.geometryDirAndFile = Path.GetFullPath(appData.AcquisitionDir) + GeometryFiles.Directory;
                                geomProp.loadedFromDatabase = true;
                                geomProp.geometryFileObject = GeometryFiles;
                            }
                            else
                            {
                                MessageBox.Show("Geometry could not be found!", "Geometry not found!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }

                        }
                        else
                        {
                            MessageBox.Show("Geometry could not be found!", "Geometry not found!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                    }
                    bodyProp._OsimGeometryPropertyList.Add(geomProp);

                    geomProp.MakeVTKActor();

                    Transform internalTransform = geomProp.internalTransform;
                    if (geomProp._vtkActor == null)
                    { return; }
                    geomProp._vtkActor.SetUserTransform(ConvertTransformFromSim2VTK(internalTransform));

                    bodyProp.assembly.AddPart(geomProp._vtkActor);
                }
                #endregion

                bodyProp.assembly.SetScale(scaleFactors.get(0), scaleFactors.get(1), scaleFactors.get(2));

                Body parent = new Body();
                Transform absoluteChildTransform = new Transform();
                absoluteChildTransform = _osimModel.getSimbodyEngine().getTransform(_si, body);

                //absoluteChildTransform = body.getJoint().getLocationInChild();

                /// METHOD 2
                vtkTransform relativeChildTransform = new vtkTransform();
               // vtkTransform relativeParentTransform = new vtkTransform();
                vtkTransform currentTransform = new vtkTransform();

                Transform absoluteParentTransform = new Transform();  //OpenSim Transformation
               
                if (body.hasJoint())
                {

                    parent = body.getJoint().getParentBody();
                 
                    absoluteParentTransform = _osimModel.getSimbodyEngine().getTransform(_si, parent);
                    //relativeParentTransform = (vtkTransform)getSpecifiedBodyProperty(parent).assembly.GetUserTransform();
                    bodyProp._transformChild = ConvertTransformFromSim2VTK(absoluteChildTransform);
                    relativeChildTransform = getRelativeVTKTransform(bodyProp._transformChild, ConvertTransformFromSim2VTK(absoluteParentTransform));

                    relativeChildTransform.SetInput(getSpecifiedBodyProperty(parent).assembly.GetUserTransform());
                    bodyProp.assembly.SetUserTransform(relativeChildTransform);
                    bodyProp.transform = relativeChildTransform;


                    //bodyProp.osimJointProperty.renderer = ren1;
                    bodyProp.osimJointProperty.MakeVtkObject();

                    renderer.AddActor(bodyProp.osimJointProperty.jointActor);
                    renderer.AddActor(bodyProp.osimJointProperty.axesActor);
                    bodyProp.absoluteHeight = getRelativeVTKTransform(relativeChildTransform, getSpecifiedBodyProperty(osimModel.getGroundBody()).transform).GetPosition()[1];
                    // bodyProp.absoluteHeight = getRelativeVTKTransform(currentTransform, getSpecifiedBodyProperty(osimModel.getGroundBody()).transform).GetPosition()[1];
                }
                else
                {
                    parent = null;
                    vtkTransform transf = ConvertTransformFromSim2VTK(absoluteChildTransform);
                    // transf.RotateY(90);
                    bodyProp.assembly.SetUserTransform(transf);

                    bodyProp.transform = ConvertTransformFromSim2VTK(absoluteChildTransform);

                }

                renderer.AddActor(bodyProp.assembly);


                /// METHOD 1
                //bodyProp.transform = ConvertTransformFromSim2VTK(absoluteChildTransform);
                //bodyProp.assembly.SetUserTransform(ConvertTransformFromSim2VTK(absoluteChildTransform));


                //-----
                //Vec3 aPoint = new Vec3();
                //Vec3 rPos = new Vec3();
                //aPoint.set(0, 0.20);
                //aPoint.set(1, 0.20);
                //aPoint.set(2, 0.20);
                //_osimModel.getSimbodyEngine().getPosition(si, body, aPoint, rPos);
                //_osimModel.getSimbodyEngine().
                //rPos.get(0);
                //rPos.get(1);
                //rPos.get(2);
            }
        }

        public void InitializedBodiesInRendererNEW(vtkRenderer renderer)
        {
            foreach (OsimBodyProperty bodyProp in _bodyPropertyList)
            {
                GeometrySet GeometrySet = new GeometrySet();
                GeometrySet = bodyProp.body.getDisplayer().getGeometrySet();
                Vec3 scaleFactors = new Vec3();
                bodyProp.body.getScaleFactors(scaleFactors);

                #region Making an actor for each geometry and adding it to an Assembly of the object body
                int nrGeometries = GeometrySet.getSize();
                for (int j = 0; j < nrGeometries; j++)
                {
                    OsimGeometryProperty geomProp = new OsimGeometryProperty();
                    geomProp.Model = osimModel;
                    geomProp.ReadGeometry(GeometrySet.get(j));
                    geomProp.IndexNumberOfGeometry = j;
                    bodyProp.colorR = geomProp.geomColorR;
                    bodyProp.colorG = geomProp.geomColorG;
                    bodyProp.colorB = geomProp.geomColorB;

                    FindAndInitializeGeometryObject(geomProp);

                    bodyProp._OsimGeometryPropertyList.Add(geomProp);

                    if (geomProp != null && geomProp._vtkActor != null)
                    {
                        geomProp.internalTransformVTK = ConvertTransformFromSim2VTK(geomProp.internalTransform);
                        bodyProp.assembly.AddPart(geomProp._vtkActor);
                    }
                }
                #endregion

                bodyProp.assembly.SetScale(scaleFactors.get(0), scaleFactors.get(1), scaleFactors.get(2));

                Body parent = new Body();

                Transform absoluteChildTransform = new Transform();
                vtkTransform relativeChildTransform = new vtkTransform();
                vtkTransform currentTransform = new vtkTransform();
                Transform absoluteParentTransform = new Transform();  //OpenSim Transformation

                absoluteChildTransform = _osimModel.getSimbodyEngine().getTransform(_si, bodyProp.body);
             
                bodyProp.absoluteChildTransform = absoluteChildTransform;


                if (bodyProp.body.hasJoint())
                {
                    parent = bodyProp.body.getJoint().getParentBody();
                    absoluteParentTransform = _osimModel.getSimbodyEngine().getTransform(_si, parent);
                    bodyProp.absoluteParentTransform = absoluteParentTransform;
                    relativeChildTransform = getRelativeVTKTransform(ConvertTransformFromSim2VTK(absoluteChildTransform), ConvertTransformFromSim2VTK(absoluteParentTransform));
                    relativeChildTransform.SetInput(getSpecifiedBodyProperty(parent).transform);
                    bodyProp.assembly.SetUserTransform(relativeChildTransform);

                    bodyProp.osimJointProperty.MakeVtkObject();
                    bodyProp.osimJointProperty.MakeJointAxes();

                    renderer.AddActor(bodyProp.osimJointProperty.jointActor);
                    renderer.AddActor(bodyProp.osimJointProperty.axesActor);

                    bodyProp.absoluteHeight = getRelativeVTKTransform(relativeChildTransform, getSpecifiedBodyProperty(osimModel.getGroundBody()).transform).GetPosition()[1];
                   
                }
                else
                {
                    vtkTransform transf = ConvertTransformFromSim2VTK(absoluteChildTransform);
                    // transf.RotateY(90);
                    bodyProp.assembly.SetUserTransform(transf);
                    bodyProp.transform = transf;
                }

                renderer.AddActor(bodyProp.assembly);
            }

            if(OneOrmoreGeomsNotfound)
            {
                MessageBox.Show("On or more geometry files could not be found! Check the preferences to allow the software to search in multiple directories. Close and reload.", "Geometry not found!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
       }

     
        private bool checkIfGeometryExistsInDir(string dir, OsimGeometryProperty geomProp)
        {

            if(dir.ToLower().Trim() ==  "SWS".ToLower())
            {
                int n;
                bool isNumeric = int.TryParse(Path.GetFileNameWithoutExtension(geomProp.geometryFile), out n);

                if (isNumeric)
                {
                    //Try to find the Geometry in the database. 
                    int geomNumber = n;
                    DataBase SQLDB = new DataBase(appData.SQLServer, appData.SQLDatabase, appData.SQLAuthSQL, appData.SQLUser, appData.SQLPassword);
                    Acquisitions.GeometryFiles GeometryFiles = new SpineAnalyzer.Acquisitions.GeometryFiles(SQLDB, Subject, geomNumber);

                    if(File.Exists(Path.GetFullPath(appData.AcquisitionDir) + GeometryFiles.Directory))
                    {
                        geometryDir = appData.AcquisitionDir;
                        geomProp.loadedFromDatabase = true;
                        geomProp.geometryFileObject = GeometryFiles;
                        geomProp.geometryDirAndFile = Path.GetFullPath(geometryDir) + GeometryFiles.Directory;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {

                    if (File.Exists(Path.Combine( Path.GetFullPath(appData.GeometryDir), geomProp.geometryFile) ))
                    {
                        geometryDir = Path.GetFullPath(appData.GeometryDir);
                        geomProp.loadedFromDatabase = true;
                        //geomProp.geometryFileObject = GeometryFiles;
                        geomProp.geometryDirAndFile = Path.Combine(Path.GetFullPath(appData.GeometryDir), geomProp.geometryFile) ;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
               
            }
            else
            {

                if(File.Exists(Path.Combine(dir, geomProp.geometryFile)))
                    {
                    geometryDir = dir;
                    geomProp.geometryDirAndFile = Path.Combine(geometryDir, geomProp.geometryFile);
                    return true;
                }
                else
                { return false; }
         
            }

        }

        public bool OneOrmoreGeomsNotfound = false;
        public void FindAndInitializeGeometryObject(OsimGeometryProperty geomProp)
        {
            int i = 0;
            while(!checkIfGeometryExistsInDir(GeoemtryDirs[i], geomProp))
            {
                i++;
                if(i==GeoemtryDirs.Count)
                {
                    OneOrmoreGeomsNotfound = true;
                    geometryDir = string.Empty;
                    return;
                }
            }

            geomProp.MakeVTKActor();
        }

        public void UpdateBodyJointTransform(Body body)
        {
            _si = _osimModel.initSystem();
            OsimBodyProperty bodyProp = getSpecifiedBodyProperty(body);
            Transform absoluteChildTransform = new Transform();
            absoluteChildTransform = _osimModel.getSimbodyEngine().getTransform(si, body);
            Transform absoluteParentTransform = new Transform();
            vtkTransform relativeChildTransform = new vtkTransform();
            vtkTransform relativeParentTransform = new vtkTransform();
            vtkTransform currentTransform = new vtkTransform();
            Body parent = body.getJoint().getParentBody();
             
            absoluteParentTransform = _osimModel.getSimbodyEngine().getTransform(si, parent);
            relativeParentTransform = (vtkTransform)getSpecifiedBodyProperty(parent).assembly.GetUserTransform();

            relativeChildTransform = getRelativeVTKTransform(ConvertTransformFromSim2VTK(absoluteChildTransform), ConvertTransformFromSim2VTK(absoluteParentTransform));

            currentTransform.Translate(relativeChildTransform.GetPosition()[0], relativeChildTransform.GetPosition()[1], relativeChildTransform.GetPosition()[2]);
            currentTransform.PreMultiply();

            currentTransform.RotateZ(relativeChildTransform.GetOrientation()[2]);
            currentTransform.RotateY(relativeChildTransform.GetOrientation()[1]);
            currentTransform.RotateX(relativeChildTransform.GetOrientation()[0]);

            
            currentTransform.PreMultiply();
             
            currentTransform.SetInput(getSpecifiedBodyProperty(parent).assembly.GetUserTransform());  //This should then change in the user transform of the joint transform below.
                                                                                                      //  bodyProp.transform.Concatenate(getRelativeVTKTransform(currentTransform, bodyProp.transform));
            double x = getRelativeVTKTransform(currentTransform, bodyProp.transform).GetPosition()[0];
            double y = getRelativeVTKTransform(currentTransform, bodyProp.transform).GetPosition()[1];
            double z = getRelativeVTKTransform(currentTransform, bodyProp.transform).GetPosition()[2];
            double[] orient =  getRelativeVTKTransform(currentTransform, bodyProp.transform).GetOrientation();

            vtkTransform  test = (vtkTransform)bodyProp.assembly.GetUserTransform();
            test.PostMultiply();
            test.RotateZ(orient[2]);
            test.RotateY(orient[1]);
            test.RotateX(orient[0]);
            test.Translate(x, y, z);
            bodyProp.transform = test;




            UpdateMuscleLineActuators();
        }

       
        public void UpdateBodyJointTransform_NEW(OsimBodyProperty bodyProp)
        {

            _si = _osimModel.initSystem();
            bodyProp.absoluteChildTransform = new Transform();
            bodyProp.absoluteChildTransform = _osimModel.getSimbodyEngine().getTransform(_si, bodyProp.body);
            return;


           

         
            Transform absoluteParentTransform = new Transform();
            vtkTransform relativeChildTransform = new vtkTransform();
           // vtkTransform relativeParentTransform = new vtkTransform();
            vtkTransform currentTransform = new vtkTransform();
            Body parent = bodyProp._parentBody; // body.getJoint().getParentBody(); TO 16/6 this was changed.

            absoluteParentTransform = _osimModel.getSimbodyEngine().getTransform(_si, parent);
            //relativeParentTransform = (vtkTransform)getSpecifiedBodyProperty(parent).assembly.GetUserTransform();

            relativeChildTransform = getRelativeVTKTransform(ConvertTransformFromSim2VTK(bodyProp.absoluteChildTransform), ConvertTransformFromSim2VTK(absoluteParentTransform));

            currentTransform.Translate(relativeChildTransform.GetPosition()[0], relativeChildTransform.GetPosition()[1], relativeChildTransform.GetPosition()[2]);
            currentTransform.PreMultiply();

            currentTransform.RotateZ(relativeChildTransform.GetOrientation()[2]);
            currentTransform.RotateY(relativeChildTransform.GetOrientation()[1]);
            currentTransform.RotateX(relativeChildTransform.GetOrientation()[0]);

            //currentTransform.RotateZ(relativeChildTransform.GetOrientation()[2]); //TO deze volgorde nog eens checken.
            //currentTransform.RotateX(relativeChildTransform.GetOrientation()[0]);
            //currentTransform.RotateY(relativeChildTransform.GetOrientation()[1]);


            currentTransform.PreMultiply();

            currentTransform.SetInput(getSpecifiedBodyProperty(parent).transform);  //This should then change in the user transform of the joint transform below.
                                                                                                      //  bodyProp.transform.Concatenate(getRelativeVTKTransform(currentTransform, bodyProp.transform));
            double x = getRelativeVTKTransform(currentTransform, bodyProp.transform).GetPosition()[0];
            double y = getRelativeVTKTransform(currentTransform, bodyProp.transform).GetPosition()[1];
            double z = getRelativeVTKTransform(currentTransform, bodyProp.transform).GetPosition()[2];
            double[] orient = getRelativeVTKTransform(currentTransform, bodyProp.transform).GetOrientation();

            vtkTransform test = (vtkTransform)bodyProp.assembly.GetUserTransform();
            test.PostMultiply();
            test.RotateZ(orient[2]);   //Is het niet ZXY in mijn opensim model? 
            test.RotateY(orient[1]);
            test.RotateX(orient[0]);

            //This is a test
            //test.RotateZ(orient[2]);   //Is het niet ZXY in mijn opensim model? 
            //test.RotateX(orient[0]);
            //test.RotateY(orient[1]);

            test.Translate(x, y, z);
            bodyProp.transform = test;


            if(printMuscles)
            {
                UpdateMuscleLineActuators();
            }
         

        }

        public void InitializeMusclesInRenderer(vtkRenderer ren1)
        {
            for (int i = 0; i < _forcePropertyList.Count; i++)
            {
                if (_forcePropertyList[i].isMuscle)
                {
                    _forcePropertyList[i].ren1 = ren1;
                _forcePropertyList[i].CreateTheObjects();

                foreach (OsimControlPointProperty osimControlPointProperty in _forcePropertyList[i].controlPointsList)
                {
                    osimControlPointProperty.parentBodyProp = getSpecifiedBodyProperty(osimControlPointProperty.pathPoint.getBody());
                    osimControlPointProperty.controlPointTransform.Translate(osimControlPointProperty.rOffset.get(0), osimControlPointProperty.rOffset.get(1), osimControlPointProperty.rOffset.get(2));
                    osimControlPointProperty.controlPointTransform.PreMultiply();
                    osimControlPointProperty.controlPointTransform.SetInput(osimControlPointProperty.parentBodyProp.assembly.GetUserTransform());
                    
                    osimControlPointProperty.controlPointActor.SetUserTransform(osimControlPointProperty.controlPointTransform);
                    ren1.AddActor(osimControlPointProperty.controlPointActor);
                }


                    _forcePropertyList[i].MakeMuscleLineActors();

                    //foreach (OsimMuscleActuatorLineProperty osimMuscleActuatorLineProperty in _forcePropertyList[i].muscleLineList)
                    //{
                    //    ren1.AddActor(osimMuscleActuatorLineProperty.muscleActor);
                    //}
                }
            }
        }

        public void UpdateMuscleLineActuators()
        {

            for (int i = 0; i < _forcePropertyList.Count; i++)
            {
                if (_forcePropertyList[i].isMuscle)
                {

                    foreach (OsimMuscleActuatorLineProperty osimMuscleActuatorLineProperty in _forcePropertyList[i].muscleLineList)
                    {
                        osimMuscleActuatorLineProperty.MakeMuscleLineActor(); 
                    }

                   
                }
            }

        }

        public void HideAllMuscles()
        {

            for (int i = 0; i < _forcePropertyList.Count; i++)
            {
                if (_forcePropertyList[i].isMuscle)
                {

                    foreach (OsimMuscleActuatorLineProperty osimMuscleActuatorLineProperty in _forcePropertyList[i].muscleLineList)
                    {
                        osimMuscleActuatorLineProperty.muscleActor.VisibilityOff();
                    }
                    foreach (OsimControlPointProperty OsimControlPointProperty in _forcePropertyList[i].controlPointsList)
                    {
                        OsimControlPointProperty.controlPointActor.VisibilityOff();
                    }

                }
            }

        }

        public void ShowAllMuscles()
        {
            for (int i = 0; i < _forcePropertyList.Count; i++)
            {
                if (_forcePropertyList[i].isMuscle)
                {

                    foreach (OsimMuscleActuatorLineProperty osimMuscleActuatorLineProperty in _forcePropertyList[i].muscleLineList)
                    {
                        osimMuscleActuatorLineProperty.muscleActor.VisibilityOn();
                    }
                    foreach (OsimControlPointProperty OsimControlPointProperty in _forcePropertyList[i].controlPointsList)
                    {
                        OsimControlPointProperty.controlPointActor.VisibilityOn();
                    }

                }
            }


        }
        public void HideAllBodies()
        {
            foreach (OsimBodyProperty osimBodyProp in _bodyPropertyList)
            {
                osimBodyProp.hideProgrammatically();
            }
            
         }
        public void HideAllTranslucentBodies()
        {
            foreach (OsimBodyProperty osimBodyProp in _bodyPropertyList)
            {
                osimBodyProp.HideTranslucentProgrammatically();
            }

        }
        public void HideAllMarkers()
        {
            foreach (OsimMakerProperty osimMarkerProp in _markerPropertyList)
            {
                osimMarkerProp.isVisible = false;
            }
        }
        public void ShowAllMarkers()
        {
            foreach (OsimMakerProperty osimMarkerProp in _markerPropertyList)
            {
                osimMarkerProp.isVisible = true;
            }
        }
        public void HideAllJointSpheres()
        {
            foreach (OsimJointProperty osimjointProp in _jointPropertyList)
            {
                osimjointProp.HideJoint();
            }
        }
        public void ShowAllJointSpheres()
        {
            foreach (OsimJointProperty osimjointProp in _jointPropertyList)
            {
                osimjointProp.ShowJoint();
            }
        }

        public void HideAllJointRefFrames()
        {
            foreach (OsimJointProperty osimjointProp in _jointPropertyList)
            {
                if (osimjointProp.axesActor != null)
                {
                    osimjointProp.axesActor.VisibilityOff();
                }
            }
        }
        public void ShowAllJointRefFrames()
        {
            foreach (OsimJointProperty osimjointProp in _jointPropertyList)
            {
                if (osimjointProp.axesActor != null)
                {
                    osimjointProp.axesActor.VisibilityOn();
                }
            }
        }

        public void UpdateRenderer(vtkRenderer ren1)
        {
            #region For the Bodies
          foreach(OsimBodyProperty bodyProp in _bodyPropertyList)
            {
                //foreach (OsimGeometryProperty geomProp in bodyProp._OsimGeometryPropertyList)
                //{
                    
                //    bodyProp.colorR = geomProp.geomColorR;
                //    bodyProp.colorG = geomProp.geomColorG;
                //    bodyProp.colorB = geomProp.geomColorB;

                //    geomProp._vtkActor.GetProperty().SetColor(geomProp.geomColorR, geomProp.geomColorG, geomProp.geomColorB);
                //}

                Transform absoluteChildTransform = new Transform();
                absoluteChildTransform = _osimModel.getSimbodyEngine().getTransform(_si, bodyProp.body);

                vtkTransform relativeChildTransform = new vtkTransform();
                Transform absoluteParentTransform = new Transform();
             
                if (bodyProp.body.hasJoint())
                {
                    absoluteParentTransform = _osimModel.getSimbodyEngine().getTransform(_si, bodyProp._parentBody);
                    relativeChildTransform = getRelativeVTKTransform(ConvertTransformFromSim2VTK(absoluteChildTransform), ConvertTransformFromSim2VTK(absoluteParentTransform));
                    relativeChildTransform.SetInput(getSpecifiedBodyProperty(bodyProp._parentBody).transform);
                    bodyProp.transform = relativeChildTransform;
                }
                else
                {
                    bodyProp.transform = ConvertTransformFromSim2VTK(absoluteChildTransform);
                }
            }
            #endregion

            #region For the Markers
            foreach (OsimMakerProperty markerprop in _markerPropertyList)
            {
               markerprop.markerTransform.SetInput(markerprop.parentbodyprop.transform);
            }

            #endregion

            #region For the Joints
            foreach (OsimJointProperty jointProperty in _jointPropertyList)
            {
                ////Vec3 rOffset = _jointPropertyList[i].join.marker.getOffset();
                //string test = jointProperty.osimBodyProp.objectName;
                //OsimBodyProperty childBodyProp = getSpecifiedBodyProperty(jointProperty.childBody);
                //jointProperty.osimBodyProp = childBodyProp;
                //jointProperty.ReadJoint();

                jointProperty.SetTransformation();


                //jointProperty.jointActor.SetUserTransform(jointProperty.vtkTransform);

            }

            #endregion

            #region For the Muscles
            if (printMuscles)
            {
                foreach (OsimForceProperty osimForceProperty in forcePropertyList)
                {
                    foreach (OsimControlPointProperty OsimControlPointProperty in osimForceProperty.controlPointsList)
                    {
                        Vec3 rOffset = OsimControlPointProperty.rOffset;

                        OsimBodyProperty parentBodyProp = OsimControlPointProperty.parentBodyProp;

                        vtkTransform markerTransform = vtkTransform.New();
                        markerTransform.Translate(rOffset.get(0), rOffset.get(1), rOffset.get(2));
                        markerTransform.PreMultiply();
                        markerTransform.SetInput(OsimControlPointProperty.parentBodyProp.assembly.GetUserTransform());


                        OsimControlPointProperty.controlPointActor.SetUserTransform(markerTransform);
                        OsimControlPointProperty.controlPointTransform = markerTransform;

                    }
                    osimForceProperty.UpdateMuscleLineActorTransform();
                }
            }

            #endregion
        }

        public void UpdateRenderer()
        {
            #region For the Bodies
            foreach (OsimBodyProperty bodyProp in _bodyPropertyList)
            {
                //foreach (OsimGeometryProperty geomProp in bodyProp._OsimGeometryPropertyList)
                //{

                //    bodyProp.colorR = geomProp.geomColorR;
                //    bodyProp.colorG = geomProp.geomColorG;
                //    bodyProp.colorB = geomProp.geomColorB;

                //    geomProp._vtkActor.GetProperty().SetColor(geomProp.geomColorR, geomProp.geomColorG, geomProp.geomColorB);
                //}

                Transform absoluteChildTransform = new Transform();
                absoluteChildTransform = _osimModel.getSimbodyEngine().getTransform(_si, bodyProp.body);

                vtkTransform relativeChildTransform = new vtkTransform();
                Transform absoluteParentTransform = new Transform();

                if (bodyProp.body.hasJoint())
                {
                    absoluteParentTransform = _osimModel.getSimbodyEngine().getTransform(_si, bodyProp._parentBody);
                    relativeChildTransform = getRelativeVTKTransform(ConvertTransformFromSim2VTK(absoluteChildTransform), ConvertTransformFromSim2VTK(absoluteParentTransform));
                    relativeChildTransform.SetInput(getSpecifiedBodyProperty(bodyProp._parentBody).transform);
                    bodyProp.transform = relativeChildTransform;
                }
                else
                {
                    bodyProp.transform = ConvertTransformFromSim2VTK(absoluteChildTransform);
                }
            }
            #endregion

            #region For the Markers
            foreach (OsimMakerProperty markerprop in _markerPropertyList)
            {
                markerprop.markerTransform.SetInput(markerprop.parentbodyprop.transform);
            }

            #endregion

            #region For the Joints
            foreach (OsimJointProperty jointProperty in _jointPropertyList)
            {
                ////Vec3 rOffset = _jointPropertyList[i].join.marker.getOffset();
                //string test = jointProperty.osimBodyProp.objectName;
                //OsimBodyProperty childBodyProp = getSpecifiedBodyProperty(jointProperty.childBody);
                //jointProperty.osimBodyProp = childBodyProp;
                //jointProperty.ReadJoint();

                jointProperty.SetTransformation();


                //jointProperty.jointActor.SetUserTransform(jointProperty.vtkTransform);

            }

            #endregion

            #region For the Muscles
            if (printMuscles)
            {
                foreach (OsimForceProperty osimForceProperty in forcePropertyList)
                {
                    foreach (OsimControlPointProperty OsimControlPointProperty in osimForceProperty.controlPointsList)
                    {
                        Vec3 rOffset = OsimControlPointProperty.rOffset;

                        OsimBodyProperty parentBodyProp = OsimControlPointProperty.parentBodyProp;

                        vtkTransform markerTransform = vtkTransform.New();
                        markerTransform.Translate(rOffset.get(0), rOffset.get(1), rOffset.get(2));
                        markerTransform.PreMultiply();
                        markerTransform.SetInput(OsimControlPointProperty.parentBodyProp.assembly.GetUserTransform());


                        OsimControlPointProperty.controlPointActor.SetUserTransform(markerTransform);
                        OsimControlPointProperty.controlPointTransform = markerTransform;

                    }
                    osimForceProperty.UpdateMuscleLineActorTransform();
                }
            }

            #endregion
        }

        public void UpdateRendererSpecificBody(OsimBodyProperty bodyProp)
        {
            #region For the Bodies
            //foreach (OsimBodyProperty bodyProp in _bodyPropertyList)
            //{
                //foreach (OsimGeometryProperty geomProp in bodyProp._OsimGeometryPropertyList)
                //{

                //    bodyProp.colorR = geomProp.geomColorR;
                //    bodyProp.colorG = geomProp.geomColorG;
                //    bodyProp.colorB = geomProp.geomColorB;

                //    geomProp._vtkActor.GetProperty().SetColor(geomProp.geomColorR, geomProp.geomColorG, geomProp.geomColorB);
                //}

                Transform absoluteChildTransform = new Transform();
                absoluteChildTransform = _osimModel.getSimbodyEngine().getTransform(_si, bodyProp.body);

                vtkTransform relativeChildTransform = new vtkTransform();
                Transform absoluteParentTransform = new Transform();

                if (bodyProp.body.hasJoint())
                {
                    absoluteParentTransform = _osimModel.getSimbodyEngine().getTransform(_si, bodyProp._parentBody);
                    relativeChildTransform = getRelativeVTKTransform(ConvertTransformFromSim2VTK(absoluteChildTransform), ConvertTransformFromSim2VTK(absoluteParentTransform));
                    relativeChildTransform.SetInput(getSpecifiedBodyProperty(bodyProp._parentBody).transform);
                    bodyProp.transform = relativeChildTransform;
                }
                else
                {
                    bodyProp.transform = ConvertTransformFromSim2VTK(absoluteChildTransform);
                }
            //}
            #endregion

            #region For the Markers
            foreach (OsimMakerProperty markerprop in _markerPropertyList)
            {
                markerprop.markerTransform.SetInput(markerprop.parentbodyprop.transform);
            }

            #endregion

            #region For the Joints
            foreach (OsimJointProperty jointProperty in _jointPropertyList)
            {
                ////Vec3 rOffset = _jointPropertyList[i].join.marker.getOffset();
                //string test = jointProperty.osimBodyProp.objectName;
                //OsimBodyProperty childBodyProp = getSpecifiedBodyProperty(jointProperty.childBody);
                //jointProperty.osimBodyProp = childBodyProp;
                //jointProperty.ReadJoint();

                jointProperty.SetTransformation();


                //jointProperty.jointActor.SetUserTransform(jointProperty.vtkTransform);

            }

            #endregion

            #region For the Muscles
            if (printMuscles)
            {
                foreach (OsimForceProperty osimForceProperty in forcePropertyList)
                {
                    foreach (OsimControlPointProperty OsimControlPointProperty in osimForceProperty.controlPointsList)
                    {
                        Vec3 rOffset = OsimControlPointProperty.rOffset;

                        OsimBodyProperty parentBodyProp = OsimControlPointProperty.parentBodyProp;

                        vtkTransform markerTransform = vtkTransform.New();
                        markerTransform.Translate(rOffset.get(0), rOffset.get(1), rOffset.get(2));
                        markerTransform.PreMultiply();
                        markerTransform.SetInput(OsimControlPointProperty.parentBodyProp.assembly.GetUserTransform());


                        OsimControlPointProperty.controlPointActor.SetUserTransform(markerTransform);
                        OsimControlPointProperty.controlPointTransform = markerTransform;

                    }
                    osimForceProperty.UpdateMuscleLineActorTransform();
                }
            }

            #endregion
        }

        public void PrintBodiesTo2DRenderer(vtkRenderer ImageRen1, vtkRenderer ImageRen2)
        {
            foreach(OsimBodyProperty bodyProp in _bodyPropertyList)
            {
              
                Vec3 scaleFactors = new Vec3();
                bodyProp.body.getScaleFactors(scaleFactors);

                #region Making an actor for each geometry and adding it to an Assembly

                foreach (OsimGeometryProperty geomProp in bodyProp._OsimGeometryPropertyList)
                {
                    geomProp.Make2Dactors();
                    geomProp._vtkActor1.GetProperty().SetOpacity(0.0);
                    geomProp._vtkActor1.SetUserTransform(geomProp.internalTransformVTK);
                    bodyProp.assemblyOpace1.AddPart(geomProp._vtkActor1);
                }
                #endregion
                bodyProp.assemblyOpace1.SetScale(scaleFactors.get(0), scaleFactors.get(1), scaleFactors.get(2));
                bodyProp.assemblyOpace1.SetUserTransform(bodyProp.transform);
                ImageRen1.AddActor(bodyProp.assemblyOpace1);
                ImageRen2.AddActor(bodyProp.assemblyOpace1);
               
            }
            printedTo2Drenderer = true;
        }

        public void PrintMarkersTo2DRenderer(vtkRenderer ren1, vtkRenderer ren2)
        {
            for (int i = 0; i < _markerPropertyList.Count; i++)
            {
                OsimMakerProperty markerProp = _markerPropertyList[i];

                OsimBodyProperty bodyProp = getSpecifiedBodyPropertyFromName(markerProp.referenceBody); // getSpecifiedBodyProperty(body);
                vtkAssembly assembly = bodyProp.assembly;


                #region Making an actor marker 

                vtkSphereSource sphere = new vtkSphereSource();
                sphere.SetRadius(0.0070);

                vtkPolyDataMapper sphereMapper = vtkPolyDataMapper.New();
                sphereMapper.SetInputConnection(sphere.GetOutputPort());
                sphereMapper.SetProgressText(markerProp.objectName);  //This is used as ID.
                OsimBodyProperty parentBodyProp = getSpecifiedBodyProperty(markerProp.marker.getBody());

                vtkActor sphereActor = vtkActor.New();
                sphereActor.SetMapper(sphereMapper);

     
                vtkTransform markerTransform = vtkTransform.New();
                markerTransform.Translate(markerProp.rOffset.get(0), markerProp.rOffset.get(1), markerProp.rOffset.get(2));
                markerTransform.PreMultiply();
                markerTransform.SetInput(parentBodyProp.transform);


                sphereActor.SetUserTransform(markerTransform);
                markerProp._opaceAssembly.AddPart(sphereActor);

                #endregion
                //markerProp._opaceAssembly.SetUserTransform(sphereActor.GetUserTransform());
            
                ren1.AddActor(markerProp._opaceAssembly);
                ren2.AddActor(markerProp._opaceAssembly);
            }
            //ren1.Render();
            //ren2.Render();
            ren1.GetRenderWindow().Render();
            ren2.GetRenderWindow().Render();
        }

        public void updateAllConeBeam()
        {

            foreach (OsimBodyProperty Bprop in bodyPropertyList)
            {
                ConeBeamCorrectBody(Bprop);
                UpdateOpaceAssembly(Bprop);
            }
        }
        public void ConeBeamCorrectBody(OsimBodyProperty Bprop)
        {
            if(!printedTo2Drenderer)
            { return; }
            foreach (OsimGeometryProperty geomProp in Bprop._OsimGeometryPropertyList)
            {

                vtkPolyData polyData = new vtkPolyData();
                polyData.DeepCopy(geomProp._vtkPolyData);


                #region DEFORM THE GEOMETRY ToDo: take the (re-)scaling into account! 
                vtkTransformPolyDataFilter filter = new vtkTransformPolyDataFilter();

                //Tranform the object to the absolute reference system:
                filter.SetInput(polyData);
                filter.SetTransform(Bprop.transform);
                filter.Update();


                vtkPolyData VtkPolyDataAbsoluteRefS = new vtkPolyData();
                VtkPolyDataAbsoluteRefS = filter.GetOutput();
                vtkPoints vtkPoints = VtkPolyDataAbsoluteRefS.GetPoints();

                //iterate over all points in the polydata file.
                for (long i = 0; i < vtkPoints.GetNumberOfPoints(); i++)
                {
                    double[] pointVec = vtkPoints.GetPoint(i);
                    double Xnew;
                    double Znew;
                    Project(pointVec[0], pointVec[2], out Xnew, out Znew);
                    vtkPoints.SetPoint(i, Xnew, pointVec[1], Znew);
                }

                VtkPolyDataAbsoluteRefS.SetPoints(vtkPoints);
                vtkTransformPolyDataFilter inverseFilter = new vtkTransformPolyDataFilter();
                inverseFilter.SetInput(VtkPolyDataAbsoluteRefS);
                inverseFilter.SetTransform(Bprop.transform.GetInverse());
                inverseFilter.Update();

                VtkPolyDataAbsoluteRefS = inverseFilter.GetOutput();
                #endregion


                geomProp._vtkPolyDataDLT = VtkPolyDataAbsoluteRefS;

                geomProp.DLTpolydataHasBeenMade = true;

            }


        }
        /// <summary>
        /// Projection Method(find locations of projections)
        /// DSTI are fixed, xR and zR should be given as input variables.
        /// </ summary >
        /// < param name = "xR" > Real coordinate x(the true coordinate in the 3D EOS space) </ param >
        /// < param name = "zR" > Real coordinate z(the true coordinate in the 3D EOS space) </ param >
        /// < param name = "xP" > Calculated Projected coordinate x on the image </ param >
        /// < param name = "zP" > Calculated Projected coordinate z on the image </ param >

        public void Project(double xR, double zR, out double xP, out double zP)
        {

            xP = (xR / (EosImage1.DistanceSourceToIsocenter + zR)) * EosImage1.DistanceSourceToIsocenter;

            zP = (zR / (EosImage2.DistanceSourceToIsocenter + xR)) * EosImage2.DistanceSourceToIsocenter;

        }



        /// <summary>
        /// Inverse Projection Method(find the true location)
        /// DSTI are fixed, xP and zP should be given as input variables.
        /// </ summary >
        /// < param name = "xP" > Projection coordinate x </ param >
        /// < param name = "zP" > Projection coordinate z </ param >
        /// < param name = "xR" > Calculated Real coordinate x </ param >
        /// < param name = "zR" > Calculated Real coordinate z </ param >

        public void InverseProject(double xP, double zP, out double xR, out double zR)

        {

            //xR = (xP * (EosImage1.DistanceSourceToIsocenter + zP) * EosImage2.DistanceSourceToIsocenter) / ((EosImage1.DistanceSourceToIsocenter * EosImage2.DistanceSourceToIsocenter) - (zP * xP));


            //zR = (zP * (EosImage2.DistanceSourceToIsocenter + xP) * EosImage1.DistanceSourceToIsocenter) / ((EosImage1.DistanceSourceToIsocenter * EosImage2.DistanceSourceToIsocenter) - (zP * xP));



            //xR = ((zP * xP * EosImage2.DistanceSourceToIsocenter * EosImage2.DistanceSourceToIsocenter) + (xP * EosImage2.DistanceSourceToIsocenter * EosImage1.DistanceSourceToIsocenter)) / ((EosImage2.DistanceSourceToIsocenter * EosImage1.DistanceSourceToIsocenter) + (xP * zP));

            //zR = (EosImage1.DistanceSourceToIsocenter / xP) * xR - EosImage1.DistanceSourceToIsocenter;

            double slopeL;

            if (xP == 0)
            {
                slopeL = 1000000000000;
            }
            else
            {
                slopeL = (0 - (-EosImage1.DistanceSourceToIsocenter)) / (xP - 0);
            }

            double slopeF = ((-zP - 0) / (0 - EosImage2.DistanceSourceToIsocenter));

            xR = ((-slopeF * EosImage2.DistanceSourceToIsocenter) - (-EosImage1.DistanceSourceToIsocenter)) / (slopeL - slopeF);

            zR = slopeL * xR + (-EosImage1.DistanceSourceToIsocenter);


        }

        public void Update2DRenderer(vtkRenderer ren1, vtkRenderer ren2)
        {
            foreach (OsimBodyProperty bodyProp in _bodyPropertyList)
            {
                bodyProp.assemblyOpace1.SetUserTransform(bodyProp.transform);
               // bodyProp.assemblyOpace2.SetUserTransform(bodyProp.transform);
            }
        }

        public void UpdateOpaceAssembly(OsimBodyProperty Bprop)
        {
            foreach (OsimGeometryProperty geomProp in Bprop._OsimGeometryPropertyList)
            {
                geomProp.Make2Dactors();
            }

            Bprop.assemblyOpace1.SetUserTransform(Bprop.transform);
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

        public vtkTransform getRelativeVTKTransform2(vtkTransform childTransform, vtkTransform parentTransform)
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

        public double markerradius = 0.0070;

        public void InitializeMarkersinRenderer(vtkRenderer renderer)
        {
            foreach( OsimMakerProperty markerProp in _markerPropertyList)
            {
                vtkSphereSource sphere = new vtkSphereSource();
                sphere.SetRadius(markerradius);

                vtkPolyDataMapper sphereMapper = vtkPolyDataMapper.New();
                sphereMapper.SetInputConnection(sphere.GetOutputPort());
                sphereMapper.SetProgressText(markerProp.objectName);  //This is used as ID.
                OsimBodyProperty relatedBodyProp = getSpecifiedBodyProperty(markerProp.marker.getBody());

                vtkActor sphereActor = new vtkActor();
                markerProp.markerActor = sphereActor;
                markerProp.markerActor.SetMapper(sphereMapper);

                markerProp.markerTransform.Translate(markerProp.rOffset.get(0), markerProp.rOffset.get(1), markerProp.rOffset.get(2));
                markerProp.markerTransform.PreMultiply();
                markerProp.markerTransform.SetInput(relatedBodyProp.transform);

                Position pos = new Position((float)markerProp.markerTransform.GetPosition()[0], (float)markerProp.markerTransform.GetPosition()[1], (float)markerProp.markerTransform.GetPosition()[2]);
                markerProp.absPosition = pos;
                markerProp.markerActor.SetUserTransform(markerProp.markerTransform);

                markerProp.markerActor.GetProperty().SetColor(markerProp.colorR, markerProp.colorG, markerProp.colorB);
                renderer.AddActor(markerProp.markerActor);
            }
        }

        public void AddPathpoint(OsimForceProperty forceprop, int currentindex, string CPName, Body body, Vec3 postionOnBody)
        {
            Muscle muscle = forceprop._muscle;
            //muscle.getGeometryPath().appendNewPathPoint((CPName, body, postionOnBody);
            muscle.getGeometryPath().addPathPoint(si, currentindex +1 , body);
            muscle.updateDisplayer(si);
            _osimModel.updBodySet();
            _osimModel.updateDisplayer(_si);
            muscle.getGeometryPath().getPathPointSet().get(currentindex + 1).setName(CPName);
            muscle.getGeometryPath().getPathPointSet().get(currentindex + 1).setLocation(si, postionOnBody);
            muscle.updateDisplayer(si);
            _osimModel.updBodySet();
            _osimModel.updateDisplayer(_si);

        }

        #endregion

        #region Utilities
        public double DegreeToRadian(double angle)
        {
            return Math.PI * angle / 180.0;
        }

        public double RadianToDegree(double angle)
        {
            return angle * (180.0 / Math.PI);
        }
        
        public vtkTransform ConvertTransformFromSim2VTK(Transform simTransform)
        {
            Vec3 transl = simTransform.T();
            Rotation rot = simTransform.R();
            Vec3 rotvect = rot.convertRotationToBodyFixedXYZ();
            
            vtkTransform vtktransf = vtkTransform.New();
            vtktransf.Translate(transl.get(0), transl.get(1), transl.get(2));

            vtktransf.PreMultiply();


            vtktransf.RotateX(RadianToDegree(rotvect.get(0)));
            vtktransf.RotateY(RadianToDegree(rotvect.get(1)));
            vtktransf.RotateZ(RadianToDegree(rotvect.get(2)));


            return vtktransf;
        }
        #endregion

        #region TreeView Methods
        public void Model2Treeview(TreeView treeView1)
        {
            ClearTreeview(treeView1);
            TV_SetRootNodes(treeView1);
            TV_SetBodyNodes(treeView1);
            TV_SetMarkerNodes(treeView1);

            if (printMuscles)
            {
                TV_SetForceNodes(treeView1);
            }
            TV_SetCoordinateNodes(treeView1);
            treeView1.Nodes[0].Expand();
        }

        public void UpdateModelTreeview(TreeView treeView1)
        {

            //saving expanded nodes
            List<string> ExpandedNodes = new List<string>();
            ExpandedNodes = collectExpandedNodes(treeView1.Nodes);
            //resetting tree view nodes status to colapsed
            treeView1.CollapseAll();



            ClearTreeview(treeView1);
            //ClearPropertyLists();  //This is not being done in the Update Method.
            //FillPropertyLists();
            TV_SetRootNodes(treeView1);
            TV_SetBodyNodes(treeView1);
            TV_SetMarkerNodes(treeView1);
            TV_SetForceNodes(treeView1);
            treeView1.Update();


            //Restore it back
            if (ExpandedNodes.Count > 0)
            {
                TreeNode IamExpandedNode;
                for (int i = 0; i < ExpandedNodes.Count; i++)
                {
                    IamExpandedNode = FindNodeByName(treeView1.Nodes, ExpandedNodes[i]);
                    expandNodePath(IamExpandedNode);
                }

            }

            treeView1.Update();
        }

        List<string> collectExpandedNodes(TreeNodeCollection Nodes)
        {
            List<string> _lst = new List<string>();
            foreach (TreeNode checknode in Nodes)
            {
                if (checknode.IsExpanded)
                    _lst.Add(checknode.Name);
                if (checknode.Nodes.Count > 0)
                    _lst.AddRange(collectExpandedNodes(checknode.Nodes));
            }
            return _lst;
        }

        TreeNode FindNodeByName(TreeNodeCollection NodesCollection, string Name)
        {
            TreeNode returnNode = null; // Default value to return
            foreach (TreeNode checkNode in NodesCollection)
            {
                if (checkNode.Name == Name)  //checks if this node name is correct
                    returnNode = checkNode;
                else if (checkNode.Nodes.Count > 0) //node has child
                {
                    returnNode = FindNodeByName(checkNode.Nodes, Name);
                }

                if (returnNode != null) //check if founded do not continue and break
                {
                    return returnNode;
                }

            }
            //not found
            return returnNode;
        }

        void expandNodePath(TreeNode node)
        {
            if (node == null)
                return;
            if (node.Level != 0) //check if it is not root
            {
                node.Expand();
                expandNodePath(node.Parent);
            }
            else
            {
                node.Expand(); // this is root 
            }

        }


        private void ClearTreeview(TreeView treeView1)
        {
            treeView1.Nodes.Clear();
        }
        private void TV_SetRootNodes(TreeView treeView1)
        {
            treeView1.Nodes.Add(_osimModel.getName(), _osimModel.getName());
            treeView1.Nodes[_osimModel.getName()].Expand();
            treeView1.Nodes[_osimModel.getName()].Nodes.Add("Bodies", "Bodies");
            treeView1.Nodes[_osimModel.getName()].Nodes.Add("Joints", "Joints");
            treeView1.Nodes[_osimModel.getName()].Nodes.Add("Forces", "Forces");
            treeView1.Nodes[_osimModel.getName()].Nodes.Add("Markers", "Markers");
            treeView1.Nodes[_osimModel.getName()].Nodes.Add("Coordinates", "Coordinates");
        }
        private void TV_SetBodyNodes(TreeView treeView1)
        {
            //Loading the bodies that are contained within a group.
            ArrayStr rGroupNames = new ArrayStr();
            _osimModel.getBodySet().getGroupNames(rGroupNames);
            int nrbodygroups = rGroupNames.getSize();

            for (int i = 0; i < nrbodygroups; i++)
            {
                OsimGroupElement osimGroupElementBody = new OsimGroupElement();
                string groupname = rGroupNames.get(i);
                osimGroupElementBody.groupName = groupname;
                osimGroupElementList.Add(osimGroupElementBody);

                treeView1.Nodes[0].Nodes["Bodies"].Nodes.Add(groupname, groupname);

                int groupsize = _osimModel.getBodySet().getGroup(groupname).getMembers().getSize();

                for (int j = 0; j < groupsize; j++)
                {
                    OpenSimObject objectgre = _osimModel.getBodySet().getGroup(groupname).getMembers().get(j);
                    string objectname = objectgre.getName();
                    TreeNode TreeNd = treeView1.Nodes[0].Nodes["Bodies"].Nodes[groupname].Nodes.Add(objectname, objectname);
                    TreeNd.Tag = getSpecifiedBodyPropertyFromName(objectname);
                    osimGroupElementBody.osimBodyPropertyList.Add(getSpecifiedBodyPropertyFromName(objectname));



                }
            }

            //Add a Node with ALL bodies
            treeView1.Nodes[0].Nodes["Bodies"].Nodes.Add("All", "All");
            int allbodies = _osimModel.getBodySet().getSize();

            OsimGroupElement osimGroupElement = new OsimGroupElement();
            osimGroupElement.groupName = "All";
            osimGroupElementList.Add(osimGroupElement);

            for (int j = 0; j < allbodies; j++)
            {
                string objectname = _osimModel.getBodySet().get(j).getName();
               TreeNode TreeNd =  treeView1.Nodes[0].Nodes["Bodies"].Nodes["All"].Nodes.Add(objectname, objectname);
                OsimBodyProperty temposimbodyprop = getSpecifiedBodyPropertyFromName(objectname);
                TreeNd.Tag = temposimbodyprop;
                osimGroupElement.osimBodyPropertyList.Add(temposimbodyprop);

                //ADDING A JOINT FOR EACH BODY
                string jointName = temposimbodyprop.osimJointProperty.objectName;
                TreeNode DN=  treeView1.Nodes[0].Nodes["Joints"].Nodes.Add(jointName, jointName);
                DN.Tag = temposimbodyprop.osimJointProperty;
            }

        }
        private void TV_SetMarkerNodes(TreeView treeView1)
        {
            //Add all the markers to the same Node ('All'-node)
            treeView1.Nodes[0].Nodes["Markers"].Nodes.Add("All", "All");
            int allmarkers = osimModel.getMarkerSet().getSize();

            for (int j = 0; j < allmarkers; j++)
            {
                string objectname = osimModel.getMarkerSet().get(j).getName();
                treeView1.Nodes[0].Nodes["Markers"].Nodes["All"].Nodes.Add(objectname, objectname);
            }

        }
        private void TV_SetForceNodes(TreeView treeView1)
        {
            //Loading the Forces that are contained within a group.
            ArrayStr rGroupNames = new ArrayStr();
            _osimModel.getForceSet().getGroupNames(rGroupNames);
            int nrforcegroups = _osimModel.getForceSet().getNumGroups();


            //Add a Group with ALL Forces
            treeView1.Nodes[0].Nodes["Forces"].Nodes.Add("All", "All");
            int allForces = _osimModel.getForceSet().getSize();

            OsimGroupElement osimGroupElementAllForce = new OsimGroupElement();
            osimGroupElementAllForce.groupName = "All";
            _osimGroupElementListForces.Add(osimGroupElementAllForce);

            for (int j = 0; j < allForces; j++)
            {
                string objectname = _osimModel.getForceSet().get(j).getName();
                treeView1.Nodes[0].Nodes["Forces"].Nodes["All"].Nodes.Add(objectname, objectname);
                osimGroupElementAllForce.osimForcePropertyList.Add(getSpecifiedForcePropertyFromName(objectname));
            }


            for (int i = 0; i < nrforcegroups; i++)
            {
                OsimGroupElement osimGroupElementForce = new OsimGroupElement();
                string groupname = rGroupNames.get(i);
                osimGroupElementForce.groupName = groupname;
                _osimGroupElementListForces.Add(osimGroupElementForce);

                treeView1.Nodes[0].Nodes["Forces"].Nodes.Add(groupname, groupname);

                int groupsize = _osimModel.getForceSet().getGroup(groupname).getMembers().getSize();

                for (int j = 0; j < groupsize; j++)
                {
                    OpenSimObject objectgre = _osimModel.getForceSet().getGroup(groupname).getMembers().get(j);
                    string objectname = objectgre.getName();
                    treeView1.Nodes[0].Nodes["Forces"].Nodes[groupname].Nodes.Add(objectname, objectname);
                    OsimForceProperty forceprop = getSpecifiedForcePropertyFromName(objectname);
                    osimGroupElementForce.osimForcePropertyList.Add(forceprop);
                    forceprop.groupname = groupname;
                }
            }
        }
        private void TV_SetCoordinateNodes(TreeView treeView1)
        {
            //Add all the coords to the same Node ('All'-node)
            treeView1.Nodes[0].Nodes["Coordinates"].Nodes.Add("All", "All");

            int allCoor = osimModel.getCoordinateSet().getSize();
            for (int j = 0; j < allCoor; j++)
            {
                string objectname = osimModel.getCoordinateSet().get(j).getName();
                treeView1.Nodes[0].Nodes["Coordinates"].Nodes["All"].Nodes.Add(objectname, objectname);
             
            }
        }


        #endregion

        #region Property Methods
        public void ClearPropertyLists()
        {
            _markerPropertyList.Clear();
            _bodyPropertyList.Clear();
            _forcePropertyList.Clear();
            _coordinatePropertyList.Clear();

        }
        public void FillPropertyLists()
        {
            

            //Filling the property list for the Bodies
            BodySet bodySet = _osimModel.getBodySet();

            for (int j = 0; j < bodySet.getSize(); j++)
            {
                OsimBodyProperty osimPropB = new OsimBodyProperty();
                if (j == 0)
                {
                    osimPropB.isGround = true;
                }
                //PROBLEM HERE!!!!
                osimPropB.renderer = renderer;
                osimPropB.ReadBodyProperties(bodySet.get(j));

                //osimPropB.RenderWindowImage1 = RenderWindowImage1;
                //osimPropB.RenderWindowImage2 = RenderWindowImage2;
                _bodyPropertyList.Add(osimPropB);
                if (osimPropB.body.hasJoint())
                {
                    _jointPropertyList.Add(osimPropB.osimJointProperty);
                    foreach (OsimJointCoordinateProperty coor in osimPropB.osimJointProperty.osimJointCoordinatePropertyList)
                    {
                        _coordinatePropertyList.Add(coor);
                    }
                }
             }

            //Filling the property list for the markers
            MarkerSet markerSet = _osimModel.getMarkerSet();

            for (int i = 0; i < markerSet.getSize(); i++)
            {
                OsimMakerProperty osimProp = new OsimMakerProperty();
                osimProp.ReadMarkerProperties(markerSet.get(i));
                osimProp.parentbodyprop = getSpecifiedBodyProperty(osimProp.marker.getBody());
                _markerPropertyList.Add(osimProp);
            }


            //Filling the property list for the Joints
            JointSet jointSet = _osimModel.getJointSet();

            for (int i = 0; i < jointSet.getSize(); i++)
            {
                OsimJointProperty osimProp = new OsimJointProperty();
                osimProp.joint = jointSet.get(i);

                osimProp.ReadJoint();

            }



            if (printMuscles)
            {
            //Filling the property list for the Muscles
            ForceSet forceSet = _osimModel.getForceSet();
            int allforces = forceSet.getSize();

                for (int j = 0; j < allforces; j++)
                {
                    OsimForceProperty osimPropB = new OsimForceProperty();
                    osimPropB.osimModel = osimModel;
                    osimPropB.SimModelVisualization = this;
                    osimPropB.forceSetIndex = j;
                    osimPropB.ren1 = renderer;
                    osimPropB.ReadForceProperties(forceSet.get(j));
                    _forcePropertyList.Add(osimPropB);
                }
            }
        }
        #endregion
       
        #endregion

    }
}
