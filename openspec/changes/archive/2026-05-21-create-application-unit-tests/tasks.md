## 1. Project Setup and Configuration

- [x] 1.1 Add test package versions to `Directory.Packages.props`
- [x] 1.2 Create the test project file `tests/Marque.UnitTests/Marque.UnitTests.csproj` referencing `Marque.Application`
- [x] 1.3 Add `Marque.UnitTests.csproj` to the solution file `Marque.sln`

## 2. Store Use Cases Unit Tests

- [x] 2.1 Implement `CreateStoreUseCaseTests.cs`
- [x] 2.2 Implement `GetStoreByUserIdUseCaseTests.cs`
- [x] 2.3 Implement `UpdateStoreUseCaseTests.cs`

## 3. Service Use Cases Unit Tests

- [x] 3.1 Implement `AddServiceUseCaseTests.cs`
- [x] 3.2 Implement `ListProfessionalsForServiceUseCaseTests.cs`
- [x] 3.3 Implement `ListServicesUseCaseTests.cs`
- [x] 3.4 Implement `UpdateServiceUseCaseTests.cs`

## 4. Professional Use Cases Unit Tests

- [x] 4.1 Implement `AddProfessionalUseCaseTests.cs`
- [x] 4.2 Implement `DeactivateProfessionalUseCaseTests.cs`
- [x] 4.3 Implement `ListProfessionalsUseCaseTests.cs`
- [x] 4.4 Implement `UpdateProfessionalUseCaseTests.cs`

## 5. Availability Use Cases Unit Tests

- [x] 5.1 Implement `GetAvailableSlotsUseCaseTests.cs`

## 6. Appointment Use Cases Unit Tests

- [x] 6.1 Implement `BookAppointmentUseCaseTests.cs`
- [x] 6.2 Implement `CancelAppointmentByTokenUseCaseTests.cs`
- [x] 6.3 Implement `CancelAppointmentUseCaseTests.cs`
- [x] 6.4 Implement `CompleteAppointmentUseCaseTests.cs`
- [x] 6.5 Implement `ListAppointmentsUseCaseTests.cs`

## 7. Verification

- [x] 7.1 Execute `dotnet test` and verify all tests pass successfully
