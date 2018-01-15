using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace MassiveCSV.Models
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Usuarios> Usuarios { get; set; }
        public DbSet<UsuariosCSVerroneos> UsuariosCSVerroneos { get; set; }
        
    }
}