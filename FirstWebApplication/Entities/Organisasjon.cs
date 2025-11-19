using System.ComponentModel.DataAnnotations;

namespace FirstWebApplication.Entities
{
    /// <summary>
    /// Representerer en organisasjon (f.eks. flyselskap, luftfartstilsyn)
    /// </summary>
    public class Organisasjon
    {
        [Key]
        public long Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [EmailAddress]
        [StringLength(255)]
        public string? ContactEmail { get; set; }

        [StringLength(20)]
        public string? ContactPhone { get; set; }

        [StringLength(255)]
        public string? Address { get; set; }

        public bool Active { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Navigation: En organisasjon har mange brukere
        public ICollection<ApplicationUser>? Users { get; set; }
    }
}