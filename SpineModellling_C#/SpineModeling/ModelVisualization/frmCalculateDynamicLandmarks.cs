using Kitware.VTK;
using OpenSim;
using SpineAnalyzer.Simulator;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace SpineAnalyzer.ModelVisualization
{
    public partial class frmCalculateDynamicLandmarks : Form
    {
        public bool useT2 = false;
        private string T1name = "thoracic1 - ";
        private Model model1;
        State si1;
        public AppData AppData;
        public DataBase SQLDB;
        public Subject Subject;

        private List<string> DataNameList = new List<string>();
        private List<string> GeometryNameList = new List<string>();
        private List<string> BodyNameList = new List<string>();
        private List<DataFileObject> DataList = new List<DataFileObject>();
        private string modelfile = string.Empty;

        private List<DataTable> PelvicAnglesDTlist = new List<DataTable>();
        private List<DataTable> T1SPIDTlist = new List<DataTable>();
        private List<DataTable> T9SPIDTlist = new List<DataTable>();
        private List<DataTable> SVAdistDTlist = new List<DataTable>();
        private List<DataTable> LL_DTlist = new List<DataTable>();
        private List<DataTable> TK_DTlist = new List<DataTable>();
        private List<DataTable> Cobb_Anglelist = new List<DataTable>();


        //VTK Stuff
        public RenderWindowControl renderWindowControl1 = new RenderWindowControl();
        public vtkRenderWindow RenderWindow;
        private vtkRenderer ren1; // Declare ren1 as a class-level variable
        public vtkRenderWindowInteractor iren;
        public vtkAxesActor axesGround = vtkAxesActor.New();


        public frmCalculateDynamicLandmarks()
        {
            InitializeComponent();
        }

        private void btnLoadData_Click(object sender, EventArgs e)
        {

            vistaOpenFileDialog1.Filter = "sto Files (*.sto)|*.sto|All files (*.*)|*.*";
            vistaOpenFileDialog1.FilterIndex = 1;

            string fileSourcePath;

            if (vistaOpenFileDialog1.ShowDialog(this) == DialogResult.OK)
            {

                System.Windows.Forms.Cursor.Current = Cursors.WaitCursor;

                string[] filepaths = vistaOpenFileDialog1.FileNames;
                int numberFiles = filepaths.Count<string>();

                if (numberFiles == 1)
                {   
                    fileSourcePath = vistaOpenFileDialog1.FileName;

                    AddDataFile(fileSourcePath);
                }

                if (numberFiles > 1)
                {
                    //if (MessageBox.Show("You selected more than one file. They will all be added to the list.", "Import", MessageBoxButtons.OKCancel, MessageBoxIcon.Information) == DialogResult.OK)
                    //{
                        foreach (string file in filepaths)
                        {
                            AddDataFile(file);

                        }
                    //}
                }

                UpdateDgViewFileNames();

            }
            else { return; }
            Application.UseWaitCursor = false;

            timerIndex = 0;
            // Create a timer with a ten second interval.
            System.Timers.Timer aTimer = new System.Timers.Timer(200);

            // Hook up the Elapsed event for the timer.
            aTimer.Elapsed += new System.Timers.ElapsedEventHandler(OnTimedEvent);

            // Set the Interval to 2 seconds (2000 milliseconds).
            aTimer.Interval = 300;

            aTimer.Enabled = true;

            //aTimer.Close();

            btnFinishedLoading.BackColor = SystemColors.Highlight;


        }

        public void ProgrammaticallyLoadData(string[] filepaths)
        {
            foreach (string file in filepaths)
            {
                AddDataFile(file);

            }
            UpdateDgViewFileNames();
        }

        private void UpdateDgViewFileNames()
        {

            dgViewFileNames.Rows.Clear();
            foreach (string name in DataNameList)
            {
                int intRow = dgViewFileNames.Rows.Add();

                dgViewFileNames.Rows[intRow].Cells["IndexData"].Value = intRow;
                dgViewFileNames.Rows[intRow].Cells["DataFileName"].Value = name;
                //((DataGridViewCheckBoxCell)dgViewFileNames.Rows[intRow].Cells["checkboxData"]).Value = false; //All unchecked
            }
        }

        private int timerIndex = 0;

        void OnTimedEvent(object source, System.Timers.ElapsedEventArgs e)
        {
            if (btnFinishedLoading.BackColor == Color.LightGreen)
            {
                btnFinishedLoading.BackColor = SystemColors.Highlight;
            }
            else
            {
                btnFinishedLoading.BackColor = Color.LightGreen;
            }
            timerIndex++;
            if (timerIndex == 6)
            {
                System.Timers.Timer timer = (System.Timers.Timer)source; // Get the timer that fired the event
                timer.Stop(); // 
                btnFinishedLoading.BackColor = SystemColors.Highlight;
            }
        }

        private void AddDataFile(string DatafileDir)
        {
            if (DataNameList.Contains(DatafileDir))
            {
                MessageBox.Show("You already loaded file: " + DatafileDir + ". All files must be unique.", "Import", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            DataNameList.Add(DatafileDir);



        }

        public void ReadModel(string file)
        {

            model1 = new Model(file);
            si1 = model1.initSystem();


            int nrBodies = model1.getBodySet().getSize();
            for (int i = 0; i < nrBodies; i++)
            {

                Body body = model1.getBodySet().get(i);
                State si = model1.initSystem();
                string name = body.getName();

                BodyNameList.Add(name);
            }

            btnLoadModelfromFile.Enabled = false;
            btnLoadModelfromCloud.Enabled = false;

            SetupTreeview();
        }

        private void AddBodyToDGV()
        {

            //dgViewFileNames.Rows.Clear();
            foreach (string name in DataNameList)
            {
                int intRow = dgViewFileNames.Rows.Add();

                dgViewFileNames.Rows[intRow].Cells["IndexData"].Value = intRow;
                dgViewFileNames.Rows[intRow].Cells["DataFileName"].Value = name;
                ((DataGridViewCheckBoxCell)dgViewFileNames.Rows[intRow].Cells["checkboxData"]).Value = false; //All unchecked
            }


        }

        private void SetupTreeview()
        {

            foreach (string bodyName in BodyNameList)
            {

                TreeNode treeNode = new TreeNode(bodyName); //, bodyName);
                treeNode.Tag = bodyName;
                treeView1.Nodes.Add(treeNode);

                //Subnode for every geometry
                AddGoemetriesAsSubnodes(bodyName, treeNode);
            }

        }

        private void AddGoemetriesAsSubnodes(string bodyName, TreeNode BodytreeNode)
        {

            Body body = model1.getBodySet().get(bodyName);
            State si = model1.initSystem();
            string name = body.getName();


            GeometrySet GeometrySet = new GeometrySet();
            GeometrySet = body.getDisplayer().getGeometrySet();


            int nrGeometries = GeometrySet.getSize();
            for (int j = 0; j < nrGeometries; j++)
            {
                string _objectName = GeometrySet.get(j).getGeometryFile();

                TreeNode treeNodeGeometry = new TreeNode(Path.GetFileNameWithoutExtension(_objectName));
                treeNodeGeometry.Tag = Path.GetFileNameWithoutExtension(_objectName);
                treeView1.Nodes[BodytreeNode.Index].Nodes.Add(treeNodeGeometry);


                int n;
                bool isNumeric = int.TryParse(Path.GetFileNameWithoutExtension(_objectName), out n);

                if (isNumeric)
                {
                    AddLandmarksAsSubnodes(Path.GetFileNameWithoutExtension(_objectName), BodytreeNode, treeNodeGeometry);

                }

            }
        }

        private List<TreeNode> AllLandmarks = new List<TreeNode>();

        private void AddLandmarksAsSubnodes(string geometryName, TreeNode BodytreeNode, TreeNode geometrytreeNode)
        {


            //Build SQL Command (=SQL select statement + Parameters)
            SqlCommand SQLcmd = new SqlCommand();

            string SQLselect;

            SQLselect = "SELECT LandmarkID, LandmarkName, PointID FROM GeometryLandmarks where GeometryID = @GeometryID";
            SQLcmd.Parameters.AddWithValue("@GeometryID", geometryName);


            SQLcmd.CommandText = SQLselect;

            //Set Database
            DataBase SQLDB = new DataBase(AppData.SQLServer, AppData.SQLDatabase, AppData.SQLAuthSQL, AppData.SQLUser, AppData.SQLPassword);

            //Fill a dataset with selected records
            DataSet ds = new DataSet();
            SQLDB.ReadDataSet(SQLcmd, ref ds);


            foreach (DataRow row in ds.Tables[0].Rows)
            {
                string name = row["LandmarkName"].ToString();
                string ID = row["LandmarkID"].ToString() + "_SWSlandmark";

                TreeNode nodeLM = new TreeNode(name);
                nodeLM.Tag = ID;
                treeView1.Nodes[BodytreeNode.Index].Nodes[geometrytreeNode.Index].Nodes.Add(nodeLM);

                BodytreeNode.BackColor = Color.LightGreen;
                if (!AllLandmarks.Contains(nodeLM))
                {
                    AllLandmarks.Add(nodeLM);
                }
            }
        }

        private void refreshLandmarks(string GeometryID)
        {
            //Clear DataGrid
            dataGridViewLM.DataSource = null;
            dataGridViewLM.Refresh();

            //Build SQL Command (=SQL select statement + Parameters)
            SqlCommand SQLcmd = new SqlCommand();

            string SQLselect;


            SQLselect = "SELECT LandmarkID, LandmarkName, UserName, PointID FROM GeometryLandmarks where GeometryID = @GeometryID";
            SQLcmd.Parameters.AddWithValue("@GeometryID", GeometryID);


            SQLcmd.CommandText = SQLselect;

            //Set Database
            DataBase SQLDB = new DataBase(AppData.SQLServer, AppData.SQLDatabase, AppData.SQLAuthSQL, AppData.SQLUser, AppData.SQLPassword);

            //Fill a dataset with selected records
            DataSet ds = new DataSet();
            SQLDB.ReadDataSet(SQLcmd, ref ds);

            // you can make the grid readonly.
            dataGridViewLM.ReadOnly = true;
            dataGridViewLM.DataSource = ds.Tables[0];
            // Resize the DataGridView columns to fit the newly loaded content.
            dataGridViewLM.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.ColumnHeader);
        }

        public vtkTransform ConvertTransformFromSim2VTK(Transform simTransform)
        {
            Vec3 transl = simTransform.T();
            Rotation rot = simTransform.R();
            Vec3 rotvect = rot.convertRotationToBodyFixedXYZ();
            vtkTransform vtktransf = vtkTransform.New();
            vtktransf.Translate(transl.get(0), transl.get(1), transl.get(2));
            vtktransf.RotateX(RadianToDegree(rotvect.get(0)));
            vtktransf.RotateY(RadianToDegree(rotvect.get(1)));
            vtktransf.RotateZ(RadianToDegree(rotvect.get(2)));

            return vtktransf;
        }

        public double RadianToDegree(double angle)
        {
            return angle * (180.0 / Math.PI);
        }

        private void btnLoadModelfromFile_Click(object sender, EventArgs e)
        {
            vistaOpenFileDialog1.Filter = "OSIM Files (*.osim)|*.osim";
            // vistaOpenFileDialog1.FilterIndex = 1;

            string fileSourcePath;

            if (vistaOpenFileDialog1.ShowDialog(this) == DialogResult.OK)
            {

                System.Windows.Forms.Cursor.Current = Cursors.WaitCursor;

                string[] filepaths = vistaOpenFileDialog1.FileNames;
                int numberFiles = filepaths.Count<string>();

                if (numberFiles == 1)
                {
                    modelfile = vistaOpenFileDialog1.FileName;


                }

                //if (numberFiles > 1)
                //{
                //    if (MessageBox.Show("You selected more than one file. They will all be added to the list.", "Import", MessageBoxButtons.OKCancel, MessageBoxIcon.Information) == DialogResult.OK)
                //    {
                //        foreach (string file in filepaths)
                //        {
                //            AddDataFile(file);

                //        }
                //    }
                //}


                ReadModel(modelfile);

               toolTip1.SetToolTip(lblLoadedModel, Path.GetFileNameWithoutExtension(filepaths[0]));
            }
            else { return; }
            Application.UseWaitCursor = false;
        }

        private void btnFinishedLoading_Click(object sender, EventArgs e)
        {
            ProcessInputData();
        }

        public void ProcessInputData()
        { 
            if (DataNameList.Count == 0)
            { return; }

            System.Windows.Forms.Cursor.Current = Cursors.WaitCursor;
            btnLoadData.Enabled = false;
            btnImportFromDatabase.Enabled = false;
            btnFinishedLoading.Enabled = false;


            ImportAllData();


            //Enable buttons

            btnVisualizeData.Enabled = true;
            btncalculateDynamicLandmarks.Enabled = true;
            btnCalculateParameters.Enabled = false;
            btnVisualizeData.Visible = true;

            Application.UseWaitCursor = false;
        }

        private void ImportAllData()
        {
            int index = 0;
            foreach (string DatafileDir in DataNameList)
            {
                DataFileObject dataFileObject = new DataFileObject();
                dataFileObject._DataFileDirectory = DatafileDir;
                dataFileObject._DataFileName = Path.GetFileNameWithoutExtension(DatafileDir);
                dataFileObject._DataIndex = index;

                dataFileObject.Read();

                DataList.Add(dataFileObject);

                index++;
            }
        }

        private List<Landmark> selectedLandmarks = new List<Landmark>();

        private void treeView1_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            string nodeTag = e.Node.Tag.ToString();

            if (nodeTag.Contains("_SWSlandmark"))
            {
                string geometryName = e.Node.Parent.Text;
                string bodyName = e.Node.Parent.Parent.Text;

                string LMID = nodeTag.Split('_')[0];

                Landmark landmark = GetLandmark(Convert.ToInt32(LMID));
                landmark.GeometryName = geometryName;
                landmark.BodyName = bodyName;

                //if(selectedLandmarks.Contains(landmark))

                int index = selectedLandmarks.FindIndex(a => a.LandmarkID == landmark.LandmarkID);
                if (index == -1)
                {
                    selectedLandmarks.Add(landmark);
                }
                else
                {
                    MessageBox.Show("This landmark has already been added to the selection.", "Landmark already selected!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                UpdateDgViewLandmarks();
            }
        }

        private Landmark GetLandmark(int ID)
        {
            Landmark landmark = new Landmark(SQLDB, ID, AppData);
            return landmark;
        }

        private void UpdateDgViewLandmarks()
        {
            dataGridViewLM.Rows.Clear();
            dataGridViewLM.Columns.Clear();

            dataGridViewLM.Columns.Add("Number", "Number");
            dataGridViewLM.Columns.Add("Body", "Body");
            dataGridViewLM.Columns.Add("Geometry", "Geometry");
            dataGridViewLM.Columns.Add("LandmarkName", "Landmark name");
            dataGridViewLM.Columns.Add("LandmarkID", "Landmark ID");
            dataGridViewLM.Columns.Add("UserName", "UserName");
            dataGridViewLM.Columns.Add("X", "X");
            dataGridViewLM.Columns.Add("Y", "Y");
            dataGridViewLM.Columns.Add("Z", "Z");
            foreach (Landmark lm in selectedLandmarks)
            {
                int intRow = dataGridViewLM.Rows.Add();

                dataGridViewLM.Rows[intRow].Cells["Number"].Value = intRow;
                dataGridViewLM.Rows[intRow].Cells["Body"].Value = lm.BodyName;
                dataGridViewLM.Rows[intRow].Cells["Geometry"].Value = lm.GeometryName;
                dataGridViewLM.Rows[intRow].Cells["LandmarkName"].Value = lm.LandmarkName;
                dataGridViewLM.Rows[intRow].Cells["LandmarkID"].Value = lm.LandmarkID;
                dataGridViewLM.Rows[intRow].Cells["UserName"].Value = lm._UserName;
                dataGridViewLM.Rows[intRow].Cells["X"].Value = lm.coorX;
                dataGridViewLM.Rows[intRow].Cells["Y"].Value = lm.coorY;
                dataGridViewLM.Rows[intRow].Cells["Z"].Value = lm.coorZ;

            }
        }

        private void btnReadBodyKinematics_Click(object sender, EventArgs e)
        {
            string bodyName = "sacrum";


            int indexX = DataList[0]._HeaderNames.FindIndex(x => x.Equals(bodyName + "_X"));
            int indexY = DataList[0]._HeaderNames.FindIndex(x => x.Equals(bodyName + "_Y"));
            int indexZ = DataList[0]._HeaderNames.FindIndex(x => x.Equals(bodyName + "_Z"));
            int indexOX = DataList[0]._HeaderNames.FindIndex(x => x.Equals(bodyName + "_Ox"));
            int indexOY = DataList[0]._HeaderNames.FindIndex(x => x.Equals(bodyName + "_Oy"));
            int indexOZ = DataList[0]._HeaderNames.FindIndex(x => x.Equals(bodyName + "_Oz"));

            DataColumn col = DataList[0].AbsoluteDt.Columns[indexX];


            foreach (DataRow row in DataList[0].AbsoluteDt.Rows)
            {
                double posX = Convert.ToDouble(row[indexX].ToString());
                double posY = Convert.ToDouble(row[indexY].ToString());
                double posZ = Convert.ToDouble(row[indexZ].ToString());
                double orX = Convert.ToDouble(row[indexOX].ToString());
                double orY = Convert.ToDouble(row[indexOY].ToString());
                double orZ = Convert.ToDouble(row[indexOZ].ToString());

            }
        }

        private void btnLoadModelfromCloud_Click(object sender, EventArgs e)
        {
            Application.UseWaitCursor = false;


            if (Subject == null)
            {
                MessageBox.Show("You didn't choose a subject from the database before opening this tool.", "No subject selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;

            }
            else
            {
                if (Subject.SWS_volgnummer == 0)
                {
                    MessageBox.Show("You didn't choose a subject from the database before opening this tool.", "No subject selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;

                }

                using (SpineAnalyzer.Acquisitions.frmSelectModel frmSelectModel = new Acquisitions.frmSelectModel())
                {
                    //Pass data to search form
                    frmSelectModel.AppData = this.AppData;
                    frmSelectModel.SQLDB = SQLDB;
                    frmSelectModel.Subject = Subject;
                    frmSelectModel.ShowDialog();
                    if (frmSelectModel.Model != null)
                    {
                        Application.UseWaitCursor = true;
                        toolTip1.SetToolTip(lblLoadedModel, frmSelectModel.Model.AcquisitionName);

                        ReadModel(Path.GetFullPath(AppData.AcquisitionDir + frmSelectModel.Model.Directory));
                        Application.UseWaitCursor = false;

                        if (model1 == null)
                        { return; }
                    }
                }



            }

        }




        private void SetVTKRenderer()
        {
            // 
            // renderWindowControl1
            // 
            this.renderWindowControl1.AddTestActors = false;
            this.renderWindowControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.renderWindowControl1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.renderWindowControl1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.renderWindowControl1.Location = new System.Drawing.Point(400, 38);
            this.renderWindowControl1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.renderWindowControl1.Name = "renderWindowControl1";
            this.renderWindowControl1.Size = new System.Drawing.Size(330, 652);
            this.renderWindowControl1.Dock = DockStyle.Fill;
            this.renderWindowControl1.TabIndex = 63;
            this.renderWindowControl1.TestText = null;
            this.pnlVTKrenderer.Controls.Add(this.renderWindowControl1);

            this.Invalidate();
            renderWindowControl1_Load(GetV());
        }

        private void GetV()
        {
            return ren1.SetBackground(0.2, 0.3, 0.4);
        }

        private void renderWindowControl1_Load(void v)
        {
            // get a reference to the renderwindow of our renderWindowControl1
            RenderWindow = renderWindowControl1.RenderWindow;
            // get a reference to the renderer
            ren1 = RenderWindow.GetRenderers().GetFirstRenderer(); // Initialize ren1 here
            v;
            iren = RenderWindow.GetInteractor();
            RenderWindow.SetInteractor(iren);
        }

        private void frmCalculateDynamicLandmarks_Load(object sender, EventArgs e)
        {
           
            SetVTKRenderer();
            sourcepath = AppData.TempDir;
            this.Invalidate();
        }



        private string VisualisationCase = string.Empty;

        private void btnVisualizeData_Click(object sender, EventArgs e)
        {
            VisualisationCase = "RawData";
            int selectedDataIndex = dgViewFileNames.SelectedRows[0].Index;
            PrintRawDataToRenderer(0, selectedDataIndex);
            trackBar1.Visible = true;
            trackBar1.Maximum = DataList[selectedDataIndex].AbsoluteDt.Rows.Count - 1;
            trackBar1.Minimum = 0;

        }

        private void PrintRawDataToRenderer(int frameIndex, int selectedDataIndex)
        {

            System.Windows.Forms.Cursor.Current = Cursors.WaitCursor;

            if (cbAlwaysClearRenderer.Checked)
            {
                ren1.RemoveAllViewProps();

            }

            foreach (string bodyName in BodyNameList)
            {
                vtkActor Sphereactor = new vtkActor();
                vtkSphereSource sphere = new vtkSphereSource();
                sphere.SetRadius(0.008);
                sphere.SetPhiResolution(8);
                sphere.SetThetaResolution(8);
                vtkPolyDataMapper mapper2 = vtkPolyDataMapper.New();
                mapper2.SetInputConnection(sphere.GetOutputPort());


                vtkAxesActor axesThorax = vtkAxesActor.New();
                axesThorax.SetTotalLength(0.05, 0.05, 0.05);
                axesThorax.AxisLabelsOff();

                Sphereactor.GetProperty().SetColor(1, 0, 0);
                Sphereactor.SetMapper(mapper2);

                if (DataList[selectedDataIndex]._HeaderNames.Contains(bodyName + "_X"))
                {

                    int indexX = DataList[selectedDataIndex]._HeaderNames.FindIndex(x => x.Equals(bodyName + "_X"));
                    int indexY = DataList[selectedDataIndex]._HeaderNames.FindIndex(x => x.Equals(bodyName + "_Y"));
                    int indexZ = DataList[selectedDataIndex]._HeaderNames.FindIndex(x => x.Equals(bodyName + "_Z"));
                    int indexOX = DataList[selectedDataIndex]._HeaderNames.FindIndex(x => x.Equals(bodyName + "_Ox"));
                    int indexOY = DataList[selectedDataIndex]._HeaderNames.FindIndex(x => x.Equals(bodyName + "_Oy"));
                    int indexOZ = DataList[selectedDataIndex]._HeaderNames.FindIndex(x => x.Equals(bodyName + "_Oz"));


                    DataRow row = DataList[selectedDataIndex].AbsoluteDt.Rows[frameIndex];

                    //foreach (DataRow row in DataList[0].AbsoluteDt.Rows)
                    //{
                    double posX = Convert.ToDouble(row[indexX].ToString());
                    double posY = Convert.ToDouble(row[indexY].ToString());
                    double posZ = Convert.ToDouble(row[indexZ].ToString());
                    double orX = Convert.ToDouble(row[indexOX].ToString());
                    double orY = Convert.ToDouble(row[indexOY].ToString());
                    double orZ = Convert.ToDouble(row[indexOZ].ToString());

                    //}



                    vtkTransform transform = new vtkTransform();

                    transform.PreMultiply();

                    Body body = model1.getBodySet().get(bodyName);
                    Vec3 vec = new Vec3();
                    body.getMassCenter(vec);
                   

                    transform.Translate(posX, posY, posZ);

                    transform.RotateX(orX);
                    transform.RotateY(orY);
                    transform.RotateZ(orZ);

                    transform.Translate(-vec.get(0), -vec.get(1), -vec.get(2));




                    Sphereactor.SetUserTransform(transform);
                    axesThorax.SetUserTransform(transform);
                    ren1.AddActor(Sphereactor);
                    ren1.AddActor(axesThorax);
                }
            }
            ren1.GetRenderWindow().Render();
            Application.UseWaitCursor = false;
        }

        private void tableLayoutPanel5_Paint(object sender, PaintEventArgs e)
        {

        }

        private void btncalculateDynamicLandmarks_Click(object sender, EventArgs e)
        {
            CalculatedynamicLMTrajectories();

        }

        public void CalculatedynamicLMTrajectories()
        {


            System.Windows.Forms.Cursor.Current = Cursors.WaitCursor;
            int datafileIndex = 0;

            foreach (DataFileObject datafile in DataList)
            {
                datafile.TransformedLandmarkList.Clear();


                foreach (Landmark landmark in selectedLandmarks)
                {
                    DataTable TransformedLandmarkDT = new DataTable();
                    TransformedLandmarkDT.Columns.Add("posX");
                    TransformedLandmarkDT.Columns.Add("posY");
                    TransformedLandmarkDT.Columns.Add("posZ");

                    for (int i = 0; i < datafile.AbsoluteDt.Rows.Count; i++)
                    {
                        double[] result = TranformLandmarkFrame(landmark, i, datafileIndex);
                        TransformedLandmarkDT.Rows.Add(result[0], result[1], result[2]);
                    }

                    datafile.TransformedLandmarkList.Add(TransformedLandmarkDT);
                }
                datafileIndex++;
            }
            SetupComboBoxes();

            Application.UseWaitCursor = false;


            btnSetOutputPath.Enabled = true;
            btnCalculateParameters.Enabled = true;

            btnVisualizeLandmarks.Enabled = true;
            btnExportCalculatedLandmarks.Enabled = true;
            btnAutoCompleteFields.Enabled = true;
        }

        private void SetupComboBoxes()
        {
            cbSVA_C7body.Items.Clear();
            cbSVA_FemHeadLeft.Items.Clear();
            cbSVA_FemHeadRight.Items.Clear();
            cbSVA_SupPostSacralEndplate.Items.Clear();
            cbPT_FemHeadLeft.Items.Clear();
            cbPT_FemHeadRight.Items.Clear();
            cbPT_SacrumPosterior.Items.Clear();
            cbPT_SacrumAnterior.Items.Clear();

            cbT1SPI_FemHeadLeft.Items.Clear();
            cbT1SPI_FemHeadRight.Items.Clear();
            cbSPIT1_T1.Items.Clear();

            cbT9SPI_FemHeadLeft.Items.Clear();
            cbT9SPI_FemHeadRight.Items.Clear();
            cbSPIT9_T9.Items.Clear();


            cb_LL_FemHeadLeft.Items.Clear();
            cb_LL_FemHeadRight.Items.Clear();
            cb_LL_T12SupAnt.Items.Clear();
            cb_LL_T12SupPost.Items.Clear();
            CB_LL_S1SupANT.Items.Clear();
            CB_LL_S1SupPOST.Items.Clear();

            cb_TK_FemHeadLeft.Items.Clear();
            cb_TK_FemHeadRight.Items.Clear();
            cb_TK_T1SupPost.Items.Clear();
            cb_TK_T1SupAnt.Items.Clear();
            cb_TK_T12SupAnt.Items.Clear();
            cb_TK_T12SupPost.Items.Clear();


            cb_Cobb_FemHeadLeft.Items.Clear();
            cb_Cobb_FemHeadRight.Items.Clear();
            cb_Cobb_T1_Left.Items.Clear();
            cb_Cobb_T1_Right.Items.Clear();
            cb_Cobb_T12_Left.Items.Clear();
            cb_Cobb_T12_Right.Items.Clear();


            cbSVA_C7body.Text = string.Empty;
            cbSVA_FemHeadLeft.Text = string.Empty;
            cbSVA_FemHeadRight.Text = string.Empty;
            cbSVA_SupPostSacralEndplate.Text = string.Empty;
            cbPT_FemHeadLeft.Text = string.Empty;
            cbPT_FemHeadRight.Text = string.Empty;
            cbPT_SacrumPosterior.Text = string.Empty;
            cbPT_SacrumAnterior.Text = string.Empty;

            cbT1SPI_FemHeadLeft.Text = string.Empty;
            cbT1SPI_FemHeadRight.Text = string.Empty;
            cbSPIT1_T1.Text = string.Empty;

            cbT9SPI_FemHeadLeft.Text = string.Empty;
            cbT9SPI_FemHeadRight.Text = string.Empty;
            cbSPIT9_T9.Text = string.Empty;


            cb_LL_FemHeadLeft.Text = string.Empty;
            cb_LL_FemHeadRight.Text = string.Empty;
            cb_LL_T12SupAnt.Text = string.Empty;
            cb_LL_T12SupPost.Text = string.Empty;
            CB_LL_S1SupANT.Text = string.Empty;
            CB_LL_S1SupPOST.Text = string.Empty;

            cb_TK_FemHeadLeft.Text = string.Empty;
            cb_TK_FemHeadRight.Text = string.Empty;
            cb_TK_T1SupPost.Text = string.Empty;
            cb_TK_T1SupAnt.Text = string.Empty;
            cb_TK_T12SupAnt.Text = string.Empty;
            cb_TK_T12SupPost.Text = string.Empty;

            cb_Cobb_FemHeadLeft.Text = string.Empty;
            cb_Cobb_FemHeadRight.Text = string.Empty;
            cb_Cobb_T1_Left.Text = string.Empty;
            cb_Cobb_T1_Right.Text = string.Empty;
            cb_Cobb_T12_Left.Text = string.Empty;
            cb_Cobb_T12_Right.Text = string.Empty;


            foreach (Landmark landmark in selectedLandmarks)
            {
                cbSVA_C7body.Items.Add(landmark.BodyName + " - " + landmark.LandmarkName);
                cbSVA_FemHeadLeft.Items.Add(landmark.BodyName + " - " + landmark.LandmarkName);
                cbSVA_FemHeadRight.Items.Add(landmark.BodyName + " - " + landmark.LandmarkName);
                cbSVA_SupPostSacralEndplate.Items.Add(landmark.BodyName + " - " + landmark.LandmarkName);
                cbPT_FemHeadLeft.Items.Add(landmark.BodyName + " - " + landmark.LandmarkName);
                cbPT_FemHeadRight.Items.Add(landmark.BodyName + " - " + landmark.LandmarkName);
                cbPT_SacrumPosterior.Items.Add(landmark.BodyName + " - " + landmark.LandmarkName);
                cbPT_SacrumAnterior.Items.Add(landmark.BodyName + " - " + landmark.LandmarkName);

                cbT1SPI_FemHeadLeft.Items.Add(landmark.BodyName + " - " + landmark.LandmarkName);
                cbT1SPI_FemHeadRight.Items.Add(landmark.BodyName + " - " + landmark.LandmarkName);
                cbSPIT1_T1.Items.Add(landmark.BodyName + " - " + landmark.LandmarkName);

                cbT9SPI_FemHeadLeft.Items.Add(landmark.BodyName + " - " + landmark.LandmarkName);
                cbT9SPI_FemHeadRight.Items.Add(landmark.BodyName + " - " + landmark.LandmarkName);
                cbSPIT9_T9.Items.Add(landmark.BodyName + " - " + landmark.LandmarkName);


                cb_LL_FemHeadLeft.Items.Add(landmark.BodyName + " - " + landmark.LandmarkName);
                cb_LL_FemHeadRight.Items.Add(landmark.BodyName + " - " + landmark.LandmarkName);
                cb_LL_T12SupAnt.Items.Add(landmark.BodyName + " - " + landmark.LandmarkName);
                cb_LL_T12SupPost.Items.Add(landmark.BodyName + " - " + landmark.LandmarkName);
                CB_LL_S1SupANT.Items.Add(landmark.BodyName + " - " + landmark.LandmarkName);
                CB_LL_S1SupPOST.Items.Add(landmark.BodyName + " - " + landmark.LandmarkName);

                cb_TK_FemHeadLeft.Items.Add(landmark.BodyName + " - " + landmark.LandmarkName);
                cb_TK_FemHeadRight.Items.Add(landmark.BodyName + " - " + landmark.LandmarkName);
                cb_TK_T1SupPost.Items.Add(landmark.BodyName + " - " + landmark.LandmarkName);
                cb_TK_T1SupAnt.Items.Add(landmark.BodyName + " - " + landmark.LandmarkName);
                cb_TK_T12SupAnt.Items.Add(landmark.BodyName + " - " + landmark.LandmarkName);
                cb_TK_T12SupPost.Items.Add(landmark.BodyName + " - " + landmark.LandmarkName);

                cb_Cobb_FemHeadLeft.Items.Add(landmark.BodyName + " - " + landmark.LandmarkName);
                cb_Cobb_FemHeadRight.Items.Add(landmark.BodyName + " - " + landmark.LandmarkName);
                cb_Cobb_T1_Left.Items.Add(landmark.BodyName + " - " + landmark.LandmarkName);
                cb_Cobb_T1_Right.Items.Add(landmark.BodyName + " - " + landmark.LandmarkName);
                cb_Cobb_T12_Left.Items.Add(landmark.BodyName + " - " + landmark.LandmarkName);
                cb_Cobb_T12_Right.Items.Add(landmark.BodyName + " - " + landmark.LandmarkName);

            }

        }

        private double[] TranformLandmarkFrame(Landmark landmark, int frameIndex, int datafileIndex)
        {
            double[] output = new double[3];

            if (DataList[datafileIndex]._HeaderNames.Contains(landmark.BodyName + "_X"))
            {

                int indexX = DataList[datafileIndex]._HeaderNames.FindIndex(x => x.Equals(landmark.BodyName + "_X"));
                int indexY = DataList[datafileIndex]._HeaderNames.FindIndex(x => x.Equals(landmark.BodyName + "_Y"));
                int indexZ = DataList[datafileIndex]._HeaderNames.FindIndex(x => x.Equals(landmark.BodyName + "_Z"));
                int indexOX = DataList[datafileIndex]._HeaderNames.FindIndex(x => x.Equals(landmark.BodyName + "_Ox"));
                int indexOY = DataList[datafileIndex]._HeaderNames.FindIndex(x => x.Equals(landmark.BodyName + "_Oy"));
                int indexOZ = DataList[datafileIndex]._HeaderNames.FindIndex(x => x.Equals(landmark.BodyName + "_Oz"));


                DataRow row = DataList[datafileIndex].AbsoluteDt.Rows[frameIndex];

                //foreach (DataRow row in DataList[0].AbsoluteDt.Rows)
                //{
                double posX = Convert.ToDouble(row[indexX].ToString());
                double posY = Convert.ToDouble(row[indexY].ToString());
                double posZ = Convert.ToDouble(row[indexZ].ToString());
                double orX = Convert.ToDouble(row[indexOX].ToString());
                double orY = Convert.ToDouble(row[indexOY].ToString());
                double orZ = Convert.ToDouble(row[indexOZ].ToString());

                //}

                vtkTransform transform = new vtkTransform();

                //transform.PostMultiply();


                //transform.Translate(landmark.coorX, landmark.coorY, landmark.coorZ);

                //Body body = model1.getBodySet().get(landmark.BodyName);
                //Vec3 vec = new Vec3();
                //body.getMassCenter(vec);
                //transform.Translate(-vec.get(0), -vec.get(1), -vec.get(2));


                //transform.RotateZ(orZ);
                //transform.RotateX(orX);
                //transform.RotateY(orY);


                //transform.Translate(posX, posY, posZ);




                transform.PreMultiply();

                

                Body body = model1.getBodySet().get(landmark.BodyName);
                Vec3 vec = new Vec3();
                body.getMassCenter(vec);  
                //COM correctie staat hieronder
             Transform T =   body.getDisplayer().getTransform();

               Vec3 RotXYZ =  T.R().convertRotationToBodyFixedXYZ();
                Vec3 p = T.p();

                //CoordinateAxis coordinateAxisX = new CoordinateAxis(0);
                //CoordinateAxis coordinateAxisY = new CoordinateAxis(1);
                //CoordinateAxis coordinateAxisZ = new CoordinateAxis(2);

                //Rotation R = new Rotation(BodyOrSpaceType.BodyRotationSequence, Rx, coordinateAxisX, Ry, coordinateAxisY, Rz, coordinateAxisZ);
                //Transform tst = new Transform();
                //tst.set(R, p);






                transform.Translate(posX, posY, posZ);

                transform.RotateX(orX);
                transform.RotateY(orY);
                transform.RotateZ(orZ);


                transform.Translate(-vec.get(0), -vec.get(1), -vec.get(2));
                transform.Translate(landmark.coorX, landmark.coorY, landmark.coorZ);


             



            

                transform.PostMultiply();
                transform.RotateX(RotXYZ.get(0));
                transform.RotateY(RotXYZ.get(1));
                transform.RotateZ(RotXYZ.get(2));
                transform.Translate(p.get(0), p.get(1), p.get(2));

            
    output = transform.GetPosition();
                return output;

            }

            else
                return output;
        }

        private void VisualizeTranformedLM()
        {
            //The first row of the motion file will be used to visualize.

            foreach (DataTable LM in DataList[0].TransformedLandmarkList)
            {
                foreach (DataRow row in LM.Rows)
                {
                    vtkActor Sphereactor = new vtkActor();
                    vtkSphereSource sphere = new vtkSphereSource();
                    sphere.SetRadius(0.008);
                    sphere.SetPhiResolution(36);
                    sphere.SetThetaResolution(36);
                    vtkPolyDataMapper mapper2 = vtkPolyDataMapper.New();
                    mapper2.SetInputConnection(sphere.GetOutputPort());

                    Sphereactor.GetProperty().SetColor(1, 0, 0);
                    Sphereactor.SetMapper(mapper2);


                    vtkTransform transform = new vtkTransform();
                    transform.PostMultiply();

                    transform.Translate(Convert.ToDouble(row[0]), Convert.ToDouble(row[1]), Convert.ToDouble(row[2]));

                    Sphereactor.SetUserTransform(transform);

                    ren1.AddActor(Sphereactor);
                }
            }

        }

        private void btnExportCalculatedLandmarks_Click(object sender, EventArgs e)
        {
            string sourcepath;

            if (vistaSaveFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                sourcepath = vistaSaveFileDialog1.FileName;
            }
            else { return; }

            System.Windows.Forms.Cursor.Current = Cursors.WaitCursor;

            int index = 0;
            foreach (DataFileObject datafile in DataList)
            {


                Write2FileTRC(sourcepath + @"\exportDynamicLandmarks\" + datafile.DataFileName + ".trc", datafile.TransformedLandmarkList);
                index++;
            }
            Application.UseWaitCursor = false;

            if (MessageBox.Show("Files were saved to " + sourcepath + @"\exportDynamicLandmarks\" + "." + '\n' + "OPEN FOLDER?", "Saved", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
            {
                // opens the folder in explorer
                Process.Start("explorer.exe", sourcepath + @"\exportDynamicLandmarks\");
            }
        }

        public void Write2FileTRC(string path, List<DataTable> TransformedLandmarkList)
        {
            System.IO.StreamWriter StreamWriter = new System.IO.StreamWriter(path);

            //Header (remains the same, except for the amount of rows and the frequency (if normalized))
            StreamWriter.WriteLine("PathFileType	4	(X/Y/Z)" + '\t' + path);
            StreamWriter.WriteLine("DataRate	CameraRate	NumFrames	NumMarkers	Units	OrigDataRate	OrigDataStartFrame	OrigNumFrames");
            StreamWriter.WriteLine("100.000000" + '\t' + "100.000000" + '\t' + (TransformedLandmarkList[0].Rows.Count).ToString() + '\t' + TransformedLandmarkList.Count.ToString() + '\t' + "m   100.000000  0" + '\t' + (TransformedLandmarkList[0].Rows.Count - 1).ToString());




            string parameterNamesLine = "Frame#" + '\t' + "Times" + '\t';

            int index = 0;
            foreach (DataTable table in TransformedLandmarkList)
            {
                parameterNamesLine += selectedLandmarks[index].LandmarkName + '\t' + '\t' + '\t';
                index++;
            }

            StreamWriter.WriteLine(parameterNamesLine);


            string coordinatesLine = ("" + '\t' + '\t' + '\t');//.ToString();

            int index3 = 1;
            foreach (DataTable table in TransformedLandmarkList)
            {
                coordinatesLine += "X" + index3.ToString() + '\t' + "Y" + index3.ToString() + '\t' + "Z" + index3.ToString() + '\t';
                index3++;
            }

            StreamWriter.WriteLine(coordinatesLine);



            string DataLine = string.Empty;

            for (int i = 0; i < TransformedLandmarkList[0].Rows.Count; i++)
            {
                DataLine = string.Empty;
                DataLine = i.ToString().Replace(',', '.') + '\t' + ((double)i / 100).ToString().Replace(',', '.') + '\t';

                int index2 = 0;
                foreach (DataTable table in TransformedLandmarkList)
                {

                    DataLine += Convert.ToString(TransformedLandmarkList[index2].Rows[i][0]).Replace(',', '.') + '\t' + Convert.ToString(TransformedLandmarkList[index2].Rows[i][1]).Replace(',', '.') + '\t' + Convert.ToString(TransformedLandmarkList[index2].Rows[i][2]).Replace(',', '.') + '\t';
                    index2++;
                }
                StreamWriter.WriteLine(DataLine);

            }


            StreamWriter.Close();

        }

        public void Write2FileMOT(string path, List<DataTable> ParametersDTlist, List<string> dataHeaders)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            System.IO.StreamWriter StreamWriter = new System.IO.StreamWriter(path);

            //Header (remains the same, except for the amount of rows and the frequency (if normalized))
            StreamWriter.WriteLine("Calculated Dynamic Landmark-based parameters");
            StreamWriter.WriteLine("version = 1");
            StreamWriter.WriteLine("Created by Thomas Overbergh");

            StreamWriter.WriteLine("nRows = " + ParametersDTlist[0].Rows.Count.ToString());



            // StreamWriter.WriteLine("nColumns = " + _nbCol.ToString()); // 133");
            StreamWriter.WriteLine("inDegrees = yes");
            StreamWriter.WriteLine("");
            StreamWriter.WriteLine("Units are S.I.units(second, meters, Newtons, ...)");
            StreamWriter.WriteLine("Angles are in degrees.");
            StreamWriter.WriteLine("");
            StreamWriter.WriteLine("endheader");






            string parameterNamesLine = "time";

            int index = 0;
            foreach (string headerName in dataHeaders)
            {
                parameterNamesLine += '\t' + headerName;
                index++;
            }

            StreamWriter.WriteLine(parameterNamesLine);






            string DataLine = string.Empty;

            for (int i = 0; i < ParametersDTlist[0].Rows.Count; i++)
            {
                DataLine = string.Empty;
                DataLine = ((double)i).ToString().Replace(',', '.');

                int index2 = 0;
                foreach (DataTable table in ParametersDTlist)
                {
                    int index2b = 0;
                    foreach (DataColumn col in table.Columns)
                    {
                        DataLine += '\t' + Convert.ToString(ParametersDTlist[index2].Rows[i][index2b]).Replace(',', '.');
                        index2b++;
                    }
                    index2++;
                }
                StreamWriter.WriteLine(DataLine);

            }


            StreamWriter.Close();

        }

        private void btnVisualizeLandmarks_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Cursor.Current = Cursors.WaitCursor;

            if (cbAlwaysClearRenderer.Checked)
            {
                ren1.RemoveAllViewProps();
                ren1.GetRenderWindow().Render();
            }

            VisualisationCase = "LM";

            VisualizeTranformedLM();

            vtkAxesActor axesThorax = vtkAxesActor.New();
            axesThorax.SetTotalLength(0.6, 0.6, 0.6);
            axesThorax.AxisLabelsOn();

            ren1.AddActor(axesThorax);
            ren1.GetRenderWindow().Render();
            Application.UseWaitCursor = false;
        }




        private void btnCalculateParameters_Click(object sender, EventArgs e)
        {
            CalculateParameters();
        }

        public bool IncludeTimeInFolderHeader = true;
        public string AdditionToFolderStructre = string.Empty;


        public void CalculateParameters()
        { 
            PelvicAnglesDTlist.Clear();
            T1SPIDTlist.Clear();
            T9SPIDTlist.Clear();
            SVAdistDTlist.Clear();
            LL_DTlist.Clear();
            TK_DTlist.Clear();
            Cobb_Anglelist.Clear();

            System.Windows.Forms.Cursor.Current = Cursors.WaitCursor;
            btnSetOutputPath.Enabled = false;

            string timestring = DateTime.Now.ToShortDateString() + "_" + DateTime.Now.ToLongTimeString().Replace(':', '_');

            if (!cbT1SPI.Checked && !cbSVA.Checked && !cbLL.Checked && !cbTK.Checked && !cbGSA.Checked && !cbPelvicAngles.Checked && !cbT9SPI.Checked && !cb_Cor_Cobb_Angles.Checked)
            {
                MessageBox.Show("You need to check at least one of the Measurements", "Not enough input", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            //Calculation


            if (cbSVA.Checked)
            {
                ProcessSVA();
            }
            if (cbT1SPI.Checked)
            {
                ProcessT1SPI();
            }
            if (cbT9SPI.Checked)
            {
                ProcessT9SPI();
            }
            if (cbLL.Checked)
            {
                ProcessLL();
            }
            if (cbTK.Checked)
            {
                ProcessTK();
            }
            if (cbGSA.Checked)
            {

            }
            if (cbPelvicAngles.Checked)
            {
                ProcessPelvicAngles();
            }
            if (cb_Cor_Cobb_Angles.Checked)
            {
                ProcessCobbAngle();
            }



            //Export
            int index = 0;
            foreach (DataFileObject datafile in DataList)
            {
                List<DataTable> data = new List<DataTable>();
                List<string> dataHeaders = new List<string>();

                if (PelvicAnglesDTlist.Count - 1 >= index && PelvicAnglesDTlist.Count != 0)
                {
                    data.Add(PelvicAnglesDTlist[index]);
                    dataHeaders.Add("Dynamic Pelvic Incidence");
                    dataHeaders.Add("Dynamic Pelvic Tilt");
                    dataHeaders.Add("Dynamic Sacral Slope");
                }
                if (T1SPIDTlist.Count - 1 >= index && T1SPIDTlist.Count != 0)
                {
                    data.Add(T1SPIDTlist[index]);
                    dataHeaders.Add("Dynamic T1 SPI (Sag)");
                    dataHeaders.Add("Dynamic T1 SPI (Cor)");
                    dataHeaders.Add("Dynamic T1 SPI (3D, Abs)");
                }
                if (T9SPIDTlist.Count - 1 >= index && T9SPIDTlist.Count != 0)
                {
                    data.Add(T9SPIDTlist[index]);
                    dataHeaders.Add("Dynamic T9 SPI (Sag)");
                    dataHeaders.Add("Dynamic T9 SPI (Cor)");
                    dataHeaders.Add("Dynamic T9 SPI (3D, Abs)");
                }
                if (SVAdistDTlist.Count - 1 >= index && SVAdistDTlist.Count != 0)
                {
                    data.Add(SVAdistDTlist[index]);
                    dataHeaders.Add("Dynamic SVA (Sag)");
                    dataHeaders.Add("Dynamic SVA (Cor)");
                    dataHeaders.Add("Dynamic SVA (3D, Abs)");
                }
                if (LL_DTlist.Count - 1 >= index && LL_DTlist.Count != 0)
                {
                    data.Add(LL_DTlist[index]);
                    dataHeaders.Add("Lumbar Lordosis " + txtSuperior_LL.Text + "-" + txtInferior_LL.Text);
                }
                if (TK_DTlist.Count - 1 >= index && TK_DTlist.Count != 0)
                {
                    data.Add(TK_DTlist[index]);
                    dataHeaders.Add("Thoracic Kyphosis " + TK_Superior.Text + "-" + TK_Inferior.Text);
                }
                if (Cobb_Anglelist.Count - 1 >= index && Cobb_Anglelist.Count != 0)
                {
                    data.Add(Cobb_Anglelist[index]);
                    dataHeaders.Add("Cor. Cobb Angle " + TXT_Cobb_Superior.Text + "-" + TXT_Cobb_Inferior.Text);
                }

                if (data.Count != 0)
                {
                    if(IncludeTimeInFolderHeader)
                    {
                        Write2FileMOT(sourcepath + @"\DynamicMeasurements_" + timestring + @"\" + datafile.DataFileName + ".mot", data, dataHeaders);
                    }
                    else
                    {
                        Write2FileMOT(sourcepath + @"\DynamicMeasurements_\" + AdditionToFolderStructre + "_" + datafile.DataFileName + ".mot", data, dataHeaders);  
                    }
             
                }
                index++;
            }


            Application.UseWaitCursor = false;


            if(!IgnoreMessages)
            {
            if (MessageBox.Show("Files were saved to " + sourcepath + @"\DynamicMeasurements_" + timestring + @"\" + "." + '\n' + "OPEN FOLDER?", "Saved", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
            {
                // opens the folder in explorer
                Process.Start("explorer.exe", sourcepath + @"\DynamicMeasurements_" + timestring + @"\");
            }
            }

        }

        public bool IgnoreMessages = false;

        private void ProcessCobbAngle()
        {
            int Index_FemHeadLeft = cb_Cobb_FemHeadLeft.SelectedIndex;
            int Index_FemHeadRight = cb_Cobb_FemHeadRight.SelectedIndex;
            int Index_T12Left = cb_Cobb_T12_Left.SelectedIndex;
            int Index_T12Right = cb_Cobb_T12_Right.SelectedIndex;
            int Index_T1Left = cb_Cobb_T1_Left.SelectedIndex;
            int Index_T1Right = cb_Cobb_T1_Right.SelectedIndex;

            if (Index_FemHeadLeft == -1 || Index_FemHeadRight == -1 || Index_T12Left == -1 || Index_T12Right == -1 || Index_T1Left == -1 || Index_T1Right == -1)
            {
                if (!IgnoreMessages)
                {
                    MessageBox.Show("One or more of the landmarks needed for the Cornal Cobb calculation are not assigned. This measurement will be skipped.", "Not enough input", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                foreach (DataFileObject dataobject in DataList)
                {
                    DataTable dataTable = new DataTable();
                    dataTable.Columns.Add("Cobb");

                    for (int i = 0; i < dataobject.TransformedLandmarkList[0].Rows.Count; i++)
                    {
                        System.Windows.Media.Media3D.Point3D T12Left = new System.Windows.Media.Media3D.Point3D(Convert.ToDouble(dataobject.TransformedLandmarkList[Index_T12Left].Rows[i]["posX"]), Convert.ToDouble(dataobject.TransformedLandmarkList[Index_T12Left].Rows[i]["posY"]), Convert.ToDouble(dataobject.TransformedLandmarkList[Index_T12Left].Rows[i]["posZ"]));
                        System.Windows.Media.Media3D.Point3D femHeadLeft = new System.Windows.Media.Media3D.Point3D(Convert.ToDouble(dataobject.TransformedLandmarkList[Index_FemHeadLeft].Rows[i]["posX"]), Convert.ToDouble(dataobject.TransformedLandmarkList[Index_FemHeadLeft].Rows[i]["posY"]), Convert.ToDouble(dataobject.TransformedLandmarkList[Index_FemHeadLeft].Rows[i]["posZ"]));
                        System.Windows.Media.Media3D.Point3D femHeadRight = new System.Windows.Media.Media3D.Point3D(Convert.ToDouble(dataobject.TransformedLandmarkList[Index_FemHeadRight].Rows[i]["posX"]), Convert.ToDouble(dataobject.TransformedLandmarkList[Index_FemHeadRight].Rows[i]["posY"]), Convert.ToDouble(dataobject.TransformedLandmarkList[Index_FemHeadRight].Rows[i]["posZ"]));
                        System.Windows.Media.Media3D.Point3D T12Right = new System.Windows.Media.Media3D.Point3D(Convert.ToDouble(dataobject.TransformedLandmarkList[Index_T12Right].Rows[i]["posX"]), Convert.ToDouble(dataobject.TransformedLandmarkList[Index_T12Right].Rows[i]["posY"]), Convert.ToDouble(dataobject.TransformedLandmarkList[Index_T12Right].Rows[i]["posZ"]));
                        System.Windows.Media.Media3D.Point3D T1Left = new System.Windows.Media.Media3D.Point3D(Convert.ToDouble(dataobject.TransformedLandmarkList[Index_T1Left].Rows[i]["posX"]), Convert.ToDouble(dataobject.TransformedLandmarkList[Index_T1Left].Rows[i]["posY"]), Convert.ToDouble(dataobject.TransformedLandmarkList[Index_T1Left].Rows[i]["posZ"]));
                        System.Windows.Media.Media3D.Point3D T1Right = new System.Windows.Media.Media3D.Point3D(Convert.ToDouble(dataobject.TransformedLandmarkList[Index_T1Right].Rows[i]["posX"]), Convert.ToDouble(dataobject.TransformedLandmarkList[Index_T1Right].Rows[i]["posY"]), Convert.ToDouble(dataobject.TransformedLandmarkList[Index_T1Right].Rows[i]["posZ"]));

                        double Cobb_Angle;

                        CalculateCobb(T12Left, T12Right, T1Left, T1Right, femHeadLeft, femHeadRight, out Cobb_Angle);


                        dataTable.Rows.Add(Cobb_Angle.ToString().Replace(',', '.'));//, CorValue.ToString().Replace(',', '.'), value3D.ToString().Replace(',', '.'));
                    }
                    Cobb_Anglelist.Add(dataTable);
                }
            }

        }

        private void ProcessLL()
        {
            int Index_FemHeadLeft = cb_LL_FemHeadLeft.SelectedIndex;
            int Index_FemHeadRight = cb_LL_FemHeadRight.SelectedIndex;
            int Index_T12Ant = cb_LL_T12SupAnt.SelectedIndex;
            int Index_T12Post = cb_LL_T12SupPost.SelectedIndex;
            int Index_S1Ant = CB_LL_S1SupANT.SelectedIndex;
            int Index_S1Post = CB_LL_S1SupPOST.SelectedIndex;

            if (Index_FemHeadLeft == -1 || Index_FemHeadRight == -1 || Index_T12Ant == -1 || Index_T12Post == -1 || Index_S1Ant == -1 || Index_S1Post == -1)
            {
                if (!IgnoreMessages)
                {
                    MessageBox.Show("One or more of the landmarks needed for the LL calculation are not assigned. This measurement will be skipped.", "Not enough input", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                foreach (DataFileObject dataobject in DataList)
                {
                    DataTable dataTable = new DataTable();
                    dataTable.Columns.Add("LL");

                    for (int i = 0; i < dataobject.TransformedLandmarkList[0].Rows.Count; i++)
                    {
                        System.Windows.Media.Media3D.Point3D T12Ant = new System.Windows.Media.Media3D.Point3D(Convert.ToDouble(dataobject.TransformedLandmarkList[Index_T12Ant].Rows[i]["posX"]), Convert.ToDouble(dataobject.TransformedLandmarkList[Index_T12Ant].Rows[i]["posY"]), Convert.ToDouble(dataobject.TransformedLandmarkList[Index_T12Ant].Rows[i]["posZ"]));
                        System.Windows.Media.Media3D.Point3D femHeadLeft = new System.Windows.Media.Media3D.Point3D(Convert.ToDouble(dataobject.TransformedLandmarkList[Index_FemHeadLeft].Rows[i]["posX"]), Convert.ToDouble(dataobject.TransformedLandmarkList[Index_FemHeadLeft].Rows[i]["posY"]), Convert.ToDouble(dataobject.TransformedLandmarkList[Index_FemHeadLeft].Rows[i]["posZ"]));
                        System.Windows.Media.Media3D.Point3D femHeadRight = new System.Windows.Media.Media3D.Point3D(Convert.ToDouble(dataobject.TransformedLandmarkList[Index_FemHeadRight].Rows[i]["posX"]), Convert.ToDouble(dataobject.TransformedLandmarkList[Index_FemHeadRight].Rows[i]["posY"]), Convert.ToDouble(dataobject.TransformedLandmarkList[Index_FemHeadRight].Rows[i]["posZ"]));
                        System.Windows.Media.Media3D.Point3D T12Post = new System.Windows.Media.Media3D.Point3D(Convert.ToDouble(dataobject.TransformedLandmarkList[Index_T12Post].Rows[i]["posX"]), Convert.ToDouble(dataobject.TransformedLandmarkList[Index_T12Post].Rows[i]["posY"]), Convert.ToDouble(dataobject.TransformedLandmarkList[Index_T12Post].Rows[i]["posZ"]));
                        System.Windows.Media.Media3D.Point3D S1Ant = new System.Windows.Media.Media3D.Point3D(Convert.ToDouble(dataobject.TransformedLandmarkList[Index_S1Ant].Rows[i]["posX"]), Convert.ToDouble(dataobject.TransformedLandmarkList[Index_S1Ant].Rows[i]["posY"]), Convert.ToDouble(dataobject.TransformedLandmarkList[Index_S1Ant].Rows[i]["posZ"]));
                        System.Windows.Media.Media3D.Point3D S1Post = new System.Windows.Media.Media3D.Point3D(Convert.ToDouble(dataobject.TransformedLandmarkList[Index_S1Post].Rows[i]["posX"]), Convert.ToDouble(dataobject.TransformedLandmarkList[Index_S1Post].Rows[i]["posY"]), Convert.ToDouble(dataobject.TransformedLandmarkList[Index_S1Post].Rows[i]["posZ"]));

                        double LL_Angle;

                        CalculateLL(T12Ant, T12Post, S1Ant, S1Post, femHeadLeft, femHeadRight, out LL_Angle);

                        dataTable.Rows.Add(LL_Angle.ToString().Replace(',', '.'));//, CorValue.ToString().Replace(',', '.'), value3D.ToString().Replace(',', '.'));
                    }
                    LL_DTlist.Add(dataTable);
                }
            }

        }

        private void ProcessTK()
        {
            int Index_FemHeadLeft = cb_TK_FemHeadLeft.SelectedIndex;
            int Index_FemHeadRight = cb_TK_FemHeadRight.SelectedIndex;
            int Index_T1Ant = cb_TK_T1SupAnt.SelectedIndex;
            int Index_T1Post = cb_TK_T1SupPost.SelectedIndex;
            int Index_T12Ant = cb_TK_T12SupAnt.SelectedIndex;
            int Index_T12Post = cb_TK_T12SupPost.SelectedIndex;

            if (Index_FemHeadLeft == -1 || Index_FemHeadRight == -1 || Index_T12Ant == -1 || Index_T12Post == -1 || Index_T1Ant == -1 || Index_T1Post == -1)
            {
                if (!IgnoreMessages)
                {
                    MessageBox.Show("One or more of the landmarks needed for the TK calculation are not assigned. This measurement will be skipped.", "Not enough input", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                foreach (DataFileObject dataobject in DataList)
                {
                    DataTable dataTable = new DataTable();
                    dataTable.Columns.Add("TK");

                    for (int i = 0; i < dataobject.TransformedLandmarkList[0].Rows.Count; i++)
                    {
                        System.Windows.Media.Media3D.Point3D T12Ant = new System.Windows.Media.Media3D.Point3D(Convert.ToDouble(dataobject.TransformedLandmarkList[Index_T12Ant].Rows[i]["posX"]), Convert.ToDouble(dataobject.TransformedLandmarkList[Index_T12Ant].Rows[i]["posY"]), Convert.ToDouble(dataobject.TransformedLandmarkList[Index_T12Ant].Rows[i]["posZ"]));
                        System.Windows.Media.Media3D.Point3D femHeadLeft = new System.Windows.Media.Media3D.Point3D(Convert.ToDouble(dataobject.TransformedLandmarkList[Index_FemHeadLeft].Rows[i]["posX"]), Convert.ToDouble(dataobject.TransformedLandmarkList[Index_FemHeadLeft].Rows[i]["posY"]), Convert.ToDouble(dataobject.TransformedLandmarkList[Index_FemHeadLeft].Rows[i]["posZ"]));
                        System.Windows.Media.Media3D.Point3D femHeadRight = new System.Windows.Media.Media3D.Point3D(Convert.ToDouble(dataobject.TransformedLandmarkList[Index_FemHeadRight].Rows[i]["posX"]), Convert.ToDouble(dataobject.TransformedLandmarkList[Index_FemHeadRight].Rows[i]["posY"]), Convert.ToDouble(dataobject.TransformedLandmarkList[Index_FemHeadRight].Rows[i]["posZ"]));
                        System.Windows.Media.Media3D.Point3D T12Post = new System.Windows.Media.Media3D.Point3D(Convert.ToDouble(dataobject.TransformedLandmarkList[Index_T12Post].Rows[i]["posX"]), Convert.ToDouble(dataobject.TransformedLandmarkList[Index_T12Post].Rows[i]["posY"]), Convert.ToDouble(dataobject.TransformedLandmarkList[Index_T12Post].Rows[i]["posZ"]));
                        System.Windows.Media.Media3D.Point3D T1Ant = new System.Windows.Media.Media3D.Point3D(Convert.ToDouble(dataobject.TransformedLandmarkList[Index_T1Ant].Rows[i]["posX"]), Convert.ToDouble(dataobject.TransformedLandmarkList[Index_T1Ant].Rows[i]["posY"]), Convert.ToDouble(dataobject.TransformedLandmarkList[Index_T1Ant].Rows[i]["posZ"]));
                        System.Windows.Media.Media3D.Point3D T1Post = new System.Windows.Media.Media3D.Point3D(Convert.ToDouble(dataobject.TransformedLandmarkList[Index_T1Post].Rows[i]["posX"]), Convert.ToDouble(dataobject.TransformedLandmarkList[Index_T1Post].Rows[i]["posY"]), Convert.ToDouble(dataobject.TransformedLandmarkList[Index_T1Post].Rows[i]["posZ"]));

                        double TK_Angle;

                        CalculateLL( T12Post, T12Ant, T1Post, T1Ant, femHeadLeft, femHeadRight, out TK_Angle); //Relative to LL, but ANT en Post switched.

                        dataTable.Rows.Add(TK_Angle.ToString().Replace(',', '.'));//, CorValue.ToString().Replace(',', '.'), value3D.ToString().Replace(',', '.'));
                    }
                    TK_DTlist.Add(dataTable);
                }
            }

        }


        private void ProcessPelvicAngles()
        {
            int Index_FemHeadLeft = cbPT_FemHeadLeft.SelectedIndex;
            int Index_FemHeadRight = cbPT_FemHeadRight.SelectedIndex;
            int Index_SacrumPosterior = cbPT_SacrumPosterior.SelectedIndex;
            int Index_SacrumAnterior = cbPT_SacrumAnterior.SelectedIndex;


            if (Index_FemHeadLeft == -1 || Index_FemHeadRight == -1 || Index_SacrumPosterior == -1 || Index_SacrumAnterior == -1)
            {
                if (!IgnoreMessages)
                {
                    MessageBox.Show("One or more of the landmarks needed for the Pelvic angle calculation are not assigned. This measurement will be skipped.", "Not enough input", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                foreach (DataFileObject dataobject in DataList)
                {
                    DataTable dataTable = new DataTable();
                    dataTable.Columns.Add("PelvicIncidence");
                    dataTable.Columns.Add("PelvicTilt");
                    dataTable.Columns.Add("SacralSlope");

                    for (int i = 0; i < dataobject.TransformedLandmarkList[0].Rows.Count; i++)
                    {
                        System.Windows.Media.Media3D.Point3D AntEndplateSacrum = new System.Windows.Media.Media3D.Point3D(Convert.ToDouble(dataobject.TransformedLandmarkList[Index_SacrumAnterior].Rows[i]["posX"]), Convert.ToDouble(dataobject.TransformedLandmarkList[Index_SacrumAnterior].Rows[i]["posY"]), Convert.ToDouble(dataobject.TransformedLandmarkList[Index_SacrumAnterior].Rows[i]["posZ"]));
                        System.Windows.Media.Media3D.Point3D postEndplateSacrum = new System.Windows.Media.Media3D.Point3D(Convert.ToDouble(dataobject.TransformedLandmarkList[Index_SacrumPosterior].Rows[i]["posX"]), Convert.ToDouble(dataobject.TransformedLandmarkList[Index_SacrumPosterior].Rows[i]["posY"]), Convert.ToDouble(dataobject.TransformedLandmarkList[Index_SacrumPosterior].Rows[i]["posZ"]));
                        System.Windows.Media.Media3D.Point3D femHeadLeft = new System.Windows.Media.Media3D.Point3D(Convert.ToDouble(dataobject.TransformedLandmarkList[Index_FemHeadLeft].Rows[i]["posX"]), Convert.ToDouble(dataobject.TransformedLandmarkList[Index_FemHeadLeft].Rows[i]["posY"]), Convert.ToDouble(dataobject.TransformedLandmarkList[Index_FemHeadLeft].Rows[i]["posZ"]));
                        System.Windows.Media.Media3D.Point3D femHeadRight = new System.Windows.Media.Media3D.Point3D(Convert.ToDouble(dataobject.TransformedLandmarkList[Index_FemHeadRight].Rows[i]["posX"]), Convert.ToDouble(dataobject.TransformedLandmarkList[Index_FemHeadRight].Rows[i]["posY"]), Convert.ToDouble(dataobject.TransformedLandmarkList[Index_FemHeadRight].Rows[i]["posZ"]));

                        if (cb_pelvicAngles_Sagittalprojection.Checked)
                        {
                            double SSangle;
                            double PTangle;
                            double PIangle;
                           
                            System.Windows.Media.Media3D.Point3D SacrMidp;
                            System.Windows.Media.Media3D.Point3D SacrPostMidp;
                            CalculatePelvicAnglesSagProj(AntEndplateSacrum, postEndplateSacrum, femHeadLeft, femHeadRight, false, out PIangle, out PTangle, out SSangle, out SacrMidp, out SacrPostMidp);
                            dataTable.Rows.Add(PIangle.ToString().Replace(',', '.'), PTangle.ToString().Replace(',', '.'), SSangle.ToString().Replace(',', '.'));
                        }
                        else
                        {
                            double PIvalue = CalculatePI(AntEndplateSacrum, postEndplateSacrum, femHeadLeft, femHeadRight);
                            double PTvalue = CalculatePT(AntEndplateSacrum, postEndplateSacrum, femHeadLeft, femHeadRight);
                            double SSvalue = PIvalue - PTvalue;
                            dataTable.Rows.Add(PIvalue.ToString().Replace(',', '.'), PTvalue.ToString().Replace(',', '.'), SSvalue.ToString().Replace(',', '.'));
                        }
                    }
                    PelvicAnglesDTlist.Add(dataTable);
                }
            }
        }

        private void ProcessT1SPI()
        {
            int Index_FemHeadLeft = cbT1SPI_FemHeadLeft.SelectedIndex;
            int Index_FemHeadRight = cbT1SPI_FemHeadRight.SelectedIndex;

            int Index_T1 = cbSPIT1_T1.SelectedIndex;

            if (Index_FemHeadLeft == -1 || Index_FemHeadRight == -1 || Index_T1 == -1)
            {
                if (!IgnoreMessages)
                {
                    MessageBox.Show("One or more of the landmarks needed for the T1-SPI calculation are not assigned. This measurement will be skipped.", "Not enough input", MessageBoxButtons.OK, MessageBoxIcon.Information);
                     }
            }
            else
            {
                foreach (DataFileObject dataobject in DataList)
                {
                    DataTable dataTable = new DataTable();
                    dataTable.Columns.Add("T1 SPI (sag)");
                    dataTable.Columns.Add("T1 SPI (cor)");
                    dataTable.Columns.Add("T1 SPI (3D)");
                    //dataTable.Columns.Add("PelvicTilt");
                    //dataTable.Columns.Add("SacralSlope");

                    for (int i = 0; i < dataobject.TransformedLandmarkList[0].Rows.Count; i++)
                    {
                        System.Windows.Media.Media3D.Point3D femHeadLeft = new System.Windows.Media.Media3D.Point3D(Convert.ToDouble(dataobject.TransformedLandmarkList[Index_FemHeadLeft].Rows[i]["posX"]), Convert.ToDouble(dataobject.TransformedLandmarkList[Index_FemHeadLeft].Rows[i]["posY"]), Convert.ToDouble(dataobject.TransformedLandmarkList[Index_FemHeadLeft].Rows[i]["posZ"]));
                        System.Windows.Media.Media3D.Point3D femHeadRight = new System.Windows.Media.Media3D.Point3D(Convert.ToDouble(dataobject.TransformedLandmarkList[Index_FemHeadRight].Rows[i]["posX"]), Convert.ToDouble(dataobject.TransformedLandmarkList[Index_FemHeadRight].Rows[i]["posY"]), Convert.ToDouble(dataobject.TransformedLandmarkList[Index_FemHeadRight].Rows[i]["posZ"]));
                        System.Windows.Media.Media3D.Point3D T1_body = new System.Windows.Media.Media3D.Point3D(Convert.ToDouble(dataobject.TransformedLandmarkList[Index_T1].Rows[i]["posX"]), Convert.ToDouble(dataobject.TransformedLandmarkList[Index_T1].Rows[i]["posY"]), Convert.ToDouble(dataobject.TransformedLandmarkList[Index_T1].Rows[i]["posZ"]));
                        double sagValue;
                        double CorValue;
                        double value3D;

                        System.Windows.Media.Media3D.Point3D T1Rotp;

                        CalculateSPI(femHeadLeft, femHeadRight, T1_body, "T1", false, out sagValue, out CorValue, out value3D, out T1Rotp);
                        //double PTvalue = CalculatePT(AntEndplateSacrum, postEndplateSacrum, femHeadLeft, femHeadRight);
                        //double SSvalue = PIvalue - PTvalue;

                        dataTable.Rows.Add(sagValue.ToString().Replace(',', '.'), CorValue.ToString().Replace(',', '.'), value3D.ToString().Replace(',', '.'));
                    }
                    T1SPIDTlist.Add(dataTable);
                }
            }

        }

        private void ProcessT9SPI()
        {
            int Index_FemHeadLeft = cbT9SPI_FemHeadLeft.SelectedIndex;
            int Index_FemHeadRight = cbT9SPI_FemHeadRight.SelectedIndex;

            int Index_T9 = cbSPIT9_T9.SelectedIndex;

            if (Index_FemHeadLeft == -1 || Index_FemHeadRight == -1 || Index_T9 == -1)
            {
                if (!IgnoreMessages)
                {
                    MessageBox.Show("One or more of the landmarks needed for the T9-SPI calculation are not assigned. This measurement will be skipped.", "Not enough input", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                foreach (DataFileObject dataobject in DataList)
                {
                    DataTable dataTable = new DataTable();
                    dataTable.Columns.Add("T9 SPI (sag)");
                    dataTable.Columns.Add("T9 SPI (cor)");
                    dataTable.Columns.Add("T9 SPI (3D)");

                    for (int i = 0; i < dataobject.TransformedLandmarkList[0].Rows.Count; i++)
                    {
                        System.Windows.Media.Media3D.Point3D femHeadLeft = new System.Windows.Media.Media3D.Point3D(Convert.ToDouble(dataobject.TransformedLandmarkList[Index_FemHeadLeft].Rows[i]["posX"]), Convert.ToDouble(dataobject.TransformedLandmarkList[Index_FemHeadLeft].Rows[i]["posY"]), Convert.ToDouble(dataobject.TransformedLandmarkList[Index_FemHeadLeft].Rows[i]["posZ"]));
                        System.Windows.Media.Media3D.Point3D femHeadRight = new System.Windows.Media.Media3D.Point3D(Convert.ToDouble(dataobject.TransformedLandmarkList[Index_FemHeadRight].Rows[i]["posX"]), Convert.ToDouble(dataobject.TransformedLandmarkList[Index_FemHeadRight].Rows[i]["posY"]), Convert.ToDouble(dataobject.TransformedLandmarkList[Index_FemHeadRight].Rows[i]["posZ"]));
                        System.Windows.Media.Media3D.Point3D T9_body = new System.Windows.Media.Media3D.Point3D(Convert.ToDouble(dataobject.TransformedLandmarkList[Index_T9].Rows[i]["posX"]), Convert.ToDouble(dataobject.TransformedLandmarkList[Index_T9].Rows[i]["posY"]), Convert.ToDouble(dataobject.TransformedLandmarkList[Index_T9].Rows[i]["posZ"]));
                        double sagValue;
                        double CorValue;
                        double value3D;
                        System.Windows.Media.Media3D.Point3D T1Rotp;

                        CalculateSPI(femHeadLeft, femHeadRight, T9_body, "T9", false, out sagValue, out CorValue, out value3D, out T1Rotp);


                        dataTable.Rows.Add(sagValue.ToString().Replace(',', '.'), CorValue.ToString().Replace(',', '.'), value3D.ToString().Replace(',', '.'));
                    }
                    T9SPIDTlist.Add(dataTable);
                }
            }
        }

        private void ProcessSVA()
        {
            int Index_FemHeadLeft = cbSVA_FemHeadLeft.SelectedIndex;
            int Index_FemHeadRight = cbSVA_FemHeadRight.SelectedIndex;
            int Index_SacrumPosterior = cbSVA_SupPostSacralEndplate.SelectedIndex;
            int Index_C7 = cbSVA_C7body.SelectedIndex;

            if (Index_FemHeadLeft == -1 || Index_FemHeadRight == -1 || Index_SacrumPosterior == -1 || Index_C7 == -1)
            {
                if (!IgnoreMessages)
                {
                    MessageBox.Show("One or more of the landmarks needed for the SVA calculation are not assigned. This measurement will be skipped.", "Not enough input", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                foreach (DataFileObject dataobject in DataList)
                {
                    DataTable dataTable = new DataTable();
                    dataTable.Columns.Add("SVA");
                    dataTable.Columns.Add("Cor");
                    dataTable.Columns.Add("3D");
                    //dataTable.Columns.Add("PelvicTilt");
                    //dataTable.Columns.Add("SacralSlope");

                    for (int i = 0; i < dataobject.TransformedLandmarkList[0].Rows.Count; i++)
                    {
                        System.Windows.Media.Media3D.Point3D postEndplateSacrum = new System.Windows.Media.Media3D.Point3D(Convert.ToDouble(dataobject.TransformedLandmarkList[Index_SacrumPosterior].Rows[i]["posX"]), Convert.ToDouble(dataobject.TransformedLandmarkList[Index_SacrumPosterior].Rows[i]["posY"]), Convert.ToDouble(dataobject.TransformedLandmarkList[Index_SacrumPosterior].Rows[i]["posZ"]));
                        System.Windows.Media.Media3D.Point3D femHeadLeft = new System.Windows.Media.Media3D.Point3D(Convert.ToDouble(dataobject.TransformedLandmarkList[Index_FemHeadLeft].Rows[i]["posX"]), Convert.ToDouble(dataobject.TransformedLandmarkList[Index_FemHeadLeft].Rows[i]["posY"]), Convert.ToDouble(dataobject.TransformedLandmarkList[Index_FemHeadLeft].Rows[i]["posZ"]));
                        System.Windows.Media.Media3D.Point3D femHeadRight = new System.Windows.Media.Media3D.Point3D(Convert.ToDouble(dataobject.TransformedLandmarkList[Index_FemHeadRight].Rows[i]["posX"]), Convert.ToDouble(dataobject.TransformedLandmarkList[Index_FemHeadRight].Rows[i]["posY"]), Convert.ToDouble(dataobject.TransformedLandmarkList[Index_FemHeadRight].Rows[i]["posZ"]));
                        System.Windows.Media.Media3D.Point3D C7_body = new System.Windows.Media.Media3D.Point3D(Convert.ToDouble(dataobject.TransformedLandmarkList[Index_C7].Rows[i]["posX"]), Convert.ToDouble(dataobject.TransformedLandmarkList[Index_C7].Rows[i]["posY"]), Convert.ToDouble(dataobject.TransformedLandmarkList[Index_C7].Rows[i]["posZ"]));

                        double SVAvalue;
                        double CorValue;
                        double value3D;
      
                        System.Windows.Media.Media3D.Point3D C7Rotp;
                        System.Windows.Media.Media3D.Point3D SacrRotP;

                        //System.Windows.Media.Media3D.Point3D sacrRotp;

                        CalculateSVA(postEndplateSacrum, femHeadLeft, femHeadRight, C7_body, false, out SVAvalue, out CorValue, out value3D, out C7Rotp, out SacrRotP);
                        //double PTvalue = CalculatePT(AntEndplateSacrum, postEndplateSacrum, femHeadLeft, femHeadRight);
                        //double SSvalue = PIvalue - PTvalue;

                        dataTable.Rows.Add(SVAvalue.ToString().Replace(',', '.'), CorValue.ToString().Replace(',', '.'), value3D.ToString().Replace(',', '.'));
                    }
                    SVAdistDTlist.Add(dataTable);
                }
            }

        }

        private void CalculateSPI(System.Windows.Media.Media3D.Point3D femHeadLeft, System.Windows.Media.Media3D.Point3D femHeadRight, System.Windows.Media.Media3D.Point3D T1_body, string Name,  bool Visualize, out double SPI_sag_angle, out double SPI_cor_angle, out double SPI_3D_angle, out System.Windows.Media.Media3D.Point3D T1Rotp)
        {
            System.Windows.Media.Media3D.Point3D point0 = new System.Windows.Media.Media3D.Point3D(T1_body.X, 0, T1_body.Z);
            System.Windows.Media.Media3D.Point3D point1 = new System.Windows.Media.Media3D.Point3D(femHeadLeft.X, 0, femHeadLeft.Z);
            System.Windows.Media.Media3D.Point3D point2 = new System.Windows.Media.Media3D.Point3D(femHeadRight.X, 0, femHeadRight.Z);
          
            System.Numerics.Vector2 p1 = new System.Numerics.Vector2((float)point1.X, (float)point1.Z); //Left Fem head
            System.Numerics.Vector2 p2 = new System.Numerics.Vector2((float)point2.X, (float)point2.Z); //Right Fem head
            System.Numerics.Vector2 p0 = new System.Numerics.Vector2((float)point0.X, (float)point0.Z); //T1

            System.Numerics.Vector2 s = p2 - p1;
            System.Numerics.Vector2 v = p0 - p1;

            double len = System.Numerics.Vector2.Dot(v, s) / System.Numerics.Vector2.Dot(s, s);
            System.Numerics.Vector2 projection = System.Numerics.Vector2.Multiply((float)len, s);

            T1Rotp = new System.Windows.Media.Media3D.Point3D(point1.X + projection.X, (femHeadRight.Y + femHeadLeft.Y) / 2, point1.Z + projection.Y);


            SPI_sag_angle = AngleBetweenThreePoints(T1Rotp, new System.Windows.Media.Media3D.Point3D(T1_body.X, T1_body.Y, T1_body.Z), new System.Windows.Media.Media3D.Point3D(T1_body.X, T1Rotp.Y, T1_body.Z));
            SPI_cor_angle = AngleBetweenThreePoints(T1Rotp, new System.Windows.Media.Media3D.Point3D(T1Rotp.X, T1_body.Y, T1Rotp.Z), new System.Windows.Media.Media3D.Point3D((femHeadRight.X + femHeadLeft.X) / 2, (femHeadRight.Y + femHeadLeft.Y) / 2, (femHeadRight.Z + femHeadLeft.Z) / 2));
         

            System.Windows.Media.Media3D.Vector3D vectorML = femHeadRight- femHeadLeft;
            System.Windows.Media.Media3D.Vector3D vectorPA = T1Rotp - new System.Windows.Media.Media3D.Point3D(T1_body.X, T1Rotp.Y, T1_body.Z);
            
            vectorML.Normalize();
            vectorPA.Normalize();
           

            System.Windows.Media.Media3D.Vector3D crossProductVert = System.Windows.Media.Media3D.Vector3D.CrossProduct(vectorML, vectorPA);
            
            crossProductVert.Normalize();

            SPI_sag_angle = SPI_sag_angle * Math.Sign(crossProductVert.Y);

            if(DistBetween2points(T1Rotp,femHeadLeft) > DistBetween2points(T1Rotp, femHeadRight)) //Angle to the right is always positive.
            {
                SPI_cor_angle = -SPI_cor_angle;
            }
            
            SPI_3D_angle = AngleBetweenThreePoints(T1Rotp, T1_body, new System.Windows.Media.Media3D.Point3D(T1_body.X, T1Rotp.Y, T1_body.Z));

           if(Visualize)
            {

                PlumbLine(femHeadLeft, femHeadRight, 0, 1, 0);
                Spherevisualisation(femHeadLeft, 0, 1, 0, "Fem. Left", 2.5);
                Spherevisualisation(femHeadRight, 0, 1, 0, "Fem. Right", 2.5);

                Spherevisualisation(T1_body, 1, 0, 0, Name, 1);
                PlumbLine(T1_body, new System.Windows.Media.Media3D.Point3D(T1_body.X, T1_body.Y - 0.65, T1_body.Z), 1, 0, 0);
                PlumbLine(new System.Windows.Media.Media3D.Point3D((femHeadLeft.X + femHeadRight.X) / 2, (femHeadLeft.Y + femHeadRight.Y) / 2, (femHeadLeft.Z + femHeadRight.Z) / 2), T1_body, 0, 0, 0);
                PlumbLine(new System.Windows.Media.Media3D.Point3D((femHeadLeft.X + femHeadRight.X) / 2, (femHeadLeft.Y + femHeadRight.Y) / 2, (femHeadLeft.Z + femHeadRight.Z) / 2), T1Rotp, 0, 0, 0);
                PlumbLine(new System.Windows.Media.Media3D.Point3D(T1_body.X, T1Rotp.Y, T1_body.Z), T1Rotp, 0, 0, 1);
                PlumbLine(T1_body, T1Rotp, 0, 0, 1);


                txtCurrentValue.Text = Name + "-SPI (sag): " + SPI_sag_angle.ToString() + '\t' + Name + "-SPI (cor): " + SPI_cor_angle.ToString() + '\t' + Name + "-SPI (3D, Abs): " + SPI_3D_angle.ToString();



                ren1.GetRenderWindow().Render();
            }

        }

        public static double AngleBetween(System.Numerics.Vector2 vector1, System.Numerics.Vector2 vector2)
        {
            double sin = vector1.X * vector2.Y - vector2.X * vector1.Y;
            double cos = vector1.X * vector2.X + vector1.Y * vector2.Y;

            return Math.Atan2(sin, cos) * (180 / Math.PI);
        }

        // Calculate the distance between
        // point pt and the segment p1 --> p2.
        private double FindDistanceToSegment(PointF pt, PointF p1, PointF p2, out PointF closest)
        {
            float dx = p2.X - p1.X;
            float dy = p2.Y - p1.Y;
            if ((dx == 0) && (dy == 0))
            {
                // It's a point not a line segment.
                closest = p1;
                dx = pt.X - p1.X;
                dy = pt.Y - p1.Y;
                return Math.Sqrt(dx * dx + dy * dy);
            }

            // Calculate the t that minimizes the distance.
            float t = ((pt.X - p1.X) * dx + (pt.Y - p1.Y) * dy) /
                (dx * dx + dy * dy);

            // See if this represents one of the segment's
            // end points or a point in the middle.
            if (t < 0)
            {
                closest = new PointF(p1.X, p1.Y);
                dx = pt.X - p1.X;
                dy = pt.Y - p1.Y;
            }
            else if (t > 1)
            {
                closest = new PointF(p2.X, p2.Y);
                dx = pt.X - p2.X;
                dy = pt.Y - p2.Y;
            }
            else
            {
                closest = new PointF(p1.X + t * dx, p1.Y + t * dy);
                dx = pt.X - closest.X;
                dy = pt.Y - closest.Y;
            }

            return Math.Sqrt(dx * dx + dy * dy);
        }

        private System.Windows.Media.Media3D.Point3D ProjectLatestMethod(System.Windows.Media.Media3D.Point3D LeftFem, System.Windows.Media.Media3D.Point3D RightFem, System.Windows.Media.Media3D.Point3D PointInQ)
        {
            System.Windows.Media.Media3D.Point3D point0 = new System.Windows.Media.Media3D.Point3D(PointInQ.X, 0, PointInQ.Z);
            System.Windows.Media.Media3D.Point3D point1 = new System.Windows.Media.Media3D.Point3D(LeftFem.X, 0, LeftFem.Z);
            System.Windows.Media.Media3D.Point3D point2 = new System.Windows.Media.Media3D.Point3D(RightFem.X, 0, RightFem.Z);


            System.Numerics.Vector2 p1 = new System.Numerics.Vector2((float)point1.X, (float)point1.Z); //Left Fem head
            System.Numerics.Vector2 p2 = new System.Numerics.Vector2((float)point2.X, (float)point2.Z); //Right Fem head
            System.Numerics.Vector2 p0 = new System.Numerics.Vector2((float)point0.X, (float)point0.Z); //T1

            System.Numerics.Vector2 s = p2 - p1;
            System.Numerics.Vector2 v = p0 - p1;

            double len = System.Numerics.Vector2.Dot(v, s) / System.Numerics.Vector2.Dot(s, s);
            System.Numerics.Vector2 projection = System.Numerics.Vector2.Multiply((float)len, s);

            System.Windows.Media.Media3D.Point3D T1Rotp = new System.Windows.Media.Media3D.Point3D(point1.X + projection.X, PointInQ.Y , point1.Z + projection.Y);
            return T1Rotp;

        }

        private System.Windows.Media.Media3D.Point3D ProjectPerpendicularLatestMethod(System.Windows.Media.Media3D.Point3D LeftFem, System.Windows.Media.Media3D.Point3D RightFem, System.Windows.Media.Media3D.Point3D PointOnFemLine, System.Windows.Media.Media3D.Point3D PointInQ)
        {
            System.Windows.Media.Media3D.Point3D point0 = new System.Windows.Media.Media3D.Point3D(PointInQ.X, 0, PointInQ.Z);
            System.Windows.Media.Media3D.Point3D point1 = new System.Windows.Media.Media3D.Point3D(LeftFem.X, 0, LeftFem.Z);
            System.Windows.Media.Media3D.Point3D point2 = new System.Windows.Media.Media3D.Point3D(RightFem.X, 0, RightFem.Z);


            System.Numerics.Vector2 p1 = new System.Numerics.Vector2((float)point1.X, (float)point1.Z); //Left Fem head
            System.Numerics.Vector2 p2 = new System.Numerics.Vector2((float)point2.X, (float)point2.Z); //Right Fem head
            System.Numerics.Vector2 p0 = new System.Numerics.Vector2((float)point0.X, (float)point0.Z); //T1
            System.Numerics.Vector2 p4 = new System.Numerics.Vector2((float)PointOnFemLine.X, (float)PointOnFemLine.Z); //T1


            System.Numerics.Vector2 s = p2 - p1;
            System.Numerics.Vector2 sn = System.Numerics.Vector2.Normalize(s);
            System.Numerics.Vector2 sp = new System.Numerics.Vector2(-sn.Y, sn.X);
            System.Numerics.Vector2 v = p0-p4;

            double len = System.Numerics.Vector2.Dot(sp, v) / System.Numerics.Vector2.Dot(sp, sp);
            System.Numerics.Vector2 projection = System.Numerics.Vector2.Multiply((float)len, sp);

            System.Windows.Media.Media3D.Point3D T1Rotp = new System.Windows.Media.Media3D.Point3D(PointOnFemLine.X + projection.X, PointInQ.Y, PointOnFemLine.Z + projection.Y);
            return T1Rotp;

        }

        private System.Windows.Media.Media3D.Point3D ProjectPerpendicularHeightLatestMethod(System.Windows.Media.Media3D.Point3D LeftFem, System.Windows.Media.Media3D.Point3D RightFem, System.Windows.Media.Media3D.Point3D PointOnFemLine, System.Windows.Media.Media3D.Point3D PointInQ)
        {
            System.Windows.Media.Media3D.Point3D point0 = new System.Windows.Media.Media3D.Point3D(0, PointInQ.Y, PointInQ.Z);
            System.Windows.Media.Media3D.Point3D point1 = new System.Windows.Media.Media3D.Point3D(0, LeftFem.Y, LeftFem.Z);
            System.Windows.Media.Media3D.Point3D point2 = new System.Windows.Media.Media3D.Point3D(0, RightFem.Y, RightFem.Z);


            System.Numerics.Vector2 p1 = new System.Numerics.Vector2((float)point1.Y, (float)point1.Z); //Left Fem head
            System.Numerics.Vector2 p2 = new System.Numerics.Vector2((float)point2.Y, (float)point2.Z); //Right Fem head
            System.Numerics.Vector2 p0 = new System.Numerics.Vector2((float)point0.Y, (float)point0.Z); //T1

            
            System.Numerics.Vector2 s = p2 - p1;
            System.Numerics.Vector2 v = p0 - p1;

            double len = System.Numerics.Vector2.Dot(v, s) / System.Numerics.Vector2.Dot(s, s);
            System.Numerics.Vector2 projection = System.Numerics.Vector2.Multiply((float)len, s);

            System.Windows.Media.Media3D.Point3D T1Rotp = new System.Windows.Media.Media3D.Point3D(PointInQ.X, point1.Y + projection.X, point1.Z + projection.Y);
            return T1Rotp;

        }

        private void CalculateCobb(System.Windows.Media.Media3D.Point3D T12Left, System.Windows.Media.Media3D.Point3D T12Right, System.Windows.Media.Media3D.Point3D T1Left, System.Windows.Media.Media3D.Point3D T1Right, System.Windows.Media.Media3D.Point3D femHeadLeft, System.Windows.Media.Media3D.Point3D femHeadRight, out double Angle)
        {
            System.Windows.Media.Media3D.Point3D T12LeftP = ProjectLatestMethod(femHeadLeft, femHeadRight, T12Left);
            System.Windows.Media.Media3D.Point3D T12RightP = ProjectLatestMethod(femHeadLeft, femHeadRight, T12Right);
            System.Windows.Media.Media3D.Point3D T1LeftP = ProjectLatestMethod(femHeadLeft, femHeadRight, T1Left);
            System.Windows.Media.Media3D.Point3D T1RightP = ProjectLatestMethod(femHeadLeft, femHeadRight, T1Right);

            double LL_AngleRad = Angle2DBetweenFourPoints3(T12LeftP, T12RightP, T1LeftP, T1RightP);

            Angle = LL_AngleRad * (180 / Math.PI);

            return;

            System.Windows.Media.Media3D.Point3D point1 = new System.Windows.Media.Media3D.Point3D(femHeadLeft.X, 0, femHeadLeft.Z);
            System.Windows.Media.Media3D.Point3D point2 = new System.Windows.Media.Media3D.Point3D(femHeadRight.X, 0, femHeadRight.Z);

            //Calculate femur axis angle
            double Femdist = DistBetween2points(point1, point2);
            double axisNorm = femHeadRight.X - femHeadLeft.X;
            double angleref = Math.Asin(axisNorm / Femdist) * (180 / Math.PI);

            if (femHeadRight.Z < femHeadLeft.Z)
            {
                angleref += 180;
            }
            double T12LeftRotX;
            double T12LeftRotY;
            Rotate(T12Left.X, T12Left.Z, (femHeadRight.X + femHeadLeft.X) / 2, (femHeadRight.Z + femHeadLeft.Z) / 2, angleref, out T12LeftRotX, out T12LeftRotY);
            T12LeftRotX = T12LeftRotX - ((femHeadRight.X + femHeadLeft.X) / 2);
            T12LeftRotY = T12LeftRotY - ((femHeadRight.Z + femHeadLeft.Z) / 2);

            double T12RightRotX;
            double T12RightRotY;
            Rotate(T12Right.X, T12Right.Z, (femHeadRight.X + femHeadLeft.X) / 2, (femHeadRight.Z + femHeadLeft.Z) / 2, angleref, out T12RightRotX, out T12RightRotY);
            T12RightRotX = T12RightRotX - ((femHeadRight.X + femHeadLeft.X) / 2);
            T12RightRotY = T12RightRotY - ((femHeadRight.Z + femHeadLeft.Z) / 2);

            double T1LeftRotX;
            double T1LeftRotY;
            Rotate(T1Left.X, T1Left.Z, (femHeadRight.X + femHeadLeft.X) / 2, (femHeadRight.Z + femHeadLeft.Z) / 2, angleref, out T1LeftRotX, out T1LeftRotY);
            T1LeftRotX = T1LeftRotX - ((femHeadRight.X + femHeadLeft.X) / 2);
            T1LeftRotY = T1LeftRotY - ((femHeadRight.Z + femHeadLeft.Z) / 2);

            double T1RightRotX;
            double T1RightRotY;
            Rotate(T1Right.X, T1Right.Z, (femHeadRight.X + femHeadLeft.X) / 2, (femHeadRight.Z + femHeadLeft.Z) / 2, angleref, out T1RightRotX, out T1RightRotY);
            T1RightRotX = T1RightRotX - ((femHeadRight.X + femHeadLeft.X) / 2);
            T1RightRotY = T1RightRotY - ((femHeadRight.Z + femHeadLeft.Z) / 2);


            System.Windows.Media.Media3D.Point3D pointR1 = new System.Windows.Media.Media3D.Point3D(0, T12Left.Y, T12LeftRotY);
            System.Windows.Media.Media3D.Point3D pointR2 = new System.Windows.Media.Media3D.Point3D(0, T12Right.Y, T12RightRotY);
            System.Windows.Media.Media3D.Point3D pointR3 = new System.Windows.Media.Media3D.Point3D(0, T1Left.Y, T1LeftRotY);
            System.Windows.Media.Media3D.Point3D pointR4 = new System.Windows.Media.Media3D.Point3D(0, T1Right.Y, T1RightRotY);

            double LL_AngleRad2 = Angle2DBetweenFourPoints(pointR1, pointR2, pointR3, pointR4);

            Angle = LL_AngleRad2 * (180 / Math.PI);

        }

        private void CalculateLL(System.Windows.Media.Media3D.Point3D T12Ant, System.Windows.Media.Media3D.Point3D T12Post, System.Windows.Media.Media3D.Point3D S1Ant, System.Windows.Media.Media3D.Point3D S1Post, System.Windows.Media.Media3D.Point3D femHeadLeft, System.Windows.Media.Media3D.Point3D femHeadRight, out double LL_Angle)
        {
            System.Windows.Media.Media3D.Point3D point1 = new System.Windows.Media.Media3D.Point3D(femHeadLeft.X, 0, femHeadLeft.Z);
            System.Windows.Media.Media3D.Point3D point2 = new System.Windows.Media.Media3D.Point3D(femHeadRight.X, 0, femHeadRight.Z);

            //Calculate femur axis angle
            double Femdist = DistBetween2points(point1, point2);
            double axisNorm = femHeadRight.X - femHeadLeft.X;
            double angleref = Math.Asin(axisNorm / Femdist) * (180 / Math.PI);

            if (femHeadRight.Z < femHeadLeft.Z)
            {
                angleref += 180;
            }

            double T12AntRotX;
            double T12AntRotY;
            Rotate(T12Ant.X, T12Ant.Z, (femHeadRight.X + femHeadLeft.X) / 2, (femHeadRight.Z + femHeadLeft.Z) / 2, angleref, out T12AntRotX, out T12AntRotY);
            T12AntRotX = T12AntRotX - ((femHeadRight.X + femHeadLeft.X) / 2);
            T12AntRotY = T12AntRotY - ((femHeadRight.Z + femHeadLeft.Z) / 2);

            double T12PostRotX;
            double T12PostRotY;
            Rotate(T12Post.X, T12Post.Z, (femHeadRight.X + femHeadLeft.X) / 2, (femHeadRight.Z + femHeadLeft.Z) / 2, angleref, out T12PostRotX, out T12PostRotY);
            T12PostRotX = T12PostRotX - ((femHeadRight.X + femHeadLeft.X) / 2);
            T12PostRotY = T12PostRotY - ((femHeadRight.Z + femHeadLeft.Z) / 2);

            double S1AntRotX;
            double S1AntRotY;
            Rotate(S1Ant.X, S1Ant.Z, (femHeadRight.X + femHeadLeft.X) / 2, (femHeadRight.Z + femHeadLeft.Z) / 2, angleref, out S1AntRotX, out S1AntRotY);
            S1AntRotX = S1AntRotX - ((femHeadRight.X + femHeadLeft.X) / 2);
            S1AntRotY = S1AntRotY - ((femHeadRight.Z + femHeadLeft.Z) / 2);

            double S1PostRotX;
            double S1PostRotY;
            Rotate(S1Post.X, S1Post.Z, (femHeadRight.X + femHeadLeft.X) / 2, (femHeadRight.Z + femHeadLeft.Z) / 2, angleref, out S1PostRotX, out S1PostRotY);
            S1PostRotX = S1PostRotX - ((femHeadRight.X + femHeadLeft.X) / 2);
            S1PostRotY = S1PostRotY - ((femHeadRight.Z + femHeadLeft.Z) / 2);


            System.Windows.Media.Media3D.Point3D pointR1 = new System.Windows.Media.Media3D.Point3D(S1PostRotX, S1Post.Y, 0);
            System.Windows.Media.Media3D.Point3D pointR2 = new System.Windows.Media.Media3D.Point3D(S1AntRotX, S1Ant.Y, 0);
            System.Windows.Media.Media3D.Point3D pointR3 = new System.Windows.Media.Media3D.Point3D(T12PostRotX, T12Post.Y, 0);
            System.Windows.Media.Media3D.Point3D pointR4 = new System.Windows.Media.Media3D.Point3D(T12AntRotX, T12Ant.Y, 0);


            double LL_AngleRad = Angle2DBetweenFourPoints2(pointR1, pointR2, pointR3, pointR4);

            LL_Angle = LL_AngleRad * (180 / Math.PI);
        }

        private void CalculateSVA(System.Windows.Media.Media3D.Point3D postEndplateSacrum, System.Windows.Media.Media3D.Point3D femHeadLeft, System.Windows.Media.Media3D.Point3D femHeadRight, System.Windows.Media.Media3D.Point3D C7_Body, bool Visualize, out double svaDist, out double corDist, out double value3D, out System.Windows.Media.Media3D.Point3D C7Rotp, out System.Windows.Media.Media3D.Point3D SacrRotp)
        {
            //Project all points in to the ground plane  (Y=0)
            System.Windows.Media.Media3D.Point3D point0 = new System.Windows.Media.Media3D.Point3D(postEndplateSacrum.X, 0, postEndplateSacrum.Z);
            System.Windows.Media.Media3D.Point3D point1 = new System.Windows.Media.Media3D.Point3D(femHeadLeft.X, 0, femHeadLeft.Z);
            System.Windows.Media.Media3D.Point3D point2 = new System.Windows.Media.Media3D.Point3D(femHeadRight.X, 0, femHeadRight.Z);
            System.Windows.Media.Media3D.Point3D point4 = new System.Windows.Media.Media3D.Point3D(C7_Body.X, 0, C7_Body.Z);

            value3D = DistBetween2points(point0, point4);

            //Calculate femur axis angle
            double Femdist = DistBetween2points(point1, point2);
            double axisNorm = femHeadRight.X - femHeadLeft.X;

            double angleref = Math.Asin(axisNorm / Femdist) * (180 / Math.PI);

            if (femHeadRight.Z < femHeadLeft.Z)
            {
                angleref += 180;
            }
            PointF sacr = new PointF();
            PointF C7 = new PointF();

            double sacrRotX;
            double sacrRotY;
            double C7RotX;
            double C7RotY;

            

            Rotate(postEndplateSacrum.X, postEndplateSacrum.Z, (femHeadRight.X + femHeadLeft.X) / 2, (femHeadRight.Z + femHeadLeft.Z) / 2, angleref, out sacrRotX, out sacrRotY);
            sacrRotX = sacrRotX - ((femHeadRight.X + femHeadLeft.X) / 2);
            sacrRotY = sacrRotY - ((femHeadRight.Z + femHeadLeft.Z) / 2);

            Rotate(C7_Body.X, C7_Body.Z, (femHeadRight.X + femHeadLeft.X) / 2, (femHeadRight.Z + femHeadLeft.Z) / 2, angleref, out C7RotX, out C7RotY);
            C7RotX = C7RotX - ((femHeadRight.X + femHeadLeft.X) / 2);
            C7RotY = C7RotY - ((femHeadRight.Z + femHeadLeft.Z) / 2);

            svaDist = C7RotX - sacrRotX;
            corDist = C7RotY - sacrRotY;


            double C7RotXp;
            double C7RotYp;
            double angle2ref = Math.Acos(corDist / svaDist) * (180 / Math.PI);

           


         
            Project(C7_Body.X, C7_Body.Z, postEndplateSacrum.X, postEndplateSacrum.Z, angle2ref, out C7RotXp, out C7RotYp);

            C7Rotp = new System.Windows.Media.Media3D.Point3D(C7RotXp, postEndplateSacrum.Y, C7RotYp);



            System.Numerics.Vector2 p1 = new System.Numerics.Vector2((float)point1.X, (float)point1.Z); //Left Fem head
            System.Numerics.Vector2 p2 = new System.Numerics.Vector2((float)point2.X, (float)point2.Z); //Right Fem head
            System.Numerics.Vector2 p0 = new System.Numerics.Vector2((float)point0.X, (float)point0.Z); //Sacrum

            System.Numerics.Vector2 s = p2 - p1;
            System.Numerics.Vector2 v = p0 - p1;

            double len = System.Numerics.Vector2.Dot(v, s) / System.Numerics.Vector2.Dot(s, s);
            System.Numerics.Vector2 projection = System.Numerics.Vector2.Multiply((float)len, s);

            SacrRotp = new System.Windows.Media.Media3D.Point3D(point1.X + projection.X, postEndplateSacrum.Y, point1.Z + projection.Y);

            System.Numerics.Vector2 p4= new System.Numerics.Vector2((float)point4.X, (float)point4.Z); //C7
            System.Numerics.Vector2 v2 = p4 - p1;
            double len2 = System.Numerics.Vector2.Dot(v2, s) / System.Numerics.Vector2.Dot(s, s);
            System.Numerics.Vector2 projection2 = System.Numerics.Vector2.Multiply((float)(len2), s);

            C7Rotp  = new System.Windows.Media.Media3D.Point3D(point1.X + projection2.X, postEndplateSacrum.Y, point1.Z + projection2.Y);

            //double dist = DistBetween2points(SacrRotp, postEndplateSacrum) + DistBetween2points(C7Rotp, new System.Windows.Media.Media3D.Point3D(C7_Body.X, postEndplateSacrum.Y, C7_Body.Z));


            System.Numerics.Vector2 psacr = new System.Numerics.Vector2((float)postEndplateSacrum.X, (float)postEndplateSacrum.Z); 
            System.Numerics.Vector2 psacrBase = new System.Numerics.Vector2((float)SacrRotp.X, (float)SacrRotp.Z); 
            System.Numerics.Vector2 pC7 = new System.Numerics.Vector2((float)C7_Body.X, (float)C7_Body.Z); 
            System.Numerics.Vector2 pC7Base = new System.Numerics.Vector2((float)C7Rotp.X, (float)C7Rotp.Z);


            System.Numerics.Vector2 SacrV = psacrBase- psacr;
 

            System.Numerics.Vector2 C7V = pC7 - pC7Base;
            System.Numerics.Vector2 VecSum = SacrV + C7V; // System.Numerics.Vector2.Add(C7V, SacrV);

            System.Windows.Media.Media3D.Vector3D vectorML = femHeadRight - femHeadLeft;
            System.Windows.Media.Media3D.Vector3D vectorPA = new System.Windows.Media.Media3D.Point3D(psacrBase.X, (femHeadRight.Y + femHeadLeft.Y)/2, psacrBase.Y) - postEndplateSacrum;

            vectorML.Normalize();
            vectorPA.Normalize();


            System.Windows.Media.Media3D.Vector3D crossProductVert = System.Windows.Media.Media3D.Vector3D.CrossProduct(vectorML, vectorPA);

            crossProductVert.Normalize();

            //return;

           double OthersvaDist = VecSum.Length() * Math.Sign(crossProductVert.Y);
            System.Numerics.Vector2 corVec = psacrBase - pC7Base;
            //corDist = corVec.Length() * Math.Sign(corVec.Y); // DistBetween2points(SacrRotp, C7Rotp);


            //if (DistBetween2points(C7Rotp, femHeadLeft) < DistBetween2points(SacrRotp, femHeadLeft)) //Angle to the right is always positive.
            //{
            //    corDist = - Math.Abs(corDist);
            //}


            if (Visualize)
            {
                //C7 plumb-line
                Spherevisualisation(C7_Body, 1, 0, 0, "C7", 1);
                PlumbLine(C7_Body, new System.Windows.Media.Media3D.Point3D(C7_Body.X, C7_Body.Y - 0.6, C7_Body.Z), 1, 0, 0);
                PlumbLine(postEndplateSacrum, new System.Windows.Media.Media3D.Point3D(C7_Body.X, postEndplateSacrum.Y, C7_Body.Z), 0, 0, 0);
                PlumbLine(postEndplateSacrum, new System.Windows.Media.Media3D.Point3D(SacrRotp.X, SacrRotp.Y, SacrRotp.Z), 0, 0, 0);
                PlumbLine(new System.Windows.Media.Media3D.Point3D(C7_Body.X, postEndplateSacrum.Y, C7_Body.Z), new System.Windows.Media.Media3D.Point3D(C7Rotp.X, postEndplateSacrum.Y, C7Rotp.Z), 0, 0, 0);
                Spherevisualisation(femHeadLeft, 0, 1, 0, "Fem. Left", 2);
                Spherevisualisation(femHeadRight, 0, 1, 0, "Fem. Right", 2);
                PlumbLine(femHeadLeft, femHeadRight, 0, 1, 0);
                Spherevisualisation(postEndplateSacrum, 0, 0, 1, "Post. End Sacr.", 1);

                //2D stuff
                double femLeftRotX;
                double femLeftRotY;
                Rotate(femHeadLeft.X, femHeadLeft.Z, (femHeadRight.X + femHeadLeft.X) / 2, (femHeadRight.Z + femHeadLeft.Z) / 2, angleref, out femLeftRotX, out femLeftRotY);
                femLeftRotX = femLeftRotX - ((femHeadRight.X + femHeadLeft.X) / 2);
                femLeftRotY = femLeftRotY - ((femHeadRight.Z + femHeadLeft.Z) / 2);

                double femRightRotX;
                double femRightRotY;
                Rotate(femHeadRight.X, femHeadRight.Z, (femHeadRight.X + femHeadLeft.X) / 2, (femHeadRight.Z + femHeadLeft.Z) / 2, angleref, out femRightRotX, out femRightRotY);
                femRightRotX = femRightRotX - ((femHeadRight.X + femHeadLeft.X) / 2);
                femRightRotY = femRightRotY - ((femHeadRight.Z + femHeadLeft.Z) / 2);

                Spherevisualisation(new System.Windows.Media.Media3D.Point3D(femLeftRotX, 0, femLeftRotY), 0, 1, 0, "Fem. Left", 2);
                Spherevisualisation(new System.Windows.Media.Media3D.Point3D(femRightRotX, 0, femRightRotY), 0, 1, 0, "Fem. Right", 2);
                Spherevisualisation(new System.Windows.Media.Media3D.Point3D(sacrRotX, 0, sacrRotY), 1, 0, 0, "Sacr", 1);
                Spherevisualisation(new System.Windows.Media.Media3D.Point3D(C7RotX, 0, C7RotY), 1, 0, 0, "C7", 1);

                PlumbLine(new System.Windows.Media.Media3D.Point3D(femLeftRotX, 0, femLeftRotY), new System.Windows.Media.Media3D.Point3D(femRightRotX, 0, femRightRotY), 0, 1, 0);
                PlumbLine(new System.Windows.Media.Media3D.Point3D(sacrRotX, 0, sacrRotY), new System.Windows.Media.Media3D.Point3D(C7RotX, 0, sacrRotY), 0, 1, 0);
                PlumbLine(new System.Windows.Media.Media3D.Point3D(sacrRotX, 0, sacrRotY), new System.Windows.Media.Media3D.Point3D(C7RotX, 0, C7RotY), 1, 0, 0);

                //Print out values
                txtCurrentValue.Text = "Sagittal VA: " + svaDist.ToString() + '\t' + "Coronal VA: " + corDist.ToString() + '\t' + "3D VA: " + value3D.ToString();

                txtCurrentValue.AppendText(Environment.NewLine + "Sagittal SVA Other: " + OthersvaDist.ToString());
                txtCurrentValue.AppendText(Environment.NewLine + "Angle Ref: " + angleref.ToString());
                txtCurrentValue.AppendText(Environment.NewLine + "Sacr Rot (X and Y): " + sacrRotX.ToString() + "  " + sacrRotY.ToString());

                ren1.GetRenderWindow().Render();
            }
        }

        public static void Rotate(double pointX, double pointY, double pivotX, double pivotY, double angleDegree, out double rotX, out double rotY)
        {
            double angle = angleDegree * Math.PI / 180;
            double cos = Math.Cos(angle);
            double sin = Math.Sin(angle);
            double dx = pointX - pivotX;
            double dy = pointY - pivotY;
            rotX = cos * dx - sin * dy + pivotX;
            rotY = sin * dx + cos * dy + pivotY;

        }
        public static void Project(double pointX, double pointY, double pivotX, double pivotY, double angleDegree, out double rotX, out double rotY)
        {
            double angle = angleDegree * Math.PI / 180;
            double cos = Math.Cos(angle);
            double sin = Math.Sin(angle);
            double dx = pointX - pivotX;
            double dy = pointY - pivotY;

            double dist = Math.Sqrt((dx * dx) + (dy * dy));
            double newLength = dist * Math.Cos(angle);

            double rotXt = cos * dx - sin * dy + pivotX;
            double rotYt = sin * dx + cos * dy + pivotY;

            double beta = Math.Atan((rotYt - pivotY) / (rotXt - pivotX));
            //double beta = Math.Atan2((rotXt - pivotX), (rotYt - pivotY));

            rotX = newLength * Math.Cos(beta) + pivotX;
            rotY = newLength * Math.Sin(beta) + pivotY;
        }

        vtkAngleWidget vtkAngleWidget = new vtkAngleWidget();

        private void CalculatePelvicAnglesSagProj(System.Windows.Media.Media3D.Point3D AntEndplateSacrum, System.Windows.Media.Media3D.Point3D postEndplateSacrum, System.Windows.Media.Media3D.Point3D femHeadLeft, System.Windows.Media.Media3D.Point3D femHeadRight, bool Visualize, out double PIangle, out double PT_sag_angle, out double SS_sag_angle, out System.Windows.Media.Media3D.Point3D SacrMidp, out System.Windows.Media.Media3D.Point3D SacrPostMidp)
        {
            // Project onto the Transverse plane (y=0) to determine the orientation
            System.Windows.Media.Media3D.Point3D point0 = new System.Windows.Media.Media3D.Point3D((AntEndplateSacrum.X + postEndplateSacrum.X)/2, 0, (AntEndplateSacrum.Z + postEndplateSacrum.Z) / 2);
            System.Windows.Media.Media3D.Point3D point1 = new System.Windows.Media.Media3D.Point3D(femHeadLeft.X, 0, femHeadLeft.Z);
            System.Windows.Media.Media3D.Point3D point2 = new System.Windows.Media.Media3D.Point3D(femHeadRight.X, 0, femHeadRight.Z);


            System.Numerics.Vector2 p1 = new System.Numerics.Vector2((float)point1.X, (float)point1.Z); //Left Fem head
            System.Numerics.Vector2 p2 = new System.Numerics.Vector2((float)point2.X, (float)point2.Z); //Right Fem head
            System.Numerics.Vector2 p0 = new System.Numerics.Vector2((float)point0.X, (float)point0.Z); //SacrMid

            System.Numerics.Vector2 s = p2 - p1;
            System.Numerics.Vector2 v = p0 - p1;

            double len = System.Numerics.Vector2.Dot(v, s) / System.Numerics.Vector2.Dot(s, s);
            System.Numerics.Vector2 projection = System.Numerics.Vector2.Multiply((float)len, s);

            SacrMidp = new System.Windows.Media.Media3D.Point3D(point1.X + projection.X, (femHeadRight.Y + femHeadLeft.Y) / 2, point1.Z + projection.Y);
            SacrMidp = ProjectPerpendicularHeightLatestMethod(femHeadLeft, femHeadRight, new System.Windows.Media.Media3D.Point3D((femHeadLeft.X + femHeadRight.X) / 2, (femHeadLeft.Y + femHeadRight.Y) / 2, (femHeadLeft.Z + femHeadRight.Z) / 2), SacrMidp);

            System.Windows.Media.Media3D.Point3D SacrAvg = new System.Windows.Media.Media3D.Point3D((AntEndplateSacrum.X + postEndplateSacrum.X) / 2, (AntEndplateSacrum.Y + postEndplateSacrum.Y) / 2, (AntEndplateSacrum.Z + postEndplateSacrum.Z) / 2);

            PT_sag_angle = AngleBetweenThreePoints(new System.Windows.Media.Media3D.Point3D(SacrMidp.X, SacrMidp.Y + 10, SacrMidp.Z), SacrMidp, SacrAvg);
            // SPI_cor_angle = AngleBetweenThreePoints(T1Rotp, new System.Windows.Media.Media3D.Point3D(T1Rotp.X, T1_body.Y, T1Rotp.Z), new System.Windows.Media.Media3D.Point3D((femHeadRight.X + femHeadLeft.X) / 2, (femHeadRight.Y + femHeadLeft.Y) / 2, (femHeadRight.Z + femHeadLeft.Z) / 2));


            System.Windows.Media.Media3D.Vector3D vectorML = femHeadRight - femHeadLeft;
            System.Windows.Media.Media3D.Vector3D vectorPA = SacrMidp - SacrAvg;

            vectorML.Normalize();
            vectorPA.Normalize();


            System.Windows.Media.Media3D.Vector3D crossProductVert = System.Windows.Media.Media3D.Vector3D.CrossProduct(vectorML, vectorPA);

            crossProductVert.Normalize();

            PT_sag_angle = PT_sag_angle * Math.Sign(crossProductVert.Y);


            int crosproductSign = Math.Sign(crossProductVert.Y);





            System.Windows.Media.Media3D.Point3D point0s = new System.Windows.Media.Media3D.Point3D(postEndplateSacrum.X, 0, postEndplateSacrum.Z);




            System.Numerics.Vector2 p0s = new System.Numerics.Vector2((float)point0s.X, (float)point0s.Z); //SacrMid

            System.Numerics.Vector2 vs = p0s - p1;

            double lens = System.Numerics.Vector2.Dot(vs, s) / System.Numerics.Vector2.Dot(s, s);
            System.Numerics.Vector2 projections = System.Numerics.Vector2.Multiply((float)lens, s);

            SacrPostMidp = new System.Windows.Media.Media3D.Point3D(point1.X + projections.X, (femHeadRight.Y + femHeadLeft.Y) / 2, point1.Z + projections.Y);

           
            //SS_sag_angle = AngleBetweenThreePoints(new System.Windows.Media.Media3D.Point3D(SacrPostMidp.X, postEndplateSacrum.Y, SacrPostMidp.Z), postEndplateSacrum, new System.Windows.Media.Media3D.Point3D(AntEndplateSacrum.X, AntEndplateSacrum.Y, SacrPostMidp.Z));

            if ((postEndplateSacrum.Z - SacrPostMidp.Z)>0)
            {
                SS_sag_angle = AngleBetweenThreePoints(new System.Windows.Media.Media3D.Point3D(SacrPostMidp.X, postEndplateSacrum.Y, SacrPostMidp.Z), postEndplateSacrum, ProjectPerpendicularLatestMethod(femHeadLeft, femHeadRight, SacrPostMidp, AntEndplateSacrum));
            }
            else
            {
                SS_sag_angle = AngleBetweenThreePoints(postEndplateSacrum, new System.Windows.Media.Media3D.Point3D(SacrPostMidp.X, postEndplateSacrum.Y, SacrPostMidp.Z), ProjectPerpendicularLatestMethod(femHeadLeft, femHeadRight, SacrPostMidp, AntEndplateSacrum));

            }
            if (ren1 != null)
            {
                PlumbLine(new System.Windows.Media.Media3D.Point3D(SacrPostMidp.X, postEndplateSacrum.Y, SacrPostMidp.Z), postEndplateSacrum, 0.3, 0.3, 0.3);
                PlumbLine(ProjectPerpendicularLatestMethod(femHeadLeft, femHeadRight, SacrPostMidp, AntEndplateSacrum), postEndplateSacrum, 0.3, 0.3, 0.3);
                Spherevisualisation(new System.Windows.Media.Media3D.Point3D(SacrPostMidp.X, postEndplateSacrum.Y, SacrPostMidp.Z), 0.3, 0.3, 0.3, "proj. Post Sacr", 0.5);
                //Spherevisualisation(postEndplateSacrum, 0.3, 0.3, 0.3, "postEndplateSacrum", 0.5);
                Spherevisualisation(ProjectPerpendicularLatestMethod(femHeadLeft, femHeadRight, SacrPostMidp, AntEndplateSacrum), 0.3, 0.3, 0.3, "proj. Ant Sacr.", 0.5);
            }

            // SPI_cor_angle = AngleBetweenThreePoints(T1Rotp, new System.Windows.Media.Media3D.Point3D(T1Rotp.X, T1_body.Y, T1Rotp.Z), new System.Windows.Media.Media3D.Point3D((femHeadRight.X + femHeadLeft.X) / 2, (femHeadRight.Y + femHeadLeft.Y) / 2, (femHeadRight.Z + femHeadLeft.Z) / 2));


            if (postEndplateSacrum.Y < AntEndplateSacrum.Y)
            {
                SS_sag_angle = -SS_sag_angle;
            }

            PIangle = SS_sag_angle + PT_sag_angle;

           

            double Femdist = DistBetween2points(point1, point2);
            double axisNorm = femHeadRight.X - femHeadLeft.X;

            double angleref = Math.Asin(axisNorm / Femdist) * (180 / Math.PI);
            if( femHeadRight.Z <femHeadLeft.Z )
            {
                angleref += 180;
                //angleref = -angleref;
            }

            double PostEndplateSacrumRotX;
            double PostEndplateSacrumRotY;
            Rotate(postEndplateSacrum.X, postEndplateSacrum.Z, (femHeadRight.X + femHeadLeft.X) / 2, (femHeadRight.Z + femHeadLeft.Z) / 2, angleref, out PostEndplateSacrumRotX, out PostEndplateSacrumRotY);
            PostEndplateSacrumRotX = PostEndplateSacrumRotX - ((femHeadRight.X + femHeadLeft.X) / 2);
            PostEndplateSacrumRotY = PostEndplateSacrumRotY - ((femHeadRight.Z + femHeadLeft.Z) / 2);

            double AntEndplateSacrumRotX;
            double AntEndplateSacrumRotY;
            Rotate(AntEndplateSacrum.X, AntEndplateSacrum.Z, (femHeadRight.X + femHeadLeft.X) / 2, (femHeadRight.Z + femHeadLeft.Z) / 2, angleref, out AntEndplateSacrumRotX, out AntEndplateSacrumRotY);
            AntEndplateSacrumRotX = AntEndplateSacrumRotX - ((femHeadRight.X + femHeadLeft.X) / 2);
            AntEndplateSacrumRotY = AntEndplateSacrumRotY - ((femHeadRight.Z + femHeadLeft.Z) / 2);


            double femLeftRotX;
            double femLeftRotY;
            Rotate(femHeadLeft.X, femHeadLeft.Z, (femHeadRight.X + femHeadLeft.X) / 2, (femHeadRight.Z + femHeadLeft.Z) / 2, angleref, out femLeftRotX, out femLeftRotY);
            femLeftRotX = femLeftRotX - ((femHeadRight.X + femHeadLeft.X) / 2);
            femLeftRotY = femLeftRotY - ((femHeadRight.Z + femHeadLeft.Z) / 2);

            double femRightRotX;
            double femRightRotY;
            Rotate(femHeadRight.X, femHeadRight.Z, (femHeadRight.X + femHeadLeft.X) / 2, (femHeadRight.Z + femHeadLeft.Z) / 2, angleref, out femRightRotX, out femRightRotY);
            femRightRotX = femRightRotX - ((femHeadRight.X + femHeadLeft.X) / 2);
            femRightRotY = femRightRotY - ((femHeadRight.Z + femHeadLeft.Z) / 2);

           
            //System.Windows.Media.Media3D.Point3D midpointsacr  = new System.Windows.Media.Media3D.Point3D((PostEndplateSacrumRotX + AntEndplateSacrumRotX) / 2, (postEndplateSacrum.Y + AntEndplateSacrum.Y) / 2, (PostEndplateSacrumRotY + AntEndplateSacrumRotY) / 2);
            //System.Windows.Media.Media3D.Point3D midpointFem = new System.Windows.Media.Media3D.Point3D((femLeftRotX + femRightRotX) / 2, (femHeadLeft.Y + femHeadRight.Y) / 2, (femLeftRotY + femRightRotY) / 2);
System.Windows.Media.Media3D.Point3D midpointFem = new System.Windows.Media.Media3D.Point3D((femLeftRotX + femRightRotX) / 2, (femHeadLeft.Y + femHeadRight.Y) / 2, (femLeftRotY + femRightRotY) / 2);

            System.Windows.Media.Media3D.Point3D midpointsacr = new System.Windows.Media.Media3D.Point3D((PostEndplateSacrumRotX + AntEndplateSacrumRotX) / 2, (postEndplateSacrum.Y + AntEndplateSacrum.Y) / 2, midpointFem.Z);
            SS_sag_angle = AngleBetweenThreePoints(new System.Windows.Media.Media3D.Point3D(PostEndplateSacrumRotX + 10, postEndplateSacrum.Y, midpointFem.Z), new System.Windows.Media.Media3D.Point3D(PostEndplateSacrumRotX, postEndplateSacrum.Y, midpointFem.Z), new System.Windows.Media.Media3D.Point3D(AntEndplateSacrumRotX, AntEndplateSacrum.Y, midpointFem.Z));


            PT_sag_angle = AngleBetweenThreePoints(new System.Windows.Media.Media3D.Point3D(midpointFem.X, midpointFem.Y + 10, midpointFem.Z), midpointFem, midpointsacr);

            if(midpointsacr.X > midpointFem.X)
            {
                PT_sag_angle = -PT_sag_angle;
            }

            PIangle = SS_sag_angle + PT_sag_angle;

            if (Visualize)
            {
                //3D visuals
                Spherevisualisation(AntEndplateSacrum, 1, 0, 0, "Ant. Endp. Sacr.", 1);
                Spherevisualisation(SacrMidp, 0, 0, 0, "Proj Sac Mid", 0.2);

                Spherevisualisation(new System.Windows.Media.Media3D.Point3D((femHeadLeft.X + femHeadRight.X) / 2, (femHeadLeft.Y + femHeadRight.Y) / 2, (femHeadLeft.Z + femHeadRight.Z) / 2), 1, 1, 1, "Fem. Midpoint", 0.3);
                Spherevisualisation(postEndplateSacrum, 0, 0, 1, "Post. Endp. Sacr.", 1);
                PlumbLine(AntEndplateSacrum, postEndplateSacrum, 0, 0, 0);
                PlumbLine(femHeadLeft, femHeadRight, 0, 1, 0);
                Spherevisualisation(femHeadLeft, 0, 1, 0, "Fem. Left", 2.5);
                Spherevisualisation(femHeadRight, 0, 1, 0, "Fem. Right", 2.5);
                PlumbLine(new System.Windows.Media.Media3D.Point3D((postEndplateSacrum.X + AntEndplateSacrum.X) / 2, (postEndplateSacrum.Y + AntEndplateSacrum.Y) / 2, (postEndplateSacrum.Z + AntEndplateSacrum.Z) / 2), SacrMidp, 0, 0, 1);
                PlumbLine(SacrMidp, new System.Windows.Media.Media3D.Point3D(SacrMidp.X, SacrMidp.Y + 0.6, SacrMidp.Z), 1, 0, 0);


                //2D visuals
                Spherevisualisation(new System.Windows.Media.Media3D.Point3D(femLeftRotX, femHeadLeft.Y, femLeftRotY), 0, 1, 0, "Fem. Left", 2);
                Spherevisualisation(new System.Windows.Media.Media3D.Point3D(femRightRotX, femHeadRight.Y, femRightRotY), 0, 1, 0, "Fem. Right", 2);
                PlumbLine(new System.Windows.Media.Media3D.Point3D(femLeftRotX, femHeadLeft.Y, femLeftRotY), new System.Windows.Media.Media3D.Point3D(femRightRotX, femHeadRight.Y, femRightRotY), 0, 1, 0);


                System.Windows.Media.Media3D.Point3D postEndplateproj = new System.Windows.Media.Media3D.Point3D(PostEndplateSacrumRotX, postEndplateSacrum.Y, midpointFem.Z);
                Spherevisualisation(postEndplateproj, 0, 0, 1, "Post. Endp. Sacr. PROJ", 1);
                System.Windows.Media.Media3D.Point3D antEndplateproj = new System.Windows.Media.Media3D.Point3D(AntEndplateSacrumRotX, AntEndplateSacrum.Y, midpointFem.Z);
                Spherevisualisation(antEndplateproj, 0, 0, 1, "Ant. Endp. Sacr. PROJ", 1);
                Spherevisualisation(midpointsacr, 0, 0, 1, "Sacral midpoint PROJ", 1);
                Spherevisualisation(midpointFem, 0, 0, 1, "Fem midpoint PROJ", 1);
                PlumbLine(midpointsacr, midpointFem, 1, 1, 0);


                PlumbLine(postEndplateproj, antEndplateproj, 1, 0, 0);

                PlumbLine(new System.Windows.Media.Media3D.Point3D(PostEndplateSacrumRotX + 0.1, postEndplateSacrum.Y, midpointFem.Z), postEndplateproj, 1, 1, 0);

                
                //Print values to output
                txtCurrentValue.Text = "Sacral Slope: " + SS_sag_angle.ToString() + '\t' + "Pelvic Tilt: " + PT_sag_angle.ToString() + '\t' + "PI: " + PIangle.ToString();
                txtCurrentValue.AppendText(Environment.NewLine + "cross product sign: " + crosproductSign.ToString());
                txtCurrentValue.AppendText(Environment.NewLine + "Angle ref: " + angleref.ToString()+ " Fem. Distance " + Femdist.ToString() + " Axis Normal: " + axisNorm.ToString());

                ren1.GetRenderWindow().Render();

            }




        }

        private double CalculatePI(System.Windows.Media.Media3D.Point3D AntEndplateSacrum, System.Windows.Media.Media3D.Point3D postEndplateSacrum, System.Windows.Media.Media3D.Point3D femHeadLeft, System.Windows.Media.Media3D.Point3D femHeadRight)
        {
            //calculate the Pelvic Incidence Angle
            System.Windows.Media.Media3D.Point3D point0 = new System.Windows.Media.Media3D.Point3D(AntEndplateSacrum.X, AntEndplateSacrum.Y, AntEndplateSacrum.Z);
            System.Windows.Media.Media3D.Point3D point1 = new System.Windows.Media.Media3D.Point3D((AntEndplateSacrum.X + postEndplateSacrum.X) / 2, (AntEndplateSacrum.Y + postEndplateSacrum.Y) / 2, (AntEndplateSacrum.Z + postEndplateSacrum.Z) / 2);
            System.Windows.Media.Media3D.Point3D point2 = new System.Windows.Media.Media3D.Point3D((femHeadLeft.X + femHeadRight.X) / 2, (femHeadLeft.Y + femHeadRight.Y) / 2, (femHeadLeft.Z + femHeadRight.Z) / 2);
            double angle = AngleBetweenThreePoints(point0, point1, point2);
            return (90 - angle);
        }

        private double CalculatePT(System.Windows.Media.Media3D.Point3D AntEndplateSacrum, System.Windows.Media.Media3D.Point3D postEndplateSacrum, System.Windows.Media.Media3D.Point3D femHeadLeft, System.Windows.Media.Media3D.Point3D femHeadRight)
        {
            //calculate the Pelvic Incidence Angle
            System.Windows.Media.Media3D.Point3D point0 = new System.Windows.Media.Media3D.Point3D((femHeadLeft.X + femHeadRight.X) / 2, ((femHeadLeft.Y + femHeadRight.Y) / 2) + 10, (femHeadLeft.Z + femHeadRight.Z) / 2); //Vertical above Point1
            System.Windows.Media.Media3D.Point3D point1 = new System.Windows.Media.Media3D.Point3D((femHeadLeft.X + femHeadRight.X) / 2, (femHeadLeft.Y + femHeadRight.Y) / 2, (femHeadLeft.Z + femHeadRight.Z) / 2);
            System.Windows.Media.Media3D.Point3D point2 = new System.Windows.Media.Media3D.Point3D((AntEndplateSacrum.X + postEndplateSacrum.X) / 2, (AntEndplateSacrum.Y + postEndplateSacrum.Y) / 2, (AntEndplateSacrum.Z + postEndplateSacrum.Z) / 2);

            double angle = AngleBetweenThreePoints(point0, point1, point2);
            return angle;
        }

        public static double AngleBetweenThreePoints(System.Windows.Media.Media3D.Point3D point0, System.Windows.Media.Media3D.Point3D point1, System.Windows.Media.Media3D.Point3D point2) //, System.Windows.Media.Media3D.Vector3D up)
        {

            var v1 = point1 - point0;
            var v2 = point1 - point2;

            System.Windows.Media.Media3D.Vector3D up = v1; // new System.Windows.Media.Media3D.Vector3D(0, 1, 0);

            if(up.X<0)
            {
            }

            var cross = System.Windows.Media.Media3D.Vector3D.CrossProduct(v1, v2);
            var dot = System.Windows.Media.Media3D.Vector3D.DotProduct(v1, v2);

            var angle = Math.Atan2(cross.Length, dot);

            //var test = System.Windows.Media.Media3D.Vector3D.DotProduct(up, cross);
            //if (test < 0.0) angle = -angle;

            angle = angle * (180 / Math.PI);
            return angle;
        }

        public static double Angle2DBetweenFourPoints(System.Windows.Media.Media3D.Point3D point0, System.Windows.Media.Media3D.Point3D point1, System.Windows.Media.Media3D.Point3D point2, System.Windows.Media.Media3D.Point3D point3) //, System.Windows.Media.Media3D.Vector3D up)
        {
            var angle = Math.Atan2(point1.Z - point0.Z, point1.X - point0.X) - Math.Atan2(point3.Z - point2.Z, point3.X - point2.X);

            return Math.Min(angle, Math.Abs(Math.PI - angle)); //Returns the smallest absolute angle
        }
        public static double Angle2DBetweenFourPoints2(System.Windows.Media.Media3D.Point3D R1, System.Windows.Media.Media3D.Point3D R2, System.Windows.Media.Media3D.Point3D R3, System.Windows.Media.Media3D.Point3D R4) //, System.Windows.Media.Media3D.Vector3D up)
        {
            double LL_Angle;
            var angle = Math.Atan2(R1.X - R2.X, R1.Y - R2.Y) - Math.Atan2(R3.X - R4.X, R3.Y - R4.Y);

            double Tempvalue = angle * (180 / Math.PI);

            if (angle > Math.PI) { angle -= 2 * Math.PI; }
            else if (angle <= -Math.PI) { angle += 2 * Math.PI; }
            LL_Angle = angle;





            //var angle2 = Math.Abs(Math.Atan2(R1.Y - R2.Y, R1.X - R2.X) - Math.Atan2(R3.Y - R4.Y, R3.X - R4.X));
            ////return Math.PI - angle;

            ////double Tempvalue = angle * (180 / Math.PI);

            //List<double> list1 = new List<double>() { angle, Math.Abs(Math.PI - angle), Math.Abs((2 * Math.PI) - angle) };
            //LL_Angle = list1.Min();

            //if (Math.Abs(R3.Y - R1.Y) > Math.Abs(R4.Y - R2.Y)) //If Posterior is heigher than Anterior.
            //{
            //    LL_Angle = -LL_Angle;
            //    //;//Returns the smallest absolute angle
            //}
            //else
            //{
            //    LL_Angle = LL_Angle;
            //    //LL_Angle = Math.Max(angle, Math.Abs(Math.PI + angle));
            //}

            return LL_Angle;


            //return Math.Min(angle, Math.Abs(Math.PI - angle)); //Returns the smallest absolute angle
        }
        public static double Angle2DBetweenFourPoints3(System.Windows.Media.Media3D.Point3D R1, System.Windows.Media.Media3D.Point3D R2, System.Windows.Media.Media3D.Point3D R3, System.Windows.Media.Media3D.Point3D R4) //, System.Windows.Media.Media3D.Vector3D up)
        {
         //   var angle = Math.Abs(Math.Atan2(R1.Y - R2.Y, R1.Z - R2.Z) - Math.Atan2(R3.Y - R4.Y, R3.Z - R4.Z));
            var angle =Math.Atan2(R2.X - R1.X, R2.Y - R1.Y) - Math.Atan2(R4.X - R3.X, R4.Y - R3.Y);
            //return angle;
            double LL_Angle;
            double Tempvalue = angle * (180 / Math.PI);

            if (angle > Math.PI) { angle -= 2 * Math.PI; }
            else if (angle <= -Math.PI) { angle += 2 * Math.PI; }
            LL_Angle = -angle;








            //List<double> list1 = new List<double>() { Math.Abs(angle), Math.Abs(Math.PI + angle), Math.Abs(Math.PI - angle), Math.Abs((2 * Math.PI) - angle), Math.Abs((2 * Math.PI) + angle) };
            //LL_Angle = list1.Min();

            //    LL_Angle = LL_Angle * Math.Sign(Tempvalue);

            //if (Math.Abs(R3.Y - R1.Y) > Math.Abs(R4.Y - R2.Y)) //If Left is heigher than Right.
            //{
            //    LL_Angle = LL_Angle;
            //    //;//Returns the smallest absolute angle
            //}
            //else
            //{
            //    LL_Angle = -LL_Angle;
            //    //LL_Angle = Math.Max(angle, Math.Abs(Math.PI + angle));
            //}

            return LL_Angle;
        }
        public static double DistBetween2points(System.Windows.Media.Media3D.Point3D point0, System.Windows.Media.Media3D.Point3D point1)
        {
            double deltaX = point1.X - point0.X;
            double deltaY = point1.Y - point0.Y;
            double deltaZ = point1.Z - point0.Z;

            return (double)Math.Sqrt(deltaX * deltaX + deltaY * deltaY + deltaZ * deltaZ);
        }

        private void cbSVA_CheckedChanged(object sender, EventArgs e)
        {
            if (cbSVA.Checked)
            {
                flowLayoutPnlSVA.Enabled = true;
            }
            else
            {
                flowLayoutPnlSVA.Enabled = false;
            }
        }

        private void cbPelvicTilt_CheckedChanged(object sender, EventArgs e)
        {
            if (cbPelvicAngles.Checked)
            {
                flowLayoutPnlPT.Enabled = true;
            }
            else
            {
                flowLayoutPnlPT.Enabled = false;
            }
        }

        private void cbT1SPI_CheckedChanged(object sender, EventArgs e)
        {
            if (cbT1SPI.Checked)
            {
                flowLayoutPnlT1SPI.Enabled = true;
            }
            else
            {
                flowLayoutPnlT1SPI.Enabled = false;
            }
        }

        private void removeFromSelectionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Check if rows selected
            int RowCount = dataGridViewLM.Rows.GetRowCount(DataGridViewElementStates.Selected);

            if (RowCount == 0)
                return;

            if (MessageBox.Show(RowCount.ToString() + " landmark(s) selected for deletion.", "Delete Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Information, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
            {
                for (int i = 0; i < RowCount; i++)
                {
                    //Get row of clicked cell
                    DataGridViewRow row = dataGridViewLM.Rows[dataGridViewLM.SelectedRows[i].Index];
                    //Get column AcquisitionNumber
                    DataGridViewCell cell = row.Cells["LandmarkID"];
                    //create AcquisitionObject to be deleted


                    int index = selectedLandmarks.FindIndex(a => a.LandmarkID == Convert.ToInt32(cell.Value));
                    selectedLandmarks.RemoveAt(index);




                }
                UpdateDgViewLandmarks();
            }
        }

        public string sourcepath = string.Empty;

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            int selectedDataIndex = dgViewFileNames.SelectedRows[0].Index;



            switch (VisualisationCase)
            {
                case "MeasVisualizerSVA":
                    RenderSVAmeasurement(selectedDataIndex, trackBar1.Value);
                    break;
                case "MeasVisualizerPelvicAngles":
                    RenderPelvicAngelsmeasurement(selectedDataIndex, trackBar1.Value);
                    break;
                case "MeasVisualizerT1SPI":
                    RenderT1SPImeasurement(selectedDataIndex, trackBar1.Value, cbT1SPI_FemHeadLeft.SelectedIndex, cbT1SPI_FemHeadRight.SelectedIndex, cbSPIT1_T1.SelectedIndex, "T1");
                    break;
                case "MeasVisualizerT9SPI":
                    RenderT1SPImeasurement(selectedDataIndex, trackBar1.Value, cbT9SPI_FemHeadLeft.SelectedIndex, cbT9SPI_FemHeadRight.SelectedIndex, cbSPIT9_T9.SelectedIndex, "T9");
                    break;
                case "MeasVisualizerCorCobs":
                    RenderCoronalCobbmeasurement(selectedDataIndex, trackBar1.Value, cb_Cobb_FemHeadLeft.SelectedIndex, cb_Cobb_FemHeadRight.SelectedIndex, cb_Cobb_T1_Left.SelectedIndex, cb_Cobb_T1_Right.SelectedIndex, TXT_Cobb_Superior.Text, cb_Cobb_T12_Left.SelectedIndex, cb_Cobb_T12_Right.SelectedIndex, TXT_Cobb_Inferior.Text);
                    break;
                case "MeasVisualizerLL":
                    RenderLLmeasurement(selectedDataIndex, trackBar1.Value, cb_LL_FemHeadLeft.SelectedIndex, cb_LL_FemHeadRight.SelectedIndex, cb_LL_T12SupAnt.SelectedIndex, cb_LL_T12SupPost.SelectedIndex, txtSuperior_LL.Text, CB_LL_S1SupANT.SelectedIndex, CB_LL_S1SupPOST.SelectedIndex, txtInferior_LL.Text);
                    break;
                case "MeasVisualizerTK":
                    RenderTKmeasurement(selectedDataIndex, trackBar1.Value, cb_TK_FemHeadLeft.SelectedIndex, cb_TK_FemHeadRight.SelectedIndex, cb_TK_T1SupPost.SelectedIndex, cb_TK_T1SupAnt.SelectedIndex, TK_Superior.Text, cb_TK_T12SupPost.SelectedIndex, cb_TK_T12SupAnt.SelectedIndex, TK_Inferior.Text);
                    break;
                case "RawData":
                    PrintRawDataToRenderer(trackBar1.Value, selectedDataIndex);
                    break;
                default:

                    break;

            }

            axesGround.SetTotalLength(0.2, 0.2, 0.2);
            // axesGround.AxisLabelsOff();
            axesGround.SetShaftTypeToCylinder();
            ren1.AddActor(axesGround);

        }

        private void btnClearRenderer_Click(object sender, EventArgs e)
        {
            ren1.RemoveAllViewProps();
            ren1.GetRenderWindow().Render();

        }

        private void cbGSA_CheckedChanged(object sender, EventArgs e)
        {
            if (cbGSA.Checked)
            {
                flowLayoutPnlGSA.Enabled = true;
            }
            else
            {
                flowLayoutPnlGSA.Enabled = false;
            }
        }

        private void cbTKT1_T12_CheckedChanged(object sender, EventArgs e)
        {
            if (cbTK.Checked)
            {
                flowLayoutPnlTKT.Enabled = true;
            }
            else
            {
                flowLayoutPnlTKT.Enabled = false;
            }
        }

        private void cbLLT12_S1_CheckedChanged(object sender, EventArgs e)
        {

            if (cbLL.Checked)
            {
                flowLayoutPnlLL.Enabled = true;
            }
            else
            {
                flowLayoutPnlLL.Enabled = false;
            }
        }

        private void btnSetOutputPath_Click(object sender, EventArgs e)
        {

            if (vistaFolderBrowserDialog1.ShowDialog(this) == DialogResult.OK)
            {
                sourcepath = vistaFolderBrowserDialog1.SelectedPath;
                btnSetOutputPath.BackColor = Color.LightGreen;
            }
            else { return; }


        }

        private void cbT9SPI_CheckedChanged(object sender, EventArgs e)
        {
            if (cbT9SPI.Checked)
            {
                flowLayoutPnlT9SPI.Enabled = true;
            }
            else
            {
                flowLayoutPnlT9SPI.Enabled = false;
            }
        }

        private void TK_Superior_TextChanged(object sender, EventArgs e)
        {
            lblTK_SuperiorAnt.Text = TK_Superior.Text + " Mid-Anterior";
            lblTK_SuperiorPost.Text = TK_Superior.Text + " Mid-Posterior";
        }

        private void TK_Inferior_TextChanged(object sender, EventArgs e)
        {
            lblTK_InferiorAnt.Text = TK_Inferior.Text + " Mid-Anterior";
            lblTK_InferiorPost.Text = TK_Inferior.Text + " Mid-Posterior";
        }

        private void txtSuperior_LL_TextChanged(object sender, EventArgs e)
        {
            lblLL_T12SupANT.Text = txtSuperior_LL.Text + " Mid-Anterior";
            lblLL_T12SupPost.Text = txtSuperior_LL.Text + " Mid-Posterior";
        }

        private void txtInferior_LL_TextChanged(object sender, EventArgs e)
        {
            lbl_LL_S1SupANT.Text = txtInferior_LL.Text + " Mid-Anterior";
            lbl_LL_S1SupPOST.Text = txtInferior_LL.Text + " Mid-Posterior";
        }

        private void cb_Cor_Cobb_Angles_CheckedChanged(object sender, EventArgs e)
        {
            if (cb_Cor_Cobb_Angles.Checked)
            {
                flowlayout_Cobb.Enabled = true;
            }
            else
            {
                flowlayout_Cobb.Enabled = false;
            }
        }

        private void TXT_Cobb_Superior_TextChanged(object sender, EventArgs e)
        {
            lblCobb_T1_Left.Text = TXT_Cobb_Superior.Text + " Left";
            lblCobb_T1_Right.Text = TXT_Cobb_Superior.Text + " Right";
        }

        private void TXT_Cobb_Inferior_TextChanged(object sender, EventArgs e)
        {
            lblCobb_T12_Left.Text = TXT_Cobb_Inferior.Text + " Left";
            lblCobb_T12_Right.Text = TXT_Cobb_Inferior.Text + " Right";

        }

        private void label38_Click(object sender, EventArgs e)
        {

        }

        private void label33_Click(object sender, EventArgs e)
        {

        }

        private void btnImportFromDatabase_Click(object sender, EventArgs e)
        {
            MessageBox.Show("This module is not active.", "Inactive Module", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void btnVisualizeSVA_Click(object sender, EventArgs e)
        {
            VisualisationCase = "MeasVisualizerSVA";
            int selectedDataIndex = dgViewFileNames.SelectedRows[0].Index;
            trackBar1.Visible = true;
            trackBar1.Maximum = DataList[selectedDataIndex].AbsoluteDt.Rows.Count - 1;
            trackBar1.Minimum = 0;


            int frameIndex = 0;

            RenderSVAmeasurement(selectedDataIndex, frameIndex);
            vtkCamera camera = ren1.GetActiveCamera();
            camera.ParallelProjectionOn();
            ren1.ResetCamera();
            RenderWindow.Render();

        }

        private void RenderPelvicAngelsmeasurement(int DataIndex, int frameIndex)
        {
            ren1.RemoveAllViewProps();

            int Index_FemHeadLeft = cbPT_FemHeadLeft.SelectedIndex;
            int Index_FemHeadRight = cbPT_FemHeadRight.SelectedIndex;
            int Index_SacrumPosterior = cbPT_SacrumPosterior.SelectedIndex;
            int Index_SacrumAnterior = cbPT_SacrumAnterior.SelectedIndex;


            if (Index_FemHeadLeft == -1 || Index_FemHeadRight == -1 || Index_SacrumPosterior == -1 || Index_SacrumAnterior == -1)
            {
                MessageBox.Show("One or more of the landmarks needed for the Pelvic angle calculation are not assigned. This measurement will be skipped.", "Not enough input", MessageBoxButtons.OK, MessageBoxIcon.Information);

            }
            else
            {

                System.Windows.Media.Media3D.Point3D AntEndplateSacrum = new System.Windows.Media.Media3D.Point3D(Convert.ToDouble(DataList[DataIndex].TransformedLandmarkList[Index_SacrumAnterior].Rows[frameIndex]["posX"]), Convert.ToDouble(DataList[DataIndex].TransformedLandmarkList[Index_SacrumAnterior].Rows[frameIndex]["posY"]), Convert.ToDouble(DataList[DataIndex].TransformedLandmarkList[Index_SacrumAnterior].Rows[frameIndex]["posZ"]));
                System.Windows.Media.Media3D.Point3D postEndplateSacrum = new System.Windows.Media.Media3D.Point3D(Convert.ToDouble(DataList[DataIndex].TransformedLandmarkList[Index_SacrumPosterior].Rows[frameIndex]["posX"]), Convert.ToDouble(DataList[DataIndex].TransformedLandmarkList[Index_SacrumPosterior].Rows[frameIndex]["posY"]), Convert.ToDouble(DataList[DataIndex].TransformedLandmarkList[Index_SacrumPosterior].Rows[frameIndex]["posZ"]));
                System.Windows.Media.Media3D.Point3D femHeadLeft = new System.Windows.Media.Media3D.Point3D(Convert.ToDouble(DataList[DataIndex].TransformedLandmarkList[Index_FemHeadLeft].Rows[frameIndex]["posX"]), Convert.ToDouble(DataList[DataIndex].TransformedLandmarkList[Index_FemHeadLeft].Rows[frameIndex]["posY"]), Convert.ToDouble(DataList[DataIndex].TransformedLandmarkList[Index_FemHeadLeft].Rows[frameIndex]["posZ"]));
                System.Windows.Media.Media3D.Point3D femHeadRight = new System.Windows.Media.Media3D.Point3D(Convert.ToDouble(DataList[DataIndex].TransformedLandmarkList[Index_FemHeadRight].Rows[frameIndex]["posX"]), Convert.ToDouble(DataList[DataIndex].TransformedLandmarkList[Index_FemHeadRight].Rows[frameIndex]["posY"]), Convert.ToDouble(DataList[DataIndex].TransformedLandmarkList[Index_FemHeadRight].Rows[frameIndex]["posZ"]));

                if (cb_pelvicAngles_Sagittalprojection.Checked)
                {
                    double SSangle;
                    double PTangle;
                    double PIangle;
                    System.Windows.Media.Media3D.Point3D SacrMidp;
                    System.Windows.Media.Media3D.Point3D SacrPostMidp;
                    CalculatePelvicAnglesSagProj(AntEndplateSacrum, postEndplateSacrum, femHeadLeft, femHeadRight, true, out PIangle, out PTangle, out SSangle, out SacrMidp, out SacrPostMidp);

                    

                }
                else
                {
                    double PIvalue = CalculatePI(AntEndplateSacrum, postEndplateSacrum, femHeadLeft, femHeadRight);
                    double PTvalue = CalculatePT(AntEndplateSacrum, postEndplateSacrum, femHeadLeft, femHeadRight);
                    double SSvalue = PIvalue - PTvalue;
                }
            }
        }

        private void RenderSVAmeasurement(int DataIndex, int frameIndex)
        {
            ren1.RemoveAllViewProps();

            int Index_FemHeadLeft = cbSVA_FemHeadLeft.SelectedIndex;
            int Index_FemHeadRight = cbSVA_FemHeadRight.SelectedIndex;
            int Index_SacrumPosterior = cbSVA_SupPostSacralEndplate.SelectedIndex;
            int Index_C7 = cbSVA_C7body.SelectedIndex;

            if (Index_FemHeadLeft == -1 || Index_FemHeadRight == -1 || Index_SacrumPosterior == -1 || Index_C7 == -1)
            {
                MessageBox.Show("One or more of the landmarks needed for the SVA calculation are not assigned. This measurement will be skipped.", "Not enough input", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                System.Windows.Media.Media3D.Point3D postEndplateSacrum = new System.Windows.Media.Media3D.Point3D(Convert.ToDouble(DataList[DataIndex].TransformedLandmarkList[Index_SacrumPosterior].Rows[frameIndex]["posX"]), Convert.ToDouble(DataList[DataIndex].TransformedLandmarkList[Index_SacrumPosterior].Rows[frameIndex]["posY"]), Convert.ToDouble(DataList[DataIndex].TransformedLandmarkList[Index_SacrumPosterior].Rows[frameIndex]["posZ"]));
                System.Windows.Media.Media3D.Point3D femHeadLeft = new System.Windows.Media.Media3D.Point3D(Convert.ToDouble(DataList[DataIndex].TransformedLandmarkList[Index_FemHeadLeft].Rows[frameIndex]["posX"]), Convert.ToDouble(DataList[DataIndex].TransformedLandmarkList[Index_FemHeadLeft].Rows[frameIndex]["posY"]), Convert.ToDouble(DataList[DataIndex].TransformedLandmarkList[Index_FemHeadLeft].Rows[frameIndex]["posZ"]));
                System.Windows.Media.Media3D.Point3D femHeadRight = new System.Windows.Media.Media3D.Point3D(Convert.ToDouble(DataList[DataIndex].TransformedLandmarkList[Index_FemHeadRight].Rows[frameIndex]["posX"]), Convert.ToDouble(DataList[DataIndex].TransformedLandmarkList[Index_FemHeadRight].Rows[frameIndex]["posY"]), Convert.ToDouble(DataList[DataIndex].TransformedLandmarkList[Index_FemHeadRight].Rows[frameIndex]["posZ"]));
                System.Windows.Media.Media3D.Point3D C7_body = new System.Windows.Media.Media3D.Point3D(Convert.ToDouble(DataList[DataIndex].TransformedLandmarkList[Index_C7].Rows[frameIndex]["posX"]), Convert.ToDouble(DataList[DataIndex].TransformedLandmarkList[Index_C7].Rows[frameIndex]["posY"]), Convert.ToDouble(DataList[DataIndex].TransformedLandmarkList[Index_C7].Rows[frameIndex]["posZ"]));

                double SVAvalue;
                double CorValue;
                double value3D;


                System.Windows.Media.Media3D.Point3D C7Rotp;
                System.Windows.Media.Media3D.Point3D SacrRotP;
             

                CalculateSVA(postEndplateSacrum, femHeadLeft, femHeadRight, C7_body, true, out SVAvalue, out CorValue, out value3D, out C7Rotp, out SacrRotP);

               

            }
        }

        private void Spherevisualisation(System.Windows.Media.Media3D.Point3D C7_body, double R, double G, double B, string Tagname, double scale)
        {
            vtkActor Sphereactor = new vtkActor();
            vtkSphereSource sphere = new vtkSphereSource();
            sphere.SetRadius(0.006* scale);
            sphere.SetPhiResolution(36);
            sphere.SetThetaResolution(36);
            vtkPolyDataMapper mapper2 = vtkPolyDataMapper.New();
            mapper2.SetInputConnection(sphere.GetOutputPort());

            Sphereactor.GetProperty().SetColor(R, G, B);
            Sphereactor.SetMapper(mapper2);

            vtkTransform transform = new vtkTransform();
            transform.PostMultiply();
            transform.Translate(C7_body.X, C7_body.Y, C7_body.Z);

            Sphereactor.SetUserTransform(transform);
            ren1.AddActor(Sphereactor);

            // Create some text
            vtkVectorText textSource = new vtkVectorText();
            textSource.SetText(Tagname);

            // Create a mapper
            vtkPolyDataMapper mapper = vtkPolyDataMapper.New();
            mapper.SetInputConnection(textSource.GetOutputPort());

            // Create a subclass of vtkActor: a vtkFollower that remains facing the camera
            vtkFollower follower = new vtkFollower();
            follower.SetMapper(mapper);
            follower.GetProperty().SetColor(R, G, B);
            follower.SetScale(0.003);
            follower.SetPosition(C7_body.X + 0.005* scale, C7_body.Y + 0.005* scale, C7_body.Z + 0.005* scale);
            follower.SetCamera(ren1.GetActiveCamera());
            ren1.AddActor(follower);
        }

        private void PlumbLine(System.Windows.Media.Media3D.Point3D C7_body, System.Windows.Media.Media3D.Point3D infEndpoint, double R, double G, double B)
        {
            if (DistBetween2points(C7_body, infEndpoint) == 0)
            { return; }

            // Create a line
            vtkLineSource lineSource = new vtkLineSource();
            lineSource.SetPoint1(C7_body.X, C7_body.Y, C7_body.Z);
            lineSource.SetPoint2(infEndpoint.X, infEndpoint.Y, infEndpoint.Z);

            // Create a mapper and actor
            vtkPolyDataMapper lineMapper = vtkPolyDataMapper.New();
            lineMapper.SetInput(lineSource.GetOutput());
            vtkActor lineActor = new vtkActor();
            lineActor.GetProperty().SetColor(0, 1, 0);
            lineActor.SetMapper(lineMapper);

            // Create a tube (cylinder) around the line
            vtkTubeFilter tubeFilter = new vtkTubeFilter();

            tubeFilter.SetInputConnection(lineSource.GetOutputPort());
            tubeFilter.SetRadius(0.0008);
            tubeFilter.SetNumberOfSides(20);
            tubeFilter.Update();



            // Create a mapper and actor
            vtkPolyDataMapper tubeMapper = vtkPolyDataMapper.New();
            tubeMapper.SetInputConnection(tubeFilter.GetOutputPort());
            vtkActor tubeActor = new vtkActor();
            tubeActor.SetMapper(tubeMapper);

            tubeActor.GetProperty().SetColor(R, G, B);


            ren1.AddActor(tubeActor);
                
            //ren1.Render();

        }

        private void parallelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            vtkCamera camera = ren1.GetActiveCamera();
            camera.ParallelProjectionOn();
            ren1.ResetCamera();
            RenderWindow.Render();
        }

        private void orthogonalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            vtkCamera camera = ren1.GetActiveCamera();
            camera.ParallelProjectionOff();
            ren1.ResetCamera();
            RenderWindow.Render();
        }
        vtkTextActor3D vtkTextActor = new vtkTextActor3D();

        private void btnVisualizePelvicAngles_Click(object sender, EventArgs e)
        {
            VisualisationCase = "MeasVisualizerPelvicAngles";
            int selectedDataIndex = dgViewFileNames.SelectedRows[0].Index;
            trackBar1.Visible = true;
            trackBar1.Maximum = DataList[selectedDataIndex].AbsoluteDt.Rows.Count - 1;
            trackBar1.Minimum = 0;

            int frameIndex = 0;
            RenderPelvicAngelsmeasurement(selectedDataIndex, frameIndex);

            vtkCamera camera = ren1.GetActiveCamera();
            camera.ParallelProjectionOn();
            ren1.ResetCamera();
            RenderWindow.Render();
        }


        private void RenderCoronalCobbmeasurement(int selectedDataIndex, int frameIndex, int Index_FemHeadLeft, int Index_FemHeadRight, int Index_T1_Left, int Index_T1_Right, string nameT1, int Index_T12_Left, int Index_T12_Right, string nameT12)
        {
            ren1.RemoveAllViewProps();
            ren1.Clear();

            if (Index_FemHeadLeft == -1 || Index_FemHeadRight == -1 || Index_T1_Left == -1 || Index_T1_Right == -1 || Index_T12_Left == -1 || Index_T12_Right == -1)
            {
                MessageBox.Show("One or more of the landmarks needed for the Coronal Cobb calculation are not assigned. This measurement will be skipped.", "Not enough input", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                System.Windows.Media.Media3D.Point3D T12Left = new System.Windows.Media.Media3D.Point3D(Convert.ToDouble(DataList[selectedDataIndex].TransformedLandmarkList[Index_T12_Left].Rows[frameIndex]["posX"]), Convert.ToDouble(DataList[selectedDataIndex].TransformedLandmarkList[Index_T12_Left].Rows[frameIndex]["posY"]), Convert.ToDouble(DataList[selectedDataIndex].TransformedLandmarkList[Index_T12_Left].Rows[frameIndex]["posZ"]));
                System.Windows.Media.Media3D.Point3D femHeadLeft = new System.Windows.Media.Media3D.Point3D(Convert.ToDouble(DataList[selectedDataIndex].TransformedLandmarkList[Index_FemHeadLeft].Rows[frameIndex]["posX"]), Convert.ToDouble(DataList[selectedDataIndex].TransformedLandmarkList[Index_FemHeadLeft].Rows[frameIndex]["posY"]), Convert.ToDouble(DataList[selectedDataIndex].TransformedLandmarkList[Index_FemHeadLeft].Rows[frameIndex]["posZ"]));
                System.Windows.Media.Media3D.Point3D femHeadRight = new System.Windows.Media.Media3D.Point3D(Convert.ToDouble(DataList[selectedDataIndex].TransformedLandmarkList[Index_FemHeadRight].Rows[frameIndex]["posX"]), Convert.ToDouble(DataList[selectedDataIndex].TransformedLandmarkList[Index_FemHeadRight].Rows[frameIndex]["posY"]), Convert.ToDouble(DataList[selectedDataIndex].TransformedLandmarkList[Index_FemHeadRight].Rows[frameIndex]["posZ"]));
                System.Windows.Media.Media3D.Point3D T12Right = new System.Windows.Media.Media3D.Point3D(Convert.ToDouble(DataList[selectedDataIndex].TransformedLandmarkList[Index_T12_Right].Rows[frameIndex]["posX"]), Convert.ToDouble(DataList[selectedDataIndex].TransformedLandmarkList[Index_T12_Right].Rows[frameIndex]["posY"]), Convert.ToDouble(DataList[selectedDataIndex].TransformedLandmarkList[Index_T12_Right].Rows[frameIndex]["posZ"]));
                System.Windows.Media.Media3D.Point3D T1Left = new System.Windows.Media.Media3D.Point3D(Convert.ToDouble(DataList[selectedDataIndex].TransformedLandmarkList[Index_T1_Left].Rows[frameIndex]["posX"]), Convert.ToDouble(DataList[selectedDataIndex].TransformedLandmarkList[Index_T1_Left].Rows[frameIndex]["posY"]), Convert.ToDouble(DataList[selectedDataIndex].TransformedLandmarkList[Index_T1_Left].Rows[frameIndex]["posZ"]));
                System.Windows.Media.Media3D.Point3D T1Right = new System.Windows.Media.Media3D.Point3D(Convert.ToDouble(DataList[selectedDataIndex].TransformedLandmarkList[Index_T1_Right].Rows[frameIndex]["posX"]), Convert.ToDouble(DataList[selectedDataIndex].TransformedLandmarkList[Index_T1_Right].Rows[frameIndex]["posY"]), Convert.ToDouble(DataList[selectedDataIndex].TransformedLandmarkList[Index_T1_Right].Rows[frameIndex]["posZ"]));

                double Cobb_Angle;

                CalculateCobb(T12Left, T12Right, T1Left, T1Right, femHeadLeft, femHeadRight, out Cobb_Angle);

                System.Windows.Media.Media3D.Point3D T12LeftP = ProjectLatestMethod(femHeadLeft, femHeadRight, T12Left);
                System.Windows.Media.Media3D.Point3D T12RightP = ProjectLatestMethod(femHeadLeft, femHeadRight, T12Right);
                System.Windows.Media.Media3D.Point3D T1LeftP = ProjectLatestMethod(femHeadLeft, femHeadRight, T1Left);
                System.Windows.Media.Media3D.Point3D T1RightP = ProjectLatestMethod(femHeadLeft, femHeadRight, T1Right);

                //Spherevisualisation(T12LeftP, 0, 1, 0, nameT12+ " Left", 0.4);
                //Spherevisualisation(T12RightP, 0, 0, 1, nameT12+ " Right", 0.4);
                //Spherevisualisation(T1LeftP, 0, 1, 0, nameT1 + " Left", 0.4);
                //Spherevisualisation(T1RightP, 0, 0, 1, nameT1 + " Right", 0.4);

                Spherevisualisation(T12Left, 0.3, 1, 0, "", 0.4);
                Spherevisualisation(T12Right, 0.3, 0, 1, "", 0.4);
                Spherevisualisation(T1Left, 0.3, 1, 0, "", 0.4);
                Spherevisualisation(T1Right, 0.3, 0, 1, "", 0.4);

                PlumbLine(femHeadLeft, femHeadRight, 0, 1, 0);
                Spherevisualisation(femHeadLeft, 0, 1, 0, "Fem. Left", 2.5);
                Spherevisualisation(femHeadRight, 0, 1, 0, "Fem. Right", 2.5);

                double LL_AngleRad = Angle2DBetweenFourPoints3(T12LeftP, T12RightP, T1LeftP, T1RightP);
                Cobb_Angle = LL_AngleRad * (180 / Math.PI);

                PlumbLine(T12LeftP, T12RightP,1,1,1);
                PlumbLine(T1LeftP, T1RightP, 1, 1, 1);


                vtkTextActor = new vtkTextActor3D();
                vtkTextActor.SetInput(Math.Round(Cobb_Angle,2).ToString() + " °");
                vtkTextActor.SetPosition(T12LeftP.X, T12LeftP.Y,T12LeftP.Z);
                vtkTextActor.GetTextProperty().SetFontSize(50);
                vtkTextActor.SetScale(0.0005);
               // vtkTextActor.GetTextProperty().SetFontSize(1);
                ren1.AddActor(vtkTextActor);

                //Spherevisualisation(T1_body, 1, 0, 0, nameT1, 1);
                //PlumbLine(T1_body, new System.Windows.Media.Media3D.Point3D(T1_body.X, T1_body.Y - 0.65, T1_body.Z), 1, 0, 0);
                //PlumbLine(new System.Windows.Media.Media3D.Point3D((femHeadLeft.X + femHeadRight.X) / 2, (femHeadLeft.Y + femHeadRight.Y) / 2, (femHeadLeft.Z + femHeadRight.Z) / 2), T1_body, 0, 0, 0);
                //PlumbLine(new System.Windows.Media.Media3D.Point3D((femHeadLeft.X + femHeadRight.X) / 2, (femHeadLeft.Y + femHeadRight.Y) / 2, (femHeadLeft.Z + femHeadRight.Z) / 2), T1Rotp, 0, 0, 0);
                //PlumbLine(new System.Windows.Media.Media3D.Point3D(T1_body.X, T1Rotp.Y, T1_body.Z), T1Rotp, 0, 0, 1);
                //PlumbLine(T1_body, T1Rotp, 0, 0, 1);

                txtCurrentValue.Text = nameT1 + "-" + nameT12 + " Coronal Cobb Angle: "+ Cobb_Angle.ToString();

                ren1.GetRenderWindow().Render();
            }
        }

        private void RenderLLmeasurement(int selectedDataIndex, int frameIndex, int Index_FemHeadLeft, int Index_FemHeadRight, int Index_T12Ant, int Index_T12Post, string nameT12, int Index_S1Ant, int Index_S1Post, string nameS1)
        {
            ren1.RemoveAllViewProps();
            ren1.Clear();

            if (Index_FemHeadLeft == -1 || Index_FemHeadRight == -1 || Index_T12Ant == -1 || Index_T12Post == -1 || Index_S1Ant == -1 || Index_S1Post == -1)
            {
                MessageBox.Show("One or more of the landmarks needed for the LL calculation are not assigned. This measurement will be skipped.", "Not enough input", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                System.Windows.Media.Media3D.Point3D T12Ant = new System.Windows.Media.Media3D.Point3D(Convert.ToDouble(DataList[selectedDataIndex].TransformedLandmarkList[Index_T12Ant].Rows[frameIndex]["posX"]), Convert.ToDouble(DataList[selectedDataIndex].TransformedLandmarkList[Index_T12Ant].Rows[frameIndex]["posY"]), Convert.ToDouble(DataList[selectedDataIndex].TransformedLandmarkList[Index_T12Ant].Rows[frameIndex]["posZ"]));
                System.Windows.Media.Media3D.Point3D femHeadLeft = new System.Windows.Media.Media3D.Point3D(Convert.ToDouble(DataList[selectedDataIndex].TransformedLandmarkList[Index_FemHeadLeft].Rows[frameIndex]["posX"]), Convert.ToDouble(DataList[selectedDataIndex].TransformedLandmarkList[Index_FemHeadLeft].Rows[frameIndex]["posY"]), Convert.ToDouble(DataList[selectedDataIndex].TransformedLandmarkList[Index_FemHeadLeft].Rows[frameIndex]["posZ"]));
                System.Windows.Media.Media3D.Point3D femHeadRight = new System.Windows.Media.Media3D.Point3D(Convert.ToDouble(DataList[selectedDataIndex].TransformedLandmarkList[Index_FemHeadRight].Rows[frameIndex]["posX"]), Convert.ToDouble(DataList[selectedDataIndex].TransformedLandmarkList[Index_FemHeadRight].Rows[frameIndex]["posY"]), Convert.ToDouble(DataList[selectedDataIndex].TransformedLandmarkList[Index_FemHeadRight].Rows[frameIndex]["posZ"]));
                System.Windows.Media.Media3D.Point3D T12Post = new System.Windows.Media.Media3D.Point3D(Convert.ToDouble(DataList[selectedDataIndex].TransformedLandmarkList[Index_T12Post].Rows[frameIndex]["posX"]), Convert.ToDouble(DataList[selectedDataIndex].TransformedLandmarkList[Index_T12Post].Rows[frameIndex]["posY"]), Convert.ToDouble(DataList[selectedDataIndex].TransformedLandmarkList[Index_T12Post].Rows[frameIndex]["posZ"]));
                System.Windows.Media.Media3D.Point3D S1Ant = new System.Windows.Media.Media3D.Point3D(Convert.ToDouble(DataList[selectedDataIndex].TransformedLandmarkList[Index_S1Ant].Rows[frameIndex]["posX"]), Convert.ToDouble(DataList[selectedDataIndex].TransformedLandmarkList[Index_S1Ant].Rows[frameIndex]["posY"]), Convert.ToDouble(DataList[selectedDataIndex].TransformedLandmarkList[Index_S1Ant].Rows[frameIndex]["posZ"]));
                System.Windows.Media.Media3D.Point3D S1Post = new System.Windows.Media.Media3D.Point3D(Convert.ToDouble(DataList[selectedDataIndex].TransformedLandmarkList[Index_S1Post].Rows[frameIndex]["posX"]), Convert.ToDouble(DataList[selectedDataIndex].TransformedLandmarkList[Index_S1Post].Rows[frameIndex]["posY"]), Convert.ToDouble(DataList[selectedDataIndex].TransformedLandmarkList[Index_S1Post].Rows[frameIndex]["posZ"]));

                double LL_Angle;
                 
                CalculateLL(T12Ant, T12Post, S1Ant, S1Post, femHeadLeft, femHeadRight, out LL_Angle);
                System.Windows.Media.Media3D.Point3D FemMidPoint = new System.Windows.Media.Media3D.Point3D((femHeadLeft.X + femHeadRight.X) / 2, (femHeadLeft.Y + femHeadRight.Y) / 2, (femHeadLeft.Z + femHeadRight.Z) / 2);

                PlumbLine(femHeadLeft, femHeadRight, 0, 1, 0);
                Spherevisualisation(femHeadLeft, 0, 1, 0, "Fem. Left", 2.5);
                Spherevisualisation(femHeadRight, 0, 1, 0, "Fem. Right", 2.5);
                PlumbLine(FemMidPoint, new System.Windows.Media.Media3D.Point3D((femHeadLeft.X + femHeadRight.X) / 2, (femHeadLeft.Y + femHeadRight.Y) / 2 +0.7, (femHeadLeft.Z + femHeadRight.Z) / 2), 1, 0, 0);


                System.Windows.Media.Media3D.Point3D T12Antp = ProjectPerpendicularLatestMethod(femHeadLeft, femHeadRight, FemMidPoint, T12Ant);
                System.Windows.Media.Media3D.Point3D T12Postp = ProjectPerpendicularLatestMethod(femHeadLeft, femHeadRight, FemMidPoint, T12Post);
                System.Windows.Media.Media3D.Point3D S1Antp = ProjectPerpendicularLatestMethod(femHeadLeft, femHeadRight, FemMidPoint, S1Ant);
                System.Windows.Media.Media3D.Point3D S1Postp = ProjectPerpendicularLatestMethod(femHeadLeft, femHeadRight, FemMidPoint, S1Post);

                Spherevisualisation(T12Ant, 1, 0, 0, "", 0.4);
                Spherevisualisation(T12Post, 1, 0, 0,  "", 0.4);
                Spherevisualisation(S1Ant, 1, 0, 0,  "", 0.4);
                Spherevisualisation(S1Post, 1, 0, 0, "", 0.4);


                Spherevisualisation(T12Antp, 1, 0, 0, nameT12 + " Ant", 0.4);
                Spherevisualisation(T12Postp, 1, 0, 0, nameT12 + " Post", 0.4);
                Spherevisualisation(S1Antp, 1, 0, 0, nameS1 + " Ant", 0.4);
                Spherevisualisation(S1Postp, 1, 0, 0, nameS1 + " Post", 0.4);

                PlumbLine(T12Antp, T12Postp, 1, 1, 1);
                PlumbLine(S1Antp, S1Postp, 1, 1, 1);
                PlumbLine(T12Antp, T12Ant, 0.8, 0.2, 0.7);
                PlumbLine(T12Postp, T12Post, 0.8, 0.2, 0.7);
                PlumbLine(S1Antp, S1Ant, 0.8, 0.2, 0.7);
                PlumbLine(S1Postp, S1Post, 0.8, 0.2, 0.7);

                txtCurrentValue.Text = nameT12 + "-" + nameS1 +": "+ LL_Angle.ToString();

                ren1.GetRenderWindow().Render();
            }
        }


        private void RenderTKmeasurement(int selectedDataIndex, int frameIndex, int Index_FemHeadLeft, int Index_FemHeadRight, int Index_T1post, int Index_T1Ant, string nameT1, int Index_T12Post, int Index_T12Ant, string nameT12)
        {
             
            ren1.RemoveAllViewProps();
            ren1.Clear();


            if (Index_FemHeadLeft == -1 || Index_FemHeadRight == -1 || Index_T1post == -1 || Index_T1Ant == -1 || Index_T12Post == -1 || Index_T12Ant == -1)
            {
                MessageBox.Show("One or more of the landmarks needed for the LL calculation are not assigned. This measurement will be skipped.", "Not enough input", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                System.Windows.Media.Media3D.Point3D T1post = new System.Windows.Media.Media3D.Point3D(Convert.ToDouble(DataList[selectedDataIndex].TransformedLandmarkList[Index_T1post].Rows[frameIndex]["posX"]), Convert.ToDouble(DataList[selectedDataIndex].TransformedLandmarkList[Index_T1post].Rows[frameIndex]["posY"]), Convert.ToDouble(DataList[selectedDataIndex].TransformedLandmarkList[Index_T1post].Rows[frameIndex]["posZ"]));
                System.Windows.Media.Media3D.Point3D femHeadLeft = new System.Windows.Media.Media3D.Point3D(Convert.ToDouble(DataList[selectedDataIndex].TransformedLandmarkList[Index_FemHeadLeft].Rows[frameIndex]["posX"]), Convert.ToDouble(DataList[selectedDataIndex].TransformedLandmarkList[Index_FemHeadLeft].Rows[frameIndex]["posY"]), Convert.ToDouble(DataList[selectedDataIndex].TransformedLandmarkList[Index_FemHeadLeft].Rows[frameIndex]["posZ"]));
                System.Windows.Media.Media3D.Point3D femHeadRight = new System.Windows.Media.Media3D.Point3D(Convert.ToDouble(DataList[selectedDataIndex].TransformedLandmarkList[Index_FemHeadRight].Rows[frameIndex]["posX"]), Convert.ToDouble(DataList[selectedDataIndex].TransformedLandmarkList[Index_FemHeadRight].Rows[frameIndex]["posY"]), Convert.ToDouble(DataList[selectedDataIndex].TransformedLandmarkList[Index_FemHeadRight].Rows[frameIndex]["posZ"]));
                System.Windows.Media.Media3D.Point3D T1Ant = new System.Windows.Media.Media3D.Point3D(Convert.ToDouble(DataList[selectedDataIndex].TransformedLandmarkList[Index_T1Ant].Rows[frameIndex]["posX"]), Convert.ToDouble(DataList[selectedDataIndex].TransformedLandmarkList[Index_T1Ant].Rows[frameIndex]["posY"]), Convert.ToDouble(DataList[selectedDataIndex].TransformedLandmarkList[Index_T1Ant].Rows[frameIndex]["posZ"]));
                System.Windows.Media.Media3D.Point3D T12Post = new System.Windows.Media.Media3D.Point3D(Convert.ToDouble(DataList[selectedDataIndex].TransformedLandmarkList[Index_T12Post].Rows[frameIndex]["posX"]), Convert.ToDouble(DataList[selectedDataIndex].TransformedLandmarkList[Index_T12Post].Rows[frameIndex]["posY"]), Convert.ToDouble(DataList[selectedDataIndex].TransformedLandmarkList[Index_T12Post].Rows[frameIndex]["posZ"]));
                System.Windows.Media.Media3D.Point3D T12Ant = new System.Windows.Media.Media3D.Point3D(Convert.ToDouble(DataList[selectedDataIndex].TransformedLandmarkList[Index_T12Ant].Rows[frameIndex]["posX"]), Convert.ToDouble(DataList[selectedDataIndex].TransformedLandmarkList[Index_T12Ant].Rows[frameIndex]["posY"]), Convert.ToDouble(DataList[selectedDataIndex].TransformedLandmarkList[Index_T12Ant].Rows[frameIndex]["posZ"]));

                double TK_Angle;

                CalculateLL( T12Post, T12Ant, T1post, T1Ant, femHeadLeft, femHeadRight, out TK_Angle);
                System.Windows.Media.Media3D.Point3D FemMidPoint = new System.Windows.Media.Media3D.Point3D((femHeadLeft.X + femHeadRight.X) / 2, (femHeadLeft.Y + femHeadRight.Y) / 2, (femHeadLeft.Z + femHeadRight.Z) / 2);

                PlumbLine(femHeadLeft, femHeadRight, 0, 1, 0);
                Spherevisualisation(femHeadLeft, 0, 1, 0, "Fem. Left", 2.5);
                Spherevisualisation(femHeadRight, 0, 1, 0, "Fem. Right", 2.5);
                PlumbLine(FemMidPoint, new System.Windows.Media.Media3D.Point3D((femHeadLeft.X + femHeadRight.X) / 2, (femHeadLeft.Y + femHeadRight.Y) / 2 + 0.7, (femHeadLeft.Z + femHeadRight.Z) / 2), 1, 0, 0);


                System.Windows.Media.Media3D.Point3D T1Postp = ProjectPerpendicularLatestMethod(femHeadLeft, femHeadRight, FemMidPoint, T1post);
                System.Windows.Media.Media3D.Point3D T1Antp = ProjectPerpendicularLatestMethod(femHeadLeft, femHeadRight, FemMidPoint, T1Ant);
                System.Windows.Media.Media3D.Point3D T12Postp = ProjectPerpendicularLatestMethod(femHeadLeft, femHeadRight, FemMidPoint, T12Post);
                System.Windows.Media.Media3D.Point3D T12Antp = ProjectPerpendicularLatestMethod(femHeadLeft, femHeadRight, FemMidPoint, T12Ant);

                Spherevisualisation(T1post, 1, 0, 0, "", 0.4);
                Spherevisualisation(T1Ant, 1, 0, 0, "", 0.4);
                Spherevisualisation(T12Post, 1, 0, 0, "", 0.4);
                Spherevisualisation(T12Ant, 1, 0, 0, "", 0.4);


                Spherevisualisation(T1Postp, 1, 0, 0, nameT1 + " Post", 0.4);
                Spherevisualisation(T1Antp, 1, 0, 0, nameT1 + " Ant", 0.4);
                Spherevisualisation(T12Postp, 1, 0, 0, nameT12 + " Post", 0.4);
                Spherevisualisation(T12Antp, 1, 0, 0, nameT12 + " Ant", 0.4);

                PlumbLine(T1Postp, T1Antp, 1, 1, 1);
                PlumbLine(T12Postp, T12Antp, 1, 1, 1);
                PlumbLine(T1Postp, T1post, 0.8, 0.2, 0.7);
                PlumbLine(T1Antp, T1Ant, 0.8, 0.2, 0.7);
                PlumbLine(T12Postp, T12Post, 0.8, 0.2, 0.7);
                PlumbLine(T12Antp, T12Ant, 0.8, 0.2, 0.7);

                txtCurrentValue.Text = nameT1 + "-" + nameT12 + ": " + TK_Angle.ToString();



                ren1.GetRenderWindow().Render();
            }



        }



        private void RenderT1SPImeasurement(int selectedDataIndex, int frameIndex, int Index_FemHeadLeft, int Index_FemHeadRight, int Index_T1, string nameT1)
        {
            ren1.RemoveAllViewProps();
            ren1.Clear();
            

            if (Index_FemHeadLeft == -1 || Index_FemHeadRight == -1 || Index_T1 == -1)
            {
                MessageBox.Show("One or more of the landmarks needed for the T1-SPI calculation are not assigned. This measurement will be skipped.", "Not enough input", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                System.Windows.Media.Media3D.Point3D femHeadLeft = new System.Windows.Media.Media3D.Point3D(Convert.ToDouble(DataList[selectedDataIndex].TransformedLandmarkList[Index_FemHeadLeft].Rows[frameIndex]["posX"]), Convert.ToDouble(DataList[selectedDataIndex].TransformedLandmarkList[Index_FemHeadLeft].Rows[frameIndex]["posY"]), Convert.ToDouble(DataList[selectedDataIndex].TransformedLandmarkList[Index_FemHeadLeft].Rows[frameIndex]["posZ"]));
                System.Windows.Media.Media3D.Point3D femHeadRight = new System.Windows.Media.Media3D.Point3D(Convert.ToDouble(DataList[selectedDataIndex].TransformedLandmarkList[Index_FemHeadRight].Rows[frameIndex]["posX"]), Convert.ToDouble(DataList[selectedDataIndex].TransformedLandmarkList[Index_FemHeadRight].Rows[frameIndex]["posY"]), Convert.ToDouble(DataList[selectedDataIndex].TransformedLandmarkList[Index_FemHeadRight].Rows[frameIndex]["posZ"]));
                System.Windows.Media.Media3D.Point3D T1_body = new System.Windows.Media.Media3D.Point3D(Convert.ToDouble(DataList[selectedDataIndex].TransformedLandmarkList[Index_T1].Rows[frameIndex]["posX"]), Convert.ToDouble(DataList[selectedDataIndex].TransformedLandmarkList[Index_T1].Rows[frameIndex]["posY"]), Convert.ToDouble(DataList[selectedDataIndex].TransformedLandmarkList[Index_T1].Rows[frameIndex]["posZ"]));
                double sagValue;
                double CorValue;
                double value3D;
                System.Windows.Media.Media3D.Point3D T1Rotp;


                CalculateSPI(femHeadLeft, femHeadRight, T1_body, nameT1, true, out sagValue, out CorValue, out value3D, out T1Rotp);

               
            }
        }

        private void btnVisualizeT1SPI_Click(object sender, EventArgs e)
        {
            VisualisationCase = "MeasVisualizerT1SPI";
            int selectedDataIndex = dgViewFileNames.SelectedRows[0].Index;
            trackBar1.Visible = true;
            trackBar1.Maximum = DataList[selectedDataIndex].AbsoluteDt.Rows.Count - 1;
            trackBar1.Minimum = 0;

            int frameIndex = 0;

            RenderT1SPImeasurement(selectedDataIndex, trackBar1.Value, cbT1SPI_FemHeadLeft.SelectedIndex, cbT1SPI_FemHeadRight.SelectedIndex, cbSPIT1_T1.SelectedIndex, "T1");

            vtkCamera camera = ren1.GetActiveCamera();
            camera.ParallelProjectionOn();
            ren1.ResetCamera();
            RenderWindow.Render();

        }

        private void flowLayoutPnlT1SPI_Paint(object sender, PaintEventArgs e)
        {

        }

        private void btnVisualizeT9SPI_Click(object sender, EventArgs e)
        {
            VisualisationCase = "MeasVisualizerT9SPI";
            int selectedDataIndex = dgViewFileNames.SelectedRows[0].Index;
            trackBar1.Visible = true;
            trackBar1.Maximum = DataList[selectedDataIndex].AbsoluteDt.Rows.Count - 1;
            trackBar1.Minimum = 0;

            int frameIndex = 0;

            RenderT1SPImeasurement(selectedDataIndex, trackBar1.Value, cbT9SPI_FemHeadLeft.SelectedIndex, cbT9SPI_FemHeadRight.SelectedIndex, cbSPIT9_T9.SelectedIndex, "T9");

            vtkCamera camera = ren1.GetActiveCamera();
            camera.ParallelProjectionOn();
            ren1.ResetCamera();
            RenderWindow.Render();
        }

        private void btnVisualizeCoronalCobbAngles_Click(object sender, EventArgs e)
        {
            VisualisationCase = "MeasVisualizerCorCobs";
            int selectedDataIndex = dgViewFileNames.SelectedRows[0].Index;
            trackBar1.Visible = true;
            trackBar1.Maximum = DataList[selectedDataIndex].AbsoluteDt.Rows.Count - 1;
            trackBar1.Minimum = 0;

            int frameIndex = 0;

            RenderCoronalCobbmeasurement(selectedDataIndex, trackBar1.Value, cb_Cobb_FemHeadLeft.SelectedIndex, cb_Cobb_FemHeadRight.SelectedIndex, cb_Cobb_T1_Left.SelectedIndex, cb_Cobb_T1_Right.SelectedIndex, TXT_Cobb_Superior.Text, cb_Cobb_T12_Left.SelectedIndex, cb_Cobb_T12_Right.SelectedIndex, TXT_Cobb_Inferior.Text);

            vtkCamera camera = ren1.GetActiveCamera();
            camera.ParallelProjectionOn();
            ren1.ResetCamera();
            RenderWindow.Render();
        }

        private void btnVisualizeLL_Click(object sender, EventArgs e)
        {

            VisualisationCase = "MeasVisualizerLL";
            int selectedDataIndex = dgViewFileNames.SelectedRows[0].Index;
            trackBar1.Visible = true;
            trackBar1.Maximum = DataList[selectedDataIndex].AbsoluteDt.Rows.Count - 1;
            trackBar1.Minimum = 0;


            int frameIndex = 0;

            RenderLLmeasurement(selectedDataIndex, trackBar1.Value, cb_LL_FemHeadLeft.SelectedIndex, cb_LL_FemHeadRight.SelectedIndex, cb_LL_T12SupAnt.SelectedIndex, cb_LL_T12SupPost.SelectedIndex, txtSuperior_LL.Text, CB_LL_S1SupANT.SelectedIndex, CB_LL_S1SupPOST.SelectedIndex, txtInferior_LL.Text);

            vtkCamera camera = ren1.GetActiveCamera();
            camera.ParallelProjectionOn();
            ren1.ResetCamera();
            RenderWindow.Render();
        }

        private void btnVisualizeTK_Click(object sender, EventArgs e)
        {
            VisualisationCase = "MeasVisualizerTK";
            int selectedDataIndex = dgViewFileNames.SelectedRows[0].Index;
            trackBar1.Visible = true;
            trackBar1.Maximum = DataList[selectedDataIndex].AbsoluteDt.Rows.Count - 1;
            trackBar1.Minimum = 0;


            int frameIndex = 0;

            RenderTKmeasurement(selectedDataIndex, trackBar1.Value, cb_TK_FemHeadLeft.SelectedIndex, cb_TK_FemHeadRight.SelectedIndex,  cb_TK_T1SupPost.SelectedIndex, cb_TK_T1SupAnt.SelectedIndex, TK_Superior.Text, cb_TK_T12SupPost.SelectedIndex, cb_TK_T12SupAnt.SelectedIndex, TK_Inferior.Text);
      

            vtkCamera camera = ren1.GetActiveCamera();
            camera.ParallelProjectionOn();
            ren1.ResetCamera();
            RenderWindow.Render();
        }

        private void btnAutoCompleteFields_Click(object sender, EventArgs e)
        {
            AutoCompleteParameterFields();
        }

        public void AutoCompleteParameterFields()
        { 
            if(useT2)
            {
                T1name = "thoracic2 - ";
            }
            int index = selectedLandmarks.FindIndex(a => a.BodyName + " - " + a.LandmarkName == "pelvis - Left Fem Head");
            
            //string test = selectedLandmarks[0].BodyName + " - " + selectedLandmarks[0].LandmarkName;
            if (index != -1)
            {
                cbSVA_FemHeadLeft.SelectedIndex = index;
                cbPT_FemHeadLeft.SelectedIndex = index;
                cbT1SPI_FemHeadLeft.SelectedIndex = index;
                cbT9SPI_FemHeadLeft.SelectedIndex = index;
                cb_LL_FemHeadLeft.SelectedIndex = index;
                cb_TK_FemHeadLeft.SelectedIndex = index;
                cb_Cobb_FemHeadLeft.SelectedIndex = index;
            }
            else
            {
                index = selectedLandmarks.FindIndex(a => a.BodyName + " - " + a.LandmarkName == "sacrum - Left Fem Head");
                if (index != -1)
                {
                    cbSVA_FemHeadLeft.SelectedIndex = index;
                    cbPT_FemHeadLeft.SelectedIndex = index;
                    cbT1SPI_FemHeadLeft.SelectedIndex = index;
                    cbT9SPI_FemHeadLeft.SelectedIndex = index;
                    cb_LL_FemHeadLeft.SelectedIndex = index;
                    cb_TK_FemHeadLeft.SelectedIndex = index;
                    cb_Cobb_FemHeadLeft.SelectedIndex = index;
                }
            }

            index = selectedLandmarks.FindIndex(a => a.BodyName + " - " + a.LandmarkName == "pelvis - Right Fem Head");
            if (index != -1)
            {
                cbSVA_FemHeadRight.SelectedIndex = index;
                cbPT_FemHeadRight.SelectedIndex = index;
                cbT1SPI_FemHeadRight.SelectedIndex = index;
                cbT9SPI_FemHeadRight.SelectedIndex = index;
                cb_LL_FemHeadRight.SelectedIndex = index;
                cb_TK_FemHeadRight.SelectedIndex = index;
                cb_Cobb_FemHeadRight.SelectedIndex = index;
            }
            else
            {
                index = selectedLandmarks.FindIndex(a => a.BodyName + " - " + a.LandmarkName == "sacrum - Right Fem Head");
                if (index != -1)
                {
                    cbSVA_FemHeadRight.SelectedIndex = index;
                    cbPT_FemHeadRight.SelectedIndex = index;
                    cbT1SPI_FemHeadRight.SelectedIndex = index;
                    cbT9SPI_FemHeadRight.SelectedIndex = index;
                    cb_LL_FemHeadRight.SelectedIndex = index;
                    cb_TK_FemHeadRight.SelectedIndex = index;
                    cb_Cobb_FemHeadRight.SelectedIndex = index;
                }
            }

            index = selectedLandmarks.FindIndex(a => a.BodyName + " - " + a.LandmarkName == T1name+"SEP_post_mid");
            if (index != -1)
            {
                
                cb_TK_T1SupPost.SelectedIndex = index;
            }
            index = selectedLandmarks.FindIndex(a => a.BodyName + " - " + a.LandmarkName == T1name + "SEP_ant_mid");
            if (index != -1)
            {
                cb_TK_T1SupAnt.SelectedIndex = index;

            }

            index = selectedLandmarks.FindIndex(a => a.BodyName + " - " + a.LandmarkName == "thoracic12 - SEP_post_mid");
            if (index != -1)
            {
               
                cb_LL_T12SupPost.SelectedIndex = index;
            }

            index = selectedLandmarks.FindIndex(a => a.BodyName + " - " + a.LandmarkName == "thoracic12 - IEP_post_mid");
            if (index != -1)
            {
                cb_TK_T12SupPost.SelectedIndex = index;
               
            }

            index = selectedLandmarks.FindIndex(a => a.BodyName + " - " + a.LandmarkName == "thoracic12 - SEP_ant_mid");
            if (index != -1)
            {
                
               
                cb_LL_T12SupAnt.SelectedIndex = index;

            }

            index = selectedLandmarks.FindIndex(a => a.BodyName + " - " + a.LandmarkName == "thoracic12 - IEP_ant_mid");
            if (index != -1)
            {

                cb_TK_T12SupAnt.SelectedIndex = index;
               

            }

            index = selectedLandmarks.FindIndex(a => a.BodyName + " - " + a.LandmarkName == T1name + "Body");
            if (index != -1)
            {
                cbSVA_C7body.SelectedIndex = index;
                cbSPIT1_T1.SelectedIndex = index;
            }
            else
            {
                index = selectedLandmarks.FindIndex(a => a.BodyName + " - " + a.LandmarkName == T1name + "body");
                if (index != -1)
                {
                    cbSVA_C7body.SelectedIndex = index;
                    cbSPIT1_T1.SelectedIndex = index;
                }
            }
            
            
            index = selectedLandmarks.FindIndex(a => a.BodyName + " - " + a.LandmarkName == "thoracic9 - Body");
            if (index != -1)
            {
                cbSPIT9_T9.SelectedIndex = index;
            }
            else
            {

                index = selectedLandmarks.FindIndex(a => a.BodyName + " - " + a.LandmarkName == "thoracic9 - body");
                if (index != -1)
                {
                    cbSPIT9_T9.SelectedIndex = index;
                }
            }

            index = selectedLandmarks.FindIndex(a => a.BodyName + " - " + a.LandmarkName == "sacrum - SEP_post_mid");
            if (index != -1)
            {
                cbSVA_SupPostSacralEndplate.SelectedIndex = index;
                cbPT_SacrumPosterior.SelectedIndex = index;
                CB_LL_S1SupPOST.SelectedIndex = index;
            }
            else
            {
                index = selectedLandmarks.FindIndex(a => a.BodyName + " - " + a.LandmarkName == "pelvis - SEP_post_mid");
                if (index != -1)
                {
                    cbSVA_SupPostSacralEndplate.SelectedIndex = index;
                    cbPT_SacrumPosterior.SelectedIndex = index;
                    CB_LL_S1SupPOST.SelectedIndex = index;
                }

            }

            index = selectedLandmarks.FindIndex(a => a.BodyName + " - " + a.LandmarkName == "sacrum - SEP_ant_mid");
            if (index != -1)
            {
                cbPT_SacrumAnterior.SelectedIndex = index;
                CB_LL_S1SupANT.SelectedIndex = index;
            }
            else
            {
                index = selectedLandmarks.FindIndex(a => a.BodyName + " - " + a.LandmarkName == "pelvis - SEP_ant_mid");
                if (index != -1)
                {
                    cbPT_SacrumAnterior.SelectedIndex = index;
                    CB_LL_S1SupANT.SelectedIndex = index;
                }

            }



            index = selectedLandmarks.FindIndex(a => a.BodyName + " - " + a.LandmarkName == T1name + "SEP_left");
            if (index != -1)
            {

                cb_Cobb_T1_Left.SelectedIndex = index;
            }

            index = selectedLandmarks.FindIndex(a => a.BodyName + " - " + a.LandmarkName == T1name + "SEP_right");
            if (index != -1)
            {

                cb_Cobb_T1_Right.SelectedIndex = index;
            }

            index = selectedLandmarks.FindIndex(a => a.BodyName + " - " + a.LandmarkName == "thoracic12 - IEP_left");
            if (index != -1)
            {

                cb_Cobb_T12_Left.SelectedIndex = index;
            }

            index = selectedLandmarks.FindIndex(a => a.BodyName + " - " + a.LandmarkName == "thoracic12 - IEP_right");
            if (index != -1)
            {

                cb_Cobb_T12_Right.SelectedIndex = index;
            }

            
           

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void cbSVA_FemHeadLeft_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            CheckAllParamterFields();
        }

        public void CheckAllParamterFields()
        {
            cbSVA.Checked = true;
            cbPelvicAngles.Checked = true;
            cbT1SPI.Checked = true;
            cbT9SPI.Checked = true;
            cbLL.Checked = true;
            cbTK.Checked = true;
            cb_Cor_Cobb_Angles.Checked = true;

        }
        private void btnSelectAllLMS_Click(object sender, EventArgs e)
        {
            selectAllAnatomicLandmarks();
        }

        public void selectAllAnatomicLandmarks()
        {

            foreach (TreeNode node in AllLandmarks)
            {
                string nodeTag = node.Tag.ToString();

                if (nodeTag.Contains("_SWSlandmark"))
                {
                    string geometryName = node.Parent.Text;
                    string bodyName = node.Parent.Parent.Text;

                    string LMID = nodeTag.Split('_')[0];

                    Landmark landmark = GetLandmark(Convert.ToInt32(LMID));
                    landmark.GeometryName = geometryName;
                    landmark.BodyName = bodyName;

                    if(cbUserLandmarks.Text == "Current user" || string.IsNullOrEmpty(cbUserLandmarks.Text))
                    {
                        if (landmark._UserName == AppData.localStudyUser._UserName || landmark._UserName.ToLower() == (AppData.globalUser.FirstName + AppData.globalUser.LastName).ToLower())
                        {
                            //if(selectedLandmarks.Contains(landmark))

                            int index = selectedLandmarks.FindIndex(a => a.LandmarkID == landmark.LandmarkID);
                            if (index == -1)
                            {
                                selectedLandmarks.Add(landmark);
                            }
                            //else
                            //{
                            //    MessageBox.Show("This landmark has already been added to the selection.", "Landmark already selected!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            //}
                        }
                    }
                    else
                    {
                        if (landmark._UserName == cbUserLandmarks.Text)
                        {
                            //if(selectedLandmarks.Contains(landmark))

                            int index = selectedLandmarks.FindIndex(a => a.LandmarkID == landmark.LandmarkID);
                            if (index == -1)
                            {
                                selectedLandmarks.Add(landmark);
                            }
                            //else
                            //{
                            //    MessageBox.Show("This landmark has already been added to the selection.", "Landmark already selected!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            //}
                        }

                    }
                   
                }

            }
            UpdateDgViewLandmarks();
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void cb_TK_T1SupAnt_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void exportAllLandmarksToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (TreeNode node in AllLandmarks)
            {
                string nodeTag = node.Tag.ToString();

                if (nodeTag.Contains("_SWSlandmark"))
                {
                    string geometryName = node.Parent.Text;
                    string bodyName = node.Parent.Parent.Text;

                    string LMID = nodeTag.Split('_')[0];

                    Landmark landmark = GetLandmark(Convert.ToInt32(LMID));
                    landmark.GeometryName = geometryName;
                    landmark.BodyName = bodyName;
                    //if (landmark._UserName == AppData.localStudyUser._UserName || landmark._UserName.ToLower() == (AppData.globalUser.FirstName + AppData.globalUser.LastName).ToLower())
                    //{
                        //if(selectedLandmarks.Contains(landmark))

                        int index = selectedLandmarks.FindIndex(a => a.LandmarkID == landmark.LandmarkID);
                        if (index == -1)
                        {
                            selectedLandmarks.Add(landmark);
                        }
                        //else
                        //{
                        //    MessageBox.Show("This landmark has already been added to the selection.", "Landmark already selected!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        //}
                  //  }
                }

            }
            UpdateDgViewLandmarks();
        }
    }
