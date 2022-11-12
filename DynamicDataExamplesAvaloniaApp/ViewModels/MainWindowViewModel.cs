using DynamicData.Binding;
using DynamicData;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System;
using System.Reactive.Linq;

namespace DynamicDataExamplesAvaloniaApp.ViewModels
{
    public class MainWindowViewModel : ViewModelBase, IDisposable
    {
        public string Greeting => "Welcome to Avalonia!";

        private readonly IDisposable _cleanUp;
        private readonly ReadOnlyObservableCollection<SimpleItemViewModel> _selected;
        private readonly ReadOnlyObservableCollection<SimpleItemViewModel> _notSelected;
        public ReadOnlyObservableCollection<SimpleItemViewModel> Selected => _selected;
        public ReadOnlyObservableCollection<SimpleItemViewModel> NotSelected => _notSelected;

        public MainWindowViewModel()
        {
            // SourceList синхронная реактивная колекция 
            var sourceList = new SourceList<SimpleItem>();

            // добавить диапазон значений
            sourceList.AddRange(
                // создать диапазон значений 
                Enumerable.Range(1, 10)
                // преобразовать в элемент модели
                .Select(i => new SimpleItem(i)));

            //create a shared list of view models / создать общий список моделей представления
            var viewModels = sourceList
                .Connect()
                // аналог Select (map) для DynamicData , преобразовать наблюдаемое значение
                .Transform(simpleItem => new SimpleItemViewModel(simpleItem))
                // публикация , аналог onNext
                .Publish();

            //filter on items which are selected and populate into an observable collection
            //фильтровать элементы, которые выбраны и заполняются наблюдаемой коллекцией

            var selectedLoader = viewModels
                .FilterOnProperty(vm => vm.IsSelected, vm => vm.IsSelected)
                .Sort(SortExpressionComparer<SimpleItemViewModel>.Ascending(vm => vm.Number))
                .Bind(out _selected)
                .Subscribe();

            //filter on items which are not selected and populate into an observable collection
            //фильтровать элементы, которые не выбраны, и заполнять наблюдаемую коллекцию
            var notSelectedLoader = viewModels
                .FilterOnProperty(vm => vm.IsSelected, vm => !vm.IsSelected)
                .Sort(SortExpressionComparer<SimpleItemViewModel>.Ascending(vm => vm.Number))
                .Bind(out _notSelected)
                .Subscribe();

            _cleanUp = new CompositeDisposable(sourceList, selectedLoader, notSelectedLoader, viewModels.Connect());
        }

        public void Dispose()
        {
            _cleanUp.Dispose();
        }
    }

    /// связующее между моделью 
    public class SimpleItemViewModel : AbstractNotifyPropertyChanged
    {
        private bool _isSelected;
        public SimpleItem Item { get; }

        public int Number => Item.Id;

        public SimpleItemViewModel(SimpleItem item)
        {
            Item = item;
        }

        public bool IsSelected
        {
            get { return _isSelected; }
            set { SetAndRaise(ref _isSelected, value); }
        }
    }

    // модель
    public class SimpleItem
    {
        public int Id { get; }

        public SimpleItem(int id)
        {
            Id = id;
        }
    }
}
