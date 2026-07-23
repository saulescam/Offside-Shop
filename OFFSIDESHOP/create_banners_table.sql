-- ============================================================
--  Tabla Banners para OffsideShop
--  Ejecuta este script en tu base de datos MySQL 'offsideshop'
-- ============================================================

CREATE TABLE IF NOT EXISTS Banners (
    ID         INT          NOT NULL AUTO_INCREMENT,
    Title      VARCHAR(200) NOT NULL,
    Subtitle   VARCHAR(300) NULL,
    ImageURL   VARCHAR(500) NOT NULL,
    LinkURL    VARCHAR(500) NULL,
    SortOrder  INT          NOT NULL DEFAULT 0,
    IsActive   TINYINT(1)   NOT NULL DEFAULT 1,
    PRIMARY KEY (ID)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- ============================================================
--  Datos de ejemplo para arrancar el carrusel
--  (opcional – puedes borrar estas filas desde AdminBanners.aspx)
-- ============================================================

INSERT INTO Banners (Title, Subtitle, ImageURL, LinkURL, SortOrder, IsActive) VALUES
('Exclusive Collections',  'The highest quality jerseys from the world''s top clubs.',  'https://s2.sportstatics.com/relevo/www/multimedia/202407/21/media/cortadas/WEB-Camisetas_1374X916_20240721165323-R3QrneXpziNk5R9qqWQAqhK-1200x648@Relevo.png', NULL, 1, 1),
('OLIVIA RODRIGO × FC BARCELONA', 'An extraordinary collaboration.',                    'https://estaticos-cdn.prensaiberica.es/clip/0d049b74-91d0-4761-accc-bd3b5204bbad_alta-libre-aspect-ratio_default_0_x640y266.jpg',                              NULL, 2, 1),
('World Cup Heritage',     'Relive the magic with our authentic national team jerseys.', 'https://a.espncdn.com/photo/2025/1105/r1571189_920x518_16-9.jpg',                                                                                              NULL, 3, 1),
('Retro Classics',         'Timeless designs that never go out of style.',               'https://camisetasfutbolbaloncesto.com/cdn/shop/collections/retro_selecciones.jpg?v=1677160375',                                                                  NULL, 4, 1),
('Limited Edition',        'Unique pieces for true collectors and fans.',                'https://objetos-xlk.estaticos-marca.com/uploads/2025/11/19/691d8a53dcecb.jpeg',                                                                                  NULL, 5, 1),
('Fan Favorites',          'Join thousands of fans wearing Offside Shop gear.',          'assets/img/6.jpg',                                                                                                                                               NULL, 6, 1);
