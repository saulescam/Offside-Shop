<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="SignUp.aspx.cs" Inherits="OFFSIDESHOP.SignUp" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1, shrink-to-fit=no" />
    <title>Sign Up | OffsideShop</title>

    <link href="css/bootstrap.min.css" rel="stylesheet" />
    <link href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.0/css/all.min.css" rel="stylesheet" />
    <link href="https://fonts.googleapis.com/css?family=Inter:400,500,600,700&display=swap" rel="stylesheet" />
    <link href="EstilosCss/AuthSplit.css" rel="stylesheet" />

    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>


</head>
<body>
    <form id="registerForm" runat="server">
        <asp:ScriptManager ID="ScriptManager1" runat="server" />
        
        <div class="split-container">
            <!-- LEFT PANEL: VISUAL / CAROUSEL -->
            <div class="split-left">
                <div class="visual-icon">
                    <img src="assets/img/offsidelogo.png" alt="Icon" />
                </div>
                
                <div id="authCarousel" class="carousel slide" data-ride="carousel" data-bs-ride="carousel" data-interval="5000" data-bs-interval="5000">
                    <div class="carousel-inner">
                        <asp:Repeater ID="rptCarousel" runat="server">
                            <ItemTemplate>
                                <div class='carousel-item <%# Container.ItemIndex == 0 ? "active" : "" %>'>
                                    <img src='<%# ResolveUrl(Eval("ImageURL").ToString()) %>' alt="Slide" />
                                    <div class="carousel-caption-custom">
                                        <div class="carousel-text-area">
                                            <h2>"<%# Eval("QuoteText") %>"</h2>
                                            <div class="carousel-author">
                                                <h4><%# Eval("AuthorName") %></h4>
                                                <p><%# Eval("AuthorRole") %></p>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </ItemTemplate>
                        </asp:Repeater>
                    </div>
                    
                    <!-- BOTONES FUERA DEL REPEATER (ESTATICOS) -->
                    <div class="carousel-static-nav">
                        <a class="nav-btn-circle" href="#authCarousel" role="button" data-slide="prev" data-bs-slide="prev">
                            <i class="fa fa-arrow-left"></i>
                        </a>
                        <a class="nav-btn-circle" href="#authCarousel" role="button" data-slide="next" data-bs-slide="next">
                            <i class="fa fa-arrow-right"></i>
                        </a>
                    </div>
                </div>
            </div>

            <!-- RIGHT PANEL: FORM -->
            <div class="split-right">
                <asp:UpdatePanel ID="upRegister" runat="server" style="width: 100%; display: flex; justify-content: center; margin: auto 0;">
                    <ContentTemplate>
                        <div class="form-wrapper">
                            <div class="brand-logo-right">
                                <a href="Homepage.aspx">
                            <img src="assets/img/flag-football.png" alt="OFFSIDESHOP" />
                                </a>
                            </div>

                            <!-- PANEL 1: FORMULARIO DE REGISTRO -->
                            <asp:Panel ID="pnlRegister" runat="server">
                                <div class="form-header">
                                    <h2>Create an account</h2>
                                    <p>Buy the best football shirts of all time.</p>
                                </div>

                                <div class="register-body">
                                    <div class="row">
                                        <div class="col-md-6">
                                            <div class="form-group">
                                                <asp:TextBox ID="txtfirst" runat="server"
                                                    placeholder="First Name *" type="text" CssClass="form-control"
                                                    onpaste="return false" minlength="2" MaxLength="30"
                                                    onkeypress="return validateLetters(event)" required="required"></asp:TextBox>
                                            </div>
                                        </div>

                                        <div class="col-md-6">
                                            <div class="form-group">
                                                <asp:TextBox ID="txtapellido" runat="server"
                                                    placeholder="Last Name *" type="text" CssClass="form-control"
                                                    onpaste="return false" minlength="2" MaxLength="30"
                                                    onkeypress="return validateLetters(event)" required="required"></asp:TextBox>
                                            </div>
                                        </div>
                                    </div>

                                    <div class="form-group">
                                        <asp:TextBox ID="txtusuario" runat="server"
                                            placeholder="Username *" type="text" CssClass="form-control"
                                            onpaste="return false" minlength="4" MaxLength="15"
                                            required="required"></asp:TextBox>
                                    </div>

                                    <div class="form-group">
                                        <asp:TextBox ID="txtgmail" runat="server"
                                            placeholder="Email *" type="email" CssClass="form-control"
                                            onpaste="return false" minlength="5" MaxLength="50"
                                            required="required"></asp:TextBox>
                                    </div>

                                    <div class="row">
                                        <div class="col-md-6">
                                            <div class="form-group">
                                                <asp:TextBox ID="txtclave" runat="server"
                                                    placeholder="Password *" type="password" CssClass="form-control"
                                                    onpaste="return false" onkeypress="return validatePasswordChars(event)"
                                                    minlength="6" MaxLength="15" required="required"></asp:TextBox>
                                            </div>
                                        </div>

                                        <div class="col-md-6">
                                            <div class="form-group">
                                                <asp:TextBox ID="txtconfirm" runat="server"
                                                    placeholder="Confirm Password *" type="password" CssClass="form-control"
                                                    onpaste="return false" onkeypress="return validatePasswordChars(event)"
                                                    minlength="6" MaxLength="15" required="required"></asp:TextBox>
                                            </div>
                                        </div>
                                    </div>

                                    <!-- Al hacer clic, ejecuta la validación JS y muestra el loading spinner -->
                                    <asp:Button ID="btnRegistrar" runat="server"
                                        Text="Create account" CssClass="btn-register" OnClick="btnRegistrar_Click" OnClientClick="if(!validateForm()) return false;" />

                                    <asp:LinkButton ID="btnGoogleSign" runat="server"
                                        CssClass="btn-google" OnClick="btnGoogleSign_Click">
                                        <img src="https://upload.wikimedia.org/wikipedia/commons/thumb/3/3c/Google_Favicon_2025.svg/1280px-Google_Favicon_2025.svg.png" alt="Google" />
                                        <span>Sign up with Google</span>
                                    </asp:LinkButton>

                                    <div class="login-link">
                                        <p>Already have an account? <a href="Login.aspx">Log in</a></p>
                                    </div>
                                </div>
                            </asp:Panel>

                            <!-- PANEL 2: INGRESO DEL TOKEN -->
                            <asp:Panel ID="pnlVerify" runat="server" Visible="false">
                                <div class="form-header text-center">
                                    <h2><i class="fas fa-envelope-open-text" style="color: #FFC800;"></i> Verify Email</h2>
                                    <p class="mt-2">We've sent a 6-digit code to <br/><strong id="displayEmail" runat="server" style="color: #111;"></strong></p>
                                </div>
                                
                                <div class="register-body text-center p-0 mt-4">
                                    <p class="text-muted mb-4" style="font-size: 14px;">Please check your inbox (and spam folder) and enter the code below to activate your account.</p>
                                    
                                    <div class="form-group d-flex justify-content-center">
                                        <asp:TextBox ID="txtToken" runat="server" CssClass="form-control text-center mx-auto" placeholder="000000" MaxLength="6" style="font-size: 32px; letter-spacing: 12px; font-weight: bold; width: 75%; border: 1px solid #ddd; border-radius: 8px;"></asp:TextBox>
                                    </div>
                                    
                                    <asp:Button ID="btnVerify" runat="server" Text="Complete Registration" CssClass="btn-register mt-4" OnClick="btnVerify_Click" OnClientClick="showSpinner('Verifying code...');" />
                                    
                                    <div class="mt-4">
                                        <asp:LinkButton ID="btnBack" runat="server" CssClass="text-muted text-decoration-none" style="font-size: 14px; font-weight: 500;" OnClick="btnBack_Click"><i class="fas fa-arrow-left mr-1"></i> Use a different email</asp:LinkButton>
                                    </div>
                                </div>
                            </asp:Panel>

                        </div>
                    </ContentTemplate>
                </asp:UpdatePanel>
            </div>
        </div>
    </form>

    <script src="js/jquery.min.js"></script>
    <script src="js/bootstrap.bundle.min.js"></script>

    <script type="text/javascript">
        function validateLetters(e) {
            var tecla = (document.all) ? e.keyCode : e.which;
            if (tecla == 8) return true;
            var patron = /[A-Za-záéíóúñÑ\s]/;
            var te = String.fromCharCode(tecla);
            return patron.test(te);
        }

        function validatePasswordChars(e) {
            var tecla = (document.all) ? e.keyCode : e.which;
            if (tecla == 8) return true;
            var te = String.fromCharCode(tecla);
            var forbidden = /[<>'"`\\\/&%$;{}()]/;
            return !forbidden.test(te);
        }

        // Esta es la función principal que evita que la pantalla se "congele"
        function validateForm() {
            var pass = document.getElementById('<%= txtclave.ClientID %>').value;
            var confirm = document.getElementById('<%= txtconfirm.ClientID %>').value;

            if (pass !== confirm) {
                Swal.fire({
                    icon: 'error', title: 'Error', text: 'Passwords do not match', confirmButtonColor: '#FFC800'
                });
                return false;
            }

            // Mostramos una alerta de carga para que el usuario sepa que el sistema está trabajando
            Swal.fire({
                title: 'Sending Verification Code...',
                text: 'Please wait a moment while we set up your account.',
                allowOutsideClick: false,
                didOpen: () => { Swal.showLoading(); }
            });
            return true;
        }

        function showSpinner(msg) {
            Swal.fire({
                title: msg,
                allowOutsideClick: false,
                didOpen: () => { Swal.showLoading(); }
            });
        }
    </script>
</body>
</html>