<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="OFFSIDESHOP.Login" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1, shrink-to-fit=no" />
    <title>Login | OFFSIDESHOP</title>

    <!-- CSS -->
    <link href="css/bootstrap.min.css" rel="stylesheet" />
    <link href="Font-awesome/css/fontawesome.min.css" rel="stylesheet" />
    <link href="https://fonts.googleapis.com/css?family=Inter:400,500,600,700&display=swap" rel="stylesheet" />
    <link href="EstilosCss/AuthSplit.css" rel="stylesheet" />

    <!-- SweetAlert -->
    <script src="SweetAlert/sweetalert2.all.min.js"></script>
    <script src="SweetAlert/sweetalert2.js"></script>

    <script type="text/javascript">
        function validatePasswordChars(e) {
            var tecla = (document.all) ? e.keyCode : e.which;
            if (tecla == 8) return true;
            var te = String.fromCharCode(tecla);
            // Blacklist: < > ' " ` ; \ / & % $ { } ( )
            var forbidden = /[<>'"`\\\/&%$;{}()]/;
            return !forbidden.test(te);
        }
    </script>
</head>
<body>
    <form id="form1" runat="server">
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
                <div class="form-wrapper">
                    
                    <div class="brand-logo-right">
                        <a href="Homepage.aspx">
                            <img src="assets/img/flag-football.png" alt="OFFSIDESHOP" />
                        </a>
                    </div>
                    
                    <div class="form-header">
                        <h2>Log in</h2>
                        <p>Welcome back! Please enter your details.</p>
                    </div>

                    <div class="login-body">
                        <div class="form-group">
                            <asp:TextBox ID="TxtUsuario" runat="server"
                                placeholder="User or Email *"
                                type="text"
                                CssClass="form-control"
                                onpaste="return false"
                                minlength="4"
                                MaxLength="50"
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
                                onkeypress="return validatePasswordChars(event)"
                                minlength="2"
                                MaxLength="15"
                                required="required">
                            </asp:TextBox>
                            <div class="invalid-feedback">Please enter your password.</div>
                        </div>

                        <div class="forgot-link">
                            <asp:HyperLink ID="olvidaste" runat="server"
                                Text="Forgot your password?"
                                NavigateUrl="RecoverAccount.aspx">
                            </asp:HyperLink>
                        </div>

                        <asp:Button ID="btnEntrar" runat="server"
                            Text="Log in"
                            CssClass="btn-login"
                            OnClick="btnEntrar_Click"></asp:Button>
     
                        <asp:LinkButton ID="btnGoogleLogin" runat="server"
                            CssClass="btn-google"
                            OnClick="btnGoogleLogin_Click">
                            <img src="https://upload.wikimedia.org/wikipedia/commons/thumb/3/3c/Google_Favicon_2025.svg/1280px-Google_Favicon_2025.svg.png" alt="Google" />
                            <span>Continue with Google</span>
                        </asp:LinkButton>

                        <div class="register-link">
                            <p>Do not have an account? <a href="SignUp.aspx">Sign up</a></p>
                        </div>

                        <asp:Literal ID="alerta" runat="server" EnableViewState="false"></asp:Literal>
                    </div>
                </div>
            </div>
            
        </div>
    </form>

    <script src="js/jquery.min.js"></script>
    <script src="js/bootstrap.bundle.min.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@8"></script>
</body>
</html>

