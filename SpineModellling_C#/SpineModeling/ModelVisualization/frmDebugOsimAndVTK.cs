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
using TOD;


namespace SpineAnalyzer.ModelVisualization
{
    public partial class frmDebugOsimAndVTK : Form
    {
        #region Declarations
        public vtkRenderWindow RenderWindow;
        public vtkRenderer ren1;
        public RenderWindowControl renderWindowControl1 = new RenderWindowControl();
        public vtkRenderWindowInteractor iren;
        public SimModelVisualization SimModelVisualization = new SimModelVisualization();
        public AppData AppData;


        List<TreeNode> checkedNodes = new List<TreeNode>();
        private ContextMenuStrip docMenu;
        vtkPropPicker propPicker = vtkPropPicker.New();
        private int previousPositionX = 0;
        private int previousPositionY = 0;
        private int numberOfClicks = 0;
        private int resetPixelDistance = 5;
        private vtkAssembly lastPickedAssembly = vtkAssembly.New();
        public OsimBodyProperty selectedBodyProperty = new OsimBodyProperty();
        #endregion

        #region Load Methods
        public frmDebugOsimAndVTK()
        {
            InitializeComponent();
        }

        private void frmDebugOsimAndVTK_Load(object sender, EventArgs e)
        {
            //Load the VTK renderer in the window
            this.renderWindowControl1.AddTestActors = false;
            this.renderWindowControl1.Name = "renderWindowControl1";
            this.renderWindowControl1.Size = new System.Drawing.Size(552, 413);
            this.renderWindowControl1.TabIndex = 1;
            this.renderWindowControl1.TestText = null;
            this.tableLayoutPanel1.Controls.Add(this.renderWindowControl1, 0, 0);
            this.renderWindowControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            renderWindowControl1_Load();
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

            iren.SetPicker(propPicker);
            iren.LeftButtonPressEvt += new vtkObject.vtkObjectEventHandler(OnLeftButtonDown);
            //SimModelVisualization.AddReferenceCubeToRenderer(ren1, iren);   //THIS MAY BE A PROBLEM!!
        }

        public void OnLeftButtonDown(vtkObject sender, vtkObjectEventArgs e)
        {
            numberOfClicks++;

            int[] clickPos = iren.GetEventPosition();

            int xdist = clickPos[0] - previousPositionX;
            int ydist = clickPos[1] - previousPositionY;

            previousPositionX = clickPos[0];
            previousPositionY = clickPos[1];

            int moveDistance = (int)Math.Sqrt((double)(xdist * xdist + ydist * ydist));

            // Reset numClicks - If mouse moved further than resetPixelDistance
            if (moveDistance > resetPixelDistance)
            {
                numberOfClicks = 1;
            }


            if (numberOfClicks == 2)
            {
                numberOfClicks = 0;
                richTextBox1.Text += Environment.NewLine;
                richTextBox1.AppendText("Clicked twice (Left button) ");


                // Pick from this location.
                propPicker.Pick(clickPos[0], clickPos[1], 0, ren1);

               // vtkActorCollection collection = vtkActorCollection.New();
                vtkAssembly assembly = vtkAssembly.New();
                assembly = propPicker.GetAssembly();

                vtkActor actor = propPicker.GetActor();

                SimModelVisualization.UnhighlightEverything();

                if (assembly != null)
                {
                    int index = SimModelVisualization.bodyPropertyList.FindIndex(x => x.assembly == assembly);

                    selectedBodyProperty = SimModelVisualization.bodyPropertyList[index];
                    propertyGrid1.SelectedObject = selectedBodyProperty;
                    TreeNode[] treeNodeList = treeView1.Nodes.Find(selectedBodyProperty.objectName, true);
                    treeView1.SelectedNode = treeNodeList[0];

                    treeNodeList[0].TreeView.Select();
                    treeNodeList[0].TreeView.Focus();

                    selectedBodyProperty.HighlightBody();

                   // ren1.Render();
                    RenderWindow.Render();
                }

                if (actor != null)
                {
                    int index = SimModelVisualization.markerPropertyList.FindIndex(x => x.markerActor == actor);

                    OsimMakerProperty markerProp = SimModelVisualization.markerPropertyList[index];
                    //selectedBodyProperty = new OsimBodyProperty(); //Just set it empty 
                    propertyGrid1.SelectedObject = markerProp;
                    TreeNode[] treeNodeList = treeView1.Nodes.Find(markerProp.objectName, true);
                    treeView1.SelectedNode = treeNodeList[0];
                    treeNodeList[0].TreeView.Select();
                    treeNodeList[0].TreeView.Focus();
                    markerProp.HighlightMarker();

                   // ren1.Render();
                    RenderWindow.Render();
                }
            }
        }

        private void btnLoadModel_Click(object sender, EventArgs e)
        {

            // SET SOME OF THE PROPERTIES OF THE SimModelVisualization class.
            //SimModelVisualization.modelFile = @"C:\Users\Thomas\Desktop\gait2392_simbody.osim"; // "C:\\Users\\Thomas\\Desktop\\GENERIC_Model_original.osim";
            //SimModelVisualization.geometryDir = @"C:\OpenSim 3.3\Geometry";

            //SimModelVisualization.modelFile = @"C:\Users\Thomas\Desktop\Overbergh_Model_v1.4.osim";
            //SimModelVisualization.geometryDir = @"C:\OpenSim 3.3\GeometrySpine";

            SimModelVisualization.modelFile = AppData.ModelFile;
            SimModelVisualization.geometryDir = AppData.GeometryDir;
            SimModelVisualization.renderer = ren1;

            //Call methods in the simModelVisualization class.
            SimModelVisualization.ReadModel();                      //Read the Model. This adds a Model object as property to the object SimModelVisualization.
            richTextBox1.AppendText("The model: " + SimModelVisualization.osimModel.getName() + " from file: " + SimModelVisualization.modelFile + " was loaded.");

            SimModelVisualization.Model2Treeview(treeView1);        //Fill the TreeView
            richTextBox1.Text += Environment.NewLine;

            richTextBox1.AppendText("TreeView succesfully filled.");
            SimModelVisualization.InitializeModelInRen(ren1);       //Fill the VTK renderer
            richTextBox1.Text += Environment.NewLine;

            richTextBox1.AppendText("Model was initialized in Renderer.");
            richTextBox1.Text += Environment.NewLine;

            //ren1.Render();
            RenderWindow.Render();

            //Toolstrip stuff (ignore)
            toolStripProgressBar1.Value = 0;
            toolStripStatusLabel1.Text = "Visualizing";
            toolStripProgressBar1.Value = 50;
            toolStripProgressBar1.Value = 100;
            toolStripStatusLabel1.Text = "Done";
            btnLoadModel.Enabled = false;
        }
        
        #endregion

        #region VTKmethods (ignore)
        private void SetCameraParallel()
        {
            vtkCamera camera = ren1.GetActiveCamera();
            camera.ParallelProjectionOn();
            ren1.ResetCamera();
            RenderWindow.Render();

        }
        private void SetCameraOrthogonal()
        {
            vtkCamera camera = ren1.GetActiveCamera();
            camera.ParallelProjectionOff();
            ren1.ResetCamera();
            RenderWindow.Render();

        }
        private void AllignCameraLateral()
        {
            vtkCamera camera = ren1.GetActiveCamera();
            camera.SetFocalPoint(0, 1, 0);
            camera.SetPosition(0, 1, -1.5);
            camera.ComputeViewPlaneNormal();
            camera.SetViewUp(0, 1, 0);
            camera.OrthogonalizeViewUp();
            ren1.SetActiveCamera(camera);
            ren1.ResetCamera();
            RenderWindow.Render();
        }
        private void AllignCameraFrontal()
        {
            vtkCamera camera = ren1.GetActiveCamera();
            camera.SetFocalPoint(0, 1, 0);
            camera.SetPosition(-1.5, 1, 0);
            camera.ComputeViewPlaneNormal();
            camera.SetViewUp(0, 1, 0);
            camera.OrthogonalizeViewUp();
            ren1.SetActiveCamera(camera);
            ren1.ResetCamera();
            RenderWindow.Render();
        }
        private void CaptureImage()
        {
            vtkWindowToImageFilter w2i = new vtkWindowToImageFilter();
            vtkTIFFWriter writer = new vtkTIFFWriter();

            w2i.SetInput(RenderWindow);
            w2i.Update();
            writer.SetInputConnection(w2i.GetOutputPort());
            writer.SetFileName(@"C:\Users\Thomas\Desktop\testsreenshot.tiff");
            RenderWindow.Render();
            writer.Write();

            writer.Dispose();
            w2i.Dispose();

        }
        private void cameraParallelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetCameraParallel();
        }
        private void cameraOrthogonalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetCameraOrthogonal();
        }
        private void makeScreenshotToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CaptureImage();
        }
        private void saveModelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string loc = @"C:\Users\Thomas\Desktop\savedModel.osim";
            SimModelVisualization.PrintModel(loc);
            richTextBox1.AppendText("The model was saved to your desktop." + " (" + loc + ")");
            richTextBox1.Text += Environment.NewLine;
        }
        private void allignFrontalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AllignCameraFrontal();
        }
        private void allignLateralToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AllignCameraLateral();
        }
        #endregion

        #region TreeView Interaction Methods
        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            //Go the the parent node which is defining the type of object. 
         
            TreeNode tn = e.Node;
            int nodeLevel = tn.Level;
            TreeNode tnParent = e.Node;
            SimModelVisualization.UnhighlightEverything();

            string selectedNodeText = tn.Text;
            if (nodeLevel == 0)   //Be carefull when multiple models are loaded.
            {
                Model model = SimModelVisualization.osimModel;

                OsimModelProperty osimProp = new OsimModelProperty();
                osimProp.ReadModelProperties(model);
                propertyGrid1.SelectedObject = osimProp;

            }
            else
            {
                while (nodeLevel != 1)
                {
                    tnParent = tnParent.Parent;
                    nodeLevel = tnParent.Level;
                }
                string objectType = tnParent.Text;
                richTextBox1.Text += objectType;
                richTextBox1.Text += Environment.NewLine;


                if (objectType == "Markers" && e.Node.Level != 1 && e.Node.Level != 2)
                {
                    Marker marker = SimModelVisualization.osimModel.getMarkerSet().get(selectedNodeText);

                    int index = SimModelVisualization.markerPropertyList.FindIndex(x => x.objectName == selectedNodeText);

                    OsimMakerProperty markerProp =  SimModelVisualization.markerPropertyList[index];
                    //markerProp.vtkRenderwindow = RenderWindow;
                    propertyGrid1.SelectedObject = markerProp;
                    tn.ContextMenuStrip = SimModelVisualization.markerPropertyList[index].contextMenuStrip;
                    markerProp.HighlightMarker();
                }

                if (objectType == "Bodies" && e.Node.Level != 1 && e.Node.Level != 2)
                {
                    Body body = SimModelVisualization.osimModel.getBodySet().get(selectedNodeText);
                    int index = SimModelVisualization.bodyPropertyList.FindIndex(x => x.objectName == selectedNodeText);

                    OsimBodyProperty bodyProp = SimModelVisualization.bodyPropertyList[index];
                    bodyProp.vtkRenderwindow = RenderWindow;
                    propertyGrid1.SelectedObject = bodyProp;
                    tn.ContextMenuStrip = SimModelVisualization.bodyPropertyList[index].contextMenuStrip;
                    bodyProp.HighlightBody();

                }
                if (objectType == "Forces" && e.Node.Level != 1 && e.Node.Level != 2)
                {
                    Force force = SimModelVisualization.osimModel.getForceSet().get(selectedNodeText);
                    int index = SimModelVisualization.forcePropertyList.FindIndex(x => x.objectName == selectedNodeText);

                    OsimForceProperty forceProp = SimModelVisualization.forcePropertyList[index];
                    forceProp.vtkRenderwindow = RenderWindow;
                    propertyGrid1.SelectedObject = forceProp;
                    tn.ContextMenuStrip = SimModelVisualization.forcePropertyList[index].contextMenuStrip;
                    forceProp.HighlightBody();

                }


                if (objectType == "Joints" && e.Node.Level == 2)
                {
                    if (selectedNodeText == "WorldFrameFixed")
                    { return; }
                    Joint joint = SimModelVisualization.osimModel.getJointSet().get(selectedNodeText);
                    joint.getName();
                    Body body = joint.getBody();

                    int index = SimModelVisualization.bodyPropertyList.FindIndex(x => x.objectName == body.getName());

                    OsimBodyProperty bodyProp = SimModelVisualization.bodyPropertyList[index];
                    bodyProp.vtkRenderwindow = RenderWindow;
                    propertyGrid1.SelectedObject = bodyProp.osimJointProperty;
                    // UpdateCurrentBodyProperty(bodyProp);
                    tn.ContextMenuStrip = SimModelVisualization.bodyPropertyList[index].osimJointProperty.contextMenuStrip;
                    //bodyProp.HighlightBody();
                }
            }

            RenderWindow.Render();

        }

        private void btnDeleteNode_Click(object sender, EventArgs e)
        {
            Open2DFile();
        }

        vtkImagePlaneWidget planeWidgetX;
        vtkImagePlaneWidget planeWidgetY;
        vtkImagePlaneWidget planeWidgetZ;
        public void Open2DFile()
        {
            string dir = @"C:\ThomasOverbergh\DatabaseFiles\3333\MRI\MRI_002";
            ////FIXED ITEM TO THE SCREEN
            //vtkAnnotatedCubeActor cube = vtkAnnotatedCubeActor.New();
            //cube.SetXPlusFaceText("R");
            //cube.SetXMinusFaceText("L");
            //cube.SetYPlusFaceText("P");
            //cube.SetYMinusFaceText("A");
            //cube.SetZPlusFaceText("I");
            //cube.SetZMinusFaceText("S");
            //cube.SetXFaceTextRotation(180);
            //cube.SetYFaceTextRotation(180);
            //cube.SetZFaceTextRotation(-90);
            //cube.SetFaceTextScale(0.65);
            //cube.GetCubeProperty().SetColor(0.5, 0.8, 0.6);

            //vtkAxesActor axes = vtkAxesActor.New();
            //axes.SetShaftTypeToCylinder();
            //axes.SetXAxisLabelText("X");
            //axes.SetYAxisLabelText("Y");
            //axes.SetZAxisLabelText("Z");
            //axes.SetTotalLength(1.5, 1.5, 1.5);

            //vtkTextProperty tprop = vtkTextProperty.New();
            //tprop.ItalicOn();
            //tprop.ShadowOn();
            //tprop.SetFontFamilyToArial();

            //axes.GetXAxisCaptionActor2D().SetCaptionTextProperty(tprop);
            //axes.GetYAxisCaptionActor2D().SetCaptionTextProperty(tprop);
            //axes.GetZAxisCaptionActor2D().SetCaptionTextProperty(tprop);

            //vtkPropAssembly assembly = vtkPropAssembly.New();
            //assembly.AddPart(axes);
            //assembly.AddPart(cube);

            //vtkOrientationMarkerWidget marker = vtkOrientationMarkerWidget.New();
            //marker.SetOutlineColor(0.93, 0.57, 0.13);
            //marker.SetOrientationMarker(assembly);
            //marker.SetViewport(0, 0, 0.2, 0.2);
            //marker.SetInteractor(iren);
            //marker.SetEnabled(1);
            //marker.InteractiveOff();


            // The actual dicom reader
            vtkDICOMImageReader dicomreader = new vtkDICOMImageReader();
            dicomreader.SetDirectoryName(dir);
            dicomreader.UpdateWholeExtent();
            dicomreader.Update();
            dicomreader.GetDataExtent();
            
            double [] arguments =  dicomreader.GetDataSpacing(); 
            dicomreader.SetDataSpacing(arguments[0]/1000, arguments[1]/1000, arguments[2]/ 1000);
            dicomreader.GetFileDimensionality();
            dicomreader.GetHeight();
            dicomreader.Update();
            dicomreader.GetPixelRepresentation();
            dicomreader.GetWidth();
            int[] dataExtent = dicomreader.GetDataExtent();
            double[] pixSpacing = dicomreader.GetPixelSpacing();

            int[] dims = new int[3];
            dims = dicomreader.GetOutput().GetDimensions();
            //trackBarX.SetRange(0, dims[0]);
            //trackBarX.Value = dims[0] / 2;
            //trackBarY.SetRange(0, dims[1]);
            //trackBarY.Value = dims[1] / 2;
            //trackBarZ.SetRange(0, dims[2]);
            //trackBarZ.Value = dims[2] / 2;

            vtkImageCast readerImageCast = vtkImageCast.New();
            readerImageCast.SetInput((vtkDataObject)dicomreader.GetOutput());
            //readerImageCast.SetInputData((vtkDataObject)dicomreader.GetOutput());   //VTK_Version6
            //readerImageCast.SetOutputScalarTypeToUnsignedChar(); 
            readerImageCast.ClampOverflowOn();
            readerImageCast.GetOutput().SetSpacing(pixSpacing[0] / 1000, pixSpacing[1] / 1000, pixSpacing[2] / 1000);
            readerImageCast.Update();
            
            //The shared picker enables us to use 3 planes at one time
            //and gets the picking order right
            vtkCellPicker picker = new vtkCellPicker();
            picker.SetTolerance(0.005);

            // The 3 image plane widgets are used to probe the dataset.
            planeWidgetX = new vtkImagePlaneWidget();
            planeWidgetX.DisplayTextOn();
            planeWidgetX.SetInput(readerImageCast.GetOutput());
            //planeWidgetX.SetInputConnection(readerImageCast.GetOutputPort());  //VTK_version6
            planeWidgetX.SetPlaneOrientationToXAxes();
            planeWidgetX.SetKeyPressActivationValue((sbyte)'x');
            planeWidgetX.SetSliceIndex(dims[0] / 2);
            planeWidgetX.SetPicker(picker);
            
            vtkProperty prop1 = planeWidgetX.GetPlaneProperty();
            prop1.SetColor(1, 0, 0);

            planeWidgetY = new vtkImagePlaneWidget();
            planeWidgetY.DisplayTextOn();
            planeWidgetY.SetInput(readerImageCast.GetOutput());
            //planeWidgetY.SetInputConnection(readerImageCast.GetOutputPort());   //VTK_version6
            planeWidgetY.SetKeyPressActivationValue((sbyte)'y');
            planeWidgetY.SetPlaneOrientationToYAxes();
            planeWidgetY.SetSliceIndex(dims[1] / 2);
            planeWidgetY.SetPicker(picker);
            vtkProperty prop2 = planeWidgetY.GetPlaneProperty();
            prop2.SetColor(1, 1, 0);
            planeWidgetY.SetLookupTable(planeWidgetX.GetLookupTable());

            //for the z-slice, turn off texture interpolation:
            //interpolation is now nearest neighbour, to demonstrate
            //cross-hair cursor snapping to pixel centers
            planeWidgetZ = new vtkImagePlaneWidget();
            planeWidgetZ.DisplayTextOn();
            planeWidgetZ.SetInput(readerImageCast.GetOutput());
            //planeWidgetZ.SetInputConnection(readerImageCast.GetOutputPort());   //VTK_version6
            planeWidgetZ.SetPlaneOrientationToZAxes();
            planeWidgetZ.SetKeyPressActivationValue((sbyte)'z');
            planeWidgetZ.SetSliceIndex(dims[2] / 2);
            planeWidgetZ.SetPicker(picker);
            vtkProperty prop3 = planeWidgetZ.GetPlaneProperty();
            prop3.SetColor(0, 0, 1);
            planeWidgetZ.SetLookupTable(planeWidgetX.GetLookupTable());


            planeWidgetX.SetInteractor(iren);
            planeWidgetX.On();
            planeWidgetY.SetInteractor(iren);
            planeWidgetY.On();
            planeWidgetZ.SetInteractor(iren);
            planeWidgetZ.On();

            ren1.ResetCamera();
            RenderWindow.Render();

            iren.Initialize();
            //iren.Start();
            RenderWindow.Render();
            //rbLinearInterpolation.PerformClick();

            return;
        }


        private void RemoveCheckedNodes(TreeNodeCollection nodes)
        {
            foreach (TreeNode node in nodes)
            {
                if (node.Checked)
                {
                    checkedNodes.Add(node);
                }
                else
                {
                    RemoveCheckedNodes(node.Nodes);
                }
            }
            foreach (TreeNode checkedNode in checkedNodes)
            {
                nodes.Remove(checkedNode);
            }

        }
        #endregion

        #region ToolStripMenu Stuff
        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
           
        }

        private void statusStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }
        #endregion

        #region Interaction methods
        public void btnRotX_Click(object sender, EventArgs e)
        {
            if (selectedBodyProperty.body == null)
            { return; }
            SimModelVisualization.BodyRotateX(selectedBodyProperty.body, 10);
            
            //ren1.Render();
            RenderWindow.Render();
        }
        private void btnRotY_Click(object sender, EventArgs e)
        {
            if (selectedBodyProperty.body == null)
            { return; }
            SimModelVisualization.BodyRotateY(selectedBodyProperty.body, 10);

            //ren1.Render();
            RenderWindow.Render();
        }
        private void btnRotZ_Click(object sender, EventArgs e)
        {
            if (selectedBodyProperty.body == null)
            { return; }
            SimModelVisualization.BodyRotateZ(selectedBodyProperty.body, 10);

            //ren1.Render();
            RenderWindow.Render();
        }
        private void btnRotXneg_Click(object sender, EventArgs e)
        {
            if (selectedBodyProperty.body == null)
            { return; }
            SimModelVisualization.BodyRotateX(selectedBodyProperty.body, -10);

            //ren1.Render();
            RenderWindow.Render();
        }
        private void btnRotYneg_Click(object sender, EventArgs e)
        {
            if (selectedBodyProperty.body == null)
            { return; }
            SimModelVisualization.BodyRotateY(selectedBodyProperty.body, -10);

            //ren1.Render();
            RenderWindow.Render();
        }
        private void btnRotZneg_Click(object sender, EventArgs e)
        {
            if (selectedBodyProperty.body == null)
            { return; }
            SimModelVisualization.BodyRotateZ(selectedBodyProperty.body, -10);

           // ren1.Render();
            RenderWindow.Render();
        }
        private void btnTranslateX_Click(object sender, EventArgs e)
        {
            if (selectedBodyProperty.body == null)
            { return; }
            SimModelVisualization.BodyTranslate(selectedBodyProperty.body, 0.05, 0, 0);

           // ren1.Render();
            RenderWindow.Render();
        }
        private void btnTranslateY_Click(object sender, EventArgs e)
        {
            if (selectedBodyProperty.body == null)
            { return; }
            SimModelVisualization.BodyTranslate(selectedBodyProperty.body, 0, 0.05, 0);

           // ren1.Render();
            RenderWindow.Render();
        }
        private void btnTranslateZ_Click(object sender, EventArgs e)
        {
            if (selectedBodyProperty.body == null)
            { return; }
            SimModelVisualization.BodyTranslate(selectedBodyProperty.body, 0, 0, 0.05);

          // ren1.Render();
            RenderWindow.Render();
        }
        private void btnTranslateXneg_Click(object sender, EventArgs e)
        {
            if (selectedBodyProperty.body == null)
            { return; }
            SimModelVisualization.BodyTranslate(selectedBodyProperty.body, -0.05, 0, 0);

           // ren1.Render();
            RenderWindow.Render();
        }
        private void btnTranslateYneg_Click(object sender, EventArgs e)
        {
            if (selectedBodyProperty.body == null)
            { return; }
            SimModelVisualization.BodyTranslate(selectedBodyProperty.body, 0, -0.05, 0);

            //ren1.Render();
            RenderWindow.Render();
        }
        private void btnTranslateZneg_Click(object sender, EventArgs e)
        {
            if (selectedBodyProperty.body == null)
            { return; }
            SimModelVisualization.BodyTranslate(selectedBodyProperty.body, 0, 0, -0.05);

            //ren1.Render();
            RenderWindow.Render();
        }
        #endregion

        private void saveMarkersToFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SimModelVisualization.osimModel.getMarkerSet().printToXML(@"C:\Users\Thomas\Desktop\markerFileTest.xml");
            MessageBox.Show("Marker File has been written to 'C:/Users/Thomas/Desktop'", "Markers Exported", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void tabPage2_Click(object sender, EventArgs e)
        {

        }

    }
}
