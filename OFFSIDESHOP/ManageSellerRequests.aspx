<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ManageSellerRequests.aspx.cs" Inherits="OFFSIDESHOP.ManageSellerRequests" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1, shrink-to-fit=no" />
    <title>Manage Support & Seller Requests | OffsideShop</title>

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

        .nav-tabs-custom {
            border-bottom: 2px solid var(--border-color);
            margin-bottom: 25px;
        }

            .nav-tabs-custom .nav-link {
                border: none;
                color: var(--text-muted);
                font-weight: 600;
                padding: 12px 20px;
                background: transparent;
                transition: all 0.3s ease;
            }

                .nav-tabs-custom .nav-link.active {
                    color: #FFC800 !important;
                    border-bottom: 3px solid #FFC800;
                    background: transparent;
                }

        main.main-content {
            transform: none !important;
            position: relative;
            z-index: 9999;
        }

        .modal-backdrop-custom {
            position: fixed;
            top: 0;
            left: 0;
            right: 0;
            bottom: 0;
            width: 100vw;
            height: 100vh;
            background-color: rgba(0, 0, 0, 0.75);
            z-index: 99999;
            display: flex;
            align-items: center;
            justify-content: center;
            padding: 20px;
        }

        .modal-dialog-custom {
            background: var(--card-bg, #ffffff);
            border: 1px solid var(--border-color, #e0e0e0);
            border-radius: 12px;
            width: 100%;
            max-width: 750px;
            max-height: 90vh;
            display: flex;
            flex-direction: column;
            box-shadow: 0 10px 30px rgba(0, 0, 0, 0.5);
            z-index: 100000;
            overflow: hidden;
            animation: slideDown 0.3s ease-out;
            margin: auto;
        }

        @keyframes slideDown {
            from {
                transform: translateY(-30px);
                opacity: 0;
            }

            to {
                transform: translateY(0);
                opacity: 1;
            }
        }

        .zoom-effect {
            transition: transform 0.3s ease-in-out, box-shadow 0.3s ease-in-out;
            cursor: zoom-in;
        }

            .zoom-effect:hover {
                transform: scale(1.6);
                z-index: 9999;
                box-shadow: 0 10px 25px rgba(0,0,0,0.5);
                position: relative;
            }

        .badge-status {
            font-size: 0.75rem;
            padding: 5px 10px;
            border-radius: 12px;
            font-weight: bold;
        }

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
     /* ========================================================= */
/* FIX DEFINITIVO PARA VISIBILIDAD DE PESTAÑAS (TABS)        */
/* ========================================================= */
.nav-tabs-custom {
    border-bottom: 2px solid #e5e7eb !important;
    margin-bottom: 25px !important;
    display: flex !important;
    flex-wrap: wrap !important;
}

/* Estado Inactivo (Pestañas no seleccionadas: Normal, Visited, Focus, Hover) */
.nav-tabs-custom .nav-link,
.nav-tabs-custom .nav-link:link,
.nav-tabs-custom .nav-link:visited {
    border: none !important;
    background: transparent !important;
    color: #4b5563 !important; /* Gris oscuro 100% visible en fondo blanco */
    font-weight: 700 !important;
    padding: 12px 20px !important;
    transition: all 0.2s ease-in-out !important;
    text-decoration: none !important;
    opacity: 0.85 !important;
    display: inline-flex !important;
    align-items: center !important;
}

/* Iconos y texto interno en pestañas inactivas */
.nav-tabs-custom .nav-link *,
.nav-tabs-custom .nav-link i,
.nav-tabs-custom .nav-link span {
    color: #4b5563 !important;
}

/* Estado Hover y Focus (Al pasar el mouse o al hacer clic) */
.nav-tabs-custom .nav-link:hover,
.nav-tabs-custom .nav-link:focus,
.nav-tabs-custom .nav-link:active {
    color: #111827 !important;
    opacity: 1 !important;
    text-decoration: none !important;
}

.nav-tabs-custom .nav-link:hover *,
.nav-tabs-custom .nav-link:focus *,
.nav-tabs-custom .nav-link:active * {
    color: #111827 !important;
}

/* Estado Activo (Pestaña Seleccionada) */
.nav-tabs-custom .nav-link.active,
.nav-tabs-custom .nav-link.active:link,
.nav-tabs-custom .nav-link.active:visited,
.nav-tabs-custom .nav-link.active:hover,
.nav-tabs-custom .nav-link.active:focus {
    color: #d97706 !important; /* Dorado / Amarillo oscuro legible */
    border: none !important;
    border-bottom: 3px solid #f59e0b !important; /* Línea inferior dorada */
    background: transparent !important;
    opacity: 1 !important;
}

/* Iconos y texto interno en pestaña activa */
.nav-tabs-custom .nav-link.active *,
.nav-tabs-custom .nav-link.active i,
.nav-tabs-custom .nav-link.active span {
    color: #d97706 !important;
}

/* Adaptación automática si se activa el MODO OSCURO */
html.dark-mode .nav-tabs-custom .nav-link,
html.dark-mode .nav-tabs-custom .nav-link:link,
html.dark-mode .nav-tabs-custom .nav-link:visited,
body.dark-mode .nav-tabs-custom .nav-link {
    color: #9ca3af !important;
}

html.dark-mode .nav-tabs-custom .nav-link *,
body.dark-mode .nav-tabs-custom .nav-link * {
    color: #9ca3af !important;
}

html.dark-mode .nav-tabs-custom .nav-link.active,
body.dark-mode .nav-tabs-custom .nav-link.active {
    color: #FFC800 !important;
    border-bottom-color: #FFC800 !important;
}

html.dark-mode .nav-tabs-custom .nav-link.active *,
body.dark-mode .nav-tabs-custom .nav-link.active * {
    color: #FFC800 !important;
}
    </style>
    <script type="text/javascript">
        function openFullscreenImage(imgElement) {
            if (!imgElement) return;
            var src = imgElement.src;
            if (!src) return;
            var overlay = document.getElementById('fullscreenOverlay');
            var overlayImg = document.getElementById('fullscreenOverlayImg');
            overlayImg.src = src;
            overlay.style.display = 'flex';
        }
        function closeFullscreenImage() {
            document.getElementById('fullscreenOverlay').style.display = 'none';
        }
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

        <asp:UpdatePanel ID="upMainRequests" runat="server">
            <ContentTemplate>
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
                                <a id="btnManageOrders" runat="server" href="ManageOrders.aspx" class="sidebar-btn" style="font-family: 'Raleway','Font Awesome 5 Free'; font-weight: 600;">&#xf46d;
                                    <asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Nav_ManageOrders %>" />
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
                                <a id="btnManageTickets" runat="server" href="ManageSellerRequests.aspx" class="sidebar-btn active" style="font-family: 'Raleway','Font Awesome 5 Free'; font-weight: 600;">&#xf2b5;
                                    <asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Nav_SellerRequests %>" />
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
                            <h1 class="page-title">
                                <asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Seller_Title %>" /></h1>
                            <p class="text-muted mb-4">
                                <asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Seller_Subtitle %>" /></p>

                            <!-- Main Tabs -->
                            <div class="nav-tabs-custom d-flex flex-wrap mb-4" style="border-bottom: 2px solid #FFC800;">
                                <asp:LinkButton ID="btnViewTickets" runat="server" CssClass="nav-link active" OnClick="MainView_Click" CommandArgument="Tickets" Style="font-size: 1.1rem;"></asp:LinkButton>
                                <asp:LinkButton ID="btnViewReviews" runat="server" CssClass="nav-link" OnClick="MainView_Click" CommandArgument="Reviews" Style="font-size: 1.1rem;"></asp:LinkButton>
                            </div>

                            <asp:Panel ID="pnlTickets" runat="server">
                                <!-- Tab filters by status -->
                                <div class="nav-tabs-custom d-flex flex-wrap">
                                    <asp:LinkButton ID="btnTabPending" runat="server" CssClass="nav-link active" OnClick="StatusTab_Click" CommandArgument="1"></asp:LinkButton>
                                    <asp:LinkButton ID="btnTabUnderReview" runat="server" CssClass="nav-link" OnClick="StatusTab_Click" CommandArgument="2"></asp:LinkButton>
                                    <asp:LinkButton ID="btnTabResolved" runat="server" CssClass="nav-link" OnClick="StatusTab_Click" CommandArgument="3"></asp:LinkButton>
                                    <asp:LinkButton ID="btnTabDenied" runat="server" CssClass="nav-link" OnClick="StatusTab_Click" CommandArgument="4"></asp:LinkButton>
                                </div>

                                <!-- Dropdown filter by type -->
                                <div class="filter-card">
                                    <div class="row align-items-end">
                                        <div class="col-md-5 col-sm-12">
                                            <label for="ddlFilterType">
                                                <asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Seller_FilterType %>" /></label>
                                            <asp:DropDownList ID="ddlFilterType" runat="server"
                                                CssClass="form-control text-dark font-weight-bold"
                                                AutoPostBack="true"
                                                OnSelectedIndexChanged="ddlFilterType_SelectedIndexChanged">
                                                <asp:ListItem Value="ALL" Text="<%$ Resources:Strings, Admin_Seller_TypeAll %>" Selected="True"></asp:ListItem>
                                                <asp:ListItem Value="GENERAL" Text="<%$ Resources:Strings, Admin_Seller_TypeGeneral %>"></asp:ListItem>
                                                <asp:ListItem Value="ORDER" Text="<%$ Resources:Strings, Admin_Seller_TypeOrder %>"></asp:ListItem>
                                                <asp:ListItem Value="SELLER" Text="<%$ Resources:Strings, Admin_Seller_TypeReventa %>"></asp:ListItem>
                                            </asp:DropDownList>
                                        </div>
                                        <div class="col-md-7 col-sm-12 text-right">
                                            <asp:LinkButton ID="btnClearFilters" runat="server" CssClass="btn btn-outline-secondary font-weight-bold" OnClick="btnClearFilters_Click">
                                            <i class="fas fa-eraser mr-1"></i> <asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Seller_ClearFilters %>" />
                                            </asp:LinkButton>
                                        </div>
                                    </div>
                                </div>

                                <!-- Tickets Grid -->
                                <div class="table-responsive">
                                    <asp:GridView ID="gvTickets" runat="server"
                                        AutoGenerateColumns="False"
                                        GridLines="None"
                                        CssClass="table table-custom text-center align-middle"
                                        DataKeyNames="Id_Ticket"
                                        OnRowCommand="gvTickets_RowCommand"
                                        OnRowDataBound="gvTickets_RowDataBound"
                                        EmptyDataText="<%$ Resources:Strings, Admin_Seller_EmptyTickets %>">
                                        <Columns>
                                            <asp:BoundField DataField="Id_Ticket" HeaderText="<%$ Resources:Strings, Admin_Seller_ColTicketId %>" ItemStyle-Width="90px" />
                                            <asp:BoundField DataField="Created_At" HeaderText="<%$ Resources:Strings, Admin_Seller_ColDateReq %>" DataFormatString="{0:yyyy-MM-dd HH:mm}" />
                                            <asp:BoundField DataField="User_Email" HeaderText="<%$ Resources:Strings, Admin_Seller_ColEmail %>" ItemStyle-HorizontalAlign="Left" />
                                            <asp:BoundField DataField="Reason_Name" HeaderText="<%$ Resources:Strings, Admin_Seller_ColReason %>" ItemStyle-Font-Bold="true" />
                                            <asp:BoundField DataField="Subject" HeaderText="<%$ Resources:Strings, Admin_Seller_ColSubject %>" ItemStyle-HorizontalAlign="Left" />
                                            <asp:TemplateField HeaderText="<%$ Resources:Strings, Admin_Seller_ColStatus %>">
                                                <ItemTemplate>
                                                    <asp:Label ID="lblStatusBadge" runat="server"></asp:Label>
                                                </ItemTemplate>
                                            </asp:TemplateField>
                                            <asp:TemplateField HeaderText="<%$ Resources:Strings, Admin_Brand_ColActions %>" ItemStyle-Width="120px">
                                                <ItemTemplate>
                                                    <asp:LinkButton ID="btnView" runat="server"
                                                        CommandName="ViewDetails"
                                                        CommandArgument='<%# Eval("Id_Ticket") %>'
                                                        CssClass="btn btn-sm btn-warning font-weight-bold px-3">
                                                    <i class="fas fa-folder-open mr-1"></i> <asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Seller_BtnView %>" />
                                                    </asp:LinkButton>
                                                </ItemTemplate>
                                            </asp:TemplateField>
                                        </Columns>
                                    </asp:GridView>
                                </div>
                            </asp:Panel>

                            <asp:Panel ID="pnlReviews" runat="server" Visible="false">
                                <div class="table-responsive">
                                    <asp:GridView ID="gvReviews" runat="server"
                                        AutoGenerateColumns="False"
                                        GridLines="None"
                                        CssClass="table table-custom text-center align-middle"
                                        DataKeyNames="Id_Review"
                                        OnRowCommand="gvReviews_RowCommand"
                                        EmptyDataText="No product reviews found.">
                                        <Columns>
                                            <asp:BoundField DataField="Id_Review" HeaderText="ID" ItemStyle-Width="60px" />
                                            <asp:BoundField DataField="ReviewDate" HeaderText="<%$ Resources:Strings, Admin_Orders_ColDate %>" DataFormatString="{0:yyyy-MM-dd HH:mm}" />
                                            <asp:TemplateField HeaderText="User">
                                                <ItemTemplate>
                                                    <span class="font-weight-bold"><%# Eval("UserName") %> <%# Eval("UserLastName") %></span><br />
                                                    <small class="text-muted"><%# Eval("UserEmail") %></small>
                                                </ItemTemplate>
                                            </asp:TemplateField>
                                            <asp:BoundField DataField="ShirtName" HeaderText="Product" ItemStyle-Font-Bold="true" />
                                            <asp:TemplateField HeaderText="<%$ Resources:Strings, Review_Rating %>">
                                                <ItemTemplate>
                                                    <span style="color: #FFC800;"><i class="fas fa-star"></i><%# Eval("Rating") %>/5</span>
                                                </ItemTemplate>
                                            </asp:TemplateField>
                                            <asp:TemplateField HeaderText="<%$ Resources:Strings, Review_CommentLabel %>" ItemStyle-HorizontalAlign="Left" ItemStyle-Width="250px">
                                                <ItemTemplate>
                                                    <div style="max-height: 80px; overflow-y: auto; font-style: italic; font-size: 0.9rem;">
                                                        "<%# Eval("Comment") %>"
                                                    </div>
                                                </ItemTemplate>
                                            </asp:TemplateField>
                                            <asp:TemplateField HeaderText="<%$ Resources:Strings, Review_OfficialReply %>" ItemStyle-HorizontalAlign="Left" ItemStyle-Width="200px">
                                                <ItemTemplate>
                                                    <div style="max-height: 80px; overflow-y: auto; font-size: 0.85rem; color: #17a2b8;">
                                                        <%# string.IsNullOrEmpty(Eval("ReplyComment")?.ToString()) ? "<span class='text-muted'>" + GetGlobalResourceObject("Strings", "Admin_Seller_ReviewNoReply") + "</span>" : Eval("ReplyComment") %>
                                                    </div>
                                                </ItemTemplate>
                                            </asp:TemplateField>
                                            <asp:TemplateField HeaderText="<%$ Resources:Strings, Admin_Brand_ColActions %>" ItemStyle-Width="180px">
                                                <ItemTemplate>
                                                    <asp:LinkButton ID="btnReplyReview" runat="server"
                                                        CommandName="ReplyReview"
                                                        CommandArgument='<%# Eval("Id_Review") %>'
                                                        CssClass="btn btn-sm btn-info font-weight-bold mb-1">
                                                        <i class="fas fa-reply"></i> <asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Seller_ReviewReplyBtn %>" />
                                                    </asp:LinkButton>
                                                    <asp:LinkButton ID="btnDeleteReview" runat="server"
                                                        CommandName="DeleteReview"
                                                        CommandArgument='<%# Eval("Id_Review") %>'
                                                        OnClientClick='<%# "return confirm(\"" + GetGlobalResourceObject("Strings", "Confirm_DeleteReview") + "\");" %>'
                                                        CssClass="btn btn-sm btn-danger font-weight-bold mb-1">
                                                        <i class="fas fa-trash-alt"></i> <asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Seller_ReviewDeleteBtn %>" />
                                                    </asp:LinkButton>
                                                </ItemTemplate>
                                            </asp:TemplateField>
                                        </Columns>
                                    </asp:GridView>
                                </div>
                            </asp:Panel>
                        </div>
                    </main>
                </div>

                <!-- DETAIL BOOTSTRAP MODAL WRAPPED IN UPDATEPANEL -->
                <asp:PlaceHolder ID="phDetailModal" runat="server" Visible="false">
                    <div class="modal-backdrop-custom">
                        <div class="modal-dialog-custom">
                            <div class="modal-header bg-dark text-white px-4 py-3 d-flex justify-content-between align-items-center">
                                <h5 class="modal-title font-weight-bold" style="color: #FFC800;">
                                    <i class="fas fa-ticket-alt mr-2"></i>
                                    <asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Seller_ModalResolution %>" />
                                    #<asp:Literal ID="litModalTicketId" runat="server" />
                                </h5>
                                <asp:LinkButton ID="btnCloseX" runat="server" OnClick="btnCloseModal_Click" CssClass="text-white text-decoration-none" Style="font-size: 1.3rem;">&times;</asp:LinkButton>
                            </div>
                            <div class="modal-body p-4" style="color: var(--text-main) !important; background-color: var(--bg-card) !important; max-height: 70vh; overflow-y: auto;">

                                <!-- General customer / subject information -->
                                <div class="row mb-3 border-bottom pb-2" style="border-color: var(--border-color) !important;">
                                    <div class="col-6">
                                        <small class="text-muted d-block text-uppercase font-weight-bold">
                                            <asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Seller_ColEmail %>" /></small>
                                        <span class="font-weight-bold" style="color: var(--text-main) !important;">
                                            <asp:Literal ID="litModalUserEmail" runat="server" /></span>
                                    </div>
                                    <div class="col-6 text-right">
                                        <small class="text-muted d-block text-uppercase font-weight-bold">
                                            <asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Seller_ColDateReq %>" /></small>
                                        <span style="color: var(--text-main) !important;">
                                            <asp:Literal ID="litModalCreatedAt" runat="server" /></span>
                                    </div>
                                </div>

                                <div class="mb-3">
                                    <small class="text-muted d-block text-uppercase font-weight-bold">
                                        <asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Seller_ColSubject %>" /></small>
                                    <h6 class="font-weight-bold" style="font-size: 1.1rem; color: var(--text-main) !important;">
                                        <asp:Literal ID="litModalSubject" runat="server" /></h6>
                                </div>

                                <div class="mb-3 p-3 rounded" style="background: var(--bg-panel); border: 1px solid var(--border-color) !important; color: var(--text-main) !important;">
                                    <small class="text-muted d-block text-uppercase font-weight-bold mb-1">
                                        <asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Seller_ModalMessage %>" /></small>
                                    <p class="mb-0" style="white-space: pre-wrap; font-size: 0.95rem; font-style: italic; color: var(--text-main) !important;">
                                        <asp:Literal ID="litModalMessage" runat="server" /></p>
                                </div>

                                <!-- Dynamic conditional content: Order Issues -->
                                <asp:Panel ID="pnlModalOrder" runat="server" Visible="false" Style="background: var(--bg-panel); border: 1px solid var(--border-color) !important; color: var(--text-main) !important;" class="mb-3 p-3 rounded">
                                    <h6 class="text-info font-weight-bold mb-2"><i class="fas fa-shopping-bag mr-2"></i>
                                        <asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Seller_ModalLinkedOrder %>" /></h6>
                                    <p class="mb-0" style="color: var(--text-main) !important;">
                                        <strong>
                                            <asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Orders_ColOrderId %>" />:</strong>
                                        <span class="badge bg-info text-white font-weight-bold px-2 py-1" style="font-size: 0.95rem;">
                                            <asp:Literal ID="litModalOrderId" runat="server" />
                                        </span>
                                        <a href='ManageOrders.aspx?id=<%= litModalOrderId.Text %>' class="btn btn-sm btn-outline-info ml-2 font-weight-bold" target="_blank">
                                            <i class="fas fa-external-link-alt"></i>
                                            <asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Seller_ModalNavOrder %>" />
                                        </a>
                                    </p>
                                </asp:Panel>

                                <!-- Dynamic conditional content: Seller Request / Reventa -->
                                <asp:Panel ID="pnlModalSeller" runat="server" Visible="false" Style="background: var(--bg-panel); border: 1px solid var(--border-color) !important; color: var(--text-main) !important;" class="mb-3 p-3 rounded">
                                    <h6 class="text-warning font-weight-bold mb-3"><i class="fas fa-hand-holding-usd mr-2"></i>
                                        <asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Seller_ModalConsignment %>" /></h6>

                                    <div class="row mb-3" style="color: var(--text-main) !important;">
                                        <div class="col-md-6">
                                            <strong>
                                                <asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Seller_ModalPrice %>" /></strong>
                                            <span class="font-weight-bold text-success" style="font-size: 1.05rem;">$<asp:Literal ID="litModalProposedPrice" runat="server" />
                                            </span>
                                        </div>
                                        <div class="col-md-6">
                                            <strong>
                                                <asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Seller_ModalCondition %>" /></strong>
                                            <span class="badge bg-warning text-dark font-weight-bold px-2 py-1">
                                                <asp:Literal ID="litModalItemCondition" runat="server" />
                                            </span>
                                        </div>
                                    </div>

                                    <!-- Gallery with zoom-on-hover effect -->
                                    <small class="text-muted d-block text-uppercase font-weight-bold mb-2">
                                        <asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Seller_ModalProofImages %>" /></small>
                                    <div class="row mb-4">
                                        <div class="col-md-4 text-center">
                                            <asp:Image ID="imgModal1" runat="server" CssClass="img-fluid rounded border zoom-effect" Style="height: 140px; object-fit: cover; width: 100%; cursor: pointer;" onclick="openFullscreenImage(this);" />
                                            <button type="button" class="btn btn-sm btn-outline-warning mt-2 w-100 font-weight-bold" onclick="openFullscreenImage(this.previousElementSibling);">
                                                <i class="fas fa-expand-arrows-alt"></i>Fullscreen
                                            </button>
                                        </div>
                                        <div class="col-md-4 text-center">
                                            <asp:Image ID="imgModal2" runat="server" CssClass="img-fluid rounded border zoom-effect" Style="height: 140px; object-fit: cover; width: 100%; cursor: pointer;" onclick="openFullscreenImage(this);" />
                                            <button type="button" class="btn btn-sm btn-outline-warning mt-2 w-100 font-weight-bold" onclick="openFullscreenImage(this.previousElementSibling);">
                                                <i class="fas fa-expand-arrows-alt"></i>Fullscreen
                                            </button>
                                        </div>
                                        <div class="col-md-4 text-center">
                                            <asp:Image ID="imgModal3" runat="server" CssClass="img-fluid rounded border zoom-effect" Style="height: 140px; object-fit: cover; width: 100%; cursor: pointer;" onclick="openFullscreenImage(this);" />
                                            <button type="button" class="btn btn-sm btn-outline-warning mt-2 w-100 font-weight-bold" onclick="openFullscreenImage(this.previousElementSibling);">
                                                <i class="fas fa-expand-arrows-alt"></i>Fullscreen
                                            </button>
                                        </div>
                                    </div>

                                    <!-- Catalog mapping configuration options -->
                                    <asp:Panel ID="pnlModalCatalogMapping" runat="server" Style="background: var(--bg-card); border: 1px solid var(--border-color) !important; color: var(--text-main) !important;" class="mt-3 p-3 rounded">
                                        <h6 class="text-success font-weight-bold mb-3"><i class="fas fa-tags mr-2"></i>
                                            <asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Seller_ModalCatalogParams %>" /></h6>
                                        <div class="form-group">
                                            <label class="small text-muted font-weight-bold mb-1">
                                                <asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Seller_ModalDisplayName %>" /></label>
                                            <asp:TextBox ID="txtNewProductName" runat="server" CssClass="form-control form-control-sm font-weight-bold" placeholder="e.g. 1998 France Cup Final Jersey" />
                                        </div>
                                        <div class="row">
                                            <div class="col-md-3 form-group">
                                                <label class="small text-muted font-weight-bold mb-1">
                                                    <asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Seller_ModalBrandMap %>" /></label>
                                                <asp:DropDownList ID="ddlBrand" runat="server" CssClass="form-control form-control-sm" />
                                            </div>
                                            <div class="col-md-3 form-group">
                                                <label class="small text-muted font-weight-bold mb-1">
                                                    <asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Seller_ModalLeagueMap %>" /></label>
                                                <asp:DropDownList ID="ddlLeague" runat="server" CssClass="form-control form-control-sm" AutoPostBack="true" OnSelectedIndexChanged="ddlLeague_SelectedIndexChanged" />
                                            </div>
                                            <div class="col-md-3 form-group">
                                                <label class="small text-muted font-weight-bold mb-1">
                                                    <asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Seller_ModalTeamMap %>" /></label>
                                                <asp:DropDownList ID="ddlTeam" runat="server" CssClass="form-control form-control-sm" />
                                            </div>
                                            <div class="col-md-3 form-group">
                                                <label class="small text-muted font-weight-bold mb-1">
                                                    <asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Seller_ModalYear %>" /></label>
                                                <asp:TextBox ID="txtYear" runat="server" CssClass="form-control form-control-sm" MaxLength="4" placeholder="e.g. 1998" />
                                            </div>
                                        </div>
                                    </asp:Panel>
                                </asp:Panel>

                                <!-- Resolution input: Admin comments -->
                                <div class="form-group mt-3">
                                    <label for="txtAdminNotes" class="font-weight-bold text-uppercase small text-muted">
                                        <asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Seller_ModalAdminResponse %>" /></label>
                                    <asp:TextBox ID="txtAdminNotes" runat="server" TextMode="MultiLine" Rows="4" CssClass="form-control" placeholder="<%$ Resources:Strings, Placeholder_AdminNotes %>" />
                                </div>

                                <asp:Label ID="lblModalError" runat="server" CssClass="alert alert-danger d-block mt-3 font-weight-bold small" Visible="false" />
                            </div>
                            <div class="modal-footer px-4 py-3 d-flex justify-content-between" style="background: var(--bg-card) !important; border-top: 1px solid var(--border-color) !important;">
                                <asp:Button ID="btnCancel" runat="server" Text="Close Details" CssClass="btn btn-secondary font-weight-bold" OnClick="btnCloseModal_Click" />
                                <div>
                                    <asp:Button ID="btnReject" runat="server" Text="Reject & Deny Request" CssClass="btn btn-danger font-weight-bold mr-2" OnClick="btnReject_Click" />
                                    <asp:Button ID="btnApprove" runat="server" Text="Approve & Publish Catalog" CssClass="btn btn-success font-weight-bold" OnClick="btnApprove_Click" />
                                </div>
                            </div>
                        </div>
                    </div>
                </asp:PlaceHolder>

                <!-- REPLY BOOTSTRAP MODAL WRAPPED IN PLACEHOLDER -->
                <asp:PlaceHolder ID="phReplyModal" runat="server" Visible="false">
                    <div class="modal-backdrop-custom">
                        <div class="modal-dialog-custom">
                            <div class="modal-header bg-dark text-white px-4 py-3 d-flex justify-content-between align-items-center">
                                <h5 class="modal-title font-weight-bold" style="color: #FFC800;">
                                    <i class="fas fa-reply mr-2"></i>
                                    <asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Seller_ReviewModalTitle %>" />
                                    #<asp:Literal ID="litReplyReviewId" runat="server" />
                                </h5>
                                <asp:LinkButton ID="btnCloseReplyModal" runat="server" OnClick="btnCloseReplyModal_Click" CssClass="text-white text-decoration-none" Style="font-size: 1.3rem;">&times;</asp:LinkButton>
                            </div>
                            <div class="modal-body p-4" style="color: var(--text-main) !important; background-color: var(--bg-card) !important;">
                                <div class="mb-3 p-3 rounded" style="background: var(--bg-panel); border: 1px solid var(--border-color) !important;">
                                    <small class="text-muted d-block text-uppercase font-weight-bold mb-1">
                                        <asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Seller_ReviewModalOriginal %>" /></small>
                                    <p class="mb-0 font-italic" style="color: var(--text-main) !important;">
                                        <asp:Literal ID="litOriginalReview" runat="server" /></p>
                                </div>

                                <div class="form-group mt-3">
                                    <label for="txtReplyComment" class="font-weight-bold text-uppercase small text-muted">
                                        <asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Seller_ReviewModalOfficial %>" /></label>
                                    <asp:TextBox ID="txtReplyComment" runat="server" TextMode="MultiLine" Rows="4" CssClass="form-control" placeholder="<%$ Resources:Strings, Placeholder_PublicReply %>" />
                                </div>
                                <asp:Label ID="lblReplyModalError" runat="server" CssClass="alert alert-danger d-block mt-3 font-weight-bold small" Visible="false" />
                            </div>
                            <div class="modal-footer px-4 py-3 d-flex justify-content-between" style="background: var(--bg-card) !important; border-top: 1px solid var(--border-color) !important;">
                                <asp:Button ID="btnCancelReply" runat="server" Text="<%$ Resources:Strings, Account_Cancel %>" CssClass="btn btn-secondary font-weight-bold" OnClick="btnCloseReplyModal_Click" />
                                <asp:Button ID="btnSubmitReply" runat="server" Text="<%$ Resources:Strings, Admin_Seller_ReviewSubmitReply %>" CssClass="btn btn-info font-weight-bold" OnClick="btnSubmitReply_Click" />
                            </div>
                        </div>
                    </div>
                </asp:PlaceHolder>
            </ContentTemplate>
        </asp:UpdatePanel>

        <!-- Fullscreen Image Overlay -->
        <div id="fullscreenOverlay" style="display: none; position: fixed; top: 0; left: 0; width: 100%; height: 100%; background: rgba(0,0,0,0.9); z-index: 99999; align-items: center; justify-content: center;" onclick="closeFullscreenImage()">
            <span style="position: absolute; top: 20px; right: 30px; color: #fff; font-size: 40px; font-weight: bold; cursor: pointer;">&times;</span>
            <img id="fullscreenOverlayImg" style="max-width: 90%; max-height: 90%; object-fit: contain; border: 2px solid #ffc800; border-radius: 8px;" />
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
