using System.Data.Entity;
using System.IO;
using qichedangan.Data;
using Topshelf;

namespace qichedangan
{
    /// <summary>
    /// The server's main entry point.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// Main.
        /// </summary>
        public static void Main()
        {
            Database.SetInitializer<CSSoftContext>(null);
            // change from service account's dir to more logical one
            Directory.SetCurrentDirectory(System.AppDomain.CurrentDomain.BaseDirectory);

            HostFactory.Run(x =>
                                {
                                    x.RunAsLocalSystem();

                                    x.SetDescription(Configuration.ServiceDescription);
                                    x.SetDisplayName(Configuration.ServiceDisplayName);
                                    x.SetServiceName(Configuration.ServiceName);

                                    x.Service(factory =>
                                                  {
                                                      QuartzServer server = QuartzServerFactory.CreateServer();
                                                      server.Initialize();
                                                      return server;
                                                  });
                                });
        }
    }
}