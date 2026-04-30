<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="RecuperarCuenta.aspx.cs" Inherits="OFFSIDESHOP.RecuperarContrasena" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    
    <!-- CSS -->
    <link href="css/bootstrap.min.css" rel="stylesheet" />
    <link href="https://fonts.googleapis.com/css?family=Raleway:100,400,600&display=swap" rel="stylesheet" />
    <link href="EstilosCss/Recuperar.css" rel="stylesheet" />
    
    <!-- SweetAlert -->
    <script src="SweetAlert/sweetalert2.all.min.js"></script>
    <script src="SweetAlert/sweetalert2.js"></script>
    
    <title>Forgot your password? | OffsideShop</title>
</head>
<body>
    <form runat="server">
        <!-- Navbar (sin cambios) -->
        <nav class="navbar navbar-expand-lg navbar-dark fixed-top" id="mainNav">
            <div class="container">
                <a class="navbar-brand" href="Inicio.aspx">
                    <img src="assets/img/offsideshop_logo_white_letras.png" alt="OffsideShop Logo" style="max-height: 45px;" />
                </a>
                <button class="navbar-toggler" type="button" data-bs-toggle="collapse" data-bs-target="#navbarResponsive">
                    <span class="navbar-toggler-icon"></span>
                </button>
                <div class="collapse navbar-collapse" id="navbarResponsive"></div>
            </div>
        </nav>

        <!-- Formulario de Recuperar Contraseña -->
        <div class="my-content">
            <div class="container">
                <div class="row">
                    <div class="col-sm-6 col-sm-offset-3 myform-cont">
                        <h1>Recover your password</h1>
                        <div class="myform-top">
                            <div class="myform-top-left">
                                <h3>Forgot your password?</h3>
                                <p>No problem. Enter your username and we'll help you recover it.</p>
                            </div>
                        </div>
                        <div class="myform-bottom">
                            <div class="form-group">
                                <asp:TextBox ID="txtcuenta" runat="server" 
                                    placeholder="Username" 
                                    type="text" 
                                    CssClass="form-control" 
                                    required="required">
                                </asp:TextBox>
                            </div>
                            
                            <asp:Button runat="server" Text="Recovery" CssClass="mybtn" OnClick="Unnamed1_Click"></asp:Button>
                            
                            <div class="back-link">
                                <a href="Login.aspx">← Back to Login</a>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        
        <asp:Literal ID="alertas" runat="server" Text=""></asp:Literal>
        
        <script src="js/bootstrap.min.js"></script>
    </form>
</body>
</html>