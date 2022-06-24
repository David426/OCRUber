using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCRUber.Models
{
    public class OrderSummary
    {
        public Address? PickupLocation { get; set; }
        public Address? DropoffLocation { get; set; }
        public DateTime RequestTime { get; set; }
        public TimeSpan TripDuration { get; set; }
        public decimal Earnings { get; set; }
        public int Points { get; set; }
        public decimal Tip { get; set; }
        public decimal Fare { get; set; }
        public decimal BaseAmount { get; set; }
        public decimal TripSupplement { get; set; }
        public List<CustomerPayment> CustomerPayments { get; set; }
        public string OCRParseText { get; set; }
        public List<OrderParseError> Errors { get; set; }
        public OrderSummary()
        {
            CustomerPayments = new List<CustomerPayment>();
            Errors = new List<OrderParseError>();
        }

        public override string ToString()
        {
            string returnValue= 
$@"{PickupLocation}
{DropoffLocation}
{RequestTime} : {TripDuration} +{Points}
{Fare}
    {BaseAmount}
    {TripSupplement}
{Tip}
-----------------------------------------
Earnings: {Earnings}
-----------------------------------------
";
            foreach(CustomerPayment customerPayment in CustomerPayments)
            {
                returnValue +=
$@"Customer Price {customerPayment.Price}
Tip {customerPayment.Tip}
-----------------------------------------
Total {customerPayment.Total}
Paid To Uber {customerPayment.PaidToUber}
";
            }
            returnValue += "=========================================\n\n";
            return returnValue;
        }
    }
}
