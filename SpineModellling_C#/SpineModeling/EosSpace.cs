using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kitware.VTK;

using SpineAnalyzer.DrawToolsRedux;
using DrawTools;
using System.Windows.Forms;

namespace SpineAnalyzer
{
   public class EosSpace
{
        #region Declarations
        //Need to be set
        public EosImage EOSImageA;
        public EosImage EOSImageB;

        public Position PositionSource1;
        public Position PositionSource2;
        public Position PatientPosition;
        public Position PositionOriginImage1;
        public Position PositionOriginImage2;
        public Orientation OrientationImage1;       
        public Orientation OrientationImage2;
        public AppData AppData;

        public List<SpaceObject> listSpaceObjects = new List<SpaceObject>();


        //VTK Stuff
        public vtkImageActor vtkImageActor1 = new vtkImageActor();
        public vtkImageActor vtkImageActor2 = new vtkImageActor();

        public DrawArea drawArea1;
        public DrawArea drawArea2;



        #endregion

        #region Methods
        public EosSpace(EosImage EOSImage1, EosImage EOSImage2)
        {
            this.EOSImageA = EOSImage1;
            this.EOSImageB = EOSImage2; 
        }

        public void CalculateEosSpace()
       {
            PositionSource1 = new Position(0, 0 ,- EOSImageA.DistanceSourceToIsocenter);
            PositionSource2 = new Position( - EOSImageB.DistanceSourceToIsocenter, 0, 0);
            PatientPosition = new Position(-(EOSImageB.DistanceSourceToIsocenter - EOSImageB.DistanceSourceToPatient ),0,- (EOSImageA.DistanceSourceToIsocenter - EOSImageA.DistanceSourceToPatient));
            PositionOriginImage1 = new Position(EOSImageA.Width / 2, 0, (EOSImageA.DistanceSourceToDetector - EOSImageA.DistanceSourceToIsocenter));
            PositionOriginImage2 = new Position ((EOSImageB.DistanceSourceToDetector - EOSImageB.DistanceSourceToIsocenter),0, -EOSImageB.Width/2);
            OrientationImage1 = new Orientation ( 0, 180, 0 );
            OrientationImage2 = new Orientation (0, 270, 0 ); //TODO: Orientatie Beelden automatiseren

            //Position origin aan te passen: X = width of image/2 and converted to mm! 
        }


        public void MakeEosSpaceNewMethod(vtkRenderer ren1)
        {
           
            this.CalculateEosSpace();

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
            PatientConeActor.SetPosition(this.PatientPosition.X, this.PatientPosition.Y, this.PatientPosition.Z);
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
            camActorF.SetPosition(this.PositionSource1.X, this.PositionSource1.Y, this.PositionSource1.Z);
            camActorF.GetProperty().SetColor(0.8, 0.2, 0.2);
            camActorF.PickableOff();

            vtkLODActor camActorL = vtkLODActor.New();
            camActorL.SetMapper(camMapper);
            camActorL.SetScale(2, 2, 2);
            camActorL.SetOrientation(0, 180, 0);
            camActorL.SetPosition(this.PositionSource2.X, this.PositionSource2.Y, this.PositionSource2.Z);
            camActorL.GetProperty().SetColor(0.8, 0.2, 0.2);
            camActorL.PickableOff();

            ren1.AddActor(camActorF);
            ren1.AddActor(camActorL);

            //Creating a surrounding EOS volume box
            vtkCubeSource eosBox = vtkCubeSource.New();
            eosBox.SetXLength(EOSImageB.DistanceSourceToDetector);
            eosBox.SetYLength(EOSImageA.Height); // De hoogte van het eerste beeld word gekozen. Normaal gelijke hoogte voor beide beelden.
            eosBox.SetZLength(EOSImageA.DistanceSourceToDetector);
            eosBox.SetCenter(-((EOSImageB.DistanceSourceToDetector / 2) - (EOSImageB.DistanceSourceToDetector - EOSImageB.DistanceSourceToIsocenter)), EOSImageA.Height / 2, -((EOSImageA.DistanceSourceToDetector / 2) - (EOSImageA.DistanceSourceToDetector - EOSImageA.DistanceSourceToIsocenter)));

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
            cam1.SetPosition(-2 * EOSImageA.DistanceSourceToDetector, 4 * EOSImageA.DistanceSourceToDetector, -2 * EOSImageB.DistanceSourceToDetector);
            //Creating axes arount the EOS volume
            vtkCubeAxesActor axes1 = vtkCubeAxesActor.New();
            axes1.SetBounds(-EOSImageB.DistanceSourceToIsocenter, EOSImageB.DistanceSourceToDetector - EOSImageB.DistanceSourceToIsocenter, 0, EOSImageA.Height, -EOSImageA.DistanceSourceToIsocenter, EOSImageA.DistanceSourceToDetector - EOSImageA.DistanceSourceToIsocenter);
            axes1.SetCamera(ren1.GetActiveCamera());
            axes1.SetXLabelFormat("%0.1f");
            axes1.SetYLabelFormat("%0.1f");
            axes1.SetZLabelFormat("%0.1f");
            axes1.SetFlyModeToOuterEdges();

            ren1.AddActor(eosBoxActor);
            ren1.AddActor(axes2Actor);
            ren1.AddViewProp(axes1);

            DisplayEOSin3DspaceNewMethod(ren1);

        }



        private void DisplayEOSin3DspaceNewMethod(vtkRenderer ren1)
        {
            //DICOMImage dcmImage1 = new DICOMImage(EOSImageA.dcm);
            try
            {
                System.IO.Directory.CreateDirectory(AppData.TempDir);
            }
            catch
            {
                MessageBox.Show("Directory could not be created", "Application Settings ERROR", MessageBoxButtons.OK);
                return;
            }
            string dir1 = AppData.TempDir + "\\tempF.png";
            //dcmImage1.SavePNG(dir1);

           

            vtkPNGReader vtkPNGReader1 = new vtkPNGReader();
            vtkPNGReader1.SetFileName(dir1); 
            vtkPNGReader1.SetDataSpacing(EOSImageA.PixelSpacingX, EOSImageA.PixelSpacingY, 1);
            EOSImageA.PNGReader = vtkPNGReader1;



            if (EOSImageA.imageRotated)    //TEST AIS
            {

                vtkImageFlip vtkImageFlip = new vtkImageFlip();
                vtkImageFlip.SetFilteredAxis(1); //y axis
                vtkImageFlip.SetInputConnection(vtkPNGReader1.GetOutputPort());
                vtkImageFlip.Update();
                EOSImageA.vtkImageFlip = vtkImageFlip;
                vtkImageActor1.SetInput(vtkImageFlip.GetOutput());
            }
            else
            {
                vtkImageActor1.SetInput(vtkPNGReader1.GetOutput());
            }
           
            vtkImageActor1.SetOrientation(this.OrientationImage1.X, this.OrientationImage1.Y, this.OrientationImage1.Z);
            vtkImageActor1.SetPosition(this.PositionOriginImage1.X, this.PositionOriginImage1.Y, this.PositionOriginImage1.Z);
            vtkImageActor1.PickableOff();






            //DICOMImage dcmImage2 = new DICOMImage(EOSImageB.dcm);
            string dir2 = AppData.TempDir + "\\tempL.png";

            try
            {
                System.IO.Directory.CreateDirectory(AppData.TempDir);
            }
            catch
            {
                MessageBox.Show("Directory could not be created", "Application Settings ERROR", MessageBoxButtons.OK);
                return;
            }

            //dcmImage2.SavePNG(dir2);
            

            vtkPNGReader vtkPNGReader2 = new vtkPNGReader();
            vtkPNGReader2.SetFileName(dir2); 
            vtkPNGReader2.SetDataSpacing(EOSImageB.PixelSpacingX, EOSImageB.PixelSpacingY, 1);
            EOSImageB.PNGReader = vtkPNGReader2;




            if (EOSImageB.imageRotated)    //TEST AIS
            {
                vtkImageFlip vtkImageFlip = new vtkImageFlip();
                vtkImageFlip.SetFilteredAxis(1); //y axis
                vtkImageFlip.SetInputConnection(vtkPNGReader2.GetOutputPort());
                vtkImageFlip.Update();
                EOSImageB.vtkImageFlip = vtkImageFlip;
                vtkImageActor2.SetInput(vtkImageFlip.GetOutput());
            }
            else
            {
                vtkImageActor2.SetInput(vtkPNGReader2.GetOutput());
            }
              


            vtkImageActor2.SetOrientation(this.OrientationImage2.X, this.OrientationImage2.Y, this.OrientationImage2.Z);
            vtkImageActor2.SetPosition(this.PositionOriginImage2.X, this.PositionOriginImage2.Y, this.PositionOriginImage2.Z);
            vtkImageActor2.PickableOff();

            ren1.AddActor(vtkImageActor1);
            ren1.AddActor(vtkImageActor2);
        }


        #endregion




        #region Projection Methods

        public double ConvertPixelToMeters(int pixelvalue, double pixelspacing)
        {
            return (double)pixelvalue * pixelspacing;
        }

        public double ConvertPixelToMeters(int pixelvalue, string imageLabel)
        {
            if(imageLabel =="A")
            {
                return (double)pixelvalue * Convert.ToDouble(EOSImageA.PixelSpacingX);
            }
            if (imageLabel == "B")
            {
                return (double)pixelvalue * Convert.ToDouble(EOSImageB.PixelSpacingX);
            }
            return 0;
        }

        public int ConvertMetersToPixels(double meters, double pixelspacing)
        {
            double temp = meters / pixelspacing;
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
            xP = (xR / (EOSImageA.DistanceSourceToIsocenter + zR)) * EOSImageA.DistanceSourceToIsocenter;
            zP = (zR / (EOSImageB.DistanceSourceToIsocenter + xR)) * EOSImageB.DistanceSourceToIsocenter;
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
            double slopeL = (0 - (-EOSImageA.DistanceSourceToIsocenter)) / (xP - 0);
            double slopeF = ((-zP - 0) / (0 - EOSImageB.DistanceSourceToIsocenter));

            xR = ((-slopeF * EOSImageB.DistanceSourceToIsocenter) - (-EOSImageA.DistanceSourceToIsocenter)) / (slopeL - slopeF);
            zR = slopeL * xR + (-EOSImageA.DistanceSourceToIsocenter);


        }




        #endregion



    }

}

