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
            border: 1px solid var(--border-color, #e0e0e0);
            border-radius: 12px;
            width: 95% !important;
            max-width: 600px;
            max-height: 85vh !important; /* Limita la altura para que no desborde la pantalla */
            box-shadow: 0 10px 40px rgba(0,0,0,0.5);
            z-index: 99999 !important;
            display: flex;
            flex-direction: column;
            animation: modalPop 0.3s ease-out forwards;
        }

        .modal-dialog-custom .modal-body {
            overflow-y: auto !important; /* Habilita el scroll interno si hay mucho contenido */
            padding: 20px;
        }

        .modal-dialog-large { max-width: 750px !important; }

        @keyframes modalPop {
            from { transform: scale(0.9); opacity: 0; }
            to { transform: scale(1); opacity: 1; }
        }

        /* Estilos Paginación del GridView */
        .grid-pager table { margin: 15px auto 0 auto; }
        .grid-pager table td { padding: 0 5px; }
        .grid-pager a, .grid-pager span { display: inline-block; padding: 8px 14px; border-radius: 4px; font-weight: bold; background: #333; color: white; text-decoration: none; }
        .grid-pager a:hover { background: #FFC800; color: black; }
        .grid-pager span { background: #FFC800; color: black; border: 2px solid #FFC800; }
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
                    <li><a id="btnManageOrders" runat="server" href="ManageOrders.aspx" class="sidebar-btn active" style="font-family: 'Raleway','Font Awesome 5 Free'; font-weight: 600;">&#xf46d; Manage Orders</a></li>
                    <li><asp:Button ID="btnManageOffers" CssClass="sidebar-btn" runat="server" Text="&#xf155; Manage Offers" OnClick="btnManageOffers_Click" Style="font-family: 'Raleway', 'Font Awesome 5 Free'; font-weight: 600;" /></li>
                    <li><asp:Button ID="btnManageCoupons" CssClass="sidebar-btn" runat="server" Text="&#xf02c; Manage Coupons" OnClick="btnManageCoupons_Click" Style="font-family: 'Raleway', 'Font Awesome 5 Free'; font-weight: 600;" /></li>
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

            <main class="main-content fade-in" style="animation-delay: 0.15s;">
                <div class="container-fluid">
                    <h1 class="page-title">Order System Hub</h1>
                    <p class="text-muted mb-4">Process system-wide standard retail orders, check electronic settlement routes, and resolve refund requests safely.</p>

                    <asp:UpdatePanel ID="upMainOrders" runat="server">
                        <ContentTemplate>
                            <div class="nav-tabs-custom d-flex">
                                <asp:LinkButton ID="btnTabOrders" runat="server" CssClass="nav-link active" OnClick="btnTabOrders_Click">
                                    <i class="fas fa-boxes mr-2"></i>Active Standard Orders
                                </asp:LinkButton>
                                <asp:LinkButton ID="btnTabRefunds" runat="server" CssClass="nav-link" OnClick="btnTabRefunds_Click">
                                    <i class="fas fa-hand-holding-usd mr-2"></i>Refund Request Tickets
                                    <span class="badge badge-danger badge-refund">
                                        <asp:Literal ID="litRefundBadgeCount" runat="server" Text="0"></asp:Literal>
                                    </span>
                                </asp:LinkButton>
                            </div>

                            <asp:PlaceHolder ID="phOrdersView" runat="server" Visible="true">
                                <div class="filter-card">
                                    <div class="row align-items-end">
                                        <div class="col-md-3 col-sm-12 mb-2 mb-md-0">
                                            <label for="ddlFilterStatus">Status Filter</label>
                                            <asp:DropDownList ID="ddlFilterStatus" runat="server" CssClass="form-control"></asp:DropDownList>
                                        </div>
                                        <div class="col-md-3 col-sm-12 mb-2 mb-md-0">
                                            <label for="txtStartDate">Start Date</label>
                                            <asp:TextBox ID="txtStartDate" runat="server" CssClass="form-control" TextMode="Date"></asp:TextBox>
                                        </div>
                                        <div class="col-md-3 col-sm-12 mb-2 mb-md-0">
                                            <label for="txtEndDate">End Date</label>
                                            <asp:TextBox ID="txtEndDate" runat="server" CssClass="form-control" TextMode="Date"></asp:TextBox>
                                        </div>
                                        <div class="col-md-3 col-sm-12">
                                            <asp:LinkButton ID="btnApplyFilters" runat="server" CssClass="btn btn-warning font-weight-bold w-100" OnClick="btnApplyFilters_Click">
                                                <i class="fas fa-filter"></i> Apply Filters
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
                                        EmptyDataText="No active standard orders match your selection criteria.">
                                        
                                        <PagerStyle CssClass="grid-pager" HorizontalAlign="Center" />

                                        <Columns>
                                            <asp:BoundField DataField="Id_Order" HeaderText="Order ID" ItemStyle-Width="90px" />
                                            <asp:BoundField DataField="CustomerName" HeaderText="Customer Name" ItemStyle-HorizontalAlign="Left" />
                                            <asp:BoundField DataField="Mail" HeaderText="Email" />
                                            <asp:BoundField DataField="OrderDate" HeaderText="Date" DataFormatString="{0:yyyy-MM-dd HH:mm}" />
                                            <asp:BoundField DataField="City_Name" HeaderText="Department" />
                                            <asp:BoundField DataField="Total" HeaderText="Total" DataFormatString="{0:C}" HtmlEncode="false" />
                                            <asp:BoundField DataField="Status_Name" HeaderText="Status State" />
                                            <asp:TemplateField HeaderText="Actions" ItemStyle-Width="260px">
                                                <ItemTemplate>
                                                    <div class="d-flex align-items-center justify-content-center gap-2">
                                                        <asp:LinkButton ID="lnkViewDetails" runat="server" CssClass="btn btn-sm btn-info text-white font-weight-bold mr-2"
                                                            OnClick="lnkViewDetails_Click" CommandArgument='<%# Eval("Id_Order") %>'>
                                                            <i class="fas fa-eye"></i> Details
                                                        </asp:LinkButton>
                                                        <asp:DropDownList ID="ddlGridStatus" runat="server" AutoPostBack="true"
                                                            OnSelectedIndexChanged="ddlGridStatus_SelectedIndexChanged"
                                                            CssClass="form-control form-control-sm" style="max-width: 130px;">
                                                            <asp:ListItem Value="1" Text="Pending"></asp:ListItem>
                                                            <asp:ListItem Value="2" Text="Paid"></asp:ListItem>
                                                            <asp:ListItem Value="3" Text="Shipped"></asp:ListItem>
                                                            <asp:ListItem Value="4" Text="Delivered"></asp:ListItem>
                                                            <asp:ListItem Value="5" Text="Cancelled"></asp:ListItem>
                                                            <asp:ListItem Value="9" Text="Ready for Pickup"></asp:ListItem>
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
                                        EmptyDataText="Excellent! No pending refund request tickets found.">
                                        <Columns>
                                            <asp:BoundField DataField="Id_Order" HeaderText="Order ID" ItemStyle-Width="100px" />
                                            <asp:BoundField DataField="CustomerName" HeaderText="Client Name" ItemStyle-HorizontalAlign="Left" />
                                            <asp:BoundField DataField="Mail" HeaderText="Email" />
                                            <asp:BoundField DataField="Reason_Title" HeaderText="Reason Concept" ItemStyle-HorizontalAlign="Left" />
                                            <asp:BoundField DataField="Total" HeaderText="Amount to Refund" DataFormatString="{0:C}" HtmlEncode="false" />
                                            <asp:BoundField DataField="Method_Name" HeaderText="Payment Channel" />
                                            <asp:BoundField DataField="Created_At" HeaderText="Requested At" DataFormatString="{0:yyyy-MM-dd HH:mm}" />
                                            <asp:TemplateField HeaderText="Evaluation">
                                                <ItemTemplate>
                                                    <asp:LinkButton ID="lnkEvaluateRefund" runat="server"
                                                        CssClass="btn btn-sm btn-warning font-weight-bold px-3"
                                                        OnClick="lnkEvaluateRefund_Click" CommandArgument='<%# Eval("Id_Order") %>'>
                                                        <i class="fas fa-search-dollar mr-1"></i> Evaluate
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
                                                <i class="fas fa-shopping-bag mr-2"></i>Order Breakdown Details #<asp:Literal ID="litDetOrderId" runat="server" />
                                            </h5>
                                            <asp:LinkButton ID="lnkCloseDetX" runat="server" OnClick="btnCloseOrderDetails_Click" CssClass="text-white text-decoration-none" Style="font-size: 1.3rem;">&times;</asp:LinkButton>
                                        </div>
                                        <div class="modal-body">
                                            <div class="row mb-3 border-bottom pb-3">
                                                <div class="col-md-6">
                                                    <small class="text-muted d-block text-uppercase font-weight-bold">Customer Info</small>
                                                    <span class="font-weight-bold d-block"><asp:Literal ID="litDetCustomer" runat="server" /></span>
                                                    <small class="text-secondary"><asp:Literal ID="litDetEmail" runat="server" /> | <asp:Literal ID="litDetPhone" runat="server" /></small>
                                                </div>
                                                <div class="col-md-6 text-md-right">
                                                    <small class="text-muted d-block text-uppercase font-weight-bold">Logistics Address</small>
                                                    <span class="d-block font-weight-bold text-dark" style="font-size: 0.9rem;"><asp:Literal ID="litDetAddress" runat="server" /></span>
                                                    <small class="text-muted"><asp:Literal ID="litDetLocation" runat="server" /></small>
                                                </div>
                                            </div>

                                            <div class="mb-3">
                                                <h6 class="font-weight-bold text-uppercase small text-muted mb-2">Line Items</h6>
                                                <asp:GridView ID="gvOrderDetailItems" runat="server" AutoGenerateColumns="false" CssClass="table table-sm table-striped table-bordered text-center small" GridLines="None">
                                                    <Columns>
                                                        <asp:BoundField DataField="ProductName" HeaderText="Jersey Name" ItemStyle-HorizontalAlign="Left" HeaderStyle-CssClass="bg-dark text-white" />
                                                        <asp:BoundField DataField="Size" HeaderText="Size" ItemStyle-Width="60px" HeaderStyle-CssClass="bg-dark text-white" />
                                                        <asp:BoundField DataField="Price" HeaderText="Price" DataFormatString="{0:C}" HtmlEncode="false" ItemStyle-Width="80px" HeaderStyle-CssClass="bg-dark text-white" />
                                                        <asp:BoundField DataField="Quantity" HeaderText="Qty" ItemStyle-Width="50px" HeaderStyle-CssClass="bg-dark text-white" />
                                                        <asp:TemplateField HeaderText="Customization Prints" HeaderStyle-CssClass="bg-dark text-white">
                                                            <ItemTemplate>
                                                                <%# (Eval("CustomName") != DBNull.Value && !string.IsNullOrEmpty(Eval("CustomName").ToString())) ? 
                                                                    "<span class='badge bg-warning text-dark font-weight-bold'>" + Eval("CustomName") + " #" + Eval("CustomNumber") + "</span>" : 
                                                                    "<span class='text-muted'>None</span>" %>
                                                            </ItemTemplate>
                                                        </asp:TemplateField>
                                                        <asp:BoundField DataField="Subtotal" HeaderText="Subtotal" DataFormatString="{0:C}" HtmlEncode="false" ItemStyle-Width="90px" HeaderStyle-CssClass="bg-dark text-white" />
                                                    </Columns>
                                                </asp:GridView>
                                            </div>

                                            <div class="row align-items-center bg-light p-3 rounded border">
                                                <div class="col-md-7">
                                                    <small class="text-muted d-block text-uppercase font-weight-bold">Order Notes / Instructions</small>
                                                    <p class="mb-0 small text-secondary italic" style="font-style: italic;"><asp:Literal ID="litDetNotes" runat="server" /></p>
                                                </div>
                                                <div class="col-md-5 text-right">
                                                    <small class="text-muted d-block text-uppercase font-weight-bold">Financial Summary</small>
                                                    <small class="text-muted d-block">Shipping Cost: <asp:Literal ID="litDetShipping" runat="server" /></small>
                                                    <span class="font-weight-bold text-success" style="font-size: 1.3rem;">Total: <asp:Literal ID="litDetTotal" runat="server" /></span>
                                                </div>
                                            </div>
                                        </div>
                                        <div class="modal-footer bg-light px-4 py-3 text-right">
                                            <asp:Button ID="btnCloseDetailsBottom" runat="server" Text="Dismiss Window" CssClass="btn btn-dark font-weight-bold" OnClick="btnCloseOrderDetails_Click" />
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
                                                <i class="fas fa-balance-scale mr-2"></i>Refund Evaluation Ticket
                                            </h5>
                                            <asp:LinkButton ID="btnCloseX" runat="server" OnClick="btnCloseModal_Click" CssClass="text-white text-decoration-none" Style="font-size: 1.3rem;">&times;</asp:LinkButton>
                                        </div>
                                        <div class="modal-body">
                                            <div class="row mb-3">
                                                <div class="col-6">
                                                    <small class="text-muted d-block text-uppercase font-weight-bold">Order ID</small>
                                                    <span class="font-weight-bold" style="font-size: 1.1rem;"><asp:Literal ID="litModalOrderId" runat="server" /></span>
                                                </div>
                                                <div class="col-6 text-right">
                                                    <small class="text-muted d-block text-uppercase font-weight-bold">Total Amount</small>
                                                    <span class="text-danger font-weight-bold" style="font-size: 1.1rem;"><asp:Literal ID="litModalTotal" runat="server" /></span>
                                                </div>
                                            </div>

                                            <div class="mb-3">
                                                <small class="text-muted d-block text-uppercase font-weight-bold">Customer</small>
                                                <span class="font-weight-bold"><asp:Literal ID="litModalCustomer" runat="server" /></span>
                                            </div>

                                            <div class="mb-3 p-3 bg-light rounded border">
                                                <small class="text-muted d-block text-uppercase font-weight-bold mb-1">Customer Selection Reason Concept</small>
                                                <h6 class="font-weight-bold mb-2 text-dark"><asp:Literal ID="litModalReasonTitle" runat="server" /></h6>
                                                <small class="text-muted d-block font-weight-bold text-uppercase">Customer Additional Notes</small>
                                                <p class="mb-0 text-secondary" style="font-style: italic;"><asp:Literal ID="litModalReasonText" runat="server" /></p>
                                            </div>

                                            <div class="form-group mb-0">
                                                <label for="txtAdminComment" class="font-weight-bold text-uppercase small text-muted">Administrative Settlement Resolution Notes</label>
                                                <asp:TextBox ID="txtAdminComment" runat="server" TextMode="MultiLine" Rows="3"
                                                    CssClass="form-control text-dark" placeholder="Append transaction IDs, physical delivery return status checks, or grounds for denial here..." />
                                            </div>
                                            <asp:Label ID="lblModalError" runat="server" CssClass="alert alert-danger d-block mt-3 font-weight-bold small" Visible="false" />
                                        </div>
                                        <div class="modal-footer bg-light px-4 py-3 d-flex justify-content-between">
                                            <asp:Button ID="btnCancelRefund" runat="server" Text="Close Window" CssClass="btn btn-secondary font-weight-bold" OnClick="btnCloseModal_Click" />
                                            <div>
                                                <asp:LinkButton ID="btnRejectRefund" runat="server" CssClass="btn btn-danger font-weight-bold mr-2 px-3" OnClick="btnRejectRefund_Click">
                                                    <i class="fas fa-times-circle mr-1"></i> Deny Ticket
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
            // Buscamos el modal abierto
            var modal = document.querySelector('.modal-backdrop-custom');
            var form = document.forms[0]; // El form principal (form1)

            // Si el modal existe y está atrapado en el main, lo movemos a la raíz del form
            if (modal && modal.parentNode !== form) {
                form.appendChild(modal);
            }
        }

        // 1. Intentar centrarlo al cargar la página por primera vez
        document.addEventListener('DOMContentLoaded', forzarCentradoModal);

        // 2. FUNDAMENTAL: Volver a centrarlo CADA VEZ que el UpdatePanel hace un PostBack (al dar clic en "Details")
        if (typeof Sys !== 'undefined' && Sys.WebForms && Sys.WebForms.PageRequestManager) {
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
                forzarCentradoModal();
            });
        }
    </script>
</body>
</html>
