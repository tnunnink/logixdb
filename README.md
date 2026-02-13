# LogixDb

A CLI tool for transforming Rockwell Automation L5X content into a SQL database for analysis, versioning, and change tracking.

## Overview

LogixDb imports Rockwell Logix controller programs (exported as L5X files) into a structured SQL database, enabling:

- **Analysis**: Query and analyze controller logic using SQL
- **Comparison**: Compare snapshots to identify differences
- **Version Control**: Track changes to PLC programs over time
- **Documentation**: Generate reports and documentation from controller data
- **Change Management**: Maintain history of program modifications

Currently supports SQLite with SQL Server support in development.