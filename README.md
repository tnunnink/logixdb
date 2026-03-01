# LogixDb

A tool for managing and automating ingestion of Rockwell Automation Logix Designer ACD/L5X project files
into a structured and transparent SQL database schema, enabling workflows such as project analysis, validation,
documentation, change tracking, and versioning.

## Motivation

Analyzing and extracting data from Rockwell PLC projects is often slow and manual. Without opening Studio 5000, there is
no straightforward way to centrally manage or review code across multiple projects. For system integrators and
developers, tasks like comparing configurations, validating logic versions, or bulk-extracting data remain difficult.

LogixDb was built to make PLC code analysis and data extraction developer-friendly. By parsing PLC files into a
structured SQL schema, it enables developers and controls engineers to leverage the power of SQL to write custom
queries, views, and procedures for project analysis, validation, and documentation.

## Features

This tool currently offers a couple of entry points for users to work with.

| Entry Point | Description                                                                                                                                                                                                                                                                                               |
|-------------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| **CLI**     | Command-line interface for interactive and scripted operations. Ideal for manual imports, exports, and database management tasks. Supports all core commands including `migrate`, `import`, `export`, `list`, `prune`, `purge`, and `drop`.                                                               |
| **Service** | Windows service for automated background processing. Monitors directories for new ACD/L5X files and automatically converts and ingests them into the database. Also includes an API endpoint for ingestion of files. Useful for continuous integration scenarios and teams using version control systems. |

## Database Providers

This tool currently supports both Microsoft SQL Server and SQLite database providers.

| Provider       | Description                                                                                                                                                                                                                                                                             |
|----------------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| **SQLite**     | Ideal for single-developer or quick analysis scenarios. Free and open source with no additional server-side software required. Developers can quickly transform PLC projects into SQLite databases on the fly. Generated database files can be queried using any preferred client.      |
| **SQL Server** | Designed for team environments, especially those using version control systems like FTAC, Git, or SVN. Enables centralized data management and supports advanced features such as stored procedures, triggers, tSQLt, and custom tooling for enhanced collaboration and data integrity. |

This tool enables automated ingestion of L5X and ACD files into either database provider.

## ACD File Conversion

This project uses the Rockwell Logix Designer SDK to convert ACD files into L5X so that
they can be parsed and ingested. As of now, this conversion is a bit slow as it spins up a headless Studio 5k
instance before saving the project as an L5X. This issue was a driver in building the Windows service
component of the tool so that ACD files could be converted, parsed, and ingested in the background, as changes
are committed to version control.

## Installation

LogixDb is distributed as a single ZIP package containing self-contained executables for both the CLI tool and the
Windows service. No .NET runtime installation is required.

### Prerequisites

- Windows 10 or later
- PowerShell 5.1 or later (for automated installation)
- Rockwell Logix Designer SDK (required for ACD file conversion)

### Quick Install (Recommended)

1. Download the latest release ZIP from the [releases page](https://github.com/tnunnink/LogixDb/releases)
2. Extract the ZIP to a temporary location
3. Open PowerShell as an Administrator
4. Navigate to the extracted directory
5. Unblock the PowerShell script:
   ```powershell
   Unblock-File -Path .\Setup-LogixDb.ps1
   ```
6. Run the installation script:
   ```powershell
   .\Setup-LogixDb.ps1
   ```

## Quick Start

Get up and running with the CLI in seconds using a local SQLite database:

1. **Create and migrate the database**
   ```powershell
   logixdb migrate --connection ".\LogixDb.db"
   ```

2. **Import an L5X or ACD file**
   ```powershell
   logixdb import --connection ".\LogixDb.db" --source "C:\Projects\MyProject.acd"
   ```

3. **List imported snapshots**
   ```powershell
   logixdb list -c ".\LogixDb.db"
   ```

## Scope & Limitations

### Supported Objects

LogixDb currently parses and ingests the following Logix objects:

- **Project Metadata**: Controller info, software revision, and export timestamps.
- **Tags**: Controller and program-scoped tags, including data types and comments.
- **Routines**: Logic content (currently stored as raw XML/L5X segments for further analysis).
- **Add-On Instructions (AOIs)**: Definitions and logic.
- **Tasks & Programs**: Scheduling and organizational structure.

### Limitations

- **Logix Versions**: Supports any L5X version produced by Logix Designer. ACD conversion requires the corresponding
  version of Logix Designer and the SDK to be installed.
- **ACD Conversion Speed**: Converting ACD files involves spinning up a headless Logix Designer instance, which can take
  15–30 seconds per file.
- **Platform**: The CLI can run anywhere .NET is supported, but **ACD conversion is Windows-only** due to its dependency
  on the Logix Designer SDK.

## Feedback

Feedback, bug reports, and feature requests are welcome. Please use
the [GitHub Issues](https://github.com/tnunnink/LogixDb/issues) page to share your thoughts or report problems.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for full details.
