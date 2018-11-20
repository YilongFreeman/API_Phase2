using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shopbank.Models
{
    public class ShopImageItem
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Tags { get; set; }
        public IFormFile Image { get; set; }
    }
}
