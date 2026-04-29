using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace OFFSIDESHOP
{
    public partial class EditarProducto : System.Web.UI.Page
    {
        MySqlConnection conec = new MySqlConnection("server=127.0.0.1; database=offsideshop; Uid=root; pwd=Info2026/*/-;");
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["username"] == null)
            {
                Response.Redirect("Inicio.aspx");
            }
            conec.Open();
            MySqlCommand cmd = conec.CreateCommand();
            cmd.CommandText = "select * from productos";
            cmd.ExecuteNonQuery();
            DataTable dt = new DataTable();
            MySqlDataAdapter da = new MySqlDataAdapter(cmd);
            da.Fill(dt);
            gvdlista.DataSource = dt;
            gvdlista.DataBind();
            conec.Close();
        }
        protected void Unnamed2_Click(object sender, EventArgs e)
        {
            if (txtmarca.Text.Trim() != "" && txtproducto.Text.Trim() != "" && txtprecio.Text.Trim() != "" && txtcantidad.Text.Trim() != "" && txtid.Text.Trim() != "")
            {

                MySqlConnection conexion = datos.ObtenerConexion();
                string query = "UPDATE productos SET Marca = @marca,  Producto = @producto, Precio = @precio, Cantidad=@cantidad WHERE ID=@id";
                MySqlCommand comando = new MySqlCommand(query, conexion);

                comando.Parameters.AddWithValue("@marca", txtmarca.Text);
                comando.Parameters.AddWithValue("@producto", txtproducto.Text);
                comando.Parameters.AddWithValue("@precio", txtprecio.Text);
                comando.Parameters.AddWithValue("@cantidad", txtcantidad.Text);
                comando.Parameters.AddWithValue("@ID", txtid.Text);
                comando.ExecuteNonQuery();
                conexion.Close();
                conec.Open();
                MySqlCommand cmd = conec.CreateCommand();
                cmd.CommandText = "select * from productos";
                cmd.ExecuteNonQuery();
                DataTable dt = new DataTable();
                MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                da.Fill(dt);
                gvdlista.DataSource = dt;
                gvdlista.DataBind();
                conec.Close();

                txtmarca.Text = "";
                txtproducto.Text = "";
                txtprecio.Text = "";
                txtcantidad.Text = "";
                txtid.Text = "";
                alerta.Text = "<script>Swal.fire('Su producto se Editado con exito.', '¡Gracias por preferirnos!', 'success'); </script>";
            }
            else
            {
                alerta.Text = "<script>Swal.fire('OOPS', 'No deje espacios en blanco', 'error') </script>";
            }
        }
        protected void Unnamed1_Click(object sender, EventArgs e)
        {
            try
            {
                MySqlConnection conexion = datos.ObtenerConexion();
                MySqlCommand comand = new MySqlCommand("SELECT * FROM productos WHERE ID=@ID", conexion);
                comand.Parameters.AddWithValue("@ID", txtid.Text);
                MySqlDataReader registro = comand.ExecuteReader();
                if (registro.Read())
                {
                    alerta.Text = "<script>Swal.fire('Be successfully selected.', '', 'success'); </script>";

                    txtmarca.Text = registro["Marca"].ToString();
                    txtprecio.Text = registro["Precio"].ToString();
                    txtproducto.Text = registro["Producto"].ToString();
                    txtcantidad.Text = registro["Cantidad"].ToString();
                }
                conexion.Close();
            }
            catch
            {
                alerta.Text = "<script>Swal.fire('Something went wrong. Try again.', 'Verify that the ID is correct.', 'error') </script>";

            }
        }
        protected void btnInicio_Click(object sender, EventArgs e)
        {
            Response.Redirect("Inicio.aspx");
        }
    }
}