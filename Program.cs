namespace winforms;

    static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            NativeMethods.AllocConsole();
            ApplicationConfiguration.Initialize();
            Application.Run(new Form1());
            NativeMethods.FreeConsole();
        }    
    }
