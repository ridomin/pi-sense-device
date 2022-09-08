﻿//  <auto-generated/> 
using Rido.MqttCore;
using Rido.MqttCore.PnP;
using Rido.Mqtt.Client;
using Rido.Mqtt.Client.TopicBindings;

namespace dtmi_rido_pnp_sensehat.mqtt;

public class sensehat : PnPClient, Isensehat
{
    public IReadOnlyProperty<string> Property_piri { get; set; }

    public IReadOnlyProperty<string> Property_ipaddr { get; set; }
    public IWritableProperty<int> Property_interval { get; set; }
    public ITelemetry<double> Telemetry_t1 { get; set; }
    public ITelemetry<double> Telemetry_t2 { get; set; }
    public ITelemetry<double> Telemetry_h { get; set; }
    public ITelemetry<double> Telemetry_p { get; set; }
    public ITelemetry<double> Telemetry_m { get; set; }

    public ICommand<Cmd_ChangeLCDColor_Request, Cmd_ChangeLCDColor_Response> Command_ChangeLCDColor { get; set; }

    public string InitialState => String.Empty;

    internal sensehat(IMqttBaseClient c) : base(c)
    {
        Property_piri = new ReadOnlyProperty<string>(c, "piri");
        Property_ipaddr = new ReadOnlyProperty<string>(c, "ipaddr");
        Property_interval = new WritableProperty<int>(c, "interval");
        Telemetry_t1 = new Telemetry<double>(c, "t1");
        Telemetry_t2 = new Telemetry<double>(c, "t2");
        Telemetry_h = new Telemetry<double>(c, "h");
        Telemetry_p = new Telemetry<double>(c, "p");
        Command_ChangeLCDColor = new Command<Cmd_ChangeLCDColor_Request, Cmd_ChangeLCDColor_Response>(c, "ChangeLCDColor");
    }
}