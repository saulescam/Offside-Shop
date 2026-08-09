<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="DeliveryDashboard.aspx.cs" Inherits="OFFSIDESHOP.DeliveryDashboard" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1, maximum-scale=1, user-scalable=0" />
    <title><asp:Literal runat="server" Text="<%$ Resources:Strings, Driver_PageTitle %>" /> | OffsideShop</title>

    <link href="https://fonts.googleapis.com/css?family=Montserrat:400,700,900" rel="stylesheet" />
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.2.3/dist/css/bootstrap.min.css" rel="stylesheet" />
    <script src="https://use.fontawesome.com/releases/v6.3.0/js/all.js" crossorigin="anonymous"></script>
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>

    <!-- Leaflet para el Mapa del Repartidor -->
    <link rel="stylesheet" href="https://unpkg.com/leaflet@1.9.4/dist/leaflet.css" />
    <script src="https://ajax.googleapis.com/ajax/libs/jquery/3.6.0/jquery.min.js"></script>
    <script src="https://unpkg.com/leaflet@1.9.4/dist/leaflet.js"></script>
    <link rel="stylesheet" href="https://unpkg.com/leaflet-routing-machine@latest/dist/leaflet-routing-machine.css" />
    <script src="https://unpkg.com/leaflet-routing-machine@latest/dist/leaflet-routing-machine.js"></script>

    <style>
        body {
            background-color: #f4f6f9;
            font-family: 'Montserrat', sans-serif;
            padding-bottom: 20px;
        }

        .driver-nav {
            background-color: #1a1a1a;
            padding: 12px 20px;
            border-bottom: 3px solid #FFC800;
            display: flex;
            justify-content: space-between;
            align-items: center;
            position: sticky;
            top: 0;
            z-index: 1000;
        }

        .driver-nav img {
            height: 35px;
        }

        .user-menu-container {
            position: relative;
            display: flex;
            align-items: center;
            margin-left: auto;
        }

        .user-icon-btn {
            background: none;
            border: none;
            cursor: pointer;
            padding: 8px;
            color: #ffffff;
            transition: all 0.3s ease;
            display: flex;
            align-items: center;
            justify-content: center;
            width: 40px;
            height: 40px;
            border-radius: 50%;
        }

            .user-icon-btn:hover {
                color: #FFC800;
                background-color: rgba(255, 200, 0, 0.1);
            }

        .user-dropdown-menu {
            position: absolute;
            top: 50px;
            right: 0;
            background: #1a1a1a;
            border: 1px solid #FFC800;
            border-radius: 8px;
            min-width: 260px;
            box-shadow: 0 4px 12px rgba(0, 0, 0, 0.5);
            z-index: 1000;
            padding: 0;
        }

        .user-info {
            padding: 12px 16px;
            border-bottom: 1px solid #333333;
        }

        .user-fullname {
            margin: 0;
            color: #FFC800;
            font-weight: bold;
            font-size: 0.95rem;
        }

        .user-email {
            margin: 4px 0 0 0;
            color: #888888;
            font-size: 0.8rem;
        }

        .dropdown-content {
            display: flex;
            flex-direction: column;
            padding: 8px 0;
        }

        .dropdown-item {
            display: flex;
            align-items: center;
            gap: 10px;
            padding: 10px 16px;
            color: #ffffff;
            text-decoration: none;
            cursor: pointer;
            border: none;
            background: transparent;
            width: 100%;
            text-align: left;
            transition: all 0.2s;
            font-family: 'Montserrat', sans-serif;
            font-size: 0.95rem;
        }

            .dropdown-item:hover {
                background-color: #FFC800;
                color: #000000;
            }

            .dropdown-item i {
                font-size: 1rem;
                width: 20px;
            }

            .dropdown-item.btn-logout {
                border-top: 1px solid #333333;
                margin-top: 4px;
                padding-top: 10px;
            }

                .dropdown-item.btn-logout:hover {
                    background-color: #D47A00 !important;
                }

        /* Toggle Switch para On-Duty */
        .form-check-input:checked {
            background-color: #10b981;
            border-color: #10b981;
        }

        .status-badge {
            font-weight: 700;
            font-size: 0.8rem;
            text-transform: uppercase;
            padding: 6px 12px;
            border-radius: 20px;
        }

        .bg-online {
            background-color: #d1fae5;
            color: #047857;
        }

        .bg-offline {
            background-color: #f3f4f6;
            color: #374151;
        }

        /* Tarjetas de Radar (Piscina de Órdenes) */
        .radar-card {
            background: #fff;
            border-radius: 15px;
            padding: 20px;
            margin-bottom: 15px;
            box-shadow: 0 4px 15px rgba(0,0,0,0.05);
            border-left: 5px solid #FFC800;
        }

        .radar-price {
            font-size: 1.5rem;
            font-weight: 900;
            color: #1a1a1a;
        }

        .radar-city {
            font-weight: 700;
            color: #555;
            font-size: 1.1rem;
        }

        .radar-distance {
            color: #888;
            font-size: 0.9rem;
            font-weight: 600;
        }

        /* Misión Activa */
        .mission-header {
            background: #1a1a1a;
            color: #fff;
            padding: 20px;
            border-radius: 15px;
            text-align: center;
            margin-bottom: 20px;
        }

        .mission-header h2 {
            color: #FFC800;
            font-weight: 900;
            margin: 0;
        }

        .call-btn {
            background-color: #10b981;
            color: white;
            border-radius: 50%;
            width: 48px;
            height: 48px;
            display: flex;
            align-items: center;
            justify-content: center;
            font-size: 1.2rem;
            text-decoration: none;
            box-shadow: 0 4px 10px rgba(16, 185, 129, 0.4);
            transition: all 0.2s ease;
        }

        .call-btn:hover {
            color: white;
            background-color: #059669;
            transform: scale(1.08);
        }

        .whatsapp-btn {
            background-color: #25D366;
            box-shadow: 0 4px 10px rgba(37, 211, 102, 0.4);
        }

        .whatsapp-btn:hover {
            background-color: #1eb956;
            color: white;
        }

        .action-btn-huge {
            width: 100%;
            padding: 18px;
            font-size: 1.2rem;
            font-weight: 800;
            text-transform: uppercase;
            border-radius: 12px;
            letter-spacing: 1px;
        }

        .empty-state {
            text-align: center;
            padding: 50px 20px;
            color: #888;
        }

        .empty-state i {
            font-size: 4rem;
            color: #ddd;
            margin-bottom: 15px;
        }

        /* Estilo para mapa Leaflet y ocultar cuadro de texto de OSRM */
        .leaflet-routing-container {
            display: none !important;
        }

        /* Estilo del botón de cambio de idioma */
        .lang-toggle-btn {
            color: #ffffff !important;
            font-weight: 700;
            font-size: 0.9rem;
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
            if (!container) {
                cerrarTodosLosMenus();
            }
        };
    </script>
</head>
<body>
    <form id="form1" runat="server">
        <asp:ScriptManager runat="server" />

        <!-- HEADER DEL REPARTIDOR -->
        <div class="driver-nav">
            <div class="d-flex align-items-center gap-3">
                <img src="assets/img/offsideshop_logo_white_letras.png" alt="Logo" class="d-none d-md-block" />
            </div>
            
            <div class="d-flex align-items-center gap-3">
                <!-- Botón de cambio de idioma -->
                <asp:LinkButton ID="btnLanguageToggle" runat="server" OnClick="btnLanguageToggle_Click" 
                    CssClass="lang-toggle-btn" CausesValidation="false">
                    EN / ES
                </asp:LinkButton>

                <asp:UpdatePanel runat="server">
                    <ContentTemplate>
                        <div class="form-check form-switch m-0 d-flex align-items-center">
                            <input type="checkbox" id="chkDutySwitch" ClientIDMode="Static" runat="server" class="form-check-input shadow-none" style="width: 45px; height: 25px; cursor: pointer; margin-top: 0;" onchange="document.getElementById('btnHiddenDuty').click();" />
                            <asp:Button ID="btnHiddenDuty" ClientIDMode="Static" runat="server" OnClick="chkDuty_CheckedChanged" Style="display: none;" />
                            <asp:Label ID="lblDutyStatus" runat="server" CssClass="status-badge bg-offline ms-2" Text="Offline"></asp:Label>
                        </div>
                    </ContentTemplate>
                </asp:UpdatePanel>
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
                                <asp:Label ID="lblDriverName" runat="server" Text="Driver"></asp:Label>
                            </p>
                            <p class="user-email">
                                <asp:Label ID="lblDriverEmail" runat="server" Text=""></asp:Label>
                            </p>
                        </div>
                        <div class="dropdown-content">
                            <a href="MyAccount.aspx" class="dropdown-item">
                                <i class="fas fa-user-cog"></i><asp:Literal runat="server" Text="<%$ Resources:Strings, Nav_MyAccount %>" />
                            </a>
                            <asp:LinkButton ID="btnLogout" runat="server" CssClass="dropdown-item btn-logout" OnClick="btnLogout_Click" UseSubmitBehavior="false">
                                <i class="fas fa-sign-out-alt"></i><asp:Literal runat="server" Text="<%$ Resources:Strings, Nav_LogOut %>" />
                            </asp:LinkButton>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <div class="container mt-4">
            <asp:UpdatePanel ID="upMain" runat="server">
                <ContentTemplate>
                    <asp:MultiView ID="mvDriver" runat="server" ActiveViewIndex="0">

                        <!-- VISTA 1: RADAR DE PEDIDOS DISPONIBLES -->
                        <asp:View ID="viewRadar" runat="server">
                            <h5 class="fw-bold mb-3 text-uppercase text-muted" style="letter-spacing: 1px;"><asp:Literal runat="server" Text="<%$ Resources:Strings, Driver_AvailableDeliveries %>" /></h5>

                            <asp:PlaceHolder ID="phOffline" runat="server">
                                <div class="empty-state">
                                    <i class="fas fa-bed"></i>
                                    <h4 class="fw-bold text-dark"><asp:Literal runat="server" Text="<%$ Resources:Strings, Driver_OfflineTitle %>" /></h4>
                                    <p><asp:Literal runat="server" Text="<%$ Resources:Strings, Driver_OfflineDesc %>" /></p>
                                </div>
                            </asp:PlaceHolder>

                            <asp:PlaceHolder ID="phOnline" runat="server" Visible="false">
                                <asp:Repeater ID="rptRadar" runat="server" OnItemCommand="rptRadar_ItemCommand">
                                    <ItemTemplate>
                                        <div class="radar-card">
                                            <div class="d-flex justify-content-between align-items-start mb-2">
                                                <div>
                                                    <span class="badge bg-dark mb-2"><asp:Literal runat="server" Text="<%$ Resources:Strings, Driver_OrderLabel %>" /> #<%# Eval("Id_Order") %></span>
                                                    <div class="radar-city"><%# Eval("city_name") %> - <%# Eval("Municipality_Name") %></div>
                                                    <div class="radar-distance"><i class="fas fa-box me-1"></i><%# Eval("TotalItems") %> <asp:Literal runat="server" Text="<%$ Resources:Strings, Driver_ItemsToDeliver %>" /></div>
                                                </div>
                                                <div class="text-end">
                                                    <div class="text-muted" style="font-size: 12px; font-weight: 700;"><asp:Literal runat="server" Text="<%$ Resources:Strings, Driver_ToCollect %>" /></div>
                                                    <div class="radar-price">$<%# Convert.ToDecimal(Eval("Total")).ToString("N2") %></div>
                                                </div>
                                            </div>
                                            <hr style="border-color: #eee;" />
                                            <asp:LinkButton ID="btnAccept" runat="server" CommandName="ACCEPT" CommandArgument='<%# Eval("Id_Order") %>' CssClass="btn btn-dark w-100 fw-bold py-2 text-warning text-decoration-none d-block text-center">
                                                <asp:Literal runat="server" Text="<%$ Resources:Strings, Driver_AcceptDelivery %>" />
                                            </asp:LinkButton>
                                        </div>
                                    </ItemTemplate>
                                </asp:Repeater>

                                <asp:PlaceHolder ID="phNoOrders" runat="server" Visible="false">
                                    <div class="empty-state">
                                        <i class="fas fa-radar"></i>
                                        <h4 class="fw-bold text-dark"><asp:Literal runat="server" Text="<%$ Resources:Strings, Driver_SearchingOrders %>" /></h4>
                                        <p><asp:Literal runat="server" Text="<%$ Resources:Strings, Driver_NoOrdersDesc %>" /></p>
                                    </div>
                                </asp:PlaceHolder>
                            </asp:PlaceHolder>
                        </asp:View>

                        <!-- VISTA 2: MISIÓN ACTIVA (EN RUTA) -->
                        <asp:View ID="viewMission" runat="server">
                            <div class="mission-header shadow-sm">
                                <span class="text-uppercase" style="letter-spacing: 2px; font-size: 0.8rem; color: #aaa;"><asp:Literal runat="server" Text="<%$ Resources:Strings, Driver_ActiveTrip %>" /></span>
                                <h2><asp:Literal runat="server" Text="<%$ Resources:Strings, Driver_OrderLabel %>" /> #<asp:Label ID="lblMissionOrderId" runat="server"></asp:Label></h2>
                            </div>

                            <div class="card border-0 shadow-sm rounded-4 mb-4">
                                <div class="card-body p-4">
                                    
                                    <!-- Cliente y Botones de Contacto Directo -->
                                    <div class="d-flex justify-content-between align-items-center mb-3">
                                        <div>
                                            <h5 class="fw-bold mb-1">
                                                <asp:Label ID="lblClientName" runat="server"></asp:Label>
                                            </h5>
                                            <p class="text-muted mb-0">
                                                <i class="fas fa-map-marker-alt text-danger me-2"></i>
                                                <asp:Label ID="lblClientAddress" runat="server"></asp:Label>
                                            </p>
                                        </div>
                                        <div class="d-flex gap-2 align-items-center">
                                            <a id="btnCallClient" runat="server" href="#" class="call-btn" title="Call Customer">
                                                <i class="fas fa-phone-alt"></i>
                                            </a>
                                            <a id="btnWhatsappClient" runat="server" href="#" target="_blank" class="call-btn whatsapp-btn" title="WhatsApp Customer">
                                                <i class="fab fa-whatsapp"></i>
                                            </a>
                                        </div>
                                    </div>

                                    <!-- TARJETA DE ESTADO DE PAGO -->
                                    <div class="card mb-3 border-secondary bg-light">
                                        <div class="card-body text-center p-3">
                                            <div class="text-muted small fw-bold text-uppercase mb-1">
                                                <i class="fas fa-wallet me-1"></i>
                                                <asp:Literal ID="litPaymentTitle" runat="server" Text="<%$ Resources:Strings, Driver_PaymentMethod %>" />
                                            </div>
                                            <div class="d-flex justify-content-center align-items-center">
                                                <asp:Label ID="lblPaymentStatus" runat="server" CssClass="fw-bold px-3 py-2 rounded w-100" style="font-size: 1.05rem;"></asp:Label>
                                            </div>
                                        </div>
                                    </div>

                                    <!-- Botones de Navegación GPS Externa (Google Maps / Waze) -->
                                    <div class="d-flex gap-2 mb-3">
                                        <a id="btnGoogleMaps" runat="server" target="_blank" class="btn btn-primary w-50 fw-bold rounded-3">
                                            <i class="fas fa-map-marked-alt me-1"></i> <asp:Literal runat="server" Text="<%$ Resources:Strings, Driver_GoogleMaps %>" />
                                        </a>
                                        <a id="btnWaze" runat="server" target="_blank" class="btn btn-info text-white w-50 fw-bold rounded-3" style="background-color: #33ccff; border: none;">
                                            <i class="fab fa-waze me-1"></i> <asp:Literal runat="server" Text="<%$ Resources:Strings, Driver_Waze %>" />
                                        </a>
                                    </div>

                                    <!-- Instrucciones / Notas del Cliente (OrderNotes) -->
                                    <asp:PlaceHolder ID="phOrderNotes" runat="server" Visible="false">
                                        <div class="alert alert-warning border-warning d-flex align-items-start gap-2 mb-3 rounded-3">
                                            <i class="fas fa-exclamation-circle text-warning mt-1" style="font-size: 1.1rem;"></i>
                                            <div>
                                                <strong class="text-uppercase" style="font-size: 0.78rem;"><asp:Literal runat="server" Text="<%$ Resources:Strings, Driver_OrderNotesTitle %>" /></strong><br />
                                                <asp:Label ID="lblOrderNotes" runat="server" CssClass="fw-semibold text-dark" Style="font-size: 0.9rem;"></asp:Label>
                                            </div>
                                        </div>
                                    </asp:PlaceHolder>

                                    <!-- Contenido del Paquete -->
                                    <div class="bg-light p-3 rounded-3 mb-3">
                                        <h6 class="fw-bold text-uppercase" style="font-size: 0.8rem; color: #888;"><asp:Literal runat="server" Text="<%$ Resources:Strings, Driver_PackageContents %>" /></h6>
                                        <asp:Label ID="lblPackageContents" runat="server" CssClass="fw-semibold text-dark" Style="font-size: 0.95rem;"></asp:Label>
                                    </div>

                                    <!-- Mapa Leaflet Interactivo -->
                                    <div id="missionMap" style="height: 280px; width: 100%; border-radius: 12px; border: 2px solid #E4E7ED; z-index: 1; margin-top: 10px;"></div>
                                    <div id="mapStatusLabel" style="font-size: 0.78rem; color: #888; margin-top: 6px; min-height: 16px;"><i class="fas fa-spinner fa-spin me-1"></i> <asp:Literal runat="server" Text="<%$ Resources:Strings, Driver_GettingGps %>" /></div>

                                    <asp:HiddenField ID="hfDestLat" ClientIDMode="Static" runat="server" />
                                    <asp:HiddenField ID="hfDestLng" ClientIDMode="Static" runat="server" />
                                </div>
                            </div>

                            <div class="d-flex gap-2">
                                <asp:LinkButton ID="btnCancelMission" runat="server" CssClass="btn btn-outline-danger w-50 py-3 fw-bold rounded-3 text-decoration-none text-center" OnClick="btnCancelMission_Click" OnClientClick='<%# "return confirm(\"" + GetGlobalResourceObject("Strings", "Driver_CancelConfirm") + "\");" %>'>
                                    <asp:Literal runat="server" Text="<%$ Resources:Strings, Driver_CancelTrip %>" />
                                </asp:LinkButton>
                                <asp:LinkButton ID="btnCompleteMission" runat="server" CssClass="btn action-btn-huge btn-warning w-100 text-decoration-none text-center" OnClick="btnCompleteMission_Click" OnClientClick='<%# "return confirm(\"" + GetGlobalResourceObject("Strings", "Driver_CompleteConfirm") + "\");" %>'>
                                    <asp:Literal runat="server" Text="<%$ Resources:Strings, Driver_MarkDelivered %>" />
                                </asp:LinkButton>
                            </div>
                        </asp:View>

                    </asp:MultiView>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
    </form>

    <script type="text/javascript">
        var driverMap = null;
        var destMarker = null;
        var driverCurrentMarker = null;
        var routingControl = null;
        var trackingWatchId = null;
        var radarTimer = null;

        // Cargar cadenas traducidas para los mapas
        var strDropoffPoint = '<%= GetGlobalResourceObject("Strings", "Driver_DropoffPoint") %>';
        var strCustomerAddress = '<%= GetGlobalResourceObject("Strings", "Driver_CustomerAddress") %>';
        var strYourLocation = '<%= GetGlobalResourceObject("Strings", "Driver_YourLocation") %>';
        var strLiveGps = '<%= GetGlobalResourceObject("Strings", "Driver_LiveGps") %>';
        var strGettingGps = '<%= GetGlobalResourceObject("Strings", "Driver_GettingGps") %>';

        // 1. Inicializa el mapa y el destino
        function initMissionMap() {
            var latField = document.getElementById('hfDestLat');
            var lngField = document.getElementById('hfDestLng');

            if (!latField || !lngField || latField.value === '' || lngField.value === '') {
                return;
            }

            var destLat = parseFloat(latField.value);
            var destLng = parseFloat(lngField.value);

            // Limpieza profunda para UpdatePanels
            if (driverMap !== null) {
                driverMap.off();
                driverMap.remove();
                driverMap = null;
                destMarker = null;
                driverCurrentMarker = null;
                routingControl = null;
            }

            // Crear mapa con tiles CARTO Voyager
            driverMap = L.map('missionMap', { zoomControl: true }).setView([destLat, destLng], 14);

            L.tileLayer('https://{s}.basemaps.cartocdn.com/rastertiles/voyager/{z}/{x}/{y}{r}.png', {
                attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors',
                subdomains: 'abcd',
                maxZoom: 20
            }).addTo(driverMap);

            // Pin rojo: destino de entrega
            var destinationIcon = L.icon({
                iconUrl: 'https://raw.githubusercontent.com/pointhi/leaflet-color-markers/master/img/marker-icon-2x-red.png',
                shadowUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.9.4/images/marker-shadow.png',
                iconSize: [25, 41],
                iconAnchor: [12, 41],
                popupAnchor: [1, -34],
                shadowSize: [41, 41]
            });

            destMarker = L.marker([destLat, destLng], { icon: destinationIcon })
                .addTo(driverMap)
                .bindPopup('<b><i class="fas fa-map-marker-alt" style="color:#e53e3e"></i> ' + strDropoffPoint + '</b><br>' + strCustomerAddress)
                .openPopup();

            setTimeout(function () {
                if (driverMap) driverMap.invalidateSize();
            }, 400);

            setMapStatus('<i class="fas fa-satellite-dish me-1"></i> ' + strGettingGps, '#D47A00');

            // Arrancar el GPS
            startDriverTracking();
        }

        // 2. Activar el GPS del dispositivo
        function startDriverTracking() {
            if (navigator.geolocation) {
                if (trackingWatchId !== null) navigator.geolocation.clearWatch(trackingWatchId);

                trackingWatchId = navigator.geolocation.watchPosition(updateAndSendLocation, handleGpsError, {
                    enableHighAccuracy: true,
                    maximumAge: 0,
                    timeout: 15000
                });
            } else {
                Swal.fire('Error', 'Your browser does not support geolocation.', 'error');
            }
        }

        // 3. Actualizar mapa y enviar posición al servidor
        function updateAndSendLocation(position) {
            var lat = position.coords.latitude;
            var lng = position.coords.longitude;
            var accuracy = position.coords.accuracy;

            setMapStatus('<i class="fas fa-circle me-1" style="color:#10b981"></i> GPS active &mdash; Accuracy: ~' + Math.round(accuracy) + ' m', '#333');

            if (driverMap !== null) {
                var isFirstTime = (driverCurrentMarker === null);

                if (isFirstTime) {
                    var driverIcon = L.icon({
                        iconUrl: 'https://raw.githubusercontent.com/pointhi/leaflet-color-markers/master/img/marker-icon-2x-blue.png',
                        shadowUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.9.4/images/marker-shadow.png',
                        iconSize: [25, 41],
                        iconAnchor: [12, 41],
                        popupAnchor: [1, -34],
                        shadowSize: [41, 41]
                    });
                    driverCurrentMarker = L.marker([lat, lng], { icon: driverIcon })
                        .addTo(driverMap)
                        .bindPopup('<b><i class="fas fa-motorcycle" style="color:#3182ce"></i> ' + strYourLocation + '</b><br>' + strLiveGps);
                } else {
                    driverCurrentMarker.setLatLng([lat, lng]);
                }

                // Actualizar la ruta sobre calles
                if (destMarker !== null) {
                    var destLatLng = destMarker.getLatLng();

                    if (routingControl === null) {
                        routingControl = L.Routing.control({
                            waypoints: [
                                L.latLng(lat, lng),
                                L.latLng(destLatLng.lat, destLatLng.lng)
                            ],
                            router: L.Routing.osrmv1({
                                language: 'es',
                                profile: 'driving'
                            }),
                            lineOptions: {
                                styles: [{ color: '#FFC800', opacity: 0.9, weight: 5 }]
                            },
                            createMarker: function () { return null; },
                            addWaypoints: false,
                            routeWhileDragging: false,
                            fitSelectedRoutes: false,
                            show: false
                        }).addTo(driverMap);

                        var routingContainer = document.querySelector('.leaflet-routing-container');
                        if (routingContainer) routingContainer.style.display = 'none';

                    } else {
                        routingControl.setWaypoints([
                            L.latLng(lat, lng),
                            L.latLng(destLatLng.lat, destLatLng.lng)
                        ]);
                    }
                }

                if (isFirstTime) {
                    if (destMarker !== null) {
                        var group = new L.featureGroup([destMarker, driverCurrentMarker]);
                        driverMap.fitBounds(group.getBounds().pad(0.25));
                    } else {
                        driverMap.setView([lat, lng], 16);
                    }
                }
            }

            // Enviar posición al servidor vía AJAX
            $.ajax({
                type: 'POST',
                url: 'DeliveryDashboard.aspx/UpdateLocation',
                data: JSON.stringify({ currentLat: lat, currentLng: lng }),
                contentType: 'application/json; charset=utf-8',
                dataType: 'json',
                success: function () { },
                error: function (xhr, status, error) {
                    console.error('Location send error: ' + error);
                }
            });
        }

        function handleGpsError(error) {
            var msg = '';
            switch (error.code) {
                case error.PERMISSION_DENIED:
                    msg = 'GPS permission denied. Enable location in settings.';
                    break;
                case error.POSITION_UNAVAILABLE:
                    msg = 'Location unavailable. Check GPS signal.';
                    break;
                case error.TIMEOUT:
                    msg = 'GPS timeout. Retrying...';
                    break;
                default:
                    msg = 'GPS error: ' + error.message;
            }
            setMapStatus('<i class="fas fa-exclamation-triangle me-1" style="color:#e53e3e"></i> ' + msg, '#e53e3e');
        }

        function setMapStatus(html, color) {
            var el = document.getElementById('mapStatusLabel');
            if (el) { el.innerHTML = html; el.style.color = color || '#888'; }
        }

        // Auto-refresh silencioso para la piscina de órdenes (Radar) cada 15s
        function checkAutoRefreshRadar() {
            var isDutyChecked = document.getElementById('chkDutySwitch')?.checked;
            var isMissionActive = document.getElementById('missionMap') !== null;

            if (isDutyChecked && !isMissionActive) {
                if (!radarTimer) {
                    radarTimer = setInterval(function () {
                        var btnHidden = document.getElementById('btnHiddenDuty');
                        if (btnHidden) btnHidden.click();
                    }, 15000);
                }
            } else {
                if (radarTimer) {
                    clearInterval(radarTimer);
                    radarTimer = null;
                }
            }
        }

        // Inicialización en carga de página y re-binds de UpdatePanel
        document.addEventListener('DOMContentLoaded', function () {
            initMissionMap();
            checkAutoRefreshRadar();

            if (typeof Sys !== 'undefined' && Sys.WebForms && Sys.WebForms.PageRequestManager) {
                Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
                    initMissionMap();
                    checkAutoRefreshRadar();
                });
            }
        });
    </script>
</body>
</html>