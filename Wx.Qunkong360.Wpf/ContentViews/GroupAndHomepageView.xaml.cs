using Cj.EmbeddedAPP.BLL;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
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
    /// GroupAndHomepage.xaml 的交互逻辑
    /// </summary>
    public partial class GroupAndHomepageView : UserControl
    {
        ResourceManager resourceManager;
        CultureInfo cultureInfo;
        public GroupAndHomepageView()
        {
            InitializeComponent();

            resourceManager = new ResourceManager("Wx.Qunkong360.Wpf.Languages.Res", typeof(GroupAndHomepageView).Assembly);
            if (ConfigVals.Lang == 1)
            {
                cultureInfo = CultureInfo.CreateSpecificCulture("zh-cn");
            }
            else if (ConfigVals.Lang == 2)
            {
                cultureInfo = CultureInfo.CreateSpecificCulture("en-us");
            }

            lblGroupManagement.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Group_Management", SystemLanguageManager.Instance.CultureInfo);
            lblSubscribeHomepage.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Subscribe_Homepage", SystemLanguageManager.Instance.CultureInfo);
            lblInviteFriendsToGroup.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Invite_Friends_To_Group", SystemLanguageManager.Instance.CultureInfo);
            lblInviteFriendsToLike.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Invite_Friends_To_Like", SystemLanguageManager.Instance.CultureInfo);
            lblAddGroupUser.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Facebook_AddGroupUser", SystemLanguageManager.Instance.CultureInfo);


            lblGroupManagement_MouseLeftButtonDown(null, null);
        }

        private void lblGroupManagement_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            lblOperationTimes.Text = SystemLanguageManager.Instance.ResourceManager.GetString("Operation_Times_Per_Phone", SystemLanguageManager.Instance.CultureInfo);
            lblTimes.Text = SystemLanguageManager.Instance.ResourceManager.GetString("Time", SystemLanguageManager.Instance.CultureInfo);
            lblClickingInterval.Text = SystemLanguageManager.Instance.ResourceManager.GetString("Click_Interval", SystemLanguageManager.Instance.CultureInfo);
            lblIntervalUnit.Text = SystemLanguageManager.Instance.ResourceManager.GetString("Second", SystemLanguageManager.Instance.CultureInfo);
            lblGroupTypeTags.Text = SystemLanguageManager.Instance.ResourceManager.GetString("Group_Type_Tag", SystemLanguageManager.Instance.CultureInfo);
            lblGroupTypeTagsTips.Text = SystemLanguageManager.Instance.ResourceManager.GetString("Group_Type_Tag_Tips", SystemLanguageManager.Instance.CultureInfo);
            rbAddGroupMember.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Add_Group_Member", SystemLanguageManager.Instance.CultureInfo);
            rbAddGroupAdmin.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Add_Group_Admin", SystemLanguageManager.Instance.CultureInfo);
            lblAddLanguage.Text = SystemLanguageManager.Instance.ResourceManager.GetString("Add_Language", SystemLanguageManager.Instance.CultureInfo);
            cbSimplifiedChinese.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Simplified_Chinese", SystemLanguageManager.Instance.CultureInfo);
            cbTraditionalChinese.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Traditional_Chinese", SystemLanguageManager.Instance.CultureInfo);
            cbEnglish.Content = SystemLanguageManager.Instance.ResourceManager.GetString("English", SystemLanguageManager.Instance.CultureInfo);
            btnSubmitTask_GroupManagement.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Submit_Task", SystemLanguageManager.Instance.CultureInfo);

            lblGroupManagement.FontWeight = FontWeights.Bold;
            lblSubscribeHomepage.FontWeight = FontWeights.Regular;
            lblInviteFriendsToGroup.FontWeight = FontWeights.Regular;
            lblInviteFriendsToLike.FontWeight = FontWeights.Regular;
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

        #region 关注主页
        private void lblSubscribeHomepage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            lblHomePage.Text = SystemLanguageManager.Instance.ResourceManager.GetString("Home_Page_Name", SystemLanguageManager.Instance.CultureInfo);
            lblOperationPhoneFrequency.Text = SystemLanguageManager.Instance.ResourceManager.GetString("Operation_Phone_Frequency", SystemLanguageManager.Instance.CultureInfo);
            lblTimes32.Text = SystemLanguageManager.Instance.ResourceManager.GetString("Time", SystemLanguageManager.Instance.CultureInfo);
            lblTimesThumbs.Text = SystemLanguageManager.Instance.ResourceManager.GetString("Time", SystemLanguageManager.Instance.CultureInfo);
            lblOperationTimeInterval.Text = SystemLanguageManager.Instance.ResourceManager.GetString("Click_Interval", SystemLanguageManager.Instance.CultureInfo);
            lblIntervalUnit2.Text = SystemLanguageManager.Instance.ResourceManager.GetString("Second", SystemLanguageManager.Instance.CultureInfo);
            lblGroupName.Text = SystemLanguageManager.Instance.ResourceManager.GetString("Group_Name", SystemLanguageManager.Instance.CultureInfo);
            btnSubmitTask_AttentionHomePage.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Submit_Task", SystemLanguageManager.Instance.CultureInfo);
            lblHomePageIntervalUnit.Text = SystemLanguageManager.Instance.ResourceManager.GetString("Second", SystemLanguageManager.Instance.CultureInfo);

            lblGroupManagement.FontWeight = FontWeights.Regular;
            lblSubscribeHomepage.FontWeight = FontWeights.Bold;
            lblInviteFriendsToGroup.FontWeight = FontWeights.Regular;
            lblInviteFriendsToLike.FontWeight = FontWeights.Regular;

            //加载树
            InitRunningVmsTreeView(tvAttentionHomePage);
            bool enabled = VmManager.Instance.RunningGroupIndex != -1;
        }
        private void btnSubmitTask_AttentionHomePage_Click(object sender, RoutedEventArgs e)
        {
            var targets = from item in tvAttentionHomePage.ItemsSourceData.FirstOrDefault().Children
                          where item.IsChecked
                          select (int)item.Id-1;
            if (targets.Count() == 0)
            {
                MessageDialogManager.ShowDialogAsync(SystemLanguageManager.Instance.ResourceManager.GetString("Select_Vm", SystemLanguageManager.Instance.CultureInfo));
                return;
            }

            int operationTimes;
            string strOperationTimes = txtOperationPhoneFrequency.Text.Trim();
            if (!int.TryParse(strOperationTimes, out operationTimes))
            {
                MessageDialogManager.ShowDialogAsync(SystemLanguageManager.Instance.ResourceManager.GetString("Number_Of_Operations_Per_Cell_Phone", SystemLanguageManager.Instance.CultureInfo));

                return;
            }
            string homePage = txtHomePage.Text.Trim();
            string[] homepageArr = homePage.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            if (homepageArr?.Length == 0)
            {
                MessageDialogManager.ShowDialogAsync(SystemLanguageManager.Instance.ResourceManager.GetString("Home_Page", SystemLanguageManager.Instance.CultureInfo));

                return;
            }

            //List<Task<bool>> tasks = new List<Task<bool>>();
            TaskSch task = new TaskSch();
            //List<int> remotes = new List<int>();

            for (int i = 0; i < targets.Count(); i++)
            {
                //tasks.Add(TasksSchedule.SubscribeHomepageAsync(targets.ElementAt(i), homepageArr[0], operationTimes));

                var lists = new JArray
                {
                };
                var obj = new JObject() { { "opernums", operationTimes}, { "tasktype", (int)TaskType.AddRecommFriends }, { "txtmsg", "" } };
                obj.Add("list", lists);

                int id = Guid.NewGuid().GetHashCode();
                //remotes.Add(id);
                task.Remotes = $"{id}";
                task.TypeId = (int)TaskType.AttentionHomePage;
                task.MobileIndex = targets.ElementAt(i);
                task.Bodys = obj.ToString(Formatting.None);
                task.Status = "waiting";
                task.ResultVal = "";
                task.RepeatNums = Convert.ToInt32(txtOperationPhoneFrequency.Text.Trim());
                task.RandomMins =Convert.ToInt32(txtOperationTimeIntervaMin.Text.Trim());
                task.RandomMaxs =Convert.ToInt32(txtEndingIntervalMax.Text.Trim());
                task.InputName = homepageArr[0];
                task.IsStep = 1;

                TasksBLL.CreateTask(task);
            }

            MessageQueueManager.Instance.AddInfo(string.Format(SystemLanguageManager.Instance.ResourceManager.GetString("Submitted_Task", SystemLanguageManager.Instance.CultureInfo), targets.Count()));

            //启动任务列表        
            if (ConfigVals.IsRunning != 1)
            {
                Task.Run(async () =>
                {
                    await ProessTask();
                });
            }
        }
        #endregion

        #region 邀请好友进小组
        private void lblInviteFriendsToGroup_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

            lblOperationTimesPerPhone.Text = SystemLanguageManager.Instance.ResourceManager.GetString("Operation_Times_Per_Phone", SystemLanguageManager.Instance.CultureInfo);
            lblTimes2.Text = SystemLanguageManager.Instance.ResourceManager.GetString("Time", SystemLanguageManager.Instance.CultureInfo);
            lblClickingInterval2.Text = SystemLanguageManager.Instance.ResourceManager.GetString("Click_Interval", SystemLanguageManager.Instance.CultureInfo);
            lblIntervalUnit2.Text = SystemLanguageManager.Instance.ResourceManager.GetString("Second", SystemLanguageManager.Instance.CultureInfo);
            lblGroupName.Text = SystemLanguageManager.Instance.ResourceManager.GetString("Group_Name", SystemLanguageManager.Instance.CultureInfo);
            btnSubmitTask_InviteToGroup.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Submit_Task", SystemLanguageManager.Instance.CultureInfo);

            lblGroupManagement.FontWeight = FontWeights.Regular;
            lblSubscribeHomepage.FontWeight = FontWeights.Regular;
            lblInviteFriendsToGroup.FontWeight = FontWeights.Bold;
            lblInviteFriendsToLike.FontWeight = FontWeights.Regular;

            //加载树
            InitRunningVmsTreeView(tvInviteToGroup);
            bool enabled = VmManager.Instance.RunningGroupIndex != -1;
        }
        private void btnSubmitTask_InviteToGroup_Click(object sender, RoutedEventArgs e)
        {
            var targets = from item in tvInviteToGroup.ItemsSourceData.FirstOrDefault().Children
                          where item.IsChecked
                          select (int)item.Id - 1;
            if (targets.Count() == 0)
            {
                MessageDialogManager.ShowDialogAsync(SystemLanguageManager.Instance.ResourceManager.GetString("Select_Vm", SystemLanguageManager.Instance.CultureInfo));
                return;
            }
            int operationTimes;
            string strOperationTimes = tbOperationTimesPerPhone.Text.Trim();
            if (!int.TryParse(strOperationTimes, out operationTimes))
            {
                MessageDialogManager.ShowDialogAsync(SystemLanguageManager.Instance.ResourceManager.GetString("Number_Of_Operations_Per_Cell_Phone", SystemLanguageManager.Instance.CultureInfo));
                return;
            }
            string groups = tbGroupName.Text.Trim();
            string[] groupArr = groups.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            if (groupArr?.Length ==0)
            {
                MessageDialogManager.ShowDialogAsync(SystemLanguageManager.Instance.ResourceManager.GetString("Group_Name", SystemLanguageManager.Instance.CultureInfo));
                return;
            }

            //List<Task<bool>> tasks = new List<Task<bool>>();
            TaskSch task = new TaskSch();
            //List<int> remotes = new List<int>();

            for (int i = 0; i < targets.Count(); i++) 
            {
                //tasks.Add(TasksSchedule.InviteFriendsToGroupAsync(targets.ElementAt(i), groupArr[0], operationTimes));

                var lists = new JArray
                {
                };
                var obj = new JObject() { { "opernums", operationTimes }, { "tasktype", (int)TaskType.InvitingFriends }, { "txtmsg", "" } };

                obj.Add("list", lists);


                int id = Guid.NewGuid().GetHashCode();
                //remotes.Add(id);
                task.Remotes = $"{id}";
                task.TypeId = (int)TaskType.InvitingFriends;
                task.MobileIndex = targets.ElementAt(i);
                task.Bodys = obj.ToString(Formatting.None);
                task.Status = "waiting";
                task.ResultVal = string.Empty;
                task.RepeatNums = Convert.ToInt32(tbOperationTimesPerPhone.Text.Trim());
                task.RandomMins = Convert.ToInt32(tbBeginningIntervalMin.Text.Trim());
                task.RandomMaxs = Convert.ToInt32(tbEndingIntervalMax.Text.Trim());
                task.InputName = groupArr[0];
                task.IsStep = 1;

                TasksBLL.CreateTask(task);
            }

            MessageQueueManager.Instance.AddInfo(string.Format(SystemLanguageManager.Instance.ResourceManager.GetString("Submitted_Task", SystemLanguageManager.Instance.CultureInfo), targets.Count()));

            //启动任务列表        
            if (ConfigVals.IsRunning != 1)
            {
                 Task.Run(async () =>
                 {
                     await ProessTask();
                 });
            }
        }
        #endregion


        #region 邀请好友点赞
        private void lblInviteFriendsToLike_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            lblOperatingFrequency.Text = SystemLanguageManager.Instance.ResourceManager.GetString("Operation_Times_Per_Phone", SystemLanguageManager.Instance.CultureInfo);
            lblTimesThumbs.Text = SystemLanguageManager.Instance.ResourceManager.GetString("Time", SystemLanguageManager.Instance.CultureInfo);
            lblOperationInterval.Text = SystemLanguageManager.Instance.ResourceManager.GetString("Click_Interval", SystemLanguageManager.Instance.CultureInfo);
            lblGroupName1Praise.Text = SystemLanguageManager.Instance.ResourceManager.GetString("Group_Name", SystemLanguageManager.Instance.CultureInfo);
            btnSubmitTask_FriendsSomePraise.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Submit_Task", SystemLanguageManager.Instance.CultureInfo);
            lblIntervalUnit3.Text = SystemLanguageManager.Instance.ResourceManager.GetString("Second", SystemLanguageManager.Instance.CultureInfo);


            lblGroupManagement.FontWeight = FontWeights.Regular;
            lblSubscribeHomepage.FontWeight = FontWeights.Regular;
            lblInviteFriendsToGroup.FontWeight = FontWeights.Regular;
            lblInviteFriendsToLike.FontWeight = FontWeights.Bold;

            //加载树
            InitRunningVmsTreeView(tvInviteToPraise);
            bool enabled = VmManager.Instance.RunningGroupIndex != -1;
        }
        
        private  void btnSubmitTask_FriendsSomePraise_Click(object sender, RoutedEventArgs e)
        {
            var targets = from item in tvInviteToPraise.ItemsSourceData.FirstOrDefault().Children
                          where item.IsChecked
                          select (int)item.Id - 1;
            if (targets.Count() == 0)
            {
                MessageDialogManager.ShowDialogAsync(SystemLanguageManager.Instance.ResourceManager.GetString("Select_Vm", SystemLanguageManager.Instance.CultureInfo));
                return;
            }
            int operationTimes;
            string strOperationTimes = txtOperationTimes.Text.Trim();
            if (!int.TryParse(strOperationTimes, out operationTimes))
            {
                MessageDialogManager.ShowDialogAsync(SystemLanguageManager.Instance.ResourceManager.GetString("Number_Of_Operations_Per_Cell_Phone", SystemLanguageManager.Instance.CultureInfo));
                return;
            }
            string groups = txtGroupNamePraise.Text.Trim(); 
            string[] groupArr = groups.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            if (groupArr?.Length ==0)
            {
                MessageDialogManager.ShowDialogAsync(SystemLanguageManager.Instance.ResourceManager.GetString("Group_Name", SystemLanguageManager.Instance.CultureInfo));

                return;
            }

            //List<Task<bool>> tasks = new List<Task<bool>>();
            TaskSch task = new TaskSch();
            //List<int> remotes = new List<int>();

            for (int i = 0; i < targets.Count(); i++)
            {
                //tasks.Add(TasksSchedule.InviteFriendsToLikeAsync(targets.ElementAt(i), groupArr[0], operationTimes));

                var lists = new JArray()
                {
                };
                var obj = new JObject() { { "opernums", txtOperationTimes .Text},{ "tasktype",(int)TaskType.InviteFriendsLike },{"txtmsg",""} };
                obj.Add("list", lists);

                int id = Guid.NewGuid().GetHashCode();
                //remotes.Add(id);
                task.Remotes = $"{id}";
                task.TypeId = (int)TaskType.InviteFriendsLike;
                task.MobileIndex = targets.ElementAt(i);
                task.Bodys = obj.ToString(Formatting.None);
                task.Status = "waiting";
                task.ResultVal = "";
                task.RepeatNums = Convert.ToInt32(txtOperationTimes.Text.Trim());
                task.RandomMins = Convert.ToInt32(tbBeginningIntervMin.Text.Trim());
                task.RandomMaxs = Convert.ToInt32(txtEndingIntervaMax.Text.Trim());
                task.InputName = groupArr[0];
                task.IsStep = 1;

                TasksBLL.CreateTask(task);

            }

            MessageQueueManager.Instance.AddInfo(string.Format(SystemLanguageManager.Instance.ResourceManager.GetString("Submitted_Task", SystemLanguageManager.Instance.CultureInfo), targets.Count()));

            //启动任务列表        
            if (ConfigVals.IsRunning != 1)
            {
                Task.Run(async () =>
                {
                    await ProessTask();
                });
            }
        }
        #endregion
        public async Task ProessTask()
        {
            ConfigVals.IsRunning = 1;
            await TasksSchedule.ProessTask();
        }


        #region 添加群组好友
        /// <summary>
        /// 提交任务
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnGroupFriendSubmit_Click(object sender, RoutedEventArgs e)
        {
            var targets = from item in treeviewAddGroupUser.ItemsSourceData.FirstOrDefault().Children
                          where item.IsChecked
                          select (int)item.Id - 1;

            if (targets.Count() == 0)
            {
                MessageDialogManager.ShowDialogAsync(SystemLanguageManager.Instance.ResourceManager.GetString("Select_Vm", SystemLanguageManager.Instance.CultureInfo));
                return;
            }

            List<int> checkMobiles = targets.ToList();
            TaskSch task = new TaskSch();
            TasksBLL bll = new TasksBLL();

            //小组数量
            List<string> groupStr = new List<string>();
            TextRange textPages = new TextRange(rtbAddPageFriend.Document.ContentStart, rtbAddPageFriend.Document.ContentEnd);

            if (!string.IsNullOrEmpty(textPages.Text))
            {
                groupStr = textPages.Text.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.None).ToList();
                groupStr = groupStr.Where(s => !string.IsNullOrEmpty(s)).ToList();
            }
            if (groupStr.Count != checkMobiles.Count)
            {
                MessageDialogManager.ShowDialogAsync(SystemLanguageManager.Instance.ResourceManager.GetString("Facebook_Check_GroupNums", SystemLanguageManager.Instance.CultureInfo));
                return;
            }

            int nums = 0;
            for (int m = 0; m < checkMobiles.Count; m++)
            {
                var lists = new JArray
                {
                };
                var obj = new JObject() { { "opernums", txt_GroupFriendFriend.Text }, { "tasktype", (int)TaskType.AddPageFriends }, { "txtmsg", "" } };
                lists.Add(groupStr[m]);
                obj.Add("list", lists);

                task.TypeId = (int)TaskType.AddGroupUsers;
                task.Remotes = checkMobiles[m].ToString();
                task.MobileIndex = checkMobiles[m];
                task.Bodys = obj.ToString(Formatting.None);
                task.Status = "waiting";
                task.ResultVal = "";
                task.RepeatNums = Int32.Parse(txt_GroupFriendFriend.Text);
                task.RandomMins = Int32.Parse(txt_GroupFriendMinFriend.Text);
                task.RandomMaxs = Int32.Parse(txt_GroupFriendMaxFriend.Text);
                task.InputName = groupStr[m];
                task.IsStep = 1;
                TasksBLL.CreateTask(task);
                nums++;
            }

            //启动任务列表        
            if (ConfigVals.IsRunning != 1)
            {
                Task.Run(async () =>
                {
                    await ProessTask();
                });
            }

            MessageDialogManager.ShowDialogAsync(string.Format(SystemLanguageManager.Instance.ResourceManager.GetString("Submitted_Task", SystemLanguageManager.Instance.CultureInfo), nums));
        }

        
        /// <summary>
        /// 切换添加群组好友
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lblAddGroupUser_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            lblAddGroupUser.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Facebook_AddGroupUser", SystemLanguageManager.Instance.CultureInfo);
            lbl_GroupFriendNums.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Facebook_Add_Times", SystemLanguageManager.Instance.CultureInfo);
            lbl_GroupFriendSecoFriend.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Facebook_Op_Inttime", SystemLanguageManager.Instance.CultureInfo);
            tbAddPageFriend.Text = SystemLanguageManager.Instance.ResourceManager.GetString("Facebook_AddGroupUser_Tips", SystemLanguageManager.Instance.CultureInfo);
            btnGroupFriendSubmit.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Submit_Task", SystemLanguageManager.Instance.CultureInfo);

            lblGroupManagement.FontWeight = FontWeights.Regular;
            lblSubscribeHomepage.FontWeight = FontWeights.Regular;
            lblInviteFriendsToGroup.FontWeight = FontWeights.Regular;
            lblInviteFriendsToLike.FontWeight = FontWeights.Regular;

            lblAddGroupUser.FontWeight = FontWeights.Bold;

            InitRunningVmsTreeView(treeviewAddGroupUser);
        }


        #endregion
    }
}