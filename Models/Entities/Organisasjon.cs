using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NRLWebApp.Models.Entities
{
    public class Organisasjon
    {
        [Key] 
        public int OrganisasjonID { get; set; }

        [Required] 
        [StringLength(100)] 
        public string Navn { get; set; }

        public virtual ICollection<ApplicationUser> Brukere { get; set; }
    }
}
