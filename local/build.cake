#addin nuget:?package=Cake.Docker&version=1.2.0

var target = Argument("target", "Default");

//////////////////////////////////////////////////////////////////////
// Variables
//////////////////////////////////////////////////////////////////////

var sourceFolder = @"../src";

var tyeFolder = @"./tye";

var dockerComposesPath = @"./infra";

var dockerComposeFiles = new[] 
{
    "docker-compose.observability.yaml"
};

var nameOfDockerImageBase = "localhost:5010/net7-alpine-base:latest";

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

TaskSetup(taskSetupContext => { Console.Clear(); });

Task("DockerComposeDev-Down").Does(context => 
{
    context.Environment.WorkingDirectory = context.MakeAbsolute(Directory(dockerComposesPath));

    var settings = new DockerComposeDownSettings 
    {
        Volumes = true,
        Files = dockerComposeFiles
    }; 

    context.DockerComposeDown(settings);
});

Task("DockerCompose-Up").Does(context => 
{
    context.Environment.WorkingDirectory = context.MakeAbsolute(Directory(dockerComposesPath));

    var settings = new DockerComposeUpSettings
    {
        DetachedMode = true,
        Files = dockerComposeFiles
    };

    context.DockerComposeUp(settings);
});

Task("Infra-Start").Does(context => 
{
    context.Environment.WorkingDirectory = context.MakeAbsolute(Directory(dockerComposesPath));
    var settings = new DockerComposeSettings
    {
        Files = dockerComposeFiles
    };

    context.DockerComposeStart(settings);

});

Task("Stop-And-Remove-Local-Registry").Does(() => {

    // docker container stop registry && docker container rm -v registry

    DockerRm(new DockerContainerRmSettings {
        Force = true,
        Volumes = true
    }, "registry");
});

Task("Start-Local-Registry")
.IsDependentOn("Stop-And-Remove-Local-Registry")
.Does(() =>
{
    // docker run -d -p 5010:5000 --restart always --name registry registry:2
    DockerRun(new DockerContainerRunSettings {
        Detach = true,
        Publish = new[] { "5010:5000" },
        Name = "registry",
        Restart = "always"
    }, "registry:2", string.Empty);
});

Task("Build-Image-Base")
.IsDependentOn("Start-Local-Registry")
.Does(() => 
{
    DockerBuild(new DockerImageBuildSettings {
        File = "./Dockerfile",
        Tag = new [] { nameOfDockerImageBase }
    }, "../");

    DockerPush(nameOfDockerImageBase);
});

Task("Build-Docker-Images")
.DoesForEach(new[] {"Microservices", "ClientApps"}, folder => 
{
    var csprojFiles = GetFiles($@"{sourceFolder}/{folder}/**/*.csproj");

    // Uncomment if do not want to use own custom image "localhost:5010/net7-alpine-base:latest"
    //
    // nameOfDockerImageBase = "mcr.microsoft.com/dotnet/aspnet:7.0.5";
    // nameOfDockerImageBase = "mcr.microsoft.com/dotnet/aspnet:7.0.5-alpine3.17-amd64";

    foreach(var csprojFile in csprojFiles)
    {
        DotNetPublish(csprojFile.FullPath, new DotNetPublishSettings{
            ArgumentCustomization = builder => builder
                        .Append("--os linux")
                        .Append("--arch x64")
                        .Append("-c Release")
                        // .Append("-p:ContainerRuntimeIdentifier=linux-x64")
                        .Append($"-p:ContainerBaseImage={nameOfDockerImageBase}")
                        .Append("-p:ContainerImageTags=latest"),
            WorkingDirectory = csprojFile.GetDirectory()
        });
    }
});


Task("Tye").Does(() => {

    var tyeFileName = "tye.yaml";
    // tyeFileName = "tye-container.yaml";
    // tyeFileName = "tye-mixing.yaml";

    var logDirectory = System.IO.Path.Combine(tyeFolder, @".logs");
    var tyeFilePath = System.IO.Path.Combine(tyeFolder, tyeFileName);

    if (DirectoryExists(logDirectory))
    {
        DeleteDirectory(logDirectory, new DeleteDirectorySettings {
            Recursive = true,
            Force = true
        });
    }
    
    StartProcess("tye", new ProcessSettings {
        Arguments = new ProcessArgumentBuilder()
            .Append("run")
            .Append(tyeFilePath)
            // .Append("--watch")
            .Append("--dashboard")
        }
    );

});

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

Task("Default").Does(() => Information("Dotnet Practical"));

RunTarget(target);



//////////////////////////////////////////////////////////////////////
// Function
//////////////////////////////////////////////////////////////////////