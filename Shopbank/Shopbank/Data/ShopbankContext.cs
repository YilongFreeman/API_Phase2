using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Shopbank.Models
{
    public class ShopbankContext : DbContext
    {
        public ShopbankContext (DbContextOptions<ShopbankContext> options)
            : base(options)
        {
        }

        public DbSet<Shopbank.Models.ShopItem> ShopItem { get; set; }
    }
}
