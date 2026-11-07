
using System.Globalization;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using VietRove.Data;
using VietRove.Entities;
using VietRove.Security;
using VietRove.Services;
using VietRove.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// add services to the container
builder.Services.AddControllersWithViews();

// authentication
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

        // kiem tra trang thai tai khoan moi 30 giay
        options.Events.OnValidatePrincipal = async context =>
        {
            ClaimsPrincipal principal = context.Principal;

            Claim userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
            Claim userStatusClaim = principal.FindFirst("UserStatus");
            Claim roleClaim = principal.FindFirst(ClaimTypes.Role);
            Claim lastValidatedClaim = principal.FindFirst("LastValidatedUtc");

            if (userIdClaim == null ||
                userStatusClaim == null ||
                roleClaim == null)
            {
                context.RejectPrincipal();

                await context.HttpContext.SignOutAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme);

                return;
            }

            DateTimeOffset now = DateTimeOffset.UtcNow;

            if (lastValidatedClaim != null &&
                DateTimeOffset.TryParse(
                    lastValidatedClaim.Value,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind,
                    out DateTimeOffset lastValidatedTime))
            {
                TimeSpan elapsed = now - lastValidatedTime;

                if (elapsed >= TimeSpan.Zero &&
                    elapsed < TimeSpan.FromSeconds(30))
                {
                    return;
                }
            }

            if (!int.TryParse(userIdClaim.Value, out int userId) ||
                !int.TryParse(userStatusClaim.Value, out int currentUserStatus))
            {
                context.RejectPrincipal();

                await context.HttpContext.SignOutAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme);

                return;
            }

            VietRoveDbContext dbContext =
                context.HttpContext.RequestServices
                    .GetRequiredService<VietRoveDbContext>();

            User user = await dbContext.Users
                .FirstOrDefaultAsync(u => u.UserId == userId);

            // check user account status
            if (user == null ||
                user.Status != 1 ||
                user.Status != currentUserStatus)
            {
                context.HttpContext.Response.Cookies.Append(
                    "SysAuthMsg",
                    "locked",
                    new CookieOptions
                    {
                        Path = "/",
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Strict
                    });

                context.RejectPrincipal();

                await context.HttpContext.SignOutAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme);

                return;
            }

            // check user role
            string dbRole = string.Empty;

            if (user.RoleId == AppRoles.AdminId)
            {
                dbRole = AppRoles.Admin;
            }
            else if (user.RoleId == AppRoles.GuideId)
            {
                dbRole = AppRoles.Guide;
            }

            if (string.IsNullOrEmpty(dbRole) ||
                roleClaim.Value != dbRole)
            {
                context.RejectPrincipal();

                await context.HttpContext.SignOutAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme);

                return;
            }

            // check guide profile status
            if (user.RoleId == AppRoles.GuideId)
            {
                Claim profileStatusClaim = principal.FindFirst("ProfileStatus");

                if (profileStatusClaim == null ||
                    !int.TryParse(
                        profileStatusClaim.Value,
                        out int currentProfileStatus))
                {
                    context.RejectPrincipal();

                    await context.HttpContext.SignOutAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme);

                    return;
                }

                GuideProfile guide = await dbContext.GuideProfiles
                    .FirstOrDefaultAsync(g => g.UserId == userId);

                int dbProfileStatus = -1;

                if (guide != null)
                {
                    dbProfileStatus = guide.Status;
                }

                if (guide == null || dbProfileStatus != currentProfileStatus)
                {
                    if (currentProfileStatus == (int)ProfileStatus.Pending &&
                        dbProfileStatus == (int)ProfileStatus.Approved)
                    {
                        context.HttpContext.Response.Cookies.Append(
                            "SysAuthMsg",
                            "approved",
                            new CookieOptions
                            {
                                Path = "/",
                                HttpOnly = true,
                                Secure = true,
                                SameSite = SameSiteMode.Strict
                            });
                    }
                    else if (dbProfileStatus == (int)ProfileStatus.Locked)
                    {
                        context.HttpContext.Response.Cookies.Append(
                            "SysAuthMsg",
                            "locked",
                            new CookieOptions
                            {
                                Path = "/",
                                HttpOnly = true,
                                Secure = true,
                                SameSite = SameSiteMode.Strict
                            });
                    }

                    context.RejectPrincipal();

                    await context.HttpContext.SignOutAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme);

                    return;
                }
            }

            // cap nhat thoi diem kiem tra gan nhat
            ClaimsIdentity identity = principal.Identity as ClaimsIdentity;

            if (identity != null)
            {
                Claim oldValidationClaim = identity.FindFirst("LastValidatedUtc");

                if (oldValidationClaim != null)
                {
                    identity.RemoveClaim(oldValidationClaim);
                }

                identity.AddClaim(new Claim(
                    "LastValidatedUtc",
                    now.ToString("o", CultureInfo.InvariantCulture)));

                context.ShouldRenew = true;
            }
        };
    });

// authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(AppRoles.Admin, policy =>
    {
        policy.RequireRole(AppRoles.Admin);
    });

    options.AddPolicy(AppRoles.Guide, policy =>
    {
        policy.RequireRole(AppRoles.Guide);
    });
});

// database connection
string connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "DefaultConnection is not configured.");
}

builder.Services.AddDbContext<VietRoveDbContext>(options =>
{
    options.UseSqlServer(connectionString);
});

// register services
builder.Services.AddScoped<IFileStorageService, FileStorageService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IRegisterService, RegisterService>();
builder.Services.AddScoped<ILoginService, LoginService>();
builder.Services.AddScoped<IGuideProfileService, GuideProfileService>();
builder.Services.AddScoped<
    IAdminLoginHistoryService,
    AdminLoginHistoryService>();
builder.Services.AddScoped<IAdminDashboardService, AdminDashboardService>();
builder.Services.AddScoped<IAdminGuideReviewService, AdminGuideReviewService>();


// bootstrap admin password
string bootstrapAdminPassword = string.Empty;

string configuredPassword =
    builder.Configuration["BootstrapAdmin:Password"];

if (string.IsNullOrWhiteSpace(configuredPassword) == false)
{
    bootstrapAdminPassword = configuredPassword;
}

var app = builder.Build();

// create default admin
if (string.IsNullOrWhiteSpace(bootstrapAdminPassword) == false)
{
    using IServiceScope scope = app.Services.CreateScope();

    VietRoveDbContext context =
        scope.ServiceProvider.GetRequiredService<VietRoveDbContext>();

    ILogger<Program> logger =
        scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    bool adminExists = await context.Users
        .AnyAsync(user => user.Email == "admin@vietrove.com");

    if (adminExists == false)
    {
        bool adminRoleExists = await context.Roles
            .AnyAsync(role => role.RoleId == AppRoles.AdminId);

        if (adminRoleExists == false)
        {
            logger.LogError(
                "Khong the tao tai khoan Admin vi chua co Role Admin trong database.");
        }
        else
        {
            User admin = new User
            {
                Email = "admin@vietrove.com",
                Password = BCrypt.Net.BCrypt.HashPassword(
                    bootstrapAdminPassword,
                    workFactor: 12),
                RoleId = AppRoles.AdminId,
                Status = 1,
                CreatedAt = DateTime.Now
            };

            context.Users.Add(admin);
            await context.SaveChangesAsync();

            logger.LogInformation(
                "Da khoi tao tai khoan Admin mac dinh.");
        }
    }
}

// configure the http request pipeline
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
