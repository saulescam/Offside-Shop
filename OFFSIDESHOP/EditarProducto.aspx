<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="EditarProducto.aspx.cs" Inherits="OFFSIDESHOP.EditarProducto" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <title>Editar Producto - OFFSIDESHOP</title>
    <link href="css/bootstrap.min.css" rel="stylesheet" />
    <link href="EstilosCss/EstilosStorage.css" rel="stylesheet" />
    <script src="SweetAlert/sweetalert2.all.min.js"></script>
</head>
<body>
<<<<<<< HEAD
    <script type="text/javascript">
        function validar(e) { // 1
            tecla = (document.all) ? e.keyCode : e.which; // 2
            if (tecla == 8) return true; // 3
            patron = /[A-Za-z\s]/; // 4
            te = String.fromCharCode(tecla); // 5
            return patron.test(te); // 6
        }
    </script>
    <script type="text/javascript">
        function numeros(nu) { // 1
            tecla = (document.all) ? e.keyCode : e.which; // 2
            if (tecla == 8) return true; // 3
            ppatron = /\d/; // Solo acepta números// 4
            te = String.fromCharCode(tecla); // 5
            return patron.test(te); // 6
        }
    </script>

    <script>
        function NumCheck(e, field) {
            key = e.keyCode ? e.keyCode : e.which
            // backspace
            if (key == 8) return true
            // 0-9
            if (key > 47 && key < 58) {
                if (field.value == "") return true
                regexp = /.[0-9]{2}$/
                return !(regexp.test(field.value))
            }
            // .
            if (key == 46) {
                if (field.value == "") return false
                regexp = /^[0-9]+$/
                return regexp.test(field.value)
            }
            // other key
            return false
        }
    </script>

    <form runat="server">
         <nav class="navbar navbar-expand-sm bg-dark navbar-dark fixed-top">
<div class="container">
    <a class="navbar-brand" href="Dashboard.aspx">
        <img src="assets/img/offsideshop_logo_white_letras.png" alt="OFFSIDESHOP Logo" class="img-fluid" style="max-height: 45px;" />
    </a>
       <a class="navbar-brand" href="#">Edit products</a>
       <asp:Button ID="btnInicio" class="btn btn-outline-success my-2 my-sm-0" type="submit"
           runat="server" Text="Home" OnClick="btnInicio_Click" />
   </nav>
        <div class="my-content">
=======
    <form id="form1" runat="server">
        <nav class="navbar navbar-expand-sm bg-dark navbar-dark fixed-top">
>>>>>>> a37049107b096c909dfaca4b204b5e610cce8eac
            <div class="container">
                <a class="navbar-brand" href="Inicio.aspx">OFFSIDE SHOP</a>
                <asp:Button ID="btnInicio" class="btn btn-outline-success" runat="server" Text="Home" OnClick="btnInicio_Click" />
            </div>
        </nav>

        <div class="container" style="margin-top: 100px;">
            <div class="row">
                <div class="col-sm-6 offset-sm-3">
                    <h2 class="text-center">Actualizar Producto</h2>
                    <div class="form-group mb-2">
                        <asp:TextBox ID="txtid" runat="server" placeholder="ID producto..." class="form-control"></asp:TextBox>
                    </div>
                    <div class="form-group mb-2">
                        <asp:TextBox ID="txtmarca" runat="server" placeholder="Marca..." class="form-control"></asp:TextBox>
                    </div>
                    <div class="form-group mb-2">
                        <asp:TextBox ID="txtproducto" runat="server" placeholder="Producto..." class="form-control"></asp:TextBox>
                    </div>
                    <div class="form-group mb-2">
                        <asp:TextBox ID="txtprecio" runat="server" placeholder="Precio..." class="form-control"></asp:TextBox>
                    </div>
                    <div class="form-group mb-3">
                        <asp:TextBox ID="txtcantidad" runat="server" placeholder="Cantidad..." class="form-control"></asp:TextBox>
                    </div>

                    <div class="d-flex justify-content-between">
                        <asp:Button ID="btnSeleccionar" runat="server" Text="Buscar/Seleccionar" CssClass="btn btn-info" OnClick="btnSeleccionar_Click" Width="48%" />
                        <asp:Button ID="btnEditar" runat="server" Text="Guardar Cambios" CssClass="btn btn-warning" OnClick="btnEditar_Click" Width="48%" />
                    </div>
                    <asp:Literal ID="alerta" runat="server"></asp:Literal>
                </div>
            </div>

            <div class="row mt-5">
                <asp:GridView ID="gvdlista" runat="server" AutoGenerateColumns="False" CssClass="table table-dark table-striped" Width="100%">
                    <Columns>
                        <asp:BoundField DataField="ID" HeaderText="ID" />
                        <asp:BoundField DataField="Producto" HeaderText="Producto" />
                        <asp:BoundField DataField="Marca" HeaderText="Marca" />
                        <asp:BoundField DataField="Precio" HeaderText="Precio" />
                        <asp:BoundField DataField="Cantidad" HeaderText="Cantidad" />
                    </Columns>
                </asp:GridView>
            </div>
        </div>
    </form>
</body>
</html>
