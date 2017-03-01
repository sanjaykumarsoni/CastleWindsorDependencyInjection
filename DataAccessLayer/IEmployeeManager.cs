﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DALayer
{
   public interface IEmployeeManager
    {
        int GetEmployeeCount();
        EmployeeDetails GetEmployeeDetail(int EmployeeId);
        List<EmployeeDetails> GetAllEmployeeDetail();
    }
}