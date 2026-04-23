<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Registro.aspx.cs" Inherits="OFFSIDESHOP.Registro" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <title>Sign Up | OffsideShop</title>
    
    <!-- CSS -->
    <link href="css/bootstrap.min.css" rel="stylesheet" />
    <link href="Font-awesome/css/fontawesome.min.css" rel="stylesheet" />
    <link href="https://fonts.googleapis.com/css?family=Raleway:100,400,600&display=swap" rel="stylesheet" />
    <link href="EstilosCss/Login.css" rel="stylesheet" />
    
    <!-- SweetAlert -->
    <script src="SweetAlert/sweetalert2.all.min.js"></script>
    <script src="SweetAlert/sweetalert2.js"></script>
</head>
<body>
    <form id="registerForm" runat="server">
        <!-- Navbar -->
        <nav class="navbar navbar-expand-sm bg-dark navbar-dark fixed-top">
            <div class="container">
                <a class="navbar-brand" href="Inicio.aspx">
                    <img src="assets/img/offsideshop_logo_white_letras.png" alt="OffsideShop Logo" class="img-fluid" style="max-height: 45px;" />
                </a>
            </div>
        </nav>

        <!-- Registration Section -->
        <section class="register-section">
            <div class="container">
                <div class="register-card">
                    <div class="register-header">
                        <h2>SIGN UP</h2>
                        <h3>Create your account and start buying now.</h3>
                    </div>
                    
                    <div class="register-body">
                        <div class="row">
                            <div class="col-md-6">
                                <div class="form-group">
                                    <asp:TextBox ID="txtfirst" runat="server" 
                                        placeholder="First Name *" 
                                        type="text" 
                                        CssClass="form-control" 
                                        onpaste="return false" 
                                        minlength="2" 
                                        maxlength="30" 
                                        onkeypress="return validateLetters(event)" 
                                        required="required"></asp:TextBox>
                                    <div class="invalid-feedback">Please enter your first name.</div>
                                </div>
                            </div>
                            
                            <div class="col-md-6">
                                <div class="form-group">
                                    <asp:TextBox ID="txtapellido" runat="server" 
                                        placeholder="Last Name *" 
                                        type="text" 
                                        CssClass="form-control" 
                                        onpaste="return false" 
                                        minlength="2" 
                                        maxlength="30" 
                                        onkeypress="return validateLetters(event)" 
                                        required="required"></asp:TextBox>
                                    <div class="invalid-feedback">Please enter your last name.</div>
                                </div>
                            </div>
                        </div>

                        <div class="form-group">
                            <asp:TextBox ID="txtusuario" runat="server" 
                                placeholder="Username *" 
                                type="text" 
                                CssClass="form-control" 
                                onpaste="return false" 
                                minlength="4" 
                                maxlength="15" 
                                required="required"></asp:TextBox>
                            <div class="invalid-feedback">Username must have at least 4 characters.</div>
                        </div>

                        <div class="form-group">
                            <asp:TextBox ID="txtgmail" runat="server" 
                                placeholder="Email *" 
                                type="email" 
                                CssClass="form-control" 
                                onpaste="return false" 
                                minlength="5" 
                                maxlength="50" 
                                required="required"></asp:TextBox>
                            <div class="invalid-feedback">Please enter a valid email address.</div>
                        </div>

                        <div class="row">
                            <div class="col-md-6">
                                <div class="form-group">
                                    <asp:TextBox ID="txtclave" runat="server" 
                                        placeholder="Password *" 
                                        type="password" 
                                        CssClass="form-control" 
                                        onpaste="return false" 
                                        minlength="6" 
                                        maxlength="15" 
                                        required="required"></asp:TextBox>
                                    <div class="invalid-feedback">Password must have at least 6 characters.</div>
                                </div>
                            </div>
                            
                            <div class="col-md-6">
                                <div class="form-group">
                                    <asp:TextBox ID="txtconfirm" runat="server" 
                                        placeholder="Confirm Password *" 
                                        type="password" 
                                        CssClass="form-control" 
                                        onpaste="return false" 
                                        minlength="6" 
                                        maxlength="15" 
                                        required="required"></asp:TextBox>
                                    <div class="invalid-feedback">Passwords do not match.</div>
                                </div>
                            </div>
                        </div>

                        <asp:Button ID="btnRegistrar" runat="server" 
                            Text="Sign Up" 
                            CssClass="btn-register" 
                            OnClick="btnRegistrar_Click" />

                        <div class="login-link">
                            <p>Already have an account? <a href="Login.aspx">Log in here</a></p>
                        </div>

                        <asp:Literal ID="alerta" runat="server" Text=""></asp:Literal>
                    </div>
                </div>
            </div>
        </section>
    </form>

    <!-- JavaScript Validations -->
    <script type="text/javascript">
        // Validation for letters only
        function validateLetters(e) {
            var tecla = (document.all) ? e.keyCode : e.which;
            if (tecla == 8) return true;
            var patron = /[A-Za-záéíóúñÑ\s]/;
            var te = String.fromCharCode(tecla);
            return patron.test(te);
        }

        // Password confirmation validation
        function validatePasswords() {
            var pass = document.getElementById('<%= txtclave.ClientID %>');
            var confirm = document.getElementById('<%= txtconfirm.ClientID %>');

            if (pass.value !== confirm.value) {
                confirm.setCustomValidity("Passwords do not match");
                return false;
            } else {
                confirm.setCustomValidity('');
                return true;
            }
        }

        // Form validation before submit
        document.querySelector('form').addEventListener('submit', function (e) {
            var pass = document.getElementById('<%= txtclave.ClientID %>');
            var confirm = document.getElementById('<%= txtconfirm.ClientID %>');

            if (pass.value !== confirm.value) {
                e.preventDefault();
                Swal.fire({
                    icon: 'error',
                    title: 'Error',
                    text: 'Passwords do not match',
                    confirmButtonColor: '#667eea'
                });
                return false;
            }

            var email = document.getElementById('<%= txtgmail.ClientID %>');
            var emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
            if (!emailRegex.test(email.value)) {
                e.preventDefault();
                Swal.fire({
                    icon: 'error',
                    title: 'Error',
                    text: 'Please enter a valid email address',
                    confirmButtonColor: '#667eea'
                });
                return false;
            }
            
            var firstName = document.getElementById('<%= txtfirst.ClientID %>');
            if (firstName.value.trim().length < 2) {
                e.preventDefault();
                Swal.fire({
                    icon: 'error',
                    title: 'Error',
                    text: 'First name must have at least 2 characters',
                    confirmButtonColor: '#667eea'
                });
                return false;
            }
            
            var lastName = document.getElementById('<%= txtapellido.ClientID %>');
            if (lastName.value.trim().length < 2) {
                e.preventDefault();
                Swal.fire({
                    icon: 'error',
                    title: 'Error',
                    text: 'Last name must have at least 2 characters',
                    confirmButtonColor: '#667eea'
                });
                return false;
            }
            
            var username = document.getElementById('<%= txtusuario.ClientID %>');
            if (username.value.trim().length < 4) {
                e.preventDefault();
                Swal.fire({
                    icon: 'error',
                    title: 'Error',
                    text: 'Username must have at least 4 characters',
                    confirmButtonColor: '#667eea'
                });
                return false;
            }
            
            return true;
        });
        
        // Real-time password validation
        document.getElementById('<%= txtconfirm.ClientID %>').addEventListener('keyup', validatePasswords);
        document.getElementById('<%= txtclave.ClientID %>').addEventListener('keyup', function () {
            validatePasswords();
        });
    </script>
    
    <script src="js/bootstrap.min.js"></script>
</body>
</html>