using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace sample1.Models
{
    public class TestData
    {
        [Key]
        public int kid { get; set; }

        public string Name { get; set; }

        public DateTime CreatedTime { get; set; }
    }

    public class TestData2
    {
        [Key]
        public int aid { get; set; }

        public int kid { get; set; }

        public string Name { get; set; }
    }

    public class TestData3
    {
        [Key]
        public int Bid { get; set; }

        public int aid { get; set; }

        public string Name { get; set; }
    }

    public class ViewData
    {
        public int kid { get; set; }

        //[Column("TestData.Name")]
        public string Name { get; set; }

       // [Column("TestData2.Name")]
        public string Name2 { get; set; }

        public int aid { get; set; }
    }
}

