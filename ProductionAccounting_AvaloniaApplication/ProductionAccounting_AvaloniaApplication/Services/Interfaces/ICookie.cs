using Newtonsoft.Json;
using System;

namespace ProductionAccounting_AvaloniaApplication.Services.Interfaces;

public interface ICookie
{
    [JsonProperty("Id")]
    double Id { get; set; }

    [JsonProperty("Username")]
    string Username { get; set; }

    [JsonProperty("Token")]
    string Token { get; set; }

    [JsonProperty("Expires")]
    public DateTime Expires { get; set; }
}