<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="DetailsShirt.aspx.cs" Inherits="OFFSIDESHOP.DetailsShirt" %>
<%@ Register Src="~/ChatbotControl.ascx" TagPrefix="uc" TagName="Chatbot" %>
<%@ Register Src="~/FooterControl.ascx" TagPrefix="uc" TagName="Footer" %>
<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1, shrink-to-fit=no" />
    <title>OffsideShop - Shirt Details</title>
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.2.3/dist/css/bootstrap.min.css" rel="stylesheet" />
    <link rel="icon" type="image/x-icon" href="assets/favicon.ico" />

    <script src="https://use.fontawesome.com/releases/v6.3.0/js/all.js" crossorigin="anonymous"></script>

    <link href="https://fonts.googleapis.com/css?family=Montserrat:400,700" rel="stylesheet" type="text/css" />
    <link href="https://fonts.googleapis.com/css?family=Roboto+Slab:400,100,300,700" rel="stylesheet" type="text/css" />

    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>

    <link href="css/styles.css" rel="stylesheet" />
    <link href="css/details.css" rel="stylesheet" />
    <link href="EstilosCss/EstiloInicio.css" rel="stylesheet" />
    <link type="text/css" rel="stylesheet" href="css/slick.css" />
    <link type="text/css" rel="stylesheet" href="css/slick-theme.css" />

    <script type="text/javascript">
        window.onpageshow = function (event) {
            if (event.persisted) {
                window.location.reload();
            }
        };
    </script>

   <style>
        * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }

        body {
            font-family: 'Montserrat', sans-serif;
            background: #ffffff;
            min-height: 100vh;
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
            filter: brightness(1.1);
        }

        .btn-outline-warning {
            color: #000000;
            border: 2px solid #FFC800;
            background: linear-gradient(135deg, #FFC800 0%, #D4A000 100%);
            border-radius: 25px;
            padding: 8px 24px;
            font-weight: 600;
            transition: all 0.3s ease;
        }

            .btn-outline-warning:hover {
                background: linear-gradient(135deg, #FFE066 0%, #FFC800 100%);
                border-color: #FFE066;
                transform: translateY(-2px);
                box-shadow: 0 5px 15px rgba(255, 200, 0, 0.3);
                color: #000000;
            }

        .search-header-section {
            background: linear-gradient(180deg, #000000 0%, #121212 100%);
            padding-top: 110px;
            padding-bottom: 30px;
            border-bottom: 3px solid #FFC800;
            box-shadow: 0 8px 30px rgba(0, 0, 0, 0.5);
            transition: all 0.3s ease;
        }

        .search-input-group {
            border-radius: 25px;
            overflow: hidden;
            border: 1px solid #333333;
            background-color: #1e1e1e;
            transition: all 0.3s ease;
        }

            .search-input-group:focus-within {
                border-color: #FFC800;
                box-shadow: 0 0 12px rgba(255, 200, 0, 0.35);
            }

            .search-input-group .form-control {
                border: none;
                background: transparent;
                height: 46px;
                font-size: 0.95rem;
                color: #ffffff !important;
            }

                .search-input-group .form-control:focus {
                    box-shadow: none;
                    background: transparent;
                }

            .search-input-group .input-group-text {
                border: none;
                font-size: 0.95rem;
                color: #FFC800;
                padding-left: 15px;
            }

        .filter-select {
            height: 46px;
            border-radius: 25px;
            border: 1px solid #333333;
            background-color: #1e1e1e;
            color: #ffffff !important;
            padding-left: 15px;
            cursor: pointer;
            transition: all 0.3s ease;
            appearance: none;
            -webkit-appearance: none;
            background-image: url("data:image/svg+xml,%3csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 16 16'%3e%3cpath fill='none' stroke='%23FFC800' stroke-linecap='round' stroke-linejoin='round' stroke-width='2' d='m2 5 6 6 6-6'/%3e%3c/svg%3e");
            background-repeat: no-repeat;
            background-position: right 15px center;
            background-size: 12px;
            padding-right: 40px;
        }

            .filter-select:focus {
                border-color: #FFC800;
                box-shadow: 0 0 12px rgba(255, 200, 0, 0.35);
                background-color: #1e1e1e;
            }

        .btn-danger-custom {
            height: 46px;
            border-radius: 25px;
            background: linear-gradient(135deg, #FFC800 0%, #D4A000 100%);
            border: none;
            color: #000000;
            font-weight: 600;
            transition: all 0.3s ease;
            box-shadow: 0 4px 10px rgba(255, 200, 0, 0.2);
        }

            .btn-danger-custom:hover {
                background: linear-gradient(135deg, #FFE066 0%, #FFC800 100%);
                transform: translateY(-2px);
                box-shadow: 0 6px 15px rgba(255, 200, 0, 0.4);
                color: #000000;
            }

        .btn-outline-light-custom {
            height: 46px;
            width: 46px;
            display: flex;
            align-items: center;
            justify-content: center;
            border-radius: 50%;
            border: 1px solid #444444;
            background-color: transparent;
            color: #ffffff;
            transition: all 0.3s ease;
        }

            .btn-outline-light-custom:hover {
                background-color: #ffffff;
                color: #000000 !important;
                border-color: #ffffff;
                transform: rotate(-180deg);
            }

        .product-details-container {
            margin-top: 50px;
            margin-bottom: 80px;
        }

        .products-slick {
            position: relative;
            display: block;
            width: 100%;
        }

        .text-danger {
            color: #D47A00 !important;
        }

        .btn-outline-danger {
            color: #D47A00 !important;
            border-color: #FFC800 !important;
            border-radius: 20px;
            font-weight: 600;
            transition: all 0.3s ease;
        }

        form#form1 {
            display: flex;
            flex-direction: column;
            min-height: 100vh;
        }

        .btn-outline-danger:hover {
            color: #000 !important;
            background-color: #FFC800 !important;
            border-color: #FFC800 !important;
        }

        .products-slick .product-card {
            margin: 15px 10px;
            box-shadow: 0 0 15px rgba(0,0,0,0.1);
            border: 1px solid #e0e0e0;
        }

            .products-slick .product-card:hover {
                transform: translateY(-5px);
                box-shadow: 0 10px 20px rgba(255,200,0,0.15);
                border-color: #FFC800 !important;
            }

        .offside-prev, .offside-next {
            position: absolute !important;
            top: 50% !important;
            transform: translateY(-50%) !important;
            z-index: 9999 !important;
            width: 45px !important;
            height: 45px !important;
            background-color: #111 !important;
            border: 2px solid #FFC800 !important;
            border-radius: 50% !important;
            color: #FFC800 !important;
            font-size: 20px !important;
            cursor: pointer !important;
            display: flex !important;
            align-items: center !important;
            justify-content: center !important;
            transition: all 0.3s ease !important;
        }

            .offside-prev:hover, .offside-next:hover {
                background-color: #FFC800 !important;
                color: #111 !important;
                transform: translateY(-50%) scale(1.1) !important;
            }

        .offside-prev {
            left: 10px !important;
        }

        .offside-next {
            right: 10px !important;
        }

            .offside-prev.slick-disabled, .offside-next.slick-disabled {
                opacity: 0.25 !important;
                cursor: not-allowed !important;
            }

        #similar-slick {
            padding: 0 50px;
        }

        @media (max-width: 767px) {
            #wc-slick, #laliga-slick, #premier-slick, #seriea-slick, #similar-slick {
                padding: 0 45px !important;
            }

            .offside-prev, .offside-next {
                width: 36px !important;
                height: 36px !important;
                font-size: 16px !important;
            }

            .offside-prev {
                left: 5px !important;
            }

            .offside-next {
                right: 5px !important;
            }
        }

        .quantity-selector {
            display: flex;
            align-items: center;
            gap: 5px;
        }

        .btn-qty {
            width: 40px;
            height: 40px;
            background-color: #333;
            color: #fff;
            border: none;
            border-radius: 5px;
            cursor: pointer;
            font-weight: bold;
        }

        .input-qty {
            width: 50px;
            height: 40px;
            text-align: center;
            border: 1px solid #333;
            border-radius: 5px;
        }

        .size-label {
            padding: 10px 15px;
            border: 1px solid #333;
            border-radius: 5px;
            transition: 0.3s;
        }

        .size-radio:checked + .size-label {
            background-color: #333;
            color: #fff;
        }

        .btn-size-option {
            display: inline-flex;
            align-items: center;
            justify-content: center;
            border: 2px solid #1c1c1c;
            background-color: #ffffff;
            color: #1c1c1c !important;
            font-weight: bold;
            min-width: 45px;
            height: 45px;
            border-radius: 8px;
            text-decoration: none !important;
            transition: all 0.2s ease-in-out;
        }

            .btn-size-option:hover {
                border-color: #FFC800;
                color: #FFC800 !important;
            }

            .btn-size-option.active {
                background-color: #FFC800 !important;
                border-color: #FFC800 !important;
                color: #1c1c1c !important;
                box-shadow: 0px 4px 8px rgba(255, 200, 0, 0.3);
            }

        .custom-qty-group { width: max-content;
            border: 2px solid #1c1c1c;
            border-radius: 8px;
            overflow: hidden;
            background-color: #ffffff;
            height: 45px;
        }

        .btn-qty-control {
            background-color: #f8f9fa;
            color: #1c1c1c;
            border: none;
            font-weight: bold;
            font-size: 1.2rem;
            width: 40px;
            height: 100%;
            cursor: pointer;
            transition: background-color 0.2s;
        }

            .btn-qty-control:hover {
                background-color: #FFC800;
                color: #1c1c1c;
            }

        .input-qty-num {
            border: none !important;
            width: 50px;
            height: 100%;
            font-weight: bold;
            font-size: 1.1rem;
            color: #1c1c1c;
            background-color: #ffffff !important;
        }

        .carousel-inner {
            background: transparent !important;
        }

        .carousel-control-prev, .carousel-control-next {
            background: none !important;
            background-image: none !important;
            border: none !important;
            opacity: 0.5;
            width: 10%;
        }

            .carousel-control-prev:hover, .carousel-control-next:hover {
                opacity: 1;
                background: none !important;
                background-image: none !important;
            }

        .carousel, .carousel-item, .img-container {
            background-image: none !important;
            background: transparent !important;
            box-shadow: none !important;
        }

        .carousel-control-prev-icon {
            background-image: url("data:image/svg+xml,%3csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 16 16' fill='%23000'%3e%3cpath d='M11.354 1.646a.5.5 0 0 1 0 .708L5.707 8l5.647 5.646a.5.5 0 0 1-.708.708l-6-6a.5.5 0 0 1 0-.708l6-6a.5.5 0 0 1 .708 0z'/%3e%3c/svg%3e") !important;
        }

        .carousel-control-next-icon {
            background-image: url("data:image/svg+xml,%3csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 16 16' fill='%23000'%3e%3cpath d='M4.646 1.646a.5.5 0 0 1 .708 0l6 6a.5.5 0 0 1 0 .708l-6 6a.5.5 0 0 1-.708-.708L10.293 8 4.646 2.354a.5.5 0 0 1 0-.708z'/%3e%3c/svg%3e") !important;
        }

        .carousel-indicators [data-bs-target] {
            background-color: #000 !important;
        }

        .btn-size-option.out-of-stock {
            position: relative;
            color: #a0aec0;
            border-color: #e2e8f0;
            background-color: #f8fafc;
            cursor: not-allowed;
            overflow: hidden;
            opacity: 0.6;
        }

            .btn-size-option.out-of-stock::after {
                content: "";
                position: absolute;
                top: 0;
                left: 0;
                width: 100%;
                height: 100%;
                background: linear-gradient(to top right, transparent calc(50% - 1px), #cbd5e1 calc(50% - 1px), #cbd5e1 calc(50% + 1px), transparent calc(50% + 1px));
            }

        .tab-custom-style {
            transition: all 0.3s ease;
        }

            .tab-custom-style.active {
                color: #FFC800 !important;
                border-bottom: 4px solid #FFC800 !important;
            }

            .tab-custom-style:hover {
                color: #danger !important;
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
                transform: translateX(-2px);
            }

        .personalization-container {
            font-family: 'Montserrat', sans-serif;
        }

        .custom-slider {
            width: 2.8em;
            height: 1.5em;
            background-color: #e9ecef;
            border: 1px solid #ced4da;
            border-radius: 50rem;
            position: relative;
            transition: all 0.2s ease-in-out;
        }

            .custom-slider::after {
                content: "";
                width: 1.15rem;
                height: 1.15rem;
                background-color: #fff;
                border-radius: 50%;
                position: absolute;
                top: 2px;
                left: 2px;
                transition: transform 0.2s ease-in-out;
                box-shadow: 0 1px 3px rgba(0,0,0,0.15);
            }

        .slider-active {
            background-color: #FFC800 !important;
            border-color: #FFC800 !important;
        }

            .slider-active::after {
                transform: translateX(1.25rem);
            }

        .cursor-pointer {
            cursor: pointer;
        }

        #personalizationFields input:focus {
            border-color: #FFC800 !important;
            box-shadow: 0 0 0 0.25rem rgba(255, 200, 0, 0.25) !important;
        }

        #previewPersonalizacion {
            display: none;
            align-items: center;
            justify-content: center;
            padding: 15px;
            background: linear-gradient(135deg, #f5f5f5 0%, #e8e8e8 100%);
            border-radius: 12px;
            margin-top: 20px;
            height: 540px;
            width: 100%;
        }

        #shirtPreview {
            position: relative;
            width: 100%;
            height: 100%;
            max-width: 460px;
            max-height: 500px;
            margin: 0 auto;
            filter: drop-shadow(0 8px 16px rgba(0,0,0,0.15));
            overflow: hidden;
        }

        #imgCamiseta {
            position: absolute;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            object-fit: contain;
            z-index: 1;
        }

        #previewNombre {
            position: absolute;
            top: 22%;
            left: 50%;
            transform: translateX(-50%);
            z-index: 2;
            font-size: 28px;
            font-weight: 900;
            letter-spacing: 2px;
            text-transform: uppercase;
            color: #1c1c1c;
            font-family: 'Arial Black', sans-serif;
            text-align: center;
            white-space: nowrap;
            width: 85%;
            transition: font-size 0.1s ease;
        }

        #previewNumero {
            position: absolute;
            top: 35%;
            left: 50%;
            transform: translateX(-50%);
            z-index: 2;
            font-size: 120px;
            font-weight: 900;
            line-height: 1;
            color: #1c1c1c;
            font-family: 'Arial Black', sans-serif;
            letter-spacing: -1px;
            text-align: center;
            width: 85%;
        }

        .navbar-icons-container {
            display: flex;
            align-items: center;
            gap: 20px;
            margin-left: auto;
        }

        .cart-icon-btn {
            background: none;
            border: none;
            color: #fff;
            font-size: 20px;
            cursor: pointer;
            display: flex;
            align-items: center;
            gap: 6px;
        }

            .cart-icon-btn .badge {
                position: static; /* anula el position:absolute anterior */
                margin-left: 0; /* anula el margin-left:auto del .badge genÃ©rico */
                background-color: #FFC800;
                color: #000;
                border-radius: 50%;
                width: 20px;
                height: 20px;
                padding: 0;
                display: flex;
                align-items: center;
                justify-content: center;
                font-size: 12px;
                font-weight: bold;
            }
    </style>

</head>
<body id="page-top">
    <form runat="server">
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
                                        <i class="fas fa-sign-in-alt"></i><%= Resources.Strings.Nav_Login %>
                                    </a>
                                    <a href="SignUp.aspx" class="dropdown-item">
                                        <i class="fas fa-user-plus"></i><%= Resources.Strings.Nav_SignUp %>
                                    </a>
                                    <asp:Button ID="Button1" runat="server" CssClass="dropdown-item btn-logout" Text="<%$ Resources:Strings, Nav_BackToShop %>" OnClick="btnbackshop_Click" />
                                </div>
                            </div>
                        </div>

                    </asp:PlaceHolder>

                    <asp:PlaceHolder ID="phNavbarUser" runat="server">
                        <div class="navbar-icons-container">

                            <!-- Ãcono del carrito -->
                            <asp:LinkButton ID="btnNavCart" runat="server" CssClass="cart-icon-btn" OnClick="btnNavCart_Click">
                                <i class="fas fa-shopping-cart"></i>
                                <span class="badge">
                                    <asp:Label ID="lblCartCount" runat="server" Text="0"></asp:Label>
                                </span>
                            </asp:LinkButton>
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
                                                  <asp:LinkButton ID="btnGoToAccount" runat="server" CssClass="dropdown-item" OnClick="btnGoToAccount_Click">
      <i class="fas fa-user-cog"></i> <%= Resources.Strings.Nav_MyAccount %>
  </asp:LinkButton>
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
                                    <a href="Dashboard.aspx" class="dropdown-item">
                                        <i class="fas fa-chart-line"></i><%= Resources.Strings.Nav_Dashboard %>
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

        <div class="search-header-section">
            <div class="container">

                <div class="d-block d-lg-none mb-3">
                    <button type="button" class="btn btn-danger-custom w-100 fw-bold py-2 shadow-sm"
                        data-bs-toggle="collapse"
                        data-bs-target="#searchFiltersCollapse"
                        aria-expanded="false"
                        aria-controls="searchFiltersCollapse">
                        <i class="fas fa-search"></i><%= Resources.Strings.Search_MobileBtn %>
                    </button>
                </div>

                <div class="collapse d-lg-block" id="searchFiltersCollapse">
                    <div class="row g-3 align-items-end justify-content-center">

                        <div class="col-lg-4 col-md-12">
                            <label class="form-label text-light fw-bold small mb-2"><%= Resources.Strings.Search_Label %></label>
                            <div class="input-group search-input-group">
                                <span class="input-group-text bg-transparent border-0 text-muted">
                                    <i class="fas fa-search"></i>
                                </span>
                                <asp:TextBox ID="txtSearch" runat="server" CssClass="form-control bg-transparent border-0 text-light search-input" placeholder="<%$ Resources:Strings, Search_Placeholder %>"></asp:TextBox>
                            </div>
                        </div>

                        <div class="col-lg-2 col-md-4">
                            <label class="form-label text-light fw-bold small mb-2"><%= Resources.Strings.Search_LeagueLabel %></label>
                            <asp:DropDownList ID="ddlLeague" runat="server" CssClass="form-select filter-select">
                            </asp:DropDownList>
                        </div>

                        <div class="col-lg-2 col-md-4">
                            <label class="form-label text-light fw-bold small mb-2"><%= Resources.Strings.Search_BrandLabel %></label>
                            <asp:DropDownList ID="ddlBrand" runat="server" CssClass="form-select filter-select">
                            </asp:DropDownList>
                        </div>

                        <div class="col-lg-2 col-md-4">
                            <label class="form-label text-light fw-bold small mb-2"><%= Resources.Strings.Search_KitLabel %></label>
                            <asp:DropDownList ID="ddlKitType" runat="server" CssClass="form-select filter-select">
                                <asp:ListItem Value="" Text="<%$ Resources:Strings, Search_KitAny %>"></asp:ListItem>
                                <asp:ListItem Value="1" Text="<%$ Resources:Strings, Search_KitLocal %>"></asp:ListItem>
                                <asp:ListItem Value="2" Text="<%$ Resources:Strings, Search_KitAway %>"></asp:ListItem>
                                <asp:ListItem Value="3" Text="<%$ Resources:Strings, Search_KitThird %>"></asp:ListItem>
                                <asp:ListItem Value="4" Text="<%$ Resources:Strings, Search_KitRetro %>"></asp:ListItem>
                                <asp:ListItem Value="5" Text="<%$ Resources:Strings, Search_KitTraining %>"></asp:ListItem>
                                <asp:ListItem Value="6" Text="<%$ Resources:Strings, Search_KitSpecial %>"></asp:ListItem>
                            </asp:DropDownList>
                        </div>

                        <div class="col-lg-2 col-md-12 d-flex gap-2">
                            <asp:Button ID="btnSearch" runat="server" Text="<%$ Resources:Strings, Search_Btn %>" CssClass="btn btn-danger-custom w-100 fw-bold" OnClick="btnSearch_Click" />
                            <asp:LinkButton ID="btnReset" runat="server" CssClass="btn btn-outline-light-custom" OnClick="btnReset_Click" ToolTip="Clear Filters">
                        <i class="fas fa-undo"></i>
                            </asp:LinkButton>
                        </div>

                    </div>
                </div>

            </div>
        </div>

        <div class="container product-details-container">
            <div class="row g-5">
                <div class="col-lg-6">
                    <div class="img-container">
                        <div id="jerseyCarousel" class="carousel slide" data-bs-ride="carousel">
                            <div class="carousel-indicators">
                                <asp:Literal ID="litCarouselIndicators" runat="server"></asp:Literal>
                            </div>

                            <div class="carousel-inner">
                                <asp:Literal ID="litCarouselItems" runat="server"></asp:Literal>
                            </div>

                            <asp:PlaceHolder ID="phCarouselControls" runat="server">
                                <button class="carousel-control-prev" type="button" data-bs-target="#jerseyCarousel" data-bs-slide="prev">
                                    <span class="carousel-control-prev-icon" aria-hidden="true"></span>
                                    <span class="visually-hidden">Previous</span>
                                </button>
                                <button class="carousel-control-next" type="button" data-bs-target="#jerseyCarousel" data-bs-slide="next">
                                    <span class="carousel-control-next-icon" aria-hidden="true"></span>
                                    <span class="visually-hidden">Next</span>
                                </button>
                            </asp:PlaceHolder>
                        </div>
                        <div id="previewPersonalizacion" style="display: none;">
                            <div id="shirtPreview">
                                <asp:Image ID="imgCamiseta" runat="server" ClientIDMode="Static"
                                    ImageUrl="~/camisapreview/camiseta.png"
                                    alt="Preview Camiseta" />

                                <div id="previewNombre">[NOMBRE]</div>
                                <div id="previewNumero">0</div>
                            </div>
                        </div>
                    </div>
                </div>

                <div class="col-lg-6 d-flex flex-column justify-content-center">
                    <asp:UpdatePanel ID="upProductDetails" runat="server" UpdateMode="Conditional">
                        <ContentTemplate>
                            <div>
                                <span class="badge-custom">
                                    <asp:Label ID="lblBrand" runat="server" Text="BRAND"></asp:Label>
                                    |  
                                    <asp:Label ID="lblType" runat="server" Text="TYPE"></asp:Label>
                                </span>

                                <h1 class="product-title">
                                    <asp:Label ID="lblShirtName" runat="server" Text="Jersey Name"></asp:Label>
                                </h1>

                                <div class="product-meta">
                                    <i class="fas fa-tshirt text-danger me-2"></i>
                                    <strong><%= Resources.Strings.Detail_Team %></strong>
                                    <asp:Label ID="lblTeam" runat="server" Text="Team Name"></asp:Label>
                                    <span class="mx-2">|</span>
                                    <i class="far fa-calendar-alt text-danger me-2"></i>
                                    <strong><%= Resources.Strings.Detail_Year %></strong>
                                    <asp:Label ID="lblYear" runat="server" Text="2024"></asp:Label>
                                </div>

                                <div class="product-price">
                                    $
                                    <asp:Label ID="lblPrice" runat="server" Text="0.00"></asp:Label>
                                </div>

                                <p class="product-description">
                                    <asp:Label ID="lblDescription" runat="server" Text=""></asp:Label>
                                </p>

                                <asp:HiddenField ID="hfQuantity" runat="server" Value="1" />
                                <asp:HiddenField ID="hfMaxStock" runat="server" Value="0" />

                                <div class="d-flex align-items-center flex-wrap gap-5 my-4">

                                    <!-- ============================================== -->
                                    <!-- SECCIÃ“N ACTUALIZADA DE TALLAS CON BOTÃ“N MODAL -->
                                    <div class="size-selector-container">
                                        <div class="d-flex justify-content-between align-items-center mb-2">
                                            <span class="fw-bold text-dark"><%= Resources.Strings.Detail_SelectSize %></span>
                                            <button type="button" class="btn btn-sm btn-link text-warning fw-bold text-decoration-none p-0" data-bs-toggle="modal" data-bs-target="#sizeGuideModal">
                                                <i class="fas fa-ruler-combined me-1"></i><%= Resources.Strings.Detail_SizeGuide %>
                                            </button>
                                        </div>
                                        <div class="d-flex gap-2">
                                            <asp:Repeater ID="rptSizes" runat="server" OnItemCommand="rptSizes_ItemCommand">
                                                <ItemTemplate>
                                                    <asp:LinkButton ID="btnSizeOption" runat="server"
                                                        CommandName="SelectSize"
                                                        CommandArgument='<%# Eval("Id_Size") %>'
                                                        CssClass='<%# GetSizeClass(Eval("Id_Size"), Eval("Stock")) %>'
                                                        Enabled='<%# Convert.ToInt32(Eval("Stock")) > 0 %>'>
                            <%# Eval("SizeName") %>
                                                    </asp:LinkButton>
                                                </ItemTemplate>
                                            </asp:Repeater>
                                        </div>
                                    </div>
                                    <!-- ============================================== -->

                                    <div class="quantity-selector-container">
                                        <span class="fw-bold d-block mb-2 text-dark"><%= Resources.Strings.Detail_SelectQuantity %></span>
                                        <div class="custom-qty-group d-flex align-items-center">
                                            <button type="button" class="btn btn-qty-control" onclick="changeQuantity(-1)">-</button>
                                            <input type="text" id="txtDisplayQty" class="form-control text-center input-qty-num" value="1" readonly />
                                            <button type="button" class="btn btn-qty-control" onclick="changeQuantity(1)">+</button>
                                        </div>
                                    </div>

                                </div>


                                <asp:PlaceHolder ID="phPersonalizacion" runat="server">
                                    <div class="personalization-container my-4">
                                        <label class="custom-control custom-switch d-flex align-items-center justify-content-between p-3 rounded shadow-sm border cursor-pointer" style="background-color: #fcfcfc; border-color: #e0e0e0; display: flex !important;">
                                            <div>
                                                <span class="fw-bold d-block text-dark" style="font-size: 0.95rem;"><%= Resources.Strings.Detail_CustomPrompt %></span>
                                                <small class="text-muted d-block" style="font-size: 0.8rem;"><%= Resources.Strings.Detail_CustomPriceInfo %> <strong class="text-warning">+$15.00</strong></small>
                                            </div>
                                            <div class="form-switch m-0 position-relative">
                                                <asp:CheckBox ID="chkCustomize" runat="server" ClientIDMode="Static" OnChange="togglePersonalizacion()" Style="display: none;" />
                                                <div class="custom-slider" id="sliderVisual"></div>
                                            </div>
                                        </label>

                                        <div id="personalizationFields" class="mt-3 p-3 rounded border border-warning" style="display: none; background-color: #fffbeb; border-style: dashed !important; transition: all 0.3s ease;">
                                            <div class="row g-3">
                                                <div class="col-8">
                                                    <label class="form-label text-dark fw-bold mb-1" style="font-size: 0.85rem;"><%= Resources.Strings.Detail_CustomName %></label>
                                                    <asp:TextBox ID="txtCustomName" runat="server" ClientIDMode="Static" MaxLength="12" CssClass="form-control text-uppercase" Style="border-radius: 6px; border: 1.5px solid #ced4da;" />
                                                </div>
                                                <div class="col-4">
                                                    <label class="form-label text-dark fw-bold mb-1" style="font-size: 0.85rem;"><%= Resources.Strings.Detail_CustomNumber %></label>
                                                    <asp:TextBox ID="txtCustomNumber" runat="server" ClientIDMode="Static"
                                                        MaxLength="2"
                                                        inputmode="numeric"
                                                        CssClass="form-control" Style="border-radius: 6px; border: 1.5px solid #ced4da;" />
                                                </div>
                                            </div>
                                            <div class="mt-2 text-muted" style="font-size: 0.75rem;">
                                                <i class="fas fa-info-circle me-1"></i><%= Resources.Strings.Detail_CustomWarning %>
                                            </div>
                                        </div>
                                    </div>
                                </asp:PlaceHolder>

                            </div>

                            <div class="row g-3">
                                <div class="col-sm-6">
                                    <asp:Button ID="btnAddCart" runat="server" CssClass="btn btn-addcart w-100" Text="<%$ Resources:Strings, Detail_AddToCart %>" OnClick="btnAddCart_Click" />
                                </div>
                                <div class="col-sm-6">
                                    <a href="Homepage.aspx" class="btn btn-back w-100">
                                        <i class="fas fa-arrow-left me-2"></i><%= Resources.Strings.Nav_BackToShop %>
                                    </a>
                                </div>
                            </div>
                        </ContentTemplate>
                    </asp:UpdatePanel>
                </div>
            </div>
        </div>


        <div class="container my-5 text-dark">
            <ul class="nav nav-tabs mb-4 border-bottom border-warning border-3" id="productDetailTabs" role="tablist">
                <li class="nav-item" role="presentation">
                    <button class="nav-link active text-uppercase fw-bold text-dark border-0 bg-transparent position-relative py-3 px-4 tab-custom-style"
                        id="similar-tab" data-bs-toggle="tab" data-bs-target="#similar-panel" type="button" role="tab"
                        aria-controls="similar-panel" aria-selected="true" style="letter-spacing: 1px; font-size: 1.1rem;">
                        <%= Resources.Strings.Tab_Similar %>
                    </button>
                </li>
                <li class="nav-item" role="presentation">
                    <button class="nav-link text-uppercase fw-bold text-dark border-0 bg-transparent position-relative py-3 px-4 tab-custom-style"
                        id="reviews-tab" data-bs-toggle="tab" data-bs-target="#reviews-panel" type="button" role="tab"
                        aria-controls="reviews-panel" aria-selected="false" style="letter-spacing: 1px; font-size: 1.1rem;">
                        <%= Resources.Strings.Tab_Reviews %>
                    </button>
                </li>
            </ul>

            <div class="tab-content bg-white p-2 rounded" id="productDetailTabsContent">

                <div class="tab-pane fade show active" id="similar-panel" role="tabpanel" aria-labelledby="similar-tab">
                    <div class="row" id="divSimilar" runat="server">
                        <div class="col-md-12">
                            <div class="products-slick" id="similar-slick">
                                <asp:Repeater ID="rptSimilar" runat="server">
                                    <ItemTemplate>
                                        <div class="card text-dark product-card position-relative">

                                            <div class="position-absolute top-0 start-0 m-2 d-flex flex-column gap-1" style="z-index: 5;">
                                                <%# Convert.ToBoolean(Eval("IsOnSale")) ? "<span class='badge bg-danger text-uppercase fw-bold shadow-sm px-2 py-1' style='font-size:0.65rem;'><i class='fas fa-percentage me-1'></i>SALE -" + Eval("DiscountPercentage") + "%</span>" : "" %>
                                            </div>                                            <img src='<%# string.IsNullOrEmpty(Eval("ImageURL").ToString()) ? "assets/img/default-product.jpg" :  
                  (Eval("ImageURL").ToString().StartsWith("http") || Eval("ImageURL").ToString().StartsWith("assets/") || Eval("ImageURL").ToString().StartsWith("images/") ?  
                  Eval("ImageURL").ToString() : "images/camisetas/" + Eval("ImageURL").ToString()) %>'
                                                class="card-img-top" alt='<%# FormatJerseyName(Eval("Name")) %>'
                                                style="height: 250px; object-fit: cover;"
                                                onerror="this.src='assets/img/default-product.jpg';" />

                                            <div class="card-body d-flex flex-column">
                                                <h5 class="card-title text-danger" style="font-weight: 700;" title='<%# FormatJerseyName(Eval("Name")) %>'>
                                                     <%# FormatJerseyName(Eval("Name")).Length > 25 ? FormatJerseyName(Eval("Name")).Substring(0, 25) + "..." : FormatJerseyName(Eval("Name")) %>
                    </h5>
                                                <p class="card-text text-secondary mb-1"><%# Eval("Team") %> - <%# Eval("Year") %></p>
                                                <p class="card-text text-secondary mb-2"><%# Eval("Brand") %> | <%# Eval("Type") %></p>

                                                <h6 class="mb-3" style="font-size: 1.2rem; font-weight: bold;">$ <%# Eval("FinalPrice", "{0:F2}") %>
                                                    <%# Convert.ToBoolean(Eval("IsOnSale")) ? "<span class='text-muted ms-1' style='text-decoration: line-through; font-size: 0.85rem;'>$" + Eval("OriginalPrice", "{0:F2}") + "</span>" : "" %>
                                                </h6>

                                                <div class="mt-auto">
                                                    <p class="small mb-2 text-dark"><%= Resources.Strings.Card_Sizes %>: <strong><%# Eval("Sizes") %></strong></p>
                                                    <a href='DetailsShirt.aspx?id=<%# Eval("ID") %>' class="btn btn-outline-danger w-100"><%= Resources.Strings.Card_BuyBtn %></a>
                                                </div>
                                            </div>
                                        </div>
                                    </ItemTemplate>
                                </asp:Repeater>
                            </div>
                        </div>
                    </div>
                </div>

                <div class="tab-pane fade" id="reviews-panel" role="tabpanel" aria-labelledby="reviews-tab">
                    <asp:UpdatePanel ID="upReviews" runat="server" UpdateMode="Conditional">
                        <ContentTemplate>

                            <asp:PlaceHolder ID="phLeaveReview" runat="server">
                                <div class="card mb-4 border-1 shadow-sm">
                                    <div class="card-body">
                                        <h4 class="card-title text-dark mb-3" style="font-weight: 600;"><%= Resources.Strings.Review_LeaveTitle %></h4>
                                        <div class="row g-3">
                                            <div class="col-md-12">
                                                <label class="form-label text-dark fw-bold small d-block"><%= Resources.Strings.Review_Rating %></label>
                                                <!-- Selector moderno de estrellas interactivas -->
                                                <div class="star-rating-input d-flex gap-1 text-warning mb-2" style="font-size: 1.7rem; cursor: pointer;">
                                                    <i class="fas fa-star star-click" data-value="1"></i>
                                                    <i class="fas fa-star star-click" data-value="2"></i>
                                                    <i class="fas fa-star star-click" data-value="3"></i>
                                                    <i class="fas fa-star star-click" data-value="4"></i>
                                                    <i class="fas fa-star star-click" data-value="5"></i>
                                                </div>
                                                <asp:HiddenField ID="hfRatingInput" runat="server" Value="5" />
                                            </div>
                                            <div class="col-md-12">
                                                <label class="form-label text-dark fw-bold small"><%= Resources.Strings.Review_CommentLabel %></label>
                                                <asp:TextBox ID="txtComment" runat="server" TextMode="MultiLine" Rows="3" CssClass="form-control border-dark" placeholder="<%$ Resources:Strings, Review_CommentPlaceholder %>"></asp:TextBox>
                                            </div>
                                            <div class="col-md-12">
                                                <asp:Button ID="btnSubmitReview" runat="server" Text="<%$ Resources:Strings, Review_SubmitBtn %>" CssClass="btn btn-danger-custom fw-bold px-4" OnClick="btnSubmitReview_Click" />
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </asp:PlaceHolder>

                            <asp:PlaceHolder ID="phMustPurchaseToReview" runat="server" Visible="false">
                                <div class="alert alert-light text-center border border-warning my-4 p-3 rounded shadow-sm">
                                    <i class="fa-solid fa-lock text-warning mb-2" style="font-size: 1.5rem;"></i>
                                    <p class="mb-0 fw-semibold text-secondary"><%= Resources.Strings.Review_MustPurchase %></p>
                                </div>
                            </asp:PlaceHolder>

                            <div class="flex-grow-1">

                                <asp:PlaceHolder ID="phNoReviews" runat="server" Visible="false">
                                    <div class="alert alert-light text-center border border-secondary my-4 p-4 rounded shadow-sm">
                                        <i class="fa-regular fa-comments text-muted mb-2" style="font-size: 2rem;"></i>
                                        <p class="mb-0 fw-semibold text-secondary" style="font-size: 1.1rem;"><%= Resources.Strings.Review_NoReviews %></p>
                                    </div>
                                </asp:PlaceHolder>

                                <asp:PlaceHolder ID="PlaceHolder1" runat="server" Visible="false">
                                    <div class="alert alert-light text-center border border-secondary my-4 p-4 rounded shadow-sm">
                                        <i class="fa-regular fa-comments text-muted mb-2" style="font-size: 2rem;"></i>
                                        <p class="mb-0 fw-semibold text-secondary" style="font-size: 1.1rem;"><%= Resources.Strings.Review_NoReviews %></p>
                                    </div>
                                </asp:PlaceHolder>

                                <asp:Repeater ID="rptReviews" runat="server" OnItemCommand="rptReviews_ItemCommand">
                                    <ItemTemplate>
                                        <div class="card mb-3 shadow-sm border-1 text-dark">
                                            <div class="card-body">
                                                <div class="d-flex justify-content-between align-items-start mb-2">
                                                    <div>
                                                        <strong class="text-dark"><%# Eval("Name") %> <%# Eval("LastName") %></strong>

                                                        <asp:PlaceHolder ID="phAdminBadgeAuthor" runat="server"
                                                            Visible='<%# Eval("Id_Role").ToString() == "1" || Eval("Id_Role").ToString() == "2" %>'>
                                                            <span class="badge bg-danger ms-2"><i class="fa-solid fa-shield-halved"></i>Administrator</span>
                                                        </asp:PlaceHolder>

                                                        <span class="text-warning ms-2">
                                                            <%# new string('\u2605', Convert.ToInt32(Eval("Rating"))) %><%# new string('\u2606', 5 - Convert.ToInt32(Eval("Rating"))) %>
                                                        </span>
                                                    </div>
                                                    <div class="d-flex align-items-center gap-3">
                                                        <small class="text-muted"><%# Convert.ToDateTime(Eval("ReviewDate")).ToString("MM/dd/yyyy hh:mm tt") %></small>

                                                        <asp:LinkButton ID="btnDeleteReview" runat="server"
                                                            CommandName="DeleteReview"
                                                            CommandArgument='<%# Eval("Id_Review") %>'
                                                            CssClass="btn btn-sm btn-outline-danger d-inline-flex align-items-center gap-1"
                                                            Style="border-radius: 20px; padding: 4px 12px;"
                                                            OnClientClick="<%$ Resources:Strings, Review_DeleteConfirm %>"
                                                            Visible='<%# CanDeleteReview(Eval("Id_User")) %>'>
                                    <i class="fa fa-trash"></i> <%= Resources.Strings.Review_DeleteBtn %>
                                                        </asp:LinkButton>
                                                    </div>
                                                </div>

                                                <p class="card-text text-secondary mb-2"><%# Eval("Comment") %></p>

                                                <asp:Panel ID="pnlReplyForm" runat="server" CssClass="mt-3 bg-light p-2 rounded"
                                                    Visible='<%# IsCurrentUserAdminOrOwner() && string.IsNullOrEmpty(Eval("ReplyComment") as string) %>'>
                                                    <div class="input-group input-group-sm">
                                                        <asp:TextBox ID="txtReply" runat="server" CssClass="form-control" placeholder="<%$ Resources:Strings, Review_ReplyPlaceholder %>" />
                                                        <asp:Button ID="btnSubmitReply" runat="server" Text="<%$ Resources:Strings, Review_ReplyBtn %>"
                                                            CommandName="SubmitReply"
                                                            CommandArgument='<%# Eval("Id_Review") %>'
                                                            CssClass="btn btn-warning text-dark fw-bold" />
                                                    </div>
                                                </asp:Panel>

                                                <div class="clearfix"></div>

                                                <asp:PlaceHolder ID="phReplySection" runat="server" Visible='<%# !string.IsNullOrEmpty(Eval("ReplyComment") as string) %>'>
                                                    <div class="mt-3 ms-4 p-3 bg-light border-start border-warning border-3 rounded shadow-xs">
                                                        <div class="d-flex justify-content-between align-items-center mb-1">
                                                            <div>
                                                                <span class="badge bg-dark"><i class="fa-solid fa-user-check"></i><%= Resources.Strings.Nav_AdminRole %></span>
                                                                <small class="text-muted ms-2 fw-bold"><%= Resources.Strings.Review_OfficialReply %></small>
                                                            </div>
                                                            <small class="text-muted"><%# Eval("ReplyDate") != DBNull.Value ? Convert.ToDateTime(Eval("ReplyDate")).ToString("MM/dd/yyyy hh:mm tt") : "" %></small>
                                                        </div>
                                                        <p class="mb-0 text-dark fst-italic">"<%# Eval("ReplyComment") %>"</p>
                                                    </div>
                                                </asp:PlaceHolder>
                                            </div>
                                        </div>
                                    </ItemTemplate>
                                </asp:Repeater>
                            </div>
                        </ContentTemplate>
                    </asp:UpdatePanel>
                </div>
                <%-- cierra .tab-pane#reviews-panel --%>
            </div>
            <%-- cierra .tab-content --%>
        </div>
        <%-- cierra .container my-5 --%>

        <!-- ============================================== -->
        <!-- SIZE GUIDE MODAL (VENTANA DE MEDIDAS) -->
        <!-- ============================================== -->
        <div class="modal fade" id="sizeGuideModal" tabindex="-1" aria-labelledby="sizeGuideModalLabel" aria-hidden="true">
            <div class="modal-dialog modal-dialog-centered">
                <div class="modal-content" style="border-radius: 12px; border: none; box-shadow: 0 10px 30px rgba(0,0,0,0.2);">
                    <div class="modal-header bg-dark text-white" style="border-top-left-radius: 12px; border-top-right-radius: 12px;">
                        <h5 class="modal-title fw-bold text-warning" id="sizeGuideModalLabel">
                            <i class="fas fa-ruler-combined me-2"></i>Size Guide (<asp:Label ID="lblGuideBrand" runat="server"></asp:Label>)
                        </h5>
                        <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal" aria-label="Close"></button>
                    </div>
                    <div class="modal-body p-4 text-dark">
                        <p class="text-muted small mb-3"><%= Resources.Strings.Modal_SizeGuideDesc %></p>
                        <div class="table-responsive">
                            <table class="table table-bordered table-striped text-center align-middle">
                                <thead class="table-dark">
                                    <tr>
                                        <th><%= Resources.Strings.Modal_TableHeaderSize %></th>
                                        <th><%= Resources.Strings.Modal_TableHeaderChest %></th>
                                        <th><%= Resources.Strings.Modal_TableHeaderLength %></th>
                                    </tr>
                                </thead>
                                <tbody>
                                    <asp:Literal ID="litSizeGuideTable" runat="server"></asp:Literal>
                                </tbody>
                            </table>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <uc:Chatbot runat="server" ID="ucChatbot" />

        <uc:Footer ID="ControlFooter" runat="server" />
        <asp:UpdatePanel ID="upAlerta" runat="server" UpdateMode="Always">
            <ContentTemplate>
                <asp:Literal ID="alerta" runat="server" EnableViewState="false"></asp:Literal>
            </ContentTemplate>
        </asp:UpdatePanel>
    </form>


    <script src="https://ajax.googleapis.com/ajax/libs/jquery/3.6.0/jquery.min.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.2.3/dist/js/bootstrap.bundle.min.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
    <script src="js/slick.min.js"></script>

    <script type="text/javascript">
        /* ==========================================================================
           1. RECARGA DE PÃGINA (Al usar el botÃ³n de atrÃ¡s del navegador)
           ========================================================================== */
        window.onpageshow = function (event) {
            if (event.persisted) {
                window.location.reload();
            }
        };
        /* ==========================================================================
           2. REESCRITURA DE SWEETALERT (Para mantener tus colores)
           ========================================================================== */
        (function () {
            if (window.Swal) {
                const realSwalFire = window.Swal.fireOriginal || window.Swal.fire;
                window.Swal.fire = function (...args) {
                    if (args.length > 0 && typeof args[0] !== 'object') {
                        return realSwalFire.call(window.Swal, {
                            title: args[0],
                            text: args[1] || '',
                            type: args[2] || undefined,
                            confirmButtonColor: '#FFC800'
                        });
                    }
                    if (args.length === 1 && typeof args[0] === 'object') {
                        if (!args[0].confirmButtonColor) args[0].confirmButtonColor = '#FFC800';
                        if (args[0].icon && !args[0].type) {
                            args[0].type = args[0].icon;
                            delete args[0].icon;
                        }
                    }
                    return realSwalFire.apply(window.Swal, args);
                };

                if (window.swalQueue && window.swalQueue.length > 0) {
                    window.swalQueue.forEach(function (pendingArgs) {
                        window.Swal.fire(...pendingArgs);
                    });
                    window.swalQueue = [];
                }
            }
        })();

        /* ==========================================================================
            2. MENÃš DE USUARIO (Dropdown navbar)
            ========================================================================== */
        function ScriptParaAbrirMenu() {
            const menu = document.getElementById('userDropdownMenuUser');
            if (menu) menu.style.display = 'block';
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
            if (!container) cerrarTodosLosMenus();
        };

        /* ==========================================================================
           4. CANTIDAD DE PRODUCTOS EN EL CARRITO (+ / -)
           ========================================================================== */
        function changeQuantity(amount) {
            var txtDisplay = document.getElementById('txtDisplayQty');
            var hfQty = document.getElementById('<%= hfQuantity.ClientID %>');
        var hfMax = document.getElementById('<%= hfMaxStock.ClientID %>');

            var currentQty = parseInt(txtDisplay.value) || 1;
            var maxStock = parseInt(hfMax.value) || 0;

            if (maxStock === 0) {
                Swal.fire({
                    icon: 'info',
                    title: 'Please Select a Size',
                    text: 'You must choose a size before adjusting the quantity.',
                    confirmButtonColor: '#FFC800'
                });
                return;
            }

            var newQty = currentQty + amount;

            if (newQty < 1) {
                newQty = 1;
                txtDisplay.value = newQty;
                hfQty.value = newQty;
                return;
            }

            if (amount > 0 && newQty > maxStock) {
                Swal.fire({
                    icon: 'warning',
                    title: 'Stock Limit Reached',
                    text: 'You have reached the maximum available stock for this size.',
                    confirmButtonColor: '#FFC800'
                });
                return;
            }

            txtDisplay.value = newQty;
            hfQty.value = newQty;
        }

        /* ==========================================================================
           5. LÃ“GICA DE PRECIOS MATEMÃTICOS PARA EL SWITCH DE PERSONALIZACIÃ“N
           ========================================================================== */
        var basePrice = 0.00;
        var originalBasePrice = 0.00;
        var hasDiscount = false;

        document.addEventListener("DOMContentLoaded", function () {
            var lblPriceElement = document.getElementById("<%= lblPrice.ClientID %>");
        if (lblPriceElement) {
            var htmlContent = lblPriceElement.innerHTML;
            var spanIndex = htmlContent.indexOf('<span');

            if (spanIndex !== -1) {
                hasDiscount = true;
                var numStr = htmlContent.substring(0, spanIndex).replace('$', '').trim();
                basePrice = parseFloat(numStr);

                var spanContent = htmlContent.substring(spanIndex);
                var match = spanContent.match(/\$([\d\.]+)/);
                if (match && match[1]) {
                    originalBasePrice = parseFloat(match[1]);
                } else {
                    originalBasePrice = basePrice;
                }
            } else {
                hasDiscount = false;
                basePrice = parseFloat(htmlContent.replace('$', '').trim());
                originalBasePrice = basePrice;
            }
        }
    });

        function togglePersonalizacion() {
            var chk = document.getElementById("chkCustomize");
            var fieldsDiv = document.getElementById("personalizationFields");
            var previewDiv = document.getElementById("previewPersonalizacion");
            var priceLabel = document.getElementById("<%= lblPrice.ClientID %>");
            var sliderVisual = document.getElementById("sliderVisual");

            if (!chk || !priceLabel) return;

            if (chk.checked) {
                if (fieldsDiv) fieldsDiv.style.display = "block";
                if (previewDiv) previewDiv.style.display = "flex";
                if (sliderVisual) sliderVisual.classList.add("slider-active");

                var newFinalPrice = basePrice + 15.00;
                var newOriginalPrice = originalBasePrice + 15.00;

                if (hasDiscount) {
                    priceLabel.innerHTML = newFinalPrice.toFixed(2) + " <span style='text-decoration: line-through; font-size: 0.6em; color: #888; margin-left: 8px;'>$" + newOriginalPrice.toFixed(2) + "</span>";
                } else {
                    priceLabel.innerHTML = newFinalPrice.toFixed(2);
                }

                // Sync the preview with the text boxes
                var txtName = document.getElementById("txtCustomName");
                var txtNum = document.getElementById("txtCustomNumber");
                var previewNombre = document.getElementById("previewNombre");
                var previewNumero = document.getElementById("previewNumero");

                if (txtName && previewNombre) {
                    var nameVal = txtName.value.toUpperCase();
                    previewNombre.textContent = nameVal || "NAME";
                    var nameLength = nameVal.length;
                    var fontSize = 28;
                    if (nameLength > 6) {
                        fontSize = 28 - (nameLength - 6) * 1.8;
                    }
                    previewNombre.style.fontSize = fontSize + "px";
                }
                if (txtNum && previewNumero) {
                    var numVal = txtNum.value;
                    previewNumero.textContent = numVal || "0";
                    if (numVal.length > 1) {
                        previewNumero.style.fontSize = "100px";
                        previewNumero.style.left = "48.5%";
                    } else {
                        previewNumero.style.fontSize = "120px";
                    }
                }
            } else {
                if (fieldsDiv) fieldsDiv.style.display = "none";
                if (previewDiv) previewDiv.style.display = "none";

                var txtName = document.getElementById("txtCustomName");
                var txtNum = document.getElementById("txtCustomNumber");

                // SIMPLEMENTE COMENTAMOS O ELIMINAMOS LA LIMPIEZA DE LOS VALORES
                // if (txtName) txtName.value = ""; 
                // if (txtNum) txtNum.value = "";

                var previewNombre = document.getElementById("previewNombre");
                var previewNumero = document.getElementById("previewNumero");

                if (previewNombre) {
                    previewNombre.textContent = "NAME";
                    previewNombre.style.fontSize = "28px";
                }
                if (previewNumero) {
                    previewNumero.textContent = "0";
                    previewNumero.style.fontSize = "120px";
                }

                if (sliderVisual) sliderVisual.classList.remove("slider-active");

                if (hasDiscount) {
                    priceLabel.innerHTML = basePrice.toFixed(2) + " <span style='text-decoration: line-through; font-size: 0.6em; color: #888; margin-left: 8px;'>$" + originalBasePrice.toFixed(2) + "</span>";
                } else {
                    priceLabel.innerHTML = basePrice.toFixed(2);
                }
            }
        }

        /* ==========================================================================
           6. SCRIPTS QUE DEPENDEN DE JQUERY (Carrusel, Estrellas y Preview Text)
           ========================================================================== */
        $(document).ready(function () {

            // Carrusel
            $('#similar-slick').slick({
                slidesToShow: 4,
                slidesToScroll: 1,
                autoplay: true,
                autoplaySpeed: 3000,
                infinite: false,
                prevArrow: '<button type="button" class="offside-prev"><i class="fa fa-angle-left"></i></button>',
                nextArrow: '<button type="button" class="offside-next"><i class="fa fa-angle-right"></i></button>',
                responsive: [
                    { breakpoint: 1200, settings: { slidesToShow: 3 } },
                    { breakpoint: 991, settings: { slidesToShow: 2 } },
                    { breakpoint: 575, settings: { slidesToShow: 1 } }
                ]
            });

            // Recalcular carrusel en tabs
            $('button[data-bs-toggle="tab"]').on('shown.bs.tab', function (e) {
                $('#similar-slick').slick('setPosition');
            });

            // Estrellas de ReseÃ±as
            var initialRating = $('#<%= hfRatingInput.ClientID %>').val() || 5;
        renderStars(initialRating);

        $(document).on('click', '.star-click', function () {
            var selectedValue = $(this).data('value');
            $('#<%= hfRatingInput.ClientID %>').val(selectedValue);
            renderStars(selectedValue);
        });

        function renderStars(value) {
            $('.star-click').each(function () {
                var starValue = $(this).data('value');
                if (starValue <= value) {
                    $(this).removeClass('far').addClass('fas');
                } else {
                    $(this).removeClass('fas').addClass('far');
                }
            });
        }

        // Preview en vivo de Nombre y NÃºmero (Usando delegaciÃ³n de eventos de jQuery para sobrevivir a postbacks parciales)
        $(document).on('input', '#txtCustomName', function (e) {
            var cursorPosition = this.selectionStart;
            var rawVal = this.value;
            var cleaned = rawVal.replace(/[^a-zA-ZÃ¡Ã©Ã­Ã³ÃºÃÃ‰ÃÃ“ÃšÃ±Ã‘Ã¼Ãœ ]/g, "");

            if (cleaned !== rawVal) {
                this.value = cleaned;
                if (cursorPosition !== null) {
                    var diff = rawVal.length - cleaned.length;
                    this.setSelectionRange(cursorPosition - diff, cursorPosition - diff);
                }
            }
            var nameUpper = this.value.toUpperCase();
            var previewNombre = document.getElementById("previewNombre");
            if (previewNombre) {
                previewNombre.textContent = nameUpper || "NAME";

                var nameLength = nameUpper.length;
                var fontSize = 28;
                if (nameLength > 6) {
                    fontSize = 28 - (nameLength - 6) * 1.8;
                }
                previewNombre.style.fontSize = fontSize + "px";
            }
        });

        $(document).on('input', '#txtCustomNumber', function (e) {
            var cursorPosition = this.selectionStart;
            var rawVal = this.value;
            var cleaned = rawVal.replace(/[^0-9]/g, "");

            if (cleaned !== rawVal) {
                this.value = cleaned;
                if (cursorPosition !== null) {
                    var diff = rawVal.length - cleaned.length;
                    this.setSelectionRange(cursorPosition - diff, cursorPosition - diff);
                }
            }
            var numberVal = this.value;
            var previewNumero = document.getElementById("previewNumero");
            if (previewNumero) {
                previewNumero.textContent = numberVal || "0";

                if (numberVal.length > 1) {
                    previewNumero.style.fontSize = "100px";
                    previewNumero.style.left = "48.5%";
                } else {
                    previewNumero.style.fontSize = "120px";
                }
            }
        });

        if (previewNombre) {
            previewNombre.textContent = "NAME";
            previewNombre.style.fontSize = "28px";
        }
        if (previewNumero) {
            previewNumero.textContent = "0";
            previewNumero.style.fontSize = "120px";
        }

        // Manejador para mantener el estado cliente tras postbacks parciales de AJAX ASP.NET
        if (typeof Sys !== 'undefined' && Sys.WebForms && Sys.WebForms.PageRequestManager) {
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
                // 1. Sincronizar campos de personalizaciÃ³n
                if (typeof togglePersonalizacion === "function") {
                    togglePersonalizacion();
                }
                // 2. Sincronizar valoraciÃ³n de estrellas de reseÃ±a
                var hfRatingInput = document.getElementById('<%= hfRatingInput.ClientID %>');
                if (hfRatingInput && typeof renderStars === "function") {
                    renderStars($(hfRatingInput).val() || 5);
                }
                // 3. Sincronizar cantidad seleccionada visualmente con hfQuantity
                var hfQty = document.getElementById('<%= hfQuantity.ClientID %>');
                var txtDisplay = document.getElementById('txtDisplayQty');
                if (hfQty && txtDisplay) {
                    txtDisplay.value = hfQty.value;
                }
            });
        }
    });
    </script>
</body>
</html>

