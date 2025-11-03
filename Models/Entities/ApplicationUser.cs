using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NRLWebApp.Models.Entities
{
    public class ApplicationUser : IdentityUser
    {
        [PersonalData] 
        [StringLength(50)]
        public string Fornavn { get; set; }

        [PersonalData]
        [StringLength(50)]
        public string Etternavn { get; set; }

        public int OrganisasjonID { get; set; }

        [ForeignKey("OrganisasjonID")]
        public virtual Organisasjon Organisasjon { get; set; }

        public virtual ICollection<Hinder> Hindre { get; set; }
    }
}
