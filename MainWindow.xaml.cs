using HandyControl.Controls;
using HandyControl.Interactivity;
using System;
using System.Collections.Generic;
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
using WXWorkGroupSendMessage.ViewModel;

namespace WXWorkGroupSendMessage
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        #region Fields

        /// <summary>
        /// 临时储存图片的路径
        /// 为了获取每一次点击删除图片的时候，删除的图片路径是什么
        /// </summary>
        private string _tempImagePath;


        #endregion


        #region Constorctor
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new MainViewModel();
        }

        #endregion

        #region Methods

        /// <summary>
        /// 点击空白处，取消焦点
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            LostFocusBtn.Focus();
        }


        private void ImageSelector_ImageSelected(object sender, RoutedEventArgs e)
        {
            //SendTemplateTB.Text += $@"[请勿删除-{{{(sender as ImageSelector).Uri.LocalPath}}}-请勿删除]";
            SendTemplateTB.Text += (sender as ImageSelector).Uri.LocalPath.PicStringPlaceholder();
        }

        private void ImageSelector_ImageUnselected(object sender, RoutedEventArgs e)
        {
            ImageSelector s = (sender as ImageSelector);
            SendTemplateTB.Text = SendTemplateTB.Text.Replace(_tempImagePath.PicStringPlaceholder(), "");
        }

        private void ImageSelector_MorePic_ImageSelected(object sender, RoutedEventArgs e)
        {

            SendTemplateTB.Text += (sender as ImageSelector).Uri.LocalPath.PicStringPlaceholder();

            ImageSelector imageSelector = new ImageSelector();
            imageSelector.ImageSelected += ImageSelector_MorePic_ImageSelected;
            imageSelector.ImageUnselected += ImageSelector_MorePic_ImageUnselected;
            imageSelector.PreviewMouseDown += ImageSelector_PreviewMouseDown;
            MorePicWP.Children.Add(imageSelector);
        }


        private void ImageSelector_MorePic_ImageUnselected(object sender, RoutedEventArgs e)
        {
            if (MorePicWP.Children.Count > 1)
            {
                int controlIndex = MorePicWP.Children.IndexOf((UIElement)sender);

                //只要删除的不是多图片中的第一个的时候，初始化的值就应该是1
                int findIndexOfCount = controlIndex > 0 ? 1 : 0;

                //查找是否有删除的图片选择器一样的图片路径
                //目的：为了避免删除重复的路径
                for (int i = 0; i < controlIndex; i++)
                {
                    if ((MorePicWP.Children[i] as ImageSelector).Uri.LocalPath == _tempImagePath)
                    {
                        findIndexOfCount++;
                    }
                }


                int index = SendTemplateTB.Text.IndexOf(_tempImagePath.PicStringPlaceholder(), findIndexOfCount);

                SendTemplateTB.Text = SendTemplateTB.Text.Remove(index, _tempImagePath.PicStringPlaceholder().Length);

                MorePicWP.Children.Remove((UIElement)sender);
            }
            else
            {
                SendTemplateTB.Text = SendTemplateTB.Text.Replace(_tempImagePath.PicStringPlaceholder(), "");
            }
        }

        private void ImageSelector_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            StoreTempImagePath(sender as ImageSelector);
        }

      
        private void IsMorePicCB_Checked(object sender, RoutedEventArgs e)
        {
            //RaiseEvent(new RoutedEventArgs(ImageSelector.ImageUnselectedEvent, SingleImageSelector));
            ClearImageSelectorValue(SingleImageSelector);
        }

        private void IsMorePicCB_Unchecked(object sender, RoutedEventArgs e)
        {
            while ((MorePicWP.Children[0] as ImageSelector).HasValue)
            {
                ClearImageSelectorValue((ImageSelector)MorePicWP.Children[0]);
            }
        }

        /// <summary>
        /// 清除图片选择器的值
        /// </summary>
        /// <param name="imageSelector"></param>
        private void ClearImageSelectorValue(ImageSelector imageSelector)
        {
            if (imageSelector.HasValue)
            {
                StoreTempImagePath(imageSelector);
                ControlCommands.Switch.Execute(null, imageSelector);
            }
        }


        private void CheckBox_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((sender as CheckBox).IsChecked == true)
            {
                (sender as CheckBox).IsChecked = false;
            }
        }

        private void SingleImageSelector_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((sender as ImageSelector).HasValue)
            {
                ClearImageSelectorValue(sender as ImageSelector);
            }
        }
        private void StoreTempImagePath(ImageSelector imageSelector)
        {
            _tempImagePath = imageSelector.Uri?.LocalPath;
        }
        

        #endregion

    }
}
