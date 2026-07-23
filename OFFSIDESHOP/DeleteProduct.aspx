<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="DeleteProduct.aspx.cs" Inherits="OFFSIDESHOP.DeleteProduct" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1, shrink-to-fit=no" />
    <title>Delete Product | OffsideShop</title>

    <!-- CSS -->
    <link href="css/bootstrap.min.css" rel="stylesheet" />
    <link href="https://fonts.googleapis.com/css?family=Raleway:100,400,600,700&display=swap" rel="stylesheet" />
    <link rel="stylesheet" href="https://maxcdn.bootstrapcdn.com/bootstrap/4.3.1/css/bootstrap.min.css" />
    <link href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/5.15.4/css/all.min.css" rel="stylesheet" />
    <link href="css/admin-layout.css" rel="stylesheet" />

    <!-- SweetAlert -->
    <script src="SweetAlert/sweetalert2.all.min.js"></script>
    <script src="SweetAlert/sweetalert2.js"></script>

    <script type="text/javascript">
        window.onpageshow = function (event) {
            if (event.persisted) { window.location.reload(); }
        };

        function validarNumeros(e) {
            var tecla = (document.all) ? e.keyCode : e.which;
            if (tecla == 8) return true;
            return /\d/.test(String.fromCharCode(tecla));
        }
    </script>
</head>
<body>
    <form id="form1" runat="server">
        <!-- Top Navbar -->
        <nav class="top-navbar">
            <a class="navbar-brand" href="Dashboard.aspx">
                <img src="assets/img/offsideshop_logo_white_letras.png" alt="OFFSIDESHOP" />
            </a>
            <span class="navbar-brand text-muted d-none d-md-block"
                style="font-size: 1.2rem; border-left: 1px solid var(--border-color); padding-left: 15px; margin-left: 10px; font-weight: 600;">Delete Product
            </span>
        </nav>

        <div class="layout-wrapper">
            <!-- Sidebar -->
            <aside class="sidebar fade-in">
                <ul class="sidebar-menu">
                    <li>
                        <asp:Button ID="btnManageProducts" CssClass="sidebar-btn" runat="server" Text="&#xf553; Manage Products" OnClick="btnManageProducts_Click" Style="font-family: 'Raleway', 'Font Awesome 5 Free'; font-weight: 600;" />
                    </li>
                    <li>
                        <a id="btnManageOrders" runat="server" href="ManageOrders.aspx" class="sidebar-btn active" style="font-family: 'Raleway','Font Awesome 5 Free'; font-weight: 600;">&#xf46d; Manage Orders</a>
                    </li>
                    <li><a id="btnManageTickets" runat="server" href="ManageSellerRequests.aspx" class="sidebar-btn" style="font-family: 'Raleway','Font Awesome 5 Free'; font-weight: 600;">&#xf2b5; Seller Requests</a></li>

                    <li style="border-top: 1px solid var(--border-color); margin-top: 8px; padding-top: 8px;">
                        <asp:Button ID="btnAddLeague" CssClass="sidebar-btn" runat="server" Text="&#xf1ae; Add League" OnClick="btnAddLeague_Click" Style="font-family: 'Raleway', 'Font Awesome 5 Free'; font-weight: 600;" />
                    </li>
                    <li>
                        <asp:Button ID="btnAddTeam" CssClass="sidebar-btn" runat="server" Text="&#xf0c0; Add Team" OnClick="btnAddTeam_Click" Style="font-family: 'Raleway', 'Font Awesome 5 Free'; font-weight: 600;" />
                    </li>
                    <li>
                        <asp:Button ID="btnAddBrand" CssClass="sidebar-btn" runat="server" Text="&#xf0c0; Add Brand" OnClick="btnAddBrand_Click" Style="font-family: 'Raleway', 'Font Awesome 5 Free'; font-weight: 600;" />
                    </li>

                    <li style="border-top: 1px solid var(--border-color); margin-top: 8px; padding-top: 8px;">
                        <asp:Button ID="btnManageUsers" CssClass="sidebar-btn" runat="server" Text="&#xf4fe; Manage Users" OnClick="btnManageUsers_Click" Style="font-family: 'Raleway', 'Font Awesome 5 Free'; font-weight: 600;" />
                    </li>
                    <li>
                        <asp:Button ID="btnAdminBanners" CssClass="sidebar-btn" runat="server" Text="&#xf03e; Manage Banners" OnClick="btnAdminBanners_Click" Style="font-family: 'Raleway', 'Font Awesome 5 Free'; font-weight: 600;" />
                    </li>

                    <li style="border-top: 1px solid var(--border-color); margin-top: 8px; padding-top: 8px;">
                        <asp:Button ID="btncerrar" CssClass="sidebar-btn btn-logout" runat="server" Text="&#xf2f5; Logout" OnClick="btncerrar_Click" Style="font-family: 'Raleway', 'Font Awesome 5 Free'; font-weight: 600;" />
                    </li>
                </ul>
            </aside>

            <!-- Content -->
            <main class="main-content fade-in" style="animation-delay: 0.2s;">
                <div class="container-fluid">
                    <h1 class="page-title"
                        style="background: linear-gradient(135deg, #ef4444 0%, #dc2626 100%); -webkit-background-clip: text; -webkit-text-fill-color: transparent;">Delete Jersey
                    </h1>

                    <div class="row">
                        <div class="col-xl-8 col-lg-10">
                            <div class="form-card">

                                <!-- Load by ID -->
                                <div class="row mb-4">
                                    <div class="col-md-8">
                                        <div class="form-group mb-0">
                                            <label>Product ID <span class="text-danger">*</span></label>
                                            <asp:TextBox ID="txtID" runat="server"
                                                placeholder="Enter Product ID to delete"
                                                CssClass="form-control"
                                                onkeypress="return validarNumeros(event)">
                                            </asp:TextBox>
                                        </div>
                                    </div>
                                    <div class="col-md-4 d-flex align-items-end">
                                        <asp:Button ID="btnSeleccionar" runat="server"
                                            Text="&#xf021; Load Product"
                                            CssClass="mybtn"
                                            OnClick="btnSeleccionar_Click"
                                            Width="100%"
                                            CausesValidation="false"
                                            Style="font-family: 'Raleway', 'Font Awesome 5 Free'; font-weight: 600; margin-top: 0; margin-bottom: 2px;" />
                                    </div>
                                </div>

                                <hr style="border-color: var(--border-color); margin-bottom: 30px;" />

                                <!-- Product Name (READ-ONLY) -->
                                <div class="row">
                                    <div class="col-md-12">
                                        <div class="form-group">
                                            <label>Product Name</label>
                                            <asp:TextBox ID="txtNombre" runat="server"
                                                placeholder="Product Name"
                                                CssClass="form-control" ReadOnly="true">
                                            </asp:TextBox>
                                        </div>
                                    </div>
                                </div>

                                <!-- League (READ-ONLY) → Brand (READ-ONLY) -->
                                <div class="row">
                                    <div class="col-md-6">
                                        <div class="form-group">
                                            <label>League</label>
                                            <asp:DropDownList ID="ddlLeague" runat="server"
                                                CssClass="form-control" Enabled="false">
                                            </asp:DropDownList>
                                        </div>
                                    </div>
                                    <div class="col-md-6">
                                        <div class="form-group">
                                            <label>Brand</label>
                                            <asp:DropDownList ID="ddlMarca" runat="server"
                                                CssClass="form-control" Enabled="false">
                                            </asp:DropDownList>
                                        </div>
                                    </div>
                                </div>

                                <!-- Team (READ-ONLY) → Year (READ-ONLY) -->
                                <div class="row">
                                    <div class="col-md-6">
                                        <div class="form-group">
                                            <label>Team</label>
                                            <asp:DropDownList ID="ddlEquipo" runat="server"
                                                CssClass="form-control" Enabled="false">
                                            </asp:DropDownList>
                                        </div>
                                    </div>
                                    <div class="col-md-6">
                                        <div class="form-group">
                                            <label>Year</label>
                                            <asp:TextBox ID="txtAnio" runat="server"
                                                placeholder="Year" CssClass="form-control"
                                                ReadOnly="true">
                                            </asp:TextBox>
                                        </div>
                                    </div>
                                </div>

                                <!-- Type (READ-ONLY) → Price (READ-ONLY) -->
                                <div class="row">
                                    <div class="col-md-6">
                                        <div class="form-group">
                                            <label>Kit Type</label>
                                            <asp:DropDownList ID="ddlTipo" runat="server"
                                                CssClass="form-control" Enabled="false">
                                                <asp:ListItem Value="" Text="-- Select Type --"></asp:ListItem>
                                                <asp:ListItem Value="1">Local</asp:ListItem>
                                                <asp:ListItem Value="2">Away</asp:ListItem>
                                                <asp:ListItem Value="3">Third</asp:ListItem>
                                                <asp:ListItem Value="4">Retro</asp:ListItem>
                                                <asp:ListItem Value="5">Training</asp:ListItem>
                                            </asp:DropDownList>
                                        </div>
                                    </div>
                                    <div class="col-md-6">
                                        <div class="form-group">
                                            <label>Price</label>
                                            <asp:TextBox ID="txtPrecio" runat="server"
                                                placeholder="Price" CssClass="form-control"
                                                ReadOnly="true">
                                            </asp:TextBox>
                                        </div>
                                    </div>
                                </div>

                                <!-- 5 READ-ONLY size stock inputs -->
                                <div class="row">
                                    <div class="col-12">
                                        <label class="d-block mb-2">Remaining Stock per Size</label>
                                    </div>
                                    <div class="col">
                                        <div class="form-group">
                                            <label>S</label>
                                            <asp:TextBox ID="txtStockS" runat="server"
                                                Text="0" CssClass="form-control text-center"
                                                ReadOnly="true">
                                            </asp:TextBox>
                                        </div>
                                    </div>
                                    <div class="col">
                                        <div class="form-group">
                                            <label>M</label>
                                            <asp:TextBox ID="txtStockM" runat="server"
                                                Text="0" CssClass="form-control text-center"
                                                ReadOnly="true">
                                            </asp:TextBox>
                                        </div>
                                    </div>
                                    <div class="col">
                                        <div class="form-group">
                                            <label>L</label>
                                            <asp:TextBox ID="txtStockL" runat="server"
                                                Text="0" CssClass="form-control text-center"
                                                ReadOnly="true">
                                            </asp:TextBox>
                                        </div>
                                    </div>
                                    <div class="col">
                                        <div class="form-group">
                                            <label>XL</label>
                                            <asp:TextBox ID="txtStockXL" runat="server"
                                                Text="0" CssClass="form-control text-center"
                                                ReadOnly="true">
                                            </asp:TextBox>
                                        </div>
                                    </div>
                                    <div class="col">
                                        <div class="form-group">
                                            <label>XXL</label>
                                            <asp:TextBox ID="txtStockXXL" runat="server"
                                                Text="0" CssClass="form-control text-center"
                                                ReadOnly="true">
                                            </asp:TextBox>
                                        </div>
                                    </div>
                                </div>

                                <!-- Description (READ-ONLY) -->
                                <div class="form-group">
                                    <label>Description</label>
                                    <asp:TextBox ID="txtDescripcion" runat="server"
                                        placeholder="Description" CssClass="form-control"
                                        TextMode="MultiLine" Rows="3" ReadOnly="true">
                                    </asp:TextBox>
                                </div>

                                <!-- Delete button -->
                                <div class="row mt-4">
                                    <div class="col-12">
                                        <asp:Button ID="btnEliminar" runat="server"
                                            Text="&#xf2ed; Delete Product"
                                            CssClass="mybtn"
                                            OnClick="btnEliminar_Click"
                                            Style="background: linear-gradient(135deg, #ef4444 0%, #dc2626 100%); font-family: 'Raleway', 'Font Awesome 5 Free'; font-weight: 600; padding: 15px;" />
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                    <!-- GridView — Current Inventory -->
                    <div class="row mt-5">
                        <div class="col-12">
                            <h3 class="text-white mb-4" style="font-weight: 600;">Current Inventory</h3>
                            <div class="table-responsive">
                                <asp:GridView ID="gvdlista" runat="server" AutoGenerateColumns="False"
                                    GridLines="None" CssClass="table table-custom text-center">
                                    <Columns>
                                        <asp:BoundField DataField="ID" HeaderText="ID" />
                                        <asp:BoundField DataField="Name" HeaderText="Product Name" />
                                        <asp:BoundField DataField="Brand" HeaderText="Brand" />
                                        <asp:BoundField DataField="League" HeaderText="League" />
                                        <asp:BoundField DataField="Team" HeaderText="Team" />
                                        <asp:BoundField DataField="Year" HeaderText="Year" />
                                        <asp:BoundField DataField="Type" HeaderText="Type" />
                                        <asp:BoundField DataField="Price" HeaderText="Price" DataFormatString="{0:C}" />
                                    </Columns>
                                </asp:GridView>
                            </div>
                        </div>
                    </div>
                </div>


                <asp:Literal ID="alerta" runat="server"></asp:Literal>
            </main>
        </div>
    </form>

    <script src="https://ajax.googleapis.com/ajax/libs/jquery/3.4.1/jquery.min.js"></script>
    <script src="https://cdnjs.cloudflare.com/ajax/libs/popper.js/1.14.7/umd/popper.min.js"></script>
    <script src="https://maxcdn.bootstrapcdn.com/bootstrap/4.3.1/js/bootstrap.min.js"></script>
</body>
</html>

