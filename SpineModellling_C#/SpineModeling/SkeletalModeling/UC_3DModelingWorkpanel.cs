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
using System.Threading;
using System.Threading.Tasks;
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
    public partial class UC_3DModelingWorkpanel : UserControl
    {
        public bool Loaded3D = false;

       
        public AppData AppData;
        public DataBase SQLDB;
        public Subject Subject;
        public EOS EOS;

        public object selectedobject;

        //OpenSim
        public SimModelVisualization SimModelVisualization = new SimModelVisualization();
        private vtkAssembly lastPickedAssembly = vtkAssembly.New();
        public OsimBodyProperty selectedBodyProperty = new OsimBodyProperty();
        public EosImage EosImage1;
        public EosImage EosImage2;
        public EosSpace EosSpace;

       
        private int previousPositionX = 0;
        private int previousPositionY = 0;
        private int numberOfClicks = 0;
        private int resetPixelDistance = 5;


        //VTK
        public Kitware.VTK.RenderWindowControl renderWindowControl1 = new Kitware.VTK.RenderWindowControl();
        public Kitware.VTK.RenderWindowControl renderWindowControlImage1 = new Kitware.VTK.RenderWindowControl();
        public Kitware.VTK.RenderWindowControl renderWindowControlImage2 = new Kitware.VTK.RenderWindowControl();

        public vtkRenderWindow RenderWindow;
        public vtkRenderWindow RenderWindowImage1;
        public vtkRenderWindow RenderWindowImage2;

        public vtkRenderer ren1;
        public vtkRenderer ren1Image1;
        public vtkRenderer ren1Image2;

        public vtkRenderWindowInteractor iren;
        public vtkRenderWindowInteractor irenImage1 = new vtkRenderWindowInteractor();
        public vtkRenderWindowInteractor irenImage2 = new vtkRenderWindowInteractor();

        vtkImageActor vtkImageActor2Dimage1 = new vtkImageActor();
        vtkImageActor vtkImageActor2Dimage2 = new vtkImageActor();

        vtkImageActor vtkImageActor1 = new vtkImageActor();
        vtkImageActor vtkImageActor2 = new vtkImageActor();

        vtkPropPicker propPicker = vtkPropPicker.New();


        vtkBoxWidget boxWidget;

        private bool switchIm1 = false;
        private bool switchIm2 = false;
        List<TreeNode> checkedNodes = new List<TreeNode>();




        public UC_3DModelingWorkpanel()
        {
            InitializeComponent();
        }

        public void intitializeRenderers()
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
            this.renderWindowControl1.TabIndex = 63;
            this.renderWindowControl1.Dock = DockStyle.Fill;
            this.renderWindowControl1.TestText = null;
            this.tableLayoutPanel1.Controls.Add(this.renderWindowControl1, 4, 1);
            renderWindowControl1_Load();


            // 
            // renderWindowControlImage1
            // 
            this.renderWindowControlImage1.AddTestActors = false;
            this.renderWindowControlImage1.Location = new System.Drawing.Point(3, 3);
            this.renderWindowControlImage1.Name = "renderWindowControl2";
            this.renderWindowControlImage1.Size = new System.Drawing.Size(188, 621);
            this.renderWindowControlImage1.TabIndex = 0;
            this.renderWindowControlImage1.TestText = null;
            this.renderWindowControlImage1.Dock = DockStyle.Fill;
            this.renderWindowControlImage1.Load += new System.EventHandler(this.renderWindowControl2_Load);
            this.PnlRenderImage1.Controls.Add(this.renderWindowControlImage1);



            // 
            // renderWindowControlImage2
            // 
            this.renderWindowControlImage2.AddTestActors = false;
            this.renderWindowControlImage2.Location = new System.Drawing.Point(3, 3);
            this.renderWindowControlImage2.Name = "renderWindowControl3";
            this.renderWindowControlImage2.Size = new System.Drawing.Size(188, 621);
            this.renderWindowControlImage2.TabIndex = 0;
            this.renderWindowControlImage2.TestText = null;
            this.renderWindowControlImage2.Dock = DockStyle.Fill;
            this.renderWindowControlImage2.Load += new System.EventHandler(this.renderWindowControl3_Load);
            this.PnlRenderImage2.Controls.Add(this.renderWindowControlImage2);
         

            //Set Database
            SQLDB = new DataBase(AppData.SQLServer, AppData.SQLDatabase, AppData.SQLAuthSQL, AppData.SQLUser, AppData.SQLPassword);
            //Define Patient Object
            // Subject = new Subject(SQLDB, ID, AppData);
            SimModelVisualization.Subject = Subject;
            SimModelVisualization.appData = AppData;


       
            cBshowIm1.Enabled = false;
            cBshowIm2.Enabled = false;

        }

        public void LoadMSModel()
        {
            Cursor.Current = Cursors.WaitCursor;

            // SET SOME OF THE PROPERTIES OF THE SimModelVisualization class.
            //TO this needs to be changed to the Preferences set in the panel??
            SimModelVisualization.geometryDir = AppData.GeometryDir;

            //Call methods in the simModelVisualization class.
            //SimModelVisualization.RenderWindowImage1 = RenderWindowImage1;
            //SimModelVisualization.RenderWindowImage2 = RenderWindowImage2;


            try
            {
                SimModelVisualization.ReadModel();                      //Read the Model. This adds a Model object as property to the object SimModelVisualization.

                ((frmImageAnalysis_new)(this.ParentForm)).AddToLogsAndMessages("The model: " + SimModelVisualization.osimModel.getName() + " was loaded.");
            }
            catch
            {
                ((frmImageAnalysis_new)(this.ParentForm)).AddToLogsAndMessages("There is an error in your model. Could not be loaded.");
                MessageBox.Show("There is an error in your model.Could not be loaded.", "Bad model", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                return;

            }
           
            SimModelVisualization.Model2Treeview(treeView1);        //Fill the TreeView
            ((frmImageAnalysis_new)(this.ParentForm)).AddToLogsAndMessages("TreeView succesfully filled.");

            SimModelVisualization.InitializeModelInRen(ren1);       //Fill the VTK renderer

            RenderWindowImage1.DoubleBufferOn();
            RenderWindowImage2.DoubleBufferOn();
            RenderWindow.DoubleBufferOn();

            ((frmImageAnalysis_new)(this.ParentForm)).AddToLogsAndMessages("Model was initialized in Renderer.");

            Loaded3D = true;

            if (EosImage1 == null)
            {
                panel1Image.Visible = false;
            }
            if (EosImage2 == null)
            {
                panel2Image.Visible = false;
            }

            RenderAll();
        }



        public void AddMarker(Position position, string markerName)
        {
            OsimMakerProperty markerProp = new OsimMakerProperty();

            using (ModelVisualization.frmAttachMarkerToModel frmAttachMarkerToModel = new ModelVisualization.frmAttachMarkerToModel())
            {
                //Pass data to search form
                frmAttachMarkerToModel.simModelVisualization = SimModelVisualization;
                frmAttachMarkerToModel.AppData = this.AppData;
                frmAttachMarkerToModel.newOsimMarkerProperty = markerProp;
                frmAttachMarkerToModel.osimModel = SimModelVisualization.osimModel;
                frmAttachMarkerToModel.defaultName = markerName;
                frmAttachMarkerToModel.absPosition = position;
                frmAttachMarkerToModel.ShowDialog();
                markerProp = frmAttachMarkerToModel.newOsimMarkerProperty;  //Returning the Object

                if (markerProp == null)
                { return; }
                markerProp.vtkRenderwindow = RenderWindow;
                SimModelVisualization.markerPropertyList.Add(markerProp);

            }

            //Add the sphere to the rendering scene
            ren1.AddActor(markerProp.markerActor);
            //ren1.AddActor(vtkFollower);
            ren1.Render();
            RenderWindow.Render();
//            SimModelVisualization.UpdateModelTreeview(treeView1); //Update the treeview

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

                // Pick from this location.
                propPicker.Pick(clickPos[0], clickPos[1], 0, ren1);

                // vtkActorCollection collection = vtkActorCollection.New();
                vtkAssembly assembly = new vtkAssembly();
                assembly = propPicker.GetAssembly();
                vtkActor actor = new vtkActor();

                actor = propPicker.GetActor();

             

                if (assembly != null)
                {

                    SimModelVisualization.UnhighlightEverything();
                    int index = SimModelVisualization.bodyPropertyList.FindIndex(x => x.assembly == assembly);

                    selectedBodyProperty = SimModelVisualization.bodyPropertyList[index];
                    selectedobject = selectedBodyProperty;

                    ABodyWasSelected(selectedBodyProperty);

                    //TreeNode[] treeNodeList = treeView1.Nodes.Find(selectedBodyProperty.objectName, true);
                    //treeView1.SelectedNode = treeNodeList[0];

                    //treeNodeList[0].TreeView.Select();
                    //treeNodeList[0].TreeView.Focus();

                    selectedBodyProperty.HighlightBody();


                    //if (SimModelVisualization.printedTo2Drenderer)
                    //{
                    //    UpdateCamera2Dspace(0, 1);
                    //}
                  
                    RenderWindow.Render();
                }

                if (actor != null)
                {
                    int index = SimModelVisualization.markerPropertyList.FindIndex(x => x.markerActor == actor);

                    if (index == -1)
                    { return; }

                    OsimMakerProperty markerProp = SimModelVisualization.markerPropertyList[index];
                    // selectedBodyProperty = new OsimBodyProperty(); //Just set it empty 
                    selectedobject = markerProp;
                    TreeNode[] treeNodeList = treeView1.Nodes.Find(markerProp.objectName, true);
                    treeView1.SelectedNode = treeNodeList[0];
                    treeNodeList[0].TreeView.Select();
                    treeNodeList[0].TreeView.Focus();
                    markerProp.HighlightMarker();
                    RenderWindow.Render();
                }
            }
        }


        public void ExecuteHandle(vtkObject sender, vtkObjectEventArgs e)
        {
            //uncomment below again (2lines)
            vtkTransform t = new vtkTransform();
            boxWidget.GetTransform(t);

            //vtkTransform d = new vtkTransform();
            ////d.MakeTransform(SimModelVisualization.getSpecifiedBodyProperty(selectedBodyProperty._parentBody).transform)
            t.PostMultiply();
            //d.RotateY(-90);
          //  textBox1.Text = Math.Round(t.GetPosition()[0], 3).ToString() + "  " + Math.Round(t.GetPosition()[1], 3).ToString() + "  " + Math.Round(t.GetPosition()[2], 3).ToString();
            //double[] tmp = SimModelVisualization.ConvertTransformFromSim2VTK(SimModelVisualization.getSpecifiedBodyProperty(selectedBodyProperty._parentBody).absoluteChildTransform).GetOrientation();
            double[] tmp = SimModelVisualization.ConvertTransformFromSim2VTK(selectedBodyProperty.absoluteParentTransform).GetOrientation();
            //double[] tmp = SimModelVisualization.getRelativeVTKTransform(SimModelVisualization.getSpecifiedBodyProperty(selectedBodyProperty._parentBody).transform, SimModelVisualization.getSpecifiedBodyProperty(SimModelVisualization.osimModel.getGroundBody()).transform).GetOrientationWXYZ();
            //  double[] tmp = SimModelVisualization.getSpecifiedBodyProperty(selectedBodyProperty._parentBody)._transformChild.GetOrientation(); //.GetOrientationWXYZ();
            //textBox3.Text = tmp[1].ToString();
            //t.RotateWXYZ(tmp[0], tmp[1], tmp[2], tmp[3]);
            if(selectedBodyProperty.objectName=="sacrum")
            {
                t.RotateY(-90);
            }
            else
            {
       t.RotateY(-tmp[1]);
            }
     
            //t.RotateY(SimModelVisualization.getSpecifiedBodyProperty(selectedBodyProperty._parentBody).transform.GetOrientation()[1]);
            //t.RotateX(SimModelVisualization.getSpecifiedBodyProperty(selectedBodyProperty._parentBody).transform.GetOrientation()[0]);
            //d.Update();
            vtkTransform currentTransform;
            //currentTransform = selectedBodyProperty.transform;
          //  textBox2.Text = Math.Round(t.GetPosition()[0],3).ToString() + "  " + Math.Round(t.GetPosition()[1],3).ToString() + "  " + Math.Round(t.GetPosition()[2],3).ToString();
            //textBox1.Text = (selectedBodyProperty.assembly.GetPosition()[0]).ToString() + "  " + (selectedBodyProperty.assembly.GetPosition()[1]).ToString() + "  " + (selectedBodyProperty.assembly.GetPosition()[2]).ToString();

            //txtBoxwDiff.Text = (selectedBodyProperty.assembly.GetPosition()[0] - t.GetPosition()[0]).ToString() + "  " + (selectedBodyProperty.assembly.GetPosition()[1] - t.GetPosition()[1]).ToString() + "  " + (selectedBodyProperty.assembly.GetPosition()[2] - t.GetPosition()[2]).ToString();
            ////currentTransform.Inverse();
            //t.Concatenate(currentTransform);

            // SimModelVisualization.BodyTranslate(selectedBodyProperty, 0, 0, 0);
            //SimModelVisualization.BodyTranslate(selectedBodyProperty, -t.GetPosition()[2], t.GetPosition()[1], t.GetPosition()[0]);
            SimModelVisualization.BodyTranslate(selectedBodyProperty, t.GetPosition()[0], t.GetPosition()[1], t.GetPosition()[2]);
            //SimModelVisualization.BodyRotateZ(selectedBodyProperty, t.GetOrientation()[2]);
            //SimModelVisualization.BodyRotateX(selectedBodyProperty, t.GetOrientation()[0]);
            //SimModelVisualization.BodyRotateY(selectedBodyProperty, t.GetOrientation()[1]);

            boxWidget.SetTransform(t);
            boxWidget.SetProp3D(selectedBodyProperty.assembly);
            boxWidget.PlaceWidget();
          
            RenderAll();

            return;

            this.selectedBodyProperty.assembly.SetUserTransform(t);


          

            vtkTransform currentTransform2 = t;

            currentTransform.PreMultiply();

            currentTransform.Concatenate(selectedBodyProperty.transform);

            selectedBodyProperty.transform = currentTransform;



            /// Below has been commented
            // /
            // vtkTransform temp = new vtkTransform();
            //temp.DeepCopy((vtkTransform)selectedBodyProperty.transform); //.GetUserTransform();  //(currentTransform);
            //temp.Concatenate(t);

            //selectedBodyProperty.assembly.SetUserTransform(temp);

            /// UP
            //------------------


            //this.selectedBodyProperty.transform;
            currentTransform.Translate(t.GetPosition()[0], t.GetPosition()[1], t.GetPosition()[2]);


            return;
            currentTransform.PostMultiply();

            currentTransform.RotateZ(t.GetOrientation()[2]);
            currentTransform.RotateX(t.GetOrientation()[0]);
            currentTransform.RotateY(t.GetOrientation()[1]);
           

            currentTransform.PostMultiply();


            currentTransform.Scale(t.GetScale()[0], t.GetScale()[1], t.GetScale()[2]);


        }


        private void UpdateCamera2Dspace(double offset, double scaleFactor)
        {
            if (EosSpace == null)
            { return; }

            vtkCamera activecam = new vtkCamera();
            activecam = ren1Image1.GetActiveCamera();
            activecam.SetFocalPoint(0, selectedBodyProperty.absoluteHeight + offset, 0);     //TODO: The Height (Y) should change when moving the image up and down.

            if (!switchIm1)
            {
                activecam.SetPosition(0, selectedBodyProperty.absoluteHeight + offset, EosSpace.PositionSource1.Z);
            }
            else
            {
                activecam.SetPosition(0, selectedBodyProperty.absoluteHeight + offset, -EosSpace.PositionSource1.Z);
            }

            //activecam.SetFocalPoint(0, 1, 0.4);
            //activecam.SetPosition(0, 1, -0.4);
            activecam.ComputeViewPlaneNormal();
            activecam.SetViewUp(0, 1, 0);
            activecam.OrthogonalizeViewUp();
            //   activecam.ParallelProjectionOn();
            activecam.Zoom(scaleFactor);
            ren1Image1.SetActiveCamera(activecam);

            vtkCamera activecam2 = new vtkCamera();
            activecam2 = ren1Image2.GetActiveCamera();
            activecam2.SetFocalPoint(0, selectedBodyProperty.absoluteHeight + offset, 0);     //TODO: The Height (Y) should change when moving the image up and down.
            if (!switchIm2)
            {
                activecam2.SetPosition(EosSpace.PositionSource2.X, selectedBodyProperty.absoluteHeight + offset, 0);
            }
            else
            {
                activecam2.SetPosition(-EosSpace.PositionSource2.X, selectedBodyProperty.absoluteHeight + offset, 0);

            }
            //activecam.SetFocalPoint(0, 1, 0.4);
            //activecam.SetPosition(0, 1, -0.4);
            activecam2.ComputeViewPlaneNormal();
            activecam2.SetViewUp(0, 1, 0);
            activecam2.OrthogonalizeViewUp();
            // activecam.ParallelProjectionOn();
            activecam2.Zoom(scaleFactor);
            ren1Image2.SetActiveCamera(activecam2);

            RenderAll();

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

          
            // iren.MiddleButtonPressEvent
            //SimModelVisualization.AddReferenceCubeToRenderer(ren1, iren);   //THIS MAY BE A PROBLEM!!
        }
        private void renderWindowControl2_Load(object sender, EventArgs e)
        {
            //get a reference to the renderwindow of our renderWindowControlImage1
            RenderWindowImage1 = renderWindowControlImage1.RenderWindow;

            // get a reference to the renderer
            ren1Image1 = RenderWindowImage1.GetRenderers().GetFirstRenderer();
            // set background color
            ren1Image1.SetBackground(1, 1, 1);
            irenImage1 = RenderWindowImage1.GetInteractor();
            RenderWindowImage1.SetInteractor(irenImage1);
            irenImage1.SetInteractorStyle(null);
            irenImage1.MouseWheelBackwardEvt += new vtkObject.vtkObjectEventHandler(irenIm1mouseWheelBackward);
            irenImage1.MouseWheelForwardEvt += new vtkObject.vtkObjectEventHandler(irenIm1mouseWheelForward);
            irenImage1.KeyPressEvt += new vtkObject.vtkObjectEventHandler(irenIm1KeyPressed);

        }
        private void renderWindowControl3_Load(object sender, EventArgs e)
        {
            //get a reference to the renderwindow of our renderWindowControlImage1
            RenderWindowImage2 = renderWindowControlImage2.RenderWindow;
            // get a reference to the renderer
            ren1Image2 = RenderWindowImage2.GetRenderers().GetFirstRenderer();
            // set background color
            ren1Image2.SetBackground(1, 1, 1);
            irenImage2 = RenderWindowImage2.GetInteractor();
            RenderWindowImage2.SetInteractor(irenImage2);
            irenImage2.SetInteractorStyle(null);
            irenImage2.MouseWheelBackwardEvt += new vtkObject.vtkObjectEventHandler(irenIm2mouseWheelBackward);
            irenImage2.MouseWheelForwardEvt += new vtkObject.vtkObjectEventHandler(irenIm2mouseWheelForward);
            irenImage2.KeyPressEvt += new vtkObject.vtkObjectEventHandler(irenIm2KeyPressed);
            //RenderWindowImage1.AddRenderer(ren1Image2);
        }


        public void LoadModelNew()
        {
            //Call methods in the simModelVisualization class.
            SimModelVisualization.RenderWindowImage1 = RenderWindowImage1;
            SimModelVisualization.RenderWindowImage2 = RenderWindowImage2;


        }

        private void cBshowIm1_CheckedChanged(object sender, EventArgs e)
        {
            if (cBshowIm1.Checked)
            {
                vtkImageActor1.SetOpacity(1);
                vtkImageActor2Dimage1.SetOpacity(1);

            }
            else
            {
                vtkImageActor1.SetOpacity(0);
                vtkImageActor2Dimage1.SetOpacity(0);
            }
            RenderAll();
        }

        public void RenderAll()
        {

            //ren1.Render();
            //ren1Image1.Render();
            //ren1Image2.Render();

            //ren1Image1.Render();
            //ren1Image2.Render();
            //RenderWindow.Render();

            //ren1.GetRenderWindow().Render();
            //ren1Image1.GetRenderWindow().Render();
            //ren1Image2.GetRenderWindow().Render();


            //RenderWindowImage1.Render();
            //RenderWindowImage2.Render();

           
                RenderWindow.Render();
           

          
                renderWindowControlImage1.RenderWindow.Render();
          
                renderWindowControlImage2.RenderWindow.Render();
          
            
        }


        public void MakeEosSpace(EosImage EosImage1, EosImage EosImage2, vtkRenderer ren1, EosSpace EosSpace)
        {
            EosSpace = new EosSpace(EosImage1, EosImage2);
            this.EosSpace = EosSpace;
            EosSpace.EOSImageA = EosImage1;
            EosSpace.EOSImageB = EosImage2;

            EosSpace.CalculateEosSpace();

            //Isocenter Location
            vtkConeSource IsocenterCone = vtkConeSource.New();
            IsocenterCone.SetHeight(0.05);
            IsocenterCone.SetResolution(25);
            IsocenterCone.SetRadius(0.02);

            vtkPolyDataMapper IsocenterConeMapper = vtkPolyDataMapper.New();
            IsocenterConeMapper.SetInputConnection(IsocenterCone.GetOutputPort());

            vtkLODActor IsocenterConeActor = vtkLODActor.New();
            IsocenterConeActor.SetMapper(IsocenterConeMapper);
            //IsocenterConeActor.SetScale(1000, 1000, 1000);
            IsocenterConeActor.SetOrientation(0, 0, 90);
            IsocenterConeActor.SetPosition(0, 0, 0);
            IsocenterConeActor.GetProperty().SetColor(0.1, 0.7, 0.5);
            IsocenterConeActor.PickableOff();
            ren1.AddActor(IsocenterConeActor);

            //Patient Location
            vtkLODActor PatientConeActor = vtkLODActor.New();
            PatientConeActor.SetMapper(IsocenterConeMapper);
            //PatientConeActor.SetScale(1000, 1000, 1000);
            PatientConeActor.SetOrientation(0, 0, 90);
            PatientConeActor.SetPosition(EosSpace.PatientPosition.X, EosSpace.PatientPosition.Y, EosSpace.PatientPosition.Z);
            PatientConeActor.GetProperty().SetColor(0.1, 0.7, 0.1);
            PatientConeActor.PickableOff();
            ren1.AddActor(PatientConeActor);



            //Create a camera model as X-ray Sources
            vtkConeSource camCS = vtkConeSource.New();
            camCS.SetHeight(0.05);
            camCS.SetResolution(25);
            camCS.SetRadius(0.02);

            vtkCubeSource camCBS = vtkCubeSource.New();
            camCBS.SetXLength(0.05);
            camCBS.SetZLength(0.05);
            camCBS.SetYLength(0.05);
            camCBS.SetCenter(0.025, 0, 0);

            vtkAppendPolyData camAPD = vtkAppendPolyData.New();
            camAPD.AddInputConnection(camCBS.GetOutputPort());
            camAPD.AddInputConnection(camCS.GetOutputPort());

            vtkPolyDataMapper camMapper = vtkPolyDataMapper.New();
            camMapper.SetInputConnection(camAPD.GetOutputPort());

            vtkLODActor camActorF = vtkLODActor.New();
            camActorF.SetMapper(camMapper);
            camActorF.SetScale(2, 2, 2);
            camActorF.SetOrientation(0, 90, 0);
            camActorF.SetPosition(EosSpace.PositionSource1.X, EosSpace.PositionSource1.Y, EosSpace.PositionSource1.Z);
            camActorF.GetProperty().SetColor(0.8, 0.2, 0.2);
            camActorF.PickableOff();

            vtkLODActor camActorL = vtkLODActor.New();
            camActorL.SetMapper(camMapper);
            camActorL.SetScale(2, 2, 2);
            camActorL.SetOrientation(0, 180, 0);
            camActorL.SetPosition(EosSpace.PositionSource2.X, EosSpace.PositionSource2.Y, EosSpace.PositionSource2.Z);
            camActorL.GetProperty().SetColor(0.8, 0.2, 0.2);
            camActorL.PickableOff();

            ren1.AddActor(camActorF);
            ren1.AddActor(camActorL);

            //Creating a surrounding EOS volume box
            vtkCubeSource eosBox = vtkCubeSource.New();
            eosBox.SetXLength(EosImage2.DistanceSourceToDetector);
            eosBox.SetYLength(EosImage1.Height); // De hoogte van het eerste beeld word gekozen. Normaal gelijke hoogte voor beide beelden.
            eosBox.SetZLength(EosImage1.DistanceSourceToDetector);
            eosBox.SetCenter(-((EosImage2.DistanceSourceToDetector / 2) - (EosImage2.DistanceSourceToDetector - EosImage2.DistanceSourceToIsocenter)), EosImage1.Height / 2, -((EosImage1.DistanceSourceToDetector / 2) - (EosImage1.DistanceSourceToDetector - EosImage1.DistanceSourceToIsocenter)));

            vtkPolyDataMapper eosBoxMapper = vtkPolyDataMapper.New();
            eosBoxMapper.SetInputConnection(eosBox.GetOutputPort());

            vtkActor eosBoxActor = vtkActor.New();
            eosBoxActor.SetMapper(eosBoxMapper);
            eosBoxActor.SetPosition(0, 0, 0);
            eosBoxActor.GetProperty().SetRepresentationToWireframe();
            eosBoxActor.PickableOff();

            //Create the axes and the associated mapper and actor.
            vtkAxes axes2 = vtkAxes.New();
            axes2.SetOrigin(0, 0, 0);
            axes2.SetScaleFactor(0.8);

            vtkPolyDataMapper axes2Mapper = vtkPolyDataMapper.New();
            axes2Mapper.SetInputConnection(axes2.GetOutputPort());
            vtkActor axes2Actor = vtkActor.New();
            axes2Actor.SetMapper(axes2Mapper);

            vtkCamera cam1 = vtkCamera.New();
            cam1 = ren1.GetActiveCamera();
            //cam1.SetPosition(-3000, 5000, -3000); // [expr -2*$distanceSourceToDetectorF] [expr 4*$distanceSourceToDetectorF] [expr -2*$distanceSourceToDetectorL]
            cam1.SetPosition(-2 * EosImage1.DistanceSourceToDetector, 4 * EosImage1.DistanceSourceToDetector, -2 * EosImage2.DistanceSourceToDetector);
            //Creating axes arount the EOS volume
            vtkCubeAxesActor axes1 = vtkCubeAxesActor.New();
            axes1.SetBounds(-EosImage2.DistanceSourceToIsocenter, EosImage2.DistanceSourceToDetector - EosImage2.DistanceSourceToIsocenter, 0, EosImage1.Height, -EosImage1.DistanceSourceToIsocenter, EosImage1.DistanceSourceToDetector - EosImage1.DistanceSourceToIsocenter);
            axes1.SetCamera(ren1.GetActiveCamera());
            axes1.SetXLabelFormat("%0.1f");
            axes1.SetYLabelFormat("%0.1f");
            axes1.SetZLabelFormat("%0.1f");
            axes1.SetFlyModeToOuterEdges();

            ren1.AddActor(eosBoxActor);
            ren1.AddActor(axes2Actor);
            ren1.AddViewProp(axes1);


            vtkPNGReader vtkPNGReader1 = new vtkPNGReader();
            string dir1 = AppData.TempDir + "\\tempF.png";
            vtkPNGReader1.SetFileName(dir1);
            vtkPNGReader1.SetDataSpacing(EosImage1.PixelSpacingX, EosImage1.PixelSpacingY, 1);
            EosImage1.PNGReader = vtkPNGReader1;

            if (EosImage1.imageRotated)    //TEST AIS
            {

                vtkImageFlip vtkImageFlip = new vtkImageFlip();
                vtkImageFlip.SetFilteredAxis(2); //x axis
                vtkImageFlip.SetInputConnection(vtkPNGReader1.GetOutputPort());
                vtkImageFlip.Update();
                EosImage1.vtkImageFlip = vtkImageFlip;
                vtkImageActor1.SetInput(vtkImageFlip.GetOutput());
            }
            else
            {
                vtkImageActor1.SetInput(vtkPNGReader1.GetOutput());
            }

            vtkPNGReader vtkPNGReader2 = new vtkPNGReader();
            string dir2 = AppData.TempDir + "\\tempL.png";
            vtkPNGReader2.SetFileName(dir2);
            vtkPNGReader2.SetDataSpacing(EosImage2.PixelSpacingX, EosImage2.PixelSpacingY, 1);
            EosImage2.PNGReader = vtkPNGReader2;



            if (EosImage2.imageRotated)    //TEST AIS
            {
                vtkImageFlip vtkImageFlip = new vtkImageFlip();
                vtkImageFlip.SetFilteredAxis(2); //x( axis
                vtkImageFlip.SetInputConnection(vtkPNGReader2.GetOutputPort());
                vtkImageFlip.Update();
                EosImage2.vtkImageFlip = vtkImageFlip;
                vtkImageActor2.SetInput(vtkImageFlip.GetOutput());
            }
            else
            {
                vtkImageActor2.SetInput(vtkPNGReader2.GetOutput());
            }




            DisplayEOSin2Dspace(EosImage1, EosSpace, ren1Image1, vtkImageActor2Dimage1, 1);
            DisplayEOSin2Dspace(EosImage2, EosSpace, ren1Image2, vtkImageActor2Dimage2, 2);

            DisplayEOSin3Dspace(EosImage1, EosImage2, EosSpace, ren1);

            panel1Image.Visible = true;
            panel2Image.Visible = true;
            cBshowIm1.Checked = true;
            cBshowIm2.Checked = true;
            cBshowIm1.Enabled = true;
            cBshowIm2.Enabled = true;

        }

        private void DisplayEOSin3Dspace(EosImage EosImage1, EosImage EosImage2, EosSpace EosSpace, vtkRenderer ren1)
        {

            if(EosImage1.imageRotated)
            {
                vtkImageActor1.SetInput(EosImage1.vtkImageFlip.GetOutput());
            }
            else
            {
                vtkImageActor1.SetInput(EosImage1.PNGReader.GetOutput());
            }

            //vtkImageActor1.SetInputData(vtkPNGReader1.GetOutput()); //VTK_Version6
            vtkImageActor1.SetOrientation(EosSpace.OrientationImage1.X, EosSpace.OrientationImage1.Y, EosSpace.OrientationImage1.Z);
            vtkImageActor1.SetPosition(EosSpace.PositionOriginImage1.X, EosSpace.PositionOriginImage1.Y, EosSpace.PositionOriginImage1.Z);
            vtkImageActor1.PickableOff();


            if (EosImage2.imageRotated)
            {
                vtkImageActor2.SetInput(EosImage2.vtkImageFlip.GetOutput());
            }
            else
            {
                vtkImageActor2.SetInput(EosImage2.PNGReader.GetOutput());
            }
          
            //vtkImageActor2.SetInputData(vtkPNGReader2.GetOutput());  //VTK_Version6
            vtkImageActor2.SetOrientation(EosSpace.OrientationImage2.X, EosSpace.OrientationImage2.Y, EosSpace.OrientationImage2.Z);
            vtkImageActor2.SetPosition(EosSpace.PositionOriginImage2.X, EosSpace.PositionOriginImage2.Y, EosSpace.PositionOriginImage2.Z);
            vtkImageActor2.PickableOff();

            ren1.AddActor(vtkImageActor1);
            ren1.AddActor(vtkImageActor2);
        }
       

        private void DisplayEOSin2Dspace(EosImage EosImage, EosSpace EosSpace, vtkRenderer ren, vtkImageActor vtkImgageActor, int ImageNb)
        {
            if (EosImage.imageRotated)
            {
                vtkImgageActor.SetInput(EosImage.vtkImageFlip.GetOutput());   //TEST AIS
            }
            else
            {
                vtkImgageActor.SetInput(EosImage.PNGReader.GetOutput());
            }


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





        public void btnRotX_Click(object sender, EventArgs e)
        {
            RotXwithPrecision(1);
        }

        private void RotXwithPrecision(int sign)
        {
            if (selectedBodyProperty.body == null)
            { return; }

            double value = 0;
            try
            {
                value = Convert.ToDouble(txtAccAngle.Text);
            }
            catch
            {
                MessageBox.Show(txtAccAngle.Text + " is not an acceptable value for the angle accuracy.", "Bad value", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                return;
            }

            SimModelVisualization.BodyRotateX(selectedBodyProperty, sign*value);
            RenderAll();
        }

        private void RotYwithPrecision(int sign)
        {

            double value = 0;
            try
            {
                value = Convert.ToDouble(txtAccAngle.Text);
            }
            catch
            {
                MessageBox.Show(txtAccAngle.Text + " is not an acceptable value for the angle accuracy.", "Bad value", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                return;
            }


            if (selectedBodyProperty.body == null)
            { return; }
            //SimModelVisualization.UpdateRenderer(ren1);
            //var watch = System.Diagnostics.Stopwatch.StartNew();

            SimModelVisualization.BodyRotateY(selectedBodyProperty, sign*value);
            //Console.WriteLine(watch.ElapsedMilliseconds + "  body rotate");
            //SimModelVisualization.UpdateRenderer();
            //Console.WriteLine(watch.ElapsedMilliseconds + "  update renderer");

            //SimModelVisualization.ConeBeamCorrectBody(selectedBodyProperty);
            //SimModelVisualization.UpdateOpaceAssembly(selectedBodyProperty);

            RenderAll();
            // Console.WriteLine(watch.ElapsedMilliseconds + "  Render all");
            //watch.Stop();
        }

        private void RotZwithPrecision(int sign)
        {
            double value = 0;
            try
            {
                value = Convert.ToDouble(txtAccAngle.Text);
            }
            catch
            {
                MessageBox.Show(txtAccAngle.Text + " is not an acceptable value for the angle accuracy.", "Bad value", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                return;
            }

            if (selectedBodyProperty.body == null)
            { return; }

            SimModelVisualization.BodyRotateZ(selectedBodyProperty, sign*value);
            RenderAll();
        }


        private void btnRotY_Click(object sender, EventArgs e)
        {
            RotYwithPrecision(1);
        }
        private void btnRotZ_Click(object sender, EventArgs e)
        {
            RotZwithPrecision(1);


        }
        private void btnRotXneg_Click(object sender, EventArgs e)
        {
            RotXwithPrecision(-1);
        }
        private void btnRotYneg_Click(object sender, EventArgs e)
        {
            RotYwithPrecision(-1);


        }
        private void btnRotZneg_Click(object sender, EventArgs e)
        {
            RotZwithPrecision(-1);

        }
        private void TranslateXwithPrecision(int sign)
        {
            double value = 0;
            try
            {
                value = Convert.ToDouble(txtAccDist.Text);
            }
            catch
            {
                MessageBox.Show(txtAccDist.Text + " is not an acceptable value for the displacement accuracy.", "Bad value", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                return;
            }

            if (selectedBodyProperty.body == null)
            { return; }

            SimModelVisualization.BodyTranslate(selectedBodyProperty, (double)((sign * value) /  (double)1000), 0, 0);


            RenderAll();

        }
        private void TranslateYwithPrecision(int sign)
        {
            double value = 0;
            try
            {
                value = Convert.ToDouble(txtAccDist.Text);
            }
            catch
            {
                MessageBox.Show(txtAccDist.Text + " is not an acceptable value for the displacement accuracy.", "Bad value", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                return;
            }

            if (selectedBodyProperty.body == null)
            { return; }

            SimModelVisualization.BodyTranslate(selectedBodyProperty, 0, (double)((sign * value) / (double)1000), 0);
            RenderAll();

        }
        private void TranslateZwithPrecision(int sign)
        {
            double value = 0;
            try
            {
                value = Convert.ToDouble(txtAccDist.Text);
            }
            catch
            {
                MessageBox.Show(txtAccDist.Text + " is not an acceptable value for the displacement accuracy.", "Bad value", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                return;
            }

            if (selectedBodyProperty.body == null)
            { return; }

            SimModelVisualization.BodyTranslate(selectedBodyProperty, 0, 0, (double)((sign * value) / (double)1000));
            RenderAll();

        }
        private void btnTranslateX_Click(object sender, EventArgs e)
        {
            TranslateXwithPrecision(1);
        }
        private void btnTranslateY_Click(object sender, EventArgs e)
        {
            TranslateYwithPrecision(1);
        }
        private void btnTranslateZ_Click(object sender, EventArgs e)
        {
            TranslateZwithPrecision(1);
        }
        private void btnTranslateXneg_Click(object sender, EventArgs e)
        {
            TranslateXwithPrecision(-1);
        }
        private void btnTranslateYneg_Click(object sender, EventArgs e)
        {
            TranslateYwithPrecision(-1);
        }
        private void btnTranslateZneg_Click(object sender, EventArgs e)
        {
            TranslateZwithPrecision(-1);
        }

       

        private void irenIm1mouseWheelBackward(vtkObject sender, vtkObjectEventArgs e)
        {
            int j = irenImage1.GetControlKey();
            if (j == 8)
            {
                vtkCamera cam = ren1Image1.GetActiveCamera();
                cam.Zoom(0.9);
                ren1Image1.SetActiveCamera(cam);
                ren1Image1.Render();
                ren1Image1.GetRenderWindow().Render();

            }
            else
            {

                RotXwithPrecision(-1);
            }
        }
        private void irenIm1mouseWheelForward(vtkObject sender, vtkObjectEventArgs e)
        {

            int j = irenImage1.GetControlKey();
            if (j == 8)
            {
                vtkCamera cam = ren1Image1.GetActiveCamera();
                cam.Zoom(1.1);
                ren1Image1.SetActiveCamera(cam);
                ren1Image1.Render();
                ren1Image1.GetRenderWindow().Render();

            }
            else
            {
                RotXwithPrecision(1);
            }
        }
        private void irenIm2mouseWheelBackward(vtkObject sender, vtkObjectEventArgs e)
        {
            int j = irenImage2.GetControlKey();
            if (j == 8)
            {
                vtkCamera cam = ren1Image2.GetActiveCamera();
                cam.Zoom(0.9);
                ren1Image2.SetActiveCamera(cam);
                ren1Image2.Render();
                ren1Image2.GetRenderWindow().Render();

            }
            else
            {
                RotZwithPrecision(1);
            }
        }
        private void irenIm2mouseWheelForward(vtkObject sender, vtkObjectEventArgs e)
        {

            int j = irenImage2.GetControlKey();
            if (j == 8)
            {
                vtkCamera cam = ren1Image2.GetActiveCamera();
                cam.Zoom(1.1);
                ren1Image2.SetActiveCamera(cam);
                ren1Image2.Render();
                ren1Image2.GetRenderWindow().Render();

            }
            else
            {
                RotZwithPrecision(-1);
            }
        }
        private void irenIm1KeyPressed(object sender, EventArgs e)
        {
            string key = irenImage1.GetKeySym();
            try
            {
                double value = double.Parse(txtMovePrecision.Text) / 100;

                if (key == "d")
                {
                    MoveVTKCamera(ren1Image1, -value, 0, 0);
                }

                if (key == "u")
                {
                    MoveVTKCamera(ren1Image1, value, 0, 0);
                }

                if (key == "l")
                {
                    MoveVTKCamera(ren1Image1, 0, value, 0);
                }
                if (key == "r")
                {
                    MoveVTKCamera(ren1Image1, 0, -value, 0);
                }
            }
            catch
            {
                MessageBox.Show("wrong value", "error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        private void irenIm2KeyPressed(object sender, EventArgs e)
        {


            string key = irenImage2.GetKeySym();
            try
            {
                double value = double.Parse(txtMovePrecision.Text) / 100;

                if (key == "d")
                {
                    MoveVTKCamera(ren1Image2, -value, 0, 0);
                }

                if (key == "u")
                {
                    MoveVTKCamera(ren1Image2, value, 0, 0);
                }

                if (key == "r")
                {
                    MoveVTKCamera(ren1Image2, 0, 0, value);
                }
                if (key == "l")
                {
                    MoveVTKCamera(ren1Image2, 0, 0, -value);
                }
            }
            catch
            {
                MessageBox.Show("wrong value", "error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }


        }

        private void MoveVTKCamera(vtkRenderer ren, double valueVertical, double valueHorizontalX, double valueHorizontalZ)
        {
            vtkCamera cam = ren.GetActiveCamera();
            double[] currentPosCam = cam.GetPosition();
            double[] currentFocPoint = cam.GetFocalPoint();
            cam.SetPosition(currentPosCam[0] + valueHorizontalX, currentPosCam[1] + valueVertical, currentPosCam[2] + valueHorizontalZ);
            cam.SetFocalPoint(currentFocPoint[0] + valueHorizontalX, currentFocPoint[1] + valueVertical, currentFocPoint[2] + valueHorizontalZ);
            ren.SetActiveCamera(cam);
            ren.Render();
            ren.GetRenderWindow().Render();

        }

        public void DeleteMarkerProp(OsimMakerProperty markerProp1)
        {

            ren1.RemoveActor(markerProp1.markerActor);
            ren1.Render();
            ren1.GetRenderWindow().Render();
            SimModelVisualization.markerPropertyList.Remove(markerProp1);
            GC.Collect();
        }

        private void btnPrintMarkers_Click(object sender, EventArgs e)
        {
            ((frmImageAnalysis_new)(this.ParentForm))._2DMeasurementsWorkpanel.UC_measurementsMain.PrintMarkersToTrc();

            return;

        }

        private void btnSwitchIm1_Click(object sender, EventArgs e)
        {
            switchIm1 = !switchIm1;
            if (switchIm1)
            { btnSwitchIm1.BackColor = Color.DarkRed; }
            else
            {
                btnSwitchIm1.BackColor = Color.Green;
            }
            UpdateCamera2Dspace(0, 1);
        }

        private void btnSwitchIm2_Click(object sender, EventArgs e)
        {
            switchIm2 = !switchIm2;
            if (switchIm2)
            { btnSwitchIm2.BackColor = Color.DarkRed; }
            else
            {
                btnSwitchIm2.BackColor = Color.Green;
            }
            UpdateCamera2Dspace(0, 1);
        }

        private void btnRotateModel_Click(object sender, EventArgs e)
        {
            SimModelVisualization.BodyRotateY(SimModelVisualization.getSpecifiedBodyPropertyFromName("sacrum").body, 90);
            RenderAll();
        }

        private void btnTestChangeCoordinate_Click(object sender, EventArgs e)
        {
            double degrees = 50;
            double radian = SimModelVisualization.DegreeToRadian(degrees);
            SimModelVisualization.bodyPropertyList[6].osimJointProperty.osimJointCoordinatePropertyList[2].coordinate.setLocked(SimModelVisualization.si, false);  
            SimModelVisualization.bodyPropertyList[6].osimJointProperty.osimJointCoordinatePropertyList[2].coordinate.setDefaultValue(radian);
            SimModelVisualization.bodyPropertyList[6].osimJointProperty.osimJointCoordinatePropertyList[2].coordinate.setValue(SimModelVisualization.si, radian);
         

            SimModelVisualization.osimModel.updBodySet();
            SimModelVisualization.bodyPropertyList[6].body.getJoint().updDisplayer();
            SimModelVisualization.osimModel.updateDisplayer(SimModelVisualization.si);
       
            SimModelVisualization.osimModel.updateDisplayer(SimModelVisualization.si);
          
            SimModelVisualization.UpdateRenderer();
        }

        public Measurement Measurement;

        public void LoadGravityLineData()
        {

            using (frmLoadFPdata frmLoadFPdata = new frmLoadFPdata())
            {
                frmLoadFPdata.ShowDialog();

                //Set Database
                SQLDB = new DataBase(AppData.SQLServer, AppData.SQLDatabase, AppData.SQLAuthSQL, AppData.SQLUser, AppData.SQLPassword);
                //Define Measurement Object
                Measurement = new Measurement(SQLDB, Subject, EOS.AcquisitionNumber, 0, AppData);

                Measurement.PosX = frmLoadFPdata.X;
                Measurement.PosY = frmLoadFPdata.Y;
                Measurement.PosZ = frmLoadFPdata.Z;
                Measurement.MeasurementName = "Gravity Line";
                Measurement.MeasurementComment = "GRF";
                Measurement.Save();

            }


            VisualizeGRFvector(Measurement);

        }

        private void UpdateCurrentBodyProperty(OsimBodyProperty bodyProp)
        {
            selectedBodyProperty = bodyProp;
            lblCurrentBody.Text = "Active component: " + selectedBodyProperty.objectName;

            if (bodyProp.objectName == "ground")
            {
                return;
            }
            trackBarScaleX.Value = (int)(bodyProp.scaleFactors.get(0) * 100);
            trackBarScaleY.Value = (int)(bodyProp.scaleFactors.get(1) * 100);
            trackBarScaleZ.Value = (int)(bodyProp.scaleFactors.get(2) * 100);

        }

        public void ExecuteIfEOSandModelAreLoaded()
        {
            SimModelVisualization.PrintBodiesTo2DRenderer(ren1Image1, ren1Image2);


            toolTip1.SetToolTip(cBshowIm1, EosImage1._ImagePlane);
            toolTip1.SetToolTip(cBshowIm2, EosImage2._ImagePlane);
        }

        private void VisualizeGRFvector(Measurement Measurement)
        {
            vtkArrowSource GRFvectorObject = vtkArrowSource.New();
            GRFvectorObject.SetTipLength(0.30);
            GRFvectorObject.SetTipResolution(50);
            GRFvectorObject.SetShaftRadius(0.007);
            GRFvectorObject.SetTipRadius(0.020);
            GRFvectorObject.SetShaftResolution(50);

            vtkPolyDataMapper polygonmapperGRF = vtkPolyDataMapper.New();
            polygonmapperGRF.SetInputConnection(GRFvectorObject.GetOutputPort());

            vtkActor polygonActorGRF = vtkActor.New();
            polygonActorGRF.SetMapper(polygonmapperGRF);
            polygonActorGRF.SetOrientation(0, 0, 90);
            polygonActorGRF.SetPosition(Measurement.PosX, Measurement.PosY, Measurement.PosZ);
            polygonActorGRF.GetProperty().SetColor(1, 0.3, 0.3);

            ren1.AddActor(polygonActorGRF);
            RenderWindow.Render();
        }

        private void ConvertDicomToBmp()
        {
            //TODO: Convert dicom to BMP does not work. Problem with ITK package ?!?! 
            ImageFileReader ImageFileReader = new ImageFileReader();
            //VectorString VectorStringIn = new VectorString();
            //VectorStringIn.Add(EosImage1.Directory);
            ImageFileReader.SetFileName(EosImage1.Directory);
            ImageFileReader.Execute();

            //vtkImageExport vtkImageExport = new vtkImageExport();
            //vtkImageExport.SetInput(ImageSeriesReader);

            itk.simple.Image Image = new itk.simple.Image();
            Image = ImageFileReader.Execute();
            //dim = Image.GetDimension()

            ImageFileWriter ImageFileWriter = new ImageFileWriter();
            //VectorString VectorStringOut = new VectorString();
            //VectorStringOut.Add(@"C:\ThomasOverbergh\SpineAnalyzerPlatform\ProgramTestData\Converted.bmp");
            string dir1 = AppData.TempDir + "\\Converted.bmp";
            ImageFileWriter.SetFileName(dir1);
            ImageFileWriter.Execute(Image);

        }

        #region Test for MRI
        vtkImagePlaneWidget planeWidgetX;
        vtkImagePlaneWidget planeWidgetY;
        vtkImagePlaneWidget planeWidgetZ;
        public void MRItest()
        {
            Open2DFile();
        }

        public void Open2DFile()
        {
            // The actual dicom reader
            vtkDICOMImageReader dicomreader = new vtkDICOMImageReader();
            //string dir = @"C:\uz\Temp\ProgramdatabaseFiles\500226v510\MRI\_T1W_TSE_T8";
            // string dir = @"C:\uz\Temp\ProgramdatabaseFiles\500226v510\MRI\_MobiView_WIP_sT1W_3D_TFE_tra_SENSE";
            string dir = @"C:\ThomasOverbergh\ASDstudie\MRI Pieter";

            dicomreader.SetDirectoryName(dir);
            dicomreader.Update();
            dicomreader.UpdateInformation();

            //GET SOME INFORMATION
            double[] pixelspacing = dicomreader.GetPixelSpacing();
            int width = dicomreader.GetWidth();
            int heigt = dicomreader.GetHeight();
            float[] patientpos = dicomreader.GetImagePositionPatient();
            float[] patientorienation = dicomreader.GetImageOrientationPatient();
            int[] extent = dicomreader.GetDataExtent();


            vtkImageChangeInformation ici = new vtkImageChangeInformation();
            ici.SetInputConnection(dicomreader.GetOutputPort());
            if (pixelspacing[2] == 0)
            { ici.SetOutputSpacing(pixelspacing[0], pixelspacing[1], 1); }
            ici.SetSpacingScale(0.001, 0.001, 0.001);
            //ici.SetOriginScale(0.001, 0.001, 0.001);
            // ici.SetOriginTranslation(0, 0, 0);
            ici.SetOutputOrigin(-(double)extent[1] / 2000, 0.50, -(double)extent[5] / 2000);
            //ici.CenterImageOn(); //Don't know why this is?


            //An outline is shown for context.
            vtkOutlineFilter outline = vtkOutlineFilter.New();
            //outline.SetInputConnection(dicomreader.GetOutputPort());
            outline.SetInputConnection(ici.GetOutputPort());

            vtkPolyDataMapper outlineMapper = vtkPolyDataMapper.New();
            outlineMapper.SetInputConnection(outline.GetOutputPort());

            vtkActor outlineActor = vtkActor.New();
            outlineActor.SetMapper(outlineMapper);

            ren1.AddActor(outlineActor);


            int[] dims = new int[3];
            dims = dicomreader.GetOutput().GetDimensions();
            //trackBarX.SetRange(0, dims[0]);
            //trackBarX.Value = dims[0] / 2;
            //trackBarY.SetRange(0, dims[1]);
            //trackBarY.Value = dims[1] / 2;
            //trackBarZ.SetRange(0, dims[2]);
            //trackBarZ.Value = dims[2] / 2;
            //dims[2] = dims[2] / 2;


            vtkImageCast readerImageCast = vtkImageCast.New();
            //readerImageCast.SetInput((vtkDataObject)dicomreader.GetOutput());
            readerImageCast.SetInput(ici.GetOutput());


            //#if VTK_MAJOR_VERSION_5
            //            readerImageCast.SetInput((vtkDataObject)dicomreader.GetOutput());
            //#if VTK_MAJOR_VERSION_6
            //            readerImageCast.SetInputData((vtkDataObject)dicomreader.GetOutput());
            //#endif

            //readerImageCast.SetInputData((vtkDataObject)dicomreader.GetOutput());     //VTK_Version6
            //readerImageCast.SetOutputScalarTypeToUnsignedChar();
            readerImageCast.ClampOverflowOn();
            readerImageCast.Update();


            //The shared picker enables us to use 3 planes at one time
            //and gets the picking order right
            vtkCellPicker picker = new vtkCellPicker();
            picker.SetTolerance(0.005);

            // The 3 image plane widgets are used to probe the dataset.
            planeWidgetX = new vtkImagePlaneWidget();
            planeWidgetX.DisplayTextOn();
            //planeWidgetX.SetInput(center.GetOutput());
            planeWidgetX.SetInput(readerImageCast.GetOutput());
            //planeWidgetX.SetInputData(readerImageCast.GetOutput());  //VTK_Version6
            planeWidgetX.SetPlaneOrientationToXAxes();
            planeWidgetX.SetKeyPressActivationValue((sbyte)'x');
            planeWidgetX.SetSliceIndex(dims[0] / 2);
            planeWidgetX.SetPicker(picker);
            vtkProperty prop1 = planeWidgetX.GetPlaneProperty();
            prop1.SetColor(1, 0, 0);

            planeWidgetY = new vtkImagePlaneWidget();
            planeWidgetY.DisplayTextOn();
            // planeWidgetY.SetInput(center.GetOutput());
            planeWidgetY.SetInput(readerImageCast.GetOutput());
            //planeWidgetX.SetInputData(readerImageCast.GetOutput());  //VTK_Version6
            planeWidgetY.SetKeyPressActivationValue((sbyte)'y');
            planeWidgetY.SetPlaneOrientationToYAxes();
            planeWidgetY.SetSliceIndex(dims[1] / 2);
            planeWidgetY.SetPicker(picker);
            vtkProperty prop2 = planeWidgetY.GetPlaneProperty();
            prop2.SetColor(1, 1, 0);
            planeWidgetY.SetLookupTable(planeWidgetY.GetLookupTable());

            //for the z-slice, turn off texture interpolation:
            //interpolation is now nearest neighbour, to demonstrate
            //cross-hair cursor snapping to pixel centers
            planeWidgetZ = new vtkImagePlaneWidget();
            planeWidgetZ.DisplayTextOn();
            //planeWidgetZ.SetInput(center.GetOutput());
            planeWidgetZ.SetInput(readerImageCast.GetOutput());

            //planeWidgetZ.SetInputData(readerImageCast.GetOutput());   //VTK_Version6
            planeWidgetZ.SetPlaneOrientationToZAxes();
            planeWidgetZ.SetKeyPressActivationValue((sbyte)'z');
            planeWidgetZ.SetSliceIndex(dims[2] / 2);
            planeWidgetZ.SetPicker(picker);
            vtkProperty prop3 = planeWidgetZ.GetPlaneProperty();
            prop3.SetColor(0, 0, 1);
            planeWidgetZ.SetLookupTable(planeWidgetZ.GetLookupTable());


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


            //IntPtr intptr = new IntPtr(tempptr);
            //intptr( = tempptr.ToArray();


            //vtkConeSource IsocenterCone = vtkConeSource.New();
            //IsocenterCone.SetHeight(0.05);
            //IsocenterCone.SetResolution(25);
            //IsocenterCone.SetRadius(0.02);

            //vtkPolyDataMapper IsocenterConeMapper = vtkPolyDataMapper.New();
            //IsocenterConeMapper.SetInputConnection(IsocenterCone.GetOutputPort());

            //vtkLODActor IsocenterConeActor = vtkLODActor.New();
            //IsocenterConeActor.SetMapper(IsocenterConeMapper);
            //IsocenterConeActor.SetScale(1000, 1000, 1000);
            //IsocenterConeActor.SetOrientation(0, 0, 90);
            //IsocenterConeActor.SetPosition(100, 1000, 1000);
            //IsocenterConeActor.GetProperty().SetColor(0.1, 0.7, 0.5);
            //IsocenterConeActor.PickableOff();
            //ren1.AddActor(IsocenterConeActor);

            //vtkBoxWidget vtkBoxWidget = vtkBoxWidget.New();
            //vtkBoxWidget.SetInteractor(iren);
            //vtkBoxWidget.SetProp3D(IsocenterConeActor);
            //vtkBoxWidget.SetPlaceFactor(1.25);
            //vtkBoxWidget.PlaceWidget();


            // vtkBoxWidget.StartInteractionEvt += new vtkObject.vtkObjectEventHandler(BeginInteraction);
            // //= new vtkBoxCallBack.New();
            // vtkBoxCallBack boxCallback = (vtkBoxCallBack)vtkBoxCallBack.New();

            // boxCallback.SetActor(IsocenterConeActor);

            //// vtkBoxWidget.InteractionEvt += new vtkObject.vtkObjectEventHandler(boxCallback);

            // vtkBoxWidget.On();
            // vtkBoxWidget.AddObserver((uint)vtkCommand.EventIds.KeyPressEvent, boxCallback, 1.0f);

            //vtkAffineRepresentation2D rep = vtkAffineRepresentation2D.New();
            //rep.SetBoxWidth(100);
            //rep.SetCircleWidth(75);
            //rep.SetAxesWidth(60);
            //rep.DisplayTextOn();
            ////rep.PlaceWidget()
            //// rep PlaceWidget $xMin $xMax $yMin $yMax $zMin $zMax

            //vtkAffineWidget vtkAffineWidget = vtkAffineWidget.New();
            //vtkAffineWidget.SetInteractor(iren);
            //vtkAffineWidget.SetRepresentation(rep);
            ////vtkAffineWidget.AddObserver(WidgetCallback, EndInteractionEvent, 1.0);




            return;
        }
        #endregion


        private void trackBarScaleX_Scroll(object sender, EventArgs e)
        {
            double x = (double)trackBarScaleX.Value / 100;
            txtScaleX.Text = x.ToString();
            SimModelVisualization.ScaleBody(selectedBodyProperty, x, null, null);
            SimModelVisualization.UpdateRenderer();
            //SimModelVisualization.Update2DRenderer(ren1Image1, ren1Image2);
            RenderAll();
        }
        private void trackBarScaleY_Scroll(object sender, EventArgs e)
        {
            double y = (double)trackBarScaleY.Value / 100;
            txtScaleY.Text = y.ToString();
            SimModelVisualization.ScaleBody(selectedBodyProperty, null, y, null);
            SimModelVisualization.UpdateRenderer();
           // SimModelVisualization.Update2DRenderer(ren1Image1, ren1Image2);
            RenderAll();
        }
        private void trackBarScaleZ_Scroll(object sender, EventArgs e)
        {
            double z = (double)trackBarScaleZ.Value / 100;
            txtScaleZ.Text = z.ToString();
            SimModelVisualization.ScaleBody(selectedBodyProperty, null, null, z);
            SimModelVisualization.UpdateRenderer();
            //SimModelVisualization.Update2DRenderer(ren1Image1, ren1Image2);
            RenderAll();
        }

       
        private void UC_3DModelingWorkpanel_Load(object sender, EventArgs e)
        {
            intitializeRenderers();

            

        }

        private void UC_3DModelingWorkpanel_SizeChanged(object sender, EventArgs e)
        {
            tableLayoutPanel1.Height = this.Height - 22;

            panel1Image.Height  = this.Height - 22;
            panel2Image.Height = this.Height - 22;

            tableLayoutPanelActiveTools.Height = this.Height - 22;
            tableLayoutPanelModelNavigator.Height = this.Height - 22;
         
         
            tableLayoutPanel1.Update();
        }



      
        private void cBshowIm2_CheckedChanged(object sender, EventArgs e)
        {
            if (cBshowIm2.Checked)
            {
                vtkImageActor2.SetOpacity(1);
                vtkImageActor2Dimage2.SetOpacity(1);

            }
            else
            {
                vtkImageActor2.SetOpacity(0);
                vtkImageActor2Dimage2.SetOpacity(0);
            }
            RenderAll();

        }

        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            ToolStripDropDownItem item = sender as ToolStripDropDownItem;
            ToolStripDropDown menu = item.DropDown;
            ToolStripItem ownerItem = item.OwnerItem;
            ToolStrip toolStrip = item.Owner;
            string temp = ownerItem.GetCurrentParent().Name;

            System.Windows.Forms.Control control = ((ContextMenuStrip)(((ToolStripMenuItem)ownerItem).Owner)).SourceControl;


            Updatewidth(control, 1);
        }
        private void Updatewidth(System.Windows.Forms.Control control, double scalefactor)
        {
            String name = control.Name;
            if (name == "lblEosSpace")
            {

                tableLayoutPanel1.Height = this.Height - 20;
                tableLayoutPanel1.Width = Convert.ToInt32(700* scalefactor);
                tableLayoutPanel1.Update();
            }
            if (name == "cBshowIm2")
            {

                panel2Image.Height = this.Height - 20;
                panel2Image.Width = Convert.ToInt32(308 * scalefactor); 
                panel2Image.Update();
            }
            if (name == "cBshowIm1")
            {

                panel1Image.Height = this.Height - 20;
                panel1Image.Width = Convert.ToInt32(308 * scalefactor);
                panel1Image.Update();
            }

        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            ToolStripDropDownItem item = sender as ToolStripDropDownItem;
            ToolStripDropDown menu = item.DropDown;
            ToolStripItem ownerItem = item.OwnerItem;
            ToolStrip toolStrip = item.Owner;
            string temp = ownerItem.GetCurrentParent().Name;

            System.Windows.Forms.Control control = ((ContextMenuStrip)(((ToolStripMenuItem)ownerItem).Owner)).SourceControl;


            Updatewidth(control, 0.5);
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            ToolStripDropDownItem item = sender as ToolStripDropDownItem;
            ToolStripDropDown menu = item.DropDown;
            ToolStripItem ownerItem = item.OwnerItem;
            ToolStrip toolStrip = item.Owner;
            string temp = ownerItem.GetCurrentParent().Name;

            System.Windows.Forms.Control control = ((ContextMenuStrip)(((ToolStripMenuItem)ownerItem).Owner)).SourceControl;


            Updatewidth(control, 0.75);
        }

        private void toolStripMenuItem5_Click(object sender, EventArgs e)
        {
            ToolStripDropDownItem item = sender as ToolStripDropDownItem;
            ToolStripDropDown menu = item.DropDown;
            ToolStripItem ownerItem = item.OwnerItem;
            ToolStrip toolStrip = item.Owner;
            string temp = ownerItem.GetCurrentParent().Name;

            System.Windows.Forms.Control control = ((ContextMenuStrip)(((ToolStripMenuItem)ownerItem).Owner)).SourceControl;


            Updatewidth(control, 1.25);
        }

        private void toolStripMenuItem6_Click(object sender, EventArgs e)
        {
            ToolStripDropDownItem item = sender as ToolStripDropDownItem;
            ToolStripDropDown menu = item.DropDown;
            ToolStripItem ownerItem = item.OwnerItem;
            ToolStrip toolStrip = item.Owner;
            string temp = ownerItem.GetCurrentParent().Name;

            System.Windows.Forms.Control control = ((ContextMenuStrip)(((ToolStripMenuItem)ownerItem).Owner)).SourceControl;


            Updatewidth(control, 1.5);
        }

        private void toolStripMenuItem7_Click(object sender, EventArgs e)
        {
            ToolStripDropDownItem item = sender as ToolStripDropDownItem;
            ToolStripDropDown menu = item.DropDown;
            ToolStripItem ownerItem = item.OwnerItem;
            ToolStrip toolStrip = item.Owner;
            string temp = ownerItem.GetCurrentParent().Name;

            System.Windows.Forms.Control control = ((ContextMenuStrip)(((ToolStripMenuItem)ownerItem).Owner)).SourceControl;


            Updatewidth(control, 1.75);
        }

        private void toolStripMenuItem8_Click(object sender, EventArgs e)
        {
            ToolStripDropDownItem item = sender as ToolStripDropDownItem;
            ToolStripDropDown menu = item.DropDown;
            ToolStripItem ownerItem = item.OwnerItem;
            ToolStrip toolStrip = item.Owner;
           string temp= ownerItem.GetCurrentParent().Name;

            System.Windows.Forms.Control control = ((ContextMenuStrip)(((ToolStripMenuItem)ownerItem).Owner)).SourceControl;


            Updatewidth(control, 2);
        }

        private void toolStripMenuItem9_Click(object sender, EventArgs e)
        {
            ToolStripDropDownItem item = sender as ToolStripDropDownItem;
            ToolStripDropDown menu = item.DropDown;
            ToolStripItem ownerItem = item.OwnerItem;
            ToolStrip toolStrip = item.Owner;
            string temp = ownerItem.GetCurrentParent().Name;

            System.Windows.Forms.Control control = ((ContextMenuStrip)(((ToolStripMenuItem)ownerItem).Owner)).SourceControl;


            Updatewidth(control, 0.5);
        }

        private void toolStripMenuItem10_Click(object sender, EventArgs e)
        {
            ToolStripDropDownItem item = sender as ToolStripDropDownItem;
            ToolStripDropDown menu = item.DropDown;
            ToolStripItem ownerItem = item.OwnerItem;
            ToolStrip toolStrip = item.Owner;
            string temp = ownerItem.GetCurrentParent().Name;

            System.Windows.Forms.Control control = ((ContextMenuStrip)(((ToolStripMenuItem)ownerItem).Owner)).SourceControl;


            Updatewidth(control, 0.75);
        }

        private void toolStripMenuItem11_Click(object sender, EventArgs e)
        {
            ToolStripDropDownItem item = sender as ToolStripDropDownItem;
            ToolStripDropDown menu = item.DropDown;
            ToolStripItem ownerItem = item.OwnerItem;
            ToolStrip toolStrip = item.Owner;
            string temp = ownerItem.GetCurrentParent().Name;

            System.Windows.Forms.Control control = ((ContextMenuStrip)(((ToolStripMenuItem)ownerItem).Owner)).SourceControl;


            Updatewidth(control, 1);
        }

        private void toolStripMenuItem12_Click(object sender, EventArgs e)
        {
            ToolStripDropDownItem item = sender as ToolStripDropDownItem;
            ToolStripDropDown menu = item.DropDown;
            ToolStripItem ownerItem = item.OwnerItem;
            ToolStrip toolStrip = item.Owner;
            string temp = ownerItem.GetCurrentParent().Name;

            System.Windows.Forms.Control control = ((ContextMenuStrip)(((ToolStripMenuItem)ownerItem).Owner)).SourceControl;


            Updatewidth(control, 1.25);
        }

        private void toolStripMenuItem13_Click(object sender, EventArgs e)
        {
            ToolStripDropDownItem item = sender as ToolStripDropDownItem;
            ToolStripDropDown menu = item.DropDown;
            ToolStripItem ownerItem = item.OwnerItem;
            ToolStrip toolStrip = item.Owner;
            string temp = ownerItem.GetCurrentParent().Name;

            System.Windows.Forms.Control control = ((ContextMenuStrip)(((ToolStripMenuItem)ownerItem).Owner)).SourceControl;


            Updatewidth(control, 1.50);
        }

        private void toolStripMenuItem14_Click(object sender, EventArgs e)
        {
            ToolStripDropDownItem item = sender as ToolStripDropDownItem;
            ToolStripDropDown menu = item.DropDown;
            ToolStripItem ownerItem = item.OwnerItem;
            ToolStrip toolStrip = item.Owner;
            string temp = ownerItem.GetCurrentParent().Name;

            System.Windows.Forms.Control control = ((ContextMenuStrip)(((ToolStripMenuItem)ownerItem).Owner)).SourceControl;


            Updatewidth(control, 1.75);
        }

        private void toolStripMenuItem15_Click(object sender, EventArgs e)
        {
            ToolStripDropDownItem item = sender as ToolStripDropDownItem;
            ToolStripDropDown menu = item.DropDown;
            ToolStripItem ownerItem = item.OwnerItem;
            ToolStrip toolStrip = item.Owner;
            string temp = ownerItem.GetCurrentParent().Name;

            System.Windows.Forms.Control control = ((ContextMenuStrip)(((ToolStripMenuItem)ownerItem).Owner)).SourceControl;


            Updatewidth(control, 2);
        }

        private void SetCameraParallel()
        {
            vtkCamera camera = ren1.GetActiveCamera();
            camera.ParallelProjectionOn();
            ren1.ResetCamera();
            RenderWindow.Render();

            camera = ren1Image1.GetActiveCamera();
            camera.ParallelProjectionOn();
            //ren1Image1.ResetCamera();
            ren1Image1.SetActiveCamera(camera);
            ren1Image1.Render();
            ren1Image1.GetRenderWindow().Render();

            camera = ren1Image2.GetActiveCamera();
            camera.ParallelProjectionOn();
            //ren1Image2.ResetCamera();
            ren1Image2.SetActiveCamera(camera);
            ren1Image2.Render();
            ren1Image2.GetRenderWindow().Render();
        }
        private void SetCameraOrthogonal()
        {
            vtkCamera camera = ren1.GetActiveCamera();
            camera.ParallelProjectionOff();
            ren1.ResetCamera();
            RenderWindow.Render();

            camera = ren1Image1.GetActiveCamera();
            camera.ParallelProjectionOff();
            camera.OrthogonalizeViewUp();
            //ren1Image1.ResetCamera();
            ren1Image1.SetActiveCamera(camera);
            ren1Image1.Render();
            ren1Image1.GetRenderWindow().Render();

            camera = ren1Image2.GetActiveCamera();
            camera.ParallelProjectionOff();
            camera.OrthogonalizeViewUp();
            //ren1Image2.ResetCamera();
            ren1Image2.SetActiveCamera(camera);
            ren1Image2.Render();
            ren1Image2.GetRenderWindow().Render();

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

            string dir = AppData.TempDir + "\\testsreenshot.tiff";

            writer.SetFileName(dir);
            RenderWindow.Render();
            writer.Write();

            writer.Dispose();
            w2i.Dispose();

        }



        private void parallelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetCameraParallel();
        }

        private void orthogonalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetCameraOrthogonal();
        }

        private void makeScreenshotToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CaptureImage();
        }

        private void frontalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AllignCameraLateral();
        }

        private void lateralToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AllignCameraFrontal();
        }

        private void bluedefaultToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ren1.SetBackground(0.2, 0.3, 0.4);
            ren1.Render();
            ren1.GetRenderWindow().Render();
            whiteToolStripMenuItem.Checked = false;
            bluedefaultToolStripMenuItem.Checked = true;
            blackToolStripMenuItem.Checked = false;
        }

        private void whiteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ren1.SetBackground(1, 1, 1);
            ren1.Render();
            ren1.GetRenderWindow().Render();
            whiteToolStripMenuItem.Checked = true;
            bluedefaultToolStripMenuItem.Checked = false;
            blackToolStripMenuItem.Checked = false;
        }

        private void blackToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ren1.SetBackground(0, 0, 0);
            ren1.Render();
            ren1.GetRenderWindow().Render();
            whiteToolStripMenuItem.Checked = false;
            bluedefaultToolStripMenuItem.Checked = false;
            blackToolStripMenuItem.Checked = true;
        }


        #region TreeView Interaction Methods
      

        private void btnDeleteNode_Click(object sender, EventArgs e)
        {
            RemoveCheckedNodes(treeView1.Nodes);
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

        public void ConeBeamCorrectionAll()
        {
            SimModelVisualization.updateAllConeBeam();
            RenderAll();
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

        private void btnFMCpopup_Click(object sender, EventArgs e)
        {

            if (selectedBodyProperty.objectName == null)
            { return; }


            frmFundamentalModelComponentProp frmFMCProp = new frmFundamentalModelComponentProp();

            //Pass data to search form
            frmFMCProp.Appdata = this.AppData;
            frmFMCProp.Subject = this.Subject;
            frmFMCProp.SQLDB = SQLDB;
            frmFMCProp.model = SimModelVisualization.osimModel;
            frmFMCProp.SimModelVisualization = SimModelVisualization;
            frmFMCProp.si = SimModelVisualization.si;
            frmFMCProp.osimBodyProp = selectedBodyProperty;
            frmFMCProp.ShowDialog();

            //Reset the colors of the bodies
            selectedBodyProperty._OsimGeometryPropertyList[0]._vtkActor.GetProperty().SetColor(selectedBodyProperty.colorR, selectedBodyProperty.colorG, selectedBodyProperty.colorB);
            selectedBodyProperty._OsimGeometryPropertyList[0]._vtkActor.GetProperty().SetOpacity(1);
            selectedBodyProperty.osimJointProperty.osimParentBodyProp._OsimGeometryPropertyList[0]._vtkActor.GetProperty().SetOpacity(1);
            selectedBodyProperty.osimJointProperty.osimParentBodyProp._OsimGeometryPropertyList[0]._vtkActor.GetProperty().SetColor(selectedBodyProperty.osimJointProperty.osimParentBodyProp.colorR, selectedBodyProperty.osimJointProperty.osimParentBodyProp.colorG, selectedBodyProperty.osimJointProperty.osimParentBodyProp.colorB);
            RenderWindow.Render();
        }

        private void btnIm1TranslateUp_Click(object sender, EventArgs e)
        {
            TranslateYwithPrecision(1);
        }

        private void btnIm2TranslateUp_Click(object sender, EventArgs e)
        {
            TranslateYwithPrecision(1);
        }

        private void btnIm1TranslateDown_Click(object sender, EventArgs e)
        {
            TranslateYwithPrecision(-1);
        }

        private void btnIm2TranslateDown_Click(object sender, EventArgs e)
        {
            TranslateYwithPrecision(-1);
        }

        private void btnRotX_MouseDown(object sender, MouseEventArgs e)
        {

        }

       
        private void treeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            //Go the the parent node which is defining the type of object. 

            TreeNode tn = e.Node;
            int nodeLevel = tn.Level;
            TreeNode tnParent = e.Node;
            SimModelVisualization.UnhighlightEverything();

            string selectedNodeText = tn.Text;
            if (nodeLevel == 0)   //Be carefull when multiple models are loaded.
            {
                OsimModelProperty osimProp = new OsimModelProperty();
                osimProp.ReadModelProperties(SimModelVisualization.osimModel);
                selectedobject = osimProp;
            }
            else
            {
                while (nodeLevel != 1)
                {
                    tnParent = tnParent.Parent;
                    nodeLevel = tnParent.Level;
                }
                string objectType = tnParent.Text;



                if (objectType == "Markers" && e.Node.Level != 1 && e.Node.Level != 2)
                {
                    Marker marker = SimModelVisualization.osimModel.getMarkerSet().get(selectedNodeText);

                    int index = SimModelVisualization.markerPropertyList.FindIndex(x => x.objectName == selectedNodeText);

                    OsimMakerProperty markerProp = SimModelVisualization.markerPropertyList[index];
                  
                    selectedobject = markerProp;
                    tn.ContextMenuStrip = SimModelVisualization.markerPropertyList[index].contextMenuStrip;
                    markerProp.HighlightMarker();
                }

                if (objectType == "Bodies" && e.Node.Level != 1 && e.Node.Level != 2)
                {
                    ABodyWasSelected((OsimBodyProperty)e.Node.Tag);
                    e.Node.ContextMenuStrip = ((OsimBodyProperty)e.Node.Tag).contextMenuStrip;

                }
                if (objectType == "Bodies" && e.Node.Level != 1 && e.Node.Level == 2)
                {
                    string groupname = e.Node.Text;

                    int index = SimModelVisualization.osimGroupElementList.FindIndex(x => x.groupName == groupname);
                    OsimGroupElement groupProp = SimModelVisualization.osimGroupElementList[index];
                    groupProp.SetContextMenuStrip();
                    selectedobject = groupProp;
                  
                    tn.ContextMenuStrip = groupProp.contextMenuStrip;
                }

                if (objectType == "Forces" && e.Node.Level != 1 && e.Node.Level != 2)
                {
                    Force force = SimModelVisualization.osimModel.getForceSet().get(selectedNodeText);
                    int index = SimModelVisualization.forcePropertyList.FindIndex(x => x.objectName == selectedNodeText);

                    OsimForceProperty forceProp = SimModelVisualization.forcePropertyList[index];
                    forceProp.vtkRenderwindow = RenderWindow;
                    selectedobject = forceProp;
                    tn.ContextMenuStrip = SimModelVisualization.forcePropertyList[index].contextMenuStrip;
                    forceProp.HighlightBody();

                }
                if (objectType == "Joints" && e.Node.Level == 2)
                {
                    if (selectedNodeText == "WorldFrameFixed")
                    { return; }
                    //Joint joint = SimModelVisualization.osimModel.getJointSet().get(selectedNodeText);
                    //joint.getName();
                    //Body body = joint.getBody();

                    //int index = SimModelVisualization.bodyPropertyList.FindIndex(x => x.objectName == body.getName());

                    //OsimBodyProperty bodyProp = SimModelVisualization.bodyPropertyList[index];
                    //bodyProp.vtkRenderwindow = RenderWindow;
                    OsimJointProperty tmmp = (OsimJointProperty)e.Node.Tag;
                    selectedobject = tmmp;
                    tn.ContextMenuStrip = tmmp.contextMenuStrip;
                   
                }
            }
            RenderAll();
        }


        private void ABodyWasSelected(OsimBodyProperty bodyProp)
        {
            selectedBodyProperty = bodyProp;
            // bodyProp.vtkRenderwindow = RenderWindow;
            selectedobject = bodyProp;
            UpdateCurrentBodyProperty(bodyProp);
           
            bodyProp.HighlightBody();
            SimModelVisualization.HideAllTranslucentBodies();
            bodyProp.ShowTranslucentProgrammatically();
            PnlBodyButtons.Visible = true;
            RenderAll();

            //This is a test for the boxwidget
            if (boxWidget!=null)
            {
                boxWidget.Off();

            }

            if (dragAndDropIsActive)
            {
                boxWidget = new vtkBoxWidget();

                boxWidget.SetInteractor(RenderWindow.GetInteractor());

                boxWidget.SetProp3D(bodyProp.assembly);
                boxWidget.SetPlaceFactor(1.05);
                boxWidget.PlaceWidget();

                boxWidget.InteractionEvt += new vtkObject.vtkObjectEventHandler(ExecuteHandle);

                boxWidget.On();
                boxWidget.RotationEnabledOff();
                //boxWidget.TranslationEnabledOff();
                boxWidget.ScalingEnabledOff();
                boxWidget.SetHandleSize(0);
                boxWidget.GetHandleProperty().FrontfaceCullingOff();
                boxWidget.GetHandleProperty().BackfaceCullingOff();
             //   RenderWindow.GetInteractor().Start();
      
                //end boxwidget test
            }
            RenderAll();
          
        }

        private void resetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Try to cast the sender to a ToolStripItem
            ToolStripItem menuItem = sender as ToolStripItem;
            if (menuItem != null)
            {
                // Retrieve the ContextMenuStrip that owns this ToolStripItem
                ContextMenuStrip owner = menuItem.Owner as ContextMenuStrip;
                if (owner != null)
                {
                    // Get the control that is displaying this context menu
                    System.Windows.Forms.Control sourceControl = owner.SourceControl;
                    ((TrackBar)sourceControl).Value = 30;
                    updateSlider1();
                    updateSlider2();
                }
            }
           

        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            updateSlider1();
        }

        private void updateSlider1()
        {
       vtkImageActor2Dimage1.SetPosition(EosSpace.PositionOriginImage1.X, EosSpace.PositionOriginImage1.Y, (double)trackBar1.Value / 100);
            renderWindowControlImage1.RenderWindow.Render();

        }
        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            updateSlider2();
        }

        private void updateSlider2()
        {

 vtkImageActor2Dimage2.SetPosition((double)trackBar2.Value / 100, EosSpace.PositionOriginImage2.Y, EosSpace.PositionOriginImage2.Z);
            renderWindowControlImage2.RenderWindow.Render();

        }

        private void btnIm1ARRight_Click(object sender, EventArgs e)
        {
            RotYwithPrecision(-1);
        }

        private void btnIm1ARLeft_Click(object sender, EventArgs e)
        {
            RotYwithPrecision(1);
        }

        private void btnIm2ARRight_Click(object sender, EventArgs e)
        {
            RotYwithPrecision(-1);
        }

        private void btnIm2ARLeft_Click(object sender, EventArgs e)
        {
            RotYwithPrecision(1);
        }

        private void btnIm1TranslateLeft_Click(object sender, EventArgs e)
        {
            TranslateZwithPrecision(1);
        }

        private void btnIm1TranslateRight_Click(object sender, EventArgs e)
        {
            TranslateZwithPrecision(-1);
        }

        private void btnIm2TranslateLeft_Click(object sender, EventArgs e)
        {
            TranslateXwithPrecision(1);
        }

        private void btnIm2TranslateRight_Click(object sender, EventArgs e)
        {
            TranslateXwithPrecision(-1);
        }

        private void btnIm1Left_Click(object sender, EventArgs e)
        {
            RotXwithPrecision(1);
        }

        private void btnIm1RotRight_Click(object sender, EventArgs e)
        {
            RotXwithPrecision(-1);
        }

        private void btnIm2RotLeft_Click(object sender, EventArgs e)
        {
            RotZwithPrecision(-1);
        }

        private void btnIm2RotRight_Click(object sender, EventArgs e)
        {
            RotZwithPrecision(1);
        }

        private void btnEqHisto_Click(object sender, EventArgs e)
        {

        }

        private void btnGrabAndReplace_Click(object sender, EventArgs e)
        {
            boxWidget.Off();
            iren.TerminateApp();
        }

    

        private bool dragAndDropIsActive = false;
        private void btnDragandDrop3D_Click(object sender, EventArgs e)
        {
            dragAndDropIsActive = !dragAndDropIsActive;
            if(dragAndDropIsActive)
            {

                btnDragandDrop3D.BackColor = Color.LightGreen;
                btnDragandDrop3D.Text = "ON";
                if (boxWidget!=null)
                {
                    boxWidget.On();
                }
               
            }
            else
            {
                btnDragandDrop3D.BackColor = Color.Firebrick;
                btnDragandDrop3D.Text = "OFF";
                if (boxWidget != null)
                {
                    boxWidget.Off();
                }
              
            }
           
        }

        private bool VisualizeJoints = true;
        private void btnvisualizeJoints_Click(object sender, EventArgs e)
        {
            VisualizeJoints = !VisualizeJoints;
            if (VisualizeJoints)
            {

                btnvisualizeJoints.BackColor = Color.LightGreen;
                btnvisualizeJoints.Text = "ON";
                SimModelVisualization.ShowAllJointRefFrames();
                SimModelVisualization.ShowAllJointSpheres();

            }
            else
            {
                btnvisualizeJoints.BackColor = Color.Firebrick;
                btnvisualizeJoints.Text = "OFF";
                SimModelVisualization.HideAllJointRefFrames();
                SimModelVisualization.HideAllJointSpheres();

            }
            RenderAll();
        }

        private void lblAdvanced_Click(object sender, EventArgs e)
        {
            llShowProperties.Visible = !llShowProperties.Visible;
        }

        private void label6_Click(object sender, EventArgs e)
        {
            if (selectedobject == null)
            {
                MessageBox.Show("First select a model component in the model navigator", "No component selected", MessageBoxButtons.OK);
                return;
            }
            using (frmComponentProperty frmComponentProperty = new frmComponentProperty())
            {
                frmComponentProperty.selectedobject = selectedobject;
                frmComponentProperty.ShowDialog();
            }
        }

        private void btnVisualizeBodyAxes_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Not possible in this version.", "bèta release", MessageBoxButtons.OK);
            return;
        }

        private void btnEquiproportionalJointMotion_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Not possible in this version.", "bèta release", MessageBoxButtons.OK);
            return;
        }

        private void flowLayoutPanel1_Scroll(object sender, ScrollEventArgs e)
        {
            tableLayoutPanel1.Update();
            RenderAll();
        }
    }
}
