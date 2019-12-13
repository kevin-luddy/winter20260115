using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Transactions;
using GenTRAC.Models;
using IES.Common;
using Oracle.ManagedDataAccess.Client;

namespace GenTRAC.LoadGeeps
{
    class Program
    {
        private static Logger Logger = new Logger(typeof(Program));

        static void Main(string[] args)
        {
            ICollection<DataMartEmployeeDTO> employees = LoadEmployees();
            // CompareToGenTRAC(employees);
            SaveEmployees(employees);
            Console.WriteLine("Done.");
        }

        private static void CompareToGenTRAC(ICollection<DataMartEmployeeDTO> employees)
        {
            ICollection<DataMartEmployee> dataMartEmployees;
            using (genTRACEntities dbModel = new genTRACEntities())
            {
                dataMartEmployees = dbModel.DataMartEmployees.Where(e => e.nt_account_nm != "").ToList();
            }

            Logger.Info("Geeps: Loaded " + dataMartEmployees.Count.ToString() + " employees from genTRAC.");

            Dictionary<string, DataMartEmployeeDTO> emplDict = employees.ToDictionary(e => e.nt_account_nm.ToLower());
            Dictionary<string, DataMartEmployee> genTracDict = dataMartEmployees.ToDictionary(e => e.nt_account_nm.ToLower());
            int numMissing = 0;
            int numAdded = 0;

            foreach (DataMartEmployeeDTO emp in employees)
            {
                if (!genTracDict.ContainsKey(emp.nt_account_nm))
                {
                    numAdded++;
                }
            }

            foreach (DataMartEmployee emp in dataMartEmployees)
            {
                if (!emplDict.ContainsKey(emp.nt_account_nm))
                {
                    numMissing++;
                }
            }

            Logger.Info("Geeps: Number of missing entries is " + numMissing.ToString());
            Logger.Info("Geeps: Number of added entries is " + numAdded.ToString());
        }

        private static void SaveEmployees(ICollection<DataMartEmployeeDTO> employees)
        {
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, Convert.ToInt32(ConfigurationManager.AppSettings["TransactionTimeout"])) }))
            {
                DataMartEmployeeDTOLoader loader = new DataMartEmployeeDTOLoader();
                loader.BulkSave(employees);
                scope.Complete();
            }
        }

        private static ICollection<DataMartEmployeeDTO> LoadEmployees()
        {
            List<DataMartEmployeeDTO> employees = new List<DataMartEmployeeDTO>();
            using(OracleConnection conn = new OracleConnection("Data Source=DWLMP;User Id=GEEPS_PTM;Password=PLS_trythis1;"))
            {
                conn.Open();

                OracleCommand cmd = new OracleCommand("SELECT EMPLID, FIRST_NAME, LAST_NAME, LOWER(ROLEUSER), MGR_SUPV_ID_LM FROM GEEPS.geeps_nonsensitive WHERE empl_status='A'", conn);
                
                using(OracleDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader.IsDBNull(0) || reader.IsDBNull(1) || reader.IsDBNull(2) || reader.IsDBNull(3) || reader.IsDBNull(4))
                        {
                            // somehow the row is null, kick it out
                            Logger.Error("Geeps: Return value is null from geeps.");
                        }
                        else
                        {
                            string serNo = reader.GetString(0);
                            string first_nm = reader.GetString(1);
                            string last_nm = reader.GetString(2);
                            string domainNtId = reader.IsDBNull(3) ? string.Empty : reader.GetString(3);
                            string reportSerNo = reader.GetString(4);
                            string[] creds = domainNtId.Split('\\');
                            if (string.IsNullOrWhiteSpace(domainNtId))
                            {
                                // recreate ntid from employee id, this works for 17/18 people that are missing roleuser in geeps
                                creds = new string[] { string.Empty, "e" + serNo };
                            }


                            if (creds.Length != 2)
                            {
                                // bad nt id credentials
                                Logger.Error("Geeps: Bad user Role credentials in Oracle.");
                            }
                            else
                            {
                                employees.Add(new DataMartEmployeeDTO
                                {
                                    empl_ser_no = serNo,
                                    empl_first_nm = first_nm,
                                    empl_last_nm = last_nm,
                                    nt_domain_nm = creds[0],
                                    nt_account_nm = creds[1],
                                    rpt_to_ser_no = reportSerNo
                                });
                            }
                        }
                    }
                }
            }

            Logger.Info("Geeps: Loaded " + employees.Count.ToString() + " employees from Geeps.");
            return employees;
        }
    }
}
