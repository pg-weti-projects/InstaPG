using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstaPGClient
{
    public class User
    {
        public int UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        public int Age { get; set; }
        public string Description { get; set; }

        public User(int userId, string firstName, string lastName, int age, string description, string userName)
        {
            UserId = userId;
            FirstName = firstName;
            LastName = lastName;
            Age = age;
            Description = description;
            UserName = userName;
        }

        public override string ToString()
        {
            return $"User: {FirstName} {LastName}, Age: {Age}, Username: {UserName}, Description: {Description}";
        }
    }
}
