using System.Collections.ObjectModel;

using DirectoryScanner.Core;

namespace DirectoryScanner.View;

public class DirectoryTreeItemViewModel : ViewModelBase
{
    private long _size;

    private double _percentage;

    public DirectoryTreeItemViewModel(Node model)
    {
        Model = model;
        _size = model.Size;
        _percentage = model.Percentage;
    }

    public Node Model { get; }

    public ObservableCollection<DirectoryTreeItemViewModel> Children { get; } = new();

    public string Name => Model.Name;

    public bool IsDirectory => Model.IsDirectory;

    public long Size
    {
        get => _size;
        set
        {
            if (_size == value)
            {
                return;
            }

            _size = value;
            OnPropertyChanged(nameof(Size));
        }
    }

    public double Percentage
    {
        get => _percentage;
        set
        {
            if (Math.Abs(_percentage - value) < double.Epsilon)
            {
                return;
            }

            _percentage = value;
            OnPropertyChanged(nameof(Percentage));
        }
    }
}
