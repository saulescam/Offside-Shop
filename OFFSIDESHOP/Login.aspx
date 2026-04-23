<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="OFFSIDESHOP.Login" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <title>Login | OFFSIDESHOP</title>
    
    <!-- CSS -->
    <link href="css/bootstrap.min.css" rel="stylesheet" />
    <link href="Font-awesome/css/fontawesome.min.css" rel="stylesheet" />
    <link href="https://fonts.googleapis.com/css?family=Raleway:100,400,600&display=swap" rel="stylesheet" />
    <link href="css/styles.css" rel="stylesheet" />
    <link href="EstilosCss/Login.css" rel="stylesheet" /> <!-- Nuevo CSS para login negro -->
    
    <!-- SweetAlert -->
    <script src="SweetAlert/sweetalert2.all.min.js"></script>
    <script src="SweetAlert/sweetalert2.js"></script>
</head>
<body>
    <form id="form1" runat="server">
        <!-- Navbar con imagen -->
        <nav class="navbar navbar-expand-sm bg-dark navbar-dark fixed-top">
            <div class="container">
                <a class="navbar-brand" href="Inicio.aspx">
                    <img src="assets/img/offsideshop_logo_white_letras.png" alt="OFFSIDESHOP Logo" style="max-height: 45px; width: auto;" />
                </a>
                
            </div>
        </nav>

        <!-- Seccion de Login -->
        <div class="login-section">
            <div class="container">
                <div class="login-card">
                    <div class="login-header">
                        <h2>LOGIN</h2>
                        <h3>Welcome to OFFSIDESHOP</h3>
                        <p>Write your user and password</p>
                    </div>
                    
                    <div class="login-body">
                        <div class="form-group">
                            <asp:TextBox ID="TxtUsuario" runat="server" 
                                placeholder="User *" 
                                type="text" 
                                CssClass="form-control" 
                                onpaste="return false" 
                                minlength="4" 
                                MaxLength="15" 
                                required="required">
                            </asp:TextBox>
                            <div class="invalid-feedback">Enter your user.</div>
                        </div>

                        <div class="form-group">
                            <asp:TextBox ID="TxtContra" runat="server" 
                                placeholder="Password *" 
                                type="password" 
                                CssClass="form-control" 
                                onpaste="return false" 
                                minlength="2" 
                                MaxLength="15" 
                                required="required">
                            </asp:TextBox>
                            <div class="invalid-feedback">Please enter your password.</div>
                        </div>

                        <asp:Button ID="btnEntrar" runat="server" 
                            Text="Log in" 
                            CssClass="btn-login" 
                            OnClick="btnEntrar_Click">
                        </asp:Button>

                        <div class="forgot-link">
                            <asp:HyperLink ID="olvidaste" runat="server" 
                                Text="Forgot your password?" 
                                NavigateUrl="RecuperarCuenta.aspx">
                            </asp:HyperLink>
                        </div>

                        <div class="register-link">
                            <p>Do not have an account? <a href="Registro.aspx">Register here</a></p>
                        </div>

                        <asp:Literal ID="alerta" runat="server"></asp:Literal>
                    </div>
                </div>
            </div>
        </div>
    </form>

    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@8"></script>
    <script src="js/bootstrap.min.js"></script>
</body>
</html>