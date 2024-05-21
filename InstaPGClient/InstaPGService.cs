﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Ten kod został wygenerowany przez narzędzie.
//     Wersja wykonawcza:4.0.30319.42000
//
//     Zmiany w tym pliku mogą spowodować nieprawidłowe zachowanie i zostaną utracone, jeśli
//     kod zostanie ponownie wygenerowany.
// </auto-generated>
//------------------------------------------------------------------------------



using System.Collections.Generic;
using System;
using System.Runtime.Remoting.Messaging;

[System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
[System.ServiceModel.ServiceContractAttribute(ConfigurationName="IInstaPGService")]
public interface IInstaPGService
{
    
    [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IInstaPGService/GetData", ReplyAction="http://tempuri.org/IInstaPGService/GetDataResponse")]
    string GetData(int value);
    
    [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IInstaPGService/GetData", ReplyAction="http://tempuri.org/IInstaPGService/GetDataResponse")]
    System.Threading.Tasks.Task<string> GetDataAsync(int value);
}

[System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
public interface IInstaPGServiceChannel : IInstaPGService, System.ServiceModel.IClientChannel
{
}

[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
public partial class InstaPGServiceClient : System.ServiceModel.ClientBase<IInstaPGService>, IInstaPGService
{
    private SQLiteHelper sqliteHelper;
    private bool UserLogged = false;
    public Dictionary<string, object> CurrentUserData;

    public InstaPGServiceClient()
    {
        sqliteHelper = new SQLiteHelper();
        this.AddTestUserToDb();
        CurrentUserData = sqliteHelper.GetUserData(1);
    }

    public InstaPGServiceClient(string endpointConfigurationName) : 
            base(endpointConfigurationName)
    {
    }
    
    public InstaPGServiceClient(string endpointConfigurationName, string remoteAddress) : 
            base(endpointConfigurationName, remoteAddress)
    {
    }
    
    public InstaPGServiceClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
            base(endpointConfigurationName, remoteAddress)
    {
    }
    
    public InstaPGServiceClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
            base(binding, remoteAddress)
    {
    }
    
    public string GetData(int value)
    {
        return base.Channel.GetData(value);
    }

    public void AddTestUserToDb()
    {
        // Definiowanie danych do wstawienia
        Dictionary<string, object> userData = new Dictionary<string, object>();
        userData.Add("imie", "Jan");
        userData.Add("nazwisko", "Kowalski");
        userData.Add("wiek", 30);
        userData.Add("opis", "Przykładowy opis użytkownika");
        userData.Add("pseudonim", "JKowal");
        userData.Add("hash_hasla", "dupa2137");

        // Wstawianie danych do tabeli "Uzytkownicy"
        sqliteHelper.InsertData("Uzytkownicy", userData);

    }


    public void PrintTestUser()
    {
        Dictionary<string, object> xd = sqliteHelper.GetUserData(1);

        foreach (var item in xd)
        {
            Console.WriteLine($"{item.Key}: {item.Value}");
        }
    }
    
    public System.Threading.Tasks.Task<string> GetDataAsync(int value)
    {
        return base.Channel.GetDataAsync(value);
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
        return this.CurrentUserData["imie"].ToString();
    }

    public String getUserAge()
    {
        return this.CurrentUserData["wiek"].ToString();
    }

    public String getUserSurname()
    {
        return this.CurrentUserData["nazwisko"].ToString();
    }

    public String getUserDescription()
    {
        return this.CurrentUserData["opis"].ToString();
    }
}
