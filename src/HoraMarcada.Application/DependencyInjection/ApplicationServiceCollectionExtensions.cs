using HoraMarcada.Application.Common;
using HoraMarcada.Application.Policies;
using HoraMarcada.Application.UseCases.Appointments;
using HoraMarcada.Application.UseCases.Availability;
using HoraMarcada.Application.UseCases.Professionals;
using HoraMarcada.Application.UseCases.Services;
using HoraMarcada.Application.UseCases.Stores;
using HoraMarcada.Application.UseCases.TimeBlocks;
using Microsoft.Extensions.DependencyInjection;

namespace HoraMarcada.Application.DependencyInjection;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ITimeProvider, SystemTimeProvider>();
        services.AddScoped<IStoreOwnershipPolicy, StoreOwnershipPolicy>();

        services.AddScoped<CreateStoreUseCase>();
        services.AddScoped<GetStoreByUserIdUseCase>();
        services.AddScoped<GetStoreBySlugUseCase>();
        services.AddScoped<UpdateStoreUseCase>();

        services.AddScoped<AddProfessionalUseCase>();
        services.AddScoped<ListProfessionalsUseCase>();
        services.AddScoped<UpdateProfessionalUseCase>();
        services.AddScoped<DeactivateProfessionalUseCase>();

        services.AddScoped<AddServiceUseCase>();
        services.AddScoped<ListServicesUseCase>();
        services.AddScoped<UpdateServiceUseCase>();
        services.AddScoped<DeleteServiceUseCase>();
        services.AddScoped<ListProfessionalsForServiceUseCase>();

        services.AddScoped<GetAvailableSlotsUseCase>();

        services.AddScoped<CreateTimeBlockUseCase>();
        services.AddScoped<DeleteTimeBlockUseCase>();
        services.AddScoped<ListTimeBlocksUseCase>();

        services.AddScoped<BookAppointmentUseCase>();
        services.AddScoped<ListAppointmentsUseCase>();
        services.AddScoped<CancelAppointmentUseCase>();
        services.AddScoped<CancelAppointmentByTokenUseCase>();
        services.AddScoped<CompleteAppointmentUseCase>();

        return services;
    }
}
