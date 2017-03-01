using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System;
using System.ServiceModel;
using Castle.Facilities.WcfIntegration;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using DALayer;

namespace EmployeeService
{

    //The interface defines the service contract
    //The ServiceContract attribute identifies this interface as a service contract. 
    //To expose a method to clients, you use the OperationContract attribute.
    //
    [ServiceContract]
    public interface IEmployeeService
    {
        [OperationContract]
        int GetEmployeeCount();

        [OperationContract]
        string GetEmployeeName(int EmployeeId);

        [OperationContract]
       EmployeeDetails GetEmployeeDetail(int EmployeeId);

        [OperationContract]
        List<EmployeeDetails> GetAllEmployeeDetail();
    }

    //Clients and services exchange data using XML messages. The WCF runtime uses the Data Contract Serializer to serialize (convert to XML) 
    //and deserialize (convert from XML) data. This serializer has the ability to work with basic .NET types such as strings, integers, DateTime, etc. 
    //However, it does not have the built-in ability to work with classes and other complex types. 
    //To make a class serializable, you can create a data contract by adding the DataContract attribute to the class definition and by adding the DataMember 
    //attribute to each member of the class you want to serialize.

    //[DataContract]
    //public class EmployeeDataContract
    //{
    //    [DataMember]
    //    public int EmployeeId {  get;  set ; }

    //    [DataMember]
    //    public string EmployeeName {get; set;}

    //    [DataMember]
    //    public string Gender { get; set; }

    //    [DataMember]
    //    public DateTime? HireDate { get; set; }
    //}
}
