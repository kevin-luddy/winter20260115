using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IES.Common
{
    public class IESResponse<T>
    {
        public bool Status { get; set; }

        public ICollection<string> Messages { get; set; } = new List<string>();

        public ICollection<T> Data { get; set; } = new List<T>();
    }
}
