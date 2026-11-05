using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DynamicData;
using DynamicData.Binding;
using KGallery.Abstractions;
using KGallery.Services;
using KGallery.ViewModels;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace KGallery.Controls
{
    /// <summary>
    /// Interaction logic for ApplyFiltersDialog.xaml
    /// </summary>
    [ObservableObject]
    public partial class ApplyFiltersDialog : UserControl, IOpenDialog<NoValue>
    {
        private readonly GlobalInput _input;
        private readonly CategoriesService _categoriesService;

        public ReadOnlyObservableCollection<FilteringCategoryViewModel> Categories => _categories;
        private ReadOnlyObservableCollection<FilteringCategoryViewModel> _categories;
        private readonly IObservableCache<FilteringCategoryViewModel, int> _unfilteredCategories;

        private readonly Subject<Unit> _filterUpdate = new Subject<Unit>();
        private readonly Subject<Unit> _categoriesSubject = new Subject<Unit>();
        private int[] _availableCategories = [];

        public ApplyFiltersDialog(GlobalInput input, CategoriesService categoriesService, ImagesService imagesService)
        {
            _categoriesService = categoriesService;
            _input = input;

            _unfilteredCategories = _categoriesService.Categories.Connect().Transform(f => new FilteringCategoryViewModel(f)).AsObservableCache();
            _unfilteredCategories.Connect().Filter(GetFilter()).ObserveOn(SynchronizationContext.Current).SortAndBind(out _categories, SortExpressionComparer<FilteringCategoryViewModel>.Descending(f => f.ImagesCount)).Subscribe();

            IObservable<Func<ImageViewModel, bool>> filter = this.WhenPropertyChanged(f => f.IgnoreCategoryId).Select(_ => (Func<ImageViewModel, bool>)(f => _ignoreCategoryId == -1 || f.Categories.Contains(_ignoreCategoryId)));
            imagesService.Images.Connect().Filter(filter).ToCollection().Select(f => f.SelectMany(x => x.Categories)
            .Where(x => x != _ignoreCategoryId).Distinct().Order().ToArray()).Subscribe(ids =>
            {
                _availableCategories = ids;
                _categoriesSubject.OnNext(default);
            });

            InitializeComponent();
            Visibility = Visibility.Collapsed;

            this.WhenPropertyChanged(f => f.IgnoreCategoryId).Subscribe(_ =>
            {
                Exclude = null;
                Include = null;
                Intersect = null;
                _filterUpdate.OnNext(default);
                Task.Run(ResetFilters);
            });
        }

        public ISubject<Unit> GetNotification()
        {
            return _filterUpdate;
        }

        private IObservable<Func<FilteringCategoryViewModel, bool>> GetFilter()
        {
            return Observable.Merge(this.WhenPropertyChanged(x => x.SearchText).Select(_ => Unit.Default),
            this.WhenPropertyChanged(x => x.IgnoreCategoryId).Select(_ => Unit.Default), _categoriesSubject).Select(_ =>
            {
                return (Func<FilteringCategoryViewModel, bool>)
                (f => (string.IsNullOrWhiteSpace(SearchText) || f.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                && _availableCategories.Contains(f.Id));
            });
        }

        public void Open(NoValue parameter)
        {
            Visibility = Visibility.Visible;
            _input.Subscribe(OnDialogKeyDown);
        }

        [ObservableProperty]
        private string _searchText;

        [ObservableProperty]
        private IEnumerable<int> _include;

        [ObservableProperty]
        private IEnumerable<int> _exclude;

        [ObservableProperty]
        private IEnumerable<int> _intersect;

        [ObservableProperty]
        private bool _filterUnsorted;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanApplyUnsortedFilter))]
        [NotifyCanExecuteChangedFor(nameof(SubmitUnsortedCommand))]
        private int _ignoreCategoryId = -1;

        public bool CanApplyUnsortedFilter => IgnoreCategoryId == -1;

        [RelayCommand]
        private void Close()
        {
            Visibility = Visibility.Collapsed;
            _input.Unsubscribe(OnDialogKeyDown);
        }

        [RelayCommand(CanExecute = nameof(CanApplyUnsortedFilter))]
        private void SubmitUnsorted()
        {
            FilterUnsorted = true;
            _filterUpdate.OnNext(default);
            Close();
        }

        [RelayCommand]
        private async Task Submit()
        {
            FilterUnsorted = false;
            Include = _unfilteredCategories.Items.Where(f => f.IsInclude && f.Id != IgnoreCategoryId).Select(f => f.Id);
            Exclude = _unfilteredCategories.Items.Where(f => f.IsExclude && f.Id != IgnoreCategoryId).Select(f => f.Id);
            Intersect = _unfilteredCategories.Items.Where(f => f.IsIntersect && f.Id != IgnoreCategoryId).Select(f => f.Id);
            _filterUpdate.OnNext(default);
            Close();
        }

        [RelayCommand]
        private async Task ResetFilters()
        {
            foreach (FilteringCategoryViewModel category in _unfilteredCategories.Items)
            {
                category.IsNone = true;
                category.IsExclude = false;
                category.IsInclude = false;
                category.IsIntersect = false;
            }
        }

        private async void OnDialogKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                await Submit();
            }
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }
    }
}
