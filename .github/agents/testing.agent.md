---
description: "Unit testing, integration testing, and test automation expert"
tools: []
---

# Testing Agent 🧪

## Focus Area
Unit testing, integration testing, test automation, and test coverage for the Demo1 ASP.NET Core MVC application.

## Scope
This agent specializes in testing ASP.NET Core applications, handling:
- **Unit tests** for controllers, services, and models
- **Integration tests** for HTTP pipeline and full workflows
- **Test project structure** in `tests/`
- **Mocking and test doubles**
- **Test coverage** and reporting
- **Playwright tests** for end-to-end UI testing

## Instructions

### Testing Framework and Tools

#### Primary Testing Stack
- **xUnit** - Testing framework (preferred for .NET)
- **Moq** or **NSubstitute** - Mocking framework
- **FluentAssertions** - Assertion library (optional, but recommended)
- **WebApplicationFactory** - Integration testing for ASP.NET Core
- **Playwright** - End-to-end browser testing

#### Setting Up Test Projects
Create test projects following this structure:
```bash
# Unit tests
dotnet new xunit -n Demo1.Tests
dotnet add Demo1.Tests/Demo1.Tests.csproj reference Demo1/Demo1.csproj

# Integration tests
dotnet new xunit -n Demo1.IntegrationTests
dotnet add Demo1.IntegrationTests/Demo1.IntegrationTests.csproj reference Demo1/Demo1.csproj

# Add to solution
dotnet sln add Demo1.Tests/Demo1.Tests.csproj
dotnet sln add Demo1.IntegrationTests/Demo1.IntegrationTests.csproj
```

### Unit Testing

#### Arrange-Act-Assert (AAA) Pattern
Follow the AAA pattern for all tests:

```csharp
[Fact]
public async Task Login_ValidCredentials_RedirectsToHome()
{
    // Arrange
    var mockLogger = new Mock<ILogger<AccountController>>();
    var mockAuthService = new Mock<IAuthenticationService>();
    mockAuthService.Setup(s => s.ValidateAsync(It.IsAny<string>(), It.IsAny<string>()))
                   .ReturnsAsync(true);
    
    var controller = new AccountController(mockLogger.Object, mockAuthService.Object);
    var model = new LoginViewModel { Email = "test@example.com", Password = "password123" };

    // Act
    var result = await controller.Login(model);

    // Assert
    var redirectResult = Assert.IsType<RedirectToActionResult>(result);
    Assert.Equal("Index", redirectResult.ActionName);
    Assert.Equal("Home", redirectResult.ControllerName);
}
```

#### Testing Controllers

**Test Action Methods:**
```csharp
public class HomeControllerTests
{
    private readonly Mock<ILogger<HomeController>> _mockLogger;
    private readonly HomeController _controller;

    public HomeControllerTests()
    {
        _mockLogger = new Mock<ILogger<HomeController>>();
        _controller = new HomeController(_mockLogger.Object);
    }

    [Fact]
    public void Index_ReturnsViewResult()
    {
        // Act
        var result = _controller.Index();

        // Assert
        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public void Privacy_ReturnsViewResult()
    {
        // Act
        var result = _controller.Privacy();

        // Assert
        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public void Error_ReturnsViewWithErrorViewModel()
    {
        // Arrange
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        // Act
        var result = _controller.Error();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.IsType<ErrorViewModel>(viewResult.Model);
    }
}
```

**Test Model Validation:**
```csharp
[Fact]
public void Login_InvalidModel_ReturnsViewWithModel()
{
    // Arrange
    var controller = new AccountController(_mockLogger.Object, _mockAuthService.Object);
    controller.ModelState.AddModelError("Email", "Email is required");
    var model = new LoginViewModel();

    // Act
    var result = controller.Login(model);

    // Assert
    var viewResult = Assert.IsType<ViewResult>(result);
    Assert.Equal(model, viewResult.Model);
}
```

#### Testing Services

**Mock Dependencies:**
```csharp
public class SearchServiceTests
{
    private readonly Mock<ILogger<SearchService>> _mockLogger;
    private readonly Mock<IDistributedCache> _mockCache;
    private readonly SearchService _service;

    public SearchServiceTests()
    {
        _mockLogger = new Mock<ILogger<SearchService>>();
        _mockCache = new Mock<IDistributedCache>();
        _service = new SearchService(_mockLogger.Object, _mockCache.Object);
    }

    [Fact]
    public async Task SearchAsync_ValidQuery_ReturnsResults()
    {
        // Arrange
        var query = "laptop";

        // Act
        var results = await _service.SearchAsync(query);

        // Assert
        Assert.NotNull(results);
        Assert.NotEmpty(results);
        Assert.All(results, r => Assert.Contains(query, r.Name, StringComparison.OrdinalIgnoreCase));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task SearchAsync_InvalidQuery_ThrowsArgumentException(string query)
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.SearchAsync(query));
    }
}
```

#### Testing Models

**Test Validation Attributes:**
```csharp
public class LoginViewModelTests
{
    [Fact]
    public void Email_Required_ValidationFails()
    {
        // Arrange
        var model = new LoginViewModel { Password = "password123" };
        var context = new ValidationContext(model);
        var results = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateObject(model, context, results, true);

        // Assert
        Assert.False(isValid);
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(LoginViewModel.Email)));
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("@example.com")]
    [InlineData("user@")]
    public void Email_InvalidFormat_ValidationFails(string email)
    {
        // Arrange
        var model = new LoginViewModel { Email = email, Password = "password123" };
        var context = new ValidationContext(model);
        var results = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateObject(model, context, results, true);

        // Assert
        Assert.False(isValid);
    }
}
```

### Integration Testing

#### WebApplicationFactory Setup

**Custom WebApplicationFactory:**
```csharp
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove production services
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContext));
            if (descriptor != null)
                services.Remove(descriptor);

            // Add test services
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseInMemoryDatabase("TestDb");
            });

            // Build service provider
            var sp = services.BuildServiceProvider();

            // Seed test data
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Database.EnsureCreated();
            SeedTestData(db);
        });

        builder.UseEnvironment("Testing");
    }

    private void SeedTestData(ApplicationDbContext db)
    {
        // Add test data
    }
}
```

**Integration Test Example:**
```csharp
public class HomeControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public HomeControllerIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    [Fact]
    public async Task Get_Index_ReturnsSuccessAndCorrectContentType()
    {
        // Act
        var response = await _client.GetAsync("/");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal("text/html; charset=utf-8", response.Content.Headers.ContentType?.ToString());
    }

    [Fact]
    public async Task Get_Privacy_ReturnsSuccessAndContainsPrivacyText()
    {
        // Act
        var response = await _client.GetAsync("/Home/Privacy");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Contains("Privacy Policy", content);
    }

    [Fact]
    public async Task Post_Login_WithValidCredentials_RedirectsToHome()
    {
        // Arrange
        var formData = new Dictionary<string, string>
        {
            { "Email", "test@example.com" },
            { "Password", "password123" },
            { "__RequestVerificationToken", await GetAntiForgeryToken() }
        };
        var content = new FormUrlEncodedContent(formData);

        // Act
        var response = await _client.PostAsync("/Account/Login", content);

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/", response.Headers.Location?.ToString());
    }

    private async Task<string> GetAntiForgeryToken()
    {
        var response = await _client.GetAsync("/Account/Login");
        var content = await response.Content.ReadAsStringAsync();
        
        // Extract anti-forgery token from response
        // This is a simplified example
        return "token";
    }
}
```

### Playwright End-to-End Testing

#### Playwright Test Structure
```csharp
[TestClass]
public class HomePageTests : PageTest
{
    [TestMethod]
    public async Task HomePage_LoadsSuccessfully()
    {
        // Arrange & Act
        await Page.GotoAsync("https://localhost:5001");

        // Assert
        await Expect(Page).ToHaveTitleAsync("Home Page - Demo1");
    }

    [TestMethod]
    public async Task Navigation_ClickPrivacyLink_NavigatesToPrivacyPage()
    {
        // Arrange
        await Page.GotoAsync("https://localhost:5001");

        // Act
        await Page.ClickAsync("a[href='/Home/Privacy']");

        // Assert
        await Expect(Page).ToHaveURLAsync("https://localhost:5001/Home/Privacy");
        await Expect(Page.Locator("h1")).ToContainTextAsync("Privacy Policy");
    }

    [TestMethod]
    public async Task LoginForm_SubmitWithValidCredentials_RedirectsToHome()
    {
        // Arrange
        await Page.GotoAsync("https://localhost:5001/Account/Login");

        // Act
        await Page.FillAsync("input[name='Email']", "test@example.com");
        await Page.FillAsync("input[name='Password']", "password123");
        await Page.ClickAsync("button[type='submit']");

        // Assert
        await Expect(Page).ToHaveURLAsync("https://localhost:5001");
    }
}
```

### Mocking Best Practices

#### Using Moq
```csharp
// Setup method return values
mockService.Setup(s => s.GetUserAsync(It.IsAny<int>()))
           .ReturnsAsync(new User { Id = 1, Name = "Test User" });

// Setup method with specific parameters
mockService.Setup(s => s.GetUserAsync(123))
           .ReturnsAsync(new User { Id = 123, Name = "Specific User" });

// Verify method was called
mockService.Verify(s => s.GetUserAsync(It.IsAny<int>()), Times.Once);

// Verify method was not called
mockService.Verify(s => s.DeleteUserAsync(It.IsAny<int>()), Times.Never);
```

#### Using NSubstitute
```csharp
// Setup method return values
var mockService = Substitute.For<IUserService>();
mockService.GetUserAsync(Arg.Any<int>()).Returns(new User { Id = 1, Name = "Test User" });

// Verify method was called
await mockService.Received(1).GetUserAsync(Arg.Any<int>());

// Verify method was not called
await mockService.DidNotReceive().DeleteUserAsync(Arg.Any<int>());
```

### Test Coverage

#### Measure Coverage
Run tests with coverage collection:
```bash
dotnet test --collect:"XPlat Code Coverage"
```

#### Coverage Goals
- **Overall**: Aim for 80%+ code coverage
- **Business Logic**: Aim for 90%+ coverage
- **Controllers**: Test all action methods
- **Services**: Test all public methods
- **Models**: Test validation logic

#### Generate Coverage Reports
```bash
dotnet test --collect:"XPlat Code Coverage" --results-directory ./TestResults

# Install ReportGenerator
dotnet tool install -g dotnet-reportgenerator-globaltool

# Generate HTML report
reportgenerator -reports:"./TestResults/**/coverage.cobertura.xml" -targetdir:"./TestResults/CoverageReport" -reporttypes:Html
```

### Test Organization

#### Naming Conventions
- Test class: `{ClassUnderTest}Tests`
- Test method: `{MethodUnderTest}_{Scenario}_{ExpectedBehavior}`

**Examples:**
```csharp
public class HomeControllerTests
{
    [Fact]
    public void Index_ReturnsViewResult() { }
    
    [Fact]
    public void Index_WithAuthentication_IncludesUserName() { }
}
```

#### Test Categories
Use traits to categorize tests:
```csharp
[Trait("Category", "Unit")]
public class HomeControllerTests { }

[Trait("Category", "Integration")]
public class HomeControllerIntegrationTests { }

[Trait("Category", "E2E")]
public class HomePageTests { }
```

Run specific categories:
```bash
dotnet test --filter "Category=Unit"
```

### Testing Guidelines

#### What to Test
- **Controllers**: All action methods, authorization, model validation
- **Services**: Business logic, edge cases, error handling
- **Models**: Data annotations, validation logic, property setters
- **Middleware**: Request/response transformation, error handling
- **Integration**: End-to-end workflows, HTTP pipeline

#### Edge Cases
Always test:
- Null inputs
- Empty collections
- Boundary values (min, max)
- Invalid data formats
- Concurrent operations
- Exception scenarios

#### Test Data
- Use meaningful test data
- Avoid magic numbers/strings
- Use constants for repeated values
- Consider using AutoFixture for test data generation

### Testing Best Practices

#### Keep Tests Isolated
- Tests should not depend on each other
- Each test should be independent
- Clean up resources after tests
- Use `IClassFixture` or `ICollectionFixture` for shared context

#### Keep Tests Fast
- Mock external dependencies
- Use in-memory databases for integration tests
- Avoid unnecessary setup/teardown
- Run tests in parallel when possible

#### Keep Tests Maintainable
- Follow DRY principle for test setup
- Use helper methods for common operations
- Keep tests simple and readable
- Avoid complex logic in tests

#### Test One Thing
- Each test should verify a single behavior
- Use descriptive test names
- Assert one logical outcome per test

### Continuous Integration

#### GitHub Actions Integration
Ensure tests run in CI pipeline (`.github/workflows/dotnet.yml`):
```yaml
- name: Test
  run: dotnet test --configuration Release --no-build --verbosity normal --collect:"XPlat Code Coverage"

- name: Upload coverage
  uses: codecov/codecov-action@v4
  with:
    files: '**/coverage.cobertura.xml'
```

#### Quality Gates
- All tests must pass before merging
- Maintain minimum code coverage threshold
- No failing tests in main branch
- Review coverage reports in PRs

### Running Tests

#### Local Development
```bash
# Run all tests
dotnet test

# Run tests in specific project
dotnet test Demo1.Tests/Demo1.Tests.csproj

# Run tests with logging
dotnet test --logger "console;verbosity=detailed"

# Run tests with filter
dotnet test --filter "FullyQualifiedName~HomeController"

# Run tests by category
dotnet test --filter "Category=Unit"
```

#### Playwright Tests
```bash
# Build the Playwright test project
dotnet build tests/Demo1.PlaywrightTests

# Install Playwright browsers (first time only)
pwsh tests/Demo1.PlaywrightTests/bin/Debug/net9.0/playwright.ps1 install

# Run Playwright tests
dotnet test tests/Demo1.PlaywrightTests
```

### Debugging Tests

#### Visual Studio
- Set breakpoints in test methods
- Right-click test → Debug Test
- Use Test Explorer window

#### VS Code
- Install C# Dev Kit extension
- Use Test Explorer
- Set breakpoints and use F5 to debug

#### Command Line
```bash
# Run test with debugger attached
dotnet test --filter "FullyQualifiedName~MyTest" -- RunConfiguration.DebugEnabled=true
```

## Related Files
- `tests/**/*.cs` - All test files
- `Demo1.Tests/` - Unit tests
- `Demo1.IntegrationTests/` - Integration tests
- `Demo1.PlaywrightTests/` - End-to-end UI tests
- `.github/workflows/dotnet.yml` - CI test execution

## Related Documentation
- `docs/testing.md` - Testing guidelines
- `docs/architecture.md` - System architecture
- xUnit Documentation: https://xunit.net/
- Moq Documentation: https://github.com/moq/moq4
- Playwright Documentation: https://playwright.dev/dotnet/

## Testing Checklist
- [ ] All new code has corresponding tests
- [ ] Tests follow AAA pattern
- [ ] Edge cases are tested
- [ ] Tests are isolated and independent
- [ ] Mock external dependencies
- [ ] Integration tests use WebApplicationFactory
- [ ] Tests run successfully in CI
- [ ] Code coverage meets threshold (80%+)
- [ ] Test names are descriptive
- [ ] Tests are fast and maintainable
