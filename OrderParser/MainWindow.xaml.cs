using System;
using System.Collections.Generic;
using System.IO;
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
using Microsoft.Win32;
using OrderParser.Src;
using TextToOrder.Src;
using TextToOrder.Src.Model;

namespace TextToOrder
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string CurrentFilePath = "";
        public IDictionary<int, Order> OrderList;

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            CustomerOrders.instance.AddOrderEvent += OnMessageReceived;
            CustomerOrders.instance.JobFinishedEvent += OnJobFinishReceived;
            CustomerOrders.instance.ErrorEvent += OnErrorReceived;
        }

        private void OnUploadClicked(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            bool? response = openFileDialog.ShowDialog();

            if (response == true)
            {
                string path = openFileDialog.FileName;
                if (path.Split(".").Last() != "txt")
                {
                    MessageBox.Show("File type must be .txt");
                    return;
                }
                CurrentFilePath = path;
                FilePathInput.Text = CurrentFilePath;
            }
        }

        public void ShowOrders()
        {
            if (OrderList.Count == 0)
            {
                Print(TextWindow, "No order found.");
                return;
            }
            foreach (KeyValuePair<int, Order> orderEntry in OrderList)
            {
                Order order = orderEntry.Value; 
                Print(TextWindow, $"Order: {order.Id}   Total Item: {order.Header.TotalItem}    Total Cost: {order.Header.TotalCost}");
                Print(TextWindow, $"Paid: {order.Header.Paid}     Shipped: {order.Header.Shipped}     Completed: {order.Header.Complete}");
                Print(TextWindow, $"Customer: {order.Header.CustomerName}");
                Print(TextWindow, $"Phone: {order.Header.CustomerPhone}     Email: {order.Header.CustomerEmail}");
                Print(TextWindow, $"Address: {order.Address.AddressLine1} {order.Address.AddressLine2?? ""} {order.Address.City}, {order.Address.State}, {order.Address.Zip}");
                Print(TextWindow, " ");
                foreach (OrderDetail item in order.OrderDetails)
                {
                    Print(TextWindow, $"    {item.LineNumber}   {item.Description}              Unit price: $ {item.CostEach}   Qt: {item.CostEach}  Total: ${item.CostTotal}");
                }
                Print(TextWindow, "______________________________________________________________________________________________________________________________________________________________");
                Print(TextWindow, " ");
            }
        }

        public void RunJob(object sender, RoutedEventArgs e)
        {
            Message.Text = "";
            TextWindow.Text = "";
            CustomerOrders.instance.AddOrder(CurrentFilePath);
        }
        private void Print(TextBlock textbox, string text)
        {
            textbox.Text += text + Environment.NewLine;
        }
        public void OnMessageReceived(object source, MessageEventArg e)
        {
            Print(Message, e.Message);
        }
        public void OnJobFinishReceived(object source, JobFinishedEventArg e)
        {
            Print(Message, e.Message);
            OrderList = e.Result;
            MessageBox.Show(e.Message);
            ShowOrders();
        }

        public void OnErrorReceived(object source, ErrorMessageEventArg e)
        {
            MessageBox.Show(e.Message);
        }

    }
}
