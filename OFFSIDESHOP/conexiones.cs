using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OFFSIDESHOP
{
    public class conexiones
    {//******************Método para verificar si se repiten los usuario *******************
        public static int UsuariosRepetidos(string usuario, string contra, string nombre, string apellido,
        string correo)
        {
            int valorUsuario = 0;
            int valorCorreo = 0;
            MySqlConnection conexion = datos.ObtenerConexion();
            // Verificar si el nombre de usuario ya existe
            MySqlCommand cmdUsuario = new MySqlCommand("SELECT Id_Usuario FROM usuarios WHERE Nombre_Usuario = '" + usuario + "'", conexion);
            valorUsuario = Convert.ToInt32(cmdUsuario.ExecuteScalar());
            // Verificar si el correo ya existe
            MySqlCommand cmdCorreo = new MySqlCommand("SELECT Id_Usuario FROM usuarios WHERE Correo = '" + correo + "'", conexion);
            valorCorreo = Convert.ToInt32(cmdCorreo.ExecuteScalar());
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
                // Agregar nuevo usuario
                conexiones.AgregarUsuario(nombre, apellido, usuario, contra, correo);
            }
            conexion.Close();
            return valorCorreo;
        }
        //**************************** Método para los usuario ****************************
        public static int AgregarUsuario(string nombre, string apellido, string usuario, string contra,
        string correo)
        {
            int retorno = 0;
            MySqlCommand comando = new MySqlCommand(string.Format("Insert into usuarios (Nombre,Apellido, Nombre_Usuario, Password, Correo) values('{0}', '{1}', '{2}', '{3}', '{4}')", nombre,
            apellido, usuario, contra, correo), datos.ObtenerConexion());
            retorno = comando.ExecuteNonQuery();
            return retorno;
        }
        //**************************** Método para agregar producto ****************************
        public static int agregar(Agregar pAlumno)
        {
            int retorno = 0;
            MySqlCommand comado = new MySqlCommand(string.Format("Insert into productos (ID, Marca, Producto, Precio, Cantidad) values('{0}', '{1}', '{2}', '{3}', '{4}')", pAlumno.Id, pAlumno.Marca,
            pAlumno.Producto, pAlumno.Precio, pAlumno.Cantidad), datos.ObtenerConexion());
            retorno = comado.ExecuteNonQuery();
            return retorno;
        }
    }
}