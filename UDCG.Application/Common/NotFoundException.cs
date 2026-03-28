using System;
using System.Collections.Generic;
using System.Text;

namespace UDCG.Application.Common
{
    public class NotFoundException : Exception
    {
        public NotFoundException(object error)
            : base($"{error})")
        {
        }
    }
}
