using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace OFFSIDESHOP
{
    public partial class Login : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }
        protected void btnEntrar_Click(object sender, EventArgs e)
        {
            if (TxtContra.Text != "" && TxtUsuario.Text != "")
            {
                string usuarioadmin = TxtUsuario.Text.Trim();
                string contraadmin = TxtContra.Text.Trim();


                string contra, usuario;
                contra = EncryptString(TxtContra.Text, initVector);
                usuario = TxtUsuario.Text;
                datos1.valorGlobal = usuario;
                MySqlConnection conexion = new MySqlConnection("Server=127.0.0.1; database= drugstore_portillo;Uid = root; pwd = Info2026/*- ");
            var cmd = "SELECT Id_Usuario from usuarios WHERE Nombre_Usuario='" + usuario + "' AND Password = '" + contra + "'; ";
            MySqlCommand comando = new MySqlCommand(cmd, conexion);
                conexion.Open();
                int retorno = Convert.ToInt32(comando.ExecuteScalar());
                if (retorno != 0)
                {
                    Session["usermane"] = TxtUsuario;
                    Response.Redirect("Homepage.aspx");
                }
                else
                {
                    if (usuarioadmin == "admin" && contraadmin == "1234")
                    {
                        Session["username"] = usuario;

                        // Rediriges a la página protegida
                        Response.Redirect("Dashboard.aspx");
                    }
                    else
                    {
                        alerta.Text = "<script>Swal.fire('Something went wrong', 'User or password are incorrect', 'error') </script>";
                        TxtContra.Text = "";
                        TxtUsuario.Text = "";
                    }
                }
            }
            else
            {
                alerta.Text = "<script>Swal.fire('OOPS', 'Do not leave any blank spaces', 'error') </ script > ";
            }
        }
        private const string initVector = "emmanuelinfo2025";
        // This constant is used to determine the keysize of the encryption algorithm
        private const int keysize = 256;
        //Encrypt
        public static string EncryptString(string plainText, string passPhrase)
        {
            byte[] initVectorBytes = Encoding.UTF8.GetBytes(initVector);
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            PasswordDeriveBytes password = new PasswordDeriveBytes(passPhrase, null);
            byte[] keyBytes = password.GetBytes(keysize / 8);
            RijndaelManaged symmetricKey = new RijndaelManaged();
            symmetricKey.Mode = CipherMode.CBC;
            ICryptoTransform encryptor = symmetricKey.CreateEncryptor(keyBytes, initVectorBytes);
            MemoryStream memoryStream = new MemoryStream();
            CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
            cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
            cryptoStream.FlushFinalBlock();
            byte[] cipherTextBytes = memoryStream.ToArray();
            memoryStream.Close();
            cryptoStream.Close();
            return Convert.ToBase64String(cipherTextBytes);
        }
        protected void Button1_Click(object sender, EventArgs e)
        {
            Response.Redirect("Registro.aspx");
        }
        //protected void btnEntrar_Click(object sender, EventArgs e)
        //{
        //    string usuario = TxtUsuario.Text.Trim();
        //    string contra = TxtContra.Text.Trim();

        //    // Aqui tu logica de autenticacion
        //    if (usuario == "admin" && contra == "1234")
        //    {
        //        Response.Redirect("Dashboard.aspx");
        //    }
        //    else
        //    {
        //        alerta.Text = "<script>Swal.fire('Error', 'Usuario o contraseña incorrectos', 'error');</script>";
        //    }
        //}

        protected void btnregistro_Click(object sender, EventArgs e)
        {
            Response.Redirect("Registro.aspx");
        }
    }
}