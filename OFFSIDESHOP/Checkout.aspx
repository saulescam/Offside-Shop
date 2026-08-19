<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Checkout.aspx.cs" Inherits="OFFSIDESHOP.Checkout" %>
<%@ Register Src="~/FooterControl.ascx" TagPrefix="uc" TagName="Footer" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1, shrink-to-fit=no" />
    <title>OffsideShop - Checkout</title>

    <link rel="icon" type="image/x-icon" href="assets/favicon.ico" />
    <script src="https://use.fontawesome.com/releases/v6.3.0/js/all.js" crossorigin="anonymous"></script>
    <link href="https://fonts.googleapis.com/css?family=Montserrat:400,700" rel="stylesheet" type="text/css" />
    <link href="https://fonts.googleapis.com/css?family=Roboto+Slab:400,100,300,700" rel="stylesheet" type="text/css" />
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.2.3/dist/css/bootstrap.min.css" rel="stylesheet" />

    <link href="css/styles.css" rel="stylesheet" />
    <link href="css/details.css" rel="stylesheet" />

    <!-- LEAFLET MAPS CDN INTEGRATION -->
    <link rel="stylesheet" href="https://unpkg.com/leaflet@1.9.4/dist/leaflet.css" />
    <script src="https://unpkg.com/leaflet@1.9.4/dist/leaflet.js"></script>

    <style>
        .user-menu-container { position: relative; display: flex; align-items: center; margin-left: auto; }
        .user-icon-btn { background: none; border: none; cursor: pointer; padding: 8px; color: #ffffff; transition: all 0.3s ease; display: flex; align-items: center; justify-content: center; width: 40px; height: 40px; border-radius: 50%; }
        .user-icon-btn:hover { color: #FFC800; background-color: rgba(255, 200, 0, 0.1); }
        .user-dropdown-menu { position: absolute; top: 50px; right: 0; background: #1a1a1a; border: 1px solid #FFC800; border-radius: 8px; min-width: 260px; box-shadow: 0 4px 12px rgba(0, 0, 0, 0.5); z-index: 1000; padding: 0; }
        .user-info { padding: 12px 16px; border-bottom: 1px solid #333333; }
        .user-fullname { margin: 0; color: #FFC800; font-weight: bold; font-size: 0.95rem; }
        .user-email { margin: 4px 0 0 0; color: #888888; font-size: 0.8rem; }
        .dropdown-content { display: flex; flex-direction: column; padding: 8px 0; }
        .dropdown-item { display: flex; align-items: center; gap: 10px; padding: 10px 16px; color: #ffffff; text-decoration: none; cursor: pointer; border: none; background: transparent; width: 100%; text-align: left; transition: all 0.2s; font-family: 'Montserrat', sans-serif; font-size: 0.95rem; }
        .dropdown-item:hover { background-color: #FFC800; color: #000000; }
        .dropdown-item i { font-size: 1rem; width: 20px; }
        .dropdown-item.btn-logout { border-top: 1px solid #333333; margin-top: 4px; padding-top: 10px; }
        .dropdown-item.btn-logout:hover { background-color: #D47A00 !important; }
        .badge { margin-left: auto; background-color: #D47A00; color: white; padding: 2px 6px; border-radius: 10px; font-size: 0.75rem; min-width: 18px; text-align: center; }

        .coupon-wrapper { margin-top: 20px; padding-top: 15px; border-top: 1px solid #E4E7ED; }
        .coupon-toggle { font-size: 0.85rem; color: #8D99AE; text-decoration: none; display: inline-block; transition: color 0.2s; }
        .coupon-toggle:hover { color: #D47A00; }
        .coupon-toggle i { margin-right: 5px; }
        .coupon-input-container { max-width: 250px; margin-top: 10px; }
        .coupon-input { text-transform: uppercase; font-size: 0.85rem; letter-spacing: 1px; box-shadow: none !important; border: 1px solid #E4E7ED; border-right: none; }
        .coupon-input:focus { border-color: #D47A00; }
        .btn-apply-coupon { font-size: 0.85rem; padding: 5px 15px; background: #fbfbfb; border: 1px solid #E4E7ED; color: #333; transition: all 0.2s; font-weight: 600; }
        .btn-apply-coupon:hover { background: #eee; border-color: #d3d9df; }
        .discount-row { color: #d9534f !important; }
        .discount-row strong { color: #d9534f !important; }

        /* Barra de búsqueda del mapa */
        .map-search-wrapper { position: relative; margin-bottom: 8px; }
        .map-search-input { width: 100%; padding: 9px 40px 9px 14px; border: 1.5px solid #E4E7ED; border-radius: 8px; font-family: 'Montserrat', sans-serif; font-size: 0.85rem; transition: border-color 0.2s; outline: none; }
        .map-search-input:focus { border-color: #D47A00; }
        .map-search-btn { position: absolute; right: 8px; top: 50%; transform: translateY(-50%); background: none; border: none; color: #888; cursor: pointer; font-size: 0.95rem; padding: 0; }
        .map-search-btn:hover { color: #D47A00; }
        .map-location-label { font-size: 0.78rem; color: #888; margin-top: 5px; min-height: 18px; transition: color 0.2s; }
        .map-location-label.found { color: #28a745; }
        .map-location-label.searching { color: #D47A00; }
        .map-location-label.error { color: #dc3545; }

        /* ===== LOADER SOLUTIONS ===== */
        #payment-loader {
            position: fixed;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            background-color: rgba(15, 23, 42, 0.85);
            backdrop-filter: blur(10px);
            -webkit-backdrop-filter: blur(10px);
            z-index: 9500; 
            display: none; 
            align-items: center;
            justify-content: center;
            color: #ffffff;
            font-family: 'Montserrat', sans-serif;
            text-align: center;
        }

        .swal2-container {
            z-index: 100000 !important;
        }

        #payment-loader .loader-content {
            background: rgba(30, 41, 59, 0.7); 
            border: 1px solid rgba(255, 200, 0, 0.2); 
            border-radius: 16px;
            padding: 40px 30px;
            max-width: 480px;
            width: 90%;
            box-shadow: 0 20px 25px -5px rgba(0, 0, 0, 0.5), 0 10px 10px -5px rgba(0, 0, 0, 0.5);
            animation: loaderFadeInScale 0.4s ease-out;
        }

        @keyframes loaderFadeInScale {
            from { opacity: 0; transform: scale(0.95); }
            to { opacity: 1; transform: scale(1); }
        }

        #payment-loader .loader-icon-wrapper {
            position: relative;
            width: 100px;
            height: 100px;
            margin: 0 auto 25px auto;
            display: flex;
            align-items: center;
            justify-content: center;
        }

        #payment-loader .loader-spinner {
            font-size: 80px;
            color: #FFC800; 
            opacity: 0.9;
        }

        #payment-loader .loader-lock {
            position: absolute;
            font-size: 28px;
            color: #ffffff;
            animation: lockPulse 2s infinite ease-in-out;
        }

        @keyframes lockPulse {
            0%, 100% { transform: scale(1); opacity: 0.8; color: #ffffff; }
            50% { transform: scale(1.1); opacity: 1; color: #FFC800; }
        }

        #payment-loader .loader-title {
            color: #ffffff;
            font-size: 1.5rem;
            font-weight: 700;
            margin-bottom: 12px;
            letter-spacing: 0.5px;
        }

        #payment-loader .loader-text {
            color: #cbd5e1; 
            font-size: 0.95rem;
            line-height: 1.6;
            margin-bottom: 0;
        }

        #payment-loader .loader-security-badge {
            margin-top: 25px;
            display: inline-flex;
            align-items: center;
            gap: 8px;
            background: rgba(255, 200, 0, 0.1);
            border: 1px solid rgba(255, 200, 0, 0.3);
            padding: 6px 16px;
            border-radius: 50px;
            color: #FFC800;
            font-size: 0.8rem;
            font-weight: 600;
            letter-spacing: 0.5px;
            text-transform: uppercase;
        }

        .map-container-wrapper { position: relative; width: 100%; }

        .map-loader-overlay {
            position: absolute;
            top: 0; left: 0; width: 100%; height: 100%;
            background-color: #0f172a;
            z-index: 999; 
            display: flex;
            align-items: center; justify-content: center;
            border-radius: 12px; border: 2px solid #E4E7ED;
            transition: opacity 0.5s ease, visibility 0.5s ease;
        }

        .map-loader-overlay.fade-out { opacity: 0; visibility: hidden; }
        .map-loader-content { text-align: center; color: #ffffff; font-family: 'Montserrat', sans-serif; }

        .map-spinner-ring {
            display: inline-block; position: relative;
            width: 64px; height: 64px; margin-bottom: 15px;
        }

        .map-spinner-ring div {
            box-sizing: border-box; display: block; position: absolute;
            width: 50px; height: 50px; margin: 7px;
            border: 4px solid #FFC800; border-radius: 50%;
            animation: mapSpinnerRingRotate 1.2s cubic-bezier(0.5, 0, 0.5, 1) infinite;
            border-color: #FFC800 transparent transparent transparent;
        }

        .map-spinner-ring div:nth-child(1) { animation-delay: -0.45s; }
        .map-spinner-ring div:nth-child(2) { animation-delay: -0.3s; }
        .map-spinner-ring div:nth-child(3) { animation-delay: -0.15s; }

        @keyframes mapSpinnerRingRotate {
            0% { transform: rotate(0deg); }
            100% { transform: rotate(360deg); }
        }

        .map-loader-text {
            font-size: 0.85rem; font-weight: 600; letter-spacing: 0.5px; color: #cbd5e1;
        }
    </style>

    <script type="text/javascript">
        function toggleUserMenu(button) {
            const container = button.closest('.user-menu-container');
            if (!container) return;
            const menu = container.querySelector('.dynamic-dropdown');
            if (!menu) return;
            if (menu.style.display === 'block') {
                menu.style.display = 'none';
            } else {
                cerrarTodosLosMenus();
                menu.style.display = 'block';
            }
        }
        function cerrarTodosLosMenus() {
            const menus = document.querySelectorAll('.dynamic-dropdown');
            menus.forEach(m => m.style.display = 'none');
        }
        document.onclick = function (event) {
            const container = event.target.closest('.user-menu-container');
            if (!container) cerrarTodosLosMenus();
        };
    </script>
</head>

<body id="page-top">
    <!-- FULL-SCREEN SECURE PAYMENT LOADER OVERLAY -->
    <div id="payment-loader" style='<%= ShowPaymentLoader ? "display: flex;" : "display: none;" %>'>
        <div class="loader-content">
            <div class="loader-icon-wrapper">
                <i class="fas fa-circle-notch fa-spin loader-spinner"></i>
                <i class="fas fa-lock loader-lock"></i>
            </div>
            <h4 class="loader-title"><%= Resources.Strings.Checkout_LoaderTitle %></h4>
            <p class="loader-text"><%= Resources.Strings.Checkout_LoaderText %></p>
            <div class="loader-security-badge">
                <i class="fas fa-shield-alt"></i> <%= Resources.Strings.Checkout_LoaderSecurity %>
            </div>
        </div>
    </div>

    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
    <script>
        var isPaymentSubmitting = false;
        var isFormSubmitting = false;

        function validarCheckout() {
            if (isFormSubmitting) {
                return true;
            }

            var nombre = document.getElementById('<%= txtName.ClientID %>').value.trim();
            var apellido = document.getElementById('<%= txtLastName.ClientID %>').value.trim();
            var email = document.getElementById('<%= txtEmail.ClientID %>').value.trim();
            var address = document.getElementById('<%= txtAddress.ClientID %>').value.trim();
            var tel = document.getElementById('<%= txtTel.ClientID %>').value.trim();
            var city = document.getElementById('<%= ddlCity.ClientID %>').value;

            var lat = document.getElementById('<%= hfLatitude.ClientID %>').value;
            var lng = document.getElementById('<%= hfLongitude.ClientID %>').value;
            var tel = document.getElementById('<%= txtTel.ClientID %>').value.trim();
            var telRegex = /^[0-9]{8}$/;

            if (tel === '' || !telRegex.test(tel)) {
                Swal.fire('<% =GetGlobalResourceObject("Strings", "Alert_ErrorTitle") %>', '<%= GetGlobalResourceObject("Strings", "Alert_Checkout_InvalidPhoneText") %>', 'error');
                return false;
            }

            if (nombre === '' || apellido === '' || email === '' || address === '' || tel === '' || city === '') {
                Swal.fire('<% =GetGlobalResourceObject("Strings", "Alert_ErrorTitle") %>', '<%= GetGlobalResourceObject("Strings", "Alert_Checkout_FieldsRequiredText") %>', 'error');
                return false;
            }

            if (nombre.length > 50 || apellido.length > 50) {
                Swal.fire('<% =GetGlobalResourceObject("Strings", "Alert_ErrorTitle") %>', 'El nombre y el apellido no pueden exceder los 50 caracteres.', 'error');
                return false;
            }

            if (!lat || !lng || lat === "" || lng === "") {
                Swal.fire('<% =GetGlobalResourceObject("Strings", "Alert_Checkout_LocationRequiredTitle") %>', '<%= GetGlobalResourceObject("Strings", "Alert_Checkout_LocationRequiredText") %>', 'warning');
                return false;
            }

            var payment = document.querySelector('input[name="payment"]:checked');
            if (!payment) {
                Swal.fire('<% =GetGlobalResourceObject("Strings", "Alert_ErrorTitle") %>', '<%= GetGlobalResourceObject("Strings", "Alert_Checkout_PaymentRequiredText") %>', 'error');
                return false;
            }

            var terms = document.getElementById('terms').checked;
            if (!terms) {
                Swal.fire('<% =GetGlobalResourceObject("Strings", "Alert_ErrorTitle") %>', '<%= GetGlobalResourceObject("Strings", "Alert_Checkout_TermsRequiredText") %>', 'error');
                return false;
            }

            if (payment && payment.value === 'Cash') {
                isPaymentSubmitting = true;

                var loader = document.getElementById('payment-loader');
                if (loader) {
                    loader.style.display = 'flex';
                }

                isFormSubmitting = true;
                setTimeout(function () {
                    var btn = document.getElementById('<%= btnPlaceOrder.ClientID %>');
                    if (btn) {
                        btn.click();
                    }
                }, 800);

                return false;
            }

            return true;
        }
    </script>

    <form runat="server">
        <asp:ScriptManager ID="ScriptManager1" runat="server" />

        <asp:HiddenField ID="hfLatitude" runat="server" />
        <asp:HiddenField ID="hfLongitude" runat="server" />
        <asp:HiddenField ID="hfUserDefaultLat" runat="server" />
        <asp:HiddenField ID="hfUserDefaultLng" runat="server" />

        <input type="hidden" id="vw_monto" value="0.00" />
        <input type="hidden" id="DescCarrito" value="OffsideShop Jersey Collector Settlement Order" />

        <nav class="navbar navbar-expand-lg navbar-dark fixed-top" id="mainNav">
            <div class="container">
                <a class="navbar-brand" href="#page-top">
                    <img src="assets/img/offsideshop_logo_white_letras.png" alt="OffsideShop Logo" style="max-height: 45px; width: auto;" />
                </a>
                <asp:LinkButton ID="btnLanguageToggle" runat="server" OnClick="btnLanguageToggle_Click" CssClass="lang-switcher" style="color: #fff; text-decoration: none; font-weight: bold; margin-left: 10px; margin-right: auto;">EN / ES</asp:LinkButton>

                <button class="navbar-toggler" type="button" data-bs-toggle="collapse" data-bs-target="#navbarResponsive">
                    <span class="navbar-toggler-icon"></span>
                </button>

                <div class="collapse navbar-collapse" id="navbarResponsive">
                    
                    <asp:PlaceHolder ID="phNavbarGuest" runat="server">
                        <div class="user-menu-container">
                            <button type="button" class="user-icon-btn" onclick="toggleUserMenu(this)">
                                <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                                    <circle cx="12" cy="8" r="4"></circle>
                                    <path d="M 6 20c0-4 2.5-6 6-6s6 2 6 6"></path>
                                </svg>
                            </button>
                            <div class="user-dropdown-menu dynamic-dropdown" style="display: none;">
                                <div class="dropdown-content">
                                    <a href="Login.aspx" class="dropdown-item"><i class="fas fa-sign-in-alt"></i><%= Resources.Strings.Nav_Login %></a>
                                    <a href="SignUp.aspx" class="dropdown-item"><i class="fas fa-user-plus"></i><%= Resources.Strings.Nav_SignUp %></a>
                                    <asp:Button ID="Button1" runat="server" CssClass="dropdown-item btn-logout" Text="<%$ Resources:Strings, Nav_BackToShop %>" OnClick="btnbackshop_Click" />
                                </div>
                            </div>
                        </div>
                    </asp:PlaceHolder>

                    <asp:PlaceHolder ID="phNavbarUser" runat="server">
                        <div class="user-menu-container">
                            <asp:UpdatePanel ID="upPerfil" runat="server" UpdateMode="Conditional">
                                <ContentTemplate>
                                    <button type="button" class="user-icon-btn" onclick="toggleUserMenu(this)">
                                        <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                                            <circle cx="12" cy="8" r="4"></circle>
                                            <path d="M 6 20c0-4 2.5-6 6-6s6 2 6 6"></path>
                                        </svg>
                                    </button>
                                    <div id="userDropdownMenuUser" class="user-dropdown-menu dynamic-dropdown" style="display: none;">
                                        <div class="user-info">
                                            <p class="user-fullname"><asp:Label ID="lblFullName" runat="server" Text="Cargando..."></asp:Label></p>
                                            <p class="user-email"><asp:Label ID="lblUserEmail" runat="server" Text=""></asp:Label></p>
                                        </div>
                                        <div class="dropdown-content">
                                            <asp:LinkButton ID="btnMyOrders" runat="server" CssClass="dropdown-item" OnClick="btnMyOrders_Click">
                                                <i class="fas fa-clipboard-list"></i> <%= Resources.Strings.Nav_MyOrders %>
                                            </asp:LinkButton>
                                            <asp:Button ID="btnbackshop" runat="server" CssClass="dropdown-item btn-logout" Text="<%$ Resources:Strings, Nav_BackToShop %>" OnClick="btnbackshop_Click" />
                                            <asp:Button ID="btncerrar" runat="server" CssClass="dropdown-item btn-logout" Text="<%$ Resources:Strings, Nav_LogOut %>" OnClick="btncerrar_Click" />
                                        </div>
                                    </div>
                                </ContentTemplate>
                            </asp:UpdatePanel>
                        </div>
                    </asp:PlaceHolder>
                </div>
            </div>
        </nav>

        <div style="margin-top: 120px;"></div>

        <div class="section checkout-page">
            <div class="container">
                <div class="row justify-content-center">

                    <div class="col-12 col-md-6 mt-4" style="max-width: 550px;">
                        <div class="billing-details">
                            <div class="section-title">
                                <h3 class="title"><%= Resources.Strings.Checkout_BillingTitle %></h3>
                            </div>
                            <div class="form-group">
                                <asp:TextBox ID="txtName" runat="server" CssClass="input" placeholder="<%$ Resources:Strings, Checkout_FirstName %>" MaxLength="50" />
                            </div>
                            <div class="form-group">
                                <asp:TextBox ID="txtLastName" runat="server" CssClass="input" placeholder="<%$ Resources:Strings, Checkout_LastName %>" MaxLength="50" />
                            </div>
                            <div class="form-group">
                                <asp:TextBox ID="txtEmail" runat="server" CssClass="input" placeholder="<%$ Resources:Strings, Checkout_Email %>" />
                            </div>
                            <div class="form-group">
                                <asp:TextBox ID="txtAddress" runat="server" CssClass="input" placeholder="<%$ Resources:Strings, Checkout_Address %>" MaxLength="200" />
                            </div>

                            <asp:UpdatePanel ID="upLocation" runat="server">
                                <ContentTemplate>
                                    <div class="form-group">
                                        <label for="ddlCity"><%= Resources.Strings.Checkout_Department %></label>
                                        <asp:DropDownList ID="ddlCity" runat="server" CssClass="input" AutoPostBack="true" OnSelectedIndexChanged="ddlCity_SelectedIndexChanged"></asp:DropDownList>
                                    </div>
                                    <div class="form-group">
                                        <label for="ddlMunicipality"><%= Resources.Strings.Checkout_Municipality %></label>
                                        <asp:DropDownList ID="ddlMunicipality" runat="server" CssClass="input" Enabled="false" AutoPostBack="true" OnSelectedIndexChanged="ddlMunicipality_SelectedIndexChanged"></asp:DropDownList>
                                    </div>
                                    <div class="form-group">
                                        <label for="ddlDistrict"><%= Resources.Strings.Checkout_District %></label>
                                        <asp:DropDownList ID="ddlDistrict" runat="server" CssClass="input" Enabled="false"></asp:DropDownList>
                                    </div>
                                </ContentTemplate>
                                <Triggers>
                                    <asp:AsyncPostBackTrigger ControlID="ddlCity" EventName="SelectedIndexChanged" />
                                    <asp:AsyncPostBackTrigger ControlID="ddlMunicipality" EventName="SelectedIndexChanged" />
                                </Triggers>
                            </asp:UpdatePanel>

                            <div class="form-group mt-3">
                                <asp:TextBox ID="txtTel" runat="server" CssClass="input"
                                    placeholder="+503 12345678"
                                    MaxLength="8"
                                    inputmode="numeric"
                                    oninput="this.value = this.value.replace(/[^0-9]/g, '');" />
                            </div>

                            <% if (ShowMap)
                                { %>
                            <!-- INTERACTIVE MAP PANEL FOR DELIVERY LOCATION -->
                            <div class="form-group mt-4">
                                <label class="form-label font-weight-bold" style="font-weight: 700; color: #111111;"><%= Resources.Strings.Checkout_ConfirmLocation %> <span class="text-warning">*</span></label>
                                <div class="map-search-wrapper">
                                    <input type="text" id="mapSearchInput" class="map-search-input" placeholder="<%= Resources.Strings.Checkout_SearchMapPlaceholder %>" />
                                    <button type="button" class="map-search-btn" onclick="searchMapAddress()" title="Search">
                                        <i class="fas fa-search"></i>
                                    </button>
                                </div>
                                <div class="map-container-wrapper">
                                    <div id="map-canvas" style="height: 320px; width: 100%; border-radius: 12px; border: 2px solid #E4E7ED; z-index: 1;"></div>
                                    <div id="map-loader" class="map-loader-overlay">
                                        <div class="map-loader-content">
                                            <div class="map-spinner-ring">
                                                <div></div><div></div><div></div><div></div>
                                            </div>
                                            <div class="map-loader-text"><i class="fas fa-map-marked-alt text-warning me-2"></i><%= Resources.Strings.Checkout_LoadingMap %></div>
                                        </div>
                                    </div>
                                </div>
                                <div id="mapLocationLabel" class="map-location-label"><i class="fas fa-map-pin text-danger me-1"></i><%= Resources.Strings.Checkout_MapInstructions %></div>
                            </div>
                            <% } %>
                        </div>

                        <div class="order-notes mt-3">
                            <asp:TextBox ID="txtNotes" runat="server" CssClass="input"
                                placeholder="<%$ Resources:Strings, Checkout_OrderNotes %>"
                                TextMode="MultiLine" Rows="3"
                                MaxLength="200"
                                oninput="updateNotesCounter();" />
                            <div class="text-end mt-1">
                                <small id="notesCounter" class="text-muted" style="font-size: 0.8rem; font-weight: 600;">0/200</small>
                            </div>
                        </div>
                    </div>

                    <div class="col-12 col-md-6 mt-4" style="max-width: 650px;">
                        <div class="order-details">
                            <div class="section-title text-center">
                                <h3 class="title"><%= Resources.Strings.Checkout_YourOrderTitle %></h3>
                            </div>

                                <div class="order-summary">
                                    <div class="order-col">
                                    <div><strong><%= Resources.Strings.Checkout_HeaderProduct %></strong></div>
                                    <div><strong><%= Resources.Strings.Checkout_HeaderTotal %></strong></div>
                                </div>

                                <div class="order-products" id="orderProducts" runat="server"></div>

                                <asp:UpdatePanel ID="upTotalsSummary" runat="server">
                                    <ContentTemplate>
                                        <asp:PlaceHolder ID="phDiscountRow" runat="server" Visible="false">
                                            <div class="order-col discount-row">
                                                <div><%= Resources.Strings.Checkout_DiscountApplied %></div>
                                                <div><strong><asp:Label ID="lblOrderDiscount" runat="server"></asp:Label></strong></div>
                                            </div>
                                        </asp:PlaceHolder>

                                        <div class="order-col">
                                            <div>&nbsp;<%= Resources.Strings.Checkout_Shipping %></div>
                                            <div><strong><asp:Label ID="lblOrderShipping" runat="server" Text="$3.50"></asp:Label></strong></div>
                                        </div>
                                        <div class="order-col">
                                            <div><strong><%= Resources.Strings.Checkout_HeaderTotal %></strong></div>
                                            <div><strong class="order-total"><asp:Label ID="lblOrderTotal" runat="server"></asp:Label></strong></div>
                                        </div>

                                        <div class="coupon-wrapper">
                                            <a data-bs-toggle="collapse" href="#collapseCoupon" role="button" aria-expanded="false" aria-controls="collapseCoupon" class="coupon-toggle">
                                                <i class="fas fa-tag"></i> <%= Resources.Strings.Checkout_HaveCoupon %>
                                            </a>
                                            <div class="collapse" id="collapseCoupon">
                                                <div class="input-group input-group-sm coupon-input-container">
                                                    <asp:TextBox ID="txtCouponCode" runat="server" CssClass="form-control coupon-input" placeholder="XXXXXXXXXXXX" MaxLength="12"></asp:TextBox>
                                                    <asp:TextBox ID="txtHiddenTotalSync" runat="server" CssClass="d-none dynamic-total-sync"></asp:TextBox>
                                                    <asp:Button ID="btnApplyCoupon" runat="server" Text="<%$ Resources:Strings, Checkout_ApplyCoupon %>" CssClass="btn btn-apply-coupon" OnClick="btnApplyCoupon_Click" />
                                                </div>
                                                <asp:Label ID="lblCouponMessage" runat="server" CssClass="small mt-1 d-block" Visible="false"></asp:Label>
                                            </div>
                                        </div>

                                        <asp:HiddenField ID="hfTotalAmount" runat="server" Value="0.00" />
                                    </ContentTemplate>
                                    <Triggers>
                                        <asp:AsyncPostBackTrigger ControlID="ddlCity" EventName="SelectedIndexChanged" />
                                        <asp:AsyncPostBackTrigger ControlID="ddlMunicipality" EventName="SelectedIndexChanged" />
                                    </Triggers>
                                </asp:UpdatePanel>
                            </div>

                            <div class="payment-method mt-4">
                                <!-- Cash on Delivery -->
                                <div class="input-radio mt-2">
                                    <input type="radio" name="payment" id="payment2" runat="server" value="Cash" onclick="togglePaymentCaptions(this)">
                                    <label for="payment2"><span></span><%= Resources.Strings.Checkout_PayCash %></label>
                                    <div class="caption" id="caption-2" style="display: none;">
                                        <p><%= Resources.Strings.Checkout_PayCashDesc %></p>
                                    </div>
                                </div>

                                <!-- Paypal System -->
                                <div class="input-radio mt-2">
                                    <input type="radio" name="payment" id="payment3" runat="server" value="PayPal" onclick="togglePaymentCaptions(this)">
                                    <label for="payment3"><span></span><%= Resources.Strings.Checkout_PayPaypal %> <i class="fab fa-cc-paypal text-primary ms-1"></i></label>
                                    <div class="caption" id="caption-3" style="display: none;">
                                        <p><%= Resources.Strings.Checkout_PayPaypalDesc %></p>
                                    </div>
                                </div>
                                <!-- Virtual Wallet System -->
                                <div class="input-radio mt-2">
                                    <input type="radio" name="payment" id="payment4" runat="server" value="VirtualWallet" onclick="togglePaymentCaptions(this)">
                                    <label for="payment4"><span></span><%= Resources.Strings.Checkout_PayWallet %> <i class="fas fa-wallet text-success ms-1"></i></label>

                                    <div class="caption" id="caption-4" style="display: none;">
                                        <p><%= Resources.Strings.Checkout_PayWalletDesc %></p>

                                        <!-- Contenedor del Checkout de la Billetera (Sin display:none fijo) -->
                                        <div id="virtual-wallet-checkout" style="text-align: center; margin-top: 15px;">

                                            <!-- EL SCRIPT DEBE IR AQUÍ para que el botón se dibuje dentro de este panel -->
                                            <script
                                                src="http://192.168.3.27:8000/api/v1/widget/checkout.js"
                                                data-vw-widget="true"
                                                data-client-id="pk_sandbox_7hYP5Mdfg9cdLxH20XC5AmFR"
                                                data-secret-key="sk_live_P5iFosZtwGjbc4Kk6iBSBrgt"
                                                data-amount-id="vw_monto"
                                                data-desc-id="DescCarrito">
                                            </script>

                                        </div>
                                    </div>
                                </div>

                                <div class="input-checkbox mt-3">
                                    <input type="checkbox" id="terms">
                                <label for="terms"><span></span><%= Resources.Strings.Checkout_TermsText1 %> <a href="#"><%= Resources.Strings.Checkout_TermsText2 %></a></label>
                            </div>

                                <asp:HiddenField ID="hfTransactionID" runat="server" />
                                <asp:Button ID="btnConfirmPayPalPayment" runat="server" Style="display: none;" OnClick="btnConfirmPayPalPayment_Click" />

                                <!-- BOTÓN DE CONFIRMACIÓN DE LA BILLETERA VIRTUAL PARA EL BACKEND (C#) -->
                                <asp:Button ID="btnConfirmWalletPayment" runat="server" Style="display: none;" OnClick="btnConfirmWalletPayment_Click" />
                                <div class="mt-4">
                                    <asp:Button ID="btnPlaceOrder" runat="server" Text="<%$ Resources:Strings, Checkout_PlaceOrder %>" CssClass="primary-btn order-submit w-100" Style="display: none !important;" OnClick="btnPlaceOrder_Click" OnClientClick="return validarCheckout();" />
                                    <div id="paypal-button-container" class="mt-3" style="display: none !important;"></div>
                                </div>

                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <script type="text/javascript">
            function updateNotesCounter() {
                var txt = document.getElementById('<%= txtNotes.ClientID %>');
                var counter = document.getElementById('notesCounter');

                if (txt && counter) {
                    if (txt.value.length > 200) {
                        txt.value = txt.value.substring(0, 200);
                    }
                    counter.innerText = txt.value.length + '/200';
                }
            }

            document.addEventListener('DOMContentLoaded', function () {
                updateNotesCounter();
            });

            if (typeof Sys !== 'undefined' && Sys.WebForms && Sys.WebForms.PageRequestManager) {
                Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
                    updateNotesCounter();
                });
            }

            function sincronizarMontosBilletera() {
                var hfTotal = document.getElementById('<%= hfTotalAmount.ClientID %>');
                var walletAmountInput = document.getElementById('vw_monto');
                if (hfTotal && walletAmountInput) {
                    walletAmountInput.value = hfTotal.value;
                }
            }

            function togglePaymentCaptions(radioElement) {
                if (!radioElement) return;

                var cap2 = document.getElementById('caption-2');
                var cap3 = document.getElementById('caption-3');
                var cap4 = document.getElementById('caption-4');

                if (cap2) cap2.style.display = 'none';
                if (cap3) cap3.style.display = 'none';
                if (cap4) cap4.style.display = 'none';

                var btnNormal = document.getElementById('<%= btnPlaceOrder.ClientID %>');
                var btnPayPal = document.getElementById('paypal-button-container');

                if (btnNormal) btnNormal.style.setProperty('display', 'none', 'important');
                if (btnPayPal) btnPayPal.style.setProperty('display', 'none', 'important');

                if (radioElement.id.indexOf('payment2') !== -1) {
                    if (cap2) cap2.style.display = 'block';
                    if (btnNormal) btnNormal.style.setProperty('display', 'block', 'important');
                }
                else if (radioElement.id.indexOf('payment3') !== -1) {
                    if (cap3) cap3.style.display = 'block';
                    if (btnPayPal) btnPayPal.style.setProperty('display', 'block', 'important');
                }
                else if (radioElement.id.indexOf('payment4') !== -1) {
                    if (cap4) cap4.style.display = 'block';
                    if (btnNormal) btnNormal.style.setProperty('display', 'none', 'important');
                    if (btnPayPal) btnPayPal.style.setProperty('display', 'none', 'important');
                    sincronizarMontosBilletera();
                }
            }
        </script>

        <script src="https://www.paypal.com/sdk/js?client-id=AejQEmRXV3PTfVnwchx6ti6AlPsYbETlZgM4AtfUXa2IO4AykiJkUtL6wPJcafyn5kzlagVr4fH60nyH&currency=USD"></script>

        <script type="text/javascript">
                window.addEventListener('load', function () {
                    sincronizarMontosBilletera();

                    // ================================================================
                    // ESCUCHADOR ROBUSTO PARA LA BILLETERA VIRTUAL
                    // ================================================================
                    window.addEventListener('message', function (event) {
                        var data = event.data;

                        // Intentar parsear si viene como JSON string
                        if (typeof data === 'string') {
                            try { data = JSON.parse(data); } catch (e) { }
                        }

                        var isSuccess = false;
                        var txId = "";

                        // Validar éxito (soporta strings directos o campos status/type dentro del objeto JSON)
                        if (data === 'vw-payment-success' || data === 'success') {
                            isSuccess = true;
                        } else if (typeof data === 'object' && data !== null) {
                            if (data.status === 'success' || data.status === 'paid' || data.status === 'COMPLETED' || data.type === 'SUCCESS') {
                                isSuccess = true;
                            }
                            txId = data.transaction_id || data.transactionId || data.id || data.tx_id || "";
                        }

                        if (isSuccess) {
                            if (txId) {
                                document.getElementById('<%= hfTransactionID.ClientID %>').value = txId;
                            }

                            var loader = document.getElementById('payment-loader');
                            if (loader) loader.style.display = 'flex';
                            
                            isPaymentSubmitting = true;

                            setTimeout(function () {
                                var btnWallet = document.getElementById('<%= btnConfirmWalletPayment.ClientID %>');
                                if (btnWallet) btnWallet.click();
                            }, 800);
                        }
                    });

                    if (typeof Sys !== 'undefined' && Sys.WebForms && Sys.WebForms.PageRequestManager) {
                        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
                            sincronizarMontosBilletera();
                            var checkedRadio = document.querySelector('input[name="payment"]:checked');
                        if (checkedRadio) {
                            togglePaymentCaptions(checkedRadio);
                        }
                    });
                }

                paypal.Buttons({
                    onClick: function (data, actions) {
                        if (!validarCheckout()) {
                            return actions.reject();
                        }
                        return actions.resolve();
                    },
                    createOrder: function (data, actions) {
                        var totalValue = document.getElementById('<%= hfTotalAmount.ClientID %>').value;
                        return actions.order.create({
                            purchase_units: [{
                                amount: {
                                    value: totalValue
                                }
                            }]
                        });
                    },
                    onApprove: function (data, actions) {
                        return actions.order.capture().then(function (details) {
                            var trueCaptureId = details.purchase_units[0].payments.captures[0].id;
                            document.getElementById('<%= hfTransactionID.ClientID %>').value = trueCaptureId;

                            var loader = document.getElementById('payment-loader');
                            if (loader) {
                                loader.style.display = 'flex';
                            }
                            isPaymentSubmitting = true;
                            setTimeout(function () {
                                document.getElementById('<%= btnConfirmPayPalPayment.ClientID %>').click();
                            }, 800);
                        });
                    },
                    onCancel: function (data) {
                        Swal.fire('<%= GetGlobalResourceObject("Strings", "Alert_Checkout_CancelledTitle") %>', '<%= GetGlobalResourceObject("Strings", "Alert_Checkout_CancelledText") %>', 'warning');
                    },
                    onError: function (err) {
                        Swal.fire('<%= GetGlobalResourceObject("Strings", "Alert_ErrorTitle") %>', '<%= GetGlobalResourceObject("Strings", "Alert_Checkout_ProcessingErrorText") %>', 'error');
                    }
                }).render('#paypal-button-container');

                var btnNormal = document.getElementById('<%= btnPlaceOrder.ClientID %>');
                var btnPayPal = document.getElementById('paypal-button-container');
                var checkedRadio = document.querySelector('input[name="payment"]:checked');

                if (checkedRadio) {
                    togglePaymentCaptions(checkedRadio);
                } else {
                    if (btnNormal) btnNormal.style.setProperty('display', 'none', 'important');
                    if (btnPayPal) btnPayPal.style.setProperty('display', 'none', 'important');
                }

                var mainForm = document.forms[0] || document.getElementById('Form1');
                if (mainForm) {
                    mainForm.addEventListener('submit', function (e) {
                        if (typeof isPaymentSubmitting === 'undefined' || !isPaymentSubmitting) {
                            return;
                        }

                        if (typeof Page_ClientValidate === 'function') {
                            if (!Page_ClientValidate()) {
                                return; 
                            }
                        }

                        var loader = document.getElementById('payment-loader');
                        if (loader) {
                            loader.style.display = 'flex';
                        }

                        var submitBtn = document.getElementById('<%= btnPlaceOrder.ClientID %>');
                        var paypalContainer = document.getElementById('paypal-button-container');

                        setTimeout(function () {
                            if (submitBtn) submitBtn.disabled = true;
                            if (paypalContainer) paypalContainer.style.pointerEvents = 'none';
                        }, 50);
                    });
                }
            });
            function isNumberKey(evt) {
                var charCode = (evt.which) ? evt.evt.which : evt.keyCode;
                if (charCode > 31 && (charCode < 48 || charCode > 57)) {
                    return false;
                }
                return true;
            }
            </script>

        <!-- MAPA INTELIGENTE CON GEOCODING CLIENT-SIDE -->
        <script type="text/javascript">
            var checkoutMap = null;
            var deliveryMarker = null;

            function hideMapLoader() {
                var loader = document.getElementById('map-loader');
                if (loader) {
                    loader.classList.add('fade-out');
                    setTimeout(function () {
                        if (loader.parentNode) {
                            loader.parentNode.removeChild(loader);
                        }
                    }, 500);
                }
            }

            document.addEventListener('DOMContentLoaded', function () {
                var mapElement = document.getElementById('map-canvas');
                if (!mapElement) return;

                var latField = document.getElementById('<%= hfLatitude.ClientID %>');
                var lngField = document.getElementById('<%= hfLongitude.ClientID %>');
                var defaultLat = document.getElementById('<%= hfUserDefaultLat.ClientID %>').value;
                var defaultLng = document.getElementById('<%= hfUserDefaultLng.ClientID %>').value;

                var startLat = defaultLat ? parseFloat(defaultLat) : 13.6929;
                var startLng = defaultLng ? parseFloat(defaultLng) : -89.2182;
                var currentZoom = defaultLat ? 16 : 11;

                latField.value = startLat;
                lngField.value = startLng;

                checkoutMap = L.map('map-canvas').setView([startLat, startLng], currentZoom);

                var mapTiles = L.tileLayer('https://{s}.basemaps.cartocdn.com/rastertiles/voyager/{z}/{x}/{y}{r}.png', {
                    attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors &copy; <a href="https://carto.com/attributions">CARTO</a>',
                    subdomains: 'abcd',
                    maxZoom: 20
                });
                mapTiles.addTo(checkoutMap);

                mapTiles.on('load', function () {
                    hideMapLoader();
                });

                setTimeout(hideMapLoader, 4000);

                var pinIcon = L.icon({
                    iconUrl: 'https://raw.githubusercontent.com/pointhi/leaflet-color-markers/master/img/marker-icon-2x-blue.png',
                    shadowUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.9.4/images/marker-shadow.png',
                    iconSize: [25, 41],
                    iconAnchor: [12, 41],
                    popupAnchor: [1, -34],
                    shadowSize: [41, 41]
                });

                deliveryMarker = L.marker([startLat, startLng], { draggable: true, icon: pinIcon })
                    .addTo(checkoutMap)
                    .bindPopup('<b><i class="fas fa-home"></i> Delivery Point</b><br>Drag to adjust exact location')
                    .openPopup();

                deliveryMarker.on('dragend', function () {
                    var pos = deliveryMarker.getLatLng();
                    latField.value = pos.lat.toFixed(7);
                    lngField.value = pos.lng.toFixed(7);
                    checkoutMap.panTo(pos);
                    reverseGeocode(pos.lat, pos.lng);
                });

                checkoutMap.on('click', function (e) {
                    deliveryMarker.setLatLng(e.latlng);
                    latField.value = e.latlng.lat.toFixed(7);
                    lngField.value = e.latlng.lng.toFixed(7);
                    checkoutMap.panTo(e.latlng);
                    reverseGeocode(e.latlng.lat, e.latlng.lng);
                });

                hookDropdownEvents();

                if (typeof Sys !== 'undefined' && Sys.WebForms && Sys.WebForms.PageRequestManager) {
                    Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
                        hookDropdownEvents();
                    });
                }

                var inp = document.getElementById('mapSearchInput');
                if (inp) {
                    inp.addEventListener('keydown', function (e) {
                        if (e.key === 'Enter') { e.preventDefault(); searchMapAddress(); }
                    });
                }
            });

            function hookDropdownEvents() {
                var ddlCity = document.getElementById('<%= ddlCity.ClientID %>');
                var ddlMun = document.getElementById('<%= ddlMunicipality.ClientID %>');
                if (ddlCity) { ddlCity.removeEventListener('change', onLocationMenuChange); ddlCity.addEventListener('change', onLocationMenuChange); }
                if (ddlMun) { ddlMun.removeEventListener('change', onLocationMenuChange); ddlMun.addEventListener('change', onLocationMenuChange); }
            }

            function onLocationMenuChange() {
                var ddlCity = document.getElementById('<%= ddlCity.ClientID %>');
                var ddlMun = document.getElementById('<%= ddlMunicipality.ClientID %>');

                var cityText = ddlCity && ddlCity.selectedIndex > 0 ? ddlCity.options[ddlCity.selectedIndex].text : '';
                var munText = ddlMun && ddlMun.selectedIndex > 0 ? ddlMun.options[ddlMun.selectedIndex].text : '';

                if (!cityText) return;

                var queryParts = [];
                if (munText) queryParts.push(munText);
                queryParts.push(cityText);
                queryParts.push('El Salvador');

                geocodeAndMoveMap(queryParts.join(', '));
            }

            var geocodeTimeout = null;

            function geocodeAndMoveMap(queryText) {
                if (geocodeTimeout) clearTimeout(geocodeTimeout);

                setMapLabel('<i class="fas fa-spinner fa-spin me-1"></i> Searching location...', 'searching');

                geocodeTimeout = setTimeout(function () {
                    var url = 'https://nominatim.openstreetmap.org/search?q=' +
                        encodeURIComponent(queryText) + '&format=json&limit=1&accept-language=es';

                    fetch(url, { headers: { 'Accept': 'application/json', 'User-Agent': 'OffsideShop Delivery System/1.0' } })
                        .then(function (r) { return r.json(); })
                        .then(function (data) {
                            if (data && data.length > 0) {
                                var lat = parseFloat(data[0].lat);
                                var lng = parseFloat(data[0].lon);
                                moveMapAndPin(lat, lng, 14);
                                setMapLabel('<i class="fas fa-check-circle me-1"></i> Location found: ' + data[0].display_name.split(',').slice(0, 2).join(','), 'found');
                            } else {
                                setMapLabel('<i class="fas fa-exclamation-triangle me-1"></i> Could not find location. Drag the pin manually.', 'error');
                            }
                        })
                        .catch(function () {
                            setMapLabel('<i class="fas fa-exclamation-triangle me-1"></i> Connection error. Drag the pin manually.', 'error');
                        });
                }, 400); 
            }

            function searchMapAddress() {
                var input = document.getElementById('mapSearchInput');
                if (!input || !input.value.trim()) return;
                geocodeAndMoveMap(input.value.trim() + ', El Salvador');
            }

            function moveMapAndPin(lat, lng, zoom) {
                if (!checkoutMap || !deliveryMarker) return;
                var latField = document.getElementById('<%= hfLatitude.ClientID %>');
                var lngField = document.getElementById('<%= hfLongitude.ClientID %>');

                deliveryMarker.setLatLng([lat, lng]);
                checkoutMap.setView([lat, lng], zoom || 16);
                if (latField) latField.value = lat.toFixed(7);
                if (lngField) lngField.value = lng.toFixed(7);
            }

            function reverseGeocode(lat, lng) {
                var url = 'https://nominatim.openstreetmap.org/reverse?lat=' + lat + '&lon=' + lng + '&format=json&accept-language=es';
                fetch(url, { headers: { 'Accept': 'application/json' } })
                    .then(function (r) { return r.json(); })
                    .then(function (data) {
                        if (data && data.display_name) {
                            var short = data.display_name.split(',').slice(0, 3).join(',');
                            setMapLabel('<i class="fas fa-map-marker-alt text-danger me-1"></i> ' + short, 'found');
                        }
                    })
                    .catch(function () { });
            }

            function setMapLabel(text, type) {
                var lbl = document.getElementById('mapLocationLabel');
                if (!lbl) return;
                lbl.innerHTML = text;
                lbl.className = 'map-location-label ' + (type || '');
            }
        </script>

        <uc:Footer ID="ControlFooter" runat="server" />
    </form>

    <script src="https://ajax.googleapis.com/ajax/libs/jquery/3.6.0/jquery.min.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.2.3/dist/js/bootstrap.bundle.min.js"></script>
</body>
</html>