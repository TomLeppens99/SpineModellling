using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.IO;
using org.dicomcs;
using org.dicomcs.data;
using org.dicomcs.dict;
using org.dicomcs.net;
using org.dicomcs.scp;
using org.dicomcs.server;
using org.dicomcs.util;
using EvilDICOM;
using Kitware.VTK;

namespace SpineAnalyzer
{
    public class EosImage
    {
       public AppData AppData;
        private string _Directory;
        private float _DistanceSourceToIsocenter;
        private float _DistanceSourceToDetector;
        private float _DistanceSourceToPatient;
        private float _ImagerPixelSpacingX;
        private float _ImagerPixelSpacingY;
        private float _Height;
        private float _Width;
        private float _PixelSpacingX;
        private float _PixelSpacingY;
        private vtkPNGReader _PNGReader;
        public string _ImagePlane;
        public int _Columns;
        public int _Rows;
        public string _ImageComments = string.Empty;
        public int _FieldOfViewOrigin = 0;

        public DataTable DicomDataTable;
        public DICOMObject dcm;

        public Bitmap BMP; 

        BinaryReader file;

        public bool imageRotated = false;
        #region Methods
        public void ReadImage()
        {
            EvilDICOM.Element.Tag tagPATIENT_NAME = EvilDICOM.Helpers.TagHelper.PATIENT_NAME;
            EvilDICOM.Element.Tag tagROWS = EvilDICOM.Helpers.TagHelper.ROWS;
            EvilDICOM.Element.Tag tagCOLUMNS = EvilDICOM.Helpers.TagHelper.COLUMNS;
            EvilDICOM.Element.Tag tagDISTANCE_SOURCE_TO_ISOCENTER = EvilDICOM.Helpers.TagHelper.DISTANCE_SOURCE_TO_ISOCENTER;
            EvilDICOM.Element.Tag tagDISTANCE_SOURCE_TO_DETECTOR = EvilDICOM.Helpers.TagHelper.DISTANCE_SOURCE_TO_DETECTOR;
            EvilDICOM.Element.Tag tagDISTANCE_SOURCE_TO_PATIENT = EvilDICOM.Helpers.TagHelper.DISTANCE_SOURCE_TO_PATIENT;
            EvilDICOM.Element.Tag tagIMAGER_PIXEL_SPACING = EvilDICOM.Helpers.TagHelper.IMAGER_PIXEL_SPACING;
            EvilDICOM.Element.Tag tagPIXEL_SPACING = EvilDICOM.Helpers.TagHelper.PIXEL_SPACING;
            EvilDICOM.Element.Tag tagPATIENT_ORIENTATION = EvilDICOM.Helpers.TagHelper.PATIENT_ORIENTATION;
            EvilDICOM.Element.Tag FIELD_OF_VIEW_ORIGIN = EvilDICOM.Helpers.TagHelper.FIELD_OF_VIEW_ORIGIN;
            EvilDICOM.Element.Tag IMAGE_COMMENTS = EvilDICOM.Helpers.TagHelper.IMAGE_COMMENTS;
            //file = new BinaryReader(File.Open(_Directory, FileMode.Open, FileAccess.Read));
            //file.Read();
            //file.Close();
           
                if (!System.IO.File.Exists(_Directory))
                {
                    MessageBox.Show("File/Path can not be found. Check connection to network database.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    return;
                }


                var dcm = DICOMObject.Read(_Directory);
                var pName = dcm.FindFirst(tagPATIENT_NAME);
                var rows = dcm.FindFirst(tagROWS);
                _Rows = Int32.Parse(rows.DData.ToString());
                var columns = dcm.FindFirst(tagCOLUMNS);
                _Columns = Int32.Parse(columns.DData.ToString());

                _DistanceSourceToIsocenter = float.Parse(dcm.FindFirst(tagDISTANCE_SOURCE_TO_ISOCENTER).DData.ToString()) / 1000;
                _DistanceSourceToDetector = float.Parse(dcm.FindFirst(tagDISTANCE_SOURCE_TO_DETECTOR).DData.ToString()) / 1000;
                _DistanceSourceToPatient = float.Parse(dcm.FindFirst(tagDISTANCE_SOURCE_TO_PATIENT).DData.ToString()) / 1000;
                _ImagerPixelSpacingX = float.Parse(dcm.FindFirst(tagIMAGER_PIXEL_SPACING).DData.ToString()) * 1000;
                _ImagerPixelSpacingY = _ImagerPixelSpacingX;    //TODO: aanpassen voor niet uniforme resolutie
                _PixelSpacingX = float.Parse(dcm.FindFirst(tagPIXEL_SPACING).DData.ToString()) / 1000;
                _PixelSpacingY = _PixelSpacingX;   //TODO: aanpassen voor niet uniforme resolutie
                _ImagePlane = dcm.FindFirst(tagPATIENT_ORIENTATION).ToString();
                _Height = _PixelSpacingY * Rows;
                _Width = _PixelSpacingY * Columns;
           
           

        }

        public void GetDicomTagDataTable()
        {
            DicomDataTable = new DataTable();
            DicomDataTable.Columns.Add("GroupTag");
            DicomDataTable.Columns.Add("ElementTag");
            DicomDataTable.Columns.Add("TagDescription");
            DicomDataTable.Columns.Add("Value");

            dcm = DICOMObject.Read(this.Directory);
            dcm.AllElements.ForEach(element => DicomDataTable.Rows.Add(element.Tag.Group, element.Tag.Element, element.Tag.ToString(), element.DData));
        }
        
        public void ReadImageTagsToProperties()
        {
            GetDicomTagDataTable();

            if (dcm == null)
            {
                MessageBox.Show("File/Path can not be found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                return;
            }


            EvilDICOM.Element.Tag tagPATIENT_NAME = EvilDICOM.Helpers.TagHelper.PATIENT_NAME;
            EvilDICOM.Element.Tag tagROWS = EvilDICOM.Helpers.TagHelper.ROWS;
            EvilDICOM.Element.Tag tagCOLUMNS = EvilDICOM.Helpers.TagHelper.COLUMNS;
            EvilDICOM.Element.Tag tagDISTANCE_SOURCE_TO_ISOCENTER = EvilDICOM.Helpers.TagHelper.DISTANCE_SOURCE_TO_ISOCENTER;
            EvilDICOM.Element.Tag tagDISTANCE_SOURCE_TO_DETECTOR = EvilDICOM.Helpers.TagHelper.DISTANCE_SOURCE_TO_DETECTOR;
            EvilDICOM.Element.Tag tagDISTANCE_SOURCE_TO_PATIENT = EvilDICOM.Helpers.TagHelper.DISTANCE_SOURCE_TO_PATIENT;
            EvilDICOM.Element.Tag tagIMAGER_PIXEL_SPACING = EvilDICOM.Helpers.TagHelper.IMAGER_PIXEL_SPACING;
            EvilDICOM.Element.Tag tagPIXEL_SPACING = EvilDICOM.Helpers.TagHelper.PIXEL_SPACING;
            EvilDICOM.Element.Tag tagPATIENT_ORIENTATION = EvilDICOM.Helpers.TagHelper.PATIENT_ORIENTATION;
            EvilDICOM.Element.Tag FIELD_OF_VIEW_ORIGIN = EvilDICOM.Helpers.TagHelper.FIELD_OF_VIEW_ORIGIN;
            EvilDICOM.Element.Tag IMAGE_COMMENTS = EvilDICOM.Helpers.TagHelper.IMAGE_COMMENTS;

                  
            var pName = dcm.FindFirst(tagPATIENT_NAME);
            var rows = dcm.FindFirst(tagROWS);
            _Rows = Int32.Parse(rows.DData.ToString());
            var columns = dcm.FindFirst(tagCOLUMNS);
            _Columns = Int32.Parse(columns.DData.ToString());
            try
            {
                _DistanceSourceToIsocenter = float.Parse(dcm.FindFirst(tagDISTANCE_SOURCE_TO_ISOCENTER).DData.ToString()) / 1000;
                _DistanceSourceToDetector = float.Parse(dcm.FindFirst(tagDISTANCE_SOURCE_TO_DETECTOR).DData.ToString()) / 1000;
                _DistanceSourceToPatient = float.Parse(dcm.FindFirst(tagDISTANCE_SOURCE_TO_PATIENT).DData.ToString()) / 1000;
            }
            catch
            {
                MessageBox.Show("Spatial calibration parameters could not be extracted from the dicom tags. Please make sure you are working with the original EOS images. Secondary captures can't be used for 3D purposes." + Environment.NewLine + "Close and re-open to try again with a different set of images.");
                return;
            }
            _ImagerPixelSpacingX = float.Parse(dcm.FindFirst(tagIMAGER_PIXEL_SPACING).DData.ToString()) * 1000;
            _ImagerPixelSpacingY = _ImagerPixelSpacingX;    //TODO: aanpassen voor niet uniforme resolutie
            _PixelSpacingX = float.Parse(dcm.FindFirst(tagPIXEL_SPACING).DData.ToString()) / 1000;
            _PixelSpacingY = _PixelSpacingX;   //TODO: aanpassen voor niet uniforme resolutie
            _ImagePlane = dcm.FindFirst(tagPATIENT_ORIENTATION).ToString();
            _Height = _PixelSpacingY * Rows;
            _Width = _PixelSpacingY * Columns;
            _FieldOfViewOrigin = Int32.Parse(dcm.FindFirst(FIELD_OF_VIEW_ORIGIN).DData.ToString());
            _ImageComments = dcm.FindFirst(IMAGE_COMMENTS).ToString();
        }
      
        

        #endregion





        #region Properties
        public string Directory
        {
            get { return _Directory; }
            set { _Directory = value; }
        }

        public float DistanceSourceToIsocenter
        {
            get { return _DistanceSourceToIsocenter; }
            set { _DistanceSourceToIsocenter = value; }
        }

        public float DistanceSourceToDetector
        {
            get { return _DistanceSourceToDetector; }
            set { _DistanceSourceToDetector = value; }
        }

        public float DistanceSourceToPatient
        {
            get { return _DistanceSourceToPatient; }
            set { _DistanceSourceToPatient = value; }
        }

        public float ImagerPixelSpacingX
        {
            get { return _ImagerPixelSpacingX; }
            set { _ImagerPixelSpacingX = value; }
        }

        public float ImagerPixelSpacingY
        {
            get { return _ImagerPixelSpacingY; }
            set { _ImagerPixelSpacingY = value; }
        }

        public float PixelSpacingX
        {
            get { return _PixelSpacingX; }
            set { _PixelSpacingX = value; }
        }

        public float PixelSpacingY
        {
            get { return _PixelSpacingY; }
            set { _PixelSpacingY = value; }
        }

        public float Height 
        {
            get { return _Height; }
            set { _Height = value; }
        }

        public float Width
        {
            get { return _Width; }
            set { _Width = value; }
        }

        public int Columns
        {
            get { return _Columns; }
            set { _Columns = value; }
        }

        public int Rows
        {
            get { return _Rows; }
            set { _Rows = value; }
        }

        public string ImagePlane
        {
            get { return _ImagePlane; }
            set { _ImagePlane = value; }
        }

        public string ImageComments
        {
            get { return _ImageComments; }
            set { _ImageComments = value; }
        }

        public int FieldOfViewOrigin
        {
            get { return _FieldOfViewOrigin; }
            set { _FieldOfViewOrigin = value; }
        }


        public vtkImageFlip vtkImageFlip;

        public vtkPNGReader PNGReader
        {
            get { return _PNGReader; }
            set { _PNGReader = value; }
        }

    }
    #endregion
}
