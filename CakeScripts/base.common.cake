#region Variables

// Indicates that the build is not being executed by a Build system
var runningOnLocal = false;

// Indicates that the build is being executed by a TeamCity server
var runningOnTeamCity = false;

// Indicates that the build is being executed by an AppVeyor server
var runningOnAppVeyor = false;

// Define directories.

var packageOutputPath = Directory ("./package");
var nuspecFileString = "./src/{0}/{0}.nuspec";
#endregion


#region Tasks

Task ("DiscoverBuildDetails")
    .Does(() => {
        var blockText = "DiscoverBuildDetails";
        StartBlock(blockText);

        runningOnLocal = BuildSystem.IsLocalBuild;
        runningOnTeamCity = TeamCity.IsRunningOnTeamCity;
        runningOnAppVeyor = AppVeyor.IsRunningOnAppVeyor;

        Information("Execution environment:");
        Information($"Running Local build: {runningOnLocal}");
        Information($"Running on TeamCity: {runningOnTeamCity}");
        Information($"Running on AppVeyor: {runningOnAppVeyor}");

        EndBlock(blockText);
    });

#endregion

#region Public Method

// Start a log block for build systems that support it
public void StartBlock(string blockName)
{
		if(runningOnTeamCity)
		{
			TeamCity.WriteStartBlock(blockName);
		}
}

// Start a build block for build systems that support it
public void StartBuildBlock(string blockName)
{
	if(runningOnTeamCity)
	{
		TeamCity.WriteStartBuildBlock(blockName);
	}
}

// End a log block for build systems that support it
public void EndBlock(string blockName)
{
	if(runningOnTeamCity)
	{
		TeamCity.WriteEndBlock(blockName);
	}
}

// End a build block for build systems that support it
public void EndBuildBlock(string blockName)
{
	if(runningOnTeamCity)
	{
		TeamCity.WriteEndBuildBlock(blockName);
	}
}

// Push the Version number to the build system
public void PushVersion(string version)
{
	if(runningOnTeamCity)
	{
		TeamCity.SetBuildNumber(version);
	}
	if(runningOnAppVeyor)
	{
		Information("Pushing version to AppVeyor: " + version);
		AppVeyor.UpdateBuildVersion(version);
	}
}

// Push the Test Results to AppVeyor for display purposess
public void PushTestResults(string filePath)
{
	var file = MakeAbsolute(File(filePath));
	if(runningOnAppVeyor)
	{
		AppVeyor.UploadTestResults(file, AppVeyorTestResultsType.NUnit3);
	}
}


// Generates the NuGetPackSettings
public NuGetPackSettings GetNuGetPackSettings(string basePath)
{
    return new NuGetPackSettings 
    { 
        Verbosity = NuGetVerbosity.Detailed,
        OutputDirectory = packageOutputPath,
		BasePath = basePath + "/bin/Release/",
        IncludeReferencedProjects = true,
        ArgumentCustomization = args => args.Append("-properties Configuration=Release")
    };
}


// Check if NuGet packages can be pushed to nuget.org
public bool CheckIfPackagesCanBePushed()
{
    // if(!testPassed)
    // {
    //     Error("Unit tests failed - Not pushing NuGet packages");
    //     return false;
    // }
    
    // apiKey = EnvironmentVariable("NugetKey");
    // if(string.IsNullOrEmpty(apiKey))
    // {
    //     Warning("NuGet API key is empty - Not pushing to NuGet");
    //     return false;
    // }

    return true;
}

// Push NuGet packages to nuget.org
public void PushPackagesToNuget(string apiKey)
{
    foreach(var project in projectFiles)
    {
        // Get the newest (by last write time) to publish
        var newestNupkg = GetFiles ($"{packageOutputPath}/{project.Key}*.nupkg")
            .OrderBy (f => new System.IO.FileInfo (f.FullPath).LastWriteTimeUtc)
            .LastOrDefault();        

        NuGetPush (newestNupkg, new NuGetPushSettings 
        { 
            Verbosity = NuGetVerbosity.Detailed,
            Source = "https://api.nuget.org/v3/index.json",
            ApiKey = apiKey
        });
    }
}


#endregion