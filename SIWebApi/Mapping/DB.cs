using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;


namespace SIWebApi.Mapping
{
    public class DB : IdentityDbContext<ApplicationUser>
    {
        public DB()
          : base("name=DefaultConnection")
        {
            //Configuration.ProxyCreationEnabled = false;
            //Configuration.LazyLoadingEnabled = false;
        }


        public static DB Create()
        {
            return new DB();
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            Database.SetInitializer<DB>(null);
            base.OnModelCreating(modelBuilder);
        }
    }
}