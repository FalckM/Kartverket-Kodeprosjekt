using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NRLWebApp.Models.Entities
{
    public class Status
    {
        [Key]
        public int StatusID { get; set; }

        [Required]
        [StringLength(50)]
        public string Navn { get; set; } 

        public virtual ICollection<Hinder> Hindre { get; set; }
    }
}
