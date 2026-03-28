using System;
using System.Collections.Generic;
using System.Text;

namespace UDCG.Application.Feature.Users.Resources
{
    public class UserParams
    {
        private const int MaxPageSize = 50;

        public int PageNumber { get; set; } = 1;

        private int _pageSize = 10;

        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = (value > MaxPageSize) ? MaxPageSize : value;
        }


        public string Faculty { get; set; }
        public string Department { get; set; }
        public string Campus { get; set; }
        public string Username { get; set; }

    }
}
