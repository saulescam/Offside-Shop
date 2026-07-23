using MySql.Data.MySqlClient;
using OFFSIDESHOP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OFFSIDESHOP
{
    public class connections
    {
        //******************Método para verificar si se repiten los usuarios *******************
        public static int UsuariosRepetidos(string usuario, string contra, string nombre, string apellido, string correo)
        {
            int valorUsuario = 0;
            int valorCorreo = 0;

            // Usamos tu clase data para obtener la conexión
            using (MySqlConnection conexion = data.ObtenerConexion())
            {
                try
                {
                    conexion.Open(); // ¡Abrimos la conexión!

                    // Verificar si el nombre de usuario ya existe (con parámetros seguros)
                    string queryUser = "SELECT IFNULL(Id_User, 0) FROM users WHERE Name_User = @usuario";
                    using (MySqlCommand cmdUsuario = new MySqlCommand(queryUser, conexion))
                    {
                        cmdUsuario.Parameters.AddWithValue("@usuario", usuario);
                        object result = cmdUsuario.ExecuteScalar();
                        valorUsuario = result != null ? Convert.ToInt32(result) : 0;
                    }

                    // Verificar si el correo ya existe (con parámetros seguros)
                    string queryMail = "SELECT IFNULL(Id_User, 0) FROM users WHERE Mail = @correo";
                    using (MySqlCommand cmdCorreo = new MySqlCommand(queryMail, conexion))
                    {
                        cmdCorreo.Parameters.AddWithValue("@correo", correo);
                        object result = cmdCorreo.ExecuteScalar();
                        valorCorreo = result != null ? Convert.ToInt32(result) : 0;
                    }

                    if (valorUsuario != 0)
                    {
                        // El nombre de usuario ya existe
                        return valorUsuario;
                    }
                    else if (valorCorreo != 0)
                    {
                        // El correo ya existe
                        return -1;
                    }
                    else
                    {
                        // Agregar nuevo usuario pasándole la conexión activa para no abrir otra innecesariamente
                        AgregarUsuario(nombre, apellido, usuario, contra, correo);
                        return 0; // Registro exitoso
                    }
                }
                catch (Exception ex)
                {
                    // Lanza el error para saber qué falló en el SQL si llega a pasar
                    throw new Exception("Error en UsuariosRepetidos: " + ex.Message);
                }
            } // El bloque 'using' cierra la conexión automáticamente aquí, pase lo que pase
        }

        //**************************** Método para agregar usuario ****************************
        public static int AgregarUsuario(string nombre, string apellido, string usuario, string contra, string correo)
        {
            int retorno = 0;
            string query = "INSERT INTO users (Name, Lastname, Name_User, Password, Mail) VALUES (@nombre, @apellido, @usuario, @contra, @correo)";

            using (MySqlConnection conexion = data.ObtenerConexion())
            {
                using (MySqlCommand comando = new MySqlCommand(query, conexion))
                {
                    comando.Parameters.AddWithValue("@nombre", nombre);
                    comando.Parameters.AddWithValue("@apellido", apellido);
                    comando.Parameters.AddWithValue("@usuario", usuario);
                    comando.Parameters.AddWithValue("@contra", contra);
                    comando.Parameters.AddWithValue("@correo", correo);

                    conexion.Open(); // ¡Importante abrirla antes de ejecutar!
                    retorno = comando.ExecuteNonQuery();
                }
            }
            return retorno;
        }

        //**************************** Método para agregar producto ****************************
        public static int agregar(Add pAlumno)
        {
            int retorno = 0;
            // Corregido: asumo que tu tabla de inventario se llama 'tshirts' según el análisis previo de tu BD, de lo contrario mantén 'products'
            string query = "INSERT INTO tshirts (ID, Brand, Product, Price, Amount) VALUES (@id, @brand, @product, @price, @amount)";

            using (MySqlConnection conexion = data.ObtenerConexion())
            {
                using (MySqlCommand comando = new MySqlCommand(query, conexion))
                {
                    comando.Parameters.AddWithValue("@id", pAlumno.Id);
                    comando.Parameters.AddWithValue("@brand", pAlumno.Marca);
                    comando.Parameters.AddWithValue("@product", pAlumno.Producto);
                    comando.Parameters.AddWithValue("@price", pAlumno.Precio);
                    comando.Parameters.AddWithValue("@amount", pAlumno.Cantidad);

                    conexion.Open();
                    retorno = comando.ExecuteNonQuery();
                }
            }
            return retorno;
        }

        //**************************** Método para eliminar producto ****************************
        public static int Eliminar(int pId)
        {
            int retorno = 0;
            string query = "DELETE FROM tshirts WHERE ID = @id";

            using (MySqlConnection conexion = data.ObtenerConexion())
            {
                using (MySqlCommand comando = new MySqlCommand(query, conexion))
                {
                    comando.Parameters.AddWithValue("@id", pId);

                    conexion.Open();
                    retorno = comando.ExecuteNonQuery();
                }
            }
            return retorno;
        }
    }
}