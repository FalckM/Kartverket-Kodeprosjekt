using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NRLWebApp.Models.Entities 
{
    public class Behandling
    {
        [Key]
        public int BehandlingID { get; set; }

        [StringLength(500)] 
        public string Kommentar { get; set; }

        public DateTime Tidsstempel { get; set; }

        public int HinderID { get; set; }
        [ForeignKey("HinderID")]
        public virtual Hinder Hinder { get; set; }

        public string ApplicationUserId { get; set; }
        [ForeignKey("ApplicationUserId")]
        public virtual ApplicationUser ApplicationUser { get; set; }
    }
}
