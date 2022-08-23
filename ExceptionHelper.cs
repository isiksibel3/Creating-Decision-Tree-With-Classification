using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace HelpException
{
    public static class ExceptionHelper
    {
        public static bool IsConnectDB { get; set; }
        /// <summary>
        /// Params tipinde obkeler için eğer data 100 den fazla değilse ve int, string tipindeyse params kadar parametre array döner gelen obje ise type ile bilgilendirlimiş parametre döner.
        /// <returns></returns>
        public static Parameter[] ConverToParameter<T>(this T data) where T : ICollection,IList
        {
            try
            {
                              
                if (data.Count > 4)
                {
                    return new Parameter[] { new Parameter() { Name = "ParamsData", Value = "More than 4 data (Type of " + data[0].GetType().ToString() + ")" } };
                }
                else
                {
                    Parameter[] prmList = new Parameter[data.Count];
                    if (data.Count > 0)
                    {
                        if (data[0].GetType() == typeof(string) || data[0].GetType() == typeof(int) || data[0].GetType() == typeof(double) || data[0].GetType() == typeof(float))
                        {
                            int counter = 0;
                            foreach (var item in data)
                            {
                                Parameter prm = new Parameter();
                                prm.Name = item.GetType().ToString();
                                prm.Value = item.ToString();
                                prmList[counter] = prm;
                                counter++;
                            }
                        }
                        else
                        {
                            prmList = new Parameter[1];
                            Parameter prm = new Parameter() { Name = "ParamsData", Value = "Type of" + data[0].GetType().ToString() };
                            prmList[0] = prm;
                        }
                    }
                    else
                    {
                        prmList = new Parameter[1];
                        Parameter prm = new Parameter() { Name = "ParamsData", Value = "NullData" };
                        prmList[0] = prm;
                    }
                    return prmList;
                }
            }
            catch (Exception ex)
            {
                throw HException.Save(ex, "HelpException.ExceptionHelper", "ConverToParameter<T>", new Parameter() { Name = "data", Value = "Type of" + data.GetType().ToString() });
            }
        }

        public static bool Serialize(this object cls , ref string resultXml)
        {
            try
            {
                MemoryStream memoryStream = new MemoryStream();

                XmlSerializer xs = new XmlSerializer(cls.GetType());
                using (StreamWriter xmlTextWriter = new StreamWriter(memoryStream))
                {
                    xs.Serialize(xmlTextWriter, cls);
                    xmlTextWriter.Flush();
                    //xmlTextWriter.Close();
                    memoryStream = (MemoryStream)xmlTextWriter.BaseStream;
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    StreamReader reader = new StreamReader(memoryStream);

                    resultXml = reader.ReadToEnd();
                }
                return true;
                //IFormatter formatter = new BinaryFormatter();
                //Stream str = new System.IO.MemoryStream();
                //formatter.Serialize(str, cls);
                //BinaryReader rd = new BinaryReader(str);
                //resultXml = rd.ToString();
                //return true;
            }
            catch (Exception)
            {
                return false;
            }

        }
    }
}
