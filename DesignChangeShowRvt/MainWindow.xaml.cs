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



        public MainWindow()
        {
            InitializeComponent();
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
                pickDateCreate.IsEnabled = false;
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
                pickDateCreate.IsEnabled = true;
                pickDateChange.IsEnabled = true;
            }
        }


        //新建日期项 按钮
        private void btnCreateItem_Click(object sender, RoutedEventArgs e)
        {
            IsEditable = 1;
            ControlEditable();


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
                txtMajor.SelectedIndex = 0;
                txtNum.SelectedIndex = -1;
                txtUnit.Clear();
                txtChangeCost.Clear();
                txtChangeDate.Clear();
                txtChangeContent.Clear();
                txtMainSelectEles.Clear();
            }



        }


        //新建编号项 按钮
        private void btnCreateNumItem_Click(object sender, RoutedEventArgs e)
        {
            forCbxUpdate = 0;           
            if (isSelectDate == 0)
            {
                TaskDialog taskDialog = new TaskDialog(Title = "错误");
                taskDialog.MainContent = "请先选择日期....";
                taskDialog.Show();
            }
            txtMajor.SelectedIndex = -1;
            txtNum.SelectedIndex = -1;
            txtUnit.Clear();
            txtChangeCost.Clear();
            txtChangeDate.Clear();
            txtChangeContent.Clear();
            txtMainSelectEles.Clear();

            forCbxUpdate = 1;



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
            IsEditable = 1;
            ControlEditable();

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

            //弹出读取路径窗口，加载路径
            OpenFileDialog openDia=new OpenFileDialog();
            openDia.Filter = "XML文件|*.xml";
            if (openDia.ShowDialog() == true)
            {
                loadDocPath = openDia.FileName;
                xmldata.xmlLoad(loadDocPath);
                loadDoc = xmldata;

                isLoadXmlDoc = 0;
            }

            //加载日期combobox
            List<DateItem> DateItemList=new List<DateItem>();
            for (int i=0;i<loadDoc.DCDates.Count;i++)
            {
                DateItemList.Add(new DateItem {ID = i,date = loadDoc.DCDates.Item(i).Name});
            }

            cbxDate.ItemsSource = DateItemList;
            cbxDate.DisplayMemberPath = "date";
            cbxDate.SelectedValuePath = "ID";

            
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

        //checkbox 确定加入所填的内容
        private void CheckBoxSureAdd_Checked(object sender, RoutedEventArgs e)
        {

        }

        //选择了日期的CBX选项
        private void cbxDate_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //如果专业和日期cbx都选了选项，更新编号cbx选项
            isSelectDate = 1;
            if (isSelectMajor == 1&& forCbxUpdate==1)
            {
                loadDoc.DateIndex = cbxDate.SelectedIndex;
                loadDoc.NumIndex = txtMajor.SelectedIndex;



                loadDoc.DCJs = loadDoc.DCDates[loadDoc.DateIndex].ChildNodes;
                loadDoc.Orders = loadDoc.DCJs[loadDoc.NumIndex].ChildNodes;


                List<NumItem> NumItemList = new List<NumItem>();

                for (int i = 0; i < loadDoc.Orders.Count; i++)
                {
                    NumItemList.Add(new NumItem {ID=i,Num=loadDoc.Orders.Item(i).FirstChild.InnerText});
                }


                txtNum.ItemsSource = NumItemList;
                txtNum.DisplayMemberPath = "Num";
                txtNum.SelectedValuePath = "ID";
            }



        }

        //选择了专业的CBX选项
        private void txtMajor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            isSelectMajor = 1;

            if (isSelectDate == 1 && forCbxUpdate == 1)
            {
                loadDoc.DateIndex = cbxDate.SelectedIndex;
                loadDoc.NumIndex = txtMajor.SelectedIndex;



                loadDoc.DCJs = loadDoc.DCDates[loadDoc.DateIndex].ChildNodes;
                loadDoc.Orders = loadDoc.DCJs[loadDoc.NumIndex].ChildNodes;


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

        //选择了编号的CBX选项
        private void txtNum_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (forCbxUpdate == 1)
            {
                //加载单位、金额、日期和内容的文本数据
                txtUnit.Text = loadDoc.Orders.Item(txtNum.SelectedIndex).ChildNodes[1].InnerText;
                txtChangeCost.Text = loadDoc.Orders.Item(txtNum.SelectedIndex).ChildNodes[2].InnerText;
                txtChangeDate.Text = loadDoc.Orders.Item(txtNum.SelectedIndex).ChildNodes[3].InnerText;
                txtChangeContent.Text = loadDoc.Orders.Item(txtNum.SelectedIndex).ChildNodes[4].InnerText;
            }


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

        private XmlDocument doc;

        //读取XML文件
        public void xmlLoad(string filePath)
        {
            doc=new XmlDocument();

            doc.Load(filePath);

            DCDB = doc.DocumentElement;

            

            DCDates = DCDB.ChildNodes;

        }


        //选中数据时更新所有信息
        public List<string> selectUpdateXml(int dateIndex,int majorIndex,int numIndex)
        {
            List<string> orderInfos = new List<string>();

            this.DateIndex = dateIndex;
            this.MajorIndex = majorIndex;
            this.NumIndex = numIndex;

            this.DCJs = DCDates[DateIndex].ChildNodes;

            this.Orders = DCJs[MajorIndex].ChildNodes;

            this.order = Orders[NumIndex].ChildNodes;

            orderInfos.Add(order[0].InnerText);
            orderInfos.Add(order[1].InnerText);
            orderInfos.Add(order[2].InnerText);
            orderInfos.Add(order[3].InnerText);
            orderInfos.Add(order[4].InnerText);

            return orderInfos;
        }


        //增加或更新节点
        public void addItem(string date,int majorIndex,string num,string unit,string cost,string changeDate,string changeContent,string selectEleId)
        {
            int needCreateItem = 0;
            int index = 0;
            XmlElement thisItemEle = null;
            XmlNode thisItemNode = null;

            foreach (XmlNode item in DCDates)
            {
                if (item.Name == date)
                {
                    needCreateItem = 1;
                    break;
                }

                index++;
            }

            if (needCreateItem != 0)
            {
                thisItemEle = doc.CreateElement(date);
                DCDB.AppendChild(thisItemEle);
                XmlElement thisDCJZ = doc.CreateElement("DCJZ");
                XmlElement thisDCJG = doc.CreateElement("DCJG");
                XmlElement thisDCJD = doc.CreateElement("DCJZD");

                thisItemEle.AppendChild(thisDCJZ);
                thisItemEle.AppendChild(thisDCJG);
                thisItemEle.AppendChild(thisDCJD);

                thisItemNode = DCDates.Item(DCDates.Count - 1);

                switch (majorIndex)
                {
                    case 0:
                    {
                        XmlElement orders= doc.CreateElement("Order");
                        orders.SetAttribute("Num", num);
                        thisDCJZ.AppendChild(orders);
                        XmlElement Unit = doc.CreateElement("Unit");
                        XmlElement Cost = doc.CreateElement("Cost");
                        XmlElement Date = doc.CreateElement("Date");
                        XmlElement Content = doc.CreateElement("Content");
                        XmlElement ElementIds = doc.CreateElement("ElementIds");

                        Unit.InnerText = unit;
                        Cost.InnerText = cost;
                        Date.InnerText = changeDate;
                        Content.InnerText = changeContent;
                        ElementIds.InnerText = selectEleId;

                        orders.AppendChild(Unit);
                        orders.AppendChild(Cost);
                        orders.AppendChild(Date);
                        orders.AppendChild(Content);
                        orders.AppendChild(ElementIds);

                        break;
                    }
                    case 1:
                    {
                        XmlElement orders = doc.CreateElement("Order");
                        orders.SetAttribute("Num", num);
                        thisDCJG.AppendChild(orders);
                        XmlElement Unit = doc.CreateElement("Unit");
                        XmlElement Cost = doc.CreateElement("Cost");
                        XmlElement Date = doc.CreateElement("Date");
                        XmlElement Content = doc.CreateElement("Content");
                        XmlElement ElementIds = doc.CreateElement("ElementIds");

                        Unit.InnerText = unit;
                        Cost.InnerText = cost;
                        Date.InnerText = changeDate;
                        Content.InnerText = changeContent;
                        ElementIds.InnerText = selectEleId;

                        orders.AppendChild(Unit);
                        orders.AppendChild(Cost);
                        orders.AppendChild(Date);
                        orders.AppendChild(Content);
                        orders.AppendChild(ElementIds);
                        break;
                    }
                    case 2:
                    {
                        XmlElement orders = doc.CreateElement("Order");
                        orders.SetAttribute("Num", num);
                        thisDCJD.AppendChild(orders);
                        XmlElement Unit = doc.CreateElement("Unit");
                        XmlElement Cost = doc.CreateElement("Cost");
                        XmlElement Date = doc.CreateElement("Date");
                        XmlElement Content = doc.CreateElement("Content");
                        XmlElement ElementIds = doc.CreateElement("ElementIds");

                        Unit.InnerText = unit;
                        Cost.InnerText = cost;
                        Date.InnerText = changeDate;
                        Content.InnerText = changeContent;
                        ElementIds.InnerText = selectEleId;

                        orders.AppendChild(Unit);
                        orders.AppendChild(Cost);
                        orders.AppendChild(Date);
                        orders.AppendChild(Content);
                        orders.AppendChild(ElementIds);

                        break;
                    }
                }


            }
            else
            {
                thisItemNode = DCDates.Item(index);
                XmlNode Orders = null;

                switch (majorIndex)
                {
                    case 0:
                       
                    {
                        Orders = thisItemNode.ChildNodes[0];

                        XmlElement orders = doc.CreateElement("Order");
                        orders.SetAttribute("Num", num);
                        Orders.AppendChild(orders);
                        XmlElement Unit = doc.CreateElement("Unit");
                        XmlElement Cost = doc.CreateElement("Cost");
                        XmlElement Date = doc.CreateElement("Date");
                        XmlElement Content = doc.CreateElement("Content");
                        XmlElement ElementIds = doc.CreateElement("ElementIds");

                        Unit.InnerText = unit;
                        Cost.InnerText = cost;
                        Date.InnerText = changeDate;
                        Content.InnerText = changeContent;
                        ElementIds.InnerText = selectEleId;

                        orders.AppendChild(Unit);
                        orders.AppendChild(Cost);
                        orders.AppendChild(Date);
                        orders.AppendChild(Content);
                        orders.AppendChild(ElementIds);

                        break;

                     }
                    case 1:
                    {
                        Orders = thisItemNode.ChildNodes[1];

                        XmlElement orders = doc.CreateElement("Order");
                        orders.SetAttribute("Num", num);
                        Orders.AppendChild(orders);
                        XmlElement Unit = doc.CreateElement("Unit");
                        XmlElement Cost = doc.CreateElement("Cost");
                        XmlElement Date = doc.CreateElement("Date");
                        XmlElement Content = doc.CreateElement("Content");
                        XmlElement ElementIds = doc.CreateElement("ElementIds");

                        Unit.InnerText = unit;
                        Cost.InnerText = cost;
                        Date.InnerText = changeDate;
                        Content.InnerText = changeContent;
                        ElementIds.InnerText = selectEleId;

                        orders.AppendChild(Unit);
                        orders.AppendChild(Cost);
                        orders.AppendChild(Date);
                        orders.AppendChild(Content);
                        orders.AppendChild(ElementIds);

                        break;
                    }

                    case 2:
                    {
                        Orders = thisItemNode.ChildNodes[2];

                        XmlElement orders = doc.CreateElement("Order");
                        orders.SetAttribute("Num", num);
                        Orders.AppendChild(orders);
                        XmlElement Unit = doc.CreateElement("Unit");
                        XmlElement Cost = doc.CreateElement("Cost");
                        XmlElement Date = doc.CreateElement("Date");
                        XmlElement Content = doc.CreateElement("Content");
                        XmlElement ElementIds = doc.CreateElement("ElementIds");

                        Unit.InnerText = unit;
                        Cost.InnerText = cost;
                        Date.InnerText = changeDate;
                        Content.InnerText = changeContent;
                        ElementIds.InnerText = selectEleId;

                        orders.AppendChild(Unit);
                        orders.AppendChild(Cost);
                        orders.AppendChild(Date);
                        orders.AppendChild(Content);
                        orders.AppendChild(ElementIds);

                        break;
                     }                        
                }
            }
        }


        //删除当前编号项的节点
        public void delNumItem(string date, int majorIndex, string num)
        {
            
            int index = 0;
            XmlNode thisItemNode = null;

            foreach (XmlNode item in DCDates)
            {
                if (item.Name == date)
                {
                    break;
                }
                index++;
            }

            thisItemNode = DCDates.Item(index);
            XmlNode Orders = null;

            switch (majorIndex)
            {
                case 0:
                {
                    Orders = thisItemNode.ChildNodes[0];
                    break;
                }
                case 1:
                {
                    Orders = thisItemNode.ChildNodes[1];
                    break;
                }
                case 2:
                {
                    Orders = thisItemNode.ChildNodes[2];
                    break;
                }
            }

            foreach (XmlNode node in Orders.ChildNodes)
            {
                if (node.Attributes["Num"].Value == num)
                {
                    Orders.RemoveChild(node);
                    break;
                }
            }
        }

        //删除当前日期项的节点
        public void delDateItem(string date, int majorIndex)
        {

            int index = 0;
            XmlNode thisItemNode = null;

            foreach (XmlNode item in DCDates)
            {
                if (item.Name == date)
                {
                    break;
                }
                index++;
            }

            thisItemNode = DCDates.Item(index);
            XmlNode Orders = null;

            switch (majorIndex)
            {
                case 0:
                {
                    Orders = thisItemNode.ChildNodes[0];
                    break;
                }
                case 1:
                {
                    Orders = thisItemNode.ChildNodes[1];
                    break;
                }
                case 2:
                {
                    Orders = thisItemNode.ChildNodes[2];
                    break;
                }
            }

            thisItemNode.RemoveChild(Orders);
        }

        //保存文件
        public void saveXMl(string path)
        {
            doc.Save(path);
        }



    }



}
