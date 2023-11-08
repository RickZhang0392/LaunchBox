using LaunchBox.LocalStorage;
using LaunchBox.Models.PersistentStore;
using LaunchBox.Params;
using LaunchBox.Utils;
using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Shapes;

namespace LaunchBox
{
    /// <summary>
    /// Interaction logic for AppMainWindow.xaml
    /// </summary>
    public partial class AppMainWindow : Window
    {
        System.Windows.Forms.NotifyIcon notifyIcon = null;

        SettingWindow settingWindow;

        public AppMainWindow()
        {
            InitializeComponent();
            this.Loaded += AppMainWindow_Loaded;

            this.Left = SystemParameters.WorkArea.Width - this.Width;

            notifyIcon = new System.Windows.Forms.NotifyIcon();
            notifyIcon.BalloonTipText = this.Title;
            notifyIcon.Text = this.Title;
            notifyIcon.Icon = new System.Drawing.Icon("sat.ico");
            notifyIcon.Visible = true;

            notifyIcon.DoubleClick += NotifyIcon_DoubleClick;

            System.Windows.Forms.ContextMenuStrip cms_0 = new System.Windows.Forms.ContextMenuStrip();
            notifyIcon.ContextMenuStrip = cms_0;
            System.Windows.Forms.ToolStripMenuItem ShowMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ShowMenuItem.Text = "Open";
            ShowMenuItem.Click += ShowMenuItem_Click;

            System.Windows.Forms.ContextMenuStrip cms = new System.Windows.Forms.ContextMenuStrip();
            notifyIcon.ContextMenuStrip = cms;
            System.Windows.Forms.ToolStripMenuItem exitMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            exitMenuItem.Text = "Exit";
            exitMenuItem.Click += ExitMenuItem_Click;          

            cms.Items.Add(ShowMenuItem);
            cms.Items.Add(exitMenuItem);

            CompositionTarget.Rendering += CompositionTarget_Rendering;
        }

        private void CompositionTarget_Rendering(object? sender, EventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
            }
        }

        private void NotifyIcon_DoubleClick(object? sender, EventArgs e)
        {
            //this.WindowState = WindowState.Normal;
            this.Left = SystemParameters.WorkArea.Width - this.Width;
            this.Top = 0;
            this.Show();
        }

        private void ShowMenuItem_Click(object? sender, EventArgs e)
        {
            // this.WindowState = WindowState.Normal;
            this.Left = SystemParameters.WorkArea.Width - this.Width;
            this.Top = 0;
            this.Show();
        }

        private void ExitMenuItem_Click(object? sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void AppMainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Version OSVersion = Environment.OSVersion.Version;
            if (OSVersion.Major == 6 && OSVersion.Minor == 1)
            {
                // win7
                this.EnableBlurForWin7();
            }
            else if (OSVersion.Major >= 10)
            {
                this.EnableBlur();
            }

            RefreshApplicationList();
        }

        private void StackPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
            
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            //this.WindowState = WindowState.Minimized;
            this.Hide();
            notifyIcon.ShowBalloonTip(3000, "Application Manager", "I've minimized to the tray, double click my icon to reopen the window.", System.Windows.Forms.ToolTipIcon.Info);
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            string file = "";
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                file = ((System.Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();
                //testimage.Source = FileInfoUtil.GetIcon(file);
                if (!string.IsNullOrEmpty(file))
                {
                    
                    var fileInfo = new FileInfo(file);
                    var extension = fileInfo.Extension.ToLower();
                    // reject shortcut
                    if (!extension.Equals(ConstantsParams.EXTENSION_EXE))
                    {
                        return;
                    }

                    var application = new StoredApplication
                    {
                        name = fileInfo.Name,
                        extension = fileInfo.Extension,
                        path = file
                    };
                    ApplicationStorage.Add(application);

                    RefreshApplicationList();
                }
            }
        }

        public void RefreshApplicationList()
        {
            Application_List.Children.Clear();
            List<StoredApplication> applications = ApplicationStorage.Get();
            foreach(var application in applications)
            {
                var process_name = application.name.Substring(0, application.name.Length - application.extension.Length);
                var profile = ApplicationProfile.Get(application.id);
                var element = new UserControls.UserControl_App
                {
                    Status = ProcessUtil.isProcessAlive(process_name) ? Params.ApplicationStatus.START : Params.ApplicationStatus.STOP,
                    AppIcon = FileInfoUtil.GetIcon(application.path),
                    AppName = profile == null ? process_name : profile.displayname,
                    Application = application
                };
                element.OnDelete += (s, e) =>
                {
                    ApplicationStorage.Remove(application.id);
                    RefreshApplicationList();
                };
                element.OnStart += (s, e) =>
                {
                    var noWindow = profile == null ? false : profile.nowindow;
                    var pms = profile == null ? "" : profile.parameters;
                    ProcessUtil.StartProcess(application.path.Substring(0, application.path.Length-application.name.Length), application.path, application.extension, noWindow, pms);
                };
                element.OnStop += (s, e) =>
                {
                    ProcessUtil.KillProcess(application.name.Substring(0, application.name.Length - application.extension.Length));
                };
                element.OnSetting += (s, e) =>
                {
                    settingWindow = new SettingWindow();
                    settingWindow.Open(application.id);
                    settingWindow.OnUpdate += (s2, e2) =>
                    {
                        RefreshApplicationList();
                        element.RefreshProfile();
                    };
                };
                Application_List.Children.Add(element);
            }

            if (applications.Count == 0)
            {
                Empty_Box.Visibility = Visibility.Visible;
            }
            else
            {
                Empty_Box.Visibility = Visibility.Collapsed;
            }
        }

        public void SendNotify(string title, string subtitle)
        {
            new ToastContentBuilder()
                                .AddArgument("action", "viewConversation")
                                .AddArgument("conversationId", 9813)
                                .AddText(title)
                                .AddText(subtitle)
                                .Show();
        }

        private void Window_DragEnter(object sender, DragEventArgs e)
        {
            string file = "";
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                file = ((System.Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();
                //testimage.Source = FileInfoUtil.GetIcon(file);
                if (!string.IsNullOrEmpty(file))
                {

                    var fileInfo = new FileInfo(file);
                    var extension = fileInfo.Extension.ToLower();
                    // reject shortcut
                    if (extension.Equals(ConstantsParams.EXTENSION_EXE))
                    {
                        e.Effects = DragDropEffects.Move;
                    }
                    else
                    {
                        e.Effects = DragDropEffects.None;
                        e.Handled = true;
                    }

                }
            }
        }
    }
}
