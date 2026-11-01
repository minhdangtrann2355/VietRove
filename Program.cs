using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using VietRove.Data;
using VietRove.Entities;
using VietRove.Security;
using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "__Host-VietRove.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.Cookie.Path = "/";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = true;
        options.LoginPath = "/Login/Index";
        options.Events.OnRedirectToAccessDenied = context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        };

        // auto check user status moi 30s
        options.Events.OnValidatePrincipal = async context =>
        {
            var timeSinceCreated = DateTimeOffset.UtcNow - context.Properties.IssuedUtc;
            if (timeSinceCreated > TimeSpan.FromSeconds(30))
            {
                var userIdClaim = context.Principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                {
                    var dbContext = context.HttpContext.RequestServices.GetRequiredService<VietRoveDbContext>();
                    var user = await dbContext.Users.FindAsync(userId);
                    var currentCookieStatus = int.Parse(context.Principal.FindFirst("Status")?.Value ?? "0");
                    
                    int dbStatus = user?.Status ?? 0;
                    if (user != null && user.RoleId == AppRoles.GuideId)
                    {
                        var guide = await dbContext.GuideProfiles.FirstOrDefaultAsync(g => g.UserId == userId);
                        if (guide != null)
                        {
                            dbStatus = (guide.Status == 1 || user.Status == 1) ? 1 : guide.Status;
                        }
                    }

                    // neu user bi xoa hoac bi khoa thi reject principal va signout
                    if (user == null || dbStatus != currentCookieStatus)
                    {
                        if (currentCookieStatus == 0 && dbStatus == 1)
                        {
                            context.HttpContext.Response.Cookies.Append("SysAuthMsg", "approved", new CookieOptions { Path = "/" });
                        }
                        else if (dbStatus == 3)
                        {
                            context.HttpContext.Response.Cookies.Append("SysAuthMsg", "locked", new CookieOptions { Path = "/" });
                        }

                        context.RejectPrincipal();
                        await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                    }
                }
            }
        };
    });
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(AppRoles.Admin, policy => policy.RequireRole(AppRoles.Admin));
    options.AddPolicy(AppRoles.Guide, policy => policy.RequireRole(AppRoles.Guide));
});

string connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "";
builder.Services.AddDbContext<VietRoveDbContext>(options =>
{
    options.UseSqlServer(connectionString);
});

string bootstrapAdminPassword = string.Empty;
IConfigurationSection bootstrapAdminPasswordSection = builder.Configuration.GetSection("BootstrapAdmin:Password");
if (bootstrapAdminPasswordSection.Value is string configuredBootstrapAdminPassword)
{
    bootstrapAdminPassword = configuredBootstrapAdminPassword;
}

if (builder.Environment.IsDevelopment() && string.IsNullOrWhiteSpace(bootstrapAdminPassword))
{
    bootstrapAdminPassword = "Admin@123";
}

var app = builder.Build();

if (string.IsNullOrWhiteSpace(bootstrapAdminPassword) == false)
{
    using IServiceScope scope = app.Services.CreateScope();
    VietRoveDbContext context = scope.ServiceProvider.GetRequiredService<VietRoveDbContext>();

    bool adminExists = await context.Users.AnyAsync(user => user.Email == "admin@vietrove.com");
    if (adminExists == false)
    {
        bool adminRoleExists = await context.Roles.AnyAsync(role => role.RoleId == AppRoles.AdminId);
        if (adminRoleExists == false)
        {
            bool rolesTableIsEmpty = await context.Roles.AnyAsync() == false;
            if (rolesTableIsEmpty)
            {
                context.Roles.AddRange(
                    new Role { RoleName = AppRoles.Admin },
                    new Role { RoleName = AppRoles.Guide });
                await context.SaveChangesAsync();
                adminRoleExists = true;
            }
        }

        if (adminRoleExists)
        {
            User admin = new User
            {
                Email = "admin@vietrove.com",
                Password = BCrypt.Net.BCrypt.HashPassword(bootstrapAdminPassword, workFactor: 12),
                RoleId = AppRoles.AdminId,
                Status = 1,
                CreatedAt = DateTime.Now
            };

            context.Users.Add(admin);
            await context.SaveChangesAsync();
        }
    }
}


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
