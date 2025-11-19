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


namespace SpineAnalyzer.ModelVisualization
{
    public class OsimJointCoordinateProperty
    {
        #region Declarations
        //OPENSIM
        private Body _parentBody;
        private Coordinate _coordinate;
        private Vec3 _axis;
        private Function _function;

        //SYSTEM
        private string _objectName = "WorldFrameFixed";
        private string _objectType;
        private double _defaultSpeed;
        private double _defaultValue;
        private double _rangeMax;
        private double _rangeMin;
        private bool _isClamped;
        private bool _isLocked;
        private int _coorNumber;


        #endregion

        #region Properties
        [CategoryAttribute("Joint Coordinate Properties"), DescriptionAttribute("Name of the Coordinate."), ReadOnlyAttribute(true)]
        public string objectName
        {
            get { return _objectName; }
            set { _objectName = value; }
        }

        [CategoryAttribute("Joint Coordinate Properties"), DescriptionAttribute("Type of the selected node"), ReadOnlyAttribute(true)]
        public string objectType
        {
            get { return _objectType; }
            set { _objectType = value; }
        }

        [CategoryAttribute("Joint Coordinate Properties"), DescriptionAttribute("OpenSim Object: Parent Body"), ReadOnlyAttribute(true)]
        public Body parentBody
        {
            get { return _parentBody; }
            set { _parentBody = value; }
        }

        [CategoryAttribute("Joint Coordinate Properties"), DescriptionAttribute("Default coordinate value"), ReadOnlyAttribute(true)]
        public double defaultValue
        {
            get { return _defaultValue; }
            set { _defaultValue = value; }
        }

        [CategoryAttribute("Joint Coordinate Properties"), DescriptionAttribute("Number of the coordiant property"), ReadOnlyAttribute(true)]
        public int coorNumber
        {
            get { return _coorNumber; }
            set { _coorNumber = value; }
        }

        [Browsable(false)]
        public Coordinate coordinate
        {
            get { return _coordinate; }
            set { _coordinate = value; }
        }

        [Browsable(false)]
        public Vec3 axis
        {
            get { return _axis; }
            set { _axis = value; }
        }
        
        #endregion


        #region Methods
        public void ReadJointCoordinate()
        {
            _objectName = _coordinate.getName();
            _parentBody = _coordinate.getJoint().getParentBody();
            _objectType = _coordinate.GetType().ToString();
            _defaultValue = _coordinate.getDefaultValue();
            _defaultSpeed = _coordinate.getDefaultSpeedValue();
            _rangeMax =  _coordinate.getRangeMax();
            _rangeMin = _coordinate.getRangeMin();
            _isClamped = _coordinate.get_clamped();
            _isLocked =  _coordinate.get_locked();
            //if (_coordinate.getJoint().GetType().ToString() == "CustomJoint")
            //    if (_parentBody.toString() != "ground")
            //    {
            //    CustomJoint custJoint = (CustomJoint)_coordinate.getJoint();
            //    SpatialTransform sptransf = custJoint.getSpatialTransform();
            //    TransformAxis transfAxis = sptransf.getTransformAxis(_coorNumber);
            //    _axis = transfAxis.getAxis();
            //    _function = transfAxis.getFunction();
            //}
   
        }

        #endregion
    }
}
