# Onyx.Oms.Client

> A self-contained WinUI 3 desktop client for running a full multi-service Order Management System on a single machine — built for a real Sri Lankan apparel business selling through Facebook and WhatsApp.

Onyx OMS was built to solve a very specific, very real problem: a clothing business taking orders manually through social media DMs, tracking stock in spreadsheets, and losing track of who owed what. Onyx.Oms.Client is the desktop app the business owner actually opens every day — but under the hood it's the front end for a small distributed system, packaged so a non-technical user never has to know that.

## What it does

- **Order lifecycle management** — from a DM inquiry to a confirmed order, through production/procurement for out-of-stock items, packing, courier handoff, delivery, and payment reconciliation.
- **Stock-aware but stock-flexible ordering** — orders can be confirmed even when items are out of stock; the system can raise Production or Procurement tasks for the shortfall instead of blocking the sale.
- **Manual payment tracking with COD-aware balances** — supports Cash on Delivery and advance payment flows, with shipping fees correctly deducted from COD balances due.
- **Fulfillment dashboard** — separates the production backlog from the procurement/reorder backlog so the owner always knows what needs to be made vs. bought.
- **Courier & invoicing workflow** — waybill/tracking number capture, and invoice/shipping-bill generation with optional WhatsApp Business delivery updates.
- **Financial reporting** — expense tracking against configurable categories and monthly financial summaries.

## Architecture: one app, three services

This is the part that made the project genuinely interesting to build. Rather than shipping a client that talks to a server the user has to manage separately, `Onyx.Oms.Client` **orchestrates its own backend**:

- On launch, the client starts the **Onyx.Oms API** (the order/catalog/payments backend) and **Onyx.IdP** (the OpenID Connect identity provider) as managed background processes.
- Both services shut down cleanly when the app is closed — the user just sees "the app," never "three things I need to keep running."
- This let the system be architected the way a real multi-tenant SaaS product would be from day one (see [Onyx.Oms](https://github.com/gihan-aj/Onyx.Oms) and [Onyx.IdP](https://github.com/gihan-aj/Onyx.IdP) below), while still shipping as a simple local install for the MVP.

## Data safety: automatic, cloud-synced backups

Since this runs on a single local machine for a business that can't afford to lose order data:

- A background service periodically backs up both SQL databases to a **user-selected location** — typically a synced folder (OneDrive, Google Drive, etc.), so backups propagate to the cloud automatically without any custom cloud integration.
- An additional backup is triggered **on application close**, so the worst-case data loss window is a single session rather than a whole backup interval.

## Tech stack

- **UI:** WinUI 3, .NET 10
- **Bundled services:** ASP.NET Core (Onyx.Oms API, Onyx.IdP) launched and supervised as child processes
- **Auth:** OpenID Connect against the bundled Onyx.IdP instance
- **Persistence:** SQL Server (local), with automated backup service

## Screenshots

*(screenshots of the dashboard, order creation flow, and fulfillment backlog)*

## Related repositories

- [`Onyx.Oms`](https://github.com/gihan-aj/Onyx.Oms) — the order management backend (vertical slice architecture, CQRS, multi-tenant)
- [`Onyx.IdP`](https://github.com/gihan-aj/Onyx.IdP) — the OpenID Connect identity provider (OpenIddict, vertical slice architecture)

## Known limitations & roadmap

- **Meta/WhatsApp integration is partial by design.** Full WhatsApp Business API automation requires a cloud-hosted webhook endpoint, which isn't possible from a local desktop app. The current integration sends status updates but requires the business owner to configure their own WhatsApp Business Account and message templates.
- **Courier API integration was deferred**, not skipped by accident — the business ended up using Sri Lanka Post, which has no public integration API, and other courier options weren't reliable enough to justify integrating against undocumented APIs without an active account to test with. This can be added without touching order logic once a suitable courier is identified.
- **SaaS migration is in progress** as a separate effort — moving this same domain model to a cloud-hosted, multi-tenant product (Clerk auth, Neon Postgres, Railway, Vercel).

## Packaging & distribution

Onyx.Oms.Client ships as a single Windows app package that bundles both backend services:

1. `Onyx.Oms` and `Onyx.IdP` are each built as standalone releases.
2. The published output of each is copied into the client project under `BackendServices/API` and `BackendServices/IdP` respectively.
3. The whole thing is packaged into an MSIX using Visual Studio's **Create App Packages** flow, so the end user installs one package and gets the desktop UI plus both backend services with no separate setup.

## Licensing

Since this is distributed to a real paying business rather than run from source, the client includes a simple hardware-locked license check:

1. On first launch, the app reads a hardware identifier from the machine and displays it to the user.
2. The user sends that hardware ID to the publisher, who runs a small internal license-generator tool that RSA-signs the hardware ID (SHA-256 / PKCS#1) and produces a `license.key` file.
3. The user places that key file on their machine. On every launch, the app verifies the signature against the embedded public key and the local hardware ID before allowing access past login — if the check fails, the app won't proceed to the dashboard or to the Login.

This keeps license verification fully offline (no license server to run or maintain) while still tying each install to a specific machine.

## Getting started

1. Download the installer package (distributed via Google Drive — the app isn't published to the Microsoft Store).
2. Run `install_oms.bat`. This will:
   - Install **SQL Server LocalDB** if it isn't already present on the machine.
   - Install the app's MSIX package.
3. Launch the app, enter your hardware ID when prompted, and send it to receive your license key (see [Licensing](#licensing)).
4. Once licensed, log in and the bundled backend services will start automatically.

## License

MIT
