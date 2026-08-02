<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Cart.aspx.cs" Inherits="OFFSIDESHOP.Cart" %>

<%@ Register Src="~/FooterControl.ascx" TagPrefix="uc" TagName="Footer" %>
<!DOCTYPE html>
<html lang="es">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1, shrink-to-fit=no" />
    <title>Your Cart - OffsideShop</title>

    <link rel="icon" type="image/x-icon" href="assets/favicon.ico" />
    <script src="https://use.fontawesome.com/releases/v6.3.0/js/all.js" crossorigin="anonymous"></script>
    <link href="https://fonts.googleapis.com/css?family=Montserrat:400,700" rel="stylesheet" type="text/css" />
    <link href="https://fonts.googleapis.com/css?family=Roboto+Slab:400,100,300,700" rel="stylesheet" type="text/css" />
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.2.3/dist/css/bootstrap.min.css" rel="stylesheet" />
    <link href="css/styles.css" rel="stylesheet" />

    <style>
        body {
            font-family: 'Montserrat', sans-serif;
            background-color: #f8f9fa;
            min-height: 100vh;
            padding-top: 120px;
        }

        .navbar {
            background: #000000 !important;
            box-shadow: 0 4px 20px rgba(0, 0, 0, 0.5);
            padding: 12px 0;
            border-bottom: 1px solid rgba(255, 255, 255, 0.05);
        }

        .navbar-brand img {
            max-height: 45px;
            width: auto;
        }

        .btn-logout {
            color: #000000;
            border: 2px solid #FFC800;
            background: linear-gradient(135deg, #FFC800 0%, #D4A000 100%);
            border-radius: 25px;
            padding: 8px 24px;
            font-weight: 600;
            transition: all 0.3s ease;
        }

            .btn-logout:hover {
                background: linear-gradient(135deg, #FFE066 0%, #FFC800 100%);
                border-color: #FFE066;
                transform: translateY(-2px);
                box-shadow: 0 5px 15px rgba(255, 200, 0, 0.3);
                color: #000000;
            }

        .cart-container {
            background: #ffffff;
            border-radius: 20px;
            box-shadow: 0 10px 30px rgba(0, 0, 0, 0.05);
            padding: 40px;
            margin-bottom: 50px;
        }

        .cart-title {
            font-weight: 700;
            color: #111111;
            border-bottom: 3px solid #FFC800;
            display: inline-block;
            padding-bottom: 10px;
            margin-bottom: 30px;
        }

        /* â”€â”€ Tabla â”€â”€ */
        .table-custom {
            border-collapse: separate;
            border-spacing: 0 15px;
            width: 100%;
        }

            /* CENTRA TODO â€” encabezados y celdas */
            .table-custom th,
            .table-custom td {
                text-align: center !important;
                vertical-align: middle !important;
            }

            .table-custom thead th {
                background-color: #000000 !important;
                color: #ffffff !important;
                border: none;
                padding: 15px;
                font-weight: 600;
                text-transform: uppercase;
                font-size: 0.85rem;
                letter-spacing: 1px;
            }

            .table-custom tbody tr {
                background-color: #ffffff;
                box-shadow: 0 5px 15px rgba(0,0,0,0.02);
                transition: all 0.2s ease;
                border: 1px solid #f1f1f1;
            }

                .table-custom tbody tr:hover {
                    transform: translateY(-2px);
                    box-shadow: 0 8px 20px rgba(255, 200, 0, 0.08);
                    border-color: #FFC800;
                }

            .table-custom tbody td {
                padding: 20px 15px;
                border-top: 1px solid #f1f1f1;
                border-bottom: 1px solid #f1f1f1;
            }

                .table-custom tbody td:first-child {
                    border-left: 1px solid #f1f1f1;
                    border-top-left-radius: 10px;
                    border-bottom-left-radius: 10px;
                }

                .table-custom tbody td:last-child {
                    border-right: 1px solid #f1f1f1;
                    border-top-right-radius: 10px;
                    border-bottom-right-radius: 10px;
                }

        /* â”€â”€ Imagen â”€â”€ */
        .cart-img {
            border-radius: 10px;
            border: 1px solid #e0e0e0;
            transition: transform 0.2s ease;
        }

            .cart-img:hover {
                transform: scale(1.1);
            }

        /* â”€â”€ BotÃ³n Remove â”€â”€ */
        .btn-delete-item {
            background-color: #FFF9E6;
            color: #C08000;
            border: none;
            padding: 8px 16px;
            border-radius: 20px;
            font-weight: 600;
            transition: all 0.3s ease;
        }

            .btn-delete-item:hover {
                background-color: #FFC800;
                color: #000000;
            }

        /* â”€â”€ Total y Checkout â”€â”€ */
        .total-section {
            background-color: #f8f9fa;
            border-radius: 15px;
            padding: 25px;
            border: 1px solid #e9ecef;
        }

        .btn-checkout-custom {
            background: linear-gradient(135deg, #FFC800 0%, #D4A000 100%);
            color: #000000 !important;
            border: none;
            padding: 14px 30px;
            border-radius: 30px;
            font-weight: 700;
            font-size: 1.05rem;
            letter-spacing: 0.5px;
            transition: all 0.3s ease;
            box-shadow: 0 5px 15px rgba(255, 200, 0, 0.2);
            width: 100%;
        }

            .btn-checkout-custom:hover {
                background: linear-gradient(135deg, #FFE066 0%, #FFC800 100%);
                transform: translateY(-2px);
                box-shadow: 0 8px 20px rgba(255, 200, 0, 0.35);
                color: #000000 !important;
            }

            .btn-checkout-custom:disabled {
                background: #cccccc !important;
                box-shadow: none;
                cursor: not-allowed;
            }

        .btn-continue-shopping {
            border: 2px solid #cccccc;
            color: #555555 !important;
            padding: 12px 30px;
            border-radius: 30px;
            font-weight: 600;
            transition: all 0.3s ease;
            text-decoration: none;
            display: inline-block;
        }

            .btn-continue-shopping:hover {
                background-color: #eef2f3;
                color: #111111 !important;
                border-color: #999999;
                transform: translateY(-2px);
            }

        /* â”€â”€ Scrollbar â”€â”€ */
        ::-webkit-scrollbar {
            width: 10px;
        }

        ::-webkit-scrollbar-track {
            background: #1a1a1a;
        }

        ::-webkit-scrollbar-thumb {
            background: #333333;
            border-radius: 10px;
        }

            ::-webkit-scrollbar-thumb:hover {
                background: #FFC800;
            }

        /* â”€â”€ Color subtotal/total â”€â”€ */
        .text-danger {
            color: #D47A00 !important;
        }

        /* â”€â”€ Control de cantidad â”€â”€ */
        .table-custom td:has(.qty-wrapper) {
            padding: 0 !important;
        }

        .qty-wrapper {
            display: inline-flex !important;
            flex-direction: row !important;
            align-items: center !important;
            justify-content: center !important;
            gap: 10px;
            margin: 0 auto;
        }

        .btn-qty {
            background-color: #FFC800;
            color: #000000;
            border: none;
            border-radius: 50%;
            width: 32px;
            height: 32px;
            font-size: 1.1rem;
            font-weight: 700;
            cursor: pointer;
            padding: 0;
            transition: all 0.2s ease;
            display: inline-flex !important;
            align-items: center !important;
            justify-content: center !important;
            flex-shrink: 0;
        }

            .btn-qty:hover {
                background-color: #D4A000;
                transform: scale(1.1);
            }

            .btn-qty:disabled,
            .btn-qty[disabled] {
                background-color: #e0e0e0 !important;
                color: #aaaaaa !important;
                cursor: not-allowed !important;
                transform: none !important;
                box-shadow: none !important;
            }

        .qty-number {
            font-weight: 700;
            font-size: 1rem;
            min-width: 24px;
            text-align: center !important;
            display: inline-block;
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

        .btn-dark-grey {
            color: #fff;
            background-color: #4a4a4a;
            border-color: #4a4a4a;
            transition: all 0.2s ease-in-out;
        }

            .btn-dark-grey:hover {
                color: #fff;
                background-color: #333333;
                border-color: #333333;
                transform: translateX(-2px); /* Un sutil efecto que mueve el botÃ³n hacia la izquierda en hover */
                ); /* Un sutil efecto que mueve el botÃ³n hacia la izquierda en hover */
            }
    </style>

    <script type="text/javascript">
        function ScriptParaAbrirMenu() {
            const menu = document.getElementById('userDropdownMenuUser');
            if (menu) {
                menu.style.display = 'block';
            }
        }

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
        <asp:ScriptManager ID="ScriptManager1" runat="server" />

        <!-- Navbar -->
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

        <div class="container">
            <div class="cart-container">
                <h2 class="cart-title"><%= Resources.Strings.Cart_MainTitle %></h2>

                <asp:UpdatePanel ID="UpdatePanel1" runat="server" UpdateMode="Conditional">
                    <ContentTemplate>

                        <div class="table-responsive">
                            <asp:GridView ID="gvCart" runat="server"
                                AutoGenerateColumns="False"
                                CssClass="table table-custom align-middle"
                                OnRowDeleting="gvCart_RowDeleting"
                                OnRowCommand="gvCart_RowCommand"
                                DataKeyNames="ID,Size"
                                GridLines="None">
                                <Columns>

                                    <%-- Imagen --%>
                                    <asp:TemplateField HeaderText="<%$ Resources:Strings, Cart_HeaderImage %>"
                                        ItemStyle-HorizontalAlign="Center"
                                        HeaderStyle-HorizontalAlign="Center">
                                        <ItemTemplate>
                                            <img src='<%# "images/camisetas/" + Eval("ImageURL") %>'
                                                class="cart-img"
                                                style="width: 70px; height: 70px; object-fit: cover;"
                                                onerror="this.src='assets/img/default-product.jpg';" />
                                        </ItemTemplate>
                                    </asp:TemplateField>

                                    <%-- Nombre --%>
                                    <asp:TemplateField HeaderText="<%$ Resources:Strings, Cart_HeaderTShirt %>"
                                        ItemStyle-HorizontalAlign="Center"
                                        HeaderStyle-HorizontalAlign="Center">
                                        <ItemTemplate>
                                            <%# FormatJerseyName(Eval("Name")) %>
                                        </ItemTemplate>
                                    </asp:TemplateField>

                                    <%-- Talla --%>
                                    <asp:BoundField DataField="Size" HeaderText="<%$ Resources:Strings, Cart_HeaderSize %>"
                                        ItemStyle-Font-Bold="true"
                                        ItemStyle-HorizontalAlign="Center"
                                        HeaderStyle-HorizontalAlign="Center" />

                                    <%-- Precio unitario --%>
                                    <asp:BoundField DataField="Price" HeaderText="<%$ Resources:Strings, Cart_HeaderPrice %>"
                                        DataFormatString="${0:F2}"
                                        ItemStyle-HorizontalAlign="Center"
                                        HeaderStyle-HorizontalAlign="Center" />

                                    <%-- Control de cantidad --%>
                                    <asp:TemplateField HeaderText="<%$ Resources:Strings, Cart_HeaderQuantity %>" ItemStyle-HorizontalAlign="Center" HeaderStyle-HorizontalAlign="Center">
                                        <ItemTemplate>
                                            <div class="qty-wrapper">

                                                <%-- BotÃ³n MENOS: gris si Quantity = 1 --%>
                                                <asp:Button runat="server" Text="-"
                                                    CommandName="decrease"
                                                    CommandArgument='<%# Container.DataItemIndex %>'
                                                    CssClass="btn-qty"
                                                    Enabled='<%# Convert.ToInt32(Eval("Quantity")) > 1 %>' />

                                                <%-- NÃºmero actual --%>
                                                <span class="qty-number"><%# Eval("Quantity") %></span>

                                                <%-- BotÃ³n MAS: gris si Quantity = Stock --%>
                                                <asp:Button runat="server" Text="+"
                                                    CommandName="increase"
                                                    CommandArgument='<%# Container.DataItemIndex %>'
                                                    CssClass="btn-qty"
                                                    Enabled='<%# Convert.ToInt32(Eval("Quantity")) < Convert.ToInt32(Eval("Stock")) %>' />

                                            </div>
                                        </ItemTemplate>
                                    </asp:TemplateField>

                                    <%-- Subtotal --%>
                                    <asp:BoundField DataField="Subtotal" HeaderText="<%$ Resources:Strings, Cart_HeaderSubtotal %>"
                                        DataFormatString="${0:F2}"
                                        ItemStyle-Font-Bold="true"
                                        ItemStyle-CssClass="text-danger"
                                        ItemStyle-HorizontalAlign="Center"
                                        HeaderStyle-HorizontalAlign="Center" />

                                    <%-- BotÃ³n Remove --%>
                                    <asp:CommandField ShowDeleteButton="True" DeleteText="<%$ Resources:Strings, Cart_BtnRemove %>"
                                        ButtonType="Button"
                                        ControlStyle-CssClass="btn-delete-item"
                                        ItemStyle-HorizontalAlign="Center"
                                        HeaderStyle-HorizontalAlign="Center" />

                                </Columns>
                            </asp:GridView>
                        </div>

                        <div class="row mt-5 align-items-center">
                            <div class="col-md-6 mb-3 mb-md-0">
                                <a href="Homepage.aspx" class="btn-continue-shopping">
                                    <i class="fas fa-arrow-left me-2"></i><%= Resources.Strings.Cart_BtnContinue %>
                                </a>
                            </div>
                            <div class="col-md-6 col-lg-4 ms-auto">
                                <div class="total-section text-end">
                                    <div class="d-flex justify-content-between align-items-center mb-3">
                                        <span class="text-secondary fw-semibold"><%= Resources.Strings.Cart_TotalToPay %></span>
                                        <asp:Label ID="lblTotal" runat="server" Text="$0.00" CssClass="fs-4 text-danger fw-bold"></asp:Label>
                                    </div>
                                    <asp:Button ID="btnCheckout" runat="server" Text="<%$ Resources:Strings, Cart_BtnCheckout %>"
                                        CssClass="btn-checkout-custom" OnClick="btnCheckout_Click" />
                                </div>
                            </div>
                        </div>

                    </ContentTemplate>
                </asp:UpdatePanel>

            </div>
        </div>
    </form>

    <script src="https://ajax.googleapis.com/ajax/libs/jquery/3.6.0/jquery.min.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.2.3/dist/js/bootstrap.bundle.min.js"></script>
</body>
</html>

