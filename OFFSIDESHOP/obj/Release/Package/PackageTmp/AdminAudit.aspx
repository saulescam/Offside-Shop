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

        /* ── Paginación ── */
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
                    <li>
                        <asp:Button ID="btnManageProducts" CssClass="sidebar-btn" runat="server" Text="&#xf553; Manage Products" OnClick="btnManageProducts_Click" Style="font-family: 'Raleway', 'Font Awesome 5 Free'; font-weight: 600;" />
                    </li>
                    <li>
                        <a id="btnManageOrders" runat="server" href="ManageOrders.aspx" class="sidebar-btn" style="font-family: 'Raleway','Font Awesome 5 Free'; font-weight: 600;">&#xf46d; Manage Orders</a>
                    </li>
                    <li>
                        <asp:Button ID="btnManageOffers" CssClass="sidebar-btn" runat="server" Text="&#xf155; Manage Offers" OnClick="btnManageOffers_Click" Style="font-family: 'Raleway', 'Font Awesome 5 Free'; font-weight: 600;" />
                    </li>
                    <li>
                        <asp:Button ID="btnManageCoupons" CssClass="sidebar-btn" runat="server" Text="&#xf02c; Manage Coupons" OnClick="btnManageCoupons_Click" Style="font-family: 'Raleway', 'Font Awesome 5 Free'; font-weight: 600;" /></li>
                    <li><a id="btnManageTickets" runat="server" href="ManageSellerRequests.aspx" class="sidebar-btn" style="font-family: 'Raleway','Font Awesome 5 Free'; font-weight: 600;">&#xf2b5; Seller Requests</a></li>
                    <li style="border-top: 1px solid var(--border-color); margin-top: 8px; padding-top: 8px;">
                        <asp:Button ID="btnAddLeague" CssClass="sidebar-btn" runat="server" Text="&#xf1ae; Add League" OnClick="btnAddLeague_Click" Style="font-family: 'Raleway', 'Font Awesome 5 Free'; font-weight: 600;" />
                    </li>
                    <li>
                        <asp:Button ID="btnAddTeam" CssClass="sidebar-btn" runat="server" Text="&#xf0c0; Add Team" OnClick="btnAddTeam_Click" Style="font-family: 'Raleway', 'Font Awesome 5 Free'; font-weight: 600;" />
                    </li>
                    <li>
                        <asp:Button ID="btnAddBrand" CssClass="sidebar-btn" runat="server" Text="&#xf0c0; Add Brand" OnClick="btnAddBrand_Click" Style="font-family: 'Raleway', 'Font Awesome 5 Free'; font-weight: 600;" />
                    </li>

                    <asp:PlaceHolder ID="phOwnerMenu" runat="server">
                        <li style="border-top: 1px solid var(--border-color); margin-top: 8px; padding-top: 8px;">
                            <asp:Button ID="btnManageUsers" CssClass="sidebar-btn" runat="server" Text="&#xf4fe; Manage Users" OnClick="btnManageUsers_Click" Style="font-family: 'Raleway', 'Font Awesome 5 Free'; font-weight: 600;" />
                        </li>
                        <li>
                            <asp:Button ID="btnSmtpSettings" CssClass="sidebar-btn" runat="server" Text="&#xf0e0; SMTP Settings" OnClick="btnSmtpSettings_Click" Style="font-family: 'Raleway', 'Font Awesome 5 Free'; font-weight: 600;" />
                        </li>
                        <li>
                            <asp:Button ID="btnStats" CssClass="sidebar-btn" runat="server" Text="&#xf080; Stats" OnClick="btnStats_Click" Style="font-family: 'Raleway', 'Font Awesome 5 Free'; font-weight: 600;" />
                        </li>
                        <li>
                            <asp:Button ID="btnAuditLogs" CssClass="sidebar-btn active" runat="server" Text="&#xf03a; Audit Logs" OnClick="btnAuditLogs_Click" Style="font-family: 'Raleway', 'Font Awesome 5 Free'; font-weight: 600;" />
                        </li>
                    </asp:PlaceHolder>
                    <li>
                        <asp:Button ID="btnAdminBanners" CssClass="sidebar-btn" runat="server" Text="&#xf03e; Manage Banners" OnClick="btnAdminBanners_Click" Style="font-family: 'Raleway', 'Font Awesome 5 Free'; font-weight: 600;" />
                    </li>

                    <li style="border-top: 1px solid var(--border-color); margin-top: 8px; padding-top: 8px;">
                        <asp:Button ID="btncerrar" CssClass="sidebar-btn btn-logout" runat="server" Text="&#xf2f5; Logout" OnClick="btncerrar_Click" Style="font-family: 'Raleway', 'Font Awesome 5 Free'; font-weight: 600;" />
                    </li>
                </ul>
            </aside>

            <main class="main-content fade-in" style="animation-delay: 0.15s;">
                <div class="container-fluid">
                    <h1 class="page-title">System Audit Logs</h1>
                    <p class="text-muted mb-4">Monitor administrative activities, system events, and security actions performed by administrators.</p>

                    <asp:UpdatePanel ID="upAuditLogs" runat="server">
                        <ContentTemplate>
                            <div class="filter-card">
                                <div class="row align-items-end">
                                    <div class="col-md-3 col-sm-6 mb-2 mb-md-0">
                                        <label for="ddlFilterAction">Action Type</label>
                                        <asp:DropDownList ID="ddlFilterAction" runat="server"
                                            CssClass="form-control"
                                            AutoPostBack="true"
                                            OnSelectedIndexChanged="Filter_Changed">
                                        </asp:DropDownList>
                                    </div>
                                    <div class="col-md-3 col-sm-6 mb-2 mb-md-0">
                                        <label for="ddlFilterModule">Module</label>
                                        <asp:DropDownList ID="ddlFilterModule" runat="server"
                                            CssClass="form-control"
                                            AutoPostBack="true"
                                            OnSelectedIndexChanged="Filter_Changed">
                                        </asp:DropDownList>
                                    </div>
                                    <div class="col-md-4 col-sm-6 mb-2 mb-md-0">
                                        <label for="txtSearch">Search Details</label>
                                        <asp:TextBox ID="txtSearch" runat="server"
                                            CssClass="form-control"
                                            placeholder="Search description, admin or IP..."
                                            AutoPostBack="true"
                                            OnTextChanged="Filter_Changed">
                                        </asp:TextBox>
                                    </div>
                                    <div class="col-md-2 col-sm-6 text-right">
                                        <asp:Button ID="btnClear" runat="server" Text="Clear Filters"
                                            CssClass="btn btn-secondary btn-block font-weight-bold"
                                            OnClick="btnClear_Click" Style="border-radius: 8px; padding: 8px;" />
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
                                    EmptyDataText="No audit log records match your selection criteria.">
                                    <PagerStyle CssClass="pagination-custom" HorizontalAlign="Center" />
                                    <Columns>
                                        <asp:BoundField DataField="Id_Log" HeaderText="Log ID" ItemStyle-Width="80px" />
                                        <asp:BoundField DataField="AdminName" HeaderText="Administrator" ItemStyle-HorizontalAlign="Left" />
                                        <asp:BoundField DataField="Action_Type" HeaderText="Action Type" />
                                        <asp:BoundField DataField="Module" HeaderText="Module" />
                                        <asp:BoundField DataField="Description" HeaderText="Description" ItemStyle-HorizontalAlign="Left" />
                                        <asp:BoundField DataField="IP_Address" HeaderText="IP Address" ItemStyle-Width="130px" />
                                        <asp:BoundField DataField="Created_At" HeaderText="Timestamp" DataFormatString="{0:yyyy-MM-dd HH:mm:ss}" ItemStyle-Width="160px" />
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

