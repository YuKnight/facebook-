using Cj.EmbeddedAPP.BLL;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Threading;
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
    /// PublishPostView.xaml 的交互逻辑
    /// </summary>
    public partial class PublishPostView : UserControl
    {
        public PublishPostView()
        {
            InitializeComponent();

            lblPublishPicAndText.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Post_Moments", SystemLanguageManager.Instance.CultureInfo);
            lblPublishHomepage.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Send_Homepage", SystemLanguageManager.Instance.CultureInfo);
            lblSendFriendsMessages.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Group_Message_To_Friends", SystemLanguageManager.Instance.CultureInfo);
            lblSendGroupMessages.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Group_Message_To_Groups", SystemLanguageManager.Instance.CultureInfo);

            lblPublishPicAndText_MouseLeftButtonDown(null, null);
        }

        public async Task ProessTask()
        {
            ConfigVals.IsRunning = 1;
            await TasksSchedule.ProessTask();
        }


        private void btnSelectPics_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;
            openFileDialog.Title = SystemLanguageManager.Instance.ResourceManager.GetString("Select_Picture", SystemLanguageManager.Instance.CultureInfo);
            openFileDialog.Filter = SystemLanguageManager.Instance.ResourceManager.GetString("Picture_Filter", SystemLanguageManager.Instance.CultureInfo);

            bool? result = openFileDialog.ShowDialog();

            if (result.HasValue && result.Value)
            {
                if (panelPics.Controls.Count > 0)
                {
                    panelPics.Controls.Clear();
                }

                string[] addedPicPath = openFileDialog.FileNames;

                int startingIndex = panelPics.Controls.Count;
                double total = panelPics.Controls.Count + addedPicPath.Length;
                double columnCapacity = TaskManager.ColumnCapacity;

                int totalRow = (int)Math.Ceiling(total / columnCapacity);

                for (int row = 0; row < totalRow; row++)
                {
                    for (int column = 0; column < TaskManager.ColumnCapacity; column++)
                    {
                        int picturePathIndex = row * TaskManager.ColumnCapacity + column;

                        if (picturePathIndex < total && picturePathIndex >= startingIndex)
                        {

                            System.Windows.Forms.Panel panel = new System.Windows.Forms.Panel()
                            {
                                Name = $"panel{picturePathIndex}",
                                BackColor = System.Drawing.Color.Gray,
                                Width = 300,
                                Height = 300,
                                Location = new System.Drawing.Point()
                                {
                                    X = 5 + column * 305,
                                    Y = 5 + row * 305,
                                },
                            };

                            System.Windows.Forms.CheckBox checkBox = new System.Windows.Forms.CheckBox()
                            {
                                Checked = false,
                                Location = new System.Drawing.Point()
                                {
                                    X = 0,
                                    Y = 0,
                                },
                                Width = 30,
                                Height = 30
                            };

                            System.Windows.Forms.PictureBox pictureBox = new System.Windows.Forms.PictureBox()
                            {
                                BackColor = System.Drawing.Color.Gray,
                                Width = 300,
                                Height = 300,
                                ImageLocation = addedPicPath[picturePathIndex - startingIndex],
                                Location = new System.Drawing.Point()
                                {
                                    X = 0,
                                    Y = 0,
                                },
                                SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom,
                            };

                            panel.Controls.Add(checkBox);
                            panel.Controls.Add(pictureBox);

                            panelPics.Controls.Add(panel);
                        }
                    }
                }
            }
        }


        private void btnDeletePics_Click(object sender, RoutedEventArgs e)
        {
            var panels = panelPics.Controls.Cast<System.Windows.Forms.Panel>().Where(p => p.Controls[0] is System.Windows.Forms.CheckBox && ((System.Windows.Forms.CheckBox)p.Controls[0]).Checked);

            if (panels.Count() == 0)
            {
                return;
            }

            List<string> tobeDeletedKeys = new List<string>();

            foreach (var p in panels)
            {
                tobeDeletedKeys.Add(p.Name);
            }

            foreach (var key in tobeDeletedKeys)
            {
                panelPics.Controls.RemoveByKey(key);
            }

            double total = panelPics.Controls.Count;
            double columnCapacity = TaskManager.ColumnCapacity;

            int totalRow = (int)Math.Ceiling(total / columnCapacity);

            for (int row = 0; row < totalRow; row++)
            {
                for (int column = 0; column < TaskManager.ColumnCapacity; column++)
                {
                    int picturePathIndex = row * TaskManager.ColumnCapacity + column;

                    if (picturePathIndex < total)
                    {
                        System.Windows.Forms.Panel panel = panelPics.Controls[picturePathIndex] as System.Windows.Forms.Panel;

                        panel.Location = new System.Drawing.Point()
                        {
                            X = 5 + column * 305,
                            Y = 5 + row * 305,
                        };
                    }
                }
            }

        }


        private void lblPublishPicAndText_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            lblEnterCopywriting.Text = SystemLanguageManager.Instance.ResourceManager.GetString("Enter_Copywriting", SystemLanguageManager.Instance.CultureInfo);
            btnSelectPics.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Select_Picture", SystemLanguageManager.Instance.CultureInfo);
            btnDeletePics.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Delete_Picture", SystemLanguageManager.Instance.CultureInfo);
            lblSelectPicsTips.Text = SystemLanguageManager.Instance.ResourceManager.GetString("Select_Picture_Tips", SystemLanguageManager.Instance.CultureInfo);
            btnSubmitTask_PostMoment.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Submit_Task", SystemLanguageManager.Instance.CultureInfo);


            lblPublishHomepage.FontWeight = FontWeights.Regular;
            lblPublishPicAndText.FontWeight = FontWeights.Bold;
            lblSendFriendsMessages.FontWeight = FontWeights.Regular;
            lblSendGroupMessages.FontWeight = FontWeights.Regular;

            //加载树
            InitRunningVmsTreeView(treeviewPublishPicAndText);
            bool enabled = VmManager.Instance.RunningGroupIndex != -1;
            gridPublishPicAndText.IsEnabled = enabled;
        }


        private void btnSubmitTask_PostMoment_Click(object sender, RoutedEventArgs e)
        {
            var targets = from item in treeviewPublishPicAndText.ItemsSourceData.FirstOrDefault().Children
                          where item.IsChecked
                          select (int)item.Id - 1;

            if (targets.Count() == 0)
            {
                MessageDialogManager.ShowDialogAsync(SystemLanguageManager.Instance.ResourceManager.GetString("Select_Vm", SystemLanguageManager.Instance.CultureInfo));

                return;
            }


            string text = tbCopywriting.Text.Trim();


            var pictures = from pic in panelPics.Controls.Cast<System.Windows.Forms.Panel>()
                           select ((System.Windows.Forms.PictureBox)pic.Controls[1]).ImageLocation;

            string[] paths = pictures.ToArray();


            if (string.IsNullOrEmpty(text) && paths.Length == 0)
            {
                MessageDialogManager.ShowDialogAsync(SystemLanguageManager.Instance.ResourceManager.GetString("Enter_Copywriting", SystemLanguageManager.Instance.CultureInfo));

                return;
            }

            //string dir = DateTime.Now.ToString("yyyyMMddHHmmssffff");

            //List<Task<bool>> tasks = new List<Task<bool>>();

            TaskSch taskSch = new TaskSch();

            //List<int> remotes = new List<int>();

            //Assembly assembly = Assembly.GetExecutingAssembly();

            //DirectoryInfo rootDir = Directory.GetParent(Path.Combine(assembly.Location));

            //string dateFolder = DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_ffff");

            //string imagesCacheDir = Path.Combine(rootDir.FullName, "cache", "images", dateFolder);

            //if (!Directory.Exists(imagesCacheDir))
            //{
            //    Directory.CreateDirectory(imagesCacheDir);
            //}

            //List<string> pics = new List<string>();

            foreach (var target in targets)
            {
                var lists = new JArray();



                ////ProcessUtils.DeleteAllPictures(DeviceConnectionManager.Instance.GetDeviceNameByMobileIndex(target));

                for (int i = 0; i < paths.Length; i++)
                {
                    //    // cache pictures in a temp directory, so that when the task is being executed, it will grab pics from the temp directory

                    //    string fileName = $"{i + 1}{Path.GetExtension(paths[i])}";
                    //    //string dstFilePath = Path.Combine(imagesCacheDir, fileName);
                    //    //File.Copy(paths[i], dstFilePath, true);


                    lists.Add(paths[i]);


                    //    // just for test
                    //    string picPath = $"/sdcard/Pictures/a{i + 1}{Path.GetExtension(paths[i])}";

                    //    pics.Add(picPath);

                    //    string mobileId = DeviceConnectionManager.Instance.GetDeviceNameByMobileIndex(target);
                    //    ProcessUtils.PushFileToVm(mobileId, paths[i], picPath);
                }

                var obj = new JObject() {
                    {"tasktype",(int)TaskType.PublishPost },
                    { "txtmsg",text},
                };

                obj.Add("list", lists);

                int id = Guid.NewGuid().GetHashCode();

                //tasks.Add(TasksSchedule.PublishPost(target, obj.ToString(Formatting.None)));

                //remotes.Add(id);

                taskSch.Remotes = $"{id}";
                taskSch.TypeId = (int)TaskType.PublishPost;
                taskSch.MobileIndex = target;
                taskSch.Bodys = obj.ToString(Formatting.None);
                taskSch.Status = "waiting";
                taskSch.ResultVal = string.Empty;
                taskSch.SlideNums = 0;
                taskSch.FriendNums = 0;
                taskSch.RepeatNums = 0;
                taskSch.RandomMins = 1;
                taskSch.RandomMaxs = 2;
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


        //#region 发送主页
        private void lblPublishHomepage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            lblSearchHomePageName.Text = SystemLanguageManager.Instance.ResourceManager.GetString("Search_HomePage_Name", SystemLanguageManager.Instance.CultureInfo);
            btnSubmitTask_SendHomepageMsg.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Submit_Task", SystemLanguageManager.Instance.CultureInfo);
            lblPostSharing.Text = SystemLanguageManager.Instance.ResourceManager.GetString("Submit_Task", SystemLanguageManager.Instance.CultureInfo);
            rbMyDynamics.Content = SystemLanguageManager.Instance.ResourceManager.GetString("My_Dynamics", SystemLanguageManager.Instance.CultureInfo);
            rbGroupDynamics.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Group_Dynamics", SystemLanguageManager.Instance.CultureInfo);
            rbBuddyTimeline.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Buddy_Time_Line", SystemLanguageManager.Instance.CultureInfo);

            lblPublishHomepage.FontWeight = FontWeights.Bold;
            lblPublishPicAndText.FontWeight = FontWeights.Regular;
            lblSendFriendsMessages.FontWeight = FontWeights.Regular;
            lblSendGroupMessages.FontWeight = FontWeights.Regular;
            //加载树
            InitRunningVmsTreeView(treeviewSendHomepageMessages);
            bool enabled = VmManager.Instance.RunningGroupIndex != -1;
            gridPublishHomepage.IsEnabled = enabled;
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
        /// 搜索主页并分享
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSubmitTask_SendHomepageMsg_Click(object sender, RoutedEventArgs e)
        {
            #region
            var targets = from item in treeviewSendHomepageMessages.ItemsSourceData.FirstOrDefault().Children
                          where item.IsChecked
                          select (int)item.Id - 1;
            if (targets.Count() == 0)
            {
                MessageDialogManager.ShowDialogAsync(SystemLanguageManager.Instance.ResourceManager.GetString("Select_Vm", SystemLanguageManager.Instance.CultureInfo));
                return;
            }

            int addTimes;
            string strAddTimes = "2";
            if (!int.TryParse(strAddTimes, out addTimes))
            {
                MessageDialogManager.ShowDialogAsync(SystemLanguageManager.Instance.ResourceManager.GetString("Invalid_Operation_Times", SystemLanguageManager.Instance.CultureInfo));
                return;
            }

            string homepageName = txtHomepageMessage.Text.Trim();
            string[] homepageNameArr = homepageName.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

            //int RandomMins = 1;
            //int RandomMaxs = 2;
            if (homepageNameArr?.Length == 0)
            {
                MessageDialogManager.ShowDialogAsync(SystemLanguageManager.Instance.ResourceManager.GetString("Group_Name", SystemLanguageManager.Instance.CultureInfo));
                return;
            }
            if (rbMyDynamics.IsChecked != true && rbGroupDynamics.IsChecked != true && rbBuddyTimeline.IsChecked != true)
            {
                MessageDialogManager.ShowDialogAsync(SystemLanguageManager.Instance.ResourceManager.GetString("Please_Choose_To_Share_The_Content_Of_The_Post", SystemLanguageManager.Instance.CultureInfo));
                return;
            }

            List<Task<bool>> tasks = new List<Task<bool>>();
            TaskSch task = new TaskSch();
            List<int> remotes = new List<int>();

            for (var i = 0; i < targets.Count(); i++)
            {

                if (rbMyDynamics.IsChecked == true)
                {
                    tasks.Add(TasksSchedule.SendHomepageAsync(targets.ElementAt(i), homepageNameArr[0]));
                }
                else if (rbGroupDynamics.IsChecked == true)
                {
                    tasks.Add(TasksSchedule.SendHomepageAsyncPosting(targets.ElementAt(i), homepageNameArr[0]));
                }
                else if (rbBuddyTimeline.IsChecked == true)
                {
                    tasks.Add(TasksSchedule.SendHomepageAsyncMessaging(targets.ElementAt(i), homepageNameArr[0]));
                }

                var lists = new JArray()
                {
                };
                var obj = new JObject()
                {{ "opernums",addTimes},{ "tasktype",(int)TaskType.SendHomepage},{ "txtmsg",""}
                };
                obj.Add("list", lists);

                int id = Guid.NewGuid().GetHashCode();
                remotes.Add(id);

                task.Remotes = $"{id}";
                task.TypeId = (int)TaskType.SendHomepage;
                task.MobileIndex = targets.ElementAt(i);
                task.Bodys = obj.ToString(Formatting.None);
                task.Status = "waiting";
                task.ResultVal = string.Empty;
                task.RepeatNums = addTimes;
                task.RandomMins = 1;
                task.RandomMaxs = 2;
                task.InputName = homepageNameArr[0];
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
            #endregion
        }

        private static async Task LaunchAndroidCommanderAsync(string device)
        {
            await Task.Run(() =>
            {
                Console.WriteLine($"begin launching {device}");
                ProcessUtils.LaunchAndroidCommander(device);
                Console.WriteLine($"launched {device}");
            });
        }

        public static async void AndroidCommander()
        {
            List<Task> tasks = new List<Task>();

            string[] devices = ProcessUtils.GetAttachedDevices();

            foreach (var device in devices)
            {
                tasks.Add(LaunchAndroidCommanderAsync(device));
            }

            await Task.WhenAll(tasks);
        }

        async Task SearchElementng(string strName)
        {
            string id = strName;
            bool clickResult = await SimulationTaskWrapper.SearchClick(0, id);
            MessageDialogManager.ShowDialogAsync($"执行结果：{clickResult}");
        }





        private void lblSendFriendsMessages_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            lblNumberOfTargetFriends.Text = SystemLanguageManager.Instance.ResourceManager.GetString("Number_Of_Friends_To_Send", SystemLanguageManager.Instance.CultureInfo);
            lblClicking_Interval.Text = SystemLanguageManager.Instance.ResourceManager.GetString("Click_Interval", SystemLanguageManager.Instance.CultureInfo);
            lblIntervalUnit.Text = SystemLanguageManager.Instance.ResourceManager.GetString("Second", SystemLanguageManager.Instance.CultureInfo);
            lblHistoryRecord.Text = SystemLanguageManager.Instance.ResourceManager.GetString("History_Record", SystemLanguageManager.Instance.CultureInfo);
            rbFromStart.Content = SystemLanguageManager.Instance.ResourceManager.GetString("From_The_Beginning", SystemLanguageManager.Instance.CultureInfo);
            rbFromLastPosition.Content = SystemLanguageManager.Instance.ResourceManager.GetString("From_Previous_Position", SystemLanguageManager.Instance.CultureInfo);
            lblEnterFriendMessage.Text = SystemLanguageManager.Instance.ResourceManager.GetString("Enter_Copywriting", SystemLanguageManager.Instance.CultureInfo);

            btnSelectPics2.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Select_Picture", SystemLanguageManager.Instance.CultureInfo);
            btnDeletePics2.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Delete_Picture", SystemLanguageManager.Instance.CultureInfo);


            lblAddPicsTips.Text = SystemLanguageManager.Instance.ResourceManager.GetString("Select_Picture_Tips", SystemLanguageManager.Instance.CultureInfo);
            btnSubmitTask_SendFriendMsg.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Submit_Task", SystemLanguageManager.Instance.CultureInfo);

            lblPublishHomepage.FontWeight = FontWeights.Regular;
            lblPublishPicAndText.FontWeight = FontWeights.Regular;
            lblSendFriendsMessages.FontWeight = FontWeights.Bold;
            lblSendGroupMessages.FontWeight = FontWeights.Regular;

            InitRunningVmsTreeView(treeviewSendFriendsMessages);
            bool enabled = VmManager.Instance.RunningGroupIndex != -1;
            gridSendFriendsMessage.IsEnabled = enabled;
        }

        private void lblSendGroupMessages_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            tbNumberOfPicsPerPhoneTips.Text = SystemLanguageManager.Instance.ResourceManager.GetString("Number_Of_Pics_Per_Phone", SystemLanguageManager.Instance.CultureInfo);
            //cbNumberOfPicsPerPhone.Items.Clear();
            //cbNumberOfPicsPerPhone.Items.Add(string.Format(SystemLanguageManager.Instance.ResourceManager.GetString("Number_Of_Pics_Per_Phone", SystemLanguageManager.Instance.CultureInfo), 1));
            //cbNumberOfPicsPerPhone.Items.Add(string.Format(SystemLanguageManager.Instance.ResourceManager.GetString("Number_Of_Pics_Per_Phone", SystemLanguageManager.Instance.CultureInfo), 2));
            //cbNumberOfPicsPerPhone.Items.Add(string.Format(SystemLanguageManager.Instance.ResourceManager.GetString("Number_Of_Pics_Per_Phone", SystemLanguageManager.Instance.CultureInfo), 3));
            //cbNumberOfPicsPerPhone.Items.Add(string.Format(SystemLanguageManager.Instance.ResourceManager.GetString("Number_Of_Pics_Per_Phone", SystemLanguageManager.Instance.CultureInfo), 4));
            //cbNumberOfPicsPerPhone.Items.Add(string.Format(SystemLanguageManager.Instance.ResourceManager.GetString("Number_Of_Pics_Per_Phone", SystemLanguageManager.Instance.CultureInfo), 5));
            //cbNumberOfPicsPerPhone.Items.Add(string.Format(SystemLanguageManager.Instance.ResourceManager.GetString("Number_Of_Pics_Per_Phone", SystemLanguageManager.Instance.CultureInfo), 6));
            //cbNumberOfPicsPerPhone.Items.Add(string.Format(SystemLanguageManager.Instance.ResourceManager.GetString("Number_Of_Pics_Per_Phone", SystemLanguageManager.Instance.CultureInfo), 7));
            //cbNumberOfPicsPerPhone.Items.Add(string.Format(SystemLanguageManager.Instance.ResourceManager.GetString("Number_Of_Pics_Per_Phone", SystemLanguageManager.Instance.CultureInfo), 8));

            cbNumberOfPicsPerPhone.SelectedIndex = 0;

            lblContentTitle.Text = SystemLanguageManager.Instance.ResourceManager.GetString("Content_Title", SystemLanguageManager.Instance.CultureInfo);
            lblPrice.Text = SystemLanguageManager.Instance.ResourceManager.GetString("Price", SystemLanguageManager.Instance.CultureInfo);
            lblRelatedNames.Text = SystemLanguageManager.Instance.ResourceManager.GetString("Group_Name_Praise", SystemLanguageManager.Instance.CultureInfo);
            lblAddress.Text = SystemLanguageManager.Instance.ResourceManager.GetString("Address", SystemLanguageManager.Instance.CultureInfo);
            lblRelatedNameTips.Text = SystemLanguageManager.Instance.ResourceManager.GetString("One_Keyword_Per_Line", SystemLanguageManager.Instance.CultureInfo);
            lblAddressTips.Text = SystemLanguageManager.Instance.ResourceManager.GetString("One_Address_Per_Line", SystemLanguageManager.Instance.CultureInfo);
            lblDetailContent.Text = SystemLanguageManager.Instance.ResourceManager.GetString("Detail_Content", SystemLanguageManager.Instance.CultureInfo);
            btnSelectPics3.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Select_Picture", SystemLanguageManager.Instance.CultureInfo);
            btnDeletePics3.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Delete_Picture", SystemLanguageManager.Instance.CultureInfo);
            lblRelatedPics.Text = SystemLanguageManager.Instance.ResourceManager.GetString("Picture", SystemLanguageManager.Instance.CultureInfo);
            btnSubmitTask_SendGroupMsg.Content = SystemLanguageManager.Instance.ResourceManager.GetString("Submit_Task", SystemLanguageManager.Instance.CultureInfo);


            lblPublishHomepage.FontWeight = FontWeights.Regular;
            lblPublishPicAndText.FontWeight = FontWeights.Regular;
            lblSendFriendsMessages.FontWeight = FontWeights.Regular;
            lblSendGroupMessages.FontWeight = FontWeights.Bold;

            InitRunningVmsTreeView(treeviewSendGroupMessage);
            bool enabled = VmManager.Instance.RunningGroupIndex != -1;
            gridSendGroupMessages.IsEnabled = enabled;

        }

        private void btnSubmitTask_SendFriendMsg_Click(object sender, RoutedEventArgs e)
        {
            var targets = from item in treeviewSendFriendsMessages.ItemsSourceData.FirstOrDefault().Children
                          where item.IsChecked
                          select (int)item.Id - 1;

            if (targets.Count() == 0)
            {
                MessageDialogManager.ShowDialogAsync(SystemLanguageManager.Instance.ResourceManager.GetString("Select_Vm", SystemLanguageManager.Instance.CultureInfo));

                return;
            }


            int friendNum;
            string str_friendNum = tbNumberOfTargetFriends.Text.Trim();

            if (!int.TryParse(str_friendNum, out friendNum))
            {
                MessageDialogManager.ShowDialogAsync(SystemLanguageManager.Instance.ResourceManager.GetString("Invalid_Friend_Num", SystemLanguageManager.Instance.CultureInfo));
                return;
            }

            string text = tbFriendMessage.Text.Trim();


            var pictures = from pic in panelPics2.Controls.Cast<System.Windows.Forms.Panel>()
                           select ((System.Windows.Forms.PictureBox)pic.Controls[1]).ImageLocation;

            string[] paths = pictures.ToArray();


            if (string.IsNullOrEmpty(text) && paths.Length == 0)
            {
                MessageDialogManager.ShowDialogAsync(SystemLanguageManager.Instance.ResourceManager.GetString("Enter_Copywriting", SystemLanguageManager.Instance.CultureInfo));

                return;
            }

            //string dir = DateTime.Now.ToString("yyyyMMddHHmmssffff");

            //List<Task<bool>> tasks = new List<Task<bool>>();

            TaskSch taskSch = new TaskSch();

            //List<int> remotes = new List<int>();

            //Assembly assembly = Assembly.GetExecutingAssembly();

            //DirectoryInfo rootDir = Directory.GetParent(Path.Combine(assembly.Location));

            //string dateFolder = DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_ffff");

            //string imagesCacheDir = Path.Combine(rootDir.FullName, "cache", "images", dateFolder);

            //if (!Directory.Exists(imagesCacheDir))
            //{
            //    Directory.CreateDirectory(imagesCacheDir);
            //}

            //List<string> pics = new List<string>();

            foreach (var target in targets)
            {
                var lists = new JArray();



                ////ProcessUtils.DeleteAllPictures(DeviceConnectionManager.Instance.GetDeviceNameByMobileIndex(target));

                for (int i = 0; i < paths.Length; i++)
                {
                    //    // cache pictures in a temp directory, so that when the task is being executed, it will grab pics from the temp directory

                    //    string fileName = $"{i + 1}{Path.GetExtension(paths[i])}";
                    //    //string dstFilePath = Path.Combine(imagesCacheDir, fileName);
                    //    //File.Copy(paths[i], dstFilePath, true);


                    lists.Add(paths[i]);


                    //    // just for test
                    //    string picPath = $"/sdcard/Pictures/a{i + 1}{Path.GetExtension(paths[i])}";

                    //    pics.Add(picPath);

                    //    string mobileId = DeviceConnectionManager.Instance.GetDeviceNameByMobileIndex(target);
                    //    ProcessUtils.PushFileToVm(mobileId, paths[i], picPath);
                }


                int position = _startFromScratch ? 0 : TraceBLL.GetTaskTracePosition(new TaskTrace() { MobileIndex = target, TypeId = (int)TaskType.SendFriendsMessage });


                var obj = new JObject() {
                    {"tasktype",(int)TaskType.SendFriendsMessage },
                    { "txtmsg",text},
                    { "friendnums",friendNum},
                    {"position", position},
                };

                obj.Add("list", lists);

                int id = Guid.NewGuid().GetHashCode();

                //tasks.Add(TasksSchedule.SendFrinedsMsgLite(target, obj.ToString(Formatting.None)));

                //remotes.Add(id);

                taskSch.Remotes = $"{id}";
                taskSch.TypeId = (int)TaskType.SendFriendsMessage;
                taskSch.MobileIndex = target;
                taskSch.Bodys = obj.ToString(Formatting.None);
                taskSch.Status = "waiting";
                taskSch.ResultVal = string.Empty;
                taskSch.SlideNums = 0;
                taskSch.FriendNums = friendNum;
                taskSch.RepeatNums = 0;
                taskSch.RandomMins = 1;
                taskSch.RandomMaxs = 2;
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

        private void btnSelectPics2_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;
            openFileDialog.Title = SystemLanguageManager.Instance.ResourceManager.GetString("Select_Picture", SystemLanguageManager.Instance.CultureInfo);
            openFileDialog.Filter = SystemLanguageManager.Instance.ResourceManager.GetString("Picture_Filter", SystemLanguageManager.Instance.CultureInfo);

            bool? result = openFileDialog.ShowDialog();

            if (result.HasValue && result.Value)
            {
                if (panelPics2.Controls.Count > 0)
                {
                    panelPics2.Controls.Clear();
                }

                string[] addedPicPath = openFileDialog.FileNames;

                int startingIndex = panelPics2.Controls.Count;
                double total = panelPics2.Controls.Count + addedPicPath.Length;
                double columnCapacity = TaskManager.ColumnCapacity;

                int totalRow = (int)Math.Ceiling(total / columnCapacity);

                for (int row = 0; row < totalRow; row++)
                {
                    for (int column = 0; column < TaskManager.ColumnCapacity; column++)
                    {
                        int picturePathIndex = row * TaskManager.ColumnCapacity + column;

                        if (picturePathIndex < total && picturePathIndex >= startingIndex)
                        {

                            System.Windows.Forms.Panel panel = new System.Windows.Forms.Panel()
                            {
                                Name = $"panel{picturePathIndex}",
                                BackColor = System.Drawing.Color.Gray,
                                Width = 300,
                                Height = 300,
                                Location = new System.Drawing.Point()
                                {
                                    X = 5 + column * 305,
                                    Y = 5 + row * 305,
                                },
                            };

                            System.Windows.Forms.CheckBox checkBox = new System.Windows.Forms.CheckBox()
                            {
                                Checked = false,
                                Location = new System.Drawing.Point()
                                {
                                    X = 0,
                                    Y = 0,
                                },
                                Width = 30,
                                Height = 30
                            };

                            System.Windows.Forms.PictureBox pictureBox = new System.Windows.Forms.PictureBox()
                            {
                                BackColor = System.Drawing.Color.Gray,
                                Width = 300,
                                Height = 300,
                                ImageLocation = addedPicPath[picturePathIndex - startingIndex],
                                Location = new System.Drawing.Point()
                                {
                                    X = 0,
                                    Y = 0,
                                },
                                SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom,
                            };

                            panel.Controls.Add(checkBox);
                            panel.Controls.Add(pictureBox);

                            panelPics2.Controls.Add(panel);
                        }
                    }
                }
            }

        }

        private void btnDeletePics2_Click(object sender, RoutedEventArgs e)
        {
            var panels = panelPics2.Controls.Cast<System.Windows.Forms.Panel>().Where(p => p.Controls[0] is System.Windows.Forms.CheckBox && ((System.Windows.Forms.CheckBox)p.Controls[0]).Checked);

            if (panels.Count() == 0)
            {
                return;
            }

            List<string> tobeDeletedKeys = new List<string>();

            foreach (var p in panels)
            {
                tobeDeletedKeys.Add(p.Name);
            }

            foreach (var key in tobeDeletedKeys)
            {
                panelPics2.Controls.RemoveByKey(key);
            }

            double total = panelPics2.Controls.Count;
            double columnCapacity = TaskManager.ColumnCapacity;

            int totalRow = (int)Math.Ceiling(total / columnCapacity);

            for (int row = 0; row < totalRow; row++)
            {
                for (int column = 0; column < TaskManager.ColumnCapacity; column++)
                {
                    int picturePathIndex = row * TaskManager.ColumnCapacity + column;

                    if (picturePathIndex < total)
                    {
                        System.Windows.Forms.Panel panel = panelPics2.Controls[picturePathIndex] as System.Windows.Forms.Panel;

                        panel.Location = new System.Drawing.Point()
                        {
                            X = 5 + column * 305,
                            Y = 5 + row * 305,
                        };
                    }
                }
            }


        }

        private void btnSelectPics3_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;
            openFileDialog.Title = SystemLanguageManager.Instance.ResourceManager.GetString("Select_Picture", SystemLanguageManager.Instance.CultureInfo);
            openFileDialog.Filter = SystemLanguageManager.Instance.ResourceManager.GetString("Picture_Filter", SystemLanguageManager.Instance.CultureInfo);

            bool? result = openFileDialog.ShowDialog();

            if (result.HasValue && result.Value)
            {
                //if (panelPics3.Controls.Count > 0)
                //{
                //    panelPics3.Controls.Clear();
                //}

                string[] addedPicPath = openFileDialog.FileNames;

                int startingIndex = panelPics3.Controls.Count;
                double total = panelPics3.Controls.Count + addedPicPath.Length;
                double columnCapacity = TaskManager.ColumnCapacity;

                int totalRow = (int)Math.Ceiling(total / columnCapacity);

                for (int row = 0; row < totalRow; row++)
                {
                    for (int column = 0; column < TaskManager.ColumnCapacity; column++)
                    {
                        int picturePathIndex = row * TaskManager.ColumnCapacity + column;

                        if (picturePathIndex < total && picturePathIndex >= startingIndex)
                        {

                            System.Windows.Forms.Panel panel = new System.Windows.Forms.Panel()
                            {
                                Name = $"panel{picturePathIndex}",
                                BackColor = System.Drawing.Color.Gray,
                                Width = 300,
                                Height = 300,
                                Location = new System.Drawing.Point()
                                {
                                    X = 5 + column * 305,
                                    Y = 5 + row * 305,
                                },
                            };

                            System.Windows.Forms.CheckBox checkBox = new System.Windows.Forms.CheckBox()
                            {
                                Checked = false,
                                Location = new System.Drawing.Point()
                                {
                                    X = 0,
                                    Y = 0,
                                },
                                Width = 30,
                                Height = 30
                            };

                            System.Windows.Forms.PictureBox pictureBox = new System.Windows.Forms.PictureBox()
                            {
                                BackColor = System.Drawing.Color.Gray,
                                Width = 300,
                                Height = 300,
                                ImageLocation = addedPicPath[picturePathIndex - startingIndex],
                                Location = new System.Drawing.Point()
                                {
                                    X = 0,
                                    Y = 0,
                                },
                                SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom,
                            };

                            panel.Controls.Add(checkBox);
                            panel.Controls.Add(pictureBox);

                            panelPics3.Controls.Add(panel);
                        }
                    }
                }
            }

        }

        private void btnDeletePics3_Click(object sender, RoutedEventArgs e)
        {
            var panels = panelPics3.Controls.Cast<System.Windows.Forms.Panel>().Where(p => p.Controls[0] is System.Windows.Forms.CheckBox && ((System.Windows.Forms.CheckBox)p.Controls[0]).Checked);

            if (panels.Count() == 0)
            {
                return;
            }

            List<string> tobeDeletedKeys = new List<string>();

            foreach (var p in panels)
            {
                tobeDeletedKeys.Add(p.Name);
            }

            foreach (var key in tobeDeletedKeys)
            {
                panelPics3.Controls.RemoveByKey(key);
            }

            double total = panelPics3.Controls.Count;
            double columnCapacity = TaskManager.ColumnCapacity;

            int totalRow = (int)Math.Ceiling(total / columnCapacity);

            for (int row = 0; row < totalRow; row++)
            {
                for (int column = 0; column < TaskManager.ColumnCapacity; column++)
                {
                    int picturePathIndex = row * TaskManager.ColumnCapacity + column;

                    if (picturePathIndex < total)
                    {
                        System.Windows.Forms.Panel panel = panelPics3.Controls[picturePathIndex] as System.Windows.Forms.Panel;

                        panel.Location = new System.Drawing.Point()
                        {
                            X = 5 + column * 305,
                            Y = 5 + row * 305,
                        };
                    }
                }
            }

        }

        private void btnSubmitTask_SendGroupMsg_Click(object sender, RoutedEventArgs e)
        {
            var targets = from item in treeviewSendGroupMessage.ItemsSourceData.FirstOrDefault().Children
                          where item.IsChecked
                          select (int)item.Id - 1;

            if (targets.Count() == 0)
            {
                MessageDialogManager.ShowDialogAsync(SystemLanguageManager.Instance.ResourceManager.GetString("Select_Vm", SystemLanguageManager.Instance.CultureInfo));

                return;
            }

            string detail = tbDetailContent.Text.Trim();


            var pictures = from pic in panelPics3.Controls.Cast<System.Windows.Forms.Panel>()
                           select ((System.Windows.Forms.PictureBox)pic.Controls[1]).ImageLocation;

            string[] paths = pictures.ToArray();


            if (string.IsNullOrEmpty(detail) && paths.Length == 0)
            {
                MessageDialogManager.ShowDialogAsync(SystemLanguageManager.Instance.ResourceManager.GetString("Enter_Copywriting", SystemLanguageManager.Instance.CultureInfo));

                return;
            }


            string title = tbContentTitle.Text.Trim();
            string sPrice = tbPrice.Text.Trim();
            string sGroups = tbRelatedNames.Text.Trim();
            string sAddresses = tbAddress.Text.Trim();

            if (string.IsNullOrEmpty(title))
            {
                MessageDialogManager.ShowDialogAsync(SystemLanguageManager.Instance.ResourceManager.GetString("NoTitle", SystemLanguageManager.Instance.CultureInfo));
                return;
            }

            if (string.IsNullOrEmpty(sPrice))
            {
                MessageDialogManager.ShowDialogAsync(SystemLanguageManager.Instance.ResourceManager.GetString("NoPrice", SystemLanguageManager.Instance.CultureInfo));
                return;
            }

            int price;
            if (!int.TryParse(sPrice, out price))
            {
                MessageDialogManager.ShowDialogAsync(SystemLanguageManager.Instance.ResourceManager.GetString("InvalidPrice", SystemLanguageManager.Instance.CultureInfo));
                return;
            }

            if (string.IsNullOrEmpty(sGroups))
            {
                MessageDialogManager.ShowDialogAsync(SystemLanguageManager.Instance.ResourceManager.GetString("NoGroupNames", SystemLanguageManager.Instance.CultureInfo));
                return;
            }

            if (string.IsNullOrEmpty(sAddresses))
            {
                MessageDialogManager.ShowDialogAsync(SystemLanguageManager.Instance.ResourceManager.GetString("NoAddress", SystemLanguageManager.Instance.CultureInfo));
                return;
            }

            string[] addresses = sAddresses.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            if (addresses?.Length < targets.Count())
            {
                MessageDialogManager.ShowDialogAsync(SystemLanguageManager.Instance.ResourceManager.GetString("NotEnoughAddresses", SystemLanguageManager.Instance.CultureInfo));
                return;
            }

            string[] groups = sGroups.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            if (groups?.Length < targets.Count())
            {
                MessageDialogManager.ShowDialogAsync(SystemLanguageManager.Instance.ResourceManager.GetString("NotEnoughGroups", SystemLanguageManager.Instance.CultureInfo));
                return;
            }

            int numberOfPicsPerPhone = int.Parse(cbNumberOfPicsPerPhone.Text);

            if (targets.Count() * numberOfPicsPerPhone > paths.Length)
            {
                MessageDialogManager.ShowDialogAsync(SystemLanguageManager.Instance.ResourceManager.GetString("NotEnoughPictures", SystemLanguageManager.Instance.CultureInfo));
                return;
            }

            TaskSch taskSch = new TaskSch();

            for (int mobile = 0; mobile < targets.Count(); mobile++)
            {
                var list = new JArray();

                for (int i = 0; i < numberOfPicsPerPhone; i++)
                {
                    list.Add(mobile * numberOfPicsPerPhone + i);
                }

                var obj = new JObject() {
                    { "tasktype",(int)TaskType.SendGroupMessage },
                    { "title",title},
                    { "price",price},
                    {"group_name", groups[mobile]},
                    {"address",addresses[mobile] },
                    { "detail",detail},
                };

                obj.Add("list", list);

                int id = Guid.NewGuid().GetHashCode();

                taskSch.Remotes = $"{id}";
                taskSch.TypeId = (int)TaskType.SendGroupMessage;
                taskSch.MobileIndex = targets.ElementAt(mobile);
                taskSch.Bodys = obj.ToString(Formatting.None);
                taskSch.Status = "waiting";
                taskSch.ResultVal = string.Empty;
                taskSch.SlideNums = 0;
                taskSch.FriendNums = 0;
                taskSch.RepeatNums = 0;
                taskSch.RandomMins = 1;
                taskSch.RandomMaxs = 2;
                taskSch.InputName = string.Empty;
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
        }

        private bool _startFromScratch = false;
        private void rbFromStart_Checked(object sender, RoutedEventArgs e)
        {
            _startFromScratch = true;
        }

        private void rbFromLastPosition_Checked(object sender, RoutedEventArgs e)
        {
            _startFromScratch = false;
        }
    }
}
