using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using SimsCCManager.Globals;

namespace SimsCCManager.Debugging
{
    public class Logging
    {
        /// <summary>
        /// Synchronous log file implementation. 
        /// </summary>
        
        private static string debuglog = Path.Combine(GlobalVariables.LogFolder, "debug.log");
        static ReaderWriterLock locker = new ReaderWriterLock();
        static bool initialized = false;
        //Function for logging to the logfile set at the start of the program

        public static void JustWriteLog(string statement)
        {
            JustWrite(statement);
        }

        public static void WriteDebugLog(string statement, [CallerLineNumber] int lineNumber = 0, [CallerFilePath] string filePath = "", int alertLevel = 0){
            if (!Directory.Exists(GlobalVariables.LogFolder)) Directory.CreateDirectory(GlobalVariables.LogFolder);
            
                string filepath = "";
                if (filePath != "") {
                    filepath = new FileInfo(filePath).Name;
                } else {
                    filepath = "unknown";
                }        
                if (File.Exists(debuglog)){
                    if (initialized)
                    {
                        WriteLog(statement, lineNumber.ToString(), filepath);
                    } else
                    {
                        initialized = true;
                        string lastwritetime = File.GetLastWriteTimeUtc(debuglog).ToString("yyyy-dd-MM hh_mm_ss");
                        string olddebuglog = Path.Combine(GlobalVariables.LogFolder, string.Format("{0}_{1}.log", lastwritetime, "DebugLog"));
                        File.Move(debuglog, olddebuglog);
                        CreateLog(statement, lineNumber.ToString(), filepath);
                    }            
                } else {    
                    initialized = true;
                    CreateLog(statement, lineNumber.ToString(), filepath);            
                }

                if (alertLevel == 3)
                {
                    WriteExceptionReport(new StringBuilder().Append(statement));
                }           
            
        }

        private static void WriteLog(string statement, string lineNumber, string filepath)
        {   string time = DateTime.Now.ToString("h:mm:ss tt");
            statement = string.Format("[L{0} | {1} {2}]: {3}", lineNumber, filepath, time, statement);
                    
            try
                {
                    locker.AcquireWriterLock(int.MaxValue); 
                    StreamWriter addToInternalLog = new StreamWriter (debuglog, append: true);
                    addToInternalLog.WriteLine(statement);
                    addToInternalLog.Close();
                }
                finally
                {
                    locker.ReleaseWriterLock();
                } 
                GlobalVariables.mainWindow.WriteGDPrint(statement);
        }

        private static void JustWrite(string statement)
        {           
            try
                {
                    locker.AcquireWriterLock(int.MaxValue); 
                    StreamWriter addToInternalLog = new StreamWriter (debuglog, append: true);
                    addToInternalLog.WriteLine(statement);
                    addToInternalLog.Close();
                }
                finally
                {
                    locker.ReleaseWriterLock();
                } 
                GlobalVariables.mainWindow.WriteGDPrint(statement);
        }

        private static void CreateLog(string statement, string lineNumber, string filepath)
        {
    
            string time = DateTime.Now.ToString("h:mm:ss tt");
            statement = string.Format("[L{0} | {1} {2}]: {3}", lineNumber, filepath, time, statement);
                    
            try
                {
                    new FileInfo(debuglog).Directory.Create();
                    locker.AcquireWriterLock(int.MaxValue); 
                    StreamWriter addToInternalLog = new StreamWriter (debuglog, append: false);
                    addToInternalLog.WriteLine(string.Format("[L{0} | {1} {2}]: Initializing debug log file.", lineNumber, filepath, time));
                    addToInternalLog.WriteLine(statement);
                    addToInternalLog.Close();
                }
                finally
                {
                    locker.ReleaseWriterLock();
                }
                GlobalVariables.mainWindow.WriteGDPrint(statement);
        }
    
        public static void WriteExceptionReport(StringBuilder statement){
            string exceptionname = string.Format("LAST_EXCEPTION_{0}.log", DateTime.Now.ToString());
            string exceptionfile = Path.Combine(GlobalVariables.LogFolder, exceptionname);

            using (StreamWriter streamWriter = new(exceptionfile)){                
                streamWriter.Write(statement);
            }
        }
    }
}