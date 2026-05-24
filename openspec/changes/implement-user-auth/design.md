## Context

The HoraMarcada backend API requires security for its administrative endpoints. A store owner (entrepreneur) needs to authenticate, and their authenticated session must restrict operations to their own store. Currently, endpoints rely on `UserId` values supplied in the request bodies or route paths, allowing any client to perform operations on behalf of any user.

## Goals / Non-Goals

**Goals:**
- Enable JWT Bearer token authentication in ASP.NET Core using Supabase's authentication service.
- Secure store, professional, service, and owner-managed appointment endpoints.
- Retrieve the authenticated user ID (`sub` claim) securely from the token.
- Update Swagger configuration to support JWT authentication for testing.

**Non-Goals:**
- Implementing frontend authentication flow (this is handled by the Angular client).
- Managing user registration or login flows in the ASP.NET Core API (handled by Supabase directly).
- Implementing multi-tenant access controls beyond verifying store ownership using the extracted user ID.

## Decisions

### 1. Asymmetric JWT Validation via JWKS Endpoint
- **Option A (Shared Secret)**: Configure symmetric validation using a shared JWT Secret from Supabase.
- **Option B (Asymmetric JWKS)**: Configure asymmetric validation using Supabase's OIDC metadata address (`/.well-known/openid-configuration`).
- **Decision**: Option B. It is more secure, supports automatic key rotation, and does not require storing a raw JWT secret on the backend. We will retrieve the Supabase project URL from the configuration (`Supabase:Url`).

### 2. DTO Cleanups and Authenticated Identity Resolution
- **Decision**: Remove the `UserId` property from incoming API requests/DTOs (like `CreateStoreInput` and `UpdateStoreInput`). Instead, extract the user's ID (`sub` claim) from the authenticated `HttpContext.User` claims principal inside the controller and pass it to the application layer. This prevents client spoofing.
- **Implementation**: Write a helper extension method on `ClaimsPrincipal` to retrieve and parse the user ID from the `sub` claim.

### 3. Securing Private Endpoints
- **Decision**: Apply the `[Authorize]` attribute to private controllers. Keep the public scheduling and cancellation endpoints anonymous (`[AllowAnonymous]` or without `[Authorize]`).

### 4. Swagger / OpenAPI Integration
- **Decision**: Add a Security Definition for JWT Bearer token authentication to the Swagger generator configuration in `Program.cs`. This enables developers to attach an authorization token in the Swagger UI.

## Risks / Trade-offs

- **Risk**: JWT token expiration. If the frontend sends an expired token, the backend will return a 401.
  - *Mitigation*: The frontend must refresh the JWT token using Supabase SDK before making API calls.
- **Risk**: Key rotation latency. If Supabase rotates keys, the backend needs to fetch the new keys.
  - *Mitigation*: The JWKS/OIDC handler in ASP.NET Core automatically handles key caching and fetches new keys when signature validation fails.
