using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProcessMining.DataClasses;

namespace ProcessMining.Helpers
{
    public  class ExcelDataProvider
    {
        private const string _tblLogs = "Logs";
        private const string _tblProcesses = "Processes";
        private const string _tblEvents = "Events";
        private const string _tblEmployees = "Employees";

        public  List<Process> ProcessList { get; set; }
        public  List<Event> EventList { get; set; }
        public  List<Employee> EmployeeList { get; set; }
        public  List<Log> LogList { get; set; }

        public  void Init(string filePath)
        {
            ProcessList = MapTableToEntities<Process>(ReadTable(filePath, _tblProcesses));
            EventList = MapTableToEntities<Event>(ReadTable(filePath, _tblEvents));
            EmployeeList = MapTableToEntities<Employee>(ReadTable(filePath, _tblEmployees));
            LogList = MapTableToEntities<Log>(ReadTable(filePath, _tblLogs));
        }



        private  List<T> MapTableToEntities<T> (DataTable table)  where T:class, new()
        {
            var properties = typeof(T).GetProperties();
            var retVal = new List<T>();
            foreach (DataRow datarow in table.Rows)
            {
                var obj = new T();
                foreach (var property in properties)
                {
                    try
                    {
                        if (property == typeof(System.DateTime)) property.SetValue(obj, Convert.ToDateTime(datarow[property.Name]));
                        else property.SetValue(obj, datarow[property.Name].GetType() == typeof(System.Double) ? Convert.ToInt32(datarow[property.Name]) : datarow[property.Name]);
                    }
                    catch (ArgumentException)
                    {
                        if (property.Name == "Occurence") continue;
                        var idField = property.Name + "Id";
                        var listName = property.Name + "List";
                        var type = property.PropertyType;
                        var list = typeof(ExcelDataProvider).GetProperty(listName).GetValue(this);
                        switch (property.Name)
                        {
                            case "Process":

                                property.SetValue(obj, ProcessList.Where(t => t.Id == Convert.ToInt32(datarow[idField])).First());
                                break;
                            case "Employee":

                                property.SetValue(obj, EmployeeList.Where(t => t.Id == Convert.ToInt32(datarow[idField])).First());
                                break;
                            case "Event":

                                property.SetValue(obj, EventList.Where(t => t.Id == Convert.ToInt32(datarow[idField])).First());
                                break;
                        }
                    }
                }
                retVal.Add(obj);
            }
            return retVal;
        }

        

        private  DataTable ReadTable(string filePath, string sheetName)
        {
            DataTable dt = new DataTable();
            string connectionString = "Provider=Microsoft.Jet.OleDb.4.0; Data Source=" + filePath + "; Extended Properties=Excel 8.0;";
            using (OleDbConnection Connection = new OleDbConnection(connectionString))
            {
                Connection.Open();
                using (OleDbCommand command = new OleDbCommand())
                {
                    command.Connection = Connection;
                    command.CommandText = "SELECT * FROM [" + sheetName + "$]";
                    using (OleDbDataAdapter adapter = new OleDbDataAdapter())
                    {
                        adapter.SelectCommand = command;
                        adapter.Fill(dt);
                    }
                }
            }
            return dt;
        }
    }
}
