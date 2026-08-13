<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="AdminBanners.aspx.cs" Inherits="OFFSIDESHOP.AdminBanners" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1, shrink-to-fit=no" />
    <title><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Banners_PageTitle %>" /></title>

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
        .banner-preview-thumb { width: 80px; height: 50px; object-fit: cover; border-radius: 4px; border: 1px solid var(--border-color); }
        .collection-preview-thumb { width: 60px; height: 60px; object-fit: cover; border-radius: 6px; border: 1px solid var(--border-color); }
        .auth-preview-thumb { width: 50px; height: 80px; object-fit: cover; border-radius: 4px; border: 1px solid var(--border-color); }
        .status-badge { padding: 3px 10px; border-radius: 20px; font-size: 0.75rem; font-weight: 600; }
        .status-active { background: #1a7a4a; color: #a8f0c6; }
        .status-inactive { background: #5c2323; color: #f0a8a8; }
        .img-preview { display: none; max-width: 100%; max-height: 180px; border-radius: 6px; margin-top: 10px; border: 1px solid var(--border-color); object-fit: cover; }
        
        .arrow-cell { background: transparent !important; border: none !important; width: 30px; padding: 0 5px !important; vertical-align: middle; }
        .arrow-btn { display: block; color: #6c757d; font-size: 1.3rem; cursor: pointer; text-align: center; transition: color 0.2s, transform 0.2s; margin: 4px 0; text-decoration: none !important; }
        .arrow-btn:hover { color: #d4af37; transform: scale(1.2); }
        
        .action-icon { display: inline-flex; align-items: center; justify-content: center; width: 32px; height: 32px; border-radius: 6px; color: white !important; transition: transform 0.2s, box-shadow 0.2s; text-decoration: none !important; border: none; cursor: pointer; }
        .action-icon:hover { transform: translateY(-2px); }
        .edit-icon { background: #3b82f6; box-shadow: 0 4px 10px rgba(59,130,246,0.3); }
        .toggle-icon { background: #6b7280; box-shadow: 0 4px 10px rgba(107,114,128,0.3); }
        .toggle-on { background: #10b981; box-shadow: 0 4px 10px rgba(16,185,129,0.3); }
        .delete-icon { background: #ef4444; box-shadow: 0 4px 10px rgba(239,68,68,0.3); }
        
        .btn-save-order { background: linear-gradient(135deg, #d4af37, #b5952f); color: #000; font-weight: 700; border: none; padding: 10px 20px; border-radius: 8px; transition: all 0.3s ease; text-decoration: none !important; display: inline-block; }
        .btn-save-order:hover { transform: translateY(-2px); box-shadow: 0 5px 15px rgba(212, 175, 55, 0.4); color: #000; }
        .number-badge { background: #2a2a2a; color: #d4af37; border: 1px solid #444; padding: 4px 12px; border-radius: 6px; font-weight: bold; font-size: 1rem; }

        /* Tabs */
        .nav-tabs .nav-link { color: #888; font-weight: 600; border: none; border-bottom: 2px solid transparent; padding: 12px 25px; transition: all 0.3s; background: transparent; cursor: pointer; }
        .nav-tabs .nav-link:hover { color: #d4af37; border-bottom-color: #555; }
        .nav-tabs .nav-link.active { color: #FFC800 !important; background: transparent; border: none; border-bottom: 3px solid #FFC800; }
        
        .sub-nav-tabs { border-bottom: 1px solid #444; margin-bottom: 15px; }
        .sub-nav-tabs .nav-link { font-size: 0.85rem; padding: 6px 15px; color: #aaa; border: 1px solid transparent; border-radius: 6px 6px 0 0; }
        .sub-nav-tabs .nav-link.active { color: #FFC800 !important; background: rgba(255, 200, 0, 0.1); border-color: #444 #444 transparent #444; }

        .tab-content { padding-top: 15px; }
        .tab-pane { display: none; }
        .tab-pane.active { display: block; }

        /* Translate Button */
        .btn-translate { background: #334155; color: #FFC800; border: 1px solid #FFC800; border-radius: 6px; font-size: 0.78rem; font-weight: 600; padding: 4px 10px; transition: all 0.2s ease; cursor: pointer; }
        .btn-translate:hover { background: #FFC800; color: #000; }

        /* Drag and Drop Zone */
        .drag-drop-zone { border: 2px dashed var(--border-color); background: rgba(30, 41, 59, 0.05); border-radius: 12px; padding: 25px 20px; text-align: center; cursor: pointer; transition: all 0.3s ease; position: relative; margin-top: 10px; display: flex; flex-direction: column; align-items: center; justify-content: center; }
        body.dark-mode .drag-drop-zone { background: rgba(30, 41, 59, 0.2); }
        .drag-drop-zone:hover, .drag-drop-zone.dragover { border-color: var(--accent-gold); background: rgba(255, 200, 0, 0.04); box-shadow: 0 0 15px rgba(255, 200, 0, 0.1); }
        body.dark-mode .drag-drop-zone:hover, body.dark-mode .drag-drop-zone.dragover { background: rgba(255, 200, 0, 0.02); box-shadow: 0 0 20px rgba(255, 200, 0, 0.15); }
        .drag-drop-icon { font-size: 2.2rem; color: var(--text-muted); margin-bottom: 8px; transition: transform 0.3s ease, color 0.3s ease; }
        .drag-drop-zone:hover .drag-drop-icon, .drag-drop-zone.dragover .drag-drop-icon { color: var(--accent-gold); transform: translateY(-4px); }
        .drag-drop-text { font-size: 0.9rem; font-weight: 600; color: var(--text-main); margin-bottom: 4px; }
        .drag-drop-browse { color: var(--accent-gold); text-decoration: underline; }
        .drag-drop-info { font-size: 0.78rem; color: var(--text-muted); margin: 0; }
        .drag-drop-zone.has-preview .drag-drop-content { display: none; }

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
                            <asp:LinkButton ID="btnAuditLogs" CssClass="sidebar-btn" runat="server" OnClick="btnAuditLogs_Click" Style="font-family: 'Raleway', 'Font Awesome 5 Free'; font-weight: 600;">
                                &#xf03a; <asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Nav_AuditLogs %>" />
                            </asp:LinkButton>
                        </li>
                    </asp:PlaceHolder>

                    <li>
                        <asp:LinkButton ID="btnAdminBanners" CssClass="sidebar-btn active" runat="server" OnClick="btnAdminBanners_Click" Style="font-family: 'Raleway', 'Font Awesome 5 Free'; font-weight: 600;">
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
                <div class="container-fluid">
                    <h1 class="page-title"><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Banners_Title %>" /></h1>
                    <p class="text-muted mb-4"><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Banners_Subtitle %>" /></p>

                    <ul class="nav nav-tabs" id="adminTabs" role="tablist">
                        <li class="nav-item" role="presentation">
                            <a class="nav-link active" id="banners-tab" data-toggle="tab" href="#banners" role="tab"><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Banners_TabMain %>" /></a>
                        </li>
                        <li class="nav-item" role="presentation">
                            <a class="nav-link" id="collections-tab" data-toggle="tab" href="#collections" role="tab"><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Banners_TabCollections %>" /></a>
                        </li>
                        <li class="nav-item" role="presentation">
                            <a class="nav-link" id="auth-tab" data-toggle="tab" href="#auth" role="tab"><i class="fas fa-lock me-1"></i> <asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Banners_TabAuth %>" /></a>
                        </li>
                    </ul>

                    <div class="tab-content" id="adminTabsContent">
                        
                        <!-- TAB 1: BANNERS -->
                        <div class="tab-pane active" id="banners" role="tabpanel">
                            <asp:UpdatePanel ID="upMain" runat="server">
                                <ContentTemplate>
                                    <div class="row">
                                        <div class="col-xl-9 col-lg-11">
                                            <div class="form-card">
                                                <asp:HiddenField ID="hfEditId" runat="server" Value="0" />
                                                <h4 class="text-white mb-3" style="font-weight: 600;"><asp:Label ID="lblFormTitle" runat="server"></asp:Label></h4>

                                                <!-- SUB-TABS IDIOMAS PARA BANNERS -->
                                                <ul class="nav nav-tabs sub-nav-tabs" role="tablist">
                                                    <li class="nav-item">
                                                        <a class="nav-link active" id="banner-en-tab" data-toggle="tab" href="#banner-en" role="tab"><i class="fas fa-globe-americas me-1"></i> <asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Products_EnglishTab %>" /></a>
                                                    </li>
                                                    <li class="nav-item">
                                                        <a class="nav-link" id="banner-es-tab" data-toggle="tab" href="#banner-es" role="tab"><i class="fas fa-globe-americas me-1"></i> <asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Products_SpanishTab %>" /></a>
                                                    </li>
                                                </ul>

                                                <div class="tab-content border-bottom border-secondary pb-3 mb-3">
                                                    <!-- ENGLISH FIELDS -->
                                                    <div class="tab-pane active" id="banner-en" role="tabpanel">
                                                        <div class="d-flex justify-content-between align-items-center mb-2">
                                                            <small class="text-warning font-weight-bold"><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Banners_PrimaryEn %>" /></small>
                                                            <button type="button" class="btn-translate" onclick="autoTranslate('en', 'es', ['txtTitle', 'txtSubtitle'], ['txtTitle_ES', 'txtSubtitle_ES'])">
                                                                <i class="fas fa-language me-1"></i> <asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Banners_TranslateToEs %>" />
                                                            </button>
                                                        </div>
                                                        <div class="form-group mb-2">
                                                            <label><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Banners_TitleEN %>" /> <span class="text-danger">*</span></label>
                                                            <asp:TextBox ID="txtTitle" ClientIDMode="Static" runat="server" CssClass="form-control" MaxLength="200" placeholder='<%$ Resources:Strings, Admin_Banners_PhTitleEN %>'></asp:TextBox>
                                                        </div>
                                                        <div class="form-group mb-0">
                                                            <label><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Banners_SubtitleEN %>" /> <span class="text-danger">*</span></label>
                                                            <asp:TextBox ID="txtSubtitle" ClientIDMode="Static" runat="server" CssClass="form-control" MaxLength="300" placeholder='<%$ Resources:Strings, Admin_Banners_PhSubEN %>'></asp:TextBox>
                                                        </div>
                                                    </div>

                                                    <!-- SPANISH FIELDS -->
                                                    <div class="tab-pane" id="banner-es" role="tabpanel">
                                                        <div class="d-flex justify-content-between align-items-center mb-2">
                                                            <small class="text-warning font-weight-bold"><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Banners_PrimaryEs %>" /></small>
                                                            <button type="button" class="btn-translate" onclick="autoTranslate('es', 'en', ['txtTitle_ES', 'txtSubtitle_ES'], ['txtTitle', 'txtSubtitle'])">
                                                                <i class="fas fa-language me-1"></i> <asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Banners_TranslateToEn %>" />
                                                            </button>
                                                        </div>
                                                        <div class="form-group mb-2">
                                                            <label><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Banners_TitleES %>" /> <span class="text-danger">*</span></label>
                                                            <asp:TextBox ID="txtTitle_ES" ClientIDMode="Static" runat="server" CssClass="form-control" MaxLength="200" placeholder='<%$ Resources:Strings, Admin_Banners_PhTitleES %>'></asp:TextBox>
                                                        </div>
                                                        <div class="form-group mb-0">
                                                            <label><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Banners_SubtitleES %>" /> <span class="text-danger">*</span></label>
                                                            <asp:TextBox ID="txtSubtitle_ES" ClientIDMode="Static" runat="server" CssClass="form-control" MaxLength="300" placeholder='<%$ Resources:Strings, Admin_Banners_PhSubEN %>'></asp:TextBox>
                                                        </div>
                                                    </div>
                                                </div>

                                                <!-- UNIVERSAL FIELDS -->
                                                <h6 class="text-muted mb-3 font-weight-bold"><i class="fas fa-cogs me-1"></i> <asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Banners_UniversalSettings %>" /></h6>
                                                <div class="row">
                                                    <div class="col-md-6">
                                                        <div class="form-group">
                                                            <label><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Banners_LinkUrl %>" /> <small class="text-muted">(<asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Products_Optional %>" />)</small></label>
                                                            <asp:TextBox ID="txtLinkURL" runat="server" CssClass="form-control" MaxLength="500"></asp:TextBox>
                                                        </div>
                                                    </div>
                                                    <div class="col-md-6">
                                                        <div class="form-group">
                                                            <label><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Banners_Status %>" /> <span class="text-danger">*</span></label>
                                                            <asp:DropDownList ID="ddlIsActive" runat="server" CssClass="form-control">
                                                                <asp:ListItem Value="1" Text="<%$ Resources:Strings, Admin_Banners_StatusActive %>"></asp:ListItem>
                                                                <asp:ListItem Value="0" Text="<%$ Resources:Strings, Admin_Banners_StatusInactive %>"></asp:ListItem>
                                                            </asp:DropDownList>
                                                        </div>
                                                    </div>
                                                </div>

                                                <div class="form-group mt-2">
                                                    <label><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Banners_ImageLabel %>" /> <small class="text-muted">(<asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Banners_SupportsImg %>" />)</small><asp:Label ID="lblImageRequired" runat="server" Text=" *" CssClass="text-danger"></asp:Label></label>
                                                    <asp:Panel ID="pnlCurrentImage" runat="server" Visible="false" CssClass="mb-2">
                                                        <small class="text-muted"><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Banners_CurrentImage %>" /> </small><asp:Label ID="lblCurrentImagePath" runat="server" CssClass="text-info"></asp:Label><br />
                                                        <small class="text-muted"><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Banners_KeepImageNote %>" /></small>
                                                    </asp:Panel>

                                                    <div class="drag-drop-zone" id="bannerDragDropZone">
                                                        <div class="drag-drop-content">
                                                            <i class="fas fa-cloud-upload-alt drag-drop-icon"></i>
                                                            <p class="drag-drop-text"><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Banners_DragDropText %>" /></p>
                                                            <p class="drag-drop-info"><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Banners_SupportsImg %>" /></p>
                                                        </div>
                                                        <asp:FileUpload ID="fileImagen" ClientIDMode="Static" runat="server" Style="display: none;" onchange="previewImage(this, 'imgPreview', 'bannerDragDropZone')" accept=".jpg,.jpeg,.png,.webp" />
                                                        <img id="imgPreview" class="img-preview" src="#" alt="Preview" />
                                                    </div>
                                                </div>

                                                <div class="row mt-4">
                                                    <div class="col-12 d-flex gap-2">
                                                        <asp:LinkButton ID="btnSave" runat="server" CssClass="mybtn" OnClick="btnSave_Click" OnClientClick="return validateBannerForm();" Style="font-family: 'Raleway','Font Awesome 5 Free'; font-weight: 600; text-decoration: none; display: inline-block;">
                                                            &#xf0c7;&nbsp; <asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Banners_SaveBanner %>" />
                                                        </asp:LinkButton>
                                                        <asp:LinkButton ID="btnCancel" runat="server" CssClass="mybtn" OnClick="btnCancel_Click" CausesValidation="false" Style="font-family: 'Raleway','Font Awesome 5 Free'; font-weight: 600; background: #444 !important; text-decoration: none; display: inline-block;">
                                                            &#xf00d;&nbsp; <asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Banners_Cancel %>" />
                                                        </asp:LinkButton>
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                    </div>

                                    <div class="row mt-5">
                                        <div class="col-12">
                                            <div class="d-flex justify-content-between align-items-center mb-4">
                                                <h3 class="text-white m-0" style="font-weight: 600;"><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Banners_OrganizerTitle %>" /></h3>
                                                <asp:HiddenField ID="hfNewOrder" runat="server" />
                                                <asp:LinkButton ID="btnSaveOrder" runat="server" CssClass="btn-save-order" OnClick="btnSaveOrder_Click">
                                                    <i class="fas fa-save mr-2"></i><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Banners_SaveOrder %>" />
                                                </asp:LinkButton>
                                            </div>
                                            <div class="table-responsive">
                                                <asp:GridView ID="gvBanners" runat="server" AutoGenerateColumns="False" GridLines="None" CssClass="table table-custom text-center align-middle" DataKeyNames="ID" OnRowCommand="gvBanners_RowCommand" OnRowDataBound="gvBanners_RowDataBound" EmptyDataText="<%$ Resources:Strings, Admin_Banners_EmptyBanners %>">
                                                    <Columns>
                                                        <asp:TemplateField ItemStyle-CssClass="arrow-cell">
                                                            <ItemTemplate>
                                                                <a href="javascript:void(0);" class="arrow-btn move-up"><i class="fas fa-caret-up"></i></a>
                                                                <a href="javascript:void(0);" class="arrow-btn move-down"><i class="fas fa-caret-down"></i></a>
                                                            </ItemTemplate>
                                                        </asp:TemplateField>
                                                        <asp:TemplateField HeaderText="<%$ Resources:Strings, Admin_Banners_ColOrder %>">
                                                            <ItemTemplate>
                                                                <span class="number-badge sort-order-lbl"><%# Eval("SortOrder") %></span>
                                                                <input type="hidden" class="banner-id" value='<%# Eval("ID") %>' />
                                                            </ItemTemplate>
                                                        </asp:TemplateField>
                                                        <asp:TemplateField HeaderText="<%$ Resources:Strings, Admin_Banners_ColPreview %>">
                                                            <ItemTemplate>
                                                                <img src='<%# GetImageThumb(Eval("ImageURL").ToString(), "banners") %>' class="banner-preview-thumb" onerror="this.src='assets/img/default.jpg';" />
                                                            </ItemTemplate>
                                                        </asp:TemplateField>
                                                        <asp:BoundField DataField="Title" HeaderText="<%$ Resources:Strings, Admin_Banners_ColTitleEN %>" ItemStyle-HorizontalAlign="Left" />
                                                        <asp:BoundField DataField="Title_ES" HeaderText="<%$ Resources:Strings, Admin_Banners_ColTitleES %>" ItemStyle-HorizontalAlign="Left" NullDisplayText="-" />
                                                        <asp:TemplateField HeaderText="<%$ Resources:Strings, Admin_Banners_ColStatus %>">
                                                            <ItemTemplate><asp:Label ID="lblStatus" runat="server"></asp:Label></ItemTemplate>
                                                        </asp:TemplateField>
                                                        <asp:TemplateField HeaderText="<%$ Resources:Strings, Admin_Banners_ColActions %>">
                                                            <ItemTemplate>
                                                                <div class="d-flex align-items-center justify-content-center" style="gap: 8px;">
                                                                    <asp:LinkButton ID="btnEdit" runat="server" CssClass="action-icon edit-icon" CommandName="EditBanner" CommandArgument='<%# Eval("ID") %>'><i class="fas fa-pen"></i></asp:LinkButton>
                                                                    <asp:LinkButton ID="btnToggle" runat="server" CommandName="ToggleBanner" CommandArgument='<%# Eval("ID") %>'></asp:LinkButton>
                                                                    <asp:LinkButton ID="btnDelete" runat="server" CssClass="action-icon delete-icon" CommandName="DeleteBanner" CommandArgument='<%# Eval("ID") %>' OnClientClick='<%# "return confirm(\"" + GetGlobalResourceObject("Strings", "Confirm_DeleteBanner") + "\");" %>'><i class="fas fa-trash"></i></asp:LinkButton>
                                                                </div>
                                                            </ItemTemplate>
                                                        </asp:TemplateField>
                                                    </Columns>
                                                </asp:GridView>
                                            </div>
                                        </div>
                                    </div>
                                </ContentTemplate>
                                <Triggers><asp:PostBackTrigger ControlID="btnSave" /></Triggers>
                            </asp:UpdatePanel>
                        </div>

                        <!-- TAB 2: COLLECTIONS -->
                        <div class="tab-pane" id="collections" role="tabpanel">
                            <asp:UpdatePanel ID="upCollections" runat="server">
                                <ContentTemplate>
                                    <!-- Categories Management -->
                                    <div class="form-card mb-5">
                                        <h5 class="text-white mb-3" style="font-weight: 600;"><i class="fas fa-tags text-warning me-2"></i><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Banners_CategoriesTitle %>" /></h5>
                                        <div class="row align-items-end">
                                            <div class="col-md-4">
                                                <label><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Banners_CatNameEN %>" /> <span class="text-danger">*</span></label>
                                                <asp:TextBox ID="txtCategoryName" ClientIDMode="Static" runat="server" CssClass="form-control" placeholder='<%$ Resources:Strings, Admin_Banners_PhCatEN %>'></asp:TextBox>
                                            </div>
                                            <div class="col-md-4">
                                                <div class="d-flex justify-content-between align-items-center mb-1">
                                                    <label class="m-0"><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Banners_CatNameES %>" /> <span class="text-danger">*</span></label>
                                                    <button type="button" class="btn-translate py-0 px-2" onclick="autoTranslate('en', 'es', ['txtCategoryName'], ['txtCategoryName_ES'])">
                                                        Auto-ES
                                                    </button>
                                                </div>
                                                <asp:TextBox ID="txtCategoryName_ES" ClientIDMode="Static" runat="server" CssClass="form-control" placeholder='<%$ Resources:Strings, Admin_Banners_PhCatEN %>'></asp:TextBox>
                                            </div>
                                            <div class="col-md-3 mt-3 mt-md-0">
                                                <asp:LinkButton ID="btnAddCategory" runat="server" CssClass="btn-save-order w-100 text-center" OnClick="btnAddCategory_Click" OnClientClick="return validateCategoryForm();">
                                                    <asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Banners_BtnAddCategory %>" />
                                                </asp:LinkButton>
                                            </div>
                                        </div>
                                        <div class="mt-4">
                                            <asp:GridView ID="gvCategories" runat="server" AutoGenerateColumns="false" CssClass="table table-custom text-center align-middle w-75" DataKeyNames="Id_Category" OnRowDeleting="gvCategories_RowDeleting" EmptyDataText="<%$ Resources:Strings, Admin_Banners_EmptyCategories %>">
                                                <Columns>
                                                    <asp:BoundField DataField="Name_Category" HeaderText="<%$ Resources:Strings, Admin_Banners_CatNameEN %>" ItemStyle-HorizontalAlign="Left" />
                                                    <asp:BoundField DataField="Name_Category_es" HeaderText="<%$ Resources:Strings, Admin_Banners_CatNameES %>" ItemStyle-HorizontalAlign="Left" NullDisplayText="-" />
                                                    <asp:CommandField ShowDeleteButton="True" DeleteText="<i class='fas fa-trash text-danger'></i>" ControlStyle-CssClass="text-decoration-none" />
                                                </Columns>
                                            </asp:GridView>
                                        </div>
                                    </div>

                                    <!-- Collections Management -->
                                    <div class="row">
                                        <div class="col-xl-8 col-lg-10">
                                            <div class="form-card">
                                                <asp:HiddenField ID="hfColEditId" runat="server" Value="0" />
                                                <h4 class="text-white mb-4" style="font-weight: 600;"><asp:Label ID="lblColFormTitle" runat="server"></asp:Label></h4>

                                                <!-- SUB-TABS IDIOMAS PARA COLECCIONES -->
                                                <ul class="nav nav-tabs sub-nav-tabs" role="tablist">
                                                    <li class="nav-item">
                                                        <a class="nav-link active" id="col-en-tab" data-toggle="tab" href="#col-en" role="tab"><i class="fas fa-globe-americas me-1"></i> <asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Products_EnglishTab %>" /></a>
                                                    </li>
                                                    <li class="nav-item">
                                                        <a class="nav-link" id="col-es-tab" data-toggle="tab" href="#col-es" role="tab"><i class="fas fa-globe-americas me-1"></i> <asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Products_SpanishTab %>" /></a>
                                                    </li>
                                                </ul>

                                                <div class="tab-content border-bottom border-secondary pb-3 mb-3">
                                                    <!-- ENGLISH FIELDS -->
                                                    <div class="tab-pane active" id="col-en" role="tabpanel">
                                                        <div class="d-flex justify-content-between align-items-center mb-2">
                                                            <small class="text-warning font-weight-bold"><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Banners_PrimaryEn %>" /></small>
                                                            <button type="button" class="btn-translate" onclick="autoTranslate('en', 'es', ['txtColTitle'], ['txtColTitle_ES'])">
                                                                <i class="fas fa-language me-1"></i> <asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Banners_TranslateToEs %>" />
                                                            </button>
                                                        </div>
                                                        <div class="form-group mb-0">
                                                            <label><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Banners_ColTitle %>" /> (EN) <span class="text-danger">*</span></label>
                                                            <asp:TextBox ID="txtColTitle" ClientIDMode="Static" runat="server" CssClass="form-control" MaxLength="100"></asp:TextBox>
                                                        </div>
                                                    </div>

                                                    <!-- SPANISH FIELDS -->
                                                    <div class="tab-pane" id="col-es" role="tabpanel">
                                                        <div class="d-flex justify-content-between align-items-center mb-2">
                                                            <small class="text-warning font-weight-bold"><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Banners_PrimaryEs %>" /></small>
                                                            <button type="button" class="btn-translate" onclick="autoTranslate('es', 'en', ['txtColTitle_ES'], ['txtColTitle'])">
                                                                <i class="fas fa-language me-1"></i> <asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Banners_TranslateToEn %>" />
                                                            </button>
                                                        </div>
                                                        <div class="form-group mb-0">
                                                            <label><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Banners_ColTitle %>" /> (ES) <span class="text-danger">*</span></label>
                                                            <asp:TextBox ID="txtColTitle_ES" ClientIDMode="Static" runat="server" CssClass="form-control" MaxLength="255"></asp:TextBox>
                                                        </div>
                                                    </div>
                                                </div>

                                                <!-- UNIVERSAL FIELDS FOR COLLECTIONS -->
                                                <h6 class="text-muted mb-3 font-weight-bold"><i class="fas fa-cogs me-1"></i> <asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Banners_UniversalSettings %>" /></h6>

                                                <div class="row">
                                                    <div class="col-md-6">
                                                        <div class="form-group">
                                                            <label><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Banners_ColCategory %>" /> <span class="text-danger">*</span></label>
                                                            <asp:DropDownList ID="ddlColCategory" runat="server" CssClass="form-control"></asp:DropDownList>
                                                        </div>
                                                    </div>
                                                    <div class="col-md-6">
                                                        <div class="form-group">
                                                            <label><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Banners_ColTargetLink %>" /> <span class="text-danger">*</span></label>
                                                            <asp:TextBox ID="txtColLink" runat="server" CssClass="form-control" MaxLength="255"></asp:TextBox>
                                                        </div>
                                                    </div>
                                                    <div class="col-md-6">
                                                        <div class="form-group">
                                                            <label><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Banners_Status %>" /></label>
                                                            <asp:DropDownList ID="ddlColStatus" runat="server" CssClass="form-control">
                                                                <asp:ListItem Value="1" Text="<%$ Resources:Strings, Admin_Banners_StatusActive %>"></asp:ListItem>
                                                                <asp:ListItem Value="0" Text="<%$ Resources:Strings, Admin_Banners_StatusInactive %>"></asp:ListItem>
                                                            </asp:DropDownList>
                                                        </div>
                                                    </div>
                                                </div>

                                                <div class="form-group mt-2">
                                                    <label><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Banners_ColBgImage %>" /> <small class="text-muted">(<asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Banners_SupportsImg %>" />)</small><asp:Label ID="lblColImgReq" runat="server" Text=" *" CssClass="text-danger"></asp:Label></label>
                                                    <asp:Panel ID="pnlColImg" runat="server" Visible="false" CssClass="mb-2">
                                                        <small class="text-muted"><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Banners_CurrentImage %>" /> </small>
                                                        <asp:Label ID="lblColImgPath" runat="server" CssClass="text-info"></asp:Label>
                                                    </asp:Panel>

                                                    <div class="drag-drop-zone" id="colDragDropZone">
                                                        <div class="drag-drop-content">
                                                            <i class="fas fa-cloud-upload-alt drag-drop-icon"></i>
                                                            <p class="drag-drop-text"><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Banners_ColDragDrop %>" /></p>
                                                            <p class="drag-drop-info"><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Banners_SupportsImg %>" /></p>
                                                        </div>
                                                        <asp:FileUpload ID="fileColImagen" ClientIDMode="Static" runat="server" Style="display: none;" onchange="previewImage(this, 'imgColPreview', 'colDragDropZone')" accept=".jpg,.jpeg,.png,.webp" />
                                                        <img id="imgColPreview" class="img-preview" src="#" alt="Preview" />
                                                    </div>
                                                </div>

                                                <div class="row mt-4">
                                                    <div class="col-12 d-flex gap-2">
                                                        <asp:LinkButton ID="btnColSave" runat="server" CssClass="mybtn" OnClick="btnColSave_Click" OnClientClick="return validateColForm();" Style="font-family: 'Raleway','Font Awesome 5 Free'; font-weight: 600; text-decoration: none; display: inline-block;">
                                                            &#xf0c7;&nbsp; <asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Banners_SaveCollection %>" />
                                                        </asp:LinkButton>
                                                        <asp:LinkButton ID="btnColCancel" runat="server" CssClass="mybtn" OnClick="btnColCancel_Click" CausesValidation="false" Style="font-family: 'Raleway','Font Awesome 5 Free'; font-weight: 600; background: #444 !important; text-decoration: none; display: inline-block;">
                                                            &#xf00d;&nbsp; <asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Banners_Cancel %>" />
                                                        </asp:LinkButton>
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                    </div>

                                    <!-- Collections Grid -->
                                    <div class="row mt-5">
                                        <div class="col-12">
                                            <div class="d-flex justify-content-between align-items-center mb-4">
                                                <h3 class="text-white m-0" style="font-weight: 600;"><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Banners_CurrentCollections %>" /></h3>
                                                <asp:HiddenField ID="hfColOrder" runat="server" />
                                                <asp:LinkButton ID="btnSaveColOrder" runat="server" CssClass="btn-save-order" OnClick="btnSaveColOrder_Click">
                                                    <i class="fas fa-save mr-2"></i><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Banners_SaveOrder %>" />
                                                </asp:LinkButton>
                                            </div>
                                            <div class="table-responsive">
                                                <asp:GridView ID="gvCollections" runat="server" AutoGenerateColumns="False" GridLines="None" CssClass="table table-custom text-center align-middle" DataKeyNames="Id_Collection" OnRowCommand="gvCollections_RowCommand" OnRowDataBound="gvCollections_RowDataBound" EmptyDataText="<%$ Resources:Strings, Admin_Banners_EmptyCollections %>">
                                                    <Columns>
                                                        <asp:TemplateField ItemStyle-CssClass="arrow-cell">
                                                            <ItemTemplate>
                                                                <a href="javascript:void(0);" class="arrow-btn move-up-col"><i class="fas fa-caret-up"></i></a>
                                                                <a href="javascript:void(0);" class="arrow-btn move-down-col"><i class="fas fa-caret-down"></i></a>
                                                            </ItemTemplate>
                                                        </asp:TemplateField>
                                                        <asp:TemplateField HeaderText="<%$ Resources:Strings, Admin_Banners_ColOrder %>">
                                                            <ItemTemplate>
                                                                <span class="number-badge sort-col-lbl"><%# Eval("SortOrder") %></span>
                                                                <input type="hidden" class="col-id" value='<%# Eval("Id_Collection") %>' />
                                                            </ItemTemplate>
                                                        </asp:TemplateField>
                                                        <asp:TemplateField HeaderText="<%$ Resources:Strings, Admin_Banners_ColPreview %>">
                                                            <ItemTemplate>
                                                                <img src='<%# GetImageThumb(Eval("ImageURL").ToString(), "collections") %>' class="collection-preview-thumb" onerror="this.src='assets/img/default.jpg';" />
                                                            </ItemTemplate>
                                                        </asp:TemplateField>
                                                        <asp:BoundField DataField="Title" HeaderText="<%$ Resources:Strings, Admin_Banners_ColTitle %> (EN)" ItemStyle-HorizontalAlign="Left" />
                                                        <asp:BoundField DataField="Title_es" HeaderText="<%$ Resources:Strings, Admin_Banners_ColTitle %> (ES)" ItemStyle-HorizontalAlign="Left" NullDisplayText="-" />
                                                        <asp:BoundField DataField="Name_Category" HeaderText="<%$ Resources:Strings, Admin_Banners_ColCategory %>" />
                                                        <asp:TemplateField HeaderText="<%$ Resources:Strings, Admin_Banners_ColStatus %>">
                                                            <ItemTemplate><asp:Label ID="lblColStatus" runat="server"></asp:Label></ItemTemplate>
                                                        </asp:TemplateField>
                                                        <asp:TemplateField HeaderText="<%$ Resources:Strings, Admin_Banners_ColActions %>">
                                                            <ItemTemplate>
                                                                <div class="d-flex align-items-center justify-content-center" style="gap: 8px;">
                                                                    <asp:LinkButton ID="btnEditCol" runat="server" CssClass="action-icon edit-icon" CommandName="EditCol" CommandArgument='<%# Eval("Id_Collection") %>'><i class="fas fa-pen"></i></asp:LinkButton>
                                                                    <asp:LinkButton ID="btnToggleCol" runat="server" CommandName="ToggleCol" CommandArgument='<%# Eval("Id_Collection") %>'></asp:LinkButton>
                                                                    <asp:LinkButton ID="btnDelCol" runat="server" CssClass="action-icon delete-icon" CommandName="DeleteCol" CommandArgument='<%# Eval("Id_Collection") %>' OnClientClick='<%# "return confirm(\"" + GetGlobalResourceObject("Strings", "Confirm_DeleteCollection") + "\");" %>'><i class="fas fa-trash"></i></asp:LinkButton>
                                                                </div>
                                                            </ItemTemplate>
                                                        </asp:TemplateField>
                                                    </Columns>
                                                </asp:GridView>
                                            </div>
                                        </div>
                                    </div>
                                </ContentTemplate>
                                <Triggers><asp:PostBackTrigger ControlID="btnColSave" /></Triggers>
                            </asp:UpdatePanel>
                        </div>

                        <!-- TAB 3: AUTH CAROUSEL -->
                        <div class="tab-pane" id="auth" role="tabpanel">
                            <asp:UpdatePanel ID="upAuth" runat="server">
                                <ContentTemplate>
                                    <div class="row">
                                        <div class="col-xl-9 col-lg-11">
                                            <div class="form-card">
                                                <asp:HiddenField ID="hfAuthEditId" runat="server" Value="0" />
                                                <h4 class="text-white mb-3" style="font-weight: 600;"><asp:Label ID="lblAuthFormTitle" runat="server"></asp:Label></h4>

                                                <!-- SUB-TABS IDIOMAS PARA AUTH CAROUSEL -->
                                                <ul class="nav nav-tabs sub-nav-tabs" role="tablist">
                                                    <li class="nav-item">
                                                        <a class="nav-link active" id="auth-en-tab" data-toggle="tab" href="#auth-en" role="tab"><i class="fas fa-globe-americas me-1"></i> <asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Products_EnglishTab %>" /></a>
                                                    </li>
                                                    <li class="nav-item">
                                                        <a class="nav-link" id="auth-es-tab" data-toggle="tab" href="#auth-es" role="tab"><i class="fas fa-globe-americas me-1"></i> <asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Products_SpanishTab %>" /></a>
                                                    </li>
                                                </ul>

                                                <div class="tab-content border-bottom border-secondary pb-3 mb-3">
                                                    <!-- ENGLISH FIELDS -->
                                                    <div class="tab-pane active" id="auth-en" role="tabpanel">
                                                        <div class="d-flex justify-content-between align-items-center mb-2">
                                                            <small class="text-warning font-weight-bold"><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Banners_AuthQuoteRoleEN %>" /></small>
                                                            <button type="button" class="btn-translate" onclick="autoTranslate('en', 'es', ['txtAuthQuote', 'txtAuthAuthorRole'], ['txtAuthQuote_ES', 'txtAuthAuthorRole_ES'])">
                                                                <i class="fas fa-language me-1"></i> <asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Banners_TranslateToEs %>" />
                                                            </button>
                                                        </div>
                                                        <div class="form-group mb-2">
                                                            <label><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Banners_QuoteEN %>" /> <span class="text-danger">*</span></label>
                                                            <asp:TextBox ID="txtAuthQuote" ClientIDMode="Static" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="2" MaxLength="1000" placeholder='<%$ Resources:Strings, Admin_Banners_PhQuoteEN %>'></asp:TextBox>
                                                        </div>
                                                        <div class="form-group mb-0">
                                                            <label><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Banners_AuthorRoleEN %>" /></label>
                                                            <asp:TextBox ID="txtAuthAuthorRole" ClientIDMode="Static" runat="server" CssClass="form-control" MaxLength="150" placeholder='<%$ Resources:Strings, Admin_Banners_PhRoleEN %>'></asp:TextBox>
                                                        </div>
                                                    </div>

                                                    <!-- SPANISH FIELDS -->
                                                    <div class="tab-pane" id="auth-es" role="tabpanel">
                                                        <div class="d-flex justify-content-between align-items-center mb-2">
                                                            <small class="text-warning font-weight-bold"><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Banners_PrimaryEs %>" /></small>
                                                            <button type="button" class="btn-translate" onclick="autoTranslate('es', 'en', ['txtAuthQuote_ES', 'txtAuthAuthorRole_ES'], ['txtAuthQuote', 'txtAuthAuthorRole'])">
                                                                <i class="fas fa-language me-1"></i> <asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Banners_TranslateToEn %>" />
                                                            </button>
                                                        </div>
                                                        <div class="form-group mb-2">
                                                            <label><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Banners_QuoteES %>" /> <span class="text-danger">*</span></label>
                                                            <asp:TextBox ID="txtAuthQuote_ES" ClientIDMode="Static" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="2" MaxLength="1000" placeholder='<%$ Resources:Strings, Admin_Banners_PhQuoteEN %>'></asp:TextBox>
                                                        </div>
                                                        <div class="form-group mb-0">
                                                            <label><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Banners_AuthorRoleES %>" /></label>
                                                            <asp:TextBox ID="txtAuthAuthorRole_ES" ClientIDMode="Static" runat="server" CssClass="form-control" MaxLength="150" placeholder='<%$ Resources:Strings, Admin_Banners_PhRoleEN %>'></asp:TextBox>
                                                        </div>
                                                    </div>
                                                </div>

                                                <!-- UNIVERSAL FIELDS -->
                                                <h6 class="text-muted mb-3 font-weight-bold"><i class="fas fa-user-tag me-1"></i> <asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Banners_UnivAuthStatus %>" /></h6>
                                                <div class="row">
                                                    <div class="col-md-6">
                                                        <div class="form-group">
                                                            <label><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Banners_AuthorName %>" /> <span class="text-danger">*</span></label>
                                                            <asp:TextBox ID="txtAuthAuthorName" ClientIDMode="Static" runat="server" CssClass="form-control" MaxLength="100" placeholder='<%$ Resources:Strings, Admin_Banners_PhAuthor %>'></asp:TextBox>
                                                        </div>
                                                    </div>
                                                    <div class="col-md-6">
                                                        <div class="form-group">
                                                            <label><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Banners_Status %>" /> <span class="text-danger">*</span></label>
                                                            <asp:DropDownList ID="ddlAuthIsActive" runat="server" CssClass="form-control">
                                                                <asp:ListItem Value="1" Text="<%$ Resources:Strings, Admin_Banners_StatusActive %>"></asp:ListItem>
                                                                <asp:ListItem Value="0" Text="<%$ Resources:Strings, Admin_Banners_StatusInactive %>"></asp:ListItem>
                                                            </asp:DropDownList>
                                                        </div>
                                                    </div>
                                                </div>

                                                <div class="form-group mt-2">
                                                    <label><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Banners_BgImageLabel %>" /> <small class="text-muted">(<asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Banners_SupportsImg %>" />)</small><asp:Label ID="lblAuthImageRequired" runat="server" Text=" *" CssClass="text-danger"></asp:Label></label>
                                                    <asp:Panel ID="pnlAuthCurrentImage" runat="server" Visible="false" CssClass="mb-2">
                                                        <small class="text-muted"><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Banners_CurrentImage %>" /> </small><asp:Label ID="lblAuthCurrentImagePath" runat="server" CssClass="text-info"></asp:Label><br />
                                                    </asp:Panel>

                                                    <div class="drag-drop-zone" id="authDragDropZone">
                                                        <div class="drag-drop-content">
                                                            <i class="fas fa-image drag-drop-icon"></i>
                                                            <p class="drag-drop-text"><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Banners_AuthDragDrop %>" /></p>
                                                            <p class="drag-drop-info"><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Banners_SupportsImg %>" /></p>
                                                        </div>
                                                        <asp:FileUpload ID="fileAuthImagen" ClientIDMode="Static" runat="server" Style="display: none;" onchange="previewImage(this, 'imgAuthPreview', 'authDragDropZone')" accept=".jpg,.jpeg,.png,.webp" />
                                                        <img id="imgAuthPreview" class="img-preview" src="#" alt="Preview" />
                                                    </div>
                                                </div>

                                                <div class="row mt-4">
                                                    <div class="col-12 d-flex gap-2">
                                                        <asp:LinkButton ID="btnSaveAuth" runat="server" CssClass="mybtn" OnClick="btnSaveAuth_Click" OnClientClick="return validateAuthForm();" Style="font-family: 'Raleway','Font Awesome 5 Free'; font-weight: 600; text-decoration: none; display: inline-block;">
                                                            &#xf0c7;&nbsp; <asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Banners_SaveSlide %>" />
                                                        </asp:LinkButton>
                                                        <asp:LinkButton ID="btnAuthCancel" runat="server" CssClass="mybtn" OnClick="btnAuthCancel_Click" CausesValidation="false" Style="font-family: 'Raleway','Font Awesome 5 Free'; font-weight: 600; background: #444 !important; text-decoration: none; display: inline-block;">
                                                            &#xf00d;&nbsp; <asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Banners_Cancel %>" />
                                                        </asp:LinkButton>
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                    </div>

                                    <div class="row mt-5">
                                        <div class="col-12">
                                            <div class="d-flex justify-content-between align-items-center mb-4">
                                                <h3 class="text-white m-0" style="font-weight: 600;"><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Banners_CurrentAuthSlides %>" /></h3>
                                                <asp:HiddenField ID="hfAuthOrder" runat="server" />
                                                <asp:LinkButton ID="btnSaveAuthOrder" runat="server" CssClass="btn-save-order" OnClick="btnSaveAuthOrder_Click">
                                                    <i class="fas fa-save mr-2"></i><asp:Literal runat="server" Text="<%$ Resources:Strings, Admin_Banners_SaveOrder %>" />
                                                </asp:LinkButton>
                                            </div>
                                            <div class="table-responsive">
                                                <asp:GridView ID="gvAuthCarousel" runat="server" AutoGenerateColumns="False" GridLines="None" CssClass="table table-custom text-center align-middle" DataKeyNames="Id_Slide" OnRowCommand="gvAuthCarousel_RowCommand" OnRowDataBound="gvAuthCarousel_RowDataBound" EmptyDataText="<%$ Resources:Strings, Admin_Banners_EmptyAuth %>">
                                                    <Columns>
                                                        <asp:TemplateField ItemStyle-CssClass="arrow-cell">
                                                            <ItemTemplate>
                                                                <a href="javascript:void(0);" class="arrow-btn move-up-auth"><i class="fas fa-caret-up"></i></a>
                                                                <a href="javascript:void(0);" class="arrow-btn move-down-auth"><i class="fas fa-caret-down"></i></a>
                                                            </ItemTemplate>
                                                        </asp:TemplateField>
                                                        <asp:TemplateField HeaderText="<%$ Resources:Strings, Admin_Banners_ColOrder %>">
                                                            <ItemTemplate>
                                                                <span class="number-badge sort-auth-lbl"><%# Eval("DisplayOrder") %></span>
                                                                <input type="hidden" class="auth-id" value='<%# Eval("Id_Slide") %>' />
                                                            </ItemTemplate>
                                                        </asp:TemplateField>
                                                        <asp:TemplateField HeaderText="<%$ Resources:Strings, Admin_Banners_ColPreview %>">
                                                            <ItemTemplate>
                                                                <img src='<%# GetImageThumb(Eval("ImageURL").ToString(), "auth") %>' class="auth-preview-thumb" onerror="this.src='assets/img/default.jpg';" />
                                                            </ItemTemplate>
                                                        </asp:TemplateField>
                                                        <asp:BoundField DataField="AuthorName" HeaderText="<%$ Resources:Strings, Admin_Banners_ColAuthor %>" ItemStyle-HorizontalAlign="Left" />
                                                        <asp:BoundField DataField="QuoteText" HeaderText="<%$ Resources:Strings, Admin_Banners_ColQuote %>" ItemStyle-HorizontalAlign="Left" />
                                                        <asp:TemplateField HeaderText="<%$ Resources:Strings, Admin_Banners_ColStatus %>">
                                                            <ItemTemplate><asp:Label ID="lblAuthStatus" runat="server"></asp:Label></ItemTemplate>
                                                        </asp:TemplateField>
                                                        <asp:TemplateField HeaderText="<%$ Resources:Strings, Admin_Banners_ColActions %>">
                                                            <ItemTemplate>
                                                                <div class="d-flex align-items-center justify-content-center" style="gap: 8px;">
                                                                    <asp:LinkButton ID="btnEditAuth" runat="server" CssClass="action-icon edit-icon" CommandName="EditAuth" CommandArgument='<%# Eval("Id_Slide") %>'><i class="fas fa-pen"></i></asp:LinkButton>
                                                                    <asp:LinkButton ID="btnToggleAuth" runat="server" CommandName="ToggleAuth" CommandArgument='<%# Eval("Id_Slide") %>'></asp:LinkButton>
                                                                    <asp:LinkButton ID="btnDeleteAuth" runat="server" CssClass="action-icon delete-icon" CommandName="DeleteAuth" CommandArgument='<%# Eval("Id_Slide") %>' OnClientClick='<%# "return confirm(\"" + GetGlobalResourceObject("Strings", "Confirm_DeleteSlide") + "\");" %>'><i class="fas fa-trash"></i></asp:LinkButton>
                                                                </div>
                                                            </ItemTemplate>
                                                        </asp:TemplateField>
                                                    </Columns>
                                                </asp:GridView>
                                            </div>
                                        </div>
                                    </div>
                                </ContentTemplate>
                                <Triggers><asp:PostBackTrigger ControlID="btnSaveAuth" /></Triggers>
                            </asp:UpdatePanel>
                        </div>

                    </div>

                    <asp:Literal ID="alerta" runat="server" EnableViewState="false"></asp:Literal>
                </div>
            </main>
        </div>
    </form>

    <script src="https://ajax.googleapis.com/ajax/libs/jquery/3.4.1/jquery.min.js"></script>
    <script src="https://cdnjs.cloudflare.com/ajax/libs/popper.js/1.14.7/umd/popper.min.js"></script>
    <script src="https://maxcdn.bootstrapcdn.com/bootstrap/4.3.1/js/bootstrap.min.js"></script>

    <script type="text/javascript">
        $(document).ready(function () {
            $('.nav-tabs a').click(function (e) {
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
                            Toast.fire({ icon: 'success', title: '<%= GetGlobalResourceObject("Strings", "Alert_Banners_Translated") %>' });
                        }
                    })
                    .catch(function (err) {
                        console.error('Translation error:', err);
                    });
            }
        }

        // ================= VALIDACIONES BILINGÜES =================
        function validateBannerForm() {
            let titleEn = document.getElementById('txtTitle').value.trim();
            let subEn = document.getElementById('txtSubtitle').value.trim();
            let titleEs = document.getElementById('txtTitle_ES').value.trim();
            let subEs = document.getElementById('txtSubtitle_ES').value.trim();

            if (!titleEn || !subEn) {
                Swal.fire('<%= GetGlobalResourceObject("Strings", "Alert_MissingInfo") %>', '<%= GetGlobalResourceObject("Strings", "Alert_Banners_MissingEn") %>', 'warning');
                $('#banner-en-tab').tab('show');
                return false;
            }
            if (!titleEs || !subEs) {
                Swal.fire('<%= GetGlobalResourceObject("Strings", "Alert_MissingInfo") %>', '<%= GetGlobalResourceObject("Strings", "Alert_Banners_MissingEs") %>', 'warning');
                $('#banner-es-tab').tab('show');
                return false;
            }
            return true;
        }

        function validateColForm() {
            let titleEn = document.getElementById('txtColTitle').value.trim();
            let titleEs = document.getElementById('txtColTitle_ES').value.trim();
            let category = document.getElementById('<%= ddlColCategory.ClientID %>').value;
            let link = document.getElementById('<%= txtColLink.ClientID %>').value.trim();

            if (!titleEn) {
                Swal.fire('<%= GetGlobalResourceObject("Strings", "Alert_MissingInfo") %>', '<%= GetGlobalResourceObject("Strings", "Alert_Banners_MissingEn") %>', 'warning');
                $('#col-en-tab').tab('show');
                return false;
            }
            if (!titleEs) {
                Swal.fire('<%= GetGlobalResourceObject("Strings", "Alert_MissingInfo") %>', '<%= GetGlobalResourceObject("Strings", "Alert_Banners_MissingEs") %>', 'warning');
                $('#col-es-tab').tab('show');
                return false;
            }
            if (!category) {
                Swal.fire('<%= GetGlobalResourceObject("Strings", "Alert_MissingInfo") %>', '<%= GetGlobalResourceObject("Strings", "Admin_Banners_SelectCategory") %>', 'warning');
                return false;
            }
            if (!link) {
                Swal.fire('<%= GetGlobalResourceObject("Strings", "Alert_MissingInfo") %>', 'Target Link is required', 'warning');
                return false;
            }
            return true;
        }

        function validateCategoryForm() {
            let catEn = document.getElementById('txtCategoryName').value.trim();
            let catEs = document.getElementById('txtCategoryName_ES').value.trim();

            if (!catEn || !catEs) {
                Swal.fire('<%= GetGlobalResourceObject("Strings", "Alert_MissingInfo") %>', '<%= GetGlobalResourceObject("Strings", "Alert_Banners_MissingCat") %>', 'warning');
                return false;
            }
            return true;
        }

        function validateAuthForm() {
            let quoteEn = document.getElementById('txtAuthQuote').value.trim();
            let quoteEs = document.getElementById('txtAuthQuote_ES').value.trim();
            let author = document.getElementById('txtAuthAuthorName').value.trim();

            if (!quoteEn) {
                Swal.fire('<%= GetGlobalResourceObject("Strings", "Alert_MissingInfo") %>', '<%= GetGlobalResourceObject("Strings", "Alert_Banners_MissingQuoteEn") %>', 'warning');
                $('#auth-en-tab').tab('show');
                return false;
            }
            if (!quoteEs) {
                Swal.fire('<%= GetGlobalResourceObject("Strings", "Alert_MissingInfo") %>', '<%= GetGlobalResourceObject("Strings", "Alert_Banners_MissingQuoteEs") %>', 'warning');
                $('#auth-es-tab').tab('show');
                return false;
            }
            if (!author) {
                Swal.fire('<%= GetGlobalResourceObject("Strings", "Alert_MissingInfo") %>', '<%= GetGlobalResourceObject("Strings", "Alert_Banners_MissingAuthor") %>', 'warning');
                return false;
            }
            return true;
        }

        // ================= BANNERS DRAG/DROP & SORTING =================
        function attachOrderEventsBanners() {
            document.querySelectorAll('.move-up').forEach(function (btn) {
                btn.onclick = function (e) {
                    e.preventDefault();
                    let row = this.closest('tr');
                    let prevRow = row.previousElementSibling;
                    if (prevRow && !prevRow.querySelector('th')) { row.parentNode.insertBefore(row, prevRow); updateOrderBanners(); }
                };
            });
            document.querySelectorAll('.move-down').forEach(function (btn) {
                btn.onclick = function (e) {
                    e.preventDefault();
                    let row = this.closest('tr');
                    let nextRow = row.nextElementSibling;
                    if (nextRow) { row.parentNode.insertBefore(nextRow, row); updateOrderBanners(); }
                };
            });
        }
        function updateOrderBanners() {
            let rows = document.querySelectorAll('#gvBanners tbody tr');
            let orderArray = []; let index = 1;
            rows.forEach(function (row) {
                let label = row.querySelector('.sort-order-lbl');
                let hiddenId = row.querySelector('.banner-id');
                if (label && hiddenId) { label.innerText = index; orderArray.push(hiddenId.value); index++; }
            });
            let hiddenField = document.querySelector('[id$="hfNewOrder"]');
            if (hiddenField) hiddenField.value = orderArray.join(',');
        }

        // ================= COLLECTIONS DRAG/DROP & SORTING =================
        function attachOrderEventsCols() {
            document.querySelectorAll('.move-up-col').forEach(function (btn) {
                btn.onclick = function (e) {
                    e.preventDefault();
                    let row = this.closest('tr');
                    let prevRow = row.previousElementSibling;
                    if (prevRow && !prevRow.querySelector('th')) { row.parentNode.insertBefore(row, prevRow); updateOrderCols(); }
                };
            });
            document.querySelectorAll('.move-down-col').forEach(function (btn) {
                btn.onclick = function (e) {
                    e.preventDefault();
                    let row = this.closest('tr');
                    let nextRow = row.nextElementSibling;
                    if (nextRow) { row.parentNode.insertBefore(nextRow, row); updateOrderCols(); }
                };
            });
        }
        function updateOrderCols() {
            let rows = document.querySelectorAll('#gvCollections tbody tr');
            let orderArray = []; let index = 1;
            rows.forEach(function (row) {
                let label = row.querySelector('.sort-col-lbl');
                let hiddenId = row.querySelector('.col-id');
                if (label && hiddenId) { label.innerText = index; orderArray.push(hiddenId.value); index++; }
            });
            let hiddenField = document.querySelector('[id$="hfColOrder"]');
            if (hiddenField) hiddenField.value = orderArray.join(',');
        }

        // ================= AUTH CAROUSEL DRAG/DROP & SORTING =================
        function attachOrderEventsAuth() {
            document.querySelectorAll('.move-up-auth').forEach(function (btn) {
                btn.onclick = function (e) {
                    e.preventDefault();
                    let row = this.closest('tr');
                    let prevRow = row.previousElementSibling;
                    if (prevRow && !prevRow.querySelector('th')) { row.parentNode.insertBefore(row, prevRow); updateOrderAuth(); }
                };
            });
            document.querySelectorAll('.move-down-auth').forEach(function (btn) {
                btn.onclick = function (e) {
                    e.preventDefault();
                    let row = this.closest('tr');
                    let nextRow = row.nextElementSibling;
                    if (nextRow) { row.parentNode.insertBefore(nextRow, row); updateOrderAuth(); }
                };
            });
        }
        function updateOrderAuth() {
            let rows = document.querySelectorAll('#gvAuthCarousel tbody tr');
            let orderArray = []; let index = 1;
            rows.forEach(function (row) {
                let label = row.querySelector('.sort-auth-lbl');
                let hiddenId = row.querySelector('.auth-id');
                if (label && hiddenId) { label.innerText = index; orderArray.push(hiddenId.value); index++; }
            });
            let hiddenField = document.querySelector('[id$="hfAuthOrder"]');
            if (hiddenField) hiddenField.value = orderArray.join(',');
        }

        // ================= SHARED HELPER FUNCTIONS =================
        function previewImage(input, previewId, zoneId) {
            var preview = document.getElementById(previewId);
            var zone = document.getElementById(zoneId);
            if (input.files && input.files[0]) {
                var reader = new FileReader();
                reader.onload = function (e) {
                    preview.src = e.target.result;
                    preview.style.display = 'block';
                    if (zone) zone.classList.add('has-preview');
                };
                reader.readAsDataURL(input.files[0]);
            }
        }

        function initDragDrop(zoneId, inputId, previewId) {
            var zone = document.getElementById(zoneId);
            var input = document.getElementById(inputId);

            if (!zone || !input) return;

            zone.addEventListener('click', function (e) { if (e.target !== input) { input.click(); } });
            ['dragenter', 'dragover', 'dragleave', 'drop'].forEach(function (eventName) {
                zone.addEventListener(eventName, preventDefaults, false);
                document.body.addEventListener(eventName, preventDefaults, false);
            });
            ['dragenter', 'dragover'].forEach(function (eventName) { zone.addEventListener(eventName, highlight, false); });
            ['dragleave', 'drop'].forEach(function (eventName) { zone.addEventListener(eventName, unhighlight, false); });
            zone.addEventListener('drop', handleDrop, false);

            function preventDefaults(e) { e.preventDefault(); e.stopPropagation(); }
            function highlight() { zone.classList.add('dragover'); }
            function unhighlight() { zone.classList.remove('dragover'); }
            function handleDrop(e) {
                var dt = e.dataTransfer;
                var files = dt.files;
                if (files && files.length > 0) {
                    input.files = files;
                    var event = new Event('change');
                    input.dispatchEvent(event);
                }
            }
        }

        function initAllDragDrop() {
            initDragDrop('bannerDragDropZone', 'fileImagen', 'imgPreview');
            initDragDrop('colDragDropZone', 'fileColImagen', 'imgColPreview');
            initDragDrop('authDragDropZone', 'fileAuthImagen', 'imgAuthPreview');

            ['imgPreview', 'imgColPreview', 'imgAuthPreview'].forEach(function (imgId) {
                var img = document.getElementById(imgId);
                if (img) {
                    var zone = img.closest('.drag-drop-zone');
                    if (zone) {
                        if (!img.src || img.src.indexOf('#') !== -1 || img.style.display === 'none') {
                            zone.classList.remove('has-preview');
                            img.style.display = 'none';
                        } else {
                            zone.classList.add('has-preview');
                            img.style.display = 'block';
                        }
                    }
                }
            });
        }

        function initThemeToggle() {
            var themeToggle = document.getElementById('theme-toggle');
            if (themeToggle) {
                var themeIcon = themeToggle.querySelector('i');
                var isDark = document.body.classList.contains('dark-mode') || document.documentElement.classList.contains('dark-mode');
                if (isDark && themeIcon) { themeIcon.className = 'fas fa-sun'; }
                themeToggle.addEventListener('click', function (e) {
                    e.preventDefault();
                    var currentlyDark = document.body.classList.contains('dark-mode') || document.documentElement.classList.contains('dark-mode');
                    if (currentlyDark) {
                        document.body.classList.remove('dark-mode'); document.documentElement.classList.remove('dark-mode');
                        localStorage.setItem('theme', 'light'); if (themeIcon) themeIcon.className = 'fas fa-moon';
                    } else {
                        document.body.classList.add('dark-mode'); document.documentElement.classList.add('dark-mode');
                        localStorage.setItem('theme', 'dark'); if (themeIcon) themeIcon.className = 'fas fa-sun';
                    }
                });
            }
        }

        document.addEventListener('DOMContentLoaded', function () {
            attachOrderEventsBanners(); updateOrderBanners();
            attachOrderEventsCols(); updateOrderCols();
            attachOrderEventsAuth(); updateOrderAuth();
            initAllDragDrop();
            initThemeToggle();
        });

        if (typeof Sys !== 'undefined' && Sys.WebForms && Sys.WebForms.PageRequestManager) {
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
                attachOrderEventsBanners(); updateOrderBanners();
                attachOrderEventsCols(); updateOrderCols();
                attachOrderEventsAuth(); updateOrderAuth();
                initAllDragDrop();
            });
        }
    </script>
</body>
</html>