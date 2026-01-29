using CellcomServer.Classes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;

namespace CellcomServer.Controllers
{
    [ApiController]
    [Route("api/personalarea")]
    [Authorize]
    public class PersonalAreaController : ControllerBase
    {
        AppDbContext _context;
        private readonly IMongoCollection<ProductMetadata> _mongoCollection;
        private readonly ILogger<PersonalAreaController> _logger;
        public PersonalAreaController(AppDbContext context,IMongoCollection<ProductMetadata> mongoCollection, ILogger<PersonalAreaController> logger)
        {
            _context = context;
            _mongoCollection = mongoCollection;
            _logger = logger;
        }

        [HttpGet("showAllProducts")]
        public IActionResult ShowAllProducts()
        {
            try
            {
                var allProducts = _context.Products.ToList();
                return Ok(allProducts);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("product/{productId}/metadata")]
        public async Task<IActionResult> GetProductMetadata(string productId)
        {
            var metadata = await _mongoCollection
                .Find(x => x.ProductId == productId)
                .FirstOrDefaultAsync();

            if (metadata == null)
                return NotFound("No metadata found in MongoDB");

            return Ok(metadata);
        }

        [HttpPost("addUser")]
        public IActionResult AddUser([FromBody] UserDTO request)
        {
            var RightUser = _context.AllUsers.FirstOrDefault(u => u.ID == request.ID);

            if (RightUser != null)
            {
                return Unauthorized("User already exists");
            }

            try
            {
                byte[] hashedPassword;
                using (SHA256 sha256 = SHA256.Create())
                {
                    hashedPassword = sha256.ComputeHash(Encoding.UTF8.GetBytes(request.UserPassword));
                }

                var newUser = new UserItem
                {
                    ID = request.ID,
                    FullName = request.FullName,
                    PhoneNumber = request.PhoneNumber,
                    Address = request.Address,
                    UserPassword = hashedPassword
                };

                var newLoginUser = new LoginUser
                {
                    ID = request.ID,
                    phoneNumber = request.PhoneNumber,
                    userpassword = hashedPassword
                };

                _context.AllUsers.Add(newUser);
                _context.LoginUsers.Add(newLoginUser);
                _context.SaveChanges();
                return Ok("New user added successfully.");
            }
            catch (Exception ex)
            {
                return BadRequest("Exception while adding new user: " + ex.Message);
            }
        }

        [HttpPut("updateProduct/{productId}")]
        public async Task<IActionResult> UpdateProduct(string productId, [FromForm] ProductRequest request)
        {
            var RightProdcut = _context.Products.FirstOrDefault(u => u.ProductID == request.ProductID);

            if (RightProdcut == null)
            {
                return NotFound("Product does not exists");
            }

            try
            {
                RightProdcut.ProductID = request.ProductID;
                RightProdcut.ProductName = request.ProductName;
                RightProdcut.Price = request.Price;
                RightProdcut.Currency = request.Currency;

                // Update image only if a new one is uploaded
                if (request.Image != null)
                {
                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(request.Image.FileName)}";
                    var savePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/products", fileName);

                    using var stream = new FileStream(savePath, FileMode.Create);
                    await request.Image.CopyToAsync(stream);

                    RightProdcut.ImageUrl = $"/images/products/{fileName}";
                }

                await _context.SaveChangesAsync();
                return Ok("Product "+ RightProdcut.ProductID+" updated successfully.");
            }
            catch (Exception ex)
            {
                return BadRequest("Exception while adding new product: " + ex.Message);
            }
        }

        [HttpDelete("deleteProduct/{productId}")]
        public IActionResult DeleteProduct(string productId)
        {
            var product = _context.Products.FirstOrDefault(p => p.ProductID == productId);

            if (product == null)
                return NotFound();

            _context.Products.Remove(product);
            _context.SaveChanges();

            return Ok();
        }

        [HttpPost("addProduct")]
        public async Task<IActionResult> AddProduct([FromForm] ProductRequest request)
        {
            var RightProdcut= _context.Products.FirstOrDefault(u => u.ProductID == request.ProductID);

            if (RightProdcut != null)
            {
                return BadRequest("Product already exists");
            }

            try
            {
                string? imagePath = null;

                if (request.Image != null)
                {
                    // Generate a unique file name
                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(request.Image.FileName)}";

                    // This is where you put it — combine directory + folder + file name
                    var savePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/products", fileName);

                    // Save the file
                    using var stream = new FileStream(savePath, FileMode.Create);
                    await request.Image.CopyToAsync(stream);

                    // Store the relative URL in database
                    imagePath = $"/images/products/{fileName}";
                }

                var newProduct = new Product
                {
                    ProductID = request.ProductID,
                    ProductName = request.ProductName,
                    Price = request.Price,
                    Currency = request.Currency,
                    ImageUrl = imagePath
                };

                _context.Products.Add(newProduct);
                await _context.SaveChangesAsync();

                /*await _mongoCollection.InsertOneAsync(new ProductMetadata
                {
                    ProductId = request.ProductID,
                    Tags = new List<string> { "new", "sale" },
                    Views = 0
                });*/

                _logger.LogInformation("Before Mongo insert");

                await _mongoCollection.InsertOneAsync(new ProductMetadata
                    {
                        ProductId = request.ProductID,
                        Tags = new List<string> { "test" },
                        Views = 1
                });

                _logger.LogInformation("After Mongo insert");

                var inserted = await _mongoCollection
                .Find(x => x.ProductId == request.ProductID)
                .FirstOrDefaultAsync();

                _logger.LogInformation(
                    inserted != null
                        ? "Mongo insert VERIFIED"
                        : "Mongo insert FAILED"
                );

                return Ok("New product added successfully.");
                
            }
            catch (Exception ex)
            {
                return BadRequest("Exception while adding new product: " + ex.Message);
            }
        }
    }
}
