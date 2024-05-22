using RetCodeAPI;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<ITaskService, TaskService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/task", (int id) => 
{
    var taskService = app.Services.GetService<ITaskService>();
    RetCodeAPI.Task task;
    try {
        task = taskService.GetTask(id);
    } catch (KeyNotFoundException e) {
        return Results.Json(new ErrorResource (true, "No task with such id"), statusCode: 404);
    }    
    return Results.Json(new TaskResource (
        task.TaskId,
        task.Completed,
        task.ExitCode,
        task.Status
    ), statusCode: 200);
})
.WithDescription("Получить задание по id")
.WithName("GetTask")
.WithOpenApi();

app.MapPost("/task", (string path) =>
{
    var taskService = app.Services.GetService<ITaskService>();
    var task = taskService.CreateTask(path);
    return new TaskResource (
        task.TaskId,
        task.Completed,
        task.ExitCode,
        task.Status
    );
})
.WithDescription("Создать задание с запуском скрипта из path")
.WithName("PostTask")
.WithOpenApi();

app.Run();

record TaskResource(int TaskId, bool Completed, int ExitCode, string Status);
record ErrorResource(bool Error, string Message);