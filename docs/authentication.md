# Authentication Architecture

## Overview
The application uses an abstract `IAuthenticationService` to manage user identity. This allows for swapping the implementation from a developer/mock mode to a full OIDC (OpenID Connect) flow without changing the UI or consumer logic.

## Current Implementation (Mock)
Currently, `AuthenticationService` is a mock implementation that:
- Simulates a login delay.
- Returns a hardcoded `ClaimsPrincipal` with sample claims (Name: John Doe).
- Manages an `IsAuthenticated` state.

## Future Implementation (OIDC)
To integrate with an external Identity Provider (idp) using OpenIddict:
1. Replace `AuthenticationService` with an OIDC-capable implementation.
2. Use `WebAuthenticationBroker` or a system browser approach to initiate the login flow.
3. Validate the returned JWT (ID Token and Access Token).
4. Hydrate the `ClaimsPrincipal` from the token claims.

## UI Integration
- **MainWindow**: Subscribes to `AuthenticationChanged` event to toggle:
    - "Sign In" button (visible when logged out).
    - "User Avatar" & "Sign Out" button (visible when logged in).
- **PermissionService**: Can be updated to check `IAuthenticationService.IsAuthenticated` before allowing navigation to protected pages.
