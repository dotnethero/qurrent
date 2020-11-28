using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using MonoTorrent;
using MonoTorrent.Client;

namespace Qurrent.Models
{
    class MainModel
    {
        const string metadataFolder = "./Data";
        const string downloadsFolder = "./Downloads";

        ClientEngine engine;
        Timer timer;

        public ObservableCollection<TorrentModel> Torrents { get; } = new ObservableCollection<TorrentModel>();

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
    }
}
