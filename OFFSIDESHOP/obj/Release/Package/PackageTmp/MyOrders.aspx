<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="MyOrders.aspx.cs" Inherits="OFFSIDESHOP.MyOrders" %>

<%@ Register Src="~/FooterControl.ascx" TagPrefix="uc" TagName="Footer" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1, shrink-to-fit=no" />
    <title>OffsideShop - My Orders</title>

    <link rel="icon" type="image/x-icon" href="assets/favicon.ico" />

    <script src="https://use.fontawesome.com/releases/v6.3.0/js/all.js" crossorigin="anonymous"></script>

    <link href="https://fonts.googleapis.com/css?family=Montserrat:400,700" rel="stylesheet" type="text/css" />
    <link href="https://fonts.googleapis.com/css?family=Roboto+Slab:400,100,300,700" rel="stylesheet" type="text/css" />

    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.2.3/dist/css/bootstrap.min.css" rel="stylesheet" />

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

        .btn-secondary-custom {
            border: 2px solid #cccccc;
            color: #555555 !important;
            padding: 10px 25px;
            border-radius: 30px;
            font-weight: 600;
            background: transparent;
            text-decoration: none;
            transition: all 0.3s ease;
            font-family: 'Montserrat', sans-serif;
            font-size: 0.9rem;
            display: inline-flex;
            align-items: center;
            gap: 8px;
        }

            .btn-secondary-custom:hover {
                background-color: #eef2f3;
                color: #111111 !important;
                border-color: #999999;
                transform: translateY(-2px);
                box-shadow: 0 4px 10px rgba(0, 0, 0, 0.05);
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

    <script type="text/javascript">
        window.onpageshow = function (event) {
            if (event.persisted) {
                window.location.reload();
            }
        };
    </script>
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
<body id="page-top" style="display: flex; flex-direction: column; min-height: 100vh; margin: 0;">
    <form runat="server" style="flex: 1 0 auto; display: flex; flex-direction: column;">

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
                                        <i class="fas fa-sign-in-alt"></i>Log in
                                    </a>
                                    <a href="SignUp.aspx" class="dropdown-item">
                                        <i class="fas fa-user-plus"></i>Sign up
                                    </a>
                                    <asp:Button ID="Button1" runat="server" CssClass="dropdown-item btn-logout" Text="Back to Shop" OnClick="btnbackshop_Click" />
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
      <i class="fas fa-user-cog"></i> My Account
  </asp:LinkButton>
                                             <asp:Button ID="btnbackshop" runat="server" CssClass="dropdown-item btn-logout" Text="Back to Shop" OnClick="btnbackshop_Click" />
                                             <asp:Button ID="btncerrar" runat="server" CssClass="dropdown-item btn-logout" Text="Log out" OnClick="btncerrar_Click" />
                                         </div>
                                     </div>

                                 </ContentTemplate>
                             </asp:UpdatePanel>
                         </div>
                     </div>
                 </asp:PlaceHolder>

                </div>
            </div>
        </nav>

        <div style="margin-top: 140px;"></div>

        <div class="container-fluid" style="background-color: #f8f9fa !important; flex: 1 0 auto; padding-bottom: 60px;">
            <div class="container">

                <div class="mb-4">
                    <a href="Homepage.aspx" class="btn btn-secondary-custom">
                        <i class="fas fa-arrow-left"></i> Back to Homepage
                    </a>
                </div>

                <div class="row mb-5 text-center mt-5">
                    <div class="col-12">
                        <h2 style="font-weight: 800; letter-spacing: -1px; margin-bottom: 15px; color: #1a1a1a;"><asp:Literal runat="server" Text="<%$ Resources:Strings, MyOrders_Title %>" /></h2>
                        <div style="width: 60px; height: 4px; background-color: #ffc800; margin: 0 auto 20px auto; border-radius: 2px;"></div>
                        <p style="color: #666666 !important;"><asp:Literal runat="server" Text="<%$ Resources:Strings, MyOrders_SubTitle %>" /></p>
                    </div>
                </div>

                <div class="row justify-content-center">
                    <div class="col-12 col-md-8">
                        <asp:Label ID="lblNoOrders" runat="server" Style="display: block; margin: 10px auto; padding: 15px; text-align: center;" CssClass="alert alert-warning" Text="<%$ Resources:Strings, MyOrders_NoOrders %>" Visible="false" />
                    </div>
                </div>

                <div class="row g-4">
                    <asp:Repeater ID="rptOrders" runat="server">
                        <ItemTemplate>
                            <div class="col-12 col-lg-6 d-flex">
                                <div class="w-100 style-card" style="background-color: #ffffff !important; border: 1px solid #e0e0e0 !important; border-radius: 12px; padding: 25px; box-shadow: 0px 4px 20px rgba(0,0,0,0.06); display: flex; flex-direction: column; justify-content: space-between;">

                                    <div>
                                        <div class="d-flex justify-content-between align-items-start border-bottom pb-3 mb-3" style="border-color: #eeeeee !important;">
                                            <div>
                                                <span style="color: #999999; text-transform: uppercase; font-size: 11px; font-weight: 600; display: block; letter-spacing: 0.5px;"><asp:Literal runat="server" Text="<%$ Resources:Strings, MyOrders_OrderRef %>" /></span>
                                                <strong style="color: #1a1a1a !important; font-size: 20px;">#<%# Eval("id_order") %></strong>
                                            </div>

                                            <div class="text-end">
                                                <span style="color: #999999; text-transform: uppercase; font-size: 11px; font-weight: 600; display: block; letter-spacing: 0.5px; margin-bottom: 4px;"><asp:Literal runat="server" Text="<%$ Resources:Strings, MyOrders_Status %>" /></span>
                                                <span class="badge" style="background-color: #1a1a1a !important; color: #ffc800 !important; border: 1px solid #ffc800; font-size: 12px; padding: 6px 14px; font-weight: 600; text-transform: uppercase; letter-spacing: 0.5px; border-radius: 20px;">
                                                    <%# Eval("order_status") %>
                                                </span>
                                            </div>
                                        </div>

                                        <div class="order-summary">
                                            <div class="row mb-2" style="font-size: 13px;">
                                                <div class="col-12 col-sm-6 mb-2 mb-sm-0">
                                                    <span style="color: #888888; display: block; font-weight: 500; font-size: 11px;"><asp:Literal runat="server" Text="<%$ Resources:Strings, MyOrders_DatePurchased %>" /></span>
                                                    <span style="color: #333333; font-weight: 600;"><%# Eval("order_date", "{0:dd/MM/yyyy HH:mm}") %></span>
                                                </div>
                                                <div class="col-12 col-sm-6 text-sm-end">
                                                    <span style="color: #888888; display: block; font-weight: 500; font-size: 11px;"><asp:Literal runat="server" Text="<%$ Resources:Strings, MyOrders_ShippingDetails %>" /></span>
                                                    <span style="color: #333333; font-weight: 600; display: inline-block; max-width: 200px; text-overflow: ellipsis; overflow: hidden; white-space: nowrap;" title="<%# Eval("shipping_address") %>, <%# Eval("city") %>">
                                                        <%# Eval("shipping_address") %>, <%# Eval("city") %>
                                                    </span>
                                                </div>
                                            </div>

                                            <hr style="border-top: 1px dashed #cccccc !important; background-color: transparent;" />

                                            <div style="color: #1a1a1a !important; font-size: 12px; font-weight: 700; letter-spacing: 0.5px; text-transform: uppercase; margin-bottom: 8px;"><asp:Literal runat="server" Text="<%$ Resources:Strings, MyOrders_ProductsOrdered %>" /></div>

                                            <div class="order-products mt-2 ps-1" style="font-size: 14px; line-height: 1.8; color: #333333 !important; max-height: 150px; overflow-y: auto;">
                                                <asp:Repeater ID="rptProducts" runat="server">
                                                    <ItemTemplate>
                                                        <div class="d-flex justify-content-between align-items-center" style="border-bottom: 1px solid #f5f5f5; padding: 6px 0;">
                                                            <span style="color: #444444 !important; font-size: 13px;"><i class="fa-solid fa-shirt text-muted me-2"></i><%# FormatJerseyName(Eval("Product")) %>
                                                                <span style="color: #777777; font-size: 12px; margin-left: 4px;">(Size: <%# Eval("Size") %>)</span>
                                                                <strong style="color: #1a1a1a; margin-left: 6px;">x<%# Eval("Quantity") %></strong>
                                                            </span>
                                                            <span style="color: #1a1a1a !important; font-weight: 600; font-size: 13px;">$<%# Convert.ToDouble(Eval("Price")) * Convert.ToInt32(Eval("Quantity")) %>
                                                            </span>
                                                        </div>
                                                    </ItemTemplate>
                                                </asp:Repeater>
                                            </div>

                                            <hr style="border-top: 1px solid #eeeeee !important; background-color: transparent; margin-top: 15px; margin-bottom: 15px;" />

                                            <div class="d-flex justify-content-between align-items-center mb-4">
                                                <div>
                                                    <strong style="color: #1a1a1a !important; text-transform: uppercase; font-size: 12px; letter-spacing: 0.5px;"><asp:Literal runat="server" Text="<%$ Resources:Strings, MyOrders_TotalCharged %>" /></strong>
                                                </div>
                                                <div>
                                                    <strong style="color: #1a1a1a !important; font-size: 22px; font-family: 'Montserrat', sans-serif; font-weight: 700;">$<%# Eval("total_amount", "{0:F2}") %></strong>
                                                </div>
                                            </div>
                                        </div>
                                    </div>

                                    <div class="card-actions mt-auto pt-2">
                                        <a class="btn w-100 text-center" href="OrderDetail.aspx?id=<%# Eval("id_order") %>"
                                            style="color: #000; border: 2px solid #ffc800; background: linear-gradient(135deg, #ffc800 0%, #d9a300 100%); border-radius: 8px; padding: 10px 20px; font-weight: 700; font-family: 'Montserrat', sans-serif; text-transform: uppercase; font-size: 12px; letter-spacing: 0.5px; transition: all 0.2s ease;">
                                            <i class="fa-solid fa-circle-info me-2"></i><asp:Literal runat="server" Text="<%$ Resources:Strings, MyOrders_ViewDetails %>" />
                                        </a>
                                    </div>

                                </div>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>

            </div>
        </div>
    </form>

    <uc:Footer ID="ControlFooter" runat="server" />

    <script src="https://ajax.googleapis.com/ajax/libs/jquery/3.6.0/jquery.min.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.2.3/dist/js/bootstrap.bundle.min.js"></script>
</body>
</html>

