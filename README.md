# HigginsSoft.Project Utility
This project is the source code for the HigginsSoft.ProjectUtility DotNet tool. 

This tool is used to perform various tasks on a project. 

The tool is designed to be used in a CI/CD pipeline to automate tasks such as versioning, updating dependencies, and creating release notes.

# Build Status And Stats

[![Build](https://github.com/alexhiggins732/HigginsSoft.ProjectUtility/actions/workflows/master.yml/badge.svg)](https://github.com/alexhiggins732/IdentityServer8/actions/workflows/master.yml)
[![CodeQL](https://github.com/alexhiggins732/IdentityServer8/actions/workflows/codeql.yml/badge.svg?branch=master)](https://github.com/alexhiggins732/IdentityServer8/actions/workflows/codeql.yml)
[![CI/CD|Build](https://github.com/alexhiggins732/IdentityServer8/actions/workflows/pre-release.yml/badge.svg)](https://github.com/alexhiggins732/IdentityServer8/actions/workflows/pre-release.yml)

#Usage

``` pwsh
dotnet tool install --global HigginsSoft.ProjectUtility 

View the help

``` pwsh
projectutility <command>

```

#Disclaims: This code is ALPHA and is not ready for production use.

Download the latest release from the releases page or clone the repository, build the project and step through the code in your debugger to assure expected behavior before using the dotnet tool directly.
Commands:
  // Applies the text in licenseheader.txt to all .cs files in the project's directory structure  
  projectutility license [path/to/project] [path/to/licenseheader.txt]

  // Performs clean of the project directory tree by removing bin, obj, lib, and nuget directories.
  projectutility clean [path/to/project]         Display more information on a specific command.

  // Performs clean of the project directory tree by removing bin, obj, lib, and nuget directories.
  projectutility ImplicitUsings [path/to/project]    Updates the csproj files to use `ImplicitUsings` by combined all the using statements found in cs files into a single `GlobalUsings.cs` file per project.
  projectutility RepoUsing [path/to/project]  Merges all using statements from all cs into a single `RepoUsings.cs` that can be referenced as a `<compile include="path\to\RepoUsings.cs" link="RepoUsings.cs />` 
