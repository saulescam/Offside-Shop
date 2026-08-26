<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="404.aspx.cs" Inherits="OFFSIDESHOP.Error404" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1, shrink-to-fit=no" />
    <title><asp:Literal runat="server" Text="<%$ Resources:Strings, Error404_PageTitle %>" /></title>

    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.2.3/dist/css/bootstrap.min.css" rel="stylesheet" />
    <script src="https://use.fontawesome.com/releases/v6.3.0/js/all.js" crossorigin="anonymous"></script>
    <link href="https://fonts.googleapis.com/css?family=Montserrat:400,700,900&display=swap" rel="stylesheet" type="text/css" />

    <style>
        body {
            background-color: #0f172a; /* Fondo oscuro elegante */
            color: #ffffff;
            font-family: 'Montserrat', sans-serif;
            height: 100vh;
            margin: 0;
            display: flex;
            align-items: center;
            justify-content: center;
            text-align: center;
            overflow: hidden;
        }

        .error-container {
            padding: 40px;
            animation: fadeIn 0.8s ease-out;
        }

        .icon-offside {
            font-size: 6rem;
            background: linear-gradient(135deg, #FFC800, #d97706);
            -webkit-background-clip: text;
            -webkit-text-fill-color: transparent;
            margin-bottom: 20px;
            display: inline-block;
        }

        .error-code {
            font-size: 8rem;
            font-weight: 900;
            line-height: 1;
            margin: 0;
            letter-spacing: -2px;
            text-shadow: 4px 4px 0px rgba(0, 0, 0, 0.5);
        }

        .error-title {
            font-size: 2.5rem;
            font-weight: 700;
            margin-top: 10px;
            text-transform: uppercase;
            letter-spacing: 1px;
        }

        .error-text {
            color: #94a3b8;
            font-size: 1.1rem;
            margin-bottom: 35px;
            max-width: 500px;
            margin-left: auto;
            margin-right: auto;
        }

        .btn-home {
            background: linear-gradient(135deg, #FFC800, #d97706);
            color: #111827;
            padding: 12px 30px;
            border-radius: 50px;
            font-weight: 700;
            font-size: 1.1rem;
            text-decoration: none;
            transition: all 0.3s ease;
            display: inline-block;
            border: none;
            box-shadow: 0 10px 20px -5px rgba(245, 158, 11, 0.4);
        }

        .btn-home:hover {
            transform: translateY(-3px);
            box-shadow: 0 15px 25px -5px rgba(245, 158, 11, 0.6);
            color: #000;
        }

        @keyframes fadeIn {
            from { opacity: 0; transform: translateY(20px); }
            to { opacity: 1; transform: translateY(0); }
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="error-container">
            <!-- Icono de banderín de juez de línea -->
            <i class="fas fa-flag icon-offside"></i>
            
            <h1 class="error-code">404</h1>
            <h2 class="error-title"><asp:Literal runat="server" Text="<%$ Resources:Strings, Error404_Heading %>" /></h2>
            <p class="error-text"><asp:Literal runat="server" Text="<%$ Resources:Strings, Error404_Message %>" /></p>
            
           <a href="<%= ResolveUrl("~/Homepage.aspx") %>" class="btn-home">
                <i class="fas fa-home me-2"></i> <asp:Literal runat="server" Text="<%$ Resources:Strings, Error404_ReturnButton %>" />
            </a>
        </div>
    </form>
</body>
</html>