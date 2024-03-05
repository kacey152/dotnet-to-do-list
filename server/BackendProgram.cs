using MongoDB.Driver;
using Server.Models;
using dotenv.net;
DotEnv.Load();

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins",
        policy =>
        {
            policy.WithOrigins("http://localhost:5217")
            .AllowAnyHeader();
        });
});

var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");
var databaseName = Environment.GetEnvironmentVariable("DB_NAME");

if (string.IsNullOrEmpty(connectionString) || string.IsNullOrEmpty(databaseName))
{
    Console.WriteLine(connectionString);
    throw new Exception("Missing environment variables for MongoDB configuration");
}

var mongoClient = new MongoClient(connectionString);
var mongoDatabase = mongoClient.GetDatabase(databaseName);

builder.Services.AddSingleton<IMongoDatabase>(mongoDatabase);
var app = builder.Build();
app.UseCors("AllowSpecificOrigins");


app.MapPost("/todoitems", async (TodoItem todoItem, IMongoDatabase mongoDatabase) =>
{
    var collection = mongoDatabase.GetCollection<TodoItem>("todoitems");
    await collection.InsertOneAsync(todoItem);
    return Results.Created($"/todoitems/{todoItem.Id}", todoItem);
});
app.Run();
