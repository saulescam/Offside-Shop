<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ManageProducts.aspx.cs" Inherits="OFFSIDESHOP.ManageProducts" Async="true" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1, shrink-to-fit=no" />
    <title><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Products_Title %>" /> | OffsideShop</title>

    <!-- CSS -->
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

    <script src="SweetAlert/sweetalert2.all.min.js"></script>

    <style>
        /* Status badges */
        .status-badge {
            padding: 3px 12px;
            border-radius: 20px;
            font-size: 0.75rem;
            font-weight: 700;
            letter-spacing: 0.5px;
        }

        .status-active { background: #1a7a4a; color: #a8f0c6; }
        .status-inactive { background: #5c2323; color: #f0a8a8; }

        /* Filters block */
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

        /* Action form panel */
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
            from { opacity: 0; transform: translateY(-18px); }
            to { opacity: 1; transform: translateY(0); }
        }

        .form-panel h4 {
            font-weight: 700;
            margin-bottom: 20px;
            background: var(--gradient-blue);
            -webkit-background-clip: text;
            -webkit-text-fill-color: transparent;
        }

        /* Sub-tabs for Language Selection */
        .sub-nav-tabs { border-bottom: 1px solid #444; margin-bottom: 15px; }
        .sub-nav-tabs .nav-link { font-size: 0.85rem; padding: 6px 15px; color: #aaa; border: 1px solid transparent; border-radius: 6px 6px 0 0; cursor: pointer; }
        .sub-nav-tabs .nav-link.active { color: #FFC800 !important; background: rgba(255, 200, 0, 0.1); border-color: #444 #444 transparent #444; }

        .tab-content { padding-top: 10px; }
        .tab-pane { display: none; }
        .tab-pane.active { display: block; }

        /* Translate Button */
        .btn-translate { background: #334155; color: #FFC800; border: 1px solid #FFC800; border-radius: 6px; font-size: 0.78rem; font-weight: 600; padding: 4px 10px; transition: all 0.2s ease; cursor: pointer; }
        .btn-translate:hover { background: #FFC800; color: #000; }

        /* Action Buttons */
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
        .btn-edit { background: #1e3a8a; color: #93c5fd; }
        .btn-toggle { background: #374151; color: #d1d5db; }
        .btn-delete { background: #7f1d1d; color: #fca5a5; }
        .btn-action:hover { opacity: 0.85; transform: scale(1.08); }

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
            text-decoration: none !important;
            display: inline-block;
        }
        .btn-save:hover {
            transform: translateY(-2px);
            box-shadow: 0 5px 16px rgba(37,99,235,0.4);
            color: #fff;
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
            text-decoration: none !important;
            display: inline-block;
        }
        .btn-cancel-form:hover { background: #2a2a2a; color: #ccc; }

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

        /* Checkbox Dorado */
        .gold-checkbox {
            position: relative;
            display: flex;
            align-items: center;
            cursor: pointer;
            user-select: none;
            color: var(--text-muted);
            font-weight: 600;
        }
        .gold-checkbox input[type="checkbox"] { position: absolute; opacity: 0; cursor: pointer; height: 0; width: 0; }
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
        .gold-checkbox:hover input ~ .checkmark { border-color: #d4af37; }
        .gold-checkbox input:checked ~ .checkmark { background-color: #d4af37; border-color: #d4af37; }
        .checkmark:after {
            content: ""; position: absolute; display: none; left: 6px; top: 2px;
            width: 5px; height: 10px; border: solid white; border-width: 0 2px 2px 0; transform: rotate(45deg);
        }
        .gold-checkbox input:checked ~ .checkmark:after { display: block; }

        /* Drag and Drop Zone */
        .drop-zone {
            border: 2px dashed var(--border-color, #ccc);
            border-radius: 10px;
            padding: 25px 15px;
            text-align: center;
            background: rgba(0, 0, 0, 0.05);
            cursor: pointer;
            transition: all 0.3s ease;
            position: relative;
            overflow: hidden;
        }
        html.dark-mode .drop-zone { background: rgba(255, 255, 255, 0.02); border-color: #444; }
        .drop-zone:hover, .drop-zone.dragover {
            border-color: #FFC800;
            background: rgba(255, 200, 0, 0.05);
        }
        .drop-zone i {
            font-size: 2.2rem;
            color: #FFC800;
            margin-bottom: 10px;
            display: block;
        }
        .drop-zone p {
            margin: 0;
            color: var(--text-muted);
            font-weight: 600;
            font-size: 0.85rem;
            pointer-events: none;
        }
        .drop-zone input[type="file"] {
            position: absolute;
            top: 0; left: 0; width: 100%; height: 100%;
            opacity: 0; cursor: pointer;
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
    </style>
</head>
<body>
    <form id="form1" runat="server" enctype="multipart/form-data">
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

        <div class="layout-wrapper">

            <!-- SIDEBAR -->
            <aside class="sidebar fade-in">
                <ul class="sidebar-menu">
                    <li>
                        <asp:LinkButton ID="btnManageProducts" CssClass="sidebar-btn active" runat="server" OnClick="btnManageProducts_Click" Style="font-family: 'Raleway', 'Font Awesome 5 Free'; font-weight: 600;">
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

            <!-- MAIN CONTENT -->
            <main class="main-content fade-in" style="animation-delay: 0.15s;">
                <div class="container-fluid">

                    <h1 class="page-title"><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Nav_ManageProducts %>" /></h1>
                    <p class="text-muted mb-4"><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Products_SubTitle %>" /></p>

                    <!-- ACTION FORM PANEL -->
                    <asp:Panel ID="pnlProductForm" runat="server" Visible="false" CssClass="form-panel">
                        <asp:HiddenField ID="hfSelectedProductId" runat="server" Value="" />
                        <h4><asp:Label ID="lblFormTitle" runat="server" Text="<%$ Resources:Strings, Admin_Products_AddNew %>"></asp:Label></h4>

                        <!-- SUB-TABS IDIOMAS PARA PRODUCTO -->
                        <ul class="nav nav-tabs sub-nav-tabs" role="tablist">
                            <li class="nav-item">
                                <a class="nav-link active" id="prod-en-tab" data-toggle="tab" href="#prod-en" role="tab"><i class="fas fa-globe-americas me-1"></i> <asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Products_EnglishTab %>" /></a>
                            </li>
                            <li class="nav-item">
                                <a class="nav-link" id="prod-es-tab" data-toggle="tab" href="#prod-es" role="tab"><i class="fas fa-globe-americas me-1"></i> <asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Products_SpanishTab %>" /></a>
                            </li>
                        </ul>

                        <div class="tab-content border-bottom border-secondary pb-3 mb-3">
                            <!-- ENGLISH TRANSLATABLE FIELDS -->
                            <div class="tab-pane active" id="prod-en" role="tabpanel">
                                <div class="d-flex justify-content-between align-items-center mb-2">
                                    <small class="text-warning font-weight-bold"><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Products_PrimaryEnglishInfo %>" /></small>
                                    <button type="button" class="btn-translate" onclick="autoTranslate('en', 'es', ['txtName', 'txtDescription'], ['txtName_ES', 'txtDescription_ES'])">
                                        <i class="fas fa-language me-1"></i> <asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Banners_TranslateToEs %>" />
                                    </button>
                                </div>
                                <div class="form-group mb-2">
                                    <label>Shirt Name (EN) <span class="text-danger">*</span></label>
                                    <asp:TextBox ID="txtName" ClientIDMode="Static" runat="server" CssClass="form-control" placeholder="e.g. FC Barcelona Home 2024" MaxLength="200"></asp:TextBox>
                                </div>
                                <div class="form-group mb-0">
                                    <div class="d-flex justify-content-between align-items-center mb-1">
                                        <label class="mb-0">Description (EN) <small class="text-muted">(optional)</small></label>
                                        <!-- BOTÓN DE IA CLIENTE (SIN POSTBACK) -->
                                        <button type="button" id="btnAiGen" class="btn btn-sm"
                                            style="background: linear-gradient(135deg, #FFC800, #d97706); color: #fff; font-weight: 700; border: none; border-radius: 6px; padding: 2px 10px; font-size: 0.78rem;"
                                            onclick="generateAiDescription(); return false;">
                                            <i class="fas fa-magic mr-1"></i> Generate with AI
                                        </button>
                                    </div>
                                    <asp:TextBox ID="txtDescription" ClientIDMode="Static" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="3" placeholder="Short product description in English..."></asp:TextBox>
                                </div>
                            </div>

                            <!-- SPANISH TRANSLATABLE FIELDS -->
                            <div class="tab-pane" id="prod-es" role="tabpanel">
                                <div class="d-flex justify-content-between align-items-center mb-2">
                                    <small class="text-warning font-weight-bold"><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Products_SpanishInfo %>" /></small>
                                    <button type="button" class="btn-translate" onclick="autoTranslate('es', 'en', ['txtName_ES', 'txtDescription_ES'], ['txtName', 'txtDescription'])">
                                        <i class="fas fa-language me-1"></i> <asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Banners_TranslateToEn %>" />
                                    </button>
                                </div>
                                <div class="form-group mb-2">
                                    <label>Nombre de Camiseta (ES) <span class="text-danger">*</span></label>
                                    <asp:TextBox ID="txtName_ES" ClientIDMode="Static" runat="server" CssClass="form-control" placeholder="ej. Camiseta Local FC Barcelona 2024" MaxLength="200"></asp:TextBox>
                                </div>
                                <div class="form-group mb-0">
                                    <label>Descripción (ES) <small class="text-muted">(opcional)</small></label>
                                    <asp:TextBox ID="txtDescription_ES" ClientIDMode="Static" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="3" placeholder="Descripción en español..."></asp:TextBox>
                                </div>
                            </div>
                        </div>

                        <!-- UNIVERSAL FIELDS -->
                        <h6 class="text-muted mb-3 font-weight-bold"><i class="fas fa-cogs me-1"></i> <asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Products_UniversalSpecs %>" /></h6>

                        <!-- Row 1: Price & Year -->
                        <div class="row">
                            <div class="col-md-6">
                                <div class="form-group">
                                    <label>Price (USD) <span class="text-danger">*</span></label>
                                    <asp:TextBox ID="txtPrice" ClientIDMode="Static" runat="server" CssClass="form-control" placeholder="e.g. 89.99" MaxLength="10" onkeypress="return validarPrecio(event, this)" onpaste="return false"></asp:TextBox>
                                </div>
                            </div>
                            <div class="col-md-6">
                                <div class="form-group">
                                    <label><asp:Literal runat="server" Text="<%$ Resources:Strings, Detail_Year %>" /> <span class="text-danger">*</span></label>
                                    <asp:TextBox ID="txtYear" ClientIDMode="Static" runat="server" CssClass="form-control" placeholder="e.g. 2024" MaxLength="4" onkeypress="return validarAnio(event)" onpaste="return false"></asp:TextBox>
                                </div>
                            </div>
                        </div>

                        <!-- Row 2: Brand / League / Team / Kit Type (EN UPDATEPANEL SEPARADO PARA EVITAR POSTBACK GLOBAL) -->
                        <asp:UpdatePanel ID="upLeagueTeam" runat="server" UpdateMode="Conditional">
                            <ContentTemplate>
                                <div class="row">
                                    <div class="col-md-3 col-sm-6">
                                        <div class="form-group">
                                            <label><asp:Literal runat="server" Text="<%$ Resources:Strings, Search_BrandLabel %>" /> <span class="text-danger">*</span></label>
                                            <asp:DropDownList ID="ddlFormBrand" ClientIDMode="Static" runat="server" CssClass="form-control"></asp:DropDownList>
                                        </div>
                                    </div>
                                    <div class="col-md-3 col-sm-6">
                                        <div class="form-group">
                                            <label><asp:Literal runat="server" Text="<%$ Resources:Strings, Search_LeagueLabel %>" /> <span class="text-danger">*</span></label>
                                            <asp:DropDownList ID="ddlFormLeague" ClientIDMode="Static" runat="server" CssClass="form-control" AutoPostBack="true" OnSelectedIndexChanged="ddlFormLeague_SelectedIndexChanged"></asp:DropDownList>
                                        </div>
                                    </div>
                                    <div class="col-md-3 col-sm-6">
                                        <div class="form-group">
                                            <label><asp:Literal runat="server" Text="<%$ Resources:Strings, Sidebar_Team %>" /> <span class="text-danger">*</span></label>
                                            <asp:DropDownList ID="ddlFormTeam" ClientIDMode="Static" runat="server" CssClass="form-control"></asp:DropDownList>
                                        </div>
                                    </div>
                                    <div class="col-md-3 col-sm-6">
                                        <div class="form-group">
                                            <label><asp:Literal runat="server" Text="<%$ Resources:Strings, Search_KitLabel %>" /> <span class="text-danger">*</span></label>
                                            <asp:DropDownList ID="ddlFormKitType" ClientIDMode="Static" runat="server" CssClass="form-control"></asp:DropDownList>
                                        </div>
                                    </div>
                                </div>
                            </ContentTemplate>
                        </asp:UpdatePanel>

                        <!-- Row 3: Stock per Size -->
                        <div class="row">
                            <div class="col-12">
                                <label class="d-block mb-2" style="color: var(--text-muted); font-weight: 600; font-size: 0.85rem; text-transform: uppercase;"><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Products_StockPerSize %>" /> <small class="text-muted">(<asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Products_StockPerSizeHelp %>" />)</small></label>
                            </div>
                            <div class="col"><div class="form-group"><label style="font-size: 0.8rem; font-weight: 600;">S</label><asp:TextBox ID="txtStockS" runat="server" Text="0" CssClass="form-control text-center" onkeypress="return validarStock(event)" onpaste="return false" MaxLength="5"></asp:TextBox></div></div>
                            <div class="col"><div class="form-group"><label style="font-size: 0.8rem; font-weight: 600;">M</label><asp:TextBox ID="txtStockM" runat="server" Text="0" CssClass="form-control text-center" onkeypress="return validarStock(event)" onpaste="return false" MaxLength="5"></asp:TextBox></div></div>
                            <div class="col"><div class="form-group"><label style="font-size: 0.8rem; font-weight: 600;">L</label><asp:TextBox ID="txtStockL" runat="server" Text="0" CssClass="form-control text-center" onkeypress="return validarStock(event)" onpaste="return false" MaxLength="5"></asp:TextBox></div></div>
                            <div class="col"><div class="form-group"><label style="font-size: 0.8rem; font-weight: 600;">XL</label><asp:TextBox ID="txtStockXL" runat="server" Text="0" CssClass="form-control text-center" onkeypress="return validarStock(event)" onpaste="return false" MaxLength="5"></asp:TextBox></div></div>
                            <div class="col"><div class="form-group"><label style="font-size: 0.8rem; font-weight: 600;">XXL</label><asp:TextBox ID="txtStockXXL" runat="server" Text="0" CssClass="form-control text-center" onkeypress="return validarStock(event)" onpaste="return false" MaxLength="5"></asp:TextBox></div></div>
                        </div>

                        <!-- Row 4: Image Upload (DRAG & DROP) CON PREVISUALIZACIÓN MÚLTIPLE -->
                        <div class="row">
                            <div class="col-md-6">
                                <div class="form-group">
                                    <label>Product Image <small class="text-muted">(.jpg / .png / .webp, max 2 MB)</small></label>
                                    <div class="drop-zone" id="dzMainImage">
                                        <i class="fas fa-cloud-upload-alt"></i>
                                        <p id="lblMainImageText"><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Products_MainImageDrag %>" /></p>
                                        <asp:FileUpload ID="fileImagen" ClientIDMode="Static" runat="server" accept="image/png, image/jpeg, image/jpg, image/webp" />
                                    </div>
                                    <asp:Label ID="lblCurrentImage" runat="server" CssClass="text-muted d-block mt-1"></asp:Label>
                                    <img id="imgPreview" src="#" alt="Image Preview" style="display: none; max-width: 100%; max-height: 180px; margin-top: 10px; border-radius: 8px; object-fit: contain;" />
                                </div>
                            </div>
                            <div class="col-md-6">
                                <div class="form-group">
                                    <label>Gallery Images <small class="text-muted">(.jpg / .png / .webp, max 2 MB)</small></label>
                                    <div class="drop-zone" id="dzGalleryImages">
                                        <i class="fas fa-images"></i>
                                        <p id="lblGalleryImagesText"><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Products_GalleryImageDrag %>" /></p>
                                        <asp:FileUpload ID="fuExtraImages" ClientIDMode="Static" runat="server" AllowMultiple="true" accept="image/png, image/jpeg, image/jpg, image/webp" />
                                    </div>
                                    <span class="text-muted" style="font-size: 0.8rem; display: block; margin-top: 4px;"><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Products_GalleryImageHelp %>" /></span>
                                    <asp:Label ID="lblCurrentExtraImages" runat="server" CssClass="text-muted d-block mt-1"></asp:Label>

                                    <!-- CONTENEDOR PARA PREVISUALIZAR IMÁGENES DE GALERÍA -->
                                    <div id="extraImagesPreview" class="d-flex flex-wrap justify-content-center gap-2 mt-2"></div>
                                </div>
                            </div>
                        </div>

                        <!-- Row 5: Customization Checkbox -->
                        <div class="row mt-2">
                            <div class="col-12 d-flex align-items-center">
                                <div class="form-group mb-0">
                                    <label class="gold-checkbox">
                                        <asp:CheckBox ID="chkIsCustomizable" runat="server" ClientIDMode="Static" />
                                        <span class="checkmark"></span>
                                        <asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Products_AllowCustom %>" />
                                    </label>
                                </div>
                            </div>
                        </div>

                        <!-- Form Action Buttons -->
                        <div class="row mt-4">
                            <div class="col-12 d-flex justify-content-start" style="gap: 10px;">
                                <asp:LinkButton ID="btnSaveProduct" runat="server" CssClass="btn-save" OnClick="btnSaveProduct_Click" OnClientClick="return validateProductForm();" Style="font-family: 'Raleway','Font Awesome 5 Free'; font-weight: 700;">
                                    &#xf0c7;&nbsp; <asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Products_Save %>" />
                                </asp:LinkButton>
                                <asp:LinkButton ID="btnCancelForm" runat="server" CssClass="btn-cancel-form" OnClick="btnCancelForm_Click" CausesValidation="false" Style="font-family: 'Raleway','Font Awesome 5 Free'; font-weight: 600;">
                                    &#xf00d;&nbsp; <asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Products_Cancel %>" />
                                </asp:LinkButton>
                            </div>
                        </div>
                    </asp:Panel>

                    <!-- FILTERS BLOCK -->
                    <div class="filter-card">
                        <div class="row align-items-end">
                            <div class="col-md-2 col-sm-6 mb-2 mb-md-0">
                                <label><asp:Literal runat="server" Text="<%$ Resources:Strings, Search_BrandLabel %>" /></label>
                                <asp:DropDownList ID="ddlFilterBrand" runat="server" CssClass="form-control" AutoPostBack="true" OnSelectedIndexChanged="Filters_Changed"></asp:DropDownList>
                            </div>
                            <div class="col-md-2 col-sm-6 mb-2 mb-md-0">
                                <label><asp:Literal runat="server" Text="<%$ Resources:Strings, Search_LeagueLabel %>" /></label>
                                <asp:DropDownList ID="ddlFilterLeague" runat="server" CssClass="form-control" AutoPostBack="true" OnSelectedIndexChanged="ddlFilterLeague_SelectedIndexChanged"></asp:DropDownList>
                            </div>
                            <div class="col-md-2 col-sm-6 mb-2 mb-md-0">
                                <label><asp:Literal runat="server" Text="<%$ Resources:Strings, Sidebar_Team %>" /></label>
                                <asp:DropDownList ID="ddlFilterTeam" runat="server" CssClass="form-control" AutoPostBack="true" OnSelectedIndexChanged="Filters_Changed"></asp:DropDownList>
                            </div>
                            <div class="col-md-2 col-sm-6 mb-2 mb-md-0">
                                <label><asp:Literal runat="server" Text="<%$ Resources:Strings, Search_KitLabel %>" /></label>
                                <asp:DropDownList ID="ddlFilterKitType" runat="server" CssClass="form-control" AutoPostBack="true" OnSelectedIndexChanged="Filters_Changed"></asp:DropDownList>
                            </div>
                            <div class="col-md-2 col-sm-6 mb-2 mb-md-0">
                                <label>Stock Level</label>
                                <asp:DropDownList ID="ddlFilterStock" runat="server" CssClass="form-control" AutoPostBack="true" OnSelectedIndexChanged="Filters_Changed">
                                    <asp:ListItem Text="-- All Stock --" Value="0"></asp:ListItem>
                                    <asp:ListItem Text="Low Stock (< 5)" Value="1"></asp:ListItem>
                                </asp:DropDownList>
                            </div>
                            <div class="col-md-3 col-sm-6 mb-2 mb-md-0">
                                <label>Search by Name</label>
                                <asp:TextBox ID="txtSearchName" runat="server" CssClass="form-control" placeholder="<%$ Resources:Strings, Placeholder_SearchShirt %>" AutoPostBack="true" OnTextChanged="Filters_Changed"></asp:TextBox>
                            </div>
                            <div class="col-md-1 col-sm-6 text-right">
                                <asp:LinkButton ID="lbClearFilters" runat="server" CssClass="text-muted" Style="font-size: 0.8rem; text-decoration: underline; display: block; margin-top: 20px;" CausesValidation="false" OnClick="lbClearFilters_Click">Clear</asp:LinkButton>
                            </div>
                        </div>
                    </div>

                    <!-- GRID SECTION HEADER -->
                    <div class="section-header">
                        <h3><i class="fas fa-tshirt mr-2" style="color: #3b82f6;"></i><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Products_AllShirtsHeader %>" /></h3>
                        <asp:LinkButton ID="lbAddNew" runat="server" CssClass="btn-add-new" CausesValidation="false" OnClick="lbAddNew_Click">
                            <i class="fas fa-plus mr-1"></i> <asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Products_AddNew %>" />
                        </asp:LinkButton>
                    </div>

                    <!-- PRODUCTS GRIDVIEW -->
                    <div class="table-responsive">
                        <asp:GridView ID="gvProducts" runat="server" AutoGenerateColumns="False" GridLines="None" CssClass="table table-custom text-center align-middle" DataKeyNames="ID" AllowPaging="true" PageSize="24" OnRowCommand="gvProducts_RowCommand" OnRowDataBound="gvProducts_RowDataBound" OnPageIndexChanging="gvProducts_PageIndexChanging" EmptyDataText="No shirts found matching the current filters.">
                            <PagerStyle CssClass="pagination-custom" HorizontalAlign="Center" />
                            <Columns>
                                <asp:BoundField DataField="ID" HeaderText="ID" ItemStyle-Width="50px" />
                                <asp:TemplateField HeaderText="Shirt Name (EN)" ItemStyle-HorizontalAlign="Left">
                                    <ItemTemplate>
                                        <asp:PlaceHolder ID="phLowStockBadge" runat="server" Visible='<%# Convert.ToInt32(Eval("TotalStock")) < 5 %>'>
                                            <span class="badge bg-danger text-white me-1" style="font-size: 0.75rem; padding: 2px 6px;" title="Low Stock Alert">
                                                <i class="fas fa-exclamation-triangle"></i> LOW STOCK (<%# Eval("TotalStock") %>)
                                            </span>
                                        </asp:PlaceHolder>
                                        <span class="fw-bold text-white"><%# Eval("Name") %></span>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:BoundField DataField="Name_ES" HeaderText="Nombre (ES)" ItemStyle-HorizontalAlign="Left" NullDisplayText="-" />
                                <asp:BoundField DataField="Price" HeaderText="Price" DataFormatString="{0:N2}" HtmlEncode="false" />
                                <asp:BoundField DataField="Year" HeaderText="Year" />
                                <asp:BoundField DataField="BrandName" HeaderText="Brand" />
                                <asp:BoundField DataField="TeamName" HeaderText="Team" />
                                <asp:BoundField DataField="KitTypeName" HeaderText="Kit Type" />
                                <asp:TemplateField HeaderText="Status">
                                    <ItemTemplate><asp:Label ID="lblStatus" runat="server"></asp:Label></ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Actions" ItemStyle-Width="140px">
                                    <ItemTemplate>
                                        <asp:Button ID="btnEdit" runat="server" CssClass="btn-action btn-edit" CommandName="EditProduct" CommandArgument='<%# Eval("ID") %>' Text="&#xf044;" Style="font-family: 'Font Awesome 5 Free','Raleway'; font-weight: 900;" />
                                        <asp:Button ID="btnToggle" runat="server" CssClass="btn-action btn-toggle" CommandName="ToggleStatus" CommandArgument='<%# Eval("ID") %>' Text="&#xf06e;" Style="font-family: 'Font Awesome 5 Free','Raleway'; font-weight: 900;" />
                                        <asp:Button ID="btnDelete" runat="server" CssClass="btn-action btn-delete" CommandName="PermanentDelete" CommandArgument='<%# Eval("ID") %>' OnClientClick='<%# "return confirm(\"" + GetGlobalResourceObject("Strings", "Confirm_DeleteProduct") + "\");" %>' Text="&#xf2ed;" Style="font-family: 'Font Awesome 5 Free','Raleway'; font-weight: 900;" />
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                        </asp:GridView>
                    </div>

                    <asp:Literal ID="alerta" runat="server" Text="" EnableViewState="false"></asp:Literal>

                </div>
            </main>
        </div>
    </form>

    <script src="https://ajax.googleapis.com/ajax/libs/jquery/3.4.1/jquery.min.js"></script>
    <script src="https://cdnjs.cloudflare.com/ajax/libs/popper.js/1.14.7/umd/popper.min.js"></script>
    <script src="https://maxcdn.bootstrapcdn.com/bootstrap/4.3.1/js/bootstrap.min.js"></script>
    <script src="/SweetAlert/sweetalert2.all.min.js"></script>

    <script type="text/javascript">
        $(document).ready(function () {
            $('.sub-nav-tabs a').click(function (e) {
                e.preventDefault();
                $(this).tab('show');
            });
        });

        // ================= TRADUCCIÓN AUTOMÁTICA =================
        function autoTranslate(sourceLang, targetLang, sourceInputIds, targetInputIds) {
            for (let i = 0; i < sourceInputIds.length; i++) {
                let srcElem = document.getElementById(sourceInputIds[i]);
                let targetElem = document.getElementById(targetInputIds[i]);

                if (!srcElem || !targetElem) continue;

                let text = srcElem.value.trim();
                if (text === '') continue;

                let url = 'https://translate.googleapis.com/translate_a/single?client=gtx&sl=' + sourceLang + '&tl=' + targetLang + '&dt=t&q=' + encodeURIComponent(text);

                fetch(url)
                    .then(function (response) { return response.json(); })
                    .then(function (data) {
                        if (data && data[0]) {
                            let translatedText = data[0].map(function (item) { return item[0]; }).join('');
                            targetElem.value = translatedText;

                            const Toast = Swal.mixin({
                                toast: true,
                                position: 'top-end',
                                showConfirmButton: false,
                                timer: 2000
                            });
                            Toast.fire({ icon: 'success', title: 'Translated successfully!' });
                        }
                    })
                    .catch(function (err) {
                        console.error('Translation error:', err);
                    });
            }
        }

        // ================= GENERACIÓN DE DESCRIPCIÓN CON IA (AJAX SIN POSTBACK) =================
        function generateAiDescription() {
            let name = document.getElementById('txtName').value.trim();
            let brandElem = document.getElementById('ddlFormBrand');
            let teamElem = document.getElementById('ddlFormTeam');
            let year = document.getElementById('txtYear').value.trim();
            let kitElem = document.getElementById('ddlFormKitType');

            let brand = (brandElem && brandElem.selectedIndex > 0) ? brandElem.options[brandElem.selectedIndex].text : '';
            let team = (teamElem && teamElem.selectedIndex > 0) ? teamElem.options[teamElem.selectedIndex].text : '';
            let kitType = (kitElem && kitElem.selectedIndex > 0) ? kitElem.options[kitElem.selectedIndex].text : 'Jersey';

            if (!name || brandElem.value === "0" || teamElem.value === "0") {
                Swal.fire('<%= GetGlobalResourceObject("Strings", "Alert_MissingInfoTitle") %>', 'Please enter at least Name, Brand, and Team before generating the description.', 'warning');
                return;
            }

            Swal.fire({
                title: 'Generating Description...',
                text: 'AI is crafting a compelling product description. Please wait...',
                allowOutsideClick: false,
                didOpen: () => { Swal.showLoading(); }
            });

            $.ajax({
                type: "POST",
                url: "ManageProducts.aspx/GenerateAiDescription",
                data: JSON.stringify({
                    productName: name,
                    brand: brand,
                    team: team,
                    year: year,
                    kitType: kitType
                }),
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: function (response) {
                    Swal.close();
                    if (response.d && response.d.indexOf("ERROR:") === 0) {
                        Swal.fire('AI Error', response.d, 'error');
                    } else if (response.d) {
                        document.getElementById('txtDescription').value = response.d;
                        const Toast = Swal.mixin({
                            toast: true,
                            position: 'top-end',
                            showConfirmButton: false,
                            timer: 2500
                        });
                        Toast.fire({ icon: 'success', title: 'Description generated successfully!' });
                    }
                },
                error: function (xhr, status, error) {
                    Swal.close();
                    Swal.fire('Error', 'Failed to communicate with AI service.', 'error');
                }
            });
        }

        // ================= VALIDACIÓN BILINGÜE Y DE CAMPOS =================
        function validateProductForm() {
            let nameEn = document.getElementById('txtName').value.trim();
            let nameEs = document.getElementById('txtName_ES').value.trim();
            let price = document.getElementById('txtPrice').value.trim();
            let year = document.getElementById('txtYear').value.trim();
            let brand = document.getElementById('ddlFormBrand').value;
            let league = document.getElementById('ddlFormLeague').value;
            let team = document.getElementById('ddlFormTeam').value;
            let kitType = document.getElementById('ddlFormKitType').value;

            if (!nameEn) {
                Swal.fire('<%= GetGlobalResourceObject("Strings", "Alert_MissingInfoTitle") %>', '<%= GetGlobalResourceObject("Strings", "Alert_Products_NameEnRequired") %>', 'warning');
                $('#prod-en-tab').tab('show');
                return false;
            }
            if (!nameEs) {
                Swal.fire('<%= GetGlobalResourceObject("Strings", "Alert_MissingInfoTitle") %>', '<%= GetGlobalResourceObject("Strings", "Alert_Products_NameEsRequired") %>', 'warning');
                $('#prod-es-tab').tab('show');
                return false;
            }
            if (!price || parseFloat(price) <= 0) {
                Swal.fire('<%= GetGlobalResourceObject("Strings", "Alert_ValidationErrorTitle") %>', '<%= GetGlobalResourceObject("Strings", "Alert_Products_PriceRequired") %>', 'warning');
                return false;
            }
            if (!year || year.length !== 4) {
                Swal.fire('<%= GetGlobalResourceObject("Strings", "Alert_ValidationErrorTitle") %>', '<%= GetGlobalResourceObject("Strings", "Alert_Products_YearRequired") %>', 'warning');
                return false;
            }
            if (brand === "0" || !brand) {
                Swal.fire('<%= GetGlobalResourceObject("Strings", "Alert_ValidationErrorTitle") %>', '<%= GetGlobalResourceObject("Strings", "Alert_Products_BrandRequired") %>', 'warning');
                return false;
            }
            if (league === "0" || !league) {
                Swal.fire('<%= GetGlobalResourceObject("Strings", "Alert_ValidationErrorTitle") %>', '<%= GetGlobalResourceObject("Strings", "Alert_Products_LeagueRequired") %>', 'warning');
                return false;
            }
            if (team === "0" || !team) {
                Swal.fire('<%= GetGlobalResourceObject("Strings", "Alert_ValidationErrorTitle") %>', '<%= GetGlobalResourceObject("Strings", "Alert_Products_TeamRequired") %>', 'warning');
                return false;
            }
            if (kitType === "0" || !kitType) {
                Swal.fire('<%= GetGlobalResourceObject("Strings", "Alert_ValidationErrorTitle") %>', '<%= GetGlobalResourceObject("Strings", "Alert_Products_KitTypeRequired") %>', 'warning');
                return false;
            }

            return true;
        }

        // ================= HELPER VALIDATIONS =================
        function validarPrecio(e, field) {
            var key = e.keyCode ? e.keyCode : e.which;
            if (key == 8) return true;
            if (key > 47 && key < 58) {
                if (field.value === "") return true;
                return !(/.[0-9]{2}$/.test(field.value));
            }
            if (key == 46) {
                if (field.value === "") return false;
                if (field.value.indexOf('.') !== -1) return false;
                return /^[0-9]+$/.test(field.value);
            }
            return false;
        }

        function validarAnio(e) {
            var tecla = (document.all) ? e.keyCode : e.which;
            if (tecla == 8) return true;
            var campo = document.getElementById('txtYear');
            if (campo && campo.value.length >= 4) return false;
            return /\d/.test(String.fromCharCode(tecla));
        }

        function validarStock(e) {
            var tecla = (document.all) ? e.keyCode : e.which;
            if (tecla == 8) return true;
            return /\d/.test(String.fromCharCode(tecla));
        }

        function previewImage(input) {
            var preview = document.getElementById('imgPreview');
            if (input.files && input.files[0]) {
                var reader = new FileReader();
                reader.onload = function (e) {
                    preview.src = e.target.result;
                    preview.style.display = 'block';
                };
                reader.readAsDataURL(input.files[0]);
            }
        }

        // ================= DRAG AND DROP & GALLERY PREVIEWS =================
        function setupDropZone(dropZoneId, inputId, textLabelId, isMultiple) {
            const dropZone = document.getElementById(dropZoneId);
            const inputElement = document.getElementById(inputId);
            const textLabel = document.getElementById(textLabelId);

            if (!dropZone || !inputElement) return;

            ['dragenter', 'dragover', 'dragleave', 'drop'].forEach(eventName => {
                dropZone.addEventListener(eventName, function (e) {
                    e.preventDefault();
                    e.stopPropagation();
                }, false);
            });

            ['dragenter', 'dragover'].forEach(eventName => {
                dropZone.addEventListener(eventName, () => dropZone.classList.add('dragover'), false);
            });

            ['dragleave', 'drop'].forEach(eventName => {
                dropZone.addEventListener(eventName, () => dropZone.classList.remove('dragover'), false);
            });

            dropZone.addEventListener('drop', function (e) {
                const dt = e.dataTransfer;
                handleFiles(dt.files);
            }, false);

            inputElement.addEventListener('change', function () {
                handleFiles(this.files);
            });

            function handleFiles(files) {
                if (files.length === 0) return;
                const dataTransfer = new DataTransfer();

                if (isMultiple) {
                    const limit = Math.min(files.length, 4);
                    const previewContainer = document.getElementById('extraImagesPreview');
                    if (previewContainer) previewContainer.innerHTML = ''; // Limpiar previas anteriores

                    for (let i = 0; i < limit; i++) {
                        dataTransfer.items.add(files[i]);

                        // Crear vista previa para cada imagen de galería
                        if (previewContainer) {
                            let reader = new FileReader();
                            reader.onload = function (e) {
                                let img = document.createElement('img');
                                img.src = e.target.result;
                                img.style.cssText = "width: 80px; height: 80px; object-fit: cover; border-radius: 8px; border: 1px solid var(--border-color); margin: 3px;";
                                previewContainer.appendChild(img);
                            };
                            reader.readAsDataURL(files[i]);
                        }
                    }
                    textLabel.innerText = `${limit} image(s) attached ready for upload`;
                    textLabel.style.color = "#1a7a4a";
                } else {
                    dataTransfer.items.add(files[0]);
                    textLabel.innerText = files[0].name;
                    textLabel.style.color = "#1a7a4a";
                    previewImage({ files: dataTransfer.files });
                }

                inputElement.files = dataTransfer.files;
            }
        }

        document.addEventListener('DOMContentLoaded', function () {
            setupDropZone('dzMainImage', 'fileImagen', 'lblMainImageText', false);
            setupDropZone('dzGalleryImages', 'fuExtraImages', 'lblGalleryImagesText', true);

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

        // Re-vincular eventos de Drag & Drop tras llamadas de UpdatePanel
        if (typeof Sys !== 'undefined' && Sys.WebForms && Sys.WebForms.PageRequestManager) {
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
                setupDropZone('dzMainImage', 'fileImagen', 'lblMainImageText', false);
                setupDropZone('dzGalleryImages', 'fuExtraImages', 'lblGalleryImagesText', true);
            });
        }
    </script>
</body>
</html>