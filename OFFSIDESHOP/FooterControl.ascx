<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="FooterControl.ascx.cs" Inherits="OFFSIDESHOP.FooterControl" %>
<style>
    .main-footer {
        background-color: #111111; /* Negro profundo */
        color: #ffffff;
        padding: 40px 0 20px 0;
        font-family: 'Raleway', sans-serif;
        border-top: 3px solid #FFC800; /* Línea dorada superior de marca */
        margin-top: 50px;
    }

    .footer-logo img {
        max-height: 50px;
        width: auto;
        transition: transform 0.3s ease;
    }

    .footer-logo img:hover {
        transform: scale(1.05);
    }

    .footer-links {
        list-style: none;
        padding: 0;
        margin: 0;
        display: flex;
        justify-content: center;
        gap: 30px;
        flex-wrap: wrap;
    }

    .footer-links a {
        color: #aaaaaa;
        text-decoration: none;
        font-weight: 500;
        font-size: 14px;
        transition: color 0.3s ease, transform 0.3s ease;
        display: inline-block;
    }

    .footer-links a:hover {
        color: #FFC800; /* Cambio al amarillo oficial */
        text-decoration: none;
        transform: translateY(-2px);
    }

    .footer-social {
        display: flex;
        justify-content: center;
        gap: 20px;
        margin-top: 5px;
    }

    .footer-social a {
        color: #ffffff;
        background-color: #222222;
        width: 38px;
        height: 38px;
        border-radius: 50%;
        display: flex;
        align-items: center;
        justify-content: center;
        text-decoration: none;
        transition: all 0.3s ease;
        border: 1px solid #333333;
    }

    .footer-social a:hover {
        background-color: #FFC800;
        color: #111111;
        border-color: #FFC800;
        transform: translateY(-3px);
    }

    .footer-bottom {
        border-top: 1px solid #222222;
        margin-top: 25px;
        padding-top: 20px;
        font-size: 13px;
        color: #666666;
    }

    .footer-bottom strong {
        color: #FFC800;
    }
    /* Aplica esto a la etiqueta negra principal dentro de tu Footer.ascx */
.footer-black-bar { 
    width: 100vw !important;      /* Fuerza a medir el 100% de la pantalla real */
    position: relative;
    left: 50%;
    right: 50%;
    margin-left: -50vw !important;  /* Centra el elemento rompiendo los márgenes del padre */
    margin-right: -50vw !important;
    box-sizing: border-box;
}
</style>

<footer class="main-footer footer-black-bar">
    <div class="container text-center">
        <div class="row align-items-center gy-4">
            
            <div class="col-md-4 text-md-start text-center footer-logo">
                <a href="Homepage.aspx">
                    <img src="assets/img/offsideshop_logo_white_letras.png" alt="OFFSIDESHOP Logo" />
                </a>
            </div>

            <div class="col-md-5 text-center">
                <ul class="footer-links">
                    <li><a href="Homepage.aspx"><asp:Literal runat="server" Text="<%$ Resources:Strings, Nav_Home %>" /></a></li>
                    <li><a href="Homepage.aspx#collections-section"><asp:Literal runat="server" Text="<%$ Resources:Strings, Nav_Collections %>" /></a></li>
                    <li><a href="AboutUs.aspx"><asp:Literal runat="server" Text="<%$ Resources:Strings, Nav_AboutUs %>" /></a></li>
                    <li><a href="ContactSupport.aspx"><asp:Literal runat="server" Text="<%$ Resources:Strings, Nav_Contact %>" /></a></li>
                </ul>
            </div>

            

        </div>

        
    </div>
</footer>