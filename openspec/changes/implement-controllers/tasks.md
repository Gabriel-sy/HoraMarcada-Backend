## 1. Packages and Initial Setup

- [x] 1.1 Add FluentValidation package version (11.9.0) to Directory.Packages.props
- [x] 1.2 Add FluentValidation package references to Marque.Api.csproj

## 2. Global Exception Filter and Setup

- [x] 2.1 Create ApiExceptionFilterAttribute under src/Marque.Api/Filters/
- [x] 2.2 Register the global exception filter in src/Marque.Api/Program.cs

## 3. Store Endpoints

- [x] 3.1 Create CreateStoreController and CreateStoreRequestValidator under src/Marque.Api/Controllers/Stores/Create/
- [x] 3.2 Create GetStoreByUserIdController under src/Marque.Api/Controllers/Stores/GetByUserId/
- [x] 3.3 Create UpdateStoreController and UpdateStoreRequestValidator under src/Marque.Api/Controllers/Stores/Update/

## 4. Professional Endpoints

- [x] 4.1 Create AddProfessionalController and AddProfessionalRequestValidator under src/Marque.Api/Controllers/Professionals/Add/
- [x] 4.2 Create ListProfessionalsController under src/Marque.Api/Controllers/Professionals/List/
- [x] 4.3 Create UpdateProfessionalController and UpdateProfessionalRequestValidator under src/Marque.Api/Controllers/Professionals/Update/
- [x] 4.4 Create DeactivateProfessionalController under src/Marque.Api/Controllers/Professionals/Deactivate/

## 5. Service Endpoints

- [x] 5.1 Create AddServiceController and AddServiceRequestValidator under src/Marque.Api/Controllers/Services/Add/
- [x] 5.2 Create ListServicesController under src/Marque.Api/Controllers/Services/List/
- [x] 5.3 Create UpdateServiceController and UpdateServiceRequestValidator under src/Marque.Api/Controllers/Services/Update/
- [x] 5.4 Create ListProfessionalsForServiceController under src/Marque.Api/Controllers/Services/ListProfessionals/

## 6. Availability and Appointment Endpoints

- [x] 6.1 Create GetAvailableSlotsController and GetAvailableSlotsRequestValidator under src/Marque.Api/Controllers/Availability/GetSlots/
- [x] 6.2 Create BookAppointmentController and BookAppointmentRequestValidator under src/Marque.Api/Controllers/Appointments/Book/
- [x] 6.3 Create ListAppointmentsController and ListAppointmentsRequestValidator under src/Marque.Api/Controllers/Appointments/List/
- [x] 6.4 Create CancelAppointmentController and CancelAppointmentRequestValidator under src/Marque.Api/Controllers/Appointments/Cancel/
- [x] 6.5 Create CancelAppointmentByTokenController and CancelAppointmentByTokenRequestValidator under src/Marque.Api/Controllers/Appointments/CancelByToken/
- [x] 6.6 Create CompleteAppointmentController and CompleteAppointmentRequestValidator under src/Marque.Api/Controllers/Appointments/Complete/

## 7. Registration and Solution Verification

- [x] 7.1 Register FluentValidation in src/Marque.Api/Program.cs
- [x] 7.2 Verify that the solution builds successfully and passes existing unit tests
