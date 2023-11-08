using LaunchBox.LocalStorage;
using LaunchBox.Models.PersistentStore;
using LaunchBox.Utils;
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
using System.Windows.Shapes;
using System.Windows.Threading;

namespace LaunchBox
{
    /// <summary>
    /// Interaction logic for SettingWindow.xaml
    /// </summary>
    public partial class SettingWindow : Window
    {
        public event EventHandler<EventArgs> OnUpdate;

        StoredProfile profile = null;
        StoredApplication application = null;

        DispatcherTimer saveTimer = new DispatcherTimer();

        DispatcherTimer renderTimer = new DispatcherTimer();

        PerformanceCounter performance_cpu;
        PerformanceCounter performance_ram;

        PathGeometry cpu_path_geometry;

        double CPU_MAX = 100;
        double RAM_MAX = 500;

        private string APPID = "";

        List<double> cpu_x = new List<double>();
        List<double> cpu_y = new List<double>();

        List<double> ram_x = new List<double>();
        List<double> ram_y = new List<double>();

        int current_index = 0;
        int current_index_m = 0;

        List<Grid> CPU_Grid = new List<Grid>();
        List<Grid> Memory_Grid = new List<Grid>();

        private double CPUChart_Height = 0;

        public SettingWindow()
        {
            InitializeComponent();
            this.Loaded += SettingWindow_Loaded;
            this.Closed += SettingWindow_Closed;

            CompositionTarget.Rendering += CompositionTarget_Rendering;
        }

        private void CompositionTarget_Rendering(object? sender, EventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
            }
        }

        private void SettingWindow_Closed(object? sender, EventArgs e)
        {
            renderTimer.Stop();
        }

        private void SettingWindow_Loaded(object sender, RoutedEventArgs e)
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

            saveTimer = new DispatcherTimer();
            saveTimer.Interval = TimeSpan.FromSeconds(1);
            saveTimer.Tick += SaveTimer_Tick;

            renderTimer = new DispatcherTimer();
            renderTimer.Interval = TimeSpan.FromSeconds(1);
            renderTimer.Tick += RenderTimer_Tick;
            renderTimer.Start();

            Render(APPID);
        }

        private double CalculateRam(double ram)
        {
            return ram/1024/1024 / RAM_MAX * CPUChart_Height;
        }

        private double CalculateCPU(double cpu)
        {
            return cpu / Environment.ProcessorCount / CPU_MAX * CPUChart_Height;
        }

        private void RenderTimer_Tick(object? sender, EventArgs e)
        {
            if (ProcessUtil.isProcessAlive(application.name.Substring(0, application.name.Length - application.extension.Length)))
            {

                performance_cpu = new PerformanceCounter("Process", "% Processor Time", AppName_TextBox.Text, true);
                performance_ram = new PerformanceCounter("Process", "Working Set - Private", AppName_TextBox.Text, true);

                Console.WriteLine("CPU:" + performance_cpu.NextValue());
                var cpu_vl = performance_cpu.NextValue() / Environment.ProcessorCount;
                CPU_Value.Text = cpu_vl + "%";
                Memory_Value.Text = performance_ram.NextValue() / 1024 / 1024 + "MB";

                //cpu_x.Add(current_index);
                //cpu_y.Add(performance_cpu.NextValue());

                //ram_x.Add(current_index);
                //ram_y.Add(performance_ram.NextValue() / 1024 / 1024);

                // MessageBox.Show("A");

                //CPU_Chart.Children.Add(PointGrid());
                //CPU_Chart.Children.RemoveAt(0);
                //CPU_Chart.Children.Add(PointGrid());
                CPU_Chart.Margin = new Thickness(CPU_Chart.Margin.Left - 8, CPU_Chart.Margin.Top, CPU_Chart.Margin.Right, CPU_Chart.Margin.Bottom);
                CPU_PATH_Canvas.Margin = new Thickness(CPU_PATH_Canvas.Margin.Left - 8, CPU_PATH_Canvas.Margin.Top, CPU_PATH_Canvas.Margin.Right, CPU_PATH_Canvas.Margin.Bottom);
                current_index++;
                Point lp;

                if (CPU_Clip_Figure.Segments.Count >= 4)
                {
                    CPU_Clip_Figure.Segments.RemoveAt(CPU_Clip_Figure.Segments.Count - 1);
                    CPU_Clip_Figure.Segments.RemoveAt(CPU_Clip_Figure.Segments.Count - 1);
                    CPU_Clip_Figure.Segments.RemoveAt(CPU_Clip_Figure.Segments.Count - 1);
                }

                if (current_index % 4 == 0)
                {
                    CPU_Grid.RemoveAt(0);
                    CPU_Chart.Children.RemoveAt(0);

                    for (int i = 0; i < CPU_Chart.Children.Count; i++)
                    {
                        Canvas.SetLeft(CPU_Chart.Children[i], i * 8);
                    }
                    var gd = PointGrid(CalculateCPU(performance_cpu.NextValue()), true);
                    CPU_Grid.Add(gd);
                    CPU_Chart.Children.Add(gd);
                    CPU_Chart.Margin = new Thickness(0, CPU_Chart.Margin.Top, CPU_Chart.Margin.Right, CPU_Chart.Margin.Bottom);

                    var x = CPU_Path_Figure.Segments.Count == 0 ? CPU_Path_Figure.StartPoint.X : (CPU_Path_Figure.Segments[CPU_Path_Figure.Segments.Count - 1] as LineSegment).Point.X;

                    lp = new Point(x + 8, CPUChart_Height - CalculateCPU(performance_cpu.NextValue()));
                    CPU_Path_Figure.Segments.Add(new LineSegment
                    {
                        Point = lp
                    });

                    CPU_Clip_Figure.Segments.Add(new LineSegment
                    {
                        Point = lp
                    });
                    //CPU_PATH_Canvas.Margin = CPU_Chart.Margin;

                    if (CPU_Path_Figure.StartPoint.X + CPU_PATH_Canvas.Margin.Left < 0)
                    {
                        CPU_Path_Figure.StartPoint = (CPU_Path_Figure.Segments[0] as LineSegment).Point;
                        CPU_Path_Figure.Segments.RemoveAt(0);

                        CPU_Clip_Figure.StartPoint = (CPU_Path_Figure.Segments[0] as LineSegment).Point;
                        CPU_Clip_Figure.Segments.RemoveAt(0);
                    }
                }
                else
                {
                    CPU_Grid.RemoveAt(0);
                    CPU_Chart.Children.RemoveAt(0);

                    for (int i = 0; i < CPU_Chart.Children.Count; i++)
                    {
                        Canvas.SetLeft(CPU_Chart.Children[i], i * 8);
                    }
                    var gd = PointGrid(CalculateCPU(performance_cpu.NextValue()));
                    CPU_Grid.Add(gd);
                    CPU_Chart.Children.Add(gd);
                    CPU_Chart.Margin = new Thickness(0, CPU_Chart.Margin.Top, CPU_Chart.Margin.Right, CPU_Chart.Margin.Bottom);

                    var x = CPU_Path_Figure.Segments.Count == 0 ? CPU_Path_Figure.StartPoint.X : (CPU_Path_Figure.Segments[CPU_Path_Figure.Segments.Count - 1] as LineSegment).Point.X;

                    lp = new Point(x + 8, CPUChart_Height - CalculateCPU(performance_cpu.NextValue()));

                    CPU_Path_Figure.Segments.Add(new LineSegment
                    {
                        Point = lp
                    });
                    CPU_Clip_Figure.Segments.Add(new LineSegment
                    {
                        Point = lp
                    });

                    if (CPU_Path_Figure.StartPoint.X + CPU_PATH_Canvas.Margin.Left < 0)
                    {
                        CPU_Path_Figure.StartPoint = (CPU_Path_Figure.Segments[0] as LineSegment).Point;
                        CPU_Path_Figure.Segments.RemoveAt(0);

                        CPU_Clip_Figure.StartPoint = (CPU_Path_Figure.Segments[0] as LineSegment).Point;
                        CPU_Clip_Figure.Segments.RemoveAt(0);
                    }

                    //CPU_Path_Figure.Segments.Add(new LineSegment
                    //{
                    //    Point = new Point(Canvas.GetLeft(gd), CPUChart_Height - 100)
                    //});
                    //CPU_PATH_Canvas.Margin = CPU_Chart.Margin;



                }

                var endpoint = lp;
                var startpoint = CPU_Path_Figure.StartPoint;
                CPU_Clip_Figure.Segments.Add(new LineSegment
                {
                    Point = new Point(endpoint.X, CPUChart_Height),
                });
                CPU_Clip_Figure.Segments.Add(new LineSegment
                {
                    Point = new Point(startpoint.X, CPUChart_Height),
                });
                CPU_Clip_Figure.Segments.Add(new LineSegment
                {
                    Point = startpoint
                });

                Memory_Chart.Margin = new Thickness(Memory_Chart.Margin.Left - 8, Memory_Chart.Margin.Top, Memory_Chart.Margin.Right, Memory_Chart.Margin.Bottom);
                Memory_PATH_Canvas.Margin = new Thickness(Memory_PATH_Canvas.Margin.Left - 8, Memory_PATH_Canvas.Margin.Top, Memory_PATH_Canvas.Margin.Right, Memory_PATH_Canvas.Margin.Bottom);
                current_index_m++;
                Point lp_m;

                if (Memory_Clip_Figure.Segments.Count >= 4)
                {
                    Memory_Clip_Figure.Segments.RemoveAt(Memory_Clip_Figure.Segments.Count - 1);
                    Memory_Clip_Figure.Segments.RemoveAt(Memory_Clip_Figure.Segments.Count - 1);
                    Memory_Clip_Figure.Segments.RemoveAt(Memory_Clip_Figure.Segments.Count - 1);
                }

                if (current_index_m % 4 == 0)
                {
                    Memory_Grid.RemoveAt(0);
                    Memory_Chart.Children.RemoveAt(0);

                    for (int i = 0; i < Memory_Chart.Children.Count; i++)
                    {
                        Canvas.SetLeft(Memory_Chart.Children[i], i * 8);
                    }
                    var gd_m = PointGrid_Memory(CalculateRam(performance_ram.NextValue()), true);
                    Memory_Grid.Add(gd_m);
                    Memory_Chart.Children.Add(gd_m);
                    Memory_Chart.Margin = new Thickness(0, Memory_Chart.Margin.Top, Memory_Chart.Margin.Right, Memory_Chart.Margin.Bottom);

                    var x_m = Memory_Path_Figure.Segments.Count == 0 ? Memory_Path_Figure.StartPoint.X : (Memory_Path_Figure.Segments[Memory_Path_Figure.Segments.Count - 1] as LineSegment).Point.X;

                    lp_m = new Point(x_m + 8, CPUChart_Height - CalculateRam(performance_ram.NextValue()));
                    Memory_Path_Figure.Segments.Add(new LineSegment
                    {
                        Point = lp_m
                    });

                    Memory_Clip_Figure.Segments.Add(new LineSegment
                    {
                        Point = lp_m
                    });
                    //CPU_PATH_Canvas.Margin = CPU_Chart.Margin;

                    if (Memory_Path_Figure.StartPoint.X + Memory_PATH_Canvas.Margin.Left < 0)
                    {
                        Memory_Path_Figure.StartPoint = (Memory_Path_Figure.Segments[0] as LineSegment).Point;
                        Memory_Path_Figure.Segments.RemoveAt(0);

                        Memory_Clip_Figure.StartPoint = (Memory_Path_Figure.Segments[0] as LineSegment).Point;
                        Memory_Clip_Figure.Segments.RemoveAt(0);
                    }
                }
                else
                {
                    Memory_Grid.RemoveAt(0);
                    Memory_Chart.Children.RemoveAt(0);

                    for (int i = 0; i < Memory_Chart.Children.Count; i++)
                    {
                        Canvas.SetLeft(Memory_Chart.Children[i], i * 8);
                    }
                    var gd_m = PointGrid_Memory(CalculateRam(performance_ram.NextValue()));
                    Memory_Grid.Add(gd_m);
                    Memory_Chart.Children.Add(gd_m);
                    Memory_Chart.Margin = new Thickness(0, Memory_Chart.Margin.Top, Memory_Chart.Margin.Right, Memory_Chart.Margin.Bottom);

                    var x_m = Memory_Path_Figure.Segments.Count == 0 ? Memory_Path_Figure.StartPoint.X : (Memory_Path_Figure.Segments[Memory_Path_Figure.Segments.Count - 1] as LineSegment).Point.X;

                    lp_m = new Point(x_m + 8, CPUChart_Height - CalculateRam(performance_ram.NextValue()));

                    Memory_Path_Figure.Segments.Add(new LineSegment
                    {
                        Point = lp_m
                    });
                    Memory_Clip_Figure.Segments.Add(new LineSegment
                    {
                        Point = lp_m
                    });

                    if (Memory_Path_Figure.StartPoint.X + Memory_PATH_Canvas.Margin.Left < 0)
                    {
                        Memory_Path_Figure.StartPoint = (Memory_Path_Figure.Segments[0] as LineSegment).Point;
                        Memory_Path_Figure.Segments.RemoveAt(0);

                        Memory_Clip_Figure.StartPoint = (Memory_Path_Figure.Segments[0] as LineSegment).Point;
                        Memory_Clip_Figure.Segments.RemoveAt(0);
                    }
                }



                var endpoint_m = lp_m;
                var startpoint_m = Memory_Path_Figure.StartPoint;
                Memory_Clip_Figure.Segments.Add(new LineSegment
                {
                    Point = new Point(endpoint_m.X, CPUChart_Height),
                });
                Memory_Clip_Figure.Segments.Add(new LineSegment
                {
                    Point = new Point(startpoint_m.X, CPUChart_Height),
                });
                Memory_Clip_Figure.Segments.Add(new LineSegment
                {
                    Point = startpoint_m
                });
            }
        }

        private Grid PointGrid(double? value = null, bool showLine=false)
        {
            Grid parent = new Grid { Width = 8, Height=CPUChart_Height };
            Canvas.SetLeft(parent, CPU_Chart.Children.Count * 8);
            Grid child = new Grid
            {
                Width = 2,
                HorizontalAlignment = HorizontalAlignment.Left,
            };
            parent.Children.Add(child);
            Grid cline = new Grid
            {
                Width = 1,
                Background = new SolidColorBrush(Colors.White),
                HorizontalAlignment = HorizontalAlignment.Left,
                Opacity = 0.5
            };
            if (showLine)
            {
                child.Children.Add(cline);
            }
            if(value != null)
            {
                //Ellipse el = new Ellipse
                //{
                //    Width = 2,
                //    Height = 2,
                //    Stroke = FindResource("ThemeColor") as SolidColorBrush,
                //    Fill = FindResource("ThemeColor") as SolidColorBrush,
                //    StrokeThickness = 1,
                //    HorizontalAlignment = HorizontalAlignment.Left,
                //    VerticalAlignment = VerticalAlignment.Bottom,
                //    Margin = new Thickness(-1,0,0,value??0)
                //};
                //child.Children.Add(el);
            }
            return parent;
        }

        private Grid PointGrid_Memory(double? value = null, bool showLine = false)
        {
            Grid parent = new Grid { Width = 8, Height = CPUChart_Height };
            Canvas.SetLeft(parent, Memory_Chart.Children.Count * 8);
            Grid child = new Grid
            {
                Width = 2,
                HorizontalAlignment = HorizontalAlignment.Left,
            };
            parent.Children.Add(child);
            Grid cline = new Grid
            {
                Width = 1,
                Background = new SolidColorBrush(Colors.White),
                HorizontalAlignment = HorizontalAlignment.Left,
                Opacity = 0.5
            };
            if (showLine)
            {
                child.Children.Add(cline);
            }
            if (value != null)
            {
                //Ellipse el = new Ellipse
                //{
                //    Width = 2,
                //    Height = 2,
                //    Stroke = FindResource("ThemeColor") as SolidColorBrush,
                //    Fill = FindResource("ThemeColor") as SolidColorBrush,
                //    StrokeThickness = 1,
                //    HorizontalAlignment = HorizontalAlignment.Left,
                //    VerticalAlignment = VerticalAlignment.Bottom,
                //    Margin = new Thickness(-1,0,0,value??0)
                //};
                //child.Children.Add(el);
            }
            return parent;
        }

        private void SaveTimer_Tick(object? sender, EventArgs e)
        {
            profile.displayname = DisplayName_TextBox.Text;
            profile.parameters = Params_TextBox.Text;
            profile.nowindow = NoWindow_Checkbox.IsChecked ?? false;
            profile.cpu.need = Alarm_CPU_Checkbox.IsChecked ?? false;
            try
            {
                profile.cpu.value = Convert.ToDouble(Alarm_CPU_TextBox.Text);
            }catch(Exception ex) { }
            profile.memory.need = Alarm_Memory_Checkbox.IsChecked ?? false;
            try
            {
                profile.memory.value = Convert.ToDouble(Alarm_Memory_TextBox.Text);
            }catch(Exception ex) { }
            profile.time.need = Alarm_Time_Checkbox.IsChecked ?? false;
            try
            {
                profile.time.value = Convert.ToDouble(Alarm_Time_TextBox.Text);
            }
            catch { }
            profile.alarmnotification = Notification_Alarm_Checkbox.IsChecked ?? false;

            ApplicationProfile.Update(profile);

            OnUpdate?.Invoke(this, null);

            saveTimer.Stop();
        }

        public void Open(string appid)
        {
            APPID = appid;
            this.Show();
        }

        public void Render(string appid)
        {
            CPU_Grid = new List<Grid>();
            for(int i = 0; i < 80; i++)
            {
                Grid gd = PointGrid(null, i%4==0);
                CPU_Grid.Add(gd);
                CPU_Chart.Children.Add(gd);
            }

            Memory_Grid = new List<Grid>();
            for (int i = 0; i < 80; i++)
            {
                Grid gd = PointGrid_Memory(null, i % 4 == 0);
                Memory_Grid.Add(gd);
                Memory_Chart.Children.Add(gd);
            }


            cpu_x = new List<double>();
            cpu_y = new List<double>();

            ram_x = new List<double>();
            ram_y = new List<double>();

            current_index = 0;

            renderTimer.Stop();
            


            application = ApplicationStorage.Get(appid);
            AppName_TextBox.Text = application.name.Substring(0, application.name.Length-application.extension.Length);
            AppPath_TextBox.Text = application.path;

            

            profile = ApplicationProfile.Get(appid);
            if (profile == null)
            {
                profile = new StoredProfile
                {
                    id = Guid.NewGuid().ToString(),
                    appid = appid,
                    displayname = AppName_TextBox.Text,
                    parameters = "",
                    nowindow = false,
                    cpu = new StoredProfile.UseageAlarm
                    {
                        need = false,
                        value = 0
                    },
                    memory = new StoredProfile.UseageAlarm
                    {
                        need = false,
                        value = 0
                    },
                    time = new StoredProfile.UseageAlarm
                    {
                        need = false,
                        value = 0
                    },
                    alarmnotification = false
                };
                ApplicationProfile.Add(profile);
            }
            SetData();

            if (ProcessUtil.isProcessAlive(application.name.Substring(0, application.name.Length - application.extension.Length)))
            {
                renderTimer.Start();
                performance_cpu = new PerformanceCounter("Process", "% Processor Time", AppName_TextBox.Text, true);

                CPU_Value.Text = performance_cpu.NextValue()/Environment.ProcessorCount + "%";

                CPU_Path_Figure.Segments.Clear();

                Grid gd_2 = PointGrid(CalculateCPU(performance_cpu.NextValue()), true);
                CPU_Grid.Add(gd_2);
                CPU_Chart.Children.Add(gd_2);

                CPU_Path_Figure.StartPoint = new Point(Canvas.GetLeft(gd_2), CPUChart_Height - CalculateCPU(performance_cpu.NextValue()));
                CPU_Clip_Figure.StartPoint = new Point(Canvas.GetLeft(gd_2), CPUChart_Height - CalculateCPU(performance_cpu.NextValue()));

                performance_ram = new PerformanceCounter("Process", "Working Set - Private", AppName_TextBox.Text, true);
                Memory_Value.Text = performance_ram.NextValue() / 1024 / 1024 + "MB";

                Memory_Path_Figure.Segments.Clear();

                Grid gd_2_m = PointGrid_Memory(CalculateRam(performance_ram.NextValue()), true);
                Memory_Grid.Add(gd_2_m);
                Memory_Chart.Children.Add(gd_2_m);

                Memory_Path_Figure.StartPoint = new Point(Canvas.GetLeft(gd_2_m), CPUChart_Height - CalculateRam(performance_ram.NextValue()));
                Memory_Clip_Figure.StartPoint = new Point(Canvas.GetLeft(gd_2_m), CPUChart_Height - CalculateRam(performance_ram.NextValue()));
            }
        }

        private void SetData()
        {
            DisplayName_TextBox.Text = profile.displayname;
            Params_TextBox.Text = profile.parameters;
            NoWindow_Checkbox.IsChecked = profile.nowindow;
            Alarm_CPU_Checkbox.IsChecked = profile.cpu.need;
            Alarm_CPU_TextBox.Text = profile.cpu.value + "";
            Alarm_Memory_Checkbox.IsChecked = profile.memory.need;
            Alarm_Memory_TextBox.Text = profile.memory.value + "";
            Alarm_Time_Checkbox.IsChecked = profile.time.need;
            Alarm_Time_TextBox.Text = profile.time.value + "";
            Notification_Alarm_Checkbox.IsChecked = profile.alarmnotification;
        }

        private void StackPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            renderTimer.Stop();
            Close();
        }

        private void DisplayName_TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            saveTimer.Stop();
            saveTimer.Start();
        }

        private void NoWindow_Checkbox_Checked(object sender, RoutedEventArgs e)
        {
            saveTimer.Stop();
            saveTimer.Start();
        }

        private void CPU_Chart_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            CPUChart_Height = CPU_Chart.ActualHeight;
        }
    }
}
