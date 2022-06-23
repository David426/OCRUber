using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCRUber.Models
{
    public class Address
    {
        public string? Street { get; set; }
        public string? Town { get; set; }
        public string? State { get; set; }
        public string? Zip { get; set; }
        public string? Country { get; set; }

        public override string ToString()
        {
            return $"{Street}, {Town}, {State} {Zip}, {Country}";
        }

    }
}
