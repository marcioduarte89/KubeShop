using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var aspireEnvVariable = "RUNNING_UNDER_ASPIRE";

var products = builder
    .AddProject<Projects.Product_Api>("ProductApi")
    .WithEnvironment(aspireEnvVariable, "true");

builder.AddProject<Projects.Order_Api>("OrderApi")
       .WithExternalHttpEndpoints()
       .WithReference(products)
       .WithEnvironment(aspireEnvVariable, "true")
       .WaitFor(products);

builder.Build().Run();
