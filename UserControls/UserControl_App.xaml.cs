using LaunchBox.LocalStorage;
using LaunchBox.Models.PersistentStore;
using LaunchBox.Params;
using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace LaunchBox.UserControls
{
    /// <summary>
    /// Interaction logic for UserControl_App.xaml
    /// </summary>
    public partial class UserControl_App : UserControl
    {
        private static readonly DependencyProperty StatusProperty = DependencyProperty.Register("Status", typeof(ApplicationStatus), typeof(UserControl_App), new PropertyMetadata(ApplicationStatus.STOP));
        public ApplicationStatus Status { get { return (ApplicationStatus)GetValue(StatusProperty); } set { SetValue(StatusProperty, value); } }

        public static readonly DependencyProperty AppNameProperty = DependencyProperty.Register("AppName", typeof(string), typeof(UserControl_App));
        public string AppName { get { return (string)GetValue(AppNameProperty); } set { SetValue(AppNameProperty, value); } }

        public static readonly DependencyProperty AppIconProperty = DependencyProperty.Register("AppIcon", typeof(ImageSource), typeof(UserControl_App));
        public ImageSource AppIcon { get { return (ImageSource)GetValue(AppIconProperty); } set { SetValue(AppIconProperty, value); } }

        public static readonly DependencyProperty ApplicationProperty = DependencyProperty.Register("Application", typeof(StoredApplication), typeof(UserControl_App));
        public StoredApplication Application { get { return (StoredApplication)GetValue(ApplicationProperty); } set { SetValue(ApplicationProperty, value); } }

        public static readonly RoutedEvent OnDeleteEvent = EventManager.RegisterRoutedEvent("OnDelete", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(UserControl_App));
        public event RoutedEventHandler OnDelete
        {
            add
            {
                AddHandler(OnDeleteEvent, value);
            }
            remove
            {
                RemoveHandler(OnDeleteEvent, value);
            }
        }

        public static readonly RoutedEvent OnStartEvent = EventManager.RegisterRoutedEvent("OnStart", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(UserControl_App));
        public event RoutedEventHandler OnStart
        {
            add
            {
                AddHandler(OnStartEvent, value);
            }
            remove
            {
                RemoveHandler(OnStartEvent, value);
            }
        }

        public static readonly RoutedEvent OnStopEvent = EventManager.RegisterRoutedEvent("OnStop", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(UserControl_App));
        public event RoutedEventHandler OnStop
        {
            add
            {
                AddHandler(OnStopEvent, value);
            }
            remove
            {
                RemoveHandler(OnStopEvent, value);
            }
        }

        public static readonly RoutedEvent OnSettingEvent = EventManager.RegisterRoutedEvent("OnSetting", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(UserControl_App));
        public event RoutedEventHandler OnSetting
        {
            add
            {
                AddHandler(OnSettingEvent, value);
            }
            remove
            {
                RemoveHandler(OnSettingEvent, value);
            }
        }

        DispatcherTimer monitorTime;
        PerformanceCounter performance_cpu;
        PerformanceCounter performance_ram;
        StoredProfile profile;

        DateTime LastTrigger;
        DateTime LastMemoryTrigger;

        public UserControl_App()
        {
            InitializeComponent();
            this.Loaded += UserControl_App_Loaded;
        }

        private void UserControl_App_Loaded(object sender, RoutedEventArgs e)
        {
            switch (Status)
            {
                case ApplicationStatus.STOP:
                    Start_Button.Source = FindResource("Play") as BitmapImage;
                    Status_Ellipse.Fill = FindResource("Deactive") as SolidColorBrush;
                    break;
                case ApplicationStatus.START:
                    Start_Button.Source = FindResource("Pause") as BitmapImage;
                    Status_Ellipse.Fill = FindResource("Active") as SolidColorBrush;
                    break;
            }

            LastTrigger = new DateTime(1970, 1, 1);
            LastMemoryTrigger = new DateTime(1970, 1, 1);

            monitorTime = new DispatcherTimer();
            monitorTime.Interval = TimeSpan.FromSeconds(1);
            monitorTime.Tick += MonitorTime_Tick;
            monitorTime.Start();

            profile = ApplicationProfile.Get(Application.id);
        }

        public void RefreshProfile()
        {
            profile = ApplicationProfile.Get(Application.id);
        }

        private void MonitorTime_Tick(object? sender, EventArgs e)
        {
            
            var process_name = Application.name.Substring(0, Application.name.Length - Application.extension.Length);
            if (profile != null)
            {
                if (profile.alarmnotification)
                {

                    if (profile.cpu.need)
                    {
                        performance_cpu = new PerformanceCounter("Process", "% Processor Time", process_name, true);
                        Console.WriteLine("CPU:" + performance_cpu.NextValue());
                        var cpu_v = profile.cpu.value;
                        var rv = performance_cpu.NextValue() / Environment.ProcessorCount;
                        if ( rv >= cpu_v)
                        {
                            // alarm
                            if ((DateTime.Now - LastTrigger).TotalMinutes >= 10)
                            {
                                new ToastContentBuilder()
                                    .AddArgument("action", "viewConversation")
                                    .AddArgument("conversationId", 9813)
                                    .AddText($"{profile.displayname} CPU Alarm")
                                    .AddText($"{profile.displayname} cpu usage has exceed the setting value. Current cpu usage is {rv}%")
                                    .Show();
                                LastTrigger = DateTime.Now;
                            }
                        }
                    }
                    if (profile.memory.need)
                    {
                        performance_ram = new PerformanceCounter("Process", "Working Set - Private", process_name, true);
                        var ram_v = profile.memory.value;
                        var rv = performance_ram.NextValue() / 1024 / 1024;
                        if (rv >= ram_v)
                        {
                            // alarm
                            if ((DateTime.Now - LastMemoryTrigger).TotalMinutes >= 10)
                            {
                                new ToastContentBuilder()
                                    .AddArgument("action", "viewConversation")
                                    .AddArgument("conversationId", 9813)
                                    .AddText($"{profile.displayname} Memory Alarm")
                                    .AddText($"{profile.displayname} memory usage has exceed the setting value. Current memory usage is {rv}mb")
                                    .Show();
                                LastMemoryTrigger = DateTime.Now;
                            }
                        }
                    }
                }
            }
        }

        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            switch (Status)
            {
                case ApplicationStatus.START:
                    Start_Button.Source = FindResource("Play") as BitmapImage;
                    Status_Ellipse.Fill = FindResource("Deactive") as SolidColorBrush;
                    Status = ApplicationStatus.STOP;
                    RaiseEvent(new RoutedEventArgs(OnStopEvent, this));
                    break;
                case ApplicationStatus.STOP:
                    Start_Button.Source = FindResource("Pause") as BitmapImage;
                    Status_Ellipse.Fill = FindResource("Active") as SolidColorBrush;
                    Status = ApplicationStatus.START;
                    RaiseEvent(new RoutedEventArgs(OnStartEvent, this));
                    break;
            }
        }

        private void Image_MouseLeftButtonDown_1(object sender, MouseButtonEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(OnDeleteEvent, this));
        }

        private void Image_MouseLeftButtonDown_2(object sender, MouseButtonEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(OnSettingEvent, this));
        }
    }
}
