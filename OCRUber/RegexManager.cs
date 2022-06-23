using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OCRUber.Properties;

namespace OCRUber
{
    public enum RegexType
    {
        Address,
        AddressStart,
        BlankLine,
        TripDetails,
        Money,
        Number,
        Duration,
        TimeRequested,
        DateRequested,
        Time,
        Date,
        PointsEarned,
        TipIncluded,
        Fare,
        Base,
        TripSupplement,
        CustomerPayments,
        Customer,
        CustomerPrice,
        Tip,
        Total
    }
    public class RegexManager
    {
        Dictionary<string, string> RegexesPatterns;
        Dictionary<RegexType, Regex> Regexes;

        public RegexManager()
        {
            Regexes = new Dictionary<RegexType, Regex>();
            try
            {
                string json = Encoding.UTF8.GetString(Resources.Regex);
                RegexesPatterns = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            }catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
            }
            finally{
                if (RegexesPatterns == null)
                {
                    Console.Error.WriteLine("Regex could not be loaded from json");
                    RegexesPatterns = new Dictionary<string, string>();
                }
            }
        }

        public Regex GetRegex(RegexType type) 
        {
            if (!Regexes.ContainsKey(type))
            {
                Regexes.Add(type, new Regex(RegexesPatterns[type.ToString().ToLower()]));
            }
            return Regexes[type];
        } 


    }
}
