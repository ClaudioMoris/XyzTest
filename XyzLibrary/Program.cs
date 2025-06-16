using XyzModels;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

//en Net 9 ya no viene en la plantilla Swagger asi que se debe agregar
//builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var EnviromentVariable = new EnvironmentVariable
{
	hostDb = Environment.GetEnvironmentVariable("HOST_VARIABLE"),
	databaseNameDb = Environment.GetEnvironmentVariable("DATABASE_VARIABLE"),
	usernameDb = Environment.GetEnvironmentVariable("USERNAME_VARIABLE"),
	passwordDb = Environment.GetEnvironmentVariable("PASSWORD_VARIABLE"),
};
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    //app.MapOpenApi();
}
//En Desarrollo se habilita la interfaz Swagger(en este caso la dejaremos visible en todos los casos)
//https://{Tu Ip o Dominio}/swagger/index.html
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
