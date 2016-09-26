﻿using Flights.Infrastructure;
using Flights.Models;
using Flights.Services.DataModels;
using System.Threading.Tasks;

namespace Flights.Services
{
    public class FlightsService: IFlightsService
    {
        readonly IHttpService _httpService;
        readonly IJsonConverter _jsonConverter;

        public FlightsService(IHttpService httpService, IJsonConverter jsonConverter)
        {
            _httpService = httpService;
            _jsonConverter = jsonConverter; 
        }

        public async Task<FlyInfoModel> GetFlightAsync(string from, string to, string date)
        {
            string uri = "https://api.rasp.yandex.net/v1.0/search/?apikey=e07ef310-dbe4-49cf-985f-1d5738c1ebc7&format=json&transport_types=plane&system=iata&from=" + from + "&to=" + to + "&lang=en&page=1&date=" + date;
            string response = await _httpService.GetRequest(uri);
            if (response != null)
            {
                FlightInfoDataModel flightInfoModel = _jsonConverter.Deserialize<FlightInfoDataModel>(response);
                FlyInfoModel flyInfoModel = new FlyInfoModel();
                int i = 0;
                flyInfoModel.Arrival = new string[flightInfoModel.Threads.Count];
                flyInfoModel.Duration = new string[flightInfoModel.Threads.Count];
                flyInfoModel.ArrivalTerminal = new string[flightInfoModel.Threads.Count];
                flyInfoModel.From = new string[flightInfoModel.Threads.Count];
                flyInfoModel.ThreadCarrierTitle = new string[flightInfoModel.Threads.Count];
                flyInfoModel.ThreadVehicle = new string[flightInfoModel.Threads.Count];
                flyInfoModel.ThreadNumber = new string[flightInfoModel.Threads.Count];
                flyInfoModel.Departure = new string[flightInfoModel.Threads.Count];
                flyInfoModel.To = new string[flightInfoModel.Threads.Count];
                foreach (var item in flightInfoModel.Threads)
                {
                    flyInfoModel.Arrival[i] = item.Arrival;
                    flyInfoModel.Duration[i] = item.Duration;
                    flyInfoModel.ArrivalTerminal[i] = item.ArrivalTerminal;
                    flyInfoModel.From[i] = item.From.Title;
                    flyInfoModel.ThreadCarrierTitle[i] = item.Thread.Carrier.Title;
                    flyInfoModel.ThreadVehicle[i] = item.Thread.Vehicle;
                    flyInfoModel.ThreadNumber[i] = item.Thread.Number;
                    flyInfoModel.Departure[i] = item.Departure;
                    flyInfoModel.To[i] = item.To.Title;
                    flyInfoModel.Count = i + 1;
                    i++;
                }
                return flyInfoModel;
            }
            return null;
        }

        public async Task<FlyInfoModel[]> ConfigurationOfFlights(MainPageModel mainPageModel, string date, bool returnWay)
        {
            int value = -1;
            int count = mainPageModel.IataFrom.Length * mainPageModel.IataTo.Length;
            FlyInfoModel[] flyInfoModel = new FlyInfoModel[count];
            string[] from;
            string[] to;
            if (returnWay != true)
            {
                from = mainPageModel.IataFrom;
                to = mainPageModel.IataTo;
            }
            else
            {
                from = mainPageModel.IataTo;
                to = mainPageModel.IataFrom;
            }
            for (int i = 0; i < from.Length; i++)
            {
                for (int j = 0; j < to.Length; j++)
                {
                    value++;
                    flyInfoModel[value] = await GetFlightAsync(from[i], to[j], date);
                    if (flyInfoModel[value] == null || flyInfoModel[value].Count == 0)
                    {
                        value--;
                    }
                }
            }
            return flyInfoModel;
        }
    }
}
