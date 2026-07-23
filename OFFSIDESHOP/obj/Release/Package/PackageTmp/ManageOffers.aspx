<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ManageOffers.aspx.cs" Inherits="OFFSIDESHOP.ManageOffers" Culture="en-US" UICulture="en-US" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1, shrink-to-fit=no" />
    <title>Manage Offers | OffsideShop</title>

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
        window.swalQueue = [];
        window.Swal = {
            fire: function (...args) {
                window.swalQueue.push(args);
            }
        };
    </script>

    <script type="text/javascript">
        window.onpageshow = function (event) {
            if (event.persisted) { window.location.reload(); }
        };

        function validarPorcentaje(e) {
            var tecla = (document.all) ? e.keyCode : e.which;
            if (tecla == 8) return true;
            return /\d/.test(String.fromCharCode(tecla));
        }
    </script>

    <style>
        .status-badge {
            padding: 3px 12px;
            border-radius: 20px;
            font-size: 0.75rem;
            font-weight: 700;
            letter-spacing: 0.5px;
        }

        .status-active {
            background: #1a7a4a;
            color: #a8f0c6;
        }

        .status-inactive {
            background: #5c2323;
            color: #f0a8a8;
        }

        .filter-card {
            background: var(--card-bg);
            border: 1px solid var(--border-color);
            border-radius: 14px;
            padding: 20px 24px;
            margin-bottom: 28px;
            box-shadow: 0 6px 20px rgba(0,0,0,0.4);
        }

            .filter-card label {
                color: var(--text-muted);
                font-weight: 600;
                font-size: 0.8rem;
                text-transform: uppercase;
                letter-spacing: 0.8px;
                margin-bottom: 6px;
            }

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
            from {
                opacity: 0;
                transform: translateY(-18px);
            }

            to {
                opacity: 1;
                transform: translateY(0);
            }
        }

        .form-panel h4 {
            font-weight: 700;
            margin-bottom: 24px;
            background: var(--gradient-blue);
            -webkit-background-clip: text;
            -webkit-text-fill-color: transparent;
        }

        .btn-add-new {
            background: var(--gradient-blue);
            color: #fff;
            border: none;
            border-radius: 10px;
            padding: 10px 22px;
            font-weight: 700;
            font-size: 0.92rem;
            cursor: pointer;
            transition: all 0.3s ease;
            text-decoration: none;
            display: inline-block;
        }

            .btn-add-new:hover {
                transform: translateY(-2px);
                box-shadow: 0 6px 18px rgba(37,99,235,0.45);
                color: #fff;
                text-decoration: none;
            }

        .btn-action {
            border: none;
            border-radius: 7px;
            padding: 5px 10px;
            font-size: 0.82rem;
            cursor: pointer;
            transition: all 0.25s ease;
            margin: 2px;
        }

        .btn-edit {
            background: #1e3a8a;
            color: #93c5fd;
        }

        .btn-toggle {
            background: #374151;
            color: #d1d5db;
        }

        .btn-delete {
            background: #7f1d1d;
            color: #fca5a5;
        }

        .btn-action:hover {
            opacity: 0.85;
            transform: scale(1.08);
        }

        .btn-save {
            background: var(--gradient-blue);
            color: #fff;
            border: none;
            border-radius: 9px;
            padding: 11px 28px;
            font-weight: 700;
            font-size: 0.95rem;
            cursor: pointer;
            transition: all 0.3s ease;
        }

            .btn-save:hover {
                transform: translateY(-2px);
                box-shadow: 0 5px 16px rgba(37,99,235,0.4);
            }

        .btn-cancel-form {
            background: #222;
            color: #999;
            border: 1px solid #333;
            border-radius: 9px;
            padding: 11px 24px;
            font-weight: 600;
            font-size: 0.95rem;
            cursor: pointer;
            transition: all 0.3s ease;
        }

            .btn-cancel-form:hover {
                background: #2a2a2a;
                color: #ccc;
            }

        .section-header {
            display: flex;
            align-items: center;
            justify-content: space-between;
            margin-bottom: 22px;
        }

            .section-header h3 {
                font-weight: 700;
                font-size: 1.35rem;
                color: #e5e7eb;
                margin: 0;
            }

        .pagination-custom td {
            padding: 24px 4px 10px 4px;
        }

        .pagination-custom a,
        .pagination-custom span {
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
                background: linear-gradient(135deg, #f59e0b, #d97706);
                color: #111827 !important;
                box-shadow: 0 5px 15px rgba(245, 158, 11, 0.4);
            }

        html.dark-mode .pagination-custom span {
            background: linear-gradient(135deg, #fbbf24, #f59e0b);
            color: #111827;
            box-shadow: 0 5px 15px rgba(251, 191, 36, 0.5);
        }

        .gold-checkbox {
            position: relative;
            display: flex;
            align-items: center;
            cursor: pointer;
            user-select: none;
            color: var(--text-muted);
            font-weight: 600;
        }

            .gold-checkbox input[type="checkbox"] {
                position: absolute;
                opacity: 0;
                cursor: pointer;
                height: 0;
                width: 0;
            }

        .checkmark {
            height: 20px;
            width: 20px;
            background-color: transparent;
            border: 2px solid #6c757d;
            border-radius: 4px;
            margin-right: 10px;
            display: inline-block;
            position: relative;
            transition: all 0.2s ease;
        }

        .gold-checkbox:hover input ~ .checkmark {
            border-color: #d4af37;
        }

        .gold-checkbox input:checked ~ .checkmark {
            background-color: #d4af37;
            border-color: #d4af37;
        }

        .checkmark:after {
            content: "";
            position: absolute;
            display: none;
            left: 6px;
            top: 2px;
            width: 5px;
            height: 10px;
            border: solid white;
            border-width: 0 2px 2px 0;
            transform: rotate(45deg);
        }

        .gold-checkbox input:checked ~ .checkmark:after {
            display: block;
        }

        .shirt-selector-container {
            border: 1px solid var(--border-color);
            background: rgba(0,0,0,0.2);
            border-radius: 10px;
            padding: 15px;
            max-height: 450px;
            overflow-y: auto;
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
                        <asp:Button ID="btnManageOffers" CssClass="sidebar-btn active" runat="server" Text="&#xf155; Manage Offers" OnClick="btnManageOffers_Click" Style="font-family: 'Raleway', 'Font Awesome 5 Free'; font-weight: 600;" />
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
            <main class="main-content fade-in" style="animation-delay: 0.15s;">
                <div class="container-fluid">
                    <h1 class="page-title">Offer & Promotions Management</h1>
                    <p class="text-muted mb-4">Create global campaign windows, configure percentage discounts, and multi-assign items catalog-wide.</p>
                    <asp:Panel ID="pnlOfferForm" runat="server" Visible="false" CssClass="form-panel">
                        <asp:HiddenField ID="hfSelectedOfferId" runat="server" Value="" />
                        <h4>
                            <asp:Label ID="lblFormTitle" runat="server" Text="Create Promotional Campaign"></asp:Label>
                        </h4>

                        <div class="row">
                            <div class="col-md-6">
                                <div class="form-group">
                                    <label>Campaign Name <span class="text-danger">*</span></label>
                                    <asp:TextBox ID="txtOfferName" runat="server" CssClass="form-control" placeholder="e.g. Black Friday Special" MaxLength="100"></asp:TextBox>
                                </div>
                            </div>
                            <div class="col-md-6">
                                <div class="form-group">
                                    <label>Discount Percentage (%) <span class="text-danger">*</span></label>
                                    <asp:TextBox ID="txtDiscountPercentage" runat="server" CssClass="form-control" placeholder="e.g. 20" MaxLength="3" onkeypress="return validarPorcentaje(event)"></asp:TextBox>
                                </div>
                            </div>
                        </div>

                        <div class="row mt-2">
                            <div class="col-md-6">
                                <div class="form-group">
                                    <label>Start Date & Time <span class="text-danger">*</span></label>
                                    <asp:TextBox ID="txtStartDate" runat="server" CssClass="form-control en-datepicker" placeholder="Select start date..."></asp:TextBox>
                                </div>
                            </div>
                            <div class="col-md-6">
                                <div class="form-group">
                                    <label>End Date & Time <span class="text-danger">*</span></label>
                                    <asp:TextBox ID="txtEndDate" runat="server" CssClass="form-control en-datepicker" placeholder="Select expiration date..."></asp:TextBox>
                                </div>
                            </div>
                        </div>

                        <div class="row mt-3">
                            <div class="col-12">
                                <h5 class="text-white mb-3"><i class="fas fa-check-square text-warning mr-2"></i>Select T-Shirts for this Campaign</h5>

                                <asp:UpdatePanel ID="upShirtSelection" runat="server" UpdateMode="Conditional">
                                    <ContentTemplate>

                                        <div class="filter-card py-2 px-3 mb-3" style="box-shadow: none; background: rgba(255,255,255,0.02);">
                                            <div class="row align-items-end">
                                                <div class="col-md-3 col-sm-6 mb-2">
                                                    <label style="font-size: 0.7rem;">Brand</label>
                                                    <asp:DropDownList ID="ddlShirtBrand" runat="server" CssClass="form-control form-control-sm" AutoPostBack="true" OnSelectedIndexChanged="ShirtFilters_Changed"></asp:DropDownList>
                                                </div>
                                                <div class="col-md-3 col-sm-6 mb-2">
                                                    <label style="font-size: 0.7rem;">League</label>
                                                    <asp:DropDownList ID="ddlShirtLeague" runat="server" CssClass="form-control form-control-sm" AutoPostBack="true" OnSelectedIndexChanged="ddlShirtLeague_SelectedIndexChanged"></asp:DropDownList>
                                                </div>
                                                <div class="col-md-3 col-sm-6 mb-2">
                                                    <label style="font-size: 0.7rem;">Team</label>
                                                    <asp:DropDownList ID="ddlShirtTeam" runat="server" CssClass="form-control form-control-sm" AutoPostBack="true" OnSelectedIndexChanged="ShirtFilters_Changed"></asp:DropDownList>
                                                </div>
                                                <div class="col-md-3 col-sm-6 mb-2">
                                                    <label style="font-size: 0.7rem;">Search</label>
                                                    <asp:TextBox ID="txtShirtSearch" runat="server" CssClass="form-control form-control-sm" placeholder="Find item..." AutoPostBack="true" OnTextChanged="ShirtFilters_Changed"></asp:TextBox>
                                                </div>
                                            </div>
                                        </div>

                                        <div class="d-flex justify-content-end mb-2" style="gap: 10px;">
                                            <asp:LinkButton ID="btnSelectAllShirts" runat="server" CssClass="btn-action btn-edit px-3 py-2" OnClick="btnSelectAllShirts_Click" Style="font-family: 'Raleway', sans-serif; font-weight: 600; text-decoration: none;">
                                                <i class="fas fa-check-double mr-1"></i> Select all (all the pages)
                                            </asp:LinkButton>
                                            <asp:LinkButton ID="btnClearShirtSelection" runat="server" CssClass="btn-action btn-toggle px-3 py-2" OnClick="btnClearShirtSelection_Click" Style="font-family: 'Raleway', sans-serif; font-weight: 600; text-decoration: none;">
                                                <i class="fas fa-eraser mr-1"></i> Clean selection
                                            </asp:LinkButton>
                                        </div>

                                        <div class="shirt-selector-container">
                                            <asp:GridView ID="gvShirtSelection" runat="server" AutoGenerateColumns="False" GridLines="None" CssClass="table table-custom text-center align-middle mb-0" DataKeyNames="ID" AllowPaging="True" PageSize="10" OnPageIndexChanging="gvShirtSelection_PageIndexChanging">
                                                <PagerStyle CssClass="pagination-custom" HorizontalAlign="Center" />
                                                <Columns>
                                                    <asp:TemplateField HeaderText="Apply">
                                                        <ItemTemplate>
                                                            <label class="gold-checkbox d-inline-block m-0">
                                                                <asp:CheckBox ID="chkSelectShirt" runat="server" />
                                                                <span class="checkmark" style="margin-right: 0;"></span>
                                                            </label>
                                                        </ItemTemplate>
                                                        <ItemStyle Width="60px" />
                                                    </asp:TemplateField>
                                                    <asp:BoundField DataField="ID" HeaderText="ID" ItemStyle-Width="60px" />
                                                    <asp:BoundField DataField="Name" HeaderText="Shirt Model" ItemStyle-HorizontalAlign="Left" />
                                                    <asp:BoundField DataField="BrandName" HeaderText="Brand" />
                                                    <asp:BoundField DataField="TeamName" HeaderText="Team" />
                                                    <asp:BoundField DataField="Price" HeaderText="Base Price" DataFormatString="{0:C}" HtmlEncode="false" />
                                                </Columns>
                                            </asp:GridView>
                                        </div>

                                    </ContentTemplate>
                                </asp:UpdatePanel>
                                </div>
                        </div>

                        <div class="row mt-4 mb-4">
                            <div class="col-12 d-flex justify-content-start" style="gap: 10px;">
                                <asp:Button ID="btnSaveOffer" runat="server" Text="&#xf0c7;&nbsp; Save Campaign" CssClass="btn-save" OnClick="btnSaveOffer_Click" Style="font-family: 'Raleway','Font Awesome 5 Free'; font-weight: 700;" />
                                <asp:Button ID="btnCancelForm" runat="server" Text="&#xf00d;&nbsp; Cancel" CssClass="btn-cancel-form" OnClick="btnCancelForm_Click" CausesValidation="false" Style="font-family: 'Raleway','Font Awesome 5 Free'; font-weight: 600;" />
                            </div>
                        </div>

                    </asp:Panel>
                    <div class="section-header mt-4">
                        <h3><i class="fas fa-tags mr-2" style="color: #3b82f6;"></i>Active Promotional Configurations</h3>
                        <asp:LinkButton ID="lbAddNewOffer" runat="server" CssClass="btn-add-new" OnClick="lbAddNewOffer_Click" CausesValidation="false">
                            <i class="fas fa-plus mr-1"></i> Add Promotion Campaign
                        </asp:LinkButton>
                    </div>

                    <div class="table-responsive">
                        <asp:GridView ID="gvOffers" runat="server" AutoGenerateColumns="False" GridLines="None" CssClass="table table-custom text-center align-middle" DataKeyNames="Id_Offer" AllowPaging="True" PageSize="12" OnRowCommand="gvOffers_RowCommand" OnRowDataBound="gvOffers_RowDataBound" OnPageIndexChanging="gvOffers_PageIndexChanging" EmptyDataText="No promotional records found.">
                            <PagerStyle CssClass="pagination-custom" HorizontalAlign="Center" />
                            <Columns>
                                <asp:BoundField DataField="Id_Offer" HeaderText="ID" ItemStyle-Width="60px" />
                                <asp:BoundField DataField="Name_Offer" HeaderText="Promo Campaign Window" ItemStyle-HorizontalAlign="Left" ItemStyle-Font-Bold="true" ItemStyle-ForeColor="White" />
                                <asp:TemplateField HeaderText="Discount">
                                    <ItemTemplate>
                                        <span class="badge bg-warning text-dark font-weight-bold" style="font-size: 0.85rem;"><%# Eval("DiscountPercentage") %>% OFF</span>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:BoundField DataField="StartDate" HeaderText="Starts" DataFormatString="{0:yyyy-MM-dd HH:mm}" />
                                <asp:BoundField DataField="EndDate" HeaderText="Expires" DataFormatString="{0:yyyy-MM-dd HH:mm}" />
                                <asp:TemplateField HeaderText="State">
                                    <ItemTemplate>
                                        <asp:Label ID="lblOfferStatus" runat="server"></asp:Label>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Actions" ItemStyle-Width="140px">
                                    <ItemTemplate>
                                        <asp:Button ID="btnEditOffer" runat="server" CssClass="btn-action btn-edit" CommandName="EditOffer" CommandArgument='<%# Eval("Id_Offer") %>' Text="&#xf044;" Style="font-family: 'Font Awesome 5 Free','Raleway'; font-weight: 900;" />
                                        <asp:Button ID="btnToggleOffer" runat="server" CssClass="btn-action btn-toggle" CommandName="ToggleOffer" CommandArgument='<%# Eval("Id_Offer") %>' Text="&#xf06e;" Style="font-family: 'Font Awesome 5 Free','Raleway'; font-weight: 900;" />
                                        <asp:Button ID="btnDeleteOffer" runat="server" CssClass="btn-action btn-delete" CommandName="DeleteOffer" CommandArgument='<%# Eval("Id_Offer") %>' OnClientClick="return confirm('Purge this campaign and normalize affected products base pricing records?');" Text="&#xf2ed;" Style="font-family: 'Font Awesome 5 Free','Raleway'; font-weight: 900;" />
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                        </asp:GridView>
                    </div>

                    <asp:Literal ID="alerta" runat="server" EnableViewState="false"></asp:Literal>
                </div>
            </main>
        </div>
    </form>

    <script src="https://ajax.googleapis.com/ajax/libs/jquery/3.4.1/jquery.min.js"></script>
    <script src="https://cdnjs.cloudflare.com/ajax/libs/popper.js/1.14.7/umd/popper.min.js"></script>
    <script src="https://maxcdn.bootstrapcdn.com/bootstrap/4.3.1/js/bootstrap.min.js"></script>
    <script src="/SweetAlert/sweetalert2.all.min.js"></script>
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/flatpickr/dist/flatpickr.min.css" />
    <script src="https://cdn.jsdelivr.net/npm/flatpickr"></script>

    <script type="text/javascript">
        (function () {
            if (window.Swal) {
                const realSwalFire = window.Swal.fireOriginal || window.Swal.fire;
                window.Swal.fire = function (...args) {
                    if (args.length > 0 && typeof args[0] !== 'object') {
                        var iconType = args[2] || undefined;
                        return realSwalFire.call(window.Swal, {
                            title: args[0],
                            text: args[1] || '',
                            icon: iconType,
                            type: iconType,
                            confirmButtonColor: '#FFC800'
                        });
                    }
                    if (args.length === 1 && typeof args[0] === 'object') {
                        if (!args[0].confirmButtonColor) {
                            args[0].confirmButtonColor = '#FFC800';
                        }
                    }
                    return realSwalFire.apply(window.Swal, args);
                };
                if (window.swalQueue && window.swalQueue.length > 0) {
                    window.swalQueue.forEach(function (pendingArgs) { window.Swal.fire(...pendingArgs); });
                    window.swalQueue = [];
                }
            }
        })();

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

        // Configuración de Flatpickr con protección ante fechas pasadas
        document.addEventListener('DOMContentLoaded', function () {
            var txtStart = document.querySelector('[id$="txtStartDate"]');
            var txtEnd = document.querySelector('[id$="txtEndDate"]');

            if (txtStart && txtEnd) {
                var minStartDate = txtStart.value ? txtStart.value : "today";
                var minEndDate = txtEnd.value ? txtEnd.value : "today";

                var startPicker = flatpickr(txtStart, {
                    enableTime: true,
                    dateFormat: "Y-m-d H:i",
                    altInput: true,
                    altFormat: "m/d/Y h:i K",
                    time_24hr: false,
                    minDate: minStartDate,
                    onChange: function (selectedDates, dateStr, instance) {
                        endPicker.set("minDate", dateStr || "today");
                    }
                });

                var endPicker = flatpickr(txtEnd, {
                    enableTime: true,
                    dateFormat: "Y-m-d H:i",
                    altInput: true,
                    altFormat: "m/d/Y h:i K",
                    time_24hr: false,
                    minDate: minEndDate
                });
            }
        });
    </script>
</body>
</html>
