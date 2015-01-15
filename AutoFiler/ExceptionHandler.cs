using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Windows.Forms;
using System.Configuration;

namespace AutoFiler
{
    public static class Exceptions  
    {
        public static void ExceptionHandler(this Exception exc)
        {
            try
            {
                //INNEREXCEPTION WILL THROW AN ERROR IF IT'S NULL, DOUBLECHECK BEFORE ATTEMPTING TO CONVERT IT TO A STRING
                string strDetails = exc.InnerException == null ? "Not available" : exc.InnerException.ToString();

                //WRITE THE ERROR TO THE LOG
                ErrorLog log = new ErrorLog { message = exc.Message.ToString(), details = strDetails.ToString() + Environment.NewLine + exc.StackTrace.ToString() }.WriteToLog();

                //DISPLAY THE ERROR TO THE USER
                StringBuilder sBldr = new StringBuilder();
                sBldr.Append("[MESSAGE]" + Environment.NewLine + log.message);
                sBldr.Append(Environment.NewLine + Environment.NewLine);
                sBldr.Append("[INNEREXCEPTION]" + Environment.NewLine + log.details);
                sBldr.Append(Environment.NewLine + Environment.NewLine);
                sBldr.Append("[EXCEPTION]" + Environment.NewLine + exc.ToString());
                sBldr.Append(Environment.NewLine + Environment.NewLine);
                sBldr.Append("[STACKTRACE]" + Environment.NewLine + exc.StackTrace);
                sBldr.Append(Environment.NewLine + Environment.NewLine);

                //frmErrors errorForm = new frmErrors();
                //errorForm.Text = "Unexpected Error";
                //errorForm.txtDetails.Text = sBldr.ToString();
                //errorForm.ShowDialog();
            }
            catch (Exception ex)
            {
                StringBuilder sBldr = new StringBuilder();
                sBldr.Append("An unexpected error occurred while processing your request." + Environment.NewLine + Environment.NewLine);
                sBldr.Append("Error Details: " + Environment.NewLine + ex.ToString());

                MessageBox.Show(sBldr.ToString(), "Unexpected Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

    }

    public class ErrorLog
    {
        public string message { get; set; }
        public string details { get; set; }

        /* STRUCTURE OF ERRORLOG.XML
         * <ErrorLog>
         *  <Incident>
         *   <DateTimeStamp></DateTimeStamp>
         *   <Message></Message>
         *   <Details></Details>
         *  </Incident>
         * </ErrorLog>
        */

        public ErrorLog WriteToLog()
        {
            try
            {
                string filePath = "ErrorLog.xml";
                string schemaPath = "ErrorLog.xsd";
                DataSet errors = new DataSet("ErrorLog.xml").GetDataFromXML(filePath, schemaPath);
                DataRow dr = errors.Tables[0].NewRow();
                dr["DateTimeStamp"] = DateTime.Now.ToString();
                dr["Message"] = message;
                dr["Details"] = details;
                errors.AddRowToXMLDataSource(0, filePath, schemaPath, dr);
                //AutoFiler.DataAccess.AddRowToXMLDataSource(errors, 
            }
            catch (Exception exc)
            {
                MessageBox.Show("An unexpected error occurred while logging the error you just encountered. The error may not have been logged." +
                Environment.NewLine + Environment.NewLine + "Details: " + exc.ToString(), "Error Tracker", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            return this;
        }

        public static void PurgeErrorLog(int purgeDays)
        {
            try
            {
                string filePath = "ErrorLog.xml";
                string schemaPath = "ErrorLog.xsd";
                DataSet errors = new DataSet("ErrorLog").GetDataFromXML(filePath, schemaPath);
                int purgeErrorLog = purgeDays;

                if (purgeErrorLog == 0)
                {
                    purgeErrorLog = Convert.ToInt32("PURGE_ERROR_LOG");
                }

                foreach (DataRow dr in errors.Tables[0].Rows)
                {
                    DateTime dateToCompare = Convert.ToDateTime(dr["DateTimeStamp"].ToString());
                    TimeSpan timeSpan = DateTime.Now - dateToCompare;

                    if (timeSpan.Days >= purgeErrorLog)//only keeping 7 days worth of data in the log file - xml files grow large...fast
                    {
                        errors.DeleteRowFromXMLDataSource("DateTimeStamp", dateToCompare.ToString(), "Incident", filePath);
                        PurgeErrorLog(purgeErrorLog);
                    }
                }
            }
            catch (InvalidOperationException)
            {
                #region This catches the following exception when occurs after the last "out-dated" Incident is removed from the DataSet.
                //<Incident>
                //  <DateTimeStamp>6/16/2010 2:45:43 PM</DateTimeStamp>
                //  <Message>Collection was modified; enumeration operation might not execute.</Message>
                //  <Details>Not available
                //     at System.Data.RBTree`1.RBTreeEnumerator.MoveNext()
                //     at POS_Dashboard.BusinessObjectsLayer.ErrorLog.PurgeErrorLog() in C:\Users\Michelle.Edmondson\Desktop\POS\POS_Dashboard\BusinessObjectsLayer\ErrorLog.cs:line 46
                //  </Details>
                //</Incident>
                #endregion
            }
            catch (Exception exc)
            {
                exc.ExceptionHandler();
            }
        }
    }

}
