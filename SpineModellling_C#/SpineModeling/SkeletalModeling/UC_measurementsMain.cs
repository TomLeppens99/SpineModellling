//#define VTK_MAJOR_VERSION
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using Emgu;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.UI;
using Emgu.Util;
using Kitware.VTK;
using itk.simple;
using org.dicomcs;
using org.dicomcs.data;
using org.dicomcs.dict;
using org.dicomcs.net;
using org.dicomcs.scp;
using org.dicomcs.server;
using org.dicomcs.util;
using DicomImageViewer;
using gfoidl.Imaging;
using System.Data.SqlClient;
using SpineAnalyzer.Acquisitions;

//using FitEllipse;
using SpineAnalyzer.ModelVisualization;
using OpenSim;
using System.Threading;
using System.Globalization;
using ClosedXML.Excel;
using System.Reflection;

namespace SpineAnalyzer.SkeletalModeling
{
    public partial class UC_measurementsMain : UserControl
    {

        //Thsi needs to change still

       public  _2DMeasurementsWorkpanel _2DMeasurementsWorkpanel;


        public Acquisitions.EOS EOS;
        public AppData AppData;
        public DataBase SQLDB;
      
        public Subject Subject;
        private MeasurementDetail MeasurementDetail;
        public Measurement Measurement;
        public EosImage EosImage1;
        public EosImage EosImage2;
        public EosSpace EosSpace;



        private void DoubleBufferDGV(DataGridView dgview)
        {
            Type dgvType = dgview.GetType();
            PropertyInfo pi = dgvType.GetProperty("DoubleBuffered",
                        BindingFlags.Instance | BindingFlags.NonPublic);
            pi.SetValue(dgview, true, null);
        }


        public UC_measurementsMain()
        {
            InitializeComponent();
        }

       

        private DataSet measDs;
        private DataSet measDsQueried;
        public void refreshMeasurements()
        {
            if (AppData.localStudyUser._CanOnlySeeOwnEOSmeasurements)
            {
                refreshMeasurements(AppData.globalUser._UserID);
                return;
                 }
            //Clear DataGrid
            dgViewMeasurements.DataSource = null;
            dgViewMeasurements.Refresh();

            //Build SQL Command (=SQL select statement + Parameters)
            SqlCommand SQLcmd = new SqlCommand();

            string SQLselect;

           
                SQLselect = "SELECT MeasurementID, MeasurementName, MeasurementComment, UserName FROM MeasurementHeader where AcquisitionNumber = @AcquisitionNumber";
                SQLcmd.Parameters.AddWithValue("@AcquisitionNumber", EOS.AcquisitionNumber);
          

            SQLcmd.CommandText = SQLselect;

            //Set Database
            DataBase SQLDB = new DataBase(AppData.SQLServer, AppData.SQLDatabase, AppData.SQLAuthSQL, AppData.SQLUser, AppData.SQLPassword);

            //Fill a dataset with selected records
            measDs = new DataSet();
            SQLDB.ReadDataSet(SQLcmd, ref measDs);

            // you can make the grid readonly.
            dgViewMeasurements.ReadOnly = true;
            dgViewMeasurements.DataSource = measDs.Tables[0];
            dgViewMeasurements.Columns["MeasurementID"].Visible = false;
            dgViewMeasurements.Columns["UserName"].HeaderText = "Created by";
            dgViewMeasurements.Columns["MeasurementName"].HeaderText = "Measurement name";
            // Resize the DataGridView columns to fit the newly loaded content.
            dgViewMeasurements.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.ColumnHeader);



          

            LoadTreeviewMeasurements();
        }
        public void refreshMeasurements(string username)
        {

            if(username == "All users")
            { refreshMeasurements(); }
            else
            {


            //Clear DataGrid
            dgViewMeasurements.DataSource = null;
            dgViewMeasurements.Refresh();

            //Build SQL Command (=SQL select statement + Parameters)
            SqlCommand SQLcmd = new SqlCommand();

            string SQLselect;

            SQLselect = "SELECT MeasurementID, MeasurementName, MeasurementComment, UserName FROM MeasurementHeader where AcquisitionNumber = @AcquisitionNumber AND UserName = @UserName";
            SQLcmd.Parameters.AddWithValue("@AcquisitionNumber", EOS.AcquisitionNumber);
            SQLcmd.Parameters.AddWithValue("@UserName", username);
                                  

            SQLcmd.CommandText = SQLselect;

            //Set Database
            DataBase SQLDB = new DataBase(AppData.SQLServer, AppData.SQLDatabase, AppData.SQLAuthSQL, AppData.SQLUser, AppData.SQLPassword);

            //Fill a dataset with selected records
            measDsQueried = new DataSet();
            SQLDB.ReadDataSet(SQLcmd, ref measDsQueried);

            // you can make the grid readonly.
            dgViewMeasurements.ReadOnly = true;
            dgViewMeasurements.DataSource = measDsQueried.Tables[0];
            dgViewMeasurements.Columns["MeasurementID"].Visible = false;
            dgViewMeasurements.Columns["UserName"].HeaderText = "Created by";
            dgViewMeasurements.Columns["MeasurementName"].HeaderText = "Measurement name";
            // Resize the DataGridView columns to fit the newly loaded content.
            dgViewMeasurements.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.ColumnHeader);

            LoadTreeviewMeasurements();
            }
        }


        private void LoadTreeviewMeasurements()
        {
            treeViewMeasurements.Nodes.Clear();
            treeViewMeasurements.Nodes.Add("All users");
            if(measDs==null)
            { return; }

            DataView view = new DataView(measDs.Tables[0]);
            DataTable distinctValues = view.ToTable(true, "UserName");

            foreach (DataRow row in distinctValues.Rows)
            {
                treeViewMeasurements.Nodes[0].Nodes.Add(row[0].ToString());

            }
            treeViewMeasurements.ExpandAll();


        }
        private void btnDel_Click(object sender, EventArgs e)
        {
            //Set Database
            SQLDB = new DataBase(AppData.SQLServer, AppData.SQLDatabase, AppData.SQLAuthSQL, AppData.SQLUser, AppData.SQLPassword);

            //Check if rows selected
            int RowCount = dgViewMeasurements.Rows.GetRowCount(DataGridViewElementStates.Selected);

            if (RowCount == 0)
                return;

            if (MessageBox.Show(RowCount.ToString() + " measurement(s) selected for deletion.", "Delete Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Information, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
            {

                if (!AppData.localStudyUser._CanDelete)
                {
                    MessageBox.Show("You are not authorized to delete from database!", "Delete Confirmation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                for (int i = 0; i < RowCount; i++)
                {
                    //Get row of clicked cell
                    DataGridViewRow row = dgViewMeasurements.Rows[dgViewMeasurements.SelectedRows[i].Index];
                    //Get column AcquisitionNumber
                    DataGridViewCell cell = row.Cells["MeasurementID"];
                    //create AcquisitionObject to be deleted

                    Measurement DeleteMeasurement = new Measurement(SQLDB, Subject, EOS.AcquisitionNumber, Convert.ToInt32(cell.Value), AppData);

                    if(DeleteMeasurement._UserName !=AppData.globalUser._UserID)
                    {
                        MessageBox.Show("You can't delete this measurement ("+ DeleteMeasurement.MeasurementName + ") because you didn't create it (created by "+ DeleteMeasurement._UserName +").", "Delete rejected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    List<MeasurementDetail> MeasurementDetailList = DeleteMeasurement.GetAllMeasurementDetails();
                    if (MeasurementDetailList.Count != 0)
                    {
                        foreach (MeasurementDetail detail in MeasurementDetailList)
                        {
                           
                            detail.Delete();
                        }
                    }
                    DeleteMeasurement.AppData = AppData;
                    DeleteMeasurement.Delete();
                }
                refreshMeasurements();
            }
        }

        private void btnInfo_Click(object sender, EventArgs e)
        {
            //Set Database
            SQLDB = new DataBase(AppData.SQLServer, AppData.SQLDatabase, AppData.SQLAuthSQL, AppData.SQLUser, AppData.SQLPassword);

            int RowCount = dgViewMeasurements.Rows.GetRowCount(DataGridViewElementStates.Selected);
            //Check if rows selected
            if (RowCount == 0)
                return;
            for (int i = 0; i < RowCount; i++)
            {
                //Get row of clicked cell
                DataGridViewRow row = dgViewMeasurements.Rows[dgViewMeasurements.SelectedRows[i].Index];
                //Get column AcquisitionNumber
                DataGridViewCell cell = row.Cells["MeasurementID"];
                //create AcquisitionObject

                Measurement Measurement = new Measurement(SQLDB, Subject, EOS.AcquisitionNumber, Convert.ToInt32(cell.Value), AppData);

                frmMeasurementDetails frmMeasurementDetails = new frmMeasurementDetails();

                //Pass data to search form
                frmMeasurementDetails.AppData = this.AppData;
                frmMeasurementDetails.Subject = Subject;
                frmMeasurementDetails.SQLDB = SQLDB;
                frmMeasurementDetails.Measurement = Measurement;
                frmMeasurementDetails.ShowDialog();
            }

            refreshMeasurements();
        }

        private void btnCalcCenter_Click(object sender, EventArgs e)
        {

            //Set Database
            SQLDB = new DataBase(AppData.SQLServer, AppData.SQLDatabase, AppData.SQLAuthSQL, AppData.SQLUser, AppData.SQLPassword);

            //Check if rows selected
            int RowCount = dgViewMeasurements.Rows.GetRowCount(DataGridViewElementStates.Selected);

            if (RowCount == 0)
                return;
            if (RowCount == 1)
                return;
            if (RowCount > 2)
                return;


            //Get row of clicked cell
            DataGridViewRow row = dgViewMeasurements.Rows[dgViewMeasurements.SelectedRows[0].Index];
            //Get column AcquisitionNumber
            DataGridViewCell cell = row.Cells["MeasurementID"];
            //create AcquisitionObject to be deleted

            Measurement SelectedMeasurement1 = new Measurement(SQLDB, Subject, EOS.AcquisitionNumber, Convert.ToInt32(cell.Value), AppData);

            CalculateTo3D(SelectedMeasurement1);

            //Get row of clicked cell
            DataGridViewRow row2 = dgViewMeasurements.Rows[dgViewMeasurements.SelectedRows[1].Index];
            //Get column AcquisitionNumber
            DataGridViewCell cell2 = row2.Cells["MeasurementID"];
            //create AcquisitionObject to be deleted

            Measurement SelectedMeasurement2 = new Measurement(SQLDB, Subject, EOS.AcquisitionNumber, Convert.ToInt32(cell2.Value), AppData);

            CalculateTo3D(SelectedMeasurement2);
  computeCenterOf2Measurements(SelectedMeasurement1, SelectedMeasurement2);
            refreshMeasurements();

        }


        private void CalculateTo3D(Measurement SelectedMeasurement)
        {
            SelectedMeasurement.AppData = AppData;
            //Build SQL Command (=SQL select statement + Parameters)
            SqlCommand SQLcmd = new SqlCommand();

            string SQLselect = "SELECT PixelX, PixelY, ImagePanel FROM MeasurementDetail where MeasurementID = @MeasurementID and IsSinglePoint = @IsSinglePoint";
            SQLcmd.Parameters.AddWithValue("@MeasurementID", SelectedMeasurement.MeasurementID);
            SQLcmd.Parameters.AddWithValue("@IsSinglePoint", "true");

            SQLcmd.CommandText = SQLselect;

            //Fill a dataset with selected records
            DataSet ds = new DataSet();

            SQLDB.ReadDataSet(SQLcmd, ref ds);

            DataTable dt = ds.Tables[0];
            int p1X = 0;
            int p1Y = 0;
            int p2X = 0;
            int p2Y = 0;

            if (dt.Rows.Count == 0 || dt.Rows.Count == 1)
            {
                MessageBox.Show("The measurement: '" + SelectedMeasurement.MeasurementName + "' was not performed correctly. Click the Info button for more information. (Delete and redo!)", "Measurement Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            foreach (DataRow dr in dt.Rows)
            {
                if ((int)dr[2] == 1)    //this is the indication for the ImagePanel
                {
                    p1X = (int)dr[0];       //PixelX
                    p1Y = (int)dr[1];       //PixelY

                }
                if ((int)dr[2] == 2)
                {
                    p2X = (int)dr[0];
                    p2Y = (int)dr[1];
                }
            }

            double m1X = ConvertPixelToMeters((EosImage1.Columns / 2) - p1X, EosImage1.PixelSpacingX);
            double m1Y = ConvertPixelToMeters(EosImage1.Rows - p1Y, EosImage1.PixelSpacingY);   //Because Y coordinate is measured top down in EOS images.
            double m2X = ConvertPixelToMeters((EosImage2.Columns / 2) - p2X, EosImage2.PixelSpacingX);
            double m2Y = ConvertPixelToMeters(EosImage2.Rows - p2Y, EosImage2.PixelSpacingY);

            double xP;
            double zP;

            InverseProject(-m1X, m2X, out xP, out zP);

            Position pos = new Position((float)-xP, (float)((m1Y + m2Y) / 2), (float)zP);

            if (pos.X != pos.X)  //This is in case that de Pos is NaN
            {
                MessageBox.Show("Calculation could not be done", "Measurement Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (pos.Y != pos.Y)
            {
                MessageBox.Show("Calculation could not be done", "Measurement Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (pos.Z != pos.Z)
            {
                MessageBox.Show("Calculation could not be done", "Measurement Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);

                return;
            }
            //SelectedMeasurement.ReconstrCoords = pos;
            SelectedMeasurement.PosX = pos.X;
            SelectedMeasurement.PosY = pos.Y;
            SelectedMeasurement.PosZ = pos.Z;
            SelectedMeasurement.Save();

        }

        public double ConvertPixelToMeters(int pixelvalue, double pixelspacing)
        {

            return pixelvalue * pixelspacing;

        }

        public int ConvertMetersToPixels(double meters, double pixelspacing)
        {
            double temp = meters / pixelspacing;
            return (int)Math.Round(temp);

        }



        private void computeCenterOf2Measurements(Measurement meas1, Measurement meas2)
        {

          




            //Set Database
            SQLDB = new DataBase(AppData.SQLServer, AppData.SQLDatabase, AppData.SQLAuthSQL, AppData.SQLUser, AppData.SQLPassword);
            //Define Measurement Object
            Measurement CenterMeasurement = new Measurement(SQLDB, Subject, EOS.AcquisitionNumber, 0, AppData);
            CenterMeasurement.AppData = AppData;
            CenterMeasurement.MeasurementName = "Center_" + meas1.MeasurementName + "_AND_" + meas2.MeasurementName;
            CenterMeasurement.MeasurementComment = "Center";
            CenterMeasurement.PosX = (meas1.PosX + meas2.PosX) / 2;
            CenterMeasurement.PosY = (meas1.PosY + meas2.PosY) / 2;
            CenterMeasurement.PosZ = (meas1.PosZ + meas2.PosZ) / 2;
            CenterMeasurement.Save();

            refreshMeasurements();

            int RowCount = dgViewMeasurements.Rows.Count;

            DataGridViewRow row = dgViewMeasurements.Rows[RowCount - 1]; // dgViewMeasurements.Rows[dgViewMeasurements.SelectedRows[i].Index];

            dgViewMeasurements.Rows[RowCount - 1].Selected = true;
            //Get column AcquisitionNumber
            DataGridViewCell cell = row.Cells["MeasurementID"];
            //create AcquisitionObject to be deleted

            Measurement SelectedMeasurement = new Measurement(SQLDB, Subject, EOS.AcquisitionNumber, Convert.ToInt32(cell.Value), AppData);


            double xP;
            double zP;
            Project(SelectedMeasurement.PosX, SelectedMeasurement.PosZ, out xP, out zP);


            //double m1X = ConvertPixelToMeters((EosImage1.Columns / 2) - p1X, EosImage1.PixelSpacingX);
            //double m1Y = ConvertPixelToMeters(EosImage1.Rows - p1Y, EosImage1.PixelSpacingY);   //Because Y coordinate is measured top down in EOS images.
            //double m2X = ConvertPixelToMeters((EosImage2.Columns / 2) - p2X, EosImage2.PixelSpacingX);
            //double m2Y = ConvertPixelToMeters(EosImage2.Rows - p2Y, EosImage2.PixelSpacingY);

            int temp = ConvertMetersToPixels(xP, EosImage1.PixelSpacingX);

            int pixelX = (EosImage1.Columns / 2) - temp;
            int pixelY = EosImage1.Rows - ConvertMetersToPixels(SelectedMeasurement.PosY, EosImage1.PixelSpacingY);
            int pixelZ = (EosImage2.Columns / 2) + ConvertMetersToPixels(zP, EosImage2.PixelSpacingX);



            CenterMeasurement.AppData = AppData;

            MeasurementDetail MeasurementDetailPlane1 = new MeasurementDetail(SQLDB, Subject, SelectedMeasurement, 0);
            MeasurementDetailPlane1.appData = AppData;
            MeasurementDetailPlane1.PixelX = pixelX;
            MeasurementDetailPlane1.PixelY = pixelY;
            MeasurementDetailPlane1.IsSinglePoint = true.ToString();
            MeasurementDetailPlane1.ImagePanel = 1;
            MeasurementDetailPlane1.Save();

            MeasurementDetail MeasurementDetailPlane2 = new MeasurementDetail(SQLDB, Subject, SelectedMeasurement, 0);
            MeasurementDetailPlane2.appData = AppData;
            MeasurementDetailPlane2.PixelX = pixelZ;
            MeasurementDetailPlane2.PixelY = pixelY;
            MeasurementDetailPlane2.IsSinglePoint = true.ToString();
            MeasurementDetailPlane2.ImagePanel = 2;
            MeasurementDetailPlane2.Save();

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


        private void MoveMeasurement2Screen()
        {
            txtNameMeasurement.Text = Measurement.MeasurementName;
            txtCommentMeasurement.Text = Measurement.MeasurementComment;
        }

        private void MoveScreen2Measurement()
        {
            Measurement.MeasurementName = txtNameMeasurement.Text;
            Measurement.MeasurementComment = txtCommentMeasurement.Text;
            //Measurement.MeasurementID = Int32.Parse(txtMeasurementNb.Text);
        }


        private void btnConfirm_Click(object sender, EventArgs e)
        {
            if (groupBoxIm1.Enabled)
            {
                MoveScreen2Measurement();
                Measurement.Save();
                refreshMeasurements();

                btnConfirm.BackgroundImage = Properties.Resources.DoneIconGreen;
                //btnConfirm.Enabled = false;

                txtNameMeasurement.Enabled = false;
                txtCommentMeasurement.Enabled = false;
                groupBoxIm1.Enabled = false;
                groupBoxIm2.Enabled = false;
                if (RbtnIm1SinglePoint.Checked)
                {
                    _2DMeasurementsWorkpanel.SinglePointIsBeingDrawnIm1 = true;
                    _2DMeasurementsWorkpanel.EllipseIsBeingDrawnIm1 = false;
                }
                if (RbtnIm1Ellipse.Checked)
                {
                    _2DMeasurementsWorkpanel.EllipseIsBeingDrawnIm1 = true;
                    _2DMeasurementsWorkpanel.SinglePointIsBeingDrawnIm1 = false;
                }
                if (RbtnIm2SinglePoint.Checked)
                {
                    _2DMeasurementsWorkpanel.SinglePointIsBeingDrawnIm2 = true;
                    _2DMeasurementsWorkpanel.EllipseIsBeingDrawnIm2 = false;
                }
                if (RbtnIm2Ellipse.Checked)
                {
                    _2DMeasurementsWorkpanel.EllipseIsBeingDrawnIm2 = true;
                    _2DMeasurementsWorkpanel.SinglePointIsBeingDrawnIm2 = false;
                }
            }
            else
            {
                groupboxCurrentMeas.Enabled = false;
                groupboxCurrentMeas.Visible = false;
                _2DMeasurementsWorkpanel.EllipseIsBeingDrawnIm1 = false;
                _2DMeasurementsWorkpanel.SinglePointIsBeingDrawnIm1 = false;
                _2DMeasurementsWorkpanel.EllipseIsBeingDrawnIm2 = false;
                _2DMeasurementsWorkpanel.SinglePointIsBeingDrawnIm2 = false;
            }
            //groupboxCurrentMeas.Enabled = false;
            //groupboxCurrentMeas.Visible = false;
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            //btnReset.PerformClick();
            groupboxCurrentMeas.Enabled = true;
            groupboxCurrentMeas.Visible = true;
            txtCommentMeasurement.Enabled = true;
            txtNameMeasurement.Enabled = true;
            groupboxCurrentMeas.Text = "New Measurement";
            txtNameMeasurement.Text = "";
            txtCommentMeasurement.Text = "";
            RbtnIm1Ellipse.Checked = false;
            RbtnIm2Ellipse.Checked = false;
            RbtnIm1SinglePoint.Checked = false;
            RbtnIm2SinglePoint.Checked = false;

            //Set Database
            SQLDB = new DataBase(AppData.SQLServer, AppData.SQLDatabase, AppData.SQLAuthSQL, AppData.SQLUser, AppData.SQLPassword);
            //Define Measurement Object
            Measurement = new Measurement(SQLDB, Subject, EOS.AcquisitionNumber, 0, AppData);


            _2DMeasurementsWorkpanel.SinglePointIsBeingDrawnIm1 = false;
            _2DMeasurementsWorkpanel.EllipseIsBeingDrawnIm1 = false;
            _2DMeasurementsWorkpanel.SinglePointIsBeingDrawnIm2 = false;
            _2DMeasurementsWorkpanel.EllipseIsBeingDrawnIm2 = false;

            groupBoxIm1.Visible = true;
            groupBoxIm1.Enabled = true;
            groupBoxIm2.Visible = true;
            groupBoxIm2.Enabled = false;
        }

        private void btn3D_Click(object sender, EventArgs e)
        {
            //return selected measurement(s)

            //Check if rows selected
            //int RowCount = dgViewMeasurements.Rows.GetRowCount(DataGridViewElementStates.Selected);

            //if (RowCount == 0)
            //    return;


            //for (int i = 0; i < RowCount; i++)
            //{

            //    //Get row of clicked cell
            //    DataGridViewRow row = dgViewMeasurements.Rows[dgViewMeasurements.SelectedRows[i].Index];
            //    //Get column AcquisitionNumber
            //    DataGridViewCell cell = row.Cells["MeasurementID"];

            //    //Set Database
            //    DataBase SQLDB = new DataBase(AppData.SQLServer, AppData.SQLDatabase, AppData.SQLAuthSQL, AppData.SQLUser, AppData.SQLPassword);

            //    Measurement SelectedMeasurement = new Measurement(SQLDB, Subject, EOS.AcquisitionNumber, Convert.ToInt32(cell.Value), AppData);

            //    CalculateTo3D(SelectedMeasurement);

            //    if (SimModelVisualization.osimModel == null)
            //    {
            //        // MessageBox.Show("3D position of the marker has been calculated. To attach marker to model you have to load a model first.", "No model loaded!", MessageBoxButtons.OK, MessageBoxIcon.Error);

            //    }
            //    else
            //    {
            //        Position pos = new Position((float)SelectedMeasurement.PosX, (float)SelectedMeasurement.PosY, (float)SelectedMeasurement.PosZ);

            //         AddMarker(pos, SelectedMeasurement.MeasurementName);
            //    }
            //}

            //refreshMeasurements();
        }

        private void btnView_Click(object sender, EventArgs e)
        {

            _2DMeasurementsWorkpanel.ResetViews();
            //Set Database
            SQLDB = new DataBase(AppData.SQLServer, AppData.SQLDatabase, AppData.SQLAuthSQL, AppData.SQLUser, AppData.SQLPassword);

            //Check if rows selected
            int RowCount = dgViewMeasurements.Rows.GetRowCount(DataGridViewElementStates.Selected);

            if (RowCount == 0)
                return;

            for (int i = 0; i < RowCount; i++)
            {
                //Get row of clicked cell
                DataGridViewRow row = dgViewMeasurements.Rows[dgViewMeasurements.SelectedRows[i].Index];
                //Get column AcquisitionNumber
                DataGridViewCell cell = row.Cells["MeasurementID"];
                //create AcquisitionObject to be deleted

                Measurement SelectedMeasurement = new Measurement(SQLDB, Subject, EOS.AcquisitionNumber, Convert.ToInt32(cell.Value), AppData);

                List<MeasurementDetail> MeasurementDetailList = SelectedMeasurement.GetAllMeasurementDetails();
                if (MeasurementDetailList.Count != 0)
                {
                    foreach (MeasurementDetail detail in MeasurementDetailList)
                    {
                        int pixelsize;
                        if (detail.IsSinglePoint != "False     ")
                        {
                            pixelsize = 30;
                        }
                        else { pixelsize = 10; }

                        if (detail.ImagePanel == 1)
                        {
                            for (int k = 0; k < pixelsize; k++)
                            {
                                for (int j = 0; j < pixelsize; j++)
                                {
                                    if (detail.PixelX < pixelsize / 2)
                                    { pixelsize = detail.PixelX; }
                                    if (_2DMeasurementsWorkpanel.picturebox1Width() - detail.PixelX < pixelsize / 2)
                                    { pixelsize = _2DMeasurementsWorkpanel.picturebox1Width() - detail.PixelX; }
                                    if (detail.PixelY < pixelsize / 2)
                                    { pixelsize = detail.PixelY; }
                                    if (_2DMeasurementsWorkpanel.picturebox1Height() - detail.PixelY < pixelsize / 2)
                                    { pixelsize = _2DMeasurementsWorkpanel.picturebox1Height() - detail.PixelY; }
                                    _2DMeasurementsWorkpanel.ColorPixelonPictureBox1(detail.PixelX - (pixelsize / 2) + k, detail.PixelY - (pixelsize / 2) + j);

                                  
                                }
                            }


                        }
                        if (detail.ImagePanel == 2)
                        {
                            for (int k = 0; k < pixelsize; k++)
                            {
                                for (int j = 0; j < pixelsize; j++)
                                {
                                    if (detail.PixelX < pixelsize / 2)
                                    { pixelsize = detail.PixelX; }
                                    if (_2DMeasurementsWorkpanel.picturebox2Width() - detail.PixelX < pixelsize / 2)
                                    { pixelsize = _2DMeasurementsWorkpanel.picturebox2Width() - detail.PixelX; }
                                    if (detail.PixelY < pixelsize / 2)
                                    { pixelsize = detail.PixelY; }
                                    if (_2DMeasurementsWorkpanel.picturebox2Height() - detail.PixelY < pixelsize / 2)
                                    { pixelsize = _2DMeasurementsWorkpanel.picturebox2Height() - detail.PixelY; }
                                  
                                    _2DMeasurementsWorkpanel.ColorPixelonPictureBox2(detail.PixelX - (pixelsize / 2) + k, detail.PixelY - (pixelsize / 2) + j);
                                }
                            }


                        }
                    }
                }
            }
        }

        private void dgViewMeasurements_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            btnInfo.PerformClick();
        }

        private void RbtnIm1Ellipse_CheckedChanged(object sender, EventArgs e)
        {
            groupBoxIm2.Enabled = true;
        }

        private void RbtnIm1SinglePoint_CheckedChanged(object sender, EventArgs e)
        {
            groupBoxIm2.Enabled = true;
            RbtnIm2SinglePoint.Checked = true;
        }

        private void txtNameMeasurement_TextChanged(object sender, EventArgs e)
        {
            btnConfirm.Enabled = true;
            btnConfirm.BackgroundImage = Properties.Resources.imgSave;
        }

        private void txtCommentMeasurement_TextChanged(object sender, EventArgs e)
        {
            btnConfirm.Enabled = true;
            btnConfirm.BackgroundImage = Properties.Resources.imgSave;
        }

        public void ExportToExcel()
        {
            //EXPORT TO EXCEL FILE DIRECTLY


            if (EOS == null)
            { return; }

            //Build SQL Command (=SQL select statement + Parameters)
            SqlCommand SQLcmd = new SqlCommand();

            string SQLselect;

         
          
                SQLselect = "SELECT MeasurementID, MeasurementName, PosX, PosY, PosZ, MeasurementComment, PatientID, UserName, AcquisitionNumber FROM MeasurementHeader where AcquisitionNumber = @AcquisitionNumber";
                SQLcmd.Parameters.AddWithValue("@AcquisitionNumber", EOS.AcquisitionNumber);
           
            SQLcmd.CommandText = SQLselect;


            //Fill a dataset with selected records
            DataSet measDs = new DataSet();
            SQLDB.ReadDataSet(SQLcmd, ref measDs);



            string sourcePath;
            string fileName;
            if (vistaFolderBrowserDialog1.ShowDialog(this) == DialogResult.OK)
            {
                sourcePath = vistaFolderBrowserDialog1.SelectedPath;
            }
            else { return; }

            if (inputDialog1.ShowDialog(this) == DialogResult.OK)
            {
                fileName = inputDialog1.Input;
            }
            else { return; }



            XLWorkbook wb = new XLWorkbook();
            DataTable dt = measDs.Tables[0];
            wb.Worksheets.Add(dt, "Measurements export");
            wb.SaveAs(sourcePath + "/" + fileName + ".xlsx");

            if (MessageBox.Show("The Excel file was saved at: " + sourcePath + @"\" + fileName + ".xlsx", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information) == DialogResult.OK)
            {

            }


        }

        private void UC_measurementsMain_Load(object sender, EventArgs e)
        {
            DoubleBufferDGV(dgViewMeasurements);

        }

        public void PrintMarkersToTrc()
        {

            //Set Database
            SQLDB = new DataBase(AppData.SQLServer, AppData.SQLDatabase, AppData.SQLAuthSQL, AppData.SQLUser, AppData.SQLPassword);

            //Check if rows selected
            int RowCount = dgViewMeasurements.Rows.GetRowCount(DataGridViewElementStates.Selected);

            if (RowCount == 0)
            {
                MessageBox.Show("No measurments selected!", "Select points!", MessageBoxButtons.OK, MessageBoxIcon.Information);

                return;
            }




            //Some almost always identical header text

            var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("MarkerExport");

            ws.Cell(1, 1).Value = "PathFileType";
            ws.Cell(1, 2).Value = 4;
            ws.Cell(1, 3).Value = "(X/Y/Z)";
            ws.Cell(1, 4).Value = "NaamBestand.trc";


            ws.Cell(2, 1).Value = "DataRate";
            ws.Cell(2, 2).Value = "CameraRate";
            ws.Cell(2, 3).Value = "NumFrames";
            ws.Cell(2, 4).Value = "NumMarkers";
            ws.Cell(2, 5).Value = "Units";
            ws.Cell(2, 6).Value = "OrigDataRate";
            ws.Cell(2, 7).Value = "OrigDataStartFrame";
            ws.Cell(2, 8).Value = "OrigNumFrames";

            ws.Cell(3, 1).Value = 60;
            ws.Cell(3, 2).Value = 60;
            ws.Cell(3, 3).Value = 100;
            ws.Cell(3, 4).Value = dgViewMeasurements.SelectedRows.Count; // "NumMarkers";
            ws.Cell(3, 5).Value = "mm";
            ws.Cell(3, 6).Value = 60; // "OrigDataRate";
            ws.Cell(3, 7).Value = 1; // "OrigDataStartFrame";
            ws.Cell(3, 8).Value = 100;// "OrigNumFrames";

            ws.Cell(4, 1).Value = "Frame#";
            ws.Cell(4, 2).Value = "Time";

            int frames = 60;
            int cameraRate = 60;

            for (int i = 0; i < RowCount; i++)
            {
                //Get row of clicked cell
                DataGridViewRow row = dgViewMeasurements.Rows[dgViewMeasurements.SelectedRows[i].Index];
                //Get column AcquisitionNumber
                DataGridViewCell cell = row.Cells["MeasurementID"];
                //create AcquisitionObject to be deleted

                Measurement SelectedMeasurement = new Measurement(SQLDB, Subject, EOS.AcquisitionNumber, Convert.ToInt32(cell.Value), AppData);

                ws.Cell(4, 3 * (i + 1)).Value = SelectedMeasurement.MeasurementName;
                ws.Cell(5, 3 * (i + 1)).Value = "X" + (i + 1).ToString();
                ws.Cell(5, (3 * (i + 1)) + 1).Value = "Y" + (i + 1).ToString();
                ws.Cell(5, (3 * (i + 1)) + 2).Value = "Z" + (i + 1).ToString();
            }

            for (int f = 1; f <= frames; f++)
            {
                NumberFormatInfo nfi = new NumberFormatInfo();
                nfi.NumberDecimalSeparator = ".";

                ws.Cell(6 + f, 1).Value = f;
                ws.Cell(6 + f, 2).Value = Math.Round(((double)(1 / (double)cameraRate) * (f - 1)), 3).ToString(nfi);

                for (int i = 0; i < RowCount; i++)
                {
                    //Get row of clicked cell
                    DataGridViewRow row = dgViewMeasurements.Rows[dgViewMeasurements.SelectedRows[i].Index];
                    //Get column AcquisitionNumber
                    DataGridViewCell cell = row.Cells["MeasurementID"];
                    //create AcquisitionObject to be deleted

                    Measurement SelectedMeasurement = new Measurement(SQLDB, Subject, EOS.AcquisitionNumber, Convert.ToInt32(cell.Value), AppData);





                    ws.Cell(6 + f, 3 * (i + 1)).Value = (SelectedMeasurement.PosX * 1000).ToString(nfi);
                    ws.Cell(6 + f, (3 * (i + 1)) + 1).Value = (SelectedMeasurement.PosY * 1000).ToString(nfi);
                    ws.Cell(6 + f, (3 * (i + 1)) + 2).Value = (SelectedMeasurement.PosZ * 1000).ToString(nfi);

                }

            }

            ////Add headers and Xn, Yn , Zn names
            //foreach (Marker)
            //    X
            //    Y
            //            Z



            //foreach (frame)
            //{
            //    //Fill frame value and Time value
            //    foreach(Marker)
            //            X
            //            Y
            //            Z
            //}


            //Define where to place the file




            //Some predefined value for the amount of frames there need to be created. 


            //Changing the extension from.XLXS to .TRC.


            //Opening the folder
            string sourcePath = @"C:/uz/Temp";
            string filename = "MarkerExport_" + DateTime.Now.ToString().Replace(':', '_');
            //wb.SaveAs(sourcePath + "/" + filename + ".xlsx");


            Directory.CreateDirectory(sourcePath);
            string myffile = sourcePath + @"/" + filename + ".xlsx";
            string csvmyffile = sourcePath + @"/" + filename + ".csv";

            //        var lastCellAddress = ws.RangeUsed().LastCell().Address;
            //        File.WriteAllLines(csvmyffile, ws.Rows(1, lastCellAddress.RowNumber)
            //.Select(r => string.Join(",", r.Cells(1, lastCellAddress.ColumnNumber)
            //        .Select(cell =>
            //        {
            //            var cellValue = cell.GetValue<string>();
            //            return cellValue.Contains(",") ? $"\"{cellValue}\"" : cellValue;
            //        }))));



            System.IO.File.WriteAllLines(csvmyffile,
    ws.RowsUsed().Select(row =>
        string.Join("\t", row.Cells(1, row.LastCellUsed(false).Address.ColumnNumber)
                            .Select(cell => cell.GetValue<string>().Replace(',', '.'))
 )));


            File.Move(csvmyffile, Path.ChangeExtension(csvmyffile, ".trc"));


            if (MessageBox.Show("The Excel file was saved at: " + sourcePath + @"/" + "testMarkerExport" + ".trc", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information) == DialogResult.OK)
            {

            }
        }

        private void treeViewMeasurements_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            refreshMeasurements(e.Node.Text);
        }

        private void btnCalculateMean_Click(object sender, EventArgs e)
        {
            //Set Database
            SQLDB = new DataBase(AppData.SQLServer, AppData.SQLDatabase, AppData.SQLAuthSQL, AppData.SQLUser, AppData.SQLPassword);

            //Check if rows selected
            int RowCount = dgViewMeasurements.Rows.GetRowCount(DataGridViewElementStates.Selected);

            if (RowCount == 0)
                return;
            if (RowCount == 1)
                return;

            List<Measurement> temp = new List<Measurement>();
            double x = 0;
            double y = 0;
            double z = 0;


            foreach (DataGridViewRow rowx in dgViewMeasurements.SelectedRows)
            {
               
                Measurement SelectedMeasurement = new Measurement(SQLDB, Subject, EOS.AcquisitionNumber, Convert.ToInt32(rowx.Cells["MeasurementID"].Value), AppData);
                temp.Add(SelectedMeasurement);
                x += SelectedMeasurement.PosX;
                y += SelectedMeasurement.PosY;
                z += SelectedMeasurement.PosZ;
            }

            Measurement NewMeasurementMean = new Measurement(SQLDB, Subject, EOS.AcquisitionNumber,0, AppData);
            NewMeasurementMean.PosX = x/ dgViewMeasurements.SelectedRows.Count;
            NewMeasurementMean.PosY = y/ dgViewMeasurements.SelectedRows.Count;
            NewMeasurementMean.PosZ = z/ dgViewMeasurements.SelectedRows.Count;

            NewMeasurementMean.MeasurementName = temp[0].MeasurementName;
            NewMeasurementMean.MeasurementComment = "Calculated MEAN";

            NewMeasurementMean.Save();



           // refreshMeasurements();

        }
    }
}
