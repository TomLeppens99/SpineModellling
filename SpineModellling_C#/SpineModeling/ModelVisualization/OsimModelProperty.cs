using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Drawing;
using System.Threading.Tasks;
using System.ComponentModel;
using OpenSim;

namespace SpineAnalyzer.ModelVisualization
{
    class OsimModelProperty
    {
        #region Declarations
        private string _objectName;
        private string _objectType;
        private string _credits;
        private string _publications;
        private string _lengthUnits;
        private string _forceUnits;
        private Model _model;
        #endregion

        #region Properties
        [CategoryAttribute("Model Properties"), DescriptionAttribute("Name of the musculoskeletal model"), ReadOnlyAttribute(false)]
        public string objectName
        {
            get { return _objectName; }
            set { _objectName = value;
                _model.setName(_objectName);}
        }

        [CategoryAttribute("Model Properties"), DescriptionAttribute("Type of the selected node."), ReadOnlyAttribute(true)]
        public string objectType
        {
            get { return _objectType; }
            set { _objectType = value; }
        }

        [CategoryAttribute("Model Properties"), DescriptionAttribute("Credits"), ReadOnlyAttribute(true)]
        public string credits
        {
            get { return _credits; }
            set { _credits = value;}
        }

        [CategoryAttribute("Model Properties"), DescriptionAttribute("Publications"), ReadOnlyAttribute(false)]
        public string publications
        {
            get { return _publications; }
            set { _publications = value;
                _model.setPublications(_publications);
            }
        }

        [CategoryAttribute("Model Properties"), DescriptionAttribute("LengthUnits"), ReadOnlyAttribute(true)]
        public string lengthUnits
        {
            get { return _lengthUnits; }
            set { _lengthUnits = value;}
        }
        [CategoryAttribute("Model Properties"), DescriptionAttribute("ForceUnits"), ReadOnlyAttribute(true)]
        public string forceUnits
        {
            get { return _forceUnits; }
            set { _forceUnits = value; }
        }
        #endregion

        #region Methods
        public void ReadModelProperties(Model model)
        {
            _model = model;
            _objectName = model.getName();
            _objectType = model.GetType().ToString();
            _credits = model.getCredits();
            _publications = model.getPublications();
            _lengthUnits = model.getLengthUnits().getLabel();
            _forceUnits = model.getForceUnits().getLabel(); 
        }
        #endregion

    }
}
