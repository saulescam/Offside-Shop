<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="OrderDetail.aspx.cs" Inherits="OFFSIDESHOP.OrderDetail" %>

<%@ Register Src="~/FooterControl.ascx" TagPrefix="uc" TagName="Footer" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1, shrink-to-fit=no" />
    <title>OffsideShop - Order Details</title>

    <link rel="icon" type="image/x-icon" href="assets/favicon.ico" />
    <script src="https://use.fontawesome.com/releases/v6.3.0/js/all.js" crossorigin="anonymous"></script>

    <link href="https://fonts.googleapis.com/css?family=Montserrat:400,700" rel="stylesheet" type="text/css" />
    <link href="https://fonts.googleapis.com/css?family=Roboto+Slab:400,100,300,700" rel="stylesheet" type="text/css" />
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.2.3/dist/css/bootstrap.min.css" rel="stylesheet" />

    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>

    <link rel="stylesheet" href="https://unpkg.com/leaflet@1.9.4/dist/leaflet.css" />
    <script src="https://unpkg.com/leaflet@1.9.4/dist/leaflet.js"></script>

    <link href="css/styles.css" rel="stylesheet" />
    <link href="css/details.css" rel="stylesheet" />
    <style>
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

        .user-role {
            margin: 0;
            color: #FFC800;
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

        .badge {
            margin-left: auto;
            background-color: #D47A00;
            color: white;
            padding: 2px 6px;
            border-radius: 10px;
            font-size: 0.75rem;
            min-width: 18px;
            text-align: center;
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
<body id="page-top" class="d-flex flex-column min-vh-100 bg-light m-0">
    <form runat="server" class="flex-grow-1 d-flex flex-column">

        <nav class="navbar navbar-expand-lg navbar-dark fixed-top" id="mainNav" style="background-color: #1a1a1a !important; box-shadow: 0 2px 10px rgba(0,0,0,0.3); padding: 12px 0;">
            <div class="container">
                <a class="navbar-brand" href="Homepage.aspx">
                    <img src="assets/img/offsideshop_logo_white_letras.png" alt="OffsideShop Logo" style="max-height: 45px; width: auto;" />
                </a>
                <button class="navbar-toggler" type="button" data-bs-toggle="collapse" data-bs-target="#navbarResponsive">
                    <span class="navbar-toggler-icon"></span>
                </button>
                <div class="collapse navbar-collapse" id="navbarResponsive">

                    <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>

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
                                    <a href="Login.aspx" class="dropdown-item">
                                        <i class="fas fa-sign-in-alt"></i>Log in
                                    </a>
                                    <a href="SignUp.aspx" class="dropdown-item">
                                        <i class="fas fa-user-plus"></i>Sign up
                                    </a>
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
                                            <p class="user-fullname">
                                                <asp:Label ID="lblFullName" runat="server" Text="Cargando..."></asp:Label>
                                            </p>
                                            <p class="user-email">
                                                <asp:Label ID="lblUserEmail" runat="server" Text=""></asp:Label>
                                            </p>
                                        </div>
                                        <div class="dropdown-content">
                                            <asp:LinkButton ID="btnGoToAccount" runat="server" CssClass="dropdown-item" OnClick="btnGoToAccount_Click" CausesValidation="false">
                                                <i class="fas fa-user-cog"></i> My Account
                                            </asp:LinkButton>

                                            <asp:LinkButton ID="btnMyOrders" runat="server" CssClass="dropdown-item" OnClick="btnMyOrders_Click" CausesValidation="false">
                                                <i class="fas fa-clipboard-list"></i> My Orders
                                            </asp:LinkButton>

                                            <asp:LinkButton ID="btnNavCart" runat="server" CssClass="dropdown-item" OnClick="btnNavCart_Click" CausesValidation="false">
                                                <i class="fas fa-shopping-cart"></i>Cart 
                                                <span class="badge">
                                                    <asp:Label ID="lblCartCount" runat="server" Text="0"></asp:Label>
                                                </span>
                                            </asp:LinkButton>

                                            <asp:Button ID="btncerrar" runat="server" CssClass="dropdown-item btn-logout" Text="Log out" OnClick="btncerrar_Click" CausesValidation="false" />
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
                                    <p class="user-role">Administrator</p>
                                </div>
                                <div class="dropdown-content">
                                    <a href="MyAccount.aspx" class="dropdown-item">
                                        <i class="fas fa-user-cog"></i>My Account
                                    </a>
                                    <a href="Dashboard.aspx" class="dropdown-item">
                                        <i class="fas fa-chart-line"></i>Dashboard
                                    </a>
                                    <asp:Button ID="btnlogout" runat="server" CssClass="dropdown-item btn-logout" Text="Log out" OnClick="btncerrar_Click" CausesValidation="false" />
                                </div>
                            </div>
                        </div>
                    </asp:PlaceHolder>
                </div>
            </div>
        </nav>

        <div class="container" style="margin-top: 120px; padding-bottom: 60px;">

            <div class="mb-4">
                <a href="MyOrders.aspx" class="text-decoration-none" style="color: #666666; font-weight: 600; font-family: 'Montserrat', sans-serif; font-size: 14px;">
                    <i class="fas fa-arrow-left me-2"></i>Back to My Orders
                </a>
            </div>

            <div class="row justify-content-between align-items-center border-bottom pb-3 mb-4" style="border-color: #e0e0e0 !important;">
                <div class="col-12 col-md-6 text-center text-md-start">
                    <h2 style="color: #1a1a1a !important; font-family: 'Montserrat', sans-serif; font-weight: 700; letter-spacing: 1px; text-transform: uppercase; margin: 0;">Order Reference <span style="color: #ffc800;">#<asp:Label ID="lblOrderId" runat="server"></asp:Label></span>
                    </h2>
                    <p style="color: #666666 !important; margin: 5px 0 0 0;">
                        Placed on:
                        <asp:Label ID="lblOrderDate" runat="server"></asp:Label>
                    </p>
                </div>
                <div class="col-12 col-md-6 text-center text-md-end mt-3 mt-md-0">
                    <span style="color: #999999; text-transform: uppercase; font-size: 11px; font-weight: 600; display: block; letter-spacing: 0.5px; margin-bottom: 4px;">Status</span>
                    <asp:Label ID="lblStatusBadge" runat="server" CssClass="badge" Style="background-color: #1a1a1a !important; color: #ffc800 !important; border: 1px solid #ffc800; font-size: 14px; padding: 8px 18px; font-weight: 600; text-transform: uppercase; letter-spacing: 0.5px; border-radius: 20px;"></asp:Label>
                </div>
            </div>

            <!-- PANEL DINÁMICO DE ESTADO Y RASTREO -->
            <div class="row mb-4">
                <div class="col-12">
                    <div id="statusAlertBox" runat="server" class="alert d-flex align-items-center mb-0" role="alert" style="border-radius: 12px; box-shadow: 0 4px 15px rgba(0,0,0,0.05); padding: 20px;">
                        <i id="statusIcon" runat="server" class="fas fa-info-circle fa-2x me-3"></i>
                        <div>
                            <h5 id="statusTitle" runat="server" class="alert-heading fw-bold mb-1" style="font-family: 'Montserrat', sans-serif;">Status</h5>
                            <p id="statusDescription" runat="server" class="mb-0" style="font-size: 14px;">Loading order status...</p>
                        </div>

                        <!-- BOTÓN DE TRACKING (Se muestra solo si el estado es Shipped) -->
                        <div class="ms-auto" id="trackerButtonContainer" runat="server" visible="false">
                            <asp:LinkButton ID="btnTrackOrder" runat="server" CssClass="btn btn-dark fw-bold text-warning rounded-pill px-4 shadow-sm" OnClientClick="openTrackingModal(); return false;">
        <i class="fas fa-map-marked-alt me-2"></i> Track Delivery
    </asp:LinkButton>
                        </div>

                        <asp:HiddenField ID="hfTrackOrderId" ClientIDMode="Static" runat="server" />
                        <asp:HiddenField ID="hfOrderLat" ClientIDMode="Static" runat="server" />
                        <asp:HiddenField ID="hfOrderLng" ClientIDMode="Static" runat="server" />
                    </div>
                </div>
            </div>
            <!-- FIN PANEL DINÁMICO -->

            <div class="row g-4 mb-5">
                <div class="col-12 col-md-6 d-flex">
                    <div class="w-100 style-card" style="background-color: #ffffff !important; border: 1px solid #e0e0e0 !important; border-radius: 12px; padding: 25px; box-shadow: 0px 4px 20px rgba(0,0,0,0.06);">
                        <h5 style="color: #1a1a1a !important; font-family: 'Montserrat', sans-serif; font-weight: 700; font-size: 15px; border-bottom: 1px solid #eeeeee; padding-bottom: 10px; margin-bottom: 15px; text-transform: uppercase; letter-spacing: 0.5px;">
                            <i class="fas fa-shipping-fast me-2" style="color: #ffc800;"></i>Shipping Address
                        </h5>
                        <p style="font-size: 14px; color: #333333; margin-bottom: 8px;">
                            <strong>Customer:</strong>
                            <asp:Label ID="lblCustomerName" runat="server"></asp:Label>
                        </p>
                        <p style="font-size: 14px; color: #333333; margin-bottom: 8px;">
                            <strong>Phone:</strong>
                            <asp:Label ID="lblPhone" runat="server"></asp:Label>
                        </p>
                        <p style="font-size: 14px; color: #333333; margin-bottom: 15px;">
                            <strong>Address:</strong>
                            <asp:Label ID="lblAddress" runat="server"></asp:Label>
                        </p>

                        <div class="d-flex flex-wrap gap-2 pt-2">
                            <asp:Label ID="lblCity" runat="server" CssClass="badge" Style="background-color: #f1f1f1 !important; color: #444444 !important; font-size: 12px; padding: 6px 12px; border-radius: 4px; font-weight: 600;"></asp:Label>
                            <asp:Label ID="lblMunicipality" runat="server" CssClass="badge" Style="background-color: #f1f1f1 !important; color: #444444 !important; font-size: 12px; padding: 6px 12px; border-radius: 4px; font-weight: 600;"></asp:Label>
                            <asp:Label ID="lblDistrict" runat="server" CssClass="badge" Style="background-color: #f1f1f1 !important; color: #444444 !important; font-size: 12px; padding: 6px 12px; border-radius: 4px; font-weight: 600;"></asp:Label>
                        </div>
                    </div>
                </div>

                <div class="col-12 col-md-6 d-flex">
                    <div class="w-100 style-card" style="background-color: #ffffff !important; border: 1px solid #e0e0e0 !important; border-radius: 12px; padding: 25px; box-shadow: 0px 4px 20px rgba(0,0,0,0.06);">
                        <h5 style="color: #1a1a1a !important; font-family: 'Montserrat', sans-serif; font-weight: 700; font-size: 15px; border-bottom: 1px solid #eeeeee; padding-bottom: 10px; margin-bottom: 15px; text-transform: uppercase; letter-spacing: 0.5px;">
                            <i class="fas fa-credit-card me-2" style="color: #ffc800;"></i>Payment Information
                        </h5>
                        <p style="font-size: 14px; color: #333333; margin-bottom: 8px;">
                            <strong>Method:</strong>
                            <asp:Label ID="lblPaymentMethod" runat="server"></asp:Label>
                        </p>

                        <asp:PlaceHolder ID="phPayPal" runat="server" Visible="false">
                            <p style="font-size: 14px; color: #333333; margin-bottom: 8px;">
                                <strong>Transaction ID:</strong> <span class="text-muted" style="font-family: monospace; font-size: 13px;">
                                    <asp:Label ID="lblTransactionId" runat="server"></asp:Label></span>
                            </p>
                        </asp:PlaceHolder>

                        <div class="mt-3 pt-2 border-top" style="border-color: #f5f5f5 !important;">
                            <span style="color: #888888; font-weight: 600; font-size: 11px; text-transform: uppercase; display: block; margin-bottom: 4px;">Order Notes:</span>
                            <p style="color: #555555; font-size: 13px; font-style: italic; margin: 0;">
                                <asp:Label ID="lblNotes" runat="server" Text="No notes provided."></asp:Label>
                            </p>
                        </div>
                    </div>
                </div>
            </div>

            <div class="row mb-4">
                <div class="col-12">
                    <div class="w-100 style-card" style="background-color: #ffffff !important; border: 1px solid #e0e0e0 !important; border-radius: 12px; padding: 25px; box-shadow: 0px 4px 20px rgba(0,0,0,0.06);">
                        <div style="color: #1a1a1a !important; font-family: 'Montserrat', sans-serif; font-size: 14px; font-weight: 700; letter-spacing: 0.5px; text-transform: uppercase; margin-bottom: 15px; border-bottom: 1px solid #eeeeee; padding-bottom: 10px;">
                            Items in your Order
                        </div>

                        <div class="table-responsive">
                            <table class="table align-middle w-100 m-0" style="border-color: #f1f1f1;">
                                <thead style="background-color: #1a1a1a; color: #ffffff; font-family: 'Montserrat', sans-serif; font-size: 12px; text-transform: uppercase; letter-spacing: 0.5px;">
                                    <tr>
                                        <th class="p-3" style="width: 50%; font-weight: 600; border: none; border-top-left-radius: 8px; border-bottom-left-radius: 8px;">Product Details</th>
                                        <th class="p-3 text-center" style="width: 15%; font-weight: 600; border: none;">Price</th>
                                        <th class="p-3 text-center" style="width: 15%; font-weight: 600; border: none;">Quantity</th>
                                        <th class="p-3 text-end" style="width: 20%; font-weight: 600; border: none; border-top-right-radius: 8px; border-bottom-right-radius: 8px;">Subtotal</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    <asp:Repeater ID="rptOrderProducts" runat="server">
                                        <ItemTemplate>
                                            <tr style="font-size: 14px;">
                                                <td class="p-3">
                                                    <div class="d-flex align-items-center">
                                                        <img src='<%# string.IsNullOrEmpty(Eval("ImageURL")?.ToString()) ? ResolveUrl("~/images/camisetas/default.png") : ResolveUrl("~/images/camisetas/" + Eval("ImageURL")) %>'
                                                            alt="Jersey" class="img-thumbnail me-3" style="width: 60px; height: 60px; object-fit: cover; border-radius: 6px; border-color: #e0e0e0;" />
                                                        <div>
                                                            <h6 style="color: #1a1a1a; font-weight: 700; margin: 0 0 4px 0;"><%# FormatJerseyName(Eval("ProductName")) %></h6>
                                                            <span class="badge" style="background-color: #1a1a1a; color: #ffffff; font-size: 11px; padding: 4px 8px;">Size: <%# Eval("Size") %></span>
                                                        </div>
                                                    </div>
                                                </td>
                                                <td class="p-3 text-center text-muted">$<%# Convert.ToDecimal(Eval("Price")).ToString("F2") %></td>
                                                <td class="p-3 text-center fw-bold" style="color: #1a1a1a;">x<%# Eval("Quantity") %></td>
                                                <td class="p-3 text-end fw-bold" style="color: #1a1a1a;">$<%# Convert.ToDecimal(Eval("Subtotal")).ToString("F2") %></td>
                                            </tr>
                                        </ItemTemplate>
                                    </asp:Repeater>
                                </tbody>
                            </table>
                        </div>
                    </div>
                </div>
            </div>

            <div class="row justify-content-end g-4">
                <div class="col-12 col-md-5 col-lg-4">
                    <div class="w-100 style-card mb-3" style="background-color: #ffffff !important; border: 1px solid #e0e0e0 !important; border-radius: 12px; padding: 25px; box-shadow: 0px 4px 20px rgba(0,0,0,0.06);">
                        <div class="d-flex justify-content-between mb-2" style="font-size: 14px;">
                            <span style="color: #777777;">Items Subtotal:</span>
                            <span style="color: #1a1a1a; font-weight: 600;">$<asp:Label ID="lblItemsSubtotal" runat="server"></asp:Label></span>
                        </div>
                        <div class="d-flex justify-content-between mb-3 pb-3 border-bottom" style="font-size: 14px; border-color: #eeeeee !important;">
                            <span style="color: #777777;">Shipping Cost:</span>
                            <asp:Label ID="lblShippingCost" runat="server" Style="font-weight: 600;"></asp:Label>
                        </div>
                        <div class="d-flex justify-content-between align-items-center">
                            <span style="color: #1a1a1a; font-family: 'Montserrat', sans-serif; font-weight: 700; font-size: 13px; text-transform: uppercase; letter-spacing: 0.5px;">Total Charged</span>
                            <span style="color: #1a1a1a !important; font-size: 24px; font-family: 'Montserrat', sans-serif; font-weight: 700;">$<asp:Label ID="lblOrderTotal" runat="server"></asp:Label></span>
                        </div>
                    </div>

                    <div class="text-end">
                        <asp:LinkButton ID="lnkCancelOrder" runat="server" CssClass="btn btn-danger w-100 mb-2 fw-bold py-2 shadow-sm"
                            Visible="false" OnClick="lnkAction_Click" CommandArgument="CANCEL" CausesValidation="false">
                            <i class="fas fa-times-circle me-2"></i> Cancel Order
                        </asp:LinkButton>

                        <asp:LinkButton ID="lnkRequestRefund" runat="server" CssClass="btn btn-warning w-100 fw-bold text-dark py-2 shadow-sm"
                            Visible="false" OnClick="lnkAction_Click" CommandArgument="REFUND" CausesValidation="false">
                            <i class="fas fa-undo me-2"></i> Request Refund
                        </asp:LinkButton>
                    </div>
                </div>
            </div>

        </div>

        <asp:PlaceHolder ID="phReasonModal" runat="server" Visible="false">
            <div class="modal fade show d-block" tabindex="-1" style="background: rgba(0,0,0,0.6); backdrop-filter: blur(3px);">
                <div class="modal-dialog modal-dialog-centered">
                    <div class="modal-content" style="border-radius: 12px; border: none; box-shadow: 0 5px 25px rgba(0,0,0,0.4);">
                        <div class="modal-header" style="background-color: #1a1a1a; color: #ffffff;">
                            <h5 class="modal-title fw-bold text-uppercase" style="font-family: 'Montserrat', sans-serif; font-size: 16px;">
                                <i class="fas fa-comment-dots me-2 text-warning"></i>
                                Provide a Reason for 
                                <asp:Literal ID="litActionType" runat="server"></asp:Literal>
                            </h5>
                            <asp:LinkButton ID="btnCloseModalTop" runat="server" CssClass="btn-close btn-close-white" OnClick="btnCloseModal_Click" CausesValidation="false" />
                        </div>
                        <div class="modal-body p-4">
                            <p class="text-muted small">Please select a reason from the list and explain why you want to proceed with this request.</p>

                            <div class="form-group mb-3">
                                <label class="fw-bold mb-2 small text-uppercase">Select a Reason:</label>
                                <asp:DropDownList ID="ddlReasons" runat="server" CssClass="form-select" Style="border-radius: 8px;"></asp:DropDownList>
                            </div>

                            <div class="form-group">
                                <label class="fw-bold mb-2 small text-uppercase">Additional Comments (Optional):</label>
                                <asp:TextBox ID="txtReason" runat="server" TextMode="MultiLine" Rows="3" CssClass="form-control"
                                    placeholder="Provide more details here... (Required if 'Other' is selected)" Style="border-radius: 8px; resize: none;"></asp:TextBox>
                            </div>

                            <asp:Label ID="lblModalError" runat="server" CssClass="text-danger small d-block mt-2" Visible="false"></asp:Label>
                        </div>
                        <div class="modal-footer" style="background-color: #f8f9fa;">
                            <asp:Button ID="btnCancelModal" runat="server" Text="Go Back" CssClass="btn btn-secondary btn-sm rounded-pill px-4" OnClick="btnCloseModal_Click" CausesValidation="false" />
                            <asp:Button ID="btnSubmitAction" runat="server" Text="Submit Request" CssClass="btn btn-dark btn-sm rounded-pill px-4 text-warning fw-bold"
                                OnClick="btnSubmitAction_Click" />
                        </div>
                    </div>
                </div>
            </div>
        </asp:PlaceHolder>

        <div class="modal fade" id="trackingModal" tabindex="-1" aria-hidden="true">
            <div class="modal-dialog modal-lg modal-dialog-centered">
                <div class="modal-content rounded-4 border-0 shadow-lg">
                    <div class="modal-header bg-dark text-white border-0 rounded-top-4">
                        <h5 class="modal-title fw-bold text-warning">
                            <i class="fas fa-motorcycle me-2"></i>Live Tracking
                        </h5>
                        <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal" aria-label="Close"></button>
                    </div>
                    <div class="modal-body p-0">
                        <div id="liveMap" style="height: 420px; width: 100%; z-index: 1;"></div>
                        <div id="trackingStatusLabel" style="padding: 8px 16px; font-size: 0.8rem; color: #555; background: #f8f9fa; border-top: 1px solid #eee; min-height: 34px;">
                            <i class="fas fa-spinner fa-spin me-1"></i> Connecting to delivery driver...
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </form>

    <uc:Footer ID="ControlFooter" runat="server" />

    <script type="text/javascript">
        var liveMap = null;
        var homeMarker = null;
        var driverMarker = null;
        var trackingInterval = null;
        var liveRouteLine = null;

        function openTrackingModal() {
            var orderId = document.getElementById('hfTrackOrderId').value;
            var lat = parseFloat(document.getElementById('hfOrderLat').value);
            var lng = parseFloat(document.getElementById('hfOrderLng').value);

            var modalEl = document.getElementById('trackingModal');
            var modal = new bootstrap.Modal(modalEl);
            modal.show();

            modalEl.addEventListener('shown.bs.modal', function () {
                if (liveMap !== null) { liveMap.remove(); liveMap = null; homeMarker = null; driverMarker = null; liveRouteLine = null; }

                // Tiles de alta calidad CARTO Voyager
                liveMap = L.map('liveMap').setView([lat, lng], 15);
                L.tileLayer('https://{s}.basemaps.cartocdn.com/rastertiles/voyager/{z}/{x}/{y}{r}.png', {
                    attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors &copy; <a href="https://carto.com/attributions">CARTO</a>',
                    subdomains: 'abcd',
                    maxZoom: 20
                }).addTo(liveMap);

                // Pin rojo (Tu destino)
                var homeIcon = L.icon({
                    iconUrl: 'https://raw.githubusercontent.com/pointhi/leaflet-color-markers/master/img/marker-icon-2x-red.png',
                    shadowUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.9.4/images/marker-shadow.png',
                    iconSize: [25, 41], iconAnchor: [12, 41], popupAnchor: [1, -34], shadowSize: [41, 41]
                });
                homeMarker = L.marker([lat, lng], { icon: homeIcon })
                    .addTo(liveMap)
                    .bindPopup('<b><i class="fas fa-home" style="color:#e53e3e"></i> Your Delivery Address</b>')
                    .openPopup();

                setTrackingStatus('<i class="fas fa-spinner fa-spin me-1"></i> Connecting to delivery driver...', '#888');

                // Arrancar seguimiento
                fetchDriverLocation(orderId);
                trackingInterval = setInterval(function () { fetchDriverLocation(orderId); }, 3000);

                setTimeout(function () { if (liveMap) liveMap.invalidateSize(); }, 500);
            }, { once: true });

            modalEl.addEventListener('hidden.bs.modal', function () {
                if (trackingInterval !== null) { clearInterval(trackingInterval); trackingInterval = null; }
                if (liveMap !== null) { liveMap.remove(); liveMap = null; homeMarker = null; driverMarker = null; liveRouteLine = null; }
            }, { once: true });
        }

        function fetchDriverLocation(orderId) {
            $.ajax({
                type: 'POST',
                url: 'OrderDetail.aspx/GetLiveDriverLocation',
                cache: false,
                data: JSON.stringify({ orderId: parseInt(orderId) }),
                contentType: 'application/json; charset=utf-8',
                dataType: 'json',
                success: function (response) {
                    if (response.d && response.d !== 'null') {
                        var coords = JSON.parse(response.d);
                        var dLat = parseFloat(coords.lat);
                        var dLng = parseFloat(coords.lng);

                        setTrackingStatus('<i class="fas fa-circle me-1" style="color:#10b981"></i> Driver located &mdash; updating every 3 seconds', '#28a745');

                        if (!driverMarker) {
                            var driverIcon = L.icon({
                                iconUrl: 'https://raw.githubusercontent.com/pointhi/leaflet-color-markers/master/img/marker-icon-2x-blue.png',
                                shadowUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.9.4/images/marker-shadow.png',
                                iconSize: [25, 41], iconAnchor: [12, 41], popupAnchor: [1, -34], shadowSize: [41, 41]
                            });
                            driverMarker = L.marker([dLat, dLng], { icon: driverIcon })
                                .addTo(liveMap)
                                .bindPopup('<b><i class="fas fa-motorcycle" style="color:#3182ce"></i> Your Delivery Driver</b><br>Live GPS position');

                            // Primera vez: fitBounds para ver ambos pines
                            if (homeMarker) {
                                var group = new L.featureGroup([homeMarker, driverMarker]);
                                liveMap.fitBounds(group.getBounds().pad(0.25));
                            }
                        } else {
                            // Mover suavemente: Leaflet actualiza la posicion
                            driverMarker.setLatLng([dLat, dLng]);
                        }

                        // Actualizar linea de ruta entre driver y destino
                        if (homeMarker) {
                            var destLatLng = homeMarker.getLatLng();
                            if (liveRouteLine !== null) liveMap.removeLayer(liveRouteLine);
                            liveRouteLine = L.polyline([[dLat, dLng], [destLatLng.lat, destLatLng.lng]], {
                                color: '#FFC800',
                                weight: 3,
                                dashArray: '8, 6',
                                opacity: 0.9
                            }).addTo(liveMap);
                        }

                        // Mantener el driver visible si sale del bounding box
                        if (liveMap && !liveMap.getBounds().contains([dLat, dLng])) {
                            liveMap.panTo([dLat, dLng]);
                        }
                    } else {
                        setTrackingStatus('<i class="fas fa-clock me-1"></i> Waiting for driver GPS signal...', '#888');
                    }
                },
                error: function () {
                    setTrackingStatus('<i class="fas fa-exclamation-triangle me-1" style="color:#e53e3e"></i> Error connecting to server. Retrying...', '#dc3545');
                }
            });
        }

        function setTrackingStatus(html, color) {
            var el = document.getElementById('trackingStatusLabel');
            if (el) { el.innerHTML = html; el.style.color = color || '#555'; }
        }
    </script>

    <script src="https://ajax.googleapis.com/ajax/libs/jquery/3.6.0/jquery.min.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.2.3/dist/js/bootstrap.bundle.min.js"></script>
</body>
</html>
