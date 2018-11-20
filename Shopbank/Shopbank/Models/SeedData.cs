using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shopbank.Models
{
    public class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new ShopbankContext(
                serviceProvider.GetRequiredService<DbContextOptions<ShopbankContext>>()))
            {
                // Look for any movies.
                if (context.ShopItem.Count() > 0)
                {
                    return;   // DB has been seeded
                }

                context.ShopItem.AddRange(
                    new ShopItem
                    {
                        Title = "Is Mayo an Instrument?",
                        Description = "This is a lovely meme picture",
                        Url = "https://i.kym-cdn.com/photos/images/original/001/371/723/be6.jpg",
                        Tags = "spongebob",
                        Uploaded = "07-10-18 4:20T18:25:43.511Z",
                        Price = "12$",
                        AccessCode = "J7890",
                        Width = "768",
                        Height = "432"
                    }


                );
                context.SaveChanges();
            }
        }
    }
}
