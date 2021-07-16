using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Accounting.Models.Interfaces
{
    public interface IDeletable
    {
        DateTime? DeleteTime { get; set;}
    }
}
