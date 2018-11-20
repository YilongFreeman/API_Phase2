using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using Shopbank.Helpers;
using Shopbank.Models;

namespace Shopbank.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShopItemController : ControllerBase
    {
        private readonly ShopbankContext _context;
        private IConfiguration _configuration;

        public ShopItemController(ShopbankContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // GET: api/Shop
        [HttpGet]
        public IEnumerable<ShopItem> GetShopItem()
        {
            return _context.ShopItem;
        }

        // GET: api/Shop/5
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

        // PUT: api/Meme/5
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

        // POST: api/Meme
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

        // DELETE: api/Meme/5
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

        private bool ShopItemExists(int id)
        {
            return _context.ShopItem.Any(e => e.Id == id);
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
        [HttpPost, Route("upload")]
        public async Task<IActionResult> UploadFile([FromForm]ShopImageItem shop)
        {
            if (!MultipartRequestHelper.IsMultipartContentType(Request.ContentType))
            {
                return BadRequest($"Expected a multipart request, but got {Request.ContentType}");
            }
            try
            {
                using (var stream = shop.Image.OpenReadStream())
                {
                    var cloudBlock = await UploadToBlob(shop.Image.FileName, null, stream);
                    //// Retrieve the filename of the file you have uploaded
                    //var filename = provider.FileData.FirstOrDefault()?.LocalFileName;
                    if (string.IsNullOrEmpty(cloudBlock.StorageUri.ToString()))
                    {
                        return BadRequest("An error has occured while uploading your file. Please try again.");
                    }

                    ShopItem shopItem = new ShopItem();
                    shopItem.Title = shop.Title;
                    shopItem.Description = shop.Description;
                    shopItem.Tags = shop.Tags;

                    System.Drawing.Image image = System.Drawing.Image.FromStream(stream);
                    shopItem.Height = image.Height.ToString();
                    shopItem.Width = image.Width.ToString();
                    shopItem.Url = cloudBlock.SnapshotQualifiedUri.AbsoluteUri;
                    shopItem.Uploaded = DateTime.Now.ToString();

                    _context.ShopItem.Add(shopItem);
                    await _context.SaveChangesAsync();

                    return Ok($"File: {shop.Title} has successfully uploaded");
                }
            }
            catch (Exception ex)
            {
                return BadRequest($"An error has occured. Details: {ex.Message}");
            }


        }

        private async Task<CloudBlockBlob> UploadToBlob(string filename, byte[] imageBuffer = null, System.IO.Stream stream = null)
        {

            var accountName = _configuration["AzureBlob:name"];
            var accountKey = _configuration["AzureBlob:key"]; ;
            var storageAccount = new CloudStorageAccount(new StorageCredentials(accountName, accountKey), true);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            CloudBlobContainer imagesContainer = blobClient.GetContainerReference("images");

            string storageConnectionString = _configuration["AzureBlob:connectionString"];

            // Check whether the connection string can be parsed.
            if (CloudStorageAccount.TryParse(storageConnectionString, out storageAccount))
            {
                try
                {
                    // Generate a new filename for every new blob
                    var fileName = Guid.NewGuid().ToString();
                    fileName += GetFileExtention(filename);

                    // Get a reference to the blob address, then upload the file to the blob.
                    CloudBlockBlob cloudBlockBlob = imagesContainer.GetBlockBlobReference(fileName);

                    if (stream != null)
                    {
                        await cloudBlockBlob.UploadFromStreamAsync(stream);
                    }
                    else
                    {
                        return new CloudBlockBlob(new Uri(""));
                    }

                    return cloudBlockBlob;
                }
                catch (StorageException ex)
                {
                    return new CloudBlockBlob(new Uri(""));
                }
            }
            else
            {
                return new CloudBlockBlob(new Uri(""));
            }

        }

        private string GetFileExtention(string fileName)
        {
            if (!fileName.Contains("."))
                return ""; //no extension
            else
            {
                var extentionList = fileName.Split('.');
                return "." + extentionList.Last(); //assumes last item is the extension 
            }
        }
    }
}