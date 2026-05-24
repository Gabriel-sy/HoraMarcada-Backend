## 1. Authentication Configuration

- [x] 1.1 Create ClaimsPrincipalExtensions to extract user ID from JWT claims
- [x] 1.2 Configure JWT Bearer authentication services in Infrastructure Dependency Injection
- [x] 1.3 Update appsettings.json to include Supabase Url configuration
- [x] 1.4 Register authentication and authorization middlewares in Program.cs
- [x] 1.5 Configure Swagger to support JWT Bearer authorization header

## 2. Secure Store Controllers

- [x] 2.1 Apply Authorize attribute to CreateStoreController and resolve UserId from claims
- [x] 2.2 Apply Authorize attribute to UpdateStoreController and resolve UserId from claims
- [x] 2.3 Apply Authorize attribute to GetStoreByUserIdController and validate request UserId matches claims

## 3. Secure Professional Controllers

- [x] 3.1 Apply Authorize attribute to AddProfessionalController and resolve UserId from claims
- [x] 3.2 Apply Authorize attribute to UpdateProfessionalController and resolve UserId from claims
- [x] 3.3 Apply Authorize attribute to DeactivateProfessionalController and resolve UserId from claims

## 4. Secure Service Controllers

- [x] 4.1 Apply Authorize attribute to AddServiceController and resolve UserId from claims
- [x] 4.2 Apply Authorize attribute to UpdateServiceController and resolve UserId from claims

## 5. Secure Appointment Controllers

- [x] 5.1 Apply Authorize attribute to ListAppointmentsController and resolve UserId from claims
- [x] 5.2 Apply Authorize attribute to CancelAppointmentController and resolve UserId from claims
- [x] 5.3 Apply Authorize attribute to CompleteAppointmentController and resolve UserId from claims
