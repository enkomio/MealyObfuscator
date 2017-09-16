// --------------------------------------------------------------------------------------
// FAKE build script
// --------------------------------------------------------------------------------------

#r @"packages/FAKE/tools/FakeLib.dll"

open System
open System.Collections.Generic
open System.IO

open Fake
open Fake.AssemblyInfoFile
open Fake.ReleaseNotesHelper
 
// The name of the project
let project = "MealyObfuscator"

// Short summary of the project
let summary = "A Mealy automata generator to obfuscate strings."

// Longer description of the project
let description = summary

// List of author names (for NuGet package)
let authors = [ "Enkomio" ]

// File system information
let solutionFile  = "MealyObfuscatorSln.sln"

// Build dir
let buildDir = "./build"

// Package dir
let deployDir = "./deploy"

// Read additional information from the release notes document
let releaseNotesData = 
    let changelogFile = Path.Combine("..", "RELEASE_NOTES.md")
    File.ReadAllLines(changelogFile)
    |> parseAllReleaseNotes

let releaseVersion = (List.head releaseNotesData)
trace("Build release: " + releaseVersion.AssemblyVersion)

let stable = 
    match releaseNotesData |> List.tryFind (fun r -> r.NugetVersion.Contains("-") |> not) with
    | Some stable -> stable
    | _ -> releaseVersion

let genFSAssemblyInfo (projectPath) =
    let projectName = System.IO.Path.GetFileNameWithoutExtension(projectPath)
    let folderName = System.IO.Path.GetFileName(System.IO.Path.GetDirectoryName(projectPath))
    let fileName = folderName @@ "AssemblyInfo.fs"
    CreateFSharpAssemblyInfo fileName
      [ Attribute.Title (projectName)
        Attribute.Product project
        Attribute.Company (authors |> String.concat ", ")
        Attribute.Description summary
        Attribute.Version (releaseVersion.AssemblyVersion + ".*")
        Attribute.FileVersion (releaseVersion.AssemblyVersion + ".*")
        Attribute.InformationalVersion (releaseVersion.NugetVersion + ".*") ]
        
Target "Compile" (fun _ ->
    !! ("/**/*.fsproj")
    |> Seq.iter(fun project ->
        trace("Compile: " + project)
        let fileName = Path.GetFileNameWithoutExtension(project)
        let buildAppDir = Path.Combine(buildDir, fileName)
        ensureDirectory buildAppDir
        MSBuildRelease buildAppDir "Build" [project] |> Log "Build Output: "
    )
)

Target "Test" (fun _ ->
    let mealyObfuscatorTest = Path.Combine(buildDir, "MealyObfuscatorTest", "MealyObfuscatorTest.exe")
    ExecProcess(fun info -> 
        info.FileName <- mealyObfuscatorTest
    )(TimeSpan.FromMinutes 5.0) |> ignore
)

Target "AssemblyInfo" (fun _ ->
  let fsProjs =  !! "*/**/*.fsproj"
  fsProjs |> Seq.iter genFSAssemblyInfo
)

Target "Clean" (fun _ ->
    CleanDir buildDir
    ensureDirectory buildDir

    CleanDir deployDir
    ensureDirectory deployDir
)

// --------------------------------------------------------------------------------------
// Run all targets by default. Invoke 'build <Target>' to override

Target "All" DoNothing

"Clean"
  ==> "AssemblyInfo"
  ==> "Compile"
  ==> "Test"
  ==> "All"

RunTargetOrDefault "All"