<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="AddLeague.aspx.cs" Inherits="OFFSIDESHOP.AddLeague" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1, shrink-to-fit=no" />
    <title>Add League | OffsideShop</title>

    <!-- CSS -->
    <link href="css/bootstrap.min.css" rel="stylesheet" />
    <link href="https://fonts.googleapis.com/css?family=Raleway:100,400,600,700&display=swap" rel="stylesheet" />
    <link rel="stylesheet" href="https://maxcdn.bootstrapcdn.com/bootstrap/4.3.1/css/bootstrap.min.css" />
    <link href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/5.15.4/css/all.min.css" rel="stylesheet" />
    <link href="css/admin-layout.css" rel="stylesheet" />

    <script type="text/javascript">
        (function () {
            var theme = localStorage.getItem('theme') || 'light';
            if (theme === 'dark') {
                document.documentElement.classList.add('dark-mode');
                var observer = new MutationObserver(function (mutations, obs) {
                    if (document.body) {
                        document.body.classList.add('dark-mode');
                        obs.disconnect();
                    }
                });
                observer.observe(document.documentElement, { childList: true, subtree: true });
            }
        })();
    </script>

    <!-- SweetAlert -->
    <script src="SweetAlert/sweetalert2.all.min.js"></script>
    <script src="SweetAlert/sweetalert2.js"></script>

    <script type="text/javascript">
        window.onpageshow = function (event) {
            if (event.persisted) { window.location.reload(); }
        };
    </script>

    <style>
        .lang-toggle-btn {
            color: #ffffff !important;
            font-weight: 700;
            font-size: 1rem;
            text-decoration: none !important;
            letter-spacing: 1px;
            transition: opacity 0.2s ease;
        }
        .lang-toggle-btn:hover {
            opacity: 0.8;
            color: #ffffff !important;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <!-- Top Navbar -->
        <nav class="top-navbar">
            <div style="display: flex; align-items: center; gap: 20px;">
                <a class="navbar-brand" href="Dashboard.aspx" style="margin-right: 0;">
                    <img src="assets/img/offsideshop_logo_white_letras.png" alt="OFFSIDESHOP" />
                </a>
                <asp:LinkButton ID="btnLanguageToggle" runat="server" OnClick="btnLanguageToggle_Click" 
                    CssClass="lang-toggle-btn" CausesValidation="false">
                    EN / ES
                </asp:LinkButton>
            </div>
            <button type="button" id="theme-toggle" class="theme-toggle-btn" title="Toggle Light/Dark Theme">
                <i class="fas fa-moon"></i>
            </button>
        </nav>

        <div class="layout-wrapper">
            <!-- Sidebar -->
            <aside class="sidebar fade-in">
                <ul class="sidebar-menu">
                    <li>
                        <asp:LinkButton ID="btnManageProducts" CssClass="sidebar-btn" runat="server" OnClick="btnManageProducts_Click" Style="font-family: 'Raleway', 'Font Awesome 5 Free'; font-weight: 600;">
                            &#xf553; <asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Nav_ManageProducts %>" />
                        </asp:LinkButton>
                    </li>
                    <li>
                        <a id="btnManageOrders" runat="server" href="ManageOrders.aspx" class="sidebar-btn" style="font-family: 'Raleway','Font Awesome 5 Free'; font-weight: 600;">
                            &#xf46d; <asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Nav_ManageOrders %>" />
                        </a>
                    </li>
                    <li>
                        <asp:LinkButton ID="btnManageOffers" CssClass="sidebar-btn" runat="server" OnClick="btnManageOffers_Click" Style="font-family: 'Raleway', 'Font Awesome 5 Free'; font-weight: 600;">
                            &#xf155; <asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Nav_ManageOffers %>" />
                        </asp:LinkButton>
                    </li>
                    <li>
                        <asp:LinkButton ID="btnManageCoupons" CssClass="sidebar-btn" runat="server" OnClick="btnManageCoupons_Click" Style="font-family: 'Raleway', 'Font Awesome 5 Free'; font-weight: 600;">
                            &#xf02c; <asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Nav_ManageCoupons %>" />
                        </asp:LinkButton>
                    </li>
                    <li>
                        <a id="btnManageTickets" runat="server" href="ManageSellerRequests.aspx" class="sidebar-btn" style="font-family: 'Raleway','Font Awesome 5 Free'; font-weight: 600;">
                            &#xf2b5; <asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Nav_SellerRequests %>" />
                        </a>
                    </li>

                    <li style="border-top: 1px solid var(--border-color); margin-top: 8px; padding-top: 8px;">
                        <asp:LinkButton ID="btnAddLeague" CssClass="sidebar-btn active" runat="server" OnClick="btnAddLeague_Click" Style="font-family: 'Raleway', 'Font Awesome 5 Free'; font-weight: 600;">
                            &#xf1ae; <asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Nav_AddLeague %>" />
                        </asp:LinkButton>
                    </li>
                    <li>
                        <asp:LinkButton ID="btnAddTeam" CssClass="sidebar-btn" runat="server" OnClick="btnAddTeam_Click" Style="font-family: 'Raleway', 'Font Awesome 5 Free'; font-weight: 600;">
                            &#xf0c0; <asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Nav_AddTeam %>" />
                        </asp:LinkButton>
                    </li>
                    <li>
                        <asp:LinkButton ID="btnAddBrand" CssClass="sidebar-btn" runat="server" OnClick="btnAddBrand_Click" Style="font-family: 'Raleway', 'Font Awesome 5 Free'; font-weight: 600;">
                            &#xf0c0; <asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Nav_AddBrand %>" />
                        </asp:LinkButton>
                    </li>

                    <asp:PlaceHolder ID="phOwnerMenu" runat="server">
                        <li style="border-top: 1px solid var(--border-color); margin-top: 8px; padding-top: 8px;">
                            <asp:LinkButton ID="btnManageUsers" CssClass="sidebar-btn" runat="server" OnClick="btnManageUsers_Click" Style="font-family: 'Raleway', 'Font Awesome 5 Free'; font-weight: 600;">
                                &#xf4fe; <asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Nav_ManageUsers %>" />
                            </asp:LinkButton>
                        </li>
                        <li>
                            <asp:LinkButton ID="btnSmtpSettings" CssClass="sidebar-btn" runat="server" OnClick="btnSmtpSettings_Click" Style="font-family: 'Raleway', 'Font Awesome 5 Free'; font-weight: 600;">
                                &#xf0e0; <asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Nav_SmtpSettings %>" />
                            </asp:LinkButton>
                        </li>
                        <li>
                            <asp:LinkButton ID="btnStats" CssClass="sidebar-btn" runat="server" OnClick="btnStats_Click" Style="font-family: 'Raleway', 'Font Awesome 5 Free'; font-weight: 600;">
                                &#xf080; <asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Nav_Stats %>" />
                            </asp:LinkButton>
                        </li>
                        <li>
                            <asp:LinkButton ID="btnAuditLogs" CssClass="sidebar-btn" runat="server" OnClick="btnAuditLogs_Click" Style="font-family: 'Raleway', 'Font Awesome 5 Free'; font-weight: 600;">
                                &#xf03a; <asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Nav_AuditLogs %>" />
                            </asp:LinkButton>
                        </li>
                    </asp:PlaceHolder>
                    <li>
                        <asp:LinkButton ID="btnAdminBanners" CssClass="sidebar-btn" runat="server" OnClick="btnAdminBanners_Click" Style="font-family: 'Raleway', 'Font Awesome 5 Free'; font-weight: 600;">
                            &#xf03e; <asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Nav_ManageBanners %>" />
                        </asp:LinkButton>
                    </li>

                    <li style="border-top: 1px solid var(--border-color); margin-top: 8px; padding-top: 8px;">
                        <asp:LinkButton ID="btncerrar" CssClass="sidebar-btn btn-logout" runat="server" OnClick="btncerrar_Click" Style="font-family: 'Raleway', 'Font Awesome 5 Free'; font-weight: 600;">
                            &#xf2f5; <asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Nav_Logout %>" />
                        </asp:LinkButton>
                    </li>
                </ul>
            </aside>

            <!-- Main Content -->
            <main class="main-content fade-in" style="animation-delay: 0.2s;">
                <div class="container-fluid">
                    <h1 class="page-title"><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_League_Title %>" /></h1>
                    <p class="text-muted mb-4"><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_League_Subtitle %>" /></p>

                    <div class="row">
                        <div class="col-xl-6 col-lg-8">
                            <div class="form-card">

                                <div class="form-group">
                                    <label><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_League_NameLabel %>" /> <span class="text-danger">*</span></label>
                                    <asp:TextBox ID="txtLeagueName" runat="server"
                                        placeholder="e.g. LaLiga, Premier League..."
                                        CssClass="form-control"
                                        MaxLength="100"
                                        onpaste="return false">
                                    </asp:TextBox>
                                </div>

                                <div class="row mt-3">
                                    <div class="col-12">
                                        <asp:LinkButton ID="btnSaveLeague" runat="server"
                                            CssClass="mybtn"
                                            OnClick="btnSaveLeague_Click"
                                            Style="font-family: 'Raleway', 'Font Awesome 5 Free'; font-weight: 600; padding: 12px 24px; text-decoration: none; display: inline-block;">
                                            &#xf0c7;&nbsp; <asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_League_SaveBtn %>" />
                                        </asp:LinkButton>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>

                    <!-- Leagues GridView -->
                    <div class="row mt-5">
                        <div class="col-12">
                            <h3 class="text-white mb-4" style="font-weight: 600;">
                                <i class="fas fa-list mr-2"></i><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_League_ListTitle %>" />
                            </h3>
                            <div class="table-responsive">
                                <asp:GridView ID="gvLeagues" runat="server"
                                    AutoGenerateColumns="False"
                                    GridLines="None"
                                    CssClass="table table-custom text-center"
                                    DataKeyNames="Id_League"
                                    OnRowDeleting="gvLeagues_RowDeleting">
                                    <Columns>
                                        <asp:BoundField DataField="Id_League" HeaderText="<%$ Resources:Strings, Admin_League_ColID %>" />
                                        <asp:BoundField DataField="Name_League" HeaderText="<%$ Resources:Strings, Admin_League_ColName %>" />
                                        <asp:TemplateField HeaderText="<%$ Resources:Strings, Admin_League_ColActions %>">
                                            <ItemTemplate>
                                                <asp:LinkButton ID="lbDeleteLeague" runat="server"
                                                    CommandName="Delete"
                                                    CssClass="btn btn-sm btn-danger"
                                                    OnClientClick='<%# "return confirm(\"" + GetGlobalResourceObject("Strings", "Confirm_DeleteLeague") + "\");" %>'
                                                    Style="font-weight: 600; border-radius: 6px; padding: 4px 12px;">
                                                    <i class="fas fa-trash-alt mr-1"></i> <asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_League_Delete %>" />
                                                </asp:LinkButton>
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                    </Columns>
                                </asp:GridView>
                            </div>
                        </div>
                    </div>
                </div>

                <asp:Literal ID="alerta" runat="server" Text="" EnableViewState="false"></asp:Literal>
            </main>
        </div>
    </form>

    <!-- Scripts -->
    <script src="https://ajax.googleapis.com/ajax/libs/jquery/3.4.1/jquery.min.js"></script>
    <script src="https://cdnjs.cloudflare.com/ajax/libs/popper.js/1.14.7/umd/popper.min.js"></script>
    <script src="https://maxcdn.bootstrapcdn.com/bootstrap/4.3.1/js/bootstrap.min.js"></script>

    <script type="text/javascript">
        document.addEventListener('DOMContentLoaded', function () {
            var themeToggle = document.getElementById('theme-toggle');
            if (themeToggle) {
                var themeIcon = themeToggle.querySelector('i');
                var isDark = document.body.classList.contains('dark-mode') || document.documentElement.classList.contains('dark-mode');
                if (isDark && themeIcon) {
                    themeIcon.className = 'fas fa-sun';
                }
                themeToggle.addEventListener('click', function (e) {
                    e.preventDefault();
                    var currentlyDark = document.body.classList.contains('dark-mode') || document.documentElement.classList.contains('dark-mode');
                    if (currentlyDark) {
                        document.body.classList.remove('dark-mode');
                        document.documentElement.classList.remove('dark-mode');
                        localStorage.setItem('theme', 'light');
                        if (themeIcon) themeIcon.className = 'fas fa-moon';
                    } else {
                        document.body.classList.add('dark-mode');
                        document.documentElement.classList.add('dark-mode');
                        localStorage.setItem('theme', 'dark');
                        if (themeIcon) themeIcon.className = 'fas fa-sun';
                    }
                });
            }
        });
    </script>
</body>
</html>