using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using MonoTorrent;
using MonoTorrent.Client;
using Qurrent.Annotations;

namespace Qurrent.Models
{
    class MainModel: INotifyPropertyChanged
    {
        const string metadataFolder = "./Data";
        const string downloadsFolder = "./Downloads";

        ClientEngine engine;
        Timer timer;
        TorrentModel selectedTorrent;

        public ObservableCollection<TorrentModel> Torrents { get; } = new ObservableCollection<TorrentModel>();

        public TorrentModel SelectedTorrent
        {
            get => selectedTorrent;
            set
            {
                if (Equals(value, selectedTorrent)) return;
                selectedTorrent = value;
                OnPropertyChanged();
            }
        }

        public MainModel()
        {
            var settings = new EngineSettings
            {
                AllowedEncryption = EncryptionTypes.All,
                PreferEncryption = true,
                MaximumUploadSpeed = 200 * 1024,
                SavePath = metadataFolder
            };

            engine = new ClientEngine(settings);
            timer = new Timer(
                RefreshProgress,
                null,
                TimeSpan.FromMilliseconds(50),
                TimeSpan.FromMilliseconds(50));
        }

        public async Task AddTorrent(string path)
        {
            var torrent = await Torrent.LoadAsync(path);
            if (engine.Contains(torrent)) return;

            var manager = new TorrentManager(torrent, downloadsFolder, new TorrentSettings());
            await engine.Register(manager);

            var model = new TorrentModel(manager, this);
            Torrents.Add(model);
            await model.Start();
        }

        public void RemoveTorrentFromList(TorrentModel model)
        {
            Torrents.Remove(model);
        }

        void RefreshProgress(object state)
        {
            foreach (var manager in Torrents)
                manager.RefreshProgress();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
