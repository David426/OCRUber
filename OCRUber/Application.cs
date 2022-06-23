using IronOcr;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Globalization;

namespace OCRUber
{
    public class Application
    {

        static int count = 1;

        public static void Main(string[] args)
        {
            OrderParser orderParser;
            var Ocr = new IronTesseract();

            DirectoryInfo d = new DirectoryInfo(@"../../../DataSet/");

            foreach (var file in d.GetFiles("*.jpg"))
            {
                string text = null;
                using (var Input = new OcrInput(file.FullName))
                {
                    var Result = Ocr.Read(Input);
                    text = Result.Text;
                }
                orderParser = new OrderParser(text);
                orderParser.ParseOrder();
            }

        }
    }
}
