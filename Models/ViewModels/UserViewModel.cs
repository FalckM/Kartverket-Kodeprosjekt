using System.Collections.Generic;

namespace NRLWebApp.Models.ViewModels
{
    public class UserViewModel
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        public string Fornavn { get; set; }
        public string Etternavn { get; set; }
        public string OrganisasjonNavn { get; set; }
        public IList<string> Roller { get; set; }
    }
}