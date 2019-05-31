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
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Microsoft.Win32;
using Visibility = System.Windows.Visibility;

namespace DesignChangeShowRvt
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        //界面编辑状态位
        public int IsEditable = 0;

        //是否已加载文档标志位
        public int isLoadXmlDoc = 0;

        //是否已选择了日期和专业的CBX选择
        private int isSelectDate = 0;
        private int isSelectMajor = 0;

        //是否需要因CBX的选择更新数据
        private int forCbxUpdate = 1;

        //加载文档数据处理类
        private xmlData loadDoc = null;
        
        //提取元素的窗口
        public pageExtract page = null;

        //加载到日期COMBOBOX的数据类
        public class DateItem
        {
            public int ID { get; set; }
            public string date { get; set; }
        }

        //加载到编号COMBOBOX的数据类
        public class NumItem
        {
            public int ID { get; set; }
            public string Num { get; set; }
        }

        //加载文件路径
        private string loadDocPath = null;


        //就否需要添加日期、编号的节点
        private int isNeedNodeDate = 0;
        private int isNeedNodeNum = 0;
        private int isNeedNodeOrders = 0;


        public MainWindow()
        {
            InitializeComponent();

            //文本改动保存的复选框隐藏
            CheckBoxSureAdd.Visibility = Visibility.Hidden;

            //日期的DatePicker除了在创建日期时才能使用
            pickDateCreate.IsEnabled = false;
        }



        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ControlEditable();
        }


        //主窗口关闭时提取页也关闭
        private void Window_Closed(object sender, EventArgs e)
        {
            page.Close();
        }

        //日期 选择卡
        private void pickDateCreate_CalendarClosed(object sender, RoutedEventArgs e)
        {
            cbxDate.Text = "DB" + Convert.ToDateTime(pickDateCreate.Text).ToString("yyyyMMdd");
        }

        //变更日期 选择卡
        private void pickDateChange_CalendarClosed(object sender, RoutedEventArgs e)
        {
            txtChangeDate.Text = Convert.ToDateTime(pickDateChange.Text).ToString("yyyyMMdd");
        }

        //编辑状态
        private void ControlEditable()
        {
            if(IsEditable==0)
            {
                cbxDate.IsEnabled = false;
                txtMajor.IsEnabled = false;
                txtNum.IsEnabled = false;
                txtUnit.IsEnabled = false;
                txtChangeCost.IsEnabled = false;
                txtChangeDate.IsEnabled = false;
                txtChangeContent.IsEnabled = false;
                txtMainSelectEles.IsEnabled = false;
                btnDelNumItem.IsEnabled = false;
                btnDelDateItem.IsEnabled = false;
                btnSelectEles.IsEnabled = false;
                btnSave.IsEnabled = false;
                btnSaveAs.IsEnabled = false;
                pickDateChange.IsEnabled = false;
            }
            else
            {
                cbxDate.IsEnabled = true;
                txtMajor.IsEnabled = true;
                txtNum.IsEnabled = true;
                txtUnit.IsEnabled = true;
                txtChangeCost.IsEnabled = true;
                txtChangeDate.IsEnabled = true;
                txtChangeContent.IsEnabled = true;
                txtMainSelectEles.IsEnabled = true;
                btnDelNumItem.IsEnabled = true;
                btnDelDateItem.IsEnabled = true;
                btnSelectEles.IsEnabled = true;
                btnSave.IsEnabled = true;
                btnSaveAs.IsEnabled = true;
                pickDateChange.IsEnabled = true;
            }
        }


        //新建日期项 按钮
        private void btnCreateItem_Click(object sender, RoutedEventArgs e)
        {

            cbxDate.IsEnabled = true;
            cbxDate.IsEditable = true;
            pickDateCreate.IsEnabled = true;

            //若没有读取文档
            if (isLoadXmlDoc == 0)
            {
                List<DateItem> dateItems = new List<DateItem>();
                List<NumItem> numItems=new List<NumItem>();

                loadDoc=new xmlData();
                isLoadXmlDoc = 1;
            }

            //若读取了文档
            else
            {
                cbxDate.SelectedIndex=-1;
                txtMajor.SelectedIndex = -1;
                txtNum.SelectedIndex = -1;
                txtUnit.Clear();
                txtChangeCost.Clear();
                txtChangeDate.Clear();
                txtChangeContent.Clear();
                txtMainSelectEles.Clear();

                TaskDialog task=new TaskDialog("新建日期");
                task.Show();
            }
            
            isNeedNodeDate = 1;
            forCbxUpdate = 0;
        }


        //新建编号项 按钮
        private void btnCreateNumItem_Click(object sender, RoutedEventArgs e)
        {

            txtNum.IsEditable = true;

            if (cbxDate.Text.Length==0&& txtMajor.Text.Length==0)
            {
                TaskDialog taskDialog = new TaskDialog(Title = "错误");
                taskDialog.MainContent = "日期和专业必须有信息....";
                taskDialog.Show();
            }

            else
            {
                forCbxUpdate = 0;

                IsEditable = 1;
                ControlEditable();

                txtMajor.SelectedIndex = -1;
                txtNum.SelectedIndex = -1;
                txtUnit.Clear();
                txtChangeCost.Clear();
                txtChangeDate.Clear();
                txtChangeContent.Clear();
                txtMainSelectEles.Clear();

                isNeedNodeNum = 1;

                forCbxUpdate = 1;
            }

            forCbxUpdate = 0;
        }


        //删除编号项 按钮
        private void btnDelItem_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("确定要删除该编号？？？", "警告", MessageBoxButton.YesNo, MessageBoxImage.Warning) ==
                MessageBoxResult.Yes)
            {
                loadDoc.DCJs.Item(txtMajor.SelectedIndex).RemoveChild(loadDoc.Orders.Item(txtNum.SelectedIndex));
                txtUnit.Clear();
                txtChangeCost.Clear();
                txtChangeDate.Clear();
                txtChangeContent.Clear();
                txtMainSelectEles.Clear();

                

                List<NumItem> NumItemList = new List<NumItem>();

                for (int i = 0; i < loadDoc.Orders.Count; i++)
                {
                    NumItemList.Add(new NumItem { ID = i, Num = loadDoc.Orders.Item(i).FirstChild.InnerText });
                }


                txtNum.ItemsSource = NumItemList;
                txtNum.DisplayMemberPath = "Num";
                txtNum.SelectedValuePath = "ID";               

            }

        }

        //删除日期项 按钮
        private void btnDelDateItem_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("确定要删除该日期？？？", "警告", MessageBoxButton.YesNo, MessageBoxImage.Warning) ==
                MessageBoxResult.Yes)
            {
                
                loadDoc.DCDB.RemoveChild(loadDoc.DCDates[cbxDate.SelectedIndex]);
                

                txtUnit.Clear();
                txtChangeCost.Clear();
                txtChangeDate.Clear();
                txtChangeContent.Clear();
                txtMainSelectEles.Clear();



                List<DateItem> DateItemList = new List<DateItem>();

                for (int i = 0; i < loadDoc.DCDates.Count; i++)
                {
                    DateItemList.Add(new DateItem { ID = i, date = loadDoc.DCDates[i].Name });
                    TaskDialog taskDialog = new TaskDialog(loadDoc.DCDates[i].Name);
                    taskDialog.Show();
                }


                cbxDate.ItemsSource = DateItemList;
                cbxDate.DisplayMemberPath = "date";
                cbxDate.SelectedValuePath = "ID";

                                
            }
        }


        //从Rvt拾取元素 按钮
        private void btnSelectEles_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            IntPtr rvtPtr = Process.GetCurrentProcess().MainWindowHandle;
            WindowInteropHelper helper = new WindowInteropHelper(page);
            helper.Owner = rvtPtr;

            page.Show();
        }


        //读取 按钮按读取键时，把数据加载进去
        private void btnRead_Click(object sender, RoutedEventArgs e)
        {

            //弹出读取路径窗口，加载路径
            OpenFileDialog openDia=new OpenFileDialog();
            openDia.Filter = "XML文件|*.xml";
            if (openDia.ShowDialog() == true)
            {
                //清除原来窗口内加载数据
                cbxDate.Items.Clear();
                txtMajor.SelectedIndex = -1;
                txtNum.Items.Clear();
                txtUnit.Clear();
                txtChangeCost.Clear();
                txtChangeDate.Clear();
                txtChangeContent.Clear();
                txtMainSelectEles.Clear();
                xmlData xmldata = new xmlData();

                loadDocPath = openDia.FileName;
                xmldata.xmlLoad(loadDocPath);
                loadDoc = xmldata;

                //加载日期combobox
                List<DateItem> DateItemList=new List<DateItem>();
                for (int i=0;i<loadDoc.DCDates.Count;i++)
                {
                    DateItemList.Add(new DateItem {ID = i,date = loadDoc.DCDates.Item(i).Name});
                }

                cbxDate.ItemsSource = DateItemList;
                cbxDate.DisplayMemberPath = "date";
                cbxDate.SelectedValuePath = "ID";

                cbxDate.IsEnabled = true;

                //读取文档的标志位置为1
                isLoadXmlDoc = 1;
            }




            
        }

        //保存 按钮时，把数据保存
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            //如果存在文件路径就保存该路径，否则以另存为的方式
            if (loadDocPath != null)
            {
                loadDoc.saveXMl(loadDocPath);
            }
            else
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.DefaultExt = "xml";
                if (saveFileDialog.ShowDialog() == true)
                {
                    loadDoc.saveXMl(saveFileDialog.FileName);
                }
            }
        }
        //另存为 按钮
        private void btnSaveAs_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.DefaultExt = "xml";
            if (saveFileDialog.ShowDialog() == true)
            {
                loadDoc.saveXMl(saveFileDialog.FileName);
            }

        }


        //选择了日期的CBX选项
        private void cbxDate_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            forCbxUpdate = 0;
            txtNum.ItemsSource = null;
            txtNum.Text = "";
            txtUnit.Clear();
            txtChangeCost.Clear();
            txtChangeDate.Clear();
            txtChangeContent.Clear();
            txtMajor.SelectedIndex = -1;
            forCbxUpdate = 1;

            //如果专业和日期cbx都选了选项，更新编号cbx选项
            isSelectDate = 1;



        }

        //选择了专业的CBX选项
        private void txtMajor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            isSelectMajor = 1;

            if (isSelectDate == 1 && forCbxUpdate == 1)
            {
                
                loadDoc.DateIndex = cbxDate.SelectedIndex;
                loadDoc.NumIndex = txtMajor.SelectedIndex;
                TaskDialog task = new TaskDialog("专业"+ txtMajor.SelectedIndex);
                task.Show();
                loadDoc.DCJs = loadDoc.DCDates[loadDoc.DateIndex].ChildNodes;
                loadDoc.Orders = loadDoc.DCJs[loadDoc.NumIndex].ChildNodes;


                if (loadDoc.Orders.Count!=0)
                {                  
                    List<NumItem> NumItemList = new List<NumItem>();

                    for (int i = 0; i < loadDoc.Orders.Count; i++)
                    {
                        NumItemList.Add(new NumItem { ID = i, Num = loadDoc.Orders.Item(i).FirstChild.InnerText });
                    }


                    txtNum.ItemsSource = NumItemList;
                    txtNum.DisplayMemberPath = "Num";
                    txtNum.SelectedValuePath = "ID";

                }

                //重置编号、单位、金额、变更日期、内容
                txtNum.Text = "";
                txtUnit.Clear();
                txtChangeCost.Clear();
                txtChangeDate.Clear();
                txtChangeContent.Clear();
            }
        }

        //选择了编号的CBX选项
        private void txtNum_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (forCbxUpdate == 1 && isNeedNodeOrders == 1)
            {
                //加载单位、金额、日期和内容的文本数据
                txtUnit.Text = loadDoc.Orders.Item(txtNum.SelectedIndex).ChildNodes[1].InnerText;
                txtChangeCost.Text = loadDoc.Orders.Item(txtNum.SelectedIndex).ChildNodes[2].InnerText;
                txtChangeDate.Text = loadDoc.Orders.Item(txtNum.SelectedIndex).ChildNodes[3].InnerText;
                txtChangeContent.Text = loadDoc.Orders.Item(txtNum.SelectedIndex).ChildNodes[4].InnerText;

            }
        }

        //控件文本有改动时自动弹出checkbox
        private void cbxDate_OnTextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {           
            CheckBoxSureAdd.Visibility = Visibility.Visible;
        }

        private void txtNum_OnTextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            CheckBoxSureAdd.Visibility = Visibility.Visible;
        }

        private void txtUnit_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckBoxSureAdd.Visibility = Visibility.Visible;
            CheckBoxSureAdd.IsChecked = false;
        }

        private void txtChangeCost_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckBoxSureAdd.Visibility = Visibility.Visible;
            CheckBoxSureAdd.IsChecked = false;
        }

        private void txtChangeDate_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckBoxSureAdd.Visibility = Visibility.Visible;
            CheckBoxSureAdd.IsChecked = false;
        }

        private void txtChangeContent_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckBoxSureAdd.Visibility = Visibility.Visible;
            CheckBoxSureAdd.IsChecked = false;
        }


        //checkbox 确定加入所填的内容
        private void CheckBoxSureAdd_Checked(object sender, RoutedEventArgs e)
        {
            if (cbxDate.Text.Length == 0 || txtMajor.Text.Length == 0 || txtNum.Text.Length == 0)
            {
                TaskDialog taskDialog = new TaskDialog("缺少");
                taskDialog.MainContent = "日期、专业、编号应该全部选择上。。。。";
                taskDialog.Show();
            }

            else
            {
                CheckBoxSureAdd.Visibility = Visibility.Hidden;
                
                //同时添加日期和编号节点
                if (isNeedNodeDate == 1 && isNeedNodeNum == 1)
                {

                    //不需要建日期与编号的节点
                    isNeedNodeDate = 0;
                    isNeedNodeNum = 0;

                    XmlElement thisItemEle = loadDoc.doc.CreateElement(cbxDate.Text);
                    loadDoc.DCDB.AppendChild(thisItemEle);                       


                    XmlElement thisDCJZ = loadDoc.doc.CreateElement("DCJZ");
                    XmlElement thisDCJG = loadDoc.doc.CreateElement("DCJG");
                    XmlElement thisDCJD = loadDoc.doc.CreateElement("DCJD");

                    thisItemEle.AppendChild(thisDCJZ);
                    thisItemEle.AppendChild(thisDCJG);
                    thisItemEle.AppendChild(thisDCJD);
                

                    XmlElement orders = loadDoc.doc.CreateElement("Order");
                    loadDoc.DCDB.ChildNodes[loadDoc.DCDB.ChildNodes.Count-1].ChildNodes[txtMajor.SelectedIndex].AppendChild(orders);
                    XmlElement Num = loadDoc.doc.CreateElement("Num");
                    XmlElement Unit = loadDoc.doc.CreateElement("Unit");
                    XmlElement Cost = loadDoc.doc.CreateElement("Cost");
                    XmlElement Date = loadDoc.doc.CreateElement("Date");
                    XmlElement Content = loadDoc.doc.CreateElement("Content");

                    
                    Num.InnerText = txtNum.Text;
                    Unit.InnerText = txtUnit.Text;
                    Cost.InnerText = txtChangeCost.Text;
                    Date.InnerText = txtChangeDate.Text;
                    Content.InnerText = txtChangeContent.Text;

                    orders.AppendChild(Num);
                    orders.AppendChild(Unit);
                    orders.AppendChild(Cost);
                    orders.AppendChild(Date);
                    orders.AppendChild(Content);

                    //把日期设为不可编辑
                    cbxDate.IsEditable = false;
                    //把编号设为不可编辑
                    txtNum.IsEditable = false;


                    //重新加载日期combobox
                    List<DateItem> DateItemList = new List<DateItem>();
                    for (int i = 0; i < loadDoc.DCDates.Count; i++)
                    {
                        DateItemList.Add(new DateItem { ID = i, date = loadDoc.DCDates.Item(i).Name });
                    }

                    cbxDate.ItemsSource = DateItemList;
                    cbxDate.DisplayMemberPath = "date";
                    cbxDate.SelectedValuePath = "ID";



                    //选择当前刚添加的日期和编号

                    int lastlyMajorChoose = txtMajor.SelectedIndex;
                    cbxDate.SelectedIndex = loadDoc.DCDates.Count - 1;
                    txtMajor.SelectedIndex = lastlyMajorChoose;


                    //重新加载编号combobox
                    List<NumItem> NumItemList = new List<NumItem>();
                    loadDoc.Orders = loadDoc.DCDates[cbxDate.SelectedIndex].ChildNodes[txtMajor.SelectedIndex].ChildNodes;
                    NumItemList.Add(new NumItem { ID = 0, Num = loadDoc.Orders.Item(loadDoc.Orders.Count - 1).FirstChild.InnerText });

                    txtNum.ItemsSource = NumItemList;
                    txtNum.DisplayMemberPath = "Num";
                    txtNum.SelectedValuePath = "ID";

                    txtNum.SelectedIndex = 0;

                    //加载单位、金额、日期和内容的文本数据
                    txtUnit.Text = loadDoc.Orders.Item(txtNum.SelectedIndex).ChildNodes[1].InnerText;
                    txtChangeCost.Text = loadDoc.Orders.Item(txtNum.SelectedIndex).ChildNodes[2].InnerText;
                    txtChangeDate.Text = loadDoc.Orders.Item(txtNum.SelectedIndex).ChildNodes[3].InnerText;
                    txtChangeContent.Text = loadDoc.Orders.Item(txtNum.SelectedIndex).ChildNodes[4].InnerText;

                    //隐藏CheckBox
                    CheckBoxSureAdd.Visibility = Visibility.Hidden;

                    //使日期的DatePicker不可使用
                    pickDateCreate.IsEnabled = false;


                }



                //添加日期节点
                else if (isNeedNodeDate == 1)
                {
                    //不需要建日期的节点
                    isNeedNodeDate = 0;

                    XmlElement thisItemEle = loadDoc.doc.CreateElement(cbxDate.Text);
                    loadDoc.DCDB.AppendChild(thisItemEle);
                    XmlElement thisDCJZ = loadDoc.doc.CreateElement("DCJZ");
                    XmlElement thisDCJG = loadDoc.doc.CreateElement("DCJG");
                    XmlElement thisDCJD = loadDoc.doc.CreateElement("DCJD");

                    thisItemEle.AppendChild(thisDCJZ);
                    thisItemEle.AppendChild(thisDCJG);
                    thisItemEle.AppendChild(thisDCJD);

                    IsEditable = 0;
                    ControlEditable();

                    //把日期设为不可编辑
                    cbxDate.IsEditable = false;

                    //使日期的DatePicker不可使用
                    pickDateCreate.IsEnabled = false;

                    //重新加载编号combobox
                    List<NumItem> NumItemList = new List<NumItem>();
                    loadDoc.Orders = loadDoc.DCDates[cbxDate.SelectedIndex].ChildNodes[txtMajor.SelectedIndex].ChildNodes;

                    for (int i = 0; i < loadDoc.Orders.Count - 1; i++)
                    {
                        NumItemList.Add(new NumItem { ID = i, Num = loadDoc.Orders.Item(i).FirstChild.InnerText });
                    }



                    txtNum.ItemsSource = NumItemList;
                    txtNum.DisplayMemberPath = "Num";
                    txtNum.SelectedValuePath = "ID";

                    txtNum.SelectedIndex = loadDoc.Orders.Count - 1;

                    //加载单位、金额、日期和内容的文本数据
                    txtUnit.Text = loadDoc.Orders.Item(txtNum.SelectedIndex).ChildNodes[1].InnerText;
                    txtChangeCost.Text = loadDoc.Orders.Item(txtNum.SelectedIndex).ChildNodes[2].InnerText;
                    txtChangeDate.Text = loadDoc.Orders.Item(txtNum.SelectedIndex).ChildNodes[3].InnerText;
                    txtChangeContent.Text = loadDoc.Orders.Item(txtNum.SelectedIndex).ChildNodes[4].InnerText;
                }

                //添加编号节点
                else if (isNeedNodeNum == 1)
                {


                    //不需要再建编号的节点
                    isNeedNodeNum = 0;
                    XmlElement orders = loadDoc.doc.CreateElement("Order");
                    loadDoc.DCJs[txtMajor.SelectedIndex].AppendChild(orders);
                    XmlElement Num = loadDoc.doc.CreateElement("Num");
                    XmlElement Unit = loadDoc.doc.CreateElement("Unit");
                    XmlElement Cost = loadDoc.doc.CreateElement("Cost");
                    XmlElement Date = loadDoc.doc.CreateElement("Date");
                    XmlElement Content = loadDoc.doc.CreateElement("Content");


                    Num.InnerText = txtNum.Text;
                    Unit.InnerText = txtUnit.Text;
                    Cost.InnerText = txtChangeCost.Text;
                    Date.InnerText = txtChangeDate.Text;
                    Content.InnerText = txtChangeContent.Text;

                    orders.AppendChild(Num);
                    orders.AppendChild(Unit);
                    orders.AppendChild(Cost);
                    orders.AppendChild(Date);
                    orders.AppendChild(Content);

                    //把编号设为不可编辑
                    txtNum.IsEditable = false;




                }

                //更新节点内容
                else
                {
                    loadDoc.Orders[txtNum.SelectedIndex].ChildNodes[0].InnerText = txtNum.Text;
                    loadDoc.Orders[txtNum.SelectedIndex].ChildNodes[1].InnerText = txtUnit.Text;
                    loadDoc.Orders[txtNum.SelectedIndex].ChildNodes[2].InnerText = txtChangeCost.Text;
                    loadDoc.Orders[txtNum.SelectedIndex].ChildNodes[3].InnerText = txtChangeDate.Text;
                    loadDoc.Orders[txtNum.SelectedIndex].ChildNodes[4].InnerText = txtChangeContent.Text;
                }
            }

            CheckBoxSureAdd.IsChecked = false;

        }
    }

    public class xmlData
    {
        public string filePath { get; set; }

        public int DateIndex;

        public int MajorIndex;

        public int NumIndex;

        public XmlElement DCDB;

        public XmlNodeList DCDates;

        public XmlNodeList DCJs;

        public XmlNodeList Orders;

        private XmlNodeList order;

        private List<string> dataItem;

        public XmlDocument doc;

        //读取XML文件
        public void xmlLoad(string filePath)
        {
            doc=new XmlDocument();

            doc.Load(filePath);

            DCDB = doc.DocumentElement;

            

            DCDates = DCDB.ChildNodes;

        }

        //保存文件
        public void saveXMl(string path)
        {
            doc.Save(path);
        }



    }



}
