using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace sample1.Models
{
    public class TestData
    {
        [Key]
        public int kid { get; set; }

        public string Name { get; set; }
    }
}
