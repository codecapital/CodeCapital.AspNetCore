using System;
using System.Collections.Generic;

namespace CodeCapital.Services.ExchangeRate
{
    public class RateResponse
    {
        public Dictionary<string, decimal> Rates { get; set; } = new Dictionary<string, decimal>();
        public string Base { get; set; } = default!;
        public DateTime Date { get; set; }

        public decimal GetRateByCurrency(string code)
            => Rates.TryGetValue(code, out var rate) ? rate : 1;
    }
}
