﻿using System.Windows.Input;
using Slithin.Controls;
using Slithin.Core;
using Slithin.Core.Commands;
using Slithin.Core.Remarkable;
using Slithin.Core.Services;
using Slithin.Core.Sync;
using Slithin.UI.Modals;
using Slithin.ViewModels.Modals;

namespace Slithin.ViewModels.Pages
{
    public class NotebooksPageViewModel : BaseViewModel
    {
        private readonly ILoadingService _loadingService;
        private readonly IMailboxService _mailboxService;
        private bool _isMoving;
        private Metadata _movingNotebook;
        private Metadata _selectedNotebook;

        public NotebooksPageViewModel(ILoadingService loadingService, IMailboxService mailboxService)
        {
            MakeFolderCommand = DialogService.CreateOpenCommand<MakeFolderModal>(
                ServiceLocator.Container.Resolve<MakeFolderModalViewModel>());

            RenameCommand = new DelegateCommand(_ =>
            {
                DialogService.Open(new RenameModal(), new RenameModalViewModel((Metadata)_));
            }, _ => _ != null && _ is Metadata md && md.VisibleName != "Quick sheets" && md.VisibleName != "Up ..");

            RemoveNotebookCommand = ServiceLocator.Container.Resolve<RemoveNotebookCommand>();
            MoveCommand = new DelegateCommand(_ =>
            {
                IsMoving = true;
                _movingNotebook = SelectedNotebook;
            }, (_) => _ != null && _ is Metadata md && SelectedNotebook != null && !IsMoving && md.VisibleName != "Quick sheets" && md.VisibleName != "Up ..");

            MoveCancelCommand = new DelegateCommand(_ =>
            {
                IsMoving = false;
            });

            MoveHereCommand = new DelegateCommand(_ =>
            {
                MetadataStorage.Local.Move(_movingNotebook, SyncService.NotebooksFilter.Folder);
                IsMoving = false;

                var item = new SyncItem
                {
                    Direction = SyncDirection.ToDevice,
                    Data = MetadataStorage.Local.Get(_movingNotebook.ID),
                    Type = SyncType.Notebook,
                    Action = SyncAction.Update
                };

                SyncService.SyncQueue.Insert(item);

                SyncService.NotebooksFilter.Documents.Clear();
                foreach (var md in MetadataStorage.Local.GetByParent(SyncService.NotebooksFilter.Folder))
                {
                    SyncService.NotebooksFilter.Documents.Add(md);
                }

                SyncService.NotebooksFilter.Documents.Add(new Metadata { Type = "CollectionType", VisibleName = "Up .." });

                SyncService.NotebooksFilter.SortByFolder();
            });

            _loadingService = loadingService;
            _mailboxService = mailboxService;
        }

        public bool IsMoving
        {
            get { return _isMoving; }
            set { SetValue(ref _isMoving, value); }
        }

        public ICommand MakeFolderCommand { get; set; }
        public ICommand MoveCancelCommand { get; set; }
        public ICommand MoveCommand { get; set; }
        public ICommand MoveHereCommand { get; set; }
        public ICommand RemoveNotebookCommand { get; set; }
        public ICommand RenameCommand { get; set; }

        public Metadata SelectedNotebook
        {
            get { return _selectedNotebook; }
            set { SetValue(ref _selectedNotebook, value); }
        }

        public override void OnLoad()
        {
            base.OnLoad();

            _mailboxService.PostAction(() =>
            {
                NotificationService.Show("Loading Notebooks");

                _loadingService.LoadNotebooks();

                NotificationService.Hide();
            });
        }
    }
}
