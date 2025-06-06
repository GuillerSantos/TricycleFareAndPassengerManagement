﻿using TricycleFareAndPassengerManagement.Application.Common.DTOs;
using TricycleFareAndPassengerManagement.Application.Features.Driver.Commands;
using TricycleFareAndPassengerManagement.Client.Services.Interfaces;

namespace TricycleFareAndPassengerManagement.Client.Services
{
    public class DriverClientService(HttpClient _httpClient) : IDriverClientService
    {
        #region Public Methods

        public async Task<List<DriverDto>> GetAllDriversAsync()
        {
            var response = await _httpClient.GetFromJsonAsync<List<DriverDto>>("api/Driver/getall");
            return response ?? new List<DriverDto>();
        }

        public async Task<DriverDto?> GetDriverByIdAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<DriverDto>($"api/Driver/getbyid/{id}");
        }

        public async Task<int> CreateDriverAsync(CreateDriverCommand command)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Driver/create", command);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<int>();
        }

        public async Task<bool> UpdateDriverAsync(int id, UpdateDriverCommand command)
        {
            command.Id = id;
            var response = await _httpClient.PutAsJsonAsync($"api/Driver/update/{id}", command);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<bool>();
        }

        public async Task<bool> DeleteDriverAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/Driver/delete/{id}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<bool>();
        }

        #endregion Public Methods
    }
}