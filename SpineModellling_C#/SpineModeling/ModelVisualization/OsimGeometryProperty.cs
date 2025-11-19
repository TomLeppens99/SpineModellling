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
using System.IO;
using SpineAnalyzer.Acquisitions;

namespace SpineAnalyzer.ModelVisualization
{
    public class OsimGeometryProperty
    {
        #region Declarations

        //SYSTEM
        private string _objectName;
        private string _objectType;
        private string _geometryFile;
        private double _geomColorR = 1, _geomColorG = 1, _geomColorB = 1;
        private string _textureFile;
        private int _displayPreference;
        private double _opacity;
        private Color _geomColor = SystemColors.Control;
        private string _extension;
        private string _geometryDirAndFile;
        private int _IndexNumberOfGeometry = 0;
        public Boolean DLTpolydataHasBeenMade = false;
        private bool _loadedFromDatabase = false;
        public GeometryFiles geometryFileObject; 

        //OpenSim
        private DisplayGeometry _displayGeometry;
        private Transform _transform;
        private Vec3 _geomScaleFactors = new Vec3();
        public OpenSim.Model Model; 

        //VTK
        public vtkActor _vtkActor;
        public vtkActor _vtkActor1 = new vtkActor();
        public vtkActor _vtkActor2 = new vtkActor();

        public vtkPolyData _vtkPolyData = new vtkPolyData();
        public vtkPolyData _vtkPolyDataDLT = new vtkPolyData();

        #endregion


        #region Properties
        [CategoryAttribute("Geometry Properties"), DescriptionAttribute("Name of the geometry."), ReadOnlyAttribute(true)]
        public string objectName
        {
            get { return _objectName; }
            set { _objectName = value; }
        }

        [CategoryAttribute("Geometry Properties"), DescriptionAttribute("Type"), ReadOnlyAttribute(true)]
        public string objectType
        {
            get { return _objectType; }
            set { _objectType = value; }
        }

        [CategoryAttribute("Geometry Properties"), DescriptionAttribute("File"), ReadOnlyAttribute(true)]
        public string geometryFile
        {
            get { return _geometryFile; }
            set { _geometryFile = value; }
        }
        
        [CategoryAttribute("Geometry Properties"), DescriptionAttribute("Directory + File"), ReadOnlyAttribute(true)]
        public string geometryDirAndFile
        {
            get { return _geometryDirAndFile; }
            set { _geometryDirAndFile = value; }
        }

        [CategoryAttribute("Geometry Properties"), DescriptionAttribute("Geometry scale factors"), ReadOnlyAttribute(true)]
        public Vec3 geomScaleFactors
        {
            get { return _geomScaleFactors; }
            set { _geomScaleFactors = value; }
        }
        
        [CategoryAttribute("Geometry Properties"), DescriptionAttribute("File extension"), ReadOnlyAttribute(true)]
        public string extension
        {
            get { return _extension; }
            set { _extension = value; }
        }

        [CategoryAttribute("Geometry Properties"), DescriptionAttribute("Color R value"), ReadOnlyAttribute(true)]
        public double geomColorR
        {
            get { return _geomColorR; }
            set { _geomColorR = value; }
        }

        [CategoryAttribute("Geometry Properties"), DescriptionAttribute("Color G value"), ReadOnlyAttribute(true)]
        public double geomColorG
        {
            get { return _geomColorG; }
            set { _geomColorG = value; }
        }

        [CategoryAttribute("Geometry Properties"), DescriptionAttribute("Color B value"), ReadOnlyAttribute(true)]
        public double geomColorB
        {
            get { return _geomColorB; }
            set { _geomColorB = value; }
        }

        [CategoryAttribute("Geometry Properties"), DescriptionAttribute("Choose a color.")]
        public Color geomColor
        {
            get {
                             return _geomColor; }
            set
            {
                _geomColor = value;
                _geomColorR = (double)_geomColor.R / 255;
                _geomColorG = (double)_geomColor.G / 255;
                _geomColorB = (double)_geomColor.B / 255;
                if (_vtkActor != null)
                {
                    _vtkActor.GetProperty().SetColor(_geomColorR, _geomColorG, _geomColorB);
                }
            }
        }

        [CategoryAttribute("Geometry Properties"), DescriptionAttribute("Texture File"), ReadOnlyAttribute(true)]
        public string textureFile
        {
            get { return _textureFile; }
            set { _textureFile = value; }
        }

        [CategoryAttribute("Geometry Properties"), DescriptionAttribute("Display Preference"), ReadOnlyAttribute(false)]
        public int displayPreference
        {
            get { return _displayPreference; }
            set
            {
                _displayPreference = value;

                //TO DO: Dit nog uitzoeken??!!
                //DisplayGeometry.DisplayPreference number;
                //number.
                //_displayGeomety.setDisplayPreference(();
            }
        }

        [CategoryAttribute("Geometry Properties"), DescriptionAttribute("Opacity"), ReadOnlyAttribute(false)]
        public double opacity
        {
            get { return _opacity; }
            set
            {
                _opacity = value;
                _displayGeometry.setOpacity(value); 
            }
        }


        [CategoryAttribute("Geometry Properties"), DescriptionAttribute("OpenSim Object"), ReadOnlyAttribute(true)]
        public DisplayGeometry displayGeometry
        {
            get { return _displayGeometry; }
            set { _displayGeometry = value;
                Model.updDisplayer();
            }
        }

        [CategoryAttribute("Geometry Properties"), DescriptionAttribute("Loaded From Database"), ReadOnlyAttribute(true)]
        public bool loadedFromDatabase
        {
            get { return _loadedFromDatabase; }
            set
            {
                _loadedFromDatabase = value;
            }
        }

        [CategoryAttribute("Geometry Properties"), DescriptionAttribute("Geometry Transform"), ReadOnlyAttribute(true)]
        public Transform internalTransform 
        {
            get { return _transform; }
            set { _transform = value; }
        }

        [CategoryAttribute("Geometry Properties"), DescriptionAttribute("Geometry Transform VTK"), ReadOnlyAttribute(true)]
        public vtkTransform internalTransformVTK
        {
            get { return (vtkTransform)_vtkActor.GetUserTransform(); }
            set { _vtkActor.SetUserTransform(value); }
        }



        [CategoryAttribute("Geometry Properties"), DescriptionAttribute("Index number"), ReadOnlyAttribute(true)]
        public int  IndexNumberOfGeometry
        {
            get { return _IndexNumberOfGeometry; }
            set { _IndexNumberOfGeometry = value; }
        }

        #endregion


        #region Methods
        public void ReadGeometry(DisplayGeometry geom)
        {
            _displayGeometry = geom;
            _objectName = geom.getGeometryFile();
            _geometryFile = geom.getGeometryFile();
            _extension = Path.GetExtension(_geometryFile);
            _textureFile = geom.getTextureFile();
            _transform = geom.getTransform();
            _objectType = geom.GetType().ToString();
            _opacity = geom.getOpacity();
            _geomScaleFactors = geom.getScaleFactors();
            _geomColorR = geom.getColor().get(0);
            _geomColorG = geom.getColor().get(1);
            _geomColorB = geom.getColor().get(2);
            

        }

        public void WriteGeometry(string newFile)
        {
            _displayGeometry.setGeometryFile(newFile);
            Model.updBodySet();
            Model.updDisplayer();

        }
        public void MakeVTKActor()
        {
            vtkTransform vtkTransform = vtkTransform.New();

            vtkTransformFilter vtkTransformFilterOrignal = vtkTransformFilter.New();
            vtkTransformFilter vtkTransformFilterDeformed = vtkTransformFilter.New();

            if (_extension == ".vtp")
            {
                vtkXMLPolyDataReader polyDataReaderOriginal = new vtkXMLPolyDataReader();
                polyDataReaderOriginal.SetFileName(_geometryDirAndFile);
                vtkTransformFilterOrignal.SetInputConnection(polyDataReaderOriginal.GetOutputPort());
                //bodyProp.AddPolyDataToListOriginals(polyDataReaderOriginal.GetOutput());

                _vtkPolyData = polyDataReaderOriginal.GetOutput();

                vtkXMLPolyDataReader polyDataReaderDeformed = new vtkXMLPolyDataReader();
                polyDataReaderDeformed.SetFileName(_geometryDirAndFile);
                vtkTransformFilterDeformed.SetInputConnection(polyDataReaderDeformed.GetOutputPort());
                //bodyProp.AddPolyDataToListDeformed(polyDataReaderDeformed.GetOutput());
            }

            if (_extension == ".stl")
            {
                vtkSTLReader polyDataReader = new vtkSTLReader();
                polyDataReader.SetFileName(_geometryDirAndFile);
                vtkTransformFilterOrignal.SetInputConnection(polyDataReader.GetOutputPort());
                //bodyProp.AddPolyDataToListDeformed(polyDataReader.GetOutput());
                //bodyProp.AddPolyDataToListOriginals(polyDataReader.GetOutput());
                _vtkPolyData = polyDataReader.GetOutput();
            }

            if (_extension == ".obj")
            {
                vtkOBJReader polyDataReader = new vtkOBJReader();
                polyDataReader.SetFileName(_geometryDirAndFile);
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
            axes.SetScaleFactor(0.06);


            //vtkVectorText textLabel = vtkVectorText.New();
            //textLabel.SetText(fileName);
            //vtkFollower follower = vtkFollower.New();


            vtkAppendPolyData vtkAppendPolyData = vtkAppendPolyData.New();
            vtkAppendPolyData.AddInputConnection(vtkTransformFilterOrignal.GetOutputPort());
            vtkAppendPolyData.AddInputConnection(axes.GetOutputPort());    //COMMENT THIS IF YOU WANT TO HIDE THE AXES

            vtkPolyDataMapper vtkPolyDataMapper = vtkPolyDataMapper.New();
            vtkPolyDataMapper.SetInputConnection(vtkAppendPolyData.GetOutputPort());


            _vtkActor = new vtkActor();
            _vtkActor.SetMapper(vtkPolyDataMapper);

            _vtkActor.GetProperty().SetColor(_geomColorR, _geomColorG, _geomColorB);
            _vtkActor.SetScale(_geomScaleFactors.get(0), _geomScaleFactors.get(1), _geomScaleFactors.get(2));

        }

        public void Make2Dactors()
        {
            //vtkAxes axes = vtkAxes.New();
            //axes.SetOrigin(0, 0, 0);
            //axes.SetScaleFactor(0.1);


            vtkTransformFilter vtkTransformFilter = new vtkTransformFilter();

            if (!DLTpolydataHasBeenMade)
            {
                vtkTransformFilter.SetInput(_vtkPolyData);
            } else
            {
                vtkTransformFilter.SetInput(_vtkPolyDataDLT);
            }

            vtkTransform vtkTransform = new vtkTransform();
            vtkTransformFilter.SetTransform(vtkTransform);

            //vtkVectorText textLabel = vtkVectorText.New();
            //textLabel.SetText(fileName);
            //vtkFollower follower = vtkFollower.New();


            //vtkAppendPolyData vtkAppendPolyData = vtkAppendPolyData.New();
            vtkAppendPolyData vtkAppendPolyData = new vtkAppendPolyData();
            vtkAppendPolyData.AddInputConnection(vtkTransformFilter.GetOutputPort());
            //vtkAppendPolyData.AddInputConnection(axes.GetOutputPort());    //COMMENT THIS IF YOU WANT TO HIDE THE AXES

            vtkPolyDataMapper vtkPolyDataMapper = vtkPolyDataMapper.New();
            vtkPolyDataMapper.SetInputConnection(vtkAppendPolyData.GetOutputPort());

            if (!DLTpolydataHasBeenMade)
            {
                _vtkActor1.SetMapper(vtkPolyDataMapper);
                _vtkActor1.GetMapper().Update();
                _vtkActor1.GetMapper().Modified();
                //_vtkActor1.GetProperty().EdgeVisibilityOn();
                _vtkActor1.GetProperty().SetOpacity(0.1);
                _vtkActor1.GetProperty().SetColor(1, 0, 0);
                _vtkActor1.SetScale(_geomScaleFactors.get(0), _geomScaleFactors.get(1), _geomScaleFactors.get(2));
                //_vtkActor1.Modified();

            }
            else
            {
                _vtkActor1.GetMapper().SetInputConnection(vtkAppendPolyData.GetOutputPort());
                //_vtkActor2.GetMapper().SetInputConnection(vtkAppendPolyData.GetOutputPort());
            }
        }
        #endregion
    }
}
