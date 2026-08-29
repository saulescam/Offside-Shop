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
            text-decoration: none !important;
            display: inline-block;
        }
        .btn-edit { background: #3b82f6; color: white !important; }
        .btn-edit:hover { background: #2563eb; }
        .btn-perm { background: #10b981; color: white !important; }
        .btn-perm:hover { background: #059669; }
        .btn-delete { background: #ef4444; color: white !important; }
        .btn-delete:hover { background: #dc2626; }
        
        /* Paginación */
        .pagination-custom td {
            padding: 24px 4px 10px 4px;
        }

        .pagination-custom a, .pagination-custom span {
            display: inline-block;
            padding: 8px 16px;
            margin: 0 4px;
            border-radius: 8px;
            font-size: 0.9rem;
            font-weight: 700;
            text-decoration: none;
            transition: all 0.25s ease;
        }

        .pagination-custom a {
            background-color: #f9fafb;
            color: #b45309;
            border: 1px solid #f59e0b;
        }

        .pagination-custom a:hover {
            background: linear-gradient(135deg, #d97706, #b45309);
            color: #ffffff !important;
            border-color: transparent;
            transform: translateY(-1px);
            box-shadow: 0 4px 12px rgba(217, 119, 6, 0.3);
        }

        .pagination-custom span {
            background: linear-gradient(135deg, #f59e0b, #d97706);
            color: #ffffff;
            border: 1px solid transparent;
            box-shadow: 0 4px 12px rgba(245, 158, 11, 0.4);
        }

        html.dark-mode .pagination-custom a {
            background-color: #1f2937;
            color: #fbbf24;
            border: 1px solid #d97706;
        }

        html.dark-mode .pagination-custom a:hover {
            background: linear-gradient(135deg, #d97706, #b45309);
            color: #ffffff !important;
            border-color: transparent;
        }

        html.dark-mode .pagination-custom span {
            background: linear-gradient(135deg, #f59e0b, #d97706);
            color: #ffffff;
            border: 1px solid transparent;
        }
        
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

        /* Estilo del botón de cambio de idioma */
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

    <script type="text/javascript">
        window.onpageshow = function (event) {
            if (event.persisted) { window.location.reload(); }
        };
    </script>
</head>
<body>
    <form id="form1" runat="server">
        <asp:ScriptManager ID="ScriptManager1" runat="server" />
        
        <!-- TOP NAVBAR -->
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
            <!-- SIDEBAR -->
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
                        <asp:LinkButton ID="btnAddLeague" CssClass="sidebar-btn" runat="server" OnClick="btnAddLeague_Click" Style="font-family: 'Raleway', 'Font Awesome 5 Free'; font-weight: 600;">
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
                            <asp:LinkButton ID="btnManageUsers" CssClass="sidebar-btn active" runat="server" OnClick="btnManageUsers_Click" Style="font-family: 'Raleway', 'Font Awesome 5 Free'; font-weight: 600;">
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

            <!-- MAIN CONTENT -->
            <main class="main-content fade-in" style="animation-delay: 0.2s;">
                <div class="container-fluid">
                    <h1 class="page-title"><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Users_Title %>" /></h1>
                    <p class="text-muted mb-4"><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Users_Subtitle %>" /></p>

                    <asp:UpdatePanel ID="upMain" runat="server">
                        <ContentTemplate>
                            <!-- Create New User Form Card -->
                            <div class="form-card mb-4">
                                <h4 class="text-white mb-3" style="font-weight: 700;"><i class="fas fa-user-plus mr-2"></i><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Users_CreateTitle %>" /></h4>
                                <div class="row">
                                    <div class="col-md-3 form-group">
                                        <label><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Users_Username %>" /> <span class="text-danger">*</span></label>
                                        <asp:TextBox ID="txtNewUser" runat="server" CssClass="form-control" placeholder="e.g. jsmith"></asp:TextBox>
                                    </div>
                                    <div class="col-md-3 form-group">
                                        <label><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Users_Email %>" /> <span class="text-danger">*</span></label>
                                        <asp:TextBox ID="txtNewEmail" runat="server" CssClass="form-control" TextMode="Email" placeholder="user@domain.com"></asp:TextBox>
                                    </div>
                                    <div class="col-md-3 form-group">
                                        <label><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Users_Password %>" /> <span class="text-danger">*</span></label>
                                        <asp:TextBox ID="txtNewPass" runat="server" CssClass="form-control" TextMode="Password" placeholder="••••••••"></asp:TextBox>
                                    </div>
                                    <div class="col-md-3 form-group">
                                        <label><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Users_Role %>" /> <span class="text-danger">*</span></label>
                                        <asp:DropDownList ID="ddlNewRole" runat="server" CssClass="form-control">
                                        </asp:DropDownList>
                                        <small class="text-muted d-block mt-1"><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Users_RoleNote %>" /></small>
                                    </div>
                                </div>
                                <asp:LinkButton ID="btnCreateUser" runat="server" CssClass="mybtn" OnClick="btnCreateUser_Click" Style="font-family: 'Raleway', 'Font Awesome 5 Free'; font-weight: 600; width: auto; padding: 10px 25px; text-decoration: none; display: inline-block;">
                                    &#xf0c7;&nbsp; <asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Users_BtnCreate %>" />
                                </asp:LinkButton>
                            </div>

                            <!-- FILTROS DE BÚSQUEDA Y ROLES -->
                            <div class="filter-card mb-4">
                                <div class="row align-items-end">
                                    <div class="col-md-3 col-sm-6 mb-2 mb-md-0">
                                        <label><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Users_FilterRole %>" /></label>
                                        <asp:DropDownList ID="ddlFilterRole" runat="server" CssClass="form-control" AutoPostBack="true" OnSelectedIndexChanged="Filter_Changed">
                                        </asp:DropDownList>
                                    </div>
                                    <div class="col-md-3 col-sm-6 mb-2 mb-md-0">
                                        <label><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Users_FilterDeliveryStatus %>" /></label>
                                        <asp:DropDownList ID="ddlFilterDeliveryStatus" runat="server" CssClass="form-control" AutoPostBack="true" OnSelectedIndexChanged="Filter_Changed">
                                        </asp:DropDownList>
                                    </div>
                                    <div class="col-md-4 col-sm-8 mb-2 mb-md-0">
                                        <label><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Users_SearchLabel %>" /></label>
                                        <asp:TextBox ID="txtSearchUser" runat="server" CssClass="form-control" placeholder="<%$ Resources:Strings, Placeholder_SearchUser %>" AutoPostBack="true" OnTextChanged="Filter_Changed"></asp:TextBox>
                                    </div>
                                    <div class="col-md-2 col-sm-4 text-right">
                                        <asp:LinkButton ID="btnClearFilters" runat="server" CssClass="btn btn-outline-secondary font-weight-bold w-100" OnClick="btnClearFilters_Click">
                                            <i class="fas fa-eraser mr-1"></i> <asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Users_ClearFilters %>" />
                                        </asp:LinkButton>
                                    </div>
                                </div>
                            </div>

                            <h3 class="text-white mb-3" style="font-weight: 700;"><i class="fas fa-users mr-2"></i><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Users_RegisteredTitle %>" /></h3>
                            <div class="table-responsive">
                                <asp:GridView ID="gvUsers" runat="server" AutoGenerateColumns="False" GridLines="None" CssClass="table table-custom text-center align-middle" DataKeyNames="Id_User" AllowPaging="true" PageSize="10" OnRowCommand="gvUsers_RowCommand" OnRowDataBound="gvUsers_RowDataBound" OnPageIndexChanging="gvUsers_PageIndexChanging">
                                    <PagerStyle CssClass="pagination-custom" HorizontalAlign="Center" />
                                    <Columns>
                                        <asp:BoundField DataField="Id_User" HeaderText="ID" ItemStyle-Width="60px" />
                                        <asp:BoundField DataField="Name_User" HeaderText="<%$ Resources:Strings, Admin_Users_Username %>" />
                                        <asp:BoundField DataField="Mail" HeaderText="<%$ Resources:Strings, Admin_Users_Email %>" />
                                        <asp:TemplateField HeaderText="<%$ Resources:Strings, Admin_Users_ColCurrentRole %>">
                                            <ItemTemplate>
                                                <span class="badge badge-dark p-2" style="font-size: 0.85rem;"><%# GetLocalizedRoleName(Eval("Id_Role"), Eval("Name_Role")) %></span>
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                        
                                        <asp:TemplateField HeaderText="<%$ Resources:Strings, Admin_Users_ColDeliveryStatus %>">
                                            <ItemTemplate>
                                                <asp:Label ID="lblDeliveryStatusBadge" runat="server"></asp:Label>
                                            </ItemTemplate>
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="<%$ Resources:Strings, Admin_Users_ColActions %>">
                                            <ItemTemplate>
                                                <asp:Label ID="lblOwnerProtect" runat="server" Visible="false"></asp:Label>
                                                
                                                <asp:LinkButton ID="btnEdit" runat="server" CssClass="action-btn btn-edit" CommandName="EditUser" CommandArgument='<%# Eval("Id_User") %>' ToolTip="Edit User Data">
                                                    <i class="fas fa-pen"></i> <asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Users_BtnEdit %>" />
                                                </asp:LinkButton>
                                                
                                                <asp:LinkButton ID="btnPermissions" runat="server" CssClass="action-btn btn-perm" CommandName="ManagePermissions" CommandArgument='<%# Eval("Id_User") %>' ToolTip="Configure Module Access">
                                                    <i class="fas fa-key"></i> <asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Users_BtnPermissions %>" />
                                                </asp:LinkButton>
                                                
                                                <asp:LinkButton ID="btnDelete" runat="server" CssClass="action-btn btn-delete" CommandName="DeleteUser" CommandArgument='<%# Eval("Id_User") %>' OnClientClick='<%# "return confirm(\"" + GetGlobalResourceObject("Strings", "Confirm_DeleteUser") + "\");" %>' ToolTip="Delete User">
                                                    <i class="fas fa-trash"></i> <asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Users_BtnDelete %>" />
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
                                    <h5 class="modal-title font-weight-bold" style="color: #FFC800;"><i class="fas fa-user-edit mr-2"></i><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Users_EditTitle %>" /></h5>
                                    <asp:LinkButton ID="btnCloseEdit" runat="server" OnClick="btnCloseEdit_Click" CssClass="close text-decoration-none" aria-label="Close">
                                        <span aria-hidden="true">&times;</span>
                                    </asp:LinkButton>
                                </div>
                                <div class="modal-body text-left">
                                    <asp:HiddenField ID="hfEditUserId" runat="server" />
                                    <div class="form-group">
                                        <label><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Users_Username %>" /></label>
                                        <asp:TextBox ID="txtEditUsername" runat="server" CssClass="form-control"></asp:TextBox>
                                    </div>
                                    <div class="form-group">
                                        <label><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Users_Email %>" /></label>
                                        <asp:TextBox ID="txtEditEmail" runat="server" CssClass="form-control" TextMode="Email"></asp:TextBox>
                                    </div>
                                    <div class="form-group">
                                        <label><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Users_NewPassLabel %>" /> <small class="text-muted"><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Users_NewPassNote %>" /></small></label>
                                        <asp:TextBox ID="txtEditPass" runat="server" CssClass="form-control" TextMode="Password"></asp:TextBox>
                                    </div>
                                    <div class="form-group">
                                        <label><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Users_Role %>" /></label>
                                        <asp:DropDownList ID="ddlEditRole" runat="server" CssClass="form-control">
                                        </asp:DropDownList>
                                    </div>
                                </div>
                                <div class="modal-footer">
                                    <asp:Button ID="btnUpdateUser" runat="server" Text="<%$ Resources:Strings, Admin_Users_SaveChanges %>" CssClass="btn btn-warning font-weight-bold" OnClick="btnUpdateUser_Click" />
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
                                    <h5 class="modal-title font-weight-bold" style="color: #10b981;"><i class="fas fa-shield-alt mr-2"></i><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Users_PermModalTitle %>" /></h5>
                                    <asp:LinkButton ID="btnClosePerms" runat="server" OnClick="btnClosePerms_Click" CssClass="close text-decoration-none" aria-label="Close">
                                        <span aria-hidden="true">&times;</span>
                                    </asp:LinkButton>
                                </div>
                                <div class="modal-body text-left">
                                    <p class="text-muted mb-4"><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Users_PermModalDesc %>" /></p>
                                    <asp:HiddenField ID="hfPermUserId" runat="server" />
                                    
                                    <div class="perm-card">
                                        <div class="d-flex align-items-center">
                                            <asp:CheckBox ID="chkModalPermProducts" runat="server" CssClass="mr-2" />
                                            <label class="perm-title mb-0"><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Users_PermProductsTitle %>" /></label>
                                        </div>
                                        <div class="perm-desc"><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Users_PermProductsDesc %>" /></div>
                                    </div>

                                    <div class="perm-card">
                                        <div class="d-flex align-items-center">
                                            <asp:CheckBox ID="chkModalPermOrders" runat="server" CssClass="mr-2" />
                                            <label class="perm-title mb-0"><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Users_PermOrdersTitle %>" /></label>
                                        </div>
                                        <div class="perm-desc"><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Users_PermOrdersDesc %>" /></div>
                                    </div>

                                    <div class="perm-card">
                                        <div class="d-flex align-items-center">
                                            <asp:CheckBox ID="chkModalPermOffers" runat="server" CssClass="mr-2" />
                                            <label class="perm-title mb-0"><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Users_PermOffersTitle %>" /></label>
                                        </div>
                                        <div class="perm-desc"><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Users_PermOffersDesc %>" /></div>
                                    </div>

                                    <div class="perm-card">
                                        <div class="d-flex align-items-center">
                                            <asp:CheckBox ID="chkModalPermCoupons" runat="server" CssClass="mr-2" />
                                            <label class="perm-title mb-0"><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Users_PermCouponsTitle %>" /></label>
                                        </div>
                                        <div class="perm-desc"><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Users_PermCouponsDesc %>" /></div>
                                    </div>

                                    <div class="perm-card">
                                        <div class="d-flex align-items-center">
                                            <asp:CheckBox ID="chkModalPermBanners" runat="server" CssClass="mr-2" />
                                            <label class="perm-title mb-0"><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Users_PermBannersTitle %>" /></label>
                                        </div>
                                        <div class="perm-desc"><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Users_PermBannersDesc %>" /></div>
                                    </div>

                                    <div class="perm-card">
                                        <div class="d-flex align-items-center">
                                            <asp:CheckBox ID="chkModalPermTickets" runat="server" CssClass="mr-2" />
                                            <label class="perm-title mb-0"><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Users_PermTicketsTitle %>" /></label>
                                        </div>
                                        <div class="perm-desc"><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Users_PermTicketsDesc %>" /></div>
                                    </div>
                                </div>
                                <div class="modal-footer">
                                    <asp:Button ID="btnSavePermissions" runat="server" Text="<%$ Resources:Strings, Admin_Users_SavePermissions %>" CssClass="btn btn-success font-weight-bold" OnClick="btnSavePermissions_Click" />
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