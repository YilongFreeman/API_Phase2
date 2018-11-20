using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopbank.Models;

namespace Shopbank.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShopItemController : ControllerBase
    {
        private readonly ShopbankContext _context;

        public ShopItemController(ShopbankContext context)
        {
            _context = context;
        }

        // GET: api/ShopItem
        [HttpGet]
        public IEnumerable<ShopItem> GetShopItem()
        {
            return _context.ShopItem;
        }

        // GET: api/ShopItem/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetShopItem([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var shopItem = await _context.ShopItem.FindAsync(id);

            if (shopItem == null)
            {
                return NotFound();
            }

            return Ok(shopItem);
        }

        // PUT: api/ShopItem/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutShopItem([FromRoute] int id, [FromBody] ShopItem shopItem)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != shopItem.Id)
            {
                return BadRequest();
            }

            _context.Entry(shopItem).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ShopItemExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/ShopItem
        [HttpPost]
        public async Task<IActionResult> PostShopItem([FromBody] ShopItem shopItem)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.ShopItem.Add(shopItem);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetShopItem", new { id = shopItem.Id }, shopItem);
        }

        // DELETE: api/ShopItem/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteShopItem([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var shopItem = await _context.ShopItem.FindAsync(id);
            if (shopItem == null)
            {
                return NotFound();
            }

            _context.ShopItem.Remove(shopItem);
            await _context.SaveChangesAsync();

            return Ok(shopItem);
        }

        private bool ShopItemExists(int Id)
        {
            return _context.ShopItem.Any(e => e.Id == Id);
        }
        // GET: api/Meme/Tags
        [Route("tags")]
        [HttpGet]
        public async Task<List<string>> GetTags()
        {
            var shops = (from m in _context.ShopItem
                         select m.Tags).Distinct();

            var returned = await shops.ToListAsync();

            return returned;
        }
    }
}