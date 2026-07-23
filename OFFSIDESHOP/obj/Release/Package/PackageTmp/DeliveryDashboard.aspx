<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="DeliveryDashboard.aspx.cs" Inherits="OFFSIDESHOP.DeliveryDashboard" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1, maximum-scale=1, user-scalable=0" />
    <title>Driver Hub | OffsideShop</title>

    <link href="https://fonts.googleapis.com/css?family=Montserrat:400,700,900" rel="stylesheet" />
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.2.3/dist/css/bootstrap.min.css" rel="stylesheet" />
    <script src="https://use.fontawesome.com/releases/v6.3.0/js/all.js" crossorigin="anonymous"></script>
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>

    <!-- Leaflet para el Mapa del Repartidor -->
    <link rel="stylesheet" href="https://unpkg.com/leaflet@1.9.4/dist/leaflet.css" />
    <script src="https://ajax.googleapis.com/ajax/libs/jquery/3.6.0/jquery.min.js"></script>
    <script src="https://unpkg.com/leaflet@1.9.4/dist/leaflet.js"></script>

    <style>
        body {
            background-color: #f4f6f9;
            font-family: 'Montserrat', sans-serif;
            padding-bottom: 20px;
        }

        .driver-nav {
            background-color: #1a1a1a;
            padding: 15px 20px;
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

        .btn-logout {
            background: none;
            border: none;
            color: #fff;
            font-size: 1.2rem;
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
            width: 50px;
            height: 50px;
            display: flex;
            align-items: center;
            justify-content: center;
            font-size: 1.2rem;
            text-decoration: none;
            box-shadow: 0 4px 10px rgba(16, 185, 129, 0.4);
        }

            .call-btn:hover {
                color: white;
                background-color: #059669;
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
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <asp:ScriptManager runat="server" />

        <!-- HEADER DEL REPARTIDOR -->
        <div class="driver-nav">
            <img src="assets/img/offsideshop_logo_white_letras.png" alt="Logo" />
            <div class="d-flex align-items-center gap-3">
                <asp:UpdatePanel runat="server">
                    <ContentTemplate>
                        <div class="form-check form-switch m-0 d-flex align-items-center">
                            <!-- Usamos un input HTML nativo para que Bootstrap 5 dibuje el Switch a la perfección -->
                            <input type="checkbox" id="chkDutySwitch" runat="server" class="form-check-input shadow-none" style="width: 45px; height: 25px; cursor: pointer; margin-top: 0;" onchange="document.getElementById('btnHiddenDuty').click();" />
                            <!-- Botón oculto que dispara el AutoPostBack hacia tu código C# silenciosamente -->
                            <asp:Button ID="btnHiddenDuty" ClientIDMode="Static" runat="server" OnClick="chkDuty_CheckedChanged" Style="display: none;" />

                            <asp:Label ID="lblDutyStatus" runat="server" CssClass="status-badge bg-offline ms-2" Text="Offline"></asp:Label>
                        </div>
                    </ContentTemplate>
                </asp:UpdatePanel>
                <asp:LinkButton ID="btnLogout" runat="server" CssClass="btn-logout" OnClick="btnLogout_Click"><i class="fas fa-sign-out-alt"></i></asp:LinkButton>
            </div>
        </div>

        <div class="container mt-4">
            <asp:UpdatePanel ID="upMain" runat="server">
                <ContentTemplate>
                    <asp:MultiView ID="mvDriver" runat="server" ActiveViewIndex="0">

                        <!-- VISTA 1: RADAR DE PEDIDOS DISPONIBLES -->
                        <asp:View ID="viewRadar" runat="server">
                            <h5 class="fw-bold mb-3 text-uppercase text-muted" style="letter-spacing: 1px;">Available Deliveries</h5>

                            <asp:PlaceHolder ID="phOffline" runat="server">
                                <div class="empty-state">
                                    <i class="fas fa-bed"></i>
                                    <h4 class="fw-bold text-dark">You are Offline</h4>
                                    <p>Toggle your status to 'On Duty' to start receiving delivery requests.</p>
                                </div>
                            </asp:PlaceHolder>

                            <asp:PlaceHolder ID="phOnline" runat="server" Visible="false">
                                <asp:Repeater ID="rptRadar" runat="server" OnItemCommand="rptRadar_ItemCommand">
                                    <ItemTemplate>
                                        <div class="radar-card">
                                            <div class="d-flex justify-content-between align-items-start mb-2">
                                                <div>
                                                    <span class="badge bg-dark mb-2">Order #<%# Eval("Id_Order") %></span>
                                                    <div class="radar-city"><%# Eval("city_name") %> - <%# Eval("Municipality_Name") %></div>
                                                    <div class="radar-distance"><i class="fas fa-box me-1"></i><%# Eval("TotalItems") %> Items to deliver</div>
                                                </div>
                                                <div class="text-end">
                                                    <div class="text-muted" style="font-size: 12px; font-weight: 700;">TO COLLECT</div>
                                                    <div class="radar-price">$<%# Convert.ToDecimal(Eval("Total")).ToString("F2") %></div>
                                                </div>
                                            </div>
                                            <hr style="border-color: #eee;" />
                                            <asp:Button ID="btnAccept" runat="server" Text="Accept Delivery" CommandName="ACCEPT" CommandArgument='<%# Eval("Id_Order") %>' CssClass="btn btn-dark w-100 fw-bold py-2 text-warning" />
                                        </div>
                                    </ItemTemplate>
                                </asp:Repeater>

                                <asp:PlaceHolder ID="phNoOrders" runat="server" Visible="false">
                                    <div class="empty-state">
                                        <i class="fas fa-radar"></i>
                                        <h4 class="fw-bold text-dark">Searching for orders...</h4>
                                        <p>There are no packages ready for delivery at the moment. We will refresh automatically.</p>
                                    </div>
                                </asp:PlaceHolder>
                            </asp:PlaceHolder>
                        </asp:View>

                        <!-- VISTA 2: MISIÓN ACTIVA (EN RUTA) -->
                        <asp:View ID="viewMission" runat="server">
                            <div class="mission-header shadow-sm">
                                <span class="text-uppercase" style="letter-spacing: 2px; font-size: 0.8rem; color: #aaa;">Active Trip</span>
                                <h2>ORDER #<asp:Label ID="lblMissionOrderId" runat="server"></asp:Label></h2>
                            </div>

                            <div class="card border-0 shadow-sm rounded-4 mb-4">
                                <div class="card-body p-4">
                                    <div class="d-flex justify-content-between align-items-center mb-4">
                                        <div>
                                            <h5 class="fw-bold mb-1">
                                                <asp:Label ID="lblClientName" runat="server"></asp:Label></h5>
                                            <p class="text-muted mb-0">
                                                <i class="fas fa-map-marker-alt text-danger me-2"></i>
                                                <asp:Label ID="lblClientAddress" runat="server"></asp:Label>
                                            </p>
                                        </div>
                                        <a id="btnCallClient" runat="server" href="#" class="call-btn">
                                            <i class="fas fa-phone-alt"></i>
                                        </a>
                                    </div>

                                    <div class="bg-light p-3 rounded-3 mb-3">
                                        <h6 class="fw-bold text-uppercase" style="font-size: 0.8rem; color: #888;">Package Contents:</h6>
                                        <asp:Label ID="lblPackageContents" runat="server" CssClass="fw-semibold text-dark" Style="font-size: 0.95rem;"></asp:Label>
                                    </div>

                                    <div id="missionMap" style="height: 280px; width: 100%; border-radius: 12px; border: 2px solid #E4E7ED; z-index: 1; margin-top: 10px;"></div>
                                    <div id="mapStatusLabel" style="font-size: 0.78rem; color: #888; margin-top: 6px; min-height: 16px;"><i class="fas fa-spinner fa-spin me-1"></i> Getting your GPS location...</div>

                                    <asp:HiddenField ID="hfDestLat" ClientIDMode="Static" runat="server" />
                                    <asp:HiddenField ID="hfDestLng" ClientIDMode="Static" runat="server" />
                                </div>
                            </div>

                            <div class="d-flex gap-2">
                                <asp:Button ID="btnCancelMission" runat="server" Text="Cancel Trip" CssClass="btn btn-outline-danger w-50 py-3 fw-bold rounded-3" OnClientClick="return confirm('Are you sure you want to drop this order? It will return to the radar.');" OnClick="btnCancelMission_Click" />

                                <asp:Button ID="btnCompleteMission" runat="server" Text="Mark Delivered" CssClass="btn action-btn-huge btn-warning w-100" OnClick="btnCompleteMission_Click" OnClientClick="return confirm('Confirm that you have handed the package to the customer?');" />
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
        var routeLine = null;
        var trackingWatchId = null;

        // 1. Inicializa el mapa y el destino
        function initMissionMap() {
            var latField = document.getElementById('hfDestLat');
            var lngField = document.getElementById('hfDestLng');

            if (!latField || !lngField) {
                console.log('Esperando coordenadas...');
                return;
            }

            var destLat = latField.value ? parseFloat(latField.value) : 13.6929;
            var destLng = lngField.value ? parseFloat(lngField.value) : -89.2182;
            var hasDestCoords = (latField.value !== '' && lngField.value !== '');

            // Limpieza profunda para UpdatePanels
            if (driverMap !== null) {
                driverMap.off();
                driverMap.remove();
                driverMap = null;
                destMarker = null;
                driverCurrentMarker = null;
                routeLine = null;
            }

            // Crear mapa con tiles de mayor calidad (CARTO Voyager)
            driverMap = L.map('missionMap', { zoomControl: true }).setView([destLat, destLng], 14);

            L.tileLayer('https://{s}.basemaps.cartocdn.com/rastertiles/voyager/{z}/{x}/{y}{r}.png', {
                attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors &copy; <a href="https://carto.com/attributions">CARTO</a>',
                subdomains: 'abcd',
                maxZoom: 20
            }).addTo(driverMap);

            // Pin rojo: destino de entrega
            if (hasDestCoords) {
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
                    .bindPopup('<b><i class="fas fa-map-marker-alt" style="color:#e53e3e"></i> Drop-off Point</b><br>Customer delivery address')
                    .openPopup();
            }

            setTimeout(function () {
                if (driverMap) driverMap.invalidateSize();
            }, 400);

            setMapStatus('<i class="fas fa-satellite-dish me-1"></i> Connecting to GPS...', '#D47A00');

            // Arrancar el radar GPS
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

        // 3. Actualizar mapa y enviar posicion al servidor
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
                        .bindPopup('<b><i class="fas fa-motorcycle" style="color:#3182ce"></i> Your Location</b><br>Live GPS position');
                } else {
                    driverCurrentMarker.setLatLng([lat, lng]);
                }

                // Actualizar la línea de ruta entre driver y destino
                if (destMarker !== null) {
                    var destLatLng = destMarker.getLatLng();
                    if (routeLine !== null) {
                        driverMap.removeLayer(routeLine);
                    }
                    routeLine = L.polyline([[lat, lng], [destLatLng.lat, destLatLng.lng]], {
                        color: '#FFC800',
                        weight: 3,
                        dashArray: '8, 6',
                        opacity: 0.85
                    }).addTo(driverMap);
                }

                // En la primera posición: hacer fitBounds para ver ambos pines
                if (isFirstTime) {
                    if (destMarker !== null) {
                        var group = new L.featureGroup([destMarker, driverCurrentMarker]);
                        driverMap.fitBounds(group.getBounds().pad(0.25));
                    } else {
                        driverMap.setView([lat, lng], 16);
                    }
                }
            }

            // Enviar posicion al servidor via AJAX
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
                    msg = 'GPS permission denied. Enable location in your browser settings.';
                    break;
                case error.POSITION_UNAVAILABLE:
                    msg = 'Location unavailable. Check your GPS signal.';
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

        // 4. Iniciar al cargar la vista
        document.addEventListener('DOMContentLoaded', function () {
            initMissionMap();

            if (typeof Sys !== 'undefined' && Sys.WebForms && Sys.WebForms.PageRequestManager) {
                Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
                    initMissionMap();
                });
            }
        });
    </script>
</body>
</html>
