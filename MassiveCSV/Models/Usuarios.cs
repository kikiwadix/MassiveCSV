using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MassiveCSV.Models
{
    public class Usuarios
    {
        [Key]
        [Required]
        [Display(Name = "Correo electrónico")]
        [DataType(DataType.EmailAddress, ErrorMessage = "Email no válido")]
        //[EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; }

        public Usuarios(String Email, String Password)
        {
            this.Email = Email;
            this.Password = Password;
        }
    }
}