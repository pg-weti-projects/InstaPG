using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace InstaPGService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in both code and config file together.
    public class InstaPGService : IInstaPGService
    {
        private static List<string> activeUsers = new List<string>();

        public string GetData(int value)
        {
            return string.Format("You entered: {0}", value);
        }

        public void Login(string username)
        {
            if (!activeUsers.Contains(username))
            {
                activeUsers.Add(username);
            }
        }

        public void Logout(string username)
        {
            if (activeUsers.Contains(username))
            {
                activeUsers.Remove(username);
            }
        }

        public List<string> GetActiveUsers()
        {
            return activeUsers;
        }
    }
}
