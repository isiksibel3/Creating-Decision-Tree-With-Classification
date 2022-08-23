using DataMining.Engine.Objects;
using HelpException;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataMining.Engine.ReportCreator
{
    public class ClassificationReport
    {
        string tab = "\t";

        public string CreteDecisionTreeToHtml(Branch mainBranch, string classColumnName, bool isMainBranch, string tab)
        {
            try
            {
                string first = Properties.Resources.first;

                string bottom = CreateDecisionTreeHtml(mainBranch, classColumnName, isMainBranch, tab) + "}";

                string last = Properties.Resources.last;

                return first + bottom + last;
            }
            catch (Exception ex)
            {
                throw HException.Save(ex, "DataMining.Engine.ReportCreator.ClassificationReportn", "CreteDecisionTreeToHtml",
                    new Parameter() { Name = "mainBranch", Value = "Type of Branch" },
                    new Parameter() { Name = "classColumnName", Value = classColumnName.ToString() },
                    new Parameter() { Name = "isMainBranch", Value = isMainBranch.ToString() },
                    new Parameter() { Name = "tab", Value = tab.ToString() });
            }
        }

        private string CreateDecisionTreeHtml(Branch mainBranch, string classColumnName, bool isMainBranch, string tab)
        {
            try
            {

                string newLine = "";
                if (isMainBranch)
                {
                    newLine = tab + "{\n " + tab + "'name':'" + classColumnName + "',";
                    if (mainBranch.PercentageInfos != null)
                    {
                        newLine += "'percentage':'";
                        foreach (KeyValuePair<string, float> percentage in mainBranch.PercentageInfos)
                        {
                            newLine += "<tr><td>" + percentage.Key + "</td><td>" + percentage.Value.ToString("0.00") + "</td></tr>";
                        }
                        newLine += "',";
                        newLine += "'parent':'null'," +
                           "'children':\n " + tab + "[\n ";
                    }
                    else
                    {
                        newLine += "'percentage':'<tr><td></td></tr>',";
                    }
                }

                string[] seperator = new string[] { "?>#" };
                string[] splitNames = mainBranch.Name.Split(seperator, StringSplitOptions.None);

                tab += "\t";
                newLine += tab + "{\n " + tab + "'name':'" + splitNames[0] + "',";
                if (mainBranch.PercentageInfos != null)
                {
                    newLine += "'percentage':'";
                    foreach (KeyValuePair<string, float> percentage in mainBranch.PercentageInfos)
                    {
                        newLine += "<tr><td>" + percentage.Key + "</td><td>" + percentage.Value.ToString("0.00") + "</td></tr>";
                    }
                    string childeren = ",'children':\n" + tab + "[\n "; // main branc içinde branches yoksa hiç childeren ekleme

                    newLine += "',";
                    newLine += "'parent':'" + classColumnName + "'" +
                       childeren;
                }
                else
                {
                    string childeren = ",'children':\n" + tab + "[\n "; // main branc içinde branches yoksa hiç childeren ekleme

                    newLine += "'percentage':'<tr><td></td></tr>',";
                    newLine += "'parent':'" + classColumnName + "'" +
                       childeren;
                }
                if (splitNames.Length > 1)
                {
                    tab += "\t";
                    for (int i = 0; i < splitNames.Length; i++)
                    {
                        if (splitNames[i] != splitNames[0])
                        {

                            newLine += tab + "{\n" + tab + " 'name':'" + splitNames[i].ToString() + "',";
                            if (mainBranch.PercentageInfos != null)
                            {
                                newLine += "'percentage':'";
                                foreach (KeyValuePair<string, float> percentage in mainBranch.PercentageInfos)
                                {
                                    newLine += "<tr><td>" + percentage.Key + "</td><td>" + percentage.Value.ToString("0.00") + "</td></tr>";
                                }

                                string childeren = ",'children':\n " + tab + "[\n"; // main branc içinde branches yoksa hiç childeren ekleme

                                newLine += "',";
                                newLine += "'parent':'" + splitNames[i - 1] + "'" +
                                   childeren;
                            }
                        }
                    }
                }
                if (mainBranch.Branches != null && mainBranch.Branches.Count > 0)
                {
                    foreach (Branch branch in mainBranch.Branches)
                    {

                        newLine += CreateDecisionTreeHtml(branch, splitNames[splitNames.Length - 1], false, tab) + "\n";


                        if (branch != mainBranch.Branches.Last())
                        {
                            newLine += tab + "\t" + "},\n";
                        }
                        else
                        {
                            newLine += tab + "\t" + "}\n";
                        }
                    }

                    newLine += tab + "]\n";
                    newLine += tab + "}";

                    newLine += "\n" + tab + "]\n";

                }
                else
                {

                    tab += "\t";
                    newLine += tab + "{\n " + tab + " 'name':'" + mainBranch.ScreenName + "',";
                    if (mainBranch.PercentageInfos != null)
                    {
                        newLine += "'percentage':'";
                        foreach (KeyValuePair<string, float> percentage in mainBranch.PercentageInfos)
                        {
                            newLine += "<tr><td>" + percentage.Key + "</td><td>" + percentage.Value.ToString("0.00") + "</td></tr>";
                        }
                        newLine += "',";
                        string[] seperator1 = new string[] { "?>#" };
                        string[] splitNames1 = mainBranch.Name.Split(seperator1, StringSplitOptions.None);


                        newLine += "'parent':'" + splitNames1[splitNames1.Length - 1] + "'";
                        newLine += tab + "}";
                        tab = tab.Remove(tab.Length - 1);
                        newLine += "\n" + tab + "]";
                        tab += "\t";
                    }
                    else
                    {

                    }
                }
                return newLine;
            }
            catch (Exception ex)
            {
                throw HException.Save(ex, "DataMining.Engine.ReportCreator.ClassificationReportn", "CreateDecisionTreeHtml",
                    new Parameter() { Name = "mainBranch", Value = "Type of Branch" },
                    new Parameter() { Name = "classColumnName", Value = classColumnName.ToString() },
                    new Parameter() { Name = "isMainBranch", Value = isMainBranch.ToString() },
                    new Parameter() { Name = "tab", Value = tab.ToString() });
            }
        }
    }
}
