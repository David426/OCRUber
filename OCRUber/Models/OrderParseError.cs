using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace OCRUber.Models
{
    public enum OrderParseErrorTypes
    {
        NO_VALUE,
        INCORRECT_VALUE,
        OTHER
    }
    public class OrderParseError
    {
        public string PropertyName { get; set; }
        public PropertyInfo Property { get; set; }
        public object? Value { get; set; }
        public string Description { get; set; }
        public OrderParseErrorTypes Type { get; set; }
    }
}
