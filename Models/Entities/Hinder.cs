using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NRLWebApp.Models.Entities 
{
    public class Hinder
    {
        [Key]
        public int HinderID { get; set; }

        [Display(Name = "Type")]
        public int HinderTypeID { get; set; }
        [ForeignKey("HinderTypeID")]
        public virtual HinderType HinderType { get; set; }

        [Column(TypeName = "decimal(8, 2)")]
        public decimal Hoyde { get; set; }

        [StringLength(255)]
        public string Beskrivelse { get; set; }

        public string Lokasjon { get; set; }

        public DateTime Tidsstempel { get; set; }

        public int StatusID { get; set; }
        [ForeignKey("StatusID")]
        public virtual Status Status { get; set; }

        public string ApplicationUserId { get; set; }
        [ForeignKey("ApplicationUserId")]
        public virtual ApplicationUser ApplicationUser { get; set; }

        public virtual ICollection<Behandling> Behandlinger { get; set; }
    }
}