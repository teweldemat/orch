namespace fsstudio
{
    internal static class Program
    {
        public static String ApiUrl;
        public static String OpenAiApiKey;

        public static Guid AccessToken { get; internal set; } = Guid.Empty;

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            ApiUrl = System.Configuration.ConfigurationManager.AppSettings["api-endpoint"];
            OpenAiApiKey = System.Configuration.ConfigurationManager.AppSettings["openai-api-key"];
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.Run(new MainWindow());
        }
    }
}