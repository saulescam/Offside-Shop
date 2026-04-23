<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Homepage.aspx.cs" Inherits="OFFSIDESHOP.Homepage" %>

<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1, shrink-to-fit=no" />
    <meta name="description" content="" />
    <meta name="author" content="" />
    <title>OffsideShop</title>
    
    <!-- Favicon-->
    <link rel="icon" type="image/x-icon" href="assets/favicon.ico" />
    
    <!-- Font Awesome icons (free version)-->
    <script src="https://use.fontawesome.com/releases/v6.3.0/js/all.js" crossorigin="anonymous"></script>
    
    <!-- Google fonts-->
    <link href="https://fonts.googleapis.com/css?family=Montserrat:400,700" rel="stylesheet" type="text/css" />
    <link href="https://fonts.googleapis.com/css?family=Roboto+Slab:400,100,300,700" rel="stylesheet" type="text/css" />
    
    <!-- Bootstrap CSS -->
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.2.3/dist/css/bootstrap.min.css" rel="stylesheet" />
    
    <!-- Core theme CSS (includes Bootstrap)-->
    <link href="css/styles.css" rel="stylesheet" />
    
    <style>
        /* ============================================
           ESTILOS CON FONDO NEGRO TENUE
        ============================================ */
        
        * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }
        
        body {
            font-family: 'Montserrat', sans-serif;
            background: #0a0a0a;
            min-height: 100vh;
        }
        
        /* Fondo negro tenue general */
        .bg-dark-custom {
            background: #0a0a0a;
        }
        
        /* Estilos para la navbar */
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
            color: #ffffff;
            border: 2px solid #dc2626;
            background: linear-gradient(135deg, #dc2626 0%, #b91c1c 100%);
            border-radius: 25px;
            padding: 8px 24px;
            font-weight: 600;
            transition: all 0.3s ease;
        }
        
        .btn-outline-warning:hover {
            background: linear-gradient(135deg, #ef4444 0%, #dc2626 100%);
            border-color: #ef4444;
            transform: translateY(-2px);
            box-shadow: 0 5px 15px rgba(220, 38, 38, 0.3);
        }
        
        /* Masthead (header) - Fondo negro tenue */
        .masthead {
            background: #000000;
            padding: 100px 0 40px 0;
            text-align: center;
            border-bottom: 1px solid rgba(255, 255, 255, 0.05);
        }
        
        .masthead-subheading {
            font-size: 1.5rem;
            color: #888888;
            margin-bottom: 10px;
        }
        
        .masthead-heading {
            font-size: 3rem;
            font-weight: 700;
            color: #ffffff;
            letter-spacing: 2px;
            text-shadow: 0 2px 4px rgba(0, 0, 0, 0.5);
        }
        
        @media (max-width: 768px) {
            .masthead-heading {
                font-size: 2rem;
            }
            .masthead-subheading {
                font-size: 1rem;
            }
        }
        
        /* Contenedor del carrusel */
        .carousel-container {
            display: flex;
            justify-content: center;
            align-items: center;
            min-height: calc(100vh - 250px);
            padding: 60px 0;
            background: #0a0a0a;
        }
        
        .carousel {
            max-width: 1200px;
            width: 90%;
            margin: 0 auto;
            border-radius: 20px;
            overflow: hidden;
            box-shadow: 0 20px 40px rgba(0, 0, 0, 0.6);
        }
        
        /* Make the image fully responsive */
        .carousel-inner img {
            width: 100%;
            height: 500px;
            object-fit: cover;
        }
        
        /* Ajustes responsive */
        @media (max-width: 768px) {
            .carousel-inner img {
                height: 300px;
            }
            .carousel {
                width: 95%;
            }
            .carousel-container {
                min-height: calc(100vh - 200px);
                padding: 40px 0;
            }
        }
        
        @media (max-width: 480px) {
            .carousel-inner img {
                height: 200px;
            }
        }
        
        /* Indicadores del carrusel */
        .carousel-indicators {
            bottom: 20px;
        }
        
        .carousel-indicators button {
            width: 12px !important;
            height: 12px !important;
            border-radius: 50%;
            background-color: #ffffff !important;
            opacity: 0.5;
            transition: all 0.3s ease;
            margin: 0 5px;
        }
        
        .carousel-indicators button.active {
            opacity: 1;
            background-color: #dc2626 !important;
            transform: scale(1.2);
        }
        
        /* Controles del carrusel */
        .carousel-control-prev-icon,
        .carousel-control-next-icon {
            background-color: rgba(0, 0, 0, 0.6);
            border-radius: 50%;
            padding: 20px;
            background-size: 50%;
            transition: all 0.3s ease;
        }
        
        .carousel-control-prev-icon:hover,
        .carousel-control-next-icon:hover {
            background-color: rgba(220, 38, 38, 0.8);
        }
        
        /* Efecto de zoom en las imágenes */
        .carousel-item img {
            transition: transform 0.5s ease;
        }
        
        .carousel-item:hover img {
            transform: scale(1.05);
        }
        
        /* Scrollbar personalizada */
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
            background: #dc2626;
        }
        
        /* Animación de entrada */
        @keyframes fadeInUp {
            from {
                opacity: 0;
                transform: translateY(30px);
            }
            to {
                opacity: 1;
                transform: translateY(0);
            }
        }
        
        .carousel-container {
            animation: fadeInUp 0.8s ease-out;
        }
        
        /* Texto de bienvenida con efecto */
        .welcome-text {
            text-align: center;
            color: #888888;
            margin-top: 30px;
            font-size: 0.9rem;
        }
    </style>
</head>
<body id="page-top">
    <form runat="server">
        <!-- Navigation-->
        <nav class="navbar navbar-expand-lg navbar-dark fixed-top" id="mainNav">
            <div class="container">
                <a class="navbar-brand" href="#page-top">
                    <img src="assets/img/offsideshop_logo_white_letras.png" alt="OffsideShop Logo" />
                </a>
                <button class="navbar-toggler" type="button" data-bs-toggle="collapse" data-bs-target="#navbarResponsive">
                    <span class="navbar-toggler-icon"></span>
                </button>
                <div class="collapse navbar-collapse" id="navbarResponsive">
                    <ul class="navbar-nav ms-auto">
                        <li class="nav-item">
                            <asp:Button ID="btncerrar" class="btn btn-outline-warning" runat="server" Text="Log out" OnClick="btncerrar_Click" />
                        </li>
                    </ul>
                </div>
            </div>
        </nav>
        
        <!-- Masthead (Header) -->
        <header class="masthead">
            <div class="container">
                <div class="masthead-subheading">WELCOME, USER</div>
                <div class="masthead-heading text-uppercase">OFFSIDE SHOP</div>
            </div>
        </header>
        
        <!-- Carrusel Centrado con las nuevas imágenes -->
        <div class="carousel-container">
            <div id="demo" class="carousel slide" data-bs-ride="carousel">
                <!-- Indicators -->
                <div class="carousel-indicators">
                    <button type="button" data-bs-target="#demo" data-bs-slide-to="0" class="active"></button>
                    <button type="button" data-bs-target="#demo" data-bs-slide-to="1"></button>
                    <button type="button" data-bs-target="#demo" data-bs-slide-to="2"></button>
                    <button type="button" data-bs-target="#demo" data-bs-slide-to="3"></button>
                    <button type="button" data-bs-target="#demo" data-bs-slide-to="4"></button>
                    <button type="button" data-bs-target="#demo" data-bs-slide-to="5"></button>
                </div>
                
                <!-- The slideshow -->
                <div class="carousel-inner">
                    <div class="carousel-item active">
                        <img src="assets/img/1.jpg" alt="Producto 1" />
                    </div>
                    <div class="carousel-item">
                        <img src="assets/img/2.jpg" alt="Producto 2" />
                    </div>
                    <div class="carousel-item">
                        <img src="assets/img/3.jpg" alt="Producto 3" />
                    </div>
                    <div class="carousel-item">
                        <img src="assets/img/4.jpg" alt="Producto 4" />
                    </div>
                    <div class="carousel-item">
                        <img src="assets/img/5.jpg" alt="Producto 5" />
                    </div>
                    <div class="carousel-item">
                        <img src="assets/img/6.jpg" alt="Producto 6" />
                    </div>
                </div>
                
                <!-- Left and right controls -->
                <button class="carousel-control-prev" type="button" data-bs-target="#demo" data-bs-slide="prev">
                    <span class="carousel-control-prev-icon" aria-hidden="true"></span>
                    <span class="visually-hidden">Previous</span>
                </button>
                <button class="carousel-control-next" type="button" data-bs-target="#demo" data-bs-slide="next">
                    <span class="carousel-control-next-icon" aria-hidden="true"></span>
                    <span class="visually-hidden">Next</span>
                </button>
            </div>
        </div>
        
        <div class="welcome-text">
            <p>Amazing things coming soon</p>
        </div>
        
        <asp:Label ID="label1" runat="server" Text=""></asp:Label>
        <asp:Label ID="label2" runat="server" Text=""></asp:Label>
    </form>
    
    <!-- Scripts -->
    <script src="https://ajax.googleapis.com/ajax/libs/jquery/3.6.0/jquery.min.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.2.3/dist/js/bootstrap.bundle.min.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
</body>
</html>
