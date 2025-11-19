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

namespace SpineAnalyzer.ModelVisualization
{
    public partial class frmAttachMarkerToModel : Form
    {
        #region Declarations
        public AppData AppData;
        public string defaultName;
        public Position absPosition;
        public Body referenceBody;
        private OsimMakerProperty _newOsimMarkerProperty;
        public List<string> markerNameList = new List<string>();
        public List<string> bodyNameList = new List<string>();
        public Model osimModel;
        public SimModelVisualization simModelVisualization;
        #endregion

        #region Properties
        public OsimMakerProperty newOsimMarkerProperty
        {
            get { return _newOsimMarkerProperty; }
            set { _newOsimMarkerProperty = value; }
        }
        #endregion

        public frmAttachMarkerToModel()
        {
            InitializeComponent();
        }

        private void frmAttachMarkerToModel_Load(object sender, EventArgs e)
        {

            if (osimModel == null)
            {
                MessageBox.Show("You have to load a model first.", "No model loaded!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            FillMarkerNameList();
            FillBodyNameList();
           

            PopulateComboBox(cBbodies, bodyNameList);
            this.AcceptButton = btnConfirm;
            cBvisible.SelectedIndex = 0;
            txtMarkerName.Text = defaultName;   //This is the name of the measurement.
            cBbodies.SelectedIndex = SuggestReferenceBody(defaultName);
            cBfixed.SelectedIndex = 1;

        }

        private int SuggestReferenceBody(string defaultName)
        {
            int listIndex = 0;
            if ( defaultName == "C7")
            { listIndex = bodyNameList.FindIndex(x => x == "cerv7"); }

            if (defaultName == "L2M" || defaultName == "L2L" || defaultName == "L2R" || defaultName == "L2" || defaultName == "L2Mb" || defaultName == "L2Lb" || defaultName == "L2Rb")
            { listIndex = bodyNameList.FindIndex(x => x == "lumbar2"); }

            if (defaultName == "SACR")
            { listIndex = bodyNameList.FindIndex(x => x == "sacrum"); }

            if (defaultName == "L4M" || defaultName == "L4L" || defaultName == "L4R" || defaultName == "L4" || defaultName == "L4Mb" || defaultName == "L4Lb" || defaultName == "L4Rb")
            { listIndex = bodyNameList.FindIndex(x => x == "lumbar4"); }

            if (defaultName == "LPSI" || defaultName == "LASI" || defaultName == "RASI" || defaultName == "RPSI" || defaultName == "LPSIb" || defaultName == "LASIb" || defaultName == "RASIb" || defaultName == "RPSIb")
            { listIndex = bodyNameList.FindIndex(x => x == "pelvis"); }

            if (defaultName == "STRN" || defaultName == "STER")
            { listIndex = bodyNameList.FindIndex(x => x == "sternum"); }

            if (defaultName == "LFHD" || defaultName == "RFHD" || defaultName == "LBHD" || defaultName == "RBHD")
            { listIndex = bodyNameList.FindIndex(x => x == "skull"); }

            if (defaultName == "LKNE" || defaultName == "LKNM" || defaultName == "LTH1" || defaultName == "LTHI" || defaultName == "LTH3" || defaultName == "LTH2")
            { listIndex = bodyNameList.FindIndex(x => x == "femur_l"); }

            if (defaultName == "RKNE" || defaultName == "RKNM" || defaultName == "RTH1" || defaultName == "RTHI" || defaultName == "RTH3" || defaultName == "RTH2")
            { listIndex = bodyNameList.FindIndex(x => x == "femur_r"); }

            if (defaultName == "LANK" || defaultName == "LANM" || defaultName == "LTI1" || defaultName == "LTIB" || defaultName == "LTI3" || defaultName == "LTI2")
            { listIndex = bodyNameList.FindIndex(x => x == "tibia_l"); }

            if (defaultName == "RANK" || defaultName == "RANM" || defaultName == "RTI1" || defaultName == "RTIB" || defaultName == "RTI3" || defaultName == "RTI2")
            { listIndex = bodyNameList.FindIndex(x => x == "tibia_r"); }

            if (defaultName == "LTOE")
            { listIndex = bodyNameList.FindIndex(x => x == "toes_l"); }    //Not sure, might be calc as well...

            if (defaultName == "RTOE")
            { listIndex = bodyNameList.FindIndex(x => x == "toes_r"); }     //Not sure, might be calc as well...

            if (defaultName == "LHEE")
            { listIndex = bodyNameList.FindIndex(x => x == "calcn_l"); }

            if (defaultName == "RHEE")
            { listIndex = bodyNameList.FindIndex(x => x == "calcn_r"); }

            if (defaultName == "LMT5")
            { listIndex = bodyNameList.FindIndex(x => x == "calcn_l"); }

            if (defaultName == "RMT5")
            { listIndex = bodyNameList.FindIndex(x => x == "calcn_r"); }

            if (defaultName == "LSHO")
            { listIndex = bodyNameList.FindIndex(x => x == "scapula_L"); }

            if (defaultName == "RSHO")
            { listIndex = bodyNameList.FindIndex(x => x == "scapula_R"); }

            if (defaultName == "LELB" || defaultName == "LELM")
            { listIndex = bodyNameList.FindIndex(x => x == "humerus_L"); }

            if (defaultName == "RELB" || defaultName == "RELM")
            { listIndex = bodyNameList.FindIndex(x => x == "humerus_R"); }

            if (defaultName == "LWRA")
            { listIndex = bodyNameList.FindIndex(x => x == "radius_L"); }

            if (defaultName == "LWRB")
            { listIndex = bodyNameList.FindIndex(x => x == "ulna_L"); }

            if (defaultName == "RWRA")
            { listIndex = bodyNameList.FindIndex(x => x == "radius_R"); }

            if (defaultName == "RWRB")
            { listIndex = bodyNameList.FindIndex(x => x == "ulna_R"); }

            if (defaultName == "RFIN")
            { listIndex = bodyNameList.FindIndex(x => x == "hand_R"); }

            if (defaultName == "LFIN")
            { listIndex = bodyNameList.FindIndex(x => x == "hand_L"); }

            if (defaultName == "CLAV")
            { listIndex = bodyNameList.FindIndex(x => x == "sternum"); }

            if (defaultName == "RBAK")
            { listIndex = bodyNameList.FindIndex(x => x == "scapula_R"); }

            if (defaultName == "T11R" || defaultName == "T11L" || defaultName == "T11M" || defaultName == "T11" || defaultName == "T11Rb" || defaultName == "T11Lb" || defaultName == "T11Mb")
            { listIndex = bodyNameList.FindIndex(x => x == "thoracic11"); }

            if (defaultName == "T7M" || defaultName == "T7R" || defaultName == "T7L" || defaultName == "T7")
            { listIndex = bodyNameList.FindIndex(x => x == "thoracic7"); }

            if (defaultName == "T8M" || defaultName == "T8R" || defaultName == "T8L" || defaultName == "T8")
            { listIndex = bodyNameList.FindIndex(x => x == "thoracic8"); }

            if (defaultName == "T3M" || defaultName == "T3R" || defaultName == "T3L" || defaultName == "T3")
            { listIndex = bodyNameList.FindIndex(x => x == "thoracic3"); }

            if (defaultName == "T1M" || defaultName == "T1R" || defaultName == "T1L" || defaultName == "T1")
            { listIndex = bodyNameList.FindIndex(x => x == "thoracic1"); }

            if (defaultName == "T2M" || defaultName == "T2R" || defaultName == "T2L" || defaultName == "T2")
            { listIndex = bodyNameList.FindIndex(x => x == "thoracic2"); }

            if (defaultName == "T4M" || defaultName == "T4R" || defaultName == "T4L" || defaultName == "T4")
            { listIndex = bodyNameList.FindIndex(x => x == "thoracic4"); }

            if (defaultName == "T5M" || defaultName == "T5R" || defaultName == "T5L" || defaultName == "T5")
            { listIndex = bodyNameList.FindIndex(x => x == "thoracic5"); }

            if (defaultName == "T6M" || defaultName == "T6R" || defaultName == "T6L" || defaultName == "T6")
            { listIndex = bodyNameList.FindIndex(x => x == "thoracic6"); }

            if (defaultName == "T9M" || defaultName == "T9R" || defaultName == "T9L" || defaultName == "T9")
            { listIndex = bodyNameList.FindIndex(x => x == "thoracic9"); }

            if (defaultName == "T10M" || defaultName == "T10R" || defaultName == "T10L" || defaultName == "T10")
            { listIndex = bodyNameList.FindIndex(x => x == "thoracic10"); }

            if (defaultName == "L3M" || defaultName == "L3R" || defaultName == "L3L" || defaultName == "L3" || defaultName == "L3Mb" || defaultName == "L3Rb" || defaultName == "L3Lb" || defaultName == "L3b")
            { listIndex = bodyNameList.FindIndex(x => x == "lumbar3"); }

            if (defaultName == "L5M" || defaultName == "L5R" || defaultName == "L5L" || defaultName == "L5"  || defaultName == "L5Mb" || defaultName == "L5Rb" || defaultName == "L5Lb" || defaultName == "L5b")
            { listIndex = bodyNameList.FindIndex(x => x == "lumbar5"); }

            if (defaultName == "L1M" || defaultName == "L1R" || defaultName == "L1L" || defaultName == "L1" || defaultName == "L1Mb" || defaultName == "L1Rb" || defaultName == "L1Lb" || defaultName == "L1b")
            { listIndex = bodyNameList.FindIndex(x => x == "lumbar1"); }

            if (defaultName == "T12M" || defaultName == "T12R" || defaultName == "T12L" || defaultName == "T12")
            { listIndex = bodyNameList.FindIndex(x => x == "thoracic12"); }

            if (listIndex == -1)
            { listIndex = 0; }

            return listIndex;
        }

        private void FillMarkerNameList()
        {
            if(osimModel == null)
            {
                MessageBox.Show("You have to load a model first.", "No model loaded!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            MarkerSet markerset = osimModel.getMarkerSet();
            int allmarkers = markerset.getSize();

            for (int j = 0; j < allmarkers; j++)
            {
                markerNameList.Add(markerset.get(j).getName());
            }
        }

        private void FillBodyNameList()
        {
            BodySet BodySet = osimModel.getBodySet();
            int allbodies = BodySet.getSize();

            for (int j = 0; j < allbodies; j++)
            {
                bodyNameList.Add(BodySet.get(j).getName());
            }

        }

        private void PopulateComboBox(ComboBox combobox, List<string> stringList)
        {
            foreach (string bodyName in stringList)
            {
                combobox.Items.Add(bodyName);
            }

        }

        private bool CheckMarkerNameInList(string defaultName)
        {
            return markerNameList.Contains(defaultName);

        }

        private void MakeVTKobjects()
        {
            //The sphere for the marker
            vtkSphereSource vtkSphereSource = new vtkSphereSource();
            vtkSphereSource.SetRadius(0.0070);
            vtkSphereSource.SetPhiResolution(20);
            vtkSphereSource.SetThetaResolution(20);

            vtkPolyDataMapper vtkPolyDataMapperSphere = vtkPolyDataMapper.New();
            vtkPolyDataMapperSphere.SetInputConnection(vtkSphereSource.GetOutputPort());

            vtkActor vtkActor = new vtkActor();
            //GC.KeepAlive(vtkActor);
            vtkActor.SetMapper(vtkPolyDataMapperSphere);
            vtkActor.GetProperty().SetColor(0, 0, 1);
            //vtkActor.SetPosition(absPosition.X, absPosition.Y, absPosition.Z);


            _newOsimMarkerProperty.markerActor = vtkActor;


            Vec3 rOffset = _newOsimMarkerProperty.marker.getOffset();


            OsimBodyProperty parentBodyProp = simModelVisualization.getSpecifiedBodyProperty(_newOsimMarkerProperty.marker.getBody());

            vtkTransform markerTransform = vtkTransform.New();
            markerTransform.Translate(rOffset.get(0), rOffset.get(1), rOffset.get(2));
            markerTransform.PreMultiply();
            markerTransform.SetInput(parentBodyProp.transform);

            _newOsimMarkerProperty.markerActor.SetUserTransform(markerTransform);



            _newOsimMarkerProperty.colorR = 0;
            _newOsimMarkerProperty.colorG = 0;
            _newOsimMarkerProperty.colorB = 1;

            ////The Label for the marker
            //vtkVectorText vtkVectorText = new vtkVectorText();
            //vtkVectorText.SetText(_newOsimMarkerProperty.objectName);

            //vtkPolyDataMapper vtkPolyDataMapperLabel = vtkPolyDataMapper.New();
            //vtkPolyDataMapperLabel.SetInputConnection(vtkVectorText.GetOutputPort());

            //vtkFollower vtkFollower = new vtkFollower();
            //vtkFollower.SetMapper(vtkPolyDataMapperLabel);
            //vtkFollower.SetScale(0.015, 0.015, 0.015);
            ////vtkFollower.SetCamera(ren1.GetActiveCamera());
            //vtkFollower.GetProperty().SetColor(0.4, 0.4, 1);
            //vtkFollower.SetPosition(absPosition.X + 0.01, absPosition.Y + 0.01, absPosition.Z + 0.01);

        }

        private void AddMarkerToGround(string name, double rOffsetX, double rOffsetY, double rOffsetZ)
        {
            double[] rOffset = new double[3];
            rOffset.SetValue(rOffsetX, 0);
            rOffset.SetValue(rOffsetY, 1);
            rOffset.SetValue(rOffsetZ, 2);
           
            osimModel.getMarkerSet().addMarker(name, rOffset, osimModel.getGroundBody());
            osimModel.updMarkerSet();
           
        }

        private void ChangeMarkerReferenceBody(string name, Body referenceBody)
        {
            
            osimModel.getMarkerSet().get(name).changeBodyPreserveLocation(osimModel.initSystem(), referenceBody);

        }

        private void btnConfirm_Click(object sender, EventArgs e)
        {
            ConfirmAttachement();
        }

        public void ConfirmAttachement()
        {
            if (this.ValidateChildren(ValidationConstraints.Enabled))
            {
                btnConfirm.Enabled = false;
                _newOsimMarkerProperty.isFixed = (cBfixed.Text == "True");
                _newOsimMarkerProperty.isVisible = (cBvisible.Text == "True");
                _newOsimMarkerProperty.absPosition = absPosition;
                _newOsimMarkerProperty.objectName = txtMarkerName.Text;
                _newOsimMarkerProperty.referenceBody = cBbodies.Text;
                _newOsimMarkerProperty.referenceBodyObject = osimModel.getBodySet().get(cBbodies.Text);

                AddMarkerToGround(_newOsimMarkerProperty.objectName, absPosition.X, absPosition.Y, absPosition.Z);
                ChangeMarkerReferenceBody(_newOsimMarkerProperty.objectName, _newOsimMarkerProperty.referenceBodyObject);
                osimModel.updMarkerSet();

                _newOsimMarkerProperty.marker = osimModel.getMarkerSet().get(txtMarkerName.Text);
                _newOsimMarkerProperty.marker.getOffset(_newOsimMarkerProperty.rOffset);
                MakeVTKobjects();
                _newOsimMarkerProperty.objectType = _newOsimMarkerProperty.marker.GetType().ToString();
                _newOsimMarkerProperty.SetContextMenuStrip();
                this.Close();
            }

        }

        public void MoveMarker()
        {
            //_newOsimMarkerProperty.isFixed = (cBfixed.Text == "True");
            //_newOsimMarkerProperty.isVisible = (cBvisible.Text == "True");
            _newOsimMarkerProperty.absPosition = absPosition;
            _newOsimMarkerProperty.referenceBodyObject = osimModel.getBodySet().get(_newOsimMarkerProperty.referenceBody);
            // _newOsimMarkerProperty.objectName = txtMarkerName.Text;
            //_newOsimMarkerProperty.referenceBody = cBbodies.Text;
            //_newOsimMarkerProperty.referenceBodyObject = osimModel.getBodySet().get(cBbodies.Text);
            osimModel.getMarkerSet().remove(_newOsimMarkerProperty.marker);
            osimModel.updMarkerSet();
            AddMarkerToGround(_newOsimMarkerProperty.objectName, absPosition.X, absPosition.Y, absPosition.Z);
            System.Diagnostics.Debug.WriteLine(_newOsimMarkerProperty.objectName+ " "+ absPosition.X.ToString() + "  " + absPosition.Y.ToString() + " " + absPosition.Z.ToString());
            osimModel.updMarkerSet();
            ChangeMarkerReferenceBody(_newOsimMarkerProperty.objectName, _newOsimMarkerProperty.referenceBodyObject);
            osimModel.updMarkerSet();


            _newOsimMarkerProperty.marker = osimModel.getMarkerSet().get(_newOsimMarkerProperty.objectName);
            _newOsimMarkerProperty.marker.getOffset(_newOsimMarkerProperty.rOffset);
            //_newOsimMarkerProperty.marker = osimModel.getMarkerSet().get(txtMarkerName.Text);

            //this.Close();

        }

        private void txtMarkerName_Validating(object sender, CancelEventArgs e)
        {
            errorProvider.SetError(txtMarkerName, "");
            if (string.IsNullOrWhiteSpace(txtMarkerName.Text))
            {
                errorProvider.SetError(txtMarkerName, "Markername must not be blank.");
                btnConfirm.Enabled = false;
                return;
            }
            if (CheckMarkerNameInList(txtMarkerName.Text))
            {
                errorProvider.SetError(txtMarkerName, "Markername is already in use.");
                btnConfirm.Enabled = false;
                return;
            }
            btnConfirm.Enabled = true;
        }

        private void cBbodies_Validating(object sender, CancelEventArgs e)
        {
            errorProvider.SetError(cBbodies, "");
            if (string.IsNullOrWhiteSpace(cBbodies.Text))
            {
                errorProvider.SetError(cBbodies, "Choose a reference body.");
                btnConfirm.Enabled = false;
                return;
            }
        }

        private void cBfixed_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void cBfixed_Validating(object sender, CancelEventArgs e)
        {
            errorProvider.SetError(cBfixed, "");
            if (string.IsNullOrWhiteSpace(cBfixed.Text))
            {
                errorProvider.SetError(cBfixed, "Fixed must be either set to 'True' or 'False'.");
                btnConfirm.Enabled = false;
                return;
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            _newOsimMarkerProperty = null;
            this.Close();
        }

        private void txtMarkerName_TextChanged(object sender, EventArgs e)
        {
            cBbodies.SelectedIndex = SuggestReferenceBody(txtMarkerName.Text);
        }

        private void cBbodies_SelectedIndexChanged(object sender, EventArgs e)
        {
           
        }
    }
}
