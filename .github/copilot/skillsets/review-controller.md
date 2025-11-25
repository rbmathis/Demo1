# Review Controller Skill 🎮

## Purpose
Analyze ASP.NET MVC controller code for best practices and common issues.

## What to Check

### MVC Best Practices
- ✅ Controllers should be thin (delegate to services)
- ✅ Use dependency injection
- ✅ Return appropriate ActionResult types
- ✅ Async/await for I/O operations

### Security
- 🔒 [Authorize] attributes present
- 🔒 ModelState validation before processing
- 🔒 AntiForgeryToken for state-changing operations

### Code Quality
- 📝 XML comments on public actions
- 🎯 Single responsibility per method
- ⚡ Proper error handling with try-catch

## Red Flags
- ❌ Business logic in controllers
- ❌ Direct database access (use repositories/services)
- ❌ Synchronous I/O operations
- ❌ Missing authorization checks

## Example Good Pattern

```csharp
[Authorize]
public class ProductController : Controller
{
    private readonly IProductService _productService;
    
    public ProductController(IProductService productService)
    {
        _productService = productService;
    }
    
    /// <summary>
    /// Gets product details by ID
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var product = await _productService.GetByIdAsync(id);
        if (product == null)
            return NotFound();
            
        return View(product);
    }
}
```

## Now, To Actually Make This Work 💪

You still need to create those skillset files! Here's the minimum to get started:

### [review-controller.md](file:///c%3A/Users/rmathis/source/Demo1/.github/copilot/skillsets/review-controller.md)
