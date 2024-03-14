using MongoDB.Driver;
using Server.Models;
using dotenv.net;
using MongoDB.Bson;
DotEnv.Load();

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins",
        policy =>
        {
            policy.WithOrigins("https://dotnet-todolist.azurewebsites.net")
            .AllowAnyHeader()
            .AllowAnyMethod();
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
app.MapGet("/todoitems/{date}", async (DateTime date, IMongoDatabase mongoDatabase) =>
{
    var collection = mongoDatabase.GetCollection<TodoItem>("todoitems");
    var filterBuilder = Builders<TodoItem>.Filter;
    var filter = filterBuilder.Empty; // Default filter
    DateTime startDate = date.Date;
    DateTime endDate = startDate.AddDays(1);
    filter = filterBuilder.And(
               filter,
               filterBuilder.Gte(item => item.Date, startDate),
               filterBuilder.Lt(item => item.Date, endDate)
           );
    var todoItems = await collection.Find(filter).ToListAsync();
    return Results.Ok(todoItems);

});
app.MapDelete("/todoitems/{id}", async (string id, IMongoDatabase mongoDatabase) =>
{
    var collection = mongoDatabase.GetCollection<TodoItem>("todoitems");
    var filter = Builders<TodoItem>.Filter.Eq("_id", new ObjectId(id));
    await collection.DeleteOneAsync(filter);
    return Results.Ok();
});
app.MapPut("/todoitems/{id}", async (string id, TodoItem updatedItem, IMongoDatabase mongoDatabase) =>
{
    var collection = mongoDatabase.GetCollection<TodoItem>("todoitems");

    var filter = Builders<TodoItem>.Filter.Eq("_id", new ObjectId(id));
    var existingItem = await collection.Find(filter).FirstOrDefaultAsync();

    if (existingItem == null)
    {
        return Results.NotFound();
    }

    existingItem.Date = updatedItem.Date;
    existingItem.Category = updatedItem.Category;
    // existingItem.RemindEnabled = updatedItem.RemindEnabled;
    existingItem.Description = updatedItem.Description;

    // Update the item in the database
    var result = await collection.ReplaceOneAsync(filter, existingItem);

    return Results.NoContent();
});
app.Run();
