﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace InstaPGService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IService1" in both code and config file together.
    [ServiceContract]
    public interface IInstaPGService
    {
        [OperationContract]
        string GetData(int value);

        [OperationContract]
        void Login(string username);

        [OperationContract]
        void Logout(string username);

        [OperationContract]
        List<string> GetActiveUsers();
    }

}
