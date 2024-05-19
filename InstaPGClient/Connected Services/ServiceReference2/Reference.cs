﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace InstaPGClient.ServiceReference2 {
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="ServiceReference2.IInstaPGService")]
    public interface IInstaPGService {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IInstaPGService/GetData", ReplyAction="http://tempuri.org/IInstaPGService/GetDataResponse")]
        string GetData(int value);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IInstaPGService/GetData", ReplyAction="http://tempuri.org/IInstaPGService/GetDataResponse")]
        System.Threading.Tasks.Task<string> GetDataAsync(int value);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IInstaPGService/Login", ReplyAction="http://tempuri.org/IInstaPGService/LoginResponse")]
        void Login(string username);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IInstaPGService/Login", ReplyAction="http://tempuri.org/IInstaPGService/LoginResponse")]
        System.Threading.Tasks.Task LoginAsync(string username);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IInstaPGService/Logout", ReplyAction="http://tempuri.org/IInstaPGService/LogoutResponse")]
        void Logout(string username);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IInstaPGService/Logout", ReplyAction="http://tempuri.org/IInstaPGService/LogoutResponse")]
        System.Threading.Tasks.Task LogoutAsync(string username);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IInstaPGService/GetActiveUsers", ReplyAction="http://tempuri.org/IInstaPGService/GetActiveUsersResponse")]
        string[] GetActiveUsers();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IInstaPGService/GetActiveUsers", ReplyAction="http://tempuri.org/IInstaPGService/GetActiveUsersResponse")]
        System.Threading.Tasks.Task<string[]> GetActiveUsersAsync();
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IInstaPGServiceChannel : InstaPGClient.ServiceReference2.IInstaPGService, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class InstaPGServiceClient : System.ServiceModel.ClientBase<InstaPGClient.ServiceReference2.IInstaPGService>, InstaPGClient.ServiceReference2.IInstaPGService {
        
        public InstaPGServiceClient() {
        }
        
        public InstaPGServiceClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public InstaPGServiceClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public InstaPGServiceClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public InstaPGServiceClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public string GetData(int value) {
            return base.Channel.GetData(value);
        }
        
        public System.Threading.Tasks.Task<string> GetDataAsync(int value) {
            return base.Channel.GetDataAsync(value);
        }
        
        public void Login(string username) {
            base.Channel.Login(username);
        }
        
        public System.Threading.Tasks.Task LoginAsync(string username) {
            return base.Channel.LoginAsync(username);
        }
        
        public void Logout(string username) {
            base.Channel.Logout(username);
        }
        
        public System.Threading.Tasks.Task LogoutAsync(string username) {
            return base.Channel.LogoutAsync(username);
        }
        
        public string[] GetActiveUsers() {
            return base.Channel.GetActiveUsers();
        }
        
        public System.Threading.Tasks.Task<string[]> GetActiveUsersAsync() {
            return base.Channel.GetActiveUsersAsync();
        }
    }
}
