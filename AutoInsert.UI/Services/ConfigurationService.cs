using AutoInsert.Core.Controllers;
using AutoInsert.Shared.Models;

namespace AutoInsert.UI.Services;

public class ConfigurationService
{
    public static async Task<List<ThreadHole>> LoadProgramAsync(string filePath)
    {
        return await PlugController.GetProgramFromFile(filePath);
    }
}