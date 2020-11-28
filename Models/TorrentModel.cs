using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using MonoTorrent.Client;
using Qurrent.Annotations;
using Qurrent.Utils;

namespace Qurrent.Models
{
    class TorrentModel: INotifyPropertyChanged
    {
        TorrentManager manager;
        MainModel parent;

        public string Name => manager.Torrent.Name;
        public TorrentState State => manager.State;

        public double Progress => manager.Progress / 100;

        public bool IsPaused =>
            State == TorrentState.Paused ||
            State == TorrentState.HashingPaused;

        public Command StartCommand { get; }
        public Command PauseCommand { get; }
        public Command RemoveCommand { get; }

        public TorrentModel(TorrentManager manager, MainModel parent)
        {
            this.parent = parent;
            this.manager = manager;
            this.manager.TorrentStateChanged += (sender, args) =>
            {
                OnPropertyChanged(nameof(State));
                OnPropertyChanged(nameof(IsPaused));
            };

            // commands
            StartCommand = new Command(async () => await Start());
            PauseCommand = new Command(async () => await Pause());
            RemoveCommand = new Command(async () => await Remove());
        }

        public Task Start() => manager.StartAsync();

        public Task Pause() => manager.PauseAsync();

        async Task Remove()
        {
            await manager.StopAsync();
            await manager.Engine.Unregister(manager);
            parent.RemoveTorrentFromList(this);
        }

        public void RefreshProgress()
        {
            OnPropertyChanged(nameof(Progress));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}