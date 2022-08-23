using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataMining.Helper;
using DataMining.Engine.Objects;
using HelpException;

namespace DataMining.Engine.Classification
{
    public class Classification
    {
        public double CalculateEntropy(DataTable mainDataTable, string columnName)
        {
            try
            {
                IEnumerable<string> distinctColumnValues = (from data in mainDataTable.AsEnumerable()
                                                            select data[columnName].ToString()).ToList().Distinct();
                double entropy = 0.0;
                foreach (string value in distinctColumnValues)
                {
                    double valueCount = (from data in mainDataTable.AsEnumerable()
                                         select data[columnName].ToString()).ToList().Where(x => x.ToString() == value).ToList().Count;

                    entropy += ((valueCount.To<double>() / mainDataTable.Rows.Count.To<double>()) * (Math.Log10(mainDataTable.Rows.Count.To<double>() / valueCount.To<double>())));
                }


                return entropy;
            }
            catch (Exception ex)
            {
                throw HException.Save(ex, "DataMining.Engine.Classification.Classification", "CalculateEntropy",
                    new Parameter() { Name = "mainDataTable", Value = "Type of DataTable" },
                    new Parameter() { Name = "columnName", Value = columnName.ToString() });
            }
        }
        public int GetWinner(DataTable mainDataTable, List<string> classColumns)
        {
            try
            {
                double rootEntropy = CalculateEntropy(mainDataTable, classColumns.FirstOrDefault());
                Dictionary<int, double> winRatesOfColumns = new Dictionary<int, double>();
                foreach (DataColumn column in mainDataTable.Columns)
                {
                    if (!classColumns.Contains(column.ColumnName))
                    {
                        int columnIndex = column.Ordinal;
                        double winRate = rootEntropy / CalculateEntropy(mainDataTable, column.ColumnName);
                        winRatesOfColumns.Add(columnIndex, winRate);
                    }
                }
                var sortedDict = from entry in winRatesOfColumns orderby entry.Value ascending select entry;
                KeyValuePair<int, double> winColumnInfo = sortedDict.FirstOrDefault();

                return winColumnInfo.Key;
            }
            catch (Exception ex)
            {
                throw HException.Save(ex, "DataMining.Engine.Classification.Classification", "GetWinner",
                    new Parameter() { Name = "mainDataTable", Value = "Type of DataTable" },
                    new Parameter() { Name = "classColumns", Value = classColumns });
            }
        }
        int branchId = 0;
        public Branch CreateDecisionTree(DataTable reductedMainDataTable, List<string> classColumns, int beforeBranchId, string beforeBranchName, bool isBoleBranch, int learningRate)
        {
            try
            {
                int winnerColumnIndex = GetWinner(reductedMainDataTable, classColumns);
                // classColumns.Add(reductedMainDataTable.Columns[winnerColumnIndex].ColumnName);
                branchId = beforeBranchId + 1;
                Branch branch = new Branch()
                {
                    ID = branchId,
                    IsBole = isBoleBranch,
                    Name = isBoleBranch ? reductedMainDataTable.Columns[winnerColumnIndex].ColumnName : beforeBranchName + "?>#" + reductedMainDataTable.Columns[winnerColumnIndex].ColumnName,
                    PercentageInfos = CalculatePercentage(classColumns.FirstOrDefault(), reductedMainDataTable),
                    ScreenName = classColumns.FirstOrDefault().ToString()
                };

                var winningColumnDatas = (from dataRow in reductedMainDataTable.AsEnumerable()
                                          select dataRow[winnerColumnIndex].ToString()).ToList().Distinct();

                foreach (object value in winningColumnDatas)
                {
                    Branch subBranch = new Branch();

                    subBranch.Name = value.ToString();
                    subBranch.ScreenName = classColumns.FirstOrDefault().ToString();

                    List<DataRow> datasForValue = (from dtRow in reductedMainDataTable.AsEnumerable()
                                                   where dtRow[winnerColumnIndex].ToString() == value.ToString()
                                                   select (DataRow)dtRow).ToList();
                    DataTable table = datasForValue.CopyToDataTable();
                    List<object> classColumnData = (from dtRow in table.AsEnumerable()
                                                    select dtRow[classColumns.FirstOrDefault()]).Distinct().ToList();

                    bool isHaveDifferentValue = IsHaveDifferentValue(table, classColumns.FirstOrDefault());
                    if (classColumnData.Count == 1)
                    {
                        branchId++;
                        subBranch.ID = branchId;
                        subBranch.IsMainBranch = false;
                        Leaf leaf = new Leaf()
                        {
                            Name = classColumns.FirstOrDefault().ToString(),
                            LeafDatas = table,
                            LeafDatasPercentages = CalculatePercentage(classColumns.FirstOrDefault(), table)
                        };
                        subBranch.isFinishLearn = isLearnFinsh(leaf.LeafDatasPercentages, learningRate);
                        subBranch.PercentageInfos = leaf.LeafDatasPercentages;



                        subBranch.Leaf = leaf;
                    }
                    else if (classColumnData.Count > 1 && isHaveDifferentValue)
                    {
                        subBranch.IsMainBranch = true;
                        subBranch = CreateDecisionTree(table, classColumns, branchId, subBranch.Name, false, learningRate);
                    }
                    else if (!isHaveDifferentValue)
                    {
                        branchId++;
                        subBranch.ID = branchId;
                        Leaf leaf = new Leaf()
                        {
                            Name = classColumns.FirstOrDefault().ToString(),
                            LeafDatas = table,
                            LeafDatasPercentages = CalculatePercentage(classColumns.FirstOrDefault(), table)
                        };
                        subBranch.PercentageInfos = leaf.LeafDatasPercentages;
                        subBranch.isFinishLearn = isLearnFinsh(leaf.LeafDatasPercentages, learningRate);
                        subBranch.Leaf = leaf;
                    }
                    if (subBranch.Branches != null || subBranch.Leaf != null)
                    {
                        branch.Branches.Add(subBranch);    

                    }
                }
                return branch;
            }
            catch (Exception ex)
            {
                throw HException.Save(ex, "DataMining.Engine.Classification.Classification", "CreateDecisionTree",
                    new Parameter() { Name = "reductedMainDataTable", Value = "Type of DataTable" },
                    new Parameter() { Name = "classColumns", Value = classColumns },
                    new Parameter() { Name = "beforeBranchId", Value = beforeBranchId.ToString() },
                    new Parameter() { Name = "isBoleBranch", Value = isBoleBranch.ToString() },
                    new Parameter() { Name = "learningRate", Value = learningRate.ToString() },
                    new Parameter() { Name = "beforeBranchName", Value = beforeBranchName.ToString() });
            }
        }

        private bool isLearnFinsh(Dictionary<string, float> percantage, int learningRate)
        {
            try
            {
                List<float> fltValues = (from myVal in percantage
                                         select myVal.Value).ToList();
                foreach (float item in fltValues)
                {
                    if (item < learningRate)
                    {
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                throw HException.Save(ex, "DataMining.Engine.Classification.Classification", "isLearnFinsh",
                    new Parameter() { Name = "percantage", Value = "Type of Dictionary" },
                    new Parameter() { Name = "learningRate", Value = learningRate.ToString() });            
            }
        }

        public static bool IsHaveDifferentValue(DataTable table, string classColumnName)
        {
            try
            {
                int differentCount = 0;
                foreach (DataColumn column in table.Columns)
                {
                    if (classColumnName != column.ColumnName)
                    {
                        for (int i = 0; i < table.Rows.Count; i++)
                        {
                            for (int j = 0; j < table.Rows.Count; j++)
                            {
                                if (table.Rows[i][column.ColumnName].ToString() != table.Rows[j][column.ColumnName].ToString())
                                {
                                    differentCount++;
                                    return true;
                                }
                            }
                        }
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                throw HException.Save(ex, "DataMining.Engine.Classification.Classification", "IsHaveDifferentValue",
                    new Parameter() { Name = "table", Value = "Type of DataTable" },
                    new Parameter() { Name = "classColumnName", Value = classColumnName.ToString() }); 
            }
        }
        public Dictionary<string, float> CalculatePercentage(string className, DataTable leafData)
        {
            try
            {
                Dictionary<string, float> percentagesValues = new Dictionary<string, float>();
                int totalData = leafData.Rows.Count;
                List<string> distinctClassColumnDatas = (from dtrow in leafData.AsEnumerable()
                                                         select dtrow[className].ToString()).ToList();
                foreach (string data in distinctClassColumnDatas.Distinct())
                {
                    var dataCount = distinctClassColumnDatas.Count(x => x.Contains(data));
                    float percentage = (dataCount * 100) / totalData;
                    percentagesValues.Add(data, percentage);
                }
                return percentagesValues;
            }
            catch (Exception ex)
            {
                 throw HException.Save(ex, "DataMining.Engine.Classification.Classification", "CalculatePercentage",
                    new Parameter() { Name = "className", Value = className.ToString() },
                    new Parameter() { Name = "leafData", Value = "Type of DataTable" }); 
            }
        }

    }
}
