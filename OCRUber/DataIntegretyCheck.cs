using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OCRUber.Models;

namespace OCRUber
{
    public class DataIntegretyCheck
    {
        public OrderSummary CheckOrder(OrderSummary summary)
        {
            summary = CheckMoney(summary);
            return summary;
        }

        private OrderSummary CheckMoney(OrderSummary summary)
        {
            summary.Earnings = MissingDecimalCheck(summary.Earnings);
            summary.Tip = MissingDecimalCheck(summary.Tip);
            summary.Fare = MissingDecimalCheck(summary.Fare);
            summary.BaseAmount = MissingDecimalCheck(summary.BaseAmount);
            summary.TripSupplement = MissingDecimalCheck(summary.TripSupplement);
            for(int i = 0; i < summary.CustomerPayments.Count; i++)
            {
                CustomerPayment customerPayment = summary.CustomerPayments[i];
                customerPayment.Total = MissingDecimalCheck(customerPayment.Total);
                customerPayment.Tip = MissingDecimalCheck(customerPayment.Tip);
                customerPayment.Price = MissingDecimalCheck(customerPayment.Price);
                customerPayment.PaidToUber = MissingDecimalCheck(customerPayment.PaidToUber);

                summary.CustomerPayments[i] = customerPayment;
            }

            return summary;
        }

        private decimal MissingDecimalCheck(decimal amount)
        {
            //If a money amount has no fraction, and is more than 100, there is a good chance the decimal was missed by OCR
            if (amount - Math.Round(amount) == decimal.Zero && (amount > 100 || amount < -100))
            {
                amount = amount / 100;
            }
            return amount;
        }
    }
}
