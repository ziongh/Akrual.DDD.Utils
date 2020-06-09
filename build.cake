#addin "Cake.FileHelpers"
#addin "nuget:https://api.nuget.org/v3/index.json?package=Cake.Git"
#addin "nuget:https://api.nuget.org/v3/index.json?package=Cake.Json"
#addin "nuget:https://api.nuget.org/v3/index.json?package=Newtonsoft.Json&version=11.0.2"

#load "CakeScripts/base.setup.cake"


//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument ("Target", "Package");
var configuration = Argument ("configuration", "Release");

Information ($"Target Selected: {target}");

var buildDir = Directory ("./build") + Directory (configuration);
var publishDir = Directory ("./publish") + Directory (configuration);

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task ("Clean")
    .IsDependentOn ("DiscoverBuildDetails")
    .IsDependentOn ("LocateFiles")
    .Does (() => {
        foreach (var project in projectFiles) {
            var buidlDirectory = project.Value.Item1.GetDirectory ().FullPath + "/bin/";
            var objDirectory = project.Value.Item1.GetDirectory ().FullPath + "/obj/";
            CleanDirectory (buidlDirectory);
            CleanDirectory (objDirectory);
        }

        CleanDirectory (buildDir);
        CleanDirectory (publishDir);
    });

Task ("Build")
    .IsDependentOn ("Clean")
    .Does (() => {
        var blockText = "Build";
        StartBuildBlock (blockText);

        foreach (var projectF in projectFiles) {
            foreach (var framework in projectF.Value.Item3) {
                Information ($"Building ({framework}): {projectF.Key}...");
                var settings = new DotNetCoreBuildSettings {
                    Configuration = configuration,
                    Framework = framework,
                    OutputDirectory = projectF.Value.Item1.GetDirectory () + Directory ("/bin") + Directory ("/" + configuration) + Directory ("/lib") + Directory ("/" + framework)
                };
                DotNetCoreBuild (projectF.Value.Item1.FullPath, settings);
            }
        }
        EndBuildBlock (blockText);
    });

Task ("Publish")
    .IsDependentOn ("Build")
    .Does (() => {
        var blockText = "Build";
        StartBuildBlock (blockText);

        foreach (var solution in solutionFiles) {
            Information ($"Building: {solution.Key}...");
            DotNetCorePublish (
                solution.Value.GetDirectory ().FullPath,
                new DotNetCorePublishSettings {
                    OutputDirectory = publishDir
                }
            );
        }
        EndBuildBlock (blockText);
    });

Task ("Run-Unit-Tests")
    .IsDependentOn ("Build")
    .Does (() => {
        var blockText = "Test";
        StartBuildBlock (blockText);

        foreach (var testProject in testFiles) {
            Information ($"Testing: {testProject.Key}...");
            DotNetCoreTest (testProject.Value.FullPath);
        }
        EndBuildBlock (blockText);
    });

Task ("CheckIfCommitedChanges")
    .Does (() => {
        if (GitHasUncommitedChanges (".") || GitHasUntrackedFiles (".")) {
            throw new CakeException ("There Are Uncommited Changes. Commit before Packaging.");
        }
    });

Task ("Package")
    .IsDependentOn ("CheckIfCommitedChanges")
    .IsDependentOn ("Build")
    .Does (() => {
        EnsureDirectoryExists (packageOutputPath);

        var currentBranch = GitBranchCurrent (".").CanonicalName;
        var isAlpha = currentBranch.ToLower ().Contains ("alpha");

        foreach (var project in projectFiles) {
            Information ($"Generating NuGet package package for {project.Key}");
            var nuspecFile = string.Format (nuspecFileString, project.Key);

            XmlPoke (
                nuspecFile,
                "/package/metadata/version",
                project.Value.Item2 + (isAlpha? "-alpha": "")
            );

            var projectBuildDirectory = project.Value.Item1.GetDirectory () + Directory ("/bin") + Directory ("/" + configuration) + Directory ("/lib");
            foreach (var project2 in projectFiles.Where (s => s.Key != project.Key)) {
                var files = GetFiles ($"{projectBuildDirectory}/*/{project2.Key}.*");
                DeleteFiles (files);
            }

            // NuGetPack(project.Value.Item1.FullPath, GetNuGetPackSettings(project.Value.Item1.GetDirectory().FullPath));	
            NuGetPack (nuspecFile, GetNuGetPackSettings (project.Value.Item1.GetDirectory ().FullPath));
        }
        CommitChangesAndUpdateVersionFile ();
    });

Task ("Publish_Package")
    .IsDependentOn ("Package")
    .Does (() => {
        // NuGet API key
        var apiKey = "oy2gqxynwjcp6ehaq3zqunb4simr3qvedn3whlj6tj6pka";
        var blockText = "NugetPush";
        StartBlock (blockText);

        if (!CheckIfPackagesCanBePushed ()) {
            return;
        }

        PushPackagesToNuget (apiKey);

        EndBlock (blockText);

    });

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget (target);