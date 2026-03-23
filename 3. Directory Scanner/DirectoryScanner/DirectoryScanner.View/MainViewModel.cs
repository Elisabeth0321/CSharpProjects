using System.Collections.Concurrent;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Threading;

using DirectoryScanner.Core;

namespace DirectoryScanner.View;

public class MainViewModel : ViewModelBase
{
    private readonly RelayCommand _scanCommandImplementation;

    private readonly RelayCommand _cancelCommandImplementation;

    private CancellationTokenSource? _cts;

    private ConcurrentDictionary<Node, DirectoryTreeItemViewModel> _nodeMap = new();

    private int _scanGeneration;

    private DirectoryTreeItemViewModel? _rootVm;

    private bool _isScanning;

    public MainViewModel()
    {
        _scanCommandImplementation = new RelayCommand(StartScan, () => !IsScanning);
        _cancelCommandImplementation = new RelayCommand(CancelScan, () => IsScanning);
        ScanCommand = _scanCommandImplementation;
        CancelCommand = _cancelCommandImplementation;
    }

    public DirectoryTreeItemViewModel? RootVm
    {
        get => _rootVm;
        set
        {
            _rootVm = value;
            OnPropertyChanged(nameof(RootVm));
            OnPropertyChanged(nameof(RootNodes));
        }
    }

    public IEnumerable<DirectoryTreeItemViewModel> RootNodes =>
        _rootVm is null ? Array.Empty<DirectoryTreeItemViewModel>() : new[] { _rootVm };

    public bool IsScanning
    {
        get => _isScanning;
        set
        {
            if (_isScanning == value)
            {
                return;
            }

            _isScanning = value;
            OnPropertyChanged(nameof(IsScanning));
            _scanCommandImplementation.RaiseCanExecuteChanged();
            _cancelCommandImplementation.RaiseCanExecuteChanged();
        }
    }

    public ICommand ScanCommand { get; }

    public ICommand CancelCommand { get; }

    private async void StartScan()
    {
        var dialog = new FolderBrowserDialog();

        if (dialog.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        _cts?.Cancel();
        _cts?.Dispose();

        _cts = new CancellationTokenSource();

        int generation = Interlocked.Increment(ref _scanGeneration);
        _nodeMap = new ConcurrentDictionary<Node, DirectoryTreeItemViewModel>();
        IsScanning = true;

        var scanner = new Scanner(8);

        try
        {
            await scanner.ScanAsync(
                dialog.SelectedPath,
                _cts.Token,
                onScanStructureStarted: root =>
                {
                    var rootVm = new DirectoryTreeItemViewModel(root);
                    _nodeMap[root] = rootVm;
                    RootVm = rootVm;
                },
                onChildAttached: (parent, child) =>
                {
                    Dispatcher? dispatcher = System.Windows.Application.Current?.Dispatcher;
                    if (dispatcher is null)
                    {
                        return;
                    }

                    _ = dispatcher.InvokeAsync(
                        () =>
                        {
                            if (generation != Volatile.Read(ref _scanGeneration))
                            {
                                return;
                            }

                            if (!_nodeMap.TryGetValue(parent, out DirectoryTreeItemViewModel? parentVm))
                            {
                                return;
                            }

                            var childVm = new DirectoryTreeItemViewModel(child);
                            _nodeMap[child] = childVm;
                            parentVm.Children.Add(childVm);
                        },
                        DispatcherPriority.Background);
                },
                onMetricsRecalculated: root =>
                {
                    Dispatcher? dispatcher = System.Windows.Application.Current?.Dispatcher;
                    if (dispatcher is null)
                    {
                        return;
                    }

                    _ = dispatcher.InvokeAsync(
                        () =>
                        {
                            if (generation != Volatile.Read(ref _scanGeneration))
                            {
                                return;
                            }

                            RefreshMetricsFromModel(root);
                        },
                        DispatcherPriority.Background);
                });
        }
        finally
        {
            IsScanning = false;
        }
    }

    private void CancelScan()
    {
        _cts?.Cancel();
    }

    private void RefreshMetricsFromModel(Node node)
    {
        if (_nodeMap.TryGetValue(node, out DirectoryTreeItemViewModel? vm))
        {
            vm.Size = node.Size;
            vm.Percentage = node.Percentage;
        }

        foreach (Node child in node.Children)
        {
            RefreshMetricsFromModel(child);
        }
    }
}
