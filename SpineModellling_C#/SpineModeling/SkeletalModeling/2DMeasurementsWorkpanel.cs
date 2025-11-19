using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Data.SqlClient;
using EvilDICOM.Core;
using Dicom.Imaging; // Add this using directive
using Dicom.Imaging.WinForms; // Add this using directive

//using FitEllipse;
using OpenSim;

namespace SpineAnalyzer.SkeletalModeling
{
    public partial class _2DMeasurementsWorkpanel : UserControl
    {
        public AppData AppData;
        public DataBase SQLDB;
        public Subject Subject; 
        public EosImage EosImage1;
        public EosImage EosImage2;
        public DICOMImage dcmImage1;
        public DICOMImage dcmImage2;
        private MeasurementDetail MeasurementDetail;
        public Measurement Measurement;
        public UC_measurementsMain UC_measurementsMain = new UC_measurementsMain();
        public Acquisitions.EOS EOS;
        public bool Loaded2D = false;


        public EosSpace EosSpace; 

       public Boolean SinglePointIsBeingDrawnIm1 = false;
        public Boolean EllipseIsBeingDrawnIm1 = false;

public        Boolean SinglePointIsBeingDrawnIm2 = false;
      public  Boolean EllipseIsBeingDrawnIm2 = false;

        //Emgu.CV.Image<Bgr, Byte> My_Image1;
        private System.Drawing.Image _originalImage1;
        // private Bitmap OriginalImage1 = null;
        private Rectangle _selection;
        private Rectangle _selectionLocalCoords;
        private int X0, Y0, X1, Y1, X2, Y2;
        private bool _selecting;
        public Position upperLeftCorner1 = new Position(0, 0, 0);   //Are used to track the zoom. 
        public Position upperLeftCorner2 = new Position(0, 0, 0);
        private Point mouseDownPosition = Point.Empty;
        private Point mouseMovePosition = Point.Empty;
        public bool drawCircle = false;
        private bool isMoving = false;
        private Dictionary<Point, Point> Circles = new Dictionary<Point, Point>();

        //Emgu.CV.Image<Bgr, Byte> My_Image2;
        private System.Drawing.Image _originalImage2;

        public void LoadImages()
        {

            //New update for DICOM images


            Dicom.Imaging.ImageManager.SetImplementation(new WinFormsImageManager());
            var image1 = new Dicom.Imaging.DicomImage(EosImage1.Directory);

            Bitmap bitmap1 = image1.RenderImage().AsClonedBitmap();

                  
                    if (EosImage1.imageRotated)
                    {
                        bitmap1.RotateFlip(RotateFlipType.Rotate180FlipY);  //TEST AIS
                    }
                    pictureBox1.Image = bitmap1;
                    pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;

            string dir1bis = AppData.TempDir + "\\tempF.png";
            bitmap1.Save(dir1bis);
            EosImage1.BMP = bitmap1;
               


            var image2 = new Dicom.Imaging.DicomImage(EosImage2.Directory);

            Bitmap bitmap2 = image2.RenderImage().AsClonedBitmap();

            if (EosImage2.imageRotated)
            {
                bitmap2.RotateFlip(RotateFlipType.Rotate180FlipY);  //TEST AIS
            }
            pictureBox2.Image = bitmap2;
            pictureBox2.SizeMode = PictureBoxSizeMode.Zoom;

            string dir2bis = AppData.TempDir + "\\tempL.png";
            bitmap2.Save(dir2bis);
            EosImage2.BMP = bitmap2;


            if (false)
            {

                            DICOMObject dcm1 = EvilDICOM.DICOMObject.Read(EosImage1.Directory);
            dcmImage1 = new DICOMImage((EvilDICOM.DICOMObject)dcm1);


                if (EosImage1.imageRotated)
                {
                    dcmImage1.BMP.RotateFlip(RotateFlipType.Rotate180FlipY);  //TEST AIS
                }
                pictureBox1.Image = dcmImage1.BMP;
                pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;


                string dir1 = AppData.TempDir + "\\tempF.png";
                dcmImage1.SavePNG(dir1);
                EosImage1.BMP = dcmImage1.BMP;




                //Load and Display the Image
                DICOMObject dcm2 = EvilDICOM.DICOMObject.Read(EosImage2.Directory);
                dcmImage2 = new DICOMImage(dcm2);
                if (EosImage2.imageRotated)
                {
                    dcmImage2.BMP.RotateFlip(RotateFlipType.Rotate180FlipY);   //TEST AIS
                }
                pictureBox2.Image = dcmImage2.BMP;
                EosImage2.BMP = dcmImage2.BMP;
                pictureBox2.SizeMode = PictureBoxSizeMode.Zoom;
                //OriginalImage2 = dcmImage2.BMP;
                string dir2 = AppData.TempDir + "\\tempL.png";
                dcmImage2.SavePNG(dir2);

            }

            // Save just a copy of the image on no reference!
            _originalImage1 = pictureBox1.Image.Clone() as System.Drawing.Image;

            // Save just a copy of the image on no reference!
            _originalImage2 = pictureBox2.Image.Clone() as System.Drawing.Image;
            UC_measurementsMain._2DMeasurementsWorkpanel = this;
            UC_measurementsMain.Subject = Subject;
            UC_measurementsMain.SQLDB = SQLDB;
            UC_measurementsMain.AppData = AppData;
            UC_measurementsMain.EosImage1 = EosImage1;
            UC_measurementsMain.EosImage2 = EosImage2;
            UC_measurementsMain.EosSpace = EosSpace;
            UC_measurementsMain.EOS = EOS;
            LoadMeasurements();

            if (EOS != null)
            {
                UC_measurementsMain.refreshMeasurements();
            }

            Loaded2D = true;
            }
        public int picturebox1Width()
        {

           return pictureBox1.Image.Width;
        }
        public int picturebox1Height()
        {

            return pictureBox1.Image.Height;
        }
        public int picturebox2Width()
        {

            return pictureBox2.Image.Width;
        }
        public int picturebox2Height()
        {

            return pictureBox2.Image.Height;
        }
        private void LoadMeasurements()
        {
            UC_measurementsMain.AppData = AppData;
            UC_measurementsMain.SQLDB = SQLDB;
            flowLayoutPanel1.Controls.Add(UC_measurementsMain);


        }


        public void ColorPixelonPictureBox1(int x, int y)
        {
            try { 
            ((Bitmap)pictureBox1.Image).SetPixel(x,y, System.Drawing.Color.Red);
                 }
            catch
            { }
        }
        public void ColorPixelonPictureBox2(int x, int y)
        {
            try
            {



                ((Bitmap)pictureBox2.Image).SetPixel(x, y, System.Drawing.Color.Red);
            }
            catch
            {

            }
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            ResetViews();
        }

        public void ResetViews()
        {
            pictureBox1.Image = _originalImage1.Clone() as System.Drawing.Image;   //Image is ambiguous. TODO: VERKEERD !!!!!
            pictureBox2.Image = _originalImage2.Clone() as System.Drawing.Image;   //Image is ambiguous.
            upperLeftCorner1.X = 0;
            upperLeftCorner1.Y = 0;
            upperLeftCorner2.X = 0;
            upperLeftCorner2.Y = 0;

        }

        private void pictureBox2_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (SinglePointIsBeingDrawnIm2)
                {
                    ConvertCoordinates(pictureBox2, out X0, out Y0, e.X, e.Y);
                    DrawPointOnImage(pictureBox2, e.X, e.Y);
                    SavePointOnImage(2, (X0 + (int)upperLeftCorner2.X), (Y0 + (int)upperLeftCorner2.Y), true);
                    DrawSuggestionLine(pictureBox1, (int)upperLeftCorner2.Y + Y0);
                }
                if (EllipseIsBeingDrawnIm2)
                {
                    ConvertCoordinates(pictureBox2, out X0, out Y0, e.X, e.Y);
                    DrawPointOnImage(pictureBox2, e.X, e.Y);
                    SavePointOnImage(2, (X0 + (int)upperLeftCorner2.X), (Y0 + (int)upperLeftCorner2.Y), false);
                    int height = CalculateEllipseCenter(2);
                    if (height == -1)
                    { return; }
                    else
                    {
                        DrawSuggestionLine(pictureBox1, height);
                    }
                    //DrawSuggestionLine(pictureBox1, (int)upperLeftCorner2.Y + Y0);
                }
                if (drawCircle)
                {
                    pictureBox2.Cursor = Cursors.Cross;
                    isMoving = true;
                    mouseDownPosition = e.Location;

                }
                else
                { return; }
            }

            // Starting point of the selection:
            if (e.Button == MouseButtons.Right)
            {
                ConvertCoordinates(pictureBox2, out X0, out Y0, e.X, e.Y);
                if (X0 < 0 || X0 > pictureBox2.Image.Width)
                {
                    _selecting = false;
                    return;
                }
                if (Y0 < 0 || Y0 > pictureBox2.Image.Height)
                {
                    _selecting = false;
                    return;
                }

                _selecting = true;
                _selection = new Rectangle(new Point(X0, Y0), new Size());
                _selectionLocalCoords = new Rectangle(new Point(e.X, e.Y), new Size());
            }
        }
        private void SavePointOnImage(int panel, int x, int y, Boolean isSinglePoint)
        {
            //Set Database
            SQLDB = new DataBase(AppData.SQLServer, AppData.SQLDatabase, AppData.SQLAuthSQL, AppData.SQLUser, AppData.SQLPassword);

            //Check if rows selected
            //int RowCount = dgViewMeasurements.Rows.GetRowCount(DataGridViewElementStates.Selected);
            int RowCount = UC_measurementsMain.dgViewMeasurements.Rows.Count;

            if (RowCount == 0)
                return;

            //unselect all rows
            for (int i = 0; i < (RowCount - 1); i++)
            {
                UC_measurementsMain.dgViewMeasurements.Rows[i].Selected = false;

            }

            DataGridViewRow row = UC_measurementsMain.dgViewMeasurements.Rows[RowCount - 1]; // dgViewMeasurements.Rows[dgViewMeasurements.SelectedRows[i].Index];

            UC_measurementsMain.dgViewMeasurements.Rows[RowCount - 1].Selected = true;
            //Get column AcquisitionNumber
            DataGridViewCell cell = row.Cells["MeasurementID"];
            //create AcquisitionObject to be deleted

            Measurement SelectedMeasurement = new Measurement(SQLDB, Subject, EOS.AcquisitionNumber, Convert.ToInt32(cell.Value), AppData);

            //Define Measurementdetail Object
            List<MeasurementDetail> MeasurementDetailsList = SelectedMeasurement.GetAllMeasurementDetails();
            List<MeasurementDetail> FilteredList = SinglePointFilterMeasurementDetailList(PanelFilterMeasurementDetailList(MeasurementDetailsList, panel), "True      ");

            if (FilteredList.Count == 0)
            {
                MeasurementDetail = new MeasurementDetail(SQLDB, Subject, SelectedMeasurement, 0);
            }
            else
            {
                MeasurementDetail = new MeasurementDetail(SQLDB, Subject, SelectedMeasurement, FilteredList[0].MeasurementNumber);
            }

            MeasurementDetail.appData = AppData;
            MeasurementDetail.ImagePanel = panel;
            MeasurementDetail.IsSinglePoint = isSinglePoint.ToString();
            MeasurementDetail.PixelX = x;
            MeasurementDetail.PixelY = y;

            MeasurementDetail.Save();

        }


        private List<MeasurementDetail> PanelFilterMeasurementDetailList(List<MeasurementDetail> MeasurementDetailList, int Imagepanel)
        {
            List<MeasurementDetail> FilteredMeasurementDetailList = new List<MeasurementDetail>();

            foreach (MeasurementDetail detail in MeasurementDetailList)
            {
                if (detail.ImagePanel == Imagepanel)
                {
                    FilteredMeasurementDetailList.Add(detail);
                }
            }
            return FilteredMeasurementDetailList;
        }


        private List<MeasurementDetail> SinglePointFilterMeasurementDetailList(List<MeasurementDetail> MeasurementDetailList, string IsSinlePoint)
        {
            List<MeasurementDetail> FilteredMeasurementDetailList = new List<MeasurementDetail>();

            foreach (MeasurementDetail detail in MeasurementDetailList)
            {
                if (detail.IsSinglePoint == IsSinlePoint)
                {
                    FilteredMeasurementDetailList.Add(detail);
                }
            }
            return FilteredMeasurementDetailList;
        }
        private void DrawPointOnImage(PictureBox picBox, int x, int y)
        {
            SolidBrush brush = new SolidBrush(Color.GreenYellow);
            Graphics gr = picBox.CreateGraphics();
            gr.FillEllipse(brush, x, y, 7, 7);
            picBox.Update();
            // picBox.Refresh();
            //Color c = Color.FromName("SlateBlue");
            //Pen p = new Pen(c, 5);
            //gr.DrawEllipse(pen, x,y, 5,5))

        }

        private void DrawSuggestionLine(PictureBox picBox, int y)
        {
            Point p1 = new Point();
            Point p2 = new Point();
            p1.X = 0;
            p2.X = picBox.Image.Width;
            if (picBox == pictureBox1)
            {
                p1.Y = y - (int)upperLeftCorner1.Y;
                p2.Y = y - (int)upperLeftCorner1.Y;
            }
            if (picBox == pictureBox2)
            {
                p1.Y = y - (int)upperLeftCorner2.Y;
                p2.Y = y - (int)upperLeftCorner2.Y;
            }

            Color c = Color.FromName("SlateBlue");
            drawline(picBox, p1, p2, 5, c);
            Graphics gr = picBox.CreateGraphics();
        }


        private void drawline(PictureBox pb, System.Drawing.Point p1, System.Drawing.Point p2, float Bwidth, Color c1)
        {
            //refresh the picture box
            pb.Refresh();
            //create a new Bitmap object
            Bitmap map = (Bitmap)pb.Image;
            //create a graphics object
            Graphics g = Graphics.FromImage(map);
            //create a pen object and setting the color and width for the pen
            Pen p = new Pen(c1, Bwidth);
            //draw line between  point p1 and p2
            try
            {
                g.DrawLine(p, p1, p2);
            }
            catch
            {
                MessageBox.Show("Error. Trying to solve it as soon as possible... please try again. CURRENTLY IN FIX LINE.", "BUG", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            //pb.Image = map;
            //dispose pen and graphics object
            p.Dispose();
            g.Dispose();
            pb.Refresh();
        }

        private void pictureBox2_MouseMove(object sender, MouseEventArgs e)
        {
            //if no image is loaded:
            //if ((pictureBox1.Image == null)) return;

            ConvertCoordinates(pictureBox2, out X0, out Y0, e.X, e.Y);
            X_pos_LBL2.Text = "X: " + (X0 + (int)upperLeftCorner2.X).ToString();
            Y_pos_LBL2.Text = "Y: " + (Y0 + (int)upperLeftCorner2.Y).ToString();
            //Val_LBL2.Text = "Value: " + pictureBox2.Image[e.Y, e.X].ToString();

            // Update the actual size of the selection:
            if (_selecting)
            {
                if (X0 > pictureBox2.Image.Width)
                { _selection.Width = pictureBox2.Image.Width - _selection.X; }
                else { _selection.Width = X0 - _selection.X; }                           //Used for cropping the (already cropped) image
                if (Y0 > pictureBox2.Image.Height)
                { _selection.Height = pictureBox2.Image.Height - _selection.Y; }   //Limit the selection to the lower border of the image.
                else { _selection.Height = Y0 - _selection.Y; }
                _selectionLocalCoords.Width = e.X - _selectionLocalCoords.X;    //Used for drawing the zoom box.
                _selectionLocalCoords.Height = e.Y - _selectionLocalCoords.Y;

                // Redraw the picturebox:
                pictureBox2.Refresh();
            }
            if (drawCircle)
            {
                if (isMoving)
                {
                    mouseMovePosition = e.Location;
                    pictureBox2.Invalidate();
                }

            }
        }

        private void pictureBox2_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && _selecting)
            {
                if (_selection.Height < 0 || _selection.Width < 0)
                {
                    _selecting = false;
                    return;
                }

                // Create cropped image:
                System.Drawing.Image img = pictureBox2.Image.Crop(_selection);
                // Fit image to the picturebox:
                pictureBox2.Image = img;



                upperLeftCorner2.X += _selection.X;
                upperLeftCorner2.Y += _selection.Y;
                _selecting = false;
            }
            if (drawCircle)
            {
                pictureBox2.Cursor = Cursors.Default;
                if (isMoving)
                {
                    Circles.Clear();
                    Circles = new Dictionary<Point, Point>();

                    Circles.Add(mouseDownPosition, mouseMovePosition);
                }
                isMoving = false;
                drawCircle = false;
                btnDrawCircle.Enabled = true;
            }
        }

        private void btnMeasurments_Click(object sender, EventArgs e)
        {
            UC_measurementsMain.dgViewMeasurements.SelectAll();

            // Set Database
            SQLDB = new DataBase(AppData.SQLServer, AppData.SQLDatabase, AppData.SQLAuthSQL, AppData.SQLUser, AppData.SQLPassword);

            //Check if rows selected
            int RowCount = UC_measurementsMain.dgViewMeasurements.Rows.GetRowCount(DataGridViewElementStates.Selected);

            if (RowCount == 0)
                return;

            for (int i = 0; i < RowCount; i++)
            {
                //Get row of clicked cell
                DataGridViewRow row = UC_measurementsMain.dgViewMeasurements.Rows[UC_measurementsMain.dgViewMeasurements.SelectedRows[i].Index];
                //Get column AcquisitionNumber
                DataGridViewCell cell = row.Cells["MeasurementID"];
                //create AcquisitionObject to be deleted

                Measurement SelectedMeasurement = new Measurement(SQLDB, Subject, EOS.AcquisitionNumber, Convert.ToInt32(cell.Value), AppData);


                CalculateTo3D(SelectedMeasurement);
            }


            //  btn3D.PerformClick();
            frmMetrics frmMetricsInstance = new frmMetrics();

                
            //Pass data to search form
            frmMetrics.AppData = this.AppData;
            frmMetrics.SQLDB = SQLDB;
            frmMetrics.EOS = this.EOS;
            frmMetrics.EosSpace = this.EosSpace;
            frmMetrics.Subject = Subject;
            frmMetrics.Measurement = Measurement;
            frmMetrics.ShowDialog();
        }

        private void btnDrawCircle_Click(object sender, EventArgs e)
        {
            drawCircle = true;
            btnDrawCircle.Enabled = false;
        }

        private void _2DMeasurementsWorkpanel_Load(object sender, EventArgs e)
        {
            ((frmImageAnalysis_new)(this.ParentForm)).AddToLogsAndMessages("2D image panels initialized.");
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

        //private Bitmap OriginalImage2 = null;

        public double ConvertPixelToMeters(int pixelvalue, double pixelspacing)
        {

            return pixelvalue * pixelspacing;

        }

        public int ConvertMetersToPixels(double meters, double pixelspacing)
        {
            double temp = meters / pixelspacing;
            return (int)Math.Round(temp);

        }
        public _2DMeasurementsWorkpanel()
        {
            InitializeComponent();
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



        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
           
            if (e.Button == MouseButtons.Right && _selecting)
            {
                if (_selection.Height < 0 || _selection.Width < 0)
                {
                    _selecting = false;
                    return;
                }

                // Create cropped image:
                System.Drawing.Image img = pictureBox1.Image.Crop(_selection);

                // Fit image to the picturebox:
                pictureBox1.Image = img;
                upperLeftCorner1.X += _selection.X;
                upperLeftCorner1.Y += _selection.Y;
                _selecting = false;
            }
            if (drawCircle)
            {
                pictureBox1.Cursor = Cursors.Default;
                if (isMoving)
                {
                    Circles.Clear();
                    Circles = new Dictionary<Point, Point>();

                    Circles.Add(mouseDownPosition, mouseMovePosition);
                }
                isMoving = false;
                drawCircle = false;
                btnDrawCircle.Enabled = true;
            }
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            if (_selecting)
            {
                // Draw a rectangle displaying the current selection
                Pen pen = Pens.GreenYellow;
                int tempX = pictureBox1.Width / 2 - pictureBox1.Image.Width / 2;
                int tempY = pictureBox1.Height / 2 - pictureBox1.Image.Height / 2;
                pictureBox1.Padding = new Padding(0, 0, 0, 0);

                //_selectionLocalCoords.X = _selectionLocalCoords.X + tempX;
                //_selectionLocalCoords.Y = _selectionLocalCoords.Y + tempY;
                e.Graphics.DrawRectangle(pen, _selectionLocalCoords);
            }
            if (drawCircle)
            {
                Pen p = new Pen(Color.Red);
                var g = e.Graphics;
                if (isMoving)
                {
                    double straal2;
                    double valueHor2 = mouseDownPosition.X - mouseMovePosition.X;
                    double valueVert2 = mouseDownPosition.Y - mouseMovePosition.Y;

                    straal2 = Math.Sqrt(valueHor2 * valueHor2 + valueVert2 * valueVert2);
                    g.DrawEllipse(p, mouseDownPosition.X - (int)straal2, mouseDownPosition.Y - (int)straal2, (int)(2 * straal2), (int)(2 * straal2));

                }

            }
        }

        private void pictureBox2_Paint(object sender, PaintEventArgs e)
        {
            if (_selecting)
            {
                // Draw a rectangle displaying the current selection
                Pen pen = Pens.GreenYellow;
                pictureBox2.Padding = new Padding(0, 0, 0, 0);

                e.Graphics.DrawRectangle(pen, _selectionLocalCoords);
            }
            if (drawCircle)
            {
                Pen p = new Pen(Color.Red);
                var g = e.Graphics;
                if (isMoving)
                {
                    double straal2;
                    double valueHor2 = mouseDownPosition.X - mouseMovePosition.X;
                    double valueVert2 = mouseDownPosition.Y - mouseMovePosition.Y;

                    straal2 = Math.Sqrt(valueHor2 * valueHor2 + valueVert2 * valueVert2);
                    g.DrawEllipse(p, mouseDownPosition.X - (int)straal2, mouseDownPosition.Y - (int)straal2, (int)(2 * straal2), (int)(2 * straal2));

                }

            }
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            //if no image is loaded:
            //if ((pictureBox1.Image == null)) return;

            ConvertCoordinates(pictureBox1, out X0, out Y0, e.X, e.Y);
            X_pos_LBL.Text = "X: " + (X0 + (int)upperLeftCorner1.X).ToString();
            Y_pos_LBL.Text = "Y: " + (Y0 + (int)upperLeftCorner1.Y).ToString();


            //if((X0 + (int)upperLeftCorner1.X)<0 || (X0 + (int)upperLeftCorner1.X) > pictureBox1.Image.Width)
            //{ return; }

            // Update the actual size of the selection:
            if (_selecting)
            {
                if (X0 > pictureBox1.Image.Width)
                { _selection.Width = pictureBox1.Image.Width - _selection.X; }
                else { _selection.Width = X0 - _selection.X; }                           //Used for cropping the (already cropped) image
                if (Y0 > pictureBox1.Image.Height)
                { _selection.Height = pictureBox1.Image.Height - _selection.Y; }   //Limit the selection to the lower border of the image.
                else { _selection.Height = Y0 - _selection.Y; }

                _selectionLocalCoords.Width = e.X - _selectionLocalCoords.X;   //Used for drawing the zoom box.
                _selectionLocalCoords.Height = e.Y - _selectionLocalCoords.Y;

                // Redraw the picturebox:
                pictureBox1.Refresh();
            }

            if (drawCircle)
            {
                if (isMoving)
                {
                    mouseMovePosition = e.Location;
                    pictureBox1.Invalidate();
                }

            }
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (SinglePointIsBeingDrawnIm1)
                {
                    ConvertCoordinates(pictureBox1, out X0, out Y0, e.X, e.Y);
                    DrawPointOnImage(pictureBox1, e.X, e.Y);
                    SavePointOnImage(1, (X0 + (int)upperLeftCorner1.X), (Y0 + (int)upperLeftCorner1.Y), true);
                    DrawSuggestionLine(pictureBox2, (int)upperLeftCorner1.Y + Y0);
                }
                if (EllipseIsBeingDrawnIm1)
                {
                    ConvertCoordinates(pictureBox1, out X0, out Y0, e.X, e.Y);
                    DrawPointOnImage(pictureBox1, e.X, e.Y);
                    SavePointOnImage(1, (X0 + (int)upperLeftCorner1.X), (Y0 + (int)upperLeftCorner1.Y), false);
                    int height = CalculateEllipseCenter(1);
                    if (height == -1)
                    { return; }
                    else
                    {
                        DrawSuggestionLine(pictureBox2, height);
                    }
                }
                if (drawCircle)
                {
                    pictureBox1.Cursor = Cursors.Cross;
                    isMoving = true;
                    mouseDownPosition = e.Location;

                }
                else
                { return; }
            }


            // Starting point of the selection:
            if (e.Button == MouseButtons.Right)
            {
                ConvertCoordinates(pictureBox1, out X0, out Y0, e.X, e.Y);
                if (X0 < 0 || X0 > pictureBox1.Image.Width)
                {
                    _selecting = false;
                    return;
                }
                if (Y0 < 0 || Y0 > pictureBox1.Image.Height)
                {
                    _selecting = false;
                    return;
                }

                _selecting = true;

                _selection = new Rectangle(new Point(X0, Y0), new Size());
                _selectionLocalCoords = new Rectangle(new Point(e.X, e.Y), new Size());
            }
        }

        private void _2DMeasurementsWorkpanel_SizeChanged(object sender, EventArgs e)
        {
            panel1.Height = this.Height - 20;
            panel1.Width  = Convert.ToInt32(((double)(this.Height - 20))/2);
            panel1.Update();
            panel2.Height = this.Height - 20;
            panel2.Width = Convert.ToInt32(((double)(this.Height - 20)) / 2);
            panel2.Update();
        }

        private void resetViewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Control control = ((ContextMenuStrip)(((ToolStripMenuItem)sender).Owner)).SourceControl;
            String name = control.Name;

            if (name == "labelIm1")
            {

                pictureBox1.Image = _originalImage1.Clone() as System.Drawing.Image;   
                upperLeftCorner1.X = 0;
                upperLeftCorner1.Y = 0;
            }
            else
            {


                pictureBox2.Image = _originalImage2.Clone() as System.Drawing.Image;   


                upperLeftCorner2.X = 0;
                upperLeftCorner2.Y = 0;
            }
        }

        private void widerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Control control = ((ContextMenuStrip)(((ToolStripMenuItem)sender).Owner)).SourceControl;
            String name = control.Name;
            if (name == "labelIm1")
            {

                panel1.Height = this.Height - 20;
                panel1.Width = panel1.Width + 80;
                panel1.Update();
            }
            else
            {


                panel2.Height = this.Height - 20;
                panel2.Width = panel2.Width + 80;
                panel2.Update();
            }

          
           
        }

        private void narrowerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Control control = ((ContextMenuStrip)(((ToolStripMenuItem)sender).Owner)).SourceControl;
            String name = control.Name;
            if (name == "labelIm1")
            {

                panel1.Height = this.Height - 20;
                panel1.Width = panel1.Width - 80;
                panel1.Update();
            }
            else
            {


                panel2.Height = this.Height - 20;
                panel2.Width = panel2.Width - 80;
                panel2.Update();
            }

        }

        private void resetWidthToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Control control = ((ContextMenuStrip)(((ToolStripMenuItem)sender).Owner)).SourceControl;
            String name = control.Name;
            if (name == "labelIm1")
            {

                panel1.Height = this.Height - 20;
                panel1.Width = Convert.ToInt32( (double)(this.Height - 20)/2);
                panel1.Update();
            }
            else
            {


                panel2.Height = this.Height - 20;
                panel2.Width = Convert.ToInt32((double)(this.Height - 20) / 2);
                panel2.Update();
            }
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {

        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {

        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {

        }

        private void ConvertCoordinates(PictureBox pic, out int X0, out int Y0, int x, int y)
        {
            // Convert the coordinates for the image's SizeMode.
            int pic_hgt = pic.ClientSize.Height;
            int pic_wid = pic.ClientSize.Width;
            int img_hgt = pic.Image.Height;
            int img_wid = pic.Image.Width;

            X0 = x;
            Y0 = y;
            switch (pic.SizeMode)
            {
                case PictureBoxSizeMode.AutoSize:
                case PictureBoxSizeMode.Normal:
                    // These are okay. Leave them alone.
                    break;
                case PictureBoxSizeMode.CenterImage:
                    X0 = x - (pic_wid - img_wid) / 2;
                    Y0 = y - (pic_hgt - img_hgt) / 2;
                    break;
                case PictureBoxSizeMode.StretchImage:
                    X0 = (int)(img_wid * x / (float)pic_wid);
                    Y0 = (int)(img_hgt * y / (float)pic_hgt);
                    break;
                case PictureBoxSizeMode.Zoom:
                    float pic_aspect = pic_wid / (float)pic_hgt;
                    float img_aspect = img_wid / (float)img_hgt;
                    if (pic_aspect > img_aspect)
                    {
                        // The PictureBox is wider/shorter than the image.
                        Y0 = (int)(img_hgt * y / (float)pic_hgt);

                        // The image fills the height of the PictureBox.
                        // Get its width.
                        float scaled_width = img_wid * pic_hgt / img_hgt;
                        float dx = (pic_wid - scaled_width) / 2;
                        X0 = (int)((x - dx) * img_hgt / (float)pic_hgt);
                    }

                    else
                    {
                        // The PictureBox is taller/thinner than the image.
                        X0 = (int)(img_wid * x / (float)pic_wid);

                        // The image fills the height of the PictureBox.
                        // Get its height.
                        float scaled_height = img_hgt * pic_wid / img_wid;
                        float dy = (pic_hgt - scaled_height) / 2;
                        Y0 = (int)((y - dy) * img_wid / pic_wid);
                    }
                    break;
            }
        }


        private int CalculateEllipseCenter(int imagePanel)
        {
            return CalculateEllipseCenter(GetSelectedEllipseMeasurementDetails(), imagePanel);

        }
        private DataTable GetSelectedEllipseMeasurementDetails()
        {
            //return selected measurementDetail(s) that 
            DataTable dt = new DataTable();
            //Check if rows selected
            int RowCount = UC_measurementsMain.dgViewMeasurements.Rows.GetRowCount(DataGridViewElementStates.Selected);

            if (RowCount == 0)
                return dt;

            if (RowCount > 1)
            {
                MessageBox.Show("More than one measurement selected.", "Select only one", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return dt;
            }

            int i = 0;

            //Get row of clicked cell
            DataGridViewRow row = UC_measurementsMain.dgViewMeasurements.Rows[UC_measurementsMain.dgViewMeasurements.SelectedRows[i].Index];
            //Get column AcquisitionNumber
            DataGridViewCell cell = row.Cells["MeasurementID"];

            //Set Database
            DataBase SQLDB = new DataBase(AppData.SQLServer, AppData.SQLDatabase, AppData.SQLAuthSQL, AppData.SQLUser, AppData.SQLPassword);

            Measurement SelectedMeasurement = new Measurement(SQLDB, Subject, EOS.AcquisitionNumber, Convert.ToInt32(cell.Value), AppData);

            //Build SQL Command (=SQL select statement + Parameters)
            SqlCommand SQLcmd = new SqlCommand();

            string SQLselect = "SELECT PixelX, PixelY, ImagePanel FROM MeasurementDetail where MeasurementID = @MeasurementID and IsSinglePoint = @IsSinglePoint";
            SQLcmd.Parameters.AddWithValue("@MeasurementID", SelectedMeasurement.MeasurementID);
            SQLcmd.Parameters.AddWithValue("@IsSinglePoint", "false");

            SQLcmd.CommandText = SQLselect;

            //Fill a dataset with selected records
            DataSet ds = new DataSet();

            SQLDB.ReadDataSet(SQLcmd, ref ds);

            dt = ds.Tables[0];
            return dt;

        }


        private int CalculateEllipseCenter(DataTable dt, int ImagePanel)
        {
            List<FitEllipse.Ellipse_Point> points = new List<FitEllipse.Ellipse_Point>();
            // FitEllipse.PointCollection points = new FitEllipse.PointCollection();

            List<int> Xvalues = new List<int>();
            List<int> Yvalues = new List<int>();

            if (dt.Rows.Count == 0)     //If there are no ellipse points.
            { return -1; }

            foreach (DataRow dr in dt.Rows)
            {
                if ((int)dr[2] == ImagePanel)    //this is the indication for the ImagePanel
                {
                    int p1X = 0;
                    int p1Y = 0;

                    p1X = (int)dr[0];       //PixelX
                    p1Y = (int)dr[1];       //PixelY

                    if (Xvalues.Contains(p1X) || Yvalues.Contains(p1Y) || Xvalues.Contains(p1X + 1) || Yvalues.Contains(p1Y + 1) || Xvalues.Contains(p1X - 1) || Yvalues.Contains(p1Y - 1))
                    {
                    }
                    else
                    {
                        Xvalues.Add(p1X);
                        Yvalues.Add(p1Y);

                        points.Add(new FitEllipse.Ellipse_Point() { X = p1X, Y = p1Y });
                    }
                }

            }
            if (points.Count < 5)
            { return -1; }

            FitEllipse.EllipseFit fit = new FitEllipse.EllipseFit();
            Meta.Numerics.Matrices.Matrix ellipse = fit.Fit(points);
            double a = ellipse[0, 0];
            double b = ellipse[1, 0];
            double c = ellipse[2, 0];
            double d = ellipse[3, 0];
            double e2 = ellipse[4, 0];
            double f = ellipse[5, 0];
            double h = (2 * c * d - e2 * b) / (b * b - 4 * a * c);
            double k = (-2 * a * h - d) / (b);

            //DrawPointOnImage(pictureBox1, e.X, e.Y);

            SavePointOnImage(ImagePanel, (int)h, (int)k, true);
            return (int)k;

            // btnView.PerformClick();
        }

        public void EquilizeHistograms()
        {
            Bitmap masterImage1 = (Bitmap)pictureBox1.Image;
            Emgu.CV.Image<Emgu.CV.Structure.Bgr, byte> MyEMGUImageBGR1 = new Emgu.CV.Image<Emgu.CV.Structure.Bgr, byte>(masterImage1);
            Emgu.CV.Image<Gray, Byte> MyEMGUImage1 = MyEMGUImageBGR1.Convert<Gray, Byte>();


            MyEMGUImage1._EqualizeHist();
            pictureBox1.Image = MyEMGUImage1.ToBitmap();

            Bitmap masterImage2 = (Bitmap)pictureBox2.Image;
            Emgu.CV.Image<Emgu.CV.Structure.Bgr, byte> MyEMGUImageBGR2 = new Emgu.CV.Image<Emgu.CV.Structure.Bgr, byte>(masterImage2);
            Emgu.CV.Image<Gray, Byte> MyEMGUImage2 = MyEMGUImageBGR2.Convert<Gray, Byte>();

            MyEMGUImage2._EqualizeHist();
            pictureBox2.Image = MyEMGUImage2.ToBitmap();

        }

    }
}

// Ensure the AppData class has the following properties defined:
public class AppData
{
    // Legacy SQL properties (can be empty/null for file-based approach)
    public string SQLAuthSQL { get; set; }
    public string SQLDatabase { get; set; }
    public string SQLPassword { get; set; }
    public string SQLServer { get; set; }
    public string SQLUser { get; set; }

    // File-based properties
    public string TempDir { get; set; } = Path.GetTempPath();
    public string DataDirectory { get; set; } = "Data";
    public string ConfigFile { get; set; } = "config.json";
}
