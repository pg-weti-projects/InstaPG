using System;
using System.Collections.Generic;

namespace InstaPGClient
{
    public class InstaPGServiceClient
    {
        private SQLiteHelper sqliteHelper;
        private bool UserLogged = false;
        public Dictionary<string, object> CurrentUserData;

        public InstaPGServiceClient()
        {
            sqliteHelper = new SQLiteHelper();
            CurrentUserData = sqliteHelper.GetUserData(1);
            //this.AddTestUserToDb();
        }

        public void AddTestUserToDb()
        {
            // Definiowanie danych do wstawienia
            Dictionary<string, object> userData = new Dictionary<string, object>();
            userData.Add("name", "Jan");
            userData.Add("surname", "Kowalski");
            userData.Add("age", 30);
            userData.Add("description", "Przykładowy opis użytkownika");
            userData.Add("login", "JKowal");
            userData.Add("pass_hash", "dupa2137");

            // Wstawianie danych do tabeli "Users"
            sqliteHelper.InsertData("Users", userData);

        }


        public void PrintTestUser()
        {
            Dictionary<string, object> xd = sqliteHelper.GetUserData(1);

            foreach (var item in xd)
            {
                Console.WriteLine($"{item.Key}: {item.Value}");
            }
        }
        
        public bool isLogin()
        {
            return UserLogged;
        }

        public void SetUserLogged(bool value)
        {
            this.UserLogged = value;
        }

        public void ClearCurrentUserData()
        {
            this.CurrentUserData = null;
        }

        public String getUserName()
        { 
            return this.CurrentUserData["name"].ToString();
        }

        public String getUserAge()
        {
            return this.CurrentUserData["age"].ToString();
        }

        public String getUserSurname()
        {
            return this.CurrentUserData["surname"].ToString();
        }

        public String getUserDescription()
        {
            return this.CurrentUserData["description"].ToString();
        }
    }
}

