﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using Autofac;
using Prism.Autofac;
using Prism.Modularity;
using Serilog;
using St.Common;
using St.Common.Contract;
using St.Host.ViewModels;
using St.Host.Views;
using St.RtClient;
using System.Threading.Tasks;
using Caliburn.Micro;
using St.Common.RtClient;
using St.Host.Core;
using MeetingSdk.NetAgent;
using MeetingSdk.Wpf;
using Prism.Events;
using St.Host.Service;

namespace St.Host
{
    public class Bootstrapper : AutofacBootstrapper
    {
        public Bootstrapper()
        {
            InitializeCaliburn();
        }

        protected override DependencyObject CreateShell()
        {
            return Container.Resolve<MainView>();
        }

        protected override async void InitializeShell()
        {
            //Container.Resolve<DemoView>().ShowDialog();
            //return;

            RegisterSignoutHandler();

            GetSerialNo();

            bool hasDeviceInfo = await GetDeviceInfoFromServer();

            if (!hasDeviceInfo)
            {
                Application.Current.Shutdown();
                return;
            }

            if (GlobalData.Instance.Device.EnableLogin)
            {
                Log.Logger.Debug("【device startup】");
                var deviceLoginView = Container.Resolve<DeviceLoginView>();
                deviceLoginView.ShowDialog();

                var deviceLoginViewModel = deviceLoginView.DataContext as DeviceLoginViewModel;

                Log.Logger.Debug($"【device startup result：{deviceLoginViewModel.IsLoginSucceeded}");

                if (deviceLoginViewModel != null && !deviceLoginViewModel.IsLoginSucceeded)
                {
                    Application.Current.Shutdown();
                    return;
                }
            }
            else
            {
                Log.Logger.Debug("【account startup】");
                var loginView = Container.Resolve<LoginView>();
                loginView.ShowDialog();

                var loginViewModel = loginView.DataContext as LoginViewModel;

                if ((loginViewModel != null) && !loginViewModel.IsLoginSucceeded)
                {
                    Application.Current.Shutdown();
                    return;
                }
            }

            var mainView = Shell as MainView;
            mainView?.Show();
        }

        private async Task<bool> GetDeviceInfoFromServer()
        {
            IBms bmsService = Container.Resolve<IBms>();

            ResponseResult getDeviceResult =
                await
                    bmsService.GetDeviceInfo(GlobalData.Instance.SerialNo,
                        GlobalData.Instance.AggregatedConfig.DeviceKey);

            if (getDeviceResult.Status != "0")
            {
                string msg = $"{getDeviceResult.Message}\r\n本机设备号：{GlobalData.Instance.SerialNo}";

                SscDialog dialog = new SscDialog(msg);
                dialog.ShowDialog();
                return false;
            }

            Device device = getDeviceResult.Data as Device;
            if (device != null)
            {
                GlobalData.Instance.Device = device;
                GlobalData.Instance.AggregatedConfig.DeviceNo = device.Id;

                if (device.IsExpired)
                {
                    string msg = $"{Messages.WarningDeviceExpires}\r\n本机设备号：{GlobalData.Instance.SerialNo}";

                    SscDialog dialog = new SscDialog(msg);
                    dialog.ShowDialog();
                    return false;
                }
                if (device.Locked)
                {
                    string msg = $"{Messages.WarningLockedDevice}\r\n本机设备号：{GlobalData.Instance.SerialNo}";

                    SscDialog dialog = new SscDialog(msg);
                    dialog.ShowDialog();
                    return false;
                }

                return true;
            }

            string emptyDeviceMsg = $"{Messages.WarningEmptyDevice}\r\n本机设备号：{GlobalData.Instance.SerialNo}";
            SscDialog emptyDeviceDialog = new SscDialog(emptyDeviceMsg);
            emptyDeviceDialog.ShowDialog();
            return false;
        }

        protected override IContainer CreateContainer(ContainerBuilder containerBuilder)
        {
            var container = base.CreateContainer(containerBuilder);
            
            // Caliburn IoC 登记
            IoC.GetInstance = this.GetInstance;
            IoC.GetAllInstances = this.GetAllInstances;
            IoC.BuildUp = this.BuildUp;

            return container;
        }

        protected override void ConfigureContainerBuilder(ContainerBuilder builder)
        {
            base.ConfigureContainerBuilder(builder);

            builder.RegisterType<EventAggregator>().As<IEventAggregator>().SingleInstance();

            builder.RegisterType<AutofacRegister>().As<IAutofacRegister>();
            builder.RegisterType<LessonInfo>().SingleInstance();
            builder.RegisterType<Common.UserInfo>().SingleInstance();
            builder.RegisterType<LessonDetail>().SingleInstance();

            builder.RegisterInstance(new List<Common.UserInfo>()).SingleInstance();
            builder.RegisterType<MainView>().AsSelf().SingleInstance();

            builder.RegisterInstance(
                    new RtClientConfiguration(GlobalData.Instance.AggregatedConfig.GetInterfaceItem().RtsAddress))
                .AsSelf()
                .As<IRtClientConfiguration>()
                .SingleInstance();
            builder.RegisterType<RtClientService>().As<IRtClientService>().SingleInstance();

            builder.RegisterType<VisualizeShellService>().As<IVisualizeShell>().SingleInstance();
            builder.RegisterType<MainViewModel>().AsSelf().As<ISignOutHandler>().SingleInstance();

            builder.RegisterType<BmsService>().As<IBms>().SingleInstance();

            builder.RegisterInstance(DefaultMeetingSdkAgent.Instance).As<IMeetingSdkAgent>().SingleInstance();
            builder.RegisterType<MeetingWindowManager>().As<IMeetingWindowManager>().SingleInstance();
            builder.RegisterType<DeviceConfigLoader>().As<IDeviceConfigLoader>().SingleInstance();
            builder.RegisterType<DeviceNameAccessor>().As<IDeviceNameAccessor>().SingleInstance();
            builder.RegisterType<DeviceNameProvider>().As<IDeviceNameProvider>().SingleInstance();
            builder.RegisterType<VideoBoxManager>().As<IVideoBoxManager>();


            builder.RegisterType<AverageLayoutRenderer>().Named<ILayoutRenderer>("AverageLayout");
            builder.RegisterType<BigSmallsLayoutRenderer>().Named<ILayoutRenderer>("BigSmallsLayout");
            builder.RegisterType<CloseupLayoutRenderer>().Named<ILayoutRenderer>("CloseupLayout");

            builder.RegisterType<SpeakerModeDisplayer>().Named<IModeDisplayer>("SpeakerMode");
            builder.RegisterType<ShareModeDisplayer>().Named<IModeDisplayer>("ShareMode");
            builder.RegisterType<InteractionModeDisplayer>().Named<IModeDisplayer>("InteractionMode");


            builder.RegisterType<PublishMicStreamParameterProvider>().As<IStreamParameterProvider<PublishMicStreamParameter>>().SingleInstance();
            builder.RegisterType<PublishCameraStreamParameterProvider>().As<IStreamParameterProvider<PublishCameraStreamParameter>>().SingleInstance();
            builder.RegisterType<PublishDataCardStreamParameterProvider>().As<IStreamParameterProvider<PublishDataCardStreamParameter>>().SingleInstance();
            builder.RegisterType<PublishWinCaptureStreamParameterProvider>().As<IStreamParameterProvider<PublishWinCaptureStreamParameter>>().SingleInstance();
            builder.RegisterType<SubscribeMicStreamParameterProvider>().As<IStreamParameterProvider<SubscribeMicStreamParameter>>().SingleInstance();
            builder.RegisterType<SubscribeCameraStreamParameterProvider>().As<IStreamParameterProvider<SubscribeCameraStreamParameter>>().SingleInstance();
            builder.RegisterType<SubscribeDataCardStreamParameterProvider>().As<IStreamParameterProvider<SubscribeDataCardStreamParameter>>().SingleInstance();
            builder.RegisterType<SubscribeWinCaptureStreamParameterProvider>().As<IStreamParameterProvider<SubscribeWinCaptureStreamParameter>>().SingleInstance();



            //builder.RegisterType<ViewLayoutService>().As<IViewLayout>().SingleInstance();
            builder.RegisterType<GroupManager>().As<IGroupManager>().SingleInstance();
            builder.RegisterType<DialogHelper>().As<IDialogHelper>().SingleInstance();
        }

        protected override void ConfigureModuleCatalog()
        {
            var catalog = (DirectoryModuleCatalog) ModuleCatalog;
            catalog.Initialize();
        }

        protected override IModuleCatalog CreateModuleCatalog()
        {
            return new DirectoryModuleCatalog() {ModulePath = @".\modules"};
        }
        
        private void RegisterSignoutHandler()
        {
            // 注册上线冲突处理程序
            RtServerHandler.Instance.SignOutHandler = () =>
            {
                try
                {
                    // 注销时理清资源，初始化单例注册的UI组件（如MainView）
                    Application.Current.Dispatcher.BeginInvoke(new System.Action((() =>
                    {
                        var handler = IoC.Get<ISignOutHandler>();
                        handler.SignOut();
                    })));
                }
                catch (Exception ex)
                {
                    Log.Logger.Error($"【sign out exception】：{ex}");
                }
            };
        }

        private void GetSerialNo()
        {
            IMeetingSdkAgent sdkService = IoC.Get<IMeetingSdkAgent>();
            GlobalData.Instance.SerialNo = sdkService.GetSerialNo()?.Result;

            Log.Logger.Debug($"【device no.】：{GlobalData.Instance.SerialNo}");
        }

        #region Caliburn
        
        private void InitializeCaliburn()
        {
            PlatformProvider.Current = new XamlPlatformProvider();

            var baseExtractTypes = AssemblySourceCache.ExtractTypes;
            AssemblySourceCache.ExtractTypes = assembly =>
            {
                var baseTypes = baseExtractTypes(assembly);
                var elementTypes = assembly.GetExportedTypes()
                    .Where(t => typeof(UIElement).IsAssignableFrom(t));

                return baseTypes.Union(elementTypes);
            };
            AssemblySourceCache.Install();
            AssemblySource.Instance.AddRange(SelectAssemblies());
        }

        IEnumerable<Assembly> SelectAssemblies()
        {
            return new[] { GetType().Assembly };
        }

        protected object GetInstance(System.Type service, string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                object instance;
                if (this.Container.TryResolve((System.Type)service, out instance))
                    return instance;
            }
            else
            {
                object instance;
                if (this.Container.TryResolveNamed(key, (System.Type)service, out instance))
                    return instance;
            }
            throw new Exception($"Could not locate any instances of contract {(object)(key ?? service.Name)}.");
        }

        protected IEnumerable<object> GetAllInstances(System.Type service)
        {
            return this.Container.Resolve((System.Type)typeof(IEnumerable<>).MakeGenericType(service)) as IEnumerable<object>;
        }

        protected void BuildUp(object instance)
        {
            this.Container.InjectProperties<object>(instance);
        }

        #endregion
    }
}