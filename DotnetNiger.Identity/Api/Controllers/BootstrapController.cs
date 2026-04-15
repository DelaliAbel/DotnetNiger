using Asp.Versioning;
using DotnetNiger.Identity.Domain.Entities;
using DotnetNiger.Identity.Infrastructure.Data.Seeds;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DotnetNiger.Identity.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("bootstrap/super-admin")]
[ApiExplorerSettings(IgnoreApi = true)]
public class BootstrapController : ControllerBase
{
    private readonly IServiceProvider _serviceProvider;
    private readonly UserManager<ApplicationUser> _userManager;

    public BootstrapController(IServiceProvider serviceProvider, UserManager<ApplicationUser> userManager)
    {
        _serviceProvider = serviceProvider;
        _userManager = userManager;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Form()
    {
        if (await DefaultRolesSeeder.SuperAdminExistsAsync(_serviceProvider))
        {
            return Content("SuperAdmin already exists. Bootstrap is disabled.", "text/plain");
        }

        const string html = """
<!doctype html>
<html lang="en">

<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width,initial-scale=1" />
    <title>SuperAdmin Bootstrap</title>
    <style>
        * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
            font-family: Segoe UI, sans-serif;
        }

        body {
            background: linear-gradient(135deg, #F4F2F2, #512BD4);
            height: 100vh;
            display: flex;
            align-items: center;
            justify-content: center;

            flex-direction: column;
        }

        .container {
            background: white;
            padding: 30px;
            border-radius: 12px;
            width: 350px;
            box-shadow: 0 10px 25px rgba(0, 0, 0, 0.2);
            animation: fadeIn 0.6s ease-in-out;
        }

        h1 {
            text-align: center;
            margin-bottom: 10px;
            color: #333;
        }

        p {
            text-align: center;
            font-size: 14px;
            color: #777;
            margin-bottom: 20px;
        }

        input {
            width: 100%;
            padding: 12px;
            margin-bottom: 15px;
            border: 1px solid #ddd;
            border-radius: 8px;
            transition: 0.3s;
        }

        input:focus {
            border-color: #2a5298;
            outline: none;
            box-shadow: 0 0 5px rgba(42, 82, 152, 0.3);
        }

        button {
            width: 100%;
            padding: 12px;
            border: none;
            border-radius: 8px;
            background: #2a5298;
            color: white;
            font-size: 16px;
            cursor: pointer;
            transition: 0.3s;
        }

        button:hover {
            background: #1e3c72;
        }

        @keyframes fadeIn {
            from {
                opacity: 0;
                transform: translateY(20px);
            }

            to {
                opacity: 1;
                transform: translateY(0);
            }
        }
    </style>
</head>

<body>
    <div class="container">
        <h1>Create First SuperAdmin</h1>
        <p>This form is available only when no SuperAdmin exists.</p>
        <form method="post" action="/bootstrap/super-admin">
            <input name="Email" type="email" placeholder="Email" required />
            <input name="FullName" type="text" placeholder="Full name" required />
            <input name="Password" type="password" placeholder="Password" minlength="8" required />

            <button type="submit">Create SuperAdmin</button>
        </form>
    </div>

    <script>
        document.querySelector("form").addEventListener("submit", (e) => {
            const btn = document.querySelector("button");
            btn.innerText = "Creation...";
            btn.disabled = true;
        });
    </script>
</body>

</html>
""";

        return Content(html, "text/html");
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Create([FromForm] BootstrapSuperAdminRequest request)
    {
        if (await DefaultRolesSeeder.SuperAdminExistsAsync(_serviceProvider))
        {
            return Conflict(new { message = "SuperAdmin already exists." });
        }

        var email = request.Email?.Trim() ?? string.Empty;
        var fullName = request.FullName?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { message = "Email, full name and password are required." });
        }

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FullName = fullName,
            EmailConfirmed = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var createResult = await _userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            return BadRequest(new
            {
                message = "Failed to create SuperAdmin.",
                errors = createResult.Errors.Select(error => error.Description)
            });
        }

        var addRoleResult = await _userManager.AddToRoleAsync(user, "SuperAdmin");
        if (!addRoleResult.Succeeded)
        {
            return BadRequest(new
            {
                message = "SuperAdmin user created but role assignment failed.",
                errors = addRoleResult.Errors.Select(error => error.Description)
            });
        }

        const string html = """

<!doctype html>
<html lang="en">

<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width,initial-scale=1" />
    <title>SuperAdmin Bootstrap</title>
    <style>
        * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
            font-family: Segoe UI, sans-serif;
        }

        body {
            background: linear-gradient(135deg, #F4F2F2, #512BD4);
            height: 100vh;
            display: flex;
            align-items: center;
            justify-content: center;

            flex-direction: column;
        }

        .container {
            background: white;
            padding: 30px;
            border-radius: 12px;
            width: 400px;
            box-shadow: 0 10px 25px rgba(0, 0, 0, 0.2);
            animation: fadeIn 0.6s ease-in-out;
        }

        h1 {
            text-align: center;
            /* font-size: 14px; */
            margin-bottom: 10px;
            color: #333;
        }

        @keyframes fadeIn {
            from {
                opacity: 0;
                transform: translateY(20px);
            }

            to {
                opacity: 1;
                transform: translateY(0);
            }
        }
    </style>
</head>

<body>
    <div class="container">
        <h1>SuperAdmin created successfully. Bootstrap is now disabled.</h1>
    </div>
</body>

</html>
""";

        return Content(html, "text/html");
    }

    public class BootstrapSuperAdminRequest
    {
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
