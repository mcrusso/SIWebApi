using System;
using System.Linq;
using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using Microsoft.Owin.Security.OAuth;
using System.ComponentModel;

namespace SIWebApi.Mapping
{
    [Table("AspNetUsers")]
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(50)]
        [Display(Name = "First name")]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Last name")]
        public string LastName { get; set; }

        [StringLength(16)]
        public string FiscalCode { get; set; }

        [StringLength(13)]
        public string VAT { get; set; }

        [StringLength(10)]
        public string Sex { get; set; }

        public DateTime? BirthDate { get; set; }

        [StringLength(500)]
        public string BirthPlace { get; set; }

        [StringLength(100)]
        public string Nationality { get; set; }

        [StringLength(500)]
        public string Address { get; set; }

        [StringLength(10)]
        public string AddressNumber { get; set; }

        [StringLength(100)]
        public string City { get; set; }

        [StringLength(100)]
        public string Province { get; set; }

        [StringLength(5)]
        public string PostalCode { get; set; }

        [StringLength(100)]
        public string State { get; set; }

        [StringLength(100)]
        public string CoordX { get; set; }

        [StringLength(100)]
        public string CoordY { get; set; }

        [StringLength(50)]
        public string Phone { get; set; }

        [StringLength(50)]
        public string Mobile { get; set; }

        public string PictureUrl { get; set; }

        public string Metadata { get; set; }

        [DefaultValue(false)]
        public bool IsMerchant { get; set; }

        public DateTime CreationDate { get; set; }

        public DateTime LastUpdate { get; set; }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager, string authenticationType = null)
        {
            if (string.IsNullOrEmpty(authenticationType))
                authenticationType = DefaultAuthenticationTypes.ApplicationCookie;
            return await manager.CreateIdentityAsync(this, authenticationType);
        }
    }
}