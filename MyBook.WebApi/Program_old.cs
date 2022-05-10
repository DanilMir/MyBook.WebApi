// using Microsoft.AspNetCore.Identity;
// using Microsoft.EntityFrameworkCore;
// using MyBook.DataAccess;
// using MyBook.Entity;
// using MyBook.Entity.Identity;
//
// var builder = WebApplication.CreateBuilder(args);
//
// // Add services to the container.
//
// builder.Services.AddControllers();
// // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
// builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddSwaggerGen();
//
//
// builder.Services.AddCors(opt =>
//     {
//         opt.AddDefaultPolicy(builder =>
//         {
//             builder
//                 .AllowAnyOrigin()
//                 .AllowAnyHeader()
//                 .AllowAnyMethod();
//         });
//     })
//     .AddAuthenticationAndJwt()
//     .AddAuthorization()
//     .AddIdentity()
//     .AddOpenIddictServer(_env);
//
//
//
//
// builder.Services.AddDbContext<ApplicationContext>(opts =>
//     opts.UseNpgsql(builder.Configuration.GetConnectionString("sqlConnection")));
//
// builder.Services.AddControllersWithViews()
//     .AddNewtonsoftJson(options =>
//         options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
//     );
//
//
// builder.Services.AddIdentity<User, Role>(option=>option.SignIn.RequireConfirmedEmail=true)
//     .AddEntityFrameworkStores<ApplicationContext>()
//     .AddDefaultTokenProviders();
//
// builder.Services.ConfigureApplicationCookie(options =>
// {
//     options.AccessDeniedPath = new PathString("/Home/AccessDenied");
// });
//
// var app = builder.Build();
//
// // Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
// {
//     app.UseSwagger();
//     app.UseSwaggerUI();
// }
//
// app.UseHttpsRedirection();
//
// app.UseAuthentication();//???
// app.UseAuthorization();
//
// app.MapControllers();
//
// app.Run();