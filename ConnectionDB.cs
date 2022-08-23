using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAO
{
    public class ConnectionDB
    {

        public string ConnectionString { get; set; }
        private bool isLocal;
        private bool isAutantication;
        public string DatabaseIP { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public string DatabaseName { get; set; }

        /// <summary>
        /// If 
        /// </summary>
        /// <param name="databaseIp"></param>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        public ConnectionDB(string databaseIp , string userName,string password)
        {
            this.DatabaseIP = databaseIp;
            this.Password = password;
            this.UserName = userName;
            isLocal = false;
            isAutantication = false;
        }

        /// <summary>
        /// If Autantication Mode Use This Function
        /// </summary>
        /// <param name="databaseIp"></param>
        public ConnectionDB(string databaseIp)
        {
            this.DatabaseIP = databaseIp;
            isLocal = false;
            isAutantication = true;
        }
        public ConnectionDB(string userName , string password)
        {
            this.UserName = userName;
            isLocal = true;
            isAutantication = false;
            
        }
        public ConnectionDB()
        {
            this.DatabaseIP = "localhost";
            isLocal = true;
            isAutantication = true;
        }

        public SqlConnection GetSqlConnection()
        {
            try
            {
                if(isAutantication && isLocal)
                {
                    if(DatabaseName!=null)
                    {
                        this.ConnectionString = "Data Source=localhost;Initial Catalog=" + this.DatabaseName + "; Integrated Security=SSPI;";
                    }
                    else
                    {
                        this.ConnectionString = "Data Source=localhost; Integrated Security=SSPI;";
                    }
                }
                else if(isAutantication)
                {
                    if (DatabaseName != null)
                    {
                        this.ConnectionString = "Data Source=" + this.DatabaseIP + ";Initial Catalog=" + this.DatabaseName + ";Integrated Security=SSPI;";
                    }
                    else
                    {
                        this.ConnectionString = "Data Source=" + this.DatabaseIP + ";Integrated Security=SSPI;";
                    }
                    
                }
                else if(isLocal)
                {
                    if (DatabaseName != null)
                    {
                        this.ConnectionString = "Data Source=localhost;Initial Catalog=" + this.DatabaseName + ";User Id=" + this.UserName + ";Password=" + this.Password + ";";
                    }
                    else
                    {
                        this.ConnectionString = "Data Source=localhost;User Id=" + this.UserName + ";Password=" + this.Password + ";";
                    }
                    
                }
                else
                {
                    this.ConnectionString = "Data Source="+this.DatabaseIP+";Initial Catalog=" + this.DatabaseName + ";User Id=" + this.UserName + ";Password=" + this.Password + ";";
                }


                return new SqlConnection(this.ConnectionString);
            }
            catch (Exception ex)
            {
                throw ex;
                //throw HException.Save(ex, "DAO.ConnectionDB", "GetSqlConnection");
            }
        }
        
        
    }
}
