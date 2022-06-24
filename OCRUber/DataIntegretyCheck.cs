using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using OCRUber.Models;

namespace OCRUber
{
    public class DataIntegretyCheck
    {
        static readonly string[] moneyPropertyNames = { "Earnings", "Tip", "Fare", "BaseAmount", "TripSupplement" };
        static readonly string[] customerPaymenyMoneyPropertyNames = { "Total", "Tip", "Price", "PaidToUber" };
        
        List<PropertyInfo> moneyProperties;
        List<PropertyInfo> customerPaymentMoneyProperties;

        public DataIntegretyCheck()
        {
            moneyProperties = new List<PropertyInfo>();
            customerPaymentMoneyProperties = new List<PropertyInfo>();
            OrderSummary orderSummary = new OrderSummary();
            CustomerPayment custPayment = new CustomerPayment();
            foreach(string propertyName in moneyPropertyNames)
            {
                PropertyInfo prop = orderSummary.GetType()?.GetProperty(propertyName);
                if(prop != null)
                {
                    moneyProperties.Add(prop);
                }
            }
            foreach (string propertyName in customerPaymenyMoneyPropertyNames)
            {
                PropertyInfo prop = custPayment.GetType()?.GetProperty(propertyName);
                if (prop != null)
                {
                    customerPaymentMoneyProperties.Add(prop);
                }
            }
        }
        public OrderSummary CheckOrder(OrderSummary summary)
        {
            summary = CheckMoney(summary);
            var prop = summary.GetType().GetProperty("Earnings");
            var value = prop.GetValue(summary, null);
            
            return summary;
        }

        private OrderSummary CheckMoney(OrderSummary summary)
        {
            
            foreach(PropertyInfo prop in moneyProperties)
            {
                decimal? value = prop.GetValue(summary, null) as decimal?;
                if (!value.HasValue)
                {
                    summary.Errors.Add(new OrderParseError(){
                        PropertyName = prop.Name,
                        Property = prop,
                        Description = $"Unable to get value from property {prop.Name}",
                        Value = null,
                        Type = OrderParseErrorTypes.OTHER
                    });
                    continue;
                }
                if(value.Value == 0 && !string.Equals(prop.Name, "tip", StringComparison.CurrentCultureIgnoreCase))
                {
                    //0 is abnormal for earnings info, unfortunately except in the case of tip
                    summary.Errors.Add(new OrderParseError()
                    {
                        PropertyName = prop.Name,
                        Property = prop,
                        Description = $"{prop.Name} is 0 and was most likely not parsed",
                        Value = null,
                        Type = OrderParseErrorTypes.NO_VALUE
                    });
                    continue;
                }
                prop.SetValue(summary, MissingDecimalCheck(value.Value), null);
            }
            for(int i = 0; i < summary.CustomerPayments.Count; i++)
            {
                foreach (PropertyInfo prop in customerPaymentMoneyProperties)
                {
                    decimal? value = prop.GetValue(summary.CustomerPayments[i], null) as decimal?;
                    if (!value.HasValue)
                    {
                        summary.Errors.Add(new OrderParseError()
                        {
                            PropertyName = prop.Name,
                            Property = prop,
                            Description = $"CustomerPayments[{i}]{prop.Name}: Could not get value for {prop.Name}",
                            Value = null,
                            Type = OrderParseErrorTypes.OTHER
                        });
                        continue;
                    }
                    prop.SetValue(summary.CustomerPayments[i], MissingDecimalCheck(value.Value), null);
                }
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
