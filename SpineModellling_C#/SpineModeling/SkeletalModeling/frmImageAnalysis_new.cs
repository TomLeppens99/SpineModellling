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
//using itk.simple;
using org.dicomcs;
using org.dicomcs.data;
using org.dicomcs.dict;
using org.dicomcs.net;
using org.dicomcs.scp;
using org.dicomcs.server;
using org.dicomcs.util;
using DicomImageViewer;
//using gfoidl.Imaging;
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
    public partial class frmImageAnalysis_new : Form
    {
        #region Declarations
        //Settings - need to be set before showing the form !

        public SimModelVisualization SimModelVisualization = new SimModelVisualization();
        public SkeletalModeling._2DMeasurementsWorkpanel _2DMeasurementsWorkpanel = new SkeletalModeling._2DMeasurementsWorkpanel();
        public SkeletalModeling.UC_3DModelingWorkpanel uC_3DModelingWorkpanel = new SkeletalModeling.UC_3DModelingWorkpanel();
        public frmSkeletalModelingPreferences frmSkeletalModelingPreferences = new frmSkeletalModelingPreferences();


        List<TreeNode> checkedNodes = new List<TreeNode>();
        private ContextMenuStrip docMenu;
        vtkPropPicker propPicker = vtkPropPicker.New();
        private int previousPositionX = 0;
        private int previousPositionY = 0;
        private int numberOfClicks = 0;
        private int resetPixelDistance = 5;
        private vtkAssembly lastPickedAssembly = vtkAssembly.New();
        public OsimBodyProperty selectedBodyProperty = new OsimBodyProperty();
        private bool switchIm1 = false;
        private bool switchIm2 = false;

        public AppData AppData;
        public DataBase SQLDB;
        public string ID;
        public Subject Subject;
        private MeasurementDetail MeasurementDetail;
        public Measurement Measurement;
        public DICOMImage dcmImage1;
        public DICOMImage dcmImage2;
        public ToolTip tooltip = new ToolTip();
        public bool drawCircle = false;
        private bool isMoving = false;
        private Point mouseDownPosition = Point.Empty;
        private Point mouseMovePosition = Point.Empty;
        private Dictionary<Point, Point> Circles = new Dictionary<Point, Point>();

        public DataSet fullDsMeas = new DataSet();

        public Position upperLeftCorner1 = new Position(0, 0, 0);   //Are used to track the zoom. 
        public Position upperLeftCorner2 = new Position(0, 0, 0);

        //public int p1X;
        //public int p1Y;
        //public int p2X;
        //public int p2Y;

        Boolean SinglePointIsBeingDrawnIm1 = false;
        Boolean EllipseIsBeingDrawnIm1 = false;

        Boolean SinglePointIsBeingDrawnIm2 = false;
        Boolean EllipseIsBeingDrawnIm2 = false;
       


       
        public int MeasurementID = 0;

        DicomDecoder dd1;
        DicomDecoder dd2;
        List<byte> pixels8;
        List<ushort> pixels16;
        List<byte> pixels24;
        int imageWidth;
        int imageHeight;
        int bitDepth;
        int samplesPerPixel;
        //bool imageOpened;
        double winCentre;
        double winWidth;
        bool signedImage;
        int maxPixelValue;
        int minPixelValue;


        public EosImage EosImage1;
        public EosImage EosImage2;
        public EosSpace EosSpace;



        //Emgu.CV.Image<Bgr, Byte> My_Image1;
     
       // private Bitmap OriginalImage1 = null;
        private Rectangle _selection;
        private Rectangle _selectionLocalCoords;
        private int X0, Y0, X1, Y1, X2, Y2;
        private bool _selecting;

      
        private Acquisitions.EOS EOS;
        private Acquisitions.Model Model;


        vtkBoxWidget boxWidget = new vtkBoxWidget();

        #endregion

        #region General
        public frmImageAnalysis_new()
        {
            InitializeComponent();
        }


        private void DoubleBufferDGV(DataGridView dgview)
        {
            Type dgvType = dgview.GetType();
            PropertyInfo pi = dgvType.GetProperty("DoubleBuffered",
                        BindingFlags.Instance | BindingFlags.NonPublic);
            pi.SetValue(dgview, true, null);
        }



        private void frmImageAnalysis_Load(object sender, EventArgs e)
        {

            try
            {
                System.IO.Directory.CreateDirectory(AppData.TempDir);
            }
            catch
            {
                MessageBox.Show("Directory could not be created", "Application Settings ERROR", MessageBoxButtons.OK);
                return;
            }
          
        

            //Set Database
            SQLDB = new DataBase(AppData.SQLServer, AppData.SQLDatabase, AppData.SQLAuthSQL, AppData.SQLUser, AppData.SQLPassword);
            //Define Patient Object
           // Subject = new Subject(SQLDB, ID, AppData);
            SimModelVisualization.Subject = Subject;
            SimModelVisualization.appData = AppData;


            AddToLogsAndMessages("Application started. Ready for input.");

          
            btnRefresh.Enabled = false;
            InitAndhide3DVTKstuff();

          


        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void btnReturn_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void saveMarkersToFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
           
        }
        #endregion

        #region VtkRender Methods

        private void RenderAll()
        {
      
           // uC_3DModelingWorkpanel.ren1.Render();
            //uC_3DModelingWorkpanel.ren1Image1.Render();
            uC_3DModelingWorkpanel.ren1Image1.GetRenderWindow().Render();
            //uC_3DModelingWorkpanel.ren1Image2.Render();
            uC_3DModelingWorkpanel.ren1Image1.GetRenderWindow().Render();
            uC_3DModelingWorkpanel.RenderWindow.Render();
            uC_3DModelingWorkpanel.renderWindowControlImage1.RenderWindow.Render();
            uC_3DModelingWorkpanel.renderWindowControlImage2.RenderWindow.Render();
        }


     
        private void DisplayEOSin2Dspace(EosImage EosImage, EosSpace EosSpace, vtkRenderer ren, vtkImageActor vtkImgageActor, int ImageNb)
        {
            vtkImgageActor.SetInput(EosImage.PNGReader.GetOutput());
            if (ImageNb == 1)
            {
                vtkImgageActor.SetOrientation(EosSpace.OrientationImage1.X, EosSpace.OrientationImage1.Y, EosSpace.OrientationImage1.Z);
                vtkImgageActor.SetPosition(EosSpace.PositionOriginImage1.X, EosSpace.PositionOriginImage1.Y, 0.10);
                //This is for image1
                vtkCamera activecam = new vtkCamera();
                activecam = ren.GetActiveCamera();
                activecam.SetFocalPoint(0, 1, 0);     //TODO: The Height (Y) should change when moving the image up and down.
                activecam.SetPosition(0, 1, EosSpace.PositionSource1.Z);
                //activecam.SetFocalPoint(0, 1, 0.4);
                //activecam.SetPosition(0, 1, -0.4);
                activecam.ComputeViewPlaneNormal();
                activecam.SetViewUp(0, 1, 0);
                activecam.OrthogonalizeViewUp();
                activecam.ParallelProjectionOn();
                activecam.Zoom(0.8); //0.6
                ren.SetActiveCamera(activecam);
            }
            if (ImageNb == 2)
            { 
            vtkImgageActor.SetOrientation(EosSpace.OrientationImage2.X, EosSpace.OrientationImage2.Y, EosSpace.OrientationImage2.Z);
            vtkImgageActor.SetPosition(0.10, EosSpace.PositionOriginImage2.Y, EosSpace.PositionOriginImage2.Z);
               // This is for image2
            vtkCamera activecam = new vtkCamera();
                activecam = ren.GetActiveCamera();

                activecam.SetFocalPoint(0, 1, 0);
                activecam.SetPosition(EosSpace.PositionSource2.X, 1, 0);

                //activecam.SetFocalPoint(0.4, 1, 0);
                //activecam.SetPosition(-0.4, 1, 0);
                activecam.ComputeViewPlaneNormal();
                activecam.SetViewUp(0, 1, 0);
                activecam.OrthogonalizeViewUp();
                activecam.ParallelProjectionOn();
                activecam.Zoom(0.8);
                ren.SetActiveCamera(activecam);
            }

            vtkImgageActor.PickableOff();


            ren.AddActor(vtkImgageActor);



        }
      

       
        #endregion

        #region Picturebox Methods
       
      
      
        private void ReadEOSImage(EosImage EosImage, DicomDecoder dd)
        {

            EosImage.ReadImage();

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

 
        private void ReturnEllipse()
        {
   
           FitEllipse.ProgramEllipse ProgramEllipse = new FitEllipse.ProgramEllipse();
            FitEllipse.Ellipse_Point ellipseCenter = new FitEllipse.Ellipse_Point();
            ellipseCenter = ProgramEllipse.FitTheEllipse();
        }
        
      
        #endregion
    
     
      
      

        #region Measurement Database Methods

      
       

       

        private void label1_Click(object sender, EventArgs e)
        {

        }

       

        private void txtNameMeasurement_TextChanged(object sender, EventArgs e)
        {
           
        }

        private void txtCommentMeasurement_TextChanged(object sender, EventArgs e)
        {
            
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
          
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }



        private void BtnLoadFPdata_Click(object sender, EventArgs e)
        {

        }

      

      
        #endregion

        #region Measurement Drawing Methods
        
   

      

       
       


     

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

        private void btnView_Click(object sender, EventArgs e)
        {



        }

       

      
        public static void BeginInteraction(vtkObject sender, vtkObjectEventArgs e)
        {
           
        }

        //class  vtkBoxCallBack : vtkCommand
        //{
        //    //public static vtkBoxCallBack New2()
        //    //{
        //    //    return vtkBoxCallBack2();
        //    //}

        //   public vtkActor m_actor;

        //    public void SetActor(vtkActor actor)
        //    {
        //        m_actor = actor;
        //    }

        //    public void Execute(vtkObject sender, vtkObjectEventArgs e)
        //    {
        //        vtkBoxWidget2 vtkBoxWidget2 = new vtkBoxWidget2();
        //        vtkBoxWidget2.SafeDownCast(sender);



        //        vtkTransform t = vtkTransform.New();

        //        vtkBoxRepresentation.SafeDownCast(vtkBoxWidget2.GetRepresentation()).GetTransform(t);
        //        this.m_actor.SetUserTransform(t);

        //    }
        //    public vtkBoxCallBack vtkBoxCallBack2()
        //    {
        //        return new vtkBoxCallBack();
        //    }
        //};

        private void btnLoadMSM_Click(object sender, EventArgs e)
        {


        }
       

      
      
     
        public void ExecuteHandle(vtkObject sender, vtkObjectEventArgs e)
        {
            //uncomment below again (2lines)
            //vtkTransform t = new vtkTransform();
            //boxWidget.GetTransform(t);



            // this.selectedBodyProperty.assembly.SetUserTransform(t);

            //vtkTransform currentTransform = t;

            //currentTransform.PreMultiply();

            //currentTransform.Concatenate(selectedBodyProperty.transform);

            // selectedBodyProperty.transform = currentTransform;



            ///Below has been commented
            ///
            //vtkTransform temp = new vtkTransform();
            //temp.DeepCopy((vtkTransform)selectedBodyProperty.transform); //.GetUserTransform();  //(currentTransform);
            //temp.Concatenate(t);

            //selectedBodyProperty.assembly.SetUserTransform(temp);
            
            ///UP
            //------------------


            //this.selectedBodyProperty.transform;
            //currentTransform.Translate(t.GetPosition()[0], t.GetPosition()[1], t.GetPosition()[2]);

            //currentTransform.PostMultiply();

            //currentTransform.RotateX(t.GetOrientation()[0]);
            //currentTransform.RotateY(t.GetOrientation()[1]);
            //currentTransform.RotateZ(t.GetOrientation()[2]);

            //currentTransform.PostMultiply();


            //currentTransform.Scale(t.GetScale()[0], t.GetScale()[1], t.GetScale()[2]);


        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

       

        private void splitContainer1_Panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void Y_pos_LBL2_Click_1(object sender, EventArgs e)
        {

        }

        private void statusStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void lblMessages_Click(object sender, EventArgs e)
        {

        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            // set the current caret position to the end
          
        }

        private void tbRenderer_Click(object sender, EventArgs e)
        {

        }

        private void btnTest_Click(object sender, EventArgs e)
        {

            
        }

        private void numericUpDownIm2_ValueChanged(object sender, EventArgs e)
        {
           

        }

        private void numericUpDownIm1_ValueChanged(object sender, EventArgs e)
        {
           
        }

        private void propertyGrid1_Click(object sender, EventArgs e)
        {

        }

        private void propertyGrid1_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
           // e.ChangedItem.
        }

      
        //private void btnEllipseFit_Click(object sender, EventArgs e)
        //{
        //    btnConfirm.BackgroundImage = Properties.Resources.imgSave;
        //    btnConfirm.Enabled = true;
        //    EllipseIsBeingDrawn = true;
        //}

        #endregion

        #region Projection Methods

        
        public double ConvertPixelToMeters(int pixelvalue, double pixelspacing)
        {

            return pixelvalue * pixelspacing;

        }

        public int ConvertMetersToPixels(double meters, double pixelspacing)
        {
            double temp = meters / pixelspacing ;
            return (int)Math.Round(temp);

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




        #endregion

        #region Toolstrip

        private void saveModelToolStripMenuItem_Click(object sender, EventArgs e)
        {
          
            
          if (AppData.localStudyUser._CanDownload)
            {


                DialogResult dr = MessageBox.Show("Do you want to download the created model to your computer?", "Local download confirmation", MessageBoxButtons.YesNo,
    MessageBoxIcon.Information);

                if (dr == DialogResult.Yes)
                {
                string sourcePath;
                string fileName;
                if (vistaFolderBrowserDialog1.ShowDialog(this) == DialogResult.OK)
                {
                    sourcePath = vistaFolderBrowserDialog1.SelectedPath;
                }
                else { return; }

                Cursor.Current = Cursors.WaitCursor;
                    string loc = sourcePath + "\\SWS_exportedModel_" + Subject.SubjectCode.ToString() + ".osim";
                    SimModelVisualization.PrintModel(loc);
                    AddToLogsAndMessages("The model was saved to your local computer." + " (" + loc + ")");
                   
                    Application.UseWaitCursor = false;
                }
            }
        }

        private string LogsAndMessages = string.Empty;

        public void AddToLogsAndMessages(string text)
        {
            LogsAndMessages += Environment.NewLine + DateTime.Now.ToLongTimeString() + ": " + '\t' + text;

        }

        private void saveModelToDatabaseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FileManagement FileManagement = new FileManagement();

            if (AppData.globalUser._UserID.ToLower() != Model.UserName.ToLower())
            {
                DialogResult drs = MessageBox.Show("Warning: You are not the owner of this model (Owned by "+ Model.UserName + "). Please be sure you have discussed overwriting with the owner as you have been given overwrite priviliges. Continue?", "Overwrite warning cloud confirmation", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Information);
                if (drs != DialogResult.Yes)
                {
                    return;
                }
             }
            //if (AppData.globalUser._UserID.ToLower() == "toverb0")
            //{
            // DialogResult dr = MessageBox.Show("Click Yes to overwrite the model in the database. Click No to save this model as a new version.", "Overwrite warning cloud confirmation", MessageBoxButtons.YesNoCancel,MessageBoxIcon.Information);
            DialogResult dr = MessageBox.Show("This will overwrite the existing model in the databse. Continue?", "Overwrite warning cloud confirmation", MessageBoxButtons.YesNoCancel,MessageBoxIcon.Information);

                if (dr == DialogResult.Yes)
                {

                    Cursor.Current = Cursors.WaitCursor;
                    //Add entry in Database
                    SQLDB = new DataBase(AppData.SQLServer, AppData.SQLDatabase, AppData.SQLAuthSQL, AppData.SQLUser, AppData.SQLPassword);
                    //Define Model Object
                    if (Model == null)
                    {
                        MessageBox.Show("Undefined Model error!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    Acquisitions.Model ModelDBnew = new Acquisitions.Model(SQLDB, Subject, Model.AcquisitionNumber);
                    if (ModelDBnew.DateAdded != ModelDBnew.DateSaved)
                    {
                        ModelDBnew.Comments += Environment.NewLine+ "Overwritten by " + AppData.globalUser.FirstName+ " "+ AppData.globalUser.LastName+" ("+ AppData.globalUser._UserID.ToLower()+")  on "+ DateTime.Now.ToString();

                    }
                    ModelDBnew.AppData = AppData;
                    ModelDBnew.Save();  

                    string loc = Path.GetFullPath(AppData.AcquisitionDir) + ModelDBnew.Directory;
                    //Make the file and Save the file in the correct folder
                    SimModelVisualization.osimModel.printToXML(loc);

                    AddToLogsAndMessages("The model was saved in the SWS database.");

                    Application.UseWaitCursor = false;
                }
                //if(dr == DialogResult.No)
                //{
                //    Cursor.Current = Cursors.WaitCursor;
                //    //Add entry in Database
                //    SQLDB = new DataBase(AppData.SQLServer, AppData.SQLDatabase, AppData.SQLAuthSQL, AppData.SQLUser, AppData.SQLPassword);
                //    //Define Model Object
                //    if (Model == null)
                //    {
                //        MessageBox.Show("Undefined Model error!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //        return;
                //    }
                //    Acquisitions.Model ModelDBnew = new Acquisitions.Model(SQLDB, Subject, 0);
                //    if (ModelDBnew.DateAdded == ModelDBnew.DateSaved)
                //    {
                //        ModelDBnew.Comments = Model.Comments + " Saved version";

                //    }
                //    ModelDBnew.AppData = AppData;
                //    ModelDBnew.Save();
                //    strDirectory = FileManagement.GenerateTargetPath(AppData.AcquisitionDir, Model.Subject.SWS_volgnummer.ToString(), Model.DataType);
                //    string loc = Path.GetFullPath(AppData.AcquisitionDir) + ModelDBnew.Directory;
                //    //Make the file and Save the file in the correct folder
                //    SimModelVisualization.osimModel.printToXML(loc);

                //    AddToLogsAndMessages("The model was saved in the SWS database.");

                //    Application.UseWaitCursor = false;

                //}
           // }
            //else
            //{
            //    MessageBox.Show("You don't have permission to overwrite a model", "Unauthorized", MessageBoxButtons.OK, MessageBoxIcon.Information);

            //    return;
            //}
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            //before your loop
            var csv = new StringBuilder();

            //in your loop
            var first = "test";
            var second = "blablabla";            //Suggestion made by KyleMit
            var newLine = string.Format("{0}    {1}", first, second);
            csv.AppendLine(newLine);

            //after your loop
            File.WriteAllText(@"C:\Users\Thomas\Desktop\Nieuw tekstdocument.csv", csv.ToString());



            //   double m1X = -0.142683266500000; // ConvertPixelToMeters((EosImage1.Columns / 2) - p1X, EosImage1.PixelSpacingX);  //THIS NEEDS TO BE INVERTED COMPARED TO THE MATLAB CODE !!!!!!
            // //  double m1Y = ConvertPixelToMeters(EosImage1.Rows - p1Y, EosImage1.PixelSpacingY);   //Because Y coordinate is measured top down in EOS images.
            //   double m2X = -0.152548231500000; //ConvertPixelToMeters((EosImage2.Columns / 2) - p2X, EosImage2.PixelSpacingX);
            ////   double m2Y = ConvertPixelToMeters(EosImage2.Rows - p2Y, EosImage2.PixelSpacingY);

            //   double xP;
            //   double zP;

            //   InverseProject(m1X, m2X, out xP, out zP);


        }

        private void cBMuscles_CheckedChanged(object sender, EventArgs e)
        {
           
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            OpenSim.Model osimTestModel1 = new OpenSim.Model(@"C:\ThomasOverbergh\Bended Radiograph Project\EY\Bended\Bended_MarkersNotChanged.osim");
            OpenSim.Model osimTestModel2 = new OpenSim.Model(@"C:\ThomasOverbergh\Bended Radiograph Project\EY\Bended\Bended_WithPersonalizedMarkers.osim");


            Marker marker1 = osimTestModel1.getMarkerSet().get(3);
            string nameMarker1 = marker1.getName();
            Marker marker2 = osimTestModel2.getMarkerSet().get(nameMarker1);

            
            marker2.getName();

            Vec3 offset1 = marker1.getOffset();
            Vec3 offset2 = marker2.getOffset();


            double deltaX = offset1.get(0) - offset2.get(0);
            double deltaY = offset1.get(1) - offset2.get(1);
            double deltaZ = offset1.get(2) - offset2.get(2);

            double distance = (double)Math.Sqrt(deltaX * deltaX + deltaY * deltaY + deltaZ * deltaZ);

            


        }

        private void tabPage4_Click(object sender, EventArgs e)
        {

        }

        private void btnTestDLT_Click(object sender, EventArgs e)
        {

            foreach (OsimBodyProperty Bprop in SimModelVisualization.bodyPropertyList)
            {
                //OsimBodyProperty Bprop = selectedBodyProperty;


                foreach (OsimGeometryProperty geomProp in Bprop._OsimGeometryPropertyList)
                {
                    geomProp.DLTpolydataHasBeenMade = true;

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
                    //geomProp._vtkPolyDataDLT.Update();
                    //geomProp._vtkPolyDataDLT.Modified();

                    //geomProp._vtkPolyData = VtkPolyDataAbsoluteRefS;
                    //geomProp._vtkPolyData.Update();
                    //geomProp._vtkPolyData.Modified();

                    //vtkPolyData vtkPolyData2 = new vtkPolyData();
                    //vtkPolyData2 = VtkPolyDataAbsoluteRefS;




                    //vtkAppendPolyData vtkAppendPolyData2 = new vtkAppendPolyData();
                    ////vtkAppendPolyData2.AddInputConnection(vtkTransformFilter2.GetOutputPort());
                    //vtkAppendPolyData2.SetInput(vtkPolyData2);
                    ////vtkAppendPolyData2.AddInputConnection(axes.GetOutputPort());    //COMMENT THIS IF YOU WANT TO HIDE THE AXES

                    //Bprop.assemblyOpace1.RemovePart(geomProp._vtkActor1);
                    //Bprop.assemblyOpace2.RemovePart(geomProp._vtkActor2);

                    //vtkPolyDataMapper vtkPolyDataMapper = vtkPolyDataMapper.New();
                    //vtkPolyDataMapper.SetInputConnection(vtkAppendPolyData2.GetOutputPort());

                    //geomProp._vtkActor1.SetMapper(vtkPolyDataMapper);
                    //geomProp._vtkActor1.GetMapper().Update();

                    //geomProp._vtkActor2.SetMapper(vtkPolyDataMapper);
                    //geomProp._vtkActor1.GetMapper().Update();

                    //Bprop.assemblyOpace1.AddPart(geomProp._vtkActor1);
                    //Bprop.assemblyOpace2.AddPart(geomProp._vtkActor2);



                    //geomProp._vtkActor1.GetMapper().SetInputConnection(vtkAppendPolyData2.GetOutputPort());
                    //geomProp._vtkActor1.GetMapper().Update();
                    //geomProp._vtkActor2.GetMapper().SetInputConnection(vtkAppendPolyData2.GetOutputPort());
                    //geomProp._vtkActor2.GetMapper().Update();





                    // Bprop.assemblyOpace1.GetParts() RemovePart()
                    //Bprop.assemblyOpace1.AddPart(Bprop.vtkActorListOpace1[j]);
                    //Bprop.assemblyOpace2.AddPart(Bprop.vtkActorListOpace2[j]);
                    //--------

                    //vtkXMLPolyDataWriter vtkXMLPolyDataWriter = new vtkXMLPolyDataWriter();
                    //vtkXMLPolyDataWriter.SetInput(VtkPolyDataAbsoluteRefS);
                    //vtkXMLPolyDataWriter.SetFileName(@"C:\Users\Thomas\Desktop\test.vtp");
                    //vtkXMLPolyDataWriter.Write();
                    //vtkXMLPolyDataReader reader = new vtkXMLPolyDataReader();

                    //Bprop._vtkPolyDataReaderListDeformed[j].SetFileName(@"C:\OpenSim 3.3\GeometrySpine\squareTile.vtp"); //  VtkPolyDataAbsoluteRefS;
                    //Bprop._vtkPolyDataReaderListDeformed[j].Update();

                    //Bprop._vtkPolyDataReaderListDeformed[j].SetFileName(@"C:\Users\Thomas\Desktop\test.vtp"); //  VtkPolyDataAbsoluteRefS;
                    //Bprop._vtkPolyDataReaderListDeformed[j].Update();

                }
                SimModelVisualization.UpdateOpaceAssembly(Bprop);

            }
            RenderAll();

        }

        private void dgViewMeasurements_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void btnMeasurments_Click(object sender, EventArgs e)
        {
           
        }

        private void button3_Click_1(object sender, EventArgs e)
        {
           
        
    }

        private void btnCalcCenter_Click(object sender, EventArgs e)
        {

        
            
        }


        private void btnDrawCircle_Click(object sender, EventArgs e)
        {
           
        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void txtMovePrecision_TextChanged(object sender, EventArgs e)
        {

        }

        private void geometryToolToolStripMenuItem_Click(object sender, EventArgs e)
        {
           
        }

        private void saveMarkersToFileToolStripMenuItem_Click_1(object sender, EventArgs e)
        {

        }

        private void perturbateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveModelToDatabaseToolStripMenuItem.Visible = false; //to prevent the changes of the perturbator to be saved to the model in the database.

            Simulator.frmPerturbator frmPerturbator = new Simulator.frmPerturbator();

            //Pass data to search form
            frmPerturbator.AppData = this.AppData;
            frmPerturbator.Subject = this.Subject;
            frmPerturbator.SQLDB = SQLDB;
            frmPerturbator.model = SimModelVisualization.osimModel;
            frmPerturbator.SimModelVisualization = SimModelVisualization;
            frmPerturbator.si = SimModelVisualization.si;
            //frmPerturbator.currentBodyProperty = selectedBodyProperty;
            frmPerturbator.jointPropertyList = SimModelVisualization.jointPropertyList;
            frmPerturbator.markerPropertyList = SimModelVisualization.markerPropertyList;
            frmPerturbator.bodyPropertyList = SimModelVisualization.bodyPropertyList;
            frmPerturbator.ShowDialog();

        }

        private void txtAccAngle_TextChanged(object sender, EventArgs e)
        {

        }

        private void logsAndMessagesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmLogsAndMessages frmLogsAndMessages = new frmLogsAndMessages();
            frmLogsAndMessages.LogsAndMessages = this.LogsAndMessages;
            frmLogsAndMessages.ShowDialog();
        }

        private void splitContainer1_SplitterMoved(object sender, SplitterEventArgs e)
        {

        }

        private void preferencesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmSkeletalModelingPreferences.AppData = AppData;
            frmSkeletalModelingPreferences.ShowDialog();

            foreach( string dir in frmSkeletalModelingPreferences.GeoemtryDirs)
            {
                AddToLogsAndMessages(dir);
            }
            SimModelVisualization.GeoemtryDirs = frmSkeletalModelingPreferences.GeoemtryDirs;
        }

      

        private void btn2Dview_Click(object sender, EventArgs e)
        {
            uC_3DModelingWorkpanel.Visible = false;
            _2DMeasurementsWorkpanel.Visible = true;
            btn3Dview.Visible = true ;
            btn2Dview.Visible = false;
        }

        private void btn3Dview_Click(object sender, EventArgs e)
        {

            _2DMeasurementsWorkpanel.Visible = false;
            uC_3DModelingWorkpanel.Visible = true;
            btn3Dview.Visible = false;
            btn2Dview.Visible = true;
        }

        private void measurementsToExcelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _2DMeasurementsWorkpanel.UC_measurementsMain.ExportToExcel();
        }

        private void markersTotrcToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string loc = AppData.TempDir + "\\markerFileTest.xml";
            SimModelVisualization.osimModel.getMarkerSet().printToXML(loc);
            MessageBox.Show("Marker File has been written to '" + loc + "'", "Markers Exported", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void geometriesUsedInTheCurrentModelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult dr = MessageBox.Show("Do you want to download all geometries used in this model to your computer?", "Local download confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Information);


            string loc = AppData.TempDir + "\\" + Subject.SWS_volgnummer.ToString() + "_Model_" + Model.AcquisitionNumber + "_GeometryFolder";
            string loc2 = loc;

            Cursor.Current = Cursors.WaitCursor;

            if (dr == DialogResult.Yes)
            {
                AddToLogsAndMessages("Starting export of the geometry folder...");


                int i = 0;
                while (System.IO.Directory.Exists(loc2))
                {
                    i++;
                    loc2 = loc + "_" + i.ToString();
                    AddToLogsAndMessages("Folder with same name already exists. Trying something else...(" + i.ToString() + ")");

                }
                loc = loc2;
                System.IO.Directory.CreateDirectory(loc);




                foreach (OsimBodyProperty bodyProp in SimModelVisualization.bodyPropertyList)
                {
                    foreach (OsimGeometryProperty geomProp in bodyProp._OsimGeometryPropertyList)
                    {
                        FileManagement fileManagement = new FileManagement();
                        fileManagement.CopyFile(geomProp.geometryDirAndFile, loc);
                    }
                }


                AddToLogsAndMessages("The Geometry Folder was saved to your local computer." + " (" + loc + ")");
                MessageBox.Show("The Geometry Folder was saved to your local computer." + "(" + loc + ")", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            Application.UseWaitCursor = false;
        }

        private void geometryToolToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            frmGeometryTool frmGeometryTool = new frmGeometryTool();

            //Pass data to search form
            frmGeometryTool.AppData = this.AppData;
            frmGeometryTool.Subject = this.Subject;
            frmGeometryTool.SQLDB = SQLDB;
            frmGeometryTool.Subject = Subject;

            frmGeometryTool.ShowDialog();
        }

        private void InitAndhide3DVTKstuff()
        {
           
            uC_3DModelingWorkpanel.AppData = AppData;
            uC_3DModelingWorkpanel.SQLDB = SQLDB;
            uC_3DModelingWorkpanel.Subject = Subject;
            uC_3DModelingWorkpanel.Dock = DockStyle.Fill;
            splitContainer1.Panel2.Controls.Add(uC_3DModelingWorkpanel);

            uC_3DModelingWorkpanel.Visible = false;
        }

        private void importModelToolStripMenuItem_Click(object sender, EventArgs e)
        {

          

            CheckPanelsLoading();
        }

        private void renderbackgroundWhiteToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void loadModelFromSWSToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmSkeletalModelingPreferences.AppData = AppData;
            frmSkeletalModelingPreferences.Justread();
            SimModelVisualization.GeoemtryDirs = frmSkeletalModelingPreferences.GeoemtryDirs;


            using (Acquisitions.frmSelectModel frmSelectModel = new Acquisitions.frmSelectModel())
            {
                //Pass data to search form
                frmSelectModel.AppData = this.AppData;
                frmSelectModel.SQLDB = SQLDB;
                frmSelectModel.Subject = Subject;
                frmSelectModel.ShowDialog();
                Model = frmSelectModel.Model;  //Returning the Object: Model

                if (Model == null)
                { return; }
            }
            Application.UseWaitCursor = true;

            uC_3DModelingWorkpanel.Visible = true;

            SimModelVisualization.modelFile = Path.Combine(AppData.DataDirectory, Model.Directory);


                if (!System.IO.File.Exists(SimModelVisualization.modelFile))
                {
                    MessageBox.Show("Path/File can not be found. Check connection to network database.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    return;
                }


                uC_3DModelingWorkpanel.SimModelVisualization = SimModelVisualization;
                SimModelVisualization.printMuscles = loadMusclesToolStripMenuItem.Checked;

                uC_3DModelingWorkpanel.AppData = AppData;
                uC_3DModelingWorkpanel.SQLDB = SQLDB;
                uC_3DModelingWorkpanel.Subject = Subject;
                uC_3DModelingWorkpanel.EosImage1 = EosImage1;
                uC_3DModelingWorkpanel.EosImage2 = EosImage2;
                uC_3DModelingWorkpanel.EosSpace = EosSpace;
                uC_3DModelingWorkpanel.Dock = DockStyle.Fill;
                splitContainer1.Panel2.Controls.Add(uC_3DModelingWorkpanel);
                _2DMeasurementsWorkpanel.Visible = false;
                uC_3DModelingWorkpanel.Visible = true;



            uC_3DModelingWorkpanel.LoadMSModel();

            geometriesUsedInTheCurrentModelToolStripMenuItem.Enabled = true;

            loadModelFromSWSToolStripMenuItem.Enabled = false;
            importModelToolStripMenuItem.Enabled = false;
            modelToolStripMenuItem.Enabled = true;

            Application.UseWaitCursor = false;
            loadMusclesToolStripMenuItem.Enabled = false;
            CheckPanelsLoading();
        }

        private void loadGravityLineToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(EOS!=null)
            {
            uC_3DModelingWorkpanel.EOS = this.EOS;
            uC_3DModelingWorkpanel.LoadGravityLineData();
            }

        }

        private void superimposeMRIToolStripMenuItem_Click(object sender, EventArgs e)
        {
            uC_3DModelingWorkpanel.MRItest();
        }

        private void refreshRendererToolStripMenuItem_Click(object sender, EventArgs e)
        {
            uC_3DModelingWorkpanel.RenderAll();
        }

        private void coneBeamCorrectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            uC_3DModelingWorkpanel.ConeBeamCorrectionAll();
        }

        private void frmImageAnalysis_new_FormClosing(object sender, FormClosingEventArgs e)
        {
            //uC_3DModelingWorkpanel.RenderWindow.GetInteractor().event;
            //uC_3DModelingWorkpanel.Dispose();
            //this.Dispose();
        }

        private void transferMarkersToModelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //return selected measurement(s)

            //Check if rows selected
            int RowCount = _2DMeasurementsWorkpanel.UC_measurementsMain.dgViewMeasurements.Rows.GetRowCount(DataGridViewElementStates.Selected);

            if (RowCount == 0)
                return;


            for (int i = 0; i < RowCount; i++)
            {

                //Get row of clicked cell
                DataGridViewRow row = _2DMeasurementsWorkpanel.UC_measurementsMain.dgViewMeasurements.Rows[_2DMeasurementsWorkpanel.UC_measurementsMain.dgViewMeasurements.SelectedRows[i].Index];
                //Get column AcquisitionNumber
                DataGridViewCell cell = row.Cells["MeasurementID"];

                //Set Database
                DataBase SQLDB = new DataBase(AppData.SQLServer, AppData.SQLDatabase, AppData.SQLAuthSQL, AppData.SQLUser, AppData.SQLPassword);

                Measurement SelectedMeasurement = new Measurement(SQLDB, Subject, EOS.AcquisitionNumber, Convert.ToInt32(cell.Value), AppData);

                CalculateTo3D(SelectedMeasurement);

                if (SimModelVisualization.osimModel == null)
                {
                    // MessageBox.Show("3D position of the marker has been calculated. To attach marker to model you have to load a model first.", "No model loaded!", MessageBoxButtons.OK, MessageBoxIcon.Error);

                }
                else
                {
                    Position pos = new Position((float)SelectedMeasurement.PosX, (float)SelectedMeasurement.PosY, (float)SelectedMeasurement.PosZ);

                   uC_3DModelingWorkpanel.AddMarker(pos, SelectedMeasurement.MeasurementName);
                }
            }
        }

        private void getAbsoluteJointPosAndOrientationsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AddToLogsAndMessages('\t'+ "X pos" + '\t' + "Y pos" + '\t' + "Z pos" + '\t' + "X or " + '\t' + "Y or " + '\t' + "Z or ");

            foreach (OsimJointProperty j in  SimModelVisualization.jointPropertyList)
            {
                double[] pos = j.vtkTransform.GetPosition();
                double[] ori = j.vtkTransform.GetOrientation();

                string text = j.objectName.ToString() + '\t' + pos[0].ToString() + '\t' + pos[1].ToString() + '\t' + pos[2].ToString() + '\t' + ori[0].ToString() + '\t' + ori[1].ToString() + '\t' + ori[2].ToString();
                AddToLogsAndMessages(text);

            }
        }

        private void CheckPanelsLoading()
        {
            if(uC_3DModelingWorkpanel.Loaded3D && _2DMeasurementsWorkpanel.Loaded2D)
            {
                uC_3DModelingWorkpanel.SimModelVisualization.EosImage1 = EosImage1;
                uC_3DModelingWorkpanel.SimModelVisualization.EosImage2 = EosImage2;
                uC_3DModelingWorkpanel.ExecuteIfEOSandModelAreLoaded();


                if (uC_3DModelingWorkpanel.Visible)
                {
                    btn2Dview.Visible = true;
                    btn3Dview.Visible = false;
                }
                else
                {
                    btn3Dview.Visible = true;
                    btn2Dview.Visible = false;
                }
            }
            else
            {
                btn2Dview.Visible = false;
                btn3Dview.Visible = false;
            }
        }

        private void loadMusclesToolStripMenuItem_Click(object sender, EventArgs e)
        {
           
                SimModelVisualization.printMuscles = loadMusclesToolStripMenuItem.Checked;

           
        }

        private void LoadEOSimagesMain()
        {
            Cursor.Current = Cursors.WaitCursor;


            AddToLogsAndMessages("Reading in Radiographs.");


            //Read in the Images

            ReadEOSImage(EosImage1, dd1);

            ReadEOSImage(EosImage2, dd2);

            if (EosImage2.ImagePlane == "(0020,0020) : PatientOrientation (CodeString) -> A\\F")
            {
                EosImage1.imageRotated = true;
            }



            if (!System.IO.File.Exists(EosImage1.Directory))
            {
                MessageBox.Show("Path/File can not be found. Check connection to network database.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                return;
            }
            //Create the 3D EOS space
            uC_3DModelingWorkpanel.EosImage1 = EosImage1;
            uC_3DModelingWorkpanel.EosImage2 = EosImage2;
            uC_3DModelingWorkpanel.EosSpace = EosSpace;



           
            _2DMeasurementsWorkpanel.AppData = AppData;
            _2DMeasurementsWorkpanel.SQLDB = SQLDB;
            _2DMeasurementsWorkpanel.EOS = EOS;
            _2DMeasurementsWorkpanel.EosImage1 = EosImage1;
            _2DMeasurementsWorkpanel.EosImage2 = EosImage2;
            _2DMeasurementsWorkpanel.Subject = Subject;


  
            _2DMeasurementsWorkpanel.LoadImages();

            uC_3DModelingWorkpanel.MakeEosSpace(EosImage1, EosImage2, uC_3DModelingWorkpanel.ren1, EosSpace);
            EosSpace = uC_3DModelingWorkpanel.EosSpace;

          _2DMeasurementsWorkpanel.EosSpace = EosSpace;
            _2DMeasurementsWorkpanel.Dock = DockStyle.Fill;


            uC_3DModelingWorkpanel.Visible = false;
            splitContainer1.Panel2.Controls.Add(_2DMeasurementsWorkpanel);
            _2DMeasurementsWorkpanel.Visible = true;



            AddToLogsAndMessages("3D virtual EOS Space is made.");

            AddToLogsAndMessages("Previous measurements loaded from database.");

         
            Application.UseWaitCursor = false;

          

            CheckPanelsLoading();
            loadEOSFromSWSToolStripMenuItem.Enabled = false;
            importEOSFromFileToolStripMenuItem.Enabled = false;
   RenderAll();
        }


        private void importEOSFromFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (SpineAnalyzer.frmManualImportEOSimages frmManualImportEOSimages = new SpineAnalyzer.frmManualImportEOSimages())
            {
                Cursor.Current = Cursors.WaitCursor;


                EosImage1 = new EosImage();
                EosImage1.AppData = AppData;
                EosImage2 = new EosImage();
                EosImage2.AppData = AppData;

                frmManualImportEOSimages.ShowDialog();


                //Reconstruct the directory of the images
                EosImage1.Directory = frmManualImportEOSimages.file1;
                EosImage2.Directory = frmManualImportEOSimages.file2;
            }

 
            LoadEOSimagesMain();

            CheckPanelsLoading();
        }


        #endregion


    }
}

