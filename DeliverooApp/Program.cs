using System.Globalization;
using DeliverooApp.Models;

var cultureInfo = new CultureInfo("en-US");
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

/////// AGGIUNTA PER GESTIRE LA SESSIONE, va messa sempre e solo qua sennò non funzia
builder.Services.AddHttpContextAccessor();//creato il servizio
builder.Services.AddSession(options =>
    {
        options.IdleTimeout = TimeSpan.FromMinutes(30);
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Strict;
    }
    );


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStatusCodePagesWithReExecute("/Home/Error/{0}");
app.UseRouting();

///////AGGIUNTA PER ATTIVARE LA SESSIONE
app.UseSession();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


// Imposta domanda+risposta di default ("Paparino") per utenti esistenti senza risposta sicurezza
var connStr = app.Configuration.GetConnectionString("Default")!;
using (var gd = new GestioneDati(connStr))
{
    gd.MigraColonnaNote();
    gd.MigraColonnaRegistrazioneBloccata();
    gd.MigraUtentiSenzaRisposta(
        "Qual è il nome del tuo primo animale domestico?",
        BCrypt.Net.BCrypt.HashPassword("paparino")
    );
}

app.Run();