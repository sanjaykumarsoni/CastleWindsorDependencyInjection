using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.Windsor;
using Castle.DynamicProxy;

namespace EmployeeService
{
   public class TestInterceptor : IInterceptor
    {
        public void Intercept(IInvocation invocation)
        {
            try
            {
                invocation.Proceed();
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }
    }
}
