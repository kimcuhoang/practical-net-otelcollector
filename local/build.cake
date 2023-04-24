#addin nuget:?package=Cake.Docker&version=1.2.0

var target = Argument("target", "Default");

//////////////////////////////////////////////////////////////////////
// Variables
//////////////////////////////////////////////////////////////////////

var tyeFolder = @"./tye";

var dockerComposesPath = @"./infra";

var dockerComposeFiles = new[] 
{
    "docker-compose.observability.yaml"
};


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

Task("Tye").Does(() => {

    var logDirectory = System.IO.Path.Combine(tyeFolder, @".logs");
    var tyeFilePath = System.IO.Path.Combine(tyeFolder, "tye.yaml");

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
