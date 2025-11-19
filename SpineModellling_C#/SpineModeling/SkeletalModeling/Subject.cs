using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using SpineAnalyzer.Acquisitions;

namespace SpineAnalyzer
{
    public class Subject
    {

        private TOD.PWEncryption PWEncryption = new TOD.PWEncryption();
        #region Local declarations
        //Database object
        public DataBase SQLDB;
        public AppData appData;
        private Acquisitions.ListOptions OptionClass = new Acquisitions.ListOptions();

        public SubSpecimen currentSubSpecimen;

        //Patient Properties
        public int SWS_volgnummer = 0;
        private string _ID = string.Empty;
        private string _FirstName = string.Empty;
        private string _LastName = string.Empty;
        private string _Comments = string.Empty;
        private string _SubjectCode = string.Empty;
        private string _SubjectGroup = string.Empty;
        private string _ExternalID = string.Empty;
        private string _SecondID = string.Empty;
        private string _StudyGroup = string.Empty;
        private DateTime _BirthDate = DateTime.Now;
        private DateTime _DateAdded = DateTime.Now;
        private string _Abbreviation = string.Empty;
        private string _RijksregisterNummer = string.Empty;
        private string _Address = string.Empty;
        private string _Sex = string.Empty;
        private string _Phone = string.Empty;
        private string _StudyStatus = string.Empty;
        private string _ClinicalTrajectory = string.Empty;
        public int _hasBirthdate = 0;
        private int _IsInWait = 0;


        private string _MiddleName = string.Empty;
        private DateTime _DateOfDeath = DateTime.MinValue;
        public int _IsDead = 0;
        private string _PlaceOfBirth = string.Empty;
        private string _PlaceOfDeath = string.Empty;
        private string _Language = "Nederlands";

        private string _UserName = string.Empty;
        private string _Address_Street = string.Empty;
        private string _Address_Number = string.Empty;
        private string _Address_Bus = string.Empty;
        private string _Address_City = string.Empty;
        private string _Address_PostalCode = string.Empty;
        private string _Address_Country = string.Empty;
        private string _Address_Province = string.Empty;


        //These need to be removed in time
        private string _FovCT = string.Empty;
        private string _MRIProtocol = string.Empty;
        private string _BendedEOS = string.Empty;
        public double _AgeAtDeath = 0;






        public List<string> ClinicalTrajectoryOptions = new List<string>(new string[] { " ", "Pre/post", "Pre", "Control" });
        public List<string> FovCTOptions = new List<string>(new string[] { "  ", "1. T1", "2. T2", "3. T3", "4. T4", "5. T5", "6. T6", "7. T7", "8. T8", "9. T9", "10. T10", "11. T11", "12. T12", "13. L1", "14. L2", "15. L3", "16. L4", "17. L5", "NA" });
        public List<string> MRIProtcolOptions = new List<string>(new string[] { " ", "Old", "New", "Other", "NA" });
        public List<string> StudyStatusOptions = new List<string>(new string[] { "Unknown", "To be included", "In Progress", "Completed", "On Hold", "Stopped: Excluded", "Stopped: Cancelled", "Stopped: Other" });
        public List<string> BendedEOSOptions = new List<string>(new string[] { " ", "NA", "Flexion", "Extension", "Lateral Bending", "Axial Rotation", "Multiple", "Other" });
        public List<string> SexOptions = new List<string>(new string[] { " ", "M", "F", "NA" });




        //Indicator if patient exists already in the database
        private bool _NewPatient;

        #endregion

        #region "Properties"
        public string ID
        {
            get { return _ID; }
            set { _ID = value; }
        }

        public List<string> StudyGroupOptions
        {
            get { return OptionClass.Read(22, SQLDB); }
        }



        public string FirstName
        {
            get {if (appData.localStudyUser._CanSeePatients)
                {
                    return PWEncryption.TripleDESDecode(_FirstName);
                }
            else
                {
                    return _FirstName; //"Undisclosed"
                }
            }
            set {
                if (appData.localStudyUser._CanSeePatients)
                {
                    _FirstName = PWEncryption.TripleDESEncode(value);
                }
                else
                {


                }
            }
        }

        public string MiddleName
        {
            get {
                if (appData.localStudyUser._CanSeePatients)
                {
                    return PWEncryption.TripleDESDecode(_MiddleName);
                }
                else
                {
                    return _MiddleName;
                }
            }
            set
            {
                if (appData.localStudyUser._CanSeePatients)
                { _MiddleName = PWEncryption.TripleDESEncode(value); }
            }
        }

        public string UserName
        {
            get { return _UserName; }
            set { _UserName = value; }
        }

        public string LastName
        {
            get
            {
                if (appData.localStudyUser._CanSeePatients)
                {
                    return PWEncryption.TripleDESDecode(_LastName);
                }
                else
                {
                    return _LastName;
                }
            }
            set
            {
                if (appData.localStudyUser._CanSeePatients)
                {
                    _LastName = PWEncryption.TripleDESEncode(value);
                }
            }
        }

        public string Comments
        {
            get
            {
                if (appData.localStudyUser._CanSeePatients)
                {
                    return PWEncryption.TripleDESDecode(_Comments);
                }
                else
                {
                    return _Comments;
                }

            }
            set {
                if (appData.localStudyUser._CanSeePatients)
                {
                    _Comments = PWEncryption.TripleDESEncode(value);
                }
            }
        }

        public string Address
        {
            get { return _Address; }
            set { _Address = value; }
        }

        public string Phone
        {
            get { return _Phone; }
            set { _Phone = value; }
        }

        public string ExternalID
        {
            get { return _ExternalID; }
            set { _ExternalID = value; }
        }

        public string SecondID
        {
            get { return _SecondID; }
            set { _SecondID = value; }
        }

        public string ClinicalTrajectory
        {
            get { return _ClinicalTrajectory; }
            set { _ClinicalTrajectory = value; }
        }

        public string SubjectCode
        {
            get { return _SubjectCode; }
            set { _SubjectCode = value; }
        }

        public string Abbreviation
        {
            get { return PWEncryption.TripleDESDecode(_Abbreviation); }
            set { _Abbreviation = PWEncryption.TripleDESEncode(value); }
        }

        public string RijksregisterNummer
        {
            get { return PWEncryption.TripleDESDecode(_RijksregisterNummer); }
            set { _RijksregisterNummer = PWEncryption.TripleDESEncode(value); }
        }
        

        public string StudyGroup
        {
            get { return _StudyGroup; }
            set { _StudyGroup = value; }
        }

        public string FovCT
        {
            get { return _FovCT; }
            set { _FovCT = value; }
        }

        public string BendedEOS
        {
            get { return _BendedEOS; }
            set { _BendedEOS = value; }
        }

        public string MRIProtocol
        {
            get { return _MRIProtocol; }
            set { _MRIProtocol = value; }
        }

        public string StudyStatus
        {
            get { return _StudyStatus; }
            set { _StudyStatus = value; }
        }

        public string Sex
        {
            get { return _Sex; }
            set { _Sex = value; }
        }

        public string SubjectGroup
        {
            get { return _SubjectGroup; }
            set { _SubjectGroup = value; }
        }
        public DateTime BirthDate
        {
            get { return _BirthDate; }
            set { _BirthDate = value; }
        }

        public DateTime DateAdded
        {
            get { return _DateAdded; }
        }

        public bool IsNewPatient
        {
            get { return _NewPatient; }
        }

        public DateTime DateOfDeath
        {
            get { return _DateOfDeath; }
            set { _DateOfDeath = value; }
        }

        public bool IsDead
        {
            get { return (_IsDead == 1); }
            set
            {
                if (value)
                {
                    _IsDead = 1;
                }
                else
                {
                    _IsDead = 0;
                }
            }
        }

        public string PlaceOfBirth
        {
            get
            {
                if (appData.localStudyUser._CanSeePatients)
                { return PWEncryption.TripleDESDecode(_PlaceOfBirth); }
                else
                {
                    return _PlaceOfBirth;
                }
            }
            set
            {
                if (appData.localStudyUser._CanSeePatients)
                { _PlaceOfBirth = PWEncryption.TripleDESEncode(value); }
            }
        }

        public string PlaceOfDeath
        {
            get
            {
                if (appData.localStudyUser._CanSeePatients)
                {
                    return PWEncryption.TripleDESDecode(_PlaceOfDeath);
                }
                else
                {
                    return _PlaceOfDeath;
                }
            }
            set
            {
                if (appData.localStudyUser._CanSeePatients)
                {
                    _PlaceOfDeath = PWEncryption.TripleDESEncode(value);
                }
            }
        }

        public string Language
        {
            get { return _Language; }
            set { _Language = value; }
        }

        public string Address_Street
        {
            get
            {
                if (appData.localStudyUser._CanSeePatients)
                { return PWEncryption.TripleDESDecode(_Address_Street); }
                else
                {
                    return _Address_Street;
                }
            }
            set
            {
                if (appData.localStudyUser._CanSeePatients)
                { _Address_Street = PWEncryption.TripleDESEncode(value); }
            }
        }
        public string Address_Number
        {
            get
            {
                if (appData.localStudyUser._CanSeePatients)
                { return PWEncryption.TripleDESDecode(_Address_Number); }
                else
                {
                    return _Address_Number;
                }
            }
            set
            {
                if (appData.localStudyUser._CanSeePatients)
                { _Address_Number = PWEncryption.TripleDESEncode(value); }
            }
        }
        public string Address_Bus
        {
            get
            {
                if (appData.localStudyUser._CanSeePatients)
                { return PWEncryption.TripleDESDecode(_Address_Bus); }
                else
                {
                    return _Address_Bus;
                }
            }
            set {
                    if (appData.localStudyUser._CanSeePatients)
                    { _Address_Bus = PWEncryption.TripleDESEncode(value); }
                }
        }
        public string Address_City
        {
            get
            {
                if (appData.localStudyUser._CanSeePatients)
                { return PWEncryption.TripleDESDecode(_Address_City); }
                else
                {
                    return _Address_City;
                }
            }
            set
            {
                if (appData.localStudyUser._CanSeePatients)
                { _Address_City = PWEncryption.TripleDESEncode(value); }
            }
        }

        public string Address_PostalCode
        {
            get
            {
                if (appData.localStudyUser._CanSeePatients)
                { return PWEncryption.TripleDESDecode(_Address_PostalCode); }
                else
                {
                    return _Address_PostalCode;
                }
            }
            set {
                    if (appData.localStudyUser._CanSeePatients)
                    { _Address_PostalCode = PWEncryption.TripleDESEncode(value); }
                }
        }

        public string Address_Country
        {
            get
            {
                if (appData.localStudyUser._CanSeePatients)
                { return PWEncryption.TripleDESDecode(_Address_Country); }
                else
                {
                    return _Address_Country;
                }
            }
            set
            {
                if (appData.localStudyUser._CanSeePatients)
                { _Address_Country = PWEncryption.TripleDESEncode(value); }
            }
        }

        public string Address_Province
        {
            get
            {
                if (appData.localStudyUser._CanSeePatients)
                { return PWEncryption.TripleDESDecode(_Address_Province); }
                else
                {
                    return _Address_Province;
                }
            }
            set
            {
                if (appData.localStudyUser._CanSeePatients)
                { _Address_Province = PWEncryption.TripleDESEncode(value); }
            }
        }


        public bool IsInWait
        {
            get { return (_IsInWait == 1); }
            set
            {
                if (value)
                {
                    _IsInWait = 1;
                }
                else
                {
                    _IsInWait = 0;
                }
            }
        }


        #endregion

        #region Methods

        //Initializing Constructor for database settings
        public Subject(DataBase Database, int ID, AppData AppData)
        {
            this.appData = AppData;
            //Only if ID is filled
            //if (string.IsNullOrWhiteSpace(ID) || string.IsNullOrEmpty(ID))
            //{
            //    return;
            //    throw new PatientException("Patient ID is empty.");
            //}

            //Set database
            SQLDB = Database;

            //Set ID
            SWS_volgnummer = ID;

            //Check if Patient exists in database  (not needed is handled by Read!!)
            //_NewPatient = (!this.Exists());

            //If Patient does not exist then Initialize else Read data
            try
            {
                //this.Read();
            }
            catch (System.Exception exception)
            {
                throw new PatientException("Error Reading Patient with SWS_volgnummer= " + SWS_volgnummer);
            }
        }

     

        public void Save()
        {
           //DOPLr handler.
           
         
            _NewPatient = false;
        }

       


        public void Delete()
        {
           
        }

        public bool Subject_IDalreadytaken()
        {
           
        }

        //Check if Subject already exists in database
        public bool Exists()
        {
           
        }
        #endregion

        #region Database Methods

        public DataSet ReadSubjectMocap()
        {

            //Set Database
            DataBase SQLDB = new DataBase(appData.SQLServer, appData.SQLDatabase, appData.SQLAuthSQL, appData.SQLUser, appData.SQLPassword);

            //Find all Mocap Objects of this subject. 
            string strSQLRead = "SELECT AcquisitionNumber FROM MOCAP where SubjectID = @SubjectID";

            SqlCommand SQLCommand = new SqlCommand(strSQLRead, SQLDB.Connection);
            SQLCommand.Parameters.AddWithValue("@SubjectID", SWS_volgnummer);





            //Fill a dataset with selected records
            DataSet ds = new DataSet();
            SQLDB.ReadDataSet(SQLCommand, ref ds);

            return ds;
        }

        public DataSet ReadSubjectGeom()
        {

            //Set Database
            DataBase SQLDB = new DataBase(appData.SQLServer, appData.SQLDatabase, appData.SQLAuthSQL, appData.SQLUser, appData.SQLPassword);

            //Find all Mocap Objects of this subject. 
            string strSQLRead = "SELECT GeometryNumber FROM GeometryFiles where SubjectID = @SubjectID";

            SqlCommand SQLCommand = new SqlCommand(strSQLRead, SQLDB.Connection);
            SQLCommand.Parameters.AddWithValue("@SubjectID", SWS_volgnummer);





            //Fill a dataset with selected records
            DataSet ds = new DataSet();
            SQLDB.ReadDataSet(SQLCommand, ref ds);

            return ds;
        }

        public List<Junction_SubStudyX> GetJunctionsSubstudies()
        {
            List<Junction_SubStudyX> junction_SubStudyXes = new List<Junction_SubStudyX>();

            string X_table = "SubjectPers";
            string JunctionTable = "Junction_SubStudySubjectPers";


            SqlCommand SQLcmd = new SqlCommand();

            string SQLselect = "SELECT SubStudy_ID, Owner, " + X_table + "_ID FROM " + JunctionTable + " where " + X_table + "_ID =@" + X_table + "_ID";
            SQLcmd.Parameters.AddWithValue("@" + X_table + "_ID", this.SWS_volgnummer);


            SQLcmd.CommandText = SQLselect;

            //Set Database
            DataBase SQLDB = new DataBase(appData.SQLServer, appData.SQLDatabase, appData.SQLAuthSQL, appData.SQLUser, appData.SQLPassword);

            //Fill a dataset with selected records
            DataSet ds = new DataSet();
            SQLDB.ReadDataSet(SQLcmd, ref ds);

            foreach (DataRow row in ds.Tables[0].Rows)
            {
                Junction_SubStudyX junction_SubStudyX = new Junction_SubStudyX(SQLDB, Convert.ToInt32(row[0]), this.SWS_volgnummer, this.ID, "SubjectPers", 0, appData, true);
                junction_SubStudyXes.Add(junction_SubStudyX);
            }



            return junction_SubStudyXes;

        }

        public List<SubStudy> GetJunctionsSubstudies2()
        {
            List<SubStudy> listsubst = new List<SubStudy>();

            foreach (Junction_SubStudyX junct in GetJunctionsSubstudies())
            {
                SubStudy subStudy = new SubStudy(SQLDB, junct.SubStudy_ID, appData);
                listsubst.Add(subStudy);
            }

            return listsubst;

        }

        public List<ProtocolDeviation> GetProtocolDeviationList()
        {


            List<ProtocolDeviation> ProtDevList = new List<ProtocolDeviation>();
            //Build SQL Command (=SQL select statement + Parameters)
            SqlCommand SQLcmd = new SqlCommand();
            string SQLwhere = string.Empty;

            string SQLselect;
            SQLselect = "SELECT Type, ProtocolDevID, Location, AcquisitionDate, EndDate, Description, DateAdded, UserName FROM ProtocolDeviation";


            SQLselect += " WHERE SubjectID=@SubjectID";
            SQLcmd.Parameters.AddWithValue("@SubjectID", this.SWS_volgnummer);


            SQLcmd.CommandText = SQLselect;


            DataSet ds = new DataSet();
            SQLDB.ReadDataSet(SQLcmd, ref ds);


            // Resize the DataGridView columns to fit the newly loaded content.
            //dgView.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCellsExceptHeader);
            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
            {
                ProtocolDeviation protocolDeviation = new ProtocolDeviation(SQLDB, Convert.ToInt32(ds.Tables[0].Rows[i]["ProtocolDevID"]), appData);
                ProtDevList.Add(protocolDeviation);
            }

            return ProtDevList;
        }

        public List<EOS> GetEOSList()
        {

            List<EOS> ReturnList = new List<EOS>();
            //Build SQL Command (=SQL select statement + Parameters)
            SqlCommand SQLcmd = new SqlCommand();
            string SQLwhere = string.Empty;

            string SQLselect;
            SQLselect = "SELECT AcquisitionNumber FROM EOS";


            SQLselect += " WHERE SubjectID=@SubjectID";
            SQLcmd.Parameters.AddWithValue("@SubjectID", this.SWS_volgnummer);


            SQLcmd.CommandText = SQLselect;


            DataSet ds = new DataSet();
            SQLDB.ReadDataSet(SQLcmd, ref ds);


            // Resize the DataGridView columns to fit the newly loaded content.
            //dgView.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCellsExceptHeader);
            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
            {
                EOS EOS = new EOS(SQLDB, this, Convert.ToInt32(ds.Tables[0].Rows[i]["AcquisitionNumber"]));
                ReturnList.Add(EOS);
            }

            return ReturnList;
        }

        public SubSpecimen ReturnPrimairySpecimen()
        {
            //Build SQL Command (=SQL select statement + Parameters)
            SqlCommand SQLcmd = new SqlCommand();


            string SQLselect = "SELECT ID, WorkCode, SubSystemID, DateAdded, DateUpdated, UserName FROM SubSpecimen where SubjectID = @SubjectID and SubSystemID = @SubSystemID";
            SQLcmd.Parameters.AddWithValue("@SubjectID", this.SWS_volgnummer);
            SQLcmd.Parameters.AddWithValue("@SubSystemID", 0); //Zero indicates the primairy subspecimen.
            SQLcmd.CommandText = SQLselect;



            //Set Database
            DataBase SQLDB = new DataBase(appData.SQLServer, appData.SQLDatabase, appData.SQLAuthSQL, appData.SQLUser, appData.SQLPassword);

            //Fill a dataset with selected records
            DataSet ds = new DataSet();
            SQLDB.ReadDataSet(SQLcmd, ref ds);

            SubSpecimen MAINsubSpecimen = new SubSpecimen(SQLDB, 0, appData);

            if (ds.Tables[0].Rows.Count == 0)
            {
                //Create it:
                MAINsubSpecimen = new SubSpecimen(SQLDB, this, 0, appData);
                MAINsubSpecimen._WorkCode = "Main";
                MAINsubSpecimen._SubSystemID = 0;
                MAINsubSpecimen.Save();

            }
            else
            {
                if (ds.Tables[0].Rows.Count == 1)
                {
                    MAINsubSpecimen = new SubSpecimen(SQLDB, Convert.ToInt32(ds.Tables[0].Rows[0]["ID"].ToString()), appData);
                }
                else
                {// error handling in case there are more than one Main subspecimen for this subject.
                    MAINsubSpecimen = null;
                }
            }

            return MAINsubSpecimen;

        }

        public bool MyStudyIsOwnerOfPatient()
        {
            if (appData.SWS_Study.IsInTopLevelStudy)
            {
                return true;
            }
            else
            {
                if (GetJunctionsSubstudies2()[0]._SubStudyIndex == appData.SWS_Study.selectedSubStudies[0]._SubStudyIndex)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

        }

        public List<Patient_Subject.Relative> getRelatives()
        {
            List<Patient_Subject.Relative> RelativesList = new List<Patient_Subject.Relative>();





            //Find all Mocap Objects of this subject. 
            string strSQLRead = "SELECT Relatives_ID FROM db_owner.Relatives where Subject_ID = @Subject_ID";

            SqlCommand SQLCommand = new SqlCommand(strSQLRead, SQLDB.Connection);
            SQLCommand.Parameters.AddWithValue("@Subject_ID", this.SWS_volgnummer);
          




            //Fill a dataset with selected records
            DataSet ds = new DataSet();
            SQLDB.ReadDataSet(SQLCommand, ref ds);

            foreach (DataRow row in ds.Tables[0].Rows)
            {

                Patient_Subject.Relative relative = new Patient_Subject.Relative(SQLDB, Convert.ToInt32(row[0]), appData);
                RelativesList.Add(relative);
            }


            return RelativesList;

        }


        public SubSpecimen GetEquivalentSubSpecimen()
        {



            //Build SQL Command (=SQL select statement + Parameters)
            SqlCommand SQLcmd = new SqlCommand();


            string SQLselect = "SELECT ID, WorkCode, SubSystemID, DateAdded, DateUpdated, UserName FROM SubSpecimen where SubjectID = @SubjectID and SubSystemID = @SubSystemID";
            SQLcmd.Parameters.AddWithValue("@SubjectID", this.SWS_volgnummer);
            SQLcmd.Parameters.AddWithValue("@SubSystemID", 0);
            SQLcmd.CommandText = SQLselect;

            //Set Database
            DataBase SQLDB = new DataBase(appData.SQLServer, appData.SQLDatabase, appData.SQLAuthSQL, appData.SQLUser, appData.SQLPassword);

            //Fill a dataset with selected records
            DataSet ds = new DataSet();
            SQLDB.ReadDataSet(SQLcmd, ref ds);
            if(ds.Tables[0].Rows.Count==0)
            {
                return null;
            }
            else
            {
            return new SubSpecimen(SQLDB, Convert.ToInt32(ds.Tables[0].Rows[0]["ID"].ToString()), appData);
            }

        }


        #endregion



    }
}
