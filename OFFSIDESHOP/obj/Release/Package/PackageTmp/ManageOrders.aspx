<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ManageOrders.aspx.cs" Inherits="OFFSIDESHOP.ManageOrders" Async="true" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1, shrink-to-fit=no" />
    <title>Manage Orders & Refunds | OffsideShop</title>

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

    <style>
        .filter-card { background: var(--card-bg); border: 1px solid var(--border-color); border-radius: 14px; padding: 20px 24px; margin-bottom: 28px; box-shadow: 0 6px 20px rgba(0,0,0,0.15); }
        .filter-card label { color: var(--text-muted); font-weight: 600; font-size: 0.8rem; text-transform: uppercase; letter-spacing: 0.8px; margin-bottom: 6px; }
        .nav-tabs-custom { border-bottom: 2px solid var(--border-color); margin-bottom: 25px; }
        .nav-tabs-custom .nav-link { border: none; color: var(--text-muted); font-weight: 600; padding: 12px 20px; background: transparent; transition: all 0.3s ease; }
        .nav-tabs-custom .nav-link.active { color: #FFC800 !important; border-bottom: 3px solid #FFC800; background: transparent; transition: all 0.3s ease; }
        .badge-refund { font-size: 0.75rem; padding: 4px 8px; border-radius: 20px; margin-left: 6px; }
        
        /* BLOQUEO Y CENTRADO ABSOLUTO DE LOS MODALES */
        .modal-backdrop-custom {
            position: fixed !important;
            top: 0 !important;
            left: 0 !important;
            width: 100vw !important;
            height: 100vh !important;
            background-color: rgba(0,0,0,0.7) !important;
            z-index: 99998 !important;
            display: flex !important;
            align-items: center !important;
            justify-content: center !important;
        }

        .modal-dialog-custom {
            position: relative !important;
            background: var(--card-bg, #ffffff);
            color: var(--text-main, #111111);
            border: 1px solid var(--border-color, #e0e0e0);
            border-radius: 12px;
            width: 95% !important;
            max-width: 600px;
            max-height: 85vh !important;
            box-shadow: 0 10px 40px rgba(0,0,0,0.5);
            z-index: 99999 !important;
            display: flex;
            flex-direction: column;
            animation: modalPop 0.3s ease-out forwards;
        }

        .modal-dialog-custom .modal-body {
            overflow-y: auto !important;
            padding: 20px;
        }

        .modal-dialog-large { max-width: 750px !important; }

        @keyframes modalPop {
            from { transform: scale(0.9); opacity: 0; }
            to { transform: scale(1); opacity: 1; }
        }

        /* ESTILOS Y TEMA DE MODALES (MODO CLARO Y OSCURO) */
        .modal-section-bg {
            background-color: var(--bg-input-disabled, #f8f9fa);
            border: 1px solid var(--border-color, #e0e0e0);
            transition: background-color 0.3s ease, border-color 0.3s ease;
        }

        .modal-dialog-custom .modal-header {
            border-bottom: 1px solid var(--border-color, #e0e0e0);
        }

        .modal-dialog-custom .modal-footer {
            background-color: var(--bg-input-disabled, #f8f9fa);
            border-top: 1px solid var(--border-color, #e0e0e0);
        }

        body.dark-mode .modal-dialog-custom {
            background: var(--bg-card, #1a1a1a) !important;
            color: var(--text-main, #ffffff) !important;
            border-color: var(--border-color, #2c2c2c) !important;
        }

        body.dark-mode .modal-dialog-custom .modal-header {
            background-color: #0d0d0d !important;
            border-bottom-color: var(--border-color, #2c2c2c) !important;
        }

        body.dark-mode .modal-dialog-custom .modal-footer {
            background-color: #161616 !important;
            border-top-color: var(--border-color, #2c2c2c) !important;
        }

        body.dark-mode .modal-section-bg {
            background-color: #222222 !important;
            border-color: #333333 !important;
        }

        body.dark-mode .modal-dialog-custom .bg-light {
            background-color: #222222 !important;
            border-color: #333333 !important;
        }

        body.dark-mode .modal-dialog-custom .text-dark {
            color: var(--text-main, #ffffff) !important;
        }

        body.dark-mode .modal-dialog-custom .text-secondary {
            color: #aaaaaa !important;
        }

        body.dark-mode .modal-dialog-custom .text-muted {
            color: #888888 !important;
        }

        /* Modal GridView Table Dark Mode */
        .modal-dialog-custom .table {
            color: var(--text-main);
            background-color: var(--bg-card);
        }

        .modal-dialog-custom .table th {
            background-color: var(--table-header-bg, #000000) !important;
            color: var(--table-header-text, #FFC800) !important;
            border-color: var(--border-color) !important;
        }

        .modal-dialog-custom .table td {
            color: var(--text-main) !important;
            border-color: var(--border-color) !important;
            background-color: transparent !important;
        }

        body.dark-mode .modal-dialog-custom .table-striped tbody tr:nth-of-type(odd) {
            background-color: rgba(255, 255, 255, 0.05) !important;
        }

        body.dark-mode .modal-dialog-custom .table-striped tbody tr:nth-of-type(even) {
            background-color: transparent !important;
        }

        body.dark-mode .modal-dialog-custom .table-bordered {
            border: 1px solid var(--border-color, #2c2c2c) !important;
        }

        body.dark-mode .modal-dialog-custom .table-bordered td,
        body.dark-mode .modal-dialog-custom .table-bordered th {
            border: 1px solid var(--border-color, #2c2c2c) !important;
        }

        /* Botón de Detalles anti-encogimiento */
        .btn-details-action {
            background-color: #17a2b8 !important;
            border: 1px solid #17a2b8 !important;
            color: #ffffff !important;
            font-weight: 600 !important;
            font-size: 0.85rem !important;
            padding: 6px 14px !important;
            border-radius: 6px !important;
            white-space: nowrap !important;
            flex-shrink: 0 !important;
            display: inline-flex !important;
            align-items: center !important;
            gap: 6px !important;
            text-decoration: none !important;
            transition: all 0.2s ease-in-out !important;
            height: 34px !important;
            line-height: 1 !important;
            box-sizing: border-box !important;
        }

        .btn-details-action:hover {
            background-color: #138496 !important;
            border-color: #117a8b !important;
            color: #ffffff !important;
            transform: translateY(-1px) !important;
            box-shadow: 0 4px 10px rgba(23, 162, 184, 0.3) !important;
            text-decoration: none !important;
        }

        .btn-details-action:active,
        .btn-details-action:focus {
            background-color: #117a8b !important;
            border-color: #10707f !important;
            color: #ffffff !important;
            transform: none !important;
            box-shadow: none !important;
            outline: none !important;
            text-decoration: none !important;
        }

        /* Estilos Paginación del GridView */
        .grid-pager table { margin: 15px auto 0 auto; }
        .grid-pager table td { padding: 0 5px; }
        .grid-pager a, .grid-pager span { display: inline-block; padding: 8px 14px; border-radius: 4px; font-weight: bold; background: #333; color: white; text-decoration: none; }
        .grid-pager a:hover { background: #FFC800; color: black; }
        .grid-pager span { background: #FFC800; color: black; border: 2px solid #FFC800; }

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

        /* Checkbox Dorado */
        .gold-checkbox {
            position: relative;
            display: flex;
            align-items: center;
            cursor: pointer;
            user-select: none;
        }
        .gold-checkbox input[type="checkbox"] { position: absolute; opacity: 0; cursor: pointer; height: 0; width: 0; }
        .checkmark {
            height: 18px;
            width: 18px;
            background-color: transparent;
            border: 2px solid #6c757d;
            border-radius: 4px;
            margin-right: 8px;
            display: inline-block;
            position: relative;
            transition: all 0.2s ease;
        }
        .gold-checkbox:hover input ~ .checkmark { border-color: #d4af37; }
        .gold-checkbox input:checked ~ .checkmark { background-color: #d4af37; border-color: #d4af37; }
        .checkmark:after {
            content: ""; position: absolute; display: none; left: 5px; top: 1px;
            width: 5px; height: 10px; border: solid white; border-width: 0 2px 2px 0; transform: rotate(45deg);
        }
        .gold-checkbox input:checked ~ .checkmark:after { display: block; }
    </style>
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
                        <a id="btnManageOrders" runat="server" href="ManageOrders.aspx" class="sidebar-btn active" style="font-family: 'Raleway','Font Awesome 5 Free'; font-weight: 600;">
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

            <!-- MAIN CONTENT -->
            <main class="main-content fade-in" style="animation-delay: 0.15s;">
                <div class="container-fluid">
                    <h1 class="page-title"><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Orders_Title %>" /></h1>
                    <p class="text-muted mb-4"><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Orders_Subtitle %>" /></p>

                    <asp:UpdatePanel ID="upMainOrders" runat="server">
                        <ContentTemplate>
                            <div class="nav-tabs-custom d-flex">
                                <asp:LinkButton ID="btnTabOrders" runat="server" CssClass="nav-link active" OnClick="btnTabOrders_Click"></asp:LinkButton>
                                <asp:LinkButton ID="btnTabRefunds" runat="server" CssClass="nav-link" OnClick="btnTabRefunds_Click"></asp:LinkButton>
                            </div>

                            <asp:PlaceHolder ID="phOrdersView" runat="server" Visible="true">
                                <div class="filter-card">
                                    <div class="row align-items-end">
                                        <div class="col-md-3 col-sm-12 mb-2 mb-md-0">
                                            <label for="ddlFilterStatus"><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Orders_FilterStatus %>" /></label>
                                            <asp:DropDownList ID="ddlFilterStatus" runat="server" CssClass="form-control"></asp:DropDownList>
                                        </div>
                                        <div class="col-md-3 col-sm-12 mb-2 mb-md-0">
                                            <label for="txtStartDate"><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Orders_FilterStartDate %>" /></label>
                                            <asp:TextBox ID="txtStartDate" runat="server" CssClass="form-control" TextMode="Date"></asp:TextBox>
                                        </div>
                                        <div class="col-md-3 col-sm-12 mb-2 mb-md-0">
                                            <label for="txtEndDate"><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Orders_FilterEndDate %>" /></label>
                                            <asp:TextBox ID="txtEndDate" runat="server" CssClass="form-control" TextMode="Date"></asp:TextBox>
                                        </div>
                                        <div class="col-md-3 col-sm-12">
                                            <asp:LinkButton ID="btnApplyFilters" runat="server" CssClass="btn btn-warning font-weight-bold w-100" OnClick="btnApplyFilters_Click">
                                                <i class="fas fa-filter"></i> <asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Orders_ApplyFilters %>" />
                                            </asp:LinkButton>
                                        </div>
                                    </div>
                                </div>

                                <div class="table-responsive">
                                    <asp:GridView ID="gvOrders" runat="server"
                                        AutoGenerateColumns="False"
                                        GridLines="None"
                                        CssClass="table table-custom text-center align-middle"
                                        DataKeyNames="Id_Order"
                                        AllowPaging="True" PageSize="10" 
                                        OnPageIndexChanging="gvOrders_PageIndexChanging"
                                        OnRowDataBound="gvOrders_RowDataBound"
                                        EmptyDataText="<%$ Resources:Strings, Admin_Orders_EmptyOrders %>">
                                        
                                        <PagerStyle CssClass="grid-pager" HorizontalAlign="Center" />

                                        <Columns>
                                            <asp:BoundField DataField="Id_Order" HeaderText="<%$ Resources:Strings, Admin_Orders_ColOrderId %>" ItemStyle-Width="90px" />
                                            <asp:BoundField DataField="CustomerName" HeaderText="<%$ Resources:Strings, Admin_Orders_ColCustomer %>" ItemStyle-HorizontalAlign="Left" />
                                            <asp:BoundField DataField="Mail" HeaderText="<%$ Resources:Strings, Admin_Orders_ColEmail %>" />
                                            <asp:BoundField DataField="OrderDate" HeaderText="<%$ Resources:Strings, Admin_Orders_ColDate %>" DataFormatString="{0:yyyy-MM-dd HH:mm}" />
                                            <asp:BoundField DataField="City_Name" HeaderText="<%$ Resources:Strings, Admin_Orders_ColDepartment %>" />
                                            <asp:BoundField DataField="Total" HeaderText="<%$ Resources:Strings, Admin_Orders_ColTotal %>" DataFormatString="${0:N2}" HtmlEncode="false" />
                                            <asp:BoundField DataField="Status_Name" HeaderText="<%$ Resources:Strings, Admin_Orders_ColStatus %>" />
                                            <asp:TemplateField HeaderText="<%$ Resources:Strings, Admin_Orders_ColActions %>" ItemStyle-Width="260px">
                                                <ItemTemplate>
                                                    <div class="d-flex align-items-center justify-content-center" style="gap: 8px;">
                                                        <asp:LinkButton ID="lnkViewDetails" runat="server" CssClass="btn-details-action"
                                                            OnClick="lnkViewDetails_Click" CommandArgument='<%# Eval("Id_Order") %>'>
                                                            <i class="fas fa-eye"></i> <asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Orders_BtnDetails %>" />
                                                        </asp:LinkButton>
                                                        <asp:DropDownList ID="ddlGridStatus" runat="server" AutoPostBack="true"
                                                            OnSelectedIndexChanged="ddlGridStatus_SelectedIndexChanged"
                                                            CssClass="form-control form-control-sm" style="max-width: 140px; height: 34px; flex-shrink: 0;">
                                                        </asp:DropDownList>
                                                    </div>
                                                </ItemTemplate>
                                            </asp:TemplateField>
                                        </Columns>
                                    </asp:GridView>
                                </div>
                            </asp:PlaceHolder>

                            <asp:PlaceHolder ID="phRefundsView" runat="server" Visible="false">
                                <div class="table-responsive">
                                    <asp:GridView ID="gvRefunds" runat="server"
                                        AutoGenerateColumns="False" GridLines="None"
                                        CssClass="table table-custom text-center align-middle"
                                        EmptyDataText="<%$ Resources:Strings, Admin_Orders_EmptyRefunds %>">
                                        <Columns>
                                            <asp:BoundField DataField="Id_Order" HeaderText="<%$ Resources:Strings, Admin_Orders_ColOrderId %>" ItemStyle-Width="100px" />
                                            <asp:BoundField DataField="CustomerName" HeaderText="<%$ Resources:Strings, Admin_Orders_ColClient %>" ItemStyle-HorizontalAlign="Left" />
                                            <asp:BoundField DataField="Mail" HeaderText="<%$ Resources:Strings, Admin_Orders_ColEmail %>" />
                                            <asp:BoundField DataField="Reason_Title" HeaderText="<%$ Resources:Strings, Admin_Orders_ColReasonConcept %>" ItemStyle-HorizontalAlign="Left" />
                                            <asp:BoundField DataField="Total" HeaderText="<%$ Resources:Strings, Admin_Orders_ColAmountRefund %>" DataFormatString="${0:N2}" HtmlEncode="false" />
                                            <asp:BoundField DataField="Method_Name" HeaderText="<%$ Resources:Strings, Admin_Orders_ColPaymentChannel %>" />
                                            <asp:BoundField DataField="Created_At" HeaderText="<%$ Resources:Strings, Admin_Orders_ColRequestedAt %>" DataFormatString="{0:yyyy-MM-dd HH:mm}" />
                                            <asp:TemplateField HeaderText="<%$ Resources:Strings, Admin_Orders_ColEvaluation %>">
                                                <ItemTemplate>
                                                    <asp:LinkButton ID="lnkEvaluateRefund" runat="server"
                                                        CssClass="btn btn-sm btn-warning font-weight-bold px-3" Style="white-space: nowrap; flex-shrink: 0;"
                                                        OnClick="lnkEvaluateRefund_Click" CommandArgument='<%# Eval("Id_Order") %>'>
                                                        <i class="fas fa-search-dollar mr-1"></i> <asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Orders_BtnEvaluate %>" />
                                                    </asp:LinkButton>
                                                </ItemTemplate>
                                            </asp:TemplateField>
                                        </Columns>
                                    </asp:GridView>
                                </div>
                            </asp:PlaceHolder>

                            <!-- MODAL DE DETALLES DE ORDEN -->
                            <asp:PlaceHolder ID="phOrderDetailsModal" runat="server" Visible="false">
                                <div class="modal-backdrop-custom">
                                    <div class="modal-dialog-custom modal-dialog-large">
                                        <div class="modal-header bg-dark text-white px-4 py-3 d-flex justify-content-between align-items-center">
                                            <h5 class="modal-title font-weight-bold" style="color: #FFC800;">
                                                <i class="fas fa-shopping-bag mr-2"></i><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Orders_ModalBreakdown %>" /> #<asp:Literal ID="litDetOrderId" runat="server" />
                                            </h5>
                                            <asp:LinkButton ID="lnkCloseDetX" runat="server" OnClick="btnCloseOrderDetails_Click" CssClass="text-white text-decoration-none" Style="font-size: 1.3rem;">&times;</asp:LinkButton>
                                        </div>
                                        <div class="modal-body">
                                            <div class="row mb-3 border-bottom pb-3">
                                                <div class="col-md-6">
                                                    <small class="text-muted d-block text-uppercase font-weight-bold"><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Orders_ModalCustomerInfo %>" /></small>
                                                    <span class="font-weight-bold d-block"><asp:Literal ID="litDetCustomer" runat="server" /></span>
                                                    <small class="text-secondary"><asp:Literal ID="litDetEmail" runat="server" /> | <asp:Literal ID="litDetPhone" runat="server" /></small>
                                                </div>
                                                <div class="col-md-6 text-md-right">
                                                    <small class="text-muted d-block text-uppercase font-weight-bold"><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Orders_ModalLogisticsAddr %>" /></small>
                                                    <span class="d-block font-weight-bold" style="font-size: 0.9rem;"><asp:Literal ID="litDetAddress" runat="server" /></span>
                                                    <small class="text-muted"><asp:Literal ID="litDetLocation" runat="server" /></small>
                                                </div>
                                            </div>

                                            <div class="mb-3">
                                                <h6 class="font-weight-bold text-uppercase small text-muted mb-2"><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Orders_ModalLineItems %>" /></h6>
                                                <asp:GridView ID="gvOrderDetailItems" runat="server" AutoGenerateColumns="false" CssClass="table table-sm table-striped table-bordered text-center small" GridLines="None">
                                                    <Columns>
                                                        <asp:BoundField DataField="ProductName" HeaderText="Jersey Name" ItemStyle-HorizontalAlign="Left" HeaderStyle-CssClass="bg-dark text-white" />
                                                        <asp:BoundField DataField="Size" HeaderText="Size" ItemStyle-Width="60px" HeaderStyle-CssClass="bg-dark text-white" />
                                                        <asp:BoundField DataField="Price" HeaderText="Price" DataFormatString="${0:N2}" HtmlEncode="false" ItemStyle-Width="80px" HeaderStyle-CssClass="bg-dark text-white" />
                                                        <asp:BoundField DataField="Quantity" HeaderText="Qty" ItemStyle-Width="50px" HeaderStyle-CssClass="bg-dark text-white" />
                                                        <asp:TemplateField HeaderText="<%$ Resources:Strings, Admin_Orders_ModalPrints %>" HeaderStyle-CssClass="bg-dark text-white">
                                                            <ItemTemplate>
                                                                <%# (Eval("CustomName") != DBNull.Value && !string.IsNullOrEmpty(Eval("CustomName").ToString())) ? 
                                                                    "<span class='badge bg-warning text-dark font-weight-bold'>" + Eval("CustomName") + " #" + Eval("CustomNumber") + "</span>" : 
                                                                    "<span class='text-muted'>None</span>" %>
                                                            </ItemTemplate>
                                                        </asp:TemplateField>
                                                        <asp:BoundField DataField="Subtotal" HeaderText="Subtotal" DataFormatString="${0:N2}" HtmlEncode="false" ItemStyle-Width="90px" HeaderStyle-CssClass="bg-dark text-white" />
                                                    </Columns>
                                                </asp:GridView>
                                            </div>

                                            <div class="row align-items-center modal-section-bg p-3 rounded">
                                                <div class="col-md-7">
                                                    <small class="text-muted d-block text-uppercase font-weight-bold"><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Orders_ModalNotes %>" /></small>
                                                    <p class="mb-0 small text-secondary italic" style="font-style: italic;"><asp:Literal ID="litDetNotes" runat="server" /></p>
                                                </div>
                                                <div class="col-md-5 text-right">
                                                    <small class="text-muted d-block text-uppercase font-weight-bold"><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Orders_ModalFinSummary %>" /></small>
                                                    <small class="text-muted d-block">Shipping Cost: <asp:Literal ID="litDetShipping" runat="server" /></small>
                                                    <span class="font-weight-bold text-success" style="font-size: 1.3rem;">Total: <asp:Literal ID="litDetTotal" runat="server" /></span>
                                                </div>
                                            </div>
                                        </div>
                                        <div class="modal-footer px-4 py-3 text-right">
                                            <asp:Button ID="btnCloseDetailsBottom" runat="server" Text="<%$ Resources:Strings, Admin_Orders_ModalDismiss %>" CssClass="btn btn-dark font-weight-bold" OnClick="btnCloseOrderDetails_Click" />
                                        </div>
                                    </div>
                                </div>
                            </asp:PlaceHolder>

                            <!-- MODAL DE EVALUACION DE REEMBOLSO -->
                            <asp:PlaceHolder ID="phRefundModal" runat="server" Visible="false">
                                <div class="modal-backdrop-custom">
                                    <div class="modal-dialog-custom">
                                        <div class="modal-header bg-dark text-white px-4 py-3 d-flex justify-content-between align-items-center">
                                            <h5 class="modal-title font-weight-bold" style="color: #FFC800;">
                                                <i class="fas fa-balance-scale mr-2"></i><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Orders_ModalRefundTicket %>" />
                                            </h5>
                                            <asp:LinkButton ID="btnCloseX" runat="server" OnClick="btnCloseModal_Click" CssClass="text-white text-decoration-none" Style="font-size: 1.3rem;">&times;</asp:LinkButton>
                                        </div>
                                        <div class="modal-body">
                                            <div class="row mb-3">
                                                <div class="col-6">
                                                    <small class="text-muted d-block text-uppercase font-weight-bold"><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Orders_ColOrderId %>" /></small>
                                                    <span class="font-weight-bold" style="font-size: 1.1rem;"><asp:Literal ID="litModalOrderId" runat="server" /></span>
                                                </div>
                                                <div class="col-6 text-right">
                                                    <small class="text-muted d-block text-uppercase font-weight-bold"><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Orders_ColTotal %>" /></small>
                                                    <span class="text-danger font-weight-bold" style="font-size: 1.1rem;"><asp:Literal ID="litModalTotal" runat="server" /></span>
                                                </div>
                                            </div>

                                            <div class="mb-3">
                                                <small class="text-muted d-block text-uppercase font-weight-bold"><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Orders_ColCustomer %>" /></small>
                                                <span class="font-weight-bold"><asp:Literal ID="litModalCustomer" runat="server" /></span>
                                            </div>

                                            <div class="mb-3 p-3 modal-section-bg rounded">
                                                <small class="text-muted d-block text-uppercase font-weight-bold mb-1">Customer Selection Reason Concept</small>
                                                <h6 class="font-weight-bold mb-2"><asp:Literal ID="litModalReasonTitle" runat="server" /></h6>
                                                <small class="text-muted d-block font-weight-bold text-uppercase">Customer Additional Notes</small>
                                                <p class="mb-0 text-secondary" style="font-style: italic;"><asp:Literal ID="litModalReasonText" runat="server" /></p>
                                            </div>

                                            <div class="form-group mb-2">
                                                <label for="txtAdminComment" class="font-weight-bold text-uppercase small text-muted"><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Orders_ModalAdminNotes %>" /></label>
                                                <asp:TextBox ID="txtAdminComment" runat="server" TextMode="MultiLine" Rows="3"
                                                    CssClass="form-control" placeholder="Append transaction IDs, physical delivery return status checks, or grounds for denial here..." />
                                            </div>

                                            <!-- CASILLA DE FORZAR REEMBOLSO MANUAL -->
                                            <asp:Panel ID="pnlForceManualOption" runat="server" CssClass="mb-2 text-left" Visible="false">
                                                <div class="p-2 rounded border border-warning modal-section-bg">
                                                    <label class="gold-checkbox font-weight-bold m-0" style="font-size: 0.85rem;">
                                                        <asp:CheckBox ID="chkForceManualRefund" runat="server" ClientIDMode="Static" />
                                                        <span class="checkmark"></span>
                                                        <asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Orders_ForceManualRefund %>" />
                                                    </label>
                                                    <small class="text-muted d-block mt-1" style="font-size: 0.78rem;">
                                                        <asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Orders_ForceManualNote %>" />
                                                    </small>
                                                </div>
                                            </asp:Panel>

                                            <asp:Label ID="lblModalError" runat="server" CssClass="alert alert-danger d-block mt-2 font-weight-bold small" Visible="false" />
                                        </div>
                                        <div class="modal-footer px-4 py-3 d-flex justify-content-between">
                                            <asp:Button ID="btnCancelRefund" runat="server" Text="<%$ Resources:Strings, Admin_Orders_ModalDismiss %>" CssClass="btn btn-secondary font-weight-bold" OnClick="btnCloseModal_Click" />
                                            <div>
                                                <asp:LinkButton ID="btnRejectRefund" runat="server" CssClass="btn btn-danger font-weight-bold mr-2 px-3" OnClick="btnRejectRefund_Click">
                                                    <i class="fas fa-times-circle mr-1"></i> <asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Orders_ModalDenyTicket %>" />
                                                </asp:LinkButton>
                                                <asp:LinkButton ID="btnApproveRefund" runat="server" OnClick="btnApproveRefund_Click" />
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </asp:PlaceHolder>
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

    <!-- SCRIPT PARA LIBERAR EL MODAL Y CENTRARLO ABSOLUTAMENTE -->
    <script type="text/javascript">
        function forzarCentradoModal() {
            var modal = document.querySelector('.modal-backdrop-custom');
            var form = document.forms[0];

            if (modal && modal.parentNode !== form) {
                form.appendChild(modal);
            }
        }

        document.addEventListener('DOMContentLoaded', forzarCentradoModal);

        if (typeof Sys !== 'undefined' && Sys.WebForms && Sys.WebForms.PageRequestManager) {
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
                forzarCentradoModal();
            });
        }
    </script>
</body>
</html>