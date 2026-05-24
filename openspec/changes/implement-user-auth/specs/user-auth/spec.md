## ADDED Requirements

### Requirement: Enforce JWT authentication on private endpoints
The system SHALL require a valid JWT issued by Supabase in the `Authorization` header as a Bearer token for all private endpoints.

#### Scenario: Request without token is unauthorized
- **WHEN** a client sends an HTTP request to a private endpoint without an Authorization header
- **THEN** the system returns HTTP 401 Unauthorized

#### Scenario: Request with invalid token is unauthorized
- **WHEN** a client sends an HTTP request to a private endpoint with an invalid or expired JWT
- **THEN** the system returns HTTP 401 Unauthorized

#### Scenario: Request with valid token is authorized
- **WHEN** a client sends an HTTP request to a private endpoint with a valid Supabase JWT
- **THEN** the system allows the request to proceed to the controller action

### Requirement: Resolve authenticated user id from JWT
The API SHALL extract the `sub` claim from the validated Supabase JWT and treat it as the authenticated user's ID.

#### Scenario: Extract user id from JWT claims
- **WHEN** a valid JWT is authenticated by the API
- **THEN** the `sub` claim value is exposed as the authenticated user's ID for authorization and resource ownership verification
