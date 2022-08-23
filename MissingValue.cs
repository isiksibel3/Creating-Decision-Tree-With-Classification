using HelpException;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataMining.Engine.MissingValue
{
    public class MissingValue
    {
        public DataTable FillWithAverage(DataTable copyMainDataTable)
        {
            try
            {
                for (int i = 0; i < copyMainDataTable.Columns.Count; i++)
                {
                    if (copyMainDataTable.Columns[i].DataType == typeof(int))
                    {

                        List<int> notnullIntegerValues = (from notnullRow in copyMainDataTable.AsEnumerable()
                                                          where (!string.IsNullOrEmpty(notnullRow[copyMainDataTable.Columns[i].ColumnName].ToString().Trim()))
                                                          select Convert.ToInt32(notnullRow[copyMainDataTable.Columns[i].ColumnName])).ToList();
                        for (int j = 0; j < copyMainDataTable.Columns[i].Table.Rows.Count; j++)
                        {
                            if (string.IsNullOrEmpty(copyMainDataTable.Rows[j][i].ToString()))
                            {
                                copyMainDataTable.Rows[j][i] = notnullIntegerValues.Average();
                            }
                        }

                    }
                    else
                    {
                        for (int j = 0; j < copyMainDataTable.Columns[i].Table.Rows.Count; j++)
                        {
                            if (copyMainDataTable.Rows[j][i].ToString().Trim() == "?")
                            {
                                copyMainDataTable.Rows[j][i] = "Undefined";
                            }
                        }
                    }
                }
                return copyMainDataTable;
            }
            catch (Exception ex)
            {
                throw HException.Save(ex, "DataMining.Engine.MissingValue.MissingValue", "FillWithAverage", 
                    new Parameter() { Name = "copyMainDataTable", Value = "Type of DataTable" });
            }
        }
    }
}
