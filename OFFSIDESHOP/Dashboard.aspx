<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Dashboard.aspx.cs" Inherits="OFFSIDESHOP.Dashboard" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <link href="css/bootstrap.min.css" rel="stylesheet" />
    <link href="https://fonts.googleapis.com/css?family=Raleway:100,400&display=swap" rel="stylesheet" />
    <link rel="stylesheet" href="https://maxcdn.bootstrapcdn.com/bootstrap/4.3.1/css/bootstrap.min.css" />
    <link href="EstilosCss/EstiloInicio.css" rel="stylesheet" />
    <title>Dashboard</title>
    
    <style>
        /* Estilos para centrar el carrusel */
        body {
            background: #0a0a0a;
            min-height: 100vh;
        }
        
        /* Contenedor principal para centrar */
        .carousel-wrapper {
            display: flex;
            justify-content: center;
            align-items: center;
            min-height: calc(100vh - 80px);
            padding: 40px 20px;
            background: #0a0a0a;
        }
        
        /* Carrusel centrado */
        .carousel {
            max-width: 1100px;
            width: 100%;
            margin: 0 auto;
            border-radius: 20px;
            overflow: hidden;
            box-shadow: 0 20px 40px rgba(0, 0, 0, 0.5);
        }
        
        /* Imágenes del carrusel */
        .carousel-inner {
            width: 100%;
        }
        
        .carousel-inner img {
            width: 100%;
            height: 500px;
            object-fit: cover;
        }
        
        /* Indicadores del carrusel */
        .carousel-indicators {
            bottom: 20px;
        }
        
        .carousel-indicators li {
            width: 12px;
            height: 12px;
            border-radius: 50%;
            background-color: #ffffff;
            opacity: 0.5;
            transition: all 0.3s ease;
            margin: 0 5px;
        }
        
        .carousel-indicators li.active {
            opacity: 1;
            background-color: #dc2626;
            transform: scale(1.2);
        }
        
        /* Controles del carrusel */
        .carousel-control-prev,
        .carousel-control-next {
            width: 10%;
            opacity: 0;
            transition: opacity 0.3s ease;
        }
        
        .carousel:hover .carousel-control-prev,
        .carousel:hover .carousel-control-next {
            opacity: 1;
        }
        
        .carousel-control-prev-icon,
        .carousel-control-next-icon {
            background-color: rgba(0, 0, 0, 0.5);
            border-radius: 50%;
            padding: 20px;
            background-size: 50%;
        }
        
        .carousel-control-prev-icon:hover,
        .carousel-control-next-icon:hover {
            background-color: rgba(220, 38, 38, 0.8);
        }
        
        /* Efecto hover en imágenes */
        .carousel-item img {
            transition: transform 0.5s ease;
        }
        
        .carousel-item:hover img {
            transform: scale(1.05);
        }
        
        /* Responsive */
        @media (max-width: 768px) {
            .carousel-inner img {
                height: 300px;
            }
            
            .carousel-wrapper {
                min-height: calc(100vh - 70px);
                padding: 20px;
            }
        }
        
        @media (max-width: 480px) {
            .carousel-inner img {
                height: 200px;
            }
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
        
        .carousel {
            animation: fadeInUp 0.8s ease-out;
        }
    </style>
</head>
<body>
    <nav class="navbar navbar-expand-sm bg-dark navbar-dark fixed-top">
        <div class="container">
            <a class="navbar-brand" href="Inicio.aspx">
                <img src="assets/img/offsideshop_logo_white_letras.png" alt="OFFSIDESHOP Logo" class="img-fluid" style="max-height: 45px;" />
            </a>
            <form runat="server">
                <asp:Button ID="btningresar" class="btn btn-outline-success my-2 my-sm-0" type="submit"
                    runat="server" Text="Add" OnClick="btningresar_Click" />
                
            <asp:Button ID="btneliminar" class="btn btn-outline-success my-2 my-sm-0" type="submit"
                runat="server" Text="Delete" OnClick="btneliminar_Click" />

            <asp:Button ID="btneditar" class="btn btn-outline-success my-2 my-sm-0" type="submit"
                runat="server" Text="Edit" OnClick="btneditar_Click" />

                <asp:Button ID="btncerrar" class="btn btn-outline-warning my-2 my-sm-0" type="submit"
                    runat="server" Text="logout" OnClick="btncerrar_Click" />
            </form>
        </div>
    </nav>
    
    <!-- Carrusel centrado -->
    <div class="carousel-wrapper">
        <div id="demo" class="carousel slide" data-ride="carousel">
            <!-- Indicators -->
            <ul class="carousel-indicators">
                <li data-target="#demo" data-slide-to="0" class="active"></li>
                <li data-target="#demo" data-slide-to="1"></li>
                <li data-target="#demo" data-slide-to="2"></li>
                <li data-target="#demo" data-slide-to="3"></li>
                <li data-target="#demo" data-slide-to="4"></li>
            </ul>
            
            <!-- The slideshow -->
            <div class="carousel-inner">
                <div class="carousel-item active">
                    <img src="assets/img/carousel/barcelonaretro.jpg" alt="Barcelona Retro" />
                </div>
                <div class="carousel-item">
                    <img src="assets/img/carousel/camisetas.jpg" alt="Camisetas" />
                </div>
                <div class="carousel-item">
                    <img src="assets/img/carousel/fascamiseta.jpg" alt="Fascamiseta" />
                </div>
                <div class="carousel-item">
                    <img src="assets/img/carousel/rivercamisetas.png" alt="River Camisetas" />
                </div>
                <div class="carousel-item">
                    <img src="assets/img/carousel/selectacamiseta.jpg" alt="Selección Camisetas" />
                </div>
            </div>
            
            <!-- Left and right controls -->
            <a class="carousel-control-prev" href="#demo" data-slide="prev">
                <span class="carousel-control-prev-icon"></span>
            </a>
            <a class="carousel-control-next" href="#demo" data-slide="next">
                <span class="carousel-control-next-icon"></span>
            </a>
        </div>
    </div>
    
    <asp:Label ID="label1" runat="server" Text=""></asp:Label>
    <asp:Label ID="label2" runat="server" Text=""></asp:Label>
    
    <script src="https://ajax.googleapis.com/ajax/libs/jquery/3.4.1/jquery.min.js"></script>
    <script src="https://cdnjs.cloudflare.com/ajax/libs/popper.js/1.14.7/umd/popper.min.js"></script>
    <script src="https://maxcdn.bootstrapcdn.com/bootstrap/4.3.1/js/bootstrap.min.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@8"></script>
</body>
</html>
