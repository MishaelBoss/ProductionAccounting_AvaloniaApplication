using ProductionAccounting_AvaloniaApplication.Services.Interfaces;
using System;

namespace ProductionAccounting_AvaloniaApplication.Models;

internal class CookieServer(double id, string username, string token, DateTime expires) : ICookie
{
    public double Id { get; set; } = id;
    public string Username { get; set; } = username;
    public string Token { get; set; } = token;
    public DateTime Expires { get; set; } = expires;
}