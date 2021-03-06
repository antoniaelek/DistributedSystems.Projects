﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace IoTSensorDataProcessing.Sensor.ServerReference {
    using System.Runtime.Serialization;
    using System;
    
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="UserAddress", Namespace="http://schemas.datacontract.org/2004/07/IoTSensorDataProcessing")]
    [System.SerializableAttribute()]
    public partial class UserAddress : object, System.Runtime.Serialization.IExtensibleDataObject, System.ComponentModel.INotifyPropertyChanged {
        
        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string IpAddressField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private double LatitudeField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private double LongitudeField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private int PortField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string UsernameField;
        
        [global::System.ComponentModel.BrowsableAttribute(false)]
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData {
            get {
                return this.extensionDataField;
            }
            set {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string IpAddress {
            get {
                return this.IpAddressField;
            }
            set {
                if ((object.ReferenceEquals(this.IpAddressField, value) != true)) {
                    this.IpAddressField = value;
                    this.RaisePropertyChanged("IpAddress");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public double Latitude {
            get {
                return this.LatitudeField;
            }
            set {
                if ((this.LatitudeField.Equals(value) != true)) {
                    this.LatitudeField = value;
                    this.RaisePropertyChanged("Latitude");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public double Longitude {
            get {
                return this.LongitudeField;
            }
            set {
                if ((this.LongitudeField.Equals(value) != true)) {
                    this.LongitudeField = value;
                    this.RaisePropertyChanged("Longitude");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public int Port {
            get {
                return this.PortField;
            }
            set {
                if ((this.PortField.Equals(value) != true)) {
                    this.PortField = value;
                    this.RaisePropertyChanged("Port");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Username {
            get {
                return this.UsernameField;
            }
            set {
                if ((object.ReferenceEquals(this.UsernameField, value) != true)) {
                    this.UsernameField = value;
                    this.RaisePropertyChanged("Username");
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
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="ServerReference.IServer")]
    public interface IServer {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IServer/Register", ReplyAction="http://tempuri.org/IServer/RegisterResponse")]
        bool Register(string username, double latitude, double longitude, string ipAddress, int port);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IServer/Register", ReplyAction="http://tempuri.org/IServer/RegisterResponse")]
        System.Threading.Tasks.Task<bool> RegisterAsync(string username, double latitude, double longitude, string ipAddress, int port);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IServer/SearchNeighbour", ReplyAction="http://tempuri.org/IServer/SearchNeighbourResponse")]
        IoTSensorDataProcessing.Sensor.ServerReference.UserAddress SearchNeighbour(string username);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IServer/SearchNeighbour", ReplyAction="http://tempuri.org/IServer/SearchNeighbourResponse")]
        System.Threading.Tasks.Task<IoTSensorDataProcessing.Sensor.ServerReference.UserAddress> SearchNeighbourAsync(string username);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IServer/StoreMeasurement", ReplyAction="http://tempuri.org/IServer/StoreMeasurementResponse")]
        bool StoreMeasurement(string username, string parameter, float averageValue);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IServer/StoreMeasurement", ReplyAction="http://tempuri.org/IServer/StoreMeasurementResponse")]
        System.Threading.Tasks.Task<bool> StoreMeasurementAsync(string username, string parameter, float averageValue);
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IServerChannel : IoTSensorDataProcessing.Sensor.ServerReference.IServer, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class ServerClient : System.ServiceModel.ClientBase<IoTSensorDataProcessing.Sensor.ServerReference.IServer>, IoTSensorDataProcessing.Sensor.ServerReference.IServer {
        
        public ServerClient() {
        }
        
        public ServerClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public ServerClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public ServerClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public ServerClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public bool Register(string username, double latitude, double longitude, string ipAddress, int port) {
            return base.Channel.Register(username, latitude, longitude, ipAddress, port);
        }
        
        public System.Threading.Tasks.Task<bool> RegisterAsync(string username, double latitude, double longitude, string ipAddress, int port) {
            return base.Channel.RegisterAsync(username, latitude, longitude, ipAddress, port);
        }
        
        public IoTSensorDataProcessing.Sensor.ServerReference.UserAddress SearchNeighbour(string username) {
            return base.Channel.SearchNeighbour(username);
        }
        
        public System.Threading.Tasks.Task<IoTSensorDataProcessing.Sensor.ServerReference.UserAddress> SearchNeighbourAsync(string username) {
            return base.Channel.SearchNeighbourAsync(username);
        }
        
        public bool StoreMeasurement(string username, string parameter, float averageValue) {
            return base.Channel.StoreMeasurement(username, parameter, averageValue);
        }
        
        public System.Threading.Tasks.Task<bool> StoreMeasurementAsync(string username, string parameter, float averageValue) {
            return base.Channel.StoreMeasurementAsync(username, parameter, averageValue);
        }
    }
}
