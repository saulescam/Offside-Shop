<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="AboutUs.aspx.cs" Inherits="OFFSIDESHOP.AboutUs" %>
<%@ Register Src="~/FooterControl.ascx" TagPrefix="uc" TagName="Footer" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1, shrink-to-fit=no" />
    <title>OffsideShop - About Us</title>

    <link rel="icon" type="image/x-icon" href="assets/favicon.ico" />

    <script src="https://use.fontawesome.com/releases/v6.3.0/js/all.js" crossorigin="anonymous"></script>

    <link href="https://fonts.googleapis.com/css?family=Montserrat:400,700" rel="stylesheet" type="text/css" />
    <link href="https://fonts.googleapis.com/css?family=Roboto+Slab:400,100,300,700" rel="stylesheet" type="text/css" />

    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.2.3/dist/css/bootstrap.min.css" rel="stylesheet" />

    <link href="css/styles.css" rel="stylesheet" />
    
    <style>
        .advantage-card {
            background-color: #ffffff !important;
            border: 1px solid #e0e0e0 !important;
            border-radius: 12px;
            padding: 30px;
            box-shadow: 0px 4px 20px rgba(0,0,0,0.04);
            transition: transform 0.3s ease, box-shadow 0.3s ease;
            height: 100%;
        }
        .advantage-card:hover {
            transform: translateY(-5px);
            box-shadow: 0px 8px 25px rgba(0,0,0,0.08);
        }
        .icon-box {
            width: 60px;
            height: 60px;
            background-color: #1a1a1a;
            border: 2px solid #ffc800;
            color: #ffc800;
            border-radius: 50%;
            display: flex;
            align-items: center;
            justify-content: center;
            font-size: 24px;
            margin-bottom: 20px;
        }

        /* Nav dropdown styles */
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
<body id="page-top" style="display: flex; flex-direction: column; min-height: 100vh; margin: 0;"> 
    <form runat="server" style="flex: 1 0 auto; display: flex; flex-direction: column;">
        
        
<!-- Navigation Bar (Fixed Black Version) -->
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
                                    <asp:LinkButton ID="btnGoToAccount" runat="server" CssClass="dropdown-item" OnClick="btnGoToAccount_Click">
                                        <i class="fas fa-user-cog"></i> My Account
                                    </asp:LinkButton>

                                    <asp:LinkButton ID="btnMyOrders" runat="server" CssClass="dropdown-item" OnClick="btnMyOrders_Click">
                                        <i class="fas fa-clipboard-list"></i> My Orders
                                    </asp:LinkButton>

                                    <asp:LinkButton ID="btnNavCart" runat="server" CssClass="dropdown-item" OnClick="btnNavCart_Click">
                                        <i class="fas fa-shopping-cart"></i>Cart 
                                        <span class="badge">
                                            <asp:Label ID="lblCartCount" runat="server" Text="0"></asp:Label>
                                        </span>
                                    </asp:LinkButton>

                                    <asp:Button ID="btncerrar" runat="server" CssClass="dropdown-item btn-logout" Text="Log out" OnClick="btncerrar_Click" />
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
                            <asp:Button ID="btnlogout" runat="server" CssClass="dropdown-item btn-logout" Text="Log out" OnClick="btncerrar_Click" />
                        </div>
                    </div>
                </div>
            </asp:PlaceHolder>
        </div>
    </div>
</nav>
        <!-- Spacer for Fixed Navbar -->
        <div style="margin-top: 140px;"></div>

        <!-- Main Content Section -->
        <div class="container-fluid" style="background-color: #f8f9fa !important; flex: 1 0 auto; padding-bottom: 80px;">
            <div class="container">
                
                <!-- Header -->
                <div class="row justify-content-center text-center mb-5">
                    <div class="col-lg-8">
                        <h2 style="color: #1a1a1a !important; font-family: 'Montserrat', sans-serif; font-weight: 700; letter-spacing: 1px; text-transform: uppercase;">About OffsideShop</h2>
                        <p style="color: #666666 !important; font-size: 18px;">We are more than a store. We are the home of football culture, retro collector's gems, and modern kits.</p>
                    </div>
                </div>

                <!-- Our Story / Concept -->
                <div class="row justify-content-center mb-5">
                    <div class="col-md-10">
                        <div style="background-color: #ffffff !important; border: 1px solid #e0e0e0 !important; border-radius: 12px; padding: 40px; box-shadow: 0px 4px 20px rgba(0,0,0,0.06);">
                            <h3 style="font-family: 'Montserrat', sans-serif; font-weight: 700; color: #1a1a1a; margin-bottom: 20px; text-transform: uppercase; font-size: 22px;">Our Story</h3>
                            <p style="color: #444444; line-height: 1.8; font-size: 15px; margin-bottom: 0;">
                                OFFSIDESHOP was born out of pure passion for football jerseys. We understand that a jersey isn't just sports apparel—it represents a historical moment, a legendary comeback, an unrepeatable collective emotion, or a timeless aesthetic masterpiece. Whether you are looking for the latest drop or an impossible-to-find retro classic, we curate the best kits around the globe so you can wear your passion with pride.
                            </p>
                        </div>
                    </div>
                </div>

                <!-- Our Advantages Section -->
                <div class="row justify-content-center text-center mb-4">
                    <div class="col-12">
                        <h3 style="font-family: 'Montserrat', sans-serif; font-weight: 700; color: #1a1a1a; text-transform: uppercase; letter-spacing: 1px; font-size: 22px; margin-bottom: 40px;">Why Choose Us?</h3>
                    </div>
                </div>

                <!-- Advantages Grid -->
                <div class="row g-4 justify-content-center">
                    <!-- Advantage 1 -->
                    <div class="col-md-4" style="max-width: 380px;">
                        <div class="advantage-card">
                            <div class="icon-box">
                                <i class="fa-solid fa-shirt"></i>
                            </div>
                            <h4 style="font-family: 'Montserrat', sans-serif; font-weight: 700; font-size: 18px; color: #1a1a1a; margin-bottom: 12px;">Premium & Rare Kits</h4>
                            <p style="color: #666666; font-size: 14px; line-height: 1.6; margin-bottom: 0;">
                                From underground drops and high-end collabs to legendary retro grails. If it's unique, it's inside our locker room.
                            </p>
                        </div>
                    </div>

                    <!-- Advantage 2 -->
                    <div class="col-md-4" style="max-width: 380px;">
                        <div class="advantage-card">
                            <div class="icon-box">
                                <i class="fa-solid fa-shield-halved"></i>
                            </div>
                            <h4 style="font-family: 'Montserrat', sans-serif; font-weight: 700; font-size: 18px; color: #1a1a1a; margin-bottom: 12px;">Safe & Secure Shopping</h4>
                            <p style="color: #666666; font-size: 14px; line-height: 1.6; margin-bottom: 0;">
                                Integrated with industry-standard secure payment processors. Your transactions and private details are fully encrypted.
                            </p>
                        </div>
                    </div>

                    <!-- Advantage 3 -->
                    <div class="col-md-4" style="max-width: 380px;">
                        <div class="advantage-card">
                            <div class="icon-box">
                                <i class="fa-solid fa-truck-fast"></i>
                            </div>
                            <h4 style="font-family: 'Montserrat', sans-serif; font-weight: 700; font-size: 18px; color: #1a1a1a; margin-bottom: 12px;">Real-Time Logistics</h4>
                            <p style="color: #666666; font-size: 14px; line-height: 1.6; margin-bottom: 0;">
                                No guessing games. Monitor your order history and dispatch phases in real-time from your personal dashboard.
                            </p>
                        </div>
                    </div>
                </div>

            </div>
        </div>
    </form>
    
    <!-- Footer Control -->
    <uc:Footer ID="ControlFooter" runat="server" />

    <script src="https://ajax.googleapis.com/ajax/libs/jquery/3.6.0/jquery.min.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.2.3/dist/js/bootstrap.bundle.min.js"></script>
</body>
</html>