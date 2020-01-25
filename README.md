# Duck DNS Updater
A Windows worker service to update IP adressses of subdomains on Duck DNS.

The IP address is detected automatically by Duck DNS.

## Installation
1. Grab the latest version from the [Releases](https://github.com/austins/DuckDnsUpdater/releases) page.
2. Extract the files into any directory.
3. [Configure the settings](#configuration) for your subdomains and token.
4. Run DuckDnsUpdater.exe or [create a Windows service](#create-a-windows-service).

## Configuration
Create a file named `appsettings.Production.json`

Find your Duck DNS token on your Duck DNS profile page.

Append one of the following examples and replace the provided values with your settings:

#### Single Subdomain
If you only have one subdomain, the settings would look like this:
```
{
  "Subdomains": ["subdomain"],
  "Token": "example"
}
```

#### Multiple Subdomains
Multiple subdomains are supported:
```json
{
  "Subdomains": ["subdomain1", "subdomain2"],
  "Token": "example"
}
```

## Create a Windows Service
1. Open a command prompt as administrator.
2. Run `sc create DuckDnsUpdater binpath= "C:\path\to\DuckDnsUpdater.exe" start= auto`. Replace binpath argument with the location of DuckDnsUpdater.exe. Note: a space is required between the argument's equal sign and the value.
3. Run `sc start DuckDnsUpdater`

To delete the service use: `sc delete DuckDnsUpdater`
