using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdminFileStorage.Models
{
    public class File
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Extension { get; set; }
        public string Size { get; set; }
        public string Path { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
