using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using TeqRestaurant.Data;
using TeqRestaurant.Models;
using TeqRestaurant.Repository;
using TeqRestaurant.Repository.Implementation;

namespace TeqRestaurant.Controllers
{
    public class ProductController : Controller
    {
        private Repository<Product> product;
        private Repository<Ingredient> ingredient;
        private Repository<Category> category;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductController(ApplicationDbContext context,IWebHostEnvironment webHostEnvironment) 
        {
            product = new Repository<Product>(context);
            ingredient = new Repository<Ingredient>(context);
            category = new Repository<Category>(context);   
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            return View(await product.GetAllAsync());
        }

        #region Add/Edit
        [HttpGet]
        public async Task<IActionResult> AddEdit(int id) 
        {
            //ingredients info
            ViewBag.Ingredients = await ingredient.GetAllAsync();
            ViewBag.Categories = await category.GetAllAsync();    

            if(id==0)
            {
                ViewBag.Operations = "Add";
                return View(new Product());
            }
            else
            {
                Product p = await product.GetByIdAsync(id,
                    new QueryOptions<Product> { Includes = "ProductIngredients.Ingredient, Category" });
                ViewBag.Operations = "Edit";
                return View(p); 
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddEdit(Product pModel, int[] ingredientId,int catId)
        {
            ViewBag.Ingredients = await ingredient.GetAllAsync();
            ViewBag.Categories = await category.GetAllAsync();

            if (ModelState.IsValid) 
            {
                if(pModel.ImageFile != null)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images"); 
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + pModel.ImageFile.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);   

                    using(var fileStream = new FileStream(filePath,FileMode.Create))
                    {
                        await pModel.ImageFile.CopyToAsync(fileStream);
                    }
                    pModel.ImageUrl = uniqueFileName;
                }

                if(pModel.ProductId == 0)
                {
                    ViewBag.Ingredients = await ingredient.GetAllAsync();
                    ViewBag.Categories = await category.GetAllAsync();
                    pModel.CategoryId = catId;

                    //add ingredients
                    foreach(int id in ingredientId)
                    {
                        pModel.ProductIngredients?.Add(new ProductIngredient { IngredientId=id , ProductId = pModel.ProductId});
                    }

                    await product.AddAsync(pModel);
                    return RedirectToAction("Index","Product");   
                }
                else 
                {
                    var existingProduct = await product.GetByIdAsync(pModel.ProductId,
                    new QueryOptions<Product> { Includes = "ProductIngredients" });

                    if(existingProduct ==  null)
                    {
                        ModelState.AddModelError("", "Product not found");
                        ViewBag.Ingredients = await ingredient.GetAllAsync();
                        ViewBag.Categories = await category.GetAllAsync();
                        return View(pModel);
                    }

                    existingProduct.Name = pModel.Name;
                    existingProduct.Description = pModel.Description;
                    existingProduct.Price = pModel.Price;
                    existingProduct.Stock = pModel.Stock;
                    existingProduct.CategoryId = pModel.CategoryId;
                    //add ingredients
                    existingProduct.ProductIngredients?.Clear();
                    foreach (int id in ingredientId)
                    {
                        existingProduct.ProductIngredients?.Add(new ProductIngredient { IngredientId = id, ProductId = pModel.ProductId });
                    }

                    try 
                    {
                        await product.UpdateAsync(existingProduct); 
                    }
                    catch (Exception ex) 
                    {
                        ModelState.AddModelError("",$"Error:{ex.GetBaseException().Message}");
                        ViewBag.Ingredients = await ingredient.GetAllAsync();
                        ViewBag.Categories = await category.GetAllAsync();
                        return View(pModel);
                    }
                }
            }
            return RedirectToAction("Index", "Product");
        }
        #endregion
    }
}
