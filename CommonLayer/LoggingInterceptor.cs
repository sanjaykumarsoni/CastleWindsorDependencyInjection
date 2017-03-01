using System;
using System.Diagnostics;
using System.Reflection;
using Castle.DynamicProxy;

namespace CommonLayer
{
    public class LoggingInterceptor : IInterceptor
    {
        #region IInterceptor Members

        public void Intercept(IInvocation invocation)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            //TimeSpan timeStart = LoggerManager.StopWatch.Elapsed;
            string parameters = String.Empty;
            ParameterInfo[] parameterInfos = invocation.Method.GetParameters();
            foreach (var parameterInfo in parameterInfos)
            {
                parameters += parameterInfo.ParameterType.Name + " " + parameterInfo.Name + ",";
            }
            string methodName = string.Format("{0}.{1}({2})", invocation.TargetType.FullName, invocation.Method.Name,
                                              parameters.TrimEnd(','));
            string entry = string.Format("Method Entry:: {0} ", methodName);
            DefaultLogContext.LogContext.Log(entry, LogPolicy.Information);
            try
            {
                invocation.Proceed();
            }
            
            catch (System.ServiceModel.FaultException exception)
            {
                var exceptionMessage = string.Format("Method Exception:: {0} [{1} {2}]", methodName, exception.Message, exception.StackTrace);
                DefaultLogContext.LogContext.Log(exceptionMessage, LogPolicy.Error);
                throw;
            }
            catch (Exception exception)
            {
                var exceptionMessage = string.Format("Method Exception:: {0} [{1} {2}]", methodName, exception.Message, exception.StackTrace);
                DefaultLogContext.LogContext.Log(exceptionMessage, LogPolicy.Error);
                throw;
            }
            finally
            {
                stopWatch.Stop();
                var exit = string.Format("Method Exit :: {0} ({1} Milli Seconds)", methodName,stopWatch.ElapsedMilliseconds);
                DefaultLogContext.LogContext.Log(exit, LogPolicy.Information);
            }
        }

        #endregion
    }
}