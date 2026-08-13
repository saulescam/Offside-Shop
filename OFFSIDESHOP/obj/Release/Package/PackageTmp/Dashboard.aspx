<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Dashboard.aspx.cs" Inherits="OFFSIDESHOP.Dashboard" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1, shrink-to-fit=no" />
    <title><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Dash_Title %>" /> | OffsideShop</title>

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
            height: 350px !important;
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

        /* Estilo del botón de cambio de idioma en blanco */
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
            if (event.persisted) {
                window.location.reload();
            }
        };
    </script>
</head>
<body>
    <form id="form1" runat="server">
        <nav class="top-navbar">
            <div style="display: flex; align-items: center; gap: 20px;">
                <a class="navbar-brand" href="Homepage.aspx" style="margin-right: 0;">
                    <img src="assets/img/offsideshop_logo_white_letras.png" alt="OFFSIDESHOP" />
                </a>
                <!-- Botón de cambio de idioma estilo texto blanco -->
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

            <main class="main-content fade-in" style="animation-delay: 0.2s;">

                <div class="mb-4">
                    <h1 class="page-title mb-1"><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Dash_Welcome %>" /> <asp:Label ID="lblAdminName" runat="server"></asp:Label></h1>
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
                                        <h6><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Dash_RegUsers %>" /></h6>
                                        <asp:Label ID="lblUserCount" runat="server" CssClass="stat-value" Text="0"></asp:Label>
                                    </div>
                                </div>
                                <div class="stat-details-footer mt-3 pt-2">
                                    <small class="text-muted"><i class="fas fa-info-circle mr-1"></i><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Dash_RegUsersSub %>" /></small>
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
                                        <h6><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Dash_Orders7Days %>" /></h6>
                                        <asp:Label ID="lblPurchasesLast7Days" runat="server" CssClass="stat-value" Text="0"></asp:Label>
                                    </div>
                                </div>
                                <div class="stat-details-footer mt-3 pt-2">
                                    <small class="text-muted"><i class="fas fa-chart-line mr-1"></i><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Dash_Orders7DaysSub %>" /></small>
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
                                        <h6><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Dash_PendingOrders %>" /></h6>
                                        <asp:Label ID="lblPendingOrders" runat="server" CssClass="stat-value text-warning" Text="0"></asp:Label>
                                    </div>
                                </div>
                                <div class="stat-details-footer mt-3 pt-2">
                                    <small class="text-muted"><i class="fas fa-clock mr-1"></i><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Dash_PendingOrdersSub %>" /></small>
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
                                        <h6><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Dash_TotalProcessed %>" /></h6>
                                        <asp:Label ID="lblTotalOrders" runat="server" CssClass="stat-value" Text="0"></asp:Label>
                                    </div>
                                </div>
                                <div class="stat-details-footer mt-3 pt-2">
                                    <small class="text-muted"><i class="fas fa-history mr-1"></i><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Dash_TotalProcessedSub %>" /></small>
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
                                        <h6><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Dash_JerseysAdded %>" /></h6>
                                        <asp:Label ID="lblShirtCount" runat="server" CssClass="stat-value" Text="0"></asp:Label>
                                    </div>
                                </div>
                                <div class="stat-details-footer mt-3 pt-2">
                                    <small class="text-muted"><i class="fas fa-tags mr-1"></i><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Dash_JerseysAddedSub %>" /></small>
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
                                        <h6><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Dash_LeaguesAdded %>" /></h6>
                                        <asp:Label ID="lblLeagues" runat="server" CssClass="stat-value" Text="0"></asp:Label>
                                    </div>
                                </div>
                                <div class="stat-details-footer mt-3 pt-2">
                                    <small class="text-muted"><i class="fas fa-globe mr-1"></i><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Dash_LeaguesAddedSub %>" /></small>
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
                                        <h6><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Dash_TeamsAdded %>" /></h6>
                                        <asp:Label ID="lblTeams" runat="server" CssClass="stat-value" Text="0"></asp:Label>
                                    </div>
                                </div>
                                <div class="stat-details-footer mt-3 pt-2">
                                    <small class="text-muted"><i class="fas fa-trophy mr-1"></i><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Dash_TeamsAddedSub %>" /></small>
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
                                        <h6><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Dash_TopSelling %>" /></h6>
                                        <asp:Label ID="lblTopShirt" runat="server" CssClass="stat-value" Style="font-size: 1.05rem !important;" Text="None yet"></asp:Label>
                                    </div>
                                </div>
                                <div class="stat-details-footer mt-3 pt-2">
                                    <small class="text-muted"><i class="fas fa-star mr-1"></i><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Dash_TopSellingSub %>" /></small>
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
                                    <h4 class="m-0 text-white" style="font-weight: 600;"><i class="fas fa-shopping-cart text-warning mr-2"></i><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Dash_RecentOrders %>" /></h4>
                                    <span class="badge badge-primary px-3 py-1 font-weight-bold">Perm_Orders</span>
                                </div>
                                <div class="table-responsive">
                                    <asp:GridView ID="gvRecentOrders" runat="server" AutoGenerateColumns="false" CssClass="table table-custom m-0" GridLines="None" BorderStyle="None">
                                        <Columns>
                                            <asp:BoundField DataField="Id_Order" HeaderText="<%$ Resources:Strings, Admin_Orders_HeaderOrderId %>" HeaderStyle-CssClass="text-warning font-weight-bold" ItemStyle-CssClass="font-weight-bold text-white" />
                                            <asp:BoundField DataField="Name" HeaderText="<%$ Resources:Strings, Admin_Orders_HeaderFirstName %>" HeaderStyle-CssClass="text-warning font-weight-bold" ItemStyle-CssClass="text-white" />
                                            <asp:BoundField DataField="LastName" HeaderText="<%$ Resources:Strings, Admin_Orders_HeaderLastName %>" HeaderStyle-CssClass="text-warning font-weight-bold" ItemStyle-CssClass="text-white" />
                                            <asp:BoundField DataField="City" HeaderText="<%$ Resources:Strings, Admin_Orders_HeaderCity %>" HeaderStyle-CssClass="text-warning font-weight-bold" ItemStyle-CssClass="text-white" />
                                            <asp:BoundField DataField="Total" HeaderText="<%$ Resources:Strings, Admin_Orders_HeaderTotal %>" DataFormatString="{0:C}" HeaderStyle-CssClass="text-warning font-weight-bold" ItemStyle-CssClass="font-weight-bold text-success" />
                                            <asp:TemplateField HeaderText="<%$ Resources:Strings, Admin_Orders_HeaderStatus %>" HeaderStyle-CssClass="text-warning font-weight-bold">
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
                                                <asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Dash_NoRecentOrders %>" />
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
                                    <h4 class="m-0 text-white" style="font-weight: 600;"><i class="fas fa-exclamation-triangle text-warning mr-2"></i><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Dash_StockAlerts %>" /></h4>
                                    <span class="badge badge-danger px-3 py-1 font-weight-bold">Perm_Products</span>
                                </div>
                                <div class="table-responsive">
                                    <asp:GridView ID="gvCriticalStock" runat="server" AutoGenerateColumns="false" CssClass="table table-custom m-0" GridLines="None" BorderStyle="None">
                                        <Columns>
                                            <asp:BoundField DataField="ShirtName" HeaderText="<%$ Resources:Strings, Admin_Dash_HeaderShirtName %>" HeaderStyle-CssClass="text-warning font-weight-bold" ItemStyle-CssClass="text-white" />
                                            <asp:BoundField DataField="SizeName" HeaderText="<%$ Resources:Strings, Admin_Dash_HeaderSize %>" HeaderStyle-CssClass="text-warning font-weight-bold" ItemStyle-CssClass="font-weight-bold text-white" />
                                            <asp:TemplateField HeaderText="<%$ Resources:Strings, Admin_Dash_HeaderStockStatus %>" HeaderStyle-CssClass="text-warning font-weight-bold">
                                                <ItemTemplate>
                                                    <span class="badge badge-danger px-3 py-1 font-weight-bold">
                                                        <%# Eval("Stock") %> <asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Dash_UnitsLeft %>" />
                                                    </span>
                                                </ItemTemplate>
                                            </asp:TemplateField>
                                        </Columns>
                                        <EmptyDataTemplate>
                                            <div class="text-center text-muted py-5">
                                                <i class="fas fa-check-circle fa-2x mb-3 d-block text-success"></i>
                                                <asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Dash_NoStockAlerts %>" />
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
                                    <h4 class="m-0 text-white" style="font-weight: 600;"><i class="fas fa-ticket-alt text-warning mr-2"></i><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Dash_PendingTickets %>" /></h4>
                                    <span class="badge badge-warning text-dark px-3 py-1 font-weight-bold">Perm_Tickets</span>
                                </div>
                                <div class="table-responsive">
                                    <asp:GridView ID="gvPendingTickets" runat="server" AutoGenerateColumns="false" CssClass="table table-custom m-0" GridLines="None" BorderStyle="None">
                                        <Columns>
                                            <asp:BoundField DataField="Id_Ticket" HeaderText="<%$ Resources:Strings, Admin_Dash_HeaderTicketId %>" HeaderStyle-CssClass="text-warning font-weight-bold" ItemStyle-CssClass="font-weight-bold text-white" />
                                            <asp:BoundField DataField="Reason_Name" HeaderText="<%$ Resources:Strings, Admin_Dash_HeaderReason %>" HeaderStyle-CssClass="text-warning font-weight-bold" ItemStyle-CssClass="text-white" />
                                            <asp:BoundField DataField="Subject" HeaderText="<%$ Resources:Strings, Admin_Dash_HeaderSubject %>" HeaderStyle-CssClass="text-warning font-weight-bold" ItemStyle-CssClass="text-white" />
                                            <asp:BoundField DataField="User_Email" HeaderText="<%$ Resources:Strings, Admin_Dash_HeaderUserEmail %>" HeaderStyle-CssClass="text-warning font-weight-bold" ItemStyle-CssClass="text-white" />
                                        </Columns>
                                        <EmptyDataTemplate>
                                            <div class="text-center text-muted py-5">
                                                <i class="fas fa-ticket-alt fa-2x mb-3 d-block text-warning"></i>
                                                <asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Dash_NoPendingTickets %>" />
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
                                    <h4 class="m-0 text-white" style="font-weight: 600;"><i class="fas fa-shield-alt text-warning mr-2"></i><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Dash_AuditFeed %>" /></h4>
                                    <span class="badge badge-secondary px-3 py-1 font-weight-bold">Owner Only</span>
                                </div>
                                <div class="table-responsive">
                                    <asp:GridView ID="gvAuditLogs" runat="server" AutoGenerateColumns="false" CssClass="table table-custom m-0" GridLines="None" BorderStyle="None">
                                        <Columns>
                                            <asp:BoundField DataField="Created_At" HeaderText="<%$ Resources:Strings, Admin_Dash_HeaderDateTime %>" DataFormatString="{0:MM/dd/yyyy hh:mm tt}" HeaderStyle-CssClass="text-warning font-weight-bold" ItemStyle-CssClass="text-white" />
                                            <asp:BoundField DataField="Operator" HeaderText="<%$ Resources:Strings, Admin_Dash_HeaderOperator %>" HeaderStyle-CssClass="text-warning font-weight-bold" ItemStyle-CssClass="font-weight-bold text-white" />
                                            <asp:BoundField DataField="Module" HeaderText="<%$ Resources:Strings, Admin_Dash_HeaderModule %>" HeaderStyle-CssClass="text-warning font-weight-bold" ItemStyle-CssClass="text-white" />
                                            <asp:BoundField DataField="Description" HeaderText="<%$ Resources:Strings, Admin_Dash_HeaderDescription %>" HeaderStyle-CssClass="text-warning font-weight-bold" ItemStyle-CssClass="text-white" />
                                        </Columns>
                                        <EmptyDataTemplate>
                                            <div class="text-center text-muted py-5">
                                                <i class="fas fa-shield-alt fa-2x mb-3 d-block text-secondary"></i>
                                                <asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Dash_NoAuditLogs %>" />
                                            </div>
                                        </EmptyDataTemplate>
                                    </asp:GridView>
                                </div>
                            </div>
                        </div>
                    </asp:PlaceHolder>
                </div>

                <div class="carousel-wrapper fade-in" style="animation-delay: 0.4s;">
                    <h3 class="text-white mb-4" style="font-weight: 600;"><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Dash_BannerPreview %>" /></h3>

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
                            <asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Dash_NoActiveBanners %>" /> <a href="AdminBanners.aspx" style="color: var(--accent-color);"><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Nav_ManageBanners %>" /></a>
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