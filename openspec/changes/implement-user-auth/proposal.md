## Why

Currently, the Marque backend API has no authentication or authorization mechanism. Private endpoints (such as store management, professional/service updates, and private schedule retrieval) are completely unsecured and rely on the client explicitly passing the user ID in the request payload or route, which is insecure. Adding user authentication via Supabase JWT validation secures these private endpoints and ensures that users can only modify and view resources belonging to their own stores.

## What Changes

- **Add JWT Authentication Middleware**: Configure ASP.NET Core authentication services to validate incoming JWTs issued by Supabase using its public JWKS endpoint.
- **Secure Private API Endpoints**: Apply authorization checks (`[Authorize]`) to all endpoints under `/api/store`, `/api/professional`, `/api/service`, and appointments endpoints managed by the store owner.
- **Extract Authenticated User Identity**: Extract the user's ID (`sub` claim) from the validated JWT and supply it to the controllers.
- **Remove Explicit User ID from Request Payloads**: **BREAKING** Update endpoints and DTOs that previously accepted a client-provided `UserId` (e.g., store creation/update) to automatically use the authenticated user's ID from the JWT claims instead.

## Capabilities

### New Capabilities
- `user-auth`: Authenticate incoming HTTP requests to private endpoints using Supabase JWT tokens and extract the authenticated user ID.

### Modified Capabilities

## Impact

- **Marque.Api**:
  - `Program.cs`: Register JWT Bearer authentication services, configure token validation parameters pointing to the Supabase JWKS/Metadata endpoint, and inject authentication and authorization middlewares into the HTTP pipeline.
  - Controllers: Annotate private controllers with `[Authorize]` and update route handlers to retrieve the authenticated user ID directly from claims.
- **Marque.Application**:
  - Update DTO models (like `CreateStoreInput`, `UpdateStoreInput`) to remove the client-provided `UserId` parameter.
  - Update use cases where necessary to receive the user ID from the API controller.
- **Dependencies**:
  - Added dependency on `Microsoft.AspNetCore.Authentication.JwtBearer` (already present in `Marque.Infrastructure.csproj`).
