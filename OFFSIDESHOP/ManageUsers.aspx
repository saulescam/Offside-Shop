<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ManageUsers.aspx.cs" Inherits="OFFSIDESHOP.ManageUsers" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1, shrink-to-fit=no" />
    <title>Manage Users | OffsideShop</title>

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

    <style>
        .filter-card {
            background: var(--card-bg);
            border: 1px solid var(--border-color);
            border-radius: 14px;
            padding: 20px 24px;
            margin-bottom: 28px;
            box-shadow: 0 6px 20px rgba(0,0,0,0.15);
        }

        .filter-card label {
            color: var(--text-muted);
            font-weight: 600;
            font-size: 0.8rem;
            text-transform: uppercase;
            letter-spacing: 0.8px;
            margin-bottom: 6px;
        }

        .perm-card {
            background: rgba(255, 200, 0, 0.05);
            padding: 12px 15px;
            border-radius: 8px;
            border: 1px solid var(--border-color);
            margin-bottom: 15px;
            transition: all 0.2s ease;
        }
        .perm-card:hover {
            border-color: #FFC800;
            background: rgba(255, 200, 0, 0.1);
        }
        .perm-title {
            color: #FFC800;
            font-weight: 700;
            font-size: 1rem;
            margin-bottom: 2px;
        }
        .perm-desc {
            color: #888;
            font-size: 0.85rem;
            margin-left: 24px;
        }
        .action-btn {
            border: none;
            border-radius: 6px;
            padding: 5px 10px;
            font-size: 0.85rem;
            font-weight: 600;
            cursor: pointer;
            transition: all 0.2s;
            margin: 0 2px;
        }
        .btn-edit { background: #3b82f6; color: white; }
        .btn-edit:hover { background: #2563eb; }
        .btn-perm { background: #10b981; color: white; }
        .btn-perm:hover { background: #059669; }
        .btn-delete { background: #ef4444; color: white; }
        .btn-delete:hover { background: #dc2626; }
        
        /* Modal custom background overlay */
        .modal-backdrop-custom {
            position: fixed;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            background-color: rgba(0,0,0,0.6);
            z-index: 1040;
            display: flex;
            align-items: center;
            justify-content: center;
        }

        /* Modal custom dialog styling */
        .modal-dialog-custom {
            background: var(--card-bg, #ffffff);
            border: 1px solid var(--border-color, #e0e0e0);
            border-radius: 12px;
            width: 100%;
            max-width: 600px;
            box-shadow: 0 10px 30px rgba(0,0,0,0.3);
            z-index: 1050;
            overflow: hidden;
            animation: slideDown 0.3s ease-out;
        }

        @keyframes slideDown {
            from { transform: translateY(-30px); opacity: 0; }
            to { transform: translateY(0); opacity: 1; }
        }

        .modal-content { background-color: var(--card-bg); border: 1px solid var(--border-color); color: var(--text-main); }
        .modal-header { border-bottom: 1px solid var(--border-color); }
        .modal-footer { border-top: 1px solid var(--border-color); }
        .close { color: var(--text-main); text-shadow: none; }
        .close:hover { color: #FFC800; }

        /* Driver Status Badges */
        .badge-driver-delivering { background-color: #3b82f6; color: #ffffff; padding: 6px 12px; border-radius: 20px; font-weight: 700; font-size: 0.78rem; }
        .badge-driver-onduty { background-color: #10b981; color: #ffffff; padding: 6px 12px; border-radius: 20px; font-weight: 700; font-size: 0.78rem; }
        .badge-driver-offduty { background-color: #6b7280; color: #ffffff; padding: 6px 12px; border-radius: 20px; font-weight: 700; font-size: 0.78rem; }
    </style>

    <script type="text/javascript">
        window.onpageshow = function (event) {
            if (event.persisted) { window.location.reload(); }
        };
    </script>
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
                    <li><a id="btnManageOrders" runat="server" href="ManageOrders.aspx" class="sidebar-btn " style="font-family: 'Raleway','Font Awesome 5 Free'; font-weight: 600;">&#xf46d; Manage Orders</a></li>
                    <li><asp:Button ID="btnManageOffers" CssClass="sidebar-btn" runat="server" Text="&#xf155; Manage Offers" OnClick="btnManageOffers_Click" Style="font-family: 'Raleway', 'Font Awesome 5 Free'; font-weight: 600;" /></li>
                    <li><asp:Button ID="btnManageCoupons" CssClass="sidebar-btn" runat="server" Text="&#xf02c; Manage Coupons" OnClick="btnManageCoupons_Click" Style="font-family: 'Raleway', 'Font Awesome 5 Free'; font-weight: 600;" /></li>
                    <li><a id="btnManageTickets" runat="server" href="ManageSellerRequests.aspx" class="sidebar-btn" style="font-family: 'Raleway','Font Awesome 5 Free'; font-weight: 600;">&#xf2b5; Seller Requests</a></li>
                    <li style="border-top: 1px solid var(--border-color); margin-top: 8px; padding-top: 8px;"><asp:Button ID="btnAddLeague" CssClass="sidebar-btn" runat="server" Text="&#xf1ae; Add League" OnClick="btnAddLeague_Click" Style="font-family: 'Raleway', 'Font Awesome 5 Free'; font-weight: 600;" /></li>
                    <li><asp:Button ID="btnAddTeam" CssClass="sidebar-btn" runat="server" Text="&#xf0c0; Add Team" OnClick="btnAddTeam_Click" Style="font-family: 'Raleway', 'Font Awesome 5 Free'; font-weight: 600;" /></li>
                    <li><asp:Button ID="btnAddBrand" CssClass="sidebar-btn" runat="server" Text="&#xf0c0; Add Brand" OnClick="btnAddBrand_Click" Style="font-family: 'Raleway', 'Font Awesome 5 Free'; font-weight: 600;" /></li>

                    <asp:PlaceHolder ID="phOwnerMenu" runat="server">
                        <li style="border-top: 1px solid var(--border-color); margin-top: 8px; padding-top: 8px;">
                            <asp:Button ID="btnManageUsers" CssClass="sidebar-btn active" runat="server" Text="&#xf4fe; Manage Users" OnClick="btnManageUsers_Click" Style="font-family: 'Raleway', 'Font Awesome 5 Free'; font-weight: 600;" />
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
                    <h1 class="page-title">User & Role Management</h1>
                    <p class="text-muted mb-4">Only the Owner may reassign roles, edit users, or configure granular Admin Module Permissions (PBAC).</p>

                    <asp:UpdatePanel ID="upMain" runat="server">
                        <ContentTemplate>
                            <!-- Create New User Form Card -->
                            <div class="form-card mb-4">
                                <h4 class="text-white mb-3" style="font-weight: 700;"><i class="fas fa-user-plus mr-2"></i>Create New User / Admin</h4>
                                <div class="row">
                                    <div class="col-md-3 form-group">
                                        <label>Username <span class="text-danger">*</span></label>
                                        <asp:TextBox ID="txtNewUser" runat="server" CssClass="form-control" placeholder="e.g. jsmith"></asp:TextBox>
                                    </div>
                                    <div class="col-md-3 form-group">
                                        <label>Email <span class="text-danger">*</span></label>
                                        <asp:TextBox ID="txtNewEmail" runat="server" CssClass="form-control" TextMode="Email" placeholder="user@domain.com"></asp:TextBox>
                                    </div>
                                    <div class="col-md-3 form-group">
                                        <label>Password <span class="text-danger">*</span></label>
                                        <asp:TextBox ID="txtNewPass" runat="server" CssClass="form-control" TextMode="Password" placeholder="••••••••"></asp:TextBox>
                                    </div>
                                    <div class="col-md-3 form-group">
                                        <label>Role <span class="text-danger">*</span></label>
                                        <asp:DropDownList ID="ddlNewRole" runat="server" CssClass="form-control">
                                            <asp:ListItem Value="3">Customer</asp:ListItem>
                                            <asp:ListItem Value="2">Admin</asp:ListItem>
                                            <asp:ListItem Value="4">Delivery</asp:ListItem>
                                        </asp:DropDownList>
                                        <small class="text-muted d-block mt-1">If "Admin", configure permissions in the grid below after creating.</small>
                                    </div>
                                </div>
                                <asp:Button ID="btnCreateUser" runat="server" Text="&#xf0c7; Create User" CssClass="mybtn" OnClick="btnCreateUser_Click" Style="font-family: 'Raleway', 'Font Awesome 5 Free'; font-weight: 600; width: auto; padding: 10px 25px;" />
                            </div>

                            <!-- FILTROS DE BÚSQUEDA Y ROLES -->
                            <div class="filter-card mb-4">
                                <div class="row align-items-end">
                                    <div class="col-md-3 col-sm-6 mb-2 mb-md-0">
                                        <label>Filter by Role</label>
                                        <asp:DropDownList ID="ddlFilterRole" runat="server" CssClass="form-control" AutoPostBack="true" OnSelectedIndexChanged="Filter_Changed">
                                            <asp:ListItem Value="0" Text="-- All Roles --"></asp:ListItem>
                                            <asp:ListItem Value="1" Text="Owner"></asp:ListItem>
                                            <asp:ListItem Value="2" Text="Admin"></asp:ListItem>
                                            <asp:ListItem Value="3" Text="Customer"></asp:ListItem>
                                            <asp:ListItem Value="4" Text="Delivery Drivers"></asp:ListItem>
                                        </asp:DropDownList>
                                    </div>
                                    <div class="col-md-3 col-sm-6 mb-2 mb-md-0">
                                        <label>Delivery Status</label>
                                        <asp:DropDownList ID="ddlFilterDeliveryStatus" runat="server" CssClass="form-control" AutoPostBack="true" OnSelectedIndexChanged="Filter_Changed">
                                            <asp:ListItem Value="ALL" Text="-- All Delivery Statuses --"></asp:ListItem>
                                            <asp:ListItem Value="AVAILABLE" Text="🟢 On Duty (Available)"></asp:ListItem>
                                            <asp:ListItem Value="DELIVERING" Text="🔵 On the Way (Delivering Order)"></asp:ListItem>
                                            <asp:ListItem Value="OFFDUTY" Text="⚪ Off Duty (Resting)"></asp:ListItem>
                                        </asp:DropDownList>
                                    </div>
                                    <div class="col-md-4 col-sm-8 mb-2 mb-md-0">
                                        <label>Search User</label>
                                        <asp:TextBox ID="txtSearchUser" runat="server" CssClass="form-control" placeholder="Search by username or email..." AutoPostBack="true" OnTextChanged="Filter_Changed"></asp:TextBox>
                                    </div>
                                    <div class="col-md-2 col-sm-4 text-right">
                                        <asp:LinkButton ID="btnClearFilters" runat="server" CssClass="btn btn-outline-secondary font-weight-bold w-100" OnClick="btnClearFilters_Click">
                                            <i class="fas fa-eraser mr-1"></i> Clear
                                        </asp:LinkButton>
                                    </div>
                                </div>
                            </div>

                            <h3 class="text-white mb-3" style="font-weight: 700;"><i class="fas fa-users mr-2"></i>Registered Users</h3>
                            <div class="table-responsive">
                                <asp:GridView ID="gvUsers" runat="server" AutoGenerateColumns="False" GridLines="None" CssClass="table table-custom text-center align-middle" DataKeyNames="Id_User" OnRowCommand="gvUsers_RowCommand" OnRowDataBound="gvUsers_RowDataBound">
                                    <Columns>
                                        <asp:BoundField DataField="Id_User" HeaderText="ID" ItemStyle-Width="60px" />
                                        <asp:BoundField DataField="Name_User" HeaderText="Username" />
                                        <asp:BoundField DataField="Mail" HeaderText="Email" />
                                        <asp:TemplateField HeaderText="Current Role">
                                            <ItemTemplate>
                                                <span class="badge badge-dark p-2" style="font-size: 0.85rem;"><%# Eval("Name_Role") %></span>
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                        
                                        <!-- COLUMNA DINÁMICA DE ESTADO DEL REPARTIDOR -->
                                        <asp:TemplateField HeaderText="Delivery Activity Status">
                                            <ItemTemplate>
                                                <asp:Label ID="lblDeliveryStatusBadge" runat="server"></asp:Label>
                                            </ItemTemplate>
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="Actions">
                                            <ItemTemplate>
                                                <asp:Label ID="lblOwnerProtect" runat="server" Text="<span class='text-muted'><i class='fas fa-crown'></i> Owner</span>" Visible="false"></asp:Label>
                                                
                                                <asp:LinkButton ID="btnEdit" runat="server" CssClass="action-btn btn-edit" CommandName="EditUser" CommandArgument='<%# Eval("Id_User") %>' ToolTip="Edit User Data">
                                                    <i class="fas fa-pen"></i> Edit
                                                </asp:LinkButton>
                                                
                                                <asp:LinkButton ID="btnPermissions" runat="server" CssClass="action-btn btn-perm" CommandName="ManagePermissions" CommandArgument='<%# Eval("Id_User") %>' ToolTip="Configure Module Access">
                                                    <i class="fas fa-key"></i> Permissions
                                                </asp:LinkButton>
                                                
                                                <asp:LinkButton ID="btnDelete" runat="server" CssClass="action-btn btn-delete" CommandName="DeleteUser" CommandArgument='<%# Eval("Id_User") %>' OnClientClick="return confirm('Are you sure you want to permanently delete this user?');" ToolTip="Delete User">
                                                    <i class="fas fa-trash"></i> Delete
                                                </asp:LinkButton>
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                    </Columns>
                                </asp:GridView>
                            </div>
                        </ContentTemplate>
                    </asp:UpdatePanel>
                </div>
            </main>
        </div>

        <!-- MODALES DE USUARIO Y PERMISOS -->
        <asp:UpdatePanel ID="upModals" runat="server">
            <ContentTemplate>
                <!-- EDIT USER MODAL -->
                <asp:PlaceHolder ID="phEditUserModal" runat="server" Visible="false">
                    <div class="modal-backdrop-custom">
                        <div class="modal-dialog-custom">
                            <div class="modal-content" style="border: none; box-shadow: none;">
                                <div class="modal-header">
                                    <h5 class="modal-title font-weight-bold" style="color: #FFC800;"><i class="fas fa-user-edit mr-2"></i>Edit User</h5>
                                    <asp:LinkButton ID="btnCloseEdit" runat="server" OnClick="btnCloseEdit_Click" CssClass="close text-decoration-none" aria-label="Close">
                                        <span aria-hidden="true">&times;</span>
                                    </asp:LinkButton>
                                </div>
                                <div class="modal-body text-left">
                                    <asp:HiddenField ID="hfEditUserId" runat="server" />
                                    <div class="form-group">
                                        <label>Username</label>
                                        <asp:TextBox ID="txtEditUsername" runat="server" CssClass="form-control"></asp:TextBox>
                                    </div>
                                    <div class="form-group">
                                        <label>Email</label>
                                        <asp:TextBox ID="txtEditEmail" runat="server" CssClass="form-control" TextMode="Email"></asp:TextBox>
                                    </div>
                                    <div class="form-group">
                                        <label>New Password <small class="text-muted">(Leave blank to keep current)</small></label>
                                        <asp:TextBox ID="txtEditPass" runat="server" CssClass="form-control" TextMode="Password"></asp:TextBox>
                                    </div>
                                    <div class="form-group">
                                        <label>Role</label>
                                        <asp:DropDownList ID="ddlEditRole" runat="server" CssClass="form-control">
                                            <asp:ListItem Value="3">Customer</asp:ListItem>
                                            <asp:ListItem Value="2">Admin</asp:ListItem>
                                            <asp:ListItem Value="4">Delivery</asp:ListItem>
                                        </asp:DropDownList>
                                    </div>
                                </div>
                                <div class="modal-footer">
                                    <asp:Button ID="btnUpdateUser" runat="server" Text="Save Changes" CssClass="btn btn-warning font-weight-bold" OnClick="btnUpdateUser_Click" />
                                </div>
                            </div>
                        </div>
                    </div>
                </asp:PlaceHolder>

                <!-- PERMISSIONS MODAL -->
                <asp:PlaceHolder ID="phPermissionsModal" runat="server" Visible="false">
                    <div class="modal-backdrop-custom">
                        <div class="modal-dialog-custom" style="max-width: 800px;">
                            <div class="modal-content" style="border: none; box-shadow: none;">
                                <div class="modal-header">
                                    <h5 class="modal-title font-weight-bold" style="color: #10b981;"><i class="fas fa-shield-alt mr-2"></i>Admin Module Permissions</h5>
                                    <asp:LinkButton ID="btnClosePerms" runat="server" OnClick="btnClosePerms_Click" CssClass="close text-decoration-none" aria-label="Close">
                                        <span aria-hidden="true">&times;</span>
                                    </asp:LinkButton>
                                </div>
                                <div class="modal-body text-left">
                                    <p class="text-muted mb-4">Assign specific administrative modules to this user. They will only see the selected options in their sidebar.</p>
                                    <asp:HiddenField ID="hfPermUserId" runat="server" />
                                    
                                    <div class="perm-card">
                                        <div class="d-flex align-items-center">
                                            <asp:CheckBox ID="chkModalPermProducts" runat="server" CssClass="mr-2" />
                                            <label class="perm-title mb-0">Manage Products & Catalog</label>
                                        </div>
                                        <div class="perm-desc">Grants access to add, edit, and delete products, as well as manage brands, leagues, and teams.</div>
                                    </div>

                                    <div class="perm-card">
                                        <div class="d-flex align-items-center">
                                            <asp:CheckBox ID="chkModalPermOrders" runat="server" CssClass="mr-2" />
                                            <label class="perm-title mb-0">Manage Orders</label>
                                        </div>
                                        <div class="perm-desc">Grants access to view, process, and update the status of customer orders and refunds.</div>
                                    </div>

                                    <div class="perm-card">
                                        <div class="d-flex align-items-center">
                                            <asp:CheckBox ID="chkModalPermOffers" runat="server" CssClass="mr-2" />
                                            <label class="perm-title mb-0">Manage Offers</label>
                                        </div>
                                        <div class="perm-desc">Grants access to create and manage seasonal discounts and sale pricing across the catalog.</div>
                                    </div>

                                    <div class="perm-card">
                                        <div class="d-flex align-items-center">
                                            <asp:CheckBox ID="chkModalPermCoupons" runat="server" CssClass="mr-2" />
                                            <label class="perm-title mb-0">Manage Coupons</label>
                                        </div>
                                        <div class="perm-desc">Grants access to create, toggle, and delete checkout discount codes.</div>
                                    </div>

                                    <div class="perm-card">
                                        <div class="d-flex align-items-center">
                                            <asp:CheckBox ID="chkModalPermBanners" runat="server" CssClass="mr-2" />
                                            <label class="perm-title mb-0">Manage Banners</label>
                                        </div>
                                        <div class="perm-desc">Grants access to update homepage promotional carousel banners and images.</div>
                                    </div>

                                    <div class="perm-card">
                                        <div class="d-flex align-items-center">
                                            <asp:CheckBox ID="chkModalPermTickets" runat="server" CssClass="mr-2" />
                                            <label class="perm-title mb-0">Manage Seller Requests (Tickets)</label>
                                        </div>
                                        <div class="perm-desc">Grants access to review and approve/reject incoming seller applications.</div>
                                    </div>
                                </div>
                                <div class="modal-footer">
                                    <asp:Button ID="btnSavePermissions" runat="server" Text="Save Configuration" CssClass="btn btn-success font-weight-bold" OnClick="btnSavePermissions_Click" />
                                </div>
                            </div>
                        </div>
                    </div>
                </asp:PlaceHolder>
            </ContentTemplate>
        </asp:UpdatePanel>

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
                if (isDark && themeIcon) { themeIcon.className = 'fas fa-sun'; }
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