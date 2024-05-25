using System.Collections.Generic;
using System.Linq;

namespace ActiveUsersServiceLibrary
{
    public class ActiveUsersService : IActiveUsersService
    {
        private static readonly List<string> activeUsers = new List<string>();

        public List<string> GetActiveUsers()
        {
            return activeUsers.ToList();
        }

        public void AddActiveUser(string username)
        {
            if (!activeUsers.Contains(username))
            {
                activeUsers.Add(username);
            }
        }

        public void RemoveActiveUser(string username)
        {
            if (activeUsers.Contains(username))
            {
                activeUsers.Remove(username);
            }
        }
    }
}