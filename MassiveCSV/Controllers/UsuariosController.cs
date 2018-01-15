using MassiveCSV.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace MassiveCSV.Controllers
{
    public class UsuariosController : Controller
    {

        public ActionResult Index()
        {
            return View();
        }

        // GET: Usuarios
        [HttpPost]
        public ActionResult RegistroMasivo(HttpPostedFileBase file)
        {

            try
            {
                // Para guardar el archivo en local
                string directory = @"C:\CSV\";

                // Si no existe la carpeta, la crea
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                    Debug.WriteLine("The directory was created successfully at {0}.", Directory.GetCreationTime(directory));
                }

                var fileName = String.Empty;
                if (file != null && file.ContentLength > 0) // Se comprueba que haya archivo y tenga algo dentro
                {
                    // Comprobamos la extension del archivo
                    var supportedTypes = new[] { "csv", "txt" };
                    var fileExt = System.IO.Path.GetExtension(file.FileName).Substring(1);

                    if (!supportedTypes.Contains(fileExt))
                    {
                        ViewBag.Message = "Tipo de archivo inválido. Sólo se soportan los tipos *.csv y *.txt";
                        return View("Index");
                    }

                    // Si todo bien, guardamos el archivo
                    fileName = Path.GetFileName(file.FileName);
                    file.SaveAs(Path.Combine(directory, fileName));
                }
                ////////////////////////////////////////


                // Leer el archivo CSV guardado - CAMPOS(userName, password[req], email[req])
                int lineCounter = 0;
                string line;
                List<Usuarios> usuarios = new List<Usuarios>();
                List<String> lineasFallidas = new List<String>();
                List<int> numLineaFallida = new List<int>();

                System.IO.StreamReader fileReaded = new System.IO.StreamReader(@directory + fileName);
                while ((line = fileReaded.ReadLine()) != null)
                {
                    lineCounter++;
                    String[] separatedData = line.Split(',');

                    // Compruebo que haya 3 campos, que los campos importantes (email y pass) no estén vacios, y que el mail sea válido
                    if (separatedData.Length == 3 && (separatedData[1].Trim() != "" && separatedData[2].Trim() != "") && ValidarEmail(separatedData[2]) )
                    {
                        // Comprobación de que no exista el mismo email en la lista de usuarios
                        // Ver si se hace aqui, o en la base de datos
                        bool error = false;
                        foreach (Usuarios user in usuarios)
                        {
                            if (separatedData[2] == user.Email)
                            {
                                error = true;
                            }                      
                        }

                        // Si todo correcto, añado usuario
                        if (!error)
                        {
                            usuarios.Add(new Usuarios(separatedData[2], separatedData[1]));
                        }
                        else
                        {
                            // Si hay fallo por repetición de Email, añado linea a lineasFallidas
                            line += " ** Error: Email ya registrado anteriormente en el archivo *.CSV.";
                            lineasFallidas.Add(line);
                            numLineaFallida.Add(lineCounter);
                        }   

                    }
                    else
                    {
                        // Si hay fallo, añado linea a lineasFallidas
                        line += " ** Error: No existen todos los campos requeridos, o los campos son incorrectos.";
                        lineasFallidas.Add(line);
                        numLineaFallida.Add(lineCounter);
                    }

                }

                fileReaded.Close();

                ////////////////////////////////////////////////////////////
                ////////////    BD
                using (ApplicationDbContext bd = new ApplicationDbContext())
                {
                    
                    foreach (Usuarios user in usuarios)
                    {
                        // Agregar los usuarios al DbSet
                        try
                        {
                            bd.Usuarios.Add(user);
                            // Enviar cambios a la Base de Datos (uno por uno, para que salte excepción de email repetido)
                            bd.SaveChanges();
                        }
                        catch (System.Data.Entity.Infrastructure.DbUpdateException ex) // Excepcion si la PK email está repetida en la BD
                        {
                            Debug.WriteLine("Email ya registrado en la base de datos");
                            // Vacio el contexto de BD, sino sigue dando problemas
                            bd.Usuarios.Remove(user);
                            // añado linea a lineasFallidas
                            string lin = "Email: " + user.Email;
                            lin += " ** Error: Email ya registrado anteriormente en la base de datos.";
                            lineasFallidas.Add(lin);
                            lineCounter++; // Solo para la impresión debug
                            numLineaFallida.Add(lineCounter);
                        }
                        catch (Exception ex)
                        {                       
                            Debug.WriteLine("Excepción por defecto al guardar datos en BD");  
                        }

                    }
                    
                    // Ahora las lineas erroneas
                    foreach (string linea in lineasFallidas)
                    {
                        UsuariosCSVerroneos e = new UsuariosCSVerroneos();
                        e.Error = linea;
                        bd.UsuariosCSVerroneos.Add(e);
                    }
                    bd.SaveChanges();

                }
                ///////// fin BD

                // Cambiar los debugs por guardado en archivo o base de datos
                Debug.WriteLine("Se han leido {0} usuarios correctamente.", usuarios.Count);
                foreach (var user in usuarios)
                {
                    Debug.WriteLine("Email: {0}, Pass: {1}", user.Email, user.Password);

                }

                Debug.WriteLine("Se han encontrado {0} instancias erróneas.", lineasFallidas.Count );

                for (int i = 0; i < lineasFallidas.Count; i++)
                {
                    Debug.WriteLine("Num. linea: {0}, Contenido: {1}", numLineaFallida[i], lineasFallidas[i]);
                }

                return RedirectToAction("Index");
                //return View("Index"); parece que se puede usar cualquiera de los 2 return
            }
            catch(System.IO.DirectoryNotFoundException ex)
            {
                ViewBag.Message = "No se ha seleccionado ningún archivo o el archivo está vacío.";
                return View("Index");
            } 
            catch (Exception ex)
            {
                ViewBag.Message = "Error, por favor, inténtelo de nuevo.";
                Debug.WriteLine(ex);
                return View("Index");
            }           

        }

        public static bool ValidarEmail(string email)
        {
            if (email == null || email == "") return false;
            //Regex oRegExp = new Regex(@"^[A-Za-z0-9_.\-]+@[A-Za-z0-9_\-]+\.([A-Za-z0-9_\-]+\.)*[A-Za-z][A-Za-z]+$", RegexOptions.IgnoreCase);
            //return oRegExp.Match(email).Success;


            //Solucion provisional, compruebo lo básico
            String[] mailParts = email.Split('@');
            
            if (mailParts[0].Trim() != "" && mailParts[1].Contains('.') )
            {
                return true;
            }
            else
            {
                return false;
            }


            //string expresion = "\\w+([-+.']\\w+)*@\\w+([-.]\\w+)*\\.\\w+([-.]\\w+)*";

            //if (Regex.IsMatch(email, expresion))
            //{
            //    if (Regex.Replace(email, expresion, String.Empty).Length == 0)
            //    { return true; }
            //    else
            //    { return false; }
            //}
            //else
            //{ return false; }
        }
    }
}