using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Web.Core.Dto
{
    public class OrderByDto
    {
        public string Name { get; set; }
        public string value { get; set; }

        public OrderByDto(string name, string value)
        {
            this.Name = name;
            this.value = value;
        }
    }
}
