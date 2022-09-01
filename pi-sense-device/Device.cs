using dtmi_rido_pnp_sensehat;
using Iot.Device.SenseHat;
using Rido.MqttCore.PnP;
using System.Runtime.InteropServices;
using Color = System.Drawing.Color;

namespace pi_sense_device;

public class Device : BackgroundService
{
    private Isensehat? client;

    private readonly ILogger<Device> _logger;
    private readonly IConfiguration _configuration;

    private const int default_interval = 5;

    public Device(ILogger<Device> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogWarning("Connecting..");
        client = await new SenseHatFactory(_configuration).CreateSenseHatClientAsync(_configuration.GetConnectionString("cs"), stoppingToken);
        _logger.LogWarning($"Connected to {client.Connection.ConnectionSettings}");

        client.Property_interval.OnProperty_Updated = Property_interval_UpdateHandler;
        client.Command_ChangeLCDColor.OnCmdDelegate = Cmd_ChangeLCDColor_Handler;

        await client.Property_interval.InitPropertyAsync(client.InitialState, default_interval, stoppingToken);
        await client.Property_interval.ReportPropertyAsync(stoppingToken);

        client.Property_piri.PropertyValue = $"os: {Environment.OSVersion}, proc: {RuntimeInformation.ProcessArchitecture}, clr: {Environment.Version}";
        await client.Property_piri.ReportPropertyAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            ArgumentNullException.ThrowIfNull(client);
            if (RuntimeInformation.ProcessArchitecture == Architecture.Arm)
            {
                using SenseHat sh = new SenseHat();
                await client.Telemetry_t1.SendTelemetryAsync(sh.Temperature.DegreesCelsius, stoppingToken);
                await client.Telemetry_t2.SendTelemetryAsync(sh.Temperature2.DegreesCelsius, stoppingToken);
                await client.Telemetry_h.SendTelemetryAsync(sh.Humidity.Percent, stoppingToken);
                await client.Telemetry_p.SendTelemetryAsync(sh.Pressure.Pascals, stoppingToken);
            }    
            else
            {
                await client.Telemetry_t1.SendTelemetryAsync(Environment.WorkingSet, stoppingToken);
                await client.Telemetry_t2.SendTelemetryAsync(Environment.WorkingSet, stoppingToken);
                await client.Telemetry_h.SendTelemetryAsync(Environment.WorkingSet, stoppingToken);
                await client.Telemetry_p.SendTelemetryAsync(Environment.WorkingSet, stoppingToken);
                _logger.LogWarning("not running in ARM");
            }
            //await client.Telemetry_m.SendTelemetryAsync(sh.MagneticInduction., stoppingToken);
            var interval = client?.Property_interval.PropertyValue?.Value;
            await Task.Delay(interval.HasValue ? interval.Value * 1000 : 1000, stoppingToken);
        }
    }

    private async Task<PropertyAck<int>> Property_interval_UpdateHandler(PropertyAck<int> p)
    {
        ArgumentNullException.ThrowIfNull(client);
        
        var ack = new PropertyAck<int>(p.Name);

        if (p.Value > 0)
        {
            ack.Description = "desired notification accepted";
            ack.Status = 200;
            ack.Version = p.Version;
            ack.Value = p.Value;
            ack.LastReported = p.Value;
        }
        else
        {
            ack.Description = "negative values not accepted";
            ack.Status = 405;
            ack.Version = p.Version;
            ack.Value = client.Property_interval.PropertyValue.LastReported > 0 ?
                            client.Property_interval.PropertyValue.LastReported :
                            default_interval;
        };
        client.Property_interval.PropertyValue = ack;
        return await Task.FromResult(ack);
    }

    string oldColor = "white";
    private async Task<Cmd_ChangeLCDColor_Response> Cmd_ChangeLCDColor_Handler(Cmd_ChangeLCDColor_Request req)
    {
        var color = Color.FromName(req.request);
        using SenseHat sh = new SenseHat();
        sh.Fill(color);
        var result = new Cmd_ChangeLCDColor_Response()
        {
            response = oldColor
        };
        oldColor = req.request;
        return await Task.FromResult(result);
    }
}
