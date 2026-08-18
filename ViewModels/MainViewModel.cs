using CommunityToolkit.Mvvm.ComponentModel;

namespace Maqloub.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    public partial string Greeting { get; set; } = "Sa!";
}
