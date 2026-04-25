# Onyx OMS - Developer Setup & Configuration Guide

Welcome to the Onyx Order Management System repository. This guide covers the local setup required to run and build the application. 

## 🏗️ Architecture Overview
Onyx OMS is a WinUI 3 Desktop application that acts as a host for its own local microservices. When the desktop client launches, it silently starts two background processes using the dynamic port range:
1. **Onyx IdP (Identity Provider):** `http://localhost:54320`
2. **Onyx OMS API:** `http://localhost:54321`

The client communicates with these services locally. To run the packaged app, compiled versions of these services must be placed inside the desktop client's `BackendServices` folder.

---

## 🚀 Step 1: Clone the Repository
Clone the repository and ensure you are on the primary development branch.
```bash
git clone <YOUR_REPO_URL>
cd Onyx.Oms
git checkout master
```

---

## ⚙️ Step 2: Secret Configuration Files
Because we do not track production secrets or passwords in Git, you must manually create the configuration files for the backend services.

### 1. Identity Provider (`Onyx.IdP.Web`)
Create a file named `appsettings.Production.json` in the root of the IdP project and paste the following. Reach out to the repository administrator for the actual passwords/secrets to fill in the empty strings:

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
Create a file named `appsettings.Production.json` in the root of the API project:

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

---

## 🔐 Step 3: OpenIddict Certificate Generation
The Identity Provider requires an encryption/signing certificate (`openiddict-cert.pfx`) to generate JWTs. This file is `.gitignore`d for security. 

To generate your own local certificate for development, open PowerShell in the `Onyx.IdP.Web` directory and run:

```powershell
# Generates a self-signed certificate valid for 5 years
$cert = New-SelfSignedCertificate -Subject "CN=OnyxOpenIddict" -CertStoreLocation "Cert:\CurrentUser\My" -KeyExportPolicy Exportable -KeySpec Signature -NotAfter (Get-Date).AddYears(5)

# Exports the certificate to a .pfx file (Replace 'YourPasswordHere' with the password in your appsettings.json)
$pwd = ConvertTo-SecureString -String "YourPasswordHere" -Force -AsPlainText
Export-PfxCertificate -Cert $cert -FilePath "openiddict-cert.pfx" -Password $pwd
```
*Note: Ensure the password you use here exactly matches the `Certificate:Password` value in your IdP's `appsettings.Production.json`.*

---

## 🗄️ Step 4: Local Database Initialization
The application uses SQL Server LocalDB. Because database files (`.mdf`/`.ldf`) are not tracked in Git, you must apply the Entity Framework migrations to create the schemas on your local machine.

Open your terminal and run the following commands:

**Initialize the IdP Database:**
```bash
cd <Path_To_IdP_Project>
dotnet ef database update
```

**Initialize the OMS Database:**
```bash
cd <Path_To_API_Project>
dotnet ef database update
```

---

## 📦 Step 5: Packaging the Background Services
To test the full WinUI 3 deployment (where the desktop app launches the background processes), you must publish the APIs into the desktop project's `BackendServices` folder.

Run these commands from the root directory of your solution:

**Publish the Identity Provider:**
```bash
dotnet publish Onyx.IdP.Web\Onyx.IdP.Web.csproj -c Release -o Onyx.Oms.Client.Desktop\BackendServices\IdP
```

**Publish the OMS API:**
```bash
dotnet publish Onyx.Oms.Api\Onyx.Oms.Api.csproj -c Release -o Onyx.Oms.Client.Desktop\BackendServices\API
```
Once these files are in place, running or packaging the `Onyx.Oms.Client.Desktop` application will successfully launch the entire microservice stack.

---

## 📝 Notes & Troubleshooting
* **`Onyx.Oms.Client.Desktop_TemporaryKey.pfx`**: If you see this file generated in the desktop project, ignore it. It is automatically created by Visual Studio to sign the local MSIX package during debugging and is safely tracked in `.gitignore`.
* **Port Conflicts:** If the background services fail to start, ensure no other local applications are occupying ports `54320` or `54321`.