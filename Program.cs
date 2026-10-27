using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using VietRove.Data;
using VietRove.Entities;
using VietRove.Security;

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
