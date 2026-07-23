<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Dashboard.aspx.cs" Inherits="OFFSIDESHOP.Dashboard" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1, shrink-to-fit=no" />
    <title>Admin Dashboard | OffsideShop</title>

    <link href="css/bootstrap.min.css" rel="stylesheet" />
    <link href="https://fonts.googleapis.com/css?family=Raleway:100,400,600,700&display=swap" rel="stylesheet" />
    <link rel="stylesheet" href="https://maxcdn.bootstrapcdn.com/bootstrap/4.3.1/css/bootstrap.min.css" />
    <link href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/5.15.4/css/all.min.css" rel="stylesheet" />

    <link rel="stylesheet" type="text/css" href="https://cdnjs.cloudflare.com/ajax/libs/slick-carousel/1.8.1/slick.min.css" />
    <link rel="stylesheet" type="text/css" href="https://cdnjs.cloudflare.com/ajax/libs/slick-carousel/1.8.1/slick-theme.min.css" />

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

    <style>
        /* Estilos de las tarjetas de estadísticas */
        .card-stat-window {
            max-width: 100% !important;
            display: flex !important;
            flex-direction: column !important;
            align-items: stretch !important;
            height: 100% !important;
            padding: 20px !important;
            margin: 0 !important;
        }

        .stat-info-container {
            display: flex;
            flex-direction: column;
            justify-content: center;
        }

        .stat-details-footer {
            border-top: 1px solid var(--border-color) !important;
        }

        /* Recent Workspaces scrollable grids to occupy same space */
        .workspace-card {
            display: flex;
            flex-direction: column;
            height: 350px !important; /* Uniform height for all blocks */
        }

        .workspace-card .table-responsive {
            flex: 1;
            overflow-y: auto;
            max-height: 280px;
            border-radius: 0 0 15px 15px;
        }

        /* Custom premium scrollbar for workspaces */
        .workspace-card .table-responsive::-webkit-scrollbar {
            width: 6px;
        }
        .workspace-card .table-responsive::-webkit-scrollbar-track {
            background: transparent;
        }
        .workspace-card .table-responsive::-webkit-scrollbar-thumb {
            background: rgba(255, 200, 0, 0.4);
            border-radius: 10px;
        }
        .workspace-card .table-responsive::-webkit-scrollbar-thumb:hover {
            background: rgba(255, 200, 0, 0.7);
        }

        /* Sticky table headers for scrollable grids */
        .table-custom thead th {
            position: sticky;
            top: 0;
            z-index: 10;
            background: var(--table-header-bg) !important;
            box-shadow: 0 2px 2px -1px rgba(0,0,0,0.4);
        }

        .badge-warning.text-dark {
            color: #111111 !important;
        }

        /* Estilos específicos para modo oscuro y claro de la tabla */
        .table-custom td small {
            color: var(--text-muted) !important;
        }
    </style>

    <script type="text/javascript">
        window.onpageshow = function (event) {
            if (event.persisted) {
                window.location.reload();
            }
        };
    </script>
</head>
<body>
    <form id="form1" runat="server">
        <nav class="top-navbar">
            <a class="navbar-brand" href="Homepage.aspx">
                <img src="assets/img/offsideshop_logo_white_letras.png" alt="OFFSIDESHOP" />
            </a>
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
                            <asp:Button ID="btnAuditLogs" CssClass="sidebar-btn" runat="server" Text="&#xf03a; Audit Logs" OnClick="btnAuditLogs_Click" Style="font-family: 'Raleway', 'Font Awesome 5 Free'; font-weight: 600;" />
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

            <main class="main-content fade-in" style="animation-delay: 0.2s;">

                <div class="mb-4">
                    <h1 class="page-title mb-1">Welcome, <asp:Label ID="lblAdminName" runat="server"></asp:Label></h1>
                    <asp:PlaceHolder ID="phAdminPermissions" runat="server" Visible="false">
                        <p class="text-warning font-weight-bold m-0" style="font-size: 1.1rem; letter-spacing: 0.5px;">
                            <i class="fas fa-key mr-2"></i><asp:Label ID="lblAdminPermissions" runat="server"></asp:Label>
                        </p>
                    </asp:PlaceHolder>
                </div>

                <!-- Stats Grid Section -->
                <div class="stats-grid-container mb-5">
                    <div class="row">
                        <!-- Registered Users -->
                        <div class="col-xl-3 col-md-6 mb-4">
                            <div class="stat-box-dark card-stat-window" style="border-left: 4px solid #007bff !important;">
                                <div class="d-flex align-items-center">
                                    <div class="stat-icon-dark text-primary">
                                        <i class="fas fa-users fa-lg"></i>
                                    </div>
                                    <div class="stat-info-container">
                                        <h6>Registered Users</h6>
                                        <asp:Label ID="lblUserCount" runat="server" CssClass="stat-value" Text="0"></asp:Label>
                                    </div>
                                </div>
                                <div class="stat-details-footer mt-3 pt-2">
                                    <small class="text-muted"><i class="fas fa-info-circle mr-1"></i>Total active members registered</small>
                                </div>
                            </div>
                        </div>

                        <!-- Orders (7 Days) -->
                        <div class="col-xl-3 col-md-6 mb-4">
                            <div class="stat-box-dark card-stat-window" style="border-left: 4px solid #17a2b8 !important;">
                                <div class="d-flex align-items-center">
                                    <div class="stat-icon-dark text-info">
                                        <i class="fas fa-calendar-week fa-lg"></i>
                                    </div>
                                    <div class="stat-info-container">
                                        <h6>Orders (7 Days)</h6>
                                        <asp:Label ID="lblPurchasesLast7Days" runat="server" CssClass="stat-value" Text="0"></asp:Label>
                                    </div>
                                </div>
                                <div class="stat-details-footer mt-3 pt-2">
                                    <small class="text-muted"><i class="fas fa-chart-line mr-1"></i>Created in the last week</small>
                                </div>
                            </div>
                        </div>

                        <!-- Pending Orders -->
                        <div class="col-xl-3 col-md-6 mb-4">
                            <div class="stat-box-dark card-stat-window" style="border-left: 4px solid #ffc107 !important;">
                                <div class="d-flex align-items-center">
                                    <div class="stat-icon-dark text-warning">
                                        <i class="fas fa-boxes fa-lg"></i>
                                    </div>
                                    <div class="stat-info-container">
                                        <h6>Pending Orders</h6>
                                        <asp:Label ID="lblPendingOrders" runat="server" CssClass="stat-value text-warning" Text="0"></asp:Label>
                                    </div>
                                </div>
                                <div class="stat-details-footer mt-3 pt-2">
                                    <small class="text-muted"><i class="fas fa-clock mr-1"></i>Awaiting dispatch / processing</small>
                                </div>
                            </div>
                        </div>

                        <!-- Total Processed -->
                        <div class="col-xl-3 col-md-6 mb-4">
                            <div class="stat-box-dark card-stat-window" style="border-left: 4px solid #6c757d !important;">
                                <div class="d-flex align-items-center">
                                    <div class="stat-icon-dark text-secondary">
                                        <i class="fas fa-shopping-bag fa-lg"></i>
                                    </div>
                                    <div class="stat-info-container">
                                        <h6>Total Processed</h6>
                                        <asp:Label ID="lblTotalOrders" runat="server" CssClass="stat-value" Text="0"></asp:Label>
                                    </div>
                                </div>
                                <div class="stat-details-footer mt-3 pt-2">
                                    <small class="text-muted"><i class="fas fa-history mr-1"></i>Completed historical orders</small>
                                </div>
                            </div>
                        </div>

                        <!-- Jerseys Added -->
                        <div class="col-xl-3 col-md-6 mb-4">
                            <div class="stat-box-dark card-stat-window" style="border-left: 4px solid #dc3545 !important;">
                                <div class="d-flex align-items-center">
                                    <div class="stat-icon-dark text-danger">
                                        <i class="fas fa-tshirt fa-lg"></i>
                                    </div>
                                    <div class="stat-info-container">
                                        <h6>Jerseys Added</h6>
                                        <asp:Label ID="lblShirtCount" runat="server" CssClass="stat-value" Text="0"></asp:Label>
                                    </div>
                                </div>
                                <div class="stat-details-footer mt-3 pt-2">
                                    <small class="text-muted"><i class="fas fa-tags mr-1"></i>Active products in catalog</small>
                                </div>
                            </div>
                        </div>

                        <!-- Leagues Added -->
                        <div class="col-xl-3 col-md-6 mb-4">
                            <div class="stat-box-dark card-stat-window" style="border-left: 4px solid #ffc107 !important;">
                                <div class="d-flex align-items-center">
                                    <div class="stat-icon-dark text-warning" style="background-color: rgba(255, 200, 0, 0.1) !important; color: #ffc800 !important;">
                                        <i class="fas fa-flag fa-lg"></i>
                                    </div>
                                    <div class="stat-info-container">
                                        <h6>Leagues Added</h6>
                                        <asp:Label ID="lblLeagues" runat="server" CssClass="stat-value" Text="0"></asp:Label>
                                    </div>
                                </div>
                                <div class="stat-details-footer mt-3 pt-2">
                                    <small class="text-muted"><i class="fas fa-globe mr-1"></i>Football leagues registered</small>
                                </div>
                            </div>
                        </div>

                        <!-- Teams Added -->
                        <div class="col-xl-3 col-md-6 mb-4">
                            <div class="stat-box-dark card-stat-window" style="border-left: 4px solid #17a2b8 !important;">
                                <div class="d-flex align-items-center">
                                    <div class="stat-icon-dark text-info">
                                        <i class="fas fa-shield-alt fa-lg"></i>
                                    </div>
                                    <div class="stat-info-container">
                                        <h6>Teams Added</h6>
                                        <asp:Label ID="lblTeams" runat="server" CssClass="stat-value" Text="0"></asp:Label>
                                    </div>
                                </div>
                                <div class="stat-details-footer mt-3 pt-2">
                                    <small class="text-muted"><i class="fas fa-trophy mr-1"></i>Clubs and national teams</small>
                                </div>
                            </div>
                        </div>

                        <!-- Top Selling -->
                        <div class="col-xl-3 col-md-6 mb-4">
                            <div class="stat-box-dark card-stat-window" style="border-left: 4px solid #28a745 !important;">
                                <div class="d-flex align-items-center">
                                    <div class="stat-icon-dark text-success">
                                        <i class="fas fa-fire fa-lg text-danger"></i>
                                    </div>
                                    <div class="stat-info-container">
                                        <h6>Top Selling</h6>
                                        <asp:Label ID="lblTopShirt" runat="server" CssClass="stat-value" Style="font-size: 1.05rem !important;" Text="None yet"></asp:Label>
                                    </div>
                                </div>
                                <div class="stat-details-footer mt-3 pt-2">
                                    <small class="text-muted"><i class="fas fa-star mr-1"></i>Best selling item by volume</small>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>

                <!-- Permission-Based Tracking Workspaces -->
                <div class="row fade-in mb-5" style="animation-delay: 0.3s;">
                    <!-- Orders Activity Workspace (Perm_Orders) -->
                    <asp:PlaceHolder ID="phRecentOrders" runat="server" Visible="false">
                        <div class="col-xl-6 col-lg-12 mb-4">
                            <div class="form-card p-0 workspace-card" style="overflow: hidden;">
                                <div class="p-3 bg-dark border-bottom border-warning d-flex justify-content-between align-items-center">
                                    <h4 class="m-0 text-white" style="font-weight: 600;"><i class="fas fa-shopping-cart text-warning mr-2"></i>Recent Orders</h4>
                                    <span class="badge badge-primary px-3 py-1 font-weight-bold">Perm_Orders</span>
                                </div>
                                <div class="table-responsive">
                                    <asp:GridView ID="gvRecentOrders" runat="server" AutoGenerateColumns="false" CssClass="table table-custom m-0" GridLines="None" BorderStyle="None">
                                        <Columns>
                                            <asp:BoundField DataField="Id_Order" HeaderText="Order ID" HeaderStyle-CssClass="text-warning font-weight-bold" ItemStyle-CssClass="font-weight-bold text-white" />
                                            <asp:BoundField DataField="Name" HeaderText="First Name" HeaderStyle-CssClass="text-warning font-weight-bold" ItemStyle-CssClass="text-white" />
                                            <asp:BoundField DataField="LastName" HeaderText="Last Name" HeaderStyle-CssClass="text-warning font-weight-bold" ItemStyle-CssClass="text-white" />
                                            <asp:BoundField DataField="City" HeaderText="City" HeaderStyle-CssClass="text-warning font-weight-bold" ItemStyle-CssClass="text-white" />
                                            <asp:BoundField DataField="Total" HeaderText="Total" DataFormatString="{0:C}" HeaderStyle-CssClass="text-warning font-weight-bold" ItemStyle-CssClass="font-weight-bold text-success" />
                                            <asp:TemplateField HeaderText="Status" HeaderStyle-CssClass="text-warning font-weight-bold">
                                                <ItemTemplate>
                                                    <span class='<%# GetStatusBadgeClass(Eval("Status_Name").ToString()) %>'>
                                                        <%# Eval("Status_Name") %>
                                                    </span>
                                                </ItemTemplate>
                                            </asp:TemplateField>
                                        </Columns>
                                        <EmptyDataTemplate>
                                            <div class="text-center text-muted py-5">
                                                <i class="fas fa-shopping-cart fa-2x mb-3 d-block text-warning"></i>
                                                No recent orders found.
                                            </div>
                                        </EmptyDataTemplate>
                                    </asp:GridView>
                                </div>
                            </div>
                        </div>
                    </asp:PlaceHolder>

                    <!-- Inventory Stock Alerts Workspace (Perm_Products) -->
                    <asp:PlaceHolder ID="phCriticalStock" runat="server" Visible="false">
                        <div class="col-xl-6 col-lg-12 mb-4">
                            <div class="form-card p-0 workspace-card" style="overflow: hidden;">
                                <div class="p-3 bg-dark border-bottom border-warning d-flex justify-content-between align-items-center">
                                    <h4 class="m-0 text-white" style="font-weight: 600;"><i class="fas fa-exclamation-triangle text-warning mr-2"></i>Stock Alerts</h4>
                                    <span class="badge badge-danger px-3 py-1 font-weight-bold">Perm_Products</span>
                                </div>
                                <div class="table-responsive">
                                    <asp:GridView ID="gvCriticalStock" runat="server" AutoGenerateColumns="false" CssClass="table table-custom m-0" GridLines="None" BorderStyle="None">
                                        <Columns>
                                            <asp:BoundField DataField="ShirtName" HeaderText="Jersey Name" HeaderStyle-CssClass="text-warning font-weight-bold" ItemStyle-CssClass="text-white" />
                                            <asp:BoundField DataField="SizeName" HeaderText="Size" HeaderStyle-CssClass="text-warning font-weight-bold" ItemStyle-CssClass="font-weight-bold text-white" />
                                            <asp:TemplateField HeaderText="Stock Status" HeaderStyle-CssClass="text-warning font-weight-bold">
                                                <ItemTemplate>
                                                    <span class="badge badge-danger px-3 py-1 font-weight-bold">
                                                        <%# Eval("Stock") %> units left
                                                    </span>
                                                </ItemTemplate>
                                            </asp:TemplateField>
                                        </Columns>
                                        <EmptyDataTemplate>
                                            <div class="text-center text-muted py-5">
                                                <i class="fas fa-check-circle fa-2x mb-3 d-block text-success"></i>
                                                No critical stock alerts. All items are well stocked!
                                            </div>
                                        </EmptyDataTemplate>
                                    </asp:GridView>
                                </div>
                            </div>
                        </div>
                    </asp:PlaceHolder>

                    <!-- Support & Consignments Workspace (Perm_Tickets) -->
                    <asp:PlaceHolder ID="phPendingTickets" runat="server" Visible="false">
                        <div class="col-xl-6 col-lg-12 mb-4">
                            <div class="form-card p-0 workspace-card" style="overflow: hidden;">
                                <div class="p-3 bg-dark border-bottom border-warning d-flex justify-content-between align-items-center">
                                    <h4 class="m-0 text-white" style="font-weight: 600;"><i class="fas fa-ticket-alt text-warning mr-2"></i>Pending Support Tickets</h4>
                                    <span class="badge badge-warning text-dark px-3 py-1 font-weight-bold">Perm_Tickets</span>
                                </div>
                                <div class="table-responsive">
                                    <asp:GridView ID="gvPendingTickets" runat="server" AutoGenerateColumns="false" CssClass="table table-custom m-0" GridLines="None" BorderStyle="None">
                                        <Columns>
                                            <asp:BoundField DataField="Id_Ticket" HeaderText="Ticket ID" HeaderStyle-CssClass="text-warning font-weight-bold" ItemStyle-CssClass="font-weight-bold text-white" />
                                            <asp:BoundField DataField="Reason_Name" HeaderText="Reason" HeaderStyle-CssClass="text-warning font-weight-bold" ItemStyle-CssClass="text-white" />
                                            <asp:BoundField DataField="Subject" HeaderText="Subject" HeaderStyle-CssClass="text-warning font-weight-bold" ItemStyle-CssClass="text-white" />
                                            <asp:BoundField DataField="User_Email" HeaderText="User Email" HeaderStyle-CssClass="text-warning font-weight-bold" ItemStyle-CssClass="text-white" />
                                        </Columns>
                                        <EmptyDataTemplate>
                                            <div class="text-center text-muted py-5">
                                                <i class="fas fa-ticket-alt fa-2x mb-3 d-block text-warning"></i>
                                                No pending support tickets.
                                            </div>
                                        </EmptyDataTemplate>
                                    </asp:GridView>
                                </div>
                            </div>
                        </div>
                    </asp:PlaceHolder>

                    <!-- Live Security Audit Feed Workspace (Owner Level Only) -->
                    <asp:PlaceHolder ID="phAuditLogs" runat="server" Visible="false">
                        <div class="col-xl-6 col-lg-12 mb-4">
                            <div class="form-card p-0 workspace-card" style="overflow: hidden;">
                                <div class="p-3 bg-dark border-bottom border-warning d-flex justify-content-between align-items-center">
                                    <h4 class="m-0 text-white" style="font-weight: 600;"><i class="fas fa-shield-alt text-warning mr-2"></i>Live Security Audit Feed</h4>
                                    <span class="badge badge-secondary px-3 py-1 font-weight-bold">Owner Only</span>
                                </div>
                                <div class="table-responsive">
                                    <asp:GridView ID="gvAuditLogs" runat="server" AutoGenerateColumns="false" CssClass="table table-custom m-0" GridLines="None" BorderStyle="None">
                                        <Columns>
                                            <asp:BoundField DataField="Created_At" HeaderText="Date/Time" DataFormatString="{0:MM/dd/yyyy hh:mm tt}" HeaderStyle-CssClass="text-warning font-weight-bold" ItemStyle-CssClass="text-white" />
                                            <asp:BoundField DataField="Operator" HeaderText="Operator" HeaderStyle-CssClass="text-warning font-weight-bold" ItemStyle-CssClass="font-weight-bold text-white" />
                                            <asp:BoundField DataField="Module" HeaderText="Module" HeaderStyle-CssClass="text-warning font-weight-bold" ItemStyle-CssClass="text-white" />
                                            <asp:BoundField DataField="Description" HeaderText="Description" HeaderStyle-CssClass="text-warning font-weight-bold" ItemStyle-CssClass="text-white" />
                                        </Columns>
                                        <EmptyDataTemplate>
                                            <div class="text-center text-muted py-5">
                                                <i class="fas fa-shield-alt fa-2x mb-3 d-block text-secondary"></i>
                                                No security audit logs found.
                                            </div>
                                        </EmptyDataTemplate>
                                    </asp:GridView>
                                </div>
                            </div>
                        </div>
                    </asp:PlaceHolder>
                </div>

                <div class="carousel-wrapper fade-in" style="animation-delay: 0.4s;">
                    <h3 class="text-white mb-4" style="font-weight: 600;">Banner Preview (Live)</h3>

                    <asp:PlaceHolder ID="phDashCarousel" runat="server">
                        <div id="demoDash" class="carousel slide" data-ride="carousel">

                            <%-- Indicadores dinámicos --%>
                            <ul class="carousel-indicators">
                                <asp:Repeater ID="rptDashIndicators" runat="server">
                                    <ItemTemplate>
                                        <li data-target="#demoDash"
                                            data-slide-to="<%# Container.ItemIndex %>"
                                            class='<%# Container.ItemIndex == 0 ? "active" : "" %>'></li>
                                    </ItemTemplate>
                                </asp:Repeater>
                            </ul>

                            <%-- Items dinámicos --%>
                            <div class="carousel-inner">
                                <asp:Repeater ID="rptDashBanners" runat="server">
                                    <ItemTemplate>
                                        <div class='<%# "carousel-item" + (Container.ItemIndex == 0 ? " active" : "") %>'>
                                            <%# BuildDashBannerImage(
                                                    Eval("ImageURL").ToString(),
                                                    Eval("Title").ToString(),
                                                    Eval("LinkURL") == DBNull.Value ? "" : Eval("LinkURL").ToString()
                                                ) %>
                                        </div>
                                    </ItemTemplate>
                                </asp:Repeater>
                            </div>

                            <a class="carousel-control-prev" href="#demoDash" data-slide="prev">
                                <span class="carousel-control-prev-icon"></span>
                            </a>
                            <a class="carousel-control-next" href="#demoDash" data-slide="next">
                                <span class="carousel-control-next-icon"></span>
                            </a>
                        </div>
                    </asp:PlaceHolder>

                    <asp:PlaceHolder ID="phDashNoBanners" runat="server" Visible="false">
                        <p class="text-muted" style="padding: 40px; text-align: center; border: 1px dashed var(--border-color); border-radius: 8px;">
                            No active banners found. <a href="AdminBanners.aspx" style="color: var(--accent-color);">Manage Banners</a>
                        </p>
                    </asp:PlaceHolder>
                </div>

                <asp:Label ID="label1" runat="server" Text="" Visible="false"></asp:Label>
                <asp:Label ID="label2" runat="server" Text="" Visible="false"></asp:Label>
            </main>
        </div>
    </form>

    <script src="https://ajax.googleapis.com/ajax/libs/jquery/3.4.1/jquery.min.js"></script>
    <script src="https://cdnjs.cloudflare.com/ajax/libs/popper.js/1.14.7/umd/popper.min.js"></script>
    <script src="https://maxcdn.bootstrapcdn.com/bootstrap/4.3.1/js/bootstrap.min.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@8"></script>

    <script type="text/javascript" src="https://cdnjs.cloudflare.com/ajax/libs/slick-carousel/1.8.1/slick.min.js"></script>

    <!-- Slick Slider script removed since stats now use a responsive grid -->

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

