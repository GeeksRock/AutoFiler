using System;
using System.Configuration.Internal;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using MySql.Data.MySqlClient;

namespace AutoFiler
{
    public static class DataAccess
    {
        //http://www.codeproject.com/Articles/43438/Connect-C-to-MySQL
        //http://www.sqlinfo.net/mysql/mysql_stored_procedure_SELECT.php
        //http://forums.asp.net/t/988462.aspx
        public static MySqlConnection Connection(this MySqlConnection msc)
        {
            msc = new MySqlConnection();
            string myConnectionString = Properties.Settings.Default.ConnectionString;
            msc.ConnectionString = myConnectionString;
            return msc;
        }
        public static bool DBConnectOpen(MySqlConnection conn)
        {
            try
            {
                conn.Open();
            }
            catch (MySql.Data.MySqlClient.MySqlException exc)
            {
                throw (exc);
            }
            return true;
        }
        public static bool DBConnectClose(MySqlConnection conn)
        {
            try
            {
                conn.Close();
            }
            catch (MySql.Data.MySqlClient.MySqlException exc)
            {
                throw (exc);
            }
            return true;
        }

        public static int AddNewFolderDestination(this int id, string destination)
        {
            id = 0;

            MySqlConnection msc = new MySqlConnection().Connection();
            string query = Properties.Settings.Default.AddNewFolderDestination;

            if (DBConnectOpen(msc) == true)
            {
                try
                {
                    MySqlCommand cmd = new MySqlCommand(query, msc);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new MySqlParameter("destination", destination));
                    cmd.ExecuteNonQuery();

                    DataSet ds = new DataSet().GetDestinationIdByName(destination);

                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        return id = Convert.ToInt16(dr["idDestinations"].ToString());
                    }
                }
                catch (MySql.Data.MySqlClient.MySqlException exc)
                {
                    throw exc;
                }
                DBConnectClose(msc);
            }
            return id;
        }
        public static bool AddNewCustomFileType(this bool isCreated, string filetype, int id)
        {
            MySqlConnection msc = new MySqlConnection().Connection();
            string query = Properties.Settings.Default.AddNewCustomFileType;

            if (DBConnectOpen(msc) == true)
            {
                try
                {
                    MySqlCommand cmd = new MySqlCommand(query, msc);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new MySqlParameter("filetype", filetype));
                    cmd.Parameters.Add(new MySqlParameter("id", id));
                    cmd.ExecuteNonQuery();
                    isCreated = true;
                }
                catch (MySql.Data.MySqlClient.MySqlException exc)
                {
                    isCreated = false;
                    throw exc;
                }
            }
            return isCreated;
        }
        public static bool AddNewFileName(this bool isCreated, string filename, string fileoption, bool over_ride, int id)
        {
            MySqlConnection msc = new MySqlConnection().Connection();
            string query = Properties.Settings.Default.AddNewFileName;

            if (DBConnectOpen(msc) == true)
            {
                try
                {
                    MySqlCommand cmd = new MySqlCommand(query, msc);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new MySqlParameter("filename", filename));
                    cmd.Parameters.Add(new MySqlParameter("fileoption", fileoption));
                    cmd.Parameters.Add(new MySqlParameter("override", over_ride));
                    cmd.Parameters.Add(new MySqlParameter("id", id));
                    cmd.ExecuteNonQuery();
                    isCreated = true;
                }
                catch (MySql.Data.MySqlClient.MySqlException exc)
                {
                    isCreated = false;
                    throw exc;
                }
            }
            return isCreated;
        }

        public static bool AssociatePreloadedFileType(this bool isAssociated, string filetype, int id)
        {
            MySqlConnection msc = new MySqlConnection().Connection();
            string query = Properties.Settings.Default.AssociatePreloadedFileType;

            if (DBConnectOpen(msc) == true)
            {
                try
                {
                    MySqlCommand cmd = new MySqlCommand(query, msc);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new MySqlParameter("filetype", filetype));
                    cmd.Parameters.Add(new MySqlParameter("id", id));
                    cmd.ExecuteNonQuery();
                    isAssociated = true;
                }
                catch (MySql.Data.MySqlClient.MySqlException exc)
                {
                    isAssociated = false;
                    throw exc;
                }
            }
            return isAssociated;
        }
        
        public static bool UpdateFolderDestination(this bool isUpdated, string destination, int id)
        {
            MySqlConnection msc = new MySqlConnection().Connection();
            string query = Properties.Settings.Default.UpdateFolderDestination;

            if (DBConnectOpen(msc) == true)
            {
                try
                {
                    MySqlCommand cmd = new MySqlCommand(query, msc);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new MySqlParameter("destination", destination));
                    cmd.Parameters.Add(new MySqlParameter("id", id));
                    cmd.ExecuteNonQuery();
                    isUpdated = true;
                }
                catch (MySql.Data.MySqlClient.MySqlException exc)
                {
                    isUpdated = false;
                    throw exc;
                }
            }
            return isUpdated;
        }

        public static bool RemovePreloadedFileAssociation(this bool isRemoved, string filetype)
        {
            MySqlConnection msc = new MySqlConnection().Connection();
            string query = Properties.Settings.Default.RemovePreloadedFileAssociation;

            if (DBConnectOpen(msc) == true)
            {
                try
                {
                    MySqlCommand cmd = new MySqlCommand(query, msc);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new MySqlParameter("filetype", filetype));
                    cmd.ExecuteNonQuery();
                    isRemoved = true;
                }
                catch (MySql.Data.MySqlClient.MySqlException exc)
                {
                    isRemoved = false;
                    throw exc;
                }
            }
            return isRemoved;
        }
        public static bool RemoveCustomFileType(this bool isRemoved, string filetype)
        {
            MySqlConnection msc = new MySqlConnection().Connection();
            string query = Properties.Settings.Default.RemoveCustomFileType;

            if (DBConnectOpen(msc) == true)
            {
                try
                {
                    MySqlCommand cmd = new MySqlCommand(query, msc);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new MySqlParameter("filetype", filetype));
                    cmd.ExecuteNonQuery();
                    isRemoved = true;
                }
                catch (MySql.Data.MySqlClient.MySqlException exc)
                {
                    isRemoved = false;
                    throw exc;
                }
            }
            return isRemoved;
        }
        public static bool RemoveFileName(this bool isRemoved, string filename)
        {
            MySqlConnection msc = new MySqlConnection().Connection();
            string query = Properties.Settings.Default.RemoveFileName;

            if (DBConnectOpen(msc) == true)
            {
                try
                {
                    MySqlCommand cmd = new MySqlCommand(query, msc);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new MySqlParameter("filename", filename));
                    cmd.ExecuteNonQuery();
                    isRemoved = true;
                }
                catch (MySql.Data.MySqlClient.MySqlException exc)
                {
                    isRemoved = false;
                    throw exc;
                }
            }
            return isRemoved;
        }
        public static bool DeleteFolderDestination(this bool isDeleted, string destination)
        {
            MySqlConnection msc = new MySqlConnection().Connection();
            string query = Properties.Settings.Default.DeleteFolderDestination;

            if (DBConnectOpen(msc) == true)
            {
                try
                {
                    DataSet idDS = new DataSet().GetDestinationIdByName(destination);
                    int id = 0;
                    foreach (DataRow dr in idDS.Tables[0].Rows)
                    {
                        id = Convert.ToInt16(dr["idDestinations"].ToString());
                    }
                    MySqlCommand cmd = new MySqlCommand(query, msc);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new MySqlParameter("id", id));
                    cmd.ExecuteNonQuery();
                    isDeleted = true;
                }
                catch (MySql.Data.MySqlClient.MySqlException exc)
                {
                    isDeleted = false;
                    throw exc;
                }
            }
            return isDeleted;
        }

        public static DataSet GetAllPreloadedFileTypes(this DataSet ds)
        {
            MySqlConnection msc = new MySqlConnection().Connection();
            string query = Properties.Settings.Default.GetAllPreloadedFileTypes;

            if (DBConnectOpen(msc) == true)
            {
                try
                {
                    MySqlCommand cmd = new MySqlCommand(query, msc);
                    cmd.CommandType = CommandType.StoredProcedure;
                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                    adapter.Fill(ds);
                }
                catch (MySql.Data.MySqlClient.MySqlException exc)
                {
                    throw (exc);
                }
                DBConnectClose(msc);
            }
            return ds;
        }
        public static DataSet GetAllCustomFileTypes(this DataSet ds)
        {
            MySqlConnection msc = new MySqlConnection().Connection();
            string query = Properties.Settings.Default.GetAllCustomFileTypes;

            if (DBConnectOpen(msc) == true)
            {
                try
                {
                    MySqlCommand cmd = new MySqlCommand(query, msc);
                    cmd.CommandType = CommandType.StoredProcedure;
                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                    adapter.Fill(ds);
                }
                catch (MySql.Data.MySqlClient.MySqlException exc)
                {
                    throw (exc);
                }
                DBConnectClose(msc);
            }
            return ds;
        }
        public static DataSet GetAllFolderDestinations(this DataSet ds)
        {
            MySqlConnection msc = new MySqlConnection().Connection();
            string query = Properties.Settings.Default.GetAllFolderDestinations;

            if (DBConnectOpen(msc) == true)
            {
                try
                {
                    MySqlCommand cmd = new MySqlCommand(query, msc);
                    cmd.CommandType = CommandType.StoredProcedure;
                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                    adapter.Fill(ds);
                }
                catch (MySql.Data.MySqlClient.MySqlException exc)
                {
                    throw (exc);
                }
                DBConnectClose(msc);
            }
            return ds;
        }
        public static DataSet GetAllFileNames(this DataSet ds)
        {
            MySqlConnection msc = new MySqlConnection().Connection();
            string query = Properties.Settings.Default.GetAllFileNames;

            if (DBConnectOpen(msc) == true)
            {
                try
                {
                    MySqlCommand cmd = new MySqlCommand(query, msc);
                    cmd.CommandType = CommandType.StoredProcedure;
                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                    adapter.Fill(ds);
                }
                catch (MySql.Data.MySqlClient.MySqlException exc)
                {
                    throw (exc);
                }
                DBConnectClose(msc);
            }
            return ds;
        }

        public static DataSet GetPreloadedFileTypes(this DataSet ds, string destination)
        {
            MySqlConnection msc = new MySqlConnection().Connection();
            string query = Properties.Settings.Default.GetPreloadedFileTypes;

            if (DBConnectOpen(msc) == true)
            {
                try
                {
                    MySqlCommand cmd = new MySqlCommand(query, msc);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new MySqlParameter("destination", destination));
                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                    adapter.Fill(ds);                 
                }
                catch (MySql.Data.MySqlClient.MySqlException exc)
                {
                    throw (exc);
                }
                DBConnectClose(msc);
            }
            return ds;
        }
        public static DataSet GetCustomFileTypes(this DataSet ds, string destination)
        {
            MySqlConnection msc = new MySqlConnection().Connection();
            string query = Properties.Settings.Default.GetCustomFileTypes;

            if (DBConnectOpen(msc) == true)
            {
                try
                {
                    MySqlCommand cmd = new MySqlCommand(query, msc);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new MySqlParameter("destination", destination));
                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                    adapter.Fill(ds);
                }
                catch (MySql.Data.MySqlClient.MySqlException exc)
                {
                    throw (exc);
                }
                DBConnectClose(msc);
            }
            return ds;
        }
        public static DataSet GetFileNames(this DataSet ds, string destination)
        {
            MySqlConnection msc = new MySqlConnection().Connection();
            string query = Properties.Settings.Default.GetFileNames;

            if (DBConnectOpen(msc) == true)
            {
                try
                {
                    MySqlCommand cmd = new MySqlCommand(query, msc);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new MySqlParameter("destination", destination));
                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                    adapter.Fill(ds);
                }
                catch (MySql.Data.MySqlClient.MySqlException exc)
                {
                    throw (exc);
                }
                DBConnectClose(msc);
            }
            return ds;
        }

        public static DataSet GetDestinationIdByName(this DataSet ds, string destination)
        {
            MySqlConnection msc = new MySqlConnection().Connection();
            string query = Properties.Settings.Default.GetDestinationIdByName;

            if (DBConnectOpen(msc) == true)
            {
                try
                {
                    MySqlCommand cmd = new MySqlCommand(query, msc);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new MySqlParameter("destination", destination));
                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                    adapter.Fill(ds);
                }
                catch (MySql.Data.MySqlClient.MySqlException exc)
                {
                    throw (exc);
                }
                DBConnectClose(msc);
            }
            return ds;
        }
        public static DataSet GetDestinationNameById(this DataSet ds, int id)
        {
            MySqlConnection msc = new MySqlConnection().Connection();
            string query = Properties.Settings.Default.GetDestinationNameById;

            if (DBConnectOpen(msc) == true)
            {
                try
                {
                    MySqlCommand cmd = new MySqlCommand(query, msc);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new MySqlParameter("id", id));
                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                    adapter.Fill(ds);
                }
                catch (MySql.Data.MySqlClient.MySqlException exc)
                {
                    throw (exc);
                }
                DBConnectClose(msc);
            }
            return ds;
        }

        #region xml data access methods for error logs
        public static DataSet AddRowToXMLDataSource(this DataSet ds, int dataTable, string xmlFile, string xmlSchema, DataRow dr)
        {
            ds.ReadXml(xmlFile);
            ds.ReadXmlSchema(xmlSchema);

            DataRow dataRow = ds.Tables[dataTable].NewRow();
            dataRow = dr;
            ds.Tables[dataTable].Rows.Add(dataRow);
            ds.UpdateXMLDataSource(xmlFile, true);
            return ds;
        }

        public static DataSet UpdateXMLDataSource(this DataSet ds, string xmlFile, bool writeXML)
        {
            ds.GetChanges();
            if (writeXML == true) { ds.WriteXml(xmlFile, XmlWriteMode.IgnoreSchema); }
            ds.AcceptChanges();
            return ds;
        }

        public static DataSet GetDataFromXML(this DataSet ds, string xmlFile, string xmlSchema)
        {
            ds.ReadXmlSchema(xmlSchema);
            ds.ReadXml(xmlFile);
            return ds;
        }

        public static DataSet DeleteRowFromXMLDataSource(this DataSet ds, string idColumn, string idValue, string dataTable, string xmlFile)
        {
            foreach (DataRow dr in ds.Tables[dataTable].Rows)
            {
                if (dr[idColumn].ToString() == idValue.ToString())
                {
                    dr.Delete();
                    ds.UpdateXMLDataSource(xmlFile, true);
                    break;
                }
            }

            return ds;
        }
        #endregion
    }
}
