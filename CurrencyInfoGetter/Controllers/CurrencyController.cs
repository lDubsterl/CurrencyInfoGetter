using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Reflection.Metadata.Ecma335;

namespace CurrencyInfoGetter.Controllers
{
	[ApiController]
	[Route("api/[controller]/[action]")]
	public class CurrencyController : ControllerBase
	{
		private readonly ILogger<CurrencyController> _logger;
		private const string _requestUrl = "https://api.nbrb.by/exrates/rates";
		private static Rate[] _currencies = [];
		public CurrencyController(ILogger<CurrencyController> logger)
		{
			_logger = logger;
		}

		[HttpGet]
		public async Task<StatusCodeResult> GetCurrencyRate(DateTime date)
		{
			var client = new HttpClient();
			var response = await client.GetFromJsonAsync<Rate[]>($"{_requestUrl + "?ondate=" + date.ToString()}&periodicity=0&parammode=2");
			if (response == null)
				return StatusCode(StatusCodes.Status404NotFound);
			_currencies = response;
			return StatusCode(StatusCodes.Status200OK);
		}

		[HttpGet]
		public Rate? ReturnCurrencyInfo(DateTime date, string currencyAbbreviation)
		{
			foreach (var currency in _currencies)
			{
				if (currency.Cur_Abbreviation == currencyAbbreviation)
					return currency;
			}
			return null;
		}
	}
}
