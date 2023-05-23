/*
 * ATM ISO8583 Service
 * Service to receive and response to ISO request from ATM Switch
 * Bikram Gurung
 * 2022-09-13
 * */

using SimpleTCP;
using System;
using System.Configuration;
using System.ServiceProcess;


namespace ATMService
{
    public partial class Service1 : ServiceBase
    {
        Services.ISOHandler isoHandler = new Services.ISOHandler();
        SimpleTcpServer server;
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            if (!isoHandler.CheckStartup())
            {
                throw new Exception("UDL not configured.");
                //Stop Service.            
            }
            string serviceIP = isoHandler.GetServiceIp();
            server = new SimpleTcpServer();
            server.DataReceived += Server_DataReceived;
            int servicePort = 0;
            try
            {
                servicePort = int.Parse(ConfigurationManager.AppSettings["ServicePort"]);
            }
            catch (Exception ex)
            {
                isoHandler.Log("Service port is not properly configured.Eror Messsage=" + ex.Message);
                throw new Exception("Service port is not properly configured.Eror Messsage=" + ex.Message);
            }

            try
            {
                System.Net.IPAddress ip = System.Net.IPAddress.Parse(serviceIP);
                server.Start(ip, servicePort);
            }
            catch (Exception e)
            {
                isoHandler.Log("Failed to start service:" + e.Message);
                throw new Exception("Failed to start service:" + e.Message);
            }
            isoHandler.Log("Service started at " + serviceIP + ":" + servicePort.ToString());
        }

        private void Server_DataReceived(object sender, SimpleTCP.Message e)
        {
            string responseMessage = isoHandler.ProcessRequest(e.Data);
            byte[] myByte = System.Text.ASCIIEncoding.Default.GetBytes(responseMessage);
            e.TcpClient.Client.Send(myByte);
        }

        protected override void OnStop()
        {
            isoHandler.Log("Stopping Service.");
            server.Stop();
        }

    }
}
