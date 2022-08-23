using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DataMining.Engine.MissingValue;
using DataMining.Engine.Reduction;
using DataMining.Engine.Classification;
using DAO;
using DataMining.Engine.Objects;
using DataMining.Engine.ReportCreator;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Threading;

namespace DataMining
{
    public partial class frmMain : Form
    {


        DataTable mainDataTable = new DataTable();

        private Object thisLock = new Object();
        ConnectionDB connectDB;

        private bool first = true;

        public List<string> databses = new List<string>();
        public List<string> tables = new List<string>();
        public List<string> columns = new List<string>();
        Process process = null;
        DataTable resultTable;
        public frmMain()
        {
            InitializeComponent();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            try
            {
                #region[Kullanılmayan Alan]
                //mainDataTable = fillMainDataTable();
                //mainDataTable = AdultDB.GetAllDatasForAdult();
                //MissingValue missingValue = new MissingValue();
                //Reduction reduction = new Reduction();
                //Classification classification = new Classification();

                //DataTable copyMainDataTable = mainDataTable.Clone();
                //foreach (DataRow row in mainDataTable.Rows)
                //{
                //    copyMainDataTable.Rows.Add(row.ItemArray);
                //}
                //DataTable filledMissingValuesCopyMainDataTable = missingValue.FillWithAverage(copyMainDataTable);
                //DataTable reducedDataTable = reduction.Reduce(filledMissingValuesCopyMainDataTable);

                //StringBuilder sb = new StringBuilder();

                //IEnumerable<string> columnNames = reducedDataTable.Columns.Cast<DataColumn>().
                //                                  Select(column => column.ColumnName);
                //sb.AppendLine(string.Join(",", columnNames));

                //foreach (DataRow row in reducedDataTable.Rows)
                //{
                //    IEnumerable<string> fields = row.ItemArray.Select(field => field.ToString());
                //    sb.AppendLine(string.Join(",", fields));
                //}

                //File.WriteAllText(@"C:\Users\Sibel\Desktop\Dataset For Knime\ReductionData.csv", sb.ToString());
                //DataSet ds = new DataSet();
                //ds.Tables.Add(reducedDataTable);
                //ds.WriteXml(@"C:\Users\Sibel\Desktop\Dataset For Knime\ReductionData.xml");

                //classification.CalculateEntropy(reducedDataTable, "Gender");

                //List<string> testClassValues = new List<string>() { "Race" };
                //classification.GetWinner(reducedDataTable, testClassValues);

                //Branch test = classification.CreateDecisionTree(reducedDataTable, testClassValues, 0, null, true, 20);


                //ClassificationReport cr = new ClassificationReport();
                //string reportTest = cr.CreateDecisionTreeHtml(test, "Race", true, "");
                //reportTest += "}";
                #endregion 

            }
            catch (Exception ex)
            {

                throw ex;
            }

        }
    
        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void btnRunClassification_Click(object sender, EventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(cmbDatabases.Text) && !string.IsNullOrEmpty(cmbColumns.Text) && !string.IsNullOrEmpty(cmbTables.Text))
                {
                    SqlCommand cmd = new SqlCommand("select TOP 1000 * from " + cmbTables.Text, connectDB.GetSqlConnection());

                    resultTable = new DataTable();
                    SqlDataAdapter adp = new SqlDataAdapter(cmd);
                    adp.Fill(resultTable);
                    List<string> classColumns = new List<string>(){cmbColumns.Text};
                 
                    
                    MissingValue missingValue = new MissingValue();
                    Reduction reduction = new Reduction();
                    Classification classification = new Classification();

                    DataTable copyMainDataTable = resultTable.Clone();
                    foreach (DataRow row in resultTable.Rows)
                    {
                        copyMainDataTable.Rows.Add(row.ItemArray);
                    }
                    DataTable filledMissingValuesCopyMainDataTable = missingValue.FillWithAverage(copyMainDataTable);
                    DataTable reducedDataTable = reduction.Reduce(filledMissingValuesCopyMainDataTable);

                    Branch brn=  classification.CreateDecisionTree(reducedDataTable, classColumns, 0, null, true, 100);

                    ClassificationReport cr = new ClassificationReport();
                    string rsultHtml = cr.CreteDecisionTreeToHtml(brn, classColumns.FirstOrDefault(), true, "");

                    OpenFileDialog file = new OpenFileDialog();
                    //file.Filter = "Excel Dosyası |*.xlsx| Excel Dosyası|*.xls";  
                    file.FilterIndex = 2;
                    file.RestoreDirectory = true;
                    file.CheckFileExists = false;
                    file.Title = "Select excel file.";
                    file.Multiselect = true;

                    string savedFile=string.Empty;
                    if (file.ShowDialog() == DialogResult.OK)
                    {
                        string DosyaYolu = file.FileName;
                        string DosyaAdi = file.SafeFileName;
                        savedFile =WriteDecetionTreeToFile(rsultHtml, DosyaYolu);
                    }  

                    if(!string.IsNullOrEmpty(savedFile))
                    {

                        Process.Start(savedFile);
                        
                    }
                    
                   
                }
            }
            catch(HelpException.HException exp)
            {
                MessageBox.Show(exp.Message);
            }
            catch (Exception ex)
            {
                HelpException.HException exp = HelpException.HException.Save(ex, "DataMining.frmMain", "btnRunClassification_Click");
                MessageBox.Show(exp.Message);
            }
        }


        private string WriteDecetionTreeToFile(string webDocument,string saveFileFolder)
        {
            try
            {
                using (FileStream fs = File.Create(saveFileFolder + ".html"))
                {
                    byte[] byteData = null;
                    byteData = Encoding.ASCII.GetBytes(webDocument);
                    fs.Write(byteData, 0, byteData.Length);
                    fs.Close();
                }
                return saveFileFolder + ".html";
            }
            catch (Exception ex)
            {
                throw HelpException.HException.Save(ex, "DataMining.frmMain", "btnRunClassification_Click", 
                    new HelpException.Parameter() { Name = "saveFileFolder", Value = saveFileFolder.ToString()},
                    new HelpException.Parameter() { Name = "webDocument", Value = webDocument.ToString() });

            }
        }
        private void chkLocal_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                txtDatabaseIP.Enabled = !chkLocal.Checked;
            }
            catch(HelpException.HException exp)
            {
                MessageBox.Show(exp.Message);
            }
            catch (Exception ex)
            {
                HelpException.HException exp = HelpException.HException.Save(ex, "DataMining.frmMain", "chkLocal_CheckedChanged");
                MessageBox.Show(exp.Message);
            }
        }

        private void chkWindowsAutantication_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                txtPassword.Enabled = !chkWindowsAutantication.Checked;
                txtUserName.Enabled = !chkWindowsAutantication.Checked;
            }
            catch (HelpException.HException exp)
            {
                MessageBox.Show(exp.Message);
            }
            catch (Exception ex)
            {
                HelpException.HException exp = HelpException.HException.Save(ex, "DataMining.frmMain", "chkWindowsAutantication_CheckedChanged");
                MessageBox.Show(exp.Message);
            }
        }

        private List<string> GetDatabases(SqlConnection con)
        {
            try
            {
                databses.Clear();
                con.Open();
                DataTable databases = con.GetSchema("Databases");
                foreach (DataRow database in databases.Rows)
                {
                    String databaseName = database.Field<String>("database_name");

                    databses.Add(databaseName);
                }
                return databses;
            }           
            catch (Exception ex)
            {
                throw HelpException.HException.Save(ex, "DataMining.frmMain", "GetDatabases",
                                    new HelpException.Parameter() { Name = "con", Value = "Type of SqlConnection" });
            }
            finally
            {
                con.Close();
            }
        }
        private List<string> GetTables(SqlConnection con)
        {
            try
            {
                tables.Clear();
                con.Open();
                DataTable tablesDataTable = con.GetSchema("Tables");
                foreach (DataRow database in tablesDataTable.Rows)
                {
                    String databaseName = database.Field<String>("TABLE_NAME");

                    tables.Add(databaseName);
                }
                return tables;
            }
            catch (Exception ex)
            {
                throw HelpException.HException.Save(ex, "DataMining.frmMain", "GetTables",
                                    new HelpException.Parameter() { Name = "con", Value = "Type of SqlConnection" });
            }
            finally
            {
                con.Close();
            }
        }
        private List<string> GetColumns(SqlConnection con)
        {
            try
            {
                columns.Clear();
                con.Open();
                DataTable tablesDataTable = con.GetSchema("Columns");
                foreach (DataRow database in tablesDataTable.Rows)
                {
                    String databaseName = database.Field<String>("COLUMN_NAME");

                    columns.Add(databaseName);
                }
                return tables;
            }
            catch (Exception ex)
            {
                throw HelpException.HException.Save(ex, "DataMining.frmMain", "GetColumns",
                                    new HelpException.Parameter() { Name = "con", Value = "Type of SqlConnection" });
            }
            finally
            {
                con.Close();
            }
        }
        private void cmbDatabases_Click(object sender, EventArgs e)
        {
            try
            {

                lock (thisLock)
                {

                    cmbDatabases.Items.Clear();
                    if (chkLocal.Checked && chkWindowsAutantication.Checked)
                    {
                        connectDB = new ConnectionDB();

                    }
                    else if (chkLocal.Checked)
                    {
                        if (!string.IsNullOrEmpty(txtUserName.Text) && !string.IsNullOrEmpty(txtPassword.Text))
                        {
                            connectDB = new ConnectionDB(txtUserName.Text, txtPassword.Text);
                        }
                        else
                        {
                            MessageBox.Show("Please enter the user name and the surname."); //düzelt
                        }
                    }
                    else if (chkWindowsAutantication.Checked)
                    {
                        if (!string.IsNullOrEmpty(txtDatabaseIP.Text))
                        {
                            connectDB = new ConnectionDB(txtDatabaseIP.Text);
                        }
                        else
                        {
                            MessageBox.Show("Please enter the database IP."); 
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(txtUserName.Text) && !string.IsNullOrEmpty(txtPassword.Text) && !string.IsNullOrEmpty(txtDatabaseIP.Text))
                        {
                            connectDB = new ConnectionDB(txtDatabaseIP.Text, txtUserName.Text, txtPassword.Text);
                        }
                        else
                        {
                            MessageBox.Show("Required fields must be filled."); 
                        }
                    }

                    if (connectDB != null)
                    {
                        List<string> dbList = GetDatabases(connectDB.GetSqlConnection());
                        if (dbList != null && dbList.Count > 0)
                        {
                            foreach (string name in dbList)
                            {
                                cmbDatabases.Items.Add(name);
                            }
                        }
                    }


                }


            }
            catch (HelpException.HException exp)
            {
                MessageBox.Show(exp.Message);
            }
            catch (Exception ex)
            {
                HelpException.HException exp = HelpException.HException.Save(ex, "DataMining.frmMain", "cmbDatabases_Click");
                MessageBox.Show(exp.Message);
            }
        }

        private void cmbDatabases_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                ComboBox cmb = sender as ComboBox;
                if (cmb != null)
                {
                    if (cmb.SelectedItem != null && !string.IsNullOrEmpty(cmb.SelectedItem.ToString()))
                    {
                        connectDB.DatabaseName = cmb.SelectedItem.ToString();
                    }
                }

            }
            catch (HelpException.HException exp)
            {
                MessageBox.Show(exp.Message);
            }
            catch (Exception ex)
            {
                HelpException.HException exp = HelpException.HException.Save(ex, "DataMining.frmMain", "cmbDatabases_SelectedIndexChanged");
                MessageBox.Show(exp.Message);
            }
        }

        private List<string> GetColumnNames(SqlConnection con)
        {
            try
            {
                columns.Clear();
                if (cmbTables.SelectedItem != null && !string.IsNullOrEmpty(cmbTables.SelectedItem.ToString()))
                {

                    using (SqlCommand command = con.CreateCommand())
                    {
                        command.CommandText = "select top 100 * from " + cmbTables.SelectedItem.ToString();
                        con.Open();
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                columns.Add(reader.GetString(0));
                            }
                        }
                    }

                }
                return columns;
            }
            catch (Exception ex)
            {
                throw HelpException.HException.Save(ex, "DataMining.frmMain", "GetColumnNames",
                                    new HelpException.Parameter() { Name = "con", Value = "Type of SqlConnection" });
            }
        }

        private void cmbTables_Click(object sender, EventArgs e)
        {
            try
            {
                lock (thisLock)
                {
                    if (connectDB.DatabaseName != null)
                    {
                        cmbTables.Items.Clear();
                        List<string> tableList = GetTables(connectDB.GetSqlConnection());
                        if (tableList != null && tableList.Count > 0)
                        {
                            foreach (string name in tableList)
                            {
                                cmbTables.Items.Add(name);
                            }
                        }
                    }
                }
            }
            catch (HelpException.HException exp)
            {
                MessageBox.Show(exp.Message);
            }
            catch (Exception ex)
            {
                HelpException.HException exp = HelpException.HException.Save(ex, "DataMining.frmMain", "cmbTables_Click");
                MessageBox.Show(exp.Message);
            }
        }

        private void cmbColumns_Click(object sender, EventArgs e)
        {
            try
            {
                lock (thisLock)
                {
                    GetColumns(connectDB.GetSqlConnection());
                    cmbColumns.Items.Clear();
                    foreach (string columnItem in columns)
                    {
                        cmbColumns.Items.Add(columnItem);
                    }
                }
            }
            catch (HelpException.HException exp)
            {
                MessageBox.Show(exp.Message);
            }
            catch (Exception ex)
            {
                HelpException.HException exp = HelpException.HException.Save(ex, "DataMining.frmMain", "cmbColumns_Click");
                MessageBox.Show(exp.Message);
            }
        }

        #region Kullanılmayan
        //private DataTable fillMainDataTable(string filePath = @"C:\Adult\AdultDataSet.csv", char separator = ',')
        //{
        //    try
        //    {
        //        DataTable dtTable = new DataTable();

        //        Dictionary<int, Type> columnTypes = new Dictionary<int, Type>();

        //        List<string[]> rows = File.ReadAllLines(filePath).Select(a => a.Split(separator)).ToList();

        //        //string[] columnNames = rows.FirstOrDefault().FirstOrDefault().ToString().Split(',');
        //        for (int i = 0; i < rows.FirstOrDefault().Length; i++)
        //        {
        //            List<object> isNotNullValues;
        //            List<float> floatValues = new List<float>();
        //            isNotNullValues = new List<object>();
        //            for (int j = 1; j < rows.Count; j++)
        //            {
        //                if(!string.IsNullOrEmpty(rows[j][i]))
        //                {
        //                    float val = 0;
        //                    isNotNullValues.Add(rows[j][i]);
        //                    if (float.TryParse((rows[j][i].ToString().Replace('.', ',')), out val))
        //                    {
        //                        floatValues.Add(val);
        //                    }
        //                }
        //            }
        //            if (floatValues.Count == isNotNullValues.Count)
        //            {
        //                columnTypes.Add(i, typeof(float));
        //            }
        //            else
        //            {
        //                columnTypes.Add(i, typeof(string));
        //            }
        //        }
        //        for (int i = 0; i < rows.FirstOrDefault().Length; i++)
        //        {
        //            dtTable.Columns.Add(rows.FirstOrDefault()[i],columnTypes[i]);
        //        }
        //        for (int i = 1; i < rows.Count; i++)
        //        {
        //            DataRow rw = dtTable.NewRow();
        //            for (int j = 0; j < rows[i].Length - 1; j++)
        //            {
        //                rw[j] = rows[i][j];
        //            }
        //            dtTable.Rows.Add(rw);
        //        }

        //        return dtTable;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}

        //private Type GetDataType(List<string[]> rows, int columnIndex)
        //{
        //    try
        //    {
        //        for (int i = 1; i < rows[columnIndex].Length; i++)
        //        {
        //            if (!string.IsNullOrEmpty(rows[columnIndex][0]))
        //            {
        //                float val;
        //                if (float.TryParse((rows[i][0].ToString().Replace('.', ',')), out val))
        //                {
        //                    return typeof(float);
        //                }
        //            }

        //        }
        //        return null;
        //    }
        //    catch (Exception ex)
        //    {

        //        throw ex;
        //    }
        //}
        #endregion
    }
}
