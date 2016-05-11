using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.ServiceProcess;
using System.Text;
using Madaa.Lib.Win.Services.Msdtc;

namespace MSDTCManagerHelper
{
    public class Program
    {
        private static MsdtcManager msdtcManager;

        static void Main(string[] args)
        {
            try
            {
                msdtcManager = new MsdtcManager(false, 1000);
                SetMSDTCStartMode();
                Configurar();
            }
            catch (Exception)
            {
            }
        }

        public static bool MsdtcConfigurado()
        {
            return (msdtcManager.NetworkDtcAccess == NetworkDTCAccessStatus.On && msdtcManager.AllowInbound && msdtcManager.AllowOutbound
                    && msdtcManager.AuthenticationRequired == AuthenticationRequiredType.MutualAuthenticationRequired
                    && !msdtcManager.EnableXaTransactions && msdtcManager.EnableSnaLuTransactions);
        }

        private static void Configurar()
        {
            ServiceControllerStatus serviceStatus = msdtcManager.GetServiceStatus();

            msdtcManager.NetworkDtcAccess = NetworkDTCAccessStatus.On;
            msdtcManager.AllowInbound = true;
            msdtcManager.AllowOutbound = true;

            msdtcManager.AuthenticationRequired = AuthenticationRequiredType.MutualAuthenticationRequired;

            if (serviceStatus == ServiceControllerStatus.Running && msdtcManager.NeedRestart)
            {
                msdtcManager.RestartService();
            }
            else
            {
                StartarOuContinuar(serviceStatus);
            }

            msdtcManager.EnableXaTransactions = false;
            msdtcManager.EnableSnaLuTransactions = true;
        }

        private static void StartarOuContinuar(ServiceControllerStatus serviceStatus)
        {
            switch (serviceStatus)
            {
                case ServiceControllerStatus.Stopped:
                    {
                        msdtcManager.StartService();
                        break;
                    }
                case ServiceControllerStatus.Paused:
                    {
                        msdtcManager.ContinueService();
                        break;
                    }
                default: break;
            }
        }

        private static void SetMSDTCStartMode()
        {
            //if (value != "Automatic" && value != "Manual" && value != "Disabled")
            //    throw new Exception("The valid values are Automatic, Manual or Disabled");

            //construct the management path
            string path = "Win32_Service.Name='MSDTC'";
            ManagementPath p = new ManagementPath(path);
            //construct the management object
            ManagementObject ManagementObj = new ManagementObject(p);
            //we will use the invokeMethod method of the ManagementObject class
            object[] parameters = new object[1];
            parameters[0] = "Automatic";
            ManagementObj.InvokeMethod("ChangeStartMode", parameters);
        }

        private static string GetMSDTCStartMode()
        {
            //construct the management path
            string path = "Win32_Service.Name='MSDTC'";
            ManagementPath p = new ManagementPath(path);
            //construct the management object
            ManagementObject ManagementObj = new ManagementObject(p);
            return ManagementObj["StartMode"].ToString();
        }
    }
}
