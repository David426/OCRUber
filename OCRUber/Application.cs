using IronOcr;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Globalization;
using OCRUber.Models;

namespace OCRUber
{
    public class Application
    {
        public static void Main(string[] args)
        {
            OrderParser orderParser = new OrderParser();
            DataIntegretyCheck dataIntegretyCheck = new DataIntegretyCheck();
            var Ocr = new IronTesseract();

            DirectoryInfo d = new DirectoryInfo(@"../../../DataSet/");
            List<OrderSummary> orderSummaries = new List<OrderSummary>();
            foreach (var file in d.GetFiles("*.jpg"))
            {
                string text = null;
                using (var Input = new OcrInput(file.FullName))
                {
                    var Result = Ocr.Read(Input);
                    text = Result.Text;
                }
                OrderSummary summary = orderParser.ParseOrder(text);
                summary = dataIntegretyCheck.CheckOrder(summary);
                orderSummaries.Add(summary);
                Console.Write(summary.ToString());
            }
            Console.WriteLine("\n\n...\n\nERRORS\n------------------------------");
            var errorOrders = orderSummaries.Where(os => os.Errors.Count > 0);
            foreach(var orderWithError in errorOrders)
            {
                Console.WriteLine($"Order {orderWithError.RequestTime}, {orderWithError.Earnings}");
                foreach(var error in orderWithError.Errors)
                {
                    Console.WriteLine($"  {error.PropertyName} : {error.Value}");
                    Console.WriteLine($"  {error.Description}");
                    Console.WriteLine($"  --");
                }
                Console.WriteLine($"===============================");
            }
        }
    }
}
