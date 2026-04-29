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
        // Cadena de conexión única
        string cadena = "server=127.0.0.1; database=offsideshop; Uid=root; pwd=Info2026/*-;";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["username"] == null)
            {
                Response.Redirect("Login.aspx");
            }

            if (!IsPostBack) // IMPORTANTE: Solo carga la tabla la primera vez
            {
                CargarTabla();
            }
        }

        void CargarTabla()
        {
            using (MySqlConnection con = new MySqlConnection(cadena))
            {
                con.Open();
                MySqlCommand cmd = new MySqlCommand("SELECT * FROM productos", con);
                MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                gvdlista.DataSource = dt;
                gvdlista.DataBind();
<<<<<<< HEAD
                con.Close();

                txtmarca.Text = "";
                txtproducto.Text = "";
                txtprecio.Text = "";
                txtcantidad.Text = "";
                txtid.Text = "";
                alerta.Text = "<script>Swal.fire('Product edited correctly.', 'Thanks', 'success'); </script>";
            }
            else
            {
                alerta.Text = "<script>Swal.fire('OOPS', 'Do not leave any blank spaces', 'error') </script>";

            }
        }

        // EVENTO BUSCAR/SELECCIONAR
        protected void btnSeleccionar_Click(object sender, EventArgs e)
        {
            using (MySqlConnection con = new MySqlConnection(cadena))
            {
                con.Open();
                MySqlCommand cmd = new MySqlCommand("SELECT * FROM productos WHERE ID=@id", con);
                cmd.Parameters.AddWithValue("@id", txtid.Text);
                MySqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
<<<<<<< HEAD
                    alerta.Text = "<script>Swal.fire('Succesful', '', 'success'); </script>";

                    txtmarca.Text = registro["Marca"].ToString();
                    txtprecio.Text = registro["Precio"].ToString();
                    txtproducto.Text = registro["Producto"].ToString();
                    txtcantidad.Text = registro["Cantidad"].ToString();
                }
                conexion.Close();
            }
            catch
            {
                alerta.Text = "<script>Swal.fire('Something went wrong', 'Verify ID', 'error') </script>";

=======
                    txtmarca.Text = dr["Marca"].ToString();
                    txtproducto.Text = dr["Producto"].ToString();
                    txtprecio.Text = dr["Precio"].ToString();
                    txtcantidad.Text = dr["Cantidad"].ToString();
                    alerta.Text = "<script>Swal.fire('Producto Cargado', '', 'success');</script>";
                }
                else
                {
                    alerta.Text = "<script>Swal.fire('Error', 'ID no encontrado', 'error');</script>";
                }
>>>>>>> a37049107b096c909dfaca4b204b5e610cce8eac
            }
        }

        // EVENTO EDITAR
        protected void btnEditar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtid.Text)) return;

            using (MySqlConnection con = new MySqlConnection(cadena))
            {
                con.Open();
                string sql = "UPDATE productos SET Marca=@m, Producto=@p, Precio=@pr, Cantidad=@c WHERE ID=@id";
                MySqlCommand cmd = new MySqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@m", txtmarca.Text);
                cmd.Parameters.AddWithValue("@p", txtproducto.Text);
                cmd.Parameters.AddWithValue("@pr", txtprecio.Text);
                cmd.Parameters.AddWithValue("@c", txtcantidad.Text);
                cmd.Parameters.AddWithValue("@id", txtid.Text);

                int result = cmd.ExecuteNonQuery();
                if (result > 0)
                {
                    alerta.Text = "<script>Swal.fire('Actualizado', 'Cambios guardados con éxito', 'success');</script>";
                    CargarTabla(); // Refresca la tabla después de editar
                }
            }
        }

        protected void btnInicio_Click(object sender, EventArgs e)
        {
            Response.Redirect("Dashboard.aspx");
        }
    }
}