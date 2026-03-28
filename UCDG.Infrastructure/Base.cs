using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace UCDG.Infrastructure
{
    public class Base
    {
        public static string Url()
        {
            var _configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            return _configuration["ExternalApi:ITSAPI"];
        }
    }
}
