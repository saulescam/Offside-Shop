<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="AdminAudit.aspx.cs" Inherits="OFFSIDESHOP.AdminAudit" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1, shrink-to-fit=no" />
    <title>Audit Logs | OffsideShop</title>

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
    <script src="SweetAlert/sweetalert2.js"></script>

    <script type="text/javascript">
        window.onpageshow = function (event) {
            if (event.persisted) { window.location.reload(); }
        };
    </script>

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

        /* Paginación */
        .pagination-custom td { padding: 24px 4px 10px 4px; }
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
        .pagination-custom a { background-color: #f9fafb; color: #b45309; border: 1px solid #f59e0b; }
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
        html.dark-mode .pagination-custom a { background-color: #1f2937; color: #fbbf24; border: 1px solid #d97706; }
        html.dark-mode .pagination-custom a:hover {
            background: linear-gradient(135deg, #f59e0b, #d97706);
            color: #111827 !important;
            border-color: transparent;
            box-shadow: 0 5px 15px rgba(245, 158, 11, 0.4);
        }
        html.dark-mode .pagination-custom span {
            background: linear-gradient(135deg, #fbbf24, #f59e0b);
            color: #111827;
            border: 1px solid transparent;
            box-shadow: 0 5px 15px rgba(251, 191, 36, 0.5);
        }

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
        <asp:ScriptManager ID="ScriptManager1" runat="server" />

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
                            <asp:LinkButton ID="btnAuditLogs" CssClass="sidebar-btn active" runat="server" OnClick="btnAuditLogs_Click" Style="font-family: 'Raleway', 'Font Awesome 5 Free'; font-weight: 600;">
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

            <main class="main-content fade-in" style="animation-delay: 0.15s;">
                <div class="container-fluid">
                    <h1 class="page-title"><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Audit_Title %>" /></h1>
                    <p class="text-muted mb-4"><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Audit_Subtitle %>" /></p>

                    <asp:UpdatePanel ID="upAuditLogs" runat="server">
                        <ContentTemplate>
                            <div class="filter-card">
                                <div class="row align-items-end">
                                    <div class="col-md-3 col-sm-6 mb-2 mb-md-0">
                                        <label for="ddlFilterAction"><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Audit_FilterAction %>" /></label>
                                        <asp:DropDownList ID="ddlFilterAction" runat="server"
                                            CssClass="form-control"
                                            AutoPostBack="true"
                                            OnSelectedIndexChanged="Filter_Changed">
                                        </asp:DropDownList>
                                    </div>
                                    <div class="col-md-3 col-sm-6 mb-2 mb-md-0">
                                        <label for="ddlFilterModule"><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Audit_FilterModule %>" /></label>
                                        <asp:DropDownList ID="ddlFilterModule" runat="server"
                                            CssClass="form-control"
                                            AutoPostBack="true"
                                            OnSelectedIndexChanged="Filter_Changed">
                                        </asp:DropDownList>
                                    </div>
                                    <div class="col-md-4 col-sm-6 mb-2 mb-md-0">
                                        <label for="txtSearch"><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Audit_FilterSearch %>" /></label>
                                        <asp:TextBox ID="txtSearch" runat="server"
                                            CssClass="form-control"
                                            placeholder="Search description, admin or IP..."
                                            AutoPostBack="true"
                                            OnTextChanged="Filter_Changed">
                                        </asp:TextBox>
                                    </div>
                                    <div class="col-md-2 col-sm-6 text-right">
                                        <asp:LinkButton ID="btnClear" runat="server"
                                            CssClass="btn btn-secondary btn-block font-weight-bold"
                                            OnClick="btnClear_Click" Style="border-radius: 8px; padding: 8px; text-decoration: none; display: block;">
                                            <asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Audit_ClearFilters %>" />
                                        </asp:LinkButton>
                                    </div>
                                </div>
                            </div>

                            <div class="table-responsive">
                                <asp:GridView ID="gvAuditLogs" runat="server"
                                    AutoGenerateColumns="False"
                                    GridLines="None"
                                    CssClass="table table-custom text-center align-middle"
                                    DataKeyNames="Id_Log"
                                    AllowPaging="true"
                                    PageSize="20"
                                    OnPageIndexChanging="gvAuditLogs_PageIndexChanging"
                                    EmptyDataText="<%$ Resources:Strings, Admin_Audit_Empty %>">
                                    <PagerStyle CssClass="pagination-custom" HorizontalAlign="Center" />
                                    <Columns>
                                        <asp:BoundField DataField="Id_Log" HeaderText="<%$ Resources:Strings, Admin_Audit_ColLogId %>" ItemStyle-Width="80px" />
                                        <asp:BoundField DataField="AdminName" HeaderText="<%$ Resources:Strings, Admin_Audit_ColAdmin %>" ItemStyle-HorizontalAlign="Left" />
                                        <asp:BoundField DataField="Action_Type" HeaderText="<%$ Resources:Strings, Admin_Audit_ColActionType %>" />
                                        <asp:BoundField DataField="Module" HeaderText="<%$ Resources:Strings, Admin_Audit_ColModule %>" />
                                        <asp:BoundField DataField="Description" HeaderText="<%$ Resources:Strings, Admin_Audit_ColDescription %>" ItemStyle-HorizontalAlign="Left" />
                                        <asp:BoundField DataField="IP_Address" HeaderText="<%$ Resources:Strings, Admin_Audit_ColIp %>" ItemStyle-Width="130px" />
                                        <asp:BoundField DataField="Created_At" HeaderText="<%$ Resources:Strings, Admin_Audit_ColTimestamp %>" DataFormatString="{0:yyyy-MM-dd HH:mm:ss}" ItemStyle-Width="160px" />
                                    </Columns>
                                </asp:GridView>
                            </div>
                        </ContentTemplate>
                    </asp:UpdatePanel>
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