using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OFFSIDESHOP
{
    public class Add
    {
public int Id { get; set; }
        public string Marca { get; set; }
        public string Producto { get; set; }
        public string Precio { get; set; }
        public string Cantidad { get; set; }
        public Add() { }
        public Add(string pMarca, string pProducto, string pPrecio, string pCanidad, int PId)
        {
            this.Id = PId;
            this.Marca = pMarca;
            this.Producto = pProducto;
            this.Precio = pPrecio;
            this.Cantidad = pCanidad;
        }
    }
}
