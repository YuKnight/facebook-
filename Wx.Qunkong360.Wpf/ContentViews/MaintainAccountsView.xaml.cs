using Cj.EmbeddedAPP.BLL;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WpfTreeView;
using Wx.Qunkong360.Wpf.Implementation;
using Wx.Qunkong360.Wpf.Utils;
using Xzy.EmbeddedApp.Model;
using Xzy.EmbeddedApp.Utils;
using Xzy.EmbeddedApp.WinForm.Tasks;

namespace Wx.Qunkong360.Wpf.ContentViews
{
    /// <summary>
    /// MaintainAccountsView.xaml 的交互逻辑
    /// </summary>
    public partial class MaintainAccountsView : UserControl
    {
        public MaintainAccountsView()
        {
            InitializeComponent();

            lblTimelineLike.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Timeline_Like", SystemLanguageManager.Instance.CultureInfo);
            lblFriendsTimelineLike.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Friends_Timeline_Like", SystemLanguageManager.Instance.CultureInfo);
            lblMaintainAccounts.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Maintain_Accounts", SystemLanguageManager.Instance.CultureInfo);

            lblTimelineLike_MouseLeftButtonDown(null, null);
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

        /// <summary>
        /// 启动已提交任务
        /// </summary>
        public async Task ProessTask()
        {
            ConfigVals.IsRunning = 1;
            //TasksSchedule tasks = new TasksSchedule();
            await TasksSchedule.ProessTask();
        }


        private void lblTimelineLike_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            lblSlideNumber.Text = SystemLanguageManager.Instance.ResourceManager.GetString("Slide_Number", SystemLanguageManager.Instance.CultureInfo);
            lblTimes.Text = SystemLanguageManager.Instance.ResourceManager.GetString("Time", SystemLanguageManager.Instance.CultureInfo);
            lblLikeNumber.Text = SystemLanguageManager.Instance.ResourceManager.GetString("Like_Number", SystemLanguageManager.Instance.CultureInfo);
            lblTimes2.Text = SystemLanguageManager.Instance.ResourceManager.GetString("Time", SystemLanguageManager.Instance.CultureInfo);
            lblClickInterval.Text = SystemLanguageManager.Instance.ResourceManager.GetString("Click_Interval", SystemLanguageManager.Instance.CultureInfo);
            lblIntervalUnit.Text = SystemLanguageManager.Instance.ResourceManager.GetString("Second", SystemLanguageManager.Instance.CultureInfo);

            btnSubmitTask_TimelineLike.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Submit_Task", SystemLanguageManager.Instance.CultureInfo);


            lblTimelineLike.FontWeight = FontWeights.Bold;
            lblFriendsTimelineLike.FontWeight = FontWeights.Regular;
            lblMaintainAccounts.FontWeight = FontWeights.Regular;


            InitRunningVmsTreeView(treeviewTimeline);

            bool enabled = VmManager.Instance.RunningGroupIndex != -1;
            gridTimelineLike.IsEnabled = enabled;
        }

        private void lblFriendsTimelineLike_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            lblSlideNumber2.Text = SystemLanguageManager.Instance.ResourceManager.GetString("Slide_Number", SystemLanguageManager.Instance.CultureInfo);
            lblTimes3.Text = SystemLanguageManager.Instance.ResourceManager.GetString("Time", SystemLanguageManager.Instance.CultureInfo);
            lblLikeNumber2.Text = SystemLanguageManager.Instance.ResourceManager.GetString("Like_Number", SystemLanguageManager.Instance.CultureInfo);
            lblTimes4.Text = SystemLanguageManager.Instance.ResourceManager.GetString("Time", SystemLanguageManager.Instance.CultureInfo);
            lblClickInterval2.Text = SystemLanguageManager.Instance.ResourceManager.GetString("Click_Interval", SystemLanguageManager.Instance.CultureInfo);
            lblIntervalUnit2.Text = SystemLanguageManager.Instance.ResourceManager.GetString("Second", SystemLanguageManager.Instance.CultureInfo);

            lblClickFriendsNumber.Text = SystemLanguageManager.Instance.ResourceManager.GetString("Click_Friend_Number", SystemLanguageManager.Instance.CultureInfo);
            lblTimes5.Text = SystemLanguageManager.Instance.ResourceManager.GetString("Time", SystemLanguageManager.Instance.CultureInfo);


            btnSubmitTask_TimelineLike2.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Submit_Task", SystemLanguageManager.Instance.CultureInfo);


            lblTimelineLike.FontWeight = FontWeights.Regular;
            lblFriendsTimelineLike.FontWeight = FontWeights.Bold;
            lblMaintainAccounts.FontWeight = FontWeights.Regular;

            InitRunningVmsTreeView(treeviewTimeline2);

            bool enabled = VmManager.Instance.RunningGroupIndex != -1;
            gridFriendTimelineLike.IsEnabled = enabled;
        }

        private void lblMaintainAccounts_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            rbLikePages.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Like_Posts", SystemLanguageManager.Instance.CultureInfo);
            rbLikeHomepage.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Like_Homepage", SystemLanguageManager.Instance.CultureInfo);
            lblIsLoopExecution.Text = SystemLanguageManager.Instance.ResourceManager.GetString("Is_Loop_Execution", SystemLanguageManager.Instance.CultureInfo);
            rbSingleExecution.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Single_Execution", SystemLanguageManager.Instance.CultureInfo);
            rbLoopExecution.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Loop_Execution", SystemLanguageManager.Instance.CultureInfo);
            lblLoopInterval.Text = SystemLanguageManager.Instance.ResourceManager.GetString("Loop_Interval", SystemLanguageManager.Instance.CultureInfo);
            lblIntervalUnit3.Text = SystemLanguageManager.Instance.ResourceManager.GetString("Minute", SystemLanguageManager.Instance.CultureInfo);
            lblLoopTimes.Text = SystemLanguageManager.Instance.ResourceManager.GetString("Loop_Times", SystemLanguageManager.Instance.CultureInfo);
            cbIsInfinite.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Is_Infinite", SystemLanguageManager.Instance.CultureInfo);
            lblLikeNumber3.Text = SystemLanguageManager.Instance.ResourceManager.GetString("Like_Number", SystemLanguageManager.Instance.CultureInfo);
            lblSlideNumber3.Text = SystemLanguageManager.Instance.ResourceManager.GetString("Slide_Number", SystemLanguageManager.Instance.CultureInfo);
            lblInterval.Text = SystemLanguageManager.Instance.ResourceManager.GetString("Click_Interval", SystemLanguageManager.Instance.CultureInfo);
            lblIntervalUnit4.Text = SystemLanguageManager.Instance.ResourceManager.GetString("Second", SystemLanguageManager.Instance.CultureInfo);
            lblHomepage.Text = SystemLanguageManager.Instance.ResourceManager.GetString("Homepage", SystemLanguageManager.Instance.CultureInfo);
            btnSubmitTask_MaintainAccounts.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Submit_Task", SystemLanguageManager.Instance.CultureInfo);


            lblTimelineLike.FontWeight = FontWeights.Regular;
            lblFriendsTimelineLike.FontWeight = FontWeights.Regular;
            lblMaintainAccounts.FontWeight = FontWeights.Bold;


            InitRunningVmsTreeView(treeviewMaintainAccounts);

            bool enabled = VmManager.Instance.RunningGroupIndex != -1;
            gridMaintainAccount.IsEnabled = enabled;
        }


        private void btnSubmitTask_TimelineLike_Click(object sender, RoutedEventArgs e)
        {
            string str_slideTimes = tbSlideNumber.Text.Trim();
            string str_likeTimes = tbLikeNumber.Text.Trim();
            string str_minInterval = tbBeginningInterval.Text.Trim();
            string str_maxInterval = tbEndingInterval.Text.Trim();

            int slideTimes, likeTimes, minInterval, maxInterval;
            if (!int.TryParse(str_slideTimes, out slideTimes))
            {
                MessageDialogManager.ShowDialogAsync(SystemLanguageManager.Instance.ResourceManager.GetString("Invalid_Slide_Time", SystemLanguageManager.Instance.CultureInfo));
                return;
            }

            if (!int.TryParse(str_likeTimes, out likeTimes))
            {
                MessageDialogManager.ShowDialogAsync(SystemLanguageManager.Instance.ResourceManager.GetString("Invalid_Like_Time", SystemLanguageManager.Instance.CultureInfo));
                return;
            }

            if (!int.TryParse(str_minInterval, out minInterval) || !int.TryParse(str_maxInterval, out maxInterval))
            {
                MessageDialogManager.ShowDialogAsync(SystemLanguageManager.Instance.ResourceManager.GetString("Invalid_Interval", SystemLanguageManager.Instance.CultureInfo));
                return;
            }

            if (minInterval >= maxInterval)
            {
                MessageDialogManager.ShowDialogAsync(SystemLanguageManager.Instance.ResourceManager.GetString("Starting_Interval_Greater_Than_Ending", SystemLanguageManager.Instance.CultureInfo));
                return;
            }

            if (minInterval < 1)
            {
                MessageDialogManager.ShowDialogAsync(SystemLanguageManager.Instance.ResourceManager.GetString("Starting_Interval_Less_Than_One", SystemLanguageManager.Instance.CultureInfo));
                return;
            }

            var targets = from item in treeviewTimeline.ItemsSourceData.FirstOrDefault().Children
                          where item.IsChecked
                          select (int)item.Id - 1;

            if (targets.Count() == 0)
            {
                MessageDialogManager.ShowDialogAsync(SystemLanguageManager.Instance.ResourceManager.GetString("Select_Vm", SystemLanguageManager.Instance.CultureInfo));

                return;
            }


            //List<Task<bool>> tasks = new List<Task<bool>>();

            TaskSch taskSch = new TaskSch();

            //List<int> remotes = new List<int>();

            foreach (var target in targets)
            {
                //tasks.Add(TasksSchedule.TimelineLike(target, slideTimes, likeTimes, minInterval, maxInterval));


                var list = new JArray();
                var obj = new JObject()
                {
                    {"slidenums",slideTimes },
                    { "opernums",likeTimes},
                    { "tasktype",(int)TaskType.TimelineLike},
                    { "txtmsg",""}
                };
                obj.Add("list", list);


                int id = Guid.NewGuid().GetHashCode();

                //remotes.Add(id);

                taskSch.Remotes = $"{id}";
                taskSch.TypeId = (int)TaskType.TimelineLike;
                taskSch.MobileIndex = target;
                taskSch.Bodys = obj.ToString(Formatting.None);
                taskSch.Status = "waiting";
                taskSch.ResultVal = string.Empty;
                taskSch.SlideNums = slideTimes;
                taskSch.RepeatNums = likeTimes;
                taskSch.RandomMins = minInterval;
                taskSch.RandomMaxs = maxInterval;
                //taskSch.InputName = friendArr[i];
                taskSch.IsStep = 1;

                TasksBLL.CreateTask(taskSch);
            }


            MessageQueueManager.Instance.AddInfo(string.Format(SystemLanguageManager.Instance.ResourceManager.GetString("Submitted_Task", SystemLanguageManager.Instance.CultureInfo), targets.Count()));

            if (ConfigVals.IsRunning != 1)
            {
                Task.Run(async () =>
                {
                    await ProessTask();
                });
            }

            //try
            //{
            //    MessageQueueManager.Instance.AddInfo(string.Format(SystemLanguageManager.Instance.ResourceManager.GetString("Submitted_Task", SystemLanguageManager.Instance.CultureInfo), tasks.Count));

            //    EventAggregatorManager.Instance.EventAggregator.GetEvent<TaskUpdatedEvent>().Publish($"{tasks.Count}/{tasks.Count}");

            //    bool[] results = await Task.WhenAll(tasks);

            //    EventAggregatorManager.Instance.EventAggregator.GetEvent<TaskUpdatedEvent>().Publish($"0/{tasks.Count}");

            //    for (int i = 0; i < results.Length; i++)
            //    {
            //        int status = results[i] ? 1 : -1;
            //        int remote = remotes[i];
            //        TasksDAL.UpdateTaskStatusByRemotes($"{remote}", status);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    EventAggregatorManager.Instance.EventAggregator.GetEvent<TaskUpdatedEvent>().Publish($"0/{tasks.Count}");

            //    MessageDialogManager.ShowDialogAsync(ex.Message);
            //}


        }

        private void btnSubmitTask_TimelineLike2_Click(object sender, RoutedEventArgs e)
        {
            string str_slideTimes = tbSlideNumber2.Text.Trim();
            string str_likeTimes = tbLikeNumber2.Text.Trim();
            string str_minInterval = tbBeginningInterval2.Text.Trim();
            string str_maxInterval = tbEndingInterval2.Text.Trim();
            string str_friendNum = tbClickFriendsNumber.Text.Trim();

            int slideTimes, likeTimes, friendNum, minInterval, maxInterval;
            if (!int.TryParse(str_slideTimes, out slideTimes))
            {
                MessageDialogManager.ShowDialogAsync(SystemLanguageManager.Instance.ResourceManager.GetString("Invalid_Slide_Time", SystemLanguageManager.Instance.CultureInfo));
                return;
            }

            if (!int.TryParse(str_likeTimes, out likeTimes))
            {
                MessageDialogManager.ShowDialogAsync(SystemLanguageManager.Instance.ResourceManager.GetString("Invalid_Like_Time", SystemLanguageManager.Instance.CultureInfo));
                return;
            }

            if (!int.TryParse(str_minInterval, out minInterval) || !int.TryParse(str_maxInterval, out maxInterval))
            {
                MessageDialogManager.ShowDialogAsync(SystemLanguageManager.Instance.ResourceManager.GetString("Invalid_Interval", SystemLanguageManager.Instance.CultureInfo));
                return;
            }

            if (minInterval >= maxInterval)
            {
                MessageDialogManager.ShowDialogAsync(SystemLanguageManager.Instance.ResourceManager.GetString("Starting_Interval_Greater_Than_Ending", SystemLanguageManager.Instance.CultureInfo));
                return;
            }

            if (minInterval < 1)
            {
                MessageDialogManager.ShowDialogAsync(SystemLanguageManager.Instance.ResourceManager.GetString("Starting_Interval_Less_Than_One", SystemLanguageManager.Instance.CultureInfo));
                return;
            }

            if (!int.TryParse(str_friendNum, out friendNum))
            {
                MessageDialogManager.ShowDialogAsync(SystemLanguageManager.Instance.ResourceManager.GetString("Invalid_Friend_Num", SystemLanguageManager.Instance.CultureInfo));
                return;
            }

            var targets = from item in treeviewTimeline2.ItemsSourceData.FirstOrDefault().Children
                          where item.IsChecked
                          select (int)item.Id - 1;

            if (targets.Count() == 0)
            {
                MessageDialogManager.ShowDialogAsync(SystemLanguageManager.Instance.ResourceManager.GetString("Select_Vm", SystemLanguageManager.Instance.CultureInfo));

                return;
            }


            //List<Task<bool>> tasks = new List<Task<bool>>();

            TaskSch taskSch = new TaskSch();

            //List<int> remotes = new List<int>();

            foreach (var target in targets)
            {
                //tasks.Add(TasksSchedule.FriendTimelineLike(target, slideTimes, likeTimes, friendNum, minInterval, maxInterval));


                var list = new JArray();
                var obj = new JObject()
                {
                    //{"slidenums",slideTimes },
                    { "opernums",likeTimes},
                    { "tasktype",(int)TaskType.FriendTimelineLike},
                    { "txtmsg",""}
                };
                obj.Add("list", list);


                int id = Guid.NewGuid().GetHashCode();

                //remotes.Add(id);

                taskSch.Remotes = $"{id}";
                taskSch.TypeId = (int)TaskType.FriendTimelineLike;
                taskSch.MobileIndex = target;
                taskSch.Bodys = obj.ToString(Formatting.None);
                taskSch.Status = "waiting";
                taskSch.ResultVal = string.Empty;
                taskSch.SlideNums = slideTimes;
                taskSch.FriendNums = friendNum;
                taskSch.RepeatNums = likeTimes;
                taskSch.RandomMins = minInterval;
                taskSch.RandomMaxs = maxInterval;
                //taskSch.InputName = friendArr[i];
                taskSch.IsStep = 1;

                TasksBLL.CreateTask(taskSch);
            }

            MessageQueueManager.Instance.AddInfo(string.Format(SystemLanguageManager.Instance.ResourceManager.GetString("Submitted_Task", SystemLanguageManager.Instance.CultureInfo), targets.Count()));

            if (ConfigVals.IsRunning != 1)
            {
                Task.Run(async () =>
                {
                    await ProessTask();
                });
            }


            //try
            //{
            //    MessageQueueManager.Instance.AddInfo(string.Format(SystemLanguageManager.Instance.ResourceManager.GetString("Submitted_Task", SystemLanguageManager.Instance.CultureInfo), tasks.Count));

            //    EventAggregatorManager.Instance.EventAggregator.GetEvent<TaskUpdatedEvent>().Publish($"{tasks.Count}/{tasks.Count}");

            //    bool[] results = await Task.WhenAll(tasks);

            //    EventAggregatorManager.Instance.EventAggregator.GetEvent<TaskUpdatedEvent>().Publish($"0/{tasks.Count}");

            //    for (int i = 0; i < results.Length; i++)
            //    {
            //        int status = results[i] ? 1 : -1;
            //        int remote = remotes[i];
            //        TasksDAL.UpdateTaskStatusByRemotes($"{remote}", status);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    EventAggregatorManager.Instance.EventAggregator.GetEvent<TaskUpdatedEvent>().Publish($"0/{tasks.Count}");

            //    MessageDialogManager.ShowDialogAsync(ex.Message);
            //}

        }
    }
}
