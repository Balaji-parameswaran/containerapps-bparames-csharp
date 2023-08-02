using IMDSValidation;

var builder = WebApplication.CreateBuilder();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options => {
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin();
        builder.AllowAnyHeader();
        builder.AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();

app.MapGet("/", async context =>
{
    await context.Response.WriteAsync("Hit the /albums endpoint to retrieve a list of albums!");
});

app.MapGet("/imds", async context =>
{
    // Get the Authorization header
    var authorizationHeader = context.Request.Headers.Authorization;

    // Check if the Authorization header is not null and has a value
    if (!String.IsNullOrEmpty(authorizationHeader))
    {
        // Get the authorization header content
        var authorizationHeaderContent = authorizationHeader.ToString();

        // Check if the authorization header content contains "Bearer"
        if (authorizationHeaderContent.Contains("Bearer"))
        {
            // Split the authorization header content by space
            var parts = authorizationHeaderContent.Split(' ');

            // Check if the parts array has two elements
            if (parts.Length == 2)
            {
                // Get the second element as the bearer token
                var bearerToken = parts[1];
                ValidateIMDSContainerApp validateIMDSContainerApp = new ValidateIMDSContainerApp();
                Console.WriteLine(" CAlling done for validateimds cs file ");
                // Use the bearer token as needed
                await context.Response.WriteAsync("There is a bearer token - bearer token: " + bearerToken);
            }
            else
            {
                await context.Response.WriteAsync("There is no bearer token ");
            }
        }
        else
        {
            await context.Response.WriteAsync("There is no bearer");
        }
    }
    else
    {
        await context.Response.WriteAsync("There is no auth header 2");
    }
});

app.MapGet("/albums", () =>
{
    return Album.GetAll();
})
.WithName("GetAlbums");

app.Run();

record Album(int Id, string Title, string Artist, double Price, string Image_url)
{
     public static List<Album> GetAll(){
         var albums = new List<Album>(){
            new Album(1, "You, Me and an App Id", "Daprize", 10.99, "https://aka.ms/albums-daprlogo"),
            new Album(2, "Seven Revision Army", "The Blue-Green Stripes", 13.99, "https://aka.ms/albums-containerappslogo"),
            new Album(3, "Scale It Up", "KEDA Club", 13.99, "https://aka.ms/albums-kedalogo"),
            new Album(4, "Lost in Translation", "MegaDNS", 12.99,"https://aka.ms/albums-envoylogo"),
            new Album(5, "Lock Down Your Love", "V is for VNET", 12.99, "https://aka.ms/albums-vnetlogo"),
            new Album(6, "Sweet Container O' Mine", "Guns N Probeses", 14.99, "https://aka.ms/albums-containerappslogo")
         };

        return albums; 
     }
}