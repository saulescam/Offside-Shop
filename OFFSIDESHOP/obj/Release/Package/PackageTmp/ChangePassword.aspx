<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ChangePassword.aspx.cs" Inherits="OFFSIDESHOP.ChangePassword" %>


<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1, shrink-to-fit=no" />
    <title>Change Password | OFFSIDESHOP</title>

    <!-- CSS -->
    <link href="css/bootstrap.min.css" rel="stylesheet" />
    <link href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/5.15.4/css/all.min.css" rel="stylesheet" />
    <link href="https://fonts.googleapis.com/css?family=Inter:400,500,600,700&display=swap" rel="stylesheet" />
    <link href="EstilosCss/AuthSplit.css" rel="stylesheet" />

    <!-- SweetAlert -->
    <script src="SweetAlert/sweetalert2.all.min.js"></script>
    <script src="SweetAlert/sweetalert2.js"></script>

   
</head>
<body>
    <form id="form1" runat="server">
        <div class="split-container">
            <asp:LinkButton ID="btnLanguageToggle" runat="server" OnClick="btnLanguageToggle_Click" CssClass="lang-switcher" Style="position: absolute; top: 20px; right: 20px; z-index: 1000; font-weight: 600; color: #111; text-decoration: none; padding: 5px 10px; background: rgba(255,255,255,0.7); border-radius: 5px;">EN / ES</asp:LinkButton>
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
                                    <img src='<%# ResolveUrl(Eval("ImageURL").ToString()) %>' alt="Slide" style='object-fit: cover; width: 100%; height: 100%; object-position: <%# Eval("Id_Slide").ToString() == "5" ? "top center" : "center center" %>;' />
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
                            <i class="fas fa-arrow-left"></i>
                        </a>
                        <a class="nav-btn-circle" href="#authCarousel" role="button" data-slide="next" data-bs-slide="next">
                            <i class="fas fa-arrow-right"></i>
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
                        <h2><%= Resources.Strings.Auth_ChangePassTitle %></h2>
                        <p><%= Resources.Strings.Auth_ChangePassSub %></p>
                    </div>

                    <div class="login-body">
                        <div class="form-group">
                            <asp:TextBox ID="txtpassword1" runat="server" 
                                placeholder="<%$ Resources:Strings, Auth_NewPassInput %>" 
                                type="password" 
                                CssClass="form-control" 
                                onpaste="return false" 
                                minlength="4" 
                                MaxLength="15"
                                required="required">
                            </asp:TextBox>
                            <div class="invalid-feedback"><%= Resources.Strings.Auth_ValNewPass %></div>
                        </div>

                        <div class="form-group">
                            <asp:TextBox ID="txtpassword2" runat="server" 
                                placeholder="<%$ Resources:Strings, Auth_ConfirmPassInput %>" 
                                type="password" 
                                CssClass="form-control" 
                                onpaste="return false" 
                                minlength="4" 
                                MaxLength="15"
                                required="required">
                            </asp:TextBox>
                            <div class="invalid-feedback"><%= Resources.Strings.Auth_ValConfirmPass %></div>
                        </div>

                        <asp:Button runat="server" 
                            Text="<%$ Resources:Strings, Auth_BtnChangePass %>" 
                            CssClass="btn-login" 
                            OnClick="actualizar_Click"></asp:Button>

                        <div class="forgot-link" style="text-align: center; margin-top: 20px;">
                            <asp:HyperLink ID="volver" runat="server"
                                Text="<%$ Resources:Strings, Auth_BackLogin %>"
                                NavigateUrl="Login.aspx"
                                style="font-weight: 600; text-decoration: underline; color: #111;">
                            </asp:HyperLink>
                        </div>

                        <asp:Literal ID="alerta" runat="server" EnableViewState="false"></asp:Literal>
                        <asp:Literal ID="Literal1" runat="server" EnableViewState="false"></asp:Literal>
                    </div>
                </div>
            </div>
        </div>

        <script src="js/jquery.min.js"></script>
        <script src="js/bootstrap.bundle.min.js"></script>
        <script src="https://cdn.jsdelivr.net/npm/sweetalert2@8"></script>
    </form>
</body>
</html>


