using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using TextToOrder.Src;
using TextToOrder.Src.Model;

namespace OrderParser.Src
{
	public class CustomerOrders
	{
		private Dictionary<int, Order> _OrderData;
		private static CustomerOrders _CustomerOrders = new CustomerOrders();
		public static CustomerOrders instance => _CustomerOrders;

		private CustomerOrders()
		{
			_OrderData = new Dictionary<int, Order>();
		}

        public void AddOrder(string filePath)
		{
            StreamReader sr;
            try
            {
                sr = new StreamReader($"{filePath}");
            }
			catch(Exception e)
            {
                OnError(e.Message);
                return;
            }

            string? currentLine = sr.ReadLine();
			Order currentOrder = new Order();
            int totalOrderRead = 0;
			int currentLineNumber = 0;
            int orderAdded = 0;
            int failedOrders = 0;

            while (currentLine != null)
			{
                try
                {
                    currentLineNumber++;
                    if (currentLine[0] == '1' && currentLineNumber > 1 && !currentOrder.IsValid)
                        OnSendMessage($"Failed to parse order: {currentOrder.Id.ToString() ?? "Invalid Id"}");
                    OnSendMessage($"Reading Line {currentLineNumber}......");

                    if (currentLine[0] == '1')
                    {
                        currentOrder = new Order();
                        totalOrderRead++;
                        ParseHeader(currentLine, currentLineNumber, currentOrder);
                    }

                    else if (currentLine[0] == '2' && currentOrder.Header != null)
                    {
                        ParseAddress(currentLine, currentLineNumber, currentOrder);
                    }

                    else if (currentLine[0] == '3' && currentOrder.Header != null && currentOrder.Address != null)
                    {
                        ParseDetail(currentLine, currentLineNumber, currentOrder);
                        currentLine = sr.ReadLine();
                        if (currentLine == null || currentLine[0] != '3')
                        {
                            
                            if (_OrderData.ContainsKey(currentOrder.Id))
                            {
                                throw new OrderPaseException($"Duplicate order id, Order {currentOrder.Id} has already been added");
                            }
                            _OrderData.Add(currentOrder.Id, currentOrder);
                            currentOrder.IsValid = true;
                            orderAdded++;
                            OnSendMessage($"Succussfully add order {currentOrder.Id}");
                            break;
                        }
                        continue;
                    } 
                    
                    currentLine = sr.ReadLine();
                }

                catch(OrderPaseException ex)
                {
                    OnSendMessage(ex.Message);  
                    currentOrder.ErrorMessage = ex.Message;
                    failedOrders++;
                    currentLine = sr.ReadLine();
                }
                catch (Exception ex)
                {
                    OnSendMessage("Operation failed: ");
                    OnSendMessage(ex.Message);
                    break;
                }
            }
            if (!_OrderData.ContainsKey(currentOrder.Id))
            {
                _OrderData.Add(currentOrder.Id, currentOrder);
                currentOrder.IsValid = true;
                orderAdded++;
            }
            sr.Close();
            OnJobFinished($"Read {totalOrderRead} order, successfully added {orderAdded} record, {failedOrders} faild.", _OrderData);
        }

        // +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++ parsing order ++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        private void ParseHeader(string line, int lineNumber, Order currentOrder)
        {
            Validation validator = new Validation();
            Header header = new Header();
            if (validator.ParseNumber(line.Substring(0, 3), lineNumber, "header identifier") != 100)
                throw new OrderPaseException($"Incorrect Header identifier at line {lineNumber}");

            currentOrder.Id = validator.ParseNumber(line.Substring(3, 10), lineNumber, "order number");
            header.OrderNumber = currentOrder.Id;
            header.TotalItem = validator.ParseNumber(line.Substring(13, 5), lineNumber, "Total item");
            header.TotalCost = validator.ParsePrice(line.Substring(18, 10), lineNumber, "Total cost");
            header.OrderDate = validator.ParseDate(line.Substring(28, 19), lineNumber, "Order date");
            header.CustomerName = validator.ParseString(line.Substring(47, 50), lineNumber, "Customer name");
            header.CustomerPhone = validator.ParseString(line.Substring(97, 30), lineNumber, "Customer phone");
            header.CustomerEmail = validator.ParseString(line.Substring(127, 50), lineNumber, "Customer email");
            header.Paid = validator.ParseBool(line.Substring(177, 1), lineNumber, "Paid");
            header.Shipped = validator.ParseBool(line.Substring(178, 1), lineNumber, "Shipped");
            header.Complete = validator.ParseBool(line.Substring(179, 1), lineNumber, "Complete");
            currentOrder.Header = header;
        }
        private void ParseAddress(string line, int lineNumber, Order currentOrder)
        {
            Validation validator = new Validation();
            Address address = new Address();
            if (validator.ParseNumber(line.Substring(0, 3), lineNumber, "address identifier") != 200)
                throw new OrderPaseException($"Incorrect Address identifier at line {lineNumber}");
            address.AddressLine1 = validator.ParseString(line.Substring(3, 50), lineNumber, "Address line 1");
 //           address.AddressLine2 = validator.ParseString(line.Substring(53, 50), lineNumber, "Address line 2");
            address.City = validator.ParseString(line.Substring(103, 50), lineNumber, "Address line 1");
            address.State = validator.ParseString(line.Substring(153, 2), lineNumber, "Address line 1");
            address.Zip = validator.ParseString(line.Substring(155), lineNumber, "Address line 1");
            currentOrder.Address = address;
        }
        private void ParseDetail(string line, int lineNumber, Order currentOrder)
        {
            Validation validator = new Validation();
            OrderDetail detail = new OrderDetail();
            if (validator.ParseNumber(line.Substring(0, 3), lineNumber, "detail identifier") != 300)
                throw new OrderPaseException($"Incorrect Detail identifier at line {lineNumber}");
            detail.LineNumber = validator.ParseNumber(line.Substring(3, 2), lineNumber, "Line number");
            detail.Quantity = validator.ParseNumber(line.Substring(5, 5), lineNumber, "Quantity");
            detail.CostEach = validator.ParsePrice(line.Substring(10, 10), lineNumber, "Cost each");
            detail.CostTotal = validator.ParsePrice(line.Substring(20, 10), lineNumber, "Cost total");
            detail.Description = validator.ParseString(line.Substring(30), lineNumber, "Description");

            if (detail.Quantity * detail.CostEach != detail.CostTotal)
            {
                throw new OrderPaseException($"Incorrect Cost total at line {lineNumber}");
            }

            currentOrder.OrderDetails.Add(detail);
        }

        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++ Event handlers ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

        public delegate void AddOrderEventHandler(object source, MessageEventArg args);

        public event AddOrderEventHandler AddOrderEvent;

        protected virtual void OnSendMessage(string message)
        {
            if (AddOrderEvent != null)
            {
                AddOrderEvent(this, new MessageEventArg(message));
            }
        }
        public delegate void JobFinishedEventHandler(object source, JobFinishedEventArg e);

        public event JobFinishedEventHandler JobFinishedEvent;

        protected virtual void OnJobFinished(string message, Dictionary<int, Order> results)
        {
            if (JobFinishedEvent != null)
            {
                JobFinishedEvent(this, new JobFinishedEventArg(message, results));
            }
        }

        public delegate void ErrorEventHandler(object source, ErrorMessageEventArg e);

        public event ErrorEventHandler ErrorEvent;

        protected virtual void OnError(string message)
        {
            if (ErrorEvent != null)
            {
                ErrorEvent(this, new ErrorMessageEventArg(message));
            }
        }
    }
}

