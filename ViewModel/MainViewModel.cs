using HandyControl.Tools.Command;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;

namespace WXWorkGroupSendMessage.ViewModel
{
    public class MainViewModel : ObservableValidator
    {
        #region Fields

        private string _sendInputText;
        private bool _whetherItIsMassModel;
        private string _replayUserName;
        private bool _isSendPic;
        private bool _whetherToSendPicture;
        private bool _isMorePicCB;
        private string _path;


        const int WM_CLOSE = 0x0010;
        //SendMessage参数
        private const int WM_KEYDOWN = 0X100;
        private const int WM_KEYUP = 0X101;
        private const int WM_SYSCHAR = 0X106;
        private const int WM_SYSKEYUP = 0X105;
        private const int WM_SYSKEYDOWN = 0X104;
        private const int WM_CHAR = 0X102;
        #endregion

        #region Commands


        /// <summary>
        /// 开始发送消息
        /// </summary>
        public IRelayCommand StartSendMsgCommand { get; private set; }

        /// <summary>
        /// 选择发送人的文件路径
        /// </summary>
        public IRelayCommand SelectSendUserFile { get; private set; }


        #endregion

        #region Constorctors
        public MainViewModel()
        {
            PropertyChanged += (s, e) =>
            {
                //通知命令能否执行
                StartSendMsgCommand?.NotifyCanExecuteChanged();
            };
            StartSendMsgCommand = new AsyncRelayCommand(OnStartSendMsgCommand, CanStartSendMsgCommand);
            SelectSendUserFile = new AsyncRelayCommand(OnSelectSendUserFile);
        }

        #endregion

        #region Properties

        /// <summary>
        /// 发送文本
        /// </summary>
        public string SendInputText
        {
            get => _sendInputText;
            set => SetProperty(ref _sendInputText, value);
        }


        /// <summary>
        /// 是否是群发模式
        /// </summary>
        public bool WhetherItIsMassModel
        {
            get => _whetherItIsMassModel;
            set => SetProperty(ref _whetherItIsMassModel, value);
        }


        /// <summary>
        /// 替换的模板名称
        /// </summary>
        [Required(AllowEmptyStrings = false, ErrorMessage = "替换名称不能为空")]
        public string ReplayUserName
        {
            get => _replayUserName;
            set
            {
                SetProperty(ref _replayUserName, value.Trim(), true);
            }
        }


        /// <summary>
        /// 是否发送图片
        /// </summary>
        public bool IsSendPic
        {
            get => _isSendPic;
            set => SetProperty(ref _isSendPic, value);
        }

        /// <summary>
        /// 图片是否分开发送
        /// </summary>
        public bool WhetherToSendPicture
        {
            get => _whetherToSendPicture;
            set => SetProperty(ref _whetherToSendPicture, value);
        }

        /// <summary>
        /// 是否是多张照片
        /// </summary>
        public bool IsMorePicCB
        {
            get => _isMorePicCB;
            set => SetProperty(ref _isMorePicCB, value);
        }

        /// <summary>
        /// 发送人文件的path
        /// </summary>
        [Required(AllowEmptyStrings = false, ErrorMessage = "发送人文件不能为空")]
        public string Path
        {
            get => _path;
            set
            {
                SetProperty(ref _path, value.Trim(), true);
            }
        }



        #endregion

        #region Methods

        private bool CanStartSendMsgCommand()
        {
            if (string.IsNullOrEmpty(SendInputText) || string.IsNullOrEmpty(Path) || string.IsNullOrEmpty(ReplayUserName))
                return false;

            return true;
        }

        /// <summary>
        /// 开始发送消息
        /// </summary>
        /// <returns></returns>
        private Task OnStartSendMsgCommand()
        {
            //登录
            ValidateAllProperties();
            if (HasErrors)
            {
                //提示
                return null;
            }

            CloseWXWorkBlackWindow();

            IntPtr hwnd = Win32API.FindWindow("WeWorkWindow", "企业微信");
            Win32API.SetActiveWindow(hwnd);
            Win32API.SetForegroundWindow(hwnd);
            Win32API.ShowWindow(hwnd, 1);
            IntPtr CHwnd = Win32API.FindWindow("SearchTabWnd", "");
            if (CHwnd != IntPtr.Zero)
            {
                Win32API.SendMessage(CHwnd, WM_CLOSE, 0, 0);
                Thread.Sleep(100);
            }
            IntPtr SelectUserNoNameHwnd = Win32API.FindWindow("weWorkSelectUser", "");
            if (SelectUserNoNameHwnd != IntPtr.Zero)
            {
                Win32API.SendMessage(SelectUserNoNameHwnd, WM_CLOSE, 0, 0);
                Thread.Sleep(100);
            }


            //------分割线---------

            bool isSendUserFile = File.Exists(Path);
            if (isSendUserFile)
            {
                NavigateWXSearchBox();
                string text = System.IO.File.ReadAllText(Path);
                string[] sendTextArray = text.Split('|');

                foreach (var sendUserName in sendTextArray)
                {
                    var tempSendText = text.Replace(ReplayUserName, sendUserName);
                    SendWXWorkMsg(sendUserName, tempSendText, hwnd);
                }

            }

            return null;
        }



        /// <summary>
        /// 兼容第一次启动企业微信的时候出现的黑窗口
        /// </summary>
        private static void CloseWXWorkBlackWindow()
        {
            for (int i = 0; i < 2; i++)
            {
                IntPtr wndHandle = Win32API.FindWindow("WeWorkWindow", "企业微信");
                Win32API.SetForegroundWindow(wndHandle);
                Win32API.SetActiveWindow(wndHandle);
                Win32API.ShowWindow(wndHandle, 1);
                Win32API.SendMessage(wndHandle, WM_CLOSE, 0, 0);
            }
        }

        private static  void NavigateWXSearchBox()
        {
            SendKeys.SendWait("%");
            Thread.Sleep(200);
            SendKeys.SendWait("%");
        }

        public void SendWXWorkMsg(string sendUserName,string SendContent,IntPtr wxHwnd)
        {
            IntPtr CHwnd = Win32API.FindWindow("SearchTabWnd", "");
            if (CHwnd != IntPtr.Zero)
            {
                Win32API.SendMessage(CHwnd, WM_CLOSE, 0, 0);
                Thread.Sleep(100);
            }

            IntPtr SelectUserHwnd = Win32API.FindWindow("weWorkSelectUser", "发起群聊");
            if (SelectUserHwnd != IntPtr.Zero)
            {
                Win32API.SendMessage(SelectUserHwnd, WM_CLOSE, 0, 0);
                Thread.Sleep(100);
            }
            //------分割线---------

            NavigateWXSearchBox();
            SendKeys.SendWait("^A");
            Thread.Sleep(100);
            SendKeys.SendWait("{BACKSPACE}");
            Thread.Sleep(200);
            SendKeys.SendWait($"{sendUserName}");
            Thread.Sleep(1000);
            SendKeys.SendWait("~");
            Thread.Sleep(500);
            SendKeys.SendWait("^A");
            Thread.Sleep(100);
            SendKeys.SendWait("{BACKSPACE}");
            Thread.Sleep(200);


            string content = SendInputText;
            string[] splitString = content.Split(new string[] { "\r\n" }, StringSplitOptions.None);
            if (splitString.Length > 0)
            {
                foreach (var itemStr in splitString)
                {
                    string tempItemStr = itemStr.Replace(ReplayUserName, sendUserName);
                    if (Regex.IsMatch(tempItemStr, StringExtension.PicPathRegexStr))
                    {
                        string[] contents = tempItemStr.RegexSplitPicPath();
                        foreach (string item in contents)
                        {
                            if (File.Exists(item) && IsSendPic)
                            {
                                //如果是要单独发送图片，就先把上一段文字发送出去
                                if (WhetherToSendPicture)
                                {
                                    Thread.Sleep(100);
                                    SendKeys.SendWait("~");
                                }
                                //粘贴图片
                                Image img = Image.FromFile(item);
                                System.Windows.Forms.Clipboard.Clear();
                                Clipboard.SetImage(img);
                                Thread.Sleep(500);
                                SendKeys.SendWait("^v");
                                //如果是要单独发送图片，就再把图片也发送出去
                                if (WhetherToSendPicture)
                                {
                                    Thread.Sleep(100);
                                    SendKeys.SendWait("~");
                                }
                            }
                            else
                            {
                                string strConv = ConvertSendString(item);
                                SendKeys.SendWait($"{strConv}");
                                Thread.Sleep(100);
                                SendKeys.SendWait("+~");
                            }
                        }
                    }
                    else
                    {
                        if (WhetherItIsMassModel && tempItemStr.Contains("@所有人"))
                        {
                            string[] sendAllUserMsgArray = new string[0];
                            if (tempItemStr.Contains("@所有人"))
                            {
                                sendAllUserMsgArray = Regex.Split(tempItemStr, "@所有人");
                            }
                            else if (tempItemStr.Contains("@所有人 "))
                            {
                                sendAllUserMsgArray = Regex.Split(tempItemStr, "@所有人 ");
                            }

                            SendKeys.SendWait(sendAllUserMsgArray[0]);
                            SendKeys.SendWait("@所有人");
                            Thread.Sleep(500);
                            SendKeys.SendWait("~");
                            tempItemStr = sendAllUserMsgArray[1];
                        }
                        string strConv = ConvertSendString(tempItemStr);
                        SendKeys.SendWait($"{strConv}");
                        Thread.Sleep(100);
                        SendKeys.SendWait("+~");
                    }
                    
                }
            }
            SendKeys.SendWait("~");
        }

        public static string ConvertSendString(string content)
        {
            //判断是否有emoji表情
            if (Regex.IsMatch(content, @"\p{Cs}"))
            {
                content = Regex.Unescape(content);
            }
            //这是将sendSeys中的关键字做兼容
            content = content.Replace("{", "{{}");
            content = content.Replace("}", "{}}");
            content = content.Replace("+", "{+}");
            content = content.Replace("^", "{^}");
            content = content.Replace("%", "{%}");
            content = content.Replace("~", "{~}");
            content = content.Replace("(", "{(}");
            content = content.Replace(")", "{)}");
            return content;
        }


        private Task OnSelectSendUserFile()
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog()
            {
                Filter = "TXT文件 (*.txt)|*.txt"
            };
            var result = openFileDialog.ShowDialog();
            if (result == true)
            {
                Path = openFileDialog.FileName;
            }
            return null;
        }
        #endregion
    }
}
