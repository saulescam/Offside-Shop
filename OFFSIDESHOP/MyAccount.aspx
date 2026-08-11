<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="MyAccount.aspx.cs" Inherits="OFFSIDESHOP.MyAccount" %>

<%@ Register Src="~/FooterControl.ascx" TagPrefix="uc" TagName="Footer" %>
<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1, shrink-to-fit=no" />
    <title><%= Resources.Strings.Account_MainTitle %> - OffsideShop</title>

    <link rel="icon" type="image/x-icon" href="assets/favicon.ico" />
    <script src="https://use.fontawesome.com/releases/v6.3.0/js/all.js" crossorigin="anonymous"></script>
    <link href="https://fonts.googleapis.com/css?family=Montserrat:400,700" rel="stylesheet" type="text/css" />
    <link href="https://fonts.googleapis.com/css?family=Roboto+Slab:400,100,300,700" rel="stylesheet" type="text/css" />
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.2.3/dist/css/bootstrap.min.css" rel="stylesheet" />
    <link href="css/styles.css" rel="stylesheet" />
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>

    <!-- LEAFLET JS PARA EL MAPA -->
    <link rel="stylesheet" href="https://unpkg.com/leaflet@1.9.4/dist/leaflet.css" />
    <script src="https://unpkg.com/leaflet@1.9.4/dist/leaflet.js"></script>

    <style>
        body { font-family: 'Montserrat', sans-serif; background-color: #f8f9fa; min-height: 100vh; padding-top: 120px; }
        .navbar { background: #000000 !important; box-shadow: 0 4px 20px rgba(0, 0, 0, 0.5); padding: 12px 0; border-bottom: 1px solid rgba(255, 255, 255, 0.05); }
        .account-container { background: #ffffff; border-radius: 20px; box-shadow: 0 10px 30px rgba(0, 0, 0, 0.05); padding: 40px; margin-bottom: 50px; }
        .account-title { font-weight: 700; color: #111111; border-bottom: 3px solid #FFC800; display: inline-block; padding-bottom: 10px; margin-bottom: 30px; }
        .section-subtitle { font-weight: 700; font-size: 1.2rem; color: #000000; margin-bottom: 20px; display: flex; align-items: center; gap: 10px; }
        .form-label-custom { font-weight: 600; font-size: 0.85rem; text-transform: uppercase; letter-spacing: 0.5px; color: #555555; margin-bottom: 6px; }
        .form-control-custom { border: 2px solid #e9ecef; border-radius: 10px; padding: 12px 15px; font-family: 'Montserrat', sans-serif; transition: all 0.3s ease; }
        .form-control-custom:focus { border-color: #FFC800; box-shadow: 0 0 0 0.25rem rgba(255, 200, 0, 0.15); outline: none; }
        .btn-save-custom { background: linear-gradient(135deg, #FFC800 0%, #D4A000 100%); color: #000000 !important; border: none; padding: 12px 30px; border-radius: 30px; font-weight: 700; transition: all 0.3s ease; box-shadow: 0 5px 15px rgba(255, 200, 0, 0.2); }
        .btn-save-custom:hover { background: linear-gradient(135deg, #FFE066 0%, #FFC800 100%); transform: translateY(-2px); box-shadow: 0 8px 20px rgba(255, 200, 0, 0.35); }
        .btn-secondary-custom { border: 2px solid #cccccc; color: #555555 !important; padding: 10px 25px; border-radius: 30px; font-weight: 600; background: transparent; text-decoration: none; transition: all 0.3s ease; }
        .btn-secondary-custom:hover { background-color: #eef2f3; color: #111111 !important; border-color: #999999; transform: translateY(-2px); }
        .security-card { background-color: #f9f9f9; border: 1px solid #e9ecef; border-radius: 15px; padding: 25px; }
        .password-container { position: relative; }
        .btn-toggle-password { position: absolute; right: 15px; top: 50%; transform: translateY(-50%); background: none; border: none; color: #888888; cursor: pointer; z-index: 10; }
        .btn-toggle-password:hover { color: #FFC800; }
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
        .dropdown-item.btn-logout { border-top: 1px solid #333333; margin-top: 4px; padding-top: 10px; }
        .dropdown-item.btn-logout:hover { background-color: #D47A00 !important; }

        /* Badge de Rol */
        .role-badge {
            display: inline-block;
            padding: 10px 16px;
            border-radius: 10px;
            font-weight: 700;
            font-size: 0.85rem;
            text-transform: uppercase;
            letter-spacing: 1px;
            background: #1a1a1a;
            color: #FFC800;
            border: 1px solid #FFC800;
            width: 100%;
        }
    </style>

    <script type="text/javascript">
        function toggleUserMenu(button) {
            const container = button.closest('.user-menu-container');
            if (!container) return;
            const menu = container.querySelector('.dynamic-dropdown');
            if (!menu) return;
            menu.style.display = menu.style.display === 'block' ? 'none' : 'block';
        }

        document.onclick = function (event) {
            const container = event.target.closest('.user-menu-container');
            if (!container) {
                const menus = document.querySelectorAll('.dynamic-dropdown');
                menus.forEach(m => m.style.display = 'none');
            }
        };

        function togglePasswordVisibility(btn, inputId) {
            const input = document.getElementById(inputId);
            const icon = btn.querySelector('i');
            if (input.type === "password") {
                input.type = "text";
                icon.classList.remove('fa-eye');
                icon.classList.add('fa-eye-slash');
            } else {
                input.type = "password";
                icon.classList.remove('fa-eye-slash');
                icon.classList.add('fa-eye');
            }
        }

        function showPasswordFields() {
            document.getElementById('defaultPasswordRow').style.display = 'none';
            document.getElementById('passwordFieldsPanel').style.display = 'block';
        }

        function hidePasswordFields() {
            document.getElementById('defaultPasswordRow').style.display = 'block';
            document.getElementById('passwordFieldsPanel').style.display = 'none';
        }

        function trackFormChanges() {
            const inputs = document.querySelectorAll('#<%= upPersonalInfo.ClientID %> input[type="text"], #<%= upPersonalInfo.ClientID %> textarea');
            const saveBtn = document.getElementById('<%= btnSaveChanges.ClientID %>');

            if (!saveBtn || inputs.length === 0) return;

            inputs.forEach(input => {
                if (!input.hasAttribute('data-original-value')) {
                    input.setAttribute('data-original-value', input.value);
                }
                input.addEventListener('input', function () {
                    checkIfChanged(inputs, saveBtn);
                });
            });

            checkIfChanged(inputs, saveBtn);
        }

        function checkIfChanged(inputs, saveBtn) {
            let isChanged = false;
            inputs.forEach(input => {
                if (input.value !== input.getAttribute('data-original-value')) {
                    isChanged = true;
                }
            });

            if (isChanged) {
                saveBtn.style.pointerEvents = 'auto';
                saveBtn.style.filter = 'grayscale(0%)';
                saveBtn.style.opacity = '1';
                saveBtn.style.cursor = 'pointer';
            } else {
                saveBtn.style.pointerEvents = 'none';
                saveBtn.style.filter = 'grayscale(100%)';
                saveBtn.style.opacity = '0.5';
                saveBtn.style.cursor = 'not-allowed';
            }
        }
        function isNumberKey(evt) {
            var charCode = (evt.which) ? evt.which : evt.keyCode;
            if (charCode > 31 && (charCode < 48 || charCode > 57)) {
                return false;
            }
            return true;
        }
    </script>
</head>
<body>
    <form id="form1" runat="server">
        <asp:ScriptManager ID="ScriptManager1" runat="server" />

        <!-- TOP HEAD NAVIGATION BAR -->
        <nav class="navbar navbar-expand-lg navbar-dark fixed-top" id="mainNav">
            <div class="container">
                <a class="navbar-brand" href="Homepage.aspx">
                    <img src="assets/img/offsideshop_logo_white_letras.png" alt="OffsideShop Logo" />
                </a>
                <asp:LinkButton ID="btnLanguageToggle" runat="server" OnClick="btnLanguageToggle_Click" CssClass="lang-switcher" style="color: #fff; text-decoration: none; font-weight: bold; margin-left: 10px; margin-right: auto;">EN / ES</asp:LinkButton>

                <button class="navbar-toggler" type="button" data-bs-toggle="collapse" data-bs-target="#navbarResponsive">
                    <span class="navbar-toggler-icon"></span>
                </button>

                <div class="collapse navbar-collapse" id="navbarResponsive">

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
                                            <p class="user-fullname">
                                                <asp:Label ID="lblFullName" runat="server" Text="Loading..."></asp:Label>
                                            </p>
                                            <p class="user-email">
                                                <asp:Label ID="lblUserEmail" runat="server" Text=""></asp:Label>
                                            </p>
                                        </div>
                                        <div class="dropdown-content">
                                            <a href="Homepage.aspx" class="dropdown-item"><i class="fas fa-home"></i><asp:Literal runat="server" Text="<%$ Resources:Strings, Nav_BackToShop %>" /></a>
                                            <asp:LinkButton ID="btnMyOrders" runat="server" CssClass="dropdown-item" OnClick="btnMyOrders_Click">
                                                <i class="fas fa-clipboard-list"></i> <asp:Literal runat="server" Text="<%$ Resources:Strings, Nav_MyOrders %>" />
                                            </asp:LinkButton>
                                            <asp:Button ID="btncerrar" runat="server" CssClass="dropdown-item btn-logout" Text="<%$ Resources:Strings, Nav_LogOut %>" OnClick="btncerrar_Click" />
                                        </div>
                                    </div>
                                </ContentTemplate>
                            </asp:UpdatePanel>
                        </div>
                    </asp:PlaceHolder>
                    <asp:PlaceHolder ID="phNavbarAdmin" runat="server">
                        <div class="user-menu-container">
                            <button type="button" class="user-icon-btn" onclick="toggleUserMenu(this)">
                                <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                                    <circle cx="12" cy="8" r="4"></circle>
                                    <path d="M 6 20c0-4 2.5-6 6-6s6 2 6 6"></path>
                                </svg>
                            </button>
                            <div class="user-dropdown-menu dynamic-dropdown" style="display: none;">
                                <div class="user-info">
                                    <p class="user-fullname">
                                        <asp:Label ID="lblAdminName" runat="server" Text="Admin"></asp:Label>
                                    </p>
                                    <p class="user-role"><asp:Literal runat="server" Text="<%$ Resources:Strings, Nav_AdminRole %>" /></p>
                                </div>
                                <div class="dropdown-content">
                                    <a href="Dashboard.aspx" class="dropdown-item">
                                        <i class="fas fa-chart-line"></i><asp:Literal runat="server" Text="<%$ Resources:Strings, Nav_Dashboard %>" />
                                    </a>
                                    <asp:Button ID="Button2" runat="server" CssClass="dropdown-item btn-logout" Text="<%$ Resources:Strings, Nav_BackToShop %>" OnClick="btnbackshop_Click" />
                                    <asp:Button ID="btnlogout" runat="server" CssClass="dropdown-item btn-logout" Text="<%$ Resources:Strings, Nav_LogOut %>" OnClick="btncerrar_Click" />
                                </div>
                            </div>
                        </div>
                    </asp:PlaceHolder>
                </div>
            </div>
        </nav>

        <!-- MAIN CONTAINER MODULE -->
        <div class="container">
            <div class="account-container">
                <h2 class="account-title"><asp:Literal runat="server" Text="<%$ Resources:Strings, Account_MainTitle %>" /></h2>

                <asp:UpdatePanel ID="upAlerts" runat="server">
                    <ContentTemplate>
                        <asp:Label ID="lblGlobalMessage" runat="server" CssClass="d-block mb-3"></asp:Label>
                    </ContentTemplate>
                </asp:UpdatePanel>

                <div class="row g-5">
                    <!-- PERSONAL PROFILE SECTOR -->
                    <div class="col-lg-7">
                        <h4 class="section-subtitle">
                            <i class="fas fa-user-edit text-warning"></i> <asp:Literal runat="server" Text="<%$ Resources:Strings, Account_PersonalInfo %>" />
                        </h4>

                        <asp:UpdatePanel ID="upPersonalInfo" runat="server" UpdateMode="Conditional">
                            <ContentTemplate>
                                <div class="row g-3">
                                    <div class="col-md-6">
                                        <label class="form-label form-label-custom"><asp:Literal runat="server" Text="<%$ Resources:Strings, Account_FirstName %>" /></label>
                                        <asp:TextBox ID="txtFirstName" runat="server" CssClass="form-control form-control-custom" placeholder="John"></asp:TextBox>
                                    </div>
                                    <div class="col-md-6">
                                        <label class="form-label form-label-custom"><asp:Literal runat="server" Text="<%$ Resources:Strings, Account_LastName %>" /></label>
                                        <asp:TextBox ID="txtLastName" runat="server" CssClass="form-control form-control-custom" placeholder="Doe"></asp:TextBox>
                                    </div>
                                    <div class="col-md-6">
                                        <label class="form-label form-label-custom"><asp:Literal runat="server" Text="<%$ Resources:Strings, Account_Username %>" /></label>
                                        <asp:TextBox ID="txtUsername" runat="server" CssClass="form-control form-control-custom" placeholder="johndoe123"></asp:TextBox>
                                    </div>

                                    <!-- APARTADO DEL ROL DE USUARIO -->
                                    <div class="col-md-6">
                                        <label class="form-label form-label-custom"><asp:Literal runat="server" Text="<%$ Resources:Strings, Account_RoleLabel %>" /></label>
                                        <div class="role-badge text-center">
                                            <i class="fas fa-user-shield me-1"></i>
                                            <asp:Label ID="lblAccountRole" runat="server" Text="Customer"></asp:Label>
                                        </div>
                                    </div>

                                    <div class="col-md-12">
                                        <label class="form-label form-label-custom"><asp:Literal runat="server" Text="<%$ Resources:Strings, Account_EmailAddress %>" /></label>
                                        <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control form-control-custom" Enabled="false"></asp:TextBox>
                                        <small class="text-muted"><asp:Literal runat="server" Text="<%$ Resources:Strings, Account_EmailWarning %>" /></small>
                                    </div>
                                    <div class="col-12">
                                        <label class="form-label form-label-custom"><asp:Literal runat="server" Text="<%$ Resources:Strings, Account_PhoneNumber %>" /></label>
                                        <asp:TextBox ID="txtPhone" runat="server" CssClass="form-control form-control-custom" placeholder="(+503) 12345678" MaxLength="8" onkeypress="return isNumberKey(event);" onpaste="return false;"></asp:TextBox>
                                    </div>
                                    <div class="col-12">
                                        <label class="form-label form-label-custom"><asp:Literal runat="server" Text="<%$ Resources:Strings, Account_ShippingAddress %>" /></label>
                                        <asp:TextBox ID="txtAddress" runat="server" CssClass="form-control form-control-custom" placeholder="<%$ Resources:Strings, Account_AddressPlaceholder %>" TextMode="MultiLine" Rows="2"></asp:TextBox>
                                    </div>
                                    
                                    <!-- MAP CONTAINER -->
                                    <div class="col-12 mt-3">
                                        <label class="form-label form-label-custom"><asp:Literal runat="server" Text="<%$ Resources:Strings, Account_DefaultLocation %>" /> <span class="text-warning">*</span></label>
                                        <div id="accountMap" style="height: 300px; width: 100%; border-radius: 10px; border: 2px solid #e9ecef; z-index: 1;"></div>
                                        <small class="text-muted"><i class="fas fa-map-marker-alt text-danger mr-1"></i> <asp:Literal runat="server" Text="<%$ Resources:Strings, Account_MapInstructions %>" /></small>
                                        
                                        <asp:HiddenField ID="hfDefaultLat" runat="server" />
                                        <asp:HiddenField ID="hfDefaultLng" runat="server" />
                                    </div>

                                    <!-- CORE FUNCTION ACTION GROUP BUTTONS -->
                                    <div class="col-12 mt-4 d-flex gap-3 flex-wrap">
                                        <asp:Button ID="btnSaveChanges" runat="server" Text="<%$ Resources:Strings, Account_SaveChanges %>" 
                                            CssClass="btn-save-custom" OnClick="btnSaveChanges_Click" />
                                        <a href="Homepage.aspx" class="btn btn-secondary-custom d-inline-flex align-items-center gap-2">
                                            <i class="fas fa-arrow-left"></i> <asp:Literal runat="server" Text="<%$ Resources:Strings, Account_BackHome %>" />
                                        </a>
                                    </div>
                                </div>
                            </ContentTemplate>
                        </asp:UpdatePanel>
                    </div>

                    <!-- SECURITY & CRYPTOGRAPHIC PASS MANAGEMENT MODULE -->
                    <div class="col-lg-5">
                        <div class="security-card">
                            <h4 class="section-subtitle">
                                <i class="fas fa-shield-alt text-warning"></i> <asp:Literal runat="server" Text="<%$ Resources:Strings, Account_SecurityTitle %>" />
                            </h4>

                            <asp:UpdatePanel ID="upSecurity" runat="server" UpdateMode="Conditional">
                                <ContentTemplate>
                                    
                                    <!-- Masked View Mode Layout Frame -->
                                    <div id="defaultPasswordRow" class="row g-3">
                                        <div class="col-12">
                                            <label class="form-label form-label-custom"><asp:Literal runat="server" Text="<%$ Resources:Strings, Account_PasswordLabel %>" /></label>
                                            <input type="text" class="form-control form-control-custom text-muted fw-bold" value="**********" readonly="readonly" style="letter-spacing: 3px;" />
                                        </div>
                                        <div class="col-12 mt-3">
                                            <button type="button" class="btn btn-secondary-custom w-100 d-inline-flex align-items-center justify-content-center gap-2" onclick="showPasswordFields()">
                                                <i class="fas fa-key"></i> <asp:Literal runat="server" Text="<%$ Resources:Strings, Account_ChangePasswordBtn %>" />
                                            </button>
                                        </div>
                                    </div>

                                    <!-- Editable Interactive Input Fields Container Panel -->
                                    <div id="passwordFieldsPanel" class="row g-3" style="display: none;">
                                        <div class="col-12">
                                            <label class="form-label form-label-custom"><asp:Literal runat="server" Text="<%$ Resources:Strings, Account_CurrentPassword %>" /></label>
                                            <div class="password-container">
                                                <asp:TextBox ID="txtCurrentPassword" runat="server" TextMode="Password" CssClass="form-control form-control-custom w-100"></asp:TextBox>
                                                <button type="button" class="btn-toggle-password" onclick="togglePasswordVisibility(this, '<%= txtCurrentPassword.ClientID %>')">
                                                    <i class="fas fa-eye"></i>
                                                </button>
                                            </div>
                                        </div>

                                        <div class="col-12">
                                            <label class="form-label form-label-custom"><asp:Literal runat="server" Text="<%$ Resources:Strings, Account_NewPassword %>" /></label>
                                            <div class="password-container">
                                                <asp:TextBox ID="txtNewPassword" runat="server" TextMode="Password" CssClass="form-control form-control-custom w-100"></asp:TextBox>
                                                <button type="button" class="btn-toggle-password" onclick="togglePasswordVisibility(this, '<%= txtNewPassword.ClientID %>')">
                                                    <i class="fas fa-eye"></i>
                                                </button>
                                            </div>
                                        </div>

                                        <div class="col-12">
                                            <label class="form-label form-label-custom"><asp:Literal runat="server" Text="<%$ Resources:Strings, Account_ConfirmPassword %>" /></label>
                                            <div class="password-container">
                                                <asp:TextBox ID="txtConfirmPassword" runat="server" TextMode="Password" CssClass="form-control form-control-custom w-100"></asp:TextBox>
                                                <button type="button" class="btn-toggle-password" onclick="togglePasswordVisibility(this, '<%= txtConfirmPassword.ClientID %>')">
                                                    <i class="fas fa-eye"></i>
                                                </button>
                                            </div>
                                        </div>

                                        <div class="col-12 text-start mt-2 d-flex justify-content-between align-items-center">
                                            <asp:LinkButton ID="lnkForgotPassword" runat="server" CssClass="text-secondary small fw-semibold" OnClick="lnkForgotPassword_Click"><asp:Literal runat="server" Text="<%$ Resources:Strings, Account_ForgotPassword %>" /></asp:LinkButton>
                                            <button type="button" class="btn btn-link text-danger small fw-semibold text-decoration-none p-0" onclick="hidePasswordFields()"><asp:Literal runat="server" Text="<%$ Resources:Strings, Account_Cancel %>" /></button>
                                        </div>

                                        <div class="col-12 mt-4">
                                            <asp:Button ID="btnUpdatePassword" runat="server" Text="<%$ Resources:Strings, Account_UpdatePasswordBtn %>" 
                                                CssClass="btn-save-custom w-100" OnClick="btnUpdatePassword_Click" />
                                        </div>
                                    </div>

                                </ContentTemplate>
                            </asp:UpdatePanel>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </form>

    <!-- JS SECTION FOR MAP INITIALIZATION -->
    <script type="text/javascript">
        var accountMap = null;
        var accountMarker = null;

        function initAccountMap() {
            var latField = document.getElementById('<%= hfDefaultLat.ClientID %>');
            var lngField = document.getElementById('<%= hfDefaultLng.ClientID %>');

            var startLat = latField.value ? parseFloat(latField.value) : 13.6929;
            var startLng = lngField.value ? parseFloat(lngField.value) : -89.2182;
            var zoomLvl = latField.value ? 16 : 9;

            if (accountMap !== null) {
                accountMap.remove();
            }

            accountMap = L.map('accountMap').setView([startLat, startLng], zoomLvl);

            L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
                attribution: '&copy; OpenStreetMap'
            }).addTo(accountMap);

            accountMarker = L.marker([startLat, startLng], { draggable: true }).addTo(accountMap);

            accountMarker.on('dragend', function (e) {
                var pos = accountMarker.getLatLng();
                latField.value = pos.lat;
                lngField.value = pos.lng;

                const saveBtn = document.getElementById('<%= btnSaveChanges.ClientID %>');
                if (saveBtn) {
                    saveBtn.style.pointerEvents = 'auto';
                    saveBtn.style.filter = 'grayscale(0%)';
                    saveBtn.style.opacity = '1';
                    saveBtn.style.cursor = 'pointer';
                }
            });

            setTimeout(function () { accountMap.invalidateSize(); }, 200);
        }

        document.addEventListener('DOMContentLoaded', function () {
            trackFormChanges();
            initAccountMap();

            if (typeof Sys !== 'undefined' && Sys.WebForms && Sys.WebForms.PageRequestManager) {
                Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
                    const inputs = document.querySelectorAll('#<%= upPersonalInfo.ClientID %> input[type="text"], #<%= upPersonalInfo.ClientID %> textarea');
                    inputs.forEach(input => input.removeAttribute('data-original-value'));
                    trackFormChanges();
                    initAccountMap();
                });
            }
        });
    </script>

    <script src="https://ajax.googleapis.com/ajax/libs/jquery/3.6.0/jquery.min.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.2.3/dist/js/bootstrap.bundle.min.js"></script>
</body>
</html>
