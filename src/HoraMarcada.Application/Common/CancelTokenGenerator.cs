using System.Security.Cryptography;
using System.Text;
using HoraMarcada.Application.Repositories;

namespace HoraMarcada.Application.Common;

public class CancelTokenGenerator : ICancelTokenGenerator
{
    private const string Charset = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    public string Generate(int length = 5)
    {
        var bytes = RandomNumberGenerator.GetBytes(length);
        var builder = new StringBuilder(length);

        foreach (var value in bytes)
        {
            builder.Append(Charset[value % Charset.Length]);
        }

        return builder.ToString();
    }

    public async Task<string> GenerateUniqueAsync(
        IAppointmentRepository appointmentRepository,
        CancellationToken cancellationToken = default)
    {
        while (true)
        {
            var token = Generate();
            var existing = await appointmentRepository.GetByCancelTokenAsync(token, cancellationToken);

            if (existing is null)
            {
                return token;
            }
        }
    }
}
