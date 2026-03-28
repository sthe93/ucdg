using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Reflection;
using System.Text;


namespace UCDG.Persistence
{
    public  class DbContextExtensions
    {
        private static IServiceProvider _serviceProvider;
        public DbContextExtensions(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        //public static bool IsDisposed(this DbContext context)
        //{
        //    try
        //    {
        //        var dbContextType = typeof(DbContext);
        //        var databaseProperty = dbContextType.GetProperty("Database", BindingFlags.Public | BindingFlags.Instance);
        //        var database = databaseProperty.GetValue(context);
        //        return false;
        //    }
        //    catch (ObjectDisposedException)
        //    {
        //        return true;
        //    }
        //}

        public static UCDGDbContext EnsureDbContext()
        {


            using (var scope = _serviceProvider.CreateScope())
            {
                return scope.ServiceProvider.GetRequiredService<UCDGDbContext>();
            }
        }
    }
}
