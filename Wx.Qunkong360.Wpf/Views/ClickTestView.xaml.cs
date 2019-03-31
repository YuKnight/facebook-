using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using WpfTreeView;
using Wx.Qunkong360.Wpf.Implementation;
using Wx.Qunkong360.Wpf.Utils;
using Xzy.EmbeddedApp.Utils;
using Xzy.EmbeddedApp.WinForm.Tasks;
using System.Linq;
using System.IO;
using Microsoft.Win32;

namespace Wx.Qunkong360.Wpf.Views
{
    /// <summary>
    /// ClickTestView.xaml 的交互逻辑
    /// </summary>
    public partial class ClickTestView : Window
    {
        public ClickTestView()
        {
            InitializeComponent();

            cbIsFuzzySearch.IsChecked = false;
            cbIsFuzzySearchClick.IsChecked = false;

            InitRunningVmsTreeView(treeview);
        }

        /// <summary>
        /// 初始化树结构
        /// </summary>
        /// <param name="wpfTreeView"></param>
        private void InitRunningVmsTreeView(WpfTreeView.WpfTreeView wpfTreeView)
        {

            int runningGroupIndex = VmManager.Instance.RunningGroupIndex;

            if (runningGroupIndex == -1)
            {
                return;
            }

            int groupEndIndex = VmManager.Instance.VmIndexArray[runningGroupIndex, VmManager.Instance.Column - 1];
            int endNumber = groupEndIndex == -1 ? VmManager.Instance.MaxVmNumber : groupEndIndex + 1;


            //string firstLevelNodeText = $"第{runningGroupIndex + 1}组 {VmManager.Instance.VmIndexArray[runningGroupIndex, 0] + 1}-{endNumber}";
            string firstLevelNodeText = string.Format(SystemLanguageManager.Instance.ResourceManager.GetString("Group", SystemLanguageManager.Instance.CultureInfo), runningGroupIndex + 1, VmManager.Instance.VmIndexArray[runningGroupIndex, 0] + 1, endNumber);

            List<WpfTreeViewItem> wpfTreeViewItems = new List<WpfTreeViewItem>();

            WpfTreeViewItem topLevelNode = new WpfTreeViewItem()
            {
                Caption = firstLevelNodeText,
                Id = -1,
                IsExpanded = true,
            };

            wpfTreeViewItems.Add(topLevelNode);

            for (int i = 0; i < VmManager.Instance.Column; i++)
            {
                if (VmManager.Instance.VmIndexArray[runningGroupIndex, i] != -1)
                {
                    WpfTreeViewItem subNode = new WpfTreeViewItem()
                    {
                        Id = VmManager.Instance.VmIndexArray[runningGroupIndex, i] + 1,
                        Caption = string.Format(SystemLanguageManager.Instance.ResourceManager.GetString("Phone_Num", SystemLanguageManager.Instance.CultureInfo), VmManager.Instance.VmIndexArray[runningGroupIndex, i] + 1),
                        ParentId = -1,
                    };

                    wpfTreeViewItems.Add(subNode);
                }
            }

            wpfTreeView.SetItemsSourceData(wpfTreeViewItems, item => item.Caption, item => item.IsExpanded, item => item.Id, item => item.ParentId);
        }


        public async Task ProcessTask()
        {
            ConfigVals.IsRunning = 1;
            //TasksSchedule tasks = new TasksSchedule();
            await TasksSchedule.ProessTask();
        }

        private (bool, int[]) GetTargetMobileIndexs()
        {
            if (treeview.ItemsSourceData == null)
            {
                return (false, new int[0]);
            }

            var targets = from item in treeview.ItemsSourceData.FirstOrDefault().Children
                          where item.IsChecked
                          select (int)item.Id - 1;

            bool result = targets.Any();
            int[] indexs = targets.ToArray();

            return (result, indexs);
        }


        private async void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            #region old implementation

            //string id = tbSearchKey.Text.Trim();

            //var obj = new JObject()
            //{
            //    {"tasktype",(int)TaskType.ExactSearch },
            //    { "txtmsg",""},
            //};

            //var lists = new JArray();
            //lists.Add(id);

            //obj.Add("list", lists);


            //TaskSch taskSch = new TaskSch()
            //{
            //    Bodys = obj.ToString(Newtonsoft.Json.Formatting.None),
            //    MobileIndex = 0,
            //    TypeId = (int)TaskType.ExactSearch,
            //    Status = "waiting",
            //    RepeatNums = 1,
            //    RandomMins = 1,
            //    RandomMaxs = 2,
            //};

            //TasksBLL.CreateTask(taskSch);

            //Thread taskThread = new Thread(ProcessTask);
            //taskThread.Start();

            #endregion

            var target = GetTargetMobileIndexs();

            if (!target.Item1)
            {
                MessageDialogManager.ShowDialogAsync(SystemLanguageManager.Instance.ResourceManager.GetString("Select_Vm", SystemLanguageManager.Instance.CultureInfo));
                return;
            }

            string id = tbSearchKey.Text.Trim();

            bool isFuzzy = cbIsFuzzySearch.IsChecked.Value;

            List<Task<bool>> tasks = new List<Task<bool>>();

            try
            {
                foreach (var index in target.Item2)
                {
                    tasks.Add(isFuzzy ? SimulationTaskWrapper.FuzzySearch(index, id) : SimulationTaskWrapper.Search(index, id));
                }

                bool[] results = await Task.WhenAll(tasks);


                string aggregatedResult = string.Empty;

                for (int i = 0; i < results.Length; i++)
                {
                    aggregatedResult += $"{target.Item2[i] + 1}：{results[i]}\r\n";
                }

                MessageDialogManager.ShowDialogAsync($"{aggregatedResult}");
            }
            catch (Exception ex)
            {
                MessageDialogManager.ShowDialogAsync($"{ex?.Message}");
            }

        }

        private async void btnClick_Click(object sender, RoutedEventArgs e)
        {

            #region old implementation
            //string id = tbClickKey.Text.Trim();

            //var obj = new JObject()
            //{
            //    {"tasktype",(int)TaskType.ExactSearchAndClick },
            //    { "txtmsg",""},
            //};

            //var lists = new JArray();
            //lists.Add(id);
            //lists.Add("1");

            //obj.Add("list", lists);


            //TaskSch taskSch = new TaskSch()
            //{
            //    Bodys = obj.ToString(Newtonsoft.Json.Formatting.None),
            //    MobileIndex = 0,
            //    TypeId = (int)TaskType.ExactSearchAndClick,
            //    Status = "waiting",
            //    RepeatNums = 1,
            //    RandomMins = 1,
            //    RandomMaxs = 2,
            //};

            //TasksBLL.CreateTask(taskSch);

            //Thread taskThread = new Thread(ProcessTask);
            //taskThread.Start();

            #endregion

            var target = GetTargetMobileIndexs();

            if (!target.Item1)
            {
                MessageDialogManager.ShowDialogAsync(SystemLanguageManager.Instance.ResourceManager.GetString("Select_Vm", SystemLanguageManager.Instance.CultureInfo));
                return;
            }

            string id = tbClickKey.Text.Trim();
            string targetIndex = tbTargetIndex.Text.Trim();

            bool isFuzzyClick = cbIsFuzzySearchClick.IsChecked.Value;

            List<Task<bool>> tasks = new List<Task<bool>>();

            try
            {
                foreach (var index in target.Item2)
                {
                    tasks.Add(isFuzzyClick ? SimulationTaskWrapper.FuzzySearchClick(index, id) : SimulationTaskWrapper.SearchClick(index, id));
                }

                bool[] results = await Task.WhenAll(tasks);


                string aggregatedResult = string.Empty;

                for (int i = 0; i < results.Length; i++)
                {
                    aggregatedResult += $"{target.Item2[i] + 1}：{results[i]}\r\n";
                }

                MessageDialogManager.ShowDialogAsync($"{aggregatedResult}");
            }
            catch (Exception ex)
            {
                MessageDialogManager.ShowDialogAsync($"执行结果：{ex?.Message}");
            }
        }

        private async void btnClickParent_Click(object sender, RoutedEventArgs e)
        {
            var target = GetTargetMobileIndexs();

            if (!target.Item1)
            {
                MessageDialogManager.ShowDialogAsync(SystemLanguageManager.Instance.ResourceManager.GetString("Select_Vm", SystemLanguageManager.Instance.CultureInfo));
                return;
            }

            string child = tbChildKey.Text.Trim();
            string childIndex = tbChildIndex.Text.Trim();
            string parentDepth = tbParentDepth.Text.Trim();

            List<Task<bool>> tasks = new List<Task<bool>>();

            try
            {
                foreach (var index in target.Item2)
                {
                    tasks.Add(SimulationTaskWrapper.SearchClickParent(index, child, int.Parse(parentDepth), int.Parse(childIndex)));
                }

                bool[] results = await Task.WhenAll(tasks);


                string aggregatedResult = string.Empty;

                for (int i = 0; i < results.Length; i++)
                {
                    aggregatedResult += $"{target.Item2[i] + 1}：{results[i]}\r\n";
                }

                MessageDialogManager.ShowDialogAsync($"{aggregatedResult}");
            }
            catch (Exception ex)
            {
                MessageDialogManager.ShowDialogAsync($"执行结果：{ex?.Message}");
            }
        }

        private async void btnPaste_Click(object sender, RoutedEventArgs e)
        {
            var target = GetTargetMobileIndexs();

            if (!target.Item1)
            {
                MessageDialogManager.ShowDialogAsync(SystemLanguageManager.Instance.ResourceManager.GetString("Select_Vm", SystemLanguageManager.Instance.CultureInfo));
                return;
            }

            string id = tbPasteKey.Text.Trim();
            List<Task<bool>> tasks = new List<Task<bool>>();

            try
            {
                foreach (var index in target.Item2)
                {
                    tasks.Add(SimulationTaskWrapper.SearchPaste(index, id));
                }

                bool[] results = await Task.WhenAll(tasks);


                string aggregatedResult = string.Empty;

                for (int i = 0; i < results.Length; i++)
                {
                    aggregatedResult += $"{target.Item2[i] + 1}：{results[i]}\r\n";
                }

                MessageDialogManager.ShowDialogAsync($"{aggregatedResult}");
            }
            catch (Exception ex)
            {
                MessageDialogManager.ShowDialogAsync($"执行结果：{ex?.Message}");
            }
        }

        private async void btnClickCoordinate_Click(object sender, RoutedEventArgs e)
        {
            #region old implementation

            //string x = tbXClick.Text.Trim();
            //string y = tbYClick.Text.Trim();

            //var obj = new JObject()
            //{
            //    {"tasktype",(int)TaskType.ClickCoordinate },
            //    { "txtmsg",""},
            //};

            //var lists = new JArray();
            //lists.Add($"{x}");
            //lists.Add($"{y}");

            //obj.Add("list", lists);

            //TaskSch taskSch = new TaskSch()
            //{
            //    Bodys = obj.ToString(Newtonsoft.Json.Formatting.None),
            //    MobileIndex = 0,
            //    TypeId = (int)TaskType.ClickCoordinate,
            //    Status = "waiting",
            //    RepeatNums = 1,
            //    RandomMins = 1,
            //    RandomMaxs = 2,
            //};

            //TasksBLL.CreateTask(taskSch);

            //Thread taskThread = new Thread(ProcessTask);
            //taskThread.Start();
            #endregion
            var target = GetTargetMobileIndexs();

            if (!target.Item1)
            {
                MessageDialogManager.ShowDialogAsync(SystemLanguageManager.Instance.ResourceManager.GetString("Select_Vm", SystemLanguageManager.Instance.CultureInfo));
                return;
            }

            string xStr = tbXClick.Text.Trim();
            string yStr = tbYClick.Text.Trim();

            int x, y;
            if (!int.TryParse(xStr, out x) || !int.TryParse(yStr, out y))
            {

                MessageDialogManager.ShowDialogAsync("请输入有效的坐标！");

                return;
            }

            List<Task<bool>> tasks = new List<Task<bool>>();

            try
            {
                foreach (var index in target.Item2)
                {
                    tasks.Add(SimulationTaskWrapper.ClickCoordinate(index, x, y));
                }

                bool[] results = await Task.WhenAll(tasks);


                string aggregatedResult = string.Empty;

                for (int i = 0; i < results.Length; i++)
                {
                    aggregatedResult += $"{target.Item2[i] + 1}：{results[i]}\r\n";
                }

                MessageDialogManager.ShowDialogAsync($"{aggregatedResult}");
            }
            catch (Exception ex)
            {
                MessageDialogManager.ShowDialogAsync($"执行结果：{ex?.Message}");
            }
        }

        private async void btnAssign_Click(object sender, RoutedEventArgs e)
        {
            #region old implementation
            //string id = tbAssignmentKey.Text.Trim();
            //string value = tbAssignment.Text.Trim();

            //var obj = new JObject()
            //{
            //    {"tasktype",(int)TaskType.ExactSearchAndAssign },
            //    { "txtmsg",""},
            //};

            //var lists = new JArray();
            //lists.Add(id);
            //lists.Add(value);
            //lists.Add("0");

            //obj.Add("list", lists);

            //TaskSch taskSch = new TaskSch()
            //{
            //    Bodys = obj.ToString(Newtonsoft.Json.Formatting.None),
            //    MobileIndex = 0,
            //    TypeId = (int)TaskType.ExactSearchAndAssign,
            //    Status = "waiting",
            //    RepeatNums = 1,
            //    RandomMins = 1,
            //    RandomMaxs = 2,
            //};

            //TasksBLL.CreateTask(taskSch);

            //Thread taskThread = new Thread(ProcessTask);
            //taskThread.Start();
            /**/
            #endregion

            var target = GetTargetMobileIndexs();

            if (!target.Item1)
            {
                MessageDialogManager.ShowDialogAsync(SystemLanguageManager.Instance.ResourceManager.GetString("Select_Vm", SystemLanguageManager.Instance.CultureInfo));
                return;
            }

            string id = tbAssignmentKey.Text.Trim();
            string value = tbAssignment.Text.Trim();

            List<Task<bool>> tasks = new List<Task<bool>>();

            try
            {
                foreach (var index in target.Item2)
                {
                    tasks.Add(SimulationTaskWrapper.Assign(index, id, value));
                }

                bool[] results = await Task.WhenAll(tasks);


                string aggregatedResult = string.Empty;

                for (int i = 0; i < results.Length; i++)
                {
                    aggregatedResult += $"{target.Item2[i] + 1}：{results[i]}\r\n";
                }

                MessageDialogManager.ShowDialogAsync($"{aggregatedResult}");
            }
            catch (Exception ex)
            {
                MessageDialogManager.ShowDialogAsync($"执行结果：{ex?.Message}");
            }
        }

        private async void btnClear_Click(object sender, RoutedEventArgs e)
        {
            #region old implementation
            //string id = tbClearKey.Text.Trim();

            //var obj = new JObject()
            //{
            //    {"tasktype",(int)TaskType.ExactSearchAndClear },
            //    { "txtmsg",""},
            //};

            //var lists = new JArray();
            //lists.Add(id);
            //lists.Add("0");

            //obj.Add("list", lists);

            //TaskSch taskSch = new TaskSch()
            //{
            //    Bodys = obj.ToString(Newtonsoft.Json.Formatting.None),
            //    MobileIndex = 0,
            //    TypeId = (int)TaskType.ExactSearchAndClear,
            //    Status = "waiting",
            //    RepeatNums = 1,
            //    RandomMins = 1,
            //    RandomMaxs = 2,
            //};

            //TasksBLL.CreateTask(taskSch);

            //Thread taskThread = new Thread(ProcessTask);
            //taskThread.Start();

            #endregion
            var target = GetTargetMobileIndexs();

            if (!target.Item1)
            {
                MessageDialogManager.ShowDialogAsync(SystemLanguageManager.Instance.ResourceManager.GetString("Select_Vm", SystemLanguageManager.Instance.CultureInfo));
                return;
            }

            string id = tbClearKey.Text.Trim();

            List<Task<bool>> tasks = new List<Task<bool>>();
            try
            {
                foreach (var index in target.Item2)
                {
                    tasks.Add(SimulationTaskWrapper.Clear(index, id));
                }

                bool[] results = await Task.WhenAll(tasks);


                string aggregatedResult = string.Empty;

                for (int i = 0; i < results.Length; i++)
                {
                    aggregatedResult += $"{target.Item2[i] + 1}：{results[i]}\r\n";
                }

                MessageDialogManager.ShowDialogAsync($"{aggregatedResult}");
            }
            catch (Exception ex)
            {
                MessageDialogManager.ShowDialogAsync($"执行结果：{ex?.Message}");
            }

        }

        private async void btnSwipe_Click(object sender, RoutedEventArgs e)
        {

            #region old implementaion
            //string x1 = tbXSwipe.Text.Trim();
            //string y1 = tbYSwipe.Text.Trim();
            //string x2 = tbXSwipe2.Text.Trim();
            //string y2 = tbYSwipe2.Text.Trim();
            //string steps = tbSteps.Text.Trim();

            //var obj = new JObject()
            //{
            //    {"tasktype",(int)TaskType.Swipe },
            //    { "txtmsg",""},
            //};

            //var lists = new JArray();
            //lists.Add(x1);
            //lists.Add(y1);
            //lists.Add(x2);
            //lists.Add(y2);
            //lists.Add(steps);

            //obj.Add("list", lists);

            //TaskSch taskSch = new TaskSch()
            //{
            //    Bodys = obj.ToString(Newtonsoft.Json.Formatting.None),
            //    MobileIndex = 0,
            //    TypeId = (int)TaskType.Swipe,
            //    Status = "waiting",
            //    RepeatNums = 1,
            //    RandomMins = 1,
            //    RandomMaxs = 2,
            //};

            //TasksBLL.CreateTask(taskSch);

            //Thread taskThread = new Thread(ProcessTask);
            //taskThread.Start();
            #endregion
            var target = GetTargetMobileIndexs();

            if (!target.Item1)
            {
                MessageDialogManager.ShowDialogAsync(SystemLanguageManager.Instance.ResourceManager.GetString("Select_Vm", SystemLanguageManager.Instance.CultureInfo));
                return;
            }

            string x1Str = tbXSwipe.Text.Trim();
            string y1Str = tbYSwipe.Text.Trim();
            string x2Str = tbXSwipe2.Text.Trim();
            string y2Str = tbYSwipe2.Text.Trim();
            string stepsStr = tbSteps.Text.Trim();

            int x1, y1, x2, y2, steps;

            if (!int.TryParse(x1Str, out x1) | !int.TryParse(y1Str, out y1) | !int.TryParse(x2Str, out x2) | !int.TryParse(y2Str, out y2) | !int.TryParse(stepsStr, out steps))
            {
                MessageDialogManager.ShowDialogAsync("请输入有效的参数！");
                return;
            }
            List<Task<bool>> tasks = new List<Task<bool>>();

            try
            {
                foreach (var index in target.Item2)
                {
                    tasks.Add(SimulationTaskWrapper.Swipe(index, x1, y1, x2, y2, steps));
                }

                bool[] results = await Task.WhenAll(tasks);


                string aggregatedResult = string.Empty;

                for (int i = 0; i < results.Length; i++)
                {
                    aggregatedResult += $"{target.Item2[i] + 1}：{results[i]}\r\n";
                }

                MessageDialogManager.ShowDialogAsync($"{aggregatedResult}");
            }
            catch (Exception ex)
            {
                MessageDialogManager.ShowDialogAsync($"执行结果：{ex?.Message}");
            }

        }

        private async void btnLaunchAndroidCommander_Click(object sender, RoutedEventArgs e)
        {
            if (VmManager.Instance.RunningGroupIndex == -1)
            {
                MessageDialogManager.ShowDialogAsync(SystemLanguageManager.Instance.ResourceManager.GetString("Error_Please_Launch_A_Vm", SystemLanguageManager.Instance.CultureInfo));
                return;
            }

            List<Task> tasks = new List<Task>();

            string[] devices = ProcessUtils.GetAttachedDevices();

            foreach (var device in devices)
            {
                tasks.Add(LaunchAndroidCommanderAsync(device));
            }

            await Task.WhenAll(tasks);
        }

        private async Task LaunchAndroidCommanderAsync(string device)
        {
            await Task.Run(() =>
            {
                Console.WriteLine($"begin launching {device}");
                ProcessUtils.LaunchAndroidCommander(device);
                Console.WriteLine($"launched {device}");
            });
        }

        private async void btnGoback_Click(object sender, RoutedEventArgs e)
        {
            var target = GetTargetMobileIndexs();

            if (!target.Item1)
            {
                MessageDialogManager.ShowDialogAsync(SystemLanguageManager.Instance.ResourceManager.GetString("Select_Vm", SystemLanguageManager.Instance.CultureInfo));
                return;
            }

            List<Task<bool>> tasks = new List<Task<bool>>();

            try
            {
                foreach (var index in target.Item2)
                {
                    tasks.Add(SimulationTaskWrapper.GoBack(index));
                }

                bool[] results = await Task.WhenAll(tasks);


                string aggregatedResult = string.Empty;

                for (int i = 0; i < results.Length; i++)
                {
                    aggregatedResult += $"{target.Item2[i] + 1}：{results[i]}\r\n";
                }

                MessageDialogManager.ShowDialogAsync($"{aggregatedResult}");
            }
            catch (Exception ex)
            {
                MessageDialogManager.ShowDialogAsync($"执行结果：{ex?.Message}");
            }
        }

        private async void btnGotoHomepage_Click(object sender, RoutedEventArgs e)
        {
            var target = GetTargetMobileIndexs();

            if (!target.Item1)
            {
                MessageDialogManager.ShowDialogAsync(SystemLanguageManager.Instance.ResourceManager.GetString("Select_Vm", SystemLanguageManager.Instance.CultureInfo));
                return;
            }

            List<Task<bool>> tasks = new List<Task<bool>>();

            try
            {
                foreach (var index in target.Item2)
                {
                    tasks.Add(SimulationTaskWrapper.GotoHomepage(index));
                }

                bool[] results = await Task.WhenAll(tasks);


                string aggregatedResult = string.Empty;

                for (int i = 0; i < results.Length; i++)
                {
                    aggregatedResult += $"{target.Item2[i] + 1}：{results[i]}\r\n";
                }

                MessageDialogManager.ShowDialogAsync($"{aggregatedResult}");
            }
            catch (Exception ex)
            {
                MessageDialogManager.ShowDialogAsync($"执行结果：{ex?.Message}");
            }

        }

        private async void btnGetAttachedDevices_Click(object sender, RoutedEventArgs e)
        {
            string output = string.Empty;

            await Task.Run(() =>
             {
                 string[] devices = ProcessUtils.GetAttachedDevices();

                 for (int i = 0; i < devices.Length; i++)
                 {
                     output += (devices[i] + "\r\n");
                 }
             });

            MessageDialogManager.ShowDialogAsync($"连接设备列表：\r\n{output}");
        }

        private void btnOpenMonitorView_Click(object sender, RoutedEventArgs e)
        {
            MonitorView.ShowMonitorView();
        }

        private Dictionary<int, string[]> _mobilePictures = new Dictionary<int, string[]>();

        private async void btnImportImages_Click(object sender, RoutedEventArgs e)
        {
            var target = GetTargetMobileIndexs();

            if (!target.Item1)
            {
                MessageDialogManager.ShowDialogAsync(SystemLanguageManager.Instance.ResourceManager.GetString("Select_Vm", SystemLanguageManager.Instance.CultureInfo));
                return;
            }

            string images = tbImagePaths.Text.Trim();

            string[] imageArray = images.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            if (imageArray == null || imageArray.Length == 0)
            {
                MessageDialogManager.ShowDialogAsync("请填写本地图片路径！");
                return;
            }

            _mobilePictures.Clear();

            foreach (var index in target.Item2)
            {

                List<string> pics = new List<string>();
                for (int i = 0; i < imageArray.Length; i++)
                {
                    if (!File.Exists(imageArray[i]))
                    {
                        MessageDialogManager.ShowDialogAsync($"{imageArray[i]}不存在！");
                        break;
                    }

                    string picPath = $"/sdcard/a{DateTime.Now.ToString("mmssffff")}{Path.GetExtension(imageArray[i])}";

                    pics.Add(picPath);

                    string mobileId = DeviceConnectionManager.Instance.GetDeviceNameByMobileIndex(index);
                    ProcessUtils.PushFileToVm(mobileId, imageArray[i], picPath);

                }

                _mobilePictures.Add(index, pics.ToArray());

                bool updatePicturesResult = await SimulationTaskWrapper.UpdatePictures(index, pics.ToArray());

                Console.WriteLine($"update pictures reslut:{updatePicturesResult}");
            }
        }

        private async void btnDeleteAll_Click(object sender, RoutedEventArgs e)
        {
            var target = GetTargetMobileIndexs();

            if (!target.Item1)
            {
                MessageDialogManager.ShowDialogAsync(SystemLanguageManager.Instance.ResourceManager.GetString("Select_Vm", SystemLanguageManager.Instance.CultureInfo));
                return;
            }

            foreach (var index in target.Item2)
            {
                if (_mobilePictures.Keys.Contains(index))
                {
                    string[] mobilePics = _mobilePictures[index];
                    bool deletePicturesResult = await SimulationTaskWrapper.DeletePictures(index, mobilePics);

                    Console.WriteLine($"delete pictures result:{deletePicturesResult}");
                }
                else
                {
                    string mobileId = DeviceConnectionManager.Instance.GetDeviceNameByMobileIndex(index);
                    ProcessUtils.DeleteAllPictures(mobileId);
                }
            }
        }

        private void btnSelctPics_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;
            openFileDialog.Title = SystemLanguageManager.Instance.ResourceManager.GetString("Select_Picture", SystemLanguageManager.Instance.CultureInfo);
            openFileDialog.Filter = SystemLanguageManager.Instance.ResourceManager.GetString("Picture_Filter", SystemLanguageManager.Instance.CultureInfo);

            bool? result = openFileDialog.ShowDialog();

            if (result.HasValue && result.Value)
            {
                tbImagePaths.Clear();

                string[] picPathArray = openFileDialog.FileNames;

                foreach (var picPath in picPathArray)
                {
                    tbImagePaths.Text += (picPath + "\r\n");
                }
            }
        }
    }
}
