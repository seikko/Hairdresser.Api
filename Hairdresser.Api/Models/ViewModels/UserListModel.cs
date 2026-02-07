namespace Hairdresser.Api.Models.ViewModels;

public class UserListModel
{
    public User User { get; set; }
    public int ConfirmCount { get; set; }
    public int CancelCount { get; set; }
}