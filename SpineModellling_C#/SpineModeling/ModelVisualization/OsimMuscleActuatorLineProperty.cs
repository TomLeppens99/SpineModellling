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
    public class OsimMuscleActuatorLineProperty
    {
        private vtkActor _muscleActor = vtkActor.New();
        public OsimControlPointProperty cP1;
        public OsimControlPointProperty cP2;
        public double _colorR = 1;
        public double _colorG = 0.01;
        public double _colorB = 0;
        vtkLineSource vtkLineSource = new vtkLineSource();
        vtkPolyDataMapper vtkPolyDataMapper = vtkPolyDataMapper.New();

        [Browsable(false)]
        public vtkActor muscleActor
        {
            get { return _muscleActor; }
            set { _muscleActor = value; }
        }


        public void MakeMuscleLineActor()
        {
            double[] pos1 = cP1.controlPointTransform.GetPosition(); 
            string name = cP1.objectName;

            vtkLineSource.SetPoint1(pos1[0], pos1[1], pos1[2]);
            

            double[] pos2 = cP2.controlPointTransform.GetPosition(); 
            vtkLineSource.SetPoint2(pos2[0], pos2[1], pos2[2]);
            
            //vtkTubeFilter vtkTubeFilter = new vtkTubeFilter();
            //vtkTubeFilter.SetInput(vtkLineSource.GetOutput());
            //vtkTubeFilter.UseDefaultNormalOff();
            
            //vtkTubeFilter.SetRadius(0.0010);
            //vtkTubeFilter.SetNumberOfSides(8);


            //vtkPolyDataMapper.SetInput(vtkTubeFilter.GetOutput());
            vtkPolyDataMapper.SetInput(vtkLineSource.GetOutput());

            _muscleActor.SetMapper(vtkPolyDataMapper);
            _muscleActor.GetProperty().SetDiffuseColor(_colorR, _colorG, _colorB); 
        }

        public void ScaleMuscleLineActor(double value)
        {
            _muscleActor.SetScale(value);
            //THIS DOES NOT WORK!
        }

        public void UpdateMuscleLineActor()
        {
            vtkTransform trans1  = (vtkTransform)cP1.controlPointActor.GetUserTransform();
            double[] pos1 = trans1.GetPosition();
            vtkLineSource.SetPoint1(pos1[0], pos1[1], pos1[2]);

            vtkTransform trans2 = (vtkTransform)cP2.controlPointActor.GetUserTransform();
            double[] pos2 = trans2.GetPosition();
            vtkLineSource.SetPoint2(pos2[0], pos2[1], pos2[2]);
            //vtkPolyDataMapper.Update();

            vtkPolyDataMapper.SetInput(vtkLineSource.GetOutput());

            _muscleActor.SetMapper(vtkPolyDataMapper);
            _muscleActor.GetProperty().SetDiffuseColor(_colorR, _colorG, _colorB);

        }
    }
}
