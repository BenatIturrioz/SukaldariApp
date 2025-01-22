using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SukaldariApp.Dominio
{
    internal class ProduktuEskaera
    {
        public virtual int Id { get; set; }
        public virtual int Eskaera_id { get; set; }
        public virtual int ProduktuaIzena { get; set; }
        public virtual int ProduktuaKop { get; set; }
        public virtual float Prezioa { get; set; }
    }
}
