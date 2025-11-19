using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace FirstWebApplication.Entities
{
    /// <summary>
    /// Utvider IdentityUser med tilleggsfelt for organisasjon
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        // FK til Organisasjon
        public long? OrganisasjonId { get; set; }

        // Navigation property
        public Organisasjon? Organisasjon { get; set; }
    }
}