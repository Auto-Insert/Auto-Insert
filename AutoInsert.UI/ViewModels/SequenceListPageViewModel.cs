using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Controls;
using AutoInsert.Core.Controllers;
using AutoInsert.Shared.Models;

namespace AutoInsert.UI.ViewModels;

public class SequenceListPageViewModel
{
    private readonly SequenceController _sequenceController;
    public ObservableCollection<Sequence> Sequences { get; } = new();

    public SequenceListPageViewModel()
    {
        _sequenceController = new SequenceController();
    }

    public async Task LoadSequencesAsync()
    {
        await _sequenceController.InitializeAsync();
        var all = await _sequenceController.LoadAllSequencesAsync();
        Sequences.Clear();
        foreach (var seq in all)
            Sequences.Add(seq);
    }
}
