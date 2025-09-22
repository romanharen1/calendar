using Art.OnShift.Shared.Interfaces;
using Art.OnShift.Shared.Models;

namespace Art.OnShift.Scheduler.Services;

public class BackgroundEventNotificationService(
    ILogger<BackgroundEventNotificationService> logger,
    IServiceProvider serviceProvider) : BackgroundService
{
    private readonly ILogger<BackgroundEventNotificationService> _logger = logger;
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("🚀 Serviço de notificação de eventos iniciado em background");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
                var externalApiService = scope.ServiceProvider.GetRequiredService<IExternalApiService>();

                await CheckAndNotifyEvents(eventService, externalApiService);

                // Aguarda 1 minuto antes da próxima verificação
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogInformation(ex, "🛑 Serviço de notificação foi cancelado");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erro no serviço de notificação em background");

                // Aguarda um pouco antes de tentar novamente em caso de erro
                await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
            }
        }

        _logger.LogInformation("🏁 Serviço de notificação de eventos finalizado");
    }

    private double DefineNotificationTime(EventModel nextEvent)
    {
        if (nextEvent.Start == null)
        {
            _logger.LogWarning($"⚠️ Evento com horário de início nulo. Evento ID: {nextEvent.Id}");
            return double.MaxValue; // Retorna um valor alto para evitar notificação
        }
        else
        {
            // Se o evento começa entre meia-noite e 8 da manhã, notifica às 18h do dia anterior
            if (nextEvent.Start.Value.Hour < 9)
            {
                return (nextEvent.Start.Value.Hour + 6) * 60;
            }
            else
            {
                // Para outros horários, notifica 2 horas antes
                return 120;
            }
        }
    }

    private async Task CheckAndNotifyEvents(
    IEventService eventService,
    IExternalApiService externalApiService)
    {
        var nextEvent = await eventService.GetNextEventAsync();
        var isAckEntryCreated = false;

        if (nextEvent == null || nextEvent.Start > DateTime.UtcNow.AddHours(14))
        {
            _logger.LogInformation("⏰ Nenhum evento próximo para monitorar - Dentro das próximas 15 horas");
            return;
        }
        
        var eventAckInfo = await eventService.GetAcknowledgeEntryAsync(nextEvent.Id);

        if (eventAckInfo != null)
        {
            if ((eventAckInfo.NotificationSentAt - DateTime.UtcNow).TotalMinutes < 15)
            {
                _logger.LogInformation("⏰ Aguardando para enviar próxima notificação.");
                return;
            }
        }
        else
        {
            isAckEntryCreated = false;
        }


        if (nextEvent.Start != null)
        {
            var minutesUntilEvent = (nextEvent.Start - DateTime.UtcNow).Value.TotalMinutes; // Calcula os minutos restantes até o evento
            var timeToNotify = DefineNotificationTime(nextEvent); // Define o tempo para notificar antes do evento


            // Verifica se é hora de notificar
            if (minutesUntilEvent <= timeToNotify && minutesUntilEvent >= 5) // Notifica se o evento começa dentro do tempo definido e não é muito próximo
            {
                _logger.LogInformation($"📢 Processando evento: {nextEvent.Title} (em {minutesUntilEvent:F1} minutos)");

                var notificationRequest = new EventNotificationRequest
                {
                    EventId = nextEvent.Id,
                    Title = nextEvent.Title ?? "Plantão Não Entitulado",
                    StartTime = nextEvent.Start,
                    EndTime = nextEvent.End,
                    PhoneNumber = nextEvent.Level1?.PhoneNumber,
                    NotificationType = "upcoming",
                    MinutesUntilStart = (int)Math.Ceiling(minutesUntilEvent)
                };

                var success = await externalApiService.NotifyUpcomingEventAsync(notificationRequest);

                if (success)
                {
                    if (!isAckEntryCreated)
                    {
                        await eventService.CreateAcknowledgeEntryAsync(nextEvent);
                    }
                    else
                    {
                        await eventService.UpdateAckMessageTimeAsync(nextEvent.Id);
                    }

                    _logger.LogInformation($"✅ Solicitada a notificaçao do evento: {nextEvent.Title}");
                }
                else
                {
                    _logger.LogWarning($"⚠️ Falha ao notificar evento: {nextEvent.Id} para {nextEvent.Title} iniciando às {nextEvent.Start} (UTC)");
                }
            }
        }
        else
        {
            _logger.LogInformation($"⚠️ Evento com horário de início nulo .Evento ID: {nextEvent.Id}");
        }

    }
}
