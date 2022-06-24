using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCRUber.Models
{
    public class CustomerPayment
    {
        public decimal Total { get; set; }
        public decimal Price { get; set; }
        public decimal Tip { get; set; }
        public decimal PaidToUber { get; set; }

        public override string ToString()
        {
            return $"Earnings :{Price} - {PaidToUber} + {Tip}\nTotal Payment: {Total} = {Price} + {Tip}";
        }
    }
}
