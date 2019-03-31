using Cj.EmbeddedAPP.BLL;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
//using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using WpfTreeView;
using Wx.Qunkong360.Wpf.Implementation;
using Wx.Qunkong360.Wpf.Utils;
using Xzy.EmbeddedApp.Model;
using Xzy.EmbeddedApp.Utils;
using Xzy.EmbeddedApp.WinForm.Tasks;
//using Panel = System.Windows.Forms.Panel;

namespace Wx.Qunkong360.Wpf.ContentViews
{
    /// <summary>
    /// AddFriendView.xaml 的交互逻辑
    /// </summary>
    public partial class AddFriendView //: UserControl
    {
        public AddFriendView()
        {
            InitializeComponent();

            lblSearchFriend.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Search_Friends_By_Name", SystemLanguageManager.Instance.CultureInfo);
            lblAddGroups.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Join_Groups", SystemLanguageManager.Instance.CultureInfo);
            lblImportList.Content= SystemLanguageManager.Instance.ResourceManager.GetString("Import_Address_List", SystemLanguageManager.Instance.CultureInfo);
            lblAddListFriend.Content = SystemLanguageManager.Instance.ResourceManager.GetString("AddListPhoneNums", SystemLanguageManager.Instance.CultureInfo);
            lblAddFriendByFriend.Content = SystemLanguageManager.Instance.ResourceManager.GetString("AddFriendByFriend", SystemLanguageManager.Instance.CultureInfo);
            lblAllowRequestFriend.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Facebook_AllowFriend", SystemLanguageManager.Instance.CultureInfo);
            lblAddRecommFriend.Content = SystemLanguageManager.Instance.ResourceManager.GetString("AddRecommFriend", SystemLanguageManager.Instance.CultureInfo);
            lblAddPageFriend.Content = SystemLanguageManager.Instance.ResourceManager.GetString("AddHomePageFriend", SystemLanguageManager.Instance.CultureInfo);

            lblImportList_MouseLeftButtonDown(null, null);
        }

        private void lblSearchFriend_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            lblSearchFriendTips.Text = SystemLanguageManager.Instance.ResourceManager.GetString("Search_Friend_Tips", SystemLanguageManager.Instance.CultureInfo);
            lblSearchCity.Text = SystemLanguageManager.Instance.ResourceManager.GetString("Search_City", SystemLanguageManager.Instance.CultureInfo);
            lblAddTimes.Text = SystemLanguageManager.Instance.ResourceManager.GetString("Add_Times", SystemLanguageManager.Instance.CultureInfo);
            lblAddInterval.Text = SystemLanguageManager.Instance.ResourceManager.GetString("Click_Interval", SystemLanguageManager.Instance.CultureInfo);
            lblTimes.Text = SystemLanguageManager.Instance.ResourceManager.GetString("Time", SystemLanguageManager.Instance.CultureInfo);
            lblIntervalUnit.Text = SystemLanguageManager.Instance.ResourceManager.GetString("Second", SystemLanguageManager.Instance.CultureInfo);
            lblSearchExplanation.Text = SystemLanguageManager.Instance.ResourceManager.GetString("Search_Explanation", SystemLanguageManager.Instance.CultureInfo);
            btnSubmitTask_SearchFriend.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Submit_Task", SystemLanguageManager.Instance.CultureInfo);

            lblAllowRequestFriend.FontWeight = FontWeights.Regular;
            lblAddListFriend.FontWeight = FontWeights.Regular;
            lblAddFriendByFriend.FontWeight = FontWeights.Regular;
            lblAddRecommFriend.FontWeight = FontWeights.Regular;
            lblAddPageFriend.FontWeight = FontWeights.Regular;
            lblAddGroups.FontWeight = FontWeights.Regular;
            lblImportList.FontWeight = FontWeights.Regular;

            lblSearchFriend.FontWeight = FontWeights.Bold;

            InitRunningVmsTreeView(treeviewSearchFriend);


            bool enabled = VmManager.Instance.RunningGroupIndex != -1;

            gridSearchFriend.IsEnabled = enabled;
        }

        private void btnSubmitTask_SearchFriend_Click(object sender, RoutedEventArgs e)
        {
            var targets = from item in treeviewSearchFriend.ItemsSourceData.FirstOrDefault().Children
                          where item.IsChecked
                          select (int)item.Id - 1;

            if (targets.Count() == 0)
            {
                MessageDialogManager.ShowDialogAsync(SystemLanguageManager.Instance.ResourceManager.GetString("Select_Vm", SystemLanguageManager.Instance.CultureInfo));

                return;
            }


            int addTimes;
            string strAddTimes = tbAddTimes.Text.Trim();
            if (!int.TryParse(strAddTimes, out addTimes))
            {
                MessageDialogManager.ShowDialogAsync(SystemLanguageManager.Instance.ResourceManager.GetString("Invalid_Operation_Times", SystemLanguageManager.Instance.CultureInfo));

                return;
            }

            string friends = tbFriends.Text.Trim();
            string[] friendArr = friends.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);


            if (friendArr?.Length != targets.Count())
            {
                MessageDialogManager.ShowDialogAsync(SystemLanguageManager.Instance.ResourceManager.GetString("NumOfPhone_Friends_Mismatch", SystemLanguageManager.Instance.CultureInfo));

                return;
            }


            //List<Task<bool>> tasks = new List<Task<bool>>();

            TaskSch taskSch = new TaskSch();

            //List<int> remotes = new List<int>();

            for (int i = 0; i < targets.Count(); i++)
            {
                var list = new JArray();
                var obj = new JObject()
                {
                    { "opernums",addTimes},
                    { "tasktype",(int)TaskType.SearchAndAddFriend},
                    { "txtmsg",""}
                };
                obj.Add("list", list);


                int id = Guid.NewGuid().GetHashCode();

                //remotes.Add(id);

                taskSch.Remotes = $"{id}";
                taskSch.TypeId = (int)TaskType.SearchAndAddFriend;
                taskSch.MobileIndex = targets.ElementAt(i);
                taskSch.Bodys = obj.ToString(Formatting.None);
                taskSch.Status = "waiting";
                taskSch.ResultVal = string.Empty;
                taskSch.RepeatNums = addTimes;
                taskSch.RandomMins = 1;
                taskSch.RandomMaxs = 2;
                taskSch.InputName = friendArr[i];
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


        private void lblAddGroups_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            lblAddTimes2.Text = SystemLanguageManager.Instance.ResourceManager.GetString("Add_Times", SystemLanguageManager.Instance.CultureInfo);
            lblTimes2.Text = SystemLanguageManager.Instance.ResourceManager.GetString("Time", SystemLanguageManager.Instance.CultureInfo);
            lblAddInterval2.Text = SystemLanguageManager.Instance.ResourceManager.GetString("Click_Interval", SystemLanguageManager.Instance.CultureInfo);
            lblIntervalUnit2.Text = SystemLanguageManager.Instance.ResourceManager.GetString("Second", SystemLanguageManager.Instance.CultureInfo);
            lblGroupName.Text = SystemLanguageManager.Instance.ResourceManager.GetString("Group_Name", SystemLanguageManager.Instance.CultureInfo);
            lblGroupExplanation.Text = SystemLanguageManager.Instance.ResourceManager.GetString("Group_Explanation", SystemLanguageManager.Instance.CultureInfo);
            btnSubmitTask_AddGroup.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Submit_Task", SystemLanguageManager.Instance.CultureInfo);


            lblAllowRequestFriend.FontWeight = FontWeights.Regular;
            lblAddListFriend.FontWeight = FontWeights.Regular;
            lblAddFriendByFriend.FontWeight = FontWeights.Regular;
            lblAddRecommFriend.FontWeight = FontWeights.Regular;
            lblAddPageFriend.FontWeight = FontWeights.Regular;
            lblSearchFriend.FontWeight = FontWeights.Regular;
            lblImportList.FontWeight = FontWeights.Regular;

            lblAddGroups.FontWeight = FontWeights.Bold;

            InitRunningVmsTreeView(treeviewJoinGroup);

            bool enabled = VmManager.Instance.RunningGroupIndex != -1;

            gridAddGroups.IsEnabled = enabled;
        }

        private  void btnSubmitTask_AddGroup_Click(object sender, RoutedEventArgs e)
        {
            var targets = from item in treeviewJoinGroup.ItemsSourceData.FirstOrDefault().Children
                          where item.IsChecked
                          select (int)item.Id - 1;

            if (targets.Count() == 0)
            {
                MessageDialogManager.ShowDialogAsync(SystemLanguageManager.Instance.ResourceManager.GetString("Select_Vm", SystemLanguageManager.Instance.CultureInfo));

                return;
            }

            int addTimes;
            string strAddTimes = tbAddTimes2.Text.Trim();
            if (!int.TryParse(strAddTimes, out addTimes))
            {
                MessageDialogManager.ShowDialogAsync(SystemLanguageManager.Instance.ResourceManager.GetString("Invalid_Operation_Times", SystemLanguageManager.Instance.CultureInfo));

                return;
            }

            string groups = tbGroupName.Text.Trim();
            string[] groupArr = groups.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);


            if (groupArr?.Length != targets.Count())
            {
                MessageDialogManager.ShowDialogAsync(SystemLanguageManager.Instance.ResourceManager.GetString("NumOfPhone_Friends_Mismatch", SystemLanguageManager.Instance.CultureInfo));

                return;
            }

            //List<Task<bool>> tasks = new List<Task<bool>>();
            TaskSch taskSch = new TaskSch();
            //List<int> remotes = new List<int>();


            for (int i = 0; i < targets.Count(); i++)
            {
                //tasks.Add(TasksSchedule.JoinGroupAsync(targets.ElementAt(i), groupArr[i], addTimes));

                var list = new JArray();
                var obj = new JObject()
                {
                    { "opernums",addTimes},
                    { "tasktype",(int)TaskType.SearchAndJoinGroup},
                    { "txtmsg",""}
                };
                obj.Add("list", list);


                int id = Guid.NewGuid().GetHashCode();

                //remotes.Add(id);

                taskSch.Remotes = $"{id}";
                taskSch.TypeId = (int)TaskType.SearchAndJoinGroup;
                taskSch.MobileIndex = targets.ElementAt(i);
                taskSch.Bodys = obj.ToString(Formatting.None);
                taskSch.Status = "waiting";
                taskSch.ResultVal = string.Empty;
                taskSch.RepeatNums = addTimes;
                taskSch.RandomMins = 1;
                taskSch.RandomMaxs = 2;
                taskSch.InputName = groupArr[i];
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

        #region 导入通讯录好友

        /// <summary>
        /// 导入通讯录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lblImportList_MouseLeftButtonDown(object sender,MouseButtonEventArgs e)
        {           
            btnSelect.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Select_Path", SystemLanguageManager.Instance.CultureInfo);
            btnImport.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Import_Address_List", SystemLanguageManager.Instance.CultureInfo);
            btnClearContact.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Clear_Contact", SystemLanguageManager.Instance.CultureInfo);

            lblAllowRequestFriend.FontWeight = FontWeights.Regular;
            lblAddListFriend.FontWeight = FontWeights.Regular;
            lblAddFriendByFriend.FontWeight = FontWeights.Regular;
            lblAddRecommFriend.FontWeight = FontWeights.Regular;
            lblAddPageFriend.FontWeight = FontWeights.Regular;
            lblSearchFriend.FontWeight = FontWeights.Regular;
            lblAddGroups.FontWeight = FontWeights.Regular;

            lblImportList.FontWeight = FontWeights.Bold;

            //加载树
            InitRunningVmsTreeView(treeviewImportList);

            bool enabled = VmManager.Instance.RunningGroupIndex != -1;
        }

        /// <summary>
        /// 选择要导入的号码文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSelect_Click(object sender, RoutedEventArgs e)
        {
            string path = string.Empty;
            OpenFileDialog openfile = new OpenFileDialog();
            openfile.Multiselect = false;
            openfile.Title = SystemLanguageManager.Instance.ResourceManager.GetString("Select_Phone_File", SystemLanguageManager.Instance.CultureInfo);
            openfile.Filter = SystemLanguageManager.Instance.ResourceManager.GetString("Text_File", SystemLanguageManager.Instance.CultureInfo);
            if (openfile.ShowDialog() == DialogResult.OK)
            {
                tbContactPath.Text = openfile.FileName;
            }
        }

        /// <summary>
        /// 确定导入号码文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnImport_Click(object sender, RoutedEventArgs e)
        {
            //移动文件到指定的目录
            var targets = from item in treeviewImportList.ItemsSourceData.FirstOrDefault().Children
                          where item.IsChecked
                          select (int)item.Id - 1;

            if (targets.Count() == 0)
            {
                MessageDialogManager.ShowDialogAsync(SystemLanguageManager.Instance.ResourceManager.GetString("Select_Vm", SystemLanguageManager.Instance.CultureInfo));
                return;
            }

            List<int> checkMobiles = targets.ToList();

            List<string> phoneStr = new List<string>();

            TextRange textRange = new TextRange(rtbPhoneNums.Document.ContentStart, rtbPhoneNums.Document.ContentEnd);

            if (!string.IsNullOrEmpty(textRange.Text))
            {
                phoneStr = textRange.Text.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.None).ToList();
                phoneStr = phoneStr.Where(s => !string.IsNullOrEmpty(s)).ToList();
            }
            int flag = 0;
            if (tbContactPath.Text != "")
            {
                StreamReader sr = new StreamReader(tbContactPath.Text, Encoding.Default);
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    phoneStr.Add(line.ToString());

                    if (phoneStr.Count > 100000)
                    {
                        flag = -1;
                        break;
                    }
                }
            }

            if (flag == -1)
            {
                MessageDialogManager.ShowDialogAsync(string.Format(SystemLanguageManager.Instance.ResourceManager.GetString("Exceed_Max_Import_Num", SystemLanguageManager.Instance.CultureInfo)));
                return;
            }

            //插入到任务表中
            int sr1 = 21 / 1;
            int onenums = phoneStr.Count / checkMobiles.Count;
            PhonenumBLL phonebll = new PhonenumBLL();
            //TasksBLL taskbll = new TasksBLL();           

            for (int m = 0; m < checkMobiles.Count; m++)
            {
                int res = 0;
                List<string> strIds = new List<string>();
                if (checkMobiles.Count > 1 && m == checkMobiles.Count - 1)
                {
                    onenums = phoneStr.Count;
                }
                for (int i = 0; i < onenums; i++)
                {
                    Phonenum phone = new Phonenum();
                    phone.PhoneNum = phoneStr[i];
                    phone.SimulatorId = checkMobiles[m];
                    phone.Status = 0;   //待导入

                    int nflag = phonebll.InsertPhoneNum(phone);
                    if (nflag > 0)
                    {
                        res++;
                    }
                    strIds.Add(phone.PhoneNum);
                }
                if (strIds != null && strIds.Count > 0)
                {
                    for (int j = 0; j < strIds.Count; j++)
                        phoneStr.Remove(strIds[j]);
                }
                //号码写入文件
                string filepath = CopyPhoneNumsFile(strIds, checkMobiles[m]);

                var lists = new JArray
                {
                };

                if (filepath != "")
                {
                    lists.Add(filepath);
                    var obj = new JObject() { { "tasktype", 1 }, { "txtmsg", "" } };
                    obj.Add("list", lists);
                    //插入任务
                    TaskSch tasks = new TaskSch();
                    tasks.TypeId = 1;
                    tasks.Remotes = checkMobiles[m].ToString();
                    tasks.MobileIndex = checkMobiles[m];
                    tasks.Bodys = obj.ToString(Formatting.None);
                    //tasks.Bodys = JsonConvert.SerializeObject(new string[] { "tasktype:1", "txtmsg:", "filepath:"+ filepath, "nums:"+res}); 
                    tasks.Status = "waiting";
                    tasks.ResultVal = "";
                    tasks.RepeatNums = 1;
                    tasks.RandomMins = 5;
                    tasks.RandomMaxs = 12;
                    tasks.IsStep = 0;
                    TasksBLL.CreateTask(tasks);
                }
            }
            //启动任务列表        
            if (ConfigVals.IsRunning != 1)
            {
                Task.Run(async () =>
                {
                    await ProessTask();
                });
            }

            MessageDialogManager.ShowDialogAsync(string.Format(SystemLanguageManager.Instance.ResourceManager.GetString("Submitted_Task", SystemLanguageManager.Instance.CultureInfo), checkMobiles.Count));
        }

        /// <summary>
        /// 清空通讯录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnClearContact_Click(object sender, RoutedEventArgs e)
        {
            
        }
        /// <summary>
        /// 添加通讯录好友
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lblAddListFriend_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            btn_submittask.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Submit_Task", SystemLanguageManager.Instance.CultureInfo);
            lbl_PhoneAdd.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Facebook_Add_Times", SystemLanguageManager.Instance.CultureInfo);
            lbl_opersecond.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Facebook_Op_Inttime", SystemLanguageManager.Instance.CultureInfo);


            lblImportList.FontWeight = FontWeights.Regular;
            lblAddFriendByFriend.FontWeight = FontWeights.Regular;
            lblAddRecommFriend.FontWeight = FontWeights.Regular;
            lblAllowRequestFriend.FontWeight = FontWeights.Regular;
            lblAddPageFriend.FontWeight = FontWeights.Regular;
            lblSearchFriend.FontWeight = FontWeights.Regular;
            lblAddGroups.FontWeight = FontWeights.Regular;

            lblAddListFriend.FontWeight = FontWeights.Bold;

            //加载树
            InitRunningVmsTreeView(treeviewAddListFriend);

            bool enabled = VmManager.Instance.RunningGroupIndex != -1;
        }

        /// <summary>
        /// 提交添加通讯录好友任务
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_submittask_Click(object sender, RoutedEventArgs e)
        {
            var targets = from item in treeviewAddListFriend.ItemsSourceData.FirstOrDefault().Children
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

            int nums = 0;
            for (int m = 0; m < checkMobiles.Count; m++)
            {
                var lists = new JArray
                {
                };
                var obj = new JObject() { { "opernums", txt_phoneaddtxt.Text }, { "tasktype", (int)TaskType.AddListPhoneNums }, { "txtmsg", "" } };
                obj.Add("list", lists);

                task.TypeId = (int)TaskType.AddListPhoneNums;
                task.Remotes = checkMobiles[m].ToString();
                task.MobileIndex = checkMobiles[m];
                task.Bodys = obj.ToString(Formatting.None);
                task.Status = "waiting";
                task.ResultVal = "";
                task.RepeatNums = Int32.Parse(txt_phoneaddtxt.Text);
                task.RandomMins = Int32.Parse(txt_operminsecondtxt.Text);
                task.RandomMaxs = Int32.Parse(txt_opermaxsecondtxt.Text);
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
        /// 添加好友的好友
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lblAddFriendByFriend_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {            
            btn_friendsubmittask.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Submit_Task", SystemLanguageManager.Instance.CultureInfo);
            lbl_FriendPhoneAdd.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Facebook_Add_Times", SystemLanguageManager.Instance.CultureInfo);
            lbl_FriendOperSecond.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Facebook_Op_Inttime", SystemLanguageManager.Instance.CultureInfo);

            lblImportList.FontWeight = FontWeights.Regular;
            lblAddListFriend.FontWeight = FontWeights.Regular;
            lblAddRecommFriend.FontWeight = FontWeights.Regular;
            lblAllowRequestFriend.FontWeight = FontWeights.Regular;
            lblAddPageFriend.FontWeight = FontWeights.Regular;
            lblSearchFriend.FontWeight = FontWeights.Regular;
            lblAddGroups.FontWeight = FontWeights.Regular;

            lblAddFriendByFriend.FontWeight = FontWeights.Bold;

            InitRunningVmsTreeView(treeviewAddFriendByFriend);
        }

        /// <summary>
        /// 提交添加好友的好友任务
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_friendsubmittask_Click(object sender, RoutedEventArgs e)
        {
            var targets = from item in treeviewAddFriendByFriend.ItemsSourceData.FirstOrDefault().Children
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

            int nums = 0;
            for (int m = 0; m < checkMobiles.Count; m++)
            {
                var lists = new JArray
                {
                };
                var obj = new JObject() { { "opernums", txt_FriendPhoneAddtxt.Text }, { "tasktype", (int)TaskType.AddFriendByFriend }, { "txtmsg", "" } };
                obj.Add("list", lists);

                task.TypeId = (int)TaskType.AddFriendByFriend;
                task.Remotes = checkMobiles[m].ToString();
                task.MobileIndex = checkMobiles[m];
                task.Bodys = obj.ToString(Formatting.None);
                task.Status = "waiting";
                task.ResultVal = "";
                task.RepeatNums = Int32.Parse(txt_FriendPhoneAddtxt.Text);
                task.RandomMins = Int32.Parse(txt_operminsecondtxt.Text);
                task.RandomMaxs = Int32.Parse(txt_opermaxsecondtxt.Text);
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
        /// 启动已提交任务
        /// </summary>
        public async Task ProessTask()
        {
            ConfigVals.IsRunning = 1;
            //TasksSchedule tasks = new TasksSchedule();
            await TasksSchedule.ProessTask();
        }

        /// <summary>
        /// 拷贝号码文件到模拟器
        /// </summary>
        /// <returns></returns>
        public string CopyPhoneNumsFile(List<string> strIds, int mobileIndex)
        {
            string res = "";
            if (strIds != null && strIds.Count > 0)
            {
                string filename = DateTime.Now.ToString("yyyyMMddHHmmssffff");

                //判断目录是否存在
                string filepath = $"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}/PhoneFiles/";
                if (!Directory.Exists(filepath))
                {
                    Directory.CreateDirectory(filepath);
                }

                string path = $"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}/PhoneFiles/{filename}.txt";
                FileStream fs = new FileStream(path, FileMode.Create);
                StreamWriter sw = new StreamWriter(fs);

                for (int i = 0; i < strIds.Count; i++)
                {
                    sw.Write(strIds[i] + "\r\n");
                }
                //清空缓冲区
                sw.Flush();
                //关闭流
                sw.Close();
                fs.Close();
                if (File.Exists(path))
                {
                    //移动到sd卡
                    string target = $"/sdcard/qunkong/txt/{filename}.txt";
                    string mobileId = DeviceConnectionManager.Instance.GetDeviceNameByMobileIndex(mobileIndex);
                    ProcessUtils.PushFileToVm(mobileId, path, target);

                    res = target;
                }
            }

            return res;
        }

        #endregion 导入通讯录好友结束        


        #region 添加推荐好友
        private void lblAddRecommFriend_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            btn_recommsubmittask.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Submit_Task", SystemLanguageManager.Instance.CultureInfo);
            lbl_AddRecommNumsFriend.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Facebook_Add_Times", SystemLanguageManager.Instance.CultureInfo);
            lbl_AddRecommSecoFriend.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Facebook_Op_Inttime", SystemLanguageManager.Instance.CultureInfo);

            lblImportList.FontWeight = FontWeights.Regular;
            lblAddListFriend.FontWeight = FontWeights.Regular;
            lblAddFriendByFriend.FontWeight = FontWeights.Regular;
            lblAllowRequestFriend.FontWeight = FontWeights.Regular;
            lblAddPageFriend.FontWeight = FontWeights.Regular;
            lblSearchFriend.FontWeight = FontWeights.Regular;
            lblAddGroups.FontWeight = FontWeights.Regular;

            lblAddRecommFriend.FontWeight = FontWeights.Bold;

            InitRunningVmsTreeView(treeviewAddRecommFriend);
        }

        /// <summary>
        /// 添加推荐好友
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_recommsubmittask_Click(object sender, RoutedEventArgs e)
        {
            var targets = from item in treeviewAddRecommFriend.ItemsSourceData.FirstOrDefault().Children
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

            int nums = 0;
            for (int m = 0; m < checkMobiles.Count; m++)
            {
                var lists = new JArray
                {
                };
                var obj = new JObject() { { "opernums", txt_AddRecommFriend.Text }, { "tasktype", (int)TaskType.AddRecommFriends }, { "txtmsg", "" } };                
                obj.Add("list", lists);

                task.TypeId = (int)TaskType.AddRecommFriends;
                task.Remotes = checkMobiles[m].ToString();
                task.MobileIndex = checkMobiles[m];
                task.Bodys = obj.ToString(Formatting.None);
                task.Status = "waiting";
                task.ResultVal = "";
                task.RepeatNums = Int32.Parse(txt_AddRecommFriend.Text);
                task.RandomMins = Int32.Parse(txt_PageFriendMinFriend.Text);
                task.RandomMaxs = Int32.Parse(txt_PageFriendMaxFriend.Text);
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
        #endregion end添加推荐好友

        #region 通过好友请求
        /// <summary>
        /// 切换通过好友请求
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lblAllowRequestFriend_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            btn_AllowRequestSubmitTask.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Submit_Task", SystemLanguageManager.Instance.CultureInfo);
            lbl_AllowRequestNumsFriend.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Facebook_Add_Times", SystemLanguageManager.Instance.CultureInfo);
            lbl_AllowRequestSecoFriend.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Facebook_Op_Inttime", SystemLanguageManager.Instance.CultureInfo);
            //lbl_IsWholeRequestFriend.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Facebook_IsWholeTask", SystemLanguageManager.Instance.CultureInfo);

            lblImportList.FontWeight = FontWeights.Regular;
            lblAddListFriend.FontWeight = FontWeights.Regular;
            lblAddFriendByFriend.FontWeight = FontWeights.Regular;
            lblAddRecommFriend.FontWeight = FontWeights.Regular;
            lblAddPageFriend.FontWeight = FontWeights.Regular;
            lblSearchFriend.FontWeight = FontWeights.Regular;
            lblAddGroups.FontWeight = FontWeights.Regular;

            lblAllowRequestFriend.FontWeight = FontWeights.Bold;

            InitRunningVmsTreeView(treeviewAllowRequestFriend);
        }

        /// <summary>
        /// 通过好友请求
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_AllowRequestSubmitTask_Click(object sender, RoutedEventArgs e)
        {
            var targets = from item in treeviewAllowRequestFriend.ItemsSourceData.FirstOrDefault().Children
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

            int nums = 0;
            for (int m = 0; m < checkMobiles.Count; m++)
            {
                var lists = new JArray
                {
                };
                var obj = new JObject() { { "opernums", txt_AllowRequestFriend.Text }, { "tasktype", (int)TaskType.AllowRequestFriend }, { "txtmsg", "" } };
                obj.Add("list", lists);

                task.TypeId = (int)TaskType.AllowRequestFriend;
                task.Remotes = checkMobiles[m].ToString();
                task.MobileIndex = checkMobiles[m];
                task.Bodys = obj.ToString(Formatting.None);
                task.Status = "waiting";
                task.ResultVal = "";
                task.RepeatNums = Int32.Parse(txt_AllowRequestFriend.Text);
                task.RandomMins = Int32.Parse(txt_AllowRequestMinFriend.Text);
                task.RandomMaxs = Int32.Parse(txt_AllowRequestMaxFriend.Text);
                task.IsWhole = 0;//checkIsWholeRequestFriend.IsChecked==true ? 1 : 0;
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

        #endregion end通过好友请求

        #region 添加主页好友
        private void lblAddPageFriend_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            btnPageFriendSubmit.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Submit_Task", SystemLanguageManager.Instance.CultureInfo);
            lbl_PageFriendNums.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Facebook_Add_Times", SystemLanguageManager.Instance.CultureInfo);
            lbl_PageFriendSecoFriend.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Facebook_Op_Inttime", SystemLanguageManager.Instance.CultureInfo);

            lblAllowRequestFriend.FontWeight = FontWeights.Regular;
            lblAddListFriend.FontWeight = FontWeights.Regular;
            lblAddFriendByFriend.FontWeight = FontWeights.Regular;
            lblAddRecommFriend.FontWeight = FontWeights.Regular;
            lblSearchFriend.FontWeight = FontWeights.Regular;
            lblAddGroups.FontWeight = FontWeights.Regular;
            lblImportList.FontWeight = FontWeights.Regular;

            lblAddPageFriend.FontWeight = FontWeights.Bold;

            InitRunningVmsTreeView(treeviewAddPageFriend);
        }

        /// <summary>
        /// 提交添加主页好友任务
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnPageFriendSubmit_Click(object sender, RoutedEventArgs e)
        {
            var targets = from item in treeviewAddPageFriend.ItemsSourceData.FirstOrDefault().Children
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

            //处理主页数量
            List<string> phoneStr = new List<string>();
            TextRange textPages = new TextRange(rtbAddPageFriend.Document.ContentStart, rtbAddPageFriend.Document.ContentEnd);

            if (!string.IsNullOrEmpty(textPages.Text))
            {
                phoneStr = textPages.Text.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.None).ToList();
                phoneStr = phoneStr.Where(s => !string.IsNullOrEmpty(s)).ToList();
            }
            if(phoneStr.Count != checkMobiles.Count)
            {
                MessageDialogManager.ShowDialogAsync(SystemLanguageManager.Instance.ResourceManager.GetString("Check_PageNums", SystemLanguageManager.Instance.CultureInfo));
                return;
            }
            int nums = 0;
            for (int m = 0; m < checkMobiles.Count; m++)
            {
                var lists = new JArray
                {
                };
                var obj = new JObject() { { "opernums", txt_PageFriendFriend.Text }, { "tasktype", (int)TaskType.AddPageFriends }, { "txtmsg", "" } };
                lists.Add(phoneStr[m]);
                obj.Add("list", lists);

                task.TypeId = (int)TaskType.AddPageFriends;
                task.Remotes = checkMobiles[m].ToString();
                task.MobileIndex = checkMobiles[m];
                task.Bodys = obj.ToString(Formatting.None);
                task.Status = "waiting";
                task.ResultVal = "";
                task.RepeatNums = Int32.Parse(txt_PageFriendFriend.Text);
                task.RandomMins = Int32.Parse(txt_PageFriendMinFriend.Text);
                task.RandomMaxs = Int32.Parse(txt_PageFriendMaxFriend.Text);
                task.InputName = phoneStr[m];
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


        #endregion end添加主页好友

    }
}