﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:2.0.50727.3603
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// 
// This code was auto-generated by Microsoft.Silverlight.ServiceReference, version 3.0.40624.0
// 
namespace SilverlightFaultsTest.ServiceReference {
    using System.Runtime.Serialization;
    
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "3.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="ArithmeticFault", Namespace="http://schemas.datacontract.org/2004/07/TestService")]
    public partial class ArithmeticFault : object, System.ComponentModel.INotifyPropertyChanged {
        
        private string DescriptionField;
        
        private SilverlightFaultsTest.ServiceReference.Operation OperationField;
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Description {
            get {
                return this.DescriptionField;
            }
            set {
                if ((object.ReferenceEquals(this.DescriptionField, value) != true)) {
                    this.DescriptionField = value;
                    this.RaisePropertyChanged("Description");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public SilverlightFaultsTest.ServiceReference.Operation Operation {
            get {
                return this.OperationField;
            }
            set {
                if ((this.OperationField.Equals(value) != true)) {
                    this.OperationField = value;
                    this.RaisePropertyChanged("Operation");
                }
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "3.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="Operation", Namespace="http://schemas.datacontract.org/2004/07/TestService")]
    public enum Operation : int {
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        Add = 0,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        Subtract = 1,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        Multiply = 2,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        Divide = 3,
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "3.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="ServiceReference.IService")]
    public interface IService {
        
        [System.ServiceModel.OperationContractAttribute(AsyncPattern=true, Action="http://tempuri.org/IService/SayHello", ReplyAction="http://tempuri.org/IService/SayHelloResponse")]
        System.IAsyncResult BeginSayHello(System.AsyncCallback callback, object asyncState);
        
        string EndSayHello(System.IAsyncResult result);
        
        [System.ServiceModel.OperationContractAttribute(AsyncPattern=true, Action="http://tempuri.org/IService/GetFault", ReplyAction="http://tempuri.org/IService/GetFaultResponse")]
        [System.ServiceModel.FaultContractAttribute(typeof(SilverlightFaultsTest.ServiceReference.ArithmeticFault), Action="http://tempuri.org/IService/GetFaultArithmeticFaultFault", Name="ArithmeticFault", Namespace="http://schemas.datacontract.org/2004/07/TestService")]
        System.IAsyncResult BeginGetFault(System.AsyncCallback callback, object asyncState);
        
        string EndGetFault(System.IAsyncResult result);
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "3.0.0.0")]
    public interface IServiceChannel : SilverlightFaultsTest.ServiceReference.IService, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "3.0.0.0")]
    public partial class SayHelloCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        public SayHelloCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        public string Result {
            get {
                base.RaiseExceptionIfNecessary();
                return ((string)(this.results[0]));
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "3.0.0.0")]
    public partial class GetFaultCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        public GetFaultCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        public string Result {
            get {
                base.RaiseExceptionIfNecessary();
                return ((string)(this.results[0]));
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "3.0.0.0")]
    public partial class ServiceClient : System.ServiceModel.ClientBase<SilverlightFaultsTest.ServiceReference.IService>, SilverlightFaultsTest.ServiceReference.IService {
        
        private BeginOperationDelegate onBeginSayHelloDelegate;
        
        private EndOperationDelegate onEndSayHelloDelegate;
        
        private System.Threading.SendOrPostCallback onSayHelloCompletedDelegate;
        
        private BeginOperationDelegate onBeginGetFaultDelegate;
        
        private EndOperationDelegate onEndGetFaultDelegate;
        
        private System.Threading.SendOrPostCallback onGetFaultCompletedDelegate;
        
        private BeginOperationDelegate onBeginOpenDelegate;
        
        private EndOperationDelegate onEndOpenDelegate;
        
        private System.Threading.SendOrPostCallback onOpenCompletedDelegate;
        
        private BeginOperationDelegate onBeginCloseDelegate;
        
        private EndOperationDelegate onEndCloseDelegate;
        
        private System.Threading.SendOrPostCallback onCloseCompletedDelegate;
        
        public ServiceClient() {
        }
        
        public ServiceClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public ServiceClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public ServiceClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public ServiceClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public System.Net.CookieContainer CookieContainer {
            get {
                System.ServiceModel.Channels.IHttpCookieContainerManager httpCookieContainerManager = this.InnerChannel.GetProperty<System.ServiceModel.Channels.IHttpCookieContainerManager>();
                if ((httpCookieContainerManager != null)) {
                    return httpCookieContainerManager.CookieContainer;
                }
                else {
                    return null;
                }
            }
            set {
                System.ServiceModel.Channels.IHttpCookieContainerManager httpCookieContainerManager = this.InnerChannel.GetProperty<System.ServiceModel.Channels.IHttpCookieContainerManager>();
                if ((httpCookieContainerManager != null)) {
                    httpCookieContainerManager.CookieContainer = value;
                }
                else {
                    throw new System.InvalidOperationException("Unable to set the CookieContainer. Please make sure the binding contains an HttpC" +
                            "ookieContainerBindingElement.");
                }
            }
        }
        
        public event System.EventHandler<SayHelloCompletedEventArgs> SayHelloCompleted;
        
        public event System.EventHandler<GetFaultCompletedEventArgs> GetFaultCompleted;
        
        public event System.EventHandler<System.ComponentModel.AsyncCompletedEventArgs> OpenCompleted;
        
        public event System.EventHandler<System.ComponentModel.AsyncCompletedEventArgs> CloseCompleted;
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        System.IAsyncResult SilverlightFaultsTest.ServiceReference.IService.BeginSayHello(System.AsyncCallback callback, object asyncState) {
            return base.Channel.BeginSayHello(callback, asyncState);
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        string SilverlightFaultsTest.ServiceReference.IService.EndSayHello(System.IAsyncResult result) {
            return base.Channel.EndSayHello(result);
        }
        
        private System.IAsyncResult OnBeginSayHello(object[] inValues, System.AsyncCallback callback, object asyncState) {
            return ((SilverlightFaultsTest.ServiceReference.IService)(this)).BeginSayHello(callback, asyncState);
        }
        
        private object[] OnEndSayHello(System.IAsyncResult result) {
            string retVal = ((SilverlightFaultsTest.ServiceReference.IService)(this)).EndSayHello(result);
            return new object[] {
                    retVal};
        }
        
        private void OnSayHelloCompleted(object state) {
            if ((this.SayHelloCompleted != null)) {
                InvokeAsyncCompletedEventArgs e = ((InvokeAsyncCompletedEventArgs)(state));
                this.SayHelloCompleted(this, new SayHelloCompletedEventArgs(e.Results, e.Error, e.Cancelled, e.UserState));
            }
        }
        
        public void SayHelloAsync() {
            this.SayHelloAsync(null);
        }
        
        public void SayHelloAsync(object userState) {
            if ((this.onBeginSayHelloDelegate == null)) {
                this.onBeginSayHelloDelegate = new BeginOperationDelegate(this.OnBeginSayHello);
            }
            if ((this.onEndSayHelloDelegate == null)) {
                this.onEndSayHelloDelegate = new EndOperationDelegate(this.OnEndSayHello);
            }
            if ((this.onSayHelloCompletedDelegate == null)) {
                this.onSayHelloCompletedDelegate = new System.Threading.SendOrPostCallback(this.OnSayHelloCompleted);
            }
            base.InvokeAsync(this.onBeginSayHelloDelegate, null, this.onEndSayHelloDelegate, this.onSayHelloCompletedDelegate, userState);
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        System.IAsyncResult SilverlightFaultsTest.ServiceReference.IService.BeginGetFault(System.AsyncCallback callback, object asyncState) {
            return base.Channel.BeginGetFault(callback, asyncState);
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        string SilverlightFaultsTest.ServiceReference.IService.EndGetFault(System.IAsyncResult result) {
            return base.Channel.EndGetFault(result);
        }
        
        private System.IAsyncResult OnBeginGetFault(object[] inValues, System.AsyncCallback callback, object asyncState) {
            return ((SilverlightFaultsTest.ServiceReference.IService)(this)).BeginGetFault(callback, asyncState);
        }
        
        private object[] OnEndGetFault(System.IAsyncResult result) {
            string retVal = ((SilverlightFaultsTest.ServiceReference.IService)(this)).EndGetFault(result);
            return new object[] {
                    retVal};
        }
        
        private void OnGetFaultCompleted(object state) {
            if ((this.GetFaultCompleted != null)) {
                InvokeAsyncCompletedEventArgs e = ((InvokeAsyncCompletedEventArgs)(state));
                this.GetFaultCompleted(this, new GetFaultCompletedEventArgs(e.Results, e.Error, e.Cancelled, e.UserState));
            }
        }
        
        public void GetFaultAsync() {
            this.GetFaultAsync(null);
        }
        
        public void GetFaultAsync(object userState) {
            if ((this.onBeginGetFaultDelegate == null)) {
                this.onBeginGetFaultDelegate = new BeginOperationDelegate(this.OnBeginGetFault);
            }
            if ((this.onEndGetFaultDelegate == null)) {
                this.onEndGetFaultDelegate = new EndOperationDelegate(this.OnEndGetFault);
            }
            if ((this.onGetFaultCompletedDelegate == null)) {
                this.onGetFaultCompletedDelegate = new System.Threading.SendOrPostCallback(this.OnGetFaultCompleted);
            }
            base.InvokeAsync(this.onBeginGetFaultDelegate, null, this.onEndGetFaultDelegate, this.onGetFaultCompletedDelegate, userState);
        }
        
        private System.IAsyncResult OnBeginOpen(object[] inValues, System.AsyncCallback callback, object asyncState) {
            return ((System.ServiceModel.ICommunicationObject)(this)).BeginOpen(callback, asyncState);
        }
        
        private object[] OnEndOpen(System.IAsyncResult result) {
            ((System.ServiceModel.ICommunicationObject)(this)).EndOpen(result);
            return null;
        }
        
        private void OnOpenCompleted(object state) {
            if ((this.OpenCompleted != null)) {
                InvokeAsyncCompletedEventArgs e = ((InvokeAsyncCompletedEventArgs)(state));
                this.OpenCompleted(this, new System.ComponentModel.AsyncCompletedEventArgs(e.Error, e.Cancelled, e.UserState));
            }
        }
        
        public void OpenAsync() {
            this.OpenAsync(null);
        }
        
        public void OpenAsync(object userState) {
            if ((this.onBeginOpenDelegate == null)) {
                this.onBeginOpenDelegate = new BeginOperationDelegate(this.OnBeginOpen);
            }
            if ((this.onEndOpenDelegate == null)) {
                this.onEndOpenDelegate = new EndOperationDelegate(this.OnEndOpen);
            }
            if ((this.onOpenCompletedDelegate == null)) {
                this.onOpenCompletedDelegate = new System.Threading.SendOrPostCallback(this.OnOpenCompleted);
            }
            base.InvokeAsync(this.onBeginOpenDelegate, null, this.onEndOpenDelegate, this.onOpenCompletedDelegate, userState);
        }
        
        private System.IAsyncResult OnBeginClose(object[] inValues, System.AsyncCallback callback, object asyncState) {
            return ((System.ServiceModel.ICommunicationObject)(this)).BeginClose(callback, asyncState);
        }
        
        private object[] OnEndClose(System.IAsyncResult result) {
            ((System.ServiceModel.ICommunicationObject)(this)).EndClose(result);
            return null;
        }
        
        private void OnCloseCompleted(object state) {
            if ((this.CloseCompleted != null)) {
                InvokeAsyncCompletedEventArgs e = ((InvokeAsyncCompletedEventArgs)(state));
                this.CloseCompleted(this, new System.ComponentModel.AsyncCompletedEventArgs(e.Error, e.Cancelled, e.UserState));
            }
        }
        
        public void CloseAsync() {
            this.CloseAsync(null);
        }
        
        public void CloseAsync(object userState) {
            if ((this.onBeginCloseDelegate == null)) {
                this.onBeginCloseDelegate = new BeginOperationDelegate(this.OnBeginClose);
            }
            if ((this.onEndCloseDelegate == null)) {
                this.onEndCloseDelegate = new EndOperationDelegate(this.OnEndClose);
            }
            if ((this.onCloseCompletedDelegate == null)) {
                this.onCloseCompletedDelegate = new System.Threading.SendOrPostCallback(this.OnCloseCompleted);
            }
            base.InvokeAsync(this.onBeginCloseDelegate, null, this.onEndCloseDelegate, this.onCloseCompletedDelegate, userState);
        }
        
        protected override SilverlightFaultsTest.ServiceReference.IService CreateChannel() {
            return new ServiceClientChannel(this);
        }
        
        private class ServiceClientChannel : ChannelBase<SilverlightFaultsTest.ServiceReference.IService>, SilverlightFaultsTest.ServiceReference.IService {
            
            public ServiceClientChannel(System.ServiceModel.ClientBase<SilverlightFaultsTest.ServiceReference.IService> client) : 
                    base(client) {
            }
            
            public System.IAsyncResult BeginSayHello(System.AsyncCallback callback, object asyncState) {
                object[] _args = new object[0];
                System.IAsyncResult _result = base.BeginInvoke("SayHello", _args, callback, asyncState);
                return _result;
            }
            
            public string EndSayHello(System.IAsyncResult result) {
                object[] _args = new object[0];
                string _result = ((string)(base.EndInvoke("SayHello", _args, result)));
                return _result;
            }
            
            public System.IAsyncResult BeginGetFault(System.AsyncCallback callback, object asyncState) {
                object[] _args = new object[0];
                System.IAsyncResult _result = base.BeginInvoke("GetFault", _args, callback, asyncState);
                return _result;
            }
            
            public string EndGetFault(System.IAsyncResult result) {
                object[] _args = new object[0];
                string _result = ((string)(base.EndInvoke("GetFault", _args, result)));
                return _result;
            }
        }
    }
}