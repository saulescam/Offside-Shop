<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ManageCoupons.aspx.cs" Inherits="OFFSIDESHOP.ManageCoupons" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1, shrink-to-fit=no" />
    <title>Manage Coupons | OffsideShop</title>

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

    <script src="SweetAlert/sweetalert2.all.min.js"></script>

    <script type="text/javascript">
        window.onpageshow = function (event) {
            if (event.persisted) { window.location.reload(); }
        };

        // Función para validar números
        function validarNumeros(e) {
            var tecla = (document.all) ? e.keyCode : e.which;
            if (tecla == 8) return true;
            return /\d/.test(String.fromCharCode(tecla));
        }

        // Generador Aleatorio de Código de Cupón (12 caracteres)
        function generateCouponCode() {
            const characters = 'ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789';
            let result = '';
            for (let i = 0; i < 12; i++) {
                result += characters.charAt(Math.floor(Math.random() * characters.length));
            }
            document.getElementById('<%= txtCouponCode.ClientID %>').value = result;
            return false; // Evita el postback
        }
    </script>

    <style>
        .status-badge { padding: 4px 12px; border-radius: 20px; font-size: 0.75rem; font-weight: 700; }
        .status-active { background: #1a7a4a; color: #a8f0c6; }
        .status-inactive { background: #5c2323; color: #f0a8a8; }
        .status-depleted { background: #374151; color: #d1d5db; }

        .form-panel {
            background: var(--card-bg);
            border: 1px solid var(--border-color);
            border-radius: 16px;
            padding: 30px;
            margin-bottom: 34px;
            box-shadow: 0 12px 35px rgba(0,0,0,0.55);
            animation: slideDown 0.35s ease;
        }

        @keyframes slideDown {
            from { opacity: 0; transform: translateY(-18px); }
            to { opacity: 1; transform: translateY(0); }
        }

        .form-panel h4 { font-weight: 700; margin-bottom: 24px; color: #FFC800; }

        .btn-action { border: none; border-radius: 7px; padding: 6px 12px; font-size: 0.85rem; cursor: pointer; transition: all 0.2s ease; margin: 2px; }
        .btn-edit { background: #1e3a8a; color: #93c5fd; }
        .btn-toggle { background: #374151; color: #d1d5db; }
        .btn-delete { background: #7f1d1d; color: #fca5a5; }
        .btn-action:hover { opacity: 0.85; transform: scale(1.05); }

        .btn-save { background: var(--gradient-blue); color: #fff; border: none; border-radius: 9px; padding: 11px 28px; font-weight: 700; cursor: pointer; }
        .btn-save:hover { transform: translateY(-2px); box-shadow: 0 5px 16px rgba(37,99,235,0.4); }

        .btn-add-new { background: var(--gradient-blue); color: #fff; border: none; border-radius: 10px; padding: 10px 22px; font-weight: 700; cursor: pointer; text-decoration: none; }
        .btn-add-new:hover { transform: translateY(-2px); box-shadow: 0 6px 18px rgba(37,99,235,0.45); color: #fff; text-decoration: none; }
        
        .progress-bar-custom {
            background-color: #374151;
            border-radius: 10px;
            height: 10px;
            width: 100%;
            overflow: hidden;
            margin-top: 5px;
        }
        .progress-fill {
            background-color: #FFC800;
            height: 100%;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <asp:ScriptManager ID="ScriptManager1" runat="server" />

        <nav class="top-navbar">
            <div style="display: flex; align-items: center;">
                <a class="navbar-brand" href="Dashboard.aspx">
                    <img src="assets/img/offsideshop_logo_white_letras.png" alt="OFFSIDESHOP" />
                </a>
            </div>
            <button type="button" id="theme-toggle" class="theme-toggle-btn" title="Toggle Light/Dark Theme">
                <i class="fas fa-moon"></i>
            </button>
        </nav>

        <div class="layout-wrapper">
            <aside class="sidebar fade-in">
                <ul class="sidebar-menu">
                    <li><asp:Button ID="btnManageProducts" CssClass="sidebar-btn" runat="server" Text="&#xf553; Manage Products" OnClick="btnManageProducts_Click" Style="font-family: 'Raleway', 'Font Awesome 5 Free'; font-weight: 600;" /></li>
                    <li><a id="btnManageOrders" runat="server" href="ManageOrders.aspx" class="sidebar-btn" style="font-family: 'Raleway','Font Awesome 5 Free'; font-weight: 600;">&#xf46d; Manage Orders</a></li>
                    <li><asp:Button ID="btnManageOffers" CssClass="sidebar-btn" runat="server" Text="&#xf155; Manage Offers" OnClick="btnManageOffers_Click" Style="font-family: 'Raleway', 'Font Awesome 5 Free'; font-weight: 600;" /></li>
                    <li><asp:Button ID="btnManageCoupons" CssClass="sidebar-btn active" runat="server" Text="   &#xf02c; Manage Coupons" OnClick="btnManageCoupons_Click" Style="font-family: 'Raleway', 'Font Awesome 5 Free'; font-weight: 600;" /></li>
                    <li><a id="btnManageTickets" runat="server" href="ManageSellerRequests.aspx" class="sidebar-btn" style="font-family: 'Raleway','Font Awesome 5 Free'; font-weight: 600;">&#xf2b5; Seller Requests</a></li>

                    <li style="border-top: 1px solid var(--border-color); margin-top: 8px; padding-top: 8px;">
                        <asp:Button ID="btnAddLeague" CssClass="sidebar-btn" runat="server" Text="&#xf1ae; Add League" OnClick="btnAddLeague_Click" Style="font-family: 'Raleway', 'Font Awesome 5 Free'; font-weight: 600;" />
                    </li>
                    <li><asp:Button ID="btnAddTeam" CssClass="sidebar-btn" runat="server" Text="&#xf0c0; Add Team" OnClick="btnAddTeam_Click" Style="font-family: 'Raleway', 'Font Awesome 5 Free'; font-weight: 600;" /></li>
                    <li><asp:Button ID="btnAddBrand" CssClass="sidebar-btn" runat="server" Text="&#xf0c0; Add Brand" OnClick="btnAddBrand_Click" Style="font-family: 'Raleway', 'Font Awesome 5 Free'; font-weight: 600;" /></li>

                    <asp:PlaceHolder ID="phOwnerMenu" runat="server">
                        <li style="border-top: 1px solid var(--border-color); margin-top: 8px; padding-top: 8px;">
                            <asp:Button ID="btnManageUsers" CssClass="sidebar-btn" runat="server" Text="&#xf4fe; Manage Users" OnClick="btnManageUsers_Click" Style="font-family: 'Raleway', 'Font Awesome 5 Free'; font-weight: 600;" />
                        </li>
                        <li><asp:Button ID="btnSmtpSettings" CssClass="sidebar-btn" runat="server" Text="&#xf0e0; SMTP Settings" OnClick="btnSmtpSettings_Click" Style="font-family: 'Raleway', 'Font Awesome 5 Free'; font-weight: 600;" /></li>
                        <li><asp:Button ID="btnStats" CssClass="sidebar-btn" runat="server" Text="&#xf080; Stats" OnClick="btnStats_Click" Style="font-family: 'Raleway', 'Font Awesome 5 Free'; font-weight: 600;" /></li>
                        <li><asp:Button ID="btnAuditLogs" CssClass="sidebar-btn" runat="server" Text="&#xf03a; Audit Logs" OnClick="btnAuditLogs_Click" Style="font-family: 'Raleway', 'Font Awesome 5 Free'; font-weight: 600;" /></li>
                    </asp:PlaceHolder>
                    
                    <li><asp:Button ID="btnAdminBanners" CssClass="sidebar-btn" runat="server" Text="&#xf03e; Manage Banners" OnClick="btnAdminBanners_Click" Style="font-family: 'Raleway', 'Font Awesome 5 Free'; font-weight: 600;" /></li>

                    <li style="border-top: 1px solid var(--border-color); margin-top: 8px; padding-top: 8px;">
                        <asp:Button ID="btncerrar" CssClass="sidebar-btn btn-logout" runat="server" Text="&#xf2f5; Logout" OnClick="btncerrar_Click" Style="font-family: 'Raleway', 'Font Awesome 5 Free'; font-weight: 600;" />
                    </li>
                </ul>
            </aside>

            <main class="main-content fade-in" style="animation-delay: 0.2s;">
                <div class="container-fluid">
                    <h1 class="page-title">Discount Coupons</h1>
                    <p class="text-muted mb-4">Create and manage custom or auto-generated discount codes to incentivize purchases.</p>

                    <asp:UpdatePanel ID="upMain" runat="server">
                        <ContentTemplate>
                            <!-- FORMULARIO -->
                            <asp:Panel ID="pnlCouponForm" runat="server" Visible="false" CssClass="form-panel">
                                <asp:HiddenField ID="hfEditId" runat="server" Value="0" />
                                <h4><i class="fas fa-ticket-alt mr-2"></i><asp:Label ID="lblFormTitle" runat="server" Text="Create New Coupon"></asp:Label></h4>

                                <div class="row mt-4">
                                    <div class="col-md-6">
                                        <div class="form-group">
                                            <label>Coupon Code (Letters & Numbers) <span class="text-danger">*</span></label>
                                            <div class="input-group">
                                                <asp:TextBox ID="txtCouponCode" runat="server" CssClass="form-control" MaxLength="20" placeholder="e.g. SUMMER2026" style="text-transform:uppercase;"></asp:TextBox>
                                                <div class="input-group-append">
                                                    <button class="btn btn-warning font-weight-bold text-dark" onclick="return generateCouponCode();" type="button" title="Generate Random 12-char Code">
                                                        <i class="fas fa-random"></i> Generate
                                                    </button>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                    <div class="col-md-3">
                                        <div class="form-group">
                                            <label>Discount (%) <span class="text-danger">*</span></label>
                                            <asp:TextBox ID="txtDiscount" runat="server" CssClass="form-control" MaxLength="3" placeholder="e.g. 15" onkeypress="return validarNumeros(event)"></asp:TextBox>
                                        </div>
                                    </div>
                                </div>

                                <div class="row">
                                    <div class="col-md-4">
                                        <div class="form-group">
                                            <label>Maximum Usage Limit <span class="text-danger">*</span></label>
                                            <asp:TextBox ID="txtMaxUses" runat="server" CssClass="form-control" MaxLength="5" placeholder="e.g. 50" onkeypress="return validarNumeros(event)"></asp:TextBox>
                                            <small class="text-muted">How many times can this code be used globally?</small>
                                        </div>
                                    </div>
                                    <div class="col-md-4">
                                        <div class="form-group">
                                            <label>Status</label>
                                            <asp:DropDownList ID="ddlStatus" runat="server" CssClass="form-control">
                                                <asp:ListItem Text="Active" Value="1"></asp:ListItem>
                                                <asp:ListItem Text="Inactive" Value="0"></asp:ListItem>
                                            </asp:DropDownList>
                                        </div>
                                    </div>
                                </div>

                                <div class="row mt-4">
                                    <div class="col-12 d-flex gap-2">
                                        <asp:LinkButton ID="btnSave" runat="server" CssClass="btn-save mr-2" OnClick="btnSave_Click">
                                            <i class="fas fa-save mr-1"></i> Save Coupon
                                        </asp:LinkButton>
                                        <asp:LinkButton ID="btnCancel" runat="server" CssClass="btn btn-dark font-weight-bold py-2 px-4" style="border-radius: 9px;" OnClick="btnCancel_Click" CausesValidation="false">
                                            Cancel
                                        </asp:LinkButton>
                                    </div>
                                </div>
                            </asp:Panel>

                            <!-- CABECERA Y BOTÓN NUEVO -->
                            <div class="d-flex justify-content-between align-items-center mb-4">
                                <h3 class="text-white m-0" style="font-weight: 600;"><i class="fas fa-list mr-2"></i>Active Catalog</h3>
                                <asp:LinkButton ID="btnAddNew" runat="server" CssClass="btn-add-new" OnClick="btnAddNew_Click" CausesValidation="false">
                                    <i class="fas fa-plus mr-1"></i> New Coupon
                                </asp:LinkButton>
                            </div>

                            <!-- TABLA DE DATOS -->
                            <div class="table-responsive">
                                <asp:GridView ID="gvCoupons" runat="server" AutoGenerateColumns="False" GridLines="None" CssClass="table table-custom text-center align-middle" DataKeyNames="Id_Coupon" OnRowCommand="gvCoupons_RowCommand" OnRowDataBound="gvCoupons_RowDataBound" EmptyDataText="No coupons have been created yet.">
                                    <Columns>
                                        <asp:BoundField DataField="Id_Coupon" HeaderText="ID" ItemStyle-Width="60px" />
                                        <asp:TemplateField HeaderText="Coupon Code" ItemStyle-HorizontalAlign="Left">
                                            <ItemTemplate>
                                                <span class="font-weight-bold" style="color: #FFC800; font-size: 1.1rem; letter-spacing: 1px;"><%# Eval("Code") %></span>
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField HeaderText="Discount">
                                            <ItemTemplate>
                                                <span class="badge badge-info p-2" style="font-size: 0.9rem;"><%# Eval("DiscountPercentage") %>% OFF</span>
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField HeaderText="Usage Stats">
                                            <ItemTemplate>
                                                <div style="text-align: left; max-width: 150px; margin: 0 auto;">
                                                    <small class="text-muted font-weight-bold"><%# Eval("UsedCount") %> / <%# Eval("MaxUses") %> used</small>
                                                    <div class="progress-bar-custom">
                                                        <div class="progress-fill" style='width: <%# GetPercentage(Convert.ToInt32(Eval("UsedCount")), Convert.ToInt32(Eval("MaxUses"))) %>%;'></div>
                                                    </div>
                                                </div>
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField HeaderText="Status">
                                            <ItemTemplate>
                                                <asp:Label ID="lblStatus" runat="server"></asp:Label>
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField HeaderText="Actions">
                                            <ItemTemplate>
                                                <asp:LinkButton ID="btnEdit" runat="server" CssClass="btn-action btn-edit" CommandName="EditCoupon" CommandArgument='<%# Eval("Id_Coupon") %>' ToolTip="Edit"><i class="fas fa-pen"></i></asp:LinkButton>
                                                <asp:LinkButton ID="btnToggle" runat="server" CssClass="btn-action btn-toggle" CommandName="ToggleCoupon" CommandArgument='<%# Eval("Id_Coupon") %>'><i class="fas fa-power-off"></i></asp:LinkButton>
                                                <asp:LinkButton ID="btnDelete" runat="server" CssClass="btn-action btn-delete" CommandName="DeleteCoupon" CommandArgument='<%# Eval("Id_Coupon") %>' OnClientClick="return confirm('Are you sure you want to delete this coupon? Users won\'t be able to use it anymore.');" ToolTip="Delete"><i class="fas fa-trash"></i></asp:LinkButton>
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                    </Columns>
                                </asp:GridView>
                            </div>

                        </ContentTemplate>
                    </asp:UpdatePanel>

                    <asp:Literal ID="alerta" runat="server" Text="" EnableViewState="false"></asp:Literal>
                </div>
            </main>
        </div>
    </form>

    <script src="https://ajax.googleapis.com/ajax/libs/jquery/3.4.1/jquery.min.js"></script>
    <script src="https://cdnjs.cloudflare.com/ajax/libs/popper.js/1.14.7/umd/popper.min.js"></script>
    <script src="https://maxcdn.bootstrapcdn.com/bootstrap/4.3.1/js/bootstrap.min.js"></script>

    <script type="text/javascript">
        document.addEventListener('DOMContentLoaded', function () {
            var themeToggle = document.getElementById('theme-toggle');
            if (themeToggle) {
                var themeIcon = themeToggle.querySelector('i');
                var isDark = document.body.classList.contains('dark-mode') || document.documentElement.classList.contains('dark-mode');
                if (isDark && themeIcon) themeIcon.className = 'fas fa-sun';
                
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
