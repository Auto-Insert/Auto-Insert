using AutoInsert.Core.Services;
using AutoInsert.Shared.Models;

namespace AutoInsert.Core.Controllers;

public class PlugController
{
    public static async Task<List<ThreadHole>> GetProgramFromFile(string filePath)
    {
        return await ThreadHoleReadingService.ReadThreadHolesFromFile(filePath);
    }
}