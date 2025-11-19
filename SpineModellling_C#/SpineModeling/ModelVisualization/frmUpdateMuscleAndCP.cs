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

namespace SpineAnalyzer.ModelVisualization
{
    public partial class frmUpdateMuscleAndCP : Form
    {

        public OsimForceProperty forceProperty;
        OsimControlPointProperty selectedCpProp;
        public SimModelVisualization SimModelVisualization;

        public List<string> CPNameList = new List<string>();
        public List<string> bodyNameList = new List<string>();

        public frmUpdateMuscleAndCP()
        {
            InitializeComponent();
        }

        private void frmUpdateMuscleAndCP_Load(object sender, EventArgs e)
        {
            ShowForcePropInForm();
        }

        private void ShowForcePropInForm()
        {
            txtMuscleName.Text = forceProperty.objectName;
            txtMuscleGroup.Text = forceProperty.groupname;
            dgViewMuslcePath.DataSource = forceProperty.controlPointsList;
            propertyGrid1.SelectedObject = forceProperty;

        }

        private void btnReturn_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void dgViewMuslcePath_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            propertyGrid3.SelectedObject = ReturnSelectedControlPoint();
            propertyGrid3.Update();
        }

        private OsimControlPointProperty ReturnSelectedControlPoint()
        {
            int RowCount = dgViewMuslcePath.Rows.GetRowCount(DataGridViewElementStates.Selected);
            //Check if rows selected
            if (RowCount == 0)
                return null;
            //for (int i = 0; i < RowCount; i++)
            //{
            //Get row of clicked cell
            DataGridViewRow row = dgViewMuslcePath.Rows[dgViewMuslcePath.SelectedRows[0].Index];
            //Get column AcquisitionNumber
            DataGridViewCell cell = row.Cells["objectName"];
            //create AcquisitionObject

            selectedCpProp = forceProperty.getSpecifiedControlPointPropertyFromName(cell.Value.ToString());

            //}

            return selectedCpProp;

        }



        #region Add CP

        private void btnAddMuscleCP_Click(object sender, EventArgs e)
        {
            groupBoxAddCP.Visible = true;
            FillCPNameList();
            FillBodyNameList();

            PopulateComboBox(cBbodies, bodyNameList);
            
        }
        private void txtCPName_Validating(object sender, CancelEventArgs e)
        {
            errorProvider.SetError(txtCPName, "");
            if (string.IsNullOrWhiteSpace(txtCPName.Text))
            {
                errorProvider.SetError(txtCPName, "Control Point name must not be blank.");
                btnConfirm.Enabled = false;
                return;
            }
            if (CheckCPNameInList(txtCPName.Text))
            {
                errorProvider.SetError(txtCPName, "Control Point name is already in use.");
                btnConfirm.Enabled = false;
                return;
            }
            btnConfirm.Enabled = true;
        }
        private bool CheckCPNameInList(string defaultName)
        {
            return CPNameList.Contains(defaultName);

        }

        private void txtCPName_TextChanged(object sender, EventArgs e)
        {
            errorProvider.SetError(txtCPName, "");
            if (string.IsNullOrWhiteSpace(txtCPName.Text))
            {
                errorProvider.SetError(txtCPName, "Control Point name must not be blank.");
                btnConfirm.Enabled = false;
                return;
            }
            if (CheckCPNameInList(txtCPName.Text))
            {
                errorProvider.SetError(txtCPName, "Control Point name is already in use.");
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
            } else
            {
                btnConfirm.Enabled = true;
                return;

            }
        }

        private void PopulateComboBox(ComboBox combobox, List<string> stringList)
        {
            foreach (string bodyName in stringList)
            {
                combobox.Items.Add(bodyName);
            }

        }

        private void FillCPNameList()
        {
            foreach (OsimControlPointProperty cpProp in forceProperty.controlPointsList)
            {
                CPNameList.Add(cpProp.objectName);
            }
        }

        private void FillBodyNameList()
        {
            foreach (OsimBodyProperty bprop in SimModelVisualization.bodyPropertyList)
            {
                bodyNameList.Add(bprop.objectName);
            }
        }

        public void ExecuteCPAddition()
        {
            OsimControlPointProperty pathPoint = ReturnSelectedControlPoint();
            int currentindex = pathPoint.CpNumber -1;

            forceProperty.RemoveActorsFromRen();

            Vec3 postionOnBody = new Vec3(0, 0, 0);
            postionOnBody.set(0, double.Parse(locX.Text));
            postionOnBody.set(1, double.Parse(locY.Text));
            postionOnBody.set(2, double.Parse(locZ.Text));
            Body body = SimModelVisualization.getSpecifiedBodyPropertyFromName(cBbodies.Text).body;
            SimModelVisualization.AddPathpoint(forceProperty, currentindex, txtCPName.Text, body, postionOnBody);
            ForceSet forceSet = SimModelVisualization.osimModel.getForceSet();
            forceProperty.ReadForceProperties(forceSet.get(forceProperty.forceSetIndex));
            forceProperty.CreateTheObjects();


            foreach (OsimControlPointProperty osimControlPointProperty in forceProperty.controlPointsList)
            {
                osimControlPointProperty.parentBodyProp = SimModelVisualization.getSpecifiedBodyProperty(osimControlPointProperty.pathPoint.getBody());
                osimControlPointProperty.controlPointTransform.Translate(osimControlPointProperty.rOffset.get(0), osimControlPointProperty.rOffset.get(1), osimControlPointProperty.rOffset.get(2));
                osimControlPointProperty.controlPointTransform.PreMultiply();
                osimControlPointProperty.controlPointTransform.SetInput(osimControlPointProperty.parentBodyProp.assembly.GetUserTransform());

                osimControlPointProperty.controlPointActor.SetUserTransform(osimControlPointProperty.controlPointTransform);
                forceProperty.ren1.AddActor(osimControlPointProperty.controlPointActor);
            }

            if (forceProperty.isMuscle)
            {
                forceProperty.MakeMuscleLineActors();

            }

            dgViewMuslcePath.DataSource = null;

            dgViewMuslcePath.DataSource = forceProperty.controlPointsList;
            dgViewMuslcePath.Update();
            dgViewMuslcePath.Refresh();
            

        }

        private void btnConfirm_Click(object sender, EventArgs e)
        {
            if (this.ValidateChildren(ValidationConstraints.Enabled))
            {
                btnConfirm.Enabled = false;
                ExecuteCPAddition();
                groupBoxAddCP.Visible = false;
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            groupBoxAddCP.Visible = false;
        }


        #endregion

        private void groupBoxAddCP_Enter(object sender, EventArgs e)
        {

        }

        private void btnChangeBody_Click(object sender, EventArgs e)
        {

            //forceProperty.geometryPath.getPathPointSet().get(1).changeBodyPreserveLocation(SimModelVisualization.si, body)

        }

        private void btnDown_Click(object sender, EventArgs e)
        {
            forceProperty.ChangeOrder(ReturnSelectedControlPoint());
        }

        private void btnDeleteCP_Click(object sender, EventArgs e)
        {

            if(!forceProperty.geometryPath.canDeletePathPoint(ReturnSelectedControlPoint().CpNumber - 1))
            {
                MessageBox.Show("This control point cannot be deleted.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }
            forceProperty.geometryPath.deletePathPoint(SimModelVisualization.si, ReturnSelectedControlPoint().CpNumber -1 );

            forceProperty.geometryPath.updateDisplayer(SimModelVisualization.si);
            SimModelVisualization.osimModel.updBodySet();
            SimModelVisualization.osimModel.updateDisplayer(SimModelVisualization.si);
            forceProperty.RemoveActorsFromRen();

            forceProperty.ReadForceProperties(SimModelVisualization.osimModel.getForceSet().get(forceProperty.forceSetIndex));
            forceProperty.CreateTheObjects();

            foreach (OsimControlPointProperty osimControlPointProperty in forceProperty.controlPointsList)
            {
                osimControlPointProperty.parentBodyProp = SimModelVisualization.getSpecifiedBodyProperty(osimControlPointProperty.pathPoint.getBody());
                osimControlPointProperty.controlPointTransform.Translate(osimControlPointProperty.rOffset.get(0), osimControlPointProperty.rOffset.get(1), osimControlPointProperty.rOffset.get(2));
                osimControlPointProperty.controlPointTransform.PreMultiply();
                osimControlPointProperty.controlPointTransform.SetInput(osimControlPointProperty.parentBodyProp.assembly.GetUserTransform());

                osimControlPointProperty.controlPointActor.SetUserTransform(osimControlPointProperty.controlPointTransform);
                forceProperty.ren1.AddActor(osimControlPointProperty.controlPointActor);
            }

            if (forceProperty.isMuscle)
            {
                forceProperty.MakeMuscleLineActors();

            }

            dgViewMuslcePath.DataSource = null;
            dgViewMuslcePath.DataSource = forceProperty.controlPointsList;
            dgViewMuslcePath.Update();
            dgViewMuslcePath.Refresh();

        }
    }
}
