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
    public partial class AgregarProducto : System.Web.UI.Page
    {
      
MySqlConnection conec = new MySqlConnection("server=127.0.0.1; database=offsideshop; Uid=root;pwd = Info2026/*-");
protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["username"] == null)
            {
                Response.Redirect("Login.aspx");
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
        protected void Unnamed1_Click(object sender, EventArgs e)
        {
            if (txtmarca.Text.Trim() != "" && txtproducto.Text.Trim() != "" && txtprecio.Text.Trim() != ""
            && txtcantidad.Text.Trim() != "")
            {
                try
                {
                    Agregar pAlumno = new Agregar();
                    pAlumno.Marca = txtmarca.Text.Trim();
                    pAlumno.Producto = txtproducto.Text.Trim();
                    pAlumno.Precio = txtprecio.Text.Trim();
                    pAlumno.Cantidad = txtcantidad.Text.Trim();
                    int resultado = conexiones.agregar(pAlumno);
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
                    alerta.Text = "<script>Swal.fire('Your product was successfully added.', 'Thank you for choosing us!', 'success'); </script>";
                txtproducto.Text = "";
                    txtprecio.Text = "";
                    txtcantidad.Text = "";
                    txtmarca.Text = "";
                }
                catch
                {
                    alerta.Text = "<script>Swal.fire('Something went wrong', 'Try again', 'error')          </ script > ";
                }
            }
            else
            {
                alerta.Text = "<script>Swal.fire('WHOOPS', 'Do not leave any blank spaces', 'error') </ script > ";
            }
        }
        protected void btnInicio_Click(object sender, EventArgs e)
        {
            Response.Redirect("Dashboard.aspx");
        }
    }
}