using HelpException;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataMining.Engine.Reduction
{
    public class Reduction
    {
        public DataTable Reduce(DataTable filledMissingValuesCopyMainDataTable)
        {
            try
            {
                DataTable returnDataTable = filledMissingValuesCopyMainDataTable.Clone();


                foreach (DataColumn mainDataTableColumn in filledMissingValuesCopyMainDataTable.Columns)
                {
                    returnDataTable.Columns[mainDataTableColumn.ColumnName].DataType = typeof(String);
                }
                foreach (DataRow row in filledMissingValuesCopyMainDataTable.Rows)
                {
                    returnDataTable.Rows.Add(row.ItemArray);
                }
                for (int i = 0; i < filledMissingValuesCopyMainDataTable.Columns.Count; i++)
                {
                    if (filledMissingValuesCopyMainDataTable.Columns[i].DataType == typeof(int))
                    {
                        List<int> values = (from value in filledMissingValuesCopyMainDataTable.AsEnumerable()
                                            select Convert.ToInt32(value[filledMissingValuesCopyMainDataTable.Columns[i].ColumnName])).ToList();
                        List<int> distinctedValues = values.Distinct().ToList();


                        values.Sort();
                        distinctedValues.Sort();

                        List<int> minValuesFivePercent = new List<int>();
                        List<int> maxValuesFivePercent = new List<int>();
                        Dictionary<string, List<int>> percentageMinMaxValues = calculateMinMaxFivePercentValue(distinctedValues);
                        minValuesFivePercent = percentageMinMaxValues["min"];
                        maxValuesFivePercent = percentageMinMaxValues["max"];

                        if (minValuesFivePercent.Count == 0 || maxValuesFivePercent.Count == 0)
                        {
                            minValuesFivePercent.Add(distinctedValues.Min());
                            maxValuesFivePercent.Add(distinctedValues.Max());
                        }

                        List<int> prunedValues = distinctedValues;
                        foreach (int value in minValuesFivePercent)
                            prunedValues.Remove(value);
                        foreach (int value in maxValuesFivePercent)
                            prunedValues.Remove(value);

                        int minValue = prunedValues.Min();
                        int maxValue = prunedValues.Max();

                        string minValueString = Math.Abs(minValue).ToString(); //Eski değer gelme ihtimaline karşı
                        string maxValueString = Math.Abs(maxValue).ToString();

                        int mostEffectiveStep = 0;
                        int range = 0;

                        if (maxValueString.Length > minValueString.Length)
                        {
                            mostEffectiveStep = Convert.ToInt32(maxValueString[0].ToString());
                            if (mostEffectiveStep == 1)
                            {
                                range = Convert.ToInt32(maxValueString[0].ToString());
                            }
                            else
                            {
                                range = Convert.ToInt32(maxValueString[0].ToString());
                            }
                        }
                        else
                        {
                            for (int j = 0; j < minValueString.Length; j++)
                            {
                                if (maxValueString[j] > minValueString[j])
                                {
                                    mostEffectiveStep = Convert.ToInt32(maxValueString[j].ToString());
                                    range = maxValueString[j] - minValueString[j];
                                    break;
                                }
                            }
                        }
                        int groupCount = 0;
                        switch (range)
                        {
                            case 3:
                            case 6:
                            case 7:
                            case 9:
                                groupCount = 3;
                                break;
                            case 2:
                            case 4:
                            case 8:
                                groupCount = 4;
                                break;
                            case 1:
                            case 5:
                            case 10:
                                groupCount = 5;
                                break;
                            default:
                                break;
                        }

                        List<string> groups = new List<string>();
                        if (groupCount != 0)
                        {
                            int increment = (maxValue - minValue) / groupCount;
                            groups.Add(minValuesFivePercent.Min().ToString() + "-" + minValue.ToString());
                            int sum = minValue;
                            for (int j = 0; j < groupCount; j++)
                            {
                                string group = sum.ToString() + "-" + (sum += increment).ToString();
                                groups.Add(group);
                            }
                            groups.Add(maxValue.ToString() + "-" + maxValuesFivePercent.Max().ToString());
                        }
                        else // No range
                        {
                            groups.Add(minValuesFivePercent.Min().ToString() + "-" + maxValuesFivePercent.Max());
                        }
                        foreach (DataRow rw in returnDataTable.Rows)
                        {
                            foreach (string group in groups)
                            {
                                if (Convert.ToInt32(rw[i].ToString()) <= Convert.ToInt32(group.Split('-')[1]))
                                {
                                    rw[i] = group;
                                    break;
                                }
                            }
                        }
                    }

                }
                return returnDataTable;
            }
            catch (Exception ex)
            {
                throw HException.Save(ex, "DataMining.Engine.Reduction.Reduction", "Reduce",
                    new Parameter() { Name = "filledMissingValuesCopyMainDataTable", Value = "Type of DataTable" });
            }
        }
        private Dictionary<string, List<int>> calculateMinMaxFivePercentValue(List<int> values)
        {
            try
            {
                Dictionary<string, List<int>> returnMinMaxValues = new Dictionary<string, List<int>>();
                List<int> minValues = new List<int>();
                List<int> maxValues = new List<int>();
                int percentage = 5;
                while (minValues.Count < 2)
                {
                    double d = (Convert.ToDouble(values.Count) * Convert.ToDouble(percentage)) / Convert.ToDouble(100);
                    int round = Convert.ToInt32(Math.Round(d, 2));
                    minValues = values.Take(round).ToList();
                    maxValues = values.Skip(values.Count - round).Take(round).ToList();
                    percentage += 5;
                }
                returnMinMaxValues.Add("min", minValues);
                returnMinMaxValues.Add("max", maxValues);

                return returnMinMaxValues;
            }
            catch (Exception ex)
            {
                throw HException.Save(ex, "DataMining.Engine.Reduction.Reduction", "calculateMinMaxFivePercentValue",
                   new Parameter() { Name = "values", Value = values });
            }
        }

    }
}
