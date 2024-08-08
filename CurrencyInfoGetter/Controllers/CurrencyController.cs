using Microsoft.AspNetCore.Mvc;
using CurrencyInfoGetter.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using ISO._4217;

#pragma warning disable CS1591 // ����������� ����������� XML ��� ��������� �������� ���� ��� �����

namespace CurrencyInfoGetter.Controllers
{
	[ApiController]
	[Route("api/[controller]/[action]")]
	public class CurrencyController : ControllerBase
	{
		private readonly ILogger<CurrencyController> _logger;
		private const string _requestUrl = "https://api.nbrb.by/exrates/rates";
		private List<Rate> _currencies = [];
		private readonly ApplicationContext _db;
		public CurrencyController(ILogger<CurrencyController> logger, ApplicationContext context)
		{
			_logger = logger;
			_db = context;
		}
		/// <summary>
		/// ��������� ���������� � ����� ����� �� ������������ ���� � ���� ������
		/// </summary>
		/// <param name="date">������ ����: ��.��.����. � ������ ���������� ����� �������� ���������� �� ������� ����</param>
		/// <returns></returns>
		[HttpGet]
		public async Task<ObjectResult> GetCurrencyRate(string date = "")
		{
			string stringDate;
			DateTime convertedDate;
			try
			{
				convertedDate = convertDate(date);
				stringDate = $"{convertedDate.Year}-{convertedDate.Month}-{convertedDate.Day}";
			}
			catch (FormatException)
			{
				return StatusCode(StatusCodes.Status400BadRequest, "�������� ������ ����");
			}

			var client = new HttpClient();
			var dailyCurrency = await client.GetFromJsonAsync<Rate[]>($"{_requestUrl + "?ondate=" + stringDate}&periodicity=0&parammode=0");
			var monthlyCurrency = await client.GetFromJsonAsync<Rate[]>($"{_requestUrl + "?ondate=" + stringDate}&periodicity=1&parammode=0");
			if ((dailyCurrency is null || monthlyCurrency is null) || dailyCurrency.Length + monthlyCurrency.Length is 0)
				return StatusCode(StatusCodes.Status404NotFound, "���� ����� �� ������� ���� ����������");

			_currencies.AddRange(dailyCurrency);
			_currencies.AddRange(monthlyCurrency);

			_currencies = _currencies.Select(el =>
			{
				el.Date = DateTime.SpecifyKind(el.Date, DateTimeKind.Utc);
				return el;
			}).ToList();
			try
			{
				_db.UpdateRange(_currencies);
				_db.SaveChanges();
			}
			catch (DbUpdateConcurrencyException)
			{
				_db.Database.ExecuteSqlRaw("TRUNCATE TABLE \"Currencies\"");
				_db.AddRange(_currencies);
				_db.SaveChanges();
			}
			return StatusCode(StatusCodes.Status200OK, $"���� ����� �� {DateOnly.FromDateTime(convertedDate)} ������� ��������");
		}
		/// <summary>
		/// �������� ���������� � ����� ����� �� ������������ ���� �� ���� ������
		/// </summary>
		/// <param name="currencyCode">��� ������ �������� ���-4217</param>
		/// <param name="date">������ ����: ��.��.����. � ������ ���������� ����� �������� ���������� �� ������� ����</param>
		/// <returns></returns>
		[HttpGet]
		public ObjectResult ReturnCurrencyInfo([Required] int currencyCode, string date = "")
		{
			var convertedDate = convertDate(date);
			var cur = _db.Currencies.Where(element => element.Date == convertedDate 
			&& element.Cur_Abbreviation == CurrencyCodesResolver.GetCodeByNumber(currencyCode)).ToList();
			if (cur.Count > 0)
				return StatusCode(StatusCodes.Status200OK, cur[0]);
			return StatusCode(StatusCodes.Status404NotFound, $"������ � ����� {currencyCode} ����������� �� {convertedDate}");
		}

		DateTime convertDate(string date)
		{
			if (date == "")
				return DateTime.SpecifyKind(DateTime.Today, DateTimeKind.Utc);
			else
				return DateTime.SpecifyKind(DateTime.ParseExact(date, "dd.MM.yyyy", null), DateTimeKind.Utc);
		}
	}
}
