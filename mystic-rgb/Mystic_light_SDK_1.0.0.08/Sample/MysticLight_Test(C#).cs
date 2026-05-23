using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MysticLight_Sample_WPF_
{
    public enum MLAPI_Status : int
    {
        MLAPI_OK = 0,                   //!< Request is completed.
        MLAPI_ERROR = -1,               //!< Generic error.
        MLAPI_TIMEOUT = -2,             //!< Function is timeout.
        MLAPI_NO_IMPLEMENTED = -3,		//!< MSI Application not found or installed version not supported.
        MLAPI_NOT_INITIALIZED = -4,		//!< MLAPI_Initialize has not been called successful.	
        MLAPI_INVALID_ARGUMENT = -101,  //!< The parameter value is not valid.
        MLAPI_DEVICE_NOT_FOUND = -102,  //!< The device is not found.
        MLAPI_NOT_SUPPORTED = -103		//!< Requested feature is not supported in the selected LED.
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        [DllImport("MysticLight_SDK.dll", SetLastError = true, CallingConvention = CallingConvention.Cdecl, EntryPoint = "MLAPI_Initialize")]
        extern static MLAPI_Status MLAPI_Initialize();
        [DllImport("MysticLight_SDK.dll", SetLastError = true, CallingConvention = CallingConvention.Cdecl, EntryPoint = "MLAPI_Release")]
        extern static MLAPI_Status MLAPI_Release();
        [DllImport("MysticLight_SDK.dll", SetLastError = true, CallingConvention = CallingConvention.Cdecl, EntryPoint = "MLAPI_GetErrorMessage")]
        extern static MLAPI_Status MLAPI_GetErrorMessage(int ErrorCode, [MarshalAs(UnmanagedType.BStr)] out string Desc);
        [DllImport("MysticLight_SDK.dll", SetLastError = true, CallingConvention = CallingConvention.Cdecl, EntryPoint = "MLAPI_GetDeviceInfo")]
        extern static MLAPI_Status MLAPI_GetDeviceInfo([MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_BSTR)] out string[] pDevType, [MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_BSTR)] out string[] pLedCount);
        [DllImport("MysticLight_SDK.dll", SetLastError = true, CallingConvention = CallingConvention.Cdecl, EntryPoint = "MLAPI_GetLedInfo")]
        extern static MLAPI_Status MLAPI_GetLedInfo([MarshalAs(UnmanagedType.BStr)] string type, int index, [MarshalAs(UnmanagedType.BStr)] out string pName, [MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_BSTR)] out string[] pLedStyles);
        [DllImport("MysticLight_SDK.dll", SetLastError = true, CallingConvention = CallingConvention.Cdecl, EntryPoint = "MLAPI_GetLedColor")]
        extern static MLAPI_Status MLAPI_GetLedColor([MarshalAs(UnmanagedType.BStr)] string type, int index, out int R, out int G, out int B);
        [DllImport("MysticLight_SDK.dll", SetLastError = true, CallingConvention = CallingConvention.Cdecl, EntryPoint = "MLAPI_GetLedStyle")]
        extern static MLAPI_Status MLAPI_GetLedStyle([MarshalAs(UnmanagedType.BStr)] string type, int index, [MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_BSTR)] out string[] style);
        [DllImport("MysticLight_SDK.dll", SetLastError = true, CallingConvention = CallingConvention.Cdecl, EntryPoint = "MLAPI_SetLedColor")]
        extern static MLAPI_Status MLAPI_SetLedColor([MarshalAs(UnmanagedType.BStr)] string type, int index, int R, int G, int B);
        [DllImport("MysticLight_SDK.dll", SetLastError = true, CallingConvention = CallingConvention.Cdecl, EntryPoint = "MLAPI_SetLedStyle")]
        extern static MLAPI_Status MLAPI_SetLedStyle([MarshalAs(UnmanagedType.BStr)] string type, int index, [MarshalAs(UnmanagedType.BStr)] string style);
        [DllImport("MysticLight_SDK.dll", SetLastError = true, CallingConvention = CallingConvention.Cdecl, EntryPoint = "MLAPI_GetDeviceName")]
        extern static MLAPI_Status MLAPI_GetDeviceName([MarshalAs(UnmanagedType.BStr)] string type, [MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_BSTR)] out string[] DevName);
        [DllImport("MysticLight_SDK.dll", SetLastError = true, CallingConvention = CallingConvention.Cdecl, EntryPoint = "MLAPI_GetDeviceNameEx")]
        extern static MLAPI_Status MLAPI_GetDeviceNameEx([MarshalAs(UnmanagedType.BStr)] string type, int index, [MarshalAs(UnmanagedType.BStr)] out string DevName);
        [DllImport("MysticLight_SDK.dll", SetLastError = true, CallingConvention = CallingConvention.Cdecl, EntryPoint = "MLAPI_GetLedName")]
        extern static MLAPI_Status MLAPI_GetLedName([MarshalAs(UnmanagedType.BStr)] string type, [MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_BSTR)] out string[] LedName);
        [DllImport("MysticLight_SDK.dll", SetLastError = true, CallingConvention = CallingConvention.Cdecl, EntryPoint = "MLAPI_SetLedColors")]
        extern static MLAPI_Status MLAPI_SetLedColors([MarshalAs(UnmanagedType.BStr)] string type, int AreaIndex, [MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_BSTR)] ref string[] LedName, int[] R, int[] G, int[] B);
        [DllImport("MysticLight_SDK.dll", SetLastError = true, CallingConvention = CallingConvention.Cdecl, EntryPoint = "MLAPI_SetLedColorEx")]
        extern static MLAPI_Status MLAPI_SetLedColorEx([MarshalAs(UnmanagedType.BStr)] string type, int AreaIndex, [MarshalAs(UnmanagedType.BStr)] string LedName, int R, int G, int B, int Update);
        [DllImport("MysticLight_SDK.dll", SetLastError = true, CallingConvention = CallingConvention.Cdecl, EntryPoint = "MLAPI_SetLedColorSync")]
        extern static MLAPI_Status MLAPI_SetLedColorSync([MarshalAs(UnmanagedType.BStr)] string type, int AreaIndex, [MarshalAs(UnmanagedType.BStr)] string LedName, int R, int G, int B, int Update);
        [DllImport("MysticLight_SDK.dll", SetLastError = true, CallingConvention = CallingConvention.Cdecl, EntryPoint = "MLAPI_MysticLightControlNotify")]
        extern static MLAPI_Status MLAPI_MysticLightControlNotify(CallbackDelegate funcPointer);
        [DllImport("MysticLight_SDK.dll", SetLastError = true, CallingConvention = CallingConvention.Cdecl, EntryPoint = "MLAPI_SetLedColorsSync")]
        extern static MLAPI_Status MLAPI_SetLedColorsSync([MarshalAs(UnmanagedType.BStr)] string type, [MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_BSTR)] ref string[] LedName, int[] R, int[] G, int[] B);

        public delegate void CallbackDelegate();

        public Thread thr = null;
        public DateTime dt, dt2;
        public int frame = 0;

        struct LED_Info
        {
            public int AreaIndex;
            public string FullName;
        }

        public void callback()
        {
            //This is the function which will be called by the DLL

            MessageBox.Show("Called from the DLL..");
        }

        string[] devList, LedCount, LedNames, devName, LedName;

        public MainWindow()
        {
            InitializeComponent();

            MLAPI_Status ret;
            string StatusDesc = "";
            ret = MLAPI_Initialize();
            if (ret == MLAPI_Status.MLAPI_OK)
            {
                // Register Mystic Light Control Notify event
                CallbackDelegate CallbackDelegateInstance = new CallbackDelegate(callback);
                MLAPI_MysticLightControlNotify(CallbackDelegateInstance);

                ret = MLAPI_GetDeviceInfo(out devList, out LedCount);
                if (ret == MLAPI_Status.MLAPI_OK)
                {
                    List<LED_Info> list_LED_Info = new List<LED_Info>();

                    for (int i = 0; i < devList.Length; i++)
                    {
                        ret = MLAPI_GetDeviceName(devList[i], out devName);
                        for (int d = 0; d < devName.Length; d++)
                        {
                            Console.WriteLine("{0}[{1}]:{2}", devList[i], d, devName[d]);
                            textBox_Result.AppendText(string.Format("{0}[{1}]:{2}\n", devList[i], d, devName[d]));
                        }

                        string sName;
                        ret = MLAPI_GetDeviceNameEx(devList[i], 0, out sName);

                        ret = MLAPI_GetLedName(devList[i], out LedName);
                        if (ret == MLAPI_Status.MLAPI_OK)
                        {
                            LED_Info info = new LED_Info();

                            Console.WriteLine("list of LED Name:\nEx. Device Name[Area Index]:LED Name");
                            //textBox_Result.AppendText("list of LED Name:\nEx. Device Name[Area Index]:LED Name\n");

                            for (int L = 0; L < LedName.Length; L++)
                            {
                                string[] words = LedName[L].Split(':');

                                Console.WriteLine("{0}[{1}]:{2}", devList[i], words[0], words[1]);
                                //textBox_Result.AppendText(string.Format("{0}[{1}]:{2}\n", devList[i], words[0], words[1]));

                                info.AreaIndex = Convert.ToInt32(words[0]);
                                info.FullName = words[1];
                                list_LED_Info.Add(info);
                            }
                        }
                    }
                }
                else
                {
                    ret = MLAPI_GetErrorMessage(Convert.ToInt32(ret), out StatusDesc);
                    textBox_Result.AppendText(string.Format("Set style: {0}\n", StatusDesc));
                }
            }
            else
            {
                ret = MLAPI_GetErrorMessage(Convert.ToInt32(ret), out StatusDesc);
                Console.WriteLine("Set style: {0}\n", StatusDesc);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MLAPI_Release();
        }

        private void btn_Test_Click(object sender, RoutedEventArgs e)
        {
            if (thr == null)
            {
                thr = new Thread(ColorShift);
                
                string outstr;
                string[] LedInfo;
                MLAPI_GetLedInfo(devList[0], 0, out outstr, out LedInfo);
                MLAPI_GetLedName("MSI_MB", out LedNames);
                MLAPI_SetLedStyle(devList[0], 0, "Direct All Sync");
            }

            if (thr.IsAlive)
            {
                btn_Test.Content = "Color Shift";

                thr.Abort();
                thr = null;               

                dt2 = DateTime.Now;
                TimeSpan ts = dt2 - dt;
                textBox_Result.AppendText(string.Format("Stop Time : [{0}/{1}/{2}] {3}:{4}:{5}.{6}\n", dt2.Month.ToString(), dt2.Day.ToString(), dt2.Year.ToString(), dt2.Hour.ToString("00"), dt2.Minute.ToString("00"), dt2.Second.ToString("00"), dt2.Millisecond.ToString("000")));
                textBox_Result.AppendText(string.Format("Frame (setting counts) : {0}\n", frame.ToString()));
                textBox_Result.AppendText(string.Format("Interval (total seconds) : {0}\n", ts.TotalSeconds.ToString("0.00")));                
                textBox_Result.AppendText(string.Format("FPS (frame per second) : {0}\n", (frame / ts.TotalSeconds).ToString("0.00")));

                Thread.Sleep(100);

                SetLedColorSync_AllMB(LedNames, 0, 0, 0);                
            }
            else
            {
                textBox_Result.Clear();
                btn_Test.Content = "Stop";
                frame = 0;

                dt = DateTime.Now;
                textBox_Result.AppendText(string.Format("Start Time : [{0}/{1}/{2}] {3}:{4}:{5}.{6}\n", dt.Month.ToString(), dt.Day.ToString(), dt.Year.ToString(), dt.Hour.ToString("00"), dt.Minute.ToString("00"), dt.Second.ToString("00"), dt.Millisecond.ToString("000")));
                thr.Start();                
            }
        }

        private void ColorShift()
        {
            byte R = 255;
            byte G = 0;
            byte B = 0;
            int step = 20;

            while (true)
            {
                // G up
                for (int i = 0; i <= step; i++)
                {
                    byte ii = (byte)((250 / step) * i); //G 0~255 brightness
                    G = ii;
                    SetLedColorSync_AllMB(LedNames, R, G, B);
                    frame++;
                }
                //R down
                for (int i = step; i >= 0; i--)
                {
                    byte ii = (byte)((250 / step) * i); //R 0~255 brightness
                    R = ii;
                    SetLedColorSync_AllMB(LedNames, R, G, B);
                    frame++;
                }
                //B up
                for (int i = 0; i <= step; i++)
                {
                    byte ii = (byte)((250 / step) * i); //B 0~255 brightness
                    B = ii;
                    SetLedColorSync_AllMB(LedNames, R, G, B);
                    frame++;
                }
                //G down
                for (int i = step; i >= 0; i--)
                {
                    byte ii = (byte)((250 / step) * i); //G 0~255 brightness
                    G = ii;
                    SetLedColorSync_AllMB(LedNames, R, G, B);
                    frame++;
                }
                //R up
                for (int i = 0; i <= step; i++)
                {
                    byte ii = (byte)((250 / step) * i); //R 0~255 brightness
                    R = ii;
                    SetLedColorSync_AllMB(LedNames, R, G, B);
                    frame++;
                }
                //B down
                for (int i = step; i >= 0; i--)
                {
                    byte ii = (byte)((250 / step) * i); //B 0~255 brightness
                    B = ii;
                    SetLedColorSync_AllMB(LedNames, R, G, B);
                    frame++;
                }
            }
        }

        private void SetLedColorSync_AllMB(string[] LedNames, int R, int G, int B)
        {
            int[] R0, G0, B0;

            R0 = new int[LedNames.Length];
            G0 = new int[LedNames.Length];
            B0 = new int[LedNames.Length];
            for (int i = 0; i < LedNames.Length; i++)
            {
                R0[i] = R;
                G0[i] = G;
                B0[i] = B;
            }

            MLAPI_SetLedColorsSync(devList[0], ref LedNames, R0, G0, B0);
        }
    }
}