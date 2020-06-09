#load "./base.common.cake"

/*
 * Retrieve details about the project that has to be built. This include finding the solution
 * files, project files and test projects
 */


#region Variables
var PackageVersionsXmlPath = "./packageVersions.json";

// String used to search for solution files
var slnSearchString = "./src/**/*.sln";
// String used to search for project files
var projSearchString = "./src/**/*.csproj";
// String used to search for test projects
var testSearchString = "./src/**/*.Tests.csproj";
// Solution files that was found
Dictionary<string, FilePath> solutionFiles;
// Project files that were found split into project names and filepaths
Dictionary<string, Tuple<FilePath,string, List<string>>> projectFiles;
// Test projects that were found split into project names and filepaths
Dictionary<string, FilePath> testFiles;

#endregion

#region Task

Task("LocateFiles")
    .Does(() => {
        var blockText = "LocateFiles";
        StartBlock(blockText);

        solutionFiles = new Dictionary<string, FilePath>();
        projectFiles = new Dictionary<string, Tuple<FilePath,string, List<string>>>();
        testFiles = new Dictionary<string, FilePath>();        

        FindSolutionFiles();
        FindProjectFiles();
        FindTestFiles();

        if(solutionFiles.Count == 0)
        {
            throw new CakeException("No solution file could be found");
        }
        if(projectFiles.Count == 0)
        {
            throw new CakeException("No project files could be found");
        }
        if(testFiles.Count == 0)
        {
            Warning("No test projects were found");
        }

        EndBlock(blockText);
    });

#endregion

#region Private methods

// Find solution files using the slnSearchString
private void FindSolutionFiles()
{
    Information("Searching for Solution files...");
    var solutions = GetFiles(slnSearchString);
    Information("Located Solution files:");
    foreach(var entry in solutions)
    {
        var solutionName = GetSolutionName(entry);
        Information($"{solutionName} - {entry}");

        solutionFiles.Add(solutionName, entry);
    }
}

// Find project files using the projSearchString
private void FindProjectFiles()
{
    Information("Searching for Project files...");
    var projects = GetFiles(projSearchString);
    Information("Found project files:");
    foreach(var project in projects)
    {
        var projName = GetProjectName(project);
        Information("Project: " + projName);
        if(projName.EndsWith(".Tests"))
        {
            continue;
        }


        bool isNewPackageInNuget = false;
        bool isNewPackageInJson = false;

       
        var packagesAndVersions = ParseJsonFromFile(PackageVersionsXmlPath);

        var packageJObject = packagesAndVersions["packages"][projName] as dynamic;
        if(packageJObject == null || packageJObject == default(dynamic)){
            isNewPackageInJson = true;
        }

        var lastPublishedVersion = packageJObject?.lastVersion ?? "";
        var lastPublishedCommitHash = packageJObject?.commitHash ?? "";

        var counter = 1;
        var lastCommit = GitLog(".", counter).FirstOrDefault();
        var lastCommitHash = lastCommit.Sha;
        var lastCommitMessage = lastCommit.Message;
        Information($"lastCommitMessage -> {lastCommitMessage}");

        while(lastCommitMessage.Contains("(CAKE) Auto Commit")){
            counter++;
            Information($"Temp -> {lastPublishedCommitHash}");
            Information($"Temp -> {lastCommitHash}");
            lastCommit = GitLog(".", counter).LastOrDefault();
            lastCommitHash = lastCommit.Sha;
            lastCommitMessage = lastCommit.Message;
        }

        Information($"lastPublishedCommitHash -> {lastPublishedCommitHash}");
        Information($"lastCommitHash -> {lastCommitHash}");

        if(string.IsNullOrEmpty(lastPublishedVersion.ToString())){
            isNewPackageInJson = true;
        }

        if(lastPublishedCommitHash == lastCommitHash){
            throw new CakeException("No changes since last publish");
        }
    
        var versionParts = lastPublishedVersion.ToString().Split('.');
        var lastPart = int.Parse(versionParts[2]) + 1;
        var newVersion = $"{versionParts[0]}.{versionParts[1]}.{lastPart.ToString()}";


        if(isNewPackageInJson){
            var newVersionObj = JObject.Parse($"{{\"lastVersion\": \"{newVersion}\", \"commitHash\": \"{lastCommitHash}\"}}");
            var packagesJObject = packagesAndVersions["packages"] as JObject;

            if(!packagesJObject.Properties().Any()){
                packagesJObject.Add(projName, newVersionObj);
            }else{
                packagesJObject.Properties().LastOrDefault().AddAfterSelf(new JProperty(projName, newVersionObj));
            }
        }else{
            packageJObject.lastVersion = newVersion;
            packageJObject.commitHash = lastCommitHash;
        }
        SerializeJsonToPrettyFile(PackageVersionsXmlPath,packagesAndVersions);

        XmlPoke(
            project,
            "/Project/PropertyGroup/Version",
            newVersion
        );

        var framework = XmlPeek(
            project,
            "/Project/PropertyGroup/TargetFramework/text()"
        );

        var frameworks = XmlPeek(
            project,
            "/Project/PropertyGroup/TargetFrameworks/text()"
        );
        var finalframework = new List<string>{framework};

        if(string.IsNullOrEmpty(framework)){
            finalframework = frameworks.Split(',').ToList();
        }

        Information($"{projName}:{newVersion} - {project}");

        projectFiles.Add(projName, new Tuple<FilePath,string, List<string>>(project,newVersion,finalframework));
    }

    UpdateNuSpecFilesToReferenceLastProjects();
}

private void UpdateNuSpecFilesToReferenceLastProjects(){
    var currentBranch = GitBranchCurrent(".").CanonicalName;
    var isAlpha = currentBranch.ToLower().Contains("alpha");

    foreach(var project in projectFiles)
    {
        Information($"Updating NuSpec File to Reference Last Projects: {project.Key}");
        var nuspecFile = string.Format(nuspecFileString, project.Key);

        foreach(var project2 in projectFiles)
        {
            ReplaceRegexInFiles(nuspecFile,$"(<dependency id=\"{project2.Key}\" version=\")(.*)\"",$"${{1}}{project2.Value.Item2 + (isAlpha? "-alpha":"")}\"");
        }
    }
}

public void CommitChangesAndUpdateVersionFile(){
    GitAddAll(".");
    GitCommit(".","Cake Builder", "Cake@akrual.net", "(CAKE) Auto Commit");
}

public void UpdateVersionFileWithCommitHash(){
    var lastCommitHash = (GitLog(".", 1).FirstOrDefault()).Sha;
    var packagesAndVersions = ParseJsonFromFile(PackageVersionsXmlPath);
    var packagesJObject = packagesAndVersions["packages"] as JObject;
    foreach(var jObject in packagesJObject.Properties()){
        var dinObj  = jObject as dynamic;
        dinObj.commitHash = lastCommitHash;
    }
    SerializeJsonToPrettyFile(PackageVersionsXmlPath,packagesAndVersions);
    Information($"UpdateVersionFileWithCommitHash");
}

// Find test files using the testSearchString
private void FindTestFiles()
{
    Information("Searching for Test files...");
    var testProjects = GetFiles(testSearchString);
    Information("Found test files:");
    foreach(var test in testProjects)
    {
        var projectName = GetProjectName(test);
        Information($"{projectName} - {test}");
        testFiles.Add(projectName, test);
    }
}

// Get project name without .csproj and the folder path
private string GetProjectName(FilePath projPath)
{
    return projPath
        .GetFilename()
        .ToString()
        .Replace(".csproj", "");
}

// Get the solution name without .sln or the folder path
private string GetSolutionName(FilePath solutionPath)
{
    return solutionPath
        .GetFilename()
        .ToString()
        .Replace(".sln", "");
}

#endregion