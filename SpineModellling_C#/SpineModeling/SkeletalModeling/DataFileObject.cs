using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MathNet.Numerics;
using MathNet.Numerics.Data.Matlab;
using MathNet.Numerics.Data.Text;
using MathNet.Numerics.Statistics;
using MathNet.Numerics.Data;
using MathNet.Numerics.LinearAlgebra;
using ADODB;
using System.Globalization;
using System.Windows.Forms.DataVisualization.Charting;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace SpineAnalyzer.Simulator
{
    public class DataFileObject
    {
        //Declarations
        public string _DataFileName = string.Empty;
        public string _DataFileNameShort = string.Empty;
        public string _DataFileDirectory = string.Empty;
        public int _DataIndex = 0;


        //For the Dynamic Landmarks
        public List<DataTable> TransformedLandmarkList = new List<DataTable>();




        public List<string> _HeaderNames = new List<string>();

        public List<Series> AbsoluteSeriesList = new List<Series>();
        public DataTable AbsoluteDt = new DataTable();
        public MathNet.Numerics.LinearAlgebra.Matrix<double> m;

        public List<Series> RelativeSeriesList = new List<Series>();
        public DataTable RelativeDt = new DataTable();
        public MathNet.Numerics.LinearAlgebra.Matrix<double> relM;

        public List<string> ParameterUnits = new List<string>();
        public List<int> parmIsNotZeroList = new List<int>();

        private double _perturb8_X = 0;
        private double _perturb8_Y = 0;
        private double _perturb8_Z = 0;

        public double _TimeAverageParameterAverageError = 0;
        public double _MaximalErrorAngle = 0;
        public double _MinimalErrorAngle = 0;
        public double _MinimalErrorDist = 0;
        public int indexMaxParameterErrorAngles = 0;
        public int indexMinParameterErrorAngles = 0;
        public double _MaximalErrorDist = 0;
        public int indexMaxParameterErrorDist = 0;
        public int indexMinParameterErrorDist = 0;

        public List<double> _paramMaximals = new List<double>();
        public List<int> _paramMaximalsIndex = new List<int>();
        public List<int> _paramMinimalsIndex = new List<int>();
        
        public List<double> _paramMinimals = new List<double>();
        public List<double> _paramAverages = new List<double>();
        public List<double> _AbsparamAverages = new List<double>();
        public List<double> _paramRomEndStart = new List<double>();


        int _nbCol = 0;
        int _nbRow = 0;



 
        #region Properties
        [CategoryAttribute("Data File Properties"), DescriptionAttribute("Name of the file."), ReadOnlyAttribute(true)]
        public string DataFileName
        {
            get { return _DataFileName; }
            set { _DataFileName = value; }
        }
        [CategoryAttribute("Data File Properties"), DescriptionAttribute("Short name of the file."), ReadOnlyAttribute(true)]
        public string DataFileNameShort
        {
            get { return _DataFileNameShort; }
            set { _DataFileNameShort = value; }
        }
        [CategoryAttribute("Data File Properties"), DescriptionAttribute("Directory of the file."), ReadOnlyAttribute(true)]
        public string DataFileDirectory
        {
            get { return _DataFileDirectory; }
            set { _DataFileDirectory = value; }
        }

        [CategoryAttribute("Data File Properties"), DescriptionAttribute("Number of Rows."), ReadOnlyAttribute(true)]
        public int nbRow
        {
            get { return _nbRow; }
            set { _nbRow = value; }
        }


        [CategoryAttribute("Data File Properties"), DescriptionAttribute("Number of Columns."), ReadOnlyAttribute(true)]
        public int nbCol
        {
            get { return _nbCol; }
            set { _nbCol = value; }
        }

        [CategoryAttribute("Data File Properties"), DescriptionAttribute("X perturbation (mm)."), ReadOnlyAttribute(true)]
        public double perturb8_X
        {
            get { return _perturb8_X; }
            set { _perturb8_X = value; }
        }

        [CategoryAttribute("Data File Properties"), DescriptionAttribute("Y perturbation (mm)."), ReadOnlyAttribute(true)]
        public double perturb8_Y
        {
            get { return _perturb8_Y; }
            set { _perturb8_Y = value; }
        }

        [CategoryAttribute("Data File Properties"), DescriptionAttribute("Z perturbation (mm)."), ReadOnlyAttribute(true)]
        public double perturb8_Z
        {
            get { return _perturb8_Z; }
            set { _perturb8_Z = value; }
        }

        #endregion

        ///Methods

        public void Read()
        {
            _HeaderNames.Clear();
            Read2File();
            m = DenseOfDataTable(AbsoluteDt);

            if (_DataFileName.Contains("P3RTRB8_"))
            {
                try
                {
                    DeducePerturbationParamaters();
                }
                catch
                {
                    _DataFileNameShort = _DataFileName;
                }
            }
            else
            {
                _DataFileNameShort = _DataFileName; 
            }

           
            _nbCol = m.ColumnCount;
            _nbRow = m.RowCount;
            DeduceUnits();
            FindZeroColumns();
        }

        public void MakeSeries()
        { 
            Read2Series(AbsoluteSeriesList, AbsoluteDt);
        }

        private void FindZeroColumns()
        {
            parmIsNotZeroList.Clear();

            for (int i = 0; i < _nbCol; i++)
            {
               int numberNotZero = m.Column(i).EnumerateNonZero().Count<double>();
                if(numberNotZero ==0)
                {
                    parmIsNotZeroList.Add(0);
                }
                else
                {
                    parmIsNotZeroList.Add(1);
                }
            }

        }


        private void DeduceUnits()
        {
            foreach (string paramName in _HeaderNames)
            {
                string paramNameLower = paramName.ToLower();
                if (paramNameLower.Contains("time"))
                    { ParameterUnits.Add("Seconds"); }
                else {
                    if (paramNameLower.Contains("tilt") || paramNameLower.Contains("slope") || paramNameLower.Contains("SPI") || paramNameLower.Contains("incidence") || paramNameLower.Contains("list") || paramNameLower.Contains("rot") || paramNameLower.Contains("fe") || paramNameLower.Contains("lb") || paramNameLower.Contains("ar") || paramNameLower.Contains("r_x") || paramNameLower.Contains("r_y") || paramNameLower.Contains("r_z") || paramNameLower.Contains("rot") || paramNameLower.Contains("elv") || paramNameLower.Contains("flex") || paramNameLower.Contains("adduction") || paramNameLower.Contains("angle") || paramNameLower.Contains("dev") || paramNameLower.Contains("sup") || paramNameLower.Contains("jnt_r1") || paramNameLower.Contains("jnt_r2") || paramNameLower.Contains("jnt_r3"))
                    { ParameterUnits.Add("Degrees"); }
                    else { ParameterUnits.Add("Meter"); }
                }
            }
        }
        public void CalculateAllParameters(bool Relative)
        {
            if(Relative)
            {
                if (relM == null)
                { return; }
                CalculateParamMaximals(relM, Relative);
                CalculateParamMinimals(relM, Relative);
                CalculateParamAverages(relM);
                CalculateParameterROM_end_start(relM);
            }
            else
            {
                if (m == null)
                { return; }
                CalculateParamMaximals(m, Relative);
                CalculateParamMinimals(m, Relative);
                CalculateParamAverages(m);
                CalculateParameterROM_end_start(m);
            }
            
        }

        public void MakeRelativeSeries(bool makeCharts)
        {
            if (makeCharts)
            {
                Read2SeriesFromMatrix(RelativeSeriesList, relM);
            }

        }
        private void DeducePerturbationParamaters()
        {
           
            //Find the code
            string[] tokens = _DataFileName.Split(new[] { "P3RTRB8_", "_ik" }, StringSplitOptions.None);
            _DataFileNameShort = tokens[1];
            string[] tokens2 = tokens[1].Split(new[] { "_" }, StringSplitOptions.None);
            //Extract X, Y, Z.
            tokens2[0] = tokens2[0].Replace(',', '.');
            tokens2[1] = tokens2[1].Replace(',', '.');
            tokens2[2] = tokens2[2].Replace(',', '.');

            _perturb8_X = Convert.ToDouble(tokens2[0], CultureInfo.InvariantCulture);
            _perturb8_Y = Convert.ToDouble(tokens2[1], CultureInfo.InvariantCulture);
            _perturb8_Z = Convert.ToDouble(tokens2[2], CultureInfo.InvariantCulture);
        }

        private void CalculateParamMaximals(MathNet.Numerics.LinearAlgebra.Matrix<double> matrix, bool Relative)
        {
            _paramMaximals.Clear();
            _paramMaximalsIndex.Clear();

            int tempIndex =0;
            indexMaxParameterErrorAngles = 0;
            for (int i =0; i< _nbCol; i++)
            {
                if (Relative)
                {
                    _paramMaximals.Add(MaxAbsValueColumn(i, matrix, out tempIndex));
                }
                else
                {
                    _paramMaximals.Add(MaxValueColumn(i, matrix, out tempIndex));

                }
                _paramMaximalsIndex.Add(tempIndex);
            }
            
             FindMaxList(_paramMaximals, "Degrees", out indexMaxParameterErrorAngles, out _MaximalErrorAngle, Relative);
             FindMaxList(_paramMaximals, "Meter", out indexMaxParameterErrorDist, out _MaximalErrorDist, Relative);
        }

        private void CalculateParamMinimals(MathNet.Numerics.LinearAlgebra.Matrix<double> matrix, bool Relative)
        {
            _paramMinimals.Clear();
            _paramMinimalsIndex.Clear();

            int tempIndex = 0;

            indexMinParameterErrorAngles = 0;
            for (int i = 0; i < _nbCol; i++)
            {
                if (Relative)
                {
                    _paramMinimals.Add(MinAbsValueColumn(i, matrix, out tempIndex));
                }
                else
                {
                    _paramMinimals.Add(MinValueColumn(i, matrix, out tempIndex));
                }

                _paramMinimalsIndex.Add(tempIndex);

            }
            FindMinList(_paramMinimals, "Degrees", out indexMinParameterErrorAngles, out _MinimalErrorAngle, Relative);
            FindMinList(_paramMinimals, "Meter", out indexMinParameterErrorDist, out _MinimalErrorDist, Relative);
        }


        private int frameWidth = 3;
        private void CalculateParameterROM_end_start(MathNet.Numerics.LinearAlgebra.Matrix<double> matrix)
        {
            _paramRomEndStart.Clear();

            for (int i = 0; i < _nbCol; i++)
            {

                _paramRomEndStart.Add(Math.Abs(calculateAverageFromEnd(frameWidth, matrix.Column(i)) - calculateAverageFromStart(frameWidth, matrix.Column(i))));
                  
                    }

        }


        private double calculateAverageFromEnd(int frameWidth,  Vector<double> p)
        {
            double tempresult = 0;

            for (int i = 0; i < frameWidth; i++)
            {
                tempresult += (p[p.Count - 1 - i]);

            }


            return tempresult / frameWidth;
        }

        private double calculateAverageFromStart(int frameWidth, Vector<double> p)
        {
            double tempresult = 0;

            for (int i = 0; i < frameWidth; i++)
            {
                tempresult += (p[i]);

            }


            return tempresult / frameWidth;
        }



        private void CalculateParamAverages(MathNet.Numerics.LinearAlgebra.Matrix<double> matrix)
        {
            _paramAverages.Clear();
            for (int i = 0; i < _nbCol; i++)
            {
                _paramAverages.Add(AverageValueColomn(i, matrix));
            }
            _AbsparamAverages.Clear();
            for (int i = 0; i < _nbCol; i++)
            {
                _AbsparamAverages.Add(AbsAverageValueColomn(i, matrix));
            }

        }

        private void FindMaxList(List<double> list, string unit, out int index, out double max, bool Relative)
        {
            max = 0;
            index = 0;
            for(int i =0; i<list.Count; i++)
            {
                if (ParameterUnits[i] == unit)
                {
                    double value = list[i];

                    if (Relative)
                    {
                        if (Math.Abs(value) > max)
                        {
                            index = i;
                            max = value;
                        }
                    }
                    else
                    {
                        if (value> max)
                        {
                            index = i;
                            max = value;
                        }

                    }
                }
            }
          

        }

        private void FindMinList(List<double> list, string unit, out int index, out double min, bool Relative)
        {
            min = 0;
            index = 0;
            for (int i = 0; i < list.Count; i++)
            {
                if (ParameterUnits[i] == unit)
                {
                    double value = list[i];
                    if (Relative)
                    {
                        if (Math.Abs(value) < min)
                        {
                            index = i;
                            min = value;
                        }
                    }
                    else
                    {
                        if (value < min)
                        {
                            index = i;
                            min = value;
                        }
                    }
                }
            }


        }

        private double MaxAbsValueColumn(int columIndex, MathNet.Numerics.LinearAlgebra.Matrix<double> matrix, out int indexout)
        {
            indexout = (matrix.Column(columIndex)).MaximumIndex();
            return Math.Round(matrix.Column(columIndex).AbsoluteMaximum(),7);
            



        }
        private double MaxValueColumn(int columIndex, MathNet.Numerics.LinearAlgebra.Matrix<double> matrix, out int indexout)
        {
            indexout = (matrix.Column(columIndex)).MaximumIndex();
            return Math.Round(matrix.Column(columIndex).Maximum(), 7);

        }
        private double MinAbsValueColumn(int columIndex, MathNet.Numerics.LinearAlgebra.Matrix<double> matrix, out int indexout)
        {
            indexout = (matrix.Column(columIndex)).MinimumIndex();
            return Math.Round(matrix.Column(columIndex).AbsoluteMinimum(), 7);

        }
        private double MinValueColumn(int columIndex, MathNet.Numerics.LinearAlgebra.Matrix<double> matrix, out int indexout)
        {
             indexout = (matrix.Column(columIndex)).MinimumIndex();
            return Math.Round(matrix.Column(columIndex).Minimum(), 7);

        }
        private double AverageValueColomn(int columIndex, MathNet.Numerics.LinearAlgebra.Matrix<double> matrix)
        {
            return Math.Round(matrix.Column(columIndex).Average(),7);

        }

        private double AbsAverageValueColomn(int columIndex, MathNet.Numerics.LinearAlgebra.Matrix<double> matrix)
        {
            double value = 0;
            for (int i = 0; i < matrix.RowCount; i++)
            {
                value += Math.Abs(matrix[i, columIndex]);
            }
            return Math.Round(value / (double)matrix.RowCount,7);

        }

        private string GetLine(string fileName, int line)
        {
            using (var sr = new System.IO.StreamReader(fileName))
            {
                for (int i = 1; i < line; i++)
                    sr.ReadLine();
                return sr.ReadLine();
            }
        }

        public Series getSpecifiedSerieFromName(string name, List<Series> seriesList)
        {
            
            int index = seriesList.FindIndex(x => x.Name == name);
            return seriesList[index];
        }
        public int getSpecifiedIndexFromParameterName(string name)
        {

            return _HeaderNames.FindIndex(x => x == name);
           
        }
        private void Read2Series(List<Series> seriesList, DataTable Dt)
        {
            seriesList.Clear();
            for (int i = 0; i < _HeaderNames.Count; i++)
            {
                if (parmIsNotZeroList[i] == 1)
                {
                    MakeSerie(_DataFileNameShort, _HeaderNames[i], seriesList, Dt);
                }
            }

        }

        private void Read2SeriesFromMatrix(List<Series> seriesList, Matrix<double> Mat)
        {
            RelativeSeriesList.Clear();
            for (int i = 0; i < _HeaderNames.Count; i++)
            {
                if (parmIsNotZeroList[i] == 1)
                {
                    MakeSerieFromMatrix(_DataFileNameShort, _HeaderNames[i], i, RelativeSeriesList, Mat);
                }
            }

        }

        public System.IO.StreamReader file;
        public string parameterNamesLine;
        public List<string> DataLines = new List<string>();
        private void Read2File()
        {
            DataLines.Clear();
            string location = _DataFileDirectory;

            file = new System.IO.StreamReader(location);


            //SKIP ALL HEADER
            int k = 0;

            while (GetLine(location, k).ToString().Trim() != "endheader")
            {
                file.ReadLine();
                k += 1;
            }
            k += 1;

            //Skip any empty lines between the endheader and the actual data.
            while (GetLine(location, k).ToString().Trim() == "")
            {
                file.ReadLine();
                k += 1;
            }


            //After having found the headers:
            parameterNamesLine = file.ReadLine();
            string[] columnnames = parameterNamesLine.Split('\t');

           
            foreach (string c in columnnames)
            {
                string d = c.Trim();
                if (!string.IsNullOrEmpty(d))
                {
                    AddParameter(d);
                    AbsoluteDt.Columns.Add(d);
                }
            }
            string newline;
            while ((newline = file.ReadLine()) != null)
            {
                DataLines.Add(newline);
                   DataRow dr = AbsoluteDt.NewRow();
                string[] values = newline.Split('\t');
                for (int i = 0; i < values.Length; i++)
                {
                    string valuesStripped = values[i].Trim();
                    if (!string.IsNullOrEmpty(valuesStripped))
                    {
                        dr[i] = valuesStripped.Replace('.',',');
                    }
                }
                AbsoluteDt.Rows.Add(dr);
            }
            file.Close();
           // dgViewDataNumeric.DataSource = dt;


        }


        public void Write2File(string path, int start, int end, bool NormalizeTime, bool MovingAverage, int movingaverageindex)
        {
            if (NormalizeTime)
            {
                ResampleRegion(start, end, MovingAverage, movingaverageindex);
            }


            System.IO.StreamWriter StreamWriter = new System.IO.StreamWriter(path);
            
            //Header (remains the same, except for the amount of rows and the frequency (if normalized))
            StreamWriter.WriteLine("Coordinates");
            StreamWriter.WriteLine("version = 1");

            if (NormalizeTime)
            {
                StreamWriter.WriteLine("nRows = 100");
            }
            else
            {
                StreamWriter.WriteLine("nRows = "+ (end-start).ToString());

            }

            StreamWriter.WriteLine("nColumns = " + _nbCol.ToString()); // 133");
            StreamWriter.WriteLine("inDegrees = yes");
            StreamWriter.WriteLine("");
            StreamWriter.WriteLine("Units are S.I.units(second, meters, Newtons, ...)");
            StreamWriter.WriteLine("Angles are in degrees.");
            StreamWriter.WriteLine("");
            StreamWriter.WriteLine("endheader");
            StreamWriter.WriteLine(parameterNamesLine);

            //Body (is the data itself)

            if (NormalizeTime)
            {
                if (MovingAverage)
                {

                    for (int i = 0; i < 100 - movingaverageindex + 1; i++)   //Less data points here due to the moving average!!! 
                    {
                        StreamWriter.WriteLine(AddResampledRowToStreamwriter(i));
                    }
                }
                else
                {
                    for (int i = 0; i < 100; i++)   //DataLines.Count
                    {
                        StreamWriter.WriteLine(AddResampledRowToStreamwriter(i));
                    }
                }
               
            }
            else
            {
                for (int i = start; i < end; i++)   //DataLines.Count
                {
                    StreamWriter.WriteLine(DataLines[i]);
                }

            }

           

         

            

            StreamWriter.Close();

        }

        private string AddResampledRowToStreamwriter(int i)
        {
            string output = string.Empty;

            //for (int j = 0; j < 100; j++)
            //{
                foreach (float[] array in ResampledListColletion)
                {
                    output += array[i].ToString().Trim().Replace(',','.') + '\t';
                }
            //}
                return output;

        }

        public DataTable currentResampledDt = new DataTable();
        public List<float[]> ResampledListColletion = new List<float[]>();

        public void ResampleRegion(int start, int end, bool MovingAverage, int movingaverageIndex)
        {
            ResampledListColletion.Clear();

            float[] timeArray = new float[100];
            for(int f=0;f<100;f++)
            {
                timeArray[f] = f;
            }


            ResampledListColletion.Add(timeArray);


            for (int y = 1; y < AbsoluteDt.Columns.Count; y++) //
            {

                float[] source = new float[end-start];
                for (int i = start; i < end; i++)
                {
                    source[i-start] = (float)Convert.ToDouble(AbsoluteDt.Rows[i][y]);
                }



                float[] destination = new float[100];

                destination[0] = source[0];
                for (int i = 1; i < source.Length; i++)
                {
                    int j = i * (destination.Length - 1) / (source.Length - 1);
                    destination[j] = source[i];
                    destination[0] = source[0];

                    int jPrevious = 0;
                    for (int k = 1; k < source.Length; k++)
                    {
                        int l = k * (destination.Length - 1) / (source.Length - 1);
                        Interpolate(destination, jPrevious, l, source[k - 1], source[k]);

                        jPrevious = l;
                    }
                }
                if(MovingAverage)
                {
                    ResampledListColletion.Add(Getmovingaverage(movingaverageIndex,destination)); 
                }
                else
                {
                     ResampledListColletion.Add(destination);
                }
           
               
            }
           
        }

        private static float[] Getmovingaverage(int frameSize, float[] data)
        {
            float sum = 0;
            float[] avgPoints = new float[data.Length - frameSize + 1];
            for (int counter = 0; counter <= data.Length - frameSize; counter++)
            {
                int innerLoopCounter = 0;
                int index = counter;
                while (innerLoopCounter < frameSize)
                {
                    sum = sum + data[index];

                    innerLoopCounter += 1;

                    index += 1;

                }

                avgPoints[counter] = sum / frameSize;

                sum = 0;

            }
            return avgPoints;

        }


        private void AddColumnToDatatable(float[] array)
        {

            //// create columns
            //for (int i = 0; i < array.Length; i++)
            //{
            //    currentResampledDt.Columns.Add();
            //}

            currentResampledDt.Columns.Add();

            for (int j = 0; j < array.Length; j++)
            {

                //DataRow _ravi = currentResampledDt.Columns[0]. NewRow();
                //_ravi["Name"] = "ravi";
                //_ravi["Marks"] = "500";
                //dt.Rows.Add(_ravi);


                //// create a DataRow using .NewRow()
                DataRow row = currentResampledDt.NewRow();

                // iterate over all columns to fill the row
                for (int i = 0; i < array.Length; i++)
                {
                    row[i] = array[j];
                }

                // add the current row to the DataTable
                currentResampledDt.Rows.Add(row);
            }


        }

        private static void Interpolate(float[] destination, int destFrom, int destTo, float valueFrom, float valueTo)
        {
            int destLength = destTo - destFrom;
            float valueLength = valueTo - valueFrom;
            for (int i = 0; i <= destLength; i++)
                destination[destFrom + i] = valueFrom + (valueLength * i) / destLength;
        }

        public static MathNet.Numerics.LinearAlgebra.Matrix<double> DenseOfDataTable(DataTable dt)
        {
            double[][] arrayFromDataTable = dt.AsEnumerable().Select(row => Array.ConvertAll(row.ItemArray, item => Convert.ToDouble(item))).ToArray();
            
            return MathNet.Numerics.LinearAlgebra.CreateMatrix.DenseOfRowArrays(arrayFromDataTable);
        }

        private void AddParameter(string paramName)
        {
            _HeaderNames.Add(paramName);
        }

        private void MakeSerie(string dataFileName, string paramName, List<Series> seriesList, DataTable Dt)
        {
            //MathNet.Numerics.LinearAlgebra.Vector<double> T = m.Column(matrixIndex);
            //MathNet.Numerics.LinearAlgebra.Vector<double> time = m.Column(0);
           
            Series serie = new Series(dataFileName + " " + paramName);
           
            for (int i = 0; i < Dt.Rows.Count; i++)
            {
                double yValue = Convert.ToDouble((Dt.Rows[i][paramName]).ToString().Replace(',','.'), CultureInfo.InvariantCulture);
                double xValue = Convert.ToDouble((Dt.Rows[i][0]).ToString().Replace(',', '.'), CultureInfo.InvariantCulture);//The time column
                serie.Points.AddXY(xValue,yValue);
            }
            seriesList.Add(serie);

        }

        private void MakeSerieFromMatrix(string dataFileName, string paramName, int paramIndex, List<Series> seriesList, Matrix<double> Mat)
        {
            //MathNet.Numerics.LinearAlgebra.Vector<double> T = m.Column(matrixIndex);
            //MathNet.Numerics.LinearAlgebra.Vector<double> time = m.Column(0);

            Series serie = new Series(dataFileName + " " + paramName);

            for (int i = 0; i < Mat.RowCount; i++)
            {
                double yValue = Convert.ToDouble(Mat[i,paramIndex].ToString().Replace(',', '.'), CultureInfo.InvariantCulture);
                double xValue = Convert.ToDouble((AbsoluteDt.Rows[i][0]).ToString().Replace(',', '.'), CultureInfo.InvariantCulture);//The time column
                //double xValue = Convert.ToDouble(Mat[i, 0].ToString().Replace(',', '.'), CultureInfo.InvariantCulture); //The time column
                serie.Points.AddXY(xValue, yValue);
            }
            seriesList.Add(serie);

        }
    }
}
