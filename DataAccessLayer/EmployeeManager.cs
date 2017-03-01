using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace DALayer
{
    public class EmployeeManager: IEmployeeManager
    {
        public virtual int GetEmployeeCount()
        {
            CRUDOperation crud = new CRUDOperation();
            return crud.GetEmployeeCount();
        }

        public virtual EmployeeDetails GetEmployeeDetail(int EmployeeId)
        {
           CRUDOperation crud1=new CRUDOperation();
           return crud1.GetEmployeeDetail(EmployeeId);
        }

        public virtual List<EmployeeDetails> GetAllEmployeeDetail()
        {
            CRUDOperation crud1 = new CRUDOperation();
            return crud1.GetAllEmployeeDetail();
        }
                 
     }
}
