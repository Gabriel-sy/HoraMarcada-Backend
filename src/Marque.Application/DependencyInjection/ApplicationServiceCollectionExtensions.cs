using Marque.Application.Common;
using Marque.Application.Policies;
using Marque.Application.UseCases.Appointments;
using Marque.Application.UseCases.Availability;
using Marque.Application.UseCases.Professionals;
using Marque.Application.UseCases.Services;
using Marque.Application.UseCases.Stores;
using Microsoft.Extensions.DependencyInjection;

namespace Marque.Application.DependencyInjection;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ITimeProvider, SystemTimeProvider>();
        services.AddScoped<IStoreOwnershipPolicy, StoreOwnershipPolicy>();

        services.AddScoped<CreateStoreUseCase>();
        services.AddScoped<GetStoreByUserIdUseCase>();
        services.AddScoped<UpdateStoreUseCase>();

        services.AddScoped<AddProfessionalUseCase>();
        services.AddScoped<ListProfessionalsUseCase>();
        services.AddScoped<UpdateProfessionalUseCase>();
        services.AddScoped<DeactivateProfessionalUseCase>();

        services.AddScoped<AddServiceUseCase>();
        services.AddScoped<ListServicesUseCase>();
        services.AddScoped<UpdateServiceUseCase>();
        services.AddScoped<ListProfessionalsForServiceUseCase>();

        services.AddScoped<GetAvailableSlotsUseCase>();

        services.AddScoped<BookAppointmentUseCase>();
        services.AddScoped<ListAppointmentsUseCase>();
        services.AddScoped<CancelAppointmentUseCase>();
        services.AddScoped<CancelAppointmentByTokenUseCase>();
        services.AddScoped<CompleteAppointmentUseCase>();

        return services;
    }
}
