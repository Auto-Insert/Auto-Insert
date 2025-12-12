namespace AutoInsert.Core.Services.Communication;
public interface IURClient
{
    Task<bool> ConnectAsync();
    void Disconnect();
}