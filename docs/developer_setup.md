# Onyx OMS - Developer Setup & Configuration Guide

Welcome to the Onyx Order Management System repository. This guide covers the local setup required to run and build the application.

## 📦 Repositories

This system spans three separate repositories. Clone all three:

| Repository | Description | URL |
|---|---|---|
| **Onyx.IdP** | Identity Provider (OpenIddict-based auth server) | https://github.com/gihan-aj/Onyx.IdP |
| **Onyx.Oms** | OMS API backend | https://github.com/gihan-aj/Onyx.Oms |
| **Onyx.Oms.Client** | WinUI 3 Desktop client | https://github.com/gihan-aj/Onyx.Oms.Client |

---

## 🏗️ Architecture Overview
Onyx OMS is a WinUI 3 Desktop application that acts as a host for its own local microservices. When the desktop client launches, it silently starts two background processes using the dynamic port range:
1. **Onyx IdP (Identity Provider):** `http://localhost:54320`
2. **Onyx OMS API:** `http://localhost:54321`

The client communicates with these services locally. To run the packaged app, compiled versions of these services must be placed inside the desktop client's `BackendServices` folder.

---

## 🚀 Step 1: Clone the Repositories
Clone all three repositories and ensure you are on the primary development branch for each.

**Identity Provider:**
```bash
git clone https://github.com/gihan-aj/Onyx.IdP.git
cd Onyx.IdP
git checkout master
```

**OMS API:**
```bash
git clone https://github.com/gihan-aj/Onyx.Oms.git
cd Onyx.Oms
git checkout master
```

**Desktop Client:**
```bash
git clone https://github.com/gihan-aj/Onyx.Oms.Client.git
cd Onyx.Oms.Client
git checkout master
```

---

## ⚙️ Step 2: Secret Configuration Files
Because we do not track production secrets or passwords in Git, you must manually create the configuration files for the backend services.

### 1. Identity Provider (`Onyx.IdP.Web`)
Create a file named `appsettings.Production.json` in the root of the `Onyx.IdP.Web` project and paste the following. Reach out to the repository administrator for the actual passwords/secrets to fill in the empty strings:

```json
{
  "OpenIddictClients": {
    "OmsApi": {
      "ClientSecret": "oms-api-super-secret"
    }
  },
  "Certificate": {
    "Password": "SuperSecretPassword123!"
  },
  "EmailSettings": {
    "Server": "smtp.gmail.com",
    "Port": 587,
    "SenderName": "Onyx Identity",
    "SenderEmail": "userforge6@gmail.com",
    "Username": "userforge6@gmail.com",
    "Password": "hpjk sqof cxjc yyll"
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=OnyxIdP;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}
```

### 2. OMS API (`Onyx.Oms.Api`)
Create a file named `appsettings.Production.json` in the root of the `Onyx.Oms.Api` project:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=OnyxOms;Trusted_Connection=True;MultipleActiveResultSets=true"
  },
  "Authentication": {
    "Audience": "onyx.oms.api",
    "ClientSecret": "oms-api-super-secret"
  }
}
```

Additionally, right-click on `Onyx.Oms.Api` → `Manage User Secrets` and add the following to store authentication secrets locally without committing them:

```json
{
  "Authentication": {
    "Audience": "onyx.oms.api",
    "ClientSecret": "oms-api-super-secret"
  }
}
```

---

## 🔐 Step 3: OpenIddict Certificate Generation
The Identity Provider requires an encryption/signing certificate (`openiddict-cert.pfx`) to generate JWTs. This file is `.gitignore`d for security.

To generate your own local certificate for development, open PowerShell in the `Onyx.IdP.Web` project directory and run:

```powershell
# Generates a self-signed certificate valid for 5 years
$cert = New-SelfSignedCertificate -Subject "CN=OnyxOpenIddict" -CertStoreLocation "Cert:\CurrentUser\My" -KeyExportPolicy Exportable -KeySpec Signature -NotAfter (Get-Date).AddYears(5)

# Exports the certificate to a .pfx file (Replace 'YourPasswordHere' with the password in your appsettings.json)
$pwd = ConvertTo-SecureString -String "SuperSecretPassword123!" -Force -AsPlainText
Export-PfxCertificate -Cert $cert -FilePath "openiddict-cert.pfx" -Password $pwd
```
*Note: Ensure the password you use here exactly matches the `Certificate:Password` value in your IdP's `appsettings.Production.json`.*

---

## 🗄️ Step 4: Local Database Initialization (Optional)
The application uses SQL Server LocalDB. Entity Framework migrations are **automatically applied on startup** when both the IdP and OMS API projects are running, so this step is not strictly required.

However, if you prefer to initialize the databases manually before running the projects, you can apply migrations via the **Package Manager Console** in Visual Studio:

1. Open the **Package Manager Console** (`Tools` → `NuGet Package Manager` → `Package Manager Console`).
2. Set the **Default Project** dropdown (top of the console) to the infrastructure project you want to target.

**Initialize the IdP Database:**
- Set Default Project to `Onyx.IdP.Infrastructure`
- Run:
```powershell
Update-Database
```

**Initialize the OMS Database:**
- Set Default Project to `Onyx.Oms.Infrastructure`
- Run:
```powershell
Update-Database
```

---

## 📦 Step 5: Packaging the Background Services
To test the full WinUI 3 deployment — where the desktop app silently launches the backend services on startup — you must publish self-contained executables for the IdP and OMS API, then copy them into the desktop client project.

Since these are **three separate repositories**, you will publish each service from within its own project folder.

---

### Part A: Publish the IdP and API (from the `Onyx.IdP` and `Onyx.Oms` repos)

**1. Publish the Identity Provider**

Open a PowerShell terminal **inside the `Onyx.IdP.Web` project folder** (not the solution root) and run:

```powershell
dotnet publish -c Release -r win-x64 --self-contained true -o ./InstallerStaging/IdP
```

This will produce a self-contained Windows x64 executable under `Onyx.IdP.Web/InstallerStaging/IdP/`.

**2. Publish the OMS API**

Open a PowerShell terminal **inside the `Onyx.Oms.Api` project folder** (not the solution root) and run:

```powershell
dotnet publish -c Release -r win-x64 --self-contained true -o ./InstallerStaging/API
```

This will produce a self-contained Windows x64 executable under `Onyx.Oms.Api/InstallerStaging/API/`.

---

### Part B: Copy the Published Output into the Desktop Client

Once both services are published, copy the output folders into the `BackendServices` folder of the `Onyx.Oms.Client.Desktop` project:

**1. Copy the IdP output:**
- **Source:** `<Onyx.IdP repo>\Onyx.IdP.Web\InstallerStaging\IdP\`
- **Destination:** `<Onyx.Oms.Client repo>\src\Onyx.Oms.Client.Desktop\BackendServices\IdP\`

**2. Copy the API output:**
- **Source:** `<Onyx.Oms repo>\Onyx.Oms.Api\InstallerStaging\API\`
- **Destination:** `<Onyx.Oms.Client repo>\src\Onyx.Oms.Client.Desktop\BackendServices\API\`

> **Note:** Replace the entire contents of each destination folder if you are updating an existing build.

---

Once both `IdP` and `API` folders are in place under `BackendServices`, running or packaging the `Onyx.Oms.Client.Desktop` application will successfully launch the entire microservice stack.

---

## 📝 Notes & Troubleshooting
* **`Onyx.Oms.Client.Desktop_TemporaryKey.pfx`**: If you see this file generated in the desktop project, ignore it. It is automatically created by Visual Studio to sign the local MSIX package during debugging and is safely tracked in `.gitignore`.
* **Port Conflicts:** If the background services fail to start, ensure no other local applications are occupying ports `54320` or `54321`.
* **`InstallerStaging` folder:** The `InstallerStaging` output directories inside the IdP and API projects are temporary staging areas only. Do not commit their contents to Git — they should be listed in each repo's `.gitignore`.