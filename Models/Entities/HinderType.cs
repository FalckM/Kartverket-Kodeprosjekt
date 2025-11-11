using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NRLWebApp.Models.Entities
{
    public class HinderType
    {
        [Key]
        public int HinderTypeID { get; set; }

        [Required]
        [StringLength(50)]
        public string Navn { get; set; }

        [StringLength(255)]
        public string Beskrivelse { get; set; }

        public virtual ICollection<Hinder> Hindre { get; set; }
    }
}