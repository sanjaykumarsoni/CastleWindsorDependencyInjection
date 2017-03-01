using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using CommonLayer;
using Castle.Facilities.WcfIntegration;
using Castle.MicroKernel.Registration;
using DALayer;


namespace EmployeeService
{
    public class EmployeeService : IEmployeeService
    {

        public EmployeeService()
        {
            var empCRUDOperation = DependencyContainer.GetInstance<IEmployeeManager>();
        }

        public virtual int GetEmployeeCount()
        {
            var empCRUDOperation = DependencyContainer.GetInstance<IEmployeeManager>();
            return empCRUDOperation.GetEmployeeCount();
        }

        public virtual EmployeeDetails GetEmployeeDetail(int EmployeeId)
        {
            var empGetEmployeeDetails = DependencyContainer.GetInstance<IEmployeeManager>();
            EmployeeDetails empData = empGetEmployeeDetails.GetEmployeeDetail(EmployeeId);
            return empData;
        }

        public virtual List<EmployeeDetails> GetAllEmployeeDetail()
        {
            var empGetEmployeeDetails = DependencyContainer.GetInstance<IEmployeeManager>();
            return empGetEmployeeDetails.GetAllEmployeeDetail();
        }

        public virtual string GetEmployeeName(int EmployeeId)
        {
            SqlConnection sqlCon = new SqlConnection(ConfigurationManager.ConnectionStrings["DBConnectionString"].ConnectionString);

            string empName = string.Empty;

            SqlCommand sqlCom = new SqlCommand("Select * from tblEmployee Where EmployeeId=" + EmployeeId, sqlCon);
            sqlCom.CommandType = CommandType.Text;

            sqlCon.Open();
            SqlDataReader reader = sqlCom.ExecuteReader();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    empName = reader.GetString(1);
                }
            }
            sqlCon.Close();
            return empName;
        }
    }
}
