using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MassiveCSV.Models
{
    public class UsuariosCSVerroneos
    {
        public UsuariosCSVerroneos(String UserName, String Password, String Email, String Error)
        {
            this.UserName = UserName;
            this.Password = Password;
            this.Email = Email;
            this.Error = Error;
        }

        public UsuariosCSVerroneos()
        {

        }

        public int Id { get; set; }
        public String UserName { get; set; }
        public String Password { get; set; }
        public String Email { get; set; }
        public String Error { get; set; }
    }
}