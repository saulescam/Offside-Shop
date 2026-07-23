<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="AdminStats.aspx.cs" Inherits="OFFSIDESHOP.AdminStats" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1, shrink-to-fit=no" />
    <title>Store Statistics | OffsideShop</title>

    <link href="css/bootstrap.min.css" rel="stylesheet" />
    <link href="https://fonts.googleapis.com/css?family=Raleway:100,400,600,700&display=swap" rel="stylesheet" />
    <link rel="stylesheet" href="https://maxcdn.bootstrapcdn.com/bootstrap/4.3.1/css/bootstrap.min.css" />
    <link href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/5.15.4/css/all.min.css" rel="stylesheet" />
    <link href="css/admin-layout.css" rel="stylesheet" />

    <script type="text/javascript">
        (function () {
            var theme = localStorage.getItem('theme') || 'light';
            if (theme === 'dark') {
                document.documentElement.classList.add('dark-mode');
                var observer = new MutationObserver(function (mutations, obs) {
                    if (document.body) {
                        document.body.classList.add('dark-mode');
                        obs.disconnect();
                    }
                });
                observer.observe(document.documentElement, { childList: true, subtree: true });
            }
        })();
    </script>

    <script src="SweetAlert/sweetalert2.all.min.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/chart.js"></script>

    <style>
        .kpi-card {
            background: var(--card-bg);
            border: 1px solid var(--border-color);
            border-radius: 14px;
            padding: 24px;
            margin-bottom: 24px;
            box-shadow: 0 6px 15px rgba(0,0,0,0.2);
            display: flex;
            align-items: center;
            justify-content: space-between;
            transition: transform 0.3s ease;
        }
        .kpi-card:hover { transform: translateY(-5px); }
        .kpi-details h6 {
            color: var(--text-muted);
            font-size: 0.85rem;
            text-transform: uppercase;
            font-weight: 700;
            letter-spacing: 1px;
            margin-bottom: 8px;
        }
        .kpi-details h2 {
            color: #e5e7eb;
            font-size: 1.8rem;
            font-weight: 800;
            margin: 0;
        }
        html:not(.dark-mode) .kpi-details h2 { color: #111827; }
        .kpi-icon {
            width: 55px;
            height: 55px;
            border-radius: 12px;
            display: flex;
            align-items: center;
            justify-content: center;
            font-size: 1.6rem;
            color: #fff;
        }
        .bg-revenue { background: linear-gradient(135deg, #10b981, #059669); }
        .bg-orders { background: linear-gradient(135deg, #3b82f6, #2563eb); }
        .bg-users { background: linear-gradient(135deg, #8b5cf6, #6d28d9); }
        .bg-pending { background: linear-gradient(135deg, #f59e0b, #d97706); }

        .chart-container {
            background: var(--card-bg);
            border: 1px solid var(--border-color);
            border-radius: 14px;
            padding: 20px;
            margin-bottom: 24px;
            box-shadow: 0 6px 15px rgba(0,0,0,0.2);
            position: relative;
            height: 350px;
        }
        .chart-title {
            color: var(--text-muted);
            font-weight: 700;
            font-size: 1.1rem;
            margin-bottom: 15px;
            text-transform: uppercase;
            letter-spacing: 0.5px;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <!-- CAMPOS OCULTOS A PRUEBA DE ERRORES PARA PASAR DATA A JAVASCRIPT -->
        <asp:HiddenField ID="hfRevenueDates" runat="server" Value="[]" />
        <asp:HiddenField ID="hfRevenueData" runat="server" Value="[]" />
        <asp:HiddenField ID="hfStatusLabels" runat="server" Value="[]" />
        <asp:HiddenField ID="hfStatusData" runat="server" Value="[]" />
        <asp:HiddenField ID="hfTopProductsLabels" runat="server" Value="[]" />
        <asp:HiddenField ID="hfTopProductsData" runat="server" Value="[]" />

        <nav class="top-navbar">
            <div style="display: flex; align-items: center;">
                <a class="navbar-brand" href="Dashboard.aspx">
                    <img src="assets/img/offsideshop_logo_white_letras.png" alt="OFFSIDESHOP" />
                </a>
            </div>
            <button type="button" id="theme-toggle" class="theme-toggle-btn" title="Toggle Light/Dark Theme">
                <i class="fas fa-moon"></i>
            </button>
        </nav>

        <div class="layout-wrapper">
            <aside class="sidebar fade-in">
                <ul class="sidebar-menu">
                    <li>
                        <asp:Button ID="btnManageProducts" CssClass="sidebar-btn" runat="server" Text="&#xf553; Manage Products" OnClick="btnManageProducts_Click" Style="font-family: 'Raleway', 'Font Awesome 5 Free'; font-weight: 600;" />
                    </li>
                    <li>
                        <a id="btnManageOrders" runat="server" href="ManageOrders.aspx" class="sidebar-btn " style="font-family: 'Raleway','Font Awesome 5 Free'; font-weight: 600;">&#xf46d; Manage Orders</a>
                    </li>
                    <li>
                        <asp:Button ID="btnManageOffers" CssClass="sidebar-btn" runat="server" Text="&#xf155; Manage Offers" OnClick="btnManageOffers_Click" Style="font-family: 'Raleway', 'Font Awesome 5 Free'; font-weight: 600;" />
                    </li>
                    <li>
                        <asp:Button ID="btnManageCoupons" CssClass="sidebar-btn" runat="server" Text="&#xf02c; Manage Coupons" OnClick="btnManageCoupons_Click" Style="font-family: 'Raleway', 'Font Awesome 5 Free'; font-weight: 600;" /></li>
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

                    <asp:PlaceHolder ID="phOwnerMenu" runat="server">
                        <li style="border-top: 1px solid var(--border-color); margin-top: 8px; padding-top: 8px;">
                            <asp:Button ID="btnManageUsers" CssClass="sidebar-btn" runat="server" Text="&#xf4fe; Manage Users" OnClick="btnManageUsers_Click" Style="font-family: 'Raleway', 'Font Awesome 5 Free'; font-weight: 600;" />
                        </li>
                        <li>
                            <asp:Button ID="btnSmtpSettings" CssClass="sidebar-btn" runat="server" Text="&#xf0e0; SMTP Settings" OnClick="btnSmtpSettings_Click" Style="font-family: 'Raleway', 'Font Awesome 5 Free'; font-weight: 600;" />
                        </li>
                        <li>
                            <asp:Button ID="btnStats" CssClass="sidebar-btn active" runat="server" Text="&#xf080; Stats" OnClick="btnStats_Click" Style="font-family: 'Raleway', 'Font Awesome 5 Free'; font-weight: 600;" />
                        </li>
                        <li>
                            <asp:Button ID="btnAuditLogs" CssClass="sidebar-btn" runat="server" Text="&#xf03a; Audit Logs" OnClick="btnAuditLogs_Click" Style="font-family: 'Raleway', 'Font Awesome 5 Free'; font-weight: 600;" />
                        </li>
                    </asp:PlaceHolder>

                    <li>
                        <asp:Button ID="btnAdminBanners" CssClass="sidebar-btn" runat="server" Text="&#xf03e; Manage Banners" OnClick="btnAdminBanners_Click" Style="font-family: 'Raleway', 'Font Awesome 5 Free'; font-weight: 600;" />
                    </li>

                    <li style="border-top: 1px solid var(--border-color); margin-top: 8px; padding-top: 8px;">
                        <asp:Button ID="btncerrar" CssClass="sidebar-btn btn-logout" runat="server" Text="&#xf2f5; Logout" OnClick="btncerrar_Click" Style="font-family: 'Raleway', 'Font Awesome 5 Free'; font-weight: 600;" />
                    </li>
                </ul>
            </aside>

            <main class="main-content fade-in" style="animation-delay: 0.2s;">
                <div class="container-fluid">
                    
                    <!-- HEADER SECTION WITH EXPORT BUTTON -->
                    <div class="d-flex justify-content-between align-items-center mb-4 flex-wrap gap-3">
                        <div>
                            <h1 class="page-title mb-1">Store Statistics</h1>
                            <p class="text-muted mb-0">Detailed analytics and real-time performance overview of OffsideShop.</p>
                        </div>
                        
                        <div class="d-flex align-items-center" style="gap: 15px;">
                            <div class="d-flex align-items-center bg-white p-2 rounded shadow-sm border">
                                <i class="fas fa-file-pdf text-danger mx-2" style="font-size: 1.3rem;"></i>
                                <asp:DropDownList ID="ddlReportPeriod" runat="server" CssClass="form-control border-0 bg-transparent fw-bold" style="width: 150px; cursor: pointer; outline: none; box-shadow: none;">
                                    <asp:ListItem Value="WEEK" Text="Weekly Report"></asp:ListItem>
                                    <asp:ListItem Value="MONTH" Text="Monthly Report"></asp:ListItem>
                                    <asp:ListItem Value="YEAR" Text="Annual Report"></asp:ListItem>
                                </asp:DropDownList>
                                <asp:LinkButton ID="btnExportPdf" runat="server" CssClass="btn btn-dark text-warning fw-bold px-4 rounded-pill ml-2" OnClick="btnExportPdf_Click" OnClientClick="showPdfLoader();">
                                    Export Deep Report
                                </asp:LinkButton>
                            </div>
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-xl-3 col-lg-6 col-md-6">
                            <div class="kpi-card">
                                <div class="kpi-details">
                                    <h6>Gross Revenue</h6>
                                    <h2><asp:Label ID="lblTotalRevenue" runat="server" Text="$0.00"></asp:Label></h2>
                                </div>
                                <div class="kpi-icon bg-revenue"><i class="fas fa-dollar-sign"></i></div>
                            </div>
                        </div>
                        <div class="col-xl-3 col-lg-6 col-md-6">
                            <div class="kpi-card">
                                <div class="kpi-details">
                                    <h6>Total Orders</h6>
                                    <h2><asp:Label ID="lblTotalOrders" runat="server" Text="0"></asp:Label></h2>
                                </div>
                                <div class="kpi-icon bg-orders"><i class="fas fa-shopping-bag"></i></div>
                            </div>
                        </div>
                        <div class="col-xl-3 col-lg-6 col-md-6">
                            <div class="kpi-card">
                                <div class="kpi-details">
                                    <h6>Registered Users</h6>
                                    <h2><asp:Label ID="lblTotalUsers" runat="server" Text="0"></asp:Label></h2>
                                </div>
                                <div class="kpi-icon bg-users"><i class="fas fa-users"></i></div>
                            </div>
                        </div>
                        <div class="col-xl-3 col-lg-6 col-md-6">
                            <div class="kpi-card">
                                <div class="kpi-details">
                                    <h6>Pending Orders</h6>
                                    <h2><asp:Label ID="lblPendingOrders" runat="server" Text="0"></asp:Label></h2>
                                </div>
                                <div class="kpi-icon bg-pending"><i class="fas fa-clock"></i></div>
                            </div>
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-lg-8">
                            <div class="chart-container">
                                <div class="chart-title"><i class="fas fa-chart-line mr-2"></i> Sales Revenue (Last 7 Days)</div>
                                <canvas id="revenueChart"></canvas>
                            </div>
                        </div>
                        <div class="col-lg-4">
                            <div class="chart-container">
                                <div class="chart-title"><i class="fas fa-chart-pie mr-2"></i> Order Status Distribution</div>
                                <canvas id="statusChart"></canvas>
                            </div>
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-lg-12">
                            <div class="chart-container">
                                <div class="chart-title"><i class="fas fa-trophy mr-2"></i> Top 5 Best Selling Products</div>
                                <canvas id="productsChart"></canvas>
                            </div>
                        </div>
                    </div>

                    <asp:Literal ID="alerta" runat="server" Text="" EnableViewState="false"></asp:Literal>
                </div>
            </main>
        </div>
    </form>

    <script src="https://ajax.googleapis.com/ajax/libs/jquery/3.4.1/jquery.min.js"></script>
    <script src="https://cdnjs.cloudflare.com/ajax/libs/popper.js/1.14.7/umd/popper.min.js"></script>
    <script src="https://maxcdn.bootstrapcdn.com/bootstrap/4.3.1/js/bootstrap.min.js"></script>

    <!-- Theme Toggle & Loader -->
    <script type="text/javascript">
        function showPdfLoader() {
            Swal.fire({
                title: 'Generating Report...',
                text: 'Crunching sales data and drawing charts. This might take a few seconds.',
                allowOutsideClick: false,
                didOpen: () => {
                    Swal.showLoading()
                }
            });
            // El backend descargará el archivo, por lo que cerramos el loader tras unos segundos
            setTimeout(() => { Swal.close(); }, 7000);
        }

        document.addEventListener('DOMContentLoaded', function () {
            var themeToggle = document.getElementById('theme-toggle');
            if (themeToggle) {
                var themeIcon = themeToggle.querySelector('i');
                var isDark = document.body.classList.contains('dark-mode') || document.documentElement.classList.contains('dark-mode');
                if (isDark && themeIcon) themeIcon.className = 'fas fa-sun';

                themeToggle.addEventListener('click', function (e) {
                    e.preventDefault();
                    var currentlyDark = document.body.classList.contains('dark-mode') || document.documentElement.classList.contains('dark-mode');
                    if (currentlyDark) {
                        document.body.classList.remove('dark-mode');
                        document.documentElement.classList.remove('dark-mode');
                        localStorage.setItem('theme', 'light');
                        if (themeIcon) themeIcon.className = 'fas fa-moon';
                        Chart.defaults.color = '#6c757d';
                    } else {
                        document.body.classList.add('dark-mode');
                        document.documentElement.classList.add('dark-mode');
                        localStorage.setItem('theme', 'dark');
                        if (themeIcon) themeIcon.className = 'fas fa-sun';
                        Chart.defaults.color = '#9ca3af';
                    }
                    if (window.revenueChart) window.revenueChart.update();
                    if (window.statusChart) window.statusChart.update();
                    if (window.productsChart) window.productsChart.update();
                });
            }
        });
    </script>

    <!-- Renderización de Gráficos (Extrayendo de los HiddenFields) -->
    <script type="text/javascript">
        document.addEventListener('DOMContentLoaded', function () {

            const isDarkMode = document.documentElement.classList.contains('dark-mode');
            Chart.defaults.color = isDarkMode ? '#9ca3af' : '#6c757d';
            Chart.defaults.font.family = "'Raleway', sans-serif";

            const revDates = JSON.parse(document.getElementById('<%= hfRevenueDates.ClientID %>').value || "[]");
            const revData = JSON.parse(document.getElementById('<%= hfRevenueData.ClientID %>').value || "[]");
            const statLabels = JSON.parse(document.getElementById('<%= hfStatusLabels.ClientID %>').value || "[]");
            const statData = JSON.parse(document.getElementById('<%= hfStatusData.ClientID %>').value || "[]");
            const topLabels = JSON.parse(document.getElementById('<%= hfTopProductsLabels.ClientID %>').value || "[]");
            const topData = JSON.parse(document.getElementById('<%= hfTopProductsData.ClientID %>').value || "[]");

            const ctxRevenue = document.getElementById('revenueChart').getContext('2d');
            window.revenueChart = new Chart(ctxRevenue, {
                type: 'line',
                data: {
                    labels: revDates,
                    datasets: [{
                        label: 'Revenue ($)',
                        data: revData,
                        borderColor: '#3b82f6',
                        backgroundColor: 'rgba(59, 130, 246, 0.2)',
                        borderWidth: 3,
                        pointBackgroundColor: '#fff',
                        pointBorderColor: '#3b82f6',
                        pointRadius: 4,
                        fill: true,
                        tension: 0.3
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    plugins: { legend: { display: false } },
                    scales: { y: { beginAtZero: true } }
                }
            });

            const ctxStatus = document.getElementById('statusChart').getContext('2d');
            window.statusChart = new Chart(ctxStatus, {
                type: 'doughnut',
                data: {
                    labels: statLabels,
                    datasets: [{
                        data: statData,
                        backgroundColor: ['#f59e0b', '#10b981', '#3b82f6', '#8b5cf6', '#ef4444', '#6b7280', '#ec4899', '#f43f5e'],
                        borderWidth: 0
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    cutout: '70%',
                    plugins: { legend: { position: 'right' } }
                }
            });

            const ctxProducts = document.getElementById('productsChart').getContext('2d');
            window.productsChart = new Chart(ctxProducts, {
                type: 'bar',
                data: {
                    labels: topLabels,
                    datasets: [{
                        label: 'Units Sold',
                        data: topData,
                        backgroundColor: '#d4af37',
                        borderRadius: 6
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    plugins: { legend: { display: false } },
                    scales: { y: { beginAtZero: true, ticks: { precision: 0 } } }
                }
            });
        });
    </script>
</body>
</html>