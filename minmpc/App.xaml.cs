using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Autofac;
using Autofac.AttributedComponent;
using minmpc.Core;

namespace minmpc {
    public partial class App : Application {

        public IContainer Container { get; private set; }
        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        private void App_OnStartup(object sender, StartupEventArgs e) {
            var builder = new AttributedContainerBuilder();
            
            builder.RegisterAssemblyTypesWithAttribute(Assembly.GetExecutingAssembly());

            Container = builder.Build();

            Container.Resolve<MpdClient>().Start(cancellationTokenSource.Token);
            Container.Resolve<MainWindow>().Show();

            Container.Resolve<TrayIcon>().Initialize();
        }

        private void App_OnExit(object sender, ExitEventArgs e) {
            Container.Resolve<TrayIcon>().Dispose();

            cancellationTokenSource.Cancel();
        }
    }
}
