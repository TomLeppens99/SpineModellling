using Kitware.VTK;
using OpenSim;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using SpineAnalyzer;
using TOD;
using System.Threading;
using g3;
using System.Windows.Media.Media3D;
using System.Numerics;

namespace SpineAnalyzer.ModelVisualization
{
    public partial class frmFundamentalModelComponentProp : Form
    {
        #region Declarations

        //System
        public AppData Appdata;
        public OsimBodyProperty osimBodyProp; //This is actually the child.
        public OsimBodyProperty parentBodyProp;
        public SimModelVisualization SimModelVisualization;
        public Acquisitions.GeometryFiles ReplacementGeometryFiles;
        public OsimJointCoordinateProperty currentCoor;
        public DataBase SQLDB;
        public Subject Subject;
        public List<string> GeomNameList = new List<string>();
        public List<string> JointNameList = new List<string>();
        public List<Landmark> LoadedLandmarks = new List<Landmark>();
        OsimGeometryProperty currentGeomProp;
        private string selectedJointType = string.Empty;

        private double[] newJointCOM_loc = new double[] { 0, 0, 0, };
        private List<Landmark> ChildLMlist = new List<Landmark>();
        private List<Landmark> ParentLMlist = new List<Landmark>();
        private DataSet selectedDsChild = new DataSet();

        bool inferiorReference = true;
        //OPENSIM
        public Model model;
        public State si;
        public string JointProtocol = " ";

        //VTK
        public vtkActor replacingVtkActor;
        public vtkAppendPolyData vtkAppendPolyDataReplacement;
        public Kitware.VTK.RenderWindowControl renderWindowControl1 = new Kitware.VTK.RenderWindowControl();
        public RenderWindowControl renderWindowControlMini = new Kitware.VTK.RenderWindowControl();
        public vtkRenderWindow RenderWindow = vtkRenderWindow.New();
        public vtkRenderer ren1 = vtkRenderer.New();
        public vtkRenderWindowInteractor iren;
        public vtkRenderWindow RenderWindowMini = vtkRenderWindow.New();
        public vtkRenderer renMini = vtkRenderer.New();
        public vtkRenderWindowInteractor irenMini;
        vtkBoxWidget vtkBoxWidget = new vtkBoxWidget();
        vtkBoxWidget vtkBoxWidget2 = new vtkBoxWidget();
        vtkPropPicker propPicker = vtkPropPicker.New();
        private vtkActor COMactor = new vtkActor();
        private vtkActor COMendplateActor = new vtkActor();
        private vtkActor NormalActor = new vtkActor();
        vtkActor sourceActor = new vtkActor();
        vtkActor solutionActor = new vtkActor();
        vtkActor targetActor = new vtkActor();

        public double[] InitialOrientation = { 0, 0, 0 };
        public double[] InitialPosition = { 0, 0, 0 };
        public vtkTransform InitialJoinTransf = new vtkTransform();

        #endregion

        #region Methods

        public frmFundamentalModelComponentProp()
        {
            InitializeComponent();
        }

        private void frmFundamentalModelComponentProp_Load(object sender, EventArgs e)
        {
           
            SetupFMCstuff();


        }


        public void SetupFMCstuff()
        {
            // 
            // renderWindowControl1
            // 
            this.renderWindowControl1.AddTestActors = false;
            this.renderWindowControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.renderWindowControl1.Location = new System.Drawing.Point(623, 4);
            this.renderWindowControl1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.renderWindowControl1.Name = "renderWindowControl1";
            this.renderWindowControl1.Size = new System.Drawing.Size(611, 333);
            this.renderWindowControl1.TabIndex = 4;
            this.renderWindowControl1.TestText = null;
            this.tableLayoutPanel2.Controls.Add(this.renderWindowControl1, 1, 0);


            renderWindowControl1_Load();


            this.renderWindowControlMini.AddTestActors = false;
            this.renderWindowControlMini.Location = new System.Drawing.Point(580, 4);
            this.renderWindowControlMini.Dock = System.Windows.Forms.DockStyle.Fill;
            this.renderWindowControlMini.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.renderWindowControlMini.Name = "renderWindowControlMini";
            this.renderWindowControlMini.Size = new System.Drawing.Size(293, 169);
            this.renderWindowControlMini.TabIndex = 5;
            this.renderWindowControlMini.TestText = null;
            this.renderWindowControlMini.Load += new System.EventHandler(this.renderWindowControlMini_Load);
            this.panel7.Controls.Add(this.renderWindowControlMini);

            PrintPropertyWindow();
            //AddBoxWidget(); 

            SetupPanelJointDefinition();

            setupSelectedDataSetChild();
            setupSelectedDataSetParent();
            StoreInitialTransformJoint();

        }
        private void renderWindowControl1_Load()
        {
            // get a reference to the renderwindow of our renderWindowControl1
            RenderWindow = renderWindowControl1.RenderWindow;
            // get a reference to the renderer
            ren1 = RenderWindow.GetRenderers().GetFirstRenderer();
            // set background color
            ren1.SetBackground(0.2, 0.3, 0.4);
            iren = RenderWindow.GetInteractor();
            RenderWindow.SetInteractor(iren);
            ren1.ResetCameraClippingRange();

        }

        private void PopulateComboBox(ComboBox combobox, List<string> stringList)
        {
            foreach (string name in stringList)
            {
                combobox.Items.Add(name);
            }

        }

        private void combBGeoms_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentGeomProp = osimBodyProp._OsimGeometryPropertyList[combBGeoms.SelectedIndex];
            propGridGeom.SelectedObject = currentGeomProp;
        }

        private void combBJoints_SelectedIndexChanged(object sender, EventArgs e)
        {
            propGridGeom.SelectedObject = osimBodyProp.osimJointProperty;
        }

        private void rBGeometries_CheckedChanged(object sender, EventArgs e)
        {

            if (rBGeometries.Checked)
            {
                combBGeoms.Enabled = true;
                propGridGeom.SelectedObject = osimBodyProp._OsimGeometryPropertyList[combBGeoms.SelectedIndex];
                tabControl1.SelectedTab = tPageGeom;
            }
            else
            {
                combBGeoms.Enabled = false;
            }

        }

        private void rBJoints_CheckedChanged(object sender, EventArgs e)
        {
            if (rBJoints.Checked)
            {
                combBJoints.Enabled = true;
                propGridGeom.SelectedObject = osimBodyProp.osimJointProperty;
                tabControl1.SelectedTab = tPageJoint;
            }
            else
            {
                combBJoints.Enabled = false;
            }
        }

        private void btnFile1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.Cancel)
            { return; }

            string fileSourcePath = openFileDialog1.FileName;

            CreateGeometryInMiniRenderer(fileSourcePath);
        }

        public void PrintPropertyWindow()
        {
            if(osimBodyProp.objectName.ToString().ToLower() == "ground" )
            { this.Close();
                return;
            }

            //Fill the basic properties
            lblName.Text = "Name:      " + osimBodyProp.objectName.ToString();
            txtMass.Text = osimBodyProp.mass.ToString();
            Vec3 rVec = new Vec3();
            osimBodyProp.body.getMassCenter(rVec);
            txtMassCX.Text = rVec.get(0).ToString();
            txtMassCY.Text = rVec.get(1).ToString();
            txtMassCZ.Text = rVec.get(2).ToString();
            txtColorV_R.Text = osimBodyProp.colorR.ToString();
            txtColorV_G.Text = osimBodyProp.colorG.ToString();
            txtColorV_B.Text = osimBodyProp.colorB.ToString();
            txtScaleX.Text = osimBodyProp.scaleFactors.get(0).ToString();
            txtScaleY.Text = osimBodyProp.scaleFactors.get(1).ToString();
            txtScaleZ.Text = osimBodyProp.scaleFactors.get(2).ToString();
            parentBodyProp = SimModelVisualization.getSpecifiedBodyProperty(osimBodyProp.osimJointProperty.parentBody);
            osimBodyProp.osimJointProperty.osimParentBodyProp = parentBodyProp;
            txtParentBody.Text = osimBodyProp._parentBody.getName().ToString();


            lblTotalMass.Text = "Total Mass: " + model.getTotalMass(si).ToString() + " kg";
            cBisground.Checked = osimBodyProp.isGround;

            FillGeomertryNameList();
            PopulateComboBox(combBGeoms, GeomNameList);

            FillJointNameList();
            PopulateComboBox(combBJoints, JointNameList);

            combBJoints.SelectedIndex = 0;
            if (combBGeoms.Items.Count != 0)
            {
                combBGeoms.SelectedIndex = 0;
            }
            //Fill the renderer
            ren1.AddActor(osimBodyProp.assembly);
            //osimBodyProp.assembly.SetUserTransform(osimBodyProp.transform); 
            ren1.AddActor(osimBodyProp.osimJointProperty.jointActor);

            osimBodyProp.osimJointProperty.MakeJointAxes();
            ren1.AddActor(osimBodyProp.osimJointProperty.axesActor);

            if (osimBodyProp.osimJointProperty.osimParentBodyProp._OsimGeometryPropertyList.Count != 0)
            {
                osimBodyProp.osimJointProperty.osimParentBodyProp._OsimGeometryPropertyList[0]._vtkActor.GetProperty().SetDiffuseColor(0.01, 0.01, 0.01);
                osimBodyProp.osimJointProperty.osimParentBodyProp._OsimGeometryPropertyList[0]._vtkActor.GetProperty().SetOpacity(0.20);
            }


         
            ren1.AddActor(osimBodyProp.osimJointProperty.osimParentBodyProp.assembly);
            ren1.ResetCamera();
            //osimBodyProp.osimJointProperty.osimParentBodyProp.colorR = 0.05;
            //osimBodyProp.osimJointProperty.osimParentBodyProp.colorG = 0.05;
            //osimBodyProp.osimJointProperty.osimParentBodyProp.colorB = 0.05;
            //osimBodyProp.osimJointProperty.osimParentBodyProp.assembly.GetParts();


            ResetCutters();

    

            AddAxesWithLabel();


            SetupSliders();

            Model2Treeview(treeViewJoint);

            lblChildtext.Text = "Saved Landmarks on the Child Body: " + osimBodyProp.objectName;
            lblParentText.Text = "Saved Landmarks on the Parent Body: " + parentBodyProp.objectName;
            //FillCoordinateGrid();


        }

        private void ResetCutters()
        {
            if (osimBodyProp._OsimGeometryPropertyList.Count != 0 && parentBodyProp._OsimGeometryPropertyList.Count !=0)
            {
                childPolydata = osimBodyProp._OsimGeometryPropertyList[0]._vtkPolyData;
                parentPolydata = parentBodyProp._OsimGeometryPropertyList[0]._vtkPolyData;
            }

        }
        public vtkAxesActor axesActorAllIn = new vtkAxesActor();

        private void AddAxesWithLabel()
        {
            axesActorAllIn.SetUserTransform(osimBodyProp.osimJointProperty._vtkTransform);

            axesActorAllIn.SetTotalLength(0.05, 0.05, 0.05);
            axesActorAllIn.SetScale(0.10);
            
            ren1.AddActor(axesActorAllIn);
           
            //axesActor.SetXAxisLabelText("X");

        }

        private void grdSettings_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void grdSettings_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            //If image changes between Lock/Unlock
            if (grdSettings.Columns[e.ColumnIndex].Name == "LOCK")
            {
                bool enable = ((string)grdSettings.Rows[e.RowIndex].Cells["LOCK"].Tag != "LOCK");
                enableCell(grdSettings.Rows[e.RowIndex].Cells["VALUE"], enable);
                enableCell(grdSettings.Rows[e.RowIndex].Cells["DEFAULT"], enable);
            }
        }

        private void slider_setup(int intRow, double dblValue, double dblMin, double dblMax, int intDecimals)
        {
            ColorSlider cs = ((SliderCell)grdSettings.Rows[intRow].Cells["VALUE"]).Slider;
            cs.Maximum = dblMax; //ROUND
            cs.Minimum = dblMin;
            cs.Value = dblValue;
            cs.Decimals = intDecimals;

            cs.ShowSmallScale = false;
            cs.ShowDivisionsText = false;
            cs.TickStyle = TickStyle.None;

            //cs.SmallChange = 1;
            //cs.LargeChange = 5;
            cs.updownStyle = ColorSlider.UpDownStyle.Maximum;

            //add eventhandler + info to track the scrolling of the slider
            cs.ValueChanged += new System.EventHandler(this.SliderValueChanged);
            cs.Tag = ((SliderCell)grdSettings.Rows[intRow].Cells["VALUE"]); //Save reference to current cell into the slider
            //assign slidervalue to cellvalue
            grdSettings.Rows[intRow].Cells["VALUE"].Value = cs.Value; //IMPORTANT always set the value of the cell to the value of the slider !!
        }

        private void enableCell(DataGridViewCell cell, bool enable)
        {
            //toggle read-only state
            cell.ReadOnly = !enable;
            if (enable)
            {
                //restore cell style to the default value
                cell.Style.BackColor = cell.OwningColumn.DefaultCellStyle.BackColor;
                cell.Style.ForeColor = cell.OwningColumn.DefaultCellStyle.ForeColor;
                //If cell is of type SliderCell then also disable the containing Slider
                if (cell.OwningColumn.CellType == typeof(TOD.SliderCell))
                {
                    ((SliderCell)cell).Slider.Enabled = true;
                }
            }
            else
            {
                //gray out the cell
                cell.Style.BackColor = Color.LightGray;
                cell.Style.ForeColor = Color.DarkGray;
                //If cell is of type SliderCell then also disable the containing Slider
                if (cell.OwningColumn.CellType == typeof(TOD.SliderCell))
                {
                    ((SliderCell)cell).Slider.Enabled = false;
                }
            }
        }


        #region Treeview Stuff
        private void FillGeomertryNameList()
        {
            foreach (OsimGeometryProperty geomProp in osimBodyProp._OsimGeometryPropertyList)
            {
                GeomNameList.Add(geomProp.objectName);
            }
        }

        private void FillJointNameList()
        {
            //foreach (OsimJointProperty jointProp in osimBodyProp.joi _OsimGeometryPropertyList)
            //{
            JointNameList.Add(osimBodyProp.jointName);
            //}
        }

        private void Model2Treeview(TreeView treeView1)
        {
            ClearTreeview(treeView1);
            TV_SetRootNodes(treeView1);
            TV_SetCoordinateSetNodes(treeView1);
            //TV_SetSpatialTransformNodes(treeView1);
            treeView1.Nodes[0].Expand();
            treeView1.Nodes[0].Nodes[0].Expand();
            treeView1.ExpandAll();
        }

        private void TV_SetRootNodes(TreeView treeView1)
        {
            treeView1.Nodes.Add(osimBodyProp.osimJointProperty.objectName, osimBodyProp.osimJointProperty.objectName);
            treeView1.Nodes[osimBodyProp.osimJointProperty.objectName].Expand();
            treeView1.Nodes[osimBodyProp.osimJointProperty.objectName].Nodes.Add("CoordinateSet", "CoordinateSet");
            treeView1.Nodes[osimBodyProp.osimJointProperty.objectName].Nodes.Add("SpatialTransform", "SpatialTransform");
        }

        private void TV_SetCoordinateSetNodes(TreeView treeView1)
        {
            for (int i = 0; i < osimBodyProp.body.getJoint().getCoordinateSet().getSize(); i++)
            {
                Coordinate coor = osimBodyProp.body.getJoint().getCoordinateSet().get(i);

                treeView1.Nodes[0].Nodes["CoordinateSet"].Nodes.Add(coor.getName(), coor.getName());
            }
        }

        private void TV_SetSpatialTransformNodes(TreeView treeView1)
        {

            CustomJoint custJoint = (CustomJoint)osimBodyProp.body.getJoint();
            SpatialTransform sptransf = custJoint.getSpatialTransform();

            //sptransf.getNumProperties();
            for (int i = 0; i < 6; i++)
            {
                TransformAxis transfAxis = sptransf.getTransformAxis(i);

                treeView1.Nodes[0].Nodes["SpatialTransform"].Nodes.Add(transfAxis.getName(), transfAxis.getName());
            }
        }

        private void ClearTreeview(TreeView treeView1)
        {
            treeView1.Nodes.Clear();
        }

        #endregion


        #region Geometry Replacement stuff

        private void btnReplaceGeometry_Click(object sender, EventArgs e)
        {

            using (Acquisitions.frmSelectGeometryFile frmGeomFile = new Acquisitions.frmSelectGeometryFile())
            {
                //Pass data to search form
                frmGeomFile.AppData = Appdata;
                frmGeomFile.SQLDB = SQLDB;
                frmGeomFile.Subject = Subject;
                frmGeomFile.ShowDialog();
                ReplacementGeometryFiles = frmGeomFile.GeometryFiles;  //Returning the Object: Model

            }


            if (ReplacementGeometryFiles == null)
            { return; }

            Cursor.Current = Cursors.WaitCursor;

            btnConfirm.Enabled = true;
            btnConfirm.Visible = true;
            CreateGeometryInMiniRenderer(Path.GetFullPath(Appdata.AcquisitionDir) + ReplacementGeometryFiles.Directory);
            propertyGridGeometryFile.SelectedObject = ReplacementGeometryFiles;
            btnReplaceGeometry.Enabled = false;
            Application.UseWaitCursor = false;

        }

        private void CreateGeometryInMiniRenderer(string file)
        {
            vtkTransform vtkTransform = vtkTransform.New();

            vtkTransformFilter vtkTransformFilterOrignal = vtkTransformFilter.New();
            vtkTransformFilter vtkTransformFilterDeformed = vtkTransformFilter.New();
            vtkPolyData _vtkPolyData = new vtkPolyData();

            string _extension = Path.GetExtension(file);

            if (_extension == ".vtp")
            {
                vtkXMLPolyDataReader polyDataReaderOriginal = new vtkXMLPolyDataReader();
                polyDataReaderOriginal.SetFileName(file);
                vtkTransformFilterOrignal.SetInputConnection(polyDataReaderOriginal.GetOutputPort());
                //bodyProp.AddPolyDataToListOriginals(polyDataReaderOriginal.GetOutput());

                _vtkPolyData = polyDataReaderOriginal.GetOutput();

                vtkXMLPolyDataReader polyDataReaderDeformed = new vtkXMLPolyDataReader();
                polyDataReaderDeformed.SetFileName(file);
                vtkTransformFilterDeformed.SetInputConnection(polyDataReaderDeformed.GetOutputPort());
                //bodyProp.AddPolyDataToListDeformed(polyDataReaderDeformed.GetOutput());
            }

            if (_extension == ".stl")
            {
                vtkSTLReader polyDataReader = new vtkSTLReader();
                polyDataReader.SetFileName(file);
                vtkTransformFilterOrignal.SetInputConnection(polyDataReader.GetOutputPort());
                //bodyProp.AddPolyDataToListDeformed(polyDataReader.GetOutput());
                //bodyProp.AddPolyDataToListOriginals(polyDataReader.GetOutput());
                _vtkPolyData = polyDataReader.GetOutput();
            }

            if (_extension == ".obj")
            {
                vtkOBJReader polyDataReader = new vtkOBJReader();
                polyDataReader.SetFileName(file);
                polyDataReader.Update();
                vtkTransformFilterOrignal.SetInputConnection(polyDataReader.GetOutputPort());
                //bodyProp.AddPolyDataToListDeformed(polyDataReader.GetOutput());
                //bodyProp.AddPolyDataToListOriginals(polyDataReader.GetOutput());
                _vtkPolyData = polyDataReader.GetOutput();
            }
            if (_extension != ".vtp" && _extension != ".stl" && _extension != ".obj")
            {
                MessageBox.Show("Only geometry files of type VTP, STL or OBJ can be read.", "Geometry Type Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            vtkTransformFilterOrignal.SetTransform(vtkTransform);


            vtkAxes axes = vtkAxes.New();
            axes.SetOrigin(0, 0, 0);
            axes.SetScaleFactor(0.1);


            //vtkVectorText textLabel = vtkVectorText.New();
            //textLabel.SetText(fileName);
            //vtkFollower follower = vtkFollower.New();


            vtkAppendPolyDataReplacement = vtkAppendPolyData.New();
            vtkAppendPolyDataReplacement.AddInputConnection(vtkTransformFilterOrignal.GetOutputPort());
            vtkAppendPolyDataReplacement.AddInputConnection(axes.GetOutputPort());    //COMMENT THIS IF YOU WANT TO HIDE THE AXES

            vtkPolyDataMapper vtkPolyDataMapper = vtkPolyDataMapper.New();
            vtkPolyDataMapper.SetInputConnection(vtkAppendPolyDataReplacement.GetOutputPort());
            //vtkPolyDataMapper.SetProgressText(fileName);   //This is used as an Actor ID.

            replacingVtkActor = new vtkActor();
            replacingVtkActor.SetMapper(vtkPolyDataMapper);

            renMini.AddActor(replacingVtkActor);
            //renMini.Render();
            RenderWindowMini.Render();

            //replacingVtkActor.GetProperty().SetColor(_geomColorR, _geomColorG, _geomColorB);
            //replacingVtkActor.SetScale(_geomScaleFactors.get(0), _geomScaleFactors.get(1), _geomScaleFactors.get(2));

        }

        private void renderWindowControlMini_Load(object sender, EventArgs e)
        {
            // get a reference to the renderwindow of our renderWindowControl1
            RenderWindowMini = renderWindowControlMini.RenderWindow;
            // get a reference to the renderer
            renMini = RenderWindowMini.GetRenderers().GetFirstRenderer();
            // set background color
            renMini.SetBackground(0.2, 0.3, 0.4);
            irenMini = RenderWindowMini.GetInteractor();
            RenderWindowMini.SetInteractor(irenMini);
        }

        private void btnConfirm_Click(object sender, EventArgs e)
        {

            DialogResult dialogResult = MessageBox.Show("Are you sure you want to replace the geometry?", "Confirm geometry replacement", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                osimBodyProp._OsimGeometryPropertyList[combBGeoms.SelectedIndex]._vtkActor.GetMapper().SetInputConnection(vtkAppendPolyDataReplacement.GetOutputPort());
                //renMini.Render();
                //ren1.Render();
                ren1.GetRenderWindow().Render();
                osimBodyProp._OsimGeometryPropertyList[combBGeoms.SelectedIndex].displayGeometry.setGeometryFile(Path.GetFileName(Appdata.AcquisitionDir + "\\" + ReplacementGeometryFiles.Directory));
                osimBodyProp._OsimGeometryPropertyList[combBGeoms.SelectedIndex].WriteGeometry(Path.GetFileName(Appdata.AcquisitionDir + ReplacementGeometryFiles.Directory));
                osimBodyProp._OsimGeometryPropertyList[combBGeoms.SelectedIndex].ReadGeometry(osimBodyProp._OsimGeometryPropertyList[combBGeoms.SelectedIndex].displayGeometry);
                btnConfirm.Enabled = false;
                btnConfirm.Visible = false;
            }
            else if (dialogResult == DialogResult.No)
            {
                return;
            }
        }

        #endregion


        #region Joint Motion

        private void SliderValueChanged(object sender, EventArgs e)
        {
            ColorSlider cs = ((ColorSlider)sender);
            SliderCell slidercell = ((SliderCell)cs.Tag);


            double degrees = (double)cs.Value;
            double radian = SimModelVisualization.DegreeToRadian(degrees);
            OsimJointProperty jointProp = osimBodyProp.osimJointProperty;

            //OsimJointCoordinateProperty jointCoorProp = jointProp.osimJointCoordinatePropertyList[0];
            SimModelVisualization.ChangeJointCoordinate(jointProp, radian, ren1, currentCoor);
            UpdateLoadedLandmarkTransforms();

            //  txtID.Text = ((string)slidercell.OwningRow.Cells["ID"].Value) + Convert.ToString(cs.Value) + Environment.NewLine + txtID.Text;
        }

        private void SetupSliders()
        {

            //+------------------------------------------------------------------+
            //|                          Local Data                              |
            //+------------------------------------------------------------------+
            int intRow;             //Variable with rowindex
            //+------------------------------------------------------------------+
            //|                          Customize the grid                      |
            //+------------------------------------------------------------------+
            grdSettings.AllowUserToAddRows = false;     //Do not allow user to add or delete rows
            grdSettings.AllowUserToDeleteRows = false;
            grdSettings.RowHeadersVisible = false;      //Hide the RowSelector
            grdSettings.Columns["VALUE"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;  //Slider resized to fill complete grid
            grdSettings.Columns["DEFAULT"].DefaultCellStyle.Format = "N2"; //Set column Default to Numeric 2 decimals


            //Fill the DOF's
            for (int i = 0; i < osimBodyProp.body.getJoint().getCoordinateSet().getSize(); i++)
            {

                Coordinate coor = osimBodyProp.body.getJoint().getCoordinateSet().get(i);



                intRow = grdSettings.Rows.Add();        //Add an empty Row
                grdSettings.Rows[intRow].Height = 60;   //Set row height
                                                        //Set text in first column cell
                grdSettings.Rows[intRow].Cells["ID"].Value = coor.getName();
                //Create Image
                if (coor.getLocked(si))
                {
                    grdSettings.Rows[intRow].Cells["Lock"].Tag = "LOCK";
                    grdSettings.Rows[intRow].Cells["Lock"].Value = Properties.Resources.lock32;
                }
                else
                {
                    grdSettings.Rows[intRow].Cells["Lock"].Tag = "UNLOCK";
                    grdSettings.Rows[intRow].Cells["Lock"].Value = Properties.Resources.unlock32;
                }

                //Set default value
                grdSettings.Rows[intRow].Cells["Default"].Value = coor.getDefaultValue();

                //Create Slider
                slider_setup(intRow, SimModelVisualization.RadianToDegree(coor.getValue(si)), SimModelVisualization.RadianToDegree(coor.getRangeMin()), SimModelVisualization.RadianToDegree(coor.getRangeMax()), 2);


                //richTextBox1.AppendText(coor.getName());
                //richTextBox1.Text += Environment.NewLine;
                //richTextBox1.AppendText(coor.getDefaultValue().ToString());
                //richTextBox1.Text += Environment.NewLine;

                //TransformAxis axis = new TransformAxis();
                ////Model model = new Model();
                //osimBodyProp.body.getJoint().GetType();

            }



        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {

            double degrees = (double)trackBar1.Value;
            double radian = SimModelVisualization.DegreeToRadian(degrees);
            OsimJointProperty jointProp = osimBodyProp.osimJointProperty;

            OsimJointCoordinateProperty jointCoorProp = jointProp.osimJointCoordinatePropertyList[0];
            SimModelVisualization.ChangeJointCoordinate(jointProp, radian, ren1, jointCoorProp);
        }

        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            double degrees = (double)trackBar2.Value;
            double radian = SimModelVisualization.DegreeToRadian(degrees);
            OsimJointProperty jointProp = osimBodyProp.osimJointProperty;

            OsimJointCoordinateProperty jointCoorProp = jointProp.osimJointCoordinatePropertyList[1];
            SimModelVisualization.ChangeJointCoordinate(jointProp, radian, ren1, jointCoorProp);
        }

        private void trackBar3_Scroll(object sender, EventArgs e)
        {
            double degrees = (double)trackBar3.Value;
            double radian = SimModelVisualization.DegreeToRadian(degrees);
            OsimJointProperty jointProp = osimBodyProp.osimJointProperty;

            OsimJointCoordinateProperty jointCoorProp = jointProp.osimJointCoordinatePropertyList[2];
            SimModelVisualization.ChangeJointCoordinate(jointProp, radian, ren1, jointCoorProp);
        }

        #endregion


        #region Joint Defintion

        private void btntestJointReplacement_Click(object sender, EventArgs e)
        {
            SimModelVisualization.JointTranslate(osimBodyProp.osimJointProperty.joint, 0.05, 0, 0);
           // ren1.Render();
            ren1.GetRenderWindow().Render();
        }
        //public void Interaction(vtkObject sender, vtkObjectEventArgs e)
        //{
        //    vtkTransform t = vtkTransform.New();

        //    boxWidget.GetTransform(t);

        //    //vtkBoxRepresentation.SafeDownCast(boxWidget.GetRepresentation()).GetTransform(t);
        //    osimBodyProp.osimJointProperty.jointActor.SetUserTransform(t);

        //}

        private void AddBoxWidget()
        {
            vtkBoxWidget vtkBoxWidget = vtkBoxWidget.New();
            vtkBoxWidget.SetInteractor(iren);
            vtkBoxWidget.SetProp3D(osimBodyProp.osimJointProperty.jointActor);
            //vtkBoxWidget.SetPlaceFactor(1.25);
            vtkBoxWidget.PlaceWidget();

            vtkBoxWidget.StartInteractionEvt += new vtkObject.vtkObjectEventHandler(BeginInteraction);
            //= new vtkBoxCallBack.New();
            vtkMyBoxCallBack vtkMyBoxCallBack = (vtkMyBoxCallBack)vtkMyBoxCallBack.New();
            vtkMyBoxCallBack.SetActor(osimBodyProp.osimJointProperty.jointActor);


            // vtkBoxCallBack boxCallback = vtkBoxCallBack.New2();
            //vtkBoxCallBack2
            //vtkBoxCallBack boxCallback = (vtkBoxCallBack)vtkBoxCallBack.New();

            //  boxCallback.SetActor(osimBodyProp.osimJointProperty.jointActor);

            //vtkBoxWidget.InteractionEvt += new vtkObject.vtkObjectEventHandler(Interaction);

            vtkBoxWidget.On();
            vtkBoxWidget.AddObserver((uint)vtkCommand.EventIds.KeyPressEvent, vtkMyBoxCallBack, 1.0f);
        }

        class vtkMyBoxCallBack : vtkCommand
        {
            //public static vtkMyBoxCallBack  New()
            //{
            //     return new vtkMyBoxCallBack();
            //     //return vtkBoxCallBack2();
            //}

            public vtkActor m_actor;

            public void SetActor(vtkActor actor)
            {
                m_actor = actor;
            }

            public virtual void Execute(vtkObject sender, vtkObjectEventArgs e)
            {

                vtkTransform t = vtkTransform.New();


                vtkBoxWidget2 vtkBoxWidget2 = new vtkBoxWidget2();
                vtkBoxWidget2.SafeDownCast(sender);

                //vtkBoxWidget vtkBoxWidget = new vtkBoxWidget();
                //vtkBoxWidget.SafeDownCast(sender);

                //vtkBoxWidget.GetTransform(t);
                //vtkBoxWidget.GetProp3D().SetUserTransform(t);

                vtkBoxRepresentation.SafeDownCast(vtkBoxWidget2.GetRepresentation()).GetTransform(t);
                this.m_actor.SetUserTransform(t);

            }
            //public vtkMyBoxCallBack vtkMyBoxCallBack()
            //{ }

            public vtkMyBoxCallBack vtkBoxCallBack2()
            {
                return new vtkMyBoxCallBack();
            }
        };

        public static void BeginInteraction(vtkObject sender, vtkObjectEventArgs e)
        {

        }

        public void ExecuteHandle(vtkObject sender, vtkObjectEventArgs e)
        {
            vtkTransform t = new vtkTransform();
            vtkBoxWidget.GetTransform(t);
            double[] translation = t.GetPosition();
            double[] rotation = t.GetOrientation();
            double[] rotationWXYZ = t.GetOrientationWXYZ();
            vtkTransform d = new vtkTransform();
            d.SetInput(osimBodyProp.osimJointProperty._vtkTransform);
            d.PostMultiply();

            d.RotateY(rotation[1]);
            d.RotateX(rotation[0]);

            d.RotateZ(rotation[2]);

            // d.RotateWXYZ(rotationWXYZ[0], rotationWXYZ[1], rotationWXYZ[2], rotationWXYZ[3]);
            d.Translate(translation[0], translation[1], translation[2]);

            osimBodyProp.osimJointProperty._jointActor.SetUserTransform(d);
            osimBodyProp.osimJointProperty.axesActor.SetUserTransform(d);
            axesActorAllIn.SetUserTransform(d);

            /////Printing output
            //double[] newPosition = d.GetPosition();
            //double X = newPosition[0];
            //double Y = newPosition[1];
            //double Z = newPosition[2];
            //richTextBox1.AppendText(X.ToString() + "   " + Y.ToString() + "    " + Z.ToString());
            //richTextBox1.Text += Environment.NewLine;
            //richTextBox1.SelectionStart = richTextBox1.Text.Length;
            //// scroll it automatically
            //richTextBox1.ScrollToCaret();
            //// selectedCpProp.updateCpInModel(SimModelVisualization.si, t);
            //selectedCpProp.osimForceProperty.UpdateMuscleLineActorTransform();
            //dgViewMuslcePath.Refresh();
        }

       

        public void btnCalcCOM_Click(object sender, EventArgs e)
        {





        }


       

        private void btnCOMfromCombinedMesh_Click(object sender, EventArgs e)
        {

            //Combine the polydata of both endplates in to a single polydata object. 
            vtkAppendPolyData ChildParentPolyDataCombined = vtkAppendPolyData.New();
            ChildParentPolyDataCombined.AddInput(childPolydata);
            ChildParentPolyDataCombined.AddInput(parentPolydata);
            ChildParentPolyDataCombined.Update();



            //Calculate the average of the new object ==> determine coordinates for the now joint location.
            double X = 0;
            double Y = 0;
            double Z = 0;
            int nbPoints = Convert.ToInt32(ChildParentPolyDataCombined.GetOutput().GetPoints().GetNumberOfPoints());
            for (int i = 1; i < nbPoints; i++)
            {
                double[] pos = ChildParentPolyDataCombined.GetOutput().GetPoints().GetPoint(i);
                X += pos[0];
                Y += pos[1];
                Z += pos[2];

            }
            X = X / nbPoints;
            Y = Y / nbPoints;
            Z = Z / nbPoints;

            //Push location of new joint to the private variable.
            newJointCOM_loc = new double[] { X, Y, Z };


            //for vizualisation
            MakeCOMactor(newJointCOM_loc);


            PlaceJointInCOM();

        }


        private void PlaceJointInCOM()
        {
            vtkTransform t = new vtkTransform();
            vtkBoxWidget.GetTransform(t);
            double[] location = { newJointCOM_loc[0], newJointCOM_loc[1], newJointCOM_loc[2] };
            //double[] orientation = t.GetOrientation();
            vtkTransform d = new vtkTransform();
            d.SetInput(osimBodyProp.osimJointProperty._vtkTransform);
            d.PostMultiply();
            d.Translate(location[0] - osimBodyProp.osimJointProperty._vtkTransform.GetPosition()[0], location[1] - osimBodyProp.osimJointProperty._vtkTransform.GetPosition()[1], location[2] - osimBodyProp.osimJointProperty._vtkTransform.GetPosition()[2]);




            double[] locationF = d.GetPosition();
            double[] orientationF = d.GetOrientation();

          

            osimBodyProp.osimJointProperty._jointActor.SetUserTransform(d);
            osimBodyProp.osimJointProperty.axesActor.SetUserTransform(d);

            SimModelVisualization.SetJointPlacement(osimBodyProp.osimJointProperty.joint, osimBodyProp.transform, (vtkTransform)osimBodyProp.osimJointProperty._jointActor.GetUserTransform(), parentBodyProp.transform);

            osimBodyProp.osimJointProperty.ReadJoint();
            osimBodyProp.osimJointProperty.SetTransformation();

            ren1.GetRenderWindow().Render();


        }

        private void btnJoint2COM_Click(object sender, EventArgs e)
        {


        }

        #endregion


        #region Landmarks and visualisation

        private void setupSelectedDataSetChild()
        {
            DataTable table = new DataTable();
            table.Columns.Add("LandmarkID", typeof(string));
            table.Columns.Add("LandmarkName", typeof(string));
            table.Columns.Add("UserName", typeof(string));
            selectedDsChild.Tables.Add(table);

        }

        private DataSet selectedDsParent = new DataSet();

        private void setupSelectedDataSetParent()
        {
            DataTable table = new DataTable();
            table.Columns.Add("LandmarkID", typeof(string));
            table.Columns.Add("LandmarkName", typeof(string));
            table.Columns.Add("UserName", typeof(string));
            selectedDsParent.Tables.Add(table);

        }


        private void btnChildAdd2Selection_Click(object sender, EventArgs e)
        {


            //Clear DataGrid
            dgViewSelectedLMChild.DataSource = null;
            dgViewSelectedLMChild.Refresh();


            foreach (DataGridViewRow DataGridViewRow in returnSelectedDataRows(dataGridViewLMchild))
            {
                int number = Convert.ToInt32(DataGridViewRow.Cells["LandmarkID"].Value);
                //Build SQL Command (=SQL select statement + Parameters)
                SqlCommand SQLcmd = new SqlCommand();

                string SQLselect;


                SQLselect = "SELECT LandmarkID, LandmarkName, UserName FROM GeometryLandmarks where LandmarkID = @LandmarkID";
                SQLcmd.Parameters.AddWithValue("@LandmarkID", number);


                SQLcmd.CommandText = SQLselect;

                //Set Database
                DataBase SQLDB = new DataBase(Appdata.SQLServer, Appdata.SQLDatabase, Appdata.SQLAuthSQL, Appdata.SQLUser, Appdata.SQLPassword);

                //Fill a dataset with selected records
                DataSet ds = new DataSet();
                SQLDB.ReadDataSet(SQLcmd, ref ds);
                selectedDsChild.Tables[0].Rows.Add(ds.Tables[0].Rows[0].ItemArray);
            }

            // you can make the grid readonly.
            dgViewSelectedLMChild.ReadOnly = true;
            dgViewSelectedLMChild.DataSource = selectedDsChild.Tables[0];
            // Resize the DataGridView columns to fit the newly loaded content.
            dgViewSelectedLMChild.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.ColumnHeader);



        }

        private List<DataGridViewRow> returnSelectedDataRows(DataGridView dgView)
        {
            List<DataGridViewRow> dataRows = new List<DataGridViewRow>();

            //Check if rows selected
            int RowCount = dgView.SelectedRows.Count;

            // if (RowCount == 0)
            //return;

            for (int i = 0; i < RowCount; i++)
            {
                //Get row of clicked cell
                DataGridViewRow row = dgView.Rows[dgView.SelectedRows[i].Index];

                if (row.DefaultCellStyle.BackColor == Color.LightGray)
                {
                }
                else
                {
                    dataRows.Add(row);
                    row.DefaultCellStyle.BackColor = Color.LightGray;
                }
            }
            return dataRows;
        }


        private void btnSelectLMs_Click(object sender, EventArgs e)
        {
            HideAllPanels();
            panelLandmarks.Dock = DockStyle.Fill;
            panelLandmarks.Visible = true;

            if (currentGeomProp.geometryFileObject != null)
            {
                refreshLandmarks(currentGeomProp.geometryFileObject, dataGridViewLMchild);
            }
            if (parentBodyProp._OsimGeometryPropertyList[0].geometryFileObject != null)
            {
                refreshLandmarks(parentBodyProp._OsimGeometryPropertyList[0].geometryFileObject, dataGridViewLMparent);
            }

        }

        private void ShowAllLMs(Acquisitions.GeometryFiles goemFiles, DataGridView dgView, OsimBodyProperty bodyProp)
        {
            //Set Database
            SQLDB = new DataBase(Appdata.SQLServer, Appdata.SQLDatabase, Appdata.SQLAuthSQL, Appdata.SQLUser, Appdata.SQLPassword);

            //Check if rows selected
            int RowCount = dgView.Rows.Count; // (DataGridViewElementStates.Selected);

            if (RowCount == 0)
                return;

            for (int i = 0; i < RowCount; i++)
            {
                //Get row of clicked cell
                DataGridViewRow row = dgView.Rows[dgView.Rows[i].Index];
                //Get column AcquisitionNumber
                DataGridViewCell cell = row.Cells["LandmarkID"];
                //create AcquisitionObject to be deleted

                Landmark SelectedLandmark = new Landmark(SQLDB, Subject, goemFiles.GeometryNumber, Convert.ToInt32(cell.Value), Appdata);
                SelectedLandmark.bodyProp = bodyProp;
                double[] p = new double[] { SelectedLandmark.coorX, SelectedLandmark.coorY, SelectedLandmark.coorZ };
                MakeLMactor(p, SelectedLandmark, bodyProp);
                LoadedLandmarks.Add(SelectedLandmark);
            }
        }

        private void ShowSelectedLMs(Acquisitions.GeometryFiles goemFiles, DataGridView dgView, OsimBodyProperty bodyProp)
        {
            //Set Database
            SQLDB = new DataBase(Appdata.SQLServer, Appdata.SQLDatabase, Appdata.SQLAuthSQL, Appdata.SQLUser, Appdata.SQLPassword);

            //Check if rows selected
            int RowCount = dgView.SelectedRows.Count;

            if (RowCount == 0)
                return;

            for (int i = 0; i < RowCount; i++)
            {
                //Get row of clicked cell
                DataGridViewRow row = dgView.Rows[dgView.SelectedRows[i].Index];
                //Get column AcquisitionNumber
                DataGridViewCell cell = row.Cells["LandmarkID"];
                //create AcquisitionObject to be deleted

                Landmark SelectedLandmark = new Landmark(SQLDB, Subject, goemFiles.GeometryNumber, Convert.ToInt32(cell.Value), Appdata);
                SelectedLandmark.bodyProp = bodyProp;
                double[] p = new double[] { SelectedLandmark.coorX, SelectedLandmark.coorY, SelectedLandmark.coorZ };
                MakeLMactor(p, SelectedLandmark, bodyProp);
                LoadedLandmarks.Add(SelectedLandmark);
            }
        }

        private void dataGridViewLM_SelectionChanged(object sender, EventArgs e)
        {
            if (LoadedLandmarks.Count == 0)
            { return; }
            UpdateSelectedLMlist(dataGridViewLMchild, osimBodyProp._OsimGeometryPropertyList[0], ChildLMlist);
            UpdateLMvisualization();
        }

        private void dataGridViewLMparent_SelectionChanged(object sender, EventArgs e)
        {
            if (LoadedLandmarks.Count == 0)
            { return; }
            UpdateSelectedLMlist(dataGridViewLMparent, parentBodyProp._OsimGeometryPropertyList[0], ParentLMlist);
            UpdateLMvisualization();
        }


        private void UpdateSelectedLMlist(DataGridView dgView, OsimGeometryProperty geometry, List<Landmark> LMlist)
        {
            LMlist.Clear();

            //Set Database
            SQLDB = new DataBase(Appdata.SQLServer, Appdata.SQLDatabase, Appdata.SQLAuthSQL, Appdata.SQLUser, Appdata.SQLPassword);

            //Check if rows selected
            int RowCount = dgView.SelectedRows.Count;

            if (RowCount == 0)
                return;

            List<double[]> avg = new List<double[]>();

            for (int i = 0; i < RowCount; i++)
            {
                //Get row of clicked cell
                DataGridViewRow row = dgView.Rows[dgView.SelectedRows[i].Index];
                //Get column AcquisitionNumber
                DataGridViewCell cell = row.Cells["LandmarkID"];
                //create AcquisitionObject to be deleted


                Landmark SelectedLandmark = getSpecifiedLandmarkProperty(Convert.ToInt32(cell.Value));
                if (SelectedLandmark == null)
                { return; }
                else
                {
                    LMlist.Add(SelectedLandmark);
                }
            }
        }

        public Landmark getSpecifiedLandmarkProperty(int LMnumber)
        {
            int index = LoadedLandmarks.FindIndex(x => x.LandmarkID == LMnumber);
            if (index == -1)
            { return null; }
            else
            {
                return LoadedLandmarks[index];
            }
        }


        private void UpdateLMvisualization()
        {

            foreach (Landmark lm in LoadedLandmarks)
            {
                lm.sphereActor.GetProperty().SetOpacity(0.4);
                lm.sphereActor.SetScale(1);
            }
            foreach (Landmark lm in ChildLMlist)
            {
                lm.sphereActor.GetProperty().SetOpacity(1);
                lm.sphereActor.SetScale(2);
            }
            foreach (Landmark lm in ParentLMlist)
            {
                lm.sphereActor.GetProperty().SetOpacity(1);
                lm.sphereActor.SetScale(2);
            }
            RenderWindow.Render();
        }

        private void MakeLMactor(double[] p, Landmark SelectedLandmark, OsimBodyProperty bodyProp)
        {
            double R = SelectedLandmark.R;
            double G = SelectedLandmark.G;
            double B = SelectedLandmark.B;

            vtkSphereSource sphere = new vtkSphereSource();
            sphere.SetRadius(0.00050);

            vtkPolyDataMapper sphereMapper = vtkPolyDataMapper.New();
            sphereMapper.SetInputConnection(sphere.GetOutputPort());

            SelectedLandmark.sphereActor.SetMapper(sphereMapper);
            SelectedLandmark.sphereActor.GetProperty().SetColor(R, G, B);

            SelectedLandmark.transform.Translate(p[0], p[1], p[2]);
            SelectedLandmark.transform.PreMultiply();
            SelectedLandmark.transform.SetInput(bodyProp.transform);

            SelectedLandmark.sphereActor.SetUserTransform(SelectedLandmark.transform);
            double[] pos = SelectedLandmark.transform.GetPosition();
            SelectedLandmark.AbsX = pos[0];
            SelectedLandmark.AbsY = pos[1];
            SelectedLandmark.AbsZ = pos[2];
            SelectedLandmark.Save();
            ren1.AddActor(SelectedLandmark.sphereActor);

            ren1.GetRenderWindow().Render();

        }

        private void UpdateLoadedLandmarkTransforms()
        {
            foreach (Landmark LM in LoadedLandmarks)
            {

                LM.transform = new vtkTransform();
                LM.transform.Translate(LM.coorX, LM.coorY, LM.coorZ);
                LM.transform.PreMultiply();
                LM.transform.SetInput(LM.bodyProp.transform);
                LM.sphereActor.SetUserTransform(LM.transform);
            }
            RenderWindow.Render();
        }

        public void refreshLandmarks(Acquisitions.GeometryFiles goemFiles, DataGridView dgView)
        {
            //Clear DataGrid
            dgView.DataSource = null;
            dgView.Refresh();

            //Build SQL Command (=SQL select statement + Parameters)
            SqlCommand SQLcmd = new SqlCommand();

            string SQLselect;


            SQLselect = "SELECT LandmarkID, LandmarkName, LMoptionID, UserName FROM GeometryLandmarks where GeometryID = @GeometryID";
            SQLcmd.Parameters.AddWithValue("@GeometryID", goemFiles.GeometryNumber);

            if (rbOnlyMine.Checked)
            {
                SQLselect += "  AND (UserName = @UserName OR Username =@UserName2)";
                SQLcmd.Parameters.AddWithValue("@UserName", Appdata.globalUser._UserID);
                SQLcmd.Parameters.AddWithValue("@UserName2", Appdata.globalUser.FirstName + Appdata.globalUser.LastName); //this is for compatibility with the old way of def usernames.

            }

            SQLcmd.CommandText = SQLselect;

            //Set Database
            DataBase SQLDB = new DataBase(Appdata.SQLServer, Appdata.SQLDatabase, Appdata.SQLAuthSQL, Appdata.SQLUser, Appdata.SQLPassword);

            //Fill a dataset with selected records
            DataSet ds = new DataSet();
            SQLDB.ReadDataSet(SQLcmd, ref ds);

            // you can make the grid readonly.
            dgView.ReadOnly = true;
            dgView.DataSource = ds.Tables[0];
            // Resize the DataGridView columns to fit the newly loaded content.
            dgView.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.ColumnHeader);

        }

        private void DrawLineBetween2Meas(double[] meas1, double[] meas2)
        {
            vtkLineSource vtkLineSource1 = new vtkLineSource();
            vtkLineSource1.SetPoint1(meas1[0], meas1[1], meas1[2]);
            vtkLineSource1.SetPoint2(meas2[0], meas2[1], meas2[2]);

            vtkTubeFilter vtkTubeFilter1 = new vtkTubeFilter();
            vtkTubeFilter1.SetInput(vtkLineSource1.GetOutput());
            //vtkTubeFilter1.UseDefaultNormalOff();
            vtkTubeFilter1.SetRadius(0.0009);
            vtkTubeFilter1.SetNumberOfSides(25);

            vtkPolyDataMapper vtkPolyDataMapper1 = vtkPolyDataMapper.New();
            vtkPolyDataMapper1.SetInput(vtkTubeFilter1.GetOutput());
            //vtkPolyDataMapper1.SetInput(vtkLineSource.GetOutput());
            vtkActor line4 = new vtkActor();
            line4.SetMapper(vtkPolyDataMapper1);
            line4.GetProperty().SetDiffuseColor(0.2, 0.7, 0);
            ren1.AddActor(line4);

            //ren1.Render();
            ren1.GetRenderWindow().Render();


        }

        #endregion


        #region Landmark Calculations

        private void CalculatePointAverage(string location, OsimBodyProperty bodyProp)
        {
            //Set Database
            SQLDB = new DataBase(Appdata.SQLServer, Appdata.SQLDatabase, Appdata.SQLAuthSQL, Appdata.SQLUser, Appdata.SQLPassword);

            //Check if rows selected
            int RowCount = dataGridViewLMparent.SelectedRows.Count; // (DataGridViewElementStates.Selected);

            if (RowCount == 0)
                return;

            List<double[]> avg = new List<double[]>();

            for (int i = 0; i < RowCount; i++)
            {
                //Get row of clicked cell
                DataGridViewRow row = dataGridViewLMparent.Rows[dataGridViewLMparent.SelectedRows[i].Index];
                //Get column AcquisitionNumber
                DataGridViewCell cell = row.Cells["LandmarkID"];
                //create AcquisitionObject to be deleted

                Landmark SelectedLandmark = new Landmark(SQLDB, Subject, parentBodyProp._OsimGeometryPropertyList[0].geometryFileObject.GeometryNumber, Convert.ToInt32(cell.Value), Appdata);

                //double[] p = new double[] { SelectedLandmark.coorX, SelectedLandmark.coorY, SelectedLandmark.coorZ };

                double[] p = new double[] { SelectedLandmark.AbsX, SelectedLandmark.AbsY, SelectedLandmark.AbsZ };
                avg.Add(p);

            }


            //Check if rows selected
            int RowCount2 = dataGridViewLMchild.SelectedRows.Count; // (DataGridViewElementStates.Selected);

            if (RowCount2 == 0)
                return;

            for (int i = 0; i < RowCount2; i++)
            {
                //Get row of clicked cell
                DataGridViewRow row = dataGridViewLMchild.Rows[dataGridViewLMchild.SelectedRows[i].Index];
                //Get column AcquisitionNumber
                DataGridViewCell cell = row.Cells["LandmarkID"];
                //create AcquisitionObject to be deleted

                Landmark SelectedLandmark = new Landmark(SQLDB, Subject, osimBodyProp._OsimGeometryPropertyList[0].geometryFileObject.GeometryNumber, Convert.ToInt32(cell.Value), Appdata);

                //double[] p = new double[] { SelectedLandmark.coorX, SelectedLandmark.coorY, SelectedLandmark.coorZ };

                double[] p = new double[] { SelectedLandmark.AbsX, SelectedLandmark.AbsY, SelectedLandmark.AbsZ };

                avg.Add(p);
            }

            double X = 0;
            double Y = 0;
            double Z = 0;


            foreach (double[] value in avg)
            {
                X += value[0];
                Y += value[1];
                Z += value[2];


            }

            X = X / avg.Count();
            Y = Y / avg.Count();
            Z = Z / avg.Count();











            // vtkSTLReader polyDataReader = new vtkSTLReader();
            // polyDataReader.SetFileName(location);
            // polyDataReader.Update();
            // vtkTransformFilter vtkTransformFilterOrignal = new vtkTransformFilter();
            // vtkTransformFilterOrignal.SetInputConnection(polyDataReader.GetOutputPort());
            // //bodyProp.AddPolyDataToListDeformed(polyDataReader.GetOutput());
            // //bodyProp.AddPolyDataToListOriginals(polyDataReader.GetOutput());
            //vtkPolyData _vtkPolyData = polyDataReader.GetOutput();
            // _vtkPolyData.Update();

            // double X = 0;
            // double Y= 0;
            // double Z= 0;

            // for (int i =1; i < _vtkPolyData.GetPoints().GetNumberOfPoints(); i++)
            // {
            //    double[] pos=  _vtkPolyData.GetPoints().GetPoint(0);
            //     X += pos[0];
            //     Y += pos[1];
            //     Z += pos[2];


            // }
            // X = X / _vtkPolyData.GetPoints().GetNumberOfPoints();
            // Y = Y / _vtkPolyData.GetPoints().GetNumberOfPoints();
            // Z = Z / _vtkPolyData.GetPoints().GetNumberOfPoints();



            double[] loc = new double[] { X, Y, Z };
            newJointCOM_loc = loc;
            MakeCOMactor(loc);

            PlaceJointInCOM();

        }

        private vtkTransform COMtransf = new vtkTransform();

        private void MakeCOMactor(double[] loc)
        {

            vtkSphereSource sphere = new vtkSphereSource();
            sphere.SetRadius(0.00150);
            sphere.SetPhiResolution(100);
            sphere.SetThetaResolution(100);


            vtkPolyDataMapper sphereMapper = vtkPolyDataMapper.New();
            sphereMapper.SetInputConnection(sphere.GetOutputPort());


            COMactor.SetMapper(sphereMapper);
            COMactor.GetProperty().SetColor(0.2, 0.2, 0.4);

            COMtransf.RotateY(90);
            COMtransf.PostMultiply();
            COMtransf.Translate(loc[0], loc[1], loc[2]);


            COMactor.SetUserTransform(COMtransf);
            ren1.AddActor(COMactor);

            ren1.GetRenderWindow().Render();


        }


        private void CalcPostAnt(bool principal, string Endplate)
        {
            DataGridView dataGridView = new DataGridView();
            int indexLMoption1 = 0;
            int indexLMoption2 = 0;

            if (Endplate == "Inf")
            {
                dataGridView = dgViewSelectedLMParent;
                indexLMoption1 = 10;
                indexLMoption2 = 13;

            }
            if (Endplate == "Sup")
            {
                dataGridView = dgViewSelectedLMChild;
                indexLMoption1 = 1;
                indexLMoption2 = 4;

            }
            List<double[]> avg = new List<double[]>();

            //Check if rows selected
            int RowCount2 = dataGridView.Rows.Count; // (DataGridViewElementStates.Selected);

            if (RowCount2 == 0)
            {

                MessageBox.Show("Could not define the reference because not all required landmarks were selected in the first step of this protocol.", "error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            for (int i = 0; i < dataGridView.Rows.Count; i++)
            {
                //Get row of clicked cell
                DataGridViewRow row = dataGridView.Rows[i];
                //Get column AcquisitionNumber
                DataGridViewCell cell = row.Cells["LandmarkID"];
                //create AcquisitionObject to be deleted

                Landmark SelectedLandmark = new Landmark(SQLDB, Subject, osimBodyProp._OsimGeometryPropertyList[0].geometryFileObject.GeometryNumber, Convert.ToInt32(cell.Value), Appdata);

                //double[] p = new double[] { SelectedLandmark.coorX, SelectedLandmark.coorY, SelectedLandmark.coorZ };

                double[] p = new double[] { 0, 0, 0 };
                if (SelectedLandmark.LMoptionID == indexLMoption1 || SelectedLandmark.LMoptionID == indexLMoption2)
                {
                    p = new double[] { SelectedLandmark.AbsX, SelectedLandmark.AbsY, SelectedLandmark.AbsZ };
                    avg.Add(p);
                }

               

                
            }

            if (avg.Count != 2)
            {

                MessageBox.Show("Could not define the reference because not all required landmarks were selected (or multiple landmarks with the same name were selected) in the first step of this protocol. Do not rely on the placement of the reference frame (it was placed in neutral position)!", "error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }


            //DrawLineBetween2Meas(avg[0], avg[1]);

            double deltaX = avg[0][0] - avg[1][0];
            double deltaY = avg[0][1] - avg[1][1];
            double deltaZ = avg[0][2] - avg[1][2];

            double angleX = Math.Atan(deltaY / deltaX) * 180 / Math.PI;
            double angleZ = Math.Atan(deltaY / deltaZ) * 180 / Math.PI;
            double angleY = Math.Atan(deltaX / deltaZ) * 180 / Math.PI;


          
            vtkTransform t = (vtkTransform)endplateAxesActor.GetUserTransform();
            double[] position = t.GetPosition();

            double[] orientation = t.GetOrientation();

            vtkTransform s = new vtkTransform();
            if (principal)
            {
                s.RotateY(angleY + 90);
                s.RotateZ(-angleZ);
                s.RotateX(orientation[0]);
              
               
                
            }
            else
            {

                s.RotateZ(-angleZ);
                

            }
                s.PostMultiply();
            s.Translate(position[0], position[1], position[2]);

            
            endplateAxesActor.SetUserTransform(s);

            RenderWindow.Render();
            //vtkBoxWidget2.SetTransform(s);
            //double[] orientation = ((vtkTransform)osimBodyProp.osimJointProperty._jointActor.GetUserTransform()).GetOrientation();
            //((vtkTransform)osimBodyProp.osimJointProperty._jointActor.GetUserTransform()).RotateX(-orientation[0]);
            //((vtkTransform)osimBodyProp.osimJointProperty._jointActor.GetUserTransform()).RotateY(-orientation[1]);
            //((vtkTransform)osimBodyProp.osimJointProperty._jointActor.GetUserTransform()).RotateZ(-orientation[2]);

            //RenderWindow.Render();
            //((vtkTransform)osimBodyProp.osimJointProperty._jointActor.GetUserTransform()).RotateY(angleY); // - orientation[1]);

        }

        private void calcLeftRight(bool principal, string Endplate)
        {
            DataGridView dataGridView = new DataGridView();
            int indexLMoption1 = 0;
            int indexLMoption2 = 0;

            if (Endplate == "Inf")
            {
                dataGridView = dgViewSelectedLMParent;
                indexLMoption1 = 16;
                indexLMoption2 = 17;

            }
            if (Endplate == "Sup")
            {
                dataGridView = dgViewSelectedLMChild;
                indexLMoption1 = 7;
                indexLMoption2 = 8;

            }

            List<double[]> avg = new List<double[]>();

            //Check if rows selected
            int RowCount2 = dataGridView.Rows.Count; // (DataGridViewElementStates.Selected);

            if (RowCount2 == 0)
            {

                MessageBox.Show("Could not define the reference because not all required landmarks were selected in the first step of this protocol.", "error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            for (int i = 0; i < dataGridView.Rows.Count; i++)
            {
                //Get row of clicked cell
                DataGridViewRow row = dataGridView.Rows[i]; 
                //Get column AcquisitionNumber
                DataGridViewCell cell = row.Cells["LandmarkID"];
                //create AcquisitionObject to be deleted

                Landmark SelectedLandmark = new Landmark(SQLDB, Subject, osimBodyProp._OsimGeometryPropertyList[0].geometryFileObject.GeometryNumber, Convert.ToInt32(cell.Value), Appdata);

                //double[] p = new double[] { SelectedLandmark.coorX, SelectedLandmark.coorY, SelectedLandmark.coorZ };
                double[] p = new double[] { 0,0,0 };
                if (SelectedLandmark.LMoptionID == indexLMoption1 || SelectedLandmark.LMoptionID == indexLMoption2)
                {
                    p = new double[] { SelectedLandmark.AbsX, SelectedLandmark.AbsY, SelectedLandmark.AbsZ };
                    avg.Add(p);
                }

               
            }

            if (avg.Count != 2)
            {

                MessageBox.Show("Could not define the reference because not all required landmarks were selected (or multiple landmarks with the same name were selected) in the first step of this protocol. Do not rely on the placement of the reference frame (it was placed in neutral position)!", "error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            //DrawLineBetween2Meas(avg[0], avg[1]);

            double deltaX = avg[0][0] - avg[1][0];
            double deltaY = avg[0][1] - avg[1][1];
            double deltaZ = avg[0][2] - avg[1][2];

            double angleX = Math.Atan(deltaY / deltaX) * 180 / Math.PI;
            double angleZ = Math.Atan(deltaY / deltaZ) * 180 / Math.PI;
            double angleY = Math.Atan(deltaX / deltaZ) * 180 / Math.PI;

            //double[] orientation = ((vtkTransform)osimBodyProp.osimJointProperty._jointActor.GetUserTransform()).GetOrientation();
            //((vtkTransform)osimBodyProp.osimJointProperty._jointActor.GetUserTransform()).RotateX(-orientation[0]);
            //((vtkTransform)osimBodyProp.osimJointProperty._jointActor.GetUserTransform()).RotateY(-orientation[1]);
            //((vtkTransform)osimBodyProp.osimJointProperty._jointActor.GetUserTransform()).RotateZ(-orientation[2]);

            RenderWindow.Render();

            vtkTransform t = (vtkTransform)endplateAxesActor.GetUserTransform();
            double[] position = t.GetPosition();
            double[] orientation = t.GetOrientation();
            double[] orXYZ = t.GetOrientationWXYZ();

            vtkTransform s = new vtkTransform();
            s.PostMultiply();
            if (principal)
            {
                s.RotateY(angleY);
                s.RotateZ(orientation[2]);
                s.RotateX(-angleX);
                
            }
            else
            {
                //s.RotateWXYZ(orXYZ[0], orXYZ[1], orXYZ[2], orXYZ[3]);
                ////s.RotateZ(orientation[2]);
                ////s.RotateY(orientation[1]);
                
                //s.RotateX(-orientation[0]);
                s.RotateX(-angleX);
            }
            s.PostMultiply();
            s.Translate(position[0], position[1], position[2]);

            endplateAxesActor.SetUserTransform(s);
            //RenderWindow.Render();
            //((vtkTransform)osimBodyProp.osimJointProperty._jointActor.GetUserTransform()).RotateX(angleX); // - orientation[1]);


        }
        private void btnCalcPostAnt_Click(object sender, EventArgs e)
        {
            List<double[]> avg = new List<double[]>();



            //Check if rows selected
            int RowCount2 = dataGridViewLMparent.SelectedRows.Count; // (DataGridViewElementStates.Selected);

            if (RowCount2 == 0)
                return;

            for (int i = 0; i < RowCount2; i++)
            {
                //Get row of clicked cell
                DataGridViewRow row = dataGridViewLMparent.Rows[dataGridViewLMparent.SelectedRows[i].Index];
                //Get column AcquisitionNumber
                DataGridViewCell cell = row.Cells["LandmarkID"];
                //create AcquisitionObject to be deleted

                Landmark SelectedLandmark = new Landmark(SQLDB, Subject, osimBodyProp._OsimGeometryPropertyList[0].geometryFileObject.GeometryNumber, Convert.ToInt32(cell.Value), Appdata);

                //double[] p = new double[] { SelectedLandmark.coorX, SelectedLandmark.coorY, SelectedLandmark.coorZ };

                double[] p = new double[] { SelectedLandmark.AbsX, SelectedLandmark.AbsY, SelectedLandmark.AbsZ };

                avg.Add(p);
            }
            DrawLineBetween2Meas(avg[0], avg[1]);

            double deltaX = avg[0][0] - avg[1][0];
            double deltaY = avg[0][1] - avg[1][1];
            double deltaZ = avg[0][2] - avg[1][2];

            double angleX = Math.Atan(deltaY / deltaX) * 180 / Math.PI;
            double angleZ = Math.Atan(deltaY / deltaZ) * 180 / Math.PI;
            double angleY = Math.Atan(deltaX / deltaZ) * 180 / Math.PI;

            double[] orientation = ((vtkTransform)osimBodyProp.osimJointProperty._jointActor.GetUserTransform()).GetOrientation();


            //((vtkTransform)osimBodyProp.osimJointProperty._jointActor.GetUserTransform()).RotateX(-orientation[0]);
            ((vtkTransform)osimBodyProp.osimJointProperty._jointActor.GetUserTransform()).RotateY(-orientation[1]);
            //((vtkTransform)osimBodyProp.osimJointProperty._jointActor.GetUserTransform()).RotateZ(-orientation[2]);




            //((vtkTransform)osimBodyProp.osimJointProperty._jointActor.GetUserTransform()).RotateZ(angleZ); //- orientation[2]);
            ((vtkTransform)osimBodyProp.osimJointProperty._jointActor.GetUserTransform()).RotateY(angleY); // - orientation[1]);
            //((vtkTransform)osimBodyProp.osimJointProperty._jointActor.GetUserTransform()).RotateX(angleX); // - orientation[0]);
            ((vtkTransform)osimBodyProp.osimJointProperty._jointActor.GetUserTransform()).Update();




            osimBodyProp.osimJointProperty.axesActor.SetUserTransform((vtkTransform)osimBodyProp.osimJointProperty._jointActor.GetUserTransform());

            //SimModelVisualization.SetJointPlacement(osimBodyProp.osimJointProperty.joint, osimBodyProp.transform, (vtkTransform)osimBodyProp.osimJointProperty._jointActor.GetUserTransform(), parentBodyProp.transform);

            //osimBodyProp.osimJointProperty.ReadJoint();
            //osimBodyProp.osimJointProperty.SetTransformation();

            ren1.GetRenderWindow().Render();


        }


        #endregion


        #region Mesh Stuff

        private vtkActor IVDactor = new vtkActor();

        private vtkPolyData CloseMesh(vtkPolyData inputPolydata)
        {

            vtkDelaunay3D delny = new vtkDelaunay3D();
            delny.SetInput(inputPolydata);
            delny.SetTolerance(0.01);
            delny.SetAlpha(0.2);
            delny.BoundingTriangulationOff();

            vtkShrinkFilter shrink = new vtkShrinkFilter();
            shrink.SetInputConnection(delny.GetOutputPort());
            shrink.SetShrinkFactor(1);

            vtkDataSetMapper map = new vtkDataSetMapper();
            map.SetInputConnection(shrink.GetOutputPort());



            vtkDataSetSurfaceFilter surface_filter = new vtkDataSetSurfaceFilter();
            surface_filter.SetInputConnection(shrink.GetOutputPort());
            surface_filter.Update();

            vtkTriangleFilter triangle_filter = new vtkTriangleFilter();
            triangle_filter.SetInputConnection(surface_filter.GetOutputPort());
            triangle_filter.Update();

            return triangle_filter.GetOutput();

        }

        private void MakeMeshFromLMs()
        {

            vtkPolyData IVD = new vtkPolyData();
            vtkPoints vtkPoints = new vtkPoints();



            //Set Database
            SQLDB = new DataBase(Appdata.SQLServer, Appdata.SQLDatabase, Appdata.SQLAuthSQL, Appdata.SQLUser, Appdata.SQLPassword);

            int RowCount = dgViewSelectedLMParent.Rows.Count;

            if (RowCount == 0)
                return;

            for (int i = 0; i < RowCount; i++)
            {
                //Get row of clicked cell
                DataGridViewRow row = dgViewSelectedLMParent.Rows[dgViewSelectedLMParent.Rows[i].Index];
                //Get column AcquisitionNumber
                DataGridViewCell cell = row.Cells["LandmarkID"];
                //create AcquisitionObject to be deleted

                Landmark SelectedLandmark = new Landmark(SQLDB, Subject, parentBodyProp._OsimGeometryPropertyList[0].geometryFileObject.GeometryNumber, Convert.ToInt32(cell.Value), Appdata);

                //double[] p = new double[] { SelectedLandmark.coorX, SelectedLandmark.coorY, SelectedLandmark.coorZ };

                double[] p = new double[] { SelectedLandmark.AbsX, SelectedLandmark.AbsY, SelectedLandmark.AbsZ };
                vtkPoints.InsertNextPoint(p[0], p[1], p[2]);


            }



            int RowCount2 = dgViewSelectedLMChild.Rows.Count;

            if (RowCount2 == 0)
                return;

            for (int i = 0; i < RowCount2; i++)
            {
                //Get row of clicked cell
                DataGridViewRow row = dgViewSelectedLMChild.Rows[dgViewSelectedLMChild.Rows[i].Index];
                //Get column AcquisitionNumber
                DataGridViewCell cell = row.Cells["LandmarkID"];
                //create AcquisitionObject to be deleted

                Landmark SelectedLandmark = new Landmark(SQLDB, Subject, osimBodyProp._OsimGeometryPropertyList[0].geometryFileObject.GeometryNumber, Convert.ToInt32(cell.Value), Appdata);

                //double[] p = new double[] { SelectedLandmark.coorX, SelectedLandmark.coorY, SelectedLandmark.coorZ };

                double[] p = new double[] { SelectedLandmark.AbsX, SelectedLandmark.AbsY, SelectedLandmark.AbsZ };
                vtkPoints.InsertNextPoint(p[0], p[1], p[2]);



            }



            vtkPoints.GetNumberOfPoints();
            vtkPolyData pointSource = new vtkPolyData();

            //vtkProgrammableSource pointSource = new vtkProgrammableSource();
            //pointSource.GetPolyDataOutput().SetPoints(vtkPoints);
            pointSource.SetPoints(vtkPoints);
            //pointSource.Update();

            vtkDelaunay3D delny = new vtkDelaunay3D();
            delny.SetInput(pointSource);
            delny.SetTolerance(0.01);
            delny.SetAlpha(0.2);
            delny.BoundingTriangulationOff();

            vtkShrinkFilter shrink = new vtkShrinkFilter();
            shrink.SetInputConnection(delny.GetOutputPort());
            shrink.SetShrinkFactor(1);



            //vtkSurfaceReconstructionFilter surf = new vtkSurfaceReconstructionFilter();
            //surf.SetInput(pointSource);

            //vtkContourFilter cf = new vtkContourFilter();
            //cf.SetInputConnection(surf.GetOutputPort());
            //cf.SetValue(0, 0.0);
            ////     cf.Update();


            ////for (int i = 0; i < 2; i++)
            ////{
            ////    vtkTriangle vtkTriangle = new vtkTriangle();
            ////    vtkTriangle.GetPointIds().SetId(i, i);
            ////    vtkTriangle.GetPointIds().SetId(i+1, i+1);
            ////    vtkTriangle.GetPointIds().SetId(i+2, i+2);
            ////    i = i * 3;

            ////    triangles.InsertNextCell(vtkTriangle);
            ////}

            ////IVD.SetPoints(vtkPoints);
            ////// IVD.SetPolys(triangles);
            ////IVD.SetLines(Lines);



            //vtkReverseSense reverse = new vtkReverseSense();
            //reverse.SetInputConnection(cf.GetOutputPort());
            //reverse.ReverseCellsOn();
            //reverse.ReverseNormalsOn();




            vtkDataSetMapper map = new vtkDataSetMapper();
            map.SetInputConnection(shrink.GetOutputPort());



            vtkDataSetSurfaceFilter surface_filter = new vtkDataSetSurfaceFilter();
            surface_filter.SetInputConnection(shrink.GetOutputPort());
            surface_filter.Update();

            vtkTriangleFilter triangle_filter = new vtkTriangleFilter();
            triangle_filter.SetInputConnection(surface_filter.GetOutputPort());
            triangle_filter.Update();

            //vtkPolyDataWriter writer = new vtkPolyDataWriter();
            vtkSTLWriter writer = new vtkSTLWriter();
            writer.SetFileName(Appdata.TempDir + "\\" + "tempLandmarkFit.stl");
            writer.SetInputConnection(triangle_filter.GetOutputPort());
            //writer.SetDataModeToAscii()
            writer.Write();


            //vtkPolyDataMapper mapper = vtkPolyDataMapper.New();
            //mapper.SetInputConnection(delny.GetOutputPort());
            //mapper.SetScalarVisibility(0);


            IVDactor.SetMapper(map);
            //IVDactor.SetUserTransform(parentBodyProp.transform);

            ren1.AddActor(IVDactor);

            ren1.GetRenderWindow().Render();
        }

        private void COMmeshCalculator(DMesh3 mesh, OsimBodyProperty bodyProp)
        {
            double height = mesh.GetBounds().Height;
            double mass;
            double[,] inertia3x3;
            Vector3d vector = MeshMeasurements.Centroid(mesh);
            Vector3d vector2;
            double x = vector[0] / 1000000;
            double y = vector[1] / 1000000;
            double z = vector[2] / 1000000;



            MeshMeasurements.MassProperties(mesh, out mass, out vector2, out inertia3x3);

            x = vector2[0] / 1000000;
            y = vector2[1] / 1000000;
            z = vector2[2] / 1000000;

            vtkTransform vtkTransform = new vtkTransform();



            //vtkSphereSource sphere = new vtkSphereSource();
            //sphere.SetRadius(0.0030);
            //sphere.SetPhiResolution(100);
            //sphere.SetThetaResolution(100);


            //vtkPolyDataMapper sphereMapper = vtkPolyDataMapper.New();
            //sphereMapper.SetInputConnection(sphere.GetOutputPort());

            //vtkActor actor = new vtkActor();
            //actor.SetMapper(sphereMapper);
            //actor.GetProperty().SetColor(0, 1, 0);

            //vtkTransform transf = new vtkTransform();
            //transf.Translate(x, y, z);
            //transf.PreMultiply();
            //transf.SetInput(bodyProp.transform);

            //actor.SetUserTransform(transf);
            //ren1.AddActor(actor);

            //ren1.GetRenderWindow().Render();

        }


        #endregion


        private void btnSetEndplateRefPlane_Click_1(object sender, EventArgs e)
        {
           
        }

        private void btnDoneEndplateRef_Click(object sender, EventArgs e)
        {
            
        }

        public void ExecuteHandleEndplate(vtkObject sender, vtkObjectEventArgs e)
        {
            vtkTransform t = new vtkTransform();
            vtkBoxWidget2.GetTransform(t);
            //double[] translation = t.GetPosition();
            double[] rotation = t.GetOrientation();

            vtkTransform s = new vtkTransform();
            s = (vtkTransform)COMendplateActor.GetUserTransform();
            double[] translation = s.GetPosition();
            //double[] rotation = t.GetOrientation();


            vtkTransform d = new vtkTransform();
            d.PostMultiply();
            double[] wxyz = t.GetOrientationWXYZ();
            d.RotateWXYZ(wxyz[0], wxyz[1], wxyz[2], wxyz[3]);
            //d.RotateX(rotation[0]);
            //d.RotateY(rotation[1]);
            //d.RotateZ(rotation[2]);

            d.Translate(translation[0], translation[1], translation[2]);


            // d.SetInput(axesActor.GetUserTransform());

          

            endplateAxesActor.SetUserTransform(d);
            endplateAxesActor.GetUserTransform().Update();

            UpdateTextTransforms();


            RenderWindow.Render();

        }

        private vtkTransform endplateAxesTransform = new vtkTransform();
        private vtkActor endplateAxesActor = new vtkActor();
        private vtkTransform vtkTransformAnterior = new vtkTransform();
        private vtkFollower followerAnterior = vtkFollower.New();
        private vtkTransform vtkTransformRight = new vtkTransform();
        private  vtkFollower followerRight = vtkFollower.New();
        private vtkFollower followerSup = vtkFollower.New();
        private vtkTransform vtkTransformSup = new vtkTransform();

        public void MakeEndplateAxes()
        {
          

            vtkLineSource vtkLineSource1 = new vtkLineSource();

            vtkLineSource1.SetPoint1(0, 0, 0);
            vtkLineSource1.SetPoint2(0.04, 0, 0);

            vtkTubeFilter vtkTubeFilter1 = new vtkTubeFilter();
            vtkTubeFilter1.SetInput(vtkLineSource1.GetOutput());
            vtkTubeFilter1.UseDefaultNormalOff();

            vtkTubeFilter1.SetRadius(0.0008);
            vtkTubeFilter1.SetNumberOfSides(8);






            vtkLineSource vtkLineSource1neg = new vtkLineSource();
            vtkLineSource1neg.SetPoint1(-0.04, 0, 0);
            vtkLineSource1neg.SetPoint2(0, 0, 0);

            vtkTubeFilter vtkTubeFilter1neg = new vtkTubeFilter();
            vtkTubeFilter1neg.SetInput(vtkLineSource1neg.GetOutput());
            vtkTubeFilter1neg.UseDefaultNormalOff();

            vtkTubeFilter1neg.SetRadius(0.0003);
            vtkTubeFilter1neg.SetNumberOfSides(8);


            vtkLineSource vtkLineSource2 = new vtkLineSource();

            vtkLineSource2.SetPoint1(0, 0.03, 0);
            vtkLineSource2.SetPoint2(0, 0, 0);

            vtkTubeFilter vtkTubeFilter2 = new vtkTubeFilter();
            vtkTubeFilter2.SetInput(vtkLineSource2.GetOutput());
            vtkTubeFilter2.UseDefaultNormalOff();

            vtkTubeFilter2.SetRadius(0.0008);
            vtkTubeFilter2.SetNumberOfSides(8);


            vtkLineSource vtkLineSource2neg = new vtkLineSource();
            vtkLineSource2neg.SetPoint1(-0, -0.02, 0);
            vtkLineSource2neg.SetPoint2(0, 0, 0);

            vtkTubeFilter vtkTubeFilter2neg = new vtkTubeFilter();
            vtkTubeFilter2neg.SetInput(vtkLineSource2neg.GetOutput());
            vtkTubeFilter2neg.UseDefaultNormalOff();

            vtkTubeFilter2neg.SetRadius(0.0003);
            vtkTubeFilter2neg.SetNumberOfSides(8);




            vtkLineSource vtkLineSource3 = new vtkLineSource();

            vtkLineSource3.SetPoint1(0, 0, 0);
            vtkLineSource3.SetPoint2(0, 0, 0.04);

            vtkTubeFilter vtkTubeFilter3 = new vtkTubeFilter();
            vtkTubeFilter3.SetInput(vtkLineSource3.GetOutput());
            vtkTubeFilter3.UseDefaultNormalOff();

            vtkTubeFilter3.SetRadius(0.0008);
            vtkTubeFilter3.SetNumberOfSides(8);



            vtkLineSource vtkLineSource3neg = new vtkLineSource();

            vtkLineSource3neg.SetPoint1(0, 0, 0);
            vtkLineSource3neg.SetPoint2(0, 0, -0.04);

            vtkTubeFilter vtkTubeFilter3neg = new vtkTubeFilter();
            vtkTubeFilter3neg.SetInput(vtkLineSource3neg.GetOutput());
            vtkTubeFilter3neg.UseDefaultNormalOff();

            vtkTubeFilter3neg.SetRadius(0.0003);
            vtkTubeFilter3neg.SetNumberOfSides(8);



            vtkAppendPolyData vtkAppendPolyData = vtkAppendPolyData.New();
            vtkAppendPolyData.AddInputConnection(vtkTubeFilter1.GetOutputPort());
            vtkAppendPolyData.AddInputConnection(vtkTubeFilter2.GetOutputPort());
            vtkAppendPolyData.AddInputConnection(vtkTubeFilter3.GetOutputPort());

            vtkAppendPolyData.AddInputConnection(vtkTubeFilter1neg.GetOutputPort());
            vtkAppendPolyData.AddInputConnection(vtkTubeFilter2neg.GetOutputPort());
            vtkAppendPolyData.AddInputConnection(vtkTubeFilter3neg.GetOutputPort());




            vtkPolyDataMapper vtkPolyDataMapper = vtkPolyDataMapper.New();
            //vtkPolyDataMapper.SetInput(vtkTubeFilter.GetOutput());
            vtkPolyDataMapper.SetInput(vtkAppendPolyData.GetOutput());



            vtkVectorText textLabelAnterior = new vtkVectorText();
            textLabelAnterior.SetText("Anterior");

            vtkPolyDataMapper textmapperAnterior = vtkPolyDataMapper.New();
            textmapperAnterior.SetInputConnection(textLabelAnterior.GetOutputPort());




            followerAnterior.SetMapper(textmapperAnterior);
            followerAnterior.GetProperty().SetColor(1, 0, 0); //red
            followerAnterior.SetScale(0.003);
            followerAnterior.SetUserTransform(vtkTransformAnterior);
            followerAnterior.SetCamera(ren1.GetActiveCamera());

           



            vtkVectorText textLabelRight = new vtkVectorText();
            textLabelRight.SetText("Right");

            vtkPolyDataMapper textmapperRight = vtkPolyDataMapper.New();
            textmapperRight.SetInputConnection(textLabelRight.GetOutputPort());


         
          


           
            followerRight.SetMapper(textmapperRight);
            followerRight.GetProperty().SetColor(0, 1, 0); //green
            followerRight.SetScale(0.003);
            followerRight.SetUserTransform(vtkTransformRight);
            followerRight.SetCamera(ren1.GetActiveCamera());

         


            vtkVectorText textLabelSup = new vtkVectorText();
            textLabelSup.SetText("Superior");

            vtkPolyDataMapper textmapperSup = vtkPolyDataMapper.New();
            textmapperSup.SetInputConnection(textLabelSup.GetOutputPort());
       
           


           
            followerSup.SetMapper(textmapperSup);
            followerSup.GetProperty().SetColor(0, 0, 1); //Blue
            followerSup.SetScale(0.003);
          
            followerSup.SetCamera(ren1.GetActiveCamera());


            endplateAxesActor.SetMapper(vtkPolyDataMapper);
            endplateAxesActor.GetProperty().SetDiffuseColor(0.98, 0.98, 0.98);

            
            
            endplateAxesActor.SetUserTransform(COMendplateActor.GetUserTransform());

            ren1.AddActor(endplateAxesActor);




            UpdateTextTransforms();


            ren1.AddActor(followerAnterior);
            ren1.AddActor(followerRight);
            ren1.AddActor(followerSup);




        }

        private void UpdateTextTransforms()
        {
            vtkTransformAnterior = new vtkTransform();
            vtkTransformAnterior.Translate(0.04, 0, 0);
            vtkTransformAnterior.PostMultiply();
            vtkTransformAnterior.SetInput(endplateAxesActor.GetUserTransform());
            followerAnterior.SetUserTransform(vtkTransformAnterior);


            vtkTransformRight = new vtkTransform();
            vtkTransformRight.Translate(0, 0, 0.04);
            vtkTransformRight.PostMultiply();
            vtkTransformRight.SetInput(endplateAxesActor.GetUserTransform());
            followerRight.SetUserTransform(vtkTransformRight);


            vtkTransformSup = new vtkTransform();
            vtkTransformSup.Translate(0, 0.04, 0);
            vtkTransformSup.PostMultiply();
            vtkTransformSup.SetInput(endplateAxesActor.GetUserTransform());
            followerSup.SetUserTransform(vtkTransformSup);
        }

       

        private void CalculateEndplateNormal(double[] loc)
        {
            vtkPolyDataNormals normals = new vtkPolyDataNormals();
            normals.SetInput(parentPolydata);
            normals.Update();

            vtkDataArray array = normals.GetOutput().GetPointData().GetNormals();
            array.GetNumberOfTuples();
            array.GetTuple(1);
            array.GetSize();

            double x = 0;
            double y = 0;
            double z = 0;

            for (long i = 0; i < array.GetNumberOfTuples(); i++)
            {
                double[] cN = { 0, 0, 0 };
                IntPtr intPtr = array.GetTuple(i);

                Marshal.Copy(intPtr, cN, 0, cN.Length);
                //Debug.Write(cN[0] + "  " + cN[1] + "   " + cN[2]);
                x += cN[0];
                y += cN[1];
                z += cN[2];
            }
            x = x / array.GetNumberOfTuples();
            y = y / array.GetNumberOfTuples();
            z = z / array.GetNumberOfTuples();

            Debug.Write("AVERAGE");
            Debug.Write(x + "  " + y + "   " + z);
            //vtkNormals- normals = PDnormals->GetPointData()->GetNormals();


            vtkLineSource vtkLineSource = new vtkLineSource();

            vtkLineSource.SetPoint1(0, 0, 0);
            vtkLineSource.SetPoint2(x, y, z);

            vtkTubeFilter vtkTubeFilter = new vtkTubeFilter();
            vtkTubeFilter.SetInput(vtkLineSource.GetOutput());
            vtkTubeFilter.UseDefaultNormalOff();

            vtkTubeFilter.SetRadius(0.0004);
            vtkTubeFilter.SetNumberOfSides(8);



            vtkAppendPolyData vtkAppendPolyData = vtkAppendPolyData.New();

            vtkAppendPolyData.AddInputConnection(vtkTubeFilter.GetOutputPort());


            vtkPolyDataMapper vtkPolyDataMapper = vtkPolyDataMapper.New();
            //vtkPolyDataMapper.SetInput(vtkTubeFilter.GetOutput());
            vtkPolyDataMapper.SetInput(vtkAppendPolyData.GetOutput());


           

            NormalActor.SetMapper(vtkPolyDataMapper);
            ren1.AddActor(NormalActor);

            vtkTransform transf = new vtkTransform();
            transf.Translate(loc[0], loc[1], loc[2]);

            NormalActor.SetUserTransform(transf);


        }

        private void calculateEndplateCOM(vtkPolyData vtkPolyData)
        {
            double X = 0;
            double Y = 0;
            double Z = 0;
            int nbPoints = Convert.ToInt32(vtkPolyData.GetPoints().GetNumberOfPoints());
            for (int i = 1; i < nbPoints; i++)
            {
                double[] pos = vtkPolyData.GetPoints().GetPoint(i);
                X += pos[0];
                Y += pos[1];
                Z += pos[2];
            }
            X = X / nbPoints;
            Y = Y / nbPoints;
            Z = Z / nbPoints;

            double[] loc = new double[] { X, Y, Z };


            //vtkArrowSource arrowSource = new vtkArrowSource();

            //vtkGlyph3D glyph3D = vtkGlyph3D.New();
            //glyph3D.SetSourceConnection(arrowSource.GetOutputPort());
            //glyph3D.SetInput(parentPolydata);
            //glyph3D.SetVectorModeToUseNormal();
            //glyph3D.SetScaleFactor(0.002);
            //glyph3D.Update();

            //vtkPolyDataMapper mapper = vtkPolyDataMapper.New();
            //mapper.SetInputConnection(glyph3D.GetOutputPort());

            //vtkActor actor = new vtkActor();
            //actor.SetMapper(mapper);

            //ren1.AddActor(actor);

            RenderWindow.Render();


            //parentPolydata.GetCell(1).norma
            MakeCOMendplateActor(loc);
            CalculateEndplateNormal(loc);
        }

        private void MakeCOMendplateActor(double[] loc)
        {
            vtkSphereSource sphere = new vtkSphereSource();
            sphere.SetRadius(0.00150);
            sphere.SetPhiResolution(30);
            sphere.SetThetaResolution(30);

            vtkPolyDataMapper sphereMapper = vtkPolyDataMapper.New();
            sphereMapper.SetInputConnection(sphere.GetOutputPort());

            COMendplateActor.SetMapper(sphereMapper);
            COMendplateActor.GetProperty().SetColor(0.6, 0.4, 0.65);

            vtkTransform transf = new vtkTransform();
           // transf.RotateY(90);
            transf.PostMultiply();
            transf.Translate(loc[0], loc[1], loc[2]);
         
            COMendplateActor.SetUserTransform(transf);
            ren1.AddActor(COMendplateActor);

            ren1.GetRenderWindow().Render();
        }

        #region PlaneCutter Stuff

        vtkPolyData childPolydata = new vtkPolyData();
        vtkPolyData parentPolydata = new vtkPolyData();

        public vtkClipPolyData clipper = new vtkClipPolyData();
        public vtkPolyDataMapper clipMapper = vtkPolyDataMapper.New();
        public vtkActor clipActor = new vtkActor();

        public vtkActor clipActorChild = new vtkActor();
        public vtkActor clipActorParent = new vtkActor();
        public vtkCutter cutEdges = new vtkCutter();
        public vtkStripper cutStrips = new vtkStripper();
        public vtkPolyData cutPoly = new vtkPolyData();
        public vtkTriangleFilter cutTriangles = new vtkTriangleFilter();
        public vtkPolyDataMapper cutMapper = vtkPolyDataMapper.New();
        public vtkActor cutActorChild = new vtkActor();
        public vtkActor cutActorParent = new vtkActor();
        public vtkImplicitPlaneWidget planeWidget = new vtkImplicitPlaneWidget();
        public vtkPlane plane = new vtkPlane();
        vtkActor restActor = new vtkActor();

        private bool firstChildCutDone = false;
        private bool firstParentCutDone = false;

        public void ExecuteHandle2(vtkObject sender, vtkObjectEventArgs e)
        {
            planeWidget.GetPlane(plane);

            cut(0.0);
        }

        private void cut(double v)
        {
            clipper.SetValue(v);
            cutEdges.SetValue(0, v);
            cutStrips.Update();
            cutPoly.SetPoints(cutStrips.GetOutput().GetPoints());
            cutPoly.SetPolys(cutStrips.GetOutput().GetLines());
            cutMapper.Update();
            ren1.Render();

        }

        private void planecutter(vtkTransform OriginalTransform, vtkPolyData childPolydata, OsimBodyProperty osimBodyProp, vtkActor cutActor)
        {

            clipper = new vtkClipPolyData();
            clipMapper = vtkPolyDataMapper.New();
            ren1.RemoveActor(clipActor);
            clipActor = new vtkActor();
            cutEdges = new vtkCutter();
            cutStrips = new vtkStripper();
            cutPoly = new vtkPolyData();
            cutTriangles = new vtkTriangleFilter();
            cutMapper = vtkPolyDataMapper.New();
            ren1.RemoveActor(cutActor);
            cutActor = new vtkActor();
            planeWidget = new vtkImplicitPlaneWidget();
            plane = new vtkPlane();
            ren1.RemoveActor(restActor);
            restActor = new vtkActor();


            //# cut function 
            //            def Cut(v): 
            //    clipper.SetValue(v)
            //    cutEdges.SetValue(0, v)
            //    cutStrips.Update()
            //    cutPoly.SetPoints(cutStrips.GetOutput().GetPoints())
            //    cutPoly.SetPolys(cutStrips.GetOutput().GetLines())
            //    cutMapper.Update()
            //    renWin.Render()

            //# callback function 
            //def myCallback(obj, event): 
            //    global plane, selectActor
            //    obj.GetPlane(plane)
            //    Cut(0.0)# Associate the line widget with the interactor	

            //# read mesh 
            //mesh = vtk.vtkUnstructuredGridReader()
            //mesh.SetFileName(r"mesh.vtk")
            //mesh.Update()



            vtkDataSetSurfaceFilter surfaceFilter = new vtkDataSetSurfaceFilter();

            surfaceFilter.SetInput(childPolydata);
            surfaceFilter.Update();


            vtkPolyDataNormals meshNormals = new vtkPolyDataNormals();
            meshNormals.SetInput(surfaceFilter.GetOutput());

            vtkTransformPolyDataFilter transformFilter = new vtkTransformPolyDataFilter();
            transformFilter.SetInput(meshNormals.GetOutput());
            transformFilter.SetTransform(OriginalTransform);
            transformFilter.Update();



            clipper.SetInput(transformFilter.GetOutput());
            //clipper.SetInput(meshNormals.GetOutput());
            clipper.SetClipFunction(plane);
            clipper.GenerateClippedOutputOn();
            clipper.GenerateClipScalarsOn();
            clipper.SetValue(0.0);


            clipMapper.SetInput(clipper.GetOutput());
            clipMapper.ScalarVisibilityOff();

            vtkProperty backProp = new vtkProperty();
            backProp.SetDiffuseColor(1, 0, 0);


            clipActor.SetMapper(clipMapper);
            clipActor.GetProperty().SetColor(0, 1, 0);
            clipActor.SetBackfaceProperty(backProp);
            //clipActor.SetUserTransform(osimBodyProp.transform);

            cutEdges.SetInput(transformFilter.GetOutput());
            //cutEdges.SetInput(meshNormals.GetOutput());
            cutEdges.SetCutFunction(plane);
            cutEdges.GenerateCutScalarsOn();
            cutEdges.SetValue(0, 0.0);


            cutStrips.SetInput(cutEdges.GetOutput());
            cutStrips.Update();



            cutPoly.SetPoints(cutStrips.GetOutput().GetPoints());
            cutPoly.SetPolys(cutStrips.GetOutput().GetLines());



            cutTriangles.SetInput(cutPoly);


            cutMapper.SetInput(cutTriangles.GetOutput());


            cutActor.SetMapper(cutMapper);
            cutActor.GetProperty().SetColor(0, 0, 1);

            //cutActor.SetUserTransform(osimBodyProp.transform);

            //The clipped part of the mesh is rendered wireframe. 
            vtkPolyDataMapper restMapper = vtkPolyDataMapper.New();
            restMapper.SetInput(clipper.GetClippedOutput());
            restMapper.ScalarVisibilityOff();



            restActor.SetMapper(restMapper);
            restActor.GetProperty().SetRepresentationToWireframe();
            // restActor.SetUserTransform(osimBodyProp.transform);

            //Create plane stuff 

            planeWidget.SetNormalToYAxis(1);
            planeWidget.DrawPlaneOff();

            planeWidget.SetInteractor(iren);
            planeWidget.SetPlaceFactor(1);
            //planeWidget.SetInput(meshNormals.GetOutput());
            planeWidget.SetInput(transformFilter.GetOutput());
            planeWidget.SetInteractor(iren);
            double[] bounds = osimBodyProp.assembly.GetBounds();
            planeWidget.PlaceWidget(bounds[0], bounds[1], bounds[2], bounds[3], bounds[4], bounds[5]);
            planeWidget.OutsideBoundsOff();
            planeWidget.InteractionEvt += new vtkObject.vtkObjectEventHandler(ExecuteHandle2);
            //planeWidget.AddObserver((uint)vtkCommand.EventIds.InteractionEvent, myCallback, 0.01f);
            //planeWidget.InteractionEvt += new vtkObject.vtkObjectEventHandler(myCallback);
            planeWidget.UpdatePlacement();

            // planeWidget.AddObserver("InteractionEvent", myCallback,1);
            planeWidget.On();

            //Add the actors to the renderer, set the background and size 
            ren1.AddActor(clipActor); //The actual mesh you want to keep
                                      //   ren1.AddActor(cutActor); //The splitting plane
            ren1.AddActor(restActor); //The rest in wireframe
            //ren1.SetBackground(0.0, 0.0, 0.0);
            //ren1.GetActiveCamera().Azimuth(30);
            //ren1.GetActiveCamera().Elevation(30);
            //ren1.GetActiveCamera().Dolly(0.03);
            //ren1.ResetCameraClippingRange();

            //double[] origin = osimBodyProp.assembly.GetPosition();
            //plane.SetOrigin(origin[0], origin[1], origin[2]);

            iren.Initialize();
           // ren1.Render();
            ren1.GetRenderWindow().Render();
            // iren.Start();


        }


        private void btnConfirmCut_Click(object sender, EventArgs e)
        {
            childPolydata = ConfirmCut();
            osimBodyProp._OsimGeometryPropertyList[0]._vtkActor.GetProperty().SetOpacity(0.1);

            //ren1.RemoveActor(cutActorChild);


            ren1.GetRenderWindow().Render();
            if (firstChildCutDone == false)
            { firstChildCutDone = true; }

            btnConfirmCut.Visible = false;
            btnCutChildMesh.Enabled = true;
            btnConfirmChild.Visible = true;
        }
        private void btnConfirmCutParent_Click(object sender, EventArgs e)
        {
            parentPolydata = ConfirmCut();
            parentBodyProp._OsimGeometryPropertyList[0]._vtkActor.GetProperty().SetOpacity(0.1);


            ren1.GetRenderWindow().Render();

            if (firstParentCutDone == false)
            { firstParentCutDone = true; }

            btnConfirmCutParent.Visible = false;
            btnConfirmParent.Visible = true;
            btnCutParentMesh.Enabled = true;
        }

        private vtkPolyData ConfirmCut()
        {

            planeWidget.Off();


            ////THE ONE IS NEED:    
            //childPolydata = CloseMesh(clipper.GetOutput());
            //clipMapper.SetInput(childPolydata);
            //clipMapper.Update();

            //clipActor.SetMapper(cutMapper);

            vtkAppendPolyData vtkAppendPolyData = new vtkAppendPolyData();
            vtkAppendPolyData.AddInput(clipper.GetOutput());
            //vtkAppendPolyData.AddInput(cutTriangles.GetOutput());
            vtkAppendPolyData.Update();

            // Remove any duplicate points.
            vtkCleanPolyData cleanFilter = vtkCleanPolyData.New();
            cleanFilter.SetInputConnection(vtkAppendPolyData.GetOutputPort());
            cleanFilter.Update();



            //ren1.RemoveActor(clipActor);
            ren1.RemoveActor(cutActorChild);
            ren1.RemoveActor(restActor);


            return cleanFilter.GetOutput();

        }




        #endregion

        #region ICP stuff

        private void btnICPafterCut_Click(object sender, EventArgs e)
        {
            vtkTransform noTransf = new vtkTransform();

            ICPmeshProtocol(childPolydata, noTransf, parentPolydata, parentBodyProp.transform);

        }

        private void btnICPaftercuttingCandP_Click(object sender, EventArgs e)
        {
            richTextICPmessages.Clear();
            vtkTransform childtransf = new vtkTransform();
            vtkTransform parenttransf = new vtkTransform();
            if (!firstChildCutDone)
            {
                childtransf = osimBodyProp.transform;
            }

            if (!firstChildCutDone)
            {
                parenttransf = parentBodyProp.transform;
            }

            if (inferiorReference) //Default.
            {
                ICPmeshProtocol(childPolydata, childtransf, parentPolydata, parenttransf);
            } else
            {
                ICPmeshProtocol(parentPolydata, parenttransf, childPolydata, childtransf);
            }
        }


        private void ICPmeshProtocol(vtkPolyData chilPolydata, vtkTransform childTransform, vtkPolyData parentPolydata, vtkTransform parentTransform)
        {
            vtkTransformPolyDataFilter transformFilter = new vtkTransformPolyDataFilter();
            transformFilter.SetInput(chilPolydata);  //To be replaced with the cutted part
            transformFilter.SetTransform(childTransform);
            transformFilter.Update();

            vtkTransformPolyDataFilter transformFilterP = new vtkTransformPolyDataFilter();
            transformFilterP.SetInput(parentPolydata); //To be replaced with the cutted part
            transformFilterP.SetTransform(parentTransform);
            transformFilterP.Update();


            vtkIterativeClosestPointTransform icp = vtkIterativeClosestPointTransform.New();
            icp.SetSource(transformFilterP.GetOutput());
            icp.SetTarget(transformFilter.GetOutput());
            //icp->GetLandmarkTransform()->SetModeToRigidBody(); 
            icp.GetLandmarkTransform().SetModeToRigidBody();
            icp.SetMaximumNumberOfIterations(Convert.ToInt32(maxnumberIteration.Value));
            icp.StartByMatchingCentroidsOn();
            icp.Modified();
            icp.Update();



            ///Printing out the delta transformation

            vtkMatrix4x4 mat = icp.GetMatrix();
            vtkTransform vtkTransform = new vtkTransform();
            vtkTransform.SetMatrix(mat);
            vtkTransform.Update();
            double[] orientation = vtkTransform.GetOrientation();
            double[] translation = vtkTransform.GetPosition();

            double X = orientation[0];
            double Y = orientation[1];
            double Z = orientation[2];

          
            richTextICPmessages.AppendText("Delta Orientation from ICP tranform:");
            richTextICPmessages.Text += Environment.NewLine;
            richTextICPmessages.AppendText(" X: " + X.ToString() + "  " +" Y: " + Y.ToString() + "  " + " Z: " + Z.ToString());
            richTextICPmessages.Text += Environment.NewLine;


            X = translation[0];
            Y = translation[1];
            Z = translation[2];







            ///VISUALIZATION 

            //Transform the parent to the child polydata object. 
            vtkTransformPolyDataFilter transformFilterSolution = new vtkTransformPolyDataFilter();
            transformFilterSolution.SetInput(transformFilterP.GetOutput());
            transformFilterSolution.SetTransform(vtkTransform);
            transformFilterSolution.Update();


            vtkPolyDataMapper sourceMapper = vtkPolyDataMapper.New();
            sourceMapper.SetInputConnection(transformFilterP.GetOutputPort());

            sourceActor = new vtkActor();
            solutionActor = new vtkActor();
            targetActor = new vtkActor();


            sourceActor.SetMapper(sourceMapper);
            sourceActor.GetProperty().SetColor(1, 0, 0);
            sourceActor.GetProperty().SetPointSize(8);

            vtkPolyDataMapper targetMapper = vtkPolyDataMapper.New();
            targetMapper.SetInputConnection(transformFilter.GetOutputPort());

            targetActor.SetMapper(targetMapper);
            targetActor.GetProperty().SetColor(0, 1, 0);
            targetActor.GetProperty().SetPointSize(8);


            vtkPolyDataMapper solutionMapper = vtkPolyDataMapper.New();
            solutionMapper.SetInputConnection(transformFilterSolution.GetOutputPort());


            solutionActor.SetMapper(solutionMapper);
            solutionActor.GetProperty().SetColor(0, 0, 1);


            ren1.AddActor(sourceActor);
            ren1.AddActor(targetActor);
            ren1.AddActor(solutionActor);
            ren1.GetRenderWindow().Render();








            //JOINT REPLACEMENT
            vtkTransform t = new vtkTransform();

            //vtkBoxWidget.GetTransform(t);

            double[] endplateOrientation = endplateAxesTransform.GetOrientation();


            richTextICPmessages.AppendText("Orientation of the endplate reference system:");
            richTextICPmessages.Text += Environment.NewLine;
            richTextICPmessages.AppendText(" X: " + endplateOrientation[0].ToString() + "  " + " Y: " + endplateOrientation[1].ToString() + "  " + " Z: " + endplateOrientation[2].ToString());
            richTextICPmessages.Text += Environment.NewLine;


            t.PostMultiply();
            t.RotateY(endplateOrientation[1] + (orientation[1] / 2));
            t.RotateX(endplateOrientation[0] + (orientation[0] / 2));
            t.RotateZ(endplateOrientation[2] + (orientation[2] / 2));

            //t.PostMultiply();
            //t.RotateY(endplateOrientation[1]);
            //t.RotateX(endplateOrientation[0]); 
            //t.RotateZ(endplateOrientation[2]);

            t.PostMultiply();

            //t.RotateX(orientation[0] / 2);
            //t.RotateY(orientation[1] / 2);
            //t.RotateZ(orientation[2] / 2);


            t.Translate(osimBodyProp.osimJointProperty._vtkTransform.GetPosition()[0], osimBodyProp.osimJointProperty._vtkTransform.GetPosition()[1], osimBodyProp.osimJointProperty._vtkTransform.GetPosition()[2]);


            t.Update();


            osimBodyProp.osimJointProperty._jointActor.SetUserTransform(t);
            osimBodyProp.osimJointProperty.axesActor.SetUserTransform(t);




            SimModelVisualization.SetJointPlacement(osimBodyProp.osimJointProperty.joint, osimBodyProp.transform, (vtkTransform)osimBodyProp.osimJointProperty._jointActor.GetUserTransform(), parentBodyProp.transform);

            osimBodyProp.osimJointProperty.ReadJoint();
            osimBodyProp.osimJointProperty.SetTransformation();





            ///Printing out the new Joint orientation and position.

            double[] locationF = t.GetPosition();
            double[] orientationF = t.GetOrientation();


            richTextICPmessages.AppendText("New orientation of the joint:");
            richTextICPmessages.Text += Environment.NewLine;
            richTextICPmessages.AppendText(" X: " + orientationF[0].ToString() + "  " + " Y: " + orientationF[1].ToString() + "  " + " Z: " + orientationF[2].ToString());
            richTextICPmessages.Text += Environment.NewLine;



            ren1.GetRenderWindow().Render();

        }


        private void btnICPWithoutCutting_Click(object sender, EventArgs e)
        {


            ICPmeshProtocol(childPolydata, osimBodyProp.transform, parentPolydata, parentBodyProp.transform);
        }


        #endregion



        private void grdSettings_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            currentCoor = SimModelVisualization.getSpecifiedCoorPropertyFromName(osimBodyProp.osimJointProperty, grdSettings.Rows[e.RowIndex].Cells["ID"].Value.ToString());
        }

        private void btnWireframe_Click(object sender, EventArgs e)
        {
            foreach (OsimGeometryProperty geomProp in osimBodyProp._OsimGeometryPropertyList)
            {
                geomProp._vtkActor.GetProperty().SetRepresentationToWireframe();
            }
            foreach (OsimGeometryProperty geomProp in parentBodyProp._OsimGeometryPropertyList)
            {
                geomProp._vtkActor.GetProperty().SetRepresentationToWireframe();
            }

            ren1.GetRenderWindow().Render();
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void btnGetOrientation_Click(object sender, EventArgs e)
        {

        }

        #endregion
       



        private void label10_Click(object sender, EventArgs e)
        {

        }
       
        private void btnVertebralJoint_Click(object sender, EventArgs e)
        {
            btnVertebralJoint.BackColor = Color.LightGreen;
            btnHipJoint.BackColor = Color.LightGray;
            btnKneeJoint.BackColor = Color.LightGray;
            btnShoulderJoint.BackColor = Color.LightGray;
            btnConfirmJointType.Visible = true;
            pictureBox1.BackgroundImage = SpineAnalyzer.Properties.Resources.ArrowDownGreen;
            selectedJointType = "Vertebral Joint";
        }

        private void btnShoulderJoint_Click(object sender, EventArgs e)
        {

            btnShoulderJoint.BackColor = Color.LightGreen;
            btnHipJoint.BackColor = Color.LightGray;
            btnKneeJoint.BackColor = Color.LightGray;
            btnVertebralJoint.BackColor = Color.LightGray;
            btnConfirmJointType.Visible = true;
            pictureBox1.BackgroundImage = SpineAnalyzer.Properties.Resources.ArrowDownGreen;
            selectedJointType = "Shoulder Joint";
        }

        private void btnHipJoint_Click(object sender, EventArgs e)
        {

            btnHipJoint.BackColor = Color.LightGreen;
            btnVertebralJoint.BackColor = Color.LightGray;
            btnKneeJoint.BackColor = Color.LightGray;
            btnShoulderJoint.BackColor = Color.LightGray;
            btnConfirmJointType.Visible = true;
            pictureBox1.BackgroundImage = SpineAnalyzer.Properties.Resources.ArrowDownGreen;
            selectedJointType = "Hip Joint";
        }

        private void btnKneeJoint_Click(object sender, EventArgs e)
        {
            btnKneeJoint.BackColor = Color.LightGreen;
            btnHipJoint.BackColor = Color.LightGray;
            btnVertebralJoint.BackColor = Color.LightGray;
            btnShoulderJoint.BackColor = Color.LightGray;
            btnConfirmJointType.Visible = true;
            pictureBox1.BackgroundImage = SpineAnalyzer.Properties.Resources.ArrowDownGreen;
            selectedJointType = "Knee Joint";
        }

        private void btnConfirmJointType_Click(object sender, EventArgs e)
        {
            
            panelJointOptions.Visible = false;


            if (JointProtocol == "Automatic")
            {
                //CASES FOR THE DIFFERENT JOINT TYPES
                if (selectedJointType == "Vertebral Joint")
                {
                    panelStepOverviewVertebrae.Visible = true;
                    panelJointDefinition.Visible = false;
                    btnSelectLMs.Visible = true;
                    btnSelectLMs.Enabled = true;
                    btnSelectLMs.BackColor = Color.LightGreen;
                    btnSelectLMs.FlatAppearance.MouseOverBackColor = Color.Lime;
                   
                    
                    btnSelectJointType.Enabled = false;
                    pictureBox1.BackgroundImage = SpineAnalyzer.Properties.Resources.ArrowDownGreen;

                    btnSelectLMs.PerformClick();
                }
               
                
                
            }
            if (JointProtocol == "Manual")
            {
                SetupPanelJointDefinition();
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            pictureBox4.BackgroundImage = SpineAnalyzer.Properties.Resources.ArrowDownGreen;
            panelLandmarks.Visible = false;
            panelDefineJoint.Dock = DockStyle.Fill;
            btnEndplates.Visible = true;
            btnEndplates.Enabled = true;
            btnEndplates.BackColor = Color.LightGreen;
            btnEndplates.FlatAppearance.MouseOverBackColor = Color.Lime;
            btnSelectLMs.Enabled = false;

            //MakeMeshFromLMs();

            btnEndplates.PerformClick();
        }

        private void tableLayoutPanel7_Paint(object sender, PaintEventArgs e)
        {

        }

        private void panel16_Paint(object sender, PaintEventArgs e)
        {

        }

        private void btnViewLMChild_Click(object sender, EventArgs e)
        {
            if (currentGeomProp.geometryFileObject != null)
            {
                ShowSelectedLMs(currentGeomProp.geometryFileObject, dataGridViewLMchild, osimBodyProp);
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (currentGeomProp.geometryFileObject != null)
            {

                ShowAllLMs(currentGeomProp.geometryFileObject, dataGridViewLMchild, osimBodyProp);
            }
            if (parentBodyProp._OsimGeometryPropertyList[0].geometryFileObject != null)
            {

                ShowAllLMs(parentBodyProp._OsimGeometryPropertyList[0].geometryFileObject, dataGridViewLMparent, parentBodyProp);
            }

            //IF clicked: 
            button5.Visible = false;
            btnViewLMChild.Visible = false;
            button8.Visible = false;

        }

        private void btnSelectJointType_Click(object sender, EventArgs e)
        {
            SetupPanelJointDefinition();
        }

        private void SetupPanelJointDefinition()
        {
            HideAllPanels();
            panelJointDefinition.Dock = DockStyle.Fill;
            btnPredefinedJOintTypes.BackColor = Color.White;
            btnManualRedef.BackColor = Color.White;
            panelStepOverviewVertebrae.Visible = false;
            panelJointDefinition.Visible = true;
            btnConfirmJointType.Visible = false;
        }

        private void HideAllPanels()
        {
            ReferenceAxesPanel.Visible = false;
            panelEndplates.Visible = false;
            panelJointDefinition.Visible = false;
            panelLandmarks.Visible = false;
            panelDefineJoint.Visible = false;
        }

        private void button8_Click(object sender, EventArgs e)
        {
            if (parentBodyProp._OsimGeometryPropertyList[0].geometryFileObject != null)
            {

                ShowSelectedLMs(parentBodyProp._OsimGeometryPropertyList[0].geometryFileObject, dataGridViewLMparent, parentBodyProp);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            HideAllPanels();
            ReferenceAxesPanel.Visible = true;
            
        }

        private void btnConfirmAutomaticJointDefinition_Click(object sender, EventArgs e)
        {
            panelJointPosition.Visible = false;
            btnFinished.BackColor = Color.FromArgb(192, 255, 192);
            btnFinished.Enabled = true;
            EnableAllJointButtons();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            HideAllPanels();
            panelDefineJoint.Dock = DockStyle.Fill;
            panelDefineJoint.Visible = true;



        }

        private void ResetPanels()
        {
            HideAllPanels();

            pictureBox1.BackgroundImage = SpineAnalyzer.Properties.Resources.ArrowDownRed;
            pictureBox2.BackgroundImage = SpineAnalyzer.Properties.Resources.ArrowDownRed;
            pictureBox3.BackgroundImage = SpineAnalyzer.Properties.Resources.ArrowDownRed;
            pictureBox4.BackgroundImage = SpineAnalyzer.Properties.Resources.ArrowDownRed;
            panelStepOverviewVertebrae.Visible = false;
            btnSelectJointType.BackColor = Color.LightGreen;
            btnSelectJointType.FlatAppearance.MouseOverBackColor = Color.Lime;
            btnSelectLMs.BackColor = Color.Salmon;
            btnSelectLMs.FlatAppearance.MouseOverBackColor = Color.Red;
            btnDefineJoint.BackColor = Color.Salmon;
            btnDefineJoint.FlatAppearance.MouseOverBackColor = Color.Red;
            btnFinalize.BackColor = Color.Salmon;
            btnFinalize.FlatAppearance.MouseOverBackColor = Color.Red;
            btnEndplates.BackColor = Color.Salmon;
            btnEndplates.FlatAppearance.MouseOverBackColor = Color.Red;

            btnSelectJointType.Enabled = true;
            btnSelectJointType.Visible = true;
            btnFinalize.Enabled = false;
            btnSelectLMs.Enabled = false;
            btnEndplates.Enabled = false;
            btnDefineJoint.Enabled = false;

            SetupPanelJointDefinition();
        }

        private void btnParentAdd2Selection_Click(object sender, EventArgs e)
        {
            //Clear DataGrid
            dgViewSelectedLMParent.DataSource = null;
            dgViewSelectedLMParent.Refresh();


            foreach (DataGridViewRow DataGridViewRow in returnSelectedDataRows(dataGridViewLMparent))
            {
                int number = Convert.ToInt32(DataGridViewRow.Cells["LandmarkID"].Value);
                //Build SQL Command (=SQL select statement + Parameters)
                SqlCommand SQLcmd = new SqlCommand();

                string SQLselect;

                SQLselect = "SELECT LandmarkID, LandmarkName, UserName FROM GeometryLandmarks where LandmarkID = @LandmarkID";
                SQLcmd.Parameters.AddWithValue("@LandmarkID", number);

                SQLcmd.CommandText = SQLselect;

                //Set Database
                DataBase SQLDB = new DataBase(Appdata.SQLServer, Appdata.SQLDatabase, Appdata.SQLAuthSQL, Appdata.SQLUser, Appdata.SQLPassword);

                //Fill a dataset with selected records
                DataSet ds = new DataSet();
                SQLDB.ReadDataSet(SQLcmd, ref ds);
                selectedDsParent.Tables[0].Rows.Add(ds.Tables[0].Rows[0].ItemArray);
            }

            // you can make the grid readonly.
            dgViewSelectedLMParent.ReadOnly = true;
            dgViewSelectedLMParent.DataSource = selectedDsParent.Tables[0];
            // Resize the DataGridView columns to fit the newly loaded content.
            dgViewSelectedLMParent.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.ColumnHeader);


        }

        private void btnAutoSelectLMChild_Click(object sender, EventArgs e)
        {
            List<int> ChildLMoptions = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8 };
            dataGridViewLMchild.ClearSelection();

            int rowIndex = -1;
            foreach (DataGridViewRow row in dataGridViewLMchild.Rows)
            {
                if (ChildLMoptions.Contains(Convert.ToInt32(row.Cells["LMoptionID"].Value)))
                {
                    rowIndex = row.Index;
                    dataGridViewLMchild.Rows[rowIndex].Selected = true;

                }
            }
            btnChildAdd2Selection.PerformClick();
        }

        private void btnAutoSelectLMParent_Click(object sender, EventArgs e)
        {
            List<int> ParentLMoptions = new List<int>() { 10, 11, 12, 13, 14, 15, 16, 17 };

            dataGridViewLMparent.ClearSelection();
            int rowIndex = -1;
            foreach (DataGridViewRow row in dataGridViewLMparent.Rows)
            {
                if (ParentLMoptions.Contains(Convert.ToInt32(row.Cells["LMoptionID"].Value)))
                {
                    rowIndex = row.Index;
                    dataGridViewLMparent.Rows[rowIndex].Selected = true;

                }
            }
            btnParentAdd2Selection.PerformClick();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            btnAutoSelectLMChild.PerformClick();
            btnAutoSelectLMParent.PerformClick();
        }

        private void btnMakeMesh_Click(object sender, EventArgs e)
        {
            MakeMeshFromLMs();
        }

        private void btnChildRemoveFromSelection_Click(object sender, EventArgs e)
        {


            foreach (DataGridViewRow row in dgViewSelectedLMChild.SelectedRows)
            {


                foreach (DataGridViewRow row2 in dataGridViewLMchild.Rows)
                {

                    if ((Convert.ToInt32(row2.Cells["LandmarkID"].Value)) == (Convert.ToInt32(row.Cells["LandmarkID"].Value)))
                    {

                        dataGridViewLMchild.Rows[row2.Index].DefaultCellStyle.BackColor = Color.White;

                    }
                }
                selectedDsChild.Tables[0].Rows[row.Index].Delete();

            }
            selectedDsChild.AcceptChanges();
            dgViewSelectedLMChild.Update();
        }

        private void btnParentRemoveFromSelection_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dgViewSelectedLMParent.SelectedRows)
            {
                foreach (DataGridViewRow row2 in dataGridViewLMparent.Rows)
                {
                    if ((Convert.ToInt32(row2.Cells["LandmarkID"].Value)) == (Convert.ToInt32(row.Cells["LandmarkID"].Value)))
                    {
                        dataGridViewLMparent.Rows[row2.Index].DefaultCellStyle.BackColor = Color.White;
                    }
                }
                selectedDsParent.Tables[0].Rows[row.Index].Delete();
            }
            selectedDsParent.AcceptChanges();
            dgViewSelectedLMParent.Update();
        }

        private void tableLayoutPanel9_Paint(object sender, PaintEventArgs e)
        {

        }

        private void btnICP_Top_Bottom_Click(object sender, EventArgs e)
        {


            vtkPoints targetPoints = new vtkPoints();
            int RowCount2 = dgViewSelectedLMChild.Rows.Count;


            //int RowcountMin = 0;
            //if(RowCount < RowCount2)
            //{
            //    RowcountMin = RowCount;
            //}
            //else
            //{
            //    RowcountMin = RowCount2;

            //}
            if (RowCount2 == 0)
                return;

            for (int i = 0; i < RowCount2; i++)
            {
                //Get row of clicked cell
                DataGridViewRow row = dgViewSelectedLMChild.Rows[dgViewSelectedLMChild.Rows[i].Index];
                //Get column AcquisitionNumber
                DataGridViewCell cell = row.Cells["LandmarkID"];
                //create AcquisitionObject to be deleted

                Landmark SelectedLandmark = new Landmark(SQLDB, Subject, osimBodyProp._OsimGeometryPropertyList[0].geometryFileObject.GeometryNumber, Convert.ToInt32(cell.Value), Appdata);

                //double[] p = new double[] { SelectedLandmark.coorX, SelectedLandmark.coorY, SelectedLandmark.coorZ };

                if (SelectedLandmark.LMoptionID == 1 || SelectedLandmark.LMoptionID == 7 || SelectedLandmark.LMoptionID == 8)
                {
                    double[] p = new double[] { SelectedLandmark.AbsX, SelectedLandmark.AbsY, SelectedLandmark.AbsZ };

                    targetPoints.InsertNextPoint(p[0], p[1], p[2]);
                }


            }


            vtkPoints sourcePoints = new vtkPoints();

            //Check if rows selected

            int RowCount = dgViewSelectedLMParent.Rows.Count;
            if (RowCount == 0)
                return;

            for (int i = 0; i < RowCount; i++)
            {
                //Get row of clicked cell
                DataGridViewRow row = dgViewSelectedLMParent.Rows[dgViewSelectedLMParent.Rows[i].Index];
                //Get column AcquisitionNumber
                DataGridViewCell cell = row.Cells["LandmarkID"];
                //create AcquisitionObject to be deleted

                Landmark SelectedLandmark = new Landmark(SQLDB, Subject, parentBodyProp._OsimGeometryPropertyList[0].geometryFileObject.GeometryNumber, Convert.ToInt32(cell.Value), Appdata);

                //double[] p = new double[] { SelectedLandmark.coorX, SelectedLandmark.coorY, SelectedLandmark.coorZ };
                if (SelectedLandmark.LMoptionID == 10 || SelectedLandmark.LMoptionID == 16 || SelectedLandmark.LMoptionID == 17)
                {
                    double[] p = new double[] { SelectedLandmark.AbsX, SelectedLandmark.AbsY, SelectedLandmark.AbsZ };
                    sourcePoints.InsertNextPoint(p[0], p[1], p[2]);
                }

            }







            //TRANSFORMATIE
            vtkLandmarkTransform vtkLandmarkTransform = new vtkLandmarkTransform();
            vtkLandmarkTransform.SetSourceLandmarks(sourcePoints);
            vtkLandmarkTransform.SetTargetLandmarks(targetPoints);
            // vtkLandmarkTransform.SetModeToAffine(); //CHECK THE DIFFERENCES IF SCALING WOULD BE ALLOWED
            //vtkLandmarkTransform.SetModeToRigidBody();

            vtkLandmarkTransform.Update();



            vtkPolyData source = new vtkPolyData();
            source.SetPoints(sourcePoints);

            vtkPolyData target = new vtkPolyData();
            target.SetPoints(targetPoints);


            vtkVertexGlyphFilter sourceGlyphFilter = new vtkVertexGlyphFilter();
            sourceGlyphFilter.SetInputConnection(source.GetProducerPort());
            sourceGlyphFilter.Update();

            vtkVertexGlyphFilter targetGlyphFilter = new vtkVertexGlyphFilter();
            targetGlyphFilter.SetInputConnection(target.GetProducerPort());
            targetGlyphFilter.Update();


            vtkTransformPolyDataFilter transformFilter = new vtkTransformPolyDataFilter();
            transformFilter.SetInputConnection(sourceGlyphFilter.GetOutputPort());
            transformFilter.SetTransform(vtkLandmarkTransform);
            transformFilter.Update();

            vtkMatrix4x4 mat = vtkLandmarkTransform.GetMatrix();
            vtkTransform vtkTransform = new vtkTransform();
            vtkTransform.SetMatrix(mat);
            vtkTransform.Update();
            double[] orientation = vtkTransform.GetOrientation();
            double[] translation = vtkTransform.GetPosition();

            //CALCULATE NEW POSITION FOR THE JOINT


            //vtkTransform MIDTransform = new vtkTransform();

            //MIDTransform.PostMultiply();
            //MIDTransform.RotateX(orientation[0] / 2);
            //MIDTransform.RotateY(orientation[1] / 2);
            //MIDTransform.RotateZ(orientation[2] / 2);

            //MIDTransform.Update();


          

            //vtkTransformPolyDataFilter MIDtransformFilter = new vtkTransformPolyDataFilter();
            //MIDtransformFilter.SetInputConnection(sourceGlyphFilter.GetOutputPort());
            //MIDtransformFilter.SetTransform(vtkLandmarkTransform);
            //MIDtransformFilter.Update();



            //JOINT REPLACEMENT
            vtkTransform t = new vtkTransform();
            t.PostMultiply();
            //vtkBoxWidget.GetTransform(t);
            t.RotateX(orientation[0] / 2);
            t.RotateY(orientation[1] / 2);
            t.RotateZ(orientation[2] / 2);

            t.Translate(osimBodyProp.osimJointProperty._vtkTransform.GetPosition()[0], osimBodyProp.osimJointProperty._vtkTransform.GetPosition()[1], osimBodyProp.osimJointProperty._vtkTransform.GetPosition()[2]);


            t.Update();



            double[] locationF = t.GetPosition();
            double[] orientationF = t.GetOrientation();


            osimBodyProp.osimJointProperty._jointActor.SetUserTransform(t);
            osimBodyProp.osimJointProperty.axesActor.SetUserTransform(t);

            SimModelVisualization.SetJointPlacement(osimBodyProp.osimJointProperty.joint, osimBodyProp.transform, (vtkTransform)osimBodyProp.osimJointProperty._jointActor.GetUserTransform(), parentBodyProp.transform);

            osimBodyProp.osimJointProperty.ReadJoint();
            osimBodyProp.osimJointProperty.SetTransformation();

            ren1.GetRenderWindow().Render();



            //VISUALIZATION 
            vtkPolyDataMapper sourceMapper = vtkPolyDataMapper.New();
            sourceMapper.SetInputConnection(sourceGlyphFilter.GetOutputPort());

            vtkActor sourceActor = new vtkActor();
            sourceActor.SetMapper(sourceMapper);
            sourceActor.GetProperty().SetColor(0, 1, 0);
            sourceActor.GetProperty().SetPointSize(8);

            vtkPolyDataMapper targetMapper = vtkPolyDataMapper.New();
            targetMapper.SetInputConnection(targetGlyphFilter.GetOutputPort());

            vtkActor targetActor = new vtkActor();
            targetActor.SetMapper(targetMapper);
            targetActor.GetProperty().SetColor(1, 0, 0);
            targetActor.GetProperty().SetPointSize(8);


            vtkPolyDataMapper solutionMapper = vtkPolyDataMapper.New();
            solutionMapper.SetInputConnection(transformFilter.GetOutputPort());

            vtkActor solutionActor = new vtkActor();
            solutionActor.SetMapper(solutionMapper);
            solutionActor.GetProperty().SetColor(0, 0, 1);
            solutionActor.GetProperty().SetPointSize(8);

            ren1.AddActor(sourceActor);
            ren1.AddActor(targetActor);
            ren1.AddActor(solutionActor);


        }

        private void btnResetJointOrientation_Click(object sender, EventArgs e)
        {
            ResetJointOrAndPos();
            ren1.GetRenderWindow().Render();

        }

        public void ResetJointOrAndPos()
        {

            ////JOINT REPLACEMENT
            //vtkTransform t = new vtkTransform();
            //t.PostMultiply();
            ////vtkBoxWidget.GetTransform(t);
            //t.RotateX(0);
            //t.RotateY(15);
            //t.RotateZ(0);

            //t.Translate(osimBodyProp.osimJointProperty._vtkTransform.GetPosition()[0], osimBodyProp.osimJointProperty._vtkTransform.GetPosition()[1], osimBodyProp.osimJointProperty._vtkTransform.GetPosition()[2]);


            //t.Update();



            //double[] locationF = t.GetPosition();
            //double[] orientationF = t.GetOrientation();


            osimBodyProp.osimJointProperty._jointActor.SetUserTransform(InitialJoinTransf);
            osimBodyProp.osimJointProperty.axesActor.SetUserTransform(InitialJoinTransf);


            SimModelVisualization.SetJointPlacement(osimBodyProp.osimJointProperty.joint, osimBodyProp.transform, (vtkTransform)osimBodyProp.osimJointProperty._jointActor.GetUserTransform(), parentBodyProp.transform);

            osimBodyProp.osimJointProperty.ReadJoint();
            osimBodyProp.osimJointProperty.SetTransformation();


           
        }

        private void btnCalcEndplateCOM_Click(object sender, EventArgs e)
        {

        }

        private void btnEndplates_Click(object sender, EventArgs e)
        {
            HideAllPanels();
            btnChildEndplate.Enabled = true;
            panelChildEndplateTools.Enabled = false;
            btnParentEndplate.Enabled = false;
            panelParentEndplateTools.Enabled = false;
            btnConfirmChild.Visible = false;
            btnConfirmParent.Visible = false;
            btnConfirmCut.Visible = false;
            btnConfirmCutParent.Visible = false;

            firstChildCutDone = false;
            firstParentCutDone = false;

            panelEndplates.Dock = DockStyle.Fill;
            panelEndplates.Visible = true;
           
        }

        private void btnChildEndplate_Click(object sender, EventArgs e)
        {
            ren1.RemoveActor(clipActor);
            ren1.RemoveActor(osimBodyProp.osimJointProperty.axesActor);
            ren1.RemoveActor(parentBodyProp.assembly);
            RenderWindow.Render();
            panelChildEndplateTools.Enabled = true;
            btnChildEndplate.Enabled = false;

        }

        private void btnParentEndplate_Click(object sender, EventArgs e)
        {
            btnParentEndplate.Enabled = false;
            ren1.RemoveActor(clipActor);
            ren1.RemoveActor(osimBodyProp.osimJointProperty.axesActor);
            ren1.RemoveActor(osimBodyProp.assembly);
            RenderWindow.Render();
            panelParentEndplateTools.Enabled = true;


        }

        private void btnDoneChildEndplate_Click(object sender, EventArgs e)
        {
            
        }

        private void btnConfirmChild_Click(object sender, EventArgs e)
        {
            ren1.AddActor(osimBodyProp.osimJointProperty.axesActor);
            clipActorChild = clipActor;
            ren1.AddActor(parentBodyProp.assembly);
            ren1.AddActor(clipActorParent);
            RenderWindow.Render();
            panelChildEndplateTools.Enabled = false;
            btnParentEndplate.Enabled = true;
        }

        private void btnCutChildMesh_Click(object sender, EventArgs e)
        {
            btnConfirmCut.Visible = true;
            btnConfirmChild.Visible = false;
            btnCutChildMesh.Enabled = false;
            if (firstChildCutDone == false)
            {
                osimBodyProp._OsimGeometryPropertyList[0]._vtkActor.GetProperty().SetOpacity(0);
                planecutter(osimBodyProp.transform, childPolydata, osimBodyProp, cutActorChild);
            }
            else
            {

                restActor.GetProperty().SetOpacity(0);
                clipActor.GetProperty().SetOpacity(0);
                cutActorChild.GetProperty().SetOpacity(0);
                //ren1.RemoveActor(restActor);
                //ren1.RemoveActor(clipActorChild);
                //ren1.RemoveActor(cutActor);
                ren1.GetRenderWindow().Render();
                osimBodyProp._OsimGeometryPropertyList[0]._vtkActor.GetProperty().SetOpacity(0);
                vtkTransform nulltrans = new vtkTransform();
                planecutter(nulltrans, childPolydata, osimBodyProp, cutActorChild);
            }

        }

        private void btnCutParentMesh_Click(object sender, EventArgs e)
        {
            btnConfirmCutParent.Visible = true;
            btnConfirmParent.Visible = false;
            btnCutParentMesh.Enabled = false;

            if (firstParentCutDone == false)
            {
                parentBodyProp._OsimGeometryPropertyList[0]._vtkActor.GetProperty().SetOpacity(0);
                planecutter(parentBodyProp.transform, parentPolydata, parentBodyProp, cutActorParent);
            }
            else
            {
                ren1.RemoveActor(restActor);
                ren1.RemoveActor(clipActor);
                ren1.RemoveActor(cutActorParent);
                ren1.GetRenderWindow().Render();
                parentBodyProp._OsimGeometryPropertyList[0]._vtkActor.GetProperty().SetOpacity(0);
                vtkTransform nulltrans = new vtkTransform();
                planecutter(nulltrans, parentPolydata, parentBodyProp, cutActorParent);
            }
        }

        private void btnConfirmParent_Click(object sender, EventArgs e)
        {
            panelEndplates.Visible = false;
            ReferenceAxesPanel.Dock = DockStyle.Fill;
            ReferenceAxesPanel.Visible = true;

            ren1.AddActor(osimBodyProp.osimJointProperty.axesActor);
            ren1.AddActor(osimBodyProp.assembly);
            ren1.AddActor(clipActorChild);
            RenderWindow.Render();
            panelParentEndplateTools.Enabled = false;
            panelEndplates.Visible = false;

            pictureBox2.BackgroundImage = SpineAnalyzer.Properties.Resources.ArrowDownGreen;

            panelDefineJoint.Dock = DockStyle.Fill;
            btnDefineJoint.Visible = true;
            btnDefineJoint.Enabled = true;
            btnDefineJoint.BackColor = Color.LightGreen;
            btnDefineJoint.FlatAppearance.MouseOverBackColor = Color.Lime;

            btnEndplates.Enabled = false;
            btnDefineJoint.PerformClick();

        }

        private void btnDoneParentEndplate_Click(object sender, EventArgs e)
        {
            ren1.AddActor(osimBodyProp.osimJointProperty.axesActor);
            ren1.AddActor(osimBodyProp.assembly);
            ren1.AddActor(clipActorChild);
            RenderWindow.Render();
            panelParentEndplateTools.Enabled = false;
            panelEndplates.Visible = false;

            pictureBox2.BackgroundImage = SpineAnalyzer.Properties.Resources.ArrowDownGreen;

            panelDefineJoint.Dock = DockStyle.Fill;
            btnDefineJoint.Visible = true;
            btnDefineJoint.Enabled = true;
            btnDefineJoint.BackColor = Color.LightGreen;
            btnDefineJoint.FlatAppearance.MouseOverBackColor = Color.Lime;

            btnEndplates.Enabled = false;
            btnDefineJoint.PerformClick();



        }

        private void btnPredefinedJOintTypes_Click(object sender, EventArgs e)
        {
            panel21.Dock = DockStyle.Fill;
            panel21.Visible = true;
            btnManualRedef.BackColor = Color.LightGray;
            btnPredefinedJOintTypes.BackColor = Color.LightGreen;
            panelManualJointTools.Visible = false;
            btnConfirmJointType.Visible = false;


            btnVertebralJoint.BackColor = Color.White;
            btnHipJoint.BackColor = Color.White;
            btnKneeJoint.BackColor = Color.White;
            btnShoulderJoint.BackColor = Color.White;
            JointProtocol = "Automatic";
            panelJointOptions.Visible = true;
        }

        private void btnManualRedef_Click(object sender, EventArgs e)
        {
            panelManualJointTools.Dock = DockStyle.Fill;
            panel21.Visible = false;
            btnManualRedef.BackColor = Color.LightGreen;
            btnPredefinedJOintTypes.BackColor = Color.LightGray;
            panelManualJointTools.Visible = true;
            btnConfirmJointType.Visible = false;
            JointProtocol = "Manual";
            panelJointOptions.Visible = true;

        }

        public void StoreInitialTransformJoint()
        {
             InitialJoinTransf = (vtkTransform)osimBodyProp.osimJointProperty.axesActor.GetUserTransform();
             InitialOrientation = InitialJoinTransf.GetOrientation();
             InitialPosition = InitialJoinTransf.GetPosition();

        }

        public  Vec3 initGeomTransf = new Vec3();
        public Rotation rot = new Rotation();
        public Transform trst = new Transform();
        public void storeInitialTransformOfDisplayer()
        {
            trst = osimBodyProp.body.getDisplayer().getTransform();
            initGeomTransf = osimBodyProp.body.getDisplayer().getTransform().T();
            rot=  osimBodyProp.body.getDisplayer().getTransform().R();
            //initGeomTransf = osimBodyProp._OsimGeometryPropertyList[0].displayGeometry.getTransform().T();
            //rot=  osimBodyProp._OsimGeometryPropertyList[0].displayGeometry.getTransform().R();
            //trst = osimBodyProp._OsimGeometryPropertyList[0].displayGeometry.getTransform();
        }


        public void ResetInitialTransformOfDisplayer()
        {
        //            public Transform trdst = new Transform( 0,0,0,);;
        //osimBodyProp.body.getDisplayer().setTransform(trst);
           // osimBodyProp._OsimGeometryPropertyList[0].displayGeometry.setTransform(trst);  


        }
        private void UpdateCoordinatesJoint()
        {
            if (rbAbsolute.Checked)
            {
                SetJointAxisActorManually((double)nUpDownXpos.Value / 1000, 0, (double)nUpDownYpos.Value / 1000,0, (double)nUpDownZpos.Value / 1000,0, (double)nUpDownXrot.Value, 0,(double)nUpDownYrot.Value,0, (double)nUpDownZrot.Value,0);
            }
            if (rbRelativeDisplacement.Checked)
            {
                SetJointAxisActorManually(InitialPosition[0], ((double)nUpDownXpos.Value / 1000), InitialPosition[1], ((double)nUpDownYpos.Value / 1000), InitialPosition[2] , ((double)nUpDownZpos.Value / 1000), InitialOrientation[0], ((double)nUpDownXrot.Value), InitialOrientation[1] , ((double)nUpDownYrot.Value), InitialOrientation[2] , ((double)nUpDownZrot.Value));
            }
            RenderWindow.Render();
        }

        public void SetJointAxisActorManually(double transX, double deltaTransX, double transY, double deltaTransY, double transZ, double deltaTransZ, double orX,double deltaOrX, double orY, double deltaOrY,  double orZ, double deltaOrZ)
        {
            //vtkTransform t = new vtkTransform();
            //vtkBoxWidget.GetTransform(t);
            //double[] translation = t.GetPosition();
            //double[] rotation = t.GetOrientation();
            //double[] rotationWXYZ = t.GetOrientationWXYZ();
            vtkTransform d = new vtkTransform();
            //d.SetInput(osimBodyProp.osimJointProperty._vtkTransform);



            d.PostMultiply();

            //First the delta
          
            d.RotateY(deltaOrY);
            d.RotateX(deltaOrX);
            d.RotateZ(deltaOrZ);

            d.Translate(deltaTransX, deltaTransY, deltaTransZ);



            //then the original orientation and position
          
            d.RotateY(orY);
            d.RotateX(orX);
            d.RotateZ(orZ);

            // d.RotateWXYZ(rotationWXYZ[0], rotationWXYZ[1], rotationWXYZ[2], rotationWXYZ[3]);
            d.Translate(transX, transY, transZ);

            osimBodyProp.osimJointProperty._jointActor.SetUserTransform(d);
            osimBodyProp.osimJointProperty.axesActor.SetUserTransform(d);
            axesActorAllIn.SetUserTransform(d);


        }

        private void btnGrabAndReplace_Click(object sender, EventArgs e)
        {
            vtkBoxWidget.SetProp3D(osimBodyProp.osimJointProperty._jointActor);
            // vtkBoxWidget.SetTransform( (vtkTransform)osimBodyProp.osimJointProperty.axesActor.GetUserTransform());
            vtkBoxWidget.SetPlaceFactor(5);

            //boxWidget.TranslationEnabledOff();
            //boxWidget.SetHandleSize(0.0050);
            vtkBoxWidget.HandlesOff();
            vtkBoxWidget.OutlineCursorWiresOff();
            vtkBoxWidget.OutlineFaceWiresOff();
            //vtkBoxWidget.RotationEnabledOff();
            vtkBoxWidget.RotationEnabledOn();
            vtkBoxWidget.ScalingEnabledOff();
            vtkBoxWidget.GetHandleProperty().FrontfaceCullingOff();
            vtkBoxWidget.GetHandleProperty().BackfaceCullingOff();
            vtkBoxWidget.PlaceWidget();


            vtkBoxWidget.InteractionEvt += new vtkObject.vtkObjectEventHandler(ExecuteHandle);

            vtkBoxWidget.SetInteractor(iren);
            vtkBoxWidget.On();
            //vtkBoxWidget.AddObserver((uint)vtkCommand.EventIds.KeyPressEvent, boxCallback, 1.0f);
            RenderWindow.Render();

            btnGrabAndReplace.Enabled = false;
            btnConfirmManualGrabbing.Visible = true;
            btnConfirmManualGrabbing.Enabled = true;
            panel15.Enabled = false;
            btnConfirmJointType.Visible = false;
        }

        public void PushJointUpdateToOpenSim()
        {

            //The joint axes was repositioned (and reoriented) free from the model defenition. 
            //The aim is to use this transformation to recalculate the joint position under the assumption that the 
            //child and parent body do not move. (osimbodyprop of child and parent remain constant). 
            //The joint connecting them is defined in osimjointproperty as location and orientation in parent and child. These values need to be recalculated.
            //Thereafter the new values are transmitted to the osimmodel and to the vtkrendering objects.

            SimModelVisualization.SetJointPlacement(osimBodyProp.osimJointProperty.joint, osimBodyProp.transform, (vtkTransform)osimBodyProp.osimJointProperty.axesActor.GetUserTransform(), parentBodyProp.transform);

            ///SimModelVisualization.UpdateRenderer(ren1);

            osimBodyProp.osimJointProperty.ReadJoint();

            //osimBodyProp.osimJointProperty._vtkTransform = (vtkTransform)osimBodyProp.osimJointProperty.jointActor.GetUserTransform();
            //osimBodyProp.osimJointProperty.SetTransformationFromParent();

            osimBodyProp.osimJointProperty._vtkTransform = (vtkTransform)osimBodyProp.osimJointProperty.axesActor.GetUserTransform();

            //Thread.Sleep(5000);
            //axesActorAllIn.SetUserTransform((vtkTransform)osimBodyProp.osimJointProperty._jointActor.GetUserTransform());
            //SimModelVisualization.UpdateBodyJointTransform(parentBodyProp.body);
            //SimModelVisualization.UpdateBodyJointTransform(osimBodyProp.body);

            osimBodyProp.osimJointProperty.MakeVtkObject();

            //osimBodyProp.osimJointProperty.axesActor.SetUserTransform((vtkTransform)osimBodyProp.osimJointProperty.axesActor.GetUserTransform());
            //SimModelVisualization.UpdateBodyJointTransform(osimBodyProp.osimJointProperty.childBody);


        }
        private void btnConfirmManualGrabbing_Click(object sender, EventArgs e)
        {
            PushJointUpdateToOpenSim();

            ren1.Render();
            ren1.GetRenderWindow().Render();

            vtkBoxWidget.Off();
            btnConfirmManualGrabbing.Enabled = false;
            btnConfirmManualGrabbing.Visible = false;
            btnGrabAndReplace.Enabled = true;
            panel15.Enabled = true;
            btnConfirmJointType.Visible = true;

        }

        private void BtnSetEnplateRefParent_Click(object sender, EventArgs e)
        {
            BtnSetEnplateRefParent.Enabled = false;
            btnDoneEndplateRefParent.Visible = true;
            BtnSetEnplateRefParent.Enabled = false;
            btnConfirmAxisInf.Enabled = false;

            calculateEndplateCOM(parentPolydata);

            MakeEndplateAxes();

            vtkBoxWidget2.SetProp3D(endplateAxesActor);
            vtkBoxWidget2.SetPlaceFactor(1.0);

            //boxWidget.TranslationEnabledOff();
            //boxWidget.SetHandleSize(0.0050);
            vtkBoxWidget2.HandlesOff();
            vtkBoxWidget2.TranslationEnabledOff();
            vtkBoxWidget2.OutlineCursorWiresOff();
            vtkBoxWidget2.OutlineFaceWiresOff();
            
            vtkBoxWidget2.RotationEnabledOn();
            vtkBoxWidget2.ScalingEnabledOff();
            vtkBoxWidget2.GetHandleProperty().FrontfaceCullingOff();
            vtkBoxWidget2.GetHandleProperty().BackfaceCullingOff();
            vtkBoxWidget2.PlaceWidget();

            vtkBoxWidget2.InteractionEvt += new vtkObject.vtkObjectEventHandler(ExecuteHandleEndplate);

            vtkBoxWidget2.SetInteractor(iren);
            vtkBoxWidget2.On();

            RenderWindow.Render();
        }

        private void btnDoneEndplateRefParent_Click(object sender, EventArgs e)
        {
            vtkBoxWidget2.Off();
            endplateAxesTransform = (vtkTransform)endplateAxesActor.GetUserTransform();

            ren1.AddActor(clipActor);
            ren1.AddActor(osimBodyProp.osimJointProperty.axesActor);
            ren1.AddActor(osimBodyProp.assembly);

            btnDoneEndplateRefParent.Visible = false;
            BtnSetEnplateRefParent.Enabled = true;
            btnConfirmAxisInf.Visible = true;
            btnConfirmAxisInf.Enabled = true;
        }

        private void button16_Click(object sender, EventArgs e)
        {
            inferiorReference = false; //Reference is set superiorly.
            button2.Enabled = false;
            panel17.Visible = true;
            panel20.Visible = false;
            button16.BackColor = Color.LightGreen;
            button2.BackColor = Color.LightGray;
            parentBodyProp._OsimGeometryPropertyList[0]._vtkActor.GetProperty().SetOpacity(0);
            osimBodyProp._OsimGeometryPropertyList[0]._vtkActor.GetProperty().SetOpacity(0.1);
           // clipActorParent.GetProperty().SetOpacity(0);
            clipActorChild.GetProperty().SetOpacity(1);
            clipActor.GetProperty().SetOpacity(0);
            HideJointActors();
            RenderWindow.Render();
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            inferiorReference = true;

            button16.Enabled = false;
            panel17.Visible = false;
           
            panel20.Visible = true;
            btnConfirmAxisInf.Visible = false;
            button2.BackColor = Color.LightGreen;
            button16.BackColor = Color.LightGray;

            osimBodyProp._OsimGeometryPropertyList[0]._vtkActor.GetProperty().SetOpacity(0);
            parentBodyProp._OsimGeometryPropertyList[0]._vtkActor.GetProperty().SetOpacity(0.1);
            HideJointActors();
            clipActor.GetProperty().SetOpacity(1);
            clipActorChild.GetProperty().SetOpacity(0);
            //clipActorParent.GetProperty().SetOpacity(1);

            RenderWindow.Render();
        }

        private void HideJointActors()
        {
            osimBodyProp.osimJointProperty.jointActor.GetProperty().SetOpacity(0);
            osimBodyProp.osimJointProperty.axesActor.GetProperty().SetOpacity(0);
            ren1.RemoveActor(axesActorAllIn);
            RenderWindow.Render();
        }

        private void ShowJointActors()
        {
            osimBodyProp.osimJointProperty.jointActor.GetProperty().SetOpacity(1);
            osimBodyProp.osimJointProperty.axesActor.GetProperty().SetOpacity(1);
            ren1.AddActor(axesActorAllIn);
            RenderWindow.Render();
        }

        private void btnReturnFromSup_Click(object sender, EventArgs e)
        {
            button2.Enabled = true;
            button16.Enabled = true;
            panel17.Visible = false;
            panel20.Visible = false;
            button2.BackColor = Color.White;
            button16.BackColor = Color.White;
        }

        private void btnReturnFromInf_Click(object sender, EventArgs e)
        {
            button2.Enabled = true;
            button16.Enabled = true;
            panel17.Visible = false;
            panel20.Visible = false;
            button2.BackColor = Color.White;
            button16.BackColor = Color.White;
        }

        private void btnConfirmAxisSup_Click(object sender, EventArgs e)
        {
            endplateAxesTransform = (vtkTransform)endplateAxesActor.GetUserTransform();
            finishEndplateRefSystem();
        }

        private void btnConfirmAxisInf_Click(object sender, EventArgs e)    
        {
            endplateAxesTransform = (vtkTransform)endplateAxesActor.GetUserTransform();
            finishEndplateRefSystem();
        }

        private void finishEndplateRefSystem()
        {
            btnFinalize.Enabled = true;
            panel17.Visible = false;
            panel20.Visible = false;
            button2.BackColor = Color.White;
            button16.BackColor = Color.White;
            ShowJointActors();

            ren1.RemoveActor(NormalActor);

            osimBodyProp._OsimGeometryPropertyList[0]._vtkActor.GetProperty().SetOpacity(0.1);
            parentBodyProp._OsimGeometryPropertyList[0]._vtkActor.GetProperty().SetOpacity(0.1);

            clipActor.GetProperty().SetOpacity(1);
            clipActorChild.GetProperty().SetOpacity(1);

            btnDefineJoint.Enabled = false;
            btnFinalize.PerformClick();

            RenderWindow.Render();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            btnConfirmAxisSup.Enabled = false;
            BtnSetEnplateRefChild.Enabled = false;


            btnDoneEndplateRefChild.Visible = true;
            btnDoneEndplateRefChild.Enabled = true;
          

            calculateEndplateCOM(childPolydata);

            MakeEndplateAxes();

            vtkBoxWidget2.SetProp3D(endplateAxesActor);
            vtkBoxWidget2.SetPlaceFactor(1.0);

            //boxWidget.TranslationEnabledOff();
            //boxWidget.SetHandleSize(0.0050);
            vtkBoxWidget2.HandlesOff();
            vtkBoxWidget2.TranslationEnabledOff();
            vtkBoxWidget2.OutlineCursorWiresOff();
            vtkBoxWidget2.OutlineFaceWiresOff();

            vtkBoxWidget2.RotationEnabledOn();
            vtkBoxWidget2.ScalingEnabledOff();
            vtkBoxWidget2.GetHandleProperty().FrontfaceCullingOff();
            vtkBoxWidget2.GetHandleProperty().BackfaceCullingOff();
            vtkBoxWidget2.PlaceWidget();

            vtkBoxWidget2.InteractionEvt += new vtkObject.vtkObjectEventHandler(ExecuteHandleEndplate);

            vtkBoxWidget2.SetInteractor(iren);
            vtkBoxWidget2.On();

            RenderWindow.Render();




        }

        private void button1_Click(object sender, EventArgs e)
        {
            btnConfirmAxisSup.Enabled = true;
            btnConfirmAxisSup.Visible = true;
            BtnSetEnplateRefChild.Enabled = true;

            vtkBoxWidget2.Off();
            endplateAxesTransform = (vtkTransform)endplateAxesActor.GetUserTransform();

            ren1.AddActor(clipActor);
            ren1.AddActor(osimBodyProp.osimJointProperty.axesActor);
            ren1.AddActor(osimBodyProp.assembly);

            BtnSetEnplateRefChild.Visible = true;
            BtnSetEnplateRefChild.Enabled = true;
            btnDoneEndplateRefChild.Visible = false;


        }

        private void btnSetJointPosition_Click(object sender, EventArgs e)
        {
            DisableAllJointButtons();
            panelJointPosition.Visible = true;
            btnFinished.BackColor = Color.LightGray;
            btnFinished.Enabled = false;

        }

        private void btnSetJointOrientation_Click(object sender, EventArgs e)
        {
            DisableAllJointButtons();
            panelJointPosition.Visible = false;
            panelJointOrientationStuff.Visible = true;
            btnFinished.BackColor = Color.LightGray;
            btnFinished.Enabled = false;
        }

        private void button10_Click(object sender, EventArgs e)
        {
            panelJointOrientationStuff.Visible = false;
            btnFinished.BackColor = Color.FromArgb(192, 255, 192);
            btnFinished.Enabled = true;
            EnableAllJointButtons();
        }

        private void btnFinished_Click(object sender, EventArgs e)
        {
            ren1.RemoveActor(cutActorChild);
            ren1.RemoveActor(clipActor);
            ren1.RemoveActor(clipActorParent);
            ren1.RemoveActor(COMendplateActor);
            ren1.RemoveActor(endplateAxesActor);
            ren1.RemoveActor(sourceActor);
            ren1.RemoveActor(solutionActor);
            ren1.RemoveActor(targetActor);
            ren1.RemoveActor(IVDactor);
            ren1.RemoveActor(followerAnterior);
            ren1.RemoveActor(followerRight);
            ren1.RemoveActor(followerSup);


            osimBodyProp._OsimGeometryPropertyList[0]._vtkActor.GetProperty().SetOpacity(1);
            osimBodyProp._OsimGeometryPropertyList[0]._vtkActor.GetProperty().SetColor(osimBodyProp.colorR, osimBodyProp.colorG, osimBodyProp.colorB);
            parentBodyProp._OsimGeometryPropertyList[0]._vtkActor.GetProperty().SetOpacity(0.20);
            parentBodyProp._OsimGeometryPropertyList[0]._vtkActor.GetProperty().SetColor(parentBodyProp.colorR, parentBodyProp.colorG, parentBodyProp.colorB);
            ShowJointActors();

            ResetCutters();
            ResetPanels();
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void cBjointVisual_CheckedChanged(object sender, EventArgs e)
        {
            if(cBjointVisual.Checked == true)
            {
                ShowJointActors();
            }
            else
            {
                HideJointActors();
            }
        }

        private void btnBasedOnEndplateLM_Click(object sender, EventArgs e)
        {
            //DMesh3 mesh = StandardMeshReader.ReadMesh(Appdata.TempDir + "\\" + "tempLandmarkFit.stl");
            // COMmeshCalculator(mesh, parentBodyProp);
            MakeMeshFromLMs();
            CalculatePointAverage(Appdata.TempDir + "\\" + "tempLandmarkFit.stl", parentBodyProp);
        }

        private void btnGrabAndDropPOSITION_Click(object sender, EventArgs e)
        {
            vtkBoxWidget.SetProp3D(osimBodyProp.osimJointProperty._jointActor);
            // vtkBoxWidget.SetTransform( (vtkTransform)osimBodyProp.osimJointProperty.axesActor.GetUserTransform());
            vtkBoxWidget.SetPlaceFactor(5);

            //boxWidget.TranslationEnabledOff();
            //boxWidget.SetHandleSize(0.0050);
            vtkBoxWidget.HandlesOff();
            vtkBoxWidget.OutlineCursorWiresOff();
            vtkBoxWidget.OutlineFaceWiresOff();
            vtkBoxWidget.RotationEnabledOff();
            //vtkBoxWidget.RotationEnabledOn();
            vtkBoxWidget.ScalingEnabledOff();
            vtkBoxWidget.GetHandleProperty().FrontfaceCullingOff();
            vtkBoxWidget.GetHandleProperty().BackfaceCullingOff();
            vtkBoxWidget.PlaceWidget();


            vtkBoxWidget.InteractionEvt += new vtkObject.vtkObjectEventHandler(ExecuteHandle);

            vtkBoxWidget.SetInteractor(iren);
            vtkBoxWidget.On();
            //vtkBoxWidget.AddObserver((uint)vtkCommand.EventIds.KeyPressEvent, boxCallback, 1.0f);
            RenderWindow.Render();

            btnGrabAndDropPOSITION.Enabled = false;
            btnConfirmDAndDPOSITION.Visible = true;
            btnConfirmDAndDPOSITION.Enabled = true;
           
          
        }

        private void btnConfirmDAndDPOSITION_Click(object sender, EventArgs e)
        {
            //The joint axes was repositioned (and reoriented) free from the model defenition. 
            //The aim is to use this transformation to recalculate the joint position under the assumption that the 
            //child and parent body do not move. (osimbodyprop of child and parent remain constant). 
            //The joint connecting them is defined in osimjointproperty as location and orientation in parent and child. These values need to be recalculated.
            //Thereafter the new values are transmitted to the osimmodel and to the vtkrendering objects.


            SimModelVisualization.SetJointPlacement(osimBodyProp.osimJointProperty.joint, osimBodyProp.transform, (vtkTransform)osimBodyProp.osimJointProperty.axesActor.GetUserTransform(), parentBodyProp.transform);

            ///SimModelVisualization.UpdateRenderer(ren1);



            osimBodyProp.osimJointProperty.ReadJoint();

            //osimBodyProp.osimJointProperty._vtkTransform = (vtkTransform)osimBodyProp.osimJointProperty.jointActor.GetUserTransform();



            //osimBodyProp.osimJointProperty.SetTransformationFromParent();

            osimBodyProp.osimJointProperty._vtkTransform = (vtkTransform)osimBodyProp.osimJointProperty.axesActor.GetUserTransform();
            // Thread.Sleep(5000);

            // axesActorAllIn.SetUserTransform((vtkTransform)osimBodyProp.osimJointProperty._jointActor.GetUserTransform());

            //SimModelVisualization.UpdateBodyJointTransform(parentBodyProp.body);
            //SimModelVisualization.UpdateBodyJointTransform(osimBodyProp.body);
            osimBodyProp.osimJointProperty.MakeVtkObject();

            // osimBodyProp.osimJointProperty.axesActor.SetUserTransform((vtkTransform)osimBodyProp.osimJointProperty.axesActor.GetUserTransform());



            //SimModelVisualization.UpdateBodyJointTransform(osimBodyProp.osimJointProperty.childBody);


            //ren1.Render();
            ren1.GetRenderWindow().Render();



            vtkBoxWidget.Off();
            btnConfirmDAndDPOSITION.Enabled = false;
            btnConfirmDAndDPOSITION.Visible = false;
            btnGrabAndDropPOSITION.Enabled = true;
            EnableAllJointButtons();
            panel18.Visible = false;
            btnFinished.BackColor = Color.FromArgb(192, 255, 192);
            btnFinished.Enabled = true;

        }

        private void btnManualPosOverwrite_Click(object sender, EventArgs e)
        {
            panel18.Visible = true;
            panel19.Visible = false;
            DisableAllJointButtons();
            btnFinished.BackColor = Color.LightGray;
            btnFinished.Enabled = false;
        }

        private void DisableAllJointButtons()
        {
            btnManualOrientationOverwrite.Enabled = false;
            btnSetJointPosition.Enabled = false;
            btnSetJointOrientation.Enabled = false;
            btnManualPosOverwrite.Enabled = false;

        }

        private void EnableAllJointButtons()
        {
            btnManualOrientationOverwrite.Enabled = true;
            btnSetJointPosition.Enabled = true;
            btnSetJointOrientation.Enabled = true;
            btnManualPosOverwrite.Enabled = true;

        }

        private void btnManualOrientationOverwrite_Click(object sender, EventArgs e)
        {
            panel18.Visible = false;
            panel19.Visible = true;
            DisableAllJointButtons();
            btnFinished.BackColor = Color.LightGray;
            btnFinished.Enabled = false;
        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            btnManualPosOverwrite.Enabled = true;
            btnManualOrientationOverwrite.Enabled = true;
            panel19.Visible = false;
            btnFinished.BackColor = Color.FromArgb(192, 255, 192);
            btnFinished.Enabled = true;
            EnableAllJointButtons();
        }

        private void button9_Click(object sender, EventArgs e)
        {
            vtkBoxWidget.SetProp3D(osimBodyProp.osimJointProperty._jointActor);
            // vtkBoxWidget.SetTransform( (vtkTransform)osimBodyProp.osimJointProperty.axesActor.GetUserTransform());
            vtkBoxWidget.SetPlaceFactor(5);

            //boxWidget.TranslationEnabledOff();
            //boxWidget.SetHandleSize(0.0050);
            vtkBoxWidget.HandlesOff();
            vtkBoxWidget.OutlineCursorWiresOff();
            vtkBoxWidget.OutlineFaceWiresOff();
            vtkBoxWidget.TranslationEnabledOff();
            vtkBoxWidget.RotationEnabledOn();
            vtkBoxWidget.ScalingEnabledOff();
            vtkBoxWidget.GetHandleProperty().FrontfaceCullingOff();
            vtkBoxWidget.GetHandleProperty().BackfaceCullingOff();
            vtkBoxWidget.PlaceWidget();


            vtkBoxWidget.InteractionEvt += new vtkObject.vtkObjectEventHandler(ExecuteHandle);

            vtkBoxWidget.SetInteractor(iren);
            vtkBoxWidget.On();
            //vtkBoxWidget.AddObserver((uint)vtkCommand.EventIds.KeyPressEvent, boxCallback, 1.0f);
            RenderWindow.Render();

            btnManualOrientationOverwrite.Enabled = false;
            btnConfirmDandDORIENTATION.Visible = true;
            btnConfirmDandDORIENTATION.Enabled = true;
           
        }

        private void RefFrameInfLM_Click(object sender, EventArgs e)
        {
            //CHECK IF ALL REQUIRED LANDMARKS HAVE BEEN INDICATED !!!!


            //This is de inferior Vertebra (== Parent body)
            //The Ant-Post axis is based on the landmarks. 
            //The y-normal is calulated based on the average normal vector of the endlplate landmarks (what if it is negative (concave/convex differences)?)
            calculateEndplateCOM(parentPolydata);
            MakeEndplateAxes();

            lblChooseAnOptionInf.Visible = true;
          

            btnConfirmAxisInf.Visible = false;
            btnConfirmAxisInf.Enabled = false;
            rbPostAnt.Visible = true;
            rbNormalLR.Visible = true;
            rbNormalPA.Visible = true;
            rbLeftRight.Visible = true;
            btnConfirmOptionLMbased.Visible = true;
        }


        private void btnConfirmOptionLMbased_Click(object sender, EventArgs e)
        {

            if (rbPostAnt.Checked)
            {
                calcLeftRight(false, "Inf");
                CalcPostAnt(true, "Inf");


            }

            if (rbLeftRight.Checked)
            {

                CalcPostAnt(false, "Inf");
                calcLeftRight(true, "Inf");

            }


            lblChooseAnOptionInf.Visible = false;

            btnConfirmAxisInf.Visible = true;
            btnConfirmAxisInf.Enabled = true;
            rbPostAnt.Visible = false;
            rbNormalLR.Visible = false;
            rbNormalPA.Visible = false;
            rbLeftRight.Visible = false;
            btnConfirmOptionLMbased.Visible = false;

            RenderWindow.Render();
        }

        private void cameraParallelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            vtkCamera camera = ren1.GetActiveCamera();
            camera.ParallelProjectionOn();
            //ren1.ResetCamera();
            RenderWindow.Render();

            
        }

        private void RefFrameSupLM_Click(object sender, EventArgs e)
        {
            //CHECK IF ALL REQUIRED LANDMARKS HAVE BEEN INDICATED !!!!


            //This is de inferior Vertebra (== Parent body)
            //The Ant-Post axis is based on the landmarks. 
            //The y-normal is calulated based on the average normal vector of the endlplate landmarks (what if it is negative (concave/convex differences)?)
            calculateEndplateCOM(childPolydata);
            MakeEndplateAxes();

            lblChooseAnOptionSup.Visible = true;


            btnConfirmAxisSup.Visible = false;
            btnConfirmAxisSup.Enabled = false;
            rbPostAntSup.Visible = true;
            rbNormalLRSup.Visible = true;
            rbNormalPASup.Visible = true;
            rbLeftRightSup.Visible = true;
            btnConfirmOptionLMbasedSup.Visible = true;
        }

        private void btnConfirmOptionLMbasedSup_Click(object sender, EventArgs e)
        {
            if (rbPostAntSup.Checked)
            {
                calcLeftRight(false, "Sup");
                CalcPostAnt(true, "Sup");


            }

            if (rbLeftRightSup.Checked)
            {

                CalcPostAnt(false, "Sup");
                calcLeftRight(true, "Sup");

            }


            lblChooseAnOptionSup.Visible = false;

            btnConfirmAxisSup.Visible = true;
            btnConfirmAxisSup.Enabled = true;
            rbPostAntSup.Visible = false;
            rbNormalLRSup.Visible = false;
            rbNormalPASup.Visible = false;
            rbLeftRightSup.Visible = false;
            btnConfirmOptionLMbasedSup.Visible = false;

            RenderWindow.Render();
        }

        private void nUpDownXpos_ValueChanged(object sender, EventArgs e)
        {
            UpdateCoordinatesJoint();
        }

        private void nUpDownYpos_ValueChanged(object sender, EventArgs e)
        {
            UpdateCoordinatesJoint();
        }

        private void nUpDownZpos_ValueChanged(object sender, EventArgs e)
        {
            UpdateCoordinatesJoint();
        }

        private void nUpDownXrot_ValueChanged(object sender, EventArgs e)
        {
            UpdateCoordinatesJoint();
        }

        private void nUpDownYrot_ValueChanged(object sender, EventArgs e)
        {
            UpdateCoordinatesJoint();
        }

        private void nUpDownZrot_ValueChanged(object sender, EventArgs e)
        {
            UpdateCoordinatesJoint();
        }

        private void rbRelativeDisplacement_CheckedChanged(object sender, EventArgs e)
        {
            if(rbRelativeDisplacement.Checked)
            {
                nUpDownXpos.Value = 0;
                nUpDownYpos.Value = 0;
                nUpDownZpos.Value = 0;  
                nUpDownXrot.Value = 0;
                nUpDownYrot.Value = 0;
                nUpDownZrot.Value = 0;

            }
        }

        private void rbAbsolute_CheckedChanged(object sender, EventArgs e)
        {
            if (rbAbsolute.Checked)
            {
                SetupInitialUpDownNumerics();

            }
           
        }

        private void SetupInitialUpDownNumerics()
        {
            
            nUpDownXpos.Value = Convert.ToInt32((InitialPosition[0] * 1000)); //THIS IS NOT CORRECT!!!! you will have round off errors !!
            nUpDownYpos.Value = Convert.ToInt32((InitialPosition[1] * 1000));
            nUpDownZpos.Value = Convert.ToInt32((InitialPosition[2] * 1000));
            nUpDownXrot.Value = Convert.ToInt32((InitialOrientation[0]));
            nUpDownYrot.Value = Convert.ToInt32((InitialOrientation[1]));
            nUpDownZrot.Value = Convert.ToInt32((InitialOrientation[2]));

        }

        private void btnShowCOM_Click(object sender, EventArgs e)
        {
            vtkSphereSource sphere = new vtkSphereSource();
            sphere.SetRadius(0.0050);

            vtkPolyDataMapper sphereMapper = vtkPolyDataMapper.New();
            sphereMapper.SetInputConnection(sphere.GetOutputPort());

            vtkActor vtkActor = new vtkActor();
            vtkActor.SetMapper(sphereMapper);

            vtkTransform d = new vtkTransform();

            d.SetInput(osimBodyProp.transform);
            d.PostMultiply();

            // d.RotateWXYZ(rotationWXYZ[0], rotationWXYZ[1], rotationWXYZ[2], rotationWXYZ[3]);
            d.Translate(osimBodyProp.mass_center[0], osimBodyProp.mass_center[1], osimBodyProp.mass_center[2]);

            osimBodyProp.osimJointProperty._jointActor.SetUserTransform(d);

            vtkActor.SetUserTransform(osimBodyProp.osimJointProperty._vtkTransform);
            
            ren1.AddActor(vtkActor);
            ren1.Render();

        }

        private void btnCalculatePelvisJoint_Click(object sender, EventArgs e)
        {
            /// Get LASI, RASI, LPSI and RPSI 

            try
            {
                System.Windows.Media.Media3D.Point3D point_LASI = new System.Windows.Media.Media3D.Point3D(0, 0, 0);
                System.Windows.Media.Media3D.Point3D point_RASI = new System.Windows.Media.Media3D.Point3D(0, 0, 0);
                System.Windows.Media.Media3D.Point3D point_RPSI = new System.Windows.Media.Media3D.Point3D(0, 0, 0);
                System.Windows.Media.Media3D.Point3D point_LPSI = new System.Windows.Media.Media3D.Point3D(0, 0, 0);

                bool point_LASI_found = false;
                bool point_RASI_found = false;
                bool point_LPSI_found = false;
                bool point_RPSI_found = false;


                //Clear DataGrid
                dgViewPelvisJoint.DataSource = null;
                dgViewPelvisJoint.Refresh();

                //Build SQL Command (=SQL select statement + Parameters)
                SqlCommand SQLcmd = new SqlCommand();

                string SQLselect;


                SQLselect = "SELECT LandmarkID, LandmarkName, LMoptionID, UserName FROM GeometryLandmarks where GeometryID = @GeometryID";
                SQLcmd.Parameters.AddWithValue("@GeometryID", currentGeomProp.geometryFileObject.GeometryNumber);

                //if (rbOnlyMine.Checked)
                //{
                SQLselect += "  AND (UserName = @UserName OR Username =@UserName2)";
                SQLcmd.Parameters.AddWithValue("@UserName", Appdata.globalUser._UserID);
                SQLcmd.Parameters.AddWithValue("@UserName2", Appdata.globalUser.FirstName + Appdata.globalUser.LastName); //this is for compatibility with the old way of def usernames.

                //  }

                SQLcmd.CommandText = SQLselect;

                //Set Database
                DataBase SQLDB = new DataBase(Appdata.SQLServer, Appdata.SQLDatabase, Appdata.SQLAuthSQL, Appdata.SQLUser, Appdata.SQLPassword);

                //Fill a dataset with selected records
                DataSet ds = new DataSet();
                SQLDB.ReadDataSet(SQLcmd, ref ds);

                // you can make the grid readonly.
                dgViewPelvisJoint.ReadOnly = true;
                dgViewPelvisJoint.DataSource = ds.Tables[0];
                // Resize the DataGridView columns to fit the newly loaded content.
                dgViewPelvisJoint.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.ColumnHeader);


                //Set Database
                SQLDB = new DataBase(Appdata.SQLServer, Appdata.SQLDatabase, Appdata.SQLAuthSQL, Appdata.SQLUser, Appdata.SQLPassword);

                //Check if rows selected
                int RowCount = dgViewPelvisJoint.Rows.Count;

                if (RowCount == 0)
                    return;

                for (int i = 0; i < RowCount; i++)
                {
                    //Get row of clicked cell
                    DataGridViewRow row = dgViewPelvisJoint.Rows[dgViewPelvisJoint.Rows[i].Index];
                    //Get column AcquisitionNumber
                    DataGridViewCell cell = row.Cells["LandmarkID"];
                    //create AcquisitionObject to be deleted

                    if (cell.Value != null)
                    {
                        Landmark SelectedLandmark = new Landmark(SQLDB, Subject, currentGeomProp.geometryFileObject.GeometryNumber, Convert.ToInt32(cell.Value), Appdata);
                        SelectedLandmark.bodyProp = osimBodyProp;
                        double[] p = new double[] { SelectedLandmark.coorX, SelectedLandmark.coorY, SelectedLandmark.coorZ };
                        MakeLMactor(p, SelectedLandmark, osimBodyProp);
                        //LoadedLandmarks.Add(SelectedLandmark);

                        switch (SelectedLandmark.LandmarkName)
                        {
                            case "LASI":
                                point_LASI = new System.Windows.Media.Media3D.Point3D(SelectedLandmark.transform.GetPosition()[0], SelectedLandmark.transform.GetPosition()[1], SelectedLandmark.transform.GetPosition()[2]);
                                point_LASI_found = true;
                                break;
                            case "RASI":
                                point_RASI = new System.Windows.Media.Media3D.Point3D(SelectedLandmark.transform.GetPosition()[0], SelectedLandmark.transform.GetPosition()[1], SelectedLandmark.transform.GetPosition()[2]);
                                point_RASI_found = true;
                                break;
                            case "LPSI":
                                point_LPSI = new System.Windows.Media.Media3D.Point3D(SelectedLandmark.transform.GetPosition()[0], SelectedLandmark.transform.GetPosition()[1], SelectedLandmark.transform.GetPosition()[2]);
                                point_LPSI_found = true;
                                break;
                            case "RPSI":
                                point_RPSI = new System.Windows.Media.Media3D.Point3D(SelectedLandmark.transform.GetPosition()[0], SelectedLandmark.transform.GetPosition()[1], SelectedLandmark.transform.GetPosition()[2]);
                                point_RPSI_found = true;
                                break;
                            default:
                                Console.WriteLine("Default case");
                                break;
                        }
                    }

                }

                if(!point_LASI_found || !point_RASI_found || !point_LPSI_found || !point_RPSI_found)
                {

                    MessageBox.Show("Could not define the pelvic joint. Check if you have defined the following landmarks on this geometry: LASI, RASI, LPSI and RPSI.", "error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                // Calculate normals according to ISB

                vtkMatrix4x4 vtkMatrix = CalculatePelvisMatrix(point_LASI, point_RASI, point_LPSI, point_RPSI);

                vtkTransform transform = new vtkTransform();
                transform.PostMultiply();
                transform.SetMatrix(vtkMatrix);
                transform.Update();


                //axesThorax.SetUserTransform(transform);

                double[] angle = transform.GetOrientation();
                double posX = (point_LASI.X + point_RASI.X + point_LPSI.X + point_RPSI.X) / 4;
                double posY = (point_LASI.Y + point_RASI.Y + point_LPSI.Y + point_RPSI.Y) / 4;
                double posZ = (point_LASI.Z + point_RASI.Z + point_LPSI.Z + point_RPSI.Z) / 4;


                // SetJointAxisActorManually(InitialPosition[0],0, InitialPosition[1], 0, InitialPosition[2], 0, angle[0], 0, angle[1], 0, angle[2],0);
                SetJointAxisActorManually(posX, 0, posY, 0, posZ, 0, angle[0], 0, angle[1], 0, angle[2], 0);
                RenderWindow.Render();
                PushJointUpdateToOpenSim();







                MessageBox.Show("Done", "DONE", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch
            {
                MessageBox.Show("Could not define the pelvic joint. Check if you have defined the following landmarks on this geometry: LASI, RASI, LPSI and RPSI.", "error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }

            // Set Sacrum and Pelvis joint to calculated orientations.
        }

        private vtkMatrix4x4 CalculatePelvisMatrix(System.Windows.Media.Media3D.Point3D point_LASI, System.Windows.Media.Media3D.Point3D point_RASI, System.Windows.Media.Media3D.Point3D point_LPSI, System.Windows.Media.Media3D.Point3D point_RPSI)
            {
            //    System.Windows.Media.Media3D.Point3D point_LASI = new System.Windows.Media.Media3D.Point3D(Convert.ToDouble(DtLASI.Rows[i]["X"]), Convert.ToDouble(DtLASI.Rows[i]["Y"]), Convert.ToDouble(DtLASI.Rows[i]["Z"]));
            //System.Windows.Media.Media3D.Point3D point_RASI = new System.Windows.Media.Media3D.Point3D(Convert.ToDouble(DtRASI.Rows[i]["X"]), Convert.ToDouble(DtRASI.Rows[i]["Y"]), Convert.ToDouble(DtRASI.Rows[i]["Z"]));
            //System.Windows.Media.Media3D.Point3D point_RPSI = new System.Windows.Media.Media3D.Point3D(Convert.ToDouble(DtRPSI.Rows[i]["X"]), Convert.ToDouble(DtRPSI.Rows[i]["Y"]), Convert.ToDouble(DtRPSI.Rows[i]["Z"]));
            //System.Windows.Media.Media3D.Point3D point_LPSI = new System.Windows.Media.Media3D.Point3D(Convert.ToDouble(DtLPSI.Rows[i]["X"]), Convert.ToDouble(DtLPSI.Rows[i]["Y"]), Convert.ToDouble(DtLPSI.Rows[i]["Z"]));

            //System.Windows.Media.Media3D.Point3D point_LASI = new System.Windows.Media.Media3D.Point3D(Convert.ToDouble(DtLASI.Rows[i]["X"]), Convert.ToDouble(DtLASI.Rows[i]["Z"]), Convert.ToDouble(DtLASI.Rows[i]["Y"]));
            //System.Windows.Media.Media3D.Point3D point_RASI = new System.Windows.Media.Media3D.Point3D(Convert.ToDouble(DtRASI.Rows[i]["X"]), Convert.ToDouble(DtRASI.Rows[i]["Z"]), Convert.ToDouble(DtRASI.Rows[i]["Y"]));
            //System.Windows.Media.Media3D.Point3D point_RPSI = new System.Windows.Media.Media3D.Point3D(Convert.ToDouble(DtRPSI.Rows[i]["X"]), Convert.ToDouble(DtRPSI.Rows[i]["Z"]), Convert.ToDouble(DtRPSI.Rows[i]["Y"]));
            //System.Windows.Media.Media3D.Point3D point_LPSI = new System.Windows.Media.Media3D.Point3D(Convert.ToDouble(DtLPSI.Rows[i]["X"]), Convert.ToDouble(DtLPSI.Rows[i]["Z"]), Convert.ToDouble(DtLPSI.Rows[i]["Y"]));


            //SphereactorLASI.SetPosition(point_LASI.X / 1000, point_LASI.Y / 1000, point_LASI.Z / 1000);
            //SphereactorRASI.SetPosition(point_RASI.X / 1000, point_RASI.Y / 1000, point_RASI.Z / 1000);
            //SphereactorLPSI.SetPosition(point_LPSI.X / 1000, point_LPSI.Y / 1000, point_LPSI.Z / 1000);
            //SphereactorRPSI.SetPosition(point_RPSI.X / 1000, point_RPSI.Y / 1000, point_RPSI.Z / 1000);



            Vector3D vectorML = point_RASI - point_LASI;
            Vector3D vectorPA = point_RASI - point_RPSI;
            Vector3D vectorML_post = point_RPSI - point_LPSI;

            vectorML.Normalize();
            vectorPA.Normalize();
            vectorML_post.Normalize();

            Vector3D crossProductVert = Vector3D.CrossProduct(vectorML_post, vectorPA);

            crossProductVert.Normalize();
            Vector3D crossProductPA = Vector3D.CrossProduct(crossProductVert, vectorML);
            crossProductPA.Normalize();

            // Matrix3D matrix = new Matrix3D(crossProductPA.X, crossProductPA.Y, crossProductPA.Z, 0, crossProductVert.X, crossProductVert.Y, crossProductVert.Z, 0, vectorML_post.X, vectorML_post.Y, vectorML_post.Z, 0, 0, 0, 0, 1);





            vtkMatrix4x4 vtkMatrix = new vtkMatrix4x4();
            vtkMatrix.SetElement(0, 0, crossProductPA.X);
            vtkMatrix.SetElement(1, 0, crossProductPA.Y);
            vtkMatrix.SetElement(2, 0, crossProductPA.Z);
            vtkMatrix.SetElement(3, 0, 0);

            vtkMatrix.SetElement(0, 1, crossProductVert.X);
            vtkMatrix.SetElement(1, 1, crossProductVert.Y);
            vtkMatrix.SetElement(2, 1, crossProductVert.Z);
            vtkMatrix.SetElement(3, 1, 0);

            vtkMatrix.SetElement(0, 2, vectorML_post.X);
            vtkMatrix.SetElement(1, 2, vectorML_post.Y);
            vtkMatrix.SetElement(2, 2, vectorML_post.Z);
            vtkMatrix.SetElement(3, 2, 0);

            vtkMatrix.SetElement(0, 3, 0);  //Linear Translation
            vtkMatrix.SetElement(1, 3, 0);
            vtkMatrix.SetElement(2, 3, 0);
            vtkMatrix.SetElement(3, 3, 1);


            return vtkMatrix;
        }

        private void rbOnlyMine_CheckedChanged(object sender, EventArgs e)
        {
            if (currentGeomProp.geometryFileObject != null)
            {
                refreshLandmarks(currentGeomProp.geometryFileObject, dataGridViewLMchild);
            }
            if (parentBodyProp._OsimGeometryPropertyList[0].geometryFileObject != null)
            {
                refreshLandmarks(parentBodyProp._OsimGeometryPropertyList[0].geometryFileObject, dataGridViewLMparent);
            }
        }

        private void rbAll_CheckedChanged(object sender, EventArgs e)
        {
            if (currentGeomProp.geometryFileObject != null)
            {
                refreshLandmarks(currentGeomProp.geometryFileObject, dataGridViewLMchild);
            }
            if (parentBodyProp._OsimGeometryPropertyList[0].geometryFileObject != null)
            {
                refreshLandmarks(parentBodyProp._OsimGeometryPropertyList[0].geometryFileObject, dataGridViewLMparent);
            }
        }

        private void btnchangebodyDefintion_Click(object sender, EventArgs e)
        {
            //Transform tst = osimBodyProp._OsimGeometryPropertyList[0].displayGeometry.getTransform();
            ////tst.R().convertRotationToBodyFixedXYZ().set(0,0);
            ////tst.R().convertRotationToBodyFixedXYZ().set(1, 5);
            ////tst.R().convertRotationToBodyFixedXYZ().set(2, 0);

            //tst.R().setRotationFromAngleAboutX(0);
            //tst.R().setRotationFromAngleAboutY(50);
            //tst.R().setRotationFromAngleAboutZ(0);
            ////tst.T().set(0, 0);
            //tst.T().set(1, 1);
            //tst.T().set(2, 0);
            Vec3 p = new Vec3(0, 1, 0);
            CoordinateAxis coordinateAxisX = new CoordinateAxis(0);
            CoordinateAxis coordinateAxisY = new CoordinateAxis(1);
            CoordinateAxis coordinateAxisZ = new CoordinateAxis(2);

            Rotation R = new Rotation(BodyOrSpaceType.BodyRotationSequence, 10, coordinateAxisX, 0, coordinateAxisY, 0, coordinateAxisZ);
            Transform tst = new Transform();
            tst.set(R, p);


            osimBodyProp._OsimGeometryPropertyList[0].displayGeometry.setTransform(tst);

            //OpenSim.ArrayDouble ArrayDouble = new ArrayDouble();
            //osimBodyProp._OsimGeometryPropertyList[0].displayGeometry.setTransform(tst);
            //osimBodyProp._OsimGeometryPropertyList[0].displayGeometry.getRotationsAndTranslationsAsArray6(ArrayDouble);


        }

        public void setGeometryInternalTransform(double Tx, double Ty, double Tz, double Rx, double Ry, double Rz) //ROTATIONS IN RADIAN!!
        {
            
            Vec3 p = new Vec3(Tx, Ty, Tz);
            CoordinateAxis coordinateAxisX = new CoordinateAxis(0);
            CoordinateAxis coordinateAxisY = new CoordinateAxis(1);
            CoordinateAxis coordinateAxisZ = new CoordinateAxis(2);

            Rotation R = new Rotation(BodyOrSpaceType.BodyRotationSequence, Rx , coordinateAxisX, Ry, coordinateAxisY, Rz, coordinateAxisZ);
            Transform tst = new Transform();
            tst.set(R, p);


            osimBodyProp.body.getDisplayer().setTransform(tst);
            model.updateDisplayer(si);

                //_OsimGeometryPropertyList[0].displayGeometry.setTransform(tst);


        }
    }

}
