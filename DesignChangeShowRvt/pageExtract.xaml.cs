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
using System.Windows.Shapes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.IO;

namespace DesignChangeShowRvt
{
    /// <summary>
    /// pageExtract.xaml 的交互逻辑
    /// </summary>
    public partial class pageExtract : Window
    {
        //外部事件
        ExternalEvent ee = null;
        ExternalCommand cmd = null;



        public string selectElementIds { get; set; }
        MainWindow mainWin = null;

        public pageExtract(string eleIds,MainWindow mWin)
        {
            InitializeComponent();
            selectElementIds = eleIds;

            txtpageSelectEles.Text = eleIds;
            if (cmd == null)
            {
                cmd = new ExternalCommand();
            }
            if (ee == null)
            {
                ee = ExternalEvent.Create(cmd);
            }

            mainWin = mWin;
            mainWin.page = this;
        }

        //载入窗口时把主窗口的元素信息载入
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.txtpageSelectEles.Text = mainWin.txtMainSelectEles.Text;
        }


        //按继续时，触发外部事件并把窗口变小
        private void btnGoon_Click(object sender, RoutedEventArgs e)
        {
            ee.Raise();
            this.Height = 45;
        }


        //按确定时把元素信息转入主窗口里
        private void btnSure_Click(object sender, RoutedEventArgs e)
        {
            mainWin.txtMainSelectEles.Text = this.txtpageSelectEles.Text;
            mainWin.Show();
        }

        //关闭窗口时把主窗口显示
        private void Window_Closed(object sender, EventArgs e)
        {
            mainWin.Show();
        }

        
        private void Window_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            
        }
        //单击窗口时重载信息，窗口尺度恢复正常
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.txtpageSelectEles.Text = File.ReadAllText(@"C:\selectElementIds.txt");
            this.Height = 281;
        }
    }
}
