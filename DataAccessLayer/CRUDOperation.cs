using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DALayer
{
    public class CRUDOperation
    {
        public virtual int GetEmployeeCount()
        {   
            SqlConnection sqlCon = new SqlConnection(ConfigurationManager.ConnectionStrings["DBConnectionString"].ConnectionString);
            SqlCommand sqlCom = new SqlCommand("Select Count(*) from tblEmployee", sqlCon);
            sqlCom.CommandType = CommandType.Text;
            sqlCon.Open();
            int result = (int)sqlCom.ExecuteScalar();
            sqlCon.Close();
            return result;
        }
        public virtual EmployeeDetails GetEmployeeDetail(int EmployeeId)
        {
            EmployeeDetails empDet = new EmployeeDetails();
            SqlConnection sqlCon = new SqlConnection(ConfigurationManager.ConnectionStrings["DBConnectionString"].ConnectionString);
            SqlCommand sqlCom = new SqlCommand("Select * from tblEmployee Where EmployeeId=" + EmployeeId, sqlCon);
            sqlCom.CommandType = CommandType.Text;
            sqlCon.Open();
            SqlDataReader reader = sqlCom.ExecuteReader();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    empDet.EmployeeId = reader.GetInt32(0);
                    empDet.EmployeeName = reader.GetString(1);
                    empDet.Gender = reader.GetString(2);
                }
            }
            sqlCon.Close();
            return empDet;
        }

        public virtual List<EmployeeDetails> GetAllEmployeeDetail()
        {
            List<EmployeeDetails> empDet = new List<EmployeeDetails>();
            SqlConnection sqlCon = new SqlConnection(ConfigurationManager.ConnectionStrings["DBConnectionString"].ConnectionString);

            SqlCommand sqlCom = new SqlCommand("Select * from tblEmployee" , sqlCon);
            sqlCom.CommandType = CommandType.Text;
            sqlCon.Open();
            SqlDataReader reader = sqlCom.ExecuteReader();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                 empDet.Add( new EmployeeDetails{ EmployeeId = reader.GetInt32(0),
                                         EmployeeName = reader.GetString(1),
                                         Gender = reader.GetString(2),
                    });
                }
            }
            sqlCon.Close();
            return empDet;
        }
    }
}
