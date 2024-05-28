using System.Collections.Generic;
using System.ServiceModel;

namespace ActiveUsersServiceLibrary
{
    [ServiceContract]
    public interface IActiveUsersService
    {
        [OperationContract]
        List<string> GetActiveUsers();

        [OperationContract]
        void AddActiveUser(string username);

        [OperationContract]
        void RemoveActiveUser(string username);
    }
}