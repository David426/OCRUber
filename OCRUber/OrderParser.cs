using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using OCRUber.Models;

namespace OCRUber
{
    public class OrderParser
    {
        List<string> OrderLines;
        string OrderText;
        RegexManager RegexManager;

        public OrderParser()
        {
            RegexManager = new RegexManager();
        }

        public OrderSummary ParseOrder(string text)
        {
            OrderText = text;
            OrderLines = OrderText.Split('\n').ToList();
            OrderSummary orderSummary = new OrderSummary();

            //Remove blank lines
            Regex regex = RegexManager.GetRegex(RegexType.BlankLine);
            OrderLines = OrderLines.Where(t => !regex.IsMatch(t)).ToList();
            orderSummary.OCRParseText = string.Join("\n", OrderLines);
            //Get to "Trip Details"

            orderSummary.PickupLocation = ParseAddress(GetLine(RegexManager.GetRegex(RegexType.TripDetails), offset: 1));
            orderSummary.DropoffLocation = ParseAddress(GetLine(RegexManager.GetRegex(RegexType.TripDetails), offset: 2));

            //Look for Earnings
            orderSummary.Earnings = ParseMoney(GetLine(RegexManager.GetRegex(RegexType.Money)));

            //Look for duration
            orderSummary.TripDuration = ParseTripTime(GetLine(RegexManager.GetRegex(RegexType.Duration)));

            //Get Time Requested
            TimeOnly requestTime = ParseRequestTime(GetLine(RegexManager.GetRegex(RegexType.TimeRequested)));

            //Get Date Requested
            DateTime requestDateTime = ParseRequestDate(GetLine(RegexManager.GetRegex(RegexType.DateRequested)));
            orderSummary.RequestTime = requestDateTime.AddHours(requestTime.Hour).AddMinutes(requestTime.Minute);

            //Get Points Earned
            orderSummary.Points = ParsePointsEarned(GetLine(RegexManager.GetRegex(RegexType.PointsEarned)));

            //Paid To You section
            orderSummary.Tip = ParseMoney(GetLine(RegexManager.GetRegex(RegexType.TipIncluded)));
            orderSummary.Fare = ParseMoney(GetLine(RegexManager.GetRegex(RegexType.Fare)));
            orderSummary.BaseAmount = ParseMoney(GetLine(RegexManager.GetRegex(RegexType.Base)));
            orderSummary.TripSupplement = ParseMoney(GetLine(RegexManager.GetRegex(RegexType.TripSupplement)));

            //Customer Payments Section
            int currentLine = 0;
            currentLine = GetLineNumber(RegexManager.GetRegex(RegexType.CustomerPayments));
            List<CustomerPayment> customers = ParseCustomerPayments(currentLine);
            orderSummary.CustomerPayments.AddRange(customers);

            return orderSummary;
        }

        /// <summary>
        /// Scans the list of lines of text, and returns the 1st line that matches the regex, or -1 if there is none
        /// </summary>
        /// <param name="regex">The regex to check for a match</param>
        /// <param name="startLine">The line number to start from</param>
        /// <returns>The line number matching the regex, or -1 if no match is found</returns>
        private int GetLineNumber(Regex regex, int startLine = 0)
        {
            if (startLine == -1) return -1;
            for (int i = startLine; i < OrderLines.Count; i++)
            {
                if (regex.IsMatch(OrderLines[i]))
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Scans the list of lines of text, and returns the 1st line that matches the regex, or empty string if there is none
        /// </summary>
        /// <param name="regex">The regex to check for a match</param>
        /// <param name="startLine">The line number to start from</param>
        /// <param name="offset">Is added to the line number before returning the line. e.g. offset 1 returns the line after the matching line</param>
        /// <returns>The line of match +/- the offset amount. Returns empty string if no match found, or if with offset line number is out of bounds</returns>
        private string GetLine(Regex regex, int startLine = 0, int offset = 0)
        {
            int lineNumber = GetLineNumber(regex, startLine);
            if(lineNumber == -1)
            {
                return "";
            }
            if(lineNumber + offset < 0 || lineNumber + offset >= OrderLines.Count)
            {
                return "";
            }
            return OrderLines[lineNumber + offset];
        }

        private string TrimAddressBegining(string address)
        {
            Regex regex = RegexManager.GetRegex(RegexType.AddressStart);
            Match match = regex.Match(address);
            if (match != null)
            {
                address = address.Substring(match.Length);
            }
            return address;
        }

        private int GetMonthNumberFromAbbreviation(string monthAbbreviation)
        {
            string[] monthAbbrev = CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedMonthNames;

            // Creates a TextInfo based on the "en-US" culture.
            TextInfo myTI = new CultureInfo("en-US", false).TextInfo;
            string monthname = myTI.ToTitleCase(monthAbbreviation.ToLower());
            int index = Array.IndexOf(monthAbbrev, monthname) + 1;
            return index;
        }

        private Address ParseAddress(string input)
        {
            input = TrimAddressBegining(input);
            Address address = new Address();
            Regex regex = RegexManager.GetRegex(RegexType.Address);
            List<Match> match = regex.Matches(input).ToList();
            if (match == null || match.Count != 4)
            {
                Console.Error.WriteLine($"Matches for {input} = {match?.Count ?? 0} expected 4");
                return null;
            }
            address.Street = match[0].Value.Replace(",", "").Trim();
            address.Town = match[1].Value.Replace(",", "").Trim();
            address.State = match[2].Value.Replace(",", "").Split(" ")[0];
            address.Zip = match[2].Value.Replace(",", "").Split(" ")[1];
            address.Country = match[3].Value.Replace(",", "").Trim();
            return address;
        }

        private decimal ParseMoney(string input)
        {
            if (string.IsNullOrEmpty(input)) return 0;
            Regex moneyRegex = RegexManager.GetRegex(RegexType.Money);
            string earnedString = moneyRegex.Match(input).Value;
            decimal earned = decimal.Parse(earnedString.Replace("$", "").Trim());
            return earned;
        }

        private TimeSpan ParseTripTime(string input)
        {
            if (string.IsNullOrEmpty(input)) return TimeSpan.Zero;
            int hours = 0;
            int minutes = 0;
            int seconds = 0;
            Regex durationRegex = RegexManager.GetRegex(RegexType.Duration);
            Regex numberRegex = RegexManager.GetRegex(RegexType.Number);
            Match durationMatch = durationRegex.Match(input);

            if (durationMatch != null)
            {
                hours = durationMatch.Groups[1].Success ? int.Parse(numberRegex.Match(durationMatch.Groups[1].Value)?.Value?.Trim() ?? "0") : 0;
                minutes = durationMatch.Groups[2].Success ? int.Parse(numberRegex.Match(durationMatch.Groups[2].Value)?.Value?.Trim() ?? "0") : 0;
                seconds = durationMatch.Groups[3].Success ? int.Parse(numberRegex.Match(durationMatch.Groups[3].Value)?.Value?.Trim() ?? "0") : 0;
            }
            return new TimeSpan(hours, minutes, seconds);
        }

        private TimeOnly ParseRequestTime(string input)
        {
            if (string.IsNullOrEmpty(input)) return TimeOnly.MinValue;
            Regex timeRegex = RegexManager.GetRegex(RegexType.Time);
            Match timeMatch = timeRegex.Match(input);

            int tHours = 0;
            int tMinutes = 0;
            if (timeMatch != null)
            {
                tHours = timeMatch.Groups[1].Success ? int.Parse(timeMatch.Groups[1].Value?.Trim() ?? "0") : 0;
                tMinutes = timeMatch.Groups[2].Success ? int.Parse(timeMatch.Groups[2].Value?.Trim() ?? "0") : 0;
                if (timeMatch.Groups[3].Success && timeMatch.Groups[3].Value.Equals("PM", StringComparison.OrdinalIgnoreCase))
                {
                    tHours += 12;
                }
            }
            TimeOnly time = new TimeOnly(tHours, tMinutes);
            return time;
        }

        private DateTime ParseRequestDate(string input)
        {
            if (string.IsNullOrEmpty(input)) return DateTime.MinValue;
            Regex dateRegex = RegexManager.GetRegex(RegexType.Date);
            Match dateMatch = dateRegex.Match(input);
            int day = 0;
            int month = 0;
            if (dateMatch != null)
            {
                day = dateMatch.Groups[3].Success ? int.Parse(dateMatch.Groups[3].Value?.Trim() ?? "0") : 0;
                month = dateMatch.Groups[2].Success ? GetMonthNumberFromAbbreviation(dateMatch.Groups[2].Value?.Trim() ?? "0") : 0;
            }

            DateTime requestTime = new DateTime(DateTime.Now.Year, month, day, 0, 0, 0);
            return requestTime;
        }

        private int ParsePointsEarned(string input)
        {
            if (string.IsNullOrEmpty(input)) return 0;
            Regex pointsRegex = RegexManager.GetRegex(RegexType.Number);
            Match pointsMatch = pointsRegex.Match(input);
            int points = 0;
            if(pointsMatch != null && !string.IsNullOrEmpty(pointsMatch.Value))
            {
                points = int.Parse(pointsMatch.Value);
            }
            return points;
        }

        private List<CustomerPayment> ParseCustomerPayments(int startingLine)
        {
            List<CustomerPayment> customerPayment = new List<CustomerPayment>();
            if (startingLine < 0) return customerPayment;
            startingLine++;
            //Check if single or multi customer order
            Regex customerRegex = RegexManager.GetRegex(RegexType.Customer);
            Match customerMatch = customerRegex.Match(OrderLines[startingLine]);
            bool moreThanOneCustomer = false;
            if(customerMatch != null)
            {
                //Group 2 checks for the customer number on the line "Customer 1" or "Customer 2", this line is not present on single customer orders
                moreThanOneCustomer = customerMatch.Groups[2].Success;
            }

            if (!moreThanOneCustomer)
            {
                //Get Customer Paytments
                CustomerPayment customer = ParseCustomerPayment(startingLine, startingLine + 3, moreThanOneCustomer, out int finishingLine);
                //Get Customer Payments to Uber
                int paidToUberSection = GetLineNumber(RegexManager.GetRegex(RegexType.PaidToUber), startingLine);
                customer.PaidToUber = ParseMoney(GetLine(RegexManager.GetRegex(RegexType.Money), paidToUberSection));
                customerPayment.Add(customer);
            }
            else
            {
                //Get Multiple Customer Paytments
                int currentLine = GetLineNumber(RegexManager.GetRegex(RegexType.Customer), startingLine);
                int sectionEnd = GetLineNumber(RegexManager.GetRegex(RegexType.Total), startingLine);
                while (currentLine > 0 && currentLine < sectionEnd)
                {
                    customerPayment.Add(ParseCustomerPayment(currentLine, sectionEnd, moreThanOneCustomer, out int finishingLine));
                    currentLine = Math.Max(GetLineNumber(RegexManager.GetRegex(RegexType.Customer), currentLine + 2), finishingLine);
                }
                //Get Multiple Customers Payments to Uber
                currentLine = GetLineNumber(RegexManager.GetRegex(RegexType.PaidToUber), startingLine);
                sectionEnd = GetLineNumber(RegexManager.GetRegex(RegexType.Total), currentLine);
                int i = 0;
                while (currentLine > 0 && currentLine < sectionEnd && i < customerPayment.Count)
                {
                    CustomerPayment customer = customerPayment[i];
                    currentLine = GetLineNumber(RegexManager.GetRegex(RegexType.ServiceFee), currentLine);
                    customer.PaidToUber = ParseMoney(OrderLines[currentLine]);
                    currentLine = GetLineNumber(RegexManager.GetRegex(RegexType.Customer), currentLine);
                    i++;
                }
            }
            return customerPayment;
        }

        private CustomerPayment ParseCustomerPayment(int startingLine, int sectionEnd, bool isMultipleCustomer, out int finishingLine)
        {
            finishingLine = startingLine;
            if (startingLine < 0) return null;
            CustomerPayment customerPayment = new CustomerPayment();
            int totalLine = 0;
            int priceLineStart = 0;

            if (isMultipleCustomer)
            {
                totalLine = GetLineNumber(RegexManager.GetRegex(RegexType.Customer), startingLine);
                priceLineStart = totalLine;
            }
            else
            {
                totalLine = GetLineNumber(RegexManager.GetRegex(RegexType.Total), startingLine);
                priceLineStart = startingLine;
            }
            if (totalLine > 0 && totalLine < sectionEnd)
            {
                customerPayment.Total = ParseMoney(OrderLines[totalLine]);
            }

            int priceLine = GetLineNumber(RegexManager.GetRegex(RegexType.CustomerPrice), priceLineStart);
            if(priceLine > 0 && priceLine < sectionEnd)
            {
                customerPayment.Price = ParseMoney(OrderLines[priceLine]);
            }

            int tipLine = GetLineNumber(RegexManager.GetRegex(RegexType.Tip), priceLine);
            if (tipLine > 0 && tipLine < sectionEnd)
            {
                customerPayment.Tip = ParseMoney(OrderLines[tipLine]);
            }
            finishingLine = Math.Max(tipLine, priceLine);
            return customerPayment;
        }
    }
}
