using Amazon.S3;
using Amazon.S3.Model;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IAmazonS3>(sp =>
{
    var config = new AmazonS3Config
    {
        ServiceURL = "http://localhost:4566",
        ForcePathStyle = true
    };

    return new AmazonS3Client("test", "test", config);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/", () => Results.Ok(new { service = "CloudDemo", endpoints = new[] { "/upload", "/files" } }));

app.MapPost("/upload", async (IAmazonS3 s3, IFormFile file) =>
{
    using var stream = file.OpenReadStream();

    await s3.PutObjectAsync(new PutObjectRequest
    {
        BucketName = "bucket1",
        Key = file.FileName,
        InputStream = stream
    });

    return Results.Ok("Uploaded");
})
.DisableAntiforgery();

app.MapGet("/files", async (IAmazonS3 s3) =>
{
    var response = await s3.ListObjectsV2Async(new ListObjectsV2Request
    {
        BucketName = "bucket1"
    });

    return Results.Ok(response.S3Objects.Select(o => o.Key));
});

app.Run();
